using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PetaPoco;

namespace Tetr4lab;

/// <summary>基礎的なデータセット</summary>
public abstract class BasicDataSet {

    /// <summary>待機間隔</summary>
    public virtual int WaitInterval => 1000 / 60;

    /// <summary>ロードでエラーした場合の最大試行回数</summary>
    protected virtual int MaxRetryCount => 10;

    /// <summary>ロードでエラーした場合のリトライ間隔</summary>
    protected virtual int RetryInterval => 1000 / 30;

    /// <summary>PetaPocoをDI</summary>
    protected virtual Database database { get; set; }

    /// <summary>データベース名</summary>
    protected string databaseName = "";

    /// <summary>コンストラクタ</summary>
    public BasicDataSet (Database database, string? key = "database") {
        this.database = database;
        if (!string.IsNullOrEmpty (key)) {
            var words = database.ConnectionString.Split (['=', ';']);
            var index = Array.IndexOf (words, key);
            if (index < 0 || index > words.Length) {
                throw new InvalidOperationException ("The database name could not be determined.");
            }
            databaseName = words [index + 1];
        }
    }

    /// <summary>初期化</summary>
    public virtual async Task InitializeAsync () {
        if (!IsInitialized && !IsInitializeStarted) {
            try {
                IsInitializeStarted = true;
                await LoadAsync ();
                IsInitialized = true;
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine (e);
                isLoading = false;
                IsUnavailable = true;
            }
        }
    }

    /// <summary>初期化開始済み</summary>
    public virtual bool IsInitializeStarted { get; protected set; }

    /// <summary>初期化済み</summary>
    public virtual bool IsInitialized { get; protected set; }

    /// <summary>初期化が済み、ロード中でない</summary>
    public virtual bool IsReady => IsInitialized && !isLoading;

    /// <summary>初期化に失敗</summary>
    public virtual bool IsUnavailable { get; protected set; }

