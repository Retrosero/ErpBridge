using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// İki ajan (Windows servisi ve tepsi uygulaması) aynı kiracıda birlikte iş çekebiliyor.
/// Kiralama atomik değilse ikisi de aynı işi alır ve belge Mikro'ya iki kez yazılır.
/// Bu testler kiralamanın satırın okunduğu haline bağlı olduğunu gösterir.
/// </summary>
public sealed class JobLeaseConcurrencyTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CentralApiDbContext> _options;

    public JobLeaseConcurrencyTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<CentralApiDbContext>().UseSqlite(_connection).Options;
        using var db = new CentralApiDbContext(_options);
        db.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private CentralApiDbContext Db() => new(_options);

    private async Task<Guid> SeedPendingJobAsync()
    {
        await using var db = Db();
        var tenant = new Tenant { Name = "Kiracı", Code = "K1" };
        db.Tenants.Add(tenant);
        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            ExternalId = "MOB-TD-1",
            DocumentType = "disbursement",
            PayloadJson = "{}",
            Status = JobStatus.Pending,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job.Id;
    }

    /// <summary>İki ajan aynı bekleyen işi okur; yalnızca biri kiralayabilir.</summary>
    [Fact]
    public async Task Only_one_agent_can_lease_the_same_pending_job()
    {
        var jobId = await SeedPendingJobAsync();

        await using var first = Db();
        await using var second = Db();
        var forService = await first.Jobs.SingleAsync(j => j.Id == jobId);
        var forTray = await second.Jobs.SingleAsync(j => j.Id == jobId);

        Lease(forService, 1_000);
        await first.SaveChangesAsync();

        Lease(forTray, 2_000);
        var lost = async () => await second.SaveChangesAsync();

        await lost.Should().ThrowAsync<DbUpdateConcurrencyException>(
            "ikinci ajan satırı okuduğu gibi bulamaz, işi alamaz");

        await using var check = Db();
        var stored = await check.Jobs.SingleAsync(j => j.Id == jobId);
        stored.LeasedUntilMs.Should().Be(1_000, "kazanan ilk ajandır");
        stored.RetryCount.Should().Be(1, "iş yalnızca bir kez denemeye girer");
    }

    /// <summary>Kiralama süresi dolmuş bir işi kapatma yarışı da aynı şekilde tek taraf kazanır.</summary>
    [Fact]
    public async Task Only_one_agent_can_abandon_the_same_expired_job()
    {
        var jobId = await SeedPendingJobAsync();
        await using (var setup = Db())
        {
            var job = await setup.Jobs.SingleAsync(j => j.Id == jobId);
            Lease(job, 1);
            await setup.SaveChangesAsync();
        }

        await using var first = Db();
        await using var second = Db();
        var a = await first.Jobs.SingleAsync(j => j.Id == jobId);
        var b = await second.Jobs.SingleAsync(j => j.Id == jobId);

        a.Status = JobStatus.Failed;
        a.LeasedUntilMs = null;
        await first.SaveChangesAsync();

        b.Status = JobStatus.Failed;
        b.LeasedUntilMs = null;
        var lost = async () => await second.SaveChangesAsync();

        await lost.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    private static void Lease(Job job, long until)
    {
        job.Status = JobStatus.Processing;
        job.RetryCount += 1;
        job.LeasedUntilMs = until;
        job.NextAttemptAtMs = null;
    }
}
