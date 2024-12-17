using System;
using System.IO;
using System.Linq;

namespace UKSF.Launcher.ServerService {
    internal static class RepoHandler {
        private const string REPOSITORY_LOCATION = @"C:\Users\root\Documents\Launcher\repos";

        public static bool Create(string name, Action<string> progress) {
            try {
//                RepoServer repo = new RepoServer(Path.Combine(REPOSITORY_LOCATION, name), name, progress);
//                progress.Invoke($"Creating '{name}' repository");
//                repo.CreateRepo();
                progress.Invoke("Creation complete");
            } catch (Exception exception) {
                progress.Invoke($"Error: {exception}");
                return false;
            }
            return true;
        }

        public static bool Update(string name, Action<string> progress) {
            try {
//                RepoServer repo = new RepoServer(Path.Combine(REPOSITORY_LOCATION, name), name, progress);
//                progress.Invoke($"Updating '{name}' repository");
//                repo.UpdateRepo();
                progress.Invoke("Update complete");
            } catch (Exception exception) {
                progress.Invoke($"Error: {exception}");
                return false;
            }
            return true;
        }

        public static bool GetRepoData(string name, Action<string> progress) {
            try {
//                RepoServer repo = new RepoServer(Path.Combine(REPOSITORY_LOCATION, name), name, progress);
//                repo.GetRepoData();
            } catch (Exception exception) {
                progress.Invoke($"Error: {exception}");
                return false;
            }
            return true;
        }

        public static string BuildDelta(string name, string path, string relativePath, string signaturePath, Action<string> progress) {
            string deltaPath = $"{path}::";
            try {
//                RepoServer repo = new RepoServer(Path.Combine(REPOSITORY_LOCATION, name), name, progress);
//                deltaPath += repo.BuildDelta(relativePath, Path.Combine(REPOSITORY_LOCATION, name, ".repo", signaturePath));
            } catch (Exception exception) {
                progress.Invoke($"Error: {exception}");
            }
            return deltaPath;
        }

        public static bool DeleteDelta(string path, Action<string> progress) {
            path = Path.Combine(REPOSITORY_LOCATION, path);
            try {
                if (File.Exists(path)) {
                    File.Delete(path);
                    if (Directory.GetFiles(Path.GetDirectoryName(path)).Length == 0) {
                        Directory.Delete(Path.GetDirectoryName(path));
                    }
                }
            } catch (Exception exception) {
                progress.Invoke($"Error: {exception}");
                return false;
            }
            return true;
        }

        public static void CleanRepos() {
            foreach (string file in Directory.GetFiles(REPOSITORY_LOCATION, "*", SearchOption.AllDirectories).Where(file => file.Contains(".repo") && !file.Contains(".urf"))) {
                File.Delete(file);
            }
        }
    }
}