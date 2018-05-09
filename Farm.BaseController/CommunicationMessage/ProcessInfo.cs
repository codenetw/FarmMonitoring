using System.Collections.Generic;

namespace Farm.BaseController.CommunicationMessage
{
    public class ProcessInfo
    {
        public string Name { get; set; }
        public string Exec { get; set; }
        public string Parameters { get; set; }
        public bool WatchDog { get; set; }
        public Dictionary<string, string> EnvVariable { get; set; }
    }
}