using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.Sqlite;

namespace ErpBridge.LocalStore.Stores;

/// <summary>
/// SQLite-backed <see cref="ILocalQueueStore"/>. Jobs are inserted with status
/// <c>Pending</c>; workers pull a batch ordered by <c>created_at</c> and transition
/// the row through Processing → Succeeded/Failed.
/// </summary>
public sealed class SqliteLocalQueueStore : ILocalQueueStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteLocalQueueStore(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(LocalJob job, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentException.ThrowIfNullOrWhiteSpace(job.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(job.TenantId);

        var createdAt = NormaliseUtc(job.CreatedAt);
        var updatedAt = NormaliseUtc(job.UpdatedAt);
        job.CreatedAt = createdAt;
        job.UpdatedAt = updatedAt;

        const string sql = @"
INSERT INTO local_jobs (
    id, tenant_id, job_type, external_id, payload_json, status,
    retry_count, last_error, created_at, updated_at
)
VALUES (
    @Id, @TenantId, @JobType, @ExternalId, @PayloadJson, @Status,
    @RetryCount, @LastError, @CreatedAt, @UpdatedAt
);";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            job.Id,
            job.TenantId,
            job.JobType,
            job.ExternalId,
            job.PayloadJson,
            Status = job.Status.ToString(),
            job.RetryCount,
            job.LastError,
            CreatedAt = job.CreatedAt.ToString("O"),
            UpdatedAt = job.UpdatedAt.ToString("O"),
        }, cancellationToken: ct)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocalJob>> GetPendingJobsAsync(int take, CancellationToken ct = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, "Take must be greater than zero.");
        }

        const string sql = @"
SELECT id, tenant_id AS TenantId, job_type AS JobType, external_id AS ExternalId,
       payload_json AS PayloadJson, status AS Status,
       retry_count AS RetryCount, last_error AS LastError,
       created_at AS CreatedAt, updated_at AS UpdatedAt
FROM local_jobs
WHERE status = 'Pending'
ORDER BY created_at ASC, id ASC
LIMIT @take;";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var rows = await connection.QueryAsync<LocalJobRow>(
            new CommandDefinition(sql, new { take }, cancellationToken: ct)).ConfigureAwait(false);

        return rows.Select(r => r.ToDomain()).ToList();
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(LocalJobStatus? status = null, CancellationToken ct = default)
    {
        var sql = status is null
            ? "SELECT COUNT(*) FROM local_jobs;"
            : "SELECT COUNT(*) FROM local_jobs WHERE status = @status;";

        var parameters = status is null
            ? null
            : new { status = status.Value.ToString() };

        await using var connection = await _connectionFactory.OpenAsync(ct);
        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, cancellationToken: ct));
        return count;
    }

    /// <inheritdoc />
    public Task MarkProcessingAsync(string jobId, CancellationToken ct = default) =>
        UpdateStatusAsync(jobId, "Processing", touchRetryCount: false, ct, errorMessage: null);

    /// <inheritdoc />
    public Task MarkSucceededAsync(string jobId, CancellationToken ct = default) =>
        UpdateStatusAsync(jobId, "Succeeded", touchRetryCount: false, ct, errorMessage: null);

    /// <inheritdoc />
    public Task MarkFailedAsync(string jobId, string errorMessage, CancellationToken ct = default) =>
        UpdateStatusAsync(jobId, "Failed", touchRetryCount: true, ct, errorMessage: errorMessage);

    private async Task UpdateStatusAsync(
        string jobId,
        string status,
        bool touchRetryCount,
        CancellationToken ct,
        string? errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);

        const string sql = @"
UPDATE local_jobs
SET status = @status,
    updated_at = @updatedAt,
    last_error = @lastError,
    retry_count = CASE WHEN @touchRetryCount = 1 THEN retry_count + 1 ELSE retry_count END
WHERE id = @id;";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            id = jobId,
            status,
            updatedAt = DateTime.UtcNow.ToString("O"),
            lastError = errorMessage,
            touchRetryCount = touchRetryCount ? 1 : 0,
        }, cancellationToken: ct)).ConfigureAwait(false);
    }

    private static DateTime NormaliseUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };

    private sealed class LocalJobRow
    {
        public string Id { get; set; } = string.Empty;

        public string TenantId { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public string PayloadJson { get; set; } = string.Empty;

        public string Status { get; set; } = nameof(LocalJobStatus.Pending);

        public int RetryCount { get; set; }

        public string? LastError { get; set; }

        public string CreatedAt { get; set; } = string.Empty;

        public string UpdatedAt { get; set; } = string.Empty;

        public LocalJob ToDomain() => new()
        {
            Id = Id,
            TenantId = TenantId,
            JobType = JobType,
            ExternalId = ExternalId,
            PayloadJson = PayloadJson,
            Status = Enum.TryParse<LocalJobStatus>(Status, ignoreCase: true, out var parsed)
                ? parsed
                : LocalJobStatus.Pending,
            RetryCount = RetryCount,
            LastError = LastError,
            CreatedAt = DateTime.Parse(CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
            UpdatedAt = DateTime.Parse(UpdatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
        };
    }
}
