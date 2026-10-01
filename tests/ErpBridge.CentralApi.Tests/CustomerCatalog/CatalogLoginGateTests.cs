using ErpBridge.CentralApi.CustomerCatalog;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.CustomerCatalog;

/// <summary>GOAL_MUSTERI_KATALOGU §6 / S6: the per-company sign-in budget and the BCrypt cap.</summary>
public sealed class CatalogLoginGateTests
{
    [Fact]
    public void A_company_has_its_own_sign_in_budget_counted_whatever_the_case_of_its_code()
    {
        using var gate = new CatalogLoginGate(companyPermitsPerMinute: 3);
        gate.Enter("ABCD2345").Should().BeNull();
        gate.Enter("abcd2345").Should().BeNull();
        gate.Enter("AbCd2345").Should().BeNull();
        gate.Enter("ABCD2345").Should().NotBeNull().And.Subject!.Value.Should().BePositive();
        gate.Enter("EFGH6789").Should().BeNull("another company is not affected");
    }

    [Fact]
    public async Task Verification_answers_as_bcrypt_does()
    {
        using var gate = new CatalogLoginGate();
        var hash = BCrypt.Net.BCrypt.HashPassword("dogru-sifre", workFactor: 4);
        (await gate.VerifyAsync("dogru-sifre", hash, CancellationToken.None)).Should().BeTrue();
        (await gate.VerifyAsync("yanlis", hash, CancellationToken.None)).Should().BeFalse();
        CatalogLoginGate.MaxConcurrentHashes.Should().Be(8);
        CatalogLoginGate.CompanyPermitsPerMinute.Should().Be(300);
    }
}
