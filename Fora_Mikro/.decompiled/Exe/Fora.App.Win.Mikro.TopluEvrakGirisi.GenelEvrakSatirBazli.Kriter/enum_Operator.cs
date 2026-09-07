using System.ComponentModel;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public enum enum_Operator
{
	[Description("Eşittir")]
	Esit,
	[Description("Eşit değildir")]
	EsitDegil,
	[Description("(Sayı) Büyüktür")]
	Buyuk,
	[Description("(Sayı) Büyük eşittir")]
	BuyukEsit,
	[Description("(Sayı) Küçüktür")]
	Kucuk,
	[Description("(Sayı) Küçük eşittir")]
	KucukEsit,
	[Description("(Metin) İçerir")]
	Icerir,
	[Description("(Metin) İçermez")]
	Icermez,
	[Description("(Metin) İle başlar")]
	IleBaslar,
	[Description("(Metin) İle biter")]
	IleBiter
}
