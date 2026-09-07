using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Database;

public static class AyarlarSqlite
{
	public static int GetStokEklemeListeSecenek(SqliteConnection OpenedConnection)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT Deger FROM Ayarlar WHERE AyarAdi='StokEklemeListeSecenek'";
		int result = 0;
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = OpenedConnection
			};
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				result = int.Parse(val2.GetSafeString(0));
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return result;
	}

	public static void SetStokEklemeListeSecenek(SqliteConnection OpenedConnection, int Deger)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				Connection = OpenedConnection,
				CommandText = "DELETE FROM Ayarlar WHERE AyarAdi='StokEklemeListeSecenek'"
			};
			((DbCommand)val).ExecuteNonQuery();
			((DbCommand)val).CommandText = "INSERT INTO Ayarlar (AyarAdi,Deger) VALUES ('StokEklemeListeSecenek',@deger)";
			val.Parameters.AddWithValue("@deger", (object)Deger);
			((DbCommand)val).ExecuteNonQuery();
			((Component)val).Dispose();
		}
		catch
		{
		}
	}
}
