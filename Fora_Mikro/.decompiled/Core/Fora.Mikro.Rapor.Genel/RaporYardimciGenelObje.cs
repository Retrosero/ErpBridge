namespace Fora.Mikro.Rapor.Genel;

public class RaporYardimciGenelObje
{
	public int Position { get; set; }

	public int RecNo { get; set; }

	public string Kodu { get; set; }

	public string Adi { get; set; }

	public RaporYardimciGenelObje()
	{
		Kodu = "";
		Adi = "";
	}

	public RaporYardimciGenelObje(int _Position, int _RecNo, string _Kodu, string _Adi)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
	}
}
