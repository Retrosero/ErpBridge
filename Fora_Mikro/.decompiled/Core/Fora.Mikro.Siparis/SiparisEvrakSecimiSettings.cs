namespace Fora.Mikro.Siparis;

public class SiparisEvrakSecimiSettings
{
	public bool MultiSelect;

	public string SearchString;

	public string BaslikOzel { get; set; }

	public string Tag { get; set; }

	public double GenislikYuzdesi { get; set; }

	public double YukseklikYuzdesi { get; set; }

	public enum_sip_orderby SiparisSiralamaSekli { get; set; }

	public enum_sip_tip SiparisTipi { get; set; }

	public enum_sip_cins SiparisCinsi { get; set; }

	public string CariKodu { get; set; }

	public siparis_teslim_durumu TeslimDurumu { get; set; }

	public string EvrakSeri { get; set; }

	public SiparisEvrakSecimiSettings()
	{
		BaslikOzel = "";
		Tag = "";
		CariKodu = "";
		EvrakSeri = "";
		SearchString = "";
		MultiSelect = false;
		SiparisSiralamaSekli = enum_sip_orderby.SiparisTarihi;
		TeslimDurumu = siparis_teslim_durumu.Bekleyenler;
		GenislikYuzdesi = 95.0;
		YukseklikYuzdesi = 95.0;
	}
}
