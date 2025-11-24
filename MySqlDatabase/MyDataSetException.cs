using MySqlConnector;

namespace Tetr4lab;

/// <summary>内部で使用する例外</summary>
[Serializable]
public class MyDataSetException : BasicDataSetException {
    /// <summary>コンストラクタ</summary>
    public MyDataSetException () : base () { }
    /// <summary>コンストラクタ</summary>
    public MyDataSetException (string message) : base (message) { }
    /// <summary>コンストラクタ</summary>
    public MyDataSetException (string message, Exception innerException) : base (message, innerException) { }
    /// <summary>例外メッセージからエラーへの変換</summary>
    public override Dictionary<(Type type, string message), Status> ExceptionToErrorDictionary { get; } = new () {
            { (typeof (MyDataSetException), "Missing entry"), Status.MissingEntry },
            { (typeof (MyDataSetException), "Duplicate entry"), Status.DuplicateEntry },
            { (typeof (MyDataSetException), "The Command Timeout expired"), Status.CommandTimeout },
            { (typeof (MyDataSetException), "Version mismatch"), Status.VersionMismatch },
            { (typeof (MyDataSetException), "Cannot add or update a child row: a foreign key constraint fails"), Status.ForeignKeyConstraintFails },
            { (typeof (MyDataSetException), "Deadlock found"), Status.DeadlockFound },
        };
    /// <summary>例外メッセージからエラーへの変換</summary>
    public static Dictionary<(Type type, string message), Status> ExceptionToErrorDictionary2 { get; } = new () {
            { (typeof (MySqlException), "Duplicate entry"), Status.DuplicateEntry },
            { (typeof (MySqlException), "The Command Timeout expired"), Status.CommandTimeout },
            { (typeof (MySqlException), "Version mismatch"), Status.VersionMismatch },
            { (typeof (MySqlException), "Cannot add or update a child row: a foreign key constraint fails"), Status.ForeignKeyConstraintFails },
            { (typeof (MySqlException), "Deadlock found"), Status.DeadlockFound },
            { (typeof (MySqlException), "Data too long for column"), Status.DataTooLong },
        };
    /// <summary>例外がエラーか判定して該当するエラー状態を出力する</summary>
    /// <param name="ex"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool TryGetStatus2 (Exception ex, out Status status) {
        foreach (var pair in ExceptionToErrorDictionary2) {
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
    public override bool IsDeadLock (Exception ex) => ex is MySqlException && ex.Message.StartsWith ("Deadlock found");
}
