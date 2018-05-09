using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaseController.CommunicationMessage;
using log4net;
using MessageBus;
using MessageBus.MessageBus;
using Newtonsoft.Json;

namespace MinerController
{
    public sealed class MinerController : IMinerController
    {
        private readonly IMessageCommunication _messageBus;
        private const int TimeoutToDieProcessInSeconds = 15;
        private const int RetryCountConnect = 15;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CancellationTokenSource _token => new CancellationTokenSource();
        private readonly MinerConfigure _config;
        private TcpClient _clientApiMiner;

        public class ResponceSocket
        {
            public int id { get; set; }
            public string error { get; set; }
            public string[] result { get; set; }
        }


        public MinerController(MinerConfigure configuration,
            MessageCommunicationFactory messageCommunicationFactory)
        {
            _messageBus = messageCommunicationFactory.Create();
            _config = configuration;
        }

        private async Task<NetworkStream> ConnectApiMiner()
        {
            try
            {
                _clientApiMiner = new TcpClient();
                await _clientApiMiner.ConnectAsync(_config.IP, _config.Port);
                return _clientApiMiner.GetStream();
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                _logger.Error("can't connect to api minerController");
                return null;
            }
        }

        private static async Task SendApiCommand<T>(Stream stream,T obj)
        {
            var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private static async Task<ResponceSocket> ReadApiResponce(NetworkStream stream)
        {
            var response = new StringBuilder();
            var data = new byte[512];
            do
            {
                var bytes = await stream.ReadAsync(data, 0, data.Length);
                response.Append(Encoding.UTF8.GetString(data, 0, bytes));
            } while (stream.DataAvailable);

            var resp = response.ToString();
            if (resp != "{}")
                return JsonConvert.DeserializeObject<ResponceSocket>(resp);

            _logger.Warn("password api minerController is incorrect");
            return null;
        }

        public void ListenMinerApi()
        {
            _logger.Info("CmdMiner [api]");
            Task.Factory.StartNew(async () =>
            {
                do
                {
                    var retryCountConnect = RetryCountConnect;
                    var networkStream = default(NetworkStream);

                    while (retryCountConnect != 0)
                    {
                        networkStream = await ConnectApiMiner();
                        if (networkStream != null)
                            break;
                        retryCountConnect--;
                    }
                    _logger.Info("connect to api minerController success!");

                    if (retryCountConnect == 0)
                    {
                        _logger.Info("reboot minerController init >");
                        var procResultInfo = default(PrcessStatus);
                        do
                        {
                            procResultInfo =
                                _messageBus.Execute<PrcessStatus, RestartProcess, ICommunicationBase>(
                                    new RestartProcess {Process = _config.DependencyProcess});
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        } while (!procResultInfo.Ready);
                        continue;
                    }

                    if(networkStream == null)
                        continue;
                   
                        try
                        {
                            do
                            {
                                if (!_clientApiMiner.Connected)
                                    networkStream = await ConnectApiMiner();
                                await SendApiCommand(networkStream, MinerRequest.GetStatistics(_config.Password));
                                var result = await ReadApiResponce(networkStream);
                                if (result == null)
                                {
                                    _logger.Fatal("Change password api and restart app");
                                    return;
                                }

                                var resultData = ParseResult(result.result);
                                _messageBus.Publish(resultData);
                                var originalColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"Miner working {TimeSpan.FromMinutes(resultData.RuntimeMinutes)}");
                                Console.WriteLine($"Total ETH:{resultData.TotalMainShare} Mh/s / Total shares:{resultData.NumberMainShare} Rejected shares:{resultData.NumberRejectedMainShare}");
                                Console.WriteLine(string.Join(" ,",resultData.HashMainRateCards.Select( x => $"{x} Mh/s")));
                                Console.WriteLine($"Total Dual currency:{resultData.TotalSecondShare} Mh/s / Total shares:{resultData.NumberSecondShare} Rejected shares:{resultData.NumberRejectedSecondShare}");
                                Console.WriteLine(string.Join(" ,", resultData.HashSecondRateCards.Select(x => $"{x} Mh/s")));
                                Console.WriteLine(string.Join(" ,", resultData.FanSpeedAndTemp.Select((x, i) => $"Card{i} {x.Fanspeed}%:{x.Temperature}C")));
                                Console.ForegroundColor = originalColor;
                                _clientApiMiner.Close();
                                await Task.Delay(_config.IntervalCheck);
                            } while (!_token.IsCancellationRequested);
                        }
                        catch (Exception ex)
                        {
                           _logger.Error(ex);
                        }
                    
                    await Task.Delay(_config.IntervalCheck);
                } while (!_token.IsCancellationRequested);
            }, _token.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private MinerStatistics ParseResult(IReadOnlyList<string> parameters)
        {
            var splitinfo = string.Join(string.Empty, parameters[6].Split(';')
                .Select((x, i) => i > 0 && i % 2 == 0 ? $"-{x}" : $";{x}"));

            return new MinerStatistics
            {
                Version = parameters[0],
                RuntimeMinutes = long.Parse(parameters[1]),
                TotalMainShare = decimal.Parse(parameters[2].Split(';')[0]) / 1000,
                NumberMainShare = decimal.Parse(parameters[2].Split(';')[1]),
                NumberRejectedMainShare = decimal.Parse(parameters[2].Split(';')[2]),
                HashMainRateCards = parameters[3].Split(';').Select(x => (decimal.Parse(x) / 1000).ToString(CultureInfo.InvariantCulture)).ToArray(),
                TotalSecondShare = decimal.Parse(parameters[4].Split(';')[0]) /1000,
                NumberSecondShare = decimal.Parse(parameters[4].Split(';')[1]),
                NumberRejectedSecondShare = decimal.Parse(parameters[4].Split(';')[2]),
                HashSecondRateCards = parameters[5].Split(';').Select(x => (decimal.Parse(x) / 1000).ToString(CultureInfo.InvariantCulture)).ToArray(),
                FanSpeedAndTemp = splitinfo.Split('-').Select(x =>
                {
                    var spl = x.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
                    return new TempAndFan {Temperature = int.Parse(spl[0]), Fanspeed = int.Parse(spl[1])};
                }).ToArray()
            };
        }
        
        public void Start()
        {
            var result = _messageBus.Execute<PrcessStatus, CheckProcess, ICommunicationBase>(new CheckProcess { Name = _config.DependencyProcess.Name });
            if (!result.Ready)
                _messageBus.Execute<StartProcess, ICommunicationBase>(new StartProcess { Process = _config.DependencyProcess });
            _clientApiMiner = new TcpClient();
            ListenMinerApi();
        }

        public void Stop()
        {
            _token.Cancel();
        }
    }
}



