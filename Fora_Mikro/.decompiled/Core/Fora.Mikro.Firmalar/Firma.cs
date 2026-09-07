using System.IO;

namespace Fora.Mikro.Firmalar;

public class Firma
{
	public string fir_unvan { get; set; }

	public int fir_sirano { get; set; }

	public Firma()
	{
		fir_unvan = "";
		fir_sirano = 0;
	}

	public Firma(int firmano)
	{
		fir_unvan = "";
		fir_sirano = firmano;
	}

	public Firma(int firmano, string firmaunvan)
	{
		fir_unvan = firmaunvan;
		fir_sirano = firmano;
	}

	public static byte[] WriteToByteArray(Firma toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Firma toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(Firma toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.fir_sirano);
		writer.Write(toWrite.fir_unvan);
	}

	public static Firma ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Firma result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Firma ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Firma ReadFromBinaryReader(BinaryReader reader)
	{
		return new Firma
		{
			fir_sirano = reader.ReadInt32(),
			fir_unvan = reader.ReadString()
		};
	}
}
