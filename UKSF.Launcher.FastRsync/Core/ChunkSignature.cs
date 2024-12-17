using System;

namespace UKSF.Launcher.FastRsync.Core {
    public class ChunkSignature {
        public byte[] Hash; // depending on hash (20 for SHA1, 8 for xxHash64)
        public short Length; // 2
        public uint RollingChecksum; // 4
        public long StartOffset; // 8 (but not included in the file on disk)

        public override string ToString() => $"{StartOffset,6}:{Length,6} |{RollingChecksum,20}| {BitConverter.ToString(Hash).ToLowerInvariant().Replace("-", "")}";
    }
}
