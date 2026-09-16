using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// <c>GET /health/schema</c>. Production once ran for a week on a schema eight migrations behind
/// because a failed <c>--migrate</c> does not stop the container; this endpoint turns that state
/// into a 503 an operator or monitor can see.
/// </summary>
public class HealthSchemaTests
{
    [Fact]
    public async Task A_database_missing_migrations_reports_pending_with_503()
    {
        // The SQLite test database is built with EnsureCreated, so no migration is recorded as applied.
        using var factory = new SqliteCentralApiFactory();

        var response = await factory.CreateClient().GetAsync("/health/schema");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await ReadAsync(response);
        body.GetProperty("status").GetString().Should().Be("pending");
        body.GetProperty("pending").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task A_database_with_every_migration_applied_reports_current()
    {
        using var factory = new SqliteCentralApiFactory();
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" TEXT PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL)");
            foreach (var migration in db.Database.GetMigrations())
            {
                await db.Database.ExecuteSqlRawAsync(
                    "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({0}, '10.0.0')", migration);
            }
        }

        var response = await client.GetAsync("/health/schema");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadAsync(response);
        body.GetProperty("status").GetString().Should().Be("current");
        body.GetProperty("pending").GetInt32().Should().Be(0);
        body.GetProperty("applied").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task The_in_memory_test_provider_is_not_mistaken_for_a_broken_schema()
    {
        using var factory = new CentralApiFactory();

        var response = await factory.CreateClient().GetAsync("/health/schema");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync(response)).GetProperty("status").GetString().Should().Be("not-relational");
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
}
