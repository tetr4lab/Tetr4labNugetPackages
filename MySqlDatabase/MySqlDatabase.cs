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
    /// <summary>例外が発生</summary>
    /// <param name="ex">例外</param>
    /// <returns>真なら昇格</returns>
    public override bool OnException (Exception ex) {
        System.Diagnostics.Trace.WriteLine ($"Database.OnException: {LastCommand.Ellipsis (80)}\n{ex}");
        return base.OnException (ex);
    }
}
