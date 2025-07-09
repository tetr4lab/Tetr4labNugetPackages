using Tetr4lab;

namespace PetaPoco;

/// <summary>PetaPoco.Databaseの拡張</summary>
public static class BasicDatabaseHelper {
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
        catch (Exception ex) when (BasicDataSetException.IsDeadLock (ex)) {
            await database.AbortTransactionAsync ();
            // デッドロックならエスカレート
            throw;
        }
        catch (Exception ex) when (BasicDataSetException.TryGetStatus (ex, out var status)) {
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
