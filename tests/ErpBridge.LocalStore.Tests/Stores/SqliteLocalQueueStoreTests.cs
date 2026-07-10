using ErpBridge.Core.Domain;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

/// <summary>
/// Tests for <see cref="SqliteLocalQueueStore"/>. Covers enqueue ordering, the
/// pending→processing→succeeded lifecycle, and the retry-count increment on failure.
/// </summary>
public class SqliteLocalQueueStoreTests : IDisposable
    {
    private readonly SqliteConnectionFactory _factory;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _keepAlive;

    public SqliteLocalQueueStoreTests()
    {
        (_factory, _keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
    }

    public void Dispose() => _keepAlive.Dispose();

    [Fact]
    public async Task Enqueue_then_GetPendingJobs_returns_jobs_in_FIFO_order()
    {
        var sut = new SqliteLocalQueueStore(_factory);

        var t0 = DateTime.UtcNow;
        await sut.EnqueueAsync(NewJob("job-1", t0));
        await sut.EnqueueAsync(NewJob("job-2", t0.AddMilliseconds(5)));
        await sut.EnqueueAsync(NewJob("job-3", t0.AddMilliseconds(10)));

        var pending = await sut.GetPendingJobsAsync(10);

        pending.Should().HaveCount(3);
        pending.Select(j => j.Id).Should().ContainInOrder("job-1", "job-2", "job-3");
        pending.All(j => j.Status == LocalJobStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task MarkProcessing_then_MarkSucceeded_transitions_job_correctly()
    {
        var sut = new SqliteLocalQueueStore(_factory);
        await sut.EnqueueAsync(NewJob("job-flow"));

        // First batch drains the pending job.
        var drained = await sut.GetPendingJobsAsync(10);
        var job = drained.Single(j => j.Id == "job-flow");
        job.Status.Should().Be(LocalJobStatus.Pending);

        await sut.MarkProcessingAsync("job-flow");
        await sut.MarkSucceededAsync("job-flow");

        // After success, no pending rows should remain.
        var pending = await sut.GetPendingJobsAsync(10);
        pending.Should().BeEmpty();

        const string sql = "SELECT status, retry_count FROM local_jobs WHERE id = @id;";
        var row = await Dapper.SqlMapper.QuerySingleAsync<(string Status, int RetryCount)>(
            _keepAlive, sql, new { id = "job-flow" });
        row.Status.Should().Be(nameof(LocalJobStatus.Succeeded));
        row.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task MarkFailed_increments_retry_count_and_records_error()
    {
        var sut = new SqliteLocalQueueStore(_factory);
        await sut.EnqueueAsync(NewJob("job-fail"));

        await sut.MarkFailedAsync("job-fail", "boom-1");
        await sut.MarkFailedAsync("job-fail", "boom-2");

        const string sql = "SELECT status, retry_count, last_error FROM local_jobs WHERE id = @id;";
        var row = await Dapper.SqlMapper.QuerySingleAsync<(string Status, int RetryCount, string? LastError)>(
            _keepAlive, sql, new { id = "job-fail" });

        row.Status.Should().Be(nameof(LocalJobStatus.Failed));
        row.RetryCount.Should().Be(2);
        row.LastError.Should().Be("boom-2");
    }

    [Fact]
    public async Task GetPendingJobs_respects_take_limit()
    {
        var sut = new SqliteLocalQueueStore(_factory);

        for (var i = 0; i < 5; i++)
        {
            await sut.EnqueueAsync(NewJob($"job-{i}"));
        }

        var firstBatch = await sut.GetPendingJobsAsync(2);
        firstBatch.Should().HaveCount(2);
    }

    [Fact]
    public async Task CountAsync_without_filter_returns_all_jobs()
    {
        var sut = new SqliteLocalQueueStore(_factory);
        await sut.EnqueueAsync(NewJob("a"));
        await sut.EnqueueAsync(NewJob("b"));
        await sut.EnqueueAsync(NewJob("c"));

        var count = await sut.CountAsync();

        count.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_with_status_filter_returns_matching_jobs_only()
    {
        var sut = new SqliteLocalQueueStore(_factory);
        await sut.EnqueueAsync(NewJob("p1"));
        await sut.EnqueueAsync(NewJob("p2"));
        await sut.EnqueueAsync(NewJob("f1"));
        await sut.MarkFailedAsync("f1", "boom");

        var pending = await sut.CountAsync(LocalJobStatus.Pending);
        var failed = await sut.CountAsync(LocalJobStatus.Failed);
        var succeeded = await sut.CountAsync(LocalJobStatus.Succeeded);

        pending.Should().Be(2);
        failed.Should().Be(1);
        succeeded.Should().Be(0);
    }

    private static LocalJob NewJob(string id, DateTime? createdAt = null) => new()
    {
        Id = id,
        TenantId = "tenant-Q",
        JobType = "SalesOrder",
        ExternalId = $"ext-{id}",
        PayloadJson = "{}",
        Status = LocalJobStatus.Pending,
        CreatedAt = createdAt ?? DateTime.UtcNow,
        UpdatedAt = createdAt ?? DateTime.UtcNow,
    };
}
