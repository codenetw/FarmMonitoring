using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Farm.Core.webapi;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;

namespace Test
{
    class Program
    {
        private static void Main(string[] args)
        {
            using (var nancy = new NancyHost(new Uri("http://127.0.0.1:8181"), new AutofacConventionsBootstrapper(), new HostConfiguration { RewriteLocalhost = false }))
            {
                nancy.Start();

                Console.ReadLine();
            }
        }
    }
}
