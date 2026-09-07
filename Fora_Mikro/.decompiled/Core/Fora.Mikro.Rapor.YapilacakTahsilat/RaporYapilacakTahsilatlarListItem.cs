using System;
using System.IO;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class RaporYapilacakTahsilatlarListItem
{
	public int liste_sira_no { get; set; }

	public string kodu { get; set; }

	public string adi { get; set; }

	public MeblagveTarih meblag_ve_tarihler { get; set; }

	public double ToplamMeblag
	{
		get
		{
			double num = 0.0;
			foreach (double item in meblag_ve_tarihler.meblaglar)
			{
				num += item;
			}
			return num;
		}
	}

	public string DovizKodu => meblag_ve_tarihler.DovizKodu;

	public DateTime OrtalamaVade
	{
		get
		{
			DateTime result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			double num = 0.0;
			double num2 = 0.0;
			int num3 = 0;
			foreach (double item in meblag_ve_tarihler.meblaglar)
			{
				int num4 = (int)(meblag_ve_tarihler.vade_tarihleri[num3] - dateTime).TotalDays;
				num += item * (double)num4;
				num2 += item;
				num3++;
			}
			if (num2 > 0.0)
			{
				int num5 = (int)Math.Round(num / num2, 0);
				return dateTime.AddDays(num5);
			}
			return result;
		}
	}

	public int OrtalamaVadeGunSayisi
	{
		get
		{
			int result = 0;
			DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			double num = 0.0;
			double num2 = 0.0;
			int num3 = 0;
			foreach (double item in meblag_ve_tarihler.meblaglar)
			{
				int num4 = (int)(meblag_ve_tarihler.vade_tarihleri[num3] - dateTime).TotalDays;
				num += item * (double)num4;
				num2 += item;
				num3++;
			}
			if (num2 > 0.0)
			{
				result = (int)Math.Round(num / num2, 0);
			}
			return result;
		}
	}

	public RaporYapilacakTahsilatlarListItem()
	{
		kodu = "";
		adi = "";
		meblag_ve_tarihler = new MeblagveTarih();
	}

	public static byte[] WriteToByteArray(RaporYapilacakTahsilatlarListItem toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporYapilacakTahsilatlarListItem toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporYapilacakTahsilatlarListItem toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.liste_sira_no);
		writer.Write(toWrite.kodu);
		writer.Write(toWrite.adi);
		MeblagveTarih.WriteToBinaryWriter(toWrite.meblag_ve_tarihler, writer, 999);
		_ = 2;
	}

	public static RaporYapilacakTahsilatlarListItem ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporYapilacakTahsilatlarListItem result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporYapilacakTahsilatlarListItem ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static RaporYapilacakTahsilatlarListItem ReadFromStream(BinaryReader reader)
	{
		reader.ReadInt32();
		return new RaporYapilacakTahsilatlarListItem
		{
			liste_sira_no = reader.ReadInt32(),
			kodu = reader.ReadString(),
			adi = reader.ReadString(),
			meblag_ve_tarihler = MeblagveTarih.ReadFromStream(reader)
		};
	}
}
