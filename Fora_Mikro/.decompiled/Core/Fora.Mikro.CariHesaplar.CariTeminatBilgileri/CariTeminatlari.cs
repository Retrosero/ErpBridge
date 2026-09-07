using System.Collections.Generic;

namespace Fora.Mikro.CariHesaplar.CariTeminatBilgileri;

public class CariTeminatlari
{
	public List<CariTeminat> _teminatlar;

	public CariTeminatlari()
	{
		_teminatlar = new List<CariTeminat>();
	}

	public CariTeminatlari(List<CariTeminat> teminatlar)
	{
		_teminatlar = teminatlar;
	}

	public double GetToplamTeminat()
	{
		double num = 0.0;
		foreach (CariTeminat item in _teminatlar)
		{
			num += item.Tutar;
		}
		return num;
	}

	public double GetTeminatTutari(enum_cari_teminat_tipleri teminat_tipi)
	{
		double num = 0.0;
		foreach (CariTeminat item in _teminatlar)
		{
			if (item.teminat_tipi == teminat_tipi)
			{
				num += item.Tutar;
			}
		}
		return num;
	}
}
