using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Farm.BaseController;
using Farm.Core.Common;
using Farm.Core.webapi;
using Farm.Core.websocket;
using Farm.MessageBus.MessageBus;
using Farm.Moderator;
using log4net;
using Microsoft.Extensions.Configuration;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Newtonsoft.Json;


namespace Farm.Core
{
    public sealed class Core : IDisposable
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Configuration _configuration;
        private readonly ISocketServer _socketServer;
        private readonly IEnumerable<IBaseController> _baseControllers;

        private static string PathConfiguration => Path.Combine(assemblyFolder, "config.json");
        private static string PathConfigurationAutofac => Path.Combine(assemblyFolder, "autofac.json");

        internal Core(Configuration configuration,
                      ISocketServer socketServer,
                      IEnumerable<IBaseController> baseControllers,
                      params string[] parameters)
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            CoreStartParameters.ExecParams(_configuration, PathConfiguration, parameters);
            _configuration = configuration;
            _socketServer = socketServer;
            _baseControllers = baseControllers;
        }

        
        public void Start()
        {
            if (!_configuration.Debug)
            {
                Console.WriteLine("await debugger...");
                Helper.AwaitDebugger();
                Debugger.Break();
            }

            _logger.Info("Core starting...");
            _logger.Warn($": IP ADDRESS [{GetLocalIPAddress()}]");

            foreach (var controller in _configuration.ActiveControllers.Where(x => x.Enabled).OrderBy(x => x.Order))
            {
                _logger.Info($"Plugin > {controller.Name} starting...");
                _baseControllers.FirstOrDefault(x => x.GetType().Name == controller.Name)?.Start();
            }

            StartWebSocket();
            StartNancy();

            _logger.Info("Core ready!");
        }
        public void Stop()
        {
            _logger.Info("Core stoping...");

            foreach (var controller in _configuration.ActiveControllers.Where(x => x.Enabled).OrderBy(x => x.Order))
                _baseControllers.FirstOrDefault(x => x.GetType().Name == controller.Name)?.Stop();
          
            StopWebSocket();
            StopNancy();
            _logger.Info("Core stoped!");
        }
        public void Dispose()
        {
            Stop();
        }

        private static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            return AutofacConfigure(builder,(x) => {});
        }
        private static IContainer ConfigureContainer<T>()
            where T : IModule, new()
        {
            var builder = new ContainerBuilder();
            return AutofacConfigure(builder, (x) => { x.RegisterModule<T>(); });
        }
        private static IContainer AutofacConfigure(ContainerBuilder builder, Action<ContainerBuilder> register)
        {

            var config = new ConfigurationBuilder();
                config.AddJsonFile(PathConfigurationAutofac);

            var module = new ConfigurationModule(config.Build());
            builder.RegisterModule(module);
            builder.RegisterModule<Moderator.AutofacModule>();
            builder.RegisterType<Core>().FindConstructorsWith(new InternalConstructorFinder()).SingleInstance();
            builder.RegisterModule(new MessageBusModule(typeof(Core).Assembly));
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
            builder.Register(x => 
                    JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Path.Combine(assemblyFolder, "config.json"))))
                    .As<Configuration>().SingleInstance();

            builder.Register(x => new NancyHost(x.Resolve<INancyBootstrapper>(), new Uri($"http://{GetLocalIPAddress()}:{x.Resolve<Configuration>().WebApiPort}")));
            builder.RegisterType<Moderator.Moderator>().As<IModerator>();
            builder.Register(x => new SocketServer($"ws://{GetLocalIPAddress()}:{x.Resolve<Configuration>().WebSocketPort}",x.Resolve<ILifetimeScope>())).As<ISocketServer>();
            builder.RegisterType<Aggregator>().AsSelf();
            register(builder);
            return builder.Build();
        }
        

        private void StartNancy()
        {
            try
            {
                _logger.Info("nancy server start");
            }
            catch (Exception e)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug(e.Message, e);
                throw;
            }
        }
        private void StopNancy()
        {
            try
            {
                _logger.Info("nancy server stoped");
            }
            catch (Exception e)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug(e.Message, e);
                throw;
            }
        }


        private void StartWebSocket()
        {
            _socketServer.Start();
        }
        private void StopWebSocket()
        {
            _socketServer.Stop();
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static class Factory
        {
            public static Core Create<T>() where T : IModule, new() => ConfigureContainer<T>().Resolve<Core>();

            public static Core Create(params string[] parameters) => ConfigureContainer().Resolve<Core>(new NamedParameter("parameters", parameters ?? new string[0]));
        }

        public class InternalConstructorFinder : IConstructorFinder {
            public ConstructorInfo[] FindConstructors(Type t) => t.GetTypeInfo().DeclaredConstructors
                .Where(c => !c.IsPrivate && !c.IsPublic).ToArray();
        }
    }

}

