using System;
using System.IO;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Projeler;
using Fora.Mikro.SorumlulukMerkezleri;

namespace Fora.Mikro.Evraklar;

public class GenelEvrakSatirlari
{
	public GenelEvrakSatirCinsleri satir_cinsi { get; set; }

	public string hesap_kodu { get; set; }

	public string hesap_adi { get; set; }

	public string hesap_yabanci_adi { get; set; }

	public string hesap_kisa_adi { get; set; }

	public string beden_kodu { get; set; }

	public string beden_adi { get; set; }

	public string renk_kodu { get; set; }

	public string renk_adi { get; set; }

	public string altgrup_kod { get; set; }

	public string altgrup_adi { get; set; }

	public string anagrup_kod { get; set; }

	public string anagrup_adi { get; set; }

	public string sektor_kodu { get; set; }

	public string sektor_adi { get; set; }

	public string marka_kodu { get; set; }

	public string marka_adi { get; set; }

	public string model_kodu { get; set; }

	public string model_adi { get; set; }

	public string uretici_kodu { get; set; }

	public string uretici_adi { get; set; }

	public string reyon_kodu { get; set; }

	public string reyon_adi { get; set; }

	public string yer_kodu { get; set; }

	public string parti_kodu { get; set; }

	public string satir_aciklama { get; set; }

	public string satir_aciklama2 { get; set; }

	public int doviz_cinsi { get; set; }

	public Kur kur { get; set; }

	public DateTime sevk_teslim_tarihi { get; set; }

	public double birim_fiyat_brut { get; set; }

	public int birim_pntr { get; set; }

	public double miktar { get; set; }

	public double miktar2 { get; set; }

	public double tutar { get; set; }

	public SorumlulukMerkezi cari_sorumluluk_merkezi { get; set; }

	public SorumlulukMerkezi stok_sorumluluk_merkezi { get; set; }

	public SorumlulukMerkezi karsi_sorumluluk_merkezi { get; set; }

	public Proje proje { get; set; }

	public int iskonto_1_uygulama_sekli { get; set; }

	public double iskonto_1_tutari { get; set; }

	public int iskonto_2_uygulama_sekli { get; set; }

	public double iskonto_2_tutari { get; set; }

	public int iskonto_3_uygulama_sekli { get; set; }

	public double iskonto_3_tutari { get; set; }

	public int iskonto_4_uygulama_sekli { get; set; }

	public double iskonto_4_tutari { get; set; }

	public int iskonto_5_uygulama_sekli { get; set; }

	public double iskonto_5_tutari { get; set; }

	public int iskonto_6_uygulama_sekli { get; set; }

	public double iskonto_6_tutari { get; set; }

	public int masraf_1_uygulama_sekli { get; set; }

	public double masraf_1_tutari { get; set; }

	public int masraf_2_uygulama_sekli { get; set; }

	public double masraf_2_tutari { get; set; }

	public int masraf_3_uygulama_sekli { get; set; }

	public double masraf_3_tutari { get; set; }

	public int masraf_4_uygulama_sekli { get; set; }

	public double masraf_4_tutari { get; set; }

	public int vergi_pntr { get; set; }

	public double vergi_tutari { get; set; }

	public int masraf_vergi_pntr { get; set; }

	public double masraf_vergi_tutari { get; set; }

	public int otv_pntr { get; set; }

	public double otv_vergi { get; set; }

	public int oiv_pntr { get; set; }

	public double oiv_vergi { get; set; }

	public string birim1_ad { get; set; }

	public string birim2_ad { get; set; }

	public string birim3_ad { get; set; }

	public string birim4_ad { get; set; }

	public double birim1_katsayi { get; set; }

	public double birim2_katsayi { get; set; }

	public double birim3_katsayi { get; set; }

	public double birim4_katsayi { get; set; }

	public string birim1_barkod { get; set; }

	public string birim2_barkod { get; set; }

	public string birim3_barkod { get; set; }

	public string birim4_barkod { get; set; }

	public DateTime Vade { get; set; }

	public string birim_1_adi => birim1_ad;

	public double birim_1_fiyat_brut => birim_fiyat_brut;

