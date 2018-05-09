using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.HistoryController
{
    public class Config
    {
        public List<Card> cards { get; set; }
    }

    public class Card
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int Loss { get; set; }
        public int MainCard { get; set; }
        public int MineFailure { get; set; }
        public int BladeCard { get; set; }
        public int ResetFrequencies { get; set; }
    }

}
