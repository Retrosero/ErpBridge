using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.Data.Sql;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Cari_Personel_Secimi_V16 : F10_Base_V16
{
	public List<CariPersonel> _selecteditems;

	public void Setup(MikroUygulamaBilgileri mikrouygulamabilgileri, string DefaultChoose, string SearchString, bool AllowMultiSelect, bool IlkAramaAktifOlsun)
	{
		Text = "Cari personel seçimi";
		_tag = "caripersonelsecimi";
		_tableid = 104;
		_tabloadi = "CARI_PERSONEL_TANIMLARI";
		_chooseprefix = "CARI_PERSONEL_TANIMLARI_CHOOSE_";
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
		_selecteditems = new List<CariPersonel>();
		foreach (Guid selecteditemsguid in _selecteditemsguids)
		{
			_selecteditems.Add(CariPersonelData.GetCariPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, selecteditemsguid));
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
