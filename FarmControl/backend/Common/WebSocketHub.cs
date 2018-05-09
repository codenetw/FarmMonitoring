using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FarmControl;
using log4net;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace FarmMonitoring.backend.Common
{
    public class WebSocketHub<T> : WebSocketBehavior
    {
        protected static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected readonly CancellationTokenSource _ctxCancellationToken = new CancellationTokenSource();
        protected readonly Configuration _configuration;
        private readonly string _hubName;
        private static readonly SpecificConcurrentDictionary<WebSocketSessionManager> ConcurrentSession = new SpecificConcurrentDictionary<WebSocketSessionManager>();
        
        protected WebSocketHub(Configuration configuration)
        {
            _configuration = configuration;
            _hubName = typeof(T).Name;
        }

        protected override void OnError(ErrorEventArgs e)
        {
            ConcurrentSession.Remove(GetUniqCurrentSession);
            _logger.Error($"{GetUniqCurrentSession} with error: {e.Exception.Message}");
            base.OnError(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            ConcurrentSession.Remove(GetUniqCurrentSession);
            _logger.Info($"{GetUniqCurrentSession} closed with reason: {e.Reason}");
            base.OnClose(e);
        }

        protected override void OnOpen()
        {
            ConcurrentSession.Add(GetUniqCurrentSession, Sessions);
            _logger.Info($"{GetUniqCurrentSession} opened");
            base.OnOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            _logger.Info($"{GetUniqCurrentSession} receive message");
            base.OnMessage(e);
        }

        protected void BroadcastMessage(object data)
        {
            var objSeria = JsonConvert.SerializeObject(data);
            var activeSession = ConcurrentSession[GetUniqCurrentSession].FirstOrDefault();
            activeSession?.BroadcastAsync(objSeria, () =>
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug($"{GetUniqCurrentSession}> broadcast compleated: {objSeria}");
            });
        }

        protected void SendToAsync(string session, object data)
        {
            var objSeria = JsonConvert.SerializeObject(data);
            var activeSession = ConcurrentSession[GetUniqCurrentSession].FirstOrDefault();
            var dotIndex = session.IndexOf('.');
            if (dotIndex != -1)
                session = session.Skip(dotIndex).ToString();
            if(string.IsNullOrWhiteSpace(session))
                return;

            activeSession.SendToAsync(objSeria, session, null);
        }
        
        private string GetUniqCurrentSession => $"{_hubName}.{ID}";

    }
}
