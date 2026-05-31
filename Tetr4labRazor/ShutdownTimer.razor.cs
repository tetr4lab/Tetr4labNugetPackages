using Microsoft.AspNetCore.Components;
namespace Tetr4lab;

/// <summary>毎日指定時間帯に指定ページへ遷移する</summary>
/// <remarks>開きっぱなしのタブを指定ページに追い出す</remarks>
public partial class ShutdownTimer : ComponentBase {
    /// <summary>遷移開始時刻</summary>
    [Parameter] public TimeOnly StartTime { get; set; } = new TimeOnly (6, 0, 0);
    /// <summary>遷移終了時刻</summary>
    [Parameter] public TimeOnly EndTime { get; set; } = new TimeOnly (6, 10, 0);
    /// <summary>時刻確認間隔 (ms)</summary>
    [Parameter] public int Timertick { get; set; } = 300000;
    /// <summary>遷移先URL</summary>
    [Parameter] public string TargetUrl { get; set; } = "/";
}
