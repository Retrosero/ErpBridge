using System.Collections.Generic;
using System.IO;

namespace Fora.Mikro.Utility;

public class GenelListArray
{
	public List<GenelList> genel_list { get; set; }

	public GenelListArray()
	{
		genel_list = new List<GenelList>();
	}

	public static byte[] WriteToByteArray(GenelListArray toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(GenelListArray toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(GenelListArray toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.genel_list.Count);
		foreach (GenelList item in toWrite.genel_list)
		{
			writer.Write(item.Kod);
			writer.Write(item.Text);
		}
		_ = 2;
	}

	public static GenelListArray ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		GenelListArray result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static GenelListArray ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static GenelListArray ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		GenelListArray genelListArray = new GenelListArray();
		int num = reader.ReadInt32();
		genelListArray.genel_list = new List<GenelList>();
		for (int i = 0; i < num; i++)
		{
			genelListArray.genel_list.Add(new GenelList(reader.ReadString(), reader.ReadString()));
		}
		return genelListArray;
	}
}
