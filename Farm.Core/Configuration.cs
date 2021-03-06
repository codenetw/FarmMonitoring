﻿namespace Farm.Core
{
    public class Configuration
    {
        public string WebApiPort { get; set; }
        public string WebSocketPort { get; set; }
        public string[] WebApiTranslateMessages { get; set; }
        public bool Debug { get; set; }
        public string[] Coins { get; set; }
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
        public string Name { get; set; }
        public string GUID { get; set; }
        public string Path { get; set; }
    }

}
