using System.Collections.Generic;
using NUnit.Framework;

namespace UKSF.Launcher.Patching.Tests {
    public class UtilityTests {
        [Test]
        public void UtilityTestsShaFromDictionary() {
            Dictionary<string, string> signaturesDictionary =
                new Dictionary<string, string> {{"1", "78e10ca71a8e8060f5fba1b261fdff222806bc79"}, {"2", "d09adbeb611cde10bfe1c52353cbfabc320bb5df"}};
            string sha = Utility.ShaFromDictionary(signaturesDictionary);
            Assert.AreEqual(sha, "e53c7d28f7db6642cc3ac3f89ed4a98d1e2cb7b1");
        }

        [Test]
        public void UtilityTestsSha() {
            string sha1 = Utility.ShaFromFile(@"E:\Workspace\UKSF.Launcher\UKSF.Launcher\UKSF.Launcher.Patching.Tests\test\1.txt");
            string sha2 = Utility.ShaFromFile(@"E:\Workspace\UKSF.Launcher\UKSF.Launcher\UKSF.Launcher.Patching.Tests\test\2.txt");
            Dictionary<string, string> signaturesDictionary = new Dictionary<string, string> {{"1", sha1}, {"2", sha2}};
            string sha = Utility.ShaFromDictionary(signaturesDictionary);
            Assert.AreEqual(sha, "c082728de4069d1c3747b262bf0d728e8eb009a4");
        }

        [Test]
        public void UtilityTestsShaOtherLocation() {
            string sha11 = Utility.ShaFromFile(@"E:\Workspace\UKSF.Launcher\UKSF.Launcher\UKSF.Launcher.Patching.Tests\test\1.txt");
            string sha12 = Utility.ShaFromFile(@"E:\Workspace\UKSF.Launcher\UKSF.Launcher\UKSF.Launcher.Patching.Tests\test\2.txt");
            Dictionary<string, string> signaturesDictionary1 = new Dictionary<string, string> {{"1", sha11}, {"2", sha12}};
            string dictionarySha1 = Utility.ShaFromDictionary(signaturesDictionary1);

            string sha21 = Utility.ShaFromFile(@"E:\Workspace\UKSF.Launcher\UKSF.Launcher\UKSF.Launcher.Patching.Tests\test\other\1.txt");
            string sha22 = Utility.ShaFromFile(@"E:\Workspace\UKSF.Launcher\UKSF.Launcher\UKSF.Launcher.Patching.Tests\test\other\2.txt");
            Dictionary<string, string> signaturesDictionary2 = new Dictionary<string, string> {{"1", sha21}, {"2", sha22}};
            string dictionarySha2 = Utility.ShaFromDictionary(signaturesDictionary2);

            Assert.AreEqual(dictionarySha1, dictionarySha2);
        }
    }
}
