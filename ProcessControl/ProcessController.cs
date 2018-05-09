using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PluginManager;

namespace ProcessControl
{
    public sealed class ProcessController : IFarmPlugin
    {
        public string Name { get; }
        public PluginType Type { get; }

        public Task<object> Execute(CancellationToken ctx, params string[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
