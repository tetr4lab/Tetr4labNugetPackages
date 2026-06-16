using Microsoft.AspNetCore.Components.Authorization;

namespace Tetr4lab.Auth;

/// <summary>認証状態プロバイダ</summary>
/// <remarks>
/// Program.cs
/// <code>builder.Services.AddScoped&lt;AuthStateProvider&gt; ();</code>
/// </remarks>
public class AuthStateProvider {
    /// <summary>プロバイダ</summary>
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    /// <summary>コンストラクタ</summary>
    /// <param name="authenticationStateProvider">インジェクト</param>
    public AuthStateProvider (AuthenticationStateProvider authenticationStateProvider) => _authenticationStateProvider = authenticationStateProvider;
    /// <summary>認証状態</summary>
    public Task<AuthenticationState> AuthState => _authenticationStateProvider.GetAuthenticationStateAsync ();
}
