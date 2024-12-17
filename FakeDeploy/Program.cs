using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace FakeDeploy {
    class Program {

        private static string root = @"E:\Workspace\UKSF.Launcher\Test";
        private static string build = @"E:\Workspace\UKSF.Launcher\UKSF.Launcher\Release";

        static void Main(string[] args) {
            bool skip = false;
            if (args.Length > 0) {
                if (args[0] == "-s") {
                    skip = true;
                }
            }
            string launcherPath = Path.Combine(build, "Launcher", "netcoreapp3.0", "win10-x64");
            string updaterPath = Path.Combine(build, "Updater");
            string setupPath = Path.Combine(build, "UKSF Launcher Setup.msi");

            File.Copy(setupPath, Path.Combine(root, "UKSF Launcher Setup.msi"), true);
            Directory.CreateDirectory(Path.Combine(root, "Launcher"));
            Directory.CreateDirectory(Path.Combine(root, "Updater"));
            foreach (string file in Directory.EnumerateFiles(launcherPath)) {
                File.Copy(file, Path.Combine(root, "Launcher", Path.GetFileName(file)), true);
            }
            foreach (string file in Directory.EnumerateFiles(updaterPath)) {
                File.Copy(file, Path.Combine(root, "Updater", Path.GetFileName(file)), true);
            }

            string token;
            using (HttpClient client = new HttpClient()) {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(new { email = "contact.tim.here@gmail.com", password = "jWyvAotfJnQtBVtUuygfaOphEQVZ" }), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync("http://localhost:5000/login", content).Result;
                string responseString = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                token = responseString.Replace("\"", "");
            }

            string version = FileVersionInfo.GetVersionInfo(Path.Combine(root, "Launcher", "UKSF Launcher.exe")).FileVersion;
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpContent content = new StringContent(JsonConvert.SerializeObject(new { version }), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync("http://localhost:5000/launcher/version", content).Result;
                response.EnsureSuccessStatusCode();
                response.Content.ReadAsStringAsync().Wait();
            }

            if (skip) return;
            Process setup = new Process { StartInfo = { UseShellExecute = false, FileName = "msiexec", Arguments = $"/i \"{Path.Combine(root, "UKSF Launcher Setup.msi")}\"" } };
            setup.Start();
            setup.WaitForExit();
            if (setup.ExitCode != 0) return;

            Process launcher = new Process { StartInfo = { UseShellExecute = false, FileName = @"C:\Users\Tim\AppData\Local\Programs\UKSF Launcher\UKSF Launcher.exe" } };
            launcher.Start();
        }
    }
}
