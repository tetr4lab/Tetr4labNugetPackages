using System.ComponentModel;
using System.Reflection;

namespace Tetr4lab {

    /// <summary>enumと文字列の相互変換</summary>
    /// <remarks>
    /// enumと<seealso cref="DescriptionAttribute"/>で定義した文字列またはメンバー名とを相互に変換します。<br/>
    /// 文字列やenum値の重複、<seealso cref="FlagsAttribute"/>([Flags])はサポートされません。
    /// </remarks>
    public static class EnumName<TEnum> where TEnum : struct, Enum {
        /// <summary>enumから文字列への変換辞書</summary>
        private static readonly Dictionary<TEnum, string> _toNameDictionary;

        /// <summary>文字列からenumへの変換辞書</summary>
        private static readonly Dictionary<string, TEnum> _toEnumDictionary;

        /// <summary>基底文字列化</summary>
        /// <param name="enum">カテゴリ</param>
        /// <returns>説明属性または単純文字列化</returns>
        private static string ToBaseString (TEnum @enum) {
            var name = @enum.ToString ();
            return typeof (TEnum).GetField (name)?.GetCustomAttribute<DescriptionAttribute> ()?.Description ?? name;
        }

        /// <summary>クラス初期化</summary>
        static EnumName () {
            _toNameDictionary = new Dictionary<TEnum, string> ();
            _toEnumDictionary = new Dictionary<string, TEnum> ();
            foreach (var @enum in Enum.GetValues<TEnum> ()) {
                var name = ToBaseString (@enum);
                if (!_toEnumDictionary.TryAdd (name, @enum)) {
                    throw new InvalidEnumArgumentException ("enum name duplicated");
                }
                if (!_toNameDictionary.TryAdd (@enum, name)) {
                    throw new InvalidEnumArgumentException ("enum value duplicated");
                }
            }
        }

        /// <summary>enumを文字列化</summary>
        public static string ToName (TEnum @enum)
            => _toNameDictionary.TryGetValue (@enum, out var value) ? value : @enum.ToString ();

        /// <summary>文字列からenumを得る</summary>
        public static TEnum ToEnum (string name, TEnum defaultValue = default)
            => _toEnumDictionary.TryGetValue (name, out var value)
                ? value
                : Enum.TryParse<TEnum> (name, ignoreCase: false, out var parsed)
                    ? parsed
                    : defaultValue;
    }

}
