using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessageModel;

namespace Farm.BaseController.CommunicationMessage
{
    [Serializable]
    public class AggregatorState
    {
        public bool IsConnected { get; set; }
        public bool IsApiMinerAvaible { get; set; }
        public bool IsAutobernerStoped { get; set; }
        public IReadOnlyList<CardParam> Cards { get; set; }
        public MinerStatistics MinerApiInfo { get; set; }
    }
}
