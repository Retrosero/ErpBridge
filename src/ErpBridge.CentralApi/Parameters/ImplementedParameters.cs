namespace ErpBridge.CentralApi.Parameters;

/// <summary>
/// The parameters Sipariş Cepte actually honours today (P4f, D16).
///
/// The panel shows everything else with an "inert in this version" badge, so that an operator who
/// changes a setting and sees nothing happen learns why instead of filing a bug. That badge is
/// only useful if this list is true, which makes the list a promise rather than documentation.
///
/// **Kept by hand, on purpose.** The catalogue is generated from Fora's sources and knows nothing
/// about what our phone does; nothing can derive this. Whoever wires a parameter into the app adds
/// it here in the same change — and a parameter listed here that the app does not really honour is
/// worse than one missing, because the badge then lies in the direction people trust.
/// </summary>
public static class ImplementedParameters
{
    /// <summary>The phone's catalogue set. Everything below belongs to it.</summary>
    private const string MobileSet = "MobilKullanici";

    /// <summary>
    /// Mikro parameter ids of <see cref="MobileSet"/> that this release of Sipariş Cepte reads.
    ///
    /// Grouped the way the app groups them, so the next person can find the code that honours one.
    /// </summary>
    private static readonly int[] MobileIds =
    [
        // P4b — main menu visibility (Goster_AnaMenu_*), via MainMenuParameters.
        // Only the ones actually mapped onto a screen this app has; Fora's other menu switches
        // (Güne başla, ziyaret, konsinye irsaliyeleri) have nothing here to hide.
        80,    // Goster_AnaMenu_Stok        → katalog + stoklar
        81,    // Goster_AnaMenu_Cari        → müşteriler
        82,    // Goster_AnaMenu_Alinan_Siparis → satış
        88,    // Goster_AnaMenu_Alis_Faturasi  → alış
        89,    // Goster_AnaMenu_Tahsilat
        90,    // Goster_AnaMenu_Masraf     → giderler
        91,    // Goster_AnaMenu_Sayim_Sonuclari_Giris_Fisi → sayım
        92,    // Goster_AnaMenu_Depolar_Arasi_Sevk → depolar
        94,    // Goster_AnaMenu_Analizler  → raporlar
        3008,  // Goster_AnaMenu_Siparis_Onaylama → onay merkezi
        3022,  // Goster_AnaMenu_Tediye     → ödeme

        // P4c — permissions (Hak*), via PermissionParameters. Only the two wired so far; the
        // other 69 are named in the app but not yet honoured, so they stay off this list.
        186,   // HakGorme_CariBakiye → kimlik kartındaki bakiye
        198,   // HakGorme_CariEkstre → Hareketler sekmesi

        // P4d — the warehouse a sale leaves from, on the outgoing document.
        58,    // DefaultKaynakDepoNo
    ];

    /// <summary>Whether this release of the mobile app honours the parameter.</summary>
    public static bool Honours(string catalogMethod, int parametreId) =>
        catalogMethod == MobileSet && Array.IndexOf(MobileIds, parametreId) >= 0;

    /// <summary>How many parameters are honoured; used by tests and diagnostics.</summary>
    public static int Count => MobileIds.Length;
}
