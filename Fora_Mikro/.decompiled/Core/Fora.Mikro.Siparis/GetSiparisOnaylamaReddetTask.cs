using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Siparis;

public class GetSiparisOnaylamaReddetTask
{
	public delegate void GetSiparisOnaylamaReddetBittiHandler(object sender, string Hata);

	private string _evrak_seri;

	private string _evrak_sira;

	private string _kapama_nedeni;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event GetSiparisOnaylamaReddetBittiHandler OnGetSiparisOnaylamaReddetBitti;

	public void Baslat(string evrak_seri, string evrak_sira, string kapama_nedeni)
	{
		_evrak_seri = evrak_seri;
		_evrak_sira = evrak_sira;
		_kapama_nedeni = kapama_nedeni;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		try
		{
			_Cevap = "";
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
					SiparislerRestClient.GetSiparisOnaylamaReddet(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _evrak_seri, _evrak_sira, _kapama_nedeni);
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
		this.OnGetSiparisOnaylamaReddetBitti(this, _Cevap);
	}
}
