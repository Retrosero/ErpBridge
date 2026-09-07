using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Masraflar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MasrafData
{
	public static Masraf GetMasraf(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int his_RECno)
	{
		Masraf masraf = new Masraf();
		string commandText = (new SqlCommand().CommandText = "SELECT his_kod,his_isim,his_yabanci_isim,his_tipkod,his_sinifkod,his_grupkod,his_dovcinsi,his_oivuygulama,his_oivtutar FROM MASRAF_HESAPLARI WITH (NOLOCK) WHERE his_RECno=@his_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@his_RECno", his_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					masraf.his_kod = sqlDataReader.GetSafeString(0);
					masraf.his_isim = sqlDataReader.GetSafeString(1);
					masraf.his_yabanci_isim = sqlDataReader.GetSafeString(2);
					masraf.his_tipkod = sqlDataReader.GetSafeString(3);
					masraf.his_sinifkod = sqlDataReader.GetSafeString(4);
					masraf.his_grupkod = sqlDataReader.GetSafeString(5);
					masraf.his_dovcinsi = sqlDataReader.GetSafeByte(6);
					masraf.his_oivuygulama = sqlDataReader.GetSafeByte(7);
					masraf.his_oivtutar = sqlDataReader.GetSafeDouble(8);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return masraf;
	}

	public static Masraf GetMasraf(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid his_Guid)
	{
		Masraf masraf = new Masraf();
		string commandText = (new SqlCommand().CommandText = "SELECT his_kod,his_isim,his_yabanci_isim,his_tipkod,his_sinifkod,his_grupkod,his_dovcinsi,his_oivuygulama,his_oivtutar FROM MASRAF_HESAPLARI WITH (NOLOCK) WHERE his_Guid=@his_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@his_Guid", his_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					masraf.his_kod = sqlDataReader.GetSafeString(0);
					masraf.his_isim = sqlDataReader.GetSafeString(1);
					masraf.his_yabanci_isim = sqlDataReader.GetSafeString(2);
					masraf.his_tipkod = sqlDataReader.GetSafeString(3);
					masraf.his_sinifkod = sqlDataReader.GetSafeString(4);
					masraf.his_grupkod = sqlDataReader.GetSafeString(5);
					masraf.his_dovcinsi = sqlDataReader.GetSafeByte(6);
					masraf.his_oivuygulama = sqlDataReader.GetSafeByte(7);
					masraf.his_oivtutar = sqlDataReader.GetSafeDouble(8);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return masraf;
	}

	public static Masraf GetMasraf(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string his_kod)
	{
		Masraf masraf = new Masraf();
		string commandText = (new SqlCommand().CommandText = "SELECT his_kod,his_isim,his_yabanci_isim,his_tipkod,his_sinifkod,his_grupkod,his_dovcinsi,his_oivuygulama,his_oivtutar FROM MASRAF_HESAPLARI WITH (NOLOCK) WHERE his_kod=@his_kod");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@his_kod", his_kod);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					masraf.his_kod = sqlDataReader.GetSafeString(0);
					masraf.his_isim = sqlDataReader.GetSafeString(1);
					masraf.his_yabanci_isim = sqlDataReader.GetSafeString(2);
					masraf.his_tipkod = sqlDataReader.GetSafeString(3);
					masraf.his_sinifkod = sqlDataReader.GetSafeString(4);
					masraf.his_grupkod = sqlDataReader.GetSafeString(5);
					masraf.his_dovcinsi = sqlDataReader.GetSafeByte(6);
					masraf.his_oivuygulama = sqlDataReader.GetSafeByte(7);
					masraf.his_oivtutar = sqlDataReader.GetSafeDouble(8);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return masraf;
	}

	public static DataTable GetMasrafDataTable(SqlConnection OpenedConnection)
	{
		int num = 15;
		if (OpenedConnection.Database.StartsWith("MikroDB_V12"))
		{
			num = 12;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V14"))
		{
			num = 14;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V15"))
		{
			num = 15;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V16"))
		{
			num = 16;
		}
		string text = "his_RECno AS 'ID',";
		if (num > 15)
		{
			text = "his_Guid AS 'ID',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "his_kod AS 'KOD',his_isim AS 'İSİM' FROM MASRAF_HESAPLARI WITH (NOLOCK) ORDER BY his_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "MASRAF_HESAPLARI";
		return dataTable;
	}
}
