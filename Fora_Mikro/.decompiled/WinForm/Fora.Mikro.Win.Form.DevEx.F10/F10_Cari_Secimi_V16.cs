using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Cari_Secimi_V16 : F10_Base_V16
{
	public List<Cari> _selecteditems;

	public void Setup(MikroUygulamaBilgileri mikrouygulamabilgileri, string DefaultChoose, string SearchString, bool AllowMultiSelect, bool IlkAramaAktifOlsun)
	{
		Text = "Cari hesap seçimi";
		_tag = "carisecimi";
		_tableid = 31;
		_tabloadi = "CARI_HESAPLAR";
		_chooseprefix = "CARI_HESAPLAR_CHOOSE_";
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
		_selecteditems = new List<Cari>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		foreach (Guid selecteditemsguid in _selecteditemsguids)
		{
			_selecteditems.Add(CariData.GetCariByGuid(sqlDB.Connection, selecteditemsguid, AdreslerTemsilciyeGore: false, ""));
		}
		sqlDB.ConnectionClose();
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
