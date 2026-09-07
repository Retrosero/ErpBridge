using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisOlusturTask
{
	public delegate void RaporStokSiparisOlusturTaskBittiHandler(object sender, RaporStokSiparisSonuc rapor_sonuc, RaporStokSiparisSonucHam rapor_sonuc_ham, RaporStokSiparisSecenekleri rapor_secenekleri, string Cevap);

	private RaporStokSiparisSecenekleri _rapor_secenekleri;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	private RaporStokSiparisSonucHam _rapor_sonuc_ham;

	public event RaporStokSiparisOlusturTaskBittiHandler OnRaporStokSiparisOlusturTaskBitti;

	public void Baslat(RaporStokSiparisSecenekleri rapor_secenekleri)
	{
		_rapor_secenekleri = rapor_secenekleri;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		_rapor_sonuc_ham = new RaporStokSiparisSonucHam();
		try
		{
			_Cevap = "";
			bool flag = false;
			servis_kontrolu = new ServisKontrolu(270);
			if (servis_kontrolu.Sonuc == enum_servis_kontrol_sonuc.ServisCalisiyor)
			{
				try
				{
					_rapor_sonuc_ham = RaporStokSiparisRestClient.GetRaporStokSiparis(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("DepoNo")._GetString, AppBase.KullaniciParametreleri._GetParametre("ProjeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokAnaGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokUreticiKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokMarkaKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokReyonKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKategoriKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariGrupKodu")._GetString);
				}
				catch (Exception ex)
				{
					_Cevap = "Hata : " + AppResource.genel_bilgiler_cekilemedi + " : " + ex.ToString();
				}
				if (_Cevap != "")
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_STOK_HAREKETLERI")._GetBoolean)
				{
					_Cevap = "";
					_Cevap = "";
					_rapor_sonuc_ham = RaporStokSiparisSqlite.GetRapor(AppBase.GetDbConnectionSync(), _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("DepoNo")._GetString, AppBase.KullaniciParametreleri._GetParametre("ProjeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokAnaGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokUreticiKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokMarkaKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokReyonKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKategoriKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariGrupKodu")._GetString);
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
		}
		catch (Exception ex2)
		{
			_Cevap = "Hata : " + ex2.ToString();
		}
		servis_kontrolu = null;
		RaporStokSiparisSonuc raporStokSiparisSonuc = new RaporStokSiparisSonuc();
		raporStokSiparisSonuc.gruplandirma_secenegi = _rapor_secenekleri.gruplandirma_secenegi;
		raporStokSiparisSonuc.miktar1_secenek = _rapor_secenekleri.miktar1_secenek;
		raporStokSiparisSonuc.miktar2_secenek = _rapor_secenekleri.miktar2_secenek;
		raporStokSiparisSonuc.miktar3_secenek = _rapor_secenekleri.miktar3_secenek;
		raporStokSiparisSonuc.tutar1_secenek = _rapor_secenekleri.tutar1_secenek;
		raporStokSiparisSonuc.tutar2_secenek = _rapor_secenekleri.tutar2_secenek;
		raporStokSiparisSonuc.tutar3_secenek = _rapor_secenekleri.tutar3_secenek;
		raporStokSiparisSonuc.siralama_secenegi = _rapor_secenekleri.siralama_secenegi;
		this.OnRaporStokSiparisOlusturTaskBitti(this, raporStokSiparisSonuc, _rapor_sonuc_ham, _rapor_secenekleri, _Cevap);
	}
}
