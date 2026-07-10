using System.Collections.Concurrent;

namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Default MVP implementation of the three lookup interfaces. Uses in-memory sets
/// preloaded by the host (typically via DI). Production implementations will swap
/// these for SQL-backed readers without touching <c>MikroSalesOrderWriter</c>.
/// </summary>
public sealed class InMemoryCustomerLookup : ICustomerLookup
{
    private readonly ConcurrentDictionary<string, byte> _codes;

    /// <summary>Empty lookup — every query resolves to false. Useful as a default.</summary>
    public InMemoryCustomerLookup() : this(Array.Empty<string>()) { }

    /// <summary>Build a lookup from an initial set of customer codes.</summary>
    public InMemoryCustomerLookup(IEnumerable<string> customerCodes)
    {
        ArgumentNullException.ThrowIfNull(customerCodes);
        _codes = new ConcurrentDictionary<string, byte>(
            customerCodes.Where(c => !string.IsNullOrWhiteSpace(c))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Select(c => new KeyValuePair<string, byte>(c, 0)),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string customerCode, CancellationToken ct = default)
        => Task.FromResult(!string.IsNullOrWhiteSpace(customerCode) && _codes.ContainsKey(customerCode));

    /// <summary>Manually add a customer code (used by tests).</summary>
    public void Add(string code)
    {
        if (!string.IsNullOrWhiteSpace(code))
            _codes[code] = 0;
    }
}

/// <inheritdoc />
public sealed class InMemoryStockLookup : IStockLookup
{
    private readonly ConcurrentDictionary<string, byte> _codes;

    public InMemoryStockLookup() : this(Array.Empty<string>()) { }

    public InMemoryStockLookup(IEnumerable<string> stockCodes)
    {
        ArgumentNullException.ThrowIfNull(stockCodes);
        _codes = new ConcurrentDictionary<string, byte>(
            stockCodes.Where(c => !string.IsNullOrWhiteSpace(c))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Select(c => new KeyValuePair<string, byte>(c, 0)),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string stockCode, CancellationToken ct = default)
        => Task.FromResult(!string.IsNullOrWhiteSpace(stockCode) && _codes.ContainsKey(stockCode));

    public void Add(string code)
    {
        if (!string.IsNullOrWhiteSpace(code))
            _codes[code] = 0;
    }
}

/// <inheritdoc />
public sealed class InMemoryWarehouseLookup : IWarehouseLookup
{
    private readonly ConcurrentDictionary<int, byte> _codes;

    public InMemoryWarehouseLookup() : this(Array.Empty<int>()) { }

    public InMemoryWarehouseLookup(IEnumerable<int> warehouseNos)
    {
        ArgumentNullException.ThrowIfNull(warehouseNos);
        _codes = new ConcurrentDictionary<int, byte>(
            warehouseNos.Distinct().Select(n => new KeyValuePair<int, byte>(n, 0)));
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(int warehouseNo, CancellationToken ct = default)
        => Task.FromResult(_codes.ContainsKey(warehouseNo));

    public void Add(int no) => _codes[no] = 0;
}
