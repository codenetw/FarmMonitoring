using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.BaseController.CommunicationMessageModel;

namespace Farm.Moderator
{
    public static class Extension
    {
        public static object Card(this AggregatorState ext,int id, Func<CardParam, object> val)
        {
            var card = ext?.Cards.FirstOrDefault(x => x.Id == id);
            return card == null ? null : val(card);
        }

        public static object Miner(this AggregatorState ext, Func<MinerStatistics, object> val)
        {
            return ext == null ?  null : val(ext.MinerApiInfo);
        }
    }
}
