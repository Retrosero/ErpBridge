using System;
using System.IO;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.Evraklar;

public class OfflineEvrakV2
{
	public int OfflineRECno { get; set; }

	public enum_EvrakAktarimDurumu AktarimDurumu { get; set; }

	public bool YeniKayit { get; set; }

	public DateTime AktarilmaTarihi { get; set; }

	public enum_AndroidAktarimTipi Tipi { get; set; }

	public byte[] Evrak { get; set; }

	public string HataString { get; set; }

	public OfflineEvrakV2()
	{
		OfflineRECno = 0;
		AktarimDurumu = enum_EvrakAktarimDurumu.Beklemede;
		YeniKayit = false;
		AktarilmaTarihi = DateTime.Now.AddYears(-10);
		Tipi = enum_AndroidAktarimTipi.Evrak;
		HataString = "";
	}

	public static byte[] WriteToByteArray(OfflineEvrakV2 toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(OfflineEvrakV2 toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(OfflineEvrakV2 toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.OfflineRECno);
		writer.Write((int)toWrite.AktarimDurumu);
		writer.Write(toWrite.YeniKayit);
		writer.Write(toWrite.AktarilmaTarihi.Ticks);
		writer.Write((int)toWrite.Tipi);
		writer.Write(toWrite.Evrak.Length);
		writer.Write(toWrite.Evrak);
		writer.Write(toWrite.HataString);
		_ = 2;
	}

	public static OfflineEvrakV2 ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		OfflineEvrakV2 result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static OfflineEvrakV2 ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static OfflineEvrakV2 ReadFromBinaryReader(BinaryReader reader)
	{
		OfflineEvrakV2 offlineEvrakV = new OfflineEvrakV2();
		reader.ReadInt32();
		offlineEvrakV.OfflineRECno = reader.ReadInt32();
		offlineEvrakV.AktarimDurumu = (enum_EvrakAktarimDurumu)reader.ReadInt32();
		offlineEvrakV.YeniKayit = reader.ReadBoolean();
		offlineEvrakV.AktarilmaTarihi = new DateTime(reader.ReadInt64());
		offlineEvrakV.Tipi = (enum_AndroidAktarimTipi)reader.ReadInt32();
		int count = reader.ReadInt32();
		offlineEvrakV.Evrak = reader.ReadBytes(count);
		offlineEvrakV.HataString = reader.ReadString();
		return offlineEvrakV;
	}
}
