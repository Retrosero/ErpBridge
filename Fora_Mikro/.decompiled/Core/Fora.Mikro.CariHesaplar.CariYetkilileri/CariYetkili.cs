using System;
using System.IO;
using Fora.Mikro.CariHesaplar.CariAdresleri;

namespace Fora.Mikro.CariHesaplar.CariYetkilileri;

public class CariYetkili
{
	private string _mye_cari_kod;

	private string _mye_isim;

	private string _mye_soyisim;

	private string _mye_es_isim;

	private string _mye_dahili_telno;

	private string _mye_email_adres;

	private string _mye_cep_telno;

	private string _mye_tc_kimlikno;

	private string _mye_vergi_dairesi;

	private string _mye_vergi_kimlikno;

	private string _mye_dogum_yeri;

	private string _mye_ev_cadde;

	private string _mye_ev_sokak;

	private string _mye_ev_posta_kodu;

	private string _mye_ev_ilce;

	private string _mye_ev_il;

	private string _mye_ev_ulke;

	private string _mye_is_telno;

	private string _mye_ev_telno;

	private int _mye_adres_no;

	private enum_CariYetkiliUnvan _mye_unvan;

	private int _mye_hitap;

	private int _mye_hisse;

	private int _mye_tahsil;

	private DateTime _mye_dogum_tarihi;

	private DateTime _mye_evlilik_tarihi;

	private DateTime _mye_es_dogum_tarihi;

	public string mye_cari_kod
	{
		get
		{
			return _mye_cari_kod;
		}
		set
		{
			_mye_cari_kod = value;
			if (_mye_cari_kod.Length > 25)
			{
				_mye_cari_kod = _mye_cari_kod.Substring(0, 25);
			}
		}
	}

	public string mye_isim
	{
		get
		{
			return _mye_isim;
		}
		set
		{
			_mye_isim = value;
			if (_mye_isim.Length > 30)
			{
				_mye_isim = _mye_isim.Substring(0, 30);
			}
		}
	}

	public string mye_soyisim
	{
		get
		{
			return _mye_soyisim;
		}
		set
		{
			_mye_soyisim = value;
			if (_mye_soyisim.Length > 30)
			{
				_mye_soyisim = _mye_soyisim.Substring(0, 30);
			}
		}
	}

	public string mye_es_isim
	{
		get
		{
			return _mye_es_isim;
		}
		set
		{
			_mye_es_isim = value;
			if (_mye_es_isim.Length > 30)
			{
				_mye_es_isim = _mye_es_isim.Substring(0, 30);
			}
		}
	}

	public string mye_dahili_telno
	{
		get
		{
			return _mye_dahili_telno;
		}
		set
		{
			_mye_dahili_telno = value;
			if (_mye_dahili_telno.Length > 5)
			{
				_mye_dahili_telno = _mye_dahili_telno.Substring(0, 5);
			}
		}
	}

	public string mye_email_adres
	{
		get
		{
			return _mye_email_adres;
		}
		set
		{
			_mye_email_adres = value;
			if (_mye_email_adres.Length > 50)
			{
				_mye_email_adres = _mye_email_adres.Substring(0, 50);
			}
		}
	}

	public string mye_cep_telno
	{
		get
		{
			return _mye_cep_telno;
		}
		set
		{
			_mye_cep_telno = value;
			if (_mye_cep_telno.Length > 17)
			{
				_mye_cep_telno = _mye_cep_telno.Substring(0, 17);
			}
		}
	}

	public string mye_tc_kimlikno
	{
		get
		{
			return _mye_tc_kimlikno;
		}
		set
		{
			_mye_tc_kimlikno = value;
			if (_mye_tc_kimlikno.Length > 20)
			{
				_mye_tc_kimlikno = _mye_tc_kimlikno.Substring(0, 20);
			}
		}
	}

	public string mye_vergi_dairesi
	{
		get
		{
			return _mye_vergi_dairesi;
		}
		set
		{
			_mye_vergi_dairesi = value;
			if (_mye_vergi_dairesi.Length > 20)
			{
				_mye_vergi_dairesi = _mye_vergi_dairesi.Substring(0, 20);
			}
		}
	}

	public string mye_vergi_kimlikno
	{
		get
		{
			return _mye_vergi_kimlikno;
		}
		set
		{
			_mye_vergi_kimlikno = value;
			if (_mye_vergi_kimlikno.Length > 20)
			{
				_mye_vergi_kimlikno = _mye_vergi_kimlikno.Substring(0, 20);
			}
		}
	}

