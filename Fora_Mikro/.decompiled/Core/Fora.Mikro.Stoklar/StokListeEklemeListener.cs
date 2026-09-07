using System.Collections.Generic;

namespace Fora.Mikro.Stoklar;

public interface StokListeEklemeListener
{
	void OnStokListeEkleme(object sender, List<StokListItemFiyatveMiktarli> stoklar, string tag);
}
