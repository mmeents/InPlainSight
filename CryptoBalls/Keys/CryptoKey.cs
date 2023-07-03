using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using StaticExtensions;
using CryptoBalls.Exceptions;

namespace CryptoBalls.Keys {

  public interface IHasCryptoKey { 
    bool HasCryptoKey { get; }
  }
  public interface ICryptoKey : IHasCryptoKey {
    public void SetCryptoKey(string cryptoKey);
    public Task<string> ToCipherStringAsync(string messageToEncode);
    public Task<byte[]> ToCipherBitsAsync(string messageToEncode);
    public Task<string> AsDecipherStringAsync(string encodedMessage);
    public Task<byte[]> AsDecipherBitsAsync(string encodedMessage);
  }

  public class CryptoKey : ICryptoKey {
    private readonly byte[] _salt = Encoding.UTF8.GetBytes("7B381455F3BF4F7A8A61EF8A04681C4B", 0, 16);
    private byte[]? _key; 
    private byte[]? _iv;
    private bool _hasKey = false;
    public bool HasCryptoKey => _hasKey;
    public void SetCryptoKey(string cryptoKey) {            
      if (string.IsNullOrEmpty( cryptoKey )) throw new ArgumentNullException(nameof(cryptoKey));
      using var pdb = new Rfc2898DeriveBytes(cryptoKey, _salt, 100000, HashAlgorithmName.SHA256 );      
      _key = pdb.GetBytes(32);
      _iv = pdb.GetBytes(16);
      _hasKey = true;
    }
    
    public async Task<string> ToCipherStringAsync(string messageToEncode) {
      if (string.IsNullOrEmpty( messageToEncode )) throw new ArgumentNullException(nameof(messageToEncode));
      var mb = await ToCipherBitsAsync(messageToEncode);      
      return Convert.ToBase64String(mb).Replace('=', '?');
    }

    public async Task<byte[]> ToCipherBitsAsync( string messageToEncode ) {
      if (string.IsNullOrEmpty(messageToEncode)) throw new ArgumentNullException(nameof(messageToEncode));
      if (_key is null) throw new CryptoKeyNotSetException();
      if (_iv is null) throw new CryptoKeyNotSetException();
      using Aes aes = Aes.Create();
      aes.Key = _key;
      aes.IV = _iv;
      MemoryStream memoryStream = new();
      CryptoStream encodedStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
      StreamWriter writer = new(encodedStream);
      await writer.WriteAsync(messageToEncode.AsBase64Encoded());
      writer.Close();
      encodedStream.Close();
      return memoryStream.ToArray();      
    }

    public async Task<string> AsDecipherStringAsync(string encodedMessage) {
      if (string.IsNullOrEmpty(encodedMessage)) throw new ArgumentNullException(nameof(encodedMessage));
      var bits = await AsDecipherBitsAsync(encodedMessage);
      return Encoding.UTF8.GetString(bits);
    }

    public async Task<byte[]> AsDecipherBitsAsync(string encodedMessage) {
      if (string.IsNullOrEmpty(encodedMessage)) throw new ArgumentNullException(nameof(encodedMessage));
      if (_key is null) throw new CryptoKeyNotSetException();
      if (_iv is null) throw new CryptoKeyNotSetException();
      using Aes aes = Aes.Create();
      aes.Key = _key;
      aes.IV = _iv;
      var debase64decode = Convert.FromBase64String( encodedMessage.Replace('?','='));
      MemoryStream ms = new(debase64decode);
      CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
      StreamReader sr = new(cs);
      var val = await sr.ReadToEndAsync();
      return Convert.FromBase64String( val.Replace('?', '='));
    }

  }
}
