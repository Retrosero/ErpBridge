using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class RaporYapilacakTahsilatlarSonucHam
{
	public List<RaporYapilacakTahsilatlarSatir> sonuc_ham { get; set; }

	public List<RaporYardimciCariObje> cariler { get; set; }

	public List<RaporYardimciGenelObje> cari_bolgeleri { get; set; }

	public List<RaporYardimciGenelObje> cari_gruplari { get; set; }

	public List<RaporYardimciGenelObje> temsilciler { get; set; }

	public List<RaporYardimciGenelObje> sorumluluk_merkezleri { get; set; }

	public List<RaporYardimciGenelObje> projeler { get; set; }

	public List<RaporYardimciGenelObje> firmalar { get; set; }

	public List<RaporYardimciGenelObje> subeler { get; set; }

	public List<RaporYardimciGenelObje> doviz_cinsleri { get; set; }

	public RaporYapilacakTahsilatlarSonucHam()
	{
		sonuc_ham = new List<RaporYapilacakTahsilatlarSatir>();
		cariler = new List<RaporYardimciCariObje>();
		cari_bolgeleri = new List<RaporYardimciGenelObje>();
		cari_gruplari = new List<RaporYardimciGenelObje>();
		temsilciler = new List<RaporYardimciGenelObje>();
		sorumluluk_merkezleri = new List<RaporYardimciGenelObje>();
		projeler = new List<RaporYardimciGenelObje>();
		firmalar = new List<RaporYardimciGenelObje>();
		subeler = new List<RaporYardimciGenelObje>();
		doviz_cinsleri = new List<RaporYardimciGenelObje>();
	}

	public static byte[] WriteToByteArray(RaporYapilacakTahsilatlarSonucHam toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporYapilacakTahsilatlarSonucHam toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporYapilacakTahsilatlarSonucHam toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sonuc_ham.Count);
		foreach (RaporYapilacakTahsilatlarSatir item in toWrite.sonuc_ham)
		{
			RaporYapilacakTahsilatlarSatir.WriteToBinaryWriter(item, writer, 1);
		}
		writer.Write(toWrite.cariler.Count);
		foreach (RaporYardimciCariObje item2 in toWrite.cariler)
		{
			writer.Write(item2.Position);
			writer.Write(item2.RecNo);
			writer.Write(item2.Kodu);
			writer.Write(item2.Adi);
			writer.Write(item2.cari_bolge_sira_no);
			writer.Write(item2.cari_grup_sira_no);
		}
		writer.Write(toWrite.cari_bolgeleri.Count);
		foreach (RaporYardimciGenelObje item3 in toWrite.cari_bolgeleri)
		{
			writer.Write(item3.Position);
			writer.Write(item3.RecNo);
			writer.Write(item3.Kodu);
			writer.Write(item3.Adi);
		}
		writer.Write(toWrite.cari_gruplari.Count);
		foreach (RaporYardimciGenelObje item4 in toWrite.cari_gruplari)
		{
			writer.Write(item4.Position);
			writer.Write(item4.RecNo);
			writer.Write(item4.Kodu);
			writer.Write(item4.Adi);
		}
		writer.Write(toWrite.temsilciler.Count);
		foreach (RaporYardimciGenelObje item5 in toWrite.temsilciler)
		{
			writer.Write(item5.Position);
			writer.Write(item5.RecNo);
			writer.Write(item5.Kodu);
			writer.Write(item5.Adi);
		}
		writer.Write(toWrite.sorumluluk_merkezleri.Count);
		foreach (RaporYardimciGenelObje item6 in toWrite.sorumluluk_merkezleri)
		{
			writer.Write(item6.Position);
			writer.Write(item6.RecNo);
			writer.Write(item6.Kodu);
			writer.Write(item6.Adi);
		}
		writer.Write(toWrite.projeler.Count);
		foreach (RaporYardimciGenelObje item7 in toWrite.projeler)
		{
			writer.Write(item7.Position);
			writer.Write(item7.RecNo);
			writer.Write(item7.Kodu);
			writer.Write(item7.Adi);
		}
		writer.Write(toWrite.firmalar.Count);
		foreach (RaporYardimciGenelObje item8 in toWrite.firmalar)
		{
			writer.Write(item8.Position);
			writer.Write(item8.RecNo);
			writer.Write(item8.Kodu);
			writer.Write(item8.Adi);
		}
		writer.Write(toWrite.subeler.Count);
		foreach (RaporYardimciGenelObje item9 in toWrite.subeler)
		{
			writer.Write(item9.Position);
			writer.Write(item9.RecNo);
			writer.Write(item9.Kodu);
			writer.Write(item9.Adi);
		}
		writer.Write(toWrite.doviz_cinsleri.Count);
		foreach (RaporYardimciGenelObje item10 in toWrite.doviz_cinsleri)
		{
			writer.Write(item10.Position);
			writer.Write(item10.RecNo);
			writer.Write(item10.Kodu);
			writer.Write(item10.Adi);
		}
		_ = 2;
	}

	public static RaporYapilacakTahsilatlarSonucHam ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporYapilacakTahsilatlarSonucHam result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporYapilacakTahsilatlarSonucHam ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporYapilacakTahsilatlarSonucHam ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		RaporYapilacakTahsilatlarSonucHam raporYapilacakTahsilatlarSonucHam = new RaporYapilacakTahsilatlarSonucHam();
		int num = reader.ReadInt32();
		raporYapilacakTahsilatlarSonucHam.sonuc_ham = new List<RaporYapilacakTahsilatlarSatir>();
		for (int i = 0; i < num; i++)
		{
			raporYapilacakTahsilatlarSonucHam.sonuc_ham.Add(RaporYapilacakTahsilatlarSatir.ReadFromBinaryReader(reader));
		}
		num = reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			raporYapilacakTahsilatlarSonucHam.cariler.Add(new RaporYardimciCariObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32()));
		}
		num = reader.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			raporYapilacakTahsilatlarSonucHam.cari_bolgeleri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			raporYapilacakTahsilatlarSonucHam.cari_gruplari.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int m = 0; m < num; m++)
		{
			raporYapilacakTahsilatlarSonucHam.temsilciler.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int n = 0; n < num; n++)
		{
			raporYapilacakTahsilatlarSonucHam.sorumluluk_merkezleri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num2 = 0; num2 < num; num2++)
		{
			raporYapilacakTahsilatlarSonucHam.projeler.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num3 = 0; num3 < num; num3++)
		{
			raporYapilacakTahsilatlarSonucHam.firmalar.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num4 = 0; num4 < num; num4++)
		{
			raporYapilacakTahsilatlarSonucHam.subeler.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		num = reader.ReadInt32();
		for (int num5 = 0; num5 < num; num5++)
		{
			raporYapilacakTahsilatlarSonucHam.doviz_cinsleri.Add(new RaporYardimciGenelObje(reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), reader.ReadString()));
		}
		return raporYapilacakTahsilatlarSonucHam;
	}
}