	public string mye_dogum_yeri
	{
		get
		{
			return _mye_dogum_yeri;
		}
		set
		{
			_mye_dogum_yeri = value;
			if (_mye_dogum_yeri.Length > 30)
			{
				_mye_dogum_yeri = _mye_dogum_yeri.Substring(0, 30);
			}
		}
	}

	public string mye_ev_cadde
	{
		get
		{
			return _mye_ev_cadde;
		}
		set
		{
			_mye_ev_cadde = value;
			if (_mye_ev_cadde.Length > 50)
			{
				_mye_ev_cadde = _mye_ev_cadde.Substring(0, 50);
			}
		}
	}

	public string mye_ev_sokak
	{
		get
		{
			return _mye_ev_sokak;
		}
		set
		{
			_mye_ev_sokak = value;
			if (_mye_ev_sokak.Length > 50)
			{
				_mye_ev_sokak = _mye_ev_sokak.Substring(0, 50);
			}
		}
	}

	public string mye_ev_posta_kodu
	{
		get
		{
			return _mye_ev_posta_kodu;
		}
		set
		{
			_mye_ev_posta_kodu = value;
			if (_mye_ev_posta_kodu.Length > 8)
			{
				_mye_ev_posta_kodu = _mye_ev_posta_kodu.Substring(0, 8);
			}
		}
	}

	public string mye_ev_ilce
	{
		get
		{
			return _mye_ev_ilce;
		}
		set
		{
			_mye_ev_ilce = value;
			if (_mye_ev_ilce.Length > 15)
			{
				_mye_ev_ilce = _mye_ev_ilce.Substring(0, 15);
			}
		}
	}

	public string mye_ev_il
	{
		get
		{
			return _mye_ev_il;
		}
		set
		{
			_mye_ev_il = value;
			if (_mye_ev_il.Length > 15)
			{
				_mye_ev_il = _mye_ev_il.Substring(0, 15);
			}
		}
	}

	public string mye_ev_ulke
	{
		get
		{
			return _mye_ev_ulke;
		}
		set
		{
			_mye_ev_ulke = value;
			if (_mye_ev_ulke.Length > 15)
			{
				_mye_ev_ulke = _mye_ev_ulke.Substring(0, 15);
			}
		}
	}

	public string mye_is_telno
	{
		get
		{
			return _mye_is_telno;
		}
		set
		{
			_mye_is_telno = value;
			if (_mye_is_telno.Length > 17)
			{
				_mye_is_telno = _mye_is_telno.Substring(0, 17);
			}
		}
	}

	public string mye_ev_telno
	{
		get
		{
			return _mye_ev_telno;
		}
		set
		{
			_mye_ev_telno = value;
			if (_mye_ev_telno.Length > 17)
			{
				_mye_ev_telno = _mye_ev_telno.Substring(0, 17);
			}
		}
	}

	public int mye_adres_no
	{
		get
		{
			return _mye_adres_no;
		}
		set
		{
			_mye_adres_no = value;
		}
	}

	public enum_CariYetkiliUnvan mye_unvan
	{
		get
		{
			return _mye_unvan;
		}
		set
		{
			_mye_unvan = value;
		}
	}

	public int mye_hitap
	{
		get
		{
			return _mye_hitap;
		}
		set
		{
			_mye_hitap = value;
		}
	}

	public int mye_hisse
	{
		get
		{
			return _mye_hisse;
		}
		set
		{
			_mye_hisse = value;
		}
	}

	public int mye_tahsil
	{
		get
		{
			return _mye_tahsil;
		}
		set
		{
			_mye_tahsil = value;
		}
	}

	public DateTime mye_dogum_tarihi
	{
		get
		{
			return _mye_dogum_tarihi;
		}
		set
		{
			_mye_dogum_tarihi = value;
		}
	}

	public DateTime mye_evlilik_tarihi
	{
		get
		{
			return _mye_evlilik_tarihi;
		}
		set
		{
			_mye_evlilik_tarihi = value;
		}
	}

	public DateTime mye_es_dogum_tarihi
	{
		get
		{
			return _mye_es_dogum_tarihi;
		}
		set
		{
			_mye_es_dogum_tarihi = value;
		}
	}

	public CariAdres Adres { get; set; }

