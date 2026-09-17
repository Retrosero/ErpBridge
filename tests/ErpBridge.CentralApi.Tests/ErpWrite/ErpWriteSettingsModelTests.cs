using System.Reflection;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace ErpBridge.CentralApi.Tests.ErpWrite;

/// <summary>
/// Goal GOAL_ERP_YAZIM Y1a: the company's ERP write settings and each phone user's ERP
/// counterparts. Only new tables — <c>mobile_users</c> keeps its shape.
/// </summary>
public sealed class ErpWriteSettingsModelTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string MigrationName = "ErpYazimY1aWriteSettings";
    private readonly SqliteCentralApiFactory _factory;

    public ErpWriteSettingsModelTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public void The_migration_only_creates_the_two_new_tables()
    {
        var migrationType = typeof(CentralApiDbContext).Assembly.GetTypes()
            .Single(t => typeof(Migration).IsAssignableFrom(t) && t.Name == MigrationName);
        var migration = (Migration)Activator.CreateInstance(migrationType)!;
        migration.GetType().GetCustomAttribute<MigrationAttribute>().Should().NotBeNull();

        var operations = migration.UpOperations;
        operations.OfType<CreateTableOperation>().Select(o => o.Name)
            .Should().BeEquivalentTo("erp_write_settings", "mobile_user_erp_mappings");
        operations.Should().OnlyContain(o => o is CreateTableOperation || o is CreateIndexOperation,
            "existing tables must not be altered, dropped or retyped");
    }

    [Fact]
    public async Task A_new_settings_row_starts_with_order_approved_and_the_mikro_portfolio_codes()
    {
        var (tenant, _) = await _factory.SeedTenantAsync($"ERPW-{Guid.NewGuid():N}"[..20], "ERP write settings");
        await using (var db = _factory.CreateDbContext())
        {
            db.ErpWriteSettings.Add(new ErpWriteSettings { TenantId = tenant.Id });
            await db.SaveChangesAsync();
        }

        await using var read = _factory.CreateDbContext();
        var settings = await read.ErpWriteSettings.SingleAsync(s => s.TenantId == tenant.Id);
        settings.SalesDocumentKind.Should().Be(SalesDocumentKinds.Order);
        settings.OrderApprovalMode.Should().Be(OrderApprovalModes.Approved);
        settings.InvoiceSeries.Should().BeEmpty("an empty series is Mikro's series-less numbering");
        settings.ChequePortfolioCode.Should().Be("ÇEK");
        settings.NotePortfolioCode.Should().Be("SENET");
        settings.DefaultWarehouseNo.Should().BeNull();
    }

    [Fact]
    public async Task A_user_mapping_leaves_unset_values_empty_and_goes_away_with_the_user()
    {
        var (tenant, _) = await _factory.SeedTenantAsync($"ERPM-{Guid.NewGuid():N}"[..20], "ERP user mapping");
        var user = new MobileUser { TenantId = tenant.Id, Username = "plasiyer1", FullName = "Plasiyer Bir", PasswordHash = "x" };
        await using (var db = _factory.CreateDbContext())
        {
            db.MobileUsers.Add(user);
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = user.Id, TenantId = tenant.Id, SalespersonCode = "PLS01", InvoiceSeries = "T" });
            await db.SaveChangesAsync();
        }

        await using (var read = _factory.CreateDbContext())
        {
            var mapping = await read.MobileUserErpMappings.SingleAsync(m => m.UserId == user.Id);
            mapping.Should().Match<MobileUserErpMapping>(m =>
                m.SalespersonCode == "PLS01" && m.InvoiceSeries == "T" && m.CashCode == null && m.WarehouseNo == null && m.OrderSeries == null);
        }

        await using (var db = _factory.CreateDbContext())
        {
            db.MobileUsers.Remove(await db.MobileUsers.SingleAsync(u => u.Id == user.Id));
            await db.SaveChangesAsync();
        }

        await using var after = _factory.CreateDbContext();
        (await after.MobileUserErpMappings.AnyAsync(m => m.UserId == user.Id)).Should().BeFalse();
    }
}
