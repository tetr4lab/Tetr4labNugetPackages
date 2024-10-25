using System.Diagnostics.CodeAnalysis;

namespace Tetr4lab;

public static partial class ParameterHelper {
    /// <summary>汎用パラメータ辞書に該当の値が存在する</summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="parameters">辞書</param>
    /// <param name="key">鍵</param>
    /// <returns>指定の型の値を持つ鍵があれば真</returns>
    public static bool ContainsKey<T> (this Dictionary<string, object> parameters, string key)
        => parameters.ContainsKey (key) && parameters [key] is T;

    /// <summary>汎用パラメータ辞書から値を取得</summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="parameters">辞書</param>
    /// <param name="key">鍵</param>
    /// <returns>指定型の鍵があればその値、なければデフォルト値</returns>
    public static T? GetValueOrDefault<T> (this Dictionary<string, object> parameters, string key)
        => parameters.ContainsKey (key) && parameters [key] is T value ? value : default;

    /// <summary>汎用パラメータ辞書から値を取得</summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="parameters">辞書</param>
    /// <param name="key">鍵</param>
    /// <param name="value">値</param>
    /// <returns>指定型の鍵があれば真</returns>
    public static bool TryGetValue<T> (this Dictionary<string, object> parameters, string key, [MaybeNullWhen (false)] out T value) {
        if (parameters.ContainsKey (key) && parameters [key] is T tValue) {
            value = tValue;
            return true;
        }
        value = default;
        return false;
    }
}
