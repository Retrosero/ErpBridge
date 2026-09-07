using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DovizCinsiTanimlariData
{
	public static DovizCinsiTanimlari GetDovizCinsiTanimlari(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		DovizCinsiTanimlari dovizCinsiTanimlari = new DovizCinsiTanimlari();
		string commandText = (new SqlCommand().CommandText = "SELECT Kur_No ,Kur_Tip,Kur_sembol,Kur_adi,Kur_orjAdi,Kur_kusurat_isim,Kur_decimal,Kur_kusurat_sembol FROM KUR_ISIMLERI WITH (NOLOCK)");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					int safeByte = sqlDataReader.GetSafeByte(0);
					dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_Tip = sqlDataReader.GetSafeString(1);
					dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_sembol = sqlDataReader.GetSafeString(2);
					dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_adi = sqlDataReader.GetSafeString(3);
					dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_orjAdi = sqlDataReader.GetSafeString(4);
					dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_kusurat_isim = sqlDataReader.GetSafeString(5);
					dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_decimal = sqlDataReader.GetSafeByte(6);
					dovizCinsiTanimlari.GetDovizCinsiTanimi(safeByte).Kur_kusurat_sembol = sqlDataReader.GetSafeString(7);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return dovizCinsiTanimlari;
	}
}
