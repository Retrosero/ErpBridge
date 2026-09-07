using System.ComponentModel;
using System.IO;

namespace Fora.Mikro;

public class TahsilatEvrakSatirBaslik
{
	public BindingList<TahsilatEvrakSatir> _satirlar { get; set; }

	public TahsilatEvrakSatirBaslik()
	{
		_satirlar = new BindingList<TahsilatEvrakSatir>();
	}

	public static byte[] WriteToByteArray(TahsilatEvrakSatirBaslik toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(TahsilatEvrakSatirBaslik toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(TahsilatEvrakSatirBaslik toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite._satirlar.Count);
		foreach (TahsilatEvrakSatir item in toWrite._satirlar)
		{
			TahsilatEvrakSatir.WriteToBinaryWriter(item, writer, 1);
		}
		_ = 2;
	}

	public static TahsilatEvrakSatirBaslik ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		TahsilatEvrakSatirBaslik result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static TahsilatEvrakSatirBaslik ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static TahsilatEvrakSatirBaslik ReadFromBinaryReader(BinaryReader reader)
	{
		TahsilatEvrakSatirBaslik tahsilatEvrakSatirBaslik = new TahsilatEvrakSatirBaslik();
		int num = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			tahsilatEvrakSatirBaslik._satirlar.Add(TahsilatEvrakSatir.ReadFromBinaryReader(reader));
		}
		_ = 2;
		return tahsilatEvrakSatirBaslik;
	}
}
