using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatewayInterface
{
    public struct ResourceMessage
    {
        // get / set / response
        public string Command { get; set; }
        public string SenderGuid { get; set; }
        public string Data { get; set; }
        public string DataType { get; set; }
        public string DataUnits { get; set; }
    }
}
