using System;
using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSatis;

public class RaporStokSatisSecenekleri
{
	public bool degistirebilir_tarih_cinsi { get; set; }

	public enum_tarih_cinsi tarih_cinsi { get; set; }

	public bool degistirebilir_baslangic_tarihi { get; set; }

	public DateTime baslangic_tarihi { get; set; }

	public bool degistirebilir_bitis_tarihi { get; set; }

	public DateTime bitis_tarihi { get; set; }

	public bool degistirebilir_depolar_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec depolar_secenek { get; set; }

	public bool degistirebilir_depolar { get; set; }

	public string depolar { get; set; }

	public bool degistirebilir_iadeler_dusulsun_mu { get; set; }

	public bool iadeler_dusulsun_mu { get; set; }

	public bool degistirebilir_proje_kodlari_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec proje_kodlari_secenek { get; set; }

	public bool degistirebilir_proje_kodlari { get; set; }

	public string proje_kodlari { get; set; }

	public bool degistirebilir_sorumluluk_merkezleri_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec sorumluluk_merkezleri_secenek { get; set; }

	public bool degistirebilir_sorumluluk_merkezleri { get; set; }

	public string sorumluluk_merkezleri { get; set; }

	public bool degistirebilir_stok_arama_secenekleri { get; set; }

	public enum_stok_arama_secenekleri stok_arama_secenekleri { get; set; }

	public bool degistirebilir_stok_arama_metin { get; set; }

	public string stok_arama_metin { get; set; }

	public bool degistirebilir_stok_ana_gruplari_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec stok_ana_gruplari_secenek { get; set; }

	public bool degistirebilir_stok_ana_gruplari { get; set; }

	public string stok_ana_gruplari { get; set; }

	public bool degistirebilir_stok_uretici_kodu_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec stok_uretici_kodu_secenek { get; set; }

	public bool degistirebilir_stok_uretici_kodlari { get; set; }

	public string stok_uretici_kodlari { get; set; }

	public bool degistirebilir_stok_marka_kodu_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec stok_marka_kodu_secenek { get; set; }

	public bool degistirebilir_stok_marka_kodlari { get; set; }

	public string stok_marka_kodlari { get; set; }

	public bool degistirebilir_stok_reyon_kodu_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec stok_reyon_kodu_secenek { get; set; }

	public bool degistirebilir_stok_reyon_kodlari { get; set; }

	public string stok_reyon_kodlari { get; set; }

	public bool degistirebilir_stok_kategori_kodu_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec stok_kategori_kodu_secenek { get; set; }

	public bool degistirebilir_stok_kategori_kodlari { get; set; }

	public string stok_kategori_kodlari { get; set; }

	public bool degistirebilir_cari_arama_secenekleri { get; set; }

	public enum_cari_arama_secenekleri cari_arama_secenekleri { get; set; }

	public bool degistirebilir_cari_arama_metin { get; set; }

	public string cari_arama_metin { get; set; }

	public bool degistirebilir_cari_bolge_kodu_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec cari_bolge_kodu_secenek { get; set; }

	public bool degistirebilir_cari_bolge_kodlari { get; set; }

	public string cari_bolge_kodlari { get; set; }

	public bool degistirebilir_cari_grup_kodu_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec cari_grup_kodu_secenek { get; set; }

	public bool degistirebilir_cari_grup_kodlari { get; set; }

	public string cari_grup_kodlari { get; set; }

	public bool degistirebilir_cari_temsilci_kodu_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec cari_temsilci_kodu_secenek { get; set; }

	public bool degistirebilir_cari_temsilci_kodlari { get; set; }

	public string cari_temsilci_kodlari { get; set; }

	public bool degistirebilir_fatura_durumu_secenegi { get; set; }

	public enum_fatura_durumu_secenekleri fatura_durumu_secenegi { get; set; }

	public bool degistirebilir_tutar1_secenek { get; set; }

	public enum_tutar_secenekleri tutar1_secenek { get; set; }

	public bool degistirebilir_tutar2_secenek { get; set; }

	public enum_tutar_secenekleri tutar2_secenek { get; set; }

	public bool degistirebilir_tutar3_secenek { get; set; }

	public enum_tutar_secenekleri tutar3_secenek { get; set; }

	public bool degistirebilir_miktar1_secenek { get; set; }

	public enum_miktar_secenekleri miktar1_secenek { get; set; }

	public bool degistirebilir_miktar2_secenek { get; set; }

	public enum_miktar_secenekleri miktar2_secenek { get; set; }

	public bool degistirebilir_miktar3_secenek { get; set; }

