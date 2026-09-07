using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Subeler;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SubeData
{
	public static Sube GetSube(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int subeno)
	{
		Sube sube = new Sube();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		sube = GetSube(sqlDB.Connection, subeno);
		sqlDB.ConnectionClose();
		return sube;
	}

	public static Sube GetSube(SqlConnection openedconnection, int subeno)
	{
		string commandText = "SELECT Sube_no,Sube_adi FROM SUBELER WITH(NOLOCK) WHERE Sube_no=@Sube_no";
		Sube sube = new Sube();
		using SqlCommand sqlCommand = openedconnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@Sube_no", subeno);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			sube.Sube_no = sqlDataReader.GetSafeInt32(0);
			sube.Sube_adi = sqlDataReader.GetSafeString(1);
		}
		sqlDataReader.Close();
		return sube;
	}
}
