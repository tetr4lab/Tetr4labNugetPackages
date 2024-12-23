﻿using Microsoft.AspNetCore.Components;

namespace Tetr4lab;

/// <summary>セッションカウンタを表示するコンポーネントとセッション数の更新通知サービス</summary>
/// <remark>
/// 起動: 回路の閉鎖を検出するCircuitHandlerをセッション毎に使う
/// ※`CircuitClosureDetector`のアセンブリは`Tetr4labServer`
/// <example>
/// builder.Services.AddScoped&lt;CircuitHandler, CircuitClosureDetector&gt; ();
/// </example>
/// セッションカウンタ: ページにコンポーネントを配置する
/// <example>&lt;SessionCounter /&gt;</example>
/// 更新通知: 初期化でSubscribeに自身と更新処理を渡す、切断時にUnsubscribeを呼ぶ
/// <example>
/// [Inject] protected CircuitHandler CircuitHandler { get; set; } = null!;
/// override void OnInitialized () {
///     SessionCounter.Subscribe (this, () => InvokeAsync (StateHasChanged));
///     if (CircuitHandler is CircuitClosureDetector handler) {
///         handler.Disconnected += id => SessionCounter.Unsubscribe (this);
///     }
/// }
/// </example>
/// </remark>
public partial class SessionCounter : ComponentBase, IDisposable {

    /// <summary>インスタンス数</summary>
    public static int Count => _instances.Count;

    /// <summary>インスタンス一覧</summary>
    protected static List<SessionCounter> _instances { get; set; } = new List<SessionCounter> ();

    /// <summary>購読者一覧</summary>
    protected static Dictionary<ComponentBase, Action> _subscribers { get; set; } = new Dictionary<ComponentBase, Action> ();

    /// <summary>登録/削除と全体更新</summary>
    /// <param name="newOne"></param>
    /// <param name="remove"></param>
    protected static void UpdateAll (SessionCounter newOne, bool remove = false) {
        if (_instances.Contains (newOne) == remove) {
            if (remove) {
                _instances.Remove (newOne);
            } else {
                _instances.Add (newOne);
            }
            foreach (var instance in _instances.FindAll (i => i != newOne)) {
                instance.UpdateOne ();
            }
            foreach (var subscriber in new Dictionary<ComponentBase, Action> (_subscribers)) {
                subscriber.Value ();
            }
        }
    }

    /// <summary>購読開始</summary>
    /// <param name="component"></param>
    /// <param name="listener"></param>
    public static void Subscribe (ComponentBase component, Action listener) => _subscribers.Add (component, listener);

    /// <summary>購読解除</summary>
    /// <param name="component"></param>
    public static void Unsubscribe (ComponentBase component) => _subscribers.Remove (component);

    /// <summary>初期化</summary>
    protected override void OnInitialized () {
        UpdateAll (this);
        base.OnInitialized ();
    }

    /// <summary>破棄</summary>
    public void Dispose () => UpdateAll (this, remove: true);

    /// <summary>単体更新</summary>
    protected void UpdateOne () => InvokeAsync (StateHasChanged);

}
