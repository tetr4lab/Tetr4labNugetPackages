using PetaPoco;

namespace Tetr4lab;

/// <summary>アカウントクラス</summary>
/// <remarks>
/// 初期化時にDBからアカウント情報を取得し認可ポリシーを構成する
/// 情報は一度だけ取得され、静的に保持される (事実上のシングルトンとしてセッション間で共有)
/// </remarks>
public sealed class Account {

    /// <summary>インスタンス</summary>
    public static List<Account> Users { get; private set; } = new ();

    /// <summary>有効</summary>
    public static bool IsValid { get; private set; }

    /// <summary>接続文字列</summary>
    private static string? connectionString;

    /// <summary>初期化</summary>
    /// <param name="connectionString">接続文字列</param>
    /// <returns></returns>
    public static async Task InitializeAsync (string connectionString) {
        Account.connectionString = connectionString;
        await TaskEx.DelayUntil (() => IsValid);
    }

    /// <summary>コンストラクタ</summary>
    static Account () => LoadAsync ();

    /// <summary>読み込み</summary>
    /// <exception cref="MyDataSetException"></exception>
    private static async void LoadAsync () {
        await TaskEx.DelayUntil (() => connectionString is not null);
        using (var database = (Database) new MySqlDatabase (connectionString!, "MySqlConnector")) {
            var keys = await database.GetListAsync<string> ("select `key` from policies;");
            var users = await database.GetListAsync<Account> (@"
select users.email, users.`name`, users.common_name, group_concat(policies.`key`) as policies
from users
left join assigns on assigns.users_id = users.id
left join policies on assigns.policies_id = policies.id
group by users.email
;");
            if (!users.IsSuccess || !keys.IsSuccess) {
                throw new MyDataSetException ($"Account load failure {{ {(users.IsSuccess ? "" : "users, ")} {(keys.IsSuccess ? "" : "policies, ")}}}");
            }
            Users = users.Value;
            foreach (var key in keys.Value) {
                EmailsInPolicy [key] = Users.FindAll (i => {
                    foreach (var k in i.Policies.Split (',')) {
                        if (k == key) { return true; }
                    }
                    return false;
                }).ConvertAll (i => i.Email).ToArray ();
            }
            IsValid = true;
        }
    }

    /// <summary>ポリシーに属するメール</summary>
    public static Dictionary<string, string []> EmailsInPolicy = new ();

    /// <summary>メールアドレス</summary>
    [Column ("email")] public string Email { get; set; } = "";
    /// <summary>名前</summary>
    [Column ("name")] public string Name { get; set; } = "";
    /// <summary>通称</summary>
    [Column ("common_name")] public string CommonName { get; set; } = "";
    /// <summary>ポリシー</summary>
    [Column ("policies")] public string Policies { get; set; } = "";
}
