using Microsoft.AspNetCore.Components;

namespace Tetr4lab;

/// <summary>NavigationManager</summary>
public static partial class NavigationManagerHelper {
    /// <summary>ページのリロード</summary>
    public static void Reload (this NavigationManager manager, bool forceLoad = true) => manager.NavigateTo (manager.Uri, forceLoad);
}
