using System;
using Farm.BaseController.CommunicationMessage;

namespace Farm.EthernetController
{
    public class EthernetConfigure 
    {
        public string AddressCheck { get; set; }
        public TimeSpan IntervalCheck { get; set; }
        public TimeSpan ResetDelay { get; set; }
        public bool ReadOnlyAdapter { get; set; }
        public ProcessInfo RestoreTool { get; set; }
        public string[] AdapterList { get; set; }
        public int RetryCount { get; set; }
    }
}
