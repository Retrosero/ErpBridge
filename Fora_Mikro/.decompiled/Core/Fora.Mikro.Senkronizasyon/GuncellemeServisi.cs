using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Fora.Mikro.Database.Sqlite;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Interface;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Tablolar;
using Fora.Mikro.TablolarV16;
using Fora.Mikro.Utility;
using Fora.Mikro.Vergiler;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Senkronizasyon;

public class GuncellemeServisi
{
	public delegate void GuncellemeBasladiHandler(object sender);

	public delegate void GuncellemeBittiHandler(object sender);

	public delegate void HataMesajiGosterHandler(object sender, string hata_mesaji);

	public delegate void Progress1BaslikChangedHandler(object sender, string yeni_baslik);

	public delegate void Progress1MaxChangedHandler(object sender, int maksimum);

	public delegate void Progress1ProgressChangedHandler(object sender, int progress);

	public delegate void Progress2BaslikChangedHandler(object sender, string yeni_baslik);

	public delegate void Progress2AltBaslikChangedHandler(object sender, string yeni_baslik);

	public delegate void Progress2MaxChangedHandler(object sender, int maksimum);

	public delegate void Progress2ProgressChangedHandler(object sender, int progress);

	private IForaPlatformTools _Platformtools;

	private SqliteCommand command;

	private SqliteDataReader reader;

	private string uri;

	private HttpWebResponse response;

	private byte[] buffer;

	private Encoding enc = Encoding.UTF8;

	private string Response;

	private StreamReader loResponseStream;

	private bool _EvrakGonder;

	private bool _SenkronizeEt;

	private string _Android_ID;

	private string _PicturePath;

	private ServisKontrolu servis_kontrolu;

	private SqliteHelper sqlite_helper;

	private List<Tablo> V15_guncellenecektablolar;

	private List<TabloV16> V16_guncellenecektablolar;

	private int denemesayisi;

	private int BirSeferdekiMaksimumKayitSayisi;

	private bool mye_TextDataGuncelle;

	public event GuncellemeBasladiHandler OnGuncellemeBasladi;

	public event GuncellemeBittiHandler OnGuncellemeBitti;

	public event HataMesajiGosterHandler OnHataMesajiGoster;

	public event Progress1BaslikChangedHandler OnProgress1BaslikChanged;

	public event Progress1MaxChangedHandler OnProgress1MaxChanged;

	public event Progress1ProgressChangedHandler OnProgress1ProgressChanged;

	public event Progress2BaslikChangedHandler OnProgress2BaslikChanged;

	public event Progress2AltBaslikChangedHandler OnProgress2AltBaslikChanged;

	public event Progress2MaxChangedHandler OnProgress2MaxChanged;

	public event Progress2ProgressChangedHandler OnProgress2ProgressChanged;

	public void Baslat(IForaPlatformTools Platformtools, bool EvrakGonder, bool SenkronizeEt, string DeviceID, string PicturePath)
	{
		_EvrakGonder = EvrakGonder;
		_SenkronizeEt = SenkronizeEt;
		_Android_ID = DeviceID;
		_PicturePath = PicturePath;
		_Platformtools = Platformtools;
		sqlite_helper = new SqliteHelper();
		Main();
	}

