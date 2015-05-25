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

namespace Cygnus.Managers
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
        private ConcurrentDictionary<Guid, NlpQuery> m_responseBucket = new ConcurrentDictionary<Guid, NlpQuery>();
        private volatile bool m_keepThreadAlive = false;

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

        public void PushQuery(string queryText)
        {
            this.m_requestQueue.Enqueue(new NlpQuery() { Text = queryText });
        }

        // placeholder
        public object GetResponse(Guid ticket)
        {
            object response = null;
            if (m_responseBucket.ContainsKey(ticket))
            {
                //
            }
            return response;
        }

        private void Run()
        {
            while (m_keepThreadAlive)
            {
                NlpQuery pending = null;
                if (m_requestQueue.TryDequeue(out pending))
                {
                    this.ProcessQuery(pending);
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void ProcessQuery(NlpQuery query)
        {
            if (IsInitialized)
            {
                //var text = "I went or a run. Then I went to work. I had a good lunch meeting with a friend name John Jr. The commute home was pretty good.";
                var text = query.Text;
                var annotation = new Annotation(text);
                m_pipeline.annotate(annotation);

                var sentences = annotation.get(typeof(CoreAnnotations.SentencesAnnotation));
                if (sentences == null)
                {
                    return;
                }
                foreach (Annotation sentence in sentences as ArrayList)
                {
                    Trace.WriteLine(sentence);
                }
            }
        }
    }
}