namespace Fora.Mikro.Bankalar;

public interface YerelBankaListener
{
	void OnYerelBankaSelected(string bankkod_kod, string bankkod_bankadi, string tag);
}
