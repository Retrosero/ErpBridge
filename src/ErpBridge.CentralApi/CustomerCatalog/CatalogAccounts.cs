using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// The rules of a customer's catalog sign-in (GOAL_MUSTERI_KATALOGU T3): usernames as staff usernames
/// (<c>MobileSeatService.NormalizeUsername</c>), passwords of 8–72 bytes (BCrypt reads only the first 72), and a
/// server-made password when staff leave it empty, shown to them once.
/// </summary>
public static class CatalogAccounts
{
    public const int MinPasswordBytes = 8;
    public const int MaxPasswordBytes = 72;
    public const int GeneratedPasswordLength = 10;
    public const int MaxUsernameLength = 64;
    public const decimal MaxDiscountPercent = 99.99m;

    /// <summary>Lower-case letters and digits without the ones read alike (i l o 0 1): read out over the phone.</summary>
    private const string PasswordAlphabet = "abcdefghjkmnpqrstuvwxyz23456789";

    /// <summary>A suggestion stays short enough to type; it is cut at a word.</summary>
    private const int SuggestionLength = 24;

    /// <summary>Company-form words a username does without ("ltd", "şti" …).</summary>
    private static readonly HashSet<string> Filler = new(StringComparer.Ordinal) { "ltd", "sti", "tic", "san", "ve", "as", "inc", "co" };

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string GeneratePassword() =>
        string.Create(GeneratedPasswordLength, 0, static (span, _) =>
        {
            for (var i = 0; i < span.Length; i++) span[i] = PasswordAlphabet[RandomNumberGenerator.GetInt32(PasswordAlphabet.Length)];
        });

    /// <summary>
    /// Ends every session (and device trust) of the account: <c>TokenVersion</c> moves by one in the database itself
    /// (<c>TokenVersion = TokenVersion + 1</c>), so two revocations at once both count and a save of an older copy of the
    /// row cannot put a version back. The tracked <paramref name="account"/> takes the new number without becoming
    /// modified; call it after the account's own save.
    /// </summary>
    public static async Task RevokeSessionsAsync(CentralApiDbContext db, CatalogAccount account, CancellationToken ct)
    {
        await db.CatalogAccounts.Where(a => a.Id == account.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.TokenVersion, a => a.TokenVersion + 1), ct);
        var version = await db.CatalogAccounts.AsNoTracking().Where(a => a.Id == account.Id).Select(a => a.TokenVersion).FirstAsync(ct);
        var entry = db.Entry(account);
        if (entry.State == EntityState.Detached)
        {
            account.TokenVersion = version;
            return;
        }
        var property = entry.Property(a => a.TokenVersion);
        property.CurrentValue = version;
        property.OriginalValue = version;
        property.IsModified = false;
    }

    /// <summary>Null when the password is acceptable, else the reason.</summary>
    public static string? PasswordError(string password)
    {
        var bytes = Encoding.UTF8.GetByteCount(password);
        return bytes is < MinPasswordBytes or > MaxPasswordBytes
            ? $"Şifre {MinPasswordBytes}–{MaxPasswordBytes} bayt olmalı."
            : null;
    }

    /// <summary>The discount as stored (two decimals), or null outside 0–99.99 %.</summary>
    public static decimal? Discount(decimal value)
    {
        var rounded = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        return rounded is >= 0m and <= MaxDiscountPercent ? rounded : null;
    }

    /// <summary>
    /// A username made from the customer's name, else its code: Turkish letters folded to ASCII, lower case, words
    /// joined by '-' (company-form words and single letters left out), at most 24 characters; null under 3.
    /// </summary>
    public static string? UsernameFrom(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var folded = new StringBuilder(text.Length);
        foreach (var c in text.ToLower(Turkish).Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            var ascii = c == 'ı' ? 'i' : c;
            folded.Append(ascii is (>= 'a' and <= 'z') or (>= '0' and <= '9') ? ascii : ' ');
        }

        var name = new StringBuilder();
        foreach (var word in folded.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.Length == 1 || Filler.Contains(word)) continue;
            if (name.Length == 0)
            {
                name.Append(word.Length > SuggestionLength ? word[..SuggestionLength] : word);
                continue;
            }
            if (name.Length + 1 + word.Length > SuggestionLength) break;
            name.Append('-').Append(word);
        }
        return name.Length >= 3 ? name.ToString() : null;
    }

    /// <summary><paramref name="baseName"/>, or with the first number from 2 that makes it free; still at most 64 characters.</summary>
    public static string FirstFree(string baseName, IReadOnlySet<string> taken)
    {
        if (!taken.Contains(baseName)) return baseName;
        for (var n = 2; ; n++)
        {
            var suffix = n.ToString(CultureInfo.InvariantCulture);
            var stem = baseName.Length + suffix.Length > MaxUsernameLength ? baseName[..(MaxUsernameLength - suffix.Length)] : baseName;
            if (!taken.Contains(stem + suffix)) return stem + suffix;
        }
    }
}
