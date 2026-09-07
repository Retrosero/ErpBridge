using System.ComponentModel;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public enum enum_KriterDegistirmeAlanlari
{
	[Description("Kayıt ID")]
	KayitID,
	[Description("Evrak tipi")]
	evraktipi,
	[Description("Normal/İade")]
	normaliade,
	[Description("Açık/Kapalı")]
	kapamasekli,
	[Description("Ticaret türü")]
	ticaretturu,
	[Description("Evrak tarihi")]
	evraktarih,
	[Description("Evrak no seri")]
	evraknoseri,
	[Description("Evrak no sıra")]
	evraknosira,
	[Description("Belge no")]
	belgeno,
	[Description("Belge tarihi")]
	belgetarih,
	[Description("Ödeme planı")]
	odemeplani,
	[Description("Fiyat listesi")]
	fiyatlistesi,
	[Description("Döviz cinsi")]
	dovizcinsi,
	[Description("Kur")]
	kur,
	[Description("Kaynak depo")]
	kaynakdepo,
	[Description("Proje kodu")]
	proje,
	[Description("Sorumluluk merkezi kodu")]
	sorumlulukmerkezi,
	[Description("Temsilci kodu")]
	temsilcikodu,
	[Description("Firma no")]
	firma,
	[Description("Şube no")]
	sube,
	[Description("Sevk/teslim tarihi")]
	sevkteslimtarihi,
	[Description("Sevk adres no")]
	sevkadresno,
	[Description("Kapama hesap kodu")]
	kapamahesapkodu,
	[Description("Fatura açıklama")]
	faturaaciklama,
	[Description("Açıklama 1")]
	aciklama1,
	[Description("Açıklama 2")]
	aciklama2,
	[Description("Açıklama 3")]
	aciklama3,
	[Description("Açıklama 4")]
	aciklama4,
	[Description("Açıklama 5")]
	aciklama5,
	[Description("Açıklama 6")]
	aciklama6,
	[Description("Açıklama 7")]
	aciklama7,
	[Description("Açıklama 8")]
	aciklama8,
	[Description("Açıklama 9")]
	aciklama9,
	[Description("Açıklama 10")]
	aciklama10,
	[Description("Satır cinsi")]
	satircinsi,
	[Description("Miktar")]
	miktar,
	[Description("Miktar 2")]
	miktar2,
	[Description("Vergi pntr")]
	vergi_pntr,
	[Description("Fiyat farkı mı")]
	fiyat_fark_mi,
	[Description("Kriter metin 1")]
	kriter_string1,
	[Description("Kriter metin 2")]
	kriter_string2,
	[Description("Kriter metin 3")]
	kriter_string3,
	[Description("Kriter metin 4")]
	kriter_string4,
	[Description("Kriter metin 5")]
	kriter_string5,
	[Description("Kriter sayı 1")]
	kriter_double1,
	[Description("Kriter sayı 2")]
	kriter_double2,
	[Description("Kriter sayı 3")]
	kriter_double3,
	[Description("Kriter sayı 4")]
	kriter_double4,
	[Description("Kriter sayı 5")]
	kriter_double5,
	[Description("Kriter evet/hayır 1")]
	kriter_bool1,
	[Description("Kriter evet/hayır 2")]
	kriter_bool2,
	[Description("Kriter evet/hayır 3")]
	kriter_bool3,
	[Description("Kriter evet/hayır 4")]
	kriter_bool4,
	[Description("Kriter evet/hayır 5")]
	kriter_bool5,
	[Description("Birim fiyat brüt fiyat")]
	birimfiyat_brut_fiyat,
	[Description("Birim fiyat iskonto 1 uygulama şekli")]
	birimfiyat_iskonto1_uygulama_sekli,
	[Description("Birim fiyat iskonto 1 yüzde/tutar")]
	birimfiyat_iskonto1_yuzdeveyatutar,
	[Description("Birim fiyat iskonto 2 uygulama şekli")]
	birimfiyat_iskonto2_uygulama_sekli,
	[Description("Birim fiyat iskonto 2 yüzde/tutar")]
	birimfiyat_iskonto2_yuzdeveyatutar,
	[Description("Birim fiyat iskonto 3 uygulama şekli")]
	birimfiyat_iskonto3_uygulama_sekli,
	[Description("Birim fiyat iskonto 3 yüzde/tutar")]
	birimfiyat_iskonto3_yuzdeveyatutar,
	[Description("Birim fiyat iskonto 4 uygulama şekli")]
	birimfiyat_iskonto4_uygulama_sekli,
	[Description("Birim fiyat iskonto 4 yüzde/tutar")]
	birimfiyat_iskonto4_yuzdeveyatutar,
	[Description("Birim fiyat iskonto 5 uygulama şekli")]
	birimfiyat_iskonto5_uygulama_sekli,
	[Description("Birim fiyat iskonto 5 yüzde/tutar")]
	birimfiyat_iskonto5_yuzdeveyatutar,
	[Description("Birim fiyat iskonto 6 uygulama şekli")]
	birimfiyat_iskonto6_uygulama_sekli,
	[Description("Birim fiyat iskonto 6 yüzde/tutar")]
	birimfiyat_iskonto6_yuzdeveyatutar,
	[Description("Birim fiyat ÖTV uygulama şekli")]
	birimfiyat_otv_uygulama_sekli,
	[Description("Birim fiyat ÖTV yüzde/tutar")]
	birimfiyat_otv_yuzdeveyatutar,
	[Description("Birim fiyat ÖTV vergi pntr")]
	birimfiyat_otv_vergipntr,
	[Description("Cari kodu")]
	cari_kod,
	[Description("Stok/Hizmet kodu")]
	stokhizmetkodu,
	[Description("Hedef depo")]
	hedefdepo
}
