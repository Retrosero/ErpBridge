using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Dovizler;

public static class DovizCinsiTanimlariSqlite
{
	public static DovizCinsiTanimlari GetDovizCinsiTanimlari(SqliteConnection OpenedConnection)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		DovizCinsiTanimlari dovizCinsiTanimlari = new DovizCinsiTanimlari();
		string commandText = "SELECT Kur_No ,Kur_Tip,Kur_sembol,Kur_adi,Kur_orjAdi,Kur_kusurat_isim,Kur_decimal,Kur_kusurat_sembol FROM KUR_ISIMLERI";
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				int safeByte = val2.GetSafeByte(0);
				dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_Tip = val2.GetSafeString(1);
				dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_sembol = val2.GetSafeString(2);
				dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_adi = val2.GetSafeString(3);
				dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_orjAdi = val2.GetSafeString(4);
				dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_kusurat_isim = val2.GetSafeString(5);
				dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_decimal = val2.GetSafeByte(6);
				dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_kusurat_sembol = val2.GetSafeString(7);
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
		return dovizCinsiTanimlari;
	}
}
