namespace Fora.Mikro.Masraflar;

public interface MasrafHesabiListener
{
	void OnMasrafHesabiSelected(string masraf_hesap_kodu, string tag);
}
