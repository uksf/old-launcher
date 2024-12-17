using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UKSF.Launcher.FastRsync.Core {
    internal class ChunkSignatureChecksumComparer : IComparer<ChunkSignature> {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public int Compare(ChunkSignature x, ChunkSignature y) {
            int comparison = x.RollingChecksum.CompareTo(y.RollingChecksum);
            return comparison == 0 ? x.StartOffset.CompareTo(y.StartOffset) : comparison;
        }
    }
}
