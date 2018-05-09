using System.Collections.Generic;

namespace Farm.MessageBus.MessageBus
{
    public interface IMessageCommunication
    {

        void Handle<TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase;

        IEnumerable<TResult> Query<TResult, TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase, new()
            where TResult : ResultMessageBase, new();

        TResult Execute<TResult, TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase, new()
            where TResult : ResultMessageBase, new();

        TResult Execute<TResult, TMessage>(string toModule = "")
            where TMessage : IMessageBase, new()
            where TResult : ResultMessageBase, new();

        void Handle<TMessage>() 
            where TMessage : IMessageBase, new();

        void Execute<TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase;

    }
}