using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tetr4lab {

    /// <summary>自然順ソート</summary>
    public static class NaturalSortHelper {
        /// <summary>自然順ソート用比較</summary>
        private static IComparer<string> _naturalSortComparer = new NaturalSortComparer ();
        /// <summary>自然順ソート</summary>
        /// <param name="list">対象</param>
        public static void NaturalSort (this List<string> list)
            => list.Sort (_naturalSortComparer.Compare);
        /// <summary>自然順ソート</summary>
        /// <param name="array">対象</param>
        public static void NaturalSort (this string [] array)
            => Array.Sort (array, _naturalSortComparer.Compare);
    }

    /// <summary>自然順ソート用比較</summary>
    public class NaturalSortComparer : IComparer<string> {
        /// <summary>自然順ソート用セパレータ</summary>
        private static readonly Regex _separator = new (@"(-?[\d\,]+(\.\d+)?)", RegexOptions.Compiled);
        /// <summary>自然順ソート用比較</summary>
        /// <param name="x">対象1</param>
        /// <param name="y">対象2</param>
        public int Compare (string? x, string? y) {
            if (x is null && y is null) { return 0; }
            if (x is null) { return -1; }
            if (y is null) { return 1; }
            var xParts = _separator.Split (x);
            var yParts = _separator.Split (y);
            var minLength = Math.Min (xParts.Length, yParts.Length);
            for (var i = 0; i < minLength; i++) {
                var comparison = decimal.TryParse (xParts [i], out var xNum) && decimal.TryParse (yParts [i], out var yNum)
                    ? xNum.CompareTo (yNum)
                    : string.Compare (xParts [i], yParts [i], StringComparison.OrdinalIgnoreCase);
                if (comparison != 0) {
                    return comparison;
                }
            }
            return x.Length.CompareTo (y.Length);
        }
    }

}
