namespace ErpBridge.Erp.Abstractions.ChangeLog;

/// <summary>
/// Opaque resume token for a change-log read. <b>The core never interprets
/// <see cref="Value"/></b> — only the adapter that produced it knows the
/// encoding.
///
/// <para>
/// Mikro encodes a JSON map of <c>tableKey → last TriggerRECno</c>; a Logo or
/// Netsis adapter using SQL Server Change Tracking would encode a
/// <c>CHANGE_TRACKING_CURRENT_VERSION()</c> value; a REST adapter could encode
/// a webhook event id or an ISO-8601 instant. Keeping this opaque is what lets
/// the agent, the cursor store and the sync service stay ERP-agnostic.
/// </para>
/// </summary>
/// <param name="Value">
/// Adapter-defined token. Persisted verbatim by
/// <see cref="IErpSyncCursorStore"/>; empty means "never synced".
/// </param>
public sealed record ErpSyncCursor(string Value)
{
    /// <summary>The "never synced yet" cursor — the adapter should return everything it has.</summary>
    public static readonly ErpSyncCursor Start = new(string.Empty);

    /// <summary>True when this is the initial cursor (no previous sync).</summary>
    public bool IsStart => string.IsNullOrEmpty(Value);

    /// <summary>Log-safe rendering — the token can be long, so it is truncated.</summary>
    public override string ToString() =>
        IsStart ? "<start>" : (Value.Length <= 64 ? Value : Value[..64] + "…");
}
