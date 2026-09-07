using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokEnvanter;

public class RaporStokEnvanterSecenekleri
{
	public bool degistirebilir_depolar_secenek { get; set; }

	public enum_tumu_tanimli_olan_listeden_sec depolar_secenek { get; set; }

	public bool degistirebilir_depolar { get; set; }

	public string depolar { get; set; }

	public bool stokta_olan_urunler { get; set; }

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

	public enum_stok_envanter_degerleme_sekli degerleme_sekli { get; set; }

	public bool degistirebilir_tutar1_secenek { get; set; }

	public enum_stok_envanter_tutar_secenekleri tutar1_secenek { get; set; }

	public bool degistirebilir_tutar2_secenek { get; set; }

	public enum_stok_envanter_tutar_secenekleri tutar2_secenek { get; set; }

	public bool degistirebilir_tutar3_secenek { get; set; }

	public enum_stok_envanter_tutar_secenekleri tutar3_secenek { get; set; }

	public bool degistirebilir_miktar1_secenek { get; set; }

	public enum_stok_envanter_miktar_secenekleri miktar1_secenek { get; set; }

	public bool degistirebilir_miktar2_secenek { get; set; }

	public enum_stok_envanter_miktar_secenekleri miktar2_secenek { get; set; }

	public bool degistirebilir_miktar3_secenek { get; set; }

	public enum_stok_envanter_miktar_secenekleri miktar3_secenek { get; set; }

	public bool degistirebilir_gruplandirma_secenegi { get; set; }

	public enum_stok_envanter_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public bool degistirebilir_siralama_secenegi { get; set; }

	public enum_siralama_secenekleri siralama_secenegi { get; set; }

	public string fiyat_liste_no { get; set; }

	public RaporStokEnvanterSecenekleri()
	{
		degistirebilir_depolar_secenek = false;
		depolar_secenek = enum_tumu_tanimli_olan_listeden_sec.TanimliOlan;
		degistirebilir_depolar = false;
		depolar = "";
		stokta_olan_urunler = true;
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
		degerleme_sekli = enum_stok_envanter_degerleme_sekli.FiyatListesi;
		degistirebilir_tutar1_secenek = true;
		tutar1_secenek = enum_stok_envanter_tutar_secenekleri.Tutar;
		degistirebilir_tutar2_secenek = true;
		tutar2_secenek = enum_stok_envanter_tutar_secenekleri.Gosterme;
		degistirebilir_tutar3_secenek = true;
		tutar3_secenek = enum_stok_envanter_tutar_secenekleri.Gosterme;
		degistirebilir_miktar1_secenek = true;
		miktar1_secenek = enum_stok_envanter_miktar_secenekleri.MiktarBirim1;
		degistirebilir_miktar2_secenek = true;
		miktar2_secenek = enum_stok_envanter_miktar_secenekleri.Gosterme;
		degistirebilir_miktar3_secenek = true;
		miktar3_secenek = enum_stok_envanter_miktar_secenekleri.Gosterme;
		degistirebilir_gruplandirma_secenegi = true;
		gruplandirma_secenegi = enum_stok_envanter_gruplandirma_secenekleri.Stok;
		degistirebilir_siralama_secenegi = true;
		siralama_secenegi = enum_siralama_secenekleri.Isim;
		fiyat_liste_no = "1";
	}

	public static byte[] WriteToByteArray(RaporStokEnvanterSecenekleri toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokEnvanterSecenekleri toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokEnvanterSecenekleri toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.degistirebilir_depolar_secenek);
		writer.Write((int)toWrite.depolar_secenek);
		writer.Write(toWrite.degistirebilir_depolar);
		writer.Write(toWrite.depolar);
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
		writer.Write((int)toWrite.degerleme_sekli);
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
		writer.Write(toWrite.stokta_olan_urunler);
		writer.Write(toWrite.fiyat_liste_no);
		_ = 2;
	}

	public static RaporStokEnvanterSecenekleri ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokEnvanterSecenekleri result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokEnvanterSecenekleri ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokEnvanterSecenekleri ReadFromBinaryReader(BinaryReader reader)
	{
		RaporStokEnvanterSecenekleri raporStokEnvanterSecenekleri = new RaporStokEnvanterSecenekleri();
		reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_depolar_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.depolar_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_depolar = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.depolar = reader.ReadString();
		raporStokEnvanterSecenekleri.degistirebilir_stok_arama_secenekleri = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_arama_secenekleri = (enum_stok_arama_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_stok_arama_metin = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_arama_metin = reader.ReadString();
		raporStokEnvanterSecenekleri.degistirebilir_stok_ana_gruplari_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_ana_gruplari_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_stok_ana_gruplari = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_ana_gruplari = reader.ReadString();
		raporStokEnvanterSecenekleri.degistirebilir_stok_uretici_kodu_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_uretici_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_stok_uretici_kodlari = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_uretici_kodlari = reader.ReadString();
		raporStokEnvanterSecenekleri.degistirebilir_stok_marka_kodu_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_marka_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_stok_marka_kodlari = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_marka_kodlari = reader.ReadString();
		raporStokEnvanterSecenekleri.degistirebilir_stok_reyon_kodu_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_reyon_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_stok_reyon_kodlari = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_reyon_kodlari = reader.ReadString();
		raporStokEnvanterSecenekleri.degistirebilir_stok_kategori_kodu_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_kategori_kodu_secenek = (enum_tumu_tanimli_olan_listeden_sec)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_stok_kategori_kodlari = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.stok_kategori_kodlari = reader.ReadString();
		raporStokEnvanterSecenekleri.degerleme_sekli = (enum_stok_envanter_degerleme_sekli)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_tutar1_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.tutar1_secenek = (enum_stok_envanter_tutar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_tutar2_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.tutar2_secenek = (enum_stok_envanter_tutar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_tutar3_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.tutar3_secenek = (enum_stok_envanter_tutar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_miktar1_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.miktar1_secenek = (enum_stok_envanter_miktar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_miktar2_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.miktar2_secenek = (enum_stok_envanter_miktar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_miktar3_secenek = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.miktar3_secenek = (enum_stok_envanter_miktar_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_gruplandirma_secenegi = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.gruplandirma_secenegi = (enum_stok_envanter_gruplandirma_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.degistirebilir_siralama_secenegi = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.siralama_secenegi = (enum_siralama_secenekleri)reader.ReadInt32();
		raporStokEnvanterSecenekleri.stokta_olan_urunler = reader.ReadBoolean();
		raporStokEnvanterSecenekleri.fiyat_liste_no = reader.ReadString();
		return raporStokEnvanterSecenekleri;
	}
}
