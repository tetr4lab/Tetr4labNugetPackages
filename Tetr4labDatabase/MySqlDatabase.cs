using System.Data;
using System.Data.Common;
using PetaPoco.Core;
using Tetr4lab;

namespace PetaPoco;

/// <summary>PetaPoco.Databaseのラッパー</summary>
public class MySqlDatabase : Database {
    /// <summary>PetaPoco.Databaseのラッパー</summary>
    public MySqlDatabase (IDatabaseBuildConfiguration configuration) : base (configuration) { }
    /// <summary>PetaPoco.Databaseのラッパー</summary>
    public MySqlDatabase (IDbConnection connection, IMapper? defaultMapper = null) : base (connection, defaultMapper) { }
    /// <summary>PetaPoco.Databaseのラッパー</summary>
    public MySqlDatabase (string connectionString, string providerName, IMapper? defaultMapper = null) : base (connectionString, providerName, defaultMapper) { }
    /// <summary>PetaPoco.Databaseのラッパー</summary>
    public MySqlDatabase (string connectionString, DbProviderFactory factory, IMapper? defaultMapper = null) : base (connectionString, factory, defaultMapper) { }
    /// <summary>PetaPoco.Databaseのラッパー</summary>
    public MySqlDatabase (string connectionString, IProvider provider, IMapper? defaultMapper = null) : base (connectionString, provider, defaultMapper) { }
    /// <summary>連外が発生</summary>
    /// <param name="ex">例外</param>
    /// <returns>真なら昇格</returns>
    public override bool OnException (Exception ex) {
        System.Diagnostics.Debug.WriteLine ($"Database.OnException: {LastCommand.Ellipsis (80)}\n{ex}");
        return base.OnException (ex);
    }
}

/// <summary>PetaPoco.Databaseの拡張</summary>
public static class DatabaseHelper {
    /// <summary>処理を実行しコミットする、例外またはエラーがあればロールバックする</summary>
    /// <typeparam name="T">返す値の型</typeparam>
    /// <param name="database">PetaPoco.Database</param>
    /// <param name="process">処理</param>
    /// <returns>成功またはエラーの状態と値のセット</returns>
    public static async Task<Result<T>> ProcessAndCommitAsync<T> (this Database database, Func<Task<T>> process) {
        var result = default (T)!;
        await database.BeginTransactionAsync ();
        try {
            result = await process ();
            await database.CompleteTransactionAsync ();
            return new (Status.Success, result);
        }
        catch (Exception ex) when (ex.IsDeadLock ()) {
            await database.AbortTransactionAsync ();
            // デッドロックならエスカレート
            throw;
        }
        catch (Exception ex) when (ex.TryGetStatus (out var status)) {
            // エラー扱いの例外
            await database.AbortTransactionAsync ();
            System.Diagnostics.Debug.WriteLine (ex);
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

    /// <summary>一覧を取得</summary>
    /// <typeparam name="T">返す値の型</typeparam>
    /// <param name="database">PetaPoco.Database</param>
    /// <param name="sql">Fetchに渡すSQL</param>
    /// <param name="args">Fetchに渡す引数</param>
    /// <returns></returns>
    public static async Task<Result<List<T>>> GetListAsync<T> (this Database database, string sql, params object [] args)
        => await ProcessAndCommitAsync (database, async () => await database.FetchAsync<T> (sql, args));
}
