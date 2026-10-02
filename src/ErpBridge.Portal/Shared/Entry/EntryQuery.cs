using ErpBridge.Portal.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace ErpBridge.Portal.Shared.Entry;

/// <summary>An entry page opened from a customer (<c>?cari=</c>): the customer is already chosen.</summary>
public static class EntryQuery
{
    public static string? Customer(NavigationManager nav) =>
        QueryHelpers.ParseQuery(new Uri(nav.Uri).Query).TryGetValue("cari", out var code) && !string.IsNullOrWhiteSpace(code) ? code.ToString().Trim() : null;

    /// <summary>The customer with exactly this code, or null.</summary>
    public static async Task<EntryCustomerDto?> FindCustomerAsync(PortalApiClient api, string code) =>
        (await api.EntryCustomersAsync(code)).Items.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));
}
