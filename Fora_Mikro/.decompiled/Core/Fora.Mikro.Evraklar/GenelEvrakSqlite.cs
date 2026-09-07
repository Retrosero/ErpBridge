using System;
using System.Collections.Generic;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.DepolarArasiSiparisler;
using Fora.Mikro.Enumler;
using Fora.Mikro.Hizmetler;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Projeler;
using Fora.Mikro.Siparis;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.StokAltGruplari;
using Fora.Mikro.Stoklar.StokAnaGruplari;
using Fora.Mikro.Stoklar.StokMarkalari;
using Fora.Mikro.Stoklar.StokModelleri;
using Fora.Mikro.Stoklar.StokReyonlari;
using Fora.Mikro.Stoklar.StokSektorleri;
using Fora.Mikro.Stoklar.StokUreticileri;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Evraklar;

public static class GenelEvrakSqlite
{
	public static GenelEvrak ConvertEvrak(SqliteConnection openedconnection, Evrak evrak, enum_satir_gruplandirma_secenekleri gruplandirma_secenekleri)
	{
		GenelEvrak genelEvrak = new GenelEvrak();
		genelEvrak.evrak_tipi = evrak.evraktipi;
		genelEvrak.evrak_tarih = evrak.EvrakTarihi;
		genelEvrak.evrakno_seri = evrak.EvrakNoSeri;
		genelEvrak.evrakno_sira = evrak.EvrakNoSira;
		genelEvrak.belge_no = evrak.BelgeNo;
		genelEvrak.belge_tarih = evrak.BelgeTarihi;
		genelEvrak.cari = evrak.cari;
		genelEvrak.temsilci = CariPersonelSqlite.GetCariPersonel(openedconnection, evrak.TemsilciKodu);
		genelEvrak.doviz_cinsi = evrak.dovizcinsi;
		genelEvrak.kur = evrak.kur;
		genelEvrak.alternatif_doviz_cinsi = evrak.alternatifdovizcinsi;
		genelEvrak.alternatif_doviz_kuru = evrak.alternatifdovizkuru;
		genelEvrak.cikis_depo = evrak.KaynakDepo;
		genelEvrak.giris_depo = evrak.HedefDepo;
		genelEvrak.nakliye_depo = evrak.NakliyeDepo;
		genelEvrak.proje = evrak.proje;
		genelEvrak.sorumluluk_merkezi = evrak.sorumlulukmerkezi;
		genelEvrak.sevk_adres_no = evrak.sevkadresno;
		genelEvrak.aciklama1 = evrak.aciklama1;
		genelEvrak.aciklama2 = evrak.aciklama2;
		genelEvrak.aciklama3 = evrak.aciklama3;
		genelEvrak.aciklama4 = evrak.aciklama4;
		genelEvrak.aciklama5 = evrak.aciklama5;
		genelEvrak.aciklama6 = evrak.aciklama6;
		genelEvrak.aciklama7 = evrak.aciklama7;
		genelEvrak.aciklama8 = evrak.aciklama8;
		genelEvrak.aciklama9 = evrak.aciklama9;
		genelEvrak.aciklama10 = evrak.aciklama10;
		genelEvrak.satirlar = new List<GenelEvrakSatirlari>();
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			genelEvrak.satirlar = ConvertSiparisSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			genelEvrak.satirlar = ConvertFaturaSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			genelEvrak.satirlar = ConvertFaturaSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			genelEvrak.satirlar = ConvertSiparisSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			genelEvrak.satirlar = ConvertFaturaSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			genelEvrak.satirlar = ConvertFaturaSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			genelEvrak.satirlar = ConvertTahsilatSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.Tediye:
			genelEvrak.satirlar = ConvertTahsilatSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			genelEvrak.satirlar = ConvertFaturaSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			genelEvrak.satirlar = ConvertFaturaSatirlar(openedconnection, evrak);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			genelEvrak.satirlar = ConvertDepolarArasiSiparisSatirlar(openedconnection, evrak);
			break;
		}
		if (gruplandirma_secenekleri != enum_satir_gruplandirma_secenekleri.StokKodu)
		{
			List<GenelEvrakSatirlari> list = new List<GenelEvrakSatirlari>();
			foreach (GenelEvrakSatirlari item in genelEvrak.satirlar)
			{
				int num = -1;
				string text = "";
				string hesap_adi = "";
				switch (gruplandirma_secenekleri)
				{
				case enum_satir_gruplandirma_secenekleri.AltGrup:
					text = item.altgrup_kod;
					hesap_adi = item.altgrup_adi;
					break;
				case enum_satir_gruplandirma_secenekleri.AnaGrup:
					text = item.anagrup_kod;
					hesap_adi = item.anagrup_adi;
					break;
				case enum_satir_gruplandirma_secenekleri.Marka:
					text = item.marka_kodu;
					hesap_adi = item.marka_adi;
					break;
				case enum_satir_gruplandirma_secenekleri.Model:
					text = item.model_kodu;
					hesap_adi = item.model_adi;
					break;
				case enum_satir_gruplandirma_secenekleri.Reyon:
					text = item.reyon_kodu;
					hesap_adi = item.reyon_adi;
					break;
				case enum_satir_gruplandirma_secenekleri.Sektor:
					text = item.sektor_kodu;
					hesap_adi = item.sektor_adi;
					break;
				case enum_satir_gruplandirma_secenekleri.Uretici:
					text = item.uretici_kodu;
					hesap_adi = item.uretici_adi;
					break;
				}
				int num2 = 0;
				foreach (GenelEvrakSatirlari item2 in list)
				{
					if (text == item2.hesap_kodu)
					{
						num = num2;
						break;
					}
					num2++;
				}
				if (num == -1)
				{
					GenelEvrakSatirlari genelEvrakSatirlari = GenelEvrakSatirlari.ReadFromByteArray(GenelEvrakSatirlari.WriteToByteArray(item, 999));
					genelEvrakSatirlari.hesap_kodu = text;
					genelEvrakSatirlari.hesap_adi = hesap_adi;
					list.Add(genelEvrakSatirlari);
					continue;
				}
				double birim_fiyat_brut = (list[num].birim_fiyat_brut * list[num].miktar + item.birim_fiyat_brut * item.miktar) / (list[num].miktar + item.miktar);
				list[num].birim_fiyat_brut = birim_fiyat_brut;
				list[num].miktar += item.miktar;
				list[num].iskonto_1_tutari += item.iskonto_1_tutari;
				list[num].iskonto_2_tutari += item.iskonto_2_tutari;
				list[num].iskonto_3_tutari += item.iskonto_3_tutari;
				list[num].iskonto_4_tutari += item.iskonto_4_tutari;
				list[num].iskonto_5_tutari += item.iskonto_5_tutari;
				list[num].iskonto_6_tutari += item.iskonto_6_tutari;
				list[num].masraf_1_tutari += item.masraf_1_tutari;
				list[num].masraf_2_tutari += item.masraf_2_tutari;
				list[num].masraf_3_tutari += item.masraf_3_tutari;
				list[num].masraf_4_tutari += item.masraf_4_tutari;
				list[num].masraf_vergi_tutari += item.masraf_vergi_tutari;
				list[num].miktar2 += item.miktar2;
				list[num].oiv_vergi += item.oiv_vergi;
				list[num].otv_vergi += item.otv_vergi;
				list[num].tutar += item.tutar;
				list[num].vergi_tutari += item.vergi_tutari;
			}
			genelEvrak.satirlar = list;
		}
		return genelEvrak;
	}

	private static List<GenelEvrakSatirlari> ConvertSiparisSatirlar(SqliteConnection openedconnection, Evrak evrak)
	{
		List<GenelEvrakSatirlari> list = new List<GenelEvrakSatirlari>();
		foreach (SIPARISLER item in evrak.GetSiparisler())
		{
			GenelEvrakSatirlari genelEvrakSatirlari = new GenelEvrakSatirlari();
			genelEvrakSatirlari.satir_cinsi = GenelEvrakSatirCinsleri.Stok;
			Stok stok = StokSqlite.GetStok(openedconnection, item.sip_stok_kod, enum_toptan_perakende.Toptan);
			genelEvrakSatirlari.hesap_kodu = item.sip_stok_kod;
			genelEvrakSatirlari.parti_kodu = item.sip_parti_kodu;
			genelEvrakSatirlari.satir_aciklama = item.sip_aciklama;
			genelEvrakSatirlari.satir_aciklama2 = item.sip_aciklama2;
			genelEvrakSatirlari.doviz_cinsi = item.sip_doviz_cinsi;
			genelEvrakSatirlari.kur = new Kur();
			genelEvrakSatirlari.kur.dov_no = item.sip_doviz_cinsi;
			genelEvrakSatirlari.kur.dov_fiyat = item.sip_doviz_kuru;
			genelEvrakSatirlari.sevk_teslim_tarihi = item.sip_teslim_tarih;
			genelEvrakSatirlari.birim_fiyat_brut = item.sip_b_fiyat;
			genelEvrakSatirlari.birim_pntr = item.sip_birim_pntr;
			genelEvrakSatirlari.miktar = item.sip_miktar;
			genelEvrakSatirlari.miktar2 = 0.0;
			genelEvrakSatirlari.cari_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.sip_cari_sormerk);
			genelEvrakSatirlari.stok_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.sip_stok_sormerk);
			genelEvrakSatirlari.karsi_sorumluluk_merkezi = new SorumlulukMerkezi();
			genelEvrakSatirlari.proje = ProjeSqlite.GetProje(openedconnection, item.sip_projekodu);
			genelEvrakSatirlari.iskonto_1_uygulama_sekli = item.sip_iskonto1;
			genelEvrakSatirlari.iskonto_1_tutari = item.sip_iskonto_1;
			genelEvrakSatirlari.iskonto_2_uygulama_sekli = item.sip_iskonto2;
			genelEvrakSatirlari.iskonto_2_tutari = item.sip_iskonto_2;
			genelEvrakSatirlari.iskonto_3_uygulama_sekli = item.sip_iskonto3;
			genelEvrakSatirlari.iskonto_3_tutari = item.sip_iskonto_3;
			genelEvrakSatirlari.iskonto_4_uygulama_sekli = item.sip_iskonto4;
			genelEvrakSatirlari.iskonto_4_tutari = item.sip_iskonto_4;
			genelEvrakSatirlari.iskonto_5_uygulama_sekli = item.sip_iskonto5;
			genelEvrakSatirlari.iskonto_5_tutari = item.sip_iskonto_5;
			genelEvrakSatirlari.iskonto_6_uygulama_sekli = item.sip_iskonto6;
			genelEvrakSatirlari.iskonto_6_tutari = item.sip_iskonto_6;
			genelEvrakSatirlari.masraf_1_uygulama_sekli = item.sip_masraf1;
			genelEvrakSatirlari.masraf_1_tutari = item.sip_masraf_1;
			genelEvrakSatirlari.masraf_2_uygulama_sekli = item.sip_masraf2;
			genelEvrakSatirlari.masraf_2_tutari = item.sip_masraf_2;
			genelEvrakSatirlari.masraf_3_uygulama_sekli = item.sip_masraf3;
			genelEvrakSatirlari.masraf_3_tutari = item.sip_masraf_3;
			genelEvrakSatirlari.masraf_4_uygulama_sekli = item.sip_masraf4;
			genelEvrakSatirlari.masraf_4_tutari = item.sip_masraf_4;
			genelEvrakSatirlari.vergi_pntr = item.sip_vergi_pntr;
			genelEvrakSatirlari.vergi_tutari = item.sip_vergi;
			genelEvrakSatirlari.masraf_vergi_pntr = item.sip_masvergi_pntr;
			genelEvrakSatirlari.masraf_vergi_tutari = item.sip_masvergi;
			genelEvrakSatirlari.otv_pntr = item.sip_Otv_Pntr;
			genelEvrakSatirlari.otv_vergi = item.sip_Otv_Vergi;
			genelEvrakSatirlari.oiv_pntr = 1;
			genelEvrakSatirlari.oiv_vergi = 0.0;
			genelEvrakSatirlari.hesap_adi = stok.sto_isim;
			genelEvrakSatirlari.hesap_yabanci_adi = stok.sto_yabanci_isim;
			genelEvrakSatirlari.hesap_kisa_adi = stok.sto_kisa_ismi;
			genelEvrakSatirlari.altgrup_kod = stok.sto_altgrup_kod;
			genelEvrakSatirlari.altgrup_adi = StokAltGrupSqlite.GetAltGrupAdi(openedconnection, genelEvrakSatirlari.altgrup_kod, stok.sto_anagrup_kod);
			genelEvrakSatirlari.anagrup_kod = stok.sto_anagrup_kod;
			genelEvrakSatirlari.anagrup_adi = StokAnaGrupSqlite.GetAnaGrupAdi(openedconnection, genelEvrakSatirlari.anagrup_kod);
			genelEvrakSatirlari.sektor_kodu = stok.sto_sektor_kodu;
			genelEvrakSatirlari.sektor_adi = StokSektorSqlite.GetSektorAdi(openedconnection, genelEvrakSatirlari.sektor_kodu);
			genelEvrakSatirlari.marka_kodu = stok.sto_marka_kodu;
			genelEvrakSatirlari.marka_adi = StokMarkaSqlite.GetMarkaAdi(openedconnection, genelEvrakSatirlari.marka_kodu);
			genelEvrakSatirlari.model_kodu = stok.sto_model_kodu;
			genelEvrakSatirlari.model_adi = StokModelSqlite.GetModelAdi(openedconnection, genelEvrakSatirlari.model_kodu);
			genelEvrakSatirlari.uretici_kodu = stok.sto_uretici_kodu;
			genelEvrakSatirlari.uretici_adi = StokUreticiSqlite.GetUreticiAdi(openedconnection, genelEvrakSatirlari.uretici_kodu);
			genelEvrakSatirlari.reyon_kodu = stok.sto_reyon_kodu;
			genelEvrakSatirlari.reyon_adi = StokReyonSqlite.GetReyonAdi(openedconnection, genelEvrakSatirlari.uretici_kodu);
			genelEvrakSatirlari.yer_kodu = stok.sto_yer_kod;
			genelEvrakSatirlari.birim1_ad = stok.sto_birim1_ad;
			genelEvrakSatirlari.birim2_ad = stok.sto_birim2_ad;
			genelEvrakSatirlari.birim3_ad = stok.sto_birim3_ad;
			genelEvrakSatirlari.birim4_ad = stok.sto_birim4_ad;
			genelEvrakSatirlari.birim1_katsayi = stok.sto_birim1_katsayi;
			genelEvrakSatirlari.birim2_katsayi = stok.sto_birim2_katsayi;
			genelEvrakSatirlari.birim3_katsayi = stok.sto_birim3_katsayi;
			genelEvrakSatirlari.birim4_katsayi = stok.sto_birim4_katsayi;
			try
			{
				foreach (string item2 in StokSqlite.GetStokBarkodlari(openedconnection, item.sip_stok_kod))
				{
					string[] array = item2.Split(new char[1] { '|' });
					switch (array[0])
					{
					case "1":
						genelEvrakSatirlari.birim1_barkod = array[1];
						break;
					case "2":
						genelEvrakSatirlari.birim2_barkod = array[1];
						break;
					case "3":
						genelEvrakSatirlari.birim3_barkod = array[1];
						break;
					case "4":
						genelEvrakSatirlari.birim4_barkod = array[1];
						break;
					}
				}
			}
			catch
			{
			}
			list.Add(genelEvrakSatirlari);
		}
		return list;
	}

	private static List<GenelEvrakSatirlari> ConvertDepolarArasiSiparisSatirlar(SqliteConnection openedconnection, Evrak evrak)
	{
		List<GenelEvrakSatirlari> list = new List<GenelEvrakSatirlari>();
		foreach (DEPOLAR_ARASI_SIPARISLER item in evrak.GetDepolarArasiSiparisHareketleri())
		{
			GenelEvrakSatirlari genelEvrakSatirlari = new GenelEvrakSatirlari();
			genelEvrakSatirlari.satir_cinsi = GenelEvrakSatirCinsleri.Stok;
			Stok stok = StokSqlite.GetStok(openedconnection, item.ssip_stok_kod, enum_toptan_perakende.Toptan);
			genelEvrakSatirlari.hesap_kodu = item.ssip_stok_kod;
			genelEvrakSatirlari.parti_kodu = item.ssip_paket_kod;
			genelEvrakSatirlari.satir_aciklama = item.ssip_aciklama;
			genelEvrakSatirlari.satir_aciklama2 = "";
			genelEvrakSatirlari.doviz_cinsi = 1;
			genelEvrakSatirlari.kur = new Kur();
			genelEvrakSatirlari.kur.dov_no = 1;
			genelEvrakSatirlari.kur.dov_fiyat = 1.0;
			genelEvrakSatirlari.sevk_teslim_tarihi = item.ssip_teslim_tarih;
			genelEvrakSatirlari.birim_fiyat_brut = item.ssip_b_fiyat;
			genelEvrakSatirlari.birim_pntr = item.ssip_birim_pntr;
			genelEvrakSatirlari.miktar = item.ssip_miktar;
			genelEvrakSatirlari.miktar2 = 0.0;
			genelEvrakSatirlari.cari_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.ssip_sormerkezi);
			genelEvrakSatirlari.stok_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.ssip_sormerkezi);
			genelEvrakSatirlari.karsi_sorumluluk_merkezi = new SorumlulukMerkezi();
			genelEvrakSatirlari.proje = ProjeSqlite.GetProje(openedconnection, item.ssip_projekodu);
			genelEvrakSatirlari.iskonto_1_uygulama_sekli = 1;
			genelEvrakSatirlari.iskonto_1_tutari = 0.0;
			genelEvrakSatirlari.iskonto_2_uygulama_sekli = 1;
			genelEvrakSatirlari.iskonto_2_tutari = 0.0;
			genelEvrakSatirlari.iskonto_3_uygulama_sekli = 1;
			genelEvrakSatirlari.iskonto_3_tutari = 0.0;
			genelEvrakSatirlari.iskonto_4_uygulama_sekli = 1;
			genelEvrakSatirlari.iskonto_4_tutari = 0.0;
			genelEvrakSatirlari.iskonto_5_uygulama_sekli = 1;
			genelEvrakSatirlari.iskonto_5_tutari = 0.0;
			genelEvrakSatirlari.iskonto_6_uygulama_sekli = 1;
			genelEvrakSatirlari.iskonto_6_tutari = 0.0;
			genelEvrakSatirlari.masraf_1_uygulama_sekli = 1;
			genelEvrakSatirlari.masraf_1_tutari = 0.0;
			genelEvrakSatirlari.masraf_2_uygulama_sekli = 1;
			genelEvrakSatirlari.masraf_2_tutari = 0.0;
			genelEvrakSatirlari.masraf_3_uygulama_sekli = 1;
			genelEvrakSatirlari.masraf_3_tutari = 0.0;
			genelEvrakSatirlari.masraf_4_uygulama_sekli = 1;
			genelEvrakSatirlari.masraf_4_tutari = 0.0;
			genelEvrakSatirlari.vergi_pntr = 1;
			genelEvrakSatirlari.vergi_tutari = 0.0;
			genelEvrakSatirlari.masraf_vergi_pntr = 1;
			genelEvrakSatirlari.masraf_vergi_tutari = 0.0;
			genelEvrakSatirlari.otv_pntr = 1;
			genelEvrakSatirlari.otv_vergi = 0.0;
			genelEvrakSatirlari.oiv_pntr = 1;
			genelEvrakSatirlari.oiv_vergi = 0.0;
			genelEvrakSatirlari.hesap_adi = stok.sto_isim;
			genelEvrakSatirlari.hesap_yabanci_adi = stok.sto_yabanci_isim;
			genelEvrakSatirlari.hesap_kisa_adi = stok.sto_kisa_ismi;
			genelEvrakSatirlari.altgrup_kod = stok.sto_altgrup_kod;
			genelEvrakSatirlari.altgrup_adi = StokAltGrupSqlite.GetAltGrupAdi(openedconnection, genelEvrakSatirlari.altgrup_kod, stok.sto_anagrup_kod);
			genelEvrakSatirlari.anagrup_kod = stok.sto_anagrup_kod;
			genelEvrakSatirlari.anagrup_adi = StokAnaGrupSqlite.GetAnaGrupAdi(openedconnection, genelEvrakSatirlari.anagrup_kod);
			genelEvrakSatirlari.sektor_kodu = stok.sto_sektor_kodu;
			genelEvrakSatirlari.sektor_adi = StokSektorSqlite.GetSektorAdi(openedconnection, genelEvrakSatirlari.sektor_kodu);
			genelEvrakSatirlari.marka_kodu = stok.sto_marka_kodu;
			genelEvrakSatirlari.marka_adi = StokMarkaSqlite.GetMarkaAdi(openedconnection, genelEvrakSatirlari.marka_kodu);
			genelEvrakSatirlari.model_kodu = stok.sto_model_kodu;
			genelEvrakSatirlari.model_adi = StokModelSqlite.GetModelAdi(openedconnection, genelEvrakSatirlari.model_kodu);
			genelEvrakSatirlari.uretici_kodu = stok.sto_uretici_kodu;
			genelEvrakSatirlari.uretici_adi = StokUreticiSqlite.GetUreticiAdi(openedconnection, genelEvrakSatirlari.uretici_kodu);
			genelEvrakSatirlari.reyon_kodu = stok.sto_reyon_kodu;
			genelEvrakSatirlari.reyon_adi = StokReyonSqlite.GetReyonAdi(openedconnection, genelEvrakSatirlari.uretici_kodu);
			genelEvrakSatirlari.yer_kodu = stok.sto_yer_kod;
			genelEvrakSatirlari.birim1_ad = stok.sto_birim1_ad;
			genelEvrakSatirlari.birim2_ad = stok.sto_birim2_ad;
			genelEvrakSatirlari.birim3_ad = stok.sto_birim3_ad;
			genelEvrakSatirlari.birim4_ad = stok.sto_birim4_ad;
			genelEvrakSatirlari.birim1_katsayi = stok.sto_birim1_katsayi;
			genelEvrakSatirlari.birim2_katsayi = stok.sto_birim2_katsayi;
			genelEvrakSatirlari.birim3_katsayi = stok.sto_birim3_katsayi;
			genelEvrakSatirlari.birim4_katsayi = stok.sto_birim4_katsayi;
			try
			{
				foreach (string item2 in StokSqlite.GetStokBarkodlari(openedconnection, item.ssip_stok_kod))
				{
					string[] array = item2.Split(new char[1] { '|' });
					switch (array[0])
					{
					case "1":
						genelEvrakSatirlari.birim1_barkod = array[1];
						break;
					case "2":
						genelEvrakSatirlari.birim2_barkod = array[1];
						break;
					case "3":
						genelEvrakSatirlari.birim3_barkod = array[1];
						break;
					case "4":
						genelEvrakSatirlari.birim4_barkod = array[1];
						break;
					}
				}
			}
			catch
			{
			}
			list.Add(genelEvrakSatirlari);
		}
		return list;
	}

	private static List<GenelEvrakSatirlari> ConvertFaturaSatirlar(SqliteConnection openedconnection, Evrak evrak)
	{
		List<GenelEvrakSatirlari> list = new List<GenelEvrakSatirlari>();
		foreach (STOK_HAREKETLERI item in evrak.GetStokHareketleri())
		{
			GenelEvrakSatirlari genelEvrakSatirlari = new GenelEvrakSatirlari();
			genelEvrakSatirlari.satir_cinsi = GenelEvrakSatirCinsleri.Stok;
			enum_toptan_perakende toptan_perakende = enum_toptan_perakende.Toptan;
			switch (evrak.ticaretturu)
			{
			case enum_cha_ticaret_turu.ToptanYurtIciTicaret:
				toptan_perakende = enum_toptan_perakende.Toptan;
				break;
			case enum_cha_ticaret_turu.PerakendeYurtIciTicaret:
				toptan_perakende = enum_toptan_perakende.Perakende;
				break;
			}
			Stok stok = StokSqlite.GetStok(openedconnection, item.sth_stok_kod, toptan_perakende);
			genelEvrakSatirlari.hesap_kodu = item.sth_stok_kod;
			genelEvrakSatirlari.parti_kodu = item.sth_parti_kodu;
			genelEvrakSatirlari.satir_aciklama = item.sth_aciklama;
			genelEvrakSatirlari.satir_aciklama2 = "";
			genelEvrakSatirlari.doviz_cinsi = item.sth_har_doviz_cinsi;
			genelEvrakSatirlari.kur = new Kur();
			genelEvrakSatirlari.kur.dov_no = item.sth_har_doviz_cinsi;
			genelEvrakSatirlari.kur.dov_fiyat = item.sth_har_doviz_kuru;
			genelEvrakSatirlari.sevk_teslim_tarihi = item.sth_malkbl_sevk_tarihi;
			genelEvrakSatirlari.birim_fiyat_brut = item.sth_tutar / item.sth_miktar;
			genelEvrakSatirlari.birim_pntr = item.sth_birim_pntr;
			genelEvrakSatirlari.miktar = item.sth_miktar;
			if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
			{
				genelEvrakSatirlari.miktar *= -1.0;
			}
			genelEvrakSatirlari.miktar2 = item.sth_miktar2;
			genelEvrakSatirlari.cari_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.sth_cari_srm_merkezi);
			genelEvrakSatirlari.stok_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.sth_stok_srm_merkezi);
			genelEvrakSatirlari.karsi_sorumluluk_merkezi = new SorumlulukMerkezi();
			genelEvrakSatirlari.proje = ProjeSqlite.GetProje(openedconnection, item.sth_proje_kodu);
			genelEvrakSatirlari.iskonto_1_uygulama_sekli = item.sth_isk_mas1;
			genelEvrakSatirlari.iskonto_1_tutari = item.sth_iskonto1;
			genelEvrakSatirlari.iskonto_2_uygulama_sekli = item.sth_isk_mas2;
			genelEvrakSatirlari.iskonto_2_tutari = item.sth_iskonto2;
			genelEvrakSatirlari.iskonto_3_uygulama_sekli = item.sth_isk_mas3;
			genelEvrakSatirlari.iskonto_3_tutari = item.sth_iskonto3;
			genelEvrakSatirlari.iskonto_4_uygulama_sekli = item.sth_isk_mas4;
			genelEvrakSatirlari.iskonto_4_tutari = item.sth_iskonto4;
			genelEvrakSatirlari.iskonto_5_uygulama_sekli = item.sth_isk_mas5;
			genelEvrakSatirlari.iskonto_5_tutari = item.sth_iskonto5;
			genelEvrakSatirlari.iskonto_6_uygulama_sekli = item.sth_isk_mas6;
			genelEvrakSatirlari.iskonto_6_tutari = item.sth_iskonto6;
			genelEvrakSatirlari.masraf_1_uygulama_sekli = item.sth_isk_mas7;
			genelEvrakSatirlari.masraf_1_tutari = item.sth_masraf1;
			genelEvrakSatirlari.masraf_2_uygulama_sekli = item.sth_isk_mas8;
			genelEvrakSatirlari.masraf_2_tutari = item.sth_masraf2;
			genelEvrakSatirlari.masraf_3_uygulama_sekli = item.sth_isk_mas9;
			genelEvrakSatirlari.masraf_3_tutari = item.sth_masraf3;
			genelEvrakSatirlari.masraf_4_uygulama_sekli = item.sth_isk_mas10;
			genelEvrakSatirlari.masraf_4_tutari = item.sth_masraf4;
			genelEvrakSatirlari.vergi_pntr = item.sth_vergi_pntr;
			genelEvrakSatirlari.vergi_tutari = item.sth_vergi;
			if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
			{
				genelEvrakSatirlari.vergi_tutari *= -1.0;
			}
			genelEvrakSatirlari.masraf_vergi_pntr = item.sth_masraf_vergi_pntr;
			genelEvrakSatirlari.masraf_vergi_tutari = item.sth_masraf_vergi;
			if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
			{
				genelEvrakSatirlari.masraf_vergi_tutari *= -1.0;
			}
			genelEvrakSatirlari.otv_pntr = item.sth_otv_pntr;
			genelEvrakSatirlari.otv_vergi = item.sth_otvtutari;
			if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
			{
				genelEvrakSatirlari.otv_vergi *= -1.0;
			}
			genelEvrakSatirlari.oiv_pntr = item.sth_oiv_pntr;
			genelEvrakSatirlari.oiv_vergi = item.sth_oivtutari;
			if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
			{
				genelEvrakSatirlari.oiv_vergi *= -1.0;
			}
			genelEvrakSatirlari.hesap_adi = stok.sto_isim;
			genelEvrakSatirlari.hesap_yabanci_adi = stok.sto_yabanci_isim;
			genelEvrakSatirlari.hesap_kisa_adi = stok.sto_kisa_ismi;
			genelEvrakSatirlari.altgrup_kod = stok.sto_altgrup_kod;
			genelEvrakSatirlari.altgrup_adi = StokAltGrupSqlite.GetAltGrupAdi(openedconnection, genelEvrakSatirlari.altgrup_kod, stok.sto_anagrup_kod);
			genelEvrakSatirlari.anagrup_kod = stok.sto_anagrup_kod;
			genelEvrakSatirlari.anagrup_adi = StokAnaGrupSqlite.GetAnaGrupAdi(openedconnection, genelEvrakSatirlari.anagrup_kod);
			genelEvrakSatirlari.sektor_kodu = stok.sto_sektor_kodu;
			genelEvrakSatirlari.sektor_adi = StokSektorSqlite.GetSektorAdi(openedconnection, genelEvrakSatirlari.sektor_kodu);
			genelEvrakSatirlari.marka_kodu = stok.sto_marka_kodu;
			genelEvrakSatirlari.marka_adi = StokMarkaSqlite.GetMarkaAdi(openedconnection, genelEvrakSatirlari.marka_kodu);
			genelEvrakSatirlari.model_kodu = stok.sto_model_kodu;
			genelEvrakSatirlari.model_adi = StokModelSqlite.GetModelAdi(openedconnection, genelEvrakSatirlari.model_kodu);
			genelEvrakSatirlari.uretici_kodu = stok.sto_uretici_kodu;
			genelEvrakSatirlari.uretici_adi = StokUreticiSqlite.GetUreticiAdi(openedconnection, genelEvrakSatirlari.uretici_kodu);
			genelEvrakSatirlari.reyon_kodu = stok.sto_reyon_kodu;
			genelEvrakSatirlari.reyon_adi = StokReyonSqlite.GetReyonAdi(openedconnection, genelEvrakSatirlari.uretici_kodu);
			genelEvrakSatirlari.yer_kodu = stok.sto_yer_kod;
			genelEvrakSatirlari.birim1_ad = stok.sto_birim1_ad;
			genelEvrakSatirlari.birim2_ad = stok.sto_birim2_ad;
			genelEvrakSatirlari.birim3_ad = stok.sto_birim3_ad;
			genelEvrakSatirlari.birim4_ad = stok.sto_birim4_ad;
			genelEvrakSatirlari.birim1_katsayi = stok.sto_birim1_katsayi;
			genelEvrakSatirlari.birim2_katsayi = stok.sto_birim2_katsayi;
			genelEvrakSatirlari.birim3_katsayi = stok.sto_birim3_katsayi;
			genelEvrakSatirlari.birim4_katsayi = stok.sto_birim4_katsayi;
			try
			{
				foreach (string item2 in StokSqlite.GetStokBarkodlari(openedconnection, item.sth_stok_kod))
				{
					string[] array = item2.Split(new char[1] { '|' });
					switch (array[0])
					{
					case "1":
						genelEvrakSatirlari.birim1_barkod = array[1];
						break;
					case "2":
						genelEvrakSatirlari.birim2_barkod = array[1];
						break;
					case "3":
						genelEvrakSatirlari.birim3_barkod = array[1];
						break;
					case "4":
						genelEvrakSatirlari.birim4_barkod = array[1];
						break;
					}
				}
			}
			catch
			{
			}
			list.Add(genelEvrakSatirlari);
		}
		foreach (CARI_HESAP_HAREKETLERI item3 in evrak.GetHizmetHareketleri())
		{
			GenelEvrakSatirlari genelEvrakSatirlari2 = new GenelEvrakSatirlari();
			genelEvrakSatirlari2.satir_cinsi = GenelEvrakSatirCinsleri.Hizmet;
			Hizmet hizmet = HizmetSqlite.GetHizmet(openedconnection, item3.cha_kasa_hizkod);
			genelEvrakSatirlari2.hesap_kodu = item3.cha_kasa_hizkod;
			genelEvrakSatirlari2.parti_kodu = "";
			genelEvrakSatirlari2.satir_aciklama = item3.cha_aciklama;
			genelEvrakSatirlari2.satir_aciklama2 = "";
			genelEvrakSatirlari2.doviz_cinsi = item3.cha_d_cins;
			genelEvrakSatirlari2.kur = new Kur();
			genelEvrakSatirlari2.kur.dov_no = item3.cha_d_cins;
			genelEvrakSatirlari2.kur.dov_fiyat = item3.cha_d_kur;
			genelEvrakSatirlari2.sevk_teslim_tarihi = item3.cha_tarihi;
			genelEvrakSatirlari2.birim_fiyat_brut = item3.cha_aratoplam / item3.cha_miktari;
			genelEvrakSatirlari2.birim_pntr = 1;
			genelEvrakSatirlari2.miktar = item3.cha_miktari;
			genelEvrakSatirlari2.miktar2 = 0.0;
			genelEvrakSatirlari2.cari_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item3.cha_srmrkkodu);
			genelEvrakSatirlari2.stok_sorumluluk_merkezi = new SorumlulukMerkezi();
			genelEvrakSatirlari2.karsi_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item3.cha_karsisrmrkkodu);
			genelEvrakSatirlari2.proje = ProjeSqlite.GetProje(openedconnection, item3.cha_projekodu);
			genelEvrakSatirlari2.iskonto_1_uygulama_sekli = item3.cha_isk_mas1;
			genelEvrakSatirlari2.iskonto_1_tutari = item3.cha_ft_iskonto1;
			genelEvrakSatirlari2.iskonto_2_uygulama_sekli = item3.cha_isk_mas2;
			genelEvrakSatirlari2.iskonto_2_tutari = item3.cha_ft_iskonto2;
			genelEvrakSatirlari2.iskonto_3_uygulama_sekli = item3.cha_isk_mas3;
			genelEvrakSatirlari2.iskonto_3_tutari = item3.cha_ft_iskonto3;
			genelEvrakSatirlari2.iskonto_4_uygulama_sekli = item3.cha_isk_mas4;
			genelEvrakSatirlari2.iskonto_4_tutari = item3.cha_ft_iskonto4;
			genelEvrakSatirlari2.iskonto_5_uygulama_sekli = item3.cha_isk_mas5;
			genelEvrakSatirlari2.iskonto_5_tutari = item3.cha_ft_iskonto5;
			genelEvrakSatirlari2.iskonto_6_uygulama_sekli = item3.cha_isk_mas6;
			genelEvrakSatirlari2.iskonto_6_tutari = item3.cha_ft_iskonto6;
			genelEvrakSatirlari2.masraf_1_uygulama_sekli = item3.cha_isk_mas7;
			genelEvrakSatirlari2.masraf_1_tutari = item3.cha_ft_masraf1;
			genelEvrakSatirlari2.masraf_2_uygulama_sekli = item3.cha_isk_mas8;
			genelEvrakSatirlari2.masraf_2_tutari = item3.cha_ft_masraf2;
			genelEvrakSatirlari2.masraf_3_uygulama_sekli = item3.cha_isk_mas9;
			genelEvrakSatirlari2.masraf_3_tutari = item3.cha_ft_masraf3;
			genelEvrakSatirlari2.masraf_4_uygulama_sekli = item3.cha_isk_mas10;
			genelEvrakSatirlari2.masraf_4_tutari = item3.cha_ft_masraf4;
			genelEvrakSatirlari2.vergi_pntr = item3.cha_vergipntr;
			genelEvrakSatirlari2.vergi_tutari = item3.cha_vergi1 + item3.cha_vergi2 + item3.cha_vergi3 + item3.cha_vergi4 + item3.cha_vergi5 + item3.cha_vergi6 + item3.cha_vergi7 + item3.cha_vergi8 + item3.cha_vergi9 + item3.cha_vergi10;
			genelEvrakSatirlari2.masraf_vergi_pntr = 0;
			genelEvrakSatirlari2.masraf_vergi_tutari = 0.0;
			genelEvrakSatirlari2.otv_pntr = 0;
			genelEvrakSatirlari2.otv_vergi = 0.0;
			genelEvrakSatirlari2.oiv_pntr = 0;
			genelEvrakSatirlari2.oiv_vergi = 0.0;
			genelEvrakSatirlari2.hesap_adi = hizmet.hiz_isim;
			genelEvrakSatirlari2.hesap_yabanci_adi = hizmet.hiz_yabanci_isim;
			genelEvrakSatirlari2.hesap_kisa_adi = "";
			genelEvrakSatirlari2.altgrup_kod = "";
			genelEvrakSatirlari2.altgrup_adi = "";
			genelEvrakSatirlari2.anagrup_kod = "";
			genelEvrakSatirlari2.anagrup_adi = "";
			genelEvrakSatirlari2.sektor_kodu = "";
			genelEvrakSatirlari2.sektor_adi = "";
			genelEvrakSatirlari2.marka_kodu = "";
			genelEvrakSatirlari2.marka_adi = "";
			genelEvrakSatirlari2.model_kodu = "";
			genelEvrakSatirlari2.model_adi = "";
			genelEvrakSatirlari2.uretici_kodu = "";
			genelEvrakSatirlari2.uretici_adi = "";
			genelEvrakSatirlari2.reyon_kodu = "";
			genelEvrakSatirlari2.reyon_adi = "";
			genelEvrakSatirlari2.yer_kodu = "";
			genelEvrakSatirlari2.birim1_ad = "";
			genelEvrakSatirlari2.birim2_ad = "";
			genelEvrakSatirlari2.birim3_ad = "";
			genelEvrakSatirlari2.birim4_ad = "";
			genelEvrakSatirlari2.birim1_katsayi = 1.0;
			genelEvrakSatirlari2.birim2_katsayi = 1.0;
			genelEvrakSatirlari2.birim3_katsayi = 1.0;
			genelEvrakSatirlari2.birim4_katsayi = 1.0;
			genelEvrakSatirlari2.birim1_barkod = "";
			genelEvrakSatirlari2.birim2_barkod = "";
			genelEvrakSatirlari2.birim3_barkod = "";
			genelEvrakSatirlari2.birim4_barkod = "";
			list.Add(genelEvrakSatirlari2);
		}
		return list;
	}

	private static List<GenelEvrakSatirlari> ConvertTahsilatSatirlar(SqliteConnection openedconnection, Evrak evrak)
	{
		List<GenelEvrakSatirlari> list = new List<GenelEvrakSatirlari>();
		foreach (CARI_HESAP_HAREKETLERI item in evrak.GetTahsilatHareketleri())
		{
			GenelEvrakSatirlari genelEvrakSatirlari = new GenelEvrakSatirlari();
			switch (item.cha_cinsi)
			{
			case enum_cha_cinsi.Nakit:
				genelEvrakSatirlari.satir_cinsi = GenelEvrakSatirCinsleri.Nakit;
				break;
			case enum_cha_cinsi.MusteriCeki:
				genelEvrakSatirlari.satir_cinsi = GenelEvrakSatirCinsleri.Cek;
				break;
			case enum_cha_cinsi.MusteriKrediKarti:
				genelEvrakSatirlari.satir_cinsi = GenelEvrakSatirCinsleri.KrediKarti;
				break;
			case enum_cha_cinsi.MusteriSenedi:
				genelEvrakSatirlari.satir_cinsi = GenelEvrakSatirCinsleri.Senet;
				break;
			}
			genelEvrakSatirlari.hesap_kodu = item.cha_kasa_hizkod;
			genelEvrakSatirlari.parti_kodu = "";
			genelEvrakSatirlari.satir_aciklama = item.cha_aciklama;
			genelEvrakSatirlari.satir_aciklama2 = "";
			genelEvrakSatirlari.doviz_cinsi = item.cha_d_cins;
			genelEvrakSatirlari.kur = new Kur();
			genelEvrakSatirlari.kur.dov_no = item.cha_d_cins;
			genelEvrakSatirlari.kur.dov_fiyat = item.cha_d_kur;
			genelEvrakSatirlari.sevk_teslim_tarihi = item.cha_tarihi;
			genelEvrakSatirlari.cari_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.cha_srmrkkodu);
			genelEvrakSatirlari.stok_sorumluluk_merkezi = new SorumlulukMerkezi();
			genelEvrakSatirlari.karsi_sorumluluk_merkezi = SorumlulukMerkeziSqlite.GetSorumlulukMerkezi(openedconnection, item.cha_karsisrmrkkodu);
			genelEvrakSatirlari.proje = ProjeSqlite.GetProje(openedconnection, item.cha_projekodu);
			genelEvrakSatirlari.iskonto_1_uygulama_sekli = item.cha_isk_mas1;
			genelEvrakSatirlari.iskonto_1_tutari = item.cha_ft_iskonto1;
			genelEvrakSatirlari.iskonto_2_uygulama_sekli = item.cha_isk_mas2;
			genelEvrakSatirlari.iskonto_2_tutari = item.cha_ft_iskonto2;
			genelEvrakSatirlari.iskonto_3_uygulama_sekli = item.cha_isk_mas3;
			genelEvrakSatirlari.iskonto_3_tutari = item.cha_ft_iskonto3;
			genelEvrakSatirlari.iskonto_4_uygulama_sekli = item.cha_isk_mas4;
			genelEvrakSatirlari.iskonto_4_tutari = item.cha_ft_iskonto4;
			genelEvrakSatirlari.iskonto_5_uygulama_sekli = item.cha_isk_mas5;
			genelEvrakSatirlari.iskonto_5_tutari = item.cha_ft_iskonto5;
			genelEvrakSatirlari.iskonto_6_uygulama_sekli = item.cha_isk_mas6;
			genelEvrakSatirlari.iskonto_6_tutari = item.cha_ft_iskonto6;
			genelEvrakSatirlari.masraf_1_uygulama_sekli = item.cha_isk_mas7;
			genelEvrakSatirlari.masraf_1_tutari = item.cha_ft_masraf1;
			genelEvrakSatirlari.masraf_2_uygulama_sekli = item.cha_isk_mas8;
			genelEvrakSatirlari.masraf_2_tutari = item.cha_ft_masraf2;
			genelEvrakSatirlari.masraf_3_uygulama_sekli = item.cha_isk_mas9;
			genelEvrakSatirlari.masraf_3_tutari = item.cha_ft_masraf3;
			genelEvrakSatirlari.masraf_4_uygulama_sekli = item.cha_isk_mas10;
			genelEvrakSatirlari.masraf_4_tutari = item.cha_ft_masraf4;
			genelEvrakSatirlari.vergi_pntr = item.cha_vergipntr;
			genelEvrakSatirlari.vergi_tutari = item.cha_vergi1 + item.cha_vergi2 + item.cha_vergi3 + item.cha_vergi4 + item.cha_vergi5 + item.cha_vergi6 + item.cha_vergi7 + item.cha_vergi8 + item.cha_vergi9 + item.cha_vergi10;
			genelEvrakSatirlari.masraf_vergi_pntr = 0;
			genelEvrakSatirlari.masraf_vergi_tutari = 0.0;
			genelEvrakSatirlari.otv_pntr = 0;
			genelEvrakSatirlari.otv_vergi = 0.0;
			genelEvrakSatirlari.oiv_pntr = 0;
			genelEvrakSatirlari.oiv_vergi = 0.0;
			genelEvrakSatirlari.birim_fiyat_brut = item.cha_meblag;
			genelEvrakSatirlari.birim_pntr = 1;
			genelEvrakSatirlari.miktar = 1.0;
			genelEvrakSatirlari.miktar2 = 0.0;
			genelEvrakSatirlari.tutar = item.cha_meblag;
			int year = int.Parse(item.cha_vade.ToString().Substring(0, 4));
			int month = int.Parse(item.cha_vade.ToString().Substring(4, 2));
			int day = int.Parse(item.cha_vade.ToString().Substring(6, 2));
			genelEvrakSatirlari.Vade = new DateTime(year, month, day);
			genelEvrakSatirlari.hesap_adi = "";
			genelEvrakSatirlari.hesap_yabanci_adi = "";
			genelEvrakSatirlari.hesap_kisa_adi = "";
			genelEvrakSatirlari.altgrup_kod = "";
			genelEvrakSatirlari.altgrup_adi = "";
			genelEvrakSatirlari.anagrup_kod = "";
			genelEvrakSatirlari.anagrup_adi = "";
			genelEvrakSatirlari.sektor_kodu = "";
			genelEvrakSatirlari.sektor_adi = "";
			genelEvrakSatirlari.marka_kodu = "";
			genelEvrakSatirlari.marka_adi = "";
			genelEvrakSatirlari.model_kodu = "";
			genelEvrakSatirlari.model_adi = "";
			genelEvrakSatirlari.uretici_kodu = "";
			genelEvrakSatirlari.uretici_adi = "";
			genelEvrakSatirlari.reyon_kodu = "";
			genelEvrakSatirlari.reyon_adi = "";
			genelEvrakSatirlari.yer_kodu = "";
			genelEvrakSatirlari.birim1_ad = "";
			genelEvrakSatirlari.birim2_ad = "";
			genelEvrakSatirlari.birim3_ad = "";
			genelEvrakSatirlari.birim4_ad = "";
			genelEvrakSatirlari.birim1_katsayi = 1.0;
			genelEvrakSatirlari.birim2_katsayi = 1.0;
			genelEvrakSatirlari.birim3_katsayi = 1.0;
			genelEvrakSatirlari.birim4_katsayi = 1.0;
			genelEvrakSatirlari.birim1_barkod = "";
			genelEvrakSatirlari.birim2_barkod = "";
			genelEvrakSatirlari.birim3_barkod = "";
			genelEvrakSatirlari.birim4_barkod = "";
			list.Add(genelEvrakSatirlari);
		}
		return list;
	}
}
