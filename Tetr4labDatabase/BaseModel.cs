using PetaPoco;
using System.ComponentModel.DataAnnotations;

namespace Tetr4lab.Db;

/// <summary>モデルに必要な静的プロパティ</summary>
public interface IBaseModel {
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
    [ResultColumn ("created")] public DateTime Created { get; set; }
    /// <summary>生成者</summary>
    [Column ("creator")] public string Creator { get; set; } = "";
    /// <summary>更新日時</summary>
    [ResultColumn ("modified")] public DateTime Modified { get; set; }
    /// <summary>更新者</summary>
    [Column ("modifier")] public string Modifier { get; set; } = "";
    /// <summary>備考</summary>
    [Column ("remarks")] public string? Remarks { get; set; }

    /// <summary>母集合</summary>
    public virtual BasicDataSet? DataSet { get; set; }

    /// <summary>母集合</summary>
    public virtual List<T> Table => DataSet?.GetList<T> () ?? new ();

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
    public virtual T Clone () => CopyTo (new ());

    /// <summary>値のコピー</summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    public virtual T CopyTo (T destination) {
        destination.DataSet = DataSet;
        destination.Id = Id;
        destination.Version = Version;
        destination.Created = Created;
        destination.Creator = Creator;
        destination.Modified = Modified;
        destination.Modifier = Modifier;
        destination.Remarks = Remarks;
        return destination;
    }

    /// <summary>内容の比較</summary>
    /// <remarks>生成・更新・バージョン関連は対象外</remarks>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Equals (T? other) =>
        other is not null
        && Id == other.Id
        && Remarks == other.Remarks
    ;

    /// <summary>内容の比較</summary>
    /// <remarks>生成・更新・バージョン関連は対象外</remarks>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object? obj) => Equals (obj as T);

    /// <summary>ハッシュコードの取得</summary>
    /// <remarks>生成・更新・バージョン関連は対象外</remarks>
    /// <returns></returns>
    public override int GetHashCode () => HashCode.Combine (Id, Remarks);
}
