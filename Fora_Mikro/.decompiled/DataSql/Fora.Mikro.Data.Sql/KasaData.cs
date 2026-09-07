using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Enumler;
using Fora.Mikro.Kasalar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct KasaData
{
	public static Kasa GetKasa(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int kas_RECno)
	{
		Kasa kasa = new Kasa();
		string commandText = (new SqlCommand().CommandText = "SELECT kas_tip,kas_firma_no,kas_kod,kas_isim,kas_muh_kod,kas_doviz_cinsi FROM KASALAR WITH (NOLOCK) WHERE kas_RECno=@kas_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@kas_RECno", kas_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					kasa.kas_tip = (enum_kas_tip)sqlDataReader.GetSafeByte(0);
					kasa.kas_firma_no = sqlDataReader.GetSafeInt32(1);
					kasa.kas_kod = sqlDataReader.GetSafeString(2);
					kasa.kas_isim = sqlDataReader.GetSafeString(3);
					kasa.kas_muh_kod = sqlDataReader.GetSafeString(4);
					kasa.kas_doviz_cinsi = sqlDataReader.GetSafeByte(5);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return kasa;
	}

	public static Kasa GetKasa(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid kas_Guid)
	{
		Kasa kasa = new Kasa();
		string commandText = (new SqlCommand().CommandText = "SELECT kas_tip,kas_firma_no,kas_kod,kas_isim,kas_muh_kod,kas_doviz_cinsi FROM KASALAR WITH (NOLOCK) WHERE kas_Guid=@kas_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@kas_Guid", kas_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					kasa.kas_tip = (enum_kas_tip)sqlDataReader.GetSafeByte(0);
					kasa.kas_firma_no = sqlDataReader.GetSafeInt32(1);
					kasa.kas_kod = sqlDataReader.GetSafeString(2);
					kasa.kas_isim = sqlDataReader.GetSafeString(3);
					kasa.kas_muh_kod = sqlDataReader.GetSafeString(4);
					kasa.kas_doviz_cinsi = sqlDataReader.GetSafeByte(5);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return kasa;
	}

	public static Kasa GetKasa(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string kas_kod)
	{
		Kasa kasa = new Kasa();
		string commandText = (new SqlCommand().CommandText = "SELECT kas_tip,kas_firma_no,kas_kod,kas_isim,kas_muh_kod,kas_doviz_cinsi FROM KASALAR WITH (NOLOCK) WHERE kas_kod=@kas_kod");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@kas_kod", kas_kod);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					kasa.kas_tip = (enum_kas_tip)sqlDataReader.GetSafeByte(0);
					kasa.kas_firma_no = sqlDataReader.GetSafeInt32(1);
					kasa.kas_kod = sqlDataReader.GetSafeString(2);
					kasa.kas_isim = sqlDataReader.GetSafeString(3);
					kasa.kas_muh_kod = sqlDataReader.GetSafeString(4);
					kasa.kas_doviz_cinsi = sqlDataReader.GetSafeByte(5);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return kasa;
	}

	public static DataTable GetKasalarDataTable(SqlConnection OpenedConnection)
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
		string text = "kas_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "kas_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "kas_kod AS 'KOD',kas_isim AS 'İSİM',kas_firma_no AS 'FİRMA NO',dbo.fn_KasaTipi(kas_tip) AS 'KASA TİPİ' FROM KASALAR WITH (NOLOCK) ORDER BY kas_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "KASALAR";
		return dataTable;
	}
}
