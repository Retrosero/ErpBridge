using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// A further reason a user may open a stored file of one area — e.g. a task picture for whoever sees the task (S4), an
/// expense receipt for its owner's manager (S5). Registered in DI; the redirect endpoint asks every rule for the file's
/// area after the default checks fail.
/// </summary>
public interface IStoredFileReadRule
{
    /// <summary>The <see cref="StorageAreas"/> value this rule answers for.</summary>
    string Area { get; }

    Task<bool> AllowsAsync(CentralApiDbContext db, MobileUser user, StoredFile file, CancellationToken ct);
}

/// <summary>Who may open a stored file through <c>GET /api/v1/storage/files/{id}</c>.</summary>
public static class StoredFileAccess
{
    /// <summary>
    /// A user of the same company (the caller has checked the tenant) who manages storage (<c>action.storage.manage</c>:
    /// admin, manager) or uploaded the file; otherwise an area rule must allow it.
    /// </summary>
    public static async Task<bool> CanReadAsync(CentralApiDbContext db, MobileUser user, StoredFile file, IEnumerable<IStoredFileReadRule> rules, CancellationToken ct)
    {
        if (file.TenantId != user.TenantId) return false;
        if (RolePermissions.CanManageStorage(user) || (file.CreatedByUserId is { } uploader && uploader == user.Id)) return true;
        foreach (var rule in rules)
        {
            if (rule.Area == file.Area && await rule.AllowsAsync(db, user, file, ct)) return true;
        }
        return false;
    }
}
