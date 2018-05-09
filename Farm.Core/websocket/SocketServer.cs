using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using log4net;
using WebSocket4Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Farm.Core.websocket
{
    public class SocketServer : ISocketServer
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ILifetimeScope _scope;
        
        private readonly WebSocketServer _serverSocket;
        private static readonly ManualResetEvent _syncEvent = new ManualResetEvent(false);
        private readonly CancellationTokenSource _CancellationToken;

        public SocketServer(string address, ILifetimeScope scope)
        {
            _scope = scope;
            _serverSocket = new WebSocketServer(address);
            _CancellationToken = new CancellationTokenSource();
        }

        public async Task Start()
        {
            InitSocket();
            _serverSocket.Start();
            await Task.CompletedTask;
        }

        public async Task Stop()
        {
            _serverSocket.Stop(CloseStatusCode.Normal, "core stop");
            _CancellationToken.Cancel();
            await Task.CompletedTask;
        }

        private void InitSocket()
        {
            _serverSocket.AddWebSocketService("/aggregator", () => _scope.Resolve<Aggregator>());
        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"message recived {e.Message}");
            _syncEvent.Set();
        }

        private void websocket_Closed(object sender, EventArgs e)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"socket closed");
            _syncEvent.Reset();
        }

        private void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"socket error {e.Exception.Message}");
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"socket opened");
        }
    }
}