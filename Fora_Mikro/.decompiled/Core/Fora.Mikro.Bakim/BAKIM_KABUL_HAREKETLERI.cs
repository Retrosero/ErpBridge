using System;
using System.Collections.Generic;
using System.IO;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Bakim;

public class BAKIM_KABUL_HAREKETLERI
{
	public int bkmkb_RECno { get; set; }

	public int bkmkb_RECid_DBCno { get; set; }

	public int bkmkb_RECid_RECno { get; set; }

	public int bkmkb_Spec_Rec_no { get; set; }

	public bool bkmkb_iptal { get; set; }

	public int bkmkb_fileid { get; set; }

	public bool bkmkb_hidden { get; set; }

	public bool bkmkb_kilitli { get; set; }

	public bool bkmkb_degisti { get; set; }

	public int bkmkb_checksum { get; set; }

	public int bkmkb_create_user { get; set; }

	public DateTime bkmkb_create_date { get; set; }

	public int bkmkb_lastup_user { get; set; }

	public DateTime bkmkb_lastup_date { get; set; }

	public string bkmkb_special1 { get; set; }

	public string bkmkb_special2 { get; set; }

	public string bkmkb_special3 { get; set; }

	public int bkmkb_firmano { get; set; }

	public int bkmkb_subeno { get; set; }

	public DateTime bkmkb_tarihi { get; set; }

	public string bkmkb_evrakno_seri { get; set; }

	public int bkmkb_evrakno_sira { get; set; }

	public int bkmkb_satirno { get; set; }

	public string bkmkb_belgeno { get; set; }

	public DateTime bkmkb_belge_tarihi { get; set; }

	public string bkmkb_cihaz_serino { get; set; }

	public string bkmkb_fis_stok_kodu { get; set; }

	public string bkmkb_tuketici_kodu { get; set; }

	public int bkmkb_talep_gelis_sekli { get; set; }

	public string bkmkb_gelis_kargo_kodu { get; set; }

	public string bkmkb_gelis_kargo_belgeno { get; set; }

	public string bkmkb_gelis_irsaliyeno { get; set; }

	public int bkmkb_servis_turu { get; set; }

	public int bkmkb_servis_yeri { get; set; }

	public string bkmkb_aksesuarlar { get; set; }

	public string bkmkb_bildirilen_arizalar { get; set; }

	public DateTime bkmkb_teslim_alinma_tarihi { get; set; }

	public DateTime bkmkb_teslim_edilme_tarihi { get; set; }

	public int bkmkb_teslim_edilme_sekli { get; set; }

	public string bkmkb_ariza_kodu1 { get; set; }

	public string bkmkb_ariza_kodu2 { get; set; }

	public string bkmkb_ariza_kodu3 { get; set; }

	public string bkmkb_ariza_kodu4 { get; set; }

	public string bkmkb_ariza_kodu5 { get; set; }

	public string bkmkb_ariza_kodu6 { get; set; }

	public string bkmkb_ariza_kodu7 { get; set; }

	public string bkmkb_ariza_kodu8 { get; set; }

	public string bkmkb_ariza_kodu9 { get; set; }

	public string bkmkb_ariza_kodu10 { get; set; }

	public int bkmkb_bilgilendirme_sekli { get; set; }

	public string bkmkb_inceleyecek_ekip_kodu { get; set; }

	public int bkmkb_depono { get; set; }

	public string bkmkb_aciklama { get; set; }

	public int bkmkb_hareket_tipi { get; set; }

	public string bkmkb_stok_hizmet_kodu { get; set; }

	public int bkmkb_operasyon_suresi { get; set; }

	public double bkmkb_miktari { get; set; }

	public string bkmkb_satir_aciklama { get; set; }

	public bool bkmkb_planlandi_fl { get; set; }

	public int bkmkb_adres_no { get; set; }

	public string TuketiciAdi { get; set; }

	public string ArizaAdi1 { get; set; }

	public string EkipAdi { get; set; }

	public List<GenelList> Ekipler { get; set; }

