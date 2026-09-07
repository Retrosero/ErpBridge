using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Personeller;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PersonelData
{
	public static Personel GetPersonel(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int per_RECno)
	{
		Personel personel = new Personel();
		string commandText = (new SqlCommand().CommandText = "SELECT per_RECno,per_special1,per_special2,per_special3,per_kod,per_adi,per_soyadi,per_orjdildeadisoyadi,per_sicil_no,per_firma_no,per_sube_no,per_caripers_kodu,per_doviz_cinsi FROM PERSONELLER WITH (NOLOCK) WHERE per_RECno=@per_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@per_RECno", per_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					personel.per_RECno = sqlDataReader.GetSafeInt32(0);
					personel.per_special1 = sqlDataReader.GetSafeString(1);
					personel.per_special2 = sqlDataReader.GetSafeString(2);
					personel.per_special3 = sqlDataReader.GetSafeString(3);
					personel.per_kod = sqlDataReader.GetSafeString(4);
					personel.per_adi = sqlDataReader.GetSafeString(5);
					personel.per_soyadi = sqlDataReader.GetSafeString(6);
					personel.per_orjdildeadisoyadi = sqlDataReader.GetSafeString(7);
					personel.per_sicil_no = sqlDataReader.GetSafeString(8);
					personel.per_firma_no = sqlDataReader.GetSafeInt32(9);
					personel.per_sube_no = sqlDataReader.GetSafeInt32(10);
					personel.per_caripers_kodu = sqlDataReader.GetSafeString(11);
					personel.per_doviz_cinsi = sqlDataReader.GetSafeByte(12);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return personel;
	}

	public static Personel GetPersonel(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid per_Guid)
	{
		Personel personel = new Personel();
		string commandText = (new SqlCommand().CommandText = "SELECT per_special1,per_special2,per_special3,per_kod,per_adi,per_soyadi,per_orjdildeadisoyadi,per_sicil_no,per_firma_no,per_sube_no,per_caripers_kodu,per_doviz_cinsi FROM PERSONELLER WITH (NOLOCK) WHERE per_Guid=@per_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@per_Guid", per_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					personel.per_special1 = sqlDataReader.GetSafeString(0);
					personel.per_special2 = sqlDataReader.GetSafeString(1);
					personel.per_special3 = sqlDataReader.GetSafeString(2);
					personel.per_kod = sqlDataReader.GetSafeString(3);
					personel.per_adi = sqlDataReader.GetSafeString(4);
					personel.per_soyadi = sqlDataReader.GetSafeString(5);
					personel.per_orjdildeadisoyadi = sqlDataReader.GetSafeString(6);
					personel.per_sicil_no = sqlDataReader.GetSafeString(7);
					personel.per_firma_no = sqlDataReader.GetSafeInt32(8);
					personel.per_sube_no = sqlDataReader.GetSafeInt32(9);
					personel.per_caripers_kodu = sqlDataReader.GetSafeString(10);
					personel.per_doviz_cinsi = sqlDataReader.GetSafeByte(11);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return personel;
	}

	public static Personel GetPersonel(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string per_kod)
	{
		Personel personel = new Personel();
		string commandText = (new SqlCommand().CommandText = "SELECT per_special1,per_special2,per_special3,per_kod,per_adi,per_soyadi,per_orjdildeadisoyadi,per_sicil_no,per_firma_no,per_sube_no,per_caripers_kodu,per_doviz_cinsi FROM PERSONELLER WITH (NOLOCK) WHERE per_kod=@per_kod");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@per_kod", per_kod);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					personel.per_special1 = sqlDataReader.GetSafeString(0);
					personel.per_special2 = sqlDataReader.GetSafeString(1);
					personel.per_special3 = sqlDataReader.GetSafeString(2);
					personel.per_kod = sqlDataReader.GetSafeString(3);
					personel.per_adi = sqlDataReader.GetSafeString(4);
					personel.per_soyadi = sqlDataReader.GetSafeString(5);
					personel.per_orjdildeadisoyadi = sqlDataReader.GetSafeString(6);
					personel.per_sicil_no = sqlDataReader.GetSafeString(7);
					personel.per_firma_no = sqlDataReader.GetSafeInt32(8);
					personel.per_sube_no = sqlDataReader.GetSafeInt32(9);
					personel.per_caripers_kodu = sqlDataReader.GetSafeString(10);
					personel.per_doviz_cinsi = sqlDataReader.GetSafeByte(11);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return personel;
	}

	public static DataTable GetPersonelDataTable(SqlConnection OpenedConnection)
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
		string text = "per_RECno AS 'ID',";
		if (num > 15)
		{
			text = "per_Guid AS 'ID',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "per_kod AS 'KOD',per_adi AS 'İSİM',per_soyadi AS 'SOYADI' FROM PERSONELLER WITH (NOLOCK) ORDER BY per_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "PERSONELLER";
		return dataTable;
	}
}
