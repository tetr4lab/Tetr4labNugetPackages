using Microsoft.Extensions.DependencyInjection;

namespace Tetr4lab;

/// <summary>DIサービスコレクション拡張</summary>
public static partial class IServiceCollectionHelper {
    /// <summary>WebAPIを叩く際にクッキーを加える「名前付きHTTPクライアント」の生成をサービス化</summary>
    /// <param name="services">DIサービスコレクション</param>
    /// <param name="name">名前</param>
    /// <example>
    /// // クッキー付きHTTPクライアント
    /// builder.Services.AddHttpWithCookieClient ();
    /// </example>
    public static void AddHttpWithCookieClient (this IServiceCollection services, string? name = null) {
        services.AddTransient<CookieHandler> ();
        services.AddHttpClient (name ?? "fetch").AddHttpMessageHandler<CookieHandler> ();
    }
}
