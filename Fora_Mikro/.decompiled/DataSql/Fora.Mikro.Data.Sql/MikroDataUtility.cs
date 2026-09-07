using System;
using System.Runtime.InteropServices;
using Fora.Mikro.Rapor.Genel;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MikroDataUtility
{
	public static DateTime GetTarihCinsi(enum_tarih_cinsi tarihcinsi, bool BaslangicMi)
	{
		DateTime result = DateTime.Now;
		DateTime result2 = DateTime.Now;
		DateTime dateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
		DateTime dateTime2 = dateTime.AddHours(23.0).AddMinutes(59.0).AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime dateTime3 = new DateTime(dateTime.Year, dateTime.Month, 1);
		DateTime dateTime4 = dateTime3.AddMonths(1).AddDays(-1.0).AddHours(23.0)
			.AddMinutes(59.0)
			.AddSeconds(59.0)
			.AddMilliseconds(99.0);
		DateTime dateTime5 = new DateTime(dateTime.Year, 1, 1);
		DateTime dateTime6 = dateTime5.AddYears(1).AddMilliseconds(-1.0);
		switch (tarihcinsi)
		{
		case enum_tarih_cinsi.TumZamanlar:
			result = dateTime5.AddYears(-10);
			result2 = dateTime5.AddYears(10);
			break;
		case enum_tarih_cinsi.Dun:
			result = dateTime.AddDays(-1.0);
			result2 = dateTime2.AddDays(-1.0);
			break;
		case enum_tarih_cinsi.Bugun:
			result = dateTime;
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.BuHafta:
			result = StartOfWeek(dateTime, DayOfWeek.Monday);
			result2 = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(6.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.BuAy:
			result = dateTime3;
			result2 = dateTime4;
			break;
		case enum_tarih_cinsi.BuYil:
			result = dateTime5;
			result2 = dateTime6;
			break;
		case enum_tarih_cinsi.GecenHafta:
			result = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(-7.0);
			result2 = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GecenAy:
			result = dateTime3.AddMonths(-1);
			result2 = dateTime3.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GecenYil:
			result = dateTime5.AddYears(-1);
			result2 = dateTime5.AddDays(-1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Son3Ay:
			result = dateTime3.AddMonths(-2);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son6Ay:
			result = dateTime3.AddMonths(-5);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son12Ay:
			result = dateTime3.AddMonths(-11);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son7Gun:
			result = dateTime.AddDays(-6.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son15Gun:
			result = dateTime.AddDays(-14.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son30Gun:
			result = dateTime.AddDays(-29.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son60Gun:
			result = dateTime.AddDays(-59.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son90Gun:
			result = dateTime.AddDays(-89.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son365Gun:
			result = dateTime.AddDays(-364.0);
			result2 = dateTime2;
			break;
		case enum_tarih_cinsi.Son24Saat:
			result = DateTime.Now.AddHours(-24.0);
			result2 = DateTime.Now;
			break;
		case enum_tarih_cinsi.Yarin:
			result = dateTime.AddDays(1.0);
			result2 = dateTime.AddDays(1.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GelecekHafta:
			result = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(7.0);
			result2 = StartOfWeek(dateTime, DayOfWeek.Monday).AddDays(13.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek7Gun:
			result = dateTime;
			result2 = dateTime.AddDays(6.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek15Gun:
			result = dateTime;
			result2 = dateTime.AddDays(14.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek30Gun:
			result = dateTime;
			result2 = dateTime.AddDays(29.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek60Gun:
			result = dateTime;
			result2 = dateTime.AddDays(59.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek90Gun:
			result = dateTime;
			result2 = dateTime.AddDays(89.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek365Gun:
			result = dateTime;
			result2 = dateTime.AddDays(364.0).AddHours(23.0).AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GelecekAy:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(2).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek3Ay:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(4).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek6Ay:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(7).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.Gelecek12Ay:
			result = dateTime3.AddMonths(1);
			result2 = dateTime3.AddMonths(13).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		case enum_tarih_cinsi.GelecekYil:
			result = dateTime5.AddYears(1);
			result2 = dateTime5.AddYears(2).AddDays(-1.0).AddHours(23.0)
				.AddMinutes(59.0)
				.AddSeconds(59.0)
				.AddMilliseconds(99.0);
			break;
		}
		if (BaslangicMi)
		{
			return result;
		}
		return result2;
	}

	private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
	{
		int num = dt.DayOfWeek - startOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		return dt.AddDays(-1 * num).Date;
	}
}
