using System.IO;

namespace Fora.Mikro.Stoklar;

public class Stok
{
	public string sto_kod { get; set; }

	public string sto_isim { get; set; }

	public int sto_perakende_vergi_yeni { get; set; }

	public int sto_toptan_vergi_yeni { get; set; }

	public string sto_kisa_ismi { get; set; }

	public string sto_yabanci_isim { get; set; }

	public string sto_sat_cari_kod { get; set; }

	public int sto_cins { get; set; }

	public int sto_doviz_cinsi { get; set; }

	public int sto_detay_takip { get; set; }

	public string sto_birim1_ad { get; set; }

	public double sto_birim1_katsayi { get; set; }

	public string sto_birim2_ad { get; set; }

	public double sto_birim2_katsayi { get; set; }

	public string sto_birim3_ad { get; set; }

	public double sto_birim3_katsayi { get; set; }

	public string sto_birim4_ad { get; set; }

	public double sto_birim4_katsayi { get; set; }

	public bool sto_bedenli_takip { get; set; }

	public bool sto_renkDetayli { get; set; }

	public string sto_beden_kodu { get; set; }

	public string sto_renk_kodu { get; set; }

	public string sto_altgrup_kod { get; set; }

	public string sto_anagrup_kod { get; set; }

	public string sto_sektor_kodu { get; set; }

	public string sto_marka_kodu { get; set; }

	public string sto_model_kodu { get; set; }

	public string sto_uretici_kodu { get; set; }

	public string sto_reyon_kodu { get; set; }

	public double sto_standartmaliyet { get; set; }

	public double sto_birim1_agirlik { get; set; }

	public double sto_birim1_en { get; set; }

	public double sto_birim1_boy { get; set; }

	public double sto_birim1_yukseklik { get; set; }

	public double sto_birim1_dara { get; set; }

	public double sto_birim2_agirlik { get; set; }

	public double sto_birim2_en { get; set; }

	public double sto_birim2_boy { get; set; }

	public double sto_birim2_yukseklik { get; set; }

	public double sto_birim2_dara { get; set; }

	public double sto_birim3_agirlik { get; set; }

	public double sto_birim3_en { get; set; }

	public double sto_birim3_boy { get; set; }

	public double sto_birim3_yukseklik { get; set; }

	public double sto_birim3_dara { get; set; }

	public double sto_birim4_agirlik { get; set; }

	public double sto_birim4_en { get; set; }

	public double sto_birim4_boy { get; set; }

	public double sto_birim4_yukseklik { get; set; }

	public double sto_birim4_dara { get; set; }

	public string sto_muh_kod { get; set; }

	public string sto_muh_Iade_kod { get; set; }

	public string sto_muh_sat_muh_kod { get; set; }

	public string sto_muh_satIadmuhkod { get; set; }

	public string sto_muh_sat_isk_kod { get; set; }

	public string sto_muh_aIiskmuhkod { get; set; }

	public string sto_muh_satmalmuhkod { get; set; }

	public string sto_yurtdisi_satmuhk { get; set; }

	public string sto_ilavemasmuhkod { get; set; }

	public string sto_yatirimtesmuhkod { get; set; }

	public string sto_depsatmuhkod { get; set; }

	public string sto_depsatmalmuhkod { get; set; }

	public string sto_bagortsatmuhkod { get; set; }

	public string sto_bagortsatIadmuhkod { get; set; }

	public string sto_bagortsatIskmuhkod { get; set; }

	public string sto_satfiyfarkmuhkod { get; set; }

	public string sto_yurtdisisatmalmuhkod { get; set; }

	public string sto_bagortsatmalmuhkod { get; set; }

	public double sto_karorani { get; set; }

	public double sto_min_stok { get; set; }

	public double sto_siparis_stok { get; set; }

	public double sto_max_stok { get; set; }

	public int sto_ver_sip_birim { get; set; }

	public int sto_al_sip_birim { get; set; }

	public int sto_siparis_sure { get; set; }

	public string sto_yer_kod { get; set; }

	public int sto_elk_etk_tipi { get; set; }

	public int sto_raf_etiketli { get; set; }

	public bool sto_satis_dursun { get; set; }

	public bool sto_siparis_dursun { get; set; }

	public bool sto_malkabul_dursun { get; set; }

	public bool sto_iskon_yapilamaz { get; set; }

