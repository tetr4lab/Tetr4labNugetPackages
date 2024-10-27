using RabbitBalance.Services;
using PetaPoco;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace RabbitBalance.Data;

/// <summary>モデルに必要な静的プロパティ</summary>
public interface IBaseModel<T> {
    /// <summary>テーブル名</summary>
    public static abstract string TableLabel { get; }
    /// <summary>列の名前</summary>
    public static abstract Dictionary<string, string> Label { get; }
    /// <summary>データ取得SQL表現</summary>
    public static abstract string BaseSelectSql { get; }
    /// <summary>年月でフィルターしたデータ取得SQL表現</summary>
    public static abstract string FilteredSelectSql (int? year, int? month);
    /// <summary>母集合の初期化</summary>
    public static abstract void InitTable (BalanceDataSet set);
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
    public virtual BalanceDataSet DataSet { get; set; } = default!;

    /// <summary>母集合</summary>
    public virtual List<T> Table => DataSet.GetAll<T> ();

    /// <summary>母集合の初期化</summary>
    public static void InitTable (BalanceDataSet set) {
        var table = set.GetAll<T> ();
        foreach (var item in table) {
            item.DataSet = set;
        }
    }

    /// <summary>年月の適合</summary>
    public abstract bool Filter (int? year = null, int? month = null);

    /// <summary>検索対象 (複数のカラムを参照)</summary>
    public abstract string? [] SearchTargets { get; }

    /// <summary>クローン</summary>
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
    public abstract bool Equals (T? other);

    /// <summary>内容の比較</summary>
    public override bool Equals (object? obj) => Equals (obj as T);

    /// <summary>ハッシュコードの取得</summary>
    public abstract override int GetHashCode ();
}

public static class BaseModelHelper {
    /// <summary>年月でフィルター</summary>
    public static List<T> Filter<T> (this List<T> list, int? year = null, int? month = null) where T: BaseModel<T>, new ()
        => year is null && month is null ? list : list.FindAll (i => i.Filter (year, month));
}
