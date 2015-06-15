using Cygnus.Models;
using Cygnus.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cygnus.Managers
{
    public sealed class ResourceSearchEngine
    {
        private static ResourceSearchEngine m_instance;
        public static ResourceSearchEngine Instance
        {
            get
            {
                if (m_instance == null) m_instance = new ResourceSearchEngine();
                return m_instance;
            }
        }
        private ResourceSearchEngine()
        {
        }

        public IEnumerable<Resource> FindResources(IEnumerable<string> keywords)
        {
            var result = new List<Resource>();
            using (var context = new ApplicationDbContext())
            {
                var notFoundWords = new List<string>();
                foreach (var item in keywords)
                {
                    var initialResultCount = result.Count;
                    // Find exact matches, ignoring case, for resource names
                    result = result.Union(context.Resources.Where(r => r.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase))).ToList();
                    // Keep track of words that aren't found as these are resource names.
                    if (result.Count == initialResultCount)
                    {
                        notFoundWords.Add(item);
                    }
                }

                // Find descriptions that contain all keywords amongst those that haven't turned up results with the exact match against resource names.
                // We filter these out because it's likely that the user won't be specifying exact device names when also providing keyword matches.
                if (notFoundWords.Count() > 0)
                {
                    result = result.Union(context.Resources
                        .Where(r => notFoundWords.All(kw => r.Description.ToLower().Contains(kw.ToLower())))).ToList();
                }
            }
            return result;
        }

        public IEnumerable<ResourceGroup> FindGroups(IEnumerable<string> keywords)
        {
            var result = new List<ResourceGroup>();
            using (var context = new ApplicationDbContext())
            {
                foreach (var item in keywords)
                {
                    // Basic search on on resource group names
                    result = result.Union(context.ResourceGroups.Where(r => r.Name.Equals(item, StringComparison.InvariantCultureIgnoreCase))).ToList();
                }
            }

            return result;
        }
    }
}