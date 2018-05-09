using MessageBus.MessageBus;

namespace BaseController.CommunicationMessage
{
    public class MinerLog : MessageBase
    {
        public string Log { get; set; }
    }
}
