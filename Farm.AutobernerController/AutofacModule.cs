using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Farm.AutobernerController.Model;
using Farm.MessageBus;
using Newtonsoft.Json;
using Module = Autofac.Module;

namespace Farm.AutobernerController
{
    public class AutofacModule : Module
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutobernerController>().As<ICommunicationBase>();
            builder.Register(x =>
                    JsonConvert.DeserializeObject<AutobernerConfigure>(File.ReadAllText(Path.Combine(assemblyFolder, "AutobernerController.json"))))
                .As<AutobernerConfigure>().SingleInstance();
            base.Load(builder);
        }
    }
}
