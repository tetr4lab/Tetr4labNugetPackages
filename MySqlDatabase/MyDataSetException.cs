using MySqlConnector;

namespace Tetr4lab;

/// <summary>内部で使用する例外</summary>
[Serializable]
public class MyDataSetException : BasicDataSetException, IDataSetException {
    /// <summary>コンストラクタ</summary>
    public MyDataSetException () : base () { }
    /// <summary>コンストラクタ</summary>
    public MyDataSetException (string message) : base (message) { }
    /// <summary>コンストラクタ</summary>
    public MyDataSetException (string message, Exception innerException) : base (message, innerException) { }
    /// <summary>例外メッセージからエラーへの変換</summary>
    public new static readonly Dictionary<(Type type, string message), Status> ExceptionToErrorDictionary = new () {
            { (typeof (MyDataSetException), "Missing entry"), Status.MissingEntry },
            { (typeof (MyDataSetException), "Duplicate entry"), Status.DuplicateEntry },
            { (typeof (MyDataSetException), "The Command Timeout expired"), Status.CommandTimeout },
            { (typeof (MyDataSetException), "Version mismatch"), Status.VersionMismatch },
            { (typeof (MyDataSetException), "Cannot add or update a child row: a foreign key constraint fails"), Status.ForeignKeyConstraintFails },
            { (typeof (MyDataSetException), "Deadlock found"), Status.DeadlockFound },
            { (typeof (MySqlException), "Duplicate entry"), Status.DuplicateEntry },
            { (typeof (MySqlException), "The Command Timeout expired"), Status.CommandTimeout },
            { (typeof (MySqlException), "Version mismatch"), Status.VersionMismatch },
            { (typeof (MySqlException), "Cannot add or update a child row: a foreign key constraint fails"), Status.ForeignKeyConstraintFails },
            { (typeof (MySqlException), "Deadlock found"), Status.DeadlockFound },
            { (typeof (MySqlException), "Data too long for column"), Status.DataTooLong },
        };
    /// <summary>例外はデッドロックである</summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public new static bool IsDeadLock (Exception ex) => ex is MySqlException && ex.Message.StartsWith ("Deadlock found");
}
