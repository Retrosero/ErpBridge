using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Mobile;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.CustomerCatalog;

/// <summary>GOAL_MUSTERI_KATALOGU T3 / S4: catalog usernames, passwords and discounts.</summary>
public sealed class CatalogAccountsTests
{
    [Theory]
    [InlineData("Yılmaz Market Ltd. Şti.", "yilmaz-market")]
    [InlineData("ÇAĞLAR GIDA SAN. VE TİC. A.Ş.", "caglar-gida")]
    [InlineData("Öztürk & Oğulları İnşaat Malzemeleri", "ozturk-ogullari-insaat")]
    [InlineData("Ünlüoğlu", "unluoglu")]
    [InlineData("Kırtasiyecilik ve Bilgisayar Hizmetleri Uzun", "kirtasiyecilik")]
    [InlineData("120.01.001", "120-01-001")]
    [InlineData("A.Ş.", null)]
    [InlineData("  ", null)]
    [InlineData(null, null)]
    public void A_suggestion_folds_Turkish_letters_and_keeps_a_few_words(string? name, string? expected)
    {
        var suggestion = CatalogAccounts.UsernameFrom(name);

        suggestion.Should().Be(expected);
        if (suggestion is not null) MobileSeatService.NormalizeUsername(suggestion).Should().Be(suggestion, "a suggestion is a valid username as it is");
    }

    [Fact]
    public void A_name_in_use_gets_the_first_free_number()
    {
        CatalogAccounts.FirstFree("bayi", new HashSet<string>()).Should().Be("bayi");
        CatalogAccounts.FirstFree("bayi", new HashSet<string> { "bayi", "bayi2" }).Should().Be("bayi3");
        var longName = new string('a', 64);
        CatalogAccounts.FirstFree(longName, new HashSet<string> { longName }).Should().Be(new string('a', 63) + "2");
    }

    [Fact]
    public void A_made_password_is_ten_letters_and_digits_nobody_misreads()
    {
        var passwords = Enumerable.Range(0, 200).Select(_ => CatalogAccounts.GeneratePassword()).ToList();

        passwords.Should().OnlyContain(p => p.Length == 10 && p.All(ch => "abcdefghjkmnpqrstuvwxyz23456789".Contains(ch)));
        passwords.Distinct().Should().HaveCount(200);
        CatalogAccounts.PasswordError(passwords[0]).Should().BeNull();
    }

    [Theory]
    [InlineData("1234567", false)]
    [InlineData("12345678", true)]
    [InlineData("çğşı", true)] // four letters, eight bytes
    public void A_typed_password_is_eight_to_seventy_two_bytes(string password, bool accepted) =>
        (CatalogAccounts.PasswordError(password) is null).Should().Be(accepted);

    [Fact]
    public void A_password_longer_than_bcrypt_reads_is_refused()
    {
        CatalogAccounts.PasswordError(new string('x', 72)).Should().BeNull();
        CatalogAccounts.PasswordError(new string('x', 73)).Should().NotBeNull();
        CatalogAccounts.PasswordError(new string('ş', 37)).Should().NotBeNull("74 bytes");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(12.5, 12.5)]
    [InlineData(99.99, 99.99)]
    [InlineData(10.004, 10)]
    public void A_discount_is_kept_to_two_decimals(double value, double stored) =>
        CatalogAccounts.Discount((decimal)value).Should().Be((decimal)stored);

    [Theory]
    [InlineData(-0.01)]
    [InlineData(99.995)]
    [InlineData(100)]
    public void A_discount_outside_the_range_is_refused(double value) =>
        CatalogAccounts.Discount((decimal)value).Should().BeNull();
}
