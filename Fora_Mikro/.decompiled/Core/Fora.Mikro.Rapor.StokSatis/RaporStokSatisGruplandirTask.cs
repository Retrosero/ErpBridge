using System.Collections.Generic;

namespace Fora.Mikro.Rapor.StokSatis;

public class RaporStokSatisGruplandirTask
{
	public delegate void RaporStokSatisGruplandirBittiHandler(object sender, List<RaporStokSatisListItem> items);

	private RaporStokSatisSonuc _rapor_sonuc;

	private RaporStokSatisSonucHam _rapor_sonuc_ham;

	public event RaporStokSatisGruplandirBittiHandler OnRaporStokSatisGruplandirTaskBitti;

	public void Baslat(RaporStokSatisSonuc rapor_sonuc, RaporStokSatisSonucHam rapor_sonuc_ham)
	{
		_rapor_sonuc = rapor_sonuc;
		_rapor_sonuc_ham = rapor_sonuc_ham;
		Main();
	}

	public void Main()
	{
		List<RaporStokSatisListItem> items = RaporStokSatisHelper.RaporGruplandir(_rapor_sonuc, _rapor_sonuc_ham);
		this.OnRaporStokSatisGruplandirTaskBitti(this, items);
	}
}
