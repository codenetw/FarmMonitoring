using System.Collections.Generic;

namespace MessageBus.MessageBus
{
    public interface IMessageCommunication
    {
        void Publish<TMessage>(TMessage message)
            where TMessage : MessageBase;

        IEnumerable<TResult> Query<TResult, TMessage>(TMessage message)
            where TMessage : MessageBase
            where TResult : ResultMessageBase;

        TResult Execute<TResult, TMessage, TModule>(TMessage message)
            where TMessage : MessageBase
            where TResult : ResultMessageBase
            where TModule : ICommunicationBase;

        void Execute<TMessage, TModule>(TMessage message)
            where TMessage : MessageBase
            where TModule : ICommunicationBase;

        void Publish<TMessage>()
            where TMessage : MessageBase , new();
    }
}