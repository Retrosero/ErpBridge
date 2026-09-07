using System;
using System.IO;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.Temsilciler;

public class GuneBaslaBitir
{
	public enum_GuneBaslaBitir Tipi { get; set; }

	public string Temsilci_Kodu { get; set; }

	public DateTime Tarih { get; set; }

	public DateTime Saati { get; set; }

	public float Enlem { get; set; }

	public float Boylam { get; set; }

	public int Arac_Km { get; set; }

	public string Mesaj { get; set; }

	public GuneBaslaBitir()
	{
		Temsilci_Kodu = "";
		Tarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		Saati = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		Saati.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddSeconds(DateTime.Now.Second);
		Enlem = 0f;
		Boylam = 0f;
		Arac_Km = 0;
		Mesaj = "";
	}

	public static byte[] WriteToByteArray(GuneBaslaBitir toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(GuneBaslaBitir toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(GuneBaslaBitir toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.Tipi);
		writer.Write(toWrite.Temsilci_Kodu);
		writer.Write(toWrite.Tarih.Ticks);
		writer.Write(toWrite.Saati.Ticks);
		writer.Write(toWrite.Enlem);
		writer.Write(toWrite.Boylam);
		writer.Write(toWrite.Arac_Km);
		writer.Write(toWrite.Mesaj);
		_ = 2;
	}

	public static GuneBaslaBitir ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		GuneBaslaBitir result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static GuneBaslaBitir ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static GuneBaslaBitir ReadFromBinaryReader(BinaryReader reader)
	{
		GuneBaslaBitir guneBaslaBitir = new GuneBaslaBitir();
		reader.ReadInt32();
		guneBaslaBitir.Tipi = (enum_GuneBaslaBitir)reader.ReadInt32();
		guneBaslaBitir.Temsilci_Kodu = reader.ReadString();
		guneBaslaBitir.Tarih = new DateTime(reader.ReadInt64());
		guneBaslaBitir.Saati = new DateTime(reader.ReadInt64());
		guneBaslaBitir.Enlem = reader.ReadSingle();
		guneBaslaBitir.Boylam = reader.ReadSingle();
		guneBaslaBitir.Arac_Km = reader.ReadInt32();
		guneBaslaBitir.Mesaj = reader.ReadString();
		return guneBaslaBitir;
	}
}
