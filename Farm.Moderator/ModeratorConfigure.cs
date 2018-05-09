using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.MessageBus.MessageBus;
using Farm.Moderator.ConditionModels;

namespace Farm.Moderator
{
    [Serializable]
    public class ModeratorConfigure
    {
        public IEnumerable<ConditionModerator> Condtitions { get; set; }
        public TimeSpan CheckInterval { get; set; }
    }

    [Serializable]
    public class ConditionModerator
    {
        [NonSerialized]
        public bool Locked;

        public string Name { get; set; }
        public IEnumerable<IBaseConditionModel> Model { get; set; }
        public IEnumerable<IMessageBase> PositiveExec { get; set; }
        public IEnumerable<IMessageBase> NegativeExec { get; set; }
    }
}
