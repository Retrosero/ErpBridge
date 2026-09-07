using System.Collections.Generic;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.Tablolar;

public static class TabloHelper
{
	public static Tablo GetTablo(string TabloAdi, List<Tablo> Tablolar)
	{
		foreach (Tablo item in Tablolar)
		{
			if (TabloAdi == item.TabloAdi)
			{
				return item;
			}
		}
		return new Tablo();
	}

	public static List<Tablo> GetMikroV14DefaultTablolar()
	{
		return new List<Tablo>
		{
			Get_MikroV14_Tablo_STOKLAR(),
			Get_MikroV14_Tablo_CARI_HESAPLAR(),
			Get_MikroV14_Tablo_SORUMLULUK_MERKEZLERI(),
			Get_MikroV14_Tablo_PROJELER(),
			Get_MikroV14_Tablo_STOK_ANA_GRUPLARI(),
			Get_MikroV14_Tablo_STOK_ALT_GRUPLARI(),
			Get_MikroV14_Tablo_STOK_URETICILERI(),
			Get_MikroV14_Tablo_STOK_REYONLARI(),
			Get_MikroV14_Tablo_STOK_MARKALARI(),
			Get_MikroV14_Tablo_STOK_CARI_ISKONTO_TANIMLARI(),
			Get_MikroV14_Tablo_STOK_SATIS_FIYAT_LISTELERI(),
			Get_MikroV14_Tablo_STOK_SATIS_FIYAT_LISTE_TANIMLARI(),
			Get_MikroV14_Tablo_BARKOD_TANIMLARI(),
			Get_MikroV14_Tablo_CARI_HESAP_ADRESLERI(),
			Get_MikroV14_Tablo_CARI_HESAP_YETKILILERI(),
			Get_MikroV14_Tablo_DEPOLAR(),
			Get_MikroV14_Tablo_ODEME_PLANLARI(),
			Get_MikroV14_Tablo_KASALAR(),
			Get_MikroV14_Tablo_CARI_HESAP_HAREKETLERI(),
			Get_MikroV14_Tablo_SIPARISLER(),
			Get_MikroV14_Tablo_PROFORMA_SIPARISLER(),
			Get_MikroV14_Tablo_DOVIZ_KURLARI(),
			Get_MikroV14_Tablo_SATIS_SARTLARI(),
			Get_MikroV14_Tablo_SATINALMA_SARTLARI(),
			Get_MikroV14_Tablo_STOK_HAREKETLERI(),
			Get_MikroV14_Tablo_ODEME_EMIRLERI(),
			Get_MikroV14_Tablo_CARI_HESAP_TEMINATLARI(),
			Get_MikroV14_Tablo_FIRMALAR(),
			Get_MikroV14_Tablo_SUBELER(),
			Get_MikroV14_Tablo_BANKALAR(),
			Get_MikroV14_Tablo_MASRAF_HESAPLARI(),
			Get_MikroV14_Tablo_YEREL_BANKA_KODLARI(),
			Get_MikroV14_Tablo__ZIYARET_HAREKETLERI(),
			Get_MikroV14_Tablo_CARI_PERSONEL_TANIMLARI(),
			Get_MikroV14_Tablo_CARI_HESAP_BOLGELERI(),
			Get_MikroV14_Tablo_CARI_HESAP_GRUPLARI(),
			Get_MikroV14_Tablo_TESLIM_TURLERI(),
			Get_MikroV14_Tablo_DEPOLAR_ARASI_SIPARISLER(),
			Get_MikroV14_Tablo_EVRAK_ACIKLAMALARI(),
			Get_MikroV14_Tablo_STOK_KATEGORILERI(),
			Get_MikroV14_Tablo_STOK_SEKTORLERI(),
			Get_MikroV14_Tablo_IHRACAT_DOSYALARI(),
			Get_MikroV14_Tablo_KAPAMA_NEDENLERI_TANIMLARI(),
			Get_MikroV14_Tablo_STOK_PAKET_TANIMLARI(),
			Get_MikroV14_Tablo_STOK_BEDEN_TANIMLARI(),
			Get_MikroV14_Tablo_STOK_RENK_TANIMLARI(),
			Get_MikroV14_Tablo_BEDEN_HAREKETLERI(),
			Get_MikroV14_Tablo_STOK_SERINO_TANIMLARI(),
			Get_MikroV14_Tablo_CIHAZ_HAREKETLERI()
		};
	}

	public static List<Tablo> GetMikroV14TeknikTesisBakimyonetimiTablolar()
	{
		return new List<Tablo>
		{
			Get_MikroV14_Tablo_SON_KULLANICILAR(),
			Get_MikroV14_Tablo_CIHAZ_GRUPLARI(),
			Get_MikroV14_Tablo_STOK_SERINO_TANIMLARI(),
			Get_MikroV14_Tablo_CIHAZ_SORUNLARI(),
			Get_MikroV14_Tablo_STOKLAR(),
			Get_MikroV14_Tablo_STOK_ANA_GRUPLARI(),
			Get_MikroV14_Tablo_ARIZA_GRUPLARI(),
			Get_MikroV14_Tablo_HIZMET_HESAPLARI(),
			Get_MikroV14_Tablo_CARI_HESAPLAR(),
			Get_MikroV14_Tablo_CARI_HESAP_BOLGELERI(),
			Get_MikroV14_Tablo_BAKIM_HAREKETLERI(),
			Get_MikroV14_Tablo_BAKIM_KABUL_HAREKETLERI(),
			Get_MikroV14_Tablo_EKIP_TANIMLARI()
		};
	}

