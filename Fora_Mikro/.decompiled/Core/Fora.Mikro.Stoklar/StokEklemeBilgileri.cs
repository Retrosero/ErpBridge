using System.Collections.Generic;
using System.IO;
using Fora.Mikro.BedenHareketleri;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Stoklar.StokSerino;

namespace Fora.Mikro.Stoklar;

public class StokEklemeBilgileri
{
	public double Miktar { get; set; }

	public double Miktar2 { get; set; }

	public int sth_birim_pntr { get; set; }

	public FiyatTanimlamasi BirimFiyat { get; set; }

	public string Aciklama1 { get; set; }

	public string Aciklama2 { get; set; }

	public string parti_kodu { get; set; }

	public int lot_no { get; set; }

	public bool fiyat_farki_mi { get; set; }

	public double StokDovizCinsiKuru { get; set; }

	public bool SatirBirlestir { get; set; }

	public bool SepetteMevcutMuKontroluYap { get; set; }

	public double MiktarMF { get; set; }

	public string proje_kodu { get; set; }

	public string sorumluluk_merkezi_kodu { get; set; }

	public int vergi_pntr { get; set; }

	public List<BEDEN_HAREKETLERI> renk_beden_hareketleri { get; set; }

	public List<STOK_SERINO_TANIMLARI> serino_tanimlari { get; set; }

	public bool SatisFaturasiIade { get; set; }

	public double Miktar_Formul_Olcu1 { get; set; }

	public double Miktar_Formul_Olcu2 { get; set; }

	public double Miktar_Formul_Olcu3 { get; set; }

	public double Miktar_Formul_Olcu4 { get; set; }

	public double Miktar_Formul_Olcu5 { get; set; }

	public int Miktar_Formul_FormulMiktarNo { get; set; }

	public double Miktar_Formul_FormulMiktar { get; set; }

	public StokEklemeBilgileri()
	{
		Miktar = 1.0;
		Miktar2 = 0.0;
		MiktarMF = 0.0;
		sth_birim_pntr = 1;
		BirimFiyat = new FiyatTanimlamasi();
		parti_kodu = "";
		fiyat_farki_mi = false;
		StokDovizCinsiKuru = 1.0;
		SatirBirlestir = true;
		SepetteMevcutMuKontroluYap = false;
		Aciklama1 = "";
		Aciklama2 = "";
		proje_kodu = "";
		sorumluluk_merkezi_kodu = "";
		vergi_pntr = 4;
		renk_beden_hareketleri = new List<BEDEN_HAREKETLERI>();
		serino_tanimlari = new List<STOK_SERINO_TANIMLARI>();
		SatisFaturasiIade = false;
	}

