using System;
using System.IO;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.Temsilciler;

public class Ziyaret
{
	public enum_BaslangicBitis baslangicbitis { get; set; }

	public int zyrt_RecNo { get; set; }

	public DateTime zyrt_Create_Date { get; set; }

	public DateTime zyrt_Lastup_Date { get; set; }

	public bool zyrt_iptal { get; set; }

	public DateTime zyrt_Tarihi { get; set; }

	public string zyrt_Temsilci_Kodu { get; set; }

	public string zyrt_Bolge_Kodu { get; set; }

	public string zyrt_Cari_Kodu { get; set; }

	public int zyrt_Cari_Adres_No { get; set; }

	public double zyrt_Cari_Adres_Enlem { get; set; }

	public double zyrt_Cari_Adres_Boylam { get; set; }

	public DateTime zyrt_Baslama_Saati { get; set; }

	public double zyrt_Baslama_Enlem { get; set; }

	public double zyrt_Baslama_Boylam { get; set; }

	public DateTime zyrt_Bitis_Saati { get; set; }

	public double zyrt_Bitis_Enlem { get; set; }

	public double zyrt_Bitis_Boylam { get; set; }

	public bool zyrt_Tamamlandi { get; set; }

	public byte[] zyrt_Fotograf { get; set; }

	public int zyrt_Fotograf_Id { get; set; }

	public string zyrt_Proje_Kodu { get; set; }

	public string zyrt_Sor_Mer_Kodu { get; set; }

	public string zyrt_Bakim_Evrak_Seri { get; set; }

	public int zyrt_Bakim_Evrak_Sira { get; set; }

	public bool zyrt_Satis_Yapildi { get; set; }

	public bool zyrt_Siparis_Alindi { get; set; }

	public bool zyrt_Urun_Teslim_Edildi { get; set; }

	public bool zyrt_Tahsilat_Yapildi { get; set; }

	public bool zyrt_Katalog_Birakildi { get; set; }

	public bool zyrt_Fiyat_Listesi_Birakildi { get; set; }

	public bool zyrt_Numune_Urun_Birakildi { get; set; }

	public bool zyrt_Konsinye_Urun_Birakildi { get; set; }

	public bool zyrt_Promosyon_Birakildi { get; set; }

	public enum_Derecelendirme zyrt_Firma_Durumu { get; set; }

	public bool zyrt_Rakip_Firma_Var { get; set; }

	public enum_Derecelendirme zyrt_Rakip_Firma_Durumu { get; set; }

	public enum_Derecelendirme zyrt_Urun_Yerlesim_Durumu { get; set; }

	public enum_Derecelendirme zyrt_Rakip_Urun_Yerlesim_Durumu { get; set; }

	public string zyrt_Temsilci_Aciklama { get; set; }

	public enum_Derecelendirme zyrt_Cari_Firma_Memnuniyeti { get; set; }

	public enum_Derecelendirme zyrt_Cari_Urun_Memnuniyeti { get; set; }

	public enum_Derecelendirme zyrt_Cari_Fiyat_Memnuniyeti { get; set; }

	public enum_Derecelendirme zyrt_Cari_Temsilci_Memnuniyeti { get; set; }

	public string zyrt_Cari_Aciklama { get; set; }

	public bool zyrt_Temsilci_Ozel_01_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_01_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_01_Derece { get; set; }

	public int zyrt_Temsilci_Ozel_01_TamSayi { get; set; }

	public double zyrt_Temsilci_Ozel_01_OndalikliSayi { get; set; }

	public string zyrt_Temsilci_Ozel_01_Metin { get; set; }

	public byte[] zyrt_Temsilci_Ozel_01_Fotograf { get; set; }

	public int zyrt_Temsilci_Ozel_01_Fotograf_Id { get; set; }

	public bool zyrt_Temsilci_Ozel_02_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_02_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_02_Derece { get; set; }

	public int zyrt_Temsilci_Ozel_02_TamSayi { get; set; }

	public double zyrt_Temsilci_Ozel_02_OndalikliSayi { get; set; }

	public string zyrt_Temsilci_Ozel_02_Metin { get; set; }

	public byte[] zyrt_Temsilci_Ozel_02_Fotograf { get; set; }

	public int zyrt_Temsilci_Ozel_02_Fotograf_Id { get; set; }

	public bool zyrt_Temsilci_Ozel_03_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_03_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_03_Derece { get; set; }

	public int zyrt_Temsilci_Ozel_03_TamSayi { get; set; }

	public double zyrt_Temsilci_Ozel_03_OndalikliSayi { get; set; }

	public string zyrt_Temsilci_Ozel_03_Metin { get; set; }

	public byte[] zyrt_Temsilci_Ozel_03_Fotograf { get; set; }

	public int zyrt_Temsilci_Ozel_03_Fotograf_Id { get; set; }

	public bool zyrt_Temsilci_Ozel_04_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_04_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_04_Derece { get; set; }

	public int zyrt_Temsilci_Ozel_04_TamSayi { get; set; }

	public double zyrt_Temsilci_Ozel_04_OndalikliSayi { get; set; }

	public string zyrt_Temsilci_Ozel_04_Metin { get; set; }

	public byte[] zyrt_Temsilci_Ozel_04_Fotograf { get; set; }

	public int zyrt_Temsilci_Ozel_04_Fotograf_Id { get; set; }

	public bool zyrt_Temsilci_Ozel_05_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_05_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_05_Derece { get; set; }

	public int zyrt_Temsilci_Ozel_05_TamSayi { get; set; }

	public double zyrt_Temsilci_Ozel_05_OndalikliSayi { get; set; }

	public string zyrt_Temsilci_Ozel_05_Metin { get; set; }

	public byte[] zyrt_Temsilci_Ozel_05_Fotograf { get; set; }

	public int zyrt_Temsilci_Ozel_05_Fotograf_Id { get; set; }

