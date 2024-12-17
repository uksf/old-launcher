using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UKSF.Launcher.Updater {
    internal static class Program {
#if DEBUG
        public const string URL = "http://localhost:5000";
#else
        public const string URL = "https://api.uk-sf.co.uk";
#endif

        private static string token;
        private static string launcherDirectory;

        private static void Main(string[] args) {
            if (string.IsNullOrEmpty(args[0])) {
                return;
            }

            token = args[0];
            launcherDirectory = Environment.CurrentDirectory;

            Task.Run(async () => {
                await CloseLauncher();
                await Update();
                Relaunch();
            })
                .Wait();
        }

        private static async Task CloseLauncher() {
            Process[] processes = Process.GetProcessesByName("UKSF Launcher");
            while (processes.Length > 0) {
                foreach (Process process in processes) {
                    process.WaitForExit(500);
                    process.Refresh();
                    if (!process.HasExited) {
                        process.Kill();
                    }
                }

                processes = Process.GetProcessesByName("UKSF Launcher");
            }

            await Task.Delay(500);
        }

        private static async Task Update() {
            List<LauncherFile> currentFiles = (from file in Directory.EnumerateFiles(launcherDirectory) let fileName = Path.GetFileName(file) let version = FileVersionInfo.GetVersionInfo(file).FileVersion select new LauncherFile { FileName = fileName, VERSION = version }).ToList();
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpContent content = new StringContent(JsonConvert.SerializeObject(new { files = currentFiles }), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"{URL}/launcher/download/update", content);
                response.EnsureSuccessStatusCode();
                using (Stream stream = await response.Content.ReadAsStreamAsync()) {
                    using (ZipArchive zipArchive = new ZipArchive(stream)) {
                        zipArchive.Entries.Where(x => x.Name == string.Empty && !Directory.Exists(Path.Combine(launcherDirectory, x.FullName))).ToList().ForEach(o => Directory.CreateDirectory(Path.Combine(launcherDirectory, o.FullName)));
                        zipArchive.Entries.Where(x => x.Name != string.Empty).ToList().ForEach(e => e.ExtractToFile(Path.Combine(launcherDirectory, e.FullName), true));
                    }
                }
            }

            string deletedPath = Path.Combine(launcherDirectory, "deleted");
            if (File.Exists(deletedPath)) {
                List<string> deletedFiles = File.ReadAllLines(deletedPath).ToList();
                foreach (string deletedFile in deletedFiles) {
                    string deletedFilePath = Path.Combine(launcherDirectory, deletedFile);
                    if (File.Exists(deletedFilePath)) {
                        File.Delete(deletedFilePath);
                    }
                }
            }
        }

        private static void Relaunch() {
            Process launcherProcess = new Process { StartInfo = { UseShellExecute = false, FileName = Path.Combine(launcherDirectory, "UKSF Launcher.exe"), Arguments = "-u" } };
            launcherProcess.Start();
            Environment.Exit(0);
        }
    }

    internal class LauncherFile {
        public string FileName;
        public string VERSION;
    }
}
