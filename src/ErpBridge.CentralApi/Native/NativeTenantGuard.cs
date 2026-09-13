using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Native;

/// <summary>
/// Keeps the two data sources apart. A tenant without an ERP is booked by
/// <see cref="NativeDocumentProcessor"/>; an ERP upload or agent for the same
/// tenant would project ERP rows over the same keys and overwrite the cards,
/// stock and balances the phones created. Every ERP writer asks here first.
/// </summary>
public static class NativeTenantGuard
{
    public const string ErrorCode = "TENANT_IS_NATIVE";

    /// <summary>A 409 result when the tenant is native, otherwise null.</summary>
    public static async Task<IResult?> RejectErpWriteAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var native = await db.Tenants.AsNoTracking()
            .AnyAsync(t => t.Id == tenantId && t.DataSource == TenantDataSources.Native, ct);
        return native ? Rejection() : null;
    }

    public static IResult Rejection() => JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
    {
        ErrorCode = ErrorCode,
        Message = "This company has no ERP; its data is created on the phones and cannot be written by an ERP agent.",
    });
}
