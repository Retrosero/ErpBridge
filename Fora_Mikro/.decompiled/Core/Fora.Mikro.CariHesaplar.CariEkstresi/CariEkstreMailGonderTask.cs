using System;
using System.Collections.Generic;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.CariHesaplar.CariEkstresi;

public class CariEkstreMailGonderTask
{
	public delegate void CariEkstreMailGonderBittiHandler(object sender, string Cevap);

	private CariDetayBilgileri _caridetaybilgileri;

	private string _alicilar;

	private string _konu;

	private string _mesaj;

	private string _format;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event CariEkstreMailGonderBittiHandler OnCariEkstreMailGonderBitti;

	public void Baslat(CariDetayBilgileri caridetaybilgileri, string alicilar, string konu, string mesaj, string format)
	{
		_caridetaybilgileri = caridetaybilgileri;
		_alicilar = alicilar;
		_konu = konu;
		_mesaj = mesaj;
		_format = format;
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
			CariEkstreButun cariEkstreButun = new CariEkstreButun();
			if (_Cevap == "")
			{
				cariEkstreButun.cari = _caridetaybilgileri.cari;
				cariEkstreButun.tarih = DateTime.Now;
				List<CariEkstreGrup> list = new List<CariEkstreGrup>();
				foreach (CariEkstre item in _caridetaybilgileri.cari_ekstre)
				{
					if (item.BaslikMi)
					{
						CariEkstreGrup cariEkstreGrup = new CariEkstreGrup();
						cariEkstreGrup.GrupBaslik = item.BaslikMesaji;
						cariEkstreGrup.Satirlar = new List<CariEkstreString>();
						list.Add(cariEkstreGrup);
						continue;
					}
					CariEkstreString cariEkstreString = new CariEkstreString();
					cariEkstreString.Tarih = item.cha_tarihi;
					cariEkstreString.VadeTarihi = item.VadeTarihi;
					cariEkstreString.EvrakSeriSira = item.cha_evrakno_seri + "-" + item.cha_evrakno_sira;
					cariEkstreString.EvrakTipi = EnumUtility.EnumToLocalizedString(item.cha_tip);
					cariEkstreString.EvrakCinsi = EnumUtility.EnumToLocalizedString(item.cha_evrak_tip);
					cariEkstreString.Meblag = item.cha_meblag;
					cariEkstreString.Bakiye = item.Bakiye;
					if (item.cha_evrakno_sira == -1)
					{
						cariEkstreString.EvrakSeriSira = "-";
						cariEkstreString.VadeTarihi = cariEkstreString.Tarih;
						cariEkstreString.EvrakTipi = AppResource.cariekstre_devir;
						cariEkstreString.EvrakCinsi = AppResource.cariekstre_devir;
					}
					list[list.Count - 1].Satirlar.Add(cariEkstreString);
				}
				cariEkstreButun.gruplar = list;
			}
			_Cevap = CariRestclient.SendCariEkstre(cariEkstreButun, servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _alicilar, _konu, _mesaj, _format);
		}
		catch (Exception ex)
		{
			_Cevap = "Hata : " + ex.ToString();
		}
		servis_kontrolu = null;
		this.OnCariEkstreMailGonderBitti(this, _Cevap);
	}
}
