using System;
using System.IO;

namespace Fora.Mikro.Stoklar.StokSerino;

public class STOK_SERINO_TANIMLARI
{
	public int chz_RECid_DBCno { get; set; }

	public int chz_RECid_RECno { get; set; }

	public int chz_RECno { get; set; }

	public Guid chz_Guid { get; set; }

	public Guid ChHar_Guid { get; set; }

	public int chz_Spec_Rec_no { get; set; }

	public bool chz_iptal { get; set; }

	public int chz_fileid { get; set; }

	public bool chz_hidden { get; set; }

	public bool chz_kilitli { get; set; }

	public bool chz_degisti { get; set; }

	public int chz_checksum { get; set; }

	public int chz_create_user { get; set; }

	public DateTime chz_create_date { get; set; }

	public int chz_lastup_user { get; set; }

	public DateTime chz_lastup_date { get; set; }

	public string chz_special1 { get; set; }

	public string chz_special2 { get; set; }

	public string chz_special3 { get; set; }

	public string chz_serino { get; set; }

	public string chz_stok_kodu { get; set; }

	public string chz_grup_kodu { get; set; }

	public string chz_Tuktckodu { get; set; }

	public DateTime chz_GrnBasTarihi { get; set; }

	public DateTime chz_GrnBitTarihi { get; set; }

	public string chz_aciklama1 { get; set; }

	public string chz_aciklama2 { get; set; }

	public string chz_aciklama3 { get; set; }

	public DateTime chz_al_tarih { get; set; }

	public string chz_al_evr_seri { get; set; }

	public int chz_al_evr_sira { get; set; }

	public string chz_al_cari_kodu { get; set; }

	public DateTime chz_al_wd_tarih { get; set; }

	public string chz_al_wd_evr_seri { get; set; }

	public int chz_al_wd_evr_sira { get; set; }

	public DateTime chz_st_tarih { get; set; }

	public string chz_st_evr_seri { get; set; }

	public int chz_st_evr_sira { get; set; }

	public string chz_st_cari_kodu { get; set; }

	public DateTime chz_st_wd_tarih { get; set; }

	public string chz_st_wd_evr_seri { get; set; }

	public int chz_st_wd_evr_sira { get; set; }

	public double chz_brut_fiati { get; set; }

	public double chz_al_fiati_ana { get; set; }

	public double chz_al_fiati_alt { get; set; }

	public double chz_al_fiati_orj { get; set; }

	public double chz_st_fiati_ana { get; set; }

	public double chz_st_fiati_alt { get; set; }

	public double chz_st_fiati_orj { get; set; }

	public bool chz_parca_garantisi { get; set; }

	public string chz_parca_serino { get; set; }

	public DateTime chz_parca_garanti_baslangic { get; set; }

	public DateTime chz_parca_garanti_bitis { get; set; }

	public enum_chz_makina_tipi chz_makina_tipi { get; set; }

	public string chz_model_yili { get; set; }

	public DateTime chz_kiraya_acilma_tarihi { get; set; }

	public DateTime chz_musteri_garanti_baslangic { get; set; }

	public DateTime chz_musteri_garanti_bitis { get; set; }

	public string chz_demirbas_kodu { get; set; }

	public DateTime chz_tescil_tarihi { get; set; }

	public enum_chz_bakim_tipi chz_bakim_tipi { get; set; }

	public DateTime chz_bakim_tarihi { get; set; }

	public int chz_ara_bakim_sayisi { get; set; }

	public enum_chz_bakim_peryodu chz_bakim_peryodu { get; set; }

	public STOK_SERINO_TANIMLARI()
	{
		chz_fileid = 94;
		chz_create_date = DateTime.Now;
		chz_lastup_date = DateTime.Now;
		chz_special1 = "";
		chz_special2 = "";
		chz_special3 = "";
		chz_serino = "";
		chz_stok_kodu = "";
		chz_grup_kodu = "";
		chz_Tuktckodu = "";
		chz_GrnBasTarihi = new DateTime(1900, 1, 1);
		chz_GrnBitTarihi = new DateTime(1900, 1, 1);
		chz_aciklama1 = "";
		chz_aciklama2 = "";
		chz_aciklama3 = "";
		chz_al_tarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		chz_al_evr_seri = "";
		chz_al_cari_kodu = "";
		chz_al_wd_tarih = new DateTime(1900, 1, 1);
		chz_al_wd_evr_seri = "";
		chz_st_tarih = new DateTime(1900, 1, 1);
		chz_st_evr_seri = "";
		chz_st_cari_kodu = "";
		chz_st_wd_tarih = new DateTime(1900, 1, 1);
		chz_st_wd_evr_seri = "";
		chz_parca_serino = "";
		chz_parca_garanti_baslangic = new DateTime(1900, 1, 1);
		chz_parca_garanti_bitis = new DateTime(1900, 1, 1);
		chz_makina_tipi = enum_chz_makina_tipi.sifir_makine;
		chz_model_yili = "";
		chz_kiraya_acilma_tarihi = new DateTime(1900, 1, 1);
		chz_musteri_garanti_baslangic = new DateTime(1900, 1, 1);
		chz_musteri_garanti_bitis = new DateTime(1900, 1, 1);
		chz_demirbas_kodu = "";
		chz_tescil_tarihi = new DateTime(1900, 1, 1);
		chz_bakim_tipi = enum_chz_bakim_tipi.ara_bakim;
		chz_bakim_tarihi = new DateTime(1900, 1, 1);
		chz_bakim_peryodu = enum_chz_bakim_peryodu.haftalik;
		chz_Guid = Guid.Empty;
	}

