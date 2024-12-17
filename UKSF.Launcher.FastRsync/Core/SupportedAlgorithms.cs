using System;
using System.Security.Cryptography;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Core {
    public static class SupportedAlgorithms {
        public static class Hashing {
            private static IHashAlgorithm Md5() => new HashAlgorithmWrapper("MD5", MD5.Create());
            private static IHashAlgorithm Sha1() => new HashAlgorithmWrapper("SHA1", SHA1.Create());

            private static IHashAlgorithm XxHash() => new XxHashAlgorithm();

            public static IHashAlgorithm Default() => XxHash();

            public static IHashAlgorithm Create(string algorithm) {
                switch (algorithm) {
                    case "XXH64": return XxHash();
                    case "SHA1": return Sha1();
                    case "MD5": return Md5();
                }

                throw new NotSupportedException($"The hash algorithm '{algorithm}' is not supported");
            }
        }

        public static class Checksum {
            private static IRollingChecksum Adler32Rolling() => new Adler32RollingChecksum();

            public static IRollingChecksum Default() => Adler32Rolling();

            public static IRollingChecksum Create(string algorithm) {
                if (algorithm == "Adler32") {
                    return Adler32Rolling();
                }

                throw new NotSupportedException($"The rolling checksum algorithm '{algorithm}' is not supported");
            }
        }
    }
}
