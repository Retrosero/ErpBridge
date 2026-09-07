using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Cihazlar;
using Fora.Mikro.Data.Sql.Extensions;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CihazGruplariData
{
	public static List<CihazGruplari> GetCihazGruplari(SqlConnection OpenedConnection, string tuketicikodu)
	{
		List<CihazGruplari> list = new List<CihazGruplari>();
		string commandText = (new SqlCommand().CommandText = "SELECT cg_RECno,cg_kodu,cg_aciklama FROM CIHAZ_GRUPLARI WITH (NOLOCK) WHERE cg_kodu in (SELECT chz_grup_kodu FROM STOK_SERINO_TANIMLARI WITH (NOLOCK) WHERE chz_Tuktckodu=@tuketicikodu)");
		try
		{
			using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@tuketicikodu", tuketicikodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				CihazGruplari cihazGruplari = new CihazGruplari();
				cihazGruplari.cg_RECno = sqlDataReader.GetSafeInt32(0);
				cihazGruplari.cg_kodu = sqlDataReader.GetSafeString(1);
				cihazGruplari.cg_aciklama = sqlDataReader.GetSafeString(2);
				list.Add(cihazGruplari);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return list;
	}
}
