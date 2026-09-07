using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Siparis;
using Fora.Mikro.StokHareket;

namespace Fora.Mikro.DepolarArasiSiparisler;

public class GetDepolarArasiSiparisEvrakTask
{
	public delegate void GetDepolarArasiSiparisEvrakTaskBittiHandler(object sender, string Cevap, Evrak evrak);

	private string _evrak_seri;

	private int _evrak_sira;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event GetDepolarArasiSiparisEvrakTaskBittiHandler OnGetDepolarArasiSiparisEvrakTaskBitti;

	public void Baslat(string evrak_seri, int evrak_sira)
	{
		_evrak_seri = evrak_seri;
		_evrak_sira = evrak_sira;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		Evrak evrak = new Evrak();
		_Cevap = "";
		if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DEPOLAR_ARASI_SIPARISLER")._GetBoolean)
		{
			try
			{
				evrak = EvrakSqlite.GetEvrak(AppBase.GetDbConnectionSync(), _evrak_seri, _evrak_sira, enum_GenelEvrakTipleri.DepolarArasiSiparis, AppBase.KullaniciParametreleri._GetParametre("AlternatifDovizCinsi")._GetInt, enum_sip_cins.NormalSiparis, enum_sth_cins.Toptan);
			}
			catch (Exception ex)
			{
				_Cevap = "Hata : " + ex.ToString();
			}
		}
		else
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
					evrak = DepolarArasiSevkRestClient.GetDepolarArasiSiparisEvrakV2(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _evrak_seri, _evrak_sira);
				}
				catch (Exception ex2)
				{
					_Cevap = "Hata : " + AppResource.genel_bilgiler_cekilemedi + " : " + ex2.ToString();
				}
			}
		}
		this.OnGetDepolarArasiSiparisEvrakTaskBitti(this, _Cevap, evrak);
	}
}