	public List<StokBase> KullanilanStoklar { get; set; }

	public string Aciklama1 { get; set; }

	public string Aciklama2 { get; set; }

	public string Aciklama3 { get; set; }

	public string GetEvrakSeriSira
	{
		get
		{
			if (bkmkb_evrakno_seri != "")
			{
				return bkmkb_evrakno_seri + "-" + bkmkb_evrakno_sira;
			}
			return bkmkb_evrakno_sira.ToString();
		}
	}

	public BAKIM_KABUL_HAREKETLERI()
	{
		bkmkb_create_date = DateTime.Now;
		bkmkb_lastup_date = DateTime.Now;
		bkmkb_tarihi = DateTime.Now;
		bkmkb_belge_tarihi = DateTime.Now;
		bkmkb_teslim_alinma_tarihi = DateTime.Now;
		bkmkb_teslim_edilme_tarihi = DateTime.Now;
		bkmkb_special1 = "";
		bkmkb_special2 = "";
		bkmkb_special3 = "";
		bkmkb_evrakno_seri = "";
		bkmkb_belgeno = "";
		bkmkb_cihaz_serino = "";
		bkmkb_fis_stok_kodu = "";
		bkmkb_tuketici_kodu = "";
		bkmkb_gelis_kargo_kodu = "";
		bkmkb_gelis_kargo_belgeno = "";
		bkmkb_gelis_irsaliyeno = "";
		bkmkb_aksesuarlar = "";
		bkmkb_bildirilen_arizalar = "";
		bkmkb_ariza_kodu1 = "";
		bkmkb_ariza_kodu2 = "";
		bkmkb_ariza_kodu3 = "";
		bkmkb_ariza_kodu4 = "";
		bkmkb_ariza_kodu5 = "";
		bkmkb_ariza_kodu6 = "";
		bkmkb_ariza_kodu7 = "";
		bkmkb_ariza_kodu8 = "";
		bkmkb_ariza_kodu9 = "";
		bkmkb_ariza_kodu10 = "";
		bkmkb_inceleyecek_ekip_kodu = "";
		bkmkb_aciklama = "";
		bkmkb_stok_hizmet_kodu = "";
		bkmkb_satir_aciklama = "";
		TuketiciAdi = "";
		ArizaAdi1 = "";
		EkipAdi = "";
		Aciklama1 = "";
		Aciklama2 = "";
		Aciklama3 = "";
		Ekipler = new List<GenelList>();
		KullanilanStoklar = new List<StokBase>();
	}

