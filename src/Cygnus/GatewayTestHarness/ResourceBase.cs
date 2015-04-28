using Cygnus.GatewayInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cygnus.GatewayTestHarness
{
    public abstract class ResourceBase : IResource
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }

        public ResourceBase(string name = "")
        {
            this.Name = name;
            this.Guid = Guid.NewGuid();
        }

        public abstract string GetResourceData();
        public abstract string GetResourceDataUnits();
        public abstract void SetResourceData(string d);
        public abstract string GetResourceDataType();
    }
}
