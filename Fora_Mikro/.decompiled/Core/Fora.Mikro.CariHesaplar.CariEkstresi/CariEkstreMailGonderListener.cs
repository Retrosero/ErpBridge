namespace Fora.Mikro.CariHesaplar.CariEkstresi;

public interface CariEkstreMailGonderListener
{
	void OnCariEkstreMailGonder(object sender, string tag, string alicilar, string konu, string mesaj, string format);
}
