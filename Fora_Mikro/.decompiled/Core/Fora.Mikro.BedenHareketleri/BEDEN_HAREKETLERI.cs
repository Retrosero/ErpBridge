using System;
using System.IO;
using Fora.Mikro.Stoklar.StokBedenTanimlari;
using Fora.Mikro.Stoklar.StokRenkleri;

namespace Fora.Mikro.BedenHareketleri;

public class BEDEN_HAREKETLERI
{
	public int BdnHar_RECid_DBCno { get; set; }

	public int BdnHar_RECid_RECno { get; set; }

	public int BdnHar_DRECid_DBCno { get; set; }

	public int BdnHar_RECno { get; set; }

	public Guid BdnHar_Guid { get; set; }

	public int BdnHar_DRECid_RECno { get; set; }

	public Guid BdnHar_Har_uid { get; set; }

	public int BdnHar_Spec_Rec_no { get; set; }

	public bool BdnHar_iptal { get; set; }

	public int BdnHar_fileid { get; set; }

	public bool BdnHar_hidden { get; set; }

	public bool BdnHar_kilitli { get; set; }

	public bool BdnHar_degisti { get; set; }

	public int BdnHar_checksum { get; set; }

	public int BdnHar_create_user { get; set; }

	public DateTime BdnHar_create_date { get; set; }

	public int BdnHar_lastup_user { get; set; }

	public DateTime BdnHar_lastup_date { get; set; }

	public string BdnHar_special1 { get; set; }

	public string BdnHar_special2 { get; set; }

	public string BdnHar_special3 { get; set; }

	public int BdnHar_Tipi { get; set; }

	public int BdnHar_BedenNo { get; set; }

	public double BdnHar_HarGor { get; set; }

	public double BdnHar_KnsIsGor { get; set; }

	public double BdnHar_KnsFat { get; set; }

	public double BdnHar_TesMik { get; set; }

	public double BdnHar_rezervasyon_miktari { get; set; }

	public double BdnHar_rezerveden_teslim_edilen { get; set; }

	public BEDEN_HAREKETLERI()
	{
		BdnHar_special1 = "";
		BdnHar_special2 = "";
		BdnHar_special3 = "";
		BdnHar_Guid = Guid.Empty;
		BdnHar_Har_uid = Guid.Empty;
	}

	public int GetBedenNo()
	{
		int num = BdnHar_BedenNo / 40;
		int num2 = BdnHar_BedenNo - num * 40;
		if (num2 == 0)
		{
			num2 = 40;
		}
		return num2;
	}

	public string GetBedenAdi(STOK_BEDEN_TANIMLARI beden_tanimlari)
	{
		return GetBedenAdi(beden_tanimlari, GetBedenNo());
	}

	public static string GetBedenAdi(STOK_BEDEN_TANIMLARI beden_tanimlari, int beden_no)
	{
		return beden_no switch
		{
			1 => beden_tanimlari.bdn_kirilim_1, 
			2 => beden_tanimlari.bdn_kirilim_2, 
			3 => beden_tanimlari.bdn_kirilim_3, 
			4 => beden_tanimlari.bdn_kirilim_4, 
			5 => beden_tanimlari.bdn_kirilim_5, 
			6 => beden_tanimlari.bdn_kirilim_6, 
			7 => beden_tanimlari.bdn_kirilim_7, 
			8 => beden_tanimlari.bdn_kirilim_8, 
			9 => beden_tanimlari.bdn_kirilim_9, 
			10 => beden_tanimlari.bdn_kirilim_10, 
			11 => beden_tanimlari.bdn_kirilim_11, 
			12 => beden_tanimlari.bdn_kirilim_12, 
			13 => beden_tanimlari.bdn_kirilim_13, 
			14 => beden_tanimlari.bdn_kirilim_14, 
			15 => beden_tanimlari.bdn_kirilim_15, 
			16 => beden_tanimlari.bdn_kirilim_16, 
			17 => beden_tanimlari.bdn_kirilim_17, 
			18 => beden_tanimlari.bdn_kirilim_18, 
			19 => beden_tanimlari.bdn_kirilim_19, 
			20 => beden_tanimlari.bdn_kirilim_20, 
			21 => beden_tanimlari.bdn_kirilim_21, 
			22 => beden_tanimlari.bdn_kirilim_22, 
			23 => beden_tanimlari.bdn_kirilim_23, 
			24 => beden_tanimlari.bdn_kirilim_24, 
			25 => beden_tanimlari.bdn_kirilim_25, 
			26 => beden_tanimlari.bdn_kirilim_26, 
			27 => beden_tanimlari.bdn_kirilim_27, 
			28 => beden_tanimlari.bdn_kirilim_28, 
			29 => beden_tanimlari.bdn_kirilim_29, 
			30 => beden_tanimlari.bdn_kirilim_30, 
			31 => beden_tanimlari.bdn_kirilim_31, 
			32 => beden_tanimlari.bdn_kirilim_32, 
			33 => beden_tanimlari.bdn_kirilim_33, 
			34 => beden_tanimlari.bdn_kirilim_34, 
			35 => beden_tanimlari.bdn_kirilim_35, 
			36 => beden_tanimlari.bdn_kirilim_36, 
			37 => beden_tanimlari.bdn_kirilim_37, 
			38 => beden_tanimlari.bdn_kirilim_38, 
			39 => beden_tanimlari.bdn_kirilim_39, 
			40 => beden_tanimlari.bdn_kirilim_40, 
			_ => "", 
		};
	}

