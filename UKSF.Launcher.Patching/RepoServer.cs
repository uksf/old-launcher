using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Delta;
using UKSF.Launcher.FastRsync.Signature;

// ReSharper disable ObjectCreationAsStatement

namespace UKSF.Launcher.Patching {
    public class RepoServer : LogReporter {
        private readonly Dictionary<string, string> _repoFileDictionary;
        private readonly string _repoFilePath;

        private readonly string _repoPath;
        private readonly string _repoName;

        public RepoServer(string path, string name, Action<string> logAction) : base(logAction) {
            _repoPath = path;
            _repoName = name;
            _repoFilePath = Path.Combine(_repoPath, ".repo", ".repo.urf");
            Directory.CreateDirectory(Path.GetDirectoryName(_repoFilePath));
            _repoFileDictionary = new Dictionary<string, string>();
        }

        private List<DirectoryInfo> GetAddonFolders() {
            List<DirectoryInfo> addonFolders = new DirectoryInfo(_repoPath).EnumerateDirectories("@*").ToList();
            if (addonFolders.Count == 0) {
                throw new Exception("There are no addons in this location");
            }
            return addonFolders;
        }

        private void WriteRepoFile() {
            using (StreamWriter streamWriter = new StreamWriter(File.Create(_repoFilePath))) {
                foreach (KeyValuePair<string, string> addonPair in from pair in _repoFileDictionary orderby pair.Key select pair) {
                    string[] addonFiles = Directory.GetFiles(addonPair.Key, "*", SearchOption.AllDirectories);
                    string ticks = addonFiles.Length == 0 ? "" : Convert.ToString(addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks);
                    streamWriter.WriteLine($"{addonPair.Key};{addonPair.Value}:{addonFiles.Length}:{ticks}");
                }
                LogReporterAction.Invoke("Repo file written");
            }
        }

        public void CreateRepo() {
            LogReporterAction.Invoke($"Creating directory '{_repoPath}'");
            DirectoryInfo repoDirectory = new DirectoryInfo(Path.Combine(_repoPath, ".repo"));
            while (repoDirectory.Exists) {
                try {
                    repoDirectory.Delete(true);
                } catch (IOException) {
                    Thread.Sleep(50);
                }
                repoDirectory.Refresh();
            }
            LogReporterAction.Invoke("Creating .repo folder");
            repoDirectory.Create();
            Parallel.ForEach(GetAddonFolders().Select(addonFolder => new Addon(addonFolder.FullName, repoDirectory)), addon => {
                addon.GenerateAllHashes(new CancellationToken());
                addon.GenerateFullHash();
                LogReporterAction.Invoke($"Processed addon '{addon.Name}'");
                _repoFileDictionary.Add(addon.FolderPath, addon.FullHash);
            });
            if (_repoFileDictionary.Count == 0) {
                throw new Exception("No addons processed");
            }
            WriteRepoFile();
        }

