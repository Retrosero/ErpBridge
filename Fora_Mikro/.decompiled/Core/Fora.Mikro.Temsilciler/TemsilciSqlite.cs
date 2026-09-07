using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Database.Sqlite.Extension;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Temsilciler;

public static class TemsilciSqlite
{
	public static string GetTemsilciAdi(SqliteConnection connection, string temsilci_kodu)
	{
		string commandText = "SELECT cari_per_adi,cari_per_soyadi FROM CARI_PERSONEL_TANIMLARI WHERE cari_per_kod=@temsilci_kodu";
		string result = "";
		try
		{
			SqliteCommand val = connection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@temsilci_kodu", (object)temsilci_kodu);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					result = val2.GetSafeString(0) + " " + val2.GetSafeString(1);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static List<CariRotaListItem> GetCariRotaListItems(SqliteConnection connection, string temsilci_kodu, DateTime tarih, CariSiralamaSekli SiralamaSekli)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		int num = (int)tarih.DayOfWeek;
		if (num == 0)
		{
			num = 7;
		}
		string text = "SELECT adr_cari_kod,adr_adres_no,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_temsilci_kodu,adr_ozel_not,adr_gps_enlem,adr_gps_boylam,adr_yon_kodu,adr_uzaklik_kodu,adr_ziyaretperyodu,adr_ziyaretgunu,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7,cari_unvan1,cari_unvan2,cari_hareket_tipi,cari_tipi FROM CARI_HESAP_ADRESLERI INNER JOIN CARI_HESAPLAR ON CARI_HESAPLAR.cari_kod=adr_cari_kod WHERE adr_temsilci_kodu=@adr_temsilci_kodu ORDER BY adr_yon_kodu,adr_uzaklik_kodu";
		switch (SiralamaSekli)
		{
		case CariSiralamaSekli.IsmeGoreArtan:
			text += ",cari_unvan1_upper ";
			break;
		case CariSiralamaSekli.IsmeGoreAzalan:
			text += ",cari_unvan1_upper DESC ";
			break;
		case CariSiralamaSekli.KodaGoreArtan:
			text += ",cari_kod_upper ";
			break;
		case CariSiralamaSekli.KodaGoreAzalan:
			text += ",cari_kod_upper DESC ";
			break;
		}
		List<CariRotaListItem> list = new List<CariRotaListItem>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = connection;
			val.Parameters.AddWithValue("@adr_temsilci_kodu", (object)temsilci_kodu);
			SqliteDataReader val2 = val.ExecuteReader();
			bool flag = false;
			while (((DbDataReader)val2).Read())
			{
				int safeInt = val2.GetSafeInt32(20);
				decimal safeDecimal = val2.GetSafeDecimal(21);
				val2.GetSafeInt32(22);
				bool safeBoolean = val2.GetSafeBoolean(23);
				bool safeBoolean2 = val2.GetSafeBoolean(24);
				bool safeBoolean3 = val2.GetSafeBoolean(25);
				bool safeBoolean4 = val2.GetSafeBoolean(26);
				bool safeBoolean5 = val2.GetSafeBoolean(27);
				bool safeBoolean6 = val2.GetSafeBoolean(28);
				bool safeBoolean7 = val2.GetSafeBoolean(29);
				flag = false;
				switch (safeInt)
				{
				case 1:
					flag = true;
					break;
				case 2:
					if (safeDecimal == (decimal)num)
					{
						flag = true;
					}
					break;
				case 4:
					if (safeDecimal == (decimal)tarih.Day)
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
				case 11:
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
				case 12:
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
					cariRotaListItem.CariAdres.adr_cari_kod = ((DbDataReader)val2)[0].ToString();
					cariRotaListItem.cari_kod = ((DbDataReader)val2)[0].ToString();
					cariRotaListItem.CariAdres.adr_adres_no = int.Parse(((DbDataReader)val2)[1].ToString());
					cariRotaListItem.CariAdres.adr_cadde = ((DbDataReader)val2)[2].ToString();
					cariRotaListItem.CariAdres.adr_sokak = ((DbDataReader)val2)[3].ToString();
					cariRotaListItem.CariAdres.adr_posta_kodu = ((DbDataReader)val2)[4].ToString();
					cariRotaListItem.CariAdres.adr_ilce = ((DbDataReader)val2)[5].ToString();
					cariRotaListItem.CariAdres.adr_il = ((DbDataReader)val2)[6].ToString();
					cariRotaListItem.CariAdres.adr_ulke = ((DbDataReader)val2)[7].ToString();
					cariRotaListItem.CariAdres.adr_tel_ulke_kodu = ((DbDataReader)val2)[8].ToString();
					cariRotaListItem.CariAdres.adr_tel_bolge_kodu = ((DbDataReader)val2)[9].ToString();
					cariRotaListItem.CariAdres.adr_tel_no1 = ((DbDataReader)val2)[10].ToString();
					cariRotaListItem.CariAdres.adr_tel_no2 = ((DbDataReader)val2)[11].ToString();
					cariRotaListItem.CariAdres.adr_tel_faxno = ((DbDataReader)val2)[12].ToString();
					cariRotaListItem.CariAdres.adr_tel_modem = ((DbDataReader)val2)[13].ToString();
					cariRotaListItem.CariAdres.adr_temsilci_kodu = ((DbDataReader)val2)[14].ToString();
					cariRotaListItem.CariAdres.adr_ozel_not = ((DbDataReader)val2)[15].ToString();
					cariRotaListItem.CariAdres.adr_gps_enlem = float.Parse(((DbDataReader)val2)[16].ToString());
					cariRotaListItem.CariAdres.adr_gps_boylam = float.Parse(((DbDataReader)val2)[17].ToString());
					cariRotaListItem.CariAdres.adr_yon_kodu = ((DbDataReader)val2)[18].ToString();
					cariRotaListItem.CariAdres.adr_uzaklik_kodu = int.Parse(((DbDataReader)val2)[19].ToString());
					cariRotaListItem.CariAdres.adr_ziyaretperyodu = int.Parse(((DbDataReader)val2)[20].ToString());
					cariRotaListItem.CariAdres.adr_ziyaretgunu = ((DbDataReader)val2)[21].ToString();
					cariRotaListItem.CariAdres.adr_ziyarethaftasi = int.Parse(((DbDataReader)val2)[22].ToString());
					cariRotaListItem.CariAdres.adr_ziygunu2_1 = ((DbDataReader)val2)[23].ToString() == "1";
					cariRotaListItem.CariAdres.adr_ziygunu2_2 = ((DbDataReader)val2)[24].ToString() == "1";
					cariRotaListItem.CariAdres.adr_ziygunu2_3 = ((DbDataReader)val2)[25].ToString() == "1";
					cariRotaListItem.CariAdres.adr_ziygunu2_4 = ((DbDataReader)val2)[26].ToString() == "1";
					cariRotaListItem.CariAdres.adr_ziygunu2_5 = ((DbDataReader)val2)[27].ToString() == "1";
					cariRotaListItem.CariAdres.adr_ziygunu2_6 = ((DbDataReader)val2)[28].ToString() == "1";
					cariRotaListItem.CariAdres.adr_ziygunu2_7 = ((DbDataReader)val2)[29].ToString() == "1";
					cariRotaListItem.cari_unvan1 = ((DbDataReader)val2)[30].ToString();
					cariRotaListItem.cari_unvan2 = ((DbDataReader)val2)[31].ToString();
					cariRotaListItem.cari_hareket_tipi = (enum_cari_hareket_tipi)val2.GetSafeByte(32);
					cariRotaListItem.cari_tipi = (enum_cari_tipi)val2.GetSafeByte(33);
					list.Add(cariRotaListItem);
				}
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch
		{
		}
		return list;
	}
}
