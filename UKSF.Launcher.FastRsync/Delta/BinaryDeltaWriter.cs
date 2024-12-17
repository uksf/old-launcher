using System;
using System.IO;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Delta {
    public class BinaryDeltaWriter : IDeltaWriter {
        private readonly BinaryWriter _writer;

        public BinaryDeltaWriter(Stream stream) => _writer = new BinaryWriter(stream);

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash) {
            _writer.Write(BinaryFormat.DELTA_HEADER);
            _writer.Write(BinaryFormat.VERSION);
            _writer.Write((string) hashAlgorithm.Name);
            _writer.Write(expectedNewFileHash.Length);
            _writer.Write(expectedNewFileHash);
            _writer.Write(BinaryFormat.END_OF_METADATA);
        }

        public void WriteCopyCommand(DataRange segment) {
            _writer.Write(BinaryFormat.COPY_COMMAND);
            _writer.Write(segment.StartOffset);
            _writer.Write(segment.Length);
        }

        public void WriteDataCommand(Stream source, long offset, long length) {
            _writer.Write(BinaryFormat.DATA_COMMAND);
            _writer.Write(length);

            long originalPosition = source.Position;
            try {
                source.Seek(offset, SeekOrigin.Begin);

                byte[] buffer = new byte[Math.Min((int) length, 1024 * 1024)];

                int read;
                long soFar = 0;
                while ((read = source.Read(buffer, 0, (int) Math.Min(length - soFar, buffer.Length))) > 0) {
                    soFar += read;

                    _writer.Write(buffer, 0, read);
                }
            } finally {
                source.Seek(originalPosition, SeekOrigin.Begin);
            }
        }

        public void Finish() { }
    }
}
