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
        private static readonly Dictionary<string, ActionType> ActionTypeMap;
        private static readonly Dictionary<string, bool> BooleanTextMap;

        private NlpDecisionEngine()
        {
        }

        static NlpDecisionEngine()
        {
            ActionTypeMap = CreateActionTypeMap();
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
            var basicGraph = sentence.get(typeof(SemanticGraphCoreAnnotations.BasicDependenciesAnnotation)) as SemanticGraph;
            var responses = new List<UserResponsePackage>();
            if (basicGraph != null)
            {
                var deps = basicGraph.typedDependencies();
                var depMap = CreateDepMap();
                // Populate the dependency map with all relation types we understand
                foreach (TypedDependency typedDep in deps as ArrayList)
                {
                    var reln = typedDep.reln();
                    if (depMap.ContainsKey(reln.getShortName()))
                    {
                        depMap[reln.getShortName()].Add(typedDep);
                    }
                }
                var parseTree = sentence.get(typeof(TreeCoreAnnotations.TreeAnnotation)) as Tree;

                // It is possible to expand further here by extracting more than one predicate/subject pair and acting upon them. 
                //var subjectKeywords = GetSubjectKeywords(depMap);
                IEnumerable<string> subjectKeywords = null;
                if (TryFindSubjectKeywords(parseTree, out subjectKeywords))
                {
                    Predicate predicate = null;
                    if (TryFindPredicate(parseTree, out predicate))
                    {
                        var action = DetermineActionType(predicate);
                        var resources = FindResources(subjectKeywords);
                        if (action != ActionType.Unknown)
                        {
                            responses.AddRange(ExecuteAction(action, predicate.Dependent, resources));
                        }
                    }
                }
            }
            return responses;
        }

        private string GetSubjectKeywords(Dictionary<string,List<TypedDependency>> depMap)
        {
            var governors = new List<string>();
            var dependents = new List<string>();
            string accumulator = "";

            var searchRange = new List<TypedDependency>(depMap["nn"]);
            searchRange.AddRange(depMap["amod"]);

            ExtractDepStringRepresentation(searchRange, ref governors, ref dependents);

            accumulator = String.Join(" ", governors.Union(dependents));
            
            return accumulator;
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
                if (x.label().value().Equals("NP"))
                {
                    np = x;
                    return false;
                }
                return true;
            });

            if (np != null)
            {
                words.AddRange((string[])np.yieldWords().toArray());
            }

            // Conjunctions aren't really keywords so we'll filter them out
            words.RemoveAll(x => conjunctions.Contains(x));

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
                        vbWord = (string)x.yieldWords().toArray()[0];
                        return false;
                    }
                    return true;
                });

            bool found = false;
            string numberStr = null;
            pred = null;
            bool boolReln;
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
                    pred = new Predicate(gov: vbWord, dep: boolReln.ToString());
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

            Traverse(tree, x =>
                {
                    if (x.label().value().Equals("CD"))
                    {
                        found = true;
                        number = (string)x.yieldWords().toArray()[0];
                        return false;
                    }
                    return true;
                });

            numberStr = number;
            return found;
        }

        private bool TryFindBooleanRelation(Tree tree, out bool result)
        {
            bool found = false;
            bool res = false;

            Traverse(tree, x =>
                {
                    if (x.label().value().Equals("PRT") ||
                        x.label().value().Equals("ADVP"))
                    {
                        if (Boolean.TryParse((string)x.yieldWords().toArray()[0], out res))
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
        private ActionType DetermineActionType(Predicate predicate)
        {
            ActionType action = ActionType.Unknown;

            ActionTypeMap.TryGetValue(predicate.Governor.ToLower(), out action);

            return action;
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

        #region Helpers

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
                .Where(r => notFoundWords.All(kw => r.Description.ToLowerInvariant().Contains(kw.ToLower())))).ToList();

            return result;
        }

        private void ExtractDepStringRepresentation(List<TypedDependency> deps, ref List<string> governors, ref List <string> dependents)
        {
            if (governors == null || dependents == null)
            {
                throw new ArgumentNullException();
            }
            foreach (var dep in deps)
            {
                var govStr = dep.gov().toString();
                var depStr = dep.dep().toString();
                // Remove the information that trails behind the word
                governors.Add(govStr.Remove(govStr.IndexOf('/')));
                dependents.Add(depStr.Remove(depStr.IndexOf('/')));
            }
        }

        private static Dictionary<string, List<TypedDependency>> CreateDepMap()
        {
            var map = new Dictionary<string, List<TypedDependency>>();
            // Adverbial modifiers
            map.Add("advmod", new List<TypedDependency>());
            // Adjective modifiers
            map.Add("amod", new List<TypedDependency>());
            // Dependent objects
            map.Add("dobj", new List<TypedDependency>());
            // Determiners
            map.Add("det", new List<TypedDependency>());
            // nn modifier
            map.Add("nn", new List<TypedDependency>());
            // Phrasal Verb Partical
            map.Add("prt", new List<TypedDependency>());
            // Numerical 
            map.Add("num", new List<TypedDependency>());
            return map;
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

        private static Dictionary<string, ActionType> CreateActionTypeMap()
        {
            var map = new Dictionary<string, ActionType>()
            {
                { "set", ActionType.Set },
                { "change", ActionType.Set },
                { "alter", ActionType.Set },
                
                { "get", ActionType.Get },
                { "show", ActionType.Get },
                
                { "group", ActionType.Group }
            };
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
        public Predicate(string gov = "", string dep = "", ActionType action = ActionType.Unknown)
        {
            this.Governor = gov;
            this.Dependent = dep;
            this.Action = action;
        }
    }

    public enum ActionType
    {
        Get, Set, Group, Unknown
    }
}