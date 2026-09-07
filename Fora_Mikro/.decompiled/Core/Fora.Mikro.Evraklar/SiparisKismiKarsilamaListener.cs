using Fora.Mikro.Stoklar;

namespace Fora.Mikro.Evraklar;

public interface SiparisKismiKarsilamaListener
{
	void OnSiparisKismiKarsilama(object sender, Stok stok, string tag);
}
