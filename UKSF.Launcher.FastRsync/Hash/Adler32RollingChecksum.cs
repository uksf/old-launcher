namespace UKSF.Launcher.FastRsync.Hash {
    public class Adler32RollingChecksum : IRollingChecksum {
        public string Name => "Adler32";

        public uint Calculate(byte[] block, int offset, int count) {
            int a = 1;
            int b = 0;
            for (int i = offset; i < offset + count; i++) {
                byte z = block[i];
                a = (ushort) (z + a);
                b = (ushort) (b + a);
            }

            return (uint) ((b << 16) | a);
        }

        public uint Rotate(uint checksum, byte remove, byte add, int chunkSize) {
            ushort b = (ushort) ((checksum >> 16) & 0xffff);
            ushort a = (ushort) (checksum & 0xffff);

            a = (ushort) (a - remove + add);
            b = (ushort) (b - chunkSize * remove + a - 1);

            return (uint) ((b << 16) | a);
        }
    }
}
