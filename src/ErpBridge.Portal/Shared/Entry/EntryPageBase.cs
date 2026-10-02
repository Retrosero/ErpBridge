using ErpBridge.Portal.Api;
using ErpBridge.Portal.Session;

namespace ErpBridge.Portal.Shared.Entry;

/// <summary>
/// The panel's document entry pages (GOAL_PANEL_GIRIS P5): the context (owners, lists, ERP codes), the owner and date every
/// entry carries, the server's preview after each change, and the save. The server prices and checks everything; the page
/// shows its preview and sends the total the user saw. The save's <see cref="OperationId"/> is made once per submission and
/// kept until a definite answer (Codex #249): a save sent again after a lost answer is the same document, never a second one.
/// </summary>
public abstract class EntryPageBase : PortalPageBase
{
    protected EntryContextDto? Context { get; private set; }

    /// <summary>The document's owner; null = the user.</summary>
    protected Guid? OwnerUserId { get; set; }

    /// <summary><c>yyyy-MM-dd</c>.</summary>
    protected string Date { get; set; } = string.Empty;

    protected EntryPreviewDto? Preview { get; private set; }

    /// <summary>Why the server could not price the form as it is (a field missing or wrong); not a page error.</summary>
    protected string? PreviewProblem { get; private set; }

    protected EntryCreateResponse? Saved { get; private set; }

    protected string? Notice { get; set; }

    /// <summary>The current submission's key; kept across retries, dropped after a save or a reset.</summary>
    protected string? OperationId { get; private set; }

    /// <summary>The server's kind: <c>sale</c>, <c>collection</c>, <c>purchase</c>, <c>return</c>, <c>disbursement</c>, <c>expense</c>.</summary>
    protected abstract string Kind { get; }

    /// <summary>Whether the form holds enough to ask for a preview.</summary>
    protected abstract bool Complete { get; }

    /// <summary>The form as the server's request (without the key, owner, date or expected total).</summary>
    protected abstract EntryRequestBase Build();

    /// <summary>Clears the page's own fields for a new document.</summary>
    protected abstract void ClearForm();

    protected virtual Task OnContextAsync() => Task.CompletedTask;

    protected bool IsErp => Context?.IsErp == true;

    /// <summary>Saving is open when the preview is current and nothing stands in its way.</summary>
    protected bool CanSave => !Busy && Complete && Preview is not null && Preview.Refusal is null && PreviewProblem is null;

    private int _previewSequence;

    protected override async Task LoadAsync()
    {
        Context = await Api.EntryContextAsync();
        Date = Context.Today;
        await OnContextAsync();
    }

    /// <summary>Asks the server what the form would write now. Answers to older forms are dropped.</summary>
    protected async Task RefreshPreviewAsync()
    {
        var sequence = ++_previewSequence;
        Saved = null;
        if (!Complete)
        {
            Preview = null;
            PreviewProblem = null;
            return;
        }
        try
        {
            var preview = await Api.EntryPreviewAsync(Kind, Stamp(Build(), withKey: false));
            if (sequence != _previewSequence) return;
            Preview = preview;
            PreviewProblem = null;
        }
        catch (PortalApiException problem) when (problem.Status is 400 or 404)
        {
            if (sequence != _previewSequence) return;
            Preview = null;
            PreviewProblem = problem.Message;
        }
        catch (SessionEndedException ended)
        {
            await EndSessionAsync(ended.Code);
            return;
        }
        catch (Exception ex) when (ex is PortalApiException or HttpRequestException or TaskCanceledException)
        {
            if (sequence != _previewSequence) return;
            Preview = null;
            PreviewProblem = ex is PortalApiException api ? api.Message : "Sunucuya ulaşılamadı. Bağlantınızı kontrol edip tekrar deneyin.";
        }
        StateHasChanged();
    }

    // Checked before RunAsync: it marks the page busy, which closes CanSave.
    protected Task SaveAsync() => !CanSave ? Task.CompletedTask : RunAsync(async () =>
    {
        OperationId ??= Guid.NewGuid().ToString("D");
        try
        {
            var saved = await Api.EntrySaveAsync(Kind, Stamp(Build(), withKey: true));
            Saved = saved;
            Notice = $"{EntryKinds.Label(Kind)} kaydedildi." + (saved.Idempotent ? " (Daha önce kaydedilmişti.)" : string.Empty);
            OperationId = null;
            Preview = saved.Preview ?? Preview;
        }
        catch (PortalApiException changed) when (changed.Code == "PRICE_CHANGED")
        {
            // Nothing was written: show the current figures and let the user confirm them.
            await RefreshPreviewAsync();
            throw;
        }
    });

    /// <summary>Starts a new document of the same kind; the owner and date stay.</summary>
    protected void NewDocument()
    {
        ClearForm();
        Preview = null;
        PreviewProblem = null;
        Saved = null;
        Notice = null;
        Error = null;
        OperationId = null;
    }

    protected async Task OwnerChangedAsync(string? value)
    {
        OwnerUserId = Guid.TryParse(value, out var id) ? id : null;
        await RefreshPreviewAsync();
    }

    protected async Task DateChangedAsync(string? value)
    {
        Date = value ?? string.Empty;
        await RefreshPreviewAsync();
    }

    private EntryRequestBase Stamp(EntryRequestBase request, bool withKey)
    {
        request.OwnerUserId = OwnerUserId;
        request.Date = string.IsNullOrWhiteSpace(Date) ? null : Date;
        request.OperationId = withKey ? OperationId : null;
        request.ExpectedTotal = withKey ? Preview?.Total : null;
        return request;
    }

    protected static decimal? ParseDecimal(object? value) =>
        decimal.TryParse(value?.ToString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var number) ? number : null;
}
