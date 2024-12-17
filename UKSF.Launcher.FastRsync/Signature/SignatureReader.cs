using System.Collections;
using System.IO;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Signature {
    public class SignatureReader : ISignatureReader {
        private readonly BinaryReader _reader;

        public SignatureReader(Stream stream) => _reader = new BinaryReader(stream);

        public Signature ReadSignature() {
            byte[] header = _reader.ReadBytes(BinaryFormat.SIGNATURE_HEADER.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.SIGNATURE_HEADER, header)) {
                throw new InvalidDataException("The signature file appears to be corrupt.");
            }

            byte version = _reader.ReadByte();
            if (version != BinaryFormat.VERSION) {
                throw new InvalidDataException("The signature file uses a newer file format than this program can handle.");
            }

            string hashAlgorithm = _reader.ReadString();
            string rollingChecksumAlgorithm = _reader.ReadString();

            byte[] endOfMeta = _reader.ReadBytes(BinaryFormat.END_OF_METADATA.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.END_OF_METADATA, endOfMeta)) {
                throw new InvalidDataException("The signature file appears to be corrupt.");
            }

            IHashAlgorithm hashAlgo = SupportedAlgorithms.Hashing.Create(hashAlgorithm);
            Signature signature = new Signature(hashAlgo, SupportedAlgorithms.Checksum.Create(rollingChecksumAlgorithm));

            int expectedHashLength = hashAlgo.HashLength;
            long start = 0;

            long fileLength = _reader.BaseStream.Length;
            long remainingBytes = fileLength - _reader.BaseStream.Position;
            int signatureSize = sizeof(ushort) + sizeof(uint) + expectedHashLength;
            if (remainingBytes % signatureSize != 0) {
                throw new InvalidDataException("The signature file appears to be corrupt; at least one chunk has data missing.");
            }

            while (_reader.BaseStream.Position < fileLength - 1) {
                short length = _reader.ReadInt16();
                uint checksum = _reader.ReadUInt32();
                byte[] chunkHash = _reader.ReadBytes(expectedHashLength);

                signature.Chunks.Add(new ChunkSignature {StartOffset = start, Length = length, RollingChecksum = checksum, Hash = chunkHash});

                start += length;
            }

            return signature;
        }
    }
}
