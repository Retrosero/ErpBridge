using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Enumler;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CariPersonelData
{
	public static CariPersonel GetCariPersonel(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int cari_per_RECno)
	{
		CariPersonel cariPersonel = new CariPersonel();
		string commandText = (new SqlCommand().CommandText = "SELECT cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_tip,cari_per_doviz_cinsi,cari_per_prim_adet,cari_per_prim_yuzde,cari_per_prim_carpani,cari_per_basmprimcirotav1,cari_per_basmprimyuz1,cari_per_basmprimcirotav2,cari_per_basmprimyuz2,cari_per_basmprimcirotav3,cari_per_basmprimyuz3,cari_per_basmprimcirotav4,cari_per_basmprimyuz4,cari_per_basmprimcirotav5,cari_per_basmprimyuz5,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_userno,cari_per_depono ,cari_per_cepno,cari_per_mail,cari_takvim_kodu FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) WHERE cari_per_RECno=@cari_per_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@cari_per_RECno", cari_per_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					cariPersonel.cari_per_kod = sqlDataReader.GetSafeString(0);
					cariPersonel.cari_per_adi = sqlDataReader.GetSafeString(1);
					cariPersonel.cari_per_soyadi = sqlDataReader.GetSafeString(2);
					cariPersonel.cari_per_tip = (enum_Cari_Personel_Tip)sqlDataReader.GetSafeByte(3);
					cariPersonel.cari_per_doviz_cinsi = sqlDataReader.GetSafeByte(4);
					cariPersonel.cari_per_prim_adet = sqlDataReader.GetSafeDouble(5);
					cariPersonel.cari_per_prim_yuzde = sqlDataReader.GetSafeDouble(6);
					cariPersonel.cari_per_prim_carpani = sqlDataReader.GetSafeDouble(7);
					cariPersonel.cari_per_basmprimcirotav1 = sqlDataReader.GetSafeDouble(8);
					cariPersonel.cari_per_basmprimyuz1 = sqlDataReader.GetSafeDouble(9);
					cariPersonel.cari_per_basmprimcirotav2 = sqlDataReader.GetSafeDouble(10);
					cariPersonel.cari_per_basmprimyuz2 = sqlDataReader.GetSafeDouble(11);
					cariPersonel.cari_per_basmprimcirotav3 = sqlDataReader.GetSafeDouble(12);
					cariPersonel.cari_per_basmprimyuz3 = sqlDataReader.GetSafeDouble(13);
					cariPersonel.cari_per_basmprimcirotav4 = sqlDataReader.GetSafeDouble(14);
					cariPersonel.cari_per_basmprimyuz4 = sqlDataReader.GetSafeDouble(15);
					cariPersonel.cari_per_basmprimcirotav5 = sqlDataReader.GetSafeDouble(16);
					cariPersonel.cari_per_basmprimyuz5 = sqlDataReader.GetSafeDouble(17);
					cariPersonel.cari_per_kasiyerkodu = sqlDataReader.GetSafeString(18);
					cariPersonel.cari_per_kasiyersifresi = sqlDataReader.GetSafeString(19);
					cariPersonel.cari_per_kasiyerAmiri = sqlDataReader.GetSafeString(20);
					cariPersonel.cari_per_userno = sqlDataReader.GetSafeInt32(21);
					cariPersonel.cari_per_depono = sqlDataReader.GetSafeInt32(22);
					cariPersonel.cari_per_cepno = sqlDataReader.GetSafeString(23);
					cariPersonel.cari_per_mail = sqlDataReader.GetSafeString(24);
					cariPersonel.cari_takvim_kodu = sqlDataReader.GetSafeString(25);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return cariPersonel;
	}

	public static CariPersonel GetCariPersonel(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid cari_per_Guid)
	{
		CariPersonel cariPersonel = new CariPersonel();
		string commandText = (new SqlCommand().CommandText = "SELECT cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_tip,cari_per_doviz_cinsi,cari_per_prim_adet,cari_per_prim_yuzde,cari_per_prim_carpani,cari_per_basmprimcirotav1,cari_per_basmprimyuz1,cari_per_basmprimcirotav2,cari_per_basmprimyuz2,cari_per_basmprimcirotav3,cari_per_basmprimyuz3,cari_per_basmprimcirotav4,cari_per_basmprimyuz4,cari_per_basmprimcirotav5,cari_per_basmprimyuz5,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_userno,cari_per_depono ,cari_per_cepno,cari_per_mail,cari_takvim_kodu FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) WHERE cari_per_Guid=@cari_per_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@cari_per_Guid", cari_per_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					cariPersonel.cari_per_kod = sqlDataReader.GetSafeString(0);
					cariPersonel.cari_per_adi = sqlDataReader.GetSafeString(1);
					cariPersonel.cari_per_soyadi = sqlDataReader.GetSafeString(2);
					cariPersonel.cari_per_tip = (enum_Cari_Personel_Tip)sqlDataReader.GetSafeByte(3);
					cariPersonel.cari_per_doviz_cinsi = sqlDataReader.GetSafeByte(4);
					cariPersonel.cari_per_prim_adet = sqlDataReader.GetSafeDouble(5);
					cariPersonel.cari_per_prim_yuzde = sqlDataReader.GetSafeDouble(6);
					cariPersonel.cari_per_prim_carpani = sqlDataReader.GetSafeDouble(7);
					cariPersonel.cari_per_basmprimcirotav1 = sqlDataReader.GetSafeDouble(8);
					cariPersonel.cari_per_basmprimyuz1 = sqlDataReader.GetSafeDouble(9);
					cariPersonel.cari_per_basmprimcirotav2 = sqlDataReader.GetSafeDouble(10);
					cariPersonel.cari_per_basmprimyuz2 = sqlDataReader.GetSafeDouble(11);
					cariPersonel.cari_per_basmprimcirotav3 = sqlDataReader.GetSafeDouble(12);
					cariPersonel.cari_per_basmprimyuz3 = sqlDataReader.GetSafeDouble(13);
					cariPersonel.cari_per_basmprimcirotav4 = sqlDataReader.GetSafeDouble(14);
					cariPersonel.cari_per_basmprimyuz4 = sqlDataReader.GetSafeDouble(15);
					cariPersonel.cari_per_basmprimcirotav5 = sqlDataReader.GetSafeDouble(16);
					cariPersonel.cari_per_basmprimyuz5 = sqlDataReader.GetSafeDouble(17);
					cariPersonel.cari_per_kasiyerkodu = sqlDataReader.GetSafeString(18);
					cariPersonel.cari_per_kasiyersifresi = sqlDataReader.GetSafeString(19);
					cariPersonel.cari_per_kasiyerAmiri = sqlDataReader.GetSafeString(20);
					cariPersonel.cari_per_userno = sqlDataReader.GetSafeInt32(21);
					cariPersonel.cari_per_depono = sqlDataReader.GetSafeInt32(22);
					cariPersonel.cari_per_cepno = sqlDataReader.GetSafeString(23);
					cariPersonel.cari_per_mail = sqlDataReader.GetSafeString(24);
					cariPersonel.cari_takvim_kodu = sqlDataReader.GetSafeString(25);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return cariPersonel;
	}

	public static CariPersonel GetCariPersonel(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string cari_per_kod)
	{
		CariPersonel cariPersonel = new CariPersonel();
		string commandText = (new SqlCommand().CommandText = "SELECT cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_tip,cari_per_doviz_cinsi,cari_per_prim_adet,cari_per_prim_yuzde,cari_per_prim_carpani,cari_per_basmprimcirotav1,cari_per_basmprimyuz1,cari_per_basmprimcirotav2,cari_per_basmprimyuz2,cari_per_basmprimcirotav3,cari_per_basmprimyuz3,cari_per_basmprimcirotav4,cari_per_basmprimyuz4,cari_per_basmprimcirotav5,cari_per_basmprimyuz5,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_userno,cari_per_depono ,cari_per_cepno,cari_per_mail,cari_takvim_kodu FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) WHERE cari_per_kod=@cari_per_kod");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@cari_per_kod", cari_per_kod);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					cariPersonel.cari_per_kod = sqlDataReader.GetSafeString(0);
					cariPersonel.cari_per_adi = sqlDataReader.GetSafeString(1);
					cariPersonel.cari_per_soyadi = sqlDataReader.GetSafeString(2);
					cariPersonel.cari_per_tip = (enum_Cari_Personel_Tip)sqlDataReader.GetSafeByte(3);
					cariPersonel.cari_per_doviz_cinsi = sqlDataReader.GetSafeByte(4);
					cariPersonel.cari_per_prim_adet = sqlDataReader.GetSafeDouble(5);
					cariPersonel.cari_per_prim_yuzde = sqlDataReader.GetSafeDouble(6);
					cariPersonel.cari_per_prim_carpani = sqlDataReader.GetSafeDouble(7);
					cariPersonel.cari_per_basmprimcirotav1 = sqlDataReader.GetSafeDouble(8);
					cariPersonel.cari_per_basmprimyuz1 = sqlDataReader.GetSafeDouble(9);
					cariPersonel.cari_per_basmprimcirotav2 = sqlDataReader.GetSafeDouble(10);
					cariPersonel.cari_per_basmprimyuz2 = sqlDataReader.GetSafeDouble(11);
					cariPersonel.cari_per_basmprimcirotav3 = sqlDataReader.GetSafeDouble(12);
					cariPersonel.cari_per_basmprimyuz3 = sqlDataReader.GetSafeDouble(13);
					cariPersonel.cari_per_basmprimcirotav4 = sqlDataReader.GetSafeDouble(14);
					cariPersonel.cari_per_basmprimyuz4 = sqlDataReader.GetSafeDouble(15);
					cariPersonel.cari_per_basmprimcirotav5 = sqlDataReader.GetSafeDouble(16);
					cariPersonel.cari_per_basmprimyuz5 = sqlDataReader.GetSafeDouble(17);
					cariPersonel.cari_per_kasiyerkodu = sqlDataReader.GetSafeString(18);
					cariPersonel.cari_per_kasiyersifresi = sqlDataReader.GetSafeString(19);
					cariPersonel.cari_per_kasiyerAmiri = sqlDataReader.GetSafeString(20);
					cariPersonel.cari_per_userno = sqlDataReader.GetSafeInt32(21);
					cariPersonel.cari_per_depono = sqlDataReader.GetSafeInt32(22);
					cariPersonel.cari_per_cepno = sqlDataReader.GetSafeString(23);
					cariPersonel.cari_per_mail = sqlDataReader.GetSafeString(24);
					cariPersonel.cari_takvim_kodu = sqlDataReader.GetSafeString(25);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return cariPersonel;
	}

	public static CariPersonel GetCariPersonelByAdi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string cari_per_adi)
	{
		CariPersonel cariPersonel = new CariPersonel();
		string commandText = (new SqlCommand().CommandText = "SELECT cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_tip,cari_per_doviz_cinsi,cari_per_prim_adet,cari_per_prim_yuzde,cari_per_prim_carpani,cari_per_basmprimcirotav1,cari_per_basmprimyuz1,cari_per_basmprimcirotav2,cari_per_basmprimyuz2,cari_per_basmprimcirotav3,cari_per_basmprimyuz3,cari_per_basmprimcirotav4,cari_per_basmprimyuz4,cari_per_basmprimcirotav5,cari_per_basmprimyuz5,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_userno,cari_per_depono ,cari_per_cepno,cari_per_mail,cari_takvim_kodu FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) WHERE cari_per_adi=@cari_per_adi");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@cari_per_adi", cari_per_adi);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					cariPersonel.cari_per_kod = sqlDataReader.GetSafeString(0);
					cariPersonel.cari_per_adi = sqlDataReader.GetSafeString(1);
					cariPersonel.cari_per_soyadi = sqlDataReader.GetSafeString(2);
					cariPersonel.cari_per_tip = (enum_Cari_Personel_Tip)sqlDataReader.GetSafeByte(3);
					cariPersonel.cari_per_doviz_cinsi = sqlDataReader.GetSafeByte(4);
					cariPersonel.cari_per_prim_adet = sqlDataReader.GetSafeDouble(5);
					cariPersonel.cari_per_prim_yuzde = sqlDataReader.GetSafeDouble(6);
					cariPersonel.cari_per_prim_carpani = sqlDataReader.GetSafeDouble(7);
					cariPersonel.cari_per_basmprimcirotav1 = sqlDataReader.GetSafeDouble(8);
					cariPersonel.cari_per_basmprimyuz1 = sqlDataReader.GetSafeDouble(9);
					cariPersonel.cari_per_basmprimcirotav2 = sqlDataReader.GetSafeDouble(10);
					cariPersonel.cari_per_basmprimyuz2 = sqlDataReader.GetSafeDouble(11);
					cariPersonel.cari_per_basmprimcirotav3 = sqlDataReader.GetSafeDouble(12);
					cariPersonel.cari_per_basmprimyuz3 = sqlDataReader.GetSafeDouble(13);
					cariPersonel.cari_per_basmprimcirotav4 = sqlDataReader.GetSafeDouble(14);
					cariPersonel.cari_per_basmprimyuz4 = sqlDataReader.GetSafeDouble(15);
					cariPersonel.cari_per_basmprimcirotav5 = sqlDataReader.GetSafeDouble(16);
					cariPersonel.cari_per_basmprimyuz5 = sqlDataReader.GetSafeDouble(17);
					cariPersonel.cari_per_kasiyerkodu = sqlDataReader.GetSafeString(18);
					cariPersonel.cari_per_kasiyersifresi = sqlDataReader.GetSafeString(19);
					cariPersonel.cari_per_kasiyerAmiri = sqlDataReader.GetSafeString(20);
					cariPersonel.cari_per_userno = sqlDataReader.GetSafeInt32(21);
					cariPersonel.cari_per_depono = sqlDataReader.GetSafeInt32(22);
					cariPersonel.cari_per_cepno = sqlDataReader.GetSafeString(23);
					cariPersonel.cari_per_mail = sqlDataReader.GetSafeString(24);
					cariPersonel.cari_takvim_kodu = sqlDataReader.GetSafeString(25);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return cariPersonel;
	}

	public static DataTable GetCariPersonellerDataTable(SqlConnection OpenedConnection)
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
		string text = "cari_per_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "cari_per_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "cari_per_kod AS 'KOD',cari_per_adi AS 'İSİM',cari_per_soyadi AS 'SOYADI', cari_per_kasiyerkodu AS 'KASİYER' FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) ORDER BY cari_per_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "CARI_PERSONEL_TANIMLARI";
		if (num > 15)
		{
			dataTable.Rows.Add(Guid.Empty, "", "", "", "");
		}
		else
		{
			dataTable.Rows.Add(0, "", "", "", "");
		}
		return dataTable;
	}

	public static List<GenelList> GetCariPersonelGenelList(SqlConnection connection)
	{
		string cmdText = "SELECT cari_per_kod,cari_per_adi FROM CARI_PERSONEL_TANIMLARI WITH(NOLOCK) ORDER BY cari_per_adi";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeString(0);
				genelList.Text = sqlDataReader.GetSafeString(1);
				list.Add(genelList);
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return list;
	}

	public static CariPersonel GetCariPersonel(SqlConnection OpenedConnection, string cari_per_kod)
	{
		CariPersonel cariPersonel = new CariPersonel();
		string commandText = (new SqlCommand().CommandText = "SELECT cari_per_kod,cari_per_adi,cari_per_soyadi,cari_per_tip,cari_per_doviz_cinsi,cari_per_prim_adet,cari_per_prim_yuzde,cari_per_prim_carpani,cari_per_basmprimcirotav1,cari_per_basmprimyuz1,cari_per_basmprimcirotav2,cari_per_basmprimyuz2,cari_per_basmprimcirotav3,cari_per_basmprimyuz3,cari_per_basmprimcirotav4,cari_per_basmprimyuz4,cari_per_basmprimcirotav5,cari_per_basmprimyuz5,cari_per_kasiyerkodu,cari_per_kasiyersifresi,cari_per_kasiyerAmiri,cari_per_userno,cari_per_depono ,cari_per_cepno,cari_per_mail,cari_takvim_kodu FROM CARI_PERSONEL_TANIMLARI WITH (NOLOCK) WHERE cari_per_kod=@cari_per_kod");
		try
		{
			using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@cari_per_kod", cari_per_kod);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				cariPersonel.cari_per_kod = sqlDataReader.GetSafeString(0);
				cariPersonel.cari_per_adi = sqlDataReader.GetSafeString(1);
				cariPersonel.cari_per_soyadi = sqlDataReader.GetSafeString(2);
				cariPersonel.cari_per_tip = (enum_Cari_Personel_Tip)sqlDataReader.GetSafeByte(3);
				cariPersonel.cari_per_doviz_cinsi = sqlDataReader.GetSafeByte(4);
				cariPersonel.cari_per_prim_adet = sqlDataReader.GetSafeDouble(5);
				cariPersonel.cari_per_prim_yuzde = sqlDataReader.GetSafeDouble(6);
				cariPersonel.cari_per_prim_carpani = sqlDataReader.GetSafeDouble(7);
				cariPersonel.cari_per_basmprimcirotav1 = sqlDataReader.GetSafeDouble(8);
				cariPersonel.cari_per_basmprimyuz1 = sqlDataReader.GetSafeDouble(9);
				cariPersonel.cari_per_basmprimcirotav2 = sqlDataReader.GetSafeDouble(10);
				cariPersonel.cari_per_basmprimyuz2 = sqlDataReader.GetSafeDouble(11);
				cariPersonel.cari_per_basmprimcirotav3 = sqlDataReader.GetSafeDouble(12);
				cariPersonel.cari_per_basmprimyuz3 = sqlDataReader.GetSafeDouble(13);
				cariPersonel.cari_per_basmprimcirotav4 = sqlDataReader.GetSafeDouble(14);
				cariPersonel.cari_per_basmprimyuz4 = sqlDataReader.GetSafeDouble(15);
				cariPersonel.cari_per_basmprimcirotav5 = sqlDataReader.GetSafeDouble(16);
				cariPersonel.cari_per_basmprimyuz5 = sqlDataReader.GetSafeDouble(17);
				cariPersonel.cari_per_kasiyerkodu = sqlDataReader.GetSafeString(18);
				cariPersonel.cari_per_kasiyersifresi = sqlDataReader.GetSafeString(19);
				cariPersonel.cari_per_kasiyerAmiri = sqlDataReader.GetSafeString(20);
				cariPersonel.cari_per_userno = sqlDataReader.GetSafeInt32(21);
				cariPersonel.cari_per_depono = sqlDataReader.GetSafeInt32(22);
				cariPersonel.cari_per_cepno = sqlDataReader.GetSafeString(23);
				cariPersonel.cari_per_mail = sqlDataReader.GetSafeString(24);
				cariPersonel.cari_takvim_kodu = sqlDataReader.GetSafeString(25);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return cariPersonel;
	}
}
