using System;
using System.IO;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisSatir
{
	public DateTime tarih { get; set; }

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

	public double miktar_siparis { get; set; }

	public double miktar_teslim_edilen { get; set; }

	public double miktar_bekleyen { get; set; }

	public double miktar_vazgecilen { get; set; }

	public double brut_tutar_siparis { get; set; }

	public double toplam_iskonto_siparis { get; set; }

	public double toplam_vergi_siparis { get; set; }

	public double brut_tutar_teslim_edilen { get; set; }

	public double toplam_iskonto_teslim_edilen { get; set; }

	public double toplam_vergi_teslim_edilen { get; set; }

	public double brut_tutar_bekleyen { get; set; }

	public double toplam_iskonto_bekleyen { get; set; }

	public double toplam_vergi_bekleyen { get; set; }

	public double brut_tutar_vazgecilen { get; set; }

	public double toplam_iskonto_vazgecilen { get; set; }

	public double toplam_vergi_vazgecilen { get; set; }

	public RaporStokSiparisSatir()
	{
		tarih = default(DateTime);
	}

	public RaporStokSiparisSatir(DateTime _tarih, int _stok_sira_no, int _cari_sira_no, int _plasiyer_sira_no, int _depo_sira_no, int _sorumluluk_merkezi_sira_no, int _proje_sira_no, double _miktar_siparis, double _miktar_teslim_edilen, double _brut_tutar_siparis, double _toplam_iskonto_siparis, double _toplam_vergi_siparis, bool kapali_fl)
	{
		tarih = _tarih;
		stok_sira_no = _stok_sira_no;
		cari_sira_no = _cari_sira_no;
		plasiyer_sira_no = _plasiyer_sira_no;
		depo_sira_no = _depo_sira_no;
		sorumluluk_merkezi_sira_no = _sorumluluk_merkezi_sira_no;
		proje_sira_no = _proje_sira_no;
		miktar_siparis = _miktar_siparis;
		miktar_teslim_edilen = _miktar_teslim_edilen;
		if (!kapali_fl)
		{
			miktar_bekleyen = miktar_siparis - miktar_teslim_edilen;
		}
		if (kapali_fl)
		{
			miktar_vazgecilen = miktar_siparis - miktar_teslim_edilen;
		}
		brut_tutar_siparis = _brut_tutar_siparis;
		toplam_iskonto_siparis = _toplam_iskonto_siparis;
		toplam_vergi_siparis = _toplam_vergi_siparis;
		brut_tutar_bekleyen = brut_tutar_siparis / miktar_siparis * miktar_bekleyen;
		toplam_iskonto_bekleyen = toplam_iskonto_siparis / miktar_siparis * miktar_bekleyen;
		toplam_vergi_bekleyen = toplam_vergi_siparis / miktar_siparis * miktar_bekleyen;
		brut_tutar_teslim_edilen = brut_tutar_siparis / miktar_siparis * miktar_teslim_edilen;
		toplam_iskonto_teslim_edilen = toplam_iskonto_siparis / miktar_siparis * miktar_teslim_edilen;
		toplam_vergi_teslim_edilen = toplam_vergi_siparis / miktar_siparis * miktar_teslim_edilen;
		brut_tutar_vazgecilen = brut_tutar_siparis / miktar_siparis * miktar_vazgecilen;
		toplam_iskonto_vazgecilen = toplam_iskonto_siparis / miktar_siparis * miktar_vazgecilen;
		toplam_vergi_vazgecilen = toplam_vergi_siparis / miktar_siparis * miktar_vazgecilen;
	}

	public RaporStokSiparisSatir(DateTime _tarih, string _stok_kod, string _cari_kod, string _plasiyer_kod, string _depo_kod, string _sorumluluk_merkezi_kod, string _proje_kod, double _miktar_siparis, double _miktar_teslim_edilen, double _brut_tutar_siparis, double _toplam_iskonto_siparis, double _toplam_vergi_siparis, bool kapali_fl)
	{
		tarih = _tarih;
		stok_kod = _stok_kod;
		cari_kod = _cari_kod;
		plasiyer_kod = _plasiyer_kod;
		depo_kod = _depo_kod;
		sorumluluk_merkezi_kod = _sorumluluk_merkezi_kod;
		proje_kod = _proje_kod;
		miktar_siparis = _miktar_siparis;
		miktar_teslim_edilen = _miktar_teslim_edilen;
		if (!kapali_fl)
		{
			miktar_bekleyen = miktar_siparis - miktar_teslim_edilen;
		}
		if (kapali_fl)
		{
			miktar_vazgecilen = miktar_siparis - miktar_teslim_edilen;
		}
		brut_tutar_siparis = _brut_tutar_siparis;
		toplam_iskonto_siparis = _toplam_iskonto_siparis;
		toplam_vergi_siparis = _toplam_vergi_siparis;
		brut_tutar_bekleyen = brut_tutar_siparis / miktar_siparis * miktar_bekleyen;
		toplam_iskonto_bekleyen = toplam_iskonto_siparis / miktar_siparis * miktar_bekleyen;
		toplam_vergi_bekleyen = toplam_vergi_siparis / miktar_siparis * miktar_bekleyen;
		brut_tutar_teslim_edilen = brut_tutar_siparis / miktar_siparis * miktar_teslim_edilen;
		toplam_iskonto_teslim_edilen = toplam_iskonto_siparis / miktar_siparis * miktar_teslim_edilen;
		toplam_vergi_teslim_edilen = toplam_vergi_siparis / miktar_siparis * miktar_teslim_edilen;
		brut_tutar_vazgecilen = brut_tutar_siparis / miktar_siparis * miktar_vazgecilen;
		toplam_iskonto_vazgecilen = toplam_iskonto_siparis / miktar_siparis * miktar_vazgecilen;
		toplam_vergi_vazgecilen = toplam_vergi_siparis / miktar_siparis * miktar_vazgecilen;
	}

	public static byte[] getBytesWithManualWrite(RaporStokSiparisSatir toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(RaporStokSiparisSatir toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToStream(toWrite, writer);
	}

	public static void WriteToStream(RaporStokSiparisSatir toWrite, BinaryWriter writer)
	{
		int value = 1;
		writer.Write(value);
		writer.Write(toWrite.tarih.Ticks);
		writer.Write(toWrite.stok_sira_no);
		writer.Write(toWrite.cari_sira_no);
		writer.Write(toWrite.plasiyer_sira_no);
		writer.Write(toWrite.depo_sira_no);
		writer.Write(toWrite.sorumluluk_merkezi_sira_no);
		writer.Write(toWrite.proje_sira_no);
		writer.Write(toWrite.miktar_siparis);
		writer.Write(toWrite.miktar_teslim_edilen);
		writer.Write(toWrite.miktar_bekleyen);
		writer.Write(toWrite.miktar_vazgecilen);
		writer.Write(toWrite.brut_tutar_siparis);
		writer.Write(toWrite.toplam_iskonto_siparis);
		writer.Write(toWrite.toplam_vergi_siparis);
		writer.Write(toWrite.brut_tutar_teslim_edilen);
		writer.Write(toWrite.toplam_iskonto_teslim_edilen);
		writer.Write(toWrite.toplam_vergi_teslim_edilen);
		writer.Write(toWrite.brut_tutar_bekleyen);
		writer.Write(toWrite.toplam_iskonto_bekleyen);
		writer.Write(toWrite.toplam_vergi_bekleyen);
		writer.Write(toWrite.brut_tutar_vazgecilen);
		writer.Write(toWrite.toplam_iskonto_vazgecilen);
		writer.Write(toWrite.toplam_vergi_vazgecilen);
	}

	public static RaporStokSiparisSatir ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static RaporStokSiparisSatir ReadFromStream(BinaryReader reader)
	{
		reader.ReadInt32();
		return new RaporStokSiparisSatir
		{
			tarih = new DateTime(reader.ReadInt64()),
			stok_sira_no = reader.ReadInt32(),
			cari_sira_no = reader.ReadInt32(),
			plasiyer_sira_no = reader.ReadInt32(),
			depo_sira_no = reader.ReadInt32(),
			sorumluluk_merkezi_sira_no = reader.ReadInt32(),
			proje_sira_no = reader.ReadInt32(),
			miktar_siparis = reader.ReadDouble(),
			miktar_teslim_edilen = reader.ReadDouble(),
			miktar_bekleyen = reader.ReadDouble(),
			miktar_vazgecilen = reader.ReadDouble(),
			brut_tutar_siparis = reader.ReadDouble(),
			toplam_iskonto_siparis = reader.ReadDouble(),
			toplam_vergi_siparis = reader.ReadDouble(),
			brut_tutar_teslim_edilen = reader.ReadDouble(),
			toplam_iskonto_teslim_edilen = reader.ReadDouble(),
			toplam_vergi_teslim_edilen = reader.ReadDouble(),
			brut_tutar_bekleyen = reader.ReadDouble(),
			toplam_iskonto_bekleyen = reader.ReadDouble(),
			toplam_vergi_bekleyen = reader.ReadDouble(),
			brut_tutar_vazgecilen = reader.ReadDouble(),
			toplam_iskonto_vazgecilen = reader.ReadDouble(),
			toplam_vergi_vazgecilen = reader.ReadDouble()
		};
	}
}