	public static byte[] WriteToByteArray(STOK_SERINO_TANIMLARI toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(STOK_SERINO_TANIMLARI toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(STOK_SERINO_TANIMLARI toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.chz_RECno);
		writer.Write(toWrite.chz_RECid_DBCno);
		writer.Write(toWrite.chz_RECid_RECno);
		writer.Write(toWrite.chz_Spec_Rec_no);
		writer.Write(toWrite.chz_iptal);
		writer.Write(toWrite.chz_fileid);
		writer.Write(toWrite.chz_hidden);
		writer.Write(toWrite.chz_kilitli);
		writer.Write(toWrite.chz_degisti);
		writer.Write(toWrite.chz_checksum);
		writer.Write(toWrite.chz_create_user);
		writer.Write(toWrite.chz_create_date.Ticks);
		writer.Write(toWrite.chz_lastup_user);
		writer.Write(toWrite.chz_lastup_date.Ticks);
		writer.Write(toWrite.chz_special1);
		writer.Write(toWrite.chz_special2);
		writer.Write(toWrite.chz_special3);
		writer.Write(toWrite.chz_serino);
		writer.Write(toWrite.chz_stok_kodu);
		writer.Write(toWrite.chz_grup_kodu);
		writer.Write(toWrite.chz_Tuktckodu);
		writer.Write(toWrite.chz_GrnBasTarihi.Ticks);
		writer.Write(toWrite.chz_GrnBitTarihi.Ticks);
		writer.Write(toWrite.chz_aciklama1);
		writer.Write(toWrite.chz_aciklama2);
		writer.Write(toWrite.chz_aciklama3);
		writer.Write(toWrite.chz_al_tarih.Ticks);
		writer.Write(toWrite.chz_al_evr_seri);
		writer.Write(toWrite.chz_al_evr_sira);
		writer.Write(toWrite.chz_al_cari_kodu);
		writer.Write(toWrite.chz_al_wd_tarih.Ticks);
		writer.Write(toWrite.chz_al_wd_evr_seri);
		writer.Write(toWrite.chz_al_wd_evr_sira);
		writer.Write(toWrite.chz_st_tarih.Ticks);
		writer.Write(toWrite.chz_st_evr_seri);
		writer.Write(toWrite.chz_st_evr_sira);
		writer.Write(toWrite.chz_st_cari_kodu);
		writer.Write(toWrite.chz_st_wd_tarih.Ticks);
		writer.Write(toWrite.chz_st_wd_evr_seri);
		writer.Write(toWrite.chz_st_wd_evr_sira);
		writer.Write(toWrite.chz_brut_fiati);
		writer.Write(toWrite.chz_al_fiati_ana);
		writer.Write(toWrite.chz_al_fiati_alt);
		writer.Write(toWrite.chz_al_fiati_orj);
		writer.Write(toWrite.chz_st_fiati_ana);
		writer.Write(toWrite.chz_st_fiati_alt);
		writer.Write(toWrite.chz_st_fiati_orj);
		writer.Write(toWrite.chz_parca_garantisi);
		writer.Write(toWrite.chz_parca_serino);
		writer.Write(toWrite.chz_parca_garanti_baslangic.Ticks);
		writer.Write(toWrite.chz_parca_garanti_bitis.Ticks);
		writer.Write((int)toWrite.chz_makina_tipi);
		writer.Write(toWrite.chz_model_yili);
		writer.Write(toWrite.chz_kiraya_acilma_tarihi.Ticks);
		writer.Write(toWrite.chz_musteri_garanti_baslangic.Ticks);
		writer.Write(toWrite.chz_musteri_garanti_bitis.Ticks);
		writer.Write(toWrite.chz_demirbas_kodu);
		writer.Write(toWrite.chz_tescil_tarihi.Ticks);
		writer.Write((int)toWrite.chz_bakim_tipi);
		writer.Write(toWrite.chz_bakim_tarihi.Ticks);
		writer.Write(toWrite.chz_ara_bakim_sayisi);
		writer.Write((int)toWrite.chz_bakim_peryodu);
	}

	public static STOK_SERINO_TANIMLARI ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		STOK_SERINO_TANIMLARI result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static STOK_SERINO_TANIMLARI ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static STOK_SERINO_TANIMLARI ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new STOK_SERINO_TANIMLARI
		{
			chz_RECno = reader.ReadInt32(),
			chz_RECid_DBCno = reader.ReadInt32(),
			chz_RECid_RECno = reader.ReadInt32(),
			chz_Spec_Rec_no = reader.ReadInt32(),
			chz_iptal = reader.ReadBoolean(),
			chz_fileid = reader.ReadInt32(),
			chz_hidden = reader.ReadBoolean(),
			chz_kilitli = reader.ReadBoolean(),
			chz_degisti = reader.ReadBoolean(),
			chz_checksum = reader.ReadInt32(),
			chz_create_user = reader.ReadInt32(),
			chz_create_date = new DateTime(reader.ReadInt64()),
			chz_lastup_user = reader.ReadInt32(),
			chz_lastup_date = new DateTime(reader.ReadInt64()),
			chz_special1 = reader.ReadString(),
			chz_special2 = reader.ReadString(),
			chz_special3 = reader.ReadString(),
			chz_serino = reader.ReadString(),
			chz_stok_kodu = reader.ReadString(),
			chz_grup_kodu = reader.ReadString(),
			chz_Tuktckodu = reader.ReadString(),
			chz_GrnBasTarihi = new DateTime(reader.ReadInt64()),
			chz_GrnBitTarihi = new DateTime(reader.ReadInt64()),
			chz_aciklama1 = reader.ReadString(),
			chz_aciklama2 = reader.ReadString(),
			chz_aciklama3 = reader.ReadString(),
			chz_al_tarih = new DateTime(reader.ReadInt64()),
			chz_al_evr_seri = reader.ReadString(),
			chz_al_evr_sira = reader.ReadInt32(),
			chz_al_cari_kodu = reader.ReadString(),
			chz_al_wd_tarih = new DateTime(reader.ReadInt64()),
			chz_al_wd_evr_seri = reader.ReadString(),
			chz_al_wd_evr_sira = reader.ReadInt32(),
			chz_st_tarih = new DateTime(reader.ReadInt64()),
			chz_st_evr_seri = reader.ReadString(),
			chz_st_evr_sira = reader.ReadInt32(),
			chz_st_cari_kodu = reader.ReadString(),
			chz_st_wd_tarih = new DateTime(reader.ReadInt64()),
			chz_st_wd_evr_seri = reader.ReadString(),
			chz_st_wd_evr_sira = reader.ReadInt32(),
			chz_brut_fiati = reader.ReadDouble(),
			chz_al_fiati_ana = reader.ReadDouble(),
			chz_al_fiati_alt = reader.ReadDouble(),
			chz_al_fiati_orj = reader.ReadDouble(),
			chz_st_fiati_ana = reader.ReadDouble(),
			chz_st_fiati_alt = reader.ReadDouble(),
			chz_st_fiati_orj = reader.ReadDouble(),
			chz_parca_garantisi = reader.ReadBoolean(),
			chz_parca_serino = reader.ReadString(),
			chz_parca_garanti_baslangic = new DateTime(reader.ReadInt64()),
			chz_parca_garanti_bitis = new DateTime(reader.ReadInt64()),
			chz_makina_tipi = (enum_chz_makina_tipi)reader.ReadInt32(),
			chz_model_yili = reader.ReadString(),
			chz_kiraya_acilma_tarihi = new DateTime(reader.ReadInt64()),
			chz_musteri_garanti_baslangic = new DateTime(reader.ReadInt64()),
			chz_musteri_garanti_bitis = new DateTime(reader.ReadInt64()),
			chz_demirbas_kodu = reader.ReadString(),
			chz_tescil_tarihi = new DateTime(reader.ReadInt64()),
			chz_bakim_tipi = (enum_chz_bakim_tipi)reader.ReadInt32(),
			chz_bakim_tarihi = new DateTime(reader.ReadInt64()),
			chz_ara_bakim_sayisi = reader.ReadInt32(),
			chz_bakim_peryodu = (enum_chz_bakim_peryodu)reader.ReadInt32()
		};
	}
}
