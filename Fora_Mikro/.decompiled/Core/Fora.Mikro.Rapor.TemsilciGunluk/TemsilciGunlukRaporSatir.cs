using System;

namespace Fora.Mikro.Rapor.TemsilciGunluk;

public class TemsilciGunlukRaporSatir
{
	public DateTime Baslangic_tarihi { get; set; }

	public DateTime Bitis_tarihi { get; set; }

	public string Temsilci_kodu { get; set; }

	public string Temsilci_adi { get; set; }

	public string Temsilci_soyadi { get; set; }

	public double Siparis_toplami { get; set; }

	public double Proforma_siparis_toplami { get; set; }

	public double Satis_faturasi_toplami { get; set; }

	public double Nakit_tahsilat_toplami { get; set; }

	public double Cek_tahsilat_toplami { get; set; }

	public double Senet_tahsilat_toplami { get; set; }

	public double Kredi_karti_tahsilat_toplami { get; set; }

	public double Tahsilat_genel_toplami { get; set; }

	public int Siparis_adeti { get; set; }

	public int Proforma_siparis_adeti { get; set; }

	public int Satis_faturasi_adeti { get; set; }

	public int Nakit_tahsilat_adeti { get; set; }

	public int Cek_tahsilat_adeti { get; set; }

	public int Senet_tahsilat_adeti { get; set; }

	public int Kredi_karti_tahsilat_adeti { get; set; }

	public int Tahsilat_genel_adeti { get; set; }

	public TemsilciGunlukRaporSatir()
	{
		Baslangic_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		Bitis_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		Temsilci_kodu = "";
		Temsilci_adi = "";
		Temsilci_soyadi = "";
	}
}
