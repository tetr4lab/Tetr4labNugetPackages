namespace Tetr4lab;

/// <summary>結果の状態</summary>
public enum Status {
    /// <summary>成功</summary>
    Success = default,
    /// <summary>不詳の失敗</summary>
    Unknown,
    /// <summary>エントリの消失</summary>
    MissingEntry,
    /// <summary>エントリの重複</summary>
    DuplicateEntry,
    /// <summary>タイムアウト</summary>
    CommandTimeout,
    /// <summary>バージョンの不整合</summary>
    VersionMismatch,
    /// <summary>外部キー制約の違反</summary>
    ForeignKeyConstraintFails,
    /// <summary>デッドロック</summary>
    DeadlockFound,
    /// <summary>サイズ超過</summary>
    DataTooLong,
}

/// <summary>結果の状態と値</summary>
public class Result<T> {
    /// <summary>状態</summary>
    public Status Status { get; set; }
    /// <summary>値</summary>
    public T Value { get; set; } = default!;
    /// <summary>状態名</summary>
    public string StatusName => Status.GetName ();
    /// <summary>コンストラクタ</summary>
    public Result () { }
    /// <summary>コンストラクタ</summary>
    public Result (Status status, T value) { Status = status; Value = value; }
    /// <summary>成功である</summary>
    public bool IsSuccess => Status == Status.Success;
    /// <summary>失敗である</summary>
    public bool IsFailure => Status != Status.Success;
    /// <summary>致命的である</summary>
    public bool IsFatal => Status == Status.CommandTimeout || Status == Status.DeadlockFound;
    /// <summary>成功なら値、失敗なら例外</summary>
    public T ValueOrThrow => IsSuccess ? Value : throw new NotSupportedException (Status.ToString ());
    /// <summary>逆引き</summary>
    public Exception Exception => Status.GetException ();
    /// <summary>文字列化</summary>
    public override string ToString () => $"{{{Status}: {Value}}}";
}

/// <summary>ステータスの拡張</summary>
public static class StatusHelper {
    /// <summary>結果の状態の名前</summary>
    private static readonly Dictionary<Status, string> StatusNameDictionary;
    /// <summary>コンストラクタ</summary>
    static StatusHelper () {
        StatusNameDictionary = new () {
            { Status.Success, "成功" },
            { Status.Unknown, "不詳の失敗" },
            { Status.MissingEntry, "エントリの消失" },
            { Status.DuplicateEntry, "エントリの重複" },
            { Status.CommandTimeout, "タイムアウト" },
            { Status.VersionMismatch, "バージョンの不整合" },
            { Status.ForeignKeyConstraintFails, "外部キー制約の違反" },
            { Status.DeadlockFound, "デッドロック" },
            { Status.DataTooLong, "サイズ超過" },
        };
    }
    /// <summary>結果の状態の名前</summary>
    /// <param name="status"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string GetName (this Status status)
        => StatusNameDictionary.ContainsKey (status)
        ? StatusNameDictionary [status]
        : throw new ArgumentOutOfRangeException ($"Invalid status value {status}.");
    /// <summary>結果の一覧から最初に見つかった失敗状態を返す、失敗がなければ成功を返す</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Status FirstFailedState<T> (this List<Result<T>> results)
        => results.Find (r => r.IsFatal)?.Status ?? results.Find (r => r.IsFailure)?.Status ?? Status.Success;
}
