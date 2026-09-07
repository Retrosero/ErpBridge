namespace Fora.Mikro.Stoklar.StokPaketleri;

public interface StokPaketListener
{
	void OnStokPaketSelected(object sender, string pak_kod, int paket_miktar, string tag);

	void OnStokPaketAlternatifSelected(object sender, string pak_kod, int paket_miktar, int satir_no, string tag, bool barkod_okuma);
}
