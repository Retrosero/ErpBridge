using Fora.Mikro.CariHesapHareket;

namespace Fora.Mikro.Evraklar;

public interface NormalIadeListener
{
	void OnNormalIadeSelected(enum_cha_normal_Iade normaliade, string tag);
}