	public double birim_1_fiyat_net
	{
		get
		{
			double num = iskonto_1_tutari + iskonto_2_tutari + iskonto_3_tutari + iskonto_4_tutari + iskonto_5_tutari + iskonto_6_tutari;
			double num2 = masraf_1_tutari + masraf_2_tutari + masraf_3_tutari + masraf_4_tutari;
			return birim_fiyat_brut - num / miktar + num2 / miktar;
		}
	}

	public double birim_1_fiyat_net_kdv_dahil => birim_1_fiyat_net + (vergi_tutari + masraf_vergi_tutari) / miktar;

	public double birim_1_miktar => miktar;

	public string birim_2_adi => birim2_ad;

	public double birim_2_fiyat_brut
	{
		get
		{
			if (birim2_katsayi > 0.0)
			{
				return birim_fiyat_brut / Math.Abs(birim2_katsayi);
			}
			return birim_fiyat_brut * Math.Abs(birim2_katsayi);
		}
	}

	public double birim_2_fiyat_net
	{
		get
		{
			if (birim2_katsayi > 0.0)
			{
				return birim_1_fiyat_net / Math.Abs(birim2_katsayi);
			}
			return birim_1_fiyat_net * Math.Abs(birim2_katsayi);
		}
	}

	public double birim_2_fiyat_net_kdv_dahil
	{
		get
		{
			if (birim2_katsayi > 0.0)
			{
				return birim_1_fiyat_net_kdv_dahil / Math.Abs(birim2_katsayi);
			}
			return birim_1_fiyat_net_kdv_dahil * Math.Abs(birim2_katsayi);
		}
	}

	public double birim_2_miktar
	{
		get
		{
			if (birim2_katsayi < 0.0)
			{
				return birim_1_miktar / Math.Abs(birim2_katsayi);
			}
			return birim_1_miktar * Math.Abs(birim2_katsayi);
		}
	}

	public string birim_3_adi => birim3_ad;

	public double birim_3_fiyat_brut
	{
		get
		{
			if (birim3_katsayi > 0.0)
			{
				return birim_fiyat_brut / Math.Abs(birim3_katsayi);
			}
			return birim_fiyat_brut * Math.Abs(birim3_katsayi);
		}
	}

	public double birim_3_fiyat_net
	{
		get
		{
			if (birim3_katsayi > 0.0)
			{
				return birim_1_fiyat_net / Math.Abs(birim3_katsayi);
			}
			return birim_1_fiyat_net * Math.Abs(birim3_katsayi);
		}
	}

	public double birim_3_fiyat_net_kdv_dahil
	{
		get
		{
			if (birim3_katsayi > 0.0)
			{
				return birim_1_fiyat_net_kdv_dahil / Math.Abs(birim3_katsayi);
			}
			return birim_1_fiyat_net_kdv_dahil * Math.Abs(birim3_katsayi);
		}
	}

	public double birim_3_miktar
	{
		get
		{
			if (birim3_katsayi < 0.0)
			{
				return birim_1_miktar / Math.Abs(birim3_katsayi);
			}
			return birim_1_miktar * Math.Abs(birim3_katsayi);
		}
	}

	public string birim_4_adi => birim4_ad;

	public double birim_4_fiyat_brut
	{
		get
		{
			if (birim4_katsayi > 0.0)
			{
				return birim_fiyat_brut / Math.Abs(birim4_katsayi);
			}
			return birim_fiyat_brut * Math.Abs(birim4_katsayi);
		}
	}

	public double birim_4_fiyat_net
	{
		get
		{
			if (birim3_katsayi > 0.0)
			{
				return birim_1_fiyat_net / Math.Abs(birim4_katsayi);
			}
			return birim_1_fiyat_net * Math.Abs(birim4_katsayi);
		}
	}

	public double birim_4_fiyat_net_kdv_dahil
	{
		get
		{
			if (birim4_katsayi > 0.0)
			{
				return birim_1_fiyat_net_kdv_dahil / Math.Abs(birim4_katsayi);
			}
			return birim_1_fiyat_net_kdv_dahil * Math.Abs(birim4_katsayi);
		}
	}

	public double birim_4_miktar
	{
		get
		{
			if (birim4_katsayi < 0.0)
			{
				return birim_1_miktar / Math.Abs(birim4_katsayi);
			}
			return birim_1_miktar * Math.Abs(birim4_katsayi);
		}
	}

