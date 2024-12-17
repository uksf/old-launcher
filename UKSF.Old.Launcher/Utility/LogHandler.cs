using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UKSF.Old.Launcher.Utility {
    internal static class LogHandler {
        private static readonly object LOCK_OBJECT = new object();
        private static readonly string LOGS_PATH = Path.Combine(Global.Constants.APPDATA, "logs");
        private static string logFile;


        public static void StartLogging() {
            if (Global.Debug) return;
            Console.WriteLine(LOGS_PATH);
            Directory.CreateDirectory(LOGS_PATH);
            string[] logFiles = new DirectoryInfo(LOGS_PATH)
                                .EnumerateFiles("*.log")
                                .OrderByDescending(file => file.LastWriteTime)
                                .Select(file => file.Name)
                                .ToArray();
            if (logFiles.Length > 9) {
                File.Delete(Path.Combine(LOGS_PATH, logFiles.Last()));
            }

            lock (LOCK_OBJECT) {
                logFile = Path.Combine(LOGS_PATH, $"L__{DateTime.Now.ToString(Global.Constants.FORMAT_DATE)}.log");
                try {
                    File.Create(logFile).Close();
                } catch (Exception e) {
                    Console.WriteLine($"Log file not created: {logFile}. {e.Message}");
                }
            }

            LogInfo("Log Created");
        }

        private static void LogToFile(string message) {
            if (Global.Debug) {
                Console.WriteLine(message);
                return;
            };
            if (logFile == null) return;
            lock (LOCK_OBJECT) {
                using (StreamWriter writer = new StreamWriter(logFile, true)) {
                    writer.WriteLine(message);
                }
            }
        }

        public static void LogSeverity(Global.Severity severity, string message) {
            LogToFile($"{DateTime.Now.ToString(Global.Constants.FORMAT_TIME)} {severity}: {message}");
        }

        private static void LogNoTime(string message) {
            LogToFile(message);
        }

        public static void LogSpacer() {
            LogNoTime(Global.Constants.SPACER);
        }

        public static void LogSpacerMessage(Global.Severity severity, string message) {
            LogSpacer();
            LogSeverity(severity, message);
        }

        public static void LogInfo(string message) {
            LogSeverity(Global.Severity.INFO, message);
        }

        public static void CloseLog() {
            LogInfo("Closing Log");
        }
    }
}
