using PetaPoco;
namespace Tetr4lab;

/// <summary>仮想カラム属性</summary>
/// <remarks>
/// 計算列から(PetaPocoにマッピングさせて)取り込むが、フィールドが実在しないので書き出さない<br/>
/// <see cref="ColumnAttribute"/>でなく<see cref="ResultColumnAttribute"/>を使用すれば、これを付与する必要はない
/// </remarks>
[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class VirtualColumnAttribute : Attribute {
    /// <summary>仮想カラム属性</summary>
    public VirtualColumnAttribute () { }
}
