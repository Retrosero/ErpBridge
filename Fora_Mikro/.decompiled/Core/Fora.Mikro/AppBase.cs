using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Database.Sqlite;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Interface;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Vergiler;
using Mono.Data.Sqlite;

namespace Fora.Mikro;

public class AppBase
{
	public static IForaPlatformTools ForaPlatformTools;

	private static SqliteHelper DbConnectionSync;

	public static int MikroVersiyonu;

	public static string DbPath;

	public static string _FirmaID;

	public static string _KullaniciAdi;

	public static string _Sifre;

	public static double _Versiyon;

	public static string _MikroDBName;

	public static string _MikroDBNameHepsi;

	public static Parametreler KullaniciParametreleri;

	public static List<VergiTanimi> _vergitanimlari;

	public static DovizCinsiTanimlari _doviz_cinsi_tanimlari;

	public static int _ana_doviz_cinsi;

	public static string _Action;

	public static int CariDetayAktifTab;

	public static enum_CariListelemeSecenekleri CariListelemeSecenek;

	public static string CariListelemeAktifGrup;

	public static string CariListelemeRotayaGoreTemsilci;

	public static DateTime CariListelemeRotayaGoreTarih;

	public static int StokDetayAktifTab;

	public static enum_StokListelemeSecenekleri StokListelemeSecenek;

	public static string StokListelemeAktifGrup;

	public static string StokListelemeAktifAltGrup;

	public static int Counter;

	public static CultureInfo AppCulture;

	public static double Latitude;

	public static double Longitude;

	public static void InitDbConnectionSync()
	{
		try
		{
			if (DbConnectionSync != null)
			{
				DbConnectionSync.Close();
				DbConnectionSync.Dispose();
				DbConnectionSync = null;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA 435468 : " + ex.ToString());
		}
		DbConnectionSync = new SqliteHelper(GetMikroDBNameWithPath(), ReadOnly: false);
	}

	public static SqliteConnection GetDbConnectionSync()
	{
		if (DbConnectionSync == null)
		{
			DbConnectionSync = new SqliteHelper(GetMikroDBNameWithPath(), ReadOnly: false);
		}
		return DbConnectionSync.GetConnection();
	}

	public static string GetAyarlarDbPath()
	{
		return Path.Combine(DbPath, "ayarlar.db");
	}

	public static string GetFirmalarDbPath()
	{
		return Path.Combine(DbPath, "firmalar_v2.db");
	}

	public static string GetOfflineKayitlarDbPath()
	{
		string text = "";
		int num = 0;
		string[] array = _MikroDBNameHepsi.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length && !(array[i] == _MikroDBName); i++)
		{
			num++;
		}
		if (num > 0)
		{
			text = "_" + _MikroDBName;
		}
		return Path.Combine(DbPath, _FirmaID + text + "_offlineV2.db");
	}

	public static string GetMikroDBNameWithPath()
	{
		return Path.Combine(DbPath, _MikroDBName);
	}

	public static string GetKullaniciAdiSifreHatirlatma()
	{
		return Path.Combine(DbPath, "kullanici.xml");
	}
}
