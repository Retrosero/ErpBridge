using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public interface RaporYapilacakTahsilatlarGruplandirmaListener
{
	void OnRaporYapilacakTahsilatlarGrupSelected(enum_yapilacak_tahsilatlar_gruplandirma_secenekleri grup, string tag);
}
