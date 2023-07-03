using StaticExtensions;
using CryptoBalls.Models;

namespace CryptoBallsTest {
  [TestClass]
  public class VariablesTest {
    [TestMethod]
    public void TestVariables() {
      string fileFolder = $"{DllExt.MMCommonsFolder()}";
      string filePath = $"{fileFolder}\\TestVar.Ini";
      if (!Directory.Exists(fileFolder + "\\")) {
        Directory.CreateDirectory(fileFolder + "\\");
      }
      Console.WriteLine($"Filepath: {filePath}");

      if (File.Exists(filePath)) {
        File.Delete(filePath);
        Console.WriteLine($"Deleted: {filePath}");
      }

      Variables Settings = new() {
        FileName = filePath
      };

      Settings["TestA"].Value = "Test A";
      Settings["TestB"].Value = "Test B";

      string testa = Settings["TestA"].Value;
      string testb = Settings["TestB"].Value;
      Assert.AreEqual("Test A", testa);
      Assert.AreEqual("Test B", testb);

      Variables SettingsB = new() {
        FileName = filePath
      };

      string test2a = SettingsB["TestA"].Value;
      string test2b = SettingsB["TestB"].Value;      
      Assert.AreEqual("Test A", test2a);
      Assert.AreEqual("Test B", test2b);      

    }
  }
}
