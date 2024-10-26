using Microsoft.JSInterop;

namespace Tetr4lab;

/// <summary>JS連携の拡張</summary>
public static partial class JSRuntimeHelper {
    /// <summary>クリップボードへコピー</summary>
    /// <param name="jsRuntime"></param>
    /// <param name="text">クリップボードに渡す文字列</param>
    /// <returns>Task</returns>
    public static async Task CopyToClipboard (this IJSRuntime jsRuntime, string text)
        => await jsRuntime.InvokeVoidAsync ("navigator.clipboard.writeText", text);
}
