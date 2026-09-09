using System.Collections.ObjectModel;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;

namespace ErpBridge.Erp.Mikro.ChangeLog;

/// <summary>
/// The Mikro <b>V16</b> tables ErpBridge mirrors through the shadow-table change log.
///
/// <para>
/// <b>Provenance.</b> Table names, <c>TabloID</c> values, key columns and field
/// lists are transcribed from the Fora Mikro reference application's
/// <c>TabloHelperV16.GetMikroV16DefaultTablolar()</c>, which has run against production Mikro
/// installations for years. Validated column-by-column against a live
/// <c>MikroDB_V16_03</c> database.
/// </para>
///
/// <para>V16 dropped the <c>*_RECno</c> columns entirely and identifies rows by a <c>*_Guid</c> <c>uniqueidentifier</c>; the change log stores it in <c>KayitGuid</c> and joins on it natively.</para>
///
/// <para>
/// <b>TabloID values are load-bearing and must never be renumbered.</b> They are
/// written into shadow rows, persisted in the sync cursor, and echoed to the
/// mobile client — and they match the reference application's id space, so an
/// installation already carrying Fora triggers stays compatible.
/// </para>
/// </summary>
public sealed class MikroV16TrackedTableCatalog : IErpTrackedTableCatalog
{
    private static readonly ReadOnlyCollection<ErpTrackedTable> _tables = Build();

    /// <summary>Process-wide instance — the catalog is immutable.</summary>
    public static MikroV16TrackedTableCatalog Instance { get; } = new();

    /// <inheritdoc />
    public ErpType Erp => ErpType.Mikro;

    /// <inheritdoc />
    public IReadOnlyList<ErpTrackedTable> Tables => _tables;

