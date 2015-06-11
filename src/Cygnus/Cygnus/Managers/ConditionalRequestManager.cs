using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cygnus.Nlp;
using Cygnus.Models;
using Cygnus.Models.Api;
using Cygnus.GatewayInterface;

namespace Cygnus.Managers
{
    public sealed class ConditionalRequestManager
    {
        private static ConditionalRequestManager m_instance;
        public static ConditionalRequestManager Instance
        {
            get
            {
                if (m_instance == null) m_instance = new ConditionalRequestManager();
                return m_instance;
            }
        }

        private ApplicationDbContext m_db = new ApplicationDbContext();
        private List<ResolvedExpression> m_pendingExpressions;

        private ConditionalRequestManager()
        {
            m_pendingExpressions = new List<ResolvedExpression>();
        }

        public void AddExpression(ConditionalExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("Expression was null.");
            }
            var resolvedExpr = ResolvedExpression.CreateFromUnresolved(expression);
            if (!resolvedExpr.IsValid())
            {
                throw new Exception("Expression was not valid.");
            }

            this.m_pendingExpressions.Add(resolvedExpr);
        }

        private void PrepareConditionalObject(ResolvedExpression expr)
        {
            // We will be watching changes of this resources' value, so set its mode to push
            GatewayResourceProxy.Instance.RegisterOnMessageEvent(expr.ConditionalResource.Id, OnMessage);
            GatewayResourceProxy.Instance.SendSetCommunicationModeRequest(expr.ConditionalResource.Id,
                    CommunicationMode.Push);            

        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            var originalExpr = m_pendingExpressions.Where(x => x.ConditionalResource.Id == e.ResourceId).FirstOrDefault();
            TryExecuteExpression(originalExpr, e);
        }

        private void TryExecuteExpression(ResolvedExpression expr, MessageReceivedEventArgs e)
        {
            // Compare the result with the trigger value
        }
    }

    internal class ResolvedExpression : ConditionalExpression
    {
        public IEnumerable<Cygnus.Models.Api.Resource> ConsequantResources { get; set; }
        public Cygnus.Models.Api.Resource ConditionalResource { get; set; }
        public ResolvedExpression() : base()
        {
            this.Initialize();
        }

        public ResolvedExpression(ConditionalExpression expr)
        {
            this.Condition = expr.Condition;
            this.Consequant = expr.Consequant;
            this.ConstructType = expr.ConstructType;
            this.Initialize();
        }

        private void Initialize()
        {
            this.ConsequantResources = new List<Cygnus.Models.Api.Resource>();
            this.ConditionalResource = null;
        }

        public static ResolvedExpression CreateFromUnresolved(ConditionalExpression unresolvedExpr)
        {
            var resolved = new ResolvedExpression(unresolvedExpr);
            resolved.Resolve();
            return resolved;
        }

        public void Resolve()
        {
            this.ConsequantResources = ResourceSearchEngine.Instance.FindResources(this.Consequant.ObjectKeywords);
            this.ConditionalResource = ResourceSearchEngine.Instance.FindResources(this.Condition.ObjectKeywords).FirstOrDefault();
        }

        public override bool IsValid()
        {
            return
                (
                    base.IsValid() &&
                    this.ConditionalResource != null &&
                    this.ConsequantResources.Count() > 0
                );
        }
    }
}