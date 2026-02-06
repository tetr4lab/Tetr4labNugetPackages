using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tetr4lab; 

/// <summary>アプリのモード</summary>
public enum BasicAppMode {
    /// <summary>なし</summary>
    None = AppModeService<BasicAppMode>.NoneMode,
    /// <summary>起動</summary>
    Boot = AppModeService<BasicAppMode>.DefaultMode,
    /// <summary>アプリ</summary>
    AppMain,
    /// <summary>設定</summary>
    Settings,
}

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

    /// <summary>一般プロパティ</summary>
    protected virtual Dictionary<string, object> Properties { get; set; } = new ();

    /// <summary>キーが含まれているか確認</summary>
    /// <param name="key">キー</param>
    /// <returns>成否</returns>
    public virtual bool ContainsKey (string key) => Properties.ContainsKey (key);

    /// <summary>キーに対応する値の取得</summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="key">キー</param>
    /// <returns>値</returns>
    public virtual T GetProperty<T> (string key) => (T) Properties [key];

    /// <summary>キーに対応する値を設定</summary>
    /// <param name="key">キー</param>
    /// <param name="value">値</param>
    public virtual void SetProperty (string key, object value) {
        if (Properties.ContainsKey (key)){
            if (!Properties [key].Equals (value)) {
                Properties [key] = value;
            }
        } else {
            Properties.Add (key, value);
        }
        OnPropertyChanged (key);
    }

    /// <summary>アプリのモード</summary>
    public virtual TEnum CurrentMode {
        get => Properties.ContainsKey (nameof (CurrentMode)) ? GetProperty<TEnum> (nameof (CurrentMode)) : (TEnum) (object) DefaultMode;
        protected set => SetProperty (nameof (CurrentMode), value);
    }

    /// <summary>リクエストされたアプリモード</summary>
    public virtual TEnum RequestedMode {
        get => Properties.ContainsKey (nameof (RequestedMode)) ? GetProperty<TEnum> (nameof (RequestedMode)) : (TEnum) (object) NoneMode;
        protected set => SetProperty (nameof (RequestedMode), value);
    }

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