    /// <summary>(再)読み込み</summary>
    /// <remarks>既に読み込み中なら単に完了を待って戻る、再読み込み中でも以前のデータが有効</remarks>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public virtual async Task LoadAsync () {
        if (isLoading) {
            while (isLoading) {
                await Task.Delay (WaitInterval);
            }
            return;
        }
        isLoading = true;
        for (var i = 0; i < MaxRetryCount; i++) {
            var result = await GetListSetAsync ();
            if (result.IsSuccess) {
                isLoading = false;
                return;
            }
            await Task.Delay (RetryInterval);
        }
        throw new TimeoutException ("The maximum number of retries for LoadAsync was exceeded.");
    }
    /// <summary>ロード中</summary>
    protected bool isLoading;

    /// <summary>指定クラスのモデルインスタンスを取得</summary>
    /// <typeparam name="T">取得するモデルクラス</typeparam>
    /// <returns>取得したモデルインスタンス</returns>
    public virtual List<T> GetList<T> () where T : class
        => ListSet.ContainsKey (typeof (T)) && ListSet [typeof (T)] is List<T> list ? list : new ();

    /// <summary>ロード済みモデルインスタンス</summary>
    public virtual Dictionary<Type, object> ListSet { get; protected set; } = new ();

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
    /// <remarks>ColumnでありかつVirtualColumnでないプロパティだけを対象とする</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="withId"></param>
    /// <returns></returns>
    protected virtual string GetSettingSql<T> (bool withId = false) where T : class {
        var result = string.Empty;
        var properties = typeof (T).GetProperties (BindingFlags.Instance | BindingFlags.Public);
        if (properties != null) {
            result = string.Join (',', Array.ConvertAll (properties, property => {
                var @virtual = property.GetCustomAttribute<VirtualColumnAttribute> ();
                var attribute = property.GetCustomAttribute<ColumnAttribute> ();
                return @virtual == null && attribute != null && (withId || !(attribute.Name ?? property.Name).Equals ("Id", StringComparison.OrdinalIgnoreCase))
                    ? $"`{attribute.Name ?? property.Name}`=@{property.Name}"
                    : "";
            }).ToList ().FindAll (i => i != ""));
        }
        return result;
    }

    /// <summary>追加用値SQL</summary>
    /// <remarks>ColumnでありかつVirtualColumnでないプロパティだけを対象とする</remarks>
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
                var attribute = property.GetCustomAttribute<ColumnAttribute> ();
                return @virtual == null && attribute != null && (withId || !(attribute.Name ?? property.Name).Equals ("Id", StringComparison.OrdinalIgnoreCase))
                    ? $"@{property.Name}{(index >= 0 ? $"_{index}" : "")}"
                    : "";
            }).ToList ().FindAll (i => i != ""));
        }
        return result;
    }

    /// <summary>追加用カラムSQL</summary>
    /// <remarks>ColumnでありかつVirtualColumnでないプロパティだけを対象とする</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="withId"></param>
    /// <returns></returns>
    public string GetColumnsSql<T> (bool withId = false) where T : class {
        var result = string.Empty;
        var properties = typeof (T).GetProperties (BindingFlags.Instance | BindingFlags.Public);
        if (properties != null) {
            result = string.Join (',', Array.ConvertAll (properties, property => {
                var @virtual = property.GetCustomAttribute<VirtualColumnAttribute> ();
                var attribute = property.GetCustomAttribute<ColumnAttribute> ();
                return @virtual == null && attribute != null && (withId || !(attribute.Name ?? property.Name).Equals ("Id", StringComparison.OrdinalIgnoreCase))
                    ? $"`{attribute.Name ?? property.Name}`"
                    : "";
            }).ToList ().FindAll (i => i != ""));
        }
        return result;
    }

    /// <summary>必須項目のチェック</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="withId"></param>
    /// <returns></returns>
    public static bool EntityIsValid<T> (T? item, bool withId = false) where T : BaseModel<T>, new() {
        if (item == null || withId && (item.Id <= 0 || item.Modified == default)) { return false; }
        var properties = new List<PropertyInfo> ();
        foreach (var property in typeof (T).GetProperties (BindingFlags.Instance | BindingFlags.Public) ?? []) {
            var required = property.GetCustomAttribute<RequiredAttribute> ();
            var attribute = property.GetCustomAttribute<ColumnAttribute> ();
            if (attribute != null && required != null) {
                properties.Add (property);
            }
        }
        foreach (var property in properties) {
            var value = property.GetValue (item);
            var type = value?.GetType ();
            if (type == typeof (string)) {
                if (string.IsNullOrEmpty (property.GetValue (item) as string)) {
                    return false;
                }
            } else if (type?.IsClass == true) {
                if (property.GetValue (item) == null) {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>アイテムリストから辞書型パラメータを生成する</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="withId"></param>
    /// <returns></returns>
    protected virtual Dictionary<string, object?> GetParamDictionary<T> (IEnumerable<T> values, bool withId = false) {
        var parameters = new Dictionary<string, object?> ();
        var prpperties = new List<PropertyInfo> ();
        foreach (var property in typeof (T).GetProperties (BindingFlags.Instance | BindingFlags.Public) ?? []) {
            var attribute = property.GetCustomAttribute<ColumnAttribute> ();
            if (attribute != null && property.GetCustomAttribute<VirtualColumnAttribute> () == null
                && (withId || (attribute.Name ?? property.Name) != "Id")) {
                prpperties.Add (property);
            }
        }
        var i = 0;
        foreach (var value in values) {
            foreach (var property in prpperties) {
                parameters.Add ($"{property.Name}_{i}", property.GetValue (value));
            }
            i++;
        }
        return parameters;
    }

    /// <summary>処理を実行しコミットする、例外またはエラーがあればロールバックする</summary>
    /// <typeparam name="T">返す値の型</typeparam>
    /// <param name="process">処理</param>
    /// <returns>成功またはエラーの状態と値のセット</returns>
    public virtual async Task<Result<T>> ProcessAndCommitAsync<T> (Func<Task<T>> process)
        => await database.ProcessAndCommitAsync (process);

    /// <summary>一覧セットをアトミックに取得</summary>
    /// <returns>成否</returns>
    public abstract Task<Result<bool>> GetListSetAsync ();

    /// <summary>単一アイテムを取得 (Idで特定) 【注意】総リストとは別オブジェクトになる</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual async Task<Result<T?>> GetItemByIdAsync<T> (T item) where T : BaseModel<T>, new() {
        var table = GetSqlName<T> ();
        return await ProcessAndCommitAsync (async () => (await database.FetchAsync<T?> (
            $"select `{table}`.* from `{table}` where `{table}`.Id = @Id;",
            item
        )).Single ());
    }

    /// <summary>アイテムの更新サブ処理 トランザクション内専用</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual async Task<int> UpdateItemAsync<T> (T item) where T : BaseModel<T>, new() {
        var table = GetSqlName<T> ();
        return await database.ExecuteAsync (
            @$"update `{table}` set {GetSettingSql<T> ()} where `{table}`.Id = @Id",
            item
        );
    }

    /// <summary>アイテムの更新</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual async Task<Result<int>> UpdateAsync<T> (T item) where T : BaseModel<T>, new() {
        var result = await ProcessAndCommitAsync (async () => {
            item.Version++;
            return await UpdateItemAsync<T> (item);
        });
        if (result.IsSuccess && result.Value <= 0) {
            result.Status = Status.MissingEntry;
        }
        return result;
    }

    /// <summary>最後に挿入した行番号を得るSQL</summary>
    /// <returns>行番号</returns>
    protected abstract Task<int> GetLastInsertRowId ();

    /// <summary>単一アイテムの追加</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual async Task<Result<T>> AddAsync<T> (T item) where T : BaseModel<T>, new() {
        var result = await ProcessAndCommitAsync (async () => {
            await database.ExecuteAsync (
                $"insert into `{GetSqlName<T> ()}` ({GetColumnsSql<T> ()}) values ({GetValuesSql<T> ()});",
                item
            );
            item.Id = await GetLastInsertRowId ();
            return item.Id > 0 ? 1 : 0;
        });
        if (result.IsSuccess && result.Value <= 0) {
            result.Status = Status.MissingEntry;
        }
        if (result.IsFailure) {
            item.Id = default;
        } else {
            // ロード済みに追加
            GetList<T> ().Add (item);
        }
        return new (result.Status, item);
    }

    /// <summary>単一アイテムの削除</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="BasicDataSetException"></exception>
    public virtual async Task<Result<int>> RemoveAsync<T> (T item) where T : BaseModel<T>, new() {
        var result = await ProcessAndCommitAsync (async () => {
            var original = await GetItemByIdAsync<T> (item);
            if (original.IsSuccess && original.Value != null) {
                if (item.Version == original.Value.Version) {
                    return await database.ExecuteAsync (
                        $"delete from `{GetSqlName<T> ()}` where `Id` = @Id",
                        item
                    );
                } else {
                    throw new BasicDataSetException ($"Version mismatch between {item.Version} and {original.Value.Version}");
                }
            }
            return 0;
        });
        if (result.IsSuccess && result.Value <= 0) {
            result.Status = Status.MissingEntry;
        }
        // ロード済みから除去
        GetList<T> ().Remove (item);
        return result;
    }

    /// <summary>テーブルの次の自動更新値を得る</summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    protected abstract Task<long> GetAutoIncremantValueAsync<T> () where T : class;

    /// <summary>一括アイテムの追加</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public virtual async Task<Result<int>> AddRangeAsync<T> (IEnumerable<T> items) where T : BaseModel<T>, new() {
        if (items.Count () <= 0) { return new Result<int> (Status.MissingEntry, 0); }
        var result = await ProcessAndCommitAsync (async () => {
            // 開始Idを取得
            var Id = await GetAutoIncremantValueAsync<T> ();
            if (Id <= 0) {
                // 開始Idの取得に失敗
                throw new NotSupportedException ("Failed to get auto_increment value.");
            }
            // 主アイテムを挿入
            var valuesSqls = new List<string> ();
            for (int i = 0; i < items.Count (); i++) {
                valuesSqls.Add ($"({GetValuesSql<T> (i)})");
            }
            var result = await database.ExecuteAsync (
                $"insert into `{GetSqlName<T> ()}` ({GetColumnsSql<T> ()}) values {string.Join (',', valuesSqls)};",
                GetParamDictionary (items)
            );
            return result;
        });
        if (result.IsSuccess) {
            // ロード済みに追加 (リロード推奨)
            GetList<T> ().AddRange (items);
        }
        return result;
    }

}
