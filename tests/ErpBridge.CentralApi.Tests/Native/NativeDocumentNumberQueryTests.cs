using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Native;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Native;

/// <summary>
/// GOAL_PANEL_ERPSIZ E5d: <c>mobile_records.PayloadJson</c> is <c>jsonb</c> on PostgreSQL, where a text <c>LIKE</c>
/// does not exist (<c>jsonb ~~ jsonb</c>, 42883) — the relational tests run on SQLite and cannot see that, so the
/// document-number lookup's PostgreSQL translation is checked here without a server.
/// </summary>
public sealed class NativeDocumentNumberQueryTests
{
    [Fact]
    public void On_postgres_the_number_lookup_uses_jsonb_containment_not_like()
    {
        var options = new DbContextOptionsBuilder<CentralApiDbContext>()
            .UseNpgsql("Host=unused;Database=unused")
            .Options;
        using var db = new CentralApiDbContext(options);

        var sql = NativeDocumentProcessor.DocumentNumberCandidates(db, Guid.NewGuid(), "S-1-D1").ToQueryString();

        sql.Should().Contain("@>").And.NotContain("LIKE");
    }
}
