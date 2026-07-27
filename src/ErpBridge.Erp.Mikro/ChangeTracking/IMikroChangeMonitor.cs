namespace ErpBridge.Erp.Mikro.ChangeTracking;

/// <summary>SQL Server-backed change detector for Mikro source tables.</summary>
public interface IMikroChangeMonitor
{
    Task<MikroChangeBatch> PollAsync(long? lastVersion, CancellationToken ct = default);
}

public enum MikroChangeMonitorMode
{
    ChangeTracking,
    Compatibility,
}

public sealed record MikroChangeBatch(
    MikroChangeMonitorMode Mode,
    long CurrentVersion,
    IReadOnlySet<string> Sections,
    bool RequiresFullBootstrap,
    string? Warning = null);
