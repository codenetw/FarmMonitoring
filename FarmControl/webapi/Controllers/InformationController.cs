using System;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;

namespace FarmMonitoring.webapi.Controllers
{
    public sealed class InformationController : NancyModule
    {
        private readonly Configuration _configuration;

        public InformationController(Configuration configuration)
        {
            _configuration =
                configuration ?? throw new ArgumentNullException($"{nameof(configuration)} must be define");

            Get("/", x => AsyncGet());
            Get("/time", x => AsyncGetTime());
        }

        private async Task<Negotiator> AsyncGet()
        {
            await Task.CompletedTask;
            var model = new {ServerTime = DateTime.Now.ToString("T")};
            return View["CardInfo", model];
        }

        private async Task<DateTime> AsyncGetTime()
        {
            return await Task.FromResult(DateTime.Now);
        }

        
    }
}
