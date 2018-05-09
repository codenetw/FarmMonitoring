using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FarmControl.websocket;
using FarmMonitoring.backend.Dashboard;
using log4net;
using WebSocket4Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace FarmMonitoring.websocket
{
    public class SocketServer : ISocketServer
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Configuration _configuration;
        private readonly ILifetimeScope _scope;
        
        private readonly WebSocketServer _serverSocket;
        private static readonly ManualResetEvent _syncEvent = new ManualResetEvent(false);
        private readonly CancellationTokenSource _CancellationToken;

        public SocketServer(Configuration configuration, ILifetimeScope scope)
        {
            _configuration = configuration;
            _scope = scope;
            _serverSocket = new WebSocketServer(configuration.WebSocketAddress);
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
            _serverSocket.AddWebSocketService("/info_cards", () => _scope.Resolve<VideoCardInfoHub>());
            //_logger.Info($"ws hub add -> {nameof(VideoCardInfoHub)} /info_cards");
            //_serverSocket.AddWebSocketService("/autoberner_cards", () => _scope.Resolve<AutobernerHub>());
            //_logger.Info($"ws hub add -> {nameof(AutobernerHub)} /autoberner_cards");
            //_serverSocket.AddWebSocketService("/log", () => _scope.Resolve<MinerHub>());
            //_logger.Info($"ws hub add -> {nameof(MinerHub)} /log");
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