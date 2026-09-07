using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Kargolar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct KargoData
{
	public static Kargo GetKargo(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int krg_RECno)
	{
		Kargo kargo = new Kargo();
		string commandText = (new SqlCommand().CommandText = "SELECT krg_RECno,krg_kodu,krg_adi,krg_yetkili,krg_tel,krg_fax,krg_email FROM KARGO_TANIMLARI WITH (NOLOCK) WHERE krg_RECno=@krg_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@krg_RECno", krg_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					kargo.krg_RECno = sqlDataReader.GetSafeInt32(0);
					kargo.krg_kodu = sqlDataReader.GetSafeString(1);
					kargo.krg_adi = sqlDataReader.GetSafeString(2);
					kargo.krg_yetkili = sqlDataReader.GetSafeString(3);
					kargo.krg_tel = sqlDataReader.GetSafeString(4);
					kargo.krg_fax = sqlDataReader.GetSafeString(5);
					kargo.krg_email = sqlDataReader.GetSafeString(6);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return kargo;
	}

	public static Kargo GetKargo(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid krg_Guid)
	{
		Kargo kargo = new Kargo();
		string commandText = (new SqlCommand().CommandText = "SELECT krg_kodu,krg_adi,krg_yetkili,krg_tel,krg_fax,krg_email FROM KARGO_TANIMLARI WITH (NOLOCK) WHERE krg_Guid=@krg_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@krg_Guid", krg_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					kargo.krg_kodu = sqlDataReader.GetSafeString(0);
					kargo.krg_adi = sqlDataReader.GetSafeString(1);
					kargo.krg_yetkili = sqlDataReader.GetSafeString(2);
					kargo.krg_tel = sqlDataReader.GetSafeString(3);
					kargo.krg_fax = sqlDataReader.GetSafeString(4);
					kargo.krg_email = sqlDataReader.GetSafeString(5);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return kargo;
	}

	public static Kargo GetKargo(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string krg_kodu)
	{
		Kargo kargo = new Kargo();
		string commandText = (new SqlCommand().CommandText = "SELECT krg_kodu,krg_adi,krg_yetkili,krg_tel,krg_fax,krg_email FROM KARGO_TANIMLARI WITH (NOLOCK) WHERE krg_kodu=@krg_kodu");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@krg_kodu", krg_kodu);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					kargo.krg_kodu = sqlDataReader.GetSafeString(0);
					kargo.krg_adi = sqlDataReader.GetSafeString(1);
					kargo.krg_yetkili = sqlDataReader.GetSafeString(2);
					kargo.krg_tel = sqlDataReader.GetSafeString(3);
					kargo.krg_fax = sqlDataReader.GetSafeString(4);
					kargo.krg_email = sqlDataReader.GetSafeString(5);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return kargo;
	}
}
