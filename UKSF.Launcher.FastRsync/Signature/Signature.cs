using System.Collections.Generic;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Signature {
    public class Signature {
        public Signature(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm) {
            HashAlgorithm = hashAlgorithm;
            RollingChecksumAlgorithm = rollingChecksumAlgorithm;
            Chunks = new List<ChunkSignature>();
        }

        public List<ChunkSignature> Chunks { get; }

        public IHashAlgorithm HashAlgorithm { get; }
        public IRollingChecksum RollingChecksumAlgorithm { get; }
    }
}
