using System;

namespace Farm.MessageBus.MessageBus
{
    [Serializable]
    public abstract class MessageBase : IMessageBase
    {
        public MessageStatus Status { get; set; }
    }
}