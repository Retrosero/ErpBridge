using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.MuhasebeHesaplari;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MuhasebeHesabiData
{
	public static MuhasebeHesabi GetMuhasebeHesabi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int muh_RECno)
	{
		MuhasebeHesabi muhasebeHesabi = new MuhasebeHesabi();
		string commandText = (new SqlCommand().CommandText = "SELECT muh_RECno,muh_hesap_kod,muh_hesap_isim1,muh_hesap_isim2 FROM MUHASEBE_HESAP_PLANI WITH (NOLOCK) WHERE muh_RECno=@muh_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@muh_RECno", muh_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					muhasebeHesabi.muh_RECno = sqlDataReader.GetSafeInt32(0);
					muhasebeHesabi.muh_hesap_kod = sqlDataReader.GetSafeString(1);
					muhasebeHesabi.muh_hesap_isim1 = sqlDataReader.GetSafeString(2);
					muhasebeHesabi.muh_hesap_isim2 = sqlDataReader.GetSafeString(3);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return muhasebeHesabi;
	}

	public static MuhasebeHesabi GetMuhasebeHesabi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid muh_Guid)
	{
		MuhasebeHesabi muhasebeHesabi = new MuhasebeHesabi();
		string commandText = (new SqlCommand().CommandText = "SELECT muh_hesap_kod,muh_hesap_isim1,muh_hesap_isim2 FROM MUHASEBE_HESAP_PLANI WITH (NOLOCK) WHERE muh_Guid=@muh_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@muh_Guid", muh_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					muhasebeHesabi.muh_hesap_kod = sqlDataReader.GetSafeString(0);
					muhasebeHesabi.muh_hesap_isim1 = sqlDataReader.GetSafeString(1);
					muhasebeHesabi.muh_hesap_isim2 = sqlDataReader.GetSafeString(2);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return muhasebeHesabi;
	}
}
