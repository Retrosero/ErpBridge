using System;
using System.Collections.Generic;

namespace Fora.Mikro.Rapor.HareketsizCariler;

public class RaporHareketsizCarilerOlusturTask
{
	public delegate void RaporHareketsizCarilerOlusturTaskBittiHandler(object sender, string Cevap, List<RaporHareketsizCariler> Items);

	private string _Cevap;

	public event RaporHareketsizCarilerOlusturTaskBittiHandler OnRaporHareketsizCarilerOlusturTaskBitti;

	public void Baslat()
	{
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		List<RaporHareketsizCariler> items = new List<RaporHareketsizCariler>();
		try
		{
			items = RaporHareketsizCarilerSqlite.GetHareketsizCariler(AppBase.GetDbConnectionSync(), AppBase.KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString);
		}
		catch (Exception ex)
		{
			_Cevap = "Hata : " + ex.ToString();
		}
		this.OnRaporHareketsizCarilerOlusturTaskBitti(this, _Cevap, items);
	}
}
