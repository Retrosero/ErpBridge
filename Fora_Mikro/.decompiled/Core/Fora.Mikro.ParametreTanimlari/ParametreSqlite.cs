using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.ParametreTanimlari;

public static class ParametreSqlite
{
	public static void ParametreOku(SqliteConnection OpenedConnection, Parametreler parametreler, string Program, string User, string AnaGrup, string AltGrup)
	{
		List<Parametre> list = new List<Parametre>();
		string commandText = "SELECT ID,ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu,ParametreID,ParametreDegeri FROM _FORA_PARAMETRELER WITH WHERE ParametreProgram=@parametre_program AND ParametreUser=@parametre_user AND ParametreAnaGrubu=@parametre_ana_grubu AND ParametreAltGrubu=@parametre_alt_grubu";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@parametre_program", (object)Program);
				val.Parameters.AddWithValue("@parametre_user", (object)User);
				val.Parameters.AddWithValue("@parametre_ana_grubu", (object)AnaGrup);
				val.Parameters.AddWithValue("@parametre_alt_grubu", (object)AltGrup);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					Parametre parametre = new Parametre();
					if (AppBase.MikroVersiyonu >= 16)
					{
						parametre.IDGuid = ((DbDataReader)val2).GetGuid(0);
					}
					else
					{
						parametre.EskiID = val2.GetSafeInt32(0);
					}
					parametre.ParametreProgram = val2.GetSafeString(1);
					parametre.ParametreUser = val2.GetSafeString(2);
					parametre.ParametreAnaGrubu = val2.GetSafeString(3);
					parametre.ParametreAltGrubu = val2.GetSafeString(4);
					parametre.ParametreID = val2.GetSafeInt32(5);
					parametre.ParametreDegeri = val2.GetSafeString(6);
					list.Add(parametre);
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
		foreach (Parametre item in list)
		{
			foreach (Parametre item2 in parametreler.ParametreListesi)
			{
				if (item.ParametreID == item2.ParametreID && item.ParametreProgram == item2.ParametreProgram && item.ParametreUser == item2.ParametreUser && item.ParametreAnaGrubu == item2.ParametreAnaGrubu && item.ParametreAltGrubu == item2.ParametreAltGrubu)
				{
					item2.EskiID = item.EskiID;
					item2.IDGuid = item.IDGuid;
					item2.ParametreDegeri = item.ParametreDegeri;
					item2.ParametreDBDegeri = item.ParametreDegeri;
					break;
				}
			}
		}
	}
}
