using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace ErpBridge.Shared;

/// <summary>
/// Immutable descriptor of one Mikro table that ErpBridge mirrors through the
/// trigger-based change-tracking mechanism. The set of fields here is a strict
/// superset of what the bootstrap reader needs — the three-way pull path uses
/// only the subset supplied at call time so the wire payload stays small.
/// </summary>
/// <param name="TabloID">Stable Mikro-side numeric identifier (matches the reference app's TabloHelper ID space).</param>
/// <param name="TabloAdi">SQL table name (e.g. <c>STOKLAR</c>, <c>CARI_HESAPLAR</c>).</param>
/// <param name="RecnoField">Primary-key column for the row (e.g. <c>sto_RECno</c>). The trigger shadow row references the row by this column.</param>
/// <param name="Fields">Full list of columns the adapter is allowed to read. Any other column requested by the caller is rejected to keep the SQL surface safe.</param>
/// <param name="RequiresSoftDeleteFilter">When true, the row is treated as deleted by the consumer when <c>sto_iptal/cari_iptal/...</c> equals 1; the trigger still fires for that update so the change propagates as a "changed" record.</param>
public sealed record TrackedTableSchema(
    int TabloID,
    string TabloAdi,
    string RecnoField,
    IReadOnlyList<string> Fields,
    bool RequiresSoftDeleteFilter = false)
{
    /// <summary>
    /// Suggested default field set when the caller does not specify one. The list
    /// is the same as <see cref="Fields"/> — the caller may shrink it but should
    /// never extend it past <see cref="Fields"/> because every field must be
    /// whitelisted for SQL safety.
    /// </summary>
    public IReadOnlyList<string> DefaultFields => Fields;

    /// <summary>
    /// Build the SQL fragment that selects the supplied <paramref name="fields"/>
    /// with the canonical <c>T.</c> table alias used by the change-tracking
    /// joins. The field list is validated against <see cref="Fields"/> to keep
    /// callers from accidentally reading sensitive columns.
    /// </summary>
    public string BuildSelectList(IReadOnlyList<string> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        var allow = new HashSet<string>(Fields, System.StringComparer.OrdinalIgnoreCase);
        foreach (var f in fields)
        {
            if (!allow.Contains(f))
            {
                throw new ArgumentException(
                    $"Field '{f}' is not part of the {TabloAdi} allowlist. " +
                    "Update TrackedTableSchema to whitelist a new field before reading it.",
                    nameof(fields));
            }
        }
        return string.Join(", ", fields.Select(f => "T." + f));
    }
}

/// <summary>
/// Static catalogue of the 49 Mikro tables that ErpBridge watches through
/// the <c>_ERPB_SENKRONIZASYON</c> trigger table. Table IDs match the reference
/// app's <c>TabloHelper.GetMikroV14DefaultTablolar()</c> — keeping them aligned
/// is mandatory for any downstream tooling that uses those IDs as opaque
/// tokens.
///
/// The list is intentionally small enough to be read end-to-end and to make a
/// "you changed table X" code review trivial. Soft-delete filter is enabled on
/// master-data tables that carry a <c>_*_iptal</c> bit so the bootstrap reader
/// can mirror the reference app's soft-delete convention.
/// </summary>
public static class TrackedTableCatalog
{
    private static readonly ReadOnlyCollection<TrackedTableSchema> _all = BuildAll();

    /// <summary>All 49 tracked tables. The list is stable; additions require a coordinated change to the trigger installer and the catalog.</summary>
    public static IReadOnlyList<TrackedTableSchema> All => _all;

    /// <summary>Look up a table by its Mikro-side numeric ID. Returns <c>null</c> when the ID is not part of the catalog.</summary>
    public static TrackedTableSchema? FindByTabloID(int tabloID) =>
        _all.FirstOrDefault(t => t.TabloID == tabloID);

