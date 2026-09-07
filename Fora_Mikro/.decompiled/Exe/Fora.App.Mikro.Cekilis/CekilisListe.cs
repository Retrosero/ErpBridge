using System.Collections.Generic;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Utility;

namespace Fora.App.Mikro.Cekilis;

public class CekilisListe
{
	public Cari cari { get; set; }

	public List<GenelList> cekilis_numaralari { get; set; }
}
