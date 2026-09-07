using System;
using System.IO;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisPaylasTask
{
	public delegate void RaporStokSiparisPaylasTaskBittiHandler(object sender, string Format, string cevap, MemoryStream SonucMS);

	private RaporStokSiparisSonuc _rapor;

	private string _format;

	private string _dosyaismi;

	private MemoryStream _SonucMS;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event RaporStokSiparisPaylasTaskBittiHandler OnRaporStokSiparisPaylasTaskBitti;

	public void Baslat(string format, string dosyaismi, RaporStokSiparisSonuc rapor)
	{
		_rapor = rapor;
		_format = format;
		_dosyaismi = dosyaismi;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		_SonucMS = new MemoryStream();
		try
		{
			_Cevap = "";
			servis_kontrolu = new ServisKontrolu(269);
			if (servis_kontrolu.Sonuc == enum_servis_kontrol_sonuc.ServisCalisiyor)
			{
				try
				{
					_SonucMS = RaporStokSiparisRestClient.GetRaporStokSiparisFile(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _format, _dosyaismi, _rapor);
				}
				catch (Exception ex)
				{
					_Cevap = "Hata : " + AppResource.genel_bilgiler_cekilemedi + " : " + ex.ToString();
				}
			}
			else
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
		}
		catch (Exception ex2)
		{
			_Cevap = "Hata : " + ex2.ToString();
		}
		servis_kontrolu = null;
		this.OnRaporStokSiparisPaylasTaskBitti(this, _format, _Cevap, _SonucMS);
	}
}
