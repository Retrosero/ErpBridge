using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.CariHesaplar.CariTeminatBilgileri;
using Fora.Mikro.CariHesaplar.CariYetkilileri;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Enumler;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CariData
{
	public static Cari GetCariByRECno(SqlConnection connection, int cari_RECno, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByFieldAdi(connection, "cari_RECno", cari_RECno.ToString(), AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByGuid(SqlConnection connection, Guid cari_Guid, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByFieldAdi(connection, "cari_Guid", cari_Guid.ToString(), AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByCariKod(SqlConnection connection, string cari_kod, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByFieldAdi(connection, "cari_kod", cari_kod, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByCariUnvan(SqlConnection connection, string cari_unvan, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByFieldAdi(connection, "cari_unvan", cari_unvan, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByVergiTcKimlikNo(SqlConnection connection, string vergi_tc_kimlik_no, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByFieldAdi(connection, "cari_vdaire_no", vergi_tc_kimlik_no, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByEMail(SqlConnection connection, string e_mail, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByFieldAdi(connection, "cari_EMail", e_mail, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	public static Cari GetCariByBankaHesapNo(SqlConnection connection, string banka_hesap_no, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		return GetCariByFieldAdi(connection, "cari_banka_hesapno1", banka_hesap_no, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	private static Cari GetCariByFieldAdi(SqlConnection connection, string aranacak_field_adi, string aranacak_deger, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		if (aranacak_deger == null)
		{
			return new Cari();
		}
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(connection.Database);
		string text = "cari_tipi";
		if (mikroVersiyon >= 15)
		{
			text = "cari_baglanti_tipi";
		}
		string sqlstring = "SELECT cari_kod,cari_unvan1,cari_unvan2,cari_muh_kod,cari_muh_kod1,cari_muh_kod2,cari_vdaire_adi,cari_vdaire_no,cari_Ana_cari_kodu,cari_bolge_kodu,cari_grup_kodu,cari_temsilci_kodu,cari_sektor_kodu,cari_satis_isk_kod,cari_special1,cari_special2,cari_special3,cari_sicil_no,cari_VergiKimlikNo,cari_vade_fark_yuz,cari_vade_fark_yuz1,cari_vade_fark_yuz2," + text + ",cari_doviz_cinsi,cari_doviz_cinsi1,cari_doviz_cinsi2,cari_odeme_gunu,cari_hareket_tipi,cari_odemeplan_no,cari_satis_fk,cari_KurHesapSekli,cari_odeme_cinsi,cari_fatura_adres_no,cari_sevk_adres_no,cari_banka_hesapno1,cari_CepTel,cari_EMail,cari_VarsayilanGirisDepo,cari_VarsayilanCikisDepo,cari_cari_kilitli_flg FROM CARI_HESAPLAR WITH (NOLOCK) WHERE " + aranacak_field_adi + "=@deger";
		return GetCariBySqlString(connection, sqlstring, aranacak_deger, AdreslerTemsilciyeGore, TemsilciKodu);
	}

	private static Cari GetCariBySqlString(SqlConnection connection, string sqlstring, string deger, bool AdreslerTemsilciyeGore, string TemsilciKodu)
	{
		Cari cari = new Cari();
		using (SqlCommand sqlCommand = connection.CreateCommand())
		{
			sqlCommand.CommandText = sqlstring;
			sqlCommand.Parameters.AddWithValue("@deger", deger);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				cari.cari_kod = sqlDataReader.GetSafeString(0);
				cari.cari_unvan1 = sqlDataReader.GetSafeString(1);
				cari.cari_unvan2 = sqlDataReader.GetSafeString(2);
				cari.cari_muh_kod = sqlDataReader.GetSafeString(3);
				cari.cari_muh_kod1 = sqlDataReader.GetSafeString(4);
				cari.cari_muh_kod2 = sqlDataReader.GetSafeString(5);
				cari.cari_vdaire_adi = sqlDataReader.GetSafeString(6);
				cari.cari_vdaire_no = sqlDataReader.GetSafeString(7);
				cari.cari_Ana_cari_kodu = sqlDataReader.GetSafeString(8);
				cari.cari_bolge_kodu = sqlDataReader.GetSafeString(9);
				cari.cari_grup_kodu = sqlDataReader.GetSafeString(10);
				cari.cari_temsilci_kodu = sqlDataReader.GetSafeString(11);
				cari.cari_sektor_kodu = sqlDataReader.GetSafeString(12);
				cari.cari_satis_isk_kod = sqlDataReader.GetSafeString(13);
				cari.cari_special1 = sqlDataReader.GetSafeString(14);
				cari.cari_special2 = sqlDataReader.GetSafeString(15);
				cari.cari_special3 = sqlDataReader.GetSafeString(16);
				cari.cari_sicil_no = sqlDataReader.GetSafeString(17);
				cari.cari_VergiKimlikNo = sqlDataReader.GetSafeString(18);
				cari.cari_vade_fark_yuz = (float)sqlDataReader.GetSafeDouble(19);
				cari.cari_vade_fark_yuz1 = (float)sqlDataReader.GetSafeDouble(20);
				cari.cari_vade_fark_yuz2 = (float)sqlDataReader.GetSafeDouble(21);
				cari.cari_tipi = sqlDataReader.GetSafeByte(22);
				cari.cari_doviz_cinsi = sqlDataReader.GetSafeByte(23);
				cari.cari_doviz_cinsi1 = sqlDataReader.GetSafeByte(24);
				cari.cari_doviz_cinsi2 = sqlDataReader.GetSafeByte(25);
				cari.cari_odeme_gunu = sqlDataReader.GetSafeByte(26);
				cari.cari_hareket_tipi = sqlDataReader.GetSafeByte(27);
				cari.cari_odemeplan_no = sqlDataReader.GetSafeInt32(28);
				cari.cari_satis_fk = sqlDataReader.GetSafeInt32(29);
				cari.cari_KurHesapSekli = sqlDataReader.GetSafeByte(30);
				cari.cari_odeme_cinsi = sqlDataReader.GetSafeByte(31);
				cari.cari_fatura_adres_no = sqlDataReader.GetSafeInt32(32);
				cari.cari_sevk_adres_no = sqlDataReader.GetSafeInt32(33);
				cari.cari_banka_hesapno1 = sqlDataReader.GetSafeString(34);
				cari.cari_CepTel = sqlDataReader.GetSafeString(35);
				cari.cari_Email = sqlDataReader.GetSafeString(36);
				cari.cari_VarsayilanGirisDepo = sqlDataReader.GetSafeInt32(37);
				cari.cari_VarsayilanCikisDepo = sqlDataReader.GetSafeInt32(38);
				cari.cari_cari_kilitli_flg = sqlDataReader.GetSafeBoolean(39);
			}
			sqlDataReader.Close();
		}
		List<CariAdres> list = new List<CariAdres>();
		string text = "SELECT adr_cari_kod,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_yon_kodu,adr_temsilci_kodu,adr_ozel_not,adr_ziyaretgunu,adr_gps_enlem,adr_gps_boylam,adr_adres_no,adr_uzaklik_kodu,adr_ziyaretperyodu,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7 FROM CARI_HESAP_ADRESLERI WITH(NOLOCK) WHERE adr_cari_kod=@cari_kod";
		if (AdreslerTemsilciyeGore)
		{
			text = text + " AND adr_temsilci_kodu='" + TemsilciKodu + "'";
		}
		using (SqlCommand sqlCommand2 = connection.CreateCommand())
		{
			sqlCommand2.CommandText = text;
			sqlCommand2.Parameters.AddWithValue("@cari_kod", cari.cari_kod);
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				CariAdres cariAdres = new CariAdres();
				cariAdres.adr_cari_kod = sqlDataReader2.GetSafeString(0);
				cariAdres.adr_cadde = sqlDataReader2.GetSafeString(1);
				cariAdres.adr_sokak = sqlDataReader2.GetSafeString(2);
				cariAdres.adr_posta_kodu = sqlDataReader2.GetSafeString(3);
				cariAdres.adr_ilce = sqlDataReader2.GetSafeString(4);
				cariAdres.adr_il = sqlDataReader2.GetSafeString(5);
				cariAdres.adr_ulke = sqlDataReader2.GetSafeString(6);
				cariAdres.adr_tel_ulke_kodu = sqlDataReader2.GetSafeString(7);
				cariAdres.adr_tel_bolge_kodu = sqlDataReader2.GetSafeString(8);
				cariAdres.adr_tel_no1 = sqlDataReader2.GetSafeString(9);
				cariAdres.adr_tel_no2 = sqlDataReader2.GetSafeString(10);
				cariAdres.adr_tel_faxno = sqlDataReader2.GetSafeString(11);
				cariAdres.adr_tel_modem = sqlDataReader2.GetSafeString(12);
				cariAdres.adr_yon_kodu = sqlDataReader2.GetSafeString(13);
				cariAdres.adr_temsilci_kodu = sqlDataReader2.GetSafeString(14);
				cariAdres.adr_ozel_not = sqlDataReader2.GetSafeString(15);
				cariAdres.adr_ziyaretgunu = sqlDataReader2.GetSafeDouble(16).ToString();
				cariAdres.adr_gps_enlem = (float)sqlDataReader2.GetSafeDouble(17);
				cariAdres.adr_gps_boylam = (float)sqlDataReader2.GetSafeDouble(18);
				cariAdres.adr_adres_no = sqlDataReader2.GetSafeInt32(19);
				cariAdres.adr_uzaklik_kodu = sqlDataReader2.GetSafeInt16(20);
				cariAdres.adr_ziyaretperyodu = sqlDataReader2.GetSafeByte(21);
				cariAdres.adr_ziyarethaftasi = sqlDataReader2.GetSafeByte(22);
				cariAdres.adr_ziygunu2_1 = sqlDataReader2.GetSafeBoolean(23);
				cariAdres.adr_ziygunu2_2 = sqlDataReader2.GetSafeBoolean(24);
				cariAdres.adr_ziygunu2_3 = sqlDataReader2.GetSafeBoolean(25);
				cariAdres.adr_ziygunu2_4 = sqlDataReader2.GetSafeBoolean(26);
				cariAdres.adr_ziygunu2_5 = sqlDataReader2.GetSafeBoolean(27);
				cariAdres.adr_ziygunu2_6 = sqlDataReader2.GetSafeBoolean(28);
				cariAdres.adr_ziygunu2_7 = sqlDataReader2.GetSafeBoolean(29);
				cariAdres.CariUnvan = cari.cari_unvan1 + " " + cari.cari_unvan2;
				list.Add(cariAdres);
			}
			sqlDataReader2.Close();
		}
		cari.CariAdresleri = list;
		List<CariYetkili> list2 = new List<CariYetkili>();
		string commandText = "SELECT mye_cari_kod,mye_isim,mye_soyisim,mye_es_isim,mye_dahili_telno,mye_email_adres,mye_cep_telno,mye_tc_kimlikno,mye_vergi_dairesi,mye_vergi_kimlikno,mye_dogum_yeri,mye_ev_cadde,mye_ev_sokak,mye_ev_posta_kodu,mye_ev_ilce,mye_ev_il,mye_ev_ulke,mye_is_telno,mye_ev_telno,mye_adres_no,mye_unvan,mye_hitap,mye_hisse,mye_tahsil,mye_dogum_tarihi,mye_evlilik_tarihi,mye_es_dogum_tarihi FROM CARI_HESAP_YETKILILERI WITH(NOLOCK) WHERE mye_cari_kod=@cari_kod";
		using (SqlCommand sqlCommand3 = connection.CreateCommand())
		{
			sqlCommand3.CommandText = commandText;
			sqlCommand3.Parameters.AddWithValue("@cari_kod", cari.cari_kod);
			SqlDataReader sqlDataReader3 = sqlCommand3.ExecuteReader();
			while (sqlDataReader3.Read())
			{
				CariYetkili cariYetkili = new CariYetkili();
				cariYetkili.mye_cari_kod = sqlDataReader3.GetSafeString(0);
				cariYetkili.mye_isim = sqlDataReader3.GetSafeString(1);
				cariYetkili.mye_soyisim = sqlDataReader3.GetSafeString(2);
				cariYetkili.mye_es_isim = sqlDataReader3.GetSafeString(3);
				cariYetkili.mye_dahili_telno = sqlDataReader3.GetSafeString(4);
				cariYetkili.mye_email_adres = sqlDataReader3.GetSafeString(5);
				cariYetkili.mye_cep_telno = sqlDataReader3.GetSafeString(6);
				cariYetkili.mye_tc_kimlikno = sqlDataReader3.GetSafeString(7);
				cariYetkili.mye_vergi_dairesi = sqlDataReader3.GetSafeString(8);
				cariYetkili.mye_vergi_kimlikno = sqlDataReader3.GetSafeString(9);
				cariYetkili.mye_dogum_yeri = sqlDataReader3.GetSafeString(10);
				cariYetkili.mye_ev_cadde = sqlDataReader3.GetSafeString(11);
				cariYetkili.mye_ev_sokak = sqlDataReader3.GetSafeString(12);
				cariYetkili.mye_ev_posta_kodu = sqlDataReader3.GetSafeString(13);
				cariYetkili.mye_ev_ilce = sqlDataReader3.GetSafeString(14);
				cariYetkili.mye_ev_il = sqlDataReader3.GetSafeString(15);
				cariYetkili.mye_ev_ulke = sqlDataReader3.GetSafeString(16);
				cariYetkili.mye_is_telno = sqlDataReader3.GetSafeString(17);
				cariYetkili.mye_ev_telno = sqlDataReader3.GetSafeString(18);
				cariYetkili.mye_adres_no = sqlDataReader3.GetSafeInt32(19);
				cariYetkili.mye_unvan = (enum_CariYetkiliUnvan)sqlDataReader3.GetSafeByte(20);
				cariYetkili.mye_hitap = sqlDataReader3.GetSafeByte(21);
				cariYetkili.mye_hisse = sqlDataReader3.GetSafeByte(22);
				cariYetkili.mye_tahsil = sqlDataReader3.GetSafeByte(23);
				cariYetkili.mye_dogum_tarihi = sqlDataReader3.GetSafeDateTime(24);
				cariYetkili.mye_evlilik_tarihi = sqlDataReader3.GetSafeDateTime(25);
				cariYetkili.mye_es_dogum_tarihi = sqlDataReader3.GetSafeDateTime(26);
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
			sqlDataReader3.Close();
		}
		cari.CariYetkilileri = list2;
		return cari;
	}

	public static string GetCariUnvan1(SqlConnection OpenedConnection, string cari_kod)
	{
		string text = "";
		SqlCommand sqlCommand = new SqlCommand("SELECT cari_unvan1 FROM CARI_HESAPLAR WITH(NOLOCK) WHERE cari_kod=@cari_kod", OpenedConnection);
		sqlCommand.Parameters.AddWithValue("@cari_kod", cari_kod);
		text = sqlCommand.ExecuteScalar().ToString();
		sqlCommand.Dispose();
		return text;
	}

	public static double GetBakiye(SqlConnection connection, string cari_kod, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
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
		string text4 = "SELECT round (ISNULL((select SUM(" + text3 + text2 + ") from CARI_HESAP_HAREKETLERI WITH(NOLOCK) WHERE cha_tip=0 AND cha_kod=@cha_kod AND cha_cari_cins=0" + text + "),0)-ISNULL((select SUM(" + text3 + text2 + ") from CARI_HESAP_HAREKETLERI WITH(NOLOCK)  WHERE cha_tip=1 AND cha_kod=@cha_kod AND cha_cari_cins=0" + text + "),0),2) AS Tutar";
		double result = 0.0;
		try
		{
			SqlCommand sqlCommand = new SqlCommand(text4, connection);
			sqlCommand.CommandText = text4;
			sqlCommand.Parameters.AddWithValue("@cha_kod", cari_kod);
			if (cha_grupno != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_grupno", cha_grupno);
			}
			if (FirmaNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_firmano", FirmaNo);
			}
			if (SubeNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_subeno", SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", SorumlulukMerkeziKodu);
			}
			result = double.Parse(sqlCommand.ExecuteScalar().ToString());
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return result;
	}

	public static int GetAdatGunu(SqlConnection opennedconnection, string cari_kodu, DateTime ReferansTarihi, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
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
		string commandText = "SELECT cha_tip,cha_tarihi,cha_vade,(cha_aratoplam-cha_ft_iskonto1-cha_ft_iskonto2-cha_ft_iskonto3-cha_ft_iskonto4-cha_ft_iskonto5-cha_ft_iskonto6)" + text2 + " AS ARATOPLAM,(cha_ft_masraf1+cha_ft_masraf2+cha_ft_masraf3+cha_ft_masraf4)" + text2 + " AS MASRAF,(cha_vergi1+cha_vergi2+cha_vergi3+cha_vergi4+cha_vergi5+cha_vergi6+cha_vergi7+cha_vergi8+cha_vergi9+cha_vergi10)" + text2 + " AS VERGI FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) WHERE cha_kod=@cari_kodu" + text;
		int result = 0;
		List<int> list = new List<int>();
		List<DateTime> list2 = new List<DateTime>();
		List<int> list3 = new List<int>();
		List<double> list4 = new List<double>();
		List<double> list5 = new List<double>();
		List<double> list6 = new List<double>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.Connection = opennedconnection;
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@cari_kodu", cari_kodu);
			if (cha_grupno != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_grupno", cha_grupno);
			}
			if (FirmaNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_firmano", FirmaNo);
			}
			if (SubeNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_subeno", SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", SorumlulukMerkeziKodu);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				list.Add(sqlDataReader.GetSafeByte(0));
				list2.Add(sqlDataReader.GetSafeDateTime(1));
				list3.Add(sqlDataReader.GetSafeInt32(2));
				list4.Add(sqlDataReader.GetSafeDouble(3));
				list5.Add(sqlDataReader.GetSafeDouble(4));
				list6.Add(sqlDataReader.GetSafeDouble(5));
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
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
						SqlCommand sqlCommand2 = new SqlCommand();
						sqlCommand2.Connection = opennedconnection;
						sqlCommand2.CommandText = "SELECT odp_aratop FROM ODEME_PLANLARI WITH(NOLOCK) WHERE odp_no=@odpno";
						sqlCommand2.Parameters.AddWithValue("@odpno", list3[i]);
						object obj = sqlCommand2.ExecuteScalar();
						string s = obj.ToString().Substring(0, obj.ToString().IndexOf("-"));
						int result2 = 0;
						int.TryParse(s, out result2);
						dateTime = list2[i].AddDays(result2);
						sqlCommand2.Dispose();
					}
					catch
					{
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
		catch
		{
		}
		return result;
	}

	public static double GetKarsilanmamisSiparisTutari(SqlConnection connection, string cari_kod, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
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
		SqlCommand sqlCommand = new SqlCommand("SELECT ROUND((ISNULL ((SELECT SUM((((sip_tutar- sip_iskonto_1-sip_iskonto_2 -sip_iskonto_3- sip_iskonto_4-sip_iskonto_5-sip_iskonto_6+sip_masraf_1+sip_masraf_2+sip_masraf_3+sip_masraf_4+sip_vergi+sip_masvergi)/sip_miktar)*( sip_miktar-sip_teslim_miktar ))" + text2 + ") FROM SIPARISLER WITH(NOLOCK) WHERE sip_musteri_kod=@cha_kod AND sip_kapat_fl = 0 AND sip_tip =0" + text + "),0)), 2)", connection);
		sqlCommand.Parameters.AddWithValue("@cha_kod", cari_kod);
		if (cha_grupno != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sip_cari_grupno", cha_grupno);
		}
		if (FirmaNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sip_firmano", FirmaNo);
		}
		if (SubeNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sip_subeno", SubeNo);
		}
		if (SorumlulukMerkeziDetayli)
		{
			sqlCommand.Parameters.AddWithValue("@sip_cari_sormerk", SorumlulukMerkeziKodu);
		}
		double result = double.Parse(sqlCommand.ExecuteScalar().ToString());
		sqlCommand.Dispose();
		sqlCommand = null;
		return result;
	}

	public static double GetFaturalasmamisIrsaliyeTutari(SqlConnection connection, Cari cari, string cari_kod, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(connection.Database);
		string text = "";
		string text2 = "";
		string text3 = "sth_fat_recid_recno=@faturano ";
		if (mikroVersiyon > 15)
		{
			text3 = "sth_fat_uid=@faturano ";
		}
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
		SqlCommand sqlCommand = new SqlCommand("SELECT ROUND((ISNULL (( SELECT SUM ( case when sth_normal_iade = 0 then ((sth_tutar- sth_iskonto1-sth_iskonto2-sth_iskonto3-sth_iskonto4-sth_iskonto5-sth_iskonto6+sth_masraf1+sth_masraf2+sth_masraf3+sth_masraf4+sth_vergi+sth_masraf_vergi)" + text2 + ") else -1*((sth_tutar- sth_iskonto1-sth_iskonto2-sth_iskonto3-sth_iskonto4-sth_iskonto5-sth_iskonto6+sth_masraf1+sth_masraf2+sth_masraf3+sth_masraf4+sth_vergi+sth_masraf_vergi)" + text2 + ") end) FROM STOK_HAREKETLERI WITH(NOLOCK) WHERE ( sth_tip=1 AND sth_cins in (0 ,1, 2) AND sth_evraktip= 1 AND sth_cari_kodu=@cha_kod AND " + text3 + text + ")),0)), 2) AS Tutar", connection);
		sqlCommand.Parameters.AddWithValue("@cha_kod", cari_kod);
		if (mikroVersiyon > 15)
		{
			sqlCommand.Parameters.AddWithValue("@faturano", Guid.Empty);
		}
		else
		{
			sqlCommand.Parameters.AddWithValue("@faturano", 0);
		}
		switch (cha_grupno)
		{
		case 0:
			sqlCommand.Parameters.AddWithValue("@sth_har_doviz_cinsi", cari.cari_doviz_cinsi);
			break;
		case 1:
			sqlCommand.Parameters.AddWithValue("@sth_har_doviz_cinsi", cari.cari_doviz_cinsi1);
			break;
		case 2:
			sqlCommand.Parameters.AddWithValue("@sth_har_doviz_cinsi", cari.cari_doviz_cinsi2);
			break;
		}
		if (FirmaNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sth_firmano", FirmaNo);
		}
		if (SubeNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sth_subeno", SubeNo);
		}
		if (SorumlulukMerkeziDetayli)
		{
			sqlCommand.Parameters.AddWithValue("@sth_cari_srm_merkezi", SorumlulukMerkeziKodu);
		}
		double result = double.Parse(sqlCommand.ExecuteScalar().ToString());
		sqlCommand.Dispose();
		sqlCommand = null;
		return result;
	}

	public static double GetOdenmemisCekTutari(SqlConnection connection, Cari cari, int CirolanmisCekinRiskSuresi, enum_sck_imza sck_imza, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(connection.Database);
		string text = "";
		if (mikroVersiyon == 14)
		{
			text = "[MikroDB_V14].[dbo].[DOVIZ_KURLARI]";
		}
		if (mikroVersiyon == 15)
		{
			text = "[MikroDB_V15].[dbo].[DOVIZ_KURLARI]";
		}
		if (mikroVersiyon == 16)
		{
			text = "[MikroDB_V16].[dbo].[DOVIZ_KURLARI]";
		}
		string text2 = "";
		if (cha_grupno != -1)
		{
			text2 += " AND sck_sahip_cari_grupno=@sck_sahip_cari_grupno ";
		}
		if (FirmaNo != -1)
		{
			text2 += " AND sck_firmano=@sck_firmano ";
		}
		if (SubeNo != -1)
		{
			text2 += " AND sck_subeno=@sck_subeno ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text2 += " AND sck_srmmrk=@sck_srmmrk ";
		}
		string text3 = "";
		if (sck_imza != enum_sck_imza.Tumu)
		{
			int num = (int)sck_imza;
			text3 = " sck_imza=" + num + " AND ";
		}
		string cmdText = "SELECT ROUND (( ISNULL ((  SELECT SUM(Tutar) FROM ( SELECT (sck_tutar*(SELECT ISNULL((SELECT TOP 1 dov_fiyat1 FROM " + text + " WHERE dov_no=sck_doviz AND dov_tarih<getdate() ORDER BY dov_tarih DESC),1))) AS Tutar FROM ODEME_EMIRLERI WITH(NOLOCK) WHERE " + text3 + " ((sck_sahip_cari_kodu=@car_kod AND sck_sahip_cari_cins=0) OR (sck_nerede_cari_kodu=@car_kod AND sck_nerede_cari_cins=0)) AND sck_tip in (0, 1, 2, 4, 5) AND sck_vade>DATEADD(day,(-1 * @CirolanmisCekinRiskSuresi),getdate()) AND sck_sonpoz in ( 0 , 1, 2 , 3 , 5 , 6 , 8 , 9 )" + text2 + ") AS Sonuc),0)), 2)";
		double num2 = 0.0;
		SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
		sqlCommand.Parameters.AddWithValue("@car_kod", cari.cari_kod);
		sqlCommand.Parameters.AddWithValue("@CirolanmisCekinRiskSuresi", CirolanmisCekinRiskSuresi.ToString());
		if (cha_grupno != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sck_sahip_cari_grupno", cha_grupno);
		}
		if (FirmaNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sck_firmano", FirmaNo);
		}
		if (SubeNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@sck_subeno", SubeNo);
		}
		if (SorumlulukMerkeziDetayli)
		{
			sqlCommand.Parameters.AddWithValue("@sck_srmmrk", SorumlulukMerkeziKodu);
		}
		num2 = double.Parse(sqlCommand.ExecuteScalar().ToString());
		sqlCommand.Dispose();
		sqlCommand = null;
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
			sqlCommand = new SqlCommand("SELECT TOP 1 dov_fiyat1 FROM " + text + " WHERE dov_no=@dov_no AND dov_tarih<=@dov_tarih ORDER BY dov_tarih DESC", connection);
			sqlCommand.Parameters.AddWithValue("@dov_no", num3);
			sqlCommand.Parameters.AddWithValue("@dov_tarih", DateTime.Now);
			num4 = double.Parse(sqlCommand.ExecuteScalar().ToString());
			sqlCommand.Dispose();
			sqlCommand = null;
			num2 /= num4;
		}
		return num2;
	}

	public static CariTeminatlari GetTeminatTutari(SqlConnection OpenedConnection, string cari_kod, int FirmaNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu, int DovizCinsi)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(OpenedConnection.Database);
		string text = "";
		if (mikroVersiyon == 14)
		{
			text = "[MikroDB_V14].[dbo].[DOVIZ_KURLARI]";
		}
		if (mikroVersiyon == 15)
		{
			text = "[MikroDB_V15].[dbo].[DOVIZ_KURLARI]";
		}
		if (mikroVersiyon == 16)
		{
			text = "[MikroDB_V16].[dbo].[DOVIZ_KURLARI]";
		}
		string text2 = "";
		if (FirmaNo != -1)
		{
			text2 += " AND (ct_GecerliFirma=@ct_GecerliFirma OR ct_GecerliFirma=-1) ";
		}
		if (SorumlulukMerkeziDetayli)
		{
			text2 += " AND ct_srmrkkodu=@ct_srmrkkodu ";
		}
		string cmdText = "SELECT ct_Aciklama_no,ct_tutari*(SELECT ISNULL((SELECT TOP 1 dov_fiyat1 FROM " + text + " WHERE dov_no=ct_DovizCinsi AND dov_tarih<getdate() ORDER BY dov_tarih DESC),1)) AS Tutar FROM CARI_HESAP_TEMINATLARI WHERE ct_carikodu='" + cari_kod + "' AND ct_vade>=getdate() " + text2;
		List<CariTeminat> list = new List<CariTeminat>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, OpenedConnection);
			if (FirmaNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@ct_GecerliFirma", FirmaNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				sqlCommand.Parameters.AddWithValue("@ct_srmrkkodu", SorumlulukMerkeziKodu);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				CariTeminat cariTeminat = new CariTeminat();
				cariTeminat.teminat_tipi = (enum_cari_teminat_tipleri)sqlDataReader.GetSafeByte(0);
				cariTeminat.Tutar = sqlDataReader.GetSafeDouble(1);
				list.Add(cariTeminat);
			}
			sqlCommand.Dispose();
			sqlCommand = null;
			cmdText = (cmdText = "SELECT (ISNULL ((SELECT TOP 1 dov_fiyat1 FROM " + text + " WHERE dov_no=" + DovizCinsi + " AND dov_tarih<=getdate() ORDER BY dov_tarih DESC),1))");
			sqlCommand = new SqlCommand(cmdText, OpenedConnection);
			double num = double.Parse(sqlCommand.ExecuteScalar().ToString());
			sqlCommand.Dispose();
			sqlCommand = null;
			foreach (CariTeminat item in list)
			{
				item.Tutar /= num;
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
		return new CariTeminatlari(list);
	}

	public static double GetCiroTutari(SqlConnection OpenedConnection, string cari_kod, DateTime tarihbaslangic, DateTime tarihbitis, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
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
		SqlCommand sqlCommand = new SqlCommand("SELECT ROUND (( ISNULL ((SELECT SUM(((cha_aratoplam -cha_ft_iskonto1- cha_ft_iskonto2-cha_ft_iskonto3 -cha_ft_iskonto4- cha_ft_iskonto5-cha_ft_iskonto6 ) * case when ( cha_tip=1 AND cha_normal_Iade= 1) THEN - 1 ELSE 1 END)" + text2 + ") FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) WHERE cha_ciro_cari_kodu=@car_kod AND (cha_tarihi>=@tarihbaslangic AND cha_tarihi<=@tarihbitis AND ((cha_tip= 1 AND cha_normal_Iade=1 AND cha_evrak_tip=0) OR (cha_tip=0 AND cha_normal_Iade=0 AND cha_evrak_tip=63)) AND (cha_grupno in (0 ,1, 2) OR cha_ciro_cari_kodu <> cha_kod))" + text + "), 0)), 2)", OpenedConnection);
		sqlCommand.Parameters.AddWithValue("@car_kod", cari_kod);
		sqlCommand.Parameters.AddWithValue("@tarihbaslangic", tarihbaslangic);
		sqlCommand.Parameters.AddWithValue("@tarihbitis", tarihbitis);
		if (cha_grupno != -1)
		{
			sqlCommand.Parameters.AddWithValue("@cha_grupno", cha_grupno);
		}
		if (FirmaNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@cha_firmano", FirmaNo);
		}
		if (SubeNo != -1)
		{
			sqlCommand.Parameters.AddWithValue("@cha_subeno", SubeNo);
		}
		if (SorumlulukMerkeziDetayli)
		{
			sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", SorumlulukMerkeziKodu);
		}
		double result = double.Parse(sqlCommand.ExecuteScalar().ToString());
		sqlCommand.Dispose();
		sqlCommand = null;
		return result;
	}

	public static DataTable GetCarilerDataTable(SqlConnection OpenedConnection)
	{
		int num = 15;
		if (OpenedConnection.Database.StartsWith("MikroDB_V12"))
		{
			num = 12;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V14"))
		{
			num = 14;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V15"))
		{
			num = 15;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V16"))
		{
			num = 16;
		}
		string text = "cari_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "cari_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "cari_kod AS 'KOD',cari_unvan1 + ' ' + cari_unvan2 AS 'İSİM', dbo.fn_CariHareketTip(cari_hareket_tipi) AS 'HAREKET TİPİ',cari_sektor_kodu AS 'SEKTÖR KODU',cari_grup_kodu AS 'GRUP KODU',cari_temsilci_kodu AS 'TEMSİLCİ KODU',cari_bolge_kodu AS 'BÖLGE KODU',cari_Ana_cari_kodu AS 'ANA CARİ KODU',cari_vdaire_no AS 'VD. NO',cari_vdaire_adi AS 'VD. ADI',cari_EMail AS 'E-POSTA',cari_CepTel AS 'GSM',dbo.fn_FaxBul(cari_kod, cari_fatura_adres_no) AS 'FAKS' FROM CARI_HESAPLAR WITH (NOLOCK) ORDER BY cari_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "CARI_HESAPLAR";
		return dataTable;
	}

	public static bool GetKilitliMi(SqlConnection connection, string cari_kod)
	{
		bool result = false;
		SqlCommand sqlCommand = new SqlCommand("SELECT cari_cari_kilitli_flg FROM CARI_HESAPLAR WITH(NOLOCK) WHERE cari_kod=@cari_kod", connection);
		sqlCommand.Parameters.AddWithValue("@cari_kod", cari_kod);
		if (sqlCommand.ExecuteScalar().ToString() == "1")
		{
			result = true;
		}
		sqlCommand.Dispose();
		return result;
	}

	public static bool GetEFaturaCarisiMi(SqlConnection connection, string cari_kod)
	{
		bool result = false;
		using (SqlCommand sqlCommand = connection.CreateCommand())
		{
			sqlCommand.CommandText = "SELECT cari_efatura_fl FROM CARI_HESAPLAR WITH(NOLOCK) WHERE cari_kod=@cari_kod";
			sqlCommand.Parameters.AddWithValue("@cari_kod", cari_kod);
			object obj = sqlCommand.ExecuteScalar();
			if (obj != null && !(obj is DBNull))
			{
				result = (bool)obj;
			}
		}
		return result;
	}

	public static bool GetZiyaretEdilmisMi(SqlConnection connection, string DBName, string Temsilci_Kodu, string Cari_Kodu, int Cari_Adres_No, DateTime Tarihi)
	{
		bool result = false;
		using (SqlCommand sqlCommand = connection.CreateCommand())
		{
			sqlCommand.CommandText = "SELECT zyrt_Temsilci_Kodu FROM _ZIYARET_HAREKETLERI WITH(NOLOCK) WHERE zyrt_Temsilci_Kodu=@zyrt_Temsilci_Kodu AND zyrt_Cari_Kodu=@zyrt_Cari_Kodu AND zyrt_Cari_Adres_No=@zyrt_Cari_Adres_No AND zyrt_Tarihi=@zyrt_Tarihi";
			sqlCommand.Parameters.AddWithValue("@zyrt_Temsilci_Kodu", Temsilci_Kodu);
			sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Kodu", Cari_Kodu);
			sqlCommand.Parameters.AddWithValue("@zyrt_Cari_Adres_No", Cari_Adres_No);
			sqlCommand.Parameters.AddWithValue("@zyrt_Tarihi", Tarihi);
			if (sqlCommand.ExecuteScalar() != null)
			{
				result = true;
			}
		}
		return result;
	}

	public static int GetCariIlkAdresNo(SqlConnection OpenedConnection, string cari_kod)
	{
		int result = 1;
		SqlCommand sqlCommand = new SqlCommand("SELECT adr_adres_no FROM CARI_HESAP_ADRESLERI WITH(NOLOCK) WHERE adr_cari_kod=@cari_kod ORDER BY adr_adres_no", OpenedConnection);
		sqlCommand.Parameters.AddWithValue("@cari_kod", cari_kod);
		try
		{
			result = int.Parse(sqlCommand.ExecuteScalar().ToString());
		}
		catch
		{
		}
		sqlCommand.Dispose();
		sqlCommand = null;
		return result;
	}

	public static List<CariEkstre> GetYapilacakTahsilatlar(SqlConnection connection, string cari_kod, double cari_bakiye, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu, bool ProjeDetayli, string ProjeKodu)
	{
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
			SqlCommand sqlCommand = new SqlCommand("SELECT cha_tarihi,cha_vade,cha_tip,cha_evrak_tip,cha_evrakno_seri,cha_evrakno_sira,cha_grupno,cha_d_cins,cha_meblag,cha_d_kur FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) WHERE cha_kod=@cha_kod AND cha_tip=0" + text + " ORDER BY cha_tarihi DESC, cha_evrakno_sira DESC", connection);
			sqlCommand.Parameters.AddWithValue("@cha_kod", cari_kod);
			if (cha_grupno != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_grupno", cha_grupno);
			}
			if (FirmaNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_firmano", FirmaNo);
			}
			if (SubeNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_subeno", SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", SorumlulukMerkeziKodu);
			}
			if (ProjeDetayli)
			{
				sqlCommand.Parameters.AddWithValue("@cha_projekodu", ProjeKodu);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				CariEkstre cariEkstre = new CariEkstre();
				cariEkstre.BaslikMi = false;
				cariEkstre.cha_evrak_tip = (enum_cha_evrak_tip)sqlDataReader.GetSafeByte(3);
				cariEkstre.cha_evrakno_seri = sqlDataReader.GetSafeString(4);
				cariEkstre.cha_evrakno_sira = sqlDataReader.GetSafeInt32(5);
				cariEkstre.cha_grupno = sqlDataReader.GetSafeByte(6);
				cariEkstre.cha_meblag = sqlDataReader.GetSafeDouble(8);
				cariEkstre.cha_tarihi = sqlDataReader.GetSafeDateTime(0);
				cariEkstre.cha_d_kur = sqlDataReader.GetSafeDouble(9);
				cariEkstre.cha_vade = sqlDataReader.GetSafeInt32(1);
				cariEkstre.cha_tip = (enum_cha_tip)sqlDataReader.GetSafeByte(2);
				cariEkstre.DovizCinsi = sqlDataReader.GetSafeByte(7);
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
						SqlCommand sqlCommand2 = new SqlCommand();
						sqlCommand2.Connection = connection;
						sqlCommand2.CommandText = "SELECT odp_aratop FROM ODEME_PLANLARI WITH(NOLOCK) WHERE odp_no=@odpno";
						sqlCommand2.Parameters.AddWithValue("@odpno", cariEkstre.cha_vade);
						object obj = sqlCommand2.ExecuteScalar();
						string s = obj.ToString().Substring(0, obj.ToString().IndexOf("-"));
						int result = 0;
						int.TryParse(s, out result);
						vadeTarihi = cariEkstre.cha_tarihi.AddDays(result);
						sqlCommand2.Dispose();
					}
					catch
					{
					}
				}
				cariEkstre.VadeTarihi = vadeTarihi;
				list.Add(cariEkstre);
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
			double num = cari_bakiye;
			foreach (CariEkstre item in list.OrderByDescending((CariEkstre x) => x.VadeTarihi))
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
		catch
		{
		}
		return list2;
	}

	public static List<CariEkstre> GetCariEkstre(SqlConnection connection, Cari cari, DateTime baslangic_tarihi, DateTime bitis_tarihi, DovizCinsiTanimlari doviz_cinsi_tanimlari, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
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

	private static double GetCariEkstreDevredenBakiye(SqlConnection OpenedConnection, string cha_kod, int cha_grupno, enum_cha_tip cha_tip, DateTime cha_tarihi, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
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
			SqlCommand sqlCommand = new SqlCommand("SELECT ISNULL((SELECT SUM(cha_meblag) FROM CARI_HESAP_HAREKETLERI WHERE cha_kod=@cha_kod AND cha_grupno=@cha_grupno AND cha_tip=@cha_tip AND cha_tarihi<@cha_tarihi" + text + "),0) AS cha_meblag", OpenedConnection);
			sqlCommand.Parameters.AddWithValue("@cha_kod", cha_kod);
			sqlCommand.Parameters.AddWithValue("@cha_grupno", cha_grupno);
			sqlCommand.Parameters.AddWithValue("@cha_tip", (int)cha_tip);
			sqlCommand.Parameters.AddWithValue("@cha_tarihi", cha_tarihi);
			if (FirmaNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_firmano", FirmaNo);
			}
			if (SubeNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_subeno", SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", SorumlulukMerkeziKodu);
			}
			result = double.Parse(sqlCommand.ExecuteScalar().ToString());
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return result;
	}

	private static List<CariEkstre> GetCariEkstre(SqlConnection OpenedConnection, string cha_kod, int cha_grupno, DateTime baslangic_tarihi, DateTime bitis_tarihi, int dovizcinsi, double OncekiBakiye, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
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
			SqlCommand sqlCommand = new SqlCommand("SELECT MIN(cha_tarihi) AS cha_tarihi1,MIN(cha_vade) AS cha_vade1,MIN(cha_tip) AS cha_tip1,MIN(cha_evrak_tip) AS cha_evrak_tip1,MIN(cha_evrakno_seri) AS cha_evrakno_seri1,MIN(cha_evrakno_sira) AS cha_evrakno_sira1,SUM(cha_meblag) AS cha_meblag1,cha_kod FROM CARI_HESAP_HAREKETLERI WHERE (cha_kod=@cha_kod OR cha_ciro_cari_kodu=@cha_kod) AND cha_tarihi>=@baslangictarihi AND cha_tarihi<=@bitistarihi AND cha_grupno=@cha_grupno AND cha_cinsi<>11" + text + " GROUP BY cha_evrak_tip,cha_evrakno_seri,cha_evrakno_sira,cha_tarihi,cha_vade,cha_tip,cha_kod ORDER BY cha_tarihi,cha_evrakno_seri,cha_evrakno_sira", OpenedConnection);
			sqlCommand.Parameters.AddWithValue("@cha_kod", cha_kod);
			sqlCommand.Parameters.AddWithValue("@cha_grupno", cha_grupno);
			sqlCommand.Parameters.AddWithValue("@baslangictarihi", baslangic_tarihi);
			sqlCommand.Parameters.AddWithValue("@bitistarihi", bitis_tarihi);
			if (cha_grupno != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_grupno", cha_grupno);
			}
			if (FirmaNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_firmano", FirmaNo);
			}
			if (SubeNo != -1)
			{
				sqlCommand.Parameters.AddWithValue("@cha_subeno", SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", SorumlulukMerkeziKodu);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				CariEkstre cariEkstre = new CariEkstre();
				cariEkstre.BaslikMi = false;
				cariEkstre.cha_tarihi = sqlDataReader.GetSafeDateTime(0);
				cariEkstre.cha_vade = sqlDataReader.GetSafeInt32(1);
				cariEkstre.cha_tip = (enum_cha_tip)sqlDataReader.GetSafeByte(2);
				cariEkstre.cha_evrak_tip = (enum_cha_evrak_tip)sqlDataReader.GetSafeByte(3);
				cariEkstre.cha_evrakno_seri = sqlDataReader.GetSafeString(4);
				cariEkstre.cha_evrakno_sira = sqlDataReader.GetSafeInt32(5);
				cariEkstre.cha_meblag = sqlDataReader.GetSafeDouble(6);
				cariEkstre.cha_grupno = cha_grupno;
				cariEkstre.DovizCinsi = dovizcinsi;
				if (cha_kod == sqlDataReader.GetSafeString(7))
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
						SqlCommand sqlCommand2 = new SqlCommand();
						sqlCommand2.Connection = OpenedConnection;
						sqlCommand2.CommandText = "SELECT odp_aratop FROM ODEME_PLANLARI WHERE odp_no=@odpno";
						sqlCommand2.Parameters.AddWithValue("@odpno", cariEkstre.cha_vade);
						object obj = sqlCommand2.ExecuteScalar();
						string s = obj.ToString().Substring(0, obj.ToString().IndexOf("-"));
						int result = 0;
						int.TryParse(s, out result);
						vadeTarihi = cariEkstre.cha_tarihi.AddDays(result);
						sqlCommand2.Dispose();
					}
					catch
					{
					}
				}
				cariEkstre.VadeTarihi = vadeTarihi;
				list.Add(cariEkstre);
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return list;
	}

	private static List<CariEkstre> CariEkstreGrupOlustur(SqlConnection connection, int GrupNo, int DovizKodu, string DovizSembol, Cari cari, DateTime baslangic_tarihi, DateTime bitis_tarihi, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
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

	public static bool UpdateTextData(SqlConnection connection, string cari_kod, string data)
	{
		bool flag = false;
		string text = "";
		int num = 15;
		if (connection.Database.StartsWith("MikroDB_V12"))
		{
			num = 12;
		}
		if (connection.Database.StartsWith("MikroDB_V14"))
		{
			num = 14;
		}
		if (connection.Database.StartsWith("MikroDB_V15"))
		{
			num = 15;
		}
		if (connection.Database.StartsWith("MikroDB_V16"))
		{
			num = 16;
		}
		text = ((num <= 15) ? "DELETE mye_TextData WHERE TableID=31 AND RecID_DBCno=(SELECT cari_RECid_DBCno FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod) AND RecID_RECno=(SELECT cari_RECid_RECno FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod); INSERT INTO mye_TextData (TableID,RecID_DBCno,RecID_RECno,Data) VALUES (31,(SELECT cari_RECid_DBCno FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod),(SELECT cari_RECid_RECno FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod),@data);" : "DELETE mye_TextData WHERE TableID=31 AND Record_uid=(SELECT cari_Guid FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod); INSERT INTO mye_TextData (TableID,Record_uid,Data) VALUES (31,(SELECT cari_Guid FROM CARI_HESAPLAR WHERE cari_kod=@cari_kod),@data);");
		try
		{
			SqlCommand sqlCommand = new SqlCommand(text, connection);
			sqlCommand.CommandText = text;
			sqlCommand.Parameters.AddWithValue("@cari_kod", cari_kod);
			sqlCommand.Parameters.AddWithValue("@data", data);
			sqlCommand.ExecuteScalar();
			sqlCommand.Dispose();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool SetCariLokasyon(SqlConnection connection, CariAdres adres)
	{
		try
		{
			using (SqlCommand sqlCommand = connection.CreateCommand())
			{
				sqlCommand.CommandText = "UPDATE CARI_HESAP_ADRESLERI SET adr_gps_enlem=@adr_gps_enlem, adr_gps_boylam=@adr_gps_boylam, adr_lastup_date=getdate() WHERE adr_cari_kod=@cari_kod AND adr_adres_no=@adr_adres_no";
				sqlCommand.Parameters.AddWithValue("@cari_kod", adres.adr_cari_kod);
				sqlCommand.Parameters.AddWithValue("@adr_adres_no", adres.adr_adres_no);
				sqlCommand.Parameters.AddWithValue("@adr_gps_enlem", adres.adr_gps_enlem);
				sqlCommand.Parameters.AddWithValue("@adr_gps_boylam", adres.adr_gps_boylam);
				sqlCommand.ExecuteScalar();
			}
			return true;
		}
		catch
		{
			return false;
		}
	}
}
