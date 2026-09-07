using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Siparis;

public class GetSiparisOnaylamaOnaylaTask
{
	public delegate void GetSiparisOnaylamaOnaylaBittiHandler(object sender, string _Cevap);

	private string _evrak_seri;

	private string _evrak_sira;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event GetSiparisOnaylamaOnaylaBittiHandler OnGetSiparisOnaylamaOnaylaBitti;

	public void Baslat(string evrak_seri, string evrak_sira)
	{
		_evrak_seri = evrak_seri;
		_evrak_sira = evrak_sira;
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
					SiparislerRestClient.GetSiparisOnaylamaOnayla(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _evrak_seri, _evrak_sira);
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
		this.OnGetSiparisOnaylamaOnaylaBitti(this, _Cevap);
	}
}
