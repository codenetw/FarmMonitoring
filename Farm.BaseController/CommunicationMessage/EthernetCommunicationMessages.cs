using Farm.MessageBus.MessageBus;

namespace Farm.BaseController.CommunicationMessage
{
    public class ConnectionOk : MessageBase { }
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
    public class ResetAdapterMessage : MessageBase { }
}
