using CryptoBalls.IniFiles;
using CryptoBalls.Keys;
using CryptoBalls.Exceptions;
using StaticExtensions;

namespace CryptoBalls.Models {

  public interface ICryptoVarItem : IVariableItem {     
    public new ICryptoVars Owner { get; set; }
    public new string AsChunk(); 
    public new ICryptoVarItem FromChunk(string chunk);
  } 

  public class CryptoVarItem : CVariableItem, ICryptoVarItem {
    public CryptoVarItem(ICryptoVars owner, string key, string value ) : base(owner, key, value) {
      Owner = owner;
    }
    public string encodedValue = "";
    public new ICryptoVars Owner { get; set; }
    public new string AsChunk() {
      return $"{Key.AsBase64Encoded()} {encodedValue}".AsBase64Encoded();
    }
    public new ICryptoVarItem FromChunk(string chunk) {
      string base1 = chunk.AsBase64Decoded();      
      Key = base1.ParseFirst(" ").AsBase64Decoded();
      string val = base1.ParseString(" ", 1);
      encodedValue = string.IsNullOrEmpty(val) ? "" : val;
      return this;
    }
    
    public new string Value { 
      get { 
        var cryptoKey = Owner?.EncryptionKey;
        if ((cryptoKey is not null) && cryptoKey.HasCryptoKey) {
          if (string.IsNullOrEmpty(encodedValue)) {
            return "";
          } else { 
            return Task.Run(async () => await cryptoKey.AsDecipherStringAsync(encodedValue)).Result;
          }
        }
        throw new CryptoKeyNotSetException();        
      }
      set {
        var cryptoKey = Owner?.EncryptionKey;
        if ((cryptoKey is not null) && cryptoKey.HasCryptoKey) {
          encodedValue = string.IsNullOrEmpty(value) 
            ? "" : Task.Run(async () => await cryptoKey.ToCipherStringAsync(value)).Result; 
          ValueChanged = true;         
        } 
        else 
          throw new CryptoKeyNotSetException();        
      }
    }

    private bool _valueChanged = false;
    public new bool ValueChanged {
      get { return _valueChanged; }
      set {
        _valueChanged = value;
        if (value) {
          _valueChanged = false;          
          Owner.SetVarValue(Key, this); 
        }
      }
    }
  }

  public interface ICryptoVars : IVariables {
    public ICryptoKey? EncryptionKey { get; set; }
    public void SetVarValue(string key, ICryptoVarItem? value);
    public new ICryptoVarItem GetVarValue(string key);
  }

  public class CryptoVars : Variables, ICryptoVars {
    public ICryptoKey? EncryptionKey { get; set; }
    public CryptoVars() { }
    public void SetKey(CryptoKey cryptoKey) {
      EncryptionKey = cryptoKey ?? throw new ArgumentNullException(nameof(cryptoKey));
      if (!string.IsNullOrEmpty(FileName) && File.Exists(FileName)) { 
        this.LoadValues();
      } 
    }

    public void SetVarValue(string key, ICryptoVarItem? value) {
      if (!string.IsNullOrEmpty(FileName) && (EncryptionKey is not null) && !string.IsNullOrEmpty(key)) {
        if (value == null ) {
          RemoveVar(key);
        } else {
          base.SetVarValue(key, value);
        }
      }
    }

    public override ICryptoVarItem GetVarValue(string key) {
      if (!string.IsNullOrEmpty(FileName) && (EncryptionKey is not null) && !string.IsNullOrEmpty(key)) {
        if (this.Contains(key)) {
          var valueAtBase = base.GetVarValue(key);
          if (valueAtBase is ICryptoVarItem item1) {
            return item1;
          }
        }

        IniFile f = IniFile.FromFile(FileName);
        var itemChunk = f["Variables"][key];
        ICryptoVarItem item = (itemChunk != null ?
          new CryptoVarItem(this, key, "").FromChunk(itemChunk) :
          new CryptoVarItem(this, key, ""));
        this[key] = item;
        return item;                
      }
      throw new CryptoKeyNotSetException();
    }

    public void LoadValues() {
      foreach(string key in this.GetKeys()) { 
        _ = this[key];
      }
    }
    public new ICryptoVarItem this[string key] {
      get { return GetVarValue(key); }
      set { SetVarValue(key, value); }
    }
  }
}
