using System;
using Fora.Mikro.Enumler;
using Fora.Mikro.Senkronizasyon;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class RaporYapilacakTahsilatlarOlusturTask
{
	public delegate void RaporYapilacakTahsilatlarOlusturTaskBittiHandler(object sender, RaporYapilacakTahsilatlarSonuc rapor_sonuc, RaporYapilacakTahsilatlarSonucHam rapor_sonuc_ham, RaporYapilacakTahsilatlarSecenekleri rapor_secenekleri, string Cevap);

	private RaporYapilacakTahsilatlarSecenekleri _rapor_secenekleri;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	private RaporYapilacakTahsilatlarSonucHam _rapor_sonuc_ham;

	public event RaporYapilacakTahsilatlarOlusturTaskBittiHandler OnRaporYapilacakTahsilatlarOlusturTaskBitti;

	public void Baslat(RaporYapilacakTahsilatlarSecenekleri rapor_secenekleri)
	{
		_rapor_secenekleri = rapor_secenekleri;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		_rapor_sonuc_ham = new RaporYapilacakTahsilatlarSonucHam();
		try
		{
			_Cevap = "";
			bool flag = false;
			servis_kontrolu = new ServisKontrolu(272);
			if (servis_kontrolu.Sonuc == enum_servis_kontrol_sonuc.ServisCalisiyor)
			{
				RaporOlusturOnline();
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
				if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_CARI_HESAP_HAREKETLERI")._GetBoolean)
				{
					RaporOlusturOffline();
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
		catch (Exception ex)
		{
			_Cevap = "Hata : " + ex.ToString();
		}
		servis_kontrolu = null;
		RaporYapilacakTahsilatlarSonuc raporYapilacakTahsilatlarSonuc = new RaporYapilacakTahsilatlarSonuc();
		raporYapilacakTahsilatlarSonuc.gruplandirma_secenegi = _rapor_secenekleri.gruplandirma_secenegi;
		raporYapilacakTahsilatlarSonuc.siralama_secenegi = _rapor_secenekleri.siralama_secenegi;
		this.OnRaporYapilacakTahsilatlarOlusturTaskBitti(this, raporYapilacakTahsilatlarSonuc, _rapor_sonuc_ham, _rapor_secenekleri, _Cevap);
	}

	protected void RaporOlusturOffline()
	{
		_Cevap = "";
		_rapor_sonuc_ham = RaporYapilacakTahsilatlarSqlite.GetRapor(AppBase.GetDbConnectionSync(), _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("ProjeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariGrupKodu")._GetString, AppBase._doviz_cinsi_tanimlari);
	}

	protected void RaporOlusturOnline()
	{
		_rapor_sonuc_ham = new RaporYapilacakTahsilatlarSonucHam();
		try
		{
			_rapor_sonuc_ham = RaporYapilacakTahsilatlarRestClient.GetRaporYapilacakTahsilatlar(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _rapor_secenekleri, AppBase.KullaniciParametreleri._GetParametre("ProjeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, AppBase.KullaniciParametreleri._GetParametre("CariGrupKodu")._GetString);
		}
		catch (Exception ex)
		{
			_Cevap = "Hata : " + AppResource.genel_bilgiler_cekilemedi + " : " + ex.ToString();
		}
	}
}
