using System;
using System.Collections.Generic;
using System.IO;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Depolar;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Firmalar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Projeler;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Subeler;
using Fora.Mikro.Vergiler;

namespace Fora.Mikro;

public class GenelEvrakSatir
{
	public int KayitID { get; set; }

	public bool Aktar { get; set; }

	public enum_GenelEvrakAktarimDurumu AktarimDurumu { get; set; }

	public enum_GenelEvrakTipleri evraktipi { get; set; }

	public enum_cha_normal_Iade normaliade { get; set; }

	public enum_KapamaSekli kapamasekli { get; set; }

	public enum_cha_ticaret_turu ticaretturu { get; set; }

	public DateTime evraktarih { get; set; }

	public string evraknoseri { get; set; }

	public int evraknosira { get; set; }

	public string belgeno { get; set; }

	public DateTime belgetarih { get; set; }

	public Cari cari { get; set; }

	public int odemeplani { get; set; }

	public FiyatListesi fiyatlistesi { get; set; }

	public int dovizcinsi { get; set; }

	public Kur kur { get; set; }

	public int alternatifdovizcinsi { get; set; }

	public Kur alternatifdovizkuru { get; set; }

	public Depo kaynakdepo { get; set; }

	public Depo hedefdepo { get; set; }

	public Proje proje { get; set; }

	public SorumlulukMerkezi sorumlulukmerkezi { get; set; }

	public string temsilcikodu { get; set; }

	public Firma firma { get; set; }

	public Sube sube { get; set; }

	public int mikrouserno { get; set; }

	public DateTime sevkteslimtarihi { get; set; }

	public int sevkadresno { get; set; }

	public string kapamahesapkodu { get; set; }

	public string faturaaciklama { get; set; }

	public string aciklama1 { get; set; }

	public string aciklama2 { get; set; }

	public string aciklama3 { get; set; }

	public string aciklama4 { get; set; }

	public string aciklama5 { get; set; }

	public string aciklama6 { get; set; }

	public string aciklama7 { get; set; }

	public string aciklama8 { get; set; }

	public string aciklama9 { get; set; }

	public string aciklama10 { get; set; }

	public string degistirspecialalan1 { get; set; }

	public string degistirspecialalan2 { get; set; }

	public string degistirspecialalan3 { get; set; }

	public enum_SatirCinsi satircinsi { get; set; }

	public string stokhizmetkodu { get; set; }

	public string stokpartikodu { get; set; }

	public int stoklotno { get; set; }

	public double miktar { get; set; }

	public double miktar2 { get; set; }

	public int vergi_pntr { get; set; }

	public FiyatTanimlamasi birimfiyat { get; set; }

	public bool fiyat_fark_mi { get; set; }

	public string kriter_string1 { get; set; }

	public string kriter_string2 { get; set; }

	public string kriter_string3 { get; set; }

	public string kriter_string4 { get; set; }

	public string kriter_string5 { get; set; }

	public double kriter_double1 { get; set; }

	public double kriter_double2 { get; set; }

	public double kriter_double3 { get; set; }

	public double kriter_double4 { get; set; }

	public double kriter_double5 { get; set; }

	public bool kriter_bool1 { get; set; }

	public bool kriter_bool2 { get; set; }

	public bool kriter_bool3 { get; set; }

	public bool kriter_bool4 { get; set; }

	public bool kriter_bool5 { get; set; }

	public int DBCno { get; set; }

