using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace ErpBridge.Portal.Session;

/// <summary>What a paired TV keeps: its token and the names it shows.</summary>
public sealed record DisplaySession(string Token, string TenantName, string DisplayName);

/// <summary>Where a TV keeps its pairing between restarts.</summary>
public interface IDisplaySessionStore
{
    Task<DisplaySession?> LoadAsync();
    Task SaveAsync(DisplaySession session);
    Task ClearAsync();
}

/// <summary>
/// The TV's pairing, encrypted with Data Protection in the browser's <c>localStorage</c>: a TV that loses
/// power comes back to its board without a new code. Kept apart from a person's portal session.
/// </summary>
public sealed class ProtectedDisplaySessionStore(ProtectedLocalStorage storage) : IDisplaySessionStore
{
    private const string Key = "display-session";

    public async Task<DisplaySession?> LoadAsync()
    {
        try
        {
            var result = await storage.GetAsync<DisplaySession>(Key);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex) when (ex is CryptographicException or JSException or InvalidOperationException)
        {
            return null;
        }
    }

    public async Task SaveAsync(DisplaySession session) => await storage.SetAsync(Key, session);

    public async Task ClearAsync()
    {
        try
        {
            await storage.DeleteAsync(Key);
        }
        catch (Exception ex) when (ex is JSException or InvalidOperationException)
        {
            // The tab is gone.
        }
    }
}

/// <summary>The board's rhythm; tests shorten it.</summary>
public sealed record KioskTiming
{
    public TimeSpan PairingPoll { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>The least time between two long-polls, so an instantly answering server is never hammered.</summary>
    public TimeSpan MinPollGap { get; init; } = TimeSpan.FromSeconds(1);

    public TimeSpan Retry { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>How often clocks and durations on the board are redrawn.</summary>
    public TimeSpan Tick { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>How long a page of a crowded column stays before the next one.</summary>
    public TimeSpan Rotate { get; init; } = TimeSpan.FromSeconds(10);

    public int CardsPerPage { get; init; } = 6;

    public const int LongPollSeconds = 25;
}
