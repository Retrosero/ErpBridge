namespace Fora.Mikro.Stoklar;

public interface StokDetayListener
{
	void OnStokDetayChanged(string carikodu, int fiyatlisteno, int depono, int firmano);
}
