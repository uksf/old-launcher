using System.IO;
using UKSF.Launcher.FastRsync.Delta;
using UKSF.Launcher.FastRsync.Hash;

namespace UKSF.Launcher.FastRsync.Core {
    // This decorator turns any sequential copy operations into a single operation, reducing 
    // the size of the delta file.
    // For example:
    //   Copy: 0x0000 - 0x0400
    //   Copy: 0x0401 - 0x0800
    //   Copy: 0x0801 - 0x0C00
    // Gets turned into:
    //   Copy: 0x0000 - 0x0C00
    public class AggregateCopyOperationsDecorator : IDeltaWriter {
        private readonly IDeltaWriter _decorated;
        private DataRange _bufferedCopy;

        public AggregateCopyOperationsDecorator(IDeltaWriter decorated) => _decorated = decorated;

        public void WriteDataCommand(Stream source, long offset, long length) {
            FlushCurrentCopyCommand();
            _decorated.WriteDataCommand(source, offset, length);
        }

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash) {
            _decorated.WriteMetadata(hashAlgorithm, expectedNewFileHash);
        }

        public void WriteCopyCommand(DataRange chunk) {
            if (_bufferedCopy.Length > 0 && _bufferedCopy.StartOffset + _bufferedCopy.Length == chunk.StartOffset) {
                _bufferedCopy.Length += chunk.Length;
            } else {
                FlushCurrentCopyCommand();
                _bufferedCopy = chunk;
            }
        }

        public void Finish() {
            FlushCurrentCopyCommand();
            _decorated.Finish();
        }

        private void FlushCurrentCopyCommand() {
            if (_bufferedCopy.Length <= 0) return;

            _decorated.WriteCopyCommand(_bufferedCopy);
            _bufferedCopy = new DataRange();
        }
    }
}
