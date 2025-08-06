using System.Reflection;
using MudBlazor;

namespace Tetr4lab;

/// <summary>MudBlazor Icons拡張</summary>
public static class MudIconsHelper {
    /// <summary>動的アイコン生成</summary>
    /// <param name="name">アイコン名(@"((?:\w+\.){0,3}\w+)")</param>
    /// <returns>アイコン</returns>
    public static string GetIcon (this string name) {
        var parts = name.Split ('.');
        var iconField = typeof (Icons)
            .GetNestedType (parts.Length > 2 ? parts [^3] : "Material")?
            .GetNestedType (parts.Length > 1 ? parts [^2] : "Filled")?
            .GetField (parts [^1], BindingFlags.Public | BindingFlags.Static);
        if (iconField is not null) {
            return (string?) iconField.GetValue (null) ?? string.Empty;
        }
        return Icons.Material.Filled.Error;
    }
}
