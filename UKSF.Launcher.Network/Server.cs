using System;

namespace UKSF.Launcher.Network {
    [Serializable]
    public class Server {
        public readonly string Ip;

        public readonly string Name;
        public readonly string Password;
        public readonly int Port;
        public bool Active;

        public Server(string name, string ip, int port, string password, bool active) {
            Name = name;
            Ip = ip;
            Port = port;
            Password = password;
            Active = active;
        }

        public string Serialize() => $"{Name}:{Ip}:{Port}:{Password}:{Active}";

        public static Server DeSerialize(string serialized) {
            string[] parts = serialized.Split(new[] {':'});
            return new Server(parts[0], parts[1], Convert.ToInt32(parts[2]), parts[3], Convert.ToBoolean(parts[4]));
        }
    }
}