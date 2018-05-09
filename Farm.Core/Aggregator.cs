using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.BaseController.CommunicationMessageModel;
using Farm.Core.Common;
using Farm.MessageBus;
using Farm.MessageBus.MessageBus;
using Farm.Moderator;

namespace Farm.Core
{
    public class Aggregator : WebSocketHub<IMessageBase>, ICommunicationBase
    {
        private readonly Configuration _configuration;
        private readonly IModerator _moderator;
        public string Name => "aggregator";
        public static AggregatorState CurrentState => new Lazy<AggregatorState>(LazyThreadSafetyMode.ExecutionAndPublication).Value;

        public Aggregator(Configuration configuration, IModerator moderator)
        {
            _configuration = configuration;
            _moderator = moderator;
        }

        public void Handle(IMessageBase message, string messageCode)
        {
            var ctrl = _configuration.WebApiTranslateMessages.FirstOrDefault(x =>
                string.Compare(messageCode, x, StringComparison.OrdinalIgnoreCase) == 0);
            UpdateState(message);

            if (ctrl != null)
                BroadcastMessage(message);
        }

        public void Handle(IMessageBase message)
        {
            BroadcastMessage($"call {message.GetType().Name}");
        }

        private void UpdateState(IMessageBase message)
        {
            switch (true)
            {
                case bool _ when message.GetType() == typeof(ConnectionLost):
                    CurrentState.IsConnected = false;
                    break;
                case bool _ when message.GetType() == typeof(ConnectionOk):
                    CurrentState.IsConnected = true;
                    break;
                case bool _ when message.GetType() == typeof(MinerStatistics):
                    CurrentState.MinerApiInfo = (MinerStatistics) message;
                    break;
                case bool _ when message.GetType() == typeof(AutobernerInformationMessage):
                    CurrentState.Cards = ((AutobernerInformationMessage)message).CurrentInfoCards;
                    break;
                case bool _ when message.GetType() == typeof(AutobernerStopWatchingMessage):
                    CurrentState.IsAutobernerStoped = true;
                    break;
            }

            _moderator.UpdateState(CurrentState);
        }
    }
   
}
