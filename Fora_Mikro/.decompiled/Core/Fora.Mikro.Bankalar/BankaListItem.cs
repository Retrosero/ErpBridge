namespace Fora.Mikro.Bankalar;

public class BankaListItem : Banka
{
	public bool BakiyeBulundu { get; set; }

	public double Bakiye { get; set; }

	public bool Selected { get; set; }
}
