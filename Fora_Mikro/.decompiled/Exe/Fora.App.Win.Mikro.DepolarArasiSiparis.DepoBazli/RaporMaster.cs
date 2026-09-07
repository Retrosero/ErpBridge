using System.Collections.Generic;

namespace Fora.App.Win.Mikro.DepolarArasiSiparis.DepoBazli;

public class RaporMaster
{
	public int DepoNo { get; set; }

	public string DepoIsmi { get; set; }

	public bool Durum { get; set; }

	public double ToplamSiparisMiktari { get; set; }

	public List<RaporDetail> Detaylar { get; set; }
}
