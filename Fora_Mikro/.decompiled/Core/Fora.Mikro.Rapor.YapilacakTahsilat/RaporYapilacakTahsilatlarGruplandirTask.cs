using System;
using System.Collections.Generic;
using Fora.Mikro.Dovizler;

namespace Fora.Mikro.Rapor.YapilacakTahsilat;

public class RaporYapilacakTahsilatlarGruplandirTask
{
	public delegate void RaporYapilacakTahsilatlarGruplandirTaskBittiHandler(object sender, List<RaporYapilacakTahsilatlarListItem> items);

	private RaporYapilacakTahsilatlarSonuc _rapor_sonuc;

	private RaporYapilacakTahsilatlarSonucHam _rapor_sonuc_ham;

	private DovizCinsiTanimlari _doviz_cinsleri;

	private string _Cevap;

	public event RaporYapilacakTahsilatlarGruplandirTaskBittiHandler OnRaporYapilacakTahsilatlarGruplandirTaskBitti;

	public void Baslat(RaporYapilacakTahsilatlarSonuc rapor_sonuc, RaporYapilacakTahsilatlarSonucHam rapor_sonuc_ham, DovizCinsiTanimlari doviz_cinsleri)
	{
		_rapor_sonuc = rapor_sonuc;
		_rapor_sonuc_ham = rapor_sonuc_ham;
		_doviz_cinsleri = doviz_cinsleri;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		List<RaporYapilacakTahsilatlarListItem> items = new List<RaporYapilacakTahsilatlarListItem>();
		try
		{
			items = RaporYapilacakTahsilatlarHelper.RaporGruplandir(_rapor_sonuc, _rapor_sonuc_ham, _doviz_cinsleri);
		}
		catch (Exception ex)
		{
			_Cevap = "Hata : " + ex.ToString();
		}
		this.OnRaporYapilacakTahsilatlarGruplandirTaskBitti(this, items);
	}
}
