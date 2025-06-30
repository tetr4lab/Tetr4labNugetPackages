using Microsoft.JSInterop;

namespace Tetr4lab;

/// <summary>要素の位置とサイズ</summary>
public class ElementRect {
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

/// <summary>要素の位置とサイズ</summary>
internal class ElementRectPair {
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
    /// <summary>横位置</summary>
    public double X2 { get; init; }
    /// <summary>縦位置</summary>
    public double Y2 { get; init; }
    /// <summary>横幅</summary>
    public double Width2 { get; init; }
    /// <summary>縦高</summary>
    public double Height2 { get; init; }
    /// <summary>上端</summary>
    public double Top2 { get; init; }
    /// <summary>右端</summary>
    public double Right2 { get; init; }
    /// <summary>下端</summary>
    public double Bottom2 { get; init; }
    /// <summary>左端</summary>
    public double Left2 { get; init; }
    /// <summary>一方</summary>
    public ElementRect One => new ElementRect {
        X = X,
        Y = Y,
        Width = Width,
        Height = Height,
        Top = Top,
        Right = Right,
        Bottom = Bottom,
        Left = Left,
    };
    /// <summary>他方</summary>
    public ElementRect Another => new ElementRect {
        X = X2,
        Y = Y2,
        Width = Width2,
        Height = Height2,
        Top = Top2,
        Right = Right2,
        Bottom = Bottom2,
        Left = Left2,
    };
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
    public static async Task<ElementRect?> GetElementRect (this IJSRuntime JSRuntime, string selector) {
        if (string.IsNullOrEmpty (selector)) { return null; }
        return await JSRuntime.InvokeAsync<ElementRect?> ("getElementRect", selector);
    }

    /// <summary>要素の位置とサイズを得る</summary>
    /// <param name="JSRuntime"></param>
    /// <param name="selector"></param>
    /// <param name="selector2"></param>
    /// <returns></returns>
    public static async Task<(ElementRect?, ElementRect?)> GetElementRect (this IJSRuntime JSRuntime, string selector, string selector2) {
        if (string.IsNullOrEmpty (selector) || string.IsNullOrEmpty (selector2)) { return (null, null); }
        var pair = await JSRuntime.InvokeAsync<ElementRectPair?> ("getElementRectPair", selector, selector2);
        return pair is null ? (null, null) : (pair.One, pair.Another);
    }
}
