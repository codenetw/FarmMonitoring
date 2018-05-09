using System.Threading.Tasks;

namespace FarmControl.websocket
{
    public interface ISocketServer
    {
        Task Start();
        Task Stop();
    }
}