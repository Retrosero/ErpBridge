using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.BedenHareketleri;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Stoklar.StokBedenTanimlari;
using Fora.Mikro.Stoklar.StokRenkleri;
using Fora.Mikro.Vergiler;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar;

public static class StokSqlite
{
	public static Stok GetStok(SqliteConnection connection, string stok_kodu, enum_toptan_perakende toptan_perakende)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT sto_kod,sto_isim,sto_perakende_vergi,sto_toptan_vergi,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_cins,sto_doviz_cinsi,sto_detay_takip,sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_bedenli_takip,sto_renkDetayli,sto_beden_kodu,sto_renk_kodu,sto_altgrup_kod,sto_anagrup_kod,sto_sektor_kodu,sto_marka_kodu,sto_model_kodu,sto_uretici_kodu,sto_reyon_kodu,sto_renk_kodu,sto_standartmaliyet,sto_yer_kod,sto_birim1_agirlik,sto_birim1_en,sto_birim1_boy,sto_birim1_yukseklik,sto_birim1_dara,sto_birim2_agirlik,sto_birim2_en,sto_birim2_boy,sto_birim2_yukseklik,sto_birim2_dara,sto_birim3_agirlik,sto_birim3_en,sto_birim3_boy,sto_birim3_yukseklik,sto_birim3_dara,sto_birim4_agirlik,sto_birim4_en,sto_birim4_boy,sto_birim4_yukseklik,sto_birim4_dara FROM STOKLAR WHERE sto_kod=@sto_kod";
		Stok stok = new Stok();
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = connection
			};
			val.Parameters.AddWithValue("@sto_kod", (object)stok_kodu);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				stok.sto_kod = val2.GetSafeString(0);
				stok.sto_isim = val2.GetSafeString(1);
				stok.sto_perakende_vergi_yeni = val2.GetSafeInt16(2);
				stok.sto_toptan_vergi_yeni = val2.GetSafeInt16(3);
				stok.sto_kisa_ismi = val2.GetSafeString(4);
				stok.sto_yabanci_isim = val2.GetSafeString(5);
				stok.sto_sat_cari_kod = val2.GetSafeString(6);
				stok.sto_cins = val2.GetSafeInt16(7);
				stok.sto_doviz_cinsi = val2.GetSafeInt16(8);
				stok.sto_detay_takip = val2.GetSafeInt16(9);
				stok.sto_birim1_ad = val2.GetSafeString(10);
				stok.sto_birim1_katsayi = val2.GetSafeDouble(11);
				stok.sto_birim2_ad = val2.GetSafeString(12);
				stok.sto_birim2_katsayi = val2.GetSafeDouble(13);
				stok.sto_birim3_ad = val2.GetSafeString(14);
				stok.sto_birim3_katsayi = val2.GetSafeDouble(15);
				stok.sto_birim4_ad = val2.GetSafeString(16);
				stok.sto_birim4_katsayi = val2.GetSafeDouble(17);
				stok.sto_bedenli_takip = val2.GetSafeBoolean(18);
				stok.sto_renkDetayli = val2.GetSafeBoolean(19);
				stok.sto_beden_kodu = val2.GetSafeString(20);
				stok.sto_renk_kodu = val2.GetSafeString(21);
				stok.sto_altgrup_kod = val2.GetSafeString(22);
				stok.sto_anagrup_kod = val2.GetSafeString(23);
				stok.sto_sektor_kodu = val2.GetSafeString(24);
				stok.sto_marka_kodu = val2.GetSafeString(25);
				stok.sto_model_kodu = val2.GetSafeString(26);
				stok.sto_uretici_kodu = val2.GetSafeString(27);
				stok.sto_reyon_kodu = val2.GetSafeString(28);
				stok.sto_renk_kodu = val2.GetSafeString(29);
				stok.sto_standartmaliyet = val2.GetSafeDouble(30);
				stok.sto_yer_kod = val2.GetSafeString(31);
				stok.sto_birim1_agirlik = val2.GetSafeDouble(32);
				stok.sto_birim1_en = val2.GetSafeDouble(33);
				stok.sto_birim1_boy = val2.GetSafeDouble(34);
				stok.sto_birim1_yukseklik = val2.GetSafeDouble(35);
				stok.sto_birim1_dara = val2.GetSafeDouble(36);
				stok.sto_birim2_agirlik = val2.GetSafeDouble(37);
				stok.sto_birim2_en = val2.GetSafeDouble(38);
				stok.sto_birim2_boy = val2.GetSafeDouble(39);
				stok.sto_birim2_yukseklik = val2.GetSafeDouble(40);
				stok.sto_birim2_dara = val2.GetSafeDouble(41);
				stok.sto_birim3_agirlik = val2.GetSafeDouble(42);
				stok.sto_birim3_en = val2.GetSafeDouble(43);
				stok.sto_birim3_boy = val2.GetSafeDouble(44);
				stok.sto_birim3_yukseklik = val2.GetSafeDouble(45);
				stok.sto_birim3_dara = val2.GetSafeDouble(46);
				stok.sto_birim4_agirlik = val2.GetSafeDouble(47);
				stok.sto_birim4_en = val2.GetSafeDouble(48);
				stok.sto_birim4_boy = val2.GetSafeDouble(49);
				stok.sto_birim4_yukseklik = val2.GetSafeDouble(50);
				stok.sto_birim4_dara = val2.GetSafeDouble(51);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			switch (toptan_perakende)
			{
			case enum_toptan_perakende.Perakende:
				stok.ekleme_bilgileri.vergi_pntr = stok.sto_perakende_vergi_yeni;
				break;
			case enum_toptan_perakende.Toptan:
				stok.ekleme_bilgileri.vergi_pntr = stok.sto_toptan_vergi_yeni;
				break;
			}
			if (stok.sto_doviz_cinsi != 0)
			{
				stok.ekleme_bilgileri.StokDovizCinsiKuru = KurSqlite.GetKur(connection, stok.sto_doviz_cinsi, "1").dov_fiyat;
			}
		}
		catch
		{
		}
		return stok;
	}

	public static STOK_BEDEN_TANIMLARI GetStokBedenTanimi(SqliteConnection connection, string bdn_kodu)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT bdn_kodu,bdn_ismi,bdn_kirilim_1,bdn_kirilim_2,bdn_kirilim_3,bdn_kirilim_4,bdn_kirilim_5,bdn_kirilim_6,bdn_kirilim_7,bdn_kirilim_8,bdn_kirilim_9,bdn_kirilim_10,bdn_kirilim_11,bdn_kirilim_12,bdn_kirilim_13,bdn_kirilim_14,bdn_kirilim_15,bdn_kirilim_16,bdn_kirilim_17,bdn_kirilim_18,bdn_kirilim_19,bdn_kirilim_20,bdn_kirilim_21,bdn_kirilim_22,bdn_kirilim_23,bdn_kirilim_24,bdn_kirilim_25,bdn_kirilim_26,bdn_kirilim_27,bdn_kirilim_28,bdn_kirilim_29,bdn_kirilim_30,bdn_kirilim_31,bdn_kirilim_32,bdn_kirilim_33,bdn_kirilim_34,bdn_kirilim_35,bdn_kirilim_36,bdn_kirilim_37,bdn_kirilim_38,bdn_kirilim_39,bdn_kirilim_40 FROM STOK_BEDEN_TANIMLARI WHERE bdn_kodu=@bdn_kodu";
		STOK_BEDEN_TANIMLARI sTOK_BEDEN_TANIMLARI = new STOK_BEDEN_TANIMLARI();
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = connection
			};
			val.Parameters.AddWithValue("@bdn_kodu", (object)bdn_kodu);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				sTOK_BEDEN_TANIMLARI.bdn_kodu = val2.GetSafeString(0);
				sTOK_BEDEN_TANIMLARI.bdn_ismi = val2.GetSafeString(1);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_1 = val2.GetSafeString(2);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_2 = val2.GetSafeString(3);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_3 = val2.GetSafeString(4);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_4 = val2.GetSafeString(5);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_5 = val2.GetSafeString(6);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_6 = val2.GetSafeString(7);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_7 = val2.GetSafeString(8);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_8 = val2.GetSafeString(9);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_9 = val2.GetSafeString(10);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_10 = val2.GetSafeString(11);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_11 = val2.GetSafeString(12);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_12 = val2.GetSafeString(13);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_13 = val2.GetSafeString(14);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_14 = val2.GetSafeString(15);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_15 = val2.GetSafeString(16);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_16 = val2.GetSafeString(17);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_17 = val2.GetSafeString(18);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_18 = val2.GetSafeString(19);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_19 = val2.GetSafeString(20);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_20 = val2.GetSafeString(21);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_21 = val2.GetSafeString(22);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_22 = val2.GetSafeString(23);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_23 = val2.GetSafeString(24);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_24 = val2.GetSafeString(25);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_25 = val2.GetSafeString(26);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_26 = val2.GetSafeString(27);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_27 = val2.GetSafeString(28);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_28 = val2.GetSafeString(29);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_29 = val2.GetSafeString(30);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_30 = val2.GetSafeString(31);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_31 = val2.GetSafeString(32);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_32 = val2.GetSafeString(33);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_33 = val2.GetSafeString(34);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_34 = val2.GetSafeString(35);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_35 = val2.GetSafeString(36);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_36 = val2.GetSafeString(37);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_37 = val2.GetSafeString(38);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_38 = val2.GetSafeString(39);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_39 = val2.GetSafeString(40);
				sTOK_BEDEN_TANIMLARI.bdn_kirilim_40 = val2.GetSafeString(41);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return sTOK_BEDEN_TANIMLARI;
	}

	public static STOK_RENK_TANIMLARI GetStokRenkTanimi(SqliteConnection connection, string rnk_kodu)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT rnk_kodu,rnk_ismi,rnk_kirilim_1,rnk_kirilim_2,rnk_kirilim_3,rnk_kirilim_4,rnk_kirilim_5,rnk_kirilim_6,rnk_kirilim_7,rnk_kirilim_8,rnk_kirilim_9,rnk_kirilim_10,rnk_kirilim_11,rnk_kirilim_12,rnk_kirilim_13,rnk_kirilim_14,rnk_kirilim_15,rnk_kirilim_16,rnk_kirilim_17,rnk_kirilim_18,rnk_kirilim_19,rnk_kirilim_20,rnk_kirilim_21,rnk_kirilim_22,rnk_kirilim_23,rnk_kirilim_24,rnk_kirilim_25,rnk_kirilim_26,rnk_kirilim_27,rnk_kirilim_28,rnk_kirilim_29,rnk_kirilim_30,rnk_kirilim_31,rnk_kirilim_32,rnk_kirilim_33,rnk_kirilim_34,rnk_kirilim_35,rnk_kirilim_36,rnk_kirilim_37,rnk_kirilim_38,rnk_kirilim_39,rnk_kirilim_40,rnk_kirilim_41,rnk_kirilim_42,rnk_kirilim_43,rnk_kirilim_44,rnk_kirilim_45,rnk_kirilim_46,rnk_kirilim_47,rnk_kirilim_48,rnk_kirilim_49,rnk_kirilim_50,rnk_kirilim_51,rnk_kirilim_52,rnk_kirilim_53,rnk_kirilim_54,rnk_kirilim_55,rnk_kirilim_56,rnk_kirilim_57,rnk_kirilim_58,rnk_kirilim_59,rnk_kirilim_60 FROM STOK_RENK_TANIMLARI WHERE rnk_kodu=@rnk_kodu";
		STOK_RENK_TANIMLARI sTOK_RENK_TANIMLARI = new STOK_RENK_TANIMLARI();
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = connection
			};
			val.Parameters.AddWithValue("@rnk_kodu", (object)rnk_kodu);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				sTOK_RENK_TANIMLARI.rnk_kodu = val2.GetSafeString(0);
				sTOK_RENK_TANIMLARI.rnk_ismi = val2.GetSafeString(1);
				sTOK_RENK_TANIMLARI.rnk_kirilim_1 = val2.GetSafeString(2);
				sTOK_RENK_TANIMLARI.rnk_kirilim_2 = val2.GetSafeString(3);
				sTOK_RENK_TANIMLARI.rnk_kirilim_3 = val2.GetSafeString(4);
				sTOK_RENK_TANIMLARI.rnk_kirilim_4 = val2.GetSafeString(5);
				sTOK_RENK_TANIMLARI.rnk_kirilim_5 = val2.GetSafeString(6);
				sTOK_RENK_TANIMLARI.rnk_kirilim_6 = val2.GetSafeString(7);
				sTOK_RENK_TANIMLARI.rnk_kirilim_7 = val2.GetSafeString(8);
				sTOK_RENK_TANIMLARI.rnk_kirilim_8 = val2.GetSafeString(9);
				sTOK_RENK_TANIMLARI.rnk_kirilim_9 = val2.GetSafeString(10);
				sTOK_RENK_TANIMLARI.rnk_kirilim_10 = val2.GetSafeString(11);
				sTOK_RENK_TANIMLARI.rnk_kirilim_11 = val2.GetSafeString(12);
				sTOK_RENK_TANIMLARI.rnk_kirilim_12 = val2.GetSafeString(13);
				sTOK_RENK_TANIMLARI.rnk_kirilim_13 = val2.GetSafeString(14);
				sTOK_RENK_TANIMLARI.rnk_kirilim_14 = val2.GetSafeString(15);
				sTOK_RENK_TANIMLARI.rnk_kirilim_15 = val2.GetSafeString(16);
				sTOK_RENK_TANIMLARI.rnk_kirilim_16 = val2.GetSafeString(17);
				sTOK_RENK_TANIMLARI.rnk_kirilim_17 = val2.GetSafeString(18);
				sTOK_RENK_TANIMLARI.rnk_kirilim_18 = val2.GetSafeString(19);
				sTOK_RENK_TANIMLARI.rnk_kirilim_19 = val2.GetSafeString(20);
				sTOK_RENK_TANIMLARI.rnk_kirilim_20 = val2.GetSafeString(21);
				sTOK_RENK_TANIMLARI.rnk_kirilim_21 = val2.GetSafeString(22);
				sTOK_RENK_TANIMLARI.rnk_kirilim_22 = val2.GetSafeString(23);
				sTOK_RENK_TANIMLARI.rnk_kirilim_23 = val2.GetSafeString(24);
				sTOK_RENK_TANIMLARI.rnk_kirilim_24 = val2.GetSafeString(25);
				sTOK_RENK_TANIMLARI.rnk_kirilim_25 = val2.GetSafeString(26);
				sTOK_RENK_TANIMLARI.rnk_kirilim_26 = val2.GetSafeString(27);
				sTOK_RENK_TANIMLARI.rnk_kirilim_27 = val2.GetSafeString(28);
				sTOK_RENK_TANIMLARI.rnk_kirilim_28 = val2.GetSafeString(29);
				sTOK_RENK_TANIMLARI.rnk_kirilim_29 = val2.GetSafeString(30);
				sTOK_RENK_TANIMLARI.rnk_kirilim_30 = val2.GetSafeString(31);
				sTOK_RENK_TANIMLARI.rnk_kirilim_31 = val2.GetSafeString(32);
				sTOK_RENK_TANIMLARI.rnk_kirilim_32 = val2.GetSafeString(33);
				sTOK_RENK_TANIMLARI.rnk_kirilim_33 = val2.GetSafeString(34);
				sTOK_RENK_TANIMLARI.rnk_kirilim_34 = val2.GetSafeString(35);
				sTOK_RENK_TANIMLARI.rnk_kirilim_35 = val2.GetSafeString(36);
				sTOK_RENK_TANIMLARI.rnk_kirilim_36 = val2.GetSafeString(37);
				sTOK_RENK_TANIMLARI.rnk_kirilim_37 = val2.GetSafeString(38);
				sTOK_RENK_TANIMLARI.rnk_kirilim_38 = val2.GetSafeString(39);
				sTOK_RENK_TANIMLARI.rnk_kirilim_39 = val2.GetSafeString(40);
				sTOK_RENK_TANIMLARI.rnk_kirilim_40 = val2.GetSafeString(41);
				sTOK_RENK_TANIMLARI.rnk_kirilim_41 = val2.GetSafeString(42);
				sTOK_RENK_TANIMLARI.rnk_kirilim_42 = val2.GetSafeString(43);
				sTOK_RENK_TANIMLARI.rnk_kirilim_43 = val2.GetSafeString(44);
				sTOK_RENK_TANIMLARI.rnk_kirilim_44 = val2.GetSafeString(45);
				sTOK_RENK_TANIMLARI.rnk_kirilim_45 = val2.GetSafeString(46);
				sTOK_RENK_TANIMLARI.rnk_kirilim_46 = val2.GetSafeString(47);
				sTOK_RENK_TANIMLARI.rnk_kirilim_47 = val2.GetSafeString(48);
				sTOK_RENK_TANIMLARI.rnk_kirilim_48 = val2.GetSafeString(49);
				sTOK_RENK_TANIMLARI.rnk_kirilim_49 = val2.GetSafeString(50);
				sTOK_RENK_TANIMLARI.rnk_kirilim_50 = val2.GetSafeString(51);
				sTOK_RENK_TANIMLARI.rnk_kirilim_51 = val2.GetSafeString(52);
				sTOK_RENK_TANIMLARI.rnk_kirilim_52 = val2.GetSafeString(53);
				sTOK_RENK_TANIMLARI.rnk_kirilim_53 = val2.GetSafeString(54);
				sTOK_RENK_TANIMLARI.rnk_kirilim_54 = val2.GetSafeString(55);
				sTOK_RENK_TANIMLARI.rnk_kirilim_55 = val2.GetSafeString(56);
				sTOK_RENK_TANIMLARI.rnk_kirilim_56 = val2.GetSafeString(57);
				sTOK_RENK_TANIMLARI.rnk_kirilim_57 = val2.GetSafeString(58);
				sTOK_RENK_TANIMLARI.rnk_kirilim_58 = val2.GetSafeString(59);
				sTOK_RENK_TANIMLARI.rnk_kirilim_59 = val2.GetSafeString(60);
				sTOK_RENK_TANIMLARI.rnk_kirilim_60 = val2.GetSafeString(61);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return sTOK_RENK_TANIMLARI;
	}

	public static List<BEDEN_HAREKETLERI> V15_GetStokRenkBedenHareketleri(SqliteConnection connection, enum_BdnHar_Tipi BdnHar_Tipi, int BdnHar_DRECid_DBCno, int BdnHar_DRECid_RECno)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT BdnHar_RECno,BdnHar_RECid_DBCno,BdnHar_RECid_RECno,BdnHar_Spec_Rec_no,BdnHar_iptal,BdnHar_fileid,BdnHar_hidden,BdnHar_kilitli,BdnHar_degisti,BdnHar_checksum,BdnHar_create_user,BdnHar_create_date,BdnHar_lastup_user,BdnHar_lastup_date,BdnHar_special1,BdnHar_special2,BdnHar_special3,BdnHar_Tipi,BdnHar_DRECid_DBCno,BdnHar_DRECid_RECno,BdnHar_BedenNo,BdnHar_HarGor,BdnHar_KnsIsGor,BdnHar_KnsFat,BdnHar_TesMik,BdnHar_rezervasyon_miktari,BdnHar_rezerveden_teslim_edilen FROM BEDEN_HAREKETLERI WHERE BdnHar_Tipi=@BdnHar_Tipi AND BdnHar_DRECid_DBCno=@BdnHar_DRECid_DBCno AND BdnHar_DRECid_RECno=@BdnHar_DRECid_RECno";
		List<BEDEN_HAREKETLERI> list = new List<BEDEN_HAREKETLERI>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@BdnHar_Tipi", (object)(int)BdnHar_Tipi);
			val.Parameters.AddWithValue("@BdnHar_DRECid_DBCno", (object)BdnHar_DRECid_DBCno);
			val.Parameters.AddWithValue("@BdnHar_DRECid_RECno", (object)BdnHar_DRECid_RECno);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
				bEDEN_HAREKETLERI.BdnHar_RECno = val2.GetSafeInt32(0);
				bEDEN_HAREKETLERI.BdnHar_RECid_DBCno = val2.GetSafeInt16(1);
				bEDEN_HAREKETLERI.BdnHar_RECid_RECno = val2.GetSafeInt32(2);
				bEDEN_HAREKETLERI.BdnHar_Spec_Rec_no = val2.GetSafeInt32(3);
				bEDEN_HAREKETLERI.BdnHar_iptal = val2.GetSafeBoolean(4);
				bEDEN_HAREKETLERI.BdnHar_fileid = val2.GetSafeInt16(5);
				bEDEN_HAREKETLERI.BdnHar_hidden = val2.GetSafeBoolean(6);
				bEDEN_HAREKETLERI.BdnHar_kilitli = val2.GetSafeBoolean(7);
				bEDEN_HAREKETLERI.BdnHar_degisti = val2.GetSafeBoolean(8);
				bEDEN_HAREKETLERI.BdnHar_checksum = val2.GetSafeInt32(9);
				bEDEN_HAREKETLERI.BdnHar_create_user = val2.GetSafeInt16(10);
				bEDEN_HAREKETLERI.BdnHar_create_date = val2.GetSafeDateTime(11);
				bEDEN_HAREKETLERI.BdnHar_lastup_user = val2.GetSafeInt16(12);
				bEDEN_HAREKETLERI.BdnHar_lastup_date = val2.GetSafeDateTime(13);
				bEDEN_HAREKETLERI.BdnHar_special1 = val2.GetSafeString(14);
				bEDEN_HAREKETLERI.BdnHar_special2 = val2.GetSafeString(15);
				bEDEN_HAREKETLERI.BdnHar_special3 = val2.GetSafeString(16);
				bEDEN_HAREKETLERI.BdnHar_Tipi = val2.GetSafeByte(17);
				bEDEN_HAREKETLERI.BdnHar_DRECid_DBCno = val2.GetSafeInt16(18);
				bEDEN_HAREKETLERI.BdnHar_DRECid_RECno = val2.GetSafeInt32(19);
				bEDEN_HAREKETLERI.BdnHar_BedenNo = val2.GetSafeInt16(20);
				bEDEN_HAREKETLERI.BdnHar_HarGor = val2.GetSafeDouble(21);
				bEDEN_HAREKETLERI.BdnHar_KnsIsGor = val2.GetSafeDouble(22);
				bEDEN_HAREKETLERI.BdnHar_KnsFat = val2.GetSafeDouble(23);
				bEDEN_HAREKETLERI.BdnHar_TesMik = val2.GetSafeDouble(24);
				bEDEN_HAREKETLERI.BdnHar_rezervasyon_miktari = val2.GetSafeDouble(25);
				bEDEN_HAREKETLERI.BdnHar_rezerveden_teslim_edilen = val2.GetSafeDouble(26);
				list.Add(bEDEN_HAREKETLERI);
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

	public static List<BEDEN_HAREKETLERI> V16_GetStokRenkBedenHareketleri(SqliteConnection connection, enum_BdnHar_Tipi BdnHar_Tipi, Guid BdnHar_Har_uid)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT BdnHar_Guid,BdnHar_Spec_Rec_no,BdnHar_iptal,BdnHar_fileid,BdnHar_hidden,BdnHar_kilitli,BdnHar_degisti,BdnHar_checksum,BdnHar_create_user,BdnHar_create_date,BdnHar_lastup_user,BdnHar_lastup_date,BdnHar_special1,BdnHar_special2,BdnHar_special3,BdnHar_Tipi,BdnHar_Har_uid,BdnHar_BedenNo,BdnHar_HarGor,BdnHar_KnsIsGor,BdnHar_KnsFat,BdnHar_TesMik,BdnHar_rezervasyon_miktari,BdnHar_rezerveden_teslim_edilen FROM BEDEN_HAREKETLERI WHERE BdnHar_Tipi=@BdnHar_Tipi AND BdnHar_Har_uid=@BdnHar_Har_uid";
		List<BEDEN_HAREKETLERI> list = new List<BEDEN_HAREKETLERI>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@BdnHar_Tipi", (object)(int)BdnHar_Tipi);
			val.Parameters.AddWithValue("@BdnHar_Har_uid", (object)BdnHar_Har_uid.ToByteArray());
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
				bEDEN_HAREKETLERI.BdnHar_Guid = ((DbDataReader)val2).GetGuid(0);
				bEDEN_HAREKETLERI.BdnHar_Spec_Rec_no = val2.GetSafeInt32(1);
				bEDEN_HAREKETLERI.BdnHar_iptal = val2.GetSafeBoolean(2);
				bEDEN_HAREKETLERI.BdnHar_fileid = val2.GetSafeInt16(3);
				bEDEN_HAREKETLERI.BdnHar_hidden = val2.GetSafeBoolean(4);
				bEDEN_HAREKETLERI.BdnHar_kilitli = val2.GetSafeBoolean(5);
				bEDEN_HAREKETLERI.BdnHar_degisti = val2.GetSafeBoolean(6);
				bEDEN_HAREKETLERI.BdnHar_checksum = val2.GetSafeInt32(7);
				bEDEN_HAREKETLERI.BdnHar_create_user = val2.GetSafeInt16(8);
				bEDEN_HAREKETLERI.BdnHar_create_date = val2.GetSafeDateTime(9);
				bEDEN_HAREKETLERI.BdnHar_lastup_user = val2.GetSafeInt16(10);
				bEDEN_HAREKETLERI.BdnHar_lastup_date = val2.GetSafeDateTime(11);
				bEDEN_HAREKETLERI.BdnHar_special1 = val2.GetSafeString(12);
				bEDEN_HAREKETLERI.BdnHar_special2 = val2.GetSafeString(13);
				bEDEN_HAREKETLERI.BdnHar_special3 = val2.GetSafeString(14);
				bEDEN_HAREKETLERI.BdnHar_Tipi = val2.GetSafeByte(15);
				bEDEN_HAREKETLERI.BdnHar_Har_uid = ((DbDataReader)val2).GetGuid(16);
				bEDEN_HAREKETLERI.BdnHar_BedenNo = val2.GetSafeInt16(17);
				bEDEN_HAREKETLERI.BdnHar_HarGor = val2.GetSafeDouble(18);
				bEDEN_HAREKETLERI.BdnHar_KnsIsGor = val2.GetSafeDouble(19);
				bEDEN_HAREKETLERI.BdnHar_KnsFat = val2.GetSafeDouble(20);
				bEDEN_HAREKETLERI.BdnHar_TesMik = val2.GetSafeDouble(21);
				bEDEN_HAREKETLERI.BdnHar_rezervasyon_miktari = val2.GetSafeDouble(22);
				bEDEN_HAREKETLERI.BdnHar_rezerveden_teslim_edilen = val2.GetSafeDouble(23);
				list.Add(bEDEN_HAREKETLERI);
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

	public static double GetDepoMiktar(SqliteConnection OpenedConnection, string sto_kod, int dep_no)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		string text = "SELECT SUM(CASE WHEN (sth_tip=0) OR ((sth_tip=2) AND (sth_giris_depo_no=@depo_no)) THEN sth_miktar WHEN (sth_tip=1) OR ((sth_tip=2) AND (sth_cikis_depo_no=@depo_no)) THEN (-1) * sth_miktar ELSE 0 END) FROM STOK_HAREKETLERI WHERE (sth_stok_kod=@sto_kod) AND (((sth_tip=0) AND ((sth_giris_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=1) AND ((sth_cikis_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=2) AND (sth_giris_depo_no<>sth_cikis_depo_no) AND ((sth_giris_depo_no=@depo_no) OR (sth_cikis_depo_no=@depo_no)))) AND (NOT (sth_cins in (9,15)))";
		double num = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand(text, OpenedConnection);
			val.Parameters.AddWithValue("@depo_no", (object)dep_no);
			val.Parameters.AddWithValue("@sto_kod", (object)sto_kod);
			object obj = ((DbCommand)val).ExecuteScalar();
			num = ((obj != null && !(obj is DBNull)) ? ((double)obj) : 0.0);
			((Component)val).Dispose();
		}
		catch
		{
			num = 0.0;
		}
		return num;
	}

	public static double GetDepoMiktarRenkBedenliUrun(SqliteConnection OpenedConnection, string sto_kod, int dep_no, int beden_no)
	{
		double num = 0.0;
		foreach (BEDEN_HAREKETLERI item in GetStokRenkBedenDepoDurumu(AppBase.GetDbConnectionSync(), sto_kod, dep_no))
		{
			if (item.BdnHar_BedenNo == beden_no)
			{
				num += item.BdnHar_HarGor;
			}
		}
		return num;
	}

	public static double GetDepoMiktarWithSeriNo(SqliteConnection OpenedConnection, string sto_kod, int dep_no, string seri_no)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		text = ((AppBase.MikroVersiyonu <= 15) ? "SELECT SUM(CASE WHEN (sth_tip=0) OR ((sth_tip=2) AND (sth_giris_depo_no=@depo_no)) THEN 1 WHEN (sth_tip=1) OR ((sth_tip=2) AND (sth_cikis_depo_no=@depo_no)) THEN -1 ELSE 0 END) FROM STOK_HAREKETLERI WHERE (sth_RECno in (SELECT ChHar_master_recno FROM CIHAZ_HAREKETLERI WHERE ChHar_StokKodu=@stok_kodu AND ChHar_SeriNo=@seri_no)) AND (((sth_tip=0) AND ((sth_giris_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=1) AND ((sth_cikis_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=2) AND (sth_giris_depo_no<>sth_cikis_depo_no) AND ((sth_giris_depo_no=@depo_no) OR (sth_cikis_depo_no=@depo_no)))) AND (NOT (sth_cins in (9,15)))" : "SELECT SUM(CASE WHEN (sth_tip=0) OR ((sth_tip=2) AND (sth_giris_depo_no=@depo_no)) THEN 1 WHEN (sth_tip=1) OR ((sth_tip=2) AND (sth_cikis_depo_no=@depo_no)) THEN -1 ELSE 0 END) FROM STOK_HAREKETLERI WHERE (sto_Guid in (SELECT ChHar_master_uid FROM CIHAZ_HAREKETLERI WHERE ChHar_StokKodu=@stok_kodu AND ChHar_SeriNo=@seri_no)) AND (((sth_tip=0) AND ((sth_giris_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=1) AND ((sth_cikis_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=2) AND (sth_giris_depo_no<>sth_cikis_depo_no) AND ((sth_giris_depo_no=@depo_no) OR (sth_cikis_depo_no=@depo_no)))) AND (NOT (sth_cins in (9,15)))");
		double num = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand(text, OpenedConnection);
			val.Parameters.AddWithValue("@depo_no", (object)dep_no);
			val.Parameters.AddWithValue("@stok_kodu", (object)sto_kod);
			val.Parameters.AddWithValue("@seri_no", (object)seri_no);
			object obj = ((DbCommand)val).ExecuteScalar();
			num = ((obj != null && !(obj is DBNull)) ? ((double)obj) : 0.0);
			((Component)val).Dispose();
		}
		catch
		{
			num = 0.0;
		}
		return num;
	}

	public static double GetAcikSiparisMiktari(SqliteConnection OpenedConnection, string sto_kod, int dep_no)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		string text = "SELECT SUM(CASE WHEN (sip_kapat_fl=0) THEN sip_miktar - sip_teslim_miktar ELSE 0 END) FROM SIPARISLER WHERE (sip_tip=0) AND (sip_stok_kod=@stok_kodu) AND ((sip_depono=@depo) OR @depo=0)";
		double num = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand(text, OpenedConnection);
			val.Parameters.AddWithValue("@depo", (object)dep_no);
			val.Parameters.AddWithValue("@stok_kodu", (object)sto_kod);
			object obj = ((DbCommand)val).ExecuteScalar();
			num = ((obj != null && !(obj is DBNull)) ? ((double)obj) : 0.0);
			((Component)val).Dispose();
		}
		catch
		{
			num = 0.0;
		}
		return num;
	}

	public static double GetProformaSiparisMiktari(SqliteConnection OpenedConnection, string sto_kod, int dep_no)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		string text = "SELECT SUM(CASE WHEN (pro_kapat=0) THEN pro_miktar - pro_tesmiktari ELSE 0 END) FROM PROFORMA_SIPARISLER WHERE  (pro_tipi=0) AND (pro_stokkodu=@stok_kodu) AND ((pro_depono=@depo) OR @depo=0)";
		double num = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand(text, OpenedConnection);
			val.Parameters.AddWithValue("@depo", (object)dep_no);
			val.Parameters.AddWithValue("@stok_kodu", (object)sto_kod);
			object obj = ((DbCommand)val).ExecuteScalar();
			num = ((obj != null && !(obj is DBNull)) ? ((double)obj) : 0.0);
			((Component)val).Dispose();
		}
		catch
		{
			num = 0.0;
		}
		return num;
	}

	public static double GetYoldakiMalMiktari(SqliteConnection OpenedConnection, string sto_kod, int dep_no)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		string text = "SELECT SUM(CASE WHEN sip_kapat_fl=0 THEN (sip_miktar-sip_teslim_miktar) ELSE (0.0) END) FROM SIPARISLER WHERE (sip_tip=1) AND (sip_stok_kod=@stok_kodu) AND ((sip_depono=@depo) OR @depo=0)";
		double num = 0.0;
		try
		{
			SqliteCommand val = new SqliteCommand(text, OpenedConnection);
			val.Parameters.AddWithValue("@depo", (object)dep_no);
			val.Parameters.AddWithValue("@stok_kodu", (object)sto_kod);
			object obj = ((DbCommand)val).ExecuteScalar();
			num = ((obj != null && !(obj is DBNull)) ? ((double)obj) : 0.0);
			((Component)val).Dispose();
		}
		catch
		{
			num = 0.0;
		}
		return num;
	}

	public static List<StokListItem> GetStokListItems(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende, bool CalistigiStoklar, string CariKodu)
	{
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Expected O, but got Unknown
		string text = "SELECT sto_kod,sto_isim,sto_yabanci_isim,sto_kisa_ismi,sto_perakende_vergi,sto_toptan_vergi,sto_birim1_ad,sto_al_sip_birim FROM STOKLAR ";
		string text2 = " WHERE";
		switch (WebSayfasinaGonderilecekler)
		{
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=1";
			text2 = " AND";
			break;
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilmeyecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=0";
			text2 = " AND";
			break;
		}
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			text = text + text2 + " sto_siparis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		}
		switch (StokListelemeSecenek)
		{
		case enum_StokListelemeSecenekleri.AnaAltGrubaGore:
			text = text + text2 + " sto_anagrup_kod in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			if (StokListelemeAktifAltGrup != "")
			{
				text = text + text2 + " sto_altgrup_kod in ('" + StokListelemeAktifAltGrup.Replace(",", "','") + "')";
				text2 = " AND";
			}
			break;
		case enum_StokListelemeSecenekleri.UreticilereGore:
			text = text + text2 + " sto_uretici_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.KategorilereGore:
			text = text + text2 + " sto_kategori_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.ReyonlaraGore:
			text = text + text2 + " sto_reyon_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.MarkalaraGore:
			text = text + text2 + " sto_marka_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		}
		if (CalistigiStoklar)
		{
			text = text + text2 + " sto_kod in (SELECT DISTINCT sth_stok_kod FROM STOK_HAREKETLERI WHERE sth_cari_kodu='" + CariKodu + "')";
			text2 = " AND";
		}
		string text3 = " LIMIT 5000";
		if (Searchstring != "")
		{
			string text4 = "%" + Searchstring.ToUpper().Replace(" ", "%") + "%";
			text3 = " LIMIT 1000";
			text = text + text2 + " ((sto_kod_upper like '" + text4 + "') OR (sto_isim_upper like '" + text4 + "')";
			if (BarkodlardaAra)
			{
				text = text + " OR sto_kod in (SELECT bar_stokkodu FROM BARKOD_TANIMLARI WHERE bar_kodu like '" + text4 + "')";
			}
			text += ")";
			text2 = " AND";
		}
		switch (SiralamaSekli)
		{
		case enum_StokSiralamaSekli.stok_kodu:
			text += " ORDER BY sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.stok_ismi:
			text += " ORDER BY sto_isim_upper ";
			break;
		case enum_StokSiralamaSekli.ana_alt_grub:
			text += " ORDER BY sto_anagrup_kod,sto_altgrup_kod,sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.kategoriler:
			text += " ORDER BY sto_kategori_kodu,sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.markalar:
			text += " ORDER BY sto_marka_kodu,sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.reyonlar:
			text += " ORDER BY sto_reyon_kodu,sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.ureticiler:
			text += " ORDER BY sto_uretici_kodu,sto_kod_upper ";
			break;
		}
		text = text + " " + text3;
		List<StokListItem> list = new List<StokListItem>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(new StokListItem(val2.GetSafeString(0), val2.GetSafeString(1), val2.GetSafeString(2), val2.GetSafeString(3), val2.GetSafeInt16(4), val2.GetSafeInt16(5), val2.GetSafeString(6), val2.GetSafeInt32(7), toptan_perakende));
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

	public static List<StokListItem> GetStokListItemsWithFullText(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende, bool CalistigiStoklar, string CariKodu)
	{
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Expected O, but got Unknown
		string text = "SELECT STOKLAR.sto_kod,STOKLAR.sto_isim,STOKLAR.sto_yabanci_isim,STOKLAR.sto_kisa_ismi,STOKLAR.sto_perakende_vergi,STOKLAR.sto_toptan_vergi,STOKLAR.sto_birim1_ad,STOKLAR.sto_al_sip_birim FROM STOKLAR ";
		string text2 = " WHERE";
		string[] array = Searchstring.ToUpper().Split(new char[1] { ' ' });
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (text3.Length > 1)
			{
				list.Add(text3 + "*");
			}
		}
		if (list.Count != 0)
		{
			string text4 = "";
			bool flag = true;
			foreach (string item in list)
			{
				if (!flag)
				{
					text4 += " ";
				}
				text4 += item;
				flag = false;
			}
			string text5 = "%" + Searchstring.ToUpper().Replace(" ", "%") + "%";
			text = text + text2 + " (STOKLAR.rowid in (  SELECT docid FROM STOKLAR_fts WHERE STOKLAR_fts MATCH '" + text4 + "' LIMIT 1000)";
			if (BarkodlardaAra)
			{
				text = text + " OR sto_kod in (SELECT bar_stokkodu FROM BARKOD_TANIMLARI WHERE bar_kodu like '" + text5 + "')";
			}
			text += ")";
			text2 = " AND";
		}
		switch (WebSayfasinaGonderilecekler)
		{
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=1";
			text2 = " AND";
			break;
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilmeyecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=0";
			text2 = " AND";
			break;
		}
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			text = text + text2 + " sto_siparis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		}
		switch (StokListelemeSecenek)
		{
		case enum_StokListelemeSecenekleri.AnaAltGrubaGore:
			text = text + text2 + " sto_anagrup_kod in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			if (StokListelemeAktifAltGrup != "")
			{
				text = text + text2 + " sto_altgrup_kod in ('" + StokListelemeAktifAltGrup.Replace(",", "','") + "')";
				text2 = " AND";
			}
			break;
		case enum_StokListelemeSecenekleri.UreticilereGore:
			text = text + text2 + " sto_uretici_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.KategorilereGore:
			text = text + text2 + " sto_kategori_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.ReyonlaraGore:
			text = text + text2 + " sto_reyon_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.MarkalaraGore:
			text = text + text2 + " sto_marka_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		}
		if (CalistigiStoklar)
		{
			text = text + text2 + " sto_kod in (SELECT DISTINCT sth_stok_kod FROM STOK_HAREKETLERI WHERE sth_cari_kodu='" + CariKodu + "')";
			text2 = " AND";
		}
		switch (SiralamaSekli)
		{
		case enum_StokSiralamaSekli.stok_kodu:
			text += " ORDER BY sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.stok_ismi:
			text += " ORDER BY sto_isim_upper ";
			break;
		case enum_StokSiralamaSekli.ana_alt_grub:
			text += " ORDER BY sto_anagrup_kod,sto_altgrup_kod ";
			break;
		case enum_StokSiralamaSekli.kategoriler:
			text += " ORDER BY sto_kategori_kodu ";
			break;
		case enum_StokSiralamaSekli.markalar:
			text += " ORDER BY sto_marka_kodu ";
			break;
		case enum_StokSiralamaSekli.reyonlar:
			text += " ORDER BY sto_reyon_kodu ";
			break;
		case enum_StokSiralamaSekli.ureticiler:
			text += " ORDER BY sto_uretici_kodu ";
			break;
		}
		text += " LIMIT 5000";
		List<StokListItem> list2 = new List<StokListItem>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list2.Add(new StokListItem(val2.GetSafeString(0), val2.GetSafeString(1), val2.GetSafeString(2), val2.GetSafeString(3), val2.GetSafeInt16(4), val2.GetSafeInt16(5), val2.GetSafeString(6), val2.GetSafeInt32(7), toptan_perakende));
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
		return list2;
	}

	public static List<StokListItemFiyatveMiktarli> GetStokListFiyatliItems(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende)
	{
		if (AppBase.MikroVersiyonu > 15)
		{
			return V16_GetStokListFiyatliItems(OpenedConnection, Searchstring, StokListelemeSecenek, StokListelemeAktifGrup, StokListelemeAktifAltGrup, WebSayfasinaGonderilecekler, BarkodlardaAra, SiralamaSekli, evraktipi, toptan_perakende);
		}
		return V15_GetStokListFiyatliItems(OpenedConnection, Searchstring, StokListelemeSecenek, StokListelemeAktifGrup, StokListelemeAktifAltGrup, WebSayfasinaGonderilecekler, BarkodlardaAra, SiralamaSekli, evraktipi, toptan_perakende);
	}

	public static List<StokListItemFiyatveMiktarli> V15_GetStokListFiyatliItems(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende)
	{
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Expected O, but got Unknown
		string text = "SELECT STOKLAR.sto_RECno,sto_kod,sto_isim,sto_yabanci_isim,sto_kisa_ismi,sto_perakende_vergi,sto_toptan_vergi,sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_al_sip_birim,sto_anagrup_kod,sto_altgrup_kod,sto_uretici_kodu,sto_reyon_kodu,sto_marka_kodu FROM STOKLAR ";
		string text2 = " WHERE";
		switch (WebSayfasinaGonderilecekler)
		{
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=1";
			text2 = " AND";
			break;
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilmeyecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=0";
			text2 = " AND";
			break;
		}
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			text = text + text2 + " sto_siparis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		}
		switch (StokListelemeSecenek)
		{
		case enum_StokListelemeSecenekleri.AnaAltGrubaGore:
			text = text + text2 + " sto_anagrup_kod in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			if (StokListelemeAktifAltGrup != "")
			{
				text = text + text2 + " sto_altgrup_kod in ('" + StokListelemeAktifAltGrup.Replace(",", "','") + "')";
				text2 = " AND";
			}
			break;
		case enum_StokListelemeSecenekleri.UreticilereGore:
			text = text + text2 + " sto_uretici_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.KategorilereGore:
			text = text + text2 + " sto_kategori_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.ReyonlaraGore:
			text = text + text2 + " sto_reyon_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.MarkalaraGore:
			text = text + text2 + " sto_marka_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		}
		string text3 = " LIMIT 1000";
		if (Searchstring != "")
		{
			string text4 = "%" + Searchstring.ToUpper().Replace(" ", "%") + "%";
			text3 = " LIMIT 500";
			text = text + text2 + " ((sto_kod_upper like '" + text4 + "') OR (sto_isim_upper like '" + text4 + "')";
			if (BarkodlardaAra)
			{
				text = text + " OR sto_kod in (SELECT bar_stokkodu FROM BARKOD_TANIMLARI WHERE bar_kodu like '" + text4 + "')";
			}
			text += ")";
			text2 = " AND";
		}
		switch (SiralamaSekli)
		{
		case enum_StokSiralamaSekli.stok_kodu:
			text += " ORDER BY sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.stok_ismi:
			text += " ORDER BY sto_isim_upper ";
			break;
		case enum_StokSiralamaSekli.ana_alt_grub:
			text += " ORDER BY sto_anagrup_kod,sto_altgrup_kod ";
			break;
		case enum_StokSiralamaSekli.kategoriler:
			text += " ORDER BY sto_kategori_kodu ";
			break;
		case enum_StokSiralamaSekli.markalar:
			text += " ORDER BY sto_marka_kodu ";
			break;
		case enum_StokSiralamaSekli.reyonlar:
			text += " ORDER BY sto_reyon_kodu ";
			break;
		case enum_StokSiralamaSekli.ureticiler:
			text += " ORDER BY sto_uretici_kodu ";
			break;
		}
		text = text + " " + text3;
		List<StokListItemFiyatveMiktarli> list = new List<StokListItemFiyatveMiktarli>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(new StokListItemFiyatveMiktarli(val2.GetSafeInt32(0), val2.GetSafeString(1), val2.GetSafeString(2), val2.GetSafeString(3), val2.GetSafeString(4), val2.GetSafeInt16(5), val2.GetSafeInt16(6), val2.GetSafeString(7), val2.GetSafeDouble(8), val2.GetSafeString(9), val2.GetSafeDouble(10), val2.GetSafeString(11), val2.GetSafeDouble(12), val2.GetSafeString(13), val2.GetSafeDouble(14), val2.GetSafeInt16(15), val2.GetSafeString(16), val2.GetSafeString(17), val2.GetSafeString(18), val2.GetSafeString(19), val2.GetSafeString(20), toptan_perakende));
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

	public static List<StokListItemFiyatveMiktarli> V16_GetStokListFiyatliItems(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende)
	{
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Expected O, but got Unknown
		string text = "SELECT STOKLAR.sto_Guid,sto_kod,sto_isim,sto_yabanci_isim,sto_kisa_ismi,sto_perakende_vergi,sto_toptan_vergi,sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_al_sip_birim,sto_anagrup_kod,sto_altgrup_kod,sto_uretici_kodu,sto_reyon_kodu,sto_marka_kodu FROM STOKLAR ";
		string text2 = " WHERE";
		switch (WebSayfasinaGonderilecekler)
		{
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=1";
			text2 = " AND";
			break;
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilmeyecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=0";
			text2 = " AND";
			break;
		}
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			text = text + text2 + " sto_siparis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		}
		switch (StokListelemeSecenek)
		{
		case enum_StokListelemeSecenekleri.AnaAltGrubaGore:
			text = text + text2 + " sto_anagrup_kod in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			if (StokListelemeAktifAltGrup != "")
			{
				text = text + text2 + " sto_altgrup_kod in ('" + StokListelemeAktifAltGrup.Replace(",", "','") + "')";
				text2 = " AND";
			}
			break;
		case enum_StokListelemeSecenekleri.UreticilereGore:
			text = text + text2 + " sto_uretici_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.KategorilereGore:
			text = text + text2 + " sto_kategori_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.ReyonlaraGore:
			text = text + text2 + " sto_reyon_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.MarkalaraGore:
			text = text + text2 + " sto_marka_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		}
		string text3 = " LIMIT 1000";
		if (Searchstring != "")
		{
			string text4 = "%" + Searchstring.ToUpper().Replace(" ", "%") + "%";
			text3 = " LIMIT 500";
			text = text + text2 + " ((sto_kod_upper like '" + text4 + "') OR (sto_isim_upper like '" + text4 + "')";
			if (BarkodlardaAra)
			{
				text = text + " OR sto_kod in (SELECT bar_stokkodu FROM BARKOD_TANIMLARI WHERE bar_kodu like '" + text4 + "')";
			}
			text += ")";
			text2 = " AND";
		}
		switch (SiralamaSekli)
		{
		case enum_StokSiralamaSekli.stok_kodu:
			text += " ORDER BY sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.stok_ismi:
			text += " ORDER BY sto_isim_upper ";
			break;
		case enum_StokSiralamaSekli.ana_alt_grub:
			text += " ORDER BY sto_anagrup_kod,sto_altgrup_kod ";
			break;
		case enum_StokSiralamaSekli.kategoriler:
			text += " ORDER BY sto_kategori_kodu ";
			break;
		case enum_StokSiralamaSekli.markalar:
			text += " ORDER BY sto_marka_kodu ";
			break;
		case enum_StokSiralamaSekli.reyonlar:
			text += " ORDER BY sto_reyon_kodu ";
			break;
		case enum_StokSiralamaSekli.ureticiler:
			text += " ORDER BY sto_uretici_kodu ";
			break;
		}
		text = text + " " + text3;
		List<StokListItemFiyatveMiktarli> list = new List<StokListItemFiyatveMiktarli>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(new StokListItemFiyatveMiktarli(((DbDataReader)val2).GetGuid(0), val2.GetSafeString(1), val2.GetSafeString(2), val2.GetSafeString(3), val2.GetSafeString(4), val2.GetSafeInt16(5), val2.GetSafeInt16(6), val2.GetSafeString(7), val2.GetSafeDouble(8), val2.GetSafeString(9), val2.GetSafeDouble(10), val2.GetSafeString(11), val2.GetSafeDouble(12), val2.GetSafeString(13), val2.GetSafeDouble(14), val2.GetSafeInt16(15), val2.GetSafeString(16), val2.GetSafeString(17), val2.GetSafeString(18), val2.GetSafeString(19), val2.GetSafeString(20), toptan_perakende));
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

	public static List<StokListItemFiyatveMiktarli> GetStokListFiyatliItemsWithFullText(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende)
	{
		if (AppBase.MikroVersiyonu > 15)
		{
			return V16_GetStokListFiyatliItemsWithFullText(OpenedConnection, Searchstring, StokListelemeSecenek, StokListelemeAktifGrup, StokListelemeAktifAltGrup, WebSayfasinaGonderilecekler, BarkodlardaAra, SiralamaSekli, evraktipi, toptan_perakende);
		}
		return V15_GetStokListFiyatliItemsWithFullText(OpenedConnection, Searchstring, StokListelemeSecenek, StokListelemeAktifGrup, StokListelemeAktifAltGrup, WebSayfasinaGonderilecekler, BarkodlardaAra, SiralamaSekli, evraktipi, toptan_perakende);
	}

	public static List<StokListItemFiyatveMiktarli> V15_GetStokListFiyatliItemsWithFullText(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende)
	{
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Expected O, but got Unknown
		string text = "SELECT STOKLAR.sto_RECno,STOKLAR.sto_kod,STOKLAR.sto_isim,STOKLAR.sto_yabanci_isim,STOKLAR.sto_kisa_ismi,STOKLAR.sto_perakende_vergi,STOKLAR.sto_toptan_vergi,STOKLAR.sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_al_sip_birim,sto_anagrup_kod,sto_altgrup_kod,sto_uretici_kodu,sto_reyon_kodu,sto_marka_kodu FROM STOKLAR ";
		string text2 = " WHERE";
		string[] array = Searchstring.ToUpper().Split(new char[1] { ' ' });
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (text3.Length > 1)
			{
				list.Add(text3 + "*");
			}
		}
		if (list.Count != 0)
		{
			string text4 = "";
			bool flag = true;
			foreach (string item in list)
			{
				if (!flag)
				{
					text4 += " ";
				}
				text4 += item;
				flag = false;
			}
			string text5 = "%" + Searchstring.ToUpper().Replace(" ", "%") + "%";
			text = text + text2 + " (STOKLAR.rowid in (  SELECT docid FROM STOKLAR_fts WHERE STOKLAR_fts MATCH '" + text4 + "' LIMIT 200)";
			if (BarkodlardaAra)
			{
				text = text + " OR sto_kod in (SELECT bar_stokkodu FROM BARKOD_TANIMLARI WHERE bar_kodu like '" + text5 + "')";
			}
			text += ")";
			text2 = " AND";
		}
		switch (WebSayfasinaGonderilecekler)
		{
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=1";
			text2 = " AND";
			break;
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilmeyecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=0";
			text2 = " AND";
			break;
		}
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			text = text + text2 + " sto_siparis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		}
		switch (StokListelemeSecenek)
		{
		case enum_StokListelemeSecenekleri.AnaAltGrubaGore:
			text = text + text2 + " sto_anagrup_kod in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			if (StokListelemeAktifAltGrup != "")
			{
				text = text + text2 + " sto_altgrup_kod in ('" + StokListelemeAktifAltGrup.Replace(",", "','") + "')";
				text2 = " AND";
			}
			break;
		case enum_StokListelemeSecenekleri.UreticilereGore:
			text = text + text2 + " sto_uretici_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.KategorilereGore:
			text = text + text2 + " sto_kategori_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.ReyonlaraGore:
			text = text + text2 + " sto_reyon_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.MarkalaraGore:
			text = text + text2 + " sto_marka_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		}
		switch (SiralamaSekli)
		{
		case enum_StokSiralamaSekli.stok_kodu:
			text += " ORDER BY sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.stok_ismi:
			text += " ORDER BY sto_isim_upper ";
			break;
		case enum_StokSiralamaSekli.ana_alt_grub:
			text += " ORDER BY sto_anagrup_kod,sto_altgrup_kod ";
			break;
		case enum_StokSiralamaSekli.kategoriler:
			text += " ORDER BY sto_kategori_kodu ";
			break;
		case enum_StokSiralamaSekli.markalar:
			text += " ORDER BY sto_marka_kodu ";
			break;
		case enum_StokSiralamaSekli.reyonlar:
			text += " ORDER BY sto_reyon_kodu ";
			break;
		case enum_StokSiralamaSekli.ureticiler:
			text += " ORDER BY sto_uretici_kodu ";
			break;
		}
		text += " LIMIT 1000";
		List<StokListItemFiyatveMiktarli> list2 = new List<StokListItemFiyatveMiktarli>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list2.Add(new StokListItemFiyatveMiktarli(val2.GetSafeInt32(0), val2.GetSafeString(1), val2.GetSafeString(2), val2.GetSafeString(3), val2.GetSafeString(4), val2.GetSafeInt16(5), val2.GetSafeInt16(6), val2.GetSafeString(7), val2.GetSafeDouble(8), val2.GetSafeString(9), val2.GetSafeDouble(10), val2.GetSafeString(11), val2.GetSafeDouble(12), val2.GetSafeString(13), val2.GetSafeDouble(14), val2.GetSafeInt16(15), val2.GetSafeString(16), val2.GetSafeString(17), val2.GetSafeString(18), val2.GetSafeString(19), val2.GetSafeString(20), toptan_perakende));
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
		return list2;
	}

	public static List<StokListItemFiyatveMiktarli> V16_GetStokListFiyatliItemsWithFullText(SqliteConnection OpenedConnection, string Searchstring, enum_StokListelemeSecenekleri StokListelemeSecenek, string StokListelemeAktifGrup, string StokListelemeAktifAltGrup, enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinaGonderilecekler, bool BarkodlardaAra, enum_StokSiralamaSekli SiralamaSekli, enum_GenelEvrakTipleri evraktipi, enum_toptan_perakende toptan_perakende)
	{
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Expected O, but got Unknown
		string text = "SELECT STOKLAR.sto_Guid,STOKLAR.sto_kod,STOKLAR.sto_isim,STOKLAR.sto_yabanci_isim,STOKLAR.sto_kisa_ismi,STOKLAR.sto_perakende_vergi,STOKLAR.sto_toptan_vergi,STOKLAR.sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_al_sip_birim,sto_anagrup_kod,sto_altgrup_kod,sto_uretici_kodu,sto_reyon_kodu,sto_marka_kodu FROM STOKLAR ";
		string text2 = " WHERE";
		string[] array = Searchstring.ToUpper().Split(new char[1] { ' ' });
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (text3.Length > 1)
			{
				list.Add(text3 + "*");
			}
		}
		if (list.Count != 0)
		{
			string text4 = "";
			bool flag = true;
			foreach (string item in list)
			{
				if (!flag)
				{
					text4 += " ";
				}
				text4 += item;
				flag = false;
			}
			string text5 = "%" + Searchstring.ToUpper().Replace(" ", "%") + "%";
			text = text + text2 + " (STOKLAR.rowid in (  SELECT docid FROM STOKLAR_fts WHERE STOKLAR_fts MATCH '" + text4 + "' LIMIT 200)";
			if (BarkodlardaAra)
			{
				text = text + " OR sto_kod in (SELECT bar_stokkodu FROM BARKOD_TANIMLARI WHERE bar_kodu like '" + text5 + "')";
			}
			text += ")";
			text2 = " AND";
		}
		switch (WebSayfasinaGonderilecekler)
		{
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=1";
			text2 = " AND";
			break;
		case enum_StokListelemeWebSayfasinaGonderilecek.WebSayfasinaGonderilmeyecekler:
			text = text + text2 + " sto_webe_gonderilecek_fl=0";
			text2 = " AND";
			break;
		}
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			text = text + text2 + " sto_siparis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			text = text + text2 + " sto_malkabul_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			text = text + text2 + " sto_satis_dursun=0";
			text2 = " AND";
			break;
		}
		switch (StokListelemeSecenek)
		{
		case enum_StokListelemeSecenekleri.AnaAltGrubaGore:
			text = text + text2 + " sto_anagrup_kod in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			if (StokListelemeAktifAltGrup != "")
			{
				text = text + text2 + " sto_altgrup_kod in ('" + StokListelemeAktifAltGrup.Replace(",", "','") + "')";
				text2 = " AND";
			}
			break;
		case enum_StokListelemeSecenekleri.UreticilereGore:
			text = text + text2 + " sto_uretici_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.KategorilereGore:
			text = text + text2 + " sto_kategori_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.ReyonlaraGore:
			text = text + text2 + " sto_reyon_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		case enum_StokListelemeSecenekleri.MarkalaraGore:
			text = text + text2 + " sto_marka_kodu in ('" + StokListelemeAktifGrup.Replace(",", "','") + "')";
			text2 = " AND";
			break;
		}
		switch (SiralamaSekli)
		{
		case enum_StokSiralamaSekli.stok_kodu:
			text += " ORDER BY sto_kod_upper ";
			break;
		case enum_StokSiralamaSekli.stok_ismi:
			text += " ORDER BY sto_isim_upper ";
			break;
		case enum_StokSiralamaSekli.ana_alt_grub:
			text += " ORDER BY sto_anagrup_kod,sto_altgrup_kod ";
			break;
		case enum_StokSiralamaSekli.kategoriler:
			text += " ORDER BY sto_kategori_kodu ";
			break;
		case enum_StokSiralamaSekli.markalar:
			text += " ORDER BY sto_marka_kodu ";
			break;
		case enum_StokSiralamaSekli.reyonlar:
			text += " ORDER BY sto_reyon_kodu ";
			break;
		case enum_StokSiralamaSekli.ureticiler:
			text += " ORDER BY sto_uretici_kodu ";
			break;
		}
		text += " LIMIT 1000";
		List<StokListItemFiyatveMiktarli> list2 = new List<StokListItemFiyatveMiktarli>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = text;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list2.Add(new StokListItemFiyatveMiktarli(((DbDataReader)val2).GetGuid(0), val2.GetSafeString(1), val2.GetSafeString(2), val2.GetSafeString(3), val2.GetSafeString(4), val2.GetSafeInt16(5), val2.GetSafeInt16(6), val2.GetSafeString(7), val2.GetSafeDouble(8), val2.GetSafeString(9), val2.GetSafeDouble(10), val2.GetSafeString(11), val2.GetSafeDouble(12), val2.GetSafeString(13), val2.GetSafeDouble(14), val2.GetSafeInt16(15), val2.GetSafeString(16), val2.GetSafeString(17), val2.GetSafeString(18), val2.GetSafeString(19), val2.GetSafeString(20), toptan_perakende));
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
		return list2;
	}

	public static FiyatTanimlamasi GetFiyatFromMultiSource(SqliteConnection OpenedConnection, string stokkodu, int vergi_pntr, int fiyatlisteno, bool sfl_kdvdahil, string carikodu, string anacarikodu, string cari_satis_isk_kod, int depono, DateTime tarih, List<enum_Fiyat_Kaynagi> FiyatKaynaklari, List<VergiTanimi> vergitanimi, int odeme_plan)
	{
		FiyatTanimlamasi fiyatTanimlamasi = null;
		using (List<enum_Fiyat_Kaynagi>.Enumerator enumerator = FiyatKaynaklari.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case enum_Fiyat_Kaynagi.SatisSarti:
					fiyatTanimlamasi = GetFiyatFromSatisSarti(OpenedConnection, stokkodu, carikodu, depono, tarih, odeme_plan);
					break;
				case enum_Fiyat_Kaynagi.FiyatListesi:
					fiyatTanimlamasi = GetFiyatFromFiyatListesi(OpenedConnection, stokkodu, vergi_pntr, fiyatlisteno, sfl_kdvdahil, carikodu, cari_satis_isk_kod, depono, odeme_plan, vergitanimi);
					break;
				case enum_Fiyat_Kaynagi.CariyeSonSatisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(OpenedConnection, stokkodu, enum_SatisAlis.Satis, carikodu);
					break;
				case enum_Fiyat_Kaynagi.GenelSonSatisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(OpenedConnection, stokkodu, enum_SatisAlis.Satis);
					break;
				case enum_Fiyat_Kaynagi.CaridenSonAlisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(OpenedConnection, stokkodu, enum_SatisAlis.Alis, carikodu);
					break;
				case enum_Fiyat_Kaynagi.GenelSonAlisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(OpenedConnection, stokkodu, enum_SatisAlis.Alis);
					break;
				case enum_Fiyat_Kaynagi.AlisSarti:
					fiyatTanimlamasi = GetFiyatFromAlisSarti(OpenedConnection, stokkodu, carikodu, depono, tarih);
					break;
				case enum_Fiyat_Kaynagi.StokStandartMaliyeti:
					fiyatTanimlamasi = GetFiyatFromStokStandartMaliyeti(OpenedConnection, stokkodu);
					break;
				case enum_Fiyat_Kaynagi.AnaCariSatisSarti:
					fiyatTanimlamasi = GetFiyatFromSatisSarti(OpenedConnection, stokkodu, anacarikodu, depono, tarih, odeme_plan);
					break;
				}
				if (fiyatTanimlamasi != null && fiyatTanimlamasi.FiyatBrut != 0.0)
				{
					break;
				}
			}
		}
		if (fiyatTanimlamasi == null)
		{
			fiyatTanimlamasi = new FiyatTanimlamasi();
			fiyatTanimlamasi.DovizCinsi = 0;
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromSatisSarti(SqliteConnection OpenedConnection, string stokkodu, string carikodu, int depono, DateTime tarih, int sat_odeme_plan)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		FiyatTanimlamasi fiyatTanimlamasi = null;
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				Connection = OpenedConnection
			};
			string text = "SELECT sat_brut_fiyat,sat_doviz_cinsi,sat_det_isk_durum1,sat_det_isk_durum2,sat_det_isk_durum3,sat_det_isk_durum4,sat_det_isk_durum5,sat_det_isk_durum6,sat_det_isk_uyg1,sat_det_isk_uyg2,sat_det_isk_uyg3,sat_det_isk_uyg4,sat_det_isk_uyg5,sat_det_isk_uyg6,sat_det_isk_yuzde1,sat_det_isk_yuzde2,sat_det_isk_yuzde3,sat_det_isk_yuzde4,sat_det_isk_yuzde5,sat_det_isk_yuzde6,sat_det_isk_miktar1,sat_det_isk_miktar2,sat_det_isk_miktar3,sat_det_isk_miktar4,sat_det_isk_miktar5,sat_det_isk_miktar6,sat_det_mas_durum1,sat_det_mas_durum2,sat_det_mas_durum3,sat_det_mas_durum4,sat_det_mas_uyg1,sat_det_mas_uyg2,sat_det_mas_uyg3,sat_det_mas_uyg4,sat_det_mas_yuzde1,sat_det_mas_yuzde2,sat_det_mas_yuzde3,sat_det_mas_yuzde4,sat_det_mas_miktar1,sat_det_mas_miktar2,sat_det_mas_miktar3,sat_det_mas_miktar4 FROM SATIS_SARTLARI";
			text += " WHERE sat_stok_kod=@sto_kod AND sat_cari_kod=@cari_kod AND @tarih BETWEEN sat_basla_tarih AND sat_bitis_tarih AND ((sat_depo_no=@depo) OR sat_depo_no=0) AND sat_odeme_plan=@sat_odeme_plan ORDER BY sat_evrak_tarih DESC,sat_evrak_tarih DESC";
			((DbCommand)val).CommandText = text;
			val.Parameters.AddWithValue("@sto_kod", (object)stokkodu);
			val.Parameters.AddWithValue("@cari_kod", (object)carikodu);
			val.Parameters.AddWithValue("@depo", (object)depono);
			val.Parameters.AddWithValue("@tarih", (object)tarih);
			val.Parameters.AddWithValue("@sat_odeme_plan", (object)sat_odeme_plan);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				fiyatTanimlamasi = new FiyatTanimlamasi();
				fiyatTanimlamasi.BeginInit();
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.SatisSarti;
				fiyatTanimlamasi.FiyatBrut = val2.GetSafeDouble(0);
				fiyatTanimlamasi.DovizCinsi = val2.GetSafeInt16(1);
				fiyatTanimlamasi.Iskonto_1_UygulamaSekli = val2.GetSafeInt16(8);
				fiyatTanimlamasi.Iskonto_2_UygulamaSekli = val2.GetSafeInt16(9);
				fiyatTanimlamasi.Iskonto_3_UygulamaSekli = val2.GetSafeInt16(10);
				fiyatTanimlamasi.Iskonto_4_UygulamaSekli = val2.GetSafeInt16(11);
				fiyatTanimlamasi.Iskonto_5_UygulamaSekli = val2.GetSafeInt16(12);
				fiyatTanimlamasi.Iskonto_6_UygulamaSekli = val2.GetSafeInt16(13);
				if (fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = val2.GetSafeDouble(14);
				}
				else
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = val2.GetSafeDouble(20);
				}
				if (fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = val2.GetSafeDouble(15);
				}
				else
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = val2.GetSafeDouble(21);
				}
				if (fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = val2.GetSafeDouble(16);
				}
				else
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = val2.GetSafeDouble(22);
				}
				if (fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = val2.GetSafeDouble(17);
				}
				else
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = val2.GetSafeDouble(23);
				}
				if (fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = val2.GetSafeDouble(18);
				}
				else
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = val2.GetSafeDouble(24);
				}
				if (fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = val2.GetSafeDouble(19);
				}
				else
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = val2.GetSafeDouble(25);
				}
				fiyatTanimlamasi.Masraf_1_UygulamaSekli = val2.GetSafeInt16(30);
				fiyatTanimlamasi.Masraf_2_UygulamaSekli = val2.GetSafeInt16(31);
				fiyatTanimlamasi.Masraf_3_UygulamaSekli = val2.GetSafeInt16(32);
				fiyatTanimlamasi.Masraf_4_UygulamaSekli = val2.GetSafeInt16(33);
				if (fiyatTanimlamasi.Masraf_1_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = val2.GetSafeDouble(34);
				}
				else
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = val2.GetSafeDouble(38);
				}
				if (fiyatTanimlamasi.Masraf_2_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = val2.GetSafeDouble(35);
				}
				else
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = val2.GetSafeDouble(39);
				}
				if (fiyatTanimlamasi.Masraf_3_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = val2.GetSafeDouble(36);
				}
				else
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = val2.GetSafeDouble(40);
				}
				if (fiyatTanimlamasi.Masraf_4_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = val2.GetSafeDouble(37);
				}
				else
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = val2.GetSafeDouble(41);
				}
				fiyatTanimlamasi.EndInit();
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromAlisSarti(SqliteConnection OpenedConnection, string stokkodu, string carikodu, int depono, DateTime tarih)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		FiyatTanimlamasi fiyatTanimlamasi = null;
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				Connection = OpenedConnection,
				CommandText = "SELECT sas_brut_fiyat,sas_doviz_cinsi,sas_isk_durum1,sas_isk_durum2,sas_isk_durum3,sas_isk_durum4,sas_isk_durum5,sas_isk_durum6,sas_isk_uyg1,sas_isk_uyg2,sas_isk_uyg3,sas_isk_uyg4,sas_isk_uyg5,sas_isk_uyg6,sas_isk_yuzde1,sas_isk_yuzde2,sas_isk_yuzde3,sas_isk_yuzde4,sas_isk_yuzde5,sas_isk_yuzde6,sas_isk_miktar1,sas_isk_miktar2,sas_isk_miktar3,sas_isk_miktar4,sas_isk_miktar5,sas_isk_miktar6,sas_mas_durum1,sas_mas_durum2,sas_mas_durum3,sas_mas_durum4,sas_mas_uyg1,sas_mas_uyg2,sas_mas_uyg3,sas_mas_uyg4,sas_mas_yuzde1,sas_mas_yuzde2,sas_mas_yuzde3,sas_mas_yuzde4,sas_mas_miktar1,sas_mas_miktar2,sas_mas_miktar3,sas_mas_miktar4 FROM SATINALMA_SARTLARI WHERE sas_stok_kod=@sto_kod AND sas_cari_kod=@cari_kod AND @tarih BETWEEN sas_basla_tarih AND sas_bitis_tarih AND ((sas_depo_no=@depo) OR sas_depo_no=0) ORDER BY sas_evrak_tarih DESC,sas_evrak_tarih DESC"
			};
			val.Parameters.AddWithValue("@sto_kod", (object)stokkodu);
			val.Parameters.AddWithValue("@cari_kod", (object)carikodu);
			val.Parameters.AddWithValue("@depo", (object)depono);
			val.Parameters.AddWithValue("@tarih", (object)tarih);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				fiyatTanimlamasi = new FiyatTanimlamasi();
				fiyatTanimlamasi.BeginInit();
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.SatisSarti;
				fiyatTanimlamasi.FiyatBrut = double.Parse(((DbDataReader)val2)[0].ToString());
				fiyatTanimlamasi.DovizCinsi = int.Parse(((DbDataReader)val2)[1].ToString());
				fiyatTanimlamasi.Iskonto_1_UygulamaSekli = int.Parse(((DbDataReader)val2)[8].ToString());
				fiyatTanimlamasi.Iskonto_2_UygulamaSekli = int.Parse(((DbDataReader)val2)[9].ToString());
				fiyatTanimlamasi.Iskonto_3_UygulamaSekli = int.Parse(((DbDataReader)val2)[10].ToString());
				fiyatTanimlamasi.Iskonto_4_UygulamaSekli = int.Parse(((DbDataReader)val2)[11].ToString());
				fiyatTanimlamasi.Iskonto_5_UygulamaSekli = int.Parse(((DbDataReader)val2)[12].ToString());
				fiyatTanimlamasi.Iskonto_6_UygulamaSekli = int.Parse(((DbDataReader)val2)[13].ToString());
				if (fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[14].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[20].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[15].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[21].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[16].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[22].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[17].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[23].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[18].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[24].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[19].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[25].ToString());
				}
				fiyatTanimlamasi.Masraf_1_UygulamaSekli = int.Parse(((DbDataReader)val2)[30].ToString());
				fiyatTanimlamasi.Masraf_2_UygulamaSekli = int.Parse(((DbDataReader)val2)[31].ToString());
				fiyatTanimlamasi.Masraf_3_UygulamaSekli = int.Parse(((DbDataReader)val2)[32].ToString());
				fiyatTanimlamasi.Masraf_4_UygulamaSekli = int.Parse(((DbDataReader)val2)[33].ToString());
				if (fiyatTanimlamasi.Masraf_1_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[34].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[38].ToString());
				}
				if (fiyatTanimlamasi.Masraf_2_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[35].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[39].ToString());
				}
				if (fiyatTanimlamasi.Masraf_3_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[36].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[40].ToString());
				}
				if (fiyatTanimlamasi.Masraf_4_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[37].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = double.Parse(((DbDataReader)val2)[41].ToString());
				}
				fiyatTanimlamasi.EndInit();
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromFiyatListesi(SqliteConnection OpenedConnection, string stokkodu, int vergi_pntr, int fiyatlisteno, bool sfl_kdvdahil, string carikodu, string cari_satis_isk_kod, int depono, int odeme_plani, List<VergiTanimi> vergitanimlari)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.BeginInit();
		fiyatTanimlamasi.DovizCinsi = 0;
		try
		{
			string text = "";
			SqliteCommand val = new SqliteCommand();
			val.Connection = OpenedConnection;
			string text2 = "";
			if (odeme_plani > 0)
			{
				text2 = " DESC";
			}
			string text3 = "SELECT sfiyat_fiyati,sfiyat_doviz,sfiyat_iskontokod FROM STOK_SATIS_FIYAT_LISTELERI WHERE sfiyat_stokkod=@sto_kod AND sfiyat_listesirano=@fiyatlistesirano";
			text3 = text3 + " AND (sfiyat_deposirano=@depo OR sfiyat_deposirano=0) AND (sfiyat_odemeplan=@sfiyat_odemeplan OR sfiyat_odemeplan=0) ORDER BY sfiyat_odemeplan" + text2 + ", sfiyat_deposirano DESC";
			((DbCommand)val).CommandText = text3;
			val.Parameters.AddWithValue("@sto_kod", (object)stokkodu);
			val.Parameters.AddWithValue("@fiyatlistesirano", (object)fiyatlisteno);
			val.Parameters.AddWithValue("@depo", (object)depono);
			val.Parameters.AddWithValue("@sfiyat_odemeplan", (object)odeme_plani);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.FiyatListesi;
				fiyatTanimlamasi.FiyatBrut = val2.GetSafeDouble(0);
				if (sfl_kdvdahil)
				{
					double yuzde = vergitanimlari[vergi_pntr].Yuzde;
					fiyatTanimlamasi.FiyatBrut /= yuzde / 100.0 + 1.0;
				}
				fiyatTanimlamasi.DovizCinsi = val2.GetSafeInt16(1);
				text = val2.GetSafeString(2);
				if (fiyatTanimlamasi.FiyatBrut != 0.0)
				{
					break;
				}
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
			if (text != "")
			{
				val = new SqliteCommand();
				val.Connection = OpenedConnection;
				text2 = "";
				if (odeme_plani > 0)
				{
					text2 = " DESC";
				}
				((DbCommand)val).CommandText = "SELECT isk_uygulama_odeme_plani,isk_isk1_uygulama,isk_isk2_uygulama,isk_isk3_uygulama,isk_isk4_uygulama,isk_isk5_uygulama,isk_isk6_uygulama,isk_isk1_yuzde,isk_isk2_yuzde,isk_isk3_yuzde,isk_isk4_yuzde,isk_isk5_yuzde,isk_isk6_yuzde,isk_mas1_uygulama,isk_mas2_uygulama,isk_mas3_uygulama,isk_mas4_uygulama,isk_mas1_yuzde,isk_mas2_yuzde,isk_mas3_yuzde,isk_mas4_yuzde FROM STOK_CARI_ISKONTO_TANIMLARI WHERE isk_stok_kod=@stok_iskonto_kodu AND isk_cari_kod=@cari_iskonto_kodu AND (isk_uygulama_odeme_plani=@isk_uygulama_odeme_plani OR isk_uygulama_odeme_plani=0) ORDER BY isk_uygulama_odeme_plani" + text2;
				val.Parameters.AddWithValue("@stok_iskonto_kodu", (object)text);
				val.Parameters.AddWithValue("@cari_iskonto_kodu", (object)cari_satis_isk_kod);
				val.Parameters.AddWithValue("@isk_uygulama_odeme_plani", (object)odeme_plani);
				val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					fiyatTanimlamasi.Iskonto_1_UygulamaSekli = val2.GetSafeInt16(1);
					fiyatTanimlamasi.Iskonto_2_UygulamaSekli = val2.GetSafeInt16(2);
					fiyatTanimlamasi.Iskonto_3_UygulamaSekli = val2.GetSafeInt16(3);
					fiyatTanimlamasi.Iskonto_4_UygulamaSekli = val2.GetSafeInt16(4);
					fiyatTanimlamasi.Iskonto_5_UygulamaSekli = val2.GetSafeInt16(5);
					fiyatTanimlamasi.Iskonto_6_UygulamaSekli = val2.GetSafeInt16(6);
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = val2.GetSafeDouble(7);
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = val2.GetSafeDouble(8);
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = val2.GetSafeDouble(9);
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = val2.GetSafeDouble(10);
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = val2.GetSafeDouble(11);
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = val2.GetSafeDouble(12);
					fiyatTanimlamasi.Masraf_1_UygulamaSekli = val2.GetSafeInt16(13);
					fiyatTanimlamasi.Masraf_2_UygulamaSekli = val2.GetSafeInt16(14);
					fiyatTanimlamasi.Masraf_3_UygulamaSekli = val2.GetSafeInt16(15);
					fiyatTanimlamasi.Masraf_4_UygulamaSekli = val2.GetSafeInt16(16);
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = val2.GetSafeDouble(17);
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = val2.GetSafeDouble(18);
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = val2.GetSafeDouble(19);
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = val2.GetSafeDouble(20);
				}
				((DbDataReader)val2).Close();
				((DbDataReader)val2).Dispose();
				val2 = null;
				((Component)val).Dispose();
				val = null;
			}
		}
		catch
		{
		}
		fiyatTanimlamasi.EndInit();
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromSonFiyati(SqliteConnection OpenedConnection, string stokkodu, enum_SatisAlis SatisAlis)
	{
		return GetFiyatFromSonFiyati(OpenedConnection, stokkodu, SatisAlis, "");
	}

	public static FiyatTanimlamasi GetFiyatFromSonFiyati(SqliteConnection OpenedConnection, string stokkodu, enum_SatisAlis SatisAlis, string carikodu)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		try
		{
			string text = "";
			if (carikodu != "")
			{
				text = " AND sth_cari_kodu=@sth_cari_kodu";
			}
			string commandText = "SELECT sth_tutar,sth_iskonto1,sth_iskonto2,sth_iskonto3,sth_iskonto4,sth_iskonto5,sth_iskonto6,sth_miktar, sth_har_doviz_cinsi FROM STOK_HAREKETLERI WHERE sth_stok_kod=@sto_kod" + text + " AND sth_tip=@sth_tip AND sth_normal_iade=0 ORDER BY sth_tarih DESC LIMIT 5";
			SqliteCommand val = new SqliteCommand();
			val.Connection = OpenedConnection;
			((DbCommand)val).CommandText = commandText;
			switch (SatisAlis)
			{
			case enum_SatisAlis.Satis:
				val.Parameters.AddWithValue("@sth_tip", (object)1);
				break;
			case enum_SatisAlis.Alis:
				val.Parameters.AddWithValue("@sth_tip", (object)0);
				break;
			}
			val.Parameters.AddWithValue("@sto_kod", (object)stokkodu);
			if (carikodu != "")
			{
				val.Parameters.AddWithValue("@sth_cari_kodu", (object)carikodu);
			}
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				fiyatTanimlamasi.FiyatBrut = (val2.GetSafeDouble(0) - val2.GetSafeDouble(1) - val2.GetSafeDouble(2) - val2.GetSafeDouble(3) - val2.GetSafeDouble(4) - val2.GetSafeDouble(5) - val2.GetSafeDouble(6)) / val2.GetSafeDouble(7);
				fiyatTanimlamasi.DovizCinsi = int.Parse(((DbDataReader)val2)[8].ToString());
				if (fiyatTanimlamasi.FiyatBrut == 0.0)
				{
					continue;
				}
				if (carikodu != "")
				{
					switch (SatisAlis)
					{
					case enum_SatisAlis.Satis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.CariyeSonSatisFiyati;
						break;
					case enum_SatisAlis.Alis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.CaridenSonAlisFiyati;
						break;
					}
				}
				else
				{
					switch (SatisAlis)
					{
					case enum_SatisAlis.Satis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.GenelSonSatisFiyati;
						break;
					case enum_SatisAlis.Alis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.GenelSonAlisFiyati;
						break;
					}
				}
				break;
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return fiyatTanimlamasi;
	}

	public static DateTime GetFiyatFromSonFiyatiTarihi(SqliteConnection OpenedConnection, string stokkodu, enum_SatisAlis SatisAlis)
	{
		return GetFiyatFromSonFiyatiTarihi(OpenedConnection, stokkodu, SatisAlis, "");
	}

	public static DateTime GetFiyatFromSonFiyatiTarihi(SqliteConnection OpenedConnection, string stokkodu, enum_SatisAlis SatisAlis, string carikodu)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		DateTime result = DateTime.Now;
		try
		{
			string text = "";
			if (carikodu != "")
			{
				text = " AND sth_cari_kodu=@sth_cari_kodu";
			}
			string commandText = "SELECT sth_tarih FROM STOK_HAREKETLERI WHERE sth_stok_kod=@sto_kod" + text + " AND sth_tip=@sth_tip AND sth_normal_iade=0 ORDER BY sth_tarih DESC LIMIT 1";
			SqliteCommand val = new SqliteCommand();
			val.Connection = OpenedConnection;
			((DbCommand)val).CommandText = commandText;
			switch (SatisAlis)
			{
			case enum_SatisAlis.Satis:
				val.Parameters.AddWithValue("@sth_tip", (object)1);
				break;
			case enum_SatisAlis.Alis:
				val.Parameters.AddWithValue("@sth_tip", (object)0);
				break;
			}
			val.Parameters.AddWithValue("@sto_kod", (object)stokkodu);
			if (carikodu != "")
			{
				val.Parameters.AddWithValue("@sth_cari_kodu", (object)carikodu);
			}
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				result = val2.GetSafeDateTime(0);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static FiyatTanimlamasi GetFiyatFromStokStandartMaliyeti(SqliteConnection OpenedConnection, string stokkodu)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				Connection = OpenedConnection,
				CommandText = "SELECT sto_standartmaliyet,sto_doviz_cinsi FROM STOKLAR WHERE sto_kod=@sto_kod"
			};
			val.Parameters.AddWithValue("@sto_kod", (object)stokkodu);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.StokStandartMaliyeti;
				fiyatTanimlamasi.FiyatBrut = double.Parse(((DbDataReader)val2)[0].ToString());
				fiyatTanimlamasi.DovizCinsi = int.Parse(((DbDataReader)val2)[1].ToString());
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return fiyatTanimlamasi;
	}

	public static List<string> GetStokBarkodlari(SqliteConnection OpenedConnection, string sto_kod)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		string text = "SELECT bar_kodu,bar_birimpntr FROM BARKOD_TANIMLARI WHERE bar_stokkodu=@sto_kod AND bar_baglantitipi=0";
		List<string> list = new List<string>();
		try
		{
			SqliteCommand val = new SqliteCommand(text, OpenedConnection);
			val.Parameters.AddWithValue("@sto_kod", (object)sto_kod);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeByte(1) + "|" + val2.GetSafeString(0));
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

	public static List<BEDEN_HAREKETLERI> GetStokRenkBedenDepoDurumu(SqliteConnection OpenedConnection, string sto_kod, int depo_no)
	{
		if (AppBase.MikroVersiyonu > 15)
		{
			return V16_GetStokRenkBedenDepoDurumu(OpenedConnection, sto_kod, depo_no);
		}
		return V15_GetStokRenkBedenDepoDurumu(OpenedConnection, sto_kod, depo_no);
	}

	public static List<BEDEN_HAREKETLERI> V15_GetStokRenkBedenDepoDurumu(SqliteConnection OpenedConnection, string sto_kod, int depo_no)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT B.BdnHar_BedenNo, SUM(CASE WHEN(sth_tip = 0) OR((sth_tip = 2) AND(sth_giris_depo_no = @depo)) THEN B.BdnHar_HarGor WHEN(sth_tip = 1) OR((sth_tip = 2) AND(sth_cikis_depo_no = @depo)) THEN(-1) * B.BdnHar_HarGor ELSE 0 END ) FROM STOK_HAREKETLERI AS A INNER JOIN BEDEN_HAREKETLERI AS B ON (A.sth_RECno = B.BdnHar_DRECid_RECno) AND (B.BdnHar_Tipi = 11) WHERE(sth_stok_kod = @stok_kodu) AND (sth_miktar <> 0) AND (((sth_tip = 0) and((sth_giris_depo_no = @depo) OR(@depo = 0))) OR ((sth_tip = 1) and((sth_cikis_depo_no = @depo) OR(@depo = 0))) OR ((sth_tip = 2) AND(sth_giris_depo_no = @depo) AND(sth_giris_depo_no <> sth_cikis_depo_no)) OR ((sth_tip = 2) AND(sth_cikis_depo_no = @depo) AND(sth_giris_depo_no <> sth_cikis_depo_no)) ) AND (not(sth_cins in (9, 15))) GROUP BY B.BdnHar_BedenNo";
		List<BEDEN_HAREKETLERI> list = new List<BEDEN_HAREKETLERI>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Parameters.AddWithValue("@stok_kodu", (object)sto_kod);
			val.Parameters.AddWithValue("@depo", (object)depo_no);
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
				bEDEN_HAREKETLERI.BdnHar_BedenNo = val2.GetSafeInt32(0);
				bEDEN_HAREKETLERI.BdnHar_HarGor = val2.GetSafeDouble(1);
				list.Add(bEDEN_HAREKETLERI);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception)
		{
		}
		return list;
	}

	public static List<BEDEN_HAREKETLERI> V16_GetStokRenkBedenDepoDurumu(SqliteConnection OpenedConnection, string sto_kod, int depo_no)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT B.BdnHar_BedenNo, SUM(CASE WHEN(sth_tip = 0) OR((sth_tip = 2) AND(sth_giris_depo_no = @depo)) THEN B.BdnHar_HarGor WHEN(sth_tip = 1) OR((sth_tip = 2) AND(sth_cikis_depo_no = @depo)) THEN(-1) * B.BdnHar_HarGor ELSE 0 END ) FROM STOK_HAREKETLERI AS A INNER JOIN BEDEN_HAREKETLERI AS B ON (A.sth_Guid = B.BdnHar_Har_uid) AND (B.BdnHar_Tipi = 11) WHERE(sth_stok_kod = @stok_kodu) AND (sth_miktar <> 0) AND (((sth_tip = 0) and((sth_giris_depo_no = @depo) OR(@depo = 0))) OR ((sth_tip = 1) and((sth_cikis_depo_no = @depo) OR(@depo = 0))) OR ((sth_tip = 2) AND(sth_giris_depo_no = @depo) AND(sth_giris_depo_no <> sth_cikis_depo_no)) OR ((sth_tip = 2) AND(sth_cikis_depo_no = @depo) AND(sth_giris_depo_no <> sth_cikis_depo_no)) ) AND (not(sth_cins in (9, 15))) GROUP BY B.BdnHar_BedenNo";
		List<BEDEN_HAREKETLERI> list = new List<BEDEN_HAREKETLERI>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Parameters.AddWithValue("@stok_kodu", (object)sto_kod);
			val.Parameters.AddWithValue("@depo", (object)depo_no);
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
				bEDEN_HAREKETLERI.BdnHar_BedenNo = val2.GetSafeInt32(0);
				bEDEN_HAREKETLERI.BdnHar_HarGor = val2.GetSafeDouble(1);
				list.Add(bEDEN_HAREKETLERI);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception)
		{
		}
		return list;
	}
}
