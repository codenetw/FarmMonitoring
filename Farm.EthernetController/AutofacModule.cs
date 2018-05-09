using System.IO;
using System.Reflection;
using Autofac;
using Farm.MessageBus;
using Newtonsoft.Json;
using Module = Autofac.Module;

namespace Farm.EthernetController
{
    public class AutofacModule : Module
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EthernetController>().As<ICommunicationBase>();
            builder.Register(x =>
                    JsonConvert.DeserializeObject<EthernetConfigure>(File.ReadAllText(Path.Combine(assemblyFolder, "EthernetController.json"))))
                .As<EthernetConfigure>().SingleInstance();
            base.Load(builder);
        }
    }
}
