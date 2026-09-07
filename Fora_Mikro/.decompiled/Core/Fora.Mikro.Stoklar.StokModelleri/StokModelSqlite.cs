using System;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokModelleri;

public static class StokModelSqlite
{
	public static string GetModelAdi(SqliteConnection OpenedConnection, string mdl_kodu)
	{
		string commandText = "SELECT mdl_ismi FROM STOK_MODEL_TANIMLARI WHERE mdl_kodu=@mdl_kodu";
		string result = "";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@mdl_kodu", (object)mdl_kodu);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					result = val2.GetSafeString(0);
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
		return result;
	}
}
