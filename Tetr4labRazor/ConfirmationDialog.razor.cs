using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Tetr4lab;

/// <summary>汎用のYes/Noダイアログ</summary>
/// <remarks>MudMessageBoxの再発明</remarks>
public partial class ConfirmationDialog : ComponentBase {
    /// <summary>MudBlazorniに渡される自身のインスタンス(MudDialogInstance)</summary>
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = new MudDialogInstance ();
    /// <summary>ダイアログ本文</summary>
    [Parameter] public IEnumerable<string> Contents { get; set; } = new string [] { };
    /// <summary>OKボタンのラベル</summary>
    [Parameter] public string AcceptionLabel { get; set; } = "OK";
    /// <summary>OKボタンの色</summary>
    [Parameter] public Color AcceptionColor { get; set; } = Color.Success;
    /// <summary>OKボタンのアイコン</summary>
    [Parameter] public string? AcceptionIcon { get; set; } = Icons.Material.Filled.Check;
    /// <summary>Cancelボタンのラベル</summary>
    [Parameter] public string CancellationLabel { get; set; } = "Cancel";
    /// <summary>Cancelボタンの色</summary>
    [Parameter] public Color CancellationColor { get; set; } = Color.Default;

    /// <summary>取り消し</summary>
    protected void Cancel() => MudDialog.Cancel();

    /// <summary>承認</summary>
    protected void Accept () => MudDialog.Close (DialogResult.Ok (true));

}

/// <summary>MudDialogService拡張</summary>
public static partial class MudDialogServiceHelper {

    /// <summary>リロードの確認</summary>
    public static async Task<DialogResult?> ReloadConfirmation (this IDialogService dialogService, IEnumerable<string?> message, string cancellationLabel = "")
        => await Confirmation (dialogService, message, title: "リロードの確認", acceptionLabel: "Reload", acceptionColor: Color.Success, acceptionIcon: Icons.Material.Filled.Refresh, cancellationLabel: cancellationLabel);

    /// <summary>汎用の確認</summary>
    public static async Task<DialogResult?> Confirmation (this IDialogService dialogService, IEnumerable<string?> message, string? title = null, MaxWidth width = MaxWidth.Small, DialogPosition position = DialogPosition.Center, string acceptionLabel = "Ok", Color acceptionColor = Color.Success, string? acceptionIcon = Icons.Material.Filled.Check, string cancellationLabel = "Cancel", Color cancellationColor = Color.Default) {
        var options = new DialogOptions { MaxWidth = width, FullWidth = true, Position = position, BackdropClick = false, };
        var parameters = new DialogParameters {
            ["Contents"] = message,
            ["AcceptionLabel"] = acceptionLabel,
            ["AcceptionColor"] = acceptionColor,
            ["AcceptionIcon"] = acceptionIcon,
            ["CancellationLabel"] = cancellationLabel,
            ["CancellationColor"] = cancellationColor,
        };
        return await (await dialogService.ShowAsync<ConfirmationDialog> (title ?? "確認", parameters, options)).Result;
    }

}
