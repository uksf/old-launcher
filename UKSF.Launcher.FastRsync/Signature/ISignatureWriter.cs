using System.IO;
using System.Threading.Tasks;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Signature {
    public interface ISignatureWriter {
        void WriteMetadata(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm);
        Task WriteMetadataAsync(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm);
        void WriteChunk(ChunkSignature signature);
        Task WriteChunkAsync(ChunkSignature signature);
    }

    public class SignatureWriter : ISignatureWriter {
        private readonly BinaryWriter _signaturebw;

        public SignatureWriter(Stream signatureStream) => _signaturebw = new BinaryWriter(signatureStream);

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm) {
            _signaturebw.Write(BinaryFormat.SIGNATURE_HEADER);
            _signaturebw.Write(BinaryFormat.VERSION);
            _signaturebw.Write(hashAlgorithm.Name);
            _signaturebw.Write(rollingChecksumAlgorithm.Name);
            _signaturebw.Write(BinaryFormat.END_OF_METADATA);
        }

        public async Task WriteMetadataAsync(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm) {
            MemoryStream ms = new MemoryStream();
            BinaryWriter msbw = new BinaryWriter(ms);
            msbw.Write(BinaryFormat.SIGNATURE_HEADER);
            msbw.Write(BinaryFormat.VERSION);
            msbw.Write(hashAlgorithm.Name);
            msbw.Write(rollingChecksumAlgorithm.Name);
            msbw.Write(BinaryFormat.END_OF_METADATA);
            ms.Seek(0, SeekOrigin.Begin);

            await ms.CopyToAsync(_signaturebw.BaseStream).ConfigureAwait(false);
        }

        public void WriteChunk(ChunkSignature signature) {
            _signaturebw.Write(signature.Length);
            _signaturebw.Write(signature.RollingChecksum);
            _signaturebw.Write(signature.Hash);
        }

        public async Task WriteChunkAsync(ChunkSignature signature) {
            _signaturebw.Write(signature.Length);
            _signaturebw.Write(signature.RollingChecksum);
            await _signaturebw.BaseStream.WriteAsync(signature.Hash, 0, signature.Hash.Length).ConfigureAwait(false);
        }
    }
}
