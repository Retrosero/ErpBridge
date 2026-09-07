using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Rapor.StokEnvanter;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RaporStokEnvanterData
{
	public static List<GenelList> GetAlabilecegiRaporlarList(SqlConnection connection, string AlabilecegiRaporlar)
	{
		string text = "'" + AlabilecegiRaporlar.Replace(",", "','") + "'";
		string cmdText = "SELECT ParametreUser AS kod,ParametreDegeri AS isim FROM _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporStokEnvanter' AND ParametreAdi='RaporAdi' AND ParametreUser in (" + text + ")";
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

	public static RaporStokEnvanterSonucHam GetRapor(SqlConnection connection, SqlConnection connection_mikroanadb, RaporStokEnvanterSecenekleri rapor_secenekleri, string DepoNo, string StokKodu, string StokAnaGrupKodu, string StokUreticiKodu, string StokMarkaKodu, string StokReyonKodu, string StokKategoriKodu)
	{
		GenelUtility.GetMikroVersiyon(connection.Database);
		RaporStokEnvanterSonucHam raporStokEnvanterSonucHam = new RaporStokEnvanterSonucHam();
		raporStokEnvanterSonucHam.sonuc_ham = new List<RaporStokEnvanterSatir>();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<double> list3 = new List<double>();
		List<double> list4 = new List<double>();
		string text = "";
		text = "SELECT stok.sto_kod AS sto_kod, stok.sto_birim2_katsayi AS birim2_katsayi, stok.sto_birim3_katsayi AS birim3_katsayi FROM STOKLAR AS stok WITH (NOLOCK)";
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
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.CommandText = text;
			sqlCommand.Connection = connection;
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				list.Add(sqlDataReader.GetSafeString(0));
				list3.Add(sqlDataReader.GetSafeDouble(1));
				list4.Add(sqlDataReader.GetSafeDouble(2));
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		text = "SELECT dep_adi FROM DEPOLAR AS depolar WITH (NOLOCK)";
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
			SqlCommand sqlCommand2 = new SqlCommand();
			sqlCommand2.CommandText = text;
			sqlCommand2.Connection = connection;
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			while (sqlDataReader2.Read())
			{
				list2.Add(sqlDataReader2.GetSafeString(0));
			}
			sqlDataReader2.Close();
			sqlDataReader2.Dispose();
			sqlDataReader2 = null;
			sqlCommand2.Dispose();
			sqlCommand2 = null;
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
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> hashSet2 = new HashSet<string>();
		foreach (RaporStokEnvanterSatir item3 in raporStokEnvanterSonucHam.sonuc_ham)
		{
			hashSet.Add(item3.stok_kod);
			hashSet2.Add(item3.depo_kod);
		}
		raporStokEnvanterSonucHam.stoklar.Add(new RaporYardimciStokObje(0, 0, "TANIMSIZ", "TANIMSIZ", "", "", 0, 0, 0, 0, 0));
		if (hashSet.Count > 0)
		{
			SqlCommand sqlCommand3 = new SqlCommand();
			sqlCommand3.Connection = connection;
			sqlCommand3.CommandText = "SELECT 0,sto_kod,sto_isim,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,anagrup.san_kod AS anagrup_kod,uretici.urt_kod AS stok_uretici_kod,marka.mrk_kod AS stok_marka_kod,reyon.ryn_kod AS stok_reyon_kod,kategori.ktg_kod AS stok_kategori_kod,sto_doviz_cinsi FROM STOKLAR WITH (NOLOCK) LEFT JOIN STOK_ANA_GRUPLARI AS anagrup WITH (NOLOCK) ON STOKLAR.sto_anagrup_kod=anagrup.san_kod LEFT JOIN STOK_URETICILERI AS uretici WITH (NOLOCK) ON STOKLAR.sto_uretici_kodu=uretici.urt_kod LEFT JOIN STOK_MARKALARI AS marka WITH (NOLOCK) ON STOKLAR.sto_marka_kodu=marka.mrk_kod LEFT JOIN STOK_REYONLARI AS reyon WITH (NOLOCK) ON STOKLAR.sto_reyon_kodu=reyon.ryn_kod LEFT JOIN STOK_KATEGORILERI AS kategori WITH (NOLOCK) ON STOKLAR.sto_kategori_kodu=kategori.ktg_kod WHERE sto_kod in ('" + string.Join("','", hashSet) + "')";
			SqlDataReader sqlDataReader3 = sqlCommand3.ExecuteReader();
			int num2 = 1;
			while (sqlDataReader3.Read())
			{
				raporStokEnvanterSonucHam.stoklar.Add(new RaporYardimciStokObje(num2, sqlDataReader3.GetSafeInt32(0), sqlDataReader3.GetSafeString(1), sqlDataReader3.GetSafeString(2), sqlDataReader3.GetSafeString(3), sqlDataReader3.GetSafeString(4), sqlDataReader3.GetSafeString(5), sqlDataReader3.GetSafeString(6), sqlDataReader3.GetSafeString(7), sqlDataReader3.GetSafeString(8), sqlDataReader3.GetSafeString(9), sqlDataReader3.GetSafeString(10), sqlDataReader3.GetSafeByte(11)));
				num2++;
			}
			sqlDataReader3.Close();
			sqlDataReader3.Dispose();
			sqlDataReader3 = null;
			sqlCommand3.Dispose();
			sqlCommand3 = null;
		}
		raporStokEnvanterSonucHam.depolar.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet2.Count > 0)
		{
			SqlCommand sqlCommand4 = new SqlCommand();
			sqlCommand4.Connection = connection;
			sqlCommand4.CommandText = "SELECT 0,ISNULL(dep_no,0),ISNULL(dep_adi,'') FROM DEPOLAR WITH (NOLOCK) WHERE dep_adi in ('" + string.Join("','", hashSet2) + "')";
			SqlDataReader sqlDataReader4 = sqlCommand4.ExecuteReader();
			int num3 = 1;
			while (sqlDataReader4.Read())
			{
				raporStokEnvanterSonucHam.depolar.Add(new RaporYardimciGenelObje(num3, sqlDataReader4.GetInt32(0), sqlDataReader4.GetInt32(1).ToString(), sqlDataReader4.GetSafeString(2)));
				num3++;
			}
			sqlDataReader4.Close();
			sqlDataReader4.Dispose();
			sqlDataReader4 = null;
			sqlCommand4.Dispose();
			sqlCommand4 = null;
		}
		HashSet<string> hashSet3 = new HashSet<string>();
		HashSet<string> hashSet4 = new HashSet<string>();
		HashSet<string> hashSet5 = new HashSet<string>();
		HashSet<string> hashSet6 = new HashSet<string>();
		HashSet<string> hashSet7 = new HashSet<string>();
		foreach (RaporYardimciStokObje item4 in raporStokEnvanterSonucHam.stoklar)
		{
			hashSet3.Add(item4.stok_anagrup_kod);
			hashSet4.Add(item4.stok_uretici_kod);
			hashSet5.Add(item4.stok_marka_kod);
			hashSet6.Add(item4.stok_reyon_kod);
			hashSet7.Add(item4.stok_kategori_kod);
		}
		raporStokEnvanterSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet3.Count > 0)
		{
			SqlCommand sqlCommand5 = new SqlCommand();
			sqlCommand5.Connection = connection;
			sqlCommand5.CommandText = "SELECT 0,ISNULL(san_kod,''),ISNULL(san_isim,'') FROM STOK_ANA_GRUPLARI WITH (NOLOCK) WHERE san_kod in ('" + string.Join("','", hashSet3) + "')";
			SqlDataReader sqlDataReader5 = sqlCommand5.ExecuteReader();
			int num4 = 1;
			while (sqlDataReader5.Read())
			{
				raporStokEnvanterSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(num4, sqlDataReader5.GetInt32(0), sqlDataReader5.GetSafeString(1), sqlDataReader5.GetSafeString(2)));
				num4++;
			}
			sqlDataReader5.Close();
			sqlDataReader5.Dispose();
			sqlDataReader5 = null;
			sqlCommand5.Dispose();
			sqlCommand5 = null;
		}
		raporStokEnvanterSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet4.Count > 0)
		{
			SqlCommand sqlCommand6 = new SqlCommand();
			sqlCommand6.Connection = connection;
			sqlCommand6.CommandText = "SELECT 0,ISNULL(urt_kod,''),ISNULL(urt_ismi,'') FROM STOK_URETICILERI WITH (NOLOCK) WHERE urt_kod in ('" + string.Join("','", hashSet4) + "')";
			SqlDataReader sqlDataReader6 = sqlCommand6.ExecuteReader();
			int num5 = 1;
			while (sqlDataReader6.Read())
			{
				raporStokEnvanterSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(num5, sqlDataReader6.GetInt32(0), sqlDataReader6.GetSafeString(1), sqlDataReader6.GetSafeString(2)));
				num5++;
			}
			sqlDataReader6.Close();
			sqlDataReader6.Dispose();
			sqlDataReader6 = null;
			sqlCommand6.Dispose();
			sqlCommand6 = null;
		}
		raporStokEnvanterSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet5.Count > 0)
		{
			SqlCommand sqlCommand7 = new SqlCommand();
			sqlCommand7.Connection = connection;
			sqlCommand7.CommandText = "SELECT 0,ISNULL(mrk_kod,''),ISNULL(mrk_ismi,'') FROM STOK_MARKALARI WITH (NOLOCK) WHERE mrk_kod in ('" + string.Join("','", hashSet5) + "')";
			SqlDataReader sqlDataReader7 = sqlCommand7.ExecuteReader();
			int num6 = 1;
			while (sqlDataReader7.Read())
			{
				raporStokEnvanterSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(num6, sqlDataReader7.GetInt32(0), sqlDataReader7.GetSafeString(1), sqlDataReader7.GetSafeString(2)));
				num6++;
			}
			sqlDataReader7.Close();
			sqlDataReader7.Dispose();
			sqlDataReader7 = null;
			sqlCommand7.Dispose();
			sqlCommand7 = null;
		}
		raporStokEnvanterSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet6.Count > 0)
		{
			SqlCommand sqlCommand8 = new SqlCommand();
			sqlCommand8.Connection = connection;
			sqlCommand8.CommandText = "SELECT 0,ISNULL(ryn_kod,''),ISNULL(ryn_ismi,'') FROM STOK_REYONLARI WITH (NOLOCK) WHERE ryn_kod in ('" + string.Join("','", hashSet6) + "')";
			SqlDataReader sqlDataReader8 = sqlCommand8.ExecuteReader();
			int num7 = 1;
			while (sqlDataReader8.Read())
			{
				raporStokEnvanterSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(num7, sqlDataReader8.GetInt32(0), sqlDataReader8.GetSafeString(1), sqlDataReader8.GetSafeString(2)));
				num7++;
			}
			sqlDataReader8.Close();
			sqlDataReader8.Dispose();
			sqlDataReader8 = null;
			sqlCommand8.Dispose();
			sqlCommand8 = null;
		}
		raporStokEnvanterSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet7.Count > 0)
		{
			SqlCommand sqlCommand9 = new SqlCommand();
			sqlCommand9.Connection = connection;
			sqlCommand9.CommandText = "SELECT 0,ISNULL(ktg_kod,''),ISNULL(ktg_isim,'') FROM STOK_KATEGORILERI WITH (NOLOCK) WHERE ktg_kod in ('" + string.Join("','", hashSet7) + "')";
			SqlDataReader sqlDataReader9 = sqlCommand9.ExecuteReader();
			int num8 = 1;
			while (sqlDataReader9.Read())
			{
				raporStokEnvanterSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(num8, sqlDataReader9.GetInt32(0), sqlDataReader9.GetSafeString(1), sqlDataReader9.GetSafeString(2)));
				num8++;
			}
			sqlDataReader9.Close();
			sqlDataReader9.Dispose();
			sqlDataReader9 = null;
			sqlCommand9.Dispose();
			sqlCommand9 = null;
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
		FiyatListesi fiyatListesi = FiyatListesiData.GetFiyatListesi(connection, result);
		foreach (RaporStokEnvanterSatir item14 in raporStokEnvanterSonucHam.sonuc_ham)
		{
			SqlDataReader sqlDataReader10 = new SqlCommand
			{
				Connection = connection,
				CommandText = "SELECT dep_no FROM DEPOLAR WITH (NOLOCK) WHERE dep_adi in ('" + item14.depo_kod + "')"
			}.ExecuteReader();
			int num9 = 9999999;
			while (sqlDataReader10.Read())
			{
				num9 = sqlDataReader10.GetInt32(0);
			}
			item14.miktar = StokData.GetDepoMiktar(connection, connection.Database, item14.stok_kod, num9);
			FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
			DateTime tarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			switch (rapor_secenekleri.degerleme_sekli)
			{
			case enum_stok_envanter_degerleme_sekli.FiyatListesi:
				fiyatTanimlamasi = StokData.GetFiyatFromFiyatListesi(connection, item14.stok_kod, 0, fiyatListesi.sfl_sirano, fiyatListesi.sfl_kdvdahil, "", "", num9, 0, AppBase._vergitanimlari);
				break;
			case enum_stok_envanter_degerleme_sekli.SonAlisFiyati:
				fiyatTanimlamasi = StokData.GetFiyatFromSonFiyati(connection, item14.stok_kod, enum_SatisAlis.Alis);
				tarih = StokData.GetFiyatFromSonFiyatiTarihi(connection, item14.stok_kod, enum_SatisAlis.Alis);
				break;
			case enum_stok_envanter_degerleme_sekli.SonSatisFiyati:
				fiyatTanimlamasi = StokData.GetFiyatFromSonFiyati(connection, item14.stok_kod, enum_SatisAlis.Satis);
				tarih = StokData.GetFiyatFromSonFiyatiTarihi(connection, item14.stok_kod, enum_SatisAlis.Alis);
				break;
			case enum_stok_envanter_degerleme_sekli.StandartMaliyet:
				fiyatTanimlamasi = StokData.GetFiyatFromStokStandartMaliyeti(connection, item14.stok_kod);
				fiyatTanimlamasi.DovizCinsi = raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi;
				break;
			}
			if (raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi != fiyatTanimlamasi.DovizCinsi)
			{
				Kur kur = new Kur();
				if (raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi != 0)
				{
					kur = KurData.GetKur(connection_mikroanadb, raporStokEnvanterSonucHam.stoklar[item14.stok_sira_no].stok_doviz_cinsi, AppBase.KullaniciParametreleri._GetParametre("DovizFiyatNo")._GetString, tarih);
				}
				Kur kur2 = new Kur();
				if (fiyatTanimlamasi.DovizCinsi != 0)
				{
					kur2 = KurData.GetKur(connection_mikroanadb, fiyatTanimlamasi.DovizCinsi, AppBase.KullaniciParametreleri._GetParametre("DovizFiyatNo")._GetString, tarih);
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
