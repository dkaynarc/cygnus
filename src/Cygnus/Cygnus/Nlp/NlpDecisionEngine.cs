using Cygnus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using edu.stanford.nlp.pipeline;
using java.util;
using java.io;
using System.IO;
using System.Diagnostics;
using edu.stanford.nlp.ling;
using System.Threading;
using System.Threading.Tasks;
using edu.stanford.nlp.util;
using edu.stanford.nlp.semgraph;
using edu.stanford.nlp.trees;
using NHunspell;
using Cygnus.Models.Api;
using Cygnus.Managers;
using Cygnus.Common;

namespace Cygnus.Nlp
{
    public class NlpDecisionEngine
    {
        private static NlpDecisionEngine m_instance;
        
        public static NlpDecisionEngine Instance
        {
            get
            {
                if (m_instance == null) m_instance = new NlpDecisionEngine();
                return m_instance;
            }
        }
        private NlpEngineThread m_engineThread = new NlpEngineThread();
        private static readonly Dictionary<string, bool> BooleanTextMap;

        private NlpDecisionEngine()
        {
        }

        static NlpDecisionEngine()
        {
            BooleanTextMap = CreateBooleanTextMap();
        }

        public void Initialize()
        {
            m_engineThread.Start();
        }

        public IEnumerable<UserResponsePackage> ExecuteQuery(string query)
        {
            var allResponses = new List<UserResponsePackage>();
            try
            {
                var analysis = m_engineThread.AnalyseText(query);
                foreach (Annotation sentence in analysis.Sentences)
                {
                    var sentenceResponses = ExecuteSentenceRequest(sentence);
                    allResponses.AddRange(sentenceResponses);
                }
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Decision engine error.", e);
            }
            return allResponses;
        }
        private IEnumerable<UserResponsePackage> ExecuteSentenceRequest(Annotation sentence)
        {
            var responses = new List<UserResponsePackage>();
            var parseTree = sentence.get(typeof(TreeCoreAnnotations.TreeAnnotation)) as Tree;

            IEnumerable<string> subjectKeywords = null;
            ConditionalExpression condExpr = null;
            GroupExpression groupExpr = null;
            if (TryFindGroupExpression(parseTree, out groupExpr))
            {
                responses.AddRange(ResourceGroupManager.Instance.ExecuteExpressions(groupExpr.Yield()));
            }
            else if (TryFindConditionalExpression(parseTree, out condExpr))
            {
                // Process conditional
                var response = new UserResponsePackage() { Data = "Success." };
                try
                {
                    ConditionalRequestManager.Instance.AddExpression(condExpr);
                }
                catch (Exception e)
                {
                    response.Data = e;
                }
                responses.Add(response);
            }
            else if (TryFindSubjectKeywords(parseTree, out subjectKeywords))
            {
                Predicate predicate = null;
                if (TryFindPredicate(parseTree, out predicate))
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var se = new ResourceSearchEngine(context);
                        var resources = se.FindResources(subjectKeywords);
                        var groups = se.FindGroups(subjectKeywords);
                        predicate.ResetActionType();
                        responses.AddRange(ResourceGroupManager.Instance.ExecuteOnGroups(groups, predicate));
                        responses.AddRange(predicate.ExecuteAction(resources));
                    }
                }
            }
            return responses;
        }

        private bool TryFindSubjectKeywords(Tree parseTree, out IEnumerable<string> keywords)
        {
            bool found = false;
            var words = new List<string>();
            var conjunctions = new List<string>();
            Tree np = null;
            // Find the noun phrase part
            Traverse(parseTree, x =>
            {
                if (np == null && x.label().value().Equals("NP"))
                {
                    np = x;
                }
                if (x.label().value().Equals("DT") ||
                    x.label().value().Equals("IN") || 
                    x.label().value().Equals("CC") || 
                    x.label().value().StartsWith(","))
                {
                    conjunctions.AddRange(WordsListToStringList(x.yieldWords()));
                }
                return true;
            });

            if (np != null)
            {
                words = WordsListToStringList(np.yieldWords());

                // Conjunctions aren't really keywords so we'll filter them out
                words.RemoveAll(x => conjunctions.Contains(x));
                found = words.Count > 0;
            }

            keywords = words;
            return found;
        }

        private bool TryFindPredicate(Tree parseTree, out Predicate pred)
        {
            Tree verbPhrase = null;
            return TryFindPredicate(parseTree, out pred, out verbPhrase);
        }

        private bool TryFindPredicate(Tree parseTree, out Predicate pred, out Tree verbPhrase)
        {
            Tree vp = null;
            verbPhrase = vp;
            string numberStr = null;
            pred = null;
            string boolReln;

            if (parseTree == null) { return false; }
            // find the verb 
            Traverse(parseTree, x => 
                {
                    if (x.label().value().Equals("VP"))
                    {
                        vp = x;
                        return false;
                    }
                    return true;
                });

            if (vp == null) { return false; }
            string vbWord = null;
            bool found = false;
            Traverse(vp, x =>
            {
                if (x.label().value().StartsWith("VB"))
                {
                    var vbWordNode = x;
                    var possiblePP = vp.lastChild().firstChild();
                    if (x.label().value().Equals("VBZ") && 
                        possiblePP.label().value().Equals("IN"))
                    {
                        vbWordNode = possiblePP;
                    }
                    vbWord = WordsListToStringList(vbWordNode.yieldWords()).FirstOrDefault();
                    return false;
                }

                return true;
            });

            if (TryFindNumberRelation(vp, out numberStr))
            {
                // Small edge case: assume that when the user inputs a number-type query,
                // e.g. "<verb> <conj> <resource-noun> to 4.4", that they are trying to set some value
                // Note that this would allow badly-formed sentences such as "<resource-noun> 54" and these would still 
                // be treated as valid Set commands. Sentences with just a value provided, e.g. just "3213" would not pass muster as 
                // no noun-phrase has been provided.
                pred = new Predicate(gov: vbWord, dep: numberStr, action: ActionType.Set);
                found = true;
            }
            else if (TryFindBooleanRelation(vp, out boolReln))
            {
                if (vbWord != null)
                {
                    pred = new Predicate(gov: vbWord, dep: boolReln);
                    found = true;
                }
            }
            else if (!string.IsNullOrEmpty(vbWord))
            {
                pred = new Predicate(gov: vbWord);
                found = true;
            }

            verbPhrase = vp;

            return found;
        }

        private bool TryFindConditionalPredicate(Tree parseTree)
        {
            bool found = false;

            var conjunction = parseTree.firstChild();
            if (conjunction.label().value().Equals("IN"))
            {
                found = ConditionalExpression.IsValidConstructType(WordsListToStringList(conjunction.yieldWords()).FirstOrDefault());
            }

            return found;
        }

        private bool TryFindNumberRelation(Tree tree, out string numberStr)
        {
            bool found = false;
            string number = null;
            decimal placeholder = 0;

            Traverse(tree, x =>
                {
                    if (x.label().value().Equals("CD"))
                    {
                        number = WordsListToStringList(x.yieldWords()).FirstOrDefault();
                        if (decimal.TryParse(number, out placeholder))
                        {
                            found = true;
                            return false;
                        }
                    }
                    return true;
                });

            numberStr = number;
            return found;
        }

        private bool TryFindBooleanRelation(Tree tree, out string result)
        {
            bool found = false;
            string res = null;

            Traverse(tree, x =>
                {
                    if (x.label().value().Equals("PRT") ||
                        x.label().value().Equals("ADVP") ||
                        x.label().value().Equals("PP"))
                    {
                        foreach (var child in x.children())
                        {
                            if (child.label().value().Equals("IN") ||
                                child.label().value().Equals("RP") ||
                                child.label().value().Equals("RB"))
                            {
                                res = WordsListToStringList(child.yieldWords()).FirstOrDefault();
                                bool converted;
                                if (BooleanTextMap.TryGetValue(res, out converted))
                                {
                                    res = converted.ToString();
                                    found = true;
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                });

            result = res;
            return found;
        }

        private bool TryFindConditionalExpression(Tree tree, out ConditionalExpression conditional)
        {
            conditional = new ConditionalExpression();
            bool found = false;
            Tree condParent = null;
            Tree condClause = TryFindConditionalClause(tree, out condParent);
            if (condClause != null)
            {
                var condConstructType = WordsListToStringList(condClause.yieldWords()).FirstOrDefault();
                if (!String.IsNullOrEmpty(condConstructType))
                {
                    conditional.SetConstructType(condConstructType);
                    IEnumerable<string> condSubjKeywords = null;
                    if (TryFindSubjectKeywords(condClause, out condSubjKeywords))
                    {
                        if (condSubjKeywords.Count() > 0)
                        {
                            Predicate condPred = null;
                            // This is the verb-phrase embedded within the main part of the conditional clause
                            if (TryFindPredicate(condClause, out condPred))
                            {
                                // Find final action part (either comma-seperated clause or base-level verb-phrase)
                                // This will be a subject + predicate pair
                                var idx = condParent.objectIndexOf(condClause);
                                var pruned = condParent.removeChild(idx);
                                found = TryFindConditionalObject(tree, ref conditional);
                                conditional.SetConstructType(condConstructType);
                                conditional.Condition.Predicate = condPred;
                                conditional.Condition.CoerceOperatorFromPredicate();
                                conditional.Condition.ObjectKeywords.AddRange(condSubjKeywords);
                            }
                        }
                    }
                }
            }

            return found;
        }

        private bool TryFindConditionalObject(Tree tree, ref ConditionalExpression expr)
        {
            bool found = false;
            Predicate pred = null;
            Tree actualVp = null;

            if (tree == null || expr == null) { return false; }
            found = TryFindPredicate(tree, out pred, out actualVp);

            if (found)
            {
                IEnumerable<string> objKeywords = null;
                found = false;
                if (TryFindSubjectKeywords(actualVp, out objKeywords))
                {
                    found = true;
                    expr.Consequent.Predicate = pred;
                    expr.Consequent.ObjectKeywords.AddRange(objKeywords);
                }
            }

            return found;
        }

        private Tree TryFindConditionalClause(Tree tree, out Tree condParent)
        {
            Tree sbar = null;
            condParent = null;
            Tree cParent = null;

            // Search for a clause introduced by a subordinating conjunction 
            Traverse(tree, x =>
            {
                foreach (var child in x.children())
                {
                    if (child.label().value().Equals("SBAR"))
                    {
                        sbar = child;
                        cParent = x;
                        return false;
                    }
                }
                return true;
            });
            condParent = cParent;

            Tree subConj = null;
            if (sbar != null)
            {
                // Get the subordinating conjunction that started this clause
                if (sbar.firstChild().label().value().Equals("IN"))
                {
                    subConj = sbar.firstChild();
                }
            }

            // We haven't found a non-empty conditional clause if either the clause itself is missing
            // or there isn't a subordinating conjunction immediately following the clause start.
            return (subConj != null) ? sbar : null;
        }

        private bool TryFindGroupExpression(Tree tree, out GroupExpression groupExpr)
        {
            groupExpr = new GroupExpression();
            if (tree == null) { return false; }
            bool found = false;
            Tree vp = null;

            Traverse(tree, x =>
            {
                if (x.label().value().Equals("VP"))
                {
                    vp = x;
                    return false;
                }
                return true;
            });

            if (vp != null)
            {
                var allWords = WordsListToStringList(vp.yieldWords());
                if (allWords.Contains("add", StringComparer.InvariantCultureIgnoreCase))
                {
                    IEnumerable<string> resKeywords = null;
                    if (TryFindSubjectKeywords(vp, out resKeywords))
                    {
                        Tree pp = null;
                        Traverse(vp, x =>
                            {
                                if (x.label().value().Equals("PP"))
                                {
                                    pp = x;
                                    return false;
                                }
                                return true;
                            });
                        if (pp != null)
                        {
                            IEnumerable<string> groupKeywords = null;
                            if (TryFindSubjectKeywords(pp, out groupKeywords))
                            {
                                if (groupKeywords.Count() > 0)
                                {
                                    found = true;
                                    groupExpr.GroupKeywords = groupKeywords;
                                    groupExpr.ResourceKeywords = resKeywords;
                                }
                            }
                        }
                    }
                }
            }

            return found;
        }

        #region Helpers

        private List<string> WordsListToStringList(ArrayList words)
        {
            var strList = new List<string>();

            foreach (Word word in words)
            {
                strList.Add(word.value());
            }
            return strList;
        }

        private static Dictionary<string, bool> CreateBooleanTextMap()
        {
            var map = new Dictionary<string, bool>();
            map.Add("true", true);
            map.Add("on", true);
            map.Add("active", true);
            map.Add("enable", true);
            map.Add("enabled", true);

            map.Add("false", false);
            map.Add("off", false);
            map.Add("inactive", false);
            map.Add("disable", false);
            map.Add("disabled", false);
            return map;
        }

        /// <summary>
        /// Traverses a stanford NLP tree, executing f at each node before continuing to traverse.
        /// </summary>
        /// <param name="tree">The tree to traverse.</param>
        /// <param name="f">The function to execute at each node. Returns true to continue traversal.</param>
        private static void Traverse(Tree tree, Func<Tree, bool> f)
        {
            foreach (var child in tree.children())
            {
                if (!f(child)) return;
                Traverse(child, f);
            }
        }
        #endregion
    }

    public class NlpQuery
    {
        public string Text { get; set; }
        public Guid Id { get; set; }
        public NlpQuery(string text = "")
        {
            this.Text = text;
            this.Id = Guid.NewGuid();
        }
    }

    public class NlpAnalysis
    {
        public ArrayList Sentences { get; set; }
        public NlpAnalysis()
        {
            this.Sentences = null;
        }
    }

    public class Predicate
    {
        public ActionType Action { get; set; }
        public string Governor { get; set; }
        public string Dependent { get; set; }

        private static Dictionary<string, ActionType> ActionTypeMap()
        {
            var map = new Dictionary<string, ActionType>()
            {
                { "set", ActionType.Set },
                { "change", ActionType.Set },
                { "alter", ActionType.Set },
                { "turn", ActionType.Set },
                
                { "get", ActionType.Get },
                { "show", ActionType.Get },
                { "display", ActionType.Get }
            };
            return map;
        }


        public Predicate(string gov = "", string dep = "", ActionType action = ActionType.Unknown)
        {
            this.Governor = gov;
            this.Dependent = dep;
            this.Action = action;
        }

        public ActionType ResetActionType()
        {
            ActionType action = ActionType.Unknown;

            ActionTypeMap().TryGetValue(this.Governor.ToLowerInvariant(), out action);
            this.Action = action;

            return action;
        }

        public bool IsValid()
        {
            return this.Action != ActionType.Unknown;
        }

        public List<UserResponsePackage> ExecuteAction(IEnumerable<Resource> resources)
        {
            var responses = new List<UserResponsePackage>();
            foreach (var resource in resources)
            {
                switch (this.Action)
                {
                    case ActionType.Get:
                        responses.Add(UserRequestDispatcher.Instance.GetResourceData(resource.Id));
                        break;
                    case ActionType.Set:
                        UserRequestDispatcher.Instance.SetResourceData(resource.Id, this.Dependent);
                        break;
                    case ActionType.Unknown:
                    default:
                        break;
                }
            }
            return responses;
        }
    }

    public enum ActionType
    {
        Unknown, Get, Set
    }
}