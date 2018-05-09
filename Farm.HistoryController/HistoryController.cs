using Farm.MessageBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.MessageBus.MessageBus;

namespace Farm.HistoryController
{
    public sealed class HistoryController : ICommunicationBase
    {
        private readonly MessageCommunicationFactory _messageCommunicationFactory;
        public string Name => "history";

        public HistoryController(MessageCommunicationFactory messageCommunicationFactory)
        {
            _messageCommunicationFactory = messageCommunicationFactory;
        }

        public void Handle(object x)
        {
            var bus = _messageCommunicationFactory.Create();
            bus.Handle(new HistoryCardMessages
            {
                
            });
        }
    }
}
