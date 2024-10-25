using System.Globalization;

namespace Tetr4lab;

public static class StringHelper {
    /// <summary>曜日付きの短い日付表記に変換する</summary>
    public static string ToShortDateWithDayOfWeekString (this DateTime dateTime) {
        return $"{dateTime.ToShortDateString ()} {dateTime.ToString ("ddd", CultureInfo.GetCultureInfo ("ja-JP"))}";
    }

    /// <summary>文字列を指定幅に丸める</summary>
    public static string Ellipsis (this string str, int width, string mark = "…") => str.Length <= width ? str : $"{str [0..(width - mark.Length)]}{mark}";

    /// <summary>文字列集合が指定の部分文字列を含むか</summary>
    public static bool SubContains (this IEnumerable<string> list, string target) {
        foreach (var item in list) {
            if (item.Contains (target)) { return true; }
        }
        return false;
    }
}
