using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Ekipler;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EkipData
{
	public static Ekip GetEkip(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int ekp_RECno)
	{
		Ekip ekip = new Ekip();
		string commandText = (new SqlCommand().CommandText = "SELECT ekp_kodu,ekp_adi,ekp_cari_kodu,ekp_personel_kodu1,ekp_personel_agirlik_puan1,ekp_personel_kodu2,ekp_personel_agirlik_puan2,ekp_personel_kodu3,ekp_personel_agirlik_puan3,ekp_personel_kodu4,ekp_personel_agirlik_puan4,ekp_personel_kodu5,ekp_personel_agirlik_puan5,ekp_personel_kodu6,ekp_personel_agirlik_puan6,ekp_personel_kodu7,ekp_personel_agirlik_puan7,ekp_personel_kodu8,ekp_personel_agirlik_puan8,ekp_personel_kodu9,ekp_personel_agirlik_puan9,ekp_personel_kodu10,ekp_personel_agirlik_puan10 FROM EKIP_TANIMLARI WITH (NOLOCK) WHERE ekp_RECno=@ekp_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ekp_RECno", ekp_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					ekip.ekp_kodu = sqlDataReader.GetSafeString(0);
					ekip.ekp_adi = sqlDataReader.GetSafeString(1);
					ekip.ekp_cari_kodu = sqlDataReader.GetSafeString(2);
					ekip.ekp_personel_kodu1 = sqlDataReader.GetSafeString(3);
					ekip.ekp_personel_agirlik_puan1 = sqlDataReader.GetSafeDouble(4);
					ekip.ekp_personel_kodu2 = sqlDataReader.GetSafeString(5);
					ekip.ekp_personel_agirlik_puan2 = sqlDataReader.GetSafeDouble(6);
					ekip.ekp_personel_kodu3 = sqlDataReader.GetSafeString(7);
					ekip.ekp_personel_agirlik_puan3 = sqlDataReader.GetSafeDouble(8);
					ekip.ekp_personel_kodu4 = sqlDataReader.GetSafeString(9);
					ekip.ekp_personel_agirlik_puan4 = sqlDataReader.GetSafeDouble(10);
					ekip.ekp_personel_kodu5 = sqlDataReader.GetSafeString(11);
					ekip.ekp_personel_agirlik_puan5 = sqlDataReader.GetSafeDouble(12);
					ekip.ekp_personel_kodu6 = sqlDataReader.GetSafeString(13);
					ekip.ekp_personel_agirlik_puan6 = sqlDataReader.GetSafeDouble(14);
					ekip.ekp_personel_kodu7 = sqlDataReader.GetSafeString(15);
					ekip.ekp_personel_agirlik_puan7 = sqlDataReader.GetSafeDouble(16);
					ekip.ekp_personel_kodu8 = sqlDataReader.GetSafeString(17);
					ekip.ekp_personel_agirlik_puan8 = sqlDataReader.GetSafeDouble(18);
					ekip.ekp_personel_kodu9 = sqlDataReader.GetSafeString(19);
					ekip.ekp_personel_agirlik_puan9 = sqlDataReader.GetSafeDouble(20);
					ekip.ekp_personel_kodu10 = sqlDataReader.GetSafeString(21);
					ekip.ekp_personel_agirlik_puan10 = sqlDataReader.GetSafeDouble(22);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return ekip;
	}

	public static Ekip GetEkipByGuid(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid ekp_Guid)
	{
		Ekip ekip = new Ekip();
		string commandText = (new SqlCommand().CommandText = "SELECT ekp_kodu,ekp_adi,ekp_cari_kodu,ekp_personel_kodu1,ekp_personel_agirlik_puan1,ekp_personel_kodu2,ekp_personel_agirlik_puan2,ekp_personel_kodu3,ekp_personel_agirlik_puan3,ekp_personel_kodu4,ekp_personel_agirlik_puan4,ekp_personel_kodu5,ekp_personel_agirlik_puan5,ekp_personel_kodu6,ekp_personel_agirlik_puan6,ekp_personel_kodu7,ekp_personel_agirlik_puan7,ekp_personel_kodu8,ekp_personel_agirlik_puan8,ekp_personel_kodu9,ekp_personel_agirlik_puan9,ekp_personel_kodu10,ekp_personel_agirlik_puan10 FROM EKIP_TANIMLARI WITH (NOLOCK) WHERE ekp_Guid=@ekp_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ekp_Guid", ekp_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					ekip.ekp_kodu = sqlDataReader.GetSafeString(0);
					ekip.ekp_adi = sqlDataReader.GetSafeString(1);
					ekip.ekp_cari_kodu = sqlDataReader.GetSafeString(2);
					ekip.ekp_personel_kodu1 = sqlDataReader.GetSafeString(3);
					ekip.ekp_personel_agirlik_puan1 = sqlDataReader.GetSafeDouble(4);
					ekip.ekp_personel_kodu2 = sqlDataReader.GetSafeString(5);
					ekip.ekp_personel_agirlik_puan2 = sqlDataReader.GetSafeDouble(6);
					ekip.ekp_personel_kodu3 = sqlDataReader.GetSafeString(7);
					ekip.ekp_personel_agirlik_puan3 = sqlDataReader.GetSafeDouble(8);
					ekip.ekp_personel_kodu4 = sqlDataReader.GetSafeString(9);
					ekip.ekp_personel_agirlik_puan4 = sqlDataReader.GetSafeDouble(10);
					ekip.ekp_personel_kodu5 = sqlDataReader.GetSafeString(11);
					ekip.ekp_personel_agirlik_puan5 = sqlDataReader.GetSafeDouble(12);
					ekip.ekp_personel_kodu6 = sqlDataReader.GetSafeString(13);
					ekip.ekp_personel_agirlik_puan6 = sqlDataReader.GetSafeDouble(14);
					ekip.ekp_personel_kodu7 = sqlDataReader.GetSafeString(15);
					ekip.ekp_personel_agirlik_puan7 = sqlDataReader.GetSafeDouble(16);
					ekip.ekp_personel_kodu8 = sqlDataReader.GetSafeString(17);
					ekip.ekp_personel_agirlik_puan8 = sqlDataReader.GetSafeDouble(18);
					ekip.ekp_personel_kodu9 = sqlDataReader.GetSafeString(19);
					ekip.ekp_personel_agirlik_puan9 = sqlDataReader.GetSafeDouble(20);
					ekip.ekp_personel_kodu10 = sqlDataReader.GetSafeString(21);
					ekip.ekp_personel_agirlik_puan10 = sqlDataReader.GetSafeDouble(22);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return ekip;
	}

	public static Ekip GetEkip(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string ekp_kodu)
	{
		Ekip ekip = new Ekip();
		string commandText = (new SqlCommand().CommandText = "SELECT ekp_kodu,ekp_adi,ekp_cari_kodu,ekp_personel_kodu1,ekp_personel_agirlik_puan1,ekp_personel_kodu2,ekp_personel_agirlik_puan2,ekp_personel_kodu3,ekp_personel_agirlik_puan3,ekp_personel_kodu4,ekp_personel_agirlik_puan4,ekp_personel_kodu5,ekp_personel_agirlik_puan5,ekp_personel_kodu6,ekp_personel_agirlik_puan6,ekp_personel_kodu7,ekp_personel_agirlik_puan7,ekp_personel_kodu8,ekp_personel_agirlik_puan8,ekp_personel_kodu9,ekp_personel_agirlik_puan9,ekp_personel_kodu10,ekp_personel_agirlik_puan10 FROM EKIP_TANIMLARI WITH (NOLOCK) WHERE ekp_kodu=@ekp_kodu");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ekp_kodu", ekp_kodu);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					ekip.ekp_kodu = sqlDataReader.GetSafeString(0);
					ekip.ekp_adi = sqlDataReader.GetSafeString(1);
					ekip.ekp_cari_kodu = sqlDataReader.GetSafeString(2);
					ekip.ekp_personel_kodu1 = sqlDataReader.GetSafeString(3);
					ekip.ekp_personel_agirlik_puan1 = sqlDataReader.GetSafeDouble(4);
					ekip.ekp_personel_kodu2 = sqlDataReader.GetSafeString(5);
					ekip.ekp_personel_agirlik_puan2 = sqlDataReader.GetSafeDouble(6);
					ekip.ekp_personel_kodu3 = sqlDataReader.GetSafeString(7);
					ekip.ekp_personel_agirlik_puan3 = sqlDataReader.GetSafeDouble(8);
					ekip.ekp_personel_kodu4 = sqlDataReader.GetSafeString(9);
					ekip.ekp_personel_agirlik_puan4 = sqlDataReader.GetSafeDouble(10);
					ekip.ekp_personel_kodu5 = sqlDataReader.GetSafeString(11);
					ekip.ekp_personel_agirlik_puan5 = sqlDataReader.GetSafeDouble(12);
					ekip.ekp_personel_kodu6 = sqlDataReader.GetSafeString(13);
					ekip.ekp_personel_agirlik_puan6 = sqlDataReader.GetSafeDouble(14);
					ekip.ekp_personel_kodu7 = sqlDataReader.GetSafeString(15);
					ekip.ekp_personel_agirlik_puan7 = sqlDataReader.GetSafeDouble(16);
					ekip.ekp_personel_kodu8 = sqlDataReader.GetSafeString(17);
					ekip.ekp_personel_agirlik_puan8 = sqlDataReader.GetSafeDouble(18);
					ekip.ekp_personel_kodu9 = sqlDataReader.GetSafeString(19);
					ekip.ekp_personel_agirlik_puan9 = sqlDataReader.GetSafeDouble(20);
					ekip.ekp_personel_kodu10 = sqlDataReader.GetSafeString(21);
					ekip.ekp_personel_agirlik_puan10 = sqlDataReader.GetSafeDouble(22);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return ekip;
	}

	public static Ekip GetEkipByAdi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string ekp_adi)
	{
		Ekip ekip = new Ekip();
		string commandText = (new SqlCommand().CommandText = "SELECT ekp_kodu,ekp_adi,ekp_cari_kodu,ekp_personel_kodu1,ekp_personel_agirlik_puan1,ekp_personel_kodu2,ekp_personel_agirlik_puan2,ekp_personel_kodu3,ekp_personel_agirlik_puan3,ekp_personel_kodu4,ekp_personel_agirlik_puan4,ekp_personel_kodu5,ekp_personel_agirlik_puan5,ekp_personel_kodu6,ekp_personel_agirlik_puan6,ekp_personel_kodu7,ekp_personel_agirlik_puan7,ekp_personel_kodu8,ekp_personel_agirlik_puan8,ekp_personel_kodu9,ekp_personel_agirlik_puan9,ekp_personel_kodu10,ekp_personel_agirlik_puan10 FROM EKIP_TANIMLARI WITH (NOLOCK) WHERE ekp_adi=@ekp_adi");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ekp_adi", ekp_adi);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					ekip.ekp_kodu = sqlDataReader.GetSafeString(0);
					ekip.ekp_adi = sqlDataReader.GetSafeString(1);
					ekip.ekp_cari_kodu = sqlDataReader.GetSafeString(2);
					ekip.ekp_personel_kodu1 = sqlDataReader.GetSafeString(3);
					ekip.ekp_personel_agirlik_puan1 = sqlDataReader.GetSafeDouble(4);
					ekip.ekp_personel_kodu2 = sqlDataReader.GetSafeString(5);
					ekip.ekp_personel_agirlik_puan2 = sqlDataReader.GetSafeDouble(6);
					ekip.ekp_personel_kodu3 = sqlDataReader.GetSafeString(7);
					ekip.ekp_personel_agirlik_puan3 = sqlDataReader.GetSafeDouble(8);
					ekip.ekp_personel_kodu4 = sqlDataReader.GetSafeString(9);
					ekip.ekp_personel_agirlik_puan4 = sqlDataReader.GetSafeDouble(10);
					ekip.ekp_personel_kodu5 = sqlDataReader.GetSafeString(11);
					ekip.ekp_personel_agirlik_puan5 = sqlDataReader.GetSafeDouble(12);
					ekip.ekp_personel_kodu6 = sqlDataReader.GetSafeString(13);
					ekip.ekp_personel_agirlik_puan6 = sqlDataReader.GetSafeDouble(14);
					ekip.ekp_personel_kodu7 = sqlDataReader.GetSafeString(15);
					ekip.ekp_personel_agirlik_puan7 = sqlDataReader.GetSafeDouble(16);
					ekip.ekp_personel_kodu8 = sqlDataReader.GetSafeString(17);
					ekip.ekp_personel_agirlik_puan8 = sqlDataReader.GetSafeDouble(18);
					ekip.ekp_personel_kodu9 = sqlDataReader.GetSafeString(19);
					ekip.ekp_personel_agirlik_puan9 = sqlDataReader.GetSafeDouble(20);
					ekip.ekp_personel_kodu10 = sqlDataReader.GetSafeString(21);
					ekip.ekp_personel_agirlik_puan10 = sqlDataReader.GetSafeDouble(22);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return ekip;
	}
}
