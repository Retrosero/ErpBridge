namespace ErpBridge.Core.Domain;

/// <summary>
/// Mikro ERP major version detected by the adapter. The Core project must not branch
/// on this; the value is only reported for diagnostics.
/// </summary>
public enum MikroVersion
{
    Unknown = 0,
    V15 = 15,
    V16 = 16,
}
