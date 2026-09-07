using System.IO;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public class AlanIslemTipiDeger
{
	public int islem_sirasi { get; set; }

	public enum_YapilacakIslem yapilacak_islem { get; set; }

	public bool ek_kontrol_yap { get; set; }

	public enum_KriterAramaAlanlari aranacak_alan { get; set; }

	public enum_Operator kullanilan_operator { get; set; }

	public string aranacak_deger { get; set; }

	public enum_KriterDegistirmeAlanlari degistirilecek_alan { get; set; }

	public enum_IslemTipi islem_tipi { get; set; }

	public enum_DegerTipi deger_tipi { get; set; }

	public string ozel_deger { get; set; }

	public enum_OzelIslem ozel_islem_tipi { get; set; }

	public enum_KriterDegistirmeAlanlari ozel_islem_yapilacak_alan1 { get; set; }

	public enum_KriterDegistirmeAlanlari ozel_islem_yapilacak_alan2 { get; set; }

	public enum_KriterDegistirmeAlanlari ozel_islem_yapilacak_alan3 { get; set; }

	public enum_KriterDegistirmeAlanlari ozel_islem_yapilacak_alan4 { get; set; }

	public enum_KriterDegistirmeAlanlari ozel_islem_yapilacak_alan5 { get; set; }

	public enum_DegerTipi ozel_islem_parametre1_deger_tipi { get; set; }

	public string ozel_islem_parametre1_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre2_deger_tipi { get; set; }

	public string ozel_islem_parametre2_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre3_deger_tipi { get; set; }

	public string ozel_islem_parametre3_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre4_deger_tipi { get; set; }

	public string ozel_islem_parametre4_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre5_deger_tipi { get; set; }

	public string ozel_islem_parametre5_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre6_deger_tipi { get; set; }

	public string ozel_islem_parametre6_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre7_deger_tipi { get; set; }

	public string ozel_islem_parametre7_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre8_deger_tipi { get; set; }

	public string ozel_islem_parametre8_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre9_deger_tipi { get; set; }

	public string ozel_islem_parametre9_ozel_deger { get; set; }

	public enum_DegerTipi ozel_islem_parametre10_deger_tipi { get; set; }

	public string ozel_islem_parametre10_ozel_deger { get; set; }

	public AlanIslemTipiDeger()
	{
		islem_sirasi = 0;
		yapilacak_islem = enum_YapilacakIslem.VeriDegistir;
		ek_kontrol_yap = false;
		aranacak_alan = enum_KriterAramaAlanlari.cari_kod;
		kullanilan_operator = enum_Operator.Esit;
		aranacak_deger = "";
		degistirilecek_alan = enum_KriterDegistirmeAlanlari.cari_kod;
		islem_tipi = enum_IslemTipi.Degistir;
		deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_deger = "";
		ozel_islem_tipi = enum_OzelIslem.Yok;
		ozel_islem_yapilacak_alan1 = enum_KriterDegistirmeAlanlari.cari_kod;
		ozel_islem_yapilacak_alan2 = enum_KriterDegistirmeAlanlari.cari_kod;
		ozel_islem_yapilacak_alan3 = enum_KriterDegistirmeAlanlari.cari_kod;
		ozel_islem_yapilacak_alan4 = enum_KriterDegistirmeAlanlari.cari_kod;
		ozel_islem_yapilacak_alan5 = enum_KriterDegistirmeAlanlari.cari_kod;
		ozel_islem_parametre1_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre1_ozel_deger = "";
		ozel_islem_parametre2_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre2_ozel_deger = "";
		ozel_islem_parametre3_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre3_ozel_deger = "";
		ozel_islem_parametre4_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre4_ozel_deger = "";
		ozel_islem_parametre5_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre5_ozel_deger = "";
		ozel_islem_parametre6_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre6_ozel_deger = "";
		ozel_islem_parametre7_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre7_ozel_deger = "";
		ozel_islem_parametre8_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre8_ozel_deger = "";
		ozel_islem_parametre9_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre9_ozel_deger = "";
		ozel_islem_parametre10_deger_tipi = enum_DegerTipi.OzelDeger;
		ozel_islem_parametre10_ozel_deger = "";
	}

	public static byte[] WriteToByteArray(AlanIslemTipiDeger toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(AlanIslemTipiDeger toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(AlanIslemTipiDeger toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.islem_sirasi);
		writer.Write((int)toWrite.yapilacak_islem);
		writer.Write(toWrite.ek_kontrol_yap);
		writer.Write((int)toWrite.aranacak_alan);
		writer.Write((int)toWrite.kullanilan_operator);
		writer.Write(toWrite.aranacak_deger);
		writer.Write((int)toWrite.degistirilecek_alan);
		writer.Write((int)toWrite.islem_tipi);
		writer.Write((int)toWrite.deger_tipi);
		writer.Write(toWrite.ozel_deger);
		writer.Write((int)toWrite.ozel_islem_tipi);
		writer.Write((int)toWrite.ozel_islem_yapilacak_alan1);
		writer.Write((int)toWrite.ozel_islem_yapilacak_alan2);
		writer.Write((int)toWrite.ozel_islem_yapilacak_alan3);
		writer.Write((int)toWrite.ozel_islem_yapilacak_alan4);
		writer.Write((int)toWrite.ozel_islem_yapilacak_alan5);
		writer.Write((int)toWrite.ozel_islem_parametre1_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre1_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre2_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre2_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre3_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre3_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre4_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre4_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre5_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre5_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre6_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre6_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre7_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre7_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre8_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre8_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre9_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre9_ozel_deger);
		writer.Write((int)toWrite.ozel_islem_parametre10_deger_tipi);
		writer.Write(toWrite.ozel_islem_parametre10_ozel_deger);
		_ = 2;
	}

	public static AlanIslemTipiDeger ReadFromFile(string FileName)
	{
		FileStream fileStream = new FileStream(FileName, FileMode.Open);
		AlanIslemTipiDeger result = ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		return result;
	}

	public static AlanIslemTipiDeger ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		AlanIslemTipiDeger result = ReadFromStream(memoryStream);
		memoryStream.Close();
		memoryStream.Dispose();
		return result;
	}

	public static AlanIslemTipiDeger ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static AlanIslemTipiDeger ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new AlanIslemTipiDeger
		{
			islem_sirasi = reader.ReadInt32(),
			yapilacak_islem = (enum_YapilacakIslem)reader.ReadInt32(),
			ek_kontrol_yap = reader.ReadBoolean(),
			aranacak_alan = (enum_KriterAramaAlanlari)reader.ReadInt32(),
			kullanilan_operator = (enum_Operator)reader.ReadInt32(),
			aranacak_deger = reader.ReadString(),
			degistirilecek_alan = (enum_KriterDegistirmeAlanlari)reader.ReadInt32(),
			islem_tipi = (enum_IslemTipi)reader.ReadInt32(),
			deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_deger = reader.ReadString(),
			ozel_islem_tipi = (enum_OzelIslem)reader.ReadInt32(),
			ozel_islem_yapilacak_alan1 = (enum_KriterDegistirmeAlanlari)reader.ReadInt32(),
			ozel_islem_yapilacak_alan2 = (enum_KriterDegistirmeAlanlari)reader.ReadInt32(),
			ozel_islem_yapilacak_alan3 = (enum_KriterDegistirmeAlanlari)reader.ReadInt32(),
			ozel_islem_yapilacak_alan4 = (enum_KriterDegistirmeAlanlari)reader.ReadInt32(),
			ozel_islem_yapilacak_alan5 = (enum_KriterDegistirmeAlanlari)reader.ReadInt32(),
			ozel_islem_parametre1_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre1_ozel_deger = reader.ReadString(),
			ozel_islem_parametre2_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre2_ozel_deger = reader.ReadString(),
			ozel_islem_parametre3_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre3_ozel_deger = reader.ReadString(),
			ozel_islem_parametre4_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre4_ozel_deger = reader.ReadString(),
			ozel_islem_parametre5_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre5_ozel_deger = reader.ReadString(),
			ozel_islem_parametre6_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre6_ozel_deger = reader.ReadString(),
			ozel_islem_parametre7_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre7_ozel_deger = reader.ReadString(),
			ozel_islem_parametre8_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre8_ozel_deger = reader.ReadString(),
			ozel_islem_parametre9_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre9_ozel_deger = reader.ReadString(),
			ozel_islem_parametre10_deger_tipi = (enum_DegerTipi)reader.ReadInt32(),
			ozel_islem_parametre10_ozel_deger = reader.ReadString()
		};
	}
}
