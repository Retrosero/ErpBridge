namespace ErpBridge.Portal.Session;

/// <summary>How often signed-in screens read again on their own; tests shorten it.</summary>
public sealed record PortalRefreshTiming
{
    /// <summary>
    /// The menu's "new customer requests" badge and the customer requests list (GOAL_MUSTERI_KATALOGU P7). A minute keeps a
    /// forgotten tab from hammering the server; the badge also reads on every page change.
    /// </summary>
    public TimeSpan CustomerOrders { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>The app bar's notification bell (GOAL_PANEL_GIRIS P6): tasks and catalog requests alike.</summary>
    public TimeSpan Notifications { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// The tasks page reads the whole list again at most this often, as the phone does now and then: a task the user may no
    /// longer see is left out of the changes read (Codex #253).
    /// </summary>
    public TimeSpan TasksFullRead { get; init; } = TimeSpan.FromMinutes(5);
}