	public GenelEvrakSatir()
	{
		KayitID = 0;
		Aktar = true;
		AktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
		evraktipi = enum_GenelEvrakTipleri.SatisFaturasi;
		normaliade = enum_cha_normal_Iade.Normal;
		kapamasekli = enum_KapamaSekli.AcikHesap;
		ticaretturu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
		evraktarih = DateTime.Now;
		evraknoseri = "";
		evraknosira = 0;
		belgeno = "";
		belgetarih = DateTime.Now;
		cari = new Cari();
		odemeplani = 0;
		fiyatlistesi = new FiyatListesi();
		dovizcinsi = 0;
		kur = new Kur();
		alternatifdovizcinsi = 0;
		alternatifdovizkuru = new Kur();
		kaynakdepo = new Depo();
		hedefdepo = new Depo();
		proje = new Proje();
		sorumlulukmerkezi = new SorumlulukMerkezi();
		temsilcikodu = "";
		firma = new Firma();
		sube = new Sube();
		mikrouserno = 1;
		sevkteslimtarihi = DateTime.Now;
		sevkadresno = 1;
		kapamahesapkodu = "";
		faturaaciklama = "";
		aciklama1 = "";
		aciklama2 = "";
		aciklama3 = "";
		aciklama4 = "";
		aciklama5 = "";
		aciklama6 = "";
		aciklama7 = "";
		aciklama8 = "";
		aciklama9 = "";
		aciklama10 = "";
		degistirspecialalan1 = "";
		degistirspecialalan2 = "";
		degistirspecialalan3 = "";
		satircinsi = enum_SatirCinsi.Stok;
		stokhizmetkodu = "";
		vergi_pntr = 0;
		miktar = 1.0;
		miktar2 = 0.0;
		birimfiyat = new FiyatTanimlamasi();
		fiyat_fark_mi = false;
		kriter_string1 = "";
		kriter_string2 = "";
		kriter_string3 = "";
		kriter_string4 = "";
		kriter_string5 = "";
		kriter_double1 = 0.0;
		kriter_double2 = 0.0;
		kriter_double3 = 0.0;
		kriter_double4 = 0.0;
		kriter_double5 = 0.0;
		kriter_bool1 = false;
		kriter_bool2 = false;
		kriter_bool3 = false;
		kriter_bool4 = false;
		kriter_bool5 = false;
		DBCno = 0;
		stokpartikodu = "";
		stoklotno = 0;
	}

	public double GetEvrakAraToplam()
	{
		return miktar * birimfiyat.FiyatBrut;
	}

	public double GetEvrakIskontoToplam()
	{
		return miktar * birimfiyat.IskontoTutariToplam;
	}

	public double GetEvrakMasrafToplam()
	{
		return miktar * birimfiyat.MasrafTutariToplam;
	}

	public double GetEvrakOtvToplam()
	{
		return miktar * birimfiyat.OtvTutariToplam;
	}

	public double GetEvrakKdvToplam(List<VergiTanimi> vergitanimlari)
	{
		return miktar * birimfiyat.FiyatNetMasrafsiz / 100.0 * vergitanimlari[vergi_pntr].Yuzde + miktar * birimfiyat.MasrafTutariToplam / 100.0 * 18.0 + miktar * birimfiyat.OtvTutariToplam / 100.0 * vergitanimlari[birimfiyat.OtvVergiPntr].Yuzde;
	}

	public double GetKdvTutariBirimNet(List<VergiTanimi> vergitanimlari)
	{
		return birimfiyat.FiyatNetMasrafsiz / 100.0 * vergitanimlari[vergi_pntr].Yuzde + birimfiyat.MasrafTutariToplam / 100.0 * 18.0 + birimfiyat.OtvTutariToplam / 100.0 * vergitanimlari[birimfiyat.OtvVergiPntr].Yuzde;
	}

	public double GetEvrakYekun(List<VergiTanimi> vergitanimlari)
	{
		return miktar * (birimfiyat.FiyatNetMasrafli + GetKdvTutariBirimNet(vergitanimlari)) + GetEvrakOtvToplam();
	}

