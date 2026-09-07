using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokPaketleri;

public static class StokPaketSqlite
{
	public static List<STOK_PAKET_TANIMLARI> GetStokPaket(SqliteConnection connection, string pak_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT pak_kod,pak_stokkod,pak_miktar,pak_aciklama,pak_satirno,pak_fiyat,pak_vergidahilfl,pak_master_tip,pak_detay_tip,pak_doviz_cins,pak_ve_veya,pak_ismi FROM STOK_PAKET_TANIMLARI WHERE pak_kod=@pak_kod";
		List<STOK_PAKET_TANIMLARI> list = new List<STOK_PAKET_TANIMLARI>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@pak_kod", (object)pak_kod);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				STOK_PAKET_TANIMLARI sTOK_PAKET_TANIMLARI = new STOK_PAKET_TANIMLARI();
				sTOK_PAKET_TANIMLARI.pak_kod = val2.GetSafeString(0);
				sTOK_PAKET_TANIMLARI.pak_stokkod = val2.GetSafeString(1);
				sTOK_PAKET_TANIMLARI.pak_miktar = val2.GetSafeDouble(2);
				sTOK_PAKET_TANIMLARI.pak_aciklama = val2.GetSafeString(3);
				sTOK_PAKET_TANIMLARI.pak_satirno = val2.GetSafeInt32(4);
				sTOK_PAKET_TANIMLARI.pak_fiyat = val2.GetSafeDouble(5);
				sTOK_PAKET_TANIMLARI.pak_vergidahilfl = val2.GetSafeInt16(6);
				sTOK_PAKET_TANIMLARI.pak_master_tip = val2.GetSafeInt16(7);
				sTOK_PAKET_TANIMLARI.pak_detay_tip = val2.GetSafeInt16(8);
				sTOK_PAKET_TANIMLARI.pak_doviz_cins = val2.GetSafeInt16(9);
				sTOK_PAKET_TANIMLARI.pak_ve_veya = val2.GetSafeInt16(10);
				sTOK_PAKET_TANIMLARI.pak_ismi = val2.GetSafeString(11);
				list.Add(sTOK_PAKET_TANIMLARI);
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

	public static List<GenelList> GetPaketGenelList(SqliteConnection connection)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		List<GenelList> list = new List<GenelList>();
		string commandText = "SELECT pak_kod,pak_ismi FROM STOK_PAKET_TANIMLARI GROUP BY pak_kod,pak_ismi ORDER BY pak_kod";
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = val2.GetSafeString(0);
				genelList.Text = val2.GetSafeString(1);
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
