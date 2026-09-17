using System.Globalization;
using ErpBridge.Core.Stores;

namespace ErpBridge.Core.Logging;

/// <summary>
/// Log Merkezi L3e: one INFO event per sync round, the agent's counterpart to the phone's <c>SYNC_ROUND</c>.
/// It answers "is this agent actually syncing, how long does a round take, and how much is moving" without
/// anyone opening a remote session — and it carries counts only, never a customer, a stock code or a price.
/// </summary>
public static class AgentSyncRound
{
    public const string Kind = "AGENT_SYNC_ROUND";

    /// <summary>What started the round; the panel uses it to tell a timer tick from an operator's click.</summary>
    public static class Triggers
    {
        public const string Timer = "timer";
        public const string Manual = "manual";
        public const string Signal = "signal";
    }

    /// <summary>Which cycle ran: the ERP change log, or the snapshot delta.</summary>
    public static class Modes
    {
        public const string ChangeLog = "changelog";
        public const string Snapshot = "snapshot";
        public const string Section = "section";
    }

    /// <summary>The snapshot-delta round.</summary>
    public static Task<bool> ReportAsync(
        this IAgentLogReporter? reporter,
        string trigger,
        BootstrapSyncResult result,
        string mode = Modes.Snapshot,
        string? section = null,
        CancellationToken ct = default)
    {
        if (reporter is null) return Task.FromResult(false);
        ArgumentNullException.ThrowIfNull(result);

        var sections = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["customers"] = result.CustomersCount,
            ["stocks"] = result.StocksCount,
            ["prices"] = result.PricesCount,
            ["inventory"] = result.InventoryCount,
            ["openOrders"] = result.OpenOrdersCount,
            ["cashAndBank"] = result.CashAndBankCount,
            ["lookups"] = result.LookupsCount,
            ["customerAddresses"] = result.CustomerAddressesCount,
            ["customerContacts"] = result.CustomerContactsCount,
            ["barcodes"] = result.BarcodesCount,
            ["salesConditions"] = result.SalesConditionsCount,
            ["customerTransactions"] = result.CustomerTransactionsCount,
            ["stockTransactions"] = result.StockTransactionsCount,
        };
        var rows = sections.Values.Sum();
        var properties = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["trigger"] = trigger,
            ["mode"] = mode,
            ["success"] = result.Success,
            ["durationMs"] = result.DurationMs,
            ["rows"] = rows,
            ["payloadBytes"] = result.PayloadBytes,
            ["errorCode"] = result.ErrorCode,
            ["section"] = section,
        };
        // Only the sections that moved: an event listing thirteen zeroes says nothing and costs the 4 KB budget.
        foreach (var (name, count) in sections.Where(pair => pair.Value > 0))
            properties["rows." + name] = count;

        return reporter.ReportAsync("INFO", Kind, Operation(mode),
            Message(result.Success, rows, result.DurationMs, result.ErrorCode), null, properties, ct: ct);
    }

    /// <summary>The ERP change-log round.</summary>
    public static Task<bool> ReportAsync(
        this IAgentLogReporter? reporter,
        string trigger,
        ErpChangeLogSyncResult result,
        CancellationToken ct = default)
    {
        if (reporter is null) return Task.FromResult(false);
        ArgumentNullException.ThrowIfNull(result);

        var properties = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["trigger"] = trigger,
            ["mode"] = Modes.ChangeLog,
            ["success"] = result.Success,
            ["durationMs"] = result.DurationMs,
            ["rows"] = result.TotalRowsPushed,
            ["rows.upserts"] = result.UpsertRowsPushed,
            ["rows.deletes"] = result.DeleteRowsPushed,
            ["tablesTouched"] = result.TablesTouched,
            ["moreAvailable"] = result.MoreAvailable,
            ["errorCode"] = result.ErrorCode,
        };

        return reporter.ReportAsync("INFO", Kind, Operation(Modes.ChangeLog),
            Message(result.Success, result.TotalRowsPushed, result.DurationMs, result.ErrorCode), null, properties, ct: ct);
    }

    private static string Operation(string mode) => "sync.round." + mode;

    /// <summary>
    /// The text carries the numbers for a human reading one event; the fingerprint drops them, so a round every
    /// twenty seconds still queues one event per throttle window with the repeats counted.
    /// </summary>
    private static string Message(bool success, int rows, long durationMs, string? errorCode) => success
        ? string.Format(CultureInfo.InvariantCulture, "Senkron turu tamamlandı: {0} satır, {1} ms.", rows, durationMs)
        : string.Format(CultureInfo.InvariantCulture, "Senkron turu başarısız ({0}), {1} ms.", errorCode ?? "UNKNOWN", durationMs);
}
