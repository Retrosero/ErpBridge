using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace ErpBridge.Portal.Session;

/// <summary>
/// Stores the session in the tab's <c>sessionStorage</c>, encrypted with ASP.NET Core Data
/// Protection: it survives a reload, dies with the tab, and scripts on the page cannot read
/// the token. A key change (new container) only means signing in again.
/// </summary>
public sealed class ProtectedSessionPersistence(ProtectedSessionStorage storage) : ISessionPersistence
{
    private const string Key = "portal-session";

    public async Task SaveAsync(PortalSessionState state) => await storage.SetAsync(Key, state);

    public async Task<PortalSessionState?> LoadAsync()
    {
        try
        {
            var result = await storage.GetAsync<PortalSessionState>(Key);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex) when (ex is CryptographicException or JSException or InvalidOperationException)
        {
            // Unreadable after a key rotation, or JS not available yet: treat as signed out.
            return null;
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await storage.DeleteAsync(Key);
        }
        catch (Exception ex) when (ex is JSException or InvalidOperationException)
        {
            // The tab is already gone; nothing to clear.
        }
    }
}
