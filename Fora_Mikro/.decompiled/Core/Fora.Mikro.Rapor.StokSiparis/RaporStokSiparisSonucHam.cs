using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisSonucHam
{
	public List<RaporStokSiparisSatir> sonuc_ham { get; set; }

	public List<RaporYardimciStokObje> stoklar { get; set; }

	public List<RaporYardimciCariObje> cariler { get; set; }

	public List<RaporYardimciGenelObje> stok_ana_gruplari { get; set; }

	public List<RaporYardimciGenelObje> stok_ureticileri { get; set; }

	public List<RaporYardimciGenelObje> stok_markalari { get; set; }

	public List<RaporYardimciGenelObje> stok_reyonlari { get; set; }

	public List<RaporYardimciGenelObje> stok_kategorileri { get; set; }

	public List<RaporYardimciGenelObje> cari_bolgeleri { get; set; }

	public List<RaporYardimciGenelObje> cari_gruplari { get; set; }

	public List<RaporYardimciGenelObje> temsilciler { get; set; }

	public List<RaporYardimciGenelObje> depolar { get; set; }

	public List<RaporYardimciGenelObje> sorumluluk_merkezleri { get; set; }

	public List<RaporYardimciGenelObje> projeler { get; set; }

	public RaporStokSiparisSonucHam()
	{
		sonuc_ham = new List<RaporStokSiparisSatir>();
		stoklar = new List<RaporYardimciStokObje>();
		cariler = new List<RaporYardimciCariObje>();
		stok_ana_gruplari = new List<RaporYardimciGenelObje>();
		stok_ureticileri = new List<RaporYardimciGenelObje>();
		stok_markalari = new List<RaporYardimciGenelObje>();
		stok_reyonlari = new List<RaporYardimciGenelObje>();
		stok_kategorileri = new List<RaporYardimciGenelObje>();
		cari_bolgeleri = new List<RaporYardimciGenelObje>();
		cari_gruplari = new List<RaporYardimciGenelObje>();
		temsilciler = new List<RaporYardimciGenelObje>();
		depolar = new List<RaporYardimciGenelObje>();
		sorumluluk_merkezleri = new List<RaporYardimciGenelObje>();
		projeler = new List<RaporYardimciGenelObje>();
	}

	public static byte[] WriteToByteArray(RaporStokSiparisSonucHam toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSiparisSonucHam toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokSiparisSonucHam toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sonuc_ham.Count);
		foreach (RaporStokSiparisSatir item in toWrite.sonuc_ham)
		{
			RaporStokSiparisSatir.WriteToStream(item, writer);
		}
		writer.Write(toWrite.stoklar.Count);
		foreach (RaporYardimciStokObje item2 in toWrite.stoklar)
		{
			writer.Write(item2.Position);
			writer.Write(item2.RecNo);
			writer.Write(item2.Kodu);
			writer.Write(item2.Adi);
			writer.Write(item2.Birim1Adi);
			writer.Write(item2.Birim2Adi);
			writer.Write(item2.stok_anagrup_sira_no);
			writer.Write(item2.stok_uretici_sira_no);
			writer.Write(item2.stok_marka_sira_no);
			writer.Write(item2.stok_reyon_sira_no);
			writer.Write(item2.stok_kategori_sira_no);
		}
		writer.Write(toWrite.cariler.Count);
		foreach (RaporYardimciCariObje item3 in toWrite.cariler)
		{
			writer.Write(item3.Position);
			writer.Write(item3.RecNo);
			writer.Write(item3.Kodu);
			writer.Write(item3.Adi);
			writer.Write(item3.cari_bolge_sira_no);
			writer.Write(item3.cari_grup_sira_no);
		}
		writer.Write(toWrite.stok_ana_gruplari.Count);
		foreach (RaporYardimciGenelObje item4 in toWrite.stok_ana_gruplari)
		{
			writer.Write(item4.Position);
			writer.Write(item4.RecNo);
			writer.Write(item4.Kodu);
			writer.Write(item4.Adi);
		}
		writer.Write(toWrite.stok_ureticileri.Count);
		foreach (RaporYardimciGenelObje item5 in toWrite.stok_ureticileri)
		{
			writer.Write(item5.Position);
			writer.Write(item5.RecNo);
			writer.Write(item5.Kodu);
			writer.Write(item5.Adi);
		}
		writer.Write(toWrite.stok_markalari.Count);
		foreach (RaporYardimciGenelObje item6 in toWrite.stok_markalari)
		{
			writer.Write(item6.Position);
			writer.Write(item6.RecNo);
			writer.Write(item6.Kodu);
			writer.Write(item6.Adi);
		}
		writer.Write(toWrite.stok_reyonlari.Count);
		foreach (RaporYardimciGenelObje item7 in toWrite.stok_reyonlari)
		{
			writer.Write(item7.Position);
			writer.Write(item7.RecNo);
			writer.Write(item7.Kodu);
			writer.Write(item7.Adi);
		}
		writer.Write(toWrite.stok_kategorileri.Count);
		foreach (RaporYardimciGenelObje item8 in toWrite.stok_kategorileri)
		{
			writer.Write(item8.Position);
			writer.Write(item8.RecNo);
			writer.Write(item8.Kodu);
			writer.Write(item8.Adi);
		}
		writer.Write(toWrite.cari_bolgeleri.Count);
		foreach (RaporYardimciGenelObje item9 in toWrite.cari_bolgeleri)
		{
			writer.Write(item9.Position);
			writer.Write(item9.RecNo);
			writer.Write(item9.Kodu);
			writer.Write(item9.Adi);
		}
		writer.Write(toWrite.cari_gruplari.Count);
		foreach (RaporYardimciGenelObje item10 in toWrite.cari_gruplari)
		{
			writer.Write(item10.Position);
			writer.Write(item10.RecNo);
			writer.Write(item10.Kodu);
			writer.Write(item10.Adi);
		}
		writer.Write(toWrite.temsilciler.Count);
		foreach (RaporYardimciGenelObje item11 in toWrite.temsilciler)
		{
			writer.Write(item11.Position);
			writer.Write(item11.RecNo);
			writer.Write(item11.Kodu);
			writer.Write(item11.Adi);
		}
		writer.Write(toWrite.depolar.Count);
		foreach (RaporYardimciGenelObje item12 in toWrite.depolar)
		{
			writer.Write(item12.Position);
			writer.Write(item12.RecNo);
			writer.Write(item12.Kodu);
			writer.Write(item12.Adi);
		}
		writer.Write(toWrite.sorumluluk_merkezleri.Count);
		foreach (RaporYardimciGenelObje item13 in toWrite.sorumluluk_merkezleri)
		{
			writer.Write(item13.Position);
			writer.Write(item13.RecNo);
			writer.Write(item13.Kodu);
			writer.Write(item13.Adi);
		}
		writer.Write(toWrite.projeler.Count);
		foreach (RaporYardimciGenelObje item14 in toWrite.projeler)
		{
			writer.Write(item14.Position);
			writer.Write(item14.RecNo);
			writer.Write(item14.Kodu);
			writer.Write(item14.Adi);
		}
		_ = 2;
	}

	public static RaporStokSiparisSonucHam ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokSiparisSonucHam result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokSiparisSonucHam ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokSiparisSonucHam ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		RaporStokSiparisSonucHam raporStokSiparisSonucHam = new RaporStokSiparisSonucHam();
		int num = 0;
		num = reader.ReadInt32();
		raporStokSiparisSonucHam.sonuc_ham = new List<RaporStokSiparisSatir>();
		for (int i = 0; i < num; i++)
		{
			raporStokSiparisSonucHam.sonuc_ham.Add(RaporStokSiparisSatir.ReadFromStream(reader));
		}
		num = reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			raporStokSiparisSonucHam.stoklar.Add(new RaporYardimciStokObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
		}
		num = reader.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			raporStokSiparisSonucHam.cariler.Add(new RaporYardimciCariObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32()));
		}
		num = reader.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			raporStokSiparisSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int m = 0; m < num; m++)
		{
			raporStokSiparisSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int n = 0; n < num; n++)
		{
			raporStokSiparisSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num2 = 0; num2 < num; num2++)
		{
			raporStokSiparisSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num3 = 0; num3 < num; num3++)
		{
			raporStokSiparisSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num4 = 0; num4 < num; num4++)
		{
			raporStokSiparisSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num5 = 0; num5 < num; num5++)
		{
			raporStokSiparisSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num6 = 0; num6 < num; num6++)
		{
			raporStokSiparisSonucHam.temsilciler.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num7 = 0; num7 < num; num7++)
		{
			raporStokSiparisSonucHam.depolar.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num8 = 0; num8 < num; num8++)
		{
			raporStokSiparisSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num9 = 0; num9 < num; num9++)
		{
			raporStokSiparisSonucHam.projeler.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		return raporStokSiparisSonucHam;
	}
}
