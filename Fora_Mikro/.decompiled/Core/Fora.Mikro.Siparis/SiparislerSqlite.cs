using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Siparis;

public static class SiparislerSqlite
{
	public static List<SIPARISLER> GetSiparisSatirlariByCariKod(SqliteConnection OpenedConnection, string cari_kod, enum_sip_orderby Siralama, int Limit, string SearchString, siparis_teslim_durumu teslim_durumu, enum_sip_tip sip_tip, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		if (AppBase.MikroVersiyonu > 15)
		{
			return V16_GetSiparisSatirlariByCariKod(OpenedConnection, cari_kod, Siralama, Limit, SearchString, teslim_durumu, sip_tip, cha_grupno, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu);
		}
		return V15_GetSiparisSatirlariByCariKod(OpenedConnection, cari_kod, Siralama, Limit, SearchString, teslim_durumu, sip_tip, cha_grupno, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu);
	}

	public static List<SIPARISLER> V15_GetSiparisSatirlariByCariKod(SqliteConnection OpenedConnection, string cari_kod, enum_sip_orderby Siralama, int Limit, string SearchString, siparis_teslim_durumu teslim_durumu, enum_sip_tip sip_tip, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		string text = "";
		if (cha_grupno != -1)
		{
			text += " AND sip_cari_grupno=@sip_cari_grupno ";
		}
		if (FirmaNo != -1)
		{
			text += " AND sip_firmano=@sip_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND sip_subeno=@sip_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND sip_cari_sormerk=@sip_cari_sormerk ";
		}
		List<SIPARISLER> list = new List<SIPARISLER>();
		string text2 = "SELECT sip_RECno,sip_tarih,sip_teslim_tarih,sip_evrakno_seri,sip_evrakno_sira,sip_satirno,sip_musteri_kod,sip_stok_kod,sip_b_fiyat,sip_miktar,sip_birim_pntr,sip_teslim_miktar,sip_tutar,sip_iskonto_1,sip_iskonto_2,sip_iskonto_3,sip_iskonto_4,sip_iskonto_5,sip_iskonto_6,sip_masraf_1,sip_masraf_2,sip_masraf_3,sip_masraf_4,sip_vergi_pntr,sip_vergi,sip_masvergi_pntr,sip_masvergi,sip_aciklama,sip_aciklama2,sip_depono,sip_OnaylayanKulNo,sip_vergisiz_fl,sip_doviz_cinsi,sip_doviz_kuru,sip_alt_doviz_kuru,sip_iskonto1,sip_iskonto2,sip_iskonto3,sip_iskonto4,sip_iskonto5,sip_iskonto6,sip_masraf1,sip_masraf2,sip_masraf3,sip_masraf4,sip_isk1,sip_isk2,sip_isk3,sip_isk4,sip_isk5,sip_isk6,sip_mas1,sip_mas2,sip_mas3,sip_mas4,sip_durumu,sip_Otv_Pntr,sip_Otv_Vergi,sip_otvtutari,sip_OtvVergisiz_Fl,sto_isim,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,sto_birim4_ad,sto_birim1_katsayi,sto_birim2_katsayi,sto_birim3_katsayi,sto_birim4_katsayi,sip_RECid_DBCno,sip_RECid_RECno,sip_kapat_fl,sto_bedenli_takip,sto_renkDetayli FROM SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod";
		text2 = text2 + " WHERE sip_tip=@sip_tip " + text;
		string text3 = "AND";
		if (cari_kod != "")
		{
			text2 = text2 + " " + text3 + " sip_musteri_kod=@sip_musteri_kod";
			text3 = "AND";
		}
		if (SearchString != "")
		{
			string text4 = "%" + SearchString.ToUpper().Replace(" ", "%") + "%";
			text2 = text2 + " " + text3 + " ((STOKLAR.sto_kod_upper like '" + text4 + "') OR (STOKLAR.sto_isim_upper like '" + text4 + "'))";
			text3 = "AND";
		}
		switch (teslim_durumu)
		{
		case siparis_teslim_durumu.Bekleyenler:
			text2 = text2 + " " + text3 + " (sip_miktar>sip_teslim_miktar AND sip_kapat_fl=0)";
			text3 = "AND";
			break;
		case siparis_teslim_durumu.TeslimEdilenler:
			text2 = text2 + " " + text3 + " sip_miktar=sip_teslim_miktar";
			text3 = "AND";
			break;
		case siparis_teslim_durumu.Vazgecilenler:
			text2 = text2 + " " + text3 + " sip_kapat_fl=1";
			text3 = "AND";
			break;
		}
		switch (Siralama)
		{
		case enum_sip_orderby.SeriSira:
			text2 += " ORDER BY sip_evrakno_seri,sip_evrakno_sira";
			break;
		case enum_sip_orderby.SiparisTarihi:
			text2 += " ORDER BY sip_tarih DESC,sip_evrakno_seri,sip_evrakno_sira DESC";
			break;
		case enum_sip_orderby.TeslimTarihi:
			text2 += " ORDER BY sip_teslim_tarih DESC,sip_evrakno_seri,sip_evrakno_sira DESC";
			break;
		}
		text2 = text2 + " LIMIT " + Limit;
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = text2;
				if (cari_kod != "")
				{
					val.Parameters.AddWithValue("@sip_musteri_kod", (object)cari_kod);
					val.Parameters.AddWithValue("@sip_tip", (object)sip_tip);
					if (cha_grupno != -1)
					{
						val.Parameters.AddWithValue("@sip_cari_grupno", (object)cha_grupno);
					}
					if (FirmaNo != -1)
					{
						val.Parameters.AddWithValue("@sip_firmano", (object)FirmaNo);
					}
					if (SubeNo != -1)
					{
						val.Parameters.AddWithValue("@sip_subeno", (object)SubeNo);
					}
					if (SorumlulukMerkeziDetayli)
					{
						val.Parameters.AddWithValue("@sip_cari_sormerk", (object)SorumlulukMerkeziKodu);
					}
				}
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SIPARISLER sIPARISLER = new SIPARISLER();
					sIPARISLER.sip_RECno = val2.GetSafeInt32(0);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(1);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(2);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(3);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(4);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(5);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(6);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(7);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(8);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(9);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(10);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(11);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(12);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(13);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(14);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(15);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(16);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(17);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(18);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(19);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(20);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(21);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(22);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(23);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(24);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(25);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(26);
					sIPARISLER.sip_aciklama = val2.GetSafeString(27);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(28);
					sIPARISLER.sip_depono = val2.GetSafeInt32(29);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(30);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(31);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(32);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(33);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(35);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(36);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(37);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(38);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(39);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(40);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(41);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(42);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(43);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(44);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(45);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(46);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(47);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(48);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(49);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(50);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(51);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(52);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(53);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(54);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(55);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(56);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(57);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(58);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(59);
					sIPARISLER.sto_isim = val2.GetSafeString(60);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(61);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(62);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(63);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(64);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(65);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(66);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(67);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(68);
					sIPARISLER.sip_RECid_DBCno = val2.GetSafeInt32(69);
					sIPARISLER.sip_RECid_RECno = val2.GetSafeInt32(70);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(71);
					bool safeBoolean = val2.GetSafeBoolean(72);
					bool safeBoolean2 = val2.GetSafeBoolean(73);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V15_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.Siparis, sIPARISLER.sip_RECid_DBCno, sIPARISLER.sip_RECid_RECno);
					}
					list.Add(sIPARISLER);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return list;
	}

	public static List<SIPARISLER> V16_GetSiparisSatirlariByCariKod(SqliteConnection OpenedConnection, string cari_kod, enum_sip_orderby Siralama, int Limit, string SearchString, siparis_teslim_durumu teslim_durumu, enum_sip_tip sip_tip, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		string text = "";
		if (cha_grupno != -1)
		{
			text += " AND sip_cari_grupno=@sip_cari_grupno ";
		}
		if (FirmaNo != -1)
		{
			text += " AND sip_firmano=@sip_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND sip_subeno=@sip_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND sip_cari_sormerk=@sip_cari_sormerk ";
		}
		List<SIPARISLER> list = new List<SIPARISLER>();
		string text2 = "SELECT sip_Guid,sip_tarih,sip_teslim_tarih,sip_evrakno_seri,sip_evrakno_sira,sip_satirno,sip_musteri_kod,sip_stok_kod,sip_b_fiyat,sip_miktar,sip_birim_pntr,sip_teslim_miktar,sip_tutar,sip_iskonto_1,sip_iskonto_2,sip_iskonto_3,sip_iskonto_4,sip_iskonto_5,sip_iskonto_6,sip_masraf_1,sip_masraf_2,sip_masraf_3,sip_masraf_4,sip_vergi_pntr,sip_vergi,sip_masvergi_pntr,sip_masvergi,sip_aciklama,sip_aciklama2,sip_depono,sip_OnaylayanKulNo,sip_vergisiz_fl,sip_doviz_cinsi,sip_doviz_kuru,sip_alt_doviz_kuru,sip_iskonto1,sip_iskonto2,sip_iskonto3,sip_iskonto4,sip_iskonto5,sip_iskonto6,sip_masraf1,sip_masraf2,sip_masraf3,sip_masraf4,sip_isk1,sip_isk2,sip_isk3,sip_isk4,sip_isk5,sip_isk6,sip_mas1,sip_mas2,sip_mas3,sip_mas4,sip_durumu,sip_Otv_Pntr,sip_Otv_Vergi,sip_otvtutari,sip_OtvVergisiz_Fl,sto_isim,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,sto_birim4_ad,sto_birim1_katsayi,sto_birim2_katsayi,sto_birim3_katsayi,sto_birim4_katsayi,sip_kapat_fl,sto_bedenli_takip,sto_renkDetayli FROM SIPARISLER INNER JOIN STOKLAR ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod";
		text2 = text2 + " WHERE sip_tip=@sip_tip " + text;
		string text3 = "AND";
		if (cari_kod != "")
		{
			text2 = text2 + " " + text3 + " sip_musteri_kod=@sip_musteri_kod";
			text3 = "AND";
		}
		if (SearchString != "")
		{
			string text4 = "%" + SearchString.ToUpper().Replace(" ", "%") + "%";
			text2 = text2 + " " + text3 + " ((STOKLAR.sto_kod_upper like '" + text4 + "') OR (STOKLAR.sto_isim_upper like '" + text4 + "'))";
			text3 = "AND";
		}
		switch (teslim_durumu)
		{
		case siparis_teslim_durumu.Bekleyenler:
			text2 = text2 + " " + text3 + " (sip_miktar>sip_teslim_miktar AND sip_kapat_fl=0)";
			text3 = "AND";
			break;
		case siparis_teslim_durumu.TeslimEdilenler:
			text2 = text2 + " " + text3 + " sip_miktar=sip_teslim_miktar";
			text3 = "AND";
			break;
		case siparis_teslim_durumu.Vazgecilenler:
			text2 = text2 + " " + text3 + " sip_kapat_fl=1";
			text3 = "AND";
			break;
		}
		switch (Siralama)
		{
		case enum_sip_orderby.SeriSira:
			text2 += " ORDER BY sip_evrakno_seri,sip_evrakno_sira";
			break;
		case enum_sip_orderby.SiparisTarihi:
			text2 += " ORDER BY sip_tarih DESC,sip_evrakno_seri,sip_evrakno_sira DESC";
			break;
		case enum_sip_orderby.TeslimTarihi:
			text2 += " ORDER BY sip_teslim_tarih DESC,sip_evrakno_seri,sip_evrakno_sira DESC";
			break;
		}
		text2 = text2 + " LIMIT " + Limit;
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = text2;
				if (cari_kod != "")
				{
					val.Parameters.AddWithValue("@sip_musteri_kod", (object)cari_kod);
					val.Parameters.AddWithValue("@sip_tip", (object)sip_tip);
					if (cha_grupno != -1)
					{
						val.Parameters.AddWithValue("@sip_cari_grupno", (object)cha_grupno);
					}
					if (FirmaNo != -1)
					{
						val.Parameters.AddWithValue("@sip_firmano", (object)FirmaNo);
					}
					if (SubeNo != -1)
					{
						val.Parameters.AddWithValue("@sip_subeno", (object)SubeNo);
					}
					if (SorumlulukMerkeziDetayli)
					{
						val.Parameters.AddWithValue("@sip_cari_sormerk", (object)SorumlulukMerkeziKodu);
					}
				}
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SIPARISLER sIPARISLER = new SIPARISLER();
					sIPARISLER.sip_Guid = ((DbDataReader)val2).GetGuid(0);
					sIPARISLER.sip_tarih = val2.GetSafeDateTime(1);
					sIPARISLER.sip_teslim_tarih = val2.GetSafeDateTime(2);
					sIPARISLER.sip_evrakno_seri = val2.GetSafeString(3);
					sIPARISLER.sip_evrakno_sira = val2.GetSafeInt32(4);
					sIPARISLER.sip_satirno = val2.GetSafeInt32(5);
					sIPARISLER.sip_musteri_kod = val2.GetSafeString(6);
					sIPARISLER.sip_stok_kod = val2.GetSafeString(7);
					sIPARISLER.sip_b_fiyat = val2.GetSafeDouble(8);
					sIPARISLER.sip_miktar = val2.GetSafeDouble(9);
					sIPARISLER.sip_birim_pntr = val2.GetSafeByte(10);
					sIPARISLER.sip_teslim_miktar = val2.GetSafeDouble(11);
					sIPARISLER.sip_tutar = val2.GetSafeDouble(12);
					sIPARISLER.sip_iskonto_1 = val2.GetSafeDouble(13);
					sIPARISLER.sip_iskonto_2 = val2.GetSafeDouble(14);
					sIPARISLER.sip_iskonto_3 = val2.GetSafeDouble(15);
					sIPARISLER.sip_iskonto_4 = val2.GetSafeDouble(16);
					sIPARISLER.sip_iskonto_5 = val2.GetSafeDouble(17);
					sIPARISLER.sip_iskonto_6 = val2.GetSafeDouble(18);
					sIPARISLER.sip_masraf_1 = val2.GetSafeDouble(19);
					sIPARISLER.sip_masraf_2 = val2.GetSafeDouble(20);
					sIPARISLER.sip_masraf_3 = val2.GetSafeDouble(21);
					sIPARISLER.sip_masraf_4 = val2.GetSafeDouble(22);
					sIPARISLER.sip_vergi_pntr = val2.GetSafeByte(23);
					sIPARISLER.sip_vergi = val2.GetSafeDouble(24);
					sIPARISLER.sip_masvergi_pntr = val2.GetSafeByte(25);
					sIPARISLER.sip_masvergi = val2.GetSafeDouble(26);
					sIPARISLER.sip_aciklama = val2.GetSafeString(27);
					sIPARISLER.sip_aciklama2 = val2.GetSafeString(28);
					sIPARISLER.sip_depono = val2.GetSafeInt32(29);
					sIPARISLER.sip_OnaylayanKulNo = val2.GetSafeInt16(30);
					sIPARISLER.sip_vergisiz_fl = val2.GetSafeBoolean(31);
					sIPARISLER.sip_doviz_cinsi = val2.GetSafeByte(32);
					sIPARISLER.sip_doviz_kuru = val2.GetSafeDouble(33);
					sIPARISLER.sip_alt_doviz_kuru = val2.GetSafeDouble(34);
					sIPARISLER.sip_iskonto1 = val2.GetSafeByte(35);
					sIPARISLER.sip_iskonto2 = val2.GetSafeByte(36);
					sIPARISLER.sip_iskonto3 = val2.GetSafeByte(37);
					sIPARISLER.sip_iskonto4 = val2.GetSafeByte(38);
					sIPARISLER.sip_iskonto5 = val2.GetSafeByte(39);
					sIPARISLER.sip_iskonto6 = val2.GetSafeByte(40);
					sIPARISLER.sip_masraf1 = val2.GetSafeByte(41);
					sIPARISLER.sip_masraf2 = val2.GetSafeByte(42);
					sIPARISLER.sip_masraf3 = val2.GetSafeByte(43);
					sIPARISLER.sip_masraf4 = val2.GetSafeByte(44);
					sIPARISLER.sip_isk1 = val2.GetSafeBoolean(45);
					sIPARISLER.sip_isk2 = val2.GetSafeBoolean(46);
					sIPARISLER.sip_isk3 = val2.GetSafeBoolean(47);
					sIPARISLER.sip_isk4 = val2.GetSafeBoolean(48);
					sIPARISLER.sip_isk5 = val2.GetSafeBoolean(49);
					sIPARISLER.sip_isk6 = val2.GetSafeBoolean(50);
					sIPARISLER.sip_mas1 = val2.GetSafeBoolean(51);
					sIPARISLER.sip_mas2 = val2.GetSafeBoolean(52);
					sIPARISLER.sip_mas3 = val2.GetSafeBoolean(53);
					sIPARISLER.sip_mas4 = val2.GetSafeBoolean(54);
					sIPARISLER.sip_durumu = (enum_sip_durumu)val2.GetSafeByte(55);
					sIPARISLER.sip_Otv_Pntr = val2.GetSafeByte(56);
					sIPARISLER.sip_Otv_Vergi = val2.GetSafeDouble(57);
					sIPARISLER.sip_otvtutari = val2.GetSafeDouble(58);
					sIPARISLER.sip_OtvVergisiz_Fl = val2.GetSafeByte(59);
					sIPARISLER.sto_isim = val2.GetSafeString(60);
					sIPARISLER.sto_birim1_ad = val2.GetSafeString(61);
					sIPARISLER.sto_birim2_ad = val2.GetSafeString(62);
					sIPARISLER.sto_birim3_ad = val2.GetSafeString(63);
					sIPARISLER.sto_birim4_ad = val2.GetSafeString(64);
					sIPARISLER.sto_birim1_katsayi = val2.GetSafeDouble(65);
					sIPARISLER.sto_birim2_katsayi = val2.GetSafeDouble(66);
					sIPARISLER.sto_birim3_katsayi = val2.GetSafeDouble(67);
					sIPARISLER.sto_birim4_katsayi = val2.GetSafeDouble(68);
					sIPARISLER.sip_kapat_fl = val2.GetSafeBoolean(69);
					bool safeBoolean = val2.GetSafeBoolean(70);
					bool safeBoolean2 = val2.GetSafeBoolean(71);
					if (safeBoolean || safeBoolean2)
					{
						sIPARISLER.renk_beden_hareketleri = StokSqlite.V16_GetStokRenkBedenHareketleri(OpenedConnection, enum_BdnHar_Tipi.Siparis, sIPARISLER.sip_Guid);
					}
					list.Add(sIPARISLER);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return list;
	}

	public static List<SiparisEvrakListItem> GetSiparisEvrakList(SqliteConnection OpenedConnection, enum_sip_tip SiparisTipi, enum_sip_cins SiparisCinsi, string SiparisSeri, string CariKodu, enum_sip_orderby Siralama, siparis_teslim_durumu teslim_durumu)
	{
		List<SiparisEvrakListItem> list = new List<SiparisEvrakListItem>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string text = "SELECT sip_evrakno_seri,sip_evrakno_sira,sip_tarih,MIN(sip_teslim_tarih) AS sip_teslim_tarih,sip_musteri_kod,SUM(sip_miktar) AS sip_miktar,SUM(sip_teslim_miktar) AS sip_teslim_miktar,sip_tip,sip_cins FROM SIPARISLER WHERE ";
				string text2 = text;
				int num = (int)SiparisTipi;
				text = text2 + "sip_tip=" + num + " ";
				string text3 = text;
				num = (int)SiparisCinsi;
				text = text3 + "AND sip_cins=" + num + " ";
				if (SiparisSeri != "")
				{
					text += "AND sip_evrakno_seri=@sip_evrakno_seri ";
					val.Parameters.AddWithValue("@sip_evrakno_seri", (object)SiparisSeri);
				}
				if (CariKodu != "")
				{
					text += "AND sip_musteri_kod=@sip_musteri_kod ";
					val.Parameters.AddWithValue("@sip_musteri_kod", (object)CariKodu);
				}
				switch (teslim_durumu)
				{
				case siparis_teslim_durumu.Bekleyenler:
					text += " AND (sip_miktar>sip_teslim_miktar AND sip_kapat_fl=0)";
					break;
				case siparis_teslim_durumu.TeslimEdilenler:
					text += " AND sip_miktar=sip_teslim_miktar";
					break;
				case siparis_teslim_durumu.Vazgecilenler:
					text += " AND sip_kapat_fl=1";
					break;
				}
				text += " GROUP BY sip_evrakno_seri,sip_evrakno_sira,sip_tarih,sip_musteri_kod,sip_tip,sip_cins";
				switch (Siralama)
				{
				case enum_sip_orderby.SeriSira:
					text += " ORDER BY sip_evrakno_seri,sip_evrakno_sira";
					break;
				case enum_sip_orderby.SiparisTarihi:
					text += " ORDER BY sip_tarih DESC,sip_evrakno_seri,sip_evrakno_sira DESC";
					break;
				case enum_sip_orderby.TeslimTarihi:
					text += " ORDER BY sip_teslim_tarih DESC,sip_evrakno_seri,sip_evrakno_sira DESC";
					break;
				}
				((DbCommand)val).CommandText = text;
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					SiparisEvrakListItem siparisEvrakListItem = new SiparisEvrakListItem();
					siparisEvrakListItem.sip_evrakno_seri = val2.GetSafeString(0);
					siparisEvrakListItem.sip_evrakno_sira = val2.GetSafeInt32(1);
					siparisEvrakListItem.sip_tarih = val2.GetSafeDateTime(2);
					siparisEvrakListItem.sip_teslim_tarih = val2.GetSafeDateTime(3);
					siparisEvrakListItem.sip_musteri_kod = val2.GetSafeString(4);
					siparisEvrakListItem.sip_miktar = val2.GetSafeDouble(5);
					siparisEvrakListItem.sip_teslim_miktar = val2.GetSafeDouble(6);
					siparisEvrakListItem.sip_tip = (enum_sip_tip)val2.GetSafeByte(7);
					siparisEvrakListItem.sip_cins = (enum_sip_cins)val2.GetSafeByte(8);
					list.Add(siparisEvrakListItem);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<string> GetKapamaNedenleri(SqliteConnection OpenedConnection)
	{
		List<string> list = new List<string>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string commandText = "SELECT Kpm_kod,Kpm_ismi FROM KAPAMA_NEDENLERI_TANIMLARI";
				((DbCommand)val).CommandText = commandText;
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					list.Add(val2.GetSafeString(0) + " | " + val2.GetSafeString(1));
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<StokListItemFiyatveMiktarli> GetSonSiparisUrunler(SqliteConnection OpenedConnection, enum_sip_tip SiparisTipi, enum_sip_cins SiparisCinsi, string cari_kod)
	{
		List<StokListItemFiyatveMiktarli> list = new List<StokListItemFiyatveMiktarli>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string[] array = new string[10];
				int num = (int)SiparisTipi;
				string text = num.ToString();
				num = (int)SiparisCinsi;
				array[0] = "SELECT sip_stok_kod,sip_miktar,sip_birim_pntr FROM SIPARISLER WHERE sip_tip =" + text + " AND sip_cins=" + num;
				array[1] = " AND sip_evrakno_seri = (SELECT sip_evrakno_seri FROM SIPARISLER WHERE sip_tip=";
				num = (int)SiparisTipi;
				array[2] = num.ToString();
				array[3] = " AND sip_cins=";
				num = (int)SiparisCinsi;
				array[4] = num.ToString();
				array[5] = " AND sip_musteri_kod=@cari_kodu ORDER BY sip_tarih DESC LIMIT 1) AND sip_evrakno_sira = (SELECT sip_evrakno_sira FROM SIPARISLER WHERE sip_tip=";
				num = (int)SiparisTipi;
				array[6] = num.ToString();
				array[7] = " AND sip_cins=";
				num = (int)SiparisCinsi;
				array[8] = num.ToString();
				array[9] = " AND sip_musteri_kod=@cari_kodu ORDER BY sip_tarih DESC LIMIT 1) ORDER BY sip_satirno";
				string commandText = string.Concat(array);
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@cari_kodu", (object)cari_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					StokListItemFiyatveMiktarli stokListItemFiyatveMiktarli = new StokListItemFiyatveMiktarli();
					stokListItemFiyatveMiktarli.sto_kod = val2.GetSafeString(0);
					stokListItemFiyatveMiktarli.ekleme_bilgileri.Miktar = val2.GetSafeDouble(1);
					stokListItemFiyatveMiktarli.ekleme_bilgileri.sth_birim_pntr = val2.GetSafeInt32(2);
					list.Add(stokListItemFiyatveMiktarli);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<StokListItemFiyatveMiktarli> GetSonProformaSiparisUrunler(SqliteConnection OpenedConnection, enum_sip_tip SiparisTipi, enum_sip_cins SiparisCinsi, string cari_kod)
	{
		List<StokListItemFiyatveMiktarli> list = new List<StokListItemFiyatveMiktarli>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string[] array = new string[10];
				int num = (int)SiparisTipi;
				string text = num.ToString();
				num = (int)SiparisCinsi;
				array[0] = "SELECT pro_stokkodu,pro_miktar,pro_birim_pntr FROM PROFORMA_SIPARISLER WHERE pro_tipi =" + text + " AND pro_cinsi=" + num;
				array[1] = " AND pro_evrakno_seri = (SELECT pro_evrakno_seri FROM PROFORMA_SIPARISLER WHERE pro_tipi=";
				num = (int)SiparisTipi;
				array[2] = num.ToString();
				array[3] = " AND pro_cinsi=";
				num = (int)SiparisCinsi;
				array[4] = num.ToString();
				array[5] = " AND pro_mustkodu=@cari_kodu ORDER BY pro_tarihi DESC LIMIT 1) AND pro_evrakno_sira = (SELECT pro_evrakno_sira FROM PROFORMA_SIPARISLER WHERE pro_tipi=";
				num = (int)SiparisTipi;
				array[6] = num.ToString();
				array[7] = " AND pro_cinsi=";
				num = (int)SiparisCinsi;
				array[8] = num.ToString();
				array[9] = " AND pro_mustkodu=@cari_kodu ORDER BY pro_tarihi DESC LIMIT 1) ORDER BY pro_satirno";
				string commandText = string.Concat(array);
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@cari_kodu", (object)cari_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					StokListItemFiyatveMiktarli stokListItemFiyatveMiktarli = new StokListItemFiyatveMiktarli();
					stokListItemFiyatveMiktarli.sto_kod = val2.GetSafeString(0);
					stokListItemFiyatveMiktarli.ekleme_bilgileri.Miktar = val2.GetSafeDouble(1);
					stokListItemFiyatveMiktarli.ekleme_bilgileri.sth_birim_pntr = val2.GetSafeInt32(2);
					list.Add(stokListItemFiyatveMiktarli);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<double> GetRaporSiparisMeblag(SqliteConnection OpenedConnection, string temsilci_kodu, DateTime baslangic_tarihi, DateTime bitis_tarihi)
	{
		List<double> list = new List<double>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string commandText = "SELECT SUM((sip_tutar- sip_iskonto_1-sip_iskonto_2 -sip_iskonto_3- sip_iskonto_4-sip_iskonto_5-sip_iskonto_6+sip_masraf_1+sip_masraf_2+sip_masraf_3+sip_masraf_4+sip_vergi+sip_masvergi)* sip_doviz_kuru) AS MEBLAG,sip_evrakno_seri,sip_evrakno_sira FROM SIPARISLER WHERE sip_tarih>=@tarih_baslangic AND sip_tarih<@tarih_bitis AND sip_satici_kod=@sip_satici_kod AND sip_tip=0 AND sip_cins=0 GROUP BY sip_evrakno_seri,sip_evrakno_sira";
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sip_satici_kod", (object)temsilci_kodu);
				val.Parameters.AddWithValue("@tarih_baslangic", (object)baslangic_tarihi);
				val.Parameters.AddWithValue("@tarih_bitis", (object)bitis_tarihi);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					list.Add(val2.GetSafeDouble(0));
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<double> GetRaporProformaSiparisMeblag(SqliteConnection OpenedConnection, string temsilci_kodu, DateTime baslangic_tarihi, DateTime bitis_tarihi)
	{
		List<double> list = new List<double>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string commandText = "SELECT SUM((pro_tutari-pro_iskonto1-pro_iskonto2-pro_iskonto3-pro_iskonto4-pro_iskonto5-pro_iskonto6+pro_masraf1+pro_masraf2+pro_masraf3+pro_masraf4+pro_vergi+pro_masrafvergi)*pro_dovizkuru) AS MEBLAG,pro_evrakno_seri,pro_evrakno_sira FROM PROFORMA_SIPARISLER WHERE pro_tarihi>=@tarih_baslangic AND pro_tarihi<@tarih_bitis AND pro_saticikodu=@pro_saticikodu AND pro_tipi=0 AND pro_cinsi=2 GROUP BY pro_evrakno_seri,pro_evrakno_sira";
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@pro_saticikodu", (object)temsilci_kodu);
				val.Parameters.AddWithValue("@tarih_baslangic", (object)baslangic_tarihi);
				val.Parameters.AddWithValue("@tarih_bitis", (object)bitis_tarihi);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					list.Add(val2.GetSafeDouble(0));
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<GenelList> GetTeslimTurleriListItems(SqliteConnection connection)
	{
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = connection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = "SELECT tslt_kod,tslt_ismi FROM TESLIM_TURLERI ORDER BY tslt_ismi";
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					GenelList genelList = new GenelList();
					genelList.Kod = val2.GetSafeString(0);
					genelList.Text = val2.GetSafeString(1);
					list.Add(genelList);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}
}
