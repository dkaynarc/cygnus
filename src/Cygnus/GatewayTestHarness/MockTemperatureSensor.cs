using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cygnus.GatewayInterface;

namespace Cygnus.GatewayTestHarness
{
    public class MockTemperatureSensor : IResource
    {
        public string GetResourceData()
        {
            var r = new Random();
            return (r.NextDouble() * (100.0 - 0.0) + 0.0).ToString();
        }

        public string GetResourceDataUnits()
        {
            return "Celcius";
        }

        public void SetResourceData(string d)
        {
        }

        public string GetResourceDataType()
        {
            return typeof(double).ToString();
        }
    }


}