	public string sto_kategori_kodu { get; set; }

	public string sto_urun_sorkod { get; set; }

	public string sto_muhgrup_kodu { get; set; }

	public string sto_ambalaj_kodu { get; set; }

	public string sto_sezon_kodu { get; set; }

	public string sto_hammadde_kodu { get; set; }

	public string sto_prim_kodu { get; set; }

	public string sto_mkod_artik { get; set; }

	public bool sto_eksiyedusebilir_fl { get; set; }

	public int sto_otvuygulama { get; set; }

	public double sto_otvtutar { get; set; }

	public int sto_otvliste { get; set; }

	public double sto_prim_orani { get; set; }

	public int sto_garanti_sure { get; set; }

	public int sto_garanti_sure_tipi { get; set; }

	public int sto_oivuygulama { get; set; }

	public double sto_maxiskonto_orani { get; set; }

	public double sto_oivtutar { get; set; }

	public int sto_oivvergipntr { get; set; }

	public bool sto_webe_gonderilecek_fl { get; set; }

	public string sto_special1 { get; set; }

	public string sto_special2 { get; set; }

	public string sto_special3 { get; set; }

	public StokEklemeBilgileri ekleme_bilgileri { get; set; }

	public Stok()
	{
		sto_isim = "";
		sto_kisa_ismi = "";
		sto_yabanci_isim = "";
		sto_sat_cari_kod = "";
		sto_birim1_ad = "";
		sto_birim2_ad = "";
		sto_birim3_ad = "";
		sto_birim4_ad = "";
		sto_beden_kodu = "";
		sto_renk_kodu = "";
		sto_altgrup_kod = "";
		sto_anagrup_kod = "";
		sto_sektor_kodu = "";
		sto_marka_kodu = "";
		sto_model_kodu = "";
		sto_uretici_kodu = "";
		sto_reyon_kodu = "";
		sto_muh_kod = "";
		sto_muh_Iade_kod = "";
		sto_muh_sat_muh_kod = "";
		sto_muh_satIadmuhkod = "";
		sto_muh_sat_isk_kod = "";
		sto_muh_aIiskmuhkod = "";
		sto_muh_satmalmuhkod = "";
		sto_yurtdisi_satmuhk = "";
		sto_ilavemasmuhkod = "";
		sto_yatirimtesmuhkod = "";
		sto_depsatmuhkod = "";
		sto_depsatmalmuhkod = "";
		sto_bagortsatmuhkod = "";
		sto_bagortsatIadmuhkod = "";
		sto_bagortsatIskmuhkod = "";
		sto_satfiyfarkmuhkod = "";
		sto_yurtdisisatmalmuhkod = "";
		sto_bagortsatmalmuhkod = "";
		sto_yer_kod = "";
		sto_kategori_kodu = "";
		sto_urun_sorkod = "";
		sto_muhgrup_kodu = "";
		sto_ambalaj_kodu = "";
		sto_sezon_kodu = "";
		sto_hammadde_kodu = "";
		sto_prim_kodu = "";
		sto_mkod_artik = "";
		sto_special1 = "";
		sto_special2 = "";
		sto_special3 = "";
		ekleme_bilgileri = new StokEklemeBilgileri();
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

	public static byte[] WriteToByteArray(Stok toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Stok toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Stok toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 3;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sto_kod);
		writer.Write(toWrite.sto_isim);
		writer.Write(toWrite.sto_perakende_vergi_yeni);
		writer.Write(toWrite.sto_toptan_vergi_yeni);
		writer.Write(toWrite.sto_kisa_ismi);
		writer.Write(toWrite.sto_yabanci_isim);
		writer.Write(toWrite.sto_sat_cari_kod);
		writer.Write(toWrite.sto_cins);
		writer.Write(toWrite.sto_doviz_cinsi);
		writer.Write(toWrite.sto_detay_takip);
		writer.Write(toWrite.sto_birim1_ad);
		writer.Write(toWrite.sto_birim1_katsayi);
		writer.Write(toWrite.sto_birim2_ad);
		writer.Write(toWrite.sto_birim2_katsayi);
		writer.Write(toWrite.sto_birim3_ad);
		writer.Write(toWrite.sto_birim3_katsayi);
		writer.Write(toWrite.sto_birim4_ad);
		writer.Write(toWrite.sto_birim4_katsayi);
		writer.Write(toWrite.sto_bedenli_takip);
		writer.Write(toWrite.sto_renkDetayli);
		writer.Write(toWrite.sto_beden_kodu);
		writer.Write(toWrite.sto_renk_kodu);
		writer.Write(toWrite.sto_altgrup_kod);
		writer.Write(toWrite.sto_anagrup_kod);
		writer.Write(toWrite.sto_sektor_kodu);
		writer.Write(toWrite.sto_marka_kodu);
		writer.Write(toWrite.sto_model_kodu);
		writer.Write(toWrite.sto_uretici_kodu);
		writer.Write(toWrite.sto_reyon_kodu);
		writer.Write(toWrite.sto_standartmaliyet);
		writer.Write(toWrite.sto_birim1_agirlik);
		writer.Write(toWrite.sto_birim1_en);
		writer.Write(toWrite.sto_birim1_boy);
		writer.Write(toWrite.sto_birim1_yukseklik);
		writer.Write(toWrite.sto_birim1_dara);
		writer.Write(toWrite.sto_birim2_agirlik);
		writer.Write(toWrite.sto_birim2_en);
		writer.Write(toWrite.sto_birim2_boy);
		writer.Write(toWrite.sto_birim2_yukseklik);
		writer.Write(toWrite.sto_birim2_dara);
		writer.Write(toWrite.sto_birim3_agirlik);
		writer.Write(toWrite.sto_birim3_en);
		writer.Write(toWrite.sto_birim3_boy);
		writer.Write(toWrite.sto_birim3_yukseklik);
		writer.Write(toWrite.sto_birim3_dara);
		writer.Write(toWrite.sto_birim4_agirlik);
		writer.Write(toWrite.sto_birim4_en);
		writer.Write(toWrite.sto_birim4_boy);
		writer.Write(toWrite.sto_birim4_yukseklik);
		writer.Write(toWrite.sto_birim4_dara);
		writer.Write(toWrite.sto_muh_kod);
		writer.Write(toWrite.sto_muh_Iade_kod);
		writer.Write(toWrite.sto_muh_sat_muh_kod);
		writer.Write(toWrite.sto_muh_satIadmuhkod);
		writer.Write(toWrite.sto_muh_sat_isk_kod);
		writer.Write(toWrite.sto_muh_aIiskmuhkod);
		writer.Write(toWrite.sto_muh_satmalmuhkod);
		writer.Write(toWrite.sto_yurtdisi_satmuhk);
		writer.Write(toWrite.sto_ilavemasmuhkod);
		writer.Write(toWrite.sto_yatirimtesmuhkod);
		writer.Write(toWrite.sto_depsatmuhkod);
		writer.Write(toWrite.sto_depsatmalmuhkod);
		writer.Write(toWrite.sto_bagortsatmuhkod);
		writer.Write(toWrite.sto_bagortsatIadmuhkod);
		writer.Write(toWrite.sto_bagortsatIskmuhkod);
		writer.Write(toWrite.sto_satfiyfarkmuhkod);
		writer.Write(toWrite.sto_yurtdisisatmalmuhkod);
		writer.Write(toWrite.sto_bagortsatmalmuhkod);
		writer.Write(toWrite.sto_karorani);
		writer.Write(toWrite.sto_min_stok);
		writer.Write(toWrite.sto_siparis_stok);
		writer.Write(toWrite.sto_max_stok);
		writer.Write(toWrite.sto_ver_sip_birim);
		writer.Write(toWrite.sto_al_sip_birim);
		writer.Write(toWrite.sto_siparis_sure);
		writer.Write(toWrite.sto_yer_kod);
		writer.Write(toWrite.sto_elk_etk_tipi);
		writer.Write(toWrite.sto_raf_etiketli);
		writer.Write(toWrite.sto_satis_dursun);
		writer.Write(toWrite.sto_siparis_dursun);
		writer.Write(toWrite.sto_malkabul_dursun);
		writer.Write(toWrite.sto_iskon_yapilamaz);
		writer.Write(toWrite.sto_kategori_kodu);
		writer.Write(toWrite.sto_urun_sorkod);
		writer.Write(toWrite.sto_muhgrup_kodu);
		writer.Write(toWrite.sto_ambalaj_kodu);
		writer.Write(toWrite.sto_sezon_kodu);
		writer.Write(toWrite.sto_hammadde_kodu);
		writer.Write(toWrite.sto_prim_kodu);
		writer.Write(toWrite.sto_mkod_artik);
		writer.Write(toWrite.sto_eksiyedusebilir_fl);
		writer.Write(toWrite.sto_otvuygulama);
		writer.Write(toWrite.sto_otvtutar);
		writer.Write(toWrite.sto_otvliste);
		writer.Write(toWrite.sto_prim_orani);
		writer.Write(toWrite.sto_garanti_sure);
		writer.Write(toWrite.sto_garanti_sure_tipi);
		writer.Write(toWrite.sto_oivuygulama);
		writer.Write(toWrite.sto_maxiskonto_orani);
		writer.Write(toWrite.sto_oivtutar);
		writer.Write(toWrite.sto_oivvergipntr);
		writer.Write(toWrite.sto_webe_gonderilecek_fl);
		writer.Write(toWrite.sto_special1);
		writer.Write(toWrite.sto_special2);
		writer.Write(toWrite.sto_special3);
		if (num == 1)
		{
			StokEklemeBilgileri.WriteToBinaryWriter(toWrite.ekleme_bilgileri, writer, 1);
		}
		if (num == 2)
		{
			StokEklemeBilgileri.WriteToBinaryWriter(toWrite.ekleme_bilgileri, writer, 2);
		}
		if (num > 2)
		{
			StokEklemeBilgileri.WriteToBinaryWriter(toWrite.ekleme_bilgileri, writer, 3);
		}
	}

