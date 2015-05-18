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
        bool IsInitialized { get; }
        string GetResourceData();
        string GetResourceDataUnits();
        void SetResourceData(string d);
        string GetResourceDataType();
        string GetMax();
        string GetMin();
        event DataChangedHandler OnDataChanged;
        CommunicationMode CommunicationMode { get; set; }
        
    }
    public delegate void DataChangedHandler(object sender, DataChangedEventArgs e);
    public class DataChangedEventArgs
    {
        public string Data { get; set; }
        public DataChangedEventArgs(string data = "")
        {
            this.Data = data;
        }
    }
    public enum CommunicationMode
    {
        Push,
        Poll
    }
}
