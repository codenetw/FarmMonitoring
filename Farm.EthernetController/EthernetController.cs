using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.MessageBus;
using Farm.MessageBus.MessageBus;
using log4net;

namespace Farm.EthernetController
{
    public class EthernetController : ICommunicationBase, IEthernetController
    {
        private readonly MessageCommunicationFactory _messageCommunicationFactory;
        private readonly EthernetConfigure _config;
        private static CancellationTokenSource _token => new CancellationTokenSource();

        public string Name => "ethernet";

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EthernetController(EthernetConfigure configuration, MessageCommunicationFactory messageCommunicationFactory)
        {
            _messageCommunicationFactory = messageCommunicationFactory;
            _config = configuration;
        }

        public void Start()
        {
            CheckConnection();
        }
        public void Stop()
        {
            _token.Cancel();
        }

        #region listener message

        public void Handle(ResetAdapterMessage resetAdapter)
        {
            if (_config.ReadOnlyAdapter)
            {
                _logger.Warn("Ethernet adapter readonly");
                return;
            }
            DisableAdapters(_config.AdapterList);
            EnableAdapters(_config.AdapterList);
        }

        #endregion

        private void CheckConnection()
        {
            Task.Factory.StartNew(async () =>
                {
                    var bus = _messageCommunicationFactory.Create();
                    do
                    {
                        if (!PingHost(_config.AddressCheck))
                        {
                            bus.Handle<ConnectionLost>();
                            _logger.Warn($"Connection lost to {_config.AddressCheck}");
                            if (_config.ReadOnlyAdapter)
                            {
                                if (_config.RestoreTool != null)
                                    bus.Execute(new StartProcess { Process = _config.RestoreTool });
                                else
                                {
                                    DisableAdapters(_config.AdapterList);
                                    await Task.Delay(_config.ResetDelay);
                                    EnableAdapters(_config.AdapterList);
                                }
                            }
                        }
                        else
                        {
                            bus.Handle<ConnectionOk>();
                        }
                        await Task.Delay(_config.IntervalCheck);
                    } while (!_token.IsCancellationRequested);
                },
                _token.Token);
        }

        private void EnableAdapters(params string[] adapters)
        {
            var bus = _messageCommunicationFactory.Create();
            AdaptersControl("Enable", adapters);
            bus.Handle(new NetworkAdapter{Adapter = NetworkAdapter.AdapterStatus.Start});
        }

        private void DisableAdapters(params string[] adapters)
        {
            var bus = _messageCommunicationFactory.Create();
            AdaptersControl("Disable", adapters);
            bus.Handle(new NetworkAdapter { Adapter = NetworkAdapter.AdapterStatus.Stop });
        }

        private static void AdaptersControl(string function, params string[] adapters)
        {
            var wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
            var searchProcedure = new ManagementObjectSearcher(wmiQuery);
            foreach (var o in searchProcedure.Get())
            {
                var item = (ManagementObject)o;

                if(adapters.Any() || adapters.Contains(item["NetConnectionId"]))
                    item.InvokeMethod(function, null);
            }
        }

        private bool PingHost(string nameOrAddress)
        {
            var xRet = _config.RetryCount;
            var pinger = new Ping();
            do
            {
                try
                {
                    var reply = pinger.Send(nameOrAddress);
                    var pingable = reply?.Status == IPStatus.Success;
                    if (pingable)
                    {
                        return true;
                    }
                }
                catch (PingException)
                {
                    _logger.Debug($"Try ping  {nameOrAddress} > fail");
                }
            } while (xRet-- > 0);
         return false;
        }

    }
}
