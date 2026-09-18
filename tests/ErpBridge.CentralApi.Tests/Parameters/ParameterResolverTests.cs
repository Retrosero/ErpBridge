using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Parameters;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Parameters;

/// <summary>
/// Guards the heart of the parameter system: what value actually applies, and what a write does.
///
/// Fora keeps defaults inside the application and stores only deviations — a value set back to
/// its default is deleted rather than written down. Getting that wrong either fills the table
/// with defaults or, worse, leaves a stale row that keeps overriding a default nobody chose.
/// </summary>
public sealed class ParameterResolverTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CentralApiDbContext> _options;
    private readonly World _world;

    public ParameterResolverTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<CentralApiDbContext>().UseSqlite(_connection).Options;

        using var db = Db();
        db.Database.EnsureCreated();
        _world = Seed(db);
    }

    public void Dispose() => _connection.Dispose();

    private CentralApiDbContext Db() => new(_options);

    private sealed record World(
        Guid TenantId, Guid CompanyA, Guid CompanyB, Guid UserA, Guid UserB,
        Guid DepotId, Guid MenuId, Guid TemplateFieldId, Guid FirmWideId);

    private static World Seed(CentralApiDbContext db)
    {
        var tenant = new Tenant { Name = "Kiracı", Code = "K1" };
        db.Tenants.Add(tenant);

        var a = new ErpCompany { TenantId = tenant.Id, Code = "A", Name = "Firma A", SourceDatabase = "MikroDB_V15_02" };
        var b = new ErpCompany { TenantId = tenant.Id, Code = "B", Name = "Firma B", SourceDatabase = "MikroDB_V15_03" };
        db.ErpCompanies.AddRange(a, b);

        var ali = new MobileUser { TenantId = tenant.Id, Username = "ali", FullName = "Ali" };
        var veli = new MobileUser { TenantId = tenant.Id, Username = "veli", FullName = "Veli" };
        db.MobileUsers.AddRange(ali, veli);

        ParameterCatalogEntry Entry(string method, int id, string name, string def,
            string kind, string fields, int order) => new()
        {
            Program = method == "MobilKullanici" ? "akilli" : "YaziciAyarlari",
            CatalogMethod = method,
            ParametreId = id,
            Name = name,
            DefaultValue = def,
            ScopeKind = kind,
            ScopeFields = fields,
            EditorOrder = order,
            SourceBuild = "unknown",
        };

        var depot = Entry("MobilKullanici", 58, "DefaultKaynakDepoNo", "1", ParameterScopeKinds.MobileUser, "user", 1);
        var menu = Entry("MobilKullanici", 89, "Goster_AnaMenu_Tahsilat", "1", ParameterScopeKinds.MobileUser, "user", 0);
        var field = Entry("alanekle", 5, "Kolon", "1", ParameterScopeKinds.PrinterTemplate, "user,altGrubu", 0);
        var firmWide = new ParameterCatalogEntry
        {
            Program = "ForaMikro",
            CatalogMethod = "ForaMikro",
            ParametreId = 16,
            Name = "Vergi4Yuzde",
            DefaultValue = "18",
            ScopeKind = ParameterScopeKinds.None,
            ScopeFields = string.Empty,
            SourceBuild = "unknown",
        };

        db.ParameterCatalog.AddRange(depot, menu, field, firmWide);
        db.SaveChanges();

        return new World(tenant.Id, a.Id, b.Id, ali.Id, veli.Id, depot.Id, menu.Id, field.Id, firmWide.Id);
    }

    private ParameterScope UserScope(Guid? company = null, Guid? user = null) =>
        ParameterScope.ForMobileUser(_world.TenantId, company ?? _world.CompanyA, user ?? _world.UserA);

    /// <summary>Every write is attributed; these tests stand in for an operator in the panel.</summary>
    private static ParameterResolver.ChangeContext By =>
        new(ParameterChangeSources.Panel, Actor: "test");

    [Fact]
    public async Task An_untouched_parameter_reads_as_its_default()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        var values = await resolver.ResolveAsync(UserScope(), "MobilKullanici");

        values.Should().HaveCount(2);
        values.Should().OnlyContain(v => !v.IsOverridden);
        values.Single(v => v.Entry.Name == "DefaultKaynakDepoNo").Value.Should().Be("1");
    }

    [Fact]
    public async Task Parameters_come_back_in_the_order_Foras_editor_lists_them()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        var values = await resolver.ResolveAsync(UserScope(), "MobilKullanici");

        values.Select(v => v.Entry.Name).Should().Equal("Goster_AnaMenu_Tahsilat", "DefaultKaynakDepoNo");
    }

    [Fact]
    public async Task Writing_the_default_stores_nothing()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        var outcome = await resolver.SetAsync(UserScope(), _world.DepotId, "1", By);

        outcome.Should().Be(ParameterResolver.WriteOutcome.Unchanged);
        db.ParameterValues.Should().BeEmpty("Fora stores deviations, not defaults");
    }

    [Fact]
    public async Task The_first_deviation_is_inserted_and_a_later_one_updates()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        (await resolver.SetAsync(UserScope(), _world.DepotId, "3", By))
            .Should().Be(ParameterResolver.WriteOutcome.Inserted);
        (await resolver.SetAsync(UserScope(), _world.DepotId, "5", By))
            .Should().Be(ParameterResolver.WriteOutcome.Updated);
        (await resolver.SetAsync(UserScope(), _world.DepotId, "5", By))
            .Should().Be(ParameterResolver.WriteOutcome.Unchanged, "writing the same value changes nothing");

        db.ParameterValues.Should().ContainSingle().Which.Value.Should().Be("5");
        (await resolver.ResolveOneAsync(UserScope(), _world.DepotId))!.Value.Should().Be("5");
    }

    [Fact]
    public async Task Going_back_to_the_default_deletes_the_row()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);
        await resolver.SetAsync(UserScope(), _world.DepotId, "3", By);

        var outcome = await resolver.SetAsync(UserScope(), _world.DepotId, "1", By);

        outcome.Should().Be(ParameterResolver.WriteOutcome.Deleted);
        db.ParameterValues.Should().BeEmpty(
            "a stale row would keep overriding a default nobody chose");
        (await resolver.ResolveOneAsync(UserScope(), _world.DepotId))!.IsOverridden.Should().BeFalse();
    }

    [Fact]
    public async Task Reset_removes_the_deviation_whatever_it_was()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);
        await resolver.SetAsync(UserScope(), _world.DepotId, "9", By);

        (await resolver.ResetAsync(UserScope(), _world.DepotId, By))
            .Should().Be(ParameterResolver.WriteOutcome.Deleted);
        db.ParameterValues.Should().BeEmpty();
    }

    [Fact]
    public async Task One_companys_value_does_not_reach_another()
    {
        // The dimension review added: two companies are two Mikro databases with different
        // warehouses, so a shared value would point one of them at a warehouse it does not have.
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(UserScope(company: _world.CompanyA), _world.DepotId, "3", By);

        (await resolver.ResolveOneAsync(UserScope(company: _world.CompanyB), _world.DepotId))!
            .Should().BeEquivalentTo(new { Value = "1", IsOverridden = false });

        await resolver.SetAsync(UserScope(company: _world.CompanyB), _world.DepotId, "7", By);

        (await resolver.ResolveOneAsync(UserScope(company: _world.CompanyA), _world.DepotId))!.Value.Should().Be("3");
        (await resolver.ResolveOneAsync(UserScope(company: _world.CompanyB), _world.DepotId))!.Value.Should().Be("7");
    }

    [Fact]
    public async Task One_users_value_does_not_reach_another()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(UserScope(user: _world.UserA), _world.MenuId, "0", By);

        (await resolver.ResolveOneAsync(UserScope(user: _world.UserB), _world.MenuId))!
            .Should().BeEquivalentTo(new { Value = "1", IsOverridden = false });
    }

    [Fact]
    public async Task A_deleted_users_settings_do_not_pass_to_the_next_user_of_that_name()
    {
        // The reason values are keyed by MobileUser.Id: deleting a user keeps the row for history
        // and frees the username, so a replacement could otherwise inherit permissions (D5).
        using var db = Db();
        var resolver = new ParameterResolver(db);
        await resolver.SetAsync(UserScope(user: _world.UserA), _world.MenuId, "0", By);

        var ali = db.MobileUsers.Single(u => u.Id == _world.UserA);
        ali.IsActive = false;
        ali.DeletedAtUtc = DateTimeOffset.UtcNow;
        var replacement = new MobileUser { TenantId = _world.TenantId, Username = "ali", FullName = "Yeni Ali" };
        db.MobileUsers.Add(replacement);
        await db.SaveChangesAsync();

        var inherited = await resolver.ResolveOneAsync(UserScope(user: replacement.Id), _world.MenuId);

        inherited!.IsOverridden.Should().BeFalse("the new user starts from the catalogue defaults");
        inherited.Value.Should().Be("1");
    }

    [Fact]
    public async Task A_printer_field_is_addressed_by_template_and_field_together()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        var sales = ParameterScope.ForTemplateField(_world.TenantId, _world.CompanyA, "SATIS", "StokKodu");
        var returns = ParameterScope.ForTemplateField(_world.TenantId, _world.CompanyA, "IADE", "StokKodu");

        await resolver.SetAsync(sales, _world.TemplateFieldId, "5", By);

        (await resolver.ResolveOneAsync(sales, _world.TemplateFieldId))!.Value.Should().Be("5");
        (await resolver.ResolveOneAsync(returns, _world.TemplateFieldId))!.Value.Should().Be("1");
    }

    [Fact]
    public async Task A_scope_that_cannot_address_the_parameter_is_refused()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        // A mobile user's setting without saying which user would create a row no read finds and
        // the mirror cannot place in Mikro.
        var noUser = ParameterScope.ForCompany(_world.TenantId, _world.CompanyA);
        var write = () => resolver.SetAsync(noUser, _world.DepotId, "3", By);
        await write.Should().ThrowAsync<ParameterResolver.ScopeMismatchException>()
            .WithMessage("*belongs to one mobile user*");

        // …and a company-wide setting must not be filed under a user.
        var withUser = () => resolver.SetAsync(UserScope(), _world.FirmWideId, "20", By);
        await withUser.Should().ThrowAsync<ParameterResolver.ScopeMismatchException>()
            .WithMessage("*not addressed by mobile user*");

        // A printer field needs both names, not one.
        var halfNamed = ParameterScope.ForName(_world.TenantId, _world.CompanyA, "SATIS");
        var partial = () => resolver.SetAsync(halfNamed, _world.TemplateFieldId, "5", By);
        await partial.Should().ThrowAsync<ParameterResolver.ScopeMismatchException>()
            .WithMessage("*addressed by 2 name(s)*");

        db.ParameterValues.Should().BeEmpty();
    }

    [Fact]
    public async Task Only_the_deviations_are_listed_for_the_mirror()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(UserScope(), _world.DepotId, "3", By);
        await resolver.SetAsync(UserScope(), _world.MenuId, "1", By); // the default: stores nothing

        var overrides = await resolver.OverridesAsync(UserScope());

        overrides.Should().ContainSingle().Which.Value.Should().Be("3");
    }
}