        public void UpdateRepo() {
            if (!Directory.Exists(_repoPath)) {
                throw new Exception($"Repo '{_repoName}' does not exist");
            }
            DirectoryInfo repoDirectory = new DirectoryInfo(Path.Combine(_repoPath, ".repo"));
            if (!repoDirectory.Exists) {
                throw new Exception("Repo folder does not exist");
            }
            List<Addon> changedAddons = new List<Addon>();
            Dictionary<string, string[]> currentRepoDictionary = File.ReadAllLines(Path.Combine(_repoPath, ".repo", ".repo.urf")).Select(line => line.Split(new[] {';'}))
                                                                     .ToDictionary(values => values[0], values => values[1].Split(new[] {':'}));
            List<DirectoryInfo> addonFolders = GetAddonFolders();
            foreach (KeyValuePair<string, string[]> addonPair in currentRepoDictionary) {
                if (addonFolders.Count(addonFolder => addonFolder.FullName.Equals(addonPair.Key)) == 0) {
                    LogReporterAction.Invoke($"Addon deleted '{addonPair.Key}'");
                    File.Delete(Path.Combine(_repoPath, ".repo", $"{Path.GetFileName(addonPair.Key)}.urf"));
                } else {
                    string[] addonFiles = Directory.GetFiles(addonPair.Key, "*", SearchOption.AllDirectories);
                    if (Convert.ToInt32(addonPair.Value[1]) != addonFiles.Length ||
                        Convert.ToInt64(addonPair.Value[2]) != addonFiles.ToList().Max(file => new FileInfo(file).LastWriteTime).Ticks) {
                        LogReporterAction.Invoke($"Addon changed '{addonPair.Key}'");
                        changedAddons.Add(new Addon(addonPair.Key, repoDirectory));
                    } else {
                        Dictionary<string, string[]> addonDictionary = File.ReadAllLines(Path.Combine(_repoPath, ".repo", $"{Path.GetFileName(addonPair.Key)}.urf"))
                                                                           .Select(line => line.Split(new[] {';'})).ToDictionary(values => values[0], values => values[1].Split(new[] {':'}));
                        if ((from dataPair in addonDictionary
                             let addonFile =
                                 addonFiles.FirstOrDefault(file => file.Replace($"{Path.Combine(_repoPath, Path.GetFileName(addonPair.Key))}{Path.DirectorySeparatorChar}", "")
                                                                       .Equals(dataPair.Key))
                             where !string.IsNullOrEmpty(addonFile)
                             where Convert.ToInt64(dataPair.Value[1]) != new FileInfo(addonFile).Length ||
                                   Convert.ToInt64(dataPair.Value[2]) != new FileInfo(addonFile).LastWriteTime.Ticks
                             select addonFile).Any()) {
                            LogReporterAction.Invoke($"Addon changed '{addonPair.Key}'");
                            changedAddons.Add(new Addon(addonPair.Key, repoDirectory));
                        } else {
                            _repoFileDictionary.Add(addonPair.Key, addonPair.Value[0]);
                        }
                    }
                }
            }
            changedAddons.AddRange(addonFolders.Where(addonFolder => !currentRepoDictionary.ContainsKey(addonFolder.FullName))
                                               .Select(addonFolder => new Addon(addonFolder.FullName, repoDirectory)));

            if (changedAddons.Count == 0) {
                LogReporterAction.Invoke("No addons changed");
            }
            foreach (Addon changedAddon in changedAddons) {
                changedAddon.GenerateAllHashes(new CancellationToken());
                changedAddon.GenerateFullHash();
                LogReporterAction.Invoke($"Processed addon '{changedAddon.Name}'");
                if (_repoFileDictionary.ContainsKey(changedAddon.FolderPath)) {
                    _repoFileDictionary[changedAddon.FolderPath] = changedAddon.FullHash;
                } else {
                    _repoFileDictionary.Add(changedAddon.FolderPath, changedAddon.FullHash);
                }
            }
            WriteRepoFile();
        }

        public void GetRepoData() {
            if (!Directory.Exists(_repoPath)) {
                throw new Exception($"Repo '{_repoName}' does not exist");
            }
            DirectoryInfo repoDirectory = new DirectoryInfo(Path.Combine(_repoPath, ".repo"));
            if (!repoDirectory.Exists) {
                throw new Exception("Repo file does not exist");
            }
            string[] repoFileLines = File.ReadAllLines(Path.Combine(_repoPath, ".repo", ".repo.urf"));
            LogReporterAction.Invoke(repoFileLines.Aggregate("command::repodata", (current, line) => string.Join("::", current, line)));
        }

        public string BuildDelta(string addonPath, string signaturePath) {
            DeltaBuilder delta = new DeltaBuilder();
            string deltaPath = Path.Combine(_repoPath, ".repo", Path.GetRandomFileName());
            try {
                using (FileStream updatedStream = new FileStream(Path.Combine(_repoPath, addonPath), FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream signatureStream = new FileStream(signaturePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        using (FileStream deltaStream = new FileStream(deltaPath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                            delta.BuildDelta(updatedStream, new SignatureReader(signatureStream), new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
                        }
                    }
                }
                File.Delete(signaturePath);
                return Path.GetFileName(deltaPath);
            } catch (Exception) {
                if (File.Exists(deltaPath)) {
                    File.Delete(deltaPath);
                }
                return "";
            }
        }
    }
}