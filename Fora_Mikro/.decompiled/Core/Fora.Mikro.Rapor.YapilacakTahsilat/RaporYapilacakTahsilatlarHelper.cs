using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Fora.Mikro.Dovizler;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public static class RaporYapilacakTahsilatlarHelper
{
	public static List<RaporYapilacakTahsilatlarListItem> RaporGruplandir(RaporYapilacakTahsilatlarSonuc Rapor, RaporYapilacakTahsilatlarSonucHam RaporHam, DovizCinsiTanimlari doviz_cinsleri)
	{
		List<RaporYapilacakTahsilatlarListItem> list = new List<RaporYapilacakTahsilatlarListItem>();
		foreach (RaporYapilacakTahsilatlarSatir item in RaporHam.sonuc_ham)
		{
			int num = 0;
			int num2 = -1;
			foreach (RaporYapilacakTahsilatlarListItem item2 in list)
			{
				switch (Rapor.gruplandirma_secenegi)
				{
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Ay:
					if (item.vade_tarihi.Year + "." + item.vade_tarihi.Month.ToString("d2") == item2.kodu && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Cari:
					if (item.cari_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.CariBolge:
					if (RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.CariGrup:
					if (RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Gun:
					if (item.vade_tarihi.Year + "." + item.vade_tarihi.Month.ToString("d2") + "." + item.vade_tarihi.Day.ToString("d2") == item2.kodu && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Proje:
					if (item.proje_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.SorumlulukMerkezi:
					if (item.sorumluluk_merkezi_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Temsilci:
					if (item.plasiyer_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Firma:
					if (item.firma_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Sube:
					if (item.sube_sira_no == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
					{
						num2 = num;
					}
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.DovizCinsi:
					if (item.doviz_cinsi == item2.liste_sira_no && item.doviz_cinsi == item2.meblag_ve_tarihler.doviz_cinsi)
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
				RaporYapilacakTahsilatlarListItem raporYapilacakTahsilatlarListItem = new RaporYapilacakTahsilatlarListItem();
				switch (Rapor.gruplandirma_secenegi)
				{
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Ay:
				{
					raporYapilacakTahsilatlarListItem.kodu = item.vade_tarihi.Year + "." + item.vade_tarihi.Month.ToString("d2");
					CultureInfo cultureInfo = new CultureInfo("tr-TR");
					raporYapilacakTahsilatlarListItem.adi = item.vade_tarihi.ToString("MMMM", cultureInfo) + " " + item.vade_tarihi.Year;
					break;
				}
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Cari:
					raporYapilacakTahsilatlarListItem.liste_sira_no = item.cari_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.cariler[item.cari_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.cariler[item.cari_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.CariBolge:
					raporYapilacakTahsilatlarListItem.liste_sira_no = RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.cari_bolgeleri[RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.cari_bolgeleri[RaporHam.cariler[item.cari_sira_no].cari_bolge_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.CariGrup:
					raporYapilacakTahsilatlarListItem.liste_sira_no = RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.cari_gruplari[RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.cari_gruplari[RaporHam.cariler[item.cari_sira_no].cari_grup_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Gun:
				{
					string adi = (raporYapilacakTahsilatlarListItem.kodu = item.vade_tarihi.Year + "." + item.vade_tarihi.Month.ToString("d2") + "." + item.vade_tarihi.Day.ToString("d2"));
					raporYapilacakTahsilatlarListItem.adi = adi;
					break;
				}
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Proje:
					raporYapilacakTahsilatlarListItem.liste_sira_no = item.proje_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.projeler[item.proje_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.projeler[item.proje_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.SorumlulukMerkezi:
					raporYapilacakTahsilatlarListItem.liste_sira_no = item.sorumluluk_merkezi_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.sorumluluk_merkezleri[item.sorumluluk_merkezi_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.sorumluluk_merkezleri[item.sorumluluk_merkezi_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Temsilci:
					raporYapilacakTahsilatlarListItem.liste_sira_no = item.plasiyer_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.temsilciler[item.plasiyer_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.temsilciler[item.plasiyer_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Firma:
					raporYapilacakTahsilatlarListItem.liste_sira_no = item.firma_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.firmalar[item.firma_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.firmalar[item.firma_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Sube:
					raporYapilacakTahsilatlarListItem.liste_sira_no = item.sube_sira_no;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.subeler[item.sube_sira_no].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.subeler[item.sube_sira_no].Adi;
					break;
				case enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.DovizCinsi:
					raporYapilacakTahsilatlarListItem.liste_sira_no = item.doviz_cinsi;
					raporYapilacakTahsilatlarListItem.kodu = RaporHam.doviz_cinsleri[item.doviz_cinsi].Kodu;
					raporYapilacakTahsilatlarListItem.adi = RaporHam.doviz_cinsleri[item.doviz_cinsi].Adi;
					break;
				}
				if (raporYapilacakTahsilatlarListItem.adi == "")
				{
					raporYapilacakTahsilatlarListItem.adi = "TANIMSIZ";
				}
				list.Add(raporYapilacakTahsilatlarListItem);
				num2 = list.Count - 1;
			}
			list[num2].meblag_ve_tarihler.doviz_cinsi = item.doviz_cinsi;
			list[num2].meblag_ve_tarihler.DovizKodu = doviz_cinsleri.GetDovizCinsiTanimi(item.doviz_cinsi).Kur_sembol;
			list[num2].meblag_ve_tarihler.meblaglar.Add(item.meblag);
			list[num2].meblag_ve_tarihler.vade_tarihleri.Add(item.vade_tarihi);
		}
		switch (Rapor.siralama_secenegi)
		{
		case enum_yapilacak_tahsilatlar_siralama_secenekleri.Isim:
			list = Enumerable.ToList<RaporYapilacakTahsilatlarListItem>((IEnumerable<RaporYapilacakTahsilatlarListItem>)Enumerable.OrderBy<RaporYapilacakTahsilatlarListItem, string>((IEnumerable<RaporYapilacakTahsilatlarListItem>)list, (Func<RaporYapilacakTahsilatlarListItem, string>)((RaporYapilacakTahsilatlarListItem i) => i.adi)));
			break;
		case enum_yapilacak_tahsilatlar_siralama_secenekleri.Kod:
			list = Enumerable.ToList<RaporYapilacakTahsilatlarListItem>((IEnumerable<RaporYapilacakTahsilatlarListItem>)Enumerable.OrderBy<RaporYapilacakTahsilatlarListItem, string>((IEnumerable<RaporYapilacakTahsilatlarListItem>)list, (Func<RaporYapilacakTahsilatlarListItem, string>)((RaporYapilacakTahsilatlarListItem i) => i.kodu)));
			break;
		case enum_yapilacak_tahsilatlar_siralama_secenekleri.Tutar:
			list = Enumerable.ToList<RaporYapilacakTahsilatlarListItem>((IEnumerable<RaporYapilacakTahsilatlarListItem>)Enumerable.OrderByDescending<RaporYapilacakTahsilatlarListItem, double>((IEnumerable<RaporYapilacakTahsilatlarListItem>)list, (Func<RaporYapilacakTahsilatlarListItem, double>)((RaporYapilacakTahsilatlarListItem i) => i.ToplamMeblag)));
			break;
		case enum_yapilacak_tahsilatlar_siralama_secenekleri.OrtalamaVade:
			list = Enumerable.ToList<RaporYapilacakTahsilatlarListItem>((IEnumerable<RaporYapilacakTahsilatlarListItem>)Enumerable.OrderBy<RaporYapilacakTahsilatlarListItem, DateTime>((IEnumerable<RaporYapilacakTahsilatlarListItem>)list, (Func<RaporYapilacakTahsilatlarListItem, DateTime>)((RaporYapilacakTahsilatlarListItem i) => i.OrtalamaVade)));
			break;
		}
		return list;
	}
}
