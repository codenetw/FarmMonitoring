using System.IO;
using System.Reflection;
using Autofac;
using Farm.MessageBus;
using Newtonsoft.Json;
using Module = Autofac.Module;

namespace Farm.MinerController
{
    public class AutofacModule : Module
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MinerController>().As<ICommunicationBase>();
            builder.Register(x =>
                    JsonConvert.DeserializeObject<MinerConfigure>(File.ReadAllText(Path.Combine(assemblyFolder, "MinerController.json"))))
                .As<MinerConfigure>().SingleInstance();
            base.Load(builder);
        }
    }
}
