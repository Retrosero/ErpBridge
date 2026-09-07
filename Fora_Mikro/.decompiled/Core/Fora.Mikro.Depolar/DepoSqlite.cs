using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Depolar;

public static class DepoSqlite
{
	public static Depo GetDepo(SqliteConnection connection, int dep_no)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WHERE dep_no=@dep_no";
		Depo depo = new Depo();
		if (dep_no == 0)
		{
			depo.dep_no = 0;
			depo.dep_adi = "Tüm Depolar";
			return depo;
		}
		try
		{
			SqliteCommand val = connection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@dep_no", (object)dep_no);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					depo.dep_no = val2.GetSafeInt32(0);
					depo.dep_adi = val2.GetSafeString(1);
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
		return depo;
	}

	public static List<GenelList> GetDepoGenelList(SqliteConnection connection, int firmano)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WHERE (dep_firmano=@dep_firmano OR @dep_firmano=-1) ORDER BY dep_no";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = connection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@dep_firmano", (object)firmano);
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