	public bool zyrt_Temsilci_Ozel_06_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_06_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_06_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_06_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_07_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_07_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_07_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_07_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_08_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_08_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_08_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_08_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_09_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_09_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_09_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_09_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_10_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_10_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_10_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_10_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_11_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_11_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_11_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_11_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_12_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_12_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_12_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_12_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_13_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_13_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_13_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_13_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_14_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_14_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_14_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_14_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_15_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_15_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_15_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_15_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_16_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_16_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_16_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_16_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_17_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_17_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_17_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_17_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_18_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_18_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_18_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_18_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_19_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_19_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_19_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_19_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_20_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_20_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_20_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_20_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_21_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_21_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_21_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_21_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_22_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_22_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_22_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_22_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_23_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_23_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_23_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_23_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_24_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_24_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_24_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_24_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_25_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_25_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_25_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_25_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_26_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_26_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_26_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_26_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_27_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_27_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_27_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_27_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_28_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_28_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_28_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_28_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_29_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_29_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_29_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_29_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_30_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_30_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_30_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_30_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_31_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_31_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_31_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_31_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_32_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_32_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_32_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_32_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_33_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_33_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_33_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_33_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_34_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_34_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_34_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_34_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_35_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_35_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_35_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_35_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_36_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_36_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_36_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_36_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_37_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_37_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_37_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_37_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_38_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_38_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_38_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_38_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_39_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_39_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_39_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_39_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_40_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_40_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_40_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_40_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_41_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_41_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_41_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_41_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_42_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_42_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_42_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_42_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_43_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_43_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_43_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_43_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_44_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_44_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_44_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_44_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_45_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_45_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_45_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_45_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_46_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_46_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_46_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_46_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_47_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_47_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_47_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_47_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_48_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_48_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_48_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_48_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_49_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_49_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_49_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_49_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_50_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_50_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_50_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_50_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_51_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_51_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_51_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_51_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_52_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_52_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_52_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_52_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_53_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_53_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_53_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_53_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_54_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_54_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_54_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_54_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_55_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_55_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_55_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_55_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_56_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_56_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_56_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_56_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_57_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_57_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_57_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_57_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_58_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_58_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_58_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_58_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_59_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_59_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_59_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_59_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_60_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_60_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_60_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_60_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_61_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_61_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_61_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_61_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_62_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_62_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_62_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_62_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_63_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_63_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_63_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_63_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_64_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_64_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_64_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_64_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_65_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_65_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_65_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_65_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_66_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_66_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_66_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_66_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_67_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_67_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_67_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_67_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_68_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_68_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_68_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_68_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_69_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_69_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_69_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_69_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_70_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_70_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_70_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_70_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_71_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_71_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_71_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_71_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_72_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_72_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_72_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_72_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_73_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_73_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_73_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_73_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_74_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_74_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_74_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_74_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_75_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_75_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_75_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_75_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_76_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_76_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_76_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_76_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_77_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_77_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_77_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_77_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_78_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_78_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_78_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_78_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_79_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_79_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_79_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_79_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_80_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_80_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_80_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_80_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_81_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_81_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_81_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_81_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_82_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_82_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_82_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_82_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_83_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_83_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_83_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_83_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_84_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_84_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_84_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_84_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_85_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_85_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_85_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_85_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_86_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_86_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_86_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_86_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_87_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_87_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_87_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_87_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_88_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_88_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_88_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_88_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_89_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_89_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_89_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_89_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_90_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_90_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_90_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_90_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_91_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_91_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_91_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_91_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_92_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_92_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_92_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_92_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_93_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_93_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_93_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_93_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_94_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_94_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_94_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_94_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_95_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_95_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_95_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_95_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_96_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_96_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_96_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_96_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_97_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_97_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_97_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_97_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_98_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_98_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_98_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_98_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_99_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_99_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_99_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_99_Metin { get; set; }

	public bool zyrt_Temsilci_Ozel_100_Var_Yok { get; set; }

	public bool zyrt_Temsilci_Ozel_100_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Temsilci_Ozel_100_Derece { get; set; }

	public string zyrt_Temsilci_Ozel_100_Metin { get; set; }

	public bool zyrt_Cari_Ozel_01_Var_Yok { get; set; }

	public bool zyrt_Cari_Ozel_01_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Cari_Ozel_01_Derece { get; set; }

	public int zyrt_Cari_Ozel_01_TamSayi { get; set; }

	public double zyrt_Cari_Ozel_01_OndalikliSayi { get; set; }

	public string zyrt_Cari_Ozel_01_Metin { get; set; }

	public byte[] zyrt_Cari_Ozel_01_Fotograf { get; set; }

	public int zyrt_Cari_Ozel_01_Fotograf_Id { get; set; }

	public bool zyrt_Cari_Ozel_02_Var_Yok { get; set; }

	public bool zyrt_Cari_Ozel_02_Evet_Hayir { get; set; }

	public enum_Derecelendirme zyrt_Cari_Ozel_02_Derece { get; set; }

	public int zyrt_Cari_Ozel_02_TamSayi { get; set; }

	public double zyrt_Cari_Ozel_02_OndalikliSayi { get; set; }

	public string zyrt_Cari_Ozel_02_Metin { get; set; }

	public byte[] zyrt_Cari_Ozel_02_Fotograf { get; set; }

	public int zyrt_Cari_Ozel_02_Fotograf_Id { get; set; }

