using PetaPoco;
using System.ComponentModel.DataAnnotations;

namespace Tetr4lab;

/// <summary>モデルに必要な静的プロパティ</summary>
public interface IBaseModel<T> {
    /// <summary>テーブル名</summary>
    public static abstract string TableLabel { get; }
    /// <summary>列の名前</summary>
    public static abstract Dictionary<string, string> Label { get; }
    /// <summary>データ取得SQL表現</summary>
    public static abstract string BaseSelectSql { get; }
    /// <summary>母集合の初期化</summary>
    public static abstract void InitTable (BasicDataSet set);
}

/// <summary>基底モデル</summary>
[PrimaryKey ("id", AutoIncrement = true), ExplicitColumns]
public abstract class BaseModel<T> : IEquatable<T> where T : BaseModel<T>, new() {
    /// <summary>識別子</summary>
    [Column ("id"), Required] public long Id { get; set; }
    /// <summary>バージョン</summary>
    [Column ("version"), Required] public int Version { get; set; }
    /// <summary>生成日時</summary>
    [Column ("created"), VirtualColumn] public DateTime Created { get; set; }
    /// <summary>生成者</summary>
    [Column ("creator")] public string Creator { get; set; } = "";
    /// <summary>更新日時</summary>
    [Column ("modified"), VirtualColumn] public DateTime Modified { get; set; }
    /// <summary>更新者</summary>
    [Column ("modifier")] public string Modifier { get; set; } = "";

    /// <summary>母集合</summary>
    public virtual BasicDataSet DataSet { get; set; } = default!;

    /// <summary>母集合</summary>
    public virtual List<T> Table => DataSet.GetList<T> ();

    /// <summary>母集合の初期化</summary>
    /// <param name="set"></param>
    public static void InitTable (BasicDataSet set) {
        var table = set.GetList<T> ();
        foreach (var item in table) {
            item.DataSet = set;
        }
    }

    /// <summary>検索対象 (複数のカラムを参照)</summary>
    public abstract string? [] SearchTargets { get; }

    /// <summary>クローン</summary>
    /// <returns></returns>
    public virtual T Clone ()
        => new T {
            Id = Id,
            Version = Version,
            Created = Created,
            Creator = Creator,
            Modified = Modified,
            Modifier = Modifier,
        };

    /// <summary>値のコピー</summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    public virtual T CopyTo (T destination) {
        destination.Id = Id;
        destination.Version = Version;
        destination.Created = Created;
        destination.Creator = Creator;
        destination.Modified = Modified;
        destination.Modifier = Modifier;
        return destination;
    }

    /// <summary>内容の比較</summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract bool Equals (T? other);

    /// <summary>内容の比較</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object? obj) => Equals (obj as T);

    /// <summary>ハッシュコードの取得</summary>
    /// <returns></returns>
    public abstract override int GetHashCode ();
}
