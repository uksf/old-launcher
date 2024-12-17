using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace UKSF.Launcher.FastRsync.Hash {
    public class HashAlgorithmWrapper : IHashAlgorithm {
        private readonly HashAlgorithm _algorithm;

        public HashAlgorithmWrapper(string name, HashAlgorithm algorithm) {
            Name = name;
            _algorithm = algorithm;
        }

        public string Name { get; }
        public int HashLength => _algorithm.HashSize / 8;

        public byte[] ComputeHash(Stream stream) => _algorithm.ComputeHash(stream);

        public Task<byte[]> ComputeHashAsync(Stream stream) => Task.FromResult(_algorithm.ComputeHash(stream));

        public byte[] ComputeHash(byte[] buffer, int offset, int length) => _algorithm.ComputeHash(buffer, offset, length);
    }
}
