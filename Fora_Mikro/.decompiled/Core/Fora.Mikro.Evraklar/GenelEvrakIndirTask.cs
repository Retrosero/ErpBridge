using System;
using System.IO;
using System.Net;
using System.Web;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Evraklar;

public class GenelEvrakIndirTask
{
	public delegate void GenelEvrakIndirBittiHandler(object sender, string Cevap, string OlusturulanDosya, string Format);

	private string _format;

	private string _formdosyaismi;

	private enum_GenelEvrakTipleri _evraktipi;

	private string _evrakseri;

	private string _evraksira;

	private string _FileNameWithPath;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event GenelEvrakIndirBittiHandler OnGenelEvrakIndirBitti;

	public void Baslat(string FileNameWithPath, string format, string formdosyaismi, enum_GenelEvrakTipleri evraktipi, string evrakseri, string evraksira)
	{
		_format = format;
		_formdosyaismi = formdosyaismi;
		_evraktipi = evraktipi;
		_evrakseri = evrakseri;
		_evraksira = evraksira;
		_FileNameWithPath = FileNameWithPath;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		try
		{
			servis_kontrolu = new ServisKontrolu(1);
			if (servis_kontrolu.Sonuc != enum_servis_kontrol_sonuc.ServisCalisiyor)
			{
				switch (servis_kontrolu.Sonuc)
				{
				case enum_servis_kontrol_sonuc.AgBaglantisiYok:
					_Cevap = "Hata : " + AppResource.guncelleme_internete_baglanilamadi;
					break;
				case enum_servis_kontrol_sonuc.ServiseErisilemedi:
					_Cevap = "Hata : " + AppResource.guncelleme_servise_erisilemedi;
					break;
				case enum_servis_kontrol_sonuc.ServisVersiyonuEski:
					_Cevap = "Hata : " + AppResource.guncelleme_servis_guncelleme_gerekmekte;
					break;
				}
			}
			if (_Cevap == "")
			{
				string text = "";
				ServisKontrolu servisKontrolu = new ServisKontrolu(1);
				if (servisKontrolu.ServisVersiyonu > 30005)
				{
					string[] obj = new string[20]
					{
						"http://",
						servisKontrolu.ServisAdresi,
						"/GetGenelEvrakRaporV2/",
						AppBase._FirmaID,
						"/",
						HttpUtility.UrlEncode(AppBase._KullaniciAdi),
						"/",
						HttpUtility.UrlEncode(AppBase._Sifre),
						"/",
						_format,
						"/",
						_formdosyaismi,
						"/",
						null,
						null,
						null,
						null,
						null,
						null,
						null
					};
					int evraktipi = (int)_evraktipi;
					obj[13] = evraktipi.ToString();
					obj[14] = "/_";
					obj[15] = _evrakseri;
					obj[16] = "/";
					obj[17] = _evraksira;
					obj[18] = "/";
					obj[19] = AppBase._MikroDBName;
					text = string.Concat(obj);
				}
				else
				{
					string[] obj2 = new string[18]
					{
						"http://",
						servisKontrolu.ServisAdresi,
						"/GetGenelEvrakRapor/",
						AppBase._FirmaID,
						"/",
						HttpUtility.UrlEncode(AppBase._KullaniciAdi),
						"/",
						HttpUtility.UrlEncode(AppBase._Sifre),
						"/",
						_format,
						"/",
						_formdosyaismi,
						"/",
						null,
						null,
						null,
						null,
						null
					};
					int evraktipi = (int)_evraktipi;
					obj2[13] = evraktipi.ToString();
					obj2[14] = "/_";
					obj2[15] = _evrakseri;
					obj2[16] = "/";
					obj2[17] = _evraksira;
					text = string.Concat(obj2);
				}
				Stream streamFromUrl = GetStreamFromUrl(text);
				if (File.Exists(_FileNameWithPath))
				{
					try
					{
						File.Delete(_FileNameWithPath);
					}
					catch
					{
					}
				}
				using FileStream fileStream = File.Open(_FileNameWithPath, FileMode.Create);
				byte[] array = new byte[1024];
				int count;
				while ((count = streamFromUrl.Read(array, 0, array.Length)) > 0)
				{
					fileStream.Write(array, 0, count);
				}
			}
		}
		catch (Exception ex)
		{
			_Cevap = "Hata : " + ex.ToString();
		}
		servis_kontrolu = null;
		this.OnGenelEvrakIndirBitti(this, _Cevap, _FileNameWithPath, _format);
	}

	private static Stream GetStreamFromUrl(string url)
	{
		byte[] buffer = null;
		WebClient webClient = new WebClient();
		try
		{
			buffer = webClient.DownloadData(url);
		}
		finally
		{
			((IDisposable)webClient)?.Dispose();
		}
		return new MemoryStream(buffer);
	}
}
