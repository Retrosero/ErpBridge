using System;
using System.IO;

namespace Fora.Mikro.CariHesapHareket;

public class CARI_HESAP_HAREKETLERI
{
	public int cha_RECid_DBCno { get; set; }

	public int cha_RECid_RECno { get; set; }

	public int cha_gidkatsoz_recid_dbcno { get; set; }

	public int cha_sip_recid_dbcno { get; set; }

	public int cha_sozlesme_DBCno { get; set; }

	public int cha_ciroprim_DBCno { get; set; }

	public int cha_bakimhar_DBCno { get; set; }

	public int cha_avanstalep_DBCno { get; set; }

	public int cha_RECno { get; set; }

	public Guid cha_Guid { get; set; }

	public int cha_sip_recid_recno { get; set; }

	public Guid cha_sip_uid { get; set; }

	public int cha_gidkatsoz_recid_recno { get; set; }

	public Guid cha_kirahar_uid { get; set; }

	public int cha_sozlesme_RECno { get; set; }

	public int cha_ciroprim_RECno { get; set; }

	public int cha_bakimhar_RECno { get; set; }

	public int cha_avanstalep_RECno { get; set; }

	public int cha_SpecRecNo { get; set; }

	public bool cha_iptal { get; set; }

	public int cha_fileid { get; set; }

	public bool cha_hidden { get; set; }

	public bool cha_kilitli { get; set; }

	public bool cha_degisti { get; set; }

	public int cha_CheckSum { get; set; }

	public int cha_create_user { get; set; }

	public DateTime cha_create_date { get; set; }

	public int cha_lastup_user { get; set; }

	public DateTime cha_lastup_date { get; set; }

	public string cha_special1 { get; set; }

	public string cha_special2 { get; set; }

	public string cha_special3 { get; set; }

	public int cha_firmano { get; set; }

	public int cha_subeno { get; set; }

	public DateTime cha_tarihi { get; set; }

	public enum_cha_tip cha_tip { get; set; }

	public enum_cha_cinsi cha_cinsi { get; set; }

	public enum_cha_normal_Iade cha_normal_Iade { get; set; }

	public enum_cha_evrak_tip cha_evrak_tip { get; set; }

	public int cha_satir_no { get; set; }

	public string cha_evrakno_seri { get; set; }

	public int cha_evrakno_sira { get; set; }

	public string cha_belge_no { get; set; }

	public DateTime cha_belge_tarih { get; set; }

	public enum_cha_cari_cins cha_cari_cins { get; set; }

	public string cha_kod { get; set; }

	public enum_cha_kasa_hizmet cha_kasa_hizmet { get; set; }

	public string cha_kasa_hizkod { get; set; }

	public DateTime cha_d_kurtar { get; set; }

	public int cha_d_cins { get; set; }

	public double cha_d_kur { get; set; }

	public double cha_altd_kur { get; set; }

	public int cha_grupno { get; set; }

	public double cha_meblag { get; set; }

	public int cha_vade { get; set; }

	public DateTime cha_fis_tarih { get; set; }

	public int cha_fis_sirano { get; set; }

	public double cha_ft_iskonto1 { get; set; }

	public double cha_ft_iskonto2 { get; set; }

	public double cha_ft_iskonto3 { get; set; }

	public double cha_ft_iskonto4 { get; set; }

	public double cha_ft_iskonto5 { get; set; }

	public double cha_ft_iskonto6 { get; set; }

	public double cha_ft_masraf1 { get; set; }

	public double cha_ft_masraf2 { get; set; }

	public double cha_ft_masraf3 { get; set; }

	public double cha_ft_masraf4 { get; set; }

	public double cha_vergi1 { get; set; }

	public double cha_vergi2 { get; set; }

	public double cha_vergi3 { get; set; }

	public double cha_vergi4 { get; set; }

	public double cha_vergi5 { get; set; }

	public double cha_vergi6 { get; set; }

	public double cha_vergi7 { get; set; }

	public double cha_vergi8 { get; set; }

	public double cha_vergi9 { get; set; }

	public double cha_vergi10 { get; set; }

	public double cha_yuvarlama { get; set; }

