using System;

namespace MessageBus.MessageBus
{
    public abstract class MessageBase : IMessageBase
    {
        public MessageStatus Status { get; set; }
    }
}