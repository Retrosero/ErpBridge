using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Rapor.Genel;
using Fora.Mikro.Rapor.StokSatis;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RaporStokSatisData
{
	public static List<GenelList> GetAlabilecegiRaporlarList(SqlConnection connection, string AlabilecegiRaporlar)
	{
		string text = "'" + AlabilecegiRaporlar.Replace(",", "','") + "'";
		string cmdText = "SELECT ParametreUser AS kod,ParametreDegeri AS isim FROM _FORA_PARAMETRELER WHERE ParametreProgram='MobilRaporStokSatis' AND ParametreAdi='RaporAdi' AND ParametreUser in (" + text + ")";
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

	public static RaporStokSatisSonucHam GetRapor(SqlConnection connection, RaporStokSatisSecenekleri rapor_secenekleri, string DepoNo, string ProjeKodu, string SorumlulukMerkeziKodu, string CariPersonelKodu, string StokKodu, string StokAnaGrupKodu, string StokUreticiKodu, string StokMarkaKodu, string StokReyonKodu, string StokKategoriKodu, string CariKodu, string CariBolgeKodu, string CariGrupKodu, int RaporStokSatisMaliyetHesaplamaSekli)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(connection.Database);
		RaporStokSatisSonucHam raporStokSatisSonucHam = new RaporStokSatisSonucHam();
		raporStokSatisSonucHam.sonuc_ham = new List<RaporStokSatisSatir>();
		string text = "";
		text = "SELECT sh.sth_tarih AS tarih, sh.sth_normal_iade AS normal_iade, stok.sto_kod AS sto_kod, cari.cari_kod AS cari_kod, personel.cari_per_kod AS cari_per_kod, sh.sth_miktar AS miktar, (sh.sth_tutar*sh.sth_har_doviz_kuru) AS brut_tutar, ((sh.sth_iskonto1+sh.sth_iskonto2+sh.sth_iskonto3+sh.sth_iskonto4+sh.sth_iskonto5+sh.sth_iskonto6)*sh.sth_har_doviz_kuru) AS toplam_iskonto, (sh.sth_vergi*sh.sth_har_doviz_kuru) AS toplam_vergi, depolar.dep_adi AS dep_adi, sm.som_kod AS som_kod, p.pro_kodu AS pro_kodu, sh.sth_adres_no AS sevk_adres_no, stok.sto_birim2_katsayi AS birim2_katsayi, stok.sto_birim3_katsayi AS birim3_katsayi,";
		text = RaporStokSatisMaliyetHesaplamaSekli switch
		{
			1 => text + " (SELECT TOP 1 (sas.sas_brut_fiyat-sas.sas_isk_miktar1-sas.sas_isk_miktar2-sas.sas_isk_miktar3-sas.sas_isk_miktar4-sas.sas_isk_miktar5-sas.sas_isk_miktar6) FROM SATINALMA_SARTLARI AS sas WHERE sas.sas_stok_kod=sh.sth_stok_kod AND (sas.sas_basla_tarih<=sh.sth_tarih OR sas.sas_basla_tarih<'1910-01-01') AND (sas.sas_bitis_tarih>=sh.sth_tarih OR sas.sas_bitis_tarih<'1910-01-01') ORDER BY sas.sas_basla_tarih DESC, sas.sas_evrak_tarih DESC)*sh.sth_miktar AS net_toplam_maliyet", 
			2 => text + " (SELECT TOP 1 (((fh.sth_tutar-fh.sth_iskonto1-fh.sth_iskonto2-fh.sth_iskonto3-fh.sth_iskonto4-fh.sth_iskonto5-fh.sth_iskonto6)/fh.sth_miktar)*fh.sth_har_doviz_kuru) FROM STOK_HAREKETLERI AS fh WHERE fh.sth_stok_kod=sh.sth_stok_kod AND fh.sth_tarih<=sh.sth_tarih AND fh.sth_tip=0 AND fh.sth_normal_iade=0 AND ((fh.sth_tutar-fh.sth_iskonto1-fh.sth_iskonto2-fh.sth_iskonto3-fh.sth_iskonto4-fh.sth_iskonto5-fh.sth_iskonto6)/sth_miktar)>0 ORDER BY fh.sth_tarih DESC)*sh.sth_miktar AS net_toplam_maliyet", 
			3 => text + " (stok.sto_standartmaliyet*sh.sth_miktar) AS net_toplam_maliyet", 
			_ => text + " ((sh.sth_tutar-sh.sth_iskonto1+sh.sth_iskonto2+sh.sth_iskonto3+sh.sth_iskonto4+sh.sth_iskonto5+sh.sth_iskonto6)*sh.sth_har_doviz_kuru) AS net_toplam_maliyet", 
		} + " FROM STOK_HAREKETLERI AS sh WITH (NOLOCK) LEFT JOIN STOKLAR AS stok WITH (NOLOCK) ON sh.sth_stok_kod=stok.sto_kod LEFT JOIN CARI_HESAPLAR AS cari WITH (NOLOCK) ON sh.sth_cari_kodu=cari.cari_kod LEFT JOIN CARI_PERSONEL_TANIMLARI AS personel WITH (NOLOCK) ON sh.sth_plasiyer_kodu=personel.cari_per_kod LEFT JOIN DEPOLAR AS depolar WITH (NOLOCK) ON sh.sth_cikis_depo_no=depolar.dep_no LEFT JOIN SORUMLULUK_MERKEZLERI AS sm WITH (NOLOCK) ON sh.sth_cari_srm_merkezi=sm.som_kod LEFT JOIN PROJELER AS p WITH (NOLOCK) ON sh.sth_proje_kodu=p.pro_kodu";
		if (rapor_secenekleri.fatura_durumu_secenegi == enum_fatura_durumu_secenekleri.Faturalasmislar)
		{
			text = ((mikroVersiyon <= 15) ? (text + " LEFT JOIN CARI_HESAP_HAREKETLERI AS cari_hareket ON sh.sth_fat_recid_recno=cari_hareket.cha_RECid_RECno") : (text + " LEFT JOIN CARI_HESAP_HAREKETLERI AS cari_hareket ON sh.sth_fat_uid=cari_hareket.cha_Guid"));
		}
		text += " WHERE";
		text += " sh.sth_tarih BETWEEN @baslangic_tarihi AND @bitis_tarihi";
		if (rapor_secenekleri.fatura_durumu_secenegi == enum_fatura_durumu_secenekleri.Faturalasmislar)
		{
			text += " AND cari_hareket.cha_tarihi BETWEEN @baslangic_tarihi AND @bitis_tarihi";
		}
		text += " AND sh.sth_cins in (0,1,2,12) ";
		text = ((!rapor_secenekleri.iadeler_dusulsun_mu) ? (text + " AND (sh.sth_evraktip in (1,4) AND sh.sth_normal_iade=0) ") : (text + " AND ( (sh.sth_evraktip in (1,4) AND sh.sth_normal_iade=0) OR (sh.sth_evraktip in (3,13) AND sh.sth_normal_iade=1) ) "));
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
			text = text + " AND sh.sth_cikis_depo_no in (" + text2 + ")";
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
			text = text + " AND sh.sth_proje_kodu in ('" + text2.Replace(",", "','") + "')";
		}
		if (rapor_secenekleri.fatura_durumu_secenegi == enum_fatura_durumu_secenekleri.Irsaliyeler)
		{
			text = ((mikroVersiyon <= 15) ? (text + " AND sh.sth_fat_recid_recno=0") : (text + " AND sh.sth_fat_uid=@sth_fat_uid"));
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
			text = text + " AND sh.sth_cari_srm_merkezi in ('" + text2.Replace(",", "','") + "')";
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
			text = text + " AND sh.sth_plasiyer_kodu in ('" + text2.Replace(",", "','") + "')";
		}
		switch (rapor_secenekleri.stok_arama_secenekleri)
		{
		case enum_stok_arama_secenekleri.Esittir:
			text = text + " AND sh.sth_stok_kod='" + rapor_secenekleri.stok_arama_metin + "'";
			break;
		case enum_stok_arama_secenekleri.Icerir:
			text = text + " AND sh.sth_stok_kod like ('%" + rapor_secenekleri.stok_arama_metin + "%')";
			break;
		case enum_stok_arama_secenekleri.IleBaslar:
			text = text + " AND sh.sth_stok_kod like ('" + rapor_secenekleri.stok_arama_metin + "%')";
			break;
		case enum_stok_arama_secenekleri.TanimliOlan:
			if (StokKodu != "")
			{
				text = text + " AND sh.sth_stok_kod in ('" + StokKodu.Replace(",", "','") + "')";
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
			SqlCommand sqlCommand = new SqlCommand(text, connection);
			sqlCommand.Parameters.AddWithValue("@baslangic_tarihi", dateTime);
			sqlCommand.Parameters.AddWithValue("@bitis_tarihi", dateTime2);
			if (mikroVersiyon > 15)
			{
				sqlCommand.Parameters.AddWithValue("@sth_fat_uid", Guid.Empty);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				raporStokSatisSonucHam.sonuc_ham.Add(new RaporStokSatisSatir(sqlDataReader.GetSafeDateTime(0), sqlDataReader.GetSafeByte(1), sqlDataReader.GetSafeString(2), sqlDataReader.GetSafeString(3), sqlDataReader.GetSafeString(4), sqlDataReader.GetSafeDouble(5), sqlDataReader.GetSafeDouble(6), sqlDataReader.GetSafeDouble(7), sqlDataReader.GetSafeDouble(8), sqlDataReader.GetSafeString(9), sqlDataReader.GetSafeString(10), sqlDataReader.GetSafeString(11), sqlDataReader.GetSafeInt32(12), sqlDataReader.GetSafeDouble(13), sqlDataReader.GetSafeDouble(14), sqlDataReader.GetSafeDouble(15)));
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
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> hashSet2 = new HashSet<string>();
		HashSet<string> hashSet3 = new HashSet<string>();
		HashSet<string> hashSet4 = new HashSet<string>();
		HashSet<string> hashSet5 = new HashSet<string>();
		HashSet<string> hashSet6 = new HashSet<string>();
		foreach (RaporStokSatisSatir item in raporStokSatisSonucHam.sonuc_ham)
		{
			hashSet.Add(item.stok_kod);
			hashSet2.Add(item.cari_kod);
			hashSet3.Add(item.plasiyer_kod);
			hashSet4.Add(item.depo_kod);
			hashSet5.Add(item.sorumluluk_merkezi_kod);
			hashSet6.Add(item.proje_kod);
		}
		raporStokSatisSonucHam.stoklar.Add(new RaporYardimciStokObje(0, 0, "TANIMSIZ", "TANIMSIZ", "", "", 0, 0, 0, 0, 0));
		if (hashSet.Count > 0)
		{
			SqlCommand sqlCommand2 = new SqlCommand("SELECT 0,sto_kod,sto_isim,sto_birim1_ad,sto_birim2_ad,sto_birim3_ad,anagrup.san_kod AS anagrup_kod,uretici.urt_kod AS stok_uretici_kod,marka.mrk_kod AS stok_marka_kod,reyon.ryn_kod AS stok_reyon_kod,kategori.ktg_kod AS stok_kategori_kod FROM STOKLAR WITH (NOLOCK) LEFT JOIN STOK_ANA_GRUPLARI AS anagrup WITH (NOLOCK) ON STOKLAR.sto_anagrup_kod=anagrup.san_kod LEFT JOIN STOK_URETICILERI AS uretici WITH (NOLOCK) ON STOKLAR.sto_uretici_kodu=uretici.urt_kod LEFT JOIN STOK_MARKALARI AS marka WITH (NOLOCK) ON STOKLAR.sto_marka_kodu=marka.mrk_kod LEFT JOIN STOK_REYONLARI AS reyon WITH (NOLOCK) ON STOKLAR.sto_reyon_kodu=reyon.ryn_kod LEFT JOIN STOK_KATEGORILERI AS kategori WITH (NOLOCK) ON STOKLAR.sto_kategori_kodu=kategori.ktg_kod WHERE sto_kod in ('" + string.Join("','", hashSet) + "')", connection);
			SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
			int num = 1;
			while (sqlDataReader2.Read())
			{
				raporStokSatisSonucHam.stoklar.Add(new RaporYardimciStokObje(num, sqlDataReader2.GetSafeInt32(0), sqlDataReader2.GetSafeString(1), sqlDataReader2.GetSafeString(2), sqlDataReader2.GetSafeString(3), sqlDataReader2.GetSafeString(4), sqlDataReader2.GetSafeString(5), sqlDataReader2.GetSafeString(6), sqlDataReader2.GetSafeString(7), sqlDataReader2.GetSafeString(8), sqlDataReader2.GetSafeString(9), sqlDataReader2.GetSafeString(10)));
				num++;
			}
			sqlDataReader2.Close();
			sqlDataReader2.Dispose();
			sqlDataReader2 = null;
			sqlCommand2.Dispose();
			sqlCommand2 = null;
		}
		raporStokSatisSonucHam.cariler.Add(new RaporYardimciCariObje(0, 0, "TANIMSIZ", "TANIMSIZ", 0, 0));
		if (hashSet2.Count > 0)
		{
			SqlCommand sqlCommand3 = new SqlCommand("SELECT 0,cari_kod,cari_unvan1,cari_unvan2,bolge.bol_kod AS cari_bolge_kod,grup.crg_kod AS cari_grup_kod FROM CARI_HESAPLAR  WITH (NOLOCK) LEFT JOIN CARI_HESAP_BOLGELERI AS bolge WITH (NOLOCK) ON CARI_HESAPLAR.cari_bolge_kodu=bolge.bol_kod LEFT JOIN CARI_HESAP_GRUPLARI AS grup WITH (NOLOCK) ON CARI_HESAPLAR.cari_grup_kodu=grup.crg_kod WHERE cari_kod in ('" + string.Join("','", hashSet2) + "')", connection);
			SqlDataReader sqlDataReader3 = sqlCommand3.ExecuteReader();
			int num2 = 1;
			while (sqlDataReader3.Read())
			{
				raporStokSatisSonucHam.cariler.Add(new RaporYardimciCariObje(num2, sqlDataReader3.GetSafeInt32(0), sqlDataReader3.GetSafeString(1), sqlDataReader3.GetSafeString(2) + " " + sqlDataReader3.GetSafeString(3), sqlDataReader3.GetSafeString(4), sqlDataReader3.GetSafeString(5)));
				num2++;
			}
			sqlDataReader3.Close();
			sqlDataReader3.Dispose();
			sqlDataReader3 = null;
			sqlCommand3.Dispose();
			sqlCommand3 = null;
		}
		raporStokSatisSonucHam.temsilciler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet3.Count > 0)
		{
			SqlCommand sqlCommand4 = new SqlCommand("SELECT 0,ISNULL(cari_per_kod,''),ISNULL(cari_per_adi,''),ISNULL(cari_per_soyadi,'') FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) WHERE cari_per_kod in ('" + string.Join("','", hashSet3) + "')", connection);
			SqlDataReader sqlDataReader4 = sqlCommand4.ExecuteReader();
			int num3 = 1;
			while (sqlDataReader4.Read())
			{
				raporStokSatisSonucHam.temsilciler.Add(new RaporYardimciGenelObje(num3, sqlDataReader4.GetInt32(0), sqlDataReader4.GetSafeString(1), sqlDataReader4.GetSafeString(2) + " " + sqlDataReader4.GetSafeString(3)));
				num3++;
			}
			sqlDataReader4.Close();
			sqlDataReader4.Dispose();
			sqlDataReader4 = null;
			sqlCommand4.Dispose();
			sqlCommand4 = null;
		}
		raporStokSatisSonucHam.depolar.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet4.Count > 0)
		{
			SqlCommand sqlCommand5 = new SqlCommand("SELECT 0,ISNULL(dep_no,0),ISNULL(dep_adi,'') FROM DEPOLAR WITH (NOLOCK) WHERE dep_adi in ('" + string.Join("','", hashSet4) + "')", connection);
			SqlDataReader sqlDataReader5 = sqlCommand5.ExecuteReader();
			int num4 = 1;
			while (sqlDataReader5.Read())
			{
				raporStokSatisSonucHam.depolar.Add(new RaporYardimciGenelObje(num4, sqlDataReader5.GetInt32(0), sqlDataReader5.GetInt32(1).ToString(), sqlDataReader5.GetSafeString(2)));
				num4++;
			}
			sqlDataReader5.Close();
			sqlDataReader5.Dispose();
			sqlDataReader5 = null;
			sqlCommand5.Dispose();
			sqlCommand5 = null;
		}
		raporStokSatisSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet5.Count > 0)
		{
			SqlCommand sqlCommand6 = new SqlCommand("SELECT 0,ISNULL(som_kod,''),ISNULL(som_isim,'') FROM SORUMLULUK_MERKEZLERI WITH (NOLOCK) WHERE som_kod in ('" + string.Join("','", hashSet5) + "')", connection);
			SqlDataReader sqlDataReader6 = sqlCommand6.ExecuteReader();
			int num5 = 1;
			while (sqlDataReader6.Read())
			{
				raporStokSatisSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(num5, sqlDataReader6.GetInt32(0), sqlDataReader6.GetSafeString(1), sqlDataReader6.GetSafeString(2)));
				num5++;
			}
			sqlDataReader6.Close();
			sqlDataReader6.Dispose();
			sqlDataReader6 = null;
			sqlCommand6.Dispose();
			sqlCommand6 = null;
		}
		raporStokSatisSonucHam.projeler.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet6.Count > 0)
		{
			SqlCommand sqlCommand7 = new SqlCommand("SELECT 0,ISNULL(pro_kodu,''),ISNULL(pro_adi,'') FROM PROJELER WITH (NOLOCK) WHERE pro_kodu in ('" + string.Join("','", hashSet6) + "')", connection);
			SqlDataReader sqlDataReader7 = sqlCommand7.ExecuteReader();
			int num6 = 1;
			while (sqlDataReader7.Read())
			{
				raporStokSatisSonucHam.projeler.Add(new RaporYardimciGenelObje(num6, sqlDataReader7.GetInt32(0), sqlDataReader7.GetSafeString(1), sqlDataReader7.GetSafeString(2)));
				num6++;
			}
			sqlDataReader7.Close();
			sqlDataReader7.Dispose();
			sqlDataReader7 = null;
			sqlCommand7.Dispose();
			sqlCommand7 = null;
		}
		HashSet<string> hashSet7 = new HashSet<string>();
		HashSet<string> hashSet8 = new HashSet<string>();
		HashSet<string> hashSet9 = new HashSet<string>();
		HashSet<string> hashSet10 = new HashSet<string>();
		HashSet<string> hashSet11 = new HashSet<string>();
		HashSet<string> hashSet12 = new HashSet<string>();
		HashSet<string> hashSet13 = new HashSet<string>();
		foreach (RaporYardimciStokObje item2 in raporStokSatisSonucHam.stoklar)
		{
			hashSet7.Add(item2.stok_anagrup_kod);
			hashSet8.Add(item2.stok_uretici_kod);
			hashSet9.Add(item2.stok_marka_kod);
			hashSet10.Add(item2.stok_reyon_kod);
			hashSet11.Add(item2.stok_kategori_kod);
		}
		foreach (RaporYardimciCariObje item3 in raporStokSatisSonucHam.cariler)
		{
			hashSet12.Add(item3.cari_bolge_kod);
			hashSet13.Add(item3.cari_grup_kod);
		}
		raporStokSatisSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet7.Count > 0)
		{
			SqlCommand sqlCommand8 = new SqlCommand("SELECT 0,ISNULL(san_kod,''),ISNULL(san_isim,'') FROM STOK_ANA_GRUPLARI WITH (NOLOCK) WHERE san_kod in ('" + string.Join("','", hashSet7) + "')", connection);
			SqlDataReader sqlDataReader8 = sqlCommand8.ExecuteReader();
			int num7 = 1;
			while (sqlDataReader8.Read())
			{
				raporStokSatisSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(num7, sqlDataReader8.GetInt32(0), sqlDataReader8.GetSafeString(1), sqlDataReader8.GetSafeString(2)));
				num7++;
			}
			sqlDataReader8.Close();
			sqlDataReader8.Dispose();
			sqlDataReader8 = null;
			sqlCommand8.Dispose();
			sqlCommand8 = null;
		}
		raporStokSatisSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet8.Count > 0)
		{
			SqlCommand sqlCommand9 = new SqlCommand("SELECT 0,ISNULL(urt_kod,''),ISNULL(urt_ismi,'') FROM STOK_URETICILERI WITH (NOLOCK) WHERE urt_kod in ('" + string.Join("','", hashSet8) + "')", connection);
			SqlDataReader sqlDataReader9 = sqlCommand9.ExecuteReader();
			int num8 = 1;
			while (sqlDataReader9.Read())
			{
				raporStokSatisSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(num8, sqlDataReader9.GetInt32(0), sqlDataReader9.GetSafeString(1), sqlDataReader9.GetSafeString(2)));
				num8++;
			}
			sqlDataReader9.Close();
			sqlDataReader9.Dispose();
			sqlDataReader9 = null;
			sqlCommand9.Dispose();
			sqlCommand9 = null;
		}
		raporStokSatisSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet9.Count > 0)
		{
			SqlCommand sqlCommand10 = new SqlCommand("SELECT 0,ISNULL(mrk_kod,''),ISNULL(mrk_ismi,'') FROM STOK_MARKALARI WITH (NOLOCK) WHERE mrk_kod in ('" + string.Join("','", hashSet9) + "')", connection);
			SqlDataReader sqlDataReader10 = sqlCommand10.ExecuteReader();
			int num9 = 1;
			while (sqlDataReader10.Read())
			{
				raporStokSatisSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(num9, sqlDataReader10.GetInt32(0), sqlDataReader10.GetSafeString(1), sqlDataReader10.GetSafeString(2)));
				num9++;
			}
			sqlDataReader10.Close();
			sqlDataReader10.Dispose();
			sqlDataReader10 = null;
			sqlCommand10.Dispose();
			sqlCommand10 = null;
		}
		raporStokSatisSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet10.Count > 0)
		{
			SqlCommand sqlCommand11 = new SqlCommand("SELECT 0,ISNULL(ryn_kod,''),ISNULL(ryn_ismi,'') FROM STOK_REYONLARI WITH (NOLOCK) WHERE ryn_kod in ('" + string.Join("','", hashSet10) + "')", connection);
			SqlDataReader sqlDataReader11 = sqlCommand11.ExecuteReader();
			int num10 = 1;
			while (sqlDataReader11.Read())
			{
				raporStokSatisSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(num10, sqlDataReader11.GetInt32(0), sqlDataReader11.GetSafeString(1), sqlDataReader11.GetSafeString(2)));
				num10++;
			}
			sqlDataReader11.Close();
			sqlDataReader11.Dispose();
			sqlDataReader11 = null;
			sqlCommand11.Dispose();
			sqlCommand11 = null;
		}
		raporStokSatisSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet11.Count > 0)
		{
			SqlCommand sqlCommand12 = new SqlCommand("SELECT 0,ISNULL(ktg_kod,''),ISNULL(ktg_isim,'') FROM STOK_KATEGORILERI WITH (NOLOCK) WHERE ktg_kod in ('" + string.Join("','", hashSet11) + "')", connection);
			SqlDataReader sqlDataReader12 = sqlCommand12.ExecuteReader();
			int num11 = 1;
			while (sqlDataReader12.Read())
			{
				raporStokSatisSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(num11, sqlDataReader12.GetInt32(0), sqlDataReader12.GetSafeString(1), sqlDataReader12.GetSafeString(2)));
				num11++;
			}
			sqlDataReader12.Close();
			sqlDataReader12.Dispose();
			sqlDataReader12 = null;
			sqlCommand12.Dispose();
			sqlCommand12 = null;
		}
		raporStokSatisSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet12.Count > 0)
		{
			SqlCommand sqlCommand13 = new SqlCommand("SELECT 0,ISNULL(bol_kod,''),ISNULL(bol_ismi,'') FROM CARI_HESAP_BOLGELERI WITH (NOLOCK) WHERE bol_kod in ('" + string.Join("','", hashSet12) + "')", connection);
			SqlDataReader sqlDataReader13 = sqlCommand13.ExecuteReader();
			int num12 = 1;
			while (sqlDataReader13.Read())
			{
				raporStokSatisSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(num12, sqlDataReader13.GetInt32(0), sqlDataReader13.GetSafeString(1), sqlDataReader13.GetSafeString(2)));
				num12++;
			}
			sqlDataReader13.Close();
			sqlDataReader13.Dispose();
			sqlDataReader13 = null;
			sqlCommand13.Dispose();
			sqlCommand13 = null;
		}
		raporStokSatisSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(0, 0, "TANIMSIZ", "TANIMSIZ"));
		if (hashSet13.Count > 0)
		{
			SqlCommand sqlCommand14 = new SqlCommand("SELECT 0,ISNULL(crg_kod,''),ISNULL(crg_isim,'') FROM CARI_HESAP_GRUPLARI WITH (NOLOCK) WHERE crg_kod in ('" + string.Join("','", hashSet13) + "')", connection);
			SqlDataReader sqlDataReader14 = sqlCommand14.ExecuteReader();
			int num13 = 1;
			while (sqlDataReader14.Read())
			{
				raporStokSatisSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(num13, sqlDataReader14.GetInt32(0), sqlDataReader14.GetSafeString(1), sqlDataReader14.GetSafeString(2)));
				num13++;
			}
			sqlDataReader14.Close();
			sqlDataReader14.Dispose();
			sqlDataReader14 = null;
			sqlCommand14.Dispose();
			sqlCommand14 = null;
		}
		foreach (RaporStokSatisSatir item4 in raporStokSatisSonucHam.sonuc_ham)
		{
			foreach (RaporYardimciStokObje item5 in raporStokSatisSonucHam.stoklar)
			{
				if (item5.Kodu == item4.stok_kod)
				{
					item4.stok_sira_no = item5.Position;
					break;
				}
			}
			foreach (RaporYardimciCariObje item6 in raporStokSatisSonucHam.cariler)
			{
				if (item6.Kodu == item4.cari_kod)
				{
					item4.cari_sira_no = item6.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item7 in raporStokSatisSonucHam.temsilciler)
			{
				if (item7.Kodu == item4.plasiyer_kod)
				{
					item4.plasiyer_sira_no = item7.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item8 in raporStokSatisSonucHam.depolar)
			{
				if (item8.Adi == item4.depo_kod)
				{
					item4.depo_sira_no = item8.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item9 in raporStokSatisSonucHam.sorumluluk_merkezleri)
			{
				if (item9.Kodu == item4.sorumluluk_merkezi_kod)
				{
					item4.sorumluluk_merkezi_sira_no = item9.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item10 in raporStokSatisSonucHam.projeler)
			{
				if (item10.Kodu == item4.proje_kod)
				{
					item4.proje_sira_no = item10.Position;
					break;
				}
			}
		}
		foreach (RaporYardimciStokObje item11 in raporStokSatisSonucHam.stoklar)
		{
			foreach (RaporYardimciGenelObje item12 in raporStokSatisSonucHam.stok_ana_gruplari)
			{
				if (item12.Kodu == item11.stok_anagrup_kod)
				{
					item11.stok_anagrup_sira_no = item12.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item13 in raporStokSatisSonucHam.stok_ureticileri)
			{
				if (item13.Kodu == item11.stok_uretici_kod)
				{
					item11.stok_uretici_sira_no = item13.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item14 in raporStokSatisSonucHam.stok_markalari)
			{
				if (item14.Kodu == item11.stok_marka_kod)
				{
					item11.stok_marka_sira_no = item14.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item15 in raporStokSatisSonucHam.stok_reyonlari)
			{
				if (item15.Kodu == item11.stok_reyon_kod)
				{
					item11.stok_reyon_sira_no = item15.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item16 in raporStokSatisSonucHam.stok_kategorileri)
			{
				if (item16.Kodu == item11.stok_kategori_kod)
				{
					item11.stok_kategori_sira_no = item16.Position;
					break;
				}
			}
		}
		foreach (RaporYardimciCariObje item17 in raporStokSatisSonucHam.cariler)
		{
			foreach (RaporYardimciGenelObje item18 in raporStokSatisSonucHam.cari_bolgeleri)
			{
				if (item18.Kodu == item17.cari_bolge_kod)
				{
					item17.cari_bolge_sira_no = item18.Position;
					break;
				}
			}
			foreach (RaporYardimciGenelObje item19 in raporStokSatisSonucHam.cari_gruplari)
			{
				if (item19.Kodu == item17.cari_grup_kod)
				{
					item17.cari_grup_sira_no = item19.Position;
					break;
				}
			}
		}
		return raporStokSatisSonucHam;
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