    /// <summary>Look up a table by its SQL name (case-insensitive). Returns <c>null</c> when the name is not part of the catalog.</summary>
    public static TrackedTableSchema? FindByTabloAdi(string tabloAdi)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabloAdi);
        return _all.FirstOrDefault(t => string.Equals(t.TabloAdi, tabloAdi, System.StringComparison.OrdinalIgnoreCase));
    }

    private static ReadOnlyCollection<TrackedTableSchema> BuildAll() =>
        new(BuildDefault49().ToList());

    /// <summary>
    /// 49 default tables, in the same order as the reference app's
    /// <c>GetMikroV14DefaultTablolar()</c>. The order is significant only for
    /// diagnostics — every iteration is over the full set regardless.
    /// </summary>
    private static IEnumerable<TrackedTableSchema> BuildDefault49()
    {
        // STOKLAR — ID 13 — primary master-data table
        yield return new TrackedTableSchema(
            TabloID: 13,
            TabloAdi: "STOKLAR",
            RecnoField: "sto_RECno",
            Fields: new[]
            {
                "sto_RECno", "sto_kod", "sto_isim", "sto_kisa_ismi", "sto_yabanci_isim",
                "sto_birim1_ad", "sto_birim2_ad", "sto_birim3_ad", "sto_birim4_ad",
                "sto_birim1_katsayi", "sto_birim2_katsayi", "sto_birim3_katsayi", "sto_birim4_katsayi",
                "sto_altgrup_kod", "sto_anagrup_kod", "sto_sektor_kodu", "sto_marka_kodu",
                "sto_uretici_kodu", "sto_reyon_kodu", "sto_model_kodu", "sto_kategori_kodu",
                "sto_yer_kod", "sto_ambalaj_kodu", "sto_kalkon_kodu",
                "sto_bedenli_takip", "sto_renkDetayli", "sto_perakende_vergi", "sto_toptan_vergi",
                "sto_cins", "sto_doviz_cinsi", "sto_standartmaliyet",
                "sto_sat_cari_kod", "sto_yer_kod",
                "sto_iptal", "sto_pasif_fl",
            },
            RequiresSoftDeleteFilter: true);

        // CARI_HESAPLAR — ID 31
        yield return new TrackedTableSchema(
            TabloID: 31,
            TabloAdi: "CARI_HESAPLAR",
            RecnoField: "cari_RECno",
            Fields: new[]
            {
                "cari_RECno", "cari_kod", "cari_unvan1", "cari_unvan2", "cari_muh_kod", "cari_muh_kod1", "cari_muh_kod2",
                "cari_vdaire_adi", "cari_vdaire_no", "cari_Ana_cari_kodu",
                "cari_bolge_kodu", "cari_grup_kodu", "cari_temsilci_kodu", "cari_sektor_kodu",
                "cari_satis_isk_kod", "cari_special1", "cari_special2", "cari_special3",
                "cari_sicil_no", "cari_VergiKimlikNo",
                "cari_banka_hesapno1", "cari_CepTel", "cari_EMail",
                "cari_vade_fark_yuz", "cari_vade_fark_yuz1", "cari_vade_fark_yuz2",
                "cari_doviz_cinsi", "cari_doviz_cinsi1", "cari_doviz_cinsi2",
                "cari_odeme_gunu", "cari_hareket_tipi", "cari_odemeplan_no", "cari_satis_fk",
                "cari_KurHesapSekli", "cari_odeme_cinsi", "cari_cari_kilitli_flg",
                // Some Mikro editions do not expose cari_tipi. Keep the
                // trigger payload limited to columns present in the standard
                // CARI_HESAPLAR layout used by this installation family.
                "cari_fatura_adres_no", "cari_sevk_adres_no",
                "cari_VarsayilanGirisDepo", "cari_VarsayilanCikisDepo",
                "cari_RECid_DBCno", "cari_RECid_RECno",
                "cari_iptal", "cari_cari_kilitli_flg",
            },
            RequiresSoftDeleteFilter: true);

        // SORUMLULUK_MERKEZLERI — ID 3
        yield return new TrackedTableSchema(
            TabloID: 3,
            TabloAdi: "SORUMLULUK_MERKEZLERI",
            RecnoField: "som_RECno",
            Fields: new[] { "som_RECno", "som_kod", "som_isim" });

        // PROJELER — ID 176
        yield return new TrackedTableSchema(
            TabloID: 176,
            TabloAdi: "PROJELER",
            RecnoField: "pro_RECno",
            Fields: new[] { "pro_RECno", "pro_kodu", "pro_adi" });

        // STOK_BEDEN_TANIMLARI — ID 101
        yield return new TrackedTableSchema(
            TabloID: 101,
            TabloAdi: "STOK_BEDEN_TANIMLARI",
            RecnoField: "bdn_RECno",
            Fields: new[] { "bdn_RECno", "bdn_kodu", "bdn_ismi" });

        // STOK_RENK_TANIMLARI — ID 157
        yield return new TrackedTableSchema(
            TabloID: 157,
            TabloAdi: "STOK_RENK_TANIMLARI",
            RecnoField: "rnk_RECno",
            Fields: new[] { "rnk_RECno", "rnk_kodu", "rnk_ismi" });

        // STOK_ANA_GRUPLARI — ID 11
        yield return new TrackedTableSchema(
            TabloID: 11,
            TabloAdi: "STOK_ANA_GRUPLARI",
            RecnoField: "san_RECno",
            Fields: new[] { "san_RECno", "san_kod", "san_isim" });

        // STOK_ALT_GRUPLARI — ID 12
        yield return new TrackedTableSchema(
            TabloID: 12,
            TabloAdi: "STOK_ALT_GRUPLARI",
            RecnoField: "sta_RECno",
            Fields: new[] { "sta_RECno", "sta_kod", "sta_isim", "sta_ana_grup_kod" });

        // STOK_URETICILERI — ID 6
        yield return new TrackedTableSchema(
            TabloID: 6,
            TabloAdi: "STOK_URETICILERI",
            RecnoField: "urt_RECno",
            Fields: new[] { "urt_RECno", "urt_kod", "urt_ismi" });

        // STOK_REYONLARI — ID 7
        yield return new TrackedTableSchema(
            TabloID: 7,
            TabloAdi: "STOK_REYONLARI",
            RecnoField: "ryn_RECno",
            Fields: new[] { "ryn_RECno", "ryn_kod", "ryn_ismi" });

        // STOK_AMBALAJLARI — ID 20
        yield return new TrackedTableSchema(
            TabloID: 20,
            TabloAdi: "STOK_AMBALAJLARI",
            RecnoField: "amb_RECno",
            Fields: new[] { "amb_RECno", "amb_kod", "amb_ismi", "amb_miktar", "amb_dara" });

        // STOK_MARKALARI — ID 19
        yield return new TrackedTableSchema(
            TabloID: 19,
            TabloAdi: "STOK_MARKALARI",
            RecnoField: "mrk_RECno",
            Fields: new[] { "mrk_RECno", "mrk_kod", "mrk_ismi" });

        // STOK_CARI_ISKONTO_TANIMLARI — ID 14
        yield return new TrackedTableSchema(
            TabloID: 14,
            TabloAdi: "STOK_CARI_ISKONTO_TANIMLARI",
            RecnoField: "isk_RECno",
            Fields: new[]
            {
                "isk_RECno", "isk_stok_kod", "isk_cari_kod", "isk_isim",
                "isk_isk1_yuzde", "isk_isk2_yuzde", "isk_isk3_yuzde",
                "isk_isk4_yuzde", "isk_isk5_yuzde", "isk_isk6_yuzde",
                "isk_mas1_yuzde", "isk_mas2_yuzde", "isk_mas3_yuzde", "isk_mas4_yuzde",
                "isk_uygulama_odeme_plani",
                "isk_isk1_uygulama", "isk_isk2_uygulama", "isk_isk3_uygulama",
                "isk_isk4_uygulama", "isk_isk5_uygulama", "isk_isk6_uygulama",
                "isk_mas1_uygulama", "isk_mas2_uygulama", "isk_mas3_uygulama", "isk_mas4_uygulama",
            });

        // STOK_SATIS_FIYAT_LISTELERI — ID 228
        yield return new TrackedTableSchema(
            TabloID: 228,
            TabloAdi: "STOK_SATIS_FIYAT_LISTELERI",
            RecnoField: "sfiyat_RECno",
            Fields: new[]
            {
                "sfiyat_RECno", "sfiyat_stokkod", "sfiyat_iskontokod", "sfiyat_kampanyakod",
                "sfiyat_primyuzdesi", "sfiyat_fiyati",
                "sfiyat_listesirano", "sfiyat_deposirano", "sfiyat_odemeplan", "sfiyat_doviz", "sfiyat_deg_nedeni",
            });

        // STOK_SATIS_FIYAT_LISTE_TANIMLARI — ID 227
        // This Mikro layout exposes the system/audit and formula fields shown
        // by the customer's database; it does not use the older sfl_no,
        // sfl_adi, or sfl_doviz_cinsi names.
        yield return new TrackedTableSchema(
            TabloID: 227,
            TabloAdi: "STOK_SATIS_FIYAT_LISTE_TANIMLARI",
            RecnoField: "sfl_RECno",
            Fields: new[]
            {
                "sfl_RECno", "sfl_RECid_DBCno", "sfl_RECid_RECno", "sfl_SpecRECno", "sfl_iptal",
                "sfl_fileid", "sfl_hidden", "sfl_kilitli", "sfl_degisti", "sfl_checksum",
                "sfl_create_user", "sfl_create_date", "sfl_lastup_user", "sfl_lastup_date",
                "sfl_special1", "sfl_special2", "sfl_special3", "sfl_sirano", "sfl_aciklama",
                "sfl_fiyatuygulama", "sfl_fiyatformul", "sfl_odepluygulama", "sfl_odeplformul",
                "sfl_sabit_odeme_plani", "sfl_kdvdahil", "sfl_ilktarih", "sfl_sontarih",
                "sfl_yerineuygulanacakfiyat", "sfl_kurhesaplamasekli", "sfl_doviz_uygulama",
                "sfl_sabit_doviz", "sfl_iskonto_uygulama", "sfl_sabit_iskonto", "sfl_sabit_kur",
                "sfl_kampanya_uygulama", "sfl_sabit_kampanya", "sfl_kampanya_vade_gozardi",
                "sfl_kampanya_iskonto_gozardi", "sfl_otvdahil", "sfl_oivdahil",
            },
            RequiresSoftDeleteFilter: true);

        // STOK_CARI_KAMPANYA_TANIMLARI — ID 232
        // Mikro campaign columns use the kampanya_* prefix in this schema;
        // kam_* belongs to a different/older table layout.
        yield return new TrackedTableSchema(
            TabloID: 232,
            TabloAdi: "STOK_CARI_KAMPANYA_TANIMLARI",
            RecnoField: "kampanya_RECno",
            Fields: new[] { "kampanya_RECno", "kampanya_kod", "kampanya_ismi" });

        // STOK_SEKTORLERI — ID 8
        yield return new TrackedTableSchema(
            TabloID: 8,
            TabloAdi: "STOK_SEKTORLERI",
            RecnoField: "skr_RECno",
            Fields: new[] { "skr_RECno", "skr_kod", "skr_ismi" });

        // STOK_KATEGORILERI — ID 8 (legacy shared ID with SEKTORLERI in reference; reuses RECno pattern)
        yield return new TrackedTableSchema(
            TabloID: 8,
            TabloAdi: "STOK_KATEGORILERI",
            RecnoField: "ktg_RECno",
            Fields: new[] { "ktg_RECno", "ktg_kod", "ktg_ismi" });

        // CARI_HESAP_BOLGELERI — ID 39
        yield return new TrackedTableSchema(
            TabloID: 39,
            TabloAdi: "CARI_HESAP_BOLGELERI",
            RecnoField: "bol_RECno",
            Fields: new[] { "bol_RECno", "bol_kod", "bol_ismi" });

        // CARI_HESAP_GRUPLARI — ID 9
        yield return new TrackedTableSchema(
            TabloID: 9,
            TabloAdi: "CARI_HESAP_GRUPLARI",
            RecnoField: "gr_RECno",
            Fields: new[] { "gr_RECno", "gr_kod", "gr_ismi" });

        // CARI_PERSONEL_TANIMLARI — ID 104
        yield return new TrackedTableSchema(
            TabloID: 104,
            TabloAdi: "CARI_PERSONEL_TANIMLARI",
            RecnoField: "cari_per_RECno",
            Fields: new[]
            {
                "cari_per_RECno", "cari_per_kod", "cari_per_adi", "cari_per_soyadi",
                "cari_per_tel", "cari_per_ceptel", "cari_per_email",
                "cari_per_bolge_kodu", "cari_per_satis_isk_kod",
                "cari_per_aktif_fl",
            });

        // BARKOD_TANIMLARI — ID 15
        yield return new TrackedTableSchema(
            TabloID: 15,
            TabloAdi: "BARKOD_TANIMLARI",
            RecnoField: "bar_RECno",
            Fields: new[]
            {
                "bar_RECno", "bar_kodu", "bar_stokkodu", "bar_partikodu", "bar_lotno",
                "bar_serino_veya_bagkodu", "bar_birimpntr", "bar_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // ASORTI_TANIMLARI — ID 172
        yield return new TrackedTableSchema(
            TabloID: 172,
            TabloAdi: "ASORTI_TANIMLARI",
            RecnoField: "asr_RECno",
            Fields: new[] { "asr_RECno", "asr_kod", "asr_isim" });

        // CARI_HESAP_ADRESLERI — ID 32
        yield return new TrackedTableSchema(
            TabloID: 32,
            TabloAdi: "CARI_HESAP_ADRESLERI",
            RecnoField: "adr_RECno",
            Fields: new[]
            {
                "adr_RECno", "adr_cari_kod", "adr_adres_no", "adr_il", "adr_ilce",
                "adr_cadde", "adr_mahalle", "adr_sokak", "adr_Semt",
                "adr_Apt_No", "adr_Daire_No", "adr_posta_kodu",
                "adr_gps_enlem", "adr_gps_boylam", "adr_temsilci_kodu",
                "adr_aprint_fl", "adr_ulke", "adr_tel_ulke_kodu", "adr_tel_bolge_kodu", "adr_tel_no1",
                "adr_ziyaretperyodu", "adr_ziyaretgunu", "adr_efatura_alias",
                "adr_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // CARI_HESAP_YETKILILERI — ID 33
        yield return new TrackedTableSchema(
            TabloID: 33,
            TabloAdi: "CARI_HESAP_YETKILILERI",
            RecnoField: "mye_RECno",
            Fields: new[]
            {
                "mye_RECno", "mye_cari_kod", "mye_isim", "mye_soyisim", "mye_email_adres",
                "mye_cep_telno", "mye_tc_kimlikno", "mye_vergi_kimlikno", "mye_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // DEPOLAR — ID 111
        yield return new TrackedTableSchema(
            TabloID: 111,
            TabloAdi: "DEPOLAR",
            RecnoField: "dep_RECno",
            Fields: new[]
            {
                "dep_RECno", "dep_firmano", "dep_no", "dep_adi", "dep_tip", "dep_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // IHRACAT_DOSYALARI — ID 122
        yield return new TrackedTableSchema(
            TabloID: 122,
            TabloAdi: "IHRACAT_DOSYALARI",
            RecnoField: "ihr_RECno",
            Fields: new[] { "ihr_RECno", "ihr_no", "ihr_adi" });

        // KUR_ISIMLERI — ID 1020 (V15+ only; lives in master DB MikroDB_V15)
        yield return new TrackedTableSchema(
            TabloID: 1020,
            TabloAdi: "KUR_ISIMLERI",
            RecnoField: "kur_RECno",
            Fields: new[] { "kur_RECno", "kur_kodu", "kur_ismi" });

        // ITHALAT_DOSYALARI — ID 119
        yield return new TrackedTableSchema(
            TabloID: 119,
            TabloAdi: "ITHALAT_DOSYALARI",
            RecnoField: "ith_RECno",
            Fields: new[] { "ith_RECno", "ith_no", "ith_adi" });

        // ODEME_PLANLARI — ID 72
        yield return new TrackedTableSchema(
            TabloID: 72,
            TabloAdi: "ODEME_PLANLARI",
            RecnoField: "odp_RECno",
            Fields: new[] { "odp_RECno", "odp_no", "odp_aratop", "odp_iptal" },
            RequiresSoftDeleteFilter: true);

        // KASALAR — ID 53
        yield return new TrackedTableSchema(
            TabloID: 53,
            TabloAdi: "KASALAR",
            RecnoField: "kas_RECno",
            Fields: new[] { "kas_RECno", "kas_firma_no", "kas_kod", "kas_isim", "kas_doviz_cinsi", "kas_iptal" },
            RequiresSoftDeleteFilter: true);

        // STOK_SERINO_TANIMLARI — ID 94
        yield return new TrackedTableSchema(
            TabloID: 94,
            TabloAdi: "STOK_SERINO_TANIMLARI",
            RecnoField: "sn_RECno",
            Fields: new[] { "sn_RECno", "sn_stokkodu", "sn_serino" });

        // CARI_HESAP_HAREKETLERI — ID 51 (large; usually only delta)
        yield return new TrackedTableSchema(
            TabloID: 51,
            TabloAdi: "CARI_HESAP_HAREKETLERI",
            RecnoField: "cha_RECno",
            Fields: new[]
            {
                "cha_RECno", "cha_tarih", "cha_tip", "cha_evrak_tip", "cha_evrak_seri", "cha_evrak_sira",
                "cha_cari_kod", "cha_meblag", "cha_aratop", "cha_vade_tarih", "cha_doviz_cinsi", "cha_doviz_kuru",
                "cha_aciklama", "cha_kod", "cha_grup_no", "cha_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // SIPARISLER — ID 21
        yield return new TrackedTableSchema(
            TabloID: 21,
            TabloAdi: "SIPARISLER",
            RecnoField: "sip_RECno",
            Fields: new[]
            {
                "sip_RECno", "sip_firmano", "sip_sube_no",
                "sip_evrakno_seri", "sip_evrakno_sira", "sip_satirno",
                "sip_tarih", "sip_tip", "sip_cins", "sip_normal_iade",
                "sip_musteri_kod", "sip_stok_kod",
                "sip_miktar", "sip_teslim_miktar", "sip_birim_pntr",
                "sip_tutar", "sip_iskonto_1", "sip_iskonto_2", "sip_iskonto_3",
                "sip_iskonto_4", "sip_iskonto_5", "sip_iskonto_6",
                "sip_vergi", "sip_masvergi",
                "sip_depono", "sip_satici_kod", "sip_teslim_tarih", "sip_teslimturu",
                "sip_doviz_cinsi", "sip_doviz_kuru",
                "sip_kapat_fl", "sip_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // PROFORMA_SIPARISLER — ID 22
        yield return new TrackedTableSchema(
            TabloID: 22,
            TabloAdi: "PROFORMA_SIPARISLER",
            RecnoField: "psp_RECno",
            Fields: new[]
            {
                "psp_RECno", "psp_firmano", "psp_sube_no", "psp_evrakno_seri", "psp_evrakno_sira",
                "psp_tarih", "psp_musteri_kod", "psp_stok_kod",
                "psp_miktar", "psp_teslim_miktar", "psp_tutar", "psp_doviz_cinsi", "psp_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // DEPOLAR_ARASI_SIPARISLER — ID 86
        yield return new TrackedTableSchema(
            TabloID: 86,
            TabloAdi: "DEPOLAR_ARASI_SIPARISLER",
            RecnoField: "dsip_RECno",
            Fields: new[]
            {
                "dsip_RECno", "dsip_firmano", "dsip_sube_no",
                "dsip_evrakno_seri", "dsip_evrakno_sira", "dsip_satirno",
                "dsip_tarih", "dsip_tip", "dsip_cins",
                "dsip_kaynak_depo_no", "dsip_hedef_depo_no",
                "dsip_stok_kod", "dsip_miktar", "dsip_teslim_miktar",
                "dsip_tutar", "dsip_doviz_cinsi", "dsip_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // DOVIZ_KURLARI — ID 1007 (lives in master DB MikroDB_V15)
        yield return new TrackedTableSchema(
            TabloID: 1007,
            TabloAdi: "DOVIZ_KURLARI",
            RecnoField: "kur_RECno",
            Fields: new[]
            {
                "kur_RECno", "kur_tarih", "kur_doviz_cinsi", "kur_alis", "kur_satis", "kur_efektif_alis", "kur_efektif_satis",
            });

        // SATIS_SARTLARI — ID 45
        yield return new TrackedTableSchema(
            TabloID: 45,
            TabloAdi: "SATIS_SARTLARI",
            RecnoField: "sat_RECno",
            Fields: new[]
            {
                "sat_RECno", "sat_stok_kod", "sat_cari_kod", "sat_basla_tarih", "sat_bitis_tarih",
                "sat_birim_pntr", "sat_brut_fiyat", "sat_isk_miktar1", "sat_isk_miktar2",
                "sat_isk_miktar3", "sat_isk_miktar4", "sat_isk_miktar5", "sat_isk_miktar6",
                "sat_doviz_cinsi", "sat_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // SATINALMA_SARTLARI — ID 44
        yield return new TrackedTableSchema(
            TabloID: 44,
            TabloAdi: "SATINALMA_SARTLARI",
            RecnoField: "sas_RECno",
            Fields: new[]
            {
                "sas_RECno", "sas_stok_kod", "sas_cari_kod", "sas_basla_tarih", "sas_bitis_tarih",
                "sas_birim_pntr", "sas_brut_fiyat", "sas_isk_miktar1", "sas_isk_miktar2",
                "sas_isk_miktar3", "sas_isk_miktar4", "sas_isk_miktar5", "sas_isk_miktar6",
                "sas_doviz_cinsi", "sas_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // STOK_HAREKETLERI — ID 16 (very large; usually only delta)
        yield return new TrackedTableSchema(
            TabloID: 16,
            TabloAdi: "STOK_HAREKETLERI",
            RecnoField: "sth_RECno",
            Fields: new[]
            {
                "sth_RECno", "sth_firmano", "sth_sube_no",
                "sth_tarih", "sth_tip", "sth_cins", "sth_normal_iade",
                "sth_evraktip", "sth_evrakno_seri", "sth_evrakno_sira", "sth_satirno",
                "sth_belge_no", "sth_belge_tarih", "sth_stok_kod",
                "sth_cari_cinsi", "sth_cari_kod", "sth_cari_grup_no",
                "sth_plasiyer_kodu",
                "sth_har_doviz_cinsi", "sth_har_doviz_kuru", "sth_stok_doviz_cinsi", "sth_stok_doviz_kuru",
                "sth_miktar", "sth_birim_pntr", "sth_tutar",
                "sth_iskonto1", "sth_iskonto2", "sth_iskonto3", "sth_iskonto4", "sth_iskonto5", "sth_iskonto6",
                "sth_vergi", "sth_vergi_pntr",
                "sth_giris_depo_no", "sth_cikis_depo_no",
                "sth_malkbl_sevk_tarihi",
                "sth_cari_srm_merkezi", "sth_stok_srm_merkezi",
                "sth_aciklama", "sth_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // CIHAZ_HAREKETLERI — ID 98
        yield return new TrackedTableSchema(
            TabloID: 98,
            TabloAdi: "CIHAZ_HAREKETLERI",
            RecnoField: "chz_RECno",
            Fields: new[] { "chz_RECno", "chz_cihaz_kod", "chz_tarih", "chz_seri_no", "chz_iptal" },
            RequiresSoftDeleteFilter: true);

        // ODEME_EMIRLERI — ID 54
        yield return new TrackedTableSchema(
            TabloID: 54,
            TabloAdi: "ODEME_EMIRLERI",
            RecnoField: "oe_RECno",
            Fields: new[]
            {
                "oe_RECno", "oe_firma_no", "oe_sube_no", "oe_tarih", "oe_vade_tarih",
                "oe_cari_kod", "oe_meblag", "oe_doviz_cinsi", "oe_doviz_kuru",
                "oe_aciklama", "oe_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // CARI_HESAP_TEMINATLARI — ID 201
        yield return new TrackedTableSchema(
            TabloID: 201,
            TabloAdi: "CARI_HESAP_TEMINATLARI",
            RecnoField: "tem_RECno",
            Fields: new[]
            {
                "tem_RECno", "tem_cari_kod", "tem_tip", "tem_tutar", "tem_doviz_cinsi", "tem_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // FIRMALAR — ID 107
        yield return new TrackedTableSchema(
            TabloID: 107,
            TabloAdi: "FIRMALAR",
            RecnoField: "frm_RECno",
            Fields: new[] { "frm_RECno", "frm_no", "frm_adi", "frm_iptal" },
            RequiresSoftDeleteFilter: true);

        // SUBELER — ID 112
        yield return new TrackedTableSchema(
            TabloID: 112,
            TabloAdi: "SUBELER",
            RecnoField: "sub_RECno",
            Fields: new[] { "sub_RECno", "sub_no", "sub_adi", "sub_iptal" },
            RequiresSoftDeleteFilter: true);

        // BANKALAR — ID 52
        yield return new TrackedTableSchema(
            TabloID: 52,
            TabloAdi: "BANKALAR",
            RecnoField: "ban_RECno",
            Fields: new[]
            {
                "ban_RECno", "ban_firma_no", "ban_kod", "ban_ismi", "ban_sube", "ban_hesapno",
                "ban_doviz_cinsi", "ban_TCMB_Kodu", "ban_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // TESLIM_TURLERI — ID 99
        yield return new TrackedTableSchema(
            TabloID: 99,
            TabloAdi: "TESLIM_TURLERI",
            RecnoField: "tslt_RECno",
            Fields: new[] { "tslt_RECno", "tslt_kod", "tslt_ismi" });

        // MASRAF_HESAPLARI — ID 62
        yield return new TrackedTableSchema(
            TabloID: 62,
            TabloAdi: "MASRAF_HESAPLARI",
            RecnoField: "msf_RECno",
            Fields: new[] { "msf_RECno", "msf_kod", "msf_ismi" });

        // EVRAK_ACIKLAMALARI — ID 66
        yield return new TrackedTableSchema(
            TabloID: 66,
            TabloAdi: "EVRAK_ACIKLAMALARI",
            RecnoField: "egk_RECno",
            Fields: new[]
            {
                "egk_RECno", "egk_hareket_tip", "egk_evr_tip", "egk_evr_seri", "egk_evr_sira",
                "egk_evracik1", "egk_evracik2", "egk_evracik3", "egk_evracik4", "egk_evracik5",
                "egk_evracik6", "egk_evracik7", "egk_evracik8", "egk_evracik9", "egk_evracik10",
            });

        // YEREL_BANKA_KODLARI — ID 127 (V14 only — V12 uses BANKA_TCMB_KODLARI alias)
        yield return new TrackedTableSchema(
            TabloID: 127,
            TabloAdi: "YEREL_BANKA_KODLARI",
            RecnoField: "ybn_RECno",
            Fields: new[] { "ybn_RECno", "ybn_kod", "ybn_ismi" });

        // ZIYARET_HAREKETLERI — ID 99990 (ErpBridge's own table; usually not in Mikro, kept here for parity with reference app)
        yield return new TrackedTableSchema(
            TabloID: 99990,
            TabloAdi: "_ZIYARET_HAREKETLERI",
            RecnoField: "zyrt_RecNo",
            Fields: new[]
            {
                "zyrt_RecNo", "zyrt_Tarihi", "zyrt_Temsilci_Kodu", "zyrt_Cari_Kodu", "zyrt_Cari_Adres_No",
                "zyrt_Satis_Yapildi", "zyrt_Siparis_Alindi", "zyrt_Tamamlandi", "zyrt_iptal",
            },
            RequiresSoftDeleteFilter: true);

        // _FORA_PARAMETRELER — ID 99991 (NOT used by ErpBridge; recorded here only so the ID space is identical to the reference app. ErpBridge uses its own API-key auth, so no trigger is created for this table by the installer.)
        yield return new TrackedTableSchema(
            TabloID: 99991,
            TabloAdi: "_ERPB_SYNC_BOOTSTRAP",
            RecnoField: "boot_RECno",
            Fields: new[] { "boot_RECno", "boot_SyncState", "boot_LastTriggerRecNo", "boot_TenantId" });

        // SON_KULLANICILAR — ID 95
        yield return new TrackedTableSchema(
            TabloID: 95,
            TabloAdi: "SON_KULLANICILAR",
            RecnoField: "sk_RECno",
            Fields: new[] { "sk_RECno", "sk_kod", "sk_ismi" });

        // CIHAZ_GRUPLARI — ID 268
        yield return new TrackedTableSchema(
            TabloID: 268,
            TabloAdi: "CIHAZ_GRUPLARI",
            RecnoField: "cg_RECno",
            Fields: new[] { "cg_RECno", "cg_kod", "cg_ismi" });

        // CIHAZ_SORUNLARI — ID 96
        yield return new TrackedTableSchema(
            TabloID: 96,
            TabloAdi: "CIHAZ_SORUNLARI",
            RecnoField: "cs_RECno",
            Fields: new[] { "cs_RECno", "cs_kod", "cs_ismi" });

        // ARIZA_GRUPLARI — ID 263
        yield return new TrackedTableSchema(
            TabloID: 263,
            TabloAdi: "ARIZA_GRUPLARI",
            RecnoField: "ar_RECno",
            Fields: new[] { "ar_RECno", "ar_kod", "ar_ismi" });

        // HIZMET_HESAPLARI — ID 61
        yield return new TrackedTableSchema(
            TabloID: 61,
            TabloAdi: "HIZMET_HESAPLARI",
            RecnoField: "hz_RECno",
            Fields: new[] { "hz_RECno", "hz_kod", "hz_ismi" });

        // BAKIM_HAREKETLERI — ID 147
        yield return new TrackedTableSchema(
            TabloID: 147,
            TabloAdi: "BAKIM_HAREKETLERI",
            RecnoField: "bh_RECno",
            Fields: new[] { "bh_RECno", "bh_kod", "bh_ismi" });

        // BAKIM_KABUL_HAREKETLERI — ID 97
        yield return new TrackedTableSchema(
            TabloID: 97,
            TabloAdi: "BAKIM_KABUL_HAREKETLERI",
            RecnoField: "bkh_RECno",
            Fields: new[] { "bkh_RECno", "bkh_kod", "bkh_ismi" });

        // EKIP_TANIMLARI — ID 260
        yield return new TrackedTableSchema(
            TabloID: 260,
            TabloAdi: "EKIP_TANIMLARI",
            RecnoField: "ek_RECno",
            Fields: new[] { "ek_RECno", "ek_kod", "ek_ismi" });

        // BEDEN_HAREKETLERI — ID 147
        yield return new TrackedTableSchema(
            TabloID: 147,
            TabloAdi: "BEDEN_HAREKETLERI",
            RecnoField: "bdh_RECno",
            Fields: new[] { "bdh_RECno", "bdh_stok_kod", "bdh_beden_kodu" });

        // STOK_PAKET_TANIMLARI
        yield return new TrackedTableSchema(
            TabloID: 150,
            TabloAdi: "STOK_PAKET_TANIMLARI",
            RecnoField: "pkt_RECno",
            Fields: new[] { "pkt_RECno", "pkt_kod", "pkt_ismi" });

        // KAPAMA_NEDENLERI_TANIMLARI
        yield return new TrackedTableSchema(
            TabloID: 170,
            TabloAdi: "KAPAMA_NEDENLERI_TANIMLARI",
            RecnoField: "kn_RECno",
            Fields: new[] { "kn_RECno", "kn_kod", "kn_ismi" });
    }
}