	public string GetRenkAdi(STOK_RENK_TANIMLARI renk_tanimlari)
	{
		return GetRenkAdi(renk_tanimlari, GetRenkNo());
	}

	public static string GetRenkAdi(STOK_RENK_TANIMLARI renk_tanimlari, int renk_no)
	{
		return renk_no switch
		{
			1 => renk_tanimlari.rnk_kirilim_1, 
			2 => renk_tanimlari.rnk_kirilim_2, 
			3 => renk_tanimlari.rnk_kirilim_3, 
			4 => renk_tanimlari.rnk_kirilim_4, 
			5 => renk_tanimlari.rnk_kirilim_5, 
			6 => renk_tanimlari.rnk_kirilim_6, 
			7 => renk_tanimlari.rnk_kirilim_7, 
			8 => renk_tanimlari.rnk_kirilim_8, 
			9 => renk_tanimlari.rnk_kirilim_9, 
			10 => renk_tanimlari.rnk_kirilim_10, 
			11 => renk_tanimlari.rnk_kirilim_11, 
			12 => renk_tanimlari.rnk_kirilim_12, 
			13 => renk_tanimlari.rnk_kirilim_13, 
			14 => renk_tanimlari.rnk_kirilim_14, 
			15 => renk_tanimlari.rnk_kirilim_15, 
			16 => renk_tanimlari.rnk_kirilim_16, 
			17 => renk_tanimlari.rnk_kirilim_17, 
			18 => renk_tanimlari.rnk_kirilim_18, 
			19 => renk_tanimlari.rnk_kirilim_19, 
			20 => renk_tanimlari.rnk_kirilim_20, 
			21 => renk_tanimlari.rnk_kirilim_21, 
			22 => renk_tanimlari.rnk_kirilim_22, 
			23 => renk_tanimlari.rnk_kirilim_23, 
			24 => renk_tanimlari.rnk_kirilim_24, 
			25 => renk_tanimlari.rnk_kirilim_25, 
			26 => renk_tanimlari.rnk_kirilim_26, 
			27 => renk_tanimlari.rnk_kirilim_27, 
			28 => renk_tanimlari.rnk_kirilim_28, 
			29 => renk_tanimlari.rnk_kirilim_29, 
			30 => renk_tanimlari.rnk_kirilim_30, 
			31 => renk_tanimlari.rnk_kirilim_31, 
			32 => renk_tanimlari.rnk_kirilim_32, 
			33 => renk_tanimlari.rnk_kirilim_33, 
			34 => renk_tanimlari.rnk_kirilim_34, 
			35 => renk_tanimlari.rnk_kirilim_35, 
			36 => renk_tanimlari.rnk_kirilim_36, 
			37 => renk_tanimlari.rnk_kirilim_37, 
			38 => renk_tanimlari.rnk_kirilim_38, 
			39 => renk_tanimlari.rnk_kirilim_39, 
			40 => renk_tanimlari.rnk_kirilim_40, 
			41 => renk_tanimlari.rnk_kirilim_41, 
			42 => renk_tanimlari.rnk_kirilim_42, 
			43 => renk_tanimlari.rnk_kirilim_43, 
			44 => renk_tanimlari.rnk_kirilim_44, 
			45 => renk_tanimlari.rnk_kirilim_45, 
			46 => renk_tanimlari.rnk_kirilim_46, 
			47 => renk_tanimlari.rnk_kirilim_47, 
			48 => renk_tanimlari.rnk_kirilim_48, 
			49 => renk_tanimlari.rnk_kirilim_49, 
			50 => renk_tanimlari.rnk_kirilim_50, 
			51 => renk_tanimlari.rnk_kirilim_51, 
			52 => renk_tanimlari.rnk_kirilim_52, 
			53 => renk_tanimlari.rnk_kirilim_53, 
			54 => renk_tanimlari.rnk_kirilim_54, 
			55 => renk_tanimlari.rnk_kirilim_55, 
			56 => renk_tanimlari.rnk_kirilim_56, 
			57 => renk_tanimlari.rnk_kirilim_57, 
			58 => renk_tanimlari.rnk_kirilim_58, 
			59 => renk_tanimlari.rnk_kirilim_59, 
			60 => renk_tanimlari.rnk_kirilim_60, 
			_ => "", 
		};
	}

