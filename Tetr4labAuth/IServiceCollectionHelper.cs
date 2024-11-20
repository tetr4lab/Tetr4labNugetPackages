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
    /// <example>
    /// // クッキーとグーグルの認証を構成
    /// builder.Services.AddAuthentication (
    ///     builder.Configuration ["Authentication:Google:ClientId"]!,
    ///     builder.Configuration ["Authentication:Google:ClientSecret"]!
    /// );
    /// </example>
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
    /// <example>
    /// // メールアドレスを保持するクレームを要求する認可用のポリシーを構成
    /// await builder.Services.AddAuthorizationAsync ($"database=accounts;{builder.Configuration.GetConnectionString ("Host")}{builder.Configuration.GetConnectionString ("Account")}Allow User Variables=true;");
    /// </example>
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
    /// <example>
    /// // メールアドレスを保持するクレームを要求する認可用のポリシーを構成
    /// await builder.Services.AddAuthorizationAsync (
    ///     $"database=accounts;{builder.Configuration.GetConnectionString ("Host")}{builder.Configuration.GetConnectionString ("Account")}Allow User Variables=true;",
    ///     new () {
    ///         { "Admin", "Administrator" },
    ///         { "Users", "Family" },
    ///     }
    /// );
    /// </example>
    public static async Task AddAuthorizationAsync (this IServiceCollection services, string connectionString, Dictionary<string, string> claimConvert) {
        await Account.InitializeAsync (connectionString);
        services.AddAuthorization (options => {
            foreach (var claim in claimConvert.Keys) {
                options.AddPolicy (claim, policyBuilder => policyBuilder.RequireClaim (ClaimTypes.Email, Account.EmailsInPolicy [claimConvert [claim]]));
            }
        });
    }

    /// <summary>限定クロスオリジンを許容する名前付きCORS設定を構成</summary>
    /// <param name="services">DIサービスコレクション</param>
    /// <param name="name">名前</param>
    /// <param name="origin">ホスト</param>
    /// <example>
    /// // 名前付きCORS設定
    /// bulder.Services.AddCors ("AllowSpecificOrigin", builder.Configuration ["Authentication:Jwt:Host"]!);
    /// // ポリシー名を指定してCORS設定を有効化
    /// app.UseCors("AllowSpecificOrigin");
    /// </example>
    public static void AddCors (this IServiceCollection services, string name, string origin) {
        services.AddCors (options => {
            options.AddPolicy (name, policy =>
                policy.WithOrigins (origin)
                      .AllowAnyHeader ()
                      .AllowAnyMethod ()
                      .AllowCredentials ());
        });
    }
}
