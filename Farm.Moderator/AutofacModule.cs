using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Farm.Moderator.Helper;
using Newtonsoft.Json;
using Module = Autofac.Module;

namespace Farm.Moderator
{
    public class AutofacModule : Module
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Moderator>().As<IModerator>();
            builder.RegisterType<FarmVisitor>().As<IFarmVisitor<Result>>();
            
            builder.Register(x => !File.Exists(Path.Combine(assemblyFolder, "Moderator.bin")) 
                    ? new ModeratorConfigure()
                    : BinarySerializer.DeserializeFromString<ModeratorConfigure>(File.ReadAllText(Path.Combine(assemblyFolder, "Moderator.bin"))))
                .As<ModeratorConfigure>().SingleInstance();
            base.Load(builder);
        }

       
    }
}

