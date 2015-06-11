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
using System.Collections.Concurrent;

namespace Cygnus.Nlp
{
    public sealed class NlpEngineThread
    {
        public bool IsInitialized { get; private set; }
        private StanfordCoreNLP m_pipeline;
        private ApplicationDbContext m_db = new ApplicationDbContext();
        private const string NlpJarRoot = @"C:\NlpModels\stanford-corenlp-3.5.1-models";
        private Thread m_worker = null;
        private const int StackSize = 4 * 1024 * 1024;
        private ConcurrentQueue<NlpQuery> m_requestQueue = new ConcurrentQueue<NlpQuery>();
        private ConcurrentDictionary<Guid, NlpAnalysis> m_responseBucket = new ConcurrentDictionary<Guid, NlpAnalysis>();
        private volatile bool m_keepThreadAlive = false;
        private static AutoResetEvent m_waitEvent = new AutoResetEvent(false);

        public NlpEngineThread()
        {
            this.IsInitialized = false;
            m_worker = new Thread(new ThreadStart(this.InitializeThread), StackSize);
        }

        private void InitializeThread()
        {
            Trace.WriteLine("Initializing NlpEngineThread.");
            // Annotation pipeline configuration
            var props = new Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner, parse, dcoref");
            props.setProperty("sutime.binders", "0");

            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(NlpJarRoot);
            m_pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);

            IsInitialized = true;
            Trace.WriteLine("NlpEngineThread initialization complete.");
            Run();
        }

        public void Start()
        {
            m_keepThreadAlive = true;
            m_worker.Start();
        }

        public void Stop()
        {
            m_keepThreadAlive = false;
        }

        public NlpAnalysis AnalyseText(string queryText)
        {
            var analysis = new NlpAnalysis();
            if (IsInitialized)
            {
                var query = new NlpQuery(queryText);
                m_requestQueue.Enqueue(query);
                m_waitEvent.WaitOne();

                if (!m_responseBucket.TryRemove(query.Id, out analysis))
                {
                    throw new Exception("Could not remove analysis from the bucket.");
                }
            }
            else
            {
                throw new InvalidOperationException("NlpEngineThread not yet initialized");
            }
            return analysis;
        }

        private void Run()
        {
            while (m_keepThreadAlive)
            {
                NlpQuery pending = null;
                if (m_requestQueue.TryDequeue(out pending))
                {
                    var analysis = AnalyseText(pending);
                    m_responseBucket.AddOrUpdate(pending.Id, analysis, (key, prev) => analysis);
                    m_waitEvent.Set();
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private NlpAnalysis AnalyseText(NlpQuery query)
        {
            var analysis = new NlpAnalysis();
            if (IsInitialized)
            {
                var text = query.Text;
                var annotation = new Annotation(text);
                m_pipeline.annotate(annotation);

                var sentences = annotation.get(typeof(CoreAnnotations.SentencesAnnotation)) as ArrayList;
                if (sentences != null)
                {
                    analysis.Sentences = sentences;
                }
            }
            return analysis;
        }
    }
}