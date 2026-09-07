using System.IO;

namespace Fora.Mikro.SorumlulukMerkezleri;

public class SorumlulukMerkezi
{
	public string som_kod { get; set; }

	public string som_isim { get; set; }

	public string som_MuhArtikeli { get; set; }

	public SorumlulukMerkezi()
	{
		som_kod = "";
		som_isim = "";
		som_MuhArtikeli = "";
	}

	public SorumlulukMerkezi(string SorumlulukMerkeziKodu)
	{
		som_kod = SorumlulukMerkeziKodu;
		som_isim = "";
		som_MuhArtikeli = "";
	}

	public static byte[] WriteToByteArray(SorumlulukMerkezi toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(SorumlulukMerkezi toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(SorumlulukMerkezi toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.som_kod);
		writer.Write(toWrite.som_isim);
		writer.Write(toWrite.som_MuhArtikeli);
	}

	public static SorumlulukMerkezi ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		SorumlulukMerkezi result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static SorumlulukMerkezi ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static SorumlulukMerkezi ReadFromBinaryReader(BinaryReader reader)
	{
		return new SorumlulukMerkezi
		{
			som_kod = reader.ReadString(),
			som_isim = reader.ReadString(),
			som_MuhArtikeli = reader.ReadString()
		};
	}
}
