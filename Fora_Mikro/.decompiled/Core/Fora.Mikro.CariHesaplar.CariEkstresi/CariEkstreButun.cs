using System;
using System.Collections.Generic;

namespace Fora.Mikro.CariHesaplar.CariEkstresi;

public class CariEkstreButun
{
	public Cari cari { get; set; }

	public DateTime tarih { get; set; }

	public List<CariEkstreGrup> gruplar { get; set; }
}
