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
        public event DataChangedHandler OnDataChanged;
        public Guid Guid { get; set; }
        public string Name { get; set; }
        protected bool m_isInitialized = false;
        public bool IsInitialized { get { return m_isInitialized; } }

        public ResourceBase(string name = "")
        {
            this.Name = name;
            this.Guid = Guid.NewGuid();
        }

        public abstract string GetResourceData();
        public abstract string GetResourceDataUnits();
        public abstract void SetResourceData(string d);
        public abstract string GetResourceDataType();
        public abstract string GetMax();
        public abstract string GetMin();

        public void RaiseOnDataChangedEvent(string data)
        {
            if (OnDataChanged != null)
            {
                OnDataChanged(this, new DataChangedEventArgs(data));
            }
        }
    }
}
