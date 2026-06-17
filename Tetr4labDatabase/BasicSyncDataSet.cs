using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using PetaPoco;
using Tetr4lab.Auth;

namespace Tetr4lab.Db;

/// <summary>基礎的な同期データセット</summary>
public abstract class BasicSyncDataSet {

    /// <summary>待機間隔</summary>
    public virtual int WaitInterval => 1000 / 60;

    /// <summary>ロードでエラーした場合の最大試行回数</summary>
    protected virtual int MaxRetryCount => 10;

    /// <summary>ロードでエラーした場合のリトライ間隔</summary>
    protected virtual int RetryInterval => 1000 / 30;

    /// <summary>PetaPocoをDI</summary>
    public virtual Database database { get; init; }

    /// <summary>データベース名</summary>
    protected string databaseName = string.Empty;

    /// <summary>認証状態プロバイダ</summary>
    protected AuthStateProvider AuthStateProvider { get; init; } = default!;

    /// <summary>ユーザID</summary>
    public AuthedIdentity Identity { get; protected set; } = default!;

    /// <summary>ユーザID</summary>
    public string UserIdentifier => Identity?.Identifier ?? "unknown";

    /// <summary>初期化済み</summary>
    public bool IsInitialized { get; protected set; }

    /// <summary>初期化失敗</summary>
    public bool Isfailure { get; protected set; }

    /// <summary>コンストラクタ</summary>
    public BasicSyncDataSet (Database database, AuthStateProvider authStateProvider, string? key = "database") {
        this.database = database;
        AuthStateProvider = authStateProvider;
        if (!string.IsNullOrEmpty (key)) {
            var words = database.ConnectionString.Split (['=', ';']);
            var index = Array.IndexOf (words, key);
            if (index < 0 || index > words.Length) {
                Isfailure = true;
                throw new InvalidOperationException ("The database name could not be determined.");
            }
            databaseName = words [index + 1];
        }
        _ = InitializeAsync ();
    }

    /// <summary>初期化</summary>
    protected virtual async Task InitializeAsync (bool done = true) {
        var identity = await AuthStateProvider.AuthState.GetIdentityAsync ();
        if (identity is null) {
            Isfailure = true;
            throw new AuthenticationFailureException ("Identity is null");
        } else {
            Identity = identity;
            if (done) {
                IsInitialized = true;
            }
        }
    }