	public enum_cha_tpoz cha_tpoz { get; set; }

	public string cha_aciklama { get; set; }

	public string cha_trefno { get; set; }

	public enum_cha_sntck_poz cha_sntck_poz { get; set; }

	public int cha_karsidcinsi { get; set; }

	public double cha_karsid_kur { get; set; }

	public int cha_karsidgrupno { get; set; }

	public string cha_srmrkkodu { get; set; }

	public DateTime cha_reftarihi { get; set; }

	public double cha_odeme_arr1 { get; set; }

	public double cha_odeme_arr2 { get; set; }

	public double cha_odeme_arr3 { get; set; }

	public double cha_odeme_arr4 { get; set; }

	public double cha_odeme_arr5 { get; set; }

	public double cha_odeme_arr6 { get; set; }

	public double cha_odeme_arr7 { get; set; }

	public double cha_odeme_arr8 { get; set; }

	public double cha_miktari { get; set; }

	public double cha_aratoplam { get; set; }

	public int cha_vergipntr { get; set; }

	public int cha_istisnakodu { get; set; }

	public double cha_ver_tev_carpani { get; set; }

	public double cha_stopaj { get; set; }

	public double cha_savsandesfonu { get; set; }

	public bool cha_vergisiz_fl { get; set; }

	public string cha_satici_kodu { get; set; }

	public double cha_mustahsil_borsa { get; set; }

	public double cha_mustahsil_bagkur { get; set; }

	public double cha_mustahsil_diger { get; set; }

	public double cha_HalMSDF { get; set; }

	public double cha_HalHamaliye { get; set; }

	public double cha_HalStopaj { get; set; }

	public double cha_HalKomisyonu { get; set; }

	public int cha_StFonPntr { get; set; }

	public bool cha_pos_hareketi { get; set; }

	public DateTime cha_vardiya_tarihi { get; set; }

	public int cha_vardiya_no { get; set; }

	public enum_cha_vardita_evrak_ti cha_vardiya_evrak_ti { get; set; }

	public double cha_HalRusum { get; set; }

	public double cha_HalNavlunTut { get; set; }

	public double cha_HalRehinFuture { get; set; }

	public double cha_HalKomisyon { get; set; }

	public double cha_Vade_Farki_Yuz { get; set; }

	public string cha_karsisrmrkkodu { get; set; }

	public string cha_EXIMkodu { get; set; }

	public double cha_HalRehinSandikmiktari { get; set; }

	public double cha_HalSandikVrMiktar { get; set; }

	public double cha_HalSandikTutari { get; set; }

	public double cha_HalSandikKDVTutari { get; set; }

	public double cha_HalrehinSandikTutari { get; set; }

	public enum_cha_Tevkifat_turu cha_Tevkifat_turu { get; set; }

	public enum_cha_ticaret_turu cha_ticaret_turu { get; set; }

	public double cha_otvtutari { get; set; }

	public bool cha_otvvergisiz_fl { get; set; }

	public string cha_projekodu { get; set; }

	public string cha_yat_tes_kodu { get; set; }

	public string cha_ciro_cari_kodu { get; set; }

	public bool cha_oivergisiz_fl { get; set; }

	public int cha_meblag_ana_doviz_icin_gecersiz_fl { get; set; }

	public int cha_meblag_alt_doviz_icin_gecersiz_fl { get; set; }

	public int cha_meblag_orj_doviz_icin_gecersiz_fl { get; set; }

	public double cha_HalHamaliyeKdv { get; set; }

	public bool cha_HalHamaliyeVergisiz_fl { get; set; }

	public int cha_oiv_pntr { get; set; }

	public double cha_oiv_vergi { get; set; }

	public double cha_oivtutari { get; set; }

	public int cha_isk_mas1 { get; set; }

	public int cha_isk_mas2 { get; set; }

	public int cha_isk_mas3 { get; set; }

	public int cha_isk_mas4 { get; set; }

	public int cha_isk_mas5 { get; set; }

	public int cha_isk_mas6 { get; set; }

	public int cha_isk_mas7 { get; set; }

	public int cha_isk_mas8 { get; set; }

