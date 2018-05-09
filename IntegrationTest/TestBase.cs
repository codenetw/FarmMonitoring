using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using Module = Autofac.Module;

namespace IntegrationTest
{
    public abstract class TestBase
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
        private static IModule _module;

        protected TestBase(IModule module)
        {
            _module = module;
        }

        [TestFixtureSetUp]
        protected void TestBaseInit()
        {
        }

        [TestFixtureTearDown]
        protected void TestBaseCleanup()
        {
        }

        private class TestModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.Register(x => JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Path.Combine(assemblyFolder, "config.json"))))
                    .As<Configuration>().SingleInstance();
                if (_module != null)
                    builder.RegisterModule(_module);
                base.Load(builder);
            }
        }
    }
}
