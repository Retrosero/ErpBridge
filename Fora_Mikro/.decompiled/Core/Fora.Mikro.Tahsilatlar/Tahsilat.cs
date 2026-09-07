using System;
using System.IO;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.SorumlulukMerkezleri;

namespace Fora.Mikro.Tahsilatlar;

public class Tahsilat
{
	public enum_tahsilat_cinsi cinsi { get; set; }

	public DateTime vadesi { get; set; }

	public string kasa_banka_kodu { get; set; }

	public double tutar { get; set; }

	public string aciklama { get; set; }

	public string referans { get; set; }

	public int evrak_dovizcinsi { get; set; }

	public int kasabanka_dovizcinsi { get; set; }

	public SorumlulukMerkezi sorumlulukmerkezi { get; set; }

	public Kur kasabanka_dovizcinsi_kur { get; set; }

	public string sck_borclu { get; set; }

	public string sck_bankano { get; set; }

	public string sck_vdaire_no { get; set; }

	public string sck_banka_adres1 { get; set; }

	public string sck_sube_adres2 { get; set; }

	public string sck_hesapno_sehir { get; set; }

	public string sck_no { get; set; }

	public string Sck_TCMB_Banka_kodu { get; set; }

	public string Sck_TCMB_Sube_kodu { get; set; }

	public string Sck_TCMB_il_kodu { get; set; }

	public Tahsilat()
	{
		cinsi = enum_tahsilat_cinsi.Nakit;
		vadesi = DateTime.Now;
		kasa_banka_kodu = "";
		tutar = 0.0;
		aciklama = "";
		referans = "";
		evrak_dovizcinsi = 0;
		kasabanka_dovizcinsi = 0;
		sorumlulukmerkezi = new SorumlulukMerkezi();
		kasabanka_dovizcinsi_kur = new Kur();
		sck_borclu = "";
		sck_bankano = "";
		sck_vdaire_no = "";
		sck_banka_adres1 = "";
		sck_sube_adres2 = "";
		sck_hesapno_sehir = "";
		sck_no = "";
		Sck_TCMB_Banka_kodu = "";
		Sck_TCMB_Sube_kodu = "";
		Sck_TCMB_il_kodu = "";
	}

	public static byte[] WriteToByteArray(Tahsilat toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Tahsilat toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Tahsilat toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.cinsi);
		writer.Write(toWrite.vadesi.Ticks);
		writer.Write(toWrite.kasa_banka_kodu);
		writer.Write(toWrite.tutar);
		writer.Write(toWrite.aciklama);
		writer.Write(toWrite.referans);
		writer.Write(toWrite.evrak_dovizcinsi);
		writer.Write(toWrite.kasabanka_dovizcinsi);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.sorumlulukmerkezi, writer);
		writer.Write(toWrite.kasabanka_dovizcinsi_kur.dov_no);
		writer.Write(toWrite.kasabanka_dovizcinsi_kur.dov_fiyat);
		writer.Write(toWrite.sck_borclu);
		writer.Write(toWrite.sck_bankano);
		writer.Write(toWrite.sck_vdaire_no);
		writer.Write(toWrite.sck_banka_adres1);
		writer.Write(toWrite.sck_sube_adres2);
		writer.Write(toWrite.sck_hesapno_sehir);
		writer.Write(toWrite.sck_no);
		writer.Write(toWrite.Sck_TCMB_Banka_kodu);
		writer.Write(toWrite.Sck_TCMB_Sube_kodu);
		writer.Write(toWrite.Sck_TCMB_il_kodu);
		_ = 2;
	}

	public static Tahsilat ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Tahsilat result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Tahsilat ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Tahsilat ReadFromBinaryReader(BinaryReader reader)
	{
		Tahsilat tahsilat = new Tahsilat();
		reader.ReadInt32();
		tahsilat.cinsi = (enum_tahsilat_cinsi)reader.ReadInt32();
		tahsilat.vadesi = new DateTime(reader.ReadInt64());
		tahsilat.kasa_banka_kodu = reader.ReadString();
		tahsilat.tutar = reader.ReadDouble();
		tahsilat.aciklama = reader.ReadString();
		tahsilat.referans = reader.ReadString();
		tahsilat.evrak_dovizcinsi = reader.ReadInt32();
		tahsilat.kasabanka_dovizcinsi = reader.ReadInt32();
		tahsilat.sorumlulukmerkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader);
		tahsilat.kasabanka_dovizcinsi_kur = new Kur();
		tahsilat.kasabanka_dovizcinsi_kur.dov_no = reader.ReadInt32();
		tahsilat.kasabanka_dovizcinsi_kur.dov_fiyat = reader.ReadDouble();
		tahsilat.sck_borclu = reader.ReadString();
		tahsilat.sck_bankano = reader.ReadString();
		tahsilat.sck_vdaire_no = reader.ReadString();
		tahsilat.sck_banka_adres1 = reader.ReadString();
		tahsilat.sck_sube_adres2 = reader.ReadString();
		tahsilat.sck_hesapno_sehir = reader.ReadString();
		tahsilat.sck_no = reader.ReadString();
		tahsilat.Sck_TCMB_Banka_kodu = reader.ReadString();
		tahsilat.Sck_TCMB_Sube_kodu = reader.ReadString();
		tahsilat.Sck_TCMB_il_kodu = reader.ReadString();
		return tahsilat;
	}
}
