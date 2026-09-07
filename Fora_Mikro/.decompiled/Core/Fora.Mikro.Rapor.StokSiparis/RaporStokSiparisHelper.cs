using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSiparis;

public static class RaporStokSiparisHelper
{
	public static List<RaporStokSiparisListItem> RaporGruplandir(RaporStokSiparisSonuc Rapor, RaporStokSiparisSonucHam RaporHam)
	{
		List<RaporStokSiparisListItem> list = new List<RaporStokSiparisListItem>();
		foreach (RaporStokSiparisSatir item in RaporHam.sonuc_ham)
		{
			int num = 0;
			int num2 = -1;
			foreach (RaporStokSiparisListItem item2 in list)
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
				RaporStokSiparisListItem raporStokSiparisListItem = new RaporStokSiparisListItem();
				switch (Rapor.gruplandirma_secenegi)
				{
				case enum_gruplandirma_secenekleri.Ay:
				{
					raporStokSiparisListItem.kodu = item.tarih.Year + "." + item.tarih.Month.ToString("d2");
					CultureInfo cultureInfo = new CultureInfo("tr-TR");
					raporStokSiparisListItem.adi = item.tarih.ToString("MMMM", cultureInfo) + " " + item.tarih.Year;
					break;
				}
				case enum_gruplandirma_secenekleri.Cari:
					raporStokSiparisListItem.liste_sira_no = item.cari_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.cariler[item.cari_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.cariler[item.cari_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.CariBolge:
					raporStokSiparisListItem.liste_sira_no = RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.cari_bolgeleri[RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.cari_bolgeleri[RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.CariGrup:
					raporStokSiparisListItem.liste_sira_no = RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.cari_gruplari[RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.cari_gruplari[RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Gun:
				{
					string adi = (raporStokSiparisListItem.kodu = item.tarih.Year + "." + item.tarih.Month.ToString("d2") + "." + item.tarih.Day.ToString("d2"));
					raporStokSiparisListItem.adi = adi;
					break;
				}
				case enum_gruplandirma_secenekleri.Proje:
					raporStokSiparisListItem.liste_sira_no = item.proje_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.projeler[item.proje_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.projeler[item.proje_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.SorumlulukMerkezi:
					raporStokSiparisListItem.liste_sira_no = item.sorumluluk_merkezi_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.sorumluluk_merkezleri[item.sorumluluk_merkezi_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.sorumluluk_merkezleri[item.sorumluluk_merkezi_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Stok:
					raporStokSiparisListItem.liste_sira_no = item.stok_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.stoklar[item.stok_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.stoklar[item.stok_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokAnaGrubu:
					raporStokSiparisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.stok_ana_gruplari[RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.stok_ana_gruplari[RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokKategori:
					raporStokSiparisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.stok_kategorileri[RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.stok_kategorileri[RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokMarka:
					raporStokSiparisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.stok_markalari[RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.stok_markalari[RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokReyon:
					raporStokSiparisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.stok_reyonlari[RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.stok_reyonlari[RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.StokUretici:
					raporStokSiparisListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.stok_ureticileri[RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.stok_ureticileri[RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Temsilci:
					raporStokSiparisListItem.liste_sira_no = item.plasiyer_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.temsilciler[item.plasiyer_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.temsilciler[item.plasiyer_sira_no].Adi;
					break;
				case enum_gruplandirma_secenekleri.Depo:
					raporStokSiparisListItem.liste_sira_no = item.depo_sira_no;
					raporStokSiparisListItem.kodu = RaporHam.depolar[item.depo_sira_no].Kodu;
					raporStokSiparisListItem.adi = RaporHam.depolar[item.depo_sira_no].Adi;
					break;
				}
				raporStokSiparisListItem.miktar1 = 0.0;
				raporStokSiparisListItem.miktar2 = 0.0;
				raporStokSiparisListItem.miktar3 = 0.0;
				raporStokSiparisListItem.tutar1 = 0.0;
				raporStokSiparisListItem.tutar2 = 0.0;
				raporStokSiparisListItem.tutar3 = 0.0;
				if (raporStokSiparisListItem.adi == "")
				{
					raporStokSiparisListItem.adi = "TANIMSIZ";
				}
				list.Add(raporStokSiparisListItem);
				num2 = list.Count - 1;
			}
			switch (Rapor.miktar1_secenek)
			{
			case enum_siparis_miktar_secenekleri.BekleyenMiktar:
				list[num2].miktar1 += item.miktar_bekleyen;
				break;
			case enum_siparis_miktar_secenekleri.SiparisMiktari:
				list[num2].miktar1 += item.miktar_siparis;
				break;
			case enum_siparis_miktar_secenekleri.TeslimEdilenMiktar:
				list[num2].miktar1 += item.miktar_teslim_edilen;
				break;
			case enum_siparis_miktar_secenekleri.VazgecilenMiktar:
				list[num2].miktar1 += item.miktar_vazgecilen;
				break;
			}
			switch (Rapor.miktar2_secenek)
			{
			case enum_siparis_miktar_secenekleri.BekleyenMiktar:
				list[num2].miktar2 += item.miktar_bekleyen;
				break;
			case enum_siparis_miktar_secenekleri.SiparisMiktari:
				list[num2].miktar2 += item.miktar_siparis;
				break;
			case enum_siparis_miktar_secenekleri.TeslimEdilenMiktar:
				list[num2].miktar2 += item.miktar_teslim_edilen;
				break;
			case enum_siparis_miktar_secenekleri.VazgecilenMiktar:
				list[num2].miktar2 += item.miktar_vazgecilen;
				break;
			}
			switch (Rapor.miktar3_secenek)
			{
			case enum_siparis_miktar_secenekleri.BekleyenMiktar:
				list[num2].miktar3 += item.miktar_bekleyen;
				break;
			case enum_siparis_miktar_secenekleri.SiparisMiktari:
				list[num2].miktar3 += item.miktar_siparis;
				break;
			case enum_siparis_miktar_secenekleri.TeslimEdilenMiktar:
				list[num2].miktar3 += item.miktar_teslim_edilen;
				break;
			case enum_siparis_miktar_secenekleri.VazgecilenMiktar:
				list[num2].miktar3 += item.miktar_vazgecilen;
				break;
			}
			switch (Rapor.tutar1_secenek)
			{
			case enum_siparis_tutar_secenekleri.BekleyenBrutTutari:
				list[num2].tutar1 += item.brut_tutar_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenIskontoTutari:
				list[num2].tutar1 += item.toplam_iskonto_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenNetTutar:
				list[num2].tutar1 += item.brut_tutar_bekleyen - item.toplam_iskonto_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenNetTutarKdvDahil:
				list[num2].tutar1 += item.brut_tutar_bekleyen - item.toplam_iskonto_bekleyen + item.toplam_vergi_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.SiparisBrutTutari:
				list[num2].tutar1 += item.brut_tutar_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisIskontoTutari:
				list[num2].tutar1 += item.toplam_iskonto_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisNetTutar:
				list[num2].tutar1 += item.brut_tutar_siparis - item.toplam_iskonto_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisNetTutarKdvDahil:
				list[num2].tutar1 += item.brut_tutar_siparis - item.toplam_iskonto_siparis + item.toplam_vergi_siparis;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenBrutTutari:
				list[num2].tutar1 += item.brut_tutar_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenIskontoTutari:
				list[num2].tutar1 += item.toplam_iskonto_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenNetTutar:
				list[num2].tutar1 += item.brut_tutar_teslim_edilen - item.toplam_iskonto_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenNetTutarKdvDahil:
				list[num2].tutar1 += item.brut_tutar_teslim_edilen - item.toplam_iskonto_teslim_edilen + item.toplam_vergi_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenBrutTutari:
				list[num2].tutar1 += item.brut_tutar_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenIskontoTutari:
				list[num2].tutar1 += item.toplam_iskonto_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenNetTutar:
				list[num2].tutar1 += item.brut_tutar_vazgecilen - item.toplam_iskonto_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenNetTutarKdvDahil:
				list[num2].tutar1 += item.brut_tutar_vazgecilen - item.toplam_iskonto_vazgecilen + item.toplam_vergi_vazgecilen;
				break;
			}
			switch (Rapor.tutar2_secenek)
			{
			case enum_siparis_tutar_secenekleri.BekleyenBrutTutari:
				list[num2].tutar2 += item.brut_tutar_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenIskontoTutari:
				list[num2].tutar2 += item.toplam_iskonto_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenNetTutar:
				list[num2].tutar2 += item.brut_tutar_bekleyen - item.toplam_iskonto_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenNetTutarKdvDahil:
				list[num2].tutar2 += item.brut_tutar_bekleyen - item.toplam_iskonto_bekleyen + item.toplam_vergi_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.SiparisBrutTutari:
				list[num2].tutar2 += item.brut_tutar_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisIskontoTutari:
				list[num2].tutar2 += item.toplam_iskonto_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisNetTutar:
				list[num2].tutar2 += item.brut_tutar_siparis - item.toplam_iskonto_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisNetTutarKdvDahil:
				list[num2].tutar2 += item.brut_tutar_siparis - item.toplam_iskonto_siparis + item.toplam_vergi_siparis;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenBrutTutari:
				list[num2].tutar2 += item.brut_tutar_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenIskontoTutari:
				list[num2].tutar2 += item.toplam_iskonto_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenNetTutar:
				list[num2].tutar2 += item.brut_tutar_teslim_edilen - item.toplam_iskonto_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenNetTutarKdvDahil:
				list[num2].tutar2 += item.brut_tutar_teslim_edilen - item.toplam_iskonto_teslim_edilen + item.toplam_vergi_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenBrutTutari:
				list[num2].tutar2 += item.brut_tutar_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenIskontoTutari:
				list[num2].tutar2 += item.toplam_iskonto_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenNetTutar:
				list[num2].tutar2 += item.brut_tutar_vazgecilen - item.toplam_iskonto_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenNetTutarKdvDahil:
				list[num2].tutar2 += item.brut_tutar_vazgecilen - item.toplam_iskonto_vazgecilen + item.toplam_vergi_vazgecilen;
				break;
			}
			switch (Rapor.tutar3_secenek)
			{
			case enum_siparis_tutar_secenekleri.BekleyenBrutTutari:
				list[num2].tutar3 += item.brut_tutar_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenIskontoTutari:
				list[num2].tutar3 += item.toplam_iskonto_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenNetTutar:
				list[num2].tutar3 += item.brut_tutar_bekleyen - item.toplam_iskonto_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.BekleyenNetTutarKdvDahil:
				list[num2].tutar3 += item.brut_tutar_bekleyen - item.toplam_iskonto_bekleyen + item.toplam_vergi_bekleyen;
				break;
			case enum_siparis_tutar_secenekleri.SiparisBrutTutari:
				list[num2].tutar3 += item.brut_tutar_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisIskontoTutari:
				list[num2].tutar3 += item.toplam_iskonto_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisNetTutar:
				list[num2].tutar3 += item.brut_tutar_siparis - item.toplam_iskonto_siparis;
				break;
			case enum_siparis_tutar_secenekleri.SiparisNetTutarKdvDahil:
				list[num2].tutar3 += item.brut_tutar_siparis - item.toplam_iskonto_siparis + item.toplam_vergi_siparis;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenBrutTutari:
				list[num2].tutar3 += item.brut_tutar_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenIskontoTutari:
				list[num2].tutar3 += item.toplam_iskonto_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenNetTutar:
				list[num2].tutar3 += item.brut_tutar_teslim_edilen - item.toplam_iskonto_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.TeslimEdilenNetTutarKdvDahil:
				list[num2].tutar3 += item.brut_tutar_teslim_edilen - item.toplam_iskonto_teslim_edilen + item.toplam_vergi_teslim_edilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenBrutTutari:
				list[num2].tutar3 += item.brut_tutar_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenIskontoTutari:
				list[num2].tutar3 += item.toplam_iskonto_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenNetTutar:
				list[num2].tutar3 += item.brut_tutar_vazgecilen - item.toplam_iskonto_vazgecilen;
				break;
			case enum_siparis_tutar_secenekleri.VazgecilenNetTutarKdvDahil:
				list[num2].tutar3 += item.brut_tutar_vazgecilen - item.toplam_iskonto_vazgecilen + item.toplam_vergi_vazgecilen;
				break;
			}
		}
		switch (Rapor.siralama_secenegi)
		{
		case enum_siralama_secenekleri.Isim:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderBy<RaporStokSiparisListItem, string>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, string>)((RaporStokSiparisListItem i) => i.adi)));
			break;
		case enum_siralama_secenekleri.Kod:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderBy<RaporStokSiparisListItem, string>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, string>)((RaporStokSiparisListItem i) => i.kodu)));
			break;
		case enum_siralama_secenekleri.Miktar1:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderByDescending<RaporStokSiparisListItem, double>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, double>)((RaporStokSiparisListItem i) => i.miktar1)));
			break;
		case enum_siralama_secenekleri.Miktar2:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderByDescending<RaporStokSiparisListItem, double>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, double>)((RaporStokSiparisListItem i) => i.miktar2)));
			break;
		case enum_siralama_secenekleri.Miktar3:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderByDescending<RaporStokSiparisListItem, double>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, double>)((RaporStokSiparisListItem i) => i.miktar3)));
			break;
		case enum_siralama_secenekleri.Tutar1:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderByDescending<RaporStokSiparisListItem, double>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, double>)((RaporStokSiparisListItem i) => i.tutar1)));
			break;
		case enum_siralama_secenekleri.Tutar2:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderByDescending<RaporStokSiparisListItem, double>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, double>)((RaporStokSiparisListItem i) => i.tutar2)));
			break;
		case enum_siralama_secenekleri.Tutar3:
			list = Enumerable.ToList<RaporStokSiparisListItem>((IEnumerable<RaporStokSiparisListItem>)Enumerable.OrderByDescending<RaporStokSiparisListItem, double>((IEnumerable<RaporStokSiparisListItem>)list, (Func<RaporStokSiparisListItem, double>)((RaporStokSiparisListItem i) => i.tutar3)));
			break;
		}
		return list;
	}
}