    /// <summary>SQLで使用するテーブル名またはカラム名を得る</summary>
    /// <param name="name">プロパティ名</param>
    /// <returns>テーブル名またはカラム名</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string GetSqlName<T> (string? name = null) where T : class {
        var type = typeof (T);
        if (name == null) {
            return type.GetCustomAttribute<PetaPoco.TableNameAttribute> ()?.Value ?? type.Name;
        } else {
            return type.GetProperty (name)?.GetCustomAttribute<PetaPoco.ColumnAttribute> ()?.Name ?? name;
        }
    }

    /// <summary>更新用カラム&amp;値SQL</summary>
    /// <remarks>ColumnでありかつResultColumn(VirtualColumn)でないプロパティだけを対象とする</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="withId"></param>
    /// <returns></returns>
    public virtual string GetSettingSql<T> (bool withId = false) where T : class {
        var result = string.Empty;
        var properties = typeof (T).GetProperties (BindingFlags.Instance | BindingFlags.Public);
        if (properties != null) {
            result = string.Join (',', Array.ConvertAll (properties, property => {
                var @virtual = property.GetCustomAttribute<VirtualColumnAttribute> ();
                var resultColumn = property.GetCustomAttribute<ResultColumnAttribute> ();
                var attribute = property.GetCustomAttribute<ColumnAttribute> ();
                return @virtual == null && resultColumn == null && attribute != null && (withId || !(attribute.Name ?? property.Name).Equals ("Id", StringComparison.OrdinalIgnoreCase))
                    ? $"`{attribute.Name ?? property.Name}`=@{property.Name}"
                    : "";
            }).ToList ().FindAll (i => i != ""));
        }
        return result;
    }

    /// <summary>追加用値SQL</summary>
    /// <remarks>ColumnでありかつResultColumn(VirtualColumn)でないプロパティだけを対象とする</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="index"></param>
    /// <param name="withId"></param>
    /// <returns></returns>
    public string GetValuesSql<T> (int index = -1, bool withId = false) where T : class {
        var result = string.Empty;
        var properties = typeof (T).GetProperties (BindingFlags.Instance | BindingFlags.Public);
        if (properties != null) {
            result = string.Join (',', Array.ConvertAll (properties, property => {
                var @virtual = property.GetCustomAttribute<VirtualColumnAttribute> ();
                var resultColumn = property.GetCustomAttribute<ResultColumnAttribute> ();
                var attribute = property.GetCustomAttribute<ColumnAttribute> ();
                return @virtual == null && resultColumn == null && attribute != null && (withId || !(attribute.Name ?? property.Name).Equals ("Id", StringComparison.OrdinalIgnoreCase))
                    ? $"@{property.Name}{(index >= 0 ? $"_{index}" : "")}"
                    : "";
            }).ToList ().FindAll (i => i != ""));
        }
        return result;
    }

    /// <summary>追加用カラムSQL</summary>
    /// <remarks>ColumnでありかつResultColumn(VirtualColumn)でないプロパティだけを対象とする</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="withId"></param>
    /// <returns></returns>
    public string GetColumnsSql<T> (bool withId = false) where T : class {
        var result = string.Empty;
        var properties = typeof (T).GetProperties (BindingFlags.Instance | BindingFlags.Public);
        if (properties != null) {
            result = string.Join (',', Array.ConvertAll (properties, property => {
                var @virtual = property.GetCustomAttribute<VirtualColumnAttribute> ();
                var resultColumn = property.GetCustomAttribute<ResultColumnAttribute> ();
                var attribute = property.GetCustomAttribute<ColumnAttribute> ();
                return @virtual == null && resultColumn == null && attribute != null && (withId || !(attribute.Name ?? property.Name).Equals ("Id", StringComparison.OrdinalIgnoreCase))
                    ? $"`{attribute.Name ?? property.Name}`"
                    : "";
            }).ToList ().FindAll (i => i != ""));
        }
        return result;
    }

    /// <summary>処理を実行しコミットする、例外またはエラーがあればロールバックする</summary>
    /// <typeparam name="T">返す値の型</typeparam>
    /// <param name="process">処理</param>
    /// <returns>成功またはエラーの状態と値のセット</returns>
    public virtual async Task<Result<T>> ProcessAndCommitAsync<T> (Func<Task<T>> process) {
        var result = default (T)!;
        await database.BeginTransactionAsync ();
        try {
            result = await process ();
            await database.CompleteTransactionAsync ();
            return new (Status.Success, result);
        }
        catch (BasicDataSetException ex) when (ex.IsDeadLock (ex)) {
            await database.AbortTransactionAsync ();
            // デッドロックならエスカレート
            throw;
        }
        catch (BasicDataSetException ex) when (ex.TryGetStatus (ex, out var status)) {
            // エラー扱いの例外
            await database.AbortTransactionAsync ();
            System.Diagnostics.Trace.WriteLine (ex);
            if (status == Status.CommandTimeout) {
                // タイムアウトならエスカレート
                throw;
            }
            return new (status, result);
        }
        catch (Exception) {
            await database.AbortTransactionAsync ();
            throw;
        }
    }

    /// <summary>最後に挿入した行番号を得るSQL</summary>
    /// <returns>行番号</returns>
    public abstract int GetLastInsertRowId ();

    /// <summary>テーブルの次の自動更新値を得る</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public abstract long GetAutoIncremantValue<T> () where T : class;


    /// <summary>キャッシュ</summary>
    protected virtual Dictionary<(Type type, string sql, object [] args), IEnumerable<object>> _cache { get; set; } = new ();

    /// <summary>キャッシュ全クリア</summary>
    public virtual void ClearCache () {
        _cache.Clear ();
    }

    /// <summary>対象型のキャッシュクリア</summary>
    /// <param name="type">モデル型</param>
    public virtual void ClearCache (Type type) {
        foreach (var param in _cache.Keys.ToList ().FindAll (x => x.type == type)) {
            _cache.Remove (param);
        }
    }

    /// <summary>単要素読み込み</summary>
    /// <remarks>
    /// (通常は、条件付きの)SQLを与えて取得した最初の1行を状態付きで返す。
    /// 1行に絞り込まれるSQLを与えることが望ましい。
    /// 内部で一般テーブル読み込みを使用する。
    /// </remarks>
    /// <typeparam name="TModel">モデル</typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="args">バインド引数</param>
    /// <returns>ステータス付きモデル</returns>
    public virtual Result<TModel?> First<TModel> (string sql, params object [] args) where TModel : BaseSyncModel<TModel>, IBaseSyncModel<TModel>, new() {
        var result = Fetch<TModel> (sql, args);
        if (result.IsSuccess && result.Value.Count > 0) {
            return new (result.Status, result.Value [0]);
        }
        return new (result.Status, null);
    }

    /// <summary>一般テーブル読み込み</summary>
    /// <remarks>
    /// SQLを与えて取得した複数行のリストを状態付きで返す。
    /// リストが空になる場合もあるが、nullにはならない。<br/>
    /// 内部でモデルのAPIを呼んでモデル独自のマッピングを行う。
    /// 結果はキャッシュされるが、複数モデルにマップされても主モデルのマークしか付かないため、副モデルの情報は古いままになる可能性がある。
    /// </remarks>
    /// <typeparam name="TModel">モデル</typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="args">バインド引数</param>
    /// <returns>ステータス付きモデルリスト</returns>
    public virtual Result<List<TModel>> Fetch<TModel> (string sql, params object [] args) where TModel : BaseSyncModel<TModel>, IBaseSyncModel<TModel>, new() {
        var cacheKey = (typeof (TModel), sql, args);
        if (_cache.ContainsKey (cacheKey)) {
            return new (Status.Success, (List<TModel>) _cache [cacheKey]);
        }
        var result = ProcessAndCommit (() => TModel.Fetch (this, sql, args));
        if (result.IsSuccess) {
            _cache [cacheKey] = result.Value;
        }
        return result;
    }

    /// <summary>単一アイテムを取得 (Idで特定)</summary>
    /// <remarks>キャッシュなし</remarks>
    /// <typeparam name="TModel">モデル</typeparam>
    /// <param name="id">ID</param>
    /// <returns>モデル</returns>
    public virtual TModel First<TModel> (long id) where TModel : BaseSyncModel<TModel>, IBaseSyncModel<TModel>, new() {
        try {
            var table = GetSqlName<TModel> ();
            var result = ProcessAndCommit (() => {
                var list = TModel.Fetch (this, $"{TModel.SelectSql} where `{table}`.`id` = @Id {TModel.OrderSql};", new { Id = id, });
                return list.Count > 0 ? list [0] : null;
            });
            if (result.IsSuccess && result.Value is not null) {
                return result.Value;
            }
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine ($"Exception: {ex.Message}\n{ex.StackTrace}");
        }
        return TModel.Create (this);
    }

    /// <summary>トランザクション化した実行</summary>
    public virtual Result<T> ExecuteScalar<T> (string sql, params object [] args) where T : struct {
        try {
            return ProcessAndCommit (() => database.ExecuteScalar<T> (sql, args));
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine ($"{ex.Message}\n{ex.StackTrace}");
            return new (Status.Unknown, default);
        }
    }

    /// <summary>トランザクション化した実行</summary>
    public virtual Result<int> Execute (string sql, params object [] args) {
        try {
            return ProcessAndCommit (() => database.Execute (sql, args));
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine ($"{ex.Message}\n{ex.StackTrace}");
        }
        return new (Status.Unknown, 0);
    }

    /// <summary>アイテムの更新サブ処理 トランザクション内専用</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual int UpdateItem<T> (T item) where T : BaseSyncModel<T>, new() {
        var table = GetSqlName<T> ();
        return database.Execute (
            @$"update `{table}` set {GetSettingSql<T> ()} where `{table}`.Id = @Id",
            item
        );
    }

    /// <summary>アイテムの更新</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual Result<int> Update<T> (T item) where T : BaseSyncModel<T>, new() {
        try {
            var result = ProcessAndCommit (() => {
                item.Modifier = UserIdentifier;
                item.Version++;
                return UpdateItem (item);
            });
            if (result.IsSuccess && result.Value <= 0) {
                result.Status = Status.MissingEntry;
            }
            ClearCache (typeof (T));
            return result;
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine ($"{ex.Message}\n{ex.StackTrace}");
        }
        ClearCache (typeof (T));
        return new (Status.Unknown, 0);
    }

    /// <summary>アイテムの更新</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public virtual Result<int> UpdateRange<T> (IEnumerable<T> items) where T : BaseSyncModel<T>, new() {
        try {
            var result = ProcessAndCommit (() => {
                var count = 0;
                foreach (var item in items) {
                    item.Modifier = UserIdentifier;
                    item.Version++;
                    count += UpdateItem (item);
                }
                return count;
            });
            if (result.IsSuccess && result.Value <= 0) {
                result.Status = Status.MissingEntry;
            }
            ClearCache (typeof (T));
            return result;
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine ($"{ex.Message}\n{ex.StackTrace}");
        }
        ClearCache (typeof (T));
        return new (Status.Unknown, 0);
    }

    /// <summary>単一アイテムの追加</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual Result<T> Add<T> (T item) where T : BaseSyncModel<T>, new() {
        try {
            var result = ProcessAndCommit (() => {
                database.Execute (
                    $"insert into `{GetSqlName<T> ()}` ({GetColumnsSql<T> ()}) values ({GetValuesSql<T> ()});",
                    item
                );
                item.Id = GetLastInsertRowId ();
                return item.Id > 0 ? 1 : 0;
            });
            if (result.IsSuccess && result.Value <= 0) {
                result.Status = Status.MissingEntry;
            }
            if (result.IsFailure) {
                item.Id = default;
            }
            ClearCache (typeof (T));
            return new (result.Status, item);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine ($"{ex.Message}\n{ex.StackTrace}");
        }
        ClearCache (typeof (T));
        item.Id = 0L;
        return new (Status.Unknown, item);
    }

    /// <summary>単一アイテムの削除</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="BasicDataSetException"></exception>
    public virtual Result<int> Remove<T> (T item) where T : BaseSyncModel<T>, IBaseSyncModel<T>, new() {
        try {
            var result = ProcessAndCommit (() => {
                var original = First<T> (item.Id);
                if (original.Id > 0) {
                    if (item.Version == original.Version) {
                        return database.Execute (
                            $"delete from `{GetSqlName<T> ()}` where `Id` = @Id",
                            item
                        );
                    } else {
                        throw new BasicDataSetException ($"Version mismatch between {item.Version} and {original.Version}");
                    }
                }
                return 0;
            });
            if (result.IsSuccess && result.Value <= 0) {
                result.Status = Status.MissingEntry;
            }
            ClearCache (typeof (T));
            return result;
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine ($"{ex.Message}\n{ex.StackTrace}");
        }
        ClearCache (typeof (T));
        return new (Status.Unknown, 0);
    }

    /// <summary>処理を実行しコミットする、例外またはエラーがあればロールバックする</summary>
    /// <remarks>MySQL/MariaDBに依存</remarks>
    /// <typeparam name="T">返す値の型</typeparam>
    /// <param name="process">処理</param>
    /// <returns>成功またはエラーの状態と値のセット</returns>
    public virtual Result<T> ProcessAndCommit<T> (Func<T> process) {
        var result = default (T)!;
        database.BeginTransaction ();
        try {
            result = process ();
            database.CompleteTransaction ();
            return new (Status.Success, result);
        }
        catch (BasicDataSetException ex) when (ex.IsDeadLock (ex)) {
            database.AbortTransaction ();
            // デッドロックならエスカレート
            throw;
        }
        catch (BasicDataSetException ex) when (ex.TryGetStatus (ex, out var status)) {
            // エラー扱いの例外
            database.AbortTransaction ();
            System.Diagnostics.Trace.WriteLine (ex);
            if (status == Status.CommandTimeout) {
                // タイムアウトならエスカレート
                throw;
            }
            return new (status, result);
        }
        catch (Exception ex) {
            database.AbortTransaction ();
            System.Diagnostics.Trace.WriteLine (ex);
            if (CatchException (ex, result, out var rc)) {
                // 処理済みなら処理結果を返す
                return rc;
            }
            // 未処理ならエスカレート
            throw;
        }
    }

    /// <summary>例外ハンドラ</summary>
    /// <param name="ex">例外</param>
    /// <param name="value">例外発生時の値</param>
    /// <param name="result">処理結果</param>
    /// <returns>処理済/未処理(エスカレート)</returns>
    protected virtual bool CatchException<T> (Exception ex, T value, out Result<T> result) {
        result = new (Status.Unknown, value);
        return false;
    }
}
