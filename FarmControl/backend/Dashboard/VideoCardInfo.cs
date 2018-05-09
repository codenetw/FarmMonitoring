using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FarmMonitoring.backend.Dashboard
{
    public class VideoCardInfo 
    {
        private readonly Configuration _configuration;
        private readonly VideoControllerWMI _videoCardInfoWmi;

        //public VideoCardInfo(Configuration configuration, VideoControllerWMI videoCardInfoWMI)
        //{
        //    _configuration = configuration;
        //    _videoCardInfoWmi = videoCardInfoWMI;
        //    _videoCardInfoWmi = videoCardInfoWMI ?? throw new ArgumentNullException($"{nameof(videoCardInfoWMI)} must be define");
        //}

        public async Task<object> GetDetails(params string[] query)
        {
            return await AggregateResult(query);
        }

        public async Task<object> VideoCards(params string[] query)
        {
            return await AggregateResult(query);
        }

        private async Task<object> AggregateResult(params string[] query)
        {
            var results = new List<Task<object>>(0);

            await _videoCardInfoWmi.Execute(CancellationToken.None, query);
            await Task.WhenAll(results);
            return await Task.FromResult(results);
        }

    }
}
