using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tetr4lab {

    /// <summary>型拡張</summary>
    public static class TypeHelper {
        /// <summary>アセンブリバージョン取得</summary><summary>
        /// <param name="type">型</param>
        /// <returns>バージョン文字列</returns>
        public static string GetAssemblyVersion (this Type type) {
            var assembly = type.Assembly;
            var name = assembly.GetName ();
            return @$"{name.Name} {(
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute> ()?.InformationalVersion
                ?? name.Version?.ToString ()
                ?? "?"
            ).Split ('+') [0]}";
        }
    }

}