	public void Main()
	{
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Expected O, but got Unknown
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Invalid comparison between Unknown and I4
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Invalid comparison between Unknown and I4
		this.OnGuncellemeBasladi(this);
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_servis_bilgileri_cekiliyor);
		servis_kontrolu = new ServisKontrolu(1);
		if (servis_kontrolu.Sonuc == enum_servis_kontrol_sonuc.AgBaglantisiYok)
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_internete_baglanilamadi);
			IslemiBitir();
			return;
		}
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_veritabanina_baglaniyor);
		if (servis_kontrolu.Sonuc != enum_servis_kontrol_sonuc.ServisCalisiyor)
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_servise_erisilemedi);
			IslemiBitir();
			return;
		}
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_lisans_bilgisi_kontrol_ediliyor);
		bool flag = false;
		uri = "http://server.forayazilim.com/IsAlive";
		try
		{
			response = HttpWebUtility.GetWebResponseGet(uri, KeepAlive: true, "gzip, deflate", 5000, 5000);
			if (response != null && (int)response.StatusCode == 200)
			{
				loResponseStream = new StreamReader(((WebResponse)response).GetResponseStream(), enc);
				Response = loResponseStream.ReadToEnd();
				loResponseStream.Dispose();
				((WebResponse)response).Dispose();
				if (!Response.StartsWith("Hata") && Response == "true")
				{
					flag = true;
				}
			}
		}
		catch
		{
		}
		if (flag)
		{
			string text = "";
			uri = "http://server.forayazilim.com/GetLisansV3/" + AppBase._FirmaID + "/" + _Android_ID + "/" + AppBase._Versiyon.ToString("0.0000").Replace(".", "") + "/" + AppBase.MikroVersiyonu + "/" + AppBase._KullaniciAdi;
			try
			{
				response = HttpWebUtility.GetWebResponseGet(uri, KeepAlive: true, "gzip, deflate", 10000, 10000);
				if (response != null && (int)response.StatusCode == 200)
				{
					loResponseStream = new StreamReader(((WebResponse)response).GetResponseStream(), enc);
					Response = loResponseStream.ReadToEnd();
					loResponseStream.Dispose();
					((WebResponse)response).Dispose();
					if (!Response.StartsWith("Hata"))
					{
						text = Response;
					}
				}
			}
			catch
			{
			}
			if (text != "")
			{
				sqlite_helper.Open(AppBase.GetFirmalarDbPath(), ReadOnly: false);
				command = new SqliteCommand("UPDATE Firmalar SET Lisans=@lisans WHERE FirmaID=@firma_id");
				command.Connection = sqlite_helper.GetConnection();
				command.Parameters.AddWithValue("@firma_id", (object)AppBase._FirmaID);
				command.Parameters.AddWithValue("@lisans", (object)text);
				((DbCommand)command).ExecuteNonQuery();
				((Component)command).Dispose();
				command = null;
				sqlite_helper.Dispose();
			}
		}
		bool flag2 = false;
		try
		{
			string encryptedText = "";
			sqlite_helper.Open(AppBase.GetFirmalarDbPath(), ReadOnly: true);
			command = new SqliteCommand();
			((DbCommand)command).CommandText = "SELECT Lisans FROM Firmalar WHERE FirmaID='" + AppBase._FirmaID + "'";
			command.Connection = sqlite_helper.GetConnection();
			reader = command.ExecuteReader();
			if (((DbDataReader)reader).HasRows)
			{
				((DbDataReader)reader).Read();
				encryptedText = reader.GetSafeString(0);
			}
			((DbDataReader)reader).Close();
			((DbDataReader)reader).Dispose();
			reader = null;
			((Component)command).Dispose();
			command = null;
			sqlite_helper.Dispose();
			CultureInfo provider = new CultureInfo("tr-TR");
			DateTime dateTime = DateTime.Parse(_Platformtools.DecryptText(_Android_ID, encryptedText, useHashing: true), provider);
			flag2 = !(DateTime.Now > dateTime);
		}
		catch
		{
			flag2 = false;
		}
		if (!flag2)
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_lisans_hatasi);
			IslemiBitir();
		}
		else if (AppBase._MikroDBName.StartsWith("MikroDB_V16"))
		{
			V16_SenkronizasyonOncesi();
		}
		else
		{
			V15_SenkronizasyonOncesi();
		}
	}

	private void V15_SenkronizasyonOncesi()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		this.OnProgress1MaxChanged(this, 1);
		if (!_Platformtools.Exists(AppBase.GetMikroDBNameWithPath()))
		{
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_veritabani_olusturuluyor);
			_Platformtools.CreateDb(AppBase.GetMikroDBNameWithPath());
			command = new SqliteCommand("CREATE TABLE IF NOT EXISTS [_FORA_PARAMETRELER] ([ID] INTEGER NOT NULL PRIMARY KEY,[ParametreProgram] TEXT NULL,[ParametreUser] TEXT NULL,[ParametreAnaGrubu] TEXT NULL,[ParametreAltGrubu] TEXT NULL, [ParametreID] INTEGER NULL,[ParametreAdi] TEXT NULL,[ParametreDegeri] TEXT NULL )");
			command.Connection = AppBase.GetDbConnectionSync();
			((DbCommand)command).ExecuteNonQuery();
			((Component)command).Dispose();
			command = null;
		}
		command = new SqliteCommand();
		((DbCommand)command).CommandText = "CREATE TABLE IF NOT EXISTS [OfflineBilgi] ([TabloID] INTEGER NOT NULL PRIMARY KEY, [SonGuncellemeZamani] TEXT NULL,[UpdateLastTriggerRecNo] INTEGER NULL,[DeleteLastTriggerRecNo] INTEGER NULL)";
		command.Connection = AppBase.GetDbConnectionSync();
		((DbCommand)command).ExecuteNonQuery();
		((Component)command).Dispose();
		command = null;
		denemesayisi = 0;
		if (servis_kontrolu.ServisVersiyonu < 225)
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_servis_guncelleme_gerekmekte);
			IslemiBitir();
		}
		else
		{
			V15_Senkronizasyon_Giris();
		}
	}

	private void V15_Senkronizasyon_Giris()
	{
		BirSeferdekiMaksimumKayitSayisi = 5000;
		this.OnProgress1BaslikChanged(this, AppResource.guncelleme_parametreler_guncelleniyor);
		this.OnProgress1MaxChanged(this, 1);
		this.OnProgress1ProgressChanged(this, 0);
		this.OnProgress2BaslikChanged(this, "");
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		Tablo mikroV14_Tablo__FORA_PARAMETRELER = TabloHelper.Get_MikroV14_Tablo__FORA_PARAMETRELER();
		V15_guncellenecektablolar = new List<Tablo>();
		V15_guncellenecektablolar.Add(mikroV14_Tablo__FORA_PARAMETRELER);
		if (!V15_Senkronizasyon_Main())
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_hata_parametreler_cekilemedi);
			IslemiBitir();
			return;
		}
		bool flag = true;
		if (_EvrakGonder)
		{
			this.OnProgress1BaslikChanged(this, AppResource.guncelleme_kayitlar_gonderiliyor);
			this.OnProgress1MaxChanged(this, 1);
			this.OnProgress1ProgressChanged(this, 0);
			this.OnProgress2BaslikChanged(this, "");
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_kayitlar_gonderiliyor);
			flag = V15_OfflineKayitlariGonder_Yeni();
		}
		if (!flag)
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_kayitlar_gonderilemedi);
		}
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_tablo_yapisi_kontrol_ediliyor);
		List<Tablo> mikroV14DefaultTablolar = TabloHelper.GetMikroV14DefaultTablolar();
		if (servis_kontrolu.ServisVersiyonu >= 260 && AppBase.GetMikroDBNameWithPath().Contains("MikroDB_V15"))
		{
			mikroV14DefaultTablolar.Add(TabloHelper.Get_MikroV15_Tablo_KUR_ISIMLERI());
		}
		if (AppBase.KullaniciParametreleri._GetParametre("EFaturaAktif")._GetBoolean)
		{
			foreach (Tablo item7 in mikroV14DefaultTablolar)
			{
				if (item7.TabloAdi == "CARI_HESAPLAR")
				{
					Field item = new Field("cari_efatura_fl", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false);
					Field item2 = new Field("cari_def_efatura_cinsi", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false);
					item7.Fieldlar.Add(item);
					item7.Fieldlar.Add(item2);
					break;
				}
			}
		}
		if (AppBase.KullaniciParametreleri._GetParametre("EvrakKayitCariSifresiSor")._GetBoolean)
		{
			foreach (Tablo item8 in mikroV14DefaultTablolar)
			{
				if (item8.TabloAdi == "CARI_HESAPLAR")
				{
					Field item3 = new Field("cari_wwwadresi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false);
					item8.Fieldlar.Add(item3);
					break;
				}
			}
		}
		foreach (Tablo item9 in mikroV14DefaultTablolar)
		{
			this.OnProgress2AltBaslikChanged(this, item9.TabloAdi);
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			V15_TabloYapisiKontrolu(item9);
		}
		this.OnProgress1BaslikChanged(this, AppResource.guncelleme_tablolar_guncelleniyor);
		this.OnProgress1MaxChanged(this, 1);
		this.OnProgress1ProgressChanged(this, 0);
		this.OnProgress2BaslikChanged(this, "");
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		if (_SenkronizeEt)
		{
			mye_TextDataGuncelle = AppBase.KullaniciParametreleri._GetParametre("mye_TextDataGuncelle")._GetBoolean;
			V15_guncellenecektablolar = new List<Tablo>();
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOKLAR")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOKLAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAPLAR")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_HESAPLAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SORUMLULUK_MERKEZLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_SORUMLULUK_MERKEZLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_PROJELER")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_PROJELER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_ANA_GRUPLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_ANA_GRUPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_ALT_GRUPLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_ALT_GRUPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_URETICILERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_URETICILERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_REYONLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_REYONLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_MARKALARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_MARKALARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_CARI_ISKONTO_TANIMLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_CARI_ISKONTO_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_SATIS_FIYAT_LISTELERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_SATIS_FIYAT_LISTELERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_SATIS_FIYAT_LISTE_TANIMLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_SATIS_FIYAT_LISTE_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_BARKOD_TANIMLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_BARKOD_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_ADRESLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_HESAP_ADRESLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_YETKILILERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_HESAP_YETKILILERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DEPOLAR")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_DEPOLAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_ODEME_PLANLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_ODEME_PLANLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_KASALAR")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_KASALAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_HAREKETLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_HESAP_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SIPARISLER")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_SIPARISLER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_PROFORMA_SIPARISLER")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_PROFORMA_SIPARISLER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DOVIZ_KURLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_DOVIZ_KURLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SATIS_SARTLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_SATIS_SARTLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SATINALMA_SARTLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_SATINALMA_SARTLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_HAREKETLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_ODEME_EMIRLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_ODEME_EMIRLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_TEMINATLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_HESAP_TEMINATLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_FIRMALAR")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_FIRMALAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SUBELER")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_SUBELER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_BANKALAR")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_BANKALAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_MASRAF_HESAPLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_MASRAF_HESAPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_YEREL_BANKA_KODLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_YEREL_BANKA_KODLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt__ZIYARET_HAREKETLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo__ZIYARET_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_PERSONEL_TANIMLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_PERSONEL_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_BOLGELERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_HESAP_BOLGELERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_GRUPLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CARI_HESAP_GRUPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_TESLIM_TURLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_TESLIM_TURLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DEPOLAR_ARASI_SIPARISLER")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_DEPOLAR_ARASI_SIPARISLER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_EVRAK_ACIKLAMALARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_EVRAK_ACIKLAMALARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_KATEGORILERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_KATEGORILERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_SEKTORLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_SEKTORLERI());
			}
			V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_IHRACAT_DOSYALARI());
			V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_PAKET_TANIMLARI());
			if (servis_kontrolu.ServisVersiyonu >= 260 && AppBase.GetMikroDBNameWithPath().Contains("MikroDB_V15"))
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV15_Tablo_KUR_ISIMLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_BEDEN_HAREKETLERI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_BEDEN_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_BEDEN_TANIMLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_BEDEN_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_RENK_TANIMLARI")._GetBoolean)
			{
				V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_RENK_TANIMLARI());
			}
			V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_STOK_SERINO_TANIMLARI());
			V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_KAPAMA_NEDENLERI_TANIMLARI());
			V15_guncellenecektablolar.Add(TabloHelper.Get_MikroV14_Tablo_CIHAZ_HAREKETLERI());
			if (AppBase.KullaniciParametreleri._GetParametre("EFaturaAktif")._GetBoolean)
			{
				foreach (Tablo item10 in V15_guncellenecektablolar)
				{
					if (item10.TabloAdi == "CARI_HESAPLAR")
					{
						Field item4 = new Field("cari_efatura_fl", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false);
						Field item5 = new Field("cari_def_efatura_cinsi", enum_sqlite_data_tip.INTEGER, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false);
						item10.Fieldlar.Add(item4);
						item10.Fieldlar.Add(item5);
						break;
					}
				}
			}
			if (AppBase.KullaniciParametreleri._GetParametre("EvrakKayitCariSifresiSor")._GetBoolean)
			{
				foreach (Tablo item11 in V15_guncellenecektablolar)
				{
					if (item11.TabloAdi == "CARI_HESAPLAR")
					{
						Field item6 = new Field("cari_wwwadresi", enum_sqlite_data_tip.TEXT, datetimemi: false, Cekilecek: true, Null: true, Primary: false, RecNoMu: false);
						item11.Fieldlar.Add(item6);
						break;
					}
				}
			}
			if (!V15_Senkronizasyon_Main())
			{
				IslemiBitir();
				return;
			}
		}
		StokFotograflariniGuncelle_Yeni();
		if (mye_TextDataGuncelle)
		{
			if (servis_kontrolu.ServisVersiyonu > 247)
			{
				V15_TextDataGuncelle();
			}
			else
			{
				this.OnHataMesajiGoster(this, AppResource.guncelleme_yazboz_tahtasi_sunucu_versiyonu_hatasi);
			}
		}
		IslemiBitir();
	}

	private bool V15_Senkronizasyon_Main()
	{
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Invalid comparison between Unknown and I4
		foreach (Tablo item in V15_guncellenecektablolar)
		{
			item.SonRECno = 0;
			item.YeniKayitSayisi = 0;
			item.DegisenKayitSayisi = 0;
			item.OfflineKayitSayisi = 0;
			item.UpdateEdildi = false;
			item.OfflineLastUpdateTriggerRecNo = 0;
			item.OfflineLastDeleteTriggerRecNo = 0;
		}
		if (servis_kontrolu.ServisVersiyonu >= 238)
		{
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_online_tablo_yapisi_kontrol_ediliyor);
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			if (servis_kontrolu.ServisVersiyonu > 30005)
			{
				uri = "http://" + servis_kontrolu.ServisAdresi + "/GetTabloYapisiTopluV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
			}
			else
			{
				uri = "http://" + servis_kontrolu.ServisAdresi + "/GetTabloYapisiToplu/" + _Platformtools.UrlEncode(AppBase._FirmaID);
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(AppBase._KullaniciAdi);
			binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
			binaryWriter.Write(V15_guncellenecektablolar.Count);
			foreach (Tablo item2 in V15_guncellenecektablolar)
			{
				binaryWriter.Write(item2.TabloAdi);
			}
			buffer = memoryStream.ToArray();
			try
			{
				response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 40000, 40000, "application");
			}
			catch
			{
				return false;
			}
			if (response == null)
			{
				return false;
			}
			if ((int)response.StatusCode != 200)
			{
				return false;
			}
			BinaryReader binaryReader;
			try
			{
				this.OnProgress2BaslikChanged(this, AppResource.guncelleme_online_tablo_yapisi_kontrol_ediliyor);
				this.OnProgress2AltBaslikChanged(this, "");
				this.OnProgress2MaxChanged(this, 1);
				this.OnProgress2ProgressChanged(this, 0);
				MemoryStream memoryStream2 = new MemoryStream();
				((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
				memoryStream2.Position = 0L;
				binaryReader = new BinaryReader(memoryStream2);
				MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
				BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
				if (!binaryReader2.ReadBoolean())
				{
					return false;
				}
				foreach (Tablo item3 in V15_guncellenecektablolar)
				{
					string text = binaryReader2.ReadString();
					List<Field> list = new List<Field>();
					foreach (Field item4 in item3.Fieldlar)
					{
						if (text.Contains(item4.Adi))
						{
							list.Add(item4);
						}
					}
					item3.Fieldlar = list;
				}
				binaryReader2.Dispose();
				binaryReader2 = null;
				memoryStream3.Dispose();
				memoryStream3 = null;
			}
			catch
			{
				return false;
			}
			binaryReader.Dispose();
			((WebResponse)response).Dispose();
		}
		if (!V15_Yeni_Kayitlari_Senkronize_Et())
		{
			this.OnHataMesajiGoster(this, AppResource.genel_hata + " : " + AppResource.guncelleme_yeni_kayitlar_cekilemedi);
			return false;
		}
		if (!V15_Degisen_Kayitlari_Senkronize_Et())
		{
			this.OnHataMesajiGoster(this, AppResource.genel_hata + " : " + AppResource.guncelleme_degisen_kayitlar_cekilemedi);
			return false;
		}
		if (!V15_Silinen_Kayitlari_Senkronize_Et())
		{
			this.OnHataMesajiGoster(this, AppResource.genel_hata + " : " + AppResource.guncelleme_silinen_kayitlar_cekilemedi);
			return false;
		}
		if (!V15_Senkronizasyon_Kontrolu())
		{
			this.OnHataMesajiGoster(this, AppResource.genel_hata + " : " + AppResource.guncelleme_kontrol_islemi_yapilamadi);
			return false;
		}
		return true;
	}

	private bool V15_Yeni_Kayitlari_Senkronize_Et()
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Invalid comparison between Unknown and I4
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Expected O, but got Unknown
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Invalid comparison between Unknown and I4
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_0723: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0966: Unknown result type (might be due to invalid IL or missing references)
		//IL_0970: Invalid comparison between Unknown and I4
		//IL_0c1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c21: Expected O, but got Unknown
		//IL_0ca2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d73: Expected O, but got Unknown
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_yeni_kayitlar_kontrol_ediliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		int num = 0;
		if (servis_kontrolu.ServisVersiyonu > 30005)
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetLastTriggerRecNoV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		}
		else
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetLastTriggerRecNo/" + _Platformtools.UrlEncode(AppBase._FirmaID);
		}
		try
		{
			response = HttpWebUtility.GetWebResponseGet(uri, KeepAlive: true, "gzip, deflate", 20000, 20000);
		}
		catch
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if ((int)response.StatusCode != 200)
		{
			return false;
		}
		loResponseStream = new StreamReader(((WebResponse)response).GetResponseStream(), enc);
		Response = loResponseStream.ReadToEnd();
		loResponseStream.Dispose();
		((WebResponse)response).Dispose();
		if (Response.StartsWith("Hata"))
		{
			return false;
		}
		try
		{
			num = int.Parse(Response);
		}
		catch
		{
			return false;
		}
		if (num == -1)
		{
			return false;
		}
		foreach (Tablo item in V15_guncellenecektablolar)
		{
			try
			{
				string commandText = "SELECT COUNT(*) FROM " + item.TabloAdi;
				SqliteCommand val = new SqliteCommand
				{
					Connection = AppBase.GetDbConnectionSync(),
					CommandText = commandText
				};
				object obj3 = ((DbCommand)val).ExecuteScalar();
				((Component)val).Dispose();
				if (obj3 != null)
				{
					item.OfflineKayitSayisi = int.Parse(obj3.ToString());
				}
				else
				{
					item.OfflineKayitSayisi = 0;
				}
			}
			catch
			{
				return false;
			}
			if (item.OfflineKayitSayisi == 0)
			{
				try
				{
					command = new SqliteCommand();
					command.Connection = AppBase.GetDbConnectionSync();
					((DbCommand)command).CommandText = "INSERT OR REPLACE INTO OfflineBilgi (TabloID,SonGuncellemeZamani,UpdateLastTriggerRecNo,DeleteLastTriggerRecNo) VALUES (@tablo_id,@son_guncelleme_zamani,@update_last_trigger_rec_no,@delete_last_trigger_rec_no)";
					command.Parameters.AddWithValue("@tablo_id", (object)item.TabloID);
					command.Parameters.AddWithValue("@son_guncelleme_zamani", (object)DateTime.Now.AddYears(-10));
					command.Parameters.AddWithValue("@update_last_trigger_rec_no", (object)num);
					command.Parameters.AddWithValue("@delete_last_trigger_rec_no", (object)num);
					((DbCommand)command).ExecuteNonQuery();
					((Component)command).Dispose();
					command = null;
				}
				catch
				{
					return false;
				}
			}
		}
		foreach (Tablo item2 in V15_guncellenecektablolar)
		{
			try
			{
				string commandText2 = "SELECT " + item2.GetRECnoFieldName() + " FROM " + item2.TabloAdi + " ORDER BY " + item2.GetRECnoFieldName() + " DESC LIMIT 1";
				SqliteCommand val2 = new SqliteCommand
				{
					Connection = AppBase.GetDbConnectionSync(),
					CommandText = commandText2
				};
				object obj6 = ((DbCommand)val2).ExecuteScalar();
				((Component)val2).Dispose();
				if (obj6 != null)
				{
					item2.SonRECno = int.Parse(obj6.ToString());
				}
				else
				{
					item2.SonRECno = 0;
				}
			}
			catch
			{
				return false;
			}
		}
		if (servis_kontrolu.ServisVersiyonu > 30005)
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetTabloYeniKayitSayilariV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		}
		else
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetTabloYeniKayitSayilari/" + _Platformtools.UrlEncode(AppBase._FirmaID);
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		binaryWriter.Write(V15_guncellenecektablolar.Count);
		foreach (Tablo item3 in V15_guncellenecektablolar)
		{
			binaryWriter.Write(item3.TabloAdi);
			binaryWriter.Write(item3.GetRECnoFieldName());
			binaryWriter.Write(item3.SonRECno);
		}
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 20000, 20000, "application");
		}
		catch
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if ((int)response.StatusCode != 200)
		{
			return false;
		}
		MemoryStream memoryStream2 = new MemoryStream();
		((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
		memoryStream2.Position = 0L;
		BinaryReader binaryReader = new BinaryReader(memoryStream2);
		try
		{
			if (!binaryReader.ReadBoolean())
			{
				return false;
			}
			foreach (Tablo item4 in V15_guncellenecektablolar)
			{
				item4.YeniKayitSayisi = binaryReader.ReadInt32();
			}
		}
		catch
		{
			return false;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		int num2 = 0;
		List<Tablo> list = new List<Tablo>();
		foreach (Tablo item5 in V15_guncellenecektablolar)
		{
			if (item5.YeniKayitSayisi > 0)
			{
				list.Add(item5);
			}
			if (item5.YeniKayitSayisi > num2)
			{
				num2 = item5.YeniKayitSayisi;
			}
		}
		if (list.Count > 0)
		{
			int maksimum = num2 / BirSeferdekiMaksimumKayitSayisi + 1;
			this.OnProgress1MaxChanged(this, maksimum);
			int num3 = 1;
			while (true)
			{
				this.OnProgress1ProgressChanged(this, num3 - 1);
				if (num3 != 1)
				{
					foreach (Tablo item6 in list)
					{
						try
						{
							string commandText3 = "SELECT " + item6.GetRECnoFieldName() + " FROM " + item6.TabloAdi + " ORDER BY " + item6.GetRECnoFieldName() + " DESC LIMIT 1";
							SqliteCommand val3 = new SqliteCommand
							{
								Connection = AppBase.GetDbConnectionSync(),
								CommandText = commandText3
							};
							object obj10 = ((DbCommand)val3).ExecuteScalar();
							((Component)val3).Dispose();
							if (obj10 != null)
							{
								item6.SonRECno = int.Parse(obj10.ToString());
							}
							else
							{
								item6.SonRECno = 0;
							}
						}
						catch
						{
							return false;
						}
					}
				}
				List<DataTable> list2 = new List<DataTable>();
				List<int> list3 = new List<int>();
				this.OnProgress2BaslikChanged(this, AppResource.guncelleme_yeni_kayitlar_indiriliyor);
				this.OnProgress2AltBaslikChanged(this, "");
				this.OnProgress2MaxChanged(this, 1);
				this.OnProgress2ProgressChanged(this, 0);
				if (servis_kontrolu.ServisVersiyonu > 30005)
				{
					uri = "http://" + servis_kontrolu.ServisAdresi + "/GetYeniKayitlarTopluV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
				}
				else
				{
					uri = "http://" + servis_kontrolu.ServisAdresi + "/GetYeniKayitlarToplu/" + _Platformtools.UrlEncode(AppBase._FirmaID);
				}
				memoryStream = new MemoryStream();
				binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(AppBase._KullaniciAdi);
				binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
				binaryWriter.Write(BirSeferdekiMaksimumKayitSayisi);
				binaryWriter.Write(list.Count);
				foreach (Tablo item7 in list)
				{
					binaryWriter.Write(item7.TabloAdi);
					binaryWriter.Write(item7.GetRECnoFieldName());
					binaryWriter.Write(item7.SonRECno);
					binaryWriter.Write(item7.GetCekilecekFieldlar(""));
				}
				buffer = memoryStream.ToArray();
				try
				{
					response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
				}
				catch
				{
					return false;
				}
				if (response == null)
				{
					return false;
				}
				if ((int)response.StatusCode != 200)
				{
					return false;
				}
				try
				{
					this.OnProgress2BaslikChanged(this, AppResource.guncelleme_yeni_kayitlar_hazirlaniyor);
					this.OnProgress2AltBaslikChanged(this, "");
					this.OnProgress2MaxChanged(this, 1);
					this.OnProgress2ProgressChanged(this, 0);
					memoryStream2 = new MemoryStream();
					((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
					memoryStream2.Position = 0L;
					binaryReader = new BinaryReader(memoryStream2);
					MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
					BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
					if (!binaryReader2.ReadBoolean())
					{
						return false;
					}
					foreach (Tablo item8 in list)
					{
						_ = item8;
						list2.Add(DataTableManualReader(binaryReader2));
					}
					binaryReader2.Dispose();
					binaryReader2 = null;
					memoryStream3.Dispose();
					memoryStream3 = null;
				}
				catch
				{
					return false;
				}
				binaryReader.Dispose();
				((WebResponse)response).Dispose();
				this.OnProgress2BaslikChanged(this, AppResource.guncelleme_yeni_kayitlar_yaziliyor);
				this.OnProgress2AltBaslikChanged(this, "");
				this.OnProgress2MaxChanged(this, 1);
				this.OnProgress2ProgressChanged(this, 0);
				int num4 = 0;
				foreach (Tablo item9 in list)
				{
					this.OnProgress2AltBaslikChanged(this, item9.TabloAdi);
					this.OnProgress2MaxChanged(this, ((InternalDataCollectionBase)list2[num4].Rows).Count);
					this.OnProgress2ProgressChanged(this, 0);
					int num5 = 0;
					int num6 = 0;
					SqliteTransaction val4 = AppBase.GetDbConnectionSync().BeginTransaction();
					string text = item9.GetCekilecekFieldlar("");
					string cekilecekFieldlar = item9.GetCekilecekFieldlar("@");
					if (item9.TabloAdi == "STOKLAR")
					{
						text += ",sto_kod_upper,sto_isim_upper";
					}
					if (item9.TabloAdi == "CARI_HESAPLAR")
					{
						text += ",cari_kod_upper,cari_unvan1_upper";
					}
					string text2 = "INSERT OR REPLACE INTO " + item9.TabloAdi + " (" + text + ") VALUES";
					text2 = text2 + "(" + cekilecekFieldlar;
					if (item9.TabloAdi == "STOKLAR")
					{
						text2 += ",@sto_kod_upper,@sto_isim_upper";
					}
					if (item9.TabloAdi == "CARI_HESAPLAR")
					{
						text2 += ",@cari_kod_upper,@cari_unvan1_upper";
					}
					text2 += ")";
					int num7 = 0;
					int num8 = 0;
					SqliteCommand val5 = new SqliteCommand(text2);
					val5.Connection = AppBase.GetDbConnectionSync();
					val5.Transaction = val4;
					int num9 = 0;
					foreach (Field item10 in item9.Fieldlar)
					{
						string fullName = list2[num4].Columns[num9].DataType.FullName;
						val5.Parameters.Add("@" + item10.Adi.ToLower().Replace("ı", "i"), GetDbType(fullName));
						num9++;
					}
					int num10 = 0;
					if (item9.TabloAdi == "STOKLAR")
					{
						val5.Parameters.Add("@sto_kod_upper", (DbType)16);
						val5.Parameters.Add("@sto_isim_upper", (DbType)16);
						num10 = 1;
					}
					if (item9.TabloAdi == "CARI_HESAPLAR")
					{
						val5.Parameters.Add("@cari_kod_upper", (DbType)16);
						val5.Parameters.Add("@cari_unvan1_upper", (DbType)16);
						num10 = 2;
					}
					foreach (DataRow item11 in (InternalDataCollectionBase)list2[num4].Rows)
					{
						DataRow val6 = item11;
						int num11 = 0;
						foreach (Field item12 in item9.Fieldlar)
						{
							_ = item12;
							((DbParameter)val5.Parameters[num11]).Value = val6[num11];
							num11++;
						}
						if (num10 == 1)
						{
							((DbParameter)val5.Parameters[num11]).Value = val6["sto_kod"].ToString().ToUpper();
							((DbParameter)val5.Parameters[num11 + 1]).Value = val6["sto_isim"].ToString().ToUpper();
						}
						if (num10 == 2)
						{
							((DbParameter)val5.Parameters[num11]).Value = val6["cari_kod"].ToString().ToUpper();
							((DbParameter)val5.Parameters[num11 + 1]).Value = val6["cari_unvan1"].ToString().ToUpper();
						}
						try
						{
							((DbCommand)val5).ExecuteNonQuery();
						}
						catch
						{
							((DbTransaction)val4).Rollback();
							((Component)val5).Dispose();
							return false;
						}
						if (num7 == 10)
						{
							this.OnProgress2ProgressChanged(this, num5);
							num7 = 0;
						}
						if (num8 == 5000)
						{
							((DbTransaction)val4).Commit();
							foreach (Tablo item13 in V15_guncellenecektablolar)
							{
								if (item13.TabloID == item9.TabloID)
								{
									item13.UpdateEdildi = true;
									break;
								}
							}
							val4 = AppBase.GetDbConnectionSync().BeginTransaction();
							num8 = 0;
						}
						num6++;
						num5++;
						num7++;
						num8++;
					}
					((DbTransaction)val4).Commit();
					foreach (Tablo item14 in V15_guncellenecektablolar)
					{
						if (item14.TabloID == item9.TabloID)
						{
							item14.UpdateEdildi = true;
							break;
						}
					}
					((Component)val5).Dispose();
					if (((InternalDataCollectionBase)list2[num4].Rows).Count != BirSeferdekiMaksimumKayitSayisi)
					{
						list3.Add(item9.TabloID);
					}
					num4++;
				}
				foreach (int item15 in list3)
				{
					int num12 = 0;
					using (List<Tablo>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext() && enumerator.Current.TabloID != item15)
						{
							num12++;
						}
					}
					list.RemoveAt(num12);
				}
				if (list.Count == 0)
				{
					break;
				}
				num3++;
			}
		}
		return true;
	}

	private bool V15_Degisen_Kayitlari_Senkronize_Et()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0a2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a35: Expected O, but got Unknown
		//IL_0ab6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Expected O, but got Unknown
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Invalid comparison between Unknown and I4
		//IL_0b80: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b87: Expected O, but got Unknown
		//IL_077a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0784: Invalid comparison between Unknown and I4
		//IL_0e1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e28: Expected O, but got Unknown
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_kontrol_ediliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		foreach (Tablo item in V15_guncellenecektablolar)
		{
			try
			{
				string commandText = "SELECT " + item.GetRECnoFieldName() + " FROM " + item.TabloAdi + " ORDER BY " + item.GetRECnoFieldName() + " DESC LIMIT 1";
				SqliteCommand val = new SqliteCommand
				{
					Connection = AppBase.GetDbConnectionSync(),
					CommandText = commandText
				};
				object obj = ((DbCommand)val).ExecuteScalar();
				((Component)val).Dispose();
				if (obj != null)
				{
					item.SonRECno = int.Parse(obj.ToString());
				}
				else
				{
					item.SonRECno = 0;
				}
			}
			catch
			{
				return false;
			}
		}
		foreach (Tablo item2 in V15_guncellenecektablolar)
		{
			try
			{
				string commandText2 = "SELECT UpdateLastTriggerRecNo FROM OfflineBilgi WHERE TabloID=@tablo_id LIMIT 1";
				command = new SqliteCommand();
				command.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)command).CommandText = commandText2;
				command.Parameters.AddWithValue("@tablo_id", (object)item2.TabloID);
				object obj3 = ((DbCommand)command).ExecuteScalar();
				((Component)command).Dispose();
				command = null;
				if (obj3 != null)
				{
					item2.OfflineLastUpdateTriggerRecNo = int.Parse(obj3.ToString());
				}
				else
				{
					item2.OfflineLastUpdateTriggerRecNo = 0;
				}
			}
			catch
			{
				return false;
			}
		}
		if (servis_kontrolu.ServisVersiyonu > 30005)
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetTabloDegisenKayitSayilariV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		}
		else
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetTabloDegisenKayitSayilari/" + _Platformtools.UrlEncode(AppBase._FirmaID);
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		binaryWriter.Write(V15_guncellenecektablolar.Count);
		foreach (Tablo item3 in V15_guncellenecektablolar)
		{
			binaryWriter.Write(item3.TabloID);
			binaryWriter.Write(item3.SonRECno);
			binaryWriter.Write(item3.OfflineLastUpdateTriggerRecNo);
		}
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 40000, 40000, "application");
		}
		catch
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if ((int)response.StatusCode != 200)
		{
			return false;
		}
		MemoryStream memoryStream2 = new MemoryStream();
		((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
		memoryStream2.Position = 0L;
		BinaryReader binaryReader = new BinaryReader(memoryStream2);
		try
		{
			if (!binaryReader.ReadBoolean())
			{
				return false;
			}
			foreach (Tablo item4 in V15_guncellenecektablolar)
			{
				item4.DegisenKayitSayisi = binaryReader.ReadInt32();
			}
		}
		catch
		{
			return false;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		int num = 0;
		List<Tablo> list = new List<Tablo>();
		foreach (Tablo item5 in V15_guncellenecektablolar)
		{
			if (item5.DegisenKayitSayisi > 0)
			{
				list.Add(item5);
			}
			if (item5.DegisenKayitSayisi > num)
			{
				num = item5.DegisenKayitSayisi;
			}
		}
		if (list.Count > 0)
		{
			int maksimum = num / BirSeferdekiMaksimumKayitSayisi + 1;
			this.OnProgress1MaxChanged(this, maksimum);
			int num2 = 1;
			while (true)
			{
				this.OnProgress1ProgressChanged(this, num2 - 1);
				if (num2 != 1)
				{
					foreach (Tablo item6 in V15_guncellenecektablolar)
					{
						try
						{
							string commandText3 = "SELECT UpdateLastTriggerRecNo FROM OfflineBilgi WHERE TabloID=@tablo_id LIMIT 1";
							command = new SqliteCommand();
							command.Connection = AppBase.GetDbConnectionSync();
							((DbCommand)command).CommandText = commandText3;
							command.Parameters.AddWithValue("@tablo_id", (object)item6.TabloID);
							object obj7 = ((DbCommand)command).ExecuteScalar();
							((Component)command).Dispose();
							command = null;
							if (obj7 != null)
							{
								item6.OfflineLastUpdateTriggerRecNo = int.Parse(obj7.ToString());
							}
							else
							{
								item6.OfflineLastUpdateTriggerRecNo = 0;
							}
						}
						catch
						{
							return false;
						}
					}
				}
				List<DataTable> list2 = new List<DataTable>();
				List<int> list3 = new List<int>();
				this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_indiriliyor);
				this.OnProgress2AltBaslikChanged(this, "");
				this.OnProgress2MaxChanged(this, 1);
				this.OnProgress2ProgressChanged(this, 0);
				if (servis_kontrolu.ServisVersiyonu > 30005)
				{
					uri = "http://" + servis_kontrolu.ServisAdresi + "/GetDegisenKayitlarTopluV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
				}
				else
				{
					uri = "http://" + servis_kontrolu.ServisAdresi + "/GetDegisenKayitlarToplu/" + _Platformtools.UrlEncode(AppBase._FirmaID);
				}
				memoryStream = new MemoryStream();
				binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(AppBase._KullaniciAdi);
				binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
				binaryWriter.Write(BirSeferdekiMaksimumKayitSayisi);
				binaryWriter.Write(list.Count);
				foreach (Tablo item7 in list)
				{
					binaryWriter.Write(item7.TabloAdi);
					binaryWriter.Write(item7.TabloID);
					binaryWriter.Write(item7.OfflineLastUpdateTriggerRecNo);
					binaryWriter.Write(item7.SonRECno);
					binaryWriter.Write(item7.GetRECnoFieldName());
					binaryWriter.Write(item7.GetCekilecekFieldlar(""));
				}
				buffer = memoryStream.ToArray();
				try
				{
					response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
				}
				catch
				{
					return false;
				}
				if (response == null)
				{
					return false;
				}
				if ((int)response.StatusCode != 200)
				{
					return false;
				}
				try
				{
					this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_hazirlaniyor);
					this.OnProgress2AltBaslikChanged(this, "");
					this.OnProgress2MaxChanged(this, 1);
					this.OnProgress2ProgressChanged(this, 0);
					memoryStream2 = new MemoryStream();
					((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
					memoryStream2.Position = 0L;
					binaryReader = new BinaryReader(memoryStream2);
					MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
					BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
					if (!binaryReader2.ReadBoolean())
					{
						return false;
					}
					foreach (Tablo item8 in list)
					{
						_ = item8;
						list2.Add(DataTableManualReader(binaryReader2));
					}
					binaryReader2.Dispose();
					binaryReader2 = null;
					memoryStream3.Dispose();
					memoryStream3 = null;
				}
				catch
				{
					return false;
				}
				binaryReader.Dispose();
				((WebResponse)response).Dispose();
				this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_yaziliyor);
				this.OnProgress2AltBaslikChanged(this, "");
				this.OnProgress2MaxChanged(this, 1);
				this.OnProgress2ProgressChanged(this, 0);
				int num3 = 0;
				foreach (Tablo item9 in list)
				{
					this.OnProgress2AltBaslikChanged(this, item9.TabloAdi);
					this.OnProgress2MaxChanged(this, ((InternalDataCollectionBase)list2[num3].Rows).Count);
					this.OnProgress2ProgressChanged(this, 0);
					int num4 = 0;
					int num5 = 0;
					SqliteTransaction val2 = AppBase.GetDbConnectionSync().BeginTransaction();
					string text = item9.GetCekilecekFieldlar("");
					string cekilecekFieldlar = item9.GetCekilecekFieldlar("@");
					if (item9.TabloAdi == "STOKLAR")
					{
						text += ",sto_kod_upper,sto_isim_upper";
					}
					if (item9.TabloAdi == "CARI_HESAPLAR")
					{
						text += ",cari_kod_upper,cari_unvan1_upper";
					}
					string text2 = "INSERT OR REPLACE INTO " + item9.TabloAdi + " (" + text + ") VALUES";
					text2 = text2 + "(" + cekilecekFieldlar;
					if (item9.TabloAdi == "STOKLAR")
					{
						text2 += ",@sto_kod_upper,@sto_isim_upper";
					}
					if (item9.TabloAdi == "CARI_HESAPLAR")
					{
						text2 += ",@cari_kod_upper,@cari_unvan1_upper";
					}
					text2 += ")";
					int num6 = 0;
					int num7 = 0;
					SqliteCommand val3 = new SqliteCommand(text2);
					val3.Connection = AppBase.GetDbConnectionSync();
					val3.Transaction = val2;
					int num8 = 0;
					foreach (Field item10 in item9.Fieldlar)
					{
						string fullName = list2[num3].Columns[num8].DataType.FullName;
						val3.Parameters.Add("@" + item10.Adi.ToLower().Replace("ı", "i"), GetDbType(fullName));
						num8++;
					}
					int num9 = 0;
					if (item9.TabloAdi == "STOKLAR")
					{
						val3.Parameters.Add("@sto_kod_upper", (DbType)16);
						val3.Parameters.Add("@sto_isim_upper", (DbType)16);
						num9 = 1;
					}
					if (item9.TabloAdi == "CARI_HESAPLAR")
					{
						val3.Parameters.Add("@cari_kod_upper", (DbType)16);
						val3.Parameters.Add("@cari_unvan1_upper", (DbType)16);
						num9 = 2;
					}
					foreach (DataRow item11 in (InternalDataCollectionBase)list2[num3].Rows)
					{
						DataRow val4 = item11;
						int num10 = 0;
						foreach (Field item12 in item9.Fieldlar)
						{
							_ = item12;
							((DbParameter)val3.Parameters[num10]).Value = val4[num10];
							num10++;
						}
						if (num9 == 1)
						{
							((DbParameter)val3.Parameters[num10]).Value = val4["sto_kod"].ToString().ToUpper();
							((DbParameter)val3.Parameters[num10 + 1]).Value = val4["sto_isim"].ToString().ToUpper();
						}
						if (num9 == 2)
						{
							((DbParameter)val3.Parameters[num10]).Value = val4["cari_kod"].ToString().ToUpper();
							((DbParameter)val3.Parameters[num10 + 1]).Value = val4["cari_unvan1"].ToString().ToUpper();
						}
						try
						{
							((DbCommand)val3).ExecuteNonQuery();
						}
						catch
						{
							((DbTransaction)val2).Rollback();
							((Component)val3).Dispose();
							return false;
						}
						if (num6 == 10)
						{
							this.OnProgress2ProgressChanged(this, num4);
							num6 = 0;
						}
						if (num7 == 5000)
						{
							((DbTransaction)val2).Commit();
							foreach (Tablo item13 in V15_guncellenecektablolar)
							{
								if (item13.TabloID == item9.TabloID)
								{
									item13.UpdateEdildi = true;
									break;
								}
							}
							val2 = AppBase.GetDbConnectionSync().BeginTransaction();
							num7 = 0;
						}
						num5++;
						num4++;
						num6++;
						num7++;
					}
					((DbTransaction)val2).Commit();
					foreach (Tablo item14 in V15_guncellenecektablolar)
					{
						if (item14.TabloID == item9.TabloID)
						{
							item14.UpdateEdildi = true;
							break;
						}
					}
					((Component)val3).Dispose();
					if (((InternalDataCollectionBase)list2[num3].Rows).Count > 0)
					{
						try
						{
							int num11 = int.Parse(list2[num3].Rows[((InternalDataCollectionBase)list2[num3].Rows).Count - 1]["TriggerRECno"].ToString());
							command = new SqliteCommand();
							command.Connection = AppBase.GetDbConnectionSync();
							((DbCommand)command).CommandText = "UPDATE OfflineBilgi SET UpdateLastTriggerRecNo=@update_son_trigger_rec_no WHERE TabloID=@tablo_id";
							command.Parameters.AddWithValue("@tablo_id", (object)item9.TabloID);
							command.Parameters.AddWithValue("@update_son_trigger_rec_no", (object)num11);
							((DbCommand)command).ExecuteNonQuery();
							((Component)command).Dispose();
							command = null;
						}
						catch
						{
						}
					}
					if (((InternalDataCollectionBase)list2[num3].Rows).Count != BirSeferdekiMaksimumKayitSayisi)
					{
						list3.Add(item9.TabloID);
					}
					num3++;
				}
				foreach (int item15 in list3)
				{
					int num12 = 0;
					using (List<Tablo>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext() && enumerator.Current.TabloID != item15)
						{
							num12++;
						}
					}
					list.RemoveAt(num12);
				}
				if (list.Count == 0)
				{
					break;
				}
				num2++;
			}
		}
		return true;
	}

	private bool V15_Silinen_Kayitlari_Senkronize_Et()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Invalid comparison between Unknown and I4
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Expected O, but got Unknown
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Expected O, but got Unknown
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Expected O, but got Unknown
		List<Tablo> list = new List<Tablo>();
		foreach (Tablo item in V15_guncellenecektablolar)
		{
			list.Add(item);
		}
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_silinen_kayitlar_kontrol_ediliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		foreach (Tablo item2 in list)
		{
			try
			{
				string commandText = "SELECT DeleteLastTriggerRecNo FROM OfflineBilgi WHERE TabloID=@tablo_id LIMIT 1";
				command = new SqliteCommand();
				command.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)command).CommandText = commandText;
				command.Parameters.AddWithValue("@tablo_id", (object)item2.TabloID);
				object obj = ((DbCommand)command).ExecuteScalar();
				((Component)command).Dispose();
				command = null;
				if (obj != null)
				{
					item2.OfflineLastDeleteTriggerRecNo = int.Parse(obj.ToString());
				}
				else
				{
					item2.OfflineLastDeleteTriggerRecNo = 0;
				}
			}
			catch
			{
				return false;
			}
		}
		List<DataTable> list2 = new List<DataTable>();
		if (servis_kontrolu.ServisVersiyonu > 30005)
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetSilinenKayitlarTopluV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		}
		else
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/GetSilinenKayitlarToplu/" + _Platformtools.UrlEncode(AppBase._FirmaID);
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		binaryWriter.Write(BirSeferdekiMaksimumKayitSayisi);
		binaryWriter.Write(list.Count);
		foreach (Tablo item3 in list)
		{
			binaryWriter.Write(item3.TabloID);
			binaryWriter.Write(item3.OfflineLastDeleteTriggerRecNo);
		}
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
		}
		catch
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if ((int)response.StatusCode != 200)
		{
			return false;
		}
		BinaryReader binaryReader;
		try
		{
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_silinen_kayitlar_hazirlaniyor);
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			MemoryStream memoryStream2 = new MemoryStream();
			((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
			memoryStream2.Position = 0L;
			binaryReader = new BinaryReader(memoryStream2);
			MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
			BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
			if (!binaryReader2.ReadBoolean())
			{
				return false;
			}
			foreach (Tablo item4 in list)
			{
				_ = item4;
				list2.Add(DataTableManualReader(binaryReader2));
			}
			binaryReader2.Dispose();
			binaryReader2 = null;
			memoryStream3.Dispose();
			memoryStream3 = null;
		}
		catch
		{
			return false;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		int num = 0;
		foreach (Tablo item5 in list)
		{
			this.OnProgress2AltBaslikChanged(this, item5.TabloAdi);
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			if (((InternalDataCollectionBase)list2[num].Rows).Count > 0)
			{
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("DELETE FROM " + item5.TabloAdi + " WHERE " + item5.GetRECnoFieldName() + " in (");
					foreach (DataRow item6 in (InternalDataCollectionBase)list2[num].Rows)
					{
						DataRow val = item6;
						stringBuilder.Append((int)val[0]).Append(",");
					}
					stringBuilder.Append("-100)");
					command = new SqliteCommand();
					command.Connection = AppBase.GetDbConnectionSync();
					((DbCommand)command).CommandText = stringBuilder.ToString();
					((DbCommand)command).ExecuteNonQuery();
					((Component)command).Dispose();
					command = null;
					foreach (Tablo item7 in V15_guncellenecektablolar)
					{
						if (item7.TabloID == item5.TabloID)
						{
							item7.UpdateEdildi = true;
							break;
						}
					}
					try
					{
						int num2 = int.Parse(list2[num].Rows[((InternalDataCollectionBase)list2[num].Rows).Count - 1]["TriggerRECno"].ToString());
						command = new SqliteCommand();
						command.Connection = AppBase.GetDbConnectionSync();
						((DbCommand)command).CommandText = "UPDATE OfflineBilgi SET DeleteLastTriggerRecNo=@delete_last_trigger_rec_no WHERE TabloID=@tablo_id";
						command.Parameters.AddWithValue("@tablo_id", (object)item5.TabloID);
						command.Parameters.AddWithValue("@delete_last_trigger_rec_no", (object)num2);
						((DbCommand)command).ExecuteNonQuery();
						((Component)command).Dispose();
						command = null;
					}
					catch
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}
			num++;
		}
		return true;
	}

	private bool V15_Senkronizasyon_Kontrolu()
	{
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Invalid comparison between Unknown and I4
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Expected O, but got Unknown
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		List<Tablo> list = new List<Tablo>();
		List<Tablo> list2 = new List<Tablo>();
		foreach (Tablo item in V15_guncellenecektablolar)
		{
			if (item.UpdateEdildi)
			{
				list2.Add(item);
			}
		}
		if (list2.Count > 0)
		{
			foreach (Tablo item2 in list2)
			{
				try
				{
					string commandText = "SELECT COUNT(*) FROM " + item2.TabloAdi;
					SqliteCommand val = new SqliteCommand
					{
						Connection = AppBase.GetDbConnectionSync(),
						CommandText = commandText
					};
					object obj = ((DbCommand)val).ExecuteScalar();
					((Component)val).Dispose();
					if (obj != null)
					{
						item2.OfflineKayitSayisi = int.Parse(obj.ToString());
					}
					else
					{
						item2.OfflineKayitSayisi = 0;
					}
				}
				catch
				{
					return false;
				}
				try
				{
					string commandText2 = "SELECT " + item2.GetRECnoFieldName() + " FROM " + item2.TabloAdi + " ORDER BY " + item2.GetRECnoFieldName() + " DESC LIMIT 1";
					SqliteCommand val2 = new SqliteCommand
					{
						Connection = AppBase.GetDbConnectionSync(),
						CommandText = commandText2
					};
					object obj3 = ((DbCommand)val2).ExecuteScalar();
					((Component)val2).Dispose();
					if (obj3 != null)
					{
						item2.SonRECno = int.Parse(obj3.ToString());
					}
					else
					{
						item2.SonRECno = 0;
					}
				}
				catch
				{
					return false;
				}
				int num = 0;
				if (servis_kontrolu.ServisVersiyonu > 30005)
				{
					uri = "http://" + servis_kontrolu.ServisAdresi + "/GetRecordCountV3/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + _Platformtools.UrlEncode(item2.TabloAdi) + "/" + _Platformtools.UrlEncode(item2.GetRECnoFieldName()) + "/" + item2.SonRECno + "/" + AppBase._MikroDBName;
				}
				else
				{
					uri = "http://" + servis_kontrolu.ServisAdresi + "/GetRecordCountV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + _Platformtools.UrlEncode(item2.TabloAdi) + "/" + _Platformtools.UrlEncode(item2.GetRECnoFieldName()) + "/" + item2.SonRECno;
				}
				try
				{
					response = HttpWebUtility.GetWebResponseGet(uri, KeepAlive: true, "gzip, deflate", 20000, 20000);
				}
				catch
				{
					num = -1;
				}
				if (response == null)
				{
					num = -1;
				}
				if ((int)response.StatusCode != 200)
				{
					num = -1;
				}
				loResponseStream = new StreamReader(((WebResponse)response).GetResponseStream(), enc);
				Response = loResponseStream.ReadToEnd();
				loResponseStream.Dispose();
				((WebResponse)response).Dispose();
				if (Response.StartsWith("Hata"))
				{
					num = -1;
				}
				try
				{
					num = int.Parse(Response);
				}
				catch
				{
					num = -1;
				}
				if (item2.OfflineKayitSayisi != num)
				{
					list.Add(item2);
					if (denemesayisi > 3)
					{
						try
						{
							string commandText3 = "DELETE FROM " + item2.TabloAdi;
							command = new SqliteCommand();
							command.Connection = AppBase.GetDbConnectionSync();
							((DbCommand)command).CommandText = commandText3;
							((DbCommand)command).ExecuteNonQuery();
							((Component)command).Dispose();
							command = null;
							denemesayisi = 0;
						}
						catch
						{
						}
					}
				}
				if (item2.TabloAdi == "STOKLAR" && item2.UpdateEdildi)
				{
					try
					{
						SqliteCommand val3 = new SqliteCommand
						{
							Connection = AppBase.GetDbConnectionSync()
						};
						string commandText4 = "INSERT INTO STOKLAR_fts(STOKLAR_fts) VALUES('rebuild')";
						((DbCommand)val3).CommandText = commandText4;
						((DbCommand)val3).ExecuteNonQuery();
					}
					catch
					{
					}
				}
				if (item2.TabloAdi == "_FORA_PARAMETRELER" && item2.UpdateEdildi)
				{
					AppBase.KullaniciParametreleri = ParametrelerDefault.MobilKullanici(AppBase._KullaniciAdi);
					ParametreSqlite.ParametreOku(AppBase.GetDbConnectionSync(), AppBase.KullaniciParametreleri, "akilli", AppBase._KullaniciAdi, "", "");
					AppBase._vergitanimlari = new List<VergiTanimi>();
					VergiTanimi vergiTanimi = new VergiTanimi();
					vergiTanimi.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi0KisaAdi")._GetString;
					vergiTanimi.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi0UzunAdi")._GetString;
					vergiTanimi.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi0Yuzde")._GetDouble;
					VergiTanimi vergiTanimi2 = new VergiTanimi();
					vergiTanimi2.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi1KisaAdi")._GetString;
					vergiTanimi2.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi1UzunAdi")._GetString;
					vergiTanimi2.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi1Yuzde")._GetDouble;
					VergiTanimi vergiTanimi3 = new VergiTanimi();
					vergiTanimi3.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi2KisaAdi")._GetString;
					vergiTanimi3.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi2UzunAdi")._GetString;
					vergiTanimi3.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi2Yuzde")._GetDouble;
					VergiTanimi vergiTanimi4 = new VergiTanimi();
					vergiTanimi4.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi3KisaAdi")._GetString;
					vergiTanimi4.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi3UzunAdi")._GetString;
					vergiTanimi4.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi3Yuzde")._GetDouble;
					VergiTanimi vergiTanimi5 = new VergiTanimi();
					vergiTanimi5.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi4KisaAdi")._GetString;
					vergiTanimi5.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi4UzunAdi")._GetString;
					vergiTanimi5.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi4Yuzde")._GetDouble;
					VergiTanimi vergiTanimi6 = new VergiTanimi();
					vergiTanimi6.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi5KisaAdi")._GetString;
					vergiTanimi6.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi5UzunAdi")._GetString;
					vergiTanimi6.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi5Yuzde")._GetDouble;
					VergiTanimi vergiTanimi7 = new VergiTanimi();
					vergiTanimi7.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi6KisaAdi")._GetString;
					vergiTanimi7.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi6UzunAdi")._GetString;
					vergiTanimi7.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi6Yuzde")._GetDouble;
					VergiTanimi vergiTanimi8 = new VergiTanimi();
					vergiTanimi8.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi7KisaAdi")._GetString;
					vergiTanimi8.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi7UzunAdi")._GetString;
					vergiTanimi8.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi7Yuzde")._GetDouble;
					VergiTanimi vergiTanimi9 = new VergiTanimi();
					vergiTanimi9.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi8KisaAdi")._GetString;
					vergiTanimi9.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi8UzunAdi")._GetString;
					vergiTanimi9.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi8Yuzde")._GetDouble;
					VergiTanimi vergiTanimi10 = new VergiTanimi();
					vergiTanimi10.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi9KisaAdi")._GetString;
					vergiTanimi10.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi9UzunAdi")._GetString;
					vergiTanimi10.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi9Yuzde")._GetDouble;
					VergiTanimi vergiTanimi11 = new VergiTanimi();
					vergiTanimi11.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi10KisaAdi")._GetString;
					vergiTanimi11.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi10UzunAdi")._GetString;
					vergiTanimi11.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi10Yuzde")._GetDouble;
					AppBase._vergitanimlari.Add(vergiTanimi);
					AppBase._vergitanimlari.Add(vergiTanimi2);
					AppBase._vergitanimlari.Add(vergiTanimi3);
					AppBase._vergitanimlari.Add(vergiTanimi4);
					AppBase._vergitanimlari.Add(vergiTanimi5);
					AppBase._vergitanimlari.Add(vergiTanimi6);
					AppBase._vergitanimlari.Add(vergiTanimi7);
					AppBase._vergitanimlari.Add(vergiTanimi8);
					AppBase._vergitanimlari.Add(vergiTanimi9);
					AppBase._vergitanimlari.Add(vergiTanimi10);
					AppBase._vergitanimlari.Add(vergiTanimi11);
				}
				if (item2.TabloAdi == "KUR_ISIMLERI" && item2.UpdateEdildi)
				{
					AppBase._doviz_cinsi_tanimlari = DovizCinsiTanimlariSqlite.GetDovizCinsiTanimlari(AppBase.GetDbConnectionSync());
				}
			}
		}
		denemesayisi++;
		if (list.Count > 0)
		{
			V15_guncellenecektablolar = list;
			V15_Senkronizasyon_Main();
		}
		return true;
	}

	private bool V15_OfflineKayitlariGonder_Yeni()
	{
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		sqlite_helper.Open(AppBase.GetOfflineKayitlarDbPath(), ReadOnly: false);
		List<OfflineEvrakV2> offlineEvraklarV = OfflineEvrakSqlite.GetOfflineEvraklarV2(sqlite_helper.GetConnection(), enum_EvrakAktarimDurumu.Aktarilacak, TerstenMi: false, -365);
		this.OnProgress2MaxChanged(this, offlineEvraklarV.Count);
		int num = 0;
		foreach (OfflineEvrakV2 item in offlineEvraklarV)
		{
			if (servis_kontrolu.ServisVersiyonu > 30005)
			{
				uri = "http://" + servis_kontrolu.ServisAdresi + "/SaveOfflineEvrakV3/" + AppBase._FirmaID + "/" + AppBase._MikroDBName;
			}
			else
			{
				uri = "http://" + servis_kontrolu.ServisAdresi + "/SaveOfflineEvrakV2/" + AppBase._FirmaID;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(AppBase._KullaniciAdi);
			binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
			binaryWriter.Write(OfflineEvrakV2.WriteToByteArray(item, 1));
			buffer = memoryStream.ToArray();
			try
			{
				response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
			}
			catch
			{
			}
			_ = response.StatusCode;
			_ = 200;
			Stream stream = new MemoryStream();
			((WebResponse)response).GetResponseStream().CopyTo(stream);
			stream.Position = 0L;
			BinaryReader binaryReader = new BinaryReader(stream);
			try
			{
				if (binaryReader.ReadBoolean())
				{
					OfflineEvrakV2 offlineevrak = OfflineEvrakV2.ReadFromBinaryReader(binaryReader);
					sqlite_helper.Open(AppBase.GetOfflineKayitlarDbPath(), ReadOnly: false);
					OfflineEvrakSqlite.EvrakOfflineGuncelleV2(sqlite_helper.GetConnection(), offlineevrak);
				}
				else
				{
					item.HataString = binaryReader.ReadString();
					sqlite_helper.Open(AppBase.GetOfflineKayitlarDbPath(), ReadOnly: false);
					OfflineEvrakSqlite.EvrakOfflineGuncelleV2(sqlite_helper.GetConnection(), item);
				}
			}
			catch
			{
			}
			((WebResponse)response).Dispose();
			num++;
			this.OnProgress2ProgressChanged(this, num);
		}
		sqlite_helper.Dispose();
		return true;
	}

	private void V15_TabloYapisiKontrolu(Tablo tablo)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		try
		{
			V15_TabloOlustur(tablo);
			command = new SqliteCommand();
			command.Connection = AppBase.GetDbConnectionSync();
			((DbCommand)command).CommandText = "SELECT * FROM " + tablo.TabloAdi + " LIMIT 1";
			SqliteDataReader val = command.ExecuteReader();
			bool flag = false;
			int num = ((DbDataReader)val).FieldCount;
			if (tablo.TabloAdi == "STOKLAR" || tablo.TabloAdi == "CARI_HESAPLAR")
			{
				num -= 2;
			}
			if (num != tablo.Fieldlar.Count)
			{
				flag = true;
			}
			if (!flag)
			{
				for (int i = 0; i < num; i++)
				{
					if (i >= tablo.Fieldlar.Count)
					{
						flag = true;
						break;
					}
					if (((DbDataReader)val).GetName(i) != tablo.Fieldlar[i].Adi)
					{
						flag = true;
						break;
					}
				}
			}
			((DbDataReader)val).Close();
			((DbDataReader)val).Dispose();
			val = null;
			((Component)command).Dispose();
			if (flag)
			{
				command = new SqliteCommand();
				command.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)command).CommandText = "DROP TABLE IF EXISTS [" + tablo.TabloAdi + "]";
				((DbCommand)command).ExecuteNonQuery();
				((Component)command).Dispose();
				V15_TabloOlustur(tablo);
			}
		}
		catch
		{
		}
	}

	private void V15_TabloOlustur(Tablo tablo)
	{
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Expected O, but got Unknown
		List<SqliteCommand> list = new List<SqliteCommand>();
		string text = "CREATE TABLE IF NOT EXISTS [" + tablo.TabloAdi + "] (";
		bool flag = true;
		foreach (Field item in tablo.Fieldlar)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				text += ",";
			}
			text = string.Concat(text, "[", item.Adi, "] ", item.TipiSqLite, " ");
			text = ((!item.NullOlurmu) ? (text + "NOT NULL") : (text + "NULL"));
			if (item.PrimaryKey)
			{
				text += " PRIMARY KEY";
			}
		}
		if (tablo.TabloAdi == "STOKLAR")
		{
			text += ",[sto_kod_upper] TEXT,[sto_isim_upper] TEXT";
		}
		if (tablo.TabloAdi == "CARI_HESAPLAR")
		{
			text += ",[cari_kod_upper] TEXT,[cari_unvan1_upper] TEXT";
		}
		text += ")";
		list.Add(new SqliteCommand(text));
		foreach (string item2 in tablo.Indexler)
		{
			list.Add(new SqliteCommand(item2));
		}
		string text2 = "";
		foreach (SqliteCommand item3 in list)
		{
			try
			{
				item3.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)item3).ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				text2 = ex.ToString();
			}
		}
		if (text2 != "")
		{
			this.OnHataMesajiGoster(this, text2);
		}
		if (tablo.TabloAdi == "STOKLAR")
		{
			try
			{
				((DbCommand)new SqliteCommand("CREATE VIRTUAL TABLE IF NOT EXISTS STOKLAR_fts USING fts4(content='STOKLAR', sto_isim_upper,sto_kod_upper,sto_yabanci_isim,sto_kisa_ismi)")
				{
					Connection = AppBase.GetDbConnectionSync()
				}).ExecuteNonQuery();
			}
			catch
			{
			}
		}
	}

	private void V15_TextDataGuncelle()
	{
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Invalid comparison between Unknown and I4
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Expected O, but got Unknown
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Expected O, but got Unknown
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Expected O, but got Unknown
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Expected O, but got Unknown
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Expected O, but got Unknown
		this.OnProgress1BaslikChanged(this, AppResource.guncelleme_yazboz_bilgisi_guncelleniyor);
		this.OnProgress1MaxChanged(this, 1);
		this.OnProgress1ProgressChanged(this, 0);
		this.OnProgress2BaslikChanged(this, "");
		this.OnProgress2AltBaslikChanged(this, AppResource.guncelleme_kayitlar_cekiliyor);
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 1);
		if (servis_kontrolu.ServisVersiyonu > 30005)
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/Getmye_TextDataV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		}
		else
		{
			uri = "http://" + servis_kontrolu.ServisAdresi + "/Getmye_TextData/" + _Platformtools.UrlEncode(AppBase._FirmaID);
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
		}
		catch
		{
			return;
		}
		if (response == null || (int)response.StatusCode != 200)
		{
			return;
		}
		BinaryReader binaryReader;
		DataTable val;
		try
		{
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_kayitlar_hazirlaniyor);
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			MemoryStream memoryStream2 = new MemoryStream();
			((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
			memoryStream2.Position = 0L;
			binaryReader = new BinaryReader(memoryStream2);
			MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
			BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
			if (!binaryReader2.ReadBoolean())
			{
				return;
			}
			val = DataTableManualReader(binaryReader2);
			binaryReader2.Dispose();
			binaryReader2 = null;
			memoryStream3.Dispose();
			memoryStream3 = null;
		}
		catch
		{
			return;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_kayitlar_yaziliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		this.OnProgress2AltBaslikChanged(this, "mye_TextData");
		this.OnProgress2MaxChanged(this, ((InternalDataCollectionBase)val.Rows).Count);
		this.OnProgress2ProgressChanged(this, 0);
		int num = 0;
		int num2 = 0;
		SqliteTransaction val2 = AppBase.GetDbConnectionSync().BeginTransaction();
		SqliteCommand val3 = new SqliteCommand("CREATE TABLE IF NOT EXISTS [mye_TextData] ([TableID] INTEGER,[RecID_DBCno] INTEGER,[RecID_RECno] INTEGER,[Data] TEXT NULL )");
		val3.Connection = AppBase.GetDbConnectionSync();
		val3.Transaction = val2;
		try
		{
			((DbCommand)val3).ExecuteNonQuery();
		}
		catch
		{
			((DbTransaction)val2).Rollback();
			((Component)val3).Dispose();
			return;
		}
		val3 = new SqliteCommand("CREATE INDEX IF NOT EXISTS NDX_mye_TextData_01 ON mye_TextData (TableID,RecID_DBCno,RecID_RECno)");
		val3.Connection = AppBase.GetDbConnectionSync();
		try
		{
			((DbCommand)val3).ExecuteNonQuery();
		}
		catch
		{
			((DbTransaction)val2).Rollback();
			((Component)val3).Dispose();
			return;
		}
		val3 = new SqliteCommand("DELETE FROM mye_TextData");
		val3.Connection = AppBase.GetDbConnectionSync();
		try
		{
			((DbCommand)val3).ExecuteNonQuery();
		}
		catch
		{
			((DbTransaction)val2).Rollback();
			((Component)val3).Dispose();
			return;
		}
		string text = "INSERT INTO mye_TextData (TableID,RecID_DBCno,RecID_RECno,Data) VALUES" + "(@TableID,@RecID_DBCno,@RecID_RECno,@Data)";
		int num3 = 0;
		int num4 = 0;
		val3 = new SqliteCommand(text);
		val3.Connection = AppBase.GetDbConnectionSync();
		val3.Parameters.Add("@TableID", (DbType)11);
		val3.Parameters.Add("@RecID_DBCno", (DbType)11);
		val3.Parameters.Add("@RecID_RECno", (DbType)11);
		val3.Parameters.Add("@Data", (DbType)16);
		foreach (DataRow item in (InternalDataCollectionBase)val.Rows)
		{
			DataRow val4 = item;
			((DbParameter)val3.Parameters[0]).Value = val4[0];
			((DbParameter)val3.Parameters[1]).Value = val4[1];
			((DbParameter)val3.Parameters[2]).Value = val4[2];
			((DbParameter)val3.Parameters[3]).Value = val4[3];
			try
			{
				((DbCommand)val3).ExecuteNonQuery();
			}
			catch
			{
				((DbTransaction)val2).Rollback();
				((Component)val3).Dispose();
				return;
			}
			if (num3 == 10)
			{
				this.OnProgress2ProgressChanged(this, num);
				num3 = 0;
			}
			if (num4 == 5000)
			{
				((DbTransaction)val2).Commit();
				val2 = AppBase.GetDbConnectionSync().BeginTransaction();
				num4 = 0;
			}
			num2++;
			num++;
			num3++;
			num4++;
		}
		((DbTransaction)val2).Commit();
		((Component)val3).Dispose();
	}

	private void V16_SenkronizasyonOncesi()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		this.OnProgress1MaxChanged(this, 1);
		command = new SqliteCommand();
		((DbCommand)command).CommandText = "CREATE TABLE IF NOT EXISTS [OfflineBilgi] ([TabloID] INTEGER NOT NULL PRIMARY KEY, [SonGuncellemeZamani] TEXT NULL,[UpdateLastTriggerRecNo] INTEGER NULL,[DeleteLastTriggerRecNo] INTEGER NULL)";
		command.Connection = AppBase.GetDbConnectionSync();
		((DbCommand)command).ExecuteNonQuery();
		((Component)command).Dispose();
		command = null;
		denemesayisi = 0;
		if (servis_kontrolu.ServisVersiyonu < 30007)
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_servis_guncelleme_gerekmekte);
			IslemiBitir();
		}
		else
		{
			V16_Senkronizasyon_Giris();
		}
	}

	private void V16_Senkronizasyon_Giris()
	{
		BirSeferdekiMaksimumKayitSayisi = 5000;
		this.OnProgress1BaslikChanged(this, AppResource.guncelleme_parametreler_guncelleniyor);
		this.OnProgress1MaxChanged(this, 1);
		this.OnProgress1ProgressChanged(this, 0);
		this.OnProgress2BaslikChanged(this, "");
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		TabloV16 mikroV16_Tablo__FORA_PARAMETRELER = TabloHelperV16.Get_MikroV16_Tablo__FORA_PARAMETRELER();
		V16_guncellenecektablolar = new List<TabloV16>();
		V16_guncellenecektablolar.Add(mikroV16_Tablo__FORA_PARAMETRELER);
		if (!V16_Senkronizasyon_Main())
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_hata_parametreler_cekilemedi + " Fora Mikro -> Kurulum -> Fora mobil veritabanı kurulumu adımından gerekli kurulumu yapınız.");
			IslemiBitir();
			return;
		}
		bool flag = true;
		if (_EvrakGonder)
		{
			this.OnProgress1BaslikChanged(this, AppResource.guncelleme_kayitlar_gonderiliyor);
			this.OnProgress1MaxChanged(this, 1);
			this.OnProgress1ProgressChanged(this, 0);
			this.OnProgress2BaslikChanged(this, "");
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_kayitlar_gonderiliyor);
			flag = V16_OfflineKayitlariGonder_Yeni();
		}
		if (!flag)
		{
			this.OnHataMesajiGoster(this, AppResource.guncelleme_kayitlar_gonderilemedi);
		}
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_tablo_yapisi_kontrol_ediliyor);
		foreach (TabloV16 item in TabloHelperV16.GetMikroV16DefaultTablolar())
		{
			this.OnProgress2AltBaslikChanged(this, item.TabloAdi);
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			V16_TabloYapisiKontrolu(item);
		}
		this.OnProgress1BaslikChanged(this, AppResource.guncelleme_tablolar_guncelleniyor);
		this.OnProgress1MaxChanged(this, 1);
		this.OnProgress1ProgressChanged(this, 0);
		this.OnProgress2BaslikChanged(this, "");
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		if (_SenkronizeEt)
		{
			mye_TextDataGuncelle = AppBase.KullaniciParametreleri._GetParametre("mye_TextDataGuncelle")._GetBoolean;
			V16_guncellenecektablolar = new List<TabloV16>();
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOKLAR")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOKLAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAPLAR")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_HESAPLAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SORUMLULUK_MERKEZLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_SORUMLULUK_MERKEZLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_PROJELER")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_PROJELER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_ANA_GRUPLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_ANA_GRUPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_ALT_GRUPLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_ALT_GRUPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_URETICILERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_URETICILERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_REYONLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_REYONLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_MARKALARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_MARKALARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_CARI_ISKONTO_TANIMLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_CARI_ISKONTO_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_SATIS_FIYAT_LISTELERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_SATIS_FIYAT_LISTELERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_SATIS_FIYAT_LISTE_TANIMLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_SATIS_FIYAT_LISTE_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_BARKOD_TANIMLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_BARKOD_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_ADRESLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_HESAP_ADRESLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_YETKILILERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_HESAP_YETKILILERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DEPOLAR")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_DEPOLAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_ODEME_PLANLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_ODEME_PLANLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_KASALAR")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_KASALAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_HAREKETLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_HESAP_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SIPARISLER")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_SIPARISLER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_PROFORMA_SIPARISLER")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_PROFORMA_SIPARISLER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DOVIZ_KURLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_DOVIZ_KURLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SATIS_SARTLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_SATIS_SARTLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SATINALMA_SARTLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_SATINALMA_SARTLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_HAREKETLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_ODEME_EMIRLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_ODEME_EMIRLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_TEMINATLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_HESAP_TEMINATLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_FIRMALAR")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_FIRMALAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_SUBELER")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_SUBELER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_BANKALAR")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_BANKALAR());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_MASRAF_HESAPLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_MASRAF_HESAPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_YEREL_BANKA_KODLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_YEREL_BANKA_KODLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt__ZIYARET_HAREKETLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo__ZIYARET_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_PERSONEL_TANIMLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_PERSONEL_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_BOLGELERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_HESAP_BOLGELERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_GRUPLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CARI_HESAP_GRUPLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_TESLIM_TURLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_TESLIM_TURLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DEPOLAR_ARASI_SIPARISLER")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_DEPOLAR_ARASI_SIPARISLER());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_EVRAK_ACIKLAMALARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_EVRAK_ACIKLAMALARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_KATEGORILERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_KATEGORILERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_SEKTORLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_SEKTORLERI());
			}
			V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_IHRACAT_DOSYALARI());
			V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_PAKET_TANIMLARI());
			if (servis_kontrolu.ServisVersiyonu >= 260 && AppBase.GetMikroDBNameWithPath().Contains("MikroDB_V15"))
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_KUR_ISIMLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_BEDEN_HAREKETLERI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_BEDEN_HAREKETLERI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_BEDEN_TANIMLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_BEDEN_TANIMLARI());
			}
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_RENK_TANIMLARI")._GetBoolean)
			{
				V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_RENK_TANIMLARI());
			}
			V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_STOK_SERINO_TANIMLARI());
			V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_KAPAMA_NEDENLERI_TANIMLARI());
			V16_guncellenecektablolar.Add(TabloHelperV16.Get_MikroV16_Tablo_CIHAZ_HAREKETLERI());
			if (!V16_Senkronizasyon_Main())
			{
				IslemiBitir();
				return;
			}
		}
		StokFotograflariniGuncelle_Yeni();
		if (mye_TextDataGuncelle)
		{
			V16_TextDataGuncelle();
		}
		IslemiBitir();
	}

	private bool V16_Senkronizasyon_Main()
	{
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Invalid comparison between Unknown and I4
		foreach (TabloV16 item in V16_guncellenecektablolar)
		{
			item.DegisenKayitSayisi = 0;
			item.OfflineKayitSayisi = 0;
			item.UpdateEdildi = false;
			item.OfflineLastUpdateTriggerRecNo = 0;
			item.OfflineLastDeleteTriggerRecNo = 0;
		}
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_online_tablo_yapisi_kontrol_ediliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		uri = "http://" + servis_kontrolu.ServisAdresi + "/V16_GetTabloYapisiTopluV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		binaryWriter.Write(V16_guncellenecektablolar.Count);
		foreach (TabloV16 item2 in V16_guncellenecektablolar)
		{
			binaryWriter.Write(item2.TabloAdi);
		}
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 40000, 40000, "application");
		}
		catch
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if ((int)response.StatusCode != 200)
		{
			return false;
		}
		BinaryReader binaryReader;
		try
		{
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_online_tablo_yapisi_kontrol_ediliyor);
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			MemoryStream memoryStream2 = new MemoryStream();
			((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
			memoryStream2.Position = 0L;
			binaryReader = new BinaryReader(memoryStream2);
			MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
			BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
			if (!binaryReader2.ReadBoolean())
			{
				return false;
			}
			foreach (TabloV16 item3 in V16_guncellenecektablolar)
			{
				string text = binaryReader2.ReadString();
				List<FieldV16> list = new List<FieldV16>();
				foreach (FieldV16 item4 in item3.Fieldlar)
				{
					if (text.Contains(item4.Adi))
					{
						list.Add(item4);
					}
				}
				item3.Fieldlar = list;
			}
			binaryReader2.Dispose();
			binaryReader2 = null;
			memoryStream3.Dispose();
			memoryStream3 = null;
		}
		catch
		{
			return false;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		if (!V16_Degisen_Kayitlari_Senkronize_Et())
		{
			this.OnHataMesajiGoster(this, AppResource.genel_hata + " : " + AppResource.guncelleme_degisen_kayitlar_cekilemedi);
			return false;
		}
		if (!V16_Silinen_Kayitlari_Senkronize_Et())
		{
			this.OnHataMesajiGoster(this, AppResource.genel_hata + " : " + AppResource.guncelleme_silinen_kayitlar_cekilemedi);
			return false;
		}
		if (!V16_Senkronizasyon_Kontrolu())
		{
			this.OnHataMesajiGoster(this, AppResource.genel_hata + " : " + AppResource.guncelleme_kontrol_islemi_yapilamadi);
			return false;
		}
		return true;
	}

	private bool V16_Degisen_Kayitlari_Senkronize_Et()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_08c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cb: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Invalid comparison between Unknown and I4
		//IL_094c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected O, but got Unknown
		//IL_0a16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1d: Expected O, but got Unknown
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_061a: Invalid comparison between Unknown and I4
		//IL_0a37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3e: Invalid comparison between Unknown and I4
		//IL_0ce8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf2: Expected O, but got Unknown
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_kontrol_ediliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		foreach (TabloV16 item in V16_guncellenecektablolar)
		{
			try
			{
				string commandText = "SELECT UpdateLastTriggerRecNo FROM OfflineBilgi WHERE TabloID=@tablo_id LIMIT 1";
				command = new SqliteCommand();
				command.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)command).CommandText = commandText;
				command.Parameters.AddWithValue("@tablo_id", (object)item.TabloID);
				object obj = ((DbCommand)command).ExecuteScalar();
				((Component)command).Dispose();
				command = null;
				if (obj != null)
				{
					item.OfflineLastUpdateTriggerRecNo = int.Parse(obj.ToString());
				}
				else
				{
					item.OfflineLastUpdateTriggerRecNo = 0;
				}
			}
			catch
			{
				return false;
			}
		}
		uri = "http://" + servis_kontrolu.ServisAdresi + "/V16_GetTabloDegisenKayitSayilariV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		binaryWriter.Write(V16_guncellenecektablolar.Count);
		foreach (TabloV16 item2 in V16_guncellenecektablolar)
		{
			binaryWriter.Write(item2.TabloID);
			binaryWriter.Write(item2.OfflineLastUpdateTriggerRecNo);
		}
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 40000, 40000, "application");
		}
		catch
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if ((int)response.StatusCode != 200)
		{
			return false;
		}
		MemoryStream memoryStream2 = new MemoryStream();
		((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
		memoryStream2.Position = 0L;
		BinaryReader binaryReader = new BinaryReader(memoryStream2);
		try
		{
			if (!binaryReader.ReadBoolean())
			{
				return false;
			}
			foreach (TabloV16 item3 in V16_guncellenecektablolar)
			{
				item3.DegisenKayitSayisi = binaryReader.ReadInt32();
			}
		}
		catch
		{
			return false;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		int num = 0;
		List<TabloV16> list = new List<TabloV16>();
		foreach (TabloV16 item4 in V16_guncellenecektablolar)
		{
			if (item4.DegisenKayitSayisi > 0)
			{
				list.Add(item4);
			}
			if (item4.DegisenKayitSayisi > num)
			{
				num = item4.DegisenKayitSayisi;
			}
		}
		if (list.Count > 0)
		{
			int maksimum = num / BirSeferdekiMaksimumKayitSayisi + 1;
			this.OnProgress1MaxChanged(this, maksimum);
			int num2 = 1;
			while (true)
			{
				this.OnProgress1ProgressChanged(this, num2 - 1);
				if (num2 != 1)
				{
					foreach (TabloV16 item5 in V16_guncellenecektablolar)
					{
						try
						{
							string commandText2 = "SELECT UpdateLastTriggerRecNo FROM OfflineBilgi WHERE TabloID=@tablo_id LIMIT 1";
							command = new SqliteCommand();
							command.Connection = AppBase.GetDbConnectionSync();
							((DbCommand)command).CommandText = commandText2;
							command.Parameters.AddWithValue("@tablo_id", (object)item5.TabloID);
							object obj5 = ((DbCommand)command).ExecuteScalar();
							((Component)command).Dispose();
							command = null;
							if (obj5 != null)
							{
								item5.OfflineLastUpdateTriggerRecNo = int.Parse(obj5.ToString());
							}
							else
							{
								item5.OfflineLastUpdateTriggerRecNo = 0;
							}
						}
						catch
						{
							return false;
						}
					}
				}
				List<DataTable> list2 = new List<DataTable>();
				List<int> list3 = new List<int>();
				this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_indiriliyor);
				this.OnProgress2AltBaslikChanged(this, "");
				this.OnProgress2MaxChanged(this, 1);
				this.OnProgress2ProgressChanged(this, 0);
				uri = "http://" + servis_kontrolu.ServisAdresi + "/V16_GetDegisenKayitlarTopluV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
				memoryStream = new MemoryStream();
				binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(AppBase._KullaniciAdi);
				binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
				binaryWriter.Write(BirSeferdekiMaksimumKayitSayisi);
				binaryWriter.Write(list.Count);
				foreach (TabloV16 item6 in list)
				{
					binaryWriter.Write(item6.TabloAdi);
					binaryWriter.Write(item6.TabloID);
					binaryWriter.Write(item6.OfflineLastUpdateTriggerRecNo);
					binaryWriter.Write(item6.Fieldlar[0].Adi);
					binaryWriter.Write(item6.GetCekilecekFieldlar(""));
				}
				buffer = memoryStream.ToArray();
				try
				{
					response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
				}
				catch
				{
					return false;
				}
				if (response == null)
				{
					return false;
				}
				if ((int)response.StatusCode != 200)
				{
					return false;
				}
				try
				{
					this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_hazirlaniyor);
					this.OnProgress2AltBaslikChanged(this, "");
					this.OnProgress2MaxChanged(this, 1);
					this.OnProgress2ProgressChanged(this, 0);
					memoryStream2 = new MemoryStream();
					((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
					memoryStream2.Position = 0L;
					binaryReader = new BinaryReader(memoryStream2);
					MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
					BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
					if (!binaryReader2.ReadBoolean())
					{
						return false;
					}
					foreach (TabloV16 item7 in list)
					{
						_ = item7;
						list2.Add(DataTableManualReader(binaryReader2));
					}
					binaryReader2.Dispose();
					binaryReader2 = null;
					memoryStream3.Dispose();
					memoryStream3 = null;
				}
				catch
				{
					return false;
				}
				binaryReader.Dispose();
				((WebResponse)response).Dispose();
				this.OnProgress2BaslikChanged(this, AppResource.guncelleme_degisen_kayitlar_yaziliyor);
				this.OnProgress2AltBaslikChanged(this, "");
				this.OnProgress2MaxChanged(this, 1);
				this.OnProgress2ProgressChanged(this, 0);
				int num3 = 0;
				foreach (TabloV16 item8 in list)
				{
					this.OnProgress2AltBaslikChanged(this, item8.TabloAdi);
					this.OnProgress2MaxChanged(this, ((InternalDataCollectionBase)list2[num3].Rows).Count);
					this.OnProgress2ProgressChanged(this, 0);
					int num4 = 0;
					int num5 = 0;
					SqliteTransaction val = AppBase.GetDbConnectionSync().BeginTransaction();
					string text = item8.GetCekilecekFieldlar("");
					string cekilecekFieldlar = item8.GetCekilecekFieldlar("@");
					if (item8.TabloAdi == "STOKLAR")
					{
						text += ",sto_kod_upper,sto_isim_upper";
					}
					if (item8.TabloAdi == "CARI_HESAPLAR")
					{
						text += ",cari_kod_upper,cari_unvan1_upper";
					}
					string text2 = "INSERT OR REPLACE INTO " + item8.TabloAdi + " (" + text + ") VALUES";
					text2 = text2 + "(" + cekilecekFieldlar;
					if (item8.TabloAdi == "STOKLAR")
					{
						text2 += ",@sto_kod_upper,@sto_isim_upper";
					}
					if (item8.TabloAdi == "CARI_HESAPLAR")
					{
						text2 += ",@cari_kod_upper,@cari_unvan1_upper";
					}
					text2 += ")";
					int num6 = 0;
					int num7 = 0;
					SqliteCommand val2 = new SqliteCommand(text2);
					val2.Connection = AppBase.GetDbConnectionSync();
					val2.Transaction = val;
					int num8 = 0;
					foreach (FieldV16 item9 in item8.Fieldlar)
					{
						string fullName = list2[num3].Columns[num8].DataType.FullName;
						val2.Parameters.Add("@" + item9.Adi.ToLower().Replace("ı", "i"), GetDbType(fullName));
						num8++;
					}
					int num9 = 0;
					if (item8.TabloAdi == "STOKLAR")
					{
						val2.Parameters.Add("@sto_kod_upper", (DbType)16);
						val2.Parameters.Add("@sto_isim_upper", (DbType)16);
						num9 = 1;
					}
					if (item8.TabloAdi == "CARI_HESAPLAR")
					{
						val2.Parameters.Add("@cari_kod_upper", (DbType)16);
						val2.Parameters.Add("@cari_unvan1_upper", (DbType)16);
						num9 = 2;
					}
					foreach (DataRow item10 in (InternalDataCollectionBase)list2[num3].Rows)
					{
						DataRow val3 = item10;
						int num10 = 0;
						foreach (FieldV16 item11 in item8.Fieldlar)
						{
							if ((int)item11.SqlType == 14)
							{
								((DbParameter)val2.Parameters[num10]).Value = ((Guid)val3[num10]).ToByteArray();
							}
							else
							{
								((DbParameter)val2.Parameters[num10]).Value = val3[num10];
							}
							num10++;
						}
						if (num9 == 1)
						{
							((DbParameter)val2.Parameters[num10]).Value = val3["sto_kod"].ToString().ToUpper();
							((DbParameter)val2.Parameters[num10 + 1]).Value = val3["sto_isim"].ToString().ToUpper();
						}
						if (num9 == 2)
						{
							((DbParameter)val2.Parameters[num10]).Value = val3["cari_kod"].ToString().ToUpper();
							((DbParameter)val2.Parameters[num10 + 1]).Value = val3["cari_unvan1"].ToString().ToUpper();
						}
						try
						{
							((DbCommand)val2).ExecuteNonQuery();
						}
						catch
						{
							((DbTransaction)val).Rollback();
							((Component)val2).Dispose();
							return false;
						}
						if (num6 == 10)
						{
							this.OnProgress2ProgressChanged(this, num4);
							num6 = 0;
						}
						if (num7 == 5000)
						{
							((DbTransaction)val).Commit();
							foreach (TabloV16 item12 in V16_guncellenecektablolar)
							{
								if (item12.TabloID == item8.TabloID)
								{
									item12.UpdateEdildi = true;
									break;
								}
							}
							val = AppBase.GetDbConnectionSync().BeginTransaction();
							num7 = 0;
						}
						num5++;
						num4++;
						num6++;
						num7++;
					}
					((DbTransaction)val).Commit();
					foreach (TabloV16 item13 in V16_guncellenecektablolar)
					{
						if (item13.TabloID == item8.TabloID)
						{
							item13.UpdateEdildi = true;
							break;
						}
					}
					((Component)val2).Dispose();
					if (((InternalDataCollectionBase)list2[num3].Rows).Count > 0)
					{
						try
						{
							int num11 = int.Parse(list2[num3].Rows[((InternalDataCollectionBase)list2[num3].Rows).Count - 1]["TriggerRECno"].ToString());
							command = new SqliteCommand();
							command.Connection = AppBase.GetDbConnectionSync();
							((DbCommand)command).CommandText = "INSERT OR REPLACE INTO OfflineBilgi (TabloID,UpdateLastTriggerRecNo) VALUES (@tablo_id,@update_last_trigger_rec_no)";
							command.Parameters.AddWithValue("@tablo_id", (object)item8.TabloID);
							command.Parameters.AddWithValue("@update_last_trigger_rec_no", (object)num11);
							((DbCommand)command).ExecuteNonQuery();
							((Component)command).Dispose();
							command = null;
						}
						catch
						{
						}
					}
					if (((InternalDataCollectionBase)list2[num3].Rows).Count != BirSeferdekiMaksimumKayitSayisi)
					{
						list3.Add(item8.TabloID);
					}
					num3++;
				}
				foreach (int item14 in list3)
				{
					int num12 = 0;
					using (List<TabloV16>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext() && enumerator.Current.TabloID != item14)
						{
							num12++;
						}
					}
					list.RemoveAt(num12);
				}
				if (list.Count == 0)
				{
					break;
				}
				num2++;
			}
		}
		return true;
	}

	private bool V16_Silinen_Kayitlari_Senkronize_Et()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Invalid comparison between Unknown and I4
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Expected O, but got Unknown
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Expected O, but got Unknown
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Expected O, but got Unknown
		List<TabloV16> list = new List<TabloV16>();
		foreach (TabloV16 item in V16_guncellenecektablolar)
		{
			list.Add(item);
		}
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_silinen_kayitlar_kontrol_ediliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		foreach (TabloV16 item2 in list)
		{
			try
			{
				string commandText = "SELECT DeleteLastTriggerRecNo FROM OfflineBilgi WHERE TabloID=@tablo_id LIMIT 1";
				command = new SqliteCommand();
				command.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)command).CommandText = commandText;
				command.Parameters.AddWithValue("@tablo_id", (object)item2.TabloID);
				object obj = ((DbCommand)command).ExecuteScalar();
				((Component)command).Dispose();
				command = null;
				if (obj != null)
				{
					int result = 0;
					int.TryParse(obj.ToString(), out result);
					item2.OfflineLastDeleteTriggerRecNo = result;
				}
				else
				{
					item2.OfflineLastDeleteTriggerRecNo = 0;
				}
			}
			catch
			{
				return false;
			}
		}
		List<DataTable> list2 = new List<DataTable>();
		uri = "http://" + servis_kontrolu.ServisAdresi + "/V16_GetSilinenKayitlarTopluV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		binaryWriter.Write(BirSeferdekiMaksimumKayitSayisi);
		binaryWriter.Write(list.Count);
		foreach (TabloV16 item3 in list)
		{
			binaryWriter.Write(item3.TabloID);
			binaryWriter.Write(item3.OfflineLastDeleteTriggerRecNo);
		}
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
		}
		catch
		{
			return false;
		}
		if (response == null)
		{
			return false;
		}
		if ((int)response.StatusCode != 200)
		{
			return false;
		}
		BinaryReader binaryReader;
		try
		{
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_silinen_kayitlar_hazirlaniyor);
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			MemoryStream memoryStream2 = new MemoryStream();
			((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
			memoryStream2.Position = 0L;
			binaryReader = new BinaryReader(memoryStream2);
			MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
			BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
			if (!binaryReader2.ReadBoolean())
			{
				return false;
			}
			foreach (TabloV16 item4 in list)
			{
				_ = item4;
				list2.Add(DataTableManualReader(binaryReader2));
			}
			binaryReader2.Dispose();
			binaryReader2 = null;
			memoryStream3.Dispose();
			memoryStream3 = null;
		}
		catch
		{
			return false;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		int num = 0;
		foreach (TabloV16 item5 in list)
		{
			this.OnProgress2AltBaslikChanged(this, item5.TabloAdi);
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			if (((InternalDataCollectionBase)list2[num].Rows).Count > 0)
			{
				try
				{
					foreach (DataRow item6 in (InternalDataCollectionBase)list2[num].Rows)
					{
						DataRow val = item6;
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append("DELETE FROM " + item5.TabloAdi + " WHERE " + item5.Fieldlar[0].Adi + " =@deger");
						command = new SqliteCommand();
						command.Connection = AppBase.GetDbConnectionSync();
						command.Parameters.AddWithValue("@deger", (object)((Guid)val[0]).ToByteArray());
						((DbCommand)command).CommandText = stringBuilder.ToString();
						((DbCommand)command).ExecuteNonQuery();
						((Component)command).Dispose();
						command = null;
					}
					foreach (TabloV16 item7 in V16_guncellenecektablolar)
					{
						if (item7.TabloID == item5.TabloID)
						{
							item7.UpdateEdildi = true;
							break;
						}
					}
					try
					{
						int num2 = int.Parse(list2[num].Rows[((InternalDataCollectionBase)list2[num].Rows).Count - 1]["TriggerRECno"].ToString());
						command = new SqliteCommand();
						command.Connection = AppBase.GetDbConnectionSync();
						((DbCommand)command).CommandText = "UPDATE OfflineBilgi SET DeleteLastTriggerRecNo=@delete_last_trigger_rec_no WHERE TabloID=@tablo_id";
						command.Parameters.AddWithValue("@tablo_id", (object)item5.TabloID);
						command.Parameters.AddWithValue("@delete_last_trigger_rec_no", (object)num2);
						((DbCommand)command).ExecuteNonQuery();
						((Component)command).Dispose();
						command = null;
					}
					catch
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}
			num++;
		}
		return true;
	}

	private bool V16_Senkronizasyon_Kontrolu()
	{
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Invalid comparison between Unknown and I4
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Expected O, but got Unknown
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Expected O, but got Unknown
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		List<TabloV16> list = new List<TabloV16>();
		List<TabloV16> list2 = new List<TabloV16>();
		foreach (TabloV16 item in V16_guncellenecektablolar)
		{
			if (item.UpdateEdildi)
			{
				list2.Add(item);
			}
		}
		if (list2.Count > 0)
		{
			foreach (TabloV16 item2 in list2)
			{
				try
				{
					string commandText = "SELECT COUNT(*) FROM " + item2.TabloAdi;
					SqliteCommand val = new SqliteCommand
					{
						Connection = AppBase.GetDbConnectionSync(),
						CommandText = commandText
					};
					object obj = ((DbCommand)val).ExecuteScalar();
					((Component)val).Dispose();
					if (obj != null)
					{
						item2.OfflineKayitSayisi = int.Parse(obj.ToString());
					}
					else
					{
						item2.OfflineKayitSayisi = 0;
					}
				}
				catch
				{
					return false;
				}
				int num = 0;
				uri = "http://" + servis_kontrolu.ServisAdresi + "/V16_GetRecordCountV3/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + _Platformtools.UrlEncode(item2.TabloAdi) + "/" + AppBase._MikroDBName;
				try
				{
					response = HttpWebUtility.GetWebResponseGet(uri, KeepAlive: true, "gzip, deflate", 20000, 20000);
				}
				catch
				{
					num = -1;
				}
				if (response == null)
				{
					num = -1;
				}
				if ((int)response.StatusCode != 200)
				{
					num = -1;
				}
				loResponseStream = new StreamReader(((WebResponse)response).GetResponseStream(), enc);
				Response = loResponseStream.ReadToEnd();
				loResponseStream.Dispose();
				((WebResponse)response).Dispose();
				if (Response.StartsWith("Hata"))
				{
					num = -1;
				}
				try
				{
					num = int.Parse(Response);
				}
				catch
				{
					num = -1;
				}
				if (item2.OfflineKayitSayisi != num)
				{
					list.Add(item2);
					if (denemesayisi > 3)
					{
						try
						{
							string commandText2 = "DELETE FROM " + item2.TabloAdi;
							command = new SqliteCommand();
							command.Connection = AppBase.GetDbConnectionSync();
							((DbCommand)command).CommandText = commandText2;
							((DbCommand)command).ExecuteNonQuery();
							command = new SqliteCommand();
							((DbCommand)command).CommandText = "INSERT OR REPLACE INTO OfflineBilgi (TabloID,UpdateLastTriggerRecNo) VALUES (@tablo_id,@update_last_trigger_rec_no)";
							command.Parameters.AddWithValue("@tablo_id", (object)item2.TabloID);
							command.Parameters.AddWithValue("@update_last_trigger_rec_no", (object)0);
							command.Connection = AppBase.GetDbConnectionSync();
							((DbCommand)command).CommandText = commandText2;
							((DbCommand)command).ExecuteNonQuery();
							((Component)command).Dispose();
							command = null;
							denemesayisi = 0;
						}
						catch
						{
						}
					}
				}
				if (item2.TabloAdi == "STOKLAR" && item2.UpdateEdildi)
				{
					try
					{
						SqliteCommand val2 = new SqliteCommand
						{
							Connection = AppBase.GetDbConnectionSync()
						};
						string commandText3 = "INSERT INTO STOKLAR_fts(STOKLAR_fts) VALUES('rebuild')";
						((DbCommand)val2).CommandText = commandText3;
						((DbCommand)val2).ExecuteNonQuery();
					}
					catch
					{
					}
				}
				if (item2.TabloAdi == "_FORA_PARAMETRELER" && item2.UpdateEdildi)
				{
					AppBase.KullaniciParametreleri = ParametrelerDefault.MobilKullanici(AppBase._KullaniciAdi);
					ParametreSqlite.ParametreOku(AppBase.GetDbConnectionSync(), AppBase.KullaniciParametreleri, "akilli", AppBase._KullaniciAdi, "", "");
					AppBase._vergitanimlari = new List<VergiTanimi>();
					VergiTanimi vergiTanimi = new VergiTanimi();
					vergiTanimi.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi0KisaAdi")._GetString;
					vergiTanimi.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi0UzunAdi")._GetString;
					vergiTanimi.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi0Yuzde")._GetDouble;
					VergiTanimi vergiTanimi2 = new VergiTanimi();
					vergiTanimi2.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi1KisaAdi")._GetString;
					vergiTanimi2.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi1UzunAdi")._GetString;
					vergiTanimi2.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi1Yuzde")._GetDouble;
					VergiTanimi vergiTanimi3 = new VergiTanimi();
					vergiTanimi3.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi2KisaAdi")._GetString;
					vergiTanimi3.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi2UzunAdi")._GetString;
					vergiTanimi3.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi2Yuzde")._GetDouble;
					VergiTanimi vergiTanimi4 = new VergiTanimi();
					vergiTanimi4.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi3KisaAdi")._GetString;
					vergiTanimi4.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi3UzunAdi")._GetString;
					vergiTanimi4.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi3Yuzde")._GetDouble;
					VergiTanimi vergiTanimi5 = new VergiTanimi();
					vergiTanimi5.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi4KisaAdi")._GetString;
					vergiTanimi5.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi4UzunAdi")._GetString;
					vergiTanimi5.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi4Yuzde")._GetDouble;
					VergiTanimi vergiTanimi6 = new VergiTanimi();
					vergiTanimi6.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi5KisaAdi")._GetString;
					vergiTanimi6.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi5UzunAdi")._GetString;
					vergiTanimi6.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi5Yuzde")._GetDouble;
					VergiTanimi vergiTanimi7 = new VergiTanimi();
					vergiTanimi7.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi6KisaAdi")._GetString;
					vergiTanimi7.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi6UzunAdi")._GetString;
					vergiTanimi7.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi6Yuzde")._GetDouble;
					VergiTanimi vergiTanimi8 = new VergiTanimi();
					vergiTanimi8.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi7KisaAdi")._GetString;
					vergiTanimi8.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi7UzunAdi")._GetString;
					vergiTanimi8.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi7Yuzde")._GetDouble;
					VergiTanimi vergiTanimi9 = new VergiTanimi();
					vergiTanimi9.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi8KisaAdi")._GetString;
					vergiTanimi9.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi8UzunAdi")._GetString;
					vergiTanimi9.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi8Yuzde")._GetDouble;
					VergiTanimi vergiTanimi10 = new VergiTanimi();
					vergiTanimi10.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi9KisaAdi")._GetString;
					vergiTanimi10.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi9UzunAdi")._GetString;
					vergiTanimi10.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi9Yuzde")._GetDouble;
					VergiTanimi vergiTanimi11 = new VergiTanimi();
					vergiTanimi11.KisaAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi10KisaAdi")._GetString;
					vergiTanimi11.UzunAdi = AppBase.KullaniciParametreleri._GetParametre("Vergi10UzunAdi")._GetString;
					vergiTanimi11.Yuzde = AppBase.KullaniciParametreleri._GetParametre("Vergi10Yuzde")._GetDouble;
					AppBase._vergitanimlari.Add(vergiTanimi);
					AppBase._vergitanimlari.Add(vergiTanimi2);
					AppBase._vergitanimlari.Add(vergiTanimi3);
					AppBase._vergitanimlari.Add(vergiTanimi4);
					AppBase._vergitanimlari.Add(vergiTanimi5);
					AppBase._vergitanimlari.Add(vergiTanimi6);
					AppBase._vergitanimlari.Add(vergiTanimi7);
					AppBase._vergitanimlari.Add(vergiTanimi8);
					AppBase._vergitanimlari.Add(vergiTanimi9);
					AppBase._vergitanimlari.Add(vergiTanimi10);
					AppBase._vergitanimlari.Add(vergiTanimi11);
				}
				if (item2.TabloAdi == "KUR_ISIMLERI" && item2.UpdateEdildi)
				{
					AppBase._doviz_cinsi_tanimlari = DovizCinsiTanimlariSqlite.GetDovizCinsiTanimlari(sqlite_helper.GetConnection());
				}
			}
		}
		denemesayisi++;
		if (list.Count > 0)
		{
			V16_guncellenecektablolar = list;
			V16_Senkronizasyon_Main();
		}
		return true;
	}

	private bool V16_OfflineKayitlariGonder_Yeni()
	{
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		sqlite_helper.Open(AppBase.GetOfflineKayitlarDbPath(), ReadOnly: false);
		List<OfflineEvrakV2> offlineEvraklarV = OfflineEvrakSqlite.GetOfflineEvraklarV2(sqlite_helper.GetConnection(), enum_EvrakAktarimDurumu.Aktarilacak, TerstenMi: false, -365);
		this.OnProgress2MaxChanged(this, offlineEvraklarV.Count);
		int num = 0;
		foreach (OfflineEvrakV2 item in offlineEvraklarV)
		{
			if (servis_kontrolu.ServisVersiyonu > 30005)
			{
				uri = "http://" + servis_kontrolu.ServisAdresi + "/SaveOfflineEvrakV3/" + AppBase._FirmaID + "/" + AppBase._MikroDBName;
			}
			else
			{
				uri = "http://" + servis_kontrolu.ServisAdresi + "/SaveOfflineEvrakV2/" + AppBase._FirmaID;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(AppBase._KullaniciAdi);
			binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
			binaryWriter.Write(OfflineEvrakV2.WriteToByteArray(item, 1));
			buffer = memoryStream.ToArray();
			try
			{
				response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
			}
			catch
			{
			}
			_ = response.StatusCode;
			_ = 200;
			Stream stream = new MemoryStream();
			((WebResponse)response).GetResponseStream().CopyTo(stream);
			stream.Position = 0L;
			BinaryReader binaryReader = new BinaryReader(stream);
			try
			{
				if (binaryReader.ReadBoolean())
				{
					OfflineEvrakV2 offlineevrak = OfflineEvrakV2.ReadFromBinaryReader(binaryReader);
					sqlite_helper.Open(AppBase.GetOfflineKayitlarDbPath(), ReadOnly: false);
					OfflineEvrakSqlite.EvrakOfflineGuncelleV2(sqlite_helper.GetConnection(), offlineevrak);
				}
				else
				{
					item.HataString = binaryReader.ReadString();
					sqlite_helper.Open(AppBase.GetOfflineKayitlarDbPath(), ReadOnly: false);
					OfflineEvrakSqlite.EvrakOfflineGuncelleV2(sqlite_helper.GetConnection(), item);
				}
			}
			catch
			{
			}
			((WebResponse)response).Dispose();
			num++;
			this.OnProgress2ProgressChanged(this, num);
		}
		sqlite_helper.Dispose();
		return true;
	}

	private void V16_TabloYapisiKontrolu(TabloV16 tablo)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		try
		{
			V16_TabloOlustur(tablo);
			command = new SqliteCommand();
			command.Connection = AppBase.GetDbConnectionSync();
			((DbCommand)command).CommandText = "SELECT * FROM " + tablo.TabloAdi + " LIMIT 1";
			SqliteDataReader val = command.ExecuteReader();
			bool flag = false;
			int num = ((DbDataReader)val).FieldCount;
			if (tablo.TabloAdi == "STOKLAR" || tablo.TabloAdi == "CARI_HESAPLAR")
			{
				num -= 2;
			}
			if (num != tablo.Fieldlar.Count)
			{
				flag = true;
			}
			if (!flag)
			{
				for (int i = 0; i < num; i++)
				{
					if (i >= tablo.Fieldlar.Count)
					{
						flag = true;
						break;
					}
					if (((DbDataReader)val).GetName(i) != tablo.Fieldlar[i].Adi)
					{
						flag = true;
						break;
					}
				}
			}
			((DbDataReader)val).Close();
			((DbDataReader)val).Dispose();
			val = null;
			((Component)command).Dispose();
			if (flag)
			{
				command = new SqliteCommand();
				command.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)command).CommandText = "DROP TABLE IF EXISTS [" + tablo.TabloAdi + "]";
				((DbCommand)command).ExecuteNonQuery();
				((Component)command).Dispose();
				command = new SqliteCommand();
				command.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)command).CommandText = "INSERT OR REPLACE INTO OfflineBilgi (TabloID,UpdateLastTriggerRecNo) VALUES (@tablo_id,@update_last_trigger_rec_no)";
				command.Parameters.AddWithValue("@tablo_id", (object)tablo.TabloID);
				command.Parameters.AddWithValue("@update_last_trigger_rec_no", (object)0);
				((DbCommand)command).ExecuteNonQuery();
				((Component)command).Dispose();
				V16_TabloOlustur(tablo);
			}
		}
		catch
		{
		}
	}

	private void V16_TabloOlustur(TabloV16 tablo)
	{
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		List<SqliteCommand> list = new List<SqliteCommand>();
		string text = "CREATE TABLE IF NOT EXISTS [" + tablo.TabloAdi + "] (";
		bool flag = true;
		foreach (FieldV16 item in tablo.Fieldlar)
		{
			if (!flag)
			{
				text += ",";
			}
			text = string.Concat(text, "[", item.Adi, "] ", item.SqliteType, " ");
			text = ((!flag) ? (text + "NULL") : (text + "NOT NULL PRIMARY KEY"));
			flag = false;
		}
		if (tablo.TabloAdi == "STOKLAR")
		{
			text += ",[sto_kod_upper] TEXT,[sto_isim_upper] TEXT";
		}
		if (tablo.TabloAdi == "CARI_HESAPLAR")
		{
			text += ",[cari_kod_upper] TEXT,[cari_unvan1_upper] TEXT";
		}
		text += ")";
		list.Add(new SqliteCommand(text));
		foreach (string item2 in tablo.Indexler)
		{
			list.Add(new SqliteCommand(item2));
		}
		string text2 = "";
		foreach (SqliteCommand item3 in list)
		{
			try
			{
				item3.Connection = AppBase.GetDbConnectionSync();
				((DbCommand)item3).ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				text2 = ex.ToString();
			}
		}
		if (text2 != "")
		{
			this.OnHataMesajiGoster(this, text2);
		}
		if (tablo.TabloAdi == "STOKLAR")
		{
			try
			{
				((DbCommand)new SqliteCommand("CREATE VIRTUAL TABLE IF NOT EXISTS STOKLAR_fts USING fts4(content='STOKLAR', sto_isim_upper,sto_kod_upper,sto_yabanci_isim,sto_kisa_ismi)")
				{
					Connection = AppBase.GetDbConnectionSync()
				}).ExecuteNonQuery();
			}
			catch
			{
			}
		}
	}

	private void V16_TextDataGuncelle()
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Invalid comparison between Unknown and I4
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Expected O, but got Unknown
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Expected O, but got Unknown
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Expected O, but got Unknown
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Expected O, but got Unknown
		this.OnProgress1BaslikChanged(this, AppResource.guncelleme_yazboz_bilgisi_guncelleniyor);
		this.OnProgress1MaxChanged(this, 1);
		this.OnProgress1ProgressChanged(this, 0);
		this.OnProgress2BaslikChanged(this, "");
		this.OnProgress2AltBaslikChanged(this, AppResource.guncelleme_kayitlar_cekiliyor);
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 1);
		uri = "http://" + servis_kontrolu.ServisAdresi + "/V16_Getmye_TextDataV2/" + _Platformtools.UrlEncode(AppBase._FirmaID) + "/" + AppBase._MikroDBName;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(AppBase._KullaniciAdi);
		binaryWriter.Write(_Platformtools.EncryptText("drjbq8777!#45", AppBase._Sifre, useHashing: true));
		buffer = memoryStream.ToArray();
		try
		{
			response = HttpWebUtility.GetWebResponsePost(uri, buffer, KeepAlive: true, "gzip, deflate", 120000, 120000, "application");
		}
		catch
		{
			return;
		}
		if (response == null || (int)response.StatusCode != 200)
		{
			return;
		}
		BinaryReader binaryReader;
		DataTable val;
		try
		{
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_kayitlar_hazirlaniyor);
			this.OnProgress2AltBaslikChanged(this, "");
			this.OnProgress2MaxChanged(this, 1);
			this.OnProgress2ProgressChanged(this, 0);
			MemoryStream memoryStream2 = new MemoryStream();
			((WebResponse)response).GetResponseStream().CopyTo(memoryStream2);
			memoryStream2.Position = 0L;
			binaryReader = new BinaryReader(memoryStream2);
			MemoryStream memoryStream3 = new MemoryStream((byte[])_Platformtools.ToObjectGZip(memoryStream2.ToArray()));
			BinaryReader binaryReader2 = new BinaryReader(memoryStream3);
			if (!binaryReader2.ReadBoolean())
			{
				return;
			}
			val = DataTableManualReader(binaryReader2);
			binaryReader2.Dispose();
			binaryReader2 = null;
			memoryStream3.Dispose();
			memoryStream3 = null;
		}
		catch
		{
			return;
		}
		binaryReader.Dispose();
		((WebResponse)response).Dispose();
		this.OnProgress2BaslikChanged(this, AppResource.guncelleme_kayitlar_yaziliyor);
		this.OnProgress2AltBaslikChanged(this, "");
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 0);
		this.OnProgress2AltBaslikChanged(this, "mye_TextData");
		this.OnProgress2MaxChanged(this, ((InternalDataCollectionBase)val.Rows).Count);
		this.OnProgress2ProgressChanged(this, 0);
		int num = 0;
		int num2 = 0;
		SqliteTransaction val2 = AppBase.GetDbConnectionSync().BeginTransaction();
		SqliteCommand val3 = new SqliteCommand("CREATE TABLE IF NOT EXISTS [mye_TextData] ([TableID] INTEGER,[Record_uid] BLOB,[Data] TEXT NULL )");
		val3.Connection = AppBase.GetDbConnectionSync();
		val3.Transaction = val2;
		try
		{
			((DbCommand)val3).ExecuteNonQuery();
		}
		catch
		{
			((DbTransaction)val2).Rollback();
			((Component)val3).Dispose();
			return;
		}
		val3 = new SqliteCommand("CREATE INDEX IF NOT EXISTS NDX_mye_TextData_01 ON mye_TextData (Record_uid)");
		val3.Connection = AppBase.GetDbConnectionSync();
		try
		{
			((DbCommand)val3).ExecuteNonQuery();
		}
		catch
		{
			((DbTransaction)val2).Rollback();
			((Component)val3).Dispose();
			return;
		}
		val3 = new SqliteCommand("DELETE FROM mye_TextData");
		val3.Connection = AppBase.GetDbConnectionSync();
		try
		{
			((DbCommand)val3).ExecuteNonQuery();
		}
		catch
		{
			((DbTransaction)val2).Rollback();
			((Component)val3).Dispose();
			return;
		}
		string text = "INSERT INTO mye_TextData (TableID,Record_uid,Data) VALUES" + "(@TableID,@Record_uid,@Data)";
		int num3 = 0;
		int num4 = 0;
		val3 = new SqliteCommand(text);
		val3.Connection = AppBase.GetDbConnectionSync();
		val3.Parameters.Add("@TableID", (DbType)11);
		val3.Parameters.Add("@Record_uid", (DbType)1);
		val3.Parameters.Add("@Data", (DbType)16);
		foreach (DataRow item in (InternalDataCollectionBase)val.Rows)
		{
			DataRow val4 = item;
			((DbParameter)val3.Parameters[0]).Value = val4[0];
			((DbParameter)val3.Parameters[1]).Value = ((Guid)val4[1]).ToByteArray();
			((DbParameter)val3.Parameters[2]).Value = val4[2];
			try
			{
				((DbCommand)val3).ExecuteNonQuery();
			}
			catch
			{
				((DbTransaction)val2).Rollback();
				((Component)val3).Dispose();
				return;
			}
			if (num3 == 10)
			{
				this.OnProgress2ProgressChanged(this, num);
				num3 = 0;
			}
			if (num4 == 5000)
			{
				((DbTransaction)val2).Commit();
				val2 = AppBase.GetDbConnectionSync().BeginTransaction();
				num4 = 0;
			}
			num2++;
			num++;
			num3++;
			num4++;
		}
		((DbTransaction)val2).Commit();
		((Component)val3).Dispose();
	}

	private void StokFotograflariniGuncelle_Yeni()
	{
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Expected O, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Expected O, but got Unknown
		this.OnProgress1BaslikChanged(this, AppResource.guncelleme_stok_fotograflari_guncelleniyor);
		this.OnProgress1MaxChanged(this, 1);
		this.OnProgress1ProgressChanged(this, 0);
		this.OnProgress2BaslikChanged(this, "");
		this.OnProgress2AltBaslikChanged(this, AppResource.guncelleme_liste_cekiliyor);
		this.OnProgress2MaxChanged(this, 1);
		this.OnProgress2ProgressChanged(this, 1);
		try
		{
			DataTable stokFotoList = GetStokFotoList();
			if (!Directory.Exists(_PicturePath + "/Fora"))
			{
				Directory.CreateDirectory(_PicturePath + "/Fora");
			}
			if (!Directory.Exists(_PicturePath + "/Fora/" + AppBase._FirmaID))
			{
				Directory.CreateDirectory(_PicturePath + "/Fora/" + AppBase._FirmaID);
			}
			if (!Directory.Exists(_PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar"))
			{
				Directory.CreateDirectory(_PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar");
			}
			if (!Directory.Exists(_PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar/lmd"))
			{
				Directory.CreateDirectory(_PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar/lmd");
			}
			DataTable val = new DataTable("files");
			val.Columns.Add("filename", typeof(string));
			val.Columns.Add("tarih", typeof(DateTime));
			foreach (DataRow item in (InternalDataCollectionBase)stokFotoList.Rows)
			{
				DataRow val2 = item;
				bool flag = true;
				_ = _PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar/" + (string)val2[0];
				string text = _PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar/lmd/" + (string)val2[0];
				try
				{
					if (File.Exists(text + ".lmd"))
					{
						StreamReader streamReader = new StreamReader(text + ".lmd");
						string? s = streamReader.ReadLine();
						DateTime dateTime = (DateTime)val2[1];
						DateTime dateTime2 = DateTime.Parse(s);
						if (dateTime.Second == dateTime2.Second && dateTime.Minute == dateTime2.Minute && dateTime.Hour == dateTime2.Hour)
						{
							flag = false;
						}
						streamReader.Close();
					}
				}
				catch
				{
				}
				if (flag)
				{
					val.Rows.Add(new object[2]
					{
						(string)val2[0],
						(DateTime)val2[1]
					});
				}
			}
			this.OnProgress2BaslikChanged(this, AppResource.guncelleme_stok_fotograflari_guncelleniyor);
			this.OnProgress2MaxChanged(this, ((InternalDataCollectionBase)val.Rows).Count);
			this.OnProgress2AltBaslikChanged(this, "");
			int num = 1;
			foreach (DataRow item2 in (InternalDataCollectionBase)val.Rows)
			{
				DataRow val3 = item2;
				this.OnProgress2AltBaslikChanged(this, (string)val3[0]);
				StokFotoCek((string)val3[0], (DateTime)val3[1]);
				this.OnProgress2ProgressChanged(this, num);
				num++;
			}
		}
		catch
		{
		}
	}

	private void StokFotoCek(string filename, DateTime LastWriteTime)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		uri = "http://" + servis_kontrolu.ServisAdresi + "/GetStokFoto/" + AppBase._FirmaID + "/" + filename;
		try
		{
			response = HttpWebUtility.GetWebResponseGet(uri, KeepAlive: true, "gzip, deflate", 120000, 120000);
		}
		catch
		{
			return;
		}
		if (response == null || (int)response.StatusCode != 200)
		{
			return;
		}
		string path = _PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar/" + filename;
		string text = _PicturePath + "/Fora/" + AppBase._FirmaID + "/stoklar/lmd/" + filename;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		using (FileStream destination = File.Create(path))
		{
			((WebResponse)response).GetResponseStream().CopyTo(destination);
		}
		((WebResponse)response).Dispose();
		try
		{
			if (File.Exists(text + ".lmd"))
			{
				File.Delete(text + ".lmd");
			}
			using StreamWriter streamWriter = new StreamWriter(text + ".lmd", append: true);
			streamWriter.WriteLine(LastWriteTime.ToString());
		}
		catch
		{
		}
	}

	private DataTable GetStokFotoList()
	{
		return GetStokFotoListV3();
	}

	private DataTable GetStokFotoListV3()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Invalid comparison between Unknown and I4
		DataTable val = new DataTable("files");
		val.Columns.Add("filename", typeof(string));
		val.Columns.Add("tarih", typeof(DateTime));
		uri = "http://" + servis_kontrolu.ServisAdresi + "/GetStokFotoListV3/" + AppBase._FirmaID;
		try
		{
			response = HttpWebUtility.GetWebResponseGet(uri, KeepAlive: true, "gzip, deflate", 60000, 60000);
		}
		catch
		{
			return null;
		}
		if (response == null)
		{
			return null;
		}
		if ((int)response.StatusCode != 200)
		{
			return null;
		}
		Stream stream = new MemoryStream();
		((WebResponse)response).GetResponseStream().CopyTo(stream);
		stream.Position = 0L;
		BinaryReader binaryReader = new BinaryReader(stream);
		int num = binaryReader.ReadInt32();
		try
		{
			for (int i = 0; i < num; i++)
			{
				val.Rows.Add(new object[2]
				{
					binaryReader.ReadString(),
					new DateTime(binaryReader.ReadInt64())
				});
			}
		}
		catch
		{
		}
		((WebResponse)response).Dispose();
		return val;
	}

	private DbType GetDbType(string tip)
	{
		return (DbType)(tip switch
		{
			"System.Guid" => 1, 
			"System.DateTime" => 6, 
			"System.Double" => 8, 
			"System.Byte" => 2, 
			"System.Int32" => 11, 
			"System.String" => 16, 
			"System.Boolean" => 3, 
			"System.Int16" => 10, 
			_ => 16, 
		});
	}

	private DataTable DataTableManualReader(BinaryReader reader)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		DataTable val = new DataTable();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string text = reader.ReadString();
			Type type = Type.GetType(reader.ReadString());
			val.Columns.Add(text, type);
		}
		object[] array = new object[num];
		int num2 = reader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			for (int k = 0; k < num; k++)
			{
				if (val.Columns[k].DataType == typeof(string))
				{
					array[k] = reader.ReadString();
				}
				else if (val.Columns[k].DataType == typeof(double))
				{
					array[k] = double.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
				}
				else if (val.Columns[k].DataType == typeof(DateTime))
				{
					array[k] = new DateTime(reader.ReadInt64());
				}
				else if (val.Columns[k].DataType == typeof(byte))
				{
					array[k] = reader.ReadByte();
				}
				else if (val.Columns[k].DataType == typeof(short))
				{
					array[k] = reader.ReadInt16();
				}
				else if (val.Columns[k].DataType == typeof(int))
				{
					array[k] = reader.ReadInt32();
				}
				else if (val.Columns[k].DataType == typeof(long))
				{
					array[k] = reader.ReadInt64();
				}
				else if (val.Columns[k].DataType == typeof(bool))
				{
					array[k] = reader.ReadBoolean();
				}
				else
				{
					array[k] = reader.ReadString();
				}
			}
			val.Rows.Add(array);
		}
		return val;
	}

	private void IslemiBitir()
	{
		try
		{
			command = null;
			reader = null;
		}
		catch
		{
		}
		this.OnGuncellemeBitti(this);
	}
}
