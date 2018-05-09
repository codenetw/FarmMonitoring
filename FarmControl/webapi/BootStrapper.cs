using System.Reflection;
using Autofac;
using log4net;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Hosting.Self;

namespace FarmMonitoring.webapi
{
    internal sealed class BootStrapper : IWebApiBootstraper
    {
        private readonly NancyHost _nancyHost;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public class AutofacConventionsBootstrapper : AutofacNancyBootstrapper
        {
            private readonly ILifetimeScope _lifetimeScope;
            protected override IRootPathProvider RootPathProvider => new CustomRootPathProvider();

            public AutofacConventionsBootstrapper(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
            }

            protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
            {
                pipelines.BeforeRequest += (ctx) =>
                {
                    _logger.Info($"Request method: {ctx.Request.Method}");
                    return null;
                };
                pipelines.OnError += (ctx, ex) =>
                {
                    _logger.Info($"Error request method: {ctx.Request.Method}, error {ex.Message}");
                    return null;
                };
                base.ApplicationStartup(container, pipelines);
            }

            protected override void ConfigureConventions(NancyConventions conventions)
            {
                conventions.StaticContentsConventions.Clear();
                conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("scripts", @"scripts"));
                conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("contents", @"contents"));
                conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("Templates", @"Views\Templates"));

                base.ConfigureConventions(conventions);
            }
            protected override ILifetimeScope GetApplicationContainer()
            {
                return _lifetimeScope;
            }
            private class CustomRootPathProvider : IRootPathProvider
            {
                public string GetRootPath()
                {
                    return "webapi";
                }
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
