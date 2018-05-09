using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BaseController;
using BaseController.CommunicationMessage;
using log4net;
using MessageBus;
using MessageBus.MessageBus;

namespace EthernetController
{
    public class EthernetController : IEthernetController
    {
        private readonly MessageCommunicationFactory _messageCommunicationFactory;
        private readonly EthernetConfigure _config;
        private static CancellationTokenSource _token => new CancellationTokenSource();
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EthernetController(EthernetConfigure configuration, MessageCommunicationFactory messageCommunicationFactory)
        {
            _messageCommunicationFactory = messageCommunicationFactory;
            _config = configuration;
        }

        public void Start()
        {
            Task.Factory.StartNew(async () =>
                {
                    var bus = _messageCommunicationFactory.Create();
                    do
                    {
                        if (!PingHost(_config.AddressCheck))
                        {
                            bus.Publish<ConnectionLost>();
                            _logger.Warn($"Connection lost to {_config.AddressCheck}");
                            if (_config.ResetIfLost)
                            {
                                if(_config.RestoreTool != null)
                                    bus.Execute<StartProcess, ICommunicationBase>(new StartProcess{Process = _config.RestoreTool });
                                else
                                {
                                    DisableAdapters(_config.AdapterList);
                                    await Task.Delay(_config.ResetDelay);
                                    EnableAdapters(_config.AdapterList);
                                }
                            }

                        }
                        await Task.Delay(_config.IntervalCheck);
                    } while (!_token.IsCancellationRequested);
                },
                _token.Token);
        }

        private void EnableAdapters(params string[] adapters)
        {
            var bus = _messageCommunicationFactory.Create();
            AdaptersControl("Enable");
            bus.Publish(new NetworkAdapter{Adapter = NetworkAdapter.AdapterStatus.Start});
        }

        private void DisableAdapters(params string[] adapters)
        {
            var bus = _messageCommunicationFactory.Create();
            AdaptersControl("Disable");
            bus.Publish(new NetworkAdapter { Adapter = NetworkAdapter.AdapterStatus.Stop });
        }

        private void AdaptersControl(string function, params string[] adapters)
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

        public bool PingHost(string nameOrAddress)
        {
            var xRet = _config.RetryCount;
            var pinger = new Ping();
            do
            {
                try
                {
                    var reply = pinger.Send(nameOrAddress);
                    var pingable = reply.Status == IPStatus.Success;
                    if (pingable == true)
                        return true;
                }
                catch (PingException)
                {
                    _logger.Debug($"Try ping  {nameOrAddress} > fail");
                }
            } while (xRet-- > 0);
         return false;
        }

        public void Stop()
        { }
    }
}