	public CariYetkili()
	{
		_mye_cari_kod = "";
		_mye_isim = "";
		_mye_soyisim = "";
		_mye_es_isim = "";
		_mye_dahili_telno = "";
		_mye_email_adres = "";
		_mye_cep_telno = "";
		_mye_tc_kimlikno = "";
		_mye_vergi_dairesi = "";
		_mye_vergi_kimlikno = "";
		_mye_dogum_yeri = "";
		_mye_ev_cadde = "";
		_mye_ev_sokak = "";
		_mye_ev_posta_kodu = "";
		_mye_ev_ilce = "";
		_mye_ev_il = "";
		_mye_ev_ulke = "";
		_mye_is_telno = "";
		_mye_ev_telno = "";
		_mye_dogum_tarihi = new DateTime(1900, 1, 1);
		_mye_evlilik_tarihi = new DateTime(1900, 1, 1);
		_mye_es_dogum_tarihi = new DateTime(1900, 1, 1);
		Adres = new CariAdres();
	}

	public static byte[] WriteToByteArray(CariYetkili toSerialize)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(CariYetkili toWrite, Stream where)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer);
	}

	public static void WriteToBinaryWriter(CariYetkili toWrite, BinaryWriter writer)
	{
		writer.Write(toWrite.mye_cari_kod);
		writer.Write(toWrite.mye_isim);
		writer.Write(toWrite.mye_soyisim);
		writer.Write(toWrite.mye_es_isim);
		writer.Write(toWrite.mye_dahili_telno);
		writer.Write(toWrite.mye_email_adres);
		writer.Write(toWrite.mye_cep_telno);
		writer.Write(toWrite.mye_tc_kimlikno);
		writer.Write(toWrite.mye_vergi_dairesi);
		writer.Write(toWrite.mye_vergi_kimlikno);
		writer.Write(toWrite.mye_dogum_yeri);
		writer.Write(toWrite.mye_ev_cadde);
		writer.Write(toWrite.mye_ev_sokak);
		writer.Write(toWrite.mye_ev_posta_kodu);
		writer.Write(toWrite.mye_ev_ilce);
		writer.Write(toWrite.mye_ev_il);
		writer.Write(toWrite.mye_ev_ulke);
		writer.Write(toWrite.mye_is_telno);
		writer.Write(toWrite.mye_ev_telno);
		writer.Write(toWrite.mye_adres_no);
		writer.Write((int)toWrite.mye_unvan);
		writer.Write(toWrite.mye_hitap);
		writer.Write(toWrite.mye_hisse);
		writer.Write(toWrite.mye_tahsil);
		writer.Write(toWrite.mye_dogum_tarihi.Ticks);
		writer.Write(toWrite.mye_evlilik_tarihi.Ticks);
		writer.Write(toWrite.mye_es_dogum_tarihi.Ticks);
	}

	public static CariYetkili ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		CariYetkili result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static CariYetkili ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static CariYetkili ReadFromBinaryReader(BinaryReader reader)
	{
		return new CariYetkili
		{
			mye_cari_kod = reader.ReadString(),
			mye_isim = reader.ReadString(),
			mye_soyisim = reader.ReadString(),
			mye_es_isim = reader.ReadString(),
			mye_dahili_telno = reader.ReadString(),
			mye_email_adres = reader.ReadString(),
			mye_cep_telno = reader.ReadString(),
			mye_tc_kimlikno = reader.ReadString(),
			mye_vergi_dairesi = reader.ReadString(),
			mye_vergi_kimlikno = reader.ReadString(),
			mye_dogum_yeri = reader.ReadString(),
			mye_ev_cadde = reader.ReadString(),
			mye_ev_sokak = reader.ReadString(),
			mye_ev_posta_kodu = reader.ReadString(),
			mye_ev_ilce = reader.ReadString(),
			mye_ev_il = reader.ReadString(),
			mye_ev_ulke = reader.ReadString(),
			mye_is_telno = reader.ReadString(),
			mye_ev_telno = reader.ReadString(),
			mye_adres_no = reader.ReadInt32(),
			mye_unvan = (enum_CariYetkiliUnvan)reader.ReadInt32(),
			mye_hitap = reader.ReadInt32(),
			mye_hisse = reader.ReadInt32(),
			mye_tahsil = reader.ReadInt32(),
			mye_dogum_tarihi = new DateTime(reader.ReadInt64()),
			mye_evlilik_tarihi = new DateTime(reader.ReadInt64()),
			mye_es_dogum_tarihi = new DateTime(reader.ReadInt64())
		};
	}
}