	public enum_miktar_secenekleri miktar3_secenek { get; set; }

	public bool degistirebilir_gruplandirma_secenegi { get; set; }

	public enum_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public bool degistirebilir_siralama_secenegi { get; set; }

	public enum_siralama_secenekleri siralama_secenegi { get; set; }

	public RaporStokSatisSecenekleri()
	{
		degistirebilir_tarih_cinsi = true;
		tarih_cinsi = enum_tarih_cinsi.BuAy;
		degistirebilir_baslangic_tarihi = true;
		baslangic_tarihi = DateTime.Now;
		degistirebilir_bitis_tarihi = true;
		bitis_tarihi = DateTime.Now;
		degistirebilir_depolar_secenek = false;
		depolar_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_depolar = false;
		depolar = "";
		degistirebilir_iadeler_dusulsun_mu = true;
		iadeler_dusulsun_mu = true;
		degistirebilir_proje_kodlari_secenek = false;
		proje_kodlari_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_proje_kodlari = false;
		proje_kodlari = "";
		degistirebilir_sorumluluk_merkezleri_secenek = false;
		sorumluluk_merkezleri_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_sorumluluk_merkezleri = false;
		sorumluluk_merkezleri = "";
		degistirebilir_stok_arama_secenekleri = true;
		stok_arama_secenekleri = enum_stok_arama_secenekleri.TanimliOlan;
		degistirebilir_stok_arama_metin = true;
		stok_arama_metin = "";
		degistirebilir_stok_ana_gruplari_secenek = true;
		stok_ana_gruplari_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_stok_ana_gruplari = true;
		stok_ana_gruplari = "";
		degistirebilir_stok_uretici_kodu_secenek = true;
		stok_uretici_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_stok_uretici_kodlari = true;
		stok_uretici_kodlari = "";
		degistirebilir_stok_marka_kodu_secenek = true;
		stok_marka_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_stok_marka_kodlari = true;
		stok_marka_kodlari = "";
		degistirebilir_stok_reyon_kodu_secenek = true;
		stok_reyon_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_stok_reyon_kodlari = true;
		stok_reyon_kodlari = "";
		degistirebilir_stok_kategori_kodu_secenek = true;
		stok_kategori_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_stok_kategori_kodlari = true;
		stok_kategori_kodlari = "";
		degistirebilir_cari_arama_secenekleri = true;
		cari_arama_secenekleri = enum_cari_arama_secenekleri.TanimliOlan;
		degistirebilir_cari_arama_metin = true;
		cari_arama_metin = "";
		degistirebilir_cari_bolge_kodu_secenek = false;
		cari_bolge_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_cari_bolge_kodlari = false;
		cari_bolge_kodlari = "";
		degistirebilir_cari_grup_kodu_secenek = false;
		cari_grup_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_cari_grup_kodlari = false;
		cari_grup_kodlari = "";
		degistirebilir_cari_temsilci_kodu_secenek = false;
		cari_temsilci_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_cari_temsilci_kodlari = false;
		cari_temsilci_kodlari = "";
		degistirebilir_fatura_durumu_secenegi = true;
		fatura_durumu_secenegi = enum_fatura_durumu_secenekleri.Hepsi;
		degistirebilir_tutar1_secenek = true;
		tutar1_secenek = enum_tutar_secenekleri.NetTutar;
		degistirebilir_tutar2_secenek = true;
		tutar2_secenek = enum_tutar_secenekleri.Gosterme;
		degistirebilir_tutar3_secenek = true;
		tutar3_secenek = enum_tutar_secenekleri.Gosterme;
		degistirebilir_miktar1_secenek = true;
		miktar1_secenek = enum_miktar_secenekleri.NetSatisMiktari;
		degistirebilir_miktar2_secenek = true;
		miktar2_secenek = enum_miktar_secenekleri.Gosterme;
		degistirebilir_miktar3_secenek = true;
		miktar3_secenek = enum_miktar_secenekleri.Gosterme;
		degistirebilir_gruplandirma_secenegi = true;
		gruplandirma_secenegi = enum_gruplandirma_secenekleri.Stok;
		degistirebilir_siralama_secenegi = true;
		siralama_secenegi = enum_siralama_secenekleri.Tutar1;
	}

