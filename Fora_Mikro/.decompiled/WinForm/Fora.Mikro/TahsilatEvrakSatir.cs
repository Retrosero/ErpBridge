using System;
using System.IO;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Firmalar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Projeler;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.Subeler;

namespace Fora.Mikro;

public class TahsilatEvrakSatir
{
	public int KayitID { get; set; }

	public bool Aktar { get; set; }

	public enum_GenelEvrakAktarimDurumu AktarimDurumu { get; set; }

	public DateTime evraktarih { get; set; }

	public string evraknoseri { get; set; }

	public int evraknosira { get; set; }

	public string belgeno { get; set; }

	public DateTime belgetarih { get; set; }

	public Cari cari { get; set; }

	public int dovizcinsi { get; set; }

	public Kur kur { get; set; }

	public int alternatifdovizcinsi { get; set; }

	public Kur alternatifdovizkuru { get; set; }

	public Proje proje { get; set; }

	public SorumlulukMerkezi sorumlulukmerkezi { get; set; }

	public string temsilcikodu { get; set; }

	public Firma firma { get; set; }

	public Sube sube { get; set; }

	public int mikrouserno { get; set; }

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

	public enum_tahsilat_cinsi satir_cinsi { get; set; }

	public double satir_tutar { get; set; }

	public DateTime satir_vadesi { get; set; }

	public string satir_aciklama { get; set; }

	public SorumlulukMerkezi satir_sorumlulukmerkezi { get; set; }

	public string satir_kasa_banka_kodu { get; set; }

	public string satir_referans { get; set; }

	public string satir_sck_banka_adres1 { get; set; }

	public string satir_sck_bankano { get; set; }

	public string satir_sck_borclu { get; set; }

	public string satir_sck_hesapno_sehir { get; set; }

	public string satir_sck_no { get; set; }

	public string satir_sck_sube_adres2 { get; set; }

	public string satir_Sck_TCMB_Banka_kodu { get; set; }

	public string satir_Sck_TCMB_il_kodu { get; set; }

	public string satir_Sck_TCMB_Sube_kodu { get; set; }

	public string satir_sck_vdaire_no { get; set; }

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

	public TahsilatEvrakSatir()
	{
		KayitID = 0;
		Aktar = true;
		AktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
		evraktarih = DateTime.Now;
		evraknoseri = "";
		evraknosira = 0;
		belgeno = "";
		belgetarih = DateTime.Now;
		cari = new Cari();
		dovizcinsi = 0;
		kur = new Kur();
		alternatifdovizcinsi = 0;
		alternatifdovizkuru = new Kur();
		proje = new Proje();
		sorumlulukmerkezi = new SorumlulukMerkezi();
		temsilcikodu = "";
		firma = new Firma();
		sube = new Sube();
		mikrouserno = 1;
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
		satir_cinsi = enum_tahsilat_cinsi.Nakit;
		satir_tutar = 0.0;
		satir_vadesi = DateTime.Now;
		satir_aciklama = "";
		satir_sorumlulukmerkezi = new SorumlulukMerkezi();
		satir_kasa_banka_kodu = "";
		satir_referans = "";
		satir_sck_banka_adres1 = "";
		satir_sck_bankano = "";
		satir_sck_borclu = "";
		satir_sck_hesapno_sehir = "";
		satir_sck_no = "";
		satir_sck_sube_adres2 = "";
		satir_Sck_TCMB_Banka_kodu = "";
		satir_Sck_TCMB_il_kodu = "";
		satir_Sck_TCMB_Sube_kodu = "";
		satir_sck_vdaire_no = "";
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
	}

