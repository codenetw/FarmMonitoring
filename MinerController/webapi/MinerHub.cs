using BaseController.CommunicationMessage;
using FarmMonitoring.backend.Common;
using MessageBus;
using MessageBus.MessageBus;

namespace MinerController.webapi
{
    public class MinerHub : WebSocketHub<MinerHub>, ICommunicationBase
    {
        public MinerHub()
        { }

        #region listener message

        public void Handle(MinerStatistics statistics)
        {
            if (statistics.Status == MessageStatus.Ok)
                BroadcastMessage(statistics);
        }

        public void Handle(MinerLog log)
        {
            if (log.Status == MessageStatus.Ok)
                BroadcastMessage(log);
        }

        #endregion

    }
}
