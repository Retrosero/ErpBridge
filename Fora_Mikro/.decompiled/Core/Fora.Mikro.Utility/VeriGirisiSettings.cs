namespace Fora.Mikro.Utility;

public class VeriGirisiSettings
{
	public enum enum_VeriTipi
	{
		Text,
		Integer,
		Double
	}

	public double TextSize;

	public string BaslikOzel;

	public string Tag;

	public object DefaultDeger;

	public int MaxTextLengt;

	public enum_VeriTipi VeriTipi;

	public double GenislikYuzdesi;

	public double YukseklikYuzdesi;

	public VeriGirisiSettings()
	{
		VeriTipi = enum_VeriTipi.Text;
		MaxTextLengt = 0;
		BaslikOzel = "";
		TextSize = 14.0;
		Tag = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
