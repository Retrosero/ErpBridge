using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;
using Fora.Mikro.Veritabanlari;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct VeritabaniData
{
	public static Veritabani GetVeritabani(SqlBaglantiBilgileri BaglantiBilgileri, string MikroAnaDBName, string DB_kod)
	{
		Veritabani veritabani = new Veritabani();
		string commandText = (new SqlCommand().CommandText = "SELECT DB_kod,DB_isim,DB_yerli_doviz_cins,DB_alternatif_doviz FROM VERI_TABANLARI WITH (NOLOCK) WHERE DB_kod=@DB_kod");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, MikroAnaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@DB_kod", DB_kod);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					veritabani.DB_kod = sqlDataReader.GetSafeString(0);
					veritabani.DB_isim = sqlDataReader.GetSafeString(1);
					veritabani.DB_yerli_doviz_cins = sqlDataReader.GetSafeByte(2);
					veritabani.DB_alternatif_doviz = sqlDataReader.GetSafeByte(3);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return veritabani;
	}
}
