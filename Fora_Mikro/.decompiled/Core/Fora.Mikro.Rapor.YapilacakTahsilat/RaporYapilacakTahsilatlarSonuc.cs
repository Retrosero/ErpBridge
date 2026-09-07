using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class RaporYapilacakTahsilatlarSonuc
{
	public int satir_sayisi => list_items.Count;

	public enum_yapilacak_tahsilatlar_gruplandirma_secenekleri gruplandirma_secenegi { get; set; }

	public enum_yapilacak_tahsilatlar_siralama_secenekleri siralama_secenegi { get; set; }

	public List<RaporYapilacakTahsilatlarListItem> list_items { get; set; }

	public RaporYapilacakTahsilatlarSonuc()
	{
		list_items = new List<RaporYapilacakTahsilatlarListItem>();
	}

	public static byte[] WriteToByteArray(RaporYapilacakTahsilatlarSonuc toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporYapilacakTahsilatlarSonuc toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporYapilacakTahsilatlarSonuc toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.gruplandirma_secenegi);
		writer.Write((int)toWrite.siralama_secenegi);
		writer.Write(toWrite.list_items.Count);
		foreach (RaporYapilacakTahsilatlarListItem list_item in toWrite.list_items)
		{
			RaporYapilacakTahsilatlarListItem.WriteToBinaryWriter(list_item, writer, 1);
		}
		_ = 2;
	}

	public static RaporYapilacakTahsilatlarSonuc ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporYapilacakTahsilatlarSonuc result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporYapilacakTahsilatlarSonuc ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporYapilacakTahsilatlarSonuc ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		RaporYapilacakTahsilatlarSonuc raporYapilacakTahsilatlarSonuc = new RaporYapilacakTahsilatlarSonuc();
		raporYapilacakTahsilatlarSonuc.gruplandirma_secenegi = (enum_yapilacak_tahsilatlar_gruplandirma_secenekleri)reader.ReadInt32();
		raporYapilacakTahsilatlarSonuc.siralama_secenegi = (enum_yapilacak_tahsilatlar_siralama_secenekleri)reader.ReadInt32();
		int num = reader.ReadInt32();
		raporYapilacakTahsilatlarSonuc.list_items = new List<RaporYapilacakTahsilatlarListItem>();
		for (int i = 0; i < num; i++)
		{
			raporYapilacakTahsilatlarSonuc.list_items.Add(RaporYapilacakTahsilatlarListItem.ReadFromStream(reader));
		}
		return raporYapilacakTahsilatlarSonuc;
	}
}
