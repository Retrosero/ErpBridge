using System;
using Fora.Mikro.Bankalar;
using Fora.Mikro.Evraklar;

namespace Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;

public class AktarimSatirlar
{
	public enum_GenelEvrakTipleri EvrakTipi { get; set; }

	public DateTime EvrakTarihi { get; set; }

	public string EvrakSrmMerkeziKodu { get; set; }

	public enum_BankaSatirCinsi Cinsi { get; set; }

	public string HesapKodu { get; set; }

	public int GrupNo { get; set; }

	public double Tutar { get; set; }

	public string Aciklama { get; set; }

	public enum_FaturaOlusturmaDurumu FaturaOlusturmaDurumu { get; set; }

	public string FaturaHesapKodu { get; set; }

	public double FaturaMiktari { get; set; }

	public string FaturaSeri { get; set; }

	public int FaturaSira { get; set; }

	public string FaturaSorumlulukMerkezi { get; set; }

	public string FaturaProje { get; set; }

	public string BV_Aciklama { get; set; }

	public string BV_IslemKodu { get; set; }

	public string BV_HesapNo { get; set; }

	public string BV_Unvan { get; set; }

	public string BV_Unvan2 { get; set; }

	public string BV_Adres { get; set; }

	public string BV_Mahalle { get; set; }

	public string BV_Ilce { get; set; }

	public string BV_Il { get; set; }

	public string BV_Ulke { get; set; }

	public string BV_PostaKodu { get; set; }

	public string BV_Telefon { get; set; }

	public string BV_EPosta { get; set; }

	public string BV_TcVergiNo { get; set; }

	public bool OnayDurumuHesapKodu { get; set; }

	public bool OnayDurumuFaturaHesapKodu { get; set; }

	public bool OnayDurumuFaturaSorumlulukMerkezi { get; set; }

	public bool OnayDurumuFaturaProje { get; set; }

	public bool mikro_disi_ek_bilgileri_kullan { get; set; }

	public string metin1 { get; set; }

	public string metin2 { get; set; }

	public string metin3 { get; set; }

	public string metin4 { get; set; }

	public string metin5 { get; set; }

	public string metin6 { get; set; }

	public string metin7 { get; set; }

	public string metin8 { get; set; }

	public string metin9 { get; set; }

	public string metin10 { get; set; }

	public string metin11 { get; set; }

	public string metin12 { get; set; }

	public string metin13 { get; set; }

	public string metin14 { get; set; }

	public string metin15 { get; set; }

	public string metin16 { get; set; }

	public string metin17 { get; set; }

	public string metin18 { get; set; }

	public string metin19 { get; set; }

	public string metin20 { get; set; }

	public string metin21 { get; set; }

	public string metin22 { get; set; }

	public string metin23 { get; set; }

	public string metin24 { get; set; }

	public string metin25 { get; set; }

	public string metin26 { get; set; }

	public string metin27 { get; set; }

	public string metin28 { get; set; }

	public string metin29 { get; set; }

	public string metin30 { get; set; }

	public string metin31 { get; set; }

	public string metin32 { get; set; }

	public string metin33 { get; set; }

	public string metin34 { get; set; }

	public string metin35 { get; set; }

	public string metin36 { get; set; }

	public string metin37 { get; set; }

	public string metin38 { get; set; }

	public string metin39 { get; set; }

	public string metin40 { get; set; }

	public string metin41 { get; set; }

	public string metin42 { get; set; }

	public string metin43 { get; set; }

	public string metin44 { get; set; }

	public string metin45 { get; set; }

	public string metin46 { get; set; }

	public string metin47 { get; set; }

	public string metin48 { get; set; }

	public string metin49 { get; set; }

	public string metin50 { get; set; }

	public string dropbox1 { get; set; }

	public string dropbox2 { get; set; }

	public string dropbox3 { get; set; }

	public string dropbox4 { get; set; }

	public string dropbox5 { get; set; }

	public string dropbox6 { get; set; }

	public string dropbox7 { get; set; }

	public string dropbox8 { get; set; }

	public string dropbox9 { get; set; }

	public string dropbox10 { get; set; }

	public bool checkbox1 { get; set; }

	public bool checkbox2 { get; set; }

	public bool checkbox3 { get; set; }

	public bool checkbox4 { get; set; }

	public bool checkbox5 { get; set; }

	public bool checkbox6 { get; set; }

	public bool checkbox7 { get; set; }

	public bool checkbox8 { get; set; }

	public bool checkbox9 { get; set; }

	public bool checkbox10 { get; set; }

	public AktarimSatirlar()
	{
		EvrakTipi = enum_GenelEvrakTipleri.GelenHavale;
		EvrakSrmMerkeziKodu = "";
		EvrakTarihi = DateTime.MinValue;
		Cinsi = enum_BankaSatirCinsi.CariHesap;
		HesapKodu = "";
		GrupNo = 0;
		Tutar = 0.0;
		Aciklama = "";
		FaturaOlusturmaDurumu = enum_FaturaOlusturmaDurumu.Olusturma;
		FaturaHesapKodu = "";
		FaturaMiktari = 1.0;
		FaturaSeri = "";
		FaturaSira = 0;
		FaturaSorumlulukMerkezi = "";
		FaturaProje = "";
		BV_Aciklama = "";
		BV_IslemKodu = "";
		BV_HesapNo = "";
		BV_Unvan = "";
		BV_Unvan2 = "";
		BV_Adres = "";
		BV_Mahalle = "";
		BV_Ilce = "";
		BV_Il = "";
		BV_Ulke = "";
		BV_PostaKodu = "";
		BV_Telefon = "";
		BV_EPosta = "";
		BV_TcVergiNo = "";
		OnayDurumuHesapKodu = false;
		OnayDurumuFaturaHesapKodu = false;
		OnayDurumuFaturaSorumlulukMerkezi = false;
		OnayDurumuFaturaProje = false;
		mikro_disi_ek_bilgileri_kullan = false;
		metin1 = "";
		metin2 = "";
		metin3 = "";
		metin4 = "";
		metin5 = "";
		metin6 = "";
		metin7 = "";
		metin8 = "";
		metin9 = "";
		metin10 = "";
		metin11 = "";
		metin12 = "";
		metin13 = "";
		metin14 = "";
		metin15 = "";
		metin16 = "";
		metin17 = "";
		metin18 = "";
		metin19 = "";
		metin20 = "";
		metin21 = "";
		metin22 = "";
		metin23 = "";
		metin24 = "";
		metin25 = "";
		metin26 = "";
		metin27 = "";
		metin28 = "";
		metin29 = "";
		metin30 = "";
		metin31 = "";
		metin32 = "";
		metin33 = "";
		metin34 = "";
		metin35 = "";
		metin36 = "";
		metin37 = "";
		metin38 = "";
		metin39 = "";
		metin40 = "";
		metin41 = "";
		metin42 = "";
		metin43 = "";
		metin44 = "";
		metin45 = "";
		metin46 = "";
		metin47 = "";
		metin48 = "";
		metin49 = "";
		metin50 = "";
		dropbox1 = "";
		dropbox2 = "";
		dropbox3 = "";
		dropbox4 = "";
		dropbox5 = "";
		dropbox6 = "";
		dropbox7 = "";
		dropbox8 = "";
		dropbox9 = "";
		dropbox10 = "";
		checkbox1 = false;
		checkbox2 = false;
		checkbox3 = false;
		checkbox4 = false;
		checkbox5 = false;
		checkbox6 = false;
		checkbox7 = false;
		checkbox8 = false;
		checkbox9 = false;
		checkbox10 = false;
	}
}
