using System;
using System.Collections.Generic;

namespace Fora.Mikro.Stoklar;

public class StokListItemV2
{
	public int SepetRECno { get; set; }

	public int sto_RECno { get; set; }

	public string sto_kod { get; set; }

	public string sto_isim { get; set; }

	public double miktar { get; set; }

	public double birimfiyatbrut { get; set; }

	public double birimfiyatnet { get; set; }

	public string fiyatdovizcinsi { get; set; }

	public DateTime terminsuresi { get; set; }

	public string depoadi { get; set; }

	public double depomevcudu { get; set; }

	public string secili_birim { get; set; }

	public List<string> birimler { get; set; }

	public int depo_no { get; set; }

	public StokListItemV2()
	{
		miktar = 0.0;
		depo_no = 1;
	}
}
