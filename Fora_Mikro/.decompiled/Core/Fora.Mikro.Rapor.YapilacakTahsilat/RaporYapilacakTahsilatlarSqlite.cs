using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public static class RaporYapilacakTahsilatlarSqlite
{
	public static List<GenelList> GetAlabilecegiRaporlarList(SqliteConnection connection, string AlabilecegiRaporlar)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		string text = "'" + AlabilecegiRaporlar.Replace(",", "','") + "'";
		string commandText = "SELECT ParametreUser AS kod,ParametreDegeri AS isim FROM _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporYapilacakTahsilatlar' AND ParametreAdi='RaporAdi' AND ParametreUser in (" + text + ")";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			val.Connection = connection;
			((DbCommand)val).CommandText = commandText;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = val2.GetSafeString(0);
				genelList.Text = val2.GetSafeString(1);
				list.Add(genelList);
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

	public static RaporYapilacakTahsilatlarSonucHam GetRapor(SqliteConnection connection, RaporYapilacakTahsilatlarSecenekleri rapor_secenekleri, string ProjeKodu, string SorumlulukMerkeziKodu, string CariPersonelKodu, string CariKodu, string CariBolgeKodu, string CariGrupKodu, DovizCinsiTanimlari dovizcinsitanimlari)
	{
		//IL_093f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0946: Expected O, but got Unknown
		//IL_0a0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a16: Expected O, but got Unknown
		//IL_0acf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad6: Expected O, but got Unknown
		//IL_0b7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b84: Expected O, but got Unknown
		//IL_0c2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c32: Expected O, but got Unknown
		//IL_0d59: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce9: Expected O, but got Unknown
		//IL_0e5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e63: Expected O, but got Unknown
		//IL_0f0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f11: Expected O, but got Unknown
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Expected O, but got Unknown
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
		text = text + "ch.cha_tip,SUM(CASE WHEN cha_tip=0 THEN " + text2 + " ELSE (" + text2 + "*-1) END) AS MEBLAG FROM CARI_HESAP_HAREKETLERI AS ch  LEFT JOIN CARI_HESAPLAR AS cari ON ch.cha_kod=cari.cari_kod LEFT JOIN CARI_PERSONEL_TANIMLARI AS personel ON cari.cari_temsilci_kodu=personel.cari_per_kod LEFT JOIN SORUMLULUK_MERKEZLERI AS sm ON ch.cha_srmrkkodu=sm.som_kod LEFT JOIN PROJELER AS p ON ch.cha_projekodu=p.pro_kodu WHERE ch.cha_cari_cins=0";
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
		text += " GROUP BY cari.cari_kod,cari.cari_doviz_cinsi,cari.cari_doviz_cinsi1,cari.cari_doviz_cinsi2,cari_per_kod,ch.cha_grupno,ch.cha_firmano,ch.cha_subeno";
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
		try
		{
			SqliteCommand val = new SqliteCommand(text, connection);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeString(0));
				list2.Add(val2.GetSafeByte(1));
				list3.Add(val2.GetSafeByte(2));
				list4.Add(val2.GetSafeByte(3));
				list5.Add(val2.GetSafeString(4));
				list6.Add(val2.GetSafeByte(5));
				list7.Add(val2.GetSafeInt32(6));
				list8.Add(val2.GetSafeInt32(7));
				list9.Add(val2.GetSafeString(8));
				list10.Add(val2.GetSafeString(9));
				list11.Add(val2.GetSafeDouble(10));
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
		List<int> list12 = new List<int>();
		if (rapor_secenekleri.doviz_cinsleri != "")
		{
			try
			{
				string[] array = rapor_secenekleri.doviz_cinsleri.Split(new char[1] { ',' });
				foreach (string s in array)
				{
					list12.Add(int.Parse(s));
				}
			}
			catch
			{
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
			now = GenelUtility.GetTarihCinsi(rapor_secenekleri.tarih_cinsi, BaslangicMi: true);
			now2 = GenelUtility.GetTarihCinsi(rapor_secenekleri.tarih_cinsi, BaslangicMi: false);
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
				foreach (CariEkstre item3 in CariSqlite.GetYapilacakTahsilatlar(connection, item, list11[num], list6[num], list7[num], list8[num], rapor_secenekleri.SorumlulukMerkeziDetayli, list9[num], rapor_secenekleri.ProjeDetayli, list10[num]))
				{
					if (now <= item3.VadeTarihi && now2 >= item3.VadeTarihi)
					{
						raporYapilacakTahsilatlarSonucHam.sonuc_ham.Add(new RaporYapilacakTahsilatlarSatir(item3.VadeTarihi, list[num], list5[num], list9[num], list10[num], item3.cha_meblag, item3.DovizCinsi, list7[num], list8[num]));
					}
				}
			}
			num++;
		}
		HashSet<string> val3 = new HashSet<string>();
		HashSet<string> val4 = new HashSet<string>();
		HashSet<string> val5 = new HashSet<string>();
		HashSet<string> val6 = new HashSet<string>();
		HashSet<int> val7 = new HashSet<int>();
		HashSet<int> val8 = new HashSet<int>();
		HashSet<int> val9 = new HashSet<int>();
		foreach (RaporYapilacakTahsilatlarSatir item4 in raporYapilacakTahsilatlarSonucHam.sonuc_ham)
		{
			val3.Add(item4.cari_kod);
			val4.Add(item4.plasiyer_kod);
			val5.Add(item4.sorumluluk_merkezi_kod);
			val6.Add(item4.proje_kod);
			val7.Add(item4.firma_sira_no);
			val8.Add(item4.sube_sira_no);
			val9.Add(item4.doviz_cinsi);
		}
		raporYapilacakTahsilatlarSonucHam.cariler.Add(new RaporYardimciCariObje(0, 0, "TANIMSIZ", "TANIMSIZ", 0, 0));
		if (val3.Count > 0)
		{
			SqliteCommand val10 = new SqliteCommand("SELECT 0,cari_kod,cari_unvan1,cari_unvan2,bolge.bol_kod AS bol_kod,grup.crg_kod AS crg_kod FROM CARI_HESAPLAR  LEFT JOIN CARI_HESAP_BOLGELERI AS bolge ON CARI_HESAPLAR.cari_bolge_kodu=bolge.bol_kod LEFT JOIN CARI_HESAP_GRUPLARI AS grup ON CARI_HESAPLAR.cari_grup_kodu=grup.crg_kod WHERE cari_kod in ('" + string.Join("','", (IEnumerable<string?>)val3) + "')", connection);
			SqliteDataReader val11 = val10.ExecuteReader();
			int num3 = 1;
			while (((DbDataReader)val11).Read())
			{
				raporYapilacakTahsilatlarSonucHam.cariler.Add(new RaporYardimciCariObje(num3, val11.GetSafeInt32(0), val11.GetSafeString(1), val11.GetSafeString(2) + " " + val11.GetSafeString(3), val11.GetSafeString(4), val11.GetSafeString(5)));
				num3++;
			}
			((DbDataReader)val11).Close();
			((DbDataReader)val11).Dispose();
			val11 = null;
			((Component)val10).Dispose();
			val10 = null;
		}
		raporYapilacakTahsilatlarSonucHam.temsilciler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val4.Count > 0)
		{
			SqliteCommand val12 = new SqliteCommand("SELECT 0,IFNULL(cari_per_kod,''),IFNULL(cari_per_adi,''),IFNULL(cari_per_soyadi,'') FROM CARI_PERSONEL_TANIMLARI WHERE cari_per_kod in ('" + string.Join("','", (IEnumerable<string?>)val4) + "')", connection);
			SqliteDataReader val13 = val12.ExecuteReader();
			int num4 = 1;
			while (((DbDataReader)val13).Read())
			{
				raporYapilacakTahsilatlarSonucHam.temsilciler.Add(new RaporYardimciGenelObje(num4, ((DbDataReader)val13).GetInt32(0), ((DbDataReader)val13).GetString(1), ((DbDataReader)val13).GetString(2) + " " + ((DbDataReader)val13).GetString(3)));
				num4++;
			}
			((DbDataReader)val13).Close();
			((DbDataReader)val13).Dispose();
			val13 = null;
			((Component)val12).Dispose();
			val12 = null;
		}
		raporYapilacakTahsilatlarSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val5.Count > 0)
		{
			SqliteCommand val14 = new SqliteCommand("SELECT 0,IFNULL(som_kod,''),IFNULL(som_isim,'') FROM SORUMLULUK_MERKEZLERI WHERE som_kod in ('" + string.Join("','", (IEnumerable<string?>)val5) + "')", connection);
			SqliteDataReader val15 = val14.ExecuteReader();
			int num5 = 1;
			while (((DbDataReader)val15).Read())
			{
				raporYapilacakTahsilatlarSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(num5, ((DbDataReader)val15).GetInt32(0), ((DbDataReader)val15).GetString(1), ((DbDataReader)val15).GetString(2)));
				num5++;
			}
			((DbDataReader)val15).Close();
			((DbDataReader)val15).Dispose();
			val15 = null;
			((Component)val14).Dispose();
			val14 = null;
		}
		raporYapilacakTahsilatlarSonucHam.projeler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val6.Count > 0)
		{
			SqliteCommand val16 = new SqliteCommand("SELECT 0,IFNULL(pro_kodu,''),IFNULL(pro_adi,'') FROM PROJELER WHERE pro_kodu in ('" + string.Join("','", (IEnumerable<string?>)val6) + "')", connection);
			SqliteDataReader val17 = val16.ExecuteReader();
			int num6 = 1;
			while (((DbDataReader)val17).Read())
			{
				raporYapilacakTahsilatlarSonucHam.projeler.Add(new RaporYardimciGenelObje(num6, ((DbDataReader)val17).GetInt32(0), ((DbDataReader)val17).GetString(1), ((DbDataReader)val17).GetString(2)));
				num6++;
			}
			((DbDataReader)val17).Close();
			((DbDataReader)val17).Dispose();
			val17 = null;
			((Component)val16).Dispose();
			val16 = null;
		}
		raporYapilacakTahsilatlarSonucHam.firmalar.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val7.Count > 0)
		{
			SqliteCommand val18 = new SqliteCommand("SELECT 0,fir_sirano,IFNULL(fir_unvan,'') FROM FIRMALAR WHERE fir_sirano in (" + string.Join(",", (IEnumerable<int>)val7) + ")", connection);
			SqliteDataReader val19 = val18.ExecuteReader();
			int num7 = 1;
			while (((DbDataReader)val19).Read())
			{
				raporYapilacakTahsilatlarSonucHam.firmalar.Add(new RaporYardimciGenelObje(num7, ((DbDataReader)val19).GetInt32(0), ((DbDataReader)val19).GetInt32(1).ToString(), ((DbDataReader)val19).GetString(2)));
				num7++;
			}
			((DbDataReader)val19).Close();
			((DbDataReader)val19).Dispose();
			val19 = null;
			((Component)val18).Dispose();
			val18 = null;
		}
		raporYapilacakTahsilatlarSonucHam.subeler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val8.Count > 0)
		{
			SqliteCommand val20 = new SqliteCommand("SELECT 0,Sube_no,IFNULL(Sube_adi,'') FROM SUBELER WHERE Sube_no in (" + string.Join(",", (IEnumerable<int>)val8) + ")", connection);
			SqliteDataReader val21 = val20.ExecuteReader();
			int num8 = 1;
			while (((DbDataReader)val21).Read())
			{
				raporYapilacakTahsilatlarSonucHam.subeler.Add(new RaporYardimciGenelObje(num8, ((DbDataReader)val21).GetInt32(0), ((DbDataReader)val21).GetInt32(1).ToString(), ((DbDataReader)val21).GetString(2)));
				num8++;
			}
			((DbDataReader)val21).Close();
			((DbDataReader)val21).Dispose();
			val21 = null;
			((Component)val20).Dispose();
			val20 = null;
		}
		int num9 = 0;
		Enumerator<int> enumerator5 = val9.GetEnumerator();
		try
		{
			while (enumerator5.MoveNext())
			{
				int current5 = enumerator5.Current;
				raporYapilacakTahsilatlarSonucHam.doviz_cinsleri.Add(new RaporYardimciGenelObje(num9, current5, dovizcinsitanimlari.GetDovizCinsiTanimi(current5).Kur_sembol, dovizcinsitanimlari.GetDovizCinsiTanimi(current5).Kur_adi));
				num9++;
			}
		}
		finally
		{
			((IDisposable)enumerator5/*cast due to .constrained prefix*/).Dispose();
		}
		HashSet<string> val22 = new HashSet<string>();
		HashSet<string> val23 = new HashSet<string>();
		foreach (RaporYardimciCariObje item5 in raporYapilacakTahsilatlarSonucHam.cariler)
		{
			val22.Add(item5.cari_bolge_kod);
			val23.Add(item5.cari_grup_kod);
		}
		raporYapilacakTahsilatlarSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val22.Count > 0)
		{
			SqliteCommand val24 = new SqliteCommand("SELECT 0,IFNULL(bol_kod,''),IFNULL(bol_ismi,'') FROM CARI_HESAP_BOLGELERI WHERE bol_kod in ('" + string.Join("','", (IEnumerable<string?>)val22) + "')", connection);
			SqliteDataReader val25 = val24.ExecuteReader();
			int num10 = 1;
			while (((DbDataReader)val25).Read())
			{
				raporYapilacakTahsilatlarSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(num10, ((DbDataReader)val25).GetInt32(0), ((DbDataReader)val25).GetString(1), ((DbDataReader)val25).GetString(2)));
				num10++;
			}
			((DbDataReader)val25).Close();
			((DbDataReader)val25).Dispose();
			val25 = null;
			((Component)val24).Dispose();
			val24 = null;
		}
		raporYapilacakTahsilatlarSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val23.Count > 0)
		{
			SqliteCommand val26 = new SqliteCommand("SELECT 0,IFNULL(crg_kod,''),IFNULL(crg_isim,'') FROM CARI_HESAP_GRUPLARI WHERE crg_kod in ('" + string.Join("','", (IEnumerable<string?>)val23) + "')", connection);
			SqliteDataReader val27 = val26.ExecuteReader();
			int num11 = 1;
			while (((DbDataReader)val27).Read())
			{
				raporYapilacakTahsilatlarSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(num11, ((DbDataReader)val27).GetInt32(0), ((DbDataReader)val27).GetString(1), ((DbDataReader)val27).GetString(2)));
				num11++;
			}
			((DbDataReader)val27).Close();
			((DbDataReader)val27).Dispose();
			val27 = null;
			((Component)val26).Dispose();
			val26 = null;
		}
		foreach (RaporYapilacakTahsilatlarSatir item6 in raporYapilacakTahsilatlarSonucHam.sonuc_ham)
		{
			foreach (RaporYardimciCariObje item7 in raporYapilacakTahsilatlarSonucHam.cariler)
			{
				if (item7.Kodu == item6.cari_kod)
				{
					item6.cari_sira_no = item7.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item8 in raporYapilacakTahsilatlarSonucHam.temsilciler)
			{
				if (item8.Kodu == item6.plasiyer_kod)
				{
					item6.plasiyer_sira_no = item8.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item9 in raporYapilacakTahsilatlarSonucHam.sorumluluk_merkezleri)
			{
				if (item9.Kodu == item6.sorumluluk_merkezi_kod)
				{
					item6.sorumluluk_merkezi_sira_no = item9.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item10 in raporYapilacakTahsilatlarSonucHam.projeler)
			{
				if (item10.Kodu == item6.proje_kod)
				{
					item6.proje_sira_no = item10.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item11 in raporYapilacakTahsilatlarSonucHam.firmalar)
			{
				if (item11.Kodu == item6.firma_sira_no.ToString())
				{
					item6.firma_sira_no = item11.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item12 in raporYapilacakTahsilatlarSonucHam.subeler)
			{
				if (item12.Kodu == item6.sube_sira_no.ToString())
				{
					item6.sube_sira_no = item12.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item13 in raporYapilacakTahsilatlarSonucHam.doviz_cinsleri)
			{
				if (item13.Kodu == item6.doviz_cinsi.ToString())
				{
					item6.doviz_cinsi = item13.Position;
					break;
				}
			}
		}
		foreach (RaporYardimciCariObje item14 in raporYapilacakTahsilatlarSonucHam.cariler)
		{
			foreach (RaporYardimciGenelObje item15 in raporYapilacakTahsilatlarSonucHam.cari_bolgeleri)
			{
				if (item15.Kodu == item14.cari_bolge_kod)
				{
					item14.cari_bolge_sira_no = item15.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item16 in raporYapilacakTahsilatlarSonucHam.cari_gruplari)
			{
				if (item16.Kodu == item14.cari_grup_kod)
				{
					item14.cari_grup_sira_no = item16.Position;
					break;
				}
			}
		}
		return raporYapilacakTahsilatlarSonucHam;
	}
}
