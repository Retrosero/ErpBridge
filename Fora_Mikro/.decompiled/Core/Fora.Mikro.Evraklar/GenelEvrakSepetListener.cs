using Fora.Mikro.Siparis;

namespace Fora.Mikro.Evraklar;

public interface GenelEvrakSepetListener
{
	void OnSepetArtiClickedStandartGorunum();

	void OnSepetArtiClickedListeGorunum();

	void OnSepetArtiClickedPaket();

	void OnSepetArtiClickedBarkod();

	void OnSepetItemClicked(int Position);

	void OnSepetMiktarChanged(int Position, double yeni_miktar, int birim_pntr);

	void OnSepetArtiClickedSonSiparis(enum_sip_tip SiparisTipi, enum_sip_cins SiparisCinsi);
}
