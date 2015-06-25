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
        private const string SavedResourcesPath = "savedresources.xml";
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

        public void Add(IResource resource)
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
            if (!this.Contains(resource.Name))
            {
                Resources.Add(resource);
                ExportResourceMap(m_nameGuidMapping);
            }
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

        public bool Contains(Guid id)
        {
            return Resources.FindAll(x => x.Guid == id).ToList().Count() > 0;
        }

        public bool Contains(string name)
        {
            return Resources.FindAll(x => x.Name.Equals(name)).ToList().Count() > 0;
        }

        public void LoadFromFile()
        {
            if (File.Exists(SavedResourcesPath))
            {
                using (var sr = new StreamReader(SavedResourcesPath))
                {
                    var xml = new XmlSerializer(typeof (TypedResourceItem[]),
                                    new XmlRootAttribute() { ElementName = "TypedResourceItems" });
                    try 
                    {
                        var items = xml.Deserialize(sr) as TypedResourceItem[];
                        if (items != null)
                        {
                            var resources = new List<IResource>();
                            resources.AddRange(items.Select(x =>
                                {
                                    IResource r = null;
                                    if (x.Type.Contains("MockSwitch"))
                                    {
                                        r = new MockSwitch(x.Name);
                                    }
                                    else if (x.Type.Contains("MockTemperatureSensor"))
                                    {
                                        r = new MockTemperatureSensor(x.Name);
                                    }
                                    return r;
                                }).Where(x => x != null));
                            foreach (var resource in resources)
                            {
                                this.Add(resource);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.Message);
                    }
                }
            }
        }

        public void SaveResources()
        {
            using (var sw = new StreamWriter(SavedResourcesPath))
            {
                var xml = new XmlSerializer(typeof(TypedResourceItem[]),
                                new XmlRootAttribute() { ElementName = "TypedResourceItems" });
                var items = this.Resources.Select(x => new TypedResourceItem() { Name = x.Name, Type = x.GetType().ToString() });
                xml.Serialize(sw, items.ToArray());
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
                        Trace.WriteLine(e.Message);
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

    public struct TypedResourceItem
    {
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string Type;
    }
}