	public string satis_birimi_adi => birim_pntr switch
	{
		1 => birim_1_adi, 
		2 => birim_2_adi, 
		3 => birim_3_adi, 
		4 => birim_4_adi, 
		_ => birim_1_adi, 
	};

	public double satis_birimi_fiyat_brut => birim_pntr switch
	{
		1 => birim_1_fiyat_brut, 
		2 => birim_2_fiyat_brut, 
		3 => birim_3_fiyat_brut, 
		4 => birim_4_fiyat_brut, 
		_ => birim_1_fiyat_brut, 
	};

	public double satis_birimi_fiyat_net => birim_pntr switch
	{
		1 => birim_1_fiyat_net, 
		2 => birim_2_fiyat_net, 
		3 => birim_3_fiyat_net, 
		4 => birim_4_fiyat_net, 
		_ => birim_1_fiyat_net, 
	};

	public double satis_birimi_fiyat_net_kdv_dahil => birim_pntr switch
	{
		1 => birim_1_fiyat_net_kdv_dahil, 
		2 => birim_2_fiyat_net_kdv_dahil, 
		3 => birim_3_fiyat_net_kdv_dahil, 
		4 => birim_4_fiyat_net_kdv_dahil, 
		_ => birim_1_fiyat_net_kdv_dahil, 
	};

	public double satis_birimi_miktar => birim_pntr switch
	{
		1 => birim_1_miktar, 
		2 => birim_2_miktar, 
		3 => birim_3_miktar, 
		4 => birim_4_miktar, 
		_ => birim_1_miktar, 
	};

	public double toplam_fiyat_brut => birim_fiyat_brut * miktar;

	public double toplam_iskonto_tutari => iskonto_1_tutari + iskonto_2_tutari + iskonto_3_tutari + iskonto_4_tutari + iskonto_5_tutari + iskonto_6_tutari;

	public double toplam_iskonto_yuzdesi
	{
		get
		{
			if (toplam_iskonto_tutari == 0.0)
			{
				return 0.0;
			}
			return (1.0 - (toplam_fiyat_brut - toplam_iskonto_tutari) / toplam_fiyat_brut) * 100.0;
		}
	}

	public double toplam_masraf_tutari => masraf_1_tutari + masraf_2_tutari + masraf_3_tutari + masraf_4_tutari;

	public double toplam_masraf_yuzdesi
	{
		get
		{
			if (toplam_masraf_tutari == 0.0)
			{
				return 0.0;
			}
			return (1.0 - (toplam_fiyat_brut - toplam_masraf_tutari) / toplam_fiyat_brut) * 100.0;
		}
	}

	public double toplam_vergi_tutari => vergi_tutari + masraf_vergi_tutari;

	public double toplam_vergi_yuzdesi
	{
		get
		{
			if (birim_1_fiyat_net_kdv_dahil == 0.0)
			{
				return 0.0;
			}
			double num = 100.0;
			return (1.0 - birim_1_fiyat_net_kdv_dahil / birim_1_fiyat_net) * num * -1.0;
		}
	}

	public double toplam_fiyat_net => birim_1_fiyat_net * miktar;

	public double yekun => birim_1_fiyat_net_kdv_dahil * miktar;

	public double iskonto_1_yuzdesi
	{
		get
		{
			if (iskonto_1_tutari == 0.0)
			{
				return 0.0;
			}
			return (1.0 - (toplam_fiyat_brut - iskonto_1_tutari) / toplam_fiyat_brut) * 100.0;
		}
	}

	public double iskonto_2_yuzdesi
	{
		get
		{
			if (iskonto_2_tutari == 0.0)
			{
				return 0.0;
			}
			double num = toplam_fiyat_brut;
			if (iskonto_2_uygulama_sekli == 1)
			{
				num -= iskonto_1_tutari;
			}
			return (1.0 - (num - iskonto_2_tutari) / num) * 100.0;
		}
	}

