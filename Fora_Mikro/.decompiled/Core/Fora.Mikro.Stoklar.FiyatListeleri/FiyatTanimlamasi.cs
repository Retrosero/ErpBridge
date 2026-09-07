using System.IO;
using Fora.Mikro.Enumler;
using Fora.Mikro.Kurlar;

namespace Fora.Mikro.Stoklar.FiyatListeleri;

public class FiyatTanimlamasi
{
	private bool _initilazing;

	private double _FiyatBrut;

	private int _Iskonto_1_UygulamaSekli;

	private double _Iskonto_1_YuzdeVeyaMiktar;

	private int _Iskonto_2_UygulamaSekli = 1;

	private double _Iskonto_2_YuzdeVeyaMiktar;

	private int _Iskonto_3_UygulamaSekli = 1;

	private double _Iskonto_3_YuzdeVeyaMiktar;

	private int _Iskonto_4_UygulamaSekli = 1;

	private double _Iskonto_4_YuzdeVeyaMiktar;

	private int _Iskonto_5_UygulamaSekli = 1;

	private double _Iskonto_5_YuzdeVeyaMiktar;

	private int _Iskonto_6_UygulamaSekli = 1;

	private double _Iskonto_6_YuzdeVeyaMiktar;

	private int _Masraf_1_UygulamaSekli = 1;

	private double _Masraf_1_YuzdeVeyaMiktar;

	private int _Masraf_2_UygulamaSekli = 1;

	private double _Masraf_2_YuzdeVeyaMiktar;

	private int _Masraf_3_UygulamaSekli = 1;

	private double _Masraf_3_YuzdeVeyaMiktar;

	private int _Masraf_4_UygulamaSekli = 1;

	private double _Masraf_4_YuzdeVeyaMiktar;

	private double _Iskonto_1_Tutari;

	private double _Iskonto_2_Tutari;

	private double _Iskonto_3_Tutari;

	private double _Iskonto_4_Tutari;

	private double _Iskonto_5_Tutari;

	private double _Iskonto_6_Tutari;

	private double _IskontoTutariToplam;

	private double _Masraf_1_Tutari;

	private double _Masraf_2_Tutari;

	private double _Masraf_3_Tutari;

	private double _Masraf_4_Tutari;

	private double _MasrafTutariToplam;

	private double _FiyatNetMasrafli;

	private double _FiyatNetMasrafsiz;

