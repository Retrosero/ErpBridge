using System;
using System.Collections.Generic;
using System.IO;
using Fora.Mikro.BedenHareketleri;

namespace Fora.Mikro.DepolarArasiSiparisler;

public class DEPOLAR_ARASI_SIPARISLER
{
	public int ssip_RECid_DBCno { get; set; }

	public int ssip_RECid_RECno { get; set; }

	public int ssip_stalRecId_DBCno { get; set; }

	public int ssip_RECno { get; set; }

	public Guid ssip_Guid { get; set; }

	public int ssip_stalRecId_RECno { get; set; }

	public Guid ssip_stal_uid { get; set; }

	public int ssip_SpecRECno { get; set; }

	public bool ssip_iptal { get; set; }

	public int ssip_fileid { get; set; }

	public bool ssip_hidden { get; set; }

	public bool ssip_kilitli { get; set; }

	public bool ssip_degisti { get; set; }

	public int ssip_checksum { get; set; }

	public int ssip_create_user { get; set; }

	public DateTime ssip_create_date { get; set; }

	public int ssip_lastup_user { get; set; }

	public DateTime ssip_lastup_date { get; set; }

	public string ssip_special1 { get; set; }

	public string ssip_special2 { get; set; }

	public string ssip_special3 { get; set; }

	public int ssip_firmano { get; set; }

	public int ssip_subeno { get; set; }

	public DateTime ssip_tarih { get; set; }

	public DateTime ssip_teslim_tarih { get; set; }

	public string ssip_evrakno_seri { get; set; }

	public int ssip_evrakno_sira { get; set; }

	public int ssip_satirno { get; set; }

	public string ssip_belgeno { get; set; }

	public DateTime ssip_belge_tarih { get; set; }

	public string ssip_stok_kod { get; set; }

	public double ssip_miktar { get; set; }

	public double ssip_b_fiyat { get; set; }

	public double ssip_tutar { get; set; }

	public double ssip_teslim_miktar { get; set; }

	public string ssip_aciklama { get; set; }

	public int ssip_girdepo { get; set; }

	public int ssip_cikdepo { get; set; }

	public bool ssip_kapat_fl { get; set; }

	public int ssip_birim_pntr { get; set; }

	public int ssip_fiyat_liste_no { get; set; }

	public string ssip_paket_kod { get; set; }

	public string ssip_kapatmanedenkod { get; set; }

	public string ssip_projekodu { get; set; }

	public string ssip_sormerkezi { get; set; }

	public string sto_isim { get; set; }

	public string sto_birim1_ad { get; set; }

	public string sto_birim2_ad { get; set; }

	public string sto_birim3_ad { get; set; }

	public string sto_birim4_ad { get; set; }

	public double sto_birim1_katsayi { get; set; }

	public double sto_birim2_katsayi { get; set; }

	public double sto_birim3_katsayi { get; set; }

	public double sto_birim4_katsayi { get; set; }

	public List<BEDEN_HAREKETLERI> renk_beden_hareketleri { get; set; }

	public string GosterBirimAdi => ssip_birim_pntr switch
	{
		1 => sto_birim1_ad, 
		2 => sto_birim2_ad, 
		3 => sto_birim3_ad, 
		4 => sto_birim4_ad, 
		_ => "", 
	};

	public double GosterMiktar
	{
		get
		{
			double num = 1.0;
			switch (ssip_birim_pntr)
			{
			case 1:
				num = sto_birim1_katsayi;
				break;
			case 2:
				num = sto_birim2_katsayi;
				break;
			case 3:
				num = sto_birim3_katsayi;
				break;
			case 4:
				num = sto_birim4_katsayi;
				break;
			}
			if (num > 0.0)
			{
				return ssip_miktar * Math.Abs(num);
			}
			return ssip_miktar / Math.Abs(num);
		}
	}

	public double GosterTeslimMiktar
	{
		get
		{
			double num = 1.0;
			switch (ssip_birim_pntr)
			{
			case 1:
				num = sto_birim1_katsayi;
				break;
			case 2:
				num = sto_birim2_katsayi;
				break;
			case 3:
				num = sto_birim3_katsayi;
				break;
			case 4:
				num = sto_birim4_katsayi;
				break;
			}
			if (num > 0.0)
			{
				return ssip_miktar * Math.Abs(num);
			}
			return ssip_miktar / Math.Abs(num);
		}
	}

	public double GosterKalanTutar => ssip_tutar / ssip_miktar * KalanMiktar;

