using Cygnus.Controllers.Api;
using Cygnus.Models.Api;
using Cygnus.Nlp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Cygnus.Managers
{
    public class ResourceGroupManager
    {
        private static ResourceGroupManager m_instance;
        public static ResourceGroupManager Instance
        {
            get
            {
                if (m_instance == null) m_instance = new ResourceGroupManager();
                return m_instance;
            }
        }

        public IEnumerable<UserResponsePackage> ExecuteExpressions(IEnumerable<GroupExpression> expressions, Predicate p = null)
        {
            var results = new List<UserResponsePackage>();
            foreach (var expr in expressions)
            {
                var resolved = new ResolvedGroupExpression(expr);
                var exprResult = resolved.Execute(p);
                results.AddRange(exprResult);
            }
            return results;
        }

        public IEnumerable<UserResponsePackage> ExecuteOnGroups(IEnumerable<ResourceGroup> groups, Predicate p = null)
        {
            var expressions = groups.Select(s => new ResolvedGroupExpression()
                {
                    Group = s,
                    Resources = s.Resources
                });
            return this.ExecuteExpressions(expressions, p);
        }
    }

    internal class ResolvedGroupExpression : GroupExpression
    {
        public ResourceGroup Group { get; set; }
        public IEnumerable<Resource> Resources { get; set; }
        public ResolvedGroupExpression() : base()
        { }
        public ResolvedGroupExpression(GroupExpression other) : base()
        {
            this.GroupKeywords = other.GroupKeywords;
            this.ResourceKeywords = other.ResourceKeywords;
            this.Resolve();
        }

        private void Resolve()
        {
            // Only supporting a single group for the moment, so select the first one.
            this.Group = ResourceSearchEngine.Instance.FindGroups(GroupKeywords).FirstOrDefault();
            var candidateName = this.GroupKeywords.FirstOrDefault();
            if (this.Group == null && !String.IsNullOrEmpty(candidateName))
            {
                this.Group = new ResourceGroup()
                {
                    Name = candidateName,
                    Description = String.Empty,
                    Id = Guid.NewGuid()
                };
            }
            this.Resources = ResourceSearchEngine.Instance.FindResources(this.ResourceKeywords);
        }

        public IEnumerable<UserResponsePackage> Execute(Predicate p = null)
        {
            var responses = new List<UserResponsePackage>();
            var controller = new ResourceGroupsController();
            // Add operation
            // Try to add the group. This will fail internally if it's already in the database.
            if (p == null)
            {
                var task = controller.PostResourceGroup(this.Group);
                task.Wait();
                controller.AddResourcesToGroup(this.Group, this.Resources);
            }
            // Operation that executes a predicate on resources in this group.
            else
            {
                responses.AddRange(p.ExecuteAction(this.Group.Resources));
            }
            return responses;
        }
    }
}