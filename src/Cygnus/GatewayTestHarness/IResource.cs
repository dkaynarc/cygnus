using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayTestHarness
{
    public interface IResource
    {
        string GetResourceData();
        string GetResourceDataUnits();
        void SetResourceData(string d);
        string GetResourceDataType();
    }
}