	public double GosterKalanMiktar
	{
		get
		{
			double num = 1.0;
			switch (ssip_birim_pntr)
			{
			case 1:
				num = sto_birim1_katsayi;
				break;
			case 2:
				num = sto_birim2_katsayi;
				break;
			case 3:
				num = sto_birim3_katsayi;
				break;
			case 4:
				num = sto_birim4_katsayi;
				break;
			}
			if (num > 0.0)
			{
				return KalanMiktar * Math.Abs(num);
			}
			return KalanMiktar / Math.Abs(num);
		}
	}

	public double KalanMiktar => ssip_miktar - ssip_teslim_miktar;

	public DEPOLAR_ARASI_SIPARISLER()
	{
		ssip_fileid = 86;
		ssip_birim_pntr = 1;
		ssip_miktar = 1.0;
		ssip_special1 = "";
		ssip_special2 = "";
		ssip_special3 = "";
		ssip_evrakno_seri = "";
		ssip_belgeno = "";
		ssip_stok_kod = "";
		ssip_aciklama = "";
		ssip_paket_kod = "";
		ssip_kapatmanedenkod = "";
		ssip_projekodu = "";
		ssip_sormerkezi = "";
		sto_isim = "";
		sto_birim1_ad = "";
		sto_birim2_ad = "";
		sto_birim3_ad = "";
		sto_birim4_ad = "";
		sto_birim1_katsayi = 1.0;
		sto_birim2_katsayi = 1.0;
		sto_birim3_katsayi = 1.0;
		sto_birim4_katsayi = 1.0;
		ssip_create_date = DateTime.Now;
		ssip_lastup_date = DateTime.Now;
		ssip_tarih = DateTime.Now;
		ssip_teslim_tarih = DateTime.Now;
		ssip_belge_tarih = DateTime.Now;
		renk_beden_hareketleri = new List<BEDEN_HAREKETLERI>();
		ssip_Guid = Guid.Empty;
		ssip_stal_uid = Guid.Empty;
	}

	public double KalanMiktarFarkliBirim(int hangibirim)
	{
		double num = 1.0;
		switch (hangibirim)
		{
		case 1:
			num = sto_birim1_katsayi;
			break;
		case 2:
			num = sto_birim2_katsayi;
			break;
		case 3:
			num = sto_birim3_katsayi;
			break;
		case 4:
			num = sto_birim4_katsayi;
			break;
		}
		if (num == 0.0)
		{
			return 0.0;
		}
		if (num > 0.0)
		{
			return KalanMiktar * Math.Abs(num);
		}
		return KalanMiktar / Math.Abs(num);
	}