    /// <inheritdoc />
    public ErpTrackedTable? Find(string tableKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableKey);
        return _tables.FirstOrDefault(t =>
            string.Equals(t.TableKey, tableKey, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public ErpTrackedTable? FindById(int tableId) =>
        _tables.FirstOrDefault(t => t.TableId == tableId);

    private static ReadOnlyCollection<ErpTrackedTable> Build()
    {
        var t = new List<ErpTrackedTable>();
        t.Add(new ErpTrackedTable(
            TableId: 13,
            TableKey: "STOKLAR",
            TableName: "STOKLAR",
            SchemaName: "dbo",
            PrimaryKeyField: "sto_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sto_Guid", "sto_kod", "sto_isim", "sto_kisa_ismi", "sto_yabanci_isim", "sto_sat_cari_kod",
                "sto_birim1_ad", "sto_birim2_ad", "sto_birim3_ad", "sto_birim4_ad", "sto_beden_kodu",
                "sto_renk_kodu", "sto_altgrup_kod", "sto_anagrup_kod", "sto_sektor_kodu", "sto_marka_kodu",
                "sto_uretici_kodu", "sto_reyon_kodu", "sto_model_kodu", "sto_kategori_kodu", "sto_yer_kod",
                "sto_birim1_katsayi", "sto_birim2_katsayi", "sto_birim3_katsayi", "sto_birim4_katsayi",
                "sto_standartmaliyet", "sto_birim1_agirlik", "sto_birim1_en", "sto_birim1_boy",
                "sto_birim1_yukseklik", "sto_birim1_dara", "sto_birim2_agirlik", "sto_birim2_en",
                "sto_birim2_boy", "sto_birim2_yukseklik", "sto_birim2_dara", "sto_birim3_agirlik",
                "sto_birim3_en", "sto_birim3_boy", "sto_birim3_yukseklik", "sto_birim3_dara",
                "sto_birim4_agirlik", "sto_birim4_en", "sto_birim4_boy", "sto_birim4_yukseklik",
                "sto_birim4_dara", "sto_detay_takip", "sto_bedenli_takip", "sto_perakende_vergi",
                "sto_toptan_vergi", "sto_cins", "sto_doviz_cinsi", "sto_webe_gonderilecek_fl",
                "sto_renkDetayli", "sto_ver_sip_birim", "sto_al_sip_birim", "sto_satis_dursun",
                "sto_siparis_dursun", "sto_malkabul_dursun"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 11,
            TableKey: "STOK_ANA_GRUPLARI",
            TableName: "STOK_ANA_GRUPLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "san_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "san_Guid", "san_kod", "san_isim"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 12,
            TableKey: "STOK_ALT_GRUPLARI",
            TableName: "STOK_ALT_GRUPLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "sta_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sta_Guid", "sta_kod", "sta_isim", "sta_ana_grup_kod"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 6,
            TableKey: "STOK_URETICILERI",
            TableName: "STOK_URETICILERI",
            SchemaName: "dbo",
            PrimaryKeyField: "urt_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "urt_Guid", "urt_kod", "urt_ismi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 7,
            TableKey: "STOK_REYONLARI",
            TableName: "STOK_REYONLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "ryn_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "ryn_Guid", "ryn_kod", "ryn_ismi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 19,
            TableKey: "STOK_MARKALARI",
            TableName: "STOK_MARKALARI",
            SchemaName: "dbo",
            PrimaryKeyField: "mrk_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "mrk_Guid", "mrk_kod", "mrk_ismi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 14,
            TableKey: "STOK_CARI_ISKONTO_TANIMLARI",
            TableName: "STOK_CARI_ISKONTO_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "isk_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "isk_Guid", "isk_stok_kod", "isk_cari_kod", "isk_isim", "isk_isk1_aciklama",
                "isk_isk2_aciklama", "isk_isk3_aciklama", "isk_isk4_aciklama", "isk_isk5_aciklama",
                "isk_isk6_aciklama", "isk_mas1_aciklama", "isk_mas2_aciklama", "isk_mas3_aciklama",
                "isk_mas4_aciklama", "isk_bedelsiz_referans_miktar", "isk_isk1_yuzde", "isk_isk2_yuzde",
                "isk_isk3_yuzde", "isk_isk4_yuzde", "isk_isk5_yuzde", "isk_isk6_yuzde", "isk_mas1_yuzde",
                "isk_mas2_yuzde", "isk_mas3_yuzde", "isk_mas4_yuzde", "isk_uygulama_odeme_plani",
                "isk_isk1_uygulama", "isk_isk2_uygulama", "isk_isk3_uygulama", "isk_isk4_uygulama",
                "isk_isk5_uygulama", "isk_isk6_uygulama", "isk_mas1_uygulama", "isk_mas2_uygulama",
                "isk_mas3_uygulama", "isk_mas4_uygulama"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 228,
            TableKey: "STOK_SATIS_FIYAT_LISTELERI",
            TableName: "STOK_SATIS_FIYAT_LISTELERI",
            SchemaName: "dbo",
            PrimaryKeyField: "sfiyat_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sfiyat_Guid", "sfiyat_stokkod", "sfiyat_iskontokod", "sfiyat_kampanyakod",
                "sfiyat_primyuzdesi", "sfiyat_fiyati", "sfiyat_listesirano", "sfiyat_deposirano",
                "sfiyat_odemeplan", "sfiyat_doviz"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 227,
            TableKey: "STOK_SATIS_FIYAT_LISTE_TANIMLARI",
            TableName: "STOK_SATIS_FIYAT_LISTE_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "sfl_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sfl_Guid", "sfl_aciklama", "sfl_fiyatformul", "sfl_odeplformul", "sfl_sabit_iskonto",
                "sfl_sabit_kampanya", "sfl_sabit_kur", "sfl_sirano", "sfl_fiyatuygulama", "sfl_odepluygulama",
                "sfl_sabit_odeme_plani", "sfl_kdvdahil", "sfl_yerineuygulanacakfiyat", "sfl_kurhesaplamasekli",
                "sfl_doviz_uygulama", "sfl_sabit_doviz", "sfl_iskonto_uygulama", "sfl_kampanya_uygulama",
                "sfl_kampanya_vade_gozardi", "sfl_kampanya_iskonto_gozardi", "sfl_otvdahil", "sfl_oivdahil",
                "sfl_ilktarih", "sfl_sontarih"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 106,
            TableKey: "STOK_PAKET_TANIMLARI",
            TableName: "STOK_PAKET_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "pak_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "pak_Guid", "pak_kod", "pak_stokkod", "pak_aciklama", "pak_ismi", "pak_miktar", "pak_fiyat",
                "pak_satirno", "pak_vergidahilfl", "pak_master_tip", "pak_detay_tip", "pak_doviz_cins",
                "pak_ve_veya"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 101,
            TableKey: "STOK_BEDEN_TANIMLARI",
            TableName: "STOK_BEDEN_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "bdn_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "bdn_Guid", "bdn_kodu", "bdn_ismi", "bdn_kirilim_1", "bdn_kirilim_2", "bdn_kirilim_3",
                "bdn_kirilim_4", "bdn_kirilim_5", "bdn_kirilim_6", "bdn_kirilim_7", "bdn_kirilim_8",
                "bdn_kirilim_9", "bdn_kirilim_10", "bdn_kirilim_11", "bdn_kirilim_12", "bdn_kirilim_13",
                "bdn_kirilim_14", "bdn_kirilim_15", "bdn_kirilim_16", "bdn_kirilim_17", "bdn_kirilim_18",
                "bdn_kirilim_19", "bdn_kirilim_20", "bdn_kirilim_21", "bdn_kirilim_22", "bdn_kirilim_23",
                "bdn_kirilim_24", "bdn_kirilim_25", "bdn_kirilim_26", "bdn_kirilim_27", "bdn_kirilim_28",
                "bdn_kirilim_29", "bdn_kirilim_30", "bdn_kirilim_31", "bdn_kirilim_32", "bdn_kirilim_33",
                "bdn_kirilim_34", "bdn_kirilim_35", "bdn_kirilim_36", "bdn_kirilim_37", "bdn_kirilim_38",
                "bdn_kirilim_39", "bdn_kirilim_40"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 157,
            TableKey: "STOK_RENK_TANIMLARI",
            TableName: "STOK_RENK_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "rnk_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "rnk_Guid", "rnk_kodu", "rnk_ismi", "rnk_kirilim_1", "rnk_kirilim_2", "rnk_kirilim_3",
                "rnk_kirilim_4", "rnk_kirilim_5", "rnk_kirilim_6", "rnk_kirilim_7", "rnk_kirilim_8",
                "rnk_kirilim_9", "rnk_kirilim_10", "rnk_kirilim_11", "rnk_kirilim_12", "rnk_kirilim_13",
                "rnk_kirilim_14", "rnk_kirilim_15", "rnk_kirilim_16", "rnk_kirilim_17", "rnk_kirilim_18",
                "rnk_kirilim_19", "rnk_kirilim_20", "rnk_kirilim_21", "rnk_kirilim_22", "rnk_kirilim_23",
                "rnk_kirilim_24", "rnk_kirilim_25", "rnk_kirilim_26", "rnk_kirilim_27", "rnk_kirilim_28",
                "rnk_kirilim_29", "rnk_kirilim_30", "rnk_kirilim_31", "rnk_kirilim_32", "rnk_kirilim_33",
                "rnk_kirilim_34", "rnk_kirilim_35", "rnk_kirilim_36", "rnk_kirilim_37", "rnk_kirilim_38",
                "rnk_kirilim_39", "rnk_kirilim_40", "rnk_kirilim_41", "rnk_kirilim_42", "rnk_kirilim_43",
                "rnk_kirilim_44", "rnk_kirilim_45", "rnk_kirilim_46", "rnk_kirilim_47", "rnk_kirilim_48",
                "rnk_kirilim_49", "rnk_kirilim_50", "rnk_kirilim_51", "rnk_kirilim_52", "rnk_kirilim_53",
                "rnk_kirilim_54", "rnk_kirilim_55", "rnk_kirilim_56", "rnk_kirilim_57", "rnk_kirilim_58",
                "rnk_kirilim_59", "rnk_kirilim_60"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 94,
            TableKey: "STOK_SERINO_TANIMLARI",
            TableName: "STOK_SERINO_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "chz_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "chz_Guid", "chz_serino", "chz_stok_kodu", "chz_grup_kodu", "chz_Tuktckodu", "chz_aciklama1",
                "chz_aciklama2", "chz_aciklama3", "chz_al_evr_seri", "chz_al_cari_kodu", "chz_st_evr_seri",
                "chz_st_cari_kodu", "chz_parca_garantisi", "chz_parca_serino", "chz_brut_fiati",
                "chz_al_fiati_ana", "chz_al_fiati_alt", "chz_al_fiati_orj", "chz_st_fiati_ana",
                "chz_st_fiati_alt", "chz_st_fiati_orj", "chz_al_evr_sira", "chz_st_evr_sira",
                "chz_GrnBasTarihi", "chz_GrnBitTarihi", "chz_al_tarih", "chz_st_tarih",
                "chz_parca_garanti_baslangic", "chz_parca_garanti_bitis"
            }));
        // Renumbered: the reference ships this table and STOK_SEKTORLERI on the
        // same TabloID, which would make each table's reader drop the other's rows.
        t.Add(new ErpTrackedTable(
            TableId: 99992,
            TableKey: "STOK_KATEGORILERI",
            TableName: "STOK_KATEGORILERI",
            SchemaName: "dbo",
            PrimaryKeyField: "ktg_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "ktg_Guid", "ktg_kod", "ktg_isim", "ktg_aciklama"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 8,
            TableKey: "STOK_SEKTORLERI",
            TableName: "STOK_SEKTORLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "sktr_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sktr_Guid", "sktr_kod", "sktr_ismi", "sktr_muhkodu"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 45,
            TableKey: "SATIS_SARTLARI",
            TableName: "SATIS_SARTLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "sat_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sat_Guid", "sat_stok_kod", "sat_cari_kod", "sat_brut_fiyat", "sat_det_isk_yuzde1",
                "sat_det_isk_yuzde2", "sat_det_isk_yuzde3", "sat_det_isk_yuzde4", "sat_det_isk_yuzde5",
                "sat_det_isk_yuzde6", "sat_det_isk_miktar1", "sat_det_isk_miktar2", "sat_det_isk_miktar3",
                "sat_det_isk_miktar4", "sat_det_isk_miktar5", "sat_det_isk_miktar6", "sat_det_mas_yuzde1",
                "sat_det_mas_yuzde2", "sat_det_mas_yuzde3", "sat_det_mas_yuzde4", "sat_det_mas_miktar1",
                "sat_det_mas_miktar2", "sat_det_mas_miktar3", "sat_det_mas_miktar4", "sat_odeme_plan",
                "sat_doviz_cinsi", "sat_depo_no", "sat_fiyat_liste_no", "sat_det_isk_uyg1", "sat_det_isk_uyg2",
                "sat_det_isk_uyg3", "sat_det_isk_uyg4", "sat_det_isk_uyg5", "sat_det_isk_uyg6",
                "sat_det_isk_durum1", "sat_det_isk_durum2", "sat_det_isk_durum3", "sat_det_isk_durum4",
                "sat_det_isk_durum5", "sat_det_isk_durum6", "sat_det_mas_uyg1", "sat_det_mas_uyg2",
                "sat_det_mas_uyg3", "sat_det_mas_uyg4", "sat_det_mas_durum1", "sat_det_mas_durum2",
                "sat_det_mas_durum3", "sat_det_mas_durum4", "sat_evrak_tarih", "sat_basla_tarih",
                "sat_bitis_tarih"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 44,
            TableKey: "SATINALMA_SARTLARI",
            TableName: "SATINALMA_SARTLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "sas_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sas_Guid", "sas_stok_kod", "sas_cari_kod", "sas_brut_fiyat", "sas_isk_yuzde1",
                "sas_isk_yuzde2", "sas_isk_yuzde3", "sas_isk_yuzde4", "sas_isk_yuzde5", "sas_isk_yuzde6",
                "sas_isk_miktar1", "sas_isk_miktar2", "sas_isk_miktar3", "sas_isk_miktar4", "sas_isk_miktar5",
                "sas_isk_miktar6", "sas_mas_yuzde1", "sas_mas_yuzde2", "sas_mas_yuzde3", "sas_mas_yuzde4",
                "sas_mas_miktar1", "sas_mas_miktar2", "sas_mas_miktar3", "sas_mas_miktar4", "sas_odeme_plan",
                "sas_doviz_cinsi", "sas_depo_no", "sas_isk_uyg1", "sas_isk_uyg2", "sas_isk_uyg3",
                "sas_isk_uyg4", "sas_isk_uyg5", "sas_isk_uyg6", "sas_isk_durum1", "sas_isk_durum2",
                "sas_isk_durum3", "sas_isk_durum4", "sas_isk_durum5", "sas_isk_durum6", "sas_mas_uyg1",
                "sas_mas_uyg2", "sas_mas_uyg3", "sas_mas_uyg4", "sas_mas_durum1", "sas_mas_durum2",
                "sas_mas_durum3", "sas_mas_durum4", "sas_evrak_tarih", "sas_basla_tarih", "sas_bitis_tarih"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 15,
            TableKey: "BARKOD_TANIMLARI",
            TableName: "BARKOD_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "bar_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "bar_Guid", "bar_kodu", "bar_stokkodu", "bar_partikodu", "bar_serino_veya_bagkodu",
                "bar_lotno", "bar_barkodtipi", "bar_icerigi", "bar_birimpntr", "bar_master", "bar_bedenpntr",
                "bar_renkpntr", "bar_baglantitipi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 31,
            TableKey: "CARI_HESAPLAR",
            TableName: "CARI_HESAPLAR",
            SchemaName: "dbo",
            PrimaryKeyField: "cari_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "cari_Guid", "cari_kod", "cari_unvan1", "cari_unvan2", "cari_muh_kod", "cari_muh_kod1",
                "cari_muh_kod2", "cari_vdaire_adi", "cari_vdaire_no", "cari_Ana_cari_kodu", "cari_bolge_kodu",
                "cari_grup_kodu", "cari_temsilci_kodu", "cari_sektor_kodu", "cari_satis_isk_kod",
                "cari_special1", "cari_special2", "cari_special3", "cari_sicil_no", "cari_VergiKimlikNo",
                "cari_banka_hesapno1", "cari_CepTel", "cari_EMail", "cari_wwwadresi", "cari_vade_fark_yuz",
                "cari_vade_fark_yuz1", "cari_vade_fark_yuz2", "cari_doviz_cinsi", "cari_doviz_cinsi1",
                "cari_doviz_cinsi2", "cari_odeme_gunu", "cari_hareket_tipi", "cari_odemeplan_no",
                "cari_satis_fk", "cari_KurHesapSekli", "cari_odeme_cinsi", "cari_cari_kilitli_flg",
                "cari_fatura_adres_no", "cari_sevk_adres_no", "cari_VarsayilanGirisDepo",
                "cari_VarsayilanCikisDepo", "cari_DBCno", "cari_efatura_fl", "cari_def_efatura_cinsi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 32,
            TableKey: "CARI_HESAP_ADRESLERI",
            TableName: "CARI_HESAP_ADRESLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "adr_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "adr_Guid", "adr_cari_kod", "adr_cadde", "adr_sokak", "adr_posta_kodu", "adr_ilce", "adr_il",
                "adr_ulke", "adr_tel_ulke_kodu", "adr_tel_bolge_kodu", "adr_tel_no1", "adr_tel_no2",
                "adr_tel_faxno", "adr_tel_modem", "adr_yon_kodu", "adr_temsilci_kodu", "adr_ozel_not",
                "adr_ziyaretgunu", "adr_gps_enlem", "adr_gps_boylam", "adr_adres_no", "adr_uzaklik_kodu",
                "adr_ziyaretperyodu", "adr_ziyarethaftasi", "adr_ziygunu2_1", "adr_ziygunu2_2",
                "adr_ziygunu2_3", "adr_ziygunu2_4", "adr_ziygunu2_5", "adr_ziygunu2_6", "adr_ziygunu2_7"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 33,
            TableKey: "CARI_HESAP_YETKILILERI",
            TableName: "CARI_HESAP_YETKILILERI",
            SchemaName: "dbo",
            PrimaryKeyField: "mye_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "mye_Guid", "mye_cari_kod", "mye_isim", "mye_soyisim", "mye_es_isim", "mye_dahili_telno",
                "mye_email_adres", "mye_cep_telno", "mye_tc_kimlikno", "mye_vergi_dairesi",
                "mye_vergi_kimlikno", "mye_dogum_yeri", "mye_ev_cadde", "mye_ev_sokak", "mye_ev_posta_kodu",
                "mye_ev_ilce", "mye_ev_il", "mye_ev_ulke", "mye_is_telno", "mye_ev_telno", "mye_adres_no",
                "mye_unvan", "mye_hitap", "mye_hisse", "mye_tahsil", "mye_dogum_tarihi", "mye_evlilik_tarihi",
                "mye_es_dogum_tarihi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 201,
            TableKey: "CARI_HESAP_TEMINATLARI",
            TableName: "CARI_HESAP_TEMINATLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "ct_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "ct_Guid", "ct_carikodu", "ct_Aciklama_no", "ct_srmrkkodu", "ct_tutari", "ct_iptal",
                "ct_DovizCinsi", "ct_GecerliFirma", "ct_vade"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 104,
            TableKey: "CARI_PERSONEL_TANIMLARI",
            TableName: "CARI_PERSONEL_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "cari_per_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "cari_per_Guid", "cari_per_kod", "cari_per_adi", "cari_per_soyadi", "cari_per_kasiyerkodu",
                "cari_per_kasiyersifresi", "cari_per_kasiyerAmiri", "cari_per_cepno", "cari_per_mail",
                "cari_per_kasiyerfirmaid", "cari_per_tip", "cari_per_doviz_cinsi", "cari_per_userno",
                "cari_per_depono"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 39,
            TableKey: "CARI_HESAP_BOLGELERI",
            TableName: "CARI_HESAP_BOLGELERI",
            SchemaName: "dbo",
            PrimaryKeyField: "bol_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "bol_Guid", "bol_kod", "bol_ismi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 9,
            TableKey: "CARI_HESAP_GRUPLARI",
            TableName: "CARI_HESAP_GRUPLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "crg_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "crg_Guid", "crg_kod", "crg_isim", "crg_muhasebe_kodu"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 3,
            TableKey: "SORUMLULUK_MERKEZLERI",
            TableName: "SORUMLULUK_MERKEZLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "som_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "som_Guid", "som_kod", "som_isim"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 176,
            TableKey: "PROJELER",
            TableName: "PROJELER",
            SchemaName: "dbo",
            PrimaryKeyField: "pro_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "pro_Guid", "pro_kodu", "pro_adi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 111,
            TableKey: "DEPOLAR",
            TableName: "DEPOLAR",
            SchemaName: "dbo",
            PrimaryKeyField: "dep_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "dep_Guid", "dep_adi", "dep_grup_kodu", "dep_muh_kodu", "dep_sor_mer_kodu", "dep_proje_kodu",
                "dep_cadde", "dep_sokak", "dep_posta_Kodu", "dep_Ilce", "dep_Il", "dep_Ulke",
                "dep_yetkili_email", "dep_dizin_adi", "dep_tel_ulke_kodu", "dep_tel_bolge_kodu", "dep_tel_no1",
                "dep_tel_no2", "dep_tel_faxno", "dep_tel_modem", "dep_barkod_yazici_yolu",
                "dep_fason_sor_mer_kodu", "dep_kamyon_kasa_hacmi", "dep_kamyon_istiab_haddi",
                "dep_satis_alani", "dep_sergi_alani", "dep_otopark_alani", "dep_gps_enlem", "dep_gps_boylam",
                "dep_alani", "dep_rafhacmi", "dep_firmano", "dep_subeno", "dep_no", "dep_tipi",
                "dep_DepoSevkOtoFiyat", "dep_hareket_tipi", "dep_DepoSevkUygFiyat", "dep_otopark_kapasite",
                "dep_kasa_sayisi", "dep_envanter_harici_fl", "dep_detay_takibi", "dep_EksiyeDusurenStkHar",
                "dep_BagliOrtakliklaraSatisUygFiyat"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 72,
            TableKey: "ODEME_PLANLARI",
            TableName: "ODEME_PLANLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "odp_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "odp_Guid", "odp_kodu", "odp_adi", "odp_aratop", "odp_masraf", "odp_vergi", "odp_no",
                "odp_ortgun"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 53,
            TableKey: "KASALAR",
            TableName: "KASALAR",
            SchemaName: "dbo",
            PrimaryKeyField: "kas_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "kas_Guid", "kas_kod", "kas_isim", "kas_muh_kod", "kas_bankakodu", "kas_nakakincelenmesi",
                "kas_ufrs_muh_kod", "kas_tip", "kas_firma_no", "kas_doviz_cinsi"
            }));
        // Lives in the Mikro master database, not the firm DB.
        t.Add(new ErpTrackedTable(
            TableId: 1007,
            TableKey: "DOVIZ_KURLARI",
            TableName: "DOVIZ_KURLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "dov_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "dov_Guid", "dov_fiyat1", "dov_fiyat2", "dov_fiyat3", "dov_fiyat4", "dov_no", "dov_tarih"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 54,
            TableKey: "ODEME_EMIRLERI",
            TableName: "ODEME_EMIRLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "sck_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sck_Guid", "sck_sahip_cari_kodu", "sck_refno", "sck_bankano", "sck_srmmrk", "sck_borclu",
                "sck_vdaire_no", "sck_banka_adres1", "sck_sube_adres2", "sck_hesapno_sehir", "sck_no",
                "Sck_TCMB_Banka_kodu", "Sck_TCMB_Sube_kodu", "Sck_TCMB_il_kodu", "sck_ilk_evrak_seri",
                "sck_nerede_cari_kodu", "sck_tutar", "sck_doviz_kur", "sck_doviz", "sck_sonpoz",
                "sck_sahip_cari_cins", "sck_tip", "sck_firmano", "sck_subeno", "sck_sahip_cari_grupno",
                "sck_iptal", "sck_ilk_evrak_sira_no", "sck_ilk_evrak_satir_no", "sck_nerede_cari_cins",
                "sck_imza", "sck_vade"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 107,
            TableKey: "FIRMALAR",
            TableName: "FIRMALAR",
            SchemaName: "dbo",
            PrimaryKeyField: "fir_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "fir_Guid", "fir_unvan", "fir_sirano"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 112,
            TableKey: "SUBELER",
            TableName: "SUBELER",
            SchemaName: "dbo",
            PrimaryKeyField: "Sube_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "Sube_Guid", "Sube_adi", "Sube_no"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 52,
            TableKey: "BANKALAR",
            TableName: "BANKALAR",
            SchemaName: "dbo",
            PrimaryKeyField: "ban_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "ban_Guid", "ban_ismi", "ban_sube", "ban_kod", "ban_firma_no", "ban_doviz_cinsi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 62,
            TableKey: "MASRAF_HESAPLARI",
            TableName: "MASRAF_HESAPLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "his_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "his_Guid", "his_kod", "his_isim", "his_yabanci_isim", "his_tipkod", "his_sinifkod",
                "his_grupkod", "his_oivtutar", "his_dovcinsi", "his_oivuygulama"
            }));
        // Lives in the Mikro master database, not the firm DB.
        t.Add(new ErpTrackedTable(
            TableId: 127,
            TableKey: "YEREL_BANKA_KODLARI",
            TableName: "YEREL_BANKA_KODLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "bankkod_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "bankkod_Guid", "bankkod_kod", "bankkod_subekodu", "bankkod_ilkodu", "bankkod_bankadi",
                "bankkod_subeadi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 99,
            TableKey: "TESLIM_TURLERI",
            TableName: "TESLIM_TURLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "tslt_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "tslt_Guid", "tslt_kod", "tslt_ismi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 86,
            TableKey: "DEPOLAR_ARASI_SIPARISLER",
            TableName: "DEPOLAR_ARASI_SIPARISLER",
            SchemaName: "dbo",
            PrimaryKeyField: "ssip_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "ssip_Guid", "ssip_stal_uid", "ssip_special1", "ssip_special2", "ssip_special3",
                "ssip_evrakno_seri", "ssip_belgeno", "ssip_aciklama", "ssip_projekodu", "ssip_paket_kod",
                "ssip_kapatmanedenkod", "ssip_stok_kod", "ssip_tutar", "ssip_miktar", "ssip_teslim_miktar",
                "ssip_b_fiyat", "ssip_kapat_fl", "ssip_girdepo", "ssip_cikdepo", "ssip_SpecRECno",
                "ssip_fileid", "ssip_checksum", "ssip_create_user", "ssip_lastup_user", "ssip_firmano",
                "ssip_subeno", "ssip_evrakno_sira", "ssip_satirno", "ssip_birim_pntr", "ssip_iptal",
                "ssip_hidden", "ssip_kilitli", "ssip_degisti", "ssip_fiyat_liste_no", "ssip_DBCno",
                "ssip_create_date", "ssip_tarih", "ssip_teslim_tarih", "ssip_belge_tarih"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 66,
            TableKey: "EVRAK_ACIKLAMALARI",
            TableName: "EVRAK_ACIKLAMALARI",
            SchemaName: "dbo",
            PrimaryKeyField: "egk_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "egk_Guid", "egk_special1", "egk_special2", "egk_special3", "egk_evr_seri", "egk_evracik1",
                "egk_evracik2", "egk_evracik3", "egk_evracik4", "egk_evracik5", "egk_evracik6", "egk_evracik7",
                "egk_evracik8", "egk_evracik9", "egk_evracik10", "egk_dosyano", "egk_hareket_tip",
                "egk_evr_tip", "egk_evr_sira"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 122,
            TableKey: "IHRACAT_DOSYALARI",
            TableName: "IHRACAT_DOSYALARI",
            SchemaName: "dbo",
            PrimaryKeyField: "ihr_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "ihr_Guid", "ihr_kodu", "ihr_ismi", "ihr_Satici", "ihr_SpecRecNo", "ihr_firmano", "ihr_subeno",
                "ihr_TeslimSekli", "ihr_OdemeSekli", "ihr_carigrupno", "ihr_DovizCinsi", "ihr_create_date",
                "ihr_lastup_date", "ihr_GCB_Tarihi", "ihr_Intac_Tarihi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 299,
            TableKey: "KAPAMA_NEDENLERI_TANIMLARI",
            TableName: "KAPAMA_NEDENLERI_TANIMLARI",
            SchemaName: "dbo",
            PrimaryKeyField: "Kpm_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "Kpm_Guid", "Kpm_kod", "Kpm_ismi", "Kpm_aciklama"
            }));
        // Lives in the Mikro master database, not the firm DB.
        t.Add(new ErpTrackedTable(
            TableId: 1020,
            TableKey: "KUR_ISIMLERI",
            TableName: "KUR_ISIMLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "Kur_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "Kur_Guid", "Kur_Tip", "Kur_sembol", "Kur_adi", "Kur_orjAdi", "Kur_kusurat_isim",
                "Kur_kusurat_sembol", "Kur_No", "Kur_decimal"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 98,
            TableKey: "CIHAZ_HAREKETLERI",
            TableName: "CIHAZ_HAREKETLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "ChHar_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "ChHar_Guid", "ChHar_special1", "ChHar_special2", "ChHar_special3", "ChHar_SeriNo",
                "ChHar_StokKodu", "ChHar_Spec_Rec_no", "ChHar_master_tablo", "ChHar_create_date",
                "ChHar_lastup_date"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 51,
            TableKey: "CARI_HESAP_HAREKETLERI",
            TableName: "CARI_HESAP_HAREKETLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "cha_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "cha_Guid", "cha_special1", "cha_special2", "cha_special3", "cha_aciklama", "cha_projekodu",
                "cha_satici_kodu", "cha_srmrkkodu", "cha_trefno", "cha_ciro_cari_kodu", "cha_evrakno_seri",
                "cha_belge_no", "cha_kod", "cha_kasa_hizkod", "cha_EXIMkodu", "cha_ft_iskonto1",
                "cha_ft_iskonto2", "cha_ft_iskonto3", "cha_ft_iskonto4", "cha_ft_iskonto5", "cha_ft_iskonto6",
                "cha_meblag", "cha_d_kur", "cha_miktari", "cha_aratoplam", "cha_ft_masraf1", "cha_ft_masraf2",
                "cha_ft_masraf3", "cha_ft_masraf4", "cha_otvtutari", "cha_vergi1", "cha_vergi2", "cha_vergi3",
                "cha_vergi4", "cha_vergi5", "cha_vergi6", "cha_vergi7", "cha_vergi8", "cha_vergi9",
                "cha_vergi10", "cha_altd_kur", "cha_karsid_kur", "cha_tpoz", "cha_vergipntr",
                "cha_kasa_hizmet", "cha_firmano", "cha_subeno", "cha_tip", "cha_cinsi", "cha_normal_Iade",
                "cha_evrak_tip", "cha_satir_no", "cha_evrakno_sira", "cha_cari_cins", "cha_vade", "cha_grupno",
                "cha_d_cins", "cha_ticaret_turu", "cha_tarihi", "cha_belge_tarih", "cha_lastup_date"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 16,
            TableKey: "STOK_HAREKETLERI",
            TableName: "STOK_HAREKETLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "sth_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sth_Guid", "sth_sip_uid", "sth_fat_uid", "sth_cari_kodu", "sth_stok_kod", "sth_evrakno_seri",
                "sth_plasiyer_kodu", "sth_cari_srm_merkezi", "sth_proje_kodu", "sth_aciklama", "sth_tutar",
                "sth_alt_doviz_kuru", "sth_vergi", "sth_har_doviz_kuru", "sth_iskonto1", "sth_iskonto2",
                "sth_iskonto3", "sth_iskonto4", "sth_iskonto5", "sth_iskonto6", "sth_masraf1", "sth_masraf2",
                "sth_masraf3", "sth_masraf4", "sth_masraf_vergi", "sth_miktar", "sth_miktar2",
                "sth_stok_doviz_kuru", "sth_DBCno", "sth_cari_grup_no", "sth_firmano", "sth_subeno",
                "sth_normal_iade", "sth_birim_pntr", "sth_fiyat_liste_no", "sth_adres_no", "sth_tip",
                "sth_giris_depo_no", "sth_cikis_depo_no", "sth_evrakno_sira", "sth_cari_cinsi", "sth_evraktip",
                "sth_satirno", "sth_cins", "sth_vergi_pntr", "sth_har_doviz_cinsi", "sth_stok_doviz_cinsi",
                "sth_nakliyedeposu", "sth_nakliyedurumu", "sth_isk_mas1", "sth_isk_mas2", "sth_isk_mas3",
                "sth_isk_mas4", "sth_isk_mas5", "sth_isk_mas6", "sth_tarih", "sth_belge_tarih",
                "sth_lastup_date"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 21,
            TableKey: "SIPARISLER",
            TableName: "SIPARISLER",
            SchemaName: "dbo",
            PrimaryKeyField: "sip_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "sip_Guid", "sip_prosip_uid", "sip_stal_uid", "sip_teklif_uid", "sip_Rez_uid",
                "sip_yetkili_uid", "sip_musteri_kod", "sip_special1", "sip_special2", "sip_special3",
                "sip_evrakno_seri", "sip_belgeno", "sip_satici_kod", "sip_aciklama", "sip_aciklama2",
                "sip_cari_sormerk", "sip_stok_sormerk", "sip_teslimturu", "sip_Exp_Imp_Kodu", "sip_parti_kodu",
                "sip_projekodu", "sip_paket_kod", "sip_kapatmanedenkod", "sip_stok_kod", "sip_tutar",
                "sip_iskonto_1", "sip_iskonto_2", "sip_iskonto_3", "sip_iskonto_4", "sip_iskonto_5",
                "sip_iskonto_6", "sip_masraf_1", "sip_masraf_2", "sip_masraf_3", "sip_masraf_4", "sip_vergi",
                "sip_masvergi", "sip_doviz_kuru", "sip_miktar", "sip_teslim_miktar", "sip_b_fiyat",
                "sip_alt_doviz_kuru", "sip_kar_orani", "sip_planlananmiktar", "sip_Otv_Vergi", "sip_otvtutari",
                "sip_DBCno", "sip_kapat_fl", "sip_tip", "sip_depono", "sip_SpecRECno", "sip_fileid",
                "sip_checksum", "sip_create_user", "sip_lastup_user", "sip_firmano", "sip_subeno", "sip_cins",
                "sip_evrakno_sira", "sip_satirno", "sip_birim_pntr", "sip_vergi_pntr", "sip_masvergi_pntr",
                "sip_opno", "sip_OnaylayanKulNo", "sip_cari_grupno", "sip_doviz_cinsi", "sip_adresno",
                "sip_iskonto1", "sip_iskonto2", "sip_iskonto3", "sip_iskonto4", "sip_iskonto5", "sip_iskonto6",
                "sip_masraf1", "sip_masraf2", "sip_masraf3", "sip_masraf4", "sip_durumu", "sip_lot_no",
                "sip_fiyat_liste_no", "sip_Otv_Pntr", "sip_OtvVergisiz_Fl", "sip_harekettipi", "sip_iptal",
                "sip_hidden", "sip_kilitli", "sip_degisti", "sip_vergisiz_fl", "sip_promosyon_fl",
                "sip_cagrilabilir_fl", "sip_isk1", "sip_isk2", "sip_isk3", "sip_isk4", "sip_isk5", "sip_isk6",
                "sip_mas1", "sip_mas2", "sip_mas3", "sip_mas4", "sip_create_date", "sip_tarih",
                "sip_teslim_tarih", "sip_belge_tarih"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 22,
            TableKey: "PROFORMA_SIPARISLER",
            TableName: "PROFORMA_SIPARISLER",
            SchemaName: "dbo",
            PrimaryKeyField: "pro_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "pro_Guid", "pro_sip_uid", "pro_stal_uid", "pro_teklif_uid", "pro_Rez_uid", "pro_yetkili_uid",
                "pro_mustkodu", "pro_special1", "pro_special2", "pro_special3", "pro_evrakno_seri",
                "pro_belge_no", "pro_saticikodu", "pro_aciklama", "pro_aciklama2", "pro_cari_sormerk",
                "pro_stok_sormerk", "pro_teslimturu", "pro_Exp_Imp_Kodu", "pro_parti_kodu", "pro_projekodu",
                "pro_paket_kod", "pro_kapatmanedenkod", "pro_stokkodu", "pro_tutari", "pro_iskonto1",
                "pro_iskonto2", "pro_iskonto3", "pro_iskonto4", "pro_iskonto5", "pro_iskonto6", "pro_masraf1",
                "pro_masraf2", "pro_masraf3", "pro_masraf4", "pro_vergi", "pro_masrafvergi", "pro_dovizkuru",
                "pro_miktar", "pro_tesmiktari", "pro_bfiyati", "pro_altdovizkuru", "pro_karoani",
                "pro_planlananmiktar", "pro_Otv_Vergi", "pro_otvtutari", "pro_DBCno", "pro_kapat", "pro_tipi",
                "pro_depono", "pro_SpecRecNo", "pro_fileid", "pro_checksum", "pro_create_user",
                "pro_lastup_user", "pro_firmano", "pro_subeno", "pro_cinsi", "pro_evrakno_sira", "pro_satirno",
                "pro_birim_pntr", "pro_vergipntr", "pro_masrafvergipntr", "pro_opno", "pro_onaylayanKul_no",
                "pro_cari_grupno", "pro_dovizcinsi", "pro_adresno", "pro_isk_mas_1", "pro_isk_mas_2",
                "pro_isk_mas_3", "pro_isk_mas_4", "pro_isk_mas_5", "pro_isk_mas_6", "pro_isk_mas_7",
                "pro_isk_mas_8", "pro_isk_mas_9", "pro_isk_mas_10", "pro_durumu", "pro_lot_no",
                "pro_fiyat_liste_no", "pro_Otv_Pntr", "pro_OtvVergisiz_Fl", "pro_harekettipi", "pro_iptal",
                "pro_hidden", "pro_kilitli", "pro_degisti", "pro_vergisiz", "pro_promosyon_fl",
                "pro_cagrilabilir_fl", "pro_sat_isk_mas1", "pro_sat_isk_mas2", "pro_sat_isk_mas3",
                "pro_sat_isk_mas4", "pro_sat_isk_mas5", "pro_sat_isk_mas6", "pro_sat_isk_mas7",
                "pro_sat_isk_mas8", "pro_sat_isk_mas9", "pro_sat_isk_mas10", "pro_create_date", "pro_tarihi",
                "pro_testarihi", "pro_belge_tarihi"
            }));
        t.Add(new ErpTrackedTable(
            TableId: 113,
            TableKey: "BEDEN_HAREKETLERI",
            TableName: "BEDEN_HAREKETLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "BdnHar_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "BdnHar_Guid", "BdnHar_Har_uid", "BdnHar_special1", "BdnHar_special2", "BdnHar_special3",
                "BdnHar_HarGor", "BdnHar_KnsIsGor", "BdnHar_KnsFat", "BdnHar_TesMik",
                "BdnHar_rezervasyon_miktari", "BdnHar_rezerveden_teslim_edilen", "BdnHar_DBCno",
                "BdnHar_Spec_Rec_no", "BdnHar_iptal", "BdnHar_fileid", "BdnHar_hidden", "BdnHar_kilitli",
                "BdnHar_degisti", "BdnHar_checksum", "BdnHar_create_user", "BdnHar_lastup_user", "BdnHar_Tipi",
                "BdnHar_BedenNo", "BdnHar_create_date", "BdnHar_lastup_date"
            }));
        // Created by the integration installer; not shipped by Mikro.
        t.Add(new ErpTrackedTable(
            TableId: 99990,
            TableKey: "_ZIYARET_HAREKETLERI",
            TableName: "_ZIYARET_HAREKETLERI",
            SchemaName: "dbo",
            PrimaryKeyField: "zyrt_Guid",
            KeyKind: ErpRowKeyKind.Guid,
            Fields: new[]
            {
                "zyrt_Guid", "zyrt_Temsilci_Kodu", "zyrt_Cari_Kodu", "zyrt_Cari_Adres_No", "zyrt_Tarihi"
            }));

        return new ReadOnlyCollection<ErpTrackedTable>(t);
    }
}
