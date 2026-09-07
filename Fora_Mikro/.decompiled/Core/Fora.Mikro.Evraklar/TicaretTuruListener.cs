using Fora.Mikro.CariHesapHareket;

namespace Fora.Mikro.Evraklar;

public interface TicaretTuruListener
{
	void OnTicaretTuruSelected(enum_cha_ticaret_turu ticaretturu, string tag);
}
