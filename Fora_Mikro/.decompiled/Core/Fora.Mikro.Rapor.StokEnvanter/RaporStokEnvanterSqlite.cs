using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Rapor.StokEnvanter;

public static class RaporStokEnvanterSqlite
{
	public static List<GenelList> GetAlabilecegiRaporlarList(SqliteConnection connection, string AlabilecegiRaporlar)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		string text = "'" + AlabilecegiRaporlar.Replace(",", "','") + "'";
		string commandText = "SELECT ParametreUser AS kod,ParametreDegeri AS isim FROM _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporStokEnvanter' AND ParametreAdi='RaporAdi' AND ParametreUser in (" + text + ")";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
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

	public static RaporStokEnvanterSonucHam GetRapor(SqliteConnection connection, RaporStokEnvanterSecenekleri rapor_secenekleri, string DepoNo, string StokKodu, string StokAnaGrupKodu, string StokUreticiKodu, string StokMarkaKodu, string StokReyonKodu, string StokKategoriKodu)
	{
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Expected O, but got Unknown
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Expected O, but got Unknown
		//IL_063e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Expected O, but got Unknown
		//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b5: Expected O, but got Unknown
		//IL_086a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0871: Expected O, but got Unknown
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Expected O, but got Unknown
		//IL_0926: Unknown result type (might be due to invalid IL or missing references)
		//IL_092d: Expected O, but got Unknown
		//IL_09e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e9: Expected O, but got Unknown
		//IL_0a9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa5: Expected O, but got Unknown
		//IL_0e2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e3b: Unknown result type (might be due to invalid IL or missing references)
		GenelUtility.GetMikroVersiyon(((DbConnection)connection).DataSource);
		RaporStokEnvanterSonucHam raporStokEnvanterSonucHam = new RaporStokEnvanterSonucHam();
		raporStokEnvanterSonucHam.sonuc_ham = new List<RaporStokEnvanterSatir>();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<double> list3 = new List<double>();
		List<double> list4 = new List<double>();
		string text = "";
		text = "SELECT stok.sto_kod AS sto_kod, stok.sto_birim2_katsayi AS birim2_katsayi, stok.sto_birim3_katsayi AS birim3_katsayi FROM STOKLAR AS stok";
		text += " WHERE 1=1";
		string text2 = "";
		switch (rapor_secenekleri.stok_arama_secenekleri)
		{
		case enum_stok_arama_secenekleri.Esittir:
			text = text + " AND stok.sto_kod='" + rapor_secenekleri.stok_arama_metin + "'";
			break;
		case enum_stok_arama_secenekleri.Icerir:
			text = text + " AND stok.sto_kod like ('%" + rapor_secenekleri.stok_arama_metin + "%')";
			break;
		case enum_stok_arama_secenekleri.IleBaslar:
			text = text + " AND stok.sto_kod like ('" + rapor_secenekleri.stok_arama_metin + "%')";
			break;
		case enum_stok_arama_secenekleri.TanimliOlan:
			if (StokKodu != "")
			{
				text = text + " AND stok.sto_kod in ('" + StokKodu.Replace(",", "','") + "')";
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
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = connection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeString(0));
				list3.Add(val2.GetSafeDouble(1));
				list4.Add(val2.GetSafeDouble(2));
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
		text = "SELECT dep_adi FROM DEPOLAR AS depolar";
		text += " WHERE 1=1";
		text2 = "";
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
			text = text + " AND depolar.dep_no in (" + text2 + ")";
		}
		try
		{
			SqliteCommand val3 = new SqliteCommand();
			((DbCommand)val3).CommandText = text;
			val3.Connection = connection;
			SqliteDataReader val4 = val3.ExecuteReader();
			while (((DbDataReader)val4).Read())
			{
				list2.Add(val4.GetSafeString(0));
			}
			((DbDataReader)val4).Close();
			((DbDataReader)val4).Dispose();
			val4 = null;
			((Component)val3).Dispose();
			val3 = null;
		}
		catch (Exception ex2)
		{
			Console.WriteLine(ex2.ToString());
		}
		int num = 0;
		foreach (string item in list)
		{
			foreach (string item2 in list2)
			{
				raporStokEnvanterSonucHam.sonuc_ham.Add(new RaporStokEnvanterSatir(item, 0.0, 0.0, item2, list3[num], list4[num]));
			}
			num++;
		}
		HashSet<string> val5 = new HashSet<string>();
		HashSet<string> val6 = new HashSet<string>();
		foreach (RaporStokEnvanterSatir item3 in raporStokEnvanterSonucHam.sonuc_ham)
		{
			val5.Add(item3.stok_kod);
			val6.Add(item3.depo_kod);
		}
		raporStokEnvanterSonucHam.stoklar.Add(new RaporYardimciStokObje(0, 0, "TANIMSIZ", "TANIMSIZ", "", "", 0, 0, 0, 0, 0));
		if (val5.Count > 0)
		{
			SqliteCommand val7 = new SqliteCommand();
			val7.Connection = connection;
			((DbCommand)val7).CommandText = "SELECT 0,sto_kod,sto_isim,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,anagrup.san_kod AS anagrup_kod,uretici.urt_kod AS stok_uretici_kod,marka.mrk_kod AS stok_marka_kod,reyon.ryn_kod AS stok_reyon_kod,kategori.ktg_kod AS stok_kategori_kod,sto_doviz_cinsi FROM STOKLAR  LEFT JOIN STOK_ANA_GRUPLARI AS anagrup ON STOKLAR.sto_anagrup_kod=anagrup.san_kod LEFT JOIN STOK_URETICILERI AS uretici ON STOKLAR.sto_uretici_kodu=uretici.urt_kod LEFT JOIN STOK_MARKALARI AS marka ON STOKLAR.sto_marka_kodu=marka.mrk_kod LEFT JOIN STOK_REYONLARI AS reyon ON STOKLAR.sto_reyon_kodu=reyon.ryn_kod LEFT JOIN STOK_KATEGORILERI AS kategori ON STOKLAR.sto_kategori_kodu=kategori.ktg_kod WHERE sto_kod in ('" + string.Join("','", (IEnumerable<string?>)val5) + "')";
			SqliteDataReader val8 = val7.ExecuteReader();
			int num2 = 1;
			while (((DbDataReader)val8).Read())
			{
				raporStokEnvanterSonucHam.stoklar.Add(new RaporYardimciStokObje(num2, val8.GetSafeInt32(0), val8.GetSafeString(1), val8.GetSafeString(2), val8.GetSafeString(3), val8.GetSafeString(4), val8.GetSafeString(5), val8.GetSafeString(6), val8.GetSafeString(7), val8.GetSafeString(8), val8.GetSafeString(9), val8.GetSafeString(10), val8.GetSafeInt32(11)));
				num2++;
			}
			((DbDataReader)val8).Close();
			((DbDataReader)val8).Dispose();
			val8 = null;
			((Component)val7).Dispose();
			val7 = null;
		}
		raporStokEnvanterSonucHam.depolar.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val6.Count > 0)
		{
			SqliteCommand val9 = new SqliteCommand();
			val9.Connection = connection;
			((DbCommand)val9).CommandText = "SELECT 0,IFNULL(dep_no,0),IFNULL(dep_adi,'') FROM DEPOLAR WHERE dep_adi in ('" + string.Join("','", (IEnumerable<string?>)val6) + "')";
			SqliteDataReader val10 = val9.ExecuteReader();
			int num3 = 1;
			while (((DbDataReader)val10).Read())
			{
				raporStokEnvanterSonucHam.depolar.Add(new RaporYardimciGenelObje(num3, ((DbDataReader)val10).GetInt32(0), ((DbDataReader)val10).GetInt32(1).ToString(), val10.GetSafeString(2)));
				num3++;
			}
			((DbDataReader)val10).Close();
			((DbDataReader)val10).Dispose();
			val10 = null;
			((Component)val9).Dispose();
			val9 = null;
		}
		HashSet<string> val11 = new HashSet<string>();
		HashSet<string> val12 = new HashSet<string>();
		HashSet<string> val13 = new HashSet<string>();
		HashSet<string> val14 = new HashSet<string>();
		HashSet<string> val15 = new HashSet<string>();
		new HashSet<string>();
		new HashSet<string>();
		foreach (RaporYardimciStokObje item4 in raporStokEnvanterSonucHam.stoklar)
		{
			val11.Add(item4.stok_anagrup_kod);
			val12.Add(item4.stok_uretici_kod);
			val13.Add(item4.stok_marka_kod);
			val14.Add(item4.stok_reyon_kod);
			val15.Add(item4.stok_kategori_kod);
		}
		raporStokEnvanterSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val11.Count > 0)
		{
			SqliteCommand val16 = new SqliteCommand();
			val16.Connection = connection;
			((DbCommand)val16).CommandText = "SELECT 0,IFNULL(san_kod,''),IFNULL(san_isim,'') FROM STOK_ANA_GRUPLARI WHERE san_kod in ('" + string.Join("','", (IEnumerable<string?>)val11) + "')";
			SqliteDataReader val17 = val16.ExecuteReader();
			int num4 = 1;
			while (((DbDataReader)val17).Read())
			{
				raporStokEnvanterSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(num4, ((DbDataReader)val17).GetInt32(0), val17.GetSafeString(1), val17.GetSafeString(2)));
				num4++;
			}
			((DbDataReader)val17).Close();
			((DbDataReader)val17).Dispose();
			val17 = null;
			((Component)val16).Dispose();
			val16 = null;
		}
		raporStokEnvanterSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val12.Count > 0)
		{
			SqliteCommand val18 = new SqliteCommand();
			val18.Connection = connection;
			((DbCommand)val18).CommandText = "SELECT 0,IFNULL(urt_kod,''),IFNULL(urt_ismi,'') FROM STOK_URETICILERI WHERE urt_kod in ('" + string.Join("','", (IEnumerable<string?>)val12) + "')";
			SqliteDataReader val19 = val18.ExecuteReader();
			int num5 = 1;
			while (((DbDataReader)val19).Read())
			{
				raporStokEnvanterSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(num5, ((DbDataReader)val19).GetInt32(0), val19.GetSafeString(1), val19.GetSafeString(2)));
				num5++;
			}
			((DbDataReader)val19).Close();
			((DbDataReader)val19).Dispose();
			val19 = null;
			((Component)val18).Dispose();
			val18 = null;
		}
		raporStokEnvanterSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val13.Count > 0)
		{
			SqliteCommand val20 = new SqliteCommand();
			val20.Connection = connection;
			((DbCommand)val20).CommandText = "SELECT 0,IFNULL(mrk_kod,''),IFNULL(mrk_ismi,'') FROM STOK_MARKALARI WHERE mrk_kod in ('" + string.Join("','", (IEnumerable<string?>)val13) + "')";
			SqliteDataReader val21 = val20.ExecuteReader();
			int num6 = 1;
			while (((DbDataReader)val21).Read())
			{
				raporStokEnvanterSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(num6, ((DbDataReader)val21).GetInt32(0), val21.GetSafeString(1), val21.GetSafeString(2)));
				num6++;
			}
			((DbDataReader)val21).Close();
			((DbDataReader)val21).Dispose();
			val21 = null;
			((Component)val20).Dispose();
			val20 = null;
		}
		raporStokEnvanterSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val14.Count > 0)
		{
			SqliteCommand val22 = new SqliteCommand();
			val22.Connection = connection;
			((DbCommand)val22).CommandText = "SELECT 0,IFNULL(ryn_kod,''),IFNULL(ryn_ismi,'') FROM STOK_REYONLARI WHERE ryn_kod in ('" + string.Join("','", (IEnumerable<string?>)val14) + "')";
			SqliteDataReader val23 = val22.ExecuteReader();
			int num7 = 1;
			while (((DbDataReader)val23).Read())
			{
				raporStokEnvanterSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(num7, ((DbDataReader)val23).GetInt32(0), val23.GetSafeString(1), val23.GetSafeString(2)));
				num7++;
			}
			((DbDataReader)val23).Close();
			((DbDataReader)val23).Dispose();
			val23 = null;
			((Component)val22).Dispose();
			val22 = null;
		}
		raporStokEnvanterSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (val15.Count > 0)
		{
			SqliteCommand val24 = new SqliteCommand();
			val24.Connection = connection;
			((DbCommand)val24).CommandText = "SELECT 0,IFNULL(ktg_kod,''),IFNULL(ktg_isim,'') FROM STOK_KATEGORILERI WHERE ktg_kod in ('" + string.Join("','", (IEnumerable<string?>)val15) + "')";
			SqliteDataReader val25 = val24.ExecuteReader();
			int num8 = 1;
			while (((DbDataReader)val25).Read())
			{
				raporStokEnvanterSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(num8, ((DbDataReader)val25).GetInt32(0), val25.GetSafeString(1), val25.GetSafeString(2)));
				num8++;
			}
			((DbDataReader)val25).Close();
			((DbDataReader)val25).Dispose();
			val25 = null;
			((Component)val24).Dispose();
			val24 = null;
		}
		foreach (RaporStokEnvanterSatir item5 in raporStokEnvanterSonucHam.sonuc_ham)
		{
			foreach (RaporYardimciStokObje item6 in raporStokEnvanterSonucHam.stoklar)
			{
				if (item6.Kodu == item5.stok_kod)
				{
					item5.stok_sira_no = item6.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item7 in raporStokEnvanterSonucHam.depolar)
			{
				if (item7.Adi == item5.depo_kod)
				{
					item5.depo_sira_no = item7.Position;
					break;
				}
			}
		}
		foreach (RaporYardimciStokObje item8 in raporStokEnvanterSonucHam.stoklar)
		{
			foreach (RaporYardimciGenelObje item9 in raporStokEnvanterSonucHam.stok_ana_gruplari)
			{
				if (item9.Kodu == item8.stok_anagrup_kod)
				{
					item8.stok_anagrup_sira_no = item9.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item10 in raporStokEnvanterSonucHam.stok_ureticileri)
			{
				if (item10.Kodu == item8.stok_uretici_kod)
				{
					item8.stok_uretici_sira_no = item10.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item11 in raporStokEnvanterSonucHam.stok_markalari)
			{
				if (item11.Kodu == item8.stok_marka_kod)
				{
					item8.stok_marka_sira_no = item11.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item12 in raporStokEnvanterSonucHam.stok_reyonlari)
			{
				if (item12.Kodu == item8.stok_reyon_kod)
				{
					item8.stok_reyon_sira_no = item12.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item13 in raporStokEnvanterSonucHam.stok_kategorileri)
			{
				if (item13.Kodu == item8.stok_kategori_kod)
				{
					item8.stok_kategori_sira_no = item13.Position;
					break;
				}
			}
		}
		int result = 1;
		int.TryParse(rapor_secenekleri.fiyat_liste_no, out result);
		FiyatListesi fiyatListesi = FiyatListesiSqlite.GetFiyatListesi(connection, result);
		foreach (RaporStokEnvanterSatir item14 in raporStokEnvanterSonucHam.sonuc_ham)
		{
			SqliteDataReader val26 = new SqliteCommand
			{
				Connection = connection,
				CommandText = "SELECT dep_no FROM DEPOLAR WHERE dep_adi in ('" + item14.depo_kod + "')"
			}.ExecuteReader();
			int num9 = 9999999;
			while (((DbDataReader)val26).Read())
			{
				num9 = ((DbDataReader)val26).GetInt32(0);
			}
			item14.miktar = StokSqlite.GetDepoMiktar(connection, item14.stok_kod, num9);
			FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
			DateTime tarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			switch (rapor_secenekleri.degerleme_sekli)
			{
			case enum_stok_envanter_degerleme_sekli.FiyatListesi:
				fiyatTanimlamasi = StokSqlite.GetFiyatFromFiyatListesi(connection, item14.stok_kod, 0, fiyatListesi.sfl_sirano, fiyatListesi.sfl_kdvdahil, "", "", num9, 0, AppBase._vergitanimlari);
				break;
			case enum_stok_envanter_degerleme_sekli.SonAlisFiyati:
				fiyatTanimlamasi = StokSqlite.GetFiyatFromSonFiyati(connection, item14.stok_kod, enum_SatisAlis.Alis);
				tarih = StokSqlite.GetFiyatFromSonFiyatiTarihi(connection, item14.stok_kod, enum_SatisAlis.Alis);
				break;
			case enum_stok_envanter_degerleme_sekli.SonSatisFiyati:
				fiyatTanimlamasi = StokSqlite.GetFiyatFromSonFiyati(connection, item14.stok_kod, enum_SatisAlis.Satis);
				tarih = StokSqlite.GetFiyatFromSonFiyatiTarihi(connection, item14.stok_kod, enum_SatisAlis.Satis);
				break;
			case enum_stok_envanter_degerleme_sekli.StandartMaliyet:
				fiyatTanimlamasi = StokSqlite.GetFiyatFromStokStandartMaliyeti(connection, item14.stok_kod);
				fiyatTanimlamasi.DovizCinsi = raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi;
				break;
			}
			if (raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi != fiyatTanimlamasi.DovizCinsi)
			{
				Kur kur = new Kur();
				if (raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi != 0)
				{
					kur = KurSqlite.GetKur(connection, raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi, AppBase.KullaniciParametreleri._GetParametre("DovizFiyatNo")._GetString, tarih);
				}
				Kur kur2 = new Kur();
				if (fiyatTanimlamasi.DovizCinsi != 0)
				{
					kur2 = KurSqlite.GetKur(connection, fiyatTanimlamasi.DovizCinsi, AppBase.KullaniciParametreleri._GetParametre("DovizFiyatNo")._GetString, tarih);
				}
				fiyatTanimlamasi.FiyatBrut = kur2.dov_fiyat * fiyatTanimlamasi.FiyatBrut / kur.dov_fiyat;
			}
			item14.tutar = fiyatTanimlamasi.FiyatNetMasrafli * item14.miktar;
			if (item14.birim2_katsayi == 0.0)
			{
				item14.miktar_birim2 = item14.miktar;
			}
			else if (item14.birim2_katsayi > 0.0)
			{
				item14.miktar_birim2 = item14.miktar * Math.Abs(item14.birim2_katsayi);
			}
			else
			{
				item14.miktar_birim2 = item14.miktar / Math.Abs(item14.birim2_katsayi);
			}
			if (item14.birim3_katsayi == 0.0)
			{
				item14.miktar_birim3 = item14.miktar;
			}
			else if (item14.birim3_katsayi > 0.0)
			{
				item14.miktar_birim3 = item14.miktar * Math.Abs(item14.birim3_katsayi);
			}
			else
			{
				item14.miktar_birim3 = item14.miktar / Math.Abs(item14.birim3_katsayi);
			}
		}
		if (rapor_secenekleri.stokta_olan_urunler)
		{
			for (int num10 = raporStokEnvanterSonucHam.sonuc_ham.Count - 1; num10 >= 0; num10--)
			{
				if (raporStokEnvanterSonucHam.sonuc_ham[num10].miktar <= 0.0)
				{
					raporStokEnvanterSonucHam.sonuc_ham.RemoveAt(num10);
				}
			}
		}
		return raporStokEnvanterSonucHam;
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
