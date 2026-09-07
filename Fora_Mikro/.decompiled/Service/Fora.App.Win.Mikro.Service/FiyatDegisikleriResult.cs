using System.Collections.Generic;
using Fora.Mikro.Stoklar;

namespace Fora.App.Win.Mikro.Service;

public class FiyatDegisikleriResult
{
	public List<StokItemBase> liste { get; set; }

	public string sonuc { get; set; }

	public string hatamesaji { get; set; }
}
