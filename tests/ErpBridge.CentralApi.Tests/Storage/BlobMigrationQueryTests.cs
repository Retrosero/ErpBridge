using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Storage;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S10: the bytea move's queries translate on PostgreSQL too (the relational tests run on SQLite and cannot
/// see that) — the blob size is summed in the database (<c>length(bytea)</c>), never by loading the blobs.
/// </summary>
public sealed class BlobMigrationQueryTests
{
    [Fact]
    public void Every_query_of_the_move_translates_on_postgres()
    {
        var options = new DbContextOptionsBuilder<CentralApiDbContext>().UseNpgsql("Host=unused;Database=unused").Options;
        using var db = new CentralApiDbContext(options);

        BlobMigration.CatalogCandidateQuery(db, null).Take(50).ToQueryString().Should().Contain("catalog_image_blobs");
        BlobMigration.CatalogCandidateQuery(db, Guid.NewGuid()).Take(50).ToQueryString().Should().Contain("TenantId");
        BlobMigration.TaskCandidateQuery(db, Guid.NewGuid()).Take(50).ToQueryString().Should().Contain("task_attachment_blobs");
        BlobMigration.CatalogMovedQuery(db).ToQueryString().Should().NotContain("\"Data\"", "the check lists keys first, blobs come in batches");
        BlobMigration.TaskMovedQuery(db).ToQueryString().Should().NotContain("\"Data\"");

        foreach (var sql in new[] { BlobMigration.CatalogCountQuery(db).ToQueryString(), BlobMigration.TaskCountQuery(db).ToQueryString() })
        {
            sql.Should().Contain("GROUP BY");
            sql.Should().MatchRegex("(?i)(octet_)?length\\(");
        }
    }
}
