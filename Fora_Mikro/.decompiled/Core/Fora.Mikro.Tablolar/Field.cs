using System.IO;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.Tablolar;

public class Field
{
	public string Adi { get; set; }

	public enum_sqlite_data_tip TipiSqLite { get; set; }

	public bool DateTimeMi { get; set; }

	public bool NullOlurmu { get; set; }

	public bool PrimaryKey { get; set; }

	public bool FieldRecNoMu { get; set; }

	public Field()
	{
		Adi = "";
		TipiSqLite = enum_sqlite_data_tip.TEXT;
		DateTimeMi = false;
		NullOlurmu = false;
		PrimaryKey = false;
		FieldRecNoMu = false;
	}

	public Field(string FieldAdi, enum_sqlite_data_tip FieldTipiSqLite, bool datetimemi, bool Cekilecek, bool Null, bool Primary, bool RecNoMu)
	{
		Adi = FieldAdi;
		TipiSqLite = FieldTipiSqLite;
		DateTimeMi = datetimemi;
		NullOlurmu = Null;
		PrimaryKey = Primary;
		FieldRecNoMu = RecNoMu;
	}

	public static byte[] WriteToByteArray(Field toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Field toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Field toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.Adi);
		writer.Write((int)toWrite.TipiSqLite);
		writer.Write(toWrite.DateTimeMi);
		writer.Write(toWrite.NullOlurmu);
		writer.Write(toWrite.PrimaryKey);
		writer.Write(toWrite.FieldRecNoMu);
		_ = 2;
	}

	public static Field ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Field result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Field ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Field ReadFromBinaryReader(BinaryReader reader)
	{
		Field field = new Field();
		reader.ReadInt32();
		field.Adi = reader.ReadString();
		field.TipiSqLite = (enum_sqlite_data_tip)reader.ReadInt32();
		field.DateTimeMi = reader.ReadBoolean();
		field.NullOlurmu = reader.ReadBoolean();
		field.PrimaryKey = reader.ReadBoolean();
		field.FieldRecNoMu = reader.ReadBoolean();
		return field;
	}
}
