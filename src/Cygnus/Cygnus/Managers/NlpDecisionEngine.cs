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

namespace Cygnus.Managers
{
    public class NlpDecisionEngine
    {
        private static NlpDecisionEngine m_instance;
        private ApplicationDbContext m_db = new ApplicationDbContext();
        public static NlpDecisionEngine Instance
        {
            get
            {
                if (m_instance == null) m_instance = new NlpDecisionEngine();
                return m_instance;
            }
        }
        private NlpEngineThread m_engineThread = new NlpEngineThread();
        
        private NlpDecisionEngine()
        {
        }

        public void Initialize()
        {
            m_engineThread.Start();
        }

        public void Test()
        {
            MakeQuery("Sample text. The quick brown fox jumps over the lazy dog.");
        }

        public void MakeQuery(string query)
        {
            m_engineThread.PushQuery(query);
        }
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
}