using System.IO;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisListItem
{
	public int liste_sira_no { get; set; }

	public string kodu { get; set; }

	public string adi { get; set; }

	public double tutar1 { get; set; }

	public double tutar2 { get; set; }

	public double tutar3 { get; set; }

	public double miktar1 { get; set; }

	public double miktar2 { get; set; }

	public double miktar3 { get; set; }

	public RaporStokSiparisListItem()
	{
		kodu = "";
		adi = "";
	}

	public static byte[] getBytesWithManualWrite(RaporStokSiparisListItem toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSiparisListItem toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToStream(toWrite, writer);
	}

	public static void WriteToStream(RaporStokSiparisListItem toWrite, BinaryWriter writer)
	{
		int value = 1;
		writer.Write(value);
		writer.Write(toWrite.liste_sira_no);
		writer.Write(toWrite.kodu);
		writer.Write(toWrite.adi);
		writer.Write(toWrite.tutar1);
		writer.Write(toWrite.tutar2);
		writer.Write(toWrite.tutar3);
		writer.Write(toWrite.miktar1);
		writer.Write(toWrite.miktar2);
		writer.Write(toWrite.miktar3);
	}

	public static RaporStokSiparisListItem ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static RaporStokSiparisListItem ReadFromStream(BinaryReader reader)
	{
		reader.ReadInt32();
		return new RaporStokSiparisListItem
		{
			liste_sira_no = reader.ReadInt32(),
			kodu = reader.ReadString(),
			adi = reader.ReadString(),
			tutar1 = reader.ReadDouble(),
			tutar2 = reader.ReadDouble(),
			tutar3 = reader.ReadDouble(),
			miktar1 = reader.ReadDouble(),
			miktar2 = reader.ReadDouble(),
			miktar3 = reader.ReadDouble()
		};
	}
}
