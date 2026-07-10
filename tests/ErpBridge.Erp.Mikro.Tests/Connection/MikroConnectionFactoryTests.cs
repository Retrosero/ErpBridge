using ErpBridge.Erp.Mikro.Connection;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Connection;

public class MikroConnectionFactoryTests
{
    [Fact]
    public void BuildConnectionString_includes_canonical_properties()
    {
        var factory = new MikroConnectionFactory();
        var settings = new MikroConnectionSettings(
            Server: "MIKROSQL\\MIKRO",
            UserId: "sa",
            Password: "secret",
            DatabaseName: "MIKRO16");

        var connectionString = factory.BuildConnectionString(settings);

        connectionString.Should().Contain("Data Source=MIKROSQL\\MIKRO");
        connectionString.Should().Contain("User ID=sa");
        connectionString.Should().Contain("Password=secret");
        connectionString.Should().Contain("Initial Catalog=MIKRO16");
        connectionString.Should().Contain("MultipleActiveResultSets=True");
    }

    [Fact]
    public void BuildConnectionString_rejects_missing_server()
    {
        var factory = new MikroConnectionFactory();
        var settings = new MikroConnectionSettings(" ", "sa", "x", "MIKRO");

        Action act = () => factory.BuildConnectionString(settings);

        act.Should().Throw<ArgumentException>().WithMessage("*Server*");
    }

    [Fact]
    public void BuildConnectionString_rejects_missing_user()
    {
        var factory = new MikroConnectionFactory();
        var settings = new MikroConnectionSettings("srv", " ", "x", "MIKRO");

        Action act = () => factory.BuildConnectionString(settings);

        act.Should().Throw<ArgumentException>().WithMessage("*UserId*");
    }

    [Fact]
    public void BuildConnectionString_rejects_missing_database()
    {
        var factory = new MikroConnectionFactory();
        var settings = new MikroConnectionSettings("srv", "sa", "x", " ");

        Action act = () => factory.BuildConnectionString(settings);

        act.Should().Throw<ArgumentException>().WithMessage("*DatabaseName*");
    }

    [Fact]
    public void BuildConnectionString_emits_Integrated_Security_when_IntegratedSecurity_is_true()
    {
        var factory = new MikroConnectionFactory();
        var settings = new MikroConnectionSettings(
            Server: "MIKROSQL\\MIKRO",
            UserId: "",
            Password: "",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: true);

        var connectionString = factory.BuildConnectionString(settings);

        connectionString.Should().Contain("Data Source=MIKROSQL\\MIKRO");
        connectionString.Should().Contain("Initial Catalog=MIKRO16");
        connectionString.Should().Contain("Integrated Security=true");
        connectionString.Should().NotContain("User ID=");
        connectionString.Should().NotContain("Password=");
    }

    [Fact]
    public void BuildConnectionString_rejects_missing_user_when_IntegratedSecurity_is_false()
    {
        var factory = new MikroConnectionFactory();
        var settings = new MikroConnectionSettings("srv", " ", "x", "MIKRO", IntegratedSecurity: false);

        Action act = () => factory.BuildConnectionString(settings);

        act.Should().Throw<ArgumentException>().WithMessage("*UserId*");
    }

    [Fact]
    public void BuildConnectionString_accepts_blank_user_when_IntegratedSecurity_is_true()
    {
        var factory = new MikroConnectionFactory();
        var settings = new MikroConnectionSettings("srv", " ", "x", "MIKRO", IntegratedSecurity: true);

        var connectionString = factory.BuildConnectionString(settings);

        connectionString.Should().Contain("Integrated Security=true");
    }
}
