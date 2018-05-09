using System;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses.Negotiation;

namespace Farm.Core.webapi.Controllers
{
    public class DashBoardController : NancyModule
    {

        public DashBoardController()
        {
            Get["/"] = (arg) => AsyncGet();
        }

        private async Task<Negotiator> AsyncGet()
        {
            await Task.CompletedTask;
            return View["DashBoard"];
        }

        private async Task<DateTime> AsyncGetTime()
        {
            return await Task.FromResult(DateTime.Now);
        }

        
    }
}
