using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Tetr4lab;

/// <summary>AuthenticationState拡張</summary>
public static partial class AuthStateHelper {

    /// <summary>認証されたユーザがポリシーに適合するか(認可)</summary>
    /// <param name="service">認可サービス
    /// [Inject] protected IAuthorizationService AuthorizationService { get; set; } = null!;
    /// </param>
    /// <param name="id">ユーザ
    /// protected AuthedIdentity? Identity => (await AuthState).GetIdentity ();
    /// protected ClaimsPrincipal User => Identity?.User;
    /// </param>
    /// <param name="policy">適合を検証するポリシー</param>
    /// <returns>適合の真偽</returns>
    public static async Task<bool> IsAuthorizedAsync (this IAuthorizationService service, AuthedIdentity? id, string policy)
        => id?.User is not null && (await service.AuthorizeAsync (id.User, policy)).Succeeded;

    /// <summary>認証状態からIDを得る</summary>
    /// <param name="stateAsync">認証状態</param>
    /// <returns>ClaimsPrincipalを含むIdentity</returns>
    public static async Task<AuthedIdentity?> GetIdentityAsync (this Task<AuthenticationState> stateAsync)
        => new ((await stateAsync).User);

    /// <summary>認証状態からIDを得る</summary>
    /// <param name="state">認証状態</param>
    /// <returns>ClaimsPrincipalを含むIdentity</returns>
    public static AuthedIdentity? GetIdentity (this AuthenticationState state) => new (state.User);

}
