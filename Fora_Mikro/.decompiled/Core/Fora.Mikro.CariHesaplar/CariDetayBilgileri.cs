using System;
using System.Collections.Generic;
using System.IO;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar.CariBolgeleri;
using Fora.Mikro.CariHesaplar.CariEkstresi;
using Fora.Mikro.CariHesaplar.CariGruplari;
using Fora.Mikro.CariHesaplar.CariTeminatBilgileri;
using Fora.Mikro.Dovizler;
using Fora.Mikro.OdemePlanlari;
using Fora.Mikro.Siparis;
using Fora.Mikro.Stoklar.FiyatListeleri;

namespace Fora.Mikro.CariHesaplar;

public class CariDetayBilgileri
{
	public DovizCinsiTanimlari _doviz_cinsi_tanimlari { get; set; }

	public int _ana_doviz_cinsi { get; set; }

	public CariDetayParametreleri Parametreler { get; set; }

	public Cari cari { get; set; }

	public string temsilci_adi { get; set; }

	public OdemePlani odeme_plani { get; set; }

	public FiyatListesi fiyat_listesi { get; set; }

	public CariBolge cari_bolge { get; set; }

	public CariGrup cari_grup { get; set; }

	public double bakiye { get; set; }

	public DateTime adat_vadesi { get; set; }

	public double faturalasmamis_irsaliye_tutari { get; set; }

	public double karsilanmamis_siparis_tutari { get; set; }

	public double odenmemis_cek_tutari_kendisi { get; set; }

	public double odenmemis_cek_tutari_musterisi { get; set; }

	public CariTeminatlari cari_teminatlari { get; set; }

	public double bu_yil_cirosu { get; set; }

	public double gecen_yil_cirosu { get; set; }

	public double son_borc_hareketi_tutari { get; set; }

	public DateTime son_borc_hareketi_tarihi { get; set; }

	public enum_cha_evrak_tip son_borc_hareketi_evrak_tipi { get; set; }

	public double son_alacak_hareketi_tutari { get; set; }

	public DateTime son_alacak_hareketi_tarihi { get; set; }

	public enum_cha_evrak_tip son_alacak_hareketi_evrak_tipi { get; set; }

	public List<CariEkstre> cari_ekstre { get; set; }

	public List<SIPARISLER> onceki_siparisler { get; set; }

	public List<CariEkstre> yapilacak_tahsilatlar { get; set; }

	public string yaz_boz_tahtasi { get; set; }

	public double anadoviz_bakiye { get; set; }

	public double anadoviz_odenmemis_cek_tutari_kendisi { get; set; }

	public double anadoviz_odenmemis_cek_tutari_musterisi { get; set; }

	public double anadoviz_karsilanmamis_siparis_tutari { get; set; }

	public double anadoviz_faturalasmamis_irsaliye_tutari { get; set; }

	public double aktif_doviz_kuru { get; set; }

	public double GetRiskTutariBakiye => anadoviz_bakiye / aktif_doviz_kuru / 100.0 * Parametreler.risk_hesabi_bakiye_yuzdesi;

	public double GetRiskTutariIrsaliye => anadoviz_faturalasmamis_irsaliye_tutari / aktif_doviz_kuru / 100.0 * Parametreler.risk_hesabi_irsaliye_yuzdesi;

	public double GetRiskTutariSiparis => anadoviz_karsilanmamis_siparis_tutari / aktif_doviz_kuru / 100.0 * Parametreler.risk_hesabi_siparis_yuzdesi;

	public double GetRiskTutariCekKendisi => anadoviz_odenmemis_cek_tutari_kendisi / aktif_doviz_kuru / 100.0 * Parametreler.risk_hesabi_kendi_ceki_yuzdesi;

	public double GetRiskTutariCekMusterisi => anadoviz_odenmemis_cek_tutari_musterisi / aktif_doviz_kuru / 100.0 * Parametreler.risk_hesabi_musteri_ceki_yuzdesi;

	public double GetRiskTutariToplam => GetRiskTutariBakiye + GetRiskTutariIrsaliye + GetRiskTutariSiparis + GetRiskTutariCekKendisi + GetRiskTutariCekMusterisi;

	public double GetKalanKrediTutari => cari_teminatlari.GetToplamTeminat() - GetRiskTutariToplam;

