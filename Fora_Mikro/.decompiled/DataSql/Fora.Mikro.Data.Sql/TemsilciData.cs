using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql.Extensions;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TemsilciData
{
	public static string GetTemsilciAdi(SqlConnection connection, string temsilci_kodu)
	{
		string commandText = "SELECT cari_per_adi,cari_per_soyadi FROM CARI_PERSONEL_TANIMLARI WITH(NOLOCK) WHERE cari_per_kod=@temsilci_kodu";
		string result = "";
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@temsilci_kodu", temsilci_kodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				result = sqlDataReader.GetSafeString(0) + " " + sqlDataReader.GetSafeString(1);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return result;
	}

	public static List<CariRotaListItem> GetCariRotaListItems(SqlConnection connection, string temsilci_kodu, DateTime tarih)
	{
		int num = (int)tarih.DayOfWeek;
		if (num == 0)
		{
			num = 7;
		}
		string commandText = "SELECT adr_cari_kod,adr_adres_no,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_temsilci_kodu,adr_ozel_not,adr_gps_enlem,adr_gps_boylam,adr_yon_kodu,adr_uzaklik_kodu,adr_ziyaretperyodu,adr_ziyaretgunu,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7,cari_unvan1,cari_unvan2,cari_hareket_tipi FROM CARI_HESAP_ADRESLERI WITH(NOLOCK) INNER JOIN CARI_HESAPLAR WITH(NOLOCK) ON CARI_HESAPLAR.cari_kod=adr_cari_kod WHERE adr_temsilci_kodu=@adr_temsilci_kodu ORDER BY adr_yon_kodu,adr_uzaklik_kodu";
		List<CariRotaListItem> list = new List<CariRotaListItem>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.Connection = connection;
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@adr_temsilci_kodu", temsilci_kodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			bool flag = false;
			while (sqlDataReader.Read())
			{
				int safeByte = sqlDataReader.GetSafeByte(20);
				decimal num2 = (decimal)sqlDataReader.GetSafeDouble(21);
				sqlDataReader.GetSafeByte(22);
				bool safeBoolean = sqlDataReader.GetSafeBoolean(23);
				bool safeBoolean2 = sqlDataReader.GetSafeBoolean(24);
				bool safeBoolean3 = sqlDataReader.GetSafeBoolean(25);
				bool safeBoolean4 = sqlDataReader.GetSafeBoolean(26);
				bool safeBoolean5 = sqlDataReader.GetSafeBoolean(27);
				bool safeBoolean6 = sqlDataReader.GetSafeBoolean(28);
				bool safeBoolean7 = sqlDataReader.GetSafeBoolean(29);
				flag = false;
				switch (safeByte)
				{
				case 1:
					flag = true;
					break;
				case 2:
					if (num2 == (decimal)num)
					{
						flag = true;
					}
					break;
				case 4:
					if (num2 == (decimal)tarih.Day)
					{
						flag = true;
					}
					break;
				case 9:
					switch (num)
					{
					case 1:
						if (safeBoolean)
						{
							flag = true;
						}
						break;
					case 2:
						if (safeBoolean2)
						{
							flag = true;
						}
						break;
					case 3:
						if (safeBoolean3)
						{
							flag = true;
						}
						break;
					case 4:
						if (safeBoolean4)
						{
							flag = true;
						}
						break;
					case 5:
						if (safeBoolean5)
						{
							flag = true;
						}
						break;
					case 6:
						if (safeBoolean6)
						{
							flag = true;
						}
						break;
					case 7:
						if (safeBoolean7)
						{
							flag = true;
						}
						break;
					}
					break;
				case 10:
					switch (num)
					{
					case 1:
						if (safeBoolean)
						{
							flag = true;
						}
						break;
					case 2:
						if (safeBoolean2)
						{
							flag = true;
						}
						break;
					case 3:
						if (safeBoolean3)
						{
							flag = true;
						}
						break;
					case 4:
						if (safeBoolean4)
						{
							flag = true;
						}
						break;
					case 5:
						if (safeBoolean5)
						{
							flag = true;
						}
						break;
					case 6:
						if (safeBoolean6)
						{
							flag = true;
						}
						break;
					case 7:
						if (safeBoolean7)
						{
							flag = true;
						}
						break;
					}
					break;
				}
				if (flag)
				{
					CariRotaListItem cariRotaListItem = new CariRotaListItem();
					try
					{
						cariRotaListItem.CariAdres.adr_cari_kod = sqlDataReader[0].ToString();
						cariRotaListItem.cari_kod = sqlDataReader[0].ToString();
						cariRotaListItem.CariAdres.adr_adres_no = int.Parse(sqlDataReader[1].ToString());
						cariRotaListItem.CariAdres.adr_cadde = sqlDataReader[2].ToString();
						cariRotaListItem.CariAdres.adr_sokak = sqlDataReader[3].ToString();
						cariRotaListItem.CariAdres.adr_posta_kodu = sqlDataReader[4].ToString();
						cariRotaListItem.CariAdres.adr_ilce = sqlDataReader[5].ToString();
						cariRotaListItem.CariAdres.adr_il = sqlDataReader[6].ToString();
						cariRotaListItem.CariAdres.adr_ulke = sqlDataReader[7].ToString();
						cariRotaListItem.CariAdres.adr_tel_ulke_kodu = sqlDataReader[8].ToString();
						cariRotaListItem.CariAdres.adr_tel_bolge_kodu = sqlDataReader[9].ToString();
						cariRotaListItem.CariAdres.adr_tel_no1 = sqlDataReader[10].ToString();
						cariRotaListItem.CariAdres.adr_tel_no2 = sqlDataReader[11].ToString();
						cariRotaListItem.CariAdres.adr_tel_faxno = sqlDataReader[12].ToString();
						cariRotaListItem.CariAdres.adr_tel_modem = sqlDataReader[13].ToString();
						cariRotaListItem.CariAdres.adr_temsilci_kodu = sqlDataReader[14].ToString();
						cariRotaListItem.CariAdres.adr_ozel_not = sqlDataReader[15].ToString();
						cariRotaListItem.CariAdres.adr_gps_enlem = float.Parse(sqlDataReader[16].ToString());
						cariRotaListItem.CariAdres.adr_gps_boylam = float.Parse(sqlDataReader[17].ToString());
						cariRotaListItem.CariAdres.adr_yon_kodu = sqlDataReader[18].ToString();
						cariRotaListItem.CariAdres.adr_uzaklik_kodu = int.Parse(sqlDataReader[19].ToString());
						cariRotaListItem.CariAdres.adr_ziyaretperyodu = int.Parse(sqlDataReader[20].ToString());
						cariRotaListItem.CariAdres.adr_ziyaretgunu = sqlDataReader[21].ToString();
						cariRotaListItem.CariAdres.adr_ziyarethaftasi = int.Parse(sqlDataReader[22].ToString());
						cariRotaListItem.cari_unvan1 = sqlDataReader[30].ToString();
						cariRotaListItem.cari_unvan2 = sqlDataReader[31].ToString();
						cariRotaListItem.cari_hareket_tipi = (enum_cari_hareket_tipi)sqlDataReader.GetSafeByte(32);
					}
					catch
					{
					}
					list.Add(cariRotaListItem);
				}
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return list;
	}
}
