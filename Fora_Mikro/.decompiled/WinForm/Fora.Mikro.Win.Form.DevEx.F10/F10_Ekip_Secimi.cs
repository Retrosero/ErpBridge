using System.Collections.Generic;
using System.Windows.Forms;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Ekipler;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Ekip_Secimi : F10_Base
{
	public List<Ekip> _selecteditems;

	public void Setup(MikroUygulamaBilgileri mikrouygulamabilgileri, string DefaultChoose, string SearchString, bool AllowMultiSelect, bool IlkAramaAktifOlsun)
	{
		Text = "Ekip seçimi";
		_tag = "ekipsecimi";
		_tableid = 260;
		_tabloadi = "EKIP_TANIMLARI";
		_chooseprefix = "EKIP_TANIMLARI_CHOOSE_";
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
		_selecteditems = new List<Ekip>();
		foreach (int selecteditemsrecno in _selecteditemsrecnos)
		{
			_selecteditems.Add(EkipData.GetEkip(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, selecteditemsrecno));
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
