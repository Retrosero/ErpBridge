using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSiparis;

public interface RaporStokSiparisGruplandirmaListener
{
	void OnRaporStokSiparisGrupSelected(enum_gruplandirma_secenekleri grup, string tag);
}
