using System.Threading.Tasks;

namespace Farm.Core.websocket
{
    public interface ISocketServer
    {
        Task Start();
        Task Stop();
    }
}