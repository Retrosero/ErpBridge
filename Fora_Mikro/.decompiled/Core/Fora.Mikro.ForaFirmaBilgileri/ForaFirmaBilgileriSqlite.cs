using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Mono.Data.Sqlite;

namespace Fora.Mikro.ForaFirmaBilgileri;

public static class ForaFirmaBilgileriSqlite
{
	public static List<string> GetFirmalar(SqliteConnection OpenedConnection)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		List<string> list = new List<string>();
		SqliteCommand val = new SqliteCommand("SELECT FirmaID FROM Firmalar GROUP BY FirmaID");
		val.Connection = OpenedConnection;
		SqliteDataReader val2 = val.ExecuteReader();
		while (((DbDataReader)val2).Read())
		{
			list.Add(((DbDataReader)val2).GetString(0));
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		((Component)val).Dispose();
		return list;
	}

	public static string GetMikroDbName(SqliteConnection OpenedConnection, string FirmaID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		string result = "";
		SqliteCommand val = new SqliteCommand
		{
			CommandText = "SELECT MikroDBName FROM Firmalar WHERE FirmaID='" + FirmaID + "'",
			Connection = OpenedConnection
		};
		SqliteDataReader val2 = val.ExecuteReader();
		if (((DbDataReader)val2).HasRows)
		{
			((DbDataReader)val2).Read();
			result = ((DbDataReader)val2).GetString(0);
		}
		((DbDataReader)val2).Close();
		((DbDataReader)val2).Dispose();
		val2 = null;
		((Component)val).Dispose();
		return result;
	}

