using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Shared.Tests;

/// <summary>
/// Unit tests for <see cref="AgentSettingsValidation"/>. The helper is shared
/// between the WPF view-model and (potentially) the Windows Service pre-flight
/// check, so a regression here is user-visible in two places. Each rule
/// (required string, required string, required string, integer parse,
/// integer parse) gets a dedicated positive and negative test where
/// practical.
/// </summary>
public class AgentSettingsValidationTests
{
    [Fact]
    public void TryValidate_returns_true_when_all_required_fields_are_present_and_valid()
    {
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "MIKROSQL\\MIKRO",
            sqlUserName: "sa",
            mikroDatabaseName: "MIKRO16",
            companyNo: "1",
            branchNo: "1",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeTrue();
        error.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_rejects_blank_SqlServer()
    {
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "   ",
            sqlUserName: "sa",
            mikroDatabaseName: "MIKRO16",
            companyNo: "1",
            branchNo: "1",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeFalse();
        error.Should().Contain("SQL Server");
    }

    [Fact]
    public void TryValidate_rejects_null_SqlServer()
    {
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: null,
            sqlUserName: "sa",
            mikroDatabaseName: "MIKRO16",
            companyNo: "1",
            branchNo: "1",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeFalse();
        error.Should().Contain("SQL Server");
    }

    [Fact]
    public void TryValidate_rejects_blank_SqlUserName_when_SQL_auth()
    {
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "host",
            sqlUserName: "",
            mikroDatabaseName: "MIKRO16",
            companyNo: "1",
            branchNo: "1",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeFalse();
        error.Should().Contain("kullanıcı adı");
    }

    [Fact]
    public void TryValidate_accepts_blank_SqlUserName_when_Windows_Auth()
    {
        // When Windows authentication is selected the SQL user field is
        // optional — the process identity is used at the connection layer.
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "host",
            sqlUserName: "",
            mikroDatabaseName: "MIKRO16",
            companyNo: "1",
            branchNo: "1",
            useWindowsAuth: true,
            out var error);

        ok.Should().BeTrue();
        error.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_rejects_blank_MikroDatabaseName()
    {
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "host",
            sqlUserName: "sa",
            mikroDatabaseName: "  ",
            companyNo: "1",
            branchNo: "1",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeFalse();
        error.Should().Contain("database adı");
    }

    [Fact]
    public void TryValidate_rejects_non_integer_CompanyNo()
    {
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "host",
            sqlUserName: "sa",
            mikroDatabaseName: "MIKRO16",
            companyNo: "abc",
            branchNo: "1",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeFalse();
        error.Should().Contain("Firma no");
    }

    [Fact]
    public void TryValidate_rejects_blank_BranchNo()
    {
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "host",
            sqlUserName: "sa",
            mikroDatabaseName: "MIKRO16",
            companyNo: "1",
            branchNo: "   ",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeFalse();
        error.Should().Contain("Şube no");
    }

    [Fact]
    public void TryValidate_rejects_decimal_separator_CompanyNo_under_tr_culture()
    {
        // On a host where CurrentCulture is tr-TR, "1,5" parses as 1.5 in
        // TryParse(NumberStyles.Integer, tr-TR). We deliberately use the
        // invariant culture so the form must pass a culture-neutral value;
        // locale-formatted values (e.g. "1,000") must be rejected.
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "host",
            sqlUserName: "sa",
            mikroDatabaseName: "MIKRO16",
            companyNo: "1,5",
            branchNo: "1",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeFalse();
        error.Should().Contain("Firma no");
    }

    [Fact]
    public void TryValidate_accepts_zero_for_CompanyNo_and_BranchNo()
    {
        // Some Mikro databases are installed under firma/şube 0 (demo data).
        // Zero is a valid value, not a missing one.
        var ok = AgentSettingsValidation.TryValidate(
            sqlServer: "host",
            sqlUserName: "sa",
            mikroDatabaseName: "MIKRO16",
            companyNo: "0",
            branchNo: "0",
            useWindowsAuth: false,
            out var error);

        ok.Should().BeTrue();
        error.Should().BeEmpty();
    }
}