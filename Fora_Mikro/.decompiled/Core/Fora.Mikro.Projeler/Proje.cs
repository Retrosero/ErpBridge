using System.IO;

namespace Fora.Mikro.Projeler;

public class Proje
{
	public string pro_kodu { get; set; }

	public string pro_adi { get; set; }

	public string pro_musterikodu { get; set; }

	public string pro_sormerkodu { get; set; }

	public string pro_grupkodu { get; set; }

	public string pro_sektorkodu { get; set; }

	public string pro_bolgekodu { get; set; }

	public string pro_ana_projekodu { get; set; }

	public string pro_aciklama { get; set; }

	public string pro_muh_kod_artikeli { get; set; }

	public Proje()
	{
		pro_kodu = "";
		pro_adi = "";
		pro_musterikodu = "";
		pro_sormerkodu = "";
		pro_grupkodu = "";
		pro_sektorkodu = "";
		pro_bolgekodu = "";
		pro_ana_projekodu = "";
		pro_aciklama = "";
		pro_muh_kod_artikeli = "";
	}

	public Proje(string ProjeKodu)
	{
		pro_kodu = ProjeKodu;
		pro_adi = "";
		pro_musterikodu = "";
		pro_sormerkodu = "";
		pro_grupkodu = "";
		pro_sektorkodu = "";
		pro_bolgekodu = "";
		pro_ana_projekodu = "";
		pro_aciklama = "";
		pro_muh_kod_artikeli = "";
	}

	public static byte[] WriteToByteArray(Proje toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Proje toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(Proje toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.pro_kodu);
		writer.Write(toWrite.pro_aciklama);
		writer.Write(toWrite.pro_adi);
		writer.Write(toWrite.pro_ana_projekodu);
		writer.Write(toWrite.pro_bolgekodu);
		writer.Write(toWrite.pro_grupkodu);
		writer.Write(toWrite.pro_muh_kod_artikeli);
		writer.Write(toWrite.pro_musterikodu);
		writer.Write(toWrite.pro_sektorkodu);
		writer.Write(toWrite.pro_sormerkodu);
	}

	public static Proje ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Proje result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Proje ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Proje ReadFromBinaryReader(BinaryReader reader)
	{
		return new Proje
		{
			pro_kodu = reader.ReadString(),
			pro_aciklama = reader.ReadString(),
			pro_adi = reader.ReadString(),
			pro_ana_projekodu = reader.ReadString(),
			pro_bolgekodu = reader.ReadString(),
			pro_grupkodu = reader.ReadString(),
			pro_muh_kod_artikeli = reader.ReadString(),
			pro_musterikodu = reader.ReadString(),
			pro_sektorkodu = reader.ReadString(),
			pro_sormerkodu = reader.ReadString()
		};
	}
}
