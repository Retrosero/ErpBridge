using System.Collections.Generic;
using System.Windows.Forms;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Kargolar;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Kargo_Secimi : F10_Base
{
	public List<Kargo> _selecteditems;

	public void Setup(MikroUygulamaBilgileri mikrouygulamabilgileri, string DefaultChoose, string SearchString, bool AllowMultiSelect, bool IlkAramaAktifOlsun)
	{
		Text = "Kargo seçimi";
		_tag = "kargosecimi";
		_tableid = 261;
		_tabloadi = "KARGO_TANIMLARI";
		_chooseprefix = "KARGO_TANIMLARI_CHOOSE_";
		_defaultchoose = DefaultChoose;
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_searchstring = SearchString;
		_gorunum = "otomatik";
		_allowmultiselect = AllowMultiSelect;
		_AramaYapilabilir = true;
		_IlkAramaAktifOlsun = IlkAramaAktifOlsun;
		_CustomWhereString = "";
	}

	protected override void ItemSelected()
	{
		_selecteditems = new List<Kargo>();
		foreach (int selecteditemsrecno in _selecteditemsrecnos)
		{
			_selecteditems.Add(KargoData.GetKargo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, selecteditemsrecno));
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
