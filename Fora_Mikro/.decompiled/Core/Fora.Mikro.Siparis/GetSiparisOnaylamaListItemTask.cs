using System;
using System.Collections.Generic;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Siparis;

public class GetSiparisOnaylamaListItemTask
{
	public delegate void GetSiparisOnaylamaListItemBittiHandler(object sender, string Cevap, List<SiparisOnaylamaListItem> SonucListe);

	private enum_CariListelemeSecenekleri _CariListelemeSecenek;

	private string _CariListelemeAktifGrup;

	private string _TemsilciKodu;

	private string _Cevap;

	private List<SiparisOnaylamaListItem> _SonucListe;

	private ServisKontrolu servis_kontrolu;

	public event GetSiparisOnaylamaListItemBittiHandler OnGetSiparisOnaylamaListItemBitti;

	public void Baslat(enum_CariListelemeSecenekleri CariListelemeSecenek, string CariListelemeAktifGrup, string TemsilciKodu)
	{
		_CariListelemeSecenek = CariListelemeSecenek;
		_CariListelemeAktifGrup = CariListelemeAktifGrup;
		_TemsilciKodu = TemsilciKodu;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		try
		{
			_SonucListe = new List<SiparisOnaylamaListItem>();
			_Cevap = "";
			servis_kontrolu = new ServisKontrolu(285);
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
					_SonucListe = SiparislerRestClient.GetSiparisOnaylamaListItem(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _CariListelemeSecenek, _CariListelemeAktifGrup, _TemsilciKodu);
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
		this.OnGetSiparisOnaylamaListItemBitti(this, _Cevap, _SonucListe);
	}
}
