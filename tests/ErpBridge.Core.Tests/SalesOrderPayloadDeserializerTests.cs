using System.Text.Json;
using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Abstractions.SalesOrder;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Unit tests for <see cref="SalesOrderPayloadDeserializer"/>. These guard the
/// wire-shape contract between the central API and the agent worker: malformed
/// JSON must surface as a structured <see cref="Result{T}.Fail"/> (with the
/// stable error codes the worker propagates into <c>JobAck</c>) rather than
/// throwing an exception that would crash the poll loop.
/// </summary>
public class SalesOrderPayloadDeserializerTests
{
    private readonly SalesOrderPayloadDeserializer _sut = new();

    private const string ValidPayloadJson = """
        {
          "TenantId": "tenant-1",
          "ExternalId": "ext-uuid-001",
          "CustomerCode": "120.01.0001",
          "SalespersonCode": "PL01",
          "WarehouseNo": 1,
          "DocumentSeries": "S",
          "DocumentNumber": 1234,
          "OccurredAt": "2026-07-09T10:30:00Z",
          "Currency": "TRY",
          "Lines": [
            {
              "StockCode": "STK001",
              "Quantity": 2.5,
              "UnitPointer": 1,
              "UnitPrice": 100.0,
              "TaxPointer": 4,
              "Discounts": [0, 0, 0, 0, 0, 0]
            }
          ]
        }
        """;

    [Fact]
    public void Deserialize_returns_Ok_with_payload_for_valid_json()
    {
        var result = _sut.Deserialize(ValidPayloadJson);

        result.IsSuccess.Should().BeTrue();
        result.ErrorCode.Should().BeNull();
        result.Error.Should().BeNull();
        result.Value.Should().NotBeNull();

        var payload = result.Value!;
        payload.TenantId.Should().Be("tenant-1");
        payload.ExternalId.Should().Be("ext-uuid-001");
        payload.CustomerCode.Should().Be("120.01.0001");
        payload.SalespersonCode.Should().Be("PL01");
        payload.WarehouseNo.Should().Be(1);
        payload.DocumentSeries.Should().Be("S");
        payload.DocumentNumber.Should().Be(1234);
        payload.OccurredAt.Should().Be(new DateTime(2026, 7, 9, 10, 30, 0, DateTimeKind.Utc));
        payload.Currency.Should().Be("TRY");
        payload.Lines.Should().HaveCount(1);
        payload.Lines[0].StockCode.Should().Be("STK001");
        payload.Lines[0].Quantity.Should().Be(2.5m);
        payload.Lines[0].Discounts.Should().HaveCount(6);
    }

    [Fact]
    public void Deserialize_tolerates_missing_optional_SalespersonCode()
    {
        var json = ValidPayloadJson.Replace("\"SalespersonCode\": \"PL01\",", string.Empty);

        var result = _sut.Deserialize(json);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SalespersonCode.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_EMPTY_for_empty_input(string? input)
    {
        var result = _sut.Deserialize(input!);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(SalesOrderPayloadDeserializer.ErrorCodeEmptyPayload);
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("{ this is not json")]
    [InlineData("\"unwrapped string\"")]
    [InlineData("[1,2,3]")]
    [InlineData("1234")]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_JSON_for_malformed_input(string json)
    {
        var result = _sut.Deserialize(json);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(SalesOrderPayloadDeserializer.ErrorCodeMalformedJson);
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_SHAPE_when_ExternalId_missing()
    {
        var json = ValidPayloadJson.Replace("\"ExternalId\": \"ext-uuid-001\",", string.Empty);

        var result = _sut.Deserialize(json);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(SalesOrderPayloadDeserializer.ErrorCodeInvalidPayload);
        result.Error.Should().Contain("ExternalId");
    }

    [Fact]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_SHAPE_when_Lines_empty()
    {
        // Build the empty-lines variant explicitly — raw-string-literal escapes
        // make a multi-line Replace call fiddly. Keeping the structure next to
        // the assertion makes the test self-documenting.
        var json = """
            {
              "TenantId": "tenant-1",
              "ExternalId": "ext-uuid-001",
              "CustomerCode": "120.01.0001",
              "SalespersonCode": "PL01",
              "WarehouseNo": 1,
              "DocumentSeries": "S",
              "DocumentNumber": 1234,
              "OccurredAt": "2026-07-09T10:30:00Z",
              "Currency": "TRY",
              "Lines": []
            }
            """;

        var result = _sut.Deserialize(json);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(SalesOrderPayloadDeserializer.ErrorCodeInvalidPayload);
        result.Error.Should().Contain("Lines");
    }

    [Fact]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_SHAPE_when_DocumentNumber_zero()
    {
        var json = ValidPayloadJson.Replace("\"DocumentNumber\": 1234,", "\"DocumentNumber\": 0,");

        var result = _sut.Deserialize(json);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(SalesOrderPayloadDeserializer.ErrorCodeInvalidPayload);
        result.Error.Should().Contain("DocumentNumber");
    }

    [Fact]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_SHAPE_when_CustomerCode_missing()
    {
        var json = ValidPayloadJson.Replace("\"CustomerCode\": \"120.01.0001\",", string.Empty);

        var result = _sut.Deserialize(json);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(SalesOrderPayloadDeserializer.ErrorCodeInvalidPayload);
        result.Error.Should().Contain("CustomerCode");
    }

    [Fact]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_SHAPE_when_line_Quantity_negative()
    {
        var json = ValidPayloadJson.Replace("\"Quantity\": 2.5,", "\"Quantity\": -1,");

        var result = _sut.Deserialize(json);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(SalesOrderPayloadDeserializer.ErrorCodeInvalidPayload);
        result.Error.Should().Contain("Quantity");
    }

    [Fact]
    public void Deserialize_returns_Fail_INVALID_PAYLOAD_JSON_when_payload_is_null_literal()
    {
        var result = _sut.Deserialize("null");

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().BeOneOf(
            SalesOrderPayloadDeserializer.ErrorCodeMalformedJson,
            SalesOrderPayloadDeserializer.ErrorCodeInvalidPayload);
    }
}