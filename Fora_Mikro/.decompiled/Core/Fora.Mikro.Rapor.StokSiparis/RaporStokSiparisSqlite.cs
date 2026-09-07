using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Rapor.StokSiparis;

public static class RaporStokSiparisSqlite
{
	public static List<GenelList> GetAlabilecegiRaporlarList(SqliteConnection connection, string AlabilecegiRaporlar)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		string text = "'" + AlabilecegiRaporlar.Replace(",", "','") + "'";
		string commandText = "SELECT ParametreUser AS kod,ParametreDegeri AS isim FROM _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporStokSiparis' AND ParametreAdi='RaporAdi' AND ParametreUser in (" + text + ")";
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

	public static RaporStokSiparisSonucHam GetRapor(SqliteConnection connection, RaporStokSiparisSecenekleri rapor_secenekleri, string DepoNo, string ProjeKodu, string SorumlulukMerkeziKodu, string CariPersonelKodu, string StokKodu, string StokAnaGrupKodu, string StokUreticiKodu, string StokMarkaKodu, string StokReyonKodu, string StokKategoriKodu, string CariKodu, string CariBolgeKodu, string CariGrupKodu)
	{
		//IL_0bcd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd4: Expected O, but got Unknown
		//IL_0ccd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd4: Expected O, but got Unknown
		//IL_0dab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0db2: Expected O, but got Unknown
		//IL_0e79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e80: Expected O, but got Unknown
		//IL_0f3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f45: Expected O, but got Unknown
		//IL_0ffa: Unknown result type (might be due to invalid IL or missing references)
		//IL_1001: Expected O, but got Unknown
		//IL_11b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_11b9: Expected O, but got Unknown
		//IL_126e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1275: Expected O, but got Unknown
		//IL_132a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1331: Expected O, but got Unknown
		//IL_13e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ed: Expected O, but got Unknown
		//IL_14a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a9: Expected O, but got Unknown
		//IL_155e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1565: Expected O, but got Unknown
		//IL_161a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1621: Expected O, but got Unknown
		//IL_09ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f4: Expected O, but got Unknown
		GenelUtility.GetMikroVersiyon(((DbConnection)connection).DataSource);
		RaporStokSiparisSonucHam raporStokSiparisSonucHam = new RaporStokSiparisSonucHam();
		raporStokSiparisSonucHam.sonuc_ham = new List<RaporStokSiparisSatir>();
		string text = "";
		text = "SELECT sip.sip_tarih AS tarih, stok.sto_kod AS sto_kod, cari.cari_kod AS cari_kod, personel.cari_per_kod AS cari_per_kod, depolar.dep_adi AS dep_adi, sm.som_kod AS som_kod, p.pro_kodu AS pro_kodu, sip.sip_miktar AS miktar, sip.sip_teslim_miktar AS teslim_miktar, (sip.sip_tutar*sip.sip_doviz_kuru) AS brut_tutar, ((sip.sip_iskonto_1+sip.sip_iskonto_2+sip.sip_iskonto_3+sip.sip_iskonto_4+sip.sip_iskonto_5+sip.sip_iskonto_6)*sip.sip_doviz_kuru) AS toplam_iskonto, (sip.sip_vergi*sip.sip_doviz_kuru) AS toplam_vergi, sip.sip_kapat_fl AS sip_kapat_fl FROM SIPARISLER AS sip LEFT JOIN STOKLAR AS stok ON sip.sip_stok_kod=stok.sto_kod LEFT JOIN CARI_HESAPLAR AS cari ON sip.sip_musteri_kod=cari.cari_kod LEFT JOIN CARI_PERSONEL_TANIMLARI AS personel ON sip.sip_satici_kod=personel.cari_per_kod LEFT JOIN DEPOLAR AS depolar ON sip.sip_depono=depolar.dep_no LEFT JOIN SORUMLULUK_MERKEZLERI AS sm ON sip.sip_cari_sormerk=sm.som_kod LEFT JOIN PROJELER AS p ON sip.sip_projekodu=p.pro_kodu WHERE";
		text += " sip.sip_tarih BETWEEN @baslangic_tarihi AND @bitis_tarihi";
		text += " AND ( sip.sip_tip=0 AND sip.sip_cins=0 ) ";
		switch (rapor_secenekleri.teslim_durumu)
		{
		case enum_siparis_teslim_durumu_secenekleri.Bekleyen:
			text += " AND ( sip.sip_miktar>sip.sip_teslim_miktar AND sip.sip_kapat_fl=0 ) ";
			break;
		case enum_siparis_teslim_durumu_secenekleri.Tamamlanan:
			text += " AND ( sip.sip_miktar=sip.sip_teslim_miktar OR sip.sip_kapat_fl=1) ";
			break;
		case enum_siparis_teslim_durumu_secenekleri.Vazgeçilen:
			text += " AND ( sip.sip_miktar<>sip.sip_teslim_miktar AND sip.sip_kapat_fl=1) ";
			break;
		}
		DateTime dateTime = DateTime.Now;
		DateTime dateTime2 = DateTime.Now;
		DateTime now = DateTime.Now;
		DateTime dateTime3 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
		DateTime dateTime4 = dateTime3.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime dateTime5 = new DateTime(dateTime3.Year, dateTime3.Month, 1);
		DateTime dateTime6 = dateTime5.AddMonths(1).AddDays(-1.0).AddHours(23.0)
			.AddMinutes(59.0)
			.AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime dateTime7 = new DateTime(dateTime3.Year, 1, 1);
		DateTime dateTime8 = dateTime7.AddYears(1).AddMilliseconds(-1.0);
		switch (rapor_secenekleri.tarih_cinsi)
		{
		case enum_tarih_cinsi.OzelTarih:
			dateTime = rapor_secenekleri.baslangic_tarihi;
			dateTime2 = rapor_secenekleri.bitis_tarihi;
			break;
		case enum_tarih_cinsi.TumZamanlar:
			dateTime = dateTime7.AddYears(-10);
			dateTime2 = dateTime7.AddYears(10);
			break;
		case enum_tarih_cinsi.Dun:
			dateTime = dateTime3.AddDays(-1.0);
			dateTime2 = dateTime4.AddDays(-1.0);
			break;
		case enum_tarih_cinsi.Bugun:
			dateTime = dateTime3;
			dateTime2 = now;
			break;
		case enum_tarih_cinsi.BuHafta:
			dateTime = StartOfWeek(dateTime3, DayOfWeek.Monday);
			dateTime2 = now;
			break;
		case enum_tarih_cinsi.BuAy:
			dateTime = dateTime5;
			dateTime2 = dateTime6;
			break;
		case enum_tarih_cinsi.BuYil:
			dateTime = dateTime7;
			dateTime2 = dateTime8;
			break;
		case enum_tarih_cinsi.GecenHafta:
			dateTime = StartOfWeek(dateTime3, DayOfWeek.Monday).AddDays(-7.0);
			dateTime2 = StartOfWeek(dateTime3, DayOfWeek.Monday).AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GecenAy:
			dateTime = dateTime5.AddMonths(-1);
			dateTime2 = dateTime5.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GecenYil:
			dateTime = dateTime7.AddYears(-1);
			dateTime2 = dateTime7.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Son3Ay:
			dateTime = dateTime5.AddMonths(-2);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son6Ay:
			dateTime = dateTime5.AddMonths(-5);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son12Ay:
			dateTime = dateTime5.AddMonths(-11);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son7Gun:
			dateTime = dateTime3.AddDays(-6.0);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son15Gun:
			dateTime = dateTime3.AddDays(-14.0);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son30Gun:
			dateTime = dateTime3.AddDays(-29.0);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son60Gun:
			dateTime = dateTime3.AddDays(-59.0);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son90Gun:
			dateTime = dateTime3.AddDays(-89.0);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son365Gun:
			dateTime = dateTime3.AddDays(-364.0);
			dateTime2 = dateTime4;
			break;
		case enum_tarih_cinsi.Son24Saat:
			dateTime = now.AddHours(-24.0);
			dateTime2 = now;
			break;
		}
		string text2 = "";
		text2 = "";
		switch (rapor_secenekleri.depolar_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.depolar;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = DepoNo;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND sip.sip_depono in (" + text2 + ")";
		}
		text2 = "";
		switch (rapor_secenekleri.proje_kodlari_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.proje_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = ProjeKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND sip.sip_projekodu in ('" + text2.Replace(",", "','") + "')";
		}
		text2 = "";
		switch (rapor_secenekleri.sorumluluk_merkezleri_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.sorumluluk_merkezleri;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = SorumlulukMerkeziKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND sip.sip_cari_sormerk in ('" + text2.Replace(",", "','") + "')";
		}
		text2 = "";
		switch (rapor_secenekleri.cari_temsilci_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.cari_temsilci_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = CariPersonelKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND sip.sip_satici_kod in ('" + text2.Replace(",", "','") + "')";
		}
		switch (rapor_secenekleri.stok_arama_secenekleri)
		{
		case enum_stok_arama_secenekleri.Esittir:
			text = text + " AND sip.sip_stok_kod='" + rapor_secenekleri.stok_arama_metin + "'";
			break;
		case enum_stok_arama_secenekleri.Icerir:
			text = text + " AND sip.sip_stok_kod like ('%" + rapor_secenekleri.stok_arama_metin + "%')";
			break;
		case enum_stok_arama_secenekleri.IleBaslar:
			text = text + " AND sip.sip_stok_kod like ('" + rapor_secenekleri.stok_arama_metin + "%')";
			break;
		case enum_stok_arama_secenekleri.TanimliOlan:
			if (StokKodu != "")
			{
				text = text + " AND sip.sip_stok_kod in ('" + StokKodu.Replace(",", "','") + "')";
			}
			break;
		}
		text2 = "";
		switch (rapor_secenekleri.stok_ana_gruplari_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.stok_ana_gruplari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = StokAnaGrupKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND stok.sto_anagrup_kod in ('" + text2.Replace(",", "','") + "')";
		}
		text2 = "";
		switch (rapor_secenekleri.stok_uretici_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.stok_uretici_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = StokUreticiKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND stok.sto_uretici_kodu in ('" + text2.Replace(",", "','") + "')";
		}
		text2 = "";
		switch (rapor_secenekleri.stok_marka_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.stok_marka_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = StokMarkaKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND stok.sto_marka_kodu in ('" + text2.Replace(",", "','") + "')";
		}
		text2 = "";
		switch (rapor_secenekleri.stok_reyon_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.stok_reyon_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = StokReyonKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND stok.sto_reyon_kodu in ('" + text2.Replace(",", "','") + "')";
		}
		text2 = "";
		switch (rapor_secenekleri.stok_kategori_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.stok_kategori_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = StokKategoriKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND stok.sto_kategori_kodu in ('" + text2.Replace(",", "','") + "')";
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
		text2 = "";
		switch (rapor_secenekleri.cari_bolge_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.cari_bolge_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = CariBolgeKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND cari.cari_bolge_kodu in ('" + text2.Replace(",", "','") + "')";
		}
		text2 = "";
		switch (rapor_secenekleri.cari_grup_kodu_secenek)
		{
		case enum_tumu_tanimli_olan_listeden_sec.ListedenSec:
			text2 = rapor_secenekleri.cari_grup_kodlari;
			break;
		case enum_tumu_tanimli_olan_listeden_sec.TanimliOlan:
			text2 = CariGrupKodu;
			break;
		}
		if (text2 != "")
		{
			text = text + " AND cari.cari_grup_kodu in ('" + text2.Replace(",", "','") + "')";
		}
		try
		{
			SqliteCommand val = new SqliteCommand(text, connection);
			val.Parameters.AddWithValue("@baslangic_tarihi", (object)dateTime);
			val.Parameters.AddWithValue("@bitis_tarihi", (object)dateTime2);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				raporStokSiparisSonucHam.sonuc_ham.Add(new RaporStokSiparisSatir(val2.GetSafeDateTime(0), val2.GetSafeString(1), val2.GetSafeString(2), val2.GetSafeString(3), val2.GetSafeString(4), val2.GetSafeString(5), val2.GetSafeString(6), val2.GetSafeDouble(7), val2.GetSafeDouble(8), val2.GetSafeDouble(9), val2.GetSafeDouble(10), val2.GetSafeDouble(11), val2.GetSafeBoolean(12)));
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
		HashSet<string> val3 = new HashSet<string>();
		HashSet<string> val4 = new HashSet<string>();
		HashSet<string> val5 = new HashSet<string>();
		HashSet<string> val6 = new HashSet<string>();
		HashSet<string> val7 = new HashSet<string>();
		HashSet<string> val8 = new HashSet<string>();
		foreach (RaporStokSiparisSatir item in raporStokSiparisSonucHam.sonuc_ham)
		{
			val3.Add(item.stok_kod);
			val4.Add(item.cari_kod);
			val5.Add(item.plasiyer_kod);
			val6.Add(item.depo_kod);
			val7.Add(item.sorumluluk_merkezi_kod);
			val8.Add(item.proje_kod);
		}
		raporStokSiparisSonucHam.stoklar.Add(new RaporYardimciStokObje(0, 0, "TANIMSIZ", "TANIMSIZ", "", "", 0, 0, 0, 0, 0));
		if (val3.Count > 0)
		{
			SqliteCommand val9 = new SqliteCommand();
			val9.Connection = connection;
			((DbCommand)val9).CommandText = "SELECT 0,sto_kod,sto_isim,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,anagrup.san_kod AS san_kod,uretici.urt_kod AS urt_kod,marka.mrk_kod AS mrk_kod,reyon.ryn_kod AS ryn_kod,kategori.ktg_kod AS ktg_kod FROM STOKLAR  LEFT JOIN STOK_ANA_GRUPLARI AS anagrup ON STOKLAR.sto_anagrup_kod=anagrup.san_kod LEFT JOIN STOK_URETICILERI AS uretici ON STOKLAR.sto_uretici_kodu=uretici.urt_kod LEFT JOIN STOK_MARKALARI AS marka ON STOKLAR.sto_marka_kodu=marka.mrk_kod LEFT JOIN STOK_REYONLARI AS reyon ON STOKLAR.sto_reyon_kodu=reyon.ryn_kod LEFT JOIN STOK_KATEGORILERI AS kategori ON STOKLAR.sto_kategori_kodu=kategori.ktg_kod WHERE sto_kod in ('" + string.Join("','", (IEnumerable<string?>)val3) + "')";
			SqliteDataReader val10 = val9.ExecuteReader();
			int num = 1;
			while (((DbDataReader)val10).Read())
			{
				raporStokSiparisSonucHam.stoklar.Add(new RaporYardimciStokObje(num, val10.GetSafeInt32(0), val10.GetSafeString(1), val10.GetSafeString(2), val10.GetSafeString(3), val10.GetSafeString(4), val10.GetSafeString(5), val10.GetSafeString(6), val10.GetSafeString(7), val10.GetSafeString(8), val10.GetSafeString(9), val10.GetSafeString(10)));
				num++;
			}
			((DbDataReader)val10).Close();
			((DbDataReader)val10).Dispose();
			val10 = null;
			((Component)val9).Dispose();
			val9 = null;
		}
		raporStokSiparisSonucHam.cariler.Add(new RaporYardimciCariObje(0, 0, "TANIMSIZ", "TANIMSIZ", 0, 0));
		if (val4.Count > 0)
		{
			SqliteCommand val11 = new SqliteCommand();
			val11.Connection = connection;
			((DbCommand)val11).CommandText = "SELECT 0,cari_kod,cari_unvan1,cari_unvan2,bolge.bol_kod AS bol_kod,grup.crg_kod AS crg_kod FROM CARI_HESAPLAR  LEFT JOIN CARI_HESAP_BOLGELERI AS bolge ON CARI_HESAPLAR.cari_bolge_kodu=bolge.bol_kod LEFT JOIN CARI_HESAP_GRUPLARI AS grup ON CARI_HESAPLAR.cari_grup_kodu=grup.crg_kod WHERE cari_kod in ('" + string.Join("','", (IEnumerable<string?>)val4) + "')";
			SqliteDataReader val12 = val11.ExecuteReader();
			int num2 = 1;
			while (((DbDataReader)val12).Read())
			{
				raporStokSiparisSonucHam.cariler.Add(new RaporYardimciCariObje(num2, val12.GetSafeInt32(0), val12.GetSafeString(1), val12.GetSafeString(2) + " " + val12.GetSafeString(3), val12.GetSafeString(4), val12.GetSafeString(5)));
				num2++;
			}
			((DbDataReader)val12).Close();
			((DbDataReader)val12).Dispose();
			val12 = null;
			((Component)val11).Dispose();
			val11 = null;
		}
		raporStokSiparisSonucHam.temsilciler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val5.Count > 0)
		{
			SqliteCommand val13 = new SqliteCommand();
			val13.Connection = connection;
			((DbCommand)val13).CommandText = "SELECT 0,IFNULL(cari_per_kod,''),IFNULL(cari_per_adi,''),IFNULL(cari_per_soyadi,'') FROM CARI_PERSONEL_TANIMLARI WHERE cari_per_kod in ('" + string.Join("','", (IEnumerable<string?>)val5) + "')";
			SqliteDataReader val14 = val13.ExecuteReader();
			int num3 = 1;
			while (((DbDataReader)val14).Read())
			{
				raporStokSiparisSonucHam.temsilciler.Add(new RaporYardimciGenelObje(num3, ((DbDataReader)val14).GetInt32(0), val14.GetSafeString(1), val14.GetSafeString(2) + " " + val14.GetSafeString(3)));
				num3++;
			}
			((DbDataReader)val14).Close();
			((DbDataReader)val14).Dispose();
			val14 = null;
			((Component)val13).Dispose();
			val13 = null;
		}
		raporStokSiparisSonucHam.depolar.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val6.Count > 0)
		{
			SqliteCommand val15 = new SqliteCommand();
			val15.Connection = connection;
			((DbCommand)val15).CommandText = "SELECT 0,IFNULL(dep_no,0),IFNULL(dep_adi,'') FROM DEPOLAR WHERE dep_adi in ('" + string.Join("','", (IEnumerable<string?>)val6) + "')";
			SqliteDataReader val16 = val15.ExecuteReader();
			int num4 = 1;
			while (((DbDataReader)val16).Read())
			{
				raporStokSiparisSonucHam.depolar.Add(new RaporYardimciGenelObje(num4, ((DbDataReader)val16).GetInt32(0), ((DbDataReader)val16).GetInt32(1).ToString(), val16.GetSafeString(2)));
				num4++;
			}
			((DbDataReader)val16).Close();
			((DbDataReader)val16).Dispose();
			val16 = null;
			((Component)val15).Dispose();
			val15 = null;
		}
		raporStokSiparisSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val7.Count > 0)
		{
			SqliteCommand val17 = new SqliteCommand();
			val17.Connection = connection;
			((DbCommand)val17).CommandText = "SELECT 0,IFNULL(som_kod,''),IFNULL(som_isim,'') FROM SORUMLULUK_MERKEZLERI WHERE som_kod in ('" + string.Join("','", (IEnumerable<string?>)val7) + "')";
			SqliteDataReader val18 = val17.ExecuteReader();
			int num5 = 1;
			while (((DbDataReader)val18).Read())
			{
				raporStokSiparisSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(num5, ((DbDataReader)val18).GetInt32(0), val18.GetSafeString(1), val18.GetSafeString(2)));
				num5++;
			}
			((DbDataReader)val18).Close();
			((DbDataReader)val18).Dispose();
			val18 = null;
			((Component)val17).Dispose();
			val17 = null;
		}
		raporStokSiparisSonucHam.projeler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val8.Count > 0)
		{
			SqliteCommand val19 = new SqliteCommand();
			val19.Connection = connection;
			((DbCommand)val19).CommandText = "SELECT 0,IFNULL(pro_kodu,''),IFNULL(pro_adi,'') FROM PROJELER WHERE pro_kodu in ('" + string.Join("','", (IEnumerable<string?>)val8) + "')";
			SqliteDataReader val20 = val19.ExecuteReader();
			int num6 = 1;
			while (((DbDataReader)val20).Read())
			{
				raporStokSiparisSonucHam.projeler.Add(new RaporYardimciGenelObje(num6, ((DbDataReader)val20).GetInt32(0), val20.GetSafeString(1), val20.GetSafeString(2)));
				num6++;
			}
			((DbDataReader)val20).Close();
			((DbDataReader)val20).Dispose();
			val20 = null;
			((Component)val19).Dispose();
			val19 = null;
		}
		HashSet<string> val21 = new HashSet<string>();
		HashSet<string> val22 = new HashSet<string>();
		HashSet<string> val23 = new HashSet<string>();
		HashSet<string> val24 = new HashSet<string>();
		HashSet<string> val25 = new HashSet<string>();
		HashSet<string> val26 = new HashSet<string>();
		HashSet<string> val27 = new HashSet<string>();
		foreach (RaporYardimciStokObje item2 in raporStokSiparisSonucHam.stoklar)
		{
			val21.Add(item2.stok_anagrup_kod);
			val22.Add(item2.stok_uretici_kod);
			val23.Add(item2.stok_marka_kod);
			val24.Add(item2.stok_reyon_kod);
			val25.Add(item2.stok_kategori_kod);
		}
		foreach (RaporYardimciCariObje item3 in raporStokSiparisSonucHam.cariler)
		{
			val26.Add(item3.cari_bolge_kod);
			val27.Add(item3.cari_grup_kod);
		}
		raporStokSiparisSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val21.Count > 0)
		{
			SqliteCommand val28 = new SqliteCommand();
			val28.Connection = connection;
			((DbCommand)val28).CommandText = "SELECT 0,IFNULL(san_kod,''),IFNULL(san_isim,'') FROM STOK_ANA_GRUPLARI WHERE san_kod in ('" + string.Join("','", (IEnumerable<string?>)val21) + "')";
			SqliteDataReader val29 = val28.ExecuteReader();
			int num7 = 1;
			while (((DbDataReader)val29).Read())
			{
				raporStokSiparisSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(num7, ((DbDataReader)val29).GetInt32(0), val29.GetSafeString(1), val29.GetSafeString(2)));
				num7++;
			}
			((DbDataReader)val29).Close();
			((DbDataReader)val29).Dispose();
			val29 = null;
			((Component)val28).Dispose();
			val28 = null;
		}
		raporStokSiparisSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val22.Count > 0)
		{
			SqliteCommand val30 = new SqliteCommand();
			val30.Connection = connection;
			((DbCommand)val30).CommandText = "SELECT 0,IFNULL(urt_kod,''),IFNULL(urt_ismi,'') FROM STOK_URETICILERI WHERE urt_kod in ('" + string.Join("','", (IEnumerable<string?>)val22) + "')";
			SqliteDataReader val31 = val30.ExecuteReader();
			int num8 = 1;
			while (((DbDataReader)val31).Read())
			{
				raporStokSiparisSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(num8, ((DbDataReader)val31).GetInt32(0), val31.GetSafeString(1), val31.GetSafeString(2)));
				num8++;
			}
			((DbDataReader)val31).Close();
			((DbDataReader)val31).Dispose();
			val31 = null;
			((Component)val30).Dispose();
			val30 = null;
		}
		raporStokSiparisSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val23.Count > 0)
		{
			SqliteCommand val32 = new SqliteCommand();
			val32.Connection = connection;
			((DbCommand)val32).CommandText = "SELECT 0,IFNULL(mrk_kod,''),IFNULL(mrk_ismi,'') FROM STOK_MARKALARI WHERE mrk_kod in ('" + string.Join("','", (IEnumerable<string?>)val23) + "')";
			SqliteDataReader val33 = val32.ExecuteReader();
			int num9 = 1;
			while (((DbDataReader)val33).Read())
			{
				raporStokSiparisSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(num9, ((DbDataReader)val33).GetInt32(0), val33.GetSafeString(1), val33.GetSafeString(2)));
				num9++;
			}
			((DbDataReader)val33).Close();
			((DbDataReader)val33).Dispose();
			val33 = null;
			((Component)val32).Dispose();
			val32 = null;
		}
		raporStokSiparisSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val24.Count > 0)
		{
			SqliteCommand val34 = new SqliteCommand();
			val34.Connection = connection;
			((DbCommand)val34).CommandText = "SELECT 0,IFNULL(ryn_kod,''),IFNULL(ryn_ismi,'') FROM STOK_REYONLARI WHERE ryn_kod in ('" + string.Join("','", (IEnumerable<string?>)val24) + "')";
			SqliteDataReader val35 = val34.ExecuteReader();
			int num10 = 1;
			while (((DbDataReader)val35).Read())
			{
				raporStokSiparisSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(num10, ((DbDataReader)val35).GetInt32(0), val35.GetSafeString(1), val35.GetSafeString(2)));
				num10++;
			}
			((DbDataReader)val35).Close();
			((DbDataReader)val35).Dispose();
			val35 = null;
			((Component)val34).Dispose();
			val34 = null;
		}
		raporStokSiparisSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val25.Count > 0)
		{
			SqliteCommand val36 = new SqliteCommand();
			val36.Connection = connection;
			((DbCommand)val36).CommandText = "SELECT 0,IFNULL(ktg_kod,''),IFNULL(ktg_isim,'') FROM STOK_KATEGORILERI WHERE ktg_kod in ('" + string.Join("','", (IEnumerable<string?>)val25) + "')";
			SqliteDataReader val37 = val36.ExecuteReader();
			int num11 = 1;
			while (((DbDataReader)val37).Read())
			{
				raporStokSiparisSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(num11, ((DbDataReader)val37).GetInt32(0), val37.GetSafeString(1), val37.GetSafeString(2)));
				num11++;
			}
			((DbDataReader)val37).Close();
			((DbDataReader)val37).Dispose();
			val37 = null;
			((Component)val36).Dispose();
			val36 = null;
		}
		raporStokSiparisSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val26.Count > 0)
		{
			SqliteCommand val38 = new SqliteCommand();
			val38.Connection = connection;
			((DbCommand)val38).CommandText = "SELECT 0,IFNULL(bol_kod,''),IFNULL(bol_ismi,'') FROM CARI_HESAP_BOLGELERI WHERE bol_kod in ('" + string.Join("','", (IEnumerable<string?>)val26) + "')";
			SqliteDataReader val39 = val38.ExecuteReader();
			int num12 = 1;
			while (((DbDataReader)val39).Read())
			{
				raporStokSiparisSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(num12, ((DbDataReader)val39).GetInt32(0), val39.GetSafeString(1), val39.GetSafeString(2)));
				num12++;
			}
			((DbDataReader)val39).Close();
			((DbDataReader)val39).Dispose();
			val39 = null;
			((Component)val38).Dispose();
			val38 = null;
		}
		raporStokSiparisSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val27.Count > 0)
		{
			SqliteCommand val40 = new SqliteCommand();
			val40.Connection = connection;
			((DbCommand)val40).CommandText = "SELECT 0,IFNULL(crg_kod,''),IFNULL(crg_isim,'') FROM CARI_HESAP_GRUPLARI WHERE crg_kod in ('" + string.Join("','", (IEnumerable<string?>)val27) + "')";
			SqliteDataReader val41 = val40.ExecuteReader();
			int num13 = 1;
			while (((DbDataReader)val41).Read())
			{
				raporStokSiparisSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(num13, ((DbDataReader)val41).GetInt32(0), val41.GetSafeString(1), val41.GetSafeString(2)));
				num13++;
			}
			((DbDataReader)val41).Close();
			((DbDataReader)val41).Dispose();
			val41 = null;
			((Component)val40).Dispose();
			val40 = null;
		}
		foreach (RaporStokSiparisSatir item4 in raporStokSiparisSonucHam.sonuc_ham)
		{
			foreach (RaporYardimciStokObje item5 in raporStokSiparisSonucHam.stoklar)
			{
				if (item5.Kodu == item4.stok_kod)
				{
					item4.stok_sira_no = item5.Position;
					break;
				}
			}
			foreach (RaporYardimciCariObje item6 in raporStokSiparisSonucHam.cariler)
			{
				if (item6.Kodu == item4.cari_kod)
				{
					item4.cari_sira_no = item6.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item7 in raporStokSiparisSonucHam.temsilciler)
			{
				if (item7.Kodu == item4.plasiyer_kod)
				{
					item4.plasiyer_sira_no = item7.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item8 in raporStokSiparisSonucHam.depolar)
			{
				if (item8.Adi == item4.depo_kod)
				{
					item4.depo_sira_no = item8.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item9 in raporStokSiparisSonucHam.sorumluluk_merkezleri)
			{
				if (item9.Kodu == item4.sorumluluk_merkezi_kod)
				{
					item4.sorumluluk_merkezi_sira_no = item9.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item10 in raporStokSiparisSonucHam.projeler)
			{
				if (item10.Kodu == item4.proje_kod)
				{
					item4.proje_sira_no = item10.Position;
					break;
				}
			}
		}
		foreach (RaporYardimciStokObje item11 in raporStokSiparisSonucHam.stoklar)
		{
			foreach (RaporYardimciGenelObje item12 in raporStokSiparisSonucHam.stok_ana_gruplari)
			{
				if (item12.Kodu == item11.stok_anagrup_kod)
				{
					item11.stok_anagrup_sira_no = item12.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item13 in raporStokSiparisSonucHam.stok_ureticileri)
			{
				if (item13.Kodu == item11.stok_uretici_kod)
				{
					item11.stok_uretici_sira_no = item13.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item14 in raporStokSiparisSonucHam.stok_markalari)
			{
				if (item14.Kodu == item11.stok_marka_kod)
				{
					item11.stok_marka_sira_no = item14.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item15 in raporStokSiparisSonucHam.stok_reyonlari)
			{
				if (item15.Kodu == item11.stok_reyon_kod)
				{
					item11.stok_reyon_sira_no = item15.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item16 in raporStokSiparisSonucHam.stok_kategorileri)
			{
				if (item16.Kodu == item11.stok_kategori_kod)
				{
					item11.stok_kategori_sira_no = item16.Position;
					break;
				}
			}
		}
		foreach (RaporYardimciCariObje item17 in raporStokSiparisSonucHam.cariler)
		{
			foreach (RaporYardimciGenelObje item18 in raporStokSiparisSonucHam.cari_bolgeleri)
			{
				if (item18.Kodu == item17.cari_bolge_kod)
				{
					item17.cari_bolge_sira_no = item18.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item19 in raporStokSiparisSonucHam.cari_gruplari)
			{
				if (item19.Kodu == item17.cari_grup_kod)
				{
					item17.cari_grup_sira_no = item19.Position;
					break;
				}
			}
		}
		return raporStokSiparisSonucHam;
	}

	private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
	{
		int num = dt.DayOfWeek - startOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		return dt.AddDays(-1 * num).Date;
	}
}
