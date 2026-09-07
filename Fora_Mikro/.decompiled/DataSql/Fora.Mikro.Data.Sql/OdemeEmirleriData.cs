using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.OdemeEmri;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct OdemeEmirleriData
{
	public static OdemeEmirleri GetOdemeEmiri(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int sck_RECno)
	{
		OdemeEmirleri odemeEmirleri = new OdemeEmirleri();
		string commandText = (new SqlCommand().CommandText = "SELECT sck_refno,sck_tip,sck_tutar,sck_nerede_cari_cins,sck_nerede_cari_kodu,sck_srmmrk,sck_projekodu FROM ODEME_EMIRLERI WITH (NOLOCK) WHERE sck_RECno=@sck_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@sck_RECno", sck_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					odemeEmirleri.sck_refno = sqlDataReader.GetSafeString(0);
					odemeEmirleri.sck_tip = (enum_sck_tip)sqlDataReader.GetSafeByte(1);
					odemeEmirleri.sck_tutar = sqlDataReader.GetSafeDouble(2);
					odemeEmirleri.sck_nerede_cari_cins = (enum_sck_nerede_cari_cins)sqlDataReader.GetSafeByte(3);
					odemeEmirleri.sck_nerede_cari_kodu = sqlDataReader.GetSafeString(4);
					odemeEmirleri.sck_srmmrk = sqlDataReader.GetSafeString(5);
					odemeEmirleri.sck_projekodu = sqlDataReader.GetSafeString(6);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return odemeEmirleri;
	}

	public static OdemeEmirleri GetOdemeEmiri(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid sck_Guid)
	{
		OdemeEmirleri odemeEmirleri = new OdemeEmirleri();
		string commandText = (new SqlCommand().CommandText = "SELECT sck_refno,sck_tip,sck_tutar,sck_nerede_cari_cins,sck_nerede_cari_kodu,sck_srmmrk,sck_projekodu FROM ODEME_EMIRLERI WITH (NOLOCK) WHERE sck_Guid=@sck_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@sck_Guid", sck_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					odemeEmirleri.sck_refno = sqlDataReader.GetSafeString(0);
					odemeEmirleri.sck_tip = (enum_sck_tip)sqlDataReader.GetSafeByte(1);
					odemeEmirleri.sck_tutar = sqlDataReader.GetSafeDouble(2);
					odemeEmirleri.sck_nerede_cari_cins = (enum_sck_nerede_cari_cins)sqlDataReader.GetSafeByte(3);
					odemeEmirleri.sck_nerede_cari_kodu = sqlDataReader.GetSafeString(4);
					odemeEmirleri.sck_srmmrk = sqlDataReader.GetSafeString(5);
					odemeEmirleri.sck_projekodu = sqlDataReader.GetSafeString(6);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return odemeEmirleri;
	}

	public static string GetOdemeEmiriNeredeCariKodu(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string sck_refno)
	{
		string result = "";
		string commandText = (new SqlCommand().CommandText = "SELECT sck_nerede_cari_kodu FROM ODEME_EMIRLERI WITH (NOLOCK) WHERE sck_refno=@sck_refno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@sck_refno", sck_refno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					result = sqlDataReader.GetSafeString(0);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return result;
	}

	public static string GetOdemeEmiriSahipCariKodu(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string sck_refno)
	{
		string result = "";
		string commandText = (new SqlCommand().CommandText = "SELECT sck_sahip_cari_kodu FROM ODEME_EMIRLERI WITH (NOLOCK) WHERE sck_refno=@sck_refno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@sck_refno", sck_refno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					result = sqlDataReader.GetSafeString(0);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return result;
	}
}
