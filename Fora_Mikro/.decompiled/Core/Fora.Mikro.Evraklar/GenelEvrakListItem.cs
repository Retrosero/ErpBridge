using System;

namespace Fora.Mikro.Evraklar;

public class GenelEvrakListItem
{
	public int cha_RECid_RECno { get; set; }

	public Guid cha_Guid { get; set; }

	public string cha_evrakno_seri { get; set; }

	public int cha_evrakno_sira { get; set; }

	public DateTime cha_tarihi { get; set; }

	public string cha_kod { get; set; }

	public string cari_unvan1 { get; set; }

	public string cari_unvan2 { get; set; }
}
