namespace Fora.Mikro.DisTicaret;

public class Ithalat
{
	public string ith_kodu { get; set; }

	public string ith_ismi { get; set; }

	public string ith_satici { get; set; }

	public int ith_dovizcinsi { get; set; }

	public Ithalat()
	{
		ith_kodu = "";
		ith_ismi = "";
		ith_satici = "";
		ith_dovizcinsi = 0;
	}
}
