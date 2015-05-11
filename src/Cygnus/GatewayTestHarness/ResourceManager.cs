using Cygnus.GatewayInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Cygnus.GatewayTestHarness
{
    public class ResourceManager
    {
        private static ResourceManager m_instance;
        private const string GuidDbPath = "resources.xml";
        public static ResourceManager Instance
        {
            get
            {
                if (m_instance == null) m_instance = new ResourceManager();
                return m_instance;
            }
        }
        public List<IResource> Resources { get; private set; }

        private Dictionary<string, Guid> m_nameGuidMapping;

        private ResourceManager()
        {
            Resources = new List<IResource>();
            m_nameGuidMapping = ImportResourceMap();
        }

        public void Add (IResource resource)
        {
            Guid guid;
            if (m_nameGuidMapping.TryGetValue(resource.Name, out guid))
            {
                resource.Guid = guid;
            }
            else
            {
                m_nameGuidMapping.Add(resource.Name, resource.Guid);
            }
            Resources.Add(resource);
            ExportResourceMap(m_nameGuidMapping);
        }

        public void Remove(IResource resource)
        {
            if (m_nameGuidMapping.ContainsKey(resource.Name))
            {
                m_nameGuidMapping.Remove(resource.Name);
                Resources.Remove(resource);
                ExportResourceMap(m_nameGuidMapping);
            }
        }

        private static Dictionary<string, Guid> ImportResourceMap()
        {
            var resourceDb = new Dictionary<string, Guid>();
            if (File.Exists(GuidDbPath))
            {
                using (var sr = new StreamReader(GuidDbPath))
                {
                    var xml = new XmlSerializer(typeof(ResourceItem[]),
                                    new XmlRootAttribute() { ElementName = "ResourceItems" });
                    try
                    {
                        resourceDb = ((ResourceItem[])xml.Deserialize(sr)).ToDictionary(i => i.Key, i => i.Value);
                    }
                    catch (Exception e)
                    {
                        Debug.Print(e.Message);
                    }
                }
            }

            return resourceDb;
        }

        private static void ExportResourceMap(Dictionary<string, Guid> map)
        {
            using (var sw = new StreamWriter(GuidDbPath))
            {
                var xml = new XmlSerializer(typeof(ResourceItem[]),
                                new XmlRootAttribute() { ElementName = "ResourceItems" });
                xml.Serialize(sw, map.Select(kv => new ResourceItem() { Key = kv.Key, Value = kv.Value }).ToArray() );
            }
        }
    }
    public struct ResourceItem
    {
        [XmlAttribute]
        public string Key;
        [XmlAttribute]
        public Guid Value;
    }
}
