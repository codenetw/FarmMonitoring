using System;
using System.Threading;
using System.Threading.Tasks;

namespace PluginManager
{
    public enum PluginType { CardInformation, DetailInformation, Task, ControlPanel, Command }

    public interface IFarmPlugin : IDisposable
    {
        string Name { get;}
        PluginType Type { get; }
        Task<object> Execute(CancellationToken ctx, params string[] parameters);        
    }
}