	public double iskonto_3_yuzdesi
	{
		get
		{
			if (iskonto_3_tutari == 0.0)
			{
				return 0.0;
			}
			double num = toplam_fiyat_brut;
			if (iskonto_3_uygulama_sekli == 1)
			{
				num = num - iskonto_1_tutari - iskonto_2_tutari;
			}
			return (1.0 - (num - iskonto_3_tutari) / num) * 100.0;
		}
	}

	public double iskonto_4_yuzdesi
	{
		get
		{
			if (iskonto_4_tutari == 0.0)
			{
				return 0.0;
			}
			double num = toplam_fiyat_brut;
			if (iskonto_4_uygulama_sekli == 1)
			{
				num = num - iskonto_1_tutari - iskonto_2_tutari - iskonto_3_tutari;
			}
			return (1.0 - (num - iskonto_4_tutari) / num) * 100.0;
		}
	}

	public double iskonto_5_yuzdesi
	{
		get
		{
			if (iskonto_5_tutari == 0.0)
			{
				return 0.0;
			}
			double num = toplam_fiyat_brut;
			if (iskonto_5_uygulama_sekli == 1)
			{
				num = num - iskonto_1_tutari - iskonto_2_tutari - iskonto_3_tutari - iskonto_4_tutari;
			}
			return (1.0 - (num - iskonto_5_tutari) / num) * 100.0;
		}
	}

	public double iskonto_6_yuzdesi
	{
		get
		{
			if (iskonto_6_tutari == 0.0)
			{
				return 0.0;
			}
			double num = toplam_fiyat_brut;
			if (iskonto_6_uygulama_sekli == 1)
			{
				num = num - iskonto_1_tutari - iskonto_2_tutari - iskonto_3_tutari - iskonto_4_tutari - iskonto_5_tutari;
			}
			return (1.0 - (num - iskonto_6_tutari) / num) * 100.0;
		}
	}

	public string barkod_satis_birimi => birim_pntr switch
	{
		1 => birim1_barkod, 
		2 => birim2_barkod, 
		3 => birim3_barkod, 
		4 => birim4_barkod, 
		_ => birim1_barkod, 
	};

	public string vergi_adi => "";

	public GenelEvrakSatirlari()
	{
		satir_cinsi = GenelEvrakSatirCinsleri.Stok;
		hesap_kodu = "";
		hesap_adi = "";
		hesap_yabanci_adi = "";
		hesap_kisa_adi = "";
		beden_kodu = "";
		beden_adi = "";
		renk_kodu = "";
		renk_adi = "";
		altgrup_kod = "";
		altgrup_adi = "";
		anagrup_kod = "";
		anagrup_adi = "";
		sektor_kodu = "";
		sektor_adi = "";
		marka_kodu = "";
		marka_adi = "";
		model_kodu = "";
		model_adi = "";
		uretici_kodu = "";
		uretici_adi = "";
		reyon_kodu = "";
		reyon_adi = "";
		yer_kodu = "";
		parti_kodu = "";
		satir_aciklama = "";
		satir_aciklama2 = "";
		birim1_ad = "";
		birim2_ad = "";
		birim3_ad = "";
		birim4_ad = "";
		birim1_barkod = "";
		birim2_barkod = "";
		birim3_barkod = "";
		birim4_barkod = "";
		kur = new Kur();
		cari_sorumluluk_merkezi = new SorumlulukMerkezi();
		stok_sorumluluk_merkezi = new SorumlulukMerkezi();
		karsi_sorumluluk_merkezi = new SorumlulukMerkezi();
		proje = new Proje();
		Vade = default(DateTime);
	}

