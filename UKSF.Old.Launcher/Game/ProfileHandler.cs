using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UKSF.Launcher.Network;
using UKSF.Old.Launcher.Utility;

namespace UKSF.Old.Launcher.Game {
    public static class ProfileHandler {
        private const string PROFILE_EXTENSION = ".Arma3Profile";
        public static List<Profile> Profiles = new List<Profile>();
        private static List<Rank> ranks = new List<Rank>();

        public static async Task Initialise() {
            try {
                ranks = JsonConvert.DeserializeObject<List<Rank>>(await ApiWrapper.Get("ranks"));
            } catch (Exception) {
                LogHandler.LogSeverity(Global.Severity.ERROR, "Failed to retrieve ranks");
            }

            GetAllProfiles();
        }

        private static void GetAllProfiles() {
            LogHandler.LogSpacerMessage(Global.Severity.INFO, "Getting profiles");
            Profiles = GetProfiles(Global.Constants.PROFILE_LOCATION_DEFAULT);
            Profiles.AddRange(GetProfiles(Global.Constants.PROFILE_LOCATION_OTHER));
        }

        private static List<Profile> GetProfiles(string directory) {
            LogHandler.LogSeverity(Global.Severity.INFO, $"Searching {directory} for profiles");
            List<Profile> profiles = new List<Profile>();
            if (!Directory.Exists(directory)) return profiles;
            IEnumerable<string> directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
            profiles.AddRange(from profileDirectory in directories
                              select Directory.EnumerateFiles(profileDirectory, $"*{PROFILE_EXTENSION}", SearchOption.TopDirectoryOnly)
                              into directoryFiles
                              select directoryFiles.FirstOrDefault(x => Path.GetFileName(x).Count(y => y == '.') == 1)
                              into file
                              where !string.IsNullOrEmpty(file)
                              select new Profile(file));
            return profiles;
        }

        public static Profile FindFormattedProfile(IEnumerable<Profile> profiles) => profiles.FirstOrDefault(IsFormattedProfile);

        private static bool IsFormattedProfile(Profile profile) {
            string rank = profile.DisplayName.Split('.')[0].ToLower();
            return ranks.Any(x => x.Abbreviation.ToLower() == rank);
        }

        public static int CheckProfileName(Profile profile) {
            if (!IsFormattedProfile(profile)) {
                LogHandler.LogSeverity(Global.Severity.WARNING, $"Selected profile '{profile.DisplayName}' is not a formatted profile, will not update name");
                return 1;
            }

            ValidateFilenames(profile);
            if (profile.DisplayName == Global.Settings.AccountName) return 2;
            if (File.Exists(Profile.FormatFile(Global.Settings.AccountName))) return 3;
            LogHandler.LogInfo($"Updating profile '{profile.DisplayName}' to '{Global.Settings.AccountName}'");
            string originalPath = profile.FilePath;
            string originalName = profile.Name;
            profile.SetProfileName(Global.Settings.AccountName);
            string newDirectory = Path.GetDirectoryName(profile.FilePath);
            if (!Directory.Exists(newDirectory)) {
                Directory.Move(Path.GetDirectoryName(originalPath), newDirectory);
            }

            foreach (string file in Directory.EnumerateFiles(newDirectory, $"*{PROFILE_EXTENSION}", SearchOption.TopDirectoryOnly)) {
                string newFile = Path.Combine(newDirectory, file.Replace(originalName, profile.Name));
                if (!File.Exists(newFile)) {
                    File.Move(file, newFile);
                }
            }

            return 0;
        }

        private static void ValidateFilenames(Profile profile) {
            string directory = Path.Combine(Global.Constants.PROFILE_LOCATION_OTHER, profile.Name);
            if (!Directory.Exists(directory)) {
                Directory.Move(Path.GetDirectoryName(profile.FilePath), directory);
            }

            profile.FilePath = Profile.FormatFile(profile.DisplayName);
            foreach (string file in Directory.EnumerateFiles(directory, $"*{PROFILE_EXTENSION}", SearchOption.TopDirectoryOnly)) {
                string newFile = Path.Combine(directory, file.Replace(file.Split('.')[0], profile.Name));
                if (!File.Exists(newFile)) {
                    File.Move(file, newFile);
                }
            }
        }

