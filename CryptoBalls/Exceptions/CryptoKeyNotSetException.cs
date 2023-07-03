using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBalls.Exceptions {


  /// <summary>
  ///   Example of how to create a decendent exception. 
  /// </summary>
  [Serializable]
  public class CryptoKeyNotSetException : Exception {
    public CryptoKeyNotSetException() : base() { }
    public CryptoKeyNotSetException(string message) : base(message) { }
    public CryptoKeyNotSetException(string message,  Exception innerException) : base(message, innerException) { }
    protected CryptoKeyNotSetException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public override void GetObjectData(SerializationInfo info, StreamingContext context) {
      base.GetObjectData(info, context);
    }
  }

}
