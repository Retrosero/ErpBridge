namespace Fora.Mikro.DisTicaret;

public class Ihracat
{
	public string ihr_kodu { get; set; }

	public string ihr_ismi { get; set; }

	public string ihr_Satici { get; set; }

	public Ihracat()
	{
		ihr_kodu = "";
		ihr_ismi = "";
		ihr_Satici = "";
	}
}
