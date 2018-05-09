using System.Runtime.CompilerServices;

namespace FarmMonitoring
{
    public class Configuration
    {
        public string Address { get; set; }
        public bool Debug { get; set; }
        public string WebSocketAddress { get; set; }
        public Controller[] ActiveControllers { get; set; }
        public VideoCardConfigure[] Cards { get; set; }
    }
    public class Controller
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Enabled { get; set; }
    }

    public class VideoCardConfigure
    {
        public string GUID { get; set; }
        public string Path { get; set; }
    }

}
