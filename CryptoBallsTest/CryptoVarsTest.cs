using CryptoBalls.Keys;
using CryptoBalls.Models;
using StaticExtensions;

namespace CryptoBallsTest {
  [TestClass]
  public class CryptoVarsTest {
    [TestMethod]
    public void TestCryptoVars1() {
      string fileFolder = $"{DllExt.MMCommonsFolder()}";
      string filePath = $"{fileFolder}\\TestCryptoVar.Ini";
      if (!Directory.Exists(fileFolder + "\\")) {
        Directory.CreateDirectory(fileFolder + "\\");
      }
      Console.WriteLine($"Filepath: {filePath}");

      if (File.Exists(filePath)) {
        File.Delete(filePath);
        Console.WriteLine($"Deleted: {filePath}");
      }
      CryptoKey key = new();
      key.SetCryptoKey("testCryptoKey");

      CryptoVars Settings = new() {
        FileName = filePath
      };
      Settings.SetKey(key);

      Settings["TestA"].Value = "Test A";
      Settings["TestB"].Value = "Test B";

      string testa = Settings["TestA"].Value;
      string testb = Settings["TestB"].Value;
      Assert.AreEqual("Test A", testa);
      Assert.AreEqual("Test B", testb);

      CryptoVars SettingsB = new() {
        FileName = filePath
      };
      SettingsB.SetKey(key);

      string test2a = SettingsB["TestA"].Value;
      string test2b = SettingsB["TestB"].Value;
      Assert.AreEqual("Test A", test2a);
      Assert.AreEqual("Test B", test2b);     


    }
  }
}
