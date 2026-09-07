using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Rapor.StokSatis;

public interface RaporStokSatisGruplandirmaListener
{
	void OnRaporStokSatisGrupSelected(enum_gruplandirma_secenekleri grup, string tag);
}
