using System;
using System.Collections.Generic;
using System.IO;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class MeblagveTarih
{
	public int doviz_cinsi { get; set; }

	public string DovizKodu { get; set; }

	public List<double> meblaglar { get; set; }

	public List<DateTime> vade_tarihleri { get; set; }

	public MeblagveTarih()
	{
		meblaglar = new List<double>();
		vade_tarihleri = new List<DateTime>();
		DovizKodu = "";
	}

	public static byte[] WriteToByteArray(MeblagveTarih toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(MeblagveTarih toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(MeblagveTarih toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.doviz_cinsi);
		writer.Write(toWrite.DovizKodu);
		writer.Write(toWrite.meblaglar.Count);
		foreach (double item in toWrite.meblaglar)
		{
			writer.Write(item);
		}
		writer.Write(toWrite.vade_tarihleri.Count);
		foreach (DateTime item2 in toWrite.vade_tarihleri)
		{
			writer.Write(item2.Ticks);
		}
		_ = 2;
	}

	public static MeblagveTarih ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		MeblagveTarih result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static MeblagveTarih ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static MeblagveTarih ReadFromStream(BinaryReader reader)
	{
		reader.ReadInt32();
		MeblagveTarih meblagveTarih = new MeblagveTarih();
		meblagveTarih.doviz_cinsi = reader.ReadInt32();
		meblagveTarih.DovizKodu = reader.ReadString();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			meblagveTarih.meblaglar.Add(reader.ReadDouble());
		}
		num = reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			meblagveTarih.vade_tarihleri.Add(new DateTime(reader.ReadInt64()));
		}
		return meblagveTarih;
	}
}