	public int GetRenkNo()
	{
		int num = (int)Math.Ceiling((double)BdnHar_BedenNo / 40.0);
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}

	public static byte[] WriteToByteArray(BEDEN_HAREKETLERI toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(BEDEN_HAREKETLERI toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(BEDEN_HAREKETLERI toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.BdnHar_RECno);
		writer.Write(toWrite.BdnHar_RECid_DBCno);
		writer.Write(toWrite.BdnHar_RECid_RECno);
		writer.Write(toWrite.BdnHar_Spec_Rec_no);
		writer.Write(toWrite.BdnHar_iptal);
		writer.Write(toWrite.BdnHar_fileid);
		writer.Write(toWrite.BdnHar_hidden);
		writer.Write(toWrite.BdnHar_kilitli);
		writer.Write(toWrite.BdnHar_degisti);
		writer.Write(toWrite.BdnHar_checksum);
		writer.Write(toWrite.BdnHar_create_user);
		writer.Write(toWrite.BdnHar_create_date.Ticks);
		writer.Write(toWrite.BdnHar_lastup_user);
		writer.Write(toWrite.BdnHar_lastup_date.Ticks);
		writer.Write(toWrite.BdnHar_special1);
		writer.Write(toWrite.BdnHar_special2);
		writer.Write(toWrite.BdnHar_special3);
		writer.Write(toWrite.BdnHar_Tipi);
		writer.Write(toWrite.BdnHar_DRECid_DBCno);
		writer.Write(toWrite.BdnHar_DRECid_RECno);
		writer.Write(toWrite.BdnHar_BedenNo);
		writer.Write(toWrite.BdnHar_HarGor);
		writer.Write(toWrite.BdnHar_KnsIsGor);
		writer.Write(toWrite.BdnHar_KnsFat);
		writer.Write(toWrite.BdnHar_TesMik);
		writer.Write(toWrite.BdnHar_rezervasyon_miktari);
		writer.Write(toWrite.BdnHar_rezerveden_teslim_edilen);
	}

	public static BEDEN_HAREKETLERI ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		BEDEN_HAREKETLERI result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static BEDEN_HAREKETLERI ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static BEDEN_HAREKETLERI ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new BEDEN_HAREKETLERI
		{
			BdnHar_RECno = reader.ReadInt32(),
			BdnHar_RECid_DBCno = reader.ReadInt32(),
			BdnHar_RECid_RECno = reader.ReadInt32(),
			BdnHar_Spec_Rec_no = reader.ReadInt32(),
			BdnHar_iptal = reader.ReadBoolean(),
			BdnHar_fileid = reader.ReadInt32(),
			BdnHar_hidden = reader.ReadBoolean(),
			BdnHar_kilitli = reader.ReadBoolean(),
			BdnHar_degisti = reader.ReadBoolean(),
			BdnHar_checksum = reader.ReadInt32(),
			BdnHar_create_user = reader.ReadInt32(),
			BdnHar_create_date = new DateTime(reader.ReadInt64()),
			BdnHar_lastup_user = reader.ReadInt32(),
			BdnHar_lastup_date = new DateTime(reader.ReadInt64()),
			BdnHar_special1 = reader.ReadString(),
			BdnHar_special2 = reader.ReadString(),
			BdnHar_special3 = reader.ReadString(),
			BdnHar_Tipi = reader.ReadInt32(),
			BdnHar_DRECid_DBCno = reader.ReadInt32(),
			BdnHar_DRECid_RECno = reader.ReadInt32(),
			BdnHar_BedenNo = reader.ReadInt32(),
			BdnHar_HarGor = reader.ReadDouble(),
			BdnHar_KnsIsGor = reader.ReadDouble(),
			BdnHar_KnsFat = reader.ReadDouble(),
			BdnHar_TesMik = reader.ReadDouble(),
			BdnHar_rezervasyon_miktari = reader.ReadDouble(),
			BdnHar_rezerveden_teslim_edilen = reader.ReadDouble()
		};
	}
}
