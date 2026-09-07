using System.Collections.Generic;

namespace Fora.Mikro.Rapor.StokEnvanter;

public class RaporStokEnvanterGruplandirTask
{
	public delegate void RaporStokEnvanterGruplandirBittiHandler(object sender, List<RaporStokEnvanterListItem> items);

	private RaporStokEnvanterSonuc _rapor_sonuc;

	private RaporStokEnvanterSonucHam _rapor_sonuc_ham;

	public event RaporStokEnvanterGruplandirBittiHandler OnRaporStokEnvanterGruplandirTaskBitti;

	public void Baslat(RaporStokEnvanterSonuc rapor_sonuc, RaporStokEnvanterSonucHam rapor_sonuc_ham)
	{
		_rapor_sonuc = rapor_sonuc;
		_rapor_sonuc_ham = rapor_sonuc_ham;
		Main();
	}

	public void Main()
	{
		List<RaporStokEnvanterListItem> items = RaporStokEnvanterHelper.RaporGruplandir(_rapor_sonuc, _rapor_sonuc_ham);
		this.OnRaporStokEnvanterGruplandirTaskBitti(this, items);
	}
}
