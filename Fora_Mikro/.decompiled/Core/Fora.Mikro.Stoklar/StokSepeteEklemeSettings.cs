namespace Fora.Mikro.Stoklar;

public class StokSepeteEklemeSettings
{
	public string BaslikOzel { get; set; }

	public string Tag { get; set; }

	public double GenislikYuzdesi { get; set; }

	public double YukseklikYuzdesi { get; set; }

	public StokSepeteEklemeSettings()
	{
		BaslikOzel = "";
		Tag = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
