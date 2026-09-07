using System.IO;

namespace Fora.Mikro.Stoklar;

public class StokBase
{
	public int sto_RECno { get; set; }

	public int sto_RECid_RECno { get; set; }

	public string sto_kod { get; set; }

	public string sto_isim { get; set; }

	public string sto_kisa_ismi { get; set; }

	public string sto_yabanci_isim { get; set; }

	public string sto_birim1_ad { get; set; }

	public double miktar { get; set; }

	public string GetStokKodStokIsim => "(" + sto_kod + ") " + sto_isim;

	public string GetStokKodStokIsimBirim1Ad => "(" + sto_kod + ") " + sto_isim + " (" + sto_birim1_ad + ")";

	public StokBase()
	{
		sto_RECno = 0;
		sto_RECid_RECno = 0;
		sto_kod = "";
		sto_isim = "";
		sto_kisa_ismi = "";
		sto_yabanci_isim = "";
		sto_birim1_ad = "";
		miktar = 0.0;
	}

	public static byte[] WriteToByteArray(StokBase toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(StokBase toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(StokBase toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sto_RECno);
		writer.Write(toWrite.sto_RECid_RECno);
		writer.Write(toWrite.sto_kod);
		writer.Write(toWrite.sto_isim);
		writer.Write(toWrite.sto_kisa_ismi);
		writer.Write(toWrite.sto_yabanci_isim);
		writer.Write(toWrite.sto_birim1_ad);
		writer.Write(toWrite.miktar);
		_ = 2;
	}

	public static StokBase ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		StokBase result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static StokBase ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static StokBase ReadFromBinaryReader(BinaryReader reader)
	{
		StokBase stokBase = new StokBase();
		reader.ReadInt32();
		stokBase.sto_RECno = reader.ReadInt32();
		stokBase.sto_RECid_RECno = reader.ReadInt32();
		stokBase.sto_kod = reader.ReadString();
		stokBase.sto_isim = reader.ReadString();
		stokBase.sto_kisa_ismi = reader.ReadString();
		stokBase.sto_yabanci_isim = reader.ReadString();
		stokBase.sto_birim1_ad = reader.ReadString();
		stokBase.miktar = reader.ReadDouble();
		return stokBase;
	}
}
