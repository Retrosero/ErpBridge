using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Parameters;

/// <summary>
/// Guards how a parameter value is addressed. Two dimensions here were found by review rather
/// than by design, and both silently corrupt data when missing: without the ERP company one
/// company's warehouse or document series overwrites another's, and keying a mobile user by
/// username hands a new user the permissions of a deleted one.
///
/// SQLite rather than the in-memory provider: the in-memory one ignores unique indexes, so a
/// test of "this cannot happen twice" would pass against a schema that allows it.
/// </summary>
public sealed class ParameterValueTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CentralApiDbContext> _options;

    public ParameterValueTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<CentralApiDbContext>().UseSqlite(_connection).Options;

        using var db = new CentralApiDbContext(_options);
        db.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private CentralApiDbContext Db() => new(_options);

    /// <summary>One tenant, two ERP companies, two mobile users and one catalogue parameter.</summary>
    private sealed record World(
        Guid TenantId,
        Guid CompanyA,
        Guid CompanyB,
        Guid UserA,
        Guid UserB,
        Guid ParameterId,
        Guid PrinterParameterId);

    private World Seed()
    {
        using var db = Db();

        var tenant = new Tenant { Name = "Kiracı", Code = "K1" };
        db.Tenants.Add(tenant);

        var companyA = new ErpCompany { TenantId = tenant.Id, Code = "A", Name = "Firma A", SourceDatabase = "MikroDB_V15_02" };
        var companyB = new ErpCompany { TenantId = tenant.Id, Code = "B", Name = "Firma B", SourceDatabase = "MikroDB_V15_03" };
        db.ErpCompanies.AddRange(companyA, companyB);

        var userA = new MobileUser { TenantId = tenant.Id, Username = "ali", FullName = "Ali" };
        var userB = new MobileUser { TenantId = tenant.Id, Username = "veli", FullName = "Veli" };
        db.MobileUsers.AddRange(userA, userB);

        var parameter = new ParameterCatalogEntry
        {
            Program = "akilli",
            CatalogMethod = "MobilKullanici",
            ParametreId = 58,
            Name = "DefaultKaynakDepoNo",
            DefaultValue = "1",
            ScopeKind = "MobileUser",
            ScopeFields = "user",
            SourceBuild = "unknown",
        };

        var printerField = new ParameterCatalogEntry
        {
            Program = "YaziciAyarlari",
            CatalogMethod = "alanekle",
            ParametreId = 5,
            Name = "Kolon",
            DefaultValue = "1",
            ScopeKind = "PrinterTemplate",
            ScopeFields = "user,altGrubu",
            AnaGrubu = "Alan",
            SourceBuild = "unknown",
        };

        db.ParameterCatalog.AddRange(parameter, printerField);
        db.SaveChanges();

        return new World(tenant.Id, companyA.Id, companyB.Id, userA.Id, userB.Id, parameter.Id, printerField.Id);
    }

    private static ParameterValue Value(
        World world, Guid companyId, Guid? userId = null, string value = "2",
        Guid? parameterId = null, string scope1 = "", string scope2 = "") =>
        new()
        {
            TenantId = world.TenantId,
            ErpCompanyId = companyId,
            ParameterCatalogEntryId = parameterId ?? world.ParameterId,
            MobileUserId = userId,
            Scope1 = scope1,
            Scope2 = scope2,
            Value = value,
        };

    [Fact]
    public void Two_companies_hold_their_own_value_for_the_same_parameter()
    {
        var world = Seed();
        using var db = Db();

        // Without the company dimension, setting Firma A's default warehouse would silently
        // change Firma B's as well — they are different Mikro databases with different warehouses.
        db.ParameterValues.AddRange(
            Value(world, world.CompanyA, world.UserA, "3"),
            Value(world, world.CompanyB, world.UserA, "7"));
        db.SaveChanges();

        db.ParameterValues.Should().HaveCount(2);
        db.ParameterValues.Single(v => v.ErpCompanyId == world.CompanyA).Value.Should().Be("3");
        db.ParameterValues.Single(v => v.ErpCompanyId == world.CompanyB).Value.Should().Be("7");
    }

    [Fact]
    public void Two_mobile_users_hold_their_own_value()
    {
        var world = Seed();
        using var db = Db();

        db.ParameterValues.AddRange(
            Value(world, world.CompanyA, world.UserA, "3"),
            Value(world, world.CompanyA, world.UserB, "9"));
        db.SaveChanges();

        db.ParameterValues.Should().HaveCount(2);
    }

    [Fact]
    public void The_same_parameter_cannot_be_stored_twice_for_one_scope()
    {
        var world = Seed();
        using var db = Db();

        db.ParameterValues.Add(Value(world, world.CompanyA, world.UserA, "3"));
        db.SaveChanges();

        using var second = Db();
        second.ParameterValues.Add(Value(world, world.CompanyA, world.UserA, "4"));

        var save = () => second.SaveChanges();

        save.Should().Throw<DbUpdateException>("a parameter has one value per scope instance");
    }

    [Fact]
    public void A_printer_template_field_is_addressed_by_both_scope_columns()
    {
        var world = Seed();
        using var db = Db();

        // Fora addresses these by template name and field name together, so the same field name
        // in two templates is two different values.
        db.ParameterValues.AddRange(
            Value(world, world.CompanyA, parameterId: world.PrinterParameterId, scope1: "SATIS", scope2: "StokKodu", value: "5"),
            Value(world, world.CompanyA, parameterId: world.PrinterParameterId, scope1: "IADE", scope2: "StokKodu", value: "9"));
        db.SaveChanges();

        db.ParameterValues.Should().HaveCount(2);
        db.ParameterValues.Single(v => v.Scope1 == "SATIS").Value.Should().Be("5");
    }

    [Fact]
    public void A_value_survives_the_user_being_deleted_and_the_name_being_reused()
    {
        // The exact scenario that makes a username unusable as a key: deleting a user keeps the
        // row for history and frees the name, so a new user can take it. Keyed by id, the new
        // user starts clean instead of inheriting the old one's settings (D5).
        var world = Seed();

        using (var db = Db())
        {
            db.ParameterValues.Add(Value(world, world.CompanyA, world.UserA, "3"));
            db.SaveChanges();

            var ali = db.MobileUsers.Single(u => u.Id == world.UserA);
            ali.IsActive = false;
            ali.DeletedAtUtc = DateTimeOffset.UtcNow;

            db.MobileUsers.Add(new MobileUser { TenantId = world.TenantId, Username = "ali", FullName = "Yeni Ali" });
            db.SaveChanges();
        }

        using var check = Db();

        var replacement = check.MobileUsers.Single(u => u.Username == "ali" && u.IsActive);
        check.ParameterValues.Should().ContainSingle()
            .Which.MobileUserId.Should().Be(world.UserA, "the value belongs to the deleted user, not the new one");
        check.ParameterValues.Should().NotContain(v => v.MobileUserId == replacement.Id);
    }

    [Fact]
    public void A_company_cannot_be_removed_while_values_still_point_at_it()
    {
        var world = Seed();
        using var db = Db();

        db.ParameterValues.Add(Value(world, world.CompanyA, world.UserA));
        db.SaveChanges();

        var remove = () =>
        {
            db.ErpCompanies.Remove(db.ErpCompanies.Single(c => c.Id == world.CompanyA));
            db.SaveChanges();
        };

        // EF refuses before the statement is even sent: the value's company is required and the
        // relationship does not cascade, so the removal cannot silently take its values with it.
        remove.Should().Throw<InvalidOperationException>(
            "removing a company has to deal with its values first");

        using var check = Db();
        check.ParameterValues.Should().ContainSingle();
        check.ErpCompanies.Should().Contain(c => c.Id == world.CompanyA);
    }
}
