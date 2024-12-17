using System.Collections;
using System.Collections.Generic;
using System.IO;
using UKSF.Launcher.FastRsync.Core;
using UKSF.Launcher.FastRsync.Hash;
using UKSF.Launcher.FastRsync.Signature;

namespace UKSF.Launcher.FastRsync.Delta {
    public class DeltaBuilder {
        private const int READ_BUFFER_SIZE = 4 * 1024 * 1024;

        public void BuildDelta(Stream newFileStream, ISignatureReader signatureReader, IDeltaWriter deltaWriter) {
            Signature.Signature signature = signatureReader.ReadSignature();
            List<ChunkSignature> chunks = signature.Chunks;

            byte[] newFileHash = signature.HashAlgorithm.ComputeHash(newFileStream);
            newFileStream.Seek(0, SeekOrigin.Begin);

            deltaWriter.WriteMetadata(signature.HashAlgorithm, newFileHash);

            chunks = OrderChunksByChecksum(chunks);

            int minChunkSize;
            int maxChunkSize;
            Dictionary<uint, int> chunkMap = CreateChunkMap(chunks, out maxChunkSize, out minChunkSize);

            byte[] buffer = new byte[READ_BUFFER_SIZE];
            long lastMatchPosition = 0;

            while (true) {
                long startPosition = newFileStream.Position;
                int read = newFileStream.Read(buffer, 0, buffer.Length);
                if (read < 0) {
                    break;
                }

                IRollingChecksum checksumAlgorithm = signature.RollingChecksumAlgorithm;
                uint checksum = 0;

                int remainingPossibleChunkSize = maxChunkSize;

                for (int i = 0; i < read - minChunkSize + 1; i++) {
                    long readSoFar = startPosition + i;

                    int remainingBytes = read - i;
                    if (remainingBytes < maxChunkSize) {
                        remainingPossibleChunkSize = minChunkSize;
                    }

                    if (i == 0 || remainingBytes < maxChunkSize) {
                        checksum = checksumAlgorithm.Calculate(buffer, i, remainingPossibleChunkSize);
                    } else {
                        byte remove = buffer[i - 1];
                        byte add = buffer[i + remainingPossibleChunkSize - 1];
                        checksum = checksumAlgorithm.Rotate(checksum, remove, add, remainingPossibleChunkSize);
                    }

                    if (readSoFar - (lastMatchPosition - remainingPossibleChunkSize) < remainingPossibleChunkSize) {
                        continue;
                    }

                    if (!chunkMap.ContainsKey(checksum)) {
                        continue;
                    }

                    int startIndex = chunkMap[checksum];

                    for (int j = startIndex; j < chunks.Count && chunks[j].RollingChecksum == checksum; j++) {
                        ChunkSignature chunk = chunks[j];

                        byte[] hash = signature.HashAlgorithm.ComputeHash(buffer, i, remainingPossibleChunkSize);

                        if (!StructuralComparisons.StructuralEqualityComparer.Equals(hash, chunks[j].Hash)) continue;
                        readSoFar = readSoFar + remainingPossibleChunkSize;

                        long missing = readSoFar - lastMatchPosition;
                        if (missing > remainingPossibleChunkSize) {
                            deltaWriter.WriteDataCommand(newFileStream, lastMatchPosition, missing - remainingPossibleChunkSize);
                        }

                        deltaWriter.WriteCopyCommand(new DataRange(chunk.StartOffset, chunk.Length));
                        lastMatchPosition = readSoFar;
                        break;
                    }
                }

                if (read < buffer.Length) {
                    break;
                }

                newFileStream.Position = newFileStream.Position - maxChunkSize + 1;
            }

            if (newFileStream.Length != lastMatchPosition) {
                deltaWriter.WriteDataCommand(newFileStream, lastMatchPosition, newFileStream.Length - lastMatchPosition);
            }

            deltaWriter.Finish();
        }

        private static List<ChunkSignature> OrderChunksByChecksum(List<ChunkSignature> chunks) {
            chunks.Sort(new ChunkSignatureChecksumComparer());
            return chunks;
        }

        private Dictionary<uint, int> CreateChunkMap(IList<ChunkSignature> chunks, out int maxChunkSize, out int minChunkSize) {
            maxChunkSize = 0;
            minChunkSize = int.MaxValue;

            Dictionary<uint, int> chunkMap = new Dictionary<uint, int>();
            for (int i = 0; i < chunks.Count; i++) {
                ChunkSignature chunk = chunks[i];
                if (chunk.Length > maxChunkSize) maxChunkSize = chunk.Length;
                if (chunk.Length < minChunkSize) minChunkSize = chunk.Length;

                if (!chunkMap.ContainsKey(chunk.RollingChecksum)) {
                    chunkMap[chunk.RollingChecksum] = i;
                }
            }

            return chunkMap;
        }
    }
}
