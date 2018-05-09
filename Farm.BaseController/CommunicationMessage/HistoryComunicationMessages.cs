using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.MessageBus.MessageBus;

namespace Farm.BaseController.CommunicationMessage
{
    public class HistoryCardMessages : MessageBase
    {
        public int Loss { get; set; }
        public int MainCard { get; set; }
        public int MineFailure { get; set; }
        public int BladeCard { get; set; }
        public int ResetFrequencies { get; set; }
    }
}