	public static byte[] WriteToByteArray(StokEklemeBilgileri toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(StokEklemeBilgileri toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(StokEklemeBilgileri toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 3;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.Miktar);
		writer.Write(toWrite.Miktar2);
		writer.Write(toWrite.sth_birim_pntr);
		FiyatTanimlamasi.WriteToBinaryWriter(toWrite.BirimFiyat, writer, 1);
		writer.Write(toWrite.Aciklama1);
		writer.Write(toWrite.Aciklama2);
		writer.Write(toWrite.parti_kodu);
		writer.Write(toWrite.fiyat_farki_mi);
		writer.Write(toWrite.StokDovizCinsiKuru);
		writer.Write(toWrite.SatirBirlestir);
		writer.Write(toWrite.SepetteMevcutMuKontroluYap);
		writer.Write(toWrite.MiktarMF);
		writer.Write(toWrite.lot_no);
		writer.Write(toWrite.proje_kodu);
		writer.Write(toWrite.sorumluluk_merkezi_kodu);
		writer.Write(toWrite.vergi_pntr);
		writer.Write(toWrite.renk_beden_hareketleri.Count);
		foreach (BEDEN_HAREKETLERI item in toWrite.renk_beden_hareketleri)
		{
			BEDEN_HAREKETLERI.WriteToBinaryWriter(item, writer, 999);
		}
		if (versiyon >= 2)
		{
			writer.Write(toWrite.serino_tanimlari.Count);
			foreach (STOK_SERINO_TANIMLARI item2 in toWrite.serino_tanimlari)
			{
				STOK_SERINO_TANIMLARI.WriteToBinaryWriter(item2, writer, 999);
			}
		}
		if (versiyon >= 3)
		{
			writer.Write(toWrite.Miktar_Formul_Olcu1);
			writer.Write(toWrite.Miktar_Formul_Olcu2);
			writer.Write(toWrite.Miktar_Formul_Olcu3);
			writer.Write(toWrite.Miktar_Formul_Olcu4);
			writer.Write(toWrite.Miktar_Formul_Olcu5);
			writer.Write(toWrite.Miktar_Formul_FormulMiktarNo);
			writer.Write(toWrite.Miktar_Formul_FormulMiktar);
		}
	}

	public static StokEklemeBilgileri ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		StokEklemeBilgileri result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static StokEklemeBilgileri ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static StokEklemeBilgileri ReadFromBinaryReader(BinaryReader reader)
	{
		StokEklemeBilgileri stokEklemeBilgileri = new StokEklemeBilgileri();
		int num = reader.ReadInt32();
		stokEklemeBilgileri.Miktar = reader.ReadDouble();
		stokEklemeBilgileri.Miktar2 = reader.ReadDouble();
		stokEklemeBilgileri.sth_birim_pntr = reader.ReadInt32();
		stokEklemeBilgileri.BirimFiyat = FiyatTanimlamasi.ReadFromBinaryReader(reader);
		stokEklemeBilgileri.Aciklama1 = reader.ReadString();
		stokEklemeBilgileri.Aciklama2 = reader.ReadString();
		stokEklemeBilgileri.parti_kodu = reader.ReadString();
		stokEklemeBilgileri.fiyat_farki_mi = reader.ReadBoolean();
		stokEklemeBilgileri.StokDovizCinsiKuru = reader.ReadDouble();
		stokEklemeBilgileri.SatirBirlestir = reader.ReadBoolean();
		stokEklemeBilgileri.SepetteMevcutMuKontroluYap = reader.ReadBoolean();
		stokEklemeBilgileri.MiktarMF = reader.ReadDouble();
		stokEklemeBilgileri.lot_no = reader.ReadInt32();
		stokEklemeBilgileri.proje_kodu = reader.ReadString();
		stokEklemeBilgileri.sorumluluk_merkezi_kodu = reader.ReadString();
		stokEklemeBilgileri.vergi_pntr = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			stokEklemeBilgileri.renk_beden_hareketleri.Add(BEDEN_HAREKETLERI.ReadFromBinaryReader(reader));
		}
		if (num >= 2)
		{
			int num3 = reader.ReadInt32();
			for (int j = 0; j < num3; j++)
			{
				stokEklemeBilgileri.serino_tanimlari.Add(STOK_SERINO_TANIMLARI.ReadFromBinaryReader(reader));
			}
		}
		if (num >= 3)
		{
			stokEklemeBilgileri.Miktar_Formul_Olcu1 = reader.ReadDouble();
			stokEklemeBilgileri.Miktar_Formul_Olcu2 = reader.ReadDouble();
			stokEklemeBilgileri.Miktar_Formul_Olcu3 = reader.ReadDouble();
			stokEklemeBilgileri.Miktar_Formul_Olcu4 = reader.ReadDouble();
			stokEklemeBilgileri.Miktar_Formul_Olcu5 = reader.ReadDouble();
			stokEklemeBilgileri.Miktar_Formul_FormulMiktarNo = reader.ReadInt32();
			stokEklemeBilgileri.Miktar_Formul_FormulMiktar = reader.ReadDouble();
		}
		return stokEklemeBilgileri;
	}
}
