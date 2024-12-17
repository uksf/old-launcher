using System;
using System.Security.Cryptography;
using System.Text;

namespace UKSF.Old.Launcher.Utility {
    public static class PasswordHandler {
        private static readonly byte[] ENTROPY = { 2, 25, 12, 76, 21, 7 };
        
        public static string DecryptPassword(string encryptedPassword) {
            byte[] encryptedText = Convert.FromBase64String(encryptedPassword);
            byte[] originalText = ProtectedData.Unprotect(encryptedText, ENTROPY, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(originalText);
        }

        public static string EncryptPassword(string password) {
            byte[] originalText = Encoding.Unicode.GetBytes(password);
            byte[] encryptedText = ProtectedData.Protect(originalText, ENTROPY, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedText);
        }
    }
}
