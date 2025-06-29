using Microsoft.JSInterop;
using MudBlazor;

namespace Tetr4lab;

/// <summary>スクロールマネジャの拡張</summary>
public static class ScrollManagerHelper {
    /// <summary>サイズ取得の試行回数</summary>
    private const int NumberOfTrials = 10;
    /// <summary>サイズ取得の試行間隔</summary>
    private const int InterTrialInterval = 1000 / 60;
    /// <summary>リストの着目項目へスクロール</summary>
    /// <param name="JSRuntime"></param>
    /// <param name="ScrollManager"></param>
    /// <param name="index"></param>
    /// <param name="itemSelector"></param>
    /// <param name="tableSelector"></param>
    /// <param name="behavior"></param>
    public static async Task ScrollToIndexAsync (this IScrollManager ScrollManager, IJSRuntime JSRuntime, int index, string itemSelector = "tr.mud-table-row:has(td.mud-table-cell)", string tableSelector = ".mud-table-container", ScrollBehavior behavior = ScrollBehavior.Auto) {
        var lastTableHeight = double.NaN;
        var lastItemHeight = double.NaN;
        var table = (ElementDimensions?) null;
        var item = (ElementDimensions?) null;
        for (var i = 0; i < NumberOfTrials; i++) {
            lastTableHeight = table?.Height ?? double.NaN;
            lastItemHeight = item?.Height ?? double.NaN;
            table = await JSRuntime.GetElementDimensions (tableSelector);
            item = await JSRuntime.GetElementDimensions (itemSelector);
            if (table is not null && table.Height == lastTableHeight && item is not null && item.Height == lastItemHeight) {
                break; // レンダリングが落ち着いたら抜ける
            }
            await Task.Delay (InterTrialInterval);
        }
        if (table is null || item is null) { return; }
        var offset = item.Height * index - table.Height / 2.0; // 行高 * index - テーブル高の半分
        await ScrollManager.ScrollToAsync (tableSelector, 0, (int) (offset < 0d ? 0d : offset), behavior);
    }

}
