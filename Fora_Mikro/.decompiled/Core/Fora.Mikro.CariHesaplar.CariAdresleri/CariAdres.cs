using System.IO;

namespace Fora.Mikro.CariHesaplar.CariAdresleri;

public class CariAdres
{
	private string _adr_cari_kod;

	private string _adr_cadde;

	private string _adr_sokak;

	private string _adr_posta_kodu;

	private string _adr_ilce;

	private string _adr_il;

	private string _adr_ulke;

	private string _adr_tel_ulke_kodu;

	private string _adr_tel_bolge_kodu;

	private string _adr_tel_no1;

	private string _adr_tel_no2;

	private string _adr_tel_faxno;

	private string _adr_tel_modem;

	private string _adr_yon_kodu;

	private string _adr_temsilci_kodu;

	private string _adr_ozel_not;

	private string _adr_ziyaretgunu;

	private float _adr_gps_enlem;

	private float _adr_gps_boylam;

	private int _adr_adres_no;

	private int _adr_uzaklik_kodu;

	private int _adr_ziyaretperyodu;

	private int _adr_ziyarethaftasi;

	private bool _adr_ziygunu2_1;

	private bool _adr_ziygunu2_2;

	private bool _adr_ziygunu2_3;

	private bool _adr_ziygunu2_4;

	private bool _adr_ziygunu2_5;

	private bool _adr_ziygunu2_6;

	private bool _adr_ziygunu2_7;

	private string _adr_special1;

	private string _adr_special2;

	private string _adr_special3;

	private string _CariUnvan;

	public bool Selected;

	public string adr_cari_kod
	{
		get
		{
			return _adr_cari_kod;
		}
		set
		{
			_adr_cari_kod = value;
			if (_adr_cari_kod.Length > 25)
			{
				_adr_cari_kod = _adr_cari_kod.Substring(0, 25);
			}
		}
	}

	public string adr_cadde
	{
		get
		{
			return _adr_cadde;
		}
		set
		{
			_adr_cadde = value;
			if (_adr_cadde.Length > 50)
			{
				_adr_cadde = _adr_cadde.Substring(0, 50);
			}
		}
	}

	public string adr_sokak
	{
		get
		{
			return _adr_sokak;
		}
		set
		{
			_adr_sokak = value;
			if (_adr_sokak.Length > 50)
			{
				_adr_sokak = _adr_sokak.Substring(0, 50);
			}
		}
	}

	public string adr_posta_kodu
	{
		get
		{
			return _adr_posta_kodu;
		}
		set
		{
			_adr_posta_kodu = value;
			if (_adr_posta_kodu.Length > 8)
			{
				_adr_posta_kodu = _adr_posta_kodu.Substring(0, 8);
			}
		}
	}

	public string adr_ilce
	{
		get
		{
			return _adr_ilce;
		}
		set
		{
			_adr_ilce = value;
			if (_adr_ilce.Length > 15)
			{
				_adr_ilce = _adr_ilce.Substring(0, 15);
			}
		}
	}

	public string adr_il
	{
		get
		{
			return _adr_il;
		}
		set
		{
			_adr_il = value;
			if (_adr_il.Length > 15)
			{
				_adr_il = _adr_il.Substring(0, 15);
			}
		}
	}

	public string adr_ulke
	{
		get
		{
			return _adr_ulke;
		}
		set
		{
			_adr_ulke = value;
			if (_adr_ulke.Length > 15)
			{
				_adr_ulke = _adr_ulke.Substring(0, 15);
			}
		}
	}

	public string adr_tel_ulke_kodu
	{
		get
		{
			return _adr_tel_ulke_kodu;
		}
		set
		{
			_adr_tel_ulke_kodu = value;
			if (_adr_tel_ulke_kodu.Length > 5)
			{
				_adr_tel_ulke_kodu = _adr_tel_ulke_kodu.Substring(0, 5);
			}
		}
	}

	public string adr_tel_bolge_kodu
	{
		get
		{
			return _adr_tel_bolge_kodu;
		}
		set
		{
			_adr_tel_bolge_kodu = value;
			if (_adr_tel_bolge_kodu.Length > 5)
			{
				_adr_tel_bolge_kodu = _adr_tel_bolge_kodu.Substring(0, 5);
			}
		}
	}

	public string adr_tel_no1
	{
		get
		{
			return _adr_tel_no1;
		}
		set
		{
			_adr_tel_no1 = value;
			if (_adr_tel_no1.Length > 10)
			{
				_adr_tel_no1 = _adr_tel_no1.Substring(0, 10);
			}
		}
	}

	public string adr_tel_no2
	{
		get
		{
			return _adr_tel_no2;
		}
		set
		{
			_adr_tel_no2 = value;
			if (_adr_tel_no2.Length > 10)
			{
				_adr_tel_no2 = _adr_tel_no2.Substring(0, 10);
			}
		}
	}

	public string adr_tel_faxno
	{
		get
		{
			return _adr_tel_faxno;
		}
		set
		{
			_adr_tel_faxno = value;
			if (_adr_tel_faxno.Length > 10)
			{
				_adr_tel_faxno = _adr_tel_faxno.Substring(0, 10);
			}
		}
	}

