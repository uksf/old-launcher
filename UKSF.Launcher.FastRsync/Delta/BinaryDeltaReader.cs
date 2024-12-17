using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Delta {
    public class BinaryDeltaReader : IDeltaReader {
        private readonly BinaryReader _reader;
        private byte[] _expectedHash;
        private IHashAlgorithm _hashAlgorithm;
        private bool _hasReadMetadata;

        public BinaryDeltaReader(Stream stream) => _reader = new BinaryReader(stream);

        public byte[] ExpectedHash {
            get {
                EnsureMetadata();
                return _expectedHash;
            }
        }

        public IHashAlgorithm HashAlgorithm {
            get {
                EnsureMetadata();
                return _hashAlgorithm;
            }
        }

        public void Apply(Action<byte[]> writeData, Action<long, long> copy) {
            long fileLength = _reader.BaseStream.Length;

            EnsureMetadata();

            while (_reader.BaseStream.Position != fileLength) {
                byte b = _reader.ReadByte();

                if (b == BinaryFormat.COPY_COMMAND) {
                    long start = _reader.ReadInt64();
                    long length = _reader.ReadInt64();
                    copy(start, length);
                } else if (b == BinaryFormat.DATA_COMMAND) {
                    long length = _reader.ReadInt64();
                    long soFar = 0;
                    while (soFar < length) {
                        byte[] bytes = _reader.ReadBytes((int) Math.Min(length - soFar, 1024 * 1024 * 4));
                        soFar += bytes.Length;
                        writeData(bytes);
                    }
                }
            }
        }

        public async Task ApplyAsync(Func<byte[], Task> writeData, Func<long, long, Task> copy) {
            long fileLength = _reader.BaseStream.Length;

            EnsureMetadata();

            while (_reader.BaseStream.Position != fileLength) {
                byte b = _reader.ReadByte();

                if (b == BinaryFormat.COPY_COMMAND) {
                    long start = _reader.ReadInt64();
                    long length = _reader.ReadInt64();
                    await copy(start, length).ConfigureAwait(false);
                } else if (b == BinaryFormat.DATA_COMMAND) {
                    long length = _reader.ReadInt64();
                    long soFar = 0;
                    while (soFar < length) {
                        byte[] bytes = _reader.ReadBytes((int) Math.Min(length - soFar, 1024 * 1024 * 4));
                        soFar += bytes.Length;
                        await writeData(bytes).ConfigureAwait(false);
                    }
                }
            }
        }

        private void EnsureMetadata() {
            if (_hasReadMetadata) {
                return;
            }

            _reader.BaseStream.Seek(0, SeekOrigin.Begin);

            byte[] first = _reader.ReadBytes(BinaryFormat.DELTA_HEADER.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(first, BinaryFormat.DELTA_HEADER)) {
                throw new InvalidDataException("The delta file appears to be corrupt.");
            }

            byte version = _reader.ReadByte();
            if (version != BinaryFormat.VERSION) {
                throw new InvalidDataException("The delta file uses a newer file format than this program can handle.");
            }

            string hashAlgorithmName = _reader.ReadString();
            _hashAlgorithm = SupportedAlgorithms.Hashing.Create(hashAlgorithmName);

            int hashLength = _reader.ReadInt32();
            _expectedHash = _reader.ReadBytes(hashLength);
            byte[] endOfMeta = _reader.ReadBytes(BinaryFormat.END_OF_METADATA.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.END_OF_METADATA, endOfMeta)) {
                throw new InvalidDataException("The signature file appears to be corrupt.");
            }

            _hasReadMetadata = true;
        }
    }
}
