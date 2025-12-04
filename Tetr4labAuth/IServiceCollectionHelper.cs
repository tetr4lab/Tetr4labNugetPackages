using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;

namespace Tetr4lab;

/// <summary>DIサービスコレクション拡張</summary>
public static partial class IServiceCollectionHelper {
    /// <summary>クッキーとグーグルの認証を構成</summary>
    /// <param name="services">DIサービスコレクション</param>
    /// <param name="crlientId">Google OAuth2 クライアントID</param>
    /// <param name="clientSecret">Google OAuth2 クライアントシークレット</param>
    /// <remarks>
    /// Google OAuth2を使用したクッキー認証を構成します。
    /// Program.csで、例えば以下のように使用します。
    /// <example><code>
    /// // クッキーとグーグルの認証を構成
    /// builder.Services.AddAuthentication (
    ///     builder.Configuration ["Authentication:Google:ClientId"]!,
    ///     builder.Configuration ["Authentication:Google:ClientSecret"]!
    /// );
    /// </code></example></remarks>
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
    /// <remarks>
    /// `accounts`DBの内容に基づいて認可ポリシーを構成します。
    /// Program.csで、例えば以下のように使用します。
    /// <example><code>
    /// // メールアドレスを保持するクレームを要求する認可用のポリシーを構成
    /// await builder.Services.AddAuthorizationAsync ($"database=accounts;{builder.Configuration.GetConnectionString ("Host")}{builder.Configuration.GetConnectionString ("Account")}Allow User Variables=true;");
    /// </code></example></remarks>
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
    /// <remarks>
    /// 辞書で変換された名称を使用して、`accounts`DBの内容に基づいて認可ポリシーを構成します。
    /// Program.csで、例えば以下のように使用します。
    /// <example><code>
    /// // メールアドレスを保持するクレームを要求する認可用のポリシーを構成
    /// await builder.Services.AddAuthorizationAsync (
    ///     $"database=accounts;{builder.Configuration.GetConnectionString ("Host")}{builder.Configuration.GetConnectionString ("Account")}Allow User Variables=true;",
    ///     new () {
    ///         { "Admin", "Administrator" },
    ///         { "Users", "Family" },
    ///     }
    /// );
    /// </code></example></remarks>
    public static async Task AddAuthorizationAsync (this IServiceCollection services, string connectionString, Dictionary<string, string> claimConvert) {
        await Account.InitializeAsync (connectionString);
        services.AddAuthorization (options => {
            foreach (var claim in claimConvert.Keys) {
                options.AddPolicy (claim, policyBuilder => policyBuilder.RequireClaim (ClaimTypes.Email, Account.EmailsInPolicy [claimConvert [claim]]));
            }
        });
    }
}