	public static Tablo Get_MikroV14_Tablo_STOKLAR()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOKLAR";
		tablo.TabloID = 13;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOKLAR_00 ON STOKLAR (sto_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_01 ON STOKLAR (sto_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_02 ON STOKLAR (sto_isim_upper)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_03 ON STOKLAR (sto_kod_upper)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_04 ON STOKLAR (sto_anagrup_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_05 ON STOKLAR (sto_altgrup_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_06 ON STOKLAR (sto_anagrup_kod,sto_altgrup_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_07 ON STOKLAR (sto_sektor_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_08 ON STOKLAR (sto_marka_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_09 ON STOKLAR (sto_model_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_10 ON STOKLAR (sto_uretici_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_11 ON STOKLAR (sto_reyon_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_12 ON STOKLAR (sto_kategori_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_13 ON STOKLAR (sto_kategori_kodu,sto_isim_upper)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOKLAR_14 ON STOKLAR (sto_isim_upper,sto_kategori_kodu)");
		tablo.Fieldlar.Add(new Field("sto_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: true, RecNoMu: true));
		tablo.Fieldlar.Add(new Field("sto_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: false, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sto_isim,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,sto_birim4_ad,sto_beden_kodu,sto_renk_kodu,sto_altgrup_kod,sto_anagrup_kod,sto_sektor_kodu,sto_marka_kodu,sto_uretici_kodu,sto_reyon_kodu,sto_model_kodu,sto_kategori_kodu,sto_yer_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sto_birim1_katsayi,sto_birim2_katsayi,sto_birim3_katsayi,sto_birim4_katsayi,sto_standartmaliyet,sto_birim1_agirlik,sto_birim1_en,sto_birim1_boy,sto_birim1_yukseklik,sto_birim1_dara,sto_birim2_agirlik,sto_birim2_en,sto_birim2_boy,sto_birim2_yukseklik,sto_birim2_dara,sto_birim3_agirlik,sto_birim3_en,sto_birim3_boy,sto_birim3_yukseklik,sto_birim3_dara,sto_birim4_agirlik,sto_birim4_en,sto_birim4_boy,sto_birim4_yukseklik,sto_birim4_dara", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sto_detay_takip,sto_bedenli_takip,sto_perakende_vergi,sto_toptan_vergi,sto_cins,sto_doviz_cinsi,sto_webe_gonderilecek_fl,sto_renkDetayli,sto_ver_sip_birim,sto_al_sip_birim,sto_satis_dursun,sto_siparis_dursun,sto_malkabul_dursun", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_HESAPLAR()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_HESAPLAR";
		tablo.TabloID = 31;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_00 ON CARI_HESAPLAR (cari_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_01 ON CARI_HESAPLAR (cari_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_02 ON CARI_HESAPLAR (cari_unvan1_upper)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_03 ON CARI_HESAPLAR (cari_kod_upper)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_04 ON CARI_HESAPLAR (cari_grup_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_05 ON CARI_HESAPLAR (cari_temsilci_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAPLAR_06 ON CARI_HESAPLAR (cari_sektor_kodu)");
		tablo.Fieldlar.Add(new Field("cari_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: true, RecNoMu: true));
		tablo.Fieldlar.Add(new Field("cari_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: false, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("cari_unvan1,cari_unvan2,cari_muh_kod,cari_muh_kod1,cari_muh_kod2,cari_vdaire_adi,cari_vdaire_no,cari_Ana_cari_kodu,cari_bolge_kodu,cari_grup_kodu,cari_temsilci_kodu,cari_sektor_kodu,cari_satis_isk_kod,cari_special1,cari_special2,cari_special3,cari_sicil_no,cari_VergiKimlikNo,cari_banka_hesapno1,cari_CepTel,cari_EMail", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("cari_vade_fark_yuz,cari_vade_fark_yuz1,cari_vade_fark_yuz2", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("cari_doviz_cinsi,cari_doviz_cinsi1,cari_doviz_cinsi2,cari_odeme_gunu,cari_hareket_tipi,cari_odemeplan_no,cari_satis_fk,cari_KurHesapSekli,cari_odeme_cinsi,cari_cari_kilitli_flg,cari_tipi,cari_fatura_adres_no,cari_sevk_adres_no,cari_VarsayilanGirisDepo,cari_VarsayilanCikisDepo,cari_RECid_DBCno,cari_RECid_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_SORUMLULUK_MERKEZLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "SORUMLULUK_MERKEZLERI";
		tablo.TabloID = 3;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_SORUMLULUK_MERKEZLERI_00 ON SORUMLULUK_MERKEZLERI (som_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SORUMLULUK_MERKEZLERI_02 ON SORUMLULUK_MERKEZLERI (som_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SORUMLULUK_MERKEZLERI_03 ON SORUMLULUK_MERKEZLERI (som_isim)");
		tablo.Fieldlar.Add(new Field("som_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("som_kod,som_isim", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_PROJELER()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "PROJELER";
		tablo.TabloID = 176;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_PROJELER_00 ON PROJELER (pro_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_PROJELER_02 ON PROJELER (pro_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_PROJELER_03 ON PROJELER (pro_adi)");
		tablo.Fieldlar.Add(new Field("pro_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("pro_kodu,pro_adi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_BEDEN_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_BEDEN_TANIMLARI";
		tablo.TabloID = 101;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_BEDEN_TANIMLARI_00 ON STOK_BEDEN_TANIMLARI (bdn_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_BEDEN_TANIMLARI_02 ON STOK_BEDEN_TANIMLARI (bdn_kodu)");
		tablo.Fieldlar.Add(new Field("bdn_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("bdn_kodu,bdn_ismi,bdn_kirilim_1,bdn_kirilim_2,bdn_kirilim_3,bdn_kirilim_4,bdn_kirilim_5,bdn_kirilim_6,bdn_kirilim_7,bdn_kirilim_8,bdn_kirilim_9,bdn_kirilim_10,bdn_kirilim_11,bdn_kirilim_12,bdn_kirilim_13,bdn_kirilim_14,bdn_kirilim_15,bdn_kirilim_16,bdn_kirilim_17,bdn_kirilim_18,bdn_kirilim_19,bdn_kirilim_20,bdn_kirilim_21,bdn_kirilim_22,bdn_kirilim_23,bdn_kirilim_24,bdn_kirilim_25,bdn_kirilim_26,bdn_kirilim_27,bdn_kirilim_28,bdn_kirilim_29,bdn_kirilim_30,bdn_kirilim_31,bdn_kirilim_32,bdn_kirilim_33,bdn_kirilim_34,bdn_kirilim_35,bdn_kirilim_36,bdn_kirilim_37,bdn_kirilim_38,bdn_kirilim_39,bdn_kirilim_40", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_RENK_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_RENK_TANIMLARI";
		tablo.TabloID = 157;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_RENK_TANIMLARI_00 ON STOK_RENK_TANIMLARI (rnk_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_RENK_TANIMLARI_02 ON STOK_RENK_TANIMLARI (rnk_kodu)");
		tablo.Fieldlar.Add(new Field("rnk_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("rnk_kodu,rnk_ismi,rnk_kirilim_1,rnk_kirilim_2,rnk_kirilim_3,rnk_kirilim_4,rnk_kirilim_5,rnk_kirilim_6,rnk_kirilim_7,rnk_kirilim_8,rnk_kirilim_9,rnk_kirilim_10,rnk_kirilim_11,rnk_kirilim_12,rnk_kirilim_13,rnk_kirilim_14,rnk_kirilim_15,rnk_kirilim_16,rnk_kirilim_17,rnk_kirilim_18,rnk_kirilim_19,rnk_kirilim_20,rnk_kirilim_21,rnk_kirilim_22,rnk_kirilim_23,rnk_kirilim_24,rnk_kirilim_25,rnk_kirilim_26,rnk_kirilim_27,rnk_kirilim_28,rnk_kirilim_29,rnk_kirilim_30,rnk_kirilim_31,rnk_kirilim_32,rnk_kirilim_33,rnk_kirilim_34,rnk_kirilim_35,rnk_kirilim_36,rnk_kirilim_37,rnk_kirilim_38,rnk_kirilim_39,rnk_kirilim_40,rnk_kirilim_41,rnk_kirilim_42,rnk_kirilim_43,rnk_kirilim_44,rnk_kirilim_45,rnk_kirilim_46,rnk_kirilim_47,rnk_kirilim_48,rnk_kirilim_49,rnk_kirilim_50,rnk_kirilim_51,rnk_kirilim_52,rnk_kirilim_53,rnk_kirilim_54,rnk_kirilim_55,rnk_kirilim_56,rnk_kirilim_57,rnk_kirilim_58,rnk_kirilim_59,rnk_kirilim_60", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_ANA_GRUPLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_ANA_GRUPLARI";
		tablo.TabloID = 11;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_ANA_GRUPLARI_00 ON STOK_ANA_GRUPLARI (san_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_ANA_GRUPLARI_02 ON STOK_ANA_GRUPLARI (san_kod)");
		tablo.Fieldlar.Add(new Field("san_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("san_kod,san_isim", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_ALT_GRUPLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_ALT_GRUPLARI";
		tablo.TabloID = 12;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_00 ON STOK_ALT_GRUPLARI (sta_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_02 ON STOK_ALT_GRUPLARI (sta_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_03 ON STOK_ALT_GRUPLARI (sta_ana_grup_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_ALT_GRUPLARI_04 ON STOK_ALT_GRUPLARI (sta_kod,sta_ana_grup_kod)");
		tablo.Fieldlar.Add(new Field("sta_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sta_kod,sta_isim,sta_ana_grup_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_URETICILERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_URETICILERI";
		tablo.TabloID = 6;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_URETICILERI_00 ON STOK_URETICILERI (urt_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_URETICILERI_01 ON STOK_URETICILERI (urt_kod)");
		tablo.Fieldlar.Add(new Field("urt_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("urt_kod,urt_ismi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_REYONLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_REYONLARI";
		tablo.TabloID = 7;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_REYONLARI_00 ON STOK_REYONLARI (ryn_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_REYONLARI_02 ON STOK_REYONLARI (ryn_kod)");
		tablo.Fieldlar.Add(new Field("ryn_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ryn_kod,ryn_ismi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_AMBALAJLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_AMBALAJLARI";
		tablo.TabloID = 20;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_AMBALAJLARI_00 ON STOK_AMBALAJLARI (amb_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_AMBALAJLARI_02 ON STOK_AMBALAJLARI (amb_kod)");
		tablo.Fieldlar.Add(new Field("amb_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("amb_kod,amb_ismi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("amb_miktar,amb_dara", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_MARKALARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_MARKALARI";
		tablo.TabloID = 19;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_MARKALARI_00 ON STOK_MARKALARI (mrk_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_MARKALARI_02 ON STOK_MARKALARI (mrk_kod)");
		tablo.Fieldlar.Add(new Field("mrk_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("mrk_kod,mrk_ismi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_CARI_ISKONTO_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_CARI_ISKONTO_TANIMLARI";
		tablo.TabloID = 14;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_00 ON STOK_CARI_ISKONTO_TANIMLARI (isk_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_02 ON STOK_CARI_ISKONTO_TANIMLARI (isk_stok_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_03 ON STOK_CARI_ISKONTO_TANIMLARI (isk_cari_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_ISKONTO_TANIMLARI_04 ON STOK_CARI_ISKONTO_TANIMLARI (isk_stok_kod,isk_cari_kod)");
		tablo.Fieldlar.Add(new Field("isk_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("isk_stok_kod,isk_cari_kod,isk_isim,isk_isk1_aciklama,isk_isk2_aciklama,isk_isk3_aciklama,isk_isk4_aciklama,isk_isk5_aciklama,isk_isk6_aciklama,isk_mas1_aciklama,isk_mas2_aciklama,isk_mas3_aciklama,isk_mas4_aciklama", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("isk_bedelsiz_referans_miktar,isk_isk1_yuzde,isk_isk2_yuzde,isk_isk3_yuzde,isk_isk4_yuzde,isk_isk5_yuzde,isk_isk6_yuzde,isk_mas1_yuzde,isk_mas2_yuzde,isk_mas3_yuzde,isk_mas4_yuzde", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("isk_uygulama_odeme_plani,isk_isk1_uygulama,isk_isk2_uygulama,isk_isk3_uygulama,isk_isk4_uygulama,isk_isk5_uygulama,isk_isk6_uygulama,isk_mas1_uygulama,isk_mas2_uygulama,isk_mas3_uygulama,isk_mas4_uygulama", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_SATIS_FIYAT_LISTELERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_SATIS_FIYAT_LISTELERI";
		tablo.TabloID = 228;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTELERI_00 ON STOK_SATIS_FIYAT_LISTELERI (sfiyat_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTELERI_02 ON STOK_SATIS_FIYAT_LISTELERI (sfiyat_stokkod,sfiyat_listesirano,sfiyat_deposirano)");
		tablo.Fieldlar.Add(new Field("sfiyat_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sfiyat_stokkod,sfiyat_iskontokod,sfiyat_kampanyakod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sfiyat_primyuzdesi,sfiyat_fiyati", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sfiyat_listesirano,sfiyat_deposirano,sfiyat_odemeplan,sfiyat_doviz,sfiyat_deg_nedeni", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_SATIS_FIYAT_LISTE_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_SATIS_FIYAT_LISTE_TANIMLARI";
		tablo.TabloID = 227;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTE_TANIMLARI_00 ON STOK_SATIS_FIYAT_LISTE_TANIMLARI (sfl_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_SATIS_FIYAT_LISTE_TANIMLARI_02 ON STOK_SATIS_FIYAT_LISTE_TANIMLARI (sfl_sirano)");
		tablo.Fieldlar.Add(new Field("sfl_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sfl_aciklama,sfl_fiyatformul,sfl_odeplformul,sfl_sabit_iskonto,sfl_sabit_kampanya", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sfl_sabit_kur", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sfl_sirano,sfl_fiyatuygulama,sfl_odepluygulama,sfl_sabit_odeme_plani,sfl_kdvdahil,sfl_yerineuygulanacakfiyat,sfl_kurhesaplamasekli,sfl_doviz_uygulama,sfl_sabit_doviz,sfl_iskonto_uygulama,sfl_kampanya_uygulama,sfl_kampanya_vade_gozardi,sfl_kampanya_iskonto_gozardi,sfl_otvdahil,sfl_oivdahil", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sfl_ilktarih,sfl_sontarih", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_CARI_KAMPANYA_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_CARI_KAMPANYA_TANIMLARI";
		tablo.TabloID = 232;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_00 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_02 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_stok_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_03 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_cari_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_CARI_KAMPANYA_TANIMLARI_04 ON STOK_CARI_KAMPANYA_TANIMLARI (kampanya_stok_kod,kampanya_cari_kod)");
		tablo.Fieldlar.Add(new Field("kampanya_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("kampanya_stok_kod,kampanya_cari_kod,kampanya_aciklama", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("kampanya_ilave_iskonto", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("kampanya_ilave_vade,kampanya_iskonto_no", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_SEKTORLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_SEKTORLERI";
		tablo.TabloID = 8;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SEKTORLERI_00 ON STOK_SEKTORLERI (sktr_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_SEKTORLERI_02 ON STOK_SEKTORLERI (sktr_kod)");
		tablo.Fieldlar.Add(new Field("sktr_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sktr_kod,sktr_ismi,sktr_muhkodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_KATEGORILERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_KATEGORILERI";
		tablo.TabloID = 8;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_KATEGORILERI_00 ON STOK_KATEGORILERI (ktg_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_KATEGORILERI_02 ON STOK_KATEGORILERI (ktg_kod)");
		tablo.Fieldlar.Add(new Field("ktg_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ktg_kod,ktg_isim,ktg_aciklama", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_HESAP_BOLGELERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_HESAP_BOLGELERI";
		tablo.TabloID = 39;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_BOLGELERI_00 ON CARI_HESAP_BOLGELERI (bol_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_BOLGELERI_02 ON CARI_HESAP_BOLGELERI (bol_kod)");
		tablo.Fieldlar.Add(new Field("bol_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("bol_kod,bol_ismi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_HESAP_GRUPLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_HESAP_GRUPLARI";
		tablo.TabloID = 9;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_GRUPLARI_00 ON CARI_HESAP_GRUPLARI (crg_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_GRUPLARI_02 ON CARI_HESAP_GRUPLARI (crg_kod)");
		tablo.Fieldlar.Add(new Field("crg_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("crg_kod,crg_isim,crg_muhasebe_kodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_PERSONEL_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_PERSONEL_TANIMLARI";
		tablo.TabloID = 104;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_PERSONEL_TANIMLARI_00 ON CARI_PERSONEL_TANIMLARI (cari_per_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_PERSONEL_TANIMLARI_02 ON CARI_PERSONEL_TANIMLARI (cari_per_kod)");
		tablo.Fieldlar.Add(new Field("cari_per_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_cepno,cari_per_mail,cari_per_kasiyerfirmaid", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("cari_per_tip,cari_per_doviz_cinsi,cari_per_userno,cari_per_depono", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_BARKOD_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "BARKOD_TANIMLARI";
		tablo.TabloID = 15;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_BARKOD_TANIMLARI_00 ON BARKOD_TANIMLARI (bar_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_BARKOD_TANIMLARI_02 ON BARKOD_TANIMLARI (bar_kodu)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_BARKOD_TANIMLARI_03 ON BARKOD_TANIMLARI (bar_stokkodu)");
		tablo.Fieldlar.Add(new Field("bar_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("bar_kodu,bar_stokkodu,bar_partikodu,bar_serino_veya_bagkodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("bar_lotno,bar_barkodtipi,bar_icerigi,bar_birimpntr,bar_master,bar_bedenpntr,bar_renkpntr,bar_baglantitipi,bar_harrecid_dbcno,bar_harrecid_recno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_ASORTI_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "ASORTI_TANIMLARI";
		tablo.TabloID = 172;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_ASORTI_TANIMLARI_00 ON ASORTI_TANIMLARI (Asorti_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_ASORTI_TANIMLARI_02 ON ASORTI_TANIMLARI (Asorti_StokKodu)");
		tablo.Fieldlar.Add(new Field("Asorti_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("Asorti_StokKodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("Asorti_Miktar", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("Asorti_BedenNo", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_HESAP_ADRESLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_HESAP_ADRESLERI";
		tablo.TabloID = 32;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_ADRESLERI_00 ON CARI_HESAP_ADRESLERI (adr_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_ADRESLERI_02 ON CARI_HESAP_ADRESLERI (adr_cari_kod)");
		tablo.Fieldlar.Add(new Field("adr_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("adr_cari_kod,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_yon_kodu,adr_temsilci_kodu,adr_ozel_not", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("adr_ziyaretgunu,adr_gps_enlem,adr_gps_boylam", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("adr_adres_no,adr_uzaklik_kodu,adr_ziyaretperyodu,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_HESAP_YETKILILERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_HESAP_YETKILILERI";
		tablo.TabloID = 33;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_YETKILILERI_00 ON CARI_HESAP_YETKILILERI (mye_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_YETKILILERI_02 ON CARI_HESAP_YETKILILERI (mye_cari_kod)");
		tablo.Fieldlar.Add(new Field("mye_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("mye_cari_kod,mye_isim,mye_soyisim,mye_es_isim,mye_dahili_telno,mye_email_adres,mye_cep_telno,mye_tc_kimlikno,mye_vergi_dairesi,mye_vergi_kimlikno,mye_dogum_yeri,mye_ev_cadde,mye_ev_sokak,mye_ev_posta_kodu,mye_ev_ilce,mye_ev_il,mye_ev_ulke,mye_is_telno,mye_ev_telno", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("mye_adres_no,mye_unvan,mye_hitap,mye_hisse,mye_tahsil", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("mye_dogum_tarihi,mye_evlilik_tarihi,mye_es_dogum_tarihi", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_DEPOLAR()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "DEPOLAR";
		tablo.TabloID = 111;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_DEPOLAR_00 ON DEPOLAR (dep_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_DEPOLAR_02 ON DEPOLAR (dep_no)");
		tablo.Fieldlar.Add(new Field("dep_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("dep_adi,dep_grup_kodu,dep_muh_kodu,dep_sor_mer_kodu,dep_proje_kodu,dep_cadde,dep_sokak,dep_posta_Kodu,dep_Ilce,dep_Il,dep_Ulke,dep_yetkili_email,dep_dizin_adi,dep_tel_ulke_kodu,dep_tel_bolge_kodu,dep_tel_no1,dep_tel_no2,dep_tel_faxno,dep_tel_modem,dep_barkod_yazici_yolu,dep_fason_sor_mer_kodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("dep_kamyon_kasa_hacmi,dep_kamyon_istiab_haddi,dep_satis_alani,dep_sergi_alani,dep_otopark_alani,dep_gps_enlem,dep_gps_boylam,dep_alani,dep_rafhacmi", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("dep_firmano,dep_subeno,dep_no,dep_tipi,dep_DepoSevkOtoFiyat,dep_hareket_tipi,dep_DepoSevkUygFiyat,dep_otopark_kapasite,dep_kasa_sayisi,dep_envanter_harici_fl,dep_detay_takibi,dep_EksiyeDusurenStkHar,dep_BagliOrtakliklaraSatisUygFiyat", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_IHRACAT_DOSYALARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "IHRACAT_DOSYALARI";
		tablo.TabloID = 122;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_IHRACAT_DOSYALARI_00 ON IHRACAT_DOSYALARI (ihr_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_IHRACAT_DOSYALARI_02 ON IHRACAT_DOSYALARI (ihr_kodu)");
		tablo.Fieldlar.Add(new Field("ihr_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ihr_kodu,ihr_ismi,ihr_Satici", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ihr_RECid_DBCno,ihr_RECid_RECno,ihr_SpecRecNo,ihr_firmano,ihr_subeno,ihr_TeslimSekli,ihr_OdemeSekli,ihr_carigrupno,ihr_DovizCinsi", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ihr_create_date,ihr_lastup_date,ihr_GCB_Tarihi,ihr_Intac_Tarihi", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV15_Tablo_KUR_ISIMLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "KUR_ISIMLERI";
		tablo.TabloID = 1020;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_KUR_ISIMLERI_00 ON KUR_ISIMLERI (Kur_RECno)");
		tablo.Fieldlar.Add(new Field("Kur_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("Kur_Tip,Kur_sembol,Kur_adi,Kur_orjAdi,Kur_kusurat_isim,Kur_kusurat_sembol", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("Kur_No,Kur_decimal", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_ITHALAT_DOSYALARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "ITHALAT_DOSYALARI";
		tablo.TabloID = 119;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_ITHALAT_DOSYALARI_00 ON ITHALAT_DOSYALARI (ith_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_ITHALAT_DOSYALARI_02 ON ITHALAT_DOSYALARI (ith_kodu)");
		tablo.Fieldlar.Add(new Field("ith_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ith_kodu,ith_ismi,ith_satici,ith_ulkekodu,ith_gumrukkodu,ith_Araci_Banka,ith_GGB_no,ith_vasitano,ith_nakliyeci,ith_gumrukmusaviri,ith_MuhKodu_1,ith_MuhKodu_2,ith_MuhKodu_3,ith_MuhKodu_4,ith_MuhKodu_5,ith_MuhKodu_6,ith_MuhKodu_7,ith_MuhKodu_8,ith_MuhKodu_9,ith_MuhKodu_10,ith_MuhGrupKodu,ith_MalBedeliMuhKodu,ith_Mense_ulkekodu,ith_Araci_CariKodu,ith_Akreditif,ith_kilitli_fl", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ith_firmano,ith_subeno,ith_ulketipi,ith_teslimsekli,ith_odemesekli,ith_carigrupno,ith_dovizcinsi,ith_tasimasekli,ith_MalDagitimSekli1,ith_MalDagitimSekli2,ith_MalDagitimSekli3,ith_MalDagitimSekli4,ith_MalDagitimSekli5,ith_MalDagitimSekli6,ith_MalDagitimSekli7,ith_MalDagitimSekli8,ith_MalDagitimSekli9,ith_MalDagitimSek10,ith_Mense_ulketipi", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ith_GGB_tarihi", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_ODEME_PLANLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "ODEME_PLANLARI";
		tablo.TabloID = 72;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_ODEME_PLANLARI_00 ON ODEME_PLANLARI (odp_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_ODEME_PLANLARI_02 ON ODEME_PLANLARI (odp_kodu)");
		tablo.Fieldlar.Add(new Field("odp_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("odp_kodu,odp_adi,odp_aratop,odp_masraf,odp_vergi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("odp_no,odp_ortgun", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_KASALAR()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "KASALAR";
		tablo.TabloID = 53;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_KASALAR_00 ON KASALAR (kas_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_KASALAR_02 ON KASALAR (kas_kod)");
		tablo.Fieldlar.Add(new Field("kas_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("kas_kod,kas_isim,kas_muh_kod,kas_bankakodu,kas_nakakincelenmesi,kas_ufrs_muh_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("kas_tip,kas_firma_no,kas_doviz_cinsi", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_SERINO_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_SERINO_TANIMLARI";
		tablo.TabloID = 94;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_SERINO_TANIMLARI_00 ON STOK_SERINO_TANIMLARI (chz_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_SERINO_TANIMLARI_02 ON STOK_SERINO_TANIMLARI (chz_serino)");
		tablo.Fieldlar.Add(new Field("chz_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("chz_serino,chz_stok_kodu,chz_grup_kodu,chz_Tuktckodu,chz_aciklama1,chz_aciklama2,chz_aciklama3,chz_al_evr_seri,chz_al_cari_kodu,chz_st_evr_seri,chz_st_cari_kodu,chz_parca_garantisi,chz_parca_serino", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("chz_brut_fiati,chz_al_fiati_ana,chz_al_fiati_alt,chz_al_fiati_orj,chz_st_fiati_ana,chz_st_fiati_alt,chz_st_fiati_orj", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("chz_al_evr_sira,chz_st_evr_sira", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("chz_GrnBasTarihi,chz_GrnBitTarihi,chz_al_tarih,chz_st_tarih,chz_parca_garanti_baslangic,chz_parca_garanti_bitis", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_HESAP_HAREKETLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_HESAP_HAREKETLERI";
		tablo.TabloID = 51;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_00 ON CARI_HESAP_HAREKETLERI (cha_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_02 ON CARI_HESAP_HAREKETLERI (cha_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_03 ON CARI_HESAP_HAREKETLERI (cha_ciro_cari_kodu,cha_tarihi,cha_tip,cha_normal_Iade,cha_evrak_tip,cha_grupno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_04 ON CARI_HESAP_HAREKETLERI (cha_cari_cins,cha_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_HAREKETLERI_05 ON CARI_HESAP_HAREKETLERI (cha_kasa_hizmet,cha_kasa_hizkod)");
		tablo.Fieldlar.Add(new Field("cha_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("cha_special1,cha_special2,cha_special3,cha_aciklama,cha_projekodu,cha_satici_kodu,cha_srmrkkodu,cha_trefno,cha_ciro_cari_kodu,cha_evrakno_seri,cha_belge_no,cha_kod,cha_kasa_hizkod,cha_EXIMkodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("cha_ft_iskonto1,cha_ft_iskonto2,cha_ft_iskonto3,cha_ft_iskonto4,cha_ft_iskonto5,cha_ft_iskonto6,cha_meblag,cha_d_kur,cha_miktari,cha_aratoplam,cha_ft_masraf1,cha_ft_masraf2,cha_ft_masraf3,cha_ft_masraf4,cha_otvtutari,cha_vergi1,cha_vergi2,cha_vergi3,cha_vergi4,cha_vergi5,cha_vergi6,cha_vergi7,cha_vergi8,cha_vergi9,cha_vergi10,cha_altd_kur,cha_karsid_kur", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("cha_RECid_RECno,cha_tpoz,cha_vergipntr,cha_kasa_hizmet,cha_firmano,cha_subeno,cha_tip,cha_cinsi,cha_normal_Iade,cha_evrak_tip,cha_satir_no,cha_evrakno_sira,cha_cari_cins,cha_vade,cha_grupno,cha_d_cins,cha_ticaret_turu", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("cha_tarihi,cha_belge_tarih,cha_lastup_date", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_SIPARISLER()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "SIPARISLER";
		tablo.TabloID = 21;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_SIPARISLER_00 ON SIPARISLER (sip_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_02 ON SIPARISLER (sip_tip,sip_stok_kod,sip_depono)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_03 ON SIPARISLER (sip_musteri_kod,sip_kapat_fl,sip_tip)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_04 ON SIPARISLER (sip_musteri_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SIPARISLER_05 ON SIPARISLER (sip_evrakno_seri,sip_evrakno_sira,sip_cins,sip_tip)");
		tablo.Fieldlar.Add(new Field("sip_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sip_musteri_kod,sip_special1,sip_special2,sip_special3,sip_evrakno_seri,sip_belgeno,sip_satici_kod,sip_aciklama,sip_aciklama2,sip_cari_sormerk,sip_stok_sormerk,sip_teslimturu,sip_Exp_Imp_Kodu,sip_parti_kodu,sip_projekodu,sip_paket_kod,sip_kapatmanedenkod,sip_stok_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sip_tutar,sip_iskonto_1,sip_iskonto_2,sip_iskonto_3,sip_iskonto_4,sip_iskonto_5,sip_iskonto_6,sip_masraf_1,sip_masraf_2,sip_masraf_3,sip_masraf_4,sip_vergi,sip_masvergi,sip_doviz_kuru,sip_miktar,sip_teslim_miktar,sip_b_fiyat,sip_alt_doviz_kuru,sip_kar_orani,sip_planlananmiktar,sip_Otv_Vergi,sip_otvtutari", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sip_kapat_fl,sip_tip,sip_depono,sip_RECid_DBCno,sip_RECid_RECno,sip_SpecRECno,sip_fileid,sip_checksum,sip_create_user,sip_lastup_user,sip_firmano,sip_subeno,sip_cins,sip_evrakno_sira,sip_satirno,sip_birim_pntr,sip_vergi_pntr,sip_masvergi_pntr,sip_opno,sip_OnaylayanKulNo,sip_cari_grupno,sip_doviz_cinsi,sip_adresno,sip_prosiprecDbId,sip_prosiprecrecI,sip_iskonto1,sip_iskonto2,sip_iskonto3,sip_iskonto4,sip_iskonto5,sip_iskonto6,sip_masraf1,sip_masraf2,sip_masraf3,sip_masraf4,sip_durumu,sip_stalRecId_DBCno,sip_stalRecId_RECno,sip_teklifRecId_DBCno,sip_teklifRecId_RECno,sip_lot_no,sip_fiyat_liste_no,sip_Otv_Pntr,sip_OtvVergisiz_Fl,sip_RezRecId_DBCno,sip_RezRecId_RECno,sip_harekettipi,sip_yetkili_recid_dbcno,sip_yetkili_recid_recno,sip_iptal,sip_hidden,sip_kilitli,sip_degisti,sip_vergisiz_fl,sip_promosyon_fl,sip_cagrilabilir_fl,sip_isk1,sip_isk2,sip_isk3,sip_isk4,sip_isk5,sip_isk6,sip_mas1,sip_mas2,sip_mas3,sip_mas4", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sip_create_date,sip_tarih,sip_teslim_tarih,sip_belge_tarih", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_PROFORMA_SIPARISLER()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "PROFORMA_SIPARISLER";
		tablo.TabloID = 22;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_00 ON PROFORMA_SIPARISLER (pro_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_02 ON PROFORMA_SIPARISLER (pro_tipi,pro_stokkodu,pro_depono)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_03 ON PROFORMA_SIPARISLER (pro_mustkodu,pro_kapat,pro_tipi)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_PROFORMA_SIPARISLER_04 ON PROFORMA_SIPARISLER (pro_mustkodu)");
		tablo.Fieldlar.Add(new Field("pro_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("pro_mustkodu,pro_special1,pro_special2,pro_special3,pro_evrakno_seri,pro_belge_no,pro_saticikodu,pro_aciklama,pro_aciklama2,pro_cari_sormerk,pro_stok_sormerk,pro_teslimturu,pro_Exp_Imp_Kodu,pro_parti_kodu,pro_projekodu,pro_paket_kod,pro_kapatmanedenkod,pro_stokkodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("pro_tutari,pro_iskonto1,pro_iskonto2,pro_iskonto3,pro_iskonto4,pro_iskonto5,pro_iskonto6,pro_masraf1,pro_masraf2,pro_masraf3,pro_masraf4,pro_vergi,pro_masrafvergi,pro_dovizkuru,pro_miktar,pro_tesmiktari,pro_bfiyati,pro_altdovizkuru,pro_karoani,pro_planlananmiktar,pro_Otv_Vergi,pro_otvtutari", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("pro_kapat,pro_tipi,pro_depono,pro_RECid_DBCno,pro_RECid_RECno,pro_SpecRecNo,pro_fileid,pro_checksum,pro_create_user,pro_lastup_user,pro_firmano,pro_subeno,pro_cinsi,pro_evrakno_sira,pro_satirno,pro_birim_pntr,pro_vergipntr,pro_masrafvergipntr,pro_opno,pro_onaylayanKul_no,pro_cari_grupno,pro_dovizcinsi,pro_adresno,pro_isk_mas_1,pro_isk_mas_2,pro_isk_mas_3,pro_isk_mas_4,pro_isk_mas_5,pro_isk_mas_6,pro_isk_mas_7,pro_isk_mas_8,pro_isk_mas_9,pro_isk_mas_10,pro_durumu,pro_stalRecId_DBCno,pro_stalRecId_RECno,pro_teklifRecId_DBCno,pro_teklifRecId_RECno,pro_lot_no,pro_fiyat_liste_no,pro_Otv_Pntr,pro_OtvVergisiz_Fl,pro_RezRecId_DBCno,pro_RezRecId_RECno,pro_harekettipi,pro_yetkili_recid_dbcno,pro_yetkili_recid_recno,pro_iptal,pro_hidden,pro_kilitli,pro_degisti,pro_vergisiz,pro_promosyon_fl,pro_cagrilabilir_fl,pro_sipDbID,pro_sipRecID,pro_sat_isk_mas1,pro_sat_isk_mas2,pro_sat_isk_mas3,pro_sat_isk_mas4,pro_sat_isk_mas5,pro_sat_isk_mas6,pro_sat_isk_mas7,pro_sat_isk_mas8,pro_sat_isk_mas9,pro_sat_isk_mas10", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("pro_create_date,pro_tarihi,pro_testarihi,pro_belge_tarihi", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_DEPOLAR_ARASI_SIPARISLER()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "DEPOLAR_ARASI_SIPARISLER";
		tablo.TabloID = 86;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_DEPOLAR_ARASI_SIPARISLER_00 ON DEPOLAR_ARASI_SIPARISLER (ssip_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_DEPOLAR_ARASI_SIPARISLER_02 ON DEPOLAR_ARASI_SIPARISLER (ssip_girdepo)");
		tablo.Fieldlar.Add(new Field("ssip_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ssip_special1,ssip_special2,ssip_special3,ssip_evrakno_seri,ssip_belgeno,ssip_aciklama,ssip_projekodu,ssip_paket_kod,ssip_kapatmanedenkod,ssip_stok_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ssip_tutar,ssip_miktar,ssip_teslim_miktar,ssip_b_fiyat", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ssip_kapat_fl,ssip_girdepo,ssip_cikdepo,ssip_RECid_DBCno,ssip_RECid_RECno,ssip_SpecRECno,ssip_fileid,ssip_checksum,ssip_create_user,ssip_lastup_user,ssip_firmano,ssip_subeno,ssip_evrakno_sira,ssip_satirno,ssip_birim_pntr,ssip_iptal,ssip_hidden,ssip_kilitli,ssip_degisti,ssip_stalRecId_DBCno,ssip_stalRecId_RECno,ssip_fiyat_liste_no", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ssip_create_date,ssip_tarih,ssip_teslim_tarih,ssip_belge_tarih", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_DOVIZ_KURLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "DOVIZ_KURLARI";
		tablo.TabloID = 1007;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_DOVIZ_KURLARI_00 ON DOVIZ_KURLARI (dov_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_DOVIZ_KURLARI_02 ON DOVIZ_KURLARI (dov_tarih DESC)");
		tablo.Fieldlar.Add(new Field("dov_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("dov_fiyat1,dov_fiyat2,dov_fiyat3,dov_fiyat4", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("dov_no", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("dov_tarih", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_SATIS_SARTLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "SATIS_SARTLARI";
		tablo.TabloID = 45;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_00 ON SATIS_SARTLARI (sat_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_02 ON SATIS_SARTLARI (sat_stok_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_03 ON SATIS_SARTLARI (sat_cari_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_04 ON SATIS_SARTLARI (sat_stok_kod,sat_cari_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATIS_SARTLARI_05 ON SATIS_SARTLARI (sat_stok_kod,sat_cari_kod,sat_basla_tarih,sat_bitis_tarih,sat_evrak_tarih DESC)");
		tablo.Fieldlar.Add(new Field("sat_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sat_stok_kod,sat_cari_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sat_brut_fiyat,sat_det_isk_yuzde1,sat_det_isk_yuzde2,sat_det_isk_yuzde3,sat_det_isk_yuzde4,sat_det_isk_yuzde5,sat_det_isk_yuzde6,sat_det_isk_miktar1,sat_det_isk_miktar2,sat_det_isk_miktar3,sat_det_isk_miktar4,sat_det_isk_miktar5,sat_det_isk_miktar6,sat_det_mas_yuzde1,sat_det_mas_yuzde2,sat_det_mas_yuzde3,sat_det_mas_yuzde4,sat_det_mas_miktar1,sat_det_mas_miktar2,sat_det_mas_miktar3,sat_det_mas_miktar4", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sat_odeme_plan,sat_doviz_cinsi,sat_depo_no,sat_fiyat_liste_no,sat_det_isk_uyg1,sat_det_isk_uyg2,sat_det_isk_uyg3,sat_det_isk_uyg4,sat_det_isk_uyg5,sat_det_isk_uyg6,sat_det_isk_durum1,sat_det_isk_durum2,sat_det_isk_durum3,sat_det_isk_durum4,sat_det_isk_durum5,sat_det_isk_durum6,sat_det_mas_uyg1,sat_det_mas_uyg2,sat_det_mas_uyg3,sat_det_mas_uyg4,sat_det_mas_durum1,sat_det_mas_durum2,sat_det_mas_durum3,sat_det_mas_durum4", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sat_evrak_tarih,sat_basla_tarih,sat_bitis_tarih", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_SATINALMA_SARTLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "SATINALMA_SARTLARI";
		tablo.TabloID = 44;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_00 ON SATINALMA_SARTLARI (sas_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_02 ON SATINALMA_SARTLARI (sas_stok_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_03 ON SATINALMA_SARTLARI (sas_cari_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_04 ON SATINALMA_SARTLARI (sas_stok_kod,sas_cari_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_SATINALMA_SARTLARI_05 ON SATINALMA_SARTLARI (sas_stok_kod,sas_cari_kod,sas_basla_tarih,sas_bitis_tarih,sas_evrak_tarih DESC)");
		tablo.Fieldlar.Add(new Field("sas_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sas_stok_kod,sas_cari_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sas_brut_fiyat,sas_isk_yuzde1,sas_isk_yuzde2,sas_isk_yuzde3,sas_isk_yuzde4,sas_isk_yuzde5,sas_isk_yuzde6,sas_isk_miktar1,sas_isk_miktar2,sas_isk_miktar3,sas_isk_miktar4,sas_isk_miktar5,sas_isk_miktar6,sas_mas_yuzde1,sas_mas_yuzde2,sas_mas_yuzde3,sas_mas_yuzde4,sas_mas_miktar1,sas_mas_miktar2,sas_mas_miktar3,sas_mas_miktar4", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sas_odeme_plan,sas_doviz_cinsi,sas_depo_no,sas_isk_uyg1,sas_isk_uyg2,sas_isk_uyg3,sas_isk_uyg4,sas_isk_uyg5,sas_isk_uyg6,sas_isk_durum1,sas_isk_durum2,sas_isk_durum3,sas_isk_durum4,sas_isk_durum5,sas_isk_durum6,sas_mas_uyg1,sas_mas_uyg2,sas_mas_uyg3,sas_mas_uyg4,sas_mas_durum1,sas_mas_durum2,sas_mas_durum3,sas_mas_durum4", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sas_evrak_tarih,sas_basla_tarih,sas_bitis_tarih", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_HAREKETLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_HAREKETLERI";
		tablo.TabloID = 16;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_00 ON STOK_HAREKETLERI (sth_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_02 ON STOK_HAREKETLERI (sth_tarih)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_12 ON STOK_HAREKETLERI (sth_stok_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_13 ON STOK_HAREKETLERI (sth_stok_kod,sth_tip,sth_giris_depo_no,sth_cikis_depo_no,sth_cins,sth_miktar)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_HAREKETLERI_14 ON STOK_HAREKETLERI (sth_stok_kod,sth_cari_kodu,sth_tip,sth_normal_iade,sth_tarih)");
		tablo.Fieldlar.Add(new Field("sth_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sth_cari_kodu,sth_stok_kod,sth_evrakno_seri,sth_plasiyer_kodu,sth_cari_srm_merkezi,sth_proje_kodu,sth_aciklama", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sth_tutar,sth_alt_doviz_kuru,sth_vergi,sth_har_doviz_kuru,sth_iskonto1,sth_iskonto2,sth_iskonto3,sth_iskonto4,sth_iskonto5,sth_iskonto6,sth_masraf1,sth_masraf2,sth_masraf3,sth_masraf4,sth_masraf_vergi,sth_miktar,sth_miktar2,sth_stok_doviz_kuru", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sth_cari_grup_no,sth_firmano,sth_subeno,sth_normal_iade,sth_birim_pntr,sth_fiyat_liste_no,sth_adres_no,sth_tip,sth_giris_depo_no,sth_cikis_depo_no,sth_evrakno_sira,sth_cari_cinsi,sth_evraktip,sth_satirno,sth_fat_recid_dbcno,sth_fat_recid_recno,sth_sip_recid_dbcno,sth_sip_recid_recno,sth_cins,sth_vergi_pntr,sth_har_doviz_cinsi,sth_stok_doviz_cinsi,sth_nakliyedeposu,sth_nakliyedurumu,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sth_tarih,sth_belge_tarih,sth_lastup_date", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CIHAZ_HAREKETLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CIHAZ_HAREKETLERI";
		tablo.TabloID = 98;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CIHAZ_HAREKETLERI_00 ON CIHAZ_HAREKETLERI (ChHar_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CIHAZ_HAREKETLERI_02 ON CIHAZ_HAREKETLERI (ChHar_SeriNo,ChHar_StokKodu)");
		tablo.Fieldlar.Add(new Field("ChHar_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ChHar_special1,ChHar_special2,ChHar_special3,ChHar_SeriNo,ChHar_StokKodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ChHar_RECid_DBCno,ChHar_RECid_RECno,ChHar_Spec_Rec_no,ChHar_master_tablo,ChHar_master_dbcno,ChHar_master_recno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ChHar_create_date,ChHar_lastup_date", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_ODEME_EMIRLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "ODEME_EMIRLERI";
		tablo.TabloID = 54;
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_ODEME_EMIRLERI_02 ON ODEME_EMIRLERI (sck_sahip_cari_kodu,sck_sahip_cari_cins,sck_tip,sck_sonpoz,sck_firmano)");
		tablo.Fieldlar.Add(new Field("sck_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("sck_sahip_cari_kodu,sck_refno,sck_bankano,sck_srmmrk,sck_borclu,sck_vdaire_no,sck_banka_adres1,sck_sube_adres2,sck_hesapno_sehir,sck_no,Sck_TCMB_Banka_kodu,Sck_TCMB_Sube_kodu,Sck_TCMB_il_kodu,sck_ilk_evrak_seri,sck_nerede_cari_kodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sck_tutar,sck_doviz_kur", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sck_doviz,sck_sonpoz,sck_sahip_cari_cins,sck_tip,sck_firmano,sck_subeno,sck_sahip_cari_grupno,sck_iptal,sck_ilk_evrak_sira_no,sck_ilk_evrak_satir_no,sck_nerede_cari_cins,sck_imza", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("sck_vade", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CARI_HESAP_TEMINATLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CARI_HESAP_TEMINATLARI";
		tablo.TabloID = 201;
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_CARI_HESAP_TEMINATLARI_02 ON CARI_HESAP_TEMINATLARI (ct_carikodu,ct_Aciklama_no,ct_vade)");
		tablo.Fieldlar.Add(new Field("ct_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ct_carikodu,ct_Aciklama_no,ct_srmrkkodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ct_tutari", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ct_iptal,ct_DovizCinsi,ct_GecerliFirma", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ct_vade", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_FIRMALAR()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "FIRMALAR";
		tablo.TabloID = 107;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_FIRMALAR_00 ON FIRMALAR (fir_RECno)");
		tablo.Fieldlar.Add(new Field("fir_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("fir_unvan", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("fir_sirano", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_SUBELER()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "SUBELER";
		tablo.TabloID = 112;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_SUBELER_00 ON SUBELER (Sube_RECno)");
		tablo.Fieldlar.Add(new Field("Sube_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("Sube_adi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("Sube_no", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_BANKALAR()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "BANKALAR";
		tablo.TabloID = 52;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_BANKALAR_00 ON BANKALAR (ban_RECno)");
		tablo.Fieldlar.Add(new Field("ban_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ban_ismi,ban_sube,ban_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ban_firma_no,ban_doviz_cinsi", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_TESLIM_TURLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "TESLIM_TURLERI";
		tablo.TabloID = 99;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_TESLIM_TURLERI_00 ON TESLIM_TURLERI (tslt_RECno)");
		tablo.Fieldlar.Add(new Field("tslt_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("tslt_kod,tslt_ismi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_MASRAF_HESAPLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "MASRAF_HESAPLARI";
		tablo.TabloID = 62;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_MASRAF_HESAPLARI_00 ON MASRAF_HESAPLARI (his_RECno)");
		tablo.Fieldlar.Add(new Field("his_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("his_kod,his_isim,his_yabanci_isim,his_tipkod,his_sinifkod,his_grupkod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("his_oivtutar", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("his_dovcinsi,his_oivuygulama", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_EVRAK_ACIKLAMALARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "EVRAK_ACIKLAMALARI";
		tablo.TabloID = 66;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_EVRAK_ACIKLAMALARI_00 ON EVRAK_ACIKLAMALARI (egk_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_EVRAK_ACIKLAMALARI_01 ON EVRAK_ACIKLAMALARI (egk_dosyano,egk_hareket_tip,egk_evr_tip,egk_evr_seri,egk_evr_sira)");
		tablo.Fieldlar.Add(new Field("egk_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("egk_special1,egk_special2,egk_special3,egk_evr_seri,egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("egk_dosyano,egk_hareket_tip,egk_evr_tip,egk_evr_sira", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_YEREL_BANKA_KODLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "YEREL_BANKA_KODLARI";
		tablo.TabloID = 127;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_YEREL_BANKA_KODLARI_00 ON YEREL_BANKA_KODLARI (bankkod_RECno)");
		tablo.Fieldlar.Add(new Field("bankkod_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("bankkod_kod,bankkod_subekodu,bankkod_ilkodu,bankkod_bankadi,bankkod_subeadi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo__ZIYARET_HAREKETLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "_ZIYARET_HAREKETLERI";
		tablo.TabloID = 99990;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX__ZIYARET_HAREKETLERI_00 ON _ZIYARET_HAREKETLERI (zyrt_RecNo)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX__ZIYARET_HAREKETLERI_02 ON _ZIYARET_HAREKETLERI (zyrt_Temsilci_Kodu,zyrt_Cari_Kodu,zyrt_Cari_Adres_No,zyrt_Tarihi)");
		tablo.Fieldlar.Add(new Field("zyrt_RecNo", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("zyrt_Temsilci_Kodu,zyrt_Cari_Kodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("zyrt_Cari_Adres_No", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("zyrt_Tarihi", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo__FORA_PARAMETRELER()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "_FORA_PARAMETRELER";
		tablo.TabloID = 99991;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX__FORA_PARAMETRELER_00 ON _FORA_PARAMETRELER (ID)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX__FORA_PARAMETRELER_01 ON _FORA_PARAMETRELER (ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu)");
		tablo.Fieldlar.Add(new Field("ID", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu,ParametreAdi,ParametreDegeri", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("ParametreID", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_SON_KULLANICILAR()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "SON_KULLANICILAR";
		tablo.TabloID = 95;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_SON_KULLANICILAR_00 ON SON_KULLANICILAR (tuk_RECno)");
		tablo.Fieldlar.Add(new Field("tuk_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("tuk_RECid_DBCno,tuk_RECid_RECno,tuk_Spec_Rec_no,tuk_iptal,tuk_fileid,tuk_hidden,tuk_kilitli,tuk_degisti,tuk_checksum,tuk_create_user,tuk_create_date,tuk_lastup_user,tuk_lastup_date,tuk_special1,tuk_special2,tuk_special3,tuk_kodu,tuk_ismi,tuk_Adr1_Cadde,tuk_Adr1_Sokak,tuk_Adr1_Postakodu,tuk_Adr1_Ilce,tuk_Adr1_Il,tuk_Adr1_Ulke,tuk_Adr2_Cadde,tuk_Adr2_Sokak,tuk_Adr2_Postakodu,tuk_Adr2_Ilce,tuk_Adr2_Il,tuk_Adr2_Ulke,tuk_Tel1_Ulkekod,tuk_Tel1_Bolgekod,tuk_Tel1_TelNo1,tuk_Tel1_TelNo2,tuk_Tel1_FaxNo,tuk_Tel1_ModemNo,tuk_Tel2_Ulkekod,tuk_Tel2_Bolgekod,tuk_Tel2_TelNo1,tuk_Tel2_TelNo2,tuk_Tel2_FaxNo,tuk_Tel2_ModemNo,tuk_yetkili1,tuk_yetkili2,tuk_ceptel1,tuk_ceptel2,tuk_email1,tuk_email2,tuk_GrpKodu,tuk_MuhKodu,tuk_kilitli_flg,tuk_cari_kodu,tuk_sektor_kodu,tuk_bolge_kodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CIHAZ_GRUPLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CIHAZ_GRUPLARI";
		tablo.TabloID = 268;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CIHAZ_GRUPLARI_00 ON CIHAZ_GRUPLARI (cg_RECno)");
		tablo.Fieldlar.Add(new Field("cg_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("cg_RECid_DBCno,cg_RECid_RECno,cg_SpecRECno,cg_iptal,cg_fileid,cg_hidden,cg_kilitli,cg_degisti,cg_checksum,cg_create_user,cg_create_date,cg_lastup_user,cg_lastup_date,cg_special1,cg_special2,cg_special3,cg_kodu,cg_aciklama", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_CIHAZ_SORUNLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "CIHAZ_SORUNLARI";
		tablo.TabloID = 96;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_CIHAZ_SORUNLARI_00 ON CIHAZ_GRUPLARI (chs_RECno)");
		tablo.Fieldlar.Add(new Field("chs_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("chs_RECid_DBCno,chs_RECid_RECno,chs_Spec_Rec_no,chs_iptal,chs_fileid,chs_hidden,chs_kilitli,chs_degisti,chs_checksum,chs_create_user,chs_create_date,chs_lastup_user,chs_lastup_date,chs_special1,chs_special2,chs_special3,chs_kodu,chs_cihaz,chs_sorun,chs_stok_ana_grup_kodu,chs_grup_kodu,chs_sinif_kodu,chs_sorun_giderme_süresi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_ARIZA_GRUPLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "ARIZA_GRUPLARI";
		tablo.TabloID = 263;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_ARIZA_GRUPLARI_00 ON ARIZA_GRUPLARI (agr_RECno)");
		tablo.Fieldlar.Add(new Field("agr_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("agr_RECid_DBCno,agr_RECid_RECno,agr_SpecRECno,agr_iptal,agr_fileid,agr_hidden,agr_kilitli,agr_degisti,agr_checksum,agr_create_user,agr_create_date,agr_lastup_user,agr_lastup_date,agr_special1,agr_special2,agr_special3,agr_kodu,agr_adi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_HIZMET_HESAPLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "HIZMET_HESAPLARI";
		tablo.TabloID = 61;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_HIZMET_HESAPLARI_00 ON HIZMET_HESAPLARI (hiz_RECno)");
		tablo.Fieldlar.Add(new Field("hiz_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_HIZMET_HESAPLARI_01 ON HIZMET_HESAPLARI (hiz_kod)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_HIZMET_HESAPLARI_02 ON HIZMET_HESAPLARI (hiz_isim)");
		tablo.Fieldlar.AddRange(AddFields("hiz_RECid_DBCno,hiz_RECid_RECno,hiz_SpecRecno,hiz_iptal,hiz_fileid,hiz_hidden,hiz_kilitli,hiz_degisti,hiz_checksum,hiz_create_user,hiz_create_date,hiz_lastup_user,hiz_lastup_date,hiz_special1,hiz_special2,hiz_special3,hiz_tip,hiz_kod,hiz_isim,hiz_yabanci_isim,hiz_tipkod,hiz_sinifkod,hiz_grupkod,hiz_sat_muh_kod,hiz_sat_iade_muh_kod,hiz_mal_muh_kod,hiz_sat_mal_muh_kod,hiz_mal_yan_muh_kod,hiz_fiyat,hiz_doviz_cinsi,hiz_isk_grup,hiz_KDV,hiz_muh_sat_isk_kod,hiz_muh_aIiskmuhkod,hiz_ilavemasmuhkod,hiz_operasyon_suresi,hiz_oivuygulama,hiz_oivtutar", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_BAKIM_KABUL_HAREKETLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "BAKIM_KABUL_HAREKETLERI";
		tablo.TabloID = 147;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_BAKIM_KABUL_HAREKETLERI_00 ON BAKIM_KABUL_HAREKETLERI (bkmkb_RECno)");
		tablo.Fieldlar.Add(new Field("bkmkb_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("bkmkb_RECid_DBCno,bkmkb_RECid_RECno,bkmkb_Spec_Rec_no,bkmkb_iptal,bkmkb_fileid,bkmkb_hidden,bkmkb_kilitli,bkmkb_degisti,bkmkb_checksum,bkmkb_create_user,bkmkb_create_date,bkmkb_lastup_user,bkmkb_lastup_date,bkmkb_special1,bkmkb_special2,bkmkb_special3,bkmkb_firmano,bkmkb_subeno,bkmkb_tarihi,bkmkb_evrakno_seri,bkmkb_evrakno_sira,bkmkb_satirno,bkmkb_belgeno,bkmkb_belge_tarihi,bkmkb_cihaz_serino,bkmkb_fis_stok_kodu,bkmkb_tuketici_kodu,bkmkb_talep_gelis_sekli,bkmkb_gelis_kargo_kodu,bkmkb_gelis_kargo_belgeno,bkmkb_gelis_irsaliyeno,bkmkb_servis_turu,bkmkb_servis_yeri,bkmkb_aksesuarlar,bkmkb_bildirilen_arizalar,bkmkb_teslim_alinma_tarihi,bkmkb_teslim_edilme_tarihi,bkmkb_teslim_edilme_sekli,bkmkb_ariza_kodu1,bkmkb_ariza_kodu2,bkmkb_ariza_kodu3,bkmkb_ariza_kodu4,bkmkb_ariza_kodu5,bkmkb_ariza_kodu6,bkmkb_ariza_kodu7,bkmkb_ariza_kodu8,bkmkb_ariza_kodu9,bkmkb_ariza_kodu10,bkmkb_bilgilendirme_sekli,bkmkb_inceleyecek_ekip_kodu,bkmkb_depono,bkmkb_aciklama,bkmkb_hareket_tipi,bkmkb_stok_hizmet_kodu,bkmkb_operasyon_suresi,bkmkb_miktari,bkmkb_satir_aciklama,bkmkb_planlandi_fl", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_BAKIM_HAREKETLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "BAKIM_HAREKETLERI";
		tablo.TabloID = 97;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_BAKIM_HAREKETLERI_00 ON BAKIM_HAREKETLERI (bkm_RECno)");
		tablo.Fieldlar.Add(new Field("bkm_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("bkm_RECid_DBCno,bkm_RECid_RECno,bkm_Spec_Rec_no,bkm_iptal,bkm_fileid,bkm_hidden,bkm_kilitli,bkm_degisti,bkm_checksum,bkm_create_user,bkm_create_date,bkm_lastup_user,bkm_lastup_date,bkm_special1,bkm_special2,bkm_special3,bkm_firmano,bkm_subeno,bkm_tarihi,bkm_evrakno_seri,bkm_evrakno_sira,bkm_satirno,bkm_belgeno,bkm_belge_tarihi,bkm_tuketici_kodu,bkm_cihaz_serino,bkm_fis_stok_kodu,bkm_teslim_alinma_tarihi,bkm_teslim_edilme_tarihi,bkm_teslim_edilme_sekli,bkm_ariza_kodu1,bkm_ariza_kodu2,bkm_ariza_kodu3,bkm_ariza_kodu4,bkm_ariza_kodu5,bkm_ariza_kodu6,bkm_ariza_kodu7,bkm_ariza_kodu8,bkm_ariza_kodu9,bkm_ariza_kodu10,bkm_ekip_kodu,bkm_depono,bkm_aciklama,bkm_hareket_tipi,bkm_stok_hizmet_kodu,bkm_miktari,bkm_birim_fiyati,bkm_tutari,bkm_iskonto1,bkm_iskonto2,bkm_iskonto3,bkm_iskonto4,bkm_iskonto5,bkm_iskonto6,bkm_masraf1,bkm_masraf2,bkm_masraf3,bkm_masraf4,bkm_vergi_pntr,bkm_vergi,bkm_masraf_vergi_pnt,bkm_masraf_vergi,bkm_isk_mas1,bkm_isk_mas2,bkm_isk_mas3,bkm_isk_mas4,bkm_isk_mas5,bkm_isk_mas6,bkm_isk_mas7,bkm_isk_mas8,bkm_isk_mas9,bkm_isk_mas10,bkm_sat_isk_mas1,bkm_sat_isk_mas2,bkm_sat_isk_mas3,bkm_sat_isk_mas4,bkm_sat_isk_mas5,bkm_sat_isk_mas6,bkm_sat_isk_mas7,bkm_sat_isk_mas8,bkm_sat_isk_mas9,bkm_sat_isk_mas10,bkm_doviz_cins,bkm_doviz_kur,bkm_alt_doviz_kur,bkm_vergisiz_fl,bkm_satir_aciklama,bkm_faturalandi_fl,bkm_ziyaret_kodu,bkm_ziy_ac_tar,bkm_ziy_cik_zmn,bkm_ziy_bas_zmn,bkm_ziy_son_zmn,bkm_ziy_don_zmn,bkm_kabul_RECid_DBCno,bkm_kabul_RECid_RECno,bkm_isemri_RECid_DBCno,bkm_isemri_RECid_RECno,bkm_cihazdurumbastarihi1,bkm_cihazdurumbittarihi1,bkm_cihazdurumkodu1,bkm_cihazserviselemanikodu1,bkm_cihazdurumbastarihi2,bkm_cihazdurumbittarihi2,bkm_cihazdurumkodu2,bkm_cihazserviselemanikodu2,bkm_cihazdurumbastarihi3,bkm_cihazdurumbittarihi3,bkm_cihazdurumkodu3,bkm_cihazserviselemanikodu3,bkm_cihazdurumbastarihi4,bkm_cihazdurumbittarihi4,bkm_cihazdurumkodu4,bkm_cihazserviselemanikodu4,bkm_cihazdurumbastarihi5,bkm_cihazdurumbittarihi5,bkm_cihazdurumkodu5,bkm_cihazserviselemanikodu5,bkm_cihazdurumbastarihi6,bkm_cihazdurumbittarihi6,bkm_cihazdurumkodu6,bkm_cihazserviselemanikodu6,bkm_cihazdurumbastarihi7,bkm_cihazdurumbittarihi7,bkm_cihazdurumkodu7,bkm_cihazserviselemanikodu7,bkm_cihazdurumbastarihi8,bkm_cihazdurumbittarihi8,bkm_cihazdurumkodu8,bkm_cihazserviselemanikodu8,bkm_cihazdurumbastarihi9,bkm_cihazdurumbittarihi9,bkm_cihazdurumkodu9,bkm_cihazserviselemanikodu9,bkm_cihazdurumbastarihi10,bkm_cihazdurumbittarihi10,bkm_cihazdurumkodu10,bkm_cihazserviselemanikodu10,bkm_fiyat_liste_no,bkm_parti_kodu,bkm_lot_no,bkm_servis_turu,bkm_prj_kodu,bkm_srm_kodu", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_EKIP_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "EKIP_TANIMLARI";
		tablo.TabloID = 260;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_EKIP_TANIMLARI_00 ON EKIP_TANIMLARI (ekp_RECno)");
		tablo.Fieldlar.Add(new Field("ekp_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("ekp_RECid_DBCno,ekp_RECid_RECno,ekp_SpecRECNo,ekp_iptal,ekp_fileid,ekp_hidden,ekp_kilitli,ekp_degisti,ekp_CheckSum,ekp_create_user,ekp_create_date,ekp_lastup_user,ekp_lastup_date,ekp_special1,ekp_special2,ekp_special3,ekp_kodu,ekp_adi,ekp_cari_kodu,ekp_personel_kodu1,ekp_personel_agirlik_puan1,ekp_personel_kodu2,ekp_personel_agirlik_puan2,ekp_personel_kodu3,ekp_personel_agirlik_puan3,ekp_personel_kodu4,ekp_personel_agirlik_puan4,ekp_personel_kodu5,ekp_personel_agirlik_puan5,ekp_personel_kodu6,ekp_personel_agirlik_puan6,ekp_personel_kodu7,ekp_personel_agirlik_puan7,ekp_personel_kodu8,ekp_personel_agirlik_puan8,ekp_personel_kodu9,ekp_personel_agirlik_puan9,ekp_personel_kodu10,ekp_personel_agirlik_puan10", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_BEDEN_HAREKETLERI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "BEDEN_HAREKETLERI";
		tablo.TabloID = 113;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_BEDEN_HAREKETLERI_00 ON BEDEN_HAREKETLERI (BdnHar_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_BEDEN_HAREKETLERI_01 ON BEDEN_HAREKETLERI (BdnHar_Tipi,BdnHar_BedenNo)");
		tablo.Fieldlar.Add(new Field("BdnHar_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("BdnHar_HarGor,BdnHar_KnsIsGor,BdnHar_KnsFat,BdnHar_TesMik,BdnHar_rezervasyon_miktari,BdnHar_rezerveden_teslim_edilen", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("BdnHar_RECid_DBCno,BdnHar_RECid_RECno,BdnHar_Spec_Rec_no,BdnHar_iptal,BdnHar_fileid,BdnHar_hidden,BdnHar_kilitli,BdnHar_degisti,BdnHar_checksum,BdnHar_create_user,BdnHar_lastup_user,BdnHar_Tipi,BdnHar_DRECid_DBCno,BdnHar_DRECid_RECno,BdnHar_BedenNo", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("BdnHar_special1,BdnHar_special2,BdnHar_special3", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("BdnHar_create_date,BdnHar_lastup_date", enum_sqlite_data_tip.TEXT, datetimemi: true, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_STOK_PAKET_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "STOK_PAKET_TANIMLARI";
		tablo.TabloID = 106;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_STOK_PAKET_TANIMLARI_00 ON STOK_PAKET_TANIMLARI (pak_RECno)");
		tablo.Indexler.Add("CREATE INDEX IF NOT EXISTS NDX_STOK_PAKET_TANIMLARI_02 ON STOK_PAKET_TANIMLARI (pak_kod)");
		tablo.Fieldlar.Add(new Field("pak_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: false, Primary: true, RecNoMu: true));
		tablo.Fieldlar.AddRange(AddFields("pak_kod,pak_stokkod,pak_aciklama,pak_ismi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("pak_miktar,pak_fiyat", enum_sqlite_data_tip.DOUBLE, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("pak_satirno,pak_vergidahilfl,pak_master_tip,pak_detay_tip,pak_doviz_cins,pak_ve_veya", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	public static Tablo Get_MikroV14_Tablo_KAPAMA_NEDENLERI_TANIMLARI()
	{
		Tablo tablo = new Tablo();
		tablo.TabloAdi = "KAPAMA_NEDENLERI_TANIMLARI";
		tablo.TabloID = 299;
		tablo.Indexler.Add("CREATE UNIQUE INDEX IF NOT EXISTS NDX_KAPAMA_NEDENLERI_TANIMLARI_00 ON KAPAMA_NEDENLERI_TANIMLARI (Kpm_RECno)");
		tablo.Fieldlar.Add(new Field("Kpm_RECno", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: true, RecNoMu: true));
		tablo.Fieldlar.Add(new Field("Kpm_kod", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: false, Primary: false, RecNoMu: false));
		tablo.Fieldlar.AddRange(AddFields("Kpm_ismi,Kpm_aciklama", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false));
		return tablo;
	}

	private static List<Field> AddFields(string Fieldlar, enum_sqlite_data_tip FieldTipiSqLite, bool datetimemi, bool Cekilecek, bool Null, bool Primary, bool RecNoMu)
	{
		List<Field> list = new List<Field>();
		string[] array = Fieldlar.Split(new char[1] { ',' });
		foreach (string adi in array)
		{
			Field field = new Field();
			field.Adi = adi;
			field.TipiSqLite = FieldTipiSqLite;
			field.DateTimeMi = datetimemi;
			field.NullOlurmu = Null;
			field.PrimaryKey = Primary;
			field.FieldRecNoMu = RecNoMu;
			list.Add(field);
		}
		return list;
	}
}
