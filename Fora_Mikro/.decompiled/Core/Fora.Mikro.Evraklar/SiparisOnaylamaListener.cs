namespace Fora.Mikro.Evraklar;

public interface SiparisOnaylamaListener
{
	void OnOnaylaClicked(int itemposition, string tag);

	void OnOnaylamaClicked(int itemposition, string kapama_neden_kod, string tag);

	void OnEvrakDetayAc(int itemposition, string tag);
}
