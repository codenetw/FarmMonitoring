using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace MessageBus.MessageBus
{
    public class MessageBusModule : Module
    {
        private readonly Assembly _module;

        public MessageBusModule(Assembly module)
        {
            _module = module;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(_module).AssignableTo<ICommunicationBase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MessageDispatch>().As<IMessageCommunication>();
            builder.RegisterType<MessageCommunicationFactory>(); 
            base.Load(builder);
        }
    }
}
