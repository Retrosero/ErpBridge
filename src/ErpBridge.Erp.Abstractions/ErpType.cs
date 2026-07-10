namespace ErpBridge.Erp.Abstractions;

/// <summary>
/// Supported ERP back-ends. Values are stable across releases because they are
/// persisted in the local SQLite store.
/// </summary>
/// <remarks>
/// This enumeration lives in the abstractions project so adapter packages can speak
/// about their ERP family without reaching into Core. The Core project may expose
/// its own equivalent for domain-model reasons — that duplication is acknowledged
/// as a known reconciliation item (see <c>track4-deliverable.md</c>).
/// </remarks>
public enum ErpType
{
    /// <summary>Mikro ERP — V15 (RECno/RECid) and V16 (Guid/uid).</summary>
    Mikro = 1,

    /// <summary>Logo (reserved — not yet implemented).</summary>
    Logo = 2,

    /// <summary>Paraşüt (reserved — not yet implemented).</summary>
    Parasut = 3,

    /// <summary>Netsis (reserved — not yet implemented).</summary>
    Netsis = 4
}
