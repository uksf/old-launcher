using System;
using System.Diagnostics;
using System.IO;
using UKSF.Launcher.Network;
using UKSF.Launcher.Patching;

namespace UKSF.Old.Launcher {
    public static class Global {
        public enum Severity {
            INFO,
            WARNING,
            ERROR
        }

        public static bool Debug = false;
        public static Process GameProcess = null;
        public static RepoClient Repo = null;
        public static Server Server = null;
        public static Version VERSION = Version.Parse("0.0.0.0");

        public class Constants {
            public const string FORMAT_DATE = "yyyy-MM-dd__HH-mm-ss";
            public const string FORMAT_TIME = "HH:mm:ss";
            public const string GAME_REGISTRY = @"SOFTWARE\WOW6432Node\bohemia interactive\arma 3";
            public const string MALLOC_SYSTEM_DEFAULT = "System Default";
            public const string REGSITRY = @"SOFTWARE\UKSF Launcher";
            public const long REQUIREDSPACE = 32212254720; // ~30GB // TODO: Get modpack size from api
            public static readonly string APPDATA = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UKSF Launcher");
            public static readonly string SPACER = Environment.NewLine + "##########################################################################################";
            public static readonly bool IS64_BIT = Environment.Is64BitOperatingSystem;
            public static readonly string NL = Environment.NewLine + Environment.NewLine;
            public static readonly string PROFILE_LOCATION_DEFAULT = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3");
            public static readonly string PROFILE_LOCATION_OTHER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3 - Other Profiles");
        }

        public class Settings {
            public static string AccountName = "";
            public static bool Admin = false;
            public static bool AutoUpdateLauncher = true;
            public static bool FirstTimeSetupDone = false;
            public static string GameLocation = "";
            public static string LoginEmail = "";
            public static string LoginPassword = "";
            public static string ModLocation = "";
            public static string Profile = "";
            public static bool StartupFilePatching = true;
            public static bool StartupHugePages = true;
            public static string StartupMalloc = "";
            public static bool StartupNoSplash = true;
            public static bool StartupScriptErrors = false;
        }
    }
}
