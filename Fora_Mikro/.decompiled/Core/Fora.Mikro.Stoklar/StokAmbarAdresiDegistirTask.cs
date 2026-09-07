using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Stoklar;

public class StokAmbarAdresiDegistirTask
{
	public delegate void StokAmbarAdresiDegistirBittiHandler(object sender, string Cevap);

	private string _stok_kodu;

	private string _ambaradresleri;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event StokAmbarAdresiDegistirBittiHandler OnStokAmbarAdresiDegistirTaskBitti;

	public void Baslat(string stok_kodu, string ambaradresleri)
	{
		_stok_kodu = stok_kodu;
		_ambaradresleri = ambaradresleri;
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
				try
				{
					_Cevap = StokRestClient.StokAmbarAdresiDegistir(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _stok_kodu, _ambaradresleri);
				}
				catch (Exception ex)
				{
					_Cevap = "Hata : " + AppResource.genel_bilgiler_cekilemedi + " : " + ex.ToString();
				}
			}
		}
		catch (Exception ex2)
		{
			_Cevap = "Hata : " + ex2.ToString();
		}
		servis_kontrolu = null;
		this.OnStokAmbarAdresiDegistirTaskBitti(this, _Cevap);
	}
}
