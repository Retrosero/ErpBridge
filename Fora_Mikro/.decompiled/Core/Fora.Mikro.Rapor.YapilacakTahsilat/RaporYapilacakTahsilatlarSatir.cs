using System;
using System.IO;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class RaporYapilacakTahsilatlarSatir
{
	public DateTime vade_tarihi { get; set; }

	public int cari_sira_no { get; set; }

	public string cari_kod { get; set; }

	public int plasiyer_sira_no { get; set; }

	public string plasiyer_kod { get; set; }

	public int sorumluluk_merkezi_sira_no { get; set; }

	public string sorumluluk_merkezi_kod { get; set; }

	public int proje_sira_no { get; set; }

	public string proje_kod { get; set; }

	public double meblag { get; set; }

	public int doviz_cinsi { get; set; }

	public int firma_sira_no { get; set; }

	public int sube_sira_no { get; set; }

	public RaporYapilacakTahsilatlarSatir()
	{
		vade_tarihi = default(DateTime);
	}

	public RaporYapilacakTahsilatlarSatir(DateTime _vade_tarihi, int _cari_sira_no, int _plasiyer_sira_no, int _sorumluluk_merkezi_sira_no, int _proje_sira_no, double _meblag, int _doviz_cinsi, int _firma_sira_no, int _sube_sira_no)
	{
		vade_tarihi = _vade_tarihi;
		cari_sira_no = _cari_sira_no;
		plasiyer_sira_no = _plasiyer_sira_no;
		sorumluluk_merkezi_sira_no = _sorumluluk_merkezi_sira_no;
		proje_sira_no = _proje_sira_no;
		meblag = _meblag;
		doviz_cinsi = _doviz_cinsi;
		firma_sira_no = _firma_sira_no;
		sube_sira_no = _sube_sira_no;
	}

	public RaporYapilacakTahsilatlarSatir(DateTime _vade_tarihi, string _cari_kod, string _plasiyer_kod, string _sorumluluk_merkezi_kod, string _proje_kod, double _meblag, int _doviz_cinsi, int _firma_sira_no, int _sube_sira_no)
	{
		vade_tarihi = _vade_tarihi;
		cari_kod = _cari_kod;
		plasiyer_kod = _plasiyer_kod;
		sorumluluk_merkezi_kod = _sorumluluk_merkezi_kod;
		proje_kod = _proje_kod;
		meblag = _meblag;
		doviz_cinsi = _doviz_cinsi;
		firma_sira_no = _firma_sira_no;
		sube_sira_no = _sube_sira_no;
	}

	public static byte[] WriteToByteArray(RaporYapilacakTahsilatlarSatir toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporYapilacakTahsilatlarSatir toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(RaporYapilacakTahsilatlarSatir toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.vade_tarihi.Ticks);
		writer.Write(toWrite.cari_sira_no);
		writer.Write(toWrite.plasiyer_sira_no);
		writer.Write(toWrite.sorumluluk_merkezi_sira_no);
		writer.Write(toWrite.proje_sira_no);
		writer.Write(toWrite.meblag);
		writer.Write(toWrite.doviz_cinsi);
		writer.Write(toWrite.firma_sira_no);
		writer.Write(toWrite.sube_sira_no);
		_ = 2;
	}

	public static RaporYapilacakTahsilatlarSatir ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		RaporYapilacakTahsilatlarSatir result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static RaporYapilacakTahsilatlarSatir ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static RaporYapilacakTahsilatlarSatir ReadFromBinaryReader(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		RaporYapilacakTahsilatlarSatir result = new RaporYapilacakTahsilatlarSatir
		{
			vade_tarihi = new DateTime(reader.ReadInt64()),
			cari_sira_no = reader.ReadInt32(),
			plasiyer_sira_no = reader.ReadInt32(),
			sorumluluk_merkezi_sira_no = reader.ReadInt32(),
			proje_sira_no = reader.ReadInt32(),
			meblag = reader.ReadDouble(),
			doviz_cinsi = reader.ReadInt32(),
			firma_sira_no = reader.ReadInt32(),
			sube_sira_no = reader.ReadInt32()
		};
		_ = 2;
		return result;
	}
}
