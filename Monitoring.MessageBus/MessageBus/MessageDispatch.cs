using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MessageBus.MessageBus
{
    internal sealed class MessageDispatch : IMessageCommunication
    {
        private readonly IEnumerable<ICommunicationBase> _comunications;
        private readonly ConcurrentDictionary<string, KeyValuePair<ICommunicationBase, MethodInfo>> _cache;

        public MessageDispatch(IEnumerable<ICommunicationBase> comunications)
        {
            _cache = new ConcurrentDictionary<string, KeyValuePair<ICommunicationBase, MethodInfo>>();
            _comunications = comunications;
        }

        public void Publish<TMessage>(TMessage message)
            where TMessage: MessageBase
        {
            var type = typeof(TMessage);
            Action<ICommunicationBase> handle = (x) =>
            {
                var @class = x.GetType().Name;
                if (!_cache.ContainsKey($"{nameof(Publish)}.{@class}.{type.Name}"))
                {
                    var method = x.GetType().GetMethod(nameof(Publish), new Type[] { typeof(TMessage) });
                    if (method != null)
                        _cache[$"{nameof(Publish)}.{@class}.{type.Name}"] = new KeyValuePair<ICommunicationBase, MethodInfo>(x, method);
                    else
                        return;
                }
                var cacheFunc = _cache[$"{nameof(Publish)}.{@class}.{type.Name}"];
                cacheFunc.Value.Invoke(cacheFunc.Key, new object[] { message });
            };
            if (_comunications.Count() > 2)
                Parallel.ForEach(_comunications, handle);
            else
            {
                foreach (var elem in _comunications)
                    handle(elem);                
            }
        }

        public IEnumerable<TResult> Query<TResult, TMessage>(TMessage message)
            where TMessage : MessageBase
            where TResult : ResultMessageBase
        {
            var type = typeof(TMessage);
            var result = new BlockingCollection<TResult>();

            void Action(ICommunicationBase x)
            {
                var @class = x.GetType().Name;
                if (!_cache.ContainsKey($"{nameof(Query)}.{@class}.{type.Name}"))
                {
                    var method = x.GetType().GetMethod(nameof(Query), new Type[] {typeof(TMessage)});
                    if (method != null && method.ReturnParameter.ParameterType == typeof(TResult))
                        _cache[$"{nameof(Query)}.{@class}.{type.Name}"] = new KeyValuePair<ICommunicationBase, MethodInfo>(x, method);
                    else
                        return;
                }

                var cacheFunc = _cache[$"{nameof(Query)}.{@class}.{type.Name}"];
                result.Add((TResult) cacheFunc.Value.Invoke(cacheFunc.Key, new object[] {message}));
            }

            if (_comunications.Count() > 2)
                Parallel.ForEach(_comunications, (Action<ICommunicationBase>) Action);
            else
            {
                foreach (var elem in _comunications)
                    Action(elem);
            }

            return result.ToList();
        }

        public TResult Execute<TResult, TMessage, TModule>(TMessage message)
            where TMessage : MessageBase
            where TResult : ResultMessageBase
            where TModule : ICommunicationBase
        {
            var type = typeof(TMessage);
            var result = default(TResult);

            var module = _comunications.OfType<TModule>().SingleOrDefault();
            if(module == null)
                throw new Exception("ambiguity call detect");

            var @class = module.GetType().Name;
            var key = $"{nameof(Execute)}.{@class}.{type.Name}";
            if (!_cache.ContainsKey(key))
            {
                var method = module.GetType().GetMethod(nameof(Execute), new Type[] { typeof(TMessage) });
                if (method != null && method.ReturnParameter.ParameterType == typeof(TResult))
                    _cache[key] = new KeyValuePair<ICommunicationBase, MethodInfo>(module, method);
                else
                    return default(TResult);
            }
            var cacheFunc = _cache[key];
            result = (TResult)cacheFunc.Value.Invoke(cacheFunc.Key, new object[] { message });
            return result;
        }

        public void Execute<TMessage, TModule>(TMessage message)
            where TMessage : MessageBase
            where TModule : ICommunicationBase
        {
            var type = typeof(TMessage);

            var module = _comunications.OfType<TModule>().SingleOrDefault();
            if (module == null)
                throw new Exception("ambiguity call detect");

            var @class = module.GetType().Name;
            var key = $"{nameof(Execute)}.{@class}.{type.Name}";
            if (!_cache.ContainsKey(key))
            {
                var method = module.GetType().GetMethod(nameof(Execute), new Type[] { typeof(TMessage) });
                if (method != null)
                    _cache[key] = new KeyValuePair<ICommunicationBase, MethodInfo>(module, method);
            
            }
            var cacheFunc = _cache[key];
            cacheFunc.Value.Invoke(cacheFunc.Key, new object[] { message });
        }

        public void Publish<TMessage>() where TMessage : MessageBase, new()
        {
            Publish(new TMessage());
        }
    }
}
