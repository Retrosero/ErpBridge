using System;
using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisSecenekleri
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

	public bool degistirebilir_teslim_durumu { get; set; }

	public enum_siparis_teslim_durumu_secenekleri teslim_durumu { get; set; }

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

	public bool degistirebilir_tutar1_secenek { get; set; }

	public enum_siparis_tutar_secenekleri tutar1_secenek { get; set; }

	public bool degistirebilir_tutar2_secenek { get; set; }

	public enum_siparis_tutar_secenekleri tutar2_secenek { get; set; }

	public bool degistirebilir_tutar3_secenek { get; set; }

	public enum_siparis_tutar_secenekleri tutar3_secenek { get; set; }

	public bool degistirebilir_miktar1_secenek { get; set; }

	public enum_siparis_miktar_secenekleri miktar1_secenek { get; set; }

	public bool degistirebilir_miktar2_secenek { get; set; }

	public enum_siparis_miktar_secenekleri miktar2_secenek { get; set; }

	public bool degistirebilir_miktar3_secenek { get; set; }

	public enum_siparis_miktar_secenekleri miktar3_secenek { get; set; }

	public bool degistirebilir_gruplandirma_secenegi { get; set; }

	public enum_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public bool degistirebilir_siralama_secenegi { get; set; }

	public enum_siralama_secenekleri siralama_secenegi { get; set; }

	public RaporStokSiparisSecenekleri()
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
		degistirebilir_teslim_durumu = true;
		teslim_durumu = enum_siparis_teslim_durumu_secenekleri.Hepsi;
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
		degistirebilir_tutar1_secenek = true;
		tutar1_secenek = enum_siparis_tutar_secenekleri.SiparisNetTutar;
		degistirebilir_tutar2_secenek = true;
		tutar2_secenek = enum_siparis_tutar_secenekleri.Gosterme;
		degistirebilir_tutar3_secenek = true;
		tutar3_secenek = enum_siparis_tutar_secenekleri.Gosterme;
		degistirebilir_miktar1_secenek = true;
		miktar1_secenek = enum_siparis_miktar_secenekleri.SiparisMiktari;
		degistirebilir_miktar2_secenek = true;
		miktar2_secenek = enum_siparis_miktar_secenekleri.Gosterme;
		degistirebilir_miktar3_secenek = true;
		miktar3_secenek = enum_siparis_miktar_secenekleri.Gosterme;
		degistirebilir_gruplandirma_secenegi = true;
		gruplandirma_secenegi = enum_gruplandirma_secenekleri.Stok;
		degistirebilir_siralama_secenegi = true;
		siralama_secenegi = enum_siralama_secenekleri.Tutar1;
	}

	public static byte[] WriteToByteArray(RaporStokSiparisSecenekleri toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSiparisSecenekleri toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokSiparisSecenekleri toWrite, BinaryWriter writer, int versiyon)
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
		writer.Write(toWrite.degistirebilir_teslim_durumu);
		writer.Write((int)toWrite.teslim_durumu);
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

	public static RaporStokSiparisSecenekleri ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokSiparisSecenekleri result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokSiparisSecenekleri ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokSiparisSecenekleri ReadFromBinaryReader(BinaryReader reader)
	{
		RaporStokSiparisSecenekleri raporStokSiparisSecenekleri = new RaporStokSiparisSecenekleri();
		reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_tarih_cinsi = reader.ReadBoolean();
		raporStokSiparisSecenekleri.tarih_cinsi = (enum_tarih_cinsi)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_baslangic_tarihi = reader.ReadBoolean();
		raporStokSiparisSecenekleri.baslangic_tarihi = new DateTime(reader.ReadInt64());
		raporStokSiparisSecenekleri.degistirebilir_bitis_tarihi = reader.ReadBoolean();
		raporStokSiparisSecenekleri.bitis_tarihi = new DateTime(reader.ReadInt64());
		raporStokSiparisSecenekleri.degistirebilir_depolar_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.depolar_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_depolar = reader.ReadBoolean();
		raporStokSiparisSecenekleri.depolar = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_teslim_durumu = reader.ReadBoolean();
		raporStokSiparisSecenekleri.teslim_durumu = (enum_siparis_teslim_durumu_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_proje_kodlari_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.proje_kodlari_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_proje_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.proje_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_sorumluluk_merkezleri_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.sorumluluk_merkezleri_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_sorumluluk_merkezleri = reader.ReadBoolean();
		raporStokSiparisSecenekleri.sorumluluk_merkezleri = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_stok_arama_secenekleri = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_arama_secenekleri = (enum_stok_arama_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_stok_arama_metin = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_arama_metin = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_stok_ana_gruplari_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_ana_gruplari_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_stok_ana_gruplari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_ana_gruplari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_stok_uretici_kodu_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_uretici_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_stok_uretici_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_uretici_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_stok_marka_kodu_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_marka_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_stok_marka_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_marka_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_stok_reyon_kodu_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_reyon_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_stok_reyon_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_reyon_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_stok_kategori_kodu_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_kategori_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_stok_kategori_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.stok_kategori_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_cari_arama_secenekleri = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_arama_secenekleri = (enum_cari_arama_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_cari_arama_metin = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_arama_metin = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_cari_bolge_kodu_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_bolge_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_cari_bolge_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_bolge_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_cari_grup_kodu_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_grup_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_cari_grup_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_grup_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_cari_temsilci_kodu_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_temsilci_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_cari_temsilci_kodlari = reader.ReadBoolean();
		raporStokSiparisSecenekleri.cari_temsilci_kodlari = reader.ReadString();
		raporStokSiparisSecenekleri.degistirebilir_tutar1_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.tutar1_secenek = (enum_siparis_tutar_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_tutar2_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.tutar2_secenek = (enum_siparis_tutar_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_tutar3_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.tutar3_secenek = (enum_siparis_tutar_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_miktar1_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.miktar1_secenek = (enum_siparis_miktar_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_miktar2_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.miktar2_secenek = (enum_siparis_miktar_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_miktar3_secenek = reader.ReadBoolean();
		raporStokSiparisSecenekleri.miktar3_secenek = (enum_siparis_miktar_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_gruplandirma_secenegi = reader.ReadBoolean();
		raporStokSiparisSecenekleri.gruplandirma_secenegi = (enum_gruplandirma_secenekleri)reader.ReadInt32();
		raporStokSiparisSecenekleri.degistirebilir_siralama_secenegi = reader.ReadBoolean();
		raporStokSiparisSecenekleri.siralama_secenegi = (enum_siralama_secenekleri)reader.ReadInt32();
		return raporStokSiparisSecenekleri;
	}
}
