using System;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Threading.Tasks;

namespace UKSF.Launcher.FastRsync.Hash {
    public class XxHashAlgorithm : IHashAlgorithm {
        private readonly IxxHash _algorithm;

        public XxHashAlgorithm() => _algorithm = xxHashFactory.Instance.Create();
        public string Name => "XXH64";
        public int HashLength => 64 / 8;

        public byte[] ComputeHash(Stream stream) => _algorithm.ComputeHash(stream).Hash;

        public async Task<byte[]> ComputeHashAsync(Stream stream) => (await _algorithm.ComputeHashAsync(stream).ConfigureAwait(false)).Hash;

        public byte[] ComputeHash(byte[] buffer, int offset, int length) {
            byte[] data = new byte[length];
            Buffer.BlockCopy(buffer, offset, data, 0, length);
            return _algorithm.ComputeHash(data).Hash;
        }
    }
}
