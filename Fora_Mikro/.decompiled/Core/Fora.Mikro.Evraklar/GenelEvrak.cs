using System;
using System.Collections.Generic;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.Depolar;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Kurlar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Projeler;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.Utility;
using Fora.Mikro.Vergiler;
using Fora.Mikro.Yazdirma;

namespace Fora.Mikro.Evraklar;

public class GenelEvrak
{
	public enum_GenelEvrakTipleri evrak_tipi { get; set; }

	public DateTime evrak_tarih { get; set; }

	public string evrakno_seri { get; set; }

	public int evrakno_sira { get; set; }

	public string evrak_seri_sira => evrakno_seri + "-" + evrakno_sira;

	public string belge_no { get; set; }

	public DateTime belge_tarih { get; set; }

	public Cari cari { get; set; }

	public CariPersonel temsilci { get; set; }

	public int doviz_cinsi { get; set; }

	public Kur kur { get; set; }

	public int alternatif_doviz_cinsi { get; set; }

	public Kur alternatif_doviz_kuru { get; set; }

	public Depo cikis_depo { get; set; }

	public Depo giris_depo { get; set; }

	public Depo nakliye_depo { get; set; }

	public Proje proje { get; set; }

	public SorumlulukMerkezi sorumluluk_merkezi { get; set; }

	public int sevk_adres_no { get; set; }

	public string aciklama1 { get; set; }

	public string aciklama2 { get; set; }

	public string aciklama3 { get; set; }

	public string aciklama4 { get; set; }

	public string aciklama5 { get; set; }

	public string aciklama6 { get; set; }

	public string aciklama7 { get; set; }

	public string aciklama8 { get; set; }

	public string aciklama9 { get; set; }

	public string aciklama10 { get; set; }

	public double onceki_bakiye { get; set; }

	public double simdiki_bakiye { get; set; }

	public List<GenelEvrakSatirlari> satirlar { get; set; }

