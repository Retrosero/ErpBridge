using System.IO;

namespace Fora.Mikro.Depolar;

public class Depo
{
	public string dep_adi { get; set; }

	public int dep_no { get; set; }

	public Depo()
	{
		dep_adi = "";
		dep_no = 1;
	}

	public Depo(int DepoNo)
	{
		dep_no = DepoNo;
	}

	public static byte[] WriteToByteArray(Depo toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Depo toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(Depo toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.dep_no);
		writer.Write(toWrite.dep_adi);
	}

	public static Depo ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Depo result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Depo ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Depo ReadFromBinaryReader(BinaryReader reader)
	{
		return new Depo
		{
			dep_no = reader.ReadInt32(),
			dep_adi = reader.ReadString()
		};
	}
}