	public static byte[] WriteToByteArray(GenelEvrakSatir toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(GenelEvrakSatir toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(GenelEvrakSatir toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.KayitID);
		writer.Write(toWrite.Aktar);
		writer.Write((int)toWrite.AktarimDurumu);
		writer.Write((int)toWrite.evraktipi);
		writer.Write((int)toWrite.normaliade);
		writer.Write((int)toWrite.kapamasekli);
		writer.Write((int)toWrite.ticaretturu);
		writer.Write(toWrite.evraktarih.Ticks);
		writer.Write(toWrite.evraknoseri);
		writer.Write(toWrite.evraknosira);
		writer.Write(toWrite.belgeno);
		writer.Write(toWrite.belgetarih.Ticks);
		Cari.WriteToBinaryWriter(toWrite.cari, writer, 1);
		writer.Write(toWrite.odemeplani);
		FiyatListesi.WriteToBinaryWriter(toWrite.fiyatlistesi, writer);
		writer.Write(toWrite.dovizcinsi);
		Kur.WriteToBinaryWriter(toWrite.kur, writer);
		writer.Write(toWrite.alternatifdovizcinsi);
		Kur.WriteToBinaryWriter(toWrite.alternatifdovizkuru, writer);
		Depo.WriteToBinaryWriter(toWrite.kaynakdepo, writer);
		Depo.WriteToBinaryWriter(toWrite.hedefdepo, writer);
		Proje.WriteToBinaryWriter(toWrite.proje, writer);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.sorumlulukmerkezi, writer);
		writer.Write(toWrite.temsilcikodu);
		Firma.WriteToBinaryWriter(toWrite.firma, writer);
		Sube.WriteToBinaryWriter(toWrite.sube, writer);
		writer.Write(toWrite.mikrouserno);
		writer.Write(toWrite.sevkteslimtarihi.Ticks);
		writer.Write(toWrite.sevkadresno);
		writer.Write(toWrite.kapamahesapkodu);
		writer.Write(toWrite.faturaaciklama);
		writer.Write(toWrite.aciklama1);
		writer.Write(toWrite.aciklama2);
		writer.Write(toWrite.aciklama3);
		writer.Write(toWrite.aciklama4);
		writer.Write(toWrite.aciklama5);
		writer.Write(toWrite.aciklama6);
		writer.Write(toWrite.aciklama7);
		writer.Write(toWrite.aciklama8);
		writer.Write(toWrite.aciklama9);
		writer.Write(toWrite.aciklama10);
		writer.Write(toWrite.degistirspecialalan1);
		writer.Write(toWrite.degistirspecialalan2);
		writer.Write(toWrite.degistirspecialalan3);
		writer.Write((int)toWrite.satircinsi);
		writer.Write(toWrite.stokhizmetkodu);
		writer.Write(toWrite.miktar);
		writer.Write(toWrite.miktar2);
		writer.Write(toWrite.vergi_pntr);
		FiyatTanimlamasi.WriteToBinaryWriter(toWrite.birimfiyat, writer, 1);
		writer.Write(toWrite.fiyat_fark_mi);
		writer.Write(toWrite.kriter_string1);
		writer.Write(toWrite.kriter_string2);
		writer.Write(toWrite.kriter_string3);
		writer.Write(toWrite.kriter_string4);
		writer.Write(toWrite.kriter_string5);
		writer.Write(toWrite.kriter_double1);
		writer.Write(toWrite.kriter_double2);
		writer.Write(toWrite.kriter_double3);
		writer.Write(toWrite.kriter_double4);
		writer.Write(toWrite.kriter_double5);
		writer.Write(toWrite.kriter_bool1);
		writer.Write(toWrite.kriter_bool2);
		writer.Write(toWrite.kriter_bool3);
		writer.Write(toWrite.kriter_bool4);
		writer.Write(toWrite.kriter_bool5);
		writer.Write(toWrite.DBCno);
		writer.Write(toWrite.stokpartikodu);
		writer.Write(toWrite.stoklotno);
	}

	public static GenelEvrakSatir ReadFromFile(string FileName)
	{
		FileStream fileStream = new FileStream(FileName, FileMode.Open);
		GenelEvrakSatir result = ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		return result;
	}

