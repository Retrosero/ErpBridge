using Fora.Mikro.Enumler;

namespace Fora.Mikro.Rapor.TemsilciGunluk;

public interface RaporTemsilciGunlukMenuListener
{
	void OnTemsilciGunlukRaporuMenuSelected(enum_rapor_temsilci_gunluk_menu_secenekleri secenek, string tag);
}