	public int cha_isk_mas9 { get; set; }

	public int cha_isk_mas10 { get; set; }

	public bool cha_sat_iskmas1 { get; set; }

	public bool cha_sat_iskmas2 { get; set; }

	public bool cha_sat_iskmas3 { get; set; }

	public bool cha_sat_iskmas4 { get; set; }

	public bool cha_sat_iskmas5 { get; set; }

	public bool cha_sat_iskmas6 { get; set; }

	public bool cha_sat_iskmas7 { get; set; }

	public bool cha_sat_iskmas8 { get; set; }

	public bool cha_sat_iskmas9 { get; set; }

	public bool cha_sat_iskmas10 { get; set; }

	public double cha_tevkifat1Yok { get; set; }

	public double cha_tevkifat131 { get; set; }

	public double cha_tevkifat191 { get; set; }

	public double cha_tevkifat121 { get; set; }

	public double cha_tevkifat132 { get; set; }

	public double cha_tevkifat161 { get; set; }

	public double cha_tevkifat145 { get; set; }

	public double cha_tevkifat1Tam { get; set; }

	public double cha_tevkifat1102 { get; set; }

	public double cha_tevkifat1105 { get; set; }

	public double cha_tevkifat1107 { get; set; }

	public double cha_tevkifat2Yok { get; set; }

	public double cha_tevkifat231 { get; set; }

	public double cha_tevkifat291 { get; set; }

	public double cha_tevkifat221 { get; set; }

	public double cha_tevkifat232 { get; set; }

	public double cha_tevkifat261 { get; set; }

	public double cha_tevkifat245 { get; set; }

	public double cha_tevkifat2Tam { get; set; }

	public double cha_tevkifat2102 { get; set; }

	public double cha_tevkifat2105 { get; set; }

	public double cha_tevkifat2107 { get; set; }

	public double cha_tevkifat3Yok { get; set; }

	public double cha_tevkifat331 { get; set; }

	public double cha_tevkifat391 { get; set; }

	public double cha_tevkifat321 { get; set; }

	public double cha_tevkifat332 { get; set; }

	public double cha_tevkifat361 { get; set; }

	public double cha_tevkifat345 { get; set; }

	public double cha_tevkifat3Tam { get; set; }

	public double cha_tevkifat3102 { get; set; }

	public double cha_tevkifat3105 { get; set; }

	public double cha_tevkifat3107 { get; set; }

	public double cha_tevkifat4Yok { get; set; }

	public double cha_tevkifat431 { get; set; }

	public double cha_tevkifat491 { get; set; }

	public double cha_tevkifat421 { get; set; }

	public double cha_tevkifat432 { get; set; }

	public double cha_tevkifat461 { get; set; }

	public double cha_tevkifat445 { get; set; }

	public double cha_tevkifat4Tam { get; set; }

	public double cha_tevkifat4102 { get; set; }

	public double cha_tevkifat4105 { get; set; }

	public double cha_tevkifat4107 { get; set; }

	public double cha_tevkifat5Yok { get; set; }

	public double cha_tevkifat531 { get; set; }

	public double cha_tevkifat591 { get; set; }

	public double cha_tevkifat521 { get; set; }

	public double cha_tevkifat532 { get; set; }

	public double cha_tevkifat561 { get; set; }

	public double cha_tevkifat545 { get; set; }

	public double cha_tevkifat5Tam { get; set; }

	public double cha_tevkifat5102 { get; set; }

	public double cha_tevkifat5105 { get; set; }

	public double cha_tevkifat5107 { get; set; }

	public double cha_tevkifat6Yok { get; set; }

	public double cha_tevkifat631 { get; set; }

	public double cha_tevkifat691 { get; set; }

	public double cha_tevkifat621 { get; set; }

	public double cha_tevkifat632 { get; set; }

	public double cha_tevkifat661 { get; set; }

	public double cha_tevkifat645 { get; set; }

	public double cha_tevkifat6Tam { get; set; }

	public double cha_tevkifat6102 { get; set; }

	public double cha_tevkifat6105 { get; set; }

	public double cha_tevkifat6107 { get; set; }

