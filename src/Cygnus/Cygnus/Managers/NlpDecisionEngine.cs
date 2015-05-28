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
            MakeQuery("Turn off the main kitchen light.");
        }

        public IEnumerable<UserResponsePackage> MakeQuery(string query)
        {
            var allResponses = new List<UserResponsePackage>();
            var analysis = m_engineThread.AnalyseText(query);
            foreach (Annotation sentence in analysis.Sentences)
            {
                var sentenceResponses = ExecuteSentenceRequest(sentence);
                allResponses.AddRange(sentenceResponses);
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

                // It is possible to expand further here by extracting more than one predicate/subject pair and acting upon them. 
                var subjectKeywords = GetSubjectKeywords(depMap);
                var predicate = GetActionablePredicate(depMap);
                var action = DetermineActionType(predicate);
                var resources = FindResources(subjectKeywords);
                if (action != ActionType.Unknown)
                {
                    responses.AddRange(ExecuteAction(action, predicate.Dependent, resources));
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

        private Predicate GetActionablePredicate(Dictionary<string, List<TypedDependency>> depMap)
        {
            var candidates = GetPredicateCandidates(depMap);
            
            // HACK
            return candidates[0];
        }

        private List<Predicate> GetPredicateCandidates(Dictionary<string, List<TypedDependency>> depMap)
        {
            var candidates = new List<Predicate>();
            var governors = new List<string>();
            var dependents = new List<string>();

            var searchRange = new List<TypedDependency>(depMap["advmod"]);
            searchRange.AddRange(depMap["prt"]);
            searchRange.AddRange(depMap["dobj"]);            

            ExtractDepStringRepresentation(searchRange, ref governors, ref dependents);

            if (governors.Count == dependents.Count)
            {
                for (int i = 0; i < governors.Count; i++)
                {
                    candidates.Add(new Predicate(governors[i], dependents[i]));
                }
            }

            return candidates;
            
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

        private List<Resource> FindResources(string query)
        {
            var keywords = query.Split(' ');
            var result = new List<Resource>();
            foreach (var item in keywords)
            {
                result = result.Concat(m_dbContext.Resources.Where(r =>
                    r.Name.Contains(item) ||
                    r.Description.Contains(item))).ToList();
            }

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
        public string Governor { get; set; }
        public string Dependent { get; set; }
        public Predicate(string gov = "", string dep = "")
        {
            this.Governor = gov;
            this.Dependent = dep;
        }
    }

    public enum ActionType
    {
        Get, Set, Group, Unknown
    }
}