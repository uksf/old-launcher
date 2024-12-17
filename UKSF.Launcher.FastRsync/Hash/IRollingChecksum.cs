namespace UKSF.Launcher.FastRsync.Hash {
    public interface IRollingChecksum {
        string Name { get; }
        uint Calculate(byte[] block, int offset, int count);
        uint Rotate(uint checksum, byte remove, byte add, int chunkSize);
    }
}
