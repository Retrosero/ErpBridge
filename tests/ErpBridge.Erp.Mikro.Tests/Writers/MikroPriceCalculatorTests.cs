using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>Mikro's line arithmetic for phone documents (goal ERP yazım Y3b, D8/D9/D11).</summary>
public class MikroPriceCalculatorTests
{
    [Fact]
    public void Discounts_chain_on_what_the_previous_one_left_and_vat_is_on_the_net()
    {
        // 2 × 400, line 10 %, customer 0 %, basket 5 %, VAT 20 %.
        var line = MikroPriceCalculator.SaleLine(400m, 2m, 10m, 0m, 5m, vatPointer: 4, vatRate: 20m, listIncludesVat: false);

        line.Should().Be(new MikroPricedLine(400m, 2m, 800m, 80m, 0m, 36m, 4, 20m, 136.80m));
        line.Net.Should().Be(684m);
        line.Total.Should().Be(820.80m);
    }

    [Fact]
    public void A_price_from_a_vat_included_list_is_stored_without_vat()
    {
        var included = MikroPriceCalculator.SaleLine(480m, 2m, 10m, 0m, 5m, 4, 20m, listIncludesVat: true);
        var excluded = MikroPriceCalculator.SaleLine(400m, 2m, 10m, 0m, 5m, 4, 20m, listIncludesVat: false);

        included.Should().Be(excluded);
        MikroPriceCalculator.SaleLine(100m, 1m, 0m, 0m, 0m, 1, 0m, listIncludesVat: true).Gross.Should().Be(100m, "a 0 % list price is the same with or without VAT");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1.00)]
    [InlineData(10, 10.00)]
    [InlineData(20, 20.00)]
    public void Each_vat_rate_applies_to_the_net(decimal rate, decimal vat)
    {
        MikroPriceCalculator.SaleLine(100m, 1m, 0m, 0m, 0m, 2, rate, false).Vat.Should().Be(vat);
    }

    [Fact]
    public void Amounts_round_to_two_decimals_away_from_zero()
    {
        // 3 × 3.335 = 10.005 → 10.01; 10 % of 10.01 = 1.001 → 1.00; VAT 20 % of 9.01 = 1.802 → 1.80.
        var line = MikroPriceCalculator.SaleLine(3.335m, 3m, 10m, 0m, 0m, 4, 20m, false);

        line.Gross.Should().Be(10.01m);
        line.Discount1.Should().Be(1.00m);
        line.Vat.Should().Be(1.80m);
        MikroPriceCalculator.Round(0.125m).Should().Be(0.13m);
        MikroPriceCalculator.Round(-0.125m).Should().Be(-0.13m);
    }

    [Fact]
    public void A_damaged_return_keeps_the_condition_difference_as_the_first_discount()
    {
        var line = MikroPriceCalculator.ReturnLine(400m, 1m, conditionRatio: 0.3m, vatPointer: 4, vatRate: 20m, listIncludesVat: false);

        line.Should().Be(new MikroPricedLine(400m, 1m, 400m, 280m, 0m, 0m, 4, 20m, 24m));
        line.Total.Should().Be(144m);
        MikroPriceCalculator.ReturnLine(400m, 1m, 1m, 1, 0m, false).Discount1.Should().Be(0m, "a sound item is refunded in full");
    }

    [Fact]
    public void Header_figures_are_line_sums_and_vat_is_filed_per_pointer()
    {
        var document = new MikroPricedDocument(
        [
            MikroPriceCalculator.SaleLine(400m, 2m, 10m, 0m, 5m, 4, 20m, false),
            MikroPriceCalculator.SaleLine(50m, 1m, 0m, 0m, 0m, 3, 10m, false),
            MikroPriceCalculator.SaleLine(10m, 1m, 0m, 0m, 0m, 1, 0m, false),
        ]);

        document.Gross.Should().Be(860m);
        document.Discount1.Should().Be(80m);
        document.Discount3.Should().Be(36m);
        document.Vat.Should().Be(141.80m);
        document.Total.Should().Be(885.80m);
        document.VatBucket(4).Should().Be(136.80m);
        document.VatBucket(3).Should().Be(5m);
        document.VatBucket(1).Should().Be(0m);
    }

    [Theory]
    [InlineData(820.80, false)]
    [InlineData(820.85, false)]
    [InlineData(820.75, false)]
    [InlineData(820.86, true)]
    [InlineData(684.00, true)]
    public void The_phone_total_must_match_within_five_kurus(decimal phoneTotal, bool refused)
    {
        var document = new MikroPricedDocument([MikroPriceCalculator.SaleLine(400m, 2m, 10m, 0m, 5m, 4, 20m, false)]);

        var act = () => MikroPriceCalculator.EnsureTotal(document, phoneTotal);

        if (refused) act.Should().Throw<MikroWriteException>().Which.Error.Code.Should().Be(ErpWriteError.TotalMismatchCode);
        else act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0, MikroCustomerUse.Sale, true)]
    [InlineData(0, MikroCustomerUse.SaleReturn, true)]
    [InlineData(1, MikroCustomerUse.Sale, true)]
    [InlineData(1, MikroCustomerUse.SaleReturn, false)]
    [InlineData(2, MikroCustomerUse.Sale, false)]
    [InlineData(2, MikroCustomerUse.Collection, true)]
    [InlineData(3, MikroCustomerUse.Sale, false)]
    [InlineData(3, MikroCustomerUse.Collection, true)]
    [InlineData(4, MikroCustomerUse.Collection, false)]
    public void A_customer_card_allows_only_its_movement_type(byte movementType, MikroCustomerUse use, bool allowed)
    {
        MikroDocumentLookup.CustomerAllows(movementType, use, isOrder: false, orderLocked: false).Should().Be(allowed);
    }

    [Fact]
    public void A_card_locked_for_orders_still_takes_invoices()
    {
        MikroDocumentLookup.CustomerAllows(0, MikroCustomerUse.Sale, isOrder: true, orderLocked: true).Should().BeFalse();
        MikroDocumentLookup.CustomerAllows(0, MikroCustomerUse.Sale, isOrder: false, orderLocked: true).Should().BeTrue();
    }
}
