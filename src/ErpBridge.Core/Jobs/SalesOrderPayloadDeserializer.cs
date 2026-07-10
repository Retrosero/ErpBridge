using System.Text.Json;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Shared;

namespace ErpBridge.Core.Jobs;

/// <summary>
/// Parses the JSON payload string of a <see cref="Domain.RemoteJob"/> with
/// <see cref="Domain.RemoteJob.DocumentType"/> = <c>sales_order</c> into a
/// strongly-typed <see cref="SalesOrderPayload"/>.
///
/// Lives in <c>ErpBridge.Core</c> so the Agent worker can validate the wire
/// shape BEFORE handing the payload to the ERP adapter. The Core package
/// references only <c>ErpBridge.Shared</c> and <c>ErpBridge.Erp.Abstractions</c>,
/// so this file MUST NOT import <c>System.Text.Json.Serialization</c> from a
/// non-BCL package (Newtonsoft.Json is intentionally not referenced) and
/// MUST NOT touch anything from <c>ErpBridge.Erp.Mikro</c>.
///
/// Errors are surfaced through <see cref="Result{T}"/> rather than exceptions
/// because payload corruption is an expected control-flow outcome (the central
/// API may push malformed JSON during a partial deploy) — the worker should
/// produce a structured <c>JobAck(status="failed", code="INVALID_PAYLOAD")</c>
/// instead of crashing the poll loop.
///
/// JSON library choice: <c>System.Text.Json</c>. It is part of the .NET 8 BCL,
/// already used by the <c>ErpBridge.RemoteApi</c> HTTP client to (de)serialize
/// the same payload shape. Switching to Newtonsoft would add a dependency to
/// <c>Core</c> just to consume one document type — not justified at this stage.
/// </summary>
public sealed class SalesOrderPayloadDeserializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Stable error code for empty / whitespace-only payloads.
    /// </summary>
    public const string ErrorCodeEmptyPayload = "INVALID_PAYLOAD_EMPTY";

    /// <summary>
    /// Stable error code for JSON that does not parse into a payload object.
    /// </summary>
    public const string ErrorCodeMalformedJson = "INVALID_PAYLOAD_JSON";

    /// <summary>
    /// Stable error code for JSON that parses but violates the structural
    /// invariants of <see cref="SalesOrderPayload"/> (missing required field,
    /// empty Lines, etc.). Returned by the deserializer for shape errors
    /// only — the adapter owns deeper business validation (cari / stok /
    /// depo lookups).
    /// </summary>
    public const string ErrorCodeInvalidPayload = "INVALID_PAYLOAD_SHAPE";

    /// <summary>
    /// Parse <paramref name="json"/> into a <see cref="SalesOrderPayload"/>.
    /// Returns <see cref="Result{T}.Fail"/> on any structural problem —
    /// never throws for an expected bad-payload path.
    /// </summary>
    /// <param name="json">Raw JSON body from the central API.</param>
    /// <returns>
    /// <see cref="Result{T}.Ok"/> with a fully-populated payload on success.
    /// <see cref="Result{T}.Fail"/> with one of the
    /// <see cref="ErrorCodeEmptyPayload"/> / <see cref="ErrorCodeMalformedJson"/> /
    /// <see cref="ErrorCodeInvalidPayload"/> codes plus a human-readable
    /// diagnostic.
    /// </returns>
    public Result<SalesOrderPayload> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Result<SalesOrderPayload>.Fail(
                ErrorCodeEmptyPayload,
                "RemoteJob payload is empty or whitespace.");
        }

        SalesOrderPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SalesOrderPayload>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            return Result<SalesOrderPayload>.Fail(
                ErrorCodeMalformedJson,
                $"Sales order payload could not be parsed as JSON: {ex.Message}");
        }

        if (payload is null)
        {
            // System.Text.Json returns null for the JSON literal "null" and for
            // an empty document — both are programmer / pipeline errors at the
            // central API, so surface them as INVALID_PAYLOAD_SHAPE rather than
            // INVALID_PAYLOAD_EMPTY (the caller already trimmed whitespace).
            return Result<SalesOrderPayload>.Fail(
                ErrorCodeInvalidPayload,
                "Sales order payload deserialized to null.");
        }

        var validation = ValidateShape(payload);
        if (validation is not null)
        {
            return Result<SalesOrderPayload>.Fail(ErrorCodeInvalidPayload, validation);
        }

        return Result<SalesOrderPayload>.Ok(payload);
    }

    /// <summary>
    /// Shape-level invariants that <see cref="System.Text.Json.JsonSerializer"/>
    /// cannot enforce on its own (positional records accept defaults for
    /// missing JSON members). Mirrors the essentials of
    /// <c>MikroSalesOrderWriter.ValidatePayload</c> but at the deserialization
    /// boundary: anything beyond this is the adapter's responsibility, so we
    /// don't duplicate Mikro-specific guards here.
    /// </summary>
    private static string? ValidateShape(SalesOrderPayload p)
    {
        if (string.IsNullOrWhiteSpace(p.TenantId))
        {
            return "TenantId is required.";
        }

        if (string.IsNullOrWhiteSpace(p.ExternalId))
        {
            return "ExternalId is required.";
        }

        if (string.IsNullOrWhiteSpace(p.CustomerCode))
        {
            return "CustomerCode is required.";
        }

        if (string.IsNullOrWhiteSpace(p.DocumentSeries))
        {
            return "DocumentSeries is required.";
        }

        if (p.DocumentNumber <= 0)
        {
            return "DocumentNumber must be greater than zero.";
        }

        if (p.WarehouseNo <= 0)
        {
            return "WarehouseNo must be greater than zero.";
        }

        if (string.IsNullOrWhiteSpace(p.Currency))
        {
            return "Currency is required.";
        }

        if (p.Lines is null || p.Lines.Count == 0)
        {
            return "Lines must contain at least one entry.";
        }

        for (var i = 0; i < p.Lines.Count; i++)
        {
            var line = p.Lines[i];
            if (line is null)
            {
                return $"Line {i}: payload entry is null.";
            }

            if (string.IsNullOrWhiteSpace(line.StockCode))
            {
                return $"Line {i}: StockCode is required.";
            }

            if (line.Quantity <= 0)
            {
                return $"Line {i}: Quantity must be greater than zero.";
            }

            if (line.UnitPointer <= 0)
            {
                return $"Line {i}: UnitPointer must be greater than zero.";
            }

            if (line.UnitPrice < 0)
            {
                return $"Line {i}: UnitPrice cannot be negative.";
            }
        }

        return null;
    }
}