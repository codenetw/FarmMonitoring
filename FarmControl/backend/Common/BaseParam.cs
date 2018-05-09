using System;
using System.Collections.Generic;
using System.Text;

namespace FarmMonitoring.backend.Common
{
    public abstract class BaseOptionalConfigure
    {
        /// <summary>
        /// true если модуль Включен 
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