	public static Stok ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Stok result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Stok ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Stok ReadFromBinaryReader(BinaryReader reader)
	{
		Stok stok = new Stok();
		reader.ReadInt32();
		stok.sto_kod = reader.ReadString();
		stok.sto_isim = reader.ReadString();
		stok.sto_perakende_vergi_yeni = reader.ReadInt32();
		stok.sto_toptan_vergi_yeni = reader.ReadInt32();
		stok.sto_kisa_ismi = reader.ReadString();
		stok.sto_yabanci_isim = reader.ReadString();
		stok.sto_sat_cari_kod = reader.ReadString();
		stok.sto_cins = reader.ReadInt32();
		stok.sto_doviz_cinsi = reader.ReadInt32();
		stok.sto_detay_takip = reader.ReadInt32();
		stok.sto_birim1_ad = reader.ReadString();
		stok.sto_birim1_katsayi = reader.ReadDouble();
		stok.sto_birim2_ad = reader.ReadString();
		stok.sto_birim2_katsayi = reader.ReadDouble();
		stok.sto_birim3_ad = reader.ReadString();
		stok.sto_birim3_katsayi = reader.ReadDouble();
		stok.sto_birim4_ad = reader.ReadString();
		stok.sto_birim4_katsayi = reader.ReadDouble();
		stok.sto_bedenli_takip = reader.ReadBoolean();
		stok.sto_renkDetayli = reader.ReadBoolean();
		stok.sto_beden_kodu = reader.ReadString();
		stok.sto_renk_kodu = reader.ReadString();
		stok.sto_altgrup_kod = reader.ReadString();
		stok.sto_anagrup_kod = reader.ReadString();
		stok.sto_sektor_kodu = reader.ReadString();
		stok.sto_marka_kodu = reader.ReadString();
		stok.sto_model_kodu = reader.ReadString();
		stok.sto_uretici_kodu = reader.ReadString();
		stok.sto_reyon_kodu = reader.ReadString();
		stok.sto_standartmaliyet = reader.ReadDouble();
		stok.sto_birim1_agirlik = reader.ReadDouble();
		stok.sto_birim1_en = reader.ReadDouble();
		stok.sto_birim1_boy = reader.ReadDouble();
		stok.sto_birim1_yukseklik = reader.ReadDouble();
		stok.sto_birim1_dara = reader.ReadDouble();
		stok.sto_birim2_agirlik = reader.ReadDouble();
		stok.sto_birim2_en = reader.ReadDouble();
		stok.sto_birim2_boy = reader.ReadDouble();
		stok.sto_birim2_yukseklik = reader.ReadDouble();
		stok.sto_birim2_dara = reader.ReadDouble();
		stok.sto_birim3_agirlik = reader.ReadDouble();
		stok.sto_birim3_en = reader.ReadDouble();
		stok.sto_birim3_boy = reader.ReadDouble();
		stok.sto_birim3_yukseklik = reader.ReadDouble();
		stok.sto_birim3_dara = reader.ReadDouble();
		stok.sto_birim4_agirlik = reader.ReadDouble();
		stok.sto_birim4_en = reader.ReadDouble();
		stok.sto_birim4_boy = reader.ReadDouble();
		stok.sto_birim4_yukseklik = reader.ReadDouble();
		stok.sto_birim4_dara = reader.ReadDouble();
		stok.sto_muh_kod = reader.ReadString();
		stok.sto_muh_Iade_kod = reader.ReadString();
		stok.sto_muh_sat_muh_kod = reader.ReadString();
		stok.sto_muh_satIadmuhkod = reader.ReadString();
		stok.sto_muh_sat_isk_kod = reader.ReadString();
		stok.sto_muh_aIiskmuhkod = reader.ReadString();
		stok.sto_muh_satmalmuhkod = reader.ReadString();
		stok.sto_yurtdisi_satmuhk = reader.ReadString();
		stok.sto_ilavemasmuhkod = reader.ReadString();
		stok.sto_yatirimtesmuhkod = reader.ReadString();
		stok.sto_depsatmuhkod = reader.ReadString();
		stok.sto_depsatmalmuhkod = reader.ReadString();
		stok.sto_bagortsatmuhkod = reader.ReadString();
		stok.sto_bagortsatIadmuhkod = reader.ReadString();
		stok.sto_bagortsatIskmuhkod = reader.ReadString();
		stok.sto_satfiyfarkmuhkod = reader.ReadString();
		stok.sto_yurtdisisatmalmuhkod = reader.ReadString();
		stok.sto_bagortsatmalmuhkod = reader.ReadString();
		stok.sto_karorani = reader.ReadDouble();
		stok.sto_min_stok = reader.ReadDouble();
		stok.sto_siparis_stok = reader.ReadDouble();
		stok.sto_max_stok = reader.ReadDouble();
		stok.sto_ver_sip_birim = reader.ReadInt32();
		stok.sto_al_sip_birim = reader.ReadInt32();
		stok.sto_siparis_sure = reader.ReadInt32();
		stok.sto_yer_kod = reader.ReadString();
		stok.sto_elk_etk_tipi = reader.ReadInt32();
		stok.sto_raf_etiketli = reader.ReadInt32();
		stok.sto_satis_dursun = reader.ReadBoolean();
		stok.sto_siparis_dursun = reader.ReadBoolean();
		stok.sto_malkabul_dursun = reader.ReadBoolean();
		stok.sto_iskon_yapilamaz = reader.ReadBoolean();
		stok.sto_kategori_kodu = reader.ReadString();
		stok.sto_urun_sorkod = reader.ReadString();
		stok.sto_muhgrup_kodu = reader.ReadString();
		stok.sto_ambalaj_kodu = reader.ReadString();
		stok.sto_sezon_kodu = reader.ReadString();
		stok.sto_hammadde_kodu = reader.ReadString();
		stok.sto_prim_kodu = reader.ReadString();
		stok.sto_mkod_artik = reader.ReadString();
		stok.sto_eksiyedusebilir_fl = reader.ReadBoolean();
		stok.sto_otvuygulama = reader.ReadInt32();
		stok.sto_otvtutar = reader.ReadDouble();
		stok.sto_otvliste = reader.ReadInt32();
		stok.sto_prim_orani = reader.ReadDouble();
		stok.sto_garanti_sure = reader.ReadInt32();
		stok.sto_garanti_sure_tipi = reader.ReadInt32();
		stok.sto_oivuygulama = reader.ReadInt32();
		stok.sto_maxiskonto_orani = reader.ReadDouble();
		stok.sto_oivtutar = reader.ReadDouble();
		stok.sto_oivvergipntr = reader.ReadInt32();
		stok.sto_webe_gonderilecek_fl = reader.ReadBoolean();
		stok.sto_special1 = reader.ReadString();
		stok.sto_special2 = reader.ReadString();
		stok.sto_special3 = reader.ReadString();
		stok.ekleme_bilgileri = StokEklemeBilgileri.ReadFromBinaryReader(reader);
		return stok;
	}
}
