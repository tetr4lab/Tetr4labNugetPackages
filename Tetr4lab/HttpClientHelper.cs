using System.Net;

namespace Tetr4lab;

/// <summary>HttpClient</summary>
public static partial class HttpClientHelper {
    /// <summary>ヘッダ要求への応答ステータスコードを返す 失敗したら0を返す</summary>
    /// <param name="httpClient">クライアント</param>
    /// <param name="uri">URI</param>
    /// <returns>ステータスコード</returns>
    public static async Task<HttpStatusCode> GetStatusCodeAsync (this HttpClient httpClient, string uri) {
        var request = new HttpRequestMessage (HttpMethod.Head, uri);
        if (request != null) {
            using (var response = await httpClient.SendAsync (request)) {
                return response.StatusCode;
            }
        }
        return 0;
    }
}
