using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Permissions;
using FluentAssertions;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;
using R = ErpBridge.CentralApi.Domain.MobileUserRoles;

namespace ErpBridge.CentralApi.Tests.Permissions;

/// <summary>
/// GOAL_YETKILER S5: what a phone document says about the sender's limits, and when it has to go to approval instead.
/// A value the document does not carry checks nothing, so an older phone's document is never refused for it.
/// </summary>
public sealed class DocumentPermissionCheckTests
{
    private static EffectivePermissions Sales(Dictionary<string, string> overrides) =>
        PermissionResolver.Resolve([R.Sales], [], overrides);

    [Fact]
    public void The_facts_are_read_from_the_amount_and_the_lines()
    {
        var facts = DocumentLimitFacts.Read("""
            {"grossAmount": 1200.5, "amount": 900, "lines": [
              {"lineDiscountPercent": 5, "generalDiscountPercent": 2},
              {"lineDiscountPercent": 12.5},
              {"note": "no discount"}]}
            """);

        facts.Should().Be(new DocumentLimitFacts(1200.5m, 12.5m, 2m, OnAccount: true));
        DocumentLimitFacts.Read("""{"total": 40, "paymentType": "Nakit"}""").Should().Be(new DocumentLimitFacts(40m, null, null));
        DocumentLimitFacts.Read("not json").Should().Be(DocumentLimitFacts.None);
        DocumentLimitFacts.Read(null).Should().Be(DocumentLimitFacts.None);
    }

    [Fact]
    public void A_sale_over_a_limit_names_the_first_limit_gone_over()
    {
        var limited = Sales(new()
        {
            [K.LimitSaleLineDiscountPct] = "10",
            [K.LimitSaleAmount] = "1000",
        });

        DocumentPermissionCheck.Refusal(limited, ApprovalKinds.Sale, new DocumentLimitFacts(1000m, 10m, 50m)).Should().BeNull();
        DocumentPermissionCheck.Refusal(limited, ApprovalKinds.Sale, new DocumentLimitFacts(5000m, 15m, null))
            .Should().Contain("Satır iskontosu").And.Contain("%10");
        DocumentPermissionCheck.Refusal(limited, ApprovalKinds.Sale, new DocumentLimitFacts(1000.01m, null, null))
            .Should().Contain("Satış tutarı").And.Contain("onaya");
        DocumentPermissionCheck.Refusal(limited, ApprovalKinds.Sale, DocumentLimitFacts.None).Should().BeNull();
    }

    [Fact]
    public void A_sale_on_account_needs_the_open_account_right()
    {
        DocumentLimitFacts.Read("""{"paymentType": "Cari Borç", "amount": 10}""").OnAccount.Should().BeTrue();
        DocumentLimitFacts.Read("""{"amount": 10}""").OnAccount.Should().BeTrue("an empty payment type books the sale on account");
        DocumentLimitFacts.Read("""{"paymentType": "Nakit", "amount": 10}""").OnAccount.Should().BeFalse();
        DocumentLimitFacts.Read("""{"paymentType": "Cari Borç", "payments": [{"type": "cash", "amount": 10}]}""").OnAccount.Should().BeFalse();
        DocumentLimitFacts.None.OnAccount.Should().BeFalse();

        var noAccount = Sales(new() { [K.SaleOpenAccount] = PermissionValues.False });
        DocumentPermissionCheck.Refusal(noAccount, ApprovalKinds.Sale, new DocumentLimitFacts(10m, null, null, OnAccount: true))
            .Should().Contain("Açık hesap");
        DocumentPermissionCheck.Refusal(noAccount, ApprovalKinds.Sale, new DocumentLimitFacts(10m, null, null)).Should().BeNull();
        DocumentPermissionCheck.Refusal(Sales(new()), ApprovalKinds.Sale, new DocumentLimitFacts(10m, null, null, OnAccount: true)).Should().BeNull();
    }

    [Fact]
    public void Each_document_kind_uses_its_own_module_and_limit()
    {
        var p = Sales(new() { [K.ModuleReturns] = PermissionValues.False, [K.LimitPurchaseAmount] = "500" });

        DocumentPermissionCheck.Refusal(p, ApprovalKinds.Return, DocumentLimitFacts.None).Should().Contain("yetkiniz yok");
        DocumentPermissionCheck.Refusal(p, ApprovalKinds.Purchase, new DocumentLimitFacts(600m, null, null)).Should().Contain("Alış tutarı");
        DocumentPermissionCheck.Refusal(p, ApprovalKinds.Disbursement, new DocumentLimitFacts(1e9m, null, null)).Should().BeNull();
        DocumentPermissionCheck.Refusal(p, ApprovalKinds.Collection, new DocumentLimitFacts(1e9m, null, null)).Should().BeNull();
    }

    [Fact]
    public void Administrators_and_default_roles_send_everything_as_before()
    {
        var huge = new DocumentLimitFacts(1e12m, 99m, 99m);
        var admin = PermissionResolver.Resolve([R.Admin], [], new Dictionary<string, string>());
        foreach (var kind in new[] { ApprovalKinds.Sale, ApprovalKinds.Return, ApprovalKinds.Purchase, ApprovalKinds.Disbursement, ApprovalKinds.Collection })
        {
            DocumentPermissionCheck.Refusal(admin, kind, huge).Should().BeNull();
            DocumentPermissionCheck.Refusal(Sales(new()), kind, huge).Should().BeNull();
        }
    }
}
