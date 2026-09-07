namespace Fora.Mikro.Stoklar;

public interface StokListener
{
	void OnStokSelected(object sender, StokListItem stok, string tag);

	void OnStokLongPress(object sender, StokListItem stok, string tag);
}
