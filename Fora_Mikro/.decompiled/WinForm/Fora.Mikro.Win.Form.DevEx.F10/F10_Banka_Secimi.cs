using System.Collections.Generic;
using System.Windows.Forms;
using Fora.Mikro.Bankalar;
using Fora.Mikro.Data.Sql;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Banka_Secimi : F10_Base
{
	public List<Banka> _selecteditems;

	public void Setup(MikroUygulamaBilgileri mikrouygulamabilgileri, string DefaultChoose, string SearchString, bool AllowMultiSelect, bool IlkAramaAktifOlsun)
	{
		Text = "Banka seçimi";
		_tag = "bankasecimi";
		_tableid = 52;
		_tabloadi = "BANKALAR";
		_chooseprefix = "BANKALAR_CHOOSE_";
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
		_selecteditems = new List<Banka>();
		foreach (int selecteditemsrecno in _selecteditemsrecnos)
		{
			_selecteditems.Add(BankaData.GetBanka(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, selecteditemsrecno));
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
