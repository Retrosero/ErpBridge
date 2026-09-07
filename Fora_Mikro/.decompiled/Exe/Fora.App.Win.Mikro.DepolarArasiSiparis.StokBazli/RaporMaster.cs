using System.Collections.Generic;

namespace Fora.App.Win.Mikro.DepolarArasiSiparis.StokBazli;

public class RaporMaster
{
	public string StokKodu { get; set; }

	public string StokIsmi { get; set; }

	public string Birim1Adi { get; set; }

	public double MerkezDepoMiktari { get; set; }

	public List<RaporDetail> Detaylar { get; set; }

	public double AcikSiparisMiktari
	{
		get
		{
			double num = 0.0;
			foreach (RaporDetail item in Detaylar)
			{
				num += item.KalanMiktar;
			}
			return num;
		}
	}

	public double EksikMiktar
	{
		get
		{
			double num = 0.0;
			num = AcikSiparisMiktari - MerkezDepoMiktari;
			if (num < 0.0)
			{
				num = 0.0;
			}
			return num;
		}
	}
}
