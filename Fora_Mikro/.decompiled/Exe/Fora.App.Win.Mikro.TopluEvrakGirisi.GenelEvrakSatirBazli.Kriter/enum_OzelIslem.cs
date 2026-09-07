using System.ComponentModel;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public enum enum_OzelIslem
{
	[Description("Yok")]
	Yok,
	[Description("Sql den veri çek ve belirtilen alana yaz")]
	SqldenVeriCekveBelirtilenAlanaYaz,
	[Description("Satırı sil")]
	SatiriSil,
	[Description("Satırı aktarılmayacak olarak işaretle")]
	SatiriAktarilmayacakOlarakIsaretle,
	[Description("Uyarı mesajı ver")]
	UyariMesajiVer,
	[Description("Miktar 1 ile Miktar 2 yi yer değiştir")]
	Miktar1Miktar2YerDegistir
}
