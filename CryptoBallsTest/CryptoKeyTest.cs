using CryptoBalls.Keys;

namespace CryptoBallsTest {
  [TestClass]
  public class CryptoKeyTest {

    [TestMethod]
    public async Task TestMethod1Async() {
      CryptoKey key = new();
      key.SetCryptoKey("testKey");
      string message = "This is a test of a message.";
      string encoded = await key.ToCipherStringAsync(message);
      string decoded = await key.AsDecipherStringAsync(encoded);      
      Assert.AreEqual(message, decoded);
    }


    [TestMethod]
    public void TestMethod1() {
      CryptoKey key = new();
      key.SetCryptoKey("testKey");
      string message = "This is a test of a message.";
      string encoded = Task.Run(async () => await key.ToCipherStringAsync(message)).Result;
      string decoded = Task.Run(async () => await key.AsDecipherStringAsync(encoded)).Result;
      Assert.AreEqual(message, decoded);
    }

  }
}