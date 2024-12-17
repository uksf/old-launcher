namespace UKSF.Launcher.ServerService {
    internal static class ServerHandler {
//        public static readonly List<UKSF.Launcher.Network.> SERVERS = new List<Server> {
//            new Server("Primary Server", "arma.uk-sf.com", 2303, "l85", false),
//            new Server("Secondary Server", "arma.uk-sf.com", 2333, "l85", false),
//            new Server("Tertiary Server", "arma.uk-sf.com", 2343, "l85", false),
//            new Server("Test Server", "arma.uk-sf.com", 2353, "hi", false)
//        };

//        public static bool ServerRunning;

        public static void CheckServers() {
//            foreach (Server server in SERVERS) {
//                using (UdpClient udpClient = new UdpClient()) {
//                    try {
//                        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), server.Port);
//                        udpClient.Client.ReceiveTimeout = 100;
//                        udpClient.Connect(ipEndPoint);
//                        byte[] request = {
//                            0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
//                        };
//                        udpClient.Send(request, request.Length);
//                        udpClient.Receive(ref ipEndPoint);
//                        udpClient.Close();
//                        server.Active = true;
//                    } catch (SocketException) {
//                        server.Active = false;
//                    }
//                }
//            }
            //ServerRunning = SERVERS.Any(server => server.Name != "Test Server" && server.Active);
        }
    }
}