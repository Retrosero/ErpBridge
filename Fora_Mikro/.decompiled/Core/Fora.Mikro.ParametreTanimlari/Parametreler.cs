using System.Collections.Generic;
using System.IO;

namespace Fora.Mikro.ParametreTanimlari;

public class Parametreler
{
	public string KullanimAlani { get; set; }

	public List<Parametre> ParametreListesi { get; set; }

	public Parametreler()
	{
		KullanimAlani = "";
		ParametreListesi = new List<Parametre>();
	}

	public Parametre _GetParametre(int ParametreID)
	{
		foreach (Parametre item in ParametreListesi)
		{
			if (ParametreID == item.ParametreID)
			{
				return item;
			}
		}
		return null;
	}

	public Parametre _GetParametre(string ParametreAdi)
	{
		foreach (Parametre item in ParametreListesi)
		{
			if (ParametreAdi == item.ParametreAdi)
			{
				return item;
			}
		}
		return null;
	}

	public Parametre _GetParametre(string AnaGrubu, string AltGrubu, int ParametreID)
	{
		foreach (Parametre item in ParametreListesi)
		{
			if (ParametreID == item.ParametreID && (AnaGrubu == item.ParametreAnaGrubu || AnaGrubu == null) && (AltGrubu == item.ParametreAltGrubu || AltGrubu == null))
			{
				return item;
			}
		}
		return null;
	}

	public Parametre _GetParametre(string AnaGrubu, string AltGrubu, string ParametreAdi)
	{
		foreach (Parametre item in ParametreListesi)
		{
			if (ParametreAdi == item.ParametreAdi && (AnaGrubu == item.ParametreAnaGrubu || AnaGrubu == null) && (AltGrubu == item.ParametreAltGrubu || AltGrubu == null))
			{
				return item;
			}
		}
		return null;
	}

	public static byte[] WriteToByteArray(Parametreler toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Parametreler toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Parametreler toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.KullanimAlani);
		writer.Write(toWrite.ParametreListesi.Count);
		foreach (Parametre item in toWrite.ParametreListesi)
		{
			if (num == 1)
			{
				Parametre.WriteToBinaryWriter(item, writer, 1);
			}
			else
			{
				Parametre.WriteToBinaryWriter(item, writer, 2);
			}
		}
	}

	public static Parametreler ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Parametreler result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Parametreler ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Parametreler ReadFromBinaryReader(BinaryReader reader)
	{
		Parametreler parametreler = new Parametreler();
		reader.ReadInt32();
		parametreler.KullanimAlani = reader.ReadString();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			parametreler.ParametreListesi.Add(Parametre.ReadFromBinaryReader(reader));
		}
		return parametreler;
	}
}
