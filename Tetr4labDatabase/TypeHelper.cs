using System.ComponentModel;
using System.Reflection;
using PetaPoco;

namespace Tetr4lab.Db;

/// <summary>型拡張</summary>
public static partial class TypeHelper {
    /// <summary>列ラベルの生成</summary>
    /// <param name="type">型</param>
    /// <returns>辞書</returns>
    public static Dictionary<string, string> CreateLabel (this Type type) {
        var labels = new Dictionary<string, string> ();
        // プロパティ
        foreach (var property in type.GetProperties (BindingFlags.Public | BindingFlags.Instance)) {
            var attribute = property.GetCustomAttribute<DescriptionAttribute> ();
            if (attribute is not null) {
                // Description
                labels [property.Name] = attribute.Description;
            } else if (!property.Name.StartsWith ('_') && (property.IsDefined (typeof (ColumnAttribute)) || property.IsDefined (typeof (ResultColumnAttribute)))) {
                // アンダースコアで始まらない名前のカラム
                labels [property.Name] = property.Name;
            }
        }
        // メソッド
        foreach (var method in type.GetMethods (BindingFlags.Public | BindingFlags.Instance)) {
            var attribute = method.GetCustomAttribute<DescriptionAttribute> ();
            if (attribute is not null) {
                // Description
                labels [method.Name] = attribute.Description;
            }
        }
        return labels;
    }
}
