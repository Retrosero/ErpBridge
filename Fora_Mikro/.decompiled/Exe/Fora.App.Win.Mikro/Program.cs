using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DevExpress.Data;
using Fora.App.Mikro;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Enumler;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;
using Fora.Mikro.Vergiler;
using Fora.Mikro.Win.Form.DevEx;

namespace Fora.App.Win.Mikro;

internal static class Program
{
	public static MikroUygulamaBilgileri _mikrouygulamabilgileri;

	[STAThread]
	private static void Main(string[] args)
	{
		CurrencyDataController.DisableThreadingProblemsDetection = true;
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		if (!Directory.Exists("data"))
		{
			Directory.CreateDirectory("data");
		}
		if (!Directory.Exists("data\\views"))
		{
			Directory.CreateDirectory("data\\views");
		}
		if (!Directory.Exists("data\\raporlar"))
		{
			Directory.CreateDirectory("data\\raporlar");
		}
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "00000000";
		if (args.Length < 3)
		{
			Application.Run(new KurulumAnlatim());
			return;
		}
		text = args[0];
		text2 = args[1];
		text3 = args[2];
		text4 = args[3];
		string mikroanadbname = "";
		switch (text)
		{
		case "12":
			mikroanadbname = "MikroDB_V12";
			AppBase.MikroVersiyonu = 12;
			break;
		case "14":
			mikroanadbname = "MikroDB_V14";
			AppBase.MikroVersiyonu = 14;
			break;
		case "15":
			mikroanadbname = "MikroDB_V15";
			AppBase.MikroVersiyonu = 15;
			break;
		case "16":
			mikroanadbname = "MikroDB_V16";
			AppBase.MikroVersiyonu = 16;
			break;
		}
		_mikrouygulamabilgileri = new MikroUygulamaBilgileri(mikroanadbname);
		_mikrouygulamabilgileri.baglantibilgileri = new SqlBaglantiBilgileri("data\\sqlbaglantibilgileri.xml");
		if (_mikrouygulamabilgileri.baglantibilgileri.SqlServer == "")
		{
			SqlBaglantiAyarlariAc();
		}
		_mikrouygulamabilgileri.veritabani = VeritabaniData.GetVeritabani(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName, text2);
		ParametreData.ForaParametrelerTablosuOlustur(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		_mikrouygulamabilgileri.KullaniciParametreleri = ParametrelerDefault.ForaMikroKullanici(text3);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri, "foramikro", text3, "", "");
		_mikrouygulamabilgileri.mikrokullanici = MikroKullaniciData.GetMikroKullanici(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName, text3);
		_mikrouygulamabilgileri.KullaniciAdi = text3;
		_mikrouygulamabilgileri.ResetGenelParametreler();
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.GenelParametreler, "ForaMikro", "", "", "");
		_mikrouygulamabilgileri.vergitanimlari = new List<VergiTanimi>();
		VergiTanimi vergiTanimi = new VergiTanimi();
		vergiTanimi.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi0KisaAdi")._GetString;
		vergiTanimi.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi0UzunAdi")._GetString;
		vergiTanimi.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi0Yuzde")._GetDouble;
		VergiTanimi vergiTanimi2 = new VergiTanimi();
		vergiTanimi2.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1KisaAdi")._GetString;
		vergiTanimi2.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1UzunAdi")._GetString;
		vergiTanimi2.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1Yuzde")._GetDouble;
		VergiTanimi vergiTanimi3 = new VergiTanimi();
		vergiTanimi3.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2KisaAdi")._GetString;
		vergiTanimi3.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2UzunAdi")._GetString;
		vergiTanimi3.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2Yuzde")._GetDouble;
		VergiTanimi vergiTanimi4 = new VergiTanimi();
		vergiTanimi4.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3KisaAdi")._GetString;
		vergiTanimi4.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3UzunAdi")._GetString;
		vergiTanimi4.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3Yuzde")._GetDouble;
		VergiTanimi vergiTanimi5 = new VergiTanimi();
		vergiTanimi5.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4KisaAdi")._GetString;
		vergiTanimi5.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4UzunAdi")._GetString;
		vergiTanimi5.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4Yuzde")._GetDouble;
		VergiTanimi vergiTanimi6 = new VergiTanimi();
		vergiTanimi6.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5KisaAdi")._GetString;
		vergiTanimi6.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5UzunAdi")._GetString;
		vergiTanimi6.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5Yuzde")._GetDouble;
		VergiTanimi vergiTanimi7 = new VergiTanimi();
		vergiTanimi7.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6KisaAdi")._GetString;
		vergiTanimi7.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6UzunAdi")._GetString;
		vergiTanimi7.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6Yuzde")._GetDouble;
		VergiTanimi vergiTanimi8 = new VergiTanimi();
		vergiTanimi8.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7KisaAdi")._GetString;
		vergiTanimi8.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7UzunAdi")._GetString;
		vergiTanimi8.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7Yuzde")._GetDouble;
		VergiTanimi vergiTanimi9 = new VergiTanimi();
		vergiTanimi9.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8KisaAdi")._GetString;
		vergiTanimi9.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8UzunAdi")._GetString;
		vergiTanimi9.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8Yuzde")._GetDouble;
		VergiTanimi vergiTanimi10 = new VergiTanimi();
		vergiTanimi10.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9KisaAdi")._GetString;
		vergiTanimi10.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9UzunAdi")._GetString;
		vergiTanimi10.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9Yuzde")._GetDouble;
		VergiTanimi vergiTanimi11 = new VergiTanimi();
		vergiTanimi11.KisaAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10KisaAdi")._GetString;
		vergiTanimi11.UzunAdi = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10UzunAdi")._GetString;
		vergiTanimi11.Yuzde = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10Yuzde")._GetDouble;
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi2);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi3);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi4);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi5);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi6);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi7);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi8);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi9);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi10);
		_mikrouygulamabilgileri.vergitanimlari.Add(vergiTanimi11);
		_mikrouygulamabilgileri.doviz_cinsi_tanimlari = DovizCinsiTanimlariData.GetDovizCinsiTanimlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName);
		AppBase._vergitanimlari = _mikrouygulamabilgileri.vergitanimlari;
		List<ForaAppWinAnaMenu> list = new List<ForaAppWinAnaMenu>();
		list.Add(new ForaAppWinAnaMenu("ANA", "00000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Cari hesaplar", "20000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Yeni cari hesap ekle", "20050000", "20000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Kart));
		list.Add(new ForaAppWinAnaMenu("Aktarımlar", "25000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Genel evrak aktarımı", "25050000", "25000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Evrak));
		list.Add(new ForaAppWinAnaMenu("Tahsilat evrak aktarımı", "25060000", "25000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Evrak));
		list.Add(new ForaAppWinAnaMenu("Banka verisi aktarımı", "25100000", "25000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Evrak));
		list.Add(new ForaAppWinAnaMenu("Comarch Edi aktarımı", "25200000", "25000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Evrak));
		list.Add(new ForaAppWinAnaMenu("Depolar", "35000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Raporlar", "35200000", "35000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Depolar arası siparişler", "35201000", "35200000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Açık sipariş analizi (Stok bazlı)", "35201010", "35201000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Açık sipariş analizi (Depo bazlı)", "35201020", "35201000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Kalite kontrol yönetimi", "40000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("İşlem girişi", "40050000", "40000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Evrak));
		list.Add(new ForaAppWinAnaMenu("Kayıt hikayesi", "40100000", "40000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Fora Mikro Mobil", "45000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Raporlar", "45050000", "45000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Temsilci raporu", "45051000", "45050000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Stok satış raporu", "45052000", "45050000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Ziyaret raporu", "45053000", "45050000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Rapor tanımlama", "45060000", "45000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Stok satış raporu tanımlama", "45061000", "45060000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Stok sipariş raporu tanımlama", "45061100", "45060000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Yapılacak tahsilatlar raporu tanımlama", "45061200", "45060000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Stok envanter raporu tanımlama", "45061300", "45060000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Rut", "45080000", "45000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Temsilci rut düzenleme", "45081000", "45080000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Mobil kullanıcı tanımlama", "45100000", "45000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Kart));
		list.Add(new ForaAppWinAnaMenu("Cari ekstre tasarımı", "45110000", "45000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
		list.Add(new ForaAppWinAnaMenu("Teknik servis yönetimi", "46000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Toplu bakım talep evrağı girişi", "46050000", "46000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Zamanlanmış görevler", "55000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Başka firmanın stok miktarını aktar", "55100000", "55000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("B2B", "60000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("B2B ayarları", "60100000", "60000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Ayarlar", "65000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Genel parametreler", "65100000", "65000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Yazıcı ayarları", "65200000", "65000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Kullanıcı tanımlama", "65300000", "65000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Genel evrak form tasarımı", "65400000", "65000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Kurulum", "80000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
		list.Add(new ForaAppWinAnaMenu("Lisans yöneticisi", "80100000", "80000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Fora Mobil servis kurulumu", "80200000", "80000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Fora Mobil veritabanı kurulumu", "80300000", "80000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Performans testi kurulumu", "80400000", "80000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		list.Add(new ForaAppWinAnaMenu("Banka aktarım kurulumu", "80600000", "80000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Diger));
		if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuar")
		{
			list = new List<ForaAppWinAnaMenu>();
			list.Add(new ForaAppWinAnaMenu("ANA", "00000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
			list.Add(new ForaAppWinAnaMenu("Stok Satış Raporu Girişi", "45052000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
			list.Add(new ForaAppWinAnaMenu("Kupon kodu verme", "6006", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Evrak));
			return;
		}
		if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuarcekilis")
		{
			list = new List<ForaAppWinAnaMenu>();
			list.Add(new ForaAppWinAnaMenu("ANA", "00000000", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Menu));
			list.Add(new ForaAppWinAnaMenu("Çekiliş yapma", "6007", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Rapor));
			list.Add(new ForaAppWinAnaMenu("Çekiliş gösterim", "6008", "00000000", gorunur: true, enum_ForaAppWinAnaMenuTipi.Evrak));
			return;
		}
		_mikrouygulamabilgileri.anamenu = list;
		bool flag = false;
		foreach (ForaAppWinAnaMenu item in list)
		{
			if (item.Kod == text4)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			MessageBox.Show("Uygulama bulunamadı. Lütfen parametreleri kontrol ediniz.", "HATA");
		}
		else
		{
			Application.Run(new AnaMenu(_mikrouygulamabilgileri, text4));
		}
	}

	private static void SqlBaglantiAyarlariAc()
	{
		BaglantiAyarlari baglantiAyarlari = new BaglantiAyarlari("Sql Bağlantı Bilgileri", _mikrouygulamabilgileri.MikroAnaDBName);
		baglantiAyarlari._BaglantiBilgileri = _mikrouygulamabilgileri.baglantibilgileri;
		if (baglantiAyarlari.ShowDialog() == DialogResult.OK)
		{
			_mikrouygulamabilgileri.baglantibilgileri = baglantiAyarlari._BaglantiBilgileri;
			if (!_mikrouygulamabilgileri.baglantibilgileri.Save("data\\sqlbaglantibilgileri.xml"))
			{
				MessageBox.Show("Bağlantı Ayarları Kayıt Edilemedi!", "HATA");
			}
		}
	}
}
