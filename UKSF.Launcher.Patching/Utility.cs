using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UKSF.Launcher.Patching {
    public static class Utility {
        public static string ShaFromDictionary(Dictionary<string, string> filesDictionary) {
            using (SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider()) {
                return HashBytesToString(sha.ComputeHash(new MemoryStream(Encoding.UTF8.GetBytes(string.Join(",", filesDictionary.Values.ToArray())))));
            }
        }

        public static string ShaFromFile(string path) {
            using (BufferedStream stream = new BufferedStream(File.OpenRead(path), 16777216)) {
                using (SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider()) {
                    return HashBytesToString(sha.ComputeHash(stream));
                }
            }
        }

        private static string HashBytesToString(IReadOnlyList<byte> hashBytes) {
            int characterArrayLength = hashBytes.Count * 2;
            char[] characterArray = new char[characterArrayLength];
            int i;
            int index = 0;
            for (i = 0; i < characterArrayLength; i += 2) {
                byte b = hashBytes[index++];
                characterArray[i] = GetHexValue(b / 16);
                characterArray[i + 1] = GetHexValue(b % 16);
            }
            return new string(characterArray);
        }

        private static char GetHexValue(int i) {
            if (i < 0 || i > 15) {
                throw new ArgumentOutOfRangeException(nameof(i), "i must be between 0 and 15.");
            }
            if (i < 10) {
                return (char) (i + '0');
            }
            return (char) (i - 10 + 'a');
        }

        public static string ByteSize(double size) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1) {
                order++;
                size = size/1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
        
    }
}