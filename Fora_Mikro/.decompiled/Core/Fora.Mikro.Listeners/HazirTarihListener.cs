using Fora.Mikro.Utility;

namespace Fora.Mikro.Listeners;

public interface HazirTarihListener
{
	void OnHazirTarihSelected(ZamanAraligi zamanaraligi, string tag);
}
