using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cygnus.GatewayInterface
{
    public interface IResource
    {
        string Name { get; set;  }
        Guid Guid { get; set; }
        string GetResourceData();
        string GetResourceDataUnits();
        void SetResourceData(string d);
        string GetResourceDataType();
    }
}
