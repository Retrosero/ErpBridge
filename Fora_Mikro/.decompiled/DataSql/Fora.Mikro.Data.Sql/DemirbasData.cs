using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Demirbaslar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DemirbasData
{
	public static Demirbas GetDemirbas(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string dem_kod)
	{
		Demirbas demirbas = new Demirbas();
		string commandText = (new SqlCommand().CommandText = "SELECT dem_special1,dem_special2,dem_special3,dem_kod,dem_firmano,dem_subeno,dem_isim,dem_aciklama,dem_doviz_cinsi FROM DEMIRBASLAR WITH (NOLOCK) WHERE dem_kod=@dem_kod");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@dem_kod", dem_kod);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					demirbas.dem_special1 = sqlDataReader.GetSafeString(0);
					demirbas.dem_special2 = sqlDataReader.GetSafeString(1);
					demirbas.dem_special3 = sqlDataReader.GetSafeString(2);
					demirbas.dem_kod = sqlDataReader.GetSafeString(3);
					demirbas.dem_firmano = sqlDataReader.GetSafeInt32(4);
					demirbas.dem_subeno = sqlDataReader.GetSafeInt32(5);
					demirbas.dem_isim = sqlDataReader.GetSafeString(6);
					demirbas.dem_aciklama = sqlDataReader.GetSafeString(7);
					demirbas.dem_doviz_cinsi = sqlDataReader.GetSafeInt32(8);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return demirbas;
	}

	public static DataTable GetDemirbasDataTable(SqlConnection OpenedConnection)
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
		string text = "dem_RECno AS 'ID',";
		if (num > 15)
		{
			text = "dem_Guid AS 'ID',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "dem_kod AS 'KOD',dem_isim AS 'İSİM',dem_muh_kodu AS 'MUHASEBE KODU' FROM DEMIRBASLAR WITH (NOLOCK) ORDER BY dem_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "DEMIRBASLAR";
		return dataTable;
	}
}
