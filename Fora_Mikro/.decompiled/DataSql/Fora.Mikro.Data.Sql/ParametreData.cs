using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ParametreData
{
	public static void ForaParametrelerTablosuOlustur(SqlBaglantiBilgileri baglantibilgileri, string DBName)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(baglantibilgileri, DBName);
		string text = "[ID][int] IDENTITY(1, 1) NOT NULL,";
		if (GenelUtility.GetMikroVersiyon(DBName) >= 16)
		{
			text = "[ID] [uniqueidentifier] NOT NULL,";
		}
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_PARAMETRELER]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[_FORA_PARAMETRELER](" + text + "[ParametreProgram] [nvarchar](40) NOT NULL,[ParametreUser] [nvarchar](40) NOT NULL,[ParametreAnaGrubu] [nvarchar](100) NOT NULL,[ParametreAltGrubu] [nvarchar](100) NOT NULL,[ParametreID] [int] NOT NULL,[ParametreAdi] [nvarchar](100) NOT NULL,[ParametreDegeri] [nvarchar](max) NOT NULL,CONSTRAINT [PK__FORA_PARAMETRELER] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_FORA_PARAMETRELER] ([ParametreProgram] ASC,[ParametreUser] ASC,[ParametreAnaGrubu] ASC,[ParametreAltGrubu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]  END";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.ExecuteNonQuery();
		}
		sqlDB.ConnectionClose();
	}

	public static void ParametreOku(SqlBaglantiBilgileri baglantibilgileri, string DBName, Parametreler parametreler, string Program, string User, string AnaGrup, string AltGrup)
	{
		List<Parametre> list = new List<Parametre>();
		string commandText = (new SqlCommand().CommandText = "SELECT ID,ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu,ParametreID,ParametreDegeri FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram=@ParametreProgram AND ParametreUser=@ParametreUser AND ParametreAnaGrubu=@ParametreAnaGrubu AND ParametreAltGrubu=@ParametreAltGrubu");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantibilgileri, DBName);
			int mikroVersiyon = GenelUtility.GetMikroVersiyon(DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreProgram", Program);
				sqlCommand.Parameters.AddWithValue("@ParametreUser", User);
				sqlCommand.Parameters.AddWithValue("@ParametreAnaGrubu", AnaGrup);
				sqlCommand.Parameters.AddWithValue("@ParametreAltGrubu", AltGrup);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					Parametre parametre = new Parametre();
					if (mikroVersiyon >= 16)
					{
						parametre.IDGuid = sqlDataReader.GetGuid(0);
					}
					else
					{
						parametre.EskiID = sqlDataReader.GetSafeInt32(0);
					}
					parametre.ParametreProgram = sqlDataReader.GetSafeString(1);
					parametre.ParametreUser = sqlDataReader.GetSafeString(2);
					parametre.ParametreAnaGrubu = sqlDataReader.GetSafeString(3);
					parametre.ParametreAltGrubu = sqlDataReader.GetSafeString(4);
					parametre.ParametreID = sqlDataReader.GetSafeInt32(5);
					parametre.ParametreDegeri = sqlDataReader.GetSafeString(6);
					list.Add(parametre);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
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

	public static Parametreler ParametreOku(SqlBaglantiBilgileri baglantibilgileri, string DBName, string Program)
	{
		Parametreler parametreler = new Parametreler();
		string commandText = (new SqlCommand().CommandText = "SELECT ID,ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu,ParametreID,ParametreDegeri FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram=@ParametreProgram");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantibilgileri, DBName);
			int mikroVersiyon = GenelUtility.GetMikroVersiyon(DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreProgram", Program);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					Parametre parametre = new Parametre();
					if (mikroVersiyon >= 16)
					{
						parametre.IDGuid = sqlDataReader.GetGuid(0);
					}
					else
					{
						parametre.EskiID = sqlDataReader.GetSafeInt32(0);
					}
					parametre.ParametreProgram = sqlDataReader.GetSafeString(1);
					parametre.ParametreUser = sqlDataReader.GetSafeString(2);
					parametre.ParametreAnaGrubu = sqlDataReader.GetSafeString(3);
					parametre.ParametreAltGrubu = sqlDataReader.GetSafeString(4);
					parametre.ParametreID = sqlDataReader.GetSafeInt32(5);
					parametre.ParametreDegeri = sqlDataReader.GetSafeString(6);
					parametreler.ParametreListesi.Add(parametre);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return parametreler;
	}

	public static bool ParametreYaz(SqlBaglantiBilgileri baglantibilgileri, string DBName, Parametreler parametreler)
	{
		bool result = true;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantibilgileri, DBName);
			int mikroVersiyon = GenelUtility.GetMikroVersiyon(DBName);
			foreach (Parametre item in parametreler.ParametreListesi)
			{
				string text = "";
				if (mikroVersiyon >= 16)
				{
					if (!item.IDGuid.HasValue && item.ParametreDegeri == item.ParametreDefaultDegeri)
					{
						text = "yapma";
					}
					else if (item.ParametreDegeri == item.ParametreDefaultDegeri && item.IDGuid.HasValue)
					{
						text = "sil";
					}
					else if (item.ParametreDegeri != item.ParametreDefaultDegeri)
					{
						text = (item.IDGuid.HasValue ? "update" : "insert");
					}
				}
				else if (item.EskiID == 0 && item.ParametreDegeri == item.ParametreDefaultDegeri)
				{
					text = "yapma";
				}
				else if (item.ParametreDegeri == item.ParametreDefaultDegeri && item.EskiID != 0)
				{
					text = "sil";
				}
				else if (item.ParametreDegeri != item.ParametreDefaultDegeri)
				{
					text = ((item.EskiID != 0) ? "update" : "insert");
				}
				switch (text)
				{
				case "sil":
				{
					using (SqlCommand sqlCommand5 = sqlDB.Connection.CreateCommand())
					{
						sqlCommand5.CommandText = "DELETE FROM _FORA_PARAMETRELER WHERE ID=@ID";
						if (mikroVersiyon >= 16)
						{
							sqlCommand5.Parameters.AddWithValue("@ID", item.IDGuid);
						}
						else
						{
							sqlCommand5.Parameters.AddWithValue("@ID", item.EskiID);
						}
						sqlCommand5.ExecuteNonQuery();
					}
					break;
				}
				case "insert":
					if (mikroVersiyon >= 16)
					{
						string commandText3 = "BEGIN INSERT INTO _FORA_PARAMETRELER(ID,ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu,ParametreID,ParametreAdi,ParametreDegeri) VALUES(@ID,@ParametreProgram,@ParametreUser,@ParametreAnaGrubu,@ParametreAltGrubu,@ParametreID,@ParametreAdi,@ParametreDegeri) SELECT SCOPE_IDENTITY() END";
						using SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand();
						sqlCommand3.CommandText = commandText3;
						item.IDGuid = Guid.NewGuid();
						sqlCommand3.Parameters.AddWithValue("@ID", item.IDGuid);
						sqlCommand3.Parameters.AddWithValue("@ParametreProgram", item.ParametreProgram);
						sqlCommand3.Parameters.AddWithValue("@ParametreUser", item.ParametreUser);
						sqlCommand3.Parameters.AddWithValue("@ParametreAnaGrubu", item.ParametreAnaGrubu);
						sqlCommand3.Parameters.AddWithValue("@ParametreAltGrubu", item.ParametreAltGrubu);
						sqlCommand3.Parameters.AddWithValue("@ParametreID", item.ParametreID);
						sqlCommand3.Parameters.AddWithValue("@ParametreAdi", item.ParametreAdi);
						sqlCommand3.Parameters.AddWithValue("@ParametreDegeri", item.ParametreDegeri);
						sqlCommand3.ExecuteScalar().ToString();
					}
					else
					{
						string commandText4 = "BEGIN INSERT INTO _FORA_PARAMETRELER(ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu,ParametreID,ParametreAdi,ParametreDegeri) VALUES(@ParametreProgram,@ParametreUser,@ParametreAnaGrubu,@ParametreAltGrubu,@ParametreID,@ParametreAdi,@ParametreDegeri) SELECT SCOPE_IDENTITY() END";
						using SqlCommand sqlCommand4 = sqlDB.Connection.CreateCommand();
						sqlCommand4.CommandText = commandText4;
						sqlCommand4.Parameters.AddWithValue("@ParametreProgram", item.ParametreProgram);
						sqlCommand4.Parameters.AddWithValue("@ParametreUser", item.ParametreUser);
						sqlCommand4.Parameters.AddWithValue("@ParametreAnaGrubu", item.ParametreAnaGrubu);
						sqlCommand4.Parameters.AddWithValue("@ParametreAltGrubu", item.ParametreAltGrubu);
						sqlCommand4.Parameters.AddWithValue("@ParametreID", item.ParametreID);
						sqlCommand4.Parameters.AddWithValue("@ParametreAdi", item.ParametreAdi);
						sqlCommand4.Parameters.AddWithValue("@ParametreDegeri", item.ParametreDegeri);
						sqlCommand4.ExecuteScalar().ToString();
					}
					break;
				case "update":
					if (mikroVersiyon >= 16)
					{
						string commandText = "UPDATE _FORA_PARAMETRELER SET ParametreDegeri=@ParametreDegeri WHERE ID=@ID";
						using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
						sqlCommand.CommandText = commandText;
						sqlCommand.Parameters.AddWithValue("@ParametreDegeri", item.ParametreDegeri);
						sqlCommand.Parameters.AddWithValue("@ID", item.IDGuid);
						sqlCommand.ExecuteNonQuery().ToString();
					}
					else
					{
						string commandText2 = "UPDATE _FORA_PARAMETRELER SET ParametreDegeri=@ParametreDegeri WHERE ID=@ID";
						using SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand();
						sqlCommand2.CommandText = commandText2;
						sqlCommand2.Parameters.AddWithValue("@ParametreDegeri", item.ParametreDegeri);
						sqlCommand2.Parameters.AddWithValue("@ID", item.EskiID);
						sqlCommand2.ExecuteNonQuery().ToString();
					}
					break;
				}
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
			result = false;
		}
		return result;
	}
}
