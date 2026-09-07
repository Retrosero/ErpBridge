using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokEnvanter;

public class RaporStokEnvanterSonucHam
{
	public List<RaporStokEnvanterSatir> sonuc_ham { get; set; }

	public List<RaporYardimciStokObje> stoklar { get; set; }

	public List<RaporYardimciGenelObje> stok_ana_gruplari { get; set; }

	public List<RaporYardimciGenelObje> stok_ureticileri { get; set; }

	public List<RaporYardimciGenelObje> stok_markalari { get; set; }

	public List<RaporYardimciGenelObje> stok_reyonlari { get; set; }

	public List<RaporYardimciGenelObje> stok_kategorileri { get; set; }

	public List<RaporYardimciGenelObje> depolar { get; set; }

	public RaporStokEnvanterSonucHam()
	{
		sonuc_ham = new List<RaporStokEnvanterSatir>();
		stoklar = new List<RaporYardimciStokObje>();
		stok_ana_gruplari = new List<RaporYardimciGenelObje>();
		stok_ureticileri = new List<RaporYardimciGenelObje>();
		stok_markalari = new List<RaporYardimciGenelObje>();
		stok_reyonlari = new List<RaporYardimciGenelObje>();
		stok_kategorileri = new List<RaporYardimciGenelObje>();
		depolar = new List<RaporYardimciGenelObje>();
	}

	public static byte[] WriteToByteArray(RaporStokEnvanterSonucHam toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokEnvanterSonucHam toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokEnvanterSonucHam toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sonuc_ham.Count);
		foreach (RaporStokEnvanterSatir item in toWrite.sonuc_ham)
		{
			RaporStokEnvanterSatir.WriteToBinaryWriter(item, writer, 1);
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
			writer.Write(item2.Birim3Adi);
			writer.Write(item2.stok_doviz_cinsi);
		}
		writer.Write(toWrite.stok_ana_gruplari.Count);
		foreach (RaporYardimciGenelObje item3 in toWrite.stok_ana_gruplari)
		{
			writer.Write(item3.Position);
			writer.Write(item3.RecNo);
			writer.Write(item3.Kodu);
			writer.Write(item3.Adi);
		}
		writer.Write(toWrite.stok_ureticileri.Count);
		foreach (RaporYardimciGenelObje item4 in toWrite.stok_ureticileri)
		{
			writer.Write(item4.Position);
			writer.Write(item4.RecNo);
			writer.Write(item4.Kodu);
			writer.Write(item4.Adi);
		}
		writer.Write(toWrite.stok_markalari.Count);
		foreach (RaporYardimciGenelObje item5 in toWrite.stok_markalari)
		{
			writer.Write(item5.Position);
			writer.Write(item5.RecNo);
			writer.Write(item5.Kodu);
			writer.Write(item5.Adi);
		}
		writer.Write(toWrite.stok_reyonlari.Count);
		foreach (RaporYardimciGenelObje item6 in toWrite.stok_reyonlari)
		{
			writer.Write(item6.Position);
			writer.Write(item6.RecNo);
			writer.Write(item6.Kodu);
			writer.Write(item6.Adi);
		}
		writer.Write(toWrite.stok_kategorileri.Count);
		foreach (RaporYardimciGenelObje item7 in toWrite.stok_kategorileri)
		{
			writer.Write(item7.Position);
			writer.Write(item7.RecNo);
			writer.Write(item7.Kodu);
			writer.Write(item7.Adi);
		}
		writer.Write(toWrite.depolar.Count);
		foreach (RaporYardimciGenelObje item8 in toWrite.depolar)
		{
			writer.Write(item8.Position);
			writer.Write(item8.RecNo);
			writer.Write(item8.Kodu);
			writer.Write(item8.Adi);
		}
		writer.Write(toWrite.stoklar.Count);
		foreach (RaporYardimciStokObje item9 in toWrite.stoklar)
		{
			writer.Write(item9.Birim3Adi);
		}
		_ = 2;
	}

	public static RaporStokEnvanterSonucHam ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokEnvanterSonucHam result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokEnvanterSonucHam ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokEnvanterSonucHam ReadFromBinaryReader(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporStokEnvanterSonucHam raporStokEnvanterSonucHam = new RaporStokEnvanterSonucHam();
		int num2 = 0;
		num2 = reader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			raporStokEnvanterSonucHam.sonuc_ham.Add(RaporStokEnvanterSatir.ReadFromBinaryReader(reader));
		}
		num2 = reader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			raporStokEnvanterSonucHam.stoklar.Add(new RaporYardimciStokObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
			raporStokEnvanterSonucHam.stoklar[j].Birim3Adi = reader.ReadString();
			raporStokEnvanterSonucHam.stoklar[j].stok_doviz_cinsi = reader.ReadInt32();
		}
		num2 = reader.ReadInt32();
		for (int k = 0; k < num2; k++)
		{
			raporStokEnvanterSonucHam.stok_ana_gruplari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num2 = reader.ReadInt32();
		for (int l = 0; l < num2; l++)
		{
			raporStokEnvanterSonucHam.stok_ureticileri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num2 = reader.ReadInt32();
		for (int m = 0; m < num2; m++)
		{
			raporStokEnvanterSonucHam.stok_markalari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num2 = reader.ReadInt32();
		for (int n = 0; n < num2; n++)
		{
			raporStokEnvanterSonucHam.stok_reyonlari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num2 = reader.ReadInt32();
		for (int num3 = 0; num3 < num2; num3++)
		{
			raporStokEnvanterSonucHam.stok_kategorileri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num2 = reader.ReadInt32();
		for (int num4 = 0; num4 < num2; num4++)
		{
			raporStokEnvanterSonucHam.depolar.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num2 = reader.ReadInt32();
		for (int num5 = 0; num5 < num2; num5++)
		{
			raporStokEnvanterSonucHam.stoklar[num5].Birim3Adi = reader.ReadString();
		}
		_ = 2;
		return raporStokEnvanterSonucHam;
	}
}