	public static void FirmaEkle(SqliteConnection OpenedConnection, string FirmaID, string ServiceIP, string ServiceLocalIP, string BayiID)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		SqliteCommand val = new SqliteCommand("INSERT INTO Firmalar (FirmaID,ServiceIP,ServiceLocalIP,MikroDBName,BayiID,Lisans) VALUES" + "(@firma_id, @service_ip, @service_local_ip, @mikro_db_name, @bayi_id,@lisans)")
		{
			Connection = OpenedConnection
		};
		val.Parameters.AddWithValue("@firma_id", (object)FirmaID);
		val.Parameters.AddWithValue("@service_ip", (object)ServiceIP);
		val.Parameters.AddWithValue("@service_local_ip", (object)ServiceLocalIP);
		val.Parameters.AddWithValue("@mikro_db_name", (object)"");
		val.Parameters.AddWithValue("@bayi_id", (object)BayiID);
		val.Parameters.AddWithValue("@lisans", (object)"");
		((DbCommand)val).ExecuteNonQuery();
	}

	public static bool IsFirmaVar(SqliteConnection OpenedConnection, string FirmaID)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		try
		{
			SqliteCommand val = new SqliteCommand("SELECT FirmaID FROM Firmalar WHERE FirmaID=@firmaid", OpenedConnection);
			val.Parameters.AddWithValue("@firmaid", (object)FirmaID);
			if (((DbCommand)val).ExecuteScalar() != null)
			{
				result = true;
			}
			((Component)val).Dispose();
		}
		catch
		{
		}
		return result;
	}

	public static bool FirmaSil(SqliteConnection OpenedConnection, string FirmaID)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SqliteCommand val = new SqliteCommand("DELETE FROM Firmalar WHERE FirmaID='" + FirmaID + "'")
			{
				Connection = OpenedConnection
			};
			int num = ((DbCommand)val).ExecuteNonQuery();
			((Component)val).Dispose();
			if (num == 1)
			{
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static bool AyarlarDbOlustur(SqliteConnection OpenedConnection)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SqliteCommand val = new SqliteCommand("CREATE TABLE IF NOT EXISTS [Ayarlar] ([ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,[AyarAdi] TEXT NULL,[Deger] TEXT NULL)")
			{
				Connection = OpenedConnection
			};
			((DbCommand)val).ExecuteNonQuery();
			((Component)val).Dispose();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool FirmalarDbOlustur(SqliteConnection OpenedConnection)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SqliteCommand val = new SqliteCommand("CREATE TABLE IF NOT EXISTS [Firmalar] ([FirmaID] TEXT NOT NULL PRIMARY KEY,[ServiceIP] TEXT NULL,[ServiceLocalIP] TEXT NULL,[MikroDBName] TEXT NULL,[BayiID] TEXT NULL,[Lisans] TEXT NULL)")
			{
				Connection = OpenedConnection
			};
			((DbCommand)val).ExecuteNonQuery();
			((Component)val).Dispose();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static List<string> GetServiceIpBilgileri(SqliteConnection OpenedConnection, string FirmaID)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		list.Add("");
		list.Add("");
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = "SELECT FirmaID,ServiceIP,ServiceLocalIP,MikroDBName FROM Firmalar WHERE FirmaID='" + FirmaID + "'",
				Connection = OpenedConnection
			};
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				list[0] = ((DbDataReader)val2).GetString(1);
				list[1] = ((DbDataReader)val2).GetString(2);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return list;
	}

	public static void UpdateServiceIpBilgileri(SqliteConnection OpenedConnection, string FirmaID, string OnlineServiceIP, string OnlineServiceLocalIP)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SqliteCommand val = new SqliteCommand("UPDATE Firmalar SET ServiceIP=@service_ip,ServiceLocalIP=@service_local_ip WHERE FirmaID=@firma_id")
			{
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@firma_id", (object)FirmaID);
			val.Parameters.AddWithValue("@service_ip", (object)OnlineServiceIP);
			val.Parameters.AddWithValue("@service_local_ip", (object)OnlineServiceLocalIP);
			((DbCommand)val).ExecuteNonQuery();
			((Component)val).Dispose();
		}
		catch
		{
		}
	}

	public static void UpdateMikroDbName(SqliteConnection OpenedConnection, string FirmaID, string MikroDbName)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SqliteCommand val = new SqliteCommand("UPDATE Firmalar SET MikroDBName=@mikro_db_name WHERE FirmaID=@firma_id")
			{
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@firma_id", (object)FirmaID);
			val.Parameters.AddWithValue("@mikro_db_name", (object)MikroDbName);
			((DbCommand)val).ExecuteNonQuery();
			((Component)val).Dispose();
		}
		catch
		{
		}
	}

	public static void CreateForaParametrelerTable(SqliteConnection OpenedConnection)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (AppBase.MikroVersiyonu < 16)
			{
				SqliteCommand val = new SqliteCommand("CREATE TABLE IF NOT EXISTS [_FORA_PARAMETRELER] ([ID] INTEGER NOT NULL PRIMARY KEY,[ParametreProgram] TEXT NULL,[ParametreUser] TEXT NULL,[ParametreAnaGrubu] TEXT NULL,[ParametreAltGrubu] TEXT NULL, [ParametreID] INTEGER NULL,[ParametreAdi] TEXT NULL,[ParametreDegeri] TEXT NULL )")
				{
					Connection = OpenedConnection
				};
				((DbCommand)val).ExecuteNonQuery();
				((Component)val).Dispose();
			}
			else
			{
				SqliteCommand val2 = new SqliteCommand("CREATE TABLE IF NOT EXISTS [_FORA_PARAMETRELER] ([ID] BLOB NOT NULL PRIMARY KEY,[ParametreProgram] TEXT NULL,[ParametreUser] TEXT NULL,[ParametreAnaGrubu] TEXT NULL,[ParametreAltGrubu] TEXT NULL, [ParametreID] INTEGER NULL,[ParametreAdi] TEXT NULL,[ParametreDegeri] TEXT NULL )")
				{
					Connection = OpenedConnection
				};
				((DbCommand)val2).ExecuteNonQuery();
				((Component)val2).Dispose();
			}
		}
		catch
		{
		}
	}

	public static bool KullaniciKontrolu(SqliteConnection OpenedConnection, string KullaniciAdi, string EncryptedSifre)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		try
		{
			SqliteCommand val = new SqliteCommand();
			string commandText = "SELECT  COUNT(*) FROM _FORA_PARAMETRELER WHERE ParametreProgram='akilli' AND ParametreUser=@parametre_user AND ParametreAdi='Sifre' AND ParametreDegeri=@parametre_degeri";
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@parametre_user", (object)KullaniciAdi);
			val.Parameters.AddWithValue("@parametre_degeri", (object)EncryptedSifre);
			if (int.Parse(((DbCommand)val).ExecuteScalar().ToString()) != 0)
			{
				result = true;
			}
			((Component)val).Dispose();
		}
		catch
		{
		}
		return result;
	}
}
