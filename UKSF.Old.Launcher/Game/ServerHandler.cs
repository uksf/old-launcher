using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UKSF.Launcher.Network;
using UKSF.Old.Launcher.UI.General;
using UKSF.Old.Launcher.UI.Main;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.Game {
    public static class ServerHandler {
        private static Task repoCheckTask;

        //private static ServerSocket serverSocket;
        public static readonly Server NO_SERVER = new Server("No Server", "", 0, "", false);

        public static void StartServerHandler() {
//            serverSocket = new ServerSocket();
//            serverSocket.ServerLogEvent += ServerMessageLogCallback;
//            serverSocket.ServerCommandEvent += ServerMessageCallback;
//            serverSocket.ServerConnectedEvent += ServerSocketOnServerConnectedEvent;
//            serverSocket.AutoResetEvent.WaitOne();
        }

        private static void ServerSocketOnServerConnectedEvent(object sender, string unused) {
            MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HomeControl.HOME_CONTROL_STATE_EVENT) {Value = 1});
            new Task(async () => await SendDelayedServerMessage("reporequest uksf", 100)).Start();
        }

        private static async Task SendDelayedServerMessage(string message, int delay) {
            await Task.Delay(delay);
            SendServerMessage(message);
        }

        public static void SendServerMessage(string message) {
//            serverSocket.SendMessage(message);
        }

        public static void SendDeltaRequest(string name, string path, string relativePath, string remotePath) {
//            serverSocket.SendMessage(Encoding.ASCII.GetBytes($"deltarequest {name}::{path}::{relativePath}::{remotePath}::end"));
        }

        internal static void SendDeltaDelete(string path) {
//            serverSocket.SendMessage(Encoding.ASCII.GetBytes($"deltadelete {path}::end"));
        }

        private static void ServerMessageCallback(object sender, string message) {
            if (message.Contains("message::")) {
                LogHandler.LogInfo(message.Replace("message::", ""));
            } else {
                HandleMessage(message);
            }
        }

        private static void HandleMessage(string message) {
            string[] parts = new Regex("::").Split(message.Replace("command::", ""), 2);
            string commandArguments = parts.Length > 1 ? parts[1] : "";
            if (!message.Contains("command::")) return;
            switch (parts[0]) {
                case "servers":
                    Task serversUpdateTask = new Task(() => {
                        List<Server> servers = new List<Server>();
                        if (!string.IsNullOrEmpty(commandArguments)) {
                            string[] serverParts = commandArguments.Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
                            servers.AddRange(serverParts.Select(Server.DeSerialize));
                        }

                        MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.ServerRoutedEventArgs(HomeControl.HOME_CONTROL_SERVER_EVENT) {Servers = servers});
                    });
                    serversUpdateTask.Start();
                    break;
                case "repodata":
                    if (repoCheckTask != null) return;
                    try {
                        Task.Run(() => {
                            repoCheckTask = Task.Run(() => {
                                Core.CancellationTokenSource = new CancellationTokenSource();
                                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HomeControl.HOME_CONTROL_STATE_EVENT) {Value = 1});
                                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = false});
                                while (!Global.Repo.CheckLocalRepo(commandArguments, Core.CancellationTokenSource) && !Core.CancellationTokenSource.IsCancellationRequested) {
                                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = false});
                                }

                                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = true});
                                MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HomeControl.HOME_CONTROL_STATE_EVENT) {Value = 0});
                            });
                            repoCheckTask.Wait();
                            repoCheckTask = null;
                        });
                    } catch (Exception exception) {
                        LogHandler.LogSeverity(Global.Severity.ERROR, $"Failed to process remote repo\n{exception}");
                    }

                    break;
                case "deltaresponse":
                    try {
                        Task repoDeltaTask = new Task(() => Global.Repo.QueueDelta(commandArguments));
                        repoDeltaTask.Start();
                    } catch (Exception exception) {
                        LogHandler.LogSeverity(Global.Severity.ERROR, $"Failed to process delta\n{exception}");
                    }

                    break;
                case "unlock":
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = true});
                    break;
                default: return;
            }
        }

        private static void ServerMessageLogCallback(object sender, string message) {
            LogHandler.LogInfo(message);
        }

        public static void Stop() {
//            serverSocket?.StopCheck();
//            serverSocket = null;
        }
    }
}
