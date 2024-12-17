using System.IO;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Delta {
    public interface IDeltaWriter {
        void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash);
        void WriteCopyCommand(DataRange segment);
        void WriteDataCommand(Stream source, long offset, long length);
        void Finish();
    }
}
