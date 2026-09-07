namespace Fora.Mikro.OdemeEmri;

public class OdemeEmirleri
{
	public string sck_refno { get; set; }

	public enum_sck_tip sck_tip { get; set; }

	public double sck_tutar { get; set; }

	public enum_sck_nerede_cari_cins sck_nerede_cari_cins { get; set; }

	public string sck_nerede_cari_kodu { get; set; }

	public string sck_srmmrk { get; set; }

	public string sck_projekodu { get; set; }

	public OdemeEmirleri()
	{
		sck_refno = "";
		sck_tip = enum_sck_tip.KendiCekimiz;
		sck_tutar = 0.0;
		sck_nerede_cari_cins = enum_sck_nerede_cari_cins.Bankamiz;
		sck_nerede_cari_kodu = "";
		sck_srmmrk = "";
		sck_projekodu = "";
	}
}
