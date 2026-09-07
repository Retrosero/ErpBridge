using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Firmalar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FirmaData
{
	public static Firma GetFirma(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int firmano)
	{
		Firma firma = new Firma();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		firma = GetFirma(sqlDB.Connection, firmano);
		sqlDB.ConnectionClose();
		return firma;
	}

	public static Firma GetFirma(SqlConnection openedconnection, int firmano)
	{
		string commandText = "SELECT fir_sirano,fir_unvan FROM FIRMALAR WITH(NOLOCK) WHERE fir_sirano=@fir_sirano";
		Firma firma = new Firma();
		using SqlCommand sqlCommand = openedconnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@fir_sirano", firmano);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			firma.fir_sirano = int.Parse(sqlDataReader[0].ToString());
			firma.fir_unvan = sqlDataReader[1].ToString();
		}
		sqlDataReader.Close();
		return firma;
	}
}
