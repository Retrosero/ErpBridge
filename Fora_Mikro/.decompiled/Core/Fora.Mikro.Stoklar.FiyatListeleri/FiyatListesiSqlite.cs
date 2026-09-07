using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.FiyatListeleri;

public static class FiyatListesiSqlite
{
	public static FiyatListesi GetFiyatListesi(SqliteConnection connection, int sfl_sirano)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT sfl_sirano,sfl_aciklama,sfl_kdvdahil FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI WHERE sfl_sirano=@sfl_sirano";
		FiyatListesi fiyatListesi = new FiyatListesi();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@sfl_sirano", (object)sfl_sirano);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				fiyatListesi.sfl_sirano = int.Parse(((DbDataReader)val2)[0].ToString());
				fiyatListesi.sfl_aciklama = ((DbDataReader)val2)[1].ToString();
				fiyatListesi.sfl_kdvdahil = ((DbDataReader)val2)[2].ToString() == "1";
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return fiyatListesi;
	}

	public static List<GenelList> GetFiyatListesiGenelList(SqliteConnection connection)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT sfl_sirano,sfl_aciklama FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI ORDER BY sfl_sirano";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = ((DbDataReader)val2)[0].ToString();
				genelList.Text = ((DbDataReader)val2)[1].ToString();
				list.Add(genelList);
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
		return list;
	}
}
