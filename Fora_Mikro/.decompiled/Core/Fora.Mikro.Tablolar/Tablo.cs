using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.Tablolar;

public class Tablo
{
	public int TabloID { get; set; }

	public string TabloAdi { get; set; }

	public List<string> Indexler { get; set; }

	public List<Field> Fieldlar { get; set; }

	public int SonRECno { get; set; }

	public int YeniKayitSayisi { get; set; }

	public int DegisenKayitSayisi { get; set; }

	public int OfflineKayitSayisi { get; set; }

	public int OfflineLastUpdateTriggerRecNo { get; set; }

	public int OfflineLastDeleteTriggerRecNo { get; set; }

	public bool UpdateEdildi { get; set; }

	public Tablo()
	{
		TabloID = 0;
		SonRECno = 0;
		Indexler = new List<string>();
		Fieldlar = new List<Field>();
		YeniKayitSayisi = 0;
		DegisenKayitSayisi = 0;
		OfflineLastUpdateTriggerRecNo = 0;
		OfflineLastDeleteTriggerRecNo = 0;
		UpdateEdildi = false;
	}

	public Field GetPrimaryField()
	{
		foreach (Field item in Fieldlar)
		{
			if (item.PrimaryKey)
			{
				return item;
			}
		}
		return new Field();
	}

	public string GetPrimaryFieldName()
	{
		foreach (Field item in Fieldlar)
		{
			if (item.PrimaryKey)
			{
				return item.Adi;
			}
		}
		return "";
	}

	public string GetRECnoFieldName()
	{
		foreach (Field item in Fieldlar)
		{
			if (item.FieldRecNoMu)
			{
				return item.Adi;
			}
		}
		return "";
	}

	public string GetCekilecekFieldlar(string prefix)
	{
		bool flag = true;
		string text = "";
		foreach (Field item in Fieldlar)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				text += ",";
			}
			text = text + prefix + item.Adi;
		}
		if (prefix == "@")
		{
			text = text.ToLower().Replace("ı", "i");
		}
		return text;
	}

	public string GetFieldlarForUpdateQuery()
	{
		bool flag = true;
		string text = "";
		foreach (Field item in Fieldlar)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				text += ",";
			}
			text = text + item.Adi.ToLower() + "=@" + item.Adi.ToLower();
		}
		return text;
	}

	private List<Field> AddFields(string Fieldlar, enum_sqlite_data_tip FieldTipiSqLite, bool datetimemi, bool Cekilecek, bool Null, bool Primary)
	{
		List<Field> list = new List<Field>();
		string[] array = Fieldlar.Split(new char[1] { ',' });
		foreach (string adi in array)
		{
			Field field = new Field();
			field.Adi = adi;
			field.TipiSqLite = FieldTipiSqLite;
			field.DateTimeMi = datetimemi;
			field.NullOlurmu = Null;
			field.PrimaryKey = Primary;
			list.Add(field);
		}
		return list;
	}

	public static byte[] WriteToByteArray(Tablo toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Tablo toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Tablo toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.TabloID);
		writer.Write(toWrite.TabloAdi);
		writer.Write(toWrite.Fieldlar.Count);
		foreach (Field item in toWrite.Fieldlar)
		{
			Field.WriteToBinaryWriter(item, writer, 1);
		}
		writer.Write(toWrite.SonRECno);
		writer.Write(toWrite.YeniKayitSayisi);
		writer.Write(toWrite.DegisenKayitSayisi);
		writer.Write(toWrite.OfflineKayitSayisi);
		writer.Write(toWrite.OfflineLastUpdateTriggerRecNo);
		writer.Write(toWrite.OfflineLastDeleteTriggerRecNo);
		writer.Write(toWrite.UpdateEdildi);
		_ = 2;
	}

	public static Tablo ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Tablo result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Tablo ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Tablo ReadFromBinaryReader(BinaryReader reader)
	{
		Tablo tablo = new Tablo();
		reader.ReadInt32();
		tablo.TabloID = reader.ReadInt32();
		tablo.TabloAdi = reader.ReadString();
		int num = reader.ReadInt32();
		tablo.Fieldlar = new List<Field>();
		for (int i = 0; i < num; i++)
		{
			tablo.Fieldlar.Add(Field.ReadFromBinaryReader(reader));
		}
		tablo.SonRECno = reader.ReadInt32();
		tablo.YeniKayitSayisi = reader.ReadInt32();
		tablo.DegisenKayitSayisi = reader.ReadInt32();
		tablo.OfflineKayitSayisi = reader.ReadInt32();
		tablo.OfflineLastUpdateTriggerRecNo = reader.ReadInt32();
		tablo.OfflineLastDeleteTriggerRecNo = reader.ReadInt32();
		tablo.UpdateEdildi = reader.ReadBoolean();
		return tablo;
	}
}