	public double cha_tevkifat7Yok { get; set; }

	public double cha_tevkifat731 { get; set; }

	public double cha_tevkifat791 { get; set; }

	public double cha_tevkifat721 { get; set; }

	public double cha_tevkifat732 { get; set; }

	public double cha_tevkifat761 { get; set; }

	public double cha_tevkifat745 { get; set; }

	public double cha_tevkifat7Tam { get; set; }

	public double cha_tevkifat7102 { get; set; }

	public double cha_tevkifat7105 { get; set; }

	public double cha_tevkifat7107 { get; set; }

	public double cha_tevkifat8Yok { get; set; }

	public double cha_tevkifat831 { get; set; }

	public double cha_tevkifat891 { get; set; }

	public double cha_tevkifat821 { get; set; }

	public double cha_tevkifat832 { get; set; }

	public double cha_tevkifat861 { get; set; }

	public double cha_tevkifat845 { get; set; }

	public double cha_tevkifat8Tam { get; set; }

	public double cha_tevkifat8102 { get; set; }

	public double cha_tevkifat8105 { get; set; }

	public double cha_tevkifat8107 { get; set; }

	public double cha_tevkifat9Yok { get; set; }

	public double cha_tevkifat931 { get; set; }

	public double cha_tevkifat991 { get; set; }

	public double cha_tevkifat921 { get; set; }

	public double cha_tevkifat932 { get; set; }

	public double cha_tevkifat961 { get; set; }

	public double cha_tevkifat945 { get; set; }

	public double cha_tevkifat9Tam { get; set; }

	public double cha_tevkifat9102 { get; set; }

	public double cha_tevkifat9105 { get; set; }

	public double cha_tevkifat9107 { get; set; }

	public double cha_tevkifat10Yok { get; set; }

	public double cha_tevkifat1031 { get; set; }

	public double cha_tevkifat1091 { get; set; }

	public double cha_tevkifat1021 { get; set; }

	public double cha_tevkifat1032 { get; set; }

	public double cha_tevkifat1061 { get; set; }

	public double cha_tevkifat1045 { get; set; }

	public double cha_tevkifat10Tam { get; set; }

	public double cha_tevkifat10102 { get; set; }

	public double cha_tevkifat10105 { get; set; }

	public double cha_tevkifat10107 { get; set; }

	public double cha_HalRusumKdv { get; set; }

	public double cha_HalDiger { get; set; }

	public double cha_HalDigerKdv { get; set; }

	public bool cha_HalDigerVergisiz_fl { get; set; }

	public bool cha_HalrusumVergisiz_fl { get; set; }

	public bool cha_Halrusumsuz_fl { get; set; }

	public string sck_borclu { get; set; }

	public string sck_bankano { get; set; }

	public string sck_vdaire_no { get; set; }

	public string sck_banka_adres1 { get; set; }

	public string sck_sube_adres2 { get; set; }

	public string sck_hesapno_sehir { get; set; }

	public string sck_no { get; set; }

	public string Sck_TCMB_Banka_kodu { get; set; }

	public string Sck_TCMB_Sube_kodu { get; set; }

	public string Sck_TCMB_il_kodu { get; set; }

	public double IskontoTutariToplam => cha_ft_iskonto1 + cha_ft_iskonto2 + cha_ft_iskonto3 + cha_ft_iskonto4 + cha_ft_iskonto5 + cha_ft_iskonto6;

	public double MasrafTutariToplam => cha_ft_masraf1 + cha_ft_masraf2 + cha_ft_masraf3 + cha_ft_masraf4;

	public double KdvTutariToplam => cha_vergi1 + cha_vergi2 + cha_vergi3 + cha_vergi4 + cha_vergi5 + cha_vergi6 + cha_vergi7 + cha_vergi8 + cha_vergi9 + cha_vergi10;