	public Ziyaret()
	{
		zyrt_RecNo = 0;
		zyrt_Create_Date = DateTime.Now;
		zyrt_Lastup_Date = DateTime.Now;
		zyrt_iptal = false;
		zyrt_Tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		zyrt_Temsilci_Kodu = "";
		zyrt_Bolge_Kodu = "";
		zyrt_Cari_Kodu = "";
		zyrt_Cari_Adres_No = 0;
		zyrt_Cari_Adres_Enlem = 0.0;
		zyrt_Cari_Adres_Boylam = 0.0;
		zyrt_Baslama_Saati = DateTime.Now;
		zyrt_Baslama_Enlem = 0.0;
		zyrt_Baslama_Boylam = 0.0;
		zyrt_Bitis_Saati = DateTime.Now;
		zyrt_Bitis_Enlem = 0.0;
		zyrt_Bitis_Boylam = 0.0;
		zyrt_Tamamlandi = false;
		zyrt_Fotograf = new byte[0];
		zyrt_Fotograf_Id = 0;
		zyrt_Proje_Kodu = "";
		zyrt_Sor_Mer_Kodu = "";
		zyrt_Bakim_Evrak_Seri = "";
		zyrt_Bakim_Evrak_Sira = 0;
		zyrt_Satis_Yapildi = false;
		zyrt_Siparis_Alindi = false;
		zyrt_Urun_Teslim_Edildi = false;
		zyrt_Tahsilat_Yapildi = false;
		zyrt_Katalog_Birakildi = false;
		zyrt_Fiyat_Listesi_Birakildi = false;
		zyrt_Numune_Urun_Birakildi = false;
		zyrt_Konsinye_Urun_Birakildi = false;
		zyrt_Promosyon_Birakildi = false;
		zyrt_Firma_Durumu = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Rakip_Firma_Var = false;
		zyrt_Rakip_Firma_Durumu = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Urun_Yerlesim_Durumu = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Rakip_Urun_Yerlesim_Durumu = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Aciklama = "";
		zyrt_Cari_Firma_Memnuniyeti = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Cari_Urun_Memnuniyeti = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Cari_Fiyat_Memnuniyeti = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Cari_Temsilci_Memnuniyeti = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Cari_Aciklama = "";
		zyrt_Temsilci_Ozel_01_Var_Yok = false;
		zyrt_Temsilci_Ozel_01_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_01_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_01_TamSayi = 0;
		zyrt_Temsilci_Ozel_01_OndalikliSayi = 0.0;
		zyrt_Temsilci_Ozel_01_Metin = "";
		zyrt_Temsilci_Ozel_01_Fotograf = new byte[0];
		zyrt_Temsilci_Ozel_01_Fotograf_Id = 0;
		zyrt_Temsilci_Ozel_02_Var_Yok = false;
		zyrt_Temsilci_Ozel_02_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_02_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_02_TamSayi = 0;
		zyrt_Temsilci_Ozel_02_OndalikliSayi = 0.0;
		zyrt_Temsilci_Ozel_02_Metin = "";
		zyrt_Temsilci_Ozel_02_Fotograf = new byte[0];
		zyrt_Temsilci_Ozel_02_Fotograf_Id = 0;
		zyrt_Temsilci_Ozel_03_Var_Yok = false;
		zyrt_Temsilci_Ozel_03_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_03_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_03_TamSayi = 0;
		zyrt_Temsilci_Ozel_03_OndalikliSayi = 0.0;
		zyrt_Temsilci_Ozel_03_Metin = "";
		zyrt_Temsilci_Ozel_03_Fotograf = new byte[0];
		zyrt_Temsilci_Ozel_03_Fotograf_Id = 0;
		zyrt_Temsilci_Ozel_04_Var_Yok = false;
		zyrt_Temsilci_Ozel_04_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_04_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_04_TamSayi = 0;
		zyrt_Temsilci_Ozel_04_OndalikliSayi = 0.0;
		zyrt_Temsilci_Ozel_04_Metin = "";
		zyrt_Temsilci_Ozel_04_Fotograf = new byte[0];
		zyrt_Temsilci_Ozel_04_Fotograf_Id = 0;
		zyrt_Temsilci_Ozel_05_Var_Yok = false;
		zyrt_Temsilci_Ozel_05_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_05_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_05_TamSayi = 0;
		zyrt_Temsilci_Ozel_05_OndalikliSayi = 0.0;
		zyrt_Temsilci_Ozel_05_Metin = "";
		zyrt_Temsilci_Ozel_05_Fotograf = new byte[0];
		zyrt_Temsilci_Ozel_05_Fotograf_Id = 0;
		zyrt_Temsilci_Ozel_06_Var_Yok = false;
		zyrt_Temsilci_Ozel_06_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_06_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_06_Metin = "";
		zyrt_Temsilci_Ozel_07_Var_Yok = false;
		zyrt_Temsilci_Ozel_07_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_07_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_07_Metin = "";
		zyrt_Temsilci_Ozel_08_Var_Yok = false;
		zyrt_Temsilci_Ozel_08_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_08_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_08_Metin = "";
		zyrt_Temsilci_Ozel_09_Var_Yok = false;
		zyrt_Temsilci_Ozel_09_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_09_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_09_Metin = "";
		zyrt_Temsilci_Ozel_10_Var_Yok = false;
		zyrt_Temsilci_Ozel_10_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_10_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_10_Metin = "";
		zyrt_Temsilci_Ozel_11_Var_Yok = false;
		zyrt_Temsilci_Ozel_11_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_11_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_11_Metin = "";
		zyrt_Temsilci_Ozel_12_Var_Yok = false;
		zyrt_Temsilci_Ozel_12_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_12_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_12_Metin = "";
		zyrt_Temsilci_Ozel_13_Var_Yok = false;
		zyrt_Temsilci_Ozel_13_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_13_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_13_Metin = "";
		zyrt_Temsilci_Ozel_14_Var_Yok = false;
		zyrt_Temsilci_Ozel_14_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_14_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_14_Metin = "";
		zyrt_Temsilci_Ozel_15_Var_Yok = false;
		zyrt_Temsilci_Ozel_15_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_15_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_15_Metin = "";
		zyrt_Temsilci_Ozel_16_Var_Yok = false;
		zyrt_Temsilci_Ozel_16_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_16_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_16_Metin = "";
		zyrt_Temsilci_Ozel_17_Var_Yok = false;
		zyrt_Temsilci_Ozel_17_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_17_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_17_Metin = "";
		zyrt_Temsilci_Ozel_18_Var_Yok = false;
		zyrt_Temsilci_Ozel_18_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_18_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_18_Metin = "";
		zyrt_Temsilci_Ozel_19_Var_Yok = false;
		zyrt_Temsilci_Ozel_19_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_19_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_19_Metin = "";
		zyrt_Temsilci_Ozel_20_Var_Yok = false;
		zyrt_Temsilci_Ozel_20_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_20_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_20_Metin = "";
		zyrt_Temsilci_Ozel_21_Var_Yok = false;
		zyrt_Temsilci_Ozel_21_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_21_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_21_Metin = "";
		zyrt_Temsilci_Ozel_22_Var_Yok = false;
		zyrt_Temsilci_Ozel_22_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_22_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_22_Metin = "";
		zyrt_Temsilci_Ozel_23_Var_Yok = false;
		zyrt_Temsilci_Ozel_23_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_23_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_23_Metin = "";
		zyrt_Temsilci_Ozel_24_Var_Yok = false;
		zyrt_Temsilci_Ozel_24_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_24_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_24_Metin = "";
		zyrt_Temsilci_Ozel_25_Var_Yok = false;
		zyrt_Temsilci_Ozel_25_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_25_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_25_Metin = "";
		zyrt_Temsilci_Ozel_26_Var_Yok = false;
		zyrt_Temsilci_Ozel_26_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_26_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_26_Metin = "";
		zyrt_Temsilci_Ozel_27_Var_Yok = false;
		zyrt_Temsilci_Ozel_27_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_27_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_27_Metin = "";
		zyrt_Temsilci_Ozel_28_Var_Yok = false;
		zyrt_Temsilci_Ozel_28_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_28_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_28_Metin = "";
		zyrt_Temsilci_Ozel_29_Var_Yok = false;
		zyrt_Temsilci_Ozel_29_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_29_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_29_Metin = "";
		zyrt_Temsilci_Ozel_30_Var_Yok = false;
		zyrt_Temsilci_Ozel_30_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_30_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_30_Metin = "";
		zyrt_Temsilci_Ozel_31_Var_Yok = false;
		zyrt_Temsilci_Ozel_31_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_31_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_31_Metin = "";
		zyrt_Temsilci_Ozel_32_Var_Yok = false;
		zyrt_Temsilci_Ozel_32_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_32_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_32_Metin = "";
		zyrt_Temsilci_Ozel_33_Var_Yok = false;
		zyrt_Temsilci_Ozel_33_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_33_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_33_Metin = "";
		zyrt_Temsilci_Ozel_34_Var_Yok = false;
		zyrt_Temsilci_Ozel_34_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_34_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_34_Metin = "";
		zyrt_Temsilci_Ozel_35_Var_Yok = false;
		zyrt_Temsilci_Ozel_35_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_35_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_35_Metin = "";
		zyrt_Temsilci_Ozel_36_Var_Yok = false;
		zyrt_Temsilci_Ozel_36_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_36_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_36_Metin = "";
		zyrt_Temsilci_Ozel_37_Var_Yok = false;
		zyrt_Temsilci_Ozel_37_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_37_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_37_Metin = "";
		zyrt_Temsilci_Ozel_38_Var_Yok = false;
		zyrt_Temsilci_Ozel_38_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_38_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_38_Metin = "";
		zyrt_Temsilci_Ozel_39_Var_Yok = false;
		zyrt_Temsilci_Ozel_39_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_39_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_39_Metin = "";
		zyrt_Temsilci_Ozel_40_Var_Yok = false;
		zyrt_Temsilci_Ozel_40_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_40_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_40_Metin = "";
		zyrt_Temsilci_Ozel_41_Var_Yok = false;
		zyrt_Temsilci_Ozel_41_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_41_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_41_Metin = "";
		zyrt_Temsilci_Ozel_42_Var_Yok = false;
		zyrt_Temsilci_Ozel_42_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_42_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_42_Metin = "";
		zyrt_Temsilci_Ozel_43_Var_Yok = false;
		zyrt_Temsilci_Ozel_43_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_43_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_43_Metin = "";
		zyrt_Temsilci_Ozel_44_Var_Yok = false;
		zyrt_Temsilci_Ozel_44_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_44_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_44_Metin = "";
		zyrt_Temsilci_Ozel_45_Var_Yok = false;
		zyrt_Temsilci_Ozel_45_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_45_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_45_Metin = "";
		zyrt_Temsilci_Ozel_46_Var_Yok = false;
		zyrt_Temsilci_Ozel_46_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_46_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_46_Metin = "";
		zyrt_Temsilci_Ozel_47_Var_Yok = false;
		zyrt_Temsilci_Ozel_47_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_47_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_47_Metin = "";
		zyrt_Temsilci_Ozel_48_Var_Yok = false;
		zyrt_Temsilci_Ozel_48_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_48_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_48_Metin = "";
		zyrt_Temsilci_Ozel_49_Var_Yok = false;
		zyrt_Temsilci_Ozel_49_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_49_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_49_Metin = "";
		zyrt_Temsilci_Ozel_50_Var_Yok = false;
		zyrt_Temsilci_Ozel_50_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_50_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_50_Metin = "";
		zyrt_Temsilci_Ozel_51_Var_Yok = false;
		zyrt_Temsilci_Ozel_51_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_51_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_51_Metin = "";
		zyrt_Temsilci_Ozel_52_Var_Yok = false;
		zyrt_Temsilci_Ozel_52_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_52_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_52_Metin = "";
		zyrt_Temsilci_Ozel_53_Var_Yok = false;
		zyrt_Temsilci_Ozel_53_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_53_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_53_Metin = "";
		zyrt_Temsilci_Ozel_54_Var_Yok = false;
		zyrt_Temsilci_Ozel_54_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_54_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_54_Metin = "";
		zyrt_Temsilci_Ozel_55_Var_Yok = false;
		zyrt_Temsilci_Ozel_55_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_55_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_55_Metin = "";
		zyrt_Temsilci_Ozel_56_Var_Yok = false;
		zyrt_Temsilci_Ozel_56_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_56_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_56_Metin = "";
		zyrt_Temsilci_Ozel_57_Var_Yok = false;
		zyrt_Temsilci_Ozel_57_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_57_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_57_Metin = "";
		zyrt_Temsilci_Ozel_58_Var_Yok = false;
		zyrt_Temsilci_Ozel_58_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_58_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_58_Metin = "";
		zyrt_Temsilci_Ozel_59_Var_Yok = false;
		zyrt_Temsilci_Ozel_59_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_59_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_59_Metin = "";
		zyrt_Temsilci_Ozel_60_Var_Yok = false;
		zyrt_Temsilci_Ozel_60_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_60_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_60_Metin = "";
		zyrt_Temsilci_Ozel_61_Var_Yok = false;
		zyrt_Temsilci_Ozel_61_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_61_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_61_Metin = "";
		zyrt_Temsilci_Ozel_62_Var_Yok = false;
		zyrt_Temsilci_Ozel_62_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_62_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_62_Metin = "";
		zyrt_Temsilci_Ozel_63_Var_Yok = false;
		zyrt_Temsilci_Ozel_63_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_63_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_63_Metin = "";
		zyrt_Temsilci_Ozel_64_Var_Yok = false;
		zyrt_Temsilci_Ozel_64_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_64_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_64_Metin = "";
		zyrt_Temsilci_Ozel_65_Var_Yok = false;
		zyrt_Temsilci_Ozel_65_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_65_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_65_Metin = "";
		zyrt_Temsilci_Ozel_66_Var_Yok = false;
		zyrt_Temsilci_Ozel_66_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_66_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_66_Metin = "";
		zyrt_Temsilci_Ozel_67_Var_Yok = false;
		zyrt_Temsilci_Ozel_67_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_67_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_67_Metin = "";
		zyrt_Temsilci_Ozel_68_Var_Yok = false;
		zyrt_Temsilci_Ozel_68_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_68_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_68_Metin = "";
		zyrt_Temsilci_Ozel_69_Var_Yok = false;
		zyrt_Temsilci_Ozel_69_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_69_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_69_Metin = "";
		zyrt_Temsilci_Ozel_70_Var_Yok = false;
		zyrt_Temsilci_Ozel_70_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_70_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_70_Metin = "";
		zyrt_Temsilci_Ozel_71_Var_Yok = false;
		zyrt_Temsilci_Ozel_71_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_71_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_71_Metin = "";
		zyrt_Temsilci_Ozel_72_Var_Yok = false;
		zyrt_Temsilci_Ozel_72_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_72_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_72_Metin = "";
		zyrt_Temsilci_Ozel_73_Var_Yok = false;
		zyrt_Temsilci_Ozel_73_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_73_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_73_Metin = "";
		zyrt_Temsilci_Ozel_74_Var_Yok = false;
		zyrt_Temsilci_Ozel_74_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_74_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_74_Metin = "";
		zyrt_Temsilci_Ozel_75_Var_Yok = false;
		zyrt_Temsilci_Ozel_75_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_75_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_75_Metin = "";
		zyrt_Temsilci_Ozel_76_Var_Yok = false;
		zyrt_Temsilci_Ozel_76_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_76_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_76_Metin = "";
		zyrt_Temsilci_Ozel_77_Var_Yok = false;
		zyrt_Temsilci_Ozel_77_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_77_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_77_Metin = "";
		zyrt_Temsilci_Ozel_78_Var_Yok = false;
		zyrt_Temsilci_Ozel_78_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_78_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_78_Metin = "";
		zyrt_Temsilci_Ozel_79_Var_Yok = false;
		zyrt_Temsilci_Ozel_79_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_79_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_79_Metin = "";
		zyrt_Temsilci_Ozel_80_Var_Yok = false;
		zyrt_Temsilci_Ozel_80_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_80_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_80_Metin = "";
		zyrt_Temsilci_Ozel_81_Var_Yok = false;
		zyrt_Temsilci_Ozel_81_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_81_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_81_Metin = "";
		zyrt_Temsilci_Ozel_82_Var_Yok = false;
		zyrt_Temsilci_Ozel_82_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_82_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_82_Metin = "";
		zyrt_Temsilci_Ozel_83_Var_Yok = false;
		zyrt_Temsilci_Ozel_83_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_83_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_83_Metin = "";
		zyrt_Temsilci_Ozel_84_Var_Yok = false;
		zyrt_Temsilci_Ozel_84_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_84_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_84_Metin = "";
		zyrt_Temsilci_Ozel_85_Var_Yok = false;
		zyrt_Temsilci_Ozel_85_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_85_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_85_Metin = "";
		zyrt_Temsilci_Ozel_86_Var_Yok = false;
		zyrt_Temsilci_Ozel_86_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_86_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_86_Metin = "";
		zyrt_Temsilci_Ozel_87_Var_Yok = false;
		zyrt_Temsilci_Ozel_87_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_87_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_87_Metin = "";
		zyrt_Temsilci_Ozel_88_Var_Yok = false;
		zyrt_Temsilci_Ozel_88_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_88_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_88_Metin = "";
		zyrt_Temsilci_Ozel_89_Var_Yok = false;
		zyrt_Temsilci_Ozel_89_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_89_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_89_Metin = "";
		zyrt_Temsilci_Ozel_90_Var_Yok = false;
		zyrt_Temsilci_Ozel_90_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_90_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_90_Metin = "";
		zyrt_Temsilci_Ozel_91_Var_Yok = false;
		zyrt_Temsilci_Ozel_91_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_91_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_91_Metin = "";
		zyrt_Temsilci_Ozel_92_Var_Yok = false;
		zyrt_Temsilci_Ozel_92_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_92_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_92_Metin = "";
		zyrt_Temsilci_Ozel_93_Var_Yok = false;
		zyrt_Temsilci_Ozel_93_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_93_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_93_Metin = "";
		zyrt_Temsilci_Ozel_94_Var_Yok = false;
		zyrt_Temsilci_Ozel_94_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_94_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_94_Metin = "";
		zyrt_Temsilci_Ozel_95_Var_Yok = false;
		zyrt_Temsilci_Ozel_95_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_95_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_95_Metin = "";
		zyrt_Temsilci_Ozel_96_Var_Yok = false;
		zyrt_Temsilci_Ozel_96_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_96_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_96_Metin = "";
		zyrt_Temsilci_Ozel_97_Var_Yok = false;
		zyrt_Temsilci_Ozel_97_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_97_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_97_Metin = "";
		zyrt_Temsilci_Ozel_98_Var_Yok = false;
		zyrt_Temsilci_Ozel_98_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_98_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_98_Metin = "";
		zyrt_Temsilci_Ozel_99_Var_Yok = false;
		zyrt_Temsilci_Ozel_99_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_99_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_99_Metin = "";
		zyrt_Temsilci_Ozel_100_Var_Yok = false;
		zyrt_Temsilci_Ozel_100_Evet_Hayir = false;
		zyrt_Temsilci_Ozel_100_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Temsilci_Ozel_100_Metin = "";
		zyrt_Cari_Ozel_01_Var_Yok = false;
		zyrt_Cari_Ozel_01_Evet_Hayir = false;
		zyrt_Cari_Ozel_01_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Cari_Ozel_01_TamSayi = 0;
		zyrt_Cari_Ozel_01_OndalikliSayi = 0.0;
		zyrt_Cari_Ozel_01_Metin = "";
		zyrt_Cari_Ozel_01_Fotograf = new byte[0];
		zyrt_Cari_Ozel_01_Fotograf_Id = 0;
		zyrt_Cari_Ozel_02_Var_Yok = false;
		zyrt_Cari_Ozel_02_Evet_Hayir = false;
		zyrt_Cari_Ozel_02_Derece = enum_Derecelendirme.Derecelendirilmemis;
		zyrt_Cari_Ozel_02_TamSayi = 0;
		zyrt_Cari_Ozel_02_OndalikliSayi = 0.0;
		zyrt_Cari_Ozel_02_Metin = "";
		zyrt_Cari_Ozel_02_Fotograf = new byte[0];
		zyrt_Cari_Ozel_02_Fotograf_Id = 0;
	}

