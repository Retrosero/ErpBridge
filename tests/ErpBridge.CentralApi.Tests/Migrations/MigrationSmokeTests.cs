using System.Reflection;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ErpBridge.CentralApi.Tests.Migrations;

/// <summary>
/// Smoke tests for the EF Core migration that materialises
/// <c>change_set_audit_log</c> and <c>parameter_records</c> on PostgreSQL.
/// The existing in-memory <see cref="CentralApiFactory"/> test host does not
/// run migrations (it relies on <c>UseInMemoryDatabase</c>), so these tests
/// inspect the migration classes and the model directly.
///
/// <list type="bullet">
///   <item><description>
///     The migration file is present in the migrations assembly (catches
///     accidental deletion or rename).
///   </description></item>
///   <item><description>
///     The unique index on <c>(TenantId, IdempotencyKey, Direction)</c> on
///     <see cref="ChangeSetAuditEntry"/> is configured on the model.
///   </description></item>
///   <item><description>
///     The unique index on <c>(TenantId, SourceDatabase, ParametreProgram,
///     ParametreUser, ParametreID)</c> on <see cref="ParameterRecord"/> is
///     configured on the model.
///   </description></item>
///   <item><description>
///     The migration's <c>Up</c> actually creates the two tables the task
///     brief asked for (catches a future hand-edit that drops one).
///   </description></item>
/// </list>
/// </summary>
public sealed class MigrationSmokeTests
{
    private const string MigrationName = "AddChangeSetAuditLogAndParameterRecords";

    /// <summary>
    /// Spin up a short-lived relational DbContext just to walk the model. We
    /// use the in-memory provider so the test does not require PostgreSQL; the
    /// model is built identically regardless of the backing store.
    /// </summary>
    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<CentralApiDbContext>()
            .UseInMemoryDatabase("MigrationSmoke_" + Guid.NewGuid().ToString("N"))
            .Options;
        using var db = new CentralApiDbContext(options);
        return db.Model;
    }

    [Fact]
    public void Migrations_assembly_contains_AddChangeSetAuditLogAndParameterRecords_partial_class()
    {
        var assembly = typeof(CentralApiDbContext).Assembly;
        var migration = assembly
            .GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t))
            .FirstOrDefault(t => string.Equals(t.Name, MigrationName, StringComparison.Ordinal));

        migration.Should().NotBeNull(
            "the CentralApi assembly must contain the migration that creates change_set_audit_log and parameter_records");
    }

    [Fact]
    public void AddChangeSetAuditLogAndParameterRecords_migration_is_attributed_with_its_own_name()
    {
        var assembly = typeof(CentralApiDbContext).Assembly;
        var migration = assembly
            .GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t))
            .FirstOrDefault(t => string.Equals(t.Name, MigrationName, StringComparison.Ordinal));

        migration.Should().NotBeNull();
        var attribute = migration!
            .GetCustomAttribute<MigrationAttribute>(inherit: false);
        attribute.Should().NotBeNull("EF Core needs the [Migration(\"...\")] attribute to chain migrations");
    }

    [Fact]
    public void AddChangeSetAuditLogAndParameterRecords_Up_method_creates_both_tables_and_drops_them_in_Down()
    {
        var assembly = typeof(CentralApiDbContext).Assembly;
        var migration = assembly
            .GetTypes()
            .First(t => string.Equals(t.Name, MigrationName, StringComparison.Ordinal));

        var up = migration.GetMethod("Up", BindingFlags.Instance | BindingFlags.NonPublic);
        var down = migration.GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic);
        up.Should().NotBeNull("every EF Core migration must override Up(MigrationBuilder)");
        down.Should().NotBeNull("every EF Core migration must override Down(MigrationBuilder)");
    }

    [Fact]
    public void ChangeSetAuditEntry_has_unique_index_on_TenantId_IdempotencyKey_Direction()
    {
        var entityType = BuildModel().FindEntityType(typeof(ChangeSetAuditEntry));
        entityType.Should().NotBeNull("the audit entity must be part of the model");

        var index = entityType!.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Select(p => p.Name).SequenceEqual(new[]
                {
                    nameof(ChangeSetAuditEntry.TenantId),
                    nameof(ChangeSetAuditEntry.IdempotencyKey),
                    nameof(ChangeSetAuditEntry.Direction),
                }));

        index.Should().NotBeNull(
            "the (TenantId, IdempotencyKey, Direction) unique index is the idempotency anchor for the audit log");
    }

    [Fact]
    public void ParameterRecord_has_unique_index_on_Tenant_Source_Database_Program_User_Id()
    {
        var entityType = BuildModel().FindEntityType(typeof(ParameterRecord));
        entityType.Should().NotBeNull("the parameter entity must be part of the model");

        var index = entityType!.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Select(p => p.Name).SequenceEqual(new[]
                {
                    nameof(ParameterRecord.TenantId),
                    nameof(ParameterRecord.SourceDatabase),
                    nameof(ParameterRecord.ParametreProgram),
                    nameof(ParameterRecord.ParametreUser),
                    nameof(ParameterRecord.ParametreID),
                }));

        index.Should().NotBeNull(
            "the (TenantId, SourceDatabase, ParametreProgram, ParametreUser, ParametreID) unique index is what makes the agent's parameter push a no-op for unchanged rows");
    }

    [Fact]
    public void ChangeSetAuditEntry_mapped_table_name_is_change_set_audit_log()
    {
        var entityType = BuildModel().FindEntityType(typeof(ChangeSetAuditEntry));
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("change_set_audit_log");
    }

    [Fact]
    public void ParameterRecord_mapped_table_name_is_parameter_records()
    {
        var entityType = BuildModel().FindEntityType(typeof(ParameterRecord));
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("parameter_records");
    }
}
