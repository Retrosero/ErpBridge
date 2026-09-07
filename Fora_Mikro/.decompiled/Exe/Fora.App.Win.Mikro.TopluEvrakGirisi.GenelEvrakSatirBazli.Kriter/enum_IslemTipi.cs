using System.ComponentModel;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public enum enum_IslemTipi
{
	[Description("Değiştir")]
	Degistir,
	[Description("(Metin) Başına ekle")]
	BasinaEkle,
	[Description("(Metin) Sonuna ekle")]
	SonunaEkle,
	[Description("(Metin) Başından n kadar karakter kırp")]
	BasindanNkadarKarakterKirp,
	[Description("(Metin) Sonundan n kadar karakter kırp")]
	SonundanNkadarKarakterKirp,
	[Description("(Metin) Başından belirtilen karaktere kadar kırp")]
	BasindanBelirtilenKaraktereKadarKirp,
	[Description("(Metin) Sonundan belirtilen karaktere kadar kırp")]
	SonundanBelirtilenKaraktereKadarKirp,
	[Description("(Sayı) Topla")]
	Topla,
	[Description("(Sayı) Çıkart")]
	Cikart,
	[Description("(Sayı) Çarp")]
	Carp,
	[Description("(Sayı) Böl")]
	Bol,
	[Description("(Tarih) Gün ekle")]
	GunEkle,
	[Description("(Tarih) Gün çıkart")]
	GunCikart
}
