using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace ErpBridge.Portal.Session;

/// <summary>
/// Stores the session encrypted with ASP.NET Core Data Protection, so scripts on the page cannot
/// read the token: in the tab's <c>sessionStorage</c> (dies with the tab), or — when the user ticked
/// "Beni hatırla" — in the browser's <c>localStorage</c> for the token's 30 days. Saving to one store
/// clears the other, so a later sign-in without "remember me" does not leave a month-long token behind.
/// A key change only means signing in again.
/// </summary>
public sealed class ProtectedBrowserPersistence(ProtectedSessionStorage tab, ProtectedLocalStorage browser, TimeProvider time) : ISessionPersistence
{
    private const string Key = "portal-session";

    public async Task SaveAsync(PortalSessionState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ProtectedBrowserStorage keep = state.RememberMe ? browser : tab;
        ProtectedBrowserStorage drop = state.RememberMe ? tab : browser;
        await keep.SetAsync(Key, state);
        await DeleteAsync(drop);
    }

    /// <summary>
    /// The tab's own session first: it is the one this tab signed in with. An expired one is removed
    /// rather than returned — another tab may have signed in with "Beni hatırla" since, and returning
    /// the stale session would make the page clear that valid one too.
    /// </summary>
    public async Task<PortalSessionState?> LoadAsync() => await ReadValidAsync(tab) ?? await ReadValidAsync(browser);

    private async Task<PortalSessionState?> ReadValidAsync(ProtectedBrowserStorage store)
    {
        var state = await ReadAsync(store);
        if (state is null || state.ExpiresAtUtc > time.GetUtcNow()) return state;
        await DeleteAsync(store);
        return null;
    }

    public async Task ClearAsync()
    {
        await DeleteAsync(tab);
        await DeleteAsync(browser);
    }

    private static async Task<PortalSessionState?> ReadAsync(ProtectedBrowserStorage store)
    {
        try
        {
            var result = await store.GetAsync<PortalSessionState>(Key);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex) when (ex is CryptographicException or JSException or InvalidOperationException)
        {
            // Unreadable after a key rotation, or JS not available yet: treat as signed out.
            return null;
        }
    }

    private static async Task DeleteAsync(ProtectedBrowserStorage store)
    {
        try
        {
            await store.DeleteAsync(Key);
        }
        catch (Exception ex) when (ex is JSException or InvalidOperationException)
        {
            // The tab is already gone; nothing to clear.
        }
    }
}
