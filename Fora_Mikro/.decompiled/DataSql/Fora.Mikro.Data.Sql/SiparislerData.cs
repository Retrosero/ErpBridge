using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Siparis;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SiparislerData
{
	public static void SiparisKarsilamaAraTabloOlustur(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_SIPARIS_KARSILAMA_ARA_TABLO]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[_SIPARIS_KARSILAMA_ARA_TABLO]([RECno] [int] IDENTITY(1,1) NOT NULL,[SiparisSeriNo] [nvarchar](50) NOT NULL,[SiparisSiraNo] [int] NOT NULL,[SiparisKarsilanmaTarihi] [datetime] NOT NULL,[MikroyaAktarildi] [bit] NOT NULL,[EvrakTipi] [nvarchar](50) NOT NULL,[MikroSeriNo] [nvarchar](50) NOT NULL,[MikroSiraNo] [int] NOT NULL,[MikroyaAktarilmaTarihi] [datetime] NOT NULL,[EvrakIcerik] [nvarchar](max) NOT NULL,CONSTRAINT [PK__SIPARIS_KARSILAMA_ARA_TABLO] PRIMARY KEY CLUSTERED ([RECno] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY]END";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.ExecuteNonQuery();
		}
		sqlDB.ConnectionClose();
	}

	public static DataTable GetSiparisler(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, enum_sip_tip SiparisTipi, enum_sip_cins SiparisCinsi, string SiparisSeri, string CariKodu, enum_sip_orderby Siralama)
	{
		DataTable dataTable = new DataTable();
		SqlCommand sqlCommand = new SqlCommand();
		string text = "SELECT sip_evrakno_seri,sip_evrakno_sira,sip_tarih,MIN(sip_teslim_tarih) AS sip_teslim_tarih,sip_tip,dbo.fn_TalepTemin(sip_tip) AS MetinSiparisTipi,sip_cins,dbo.fn_SiparisCins(sip_cins) AS MetinSiparisCins,sip_musteri_kod,dbo.fn_CarininIsminiBul(0,sip_musteri_kod) AS CariIsmi,SUM(sip_miktar) AS sip_miktar,SUM( dbo.fn_SiparisNetTutar ( sip_tutar, sip_iskonto_1, sip_iskonto_2, sip_iskonto_3, sip_iskonto_4, sip_iskonto_5, sip_iskonto_6,sip_masraf_1, sip_masraf_2, sip_masraf_3, sip_masraf_4, sip_vergi, sip_masvergi, sip_Otv_Vergi, sip_otvtutari, sip_vergisiz_fl,2,sip_doviz_kuru,sip_alt_doviz_kuru)) AS SiparisTutari,SUM(sip_teslim_miktar) AS sip_teslim_miktar,CASE WHEN SUM(cast(sip_kapat_fl AS smallint))>0 THEN 0 ELSE (SUM(sip_miktar) - SUM(sip_teslim_miktar)) END AS KalanMiktar,CASE WHEN (CASE WHEN SUM(cast(sip_kapat_fl AS smallint))>0 THEN 0 ELSE (SUM(sip_miktar) - SUM(sip_teslim_miktar)) END)=0 THEN 'Evet' ELSE 'Hayır' END AS Karsilandi,COUNT(sip_evrakno_seri) AS SatirSayisi FROM SIPARISLER WITH (NOLOCK) WHERE ";
		string text2 = text;
		int num = (int)SiparisTipi;
		text = text2 + "sip_tip=" + num + " ";
		string text3 = text;
		num = (int)SiparisCinsi;
		text = text3 + "AND sip_cins=" + num + " ";
		if (SiparisSeri != "")
		{
			text += "AND sip_evrakno_seri=@sip_evrakno_seri ";
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_seri", SiparisSeri);
		}
		if (CariKodu != "")
		{
			text += "AND sip_musteri_kod=@sip_musteri_kod ";
			sqlCommand.Parameters.AddWithValue("@sip_musteri_kod", CariKodu);
		}
		text += "GROUP BY sip_evrakno_seri,sip_evrakno_sira,sip_tarih,sip_tip,sip_cins,sip_musteri_kod";
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
		sqlCommand.CommandText = text;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			sqlCommand.Connection = sqlDB.Connection;
			new SqlDataAdapter(sqlCommand).Fill(dataTable);
		}
		catch
		{
		}
		return dataTable;
	}

	public static DataTable GetSiparislerOnayli(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string SiparisSeri)
	{
		DataTable dataTable = new DataTable();
		SqlCommand sqlCommand = new SqlCommand();
		string text = "SELECT RECno,SiparisSeriNo,SiparisSiraNo,SiparisKarsilanmaTarihi,EvrakTipi,MikroyaAktarildi,MikroSeriNo,MikroSiraNo,MikroyaAktarilmaTarihi,CASE WHEN MikroyaAktarildi=1 THEN 'Evet' ELSE 'Hayır' END AS MikroyaAktarildiYazi FROM _SIPARIS_KARSILAMA_ARA_TABLO WITH (NOLOCK) ";
		if (SiparisSeri != "")
		{
			text += "WHERE SiparisSeriNo=@SiparisSeriNo ";
			sqlCommand.Parameters.AddWithValue("@SiparisSeriNo", SiparisSeri);
		}
		text += " ORDER BY SiparisKarsilanmaTarihi DESC";
		sqlCommand.CommandText = text;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			sqlCommand.Connection = sqlDB.Connection;
			new SqlDataAdapter(sqlCommand).Fill(dataTable);
		}
		catch
		{
		}
		return dataTable;
	}

	public static double GetSiparisKalanMiktar(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int sip_RECid_RECno)
	{
		double result = 0.0;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		try
		{
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			sqlCommand.CommandText = "SELECT (sip_miktar-sip_teslim_miktar) AS kalanmiktar FROM SIPARISLER WHERE sip_RECid_RECno=@sip_RECid_RECno";
			sqlCommand.Parameters.AddWithValue("@sip_RECid_RECno", sip_RECid_RECno);
			object obj = sqlCommand.ExecuteScalar();
			if (obj != null)
			{
				result = (double)obj;
			}
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		return result;
	}

	public static double GetSiparisKalanMiktar(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid sip_uid)
	{
		double result = 0.0;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		try
		{
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			sqlCommand.CommandText = "SELECT (sip_miktar-sip_teslim_miktar) AS kalanmiktar FROM SIPARISLER WHERE sip_Guid=@sip_Guid";
			sqlCommand.Parameters.AddWithValue("@sip_Guid", sip_uid);
			object obj = sqlCommand.ExecuteScalar();
			if (obj != null)
			{
				result = (double)obj;
			}
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		return result;
	}

	public static List<SiparisOnaylamaListItem> GetSiparisOnaylamaListItem(SqlConnection openedconnection, enum_CariListelemeSecenekleri CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu, int SiparisOnaylamaMaliyetHesaplamaSekli)
	{
		string text = "SELECT siparis_tarihi,evrak_seri,evrak_sira,temsilci_kodu,temsilci_adi,temsilci_soyadi,cari_kodu,cari_unvan,SUM(ara_toplam) AS ara_toplam,SUM(iskonto) AS iskonto,SUM(masraf) AS masraf,SUM(vergi) AS vergi,COUNT(*) AS satir_sayisi,SUM(net_toplam_maliyet) AS maliyet, ISNULL(teslimturu, '') AS teslim FROM (SELECT sip_tarih AS siparis_tarihi ,sip_evrakno_seri AS evrak_seri ,sip_evrakno_sira AS evrak_sira ,sip_satici_kod AS temsilci_kodu ,cp.cari_per_adi AS temsilci_adi ,cp.cari_per_soyadi AS temsilci_soyadi ,sip_musteri_kod AS cari_kodu ,cari.cari_unvan1 + ' ' + cari.cari_unvan2 AS cari_unvan ,sip_tutar AS ara_toplam ,sip_iskonto_1 + sip_iskonto_2 + sip_iskonto_3 + sip_iskonto_4 + sip_iskonto_5 + sip_iskonto_6 AS iskonto ,sip_masraf_1 + sip_masraf_2 + sip_masraf_3 + sip_masraf_4 AS masraf ,sip_vergi + sip_masvergi AS vergi ";
		text = SiparisOnaylamaMaliyetHesaplamaSekli switch
		{
			1 => text + ",(SELECT TOP 1 (sas.sas_brut_fiyat-sas.sas_isk_miktar1-sas.sas_isk_miktar2-sas.sas_isk_miktar3-sas.sas_isk_miktar4-sas.sas_isk_miktar5-sas.sas_isk_miktar6) FROM SATINALMA_SARTLARI AS sas WHERE sas.sas_stok_kod=sip_stok_kod AND (sas.sas_basla_tarih<=sip_tarih OR sas.sas_basla_tarih<'1910-01-01') AND (sas.sas_bitis_tarih>=sip_tarih OR sas.sas_bitis_tarih<'1910-01-01') ORDER BY sas.sas_basla_tarih DESC, sas.sas_evrak_tarih DESC)*sip_miktar AS net_toplam_maliyet", 
			2 => text + ",(SELECT TOP 1 (((fh.sth_tutar-fh.sth_iskonto1-fh.sth_iskonto2-fh.sth_iskonto3-fh.sth_iskonto4-fh.sth_iskonto5-fh.sth_iskonto6)/fh.sth_miktar)*fh.sth_har_doviz_kuru) FROM STOK_HAREKETLERI AS fh WHERE fh.sth_stok_kod=sip_stok_kod AND fh.sth_tarih<=sip_tarih AND fh.sth_tip=0 AND fh.sth_normal_iade=0 AND ((fh.sth_tutar-fh.sth_iskonto1-fh.sth_iskonto2-fh.sth_iskonto3-fh.sth_iskonto4-fh.sth_iskonto5-fh.sth_iskonto6)/sth_miktar)>0 ORDER BY fh.sth_tarih DESC)*sip_miktar AS net_toplam_maliyet", 
			3 => text + ",(SELECT TOP 1 sto_standartmaliyet FROM STOKLAR WHERE sto_kod=sip_stok_kod)*sip_miktar AS net_toplam_maliyet", 
			_ => text + ",((sip_tutar-sip_iskonto_1+sip_iskonto_2+sip_iskonto_3+sip_iskonto_4+sip_iskonto_5+sip_iskonto_6)) AS net_toplam_maliyet", 
		} + ",teslimturu.tslt_ismi AS teslimturu FROM SIPARISLER WITH(NOLOCK) INNER JOIN CARI_PERSONEL_TANIMLARI AS cp WITH(NOLOCK) ON sip_satici_kod = cari_per_kod INNER JOIN CARI_HESAPLAR AS cari WITH(NOLOCK) ON cari_kod = sip_musteri_kod LEFT JOIN TESLIM_TURLERI AS teslimturu WITH(NOLOCK) ON sip_teslimturu = tslt_kod WHERE sip_tip=0 AND sip_cins=0 AND sip_OnaylayanKulNo = 0 AND sip_kapat_fl = 0";
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.BolgeKodunaGore)
		{
			text = text + "AND cari_bolge_kodu in ('" + CariListelemeAktifGrup.Replace(",", "','") + "') ";
		}
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.GrupKodunaGore)
		{
			text = text + "AND cari_grup_kodu in ('" + CariListelemeAktifGrup.Replace(",", "','") + "') ";
		}
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.SektorKodunaGore)
		{
			text = text + "AND cari_sektor_kodu in ('" + CariListelemeAktifGrup.Replace(",", "','") + "') ";
		}
		if (CariListelemeSecenek == enum_CariListelemeSecenekleri.TemsilciKodunaGore)
		{
			if (CariListelemeAktifGrup == "")
			{
				CariListelemeAktifGrup = TemsilciKodu;
			}
			text = text + "AND sip_satici_kod in ('" + CariListelemeAktifGrup.Replace(",", "','") + "') ";
		}
		text += ") AS T GROUP BY siparis_tarihi,evrak_seri,evrak_sira,temsilci_kodu,temsilci_adi,temsilci_soyadi,cari_kodu,cari_unvan,teslimturu ORDER BY siparis_tarihi DESC";
		List<SiparisOnaylamaListItem> list = new List<SiparisOnaylamaListItem>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(text, openedconnection);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				SiparisOnaylamaListItem siparisOnaylamaListItem = new SiparisOnaylamaListItem();
				siparisOnaylamaListItem.siparis_tarihi = sqlDataReader.GetSafeDateTime(0);
				siparisOnaylamaListItem.evrak_seri = sqlDataReader.GetSafeString(1);
				siparisOnaylamaListItem.evrak_sira = sqlDataReader.GetSafeInt32(2);
				siparisOnaylamaListItem.temsilci_kodu = sqlDataReader.GetSafeString(3);
				siparisOnaylamaListItem.temsilci_adi = sqlDataReader.GetSafeString(4);
				siparisOnaylamaListItem.temsilci_soyadi = sqlDataReader.GetSafeString(5);
				siparisOnaylamaListItem.cari_kodu = sqlDataReader.GetSafeString(6);
				siparisOnaylamaListItem.cari_unvan = sqlDataReader.GetSafeString(7);
				siparisOnaylamaListItem.ara_toplam = sqlDataReader.GetSafeDouble(8);
				siparisOnaylamaListItem.iskonto = sqlDataReader.GetSafeDouble(9);
				siparisOnaylamaListItem.masraf = sqlDataReader.GetSafeDouble(10);
				siparisOnaylamaListItem.vergi = sqlDataReader.GetSafeDouble(11);
				siparisOnaylamaListItem.satir_sayisi = sqlDataReader.GetSafeInt32(12);
				siparisOnaylamaListItem.maliyet = sqlDataReader.GetSafeDouble(13);
				siparisOnaylamaListItem.teslim_turu = sqlDataReader.GetSafeString(14);
				list.Add(siparisOnaylamaListItem);
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
			return list;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static bool GetSiparisOnaylamaOnayla(SqlConnection openedconnection, string evrak_seri, int evrak_sira, int mikro_kullanici_no)
	{
		string cmdText = "UPDATE SIPARISLER SET sip_lastup_date=getdate(),sip_lastup_user=@mikro_user_no,sip_OnaylayanKulNo=@mikro_user_no,sip_cagrilabilir_fl=1 WHERE sip_tip=0 AND sip_cins=0 AND sip_evrakno_seri=@sip_evrakno_seri AND sip_evrakno_sira=@sip_evrakno_sira";
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection);
			sqlCommand.Parameters.AddWithValue("@mikro_user_no", mikro_kullanici_no);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_seri", evrak_seri);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_sira", evrak_sira);
			sqlCommand.ExecuteNonQuery();
			sqlCommand.Dispose();
			return true;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public static bool GetSiparisOnaylamareddet(SqlConnection openedconnection, string evrak_seri, int evrak_sira, int mikro_kullanici_no, string kapama_nedeni)
	{
		string cmdText = "UPDATE SIPARISLER SET sip_lastup_date=getdate(),sip_lastup_user=@mikro_user_no,sip_kapat_fl=1,sip_kapatmanedenkod=@sip_kapatmanedenkod WHERE sip_tip=0 AND sip_cins=0 AND sip_evrakno_seri=@sip_evrakno_seri AND sip_evrakno_sira=@sip_evrakno_sira";
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection);
			sqlCommand.Parameters.AddWithValue("@mikro_user_no", mikro_kullanici_no);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_seri", evrak_seri);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_sira", evrak_sira);
			sqlCommand.Parameters.AddWithValue("@sip_kapatmanedenkod", kapama_nedeni);
			sqlCommand.ExecuteNonQuery();
			sqlCommand.Dispose();
			return true;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
}
