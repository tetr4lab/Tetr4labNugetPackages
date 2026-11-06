using System.ComponentModel;
using System.Reflection;

namespace Tetr4lab {

    /// <summary>enumと文字列の相互変換</summary>
    /// <remarks>
    /// enumと<seealso cref="DescriptionAttribute"/>で定義した文字列またはメンバー名とを相互に変換します。<br/>
    /// 文字列の重複、文字列と別のメンバー名の衝突、値の重複、<seealso cref="FlagsAttribute"/>([Flags])の使用は例外を投げます。<br/>
    /// 未定義の整数値や整数文字列を使用した場合の挙動は保証されません。
    /// </remarks>
    public static class EnumName<TEnum> where TEnum : struct, Enum {
        /// <summary>enumから文字列への変換辞書</summary>
        private static readonly Dictionary<TEnum, string> _toNameDictionary;

        /// <summary>文字列からenumへの変換辞書</summary>
        private static readonly Dictionary<string, TEnum> _toEnumDictionary;

        /// <summary>クラス初期化</summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when using FlagsAttribute, duplicate enum value, duplicate enum name or description.
        /// </exception>
        static EnumName () {
            if (typeof (TEnum).IsDefined (typeof (FlagsAttribute), false)) {
                throw new InvalidOperationException (
                    $"{typeof (TEnum).Name} does not support FlagsAttribute.");
            }
            _toNameDictionary = new ();
            _toEnumDictionary = new (StringComparer.Ordinal);
            var enumName = typeof (TEnum).Name;
            foreach (var field in typeof (TEnum).GetFields (BindingFlags.Public | BindingFlags.Static)) {
                var name = field.Name;
                var value = (TEnum) field.GetValue (null)!;
                var xname = field.GetCustomAttribute<DescriptionAttribute> ()?.Description ?? name;
                if (!_toEnumDictionary.TryAdd (xname, value)) {
                    throw new InvalidOperationException ($"Duplicate enum name: {enumName}.{name}='{xname}'={enumName}.{_toEnumDictionary [xname]}");
                }
                if (!_toNameDictionary.TryAdd (value, xname)) {
                    throw new InvalidOperationException ($"Duplicate enum value: {enumName}.{name}={enumName}.{value}");
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
                : Enum.TryParse<TEnum> (name, ignoreCase: false, out var parsed) && Enum.IsDefined (parsed)
                    ? parsed
                    : defaultValue;
    }

}
