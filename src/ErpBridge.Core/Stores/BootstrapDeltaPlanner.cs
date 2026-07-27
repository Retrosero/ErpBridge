using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ErpBridge.Erp.Abstractions.Sync;

namespace ErpBridge.Core.Stores;

/// <summary>Creates a compact delta from a snapshot. The persisted row inventory
/// lets the agent report physical deletions as well as changed rows.</summary>
public sealed class BootstrapDeltaPlanner
{
    private static readonly Dictionary<string, string[]> KeyFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["customers"] = ["customerCode"], ["customerAddresses"] = ["customerCode", "addressNo"],
        ["customerContacts"] = ["customerCode", "email", "mobile"], ["stocks"] = ["stockCode"],
        ["barcodes"] = ["barcode"], ["prices"] = ["stockCode", "listNumber"],
        ["salesConditions"] = ["stockCode", "customerCode", "warehouseNo", "paymentPlanNo", "startDate", "endDate"],
        ["inventory"] = ["stockCode", "warehouseNo"],
        ["openOrders"] = ["series", "number", "lineNo"], ["cashAndBank"] = ["kind", "code"],
        ["lookups"] = ["kind", "code"], ["customerTransactions"] = ["erpRef"], ["stockTransactions"] = ["erpRef"]
    };

    public BootstrapDelta Create(SyncPackage package, IReadOnlyDictionary<string, string> previous)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(package, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var current = new Dictionary<string, string>(StringComparer.Ordinal);
        var upserts = new Dictionary<string, IReadOnlyList<BootstrapDeltaRow>>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!KeyFields.TryGetValue(property.Name, out var keyFields) || property.Value.ValueKind != JsonValueKind.Array) continue;
            var changed = new List<BootstrapDeltaRow>();
            foreach (var item in property.Value.EnumerateArray())
            {
                var key = property.Name + ":" + BuildKey(item, keyFields);
                var json = item.GetRawText();
                var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
                current[key] = hash;
                if (!previous.TryGetValue(key, out var oldHash) || !StringComparer.Ordinal.Equals(oldHash, hash))
                    changed.Add(new BootstrapDeltaRow(key[(property.Name.Length + 1)..], json));
            }
            if (changed.Count > 0) upserts[property.Name] = changed;
        }
        var deletes = previous.Keys.Where(key => !current.ContainsKey(key)).GroupBy(key => key[..key.IndexOf(':')])
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.Select(key => key[(key.IndexOf(':') + 1)..]).ToArray(), StringComparer.OrdinalIgnoreCase);
        return new BootstrapDelta(package.PulledAtUtc, package.SourceDatabase, upserts, deletes);
    }

    public IReadOnlyDictionary<string, string> Snapshot(SyncPackage package) => Create(package, new Dictionary<string, string>()).Upserts
        .SelectMany(section => section.Value.Select(row => new KeyValuePair<string, string>(section.Key + ":" + row.Key, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(row.PayloadJson))))))
        .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

    private static string BuildKey(JsonElement item, IEnumerable<string> fields) => string.Join("|", fields.Select(field => item.TryGetProperty(field, out var value) ? value.ToString() : ""));
}
