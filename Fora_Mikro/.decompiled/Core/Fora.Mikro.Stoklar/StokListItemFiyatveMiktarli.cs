using System;
using System.IO;
using Fora.Mikro.Evraklar;

namespace Fora.Mikro.Stoklar;

public class StokListItemFiyatveMiktarli
{
	public int sto_RECno { get; set; }

	public Guid sto_Guid { get; set; }

	public string sto_kod { get; set; }

	public string sto_isim { get; set; }

	public string sto_yabanci_isim { get; set; }

	public string sto_kisa_ismi { get; set; }

	public int sto_perakende_vergi { get; set; }

	public int sto_toptan_vergi { get; set; }

	public string sto_birim1_ad { get; set; }

	public double sto_birim1_katsayi { get; set; }

	public string sto_birim2_ad { get; set; }

	public double sto_birim2_katsayi { get; set; }

	public string sto_birim3_ad { get; set; }

	public double sto_birim3_katsayi { get; set; }

	public string sto_birim4_ad { get; set; }

	public double sto_birim4_katsayi { get; set; }

	public int sto_ver_sip_birim { get; set; }

	public int sto_al_sip_birim { get; set; }

	public string sto_anagrup_kod { get; set; }

	public string sto_altgrup_kod { get; set; }

	public string sto_uretici_kodu { get; set; }

	public string sto_reyon_kodu { get; set; }

	public string sto_marka_kodu { get; set; }

	public bool Depo_Mevcut_MiktarBulundu { get; set; }

	public double Depo_Mevcut_Miktar { get; set; }

	public bool Acik_Siparis_MiktariBulundu { get; set; }

	public double Acik_Siparis_Miktari { get; set; }

	public StokEklemeBilgileri ekleme_bilgileri { get; set; }

	public StokListItemFiyatveMiktarli()
	{
		sto_birim1_ad = "";
		sto_birim1_katsayi = 1.0;
		sto_birim2_ad = "";
		sto_birim2_katsayi = 1.0;
		sto_birim3_ad = "";
		sto_birim3_katsayi = 1.0;
		sto_birim4_ad = "";
		sto_birim4_katsayi = 1.0;
		sto_ver_sip_birim = 1;
		sto_al_sip_birim = 1;
		sto_anagrup_kod = "";
		sto_altgrup_kod = "";
		sto_uretici_kodu = "";
		sto_reyon_kodu = "";
		sto_marka_kodu = "";
		ekleme_bilgileri = new StokEklemeBilgileri();
		ekleme_bilgileri.Miktar = 0.0;
	}

	public StokListItemFiyatveMiktarli(int _sto_RECno, string _sto_kod, string _sto_isim, string _sto_yabanci_isim, string _sto_kisa_ismi, int _sto_perakende_vergi, int _sto_toptan_vergi, string _sto_birim1_ad, double _sto_birim1_katsayi, string _sto_birim2_ad, double _sto_birim2_katsayi, string _sto_birim3_ad, double _sto_birim3_katsayi, string _sto_birim4_ad, double _sto_birim4_katsayi, int _sto_al_sip_birim, string _sto_anagrup_kod, string _sto_altgrup_kod, string _sto_uretici_kodu, string _sto_reyon_kodu, string _sto_marka_kodu, enum_toptan_perakende toptan_perakende)
	{
		sto_RECno = _sto_RECno;
		sto_kod = _sto_kod;
		sto_isim = _sto_isim;
		sto_yabanci_isim = _sto_yabanci_isim;
		sto_kisa_ismi = _sto_kisa_ismi;
		sto_perakende_vergi = _sto_perakende_vergi;
		sto_toptan_vergi = _sto_toptan_vergi;
		sto_birim1_ad = _sto_birim1_ad;
		sto_birim1_katsayi = _sto_birim1_katsayi;
		sto_birim2_ad = _sto_birim2_ad;
		sto_birim2_katsayi = _sto_birim2_katsayi;
		sto_birim3_ad = _sto_birim3_ad;
		sto_birim3_katsayi = _sto_birim3_katsayi;
		sto_birim4_ad = _sto_birim4_ad;
		sto_birim4_katsayi = _sto_birim4_katsayi;
		sto_al_sip_birim = _sto_al_sip_birim;
		sto_anagrup_kod = _sto_anagrup_kod;
		sto_altgrup_kod = _sto_altgrup_kod;
		sto_uretici_kodu = _sto_uretici_kodu;
		sto_reyon_kodu = _sto_reyon_kodu;
		sto_marka_kodu = _sto_marka_kodu;
		sto_ver_sip_birim = 1;
		ekleme_bilgileri = new StokEklemeBilgileri();
		ekleme_bilgileri.Miktar = 0.0;
		switch (toptan_perakende)
		{
		case enum_toptan_perakende.Perakende:
			ekleme_bilgileri.vergi_pntr = sto_perakende_vergi;
			break;
		case enum_toptan_perakende.Toptan:
			ekleme_bilgileri.vergi_pntr = sto_toptan_vergi;
			break;
		}
	}

