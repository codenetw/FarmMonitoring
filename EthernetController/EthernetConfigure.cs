using System;
using BaseController.CommunicationMessage;

namespace EthernetController
{
    public class EthernetConfigure 
    {
        public string AddressCheck { get; set; }
        public TimeSpan IntervalCheck { get; set; }
        public TimeSpan ResetDelay { get; set; }
        public bool ResetIfLost { get; set; }
        public ProcessInfo RestoreTool { get; set; }
        public string[] AdapterList { get; set; }
        public int RetryCount { get; set; }
    }
}
