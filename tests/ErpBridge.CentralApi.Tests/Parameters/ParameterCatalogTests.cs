using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Parameters;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Parameters;

/// <summary>
/// The parameter catalogue is the fixed point everything else resolves against: Fora stores only
/// the values that differ from a default, so a stored row means nothing without it. These tests
/// guard both the reading of the shipped catalogue and the seeding that keeps the table in step.
/// </summary>
public sealed class ParameterCatalogTests
{
    private static CentralApiDbContext NewDb() => NewDb("ParameterCatalog_" + Guid.NewGuid().ToString("N"));

    /// <summary>A second context over the same store, for asserting that a seed really saved.</summary>
    private static CentralApiDbContext NewDb(string store) =>
        new(new DbContextOptionsBuilder<CentralApiDbContext>()
            .UseInMemoryDatabase(store)
            .Options);

    private static ParameterCatalogFile.CatalogRow Row(
        string method = "MobilKullanici",
        int id = 1,
        string name = "Bir",
        string defaultValue = "0") =>
        new(
            Program: "akilli",
            CatalogMethod: method,
            ParametreId: id,
            Name: name,
            DefaultValue: defaultValue,
            DefaultSource: null,
            ScopeKind: "MobileUser",
            ScopeFields: "user",
            User: string.Empty,
            AnaGrubu: string.Empty,
            AltGrubu: string.Empty,
            Editor: "boolean",
            ReferenceKind: null,
            SecretSource: null,
            Label: "Etiket",
            TabPath: "Parametreler",
            EditorOrder: 0,
            OptionsJson: null,
            SourceBuild: "unknown");

    [Fact]
    public void The_shipped_catalogue_loads_from_the_assembly()
    {
        var catalog = ParameterCatalogFile.Load();

        // 4,713 declarations, minus the five Fora makes unreachable by reusing an id inside a
        // set, minus the mobile user's password which this product refuses to carry.
        catalog.Should().HaveCount(4707);
        catalog.Select(r => (r.CatalogMethod, r.ParametreId)).Should().OnlyHaveUniqueItems(
            "ParametreID is the real key and is unique inside a set");
        catalog.Select(r => r.Program).Distinct().Should().HaveCount(14);
    }

    [Fact]
    public void Editor_metadata_is_merged_onto_the_parameter_it_belongs_to()
    {
        var catalog = ParameterCatalogFile.Load();

        var collection = catalog.Single(r =>
            r.CatalogMethod == "MobilKullanici" && r.Name == "Goster_AnaMenu_Tahsilat");

        collection.Editor.Should().Be("boolean");
        collection.Label.Should().Be("Tahsilat girebilir");
        collection.TabPath.Should().Be("Evrak girişi / Evrak Tipleri / Tahsilat / Tediye makbuzu");
        collection.DefaultValue.Should().Be("1");

        // A parameter no Fora editor exposes still exists, just without layout.
        var hidden = catalog.Single(r => r.CatalogMethod == "MobilKullanici" && r.Name == "Vergi0Yuzde");
        hidden.Editor.Should().BeNull();
        hidden.Label.Should().BeNull();
        hidden.TabPath.Should().BeNull();
    }

    [Fact]
    public void The_mobile_users_password_never_reaches_the_catalogue()
    {
        // Fora keeps it as an ordinary parameter, encrypted with a key compiled into its own
        // binary. Carrying that secret would weaken us; the panel authenticates against
        // MobileUser.PasswordHash instead (D6).
        var catalog = ParameterCatalogFile.Load();

        catalog.Should().NotContain(r => r.CatalogMethod == "MobilKullanici" && r.Name == "Sifre");

        // Other credentials — SMTP, EDI, SQL — are settings the panel legitimately manages, and
        // they stay, marked secret so no one renders them as plain text.
        catalog.Should().Contain(r => r.CatalogMethod == "ComarchEdiGenelParametreler" && r.Name == "Sifre");
    }

    [Fact]
    public void A_corrected_program_is_written_back()
    {
        using var db = NewDb();
        ParameterCatalogSeeder.Seed(db, [Row()]);

        // The row is found by (CatalogMethod, ParametreID); a stale program would address the
        // wrong Mikro rows for the rest of its life.
        var moved = Row() with { Program = "foramikro" };
        var result = ParameterCatalogSeeder.Seed(db, [moved]);

        result.Updated.Should().Be(1);
        db.ParameterCatalog.Single().Program.Should().Be("foramikro");
    }

    [Fact]
    public void A_name_is_not_enough_to_identify_a_parameter()
    {
        var catalog = ParameterCatalogFile.Load();

        // ProjeKodu exists in both akilli and the EDI relation set, with different ids and meanings.
        var byName = catalog.Where(r => r.Name == "ProjeKodu").ToList();

        byName.Select(r => r.CatalogMethod).Should().Contain(
            ["MobilKullanici", "ComarchEdiIliskiParametreleri"]);
        byName.Select(r => r.Program).Distinct().Should().HaveCountGreaterThan(1);

        // 862 of the 3,365 distinct names are shared, so a name alone can never be a key.
        catalog.GroupBy(r => r.Name).Count(g => g.Select(r => r.CatalogMethod).Distinct().Count() > 1)
            .Should().BeGreaterThan(100);
    }

    [Fact]
    public void Credential_fields_keep_their_secret_marking()
    {
        var secrets = ParameterCatalogFile.Load().Where(r => r.Editor == "secret").ToList();

        secrets.Should().HaveCount(5);
        secrets.Should().OnlyContain(r => r.SecretSource == "designer" || r.SecretSource == "name");
    }

