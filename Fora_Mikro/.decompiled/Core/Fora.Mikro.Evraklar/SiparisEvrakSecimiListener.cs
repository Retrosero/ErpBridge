namespace Fora.Mikro.Evraklar;

public interface SiparisEvrakSecimiListener
{
	void OnSiparisEvrakSelected(object sender, SiparisEvrakListItem siparis, string tag);
}
