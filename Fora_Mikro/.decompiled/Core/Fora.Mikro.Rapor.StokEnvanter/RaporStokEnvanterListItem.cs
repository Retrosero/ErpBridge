using System.IO;

namespace Fora.Mikro.Rapor.StokEnvanter;

public class RaporStokEnvanterListItem
{
	public int liste_sira_no { get; set; }

	public string kodu { get; set; }

	public string adi { get; set; }

	public string birim_adi { get; set; }

	public string birim2_adi { get; set; }

	public string birim3_adi { get; set; }

	public double tutar1 { get; set; }

	public double tutar2 { get; set; }

	public double tutar3 { get; set; }

	public double miktar1 { get; set; }

	public double miktar2 { get; set; }

	public double miktar3 { get; set; }

	public int doviz_cinsi { get; set; }

	public double net_tutar { get; set; }

	public RaporStokEnvanterListItem()
	{
		kodu = "";
		adi = "";
		birim_adi = "";
		birim2_adi = "";
		birim3_adi = "";
	}

	public static byte[] WriteToByteArray(RaporStokEnvanterListItem toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokEnvanterListItem toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokEnvanterListItem toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.liste_sira_no);
		writer.Write(toWrite.kodu);
		writer.Write(toWrite.adi);
		writer.Write(toWrite.tutar1);
		writer.Write(toWrite.tutar2);
		writer.Write(toWrite.tutar3);
		writer.Write(toWrite.miktar1);
		writer.Write(toWrite.miktar2);
		writer.Write(toWrite.miktar3);
		writer.Write(toWrite.birim_adi);
		writer.Write(toWrite.birim2_adi);
		writer.Write(toWrite.net_tutar);
		writer.Write(toWrite.birim3_adi);
		writer.Write(toWrite.doviz_cinsi);
		_ = 2;
	}

	public static RaporStokEnvanterListItem ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokEnvanterListItem result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokEnvanterListItem ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static RaporStokEnvanterListItem ReadFromStream(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporStokEnvanterListItem result = new RaporStokEnvanterListItem
		{
			liste_sira_no = reader.ReadInt32(),
			kodu = reader.ReadString(),
			adi = reader.ReadString(),
			tutar1 = reader.ReadDouble(),
			tutar2 = reader.ReadDouble(),
			tutar3 = reader.ReadDouble(),
			miktar1 = reader.ReadDouble(),
			miktar2 = reader.ReadDouble(),
			miktar3 = reader.ReadDouble(),
			birim_adi = reader.ReadString(),
			birim2_adi = reader.ReadString(),
			net_tutar = reader.ReadDouble(),
			birim3_adi = reader.ReadString(),
			doviz_cinsi = reader.ReadInt32()
		};
		_ = 2;
		return result;
	}
}
