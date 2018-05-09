using Autofac;

namespace MessageBus.MessageBus
{
    public class MessageCommunicationFactory
    {
        private readonly ILifetimeScope _scope;

        public MessageCommunicationFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public IMessageCommunication Create()
        {
            return _scope.Resolve<IMessageCommunication>();
        }
    }
}