namespace Fora.Mikro.Rapor.Genel;

public class RaporYardimciStokObje
{
	public int Position { get; set; }

	public int RecNo { get; set; }

	public string Kodu { get; set; }

	public string Adi { get; set; }

	public string Birim1Adi { get; set; }

	public string Birim2Adi { get; set; }

	public string Birim3Adi { get; set; }

	public int stok_anagrup_sira_no { get; set; }

	public string stok_anagrup_kod { get; set; }

	public int stok_uretici_sira_no { get; set; }

	public string stok_uretici_kod { get; set; }

	public int stok_marka_sira_no { get; set; }

	public string stok_marka_kod { get; set; }

	public int stok_reyon_sira_no { get; set; }

	public string stok_reyon_kod { get; set; }

	public int stok_kategori_sira_no { get; set; }

	public string stok_kategori_kod { get; set; }

	public int stok_doviz_cinsi { get; set; }

	public RaporYardimciStokObje()
	{
		Kodu = "";
		Adi = "";
		Birim1Adi = "";
		Birim2Adi = "";
		Birim3Adi = "";
	}

	public RaporYardimciStokObje(int _Position, int _RecNo, string _Kodu, string _Adi, string _Birim1Adi, string _Birim2Adi, int _stok_anagrup_sira_no, int _stok_uretici_sira_no, int _stok_marka_sira_no, int _stok_reyon_sira_no, int _stok_kategori_sira_no)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
		Birim1Adi = _Birim1Adi;
		Birim2Adi = _Birim2Adi;
		stok_anagrup_sira_no = _stok_anagrup_sira_no;
		stok_uretici_sira_no = _stok_uretici_sira_no;
		stok_marka_sira_no = _stok_marka_sira_no;
		stok_reyon_sira_no = _stok_reyon_sira_no;
		stok_kategori_sira_no = _stok_kategori_sira_no;
		Birim3Adi = "";
	}

	public RaporYardimciStokObje(int _Position, int _RecNo, string _Kodu, string _Adi, string _Birim1Adi, string _Birim2Adi, string _stok_anagrup_kod, string _stok_uretici_kod, string _stok_marka_kod, string _stok_reyon_kod, string _stok_kategori_kod)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
		Birim1Adi = _Birim1Adi;
		Birim2Adi = _Birim2Adi;
		stok_anagrup_kod = _stok_anagrup_kod;
		stok_uretici_kod = _stok_uretici_kod;
		stok_marka_kod = _stok_marka_kod;
		stok_reyon_kod = _stok_reyon_kod;
		stok_kategori_kod = _stok_kategori_kod;
		Birim3Adi = "";
	}

	public RaporYardimciStokObje(int _Position, int _RecNo, string _Kodu, string _Adi, string _Birim1Adi, string _Birim2Adi, string _Birim3Adi, int _stok_anagrup_sira_no, int _stok_uretici_sira_no, int _stok_marka_sira_no, int _stok_reyon_sira_no, int _stok_kategori_sira_no)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
		Birim1Adi = _Birim1Adi;
		Birim2Adi = _Birim2Adi;
		Birim3Adi = _Birim3Adi;
		stok_anagrup_sira_no = _stok_anagrup_sira_no;
		stok_uretici_sira_no = _stok_uretici_sira_no;
		stok_marka_sira_no = _stok_marka_sira_no;
		stok_reyon_sira_no = _stok_reyon_sira_no;
		stok_kategori_sira_no = _stok_kategori_sira_no;
	}

	public RaporYardimciStokObje(int _Position, int _RecNo, string _Kodu, string _Adi, string _Birim1Adi, string _Birim2Adi, string _Birim3Adi, string _stok_anagrup_kod, string _stok_uretici_kod, string _stok_marka_kod, string _stok_reyon_kod, string _stok_kategori_kod)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
		Birim1Adi = _Birim1Adi;
		Birim2Adi = _Birim2Adi;
		Birim3Adi = _Birim3Adi;
		stok_anagrup_kod = _stok_anagrup_kod;
		stok_uretici_kod = _stok_uretici_kod;
		stok_marka_kod = _stok_marka_kod;
		stok_reyon_kod = _stok_reyon_kod;
		stok_kategori_kod = _stok_kategori_kod;
	}

	public RaporYardimciStokObje(int _Position, int _RecNo, string _Kodu, string _Adi, string _Birim1Adi, string _Birim2Adi, string _Birim3Adi, string _stok_anagrup_kod, string _stok_uretici_kod, string _stok_marka_kod, string _stok_reyon_kod, string _stok_kategori_kod, int _doviz_cinsi)
	{
		Position = _Position;
		RecNo = _RecNo;
		Kodu = _Kodu;
		Adi = _Adi;
		Birim1Adi = _Birim1Adi;
		Birim2Adi = _Birim2Adi;
		Birim3Adi = _Birim3Adi;
		stok_anagrup_kod = _stok_anagrup_kod;
		stok_uretici_kod = _stok_uretici_kod;
		stok_marka_kod = _stok_marka_kod;
		stok_reyon_kod = _stok_reyon_kod;
		stok_kategori_kod = _stok_kategori_kod;
		stok_doviz_cinsi = _doviz_cinsi;
	}
}
