using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ErpBridge.Portal.Api;

/// <summary>
/// The catalog access sheet's wording and checks (GOAL_MUSTERI_KATALOGU P4): the message shared with the customer, its
/// WhatsApp link and the form's rules. Kept out of the component so they are tested without rendering.
/// </summary>
public static class CatalogText
{
    public const int MinPasswordBytes = 8;
    public const int MaxPasswordBytes = 72;

    private static readonly Regex UsernamePattern = new("^[a-z0-9._-]{3,64}$", RegexOptions.CultureInvariant);

    /// <summary>
    /// A Turkish phone the way WhatsApp wants it: digits only, a leading 0 becomes 90 (00 is the international prefix),
    /// a bare ten-digit mobile (5…) gets 90 in front. Null when no usable number is left.
    /// </summary>
    public static string? WhatsAppNumber(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal)) digits = digits[2..];
        else if (digits.StartsWith('0')) digits = "90" + digits[1..];
        else if (digits.Length == 10 && digits[0] == '5') digits = "90" + digits;
        return digits.Length >= 11 ? digits : null;
    }

    /// <summary>A WhatsApp link with the message filled in; without a number WhatsApp lets the user pick the chat.</summary>
    public static string WhatsAppLink(string? phone, string text) =>
        $"https://wa.me/{WhatsAppNumber(phone)}?text={Uri.EscapeDataString(text)}";

    /// <summary>
    /// What the customer is sent. The password goes in only when given: the one the server has just issued, and only when
    /// the user asked for it.
    /// </summary>
    public static string ShareMessage(string customer, string company, string link, string username, string? password = null) =>
        $"Merhaba {customer}, {company} ürün kataloğumuza {link} adresinden girebilirsiniz. Kullanıcı adınız: {username}"
        + (string.IsNullOrEmpty(password) ? string.Empty : $", şifreniz: {password}");

    /// <summary>The customer's price for a list price, two decimals half away from zero like the server's <c>CatalogPricing</c>.</summary>
    public static decimal Net(decimal listPrice, decimal discountPercent) =>
        Math.Round(listPrice * (1 - discountPercent / 100m), 2, MidpointRounding.AwayFromZero);

    /// <summary>The discount typed in the form: empty is 0; otherwise 0–99.99 with at most two decimals, else null.</summary>
    public static decimal? ParseDiscount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0m;
        if (!decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)) return null;
        return value is >= 0m and <= 99.99m && decimal.Round(value, 2) == value ? value : null;
    }

    /// <summary>"%10", "%12,5".</summary>
    public static string Percent(decimal value) => "%" + value.ToString("0.##", Fmt.Turkish);

    /// <summary>A password the user typed: 8–72 bytes in UTF-8, the BCrypt limit the server checks.</summary>
    public static bool IsValidPassword(string? password) =>
        password is not null && Encoding.UTF8.GetByteCount(password) is >= MinPasswordBytes and <= MaxPasswordBytes;

    /// <summary>The username as the server keeps it ("A.B" → "a.b").</summary>
    public static string NormalizeUsername(string? username) => (username ?? string.Empty).Trim().ToLowerInvariant();

    public static bool IsValidUsername(string username) => UsernamePattern.IsMatch(username);
}

/// <summary>
/// The access sheet's working copy of one customer's catalog access. The visibility's three screen choices map onto the
/// stored <c>{mode, rules}</c> (GOAL_MUSTERI_KATALOGU §4): "Ana katalog" = <c>all</c> with no rules, "Seçilenler hariç" =
/// <c>all</c> with deny rules, "Yalnız seçilenler" = <c>only</c> with allow rules. A product the company hid but this
/// customer sees is an allow rule under either <c>all</c> choice; under <c>only</c> a chosen product shows anyway.
/// </summary>
public sealed class CatalogAccessForm
{
    public const string Main = "main";
    public const string Only = "only";
    public const string Except = "except";

    public string Username { get; set; } = string.Empty;

    /// <summary>True: the server makes the password and returns it once (create only).</summary>
    public bool GeneratePassword { get; set; } = true;
    public string? Password { get; set; }
    public string DiscountText { get; set; } = "0";
    public int? PriceListNo { get; set; }
    public string Visibility { get; set; } = Main;

    /// <summary>Category keys: hidden ones under "Seçilenler hariç", shown ones under "Yalnız seçilenler".</summary>
    public List<string> Categories { get; set; } = [];

    /// <summary>Stock codes, chosen the same way as <see cref="Categories"/>.</summary>
    public List<string> Products { get; set; } = [];

    /// <summary>Stock codes the company hid but this customer sees (allow rules under <c>all</c>).</summary>
    public List<string> Revealed { get; set; } = [];

