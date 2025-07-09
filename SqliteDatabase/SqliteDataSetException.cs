using Microsoft.Data.Sqlite;

namespace Tetr4lab;

/// <summary>内部で使用する例外</summary>
[Serializable]
public class SqliteDataSetException : BasicDataSetException {
    /// <summary>コンストラクタ</summary>
    public SqliteDataSetException () : base () { }
    /// <summary>コンストラクタ</summary>
    public SqliteDataSetException (string message) : base (message) { }
    /// <summary>コンストラクタ</summary>
    public SqliteDataSetException (string message, Exception innerException) : base (message, innerException) { }
    /// <summary>例外メッセージからエラーへの変換</summary>
    public new static readonly Dictionary<(Type type, string message), Status> ExceptionToErrorDictionary = new () {
        { (typeof (SqliteDataSetException), "Missing entry"), Status.MissingEntry },
        { (typeof (SqliteDataSetException), "Duplicate entry"), Status.DuplicateEntry },
        { (typeof (SqliteDataSetException), "The Command Timeout expired"), Status.CommandTimeout },
        { (typeof (SqliteDataSetException), "Version mismatch"), Status.VersionMismatch },
        { (typeof (SqliteDataSetException), "Cannot add or update a child row: a foreign key constraint fails"), Status.ForeignKeyConstraintFails },
        { (typeof (SqliteDataSetException), "Deadlock found"), Status.DeadlockFound },
        { (typeof (SqliteException), "UNIQUE constraint failed"), Status.DuplicateEntry },
        { (typeof (SqliteException), "locked"), Status.CommandTimeout },
        { (typeof (SqliteException), "Version mismatch"), Status.VersionMismatch },
        { (typeof (SqliteException), "FOREIGN KEY constraint failed"), Status.ForeignKeyConstraintFails },
    };
}
