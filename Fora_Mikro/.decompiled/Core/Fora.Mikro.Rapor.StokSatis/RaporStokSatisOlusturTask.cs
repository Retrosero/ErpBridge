using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Rapor.StokSatis;

public class RaporStokSatisOlusturTask
{
	public delegate void RaporStokSatisOlusturTaskBittiHandler(object sender, RaporStokSatisSonuc rapor_sonuc, RaporStokSatisSonucHam rapor_sonuc_ham, RaporStokSatisSecenekleri rapor_secenekleri, string Cevap);

	private RaporStokSatisSecenekleri _rapor_secenekleri;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	private RaporStokSatisSonucHam _rapor_sonuc_ham;

	public event RaporStokSatisOlusturTaskBittiHandler OnRaporStokSatisOlusturTaskBitti;

	public void Baslat(RaporStokSatisSecenekleri rapor_secenekleri)
	{
		_rapor_secenekleri = rapor_secenekleri;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		_rapor_sonuc_ham = new RaporStokSatisSonucHam();
		try
		{
			_Cevap = "";
			bool flag = false;
			servis_kontrolu = new ServisKontrolu(270);
			if (servis_kontrolu.Sonuc == enum_servis_kontrol_sonuc.ServisCalisiyor)
			{
				try
				{
					_rapor_sonuc_ham = RaporStokSatisRestClient.GetRaporStokSatis(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("DepoNo")._GetString, AppBase.KullaniciParametreleri._GetParametre("ProjeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokAnaGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokUreticiKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokMarkaKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokReyonKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKategoriKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("RaporStokSatisMaliyetHesaplamaSekli")._GetInt);
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
					_rapor_sonuc_ham = RaporStokSatisSqlite.GetRapor(AppBase.GetDbConnectionSync(), _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("DepoNo")._GetString, AppBase.KullaniciParametreleri._GetParametre("ProjeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokAnaGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokUreticiKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokMarkaKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokReyonKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKategoriKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("RaporStokSatisMaliyetHesaplamaSekli")._GetInt);
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
		RaporStokSatisSonuc raporStokSatisSonuc = new RaporStokSatisSonuc();
		raporStokSatisSonuc.gruplandirma_secenegi = _rapor_secenekleri.gruplandirma_secenegi;
		raporStokSatisSonuc.miktar1_secenek = _rapor_secenekleri.miktar1_secenek;
		raporStokSatisSonuc.miktar2_secenek = _rapor_secenekleri.miktar2_secenek;
		raporStokSatisSonuc.miktar3_secenek = _rapor_secenekleri.miktar3_secenek;
		raporStokSatisSonuc.tutar1_secenek = _rapor_secenekleri.tutar1_secenek;
		raporStokSatisSonuc.tutar2_secenek = _rapor_secenekleri.tutar2_secenek;
		raporStokSatisSonuc.tutar3_secenek = _rapor_secenekleri.tutar3_secenek;
		raporStokSatisSonuc.siralama_secenegi = _rapor_secenekleri.siralama_secenegi;
		this.OnRaporStokSatisOlusturTaskBitti(this, raporStokSatisSonuc, _rapor_sonuc_ham, _rapor_secenekleri, _Cevap);
	}
}
