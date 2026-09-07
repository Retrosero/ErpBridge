using Fora.Mikro.Enumler;

namespace Fora.Mikro.Kasalar;

public class Kasa
{
	public enum_kas_tip kas_tip { get; set; }

	public int kas_firma_no { get; set; }

	public string kas_kod { get; set; }

	public string kas_isim { get; set; }

	public string kas_muh_kod { get; set; }

	public int kas_doviz_cinsi { get; set; }

	public Kasa()
	{
		kas_tip = enum_kas_tip.NakitKasasi;
		kas_firma_no = 0;
		kas_kod = "";
		kas_isim = "";
		kas_muh_kod = "";
		kas_doviz_cinsi = 0;
	}
}
