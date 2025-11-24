using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PetaPoco;

namespace Tetr4lab;

/// <summary>MySQL/MariaDBに依存</summary>
public abstract class MySqlDataSet : BasicDataSet {
    /// <inheritdoc/>
    public MySqlDataSet (Database database, string? key = "database") : base (database, key) { }
    /// <inheritdoc/>
    /// <remarks>MySQL/MariaDBに依存</remarks>
    protected override async Task<long> GetAutoIncremantValueAsync<T> () {
        // 開始Idを取得
        var Id = 0L;
        try {
            // 待避と設定 (SQLに勝手に'SELECT'を挿入しない)
            var enableAutoSelectBackup = database.EnableAutoSelect;
            database.EnableAutoSelect = false;
            try {
                try {
                    // 設定 (情報テーブルの即時更新を設定)
                    await database.ExecuteAsync ("set session information_schema_stats_expiry=1;");
                }
                catch (MySqlException ex) when (ex.Message.StartsWith ("Unknown system variable")) {
                    // MariaDBはこの変数をサポートしていない
                    System.Diagnostics.Trace.WriteLine ($"Server not supported 'information_schema_stats_expiry'\n{ex}");
                }
                // 次の自動更新値の取得
                Id = await database.SingleAsync<long> (
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
    protected override async Task<int> GetLastInsertRowId () {
        return await database.ExecuteScalarAsync<int> ("select LAST_INSERT_ID();");
    }
    /// <inheritdoc/>
    /// <remarks>MySQL/MariaDBに依存</remarks>
    public override async Task<Result<T>> ProcessAndCommitAsync<T> (Func<Task<T>> process) {
        var result = default (T)!;
        await database.BeginTransactionAsync ();
        try {
            result = await process ();
            await database.CompleteTransactionAsync ();
            return new (Status.Success, result);
        }
        catch (MySqlException ex) when (ex.Message.StartsWith ("Deadlock found")) {
            await database.AbortTransactionAsync ();
            // デッドロックならエスカレート
            throw;
        }
        catch (MySqlException ex) when (MyDataSetException.TryGetStatus2 (ex, out var status)) {
            // エラー扱いの例外
            await database.AbortTransactionAsync ();
            System.Diagnostics.Trace.WriteLine (ex);
            if (status == Status.CommandTimeout) {
                // タイムアウトならエスカレート
                throw;
            }
            return new (status, result);
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
}
