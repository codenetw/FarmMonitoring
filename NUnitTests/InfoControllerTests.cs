using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FarmControl;
using FarmControl.WebAPI;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Nancy.Testing;
using NUnit.Framework;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace NUnitTests
{
    [TestFixture]
    public partial class InfoControllerTests
    {
        [Test]
        public async Task TestInfoVideoCard()
        {
            var core = Core.Factory.Create<TestModuleResolve>();
            core.Start();
            
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper);

            var result = await browser.Get("/", with => {
                with.HttpRequest();
            });

            Assert.Equals(HttpStatusCode.OK, result.StatusCode);
        }

        public class TestModuleResolve : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                var hostConfig = new HostConfiguration
                {
                    UrlReservations = new UrlReservations {CreateAutomatically = true },
                    RewriteLocalhost = true
                };
                builder.Register(x => new NancyHost(new Uri(x.Resolve<Configuration>().Address), x.Resolve<INancyBootstrapper>(),hostConfig));
                builder.RegisterType<BootStrapper.AutofacConventionsBootstrapper>().As<INancyBootstrapper>();
                builder.RegisterType<BootStrapper>().As<IWebApiBootstraper>();
                base.Load(builder);
            }
        }

      
    }
}