	public CARI_HESAP_HAREKETLERI()
	{
		cha_fileid = 51;
		cha_d_kur = 1.0;
		cha_altd_kur = 1.0;
		cha_karsid_kur = 1.0;
		cha_special1 = "";
		cha_special2 = "";
		cha_special3 = "";
		cha_evrakno_seri = "";
		cha_belge_no = "";
		cha_kod = "";
		cha_kasa_hizkod = "";
		cha_aciklama = "";
		cha_trefno = "";
		cha_srmrkkodu = "";
		cha_satici_kodu = "";
		cha_karsisrmrkkodu = "";
		cha_EXIMkodu = "";
		cha_projekodu = "";
		cha_yat_tes_kodu = "";
		cha_ciro_cari_kodu = "";
		sck_borclu = "";
		sck_bankano = "";
		sck_vdaire_no = "";
		sck_banka_adres1 = "";
		sck_sube_adres2 = "";
		sck_hesapno_sehir = "";
		sck_no = "";
		Sck_TCMB_Banka_kodu = "";
		Sck_TCMB_Sube_kodu = "";
		Sck_TCMB_il_kodu = "";
		cha_create_date = DateTime.Now;
		cha_lastup_date = DateTime.Now;
		cha_tarihi = DateTime.Now;
		cha_belge_tarih = DateTime.Now;
		cha_d_kurtar = DateTime.Now;
		cha_fis_tarih = DateTime.Now;
		cha_reftarihi = DateTime.Now;
		cha_vardiya_tarihi = DateTime.Now;
		cha_Guid = Guid.Empty;
		cha_sip_uid = Guid.Empty;
		cha_kirahar_uid = Guid.Empty;
	}

