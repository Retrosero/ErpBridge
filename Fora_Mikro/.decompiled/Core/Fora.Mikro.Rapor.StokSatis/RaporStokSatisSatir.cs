using System;
using System.IO;

namespace Fora.Mikro.Rapor.StokSatis;

public class RaporStokSatisSatir
{
	public DateTime tarih { get; set; }

	public int normal_iade { get; set; }

	public int stok_sira_no { get; set; }

	public string stok_kod { get; set; }

	public int cari_sira_no { get; set; }

	public string cari_kod { get; set; }

	public int plasiyer_sira_no { get; set; }

	public string plasiyer_kod { get; set; }

	public int depo_sira_no { get; set; }

	public string depo_kod { get; set; }

	public int sorumluluk_merkezi_sira_no { get; set; }

	public string sorumluluk_merkezi_kod { get; set; }

	public int proje_sira_no { get; set; }

	public string proje_kod { get; set; }

	public double miktar { get; set; }

	public double brut_tutar { get; set; }

	public double toplam_iskonto { get; set; }

	public double toplam_vergi { get; set; }

	public int sevk_adres_no { get; set; }

	public double miktar_birim2 { get; set; }

	public double miktar_birim3 { get; set; }

	public double net_toplam_maliyet { get; set; }

	public RaporStokSatisSatir()
	{
		tarih = default(DateTime);
	}

	public RaporStokSatisSatir(DateTime _tarih, int _normal_iade, int _stok_sira_no, int _cari_sira_no, int _plasiyer_sira_no, double _miktar, double _brut_tutar, double _toplam_iskonto, double _toplam_vergi, int _depo_sira_no, int _sorumluluk_merkezi_sira_no, int _proje_sira_no, int _sevk_adres_no, double _birim2_katsayi, double _birim3_katsayi, double _net_toplam_maliyet)
	{
		tarih = _tarih;
		normal_iade = _normal_iade;
		stok_sira_no = _stok_sira_no;
		cari_sira_no = _cari_sira_no;
		plasiyer_sira_no = _plasiyer_sira_no;
		miktar = _miktar;
		brut_tutar = _brut_tutar;
		toplam_iskonto = _toplam_iskonto;
		toplam_vergi = _toplam_vergi;
		depo_sira_no = _depo_sira_no;
		sorumluluk_merkezi_sira_no = _sorumluluk_merkezi_sira_no;
		proje_sira_no = _proje_sira_no;
		sevk_adres_no = _sevk_adres_no;
		if (_birim2_katsayi == 0.0)
		{
			miktar_birim2 = miktar;
		}
		else if (_birim2_katsayi > 0.0)
		{
			miktar_birim2 = miktar * Math.Abs(_birim2_katsayi);
		}
		else
		{
			miktar_birim2 = miktar / Math.Abs(_birim2_katsayi);
		}
		if (_birim3_katsayi == 0.0)
		{
			miktar_birim3 = miktar;
		}
		else if (_birim3_katsayi > 0.0)
		{
			miktar_birim3 = miktar * Math.Abs(_birim3_katsayi);
		}
		else
		{
			miktar_birim3 = miktar / Math.Abs(_birim3_katsayi);
		}
		if (_net_toplam_maliyet != 0.0)
		{
			net_toplam_maliyet = _net_toplam_maliyet;
		}
		else
		{
			net_toplam_maliyet = brut_tutar - toplam_iskonto;
		}
	}

	public RaporStokSatisSatir(DateTime _tarih, int _normal_iade, string _stok_kod, string _cari_kod, string _plasiyer_kod, double _miktar, double _brut_tutar, double _toplam_iskonto, double _toplam_vergi, string _depo_kod, string _sorumluluk_merkezi_kod, string _proje_kod, int _sevk_adres_no, double _birim2_katsayi, double _birim3_katsayi, double _net_toplam_maliyet)
	{
		tarih = _tarih;
		normal_iade = _normal_iade;
		stok_kod = _stok_kod;
		cari_kod = _cari_kod;
		plasiyer_kod = _plasiyer_kod;
		miktar = _miktar;
		brut_tutar = _brut_tutar;
		toplam_iskonto = _toplam_iskonto;
		toplam_vergi = _toplam_vergi;
		depo_kod = _depo_kod;
		sorumluluk_merkezi_kod = _sorumluluk_merkezi_kod;
		proje_kod = _proje_kod;
		sevk_adres_no = _sevk_adres_no;
		if (_birim2_katsayi == 0.0)
		{
			miktar_birim2 = miktar;
		}
		else if (_birim2_katsayi > 0.0)
		{
			miktar_birim2 = miktar * Math.Abs(_birim2_katsayi);
		}
		else
		{
			miktar_birim2 = miktar / Math.Abs(_birim2_katsayi);
		}
		if (_birim3_katsayi == 0.0)
		{
			miktar_birim3 = miktar;
		}
		else if (_birim3_katsayi > 0.0)
		{
			miktar_birim3 = miktar * Math.Abs(_birim3_katsayi);
		}
		else
		{
			miktar_birim3 = miktar / Math.Abs(_birim3_katsayi);
		}
		if (_net_toplam_maliyet != 0.0)
		{
			net_toplam_maliyet = _net_toplam_maliyet;
		}
		else
		{
			net_toplam_maliyet = brut_tutar - toplam_iskonto;
		}
	}

	public static byte[] WriteToByteArray(RaporStokSatisSatir toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSatisSatir toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporStokSatisSatir toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 2;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.tarih.Ticks);
		writer.Write(toWrite.normal_iade);
		writer.Write(toWrite.stok_sira_no);
		writer.Write(toWrite.cari_sira_no);
		writer.Write(toWrite.plasiyer_sira_no);
		writer.Write(toWrite.depo_sira_no);
		writer.Write(toWrite.sorumluluk_merkezi_sira_no);
		writer.Write(toWrite.proje_sira_no);
		writer.Write(toWrite.miktar);
		writer.Write(toWrite.brut_tutar);
		writer.Write(toWrite.toplam_iskonto);
		writer.Write(toWrite.toplam_vergi);
		writer.Write(toWrite.sevk_adres_no);
		writer.Write(toWrite.miktar_birim2);
		writer.Write(toWrite.net_toplam_maliyet);
		if (versiyon >= 2)
		{
			writer.Write(toWrite.miktar_birim3);
		}
	}

	public static RaporStokSatisSatir ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporStokSatisSatir result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporStokSatisSatir ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporStokSatisSatir ReadFromBinaryReader(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporStokSatisSatir raporStokSatisSatir = new RaporStokSatisSatir
		{
			tarih = new DateTime(reader.ReadInt64()),
			normal_iade = reader.ReadInt32(),
			stok_sira_no = reader.ReadInt32(),
			cari_sira_no = reader.ReadInt32(),
			plasiyer_sira_no = reader.ReadInt32(),
			depo_sira_no = reader.ReadInt32(),
			sorumluluk_merkezi_sira_no = reader.ReadInt32(),
			proje_sira_no = reader.ReadInt32(),
			miktar = reader.ReadDouble(),
			brut_tutar = reader.ReadDouble(),
			toplam_iskonto = reader.ReadDouble(),
			toplam_vergi = reader.ReadDouble(),
			sevk_adres_no = reader.ReadInt32(),
			miktar_birim2 = reader.ReadDouble(),
			net_toplam_maliyet = reader.ReadDouble()
		};
		if (num >= 2)
		{
			raporStokSatisSatir.miktar_birim3 = reader.ReadDouble();
		}
		return raporStokSatisSatir;
	}
}
