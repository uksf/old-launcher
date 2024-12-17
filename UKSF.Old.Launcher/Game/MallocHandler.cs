using System.Collections.Generic;
using System.IO;
using System.Linq;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.Game {
    public static class MallocHandler {
        public static List<Malloc> GetMallocs() {
            LogHandler.LogSpacerMessage(Global.Severity.INFO, "Getting mallocs");
            string directory = Path.Combine(Global.Settings.GameLocation, "..", "Dll");
            LogHandler.LogSeverity(Global.Severity.INFO, $"Searching {directory} for malloc DLLs");
            List<Malloc> mallocs = new List<Malloc> {new Malloc(Global.Constants.MALLOC_SYSTEM_DEFAULT)};
            if (!Directory.Exists(directory)) return mallocs;
            mallocs.AddRange(from file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly) where !Path.GetFileNameWithoutExtension(file).Contains("x64") select new Malloc(file));
            return mallocs;
        }

        public class Malloc {
            public readonly string Name;

            public Malloc(string fileName) {
                Name = Path.GetFileNameWithoutExtension(fileName);
                LogHandler.LogInfo("Found malloc: " + Name);
            }
        }
    }
}
