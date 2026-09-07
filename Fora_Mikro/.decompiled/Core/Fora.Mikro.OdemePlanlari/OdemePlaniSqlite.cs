using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.OdemePlanlari;

public static class OdemePlaniSqlite
{
	public static OdemePlani GetOdemePlani(SqliteConnection connection, int odp_no)
	{
		string commandText = "SELECT odp_no,odp_kodu,odp_adi FROM ODEME_PLANLARI WHERE odp_no=@odp_no";
		OdemePlani odemePlani = new OdemePlani();
		if (odp_no < 0)
		{
			odemePlani.odp_adi = odp_no.ToString().Remove(0, 1) + " GÜN";
			odemePlani.odp_no = odp_no;
			odemePlani.odp_kodu = "";
		}
		if (odp_no == 0)
		{
			odemePlani.odp_adi = "PEŞİN";
			odemePlani.odp_no = odp_no;
			odemePlani.odp_kodu = "";
		}
		if (odp_no > 0)
		{
			try
			{
				SqliteCommand val = connection.CreateCommand();
				try
				{
					((DbCommand)val).CommandText = commandText;
					val.Parameters.AddWithValue("@odp_no", (object)odp_no);
					SqliteDataReader val2 = val.ExecuteReader();
					while (((DbDataReader)val2).Read())
					{
						odemePlani.odp_no = val2.GetSafeInt32(0);
						odemePlani.odp_kodu = val2.GetSafeString(1);
						odemePlani.odp_adi = val2.GetSafeString(2);
					}
					((DbDataReader)val2).Close();
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		return odemePlani;
	}

	public static List<GenelList> GetOdemePlaniGenelList(SqliteConnection connection)
	{
		string commandText = "SELECT odp_no,odp_adi FROM ODEME_PLANLARI ORDER BY odp_adi COLLATE COLLATION_CASE_INSENSITIVE";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = connection.CreateCommand();
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
