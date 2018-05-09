using System;
using System.Reflection;
using Autofac;
using FarmControl;
using FarmControl.WebAPI;
using log4net;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;

namespace NUnitTests
{
    public partial class InfoControllerTests
    {
        public class BootStrapper : IWebApiBootstraper
        {
            private readonly NancyHost _nancyHost;

            private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            public class CustomConventionsBootstrapper : DefaultNancyBootstrapper
            {
                protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
                {
                    pipelines.BeforeRequest += (ctx) => {
                        _logger.Info($"Request method:{ctx.Request.Method}");
                        return null;
                    };
                    base.RequestStartup(container, pipelines, context);
                }

                protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
                {
                    Conventions.ViewLocationConventions.Add((viewName, model, context) => string.Concat("WebApi/Views/", viewName));
                }
            }
            public class AutofacConventionsBootstrapper : AutofacNancyBootstrapper
            {
                private readonly ILifetimeScope _lifetimeScope;

                public AutofacConventionsBootstrapper(ILifetimeScope lifetimeScope)
                {
                    _lifetimeScope = lifetimeScope;
                }

                protected override ILifetimeScope GetApplicationContainer()
                {
                    return _lifetimeScope;
                }
            }

            public BootStrapper(NancyHost nancyHost)
            {
                _nancyHost = nancyHost;
            }
            
            public void Start()
            {
                _nancyHost.Start();
            }

            public void Stop()
            {
                _nancyHost.Stop();
            }
        }
    }
}
