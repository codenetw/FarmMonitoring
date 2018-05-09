using System.Reflection;
using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Hosting.Self;

namespace Farm.Core.webapi
{
    public class AutofacConventionsBootstrapper : AutofacNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions conventions)
        {
            conventions.StaticContentsConventions.Clear();

            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("scripts", @"assets\js\scripts"));
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("contents", @"assets\css\contents"));
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("templates", @"templates"));
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("assets", @"assets"));
            base.ConfigureConventions(conventions);
        }

    }
}