    [Fact]
    public void Seeding_an_empty_table_inserts_the_whole_catalogue()
    {
        using var db = NewDb();

        var result = ParameterCatalogSeeder.Seed(db, ParameterCatalogFile.Load());

        result.Added.Should().Be(4707);
        result.Updated.Should().Be(0);
        result.Deprecated.Should().Be(0);
        db.ParameterCatalog.Count().Should().Be(4707);
    }

    [Fact]
    public void Seeding_twice_changes_nothing_the_second_time()
    {
        using var db = NewDb();
        var catalog = ParameterCatalogFile.Load();

        ParameterCatalogSeeder.Seed(db, catalog);
        var second = ParameterCatalogSeeder.Seed(db, catalog);

        second.Changed.Should().BeFalse("seeding is idempotent and runs on every start");
    }

    [Fact]
    public void A_changed_default_is_written_back()
    {
        using var db = NewDb();
        ParameterCatalogSeeder.Seed(db, [Row(defaultValue: "0")]);

        var result = ParameterCatalogSeeder.Seed(db, [Row(defaultValue: "1")]);

        result.Updated.Should().Be(1);
        db.ParameterCatalog.Single().DefaultValue.Should().Be("1");
    }

    [Fact]
    public void A_withdrawn_parameter_is_deprecated_rather_than_deleted()
    {
        using var db = NewDb();
        ParameterCatalogSeeder.Seed(db, [Row(id: 1), Row(id: 2, name: "Iki")]);

        var result = ParameterCatalogSeeder.Seed(db, [Row(id: 1)]);

        result.Deprecated.Should().Be(1);
        db.ParameterCatalog.Should().HaveCount(2, "values stored against it must not be orphaned");
        db.ParameterCatalog.Single(e => e.ParametreId == 2).IsDeprecated.Should().BeTrue();
        db.ParameterCatalog.Single(e => e.ParametreId == 1).IsDeprecated.Should().BeFalse();
    }

    [Fact]
    public void A_parameter_that_comes_back_is_revived()
    {
        using var db = NewDb();
        ParameterCatalogSeeder.Seed(db, [Row(id: 1), Row(id: 2, name: "Iki")]);
        ParameterCatalogSeeder.Seed(db, [Row(id: 1)]);

        var result = ParameterCatalogSeeder.Seed(db, [Row(id: 1), Row(id: 2, name: "Iki")]);

        result.Revived.Should().Be(1);
        db.ParameterCatalog.Single(e => e.ParametreId == 2).IsDeprecated.Should().BeFalse();
    }

    [Fact]
    public void The_catalogue_cannot_say_what_this_product_honours()
    {
        using var db = NewDb();

        // Whether Sipariş Cepte honours a parameter is ours to record, not the catalogue's: a new
        // Fora build can neither set nor clear it (D16). It comes from ImplementedParameters.
        ParameterCatalogSeeder.Seed(db, [Row()]);

        db.ParameterCatalog.Single().IsImplemented.Should().BeFalse(
            "the stand-in row is not one the mobile app honours");
    }

    [Fact]
    public void A_parameter_the_app_honours_is_marked_on_every_seed()
    {
        using var db = NewDb();

        // 58 is DefaultKaynakDepoNo, which the phone reads (P4d). Derived on every seed rather
        // than only on insert, so a feature landing in a later release turns the panel's
        // "inert in this version" badge off without anyone editing the database.
        ParameterCatalogSeeder.Seed(db, [Row(id: 58)]);
        db.ParameterCatalog.Single().IsImplemented.Should().BeTrue();

        // And a hand-edited row does not get to claim otherwise: a database that says "honoured"
        // for something the app ignores is exactly the lie the badge must not tell.
        db.ParameterCatalog.Single().IsImplemented = false;
        db.SaveChanges();

        ParameterCatalogSeeder.Seed(db, [Row(id: 58)]);
        db.ParameterCatalog.Single().IsImplemented.Should().BeTrue();
    }

    [Fact]
    public void A_newly_honoured_parameter_is_saved_even_when_the_catalogue_is_identical()
    {
        var store = "ParameterCatalog_" + Guid.NewGuid().ToString("N");

        using (var db = NewDb(store))
        {
            // The state an installation is left in by a release that predates P4d: the row exists
            // and is correct, it simply predates the phone honouring 58.
            ParameterCatalogSeeder.Seed(db, [Row(id: 58)]);
            db.ParameterCatalog.Single().IsImplemented = false;
            db.SaveChanges();
        }

        // Upgrading only grew our own honoured list; the catalogue file is byte for byte the same.
        // If the flag did not count as a change, Seed would report nothing, skip SaveChanges, and
        // the panel would keep calling a working parameter inert until some unrelated field moved.
        using (var db = NewDb(store))
        {
            ParameterCatalogSeeder.Seed(db, [Row(id: 58)]).Updated.Should().Be(1);
        }

        using (var db = NewDb(store))
        {
            db.ParameterCatalog.Single().IsImplemented.Should().BeTrue();
        }
    }

    [Fact]
    public void The_same_id_in_two_sets_is_two_different_parameters()
    {
        using var db = NewDb();

        ParameterCatalogSeeder.Seed(db, [Row(method: "MobilKullanici", id: 1), Row(method: "B2B", id: 1)]);

        db.ParameterCatalog.Should().HaveCount(2);
    }
}