    public bool ShowStatement { get; set; }
    public bool ShowInvoices { get; set; }
    public bool ShowPurchased { get; set; }
    public bool CanOrder { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public Guid? ResponsibleUserId { get; set; }

    /// <summary>
    /// Rules the screen has no control for (a category shown although hidden under <c>all</c>, a product left out under
    /// <c>only</c>; the phone may write them), with the mode they belong to. They go back unchanged while that mode stays.
    /// </summary>
    private List<CatalogVisibilityRuleDto> _kept = [];
    private string _keptMode = "all";

    public static CatalogAccessForm New(string? suggestedUsername) => new() { Username = suggestedUsername ?? string.Empty };

    public static CatalogAccessForm From(CatalogAccountDto account)
    {
        var rules = account.Visibility.Rules;
        var only = account.Visibility.Mode == "only";
        var except = !only && rules.Any(r => r.Effect == "deny");
        var form = new CatalogAccessForm
        {
            Username = account.Username,
            GeneratePassword = false,
            DiscountText = account.DiscountPercent.ToString("0.##", CultureInfo.InvariantCulture),
            PriceListNo = account.PriceListNo,
            Visibility = only ? Only : except ? Except : Main,
            ShowStatement = account.ShowStatement,
            ShowInvoices = account.ShowInvoices,
            ShowPurchased = account.ShowPurchased,
            CanOrder = account.CanOrder,
            IsActive = account.IsActive,
            ResponsibleUserId = account.ResponsibleUserId,
            _keptMode = only ? "only" : "all",
        };
        // Under "only" the allow rules are the choice; under "all" the deny rules are, and product allows are the reveals.
        var chosen = only ? "allow" : "deny";
        foreach (var rule in rules)
        {
            if (rule.Effect == chosen && rule.Type == "category") form.Categories.Add(rule.Key);
            else if (rule.Effect == chosen && rule.Type == "product") form.Products.Add(rule.Key);
            else if (!only && rule.Effect == "allow" && rule.Type == "product") form.Revealed.Add(rule.Key);
            else form._kept.Add(rule);
        }
        return form;
    }

    public CatalogVisibilityDto ToVisibility()
    {
        var mode = Visibility == Only ? "only" : "all";
        var rules = new List<CatalogVisibilityRuleDto>();
        if (Visibility != Main)
        {
            var effect = Visibility == Only ? "allow" : "deny";
            rules.AddRange(Categories.Distinct(StringComparer.Ordinal).Select(key => Rule("category", key, effect)));
            rules.AddRange(Products.Distinct(StringComparer.OrdinalIgnoreCase).Select(key => Rule("product", key, effect)));
        }
        if (Visibility != Only)
            rules.AddRange(Revealed.Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(code => Visibility == Main || !Products.Contains(code, StringComparer.OrdinalIgnoreCase))
                .Select(code => Rule("product", code, "allow")));
        if (mode == _keptMode) rules.AddRange(_kept);
        return new CatalogVisibilityDto { Mode = mode, Rules = rules };
    }

    /// <summary>The first problem that keeps the form from being sent, in the screen's words; null when it can go.</summary>
    public string? Problem(bool creating)
    {
        if (!CatalogText.IsValidUsername(CatalogText.NormalizeUsername(Username))) return PortalMessages.For("INVALID_USERNAME");
        if (creating && !GeneratePassword && !CatalogText.IsValidPassword(Password)) return PasswordRule;
        if (CatalogText.ParseDiscount(DiscountText) is null) return PortalMessages.For("INVALID_DISCOUNT");
        if (Visibility == Only && Categories.Count == 0 && Products.Count == 0)
            return "Yalnız seçilenler için en az bir kategori ya da ürün seçin; yoksa müşteri hiçbir ürün görmez.";
        return null;
    }

    public const string PasswordRule = "Şifre 8–72 karakter olmalı.";

    public CatalogAccountCreateRequest ToCreate(string customerCode) => new()
    {
        CustomerCode = customerCode,
        Username = CatalogText.NormalizeUsername(Username),
        Password = GeneratePassword ? null : Password,
        IsActive = IsActive,
        DiscountPercent = CatalogText.ParseDiscount(DiscountText) ?? 0m,
        PriceListNo = PriceListNo,
        Visibility = ToVisibility(),
        ShowStatement = ShowStatement,
        ShowInvoices = ShowInvoices,
        ShowPurchased = ShowPurchased,
        CanOrder = CanOrder,
        ResponsibleUserId = ResponsibleUserId,
    };

    /// <summary>The whole form; a PATCH that sends every field also clears the list and the responsible person.</summary>
    public CatalogAccountPatchRequest ToPatch() => new()
    {
        Username = CatalogText.NormalizeUsername(Username),
        IsActive = IsActive,
        DiscountPercent = CatalogText.ParseDiscount(DiscountText) ?? 0m,
        PriceListNo = PriceListNo,
        Visibility = ToVisibility(),
        ShowStatement = ShowStatement,
        ShowInvoices = ShowInvoices,
        ShowPurchased = ShowPurchased,
        CanOrder = CanOrder,
        ResponsibleUserId = ResponsibleUserId,
    };

    private static CatalogVisibilityRuleDto Rule(string type, string key, string effect) => new() { Type = type, Key = key, Effect = effect };
}
