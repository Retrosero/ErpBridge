using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StokModelData
{
	public static string GetModelAdi(SqlConnection OpenedConnection, string mdl_kodu)
	{
		string commandText = "SELECT mdl_ismi FROM STOK_MODEL_TANIMLARI WHERE mdl_kodu=@mdl_kodu";
		string result = "";
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@mdl_kodu", mdl_kodu);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			result = sqlDataReader.GetSafeString(0);
		}
		sqlDataReader.Close();
		return result;
	}
}
