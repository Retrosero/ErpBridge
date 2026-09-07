using System;
using System.Collections.Generic;
using System.IO;
using Fora.Mikro.BedenHareketleri;
using Fora.Mikro.Vergiler;

namespace Fora.Mikro.Siparis;

public class SIPARISLER
{
	public bool Selected { get; set; }

	public int sip_RECno { get; set; }

	public Guid sip_Guid { get; set; }

	public int sip_prosiprecrecI { get; set; }

	public Guid sip_prosip_uid { get; set; }

	public int sip_stalRecId_RECno { get; set; }

	public Guid sip_stal_uid { get; set; }

	public int sip_teklifRecId_RECno { get; set; }

	public Guid sip_teklif_uid { get; set; }

	public int sip_RezRecId_RECno { get; set; }

	public Guid sip_Rez_uid { get; set; }

	public int sip_yetkili_recid_recno { get; set; }

	public Guid sip_yetkili_uid { get; set; }

	public int sip_prosiprecDbId { get; set; }

	public int sip_stalRecId_DBCno { get; set; }

	public int sip_teklifRecId_DBCno { get; set; }

	public int sip_RezRecId_DBCno { get; set; }

	public int sip_yetkili_recid_dbcno { get; set; }

	public int sip_RECid_DBCno { get; set; }

	public int sip_RECid_RECno { get; set; }

	public int sip_SpecRECno { get; set; }

	public bool sip_iptal { get; set; }

	public int sip_fileid { get; set; }

	public bool sip_hidden { get; set; }

	public bool sip_kilitli { get; set; }

	public bool sip_degisti { get; set; }

	public int sip_checksum { get; set; }

	public int sip_create_user { get; set; }

	public DateTime sip_create_date { get; set; }

	public int sip_lastup_user { get; set; }

	public DateTime sip_lastup_date { get; set; }

	public string sip_special1 { get; set; }

	public string sip_special2 { get; set; }

	public string sip_special3 { get; set; }

	public int sip_firmano { get; set; }

	public int sip_subeno { get; set; }

	public DateTime sip_tarih { get; set; }

	public DateTime sip_teslim_tarih { get; set; }

	public enum_sip_tip sip_tip { get; set; }

	public enum_sip_cins sip_cins { get; set; }

	public string sip_evrakno_seri { get; set; }

	public int sip_evrakno_sira { get; set; }

	public int sip_satirno { get; set; }

	public string sip_belgeno { get; set; }

	public DateTime sip_belge_tarih { get; set; }

	public string sip_satici_kod { get; set; }

	public string sip_musteri_kod { get; set; }

	public string sip_stok_kod { get; set; }

	public double sip_b_fiyat { get; set; }

	public double sip_miktar { get; set; }

	public int sip_birim_pntr { get; set; }

	public double sip_teslim_miktar { get; set; }

	public double sip_tutar { get; set; }

	public double sip_iskonto_1 { get; set; }

	public double sip_iskonto_2 { get; set; }

	public double sip_iskonto_3 { get; set; }

	public double sip_iskonto_4 { get; set; }

	public double sip_iskonto_5 { get; set; }

	public double sip_iskonto_6 { get; set; }

	public double sip_masraf_1 { get; set; }

	public double sip_masraf_2 { get; set; }

	public double sip_masraf_3 { get; set; }

	public double sip_masraf_4 { get; set; }

	public int sip_vergi_pntr { get; set; }

	public double sip_vergi { get; set; }

	public int sip_masvergi_pntr { get; set; }

	public double sip_masvergi { get; set; }

	public int sip_opno { get; set; }

	public string sip_aciklama { get; set; }

	public string sip_aciklama2 { get; set; }

	public int sip_depono { get; set; }

	public int sip_OnaylayanKulNo { get; set; }

	public bool sip_vergisiz_fl { get; set; }

	public bool sip_kapat_fl { get; set; }

	public bool sip_promosyon_fl { get; set; }

	public string sip_cari_sormerk { get; set; }

	public string sip_stok_sormerk { get; set; }

	public int sip_cari_grupno { get; set; }

	public int sip_doviz_cinsi { get; set; }

