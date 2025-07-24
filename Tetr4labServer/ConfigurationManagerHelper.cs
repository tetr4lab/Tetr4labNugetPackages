using Microsoft.Extensions.Configuration;

namespace Tetr4lab;

/// <summary>アプリ構成マネジャ拡張</summary>
public static partial class ConfigurationManagerHelper {
    /// <summary>MySql接続文字列合成</summary>
    /// <param name="configuration"></param>
    /// <param name="database"></param>
    /// <returns></returns>
    public static string MyConnectionString (this ConfigurationManager configuration, string database)
        => $"database={database};{configuration.GetConnectionString ("Host")}{configuration.GetConnectionString ("Account")}Allow User Variables=true;";
}
