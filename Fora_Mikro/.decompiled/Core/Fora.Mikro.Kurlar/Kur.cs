using System;
using System.IO;

namespace Fora.Mikro.Kurlar;

public class Kur
{
	public int dov_no;

	public double dov_fiyat { get; set; }

	public DateTime dov_tarih { get; set; }

	public Kur()
	{
		dov_fiyat = 1.0;
		dov_tarih = new DateTime(1900, 1, 1);
	}

	public static byte[] WriteToByteArray(Kur toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Kur toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(Kur toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.dov_fiyat);
		writer.Write(toWrite.dov_no);
		writer.Write(toWrite.dov_tarih.Ticks);
	}

	public static Kur ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Kur result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Kur ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Kur ReadFromBinaryReader(BinaryReader reader)
	{
		return new Kur
		{
			dov_fiyat = reader.ReadDouble(),
			dov_no = reader.ReadInt32(),
			dov_tarih = new DateTime(reader.ReadInt64())
		};
	}
}
