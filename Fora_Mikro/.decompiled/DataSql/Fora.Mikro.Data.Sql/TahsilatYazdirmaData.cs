using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.Evraklar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Utility;
using Fora.Mikro.Yazdirma;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TahsilatYazdirmaData
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SQL
	{
		public static List<string> TextOlustur(SqlBaglantiBilgileri baglantibilgileri, string DBName, Evrak evrak, YaziciAyarlari yaziciayarlari)
		{
			List<string> list = new List<string>();
			int getInt = yaziciayarlari.genelayarlar._GetParametre("SayfaKolonSayisi")._GetInt;
			int getInt2 = yaziciayarlari.genelayarlar._GetParametre("SayfaSatirSayisi")._GetInt;
			int getInt3 = yaziciayarlari.genelayarlar._GetParametre("DetayBaslangicSatiri")._GetInt;
			int getInt4 = yaziciayarlari.genelayarlar._GetParametre("DetayBasiSatirSayisi")._GetInt;
			int getInt5 = yaziciayarlari.genelayarlar._GetParametre("DetayBirSayfadakiKayitSayisi")._GetInt;
			for (int i = 0; i < getInt2; i++)
			{
				list.Add(new string(' ', getInt));
			}
			foreach (Parametreler item in yaziciayarlari.alanlar)
			{
				if (item._GetParametre("BasilacakAlan")._GetInt == 0)
				{
					string getString = item._GetParametre("VeriTipi")._GetString;
					string text = item._GetParametre("Veri")._GetString;
					int getInt6 = item._GetParametre("Kolon")._GetInt;
					int getInt7 = item._GetParametre("Satir")._GetInt;
					int getInt8 = item._GetParametre("Genislik")._GetInt;
					enum_Yazdirma_Hizalama getInt9 = (enum_Yazdirma_Hizalama)item._GetParametre("Hizalama")._GetInt;
					if (getString != "[statik_metin]")
					{
						text = StatikAlanVeriGetir(baglantibilgileri, DBName, evrak, getString);
					}
					if (text.Length > getInt8)
					{
						text = text.Substring(0, getInt8);
					}
					int num = getInt6 - 1;
					switch (getInt9)
					{
					case enum_Yazdirma_Hizalama.Orta:
						num += (getInt8 - text.Length) / 2;
						break;
					case enum_Yazdirma_Hizalama.Sag:
						num += getInt8 - text.Length;
						break;
					}
					list[getInt7 - 1] = list[getInt7 - 1].Remove(num, text.Length);
					list[getInt7 - 1] = list[getInt7 - 1].Insert(num, text);
				}
			}
			int num2 = 0;
			foreach (Parametreler item2 in yaziciayarlari.alanlar)
			{
				if (item2._GetParametre("BasilacakAlan")._GetInt != 1)
				{
					continue;
				}
				string getString2 = item2._GetParametre("VeriTipi")._GetString;
				string text2 = item2._GetParametre("Veri")._GetString;
				int getInt10 = item2._GetParametre("Kolon")._GetInt;
				int num3 = item2._GetParametre("Satir")._GetInt - 1;
				int getInt11 = item2._GetParametre("Genislik")._GetInt;
				enum_Yazdirma_Hizalama getInt12 = (enum_Yazdirma_Hizalama)item2._GetParametre("Hizalama")._GetInt;
				int num4 = getInt3;
				int num5 = 0;
				foreach (CARI_HESAP_HAREKETLERI item3 in evrak.GetTahsilatHareketleri())
				{
					_ = item3;
					if (getString2 != "[statik_metin]")
					{
						text2 = DinamikAlanVeriGetir(evrak, getString2, num5);
					}
					if (text2.Length > getInt11)
					{
						text2 = text2.Substring(0, getInt11);
					}
					int num6 = getInt10 - 1;
					switch (getInt12)
					{
					case enum_Yazdirma_Hizalama.Orta:
						num6 += (getInt11 - text2.Length) / 2;
						break;
					case enum_Yazdirma_Hizalama.Sag:
						num6 += getInt11 - text2.Length;
						break;
					}
					list[num4 - 1 + num3] = list[num4 - 1 + num3].Remove(num6, text2.Length);
					list[num4 - 1 + num3] = list[num4 - 1 + num3].Insert(num6, text2);
					num4 += getInt4;
					num2 = num4;
					num5++;
				}
				foreach (STOK_HAREKETLERI item4 in evrak.GetStokHareketleri())
				{
					_ = item4;
					if (getString2 != "[statik_metin]")
					{
						text2 = DinamikAlanVeriGetir(evrak, getString2, num5);
					}
					if (text2.Length > getInt11)
					{
						text2 = text2.Substring(0, getInt11);
					}
					int num7 = getInt10 - 1;
					switch (getInt12)
					{
					case enum_Yazdirma_Hizalama.Orta:
						num7 += (getInt11 - text2.Length) / 2;
						break;
					case enum_Yazdirma_Hizalama.Sag:
						num7 += getInt11 - text2.Length;
						break;
					}
					list[num4 - 1 + num3] = list[num4 - 1 + num3].Remove(num7, text2.Length);
					list[num4 - 1 + num3] = list[num4 - 1 + num3].Insert(num7, text2);
					num4 += getInt4;
					num2 = num4;
					num5++;
				}
			}
			foreach (Parametreler item5 in yaziciayarlari.alanlar)
			{
				if (item5._GetParametre("BasilacakAlan")._GetInt == 2)
				{
					string getString3 = item5._GetParametre("VeriTipi")._GetString;
					string text3 = item5._GetParametre("Veri")._GetString;
					int getInt13 = item5._GetParametre("Kolon")._GetInt;
					int getInt14 = item5._GetParametre("Genislik")._GetInt;
					enum_Yazdirma_Hizalama getInt15 = (enum_Yazdirma_Hizalama)item5._GetParametre("Hizalama")._GetInt;
					bool getBoolean = item5._GetParametre("DetayinBittigiYereKaydir")._GetBoolean;
					if (getString3 != "[statik_metin]")
					{
						text3 = StatikAlanVeriGetir(baglantibilgileri, DBName, evrak, getString3);
					}
					if (text3.Length > getInt14)
					{
						text3 = text3.Substring(0, getInt14);
					}
					int num8 = getInt13 - 1;
					switch (getInt15)
					{
					case enum_Yazdirma_Hizalama.Orta:
						num8 += (getInt14 - text3.Length) / 2;
						break;
					case enum_Yazdirma_Hizalama.Sag:
						num8 += getInt14 - text3.Length;
						break;
					}
					int getInt16 = item5._GetParametre("Satir")._GetInt;
					getInt16 = ((!getBoolean) ? (getInt16 + (getInt3 + getInt5 * getInt4)) : (getInt16 + (num2 - 1)));
					list[getInt16 - 1] = list[getInt16 - 1].Remove(num8, text3.Length);
					list[getInt16 - 1] = list[getInt16 - 1].Insert(num8, text3);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j] = list[j].TrimEnd();
			}
			int num9 = getInt2;
			while (num9 > 0 && list[num9 - 1] == "")
			{
				list.RemoveAt(list.Count - 1);
				num9--;
			}
			return list;
		}

		private static string StatikAlanVeriGetir(SqlBaglantiBilgileri baglantibilgileri, string DBName, Evrak evrak, string veriadi)
		{
			switch (veriadi)
			{
			case "[cari_kod]":
				return evrak.cari.cari_kod;
			case "[cari_unvan1]":
				return evrak.cari.cari_unvan1;
			case "[cari_unvan2]":
				return evrak.cari.cari_unvan2;
			case "[evrak_tarihi]":
				return evrak.EvrakTarihi.ToString("dd.MM.yyyy");
			case "[evrak_seri]":
				return evrak.EvrakNoSeri;
			case "[evrak_sira]":
				return evrak.EvrakNoSira.ToString();
			case "[evrak_seri_sira]":
				return evrak.EvrakNoSeri + "-" + evrak.EvrakNoSira;
			case "[belge_no]":
				return evrak.BelgeNo;
			case "[vdaire_no]":
				return evrak.cari.cari_vdaire_no;
			case "[vdaire_adi]":
				return evrak.cari.cari_vdaire_adi;
			case "[adr_cadde]":
			{
				CariAdres cariAdres6 = new CariAdres();
				foreach (CariAdres item in evrak.cari.CariAdresleri)
				{
					if (item.adr_adres_no == evrak.cari.cari_fatura_adres_no)
					{
						cariAdres6 = item;
						break;
					}
				}
				return cariAdres6.adr_cadde;
			}
			case "[adr_sokak]":
			{
				CariAdres cariAdres5 = new CariAdres();
				foreach (CariAdres item2 in evrak.cari.CariAdresleri)
				{
					if (item2.adr_adres_no == evrak.cari.cari_fatura_adres_no)
					{
						cariAdres5 = item2;
						break;
					}
				}
				return cariAdres5.adr_sokak;
			}
			case "[adr_ilce]":
			{
				CariAdres cariAdres4 = new CariAdres();
				foreach (CariAdres item3 in evrak.cari.CariAdresleri)
				{
					if (item3.adr_adres_no == evrak.cari.cari_fatura_adres_no)
					{
						cariAdres4 = item3;
						break;
					}
				}
				return cariAdres4.adr_ilce;
			}
			case "[adr_il]":
			{
				CariAdres cariAdres3 = new CariAdres();
				foreach (CariAdres item4 in evrak.cari.CariAdresleri)
				{
					if (item4.adr_adres_no == evrak.cari.cari_fatura_adres_no)
					{
						cariAdres3 = item4;
						break;
					}
				}
				return cariAdres3.adr_il;
			}
			case "[adr_posta_kodu]":
			{
				CariAdres cariAdres2 = new CariAdres();
				foreach (CariAdres item5 in evrak.cari.CariAdresleri)
				{
					if (item5.adr_adres_no == evrak.cari.cari_fatura_adres_no)
					{
						cariAdres2 = item5;
						break;
					}
				}
				return cariAdres2.adr_posta_kodu;
			}
			case "[adr_ulke]":
			{
				CariAdres cariAdres = new CariAdres();
				foreach (CariAdres item6 in evrak.cari.CariAdresleri)
				{
					if (item6.adr_adres_no == evrak.cari.cari_fatura_adres_no)
					{
						cariAdres = item6;
						break;
					}
				}
				return cariAdres.adr_ulke;
			}
			case "[toplam_nakit]":
			{
				double num4 = 0.0;
				foreach (CARI_HESAP_HAREKETLERI item7 in evrak.GetTahsilatHareketleri())
				{
					if (item7.cha_cinsi == enum_cha_cinsi.Nakit)
					{
						num4 += item7.cha_meblag;
					}
				}
				return doubletostring(num4);
			}
			case "[toplam_cek]":
			{
				double num3 = 0.0;
				foreach (CARI_HESAP_HAREKETLERI item8 in evrak.GetTahsilatHareketleri())
				{
					if (item8.cha_cinsi == enum_cha_cinsi.MusteriCeki)
					{
						num3 += item8.cha_meblag;
					}
				}
				return doubletostring(num3);
			}
			case "[toplam_senet]":
			{
				double num2 = 0.0;
				foreach (CARI_HESAP_HAREKETLERI item9 in evrak.GetTahsilatHareketleri())
				{
					if (item9.cha_cinsi == enum_cha_cinsi.MusteriSenedi)
					{
						num2 += item9.cha_meblag;
					}
				}
				return doubletostring(num2);
			}
			case "[cek_sayisi]":
			{
				int num = 0;
				foreach (CARI_HESAP_HAREKETLERI item10 in evrak.GetTahsilatHareketleri())
				{
					if (item10.cha_cinsi == enum_cha_cinsi.MusteriCeki)
					{
						num++;
					}
				}
				return num.ToString();
			}
			case "[genel_toplam]":
				return doubletostring(evrak.GetYekun());
			case "[genel_toplam_yazi_ile]":
				return yaziyaCevir(evrak.GetYekun());
			case "[temsilci_adi]":
			{
				CariPersonel cariPersonel = CariPersonelData.GetCariPersonel(baglantibilgileri, DBName, evrak.TemsilciKodu);
				return cariPersonel.cari_per_adi + " " + cariPersonel.cari_per_soyadi;
			}
			case "[ara_toplam]":
				return doubletostring(evrak.GetAraToplam());
			case "[toplam_iskonto_tutari]":
				return doubletostring(evrak.GetIskontoTutari());
			case "[toplam_masraf_tutari]":
				return doubletostring(evrak.GetMasrafTutari());
			case "[toplam_vergi_tutari]":
				return doubletostring(evrak.GetKdvTutari());
			default:
				return veriadi;
			}
		}
	}

	private static string DinamikAlanVeriGetir(Evrak evrak, string veriadi, int sira)
	{
		switch (veriadi)
		{
		case "[satir_cinsi]":
			return evrak.GetTahsilatHareketleri()[sira].cha_cinsi switch
			{
				enum_cha_cinsi.Nakit => "NAKİT", 
				enum_cha_cinsi.MusteriCeki => "ÇEK", 
				enum_cha_cinsi.MusteriSenedi => "SENET", 
				enum_cha_cinsi.FirmaCeki => "ÇEK", 
				enum_cha_cinsi.FirmaKrediKarti => "KREDİ KARTI", 
				enum_cha_cinsi.FirmaSenedi => "SENET", 
				enum_cha_cinsi.MusteriKrediKarti => "KREDİ KARTI", 
				_ => "", 
			};
		case "[satir_ceksenet_no]":
			return evrak.GetTahsilatHareketleri()[sira].sck_no;
		case "[satir_vade]":
		{
			if (evrak.GetTahsilatHareketleri()[sira].cha_vade.ToString().Length == 8)
			{
				string text = evrak.GetTahsilatHareketleri()[sira].cha_vade.ToString();
				return text.Substring(6, 2) + "." + text.Substring(4, 2) + "." + text.Substring(0, 4);
			}
			string text2 = "";
			if (evrak.GetTahsilatHareketleri()[sira].cha_vade == 0)
			{
				return "";
			}
			return evrak.EvrakTarihi.AddDays(evrak.GetTahsilatHareketleri()[sira].cha_vade * -1).ToString("dd.MM.yyyy");
		}
		case "[satir_aciklama]":
			return evrak.GetTahsilatHareketleri()[sira].cha_aciklama;
		case "[satir_tutari]":
			return doubletostring(evrak.GetTahsilatHareketleri()[sira].cha_meblag);
		default:
			return veriadi;
		}
	}

	private static string doubletostring(double value)
	{
		return value.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR"));
	}

	private static string yaziyaCevir(double tutar)
	{
		string text = tutar.ToString("F2").Replace('.', ',');
		string text2 = text.Substring(0, text.IndexOf(','));
		string text3 = text.Substring(text.IndexOf(',') + 1, 2);
		string text4 = "";
		string[] array = new string[10] { "", "BİR", "İKİ", "ÜÇ", "DÖRT", "BEŞ", "ALTI", "YEDİ", "SEKİZ", "DOKUZ" };
		string[] array2 = new string[10] { "", "ON", "YİRMİ", "OTUZ", "KIRK", "ELLİ", "ALTMIŞ", "YETMİŞ", "SEKSEN", "DOKSAN" };
		string[] array3 = new string[6] { "KATRİLYON", "TRİLYON", "MİLYAR", "MİLYON", "BİN", "" };
		int num = 6;
		text2 = text2.PadLeft(num * 3, '0');
		for (int i = 0; i < num * 3; i += 3)
		{
			string text5 = "";
			if (text2.Substring(i, 1) != "0")
			{
				text5 = text5 + array[Convert.ToInt32(text2.Substring(i, 1))] + "YÜZ";
			}
			if (text5 == "BİRYÜZ")
			{
				text5 = "YÜZ";
			}
			text5 += array2[Convert.ToInt32(text2.Substring(i + 1, 1))];
			text5 += array[Convert.ToInt32(text2.Substring(i + 2, 1))];
			if (text5 != "")
			{
				text5 += array3[i / 3];
			}
			if (text5 == "BİRBİN")
			{
				text5 = "BİN";
			}
			text4 += text5;
		}
		if (text4 != "")
		{
			text4 += " TL ";
		}
		int length = text4.Length;
		if (text3.Substring(0, 1) != "0")
		{
			text4 += array2[Convert.ToInt32(text3.Substring(0, 1))];
		}
		if (text3.Substring(1, 1) != "0")
		{
			text4 += array[Convert.ToInt32(text3.Substring(1, 1))];
		}
		if (text4.Length > length)
		{
			return text4 + " Kr.";
		}
		return text4 + "SIFIR Kr.";
	}
}
