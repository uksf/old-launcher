using System.IO;
using System.Threading.Tasks;

namespace UKSF.Launcher.FastRsync.Hash {
    public interface IHashAlgorithm {
        int HashLength { get; }
        string Name { get; }
        byte[] ComputeHash(Stream stream);
        Task<byte[]> ComputeHashAsync(Stream stream);
        byte[] ComputeHash(byte[] buffer, int offset, int length);
    }
}
