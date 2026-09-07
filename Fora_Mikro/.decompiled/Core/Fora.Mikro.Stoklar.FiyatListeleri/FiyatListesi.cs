using System.IO;

namespace Fora.Mikro.Stoklar.FiyatListeleri;

public class FiyatListesi
{
	public int sfl_sirano { get; set; }

	public string sfl_aciklama { get; set; }

	public bool sfl_kdvdahil { get; set; }

	public FiyatListesi()
	{
		sfl_sirano = 1;
		sfl_aciklama = "";
		sfl_kdvdahil = false;
	}

	public static byte[] WriteToByteArray(FiyatListesi toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(FiyatListesi toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(FiyatListesi toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.sfl_sirano);
		writer.Write(toWrite.sfl_aciklama);
		writer.Write(toWrite.sfl_kdvdahil);
	}

	public static FiyatListesi ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		FiyatListesi result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static FiyatListesi ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static FiyatListesi ReadFromBinaryReader(BinaryReader reader)
	{
		return new FiyatListesi
		{
			sfl_sirano = reader.ReadInt32(),
			sfl_aciklama = reader.ReadString(),
			sfl_kdvdahil = reader.ReadBoolean()
		};
	}
}
