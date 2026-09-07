using System;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Enumler;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Temsilciler;

public static class GuneBaslaBitirSqlite
{
	public static bool GuneBasla(SqliteConnection connection, string AracKilometre, string CariPersonelKodu, string Mesaj)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		GuneBaslaBitir guneBaslaBitir = new GuneBaslaBitir();
		guneBaslaBitir.Tipi = enum_GuneBaslaBitir.GuneBasla;
		guneBaslaBitir.Arac_Km = int.Parse(AracKilometre);
		guneBaslaBitir.Boylam = 0f;
		guneBaslaBitir.Enlem = 0f;
		guneBaslaBitir.Saati = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		guneBaslaBitir.Saati = guneBaslaBitir.Saati.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
		guneBaslaBitir.Tarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		guneBaslaBitir.Temsilci_Kodu = CariPersonelKodu;
		guneBaslaBitir.Mesaj = Mesaj;
		try
		{
			SqliteCommand val = new SqliteCommand("INSERT INTO [OFFLINE_KAYITLAR] (AktarimDurumu,YeniKayit,AktarilmaTarihi,Tipi,Evrak,HataString) VALUES (@AktarimDurumu,@YeniKayit,@AktarilmaTarihi,@Tipi,@Evrak,@HataString)", connection);
			val.Parameters.AddWithValue("@AktarimDurumu", (object)2);
			val.Parameters.AddWithValue("@YeniKayit", (object)true);
			val.Parameters.AddWithValue("@AktarilmaTarihi", (object)DateTime.Now.AddYears(-10));
			val.Parameters.AddWithValue("@Tipi", (object)2);
			val.Parameters.AddWithValue("@Evrak", (object)GuneBaslaBitir.WriteToByteArray(guneBaslaBitir, 1));
			val.Parameters.AddWithValue("@HataString", (object)"");
			((DbCommand)val).ExecuteScalar();
			((Component)val).Dispose();
		}
		catch
		{
			result = false;
		}
		return result;
	}

	public static bool GunuBitir(SqliteConnection connection, string AracKilometre, string CariPersonelKodu, string Mesaj)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		GuneBaslaBitir guneBaslaBitir = new GuneBaslaBitir();
		guneBaslaBitir.Tipi = enum_GuneBaslaBitir.GunuBitir;
		guneBaslaBitir.Arac_Km = int.Parse(AracKilometre);
		guneBaslaBitir.Boylam = 0f;
		guneBaslaBitir.Enlem = 0f;
		guneBaslaBitir.Saati = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		guneBaslaBitir.Saati = guneBaslaBitir.Saati.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
		guneBaslaBitir.Tarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		guneBaslaBitir.Temsilci_Kodu = CariPersonelKodu;
		guneBaslaBitir.Mesaj = Mesaj;
		try
		{
			SqliteCommand val = new SqliteCommand("INSERT INTO [OFFLINE_KAYITLAR] (AktarimDurumu,YeniKayit,AktarilmaTarihi,Tipi,Evrak,HataString) VALUES (@AktarimDurumu,@YeniKayit,@AktarilmaTarihi,@Tipi,@Evrak,@HataString)", connection);
			val.Parameters.AddWithValue("@AktarimDurumu", (object)2);
			val.Parameters.AddWithValue("@YeniKayit", (object)true);
			val.Parameters.AddWithValue("@AktarilmaTarihi", (object)DateTime.Now.AddYears(-10));
			val.Parameters.AddWithValue("@Tipi", (object)3);
			val.Parameters.AddWithValue("@Evrak", (object)GuneBaslaBitir.WriteToByteArray(guneBaslaBitir, 1));
			val.Parameters.AddWithValue("@HataString", (object)"");
			((DbCommand)val).ExecuteScalar();
			((Component)val).Dispose();
		}
		catch
		{
			result = false;
		}
		return result;
	}
}
