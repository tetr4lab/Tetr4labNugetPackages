using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Tetr4labRazor;

/// <summary>
/// ストレージの拡張
///   ```razor
///   @inject ProtectedLocalStorage Storage
///   ```
/// </summary>
public static class ProtectedLocalStrageHelper {

    /// <summary>アプリ識別子</summary>
    private static string AppId => AppDomain.CurrentDomain.FriendlyName;

    /// <summary>ストレージから値を所得</summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="storage">ストレージ</param>
    /// <param name="key">キー</param>
    /// <returns>値</returns>
    public static async Task<ProtectedBrowserStorageResult<T>> GetValueAsync<T> (this ProtectedLocalStorage storage, string key) where T : notnull
        => await storage.GetAsync<T> ($"{AppId}|{key}");
    /// <summary>ストレージへ値を保存</summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="storage">ストレージ</param>
    /// <param name="key">キー</param>
    /// <param name="value">値</param>
    /// <returns></returns>
    public static async Task SetValueAsync<T> (this ProtectedLocalStorage storage, string key, T value) where T : notnull
        => await storage.SetAsync ($"{AppId}|{key}", value);
}
