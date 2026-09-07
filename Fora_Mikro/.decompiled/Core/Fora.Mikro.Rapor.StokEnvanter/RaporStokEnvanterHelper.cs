using System;
using System.Collections.Generic;
using System.Linq;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokEnvanter;

public static class RaporStokEnvanterHelper
{
	public static List<RaporStokEnvanterListItem> RaporGruplandir(RaporStokEnvanterSonuc Rapor, RaporStokEnvanterSonucHam RaporHam)
	{
		List<RaporStokEnvanterListItem> list = new List<RaporStokEnvanterListItem>();
		foreach (RaporStokEnvanterSatir item in RaporHam.sonuc_ham)
		{
			int num = 0;
			int num2 = -1;
			foreach (RaporStokEnvanterListItem item2 in list)
			{
				switch (Rapor.gruplandirma_secenegi)
				{
				case enum_stok_envanter_gruplandirma_secenekleri.Stok:
					if (item.stok_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokAnaGrubu:
					if (RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokKategori:
					if (RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokMarka:
					if (RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokReyon:
					if (RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokUretici:
					if (RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no == item2.liste_sira_no)
					{
						num2 = num;
					}
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.Depo:
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
				RaporStokEnvanterListItem raporStokEnvanterListItem = new RaporStokEnvanterListItem();
				switch (Rapor.gruplandirma_secenegi)
				{
				case enum_stok_envanter_gruplandirma_secenekleri.Stok:
					raporStokEnvanterListItem.liste_sira_no = item.stok_sira_no;
					raporStokEnvanterListItem.kodu = RaporHam.stoklar[item.stok_sira_no].Kodu;
					raporStokEnvanterListItem.adi = RaporHam.stoklar[item.stok_sira_no].Adi;
					raporStokEnvanterListItem.birim_adi = RaporHam.stoklar[item.stok_sira_no].Birim1Adi;
					raporStokEnvanterListItem.birim2_adi = RaporHam.stoklar[item.stok_sira_no].Birim2Adi;
					raporStokEnvanterListItem.birim3_adi = RaporHam.stoklar[item.stok_sira_no].Birim3Adi;
					raporStokEnvanterListItem.doviz_cinsi = RaporHam.stoklar[item.stok_sira_no].stok_doviz_cinsi;
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokAnaGrubu:
					raporStokEnvanterListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no;
					raporStokEnvanterListItem.kodu = RaporHam.stok_ana_gruplari[RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no].Kodu;
					raporStokEnvanterListItem.adi = RaporHam.stok_ana_gruplari[RaporHam.stoklar[item.stok_sira_no].stok_anagrup_sira_no].Adi;
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokKategori:
					raporStokEnvanterListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no;
					raporStokEnvanterListItem.kodu = RaporHam.stok_kategorileri[RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no].Kodu;
					raporStokEnvanterListItem.adi = RaporHam.stok_kategorileri[RaporHam.stoklar[item.stok_sira_no].stok_kategori_sira_no].Adi;
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokMarka:
					raporStokEnvanterListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no;
					raporStokEnvanterListItem.kodu = RaporHam.stok_markalari[RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no].Kodu;
					raporStokEnvanterListItem.adi = RaporHam.stok_markalari[RaporHam.stoklar[item.stok_sira_no].stok_marka_sira_no].Adi;
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokReyon:
					raporStokEnvanterListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no;
					raporStokEnvanterListItem.kodu = RaporHam.stok_reyonlari[RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no].Kodu;
					raporStokEnvanterListItem.adi = RaporHam.stok_reyonlari[RaporHam.stoklar[item.stok_sira_no].stok_reyon_sira_no].Adi;
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.StokUretici:
					raporStokEnvanterListItem.liste_sira_no = RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no;
					raporStokEnvanterListItem.kodu = RaporHam.stok_ureticileri[RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no].Kodu;
					raporStokEnvanterListItem.adi = RaporHam.stok_ureticileri[RaporHam.stoklar[item.stok_sira_no].stok_uretici_sira_no].Adi;
					break;
				case enum_stok_envanter_gruplandirma_secenekleri.Depo:
					raporStokEnvanterListItem.liste_sira_no = item.depo_sira_no;
					raporStokEnvanterListItem.kodu = RaporHam.depolar[item.depo_sira_no].Kodu;
					raporStokEnvanterListItem.adi = RaporHam.depolar[item.depo_sira_no].Adi;
					break;
				}
				raporStokEnvanterListItem.miktar1 = 0.0;
				raporStokEnvanterListItem.miktar2 = 0.0;
				raporStokEnvanterListItem.miktar3 = 0.0;
				raporStokEnvanterListItem.tutar1 = 0.0;
				raporStokEnvanterListItem.tutar2 = 0.0;
				raporStokEnvanterListItem.tutar3 = 0.0;
				if (raporStokEnvanterListItem.adi == "")
				{
					raporStokEnvanterListItem.adi = "TANIMSIZ";
				}
				list.Add(raporStokEnvanterListItem);
				num2 = list.Count - 1;
			}
			switch (Rapor.miktar1_secenek)
			{
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim1:
				list[num2].miktar1 += item.miktar;
				break;
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim2:
				list[num2].miktar1 += item.miktar_birim2;
				break;
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim3:
				list[num2].miktar1 += item.miktar_birim3;
				break;
			}
			switch (Rapor.miktar2_secenek)
			{
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim1:
				list[num2].miktar2 += item.miktar;
				break;
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim2:
				list[num2].miktar2 += item.miktar_birim2;
				break;
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim3:
				list[num2].miktar2 += item.miktar_birim3;
				break;
			}
			switch (Rapor.miktar3_secenek)
			{
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim1:
				list[num2].miktar3 += item.miktar;
				break;
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim2:
				list[num2].miktar3 += item.miktar_birim2;
				break;
			case enum_stok_envanter_miktar_secenekleri.MiktarBirim3:
				list[num2].miktar3 += item.miktar_birim3;
				break;
			}
			list[num2].net_tutar += item.tutar;
			enum_stok_envanter_tutar_secenekleri tutar1_secenek = Rapor.tutar1_secenek;
			if (tutar1_secenek == enum_stok_envanter_tutar_secenekleri.Tutar)
			{
				list[num2].tutar1 += item.tutar;
			}
			tutar1_secenek = Rapor.tutar2_secenek;
			if (tutar1_secenek == enum_stok_envanter_tutar_secenekleri.Tutar)
			{
				list[num2].tutar2 += item.tutar;
			}
			tutar1_secenek = Rapor.tutar3_secenek;
			if (tutar1_secenek == enum_stok_envanter_tutar_secenekleri.Tutar)
			{
				list[num2].tutar3 += item.tutar;
			}
		}
		switch (Rapor.siralama_secenegi)
		{
		case enum_siralama_secenekleri.Isim:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderBy<RaporStokEnvanterListItem, string>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, string>)((RaporStokEnvanterListItem i) => i.adi)));
			break;
		case enum_siralama_secenekleri.Kod:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderBy<RaporStokEnvanterListItem, string>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, string>)((RaporStokEnvanterListItem i) => i.kodu)));
			break;
		case enum_siralama_secenekleri.Miktar1:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderByDescending<RaporStokEnvanterListItem, double>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, double>)((RaporStokEnvanterListItem i) => i.miktar1)));
			break;
		case enum_siralama_secenekleri.Miktar2:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderByDescending<RaporStokEnvanterListItem, double>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, double>)((RaporStokEnvanterListItem i) => i.miktar2)));
			break;
		case enum_siralama_secenekleri.Miktar3:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderByDescending<RaporStokEnvanterListItem, double>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, double>)((RaporStokEnvanterListItem i) => i.miktar3)));
			break;
		case enum_siralama_secenekleri.Tutar1:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderByDescending<RaporStokEnvanterListItem, double>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, double>)((RaporStokEnvanterListItem i) => i.tutar1)));
			break;
		case enum_siralama_secenekleri.Tutar2:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderByDescending<RaporStokEnvanterListItem, double>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, double>)((RaporStokEnvanterListItem i) => i.tutar2)));
			break;
		case enum_siralama_secenekleri.Tutar3:
			list = Enumerable.ToList<RaporStokEnvanterListItem>((IEnumerable<RaporStokEnvanterListItem>)Enumerable.OrderByDescending<RaporStokEnvanterListItem, double>((IEnumerable<RaporStokEnvanterListItem>)list, (Func<RaporStokEnvanterListItem, double>)((RaporStokEnvanterListItem i) => i.tutar3)));
			break;
		}
		return list;
	}
}
