using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using UKSF.Old.Launcher.UI.General;
using UKSF.Old.Launcher.UI.Main;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.Game {
    internal static class GameHandler {
        public static string GetGameInstallation() {
            string location = "";
            RegistryKey gameKey = Registry.LocalMachine.OpenSubKey(Global.Constants.GAME_REGISTRY);
            if (gameKey == null) return location;
            string install = gameKey.GetValue("main", "").ToString();
            if (!Directory.Exists(install)) return location;
            string exe = Path.Combine(install, Global.Constants.IS64_BIT ? "arma3_x64.exe" : "arma3.exe");
            if (File.Exists(exe)) {
                location = exe;
            }

            return location;
        }

        public static bool CheckDriveSpace(string path) {
            return DriveInfo.GetDrives().Where(drive => drive.Name.Equals(Path.GetPathRoot(path))).ElementAt(0).AvailableFreeSpace >= Global.Constants.REQUIREDSPACE;
        }

        public static void StartGame() {
            if (Global.GameProcess != null) return;
            new Task(() => {
                LogHandler.LogSpacerMessage(Global.Severity.INFO, "Starting game...");
                try {
                    ProfileHandler.Profile profile = ProfileHandler.Profiles.FirstOrDefault(x => x.DisplayName == Global.Settings.Profile);
                    if (ProfileHandler.CheckProfileName(profile) == 3) {
                        LogHandler.LogSeverity(Global.Severity.ERROR, $"Tried to start game with profile '{profile?.DisplayName}' but it cannot be found");
                        MainWindow.Instance.SettingsControl.RaiseEvent(new RoutedEventArgs(SettingsControl.MAIN_SETTINGS_CONTROL_WARNING_EVENT));
                    } else {
                        ProfileHandler.UpdateProfileSquad(profile);
                        MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HomeControl.HOME_CONTROL_STATE_EVENT) {Value = 2});
                        MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = false});
                        Global.GameProcess = new Process {StartInfo = {UseShellExecute = false, FileName = Global.Settings.GameLocation, Arguments = GetStartupParameters()}};
                        LogHandler.LogInfo($"Startup Parameters {Global.GameProcess.StartInfo.Arguments}");
//                        throw new InvalidOperationException();
//                        Global.GameProcess.Start();
//                        Global.GameProcess.WaitForExit();
                    }
                } catch (Exception exception) {
                    Core.Error(exception);
                } finally {
                    Global.GameProcess = null;
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.IntRoutedEventArgs(HomeControl.HOME_CONTROL_STATE_EVENT) {Value = 0});
                    MainWindow.Instance.HomeControl.RaiseEvent(new SafeWindow.BoolRoutedEventArgs(HomeControl.HOME_CONTROL_PLAY_EVENT) {State = true});
                }
            }).Start();
        }

        public static string GetStartupParameters() {
            StringBuilder startupString = new StringBuilder();
            startupString.Append($"-name={Global.Settings.Profile}");
            startupString.Append(Global.Settings.StartupNoSplash ? " -nosplash" : "");
            startupString.Append(Global.Settings.StartupScriptErrors ? " -showScriptErrors" : "");
            startupString.Append(Global.Settings.StartupFilePatching ? " -filePatching" : "");
            startupString.Append(Global.Settings.StartupHugePages ? " -hugepages" : "");
            if (Global.Settings.StartupMalloc != Global.Constants.MALLOC_SYSTEM_DEFAULT) {
                startupString.Append(Global.Settings.GameLocation.Contains("x64") ? $" -malloc={Global.Settings.StartupMalloc}_x64" : $" -malloc={Global.Settings.StartupMalloc}");
            } else {
                startupString.Append(" -malloc=system");
            }

            if (Global.Server != null && Global.Server != ServerHandler.NO_SERVER) {
                startupString.Append($" -connect={Global.Server?.Ip} -port={Global.Server?.Port - 1} -password={Global.Server?.Password}");
            }

            //StringBuilder modsString = new StringBuilder();
            //foreach (string mod in Global.Repo.GetRepoMods()) {
            //    modsString.Append($"{mod};");
            //}

            //startupString.Append($" \"-mod={modsString}\"");
            return startupString.ToString();
        }
    }
}
