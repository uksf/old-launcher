using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UKSF.Launcher.Network;

namespace UKSF.Old.Launcher.Utility {
    internal static class UpdateHandler {
        public static async Task Initialise() {
            LogHandler.LogSpacerMessage(Global.Severity.INFO, "Initialising update handler");
            try {
                await CheckVersion();
                await ConnectHub();
            } catch (Exception) {
                LogHandler.LogSeverity(Global.Severity.ERROR, "Failed to initialise update handler");
            }
        }

        public static async Task CheckVersion(string version = "") {
            LogHandler.LogInfo("Checking for update");
            Global.VERSION = Version.Parse(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
            LogHandler.LogInfo($"Current version: {Global.VERSION}");

            if (string.IsNullOrEmpty(version)) {
                version = await ApiWrapper.Get("launcher/version");
            }

            Version latestVersion = Version.Parse(version);
            bool force = latestVersion.Major > Global.VERSION.Major || latestVersion.Minor > Global.VERSION.Minor;
            LogHandler.LogInfo($"Latest version: {latestVersion} - Auto-update: {Global.Settings.AutoUpdateLauncher} - Force update: {force}");

            if (Global.Settings.AutoUpdateLauncher && Global.VERSION < latestVersion || force) {
                LogHandler.LogInfo($"Updating to {latestVersion}");
                await Update();
            }
        }

        private static async Task Update() {
            try {
                string path = Path.Combine(Environment.CurrentDirectory, "UKSF.Launcher.Updater.exe");
                LogHandler.LogInfo($"Downloading updater to '{path}'");
                using (Stream stream = await ApiWrapper.GetFile("launcher/download/updater")) {
                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
                        await stream.CopyToAsync(fileStream);
                    }
                }
                LogHandler.LogInfo($"Downloaded updater to '{path}'");

                Process updater = new Process { StartInfo = { Arguments = ApiWrapper.Token, UseShellExecute = false, FileName = path, CreateNoWindow = true } };
                updater.Start();
                Core.ShutDown();
            } catch (Exception exception) {
                Core.Error(exception);
            }
        }

        private static async Task ConnectHub() {
            HubConnection hubConnection = await HubWrapper.Connecthub("launcher");
            hubConnection.On<string>("ReceiveLauncherVersion", async version => {
                LogHandler.LogInfo($"Received update event, new version: {version}");
                await CheckVersion(version);
            });
        }
    }
}