	public StokListItemFiyatveMiktarli(Guid _sto_Guid, string _sto_kod, string _sto_isim, string _sto_yabanci_isim, string _sto_kisa_ismi, int _sto_perakende_vergi, int _sto_toptan_vergi, string _sto_birim1_ad, double _sto_birim1_katsayi, string _sto_birim2_ad, double _sto_birim2_katsayi, string _sto_birim3_ad, double _sto_birim3_katsayi, string _sto_birim4_ad, double _sto_birim4_katsayi, int _sto_al_sip_birim, string _sto_anagrup_kod, string _sto_altgrup_kod, string _sto_uretici_kodu, string _sto_reyon_kodu, string _sto_marka_kodu, enum_toptan_perakende toptan_perakende)
	{
		sto_Guid = _sto_Guid;
		sto_kod = _sto_kod;
		sto_isim = _sto_isim;
		sto_yabanci_isim = _sto_yabanci_isim;
		sto_kisa_ismi = _sto_kisa_ismi;
		sto_perakende_vergi = _sto_perakende_vergi;
		sto_toptan_vergi = _sto_toptan_vergi;
		sto_birim1_ad = _sto_birim1_ad;
		sto_birim1_katsayi = _sto_birim1_katsayi;
		sto_birim2_ad = _sto_birim2_ad;
		sto_birim2_katsayi = _sto_birim2_katsayi;
		sto_birim3_ad = _sto_birim3_ad;
		sto_birim3_katsayi = _sto_birim3_katsayi;
		sto_birim4_ad = _sto_birim4_ad;
		sto_birim4_katsayi = _sto_birim4_katsayi;
		sto_al_sip_birim = _sto_al_sip_birim;
		sto_anagrup_kod = _sto_anagrup_kod;
		sto_altgrup_kod = _sto_altgrup_kod;
		sto_uretici_kodu = _sto_uretici_kodu;
		sto_reyon_kodu = _sto_reyon_kodu;
		sto_marka_kodu = _sto_marka_kodu;
		sto_ver_sip_birim = 1;
		ekleme_bilgileri = new StokEklemeBilgileri();
		ekleme_bilgileri.Miktar = 0.0;
		switch (toptan_perakende)
		{
		case enum_toptan_perakende.Perakende:
			ekleme_bilgileri.vergi_pntr = sto_perakende_vergi;
			break;
		case enum_toptan_perakende.Toptan:
			ekleme_bilgileri.vergi_pntr = sto_toptan_vergi;
			break;
		}
	}

	public double KdvTutariToplamNet()
	{
		return ekleme_bilgileri.Miktar * ekleme_bilgileri.BirimFiyat.FiyatNetMasrafsiz / 100.0 * AppBase._vergitanimlari[ekleme_bilgileri.vergi_pntr].Yuzde + ekleme_bilgileri.Miktar * ekleme_bilgileri.BirimFiyat.MasrafTutariToplam / 100.0 * 18.0;
	}

	public double KdvTutariToplamNetMasrafsiz()
	{
		return ekleme_bilgileri.Miktar * ekleme_bilgileri.BirimFiyat.FiyatNetMasrafsiz / 100.0 * AppBase._vergitanimlari[ekleme_bilgileri.vergi_pntr].Yuzde;
	}

