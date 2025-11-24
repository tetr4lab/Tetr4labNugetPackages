using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetr4lab;

/// <summary>DataSet内部で使用する例外</summary>
[Serializable]
public class BasicDataSetException : Exception {
    /// <summary>コンストラクタ</summary>
    public BasicDataSetException () : base () { }
    /// <summary>コンストラクタ</summary>
    public BasicDataSetException (string message) : base (message) { }
    /// <summary>コンストラクタ</summary>
    public BasicDataSetException (string message, Exception innerException) : base (message, innerException) { }
    /// <summary>例外メッセージからエラーへの変換</summary>
    public virtual Dictionary<(Type type, string message), Status> ExceptionToErrorDictionary { get; } = new ();
    /// <summary>例外がエラーか判定して該当するエラー状態を出力する</summary>
    /// <param name="ex"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public virtual bool TryGetStatus (Exception ex, out Status status) {
        foreach (var pair in ExceptionToErrorDictionary) {
            if (ex.GetType () == pair.Key.type && ex.Message.StartsWith (pair.Key.message, StringComparison.CurrentCultureIgnoreCase)) {
                status = pair.Value;
                return true;
            }
        }
        status = Status.Unknown;
        return false;
    }
    /// <summary>例外はデッドロックである</summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public virtual bool IsDeadLock (Exception ex) => false;
    /// <summary>逆引き</summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public virtual Exception GetException (Status status) {
        if (ExceptionToErrorDictionary.ContainsValue (status)) {
            return new BasicDataSetException (ExceptionToErrorDictionary.First (p => p.Value == status).Key.message);
        }
        return new Exception ("Unknown exception");
    }
}
