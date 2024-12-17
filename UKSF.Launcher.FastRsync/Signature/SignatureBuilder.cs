using System.IO;
using System.Threading.Tasks;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Signature {
    public class SignatureBuilder {
        public const int DEFAULT_CHUNK_SIZE = 8 * 1024;
        public const int MAXIMUM_CHUNK_SIZE = 128 * 1024;
        public const int MINIMUM_CHUNK_SIZE = 128;

        public SignatureBuilder() : this(SupportedAlgorithms.Hashing.Default(), SupportedAlgorithms.Checksum.Default()) { }

        public SignatureBuilder(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm) {
            HashAlgorithm = hashAlgorithm;
            RollingChecksumAlgorithm = rollingChecksumAlgorithm;
            ChunkSize = DEFAULT_CHUNK_SIZE;
        }

        public int ChunkSize { get; set; }

        public IHashAlgorithm HashAlgorithm { get; set; }

        public IRollingChecksum RollingChecksumAlgorithm { get; set; }

        public void Build(Stream stream, ISignatureWriter signatureWriter) {
            WriteMetadata(stream, signatureWriter);
            WriteChunkSignatures(stream, signatureWriter);
        }

        public async Task BuildAsync(Stream stream, ISignatureWriter signatureWriter) {
            await WriteMetadataAsync(stream, signatureWriter).ConfigureAwait(false);
            await WriteChunkSignaturesAsync(stream, signatureWriter).ConfigureAwait(false);
        }

        private void WriteMetadata(Stream stream, ISignatureWriter signatureWriter) {
            stream.Seek(0, SeekOrigin.Begin);
            signatureWriter.WriteMetadata(HashAlgorithm, RollingChecksumAlgorithm);
        }

        private async Task WriteMetadataAsync(Stream stream, ISignatureWriter signatureWriter) {
            stream.Seek(0, SeekOrigin.Begin);
            await signatureWriter.WriteMetadataAsync(HashAlgorithm, RollingChecksumAlgorithm).ConfigureAwait(false);
        }

        private void WriteChunkSignatures(Stream stream, ISignatureWriter signatureWriter) {
            IRollingChecksum checksumAlgorithm = RollingChecksumAlgorithm;
            IHashAlgorithm hashAlgorithm = HashAlgorithm;

            stream.Seek(0, SeekOrigin.Begin);

            long start = 0;
            int read;
            byte[] block = new byte[ChunkSize];
            while ((read = stream.Read(block, 0, block.Length)) > 0) {
                signatureWriter.WriteChunk(new ChunkSignature {
                    StartOffset = start, Length = (short) read, Hash = hashAlgorithm.ComputeHash(block, 0, read), RollingChecksum = checksumAlgorithm.Calculate(block, 0, read)
                });

                start += read;
            }
        }

        private async Task WriteChunkSignaturesAsync(Stream stream, ISignatureWriter signatureWriter) {
            IRollingChecksum checksumAlgorithm = RollingChecksumAlgorithm;
            IHashAlgorithm hashAlgorithm = HashAlgorithm;

            stream.Seek(0, SeekOrigin.Begin);

            long start = 0;
            int read;
            byte[] block = new byte[ChunkSize];
            while ((read = await stream.ReadAsync(block, 0, block.Length).ConfigureAwait(false)) > 0) {
                await signatureWriter.WriteChunkAsync(new ChunkSignature {
                                         StartOffset = start,
                                         Length = (short) read,
                                         Hash = hashAlgorithm.ComputeHash(block, 0, read),
                                         RollingChecksum = checksumAlgorithm.Calculate(block, 0, read)
                                     })
                                     .ConfigureAwait(false);

                start += read;
            }
        }
    }
}
