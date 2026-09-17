using System.Reflection;
using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Shared.Tests;

public class ErpWriteErrorTests
{
    /// <summary>Every factory with sample arguments, so the catalog cannot grow an untested entry.</summary>
    private static IEnumerable<ErpWriteError> All()
    {
        var samples = new Dictionary<Type, object>
        {
            [typeof(string)] = "120.001",
            [typeof(int)] = 3,
            [typeof(decimal)] = -0.12m,
        };
        return typeof(ErpWriteError).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.ReturnType == typeof(ErpWriteError))
            .Select(m => (ErpWriteError)m.Invoke(null, m.GetParameters().Select(p => samples[p.ParameterType]).ToArray())!);
    }

    [Fact]
    public void Every_error_has_a_stable_unique_code_and_a_turkish_message()
    {
        var errors = All().ToList();

        errors.Should().HaveCountGreaterThan(20);
        errors.Select(e => e.Code).Should().OnlyHaveUniqueItems();
        errors.Should().OnlyContain(e => e.Code.Length > 0 && e.Code == e.Code.ToUpperInvariant() && e.Message.EndsWith('.'));
        var constants = typeof(ErpWriteError).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.Name.EndsWith("Code", StringComparison.Ordinal))
            .Select(f => (string)f.GetRawConstantValue()!);
        errors.Select(e => e.Code).Should().BeEquivalentTo(constants, "each code constant has exactly one factory");
    }

    [Fact]
    public void Only_problems_that_fix_themselves_are_retryable()
    {
        All().Where(e => e.Retryable).Select(e => e.Code)
            .Should().BeEquivalentTo(ErpWriteError.ErpUnavailableCode, ErpWriteError.ErpContextMissingCode);
    }

    [Fact]
    public void Messages_name_the_codes_they_are_about()
    {
        ErpWriteError.CustomerNotFound("120.001").Message.Should().Be("Müşteri ERP'de bulunamadı: 120.001.");
        ErpWriteError.TotalMismatch(-0.12m).Message.Should().Contain("fark 0,12 TL");
        ErpWriteError.ErpMappingMissing("kasa kodu").Message.Should().Contain("kasa kodu").And.Contain("Portal");
    }

    [Fact]
    public void Codes_from_a_malformed_body_are_cut_to_an_erp_code_width_without_control_characters()
    {
        var message = ErpWriteError.StockNotFound("B575\r\nAuthorization: Bearer eyJhbGciOiJIUzI1NiJ9.secret").Message;

        message.Should().NotContain("\n").And.NotContain("secret");
        message.Should().StartWith("Ürün ERP'de bulunamadı: B575Authorization: Bearer…");
    }
}
