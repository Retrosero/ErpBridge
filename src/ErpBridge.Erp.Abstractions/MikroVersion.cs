namespace ErpBridge.Erp.Abstractions;

/// <summary>
/// Mikro ERP major version. Core/UI/Service/RemoteApi must never branch on this enum —
/// only the Mikro adapter uses it to pick the right identity strategy.
/// </summary>
public enum MikroVersion
{
    /// <summary>Detection failed or the version string did not match the known patterns.</summary>
    Unknown = 0,

    /// <summary>Mikro V15 — RECno / RECid identity and link pattern.</summary>
    V15 = 15,

    /// <summary>Mikro V16 — Guid / uid identity and link pattern.</summary>
    V16 = 16
}