	public string GetDovizCinsiSembol()
	{
		return Parametreler.CariGrupNo switch
		{
			0 => _doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi).Kur_sembol, 
			1 => _doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi1).Kur_sembol, 
			2 => _doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi2).Kur_sembol, 
			_ => _doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_ana_doviz_cinsi).Kur_sembol, 
		};
	}

	public int GetDovizCinsi()
	{
		return Parametreler.CariGrupNo switch
		{
			0 => cari.cari_doviz_cinsi, 
			1 => cari.cari_doviz_cinsi1, 
			2 => cari.cari_doviz_cinsi2, 
			_ => _ana_doviz_cinsi, 
		};
	}

	public CariDetayBilgileri(DovizCinsiTanimlari DovizCinsiTanimlari, int AnaDovizCinsi)
	{
		_doviz_cinsi_tanimlari = DovizCinsiTanimlari;
		_ana_doviz_cinsi = AnaDovizCinsi;
		Parametreler = new CariDetayParametreleri();
		cari_ekstre = new List<CariEkstre>();
		cari_teminatlari = new CariTeminatlari();
		onceki_siparisler = new List<SIPARISLER>();
		yapilacak_tahsilatlar = new List<CariEkstre>();
		odeme_plani = new OdemePlani();
		fiyat_listesi = new FiyatListesi();
		cari_bolge = new CariBolge();
		cari_grup = new CariGrup();
		yaz_boz_tahtasi = "";
	}

	public static byte[] WriteToByteArray(CariDetayBilgileri toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(CariDetayBilgileri toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(CariDetayBilgileri toWrite, BinaryWriter writer, int versiyon)
	{
		if (toWrite == null)
		{
			int value = 0;
			writer.Write(value);
			return;
		}
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		Cari.WriteToBinaryWriter(toWrite.cari, writer, 999);
		writer.Write(toWrite.Parametreler.temsilci_adi_goster);
		writer.Write(toWrite.temsilci_adi);
		writer.Write(toWrite.Parametreler.odeme_plani_goster);
		writer.Write(toWrite.odeme_plani.odp_no);
		writer.Write(toWrite.odeme_plani.odp_kodu);
		writer.Write(toWrite.odeme_plani.odp_adi);
		writer.Write(toWrite.Parametreler.fiyat_listesi_goster);
		writer.Write(toWrite.fiyat_listesi.sfl_sirano);
		writer.Write(toWrite.fiyat_listesi.sfl_kdvdahil);
		writer.Write(toWrite.fiyat_listesi.sfl_aciklama);
		writer.Write(toWrite.Parametreler.cari_bolge_goster);
		writer.Write(toWrite.cari_bolge.bol_kod);
		writer.Write(toWrite.cari_bolge.bol_ismi);
		writer.Write(toWrite.Parametreler.cari_grup_goster);
		writer.Write(toWrite.cari_grup.crg_kod);
		writer.Write(toWrite.cari_grup.crg_isim);
		writer.Write(toWrite.Parametreler.bakiye_goster);
		writer.Write(toWrite.bakiye);
		writer.Write(toWrite.Parametreler.adat_vadesi_goster);
		writer.Write(toWrite.adat_vadesi.Ticks);
		writer.Write(toWrite.Parametreler.faturalasmamis_irsaliye_tutari_goster);
		writer.Write(toWrite.faturalasmamis_irsaliye_tutari);
		writer.Write(toWrite.Parametreler.karsilanmamis_siparis_tutari_goster);
		writer.Write(toWrite.karsilanmamis_siparis_tutari);
		writer.Write(toWrite.Parametreler.cek_riski_tutari_goster);
		writer.Write(toWrite.odenmemis_cek_tutari_kendisi);
		writer.Write(toWrite.odenmemis_cek_tutari_musterisi);
		writer.Write(toWrite.Parametreler.cek_riski_cirolanmamis_cekin_risk_suresi);
		writer.Write(toWrite.Parametreler.tanimli_kredi_tutari_goster);
		writer.Write(toWrite.cari_teminatlari._teminatlar.Count);
		foreach (CariTeminat item in toWrite.cari_teminatlari._teminatlar)
		{
			writer.Write((int)item.teminat_tipi);
			writer.Write(item.Tutar);
		}
		writer.Write(toWrite.Parametreler.kalan_kredisi_goster);
		writer.Write(0.0);
		writer.Write(toWrite.Parametreler.bu_yil_cirosu_goster);
		writer.Write(toWrite.bu_yil_cirosu);
		writer.Write(toWrite.Parametreler.gecen_yil_cirosu_goster);
		writer.Write(toWrite.gecen_yil_cirosu);
		writer.Write(toWrite.Parametreler.son_borc_hareketi_goster);
		writer.Write(toWrite.son_borc_hareketi_tutari);
		writer.Write(toWrite.son_borc_hareketi_tarihi.Ticks);
		writer.Write((int)toWrite.son_borc_hareketi_evrak_tipi);
		writer.Write(toWrite.Parametreler.son_alacak_hareketi_goster);
		writer.Write(toWrite.son_alacak_hareketi_tutari);
		writer.Write(toWrite.son_alacak_hareketi_tarihi.Ticks);
		writer.Write((int)toWrite.son_alacak_hareketi_evrak_tipi);
		writer.Write(toWrite.Parametreler.cari_ekstre_goster);
		writer.Write(toWrite.cari_ekstre.Count);
		foreach (CariEkstre item2 in toWrite.cari_ekstre)
		{
			CariEkstre.WriteToBinaryWriter(item2, writer, 1);
		}
		writer.Write(toWrite.Parametreler.cari_ekstre_baslangic_tarihi.Ticks);
		writer.Write(toWrite.Parametreler.cari_ekstre_bitis_tarihi.Ticks);
		writer.Write(toWrite.Parametreler.onceki_siparisler_goster);
		writer.Write(toWrite.onceki_siparisler.Count);
		foreach (SIPARISLER item3 in toWrite.onceki_siparisler)
		{
			SIPARISLER.WriteToBinaryWriter(item3, writer, 1);
		}
		writer.Write(toWrite.Parametreler.yapilacak_tahsilatlar_goster);
		writer.Write(toWrite.yapilacak_tahsilatlar.Count);
		foreach (CariEkstre item4 in toWrite.yapilacak_tahsilatlar)
		{
			CariEkstre.WriteToBinaryWriter(item4, writer, 1);
		}
		writer.Write(toWrite.Parametreler.yaz_boz_tahtasi_goster);
		writer.Write(toWrite.yaz_boz_tahtasi);
		writer.Write(toWrite.Parametreler.risk_hesabi_bakiye_yuzdesi);
		writer.Write(toWrite.Parametreler.risk_hesabi_irsaliye_yuzdesi);
		writer.Write(toWrite.Parametreler.risk_hesabi_siparis_yuzdesi);
		writer.Write(toWrite.Parametreler.risk_hesabi_kendi_ceki_yuzdesi);
		writer.Write(toWrite.Parametreler.risk_hesabi_musteri_ceki_yuzdesi);
		writer.Write(toWrite.Parametreler.FirmaNo);
		writer.Write(toWrite.Parametreler.FirmaAdi);
		writer.Write(toWrite.Parametreler.SubeNo);
		writer.Write(toWrite.Parametreler.SorumlulukMerkeziDetayli);
		writer.Write(toWrite.Parametreler.SorumlulukMerkeziKodu);
		writer.Write(toWrite.Parametreler.CariGrupNo);
		writer.Write(toWrite._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari.Count);
		foreach (DovizCinsiTanimi item5 in toWrite._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari)
		{
			writer.Write(item5.Kur_No);
			writer.Write(item5.Kur_Tip);
			writer.Write(item5.Kur_sembol);
			writer.Write(item5.Kur_adi);
			writer.Write(item5.Kur_orjAdi);
			writer.Write(item5.Kur_kusurat_isim);
			writer.Write(item5.Kur_decimal);
			writer.Write(item5.Kur_kusurat_sembol);
		}
		writer.Write(toWrite._ana_doviz_cinsi);
		writer.Write(toWrite.anadoviz_bakiye);
		writer.Write(toWrite.anadoviz_odenmemis_cek_tutari_kendisi);
		writer.Write(toWrite.anadoviz_odenmemis_cek_tutari_musterisi);
		writer.Write(toWrite.anadoviz_karsilanmamis_siparis_tutari);
		writer.Write(toWrite.anadoviz_faturalasmamis_irsaliye_tutari);
		writer.Write(toWrite.aktif_doviz_kuru);
		_ = 2;
	}

	public static CariDetayBilgileri ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		CariDetayBilgileri result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static CariDetayBilgileri ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static CariDetayBilgileri ReadFromBinaryReader(BinaryReader reader)
	{
		if (reader.ReadInt32() == 0)
		{
			return null;
		}
		CariDetayBilgileri cariDetayBilgileri = new CariDetayBilgileri(new DovizCinsiTanimlari(), 0);
		cariDetayBilgileri.cari = Cari.ReadFromBinaryReader(reader);
		cariDetayBilgileri.Parametreler.temsilci_adi_goster = reader.ReadBoolean();
		cariDetayBilgileri.temsilci_adi = reader.ReadString();
		cariDetayBilgileri.Parametreler.odeme_plani_goster = reader.ReadBoolean();
		cariDetayBilgileri.odeme_plani = new OdemePlani();
		cariDetayBilgileri.odeme_plani.odp_no = reader.ReadInt32();
		cariDetayBilgileri.odeme_plani.odp_kodu = reader.ReadString();
		cariDetayBilgileri.odeme_plani.odp_adi = reader.ReadString();
		cariDetayBilgileri.Parametreler.fiyat_listesi_goster = reader.ReadBoolean();
		cariDetayBilgileri.fiyat_listesi = new FiyatListesi();
		cariDetayBilgileri.fiyat_listesi.sfl_sirano = reader.ReadInt32();
		cariDetayBilgileri.fiyat_listesi.sfl_kdvdahil = reader.ReadBoolean();
		cariDetayBilgileri.fiyat_listesi.sfl_aciklama = reader.ReadString();
		cariDetayBilgileri.Parametreler.cari_bolge_goster = reader.ReadBoolean();
		cariDetayBilgileri.cari_bolge = new CariBolge();
		cariDetayBilgileri.cari_bolge.bol_kod = reader.ReadString();
		cariDetayBilgileri.cari_bolge.bol_ismi = reader.ReadString();
		cariDetayBilgileri.Parametreler.cari_grup_goster = reader.ReadBoolean();
		cariDetayBilgileri.cari_grup = new CariGrup();
		cariDetayBilgileri.cari_grup.crg_kod = reader.ReadString();
		cariDetayBilgileri.cari_grup.crg_isim = reader.ReadString();
		cariDetayBilgileri.Parametreler.bakiye_goster = reader.ReadBoolean();
		cariDetayBilgileri.bakiye = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.adat_vadesi_goster = reader.ReadBoolean();
		cariDetayBilgileri.adat_vadesi = new DateTime(reader.ReadInt64());
		cariDetayBilgileri.Parametreler.faturalasmamis_irsaliye_tutari_goster = reader.ReadBoolean();
		cariDetayBilgileri.faturalasmamis_irsaliye_tutari = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.karsilanmamis_siparis_tutari_goster = reader.ReadBoolean();
		cariDetayBilgileri.karsilanmamis_siparis_tutari = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.cek_riski_tutari_goster = reader.ReadBoolean();
		cariDetayBilgileri.odenmemis_cek_tutari_kendisi = reader.ReadDouble();
		cariDetayBilgileri.odenmemis_cek_tutari_musterisi = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.cek_riski_cirolanmamis_cekin_risk_suresi = reader.ReadInt32();
		cariDetayBilgileri.Parametreler.tanimli_kredi_tutari_goster = reader.ReadBoolean();
		cariDetayBilgileri.cari_teminatlari = new CariTeminatlari();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			CariTeminat cariTeminat = new CariTeminat();
			cariTeminat.teminat_tipi = (enum_cari_teminat_tipleri)reader.ReadInt32();
			cariTeminat.Tutar = reader.ReadDouble();
			cariDetayBilgileri.cari_teminatlari._teminatlar.Add(cariTeminat);
		}
		cariDetayBilgileri.Parametreler.kalan_kredisi_goster = reader.ReadBoolean();
		reader.ReadDouble();
		cariDetayBilgileri.Parametreler.bu_yil_cirosu_goster = reader.ReadBoolean();
		cariDetayBilgileri.bu_yil_cirosu = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.gecen_yil_cirosu_goster = reader.ReadBoolean();
		cariDetayBilgileri.gecen_yil_cirosu = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.son_borc_hareketi_goster = reader.ReadBoolean();
		cariDetayBilgileri.son_borc_hareketi_tutari = reader.ReadDouble();
		cariDetayBilgileri.son_borc_hareketi_tarihi = new DateTime(reader.ReadInt64());
		cariDetayBilgileri.son_borc_hareketi_evrak_tipi = (enum_cha_evrak_tip)reader.ReadInt32();
		cariDetayBilgileri.Parametreler.son_alacak_hareketi_goster = reader.ReadBoolean();
		cariDetayBilgileri.son_alacak_hareketi_tutari = reader.ReadDouble();
		cariDetayBilgileri.son_alacak_hareketi_tarihi = new DateTime(reader.ReadInt64());
		cariDetayBilgileri.son_alacak_hareketi_evrak_tipi = (enum_cha_evrak_tip)reader.ReadInt32();
		cariDetayBilgileri.Parametreler.cari_ekstre_goster = reader.ReadBoolean();
		int num2 = reader.ReadInt32();
		cariDetayBilgileri.cari_ekstre = new List<CariEkstre>();
		for (int j = 0; j < num2; j++)
		{
			cariDetayBilgileri.cari_ekstre.Add(CariEkstre.ReadFromBinaryReader(reader));
		}
		cariDetayBilgileri.Parametreler.cari_ekstre_baslangic_tarihi = new DateTime(reader.ReadInt64());
		cariDetayBilgileri.Parametreler.cari_ekstre_bitis_tarihi = new DateTime(reader.ReadInt64());
		cariDetayBilgileri.Parametreler.onceki_siparisler_goster = reader.ReadBoolean();
		int num3 = reader.ReadInt32();
		cariDetayBilgileri.onceki_siparisler = new List<SIPARISLER>();
		for (int k = 0; k < num3; k++)
		{
			cariDetayBilgileri.onceki_siparisler.Add(SIPARISLER.ReadFromBinaryReader(reader));
		}
		cariDetayBilgileri.Parametreler.yapilacak_tahsilatlar_goster = reader.ReadBoolean();
		int num4 = reader.ReadInt32();
		cariDetayBilgileri.yapilacak_tahsilatlar = new List<CariEkstre>();
		for (int l = 0; l < num4; l++)
		{
			cariDetayBilgileri.yapilacak_tahsilatlar.Add(CariEkstre.ReadFromBinaryReader(reader));
		}
		cariDetayBilgileri.Parametreler.yaz_boz_tahtasi_goster = reader.ReadBoolean();
		cariDetayBilgileri.yaz_boz_tahtasi = reader.ReadString();
		cariDetayBilgileri.Parametreler.risk_hesabi_bakiye_yuzdesi = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.risk_hesabi_irsaliye_yuzdesi = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.risk_hesabi_siparis_yuzdesi = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.risk_hesabi_kendi_ceki_yuzdesi = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.risk_hesabi_musteri_ceki_yuzdesi = reader.ReadDouble();
		cariDetayBilgileri.Parametreler.FirmaNo = reader.ReadInt32();
		cariDetayBilgileri.Parametreler.FirmaAdi = reader.ReadString();
		cariDetayBilgileri.Parametreler.SubeNo = reader.ReadInt32();
		cariDetayBilgileri.Parametreler.SorumlulukMerkeziDetayli = reader.ReadBoolean();
		cariDetayBilgileri.Parametreler.SorumlulukMerkeziKodu = reader.ReadString();
		cariDetayBilgileri.Parametreler.CariGrupNo = reader.ReadInt32();
		int num5 = reader.ReadInt32();
		for (int m = 0; m < num5; m++)
		{
			int kur_no = reader.ReadInt32();
			string kur_tip = reader.ReadString();
			string kur_sembol = reader.ReadString();
			string kur_adi = reader.ReadString();
			string kur_orjadi = reader.ReadString();
			string kur_kusurat_isim = reader.ReadString();
			int kur_decimal = reader.ReadInt32();
			string kur_kusurat_sembol = reader.ReadString();
			cariDetayBilgileri._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[0].SetValue(kur_no, kur_tip, kur_sembol, kur_adi, kur_orjadi, kur_kusurat_isim, kur_decimal, kur_kusurat_sembol);
		}
		cariDetayBilgileri._ana_doviz_cinsi = reader.ReadInt32();
		cariDetayBilgileri.anadoviz_bakiye = reader.ReadDouble();
		cariDetayBilgileri.anadoviz_odenmemis_cek_tutari_kendisi = reader.ReadDouble();
		cariDetayBilgileri.anadoviz_odenmemis_cek_tutari_musterisi = reader.ReadDouble();
		cariDetayBilgileri.anadoviz_karsilanmamis_siparis_tutari = reader.ReadDouble();
		cariDetayBilgileri.anadoviz_faturalasmamis_irsaliye_tutari = reader.ReadDouble();
		cariDetayBilgileri.aktif_doviz_kuru = reader.ReadDouble();
		_ = 2;
		return cariDetayBilgileri;
	}
}