	public static byte[] WriteToByteArray(CARI_HESAP_HAREKETLERI toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(CARI_HESAP_HAREKETLERI toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(CARI_HESAP_HAREKETLERI toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.cha_RECno);
		writer.Write(toWrite.cha_RECid_DBCno);
		writer.Write(toWrite.cha_RECid_RECno);
		writer.Write(toWrite.cha_SpecRecNo);
		writer.Write(toWrite.cha_iptal);
		writer.Write(toWrite.cha_fileid);
		writer.Write(toWrite.cha_hidden);
		writer.Write(toWrite.cha_kilitli);
		writer.Write(toWrite.cha_degisti);
		writer.Write(toWrite.cha_CheckSum);
		writer.Write(toWrite.cha_create_user);
		writer.Write(toWrite.cha_create_date.Ticks);
		writer.Write(toWrite.cha_lastup_user);
		writer.Write(toWrite.cha_lastup_date.Ticks);
		writer.Write(toWrite.cha_special1);
		writer.Write(toWrite.cha_special2);
		writer.Write(toWrite.cha_special3);
		writer.Write(toWrite.cha_firmano);
		writer.Write(toWrite.cha_subeno);
		writer.Write(toWrite.cha_tarihi.Ticks);
		writer.Write((int)toWrite.cha_tip);
		writer.Write((int)toWrite.cha_cinsi);
		writer.Write((int)toWrite.cha_normal_Iade);
		writer.Write((int)toWrite.cha_evrak_tip);
		writer.Write(toWrite.cha_satir_no);
		writer.Write(toWrite.cha_evrakno_seri);
		writer.Write(toWrite.cha_evrakno_sira);
		writer.Write(toWrite.cha_belge_no);
		writer.Write(toWrite.cha_belge_tarih.Ticks);
		writer.Write((int)toWrite.cha_cari_cins);
		writer.Write(toWrite.cha_kod);
		writer.Write((int)toWrite.cha_kasa_hizmet);
		writer.Write(toWrite.cha_kasa_hizkod);
		writer.Write(toWrite.cha_d_kurtar.Ticks);
		writer.Write(toWrite.cha_d_cins);
		writer.Write(toWrite.cha_d_kur);
		writer.Write(toWrite.cha_altd_kur);
		writer.Write(toWrite.cha_grupno);
		writer.Write(toWrite.cha_meblag);
		writer.Write(toWrite.cha_vade);
		writer.Write(toWrite.cha_fis_tarih.Ticks);
		writer.Write(toWrite.cha_fis_sirano);
		writer.Write(toWrite.cha_ft_iskonto1);
		writer.Write(toWrite.cha_ft_iskonto2);
		writer.Write(toWrite.cha_ft_iskonto3);
		writer.Write(toWrite.cha_ft_iskonto4);
		writer.Write(toWrite.cha_ft_iskonto5);
		writer.Write(toWrite.cha_ft_iskonto6);
		writer.Write(toWrite.cha_ft_masraf1);
		writer.Write(toWrite.cha_ft_masraf2);
		writer.Write(toWrite.cha_ft_masraf3);
		writer.Write(toWrite.cha_ft_masraf4);
		writer.Write(toWrite.cha_vergi1);
		writer.Write(toWrite.cha_vergi2);
		writer.Write(toWrite.cha_vergi3);
		writer.Write(toWrite.cha_vergi4);
		writer.Write(toWrite.cha_vergi5);
		writer.Write(toWrite.cha_vergi6);
		writer.Write(toWrite.cha_vergi7);
		writer.Write(toWrite.cha_vergi8);
		writer.Write(toWrite.cha_vergi9);
		writer.Write(toWrite.cha_vergi10);
		writer.Write(toWrite.cha_yuvarlama);
		writer.Write((int)toWrite.cha_tpoz);
		writer.Write(toWrite.cha_aciklama);
		writer.Write(toWrite.cha_trefno);
		writer.Write((int)toWrite.cha_sntck_poz);
		writer.Write(toWrite.cha_karsidcinsi);
		writer.Write(toWrite.cha_karsid_kur);
		writer.Write(toWrite.cha_karsidgrupno);
		writer.Write(toWrite.cha_srmrkkodu);
		writer.Write(toWrite.cha_reftarihi.Ticks);
		writer.Write(toWrite.cha_miktari);
		writer.Write(toWrite.cha_aratoplam);
		writer.Write(toWrite.cha_vergipntr);
		writer.Write(toWrite.cha_vergisiz_fl);
		writer.Write(toWrite.cha_satici_kodu);
		writer.Write(toWrite.cha_Vade_Farki_Yuz);
		writer.Write(toWrite.cha_karsisrmrkkodu);
		writer.Write((int)toWrite.cha_ticaret_turu);
		writer.Write(toWrite.cha_otvtutari);
		writer.Write(toWrite.cha_otvvergisiz_fl);
		writer.Write(toWrite.cha_projekodu);
		writer.Write(toWrite.cha_ciro_cari_kodu);
		writer.Write(toWrite.cha_oivergisiz_fl);
		writer.Write(toWrite.cha_bakimhar_DBCno);
		writer.Write(toWrite.cha_bakimhar_RECno);
		writer.Write(toWrite.cha_oiv_pntr);
		writer.Write(toWrite.cha_oiv_vergi);
		writer.Write(toWrite.cha_oivtutari);
		writer.Write(toWrite.cha_isk_mas1);
		writer.Write(toWrite.cha_isk_mas2);
		writer.Write(toWrite.cha_isk_mas3);
		writer.Write(toWrite.cha_isk_mas4);
		writer.Write(toWrite.cha_isk_mas5);
		writer.Write(toWrite.cha_isk_mas6);
		writer.Write(toWrite.cha_isk_mas7);
		writer.Write(toWrite.cha_isk_mas8);
		writer.Write(toWrite.cha_isk_mas9);
		writer.Write(toWrite.cha_isk_mas10);
		writer.Write(toWrite.cha_sat_iskmas1);
		writer.Write(toWrite.cha_sat_iskmas2);
		writer.Write(toWrite.cha_sat_iskmas3);
		writer.Write(toWrite.cha_sat_iskmas4);
		writer.Write(toWrite.cha_sat_iskmas5);
		writer.Write(toWrite.cha_sat_iskmas6);
		writer.Write(toWrite.cha_sat_iskmas7);
		writer.Write(toWrite.cha_sat_iskmas8);
		writer.Write(toWrite.cha_sat_iskmas9);
		writer.Write(toWrite.cha_sat_iskmas10);
		writer.Write(toWrite.cha_sip_recid_dbcno);
		writer.Write(toWrite.cha_sip_recid_recno);
		writer.Write(toWrite.sck_borclu);
		writer.Write(toWrite.sck_bankano);
		writer.Write(toWrite.sck_vdaire_no);
		writer.Write(toWrite.sck_banka_adres1);
		writer.Write(toWrite.sck_sube_adres2);
		writer.Write(toWrite.sck_hesapno_sehir);
		writer.Write(toWrite.sck_no);
		writer.Write(toWrite.Sck_TCMB_Banka_kodu);
		writer.Write(toWrite.Sck_TCMB_Sube_kodu);
		writer.Write(toWrite.Sck_TCMB_il_kodu);
		_ = 2;
	}

	public static CARI_HESAP_HAREKETLERI ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		CARI_HESAP_HAREKETLERI result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static CARI_HESAP_HAREKETLERI ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static CARI_HESAP_HAREKETLERI ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new CARI_HESAP_HAREKETLERI
		{
			cha_RECno = reader.ReadInt32(),
			cha_RECid_DBCno = reader.ReadInt32(),
			cha_RECid_RECno = reader.ReadInt32(),
			cha_SpecRecNo = reader.ReadInt32(),
			cha_iptal = reader.ReadBoolean(),
			cha_fileid = reader.ReadInt32(),
			cha_hidden = reader.ReadBoolean(),
			cha_kilitli = reader.ReadBoolean(),
			cha_degisti = reader.ReadBoolean(),
			cha_CheckSum = reader.ReadInt32(),
			cha_create_user = reader.ReadInt32(),
			cha_create_date = new DateTime(reader.ReadInt64()),
			cha_lastup_user = reader.ReadInt32(),
			cha_lastup_date = new DateTime(reader.ReadInt64()),
			cha_special1 = reader.ReadString(),
			cha_special2 = reader.ReadString(),
			cha_special3 = reader.ReadString(),
			cha_firmano = reader.ReadInt32(),
			cha_subeno = reader.ReadInt32(),
			cha_tarihi = new DateTime(reader.ReadInt64()),
			cha_tip = (enum_cha_tip)reader.ReadInt32(),
			cha_cinsi = (enum_cha_cinsi)reader.ReadInt32(),
			cha_normal_Iade = (enum_cha_normal_Iade)reader.ReadInt32(),
			cha_evrak_tip = (enum_cha_evrak_tip)reader.ReadInt32(),
			cha_satir_no = reader.ReadInt32(),
			cha_evrakno_seri = reader.ReadString(),
			cha_evrakno_sira = reader.ReadInt32(),
			cha_belge_no = reader.ReadString(),
			cha_belge_tarih = new DateTime(reader.ReadInt64()),
			cha_cari_cins = (enum_cha_cari_cins)reader.ReadInt32(),
			cha_kod = reader.ReadString(),
			cha_kasa_hizmet = (enum_cha_kasa_hizmet)reader.ReadInt32(),
			cha_kasa_hizkod = reader.ReadString(),
			cha_d_kurtar = new DateTime(reader.ReadInt64()),
			cha_d_cins = reader.ReadInt32(),
			cha_d_kur = reader.ReadDouble(),
			cha_altd_kur = reader.ReadDouble(),
			cha_grupno = reader.ReadInt32(),
			cha_meblag = reader.ReadDouble(),
			cha_vade = reader.ReadInt32(),
			cha_fis_tarih = new DateTime(reader.ReadInt64()),
			cha_fis_sirano = reader.ReadInt32(),
			cha_ft_iskonto1 = reader.ReadDouble(),
			cha_ft_iskonto2 = reader.ReadDouble(),
			cha_ft_iskonto3 = reader.ReadDouble(),
			cha_ft_iskonto4 = reader.ReadDouble(),
			cha_ft_iskonto5 = reader.ReadDouble(),
			cha_ft_iskonto6 = reader.ReadDouble(),
			cha_ft_masraf1 = reader.ReadDouble(),
			cha_ft_masraf2 = reader.ReadDouble(),
			cha_ft_masraf3 = reader.ReadDouble(),
			cha_ft_masraf4 = reader.ReadDouble(),
			cha_vergi1 = reader.ReadDouble(),
			cha_vergi2 = reader.ReadDouble(),
			cha_vergi3 = reader.ReadDouble(),
			cha_vergi4 = reader.ReadDouble(),
			cha_vergi5 = reader.ReadDouble(),
			cha_vergi6 = reader.ReadDouble(),
			cha_vergi7 = reader.ReadDouble(),
			cha_vergi8 = reader.ReadDouble(),
			cha_vergi9 = reader.ReadDouble(),
			cha_vergi10 = reader.ReadDouble(),
			cha_yuvarlama = reader.ReadDouble(),
			cha_tpoz = (enum_cha_tpoz)reader.ReadInt32(),
			cha_aciklama = reader.ReadString(),
			cha_trefno = reader.ReadString(),
			cha_sntck_poz = (enum_cha_sntck_poz)reader.ReadInt32(),
			cha_karsidcinsi = reader.ReadInt32(),
			cha_karsid_kur = reader.ReadDouble(),
			cha_karsidgrupno = reader.ReadInt32(),
			cha_srmrkkodu = reader.ReadString(),
			cha_reftarihi = new DateTime(reader.ReadInt64()),
			cha_miktari = reader.ReadDouble(),
			cha_aratoplam = reader.ReadDouble(),
			cha_vergipntr = reader.ReadInt32(),
			cha_vergisiz_fl = reader.ReadBoolean(),
			cha_satici_kodu = reader.ReadString(),
			cha_Vade_Farki_Yuz = reader.ReadDouble(),
			cha_karsisrmrkkodu = reader.ReadString(),
			cha_ticaret_turu = (enum_cha_ticaret_turu)reader.ReadInt32(),
			cha_otvtutari = reader.ReadDouble(),
			cha_otvvergisiz_fl = reader.ReadBoolean(),
			cha_projekodu = reader.ReadString(),
			cha_ciro_cari_kodu = reader.ReadString(),
			cha_oivergisiz_fl = reader.ReadBoolean(),
			cha_bakimhar_DBCno = reader.ReadInt32(),
			cha_bakimhar_RECno = reader.ReadInt32(),
			cha_oiv_pntr = reader.ReadInt32(),
			cha_oiv_vergi = reader.ReadDouble(),
			cha_oivtutari = reader.ReadDouble(),
			cha_isk_mas1 = reader.ReadInt32(),
			cha_isk_mas2 = reader.ReadInt32(),
			cha_isk_mas3 = reader.ReadInt32(),
			cha_isk_mas4 = reader.ReadInt32(),
			cha_isk_mas5 = reader.ReadInt32(),
			cha_isk_mas6 = reader.ReadInt32(),
			cha_isk_mas7 = reader.ReadInt32(),
			cha_isk_mas8 = reader.ReadInt32(),
			cha_isk_mas9 = reader.ReadInt32(),
			cha_isk_mas10 = reader.ReadInt32(),
			cha_sat_iskmas1 = reader.ReadBoolean(),
			cha_sat_iskmas2 = reader.ReadBoolean(),
			cha_sat_iskmas3 = reader.ReadBoolean(),
			cha_sat_iskmas4 = reader.ReadBoolean(),
			cha_sat_iskmas5 = reader.ReadBoolean(),
			cha_sat_iskmas6 = reader.ReadBoolean(),
			cha_sat_iskmas7 = reader.ReadBoolean(),
			cha_sat_iskmas8 = reader.ReadBoolean(),
			cha_sat_iskmas9 = reader.ReadBoolean(),
			cha_sat_iskmas10 = reader.ReadBoolean(),
			cha_sip_recid_dbcno = reader.ReadInt32(),
			cha_sip_recid_recno = reader.ReadInt32(),
			sck_borclu = reader.ReadString(),
			sck_bankano = reader.ReadString(),
			sck_vdaire_no = reader.ReadString(),
			sck_banka_adres1 = reader.ReadString(),
			sck_sube_adres2 = reader.ReadString(),
			sck_hesapno_sehir = reader.ReadString(),
			sck_no = reader.ReadString(),
			Sck_TCMB_Banka_kodu = reader.ReadString(),
			Sck_TCMB_Sube_kodu = reader.ReadString(),
			Sck_TCMB_il_kodu = reader.ReadString()
		};
	}
}
