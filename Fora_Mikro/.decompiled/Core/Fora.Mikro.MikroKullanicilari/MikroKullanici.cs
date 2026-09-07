namespace Fora.Mikro.MikroKullanicilari;

public class MikroKullanici
{
	public int User_no { get; set; }

	public string User_name { get; set; }

	public string User_LongName { get; set; }

	public string User_EMail { get; set; }

	public MikroKullanici()
	{
		User_no = 0;
		User_name = "";
		User_LongName = "";
		User_EMail = "";
	}
}
