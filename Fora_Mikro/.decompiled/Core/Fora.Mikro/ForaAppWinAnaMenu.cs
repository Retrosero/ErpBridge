using Fora.Mikro.Enumler;

namespace Fora.Mikro;

public class ForaAppWinAnaMenu
{
	public string Adi { get; set; }

	public string Kod { get; set; }

	public string ParentKod { get; set; }

	public bool Gorunur { get; set; }

	public enum_ForaAppWinAnaMenuTipi Tipi { get; set; }

	public ForaAppWinAnaMenu()
	{
	}

	public ForaAppWinAnaMenu(string adi, string kod, string parentkod, bool gorunur, enum_ForaAppWinAnaMenuTipi tipi)
	{
		Adi = adi;
		Kod = kod;
		ParentKod = parentkod;
		Gorunur = gorunur;
		Tipi = tipi;
	}
}
