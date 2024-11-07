using System;
using System.Collections.Generic;

namespace Tetr4lab {

    /// <summary>配列とコレクションの拡張</summary>
    public static class CollectionHelper {

        /// <summary>入れ替え</summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="lhs">左項</param>
        /// <param name="rhs">右項</param>
        public static void Swap<T> (ref T lhs, ref T rhs) {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>データを16進ダンプする</summary>
        /// <param name="data">データ</param>
        /// <param name="width">列数</param>
        /// <param name="separator">列区切</param>
        /// <returns></returns>
        public static string HexDump (this byte [] data, int width = 16, string separator = " ") {
            var str = new List<string> { };
            for (var i = 0; i < data.Length; i++) {
                str.Add ($"{((i <= 0) ? "" : ((width > 0 && i % width == 0) ? "\n" : separator))}{data [i].ToString ("X2")}");
            }
            return str.Join ("");
        }

        /// <summary>配列を文字列化して連結</summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="array">配列</param>
        /// <param name="separator">区切り</param>
        /// <returns></returns>
        public static string Join<T> (this T [] array, string separator)
            => string.Join (separator, Array.ConvertAll (array, v => v?.ToString () ?? string.Empty));

        /// <summary>リストを文字列化して連結</summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="list">リスト</param>
        /// <param name="separator">区切り</param>
        /// <returns></returns>
        public static string Join<T> (this List<T> list, string separator)
            => (list == null) ? string.Empty : string.Join (separator, list.ConvertAll (v => v?.ToString () ?? string.Empty));

        /// <summary>コレクションを文字列化して連結</summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="collection">コレクション</param>
        /// <param name="separator">区切り</param>
        /// <returns></returns>
        public static string Join<T> (this IEnumerable<T> collection, string separator) {
            if (collection is null) { return string.Empty; }
            var list = new List<string> ();
            foreach (var item in collection) {
                list.Add (item?.ToString () ?? string.Empty);
            }
            return string.Join (separator, list);
        }

        /// <summary>コレクションがnullまたは空であれば真</summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="collection"></param>
        /// <returns>nullまたは空であれば真</returns>
        public static bool IsNullOrEmpty<T> (this IEnumerable<T> collection) {
            if (collection is null) { return true; }
            foreach (var item in collection) {
                return false;
            }
            return true;
        }

    }
}
