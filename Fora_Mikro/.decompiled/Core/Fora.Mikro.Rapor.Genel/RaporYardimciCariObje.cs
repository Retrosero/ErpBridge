namespace Fora.Mikro.Rapor.Genel;

public class RaporYardimciCariObje
{
	public int Position { get; set; }

	public int RecNo { get; set; }

	public string Kodu { get; set; }

	public string Adi { get; set; }

	public int cari_bolge_sira_no { get; set; }

	public string cari_bolge_kod { get; set; }

	public int cari_grup_sira_no { get; set; }

	public string cari_grup_kod { get; set; }

	public RaporYardimciCariObje()
	{
		Kodu = "";
		Adi = "";
	}

	public RaporYardimciCariObje(int _Position, int _RecNo, string _Kodu, string _Adi, int _cari_bolge_sira_no, int _cari_grup_sira_no)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
		cari_bolge_sira_no = _cari_bolge_sira_no;
		cari_grup_sira_no = _cari_grup_sira_no;
	}

	public RaporYardimciCariObje(int _Position, int _RecNo, string _Kodu, string _Adi, string _cari_bolge_kod, string _cari_grup_kod)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
		cari_bolge_kod = _cari_bolge_kod;
		cari_grup_kod = _cari_grup_kod;
	}
}
