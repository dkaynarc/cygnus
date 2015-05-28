using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cygnus.GatewayInterface
{
    public interface INotifiableRequester
    {
        void Notify(Guid id, object data);
    }
}
