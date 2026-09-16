using ErpBridge.Portal.Api;
using ErpBridge.Portal.Session;
using Microsoft.AspNetCore.Components;

namespace ErpBridge.Portal.Shared;

/// <summary>
/// Base of every signed-in page: restores the tab's session on the first render, sends a
/// visitor without one to the login page, and turns API failures into a message — or,
/// when the server ended the session, back to the login page.
/// </summary>
public abstract class PortalPageBase : ComponentBase
{
    [Inject] protected PortalSession Session { get; set; } = default!;
    [Inject] protected ISessionPersistence Persistence { get; set; } = default!;
    [Inject] protected PortalApiClient Api { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    /// <summary>True once the session is known to be valid and the page may show data.</summary>
    protected bool Ready { get; private set; }
    protected bool Busy { get; private set; }
    protected string? Error { get; set; }

    /// <summary>Pages only administrators may open.</summary>
    protected virtual bool AdminOnly => false;

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
        if (AdminOnly && !Session.IsAdmin)
        {
            Nav.NavigateTo("");
            return;
        }
        Ready = true;
        await RunAsync(LoadAsync);
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
            Session.SignOut();
            await Persistence.ClearAsync();
            Nav.NavigateTo("login?reason=" + Uri.EscapeDataString(ended.Code));
            return;
        }
        catch (PortalApiException failed)
        {
            Error = failed.Message;
        }
        catch (HttpRequestException)
        {
            Error = "Sunucuya ulaşılamadı. Bağlantınızı kontrol edip tekrar deneyin.";
        }
        catch (TaskCanceledException)
        {
            Error = "Sunucu zamanında yanıt vermedi. Tekrar deneyin.";
        }
        finally
        {
            Busy = false;
        }
        StateHasChanged();
    }
}
