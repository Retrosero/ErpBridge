using System;
using System.IO;
using Fora.Mikro.CariHesapHareket;

namespace Fora.Mikro.CariHesaplar.CariEkstresi;

public class CariEkstre
{
	public bool BaslikMi { get; set; }

	public bool SeciliMi { get; set; }

	public string BaslikMesaji { get; set; }

	public int cha_grupno { get; set; }

	public DateTime cha_tarihi { get; set; }

	public int cha_vade { get; set; }

	public DateTime VadeTarihi { get; set; }

	public enum_cha_tip cha_tip { get; set; }

	public enum_cha_evrak_tip cha_evrak_tip { get; set; }

	public string cha_evrakno_seri { get; set; }

	public int cha_evrakno_sira { get; set; }

	public double cha_meblag { get; set; }

	public double Bakiye { get; set; }

	public int DovizCinsi { get; set; }

	public double cha_d_kur { get; set; }

	public CariEkstre()
	{
		BaslikMi = false;
		BaslikMesaji = "";
		cha_evrakno_seri = "";
		DovizCinsi = 0;
		cha_d_kur = 1.0;
		SeciliMi = true;
	}

	public static byte[] WriteToByteArray(CariEkstre toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(CariEkstre toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(CariEkstre toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.BaslikMi);
		writer.Write(toWrite.SeciliMi);
		writer.Write(toWrite.BaslikMesaji);
		writer.Write(toWrite.cha_grupno);
		writer.Write(toWrite.cha_tarihi.Ticks);
		writer.Write(toWrite.cha_vade);
		writer.Write(toWrite.VadeTarihi.Ticks);
		writer.Write((int)toWrite.cha_tip);
		writer.Write((int)toWrite.cha_evrak_tip);
		writer.Write(toWrite.cha_evrakno_seri);
		writer.Write(toWrite.cha_evrakno_sira);
		writer.Write(toWrite.cha_meblag);
		writer.Write(toWrite.Bakiye);
		writer.Write(toWrite.DovizCinsi);
		writer.Write(toWrite.cha_d_kur);
		_ = 2;
	}

	public static CariEkstre ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		CariEkstre result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static CariEkstre ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static CariEkstre ReadFromBinaryReader(BinaryReader reader)
	{
		CariEkstre cariEkstre = new CariEkstre();
		reader.ReadInt32();
		cariEkstre.BaslikMi = reader.ReadBoolean();
		cariEkstre.SeciliMi = reader.ReadBoolean();
		cariEkstre.BaslikMesaji = reader.ReadString();
		cariEkstre.cha_grupno = reader.ReadInt32();
		cariEkstre.cha_tarihi = new DateTime(reader.ReadInt64());
		cariEkstre.cha_vade = reader.ReadInt32();
		cariEkstre.VadeTarihi = new DateTime(reader.ReadInt64());
		cariEkstre.cha_tip = (enum_cha_tip)reader.ReadInt32();
		cariEkstre.cha_evrak_tip = (enum_cha_evrak_tip)reader.ReadInt32();
		cariEkstre.cha_evrakno_seri = reader.ReadString();
		cariEkstre.cha_evrakno_sira = reader.ReadInt32();
		cariEkstre.cha_meblag = reader.ReadDouble();
		cariEkstre.Bakiye = reader.ReadDouble();
		cariEkstre.DovizCinsi = reader.ReadInt32();
		cariEkstre.cha_d_kur = reader.ReadDouble();
		return cariEkstre;
	}
}