        public static void CopyProfile(Profile profile, string newDirectory) {
            if (!File.Exists(profile.FilePath)) return;
            if (File.Exists(Profile.FormatFile(Global.Settings.AccountName))) return;
            string directory = Path.GetDirectoryName(profile.FilePath);
            if (!Directory.Exists(directory)) return;
            LogHandler.LogInfo($"Copying profile '{profile.DisplayName}'");
            List<string> files = Directory.EnumerateFiles(directory, $"*{PROFILE_EXTENSION}").ToList();
            Profile newProfile = new Profile();
            Directory.CreateDirectory(Path.Combine(newDirectory, newProfile.Name));
            foreach (string file in files) {
                string fileName = Path.GetFileName(file);
                if (fileName != null) {
                    File.Copy(file, Path.Combine(newDirectory, newProfile.Name, fileName.Replace(fileName.Split('.')[0], newProfile.Name)));
                }
            }

            Profiles.Add(newProfile);
        }

        public static void UpdateProfileSquad(Profile profile) {
            if (profile == null) return;
            if (!File.Exists(profile.FilePath)) return;
            if (!profile.IsUksf) {
                LogHandler.LogSeverity(Global.Severity.WARNING, $"Profile '{profile.DisplayName}' is not the UKSF profile, will not force Squad info.");
                return;
            }

            string[] lines = File.ReadAllLines(profile.FilePath);
            for (int index = 0; index < lines.Length; index++) {
                if (lines[index].Contains("glasses=")) {
                    lines[index] = @"	glasses=""None"";";
                } else if (lines[index].Contains("unitType=")) {
                    lines[index] = @"	unitType=1;";
                } else if (lines[index].Contains("unitId=")) {
                    lines[index] = @"	unitId=-1;";
                } else if (lines[index].Contains("squad=")) {
                    //lines[index] = @"	squad=""http://arma.uk-sf.com/squadtag/A3/squad.xml"";"; //TODO: Add new squad URL
                }
            }

            File.WriteAllLines(profile.FilePath, lines);
            LogHandler.LogInfo($"Squad info forced for profile '{profile.DisplayName}'");
        }

        public class Profile {
            private const string PROFILE_JOINER = "%2e";
            public readonly bool IsUksf;
            public string DisplayName;
            public string FilePath;
            public string Name;

            public Profile(string name = "") {
                if (string.IsNullOrEmpty(name)) {
                    SetProfileName(Global.Settings.AccountName);
                    IsUksf = true;
                    LogHandler.LogInfo($"Found profile: {Name} / {DisplayName}");
                    return;
                }

                FilePath = name;
                Name = Path.GetFileNameWithoutExtension(name);
                DisplayName = Regex.Replace(Name ?? throw new InvalidOperationException(), PROFILE_JOINER, ".");
                IsUksf = DisplayName == Global.Settings.AccountName;
                LogHandler.LogInfo($"Found profile: {Name} / {DisplayName}");
            }

            public void SetProfileName(string name) {
                DisplayName = name;
                Name = Regex.Replace(DisplayName ?? throw new InvalidOperationException(), @"\.", PROFILE_JOINER);
                FilePath = Path.Combine(Global.Constants.PROFILE_LOCATION_OTHER, Name, $"{Name}{PROFILE_EXTENSION}");
            }

            public static string FormatFile(string name) {
                name = Regex.Replace(name ?? throw new InvalidOperationException(), @"\.", PROFILE_JOINER);
                return Path.Combine(Global.Constants.PROFILE_LOCATION_OTHER, name, $"{name}{PROFILE_EXTENSION}");
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Rank {
#pragma warning disable 649
            public string Abbreviation;
#pragma warning restore 649
        }
    }
}
