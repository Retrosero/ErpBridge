using System.ComponentModel;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public enum enum_DegerTipi
{
	[Description("Özel değer")]
	OzelDeger = 0,
	[Description("Evrak tarihi")]
	evraktarih = 1,
	[Description("Evrak no seri")]
	evraknoseri = 2,
	[Description("Evrak no sıra")]
	evraknosira = 3,
	[Description("Belge no")]
	belgeno = 4,
	[Description("Belge tarihi")]
	belgetarih = 5,
	[Description("Ödeme planı")]
	odemeplani = 6,
	[Description("Fiyat listesi (Açıklama)")]
	fiyatlistesi_aciklama = 7,
	[Description("Fiyat listesi (Kdv dahil)")]
	fiyatlistesi_kdvdahil = 8,
	[Description("Fiyat listesi (Sıra no)")]
	fiyatlistesi_sirano = 9,
	[Description("Döviz cinsi (Döviz adı)")]
	dovizcinsi_doviz_adi = 10,
	[Description("Döviz cinsi (Döviz kod)")]
	dovizcinsi_doviz_kod = 11,
	[Description("Döviz cinsi (Döviz Sembol)")]
	dovizcinsi_doviz_sembol = 12,
	[Description("Kur")]
	kur = 13,
	[Description("Kaynak depo (Depo adı)")]
	kaynakdepo_depo_adi = 14,
	[Description("Kaynak depo (Depo no)")]
	kaynakdepo_depo_no = 15,
	[Description("Proje (Kodu)")]
	proje_kodu = 16,
	[Description("Proje (Adı)")]
	proje_adi = 17,
	[Description("Sorumluluk merkezi (Kodu)")]
	sorumlulukmerkezi_kodu = 18,
	[Description("Sorumluluk merkezi (Adı)")]
	sorumlulukmerkezi_adi = 19,
	[Description("Temsilci kodu")]
	temsilcikodu = 20,
	[Description("Firma (Sıra no)")]
	firma_sirano = 21,
	[Description("Firma (Ünvan)")]
	firma_unvan = 22,
	[Description("Şube (No)")]
	sube_no = 23,
	[Description("Şube (Adı)")]
	sube_adi = 24,
	[Description("Sevk/teslim tarihi")]
	sevkteslimtarihi = 25,
	[Description("Kapama hesap kodu")]
	kapamahesapkodu = 27,
	[Description("Fatura açıklama")]
	faturaaciklama = 28,
	[Description("Açıklama 1")]
	aciklama1 = 29,
	[Description("Açıklama 2")]
	aciklama2 = 30,
	[Description("Açıklama 3")]
	aciklama3 = 31,
	[Description("Açıklama 4")]
	aciklama4 = 32,
	[Description("Açıklama 5")]
	aciklama5 = 33,
	[Description("Açıklama 6")]
	aciklama6 = 34,
	[Description("Açıklama 7")]
	aciklama7 = 35,
	[Description("Açıklama 8")]
	aciklama8 = 36,
	[Description("Açıklama 9")]
	aciklama9 = 37,
	[Description("Açıklama 10")]
	aciklama10 = 38,
	[Description("Satır cinsi")]
	satircinsi = 39,
	[Description("Miktar")]
	miktar = 40,
	[Description("Miktar 2")]
	miktar2 = 41,
	[Description("Vergi pntr")]
	vergi_pntr = 42,
	[Description("Fiyat farkı mı")]
	fiyat_fark_mi = 43,
	[Description("Kriter metin 1")]
	kriter_string1 = 44,
	[Description("Kriter metin 2")]
	kriter_string2 = 45,
	[Description("Kriter metin 3")]
	kriter_string3 = 46,
	[Description("Kriter metin 4")]
	kriter_string4 = 47,
	[Description("Kriter metin 5")]
	kriter_string5 = 48,
	[Description("Kriter sayı 1")]
	kriter_double1 = 49,
	[Description("Kriter sayı 2")]
	kriter_double2 = 50,
	[Description("Kriter sayı 3")]
	kriter_double3 = 51,
	[Description("Kriter sayı 4")]
	kriter_double4 = 52,
	[Description("Kriter sayı 5")]
	kriter_double5 = 53,
	[Description("Kriter evet/hayır 1")]
	kriter_bool1 = 54,
	[Description("Kriter evet/hayır 2")]
	kriter_bool2 = 55,
	[Description("Kriter evet/hayır 3")]
	kriter_bool3 = 56,
	[Description("Kriter evet/hayır 4")]
	kriter_bool4 = 57,
	[Description("Kriter evet/hayır 5")]
	kriter_bool5 = 58,
	[Description("Birim fiyat brüt fiyat")]
	birimfiyat_brut_fiyat = 59,
	[Description("Birim fiyat iskonto 1 uygulama şekli")]
	birimfiyat_iskonto1_uygulama_sekli = 60,
	[Description("Birim fiyat iskonto 1 yüzde/tutar")]
	birimfiyat_iskonto1_yuzdeveyatutar = 61,
	[Description("Birim fiyat iskonto 2 uygulama şekli")]
	birimfiyat_iskonto2_uygulama_sekli = 62,
	[Description("Birim fiyat iskonto 2 yüzde/tutar")]
	birimfiyat_iskonto2_yuzdeveyatutar = 63,
	[Description("Birim fiyat iskonto 3 uygulama şekli")]
	birimfiyat_iskonto3_uygulama_sekli = 64,
	[Description("Birim fiyat iskonto 3 yüzde/tutar")]
	birimfiyat_iskonto3_yuzdeveyatutar = 65,
	[Description("Birim fiyat iskonto 4 uygulama şekli")]
	birimfiyat_iskonto4_uygulama_sekli = 66,
	[Description("Birim fiyat iskonto 4 yüzde/tutar")]
	birimfiyat_iskonto4_yuzdeveyatutar = 67,
	[Description("Birim fiyat iskonto 5 uygulama şekli")]
	birimfiyat_iskonto5_uygulama_sekli = 68,
	[Description("Birim fiyat iskonto 5 yüzde/tutar")]
	birimfiyat_iskonto5_yuzdeveyatutar = 69,
	[Description("Birim fiyat iskonto 6 uygulama şekli")]
	birimfiyat_iskonto6_uygulama_sekli = 70,
	[Description("Birim fiyat iskonto 6 yüzde/tutar")]
	birimfiyat_iskonto6_yuzdeveyatutar = 71,
	[Description("Birim fiyat ÖTV uygulama şekli")]
	birimfiyat_otv_uygulama_sekli = 72,
	[Description("Birim fiyat ÖTV yüzde/tutar")]
	birimfiyat_otv_yuzdeveyatutar = 73,
	[Description("Birim fiyat ÖTV vergi pntr")]
	birimfiyat_otv_vergipntr = 74,
	[Description("Ara toplam")]
	ara_toplam = 75,
	[Description("İskonto toplamı")]
	iskonto_toplam = 76,
	[Description("Kdv toplamı")]
	kdv_toplam = 77,
	[Description("Masraf toplamı")]
	masraf_toplam = 78,
	[Description("ÖTV toplamı")]
	otv_toplam = 79,
	[Description("Yekün")]
	yekun = 80,
	[Description("Ara toplam (Yazı ile)")]
	ara_toplam_yazi_ile = 81,
	[Description("İskonto toplamı (Yazı ile)")]
	iskonto_toplam_yazi_ile = 82,
	[Description("Kdv toplamı (Yazı ile)")]
	kdv_toplam_yazi_ile = 83,
	[Description("Masraf toplamı (Yazı ile)")]
	masraf_toplam_yazi_ile = 84,
	[Description("ÖTV toplamı (Yazı ile)")]
	otv_toplam_yazi_ile = 85,
	[Description("Yekün (Yazı ile)")]
	yekun_yazi_ile = 86,
	[Description("Evrak tipi")]
	evraktipi = 87,
	[Description("Normal/İade")]
	normaliade = 88,
	[Description("Açık/Kapalı")]
	kapamasekli = 89,
	[Description("Ticaret türü")]
	ticaretturu = 90,
	[Description("Kayıt ID")]
	KayitID = 91,
	[Description("Cari.cari_kod")]
	cari_kod = 1000,
	[Description("Cari.cari_unvan1")]
	cari_unvan1 = 1001,
	[Description("Cari.cari_unvan2")]
	cari_unvan2 = 1002,
	[Description("Cari.cari_muh_kod")]
	cari_muh_kod = 1003,
	[Description("Cari.cari_muh_kod1")]
	cari_muh_kod1 = 1004,
	[Description("Cari.cari_muh_kod2")]
	cari_muh_kod2 = 1005,
	[Description("Cari.cari_vdaire_adi")]
	cari_vdaire_adi = 1006,
	[Description("Cari.cari_vdaire_no")]
	cari_vdaire_no = 1007,
	[Description("Cari.cari_Ana_cari_kodu")]
	cari_Ana_cari_kodu = 1008,
	[Description("Cari.cari_bolge_kodu")]
	cari_bolge_kodu = 1009,
	[Description("Cari.cari_grup_kodu")]
	cari_grup_kodu = 1010,
	[Description("Cari.cari_temsilci_kodu")]
	cari_temsilci_kodu = 1011,
	[Description("Cari.cari_sektor_kodu")]
	cari_sektor_kodu = 1012,
	[Description("Cari.cari_satis_isk_kod")]
	cari_satis_isk_kod = 1013,
	[Description("Cari.cari_special1")]
	cari_special1 = 1014,
	[Description("Cari.cari_special2")]
	cari_special2 = 1015,
	[Description("Cari.cari_special3")]
	cari_special3 = 1016,
	[Description("Cari.cari_sicil_no")]
	cari_sicil_no = 1017,
	[Description("Cari.cari_VergiKimlikNo")]
	cari_VergiKimlikNo = 1018,
	[Description("Cari.cari_vade_fark_yuz")]
	cari_vade_fark_yuz = 1019,
	[Description("Cari.cari_vade_fark_yuz1")]
	cari_vade_fark_yuz1 = 1020,
	[Description("Cari.cari_vade_fark_yuz2")]
	cari_vade_fark_yuz2 = 1021,
	[Description("Cari.cari_tipi")]
	cari_tipi = 1022,
	[Description("Cari.cari_doviz_cinsi")]
	cari_doviz_cinsi = 1023,
	[Description("Cari.cari_doviz_cinsi1")]
	cari_doviz_cinsi1 = 1024,
	[Description("Cari.cari_doviz_cinsi2")]
	cari_doviz_cinsi2 = 1025,
	[Description("Cari.cari_odeme_gunu")]
	cari_odeme_gunu = 1026,
	[Description("Cari.cari_hareket_tipi")]
	cari_hareket_tipi = 1027,
	[Description("Cari.cari_odemeplan_no")]
	cari_odemeplan_no = 1028,
	[Description("Cari.cari_satis_fk")]
	cari_satis_fk = 1029,
	[Description("Cari.cari_KurHesapSekli")]
	cari_KurHesapSekli = 1030,
	[Description("Cari.cari_odeme_cinsi")]
	cari_odeme_cinsi = 1031,
	[Description("Cari.cari_fatura_adres_no")]
	cari_fatura_adres_no = 1032,
	[Description("Cari.cari_sevk_adres_no")]
	cari_sevk_adres_no = 1033,
	[Description("Cari.cari_banka_hesapno1")]
	cari_banka_hesapno1 = 1034,
	[Description("Cari.cari_CepTel")]
	cari_CepTel = 1035,
	[Description("Cari.cari_Email")]
	cari_Email = 1036,
	[Description("Cari.cari_VarsayilanGirisDepo")]
	cari_VarsayilanGirisDepo = 1037,
	[Description("Cari.cari_VarsayilanCikisDepo")]
	cari_VarsayilanCikisDepo = 1038,
	[Description("Stok.sto_kod")]
	sto_kod = 2000,
	[Description("Stok.sto_isim")]
	sto_isim = 2001,
	[Description("Stok.sto_perakende_vergi")]
	sto_perakende_vergi = 2002,
	[Description("Stok.sto_toptan_vergi")]
	sto_toptan_vergi = 2003,
	[Description("Stok.sto_kisa_ismi")]
	sto_kisa_ismi = 2004,
	[Description("Stok.sto_yabanci_isim")]
	sto_yabanci_isim = 2005,
	[Description("Stok.sto_sat_cari_kod")]
	sto_sat_cari_kod = 2006,
	[Description("Stok.sto_cins")]
	sto_cins = 2007,
	[Description("Stok.sto_doviz_cinsi")]
	sto_doviz_cinsi = 2008,
	[Description("Stok.sto_detay_takip")]
	sto_detay_takip = 2009,
	[Description("Stok.sto_birim1_ad")]
	sto_birim1_ad = 2010,
	[Description("Stok.sto_birim1_katsayi")]
	sto_birim1_katsayi = 2011,
	[Description("Stok.sto_birim2_ad")]
	sto_birim2_ad = 2012,
	[Description("Stok.sto_birim2_katsayi")]
	sto_birim2_katsayi = 2013,
	[Description("Stok.sto_birim3_ad")]
	sto_birim3_ad = 2014,
	[Description("Stok.sto_birim3_katsayi")]
	sto_birim3_katsayi = 2015,
	[Description("Stok.sto_birim4_ad")]
	sto_birim4_ad = 2016,
	[Description("Stok.sto_birim4_katsayi")]
	sto_birim4_katsayi = 2017,
	[Description("Stok.sto_bedenli_takip")]
	sto_bedenli_takip = 2018,
	[Description("Stok.sto_renkDetayli")]
	sto_renkDetayli = 2019,
	[Description("Stok.sto_beden_kodu")]
	sto_beden_kodu = 2020,
	[Description("Stok.sto_renk_kodu")]
	sto_renk_kodu = 2021,
	[Description("Stok.sto_altgrup_kod")]
	sto_altgrup_kod = 2022,
	[Description("Stok.sto_anagrup_kod")]
	sto_anagrup_kod = 2023,
	[Description("Stok.sto_sektor_kodu")]
	sto_sektor_kodu = 2024,
	[Description("Stok.sto_marka_kodu")]
	sto_marka_kodu = 2025,
	[Description("Stok.sto_model_kodu")]
	sto_model_kodu = 2026,
	[Description("Stok.sto_uretici_kodu")]
	sto_uretici_kodu = 2027,
	[Description("Stok.sto_reyon_kodu")]
	sto_reyon_kodu = 2028,
	[Description("Stok.sto_standartmaliyet")]
	sto_standartmaliyet = 2029,
	[Description("Hizmet.hiz_kod")]
	hiz_kod = 3000,
	[Description("Hizmet.hiz_tip")]
	hiz_tip = 3001,
	[Description("Hizmet.hiz_isim")]
	hiz_isim = 3002,
	[Description("Hizmet.hiz_yabanci_isim")]
	hiz_yabanci_isim = 3003,
	[Description("Hizmet.hiz_tipkod")]
	hiz_tipkod = 3004,
	[Description("Hizmet.hiz_sinifkod")]
	hiz_sinifkod = 3005,
	[Description("Hizmet.hiz_grupkod")]
	hiz_grupkod = 3006,
	[Description("Hizmet.hiz_sat_muh_kod")]
	hiz_sat_muh_kod = 3007,
	[Description("Hizmet.hiz_sat_iade_muh_kod")]
	hiz_sat_iade_muh_kod = 3008,
	[Description("Hizmet.hiz_mal_muh_kod")]
	hiz_mal_muh_kod = 3009,
	[Description("Hizmet.hiz_sat_mal_muh_kod")]
	hiz_sat_mal_muh_kod = 3010,
	[Description("Hizmet.hiz_mal_yan_muh_kod")]
	hiz_mal_yan_muh_kod = 3011,
	[Description("Hizmet.hiz_isk_grup")]
	hiz_isk_grup = 3012,
	[Description("Hizmet.hiz_KDV")]
	hiz_KDV = 3013,
	[Description("Hizmet.hiz_muh_sat_isk_kod")]
	hiz_muh_sat_isk_kod = 3014,
	[Description("Hizmet.hiz_muh_aIiskmuhkod")]
	hiz_muh_aIiskmuhkod = 3015,
	[Description("Hizmet.hiz_ilavemasmuhkod")]
	hiz_ilavemasmuhkod = 3016,
	[Description("Hizmet.hiz_operasyon_suresi")]
	hiz_operasyon_suresi = 3017,
	[Description("Hizmet.hiz_oivuygulama")]
	hiz_oivuygulama = 3018,
	[Description("Hizmet.hiz_oivtutar")]
	hiz_oivtutar = 3019,
	[Description("Hizmet.hiz_sat_ufrs_fark_muh_kod")]
	hiz_sat_ufrs_fark_muh_kod = 3020,
	[Description("Hizmet.hiz_sat_iade_ufrs_fark_muh_kod")]
	hiz_sat_iade_ufrs_fark_muh_kod = 3021,
	[Description("Hizmet.hiz_mal_ufrs_fark_muh_kod")]
	hiz_mal_ufrs_fark_muh_kod = 3022,
	[Description("Hizmet.hiz_sat_mal_ufrs_fark_muh_kod")]
	hiz_sat_mal_ufrs_fark_muh_kod = 3023,
	[Description("Hizmet.hiz_mal_yan_ufrs_fark_muh_kod")]
	hiz_mal_yan_ufrs_fark_muh_kod = 3024,
	[Description("Hizmet.hiz_muh_sat_ufrs_fark_isk_kod")]
	hiz_muh_sat_ufrs_fark_isk_kod = 3025,
	[Description("Hizmet.hiz_muh_aIiskufrs_fark_muhkod")]
	hiz_muh_aIiskufrs_fark_muhkod = 3026,
	[Description("Hizmet.hiz_ilavemasufrs_fark_muhkod")]
	hiz_ilavemasufrs_fark_muhkod = 3027,
	[Description("Sevk.sevk_adr_adres_no")]
	sevk_adr_adres_no = 4000,
	[Description("Sevk.sevk_adr_cari_kod")]
	sevk_adr_cari_kod = 4001,
	[Description("Sevk.sevk_adr_cadde")]
	sevk_adr_cadde = 4002,
	[Description("Sevk.sevk_adr_sokak")]
	sevk_adr_sokak = 4003,
	[Description("Sevk.sevk_adr_posta_kodu")]
	sevk_adr_posta_kodu = 4004,
	[Description("Sevk.sevk_adr_ilce")]
	sevk_adr_ilce = 4005,
	[Description("Sevk.sevk_adr_il")]
	sevk_adr_il = 4006,
	[Description("Sevk.sevk_adr_ulke")]
	sevk_adr_ulke = 4007,
	[Description("Sevk.sevk_adr_tel_ulke_kodu")]
	sevk_adr_tel_ulke_kodu = 4008,
	[Description("Sevk.sevk_adr_tel_bolge_kodu")]
	sevk_adr_tel_bolge_kodu = 4009,
	[Description("Sevk.sevk_adr_tel_no1")]
	sevk_adr_tel_no1 = 4010,
	[Description("Sevk.sevk_adr_tel_no2")]
	sevk_adr_tel_no2 = 4011,
	[Description("Sevk.sevk_adr_tel_faxno")]
	sevk_adr_tel_faxno = 4012,
	[Description("Sevk.sevk_adr_tel_modem")]
	sevk_adr_tel_modem = 4013,
	[Description("Sevk.sevk_adr_yon_kodu")]
	sevk_adr_yon_kodu = 4014,
	[Description("Sevk.sevk_adr_temsilci_kodu")]
	sevk_adr_temsilci_kodu = 4015,
	[Description("Sevk.sevk_adr_ozel_not")]
	sevk_adr_ozel_not = 4016,
	[Description("Sevk.sevk_adr_ziyaretgunu")]
	sevk_adr_ziyaretgunu = 4017,
	[Description("Sevk.sevk_adr_gps_enlem")]
	sevk_adr_gps_enlem = 4018,
	[Description("Sevk.sevk_adr_gps_boylam")]
	sevk_adr_gps_boylam = 4019,
	[Description("Sevk.sevk_adr_uzaklik_kodu")]
	sevk_adr_uzaklik_kodu = 4020,
	[Description("Sevk.sevk_adr_ziyaretperyodu")]
	sevk_adr_ziyaretperyodu = 4021,
	[Description("Sevk.sevk_adr_ziyarethaftasi")]
	sevk_adr_ziyarethaftasi = 4022,
	[Description("Sevk.sevk_adr_ziygunu2_1")]
	sevk_adr_ziygunu2_1 = 4023,
	[Description("Sevk.sevk_adr_ziygunu2_2")]
	sevk_adr_ziygunu2_2 = 4024,
	[Description("Sevk.sevk_adr_ziygunu2_3")]
	sevk_adr_ziygunu2_3 = 4025,
	[Description("Sevk.sevk_adr_ziygunu2_4")]
	sevk_adr_ziygunu2_4 = 4026,
	[Description("Sevk.sevk_adr_ziygunu2_5")]
	sevk_adr_ziygunu2_5 = 4027,
	[Description("Sevk.sevk_adr_ziygunu2_6")]
	sevk_adr_ziygunu2_6 = 4028,
	[Description("Sevk.sevk_adr_ziygunu2_7")]
	sevk_adr_ziygunu2_7 = 4029,
	[Description("Hedef depo (Depo adı)")]
	hedefdepo_depo_adi = 5000,
	[Description("Hedef depo (Depo no)")]
	hedefdepo_depo_no = 5001
}
