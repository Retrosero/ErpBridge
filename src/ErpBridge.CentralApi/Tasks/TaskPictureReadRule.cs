using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;

namespace ErpBridge.CentralApi.Tasks;

/// <summary>
/// A task picture in the central file store opens, through <c>GET /api/v1/storage/files/{id}</c>, to everyone who sees
/// its task (creator, assignees, followers, task managers) — the same people its task endpoint serves it to — besides
/// the uploader and storage managers (GOAL_DEPOLAMA_R2 S4).
/// </summary>
public sealed class TaskPictureReadRule : IStoredFileReadRule
{
    public string Area => StorageAreas.Task;

    public Task<bool> AllowsAsync(CentralApiDbContext db, MobileUser user, StoredFile file, CancellationToken ct) =>
        TaskService.CanSeeTaskOfAsync(db, user, file, ct);
}
