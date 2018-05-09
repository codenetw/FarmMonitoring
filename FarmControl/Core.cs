using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using BaseController;
using FarmControl.websocket;
using FarmMonitoring.backend.Dashboard;
using FarmMonitoring.webapi;
using FarmMonitoring.websocket;
using log4net;
using MessageBus.MessageBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Module = Autofac.Module;


namespace FarmMonitoring
{
    public sealed class Core : IDisposable
    {
        private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Configuration _configuration;
        private readonly IWebApiBootstraper _webapiBootstrap;
        private readonly ISocketServer _socketServer;
        private readonly IEnumerable<IBaseController> _baseControllers;
        private readonly IMessageCommunication _communication;
        private static string PathConfiguration => Path.Combine(assemblyFolder, "config.json");
        private static string PathConfigurationAutofac => Path.Combine(assemblyFolder, "autofac.json");

        internal Core(Configuration configuration,
                    IWebApiBootstraper webapiBootstrap,
                    ISocketServer socketServer,
                    IEnumerable<IBaseController> baseControllers,
                    MessageCommunicationFactory communicationFactory)
        {
            _configuration = configuration;
            _webapiBootstrap = webapiBootstrap;
            _socketServer = socketServer;
            _baseControllers = baseControllers;
            _communication = communicationFactory.Create();
        }

        public void Start()
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            if (!_configuration.Debug)
            {
                Console.WriteLine("Attach debugger and press enter");
                Console.ReadLine();
            }

            _logger.Info("Core starting...");

            FixWsHosts();

            foreach (var controller in _configuration.ActiveControllers.Where(x => x.Enabled).OrderBy(x => x.Order))
            {
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

        private static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            return Configure(builder,(x) => {});
        }

        private static IContainer ConfigureContainer<T>()
            where T : IModule, new()
        {
            var builder = new ContainerBuilder();
            return Configure(builder, (x) => { x.RegisterModule<T>(); });
        }
        
        private static IContainer Configure(ContainerBuilder builder, Action<ContainerBuilder> register)
        {
            #region core

            foreach (var controller in Directory.GetFiles(Path.Combine(assemblyFolder, "netstandard2.0"),
                "*Controller.dll"))
            {
                Assembly.LoadFrom(controller);
                var list = new List<Type>
                {
                    typeof(AutobernerController.AutobernerController),
                    typeof(EthernetController.EthernetController),
                    typeof(ProcessController.ProcessController),
                    typeof(MinerController.MinerController)
                };
            }

            var config = new ConfigurationBuilder();
            config.AddJsonFile(PathConfigurationAutofac);
            var module = new ConfigurationModule(config.Build());

            builder.RegisterModule(module);
            builder.RegisterType<Core>().FindConstructorsWith(new InternalConstructorFinder()).SingleInstance();
         
            builder.RegisterModule(new MessageBusModule(typeof(Core).Assembly));

            builder.Register(x => 
                    JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Path.Combine(assemblyFolder, "config.json"))))
                    .As<Configuration>().SingleInstance();
            
            builder.Register(x => new NancyHost(x.Resolve<INancyBootstrapper>(), new Uri(x.Resolve<Configuration>().Address)));


            #endregion

            builder.RegisterType<VideoCardInfo>();
            builder.RegisterType<VideoControllerWMI>();
            builder.RegisterType<VideoCardInfoHub>();

            //builder.RegisterType<AutobernerController.AutobernerController>().As<IBaseController>().SingleInstance();

            #region webapi

            builder.RegisterType<SocketServer>().As<ISocketServer>();
            builder.RegisterType<BootStrapper.AutofacConventionsBootstrapper>().As<INancyBootstrapper>();
            builder.RegisterType<BootStrapper>().As<IWebApiBootstraper>();
            
            #endregion

            register(builder);
            return builder.Build();
        }
        

        #region nancy 
        private void StartNancy()
        {
            try
            {
                _webapiBootstrap.Start();
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
                _webapiBootstrap.Stop();
                _logger.Info("nancy server stoped");
            }
            catch (Exception e)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug(e.Message, e);
                throw;
            }
        }

        private void FixWsHosts()
        {
            try
            {
                var file = Path.Combine(assemblyFolder, @"webapi\scripts\vmonitor.js");
                if (File.Exists(file))
                {
                    var script = File.ReadAllText(file);
                    script = script.Replace("WS_HOST_TMPL", _configuration.WebSocketAddress);
                    File.WriteAllText(file, script);
                    _logger.Info("fix wsserver address");
                }
                else
                {
                    _logger.Error("script moitoring not found");
                }
            }
            catch (Exception e)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug(e.Message, e);
                throw;
            }
        }

        #endregion

        #region websocket

        private void StartWebSocket()
        {
            _socketServer.Start();
        }
        private void StopWebSocket()
        {
            _socketServer.Stop();
        }

        #endregion

        public void Dispose()
        {
            Stop();
        }

        public static class Factory
        {
            public static Core Create<T>() where T : IModule, new() => ConfigureContainer<T>().Resolve<Core>();

            public static Core Create() => ConfigureContainer().Resolve<Core>();
        }

        public class InternalConstructorFinder : IConstructorFinder {
            public ConstructorInfo[] FindConstructors(Type t) => t.GetTypeInfo().DeclaredConstructors
                .Where(c => !c.IsPrivate && !c.IsPublic).ToArray();
        }
    }

}