	public double FiyatBrut
	{
		get
		{
			return _FiyatBrut;
		}
		set
		{
			_FiyatBrut = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Iskonto_1_UygulamaSekli
	{
		get
		{
			return _Iskonto_1_UygulamaSekli;
		}
		set
		{
			_Iskonto_1_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Iskonto_1_YuzdeVeyaMiktar
	{
		get
		{
			return _Iskonto_1_YuzdeVeyaMiktar;
		}
		set
		{
			_Iskonto_1_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Iskonto_2_UygulamaSekli
	{
		get
		{
			return _Iskonto_2_UygulamaSekli;
		}
		set
		{
			_Iskonto_2_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Iskonto_2_YuzdeVeyaMiktar
	{
		get
		{
			return _Iskonto_2_YuzdeVeyaMiktar;
		}
		set
		{
			_Iskonto_2_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Iskonto_3_UygulamaSekli
	{
		get
		{
			return _Iskonto_3_UygulamaSekli;
		}
		set
		{
			_Iskonto_3_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Iskonto_3_YuzdeVeyaMiktar
	{
		get
		{
			return _Iskonto_3_YuzdeVeyaMiktar;
		}
		set
		{
			_Iskonto_3_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Iskonto_4_UygulamaSekli
	{
		get
		{
			return _Iskonto_4_UygulamaSekli;
		}
		set
		{
			_Iskonto_4_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Iskonto_4_YuzdeVeyaMiktar
	{
		get
		{
			return _Iskonto_4_YuzdeVeyaMiktar;
		}
		set
		{
			_Iskonto_4_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Iskonto_5_UygulamaSekli
	{
		get
		{
			return _Iskonto_5_UygulamaSekli;
		}
		set
		{
			_Iskonto_5_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Iskonto_5_YuzdeVeyaMiktar
	{
		get
		{
			return _Iskonto_5_YuzdeVeyaMiktar;
		}
		set
		{
			_Iskonto_5_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Iskonto_6_UygulamaSekli
	{
		get
		{
			return _Iskonto_6_UygulamaSekli;
		}
		set
		{
			_Iskonto_6_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Iskonto_6_YuzdeVeyaMiktar
	{
		get
		{
			return _Iskonto_6_YuzdeVeyaMiktar;
		}
		set
		{
			_Iskonto_6_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Masraf_1_UygulamaSekli
	{
		get
		{
			return _Masraf_1_UygulamaSekli;
		}
		set
		{
			_Masraf_1_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Masraf_1_YuzdeVeyaMiktar
	{
		get
		{
			return _Masraf_1_YuzdeVeyaMiktar;
		}
		set
		{
			_Masraf_1_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Masraf_2_UygulamaSekli
	{
		get
		{
			return _Masraf_2_UygulamaSekli;
		}
		set
		{
			_Masraf_2_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Masraf_2_YuzdeVeyaMiktar
	{
		get
		{
			return _Masraf_2_YuzdeVeyaMiktar;
		}
		set
		{
			_Masraf_2_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Masraf_3_UygulamaSekli
	{
		get
		{
			return _Masraf_3_UygulamaSekli;
		}
		set
		{
			_Masraf_3_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Masraf_3_YuzdeVeyaMiktar
	{
		get
		{
			return _Masraf_3_YuzdeVeyaMiktar;
		}
		set
		{
			_Masraf_3_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int Masraf_4_UygulamaSekli
	{
		get
		{
			return _Masraf_4_UygulamaSekli;
		}
		set
		{
			_Masraf_4_UygulamaSekli = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public double Masraf_4_YuzdeVeyaMiktar
	{
		get
		{
			return _Masraf_4_YuzdeVeyaMiktar;
		}
		set
		{
			_Masraf_4_YuzdeVeyaMiktar = value;
			if (!_initilazing)
			{
				Initilize();
			}
		}
	}

	public int DovizCinsi { get; set; }

	public Kur Kur { get; set; }

	public enum_Fiyat_Kaynagi FiyatKaynagi { get; set; }

	public enum_YuzdeTutar OtvUygulamaSekli { get; set; }

	public double OtvYuzdeVeyaTutar { get; set; }

	public int OtvVergiPntr { get; set; }

	public double Iskonto_1_Tutari => _Iskonto_1_Tutari;

	public double Iskonto_2_Tutari => _Iskonto_2_Tutari;

	public double Iskonto_3_Tutari => _Iskonto_3_Tutari;

	public double Iskonto_4_Tutari => _Iskonto_4_Tutari;

	public double Iskonto_5_Tutari => _Iskonto_5_Tutari;

	public double Iskonto_6_Tutari => _Iskonto_6_Tutari;

	public double IskontoTutariToplam => _IskontoTutariToplam;

	public double OtvTutariToplam
	{
		get
		{
			if (OtvUygulamaSekli == enum_YuzdeTutar.Yuzde)
			{
				double num = 100.0;
				return (_FiyatBrut - _IskontoTutariToplam) / num * OtvYuzdeVeyaTutar;
			}
			return OtvYuzdeVeyaTutar;
		}
	}

	public double Masraf_1_Tutari => _Masraf_1_Tutari;

	public double Masraf_2_Tutari => _Masraf_2_Tutari;

	public double Masraf_3_Tutari => _Masraf_3_Tutari;

	public double Masraf_4_Tutari => _Masraf_4_Tutari;

	public double MasrafTutariToplam => _MasrafTutariToplam;

	public double FiyatNetMasrafli => _FiyatNetMasrafli;

	public double FiyatNetMasrafsiz => _FiyatNetMasrafsiz;

	public FiyatTanimlamasi()
	{
		FiyatKaynagi = enum_Fiyat_Kaynagi.Tanimsiz;
		Kur = new Kur();
		DovizCinsi = 0;
		OtvUygulamaSekli = enum_YuzdeTutar.Yuzde;
		OtvYuzdeVeyaTutar = 0.0;
		OtvVergiPntr = 0;
	}

	private void Initilize()
	{
		double fiyatBrut = FiyatBrut;
		fiyatBrut = Iskonto_1_UygulamaSekli switch
		{
			0 => fiyatBrut - FiyatBrut / 100.0 * Iskonto_1_YuzdeVeyaMiktar, 
			1 => fiyatBrut - fiyatBrut / 100.0 * Iskonto_1_YuzdeVeyaMiktar, 
			_ => fiyatBrut - Iskonto_1_YuzdeVeyaMiktar, 
		};
		_Iskonto_1_Tutari = FiyatBrut - fiyatBrut;
		fiyatBrut = FiyatBrut;
		fiyatBrut = Iskonto_2_UygulamaSekli switch
		{
			0 => fiyatBrut - FiyatBrut / 100.0 * Iskonto_2_YuzdeVeyaMiktar, 
			1 => fiyatBrut - (FiyatBrut - Iskonto_1_Tutari) / 100.0 * Iskonto_2_YuzdeVeyaMiktar, 
			_ => fiyatBrut - Iskonto_2_YuzdeVeyaMiktar, 
		};
		_Iskonto_2_Tutari = FiyatBrut - fiyatBrut;
		fiyatBrut = FiyatBrut;
		fiyatBrut = Iskonto_3_UygulamaSekli switch
		{
			0 => fiyatBrut - FiyatBrut / 100.0 * Iskonto_3_YuzdeVeyaMiktar, 
			1 => fiyatBrut - (FiyatBrut - Iskonto_1_Tutari - Iskonto_2_Tutari) / 100.0 * Iskonto_3_YuzdeVeyaMiktar, 
			_ => fiyatBrut - Iskonto_3_YuzdeVeyaMiktar, 
		};
		_Iskonto_3_Tutari = FiyatBrut - fiyatBrut;
		fiyatBrut = FiyatBrut;
		fiyatBrut = Iskonto_4_UygulamaSekli switch
		{
			0 => fiyatBrut - FiyatBrut / 100.0 * Iskonto_4_YuzdeVeyaMiktar, 
			1 => fiyatBrut - (FiyatBrut - Iskonto_1_Tutari - Iskonto_2_Tutari - Iskonto_3_Tutari) / 100.0 * Iskonto_4_YuzdeVeyaMiktar, 
			_ => fiyatBrut - Iskonto_4_YuzdeVeyaMiktar, 
		};
		_Iskonto_4_Tutari = FiyatBrut - fiyatBrut;
		fiyatBrut = FiyatBrut;
		fiyatBrut = Iskonto_5_UygulamaSekli switch
		{
			0 => fiyatBrut - FiyatBrut / 100.0 * Iskonto_5_YuzdeVeyaMiktar, 
			1 => fiyatBrut - (FiyatBrut - Iskonto_1_Tutari - Iskonto_2_Tutari - Iskonto_3_Tutari - Iskonto_4_Tutari) / 100.0 * Iskonto_5_YuzdeVeyaMiktar, 
			_ => fiyatBrut - Iskonto_5_YuzdeVeyaMiktar, 
		};
		_Iskonto_5_Tutari = FiyatBrut - fiyatBrut;
		fiyatBrut = FiyatBrut;
		fiyatBrut = Iskonto_6_UygulamaSekli switch
		{
			0 => fiyatBrut - FiyatBrut / 100.0 * Iskonto_6_YuzdeVeyaMiktar, 
			1 => fiyatBrut - (FiyatBrut - Iskonto_1_Tutari - Iskonto_2_Tutari - Iskonto_3_Tutari - Iskonto_4_Tutari - Iskonto_5_Tutari) / 100.0 * Iskonto_6_YuzdeVeyaMiktar, 
			_ => fiyatBrut - Iskonto_6_YuzdeVeyaMiktar, 
		};
		_Iskonto_6_Tutari = FiyatBrut - fiyatBrut;
		_IskontoTutariToplam = Iskonto_1_Tutari + Iskonto_2_Tutari + Iskonto_3_Tutari + Iskonto_4_Tutari + Iskonto_5_Tutari + Iskonto_6_Tutari;
		fiyatBrut = FiyatBrut - IskontoTutariToplam;
		switch (Masraf_1_UygulamaSekli)
		{
		case 0:
			fiyatBrut += FiyatBrut / 100.0 * Masraf_1_YuzdeVeyaMiktar;
			break;
		case 1:
			fiyatBrut += fiyatBrut / 100.0 * Masraf_1_YuzdeVeyaMiktar;
			break;
		case 2:
			fiyatBrut += Masraf_1_YuzdeVeyaMiktar;
			break;
		}
		_Masraf_1_Tutari = fiyatBrut - (FiyatBrut - IskontoTutariToplam);
		fiyatBrut = FiyatBrut - IskontoTutariToplam;
		switch (Masraf_2_UygulamaSekli)
		{
		case 0:
			fiyatBrut += FiyatBrut / 100.0 * Masraf_2_YuzdeVeyaMiktar;
			break;
		case 1:
			fiyatBrut += (FiyatBrut - IskontoTutariToplam + Masraf_1_Tutari) / 100.0 * Masraf_2_YuzdeVeyaMiktar;
			break;
		case 2:
			fiyatBrut += Masraf_2_YuzdeVeyaMiktar;
			break;
		}
		_Masraf_2_Tutari = fiyatBrut - (FiyatBrut - IskontoTutariToplam);
		fiyatBrut = FiyatBrut - IskontoTutariToplam;
		switch (Masraf_3_UygulamaSekli)
		{
		case 0:
			fiyatBrut += FiyatBrut / 100.0 * Masraf_3_YuzdeVeyaMiktar;
			break;
		case 1:
			fiyatBrut += (FiyatBrut - IskontoTutariToplam + Masraf_1_Tutari + Masraf_2_Tutari) / 100.0 * Masraf_3_YuzdeVeyaMiktar;
			break;
		case 2:
			fiyatBrut += Masraf_3_YuzdeVeyaMiktar;
			break;
		}
		_Masraf_3_Tutari = fiyatBrut - (FiyatBrut - IskontoTutariToplam);
		fiyatBrut = FiyatBrut - IskontoTutariToplam;
		switch (Masraf_4_UygulamaSekli)
		{
		case 0:
			fiyatBrut += FiyatBrut / 100.0 * Masraf_4_YuzdeVeyaMiktar;
			break;
		case 1:
			fiyatBrut += (FiyatBrut - IskontoTutariToplam + Masraf_1_Tutari + Masraf_2_Tutari + Masraf_3_Tutari) / 100.0 * Masraf_4_YuzdeVeyaMiktar;
			break;
		case 2:
			fiyatBrut += Masraf_4_YuzdeVeyaMiktar;
			break;
		}
		_Masraf_4_Tutari = fiyatBrut - (FiyatBrut - IskontoTutariToplam);
		_MasrafTutariToplam = Masraf_1_Tutari + Masraf_2_Tutari + Masraf_3_Tutari + Masraf_4_Tutari;
		_FiyatNetMasrafli = FiyatBrut - IskontoTutariToplam + MasrafTutariToplam;
		_FiyatNetMasrafsiz = FiyatBrut - IskontoTutariToplam;
	}

	public void BeginInit()
	{
		_initilazing = true;
	}

	public void EndInit()
	{
		_initilazing = false;
		Initilize();
	}

	public static byte[] WriteToByteArray(FiyatTanimlamasi toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(FiyatTanimlamasi toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(FiyatTanimlamasi toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.FiyatBrut);
		writer.Write(toWrite.Iskonto_1_UygulamaSekli);
		writer.Write(toWrite.Iskonto_1_YuzdeVeyaMiktar);
		writer.Write(toWrite.Iskonto_2_UygulamaSekli);
		writer.Write(toWrite.Iskonto_2_YuzdeVeyaMiktar);
		writer.Write(toWrite.Iskonto_3_UygulamaSekli);
		writer.Write(toWrite.Iskonto_3_YuzdeVeyaMiktar);
		writer.Write(toWrite.Iskonto_4_UygulamaSekli);
		writer.Write(toWrite.Iskonto_4_YuzdeVeyaMiktar);
		writer.Write(toWrite.Iskonto_5_UygulamaSekli);
		writer.Write(toWrite.Iskonto_5_YuzdeVeyaMiktar);
		writer.Write(toWrite.Iskonto_6_UygulamaSekli);
		writer.Write(toWrite.Iskonto_6_YuzdeVeyaMiktar);
		writer.Write(toWrite.Masraf_1_UygulamaSekli);
		writer.Write(toWrite.Masraf_1_YuzdeVeyaMiktar);
		writer.Write(toWrite.Masraf_2_UygulamaSekli);
		writer.Write(toWrite.Masraf_2_YuzdeVeyaMiktar);
		writer.Write(toWrite.Masraf_3_UygulamaSekli);
		writer.Write(toWrite.Masraf_3_YuzdeVeyaMiktar);
		writer.Write(toWrite.Masraf_4_UygulamaSekli);
		writer.Write(toWrite.Masraf_4_YuzdeVeyaMiktar);
		writer.Write(toWrite.DovizCinsi);
		writer.Write(toWrite.Kur.dov_no);
		writer.Write(toWrite.Kur.dov_fiyat);
		writer.Write((int)toWrite.FiyatKaynagi);
		writer.Write((int)toWrite.OtvUygulamaSekli);
		writer.Write(toWrite.OtvYuzdeVeyaTutar);
		writer.Write(toWrite.OtvVergiPntr);
		_ = 2;
	}

	public static FiyatTanimlamasi ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		FiyatTanimlamasi result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static FiyatTanimlamasi ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static FiyatTanimlamasi ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.BeginInit();
		fiyatTanimlamasi.FiyatBrut = reader.ReadDouble();
		fiyatTanimlamasi.Iskonto_1_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Iskonto_2_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Iskonto_3_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Iskonto_4_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Iskonto_5_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Iskonto_6_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Masraf_1_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Masraf_2_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Masraf_3_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.Masraf_4_UygulamaSekli = reader.ReadInt32();
		fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = reader.ReadDouble();
		fiyatTanimlamasi.DovizCinsi = reader.ReadInt32();
		fiyatTanimlamasi.Kur = new Kur();
		fiyatTanimlamasi.Kur.dov_no = reader.ReadInt32();
		fiyatTanimlamasi.Kur.dov_fiyat = reader.ReadDouble();
		fiyatTanimlamasi.FiyatKaynagi = (enum_Fiyat_Kaynagi)reader.ReadInt32();
		fiyatTanimlamasi.OtvUygulamaSekli = (enum_YuzdeTutar)reader.ReadInt32();
		fiyatTanimlamasi.OtvYuzdeVeyaTutar = reader.ReadDouble();
		fiyatTanimlamasi.OtvVergiPntr = reader.ReadInt32();
		fiyatTanimlamasi.EndInit();
		return fiyatTanimlamasi;
	}
}