	public string adr_tel_modem
	{
		get
		{
			return _adr_tel_modem;
		}
		set
		{
			_adr_tel_modem = value;
			if (_adr_tel_modem.Length > 10)
			{
				_adr_tel_modem = _adr_tel_modem.Substring(0, 10);
			}
		}
	}

	public string adr_yon_kodu
	{
		get
		{
			return _adr_yon_kodu;
		}
		set
		{
			_adr_yon_kodu = value;
			if (_adr_yon_kodu.Length > 4)
			{
				_adr_yon_kodu = _adr_yon_kodu.Substring(0, 4);
			}
		}
	}

	public string adr_temsilci_kodu
	{
		get
		{
			return _adr_temsilci_kodu;
		}
		set
		{
			_adr_temsilci_kodu = value;
			if (_adr_temsilci_kodu.Length > 25)
			{
				_adr_temsilci_kodu = _adr_temsilci_kodu.Substring(0, 25);
			}
		}
	}

	public string adr_ozel_not
	{
		get
		{
			return _adr_ozel_not;
		}
		set
		{
			_adr_ozel_not = value;
			if (_adr_ozel_not.Length > 50)
			{
				_adr_ozel_not = _adr_ozel_not.Substring(0, 50);
			}
		}
	}

	public string adr_ziyaretgunu
	{
		get
		{
			return _adr_ziyaretgunu;
		}
		set
		{
			_adr_ziyaretgunu = value;
		}
	}

	public float adr_gps_enlem
	{
		get
		{
			return _adr_gps_enlem;
		}
		set
		{
			_adr_gps_enlem = value;
		}
	}

	public float adr_gps_boylam
	{
		get
		{
			return _adr_gps_boylam;
		}
		set
		{
			_adr_gps_boylam = value;
		}
	}

	public int adr_adres_no
	{
		get
		{
			return _adr_adres_no;
		}
		set
		{
			_adr_adres_no = value;
		}
	}

	public int adr_uzaklik_kodu
	{
		get
		{
			return _adr_uzaklik_kodu;
		}
		set
		{
			_adr_uzaklik_kodu = value;
		}
	}

	public int adr_ziyaretperyodu
	{
		get
		{
			return _adr_ziyaretperyodu;
		}
		set
		{
			_adr_ziyaretperyodu = value;
		}
	}

	public int adr_ziyarethaftasi
	{
		get
		{
			return _adr_ziyarethaftasi;
		}
		set
		{
			_adr_ziyarethaftasi = value;
		}
	}

	public bool adr_ziygunu2_1
	{
		get
		{
			return _adr_ziygunu2_1;
		}
		set
		{
			_adr_ziygunu2_1 = value;
		}
	}

	public bool adr_ziygunu2_2
	{
		get
		{
			return _adr_ziygunu2_2;
		}
		set
		{
			_adr_ziygunu2_2 = value;
		}
	}

	public bool adr_ziygunu2_3
	{
		get
		{
			return _adr_ziygunu2_3;
		}
		set
		{
			_adr_ziygunu2_3 = value;
		}
	}

	public bool adr_ziygunu2_4
	{
		get
		{
			return _adr_ziygunu2_4;
		}
		set
		{
			_adr_ziygunu2_4 = value;
		}
	}

	public bool adr_ziygunu2_5
	{
		get
		{
			return _adr_ziygunu2_5;
		}
		set
		{
			_adr_ziygunu2_5 = value;
		}
	}

	public bool adr_ziygunu2_6
	{
		get
		{
			return _adr_ziygunu2_6;
		}
		set
		{
			_adr_ziygunu2_6 = value;
		}
	}

	public bool adr_ziygunu2_7
	{
		get
		{
			return _adr_ziygunu2_7;
		}
		set
		{
			_adr_ziygunu2_7 = value;
		}
	}

	public string adr_special1
	{
		get
		{
			return _adr_special1;
		}
		set
		{
			_adr_special1 = value;
			if (_adr_special1.Length > 4)
			{
				_adr_special1 = _adr_special1.Substring(0, 4);
			}
		}
	}

	public string adr_special2
	{
		get
		{
			return _adr_special2;
		}
		set
		{
			_adr_special2 = value;
			if (_adr_special2.Length > 4)
			{
				_adr_special2 = _adr_special2.Substring(0, 4);
			}
		}
	}

	public string adr_special3
	{
		get
		{
			return _adr_special3;
		}
		set
		{
			_adr_special3 = value;
			if (_adr_special3.Length > 4)
			{
				_adr_special3 = _adr_special3.Substring(0, 4);
			}
		}
	}

	public string CariUnvan
	{
		get
		{
			return _CariUnvan;
		}
		set
		{
			_CariUnvan = value;
		}
	}

	public string GosterAdres => adr_cadde + " " + adr_sokak + " " + adr_posta_kodu + " " + adr_ilce + " " + adr_il + " " + adr_ulke;

	public string GosterTel1 => adr_tel_ulke_kodu + " " + adr_tel_bolge_kodu + " " + adr_tel_no1;