	public static byte[] WriteToByteArray(GenelEvrakSatirlari toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(GenelEvrakSatirlari toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(GenelEvrakSatirlari toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.satir_cinsi);
		writer.Write(toWrite.hesap_kodu);
		writer.Write(toWrite.hesap_adi);
		writer.Write(toWrite.hesap_yabanci_adi);
		writer.Write(toWrite.hesap_kisa_adi);
		writer.Write(toWrite.beden_kodu);
		writer.Write(toWrite.beden_adi);
		writer.Write(toWrite.renk_kodu);
		writer.Write(toWrite.renk_adi);
		writer.Write(toWrite.altgrup_kod);
		writer.Write(toWrite.altgrup_adi);
		writer.Write(toWrite.anagrup_kod);
		writer.Write(toWrite.anagrup_adi);
		writer.Write(toWrite.sektor_kodu);
		writer.Write(toWrite.sektor_adi);
		writer.Write(toWrite.marka_kodu);
		writer.Write(toWrite.marka_adi);
		writer.Write(toWrite.model_kodu);
		writer.Write(toWrite.model_adi);
		writer.Write(toWrite.uretici_kodu);
		writer.Write(toWrite.uretici_adi);
		writer.Write(toWrite.reyon_kodu);
		writer.Write(toWrite.reyon_adi);
		writer.Write(toWrite.yer_kodu);
		writer.Write(toWrite.parti_kodu);
		writer.Write(toWrite.satir_aciklama);
		writer.Write(toWrite.satir_aciklama2);
		writer.Write(toWrite.doviz_cinsi);
		Kur.WriteToBinaryWriter(toWrite.kur, writer);
		writer.Write(toWrite.sevk_teslim_tarihi.Ticks);
		writer.Write(toWrite.birim_fiyat_brut);
		writer.Write(toWrite.birim_pntr);
		writer.Write(toWrite.miktar);
		writer.Write(toWrite.miktar2);
		writer.Write(toWrite.tutar);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.cari_sorumluluk_merkezi, writer);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.stok_sorumluluk_merkezi, writer);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.karsi_sorumluluk_merkezi, writer);
		Proje.WriteToBinaryWriter(toWrite.proje, writer);
		writer.Write(toWrite.iskonto_1_uygulama_sekli);
		writer.Write(toWrite.iskonto_1_tutari);
		writer.Write(toWrite.iskonto_2_uygulama_sekli);
		writer.Write(toWrite.iskonto_2_tutari);
		writer.Write(toWrite.iskonto_3_uygulama_sekli);
		writer.Write(toWrite.iskonto_3_tutari);
		writer.Write(toWrite.iskonto_4_uygulama_sekli);
		writer.Write(toWrite.iskonto_4_tutari);
		writer.Write(toWrite.iskonto_5_uygulama_sekli);
		writer.Write(toWrite.iskonto_5_tutari);
		writer.Write(toWrite.iskonto_6_uygulama_sekli);
		writer.Write(toWrite.iskonto_6_tutari);
		writer.Write(toWrite.masraf_1_uygulama_sekli);
		writer.Write(toWrite.masraf_1_tutari);
		writer.Write(toWrite.masraf_2_uygulama_sekli);
		writer.Write(toWrite.masraf_2_tutari);
		writer.Write(toWrite.masraf_3_uygulama_sekli);
		writer.Write(toWrite.masraf_3_tutari);
		writer.Write(toWrite.masraf_4_uygulama_sekli);
		writer.Write(toWrite.masraf_4_tutari);
		writer.Write(toWrite.vergi_pntr);
		writer.Write(toWrite.vergi_tutari);
		writer.Write(toWrite.masraf_vergi_pntr);
		writer.Write(toWrite.masraf_vergi_tutari);
		writer.Write(toWrite.otv_pntr);
		writer.Write(toWrite.otv_vergi);
		writer.Write(toWrite.oiv_pntr);
		writer.Write(toWrite.oiv_vergi);
		writer.Write(toWrite.birim1_ad);
		writer.Write(toWrite.birim2_ad);
		writer.Write(toWrite.birim3_ad);
		writer.Write(toWrite.birim4_ad);
		writer.Write(toWrite.birim1_katsayi);
		writer.Write(toWrite.birim2_katsayi);
		writer.Write(toWrite.birim3_katsayi);
		writer.Write(toWrite.birim4_katsayi);
		writer.Write(toWrite.birim1_barkod);
		writer.Write(toWrite.birim2_barkod);
		writer.Write(toWrite.birim3_barkod);
		writer.Write(toWrite.birim4_barkod);
		writer.Write(toWrite.Vade.Ticks);
	}

	public static GenelEvrakSatirlari ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		GenelEvrakSatirlari result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static GenelEvrakSatirlari ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static GenelEvrakSatirlari ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new GenelEvrakSatirlari
		{
			satir_cinsi = (GenelEvrakSatirCinsleri)reader.ReadInt32(),
			hesap_kodu = reader.ReadString(),
			hesap_adi = reader.ReadString(),
			hesap_yabanci_adi = reader.ReadString(),
			hesap_kisa_adi = reader.ReadString(),
			beden_kodu = reader.ReadString(),
			beden_adi = reader.ReadString(),
			renk_kodu = reader.ReadString(),
			renk_adi = reader.ReadString(),
			altgrup_kod = reader.ReadString(),
			altgrup_adi = reader.ReadString(),
			anagrup_kod = reader.ReadString(),
			anagrup_adi = reader.ReadString(),
			sektor_kodu = reader.ReadString(),
			sektor_adi = reader.ReadString(),
			marka_kodu = reader.ReadString(),
			marka_adi = reader.ReadString(),
			model_kodu = reader.ReadString(),
			model_adi = reader.ReadString(),
			uretici_kodu = reader.ReadString(),
			uretici_adi = reader.ReadString(),
			reyon_kodu = reader.ReadString(),
			reyon_adi = reader.ReadString(),
			yer_kodu = reader.ReadString(),
			parti_kodu = reader.ReadString(),
			satir_aciklama = reader.ReadString(),
			satir_aciklama2 = reader.ReadString(),
			doviz_cinsi = reader.ReadInt32(),
			kur = Kur.ReadFromBinaryReader(reader),
			sevk_teslim_tarihi = new DateTime(reader.ReadInt64()),
			birim_fiyat_brut = reader.ReadDouble(),
			birim_pntr = reader.ReadInt32(),
			miktar = reader.ReadDouble(),
			miktar2 = reader.ReadDouble(),
			tutar = reader.ReadDouble(),
			cari_sorumluluk_merkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader),
			stok_sorumluluk_merkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader),
			karsi_sorumluluk_merkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader),
			proje = Proje.ReadFromBinaryReader(reader),
			iskonto_1_uygulama_sekli = reader.ReadInt32(),
			iskonto_1_tutari = reader.ReadDouble(),
			iskonto_2_uygulama_sekli = reader.ReadInt32(),
			iskonto_2_tutari = reader.ReadDouble(),
			iskonto_3_uygulama_sekli = reader.ReadInt32(),
			iskonto_3_tutari = reader.ReadDouble(),
			iskonto_4_uygulama_sekli = reader.ReadInt32(),
			iskonto_4_tutari = reader.ReadDouble(),
			iskonto_5_uygulama_sekli = reader.ReadInt32(),
			iskonto_5_tutari = reader.ReadDouble(),
			iskonto_6_uygulama_sekli = reader.ReadInt32(),
			iskonto_6_tutari = reader.ReadDouble(),
			masraf_1_uygulama_sekli = reader.ReadInt32(),
			masraf_1_tutari = reader.ReadDouble(),
			masraf_2_uygulama_sekli = reader.ReadInt32(),
			masraf_2_tutari = reader.ReadDouble(),
			masraf_3_uygulama_sekli = reader.ReadInt32(),
			masraf_3_tutari = reader.ReadDouble(),
			masraf_4_uygulama_sekli = reader.ReadInt32(),
			masraf_4_tutari = reader.ReadDouble(),
			vergi_pntr = reader.ReadInt32(),
			vergi_tutari = reader.ReadDouble(),
			masraf_vergi_pntr = reader.ReadInt32(),
			masraf_vergi_tutari = reader.ReadDouble(),
			otv_pntr = reader.ReadInt32(),
			otv_vergi = reader.ReadDouble(),
			oiv_pntr = reader.ReadInt32(),
			oiv_vergi = reader.ReadDouble(),
			birim1_ad = reader.ReadString(),
			birim2_ad = reader.ReadString(),
			birim3_ad = reader.ReadString(),
			birim4_ad = reader.ReadString(),
			birim1_katsayi = reader.ReadDouble(),
			birim2_katsayi = reader.ReadDouble(),
			birim3_katsayi = reader.ReadDouble(),
			birim4_katsayi = reader.ReadDouble(),
			birim1_barkod = reader.ReadString(),
			birim2_barkod = reader.ReadString(),
			birim3_barkod = reader.ReadString(),
			birim4_barkod = reader.ReadString(),
			Vade = new DateTime(reader.ReadInt64())
		};
	}
}
