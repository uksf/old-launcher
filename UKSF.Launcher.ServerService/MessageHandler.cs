using System;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Global

namespace UKSF.Launcher.ServerService {
    internal class MessageHandler {
        private const string HELP = "\thelp\n\tcreate [name]\n\tupdate [name]\n\treporequest [name]\n";
        private readonly ServerService.Client _client;

        public MessageHandler(ServerService.Client client, string message) {
            ServerHandler.CheckServers();
            _client = client;
            string[] parts = new Regex(" ").Split(message, 2);
            string command = parts[0];
            string[] parameters = parts.Skip(1).ToArray();
            Tuple<int, string> result = (Tuple<int, string>) GetType().GetMethod(command)?.Invoke(this, new object[] {parameters});
            if (result == null) {
                _client.SendMessage($"\nCommand '{message}' not found.\n{HELP}");
            } else {
                //ServerService.EventLog.WriteEntry($"Handle message result: {result.Item2}");
                switch (result.Item1) {
                    case 0:
                    case 1: // Success/Fail
                        _client.SendMessage(result.Item2);
                        break;
                    case 2: // Param fail
                        _client.SendMessage($"\nIncorrect usage.\n{HELP}");
                        break;
                    case 3: // Server running fail
                        _client.SendMessage("\nCannot execute command, there is a server running");
                        break;
                }
            }
            _client.SendCommand("stop");
        }

        private void Progress(string message) {
            if (message.Contains("command::")) {
                _client.SendCommand(message.Replace("command::", ""));
            } else {
                _client.SendMessage(message);
            }
        }

        // help
        public Tuple<int, string> help(string[] parameters) => new Tuple<int, string>(0, $"\nAvailable commands.\n{HELP}");

        // create [name]
        public Tuple<int, string> create(string[] parameters) {
//            if (ServerHandler.ServerRunning) return new Tuple<int, string>(3, "Failed");
            if (parameters.Length != 1) return new Tuple<int, string>(2, "Failed");
            bool result = RepoHandler.Create(parameters[0], Progress);
            if (!result) return new Tuple<int, string>(1, "Failed");
            ServerService.RepoUpdated($"reporequest {parameters[0]}");
            return new Tuple<int, string>(0, "Success");
        }

        // update [name]
        public Tuple<int, string> update(string[] parameters) {
//            if (ServerHandler.ServerRunning) return new Tuple<int, string>(3, "Failed");
            if (parameters.Length != 1) return new Tuple<int, string>(2, "Failed");
            bool result = RepoHandler.Update(parameters[0], Progress);
            if (!result) return new Tuple<int, string>(1, "Failed");
            ServerService.RepoUpdated($"reporequest {parameters[0]}");
            return new Tuple<int, string>(0, "Success");
        }

        // updateforce [name]
        public Tuple<int, string> updateforce(string[] parameters) {
            if (parameters.Length != 1) return new Tuple<int, string>(2, "Failed");
            bool result = RepoHandler.Update(parameters[0], Progress);
            if (!result) return new Tuple<int, string>(1, "Failed");
            ServerService.RepoUpdated($"reporequest {parameters[0]}");
            return new Tuple<int, string>(0, "Success");
        }

        // reporequest [name]
        public Tuple<int, string> reporequest(string[] parameters) {
            if (parameters.Length != 1) {
                return new Tuple<int, string>(2, "Failed");
            }
            bool result = RepoHandler.GetRepoData(parameters[0], Progress);
            return !result ? new Tuple<int, string>(1, "Failed") : new Tuple<int, string>(0, "Success");
        }

        // deltarequest [addon name]::[signature path]::[remote signature path]
        public Tuple<int, string> deltarequest(string[] parameters) {
            if (parameters.Length != 1) {
                return new Tuple<int, string>(2, "Failed");
            }
            string[] parts = parameters[0].Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries);
            string response = RepoHandler.BuildDelta(parts[0], parts[1], parts[2], parts[3], Progress);
            _client.SendCommand($"deltaresponse::{response}");
            return string.IsNullOrEmpty(response) ? new Tuple<int, string>(1, "Failed") : new Tuple<int, string>(0, "Success");
        }

        // deltadelete [name]
        public Tuple<int, string> deltadelete(string[] parameters) {
            if (parameters.Length != 1) {
                return new Tuple<int, string>(2, "Failed");
            }
            bool result = RepoHandler.DeleteDelta(parameters[0], Progress);
            return !result ? new Tuple<int, string>(1, "Failed") : new Tuple<int, string>(0, "Success");
        }
    }
}