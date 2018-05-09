using System.IO;
using System.Reflection;
using Autofac;
using Farm.MessageBus;
using Newtonsoft.Json;
using Module = Autofac.Module;

namespace Farm.ProcessController
{
    public class AutofacModule : Module
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProcessController>().As<ICommunicationBase>();
            builder.Register(x =>
                    JsonConvert.DeserializeObject<ProcessConfigure>(File.ReadAllText(Path.Combine(assemblyFolder, "ProcessController.json"))))
                .As<ProcessConfigure>().SingleInstance();
            base.Load(builder);
        }
    }
}
