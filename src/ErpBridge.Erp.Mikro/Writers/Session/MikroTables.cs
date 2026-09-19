namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>A Mikro document table: name, column prefix and the <c>*_fileid</c> Mikro stamps on its rows.</summary>
public sealed record MikroTable(string Name, string Prefix, short FileId)
{
    /// <summary>Column name with the table's prefix, e.g. <c>cha_kod</c>.</summary>
    public string Column(string suffix) => $"{Prefix}_{suffix}";
}

/// <summary>The tables phone documents are written into (reference §1 "Dosya no").</summary>
public static class MikroTables
{
    public static readonly MikroTable CariHareket = new("CARI_HESAP_HAREKETLERI", "cha", MikroCodes.FileId.CariHesapHareketleri);
    public static readonly MikroTable StokHareket = new("STOK_HAREKETLERI", "sth", MikroCodes.FileId.StokHareketleri);
    public static readonly MikroTable OdemeEmri = new("ODEME_EMIRLERI", "sck", MikroCodes.FileId.OdemeEmirleri);
    public static readonly MikroTable EvrakAciklama = new("EVRAK_ACIKLAMALARI", "egk", MikroCodes.FileId.EvrakAciklamalari);

    /// <summary>Sayım fişi satırları (referans §14). Stok hareketi değildir; kendi tablosudur.</summary>
    public static readonly MikroTable SayimSonuclari = new("SAYIM_SONUCLARI", "sym", MikroCodes.FileId.SayimSonuclari);

    /// <summary>The single order row in the company data carries <c>sip_fileid=0</c>; the order writer (Y3d) confirms it against Fora.</summary>
    public static readonly MikroTable Siparis = new("SIPARISLER", "sip", 0);
}
