using System.Collections.Generic;
using System.Data;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.TablolarV16;

public static class TabloHelperV16
{
	public static TabloV16 GetTablo(string TabloAdi, List<TabloV16> Tablolar)
	{
		foreach (TabloV16 item in Tablolar)
		{
			if (TabloAdi == item.TabloAdi)
			{
				return item;
			}
		}
		return new TabloV16();
	}

	public static List<TabloV16> GetMikroV16DefaultTablolar()
	{
		return new List<TabloV16>
		{
			Get_MikroV16_Tablo_STOKLAR(),
			Get_MikroV16_Tablo_STOK_ANA_GRUPLARI(),
			Get_MikroV16_Tablo_STOK_ALT_GRUPLARI(),
			Get_MikroV16_Tablo_STOK_URETICILERI(),
			Get_MikroV16_Tablo_STOK_REYONLARI(),
			Get_MikroV16_Tablo_STOK_MARKALARI(),
			Get_MikroV16_Tablo_STOK_CARI_ISKONTO_TANIMLARI(),
			Get_MikroV16_Tablo_STOK_SATIS_FIYAT_LISTELERI(),
			Get_MikroV16_Tablo_STOK_SATIS_FIYAT_LISTE_TANIMLARI(),
			Get_MikroV16_Tablo_STOK_PAKET_TANIMLARI(),
			Get_MikroV16_Tablo_STOK_BEDEN_TANIMLARI(),
			Get_MikroV16_Tablo_STOK_RENK_TANIMLARI(),
			Get_MikroV16_Tablo_STOK_SERINO_TANIMLARI(),
			Get_MikroV16_Tablo_STOK_KATEGORILERI(),
			Get_MikroV16_Tablo_STOK_SEKTORLERI(),
			Get_MikroV16_Tablo_SATIS_SARTLARI(),
			Get_MikroV16_Tablo_SATINALMA_SARTLARI(),
			Get_MikroV16_Tablo_BARKOD_TANIMLARI(),
			Get_MikroV16_Tablo_CARI_HESAPLAR(),
			Get_MikroV16_Tablo_CARI_HESAP_ADRESLERI(),
			Get_MikroV16_Tablo_CARI_HESAP_YETKILILERI(),
			Get_MikroV16_Tablo_CARI_HESAP_TEMINATLARI(),
			Get_MikroV16_Tablo_CARI_PERSONEL_TANIMLARI(),
			Get_MikroV16_Tablo_CARI_HESAP_BOLGELERI(),
			Get_MikroV16_Tablo_CARI_HESAP_GRUPLARI(),
			Get_MikroV16_Tablo_SORUMLULUK_MERKEZLERI(),
			Get_MikroV16_Tablo_PROJELER(),
			Get_MikroV16_Tablo_DEPOLAR(),
			Get_MikroV16_Tablo_ODEME_PLANLARI(),
			Get_MikroV16_Tablo_KASALAR(),
			Get_MikroV16_Tablo_DOVIZ_KURLARI(),
			Get_MikroV16_Tablo_ODEME_EMIRLERI(),
			Get_MikroV16_Tablo_FIRMALAR(),
			Get_MikroV16_Tablo_SUBELER(),
			Get_MikroV16_Tablo_BANKALAR(),
			Get_MikroV16_Tablo_MASRAF_HESAPLARI(),
			Get_MikroV16_Tablo_YEREL_BANKA_KODLARI(),
			Get_MikroV16_Tablo_TESLIM_TURLERI(),
			Get_MikroV16_Tablo_DEPOLAR_ARASI_SIPARISLER(),
			Get_MikroV16_Tablo_EVRAK_ACIKLAMALARI(),
			Get_MikroV16_Tablo_IHRACAT_DOSYALARI(),
			Get_MikroV16_Tablo_KAPAMA_NEDENLERI_TANIMLARI(),
			Get_MikroV16_Tablo_KUR_ISIMLERI(),
			Get_MikroV16_Tablo_CIHAZ_HAREKETLERI(),
			Get_MikroV16_Tablo_CARI_HESAP_HAREKETLERI(),
			Get_MikroV16_Tablo_STOK_HAREKETLERI(),
			Get_MikroV16_Tablo_SIPARISLER(),
			Get_MikroV16_Tablo_PROFORMA_SIPARISLER(),
			Get_MikroV16_Tablo_BEDEN_HAREKETLERI(),
			Get_MikroV16_Tablo__ZIYARET_HAREKETLERI()
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOKLAR()
	{
		return new TabloV16
		{
			TabloAdi = "STOKLAR",
			TabloID = 13,
			Indexler = 
			{
				"CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOKLAR_00 ON STOKLAR (sto_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_01 ON STOKLAR (sto_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_02 ON STOKLAR (sto_isim_upper)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_03 ON STOKLAR (sto_kod_upper)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_04 ON STOKLAR (sto_anagrup_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_05 ON STOKLAR (sto_altgrup_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_06 ON STOKLAR (sto_anagrup_kod,sto_altgrup_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_07 ON STOKLAR (sto_sektor_kodu)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_08 ON STOKLAR (sto_marka_kodu)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_09 ON STOKLAR (sto_model_kodu)",
				"CREATE INDEX IF NOT EXISTS NDX_STOKLAR_10 ON STOKLAR (sto_uretici_kodu)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_11 ON STOKLAR (sto_reyon_kodu)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_12 ON STOKLAR (sto_kategori_kodu)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_13 ON STOKLAR (sto_kategori_kodu,sto_isim_upper)", "CREATE INDEX IF NOT EXISTS NDX_STOKLAR_14 ON STOKLAR (sto_isim_upper,sto_kategori_kodu)"
			},
			Fieldlar = 
			{
				new FieldV16("sto_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sto_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_kisa_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_yabanci_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_sat_cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_birim1_ad", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_birim2_ad", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_birim3_ad", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_birim4_ad", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_beden_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_renk_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_altgrup_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_anagrup_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_sektor_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_marka_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_uretici_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_reyon_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_model_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_kategori_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_yer_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sto_birim1_katsayi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim2_katsayi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim3_katsayi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim4_katsayi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_standartmaliyet", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim1_agirlik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim1_en", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim1_boy", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim1_yukseklik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim1_dara", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim2_agirlik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim2_en", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim2_boy", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim2_yukseklik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim2_dara", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim3_agirlik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim3_en", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim3_boy", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim3_yukseklik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim3_dara", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim4_agirlik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim4_en", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim4_boy", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim4_yukseklik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_birim4_dara", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sto_detay_takip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_bedenli_takip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_perakende_vergi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_toptan_vergi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_webe_gonderilecek_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_renkDetayli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_ver_sip_birim", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_al_sip_birim", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_satis_dursun", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_siparis_dursun", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sto_malkabul_dursun", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_HESAPLAR()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_HESAPLAR",
			TabloID = 31,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_00 ON CARI_HESAPLAR (cari_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_01 ON CARI_HESAPLAR (cari_kod)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_02 ON CARI_HESAPLAR (cari_unvan1_upper)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_03 ON CARI_HESAPLAR (cari_kod_upper)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_04 ON CARI_HESAPLAR (cari_grup_kodu)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_05 ON CARI_HESAPLAR (cari_temsilci_kodu)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_06 ON CARI_HESAPLAR (cari_sektor_kodu)" },
			Fieldlar = 
			{
				new FieldV16("cari_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_unvan1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_unvan2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_muh_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_muh_kod1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_muh_kod2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_vdaire_adi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_vdaire_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_Ana_cari_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_bolge_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_grup_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_temsilci_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_sektor_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_satis_isk_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_sicil_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_VergiKimlikNo", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_banka_hesapno1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_CepTel", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_EMail", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_wwwadresi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_vade_fark_yuz", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cari_vade_fark_yuz1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cari_vade_fark_yuz2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cari_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_doviz_cinsi1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_doviz_cinsi2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_odeme_gunu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_hareket_tipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_odemeplan_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_satis_fk", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_KurHesapSekli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_odeme_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_cari_kilitli_flg", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_tipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_fatura_adres_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_sevk_adres_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_VarsayilanGirisDepo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_VarsayilanCikisDepo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_DBCno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_efatura_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_def_efatura_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_SORUMLULUK_MERKEZLERI()
	{
		return new TabloV16
		{
			TabloAdi = "SORUMLULUK_MERKEZLERI",
			TabloID = 3,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_SORUMLULUK_MERKEZLERI_00 ON SORUMLULUK_MERKEZLERI (som_Guid)", "CREATE INDEX IF NOT EXISTS NDX_SORUMLULUK_MERKEZLERI_01 ON SORUMLULUK_MERKEZLERI (som_kod)", "CREATE INDEX IF NOT EXISTS NDX_SORUMLULUK_MERKEZLERI_02 ON SORUMLULUK_MERKEZLERI (som_isim)" },
			Fieldlar = 
			{
				new FieldV16("som_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("som_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("som_isim", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_PROJELER()
	{
		return new TabloV16
		{
			TabloAdi = "PROJELER",
			TabloID = 176,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_PROJELER_00 ON PROJELER (pro_Guid)", "CREATE INDEX IF NOT EXISTS NDX_PROJELER_01 ON PROJELER (pro_kodu)", "CREATE INDEX IF NOT EXISTS NDX_PROJELER_02 ON PROJELER (pro_adi)" },
			Fieldlar = 
			{
				new FieldV16("pro_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pro_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_adi", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_BEDEN_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_BEDEN_TANIMLARI",
			TabloID = 101,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_BEDEN_TANIMLARI_00 ON STOK_BEDEN_TANIMLARI (bdn_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_BEDEN_TANIMLARI_01 ON STOK_BEDEN_TANIMLARI (bdn_kodu)" },
			Fieldlar = 
			{
				new FieldV16("bdn_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("bdn_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_4", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_5", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_6", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_7", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_8", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_9", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_10", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_11", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_12", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_13", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_14", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_15", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_16", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_17", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_18", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_19", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_20", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_21", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_22", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_23", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_24", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_25", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_26", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_27", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_28", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_29", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_30", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_31", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_32", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_33", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_34", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_35", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_36", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_37", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_38", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_39", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bdn_kirilim_40", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_RENK_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_RENK_TANIMLARI",
			TabloID = 157,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_RENK_TANIMLARI_00 ON STOK_RENK_TANIMLARI (rnk_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_RENK_TANIMLARI_01 ON STOK_RENK_TANIMLARI (rnk_kodu)" },
			Fieldlar = 
			{
				new FieldV16("rnk_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("rnk_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_4", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_5", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_6", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_7", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_8", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_9", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_10", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_11", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_12", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_13", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_14", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_15", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_16", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_17", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_18", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_19", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_20", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_21", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_22", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_23", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_24", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_25", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_26", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_27", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_28", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_29", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_30", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_31", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_32", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_33", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_34", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_35", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_36", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_37", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_38", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_39", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_40", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_41", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_42", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_43", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_44", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_45", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_46", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_47", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_48", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_49", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_50", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_51", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_52", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_53", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_54", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_55", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_56", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_57", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_58", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_59", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("rnk_kirilim_60", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_ANA_GRUPLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_ANA_GRUPLARI",
			TabloID = 11,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_ANA_GRUPLARI_00 ON STOK_ANA_GRUPLARI (san_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_ANA_GRUPLARI_01 ON STOK_ANA_GRUPLARI (san_kod)" },
			Fieldlar = 
			{
				new FieldV16("san_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("san_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("san_isim", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_ALT_GRUPLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_ALT_GRUPLARI",
			TabloID = 12,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_00 ON STOK_ALT_GRUPLARI (sta_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_02 ON STOK_ALT_GRUPLARI (sta_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_03 ON STOK_ALT_GRUPLARI (sta_ana_grup_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_04 ON STOK_ALT_GRUPLARI (sta_kod,sta_ana_grup_kod)" },
			Fieldlar = 
			{
				new FieldV16("sta_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sta_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sta_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sta_ana_grup_kod", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_URETICILERI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_URETICILERI",
			TabloID = 6,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_URETICILERI_00 ON STOK_URETICILERI (urt_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_URETICILERI_01 ON STOK_URETICILERI (urt_kod)" },
			Fieldlar = 
			{
				new FieldV16("urt_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("urt_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("urt_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_REYONLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_REYONLARI",
			TabloID = 7,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_REYONLARI_00 ON STOK_REYONLARI (ryn_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_REYONLARI_01 ON STOK_REYONLARI (ryn_kod)" },
			Fieldlar = 
			{
				new FieldV16("ryn_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ryn_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ryn_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_AMBALAJLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_AMBALAJLARI",
			TabloID = 20,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_AMBALAJLARI_00 ON STOK_AMBALAJLARI (amb_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_AMBALAJLARI_01 ON STOK_AMBALAJLARI (amb_kod)" },
			Fieldlar = 
			{
				new FieldV16("amb_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("amb_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("amb_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("amb_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("amb_dara", (SqlDbType)6, enum_SqliteDataType.REAL)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_MARKALARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_MARKALARI",
			TabloID = 19,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_MARKALARI_00 ON STOK_MARKALARI (mrk_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_MARKALARI_02 ON STOK_MARKALARI (mrk_kod)" },
			Fieldlar = 
			{
				new FieldV16("mrk_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("mrk_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mrk_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_CARI_ISKONTO_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_CARI_ISKONTO_TANIMLARI",
			TabloID = 14,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_00 ON STOK_CARI_ISKONTO_TANIMLARI (isk_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_01 ON STOK_CARI_ISKONTO_TANIMLARI (isk_stok_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_02 ON STOK_CARI_ISKONTO_TANIMLARI (isk_cari_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_03 ON STOK_CARI_ISKONTO_TANIMLARI (isk_stok_kod,isk_cari_kod)" },
			Fieldlar = 
			{
				new FieldV16("isk_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("isk_stok_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_isk1_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_isk2_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_isk3_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_isk4_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_isk5_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_isk6_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_mas1_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_mas2_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_mas3_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_mas4_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("isk_bedelsiz_referans_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_isk1_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_isk2_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_isk3_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_isk4_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_isk5_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_isk6_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_mas1_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_mas2_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_mas3_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_mas4_yuzde", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("isk_uygulama_odeme_plani", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_isk1_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_isk2_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_isk3_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_isk4_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_isk5_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_isk6_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_mas1_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_mas2_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_mas3_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("isk_mas4_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_SATIS_FIYAT_LISTELERI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_SATIS_FIYAT_LISTELERI",
			TabloID = 228,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTELERI_00 ON STOK_SATIS_FIYAT_LISTELERI (sfiyat_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTELERI_01 ON STOK_SATIS_FIYAT_LISTELERI (sfiyat_stokkod,sfiyat_listesirano,sfiyat_deposirano)" },
			Fieldlar = 
			{
				new FieldV16("sfiyat_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sfiyat_stokkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfiyat_iskontokod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfiyat_kampanyakod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfiyat_primyuzdesi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sfiyat_fiyati", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sfiyat_listesirano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfiyat_deposirano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfiyat_odemeplan", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfiyat_doviz", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_SATIS_FIYAT_LISTE_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_SATIS_FIYAT_LISTE_TANIMLARI",
			TabloID = 227,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTE_TANIMLARI_00 ON STOK_SATIS_FIYAT_LISTE_TANIMLARI (sfl_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTE_TANIMLARI_01 ON STOK_SATIS_FIYAT_LISTE_TANIMLARI (sfl_sirano)" },
			Fieldlar = 
			{
				new FieldV16("sfl_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sfl_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfl_fiyatformul", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfl_odeplformul", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfl_sabit_iskonto", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfl_sabit_kampanya", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sfl_sabit_kur", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sfl_sirano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_fiyatuygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_odepluygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_sabit_odeme_plani", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_kdvdahil", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_yerineuygulanacakfiyat", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_kurhesaplamasekli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_doviz_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_sabit_doviz", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_iskonto_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_kampanya_uygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_kampanya_vade_gozardi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_kampanya_iskonto_gozardi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_otvdahil", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_oivdahil", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sfl_ilktarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sfl_sontarih", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_CARI_KAMPANYA_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_CARI_KAMPANYA_TANIMLARI",
			TabloID = 232,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_00 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_01 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_stok_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_02 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_cari_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_03 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_stok_kod,kampanya_cari_kod)" },
			Fieldlar = 
			{
				new FieldV16("kampanya_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("kampanya_stok_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kampanya_cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kampanya_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kampanya_ilave_iskonto", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("kampanya_ilave_vade", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("kampanya_iskonto_no", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_SEKTORLERI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_SEKTORLERI",
			TabloID = 8,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SEKTORLERI_00 ON STOK_SEKTORLERI (sktr_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_SEKTORLERI_01 ON STOK_SEKTORLERI (sktr_kod)" },
			Fieldlar = 
			{
				new FieldV16("sktr_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sktr_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sktr_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sktr_muhkodu", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_KATEGORILERI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_KATEGORILERI",
			TabloID = 8,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_KATEGORILERI_00 ON STOK_KATEGORILERI (ktg_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_KATEGORILERI_01 ON STOK_KATEGORILERI (ktg_kod)" },
			Fieldlar = 
			{
				new FieldV16("ktg_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ktg_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ktg_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ktg_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_HESAP_BOLGELERI()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_HESAP_BOLGELERI",
			TabloID = 39,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_BOLGELERI_00 ON CARI_HESAP_BOLGELERI (bol_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_BOLGELERI_01 ON CARI_HESAP_BOLGELERI (bol_kod)" },
			Fieldlar = 
			{
				new FieldV16("bol_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("bol_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bol_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_HESAP_GRUPLARI()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_HESAP_GRUPLARI",
			TabloID = 9,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_GRUPLARI_00 ON CARI_HESAP_GRUPLARI (crg_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_GRUPLARI_01 ON CARI_HESAP_GRUPLARI (crg_kod)" },
			Fieldlar = 
			{
				new FieldV16("crg_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("crg_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("crg_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("crg_muhasebe_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_PERSONEL_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_PERSONEL_TANIMLARI",
			TabloID = 104,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_PERSONEL_TANIMLARI_00 ON CARI_PERSONEL_TANIMLARI (cari_per_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_PERSONEL_TANIMLARI_01 ON CARI_PERSONEL_TANIMLARI (cari_per_kod)" },
			Fieldlar = 
			{
				new FieldV16("cari_per_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("cari_per_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_adi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_soyadi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_kasiyerkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_kasiyersifresi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_kasiyerAmiri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_cepno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_mail", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_kasiyerfirmaid", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cari_per_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_per_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_per_userno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cari_per_depono", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_BARKOD_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "BARKOD_TANIMLARI",
			TabloID = 15,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_BARKOD_TANIMLARI_00 ON BARKOD_TANIMLARI (bar_Guid)", "CREATE INDEX IF NOT EXISTS NDX_BARKOD_TANIMLARI_01 ON BARKOD_TANIMLARI (bar_kodu)", "CREATE INDEX IF NOT EXISTS NDX_BARKOD_TANIMLARI_02 ON BARKOD_TANIMLARI (bar_stokkodu)" },
			Fieldlar = 
			{
				new FieldV16("bar_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("bar_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bar_stokkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bar_partikodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bar_serino_veya_bagkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bar_lotno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("bar_barkodtipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("bar_icerigi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("bar_birimpntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("bar_master", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("bar_bedenpntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("bar_renkpntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("bar_baglantitipi", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_ASORTI_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "ASORTI_TANIMLARI",
			TabloID = 172,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_ASORTI_TANIMLARI_00 ON ASORTI_TANIMLARI (Asorti_Guid)", "CREATE INDEX IF NOT EXISTS NDX_ASORTI_TANIMLARI_01 ON ASORTI_TANIMLARI (Asorti_StokKodu)" },
			Fieldlar = 
			{
				new FieldV16("Asorti_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("Asorti_StokKodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Asorti_Miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("Asorti_BedenNo", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_HESAP_ADRESLERI()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_HESAP_ADRESLERI",
			TabloID = 32,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_ADRESLERI_00 ON CARI_HESAP_ADRESLERI (adr_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_ADRESLERI_01 ON CARI_HESAP_ADRESLERI (adr_cari_kod)" },
			Fieldlar = 
			{
				new FieldV16("adr_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("adr_cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_cadde", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_sokak", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_posta_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_ilce", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_il", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_ulke", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_tel_ulke_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_tel_bolge_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_tel_no1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_tel_no2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_tel_faxno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_tel_modem", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_yon_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_temsilci_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_ozel_not", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("adr_ziyaretgunu", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("adr_gps_enlem", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("adr_gps_boylam", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("adr_adres_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_uzaklik_kodu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziyaretperyodu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziyarethaftasi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziygunu2_1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziygunu2_2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziygunu2_3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziygunu2_4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziygunu2_5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziygunu2_6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("adr_ziygunu2_7", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_HESAP_YETKILILERI()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_HESAP_YETKILILERI",
			TabloID = 33,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_YETKILILERI_00 ON CARI_HESAP_YETKILILERI (mye_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_YETKILILERI_01 ON CARI_HESAP_YETKILILERI (mye_cari_kod)" },
			Fieldlar = 
			{
				new FieldV16("mye_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("mye_cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_soyisim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_es_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_dahili_telno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_email_adres", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_cep_telno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_tc_kimlikno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_vergi_dairesi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_vergi_kimlikno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_dogum_yeri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_ev_cadde", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_ev_sokak", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_ev_posta_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_ev_ilce", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_ev_il", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_ev_ulke", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_is_telno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_ev_telno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("mye_adres_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("mye_unvan", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("mye_hitap", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("mye_hisse", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("mye_tahsil", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("mye_dogum_tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("mye_evlilik_tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("mye_es_dogum_tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_DEPOLAR()
	{
		return new TabloV16
		{
			TabloAdi = "DEPOLAR",
			TabloID = 111,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_DEPOLAR_00 ON DEPOLAR (dep_Guid)", "CREATE INDEX IF NOT EXISTS NDX_DEPOLAR_01 ON DEPOLAR (dep_no)" },
			Fieldlar = 
			{
				new FieldV16("dep_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("dep_adi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_grup_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_muh_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_sor_mer_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_proje_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_cadde", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_sokak", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_posta_Kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_Ilce", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_Il", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_Ulke", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_yetkili_email", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_dizin_adi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_tel_ulke_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_tel_bolge_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_tel_no1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_tel_no2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_tel_faxno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_tel_modem", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_barkod_yazici_yolu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_fason_sor_mer_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("dep_kamyon_kasa_hacmi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_kamyon_istiab_haddi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_satis_alani", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_sergi_alani", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_otopark_alani", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_gps_enlem", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_gps_boylam", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_alani", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_rafhacmi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dep_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_tipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_DepoSevkOtoFiyat", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_hareket_tipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_DepoSevkUygFiyat", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_otopark_kapasite", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_kasa_sayisi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_envanter_harici_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_detay_takibi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_EksiyeDusurenStkHar", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dep_BagliOrtakliklaraSatisUygFiyat", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_IHRACAT_DOSYALARI()
	{
		return new TabloV16
		{
			TabloAdi = "IHRACAT_DOSYALARI",
			TabloID = 122,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_IHRACAT_DOSYALARI_00 ON IHRACAT_DOSYALARI (ihr_Guid)", "CREATE INDEX IF NOT EXISTS NDX_IHRACAT_DOSYALARI_01 ON IHRACAT_DOSYALARI (ihr_kodu)" },
			Fieldlar = 
			{
				new FieldV16("ihr_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ihr_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ihr_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ihr_Satici", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ihr_SpecRecNo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ihr_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ihr_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ihr_TeslimSekli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ihr_OdemeSekli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ihr_carigrupno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ihr_DovizCinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ihr_create_date", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("ihr_lastup_date", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("ihr_GCB_Tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("ihr_Intac_Tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_KUR_ISIMLERI()
	{
		return new TabloV16
		{
			TabloAdi = "KUR_ISIMLERI",
			TabloID = 1020,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_KUR_ISIMLERI_00 ON KUR_ISIMLERI (Kur_Guid)" },
			Fieldlar = 
			{
				new FieldV16("Kur_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("Kur_Tip", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kur_sembol", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kur_adi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kur_orjAdi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kur_kusurat_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kur_kusurat_sembol", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kur_No", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("Kur_decimal", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_ITHALAT_DOSYALARI()
	{
		return new TabloV16
		{
			TabloAdi = "ITHALAT_DOSYALARI",
			TabloID = 119,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_ITHALAT_DOSYALARI_00 ON ITHALAT_DOSYALARI (ith_Guid)", "CREATE INDEX IF NOT EXISTS NDX_ITHALAT_DOSYALARI_01 ON ITHALAT_DOSYALARI (ith_kodu)" },
			Fieldlar = 
			{
				new FieldV16("ith_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ith_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_satici", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_ulkekodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_gumrukkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_Araci_Banka", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_GGB_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_vasitano", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_nakliyeci", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_gumrukmusaviri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_4", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_5", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_6", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_7", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_8", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_9", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhKodu_10", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MuhGrupKodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_MalBedeliMuhKodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_Mense_ulkekodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_Araci_CariKodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_Akreditif", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_kilitli_fl", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ith_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_ulketipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_teslimsekli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_odemesekli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_carigrupno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_dovizcinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_tasimasekli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli7", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli8", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSekli9", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_MalDagitimSek10", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_Mense_ulketipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ith_GGB_tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_ODEME_PLANLARI()
	{
		return new TabloV16
		{
			TabloAdi = "ODEME_PLANLARI",
			TabloID = 72,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_ODEME_PLANLARI_00 ON ODEME_PLANLARI (odp_Guid)", "CREATE INDEX IF NOT EXISTS NDX_ODEME_PLANLARI_01 ON ODEME_PLANLARI (odp_kodu)" },
			Fieldlar = 
			{
				new FieldV16("odp_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("odp_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("odp_adi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("odp_aratop", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("odp_masraf", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("odp_vergi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("odp_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("odp_ortgun", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_KASALAR()
	{
		return new TabloV16
		{
			TabloAdi = "KASALAR",
			TabloID = 53,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_KASALAR_00 ON KASALAR (kas_Guid)", "CREATE INDEX IF NOT EXISTS NDX_KASALAR_01 ON KASALAR (kas_kod)" },
			Fieldlar = 
			{
				new FieldV16("kas_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("kas_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kas_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kas_muh_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kas_bankakodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kas_nakakincelenmesi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kas_ufrs_muh_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("kas_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("kas_firma_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("kas_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_SERINO_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_SERINO_TANIMLARI",
			TabloID = 94,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SERINO_TANIMLARI_00 ON STOK_SERINO_TANIMLARI (chz_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_SERINO_TANIMLARI_01 ON STOK_SERINO_TANIMLARI (chz_serino)" },
			Fieldlar = 
			{
				new FieldV16("chz_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("chz_serino", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_stok_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_grup_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_Tuktckodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_aciklama1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_aciklama2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_aciklama3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_al_evr_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_al_cari_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_st_evr_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_st_cari_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_parca_garantisi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_parca_serino", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("chz_brut_fiati", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("chz_al_fiati_ana", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("chz_al_fiati_alt", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("chz_al_fiati_orj", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("chz_st_fiati_ana", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("chz_st_fiati_alt", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("chz_st_fiati_orj", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("chz_al_evr_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("chz_st_evr_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("chz_GrnBasTarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("chz_GrnBitTarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("chz_al_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("chz_st_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("chz_parca_garanti_baslangic", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("chz_parca_garanti_bitis", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_HESAP_HAREKETLERI()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_HESAP_HAREKETLERI",
			TabloID = 51,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_00 ON CARI_HESAP_HAREKETLERI (cha_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_01 ON CARI_HESAP_HAREKETLERI (cha_kod)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_02 ON CARI_HESAP_HAREKETLERI (cha_ciro_cari_kodu,cha_tarihi,cha_tip,cha_normal_Iade,cha_evrak_tip,cha_grupno)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_04 ON CARI_HESAP_HAREKETLERI (cha_cari_cins,cha_kod)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_05 ON CARI_HESAP_HAREKETLERI (cha_kasa_hizmet,cha_kasa_hizkod)" },
			Fieldlar = 
			{
				new FieldV16("cha_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("cha_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_projekodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_satici_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_srmrkkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_trefno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_ciro_cari_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_evrakno_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_belge_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_kasa_hizkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_EXIMkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("cha_ft_iskonto1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_iskonto2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_iskonto3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_iskonto4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_iskonto5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_iskonto6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_meblag", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_d_kur", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_miktari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_aratoplam", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_masraf1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_masraf2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_masraf3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_ft_masraf4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_otvtutari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi7", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi8", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi9", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_vergi10", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_altd_kur", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_karsid_kur", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("cha_tpoz", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_vergipntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_kasa_hizmet", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_normal_Iade", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_evrak_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_satir_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_evrakno_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_cari_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_vade", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_grupno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_d_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_ticaret_turu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("cha_tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("cha_belge_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("cha_lastup_date", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_SIPARISLER()
	{
		return new TabloV16
		{
			TabloAdi = "SIPARISLER",
			TabloID = 21,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_SIPARISLER_00 ON SIPARISLER (sip_Guid)", "CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_01 ON SIPARISLER (sip_tip,sip_stok_kod,sip_depono)", "CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_02 ON SIPARISLER (sip_musteri_kod,sip_kapat_fl,sip_tip)", "CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_03 ON SIPARISLER (sip_musteri_kod)", "CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_04 ON SIPARISLER (sip_evrakno_seri,sip_evrakno_sira,sip_cins,sip_tip)" },
			Fieldlar = 
			{
				new FieldV16("sip_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sip_prosip_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sip_stal_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sip_teklif_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sip_Rez_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sip_yetkili_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sip_musteri_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_evrakno_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_belgeno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_satici_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_aciklama2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_cari_sormerk", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_stok_sormerk", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_teslimturu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_Exp_Imp_Kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_parti_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_projekodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_paket_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_kapatmanedenkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_stok_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sip_tutar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_iskonto_1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_iskonto_2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_iskonto_3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_iskonto_4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_iskonto_5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_iskonto_6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_masraf_1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_masraf_2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_masraf_3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_masraf_4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_vergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_masvergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_doviz_kuru", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_teslim_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_b_fiyat", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_alt_doviz_kuru", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_kar_orani", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_planlananmiktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_Otv_Vergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_otvtutari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sip_DBCno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_kapat_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_depono", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_SpecRECno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_fileid", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_checksum", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_create_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_lastup_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_evrakno_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_satirno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_birim_pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_vergi_pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_masvergi_pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_opno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_OnaylayanKulNo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_cari_grupno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_adresno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_iskonto1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_iskonto2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_iskonto3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_iskonto4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_iskonto5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_iskonto6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_masraf1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_masraf2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_masraf3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_masraf4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_durumu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_lot_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_fiyat_liste_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_Otv_Pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_OtvVergisiz_Fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_harekettipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_iptal", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_hidden", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_kilitli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_degisti", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_vergisiz_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_promosyon_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_cagrilabilir_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_isk1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_isk2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_isk3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_isk4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_isk5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_isk6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_mas1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_mas2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_mas3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_mas4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sip_create_date", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sip_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sip_teslim_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sip_belge_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_PROFORMA_SIPARISLER()
	{
		return new TabloV16
		{
			TabloAdi = "PROFORMA_SIPARISLER",
			TabloID = 22,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_00 ON PROFORMA_SIPARISLER (pro_Guid)", "CREATE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_01 ON PROFORMA_SIPARISLER (pro_tipi,pro_stokkodu,pro_depono)", "CREATE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_02 ON PROFORMA_SIPARISLER (pro_mustkodu,pro_kapat,pro_tipi)", "CREATE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_03 ON PROFORMA_SIPARISLER (pro_mustkodu)" },
			Fieldlar = 
			{
				new FieldV16("pro_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pro_sip_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pro_stal_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pro_teklif_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pro_Rez_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pro_yetkili_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pro_mustkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_evrakno_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_belge_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_saticikodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_aciklama2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_cari_sormerk", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_stok_sormerk", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_teslimturu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_Exp_Imp_Kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_parti_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_projekodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_paket_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_kapatmanedenkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_stokkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pro_tutari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_iskonto1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_iskonto2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_iskonto3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_iskonto4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_iskonto5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_iskonto6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_masraf1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_masraf2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_masraf3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_masraf4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_vergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_masrafvergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_dovizkuru", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_tesmiktari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_bfiyati", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_altdovizkuru", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_karoani", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_planlananmiktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_Otv_Vergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_otvtutari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pro_DBCno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_kapat", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_tipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_depono", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_SpecRecNo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_fileid", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_checksum", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_create_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_lastup_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_evrakno_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_satirno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_birim_pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_vergipntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_masrafvergipntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_opno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_onaylayanKul_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_cari_grupno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_dovizcinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_adresno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_7", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_8", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_9", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_isk_mas_10", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_durumu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_lot_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_fiyat_liste_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_Otv_Pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_OtvVergisiz_Fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_harekettipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_iptal", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_hidden", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_kilitli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_degisti", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_vergisiz", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_promosyon_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_cagrilabilir_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas7", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas8", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas9", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_sat_isk_mas10", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pro_create_date", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("pro_tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("pro_testarihi", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("pro_belge_tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_DEPOLAR_ARASI_SIPARISLER()
	{
		return new TabloV16
		{
			TabloAdi = "DEPOLAR_ARASI_SIPARISLER",
			TabloID = 86,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_DEPOLAR_ARASI_SIPARISLER_00 ON DEPOLAR_ARASI_SIPARISLER (ssip_Guid)", "CREATE INDEX IF NOT EXISTS NDX_DEPOLAR_ARASI_SIPARISLER_01 ON DEPOLAR_ARASI_SIPARISLER (ssip_girdepo)" },
			Fieldlar = 
			{
				new FieldV16("ssip_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ssip_stal_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ssip_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_evrakno_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_belgeno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_projekodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_paket_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_kapatmanedenkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_stok_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_tutar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("ssip_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("ssip_teslim_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("ssip_b_fiyat", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("ssip_kapat_fl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_girdepo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_cikdepo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_SpecRECno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_fileid", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_checksum", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_create_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_lastup_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_evrakno_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_satirno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_birim_pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_iptal", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_hidden", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_kilitli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_degisti", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_fiyat_liste_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_DBCno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ssip_create_date", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_teslim_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("ssip_belge_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_DOVIZ_KURLARI()
	{
		return new TabloV16
		{
			TabloAdi = "DOVIZ_KURLARI",
			TabloID = 1007,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_DOVIZ_KURLARI_00 ON DOVIZ_KURLARI (dov_Guid)", "CREATE INDEX IF NOT EXISTS NDX_DOVIZ_KURLARI_01 ON DOVIZ_KURLARI (dov_tarih DESC)" },
			Fieldlar = 
			{
				new FieldV16("dov_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("dov_fiyat1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dov_fiyat2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dov_fiyat3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dov_fiyat4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("dov_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("dov_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_SATIS_SARTLARI()
	{
		return new TabloV16
		{
			TabloAdi = "SATIS_SARTLARI",
			TabloID = 45,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_00 ON SATIS_SARTLARI (sat_Guid)", "CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_01 ON SATIS_SARTLARI (sat_stok_kod)", "CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_02 ON SATIS_SARTLARI (sat_cari_kod)", "CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_03 ON SATIS_SARTLARI (sat_stok_kod,sat_cari_kod)", "CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_04 ON SATIS_SARTLARI (sat_stok_kod,sat_cari_kod,sat_basla_tarih,sat_bitis_tarih,sat_evrak_tarih DESC)" },
			Fieldlar = 
			{
				new FieldV16("sat_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sat_stok_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sat_cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sat_brut_fiyat", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_yuzde1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_yuzde2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_yuzde3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_yuzde4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_yuzde5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_yuzde6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_miktar1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_miktar2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_miktar3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_miktar4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_miktar5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_isk_miktar6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_yuzde1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_yuzde2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_yuzde3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_yuzde4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_miktar1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_miktar2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_miktar3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_det_mas_miktar4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sat_odeme_plan", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_depo_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_fiyat_liste_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_uyg1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_uyg2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_uyg3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_uyg4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_uyg5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_uyg6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_durum1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_durum2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_durum3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_durum4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_durum5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_isk_durum6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_uyg1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_uyg2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_uyg3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_uyg4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_durum1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_durum2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_durum3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_det_mas_durum4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sat_evrak_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sat_basla_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sat_bitis_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_SATINALMA_SARTLARI()
	{
		return new TabloV16
		{
			TabloAdi = "SATINALMA_SARTLARI",
			TabloID = 44,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_00 ON SATINALMA_SARTLARI (sas_Guid)", "CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_01 ON SATINALMA_SARTLARI (sas_stok_kod)", "CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_02 ON SATINALMA_SARTLARI (sas_cari_kod)", "CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_03 ON SATINALMA_SARTLARI (sas_stok_kod,sas_cari_kod)", "CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_04 ON SATINALMA_SARTLARI (sas_stok_kod,sas_cari_kod,sas_basla_tarih,sas_bitis_tarih,sas_evrak_tarih DESC)" },
			Fieldlar = 
			{
				new FieldV16("sas_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sas_stok_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sas_cari_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sas_brut_fiyat", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_yuzde1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_yuzde2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_yuzde3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_yuzde4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_yuzde5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_yuzde6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_miktar1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_miktar2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_miktar3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_miktar4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_miktar5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_isk_miktar6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_yuzde1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_yuzde2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_yuzde3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_yuzde4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_miktar1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_miktar2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_miktar3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_mas_miktar4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sas_odeme_plan", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_depo_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_uyg1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_uyg2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_uyg3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_uyg4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_uyg5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_uyg6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_durum1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_durum2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_durum3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_durum4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_durum5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_isk_durum6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_uyg1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_uyg2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_uyg3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_uyg4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_durum1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_durum2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_durum3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_mas_durum4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sas_evrak_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sas_basla_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sas_bitis_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_HAREKETLERI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_HAREKETLERI",
			TabloID = 16,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_00 ON STOK_HAREKETLERI (sth_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_01 ON STOK_HAREKETLERI (sth_tarih)", "CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_02 ON STOK_HAREKETLERI (sth_stok_kod)", "CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_03 ON STOK_HAREKETLERI (sth_stok_kod,sth_tip,sth_giris_depo_no,sth_cikis_depo_no,sth_cins,sth_miktar)", "CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_04 ON STOK_HAREKETLERI (sth_stok_kod,sth_cari_kodu,sth_tip,sth_normal_iade,sth_tarih)" },
			Fieldlar = 
			{
				new FieldV16("sth_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sth_sip_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sth_fat_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sth_cari_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sth_stok_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sth_evrakno_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sth_plasiyer_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sth_cari_srm_merkezi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sth_proje_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sth_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sth_tutar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_alt_doviz_kuru", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_vergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_har_doviz_kuru", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_iskonto1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_iskonto2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_iskonto3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_iskonto4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_iskonto5", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_iskonto6", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_masraf1", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_masraf2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_masraf3", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_masraf4", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_masraf_vergi", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_miktar2", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_stok_doviz_kuru", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sth_DBCno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_cari_grup_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_normal_iade", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_birim_pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_fiyat_liste_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_adres_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_giris_depo_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_cikis_depo_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_evrakno_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_cari_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_evraktip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_satirno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_vergi_pntr", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_har_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_stok_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_nakliyedeposu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_nakliyedurumu", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_isk_mas1", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_isk_mas2", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_isk_mas3", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_isk_mas4", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_isk_mas5", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_isk_mas6", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sth_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sth_belge_tarih", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("sth_lastup_date", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CIHAZ_HAREKETLERI()
	{
		return new TabloV16
		{
			TabloAdi = "CIHAZ_HAREKETLERI",
			TabloID = 98,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CIHAZ_HAREKETLERI_00 ON CIHAZ_HAREKETLERI (ChHar_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CIHAZ_HAREKETLERI_01 ON CIHAZ_HAREKETLERI (ChHar_SeriNo,ChHar_StokKodu)" },
			Fieldlar = 
			{
				new FieldV16("ChHar_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ChHar_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ChHar_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ChHar_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ChHar_SeriNo", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ChHar_StokKodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ChHar_Spec_Rec_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ChHar_master_tablo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ChHar_create_date", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("ChHar_lastup_date", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_ODEME_EMIRLERI()
	{
		return new TabloV16
		{
			TabloAdi = "ODEME_EMIRLERI",
			TabloID = 54,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_ODEME_EMIRLERI_00 ON ODEME_EMIRLERI (sck_Guid)", "CREATE INDEX IF NOT EXISTS NDX_ODEME_EMIRLERI_01 ON ODEME_EMIRLERI (sck_sahip_cari_kodu,sck_sahip_cari_cins,sck_tip,sck_sonpoz,sck_firmano)" },
			Fieldlar = 
			{
				new FieldV16("sck_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("sck_sahip_cari_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_refno", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_bankano", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_srmmrk", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_borclu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_vdaire_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_banka_adres1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_sube_adres2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_hesapno_sehir", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Sck_TCMB_Banka_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Sck_TCMB_Sube_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Sck_TCMB_il_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_ilk_evrak_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_nerede_cari_kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("sck_tutar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sck_doviz_kur", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("sck_doviz", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_sonpoz", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_sahip_cari_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_firmano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_subeno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_sahip_cari_grupno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_iptal", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_ilk_evrak_sira_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_ilk_evrak_satir_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_nerede_cari_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_imza", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("sck_vade", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_CARI_HESAP_TEMINATLARI()
	{
		return new TabloV16
		{
			TabloAdi = "CARI_HESAP_TEMINATLARI",
			TabloID = 201,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_TEMINATLARI_00 ON CARI_HESAP_TEMINATLARI (ct_Guid)", "CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_TEMINATLARI_01 ON CARI_HESAP_TEMINATLARI (ct_carikodu,ct_Aciklama_no,ct_vade)" },
			Fieldlar = 
			{
				new FieldV16("ct_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ct_carikodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ct_Aciklama_no", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ct_srmrkkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ct_tutari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("ct_iptal", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ct_DovizCinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ct_GecerliFirma", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ct_vade", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_FIRMALAR()
	{
		return new TabloV16
		{
			TabloAdi = "FIRMALAR",
			TabloID = 107,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_FIRMALAR_00 ON FIRMALAR (fir_Guid)" },
			Fieldlar = 
			{
				new FieldV16("fir_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("fir_unvan", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("fir_sirano", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_SUBELER()
	{
		return new TabloV16
		{
			TabloAdi = "SUBELER",
			TabloID = 112,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_SUBELER_00 ON SUBELER (Sube_Guid)" },
			Fieldlar = 
			{
				new FieldV16("Sube_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("Sube_adi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Sube_no", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_BANKALAR()
	{
		return new TabloV16
		{
			TabloAdi = "BANKALAR",
			TabloID = 52,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_BANKALAR_00 ON BANKALAR (ban_Guid)", "CREATE INDEX IF NOT EXISTS NDX_BANKALAR_01 ON BANKALAR (ban_kod)" },
			Fieldlar = 
			{
				new FieldV16("ban_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ban_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ban_sube", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ban_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ban_firma_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("ban_doviz_cinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_TESLIM_TURLERI()
	{
		return new TabloV16
		{
			TabloAdi = "TESLIM_TURLERI",
			TabloID = 99,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_TESLIM_TURLERI_00 ON TESLIM_TURLERI (tslt_Guid)" },
			Fieldlar = 
			{
				new FieldV16("tslt_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("tslt_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("tslt_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_MASRAF_HESAPLARI()
	{
		return new TabloV16
		{
			TabloAdi = "MASRAF_HESAPLARI",
			TabloID = 62,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_MASRAF_HESAPLARI_00 ON MASRAF_HESAPLARI (his_Guid)" },
			Fieldlar = 
			{
				new FieldV16("his_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("his_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("his_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("his_yabanci_isim", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("his_tipkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("his_sinifkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("his_grupkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("his_oivtutar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("his_dovcinsi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("his_oivuygulama", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_EVRAK_ACIKLAMALARI()
	{
		return new TabloV16
		{
			TabloAdi = "EVRAK_ACIKLAMALARI",
			TabloID = 66,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_EVRAK_ACIKLAMALARI_00 ON EVRAK_ACIKLAMALARI (egk_Guid)", "CREATE INDEX IF NOT EXISTS NDX_EVRAK_ACIKLAMALARI_01 ON EVRAK_ACIKLAMALARI (egk_dosyano,egk_hareket_tip,egk_evr_tip,egk_evr_seri,egk_evr_sira)" },
			Fieldlar = 
			{
				new FieldV16("egk_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("egk_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evr_seri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik4", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik5", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik6", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik7", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik8", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik9", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_evracik10", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("egk_dosyano", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("egk_hareket_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("egk_evr_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("egk_evr_sira", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_YEREL_BANKA_KODLARI()
	{
		return new TabloV16
		{
			TabloAdi = "YEREL_BANKA_KODLARI",
			TabloID = 127,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_YEREL_BANKA_KODLARI_00 ON YEREL_BANKA_KODLARI (bankkod_Guid)" },
			Fieldlar = 
			{
				new FieldV16("bankkod_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("bankkod_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bankkod_subekodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bankkod_ilkodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bankkod_bankadi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("bankkod_subeadi", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo__ZIYARET_HAREKETLERI()
	{
		return new TabloV16
		{
			TabloAdi = "_ZIYARET_HAREKETLERI",
			TabloID = 99990,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX__ZIYARET_HAREKETLERI_00 ON _ZIYARET_HAREKETLERI (zyrt_Guid)", "CREATE INDEX IF NOT EXISTS NDX__ZIYARET_HAREKETLERI_01 ON _ZIYARET_HAREKETLERI (zyrt_Temsilci_Kodu,zyrt_Cari_Kodu,zyrt_Cari_Adres_No,zyrt_Tarihi)" },
			Fieldlar = 
			{
				new FieldV16("zyrt_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("zyrt_Temsilci_Kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("zyrt_Cari_Kodu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("zyrt_Cari_Adres_No", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("zyrt_Tarihi", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo__FORA_PARAMETRELER()
	{
		return new TabloV16
		{
			TabloAdi = "_FORA_PARAMETRELER",
			TabloID = 99991,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX__FORA_PARAMETRELER_00 ON _FORA_PARAMETRELER (ID)", "CREATE INDEX IF NOT EXISTS NDX__FORA_PARAMETRELER_01 ON _FORA_PARAMETRELER (ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu)" },
			Fieldlar = 
			{
				new FieldV16("ID", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("ParametreProgram", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ParametreUser", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ParametreAnaGrubu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ParametreAltGrubu", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ParametreAdi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ParametreDegeri", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("ParametreID", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_BEDEN_HAREKETLERI()
	{
		return new TabloV16
		{
			TabloAdi = "BEDEN_HAREKETLERI",
			TabloID = 113,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_BEDEN_HAREKETLERI_00 ON BEDEN_HAREKETLERI (BdnHar_Guid)", "CREATE INDEX IF NOT EXISTS NDX_BEDEN_HAREKETLERI_01 ON BEDEN_HAREKETLERI (BdnHar_Tipi,BdnHar_BedenNo)" },
			Fieldlar = 
			{
				new FieldV16("BdnHar_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("BdnHar_Har_uid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("BdnHar_special1", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("BdnHar_special2", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("BdnHar_special3", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("BdnHar_HarGor", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("BdnHar_KnsIsGor", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("BdnHar_KnsFat", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("BdnHar_TesMik", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("BdnHar_rezervasyon_miktari", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("BdnHar_rezerveden_teslim_edilen", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("BdnHar_DBCno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_Spec_Rec_no", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_iptal", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_fileid", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_hidden", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_kilitli", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_degisti", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_checksum", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_create_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_lastup_user", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_Tipi", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_BedenNo", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("BdnHar_create_date", (SqlDbType)4, enum_SqliteDataType.TEXT),
				new FieldV16("BdnHar_lastup_date", (SqlDbType)4, enum_SqliteDataType.TEXT)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_STOK_PAKET_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "STOK_PAKET_TANIMLARI",
			TabloID = 106,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_PAKET_TANIMLARI_00 ON STOK_PAKET_TANIMLARI (pak_Guid)", "CREATE INDEX IF NOT EXISTS NDX_STOK_PAKET_TANIMLARI_01 ON STOK_PAKET_TANIMLARI (pak_kod)" },
			Fieldlar = 
			{
				new FieldV16("pak_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("pak_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pak_stokkod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pak_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pak_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("pak_miktar", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pak_fiyat", (SqlDbType)6, enum_SqliteDataType.REAL),
				new FieldV16("pak_satirno", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pak_vergidahilfl", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pak_master_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pak_detay_tip", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pak_doviz_cins", (SqlDbType)8, enum_SqliteDataType.INTEGER),
				new FieldV16("pak_ve_veya", (SqlDbType)8, enum_SqliteDataType.INTEGER)
			}
		};
	}

	public static TabloV16 Get_MikroV16_Tablo_KAPAMA_NEDENLERI_TANIMLARI()
	{
		return new TabloV16
		{
			TabloAdi = "KAPAMA_NEDENLERI_TANIMLARI",
			TabloID = 299,
			Indexler = { "CREATE UNIQUE INDEX IF NOT EXISTS NDX_KAPAMA_NEDENLERI_TANIMLARI_00 ON KAPAMA_NEDENLERI_TANIMLARI (Kpm_Guid)" },
			Fieldlar = 
			{
				new FieldV16("Kpm_Guid", (SqlDbType)14, enum_SqliteDataType.BLOB),
				new FieldV16("Kpm_kod", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kpm_ismi", (SqlDbType)18, enum_SqliteDataType.TEXT),
				new FieldV16("Kpm_aciklama", (SqlDbType)18, enum_SqliteDataType.TEXT)
			}
		};
	}
}
