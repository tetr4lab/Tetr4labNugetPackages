using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tetr4lab {

    /// <summary>JsonSerializeer拡張</summary>
    public static class JsonSerializerEx {
        /// <summary>逐語的テキストを使用したJsonをパース</summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="atJson">`@""`を使えるJson</param>
        /// <returns></returns>
        public static TItem? Deserialize<TItem> (string atJson)
            => JsonSerializer.Deserialize<TItem> (
                new Regex (@"@""((?:[^""]|"""")*)""(?!"")", RegexOptions.Singleline)
                .Replace (atJson, m => $"\"{m.Groups [1].Value.Replace ("\\", "\\\\").Replace ("\"\"", "\\\"")}\"")
            );
    }
}
