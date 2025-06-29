using Microsoft.JSInterop;

namespace Tetr4lab;

/// <summary>要素の位置とサイズ</summary>
public class ElementDimensions {
    /// <summary>横位置</summary>
    public double X { get; init; }
    /// <summary>縦位置</summary>
    public double Y { get; init; }
    /// <summary>横幅</summary>
    public double Width { get; init; }
    /// <summary>縦高</summary>
    public double Height { get; init; }
    /// <summary>上端</summary>
    public double Top { get; init; }
    /// <summary>右端</summary>
    public double Right { get; init; }
    /// <summary>下端</summary>
    public double Bottom { get; init; }
    /// <summary>左端</summary>
    public double Left { get; init; }
}

/// <summary>JS連携の拡張</summary>
public static partial class JSRuntimeHelper {
    /// <summary>クリップボードへコピー</summary>
    /// <param name="jsRuntime"></param>
    /// <param name="text">クリップボードに渡す文字列</param>
    /// <returns>Task</returns>
    public static async Task CopyToClipboard (this IJSRuntime jsRuntime, string text)
        => await jsRuntime.InvokeVoidAsync ("navigator.clipboard.writeText", text);

    /// <summary>クリップボードのテキストを取得</summary>
    /// <param name="jsRuntime"></param>
    /// <returns></returns>
    public static async Task<string> GetClipboardText (this IJSRuntime jsRuntime)
        => await jsRuntime.InvokeAsync<string> ("navigator.clipboard.readText");

    /// <summary>URLを新しいタブで開く</summary>
    /// <param name="JSRuntime"></param>
    /// <param name="url"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static async Task OpenUrl (this IJSRuntime JSRuntime, string url, string target = "_blank") {
        if (!string.IsNullOrEmpty (url) && !string.IsNullOrEmpty (target)) {
            await JSRuntime.InvokeVoidAsync ("open", url, target);
        }
    }

    /// <summary>ページトップへスクロール</summary>
    /// <param name="JSRuntime"></param>
    /// <returns></returns>
    public static async Task ScrollToTop (this IJSRuntime JSRuntime) {
        await JSRuntime.InvokeVoidAsync ("scrollToTop");
    }

    /// <summary>ストリームからファイルを端末へダウンロード</summary>
    /// <param name="JSRuntime"></param>
    /// <param name="title"></param>
    /// <param name="streamRef"></param>
    /// <returns></returns>
    public static async Task DownloadFileFromStream (this IJSRuntime JSRuntime, string title, DotNetStreamReference streamRef) {
        if (!string.IsNullOrEmpty (title)) {
            await JSRuntime.InvokeVoidAsync ("downloadFileFromStream", title, streamRef);
        }
    }

    /// <summary>要素の位置とサイズを得る</summary>
    /// <param name="JSRuntime"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static async Task<ElementDimensions?> GetElementDimensions (this IJSRuntime JSRuntime, string selector) {
        if (string.IsNullOrEmpty (selector)) { return null; }
        return await JSRuntime.InvokeAsync<ElementDimensions?> ("getElementDimensions", selector);
    }
}
