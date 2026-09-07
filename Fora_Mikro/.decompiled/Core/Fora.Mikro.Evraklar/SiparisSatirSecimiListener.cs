using Fora.Mikro.Siparis;

namespace Fora.Mikro.Evraklar;

public interface SiparisSatirSecimiListener
{
	void OnSiparisSatirSelected(object sender, SIPARISLER siparis, string tag);
}
