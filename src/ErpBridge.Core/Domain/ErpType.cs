namespace ErpBridge.Core.Domain;

/// <summary>
/// ERP vendor enumeration. Pinned values are persisted in the local SQLite store
/// and therefore must not change between releases.
/// </summary>
public enum ErpType
{
    Mikro = 1,
    Logo = 2,
    Parasut = 3,
    Netsis = 4,
}
