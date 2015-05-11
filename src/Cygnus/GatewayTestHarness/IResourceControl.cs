using Cygnus.GatewayInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cygnus.GatewayTestHarness
{
    public interface IResourceControl
    {
        void BindResource(IResource resource);
        void UnbindResource();
    }
}
