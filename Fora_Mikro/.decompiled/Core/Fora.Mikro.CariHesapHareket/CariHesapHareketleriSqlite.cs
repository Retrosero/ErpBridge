using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.CariHesapHareket;

public static class CariHesapHareketleriSqlite
{
	public static void GetSonEvrakCariHareketleri(SqliteConnection OpenedConnection, string Cari_Kodu, enum_cha_tip cha_tip, out enum_cha_evrak_tip evraktipi, out DateTime evraktarihi, out double tutar, int cha_grupno, int FirmaNo, int SubeNo, bool SorumlulukMerkeziDetayli, string SorumlulukMerkeziKodu)
	{
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
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
		evraktipi = enum_cha_evrak_tip.Tanimsiz;
		evraktarihi = DateTime.MinValue;
		tutar = 0.0;
		string text3 = "";
		int num = 0;
		string commandText = "";
		switch (cha_tip)
		{
		case enum_cha_tip.Alacak:
			commandText = "SELECT cha_evrak_tip,cha_evrakno_seri,cha_evrakno_sira,cha_tarihi FROM CARI_HESAP_HAREKETLERI WHERE cha_kod=@car_kod AND cha_tip=1 " + text + " ORDER BY cha_tarihi DESC,cha_lastup_date DESC LIMIT 1";
			break;
		case enum_cha_tip.Borc:
			commandText = "SELECT cha_evrak_tip,cha_evrakno_seri,cha_evrakno_sira,cha_tarihi FROM CARI_HESAP_HAREKETLERI WHERE cha_kod=@car_kod AND cha_tip=0 " + text + " ORDER BY cha_tarihi DESC,cha_lastup_date DESC LIMIT 1";
			break;
		case enum_cha_tip.BorcVeAlacak:
			commandText = "SELECT cha_evrak_tip,cha_evrakno_seri,cha_evrakno_sira,cha_tarihi FROM CARI_HESAP_HAREKETLERI WHERE cha_kod=@car_kod " + text + " ORDER BY cha_tarihi DESC,cha_lastup_date DESC LIMIT 1";
			break;
		}
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@car_kod", (object)Cari_Kodu);
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
				evraktipi = (enum_cha_evrak_tip)val2.GetSafeByte(0);
				text3 = val2.GetSafeString(1);
				num = val2.GetSafeInt32(2);
				evraktarihi = val2.GetSafeDateTime(3);
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
		if (evraktipi == enum_cha_evrak_tip.Tanimsiz)
		{
			return;
		}
		string commandText2 = "SELECT SUM(cha_meblag" + text2 + ") FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip=@cha_evrak_tip AND cha_evrakno_seri=@cha_evrakno_seri AND cha_evrakno_sira=@cha_evrakno_sira AND cha_kod=@car_kod " + text;
		new List<CARI_HESAP_HAREKETLERI>();
		try
		{
			SqliteCommand val3 = new SqliteCommand();
			((DbCommand)val3).CommandText = commandText2;
			val3.Connection = OpenedConnection;
			val3.Parameters.AddWithValue("@cha_evrak_tip", (object)(int)evraktipi);
			val3.Parameters.AddWithValue("@cha_evrakno_seri", (object)text3);
			val3.Parameters.AddWithValue("@cha_evrakno_sira", (object)num);
			val3.Parameters.AddWithValue("@car_kod", (object)Cari_Kodu);
			if (cha_grupno != -1)
			{
				val3.Parameters.AddWithValue("@cha_grupno", (object)cha_grupno);
			}
			if (FirmaNo != -1)
			{
				val3.Parameters.AddWithValue("@cha_firmano", (object)FirmaNo);
			}
			if (SubeNo != -1)
			{
				val3.Parameters.AddWithValue("@cha_subeno", (object)SubeNo);
			}
			if (SorumlulukMerkeziDetayli)
			{
				val3.Parameters.AddWithValue("@cha_srmrkkodu", (object)SorumlulukMerkeziKodu);
			}
			object obj = ((DbCommand)val3).ExecuteScalar();
			tutar = double.Parse(obj.ToString());
			((Component)val3).Dispose();
			val3 = null;
		}
		catch (Exception ex2)
		{
			Console.WriteLine(ex2.ToString());
		}
	}

	public static List<double> GetRaporSatisFaturasiMeblag(SqliteConnection OpenedConnection, string temsilci_kodu, DateTime baslangic_tarihi, DateTime bitis_tarihi)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		List<double> list = new List<double>();
		try
		{
			string commandText = "SELECT SUM(cha_meblag*cha_d_kur),cha_evrakno_seri,cha_evrakno_sira FROM CARI_HESAP_HAREKETLERI WHERE cha_tarihi>=@tarih_baslangic AND cha_tarihi<@tarih_bitis AND cha_satici_kodu=@cha_satici_kodu AND cha_evrak_tip=63 AND cha_tip=0 GROUP BY cha_evrakno_seri,cha_evrakno_sira";
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@cha_satici_kodu", (object)temsilci_kodu);
			val.Parameters.AddWithValue("@tarih_baslangic", (object)baslangic_tarihi);
			val.Parameters.AddWithValue("@tarih_bitis", (object)bitis_tarihi);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeDouble(0));
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

	public static List<double> GetRaporTahsilatMeblag(SqliteConnection OpenedConnection, string temsilci_kodu, DateTime baslangic_tarihi, DateTime bitis_tarihi, enum_cha_cinsi cha_cinsi)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		List<double> list = new List<double>();
		try
		{
			int num = (int)cha_cinsi;
			string commandText = "SELECT cha_meblag*cha_d_kur FROM CARI_HESAP_HAREKETLERI WHERE cha_tarihi>=@tarih_baslangic AND cha_tarihi<@tarih_bitis AND cha_evrak_tip=1 AND cha_satici_kodu=@cha_satici_kodu AND cha_cinsi=" + num;
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@cha_satici_kodu", (object)temsilci_kodu);
			val.Parameters.AddWithValue("@tarih_baslangic", (object)baslangic_tarihi);
			val.Parameters.AddWithValue("@tarih_bitis", (object)bitis_tarihi);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeDouble(0));
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

	public static List<double> GetRaporTahsilatToplamMeblag(SqliteConnection OpenedConnection, string temsilci_kodu, DateTime baslangic_tarihi, DateTime bitis_tarihi)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		List<double> list = new List<double>();
		try
		{
			string commandText = "SELECT SUM(cha_meblag*cha_d_kur),cha_evrakno_seri,cha_evrakno_sira FROM CARI_HESAP_HAREKETLERI WHERE cha_tarihi>=@tarih_baslangic AND cha_tarihi<@tarih_bitis AND cha_evrak_tip=1 AND cha_satici_kodu=@cha_satici_kodu GROUP BY cha_evrakno_seri,cha_evrakno_sira";
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@cha_satici_kodu", (object)temsilci_kodu);
			val.Parameters.AddWithValue("@tarih_baslangic", (object)baslangic_tarihi);
			val.Parameters.AddWithValue("@tarih_bitis", (object)bitis_tarihi);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeDouble(0));
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
}
