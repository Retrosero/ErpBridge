using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Siparis;

namespace Fora.Mikro.Evraklar;

public class GetEvrakTask
{
	public delegate void GetEvrakTaskBittiHandler(object sender, string Cevap, Evrak evrak);

	private enum_GenelEvrakTipleri _evrak_tipi;

	private string _evrak_seri;

	private int _evrak_sira;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event GetEvrakTaskBittiHandler OnGetEvrakTaskBitti;

	public void Baslat(enum_GenelEvrakTipleri evrak_tipi, string evrak_seri, int evrak_sira)
	{
		_evrak_tipi = evrak_tipi;
		_evrak_seri = evrak_seri;
		_evrak_sira = evrak_sira;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		Evrak evrak = new Evrak();
		try
		{
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
					evrak = SiparislerRestClient.GetEvrak(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _evrak_tipi, _evrak_seri, _evrak_sira.ToString());
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
		this.OnGetEvrakTaskBitti(this, _Cevap, evrak);
	}
}
