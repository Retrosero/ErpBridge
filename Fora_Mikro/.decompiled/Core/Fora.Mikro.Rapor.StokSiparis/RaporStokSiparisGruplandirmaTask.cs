using System;
using System.Collections.Generic;

namespace Fora.Mikro.Rapor.StokSiparis;

public class RaporStokSiparisGruplandirmaTask
{
	public delegate void RaporStokSiparisGruplandirmaTaskBittiHandler(object sender, List<RaporStokSiparisListItem> items);

	private RaporStokSiparisSonuc _rapor_sonuc;

	private RaporStokSiparisSonucHam _rapor_sonuc_ham;

	private string _Cevap;

	public event RaporStokSiparisGruplandirmaTaskBittiHandler OnRaporStokSiparisGruplandirmaTaskBitti;

	public void Baslat(RaporStokSiparisSonuc rapor_sonuc, RaporStokSiparisSonucHam rapor_sonuc_ham)
	{
		_rapor_sonuc = rapor_sonuc;
		_rapor_sonuc_ham = rapor_sonuc_ham;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		List<RaporStokSiparisListItem> items = new List<RaporStokSiparisListItem>();
		try
		{
			items = RaporStokSiparisHelper.RaporGruplandir(_rapor_sonuc, _rapor_sonuc_ham);
		}
		catch (Exception ex)
		{
			_Cevap = "Hata : " + ex.ToString();
		}
		this.OnRaporStokSiparisGruplandirmaTaskBitti(this, items);
	}
}
