using System;

namespace Fora.Mikro.Utility;

public class ZamanAraligi
{
	private DateTime _baslangic { get; set; }

	private DateTime _bitis { get; set; }

	public DateTime Baslangic
	{
		get
		{
			return _baslangic;
		}
		set
		{
			_baslangic = value;
		}
	}

	public DateTime Bitis
	{
		get
		{
			return _bitis;
		}
		set
		{
			_bitis = value;
		}
	}

	public ZamanAraligi(DateTime Baslangic, DateTime Bitis)
	{
		_baslangic = Baslangic;
		_bitis = Bitis;
	}

	public ZamanAraligi(enum_zaman_araligi ZamanAraligi)
	{
		SetZamanAraligi(ZamanAraligi);
	}

	private DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
	{
		int num = dt.DayOfWeek - startOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		return dt.AddDays(-1 * num).Date;
	}

	public void SetZamanAraligi(enum_zaman_araligi ZamanAraligi)
	{
		DateTime now = DateTime.Now;
		DateTime dateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
		DateTime bitis = dateTime.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime baslangic = new DateTime(dateTime.Year, dateTime.Month, 1);
		DateTime bitis2 = baslangic.AddMonths(1).AddDays(-1.0).AddHours(23.0)
			.AddMinutes(59.0)
			.AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime baslangic2 = new DateTime(dateTime.Year, 1, 1);
		DateTime bitis3 = baslangic2.AddYears(1).AddMilliseconds(-1.0);
		switch (ZamanAraligi)
		{
		case enum_zaman_araligi.BuAy:
			_baslangic = baslangic;
			_bitis = bitis2;
			break;
		case enum_zaman_araligi.Bugun:
			_baslangic = dateTime;
			_bitis = now;
			break;
		case enum_zaman_araligi.BuHafta:
			_baslangic = StartOfWeek(dateTime, DayOfWeek.Monday);
			_bitis = now;
			break;
		case enum_zaman_araligi.BuYil:
			_baslangic = baslangic2;
			_bitis = bitis3;
			break;
		case enum_zaman_araligi.Dun:
			_baslangic = dateTime.AddDays(-1.0);
			_bitis = bitis.AddDays(-1.0);
			break;
		case enum_zaman_araligi.GecenAy:
			_baslangic = baslangic.AddMonths(-1);
			_bitis = baslangic.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_zaman_araligi.GecenHafta:
			_baslangic = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(-7.0);
			_bitis = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_zaman_araligi.GecenYil:
			_baslangic = baslangic2.AddYears(-1);
			_bitis = baslangic2.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_zaman_araligi.Son24Saat:
			_baslangic = now.AddHours(-24.0);
			_bitis = now;
			break;
		case enum_zaman_araligi.Son30Gun:
			_baslangic = dateTime.AddDays(-29.0);
			_bitis = bitis;
			break;
		case enum_zaman_araligi.Son365Gun:
			_baslangic = dateTime.AddDays(-364.0);
			_bitis = bitis;
			break;
		case enum_zaman_araligi.Son7Gun:
			_baslangic = dateTime.AddDays(-6.0);
			_bitis = bitis;
			break;
		case enum_zaman_araligi.Son3Ay:
			_baslangic = baslangic.AddMonths(-2);
			_bitis = bitis;
			break;
		case enum_zaman_araligi.Son60Gun:
			_baslangic = dateTime.AddDays(-59.0);
			_bitis = bitis;
			break;
		case enum_zaman_araligi.Son90Gun:
			_baslangic = dateTime.AddDays(-89.0);
			_bitis = bitis;
			break;
		case enum_zaman_araligi.Son6Ay:
			_baslangic = baslangic.AddMonths(-5);
			_bitis = bitis;
			break;
		case enum_zaman_araligi.Son12Ay:
			_baslangic = baslangic.AddMonths(-11);
			_bitis = bitis;
			break;
		}
	}
}
