using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Stoklar.SonKullanicilar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SonKullaniciData
{
	public static SonKullanici GetSonKullanici(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int tuk_RECno)
	{
		SonKullanici sonKullanici = new SonKullanici();
		string commandText = (new SqlCommand().CommandText = "SELECT tuk_RECno,tuk_kodu,tuk_ismi,tuk_Adr1_Cadde,tuk_Adr1_Sokak,tuk_Adr1_Postakodu,tuk_Adr1_Ilce,tuk_Adr1_Il,tuk_Adr1_Ulke,tuk_Adr2_Cadde,tuk_Adr2_Sokak,tuk_Adr2_Postakodu,tuk_Adr2_Ilce,tuk_Adr2_Il,tuk_Adr2_Ulke,tuk_Tel1_Ulkekod,tuk_Tel1_Bolgekod,tuk_Tel1_TelNo1,tuk_Tel1_TelNo2,tuk_Tel1_FaxNo,tuk_Tel1_ModemNo,tuk_Tel2_Ulkekod,tuk_Tel2_Bolgekod,tuk_Tel2_TelNo1,tuk_Tel2_TelNo2,tuk_Tel2_FaxNo,tuk_Tel2_ModemNo,tuk_yetkili1,tuk_yetkili2,tuk_ceptel1,tuk_ceptel2,tuk_email1,tuk_email2,tuk_GrpKodu,tuk_MuhKodu,tuk_kilitli_flg,tuk_cari_kodu,tuk_sektor_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WITH (NOLOCK) WHERE tuk_RECno=@tuk_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@tuk_RECno", tuk_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					sonKullanici.tuk_RECno = sqlDataReader.GetSafeInt32(0);
					sonKullanici.tuk_kodu = sqlDataReader.GetSafeString(1);
					sonKullanici.tuk_ismi = sqlDataReader.GetSafeString(2);
					sonKullanici.tuk_Adr1_Cadde = sqlDataReader.GetSafeString(3);
					sonKullanici.tuk_Adr1_Sokak = sqlDataReader.GetSafeString(4);
					sonKullanici.tuk_Adr1_Postakodu = sqlDataReader.GetSafeString(5);
					sonKullanici.tuk_Adr1_Ilce = sqlDataReader.GetSafeString(6);
					sonKullanici.tuk_Adr1_Il = sqlDataReader.GetSafeString(7);
					sonKullanici.tuk_Adr1_Ulke = sqlDataReader.GetSafeString(8);
					sonKullanici.tuk_Adr2_Cadde = sqlDataReader.GetSafeString(9);
					sonKullanici.tuk_Adr2_Sokak = sqlDataReader.GetSafeString(10);
					sonKullanici.tuk_Adr2_Postakodu = sqlDataReader.GetSafeString(11);
					sonKullanici.tuk_Adr2_Ilce = sqlDataReader.GetSafeString(12);
					sonKullanici.tuk_Adr2_Il = sqlDataReader.GetSafeString(13);
					sonKullanici.tuk_Adr2_Ulke = sqlDataReader.GetSafeString(14);
					sonKullanici.tuk_Tel1_Ulkekod = sqlDataReader.GetSafeString(15);
					sonKullanici.tuk_Tel1_Bolgekod = sqlDataReader.GetSafeString(16);
					sonKullanici.tuk_Tel1_TelNo1 = sqlDataReader.GetSafeString(17);
					sonKullanici.tuk_Tel1_TelNo2 = sqlDataReader.GetSafeString(18);
					sonKullanici.tuk_Tel1_FaxNo = sqlDataReader.GetSafeString(19);
					sonKullanici.tuk_Tel1_ModemNo = sqlDataReader.GetSafeString(20);
					sonKullanici.tuk_Tel2_Ulkekod = sqlDataReader.GetSafeString(21);
					sonKullanici.tuk_Tel2_Bolgekod = sqlDataReader.GetSafeString(22);
					sonKullanici.tuk_Tel2_TelNo1 = sqlDataReader.GetSafeString(23);
					sonKullanici.tuk_Tel2_TelNo2 = sqlDataReader.GetSafeString(24);
					sonKullanici.tuk_Tel2_FaxNo = sqlDataReader.GetSafeString(25);
					sonKullanici.tuk_Tel2_ModemNo = sqlDataReader.GetSafeString(26);
					sonKullanici.tuk_yetkili1 = sqlDataReader.GetSafeString(27);
					sonKullanici.tuk_yetkili2 = sqlDataReader.GetSafeString(28);
					sonKullanici.tuk_ceptel1 = sqlDataReader.GetSafeString(29);
					sonKullanici.tuk_ceptel2 = sqlDataReader.GetSafeString(30);
					sonKullanici.tuk_email1 = sqlDataReader.GetSafeString(31);
					sonKullanici.tuk_email2 = sqlDataReader.GetSafeString(32);
					sonKullanici.tuk_GrpKodu = sqlDataReader.GetSafeString(33);
					sonKullanici.tuk_MuhKodu = sqlDataReader.GetSafeString(34);
					sonKullanici.tuk_kilitli_flg = sqlDataReader.GetSafeBoolean(35);
					sonKullanici.tuk_cari_kodu = sqlDataReader.GetSafeString(36);
					sonKullanici.tuk_sektor_kodu = sqlDataReader.GetSafeString(37);
					sonKullanici.tuk_bolge_kodu = sqlDataReader.GetSafeString(38);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return sonKullanici;
	}

	public static SonKullanici GetSonKullanici(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid tuk_Guid)
	{
		SonKullanici sonKullanici = new SonKullanici();
		string commandText = (new SqlCommand().CommandText = "SELECT 0,tuk_kodu,tuk_ismi,tuk_Adr1_Cadde,tuk_Adr1_Sokak,tuk_Adr1_Postakodu,tuk_Adr1_Ilce,tuk_Adr1_Il,tuk_Adr1_Ulke,tuk_Adr2_Cadde,tuk_Adr2_Sokak,tuk_Adr2_Postakodu,tuk_Adr2_Ilce,tuk_Adr2_Il,tuk_Adr2_Ulke,tuk_Tel1_Ulkekod,tuk_Tel1_Bolgekod,tuk_Tel1_TelNo1,tuk_Tel1_TelNo2,tuk_Tel1_FaxNo,tuk_Tel1_ModemNo,tuk_Tel2_Ulkekod,tuk_Tel2_Bolgekod,tuk_Tel2_TelNo1,tuk_Tel2_TelNo2,tuk_Tel2_FaxNo,tuk_Tel2_ModemNo,tuk_yetkili1,tuk_yetkili2,tuk_ceptel1,tuk_ceptel2,tuk_email1,tuk_email2,tuk_GrpKodu,tuk_MuhKodu,tuk_kilitli_flg,tuk_cari_kodu,tuk_sektor_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WITH (NOLOCK) WHERE tuk_Guid=@tuk_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@tuk_Guid", tuk_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					sonKullanici.tuk_RECno = sqlDataReader.GetSafeInt32(0);
					sonKullanici.tuk_kodu = sqlDataReader.GetSafeString(1);
					sonKullanici.tuk_ismi = sqlDataReader.GetSafeString(2);
					sonKullanici.tuk_Adr1_Cadde = sqlDataReader.GetSafeString(3);
					sonKullanici.tuk_Adr1_Sokak = sqlDataReader.GetSafeString(4);
					sonKullanici.tuk_Adr1_Postakodu = sqlDataReader.GetSafeString(5);
					sonKullanici.tuk_Adr1_Ilce = sqlDataReader.GetSafeString(6);
					sonKullanici.tuk_Adr1_Il = sqlDataReader.GetSafeString(7);
					sonKullanici.tuk_Adr1_Ulke = sqlDataReader.GetSafeString(8);
					sonKullanici.tuk_Adr2_Cadde = sqlDataReader.GetSafeString(9);
					sonKullanici.tuk_Adr2_Sokak = sqlDataReader.GetSafeString(10);
					sonKullanici.tuk_Adr2_Postakodu = sqlDataReader.GetSafeString(11);
					sonKullanici.tuk_Adr2_Ilce = sqlDataReader.GetSafeString(12);
					sonKullanici.tuk_Adr2_Il = sqlDataReader.GetSafeString(13);
					sonKullanici.tuk_Adr2_Ulke = sqlDataReader.GetSafeString(14);
					sonKullanici.tuk_Tel1_Ulkekod = sqlDataReader.GetSafeString(15);
					sonKullanici.tuk_Tel1_Bolgekod = sqlDataReader.GetSafeString(16);
					sonKullanici.tuk_Tel1_TelNo1 = sqlDataReader.GetSafeString(17);
					sonKullanici.tuk_Tel1_TelNo2 = sqlDataReader.GetSafeString(18);
					sonKullanici.tuk_Tel1_FaxNo = sqlDataReader.GetSafeString(19);
					sonKullanici.tuk_Tel1_ModemNo = sqlDataReader.GetSafeString(20);
					sonKullanici.tuk_Tel2_Ulkekod = sqlDataReader.GetSafeString(21);
					sonKullanici.tuk_Tel2_Bolgekod = sqlDataReader.GetSafeString(22);
					sonKullanici.tuk_Tel2_TelNo1 = sqlDataReader.GetSafeString(23);
					sonKullanici.tuk_Tel2_TelNo2 = sqlDataReader.GetSafeString(24);
					sonKullanici.tuk_Tel2_FaxNo = sqlDataReader.GetSafeString(25);
					sonKullanici.tuk_Tel2_ModemNo = sqlDataReader.GetSafeString(26);
					sonKullanici.tuk_yetkili1 = sqlDataReader.GetSafeString(27);
					sonKullanici.tuk_yetkili2 = sqlDataReader.GetSafeString(28);
					sonKullanici.tuk_ceptel1 = sqlDataReader.GetSafeString(29);
					sonKullanici.tuk_ceptel2 = sqlDataReader.GetSafeString(30);
					sonKullanici.tuk_email1 = sqlDataReader.GetSafeString(31);
					sonKullanici.tuk_email2 = sqlDataReader.GetSafeString(32);
					sonKullanici.tuk_GrpKodu = sqlDataReader.GetSafeString(33);
					sonKullanici.tuk_MuhKodu = sqlDataReader.GetSafeString(34);
					sonKullanici.tuk_kilitli_flg = sqlDataReader.GetSafeBoolean(35);
					sonKullanici.tuk_cari_kodu = sqlDataReader.GetSafeString(36);
					sonKullanici.tuk_sektor_kodu = sqlDataReader.GetSafeString(37);
					sonKullanici.tuk_bolge_kodu = sqlDataReader.GetSafeString(38);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return sonKullanici;
	}

	public static SonKullanici GetSonKullanici(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string tuk_kodu)
	{
		SonKullanici sonKullanici = new SonKullanici();
		string commandText = (new SqlCommand().CommandText = "SELECT 0,tuk_kodu,tuk_ismi,tuk_Adr1_Cadde,tuk_Adr1_Sokak,tuk_Adr1_Postakodu,tuk_Adr1_Ilce,tuk_Adr1_Il,tuk_Adr1_Ulke,tuk_Adr2_Cadde,tuk_Adr2_Sokak,tuk_Adr2_Postakodu,tuk_Adr2_Ilce,tuk_Adr2_Il,tuk_Adr2_Ulke,tuk_Tel1_Ulkekod,tuk_Tel1_Bolgekod,tuk_Tel1_TelNo1,tuk_Tel1_TelNo2,tuk_Tel1_FaxNo,tuk_Tel1_ModemNo,tuk_Tel2_Ulkekod,tuk_Tel2_Bolgekod,tuk_Tel2_TelNo1,tuk_Tel2_TelNo2,tuk_Tel2_FaxNo,tuk_Tel2_ModemNo,tuk_yetkili1,tuk_yetkili2,tuk_ceptel1,tuk_ceptel2,tuk_email1,tuk_email2,tuk_GrpKodu,tuk_MuhKodu,tuk_kilitli_flg,tuk_cari_kodu,tuk_sektor_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WITH (NOLOCK) WHERE tuk_kodu=@tuk_kodu");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@tuk_kodu", tuk_kodu);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					sonKullanici.tuk_RECno = sqlDataReader.GetSafeInt32(0);
					sonKullanici.tuk_kodu = sqlDataReader.GetSafeString(1);
					sonKullanici.tuk_ismi = sqlDataReader.GetSafeString(2);
					sonKullanici.tuk_Adr1_Cadde = sqlDataReader.GetSafeString(3);
					sonKullanici.tuk_Adr1_Sokak = sqlDataReader.GetSafeString(4);
					sonKullanici.tuk_Adr1_Postakodu = sqlDataReader.GetSafeString(5);
					sonKullanici.tuk_Adr1_Ilce = sqlDataReader.GetSafeString(6);
					sonKullanici.tuk_Adr1_Il = sqlDataReader.GetSafeString(7);
					sonKullanici.tuk_Adr1_Ulke = sqlDataReader.GetSafeString(8);
					sonKullanici.tuk_Adr2_Cadde = sqlDataReader.GetSafeString(9);
					sonKullanici.tuk_Adr2_Sokak = sqlDataReader.GetSafeString(10);
					sonKullanici.tuk_Adr2_Postakodu = sqlDataReader.GetSafeString(11);
					sonKullanici.tuk_Adr2_Ilce = sqlDataReader.GetSafeString(12);
					sonKullanici.tuk_Adr2_Il = sqlDataReader.GetSafeString(13);
					sonKullanici.tuk_Adr2_Ulke = sqlDataReader.GetSafeString(14);
					sonKullanici.tuk_Tel1_Ulkekod = sqlDataReader.GetSafeString(15);
					sonKullanici.tuk_Tel1_Bolgekod = sqlDataReader.GetSafeString(16);
					sonKullanici.tuk_Tel1_TelNo1 = sqlDataReader.GetSafeString(17);
					sonKullanici.tuk_Tel1_TelNo2 = sqlDataReader.GetSafeString(18);
					sonKullanici.tuk_Tel1_FaxNo = sqlDataReader.GetSafeString(19);
					sonKullanici.tuk_Tel1_ModemNo = sqlDataReader.GetSafeString(20);
					sonKullanici.tuk_Tel2_Ulkekod = sqlDataReader.GetSafeString(21);
					sonKullanici.tuk_Tel2_Bolgekod = sqlDataReader.GetSafeString(22);
					sonKullanici.tuk_Tel2_TelNo1 = sqlDataReader.GetSafeString(23);
					sonKullanici.tuk_Tel2_TelNo2 = sqlDataReader.GetSafeString(24);
					sonKullanici.tuk_Tel2_FaxNo = sqlDataReader.GetSafeString(25);
					sonKullanici.tuk_Tel2_ModemNo = sqlDataReader.GetSafeString(26);
					sonKullanici.tuk_yetkili1 = sqlDataReader.GetSafeString(27);
					sonKullanici.tuk_yetkili2 = sqlDataReader.GetSafeString(28);
					sonKullanici.tuk_ceptel1 = sqlDataReader.GetSafeString(29);
					sonKullanici.tuk_ceptel2 = sqlDataReader.GetSafeString(30);
					sonKullanici.tuk_email1 = sqlDataReader.GetSafeString(31);
					sonKullanici.tuk_email2 = sqlDataReader.GetSafeString(32);
					sonKullanici.tuk_GrpKodu = sqlDataReader.GetSafeString(33);
					sonKullanici.tuk_MuhKodu = sqlDataReader.GetSafeString(34);
					sonKullanici.tuk_kilitli_flg = sqlDataReader.GetSafeBoolean(35);
					sonKullanici.tuk_cari_kodu = sqlDataReader.GetSafeString(36);
					sonKullanici.tuk_sektor_kodu = sqlDataReader.GetSafeString(37);
					sonKullanici.tuk_bolge_kodu = sqlDataReader.GetSafeString(38);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return sonKullanici;
	}

	public static SonKullanici GetSonKullaniciByIsmi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string tuk_ismi)
	{
		SonKullanici sonKullanici = new SonKullanici();
		string commandText = (new SqlCommand().CommandText = "SELECT 0,tuk_kodu,tuk_ismi,tuk_Adr1_Cadde,tuk_Adr1_Sokak,tuk_Adr1_Postakodu,tuk_Adr1_Ilce,tuk_Adr1_Il,tuk_Adr1_Ulke,tuk_Adr2_Cadde,tuk_Adr2_Sokak,tuk_Adr2_Postakodu,tuk_Adr2_Ilce,tuk_Adr2_Il,tuk_Adr2_Ulke,tuk_Tel1_Ulkekod,tuk_Tel1_Bolgekod,tuk_Tel1_TelNo1,tuk_Tel1_TelNo2,tuk_Tel1_FaxNo,tuk_Tel1_ModemNo,tuk_Tel2_Ulkekod,tuk_Tel2_Bolgekod,tuk_Tel2_TelNo1,tuk_Tel2_TelNo2,tuk_Tel2_FaxNo,tuk_Tel2_ModemNo,tuk_yetkili1,tuk_yetkili2,tuk_ceptel1,tuk_ceptel2,tuk_email1,tuk_email2,tuk_GrpKodu,tuk_MuhKodu,tuk_kilitli_flg,tuk_cari_kodu,tuk_sektor_kodu,tuk_bolge_kodu FROM SON_KULLANICILAR WITH (NOLOCK) WHERE tuk_ismi=@tuk_ismi");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@tuk_ismi", tuk_ismi);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					sonKullanici.tuk_RECno = sqlDataReader.GetSafeInt32(0);
					sonKullanici.tuk_kodu = sqlDataReader.GetSafeString(1);
					sonKullanici.tuk_ismi = sqlDataReader.GetSafeString(2);
					sonKullanici.tuk_Adr1_Cadde = sqlDataReader.GetSafeString(3);
					sonKullanici.tuk_Adr1_Sokak = sqlDataReader.GetSafeString(4);
					sonKullanici.tuk_Adr1_Postakodu = sqlDataReader.GetSafeString(5);
					sonKullanici.tuk_Adr1_Ilce = sqlDataReader.GetSafeString(6);
					sonKullanici.tuk_Adr1_Il = sqlDataReader.GetSafeString(7);
					sonKullanici.tuk_Adr1_Ulke = sqlDataReader.GetSafeString(8);
					sonKullanici.tuk_Adr2_Cadde = sqlDataReader.GetSafeString(9);
					sonKullanici.tuk_Adr2_Sokak = sqlDataReader.GetSafeString(10);
					sonKullanici.tuk_Adr2_Postakodu = sqlDataReader.GetSafeString(11);
					sonKullanici.tuk_Adr2_Ilce = sqlDataReader.GetSafeString(12);
					sonKullanici.tuk_Adr2_Il = sqlDataReader.GetSafeString(13);
					sonKullanici.tuk_Adr2_Ulke = sqlDataReader.GetSafeString(14);
					sonKullanici.tuk_Tel1_Ulkekod = sqlDataReader.GetSafeString(15);
					sonKullanici.tuk_Tel1_Bolgekod = sqlDataReader.GetSafeString(16);
					sonKullanici.tuk_Tel1_TelNo1 = sqlDataReader.GetSafeString(17);
					sonKullanici.tuk_Tel1_TelNo2 = sqlDataReader.GetSafeString(18);
					sonKullanici.tuk_Tel1_FaxNo = sqlDataReader.GetSafeString(19);
					sonKullanici.tuk_Tel1_ModemNo = sqlDataReader.GetSafeString(20);
					sonKullanici.tuk_Tel2_Ulkekod = sqlDataReader.GetSafeString(21);
					sonKullanici.tuk_Tel2_Bolgekod = sqlDataReader.GetSafeString(22);
					sonKullanici.tuk_Tel2_TelNo1 = sqlDataReader.GetSafeString(23);
					sonKullanici.tuk_Tel2_TelNo2 = sqlDataReader.GetSafeString(24);
					sonKullanici.tuk_Tel2_FaxNo = sqlDataReader.GetSafeString(25);
					sonKullanici.tuk_Tel2_ModemNo = sqlDataReader.GetSafeString(26);
					sonKullanici.tuk_yetkili1 = sqlDataReader.GetSafeString(27);
					sonKullanici.tuk_yetkili2 = sqlDataReader.GetSafeString(28);
					sonKullanici.tuk_ceptel1 = sqlDataReader.GetSafeString(29);
					sonKullanici.tuk_ceptel2 = sqlDataReader.GetSafeString(30);
					sonKullanici.tuk_email1 = sqlDataReader.GetSafeString(31);
					sonKullanici.tuk_email2 = sqlDataReader.GetSafeString(32);
					sonKullanici.tuk_GrpKodu = sqlDataReader.GetSafeString(33);
					sonKullanici.tuk_MuhKodu = sqlDataReader.GetSafeString(34);
					sonKullanici.tuk_kilitli_flg = sqlDataReader.GetSafeBoolean(35);
					sonKullanici.tuk_cari_kodu = sqlDataReader.GetSafeString(36);
					sonKullanici.tuk_sektor_kodu = sqlDataReader.GetSafeString(37);
					sonKullanici.tuk_bolge_kodu = sqlDataReader.GetSafeString(38);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return sonKullanici;
	}
}
