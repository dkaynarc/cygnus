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

        private static string NlpJarRoot = @"C:\NlpModels\";

        private NlpDecisionEngine()
        {
        }

        public void Test()
        {
            var text = "The quick brown fox jumped over the lazy dog.";

            // Annotation pipeline configuration
            var props = new Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner, parse, dcoref");
            props.setProperty("sutime.binders", "0");

            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(NlpJarRoot);
            var pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);

            var annotation = new Annotation(text);
            pipeline.annotate(annotation);

            using (var stream = new ByteArrayOutputStream())
            {
                pipeline.prettyPrint(annotation, new PrintWriter(stream));
                Debug.WriteLine(stream.toString());
                stream.close();
            }
        }
    }
}