using System.Collections.Generic;
using System.IO;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.Mikro.Yazdirma;

public class YaziciAyarlari
{
	public Parametreler genelayarlar { get; set; }

	public List<Parametreler> alanlar { get; set; }

	private string _sablonadi { get; set; }

	public YaziciAyarlari(string SablonAdi)
	{
		_sablonadi = SablonAdi;
		alanlar = new List<Parametreler>();
		genelayarlaritanimla();
	}

	private void genelayarlaritanimla()
	{
		genelayarlar = new Parametreler();
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 1, "SablonAdi", ""));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 2, "SayfaKolonSayisi", "120"));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 3, "SayfaSatirSayisi", "60"));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 4, "SayfaDokumSayisi", "1"));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 5, "DetayBaslangicSatiri", "15"));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 6, "DetayBasiSatirSayisi", "1"));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 7, "DetayBirSayfadakiKayitSayisi", "20"));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 8, "AltBasliklarSadeceSonSayfadaYazilsin", "1"));
		genelayarlar.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "GenelAyarlar", "", 9, "StokGruplandirmaSecenegi", "0"));
	}

	public void alanekle(string alanismi)
	{
		Parametreler parametreler = new Parametreler();
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 1, "Isim", alanismi));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 2, "BasilacakAlan", "0"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 3, "VeriTipi", "0"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 4, "Veri", ""));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 5, "Kolon", "1"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 6, "Satir", "1"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 7, "Genislik", "20"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 8, "Hizalama", "0"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 9, "DetayinBittigiYereKaydir", "0"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 10, "OndalikHaneSayisi", "2"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 11, "SonunaParaBirimiEkle", "0"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 12, "BasinaParaBirimiEkle", "0"));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 13, "BinlikAyraci", "."));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 14, "OndalikAyraci", ","));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 15, "OnEk", ""));
		parametreler.ParametreListesi.Add(new Parametre("YaziciAyarlari", _sablonadi, "Alan", alanismi, 16, "SonEk", ""));
		alanlar.Add(parametreler);
	}

	public void alansil(string alanismi)
	{
		int num = -1;
		int num2 = 0;
		foreach (Parametreler item in alanlar)
		{
			if (item._GetParametre("Isim")._GetString == alanismi)
			{
				num = num2;
				break;
			}
			num2++;
		}
		if (num != -1)
		{
			alanlar.RemoveAt(num);
		}
	}

	public static byte[] WriteToByteArray(YaziciAyarlari toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(YaziciAyarlari toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(YaziciAyarlari toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite._sablonadi);
		Parametreler.WriteToBinaryWriter(toWrite.genelayarlar, writer, 2);
		writer.Write(toWrite.alanlar.Count);
		foreach (Parametreler item in toWrite.alanlar)
		{
			Parametreler.WriteToBinaryWriter(item, writer, 2);
		}
	}

	public static YaziciAyarlari ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		YaziciAyarlari result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static YaziciAyarlari ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static YaziciAyarlari ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		reader.ReadString();
		YaziciAyarlari yaziciAyarlari = new YaziciAyarlari("TEST");
		yaziciAyarlari.genelayarlar = Parametreler.ReadFromBinaryReader(reader);
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			yaziciAyarlari.alanlar.Add(Parametreler.ReadFromBinaryReader(reader));
		}
		return yaziciAyarlari;
	}
}
