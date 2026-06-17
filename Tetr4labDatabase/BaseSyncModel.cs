using System.ComponentModel.DataAnnotations;
using PetaPoco;

namespace Tetr4lab.Db;

/// <summary>モデルに必要な静的プロパティ</summary>
public interface IBaseSyncModel<T> where T : IBaseSyncModel<T> {
    /// <summary>テーブル名</summary>
    public static abstract string TableLabel { get; }
    /// <summary>列の名前</summary>
    public static abstract Dictionary<string, string> Label { get; }
    /// <summary>`select ~`</summary>
    public static abstract string SelectSql { get; }
    /// <summary>`order by ~`</summary>
    public static abstract string OrderSql { get; }
    /// <summary>データ取得SQL表現 `select ~ order (without where)`</summary>
    public static abstract string BaseSelectSql { get; }
    /// <summary>コンストラクタの代用</summary>
    public static abstract T Create (BasicSyncDataSet dataSet);
    /// <summary>基本テーブル読み込み</summary>
    public static abstract List<T> Fetch (BasicSyncDataSet dataSet, string sql, params object [] args);
}

/// <summary>基底モデル</summary>
[PrimaryKey ("id", AutoIncrement = true), ExplicitColumns]
public abstract class BaseSyncModel<T> : IEquatable<T> where T : BaseSyncModel<T>, new() {
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
    public virtual BasicSyncDataSet? DataSet { get; set; }

    /// <summary>データセット</summary>
    public virtual List<T> Table => DataSet?.GetList<T> () ?? new ();

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

    /// <summary>コンストラクタの代用</summary>
    /// <remarks>
    /// 必要に応じて派生クラスでモデル独自の初期化を行う<br/>
    /// 上書きの際は、必ず引数の<paramref name="dataSet"/>を使い、<see cref="DataSet"/>を含めて自身のメンバーは使用してはならない
    /// </remarks>
    /// <param name="dataSet">データセット</param>
    /// <returns>モデルインスタンス</returns>
    protected virtual T CreateInstance (BasicSyncDataSet dataSet) => new () { DataSet = dataSet, };

    /// <summary>低レベル基本読み込み</summary>
    /// <remarks>
    /// 必要に応じて派生クラスでモデル独自のマッピングを行う<br/>
    /// ProcessAndCommit越しに呼ぶこと<br/>
    /// 上書きの際は、必ず引数の<paramref name="dataSet"/>を使い、<see cref="DataSet"/>を含めて自身のメンバーは使用してはならない
    /// </remarks>
    /// <param name="dataSet">データセット</param>
    /// <param name="sql">SQL</param>
    /// <param name="args">引数</param>
    /// <returns>モデルインスタンスの一覧</returns>
    protected virtual List<T> FetchInstances (BasicSyncDataSet dataSet, string sql, params object [] args) {
        var items = dataSet.database.Fetch<T> (sql, args);
        foreach (var item in items) {
            item.DataSet = dataSet;
        }
        return items;
    }

    /// <summary>クラスメソッドからインスタンスメソッドを呼ぶための静的インスタンス</summary>
    protected static T Dispatcher { get; } = new ();

    /// <summary>コンストラクタの代用</summary>
    /// <param name="dataSet">データセット</param>
    /// <returns>モデルインスタンス</returns>
    public static T Create (BasicSyncDataSet dataSet) => Dispatcher.CreateInstance (dataSet);

    /// <summary>一覧の取得</summary>
    /// <param name="dataSet">データセット</param>
    /// <param name="sql">SQL</param>
    /// <param name="args">引数</param>
    /// <returns>モデルインスタンスの一覧</returns>
    public static List<T> Fetch (BasicSyncDataSet dataSet, string sql, params object [] args) => Dispatcher.FetchInstances (dataSet, sql, args);
}
