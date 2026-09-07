using System;
using System.IO;

namespace Fora.Mikro.SayimSonuclari;

public class SAYIM_SONUCLARI
{
	public int sym_RECid_DBCno { get; set; }

	public int sym_RECid_RECno { get; set; }

	public int sym_RECno { get; set; }

	public Guid sym_Guid { get; set; }

	public int sym_SpecRECno { get; set; }

	public bool sym_iptal { get; set; }

	public int sym_fileid { get; set; }

	public bool sym_hidden { get; set; }

	public bool sym_kilitli { get; set; }

	public bool sym_degisti { get; set; }

	public int sym_checksum { get; set; }

	public int sym_create_user { get; set; }

	public DateTime sym_create_date { get; set; }

	public int sym_lastup_user { get; set; }

	public DateTime sym_lastup_date { get; set; }

	public string sym_special1 { get; set; }

	public string sym_special2 { get; set; }

	public string sym_special3 { get; set; }

	public DateTime sym_tarihi { get; set; }

	public int sym_depono { get; set; }

	public int sym_evrakno { get; set; }

	public int sym_satirno { get; set; }

	public string sym_Stokkodu { get; set; }

	public string sym_reyonkodu { get; set; }

	public string sym_koridorkodu { get; set; }

	public string sym_rafkodu { get; set; }

	public double sym_miktar1 { get; set; }

	public double sym_miktar2 { get; set; }

	public double sym_miktar3 { get; set; }

	public double sym_miktar4 { get; set; }

	public double sym_miktar5 { get; set; }

	public int sym_birim_pntr { get; set; }

	public string sym_barkod { get; set; }

	public int sym_renkno { get; set; }

	public int sym_bedenno { get; set; }

	public string sym_parti_kodu { get; set; }

	public int sym_lot_no { get; set; }

	public string sym_serino { get; set; }

	public SAYIM_SONUCLARI()
	{
		sym_fileid = 28;
		sym_birim_pntr = 1;
		sym_special1 = "";
		sym_special2 = "";
		sym_special3 = "";
		sym_Stokkodu = "";
		sym_barkod = "";
		sym_parti_kodu = "";
		sym_serino = "";
		sym_reyonkodu = "0";
		sym_koridorkodu = "0";
		sym_rafkodu = "0";
		sym_create_date = DateTime.Now;
		sym_lastup_date = DateTime.Now;
		sym_tarihi = DateTime.Now;
		sym_Guid = Guid.Empty;
	}

	public static byte[] WriteToByteArray(SAYIM_SONUCLARI toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(SAYIM_SONUCLARI toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(SAYIM_SONUCLARI toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sym_RECno);
		writer.Write(toWrite.sym_RECid_DBCno);
		writer.Write(toWrite.sym_RECid_RECno);
		writer.Write(toWrite.sym_SpecRECno);
		writer.Write(toWrite.sym_iptal);
		writer.Write(toWrite.sym_fileid);
		writer.Write(toWrite.sym_hidden);
		writer.Write(toWrite.sym_kilitli);
		writer.Write(toWrite.sym_degisti);
		writer.Write(toWrite.sym_checksum);
		writer.Write(toWrite.sym_create_user);
		writer.Write(toWrite.sym_create_date.Ticks);
		writer.Write(toWrite.sym_lastup_user);
		writer.Write(toWrite.sym_lastup_date.Ticks);
		writer.Write(toWrite.sym_special1);
		writer.Write(toWrite.sym_special2);
		writer.Write(toWrite.sym_special3);
		writer.Write(toWrite.sym_tarihi.Ticks);
		writer.Write(toWrite.sym_depono);
		writer.Write(toWrite.sym_evrakno);
		writer.Write(toWrite.sym_satirno);
		writer.Write(toWrite.sym_Stokkodu);
		writer.Write(toWrite.sym_reyonkodu);
		writer.Write(toWrite.sym_koridorkodu);
		writer.Write(toWrite.sym_rafkodu);
		writer.Write(toWrite.sym_miktar1);
		writer.Write(toWrite.sym_miktar2);
		writer.Write(toWrite.sym_miktar3);
		writer.Write(toWrite.sym_miktar4);
		writer.Write(toWrite.sym_miktar5);
		writer.Write(toWrite.sym_birim_pntr);
		writer.Write(toWrite.sym_barkod);
		writer.Write(toWrite.sym_renkno);
		writer.Write(toWrite.sym_bedenno);
		writer.Write(toWrite.sym_parti_kodu);
		writer.Write(toWrite.sym_lot_no);
		writer.Write(toWrite.sym_serino);
		_ = 2;
	}

	public static SAYIM_SONUCLARI ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		SAYIM_SONUCLARI result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static SAYIM_SONUCLARI ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static SAYIM_SONUCLARI ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new SAYIM_SONUCLARI
		{
			sym_RECno = reader.ReadInt32(),
			sym_RECid_DBCno = reader.ReadInt32(),
			sym_RECid_RECno = reader.ReadInt32(),
			sym_SpecRECno = reader.ReadInt32(),
			sym_iptal = reader.ReadBoolean(),
			sym_fileid = reader.ReadInt32(),
			sym_hidden = reader.ReadBoolean(),
			sym_kilitli = reader.ReadBoolean(),
			sym_degisti = reader.ReadBoolean(),
			sym_checksum = reader.ReadInt32(),
			sym_create_user = reader.ReadInt32(),
			sym_create_date = new DateTime(reader.ReadInt64()),
			sym_lastup_user = reader.ReadInt32(),
			sym_lastup_date = new DateTime(reader.ReadInt64()),
			sym_special1 = reader.ReadString(),
			sym_special2 = reader.ReadString(),
			sym_special3 = reader.ReadString(),
			sym_tarihi = new DateTime(reader.ReadInt64()),
			sym_depono = reader.ReadInt32(),
			sym_evrakno = reader.ReadInt32(),
			sym_satirno = reader.ReadInt32(),
			sym_Stokkodu = reader.ReadString(),
			sym_reyonkodu = reader.ReadString(),
			sym_koridorkodu = reader.ReadString(),
			sym_rafkodu = reader.ReadString(),
			sym_miktar1 = reader.ReadDouble(),
			sym_miktar2 = reader.ReadDouble(),
			sym_miktar3 = reader.ReadDouble(),
			sym_miktar4 = reader.ReadDouble(),
			sym_miktar5 = reader.ReadDouble(),
			sym_birim_pntr = reader.ReadInt32(),
			sym_barkod = reader.ReadString(),
			sym_renkno = reader.ReadInt32(),
			sym_bedenno = reader.ReadInt32(),
			sym_parti_kodu = reader.ReadString(),
			sym_lot_no = reader.ReadInt32(),
			sym_serino = reader.ReadString()
		};
	}
}
