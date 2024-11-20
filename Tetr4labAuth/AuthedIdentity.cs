using System.Security.Claims;

namespace Tetr4lab;

/// <summary>ID</summary>
public class AuthedIdentity {
    /// <summary>ユーザ</summary>
    public ClaimsPrincipal User { get; init; }
    /// <summary>名前</summary>
    public string? Name { get; init; }
    /// <summary>メールアドレス</summary>
    public string? EmailAddress { get; init; }
    /// <summary>コンストラクタ</summary>
    /// <param name="user">ユーザ</param>
    public AuthedIdentity (ClaimsPrincipal user) {
        User = user;
        Name = user.Identity?.Name;
        if (user.Identity is ClaimsIdentity claimsIdentity) {
            foreach (var claim in claimsIdentity.Claims) {
                if (claim.Type.EndsWith ("emailaddress")) {
                    EmailAddress = claim.Value;
                    break;
                }
            }
        }
    }
    /// <summary>識別子</summary>
    public string? Identifier => EmailAddress ?? Name;
}
