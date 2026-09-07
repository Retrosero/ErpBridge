using System;

namespace Fora.Mikro.Utility;

public class TarihGirisiSettings
{
	public string BaslikOzel;

	public string Tag;

	public DateTime DefaultDeger;

	public double GenislikYuzdesi;

	public double YukseklikYuzdesi;

	public TarihGirisiSettings()
	{
		BaslikOzel = "";
		DefaultDeger = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		Tag = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
