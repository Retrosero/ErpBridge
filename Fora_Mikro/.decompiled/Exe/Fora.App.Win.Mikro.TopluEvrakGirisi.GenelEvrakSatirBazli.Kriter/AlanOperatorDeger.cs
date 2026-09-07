using System.IO;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public class AlanOperatorDeger
{
	public enum_KriterAramaAlanlari aranacak_alan { get; set; }

	public enum_Operator kullanilan_operator { get; set; }

	public string aranacak_deger { get; set; }

	public AlanOperatorDeger()
	{
		aranacak_alan = enum_KriterAramaAlanlari.cari_kod;
		kullanilan_operator = enum_Operator.Esit;
		aranacak_deger = "";
	}

	public static byte[] WriteToByteArray(AlanOperatorDeger toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(AlanOperatorDeger toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(AlanOperatorDeger toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.aranacak_alan);
		writer.Write((int)toWrite.kullanilan_operator);
		writer.Write(toWrite.aranacak_deger);
		_ = 2;
	}

	public static AlanOperatorDeger ReadFromFile(string FileName)
	{
		FileStream fileStream = new FileStream(FileName, FileMode.Open);
		AlanOperatorDeger result = ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		return result;
	}

	public static AlanOperatorDeger ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		AlanOperatorDeger result = ReadFromStream(memoryStream);
		memoryStream.Close();
		memoryStream.Dispose();
		return result;
	}

	public static AlanOperatorDeger ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static AlanOperatorDeger ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new AlanOperatorDeger
		{
			aranacak_alan = (enum_KriterAramaAlanlari)reader.ReadInt32(),
			kullanilan_operator = (enum_Operator)reader.ReadInt32(),
			aranacak_deger = reader.ReadString()
		};
	}
}
