using System.Collections.Generic;
using Fora.Mikro.Enumler;

namespace Fora.Mikro.Stoklar.FiyatListeleri;

public static class FiyatUtility
{
	public static List<enum_Fiyat_Kaynagi> GetFiyatKaynaklariList(string virgulileayrilmiskaynaklar)
	{
		List<enum_Fiyat_Kaynagi> list = new List<enum_Fiyat_Kaynagi>();
		string[] array = virgulileayrilmiskaynaklar.Split(new char[1] { ',' });
		foreach (string s in array)
		{
			list.Add((enum_Fiyat_Kaynagi)int.Parse(s));
		}
		return list;
	}
}
