using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSatis;

public static class RaporStokSatisHelper
{
	public static List<RaporStokSatisListItem> RaporGruplandir(RaporStokSatisSonuc Rapor, RaporStokSatisSonucHam RaporHam)
	{
		List<RaporStokSatisListItem> list = new List<RaporStokSatisListItem>();
		foreach (RaporStokSatisSatir item in RaporHam.sonuc_ham)
		{
			int num = 0;
			int num2 = -1;
			foreach (RaporStokSatisListItem item2 in list)
			{
				switch (Rapor.gruplandirma_secenegi)
				{
				case enum_gruplandirma_secenekleri.Ay:
					if (item.tarih.Year + "." + item.tarih.Month.ToString("d2") == item2.kodu)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.Cari:
					if (item.cari_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.CariBolge:
					if (RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.CariGrup:
					if (RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.Gun:
					if (item.tarih.Year + "." + item.tarih.Month.ToString("d2") + "." + item.tarih.Day.ToString("d2") == item2.kodu)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.Proje:
					if (item.proje_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.SorumlulukMerkezi:
					if (item.sorumluluk_merkezi_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.Stok:
					if (item.stok_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.StokAnaGrubu:
					if (RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.StokKategori:
					if (RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.StokMarka:
					if (RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.StokReyon:
					if (RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.StokUretici:
					if (RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.Temsilci:
					if (item.plasiyer_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_gruplandirma_secenekleri.Depo:
					if (item.depo_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				}
				if (num2 != -1)
				{
					break;
				}
				num++;
			}
			if (num2 == -1)
			{
				RaporStokSatisListItem raporStokSatisListItem = new RaporStokSatisListItem();
				switch (Rapor.gruplandirma_secenegi)
				{
				case enum_gruplandirma_secenekleri.Ay:
				{
					raporStokSatisListItem.kodu = item.tarih.Year + "." + item.tarih.Month.ToString("d2");
					CultureInfo cultureInfo = new CultureInfo("tr-TR");
					raporStokSatisListItem.adi = item.tarih.ToString("MMMM", cultureInfo) + " " + item.tarih.Year;
					break;
				}
				case enum_gruplandirma_secenekleri.Cari:
					raporStokSatisListItem.liste_sira_no = item.cari_sira_no;
					raporStokSatisListItem.kodu = RaporHam.cariler[item.cari_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.cariler[item.cari_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.CariBolge:
					raporStokSatisListItem.liste_sira_no = RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no;
					raporStokSatisListItem.kodu = RaporHam.cari_bolgeleri[RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.cari_bolgeleri[RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.CariGrup:
					raporStokSatisListItem.liste_sira_no = RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no;
					raporStokSatisListItem.kodu = RaporHam.cari_gruplari[RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.cari_gruplari[RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Gun:
				{
					string adi = (raporStokSatisListItem.kodu = item.tarih.Year + "." + item.tarih.Month.ToString("d2") + "." + item.tarih.Day.ToString("d2"));
					raporStokSatisListItem.adi = adi;
					break;
				}
				case enum_gruplandirma_secenekleri.Proje:
					raporStokSatisListItem.liste_sira_no = item.proje_sira_no;
					raporStokSatisListItem.kodu = RaporHam.projeler[item.proje_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.projeler[item.proje_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.SorumlulukMerkezi:
					raporStokSatisListItem.liste_sira_no = item.sorumluluk_merkezi_sira_no;
					raporStokSatisListItem.kodu = RaporHam.sorumluluk_merkezleri[item.sorumluluk_merkezi_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.sorumluluk_merkezleri[item.sorumluluk_merkezi_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Stok:
					raporStokSatisListItem.liste_sira_no = item.stok_sira_no;
					raporStokSatisListItem.kodu = RaporHam.stoklar[item.stok_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.stoklar[item.stok_sira_no].Adi;
					raporStokSatisListItem.birim_adi = RaporHam.stoklar[item.stok_sira_no].Birim1Adi;
					raporStokSatisListItem.birim2_adi = RaporHam.stoklar[item.stok_sira_no].Birim2Adi;
					raporStokSatisListItem.birim3_adi = RaporHam.stoklar[item.stok_sira_no].Birim3Adi;
					break;
				case enum_gruplandirma_secenekleri.StokAnaGrubu:
					raporStokSatisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no;
					raporStokSatisListItem.kodu = RaporHam.stok_ana_gruplari[RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.stok_ana_gruplari[RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokKategori:
					raporStokSatisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no;
					raporStokSatisListItem.kodu = RaporHam.stok_kategorileri[RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.stok_kategorileri[RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokMarka:
					raporStokSatisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no;
					raporStokSatisListItem.kodu = RaporHam.stok_markalari[RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.stok_markalari[RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokReyon:
					raporStokSatisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no;
					raporStokSatisListItem.kodu = RaporHam.stok_reyonlari[RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.stok_reyonlari[RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokUretici:
					raporStokSatisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no;
					raporStokSatisListItem.kodu = RaporHam.stok_ureticileri[RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.stok_ureticileri[RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Temsilci:
					raporStokSatisListItem.liste_sira_no = item.plasiyer_sira_no;
					raporStokSatisListItem.kodu = RaporHam.temsilciler[item.plasiyer_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.temsilciler[item.plasiyer_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Depo:
					raporStokSatisListItem.liste_sira_no = item.depo_sira_no;
					raporStokSatisListItem.kodu = RaporHam.depolar[item.depo_sira_no].Kodu;
					raporStokSatisListItem.adi = RaporHam.depolar[item.depo_sira_no].Adi;
					break;
				}
				raporStokSatisListItem.miktar1 = 0.0;
				raporStokSatisListItem.miktar2 = 0.0;
				raporStokSatisListItem.miktar3 = 0.0;
				raporStokSatisListItem.tutar1 = 0.0;
				raporStokSatisListItem.tutar2 = 0.0;
				raporStokSatisListItem.tutar3 = 0.0;
				if (raporStokSatisListItem.adi == "")
				{
					raporStokSatisListItem.adi = "TANIMSIZ";
				}
				list.Add(raporStokSatisListItem);
				num2 = list.Count - 1;
			}
			switch (Rapor.miktar1_secenek)
			{
			case enum_miktar_secenekleri.BrutSatisMiktari:
				if (item.normal_iade == 0)
				{
					list[num2].miktar1 += item.miktar;
				}
				break;
			case enum_miktar_secenekleri.IadeMiktari:
				if (item.normal_iade == 1)
				{
					list[num2].miktar1 += item.miktar;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktari:
				if (item.normal_iade == 0)
				{
					list[num2].miktar1 += item.miktar;
				}
				else
				{
					list[num2].miktar1 -= item.miktar;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktariBirim2:
				if (item.normal_iade == 0)
				{
					list[num2].miktar1 += item.miktar_birim2;
				}
				else
				{
					list[num2].miktar1 -= item.miktar_birim2;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktariBirim3:
				if (item.normal_iade == 0)
				{
					list[num2].miktar1 += item.miktar_birim3;
				}
				else
				{
					list[num2].miktar1 -= item.miktar_birim3;
				}
				break;
			case enum_miktar_secenekleri.CariAdresSayisi:
			{
				if (item.normal_iade != 0)
				{
					break;
				}
				bool flag = false;
				string text2 = RaporHam.cariler[item.cari_sira_no].Kodu + "|" + item.sevk_adres_no;
				foreach (string item3 in list[num2].cari_adresleri)
				{
					if (item3 == text2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list[num2].miktar1 += 1.0;
					list[num2].cari_adresleri.Add(text2);
				}
				break;
			}
			}
			switch (Rapor.miktar2_secenek)
			{
			case enum_miktar_secenekleri.BrutSatisMiktari:
				if (item.normal_iade == 0)
				{
					list[num2].miktar2 += item.miktar;
				}
				break;
			case enum_miktar_secenekleri.IadeMiktari:
				if (item.normal_iade == 1)
				{
					list[num2].miktar2 += item.miktar;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktari:
				if (item.normal_iade == 0)
				{
					list[num2].miktar2 += item.miktar;
				}
				else
				{
					list[num2].miktar2 -= item.miktar;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktariBirim2:
				if (item.normal_iade == 0)
				{
					list[num2].miktar2 += item.miktar_birim2;
				}
				else
				{
					list[num2].miktar2 -= item.miktar_birim2;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktariBirim3:
				if (item.normal_iade == 0)
				{
					list[num2].miktar2 += item.miktar_birim3;
				}
				else
				{
					list[num2].miktar2 -= item.miktar_birim3;
				}
				break;
			case enum_miktar_secenekleri.CariAdresSayisi:
			{
				if (item.normal_iade != 0)
				{
					break;
				}
				bool flag2 = false;
				string text3 = RaporHam.cariler[item.cari_sira_no].Kodu + "|" + item.sevk_adres_no;
				foreach (string item4 in list[num2].cari_adresleri)
				{
					if (item4 == text3)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					list[num2].miktar2 += 1.0;
					list[num2].cari_adresleri.Add(text3);
				}
				break;
			}
			}
			switch (Rapor.miktar3_secenek)
			{
			case enum_miktar_secenekleri.BrutSatisMiktari:
				if (item.normal_iade == 0)
				{
					list[num2].miktar3 += item.miktar;
				}
				break;
			case enum_miktar_secenekleri.IadeMiktari:
				if (item.normal_iade == 1)
				{
					list[num2].miktar3 += item.miktar;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktari:
				if (item.normal_iade == 0)
				{
					list[num2].miktar3 += item.miktar;
				}
				else
				{
					list[num2].miktar3 -= item.miktar;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktariBirim2:
				if (item.normal_iade == 0)
				{
					list[num2].miktar3 += item.miktar_birim2;
				}
				else
				{
					list[num2].miktar3 -= item.miktar_birim2;
				}
				break;
			case enum_miktar_secenekleri.NetSatisMiktariBirim3:
				if (item.normal_iade == 0)
				{
					list[num2].miktar3 += item.miktar_birim3;
				}
				else
				{
					list[num2].miktar3 -= item.miktar_birim3;
				}
				break;
			case enum_miktar_secenekleri.CariAdresSayisi:
			{
				if (item.normal_iade != 0)
				{
					break;
				}
				bool flag3 = false;
				string text4 = RaporHam.cariler[item.cari_sira_no].Kodu + "|" + item.sevk_adres_no;
				foreach (string item5 in list[num2].cari_adresleri)
				{
					if (item5 == text4)
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					list[num2].miktar3 += 1.0;
					list[num2].cari_adresleri.Add(text4);
				}
				break;
			}
			}
			if (item.normal_iade == 0)
			{
				list[num2].net_tutar += item.brut_tutar - item.toplam_iskonto;
			}
			else
			{
				list[num2].net_tutar -= item.brut_tutar - item.toplam_iskonto;
			}
			if (item.normal_iade == 0)
			{
				list[num2].net_maliyet += item.net_toplam_maliyet;
			}
			else
			{
				list[num2].net_maliyet -= item.net_toplam_maliyet;
			}
			switch (Rapor.tutar1_secenek)
			{
			case enum_tutar_secenekleri.BrutTutar:
				if (item.normal_iade == 0)
				{
					list[num2].tutar1 += item.brut_tutar;
				}
				else
				{
					list[num2].tutar1 -= item.brut_tutar;
				}
				break;
			case enum_tutar_secenekleri.IskontoTutari:
				if (item.normal_iade == 0)
				{
					list[num2].tutar1 += item.toplam_iskonto;
				}
				else
				{
					list[num2].tutar1 -= item.toplam_iskonto;
				}
				break;
			case enum_tutar_secenekleri.NetTutar:
				if (item.normal_iade == 0)
				{
					list[num2].tutar1 += item.brut_tutar - item.toplam_iskonto;
				}
				else
				{
					list[num2].tutar1 -= item.brut_tutar - item.toplam_iskonto;
				}
				break;
			case enum_tutar_secenekleri.NetTutarKdvDahil:
				if (item.normal_iade == 0)
				{
					list[num2].tutar1 += item.brut_tutar - item.toplam_iskonto + item.toplam_vergi;
				}
				else
				{
					list[num2].tutar1 -= item.brut_tutar - item.toplam_iskonto + item.toplam_vergi;
				}
				break;
			case enum_tutar_secenekleri.NetKarTutari:
				if (item.normal_iade == 0)
				{
					list[num2].tutar1 += item.brut_tutar - item.toplam_iskonto - item.net_toplam_maliyet;
				}
				else
				{
					list[num2].tutar1 -= item.brut_tutar - item.toplam_iskonto - item.net_toplam_maliyet;
				}
				break;
			case enum_tutar_secenekleri.NetKarYuzdesi:
				if (item.normal_iade == 0)
				{
					list[num2].tutar1 += 0.0;
				}
				else
				{
					list[num2].tutar1 -= 0.0;
				}
				break;
			case enum_tutar_secenekleri.NetMaliyet:
				if (item.normal_iade == 0)
				{
					list[num2].tutar1 += item.net_toplam_maliyet;
				}
				else
				{
					list[num2].tutar1 -= item.net_toplam_maliyet;
				}
				break;
			}
			switch (Rapor.tutar2_secenek)
			{
			case enum_tutar_secenekleri.BrutTutar:
				if (item.normal_iade == 0)
				{
					list[num2].tutar2 += item.brut_tutar;
				}
				else
				{
					list[num2].tutar2 -= item.brut_tutar;
				}
				break;
			case enum_tutar_secenekleri.IskontoTutari:
				if (item.normal_iade == 0)
				{
					list[num2].tutar2 += item.toplam_iskonto;
				}
				else
				{
					list[num2].tutar2 -= item.toplam_iskonto;
				}
				break;
			case enum_tutar_secenekleri.NetTutar:
				if (item.normal_iade == 0)
				{
					list[num2].tutar2 += item.brut_tutar - item.toplam_iskonto;
				}
				else
				{
					list[num2].tutar2 -= item.brut_tutar - item.toplam_iskonto;
				}
				break;
			case enum_tutar_secenekleri.NetTutarKdvDahil:
				if (item.normal_iade == 0)
				{
					list[num2].tutar2 += item.brut_tutar - item.toplam_iskonto + item.toplam_vergi;
				}
				else
				{
					list[num2].tutar2 -= item.brut_tutar - item.toplam_iskonto + item.toplam_vergi;
				}
				break;
			case enum_tutar_secenekleri.NetKarTutari:
				if (item.normal_iade == 0)
				{
					list[num2].tutar2 += item.brut_tutar - item.toplam_iskonto - item.net_toplam_maliyet;
				}
				else
				{
					list[num2].tutar2 -= item.brut_tutar - item.toplam_iskonto - item.net_toplam_maliyet;
				}
				break;
			case enum_tutar_secenekleri.NetKarYuzdesi:
				if (item.normal_iade == 0)
				{
					list[num2].tutar2 += 0.0;
				}
				else
				{
					list[num2].tutar2 -= 0.0;
				}
				break;
			case enum_tutar_secenekleri.NetMaliyet:
				if (item.normal_iade == 0)
				{
					list[num2].tutar2 += item.net_toplam_maliyet;
				}
				else
				{
					list[num2].tutar2 -= item.net_toplam_maliyet;
				}
				break;
			}
			switch (Rapor.tutar3_secenek)
			{
			case enum_tutar_secenekleri.BrutTutar:
				if (item.normal_iade == 0)
				{
					list[num2].tutar3 += item.brut_tutar;
				}
				else
				{
					list[num2].tutar3 -= item.brut_tutar;
				}
				break;
			case enum_tutar_secenekleri.IskontoTutari:
				if (item.normal_iade == 0)
				{
					list[num2].tutar3 += item.toplam_iskonto;
				}
				else
				{
					list[num2].tutar3 -= item.toplam_iskonto;
				}
				break;
			case enum_tutar_secenekleri.NetTutar:
				if (item.normal_iade == 0)
				{
					list[num2].tutar3 += item.brut_tutar - item.toplam_iskonto;
				}
				else
				{
					list[num2].tutar3 -= item.brut_tutar - item.toplam_iskonto;
				}
				break;
			case enum_tutar_secenekleri.NetTutarKdvDahil:
				if (item.normal_iade == 0)
				{
					list[num2].tutar3 += item.brut_tutar - item.toplam_iskonto + item.toplam_vergi;
				}
				else
				{
					list[num2].tutar3 -= item.brut_tutar - item.toplam_iskonto + item.toplam_vergi;
				}
				break;
			case enum_tutar_secenekleri.NetKarTutari:
				if (item.normal_iade == 0)
				{
					list[num2].tutar3 += item.brut_tutar - item.toplam_iskonto - item.net_toplam_maliyet;
				}
				else
				{
					list[num2].tutar3 -= item.brut_tutar - item.toplam_iskonto - item.net_toplam_maliyet;
				}
				break;
			case enum_tutar_secenekleri.NetKarYuzdesi:
				if (item.normal_iade == 0)
				{
					list[num2].tutar3 += 0.0;
				}
				else
				{
					list[num2].tutar3 -= 0.0;
				}
				break;
			case enum_tutar_secenekleri.NetMaliyet:
				if (item.normal_iade == 0)
				{
					list[num2].tutar3 += item.net_toplam_maliyet;
				}
				else
				{
					list[num2].tutar3 -= item.net_toplam_maliyet;
				}
				break;
			}
		}
		if (Rapor.tutar1_secenek == enum_tutar_secenekleri.NetKarYuzdesi)
		{
			foreach (RaporStokSatisListItem item6 in list)
			{
				item6.tutar1 = item6.net_tutar * 100.0 / item6.net_maliyet - 100.0;
			}
		}
		if (Rapor.tutar2_secenek == enum_tutar_secenekleri.NetKarYuzdesi)
		{
			foreach (RaporStokSatisListItem item7 in list)
			{
				item7.tutar2 = item7.net_tutar * 100.0 / item7.net_maliyet - 100.0;
			}
		}
		if (Rapor.tutar3_secenek == enum_tutar_secenekleri.NetKarYuzdesi)
		{
			foreach (RaporStokSatisListItem item8 in list)
			{
				item8.tutar3 = item8.net_tutar * 100.0 / item8.net_maliyet - 100.0;
			}
		}
		switch (Rapor.siralama_secenegi)
		{
		case enum_siralama_secenekleri.Isim:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderBy<RaporStokSatisListItem, string>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, string>)((RaporStokSatisListItem i) => i.adi)));
			break;
		case enum_siralama_secenekleri.Kod:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderBy<RaporStokSatisListItem, string>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, string>)((RaporStokSatisListItem i) => i.kodu)));
			break;
		case enum_siralama_secenekleri.Miktar1:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderByDescending<RaporStokSatisListItem, double>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, double>)((RaporStokSatisListItem i) => i.miktar1)));
			break;
		case enum_siralama_secenekleri.Miktar2:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderByDescending<RaporStokSatisListItem, double>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, double>)((RaporStokSatisListItem i) => i.miktar2)));
			break;
		case enum_siralama_secenekleri.Miktar3:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderByDescending<RaporStokSatisListItem, double>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, double>)((RaporStokSatisListItem i) => i.miktar3)));
			break;
		case enum_siralama_secenekleri.Tutar1:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderByDescending<RaporStokSatisListItem, double>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, double>)((RaporStokSatisListItem i) => i.tutar1)));
			break;
		case enum_siralama_secenekleri.Tutar2:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderByDescending<RaporStokSatisListItem, double>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, double>)((RaporStokSatisListItem i) => i.tutar2)));
			break;
		case enum_siralama_secenekleri.Tutar3:
			list = Enumerable.ToList<RaporStokSatisListItem>((IEnumerable<RaporStokSatisListItem>)Enumerable.OrderByDescending<RaporStokSatisListItem, double>((IEnumerable<RaporStokSatisListItem>)list, (Func<RaporStokSatisListItem, double>)((RaporStokSatisListItem i) => i.tutar3)));
			break;
		}
		return list;
	}
}
