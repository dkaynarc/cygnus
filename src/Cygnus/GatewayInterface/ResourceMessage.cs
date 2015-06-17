using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cygnus.GatewayInterface
{
    public struct ResourceMessage
    {
        // get / set / response / mode / push-response
        public string Command { get; set; }
        public string TargetGuid { get; set; }
        public string SenderGuid { get; set; }
        public string RequestGuid { get; set; }
        public string Data { get; set; }
        public string DataType { get; set; }
        public string DataUnits { get; set; }
    }
}
