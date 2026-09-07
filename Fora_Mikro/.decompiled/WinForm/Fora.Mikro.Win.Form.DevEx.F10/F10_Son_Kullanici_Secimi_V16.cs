using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Stoklar.SonKullanicilar;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Son_Kullanici_Secimi_V16 : F10_Base_V16
{
	public List<SonKullanici> _selecteditems;

	public void Setup(MikroUygulamaBilgileri mikrouygulamabilgileri, string DefaultChoose, string SearchString, bool AllowMultiSelect, bool IlkAramaAktifOlsun)
	{
		Text = "Son kullanıcı seçimi";
		_tag = "sonkullanicisecimi";
		_tableid = 95;
		_tabloadi = "SON_KULLANICILAR";
		_chooseprefix = "SON_KULLANICILAR_CHOOSE_";
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
		_selecteditems = new List<SonKullanici>();
		foreach (Guid selecteditemsguid in _selecteditemsguids)
		{
			_selecteditems.Add(SonKullaniciData.GetSonKullanici(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, selecteditemsguid));
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
