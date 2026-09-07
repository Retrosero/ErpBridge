using System;
using System.Collections.Generic;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.Siparis;

namespace Fora.Mikro.Rapor.TemsilciGunluk;

public class TemsilciGunlukRaporTask
{
	public delegate void TemsilciGunlukRaporBittiHandler(object sender, TemsilciGunlukRapor rapor);

	private TemsilciGunlukRapor _rapor;

	public event TemsilciGunlukRaporBittiHandler OnTemsilciGunlukRaporBitti;

	public void Baslat(TemsilciGunlukRapor rapor)
	{
		_rapor = rapor;
		Main();
	}

	public void Main()
	{
		_rapor.Satirlar = new List<TemsilciGunlukRaporSatir>();
		try
		{
			foreach (CariPersonel item in _rapor.Temsilciler)
			{
				TemsilciGunlukRaporSatir temsilciGunlukRaporSatir = new TemsilciGunlukRaporSatir();
				temsilciGunlukRaporSatir.Temsilci_kodu = item.cari_per_kod;
				temsilciGunlukRaporSatir.Temsilci_adi = item.cari_per_adi;
				temsilciGunlukRaporSatir.Temsilci_soyadi = item.cari_per_soyadi;
				temsilciGunlukRaporSatir.Siparis_adeti = 0;
				temsilciGunlukRaporSatir.Siparis_toplami = 0.0;
				foreach (double item2 in SiparislerSqlite.GetRaporSiparisMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0)))
				{
					temsilciGunlukRaporSatir.Siparis_adeti++;
					temsilciGunlukRaporSatir.Siparis_toplami += item2;
				}
				temsilciGunlukRaporSatir.Proforma_siparis_adeti = 0;
				temsilciGunlukRaporSatir.Proforma_siparis_toplami = 0.0;
				foreach (double item3 in SiparislerSqlite.GetRaporProformaSiparisMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0)))
				{
					temsilciGunlukRaporSatir.Proforma_siparis_adeti++;
					temsilciGunlukRaporSatir.Proforma_siparis_toplami += item3;
				}
				temsilciGunlukRaporSatir.Satis_faturasi_adeti = 0;
				temsilciGunlukRaporSatir.Satis_faturasi_toplami = 0.0;
				foreach (double item4 in CariHesapHareketleriSqlite.GetRaporSatisFaturasiMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0)))
				{
					temsilciGunlukRaporSatir.Satis_faturasi_adeti++;
					temsilciGunlukRaporSatir.Satis_faturasi_toplami += item4;
				}
				temsilciGunlukRaporSatir.Cek_tahsilat_adeti = 0;
				temsilciGunlukRaporSatir.Cek_tahsilat_toplami = 0.0;
				foreach (double item5 in CariHesapHareketleriSqlite.GetRaporTahsilatMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0), enum_cha_cinsi.MusteriCeki))
				{
					temsilciGunlukRaporSatir.Cek_tahsilat_adeti++;
					temsilciGunlukRaporSatir.Cek_tahsilat_toplami += item5;
				}
				temsilciGunlukRaporSatir.Kredi_karti_tahsilat_adeti = 0;
				temsilciGunlukRaporSatir.Kredi_karti_tahsilat_toplami = 0.0;
				foreach (double item6 in CariHesapHareketleriSqlite.GetRaporTahsilatMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0), enum_cha_cinsi.MusteriKrediKarti))
				{
					temsilciGunlukRaporSatir.Kredi_karti_tahsilat_adeti++;
					temsilciGunlukRaporSatir.Kredi_karti_tahsilat_toplami += item6;
				}
				temsilciGunlukRaporSatir.Nakit_tahsilat_adeti = 0;
				temsilciGunlukRaporSatir.Nakit_tahsilat_toplami = 0.0;
				foreach (double item7 in CariHesapHareketleriSqlite.GetRaporTahsilatMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0), enum_cha_cinsi.Nakit))
				{
					temsilciGunlukRaporSatir.Nakit_tahsilat_adeti++;
					temsilciGunlukRaporSatir.Nakit_tahsilat_toplami += item7;
				}
				temsilciGunlukRaporSatir.Senet_tahsilat_adeti = 0;
				temsilciGunlukRaporSatir.Senet_tahsilat_toplami = 0.0;
				foreach (double item8 in CariHesapHareketleriSqlite.GetRaporTahsilatMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0), enum_cha_cinsi.MusteriSenedi))
				{
					temsilciGunlukRaporSatir.Senet_tahsilat_adeti++;
					temsilciGunlukRaporSatir.Senet_tahsilat_toplami += item8;
				}
				temsilciGunlukRaporSatir.Tahsilat_genel_adeti = 0;
				temsilciGunlukRaporSatir.Tahsilat_genel_toplami = 0.0;
				foreach (double item9 in CariHesapHareketleriSqlite.GetRaporTahsilatToplamMeblag(AppBase.GetDbConnectionSync(), item.cari_per_kod, _rapor.Baslangic_tarihi, _rapor.Bitis_tarihi.AddDays(1.0)))
				{
					temsilciGunlukRaporSatir.Tahsilat_genel_adeti++;
					temsilciGunlukRaporSatir.Tahsilat_genel_toplami += item9;
				}
				_rapor.Satirlar.Add(temsilciGunlukRaporSatir);
				temsilciGunlukRaporSatir = null;
			}
		}
		catch (Exception)
		{
		}
		this.OnTemsilciGunlukRaporBitti(this, _rapor);
	}
}
