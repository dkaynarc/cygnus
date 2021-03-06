﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cygnus.GatewayInterface;

namespace Cygnus.GatewayTestHarness
{
    public class MockTemperatureSensor : ResourceBase
    {
        private double m_temperature = 0.0;
        public MockTemperatureSensor(string name = "") : base(name)
        {
            m_isInitialized = true;
        }
        public override string GetResourceData()
        {
            return m_temperature.ToString();
        }

        public override string GetResourceDataUnits()
        {
            return "Celcius";
        }

        public override void SetResourceData(string d)
        {
            if (Double.TryParse(d, out m_temperature))
            {
                RaiseOnDataChangedEvent(d);
            }
        }

        public override string GetResourceDataType()
        {
            return typeof(double).ToString();
        }

        public override string GetMax()
        {
            return (100.0).ToString();
        }

        public override string GetMin()
        {
            return (0.0).ToString();
        }
    }


}