	public double sip_doviz_kuru { get; set; }

	public double sip_alt_doviz_kuru { get; set; }

	public int sip_adresno { get; set; }

	public string sip_teslimturu { get; set; }

	public bool sip_cagrilabilir_fl { get; set; }

	public int sip_iskonto1 { get; set; }

	public int sip_iskonto2 { get; set; }

	public int sip_iskonto3 { get; set; }

	public int sip_iskonto4 { get; set; }

	public int sip_iskonto5 { get; set; }

	public int sip_iskonto6 { get; set; }

	public int sip_masraf1 { get; set; }

	public int sip_masraf2 { get; set; }

	public int sip_masraf3 { get; set; }

	public int sip_masraf4 { get; set; }

	public bool sip_isk1 { get; set; }

	public bool sip_isk2 { get; set; }

	public bool sip_isk3 { get; set; }

	public bool sip_isk4 { get; set; }

	public bool sip_isk5 { get; set; }

	public bool sip_isk6 { get; set; }

	public bool sip_mas1 { get; set; }

	public bool sip_mas2 { get; set; }

	public bool sip_mas3 { get; set; }

	public bool sip_mas4 { get; set; }

	public string sip_Exp_Imp_Kodu { get; set; }

	public double sip_kar_orani { get; set; }

	public enum_sip_durumu sip_durumu { get; set; }

	public double sip_planlananmiktar { get; set; }

	public string sip_parti_kodu { get; set; }

	public int sip_lot_no { get; set; }

	public string sip_projekodu { get; set; }

	public int sip_fiyat_liste_no { get; set; }

	public int sip_Otv_Pntr { get; set; }

	public double sip_Otv_Vergi { get; set; }

	public double sip_otvtutari { get; set; }

	public int sip_OtvVergisiz_Fl { get; set; }

	public string sip_paket_kod { get; set; }

	public enum_sip_harekettipi sip_harekettipi { get; set; }

	public string sip_kapatmanedenkod { get; set; }

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

	public double sip_Olcu1 { get; set; }

	public double sip_Olcu2 { get; set; }

	public double sip_Olcu3 { get; set; }

	public double sip_Olcu4 { get; set; }

	public double sip_Olcu5 { get; set; }

	public int sip_FormulMiktarNo { get; set; }

	public double sip_FormulMiktar { get; set; }

