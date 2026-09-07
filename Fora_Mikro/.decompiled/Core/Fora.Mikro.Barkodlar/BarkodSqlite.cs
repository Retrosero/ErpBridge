using System;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Barkodlar;

public static class BarkodSqlite
{
	public static Barkod GetBarkodBilgisi(SqliteConnection OpenedConnection, string Barkod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT bar_kodu,bar_stokkodu,bar_partikodu,bar_serino_veya_bagkodu,bar_lotno,bar_barkodtipi,bar_icerigi,bar_birimpntr,bar_master,bar_bedenpntr,bar_renkpntr,bar_baglantitipi FROM BARKOD_TANIMLARI WHERE bar_kodu=@Barkod";
		Barkod barkod = new Barkod();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@Barkod", (object)Barkod);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				barkod.bar_kodu = val2.GetSafeString(0);
				barkod.bar_stokkodu = val2.GetSafeString(1);
				barkod.bar_partikodu = val2.GetSafeString(2);
				barkod.bar_serino_veya_bagkodu = val2.GetSafeString(3);
				try
				{
					barkod.bar_lotno = val2.GetSafeInt32(4);
					barkod.bar_barkodtipi = val2.GetSafeByte(5);
					barkod.bar_icerigi = val2.GetSafeByte(6);
					barkod.bar_birimpntr = val2.GetSafeByte(7);
					barkod.bar_master = val2.GetSafeBoolean(8);
					barkod.bar_bedenpntr = val2.GetSafeByte(9);
					barkod.bar_renkpntr = val2.GetSafeByte(10);
					barkod.bar_baglantitipi = val2.GetSafeByte(11);
				}
				catch
				{
				}
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
		return barkod;
	}

	public static string GetBarkodKodu(SqliteConnection OpenedConnection, string StokKodu, int BirimPntr)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT bar_kodu FROM BARKOD_TANIMLARI WHERE bar_stokkodu=@bar_stokkodu AND bar_birimpntr=@bar_birimpntr";
		string result = "";
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@bar_stokkodu", (object)StokKodu);
			val.Parameters.AddWithValue("@bar_birimpntr", (object)BirimPntr);
			object obj = ((DbCommand)val).ExecuteScalar();
			result = ((obj != null && !(obj is DBNull)) ? obj.ToString() : "");
			((Component)val).Dispose();
		}
		catch
		{
		}
		return result;
	}
}
