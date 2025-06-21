using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tetr4lab;

/// <summary>UIロックを管理する</summary>
/// <remarks><example>Program.cs でのサービス登録<code>
/// builder.Services.AddScoped&lt;<see cref="IAppLockState"/>, <see cref="AppLockState"/>&gt; ();
/// </code></example></remarks>
public interface IAppLockState {
    /// <summary>プロパティの変更を通知するイベント</summary>
    /// <remarks><example>使用例<code>
    /// @implements IDisposable
    /// @inject IAppLockState AppLockState
    /// @code {
    ///     // イベントハンドラ
    ///     protected void OnAppLockPropertyChanged (object? sender, PropertyChangedEventArgs e) {
    ///         InvokeAsync (StateHasChanged); // 再描画
    ///     }
    ///     protected override void OnInitialized () {
    ///         base.OnInitialized ();
    ///         AppLockState.PropertyChanged += OnAppLockPropertyChanged; // 購読開始
    ///     }
    ///     public void Dispose () {
    ///         AppLockState.PropertyChanged -= OnAppLockPropertyChanged; // 購読終了
    ///     }
    /// }
    /// </code></example></remarks>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>現在操作不能である</summary>
    public bool IsLocked { get; }
    /// <summary>操作不能状態の理由</summary>
    public string Reason { get; }
    /// <summary>目標値</summary>
    public int TotalProgressValue { get; }
    /// <summary>現在値</summary>
    public int CurrentProgressValue { get; }
    /// <summary>ロック状態にする</summary>
    /// <param name="reason">ロックの理由</param>
    /// <param name="totalProgressValue">進捗の完了目標値</param>
    public void Lock (string reason, int totalProgressValue);
    /// <summary>ロック状態にする</summary>
    /// <param name="reason">ロックの理由</param>
    public void Lock (string reason);
    /// <summary>ロック状態にする</summary>
    /// <param name="totalProgressValue">進捗の完了目標値</param>
    public void Lock (int totalProgressValue);
    /// <summary>ロック状態にする</summary>
    public void Lock ();
    /// <summary>ロック状態を解除する</summary>
    public void Unlock ();
    /// <summary>進捗の現在値を更新</summary>
    /// <param name="value">新しい現在値</param>
    public void UpdateProgress (int value);
    /// <summary>進捗率</summary>
    public double ProgressPercentage { get; }
}

/// <summary>UIロックを管理する</summary>
public class AppLockState : IAppLockState, INotifyPropertyChanged {
    /// <summary>プロパティの変更を通知するイベント</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>プロパティの変更を通知するヘルパーメソッド</summary>
    /// <param name="propertyName"></param>
    protected void OnPropertyChanged ([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
    }

    /// <summary>現在操作不能である</summary>
    public bool IsLocked {
        get => _isLocked;
        protected set {
            if (_isLocked != value) {
                _isLocked = value;
                OnPropertyChanged ();
            }
        }
    }
    /// <summary>操作不能の内部値</summary>
    protected bool _isLocked;

    /// <summary>操作不能状態の理由</summary>
    public string Reason {
        get => _reason;
        protected set {
            if (_reason != value) {
                _reason = value;
                OnPropertyChanged ();
            }
        }
    }
    /// <summary>理由の内部値</summary>
    protected string _reason = string.Empty;

    /// <summary>目標値</summary>
    public int TotalProgressValue {
        get => _totalProgress;
        protected set {
            if (_totalProgress != value) {
                _totalProgress = value;
                OnPropertyChanged ();
                OnPropertyChanged (nameof (ProgressPercentage));
            }
        }
    }
    /// <summary>目標内部値</summary>
    protected int _totalProgress;

    /// <summary>現在値</summary>
    public int CurrentProgressValue {
        get => _currentProgress;
        protected set {
            if (_currentProgress != value) {
                _currentProgress = value;
                OnPropertyChanged ();
                OnPropertyChanged (nameof (ProgressPercentage));
            }
        }
    }
    /// <summary>現在内部値</summary>
    protected int _currentProgress;

    /// <summary>ロック状態にする</summary>
    /// <param name="reason">ロックの理由</param>
    /// <param name="totalProgressValue">進捗の完了目標値</param>
    public void Lock (string reason, int totalProgressValue) {
        IsLocked = true;
        Reason = reason;
        TotalProgressValue = totalProgressValue < 0 ? 0 : totalProgressValue;
        CurrentProgressValue = 0;
    }

    /// <summary>ロック状態にする</summary>
    /// <param name="reason">ロックの理由</param>
    public void Lock (string reason) {
        IsLocked = true;
        Reason = reason;
        TotalProgressValue = CurrentProgressValue = 0;
    }

    /// <summary>ロック状態にする</summary>
    /// <param name="totalProgressValue">進捗の完了目標値</param>
    public void Lock (int totalProgressValue) {
        IsLocked = true;
        Reason = string.Empty;
        TotalProgressValue = totalProgressValue < 0 ? 0 : totalProgressValue;
        CurrentProgressValue = 0;
    }

    /// <summary>ロック状態にする</summary>
    public void Lock () {
        IsLocked = true;
        Reason = string.Empty;
        TotalProgressValue = CurrentProgressValue = 0;
    }

    /// <summary>ロック状態を解除する</summary>
    public void Unlock () {
        IsLocked = false;
        Reason = string.Empty;
        TotalProgressValue = 0;
        CurrentProgressValue = 0;
    }

    /// <summary>進捗の現在値を更新</summary>
    /// <param name="value">新しい現在値</param>
    public void UpdateProgress (int value) {
        CurrentProgressValue = value < 0 ? 0 : value > TotalProgressValue ? TotalProgressValue : value;
    }

    /// <summary>進捗率</summary>
    public double ProgressPercentage => 100d * CurrentProgressValue / TotalProgressValue;
}
