using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Tetr4lab;

/// <summary>クッキーを加える処理</summary>
public class CookieHandler : DelegatingHandler {
    /// <summary>送信処理の上書き</summary>
    protected override Task<HttpResponseMessage> SendAsync (
        HttpRequestMessage request, CancellationToken cancellationToken) {
        request.SetBrowserRequestCredentials (BrowserRequestCredentials.Include);
        request.Headers.Add ("X-Requested-With", "XMLHttpRequest");
        return base.SendAsync (request, cancellationToken);
    }
}
