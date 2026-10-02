using ErpBridge.Portal.Api;

namespace ErpBridge.Portal.Shared.Tasks;

/// <summary>How a batch of task ops ended for the page that sent it (Codex #253).</summary>
public enum TaskSendResult
{
    /// <summary>Every op was applied (or was already: <c>duplicate</c>).</summary>
    Applied,

    /// <summary>The server answered and refused: a rejected op leaves nothing behind (<c>TaskService</c> savepoint).</summary>
    Refused,

    /// <summary>No answer, or the server failed midway: the ops may be applied. Sent again, they must be the same ops.</summary>
    Unknown,
}

public static class TaskSend
{
    /// <summary>A failure after which the ops may still have been applied.</summary>
    public static bool IsUnknown(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException || ex is PortalApiException { Status: >= 500 };
}
