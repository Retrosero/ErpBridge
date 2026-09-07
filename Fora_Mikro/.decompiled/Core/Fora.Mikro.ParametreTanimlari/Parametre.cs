using System;
using System.IO;

namespace Fora.Mikro.ParametreTanimlari;

public class Parametre
{
	public int EskiID { get; set; }

	public Guid? IDGuid { get; set; }

	public string ParametreProgram { get; set; }

	public string ParametreUser { get; set; }

	public string ParametreAnaGrubu { get; set; }

	public string ParametreAltGrubu { get; set; }

	public int ParametreID { get; set; }

	public string ParametreAdi { get; set; }

	public string ParametreDefaultDegeri { get; set; }

	public string ParametreDBDegeri { get; set; }

	public string ParametreDegeri { get; set; }

	public string _GetString => ParametreDegeri;

	public int _GetInt
	{
		get
		{
			int result = 0;
			try
			{
				result = int.Parse(ParametreDegeri);
			}
			catch
			{
			}
			return result;
		}
	}

	public bool _GetBoolean
	{
		get
		{
			if (ParametreDegeri == "1")
			{
				return true;
			}
			return false;
		}
	}

	public double _GetDouble => double.Parse(ParametreDegeri);

	public float _GetFloat => float.Parse(ParametreDegeri);

	public DateTime _GetDateTime => DateTime.Parse(ParametreDegeri);

	public TimeSpan _GetTimeSpan => TimeSpan.Parse(ParametreDegeri);

	public string _SetString
	{
		set
		{
			ParametreDegeri = value;
		}
	}

	public int _SetInt
	{
		set
		{
			ParametreDegeri = value.ToString();
		}
	}

	public bool _SetBoolean
	{
		set
		{
			if (value)
			{
				ParametreDegeri = "1";
			}
			else
			{
				ParametreDegeri = "0";
			}
		}
	}

	public double _SetDouble
	{
		set
		{
			ParametreDegeri = value.ToString();
		}
	}

	public float _SetFloat
	{
		set
		{
			ParametreDegeri = value.ToString();
		}
	}

	public DateTime _SetDateTime
	{
		set
		{
			ParametreDegeri = value.ToString();
		}
	}

	public TimeSpan _SetTimeSpan
	{
		set
		{
			ParametreDegeri = value.ToString();
		}
	}

	public Parametre()
	{
		EskiID = 0;
		ParametreProgram = "";
		ParametreUser = "";
		ParametreAnaGrubu = "";
		ParametreAltGrubu = "";
		ParametreID = 0;
		ParametreAdi = "";
		ParametreDegeri = "";
		ParametreDefaultDegeri = "";
		ParametreDBDegeri = "";
	}

	public Parametre(string Program, string User, string AnaGrubu, string AltGrubu, int parametreID, string Adi, string Degeri)
	{
		EskiID = 0;
		ParametreProgram = Program;
		ParametreUser = User;
		ParametreAnaGrubu = AnaGrubu;
		ParametreAltGrubu = AltGrubu;
		ParametreID = parametreID;
		ParametreAdi = Adi;
		ParametreDegeri = Degeri;
		ParametreDefaultDegeri = Degeri;
		ParametreDBDegeri = "";
	}

	public static byte[] WriteToByteArray(Parametre toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Parametre toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Parametre toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
		if (versiyon < num)
		{
			num = versiyon;
		}
		if (versiyon >= 2)
		{
			int value = 2147483600;
			writer.Write(value);
		}
		writer.Write(toWrite.EskiID);
		writer.Write(toWrite.ParametreProgram);
		writer.Write(toWrite.ParametreUser);
		writer.Write(toWrite.ParametreAnaGrubu);
		writer.Write(toWrite.ParametreAltGrubu);
		writer.Write(toWrite.ParametreID);
		writer.Write(toWrite.ParametreAdi);
		writer.Write(toWrite.ParametreDefaultDegeri);
		writer.Write(toWrite.ParametreDBDegeri);
		writer.Write(toWrite.ParametreDegeri);
		if (num >= 2)
		{
			string value2 = toWrite.IDGuid.ToString();
			writer.Write(value2);
		}
	}

	public static Parametre ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Parametre result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Parametre ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Parametre ReadFromBinaryReader(BinaryReader reader)
	{
		int num = 1;
		Parametre parametre = new Parametre();
		int num2 = reader.ReadInt32();
		if (num2 >= 2147483600)
		{
			num = 2;
			num2 = reader.ReadInt32();
		}
		parametre.EskiID = num2;
		parametre.ParametreProgram = reader.ReadString();
		parametre.ParametreUser = reader.ReadString();
		parametre.ParametreAnaGrubu = reader.ReadString();
		parametre.ParametreAltGrubu = reader.ReadString();
		parametre.ParametreID = reader.ReadInt32();
		parametre.ParametreAdi = reader.ReadString();
		parametre.ParametreDefaultDegeri = reader.ReadString();
		parametre.ParametreDBDegeri = reader.ReadString();
		parametre.ParametreDegeri = reader.ReadString();
		if (num >= 2)
		{
			string text = reader.ReadString();
			if (text != "")
			{
				parametre.IDGuid = new Guid(text);
			}
			else
			{
				parametre.IDGuid = null;
			}
		}
		return parametre;
	}
}