	public string GosterTel2 => adr_tel_ulke_kodu + " " + adr_tel_bolge_kodu + " " + adr_tel_no2;

	public CariAdres()
	{
		_adr_cari_kod = "";
		_adr_cadde = "";
		_adr_sokak = "";
		_adr_posta_kodu = "";
		_adr_ilce = "";
		_adr_il = "";
		_adr_ulke = "";
		_adr_tel_ulke_kodu = "";
		_adr_tel_bolge_kodu = "";
		_adr_tel_no1 = "";
		_adr_tel_no2 = "";
		_adr_tel_faxno = "";
		_adr_tel_modem = "";
		_adr_yon_kodu = "";
		_adr_temsilci_kodu = "";
		_adr_ozel_not = "";
		_adr_ziyaretgunu = "0";
		_adr_special1 = "";
		_adr_special2 = "";
		_adr_special3 = "";
		_CariUnvan = "";
		Selected = false;
	}

	public static byte[] WriteToByteArray(CariAdres toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(CariAdres toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(CariAdres toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.adr_cari_kod);
		writer.Write(toWrite.adr_cadde);
		writer.Write(toWrite.adr_sokak);
		writer.Write(toWrite.adr_posta_kodu);
		writer.Write(toWrite.adr_ilce);
		writer.Write(toWrite.adr_il);
		writer.Write(toWrite.adr_ulke);
		writer.Write(toWrite.adr_tel_ulke_kodu);
		writer.Write(toWrite.adr_tel_bolge_kodu);
		writer.Write(toWrite.adr_tel_no1);
		writer.Write(toWrite.adr_tel_no2);
		writer.Write(toWrite.adr_tel_faxno);
		writer.Write(toWrite.adr_tel_modem);
		writer.Write(toWrite.adr_yon_kodu);
		writer.Write(toWrite.adr_temsilci_kodu);
		writer.Write(toWrite.adr_ozel_not);
		writer.Write(toWrite.adr_ziyaretgunu);
		writer.Write(toWrite.adr_gps_enlem);
		writer.Write(toWrite.adr_gps_boylam);
		writer.Write(toWrite.adr_adres_no);
		writer.Write(toWrite.adr_uzaklik_kodu);
		writer.Write(toWrite.adr_ziyaretperyodu);
		writer.Write(toWrite.adr_ziyarethaftasi);
		writer.Write(toWrite.adr_ziygunu2_1);
		writer.Write(toWrite.adr_ziygunu2_2);
		writer.Write(toWrite.adr_ziygunu2_3);
		writer.Write(toWrite.adr_ziygunu2_4);
		writer.Write(toWrite.adr_ziygunu2_5);
		writer.Write(toWrite.adr_ziygunu2_6);
		writer.Write(toWrite.adr_ziygunu2_7);
		writer.Write(toWrite.adr_special1);
		writer.Write(toWrite.adr_special2);
		writer.Write(toWrite.adr_special3);
		writer.Write(toWrite.CariUnvan);
		_ = 2;
	}

	public static CariAdres ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		CariAdres result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static CariAdres ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static CariAdres ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new CariAdres
		{
			adr_cari_kod = reader.ReadString(),
			adr_cadde = reader.ReadString(),
			adr_sokak = reader.ReadString(),
			adr_posta_kodu = reader.ReadString(),
			adr_ilce = reader.ReadString(),
			adr_il = reader.ReadString(),
			adr_ulke = reader.ReadString(),
			adr_tel_ulke_kodu = reader.ReadString(),
			adr_tel_bolge_kodu = reader.ReadString(),
			adr_tel_no1 = reader.ReadString(),
			adr_tel_no2 = reader.ReadString(),
			adr_tel_faxno = reader.ReadString(),
			adr_tel_modem = reader.ReadString(),
			adr_yon_kodu = reader.ReadString(),
			adr_temsilci_kodu = reader.ReadString(),
			adr_ozel_not = reader.ReadString(),
			adr_ziyaretgunu = reader.ReadString(),
			adr_gps_enlem = reader.ReadSingle(),
			adr_gps_boylam = reader.ReadSingle(),
			adr_adres_no = reader.ReadInt32(),
			adr_uzaklik_kodu = reader.ReadInt32(),
			adr_ziyaretperyodu = reader.ReadInt32(),
			adr_ziyarethaftasi = reader.ReadInt32(),
			adr_ziygunu2_1 = reader.ReadBoolean(),
			adr_ziygunu2_2 = reader.ReadBoolean(),
			adr_ziygunu2_3 = reader.ReadBoolean(),
			adr_ziygunu2_4 = reader.ReadBoolean(),
			adr_ziygunu2_5 = reader.ReadBoolean(),
			adr_ziygunu2_6 = reader.ReadBoolean(),
			adr_ziygunu2_7 = reader.ReadBoolean(),
			adr_special1 = reader.ReadString(),
			adr_special2 = reader.ReadString(),
			adr_special3 = reader.ReadString(),
			CariUnvan = reader.ReadString()
		};
	}
}
