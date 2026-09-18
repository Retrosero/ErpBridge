using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Parameters;

/// <summary>
/// Whose settings are being read or written: a company, and within it the thing the parameter set
/// is addressed by — one mobile user, one printer template, one import template, and so on.
/// </summary>
/// <param name="TenantId">Customer.</param>
/// <param name="ErpCompanyId">
/// ERP company. Required: a tenant can own several, each a different Mikro database with its own
/// warehouses, branches and document series (D3b).
/// </param>
/// <param name="MobileUserId">
/// The mobile user, for the <c>MobileUser</c> scope. Held as an id rather than a username because
/// a username is a reusable label: deleting a user keeps the row and frees the name (D5).
/// </param>
/// <param name="Scope1">First scope column's value for every other scope kind.</param>
/// <param name="Scope2">Second scope column's value; only printer template fields need one.</param>
public readonly record struct ParameterScope(
    Guid TenantId,
    Guid ErpCompanyId,
    Guid? MobileUserId = null,
    string Scope1 = "",
    string Scope2 = "")
{
    /// <summary>A scope for a mobile user's own settings.</summary>
    public static ParameterScope ForMobileUser(Guid tenantId, Guid erpCompanyId, Guid mobileUserId) =>
        new(tenantId, erpCompanyId, mobileUserId);

    /// <summary>A scope for a set that exists once per company.</summary>
    public static ParameterScope ForCompany(Guid tenantId, Guid erpCompanyId) =>
        new(tenantId, erpCompanyId);

    /// <summary>A scope named by one value: a template name, criteria name or report code.</summary>
    public static ParameterScope ForName(Guid tenantId, Guid erpCompanyId, string name) =>
        new(tenantId, erpCompanyId, Scope1: name);

    /// <summary>A scope named by two values: a printer template and one of its fields.</summary>
    public static ParameterScope ForTemplateField(
        Guid tenantId, Guid erpCompanyId, string template, string field) =>
        new(tenantId, erpCompanyId, Scope1: template, Scope2: field);

    /// <summary>
    /// Whether this scope can address <paramref name="entry"/>, and why not when it cannot.
    ///
    /// Worth checking rather than trusting: writing a mobile user's permission without saying
    /// which user, or a printer field without saying which template, would create a row that no
    /// read ever finds and that the mirror cannot place in Mikro.
    /// </summary>
    public string? Reject(ParameterCatalogEntry entry)
    {
        var fields = entry.ScopeFields.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var wantsUser = entry.ScopeKind == ParameterScopeKinds.MobileUser;
        var named = fields.Length - (wantsUser ? 1 : 0);

        if (wantsUser && MobileUserId is null)
        {
            return $"'{entry.Name}' belongs to one mobile user; the scope names none.";
        }

        if (!wantsUser && MobileUserId is not null)
        {
            return $"'{entry.Name}' is not addressed by mobile user, but the scope names one.";
        }

        // A named scope carries as many values as the catalogue says the set is addressed by.
        var supplied = (string.IsNullOrEmpty(Scope1) ? 0 : 1) + (string.IsNullOrEmpty(Scope2) ? 0 : 1);
        if (supplied != named)
        {
            return $"'{entry.Name}' is addressed by {named} name(s); the scope supplies {supplied}.";
        }

        return null;
    }
}

/// <summary>Scope kinds as the catalogue records them.</summary>
public static class ParameterScopeKinds
{
    public const string None = "None";
    public const string MobileUser = "MobileUser";
    public const string DesktopUser = "DesktopUser";
    public const string ReportCode = "ReportCode";
    public const string ImportTemplate = "ImportTemplate";
    public const string CriteriaName = "CriteriaName";
    public const string EdiRelation = "EdiRelation";
    public const string PrinterTemplate = "PrinterTemplate";
}
