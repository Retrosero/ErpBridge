using System.Text.Json;
using ErpBridge.CentralApi.Endpoints;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// The one order a product's barcodes are served in, and therefore its primary barcode
/// (<c>barkod</c> = the first one).
///
/// <para>The phone keys its product table by the primary barcode. Neither source of
/// barcode rows has an order of its own: the agent reads <c>BARKOD_TANIMLARI</c> without
/// <c>ORDER BY</c> (so <c>/sync/urun</c> got whatever order the snapshot chunks held) and
/// the change feed reads <c>mobile_records</c> without one either. A primary barcode
/// that changes between two builds of the same product makes the phone store it twice.
/// Both paths sort through here, so a tenant that moves from the table endpoints to the
/// feed keeps the same primary barcode (knowledge base, rule 12).</para>
///
/// <para>The order:</para>
/// <list type="number">
/// <item>A real barcode before a stand-in — the phone's table path skips a barcode that
/// is blank, equals the stock code or starts with <c>STK-</c>, so serving such a value as
/// <c>barkod</c> would make the two paths pick differently.</item>
/// <item>The main unit's barcode (unit pointer 0 or 1) before a carton's.</item>
/// <item>Then the barcode text, ordinal; the raw row text breaks any remaining tie.</item>
/// </list>
/// </summary>
public static class ProductBarcodes
{
    /// <summary>Prefix of the stand-in barcodes the phone makes for products that have none.</summary>
    public const string StandInPrefix = "STK-";

    /// <summary>The rows of one product in serving order.</summary>
    /// <param name="rows">Barcode rows (<c>barcode</c>, <c>stockCode</c>, <c>unitPointer</c>).</param>
    /// <param name="stockCode">The product's stock code; a barcode equal to it is a stand-in.</param>
    public static JsonElement[] Order(IEnumerable<JsonElement> rows, string? stockCode)
    {
        ArgumentNullException.ThrowIfNull(rows);
        return rows
            .OrderBy(row => IsReal(AndroidEndpoints.GetString(row, "barcode"), stockCode) ? 0 : 1)
            .ThenBy(row => Math.Max(AndroidEndpoints.GetInt32(row, "unitPointer") ?? 1, 1))
            .ThenBy(row => AndroidEndpoints.GetString(row, "barcode") ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(row => row.GetRawText(), StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>The primary barcode of rows already in <see cref="Order"/>; empty when there are none.</summary>
    public static string Primary(IReadOnlyList<JsonElement> ordered)
    {
        ArgumentNullException.ThrowIfNull(ordered);
        return ordered.Count == 0 ? string.Empty : AndroidEndpoints.GetString(ordered[0], "barcode") ?? string.Empty;
    }

    /// <summary>Whether the phone treats <paramref name="barcode"/> as a real barcode of the product.</summary>
    public static bool IsReal(string? barcode, string? stockCode) =>
        !string.IsNullOrWhiteSpace(barcode)
        && !string.Equals(barcode, stockCode, StringComparison.Ordinal)
        && !barcode.StartsWith(StandInPrefix, StringComparison.Ordinal);
}
