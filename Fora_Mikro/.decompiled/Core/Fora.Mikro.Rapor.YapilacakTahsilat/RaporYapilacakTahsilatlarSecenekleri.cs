using System;
using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class RaporYapilacakTahsilatlarSecenekleri
{
	public bool degistirebilir_tarih_cinsi { get; set; }

	public enum_tarih_cinsi tarih_cinsi { get; set; }

	public DateTime baslangic_tarihi { get; set; }

	public DateTime bitis_tarihi { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec proje_kodlari_secenek { get; set; }

	public bool degistirebilir_proje_kodlari { get; set; }

	public string proje_kodlari { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec sorumluluk_merkezleri_secenek { get; set; }

	public bool degistirebilir_sorumluluk_merkezleri { get; set; }

	public string sorumluluk_merkezleri { get; set; }

	public enum_cari_arama_secenekleri cari_arama_secenekleri { get; set; }

	public bool degistirebilir_cari_arama_metin { get; set; }

	public string cari_arama_metin { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec cari_bolge_kodu_secenek { get; set; }

	public bool degistirebilir_cari_bolge_kodlari { get; set; }

	public string cari_bolge_kodlari { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec cari_grup_kodu_secenek { get; set; }

	public bool degistirebilir_cari_grup_kodlari { get; set; }

	public string cari_grup_kodlari { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec cari_temsilci_kodu_secenek { get; set; }

	public bool degistirebilir_cari_temsilci_kodlari { get; set; }

	public string cari_temsilci_kodlari { get; set; }

	public bool degistirebilir_gruplandirma_secenegi { get; set; }

	public enum_yapilacak_tahsilatlar_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public bool degistirebilir_siralama_secenegi { get; set; }

	public enum_yapilacak_tahsilatlar_siralama_secenekleri siralama_secenegi { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec firma_secenek { get; set; }

	public string firma_nolari { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec sube_secenek { get; set; }

	public string sube_nolari { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec doviz_cinsi_secenek { get; set; }

	public string doviz_cinsleri { get; set; }

	public bool degistirebilir_sorumluluk_merkezi_detayli { get; set; }

	public bool SorumlulukMerkeziDetayli { get; set; }

	public bool degistirebilir_proje_detayli { get; set; }

	public bool ProjeDetayli { get; set; }

	public bool degistirebilir_minimum_bakiye { get; set; }

	public int MinimumBakiye { get; set; }

	public RaporYapilacakTahsilatlarSecenekleri()
	{
		degistirebilir_tarih_cinsi = true;
		tarih_cinsi = enum_tarih_cinsi.TumZamanlar;
		baslangic_tarihi = DateTime.Now;
		bitis_tarihi = DateTime.Now;
		proje_kodlari_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_proje_kodlari = false;
		proje_kodlari = "";
		sorumluluk_merkezleri_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_sorumluluk_merkezleri = false;
		sorumluluk_merkezleri = "";
		cari_arama_secenekleri = enum_cari_arama_secenekleri.TanimliOlan;
		degistirebilir_cari_arama_metin = true;
		cari_arama_metin = "";
		cari_bolge_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_cari_bolge_kodlari = false;
		cari_bolge_kodlari = "";
		cari_grup_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_cari_grup_kodlari = false;
		cari_grup_kodlari = "";
		cari_temsilci_kodu_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_cari_temsilci_kodlari = false;
		cari_temsilci_kodlari = "";
		degistirebilir_gruplandirma_secenegi = true;
		gruplandirma_secenegi = enum_yapilacak_tahsilatlar_gruplandirma_secenekleri.Ay;
		degistirebilir_siralama_secenegi = true;
		siralama_secenegi = enum_yapilacak_tahsilatlar_siralama_secenekleri.Kod;
		firma_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		firma_nolari = "";
		sube_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		sube_nolari = "";
		doviz_cinsi_secenek = enum_tumu_tanimli_olan_listeden_sec.Tumu;
		doviz_cinsleri = "";
		degistirebilir_sorumluluk_merkezi_detayli = true;
		SorumlulukMerkeziDetayli = false;
		degistirebilir_proje_detayli = true;
		ProjeDetayli = false;
		degistirebilir_minimum_bakiye = true;
		MinimumBakiye = 0;
	}

	public static byte[] WriteToByteArray(RaporYapilacakTahsilatlarSecenekleri toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporYapilacakTahsilatlarSecenekleri toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporYapilacakTahsilatlarSecenekleri toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.degistirebilir_tarih_cinsi);
		writer.Write((int)toWrite.tarih_cinsi);
		writer.Write(toWrite.baslangic_tarihi.Ticks);
		writer.Write(toWrite.bitis_tarihi.Ticks);
		writer.Write((int)toWrite.proje_kodlari_secenek);
		writer.Write(toWrite.degistirebilir_proje_kodlari);
		writer.Write(toWrite.proje_kodlari);
		writer.Write((int)toWrite.sorumluluk_merkezleri_secenek);
		writer.Write(toWrite.degistirebilir_sorumluluk_merkezleri);
		writer.Write(toWrite.sorumluluk_merkezleri);
		writer.Write((int)toWrite.cari_arama_secenekleri);
		writer.Write(toWrite.degistirebilir_cari_arama_metin);
		writer.Write(toWrite.cari_arama_metin);
		writer.Write((int)toWrite.cari_bolge_kodu_secenek);
		writer.Write(toWrite.degistirebilir_cari_bolge_kodlari);
		writer.Write(toWrite.cari_bolge_kodlari);
		writer.Write((int)toWrite.cari_grup_kodu_secenek);
		writer.Write(toWrite.degistirebilir_cari_grup_kodlari);
		writer.Write(toWrite.cari_grup_kodlari);
		writer.Write((int)toWrite.cari_temsilci_kodu_secenek);
		writer.Write(toWrite.degistirebilir_cari_temsilci_kodlari);
		writer.Write(toWrite.cari_temsilci_kodlari);
		writer.Write(toWrite.degistirebilir_gruplandirma_secenegi);
		writer.Write((int)toWrite.gruplandirma_secenegi);
		writer.Write(toWrite.degistirebilir_siralama_secenegi);
		writer.Write((int)toWrite.siralama_secenegi);
		writer.Write((int)toWrite.firma_secenek);
		writer.Write(toWrite.firma_nolari);
		writer.Write((int)toWrite.sube_secenek);
		writer.Write(toWrite.sube_nolari);
		writer.Write((int)toWrite.doviz_cinsi_secenek);
		writer.Write(toWrite.doviz_cinsleri);
		writer.Write(toWrite.degistirebilir_sorumluluk_merkezi_detayli);
		writer.Write(toWrite.SorumlulukMerkeziDetayli);
		writer.Write(toWrite.degistirebilir_proje_detayli);
		writer.Write(toWrite.ProjeDetayli);
		writer.Write(toWrite.degistirebilir_minimum_bakiye);
		writer.Write(toWrite.MinimumBakiye);
		_ = 2;
	}

	public static RaporYapilacakTahsilatlarSecenekleri ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporYapilacakTahsilatlarSecenekleri result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporYapilacakTahsilatlarSecenekleri ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporYapilacakTahsilatlarSecenekleri ReadFromBinaryReader(BinaryReader reader)
	{
		RaporYapilacakTahsilatlarSecenekleri raporYapilacakTahsilatlarSecenekleri = new RaporYapilacakTahsilatlarSecenekleri();
		reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_tarih_cinsi = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.tarih_cinsi = (enum_tarih_cinsi)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.baslangic_tarihi = new DateTime(reader.ReadInt64());
		raporYapilacakTahsilatlarSecenekleri.bitis_tarihi = new DateTime(reader.ReadInt64());
		raporYapilacakTahsilatlarSecenekleri.proje_kodlari_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_proje_kodlari = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.proje_kodlari = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.sorumluluk_merkezleri_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_sorumluluk_merkezleri = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.sorumluluk_merkezleri = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.cari_arama_secenekleri = (enum_cari_arama_secenekleri)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_cari_arama_metin = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.cari_arama_metin = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.cari_bolge_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_cari_bolge_kodlari = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.cari_bolge_kodlari = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.cari_grup_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_cari_grup_kodlari = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.cari_grup_kodlari = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.cari_temsilci_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_cari_temsilci_kodlari = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.cari_temsilci_kodlari = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_gruplandirma_secenegi = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.gruplandirma_secenegi = (enum_yapilacak_tahsilatlar_gruplandirma_secenekleri)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_siralama_secenegi = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.siralama_secenegi = (enum_yapilacak_tahsilatlar_siralama_secenekleri)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.firma_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.firma_nolari = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.sube_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.sube_nolari = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.doviz_cinsi_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporYapilacakTahsilatlarSecenekleri.doviz_cinsleri = reader.ReadString();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_sorumluluk_merkezi_detayli = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.SorumlulukMerkeziDetayli = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_proje_detayli = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.ProjeDetayli = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.degistirebilir_minimum_bakiye = reader.ReadBoolean();
		raporYapilacakTahsilatlarSecenekleri.MinimumBakiye = reader.ReadInt32();
		return raporYapilacakTahsilatlarSecenekleri;
	}
}
