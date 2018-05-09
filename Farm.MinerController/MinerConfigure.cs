using System;
using Farm.BaseController.CommunicationMessage;

namespace Farm.MinerController
{
    public class MinerConfigure 
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public TimeSpan IntervalCheck { get; set; }
        public string RestartIfNotResponce { get; set; }
        public ProcessInfo DependencyProcess { get; set; }
    }
}
