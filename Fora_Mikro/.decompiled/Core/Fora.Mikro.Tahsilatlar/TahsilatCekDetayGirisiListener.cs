namespace Fora.Mikro.Tahsilatlar;

public interface TahsilatCekDetayGirisiListener
{
	void OnCekDetayChanged(string cekno, string hesapno, string aciklama, string tag);
}
