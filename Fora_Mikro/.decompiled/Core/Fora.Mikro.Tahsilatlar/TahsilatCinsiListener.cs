using Fora.Mikro.Evraklar;

namespace Fora.Mikro.Tahsilatlar;

public interface TahsilatCinsiListener
{
	void OnTahsilatCinsiSelected(enum_tahsilat_cinsi tahsilatcinsi, string tag);
}
