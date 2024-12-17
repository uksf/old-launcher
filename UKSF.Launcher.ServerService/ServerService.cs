using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace UKSF.Launcher.ServerService {
    public class ServerService {
        private const int PORT = 48900;
        private const int MAX_RECEIVE_ATTEMPT = 10;
//        public new static EventLog EventLog;
        private static Socket _socket;

        private static int _receiveAttempt;

        private static List<Client> _clients;

        private Container _components;
        private Timer _gameServerTimer, _cleanupTimer;

        public ServerService() {
            InitializeComponent();

//            EventLog = new EventLog();
//            if (!EventLog.SourceExists("Server Service Source")) {
//                EventLog.CreateEventSource("Server Service Source", "Server Service Log");
//            }
//            EventLog.Source = "Server Service Source";
//            EventLog.Log = "Server Service Log";
        }

        private void InitializeComponent() {
            _components = new Container();
//            ServiceName = "ServerService";
        }

        protected void Dispose(bool disposing) {
            OnStop();
            if (disposing) {
                _components?.Dispose();
            }
//            base.Dispose(disposing);
        }

        protected void OnStart(string[] args) {
//            EventLog.WriteEntry("Started");
            // ReSharper disable once InconsistentlySynchronizedField
            _clients = new List<Client>();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            _socket.Listen(100);
            _socket.BeginAccept(AcceptCallback, _socket);
            _gameServerTimer = new Timer(CheckGameServers, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            _cleanupTimer = new Timer(Cleanup, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        private static void AcceptCallback(IAsyncResult asyncResult) {
            if (_socket != (Socket) asyncResult.AsyncState) return;
            try {
                Client client = new Client(_socket.EndAccept(asyncResult)) {Buffer = new byte[8192]};
                client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, ReceiveCallback, client);
                _socket.BeginAccept(AcceptCallback, _socket);
//                EventLog.WriteEntry($"Client connected {client.Guid}");
            } catch (Exception exception) {
//                EventLog.WriteEntry(exception.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult asyncResult) {
            try {
                Client client = (Client) asyncResult.AsyncState;
                if (!client.Socket.Connected) return;
                int received = client.Socket.EndReceive(asyncResult);
                if (received > 0) {
                    _receiveAttempt = 0;
                    byte[] data = new byte[received];
                    Buffer.BlockCopy(client.Buffer, 0, data, 0, data.Length);
                    string fullMessage = Encoding.UTF8.GetString(data);
                    if (fullMessage.Contains("::end")) {
                        string[] messages = Regex.Split(fullMessage, @"(?<=::end)");
                        int done = 0;
                        foreach (string message in messages.Where(message => message.Contains("::end"))) {
                            HandleMessage(client, message);
                            done++;
                        }
                        if (done != messages.Length) {
                            fullMessage = messages[messages.Length - 1];
                        }
                        data = Encoding.UTF8.GetBytes(fullMessage);
                        Buffer.BlockCopy(client.Buffer, 0, data, 0, data.Length);
                    }
                    client.Socket?.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, ReceiveCallback, client);
                } else if (_receiveAttempt < MAX_RECEIVE_ATTEMPT) {
                    _receiveAttempt++;
                    client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, ReceiveCallback, client);
                } else {
                    _receiveAttempt = 0;
                }
            } catch (Exception) {
//                EventLog.WriteEntry("Client receive failed");
            }
        }

        private static void HandleMessage(Client client, string message) {
            //EventLog.WriteEntry($"Message '{message}' received from {client.Guid}");
            message = message.Replace("::end", "");
            switch (message) {
                case "connect":
                    lock (_clients) {
                        if (!_clients.Contains(client)) {
                            _clients.Add(client);
                        }
                    }
                    break;
                case "disconnect":
                    //EventLog.WriteEntry($"Client disconnected {client.Guid}");
                    client.Socket?.Close();
                    lock (_clients) {
                        _clients.Remove(client);
                    }
                    break;
                case "break":
                    client.Socket?.EndReceive(null);
                    break;
                default:
                    // ReSharper disable once ObjectCreationAsStatement
                    Task.Run(() => new MessageHandler(client, message));
                    break;
            }
        }

        protected void OnStop() {
            //EventLog.WriteEntry("Stopped");
            _socket?.Dispose();
            _socket?.Close();
            _socket = null;

            _gameServerTimer?.Dispose();
            _cleanupTimer?.Dispose();

            lock (_clients) {
                foreach (Client client in _clients) {
                    client.Socket?.Close();
                }
                _clients.Clear();
            }
        }

        private static void CheckGameServers(object unused) {
            // TODO: Add framework for launching servers. Switch to events instead of polling.
            lock (_clients) {
                if (_clients.Count <= 0) return;
                ServerHandler.CheckServers();
                foreach (Client client in _clients) {
//                    client.SendCommand(ServerHandler.SERVERS.Where(server => server.Active)
//                                                    .Aggregate("servers", (current, server) => string.Join("::", current, $"{server.Serialize()}")));
                }
            }
        }

        private void Cleanup(object unused) {
            try {
                List<Client> delete = new List<Client>();
                lock (_clients) {
                    foreach (Client client in _clients.Where(client => !client.Socket.Connected)) {
                        delete.Add(client);
                        client.Socket?.Shutdown(SocketShutdown.Both);
                        client.Socket?.Close();
                    }
                    foreach (Client client in delete) {
                        //EventLog.WriteEntry($"Client disconnected {client.Guid}");
                        _clients.Remove(client);
                    }
                    if (_clients.Count == 0) {
                        RepoHandler.CleanRepos();
                    }
                }
            } catch {
                // ignored
            }
        }

        public static void RepoUpdated(string message) {
            Task repoUpdatedTask = new Task(() => {
                lock (_clients) {
                    if (_clients.Count <= 0) return;
                    foreach (Client client in _clients) {
                        HandleMessage(client, message);
                    }
                }
            });
            repoUpdatedTask.Start();
        }

        public class Client {
            public readonly string Guid;

            public readonly Socket Socket;

            public byte[] Buffer;

            public Client(Socket socket) {
                Socket = socket;
                Guid = $"{((IPEndPoint) Socket.RemoteEndPoint).Address}_{System.Guid.NewGuid()}";
            }

            public void SendCommand(string message) {
                try {
//                    EventLog.WriteEntry($"Command '{message}' sent to {Guid}");
                    Socket.Send(Encoding.ASCII.GetBytes($"command::{message}::end"));
                } catch (Exception exception) {
//                    EventLog.WriteEntry($"Failed to send '{message}' to {Guid}\n{exception}");
                }
            }

            public void SendMessage(string message) {
                try {
                    Socket.Send(Encoding.ASCII.GetBytes($"message::{message}::end"));
                } catch (Exception exception) {
//                    EventLog.WriteEntry($"Failed to send '{message}' to {Guid}\n{exception}");
                }
            }
        }
    }
}