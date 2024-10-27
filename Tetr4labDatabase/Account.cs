using PetaPoco;
using Tetr4lab;

namespace RabbitBalance.Services;

/// <summary>アカウントクラス</summary>
public sealed class Account {

    /// <summary>インスタンス</summary>
    public static List<Account> Users { get; private set; } = new ();

    /// <summary>有効</summary>
    public static bool IsValid { get; private set; }

    /// <summary>接続文字列</summary>
    private static string? connectionString;

    /// <summary>初期化 (接続文字列の設定)</summary>
    public static void Initialize (string connectionString) => Account.connectionString = connectionString;

    /// <summary>コンストラクタ</summary>
    static Account () => LoadAsync ();

    /// <summary>読み込み</summary>
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

    [Column ("email")] public string Email { get; set; } = "";
    [Column ("name")] public string Name { get; set; } = "";
    [Column ("common_name")] public string CommonName { get; set; } = "";
    [Column ("policies")] public string Policies { get; set; } = "";
}
