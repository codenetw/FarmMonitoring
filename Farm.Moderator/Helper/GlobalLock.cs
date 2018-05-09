using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Farm.BaseController;
using Farm.MessageBus.MessageBus;

namespace Farm.Moderator.Helper
{
    static class GlobalLock
    {
        private static ConcurrentDictionary<string, CancellationTokenSource> _tasks = new ConcurrentDictionary<string, CancellationTokenSource>();
        public static void Lock(this ConditionModerator condition, IMessageCommunication bus, TimeSpan timeout)
        {
            var token = _tasks[condition.Name].Token;
            Task.Run(() =>
            {
                condition.Locked = true;
                Task.Delay(timeout, token);
                if (!token.IsCancellationRequested)
                    bus.Handle<ModeratorTimeoutOperation>();
                
                condition.Locked = false;
            }, token);
        }

        public static void UnLock(this ConditionModerator condition)
        {
            _tasks[condition.Name].Cancel();
        }

        public static bool IsLock(this ConditionModerator condition)
        {
            return condition.Locked;
        }

    }
}
