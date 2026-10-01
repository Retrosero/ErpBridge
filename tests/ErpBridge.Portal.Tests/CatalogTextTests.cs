using ErpBridge.Portal.Api;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_MUSTERI_KATALOGU P4: the access sheet's share message, WhatsApp link, form rules and visibility mapping.</summary>
public sealed class CatalogTextTests
{
    [Theory]
    [InlineData("0532 123 45 67", "905321234567")]
    [InlineData("(0532) 123-45-67", "905321234567")]
    [InlineData("5321234567", "905321234567")]
    [InlineData("+90 532 123 45 67", "905321234567")]
    [InlineData("0090 532 123 45 67", "905321234567")]
    [InlineData("0232 444 55 66", "902324445566")]
    [InlineData("123", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void A_phone_becomes_the_number_whatsapp_wants(string? phone, string? expected) =>
        CatalogText.WhatsAppNumber(phone).Should().Be(expected);

    [Fact]
    public void The_whatsapp_link_carries_the_number_and_the_encoded_message()
    {
        CatalogText.WhatsAppLink("0532 123 45 67", "Merhaba Ali, şifre: a&b?").Should()
            .Be("https://wa.me/905321234567?text=Merhaba%20Ali%2C%20%C5%9Fifre%3A%20a%26b%3F");
        CatalogText.WhatsAppLink(null, "Merhaba").Should().Be("https://wa.me/?text=Merhaba", "without a number WhatsApp lets the user pick the chat");
    }

    [Fact]
    public void The_share_message_names_the_customer_company_link_and_username_and_the_password_only_when_given()
    {
        var message = CatalogText.ShareMessage("Bakkal Ali", "Ege Dağıtım", "https://sipariscepte.appsgo.cloud/ABCD2345", "bakkal.ali");

        message.Should().Be("Merhaba Bakkal Ali, Ege Dağıtım ürün kataloğumuza https://sipariscepte.appsgo.cloud/ABCD2345 adresinden girebilirsiniz. Kullanıcı adınız: bakkal.ali");
        CatalogText.ShareMessage("Bakkal Ali", "Ege Dağıtım", "https://k/ABCD2345", "bakkal.ali", "Xy7kP2mQ9a").Should().EndWith("Kullanıcı adınız: bakkal.ali, şifreniz: Xy7kP2mQ9a");
    }

    [Theory]
    [InlineData("", 0.0)]
    [InlineData("10", 10.0)]
    [InlineData("12,5", 12.5)]
    [InlineData("99.99", 99.99)]
    [InlineData("100", null)]
    [InlineData("-1", null)]
    [InlineData("10.555", null)]
    [InlineData("on", null)]
    public void A_discount_is_zero_to_99_99_with_two_decimals(string text, double? expected) =>
        CatalogText.ParseDiscount(text).Should().Be(expected is { } value ? (decimal)value : null);

    [Fact]
    public void The_net_price_rounds_half_away_from_zero_like_the_server()
    {
        CatalogText.Net(100m, 10m).Should().Be(90m);
        CatalogText.Net(0.25m, 50m).Should().Be(0.13m);
        CatalogText.Percent(12.5m).Should().Be("%12,5");
    }

    [Fact]
    public void A_typed_password_is_8_to_72_bytes_and_a_username_follows_the_server_rule()
    {
        CatalogText.IsValidPassword("1234567").Should().BeFalse();
        CatalogText.IsValidPassword("12345678").Should().BeTrue();
        CatalogText.IsValidPassword(new string('ş', 37)).Should().BeFalse("74 bytes in UTF-8");
        CatalogText.NormalizeUsername("  Bakkal.Ali ").Should().Be("bakkal.ali");
        CatalogText.IsValidUsername("bakkal.ali").Should().BeTrue();
        CatalogText.IsValidUsername("ab").Should().BeFalse();
        CatalogText.IsValidUsername("bakkal ali").Should().BeFalse();
    }

    private static CatalogAccountDto Account(string mode, params (string Type, string Key, string Effect)[] rules) => new()
    {
        Username = "bakkal.ali",
        DiscountPercent = 7.5m,
        Visibility = new CatalogVisibilityDto
        {
            Mode = mode,
            Rules = [.. rules.Select(r => new CatalogVisibilityRuleDto { Type = r.Type, Key = r.Key, Effect = r.Effect })],
        },
    };

    private static IEnumerable<string> Rules(CatalogVisibilityDto visibility) => visibility.Rules.Select(r => $"{r.Type}:{r.Key}:{r.Effect}");

    [Fact]
    public void The_three_screen_choices_map_onto_mode_and_rules()
    {
        var form = new CatalogAccessForm { Categories = ["Icecek"], Products = ["CAY-1"], Revealed = ["GIZLI-1"] };

        form.Visibility = CatalogAccessForm.Main;
        form.ToVisibility().Mode.Should().Be("all");
        Rules(form.ToVisibility()).Should().Equal("product:GIZLI-1:allow");

        form.Visibility = CatalogAccessForm.Except;
        form.ToVisibility().Mode.Should().Be("all");
        Rules(form.ToVisibility()).Should().Equal("category:Icecek:deny", "product:CAY-1:deny", "product:GIZLI-1:allow");

        form.Visibility = CatalogAccessForm.Only;
        form.ToVisibility().Mode.Should().Be("only");
        Rules(form.ToVisibility()).Should().Equal("category:Icecek:allow", "product:CAY-1:allow");
    }

    [Fact]
    public void A_stored_access_reads_back_into_the_same_choice_and_keeps_rules_the_screen_has_no_control_for()
    {
        var except = CatalogAccessForm.From(Account("all", ("category", "Icecek", "deny"), ("product", "CAY-1", "deny"),
            ("product", "GIZLI-1", "allow"), ("category", "Gizli kategori", "allow")));
        except.Visibility.Should().Be(CatalogAccessForm.Except);
        except.Categories.Should().Equal("Icecek");
        except.Products.Should().Equal("CAY-1");
        except.Revealed.Should().Equal("GIZLI-1");
        except.DiscountText.Should().Be("7.5");
        Rules(except.ToVisibility()).Should().Contain("category:Gizli kategori:allow", "a rule the phone wrote is not lost on save");

        var main = CatalogAccessForm.From(Account("all", ("product", "GIZLI-1", "allow")));
        main.Visibility.Should().Be(CatalogAccessForm.Main);
        main.Revealed.Should().Equal("GIZLI-1");

        var only = CatalogAccessForm.From(Account("only", ("category", "Icecek", "allow"), ("product", "CAY-9", "deny")));
        only.Visibility.Should().Be(CatalogAccessForm.Only);
        only.Categories.Should().Equal("Icecek");
        Rules(only.ToVisibility()).Should().Equal("category:Icecek:allow", "product:CAY-9:deny");
        only.Visibility = CatalogAccessForm.Main;
        Rules(only.ToVisibility()).Should().BeEmpty("an only-mode exception means nothing in the main catalog");
    }

    [Fact]
    public void The_form_names_its_first_problem()
    {
        new CatalogAccessForm { Username = "ab" }.Problem(creating: true).Should().Contain("3-64");
        new CatalogAccessForm { Username = "bakkal.ali", GeneratePassword = false, Password = "kisa" }.Problem(creating: true).Should().Be(CatalogAccessForm.PasswordRule);
        new CatalogAccessForm { Username = "bakkal.ali", GeneratePassword = false }.Problem(creating: false).Should().BeNull("an existing access keeps its password");
        new CatalogAccessForm { Username = "bakkal.ali", DiscountText = "100" }.Problem(creating: true).Should().Contain("99,99");
        new CatalogAccessForm { Username = "bakkal.ali", Visibility = CatalogAccessForm.Only }.Problem(creating: true).Should().Contain("en az bir");
    }
}
