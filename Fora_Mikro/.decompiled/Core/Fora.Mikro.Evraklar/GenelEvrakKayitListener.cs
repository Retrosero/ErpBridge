namespace Fora.Mikro.Evraklar;

public interface GenelEvrakKayitListener
{
	void OnKaydetClicked();

	void OnKdvSilClicked();

	void OnIskontoDuzenleClicked();

	void OnIskontoChanged(int Iskonto_1_UygulamaSekli, double Iskonto_1_YuzdeVeyaMiktar, int Iskonto_2_UygulamaSekli, double Iskonto_2_YuzdeVeyaMiktar, int Iskonto_3_UygulamaSekli, double Iskonto_3_YuzdeVeyaMiktar, int Iskonto_4_UygulamaSekli, double Iskonto_4_YuzdeVeyaMiktar, int Iskonto_5_UygulamaSekli, double Iskonto_5_YuzdeVeyaMiktar, int Iskonto_6_UygulamaSekli, double Iskonto_6_YuzdeVeyaMiktar);

	void OnKoliEtiketiYazdirClicked();

	void OnKoliEtiketiFormChanged(string YeniForm);

	void OnKoliEtiketiMetin1Changed(string yenimetin);

	void OnKoliEtiketiMetin2Changed(string yenimetin);

	void OnKoliEtiketiMetin3Changed(string yenimetin);

	void OnKoliEtiketiMetin4Changed(string yenimetin);

	void OnKoliEtiketiMetin5Changed(string yenimetin);

	void OnKoliEtiketiOtomatikYazdirClick();

	void OnCekiListesiGosterClicked();

	void OnFiyatEtiketiFormChanged(string YeniForm);

	void OnFiyatEtiketiYazdirClicked();

	void OnFiyatEtiketiOtomatikYazdirClick();
}
