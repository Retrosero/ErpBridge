namespace Fora.Mikro.Evraklar;

public enum enum_evrak_siparis_ekleme_sonuc
{
	UrunEklendi = 0,
	UrunSiparisteYok = 2,
	UrunMiktariSiparisMiktarindanFazla = 3,
	MaksimumSatirSayisinaUlasildi = 4,
	StokSeviyesiYeterliDegil = 5,
	MiktarSifirOlamaz = 6,
	SeriNoGirilmekZorunda = 7,
	SeriNoDahaOnceEklenmis = 8,
	SiparisKarsilamaDisiGirisYapilamaz = 9,
	GenelHata = 10
}
