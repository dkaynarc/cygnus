using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cygnus.GatewayTestHarness
{
    public class MockSwitch : ResourceBase
    {
        public bool m_isSwitchOn = false;
        public MockSwitch(string name = "") : base(name)
        {
            m_isInitialized = true;
        }
        public override string GetResourceData()
        {
            return m_isSwitchOn.ToString();
        }

        public override string GetResourceDataUnits()
        {
            return "boolean";
        }

        public override void SetResourceData(string d)
        {
            if (Boolean.TryParse(d, out m_isSwitchOn))
            {
                RaiseOnDataChangedEvent(d);
            }
        }

        public override string GetResourceDataType()
        {
            return typeof(bool).ToString();
        }

        public override string GetMax()
        {
            return "true";
        }

        public override string GetMin()
        {
            return "false";
        }
    }
}
