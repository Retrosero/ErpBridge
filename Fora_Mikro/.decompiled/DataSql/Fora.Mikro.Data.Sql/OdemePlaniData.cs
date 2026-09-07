using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.OdemePlanlari;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct OdemePlaniData
{
	public static OdemePlani GetOdemePlani(SqlConnection connection, int odp_no)
	{
		string commandText = "SELECT odp_no,odp_kodu,odp_adi FROM ODEME_PLANLARI WITH(NOLOCK) WHERE odp_no=@odp_no";
		OdemePlani odemePlani = new OdemePlani();
		if (odp_no < 0)
		{
			odemePlani.odp_adi = odp_no.ToString().Remove(0, 1) + " GÜN";
			odemePlani.odp_no = odp_no;
			odemePlani.odp_kodu = "";
		}
		if (odp_no == 0)
		{
			odemePlani.odp_adi = "PEŞİN";
			odemePlani.odp_no = odp_no;
			odemePlani.odp_kodu = "";
		}
		if (odp_no > 0)
		{
			try
			{
				using SqlCommand sqlCommand = connection.CreateCommand();
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@odp_no", odp_no);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					odemePlani.odp_no = sqlDataReader.GetSafeInt32(0);
					odemePlani.odp_kodu = sqlDataReader.GetSafeString(1);
					odemePlani.odp_adi = sqlDataReader.GetSafeString(2);
				}
				sqlDataReader.Close();
			}
			catch
			{
			}
		}
		return odemePlani;
	}

	private static List<GenelList> GetOdemePlaniGenelList(SqlConnection connection)
	{
		string commandText = "SELECT odp_no,odp_adi FROM ODEME_PLANLARI WITH(NOLOCK) ORDER BY odp_adi";
		List<GenelList> list = new List<GenelList>();
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			sqlCommand.CommandText = commandText;
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeInt32(0).ToString();
				genelList.Text = sqlDataReader.GetSafeString(1);
				list.Add(genelList);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return list;
	}

	public static DataTable GetOdemePlanlariDataTable(SqlConnection OpenedConnection)
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
		string text = "odp_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "odp_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "odp_no AS 'ÖDEME PLAN NO',odp_kodu AS 'ÖDEME PLANI KODU',odp_adi AS 'ÖDEME PLANI ADI' FROM ODEME_PLANLARI WITH (NOLOCK) ORDER BY odp_no", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "ODEME_PLANLARI";
		return dataTable;
	}
}
