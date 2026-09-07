using System;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Kurlar;

public static class KurSqlite
{
	public static Kur GetKur(SqliteConnection OpenedConnection, int dov_no, string fiyat_no)
	{
		if (dov_no == 0)
		{
			return new Kur();
		}
		string commandText = "SELECT dov_no,dov_tarih,dov_fiyat1,dov_fiyat2,dov_fiyat3,dov_fiyat4 FROM DOVIZ_KURLARI WHERE dov_no=@dov_no ORDER BY dov_tarih DESC LIMIT 1";
		if (fiyat_no == "" || fiyat_no == null)
		{
			fiyat_no = "1";
		}
		Kur kur = new Kur();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@dov_no", (object)dov_no);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					kur.dov_no = val2.GetSafeInt16(0);
					kur.dov_tarih = val2.GetSafeDateTime(1);
					switch (fiyat_no)
					{
					case "1":
						kur.dov_fiyat = val2.GetSafeDouble(2);
						break;
					case "2":
						kur.dov_fiyat = val2.GetSafeDouble(3);
						break;
					case "3":
						kur.dov_fiyat = val2.GetSafeDouble(4);
						break;
					case "4":
						kur.dov_fiyat = val2.GetSafeDouble(5);
						break;
					}
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
		return kur;
	}

	public static Kur GetKur(SqliteConnection OpenedConnection, int dov_no, string fiyat_no, DateTime tarih)
	{
		if (dov_no == 0)
		{
			return new Kur();
		}
		string commandText = "SELECT dov_no,dov_tarih,dov_fiyat1,dov_fiyat2,dov_fiyat3,dov_fiyat4 FROM DOVIZ_KURLARI WHERE dov_no=@dov_no AND dov_tarih<=@dov_tarih ORDER BY dov_tarih DESC LIMIT 1";
		if (fiyat_no == "" || fiyat_no == null)
		{
			fiyat_no = "1";
		}
		Kur kur = new Kur();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@dov_no", (object)dov_no);
				val.Parameters.AddWithValue("@dov_tarih", (object)tarih);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					kur.dov_no = val2.GetSafeInt16(0);
					kur.dov_tarih = val2.GetSafeDateTime(1);
					switch (fiyat_no)
					{
					case "1":
						kur.dov_fiyat = val2.GetSafeDouble(2);
						break;
					case "2":
						kur.dov_fiyat = val2.GetSafeDouble(3);
						break;
					case "3":
						kur.dov_fiyat = val2.GetSafeDouble(4);
						break;
					case "4":
						kur.dov_fiyat = val2.GetSafeDouble(5);
						break;
					}
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
		return kur;
	}
}
