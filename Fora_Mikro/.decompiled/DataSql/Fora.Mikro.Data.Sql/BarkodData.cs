using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Barkodlar;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BarkodData
{
	public static Barkod GetBarkodBilgisi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string Barkod)
	{
		Barkod barkod = new Barkod();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		SqlCommand sqlCommand = new SqlCommand("SELECT bar_kodu,bar_stokkodu,bar_partikodu,bar_lotno,bar_serino_veya_bagkodu,bar_barkodtipi,bar_icerigi,bar_birimpntr,bar_master,bar_bedenpntr,bar_renkpntr,bar_baglantitipi FROM BARKOD_TANIMLARI WITH(NOLOCK) WHERE bar_kodu=@Barkod");
		sqlCommand.Parameters.AddWithValue("@Barkod", Barkod);
		SqlDataReader sqlDataReader = sqlDB.ExecuteReader(sqlCommand);
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				barkod.bar_kodu = sqlDataReader.GetSafeString(0);
				barkod.bar_stokkodu = sqlDataReader.GetSafeString(1);
				barkod.bar_partikodu = sqlDataReader.GetSafeString(2);
				barkod.bar_lotno = sqlDataReader.GetSafeInt32(3);
				barkod.bar_serino_veya_bagkodu = sqlDataReader.GetSafeString(4);
				barkod.bar_barkodtipi = sqlDataReader.GetSafeByte(5);
				barkod.bar_icerigi = sqlDataReader.GetSafeByte(6);
				barkod.bar_birimpntr = sqlDataReader.GetSafeByte(7);
				barkod.bar_master = sqlDataReader.GetSafeBoolean(8);
				barkod.bar_bedenpntr = sqlDataReader.GetSafeByte(9);
				barkod.bar_renkpntr = sqlDataReader.GetSafeByte(10);
				barkod.bar_baglantitipi = sqlDataReader.GetSafeByte(11);
			}
		}
		return barkod;
	}
}
