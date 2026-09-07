namespace Fora.Mikro.CariHesaplar;

public interface CariListener
{
	void OnCariSelected(Cari cari, string tag);

	void OnCariLongPress(Cari cari, string tag);
}
