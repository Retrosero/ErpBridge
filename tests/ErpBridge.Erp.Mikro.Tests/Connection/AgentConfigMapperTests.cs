using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Mikro.Connection;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Connection;

/// <summary>
/// Unit tests for <see cref="AgentConfigMapper"/> — the Core-owned contract
/// glue that turns an <see cref="AgentConfig"/> into a Mikro-bound
/// <see cref="MikroConnectionSettings"/>.
/// </summary>
public class AgentConfigMapperTests
{
    private static AgentConfig ValidConfig(
        string server = "MIKROSQL\\MIKRO",
        string user = "sa",
        string password = "topsecret",
        string database = "MIKRO16",
        int companyNo = 1,
        int branchNo = 1,
        int warehouseNo = 1)
    {
        return new AgentConfig
        {
            SqlServer = server,
            SqlUserName = user,
            SqlPassword = password,
            MikroDatabaseName = database,
            CompanyNo = companyNo,
            BranchNo = branchNo,
            WarehouseNo = warehouseNo,
        };
    }

    [Fact]
    public void ToErpSettings_returns_a_populated_MikroConnectionSettings_for_a_valid_config()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig());

        settings.Should().NotBeNull();
        settings.Should().BeOfType<MikroConnectionSettings>();
        var mikro = (MikroConnectionSettings)settings!;
        mikro.Server.Should().Be("MIKROSQL\\MIKRO");
        mikro.UserId.Should().Be("sa");
        mikro.Password.Should().Be("topsecret");
        mikro.DatabaseName.Should().Be("MIKRO16");
    }

    [Fact]
    public void ToErpSettings_returns_null_when_SqlServer_is_blank()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(server: "  "));

        settings.Should().BeNull();
    }

    [Fact]
    public void ToErpSettings_returns_null_when_SqlUserName_is_blank()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(user: string.Empty));

        settings.Should().BeNull();
    }

    [Fact]
    public void ToErpSettings_returns_null_when_MikroDatabaseName_is_blank()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(database: "  \t "));

        settings.Should().BeNull();
    }

    [Fact]
    public void ToErpSettings_accepts_an_empty_password_for_trusted_auth_setups()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(password: string.Empty));

        settings.Should().NotBeNull();
        var mikro = (MikroConnectionSettings)settings!;
        mikro.Password.Should().Be(string.Empty);
    }

    [Fact]
    public void ToErpSettings_returns_null_for_a_null_config_argument()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        Action act = () => mapper.ToErpSettings(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromAgentConfig_typed_helper_returns_a_valid_settings_object()
    {
        var config = ValidConfig();

        var mikro = AgentConfigMapper.FromAgentConfig(config);

        mikro.Should().NotBeNull();
        mikro!.Server.Should().Be(config.SqlServer);
        mikro.UserId.Should().Be(config.SqlUserName);
        mikro.DatabaseName.Should().Be(config.MikroDatabaseName);
        mikro.Password.Should().Be(config.SqlPassword);
    }

    [Fact]
    public void FromAgentConfig_with_zero_companyNo_returns_null()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(companyNo: 0));

        settings.Should().BeNull();
    }

    [Fact]
    public void FromAgentConfig_with_negative_branchNo_returns_null()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(branchNo: -1));

        settings.Should().BeNull();
    }

    [Fact]
    public void FromAgentConfig_trims_whitespace_around_credential_fields()
    {
        var config = ValidConfig(
            server: "  srv  ",
            user: "  sa ",
            database: "  MIKRO16 ");

        var mikro = AgentConfigMapper.FromAgentConfig(config);

        mikro.Should().NotBeNull();
        mikro!.Server.Should().Be("srv");
        mikro.UserId.Should().Be("sa");
        mikro.DatabaseName.Should().Be("MIKRO16");
    }

    // ------------------------------------------------------------------------
    // Faz 10 — multi-firm Mikro support: CompanyNo + WarehouseNo propagation.
    // ------------------------------------------------------------------------

    [Fact]
    public void FromAgentConfig_propagates_CompanyNo_and_WarehouseNo()
    {
        var config = ValidConfig(companyNo: 3, warehouseNo: 5);

        var mikro = AgentConfigMapper.FromAgentConfig(config);

        mikro.Should().NotBeNull();
        mikro!.CompanyNo.Should().Be(3);
        mikro.WarehouseNo.Should().Be(5);
    }

    [Fact]
    public void ToErpSettings_propagates_CompanyNo_and_WarehouseNo()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(companyNo: 7, warehouseNo: 2));

        settings.Should().NotBeNull();
        var mikro = (MikroConnectionSettings)settings!;
        mikro.CompanyNo.Should().Be(7);
        mikro.WarehouseNo.Should().Be(2);
    }

    [Fact]
    public void FromAgentConfig_with_zero_warehouseNo_returns_null()
    {
        IAgentConfigToErpSettingsMapper mapper = new AgentConfigMapper();

        var settings = mapper.ToErpSettings(ValidConfig(warehouseNo: 0));

        settings.Should().BeNull();
    }

    [Fact]
    public void FromAgentConfig_defaults_warehouseNo_to_1_when_AgentConfig_is_fresh()
    {
        // A brand-new AgentConfig (no setter calls) must not break the mapper;
        // the AgentConfig field default of 1 is propagated.
        var config = new AgentConfig
        {
            SqlServer = "MIKROSQL\\MIKRO",
            SqlUserName = "sa",
            SqlPassword = "secret",
            MikroDatabaseName = "MIKRO16",
        };

        var mikro = AgentConfigMapper.FromAgentConfig(config);

        mikro.Should().NotBeNull();
        mikro!.CompanyNo.Should().Be(1);
        mikro.WarehouseNo.Should().Be(1);
    }
}
