namespace Fora.Mikro.ForaFirmaBilgileri;

public interface ForaFirmaSilmeListener
{
	void OnFirmaDeleted(string firmaid, string tag, bool SilFirmaTanimi, bool SilSenkronizasyonBilgisi, bool SilOfflineData, string Veritabani);
}
