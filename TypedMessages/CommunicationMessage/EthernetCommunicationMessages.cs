using MessageBus.MessageBus;

namespace BaseController.CommunicationMessage
{
    public class ConnectionLost : MessageBase { }

    public class NetworkAdapter : MessageBase
    {
        public AdapterStatus Adapter { get; set; }

        public enum AdapterStatus
        {
            Stop,
            Start
        }
    }
    public class ConnectionRestored : MessageBase { }
}
