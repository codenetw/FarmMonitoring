using MessageBus.MessageBus;

namespace BaseController.CommunicationMessage
{
    /// <summary>
    /// Сообщение отправляется как запрос на проверку состояния процесса
    /// </summary>
    public class CheckProcess : MessageBase
    {
        /// <summary>
        /// Имя проверяемого процесса
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Сообщение отправляется на запуск процесса
    /// </summary>
    public class StartProcess : MessageBase
    {
        /// <summary>
        /// Запускаемый процесс
        /// </summary>
        public ProcessInfo Process { get; set; }
    }

    /// <summary>
    /// Сообщение отправляется на перезапуск процесса
    /// </summary>
    public class RestartProcess : MessageBase
    {
        /// <summary>
        /// Перезапускаемый процесс
        /// </summary>
        public ProcessInfo Process { get; set; }
    }

    /// <summary>
    /// Сообщение отправляется на получения списка процессов со статусом
    /// </summary>
    public class ProcessList : MessageBase{  }

    /// <summary>
    /// Сообщение отправляется завершение процесса
    /// </summary>
    public class KillProcess : MessageBase
    {
        /// <summary>
        /// Имя убиваемого процесса
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Сообщение для ответа для информаировании о стостоянии процесса
    /// </summary>
    public class PrcessStatus : ResultMessageBase
    {
        public enum ProcStatus 
        {
            Running,
            NotRunning,
            NotResponding
        }
        
        /// <summary>
        /// Имя процесса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public ProcStatus StatusProcess { get; set; }

        public bool PrcessName(string name) => string.CompareOrdinal(name, Name) == 0;
        public bool Ready => StatusProcess == ProcStatus.Running;
        public bool ProcessNotRunning => StatusProcess == ProcStatus.NotRunning;
        public bool ProcessNotResponding => StatusProcess == ProcStatus.NotResponding;
        
    }

}
