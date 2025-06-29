using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tetr4lab;

/// <summary>アプリモード管理</summary>
/// <remarks><example>Program.cs でのサービス登録<code>
/// builder.Services.AddScoped&lt;<see cref="IAppModeService&lt;TEnum&gt;"/>, <see cref="AppModeService&lt;TEnum&gt;"/>&gt; ();
/// </code></example></remarks>
public interface IAppModeService<TEnum> where TEnum : Enum {
    /// <summary>プロパティの変更を通知するイベント</summary>
    /// <remarks><example>使用例<code>
    /// // アプリのモード
    /// public enum AppMode {
    ///     None = AppModeService&lt;AppMode&gt;.NoneMode,
    ///     Boot = AppModeService&lt;AppMode&gt;.DefaultMode,
    ///     List,
    ///     Detail,
    ///     Settings,
    /// }
    /// </code><code>
    /// @implements IDisposable
    /// @inject IAppModeService&lt;AppMode&gt; AppModeService
    /// @code {
    ///     // イベントハンドラ
    ///     protected void OnAppModePropertyChanged (object? sender, PropertyChangedEventArgs e) {
    ///         if (e.PropertyName == &quot;CurrentMode&quot;) {
    ///             InvokeAsync (StateHasChanged); // モード変更による再描画
    ///         }
    ///         if (e.PropertyName == &quot;RequestedMode&quot; &amp;&amp; sender is IAppModeService&lt;AppMode&gt; service) {
    ///             if (service.RequestedMode != AppMode.None &amp;&amp; service.RequestedMode != service.CurrentMode) {
    ///                 if (true /*or 可否判断*/) {
    ///                     service.SetMode (service.RequestedMode); // モード変更要求を受け付けて実際に変更
    ///                 }
    ///                 service.RequestMode (AppMode.None);
    ///             }
    ///         }
    ///     }
    ///     protected override void OnInitialized () {
    ///         base.OnInitialized ();
    ///         AppModeService.PropertyChanged += OnAppModePropertyChanged; // 購読開始
    ///     }
    ///     public void Dispose () {
    ///         AppModeService.PropertyChanged -= OnAppModePropertyChanged; // 購読終了
    ///     }
    /// }
    /// </code></example></remarks>
    event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>現在のモード</summary>
    TEnum CurrentMode { get; }
    /// <summary>要求されたモード</summary>
    /// <remarks>See: <see cref="PropertyChanged"/></remarks>
    TEnum RequestedMode { get; }
    /// <summary>モードを設定</summary>
    void SetMode (TEnum mode);
    /// <summary>モードを要求</summary>
    /// <remarks>See: <see cref="PropertyChanged"/></remarks>
    void RequestMode (TEnum mode);
}

    /// <summary>アプリモード管理</summary>
public class AppModeService<TEnum> : IAppModeService<TEnum>, INotifyPropertyChanged where TEnum : Enum {

    /// <summary>抹消番号</summary>
    public const int NoneMode = -1;

    /// <summary>初期番号</summary>
    public const int DefaultMode = 0;

    /// <summary>プロパティの変更を通知するイベント</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>プロパティの変更を通知するヘルパーメソッド</summary>
    /// <param name="propertyName">変更されたプロパティとして通知に含める名前 (省略時は元の名前)</param>
    protected void OnPropertyChanged ([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
    }

    /// <summary>アプリのモード</summary>
    public virtual TEnum CurrentMode {
        get => _currentMode;
        protected set {
            if (!_currentMode.Equals (value)) {
                _currentMode = value;
                OnPropertyChanged ();
            }
        }
    }
    /// <summary>現在のモード内部値</summary>
    protected TEnum _currentMode = (TEnum) (object) DefaultMode;

    /// <summary>リクエストされたアプリモード</summary>
    public virtual TEnum RequestedMode {
        get => _requestedMode;
        protected set {
            if (!_requestedMode.Equals (value)) {
                _requestedMode = value;
                OnPropertyChanged ();
            }
        }
    }
    /// <summary>要求されたモード内部値</summary>
    protected TEnum _requestedMode = (TEnum) (object) NoneMode;

    /// <summary>モードを設定</summary>
    /// <param name="mode">新しいモード</param>
    public virtual void SetMode (TEnum mode) {
        CurrentMode = mode;
    }

    /// <summary>モードをリクエスト</summary>
    /// <param name="mode">要求するモード</param>
    public virtual void RequestMode (TEnum mode) {
        RequestedMode = mode;
    }
}