	public double cek_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.satir_cinsi == GenelEvrakSatirCinsleri.Cek)
				{
					num += item.tutar;
				}
			}
			return num;
		}
	}

	public double nakit_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.satir_cinsi == GenelEvrakSatirCinsleri.Nakit)
				{
					num += item.tutar;
				}
			}
			return num;
		}
	}

	public int cek_sayisi
	{
		get
		{
			int num = 0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.satir_cinsi == GenelEvrakSatirCinsleri.Cek)
				{
					num++;
				}
			}
			return num;
		}
	}

	public double yekun => ara_toplam - toplam_iskonto_tutari + toplam_masraf_tutari + toplam_vergi_tutari;

	public string yazi_ile_yekun => GenelUtility.YaziIleTutar(yekun);

	public double ara_toplam
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.toplam_fiyat_brut;
			}
			return num;
		}
	}

	public double toplam_iskonto_tutari
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.toplam_iskonto_tutari;
			}
			return num;
		}
	}

	public double toplam_iskonto_yuzdesi
	{
		get
		{
			if (toplam_iskonto_tutari == 0.0)
			{
				return 0.0;
			}
			return (1.0 - (ara_toplam - toplam_iskonto_tutari) / ara_toplam) * 100.0;
		}
	}

	public double toplam_masraf_tutari
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.toplam_masraf_tutari;
			}
			return num;
		}
	}

	public double toplam_masraf_yuzdesi
	{
		get
		{
			if (toplam_masraf_tutari == 0.0)
			{
				return 0.0;
			}
			return (1.0 - toplam_masraf_tutari / ara_toplam) * 100.0;
		}
	}

	public double toplam_vergi_tutari => vergi_1_toplami + vergi_2_toplami + vergi_3_toplami + vergi_4_toplami + vergi_5_toplami + vergi_6_toplami + vergi_7_toplami + vergi_8_toplami + vergi_8_toplami + vergi_10_toplami;

	public double iskonto_1_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.iskonto_1_tutari;
			}
			return num;
		}
	}

	public double iskonto_2_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.iskonto_2_tutari;
			}
			return num;
		}
	}

	public double iskonto_3_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.iskonto_3_tutari;
			}
			return num;
		}
	}

	public double iskonto_4_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.iskonto_4_tutari;
			}
			return num;
		}
	}

	public double iskonto_5_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.iskonto_5_tutari;
			}
			return num;
		}
	}

	public double iskonto_6_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.iskonto_6_tutari;
			}
			return num;
		}
	}

	public double masraf_1_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.masraf_1_tutari;
			}
			return num;
		}
	}

	public double masraf_2_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.masraf_2_tutari;
			}
			return num;
		}
	}

	public double masraf_3_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.masraf_3_tutari;
			}
			return num;
		}
	}

	public double masraf_4_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.masraf_4_tutari;
			}
			return num;
		}
	}

	public double vergi_1_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 1)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_1_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 1)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_2_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 2)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_2_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 2)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_3_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 3)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_3_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 3)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_4_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 4)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_4_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 4)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_5_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 5)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_5_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 5)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_6_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 6)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_6_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 6)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_7_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 7)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_7_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 7)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_8_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 8)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_8_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 8)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_9_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 9)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_9_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 9)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double vergi_10_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 10)
				{
					num += item.toplam_vergi_tutari;
				}
			}
			return Math.Round(num, 2);
		}
	}

	public double vergi_10_matrahi
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				if (item.vergi_pntr == 10)
				{
					num += item.toplam_fiyat_net;
				}
			}
			return num;
		}
	}

	public double miktar_1_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.birim_1_miktar;
			}
			return num;
		}
	}

	public double miktar_2_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.birim_2_miktar;
			}
			return num;
		}
	}

	public double miktar_3_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.birim_3_miktar;
			}
			return num;
		}
	}

	public double miktar_4_toplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.birim_4_miktar;
			}
			return num;
		}
	}

	public CariAdres sevk_adresi
	{
		get
		{
			if (cari != null)
			{
				foreach (CariAdres item in cari.CariAdresleri)
				{
					if (item.adr_adres_no == sevk_adres_no)
					{
						return item;
					}
				}
			}
			return new CariAdres();
		}
	}

	public CariAdres fatura_adresi
	{
		get
		{
			if (cari != null)
			{
				foreach (CariAdres item in cari.CariAdresleri)
				{
					if (item.adr_adres_no == cari.cari_fatura_adres_no)
					{
						return item;
					}
				}
			}
			return new CariAdres();
		}
	}

	public string saat => DateTime.Now.ToString("HH:mm:ss");

	public double toplam_vergi_yuzdesi => 0.0;

	public string iskonto_1_adi => "";

	public double iskonto_1_yuzdesi => 0.0;

	public string iskonto_2_adi => "";

	public double iskonto_2_yuzdesi => 0.0;

	public string iskonto_3_adi => "";

	public double iskonto_3_yuzdesi => 0.0;

	public string iskonto_4_adi => "";

	public double iskonto_4_yuzdesi => 0.0;

	public string iskonto_5_adi => "";

	public double iskonto_5_yuzdesi => 0.0;

	public string iskonto_6_adi => "";

	public double iskonto_6_yuzdesi => 0.0;

	public string masraf_1_adi => "";

	public double masraf_1_yuzdesi => 0.0;

	public string masraf_2_adi => "";

	public double masraf_2_yuzdesi => 0.0;

	public string masraf_3_adi => "";

	public double masraf_3_yuzdesi => 0.0;

	public string masraf_4_adi => "";

	public double masraf_4_yuzdesi => 0.0;

	public string vergi_1_adi => "";

	public double vergi_1_yuzdesi => 0.0;

	public string vergi_2_adi => "";

	public double vergi_2_yuzdesi => 0.0;

	public string vergi_3_adi => "";

	public double vergi_3_yuzdesi => 0.0;

	public string vergi_4_adi => "";

	public double vergi_4_yuzdesi => 0.0;

	public string vergi_5_adi => "";

	public double vergi_5_yuzdesi => 0.0;

	public string vergi_6_adi => "";

	public double vergi_6_yuzdesi => 0.0;

	public string vergi_7_adi => "";

	public double vergi_7_yuzdesi => 0.0;

	public string vergi_8_adi => "";

	public double vergi_8_yuzdesi => 0.0;

	public string vergi_9_adi => "";

	public double vergi_9_yuzdesi => 0.0;

	public string vergi_10_adi => "";

	public double vergi_10_yuzdesi => 0.0;

	public double miktar2bagimsizbirimtoplami
	{
		get
		{
			double num = 0.0;
			foreach (GenelEvrakSatirlari item in satirlar)
			{
				num += item.miktar2;
			}
			return num;
		}
	}

	public GenelEvrak()
	{
		evrak_tipi = enum_GenelEvrakTipleri.AlinanSiparis;
	}

	public List<string> GetDotMatrixText(YaziciAyarlari yaziciayarlari, List<VergiTanimi> vergitanimlari, DovizCinsiTanimlari doviz_cinsi_tanimlari)
	{
		List<string> list = new List<string>();
		int getInt = yaziciayarlari.genelayarlar._GetParametre("SayfaKolonSayisi")._GetInt;
		int getInt2 = yaziciayarlari.genelayarlar._GetParametre("SayfaSatirSayisi")._GetInt;
		int getInt3 = yaziciayarlari.genelayarlar._GetParametre("DetayBaslangicSatiri")._GetInt;
		int getInt4 = yaziciayarlari.genelayarlar._GetParametre("DetayBasiSatirSayisi")._GetInt;
		int getInt5 = yaziciayarlari.genelayarlar._GetParametre("DetayBirSayfadakiKayitSayisi")._GetInt;
		bool getBoolean = yaziciayarlari.genelayarlar._GetParametre("AltBasliklarSadeceSonSayfadaYazilsin")._GetBoolean;
		int num = 1;
		num = (int)Math.Ceiling((double)satirlar.Count / (double)getInt5);
		for (int i = 0; i < getInt2 * num; i++)
		{
			list.Add(new string(' ', getInt));
		}
		List<YazdirmaAlani> list2 = new List<YazdirmaAlani>();
		List<YazdirmaAlani> list3 = new List<YazdirmaAlani>();
		List<YazdirmaAlani> list4 = new List<YazdirmaAlani>();
		foreach (Parametreler item in yaziciayarlari.alanlar)
		{
			YazdirmaAlani yazdirmaAlani = new YazdirmaAlani();
			yazdirmaAlani.basilacakalan = (enum_Yazdirma_BasilacakAlan)item._GetParametre("BasilacakAlan")._GetInt;
			yazdirmaAlani.veritipi = item._GetParametre("VeriTipi")._GetInt;
			yazdirmaAlani.veri = item._GetParametre("Veri")._GetString;
			yazdirmaAlani.kolon = item._GetParametre("Kolon")._GetInt;
			yazdirmaAlani.satir = item._GetParametre("Satir")._GetInt;
			yazdirmaAlani.genislik = item._GetParametre("Genislik")._GetInt;
			yazdirmaAlani.hizalama = (enum_Yazdirma_Hizalama)item._GetParametre("Hizalama")._GetInt;
			yazdirmaAlani.binlik_ayraci = item._GetParametre("BinlikAyraci")._GetString;
			yazdirmaAlani.ondalik_ayraci = item._GetParametre("OndalikAyraci")._GetString;
			yazdirmaAlani.ondalik_hane_sayisi = item._GetParametre("OndalikHaneSayisi")._GetInt;
			yazdirmaAlani.sonuna_para_birimi_ekle = item._GetParametre("SonunaParaBirimiEkle")._GetBoolean;
			yazdirmaAlani.basina_para_birimi_ekle = item._GetParametre("BasinaParaBirimiEkle")._GetBoolean;
			yazdirmaAlani.on_ek = item._GetParametre("OnEk")._GetString;
			yazdirmaAlani.son_ek = item._GetParametre("SonEk")._GetString;
			yazdirmaAlani.detayin_bittigi_yere_kaydir = item._GetParametre("DetayinBittigiYereKaydir")._GetBoolean;
			switch (yazdirmaAlani.basilacakalan)
			{
			case enum_Yazdirma_BasilacakAlan.AltBaslik:
				list4.Add(yazdirmaAlani);
				break;
			case enum_Yazdirma_BasilacakAlan.Satir:
				list3.Add(yazdirmaAlani);
				break;
			case enum_Yazdirma_BasilacakAlan.UstBaslik:
				list2.Add(yazdirmaAlani);
				break;
			}
		}
		int num2 = 0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		for (int j = 1; j < num + 1; j++)
		{
			foreach (YazdirmaAlani item2 in list2)
			{
				string text = item2.veri;
				if (item2.veritipi != 0)
				{
					text = StatikAlanVeriGetir(item2.veri, item2.binlik_ayraci, item2.ondalik_ayraci, item2.ondalik_hane_sayisi, item2.sonuna_para_birimi_ekle, item2.basina_para_birimi_ekle, doviz_cinsi_tanimlari);
				}
				text = item2.on_ek + text + item2.son_ek;
				if (text.Length > item2.genislik)
				{
					text = text.Substring(0, item2.genislik);
				}
				int num6 = item2.kolon - 1;
				switch (item2.hizalama)
				{
				case enum_Yazdirma_Hizalama.Orta:
					num6 += (item2.genislik - text.Length) / 2;
					break;
				case enum_Yazdirma_Hizalama.Sag:
					num6 += item2.genislik - text.Length;
					break;
				}
				int num7 = item2.satir - 1;
				num7 += (j - 1) * getInt2;
				list[num7] = list[num7].Remove(num6, text.Length);
				list[num7] = list[num7].Insert(num6, text);
			}
			int num8 = 0;
			int k = 0;
			int num9 = getInt3;
			num9 += (j - 1) * getInt2;
			if (j != 1)
			{
				foreach (YazdirmaAlani item3 in list3)
				{
					GenelEvrakDinamikAlanlar genelEvrakDinamikAlanlar = (GenelEvrakDinamikAlanlar)int.Parse(item3.veri);
					if (genelEvrakDinamikAlanlar == GenelEvrakDinamikAlanlar.ToplamFiyatBrut || genelEvrakDinamikAlanlar == GenelEvrakDinamikAlanlar.ToplamFiyatNetKdvDahil || genelEvrakDinamikAlanlar == GenelEvrakDinamikAlanlar.ToplamFiyatNet)
					{
						string text2 = item3.veri;
						if (genelEvrakDinamikAlanlar == GenelEvrakDinamikAlanlar.ToplamFiyatBrut)
						{
							text2 = GenelUtility.DoubleToString(num3, item3.binlik_ayraci, item3.ondalik_ayraci, item3.ondalik_hane_sayisi, item3.sonuna_para_birimi_ekle, item3.basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol);
						}
						if (genelEvrakDinamikAlanlar == GenelEvrakDinamikAlanlar.ToplamFiyatNetKdvDahil)
						{
							text2 = GenelUtility.DoubleToString(num5, item3.binlik_ayraci, item3.ondalik_ayraci, item3.ondalik_hane_sayisi, item3.sonuna_para_birimi_ekle, item3.basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol);
						}
						if (genelEvrakDinamikAlanlar == GenelEvrakDinamikAlanlar.ToplamFiyatNet)
						{
							text2 = GenelUtility.DoubleToString(num4, item3.binlik_ayraci, item3.ondalik_ayraci, item3.ondalik_hane_sayisi, item3.sonuna_para_birimi_ekle, item3.basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol);
						}
						text2 = item3.on_ek + text2 + item3.son_ek;
						if (text2.Length > item3.genislik)
						{
							text2 = text2.Substring(0, item3.genislik);
						}
						int num10 = item3.kolon - 1;
						switch (item3.hizalama)
						{
						case enum_Yazdirma_Hizalama.Orta:
							num10 += (item3.genislik - text2.Length) / 2;
							break;
						case enum_Yazdirma_Hizalama.Sag:
							num10 += item3.genislik - text2.Length;
							break;
						}
						list[num9 - 1 + item3.satir] = list[num9 - 1 + item3.satir].Remove(num10, text2.Length);
						list[num9 - 1 + item3.satir] = list[num9 - 1 + item3.satir].Insert(num10, text2);
						num9++;
						num8 = num9;
					}
				}
			}
			for (; k <= getInt5 - 1; k++)
			{
				if (num2 > satirlar.Count - 1)
				{
					break;
				}
				GenelEvrakSatirlari genelEvrakSatirlari = satirlar[num2];
				num3 += genelEvrakSatirlari.toplam_fiyat_brut;
				num4 += genelEvrakSatirlari.toplam_fiyat_net;
				num5 += genelEvrakSatirlari.toplam_fiyat_net + genelEvrakSatirlari.toplam_vergi_tutari;
				foreach (YazdirmaAlani item4 in list3)
				{
					string text3 = item4.veri;
					if (item4.veritipi != 0)
					{
						text3 = DinamikAlanVeriGetir(genelEvrakSatirlari, item4.veri, item4.binlik_ayraci, item4.ondalik_ayraci, item4.ondalik_hane_sayisi, item4.sonuna_para_birimi_ekle, item4.basina_para_birimi_ekle, doviz_cinsi_tanimlari, vergitanimlari);
					}
					text3 = item4.on_ek + text3 + item4.son_ek;
					if (text3.Length > item4.genislik)
					{
						text3 = text3.Substring(0, item4.genislik);
					}
					int num11 = item4.kolon - 1;
					switch (item4.hizalama)
					{
					case enum_Yazdirma_Hizalama.Orta:
						num11 += (item4.genislik - text3.Length) / 2;
						break;
					case enum_Yazdirma_Hizalama.Sag:
						num11 += item4.genislik - text3.Length;
						break;
					}
					list[num9 - 1 + item4.satir] = list[num9 - 1 + item4.satir].Remove(num11, text3.Length);
					list[num9 - 1 + item4.satir] = list[num9 - 1 + item4.satir].Insert(num11, text3);
				}
				num9 += getInt4;
				num8 = num9;
				num2++;
			}
			if (j != num && num != 1)
			{
				foreach (YazdirmaAlani item5 in list3)
				{
					GenelEvrakDinamikAlanlar genelEvrakDinamikAlanlar2 = (GenelEvrakDinamikAlanlar)int.Parse(item5.veri);
					if (genelEvrakDinamikAlanlar2 == GenelEvrakDinamikAlanlar.ToplamFiyatBrut || genelEvrakDinamikAlanlar2 == GenelEvrakDinamikAlanlar.ToplamFiyatNetKdvDahil || genelEvrakDinamikAlanlar2 == GenelEvrakDinamikAlanlar.ToplamFiyatNet)
					{
						string text4 = item5.veri;
						if (genelEvrakDinamikAlanlar2 == GenelEvrakDinamikAlanlar.ToplamFiyatBrut)
						{
							text4 = GenelUtility.DoubleToString(num3, item5.binlik_ayraci, item5.ondalik_ayraci, item5.ondalik_hane_sayisi, item5.sonuna_para_birimi_ekle, item5.basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol);
						}
						if (genelEvrakDinamikAlanlar2 == GenelEvrakDinamikAlanlar.ToplamFiyatNetKdvDahil)
						{
							text4 = GenelUtility.DoubleToString(num5, item5.binlik_ayraci, item5.ondalik_ayraci, item5.ondalik_hane_sayisi, item5.sonuna_para_birimi_ekle, item5.basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol);
						}
						if (genelEvrakDinamikAlanlar2 == GenelEvrakDinamikAlanlar.ToplamFiyatNet)
						{
							text4 = GenelUtility.DoubleToString(num4, item5.binlik_ayraci, item5.ondalik_ayraci, item5.ondalik_hane_sayisi, item5.sonuna_para_birimi_ekle, item5.basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol);
						}
						text4 = item5.on_ek + text4 + item5.son_ek;
						if (text4.Length > item5.genislik)
						{
							text4 = text4.Substring(0, item5.genislik);
						}
						int num12 = item5.kolon - 1;
						switch (item5.hizalama)
						{
						case enum_Yazdirma_Hizalama.Orta:
							num12 += (item5.genislik - text4.Length) / 2;
							break;
						case enum_Yazdirma_Hizalama.Sag:
							num12 += item5.genislik - text4.Length;
							break;
						}
						list[num9 - 1 + item5.satir] = list[num9 - 1 + item5.satir].Remove(num12, text4.Length);
						list[num9 - 1 + item5.satir] = list[num9 - 1 + item5.satir].Insert(num12, text4);
						num9++;
						num8 = num9;
					}
				}
			}
			bool flag = true;
			if (getBoolean && num != j)
			{
				flag = false;
			}
			if (!flag)
			{
				continue;
			}
			foreach (YazdirmaAlani item6 in list4)
			{
				string text5 = item6.veri;
				if (item6.veritipi != 0)
				{
					text5 = StatikAlanVeriGetir(item6.veri, item6.binlik_ayraci, item6.ondalik_ayraci, item6.ondalik_hane_sayisi, item6.sonuna_para_birimi_ekle, item6.basina_para_birimi_ekle, doviz_cinsi_tanimlari);
				}
				text5 = item6.on_ek + text5 + item6.son_ek;
				if (text5.Length > item6.genislik)
				{
					text5 = text5.Substring(0, item6.genislik);
				}
				int num13 = item6.kolon - 1;
				switch (item6.hizalama)
				{
				case enum_Yazdirma_Hizalama.Orta:
					num13 += (item6.genislik - text5.Length) / 2;
					break;
				case enum_Yazdirma_Hizalama.Sag:
					num13 += item6.genislik - text5.Length;
					break;
				}
				int satir = item6.satir;
				satir = ((!item6.detayin_bittigi_yere_kaydir) ? (satir + (getInt3 + getInt5 * getInt4)) : (satir + (num8 - 1)));
				list[satir - 1] = list[satir - 1].Remove(num13, text5.Length);
				list[satir - 1] = list[satir - 1].Insert(num13, text5);
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			list[l] = list[l].TrimEnd(Array.Empty<char>());
		}
		return list;
	}

	private string StatikAlanVeriGetir(string veristring, string binlik_ayraci, string ondalik_ayraci, int ondalik_hane_sayisi, bool sonuna_para_birimi_ekle, bool basina_para_birimi_ekle, DovizCinsiTanimlari doviz_cinsi_tanimlari)
	{
		GenelEvrakStatikAlanlar genelEvrakStatikAlanlar = (GenelEvrakStatikAlanlar)int.Parse(veristring);
		return genelEvrakStatikAlanlar switch
		{
			GenelEvrakStatikAlanlar.EvrakTarihi => evrak_tarih.ToString("dd.MM.yyyy"), 
			GenelEvrakStatikAlanlar.Saat => DateTime.Now.ToString("HH:mm"), 
			GenelEvrakStatikAlanlar.EvrakSeri => evrakno_seri, 
			GenelEvrakStatikAlanlar.EvrakSira => evrakno_sira.ToString(), 
			GenelEvrakStatikAlanlar.EvrakSeriSira => evrak_seri_sira, 
			GenelEvrakStatikAlanlar.BelgeNo => belge_no, 
			GenelEvrakStatikAlanlar.BelgeTarihi => belge_tarih.ToString("dd.MM.yyyy"), 
			GenelEvrakStatikAlanlar.CariKod => cari.cari_kod, 
			GenelEvrakStatikAlanlar.CariUnvan1 => cari.cari_unvan1, 
			GenelEvrakStatikAlanlar.CariUnvan2 => cari.cari_unvan2, 
			GenelEvrakStatikAlanlar.CariUnvanBirlesik => cari.cari_unvan1 + " " + cari.cari_unvan2, 
			GenelEvrakStatikAlanlar.CariVDaireNo => cari.cari_vdaire_no, 
			GenelEvrakStatikAlanlar.CariVDaireAdi => cari.cari_vdaire_adi, 
			GenelEvrakStatikAlanlar.CariVergiKimlikNo => cari.cari_VergiKimlikNo, 
			GenelEvrakStatikAlanlar.CariBolgeKodu => cari.cari_bolge_kodu, 
			GenelEvrakStatikAlanlar.CariCepTelefonu => cari.cari_CepTel, 
			GenelEvrakStatikAlanlar.CariEPosta => cari.cari_Email, 
			GenelEvrakStatikAlanlar.CariGrupKodu => cari.cari_grup_kodu, 
			GenelEvrakStatikAlanlar.CariSektörKodu => cari.cari_sektor_kodu, 
			GenelEvrakStatikAlanlar.CariSicilNo => cari.cari_sicil_no, 
			GenelEvrakStatikAlanlar.CariWebAdresi => cari.cari_wwwadresi, 
			GenelEvrakStatikAlanlar.TemsilciKodu => temsilci.cari_per_kod, 
			GenelEvrakStatikAlanlar.TemsilciAdi => temsilci.cari_per_adi, 
			GenelEvrakStatikAlanlar.TemsilciSoyAdi => temsilci.cari_per_soyadi, 
			GenelEvrakStatikAlanlar.TemsilciAdiSoyAdi => temsilci.adi_soyadi, 
			GenelEvrakStatikAlanlar.DovizCinsiAdi => doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_adi, 
			GenelEvrakStatikAlanlar.DovizCinsiSembol => doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol, 
			GenelEvrakStatikAlanlar.Kur => GenelUtility.DoubleToString(kur.dov_fiyat, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.AlternatifDovizCinsiAdi => doviz_cinsi_tanimlari.GetDovizCinsiTanimi(alternatif_doviz_cinsi).Kur_adi, 
			GenelEvrakStatikAlanlar.AlternatifDovizCinsiSembol => doviz_cinsi_tanimlari.GetDovizCinsiTanimi(alternatif_doviz_cinsi).Kur_sembol, 
			GenelEvrakStatikAlanlar.AlternatifDovizKuru => GenelUtility.DoubleToString(alternatif_doviz_kuru.dov_fiyat, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.CikisDepoNo => cikis_depo.dep_no.ToString(), 
			GenelEvrakStatikAlanlar.CikisDepoAdi => cikis_depo.dep_adi, 
			GenelEvrakStatikAlanlar.GirisDepoNo => giris_depo.dep_no.ToString(), 
			GenelEvrakStatikAlanlar.GirisDepoAdi => giris_depo.dep_adi, 
			GenelEvrakStatikAlanlar.NakliyeDepoNo => nakliye_depo.dep_no.ToString(), 
			GenelEvrakStatikAlanlar.NakliyeDepoAdi => nakliye_depo.dep_adi, 
			GenelEvrakStatikAlanlar.ProjeKodu => proje.pro_kodu, 
			GenelEvrakStatikAlanlar.ProjeAdi => proje.pro_adi, 
			GenelEvrakStatikAlanlar.SorumlulukMerkeziKodu => sorumluluk_merkezi.som_kod, 
			GenelEvrakStatikAlanlar.SorumlulukMerkeziAdi => sorumluluk_merkezi.som_isim, 
			GenelEvrakStatikAlanlar.Aciklama1 => aciklama1, 
			GenelEvrakStatikAlanlar.Aciklama2 => aciklama2, 
			GenelEvrakStatikAlanlar.Aciklama3 => aciklama3, 
			GenelEvrakStatikAlanlar.Aciklama4 => aciklama4, 
			GenelEvrakStatikAlanlar.Aciklama5 => aciklama5, 
			GenelEvrakStatikAlanlar.Aciklama6 => aciklama6, 
			GenelEvrakStatikAlanlar.Aciklama7 => aciklama7, 
			GenelEvrakStatikAlanlar.Aciklama8 => aciklama8, 
			GenelEvrakStatikAlanlar.Aciklama9 => aciklama9, 
			GenelEvrakStatikAlanlar.Aciklama10 => aciklama10, 
			GenelEvrakStatikAlanlar.CekToplami => GenelUtility.DoubleToString(cek_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.NakitToplami => GenelUtility.DoubleToString(nakit_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.CekSayisi => cek_sayisi.ToString(), 
			GenelEvrakStatikAlanlar.Yekun => GenelUtility.DoubleToString(yekun, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.YekunYaziIle => yazi_ile_yekun, 
			GenelEvrakStatikAlanlar.AraToplam => GenelUtility.DoubleToString(ara_toplam, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.ToplamIskontoTutari => GenelUtility.DoubleToString(toplam_iskonto_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.ToplamIskontoYuzdesi => GenelUtility.DoubleToString(toplam_iskonto_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.ToplamMasrafTutari => GenelUtility.DoubleToString(toplam_masraf_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.ToplamMasrafYuzdesi => GenelUtility.DoubleToString(toplam_masraf_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.ToplamVergiTutari => GenelUtility.DoubleToString(toplam_vergi_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.ToplamVergiYuzdesi => GenelUtility.DoubleToString(toplam_vergi_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto1Toplami => GenelUtility.DoubleToString(iskonto_1_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto1Yuzdesi => GenelUtility.DoubleToString(iskonto_1_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto1Adi => iskonto_1_adi, 
			GenelEvrakStatikAlanlar.Iskonto2Toplami => GenelUtility.DoubleToString(iskonto_2_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto2Yuzdesi => GenelUtility.DoubleToString(iskonto_2_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto2Adi => iskonto_2_adi, 
			GenelEvrakStatikAlanlar.Iskonto3Toplami => GenelUtility.DoubleToString(iskonto_3_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto3Yuzdesi => GenelUtility.DoubleToString(iskonto_3_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto3Adi => iskonto_3_adi, 
			GenelEvrakStatikAlanlar.Iskonto4Toplami => GenelUtility.DoubleToString(iskonto_4_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto4Yuzdesi => GenelUtility.DoubleToString(iskonto_4_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto4Adi => iskonto_4_adi, 
			GenelEvrakStatikAlanlar.Iskonto5Toplami => GenelUtility.DoubleToString(iskonto_5_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto5Yuzdesi => GenelUtility.DoubleToString(iskonto_5_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto5Adi => iskonto_5_adi, 
			GenelEvrakStatikAlanlar.Iskonto6Toplami => GenelUtility.DoubleToString(iskonto_6_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto6Yuzdesi => GenelUtility.DoubleToString(iskonto_6_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Iskonto6Adi => iskonto_6_adi, 
			GenelEvrakStatikAlanlar.Masraf1Toplami => GenelUtility.DoubleToString(masraf_1_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Masraf1Yuzdesi => GenelUtility.DoubleToString(masraf_1_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Masraf1Adi => masraf_1_adi, 
			GenelEvrakStatikAlanlar.Vergi1Toplami => GenelUtility.DoubleToString(vergi_1_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi1Matrahi => GenelUtility.DoubleToString(vergi_1_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi1Yuzdesi => GenelUtility.DoubleToString(vergi_1_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi1Adi => vergi_1_adi, 
			GenelEvrakStatikAlanlar.Vergi2Toplami => GenelUtility.DoubleToString(vergi_2_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi2Matrahi => GenelUtility.DoubleToString(vergi_2_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi2Yuzdesi => GenelUtility.DoubleToString(vergi_2_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi2Adi => vergi_2_adi, 
			GenelEvrakStatikAlanlar.Vergi3Toplami => GenelUtility.DoubleToString(vergi_3_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi3Matrahi => GenelUtility.DoubleToString(vergi_3_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi3Yuzdesi => GenelUtility.DoubleToString(vergi_3_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi3Adi => vergi_3_adi, 
			GenelEvrakStatikAlanlar.Vergi4Toplami => GenelUtility.DoubleToString(vergi_4_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi4Matrahi => GenelUtility.DoubleToString(vergi_4_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi4Yuzdesi => GenelUtility.DoubleToString(vergi_4_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi4Adi => vergi_4_adi, 
			GenelEvrakStatikAlanlar.Vergi5Toplami => GenelUtility.DoubleToString(vergi_5_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi5Matrahi => GenelUtility.DoubleToString(vergi_5_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi5Yuzdesi => GenelUtility.DoubleToString(vergi_5_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi5Adi => vergi_5_adi, 
			GenelEvrakStatikAlanlar.Vergi6Toplami => GenelUtility.DoubleToString(vergi_6_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi6Matrahi => GenelUtility.DoubleToString(vergi_6_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi6Yuzdesi => GenelUtility.DoubleToString(vergi_6_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi6Adi => vergi_6_adi, 
			GenelEvrakStatikAlanlar.Vergi7Toplami => GenelUtility.DoubleToString(vergi_7_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi7Matrahi => GenelUtility.DoubleToString(vergi_7_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi7Yuzdesi => GenelUtility.DoubleToString(vergi_7_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi7Adi => vergi_7_adi, 
			GenelEvrakStatikAlanlar.Vergi8Toplami => GenelUtility.DoubleToString(vergi_8_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi8Matrahi => GenelUtility.DoubleToString(vergi_8_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi8Yuzdesi => GenelUtility.DoubleToString(vergi_8_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi8Adi => vergi_8_adi, 
			GenelEvrakStatikAlanlar.Vergi9Toplami => GenelUtility.DoubleToString(vergi_9_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi9Matrahi => GenelUtility.DoubleToString(vergi_9_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi9Yuzdesi => GenelUtility.DoubleToString(vergi_9_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi9Adi => vergi_9_adi, 
			GenelEvrakStatikAlanlar.Vergi10Toplami => GenelUtility.DoubleToString(vergi_10_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi10Matrahi => GenelUtility.DoubleToString(vergi_10_matrahi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi10Yuzdesi => GenelUtility.DoubleToString(vergi_10_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Vergi10Adi => vergi_10_adi, 
			GenelEvrakStatikAlanlar.Miktar1Toplami => GenelUtility.DoubleToString(miktar_1_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Miktar2Toplami => GenelUtility.DoubleToString(miktar_2_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Miktar3Toplami => GenelUtility.DoubleToString(miktar_3_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Miktar4Toplami => GenelUtility.DoubleToString(miktar_4_toplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.SevkAdresiSokak => sevk_adresi.adr_sokak, 
			GenelEvrakStatikAlanlar.SevkAdresiCadde => sevk_adresi.adr_cadde, 
			GenelEvrakStatikAlanlar.SevkAdresiSokakCadde => sevk_adresi.adr_sokak + " " + sevk_adresi.adr_cadde, 
			GenelEvrakStatikAlanlar.SevkAdresiPostaKodu => sevk_adresi.adr_posta_kodu, 
			GenelEvrakStatikAlanlar.SevkAdresiIlce => sevk_adresi.adr_ilce, 
			GenelEvrakStatikAlanlar.SevkAdresiIl => sevk_adresi.adr_il, 
			GenelEvrakStatikAlanlar.SevkAdresiPostaKoduIlceIl => sevk_adresi.adr_posta_kodu + " " + sevk_adresi.adr_ilce + " " + sevk_adresi.adr_il, 
			GenelEvrakStatikAlanlar.SevkAdresiIlceIl => sevk_adresi.adr_ilce + " " + sevk_adresi.adr_il, 
			GenelEvrakStatikAlanlar.SevkAdresiUlke => sevk_adresi.adr_ulke, 
			GenelEvrakStatikAlanlar.SevkAdresiTelefon1 => sevk_adresi.adr_tel_no1, 
			GenelEvrakStatikAlanlar.SevkAdresiTelefon2 => sevk_adresi.adr_tel_no2, 
			GenelEvrakStatikAlanlar.FaturaAdresiSokak => fatura_adresi.adr_sokak, 
			GenelEvrakStatikAlanlar.FaturaAdresiCadde => fatura_adresi.adr_cadde, 
			GenelEvrakStatikAlanlar.FaturaAdresiSokakCadde => fatura_adresi.adr_sokak + " " + fatura_adresi.adr_cadde, 
			GenelEvrakStatikAlanlar.FaturaAdresiPostaKodu => fatura_adresi.adr_posta_kodu, 
			GenelEvrakStatikAlanlar.FaturaAdresiIlce => fatura_adresi.adr_ilce, 
			GenelEvrakStatikAlanlar.FaturaAdresiIl => fatura_adresi.adr_il, 
			GenelEvrakStatikAlanlar.FaturaAdresiPostaKoduIlceIl => fatura_adresi.adr_posta_kodu + " " + fatura_adresi.adr_ilce + " " + fatura_adresi.adr_il, 
			GenelEvrakStatikAlanlar.FaturaAdresiIlceIl => fatura_adresi.adr_ilce + " " + fatura_adresi.adr_il, 
			GenelEvrakStatikAlanlar.FaturaAdresiUlke => fatura_adresi.adr_ulke, 
			GenelEvrakStatikAlanlar.FaturaAdresiTelefon1 => fatura_adresi.adr_tel_no1, 
			GenelEvrakStatikAlanlar.FaturaAdresiTelefon2 => fatura_adresi.adr_tel_no2, 
			GenelEvrakStatikAlanlar.ToplamVergiMatrahi => GenelUtility.DoubleToString(ara_toplam - toplam_iskonto_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.OncekiBakiye => GenelUtility.DoubleToString(onceki_bakiye, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.SimdikiBakiye => GenelUtility.DoubleToString(simdiki_bakiye, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakStatikAlanlar.Miktar2BagimsizBirimToplami => GenelUtility.DoubleToString(miktar2bagimsizbirimtoplami, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			_ => genelEvrakStatikAlanlar.ToString(), 
		};
	}

	private string DinamikAlanVeriGetir(GenelEvrakSatirlari satir, string veristring, string binlik_ayraci, string ondalik_ayraci, int ondalik_hane_sayisi, bool sonuna_para_birimi_ekle, bool basina_para_birimi_ekle, DovizCinsiTanimlari doviz_cinsi_tanimlari, List<VergiTanimi> vergitanimlari)
	{
		GenelEvrakDinamikAlanlar genelEvrakDinamikAlanlar = (GenelEvrakDinamikAlanlar)int.Parse(veristring);
		return genelEvrakDinamikAlanlar switch
		{
			GenelEvrakDinamikAlanlar.Vade => satir.Vade.ToString("dd.MM.yyyy"), 
			GenelEvrakDinamikAlanlar.SatirCinsi => satir.satir_cinsi switch
			{
				GenelEvrakSatirCinsleri.Cek => "ÇEK", 
				GenelEvrakSatirCinsleri.Hizmet => "HİZMET", 
				GenelEvrakSatirCinsleri.Nakit => "NAKİT", 
				GenelEvrakSatirCinsleri.Stok => "STOK", 
				GenelEvrakSatirCinsleri.KrediKarti => "KREDİ KARTI", 
				GenelEvrakSatirCinsleri.Senet => "SENET", 
				_ => "TANIMSIZ", 
			}, 
			GenelEvrakDinamikAlanlar.SatirCinsiKisa => satir.satir_cinsi switch
			{
				GenelEvrakSatirCinsleri.Cek => "Ç", 
				GenelEvrakSatirCinsleri.Hizmet => "H", 
				GenelEvrakSatirCinsleri.Nakit => "N", 
				GenelEvrakSatirCinsleri.Stok => "S", 
				GenelEvrakSatirCinsleri.KrediKarti => "KK", 
				GenelEvrakSatirCinsleri.Senet => "SN", 
				_ => "T", 
			}, 
			GenelEvrakDinamikAlanlar.HesapKodu => satir.hesap_kodu, 
			GenelEvrakDinamikAlanlar.HesapAdi => satir.hesap_adi, 
			GenelEvrakDinamikAlanlar.HesapYabanciAdi => satir.hesap_yabanci_adi, 
			GenelEvrakDinamikAlanlar.HesapKisaAdi => satir.hesap_kisa_adi, 
			GenelEvrakDinamikAlanlar.BedenKodu => satir.beden_kodu, 
			GenelEvrakDinamikAlanlar.BedenAdi => satir.beden_adi, 
			GenelEvrakDinamikAlanlar.RenkKodu => satir.renk_kodu, 
			GenelEvrakDinamikAlanlar.RenkAdi => satir.renk_adi, 
			GenelEvrakDinamikAlanlar.AltGrupKodu => satir.altgrup_kod, 
			GenelEvrakDinamikAlanlar.AltGrupAdi => satir.altgrup_adi, 
			GenelEvrakDinamikAlanlar.AnaGrupKodu => satir.anagrup_kod, 
			GenelEvrakDinamikAlanlar.AnaGrupAdi => satir.anagrup_adi, 
			GenelEvrakDinamikAlanlar.SektorKodu => satir.sektor_kodu, 
			GenelEvrakDinamikAlanlar.SektorAdi => satir.sektor_adi, 
			GenelEvrakDinamikAlanlar.MarkaKodu => satir.marka_kodu, 
			GenelEvrakDinamikAlanlar.MarkaAdi => satir.marka_adi, 
			GenelEvrakDinamikAlanlar.ModelKodu => satir.model_kodu, 
			GenelEvrakDinamikAlanlar.ModelAdi => satir.model_adi, 
			GenelEvrakDinamikAlanlar.UreticiKodu => satir.uretici_kodu, 
			GenelEvrakDinamikAlanlar.UreticiAdi => satir.uretici_adi, 
			GenelEvrakDinamikAlanlar.ReyonKodu => satir.reyon_kodu, 
			GenelEvrakDinamikAlanlar.ReyonAdi => satir.reyon_adi, 
			GenelEvrakDinamikAlanlar.YerKodu => satir.yer_kodu, 
			GenelEvrakDinamikAlanlar.PartiKodu => satir.parti_kodu, 
			GenelEvrakDinamikAlanlar.SatirAciklama => satir.satir_aciklama, 
			GenelEvrakDinamikAlanlar.SatirAciklama2 => satir.satir_aciklama2, 
			GenelEvrakDinamikAlanlar.DovizCinsiAdi => doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_adi, 
			GenelEvrakDinamikAlanlar.DovizCinsiSembol => doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol, 
			GenelEvrakDinamikAlanlar.Kur => GenelUtility.DoubleToString(satir.kur.dov_fiyat, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.BirimFiyatBrut => GenelUtility.DoubleToString(satir.birim_fiyat_brut, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Miktar => GenelUtility.DoubleToString(satir.miktar, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Miktar2 => GenelUtility.DoubleToString(satir.miktar2, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Tutar => GenelUtility.DoubleToString(satir.tutar, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.CariSorumlulukMerkeziAdi => satir.cari_sorumluluk_merkezi.som_isim, 
			GenelEvrakDinamikAlanlar.CariSorumlulukMerkeziKodu => satir.cari_sorumluluk_merkezi.som_kod, 
			GenelEvrakDinamikAlanlar.StokSorumlulukMerkeziAdi => satir.stok_sorumluluk_merkezi.som_isim, 
			GenelEvrakDinamikAlanlar.StokSorumlulukMerkeziKodu => satir.stok_sorumluluk_merkezi.som_kod, 
			GenelEvrakDinamikAlanlar.KarsiSorumlulukMerkeziAdi => satir.karsi_sorumluluk_merkezi.som_isim, 
			GenelEvrakDinamikAlanlar.KarsiSorumlulukMerkeziKodu => satir.karsi_sorumluluk_merkezi.som_kod, 
			GenelEvrakDinamikAlanlar.ProjeKodu => satir.proje.pro_kodu, 
			GenelEvrakDinamikAlanlar.ProjeAdi => satir.proje.pro_adi, 
			GenelEvrakDinamikAlanlar.Iskonto1Tutari => GenelUtility.DoubleToString(satir.iskonto_1_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto2Tutari => GenelUtility.DoubleToString(satir.iskonto_2_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto3Tutari => GenelUtility.DoubleToString(satir.iskonto_3_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto4Tutari => GenelUtility.DoubleToString(satir.iskonto_4_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto5Tutari => GenelUtility.DoubleToString(satir.iskonto_5_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto6Tutari => GenelUtility.DoubleToString(satir.iskonto_6_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Masraf1Tutari => GenelUtility.DoubleToString(satir.masraf_1_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Masraf2Tutari => GenelUtility.DoubleToString(satir.masraf_2_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Masraf3Tutari => GenelUtility.DoubleToString(satir.masraf_3_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Masraf4Tutari => GenelUtility.DoubleToString(satir.masraf_4_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.VergiTutari => GenelUtility.DoubleToString(satir.vergi_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.MasrafVergiTutari => GenelUtility.DoubleToString(satir.masraf_vergi_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.OtvVergi => GenelUtility.DoubleToString(satir.otv_vergi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.OivVergi => GenelUtility.DoubleToString(satir.oiv_vergi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim1Adi => satir.birim_1_adi, 
			GenelEvrakDinamikAlanlar.Birim1FiyatBrut => GenelUtility.DoubleToString(satir.birim_1_fiyat_brut, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim1FiyatNet => GenelUtility.DoubleToString(satir.birim_1_fiyat_net, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim1FiyatNetKdvDahil => GenelUtility.DoubleToString(satir.birim_1_fiyat_net_kdv_dahil, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim1Miktar => GenelUtility.DoubleToString(satir.birim_1_miktar, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim2Adi => satir.birim_2_adi, 
			GenelEvrakDinamikAlanlar.Birim2FiyatBrut => GenelUtility.DoubleToString(satir.birim_2_fiyat_brut, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim2FiyatNet => GenelUtility.DoubleToString(satir.birim_2_fiyat_net, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim2FiyatNetKdvDahil => GenelUtility.DoubleToString(satir.birim_2_fiyat_net_kdv_dahil, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim2Miktar => GenelUtility.DoubleToString(satir.birim_2_miktar, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim3Adi => satir.birim_3_adi, 
			GenelEvrakDinamikAlanlar.Birim3FiyatBrut => GenelUtility.DoubleToString(satir.birim_3_fiyat_brut, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim3FiyatNet => GenelUtility.DoubleToString(satir.birim_3_fiyat_net, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim3FiyatNetKdvDahil => GenelUtility.DoubleToString(satir.birim_3_fiyat_net_kdv_dahil, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim3Miktar => GenelUtility.DoubleToString(satir.birim_3_miktar, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim4Adi => satir.birim_4_adi, 
			GenelEvrakDinamikAlanlar.Birim4FiyatBrut => GenelUtility.DoubleToString(satir.birim_4_fiyat_brut, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim4FiyatNet => GenelUtility.DoubleToString(satir.birim_4_fiyat_net, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim4FiyatNetKdvDahil => GenelUtility.DoubleToString(satir.birim_4_fiyat_net_kdv_dahil, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Birim4Miktar => GenelUtility.DoubleToString(satir.birim_4_miktar, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.SatisBirimiAdi => satir.satis_birimi_adi, 
			GenelEvrakDinamikAlanlar.SatisBirimiFiyatBrut => GenelUtility.DoubleToString(satir.satis_birimi_fiyat_brut, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.SatisBirimiFiyatNet => GenelUtility.DoubleToString(satir.satis_birimi_fiyat_net, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.SatisBirimiFiyatNetKdvDahil => GenelUtility.DoubleToString(satir.satis_birimi_fiyat_net_kdv_dahil, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.SatisBirimiMiktar => GenelUtility.DoubleToString(satir.satis_birimi_miktar, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamFiyatBrut => GenelUtility.DoubleToString(satir.toplam_fiyat_brut, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamFiyatNet => GenelUtility.DoubleToString(satir.toplam_fiyat_net, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamFiyatNetKdvDahil => GenelUtility.DoubleToString(satir.toplam_fiyat_net + satir.toplam_vergi_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamIskontoTutari => GenelUtility.DoubleToString(satir.toplam_iskonto_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamIskontoYuzdesi => GenelUtility.DoubleToString(satir.toplam_iskonto_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamMasrafTutari => GenelUtility.DoubleToString(satir.toplam_masraf_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamMasrafYuzdesi => GenelUtility.DoubleToString(satir.toplam_masraf_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamVergiTutari => GenelUtility.DoubleToString(satir.toplam_vergi_tutari, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.ToplamVergiYuzdesi => GenelUtility.DoubleToString(satir.toplam_vergi_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Yekun => GenelUtility.DoubleToString(satir.yekun, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto1Yuzdesi => GenelUtility.DoubleToString(satir.iskonto_1_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto2Yuzdesi => GenelUtility.DoubleToString(satir.iskonto_2_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto3Yuzdesi => GenelUtility.DoubleToString(satir.iskonto_3_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto4Yuzdesi => GenelUtility.DoubleToString(satir.iskonto_4_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto5Yuzdesi => GenelUtility.DoubleToString(satir.iskonto_5_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.Iskonto6Yuzdesi => GenelUtility.DoubleToString(satir.iskonto_6_yuzdesi, binlik_ayraci, ondalik_ayraci, ondalik_hane_sayisi, sonuna_para_birimi_ekle, basina_para_birimi_ekle, doviz_cinsi_tanimlari.GetDovizCinsiTanimi(doviz_cinsi).Kur_sembol), 
			GenelEvrakDinamikAlanlar.VergiAdi => satir.vergi_adi, 
			GenelEvrakDinamikAlanlar.ToplamVergiYuzdesiKarttan => vergitanimlari[satir.vergi_pntr].Yuzde.ToString(), 
			GenelEvrakDinamikAlanlar.BarkodBirim1 => satir.birim1_barkod, 
			GenelEvrakDinamikAlanlar.BarkodBirim2 => satir.birim2_barkod, 
			GenelEvrakDinamikAlanlar.BarkodBirim3 => satir.birim3_barkod, 
			GenelEvrakDinamikAlanlar.BarkodBirim4 => satir.birim4_barkod, 
			GenelEvrakDinamikAlanlar.BarkodSatisBirimi => satir.barkod_satis_birimi, 
			_ => genelEvrakDinamikAlanlar.ToString(), 
		};
	}
}
