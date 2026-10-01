using ErpBridge.CentralApi.CustomerCatalog;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.CustomerCatalog;

/// <summary>
/// The catalog's request total must be the total the phone books when staff turn the request into a sale. The sale
/// examples are Sipariş Cepte's <c>ErpSalePricingTest</c> one for one (which pins the server's
/// <c>MikroPriceCalculatorTests</c> values); the return example is left out, the catalog has no returns.
/// </summary>
public sealed class CatalogPricingTests
{
    [Fact]
    public void Discounts_chain_on_what_the_previous_one_left_and_vat_is_on_the_net()
    {
        var line = CatalogPricing.Price(new CatalogPricing.Line(400m, 2m, 10m, 0m, 5m, VatPercent: 20m));

        line.Gross.Should().Be(800m);
        line.LineDiscount.Should().Be(80m);
        line.GeneralDiscount.Should().Be(36m);
        line.Net.Should().Be(684m);
        line.Vat.Should().Be(136.8m);
        line.Total.Should().Be(820.8m);
    }

    [Fact]
    public void Amounts_round_to_two_decimals_half_away_from_zero()
    {
        var line = CatalogPricing.Price(new CatalogPricing.Line(3.335m, 3m, 10m, VatPercent: 20m));

        line.Gross.Should().Be(10.01m);
        line.LineDiscount.Should().Be(1.0m);
        line.Vat.Should().Be(1.8m);
    }

    [Fact]
    public void Vat_is_charged_after_the_basket_discount_unlike_the_old_phone_total()
    {
        var totals = CatalogPricing.Total([new CatalogPricing.Line(1000m, 1m, GeneralDiscountPercent: 10m, VatPercent: 20m)]);

        totals.Total.Should().Be(1080m);
        totals.Discount.Should().Be(100m);
        totals.Vat.Should().Be(180m);
    }

    [Fact]
    public void The_document_total_is_the_sum_of_rounded_lines()
    {
        var totals = CatalogPricing.Total(
        [
            new CatalogPricing.Line(400m, 2m, 10m, 0m, 5m, 20m),
            new CatalogPricing.Line(50m, 1m, VatPercent: 10m),
            new CatalogPricing.Line(10m, 1m),
        ]);

        totals.Gross.Should().Be(860m);
        totals.Vat.Should().Be(141.8m);
        totals.Total.Should().Be(885.8m);
    }

    [Fact]
    public void A_zero_rate_company_pays_no_vat()
    {
        var totals = CatalogPricing.Total([new CatalogPricing.Line(125m, 3m, 10m, 5m, 2m, VatPercent: 0m)]);

        totals.Vat.Should().Be(0m);
        totals.Total.Should().Be(totals.Gross - totals.Discount);
    }

    [Fact]
    public void A_vat_inclusive_list_price_books_the_same_as_its_net_price()
    {
        var included = CatalogPricing.Price(new CatalogPricing.Line(480m, 2m, 10m, 0m, 5m, 20m, ListIncludesVat: true));
        var excluded = CatalogPricing.Price(new CatalogPricing.Line(400m, 2m, 10m, 0m, 5m, 20m));

        included.Should().Be(excluded);
        CatalogPricing.Price(new CatalogPricing.Line(100m, 1m, ListIncludesVat: true)).Gross.Should().Be(100m);
    }

    [Fact]
    public void A_catalog_line_carries_the_customer_discount_alone()
    {
        // The phone sends the request's discount as the line's customer discount (Mikro's second discount column).
        var line = CatalogPricing.CatalogLine(listPrice: 120m, quantity: 3m, discountPercent: 12.5m, vatPercent: 20m, listIncludesVat: true);

        line.Gross.Should().Be(300m, "120 incl. 20 % VAT is 100 net, three of them");
        line.LineDiscount.Should().Be(0m);
        line.CustomerDiscount.Should().Be(37.5m);
        line.GeneralDiscount.Should().Be(0m);
        line.Vat.Should().Be(52.5m);
        line.Total.Should().Be(315m);
        line.Should().Be(CatalogPricing.Price(new CatalogPricing.Line(120m, 3m, CustomerDiscountPercent: 12.5m, VatPercent: 20m, ListIncludesVat: true)));
    }

    [Theory]
    [InlineData(100, 10, 90)]
    [InlineData(19.99, 7.5, 18.49)] // 18.49075
    [InlineData(10.05, 50, 5.03)]   // 5.025, half away from zero
    [InlineData(37.5, 0, 37.5)]
    public void The_shown_price_is_the_list_price_less_the_discount_rounded(double list, double discount, double net) =>
        CatalogPricing.NetUnitPrice((decimal)list, (decimal)discount).Should().Be((decimal)net);

    [Fact]
    public void A_no_discount_product_sells_at_the_list_price()
    {
        CatalogPricing.DiscountFor(noDiscount: true, accountDiscountPercent: 15m).Should().Be(0m);
        CatalogPricing.DiscountFor(noDiscount: false, accountDiscountPercent: 15m).Should().Be(15m);
        CatalogPricing.NetUnitPrice(80m, CatalogPricing.DiscountFor(true, 15m)).Should().Be(80m);
    }
}
