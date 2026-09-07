using System.IO;

namespace Fora.Mikro.Masraflar;

public class Masraf
{
	public int his_RECno { get; set; }

	public string his_kod { get; set; }

	public string his_isim { get; set; }

	public string his_yabanci_isim { get; set; }

	public string his_tipkod { get; set; }

	public string his_sinifkod { get; set; }

	public string his_grupkod { get; set; }

	public int his_dovcinsi { get; set; }

	public int his_oivuygulama { get; set; }

	public double his_oivtutar { get; set; }

	public double tutar { get; set; }

	public string aciklama { get; set; }

	public int vergi_pntr { get; set; }

	public bool kdvdahil { get; set; }

	public Masraf()
	{
		his_RECno = 0;
		his_kod = "";
		his_isim = "";
		his_yabanci_isim = "";
		his_tipkod = "";
		his_sinifkod = "";
		his_grupkod = "";
		his_dovcinsi = 0;
		his_oivuygulama = 0;
		his_oivtutar = 0.0;
		tutar = 0.0;
		aciklama = "";
		vergi_pntr = 4;
		kdvdahil = true;
	}

	public static byte[] WriteToByteArray(Masraf toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Masraf toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Masraf toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.his_RECno);
		writer.Write(toWrite.his_kod);
		writer.Write(toWrite.his_isim);
		writer.Write(toWrite.his_yabanci_isim);
		writer.Write(toWrite.his_tipkod);
		writer.Write(toWrite.his_sinifkod);
		writer.Write(toWrite.his_grupkod);
		writer.Write(toWrite.his_dovcinsi);
		writer.Write(toWrite.his_oivuygulama);
		writer.Write(toWrite.his_oivtutar);
		writer.Write(toWrite.tutar);
		writer.Write(toWrite.aciklama);
		writer.Write(toWrite.vergi_pntr);
		writer.Write(toWrite.kdvdahil);
		_ = 2;
	}

	public static Masraf ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Masraf result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Masraf ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Masraf ReadFromBinaryReader(BinaryReader reader)
	{
		Masraf masraf = new Masraf();
		reader.ReadInt32();
		masraf.his_RECno = reader.ReadInt32();
		masraf.his_kod = reader.ReadString();
		masraf.his_isim = reader.ReadString();
		masraf.his_yabanci_isim = reader.ReadString();
		masraf.his_tipkod = reader.ReadString();
		masraf.his_sinifkod = reader.ReadString();
		masraf.his_grupkod = reader.ReadString();
		masraf.his_dovcinsi = reader.ReadInt32();
		masraf.his_oivuygulama = reader.ReadInt32();
		masraf.his_oivtutar = reader.ReadDouble();
		masraf.tutar = reader.ReadDouble();
		masraf.aciklama = reader.ReadString();
		masraf.vergi_pntr = reader.ReadInt32();
		masraf.kdvdahil = reader.ReadBoolean();
		return masraf;
	}
}
