using System.Globalization;

namespace Tetr4lab {

    /// <summary>文字列クラス拡張</summary>
    public static class StringHelper {
        /// <summary>曜日付きの短い日付表記に変換する</summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToShortDateWithDayOfWeekString (this DateTime dateTime) {
            return $"{dateTime.ToShortDateString ()} {dateTime.ToString ("ddd", CultureInfo.GetCultureInfo ("ja-JP"))}";
        }

        /// <summary>文字列を指定幅に丸める</summary>
        /// <param name="str"></param>
        /// <param name="width"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public static string Ellipsis (this string str, int width, string mark = "…") => str.Length <= width ? str : $"{str [0..(width - mark.Length)]}{mark}";

        /// <summary>文字列集合が指定の部分文字列を含むか</summary>
        /// <param name="list"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool SubContains (this IEnumerable<string> list, string target) {
            foreach (var item in list) {
                if (item.Contains (target)) { return true; }
            }
            return false;
        }
    }

}
