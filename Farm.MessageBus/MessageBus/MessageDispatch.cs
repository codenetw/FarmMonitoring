using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Farm.MessageBus.MessageBus
{
    internal sealed class MessageDispatch : IMessageCommunication
    {
        private readonly IEnumerable<ICommunicationBase> _comunications;
        private readonly bool _aggregatorTranslate = true;
        private readonly ConcurrentDictionary<string, KeyValuePair<ICommunicationBase, MethodInfo>> _cache;
        private readonly BlockingCollection<string> _notFoundCache;
        public MessageDispatch(IEnumerable<ICommunicationBase> comunications)
        {
            _cache = new ConcurrentDictionary<string, KeyValuePair<ICommunicationBase, MethodInfo>>();
            _notFoundCache = new BlockingCollection<string>();
            _comunications = comunications;
        }
        
        public void Handle<TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase
        {
            var messageType = typeof(TMessage);
            IEnumerable<(MethodInfo method, ICommunicationBase @class)> functions = GetMethodsInner(nameof(Handle), toModule, messageType, typeof(void)).ToList();

            GuardFunc(functions);

            foreach (var (method, @class) in functions.AsParallel().WithExecutionMode(ParallelExecutionMode.Default))
            {
                var Params = method.GetParameters().Length > 1 ? new object[] {message, messageType.Name } : new object[] { message };
                method.Invoke(@class, Params);
            }
        }

        public IEnumerable<TResult> Query<TResult, TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase, new()
            where TResult : ResultMessageBase, new()
        {
            var result = new BlockingCollection<TResult>();

            IEnumerable<(MethodInfo method, ICommunicationBase @class)> functions = GetMethodsInner(nameof(Execute), toModule, typeof(TMessage), typeof(TResult)).ToList();

            GuardFunc(functions);

            foreach (var (method, @class) in functions.AsParallel().WithExecutionMode(ParallelExecutionMode.Default))
                result.Add((TResult)method.Invoke(@class, new object[] { message }));

            return result.ToList();
        }

        public TResult Execute<TResult, TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase, new()
            where TResult : ResultMessageBase, new()
        {
            var functions = GetMethodsInner(nameof(Execute), toModule, typeof(TMessage), typeof(TResult)).ToList();

            GuardFunc(functions);

            (MethodInfo method, ICommunicationBase @class) = functions.Single();
            
            return (TResult)method.Invoke(@class, new object[] { message });
        }

        public TResult Execute<TResult, TMessage>(string toModule = "") 
            where TMessage : IMessageBase, new()
            where TResult : ResultMessageBase, new()
        {
            return Execute<TResult, TMessage>(new TMessage(), toModule);
        }

        public void Handle<TMessage>()
            where TMessage : IMessageBase, new()
        {
            Handle(new TMessage());
        }

        public void Execute<TMessage>(TMessage message, string toModule = "")
            where TMessage : IMessageBase
        {
            var functions = GetMethodsInner(nameof(Execute), toModule, typeof(TMessage), typeof(void));

            GuardFunc(functions);

            (MethodInfo method, ICommunicationBase @class) = functions.Single();
            
            method.Invoke(@class, new object[] { message });
        }

        private bool InModule(string funcName, string name, string modules) =>
                string.IsNullOrWhiteSpace(modules) 
            || (modules.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(name))
            || (_aggregatorTranslate && new[]{ nameof(Handle) }.Contains(funcName));

        private void GuardFunc<T>(IEnumerable<T> functions)
        {
            if (!functions.Any())
                throw new NotImplementedException($"function not found");
        }

        private IEnumerable<(MethodInfo, ICommunicationBase)> GetMethodsInner(string funcName, string toModule, Type messageType, Type resultType)
        {
            var modules = _comunications.Where(x => InModule(funcName, x.Name, toModule));
                   
            foreach (var module in modules)
            {
                var @class = module.GetType().Name;
                var key = $"{funcName}.{@class}.{messageType.Name}";

                if (_notFoundCache.Contains(key))
                    continue;
                
                if (!_cache.ContainsKey(key))
                {
                    var method = module.GetType().GetMethod(funcName, new[] { messageType, typeof(string) }) 
                                 ?? module.GetType().GetMethod(funcName, new [] { messageType });
                    if (method?.ReturnParameter != null && (method.ReturnParameter.ParameterType == resultType))
                    {
                        _cache[key] = new KeyValuePair<ICommunicationBase, MethodInfo>(module, method);
                    }
                    else
                    {
                        _notFoundCache.Add(key);
                        continue;
                    }
                    
                }
                
                yield return (_cache[key].Value, _cache[key].Key);
            }
        }
    }
}
