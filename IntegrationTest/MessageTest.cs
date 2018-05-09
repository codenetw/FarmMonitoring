using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Farm.MessageBus;
using Farm.MessageBus.MessageBus;
using Moq;
using NUnit.Framework;

namespace IntegrationTest
{
    public class TestMessage : MessageBase
    {
        public string TestMsg { get; set; }
        public int Num { get; set; }
    }

    public class TestResultMessage : ResultMessageBase
    {
        public int num { get; set; }
    }

    [TestFixture]
    internal class MessageTest : TestBase, ICommunicationBase
    {
        public string Name => throw new NotImplementedException();

        public MessageTest()
            : base(new ModuleMessageBus())
        {
           
        }

        [SetUp]
        public void Init() { 
        }

        [Test]
        public void LoadTest()
        {
            
        }

        [Test]
        public void LoadTestGetType()
        {
            const int iteration = 1_000_000;
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < iteration; i++)
            {
                var x = new TestMessage().GetType();
                GC.KeepAlive(x);
            }
            
            sw.Stop();
            Console.WriteLine($"GetType {sw.Elapsed.Milliseconds}");
            sw.Reset();
            sw.Start();
            for (var i = 0; i < iteration; i++)
            {
                var y = new TestMessage().GetType();
                GC.KeepAlive(y);
            }
            sw.Stop();
            Console.WriteLine($"GetTypeOptimized {sw.Elapsed.Milliseconds}");
            
        }

        public void Handle(TestMessage checkProcess)
        {
           
        }

        public void Query(TestMessage checkProcess)
        {

        }

        public TestResultMessage Execute(TestMessage checkProcess)
        {
            return new TestResultMessage { num = checkProcess.Num };
        }

  
        private class ModuleMessageBus : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<MessageTest>().As<ICommunicationBase>();
                base.Load(builder);
            }
        }
    }
}
