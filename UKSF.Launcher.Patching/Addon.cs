using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Signature;

namespace UKSF.Launcher.Patching {
    internal class Addon {
        private readonly string _repoFolder;

        public readonly string FolderPath;
        public readonly string Name;
        public string FullHash;

        public Addon(string addonFolder, DirectoryInfo repoDirectory) {
            DirectoryInfo directoryInfo = new DirectoryInfo(addonFolder);
            directoryInfo.Create();
            FolderPath = directoryInfo.FullName;
            Name = directoryInfo.Name;
            _repoFolder = repoDirectory.FullName;
        }

        public void GenerateFullHash() {
            Dictionary<string, string> hashDictionary = File.ReadAllLines(Path.Combine(_repoFolder, $"{Name}.urf")).Select(line => line.Split(';'))
                                                            .ToDictionary(values => values[0], values => values[1].Split(new[] {':'})[0]);
            FullHash = Utility.ShaFromDictionary(hashDictionary);
        }

        public void GenerateAllHashes(CancellationToken token) {
            ConcurrentDictionary<string, string> hashDictionary = new ConcurrentDictionary<string, string>();
            Parallel.ForEach(new DirectoryInfo(FolderPath).EnumerateFiles("*", SearchOption.AllDirectories).ToList(), new ParallelOptions {CancellationToken = token},
                             file => hashDictionary.TryAdd(file.FullName, Utility.ShaFromFile(file.FullName)));
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(_repoFolder, $"{Name}.urf")))) {
                foreach (string key in from file in hashDictionary.Keys orderby file select file) {
                    streamWriter.WriteLine($"{key.Replace($"{FolderPath}{Path.DirectorySeparatorChar}", "")};{hashDictionary[key]}:{new FileInfo(key).Length}:{new FileInfo(key).LastWriteTime.Ticks}");
                }
            }
        }

        public void GenerateHash(string addonFile) {
            Dictionary<string, string> hashDictionary = File.ReadAllLines(Path.Combine(_repoFolder, $"{Name}.urf")).Select(line => line.Split(new[] {';'}))
                                                            .ToDictionary(values => values[0], values => values[1].Split(new[] {':'})[0]);
            string key = addonFile.Replace($"{FolderPath}{Path.DirectorySeparatorChar}", "");
            if (hashDictionary.ContainsKey(key)) {
                hashDictionary[key] = Utility.ShaFromFile(addonFile);
            } else {
                hashDictionary.Add(key, Utility.ShaFromFile(addonFile));
            }
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(_repoFolder, $"{Name}.urf")))) {
                foreach (string file in from file in hashDictionary.Keys orderby file select file) {
                    streamWriter.WriteLine($"{file};{hashDictionary[file]}:{new FileInfo(Path.Combine(FolderPath, file)).Length}:{new FileInfo(Path.Combine(FolderPath, file)).LastWriteTime.Ticks}");
                }
            }
        }

        public void RemoveHash(string addonFile) {
            Dictionary<string, string> hashDictionary = File.ReadAllLines(Path.Combine(_repoFolder, $"{Name}.urf")).Select(line => line.Split(new[] {';'}))
                                                            .ToDictionary(values => values[0], values => values[1].Split(new[] {':'})[0]);
            addonFile = addonFile.Replace($"{FolderPath}{Path.DirectorySeparatorChar}", "").Replace($"{Path.DirectorySeparatorChar}", "\\");
            using (StreamWriter streamWriter = new StreamWriter(File.Create(Path.Combine(_repoFolder, $"{Name}.urf")))) {
                foreach (string file in from file in hashDictionary.Keys where !file.Equals(addonFile) orderby file select file) {
                    FileInfo fileInfo = new FileInfo(Path.Combine(FolderPath, file));
                    if (fileInfo.Exists) {
                        streamWriter.WriteLine($"{file};{hashDictionary[file]}:{fileInfo.Length}:{fileInfo.LastWriteTime.Ticks}");
                    }
                }
            }
        }

        public string GenerateSignature(string filePath) {
            FileInfo signatureFileInfo = new FileInfo(Path.Combine(_repoFolder, Path.GetRandomFileName()));
            if (!Directory.Exists(signatureFileInfo.DirectoryName)) {
                Directory.CreateDirectory(signatureFileInfo.DirectoryName);
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(signatureFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    SignatureBuilder signatureBuilder = new SignatureBuilder {ChunkSize = 8 * 1024, HashAlgorithm = SupportedAlgorithms.Hashing.Create("MD5")};
                    signatureBuilder.Build(fileStream, new SignatureWriter(signatureStream));
                    return signatureFileInfo.FullName;
                }
            }
        }
    }
}