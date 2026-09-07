namespace Fora.Mikro.Bankalar;

public interface BankaListener
{
	void OnBankaSelected(Banka banka, string tag);
}
