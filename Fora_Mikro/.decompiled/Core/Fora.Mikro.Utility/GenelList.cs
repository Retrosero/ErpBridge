namespace Fora.Mikro.Utility;

public class GenelList
{
	public string Kod { get; set; }

	public string Text { get; set; }

	public bool Selected { get; set; }

	public GenelList()
	{
		Kod = "";
		Text = "";
	}

	public GenelList(string Kodu, string Yazi)
	{
		Kod = Kodu;
		Text = Yazi;
	}
}
