using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

/// <summary>DIサービスコレクション拡張</summary>
public static partial class IServiceCollectionHelper {
    /// <summary>クッキーとグーグルの認証を構成</summary>
    /// <param name="services">DIサービスコレクション</param>
    /// <param name="crlientId">Google OAuth2 クライアントID</param>
    /// <param name="clientSecret">Google OAuth2 クライアントシークレット</param>
    public static void AddAuthentication (this IServiceCollection services, string crlientId, string clientSecret) {
        services.AddAuthentication (options => {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        })
            .AddCookie ()
            .AddGoogle (options => {
                options.ClientId = crlientId;
                options.ClientSecret = clientSecret;
            });
    }

    /// <summary>メールアドレスを保持するクレームを要求する認可用のポリシーを構成 (Accountクラスに依存)</summary>
    /// <param name="services">DIサービスコレクション</param>
    /// <param name="connectionString">接続文字列</param>
    /// <returns></returns>
    public static async Task AddAuthorizationAsync (this IServiceCollection services, string connectionString) {
        await Account.InitializeAsync (connectionString);
        services.AddAuthorization (options => {
            foreach (var policy in Account.EmailsInPolicy.Keys) {
                options.AddPolicy (policy, policyBuilder => policyBuilder.RequireClaim (ClaimTypes.Email, Account.EmailsInPolicy [policy]));
            }
        });
    }

    /// <summary>メールアドレスを保持するクレームを要求する認可用のポリシーを構成 (Accountクラスに依存)</summary>
    /// <param name="services">DIサービスコレクション</param>
    /// <param name="connectionString">接続文字列</param>
    /// <param name="claimConvert">クレームの変換辞書 (実際の認可名 ⇒ Accountの登録名)</param>
    /// <returns></returns>
    public static async Task AddAuthorizationAsync (this IServiceCollection services, string connectionString, Dictionary<string, string> claimConvert) {
        await Account.InitializeAsync (connectionString);
        services.AddAuthorization (options => {
            foreach (var claim in claimConvert.Keys) {
                options.AddPolicy (claim, policyBuilder => policyBuilder.RequireClaim (ClaimTypes.Email, Account.EmailsInPolicy [claimConvert [claim]]));
            }
        });
    }
}