	public static byte[] WriteToByteArray(TahsilatEvrakSatir toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(TahsilatEvrakSatir toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(TahsilatEvrakSatir toWrite, BinaryWriter writer, int versiyon)
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
		writer.Write(toWrite.evraktarih.Ticks);
		writer.Write(toWrite.evraknoseri);
		writer.Write(toWrite.evraknosira);
		writer.Write(toWrite.belgeno);
		writer.Write(toWrite.belgetarih.Ticks);
		Cari.WriteToBinaryWriter(toWrite.cari, writer, 1);
		writer.Write(toWrite.dovizcinsi);
		Kur.WriteToBinaryWriter(toWrite.kur, writer);
		writer.Write(toWrite.alternatifdovizcinsi);
		Kur.WriteToBinaryWriter(toWrite.alternatifdovizkuru, writer);
		Proje.WriteToBinaryWriter(toWrite.proje, writer);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.sorumlulukmerkezi, writer);
		writer.Write(toWrite.temsilcikodu);
		Firma.WriteToBinaryWriter(toWrite.firma, writer);
		Sube.WriteToBinaryWriter(toWrite.sube, writer);
		writer.Write(toWrite.mikrouserno);
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
		writer.Write((int)toWrite.satir_cinsi);
		writer.Write(toWrite.satir_tutar);
		writer.Write(toWrite.satir_vadesi.Ticks);
		writer.Write(toWrite.satir_aciklama);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.satir_sorumlulukmerkezi, writer);
		writer.Write(toWrite.satir_kasa_banka_kodu);
		writer.Write(toWrite.satir_referans);
		writer.Write(toWrite.satir_sck_banka_adres1);
		writer.Write(toWrite.satir_sck_bankano);
		writer.Write(toWrite.satir_sck_borclu);
		writer.Write(toWrite.satir_sck_hesapno_sehir);
		writer.Write(toWrite.satir_sck_no);
		writer.Write(toWrite.satir_sck_sube_adres2);
		writer.Write(toWrite.satir_Sck_TCMB_Banka_kodu);
		writer.Write(toWrite.satir_Sck_TCMB_il_kodu);
		writer.Write(toWrite.satir_Sck_TCMB_Sube_kodu);
		writer.Write(toWrite.satir_sck_vdaire_no);
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
		_ = 2;
	}

	public static TahsilatEvrakSatir ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		TahsilatEvrakSatir result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static TahsilatEvrakSatir ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static TahsilatEvrakSatir ReadFromBinaryReader(BinaryReader reader)
	{
		TahsilatEvrakSatir tahsilatEvrakSatir = new TahsilatEvrakSatir();
		int num = reader.ReadInt32();
		tahsilatEvrakSatir.KayitID = reader.ReadInt32();
		tahsilatEvrakSatir.Aktar = reader.ReadBoolean();
		tahsilatEvrakSatir.AktarimDurumu = (enum_GenelEvrakAktarimDurumu)reader.ReadInt32();
		tahsilatEvrakSatir.evraktarih = new DateTime(reader.ReadInt64());
		tahsilatEvrakSatir.evraknoseri = reader.ReadString();
		tahsilatEvrakSatir.evraknosira = reader.ReadInt32();
		tahsilatEvrakSatir.belgeno = reader.ReadString();
		tahsilatEvrakSatir.belgetarih = new DateTime(reader.ReadInt64());
		tahsilatEvrakSatir.cari = Cari.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.dovizcinsi = reader.ReadInt32();
		tahsilatEvrakSatir.kur = Kur.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.alternatifdovizcinsi = reader.ReadInt32();
		tahsilatEvrakSatir.alternatifdovizkuru = Kur.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.proje = Proje.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.sorumlulukmerkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.temsilcikodu = reader.ReadString();
		tahsilatEvrakSatir.firma = Firma.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.sube = Sube.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.mikrouserno = reader.ReadInt32();
		tahsilatEvrakSatir.aciklama1 = reader.ReadString();
		tahsilatEvrakSatir.aciklama2 = reader.ReadString();
		tahsilatEvrakSatir.aciklama3 = reader.ReadString();
		tahsilatEvrakSatir.aciklama4 = reader.ReadString();
		tahsilatEvrakSatir.aciklama5 = reader.ReadString();
		tahsilatEvrakSatir.aciklama6 = reader.ReadString();
		tahsilatEvrakSatir.aciklama7 = reader.ReadString();
		tahsilatEvrakSatir.aciklama8 = reader.ReadString();
		tahsilatEvrakSatir.aciklama9 = reader.ReadString();
		tahsilatEvrakSatir.aciklama10 = reader.ReadString();
		tahsilatEvrakSatir.degistirspecialalan1 = reader.ReadString();
		tahsilatEvrakSatir.degistirspecialalan2 = reader.ReadString();
		tahsilatEvrakSatir.degistirspecialalan3 = reader.ReadString();
		tahsilatEvrakSatir.satir_cinsi = (enum_tahsilat_cinsi)reader.ReadInt32();
		tahsilatEvrakSatir.satir_tutar = reader.ReadDouble();
		tahsilatEvrakSatir.satir_vadesi = new DateTime(reader.ReadInt64());
		tahsilatEvrakSatir.satir_aciklama = reader.ReadString();
		tahsilatEvrakSatir.satir_sorumlulukmerkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader);
		tahsilatEvrakSatir.satir_kasa_banka_kodu = reader.ReadString();
		tahsilatEvrakSatir.satir_referans = reader.ReadString();
		tahsilatEvrakSatir.satir_sck_banka_adres1 = reader.ReadString();
		tahsilatEvrakSatir.satir_sck_bankano = reader.ReadString();
		tahsilatEvrakSatir.satir_sck_borclu = reader.ReadString();
		tahsilatEvrakSatir.satir_sck_hesapno_sehir = reader.ReadString();
		tahsilatEvrakSatir.satir_sck_no = reader.ReadString();
		tahsilatEvrakSatir.satir_sck_sube_adres2 = reader.ReadString();
		tahsilatEvrakSatir.satir_Sck_TCMB_Banka_kodu = reader.ReadString();
		tahsilatEvrakSatir.satir_Sck_TCMB_il_kodu = reader.ReadString();
		tahsilatEvrakSatir.satir_Sck_TCMB_Sube_kodu = reader.ReadString();
		tahsilatEvrakSatir.satir_sck_vdaire_no = reader.ReadString();
		tahsilatEvrakSatir.kriter_string1 = reader.ReadString();
		tahsilatEvrakSatir.kriter_string2 = reader.ReadString();
		tahsilatEvrakSatir.kriter_string3 = reader.ReadString();
		tahsilatEvrakSatir.kriter_string4 = reader.ReadString();
		tahsilatEvrakSatir.kriter_string5 = reader.ReadString();
		tahsilatEvrakSatir.kriter_double1 = reader.ReadDouble();
		tahsilatEvrakSatir.kriter_double2 = reader.ReadDouble();
		tahsilatEvrakSatir.kriter_double3 = reader.ReadDouble();
		tahsilatEvrakSatir.kriter_double4 = reader.ReadDouble();
		tahsilatEvrakSatir.kriter_double5 = reader.ReadDouble();
		tahsilatEvrakSatir.kriter_bool1 = reader.ReadBoolean();
		tahsilatEvrakSatir.kriter_bool2 = reader.ReadBoolean();
		tahsilatEvrakSatir.kriter_bool3 = reader.ReadBoolean();
		tahsilatEvrakSatir.kriter_bool4 = reader.ReadBoolean();
		tahsilatEvrakSatir.kriter_bool5 = reader.ReadBoolean();
		tahsilatEvrakSatir.DBCno = reader.ReadInt32();
		_ = 2;
		return tahsilatEvrakSatir;
	}
}
