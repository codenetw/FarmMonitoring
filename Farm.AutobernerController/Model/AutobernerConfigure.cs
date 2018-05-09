using System;
using System.Collections.Generic;
using Farm.BaseController.CommunicationMessage;

namespace Farm.AutobernerController.Model
{
    /// <summary>
    /// Параметры
    /// </summary>
    public class AutobernerConfigure 
    {
        /// <summary>
        /// Вкл \ выкл WatchDog
        /// </summary>
        public bool ReadOnlyCards { get; set; }
        
        /// <summary>
        /// Интервал опроса карт
        /// </summary>
        public TimeSpan MonitoringInterval { get; set; }

        /// <summary>
        /// Процесс который необходимо контролировать для работы модуля
        /// </summary>
        public ProcessInfo DependencyProcess { get; set; }
    }

}