	public static byte[] WriteToByteArray(RaporStokSatisSecenekleri toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSatisSecenekleri toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokSatisSecenekleri toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.degistirebilir_tarih_cinsi);
		writer.Write((int)toWrite.tarih_cinsi);
		writer.Write(toWrite.degistirebilir_baslangic_tarihi);
		writer.Write(toWrite.baslangic_tarihi.Ticks);
		writer.Write(toWrite.degistirebilir_bitis_tarihi);
		writer.Write(toWrite.bitis_tarihi.Ticks);
		writer.Write(toWrite.degistirebilir_depolar_secenek);
		writer.Write((int)toWrite.depolar_secenek);
		writer.Write(toWrite.degistirebilir_depolar);
		writer.Write(toWrite.depolar);
		writer.Write(toWrite.degistirebilir_iadeler_dusulsun_mu);
		writer.Write(toWrite.iadeler_dusulsun_mu);
		writer.Write(toWrite.degistirebilir_proje_kodlari_secenek);
		writer.Write((int)toWrite.proje_kodlari_secenek);
		writer.Write(toWrite.degistirebilir_proje_kodlari);
		writer.Write(toWrite.proje_kodlari);
		writer.Write(toWrite.degistirebilir_sorumluluk_merkezleri_secenek);
		writer.Write((int)toWrite.sorumluluk_merkezleri_secenek);
		writer.Write(toWrite.degistirebilir_sorumluluk_merkezleri);
		writer.Write(toWrite.sorumluluk_merkezleri);
		writer.Write(toWrite.degistirebilir_stok_arama_secenekleri);
		writer.Write((int)toWrite.stok_arama_secenekleri);
		writer.Write(toWrite.degistirebilir_stok_arama_metin);
		writer.Write(toWrite.stok_arama_metin);
		writer.Write(toWrite.degistirebilir_stok_ana_gruplari_secenek);
		writer.Write((int)toWrite.stok_ana_gruplari_secenek);
		writer.Write(toWrite.degistirebilir_stok_ana_gruplari);
		writer.Write(toWrite.stok_ana_gruplari);
		writer.Write(toWrite.degistirebilir_stok_uretici_kodu_secenek);
		writer.Write((int)toWrite.stok_uretici_kodu_secenek);
		writer.Write(toWrite.degistirebilir_stok_uretici_kodlari);
		writer.Write(toWrite.stok_uretici_kodlari);
		writer.Write(toWrite.degistirebilir_stok_marka_kodu_secenek);
		writer.Write((int)toWrite.stok_marka_kodu_secenek);
		writer.Write(toWrite.degistirebilir_stok_marka_kodlari);
		writer.Write(toWrite.stok_marka_kodlari);
		writer.Write(toWrite.degistirebilir_stok_reyon_kodu_secenek);
		writer.Write((int)toWrite.stok_reyon_kodu_secenek);
		writer.Write(toWrite.degistirebilir_stok_reyon_kodlari);
		writer.Write(toWrite.stok_reyon_kodlari);
		writer.Write(toWrite.degistirebilir_stok_kategori_kodu_secenek);
		writer.Write((int)toWrite.stok_kategori_kodu_secenek);
		writer.Write(toWrite.degistirebilir_stok_kategori_kodlari);
		writer.Write(toWrite.stok_kategori_kodlari);
		writer.Write(toWrite.degistirebilir_cari_arama_secenekleri);
		writer.Write((int)toWrite.cari_arama_secenekleri);
		writer.Write(toWrite.degistirebilir_cari_arama_metin);
		writer.Write(toWrite.cari_arama_metin);
		writer.Write(toWrite.degistirebilir_cari_bolge_kodu_secenek);
		writer.Write((int)toWrite.cari_bolge_kodu_secenek);
		writer.Write(toWrite.degistirebilir_cari_bolge_kodlari);
		writer.Write(toWrite.cari_bolge_kodlari);
		writer.Write(toWrite.degistirebilir_cari_grup_kodu_secenek);
		writer.Write((int)toWrite.cari_grup_kodu_secenek);
		writer.Write(toWrite.degistirebilir_cari_grup_kodlari);
		writer.Write(toWrite.cari_grup_kodlari);
		writer.Write(toWrite.degistirebilir_cari_temsilci_kodu_secenek);
		writer.Write((int)toWrite.cari_temsilci_kodu_secenek);
		writer.Write(toWrite.degistirebilir_cari_temsilci_kodlari);
		writer.Write(toWrite.cari_temsilci_kodlari);
		writer.Write(toWrite.degistirebilir_fatura_durumu_secenegi);
		writer.Write((int)toWrite.fatura_durumu_secenegi);
		writer.Write(toWrite.degistirebilir_tutar1_secenek);
		writer.Write((int)toWrite.tutar1_secenek);
		writer.Write(toWrite.degistirebilir_tutar2_secenek);
		writer.Write((int)toWrite.tutar2_secenek);
		writer.Write(toWrite.degistirebilir_tutar3_secenek);
		writer.Write((int)toWrite.tutar3_secenek);
		writer.Write(toWrite.degistirebilir_miktar1_secenek);
		writer.Write((int)toWrite.miktar1_secenek);
		writer.Write(toWrite.degistirebilir_miktar2_secenek);
		writer.Write((int)toWrite.miktar2_secenek);
		writer.Write(toWrite.degistirebilir_miktar3_secenek);
		writer.Write((int)toWrite.miktar3_secenek);
		writer.Write(toWrite.degistirebilir_gruplandirma_secenegi);
		writer.Write((int)toWrite.gruplandirma_secenegi);
		writer.Write(toWrite.degistirebilir_siralama_secenegi);
		writer.Write((int)toWrite.siralama_secenegi);
		_ = 2;
	}

	public static RaporStokSatisSecenekleri ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokSatisSecenekleri result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokSatisSecenekleri ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokSatisSecenekleri ReadFromBinaryReader(BinaryReader reader)
	{
		RaporStokSatisSecenekleri raporStokSatisSecenekleri = new RaporStokSatisSecenekleri();
		reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_tarih_cinsi = reader.ReadBoolean();
		raporStokSatisSecenekleri.tarih_cinsi = (enum_tarih_cinsi)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_baslangic_tarihi = reader.ReadBoolean();
		raporStokSatisSecenekleri.baslangic_tarihi = new DateTime(reader.ReadInt64());
		raporStokSatisSecenekleri.degistirebilir_bitis_tarihi = reader.ReadBoolean();
		raporStokSatisSecenekleri.bitis_tarihi = new DateTime(reader.ReadInt64());
		raporStokSatisSecenekleri.degistirebilir_depolar_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.depolar_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_depolar = reader.ReadBoolean();
		raporStokSatisSecenekleri.depolar = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_iadeler_dusulsun_mu = reader.ReadBoolean();
		raporStokSatisSecenekleri.iadeler_dusulsun_mu = reader.ReadBoolean();
		raporStokSatisSecenekleri.degistirebilir_proje_kodlari_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.proje_kodlari_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_proje_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.proje_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_sorumluluk_merkezleri_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.sorumluluk_merkezleri_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_sorumluluk_merkezleri = reader.ReadBoolean();
		raporStokSatisSecenekleri.sorumluluk_merkezleri = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_stok_arama_secenekleri = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_arama_secenekleri = (enum_stok_arama_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_stok_arama_metin = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_arama_metin = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_stok_ana_gruplari_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_ana_gruplari_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_stok_ana_gruplari = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_ana_gruplari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_stok_uretici_kodu_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_uretici_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_stok_uretici_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_uretici_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_stok_marka_kodu_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_marka_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_stok_marka_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_marka_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_stok_reyon_kodu_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_reyon_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_stok_reyon_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_reyon_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_stok_kategori_kodu_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_kategori_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_stok_kategori_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.stok_kategori_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_cari_arama_secenekleri = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_arama_secenekleri = (enum_cari_arama_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_cari_arama_metin = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_arama_metin = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_cari_bolge_kodu_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_bolge_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_cari_bolge_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_bolge_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_cari_grup_kodu_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_grup_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_cari_grup_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_grup_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_cari_temsilci_kodu_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_temsilci_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_cari_temsilci_kodlari = reader.ReadBoolean();
		raporStokSatisSecenekleri.cari_temsilci_kodlari = reader.ReadString();
		raporStokSatisSecenekleri.degistirebilir_fatura_durumu_secenegi = reader.ReadBoolean();
		raporStokSatisSecenekleri.fatura_durumu_secenegi = (enum_fatura_durumu_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_tutar1_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.tutar1_secenek = (enum_tutar_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_tutar2_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.tutar2_secenek = (enum_tutar_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_tutar3_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.tutar3_secenek = (enum_tutar_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_miktar1_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.miktar1_secenek = (enum_miktar_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_miktar2_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.miktar2_secenek = (enum_miktar_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_miktar3_secenek = reader.ReadBoolean();
		raporStokSatisSecenekleri.miktar3_secenek = (enum_miktar_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_gruplandirma_secenegi = reader.ReadBoolean();
		raporStokSatisSecenekleri.gruplandirma_secenegi = (enum_gruplandirma_secenekleri)reader.ReadInt32();
		raporStokSatisSecenekleri.degistirebilir_siralama_secenegi = reader.ReadBoolean();
		raporStokSatisSecenekleri.siralama_secenegi = (enum_siralama_secenekleri)reader.ReadInt32();
		return raporStokSatisSecenekleri;
	}
}