	public static GenelEvrakSatir ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		GenelEvrakSatir result = ReadFromStream(memoryStream);
		memoryStream.Close();
		memoryStream.Dispose();
		return result;
	}

	public static GenelEvrakSatir ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static GenelEvrakSatir ReadFromBinaryReader(BinaryReader reader)
	{
		GenelEvrakSatir genelEvrakSatir = new GenelEvrakSatir();
		reader.ReadInt32();
		genelEvrakSatir.KayitID = reader.ReadInt32();
		genelEvrakSatir.Aktar = reader.ReadBoolean();
		genelEvrakSatir.AktarimDurumu = (enum_GenelEvrakAktarimDurumu)reader.ReadInt32();
		genelEvrakSatir.evraktipi = (enum_GenelEvrakTipleri)reader.ReadInt32();
		genelEvrakSatir.normaliade = (enum_cha_normal_Iade)reader.ReadInt32();
		genelEvrakSatir.kapamasekli = (enum_KapamaSekli)reader.ReadInt32();
		genelEvrakSatir.ticaretturu = (enum_cha_ticaret_turu)reader.ReadInt32();
		genelEvrakSatir.evraktarih = new DateTime(reader.ReadInt64());
		genelEvrakSatir.evraknoseri = reader.ReadString();
		genelEvrakSatir.evraknosira = reader.ReadInt32();
		genelEvrakSatir.belgeno = reader.ReadString();
		genelEvrakSatir.belgetarih = new DateTime(reader.ReadInt64());
		genelEvrakSatir.cari = Cari.ReadFromBinaryReader(reader);
		genelEvrakSatir.odemeplani = reader.ReadInt32();
		genelEvrakSatir.fiyatlistesi = FiyatListesi.ReadFromBinaryReader(reader);
		genelEvrakSatir.dovizcinsi = reader.ReadInt32();
		genelEvrakSatir.kur = Kur.ReadFromBinaryReader(reader);
		genelEvrakSatir.alternatifdovizcinsi = reader.ReadInt32();
		genelEvrakSatir.alternatifdovizkuru = Kur.ReadFromBinaryReader(reader);
		genelEvrakSatir.kaynakdepo = Depo.ReadFromBinaryReader(reader);
		genelEvrakSatir.hedefdepo = Depo.ReadFromBinaryReader(reader);
		genelEvrakSatir.proje = Proje.ReadFromBinaryReader(reader);
		genelEvrakSatir.sorumlulukmerkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader);
		genelEvrakSatir.temsilcikodu = reader.ReadString();
		genelEvrakSatir.firma = Firma.ReadFromBinaryReader(reader);
		genelEvrakSatir.sube = Sube.ReadFromBinaryReader(reader);
		genelEvrakSatir.mikrouserno = reader.ReadInt32();
		genelEvrakSatir.sevkteslimtarihi = new DateTime(reader.ReadInt64());
		genelEvrakSatir.sevkadresno = reader.ReadInt32();
		genelEvrakSatir.kapamahesapkodu = reader.ReadString();
		genelEvrakSatir.faturaaciklama = reader.ReadString();
		genelEvrakSatir.aciklama1 = reader.ReadString();
		genelEvrakSatir.aciklama2 = reader.ReadString();
		genelEvrakSatir.aciklama3 = reader.ReadString();
		genelEvrakSatir.aciklama4 = reader.ReadString();
		genelEvrakSatir.aciklama5 = reader.ReadString();
		genelEvrakSatir.aciklama6 = reader.ReadString();
		genelEvrakSatir.aciklama7 = reader.ReadString();
		genelEvrakSatir.aciklama8 = reader.ReadString();
		genelEvrakSatir.aciklama9 = reader.ReadString();
		genelEvrakSatir.aciklama10 = reader.ReadString();
		genelEvrakSatir.degistirspecialalan1 = reader.ReadString();
		genelEvrakSatir.degistirspecialalan2 = reader.ReadString();
		genelEvrakSatir.degistirspecialalan3 = reader.ReadString();
		genelEvrakSatir.satircinsi = (enum_SatirCinsi)reader.ReadInt32();
		genelEvrakSatir.stokhizmetkodu = reader.ReadString();
		genelEvrakSatir.miktar = reader.ReadDouble();
		genelEvrakSatir.miktar2 = reader.ReadDouble();
		genelEvrakSatir.vergi_pntr = reader.ReadInt32();
		genelEvrakSatir.birimfiyat = FiyatTanimlamasi.ReadFromBinaryReader(reader);
		genelEvrakSatir.fiyat_fark_mi = reader.ReadBoolean();
		genelEvrakSatir.kriter_string1 = reader.ReadString();
		genelEvrakSatir.kriter_string2 = reader.ReadString();
		genelEvrakSatir.kriter_string3 = reader.ReadString();
		genelEvrakSatir.kriter_string4 = reader.ReadString();
		genelEvrakSatir.kriter_string5 = reader.ReadString();
		genelEvrakSatir.kriter_double1 = reader.ReadDouble();
		genelEvrakSatir.kriter_double2 = reader.ReadDouble();
		genelEvrakSatir.kriter_double3 = reader.ReadDouble();
		genelEvrakSatir.kriter_double4 = reader.ReadDouble();
		genelEvrakSatir.kriter_double5 = reader.ReadDouble();
		genelEvrakSatir.kriter_bool1 = reader.ReadBoolean();
		genelEvrakSatir.kriter_bool2 = reader.ReadBoolean();
		genelEvrakSatir.kriter_bool3 = reader.ReadBoolean();
		genelEvrakSatir.kriter_bool4 = reader.ReadBoolean();
		genelEvrakSatir.kriter_bool5 = reader.ReadBoolean();
		genelEvrakSatir.DBCno = reader.ReadInt32();
		genelEvrakSatir.stokpartikodu = reader.ReadString();
		genelEvrakSatir.stoklotno = reader.ReadInt32();
		return genelEvrakSatir;
	}
}
