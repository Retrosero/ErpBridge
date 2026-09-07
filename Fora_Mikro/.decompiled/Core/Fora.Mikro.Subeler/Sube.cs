using System.IO;

namespace Fora.Mikro.Subeler;

public class Sube
{
	public string Sube_adi { get; set; }

	public int Sube_no { get; set; }

	public Sube()
	{
		Sube_adi = "";
		Sube_no = 0;
	}

	public Sube(int subeno)
	{
		Sube_adi = "";
		Sube_no = subeno;
	}

	public Sube(int subeno, string subeadi)
	{
		Sube_adi = subeadi;
		Sube_no = subeno;
	}

	public static byte[] WriteToByteArray(Sube toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Sube toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(Sube toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.Sube_no);
		writer.Write(toWrite.Sube_adi);
	}

	public static Sube ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Sube result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Sube ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Sube ReadFromBinaryReader(BinaryReader reader)
	{
		return new Sube
		{
			Sube_no = reader.ReadInt32(),
			Sube_adi = reader.ReadString()
		};
	}
}
