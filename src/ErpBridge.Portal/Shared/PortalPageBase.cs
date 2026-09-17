using ErpBridge.Portal.Api;
using ErpBridge.Portal.Session;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Portal.Shared;

/// <summary>
/// Base of every signed-in page: restores the session on the first render, sends a visitor
/// without one to the login page, reads the user's current roles when the ones held are over a
/// minute old, sends a user whose roles do not open the page to their own home page, and turns API
/// failures into a message — or, when the server ended the session, back to the login page.
/// </summary>
public abstract class PortalPageBase : ComponentBase
{
    [Inject] protected PortalSession Session { get; set; } = default!;
    [Inject] protected ISessionPersistence Persistence { get; set; } = default!;
    [Inject] protected PortalApiClient Api { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] private ILogger<PortalPageBase> Logger { get; set; } = default!;

    /// <summary>True once the session is known to be valid and the page may show data.</summary>
    protected bool Ready { get; private set; }
    protected bool Busy { get; private set; }
    protected string? Error { get; set; }

    /// <summary>The part of the portal the page belongs to; the user's roles must open it.</summary>
    protected abstract PortalArea Requires { get; }

    /// <summary>Loads the page's data once the session is ready.</summary>
    protected abstract Task LoadAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        if (!Session.IsSignedIn && await Persistence.LoadAsync() is { } saved)
            Session.SignIn(saved);
        if (!Session.IsSignedIn)
        {
            await Persistence.ClearAsync();
            Nav.NavigateTo("login");
            return;
        }
        if (Session.NeedsRefresh && !await RefreshRolesAsync()) return;
        if (!Session.Allows(Requires))
        {
            // A typed or bookmarked address the roles do not open; the server would refuse it anyway.
            Nav.NavigateTo(Session.HomePage);
            return;
        }
        Ready = true;
        await RunAsync(LoadAsync);
    }

    /// <summary>
    /// Reads the roles again so an administrator's change reaches a session that is already open
    /// (the menu redraws through <see cref="PortalSession.Changed"/>). A failed read keeps the roles
    /// held: the server still refuses what they no longer allow.
    /// </summary>
    /// <returns>False when the session cannot go on and the page has navigated away.</returns>
    private async Task<bool> RefreshRolesAsync()
    {
        try
        {
            var user = (await Api.MeAsync()).User;
            Session.Refresh(user.FullName, user.EffectiveRoles(), user.CanApprove);
            if (Session.Snapshot() is { } current) await Persistence.SaveAsync(current);
            return true;
        }
        catch (SessionEndedException ended)
        {
            await EndSessionAsync(ended.Code);
            return false;
        }
        catch (PortalApiException denied) when (denied.Code == "PORTAL_REQUIRES_MANAGER")
        {
            // No portal role is left.
            await EndSessionAsync("PORTAL_SALES_ONLY");
            return false;
        }
        catch (Exception ex) when (ex is PortalApiException or HttpRequestException or TaskCanceledException)
        {
            return true;
        }
    }

    private async Task EndSessionAsync(string reason)
    {
        Session.SignOut();
        await Persistence.ClearAsync();
        Nav.NavigateTo("login?reason=" + Uri.EscapeDataString(reason));
    }

    /// <summary>Runs an API call with a busy flag and a readable error.</summary>
    protected async Task RunAsync(Func<Task> action)
    {
        Busy = true;
        Error = null;
        StateHasChanged();
        try
        {
            await action();
        }
        catch (SessionEndedException ended)
        {
            await EndSessionAsync(ended.Code);
            return;
        }
        catch (PortalApiException failed)
        {
            Error = failed.Message;
            LogFailure(failed.Status >= 500 ? LogLevel.Error : LogLevel.Warning, "PORTAL_API_ERROR", failed,
                "Portal page {Operation} got {ErrorCode} ({HttpStatus}) from the central API.", failed.Code, failed.Status);
        }
        catch (HttpRequestException unreachable)
        {
            Error = "Sunucuya ulaşılamadı. Bağlantınızı kontrol edip tekrar deneyin.";
            LogFailure(LogLevel.Warning, "PORTAL_UPSTREAM_UNREACHABLE", unreachable, "Portal page {Operation} could not reach the central API.");
        }
        catch (TaskCanceledException timeout)
        {
            Error = "Sunucu zamanında yanıt vermedi. Tekrar deneyin.";
            LogFailure(LogLevel.Warning, "PORTAL_UPSTREAM_TIMEOUT", timeout, "Portal page {Operation} timed out waiting for the central API.");
        }
        finally
        {
            Busy = false;
        }
        StateHasChanged();
    }

    /// <summary>
    /// Log Merkezi L2e: a failure the page showed as a message still reaches the log centre, with the page, company and
    /// user. Business refusals are warnings; a 5xx is an error.
    /// </summary>
    private void LogFailure(LogLevel level, string kind, Exception exception, string template, params object?[] values)
    {
        using (Logger.BeginScope(new Dictionary<string, object?>
        {
            ["Kind"] = kind,
            ["Operation"] = GetType().Name,
            ["TenantId"] = Session.TenantId,
            ["UserId"] = Session.UserId,
        }))
        {
            Logger.Log(level, exception is PortalApiException ? null : exception, template, [GetType().Name, .. values]);
        }
    }
}
