using CryptoBalls.IniFiles;
using StaticExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBallsTest {

  [TestClass]
  public class IniFilesTests {

    [TestMethod]
    public void IniFilesTest() {

      string fileFolder = $"{DllExt.MMCommonsFolder()}";
      string filePath = $"{fileFolder}\\TestIni.Ini";
      if (!Directory.Exists(fileFolder + "\\")) {
        Directory.CreateDirectory(fileFolder + "\\");
      }
      Console.WriteLine($"Filepath: {filePath}");

      if (File.Exists(filePath)) { 
        File.Delete(filePath);
        Console.WriteLine($"Deleted: {filePath}");
      }

      string[] Values = new string[] { "TestStringA", "TestStringb", "TestStringC", "TestStringD", "TestStringE", "TestStringF", };

      IniFile iniFile = IniFile.FromFile(filePath);
      iniFile["TestSectionA"]["KeyA"] = Values[0];
      iniFile["TestSectionA"]["KeyB"] = Values[1];
      iniFile["TestSectionB"]["KeyC"] = Values[2];
      iniFile["TestSectionB"]["KeyD"] = Values[3];
      iniFile["TestSectionC"]["KeyE"] = Values[4];
      iniFile["TestSectionC"]["KeyF"] = Values[5];
      iniFile.Save(filePath);


      IniFile iniFile2 = IniFile.FromFile(filePath);

      int itemcount = 0;
      foreach (string name in iniFile2.GetSectionNames()) {
        foreach (string key in iniFile2[name].GetKeys()) {
          itemcount++;
        }
      }
      Assert.AreEqual(6, itemcount);


      Assert.AreEqual(Values[0], iniFile2["TestSectionA"]["KeyA"]);
      Assert.AreEqual(Values[1], iniFile2["TestSectionA"]["KeyB"]);
      Assert.AreEqual(Values[2], iniFile2["TestSectionB"]["KeyC"]);
      Assert.AreEqual(Values[3], iniFile2["TestSectionB"]["KeyD"]);
      Assert.AreEqual(Values[4], iniFile2["TestSectionC"]["KeyE"]);
      Assert.AreEqual(Values[5], iniFile2["TestSectionC"]["KeyF"]);

      iniFile2["TestSectionA"].DeleteKey("KeyA");
      iniFile2["TestSectionA"].DeleteKey("KeyB");
      iniFile2["TestSectionB"].DeleteKey("KeyC");
      iniFile2["TestSectionB"].DeleteKey("KeyD");
      iniFile2["TestSectionC"].DeleteKey("KeyE");
      iniFile2["TestSectionC"].DeleteKey("KeyF");
      iniFile2.Save(filePath);

      IniFile iniFile3 = IniFile.FromFile(filePath);
      int itemcount3 = 0;
      foreach (string name in iniFile3.GetSectionNames()) {
        foreach (string key in iniFile3[name].GetKeys()) {
          itemcount3++;
        }
      }
      Assert.AreEqual(0, itemcount3);

    }
  }
}
