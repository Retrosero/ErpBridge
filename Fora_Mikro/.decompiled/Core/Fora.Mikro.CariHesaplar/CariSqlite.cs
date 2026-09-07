using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariBolgeleri;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.CariHesaplar.CariGruplari;
using Fora.Mikro.CariHesaplar.CariTeminatBilgileri;
using Fora.Mikro.CariHesaplar.CariYetkilileri;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Enumler;
using Fora.Mikro.Firmalar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.OdemePlanlari;
using Fora.Mikro.Siparis;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Temsilciler;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.CariHesaplar;

public static class CariSqlite
{
	public static Cari GetCariByCariKod(SqliteConnection connection, string cari_kod, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByAranacakField(connection, "cari_kod", cari_kod, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByCariUnvan(SqliteConnection connection, string cari_unvan, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByAranacakField(connection, "cari_unvan", cari_unvan, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByVergiTcKimlikNo(SqliteConnection connection, string vergi_tc_kimlik_no, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByAranacakField(connection, "cari_vdaire_no", vergi_tc_kimlik_no, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByEMail(SqliteConnection connection, string e_mail, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByAranacakField(connection, "cari_EMail", e_mail, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByBankaHesapNo(SqliteConnection connection, string banka_hesap_no, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByAranacakField(connection, "cari_banka_hesapno1", banka_hesap_no, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	private static Cari GetCariByAranacakField(SqliteConnection connection, string aranacak_field_adi, string aranacak_deger, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		if (aranacak_deger == null)
		{
			return new Cari();
		}
		string sqlstring = "SELECT cari_kod,cari_unvan1,cari_unvan2,cari_muh_kod,cari_muh_kod1,cari_muh_kod2,cari_vdaire_adi,cari_vdaire_no,cari_Ana_cari_kodu,cari_bolge_kodu,cari_grup_kodu,cari_temsilci_kodu,cari_sektor_kodu,cari_satis_isk_kod,cari_special1,cari_special2,cari_special3,cari_sicil_no,cari_VergiKimlikNo,cari_vade_fark_yuz,cari_vade_fark_yuz1,cari_vade_fark_yuz2,cari_tipi,cari_doviz_cinsi,cari_doviz_cinsi1,cari_doviz_cinsi2,cari_odeme_gunu,cari_hareket_tipi,cari_odemeplan_no,cari_satis_fk,cari_KurHesapSekli,cari_odeme_cinsi,cari_fatura_adres_no,cari_sevk_adres_no,cari_banka_hesapno1,cari_CepTel,cari_EMail,cari_VarsayilanGirisDepo,cari_VarsayilanCikisDepo,cari_cari_kilitli_flg FROM CARI_HESAPLAR WITH (NOLOCK) WHERE " + aranacak_field_adi + "=@deger";
		return GetCariBySqlString(connection, sqlstring, aranacak_deger, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	private static Cari GetCariBySqlString(SqliteConnection connection, string sqlstring, string deger, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		Cari cari = new Cari();
		sqlstring = sqlstring.Replace(" WITH (NOLOCK)", "");
		try
		{
			SqliteCommand val = connection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = sqlstring;
				val.Parameters.AddWithValue("@deger", (object)deger);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					cari.cari_kod = val2.GetSafeString(0);
					cari.cari_unvan1 = val2.GetSafeString(1);
					cari.cari_unvan2 = val2.GetSafeString(2);
					cari.cari_muh_kod = val2.GetSafeString(3);
					cari.cari_muh_kod1 = val2.GetSafeString(4);
					cari.cari_muh_kod2 = val2.GetSafeString(5);
					cari.cari_vdaire_adi = val2.GetSafeString(6);
					cari.cari_vdaire_no = val2.GetSafeString(7);
					cari.cari_Ana_cari_kodu = val2.GetSafeString(8);
					cari.cari_bolge_kodu = val2.GetSafeString(9);
					cari.cari_grup_kodu = val2.GetSafeString(10);
					cari.cari_temsilci_kodu = val2.GetSafeString(11);
					cari.cari_sektor_kodu = val2.GetSafeString(12);
					cari.cari_satis_isk_kod = val2.GetSafeString(13);
					cari.cari_special1 = val2.GetSafeString(14);
					cari.cari_special2 = val2.GetSafeString(15);
					cari.cari_special3 = val2.GetSafeString(16);
					cari.cari_sicil_no = val2.GetSafeString(17);
					cari.cari_VergiKimlikNo = val2.GetSafeString(18);
					cari.cari_vade_fark_yuz = (float)val2.GetSafeDouble(19);
					cari.cari_vade_fark_yuz1 = (float)val2.GetSafeDouble(20);
					cari.cari_vade_fark_yuz2 = (float)val2.GetSafeDouble(21);
					cari.cari_tipi = val2.GetSafeByte(22);
					cari.cari_doviz_cinsi = val2.GetSafeByte(23);
					cari.cari_doviz_cinsi1 = val2.GetSafeByte(24);
					cari.cari_doviz_cinsi2 = val2.GetSafeByte(25);
					cari.cari_odeme_gunu = val2.GetSafeByte(26);
					cari.cari_hareket_tipi = val2.GetSafeByte(27);
					cari.cari_odemeplan_no = val2.GetSafeInt32(28);
					cari.cari_satis_fk = val2.GetSafeInt32(29);
					cari.cari_KurHesapSekli = val2.GetSafeByte(30);
					cari.cari_odeme_cinsi = val2.GetSafeByte(31);
					cari.cari_fatura_adres_no = val2.GetSafeInt32(32);
					cari.cari_sevk_adres_no = val2.GetSafeInt32(33);
					cari.cari_banka_hesapno1 = val2.GetSafeString(34);
					cari.cari_CepTel = val2.GetSafeString(35);
					cari.cari_Email = val2.GetSafeString(36);
					cari.cari_VarsayilanGirisDepo = val2.GetSafeInt32(37);
					cari.cari_VarsayilanCikisDepo = val2.GetSafeInt32(38);
					cari.cari_cari_kilitli_flg = val2.GetSafeBoolean(39);
				}
				((DbDataReader)val2).Close();
				((DbDataReader)val2).Dispose();
				val2 = null;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		List<CariAdres> list = new List<CariAdres>();
		string text = "SELECT adr_cari_kod,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_yon_kodu,adr_temsilci_kodu,adr_ozel_not,adr_ziyaretgunu,adr_gps_enlem,adr_gps_boylam,adr_adres_no,adr_uzaklik_kodu,adr_ziyaretperyodu,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7 FROM CARI_HESAP_ADRESLERI WHERE adr_cari_kod=@cari_kod";
		if (AdreslerTemsilciyeGore)
		{
			text = text + " AND adr_temsilci_kodu='" + TemsilciKodu + "'";
		}
		SqliteCommand val3 = connection.CreateCommand();
		try
		{
			((DbCommand)val3).CommandText = text;
			val3.Parameters.AddWithValue("@cari_kod", (object)cari.cari_kod);
			SqliteDataReader val4 = val3.ExecuteReader();
			while (((DbDataReader)val4).Read())
			{
				CariAdres cariAdres = new CariAdres();
				cariAdres.adr_cari_kod = val4.GetSafeString(0);
				cariAdres.adr_cadde = val4.GetSafeString(1);
				cariAdres.adr_sokak = val4.GetSafeString(2);
				cariAdres.adr_posta_kodu = val4.GetSafeString(3);
				cariAdres.adr_ilce = val4.GetSafeString(4);
				cariAdres.adr_il = val4.GetSafeString(5);
				cariAdres.adr_ulke = val4.GetSafeString(6);
				cariAdres.adr_tel_ulke_kodu = val4.GetSafeString(7);
				cariAdres.adr_tel_bolge_kodu = val4.GetSafeString(8);
				cariAdres.adr_tel_no1 = val4.GetSafeString(9);
				cariAdres.adr_tel_no2 = val4.GetSafeString(10);
				cariAdres.adr_tel_faxno = val4.GetSafeString(11);
				cariAdres.adr_tel_modem = val4.GetSafeString(12);
				cariAdres.adr_yon_kodu = val4.GetSafeString(13);
				cariAdres.adr_temsilci_kodu = val4.GetSafeString(14);
				cariAdres.adr_ozel_not = val4.GetSafeString(15);
				cariAdres.adr_ziyaretgunu = val4.GetSafeDouble(16).ToString();
				cariAdres.adr_gps_enlem = (float)val4.GetSafeDouble(17);
				cariAdres.adr_gps_boylam = (float)val4.GetSafeDouble(18);
				cariAdres.adr_adres_no = val4.GetSafeInt32(19);
				cariAdres.adr_uzaklik_kodu = val4.GetSafeInt16(20);
				cariAdres.adr_ziyaretperyodu = val4.GetSafeByte(21);
				cariAdres.adr_ziyarethaftasi = val4.GetSafeByte(22);
				cariAdres.adr_ziygunu2_1 = val4.GetSafeBoolean(23);
				cariAdres.adr_ziygunu2_2 = val4.GetSafeBoolean(24);
				cariAdres.adr_ziygunu2_3 = val4.GetSafeBoolean(25);
				cariAdres.adr_ziygunu2_4 = val4.GetSafeBoolean(26);
				cariAdres.adr_ziygunu2_5 = val4.GetSafeBoolean(27);
				cariAdres.adr_ziygunu2_6 = val4.GetSafeBoolean(28);
				cariAdres.adr_ziygunu2_7 = val4.GetSafeBoolean(29);
				cariAdres.CariUnvan = cari.cari_unvan1 + " " + cari.cari_unvan2;
				list.Add(cariAdres);
			}
			((DbDataReader)val4).Close();
			((DbDataReader)val4).Dispose();
			val4 = null;
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		cari.CariAdresleri = list;
		List<CariYetkili> list2 = new List<CariYetkili>();
		string commandText = "SELECT mye_cari_kod,mye_isim,mye_soyisim,mye_es_isim,mye_dahili_telno,mye_email_adres,mye_cep_telno,mye_tc_kimlikno,mye_vergi_dairesi,mye_vergi_kimlikno,mye_dogum_yeri,mye_ev_cadde,mye_ev_sokak,mye_ev_posta_kodu,mye_ev_ilce,mye_ev_il,mye_ev_ulke,mye_is_telno,mye_ev_telno,mye_adres_no,mye_unvan,mye_hitap,mye_hisse,mye_tahsil,mye_dogum_tarihi,mye_evlilik_tarihi,mye_es_dogum_tarihi FROM CARI_HESAP_YETKILILERI WHERE mye_cari_kod=@cari_kod";
		SqliteCommand val5 = connection.CreateCommand();
		try
		{
			((DbCommand)val5).CommandText = commandText;
			val5.Parameters.AddWithValue("@cari_kod", (object)cari.cari_kod);
			SqliteDataReader val6 = val5.ExecuteReader();
			while (((DbDataReader)val6).Read())
			{
				CariYetkili cariYetkili = new CariYetkili();
				cariYetkili.mye_cari_kod = val6.GetSafeString(0);
				cariYetkili.mye_isim = val6.GetSafeString(1);
				cariYetkili.mye_soyisim = val6.GetSafeString(2);
				cariYetkili.mye_es_isim = val6.GetSafeString(3);
				cariYetkili.mye_dahili_telno = val6.GetSafeString(4);
				cariYetkili.mye_email_adres = val6.GetSafeString(5);
				cariYetkili.mye_cep_telno = val6.GetSafeString(6);
				cariYetkili.mye_tc_kimlikno = val6.GetSafeString(7);
				cariYetkili.mye_vergi_dairesi = val6.GetSafeString(8);
				cariYetkili.mye_vergi_kimlikno = val6.GetSafeString(9);
				cariYetkili.mye_dogum_yeri = val6.GetSafeString(10);
				cariYetkili.mye_ev_cadde = val6.GetSafeString(11);
				cariYetkili.mye_ev_sokak = val6.GetSafeString(12);
				cariYetkili.mye_ev_posta_kodu = val6.GetSafeString(13);
				cariYetkili.mye_ev_ilce = val6.GetSafeString(14);
				cariYetkili.mye_ev_il = val6.GetSafeString(15);
				cariYetkili.mye_ev_ulke = val6.GetSafeString(16);
				cariYetkili.mye_is_telno = val6.GetSafeString(17);
				cariYetkili.mye_ev_telno = val6.GetSafeString(18);
				cariYetkili.mye_adres_no = val6.GetSafeInt32(19);
				cariYetkili.mye_unvan = (enum_CariYetkiliUnvan)val6.GetSafeByte(20);
				cariYetkili.mye_hitap = val6.GetSafeByte(21);
				cariYetkili.mye_hisse = val6.GetSafeByte(22);
				cariYetkili.mye_tahsil = val6.GetSafeByte(23);
				cariYetkili.mye_dogum_tarihi = val6.GetSafeDateTime(24);
				cariYetkili.mye_evlilik_tarihi = val6.GetSafeDateTime(25);
				cariYetkili.mye_es_dogum_tarihi = val6.GetSafeDateTime(26);
				foreach (CariAdres item in cari.CariAdresleri)
				{
					if (cariYetkili.mye_adres_no == item.adr_adres_no)
					{
						cariYetkili.Adres = item;
						break;
					}
				}
				list2.Add(cariYetkili);
			}
			((DbDataReader)val6).Close();
			((DbDataReader)val6).Dispose();
			val6 = null;
		}
		finally
		{
			((IDisposable)val5)?.Dispose();
		}
		cari.CariYetkilileri = list2;
		return cari;
	}

	public static string GetCariUnvan1(SqliteConnection OpenedConnection, string cari_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		SqliteCommand val = new SqliteCommand("SELECT cari_unvan1 FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod", OpenedConnection);
		val.Parameters.AddWithValue("@cari_kod", (object)cari_kod);
		text = ((DbCommand)val).ExecuteScalar().ToString();
		((Component)val).Dispose();
		return text;
	}

	public static string GetCariSifre(SqliteConnection OpenedConnection, string cari_kod)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		string text = "SELECT cari_wwwadresi FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod";
		string result = "";
		try
		{
			SqliteCommand val = new SqliteCommand(text, OpenedConnection);
			val.Parameters.AddWithValue("@cari_kod", (object)cari_kod);
			result = ((DbCommand)val).ExecuteScalar().ToString();
			((Component)val).Dispose();
		}
		catch
		{
		}
		return result;
	}

	public static double GetBakiye(SqliteConnection connection, string cari_kod, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		string text = "";
		string text2 = "";
		if (cha_grupno == -1)
		{
			text2 = "*cha_d_kur";
		}
		if (cha_grupno != -1)
		{
			text += " AND cha_grupno=@cha_grupno ";
		}
		if (FirmaNo != -1)
		{
			text += " AND cha_firmano=@cha_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND cha_subeno=@cha_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND cha_srmrkkodu=@cha_srmrkkodu ";
		}
		string text3 = "cha_meblag";
		text3 = "(CASE ";
		text3 += "WHEN (cha_cinsi IN (1,2,3,4,17,18,19,20,21,22)) AND (cha_cari_cins in (2, 4)) THEN cha_aratoplam ";
		text3 += "WHEN (cha_cinsi IN (9, 27)) AND (cha_cari_cins IN (3,5,9,12))  THEN cha_meblag - cha_vergi1 - cha_vergi2 - cha_vergi3 - cha_vergi4 - cha_vergi5 - cha_vergi6 - cha_vergi7 - cha_vergi8 - cha_vergi9 - cha_vergi10 ";
		text3 += "WHEN (cha_cinsi IN (8,14,11,10,28)) AND (cha_cari_cins IN (3, 5, 6, 8, 9, 12)) THEN cha_meblag - cha_vergi1 - cha_vergi2 - cha_vergi3 - cha_vergi4 - cha_vergi5 - cha_vergi6 - cha_vergi7 - cha_vergi8 - cha_vergi9 - cha_vergi10 ";
		text3 += "WHEN (cha_cinsi IN (13,29)) OR (cha_ticaret_turu in (2, 4) AND (cha_cinsi in (10, 11, 14, 15) OR (cha_cinsi in (8) AND cha_kasa_hizmet = 6))) OR (NOT(cha_cari_cins IN (0, 1, 2, 4, 6, 10, 11))) THEN cha_meblag - cha_vergi1 - cha_vergi2 - cha_vergi3 - cha_vergi4 - cha_vergi5 - cha_vergi6 - cha_vergi7 - cha_vergi8 - cha_vergi9 - cha_vergi10 ";
		text3 += "WHEN (cha_cinsi = 33) AND (cha_cari_cins IN (0, 1, 6)) THEN cha_aratoplam ";
		text3 += "WHEN (cha_cari_cins IN (10)) THEN cha_aratoplam ";
		text3 += "WHEN (cha_evrak_tip IN (108)) AND (cha_cari_cins IN (2)) THEN cha_aratoplam ";
		text3 += "WHEN (cha_evrak_tip IN (109)) AND (cha_cari_cins IN (11)) THEN cha_aratoplam ";
		text3 += "ELSE cha_meblag END)";
		string text4 = "SELECT round (ifnull((select SUM(" + text3 + text2 + ") from CARI_HESAP_HAREKETLERI WHERE cha_tip=0 AND cha_kod=@cha_kod AND cha_cari_cins=0" + text + "),0)-ifnull((select SUM(" + text3 + text2 + ") from CARI_HESAP_HAREKETLERI  WHERE cha_tip=1 AND cha_kod=@cha_kod AND cha_cari_cins=0" + text + "),0),2) AS Tutar";
		double result = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand(text4, connection);
			((DbCommand)val).CommandText = text4;
			val.Parameters.AddWithValue("@cha_kod", (object)cari_kod);
			if (cha_grupno != -1)
			{
				val.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			}
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@cha_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@cha_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@cha_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			result = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static int GetAdatGunu(SqliteConnection OpenedConnection, string cari_kodu, DateTime ReferansTarihi, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		string text2 = "";
		if (cha_grupno == -1)
		{
			text2 = "*cha_d_kur";
		}
		if (cha_grupno != -1)
		{
			text += " AND cha_grupno=@cha_grupno ";
		}
		if (FirmaNo != -1)
		{
			text += " AND cha_firmano=@cha_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND cha_subeno=@cha_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND cha_srmrkkodu=@cha_srmrkkodu ";
		}
		string commandText = "SELECT cha_tip,cha_tarihi,cha_vade,(cha_aratoplam-cha_ft_iskonto1-cha_ft_iskonto2-cha_ft_iskonto3-cha_ft_iskonto4-cha_ft_iskonto5-cha_ft_iskonto6)" + text2 + " AS ARATOPLAM,(cha_ft_masraf1+cha_ft_masraf2+cha_ft_masraf3+cha_ft_masraf4)" + text2 + " AS MASRAF,(cha_vergi1+cha_vergi2+cha_vergi3+cha_vergi4+cha_vergi5+cha_vergi6+cha_vergi7+cha_vergi8+cha_vergi9+cha_vergi10)" + text2 + " AS VERGI FROM CARI_HESAP_HAREKETLERI WHERE cha_kod=@cari_kodu" + text;
		int result = 0;
		List<int> list = new List<int>();
		List<DateTime> list2 = new List<DateTime>();
		List<int> list3 = new List<int>();
		List<double> list4 = new List<double>();
		List<double> list5 = new List<double>();
		List<double> list6 = new List<double>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			val.Connection = OpenedConnection;
			((DbCommand)val).CommandText = commandText;
			val.Parameters.AddWithValue("@cari_kodu", (object)cari_kodu);
			if (cha_grupno != -1)
			{
				val.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			}
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@cha_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@cha_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@cha_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeByte(0));
				list2.Add(val2.GetSafeDateTime(1));
				list3.Add(val2.GetSafeInt32(2));
				list4.Add(val2.GetSafeDouble(3));
				list5.Add(val2.GetSafeDouble(4));
				list6.Add(val2.GetSafeDouble(5));
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			double num4 = 0.0;
			for (int i = 0; i < list.Count; i++)
			{
				int num5 = 0;
				DateTime dateTime = DateTime.Now;
				if (list3[i].ToString().Length == 8)
				{
					dateTime = new DateTime(int.Parse(list3[i].ToString().Substring(0, 4)), int.Parse(list3[i].ToString().Substring(4, 2)), int.Parse(list3[i].ToString().Substring(6, 2)));
				}
				else if (list3[i] == 0)
				{
					dateTime = list2[i];
				}
				else if (list3[i] < 0)
				{
					dateTime = list2[i].AddDays(-1 * list3[i]);
				}
				else
				{
					dateTime = list2[i];
					try
					{
						SqliteCommand val3 = new SqliteCommand
						{
							Connection = OpenedConnection,
							CommandText = "SELECT odp_aratop FROM ODEME_PLANLARI WHERE odp_no=@odpno"
						};
						val3.Parameters.AddWithValue("@odpno", (object)list3[i]);
						object obj = ((DbCommand)val3).ExecuteScalar();
						string s = obj.ToString().Substring(0, obj.ToString().IndexOf("-"));
						int result2 = 0;
						int.TryParse(s, out result2);
						dateTime = list2[i].AddDays(result2);
						((Component)val3).Dispose();
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
				}
				num5 = (int)(dateTime - ReferansTarihi).TotalDays;
				if (list[i] == 0)
				{
					num += list4[i];
					num += list5[i];
					num += list6[i];
					num3 += list4[i] * (double)num5;
					num3 += list5[i] * (double)num5;
					num3 += list6[i] * (double)num5;
				}
				else
				{
					num2 += list4[i];
					num2 += list5[i];
					num2 += list6[i];
					num4 += list4[i] * (double)num5;
					num4 += list5[i] * (double)num5;
					num4 += list6[i] * (double)num5;
				}
			}
			double num6 = num - num2;
			double num7 = num3 - num4;
			result = ((!(num6 > 0.0)) ? ((int)(Math.Round(num7 * -1.0 / num6, 0) * -1.0)) : ((int)Math.Round(num7 / num6, 0)));
		}
		catch (Exception ex2)
		{
			Console.WriteLine(ex2.ToString());
		}
		return result;
	}

	public static double GetKarsilanmamisSiparisTutari(SqliteConnection connection, string cari_kod, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		string text = "";
		string text2 = "";
		if (cha_grupno == -1)
		{
			text2 = "*sip_doviz_kuru";
		}
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
		string commandText = "SELECT ROUND((IFNULL ((SELECT SUM((((sip_tutar- sip_iskonto_1-sip_iskonto_2 -sip_iskonto_3- sip_iskonto_4-sip_iskonto_5-sip_iskonto_6+sip_masraf_1+sip_masraf_2+sip_masraf_3+sip_masraf_4+sip_vergi+sip_masvergi)/sip_miktar)*( sip_miktar-sip_teslim_miktar ))" + text2 + ") FROM SIPARISLER WHERE sip_musteri_kod=@cha_kod AND sip_kapat_fl = 0 AND sip_tip =0" + text + "),0)), 2)";
		double result = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@cha_kod", (object)cari_kod);
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
			result = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static double GetFaturalasmamisIrsaliyeTutari(SqliteConnection connection, Cari cari, string cari_kod, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		string text = "";
		string text2 = "";
		if (cha_grupno == -1)
		{
			text2 = "*sth_har_doviz_kuru";
		}
		if (cha_grupno != -1)
		{
			text += " AND sth_har_doviz_cinsi=@sth_har_doviz_cinsi ";
		}
		if (FirmaNo != -1)
		{
			text += " AND sth_firmano=@sth_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND sth_subeno=@sth_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND sth_cari_srm_merkezi=@sth_cari_srm_merkezi ";
		}
		string text3 = "sth_fat_recid_recno";
		if (AppBase.MikroVersiyonu > 15)
		{
			text3 = "sth_fat_uid";
		}
		string commandText = "SELECT ROUND((IFNULL (( SELECT SUM ( case when sth_normal_iade = 0 then ((sth_tutar- sth_iskonto1-sth_iskonto2-sth_iskonto3-sth_iskonto4-sth_iskonto5-sth_iskonto6+sth_masraf1+sth_masraf2+sth_masraf3+sth_masraf4+sth_vergi+sth_masraf_vergi)" + text2 + ") else -1*((sth_tutar- sth_iskonto1-sth_iskonto2-sth_iskonto3-sth_iskonto4-sth_iskonto5-sth_iskonto6+sth_masraf1+sth_masraf2+sth_masraf3+sth_masraf4+sth_vergi+sth_masraf_vergi)" + text2 + ") end) FROM STOK_HAREKETLERI WHERE ( sth_tip=1 AND sth_cins in (0 ,1, 2) AND sth_evraktip= 1 AND sth_cari_kodu=@cha_kod  AND " + text3 + "=@faturasiz" + text + ")),0)), 2) AS Tutar";
		double result = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@cha_kod", (object)cari_kod);
			if (AppBase.MikroVersiyonu > 15)
			{
				SqliteParameterCollection parameters = val.Parameters;
				Guid empty = Guid.Empty;
				parameters.AddWithValue("@faturasiz", (object)empty.ToByteArray());
			}
			else
			{
				val.Parameters.AddWithValue("@faturasiz", (object)0);
			}
			switch (cha_grupno)
			{
			case 0:
				val.Parameters.AddWithValue("@sth_har_doviz_cinsi", (object)cari.cari_doviz_cinsi);
				break;
			case 1:
				val.Parameters.AddWithValue("@sth_har_doviz_cinsi", (object)cari.cari_doviz_cinsi1);
				break;
			case 2:
				val.Parameters.AddWithValue("@sth_har_doviz_cinsi", (object)cari.cari_doviz_cinsi2);
				break;
			}
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@sth_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@sth_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@sth_cari_srm_merkezi", (object)SorumlulukMerkeziKodu);
			}
			result = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static double GetOdenmemisCekTutari(SqliteConnection connection, Cari cari, int CirolanmisCekinRiskSuresi, enum_sck_imza sck_imza, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		double num = 0.0;
		try
		{
			string text = "";
			if (cha_grupno != -1)
			{
				text += " AND sck_sahip_cari_grupno=@sck_sahip_cari_grupno ";
			}
			if (FirmaNo != -1)
			{
				text += " AND sck_firmano=@sck_firmano ";
			}
			if (SubeNo != -1)
			{
				text += " AND sck_subeno=@sck_subeno ";
			}
			if (SorumlulukMerkeziDetayli)
			{
				text += " AND sck_srmmrk=@sck_srmmrk ";
			}
			string text2 = "";
			if (sck_imza != enum_sck_imza.Tumu)
			{
				int num2 = (int)sck_imza;
				text2 = " sck_imza=" + num2 + " AND ";
			}
			SqliteCommand val = new SqliteCommand("SELECT ROUND (( IFNULL ((  SELECT SUM(Tutar) FROM ( SELECT (sck_tutar*(SELECT IFNULL((SELECT dov_fiyat1 FROM DOVIZ_KURLARI WHERE dov_no=sck_doviz AND dov_tarih<=@dov_tarih ORDER BY dov_tarih DESC LIMIT 1),1))) AS Tutar FROM ODEME_EMIRLERI WHERE " + text2 + " ((sck_sahip_cari_kodu=@car_kod AND sck_sahip_cari_cins=0) OR (sck_nerede_cari_kodu=@car_kod AND sck_nerede_cari_cins=0)) AND sck_tip in (0, 1, 2, 4, 5) AND sck_vade>datetime ('now', '-'|| @CirolanmisCekinRiskSuresi ||' day') AND sck_sonpoz in ( 0 , 1, 2 , 3 , 5 , 6 , 8 , 9 )" + text + ") AS Sonuc),0)), 2)", connection);
			val.Parameters.AddWithValue("@car_kod", (object)cari.cari_kod);
			val.Parameters.AddWithValue("@dov_tarih", (object)DateTime.Now);
			val.Parameters.AddWithValue("@CirolanmisCekinRiskSuresi", (object)CirolanmisCekinRiskSuresi.ToString());
			if (cha_grupno != -1)
			{
				val.Parameters.AddWithValue("@sck_sahip_cari_grupno", (object)cha_grupno);
			}
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@sck_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@sck_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@sck_srmmrk", (object)SorumlulukMerkeziKodu);
			}
			num = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
			val = null;
			if (cha_grupno != -1)
			{
				int num3 = 0;
				double num4 = 1.0;
				switch (cha_grupno)
				{
				case 0:
					num3 = cari.cari_doviz_cinsi;
					break;
				case 1:
					num3 = cari.cari_doviz_cinsi1;
					break;
				case 2:
					num3 = cari.cari_doviz_cinsi2;
					break;
				}
				val = new SqliteCommand("SELECT dov_fiyat1 FROM DOVIZ_KURLARI WHERE dov_no=@dov_no AND dov_tarih<=@dov_tarih ORDER BY dov_tarih DESC LIMIT 1", connection);
				val.Parameters.AddWithValue("@dov_no", (object)num3);
				val.Parameters.AddWithValue("@dov_tarih", (object)DateTime.Now);
				num4 = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
				((Component)val).Dispose();
				val = null;
				num /= num4;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return num;
	}

	public static CariTeminatlari GetTeminatTutari(SqliteConnection OpenedConnection, string cari_kod, int FirmaNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu, int DovizCinsi)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		string text = "";
		if (FirmaNo != -1)
		{
			text += " AND (ct_GecerliFirma=@ct_GecerliFirma OR ct_GecerliFirma=-1) ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND ct_srmrkkodu=@ct_srmrkkodu ";
		}
		string commandText = "SELECT ct_Aciklama_no,ct_tutari*(SELECT IFNULL((SELECT dov_fiyat1 FROM DOVIZ_KURLARI WHERE dov_no=ct_DovizCinsi AND dov_tarih<datetime() ORDER BY dov_tarih DESC LIMIT 1),1)) AS Tutar FROM CARI_HESAP_TEMINATLARI WHERE ct_carikodu=@car_kod AND ct_vade>=datetime() " + text;
		List<CariTeminat> list = new List<CariTeminat>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@car_kod", (object)cari_kod);
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@ct_GecerliFirma", (object)FirmaNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@ct_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				CariTeminat cariTeminat = new CariTeminat();
				cariTeminat.teminat_tipi = (enum_cari_teminat_tipleri)int.Parse(val2.GetSafeString(0));
				cariTeminat.Tutar = val2.GetSafeDouble(1);
				list.Add(cariTeminat);
			}
			((Component)val).Dispose();
			val = null;
			commandText = "SELECT (IFNULL ((SELECT dov_fiyat1 FROM DOVIZ_KURLARI WHERE dov_no=@dov_no AND dov_tarih<=@dov_tarih ORDER BY dov_tarih DESC LIMIT 1),1))";
			val = new SqliteCommand(commandText, OpenedConnection);
			val.Parameters.AddWithValue("@dov_no", (object)DovizCinsi);
			val.Parameters.AddWithValue("@dov_tarih", (object)DateTime.Now);
			double num = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
			val = null;
			foreach (CariTeminat item in list)
			{
				item.Tutar /= num;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return new CariTeminatlari(list);
	}

	public static double GetCiroTutari(SqliteConnection OpenedConnection, string cari_kod, DateTime tarihbaslangic, DateTime tarihbitis, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		string text = "";
		string text2 = "";
		if (cha_grupno == -1)
		{
			text2 = "*cha_d_kur";
		}
		if (cha_grupno != -1)
		{
			text += " AND cha_grupno=@cha_grupno ";
		}
		if (FirmaNo != -1)
		{
			text += " AND cha_firmano=@cha_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND cha_subeno=@cha_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND cha_srmrkkodu=@cha_srmrkkodu ";
		}
		string commandText = "SELECT ROUND (( IFNULL ((SELECT SUM(((cha_aratoplam -cha_ft_iskonto1- cha_ft_iskonto2-cha_ft_iskonto3 -cha_ft_iskonto4- cha_ft_iskonto5-cha_ft_iskonto6 ) * case when ( cha_tip=1 AND cha_normal_Iade= 1) THEN - 1 ELSE 1 END)" + text2 + ") FROM CARI_HESAP_HAREKETLERI  WHERE cha_ciro_cari_kodu=@car_kod AND (cha_tarihi>=@tarihbaslangic AND cha_tarihi<=@tarihbitis AND ((cha_tip= 1 AND cha_normal_Iade=1 AND cha_evrak_tip=0) OR (cha_tip=0 AND cha_normal_Iade=0 AND cha_evrak_tip=63)) AND (cha_grupno in (0 ,1, 2) OR cha_ciro_cari_kodu <> cha_kod))" + text + "), 0)), 2)";
		double result = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@car_kod", (object)cari_kod);
			val.Parameters.AddWithValue("@tarihbaslangic", (object)tarihbaslangic);
			val.Parameters.AddWithValue("@tarihbitis", (object)tarihbitis);
			if (cha_grupno != -1)
			{
				val.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			}
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@cha_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@cha_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@cha_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			result = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static bool GetKilitliMi(SqliteConnection connection, string cari_kod)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		SqliteCommand val = new SqliteCommand
		{
			Connection = connection,
			CommandText = "SELECT cari_cari_kilitli_flg FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod"
		};
		val.Parameters.AddWithValue("@cari_kod", (object)cari_kod);
		if (((DbCommand)val).ExecuteScalar().ToString() == "1")
		{
			result = true;
		}
		((Component)val).Dispose();
		return result;
	}

	public static bool GetEFaturaCarisiMi(SqliteConnection connection, string cari_kod)
	{
		bool result = false;
		SqliteCommand val = connection.CreateCommand();
		try
		{
			((DbCommand)val).CommandText = "SELECT cari_efatura_fl FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod";
			val.Parameters.AddWithValue("@cari_kod", (object)cari_kod);
			object obj = ((DbCommand)val).ExecuteScalar();
			if (obj != null && !(obj is DBNull))
			{
				result = obj.ToString() == "1";
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return result;
	}

	public static bool GetZiyaretEdilmisMi(SqliteConnection connection, string Temsilci_Kodu, string Cari_Kodu, int Cari_Adres_No, DateTime Tarihi)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				Connection = connection,
				CommandText = "SELECT zyrt_Temsilci_Kodu FROM _ZIYARET_HAREKETLERI WHERE zyrt_Temsilci_Kodu=@zyrt_Temsilci_Kodu AND zyrt_Cari_Kodu=@zyrt_Cari_Kodu AND zyrt_Cari_Adres_No=@zyrt_Cari_Adres_No AND zyrt_Tarihi=@zyrt_Tarihi"
			};
			val.Parameters.AddWithValue("@zyrt_Temsilci_Kodu", (object)Temsilci_Kodu);
			val.Parameters.AddWithValue("@zyrt_Cari_Kodu", (object)Cari_Kodu);
			val.Parameters.AddWithValue("@zyrt_Cari_Adres_No", (object)Cari_Adres_No);
			val.Parameters.AddWithValue("@zyrt_Tarihi", (object)new DateTime(Tarihi.Year, Tarihi.Month, Tarihi.Day));
			if (((DbCommand)val).ExecuteScalar() != null)
			{
				result = true;
			}
			((Component)val).Dispose();
		}
		catch
		{
		}
		return result;
	}

	public static int GetCariIlkAdresNo(SqliteConnection OpenedConnection, string cari_kod)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		int result = 1;
		SqliteCommand val = new SqliteCommand("SELECT adr_adres_no FROM CARI_HESAP_ADRESLERI WHERE adr_cari_kod=@cari_kod ORDER BY adr_adres_no", OpenedConnection);
		val.Parameters.AddWithValue("@cari_kod", (object)cari_kod);
		try
		{
			result = int.Parse(((DbCommand)val).ExecuteScalar().ToString());
		}
		catch
		{
		}
		((Component)val).Dispose();
		val = null;
		return result;
	}

	public static List<CariEkstre> GetYapilacakTahsilatlar(SqliteConnection connection, string cari_kod, double cari_bakiye, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu, bool ProjeDetayli, string ProjeKodu)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Expected O, but got Unknown
		string text = "";
		if (cha_grupno != -1)
		{
			text += " AND cha_grupno=@cha_grupno ";
		}
		if (FirmaNo != -1)
		{
			text += " AND cha_firmano=@cha_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND cha_subeno=@cha_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND cha_srmrkkodu=@cha_srmrkkodu ";
		}
		if (ProjeDetayli)
		{
			text += " AND cha_projekodu=@cha_projekodu ";
		}
		List<CariEkstre> list = new List<CariEkstre>();
		List<CariEkstre> list2 = new List<CariEkstre>();
		try
		{
			string commandText = "SELECT cha_tarihi,cha_vade,cha_tip,cha_evrak_tip,cha_evrakno_seri,cha_evrakno_sira,cha_grupno,cha_d_cins,cha_meblag,cha_d_kur FROM CARI_HESAP_HAREKETLERI WHERE cha_kod=@cha_kod AND cha_tip=0" + text + " ORDER BY cha_tarihi DESC, cha_evrakno_sira DESC";
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@cha_kod", (object)cari_kod);
			if (cha_grupno != -1)
			{
				val.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			}
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@cha_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@cha_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@cha_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			if (ProjeDetayli)
			{
				val.Parameters.AddWithValue("@cha_projekodu", (object)ProjeKodu);
			}
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				CariEkstre cariEkstre = new CariEkstre();
				cariEkstre.BaslikMi = false;
				cariEkstre.cha_evrak_tip = (enum_cha_evrak_tip)val2.GetSafeByte(3);
				cariEkstre.cha_evrakno_seri = val2.GetSafeString(4);
				cariEkstre.cha_evrakno_sira = val2.GetSafeInt32(5);
				cariEkstre.cha_grupno = val2.GetSafeByte(6);
				cariEkstre.cha_meblag = val2.GetSafeDouble(8);
				cariEkstre.cha_tarihi = val2.GetSafeDateTime(0);
				cariEkstre.cha_d_kur = val2.GetSafeDouble(9);
				cariEkstre.cha_vade = val2.GetSafeInt32(1);
				cariEkstre.cha_tip = (enum_cha_tip)val2.GetSafeByte(2);
				cariEkstre.DovizCinsi = val2.GetSafeByte(7);
				DateTime vadeTarihi = DateTime.Now;
				if (cariEkstre.cha_vade.ToString().Length == 8)
				{
					vadeTarihi = new DateTime(int.Parse(cariEkstre.cha_vade.ToString().Substring(0, 4)), int.Parse(cariEkstre.cha_vade.ToString().Substring(4, 2)), int.Parse(cariEkstre.cha_vade.ToString().Substring(6, 2)));
				}
				else if (cariEkstre.cha_vade == 0)
				{
					vadeTarihi = cariEkstre.cha_tarihi;
				}
				else if (cariEkstre.cha_vade < 0)
				{
					vadeTarihi = cariEkstre.cha_tarihi.AddDays(-1 * cariEkstre.cha_vade);
				}
				else
				{
					vadeTarihi = cariEkstre.cha_tarihi;
					try
					{
						SqliteCommand val3 = new SqliteCommand();
						val3.Connection = connection;
						((DbCommand)val3).CommandText = "SELECT odp_aratop FROM ODEME_PLANLARI WHERE odp_no=@odpno";
						val3.Parameters.AddWithValue("@odpno", (object)cariEkstre.cha_vade);
						int result = 0;
						try
						{
							object obj = ((DbCommand)val3).ExecuteScalar();
							int.TryParse(obj.ToString().Substring(0, obj.ToString().IndexOf("-")), out result);
						}
						catch
						{
						}
						vadeTarihi = cariEkstre.cha_tarihi.AddDays(result);
						((Component)val3).Dispose();
						val3 = null;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
				}
				cariEkstre.VadeTarihi = vadeTarihi;
				list.Add(cariEkstre);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
			double num = cari_bakiye;
			foreach (CariEkstre item in (IEnumerable<CariEkstre>)Enumerable.OrderByDescending<CariEkstre, DateTime>((IEnumerable<CariEkstre>)list, (Func<CariEkstre, DateTime>)((CariEkstre x) => x.VadeTarihi)))
			{
				if (!(num <= 0.0))
				{
					double num2 = item.cha_meblag * item.cha_d_kur;
					if (num < num2)
					{
						item.cha_meblag = num / item.cha_d_kur;
					}
					item.Bakiye = num;
					list2.Add(item);
					num -= num2;
					continue;
				}
				break;
			}
		}
		catch (Exception ex2)
		{
			Console.WriteLine(ex2.ToString());
		}
		return list2;
	}

	public static List<CariEkstre> GetCariEkstre(SqliteConnection connection, Cari cari, DateTime baslangic_tarihi, DateTime bitis_tarihi, DovizCinsiTanimlari doviz_cinsi_tanimlari, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		List<CariEkstre> list = new List<CariEkstre>();
		if (cari.cari_doviz_cinsi != 255)
		{
			list.AddRange(CariEkstreGrupOlustur(connection, 0, cari.cari_doviz_cinsi, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi).Kur_sembol, cari, baslangic_tarihi, bitis_tarihi, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu));
			if (list[list.Count - 1].BaslikMi)
			{
				list.RemoveAt(list.Count - 1);
			}
		}
		if (cari.cari_doviz_cinsi1 != 255)
		{
			list.AddRange(CariEkstreGrupOlustur(connection, 1, cari.cari_doviz_cinsi1, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi1).Kur_sembol, cari, baslangic_tarihi, bitis_tarihi, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu));
			if (list[list.Count - 1].BaslikMi)
			{
				list.RemoveAt(list.Count - 1);
			}
		}
		if (cari.cari_doviz_cinsi2 != 255)
		{
			list.AddRange(CariEkstreGrupOlustur(connection, 2, cari.cari_doviz_cinsi2, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi2).Kur_sembol, cari, baslangic_tarihi, bitis_tarihi, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu));
			if (list[list.Count - 1].BaslikMi)
			{
				list.RemoveAt(list.Count - 1);
			}
		}
		return list;
	}

	private static double GetCariEkstreDevredenBakiye(SqliteConnection OpenedConnection, string cha_kod, int cha_grupno, enum_cha_tip cha_tip, DateTime cha_tarihi, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		string text = "";
		if (FirmaNo != -1)
		{
			text += " AND cha_firmano=@cha_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND cha_subeno=@cha_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND cha_srmrkkodu=@cha_srmrkkodu ";
		}
		double result = 0.0;
		try
		{
			string commandText = "SELECT IFNULL((SELECT SUM(cha_meblag) FROM CARI_HESAP_HAREKETLERI WHERE cha_kod=@cha_kod AND cha_grupno=@cha_grupno AND cha_tip=@cha_tip AND cha_tarihi<@cha_tarihi" + text + "),0) AS cha_meblag";
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@cha_kod", (object)cha_kod);
			val.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			val.Parameters.AddWithValue("@cha_tip", (object)(int)cha_tip);
			val.Parameters.AddWithValue("@cha_tarihi", (object)cha_tarihi);
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@cha_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@cha_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@cha_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			result = double.Parse(((DbCommand)val).ExecuteScalar().ToString());
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	private static List<CariEkstre> GetCariEkstre(SqliteConnection OpenedConnection, string cha_kod, int cha_grupno, DateTime baslangic_tarihi, DateTime bitis_tarihi, int dovizcinsi, double OncekiBakiye, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		if (cha_grupno != -1)
		{
			text += " AND cha_grupno=@cha_grupno ";
		}
		if (FirmaNo != -1)
		{
			text += " AND cha_firmano=@cha_firmano ";
		}
		if (SubeNo != -1)
		{
			text += " AND cha_subeno=@cha_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text += " AND cha_srmrkkodu=@cha_srmrkkodu ";
		}
		List<CariEkstre> list = new List<CariEkstre>();
		try
		{
			string commandText = "SELECT MIN(cha_tarihi) AS cha_tarihi1,MIN(cha_vade) AS cha_vade1,MIN(cha_tip) AS cha_tip1,MIN(cha_evrak_tip) AS cha_evrak_tip1,MIN(cha_evrakno_seri) AS cha_evrakno_seri1,MIN(cha_evrakno_sira) AS cha_evrakno_sira1,SUM(cha_meblag) AS cha_meblag1,cha_kod FROM CARI_HESAP_HAREKETLERI WHERE (cha_kod=@cha_kod OR cha_ciro_cari_kodu=@cha_kod) AND cha_tarihi>=@baslangictarihi AND cha_tarihi<=@bitistarihi AND cha_grupno=@cha_grupno AND cha_cinsi<>11" + text + " GROUP BY cha_evrak_tip,cha_evrakno_seri,cha_evrakno_sira,cha_tarihi,cha_vade,cha_tip,cha_kod ORDER BY cha_tarihi,cha_evrakno_seri,cha_evrakno_sira";
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@cha_kod", (object)cha_kod);
			val.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			val.Parameters.AddWithValue("@baslangictarihi", (object)baslangic_tarihi);
			val.Parameters.AddWithValue("@bitistarihi", (object)bitis_tarihi);
			if (cha_grupno != -1)
			{
				val.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			}
			if (FirmaNo != -1)
			{
				val.Parameters.AddWithValue("@cha_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val.Parameters.AddWithValue("@cha_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val.Parameters.AddWithValue("@cha_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				CariEkstre cariEkstre = new CariEkstre();
				cariEkstre.BaslikMi = false;
				cariEkstre.cha_tarihi = val2.GetSafeDateTime(0);
				cariEkstre.cha_vade = val2.GetSafeInt32(1);
				cariEkstre.cha_tip = (enum_cha_tip)val2.GetSafeByte(2);
				cariEkstre.cha_evrak_tip = (enum_cha_evrak_tip)val2.GetSafeByte(3);
				cariEkstre.cha_evrakno_seri = val2.GetSafeString(4);
				cariEkstre.cha_evrakno_sira = val2.GetSafeInt32(5);
				cariEkstre.cha_meblag = val2.GetSafeDouble(6);
				cariEkstre.cha_grupno = cha_grupno;
				cariEkstre.DovizCinsi = dovizcinsi;
				if (cha_kod == val2.GetSafeString(7))
				{
					OncekiBakiye = ((cariEkstre.cha_tip != enum_cha_tip.Borc) ? (OncekiBakiye - cariEkstre.cha_meblag) : (OncekiBakiye + cariEkstre.cha_meblag));
				}
				cariEkstre.Bakiye = OncekiBakiye;
				DateTime vadeTarihi = DateTime.Now;
				if (cariEkstre.cha_vade.ToString().Length == 8)
				{
					vadeTarihi = new DateTime(int.Parse(cariEkstre.cha_vade.ToString().Substring(0, 4)), int.Parse(cariEkstre.cha_vade.ToString().Substring(4, 2)), int.Parse(cariEkstre.cha_vade.ToString().Substring(6, 2)));
				}
				else if (cariEkstre.cha_vade == 0)
				{
					vadeTarihi = cariEkstre.cha_tarihi;
				}
				else if (cariEkstre.cha_vade < 0)
				{
					vadeTarihi = cariEkstre.cha_tarihi.AddDays(-1 * cariEkstre.cha_vade);
				}
				else
				{
					vadeTarihi = cariEkstre.cha_tarihi;
					try
					{
						SqliteCommand val3 = new SqliteCommand
						{
							Connection = OpenedConnection,
							CommandText = "SELECT odp_aratop FROM ODEME_PLANLARI WHERE odp_no=@odpno"
						};
						val3.Parameters.AddWithValue("@odpno", (object)cariEkstre.cha_vade);
						object obj = ((DbCommand)val3).ExecuteScalar();
						string s = obj.ToString().Substring(0, obj.ToString().IndexOf("-"));
						int result = 0;
						int.TryParse(s, out result);
						vadeTarihi = cariEkstre.cha_tarihi.AddDays(result);
						((Component)val3).Dispose();
					}
					catch
					{
					}
				}
				cariEkstre.VadeTarihi = vadeTarihi;
				list.Add(cariEkstre);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return list;
	}

	private static List<CariEkstre> CariEkstreGrupOlustur(SqliteConnection connection, int GrupNo, int DovizKodu, string DovizSembol, Cari cari, DateTime baslangic_tarihi, DateTime bitis_tarihi, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		List<CariEkstre> list = new List<CariEkstre>();
		double num = 0.0;
		CariEkstre cariEkstre = new CariEkstre();
		cariEkstre.BaslikMesaji = DovizSembol + " EKSTRESİ";
		cariEkstre.BaslikMi = true;
		cariEkstre.DovizCinsi = DovizKodu;
		list.Add(cariEkstre);
		double cariEkstreDevredenBakiye = GetCariEkstreDevredenBakiye(connection, cari.cari_kod, GrupNo, enum_cha_tip.Borc, baslangic_tarihi, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu);
		num += cariEkstreDevredenBakiye;
		double cariEkstreDevredenBakiye2 = GetCariEkstreDevredenBakiye(connection, cari.cari_kod, GrupNo, enum_cha_tip.Alacak, baslangic_tarihi, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu);
		num -= cariEkstreDevredenBakiye2;
		CariEkstre cariEkstre2 = new CariEkstre();
		cariEkstre2.Bakiye = num;
		cariEkstre2.BaslikMi = false;
		cariEkstre2.cha_evrak_tip = enum_cha_evrak_tip.Acilis;
		cariEkstre2.cha_evrakno_seri = "";
		cariEkstre2.cha_evrakno_sira = -1;
		cariEkstre2.cha_grupno = GrupNo;
		cariEkstre2.cha_meblag = num;
		cariEkstre2.cha_tarihi = baslangic_tarihi;
		cariEkstre2.cha_tip = enum_cha_tip.BorcVeAlacak;
		cariEkstre2.DovizCinsi = DovizKodu;
		if (num != 0.0)
		{
			list.Add(cariEkstre2);
		}
		list.AddRange(GetCariEkstre(connection, cari.cari_kod, GrupNo, baslangic_tarihi, bitis_tarihi, DovizKodu, num, FirmaNo, SubeNo, SorumlulukMerkeziDetayli, SorumlulukMerkeziKodu));
		return list;
	}

	public static List<CariListItem> GetCariListItems(SqliteConnection OpenedConnection, string Searchstring, enum_CariListelemeSecenekleri CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu, int TemsilciCariKartMiAdresMi, CariSiralamaSekli SiralamaSekli)
	{
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Expected O, but got Unknown
		string text = "SELECT DISTINCT cari_kod,cari_unvan1,cari_unvan2,cari_doviz_cinsi,cari_hareket_tipi,cari_tipi FROM CARI_HESAPLAR ";
		if (TemsilciCariKartMiAdresMi == 1)
		{
			text += " LEFT JOIN CARI_HESAP_ADRESLERI AS adr ON CARI_HESAPLAR.cari_kod=adr.adr_cari_kod ";
		}
		string text2 = "";
		text2 = ((text.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) != -1) ? " AND" : " WHERE");
		string text3 = " LIMIT 6000";
		if (Searchstring != "")
		{
			text3 = " LIMIT 200";
			text = text + text2 + " ((cari_kod_upper like '%" + Searchstring.ToUpper() + "%') OR (cari_unvan1_upper like '%" + Searchstring.ToUpper() + "%'))";
			text2 = " AND";
		}
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.BolgeKodunaGore)
		{
			text = text + text2 + " cari_bolge_kodu in ('" + CariListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
		}
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.GrupKodunaGore)
		{
			text = text + text2 + " cari_grup_kodu in ('" + CariListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
		}
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.SektorKodunaGore)
		{
			text = text + text2 + " cari_sektor_kodu in ('" + CariListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
		}
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.TemsilciKodunaGore)
		{
			string text4 = "";
			text4 = ((TemsilciCariKartMiAdresMi != 0) ? "adr.adr_temsilci_kodu" : "cari_temsilci_kodu");
			if (CariListelemeAktifGrup == "")
			{
				CariListelemeAktifGrup = TemsilciKodu;
			}
			text = text + text2 + " " + text4 + " in ('" + CariListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
		}
		text += " ORDER BY ";
		switch (SiralamaSekli)
		{
		case CariSiralamaSekli.IsmeGoreArtan:
			text += " cari_unvan1_upper ";
			break;
		case CariSiralamaSekli.IsmeGoreAzalan:
			text += " cari_unvan1_upper DESC ";
			break;
		case CariSiralamaSekli.KodaGoreArtan:
			text += " cari_kod_upper ";
			break;
		case CariSiralamaSekli.KodaGoreAzalan:
			text += " cari_kod_upper DESC ";
			break;
		}
		text = text + " " + text3;
		List<CariListItem> list = new List<CariListItem>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				CariListItem cariListItem = new CariListItem();
				cariListItem.cari_kod = val2.GetSafeString(0);
				cariListItem.cari_unvan1 = val2.GetSafeString(1);
				cariListItem.cari_unvan2 = val2.GetSafeString(2);
				cariListItem.cari_doviz_cinsi = val2.GetSafeByte(3);
				cariListItem.cari_hareket_tipi = (enum_cari_hareket_tipi)val2.GetSafeByte(4);
				cariListItem.cari_tipi = (enum_cari_tipi)val2.GetSafeByte(5);
				list.Add(cariListItem);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch
		{
		}
		return list;
	}

	public static CariDetayBilgileri GetCariDetayBilgileri(SqliteConnection connection, CariDetayBilgileri caridetaybilgileri, DovizCinsiTanimlari doviz_cinsi_tanimlari)
	{
		caridetaybilgileri.cari = GetCariByCariKod(connection, caridetaybilgileri.cari.cari_kod, AdreslerTemsilciyeGore: false, "");
		caridetaybilgileri.aktif_doviz_kuru = KurSqlite.GetKur(connection, caridetaybilgileri.GetDovizCinsi(), "1", DateTime.Now).dov_fiyat;
		if (caridetaybilgileri.Parametreler.FirmaNo == -1)
		{
			caridetaybilgileri.Parametreler.FirmaAdi = AppResource.genel_hepsi;
		}
		else
		{
			caridetaybilgileri.Parametreler.FirmaAdi = FirmaSqlite.GetFirma(connection, caridetaybilgileri.Parametreler.FirmaNo).fir_unvan;
		}
		caridetaybilgileri.temsilci_adi = TemsilciSqlite.GetTemsilciAdi(connection, caridetaybilgileri.cari.cari_temsilci_kodu);
		caridetaybilgileri.odeme_plani = OdemePlaniSqlite.GetOdemePlani(connection, caridetaybilgileri.cari.cari_odemeplan_no);
		caridetaybilgileri.fiyat_listesi = FiyatListesiSqlite.GetFiyatListesi(connection, caridetaybilgileri.cari.cari_satis_fk);
		caridetaybilgileri.cari_grup = CariGrupSqlite.GetCariGrup(connection, caridetaybilgileri.cari.cari_grup_kodu);
		caridetaybilgileri.cari_bolge = CariBolgeSqlite.GetCariBolge(connection, caridetaybilgileri.cari.cari_bolge_kodu);
		caridetaybilgileri.bakiye = GetBakiye(connection, caridetaybilgileri.cari.cari_kod, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		try
		{
			DateTime referansTarihi = new DateTime(DateTime.Now.Year, 1, 1);
			int adatGunu = GetAdatGunu(connection, caridetaybilgileri.cari.cari_kod, referansTarihi, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
			caridetaybilgileri.adat_vadesi = referansTarihi.AddDays(adatGunu);
		}
		catch
		{
			caridetaybilgileri.adat_vadesi = DateTime.MinValue;
		}
		caridetaybilgileri.faturalasmamis_irsaliye_tutari = GetFaturalasmamisIrsaliyeTutari(connection, caridetaybilgileri.cari, caridetaybilgileri.cari.cari_kod, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		caridetaybilgileri.karsilanmamis_siparis_tutari = GetKarsilanmamisSiparisTutari(connection, caridetaybilgileri.cari.cari_kod, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		caridetaybilgileri.odenmemis_cek_tutari_kendisi = GetOdenmemisCekTutari(connection, caridetaybilgileri.cari, caridetaybilgileri.Parametreler.cek_riski_cirolanmamis_cekin_risk_suresi, enum_sck_imza.Kendisi, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		caridetaybilgileri.odenmemis_cek_tutari_musterisi = GetOdenmemisCekTutari(connection, caridetaybilgileri.cari, caridetaybilgileri.Parametreler.cek_riski_cirolanmamis_cekin_risk_suresi, enum_sck_imza.Musterisi, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		int dovizCinsi = 0;
		switch (caridetaybilgileri.Parametreler.CariGrupNo)
		{
		case 0:
			dovizCinsi = caridetaybilgileri.cari.cari_doviz_cinsi;
			break;
		case 1:
			dovizCinsi = caridetaybilgileri.cari.cari_doviz_cinsi1;
			break;
		case 2:
			dovizCinsi = caridetaybilgileri.cari.cari_doviz_cinsi2;
			break;
		}
		caridetaybilgileri.cari_teminatlari = GetTeminatTutari(connection, caridetaybilgileri.cari.cari_kod, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu, dovizCinsi);
		caridetaybilgileri.anadoviz_bakiye = GetBakiye(connection, caridetaybilgileri.cari.cari_kod, -1, caridetaybilgileri.Parametreler.FirmaNo, -1, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		caridetaybilgileri.anadoviz_odenmemis_cek_tutari_kendisi = GetOdenmemisCekTutari(connection, caridetaybilgileri.cari, caridetaybilgileri.Parametreler.cek_riski_cirolanmamis_cekin_risk_suresi, enum_sck_imza.Kendisi, -1, caridetaybilgileri.Parametreler.FirmaNo, -1, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		caridetaybilgileri.anadoviz_odenmemis_cek_tutari_musterisi = GetOdenmemisCekTutari(connection, caridetaybilgileri.cari, caridetaybilgileri.Parametreler.cek_riski_cirolanmamis_cekin_risk_suresi, enum_sck_imza.Musterisi, -1, caridetaybilgileri.Parametreler.FirmaNo, -1, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		caridetaybilgileri.anadoviz_karsilanmamis_siparis_tutari = GetKarsilanmamisSiparisTutari(connection, caridetaybilgileri.cari.cari_kod, -1, caridetaybilgileri.Parametreler.FirmaNo, -1, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		caridetaybilgileri.anadoviz_faturalasmamis_irsaliye_tutari = GetFaturalasmamisIrsaliyeTutari(connection, caridetaybilgileri.cari, caridetaybilgileri.cari.cari_kod, -1, caridetaybilgileri.Parametreler.FirmaNo, -1, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		if (caridetaybilgileri.Parametreler.bu_yil_cirosu_goster)
		{
			DateTime tarihbaslangic = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);
			DateTime tarihbitis = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
			caridetaybilgileri.bu_yil_cirosu = GetCiroTutari(connection, caridetaybilgileri.cari.cari_kod, tarihbaslangic, tarihbitis, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		}
		if (caridetaybilgileri.Parametreler.gecen_yil_cirosu_goster)
		{
			DateTime tarihbaslangic2 = new DateTime(DateTime.Now.Year - 1, 1, 1, 0, 0, 0);
			DateTime tarihbitis2 = new DateTime(DateTime.Now.Year - 1, 12, 31, 23, 59, 59);
			caridetaybilgileri.gecen_yil_cirosu = GetCiroTutari(connection, caridetaybilgileri.cari.cari_kod, tarihbaslangic2, tarihbitis2, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		}
		if (caridetaybilgileri.Parametreler.son_borc_hareketi_goster)
		{
			enum_cha_evrak_tip evraktipi = enum_cha_evrak_tip.Tanimsiz;
			DateTime evraktarihi = DateTime.MinValue;
			double tutar = 0.0;
			CariHesapHareketleriSqlite.GetSonEvrakCariHareketleri(connection, caridetaybilgileri.cari.cari_kod, enum_cha_tip.Borc, out evraktipi, out evraktarihi, out tutar, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
			caridetaybilgileri.son_borc_hareketi_evrak_tipi = evraktipi;
			caridetaybilgileri.son_borc_hareketi_tarihi = evraktarihi;
			caridetaybilgileri.son_borc_hareketi_tutari = tutar;
		}
		if (caridetaybilgileri.Parametreler.son_alacak_hareketi_goster)
		{
			enum_cha_evrak_tip evraktipi2 = enum_cha_evrak_tip.Tanimsiz;
			DateTime evraktarihi2 = DateTime.MinValue;
			double tutar2 = 0.0;
			CariHesapHareketleriSqlite.GetSonEvrakCariHareketleri(connection, caridetaybilgileri.cari.cari_kod, enum_cha_tip.Alacak, out evraktipi2, out evraktarihi2, out tutar2, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
			caridetaybilgileri.son_alacak_hareketi_evrak_tipi = evraktipi2;
			caridetaybilgileri.son_alacak_hareketi_tarihi = evraktarihi2;
			caridetaybilgileri.son_alacak_hareketi_tutari = tutar2;
		}
		caridetaybilgileri.cari_ekstre = new List<CariEkstre>();
		if (caridetaybilgileri.Parametreler.cari_ekstre_goster)
		{
			if (caridetaybilgileri.cari.cari_doviz_cinsi != 255)
			{
				caridetaybilgileri.cari_ekstre.AddRange(CariEkstreGrupOlustur(connection, 0, caridetaybilgileri.cari.cari_doviz_cinsi, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(caridetaybilgileri.cari.cari_doviz_cinsi).Kur_sembol, caridetaybilgileri.cari, caridetaybilgileri.Parametreler.cari_ekstre_baslangic_tarihi, caridetaybilgileri.Parametreler.cari_ekstre_bitis_tarihi, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu));
				if (caridetaybilgileri.cari_ekstre[caridetaybilgileri.cari_ekstre.Count - 1].BaslikMi)
				{
					caridetaybilgileri.cari_ekstre.RemoveAt(caridetaybilgileri.cari_ekstre.Count - 1);
				}
			}
			if (caridetaybilgileri.cari.cari_doviz_cinsi1 != 255)
			{
				caridetaybilgileri.cari_ekstre.AddRange(CariEkstreGrupOlustur(connection, 1, caridetaybilgileri.cari.cari_doviz_cinsi1, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(caridetaybilgileri.cari.cari_doviz_cinsi1).Kur_sembol, caridetaybilgileri.cari, caridetaybilgileri.Parametreler.cari_ekstre_baslangic_tarihi, caridetaybilgileri.Parametreler.cari_ekstre_bitis_tarihi, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu));
				if (caridetaybilgileri.cari_ekstre[caridetaybilgileri.cari_ekstre.Count - 1].BaslikMi)
				{
					caridetaybilgileri.cari_ekstre.RemoveAt(caridetaybilgileri.cari_ekstre.Count - 1);
				}
			}
			if (caridetaybilgileri.cari.cari_doviz_cinsi2 != 255)
			{
				caridetaybilgileri.cari_ekstre.AddRange(CariEkstreGrupOlustur(connection, 2, caridetaybilgileri.cari.cari_doviz_cinsi2, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(caridetaybilgileri.cari.cari_doviz_cinsi2).Kur_sembol, caridetaybilgileri.cari, caridetaybilgileri.Parametreler.cari_ekstre_baslangic_tarihi, caridetaybilgileri.Parametreler.cari_ekstre_bitis_tarihi, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu));
				if (caridetaybilgileri.cari_ekstre[caridetaybilgileri.cari_ekstre.Count - 1].BaslikMi)
				{
					caridetaybilgileri.cari_ekstre.RemoveAt(caridetaybilgileri.cari_ekstre.Count - 1);
				}
			}
		}
		caridetaybilgileri.yapilacak_tahsilatlar = new List<CariEkstre>();
		if (caridetaybilgileri.Parametreler.yapilacak_tahsilatlar_goster)
		{
			CariEkstre cariEkstre = new CariEkstre();
			cariEkstre.BaslikMi = true;
			cariEkstre.BaslikMesaji = "YAPILACAK TAHSİLAT DETAYI";
			cariEkstre.DovizCinsi = 0;
			caridetaybilgileri.yapilacak_tahsilatlar.Add(cariEkstre);
			caridetaybilgileri.yapilacak_tahsilatlar.AddRange(GetYapilacakTahsilatlar(connection, caridetaybilgileri.cari.cari_kod, caridetaybilgileri.bakiye, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu, ProjeDetayli: false, ""));
		}
		caridetaybilgileri.onceki_siparisler = new List<SIPARISLER>();
		if (caridetaybilgileri.Parametreler.onceki_siparisler_goster)
		{
			caridetaybilgileri.onceki_siparisler = SiparislerSqlite.GetSiparisSatirlariByCariKod(connection, caridetaybilgileri.cari.cari_kod, enum_sip_orderby.SiparisTarihi, 500, "", siparis_teslim_durumu.Tumu, enum_sip_tip.Talep, caridetaybilgileri.Parametreler.CariGrupNo, caridetaybilgileri.Parametreler.FirmaNo, caridetaybilgileri.Parametreler.SubeNo, caridetaybilgileri.Parametreler.SorumlulukMerkeziDetayli, caridetaybilgileri.Parametreler.SorumlulukMerkeziKodu);
		}
		caridetaybilgileri.yaz_boz_tahtasi = "";
		if (caridetaybilgileri.Parametreler.yaz_boz_tahtasi_goster)
		{
			caridetaybilgileri.yaz_boz_tahtasi = GetTextData(connection, caridetaybilgileri.cari.cari_kod);
		}
		return caridetaybilgileri;
	}

	public static GenelListArray GetCariIskontoGruplari(SqliteConnection OpenedConnection)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT isk_cari_kod FROM STOK_CARI_ISKONTO_TANIMLARI GROUP BY isk_cari_kod";
		GenelListArray genelListArray = new GenelListArray();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = val2.GetSafeString(0);
				genelList.Text = val2.GetSafeString(0);
				genelListArray.genel_list.Add(genelList);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch
		{
		}
		return genelListArray;
	}

	public static string GetTextData(SqliteConnection connection, string cari_kod)
	{
		string result = "";
		string text = "";
		text = ((AppBase.MikroVersiyonu <= 15) ? "SELECT Data FROM mye_TextData WHERE TableID=31 AND RecID_DBCno=(SELECT cari_RECid_DBCno FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod) AND RecID_RECno=(SELECT cari_RECid_RECno FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod)" : "SELECT Data FROM mye_TextData WHERE TableID=31 AND Record_uid=(SELECT cari_Guid FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod)");
		try
		{
			SqliteCommand val = connection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = text;
				val.Parameters.AddWithValue("@cari_kod", (object)cari_kod);
				result = ((DbCommand)val).ExecuteScalar().ToString();
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
		return result;
	}
}
