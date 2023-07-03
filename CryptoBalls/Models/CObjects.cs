using CryptoBalls.IniFiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaticExtensions;
using System.Collections.ObjectModel;

namespace CryptoBalls.Models {

  public interface ICObjectItem { 
    string Key { get; set; }
    string Value { get; set; }
  }
  public interface ICObject : IDictionary<string, ICObjectItem?> {
    public bool Contains(string key);
    public new ICObjectItem? this[string key] { get; set; } 
    public new void Remove(string key);
  }
  
  public class CObject : ConcurrentDictionary<string, ICObjectItem?>, ICObject {
    public CObject() : base() { }
    public virtual Boolean Contains(String key) { 
      try { 
        return base[key] is not null; 
      } catch { 
        return false; 
      } 
    }
    public virtual new ICObjectItem? this[string key] {
      get { return Contains(key) ? base[key] : null; }
      set { if (value != null) { base[key] = value; } else { Remove(key); } }
    }
    public virtual void Remove(string key) { if (Contains(key)) { _ = base.TryRemove(key, out _); } }
  }


}


