using System.Text.Json;
using ErpBridge.Portal.Api;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>The documents of an approval request as a person reads them.</summary>
public sealed class ApprovalDocumentsTests
{
    private static IReadOnlyList<DocumentView> Read(string json)
    {
        using var document = JsonDocument.Parse(json);
        return ApprovalDocuments.Read(document.RootElement.Clone());
    }

    [Fact]
    public void A_sale_shows_its_header_lines_and_computes_a_missing_line_total()
    {
        var views = Read("""
            [{"documentType":"sales_order","externalId":"S1","payload":{
              "mobileDocumentId":"S1","occurredAt":"2026-09-16T14:05:00","dueDate":"2026-10-01","customerCode":"C-001","counterparty":"Bakkal Ali","customerName":"Bakkal Ali",
              "paymentType":"Nakit","amount":300,"note2":"x",
              "lines":[{"productCode":"CAY-1","productTitle":"Çay","quantity":2,"unitPrice":150},
                       {"barcode":"869","name":"Kahve","quantity":"1.5","unitPrice":10,"lineTotal":14}]}}]
            """);

        var sale = views.Should().ContainSingle().Subject;
        sale.Title.Should().Be("Satış");
        sale.Fields.Select(f => f.Label).Should().Equal("Belge no", "Tarih", "Cari kodu", "Cari", "Ödeme şekli", "Vade", "Tutar");
        sale.Fields.Single(f => f.Label == "Tarih").Value.Should().Be("16.09.2026 14:05");
        sale.Fields.Single(f => f.Label == "Vade").Value.Should().Be("01.10.2026");
        sale.Fields.Single(f => f.Label == "Tutar").Value.Should().Be("300,00 TL");
        sale.Lines.Should().HaveCount(2);
        sale.Lines[0].Total.Should().Be(300m);
        sale.Lines[1].Should().Match<DocumentLine>(l => l.Code == "869" && l.Name == "Kahve" && l.Quantity == 1.5m && l.Total == 14m);
        sale.Other.Should().ContainSingle(f => f.Label == "note2" && f.Value == "x");
        sale.IsCount.Should().BeFalse();
        sale.HasDiscount.Should().BeFalse();
        sale.HasVat.Should().BeFalse();
    }

    [Fact]
    public void Discount_vat_and_a_returned_items_condition_are_kept_when_the_line_carries_them()
    {
        var views = Read("""
            [{"documentType":"sales_return","payload":{"lines":[
              {"productCode":"A","quantity":1,"unitPrice":100,"discountPercent":10,"discountAmount":10,"vatRate":20,"conditionPercent":0.5,"lineTotal":45},
              {"productCode":"B","quantity":1,"unitPrice":50,"kdvOrani":"8"}]}}]
            """);

        var document = views.Single();
        document.HasDiscount.Should().BeTrue();
        document.HasVat.Should().BeTrue();
        document.HasCondition.Should().BeTrue();
        document.Lines[0].Should().Match<DocumentLine>(l => l.DiscountPercent == 10m && l.DiscountAmount == 10m && l.VatRate == 20m && l.ConditionPercent == 0.5m);
        document.Lines[1].VatRate.Should().Be(8m);
    }

    [Fact]
    public void A_count_uses_expected_and_counted_quantities_and_an_unknown_type_keeps_its_name()
    {
        var views = Read("""
            [{"documentType":"stock_count","payload":{"lines":[{"stockCode":"A","expectedQuantity":5,"countedQuantity":4}]}},
             {"documentType":"gift_card","payload":{"code":"G1"}},
             {"documentType":"collection"}]
            """);

        views.Should().HaveCount(3);
        views[0].IsCount.Should().BeTrue();
        views[0].Lines[0].Should().Match<DocumentLine>(l => l.Expected == 5m && l.Counted == 4m);
        views[1].Title.Should().Be("gift_card");
        views[1].Other.Should().ContainSingle(f => f.Label == "code");
        views[2].Title.Should().Be("Tahsilat");
        views[2].Fields.Should().BeEmpty();
    }
}
