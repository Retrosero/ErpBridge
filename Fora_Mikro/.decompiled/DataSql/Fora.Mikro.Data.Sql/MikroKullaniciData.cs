using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.MikroKullanicilari;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MikroKullaniciData
{
	public static MikroKullanici GetMikroKullanici(SqlBaglantiBilgileri BaglantiBilgileri, string MikroAnaDBName, string User_name)
	{
		MikroKullanici mikroKullanici = new MikroKullanici();
		string commandText = (new SqlCommand().CommandText = "SELECT User_no,User_name,User_LongName,User_EMail FROM KULLANICILAR WITH (NOLOCK) WHERE User_name=@User_name");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, MikroAnaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@User_name", User_name);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					mikroKullanici.User_no = sqlDataReader.GetSafeInt32(0);
					mikroKullanici.User_name = sqlDataReader.GetSafeString(1);
					mikroKullanici.User_LongName = sqlDataReader.GetSafeString(2);
					mikroKullanici.User_EMail = sqlDataReader.GetSafeString(3);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return mikroKullanici;
	}

	public static MikroKullanici GetMikroKullanici(SqlBaglantiBilgileri BaglantiBilgileri, string MikroAnaDBName, int User_no)
	{
		MikroKullanici mikroKullanici = new MikroKullanici();
		string commandText = (new SqlCommand().CommandText = "SELECT User_no,User_name,User_LongName,User_EMail FROM KULLANICILAR WITH (NOLOCK) WHERE User_no=@User_no");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, MikroAnaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@User_no", User_no);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					mikroKullanici.User_no = sqlDataReader.GetSafeInt32(0);
					mikroKullanici.User_name = sqlDataReader.GetSafeString(1);
					mikroKullanici.User_LongName = sqlDataReader.GetSafeString(2);
					mikroKullanici.User_EMail = sqlDataReader.GetSafeString(3);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return mikroKullanici;
	}

	public static DataTable GetKullanicilarDataTable(SqlBaglantiBilgileri BaglantiBilgileri, string MikroAnaDBName)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, MikroAnaDBName);
		SqlCommand sqlCommand = new SqlCommand("SELECT User_no AS 'KULLANICI NO',User_name AS 'KULLANICI ADI' FROM KULLANICILAR WITH (NOLOCK)", sqlDB.Connection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "KULLANICILAR";
		sqlDB.ConnectionClose();
		return dataTable;
	}
}
