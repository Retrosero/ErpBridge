using System;
using System.Collections.Generic;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.StokHareket;

public class GetNakliyedekiUrunlerTask
{
	public delegate void GetNakliyedekiUrunlerBittiHandler(object sender, List<STOK_HAREKETLERI> StokHareketleri, string Cevap);

	private int _NakliyeDepo;

	private int _HedefDepo;

	private List<STOK_HAREKETLERI> _StokHareketleri;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event GetNakliyedekiUrunlerBittiHandler OnGetNakliyedekiUrunlerBitti;

	public void Baslat(int NakliyeDepo, int HedefDepo)
	{
		_NakliyeDepo = NakliyeDepo;
		_HedefDepo = HedefDepo;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		try
		{
			_StokHareketleri = new List<STOK_HAREKETLERI>();
			_Cevap = "";
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_HAREKETLERI")._GetBoolean)
			{
				try
				{
					_StokHareketleri = StokHareketleriSqlite.GetNakliyedekiUrunler(AppBase.GetDbConnectionSync(), _NakliyeDepo, _HedefDepo);
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
						_StokHareketleri = DepolarArasiSevkRestClient.GetNakliyedekiUrunlerV2(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _NakliyeDepo, _HedefDepo);
					}
					catch (Exception ex2)
					{
						_Cevap = "Hata : " + AppResource.genel_bilgiler_cekilemedi + " : " + ex2.ToString();
					}
				}
			}
		}
		catch (Exception ex3)
		{
			_Cevap = "Hata : " + ex3.ToString();
		}
		servis_kontrolu = null;
		this.OnGetNakliyedekiUrunlerBitti(this, _StokHareketleri, _Cevap);
	}
}
