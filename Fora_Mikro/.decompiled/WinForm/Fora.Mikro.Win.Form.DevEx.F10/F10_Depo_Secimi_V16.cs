using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Depolar;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Depo_Secimi_V16 : F10_Base_V16
{
	public List<Depo> _selecteditems;

	public void Setup(MikroUygulamaBilgileri mikrouygulamabilgileri, string DefaultChoose, string SearchString, bool AllowMultiSelect, bool IlkAramaAktifOlsun)
	{
		Text = "Depo seçimi";
		_tag = "deposecimi";
		_tableid = 111;
		_tabloadi = "DEPOLAR";
		_chooseprefix = "DEPOLAR_CHOOSE_";
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
		_selecteditems = new List<Depo>();
		foreach (Guid selecteditemsguid in _selecteditemsguids)
		{
			_selecteditems.Add(DepoData.GetDepoByGuid(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, selecteditemsguid));
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
