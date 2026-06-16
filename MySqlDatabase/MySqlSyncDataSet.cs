using MySqlConnector;
using PetaPoco;
using Tetr4lab.Auth;

namespace Tetr4lab.Db.MySql;

/// <summary>MySQL/MariaDBに依存</summary>
public abstract class MySqlSyncDataSet : BasicSyncDataSet {
    /// <inheritdoc/>
    public MySqlSyncDataSet (Database database, AuthStateProvider authStateProvider, string? key = "database") : base (database, authStateProvider, key) { }

    /// <inheritdoc/>
    /// <remarks>MySQL/MariaDBに依存</remarks>
    public override long GetAutoIncremantValue<T> () {
        // 開始Idを取得
        var Id = 0L;
        try {
            // 待避と設定 (SQLに勝手に'SELECT'を挿入しない)
            var enableAutoSelectBackup = database.EnableAutoSelect;
            database.EnableAutoSelect = false;
            try {
                try {
                    // 設定 (情報テーブルの即時更新を設定)
                    database.Execute ("set session information_schema_stats_expiry=1;");
                }
                catch (MySqlException ex) when (ex.Message.StartsWith ("Unknown system variable")) {
                    // MariaDBはこの変数をサポートしていない
                    System.Diagnostics.Trace.WriteLine ($"Server not supported 'information_schema_stats_expiry'\n{ex}");
                }
                // 次の自動更新値の取得
                Id = database.Single<long> (
                    $"select AUTO_INCREMENT from information_schema.tables where TABLE_SCHEMA='{databaseName}' and TABLE_NAME='{GetSqlName<T> ()}';"
                );
            }
            finally {
                // 設定の復旧
                database.EnableAutoSelect = enableAutoSelectBackup;
            }
        }
        catch (Exception ex) {
            System.Diagnostics.Trace.WriteLine ($"Get auto_increment number\n{ex}");
        }
        if (Id <= 0) {
            // 開始Idの取得に失敗
            throw new NotSupportedException ("Failed to get auto_increment value.");
        }
        return Id;
    }

    /// <inheritdoc/>
    /// <remarks>MySQL/MariaDBに依存</remarks>
    public override int GetLastInsertRowId () => database.ExecuteScalar<int> ("select LAST_INSERT_ID();");

    /// <inheritdoc/>
    /// <remarks>MySQL/MariaDBに依存</remarks>
    /// <summary>処理を実行しコミットする、例外またはエラーがあればロールバックする</summary>
    /// <remarks>MySQL/MariaDBに依存</remarks>
    /// <typeparam name="T">返す値の型</typeparam>
    /// <param name="process">処理</param>
    /// <returns>成功またはエラーの状態と値のセット</returns>
    public override Result<T> ProcessAndCommit<T> (Func<T> process) {
        var result = default (T)!;
        database.BeginTransaction ();
        try {
            result = process ();
            database.CompleteTransaction ();
            return new (Status.Success, result);
        }
        catch (MySqlException ex) when (ex.Message.StartsWith ("Deadlock found")) {
            database.AbortTransaction ();
            // デッドロックならエスカレート
            throw;
        }
        catch (MySqlException ex) when (MyDataSetException.TryGetStatus2 (ex, out var status)) {
            // エラー扱いの例外
            database.AbortTransaction ();
            System.Diagnostics.Trace.WriteLine (ex);
            if (status == Status.CommandTimeout) {
                // タイムアウトならエスカレート
                throw;
            }
            return new (status, result);
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
            System.Diagnostics.Debug.WriteLine ($"Exception: {ex.Message}\n{ex.StackTrace}");
            database.AbortTransaction ();
            throw;
        }
    }
}
