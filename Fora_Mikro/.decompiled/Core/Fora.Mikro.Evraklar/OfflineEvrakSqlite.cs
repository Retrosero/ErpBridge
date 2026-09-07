using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Enumler;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Evraklar;

public static class OfflineEvrakSqlite
{
	public static void OfflineTabloOlustur(SqliteConnection OpenedConnection)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		OfflineTabloOlusturGercek(OpenedConnection);
		int num = 7;
		List<string> list = new List<string>();
		list.Add("OfflineRECno");
		list.Add("AktarimDurumu");
		list.Add("YeniKayit");
		list.Add("AktarilmaTarihi");
		list.Add("Tipi");
		list.Add("Json");
		list.Add("HataString");
		SqliteCommand val = new SqliteCommand();
		((DbCommand)val).CommandText = "SELECT * FROM OFFLINE_KAYITLAR LIMIT 1";
		val.Connection = OpenedConnection;
		SqliteDataReader val2 = val.ExecuteReader();
		bool flag = false;
		if (((DbDataReader)val2).FieldCount != num)
		{
			flag = true;
		}
		if (!flag)
		{
			for (int i = 0; i < ((DbDataReader)val2).FieldCount; i++)
			{
				if (((DbDataReader)val2).GetName(i) != list[i])
				{
					flag = true;
					break;
				}
			}
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		if (flag)
		{
			SqliteCommand val3 = new SqliteCommand
			{
				CommandText = "DROP TABLE IF EXISTS OFFLINE_KAYITLAR",
				Connection = OpenedConnection
			};
			((DbCommand)val3).ExecuteNonQuery();
			((Component)val3).Dispose();
			OfflineTabloOlusturGercek(OpenedConnection);
		}
	}

	public static void OfflineTabloOlusturV2(SqliteConnection OpenedConnection)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		OfflineTabloOlusturGercekV2(OpenedConnection);
		int num = 7;
		List<string> list = new List<string>();
		list.Add("OfflineRECno");
		list.Add("AktarimDurumu");
		list.Add("YeniKayit");
		list.Add("AktarilmaTarihi");
		list.Add("Tipi");
		list.Add("Evrak");
		list.Add("HataString");
		SqliteCommand val = new SqliteCommand();
		((DbCommand)val).CommandText = "SELECT * FROM OFFLINE_KAYITLAR LIMIT 1";
		val.Connection = OpenedConnection;
		SqliteDataReader val2 = val.ExecuteReader();
		bool flag = false;
		if (((DbDataReader)val2).FieldCount != num)
		{
			flag = true;
		}
		if (!flag)
		{
			for (int i = 0; i < ((DbDataReader)val2).FieldCount; i++)
			{
				if (((DbDataReader)val2).GetName(i) != list[i])
				{
					flag = true;
					break;
				}
			}
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		if (flag)
		{
			SqliteCommand val3 = new SqliteCommand
			{
				CommandText = "DROP TABLE IF EXISTS OFFLINE_KAYITLAR",
				Connection = OpenedConnection
			};
			((DbCommand)val3).ExecuteNonQuery();
			((Component)val3).Dispose();
			OfflineTabloOlusturGercekV2(OpenedConnection);
		}
	}

	private static void OfflineTabloOlusturGercek(SqliteConnection OpenedConnection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "CREATE TABLE IF NOT EXISTS [OFFLINE_KAYITLAR] ([OfflineRECno] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,[AktarimDurumu] INTEGER NULL,[YeniKayit] INTEGER NULL,[AktarilmaTarihi] TEXT NULL,[Tipi] INTEGER NULL,[Json] TEXT NULL,[HataString] TEXT NULL)";
		SqliteCommand val = new SqliteCommand
		{
			CommandText = commandText,
			Connection = OpenedConnection
		};
		((DbCommand)val).ExecuteScalar();
		((Component)val).Dispose();
	}

	private static void OfflineTabloOlusturGercekV2(SqliteConnection OpenedConnection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "CREATE TABLE IF NOT EXISTS [OFFLINE_KAYITLAR] ([OfflineRECno] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,[AktarimDurumu] INTEGER NULL,[YeniKayit] INTEGER NULL,[AktarilmaTarihi] TEXT NULL,[Tipi] INTEGER NULL,[Evrak] BLOB NULL,[HataString] TEXT NULL)";
		SqliteCommand val = new SqliteCommand
		{
			CommandText = commandText,
			Connection = OpenedConnection
		};
		((DbCommand)val).ExecuteScalar();
		((Component)val).Dispose();
	}

