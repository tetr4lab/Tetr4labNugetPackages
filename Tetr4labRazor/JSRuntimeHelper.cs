using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Tetr4lab;

public static partial class JSRuntimeHelper {
    /// <summary>クリップボードへコピー</summary>
    public static async Task CopyToClipboard (this IJSRuntime jsRuntime, string text)
        => await jsRuntime.InvokeVoidAsync ("navigator.clipboard.writeText", text);
}
