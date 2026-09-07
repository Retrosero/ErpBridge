using System;
using System.IO;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.DisTicaret;

public class CEKI_LISTESI
{
	public int Ckl_RECno { get; set; }

	public int Ckl_RECid_DBCno { get; set; }

	public int Ckl_RECid_RECno { get; set; }

	public int Ckl_SpecRECNo { get; set; }

	public bool Ckl_iptal { get; set; }

	public int Ckl_fileid { get; set; }

	public bool Ckl_hidden { get; set; }

	public bool Ckl_kilitli { get; set; }

	public bool Ckl_degisti { get; set; }

	public int Ckl_CheckSum { get; set; }

	public int Ckl_create_user { get; set; }

	public DateTime Ckl_create_date { get; set; }

	public int Ckl_lastup_user { get; set; }

	public DateTime Ckl_lastup_date { get; set; }

	public string Ckl_special1 { get; set; }

	public string Ckl_special2 { get; set; }

	public string Ckl_special3 { get; set; }

	public enum_Ckl_EvrakTip Ckl_EvrakTip { get; set; }

	public string Ckl_EvrakSeri { get; set; }

	public int Ckl_EvrakSira { get; set; }

	public string Ckl_StokKodu { get; set; }

	public int Ckl_BedenPntr { get; set; }

	public double Ckl_Miktari { get; set; }

	public int Ckl_AnaAmbalajNo { get; set; }

	public int Ckl_AltAmbalajNo { get; set; }

	public CEKI_LISTESI()
	{
		Ckl_fileid = 125;
		Ckl_EvrakTip = enum_Ckl_EvrakTip.CikisIrsaliyesi;
		Ckl_create_date = DateTime.Now;
		Ckl_lastup_date = DateTime.Now;
		Ckl_BedenPntr = 0;
		Ckl_Miktari = 0.0;
		Ckl_AnaAmbalajNo = 1;
		Ckl_AltAmbalajNo = 1;
		Ckl_special1 = "";
		Ckl_special2 = "";
		Ckl_special3 = "";
		Ckl_EvrakSeri = "";
		Ckl_StokKodu = "";
	}

	public static byte[] WriteToByteArray(CEKI_LISTESI toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(CEKI_LISTESI toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(CEKI_LISTESI toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.Ckl_RECno);
		writer.Write(toWrite.Ckl_RECid_DBCno);
		writer.Write(toWrite.Ckl_RECid_RECno);
		writer.Write(toWrite.Ckl_SpecRECNo);
		writer.Write(toWrite.Ckl_iptal);
		writer.Write(toWrite.Ckl_fileid);
		writer.Write(toWrite.Ckl_hidden);
		writer.Write(toWrite.Ckl_kilitli);
		writer.Write(toWrite.Ckl_degisti);
		writer.Write(toWrite.Ckl_CheckSum);
		writer.Write(toWrite.Ckl_create_user);
		writer.Write(toWrite.Ckl_create_date.Ticks);
		writer.Write(toWrite.Ckl_lastup_user);
		writer.Write(toWrite.Ckl_lastup_date.Ticks);
		writer.Write(toWrite.Ckl_special1);
		writer.Write(toWrite.Ckl_special2);
		writer.Write(toWrite.Ckl_special3);
		writer.Write((int)toWrite.Ckl_EvrakTip);
		writer.Write(toWrite.Ckl_EvrakSeri);
		writer.Write(toWrite.Ckl_EvrakSira);
		writer.Write(toWrite.Ckl_StokKodu);
		writer.Write(toWrite.Ckl_BedenPntr);
		writer.Write(toWrite.Ckl_Miktari);
		writer.Write(toWrite.Ckl_AnaAmbalajNo);
		writer.Write(toWrite.Ckl_AltAmbalajNo);
		_ = 2;
	}

	public static CEKI_LISTESI ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		CEKI_LISTESI result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static CEKI_LISTESI ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static CEKI_LISTESI ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new CEKI_LISTESI
		{
			Ckl_RECno = reader.ReadInt32(),
			Ckl_RECid_DBCno = reader.ReadInt32(),
			Ckl_RECid_RECno = reader.ReadInt32(),
			Ckl_SpecRECNo = reader.ReadInt32(),
			Ckl_iptal = reader.ReadBoolean(),
			Ckl_fileid = reader.ReadInt32(),
			Ckl_hidden = reader.ReadBoolean(),
			Ckl_kilitli = reader.ReadBoolean(),
			Ckl_degisti = reader.ReadBoolean(),
			Ckl_CheckSum = reader.ReadInt32(),
			Ckl_create_user = reader.ReadInt32(),
			Ckl_create_date = new DateTime(reader.ReadInt64()),
			Ckl_lastup_user = reader.ReadInt32(),
			Ckl_lastup_date = new DateTime(reader.ReadInt64()),
			Ckl_special1 = reader.ReadString(),
			Ckl_special2 = reader.ReadString(),
			Ckl_special3 = reader.ReadString(),
			Ckl_EvrakTip = (enum_Ckl_EvrakTip)reader.ReadInt32(),
			Ckl_EvrakSeri = reader.ReadString(),
			Ckl_EvrakSira = reader.ReadInt32(),
			Ckl_StokKodu = reader.ReadString(),
			Ckl_BedenPntr = reader.ReadInt32(),
			Ckl_Miktari = reader.ReadDouble(),
			Ckl_AnaAmbalajNo = reader.ReadInt32(),
			Ckl_AltAmbalajNo = reader.ReadInt32()
		};
	}
}
