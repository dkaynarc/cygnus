using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cygnus.GatewayInterface;

namespace Cygnus.GatewayTestHarness
{
    public class MockTemperatureSensor : ResourceBase
    {
        public MockTemperatureSensor(string name = "") : base(name)
        {
            m_isInitialized = true;
        }
        public override string GetResourceData()
        {
            var r = new Random();
            return (r.NextDouble() * (100.0 - 0.0) + 0.0).ToString();
        }

        public override string GetResourceDataUnits()
        {
            return "Celcius";
        }

        public override void SetResourceData(string d)
        {
        }

        public override string GetResourceDataType()
        {
            return typeof(double).ToString();
        }
    }


}
