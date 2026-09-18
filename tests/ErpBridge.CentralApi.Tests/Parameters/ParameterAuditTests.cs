using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Parameters;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Parameters;

/// <summary>
/// Guards the two things that make parameter changes accountable and cheap to follow: a counter
/// per scope so a client can ask one number instead of pulling 1,801 parameters, and an
/// append-only trail so "who changed this setting?" has an answer.
/// </summary>
public sealed class ParameterAuditTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CentralApiDbContext> _options;
    private readonly Guid _tenantId;
    private readonly Guid _companyA;
    private readonly Guid _companyB;
    private readonly Guid _userA;
    private readonly Guid _depotId;
    private readonly Guid _secretId;

    public ParameterAuditTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<CentralApiDbContext>().UseSqlite(_connection).Options;

        using var db = Db();
        db.Database.EnsureCreated();

        var tenant = new Tenant { Name = "Kiracı", Code = "K1" };
        db.Tenants.Add(tenant);

        var a = new ErpCompany { TenantId = tenant.Id, Code = "A", Name = "A", SourceDatabase = "MikroDB_V15_02" };
        var b = new ErpCompany { TenantId = tenant.Id, Code = "B", Name = "B", SourceDatabase = "MikroDB_V15_03" };
        db.ErpCompanies.AddRange(a, b);

        var ali = new MobileUser { TenantId = tenant.Id, Username = "ali", FullName = "Ali" };
        db.MobileUsers.Add(ali);

        var depot = new ParameterCatalogEntry
        {
            Program = "akilli", CatalogMethod = "MobilKullanici", ParametreId = 58,
            Name = "DefaultKaynakDepoNo", DefaultValue = "1",
            ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user",
            Editor = "integer", SourceBuild = "unknown",
        };

        var secret = new ParameterCatalogEntry
        {
            Program = "akilli", CatalogMethod = "MobilKullanici", ParametreId = 900,
            Name = "EMailSmtpPassword", DefaultValue = "",
            ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user",
            Editor = "secret", SecretSource = "designer", SourceBuild = "unknown",
        };

        db.ParameterCatalog.AddRange(depot, secret);
        db.SaveChanges();

        (_tenantId, _companyA, _companyB, _userA, _depotId, _secretId) =
            (tenant.Id, a.Id, b.Id, ali.Id, depot.Id, secret.Id);
    }

    public void Dispose() => _connection.Dispose();

    private CentralApiDbContext Db() => new(_options);

    private ParameterScope Scope(Guid? company = null) =>
        ParameterScope.ForMobileUser(_tenantId, company ?? _companyA, _userA);

    private static ParameterResolver.ChangeContext By(string source = ParameterChangeSources.Panel) =>
        new(source, Actor: "test");

    [Fact]
    public async Task An_untouched_scope_has_no_revision_yet()
    {
        using var db = Db();

        (await new ParameterResolver(db).RevisionAsync(Scope())).Should().Be(0);
    }

    [Fact]
    public async Task Every_change_raises_the_scopes_revision()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(Scope(), _depotId, "3", By());
        var afterInsert = await resolver.RevisionAsync(Scope());

        await resolver.SetAsync(Scope(), _depotId, "5", By());
        var afterUpdate = await resolver.RevisionAsync(Scope());

        await resolver.SetAsync(Scope(), _depotId, "1", By()); // back to default: deletes the row
        var afterDelete = await resolver.RevisionAsync(Scope());

        afterInsert.Should().Be(1);
        afterUpdate.Should().Be(2);
        afterDelete.Should().Be(3, "going back to the default is a change the client must see");
    }

    [Fact]
    public async Task A_write_that_changes_nothing_does_not_move_the_revision()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);
        await resolver.SetAsync(Scope(), _depotId, "3", By());

        await resolver.SetAsync(Scope(), _depotId, "3", By());

        (await resolver.RevisionAsync(Scope())).Should().Be(
            1, "an unchanged revision has to mean nothing moved");
        db.ParameterAudit.Should().ContainSingle("a no-op is not a change worth recording");
    }

    [Fact]
    public async Task One_companys_changes_do_not_move_another_companys_revision()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(Scope(_companyA), _depotId, "3", By());

        (await resolver.RevisionAsync(Scope(_companyB))).Should().Be(
            0, "a client would otherwise re-pull everything because a different company changed");
    }

    [Fact]
    public async Task The_trail_records_what_changed_and_who_changed_it()
    {
        var adminId = Guid.NewGuid();
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(Scope(), _depotId, "3",
            new ParameterResolver.ChangeContext(ParameterChangeSources.Panel, adminId, "gurbuz"));
        await resolver.SetAsync(Scope(), _depotId, "5", By());
        await resolver.ResetAsync(Scope(), _depotId, By(ParameterChangeSources.Reset));

        // Ordered in memory: SQLite cannot ORDER BY a DateTimeOffset, and only the tests use it.
        var trail = db.ParameterAudit.AsEnumerable().OrderBy(e => e.AtUtc).ToList();

        trail.Should().HaveCount(3);

        var first = trail.Single(e => e.Outcome == "Inserted");
        first.OldValue.Should().BeNull("there was no value before");
        first.NewValue.Should().Be("3");
        first.AdminUserId.Should().Be(adminId);
        first.Actor.Should().Be("gurbuz");
        first.Source.Should().Be(ParameterChangeSources.Panel);

        trail.Single(e => e.Outcome == "Updated").Should()
            .BeEquivalentTo(new { OldValue = "3", NewValue = "5" });

        var reset = trail.Single(e => e.Outcome == "Deleted");
        reset.OldValue.Should().Be("5");
        reset.NewValue.Should().BeNull("the row is gone, so the default applies again");
        reset.Source.Should().Be(ParameterChangeSources.Reset);
    }

    [Fact]
    public async Task A_credential_is_never_written_to_the_trail_in_clear()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(Scope(), _secretId, "hunter2", By());
        await resolver.SetAsync(Scope(), _secretId, "correct-horse", By());

        var trail = db.ParameterAudit.ToList();

        trail.Should().OnlyContain(e => e.IsMasked);
        trail.Should().NotContain(e => e.OldValue == "hunter2" || e.NewValue == "hunter2");
        trail.Should().NotContain(e => e.NewValue == "correct-horse");

        // Still useful: you can tell that it went from empty to something, and when.
        var update = trail.Single(e => e.Outcome == "Updated");
        update.OldValue.Should().NotBeNullOrEmpty().And.NotBe("hunter2");
        update.NewValue.Should().NotBeNullOrEmpty().And.NotBe("correct-horse");
    }

    [Fact]
    public async Task The_trail_points_at_the_parameter_by_catalogue_entry()
    {
        using var db = Db();
        var resolver = new ParameterResolver(db);

        await resolver.SetAsync(Scope(), _depotId, "3", By());

        // Never by name: 862 of the 3,365 distinct names appear in more than one set, so a trail
        // keyed by name could not say which parameter it means.
        var entry = db.ParameterAudit.Single();
        entry.ParameterCatalogEntryId.Should().Be(_depotId);
        entry.MobileUserId.Should().Be(_userA);
        entry.ErpCompanyId.Should().Be(_companyA);
    }
}
