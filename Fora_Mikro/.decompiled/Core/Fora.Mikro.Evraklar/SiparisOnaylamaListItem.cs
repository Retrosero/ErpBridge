using System;
using System.IO;

namespace Fora.Mikro.Evraklar;

public class SiparisOnaylamaListItem
{
	public DateTime siparis_tarihi { get; set; }

	public string evrak_seri { get; set; }

	public int evrak_sira { get; set; }

	public string temsilci_kodu { get; set; }

	public string temsilci_adi { get; set; }

	public string temsilci_soyadi { get; set; }

	public string cari_kodu { get; set; }

	public string cari_unvan { get; set; }

	public double ara_toplam { get; set; }

	public double iskonto { get; set; }

	public double masraf { get; set; }

	public double vergi { get; set; }

	public double maliyet { get; set; }

	public double cari_bakiyesi { get; set; }

	public double toplam_risk_tutari { get; set; }

	public double toplam_teminat_tutari { get; set; }

	public double kalan_kredisi { get; set; }

	public int satir_sayisi { get; set; }

	public string teslim_turu { get; set; }

	public SiparisOnaylamaListItem()
	{
		teslim_turu = "";
	}

	public static byte[] WriteToByteArray(SiparisOnaylamaListItem toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(SiparisOnaylamaListItem toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(SiparisOnaylamaListItem toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.siparis_tarihi.Ticks);
		writer.Write(toWrite.evrak_seri);
		writer.Write(toWrite.evrak_sira);
		writer.Write(toWrite.temsilci_kodu);
		writer.Write(toWrite.temsilci_adi);
		writer.Write(toWrite.temsilci_soyadi);
		writer.Write(111);
		writer.Write(toWrite.cari_kodu);
		writer.Write(toWrite.cari_unvan);
		writer.Write(toWrite.ara_toplam);
		writer.Write(toWrite.iskonto);
		writer.Write(toWrite.masraf);
		writer.Write(toWrite.vergi);
		writer.Write(toWrite.maliyet);
		writer.Write(toWrite.cari_bakiyesi);
		writer.Write(toWrite.toplam_risk_tutari);
		writer.Write(toWrite.toplam_teminat_tutari);
		writer.Write(toWrite.kalan_kredisi);
		writer.Write(toWrite.satir_sayisi);
		if (num >= 2)
		{
			writer.Write(toWrite.teslim_turu);
		}
	}

	public static SiparisOnaylamaListItem ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		SiparisOnaylamaListItem result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static SiparisOnaylamaListItem ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static SiparisOnaylamaListItem ReadFromBinaryReader(BinaryReader reader)
	{
		SiparisOnaylamaListItem siparisOnaylamaListItem = new SiparisOnaylamaListItem();
		int num = reader.ReadInt32();
		siparisOnaylamaListItem.siparis_tarihi = new DateTime(reader.ReadInt64());
		siparisOnaylamaListItem.evrak_seri = reader.ReadString();
		siparisOnaylamaListItem.evrak_sira = reader.ReadInt32();
		siparisOnaylamaListItem.temsilci_kodu = reader.ReadString();
		siparisOnaylamaListItem.temsilci_adi = reader.ReadString();
		siparisOnaylamaListItem.temsilci_soyadi = reader.ReadString();
		reader.ReadInt32();
		siparisOnaylamaListItem.cari_kodu = reader.ReadString();
		siparisOnaylamaListItem.cari_unvan = reader.ReadString();
		siparisOnaylamaListItem.ara_toplam = reader.ReadDouble();
		siparisOnaylamaListItem.iskonto = reader.ReadDouble();
		siparisOnaylamaListItem.masraf = reader.ReadDouble();
		siparisOnaylamaListItem.vergi = reader.ReadDouble();
		siparisOnaylamaListItem.maliyet = reader.ReadDouble();
		siparisOnaylamaListItem.cari_bakiyesi = reader.ReadDouble();
		siparisOnaylamaListItem.toplam_risk_tutari = reader.ReadDouble();
		siparisOnaylamaListItem.toplam_teminat_tutari = reader.ReadDouble();
		siparisOnaylamaListItem.kalan_kredisi = reader.ReadDouble();
		siparisOnaylamaListItem.satir_sayisi = reader.ReadInt32();
		if (num >= 2)
		{
			siparisOnaylamaListItem.teslim_turu = reader.ReadString();
		}
		return siparisOnaylamaListItem;
	}
}
