using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Tetr4lab;

/// <summary>AuthenticationState拡張</summary>
public static class AuthStateHelper {

    /// <summary>認証状態からIDを得る</summary>
    /// <param name="state">AuthenticationState</param>
    /// <returns>Identity</returns>
    public static AuthedIdentity? GetIdentity (this AuthenticationState state) {
        var user = state.User;
        var name = user.Identity?.Name;
        if (string.IsNullOrEmpty (name)) { return null; }
        if (user.Identity is ClaimsIdentity claimsIdentity) {
            foreach (var claim in claimsIdentity.Claims) {
                if (claim.Type.EndsWith ("emailaddress")) {
                    var emailAddress = claim.Value;
                    if (string.IsNullOrEmpty (emailAddress)) { return null; }
                    return new (user, name, emailAddress);
                }
            }
        }
        return null;
    }
}

/// <summary>ID</summary>
public class AuthedIdentity {
    /// <summary>ユーザ</summary>
    public ClaimsPrincipal User { get; init; }
    /// <summary>名前</summary>
    public string Name { get; init; }
    /// <summary>メールアドレス</summary>
    public string EmailAddress { get; init; }
    /// <summary>コンストラクタ</summary>
    /// <param name="user">ユーザ</param>
    /// <param name="name">名前</param>
    /// <param name="emailAddress">メールアドレス</param>
    public AuthedIdentity (ClaimsPrincipal user, string name, string emailAddress) {
        if (string.IsNullOrEmpty (name)) { throw new ArgumentException ("name", "null or empty"); }
        if (string.IsNullOrEmpty (emailAddress)) { throw new ArgumentException ("emailAddress", "null or empty"); }
        User = user;
        Name = name;
        EmailAddress = emailAddress;
    }
}
