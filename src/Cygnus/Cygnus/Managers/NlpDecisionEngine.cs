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

namespace Cygnus.Managers
{
    public class NlpDecisionEngine
    {
        private static NlpDecisionEngine m_instance;
        private ApplicationDbContext m_dbContext = new ApplicationDbContext();
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

        public void Test()
        {
            var resources = FindResources("room living light".Split(' '));
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
                throw new Exception("Decision engine not yet initialized", e);
            }
            return allResponses;
        }

        private IEnumerable<UserResponsePackage> ExecuteSentenceRequest(Annotation sentence)
        {
            var responses = new List<UserResponsePackage>();
            var parseTree = sentence.get(typeof(TreeCoreAnnotations.TreeAnnotation)) as Tree;

            // It is possible to expand further here by extracting more than one predicate/subject pair and acting upon them. 
            //var subjectKeywords = GetSubjectKeywords(depMap);
            IEnumerable<string> subjectKeywords = null;
            if (TryFindSubjectKeywords(parseTree, out subjectKeywords))
            {
                Predicate predicate = null;
                if (TryFindPredicate(parseTree, out predicate))
                {
                    var action = predicate.ResetActionType();
                    var resources = FindResources(subjectKeywords);
                    responses.AddRange(ExecuteAction(predicate.Action, predicate.Dependent, resources));
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
                if (x.label().value().Equals("DT"))
                {
                    conjunctions = WordsListToStringList(x.yieldWords());
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
            Tree vp = null;
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

            // find the key verb (VB)
            string vbWord = null;
            Traverse(parseTree, x =>
                {
                    if (x.label().value().Equals("VB"))
                    {
                        vbWord = WordsListToStringList(x.yieldWords()).FirstOrDefault();
                        return false;
                    }
                    return true;
                });

            bool found = false;
            string numberStr = null;
            pred = null;
            string boolReln;
            if (TryFindNumberRelation(vp, out numberStr))
            {
                // Small edge case: assume that when the user inputs a number-type query,
                // e.g. "<verb> <conj> <resource-noun> to 4.4", that they are trying to set some value
                // Note that this would allow badly-formed sentences such as "<resource-noun> 54" and these would still 
                // be treated as valid Set commands. Sentences with just a value provided, e.g. just "3213" would not pass muster as 
                // no noun-phrase has been provided.
                found = true;
                pred = new Predicate(gov: vbWord, dep: numberStr, action: ActionType.Set);
            }
            else if (TryFindBooleanRelation(vp, out boolReln))
            {
                if (vbWord != null)
                {
                    found = true;
                    pred = new Predicate(gov: vbWord, dep: boolReln);
                }
            }

            return found;
        }

        private void Traverse(Tree tree, Func<Tree, bool> f)
        {
            var curDepth = 0;
            var maxDepth = tree.depth();
            foreach (var child in tree.children())
            {
                if (curDepth > maxDepth) return;
                if (!f(child)) return;
                Traverse(child, f);
                curDepth++;
            }
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
                        x.label().value().Equals("ADVP"))
                    {
                        res = WordsListToStringList(x.yieldWords()).FirstOrDefault();
                        if (!String.IsNullOrEmpty(res))
                        {
                            found = true;
                            return false;
                        }
                    }
                    return true;
                });

            result = res;
            return found;
        }

        private List<UserResponsePackage> ExecuteAction(ActionType action, string actionParam, IEnumerable<Resource> resources)
        {
            var responses = new List<UserResponsePackage>();
            foreach (var resource in resources)
            {
                switch (action)
                {
                    case ActionType.Get:
                        responses.Add(UserRequestDispatcher.Instance.GetResourceData(resource.Id));
                        break;
                    case ActionType.Set:
                        UserRequestDispatcher.Instance.SetResourceData(resource.Id, actionParam);
                        break;
                    // No defined action handler for now
                    case ActionType.Group:
                    case ActionType.Unknown:
                    default:
                        break;
                }
            }
            return responses;
        }
        private IEnumerable<Resource> FindResources(IEnumerable<string> keywords)
        {
            var result = new List<Resource>();
            var notFoundWords = new List<string>();
            foreach (var item in keywords)
            {
                var initialResultCount = result.Count;
                // Find exact matches, ignoring case, for resource names
                result = result.Union(m_dbContext.Resources.Where(r => r.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase))).ToList();
                // Keep track of words that aren't found as these are resource names.
                if (result.Count == initialResultCount)
                {
                    notFoundWords.Add(item);
                }
            }

            // Find descriptions that contain all keywords amongst those that haven't turned up results with the exact match against resource names.
            // We filter these out because it's likely that the user won't be specifying exact device names when also providing keyword matches.
            result = result.Union(m_dbContext.Resources
                .Where(r => notFoundWords.All(kw => r.Description.ToLower().Contains(kw.ToLower())))).ToList();

            return result;
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

        #endregion
    }

    public class NlpQuery
    {
        public string Text { get; set; }
        public Guid Id { get; set; }
        public AutoResetEvent WaitEvent { get; set; }
        public NlpQuery(string text = "")
        {
            this.Text = text;
            this.Id = Guid.NewGuid();
            this.WaitEvent = new AutoResetEvent(false);
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
                { "display", ActionType.Get },
                
                { "group", ActionType.Group }
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
    }

    public enum ActionType
    {
        Get, Set, Group, Unknown
    }
}