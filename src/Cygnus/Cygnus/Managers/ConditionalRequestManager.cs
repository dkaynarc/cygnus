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

        private List<ResolvedConditionalExpression> m_pendingExpressions;

        private ConditionalRequestManager()
        {
            m_pendingExpressions = new List<ResolvedConditionalExpression>();
        }

        public void AddExpression(ConditionalExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("Expression was null.");
            }
            var resolvedExpr = ResolvedConditionalExpression.CreateFromUnresolved(expression);
            if (!resolvedExpr.IsValid())
            {
                throw new Exception("Expression was not valid.");
            }

            // Remove all expressions that have the same conditional resource target as the newest one
            // We will replace those with what the new rule.
            int removed = m_pendingExpressions.RemoveAll(x => x.ConditionalResource.Id == resolvedExpr.ConditionalResource.Id);

            for (int i = 0; i < removed; i++)
            {
                GatewayResourceProxy.Instance.UnregisterOnMessageEvent(resolvedExpr.ConditionalResource.Id, OnMessage);
            }

            m_pendingExpressions.Add(resolvedExpr);
            this.PrepareConditionalObject(resolvedExpr);
        }

        private void PrepareConditionalObject(ResolvedConditionalExpression expr)
        {
            // We will be watching changes of this resources' value, so set its mode to push
            GatewayResourceProxy.Instance.RegisterOnMessageEvent(expr.ConditionalResource.Id, OnMessage);
            GatewayResourceProxy.Instance.SendSetCommunicationModeRequest(expr.ConditionalResource.Id,
                    CommunicationMode.Push);
        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            var originalExpr = m_pendingExpressions.Where(x => x.ConditionalResource.Id == e.ResourceId).FirstOrDefault();

            if (originalExpr != null)
            {
                ExecuteExpression(originalExpr, e);
            }
        }

        private void ExecuteExpression(ResolvedConditionalExpression expr, MessageReceivedEventArgs e)
        {
            expr.Execute(e.Data);
        }
    }

    internal class ResolvedConditionalExpression : ConditionalExpression
    {
        public IEnumerable<Cygnus.Models.Api.Resource> ConsequentResources { get; set; }
        public Cygnus.Models.Api.Resource ConditionalResource { get; set; }
        public ResolvedConditionalExpression() : base()
        {
            this.Initialize();
        }

        public ResolvedConditionalExpression(ConditionalExpression expr)
        {
            this.Condition = expr.Condition;
            this.Consequent = expr.Consequent;
            this.ConstructType = expr.ConstructType;
            this.Initialize();
        }

        private void Initialize()
        {
            this.ConsequentResources = new List<Cygnus.Models.Api.Resource>();
            this.ConditionalResource = null;
        }

        public static ResolvedConditionalExpression CreateFromUnresolved(ConditionalExpression unresolvedExpr)
        {
            var resolved = new ResolvedConditionalExpression(unresolvedExpr);
            resolved.Resolve();
            return resolved;
        }

        public void Resolve()
        {
            this.CoercePredicateActions();
            this.ConsequentResources = ResourceSearchEngine.Instance.FindResources(this.Consequent.ObjectKeywords);
            this.ConditionalResource = ResourceSearchEngine.Instance.FindResources(this.Condition.ObjectKeywords).FirstOrDefault();
        }

        public override bool IsValid()
        {
            var valid = base.IsValid() &&
                    this.ConditionalResource != null &&
                    this.ConsequentResources.Count() > 0;
            return valid;
        }

        public IEnumerable<UserResponsePackage> Execute(string data)
        {
            var responses = new List<UserResponsePackage>();
            if (this.Condition.Evaluate(data))
            {
                responses.AddRange(this.Consequent.Predicate.ExecuteAction(this.ConsequentResources));
            }
            return responses;
        }
    }
}