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
    protected override bool CatchException<T> (Exception ex, T value, out Result<T> result) {
        if (ex is MySqlException && ex.Message.StartsWith ("Deadlock found")) {
            // デッドロックならエスカレート
            result = new (Status.DeadlockFound, value);
            return false;
        } else if (ex is MySqlException && MyDataSetException.TryGetStatus2 (ex, out var status)) {
            result = new (status, value);
            if (status == Status.CommandTimeout) {
                // タイムアウトならエスカレート
                return false;
            }
            // エラー扱いの例外
            return true;
        }
        return base.CatchException (ex, value, out result);
    }
}
