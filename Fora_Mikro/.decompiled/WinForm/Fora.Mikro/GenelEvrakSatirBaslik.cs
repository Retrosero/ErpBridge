using System.ComponentModel;
using System.IO;

namespace Fora.Mikro;

public class GenelEvrakSatirBaslik
{
	public BindingList<GenelEvrakSatir> _satirlar { get; set; }

	public GenelEvrakSatirBaslik()
	{
		_satirlar = new BindingList<GenelEvrakSatir>();
	}

	public static byte[] WriteToByteArray(GenelEvrakSatirBaslik toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(GenelEvrakSatirBaslik toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(GenelEvrakSatirBaslik toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite._satirlar.Count);
		foreach (GenelEvrakSatir item in toWrite._satirlar)
		{
			GenelEvrakSatir.WriteToBinaryWriter(item, writer, 999);
		}
		_ = 2;
	}

	public static GenelEvrakSatirBaslik ReadFromFile(string FileName)
	{
		FileStream fileStream = new FileStream(FileName, FileMode.Open);
		GenelEvrakSatirBaslik result = ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		return result;
	}

	public static GenelEvrakSatirBaslik ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		GenelEvrakSatirBaslik result = ReadFromStream(memoryStream);
		memoryStream.Close();
		memoryStream.Dispose();
		return result;
	}

	public static GenelEvrakSatirBaslik ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static GenelEvrakSatirBaslik ReadFromBinaryReader(BinaryReader reader)
	{
		GenelEvrakSatirBaslik genelEvrakSatirBaslik = new GenelEvrakSatirBaslik();
		int num = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			genelEvrakSatirBaslik._satirlar.Add(GenelEvrakSatir.ReadFromBinaryReader(reader));
		}
		_ = 2;
		return genelEvrakSatirBaslik;
	}
}
