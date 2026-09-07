using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Rapor.StokEnvanter;

public class RaporStokEnvanterOlusturTask
{
	public delegate void RaporStokEnvanterOlusturTaskBittiHandler(object sender, RaporStokEnvanterSonuc rapor_sonuc, RaporStokEnvanterSonucHam rapor_sonuc_ham, RaporStokEnvanterSecenekleri rapor_secenekleri, string Cevap);

	private RaporStokEnvanterSecenekleri _rapor_secenekleri;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	private RaporStokEnvanterSonucHam _rapor_sonuc_ham;

	public event RaporStokEnvanterOlusturTaskBittiHandler OnRaporStokEnvanterOlusturTaskBitti;

	public void Baslat(RaporStokEnvanterSecenekleri rapor_secenekleri)
	{
		_rapor_secenekleri = rapor_secenekleri;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		_rapor_sonuc_ham = new RaporStokEnvanterSonucHam();
		try
		{
			_Cevap = "";
			bool flag = false;
			servis_kontrolu = new ServisKontrolu(30200);
			if (servis_kontrolu.Sonuc == enum_servis_kontrol_sonuc.ServisCalisiyor)
			{
				try
				{
					_rapor_sonuc_ham = RaporStokEnvanterRestClient.GetRaporStokEnvanter(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("DepoNo")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokAnaGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokUreticiKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokMarkaKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokReyonKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKategoriKodu")._GetString);
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
					_rapor_sonuc_ham = RaporStokEnvanterSqlite.GetRapor(AppBase.GetDbConnectionSync(), _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("DepoNo")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokAnaGrupKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokUreticiKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokMarkaKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokReyonKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("StokKategoriKodu")._GetString);
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
		RaporStokEnvanterSonuc raporStokEnvanterSonuc = new RaporStokEnvanterSonuc();
		raporStokEnvanterSonuc.gruplandirma_secenegi = _rapor_secenekleri.gruplandirma_secenegi;
		raporStokEnvanterSonuc.miktar1_secenek = _rapor_secenekleri.miktar1_secenek;
		raporStokEnvanterSonuc.miktar2_secenek = _rapor_secenekleri.miktar2_secenek;
		raporStokEnvanterSonuc.miktar3_secenek = _rapor_secenekleri.miktar3_secenek;
		raporStokEnvanterSonuc.tutar1_secenek = _rapor_secenekleri.tutar1_secenek;
		raporStokEnvanterSonuc.tutar2_secenek = _rapor_secenekleri.tutar2_secenek;
		raporStokEnvanterSonuc.tutar3_secenek = _rapor_secenekleri.tutar3_secenek;
		raporStokEnvanterSonuc.siralama_secenegi = _rapor_secenekleri.siralama_secenegi;
		this.OnRaporStokEnvanterOlusturTaskBitti(this, raporStokEnvanterSonuc, _rapor_sonuc_ham, _rapor_secenekleri, _Cevap);
	}
}
