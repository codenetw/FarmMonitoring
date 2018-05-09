using System;
using System.Collections.Generic;
using Farm.MessageBus.MessageBus;

namespace Farm.BaseController.CommunicationMessage
{
    public class MinerApiStatus : MessageBase
    {
        public bool Avaible { get; set; }
    }
    public class CheckApiStatus : MessageBase { }
    public class RestartApiMiner : MessageBase { }
    public class CardChangeStatus : MessageBase
    {
        public int IdCard { get; set; }
        public StateGpu State { get; set; }
        public enum StateGpu
        {
            Disable = 0,
            MainOnly = 1,
            DualMode = 2
        }
    }

    [Serializable]
    public class MinerStatistics : MessageBase
    {
        public string Version { get; set; }
        public long RuntimeMinutes { get; set; }
        public decimal TotalMainShare { get; set; }
        public decimal NumberMainShare { get; set; }
        public decimal NumberRejectedMainShare { get; set; }
        public string[] HashMainRateCards { get; set; }
        public decimal TotalSecondShare { get; set; }
        public decimal NumberSecondShare { get; set; }
        public decimal NumberRejectedSecondShare { get; set; }
        public string[] HashSecondRateCards { get; set; }
        public TempAndFan[] FanSpeedAndTemp { get; set; }
    }
    [Serializable]
    public class TempAndFan
    {
        public int Temperature { get; set; }
        public int Fanspeed { get; set; }
    }
    public class MinerLog : MessageBase
    {
        public string Log { get; set; }
    }
}
