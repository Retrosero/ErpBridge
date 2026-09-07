using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Rapor.YapilacakTahsilat;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RaporYapilacakTahsilatlarData
{
	public static List<GenelList> GetAlabilecegiRaporlarList(SqlConnection connection, string AlabilecegiRaporlar)
	{
		string text = "'" + AlabilecegiRaporlar.Replace(",", "','") + "'";
		string cmdText = "SELECT ParametreUser AS kod,ParametreDegeri AS isim FROM _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporYapilacakTahsilatlar' AND ParametreAdi='RaporAdi' AND ParametreUser in (" + text + ")";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeString(0);
				genelList.Text = sqlDataReader.GetSafeString(1);
				list.Add(genelList);
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

	public static RaporYapilacakTahsilatlarSonucHam GetRapor(SqlConnection connection, RaporYapilacakTahsilatlarSecenekleri rapor_secenekleri, string ProjeKodu, string SorumlulukMerkeziKodu, string CariPersonelKodu, string CariKodu, string CariBolgeKodu, string CariGrupKodu, DovizCinsiTanimlari dovizcinsitanimlari)
	{
		RaporYapilacakTahsilatlarSonucHam raporYapilacakTahsilatlarSonucHam = new RaporYapilacakTahsilatlarSonucHam();
		raporYapilacakTahsilatlarSonucHam.sonuc_ham = new List<RaporYapilacakTahsilatlarSatir>();
		string text = "";
		string text2 = "cha_meblag";
		text2 = "(CASE ";
		text2 += "WHEN (cha_cinsi IN (1,2,3,4,17,18,19,20,21,22)) AND (cha_cari_cins in (2, 4)) THEN cha_aratoplam ";
		text2 += "WHEN (cha_cinsi IN (9, 27)) AND (cha_cari_cins IN (3,5,9,12))  THEN cha_meblag - cha_vergi1 - cha_vergi2 - cha_vergi3 - cha_vergi4 - cha_vergi5 - cha_vergi6 - cha_vergi7 - cha_vergi8 - cha_vergi9 - cha_vergi10 ";
		text2 += "WHEN (cha_cinsi IN (8,14,11,10,28)) AND (cha_cari_cins IN (3, 5, 6, 8, 9, 12)) THEN cha_meblag - cha_vergi1 - cha_vergi2 - cha_vergi3 - cha_vergi4 - cha_vergi5 - cha_vergi6 - cha_vergi7 - cha_vergi8 - cha_vergi9 - cha_vergi10 ";
		text2 += "WHEN (cha_cinsi IN (13,29)) OR (cha_ticaret_turu in (2, 4) AND (cha_cinsi in (10, 11, 14, 15) OR (cha_cinsi in (8) AND cha_kasa_hizmet = 6))) OR (NOT(cha_cari_cins IN (0, 1, 2, 4, 6, 10, 11))) THEN cha_meblag - cha_vergi1 - cha_vergi2 - cha_vergi3 - cha_vergi4 - cha_vergi5 - cha_vergi6 - cha_vergi7 - cha_vergi8 - cha_vergi9 - cha_vergi10 ";
		text2 += "WHEN (cha_cinsi = 33) AND (cha_cari_cins IN (0, 1, 6)) THEN cha_aratoplam ";
		text2 += "WHEN (cha_cari_cins IN (10)) THEN cha_aratoplam ";
		text2 += "WHEN (cha_evrak_tip IN (108)) AND (cha_cari_cins IN (2)) THEN cha_aratoplam ";
		text2 += "WHEN (cha_evrak_tip IN (109)) AND (cha_cari_cins IN (11)) THEN cha_aratoplam ";
		text2 += "ELSE cha_meblag END)";
		text = "SELECT  cari_kod, cari_doviz_cinsi, cari_doviz_cinsi1, cari_doviz_cinsi2, cari_per_kod, cha_grupno, cha_firmano, cha_subeno,";
		text = ((!rapor_secenekleri.SorumlulukMerkeziDetayli) ? (text + " '',") : (text + " som_kod,"));
		text = ((!rapor_secenekleri.ProjeDetayli) ? (text + " '',") : (text + " pro_kodu,"));
		text += " SUM(MEBLAG) AS bakiye  FROM ( SELECT cari.cari_kod,cari.cari_doviz_cinsi,cari.cari_doviz_cinsi1,cari.cari_doviz_cinsi2,personel.cari_per_kod,ch.cha_grupno,cha_firmano,cha_subeno,";
		if (rapor_secenekleri.SorumlulukMerkeziDetayli)
		{
			text += "sm.som_kod,";
		}
		if (rapor_secenekleri.ProjeDetayli)
		{
			text += "p.pro_kodu,";
		}
		text = text + "ch.cha_tip,SUM(CASE WHEN cha_tip=0 THEN " + text2 + " ELSE (" + text2 + "*-1) END) AS MEBLAG FROM CARI_HESAP_HAREKETLERI AS ch WITH(NOLOCK) LEFT JOIN CARI_HESAPLAR AS cari WITH (NOLOCK) ON ch.cha_kod=cari.cari_kod LEFT JOIN CARI_PERSONEL_TANIMLARI AS personel WITH (NOLOCK) ON cari.cari_temsilci_kodu=personel.cari_per_kod LEFT JOIN SORUMLULUK_MERKEZLERI AS sm WITH (NOLOCK) ON ch.cha_srmrkkodu=sm.som_kod LEFT JOIN PROJELER AS p WITH (NOLOCK) ON ch.cha_projekodu=p.pro_kodu WHERE ch.cha_cari_cins=0";
		string text3 = "";
		text3 = "";
		switch (rapor_secenekleri.cari_temsilci_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text3 = rapor_secenekleri.cari_temsilci_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text3 = CariPersonelKodu;
			break;
		}
		if (text3 != "")
		{
			text = text + " AND cari.cari_temsilci_kodu in ('" + text3.Replace(",", "','") + "')";
		}
		switch (rapor_secenekleri.cari_arama_secenekleri)
		{
		case enum_cari_arama_secenekleri.Esittir:
			text = text + " AND cari.cari_kod='" + rapor_secenekleri.cari_arama_metin + "'";
			break;
		case enum_cari_arama_secenekleri.Icerir:
			text = text + " AND cari.cari_kod like ('%" + rapor_secenekleri.cari_arama_metin + "%')";
			break;
		case enum_cari_arama_secenekleri.IleBaslar:
			text = text + " AND cari.cari_kod like ('" + rapor_secenekleri.cari_arama_metin + "%')";
			break;
		case enum_cari_arama_secenekleri.TanimliOlan:
			if (CariKodu != "")
			{
				text = text + " AND cari.cari_kod in ('" + CariKodu.Replace(",", "','") + "')";
			}
			break;
		}
		text3 = "";
		switch (rapor_secenekleri.cari_bolge_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text3 = rapor_secenekleri.cari_bolge_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text3 = CariBolgeKodu;
			break;
		}
		if (text3 != "")
		{
			text = text + " AND cari.cari_bolge_kodu in ('" + text3.Replace(",", "','") + "')";
		}
		text3 = "";
		switch (rapor_secenekleri.cari_grup_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text3 = rapor_secenekleri.cari_grup_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text3 = CariGrupKodu;
			break;
		}
		if (text3 != "")
		{
			text = text + " AND cari.cari_grup_kodu in ('" + text3.Replace(",", "','") + "')";
		}
		text3 = "";
		switch (rapor_secenekleri.proje_kodlari_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text3 = rapor_secenekleri.proje_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text3 = ProjeKodu;
			break;
		}
		if (text3 != "")
		{
			text = text + " AND ch.cha_projekodu in ('" + text3.Replace(",", "','") + "')";
		}
		text3 = "";
		switch (rapor_secenekleri.sorumluluk_merkezleri_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text3 = rapor_secenekleri.sorumluluk_merkezleri;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text3 = SorumlulukMerkeziKodu;
			break;
		}
		if (text3 != "")
		{
			text = text + " AND ch.cha_srmrkkodu in ('" + text3.Replace(",", "','") + "')";
		}
		text3 = "";
		enum_tumu_tanimli_olan_listeden_sec firma_secenek = rapor_secenekleri.firma_secenek;
		if (firma_secenek == enum_tumu_tanimli_olan_listeden_sec.ListedenSec)
		{
			text3 = rapor_secenekleri.firma_nolari;
		}
		if (text3 != "")
		{
			text = text + " AND ch.cha_firmano in (" + text3 + ")";
		}
		text3 = "";
		firma_secenek = rapor_secenekleri.sube_secenek;
		if (firma_secenek == enum_tumu_tanimli_olan_listeden_sec.ListedenSec)
		{
			text3 = rapor_secenekleri.sube_nolari;
		}
		if (text3 != "")
		{
			text = text + " AND ch.cha_subeno in (" + text3 + ")";
		}
		text += " GROUP BY cari.cari_kod,cari.cari_doviz_cinsi,cari.cari_doviz_cinsi1,cari.cari_doviz_cinsi2,cari_per_kod,cha_grupno,cha_firmano,cha_subeno";
		if (rapor_secenekleri.SorumlulukMerkeziDetayli)
		{
			text += ",sm.som_kod";
		}
		if (rapor_secenekleri.ProjeDetayli)
		{
			text += ",p.pro_kodu";
		}
		text += ",ch.cha_tip) AS T";
		text += " GROUP BY cari_kod,cari_doviz_cinsi,cari_doviz_cinsi1,cari_doviz_cinsi2,cari_per_kod,cha_grupno,cha_firmano,cha_subeno";
		if (rapor_secenekleri.SorumlulukMerkeziDetayli)
		{
			text += ",som_kod";
		}
		if (rapor_secenekleri.ProjeDetayli)
		{
			text += ",pro_kodu";
		}
		text = text + " HAVING (SUM(MEBLAG)>" + rapor_secenekleri.MinimumBakiye + ")";
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<string> list5 = new List<string>();
		List<int> list6 = new List<int>();
		List<int> list7 = new List<int>();
		List<int> list8 = new List<int>();
		List<string> list9 = new List<string>();
		List<string> list10 = new List<string>();
		List<double> list11 = new List<double>();
		SqlCommand sqlCommand = new SqlCommand(text, connection);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			list.Add(sqlDataReader.GetSafeString(0));
			list2.Add(sqlDataReader.GetSafeByte(1));
			list3.Add(sqlDataReader.GetSafeByte(2));
			list4.Add(sqlDataReader.GetSafeByte(3));
			list5.Add(sqlDataReader.GetSafeString(4));
			list6.Add(sqlDataReader.GetSafeByte(5));
			list7.Add(sqlDataReader.GetSafeInt32(6));
			list8.Add(sqlDataReader.GetSafeInt32(7));
			list9.Add(sqlDataReader.GetSafeString(8));
			list10.Add(sqlDataReader.GetSafeString(9));
			list11.Add(sqlDataReader.GetSafeDouble(10));
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		List<int> list12 = new List<int>();
		if (rapor_secenekleri.doviz_cinsleri != "")
		{
			string[] array = rapor_secenekleri.doviz_cinsleri.Split(',');
			foreach (string s in array)
			{
				list12.Add(int.Parse(s));
			}
		}
		DateTime now = DateTime.Now;
		DateTime now2 = DateTime.Now;
		if (rapor_secenekleri.tarih_cinsi == enum_tarih_cinsi.OzelTarih)
		{
			now = rapor_secenekleri.baslangic_tarihi;
			now2 = rapor_secenekleri.bitis_tarihi;
		}
		else
		{
			now = MikroDataUtility.GetTarihCinsi(rapor_secenekleri.tarih_cinsi, BaslangicMi: true);
			now2 = MikroDataUtility.GetTarihCinsi(rapor_secenekleri.tarih_cinsi, BaslangicMi: false);
		}
		int num = 0;
		foreach (string item in list)
		{
			bool flag = true;
			if (list12.Count > 0)
			{
				flag = false;
				foreach (int item2 in list12)
				{
					int num2 = 0;
					switch (list6[num])
					{
					case 0:
						num2 = list2[num];
						break;
					case 1:
						num2 = list3[num];
						break;
					case 2:
						num2 = list4[num];
						break;
					}
					if (item2 == num2)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				foreach (CariEkstre item3 in CariData.GetYapilacakTahsilatlar(connection, item, list11[num], list6[num], list7[num], list8[num], rapor_secenekleri.SorumlulukMerkeziDetayli, list9[num], rapor_secenekleri.ProjeDetayli, list10[num]))
				{
					if (now <= item3.VadeTarihi && now2 >= item3.VadeTarihi)
					{
						raporYapilacakTahsilatlarSonucHam.sonuc_ham.Add(new RaporYapilacakTahsilatlarSatir(item3.VadeTarihi, list[num], list5[num], list9[num], list10[num], item3.cha_meblag, item3.DovizCinsi, list7[num], list8[num]));
					}
				}
			}
			num++;
		}
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> hashSet2 = new HashSet<string>();
		HashSet<string> hashSet3 = new HashSet<string>();
		HashSet<string> hashSet4 = new HashSet<string>();
		HashSet<int> hashSet5 = new HashSet<int>();
		HashSet<int> hashSet6 = new HashSet<int>();
		HashSet<int> hashSet7 = new HashSet<int>();
		foreach (RaporYapilacakTahsilatlarSatir item4 in raporYapilacakTahsilatlarSonucHam.sonuc_ham)
		{
			hashSet.Add(item4.cari_kod);
			hashSet2.Add(item4.plasiyer_kod);
			hashSet3.Add(item4.sorumluluk_merkezi_kod);
			hashSet4.Add(item4.proje_kod);
			hashSet5.Add(item4.firma_sira_no);
			hashSet6.Add(item4.sube_sira_no);
			hashSet7.Add(item4.doviz_cinsi);
		}
		raporYapilacakTahsilatlarSonucHam.cariler.Add(new RaporYardimciCariObje(0, 0, "TANIMSIZ", "TANIMSIZ", 0, 0));
		if (hashSet.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,cari_kod,cari_unvan1,cari_unvan2,bolge.bol_kod AS bol_kod,grup.crg_kod AS crg_kod FROM CARI_HESAPLAR  WITH (NOLOCK) LEFT JOIN CARI_HESAP_BOLGELERI AS bolge WITH (NOLOCK) ON CARI_HESAPLAR.cari_bolge_kodu=bolge.bol_kod LEFT JOIN CARI_HESAP_GRUPLARI AS grup WITH (NOLOCK) ON CARI_HESAPLAR.cari_grup_kodu=grup.crg_kod WHERE cari_kod in ('" + string.Join("','", hashSet) + "')", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num3 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.cariler.Add(new RaporYardimciCariObje(num3, sqlDataReader.GetSafeInt32(0), sqlDataReader.GetSafeString(1), sqlDataReader.GetSafeString(2) + " " + sqlDataReader.GetSafeString(3), sqlDataReader.GetSafeString(4), sqlDataReader.GetSafeString(5)));
				num3++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		raporYapilacakTahsilatlarSonucHam.temsilciler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet2.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,ISNULL(cari_per_kod,''),ISNULL(cari_per_adi,''),ISNULL(cari_per_soyadi,'') FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) WHERE cari_per_kod in ('" + string.Join("','", hashSet2) + "')", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num4 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.temsilciler.Add(new RaporYardimciGenelObje(num4, sqlDataReader.GetInt32(0), sqlDataReader.GetString(1), sqlDataReader.GetString(2) + " " + sqlDataReader.GetString(3)));
				num4++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		raporYapilacakTahsilatlarSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet3.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,ISNULL(som_kod,''),ISNULL(som_isim,'') FROM SORUMLULUK_MERKEZLERI WITH (NOLOCK) WHERE som_kod in ('" + string.Join("','", hashSet3) + "')", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num5 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(num5, sqlDataReader.GetInt32(0), sqlDataReader.GetString(1), sqlDataReader.GetString(2)));
				num5++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		raporYapilacakTahsilatlarSonucHam.projeler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet4.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,ISNULL(pro_kodu,''),ISNULL(pro_adi,'') FROM PROJELER WITH (NOLOCK) WHERE pro_kodu in ('" + string.Join("','", hashSet4) + "')", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num6 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.projeler.Add(new RaporYardimciGenelObje(num6, sqlDataReader.GetInt32(0), sqlDataReader.GetString(1), sqlDataReader.GetString(2)));
				num6++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		raporYapilacakTahsilatlarSonucHam.firmalar.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet5.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,fir_sirano,ISNULL(fir_unvan,'') FROM FIRMALAR WITH (NOLOCK) WHERE fir_sirano in (" + string.Join(",", hashSet5) + ")", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num7 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.firmalar.Add(new RaporYardimciGenelObje(num7, sqlDataReader.GetInt32(0), sqlDataReader.GetInt32(1).ToString(), sqlDataReader.GetString(2)));
				num7++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		raporYapilacakTahsilatlarSonucHam.subeler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet6.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,Sube_no,ISNULL(Sube_adi,'') FROM SUBELER WITH (NOLOCK) WHERE Sube_no in (" + string.Join(",", hashSet6) + ")", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num8 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.subeler.Add(new RaporYardimciGenelObje(num8, sqlDataReader.GetInt32(0), sqlDataReader.GetInt32(1).ToString(), sqlDataReader.GetString(2)));
				num8++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		int num9 = 0;
		foreach (int item5 in hashSet7)
		{
			raporYapilacakTahsilatlarSonucHam.doviz_cinsleri.Add(new RaporYardimciGenelObje(num9, item5, dovizcinsitanimlari.GetDovizCinsiTanimi(item5).Kur_sembol, dovizcinsitanimlari.GetDovizCinsiTanimi(item5).Kur_adi));
			num9++;
		}
		HashSet<string> hashSet8 = new HashSet<string>();
		HashSet<string> hashSet9 = new HashSet<string>();
		foreach (RaporYardimciCariObje item6 in raporYapilacakTahsilatlarSonucHam.cariler)
		{
			hashSet8.Add(item6.cari_bolge_kod);
			hashSet9.Add(item6.cari_grup_kod);
		}
		raporYapilacakTahsilatlarSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet8.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,ISNULL(bol_kod,''),ISNULL(bol_ismi,'') FROM CARI_HESAP_BOLGELERI WITH (NOLOCK) WHERE bol_kod in ('" + string.Join("','", hashSet8) + "')", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num10 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(num10, sqlDataReader.GetInt32(0), sqlDataReader.GetString(1), sqlDataReader.GetString(2)));
				num10++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		raporYapilacakTahsilatlarSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet9.Count > 0)
		{
			sqlCommand = new SqlCommand("SELECT 0,ISNULL(crg_kod,''),ISNULL(crg_isim,'') FROM CARI_HESAP_GRUPLARI WITH (NOLOCK) WHERE crg_kod in ('" + string.Join("','", hashSet9) + "')", connection);
			sqlDataReader = sqlCommand.ExecuteReader();
			int num11 = 1;
			while (sqlDataReader.Read())
			{
				raporYapilacakTahsilatlarSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(num11, sqlDataReader.GetInt32(0), sqlDataReader.GetString(1), sqlDataReader.GetString(2)));
				num11++;
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		foreach (RaporYapilacakTahsilatlarSatir item7 in raporYapilacakTahsilatlarSonucHam.sonuc_ham)
		{
			foreach (RaporYardimciCariObje item8 in raporYapilacakTahsilatlarSonucHam.cariler)
			{
				if (item8.Kodu == item7.cari_kod)
				{
					item7.cari_sira_no = item8.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item9 in raporYapilacakTahsilatlarSonucHam.temsilciler)
			{
				if (item9.Kodu == item7.plasiyer_kod)
				{
					item7.plasiyer_sira_no = item9.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item10 in raporYapilacakTahsilatlarSonucHam.sorumluluk_merkezleri)
			{
				if (item10.Kodu == item7.sorumluluk_merkezi_kod)
				{
					item7.sorumluluk_merkezi_sira_no = item10.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item11 in raporYapilacakTahsilatlarSonucHam.projeler)
			{
				if (item11.Kodu == item7.proje_kod)
				{
					item7.proje_sira_no = item11.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item12 in raporYapilacakTahsilatlarSonucHam.firmalar)
			{
				if (item12.Kodu == item7.firma_sira_no.ToString())
				{
					item7.firma_sira_no = item12.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item13 in raporYapilacakTahsilatlarSonucHam.subeler)
			{
				if (item13.Kodu == item7.sube_sira_no.ToString())
				{
					item7.sube_sira_no = item13.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item14 in raporYapilacakTahsilatlarSonucHam.doviz_cinsleri)
			{
				if (item14.Kodu == item7.doviz_cinsi.ToString())
				{
					item7.doviz_cinsi = item14.Position;
					break;
				}
			}
		}
		foreach (RaporYardimciCariObje item15 in raporYapilacakTahsilatlarSonucHam.cariler)
		{
			foreach (RaporYardimciGenelObje item16 in raporYapilacakTahsilatlarSonucHam.cari_bolgeleri)
			{
				if (item16.Kodu == item15.cari_bolge_kod)
				{
					item15.cari_bolge_sira_no = item16.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item17 in raporYapilacakTahsilatlarSonucHam.cari_gruplari)
			{
				if (item17.Kodu == item15.cari_grup_kod)
				{
					item15.cari_grup_sira_no = item17.Position;
					break;
				}
			}
		}
		return raporYapilacakTahsilatlarSonucHam;
	}
}
