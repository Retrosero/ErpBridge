namespace ErpBridge.Erp.Abstractions.SalesOrder;

/// <summary>
/// Outcome of a <see cref="IErpAdapter.WriteSalesOrderAsync"/> call.
/// On success <see cref="Ok"/> is true and at least one of <see cref="ErpRecno"/> /
/// <see cref="ErpGuid"/> carries the ERP-assigned identifier.
/// </summary>
/// <param name="Ok">True when the write committed (or was an idempotent ack).</param>
/// <param name="ErrorCode">Stable error code when <see cref="Ok"/> is false.</param>
/// <param name="ErrorMessage">Human-readable diagnostic.</param>
/// <param name="ErpRecno">V15 RECno (int) when the adapter used RECno identity.</param>
/// <param name="ErpGuid">V16 Guid when the adapter used Guid identity.</param>
/// <param name="DocumentSeries">Echoed from payload on success.</param>
/// <param name="DocumentNumber">Echoed from payload on success.</param>
public sealed record ErpWriteResult(
    bool Ok,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    int? ErpRecno = null,
    Guid? ErpGuid = null,
    string? DocumentSeries = null,
    int? DocumentNumber = null)
{
    /// <summary>Validation failed before touching the database.</summary>
    public const string ErrorCodeValidationFailed = "ValidationFailed";

    /// <summary>Required lookup key (cari/stok/depo) did not exist.</summary>
    public const string ErrorCodeMissingLookup = "MissingLookup";

    /// <summary>The operation has not been wired up yet (reserved for MVP stubs).</summary>
    public const string ErrorCodeNotImplemented = "NotImplemented";

    /// <summary>An unexpected error occurred — see <see cref="ErrorMessage"/>.</summary>
    public const string ErrorCodeUnknown = "UnknownError";
}