	public static byte[] WriteToByteArray(Ziyaret toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Ziyaret toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Ziyaret toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.baslangicbitis);
		writer.Write(toWrite.zyrt_RecNo);
		writer.Write(toWrite.zyrt_Create_Date.Ticks);
		writer.Write(toWrite.zyrt_Lastup_Date.Ticks);
		writer.Write(toWrite.zyrt_iptal);
		writer.Write(toWrite.zyrt_Tarihi.Ticks);
		writer.Write(toWrite.zyrt_Temsilci_Kodu);
		writer.Write(toWrite.zyrt_Bolge_Kodu);
		writer.Write(toWrite.zyrt_Cari_Kodu);
		writer.Write(toWrite.zyrt_Cari_Adres_No);
		writer.Write(toWrite.zyrt_Cari_Adres_Enlem);
		writer.Write(toWrite.zyrt_Cari_Adres_Boylam);
		writer.Write(toWrite.zyrt_Baslama_Saati.Ticks);
		writer.Write(toWrite.zyrt_Baslama_Enlem);
		writer.Write(toWrite.zyrt_Baslama_Boylam);
		writer.Write(toWrite.zyrt_Bitis_Saati.Ticks);
		writer.Write(toWrite.zyrt_Bitis_Enlem);
		writer.Write(toWrite.zyrt_Bitis_Boylam);
		writer.Write(toWrite.zyrt_Tamamlandi);
		writer.Write(toWrite.zyrt_Fotograf.Length);
		writer.Write(toWrite.zyrt_Fotograf);
		writer.Write(toWrite.zyrt_Fotograf_Id);
		writer.Write(toWrite.zyrt_Proje_Kodu);
		writer.Write(toWrite.zyrt_Sor_Mer_Kodu);
		writer.Write(toWrite.zyrt_Bakim_Evrak_Seri);
		writer.Write(toWrite.zyrt_Bakim_Evrak_Sira);
		writer.Write(toWrite.zyrt_Satis_Yapildi);
		writer.Write(toWrite.zyrt_Siparis_Alindi);
		writer.Write(toWrite.zyrt_Urun_Teslim_Edildi);
		writer.Write(toWrite.zyrt_Tahsilat_Yapildi);
		writer.Write(toWrite.zyrt_Katalog_Birakildi);
		writer.Write(toWrite.zyrt_Fiyat_Listesi_Birakildi);
		writer.Write(toWrite.zyrt_Numune_Urun_Birakildi);
		writer.Write(toWrite.zyrt_Konsinye_Urun_Birakildi);
		writer.Write(toWrite.zyrt_Promosyon_Birakildi);
		writer.Write((int)toWrite.zyrt_Firma_Durumu);
		writer.Write(toWrite.zyrt_Rakip_Firma_Var);
		writer.Write((int)toWrite.zyrt_Rakip_Firma_Durumu);
		writer.Write((int)toWrite.zyrt_Urun_Yerlesim_Durumu);
		writer.Write((int)toWrite.zyrt_Rakip_Urun_Yerlesim_Durumu);
		writer.Write(toWrite.zyrt_Temsilci_Aciklama);
		writer.Write((int)toWrite.zyrt_Cari_Firma_Memnuniyeti);
		writer.Write((int)toWrite.zyrt_Cari_Urun_Memnuniyeti);
		writer.Write((int)toWrite.zyrt_Cari_Fiyat_Memnuniyeti);
		writer.Write((int)toWrite.zyrt_Cari_Temsilci_Memnuniyeti);
		writer.Write(toWrite.zyrt_Cari_Aciklama);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_Var_Yok);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_Evet_Hayir);
		writer.Write((int)toWrite.zyrt_Temsilci_Ozel_01_Derece);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_TamSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_OndalikliSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_Metin);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_Fotograf.Length);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_Fotograf);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_01_Fotograf_Id);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_Var_Yok);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_Evet_Hayir);
		writer.Write((int)toWrite.zyrt_Temsilci_Ozel_02_Derece);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_TamSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_OndalikliSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_Metin);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_Fotograf.Length);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_Fotograf);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_02_Fotograf_Id);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_Var_Yok);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_Evet_Hayir);
		writer.Write((int)toWrite.zyrt_Temsilci_Ozel_03_Derece);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_TamSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_OndalikliSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_Metin);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_Fotograf.Length);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_Fotograf);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_03_Fotograf_Id);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_Var_Yok);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_Evet_Hayir);
		writer.Write((int)toWrite.zyrt_Temsilci_Ozel_04_Derece);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_TamSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_OndalikliSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_Metin);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_Fotograf.Length);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_Fotograf);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_04_Fotograf_Id);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_Var_Yok);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_Evet_Hayir);
		writer.Write((int)toWrite.zyrt_Temsilci_Ozel_05_Derece);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_TamSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_OndalikliSayi);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_Metin);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_Fotograf.Length);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_Fotograf);
		writer.Write(toWrite.zyrt_Temsilci_Ozel_05_Fotograf_Id);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_Var_Yok);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_Evet_Hayir);
		writer.Write((int)toWrite.zyrt_Cari_Ozel_01_Derece);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_TamSayi);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_OndalikliSayi);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_Metin);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_Fotograf.Length);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_Fotograf);
		writer.Write(toWrite.zyrt_Cari_Ozel_01_Fotograf_Id);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_Var_Yok);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_Evet_Hayir);
		writer.Write((int)toWrite.zyrt_Cari_Ozel_02_Derece);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_TamSayi);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_OndalikliSayi);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_Metin);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_Fotograf.Length);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_Fotograf);
		writer.Write(toWrite.zyrt_Cari_Ozel_02_Fotograf_Id);
		if (versiyon >= 2)
		{
			writer.Write(toWrite.zyrt_Temsilci_Ozel_06_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_06_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_06_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_06_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_07_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_07_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_07_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_07_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_08_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_08_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_08_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_08_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_09_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_09_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_09_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_09_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_10_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_10_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_10_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_10_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_11_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_11_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_11_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_11_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_12_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_12_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_12_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_12_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_13_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_13_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_13_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_13_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_14_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_14_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_14_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_14_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_15_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_15_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_15_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_15_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_16_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_16_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_16_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_16_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_17_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_17_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_17_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_17_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_18_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_18_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_18_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_18_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_19_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_19_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_19_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_19_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_20_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_20_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_20_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_20_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_21_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_21_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_21_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_21_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_22_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_22_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_22_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_22_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_23_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_23_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_23_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_23_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_24_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_24_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_24_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_24_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_25_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_25_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_25_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_25_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_26_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_26_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_26_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_26_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_27_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_27_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_27_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_27_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_28_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_28_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_28_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_28_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_29_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_29_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_29_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_29_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_30_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_30_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_30_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_30_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_31_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_31_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_31_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_31_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_32_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_32_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_32_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_32_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_33_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_33_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_33_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_33_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_34_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_34_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_34_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_34_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_35_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_35_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_35_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_35_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_36_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_36_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_36_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_36_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_37_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_37_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_37_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_37_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_38_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_38_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_38_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_38_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_39_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_39_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_39_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_39_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_40_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_40_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_40_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_40_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_41_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_41_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_41_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_41_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_42_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_42_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_42_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_42_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_43_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_43_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_43_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_43_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_44_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_44_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_44_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_44_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_45_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_45_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_45_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_45_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_46_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_46_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_46_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_46_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_47_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_47_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_47_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_47_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_48_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_48_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_48_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_48_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_49_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_49_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_49_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_49_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_50_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_50_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_50_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_50_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_51_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_51_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_51_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_51_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_52_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_52_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_52_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_52_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_53_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_53_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_53_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_53_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_54_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_54_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_54_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_54_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_55_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_55_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_55_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_55_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_56_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_56_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_56_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_56_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_57_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_57_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_57_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_57_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_58_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_58_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_58_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_58_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_59_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_59_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_59_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_59_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_60_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_60_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_60_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_60_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_61_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_61_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_61_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_61_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_62_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_62_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_62_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_62_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_63_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_63_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_63_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_63_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_64_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_64_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_64_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_64_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_65_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_65_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_65_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_65_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_66_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_66_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_66_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_66_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_67_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_67_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_67_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_67_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_68_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_68_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_68_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_68_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_69_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_69_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_69_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_69_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_70_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_70_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_70_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_70_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_71_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_71_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_71_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_71_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_72_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_72_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_72_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_72_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_73_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_73_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_73_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_73_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_74_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_74_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_74_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_74_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_75_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_75_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_75_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_75_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_76_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_76_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_76_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_76_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_77_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_77_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_77_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_77_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_78_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_78_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_78_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_78_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_79_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_79_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_79_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_79_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_80_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_80_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_80_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_80_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_81_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_81_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_81_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_81_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_82_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_82_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_82_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_82_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_83_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_83_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_83_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_83_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_84_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_84_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_84_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_84_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_85_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_85_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_85_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_85_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_86_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_86_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_86_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_86_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_87_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_87_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_87_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_87_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_88_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_88_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_88_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_88_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_89_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_89_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_89_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_89_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_90_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_90_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_90_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_90_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_91_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_91_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_91_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_91_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_92_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_92_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_92_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_92_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_93_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_93_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_93_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_93_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_94_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_94_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_94_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_94_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_95_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_95_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_95_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_95_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_96_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_96_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_96_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_96_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_97_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_97_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_97_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_97_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_98_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_98_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_98_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_98_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_99_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_99_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_99_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_99_Metin);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_100_Var_Yok);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_100_Evet_Hayir);
			writer.Write((int)toWrite.zyrt_Temsilci_Ozel_100_Derece);
			writer.Write(toWrite.zyrt_Temsilci_Ozel_100_Metin);
		}
	}

	public static Ziyaret ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Ziyaret result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Ziyaret ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Ziyaret ReadFromBinaryReader(BinaryReader reader)
	{
		Ziyaret ziyaret = new Ziyaret();
		int num = reader.ReadInt32();
		ziyaret.baslangicbitis = (enum_BaslangicBitis)reader.ReadInt32();
		ziyaret.zyrt_RecNo = reader.ReadInt32();
		ziyaret.zyrt_Create_Date = new DateTime(reader.ReadInt64());
		ziyaret.zyrt_Lastup_Date = new DateTime(reader.ReadInt64());
		ziyaret.zyrt_iptal = reader.ReadBoolean();
		ziyaret.zyrt_Tarihi = new DateTime(reader.ReadInt64());
		ziyaret.zyrt_Temsilci_Kodu = reader.ReadString();
		ziyaret.zyrt_Bolge_Kodu = reader.ReadString();
		ziyaret.zyrt_Cari_Kodu = reader.ReadString();
		ziyaret.zyrt_Cari_Adres_No = reader.ReadInt32();
		ziyaret.zyrt_Cari_Adres_Enlem = reader.ReadDouble();
		ziyaret.zyrt_Cari_Adres_Boylam = reader.ReadDouble();
		ziyaret.zyrt_Baslama_Saati = new DateTime(reader.ReadInt64());
		ziyaret.zyrt_Baslama_Enlem = reader.ReadDouble();
		ziyaret.zyrt_Baslama_Boylam = reader.ReadDouble();
		ziyaret.zyrt_Bitis_Saati = new DateTime(reader.ReadInt64());
		ziyaret.zyrt_Bitis_Enlem = reader.ReadDouble();
		ziyaret.zyrt_Bitis_Boylam = reader.ReadDouble();
		ziyaret.zyrt_Tamamlandi = reader.ReadBoolean();
		int count = reader.ReadInt32();
		ziyaret.zyrt_Fotograf = reader.ReadBytes(count);
		ziyaret.zyrt_Fotograf_Id = reader.ReadInt32();
		ziyaret.zyrt_Proje_Kodu = reader.ReadString();
		ziyaret.zyrt_Sor_Mer_Kodu = reader.ReadString();
		ziyaret.zyrt_Bakim_Evrak_Seri = reader.ReadString();
		ziyaret.zyrt_Bakim_Evrak_Sira = reader.ReadInt32();
		ziyaret.zyrt_Satis_Yapildi = reader.ReadBoolean();
		ziyaret.zyrt_Siparis_Alindi = reader.ReadBoolean();
		ziyaret.zyrt_Urun_Teslim_Edildi = reader.ReadBoolean();
		ziyaret.zyrt_Tahsilat_Yapildi = reader.ReadBoolean();
		ziyaret.zyrt_Katalog_Birakildi = reader.ReadBoolean();
		ziyaret.zyrt_Fiyat_Listesi_Birakildi = reader.ReadBoolean();
		ziyaret.zyrt_Numune_Urun_Birakildi = reader.ReadBoolean();
		ziyaret.zyrt_Konsinye_Urun_Birakildi = reader.ReadBoolean();
		ziyaret.zyrt_Promosyon_Birakildi = reader.ReadBoolean();
		ziyaret.zyrt_Firma_Durumu = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Rakip_Firma_Var = reader.ReadBoolean();
		ziyaret.zyrt_Rakip_Firma_Durumu = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Urun_Yerlesim_Durumu = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Rakip_Urun_Yerlesim_Durumu = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Aciklama = reader.ReadString();
		ziyaret.zyrt_Cari_Firma_Memnuniyeti = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Cari_Urun_Memnuniyeti = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Cari_Fiyat_Memnuniyeti = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Cari_Temsilci_Memnuniyeti = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Cari_Aciklama = reader.ReadString();
		ziyaret.zyrt_Temsilci_Ozel_01_Var_Yok = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_01_Evet_Hayir = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_01_Derece = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_01_TamSayi = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_01_OndalikliSayi = reader.ReadDouble();
		ziyaret.zyrt_Temsilci_Ozel_01_Metin = reader.ReadString();
		int count2 = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_01_Fotograf = reader.ReadBytes(count2);
		ziyaret.zyrt_Temsilci_Ozel_01_Fotograf_Id = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_02_Var_Yok = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_02_Evet_Hayir = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_02_Derece = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_02_TamSayi = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_02_OndalikliSayi = reader.ReadDouble();
		ziyaret.zyrt_Temsilci_Ozel_02_Metin = reader.ReadString();
		int count3 = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_02_Fotograf = reader.ReadBytes(count3);
		ziyaret.zyrt_Temsilci_Ozel_02_Fotograf_Id = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_03_Var_Yok = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_03_Evet_Hayir = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_03_Derece = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_03_TamSayi = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_03_OndalikliSayi = reader.ReadDouble();
		ziyaret.zyrt_Temsilci_Ozel_03_Metin = reader.ReadString();
		int count4 = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_03_Fotograf = reader.ReadBytes(count4);
		ziyaret.zyrt_Temsilci_Ozel_03_Fotograf_Id = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_04_Var_Yok = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_04_Evet_Hayir = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_04_Derece = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_04_TamSayi = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_04_OndalikliSayi = reader.ReadDouble();
		ziyaret.zyrt_Temsilci_Ozel_04_Metin = reader.ReadString();
		int count5 = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_04_Fotograf = reader.ReadBytes(count5);
		ziyaret.zyrt_Temsilci_Ozel_04_Fotograf_Id = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_05_Var_Yok = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_05_Evet_Hayir = reader.ReadBoolean();
		ziyaret.zyrt_Temsilci_Ozel_05_Derece = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_05_TamSayi = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_05_OndalikliSayi = reader.ReadDouble();
		ziyaret.zyrt_Temsilci_Ozel_05_Metin = reader.ReadString();
		int count6 = reader.ReadInt32();
		ziyaret.zyrt_Temsilci_Ozel_05_Fotograf = reader.ReadBytes(count6);
		ziyaret.zyrt_Temsilci_Ozel_05_Fotograf_Id = reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_01_Var_Yok = reader.ReadBoolean();
		ziyaret.zyrt_Cari_Ozel_01_Evet_Hayir = reader.ReadBoolean();
		ziyaret.zyrt_Cari_Ozel_01_Derece = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_01_TamSayi = reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_01_OndalikliSayi = reader.ReadDouble();
		ziyaret.zyrt_Cari_Ozel_01_Metin = reader.ReadString();
		reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_01_Fotograf = reader.ReadBytes(count6);
		ziyaret.zyrt_Cari_Ozel_01_Fotograf_Id = reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_02_Var_Yok = reader.ReadBoolean();
		ziyaret.zyrt_Cari_Ozel_02_Evet_Hayir = reader.ReadBoolean();
		ziyaret.zyrt_Cari_Ozel_02_Derece = (enum_Derecelendirme)reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_02_TamSayi = reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_02_OndalikliSayi = reader.ReadDouble();
		ziyaret.zyrt_Cari_Ozel_02_Metin = reader.ReadString();
		reader.ReadInt32();
		ziyaret.zyrt_Cari_Ozel_02_Fotograf = reader.ReadBytes(count6);
		ziyaret.zyrt_Cari_Ozel_02_Fotograf_Id = reader.ReadInt32();
		if (num >= 2)
		{
			ziyaret.zyrt_Temsilci_Ozel_06_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_06_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_06_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_06_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_07_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_07_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_07_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_07_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_08_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_08_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_08_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_08_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_09_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_09_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_09_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_09_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_10_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_10_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_10_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_10_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_11_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_11_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_11_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_11_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_12_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_12_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_12_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_12_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_13_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_13_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_13_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_13_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_14_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_14_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_14_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_14_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_15_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_15_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_15_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_15_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_16_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_16_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_16_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_16_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_17_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_17_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_17_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_17_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_18_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_18_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_18_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_18_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_19_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_19_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_19_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_19_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_20_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_20_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_20_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_20_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_21_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_21_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_21_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_21_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_22_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_22_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_22_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_22_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_23_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_23_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_23_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_23_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_24_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_24_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_24_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_24_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_25_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_25_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_25_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_25_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_26_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_26_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_26_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_26_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_27_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_27_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_27_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_27_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_28_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_28_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_28_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_28_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_29_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_29_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_29_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_29_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_30_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_30_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_30_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_30_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_31_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_31_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_31_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_31_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_32_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_32_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_32_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_32_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_33_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_33_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_33_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_33_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_34_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_34_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_34_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_34_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_35_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_35_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_35_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_35_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_36_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_36_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_36_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_36_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_37_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_37_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_37_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_37_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_38_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_38_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_38_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_38_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_39_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_39_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_39_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_39_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_40_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_40_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_40_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_40_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_41_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_41_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_41_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_41_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_42_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_42_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_42_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_42_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_43_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_43_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_43_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_43_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_44_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_44_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_44_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_44_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_45_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_45_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_45_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_45_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_46_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_46_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_46_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_46_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_47_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_47_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_47_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_47_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_48_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_48_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_48_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_48_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_49_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_49_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_49_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_49_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_50_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_50_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_50_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_50_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_51_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_51_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_51_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_51_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_52_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_52_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_52_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_52_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_53_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_53_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_53_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_53_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_54_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_54_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_54_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_54_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_55_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_55_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_55_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_55_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_56_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_56_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_56_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_56_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_57_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_57_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_57_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_57_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_58_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_58_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_58_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_58_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_59_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_59_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_59_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_59_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_60_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_60_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_60_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_60_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_61_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_61_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_61_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_61_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_62_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_62_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_62_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_62_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_63_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_63_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_63_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_63_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_64_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_64_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_64_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_64_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_65_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_65_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_65_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_65_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_66_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_66_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_66_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_66_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_67_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_67_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_67_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_67_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_68_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_68_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_68_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_68_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_69_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_69_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_69_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_69_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_70_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_70_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_70_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_70_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_71_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_71_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_71_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_71_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_72_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_72_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_72_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_72_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_73_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_73_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_73_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_73_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_74_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_74_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_74_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_74_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_75_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_75_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_75_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_75_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_76_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_76_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_76_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_76_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_77_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_77_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_77_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_77_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_78_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_78_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_78_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_78_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_79_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_79_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_79_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_79_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_80_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_80_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_80_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_80_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_81_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_81_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_81_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_81_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_82_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_82_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_82_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_82_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_83_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_83_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_83_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_83_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_84_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_84_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_84_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_84_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_85_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_85_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_85_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_85_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_86_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_86_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_86_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_86_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_87_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_87_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_87_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_87_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_88_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_88_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_88_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_88_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_89_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_89_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_89_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_89_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_90_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_90_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_90_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_90_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_91_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_91_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_91_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_91_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_92_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_92_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_92_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_92_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_93_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_93_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_93_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_93_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_94_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_94_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_94_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_94_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_95_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_95_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_95_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_95_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_96_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_96_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_96_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_96_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_97_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_97_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_97_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_97_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_98_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_98_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_98_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_98_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_99_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_99_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_99_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_99_Metin = reader.ReadString();
			ziyaret.zyrt_Temsilci_Ozel_100_Var_Yok = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_100_Evet_Hayir = reader.ReadBoolean();
			ziyaret.zyrt_Temsilci_Ozel_100_Derece = (enum_Derecelendirme)reader.ReadInt32();
			ziyaret.zyrt_Temsilci_Ozel_100_Metin = reader.ReadString();
		}
		return ziyaret;
	}
}
