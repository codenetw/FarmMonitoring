using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Farm.Core;
using log4net.Config;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;

namespace Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var core = Core.Factory.Create())
            {
                XmlConfigurator.Configure();
                core.Start();
                Console.ReadLine();
            }
        }
    }
}
