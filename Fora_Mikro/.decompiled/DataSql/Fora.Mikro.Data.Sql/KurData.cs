using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct KurData
{
	public static Kur GetKur(SqlConnection connection, int dov_no, string fiyat_no, DateTime tarih)
	{
		if (dov_no == 0)
		{
			return new Kur();
		}
		tarih = new DateTime(tarih.Year, tarih.Month, tarih.Day);
		if (fiyat_no == "" || fiyat_no == null)
		{
			fiyat_no = "1";
		}
		Kur kur = new Kur();
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			sqlCommand.CommandText = "SELECT TOP 1 dov_no,dov_tarih,dov_fiyat1,dov_fiyat2,dov_fiyat3,dov_fiyat4 FROM DOVIZ_KURLARI WHERE dov_no=@dov_no AND dov_tarih<=@tarih ORDER BY dov_tarih DESC";
			sqlCommand.Parameters.AddWithValue("@dov_no", dov_no);
			sqlCommand.Parameters.AddWithValue("@tarih", tarih);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				kur.dov_no = int.Parse(sqlDataReader[0].ToString());
				kur.dov_tarih = DateTime.Parse(sqlDataReader[1].ToString());
				switch (fiyat_no)
				{
				case "1":
					kur.dov_fiyat = double.Parse(sqlDataReader[2].ToString());
					break;
				case "2":
					kur.dov_fiyat = double.Parse(sqlDataReader[3].ToString());
					break;
				case "3":
					kur.dov_fiyat = double.Parse(sqlDataReader[4].ToString());
					break;
				case "4":
					kur.dov_fiyat = double.Parse(sqlDataReader[5].ToString());
					break;
				}
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return kur;
	}

	public static Kur GetKur(SqlBaglantiBilgileri BaglantiBilgileri, int dov_no, string fiyat_no, DateTime tarih, string MikroAnaDBName)
	{
		Kur result = new Kur();
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, MikroAnaDBName);
			result = GetKur(sqlDB.Connection, dov_no, fiyat_no, tarih);
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		return result;
	}

	public static Kur GetKur(SqlConnection connection, int dov_no, string fiyat_no)
	{
		if (dov_no == 0)
		{
			return new Kur();
		}
		if (fiyat_no == "" || fiyat_no == null)
		{
			fiyat_no = "1";
		}
		Kur kur = new Kur();
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			sqlCommand.CommandText = "SELECT TOP 1 dov_no,dov_tarih,dov_fiyat1,dov_fiyat2,dov_fiyat3,dov_fiyat4 FROM DOVIZ_KURLARI WHERE dov_no=@dov_no ORDER BY dov_tarih DESC";
			sqlCommand.Parameters.AddWithValue("@dov_no", dov_no);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				kur.dov_no = int.Parse(sqlDataReader[0].ToString());
				kur.dov_tarih = DateTime.Parse(sqlDataReader[1].ToString());
				switch (fiyat_no)
				{
				case "1":
					kur.dov_fiyat = double.Parse(sqlDataReader[2].ToString());
					break;
				case "2":
					kur.dov_fiyat = double.Parse(sqlDataReader[3].ToString());
					break;
				case "3":
					kur.dov_fiyat = double.Parse(sqlDataReader[4].ToString());
					break;
				}
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return kur;
	}

	public static Kur GetKur(SqlBaglantiBilgileri BaglantiBilgileri, int dov_no, string fiyat_no, string MikroAnaDBName)
	{
		Kur result = new Kur();
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, MikroAnaDBName);
			result = GetKur(sqlDB.Connection, dov_no, fiyat_no);
		}
		catch
		{
		}
		sqlDB.ConnectionClose();
		return result;
	}
}
