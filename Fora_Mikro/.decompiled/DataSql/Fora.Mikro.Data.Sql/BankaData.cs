using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Bankalar;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BankaData
{
	public static Banka GetBanka(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int ban_RECno)
	{
		Banka banka = new Banka();
		string commandText = (new SqlCommand().CommandText = "SELECT ban_kod,ban_ismi,ban_sube,ban_hesapno,ban_firma_no,ban_doviz_cinsi,ban_temsilci_email FROM BANKALAR WITH (NOLOCK) WHERE ban_RECno=@ban_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ban_RECno", ban_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					banka.ban_kod = sqlDataReader.GetSafeString(0);
					banka.ban_ismi = sqlDataReader.GetSafeString(1);
					banka.ban_sube = sqlDataReader.GetSafeString(2);
					banka.ban_hesapno = sqlDataReader.GetSafeString(3);
					banka.ban_firma_no = sqlDataReader.GetSafeInt32(4);
					banka.ban_doviz_cinsi = sqlDataReader.GetSafeByte(5);
					banka.ban_temsilci_email = sqlDataReader.GetSafeString(6);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return banka;
	}

	public static Banka GetBanka(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid ban_Guid)
	{
		Banka banka = new Banka();
		string commandText = (new SqlCommand().CommandText = "SELECT ban_kod,ban_ismi,ban_sube,ban_hesapno,ban_firma_no,ban_doviz_cinsi,ban_temsilci_email FROM BANKALAR WITH (NOLOCK) WHERE ban_Guid=@ban_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ban_Guid", ban_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					banka.ban_kod = sqlDataReader.GetSafeString(0);
					banka.ban_ismi = sqlDataReader.GetSafeString(1);
					banka.ban_sube = sqlDataReader.GetSafeString(2);
					banka.ban_hesapno = sqlDataReader.GetSafeString(3);
					banka.ban_firma_no = sqlDataReader.GetSafeInt32(4);
					banka.ban_doviz_cinsi = sqlDataReader.GetSafeByte(5);
					banka.ban_temsilci_email = sqlDataReader.GetSafeString(6);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return banka;
	}

	public static Banka GetBanka(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string ban_kod)
	{
		Banka banka = new Banka();
		string commandText = (new SqlCommand().CommandText = "SELECT ban_kod,ban_ismi,ban_sube,ban_hesapno,ban_firma_no,ban_doviz_cinsi,ban_temsilci_email FROM BANKALAR WITH (NOLOCK) WHERE ban_kod=@ban_kod");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ban_kod", ban_kod);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					banka.ban_kod = sqlDataReader.GetSafeString(0);
					banka.ban_ismi = sqlDataReader.GetSafeString(1);
					banka.ban_sube = sqlDataReader.GetSafeString(2);
					banka.ban_hesapno = sqlDataReader.GetSafeString(3);
					banka.ban_firma_no = sqlDataReader.GetSafeInt32(4);
					banka.ban_doviz_cinsi = sqlDataReader.GetSafeByte(5);
					banka.ban_temsilci_email = sqlDataReader.GetSafeString(6);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return banka;
	}

	public static List<Banka> GetBankalar(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int FirmaNo)
	{
		List<Banka> list = new List<Banka>();
		string commandText = (new SqlCommand().CommandText = "SELECT ban_kod,ban_ismi,ban_sube,ban_hesapno,ban_firma_no,ban_doviz_cinsi,ban_temsilci_email FROM BANKALAR WITH (NOLOCK) WHERE ban_firma_no=@ban_firma_no ORDER BY ban_ismi");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ban_firma_no", FirmaNo);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					Banka banka = new Banka();
					banka.ban_kod = sqlDataReader.GetSafeString(0);
					banka.ban_ismi = sqlDataReader.GetSafeString(1);
					banka.ban_sube = sqlDataReader.GetSafeString(2);
					banka.ban_hesapno = sqlDataReader.GetSafeString(3);
					banka.ban_firma_no = sqlDataReader.GetSafeInt32(4);
					banka.ban_doviz_cinsi = sqlDataReader.GetSafeByte(5);
					banka.ban_temsilci_email = sqlDataReader.GetSafeString(6);
					list.Add(banka);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return list;
	}

	public static DataTable GetBankalarDataTable(SqlConnection OpenedConnection)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(OpenedConnection.Database);
		string text = "ban_RECno AS 'RECno',";
		if (mikroVersiyon > 15)
		{
			text = "ban_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "ban_kod AS 'KOD',ban_ismi AS 'İSİM',ban_firma_no AS 'FİRMA NO',ban_sube AS 'ŞUBE', ban_hesapno AS 'HESAP NO',dbo.fn_DovizSembolu(ban_doviz_cinsi) AS 'DÖVİZ CİNSİ',ban_TCMB_Kodu AS 'TCMB BANKA KODU' FROM BANKALAR WITH (NOLOCK) ORDER BY ban_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "BANKALAR";
		return dataTable;
	}
}