	public static byte[] WriteToByteArray(DEPOLAR_ARASI_SIPARISLER toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(DEPOLAR_ARASI_SIPARISLER toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(DEPOLAR_ARASI_SIPARISLER toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.ssip_RECno);
		writer.Write(toWrite.ssip_RECid_DBCno);
		writer.Write(toWrite.ssip_RECid_RECno);
		writer.Write(toWrite.ssip_SpecRECno);
		writer.Write(toWrite.ssip_iptal);
		writer.Write(toWrite.ssip_fileid);
		writer.Write(toWrite.ssip_hidden);
		writer.Write(toWrite.ssip_kilitli);
		writer.Write(toWrite.ssip_degisti);
		writer.Write(toWrite.ssip_checksum);
		writer.Write(toWrite.ssip_create_user);
		writer.Write(toWrite.ssip_create_date.Ticks);
		writer.Write(toWrite.ssip_lastup_user);
		writer.Write(toWrite.ssip_lastup_date.Ticks);
		writer.Write(toWrite.ssip_special1);
		writer.Write(toWrite.ssip_special2);
		writer.Write(toWrite.ssip_special3);
		writer.Write(toWrite.ssip_firmano);
		writer.Write(toWrite.ssip_subeno);
		writer.Write(toWrite.ssip_tarih.Ticks);
		writer.Write(toWrite.ssip_teslim_tarih.Ticks);
		writer.Write(toWrite.ssip_evrakno_seri);
		writer.Write(toWrite.ssip_evrakno_sira);
		writer.Write(toWrite.ssip_satirno);
		writer.Write(toWrite.ssip_belgeno);
		writer.Write(toWrite.ssip_belge_tarih.Ticks);
		writer.Write(toWrite.ssip_stok_kod);
		writer.Write(toWrite.ssip_miktar);
		writer.Write(toWrite.ssip_b_fiyat);
		writer.Write(toWrite.ssip_tutar);
		writer.Write(toWrite.ssip_teslim_miktar);
		writer.Write(toWrite.ssip_aciklama);
		writer.Write(toWrite.ssip_girdepo);
		writer.Write(toWrite.ssip_cikdepo);
		writer.Write(toWrite.ssip_kapat_fl);
		writer.Write(toWrite.ssip_birim_pntr);
		writer.Write(toWrite.ssip_fiyat_liste_no);
		writer.Write(toWrite.ssip_stalRecId_DBCno);
		writer.Write(toWrite.ssip_stalRecId_RECno);
		writer.Write(toWrite.ssip_paket_kod);
		writer.Write(toWrite.ssip_kapatmanedenkod);
		writer.Write(toWrite.ssip_projekodu);
		writer.Write(toWrite.ssip_sormerkezi);
		writer.Write(toWrite.sto_isim);
		writer.Write(toWrite.sto_birim1_ad);
		writer.Write(toWrite.sto_birim2_ad);
		writer.Write(toWrite.sto_birim3_ad);
		writer.Write(toWrite.sto_birim4_ad);
		writer.Write(toWrite.sto_birim1_katsayi);
		writer.Write(toWrite.sto_birim2_katsayi);
		writer.Write(toWrite.sto_birim3_katsayi);
		writer.Write(toWrite.sto_birim4_katsayi);
		_ = 2;
	}

	public static DEPOLAR_ARASI_SIPARISLER ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		DEPOLAR_ARASI_SIPARISLER result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static DEPOLAR_ARASI_SIPARISLER ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static DEPOLAR_ARASI_SIPARISLER ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new DEPOLAR_ARASI_SIPARISLER
		{
			ssip_RECno = reader.ReadInt32(),
			ssip_RECid_DBCno = reader.ReadInt32(),
			ssip_RECid_RECno = reader.ReadInt32(),
			ssip_SpecRECno = reader.ReadInt32(),
			ssip_iptal = reader.ReadBoolean(),
			ssip_fileid = reader.ReadInt32(),
			ssip_hidden = reader.ReadBoolean(),
			ssip_kilitli = reader.ReadBoolean(),
			ssip_degisti = reader.ReadBoolean(),
			ssip_checksum = reader.ReadInt32(),
			ssip_create_user = reader.ReadInt32(),
			ssip_create_date = new DateTime(reader.ReadInt64()),
			ssip_lastup_user = reader.ReadInt32(),
			ssip_lastup_date = new DateTime(reader.ReadInt64()),
			ssip_special1 = reader.ReadString(),
			ssip_special2 = reader.ReadString(),
			ssip_special3 = reader.ReadString(),
			ssip_firmano = reader.ReadInt32(),
			ssip_subeno = reader.ReadInt32(),
			ssip_tarih = new DateTime(reader.ReadInt64()),
			ssip_teslim_tarih = new DateTime(reader.ReadInt64()),
			ssip_evrakno_seri = reader.ReadString(),
			ssip_evrakno_sira = reader.ReadInt32(),
			ssip_satirno = reader.ReadInt32(),
			ssip_belgeno = reader.ReadString(),
			ssip_belge_tarih = new DateTime(reader.ReadInt64()),
			ssip_stok_kod = reader.ReadString(),
			ssip_miktar = reader.ReadDouble(),
			ssip_b_fiyat = reader.ReadDouble(),
			ssip_tutar = reader.ReadDouble(),
			ssip_teslim_miktar = reader.ReadDouble(),
			ssip_aciklama = reader.ReadString(),
			ssip_girdepo = reader.ReadInt32(),
			ssip_cikdepo = reader.ReadInt32(),
			ssip_kapat_fl = reader.ReadBoolean(),
			ssip_birim_pntr = reader.ReadInt32(),
			ssip_fiyat_liste_no = reader.ReadInt32(),
			ssip_stalRecId_DBCno = reader.ReadInt32(),
			ssip_stalRecId_RECno = reader.ReadInt32(),
			ssip_paket_kod = reader.ReadString(),
			ssip_kapatmanedenkod = reader.ReadString(),
			ssip_projekodu = reader.ReadString(),
			ssip_sormerkezi = reader.ReadString(),
			sto_isim = reader.ReadString(),
			sto_birim1_ad = reader.ReadString(),
			sto_birim2_ad = reader.ReadString(),
			sto_birim3_ad = reader.ReadString(),
			sto_birim4_ad = reader.ReadString(),
			sto_birim1_katsayi = reader.ReadDouble(),
			sto_birim2_katsayi = reader.ReadDouble(),
			sto_birim3_katsayi = reader.ReadDouble(),
			sto_birim4_katsayi = reader.ReadDouble()
		};
	}
}
