using System.Collections.Generic;
using MessageBus.MessageBus;

namespace BaseController.CommunicationMessage
{
    /// <summary>
    /// Сообщение отправляется с актуальный на данный момент информацией о картах
    /// </summary>
    public class AutobernerInformationMessage : MessageBase
    {
        public List<CardParam> CurrentInfoCards { get; set; }
    }

    /// <summary>
    /// Сообщение отправляется когда WatchDog сбросил настройки автобернера
    /// </summary>
    public class AutobernerWatchdogResetMessage : MessageBase { }

    /// <summary>
    /// Сообщение отправляется когда WatchDog перестал работать
    /// </summary>
    public class AutobernerWatchdogStopedMessage : MessageBase { }

    /// <summary>
    /// Сообщение отправляется когда опрос информации о картах оставновлен
    /// </summary>
    public class AutobernerStopWatchingMessage : MessageBase { }
}
