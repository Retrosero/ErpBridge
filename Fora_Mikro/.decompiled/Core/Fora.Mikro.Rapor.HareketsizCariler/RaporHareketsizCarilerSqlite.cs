using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Rapor.HareketsizCariler;

public static class RaporHareketsizCarilerSqlite
{
	public static List<RaporHareketsizCariler> GetHareketsizCariler(SqliteConnection OpenedConnection, string CariPersonelKodu)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		List<RaporHareketsizCariler> list = new List<RaporHareketsizCariler>();
		try
		{
			string text = "";
			if (CariPersonelKodu != "")
			{
				text = " AND cari.cari_temsilci_kodu='" + CariPersonelKodu + "' ";
			}
			string commandText = "SELECT cari.cari_kod, cari.cari_unvan1, cari.cari_unvan2, personel.cari_per_kod, personel.cari_per_adi, personel.cari_per_soyadi FROM CARI_HESAPLAR AS cari LEFT JOIN CARI_PERSONEL_TANIMLARI AS personel ON cari.cari_temsilci_kodu=personel.cari_per_kod WHERE cari.cari_kod not in (SELECT sth_cari_kodu FROM STOK_HAREKETLERI WHERE sth_tarih BETWEEN @baslangic_tarihi AND @bitis_tarihi  GROUP BY sth_cari_kodu)" + text + " ORDER BY cari.cari_unvan1";
			_ = DateTime.Now;
			_ = DateTime.Now;
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@baslangic_tarihi", (object)DateTime.Now.AddYears(-1));
			val.Parameters.AddWithValue("@bitis_tarihi", (object)new DateTime(DateTime.Now.Year, 12, 30));
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				RaporHareketsizCariler raporHareketsizCariler = new RaporHareketsizCariler();
				raporHareketsizCariler.cari_kodu = val2.GetSafeString(0);
				raporHareketsizCariler.cari_unvan1 = val2.GetSafeString(1);
				raporHareketsizCariler.cari_unvan2 = val2.GetSafeString(2);
				raporHareketsizCariler.cari_plasiyer_kodu = val2.GetSafeString(3);
				raporHareketsizCariler.cari_plasiyer_adi_soyadi = val2.GetSafeString(4) + " " + val2.GetSafeString(5);
				list.Add(raporHareketsizCariler);
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
