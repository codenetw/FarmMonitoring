using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Farm.BaseController;
using Farm.BaseController.CommunicationMessage;
using Farm.BaseController.CustomError;
using Farm.MessageBus;
using Farm.MessageBus.MessageBus;
using log4net;
using Newtonsoft.Json;

namespace Farm.MinerController
{
    public sealed class MinerController : ICommunicationBase, IMinerController
    {
        private const int TimeoutToDieProcessInSeconds = 15;
        private const int RetryCountConnect = 15;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CancellationTokenSource _token => new CancellationTokenSource();

        public string Name => "miner";

        private readonly MessageCommunicationFactory _messageCommunicationFactory;

        private readonly MinerConfigure _config;
       

        public class ResponceSocket
        {
            public int id { get; set; }
            public string error { get; set; }
            public string[] result { get; set; }
        }
        
        public MinerController(MinerConfigure configuration, MessageCommunicationFactory messageCommunicationFactory)
        {
            _config = configuration;
            _messageCommunicationFactory = messageCommunicationFactory;
        }
        

        #region listener message

        public async void Handle(ProcessStatus message)
        {
            if (message.InitModule && message.IsName(_config.DependencyProcess.Name) && message.Ready)
            {
                await ReadStatistic();
            }
        }
    

        public ModeratorResultStatus Execute(RestartApiMiner message)
        {
            try
            {
                using (var stream = ConnectApiMiner().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    SendApiCommand(stream, MinerRequest.RebootMiner(_config.Password)).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                return ModeratorResultStatus.Ok;
            }
            catch (FarmException ex)
            {
                return ModeratorResultStatus.FromException(ex);
            }
        }

        public ModeratorResultStatus Execute(CardChangeStatus message)
        {
            try
            {
                using (var stream = ConnectApiMiner().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    SendApiCommand(stream, MinerRequest.ControlCard(_config.Password, (GPUCommand)message.State, message.IdCard)).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                return ModeratorResultStatus.Ok;
            }
            catch (FarmException ex)
            {
                return ModeratorResultStatus.FromException(ex);
            }
        }

        #endregion

        private async Task ReadStatistic()
        {
            await Task.Factory.StartNew(async () => await ListenStatisticsMiner()
                ,_token.Token
                , TaskCreationOptions.LongRunning
                , TaskScheduler.Current);
        }

        private async Task<NetworkStream> ConnectApiMiner()
        {
            var messageBus = _messageCommunicationFactory.Create();
            try
            {
                var clientApiMiner = new TcpClient();
                await clientApiMiner.ConnectAsync(_config.IP, _config.Port);
                var stream = clientApiMiner.GetStream();
                messageBus.Handle(new MinerApiStatus { Avaible = true });
                return stream;
            }
            catch(Exception ex)
            {
                messageBus.Handle(new MinerApiStatus { Avaible = false });
                _logger.Error("can't connect to api minerController");
                throw new FarmException(ex, ErrorCode.ConnectionError);
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

        public async Task ListenStatisticsMiner()
        {
            var messageBus = _messageCommunicationFactory.Create();
            var stream = await ConnectApiMiner();

            if (stream == null)
            {
                messageBus.Handle(new MinerApiStatus {Avaible = false});
                return;
            }
            do
            {
                using (var networkStream = await ConnectApiMiner())
                {
                    try
                    {
                        await SendApiCommand(networkStream, MinerRequest.GetStatistics(_config.Password));
                        var result = await ReadApiResponce(networkStream);
                        var resultData = ParseResult(result?.result);
                        if (resultData != null)
                        {
                            messageBus.Handle(resultData);
                            OutConsoleData(resultData);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        messageBus.Handle(new MinerApiStatus { Avaible = false });
                        return;
                    }
                }
                await Task.Delay(_config.IntervalCheck);
            } while (!_token.IsCancellationRequested);

        }

        private static void OutConsoleData(MinerStatistics resultData)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Miner working {TimeSpan.FromMinutes(resultData.RuntimeMinutes)}");
            Console.WriteLine($"Total ETH:{resultData.TotalMainShare} Mh/s / Total shares:{resultData.NumberMainShare} Rejected shares:{resultData.NumberRejectedMainShare}");
            Console.WriteLine(string.Join(" ,", resultData.HashMainRateCards.Select(x => $"{x} Mh/s")));
            Console.WriteLine($"Total Dual currency:{resultData.TotalSecondShare} Mh/s / Total shares:{resultData.NumberSecondShare} Rejected shares:{resultData.NumberRejectedSecondShare}");
            Console.WriteLine(string.Join(" ,", resultData.HashSecondRateCards.Select(x => $"{x} Mh/s")));
            Console.WriteLine(string.Join(" ,", resultData.FanSpeedAndTemp.Select((x, i) => $"Card{i} {x.Fanspeed}%:{x.Temperature}C")));
            Console.ForegroundColor = originalColor;
        }

        private static MinerStatistics ParseResult(IReadOnlyList<string> parameters)
        {
            if (parameters == null)
                return null;

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
                HashSecondRateCards = parameters[5].Split(';').Where(x=>x!= "off").Select(x => (decimal.Parse(x) / 1000).ToString(CultureInfo.InvariantCulture)).ToArray(),
                FanSpeedAndTemp = splitinfo.Split('-').Select(x =>
                {
                    var spl = x.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
                    return new TempAndFan {Temperature = int.Parse(spl[0]), Fanspeed = int.Parse(spl[1])};
                }).ToArray()
            };
        }
        
        public void Start()
        {
            ReadStatistic();
            var messageBus = _messageCommunicationFactory.Create();
            messageBus.Handle(new CheckProcess { Name = _config.DependencyProcess.Name, IsInitModule = true }, "process");
        }

        public void Stop()
        {
            _token.Cancel();
        }
    }
}



