using System;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Hizmetler;

public static class HizmetSqlite
{
	public static Hizmet GetHizmet(SqliteConnection OpenedConnection, string hiz_kod)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Hizmet hizmet = new Hizmet();
		string commandText = (((DbCommand)new SqliteCommand()).CommandText = "SELECT hiz_RECno,hiz_kod,hiz_tip,hiz_isim,hiz_yabanci_isim,hiz_tipkod,hiz_sinifkod,hiz_grupkod,hiz_sat_muh_kod,hiz_sat_iade_muh_kod,hiz_mal_muh_kod,hiz_sat_mal_muh_kod,hiz_mal_yan_muh_kod,hiz_fiyat,hiz_doviz_cinsi,hiz_isk_grup,hiz_KDV,hiz_muh_sat_isk_kod,hiz_muh_aIiskmuhkod,hiz_ilavemasmuhkod,hiz_operasyon_suresi,hiz_oivuygulama,hiz_oivtutar,hiz_sat_ufrs_fark_muh_kod,hiz_sat_iade_ufrs_fark_muh_kod,hiz_mal_ufrs_fark_muh_kod,hiz_sat_mal_ufrs_fark_muh_kod,hiz_mal_yan_ufrs_fark_muh_kod,hiz_muh_sat_ufrs_fark_isk_kod,hiz_muh_aIiskufrs_fark_muhkod,hiz_ilavemasufrs_fark_muhkod FROM HIZMET_HESAPLARI WITH (NOLOCK) WHERE hiz_kod=@hiz_kod");
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@hiz_kod", (object)hiz_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					hizmet.hiz_RECno = val2.GetSafeInt32(0);
					hizmet.hiz_kod = val2.GetSafeString(1);
					hizmet.hiz_tip = val2.GetSafeByte(2);
					hizmet.hiz_isim = val2.GetSafeString(3);
					hizmet.hiz_yabanci_isim = val2.GetSafeString(4);
					hizmet.hiz_tipkod = val2.GetSafeString(5);
					hizmet.hiz_sinifkod = val2.GetSafeString(6);
					hizmet.hiz_grupkod = val2.GetSafeString(7);
					hizmet.hiz_sat_muh_kod = val2.GetSafeString(8);
					hizmet.hiz_sat_iade_muh_kod = val2.GetSafeString(9);
					hizmet.hiz_mal_muh_kod = val2.GetSafeString(10);
					hizmet.hiz_sat_mal_muh_kod = val2.GetSafeString(11);
					hizmet.hiz_mal_yan_muh_kod = val2.GetSafeString(12);
					hizmet.BirimFiyat.FiyatBrut = val2.GetSafeDouble(13);
					hizmet.hiz_doviz_cinsi = val2.GetSafeByte(14);
					hizmet.hiz_isk_grup = val2.GetSafeString(15);
					hizmet.hiz_KDV = val2.GetSafeByte(16);
					hizmet.hiz_muh_sat_isk_kod = val2.GetSafeString(17);
					hizmet.hiz_muh_aIiskmuhkod = val2.GetSafeString(18);
					hizmet.hiz_ilavemasmuhkod = val2.GetSafeString(19);
					hizmet.hiz_operasyon_suresi = val2.GetSafeInt32(20);
					hizmet.hiz_oivuygulama = val2.GetSafeByte(21);
					hizmet.hiz_oivtutar = val2.GetSafeDouble(22);
					hizmet.hiz_sat_ufrs_fark_muh_kod = val2.GetSafeString(23);
					hizmet.hiz_sat_iade_ufrs_fark_muh_kod = val2.GetSafeString(24);
					hizmet.hiz_mal_ufrs_fark_muh_kod = val2.GetSafeString(25);
					hizmet.hiz_sat_mal_ufrs_fark_muh_kod = val2.GetSafeString(26);
					hizmet.hiz_mal_yan_ufrs_fark_muh_kod = val2.GetSafeString(27);
					hizmet.hiz_muh_sat_ufrs_fark_isk_kod = val2.GetSafeString(28);
					hizmet.hiz_muh_aIiskufrs_fark_muhkod = val2.GetSafeString(29);
					hizmet.hiz_ilavemasufrs_fark_muhkod = val2.GetSafeString(30);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return hizmet;
	}
}
