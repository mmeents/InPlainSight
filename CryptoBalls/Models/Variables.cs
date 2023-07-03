using CryptoBalls.Exceptions;
using CryptoBalls.IniFiles;
using StaticExtensions;
using System.Collections.ObjectModel;

namespace CryptoBalls.Models {
  public interface IVariableItem : ICObjectItem {
    public IVariables Owner { get; set; }
    public bool ValueChanged { get; set; }
    public string AsChunk();
    public IVariableItem FromChunk(string chunk);
  }

  public class CVariableItem : IVariableItem {
    public CVariableItem(IVariables owner, string key, string value) {
      Owner = owner;     
      Key = key;
      _value = value;
    }
    public IVariables Owner { get; set; }
    public string Key { get; set; } = string.Empty;
        
    private bool _valueChanged = false;
    public bool ValueChanged { 
      get{ return _valueChanged; } 
      set{ 
        _valueChanged = value; 
        if (value) {
          _valueChanged = false;
          Owner[Key] = this;
        }
      } 
    }

    private string _value;   
    public string Value { 
      get { return _value;} 
      set { _value = value; ValueChanged = true; } 
    }
    public string AsChunk() {
      return $"{Key.AsBase64Encoded()} {Value.AsBase64Encoded()}".AsBase64Encoded();
    }
    public virtual IVariableItem FromChunk(string chunk) {
      string base1 = chunk.AsBase64Decoded();
      _valueChanged = false;
      Key = base1.ParseFirst(" ").AsBase64Decoded();
      string val = base1.ParseString(" ", 1);
      _value = string.IsNullOrEmpty(val)? "" : val.AsBase64Decoded();
      return this;
    }
  }

  public interface IVariables : ICObject {
    public string FileName { get; set; }
    public void SetVarValue(string name, IVariableItem value);
    public IVariableItem GetVarValue(string name);
    public new IVariableItem this[string key] { get; set; }
  }

  public class Variables : CObject, IVariables {
    public Variables() { }
    public string FileName { get; set; } = string.Empty;
    public virtual void SetVarValue(string key, IVariableItem? value) {
      if (!string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(key)) {
        if (value == null ) { 
          RemoveVar(key);
        } else { 
          IniFile f = IniFile.FromFile(FileName);
          f["Variables"][key] = value.AsChunk();
          f.Save(FileName);
          base[key] = value;
          value.ValueChanged = false;   
        }
      }
    }
    public virtual IVariableItem GetVarValue(string key) {
      if (!string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(key)) {
        if (this.Contains(key)) {
          if (base[key] is IVariableItem value) {
            return value;
          }
        }

        IniFile f = IniFile.FromFile(FileName);
        var itemChunk = f["Variables"][key];
        IVariableItem item = (itemChunk != null ? 
          new CVariableItem(this, key, "").FromChunk(itemChunk) : 
          new CVariableItem(this, key, "")); 
        this[key] = item;
        return item;        
      }
      throw new CryptoKeyNotSetException();
    }

    public void RemoveVar(string key) {
      IniFile f = IniFile.FromFile(FileName);
      f["Variables"].DeleteKey(key);
      f.Save(FileName);
      if (this.Contains(key)) {
        _ = base.TryRemove(key, out _);
      }
    }

    public ReadOnlyCollection<string> GetKeys() {
      IniFile f = IniFile.FromFile(FileName);
      return f["Variables"].GetKeys();
    }

    public new IVariableItem this[string key] {
      get { return GetVarValue(key); }
      set { SetVarValue(key, value); }
    }


  }
}
