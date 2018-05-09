using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;

namespace Farm.Moderator
{
    public interface IModerator
    {
        void Start();
        void Stop();
        void UpdateState(AggregatorState currentState);
    }
}
