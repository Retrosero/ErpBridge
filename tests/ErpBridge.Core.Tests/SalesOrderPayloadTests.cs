using ErpBridge.Erp.Abstractions.SalesOrder;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Unit tests for the <see cref="SalesOrderPayload"/> record and
/// <see cref="SalesOrderLinePayload"/>. These guard the contract that the central
/// API and the Mikro adapter agree on.
/// </summary>
public class SalesOrderPayloadTests
{
    [Fact]
    public void SalesOrderPayload_carries_all_fields_via_positional_record()
    {
        var line = new SalesOrderLinePayload(
            StockCode: "STK001",
            Quantity: 2m,
            UnitPointer: 1,
            UnitPrice: 100m,
            TaxPointer: 4,
            Discounts: new decimal[] { 0, 0, 0, 0, 0, 0 });

        var payload = new SalesOrderPayload(
            TenantId: "tenant-1",
            ExternalId: "ext-uuid-1",
            CustomerCode: "120.01.0001",
            SalespersonCode: "PL01",
            WarehouseNo: 1,
            DocumentSeries: "S",
            DocumentNumber: 123,
            OccurredAt: new DateTime(2026, 7, 9, 10, 30, 0),
            Currency: "TRY",
            Lines: new[] { line });

        payload.TenantId.Should().Be("tenant-1");
        payload.ExternalId.Should().Be("ext-uuid-1");
        payload.CustomerCode.Should().Be("120.01.0001");
        payload.SalespersonCode.Should().Be("PL01");
        payload.WarehouseNo.Should().Be(1);
        payload.DocumentSeries.Should().Be("S");
        payload.DocumentNumber.Should().Be(123);
        payload.OccurredAt.Should().Be(new DateTime(2026, 7, 9, 10, 30, 0));
        payload.Currency.Should().Be("TRY");
        payload.Lines.Should().HaveCount(1);
    }

    [Fact]
    public void SalesOrderLinePayload_carries_six_discount_slots()
    {
        var line = new SalesOrderLinePayload(
            "STK002", 1m, 1, 50m, 1,
            new decimal[] { 0.1m, 0.05m, 0, 0, 0.02m, 0 });

        line.Discounts.Should().HaveCount(6);
        line.Discounts[0].Should().Be(0.1m);
        line.Discounts[5].Should().Be(0);
    }

    [Fact]
    public void ErpWriteResult_defaults_failure_when_called_with_ok_true()
    {
        var result = new ErpWriteResult(Ok: true);

        result.Ok.Should().BeTrue();
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ErpWriteResult_carries_recno_and_guid_for_v15_or_v16()
    {
        var v15 = new ErpWriteResult(Ok: true, ErpRecno: 4242, DocumentSeries: "S", DocumentNumber: 7);
        var v16 = new ErpWriteResult(Ok: true, ErpGuid: Guid.NewGuid(), DocumentSeries: "S", DocumentNumber: 8);

        v15.ErpRecno.Should().Be(4242);
        v15.ErpGuid.Should().BeNull();

        v16.ErpGuid.Should().NotBeNull();
        v16.ErpRecno.Should().BeNull();
    }

    [Fact]
    public void ErpWriteResult_carries_error_when_failed()
    {
        var result = new ErpWriteResult(
            Ok: false,
            ErrorCode: ErpWriteResult.ErrorCodeMissingLookup,
            ErrorMessage: "Cari 120.01.9999 bulunamadı");

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be("MissingLookup");
        result.ErrorMessage.Should().Contain("120.01.9999");
    }
}
