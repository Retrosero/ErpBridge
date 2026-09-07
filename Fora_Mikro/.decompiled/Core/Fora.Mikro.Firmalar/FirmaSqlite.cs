using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Firmalar;

public static class FirmaSqlite
{
	public static Firma GetFirma(SqliteConnection OpenedConnection, int Firmano)
	{
		string commandText = "SELECT fir_sirano,fir_unvan FROM FIRMALAR WHERE fir_sirano=@fir_sirano";
		Firma firma = new Firma();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@fir_sirano", (object)Firmano);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					firma.fir_sirano = val2.GetSafeInt32(0);
					firma.fir_unvan = val2.GetSafeString(1);
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
		return firma;
	}

	public static List<GenelList> GetFirmaGenelList(SqliteConnection OpenedConnection)
	{
		string commandText = "SELECT fir_sirano,fir_unvan FROM FIRMALAR ORDER BY fir_sirano";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					GenelList genelList = new GenelList();
					genelList.Kod = val2.GetSafeInt32(0).ToString();
					genelList.Text = val2.GetSafeString(1);
					list.Add(genelList);
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
}
