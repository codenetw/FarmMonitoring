using System;
using System.Collections.Generic;
using BaseController.CommunicationMessage;

namespace AutobernerController.Model
{
    /// <summary>
    /// Параметры
    /// </summary>
    public class AutobernerConfigure 
    {
        /// <summary>
        /// Вкл \ выкл WatchDog
        /// </summary>
        public bool WatchDog { get; set; }

        /// <summary>
        /// Интервал опроса карт
        /// </summary>
        public TimeSpan MonitoringInterval { get; set; }

        /// <summary>
        /// Настройки карт для WatchDog, в случае сбоя WatchDog сбросит на указанные настрйоки конкретной карты
        /// </summary>
        public List<CardParam> ActualConfigCards { get; set; }

        /// <summary>
        /// Процесс который необходимо контролировать для работы модуля
        /// </summary>
        public ProcessInfo DependencyProcess { get; set; }
    }

}