	public string GosterBirimAdi => sip_birim_pntr switch
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
			switch (sip_birim_pntr)
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
				return sip_miktar * Math.Abs(num);
			}
			return sip_miktar / Math.Abs(num);
		}
	}

	public double GosterTeslimMiktar
	{
		get
		{
			double num = 1.0;
			switch (sip_birim_pntr)
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
				return sip_teslim_miktar * Math.Abs(num);
			}
			return sip_teslim_miktar / Math.Abs(num);
		}
	}

	public double GosterKalanTutar => sip_tutar / sip_miktar * KalanMiktar;

	public double GosterKalanMiktar
	{
		get
		{
			double num = 1.0;
			switch (sip_birim_pntr)
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

	public double KalanMiktar => sip_miktar - sip_teslim_miktar;

	public double IskontoTutariToplam => sip_iskonto_1 + sip_iskonto_2 + sip_iskonto_3 + sip_iskonto_4 + sip_iskonto_5 + sip_iskonto_6;

	public double IskontoTutariBirim => (sip_iskonto_1 + sip_iskonto_2 + sip_iskonto_3 + sip_iskonto_4 + sip_iskonto_5 + sip_iskonto_6) / sip_miktar;

	public double MasrafTutariToplam => sip_masraf_1 + sip_masraf_2 + sip_masraf_3 + sip_masraf_4;

	public double MasrafTutariBirim => (sip_masraf_1 + sip_masraf_2 + sip_masraf_3 + sip_masraf_4) / sip_miktar;

	public double KdvTutariToplam => sip_vergi + sip_masvergi;

	public double KdvTutariBirim
	{
		get
		{
			if (KdvTutariToplam > 0.0)
			{
				return KdvTutariToplam / sip_miktar;
			}
			return 0.0;
		}
	}

	public double KdvHaricNetFiyatToplam => sip_tutar - IskontoTutariToplam + MasrafTutariToplam;

	public double KdvHaricNetFiyatBirim => sip_b_fiyat - IskontoTutariBirim + MasrafTutariBirim;

	public SIPARISLER()
	{
		sip_fileid = 21;
		sip_birim_pntr = 1;
		sip_depono = 1;
		sip_doviz_kuru = 1.0;
		sip_alt_doviz_kuru = 1.0;
		sip_adresno = 1;
		sip_cagrilabilir_fl = true;
		sip_iskonto1 = 1;
		sip_iskonto2 = 1;
		sip_iskonto3 = 1;
		sip_iskonto4 = 1;
		sip_iskonto5 = 1;
		sip_iskonto6 = 1;
		sip_masraf1 = 1;
		sip_masraf2 = 1;
		sip_masraf3 = 1;
		sip_masraf4 = 1;
		sip_evrakno_seri = "";
		sip_satici_kod = "";
		sip_musteri_kod = "";
		sip_belgeno = "";
		sip_stok_kod = "";
		sip_cari_sormerk = "";
		sip_stok_sormerk = "";
		sip_projekodu = "";
		sip_special1 = "";
		sip_special2 = "";
		sip_special3 = "";
		sip_aciklama = "";
		sip_aciklama2 = "";
		sip_teslimturu = "";
		sip_Exp_Imp_Kodu = "";
		sip_parti_kodu = "";
		sip_paket_kod = "";
		sip_kapatmanedenkod = "";
		sto_isim = "";
		sto_birim1_ad = "";
		sto_birim2_ad = "";
		sto_birim3_ad = "";
		sto_birim4_ad = "";
		sto_birim1_katsayi = 1.0;
		sto_birim2_katsayi = 1.0;
		sto_birim3_katsayi = 1.0;
		sto_birim4_katsayi = 1.0;
		sip_create_date = DateTime.Now;
		sip_lastup_date = DateTime.Now;
		sip_tarih = DateTime.Now;
		sip_teslim_tarih = DateTime.Now;
		sip_belge_tarih = DateTime.Now;
		renk_beden_hareketleri = new List<BEDEN_HAREKETLERI>();
		sip_Guid = Guid.Empty;
		sip_prosip_uid = Guid.Empty;
		sip_stal_uid = Guid.Empty;
		sip_teklif_uid = Guid.Empty;
		sip_Rez_uid = Guid.Empty;
		sip_yetkili_uid = Guid.Empty;
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

	public double ToplamMiktarFarkliBirim(int hangibirim)
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
			return sip_miktar * Math.Abs(num);
		}
		return sip_miktar / Math.Abs(num);
	}

	public double KdvTutariToplamBrut(List<VergiTanimi> vergitanimlari)
	{
		return sip_miktar * sip_b_fiyat / 100.0 * vergitanimlari[sip_vergi_pntr].Yuzde;
	}

	public double KdvTutariBirimBrut(List<VergiTanimi> vergitanimlari)
	{
		return sip_b_fiyat / 100.0 * vergitanimlari[sip_vergi_pntr].Yuzde;
	}

	public static byte[] WriteToByteArray(SIPARISLER toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(SIPARISLER toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(SIPARISLER toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sip_RECno);
		writer.Write(toWrite.sip_RECid_DBCno);
		writer.Write(toWrite.sip_RECid_RECno);
		writer.Write(toWrite.sip_SpecRECno);
		writer.Write(toWrite.sip_iptal);
		writer.Write(toWrite.sip_fileid);
		writer.Write(toWrite.sip_hidden);
		writer.Write(toWrite.sip_kilitli);
		writer.Write(toWrite.sip_degisti);
		writer.Write(toWrite.sip_checksum);
		writer.Write(toWrite.sip_create_user);
		writer.Write(toWrite.sip_create_date.Ticks);
		writer.Write(toWrite.sip_lastup_user);
		writer.Write(toWrite.sip_lastup_date.Ticks);
		writer.Write(toWrite.sip_special1);
		writer.Write(toWrite.sip_special2);
		writer.Write(toWrite.sip_special3);
		writer.Write(toWrite.sip_firmano);
		writer.Write(toWrite.sip_subeno);
		writer.Write(toWrite.sip_tarih.Ticks);
		writer.Write(toWrite.sip_teslim_tarih.Ticks);
		writer.Write((int)toWrite.sip_tip);
		writer.Write((int)toWrite.sip_cins);
		writer.Write(toWrite.sip_evrakno_seri);
		writer.Write(toWrite.sip_evrakno_sira);
		writer.Write(toWrite.sip_satirno);
		writer.Write(toWrite.sip_belgeno);
		writer.Write(toWrite.sip_belge_tarih.Ticks);
		writer.Write(toWrite.sip_satici_kod);
		writer.Write(toWrite.sip_musteri_kod);
		writer.Write(toWrite.sip_stok_kod);
		writer.Write(toWrite.sip_b_fiyat);
		writer.Write(toWrite.sip_miktar);
		writer.Write(toWrite.sip_birim_pntr);
		writer.Write(toWrite.sip_teslim_miktar);
		writer.Write(toWrite.sip_tutar);
		writer.Write(toWrite.sip_iskonto_1);
		writer.Write(toWrite.sip_iskonto_2);
		writer.Write(toWrite.sip_iskonto_3);
		writer.Write(toWrite.sip_iskonto_4);
		writer.Write(toWrite.sip_iskonto_5);
		writer.Write(toWrite.sip_iskonto_6);
		writer.Write(toWrite.sip_masraf_1);
		writer.Write(toWrite.sip_masraf_2);
		writer.Write(toWrite.sip_masraf_3);
		writer.Write(toWrite.sip_masraf_4);
		writer.Write(toWrite.sip_vergi_pntr);
		writer.Write(toWrite.sip_vergi);
		writer.Write(toWrite.sip_masvergi_pntr);
		writer.Write(toWrite.sip_masvergi);
		writer.Write(toWrite.sip_opno);
		writer.Write(toWrite.sip_aciklama);
		writer.Write(toWrite.sip_aciklama2);
		writer.Write(toWrite.sip_depono);
		writer.Write(toWrite.sip_OnaylayanKulNo);
		writer.Write(toWrite.sip_vergisiz_fl);
		writer.Write(toWrite.sip_kapat_fl);
		writer.Write(toWrite.sip_promosyon_fl);
		writer.Write(toWrite.sip_cari_sormerk);
		writer.Write(toWrite.sip_stok_sormerk);
		writer.Write(toWrite.sip_cari_grupno);
		writer.Write(toWrite.sip_doviz_cinsi);
		writer.Write(toWrite.sip_doviz_kuru);
		writer.Write(toWrite.sip_alt_doviz_kuru);
		writer.Write(toWrite.sip_adresno);
		writer.Write(toWrite.sip_teslimturu);
		writer.Write(toWrite.sip_cagrilabilir_fl);
		writer.Write(toWrite.sip_prosiprecDbId);
		writer.Write(toWrite.sip_prosiprecrecI);
		writer.Write(toWrite.sip_iskonto1);
		writer.Write(toWrite.sip_iskonto2);
		writer.Write(toWrite.sip_iskonto3);
		writer.Write(toWrite.sip_iskonto4);
		writer.Write(toWrite.sip_iskonto5);
		writer.Write(toWrite.sip_iskonto6);
		writer.Write(toWrite.sip_masraf1);
		writer.Write(toWrite.sip_masraf2);
		writer.Write(toWrite.sip_masraf3);
		writer.Write(toWrite.sip_masraf4);
		writer.Write(toWrite.sip_isk1);
		writer.Write(toWrite.sip_isk2);
		writer.Write(toWrite.sip_isk3);
		writer.Write(toWrite.sip_isk4);
		writer.Write(toWrite.sip_isk5);
		writer.Write(toWrite.sip_isk6);
		writer.Write(toWrite.sip_mas1);
		writer.Write(toWrite.sip_mas2);
		writer.Write(toWrite.sip_mas3);
		writer.Write(toWrite.sip_mas4);
		writer.Write(toWrite.sip_Exp_Imp_Kodu);
		writer.Write(toWrite.sip_kar_orani);
		writer.Write((int)toWrite.sip_durumu);
		writer.Write(toWrite.sip_stalRecId_DBCno);
		writer.Write(toWrite.sip_stalRecId_RECno);
		writer.Write(toWrite.sip_planlananmiktar);
		writer.Write(toWrite.sip_teklifRecId_DBCno);
		writer.Write(toWrite.sip_teklifRecId_RECno);
		writer.Write(toWrite.sip_parti_kodu);
		writer.Write(toWrite.sip_lot_no);
		writer.Write(toWrite.sip_projekodu);
		writer.Write(toWrite.sip_fiyat_liste_no);
		writer.Write(toWrite.sip_Otv_Pntr);
		writer.Write(toWrite.sip_Otv_Vergi);
		writer.Write(toWrite.sip_otvtutari);
		writer.Write(toWrite.sip_OtvVergisiz_Fl);
		writer.Write(toWrite.sip_paket_kod);
		writer.Write(toWrite.sip_RezRecId_DBCno);
		writer.Write(toWrite.sip_RezRecId_RECno);
		writer.Write((int)toWrite.sip_harekettipi);
		writer.Write(toWrite.sip_yetkili_recid_dbcno);
		writer.Write(toWrite.sip_yetkili_recid_recno);
		writer.Write(toWrite.sip_kapatmanedenkod);
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

	public static SIPARISLER ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		SIPARISLER result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static SIPARISLER ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static SIPARISLER ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new SIPARISLER
		{
			sip_RECno = reader.ReadInt32(),
			sip_RECid_DBCno = reader.ReadInt32(),
			sip_RECid_RECno = reader.ReadInt32(),
			sip_SpecRECno = reader.ReadInt32(),
			sip_iptal = reader.ReadBoolean(),
			sip_fileid = reader.ReadInt32(),
			sip_hidden = reader.ReadBoolean(),
			sip_kilitli = reader.ReadBoolean(),
			sip_degisti = reader.ReadBoolean(),
			sip_checksum = reader.ReadInt32(),
			sip_create_user = reader.ReadInt32(),
			sip_create_date = new DateTime(reader.ReadInt64()),
			sip_lastup_user = reader.ReadInt32(),
			sip_lastup_date = new DateTime(reader.ReadInt64()),
			sip_special1 = reader.ReadString(),
			sip_special2 = reader.ReadString(),
			sip_special3 = reader.ReadString(),
			sip_firmano = reader.ReadInt32(),
			sip_subeno = reader.ReadInt32(),
			sip_tarih = new DateTime(reader.ReadInt64()),
			sip_teslim_tarih = new DateTime(reader.ReadInt64()),
			sip_tip = (enum_sip_tip)reader.ReadInt32(),
			sip_cins = (enum_sip_cins)reader.ReadInt32(),
			sip_evrakno_seri = reader.ReadString(),
			sip_evrakno_sira = reader.ReadInt32(),
			sip_satirno = reader.ReadInt32(),
			sip_belgeno = reader.ReadString(),
			sip_belge_tarih = new DateTime(reader.ReadInt64()),
			sip_satici_kod = reader.ReadString(),
			sip_musteri_kod = reader.ReadString(),
			sip_stok_kod = reader.ReadString(),
			sip_b_fiyat = reader.ReadDouble(),
			sip_miktar = reader.ReadDouble(),
			sip_birim_pntr = reader.ReadInt32(),
			sip_teslim_miktar = reader.ReadDouble(),
			sip_tutar = reader.ReadDouble(),
			sip_iskonto_1 = reader.ReadDouble(),
			sip_iskonto_2 = reader.ReadDouble(),
			sip_iskonto_3 = reader.ReadDouble(),
			sip_iskonto_4 = reader.ReadDouble(),
			sip_iskonto_5 = reader.ReadDouble(),
			sip_iskonto_6 = reader.ReadDouble(),
			sip_masraf_1 = reader.ReadDouble(),
			sip_masraf_2 = reader.ReadDouble(),
			sip_masraf_3 = reader.ReadDouble(),
			sip_masraf_4 = reader.ReadDouble(),
			sip_vergi_pntr = reader.ReadInt32(),
			sip_vergi = reader.ReadDouble(),
			sip_masvergi_pntr = reader.ReadInt32(),
			sip_masvergi = reader.ReadDouble(),
			sip_opno = reader.ReadInt32(),
			sip_aciklama = reader.ReadString(),
			sip_aciklama2 = reader.ReadString(),
			sip_depono = reader.ReadInt32(),
			sip_OnaylayanKulNo = reader.ReadInt32(),
			sip_vergisiz_fl = reader.ReadBoolean(),
			sip_kapat_fl = reader.ReadBoolean(),
			sip_promosyon_fl = reader.ReadBoolean(),
			sip_cari_sormerk = reader.ReadString(),
			sip_stok_sormerk = reader.ReadString(),
			sip_cari_grupno = reader.ReadInt32(),
			sip_doviz_cinsi = reader.ReadInt32(),
			sip_doviz_kuru = reader.ReadDouble(),
			sip_alt_doviz_kuru = reader.ReadDouble(),
			sip_adresno = reader.ReadInt32(),
			sip_teslimturu = reader.ReadString(),
			sip_cagrilabilir_fl = reader.ReadBoolean(),
			sip_prosiprecDbId = reader.ReadInt32(),
			sip_prosiprecrecI = reader.ReadInt32(),
			sip_iskonto1 = reader.ReadInt32(),
			sip_iskonto2 = reader.ReadInt32(),
			sip_iskonto3 = reader.ReadInt32(),
			sip_iskonto4 = reader.ReadInt32(),
			sip_iskonto5 = reader.ReadInt32(),
			sip_iskonto6 = reader.ReadInt32(),
			sip_masraf1 = reader.ReadInt32(),
			sip_masraf2 = reader.ReadInt32(),
			sip_masraf3 = reader.ReadInt32(),
			sip_masraf4 = reader.ReadInt32(),
			sip_isk1 = reader.ReadBoolean(),
			sip_isk2 = reader.ReadBoolean(),
			sip_isk3 = reader.ReadBoolean(),
			sip_isk4 = reader.ReadBoolean(),
			sip_isk5 = reader.ReadBoolean(),
			sip_isk6 = reader.ReadBoolean(),
			sip_mas1 = reader.ReadBoolean(),
			sip_mas2 = reader.ReadBoolean(),
			sip_mas3 = reader.ReadBoolean(),
			sip_mas4 = reader.ReadBoolean(),
			sip_Exp_Imp_Kodu = reader.ReadString(),
			sip_kar_orani = reader.ReadDouble(),
			sip_durumu = (enum_sip_durumu)reader.ReadInt32(),
			sip_stalRecId_DBCno = reader.ReadInt32(),
			sip_stalRecId_RECno = reader.ReadInt32(),
			sip_planlananmiktar = reader.ReadDouble(),
			sip_teklifRecId_DBCno = reader.ReadInt32(),
			sip_teklifRecId_RECno = reader.ReadInt32(),
			sip_parti_kodu = reader.ReadString(),
			sip_lot_no = reader.ReadInt32(),
			sip_projekodu = reader.ReadString(),
			sip_fiyat_liste_no = reader.ReadInt32(),
			sip_Otv_Pntr = reader.ReadInt32(),
			sip_Otv_Vergi = reader.ReadDouble(),
			sip_otvtutari = reader.ReadDouble(),
			sip_OtvVergisiz_Fl = reader.ReadInt32(),
			sip_paket_kod = reader.ReadString(),
			sip_RezRecId_DBCno = reader.ReadInt32(),
			sip_RezRecId_RECno = reader.ReadInt32(),
			sip_harekettipi = (enum_sip_harekettipi)reader.ReadInt32(),
			sip_yetkili_recid_dbcno = reader.ReadInt32(),
			sip_yetkili_recid_recno = reader.ReadInt32(),
			sip_kapatmanedenkod = reader.ReadString(),
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