	public static byte[] WriteToByteArray(BAKIM_KABUL_HAREKETLERI toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(BAKIM_KABUL_HAREKETLERI toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(BAKIM_KABUL_HAREKETLERI toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.bkmkb_RECno);
		writer.Write(toWrite.bkmkb_RECid_DBCno);
		writer.Write(toWrite.bkmkb_RECid_RECno);
		writer.Write(toWrite.bkmkb_Spec_Rec_no);
		writer.Write(toWrite.bkmkb_iptal);
		writer.Write(toWrite.bkmkb_fileid);
		writer.Write(toWrite.bkmkb_hidden);
		writer.Write(toWrite.bkmkb_kilitli);
		writer.Write(toWrite.bkmkb_degisti);
		writer.Write(toWrite.bkmkb_checksum);
		writer.Write(toWrite.bkmkb_create_user);
		writer.Write(toWrite.bkmkb_create_date.Ticks);
		writer.Write(toWrite.bkmkb_lastup_user);
		writer.Write(toWrite.bkmkb_lastup_date.Ticks);
		writer.Write(toWrite.bkmkb_special1);
		writer.Write(toWrite.bkmkb_special2);
		writer.Write(toWrite.bkmkb_special3);
		writer.Write(toWrite.bkmkb_firmano);
		writer.Write(toWrite.bkmkb_subeno);
		writer.Write(toWrite.bkmkb_tarihi.Ticks);
		writer.Write(toWrite.bkmkb_evrakno_seri);
		writer.Write(toWrite.bkmkb_evrakno_sira);
		writer.Write(toWrite.bkmkb_satirno);
		writer.Write(toWrite.bkmkb_belgeno);
		writer.Write(toWrite.bkmkb_belge_tarihi.Ticks);
		writer.Write(toWrite.bkmkb_cihaz_serino);
		writer.Write(toWrite.bkmkb_fis_stok_kodu);
		writer.Write(toWrite.bkmkb_tuketici_kodu);
		writer.Write(toWrite.bkmkb_talep_gelis_sekli);
		writer.Write(toWrite.bkmkb_gelis_kargo_kodu);
		writer.Write(toWrite.bkmkb_gelis_kargo_belgeno);
		writer.Write(toWrite.bkmkb_gelis_irsaliyeno);
		writer.Write(toWrite.bkmkb_servis_turu);
		writer.Write(toWrite.bkmkb_servis_yeri);
		writer.Write(toWrite.bkmkb_aksesuarlar);
		writer.Write(toWrite.bkmkb_bildirilen_arizalar);
		writer.Write(toWrite.bkmkb_teslim_alinma_tarihi.Ticks);
		writer.Write(toWrite.bkmkb_teslim_edilme_tarihi.Ticks);
		writer.Write(toWrite.bkmkb_teslim_edilme_sekli);
		writer.Write(toWrite.bkmkb_ariza_kodu1);
		writer.Write(toWrite.bkmkb_ariza_kodu2);
		writer.Write(toWrite.bkmkb_ariza_kodu3);
		writer.Write(toWrite.bkmkb_ariza_kodu4);
		writer.Write(toWrite.bkmkb_ariza_kodu5);
		writer.Write(toWrite.bkmkb_ariza_kodu6);
		writer.Write(toWrite.bkmkb_ariza_kodu7);
		writer.Write(toWrite.bkmkb_ariza_kodu8);
		writer.Write(toWrite.bkmkb_ariza_kodu9);
		writer.Write(toWrite.bkmkb_ariza_kodu10);
		writer.Write(toWrite.bkmkb_bilgilendirme_sekli);
		writer.Write(toWrite.bkmkb_inceleyecek_ekip_kodu);
		writer.Write(toWrite.bkmkb_depono);
		writer.Write(toWrite.bkmkb_aciklama);
		writer.Write(toWrite.bkmkb_hareket_tipi);
		writer.Write(toWrite.bkmkb_stok_hizmet_kodu);
		writer.Write(toWrite.bkmkb_operasyon_suresi);
		writer.Write(toWrite.bkmkb_miktari);
		writer.Write(toWrite.bkmkb_satir_aciklama);
		writer.Write(toWrite.bkmkb_planlandi_fl);
		writer.Write(toWrite.bkmkb_adres_no);
		writer.Write(toWrite.TuketiciAdi);
		writer.Write(toWrite.ArizaAdi1);
		writer.Write(toWrite.EkipAdi);
		writer.Write(toWrite.Aciklama1);
		writer.Write(toWrite.Aciklama2);
		writer.Write(toWrite.Aciklama3);
		writer.Write(toWrite.Ekipler.Count);
		foreach (GenelList item in toWrite.Ekipler)
		{
			writer.Write(item.Kod);
			writer.Write(item.Text);
		}
		writer.Write(toWrite.KullanilanStoklar.Count);
		foreach (StokBase item2 in toWrite.KullanilanStoklar)
		{
			writer.Write(item2.sto_RECno);
			writer.Write(item2.sto_RECid_RECno);
			writer.Write(item2.sto_kod);
			writer.Write(item2.sto_isim);
			writer.Write(item2.sto_kisa_ismi);
			writer.Write(item2.sto_yabanci_isim);
			writer.Write(item2.sto_birim1_ad);
		}
		_ = 2;
	}

	public static BAKIM_KABUL_HAREKETLERI ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		BAKIM_KABUL_HAREKETLERI result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static BAKIM_KABUL_HAREKETLERI ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static BAKIM_KABUL_HAREKETLERI ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		BAKIM_KABUL_HAREKETLERI bAKIM_KABUL_HAREKETLERI = new BAKIM_KABUL_HAREKETLERI();
		bAKIM_KABUL_HAREKETLERI.bkmkb_RECno = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_RECid_DBCno = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_RECid_RECno = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_Spec_Rec_no = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_iptal = reader.ReadBoolean();
		bAKIM_KABUL_HAREKETLERI.bkmkb_fileid = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_hidden = reader.ReadBoolean();
		bAKIM_KABUL_HAREKETLERI.bkmkb_kilitli = reader.ReadBoolean();
		bAKIM_KABUL_HAREKETLERI.bkmkb_degisti = reader.ReadBoolean();
		bAKIM_KABUL_HAREKETLERI.bkmkb_checksum = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_create_user = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_create_date = new DateTime(reader.ReadInt64());
		bAKIM_KABUL_HAREKETLERI.bkmkb_lastup_user = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_lastup_date = new DateTime(reader.ReadInt64());
		bAKIM_KABUL_HAREKETLERI.bkmkb_special1 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_special2 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_special3 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_firmano = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_subeno = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_tarihi = new DateTime(reader.ReadInt64());
		bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_seri = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_evrakno_sira = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_satirno = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_belgeno = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_belge_tarihi = new DateTime(reader.ReadInt64());
		bAKIM_KABUL_HAREKETLERI.bkmkb_cihaz_serino = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_fis_stok_kodu = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_tuketici_kodu = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_talep_gelis_sekli = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_kargo_kodu = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_kargo_belgeno = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_gelis_irsaliyeno = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_servis_turu = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_servis_yeri = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_aksesuarlar = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_bildirilen_arizalar = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_alinma_tarihi = new DateTime(reader.ReadInt64());
		bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_edilme_tarihi = new DateTime(reader.ReadInt64());
		bAKIM_KABUL_HAREKETLERI.bkmkb_teslim_edilme_sekli = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu1 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu2 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu3 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu4 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu5 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu6 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu7 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu8 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu9 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_ariza_kodu10 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_bilgilendirme_sekli = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_inceleyecek_ekip_kodu = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_depono = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_aciklama = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_hareket_tipi = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_stok_hizmet_kodu = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_operasyon_suresi = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.bkmkb_miktari = reader.ReadDouble();
		bAKIM_KABUL_HAREKETLERI.bkmkb_satir_aciklama = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.bkmkb_planlandi_fl = reader.ReadBoolean();
		bAKIM_KABUL_HAREKETLERI.bkmkb_adres_no = reader.ReadInt32();
		bAKIM_KABUL_HAREKETLERI.TuketiciAdi = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.ArizaAdi1 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.EkipAdi = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.Aciklama1 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.Aciklama2 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.Aciklama3 = reader.ReadString();
		bAKIM_KABUL_HAREKETLERI.Ekipler = new List<GenelList>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			GenelList genelList = new GenelList();
			genelList.Kod = reader.ReadString();
			genelList.Text = reader.ReadString();
			bAKIM_KABUL_HAREKETLERI.Ekipler.Add(genelList);
		}
		bAKIM_KABUL_HAREKETLERI.KullanilanStoklar = new List<StokBase>();
		int num2 = reader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			StokBase stokBase = new StokBase();
			stokBase.sto_RECno = reader.ReadInt32();
			stokBase.sto_RECid_RECno = reader.ReadInt32();
			stokBase.sto_kod = reader.ReadString();
			stokBase.sto_isim = reader.ReadString();
			stokBase.sto_kisa_ismi = reader.ReadString();
			stokBase.sto_yabanci_isim = reader.ReadString();
			stokBase.sto_birim1_ad = reader.ReadString();
			bAKIM_KABUL_HAREKETLERI.KullanilanStoklar.Add(stokBase);
		}
		return bAKIM_KABUL_HAREKETLERI;
	}
}
