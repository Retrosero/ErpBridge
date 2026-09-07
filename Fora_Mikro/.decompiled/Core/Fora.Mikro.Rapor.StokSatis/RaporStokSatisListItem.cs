using System.Collections.Generic;
using System.IO;

namespace Fora.Mikro.Rapor.StokSatis;

public class RaporStokSatisListItem
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

	public List<string> cari_adresleri { get; set; }

	public double net_tutar { get; set; }

	public double net_maliyet { get; set; }

	public RaporStokSatisListItem()
	{
		kodu = "";
		adi = "";
		birim_adi = "";
		birim2_adi = "";
		birim3_adi = "";
		cari_adresleri = new List<string>();
	}

	public static byte[] WriteToByteArray(RaporStokSatisListItem toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSatisListItem toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokSatisListItem toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
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
		writer.Write(toWrite.net_maliyet);
		if (versiyon >= 2)
		{
			writer.Write(toWrite.birim3_adi);
		}
	}

	public static RaporStokSatisListItem ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokSatisListItem result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokSatisListItem ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static RaporStokSatisListItem ReadFromStream(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporStokSatisListItem raporStokSatisListItem = new RaporStokSatisListItem
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
			net_maliyet = reader.ReadDouble()
		};
		if (num >= 2)
		{
			raporStokSatisListItem.birim3_adi = reader.ReadString();
		}
		return raporStokSatisListItem;
	}
}
