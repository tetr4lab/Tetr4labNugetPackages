using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PetaPoco;

namespace Tetr4lab;

/// <summary>基礎的なデータセット</summary>
public abstract class SqliteDataSet : BasicDataSet {
    /// <inheritdoc/>
    public SqliteDataSet (Database database, string? key = "Data Source") : base (database, key) {
        this.database = database;
    }
    /// <inheritdoc/>
    /// <remarks>SQLiteに依存</remarks>
    protected override async Task<long> GetAutoIncremantValueAsync<T> () {
        // 開始Idを取得
        var Id = 0L;
        try {
            Id = await database.SingleOrDefaultAsync<long> (
                $"SELECT seq + 1 FROM sqlite_sequence WHERE name='{GetSqlName<T> ()}';"
            );
            if (Id == 0) { Id = 1; }
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
    /// <remarks>SQLiteに依存</remarks>
    protected override async Task<int> GetLastInsertRowId () {
        return await database.ExecuteScalarAsync<int> ("select last_insert_rowid();");
    }
}
