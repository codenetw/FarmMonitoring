using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace FarmMonitoring.backend.Dashboard
{
    public class VideoCardInfoHub : WebSocketBehavior
    {
        private readonly CancellationTokenSource _ctxCancellationToken = new CancellationTokenSource();
        private const string TYPE = "type";
        private const string DETAIL = "detail";
        private const string CARDS = "cards";

        private readonly VideoCardInfo _videoCardInfo;
        private readonly Configuration _configuration;

        public VideoCardInfoHub(Configuration configuration)
        {
            _configuration = configuration;
        }   

        protected override void OnMessage(MessageEventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                do
                { 
                    var @params = e.Data.ParseParameter();
                    var inparam = @params.Where(x => string.CompareOrdinal(x.Key, TYPE) != 0).Select(x => x.Value).ToArray();
                    if (@params.ContainsKey(TYPE) && string.CompareOrdinal(@params[TYPE], DETAIL) == 0)
                    {
                        var result = _videoCardInfo.GetDetails(inparam);
                        var completeResult = result.ConfigureAwait(false).GetAwaiter().GetResult();
                        Sessions.BroadcastAsync(JsonConvert.SerializeObject(completeResult), () => { });
                    }

                    if (@params.ContainsKey(TYPE) && string.CompareOrdinal(@params[TYPE], CARDS) == 0)
                    {
                        // while (Sessions.ActiveIDs.Any())
                        var result = _videoCardInfo.VideoCards(inparam);
                        var completeResult = result.ConfigureAwait(false).GetAwaiter().GetResult();
                        Sessions.BroadcastAsync(JsonConvert.SerializeObject(completeResult), () => { });
                    }
                    base.OnMessage(e);
                 //   await Task.Delay(_configuration.MonitoringInterval, _ctxCancellationToken.Token);
                } while (!_ctxCancellationToken.IsCancellationRequested);

            }, _ctxCancellationToken.Token);
           
        }
    }
}