	public static bool EvrakOfflineKaydetV2(SqliteConnection OpenedConnection, OfflineEvrakV2 offlineevrak)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		try
		{
			SqliteCommand val = new SqliteCommand("INSERT INTO [OFFLINE_KAYITLAR] (AktarimDurumu,YeniKayit,AktarilmaTarihi,Tipi,Evrak,HataString) VALUES (@aktarim_durumu,@yeni_kayit,@aktarilma_tarihi,@tipi,@evrak,@hata_string)", OpenedConnection);
			val.Parameters.AddWithValue("@aktarim_durumu", (object)(int)offlineevrak.AktarimDurumu);
			val.Parameters.AddWithValue("@yeni_kayit", (object)offlineevrak.YeniKayit);
			val.Parameters.AddWithValue("@aktarilma_tarihi", (object)offlineevrak.AktarilmaTarihi);
			val.Parameters.AddWithValue("@tipi", (object)(int)offlineevrak.Tipi);
			val.Parameters.AddWithValue("@evrak", (object)offlineevrak.Evrak);
			val.Parameters.AddWithValue("@hata_string", (object)offlineevrak.HataString);
			((DbCommand)val).ExecuteScalar();
			((Component)val).Dispose();
		}
		catch
		{
			result = false;
		}
		return result;
	}

	public static bool EvrakOfflineGuncelleV2(SqliteConnection OpenedConnection, OfflineEvrakV2 offlineevrak)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		try
		{
			SqliteCommand val = new SqliteCommand("UPDATE [OFFLINE_KAYITLAR] SET AktarimDurumu=@aktarim_durumu,YeniKayit=@yeni_kayit,AktarilmaTarihi=@aktarilma_tarihi,Tipi=@tipi,Evrak=@evrak,HataString=@hata_string WHERE OfflineRECno=@offline_recno", OpenedConnection);
			val.Parameters.AddWithValue("@aktarim_durumu", (object)(int)offlineevrak.AktarimDurumu);
			val.Parameters.AddWithValue("@yeni_kayit", (object)offlineevrak.YeniKayit);
			val.Parameters.AddWithValue("@aktarilma_tarihi", (object)offlineevrak.AktarilmaTarihi);
			val.Parameters.AddWithValue("@tipi", (object)(int)offlineevrak.Tipi);
			val.Parameters.AddWithValue("@evrak", (object)offlineevrak.Evrak);
			val.Parameters.AddWithValue("@hata_string", (object)offlineevrak.HataString);
			val.Parameters.AddWithValue("@offline_recno", (object)offlineevrak.OfflineRECno);
			((DbCommand)val).ExecuteScalar();
			((Component)val).Dispose();
		}
		catch
		{
			result = false;
		}
		return result;
	}

	public static bool EvrakOfflineSil(SqliteConnection OpenedConnection, int OfflineRecNo)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		try
		{
			SqliteCommand val = new SqliteCommand("DELETE FROM [OFFLINE_KAYITLAR] WHERE OfflineRECno=@offline_recno", OpenedConnection);
			val.Parameters.AddWithValue("@offline_recno", (object)OfflineRecNo);
			((DbCommand)val).ExecuteScalar();
			((Component)val).Dispose();
		}
		catch
		{
			result = false;
		}
		return result;
	}

	public static List<OfflineEvrakV2> GetOfflineEvraklarV2(SqliteConnection OpenedConnection, enum_EvrakAktarimDurumu AktarimDurumu, bool TerstenMi, int KacGunluk)
	{
		string text = "WHERE (AktarilmaTarihi>=@tarih OR AktarilmaTarihi<@tariheski)";
		if (AktarimDurumu != enum_EvrakAktarimDurumu.Hepsi)
		{
			string text2 = text;
			int num = (int)AktarimDurumu;
			text = text2 + " AND AktarimDurumu=" + num;
		}
		string text3 = "";
		text3 = ((!TerstenMi) ? ("SELECT * FROM OFFLINE_KAYITLAR " + text + " ORDER BY OfflineRECno") : ("SELECT * FROM OFFLINE_KAYITLAR " + text + " ORDER BY OfflineRECno DESC"));
		List<OfflineEvrakV2> list = new List<OfflineEvrakV2>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = text3;
				val.Parameters.AddWithValue("@tarih", (object)DateTime.Now.AddDays(KacGunluk));
				val.Parameters.AddWithValue("@tariheski", (object)DateTime.Now.AddYears(-9));
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					OfflineEvrakV2 offlineEvrakV = new OfflineEvrakV2();
					offlineEvrakV.OfflineRECno = val2.GetSafeInt32(0);
					offlineEvrakV.AktarimDurumu = (enum_EvrakAktarimDurumu)val2.GetSafeInt32(1);
					offlineEvrakV.YeniKayit = val2.GetSafeBoolean(2);
					offlineEvrakV.AktarilmaTarihi = val2.GetSafeDateTime(3);
					offlineEvrakV.Tipi = (enum_AndroidAktarimTipi)val2.GetSafeInt32(4);
					offlineEvrakV.Evrak = (byte[])((DbDataReader)val2).GetValue(5);
					offlineEvrakV.HataString = val2.GetSafeString(6);
					list.Add(offlineEvrakV);
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
		return list;
	}

	public static OfflineEvrakV2 GetOfflineEvrakV2(SqliteConnection OpenedConnection, int OfflineRECno)
	{
		string commandText = "SELECT * FROM OFFLINE_KAYITLAR WHERE OfflineRECno=@OfflineRECno";
		OfflineEvrakV2 offlineEvrakV = new OfflineEvrakV2();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@OfflineRECno", (object)OfflineRECno);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					offlineEvrakV.OfflineRECno = val2.GetSafeInt32(0);
					offlineEvrakV.AktarimDurumu = (enum_EvrakAktarimDurumu)val2.GetSafeInt32(1);
					offlineEvrakV.YeniKayit = val2.GetSafeBoolean(2);
					offlineEvrakV.AktarilmaTarihi = val2.GetSafeDateTime(3);
					offlineEvrakV.Tipi = (enum_AndroidAktarimTipi)val2.GetSafeInt32(4);
					offlineEvrakV.Evrak = (byte[])((DbDataReader)val2).GetValue(5);
					offlineEvrakV.HataString = val2.GetSafeString(6);
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
		return offlineEvrakV;
	}
}