	public double KdvTutariToplamBrut()
	{
		return ekleme_bilgileri.Miktar * ekleme_bilgileri.BirimFiyat.FiyatBrut / 100.0 * AppBase._vergitanimlari[ekleme_bilgileri.vergi_pntr].Yuzde;
	}

	public double KdvTutariBirimNet()
	{
		return ekleme_bilgileri.BirimFiyat.FiyatNetMasrafsiz / 100.0 * AppBase._vergitanimlari[ekleme_bilgileri.vergi_pntr].Yuzde + ekleme_bilgileri.BirimFiyat.MasrafTutariToplam / 100.0 * 18.0;
	}

	public double KdvTutariBirimBrut()
	{
		return ekleme_bilgileri.BirimFiyat.FiyatBrut / 100.0 * AppBase._vergitanimlari[ekleme_bilgileri.vergi_pntr].Yuzde;
	}

	public static void WriteToStream(StokListItemFiyatveMiktarli toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToStream(toWrite, writer);
	}

	public static void WriteToStream(StokListItemFiyatveMiktarli toWrite, BinaryWriter writer)
	{
		try
		{
			int value = 1;
			writer.Write(value);
			writer.Write(toWrite.sto_RECno);
			writer.Write(toWrite.sto_Guid.ToByteArray());
			writer.Write(toWrite.sto_kod);
			writer.Write(toWrite.sto_isim);
			writer.Write(toWrite.sto_yabanci_isim);
			writer.Write(toWrite.sto_kisa_ismi);
			writer.Write(toWrite.sto_perakende_vergi);
			writer.Write(toWrite.sto_toptan_vergi);
			writer.Write(toWrite.sto_birim1_ad);
			writer.Write(toWrite.sto_birim1_katsayi);
			writer.Write(toWrite.sto_birim2_ad);
			writer.Write(toWrite.sto_birim2_katsayi);
			writer.Write(toWrite.sto_birim3_ad);
			writer.Write(toWrite.sto_birim3_katsayi);
			writer.Write(toWrite.sto_birim4_ad);
			writer.Write(toWrite.sto_birim4_katsayi);
			writer.Write(toWrite.sto_ver_sip_birim);
			StokEklemeBilgileri.WriteToBinaryWriter(toWrite.ekleme_bilgileri, writer, 999);
		}
		catch
		{
		}
	}

	public static StokListItemFiyatveMiktarli ReadFromStream(Stream from)
	{
		return ReadFromStream(new BinaryReader(from));
	}

	public static StokListItemFiyatveMiktarli ReadFromStream(BinaryReader reader)
	{
		StokListItemFiyatveMiktarli stokListItemFiyatveMiktarli = new StokListItemFiyatveMiktarli();
		try
		{
			reader.ReadInt32();
			stokListItemFiyatveMiktarli.sto_RECno = reader.ReadInt32();
			stokListItemFiyatveMiktarli.sto_Guid = new Guid(reader.ReadBytes(16));
			stokListItemFiyatveMiktarli.sto_kod = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_isim = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_yabanci_isim = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_kisa_ismi = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_perakende_vergi = reader.ReadInt32();
			stokListItemFiyatveMiktarli.sto_toptan_vergi = reader.ReadInt32();
			stokListItemFiyatveMiktarli.sto_birim1_ad = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_birim1_katsayi = reader.ReadDouble();
			stokListItemFiyatveMiktarli.sto_birim2_ad = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_birim2_katsayi = reader.ReadDouble();
			stokListItemFiyatveMiktarli.sto_birim3_ad = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_birim3_katsayi = reader.ReadDouble();
			stokListItemFiyatveMiktarli.sto_birim4_ad = reader.ReadString();
			stokListItemFiyatveMiktarli.sto_birim4_katsayi = reader.ReadDouble();
			stokListItemFiyatveMiktarli.sto_ver_sip_birim = reader.ReadInt32();
			stokListItemFiyatveMiktarli.ekleme_bilgileri = StokEklemeBilgileri.ReadFromBinaryReader(reader);
		}
		catch
		{
		}
		return stokListItemFiyatveMiktarli;
	}
}
