using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Tetr4lab;

/// <summary>汎用のプログレスダイアログ</summary>
public partial class ProgressDialog {

    /// <summary>了承可能</summary>
    public const int Acceptable = 101;

    /// <summary>了承済みとして閉じる</summary>
    public const int Accepted = 102;

    /// <summary>キャンセル</summary>
    public const int Cancel = -1;

    /// <summary>MudBlazorniに渡される自身のインスタンス(MudDialogInstance)</summary>
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = new MudDialogInstance ();
    /// <summary>タイトル</summary>
    [Parameter] public string Title { get; set; } = "";
    /// <summary>ダイアログ本文</summary>
    [Parameter] public string Content { get; set; } = "";
    /// <summary>色</summary>
    [Parameter] public Color Color { get; set; } = Color.Info;
    /// <summary>OKボタンのラベル</summary>
    [Parameter] public string AcceptionLabel { get; set; } = "OK";
    /// <summary>OKボタンの色</summary>
    [Parameter] public Color AcceptionColor { get; set; } = Color.Success;
    /// <summary>OKボタンのアイコン</summary>
    [Parameter] public string? AcceptionIcon { get; set; } = Icons.Material.Filled.Check;
    /// <summary>Cancelボタンのラベル</summary>
    [Parameter] public string CancellationLabel { get; set; } = "";
    /// <summary>Cancelボタンの色</summary>
    [Parameter] public Color CancellationColor { get; set; } = Color.Default;
    /// <summary>Cancelボタンのアイコン</summary>
    [Parameter] public string? CancellationIcon { get; set; } = Icons.Material.Filled.Cancel;
    /// <summary>不定量</summary>
    [Parameter] public bool Indeterminate { get; set; }
    /// <summary>処理</summary>
    [Parameter] public EventCallback<Func<int, IEnumerable<string?>, bool>>? Operation { get; set; }

    /// <summary>進行度</summary>
    public int ProgressValue { get; set; } = 0;

    /// <summary>進行度メッセージ</summary>
    public IEnumerable<string?> ProgressMessages { get; set; } = [];

    /// <summary>取り消し要求</summary>
    protected bool cancelRequest { get; set; }

    /// <summary>取り消しボタン</summary>
    protected void OnPushCancelButton () => cancelRequest = true;

    /// <summary>承認</summary>
    protected void Accept () => MudDialog.Close (DialogResult.Ok (true));

    /// <summary>初期化と進行</summary>
    /// <returns></returns>
    protected async override Task OnInitializedAsync () {
        await base.OnInitializedAsync ();
        if (Operation != null) {
            await ((EventCallback<Func<int, IEnumerable<string?>, bool>>) Operation).InvokeAsync (Update);
        } else {
            ProgressValue = Acceptable;
        }
    }

    /// <summary>進捗を更新</summary>
    /// <param name="value">進行度 -1: cancel, 0~100: progress, 101: acceptable, 102: accept</param>
    /// <param name="messages">進行状況</param>
    /// <returns>中断要求</returns>
    protected bool Update (int value, IEnumerable<string?> messages) {
        ProgressValue = value;
        if (messages != null) {
            ProgressMessages = messages;
        }
        if (ProgressValue >= Accepted) {
            Accept ();
        } else if (ProgressValue <= Cancel) {
            MudDialog.Cancel ();
        } else {
            StateHasChanged ();
        }
        return cancelRequest;
    }

}

/// <summary>プログレスダイアログ拡張</summary>
public static class ProgressDialogHelper {

    /// <summary>汎用のプログレスダイアログ</summary>
    /// <param name="dialogService">ダイアログサービスインスタンス</param>
    /// <param name="operation">進行処理</param>
    /// <param name="message">メッセージ</param>
    /// <param name="title">タイトル</param>
    /// <param name="indeterminate"></param>
    /// <param name="color">バーの色</param>
    /// <param name="maxWidth">ダイアログの最大幅</param>
    /// <param name="acceptionLabel">OKボタンのラベル</param>
    /// <param name="acceptionColor">OKボタンの色</param>
    /// <param name="acceptionIcon">OKボタンのアイコン</param>
    /// <param name="cancellationLabel">キャンセルボタンのラベル</param>
    /// <param name="cancellationColor">キャンセルボタンの色</param>
    /// <param name="cancellationIcon">キャンセルボタンのアイコン</param>
    /// <returns>結果</returns>
    public static async Task<DialogResult?> Progress (this IDialogService dialogService, Action<Func<int, IEnumerable<string?>, bool>>? operation = null, string? message = null, string? title = null, bool indeterminate = false, Color color = Color.Info, MaxWidth maxWidth = MaxWidth.Small, string acceptionLabel = "Ok", Color acceptionColor = Color.Success, string? acceptionIcon = Icons.Material.Filled.Check, string cancellationLabel = "", Color cancellationColor = Color.Default, string? cancellationIcon = Icons.Material.Filled.Cancel) {
        var options = new DialogOptions { MaxWidth = maxWidth, FullWidth = true, BackdropClick = false, };
        var parameters = new DialogParameters {
            ["Content"] = message ?? "待機しています。",
            ["Color"] = color,
            ["AcceptionLabel"] = acceptionLabel,
            ["AcceptionColor"] = acceptionColor,
            ["AcceptionIcon"] = acceptionIcon,
            ["CancellationLabel"] = cancellationLabel,
            ["CancellationColor"] = cancellationColor,
            ["CancellationIcon"] = cancellationIcon,
            ["Indeterminate"] = indeterminate,
        };
        if (operation?.Target != null) {
            parameters.Add ("Operation", EventCallback.Factory.Create (operation.Target, operation));
        }
        return await (await dialogService.ShowAsync<ProgressDialog> (title ?? "待機", parameters, options)).Result;
    }

}
