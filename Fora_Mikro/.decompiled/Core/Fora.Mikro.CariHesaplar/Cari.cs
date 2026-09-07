using System.Collections.Generic;
using System.IO;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariYetkilileri;

namespace Fora.Mikro.CariHesaplar;

public class Cari
{
	private string _cari_kod_prefix;

	private string _cari_kod;

	private string _cari_unvan1;

	private string _cari_unvan2;

	private string _cari_muh_kod;

	private string _cari_muh_kod1;

	private string _cari_muh_kod2;

	private string _cari_muhartikeli;

	private string _cari_vdaire_adi;

	private string _cari_vdaire_no;

	private string _cari_Ana_cari_kodu;

	private string _cari_bolge_kodu;

	private string _cari_grup_kodu;

	private string _cari_temsilci_kodu;

	private string _cari_sektor_kodu;

	private string _cari_satis_isk_kod;

	private string _cari_special1;

	private string _cari_special2;

	private string _cari_special3;

	private string _cari_sicil_no;

	private string _cari_VergiKimlikNo;

	private string _cari_banka_hesapno1;

	private float _cari_vade_fark_yuz;

	private float _cari_vade_fark_yuz1;

	private float _cari_vade_fark_yuz2;

	private int _cari_tipi;

	private int _cari_doviz_cinsi;

	private int _cari_doviz_cinsi1;

	private int _cari_doviz_cinsi2;

	private int _cari_odeme_gunu;

	private int _cari_hareket_tipi;

	private int _cari_odemeplan_no;

	private int _cari_satis_fk;

	private int _cari_KurHesapSekli;

	private int _cari_odeme_cinsi;

	private int _cari_fatura_adres_no;

	private int _cari_sevk_adres_no;

	private int _cari_VarsayilanGirisDepo;

	private int _cari_VarsayilanCikisDepo;

	private string _cari_CepTel;

	private string _cari_Email;

	private bool _cari_cari_kilitli_flg;

	private string _cari_wwwadresi;

	private string _cari_Portal_PW;

	private bool _cari_Portal_Enabled;

	private enum_cari_stok_alim_cinsi _cari_stok_alim_cinsi;

	private enum_cari_stok_satim_cinsi _cari_stok_satim_cinsi;

	private string _cari_banka_swiftkodu1;

	private string _cari_banka_swiftkodu2;

	private string _cari_banka_swiftkodu3;

	private string _cari_banka_tcmb_kod4;

	private string _cari_banka_tcmb_subekod4;

	private string _cari_banka_tcmb_ilkod4;

	private string _cari_banka_hesapno4;

	private string _cari_banka_swiftkodu4;

	private string _cari_banka_tcmb_kod5;

	private string _cari_banka_tcmb_subekod5;

	private string _cari_banka_tcmb_ilkod5;

	private string _cari_banka_hesapno5;

	private string _cari_banka_swiftkodu5;

	private string _cari_banka_tcmb_kod6;

	private string _cari_banka_tcmb_subekod6;

	private string _cari_banka_tcmb_ilkod6;

	private string _cari_banka_hesapno6;

	private string _cari_banka_swiftkodu6;

	private string _cari_banka_tcmb_kod7;

	private string _cari_banka_tcmb_subekod7;

	private string _cari_banka_tcmb_ilkod7;

	private string _cari_banka_hesapno7;

	private string _cari_banka_swiftkodu7;

	private string _cari_banka_tcmb_kod8;

	private string _cari_banka_tcmb_subekod8;

	private string _cari_banka_tcmb_ilkod8;

	private string _cari_banka_hesapno8;

	private string _cari_banka_swiftkodu8;

	private string _cari_banka_tcmb_kod9;

	private string _cari_banka_tcmb_subekod9;

	private string _cari_banka_tcmb_ilkod9;

	private string _cari_banka_hesapno9;

	private string _cari_banka_swiftkodu9;

	private string _cari_banka_tcmb_kod10;

	private string _cari_banka_tcmb_subekod10;

	private string _cari_banka_tcmb_ilkod10;

	private string _cari_banka_hesapno10;

	private string _cari_banka_swiftkodu10;

	private bool _cari_efatura_fl;

	private enum_cari_odeme_sekli _cari_odeme_sekli;

	private string _cari_TeminatMekAlacakMuhKodu;

	private string _cari_TeminatMekAlacakMuhKodu1;

	private string _cari_TeminatMekAlacakMuhKodu2;

	private string _cari_TeminatMekBorcMuhKodu;

	private string _cari_TeminatMekBorcMuhKodu1;

	private string _cari_TeminatMekBorcMuhKodu2;

	private string _cari_VerilenDepozitoTeminatMuhKodu;

	private string _cari_VerilenDepozitoTeminatMuhKodu1;

	private string _cari_VerilenDepozitoTeminatMuhKodu2;

	private string _cari_AlinanDepozitoTeminatMuhKodu;

	private string _cari_AlinanDepozitoTeminatMuhKodu1;

	private string _cari_AlinanDepozitoTeminatMuhKodu2;

	private enum_cari_def_efatura_cinsi _cari_def_efatura_cinsi;

	private bool _cari_otv_tevkifatina_tabii_fl;

	public string cari_kod_prefix
	{
		get
		{
			return _cari_kod_prefix;
		}
		set
		{
			_cari_kod_prefix = value;
			if (_cari_kod_prefix.Length > 25)
			{
				_cari_kod_prefix = _cari_kod_prefix.Substring(0, 25);
			}
		}
	}

	public string cari_kod
	{
		get
		{
			return _cari_kod;
		}
		set
		{
			_cari_kod = value;
			if (_cari_kod.Length > 25)
			{
				_cari_kod = _cari_kod.Substring(0, 25);
			}
		}
	}

	public string cari_unvan1
	{
		get
		{
			return _cari_unvan1;
		}
		set
		{
			_cari_unvan1 = value;
			if (_cari_unvan1.Length > 50)
			{
				_cari_unvan1 = _cari_unvan1.Substring(0, 50);
			}
		}
	}

	public string cari_unvan2
	{
		get
		{
			return _cari_unvan2;
		}
		set
		{
			_cari_unvan2 = value;
			if (_cari_unvan2.Length > 50)
			{
				_cari_unvan2 = _cari_unvan2.Substring(0, 50);
			}
		}
	}

	public string cari_muh_kod
	{
		get
		{
			return _cari_muh_kod;
		}
		set
		{
			_cari_muh_kod = value;
			if (_cari_muh_kod.Length > 40)
			{
				_cari_muh_kod = _cari_muh_kod.Substring(0, 40);
			}
		}
	}

	public string cari_muh_kod1
	{
		get
		{
			return _cari_muh_kod1;
		}
		set
		{
			_cari_muh_kod1 = value;
			if (_cari_muh_kod1.Length > 40)
			{
				_cari_muh_kod1 = _cari_muh_kod1.Substring(0, 40);
			}
		}
	}

	public string cari_muh_kod2
	{
		get
		{
			return _cari_muh_kod2;
		}
		set
		{
			_cari_muh_kod2 = value;
			if (_cari_muh_kod2.Length > 40)
			{
				_cari_muh_kod2 = _cari_muh_kod2.Substring(0, 40);
			}
		}
	}

	public string cari_muhartikeli
	{
		get
		{
			return _cari_muhartikeli;
		}
		set
		{
			_cari_muhartikeli = value;
			if (_cari_muhartikeli.Length > 10)
			{
				_cari_muhartikeli = _cari_muhartikeli.Substring(0, 10);
			}
		}
	}

	public string cari_vdaire_adi
	{
		get
		{
			return _cari_vdaire_adi;
		}
		set
		{
			_cari_vdaire_adi = value;
			if (_cari_vdaire_adi.Length > 50)
			{
				_cari_vdaire_adi = _cari_vdaire_adi.Substring(0, 50);
			}
		}
	}

	public string cari_vdaire_no
	{
		get
		{
			return _cari_vdaire_no;
		}
		set
		{
			_cari_vdaire_no = value;
			if (_cari_vdaire_no.Length > 15)
			{
				_cari_vdaire_no = _cari_vdaire_no.Substring(0, 15);
			}
		}
	}

	public string cari_Ana_cari_kodu
	{
		get
		{
			return _cari_Ana_cari_kodu;
		}
		set
		{
			_cari_Ana_cari_kodu = value;
			if (_cari_Ana_cari_kodu.Length > 25)
			{
				_cari_Ana_cari_kodu = _cari_Ana_cari_kodu.Substring(0, 25);
			}
		}
	}

	public string cari_bolge_kodu
	{
		get
		{
			return _cari_bolge_kodu;
		}
		set
		{
			_cari_bolge_kodu = value;
			if (_cari_bolge_kodu.Length > 25)
			{
				_cari_bolge_kodu = _cari_bolge_kodu.Substring(0, 25);
			}
		}
	}

	public string cari_grup_kodu
	{
		get
		{
			return _cari_grup_kodu;
		}
		set
		{
			_cari_grup_kodu = value;
			if (_cari_grup_kodu.Length > 25)
			{
				_cari_grup_kodu = _cari_grup_kodu.Substring(0, 25);
			}
		}
	}

	public string cari_temsilci_kodu
	{
		get
		{
			return _cari_temsilci_kodu;
		}
		set
		{
			_cari_temsilci_kodu = value;
			if (_cari_temsilci_kodu.Length > 25)
			{
				_cari_temsilci_kodu = _cari_temsilci_kodu.Substring(0, 25);
			}
		}
	}

	public string cari_sektor_kodu
	{
		get
		{
			return _cari_sektor_kodu;
		}
		set
		{
			_cari_sektor_kodu = value;
			if (_cari_sektor_kodu.Length > 25)
			{
				_cari_sektor_kodu = _cari_sektor_kodu.Substring(0, 25);
			}
		}
	}

	public string cari_satis_isk_kod
	{
		get
		{
			return _cari_satis_isk_kod;
		}
		set
		{
			_cari_satis_isk_kod = value;
			if (_cari_satis_isk_kod.Length > 4)
			{
				_cari_satis_isk_kod = _cari_satis_isk_kod.Substring(0, 4);
			}
		}
	}

	public string cari_special1
	{
		get
		{
			return _cari_special1;
		}
		set
		{
			_cari_special1 = value;
			if (_cari_special1.Length > 4)
			{
				_cari_special1 = _cari_special1.Substring(0, 4);
			}
		}
	}

	public string cari_special2
	{
		get
		{
			return _cari_special2;
		}
		set
		{
			_cari_special2 = value;
			if (_cari_special2.Length > 4)
			{
				_cari_special2 = _cari_special2.Substring(0, 4);
			}
		}
	}

	public string cari_special3
	{
		get
		{
			return _cari_special3;
		}
		set
		{
			_cari_special3 = value;
			if (_cari_special3.Length > 4)
			{
				_cari_special3 = _cari_special3.Substring(0, 4);
			}
		}
	}

	public string cari_sicil_no
	{
		get
		{
			return _cari_sicil_no;
		}
		set
		{
			_cari_sicil_no = value;
			if (_cari_sicil_no.Length > 15)
			{
				_cari_sicil_no = _cari_sicil_no.Substring(0, 15);
			}
		}
	}

	public string cari_VergiKimlikNo
	{
		get
		{
			return _cari_VergiKimlikNo;
		}
		set
		{
			_cari_VergiKimlikNo = value;
			if (_cari_VergiKimlikNo.Length > 10)
			{
				_cari_VergiKimlikNo = _cari_VergiKimlikNo.Substring(0, 10);
			}
		}
	}

	public string cari_banka_hesapno1
	{
		get
		{
			return _cari_banka_hesapno1;
		}
		set
		{
			_cari_banka_hesapno1 = value;
			if (_cari_banka_hesapno1.Length > 30)
			{
				_cari_banka_hesapno1 = _cari_banka_hesapno1.Substring(0, 30);
			}
		}
	}

	public float cari_vade_fark_yuz
	{
		get
		{
			return _cari_vade_fark_yuz;
		}
		set
		{
			_cari_vade_fark_yuz = value;
		}
	}

	public float cari_vade_fark_yuz1
	{
		get
		{
			return _cari_vade_fark_yuz1;
		}
		set
		{
			_cari_vade_fark_yuz1 = value;
		}
	}

	public float cari_vade_fark_yuz2
	{
		get
		{
			return _cari_vade_fark_yuz2;
		}
		set
		{
			_cari_vade_fark_yuz2 = value;
		}
	}

	public int cari_tipi
	{
		get
		{
			return _cari_tipi;
		}
		set
		{
			_cari_tipi = value;
		}
	}

	public int cari_doviz_cinsi
	{
		get
		{
			return _cari_doviz_cinsi;
		}
		set
		{
			_cari_doviz_cinsi = value;
		}
	}

	public int cari_doviz_cinsi1
	{
		get
		{
			return _cari_doviz_cinsi1;
		}
		set
		{
			_cari_doviz_cinsi1 = value;
		}
	}

	public int cari_doviz_cinsi2
	{
		get
		{
			return _cari_doviz_cinsi2;
		}
		set
		{
			_cari_doviz_cinsi2 = value;
		}
	}

	public int cari_odeme_gunu
	{
		get
		{
			return _cari_odeme_gunu;
		}
		set
		{
			_cari_odeme_gunu = value;
		}
	}

	public int cari_hareket_tipi
	{
		get
		{
			return _cari_hareket_tipi;
		}
		set
		{
			_cari_hareket_tipi = value;
		}
	}

	public int cari_odemeplan_no
	{
		get
		{
			return _cari_odemeplan_no;
		}
		set
		{
			_cari_odemeplan_no = value;
		}
	}

	public int cari_satis_fk
	{
		get
		{
			return _cari_satis_fk;
		}
		set
		{
			_cari_satis_fk = value;
		}
	}

	public int cari_KurHesapSekli
	{
		get
		{
			return _cari_KurHesapSekli;
		}
		set
		{
			_cari_KurHesapSekli = value;
		}
	}

	public int cari_odeme_cinsi
	{
		get
		{
			return _cari_odeme_cinsi;
		}
		set
		{
			_cari_odeme_cinsi = value;
		}
	}

	public int cari_fatura_adres_no
	{
		get
		{
			return _cari_fatura_adres_no;
		}
		set
		{
			_cari_fatura_adres_no = value;
		}
	}

	public int cari_sevk_adres_no
	{
		get
		{
			return _cari_sevk_adres_no;
		}
		set
		{
			_cari_sevk_adres_no = value;
		}
	}

	public int cari_VarsayilanGirisDepo
	{
		get
		{
			return _cari_VarsayilanGirisDepo;
		}
		set
		{
			_cari_VarsayilanGirisDepo = value;
		}
	}

	public int cari_VarsayilanCikisDepo
	{
		get
		{
			return _cari_VarsayilanCikisDepo;
		}
		set
		{
			_cari_VarsayilanCikisDepo = value;
		}
	}

	public string cari_CepTel
	{
		get
		{
			return _cari_CepTel;
		}
		set
		{
			_cari_CepTel = value;
			if (_cari_CepTel.Length > 20)
			{
				_cari_CepTel = _cari_CepTel.Substring(0, 20);
			}
		}
	}

	public string cari_Email
	{
		get
		{
			return _cari_Email;
		}
		set
		{
			_cari_Email = value;
			if (_cari_Email.Length > 80)
			{
				_cari_Email = _cari_Email.Substring(0, 80);
			}
		}
	}

	public bool cari_cari_kilitli_flg
	{
		get
		{
			return _cari_cari_kilitli_flg;
		}
		set
		{
			_cari_cari_kilitli_flg = value;
		}
	}

	public string cari_wwwadresi
	{
		get
		{
			return _cari_wwwadresi;
		}
		set
		{
			_cari_wwwadresi = value;
			if (_cari_wwwadresi.Length > 30)
			{
				_cari_wwwadresi = _cari_wwwadresi.Substring(0, 30);
			}
		}
	}

	public string cari_Portal_PW
	{
		get
		{
			return _cari_Portal_PW;
		}
		set
		{
			_cari_Portal_PW = value;
			if (_cari_Portal_PW.Length > 127)
			{
				_cari_Portal_PW = _cari_Portal_PW.Substring(0, 127);
			}
		}
	}

	public bool cari_Portal_Enabled
	{
		get
		{
			return _cari_Portal_Enabled;
		}
		set
		{
			_cari_Portal_Enabled = value;
		}
	}

	public List<CariAdres> CariAdresleri { get; set; }

	public List<CariYetkili> CariYetkilileri { get; set; }

	public enum_cari_stok_alim_cinsi cari_stok_alim_cinsi
	{
		get
		{
			return _cari_stok_alim_cinsi;
		}
		set
		{
			_cari_stok_alim_cinsi = value;
		}
	}

	public enum_cari_stok_satim_cinsi cari_stok_satim_cinsi
	{
		get
		{
			return _cari_stok_satim_cinsi;
		}
		set
		{
			_cari_stok_satim_cinsi = value;
		}
	}

	public string cari_banka_swiftkodu1
	{
		get
		{
			return _cari_banka_swiftkodu1;
		}
		set
		{
			_cari_banka_swiftkodu1 = value;
			if (_cari_banka_swiftkodu1.Length > 25)
			{
				_cari_banka_swiftkodu1 = _cari_banka_swiftkodu1.Substring(0, 25);
			}
		}
	}

	public string cari_banka_swiftkodu2
	{
		get
		{
			return _cari_banka_swiftkodu2;
		}
		set
		{
			_cari_banka_swiftkodu2 = value;
			if (_cari_banka_swiftkodu2.Length > 25)
			{
				_cari_banka_swiftkodu2 = _cari_banka_swiftkodu2.Substring(0, 25);
			}
		}
	}

	public string cari_banka_swiftkodu3
	{
		get
		{
			return _cari_banka_swiftkodu3;
		}
		set
		{
			_cari_banka_swiftkodu3 = value;
			if (_cari_banka_swiftkodu3.Length > 25)
			{
				_cari_banka_swiftkodu3 = _cari_banka_swiftkodu3.Substring(0, 25);
			}
		}
	}

	public string cari_banka_tcmb_kod4
	{
		get
		{
			return _cari_banka_tcmb_kod4;
		}
		set
		{
			_cari_banka_tcmb_kod4 = value;
			if (_cari_banka_tcmb_kod4.Length > 4)
			{
				_cari_banka_tcmb_kod4 = _cari_banka_tcmb_kod4.Substring(0, 4);
			}
		}
	}

	public string cari_banka_tcmb_subekod4
	{
		get
		{
			return _cari_banka_tcmb_subekod4;
		}
		set
		{
			_cari_banka_tcmb_subekod4 = value;
			if (_cari_banka_tcmb_subekod4.Length > 8)
			{
				_cari_banka_tcmb_subekod4 = _cari_banka_tcmb_subekod4.Substring(0, 8);
			}
		}
	}

	public string cari_banka_tcmb_ilkod4
	{
		get
		{
			return _cari_banka_tcmb_ilkod4;
		}
		set
		{
			_cari_banka_tcmb_ilkod4 = value;
			if (_cari_banka_tcmb_ilkod4.Length > 3)
			{
				_cari_banka_tcmb_ilkod4 = _cari_banka_tcmb_ilkod4.Substring(0, 3);
			}
		}
	}

	public string cari_banka_hesapno4
	{
		get
		{
			return _cari_banka_hesapno4;
		}
		set
		{
			_cari_banka_hesapno4 = value;
			if (_cari_banka_hesapno4.Length > 30)
			{
				_cari_banka_hesapno4 = _cari_banka_hesapno4.Substring(0, 30);
			}
		}
	}

	public string cari_banka_swiftkodu4
	{
		get
		{
			return _cari_banka_swiftkodu4;
		}
		set
		{
			_cari_banka_swiftkodu4 = value;
			if (_cari_banka_swiftkodu4.Length > 25)
			{
				_cari_banka_swiftkodu4 = _cari_banka_swiftkodu4.Substring(0, 25);
			}
		}
	}

	public string cari_banka_tcmb_kod5
	{
		get
		{
			return _cari_banka_tcmb_kod5;
		}
		set
		{
			_cari_banka_tcmb_kod5 = value;
			if (_cari_banka_tcmb_kod5.Length > 4)
			{
				_cari_banka_tcmb_kod5 = _cari_banka_tcmb_kod5.Substring(0, 4);
			}
		}
	}

	public string cari_banka_tcmb_subekod5
	{
		get
		{
			return _cari_banka_tcmb_subekod5;
		}
		set
		{
			_cari_banka_tcmb_subekod5 = value;
			if (_cari_banka_tcmb_subekod5.Length > 8)
			{
				_cari_banka_tcmb_subekod5 = _cari_banka_tcmb_subekod5.Substring(0, 8);
			}
		}
	}

	public string cari_banka_tcmb_ilkod5
	{
		get
		{
			return _cari_banka_tcmb_ilkod5;
		}
		set
		{
			_cari_banka_tcmb_ilkod5 = value;
			if (_cari_banka_tcmb_ilkod5.Length > 3)
			{
				_cari_banka_tcmb_ilkod5 = _cari_banka_tcmb_ilkod5.Substring(0, 3);
			}
		}
	}

	public string cari_banka_hesapno5
	{
		get
		{
			return _cari_banka_hesapno5;
		}
		set
		{
			_cari_banka_hesapno5 = value;
			if (_cari_banka_hesapno5.Length > 30)
			{
				_cari_banka_hesapno5 = _cari_banka_hesapno5.Substring(0, 30);
			}
		}
	}

	public string cari_banka_swiftkodu5
	{
		get
		{
			return _cari_banka_swiftkodu5;
		}
		set
		{
			_cari_banka_swiftkodu5 = value;
			if (_cari_banka_swiftkodu5.Length > 25)
			{
				_cari_banka_swiftkodu5 = _cari_banka_swiftkodu5.Substring(0, 25);
			}
		}
	}

	public string cari_banka_tcmb_kod6
	{
		get
		{
			return _cari_banka_tcmb_kod6;
		}
		set
		{
			_cari_banka_tcmb_kod6 = value;
			if (_cari_banka_tcmb_kod6.Length > 4)
			{
				_cari_banka_tcmb_kod6 = _cari_banka_tcmb_kod6.Substring(0, 4);
			}
		}
	}

	public string cari_banka_tcmb_subekod6
	{
		get
		{
			return _cari_banka_tcmb_subekod6;
		}
		set
		{
			_cari_banka_tcmb_subekod6 = value;
			if (_cari_banka_tcmb_subekod6.Length > 8)
			{
				_cari_banka_tcmb_subekod6 = _cari_banka_tcmb_subekod6.Substring(0, 8);
			}
		}
	}

	public string cari_banka_tcmb_ilkod6
	{
		get
		{
			return _cari_banka_tcmb_ilkod6;
		}
		set
		{
			_cari_banka_tcmb_ilkod6 = value;
			if (_cari_banka_tcmb_ilkod6.Length > 3)
			{
				_cari_banka_tcmb_ilkod6 = _cari_banka_tcmb_ilkod6.Substring(0, 3);
			}
		}
	}

	public string cari_banka_hesapno6
	{
		get
		{
			return _cari_banka_hesapno6;
		}
		set
		{
			_cari_banka_hesapno6 = value;
			if (_cari_banka_hesapno6.Length > 30)
			{
				_cari_banka_hesapno6 = _cari_banka_hesapno6.Substring(0, 30);
			}
		}
	}

	public string cari_banka_swiftkodu6
	{
		get
		{
			return _cari_banka_swiftkodu6;
		}
		set
		{
			_cari_banka_swiftkodu6 = value;
			if (_cari_banka_swiftkodu6.Length > 25)
			{
				_cari_banka_swiftkodu6 = _cari_banka_swiftkodu6.Substring(0, 25);
			}
		}
	}

	public string cari_banka_tcmb_kod7
	{
		get
		{
			return _cari_banka_tcmb_kod7;
		}
		set
		{
			_cari_banka_tcmb_kod7 = value;
			if (_cari_banka_tcmb_kod7.Length > 4)
			{
				_cari_banka_tcmb_kod7 = _cari_banka_tcmb_kod7.Substring(0, 4);
			}
		}
	}

	public string cari_banka_tcmb_subekod7
	{
		get
		{
			return _cari_banka_tcmb_subekod7;
		}
		set
		{
			_cari_banka_tcmb_subekod7 = value;
			if (_cari_banka_tcmb_subekod7.Length > 8)
			{
				_cari_banka_tcmb_subekod7 = _cari_banka_tcmb_subekod7.Substring(0, 8);
			}
		}
	}

	public string cari_banka_tcmb_ilkod7
	{
		get
		{
			return _cari_banka_tcmb_ilkod7;
		}
		set
		{
			_cari_banka_tcmb_ilkod7 = value;
			if (_cari_banka_tcmb_ilkod7.Length > 3)
			{
				_cari_banka_tcmb_ilkod7 = _cari_banka_tcmb_ilkod7.Substring(0, 3);
			}
		}
	}

	public string cari_banka_hesapno7
	{
		get
		{
			return _cari_banka_hesapno7;
		}
		set
		{
			_cari_banka_hesapno7 = value;
			if (_cari_banka_hesapno7.Length > 30)
			{
				_cari_banka_hesapno7 = _cari_banka_hesapno7.Substring(0, 30);
			}
		}
	}

	public string cari_banka_swiftkodu7
	{
		get
		{
			return _cari_banka_swiftkodu7;
		}
		set
		{
			_cari_banka_swiftkodu7 = value;
			if (_cari_banka_swiftkodu7.Length > 25)
			{
				_cari_banka_swiftkodu7 = _cari_banka_swiftkodu7.Substring(0, 25);
			}
		}
	}

	public string cari_banka_tcmb_kod8
	{
		get
		{
			return _cari_banka_tcmb_kod8;
		}
		set
		{
			_cari_banka_tcmb_kod8 = value;
			if (_cari_banka_tcmb_kod8.Length > 4)
			{
				_cari_banka_tcmb_kod8 = _cari_banka_tcmb_kod8.Substring(0, 4);
			}
		}
	}

	public string cari_banka_tcmb_subekod8
	{
		get
		{
			return _cari_banka_tcmb_subekod8;
		}
		set
		{
			_cari_banka_tcmb_subekod8 = value;
			if (_cari_banka_tcmb_subekod8.Length > 8)
			{
				_cari_banka_tcmb_subekod8 = _cari_banka_tcmb_subekod8.Substring(0, 8);
			}
		}
	}

	public string cari_banka_tcmb_ilkod8
	{
		get
		{
			return _cari_banka_tcmb_ilkod8;
		}
		set
		{
			_cari_banka_tcmb_ilkod8 = value;
			if (_cari_banka_tcmb_ilkod8.Length > 3)
			{
				_cari_banka_tcmb_ilkod8 = _cari_banka_tcmb_ilkod8.Substring(0, 3);
			}
		}
	}

	public string cari_banka_hesapno8
	{
		get
		{
			return _cari_banka_hesapno8;
		}
		set
		{
			_cari_banka_hesapno8 = value;
			if (_cari_banka_hesapno8.Length > 30)
			{
				_cari_banka_hesapno8 = _cari_banka_hesapno8.Substring(0, 30);
			}
		}
	}

	public string cari_banka_swiftkodu8
	{
		get
		{
			return _cari_banka_swiftkodu8;
		}
		set
		{
			_cari_banka_swiftkodu8 = value;
			if (_cari_banka_swiftkodu8.Length > 25)
			{
				_cari_banka_swiftkodu8 = _cari_banka_swiftkodu8.Substring(0, 25);
			}
		}
	}

	public string cari_banka_tcmb_kod9
	{
		get
		{
			return _cari_banka_tcmb_kod9;
		}
		set
		{
			_cari_banka_tcmb_kod9 = value;
			if (_cari_banka_tcmb_kod9.Length > 4)
			{
				_cari_banka_tcmb_kod9 = _cari_banka_tcmb_kod9.Substring(0, 4);
			}
		}
	}

	public string cari_banka_tcmb_subekod9
	{
		get
		{
			return _cari_banka_tcmb_subekod9;
		}
		set
		{
			_cari_banka_tcmb_subekod9 = value;
			if (_cari_banka_tcmb_subekod9.Length > 8)
			{
				_cari_banka_tcmb_subekod9 = _cari_banka_tcmb_subekod9.Substring(0, 8);
			}
		}
	}

	public string cari_banka_tcmb_ilkod9
	{
		get
		{
			return _cari_banka_tcmb_ilkod9;
		}
		set
		{
			_cari_banka_tcmb_ilkod9 = value;
			if (_cari_banka_tcmb_ilkod9.Length > 3)
			{
				_cari_banka_tcmb_ilkod9 = _cari_banka_tcmb_ilkod9.Substring(0, 3);
			}
		}
	}

	public string cari_banka_hesapno9
	{
		get
		{
			return _cari_banka_hesapno9;
		}
		set
		{
			_cari_banka_hesapno9 = value;
			if (_cari_banka_hesapno9.Length > 30)
			{
				_cari_banka_hesapno9 = _cari_banka_hesapno9.Substring(0, 30);
			}
		}
	}

	public string cari_banka_swiftkodu9
	{
		get
		{
			return _cari_banka_swiftkodu9;
		}
		set
		{
			_cari_banka_swiftkodu9 = value;
			if (_cari_banka_swiftkodu9.Length > 25)
			{
				_cari_banka_swiftkodu9 = _cari_banka_swiftkodu9.Substring(0, 25);
			}
		}
	}

	public string cari_banka_tcmb_kod10
	{
		get
		{
			return _cari_banka_tcmb_kod10;
		}
		set
		{
			_cari_banka_tcmb_kod10 = value;
			if (_cari_banka_tcmb_kod10.Length > 4)
			{
				_cari_banka_tcmb_kod10 = _cari_banka_tcmb_kod10.Substring(0, 4);
			}
		}
	}

	public string cari_banka_tcmb_subekod10
	{
		get
		{
			return _cari_banka_tcmb_subekod10;
		}
		set
		{
			_cari_banka_tcmb_subekod10 = value;
			if (_cari_banka_tcmb_subekod10.Length > 8)
			{
				_cari_banka_tcmb_subekod10 = _cari_banka_tcmb_subekod10.Substring(0, 8);
			}
		}
	}

	public string cari_banka_tcmb_ilkod10
	{
		get
		{
			return _cari_banka_tcmb_ilkod10;
		}
		set
		{
			_cari_banka_tcmb_ilkod10 = value;
			if (_cari_banka_tcmb_ilkod10.Length > 3)
			{
				_cari_banka_tcmb_ilkod10 = _cari_banka_tcmb_ilkod10.Substring(0, 3);
			}
		}
	}

	public string cari_banka_hesapno10
	{
		get
		{
			return _cari_banka_hesapno10;
		}
		set
		{
			_cari_banka_hesapno10 = value;
			if (_cari_banka_hesapno10.Length > 30)
			{
				_cari_banka_hesapno10 = _cari_banka_hesapno10.Substring(0, 30);
			}
		}
	}

	public string cari_banka_swiftkodu10
	{
		get
		{
			return _cari_banka_swiftkodu10;
		}
		set
		{
			_cari_banka_swiftkodu10 = value;
			if (_cari_banka_swiftkodu10.Length > 25)
			{
				_cari_banka_swiftkodu10 = _cari_banka_swiftkodu10.Substring(0, 25);
			}
		}
	}

	public bool cari_efatura_fl
	{
		get
		{
			return _cari_efatura_fl;
		}
		set
		{
			_cari_efatura_fl = value;
		}
	}

	public enum_cari_odeme_sekli cari_odeme_sekli
	{
		get
		{
			return _cari_odeme_sekli;
		}
		set
		{
			_cari_odeme_sekli = value;
		}
	}

	public string cari_TeminatMekAlacakMuhKodu
	{
		get
		{
			return _cari_TeminatMekAlacakMuhKodu;
		}
		set
		{
			_cari_TeminatMekAlacakMuhKodu = value;
			if (_cari_TeminatMekAlacakMuhKodu.Length > 40)
			{
				_cari_TeminatMekAlacakMuhKodu = _cari_TeminatMekAlacakMuhKodu.Substring(0, 40);
			}
		}
	}

	public string cari_TeminatMekAlacakMuhKodu1
	{
		get
		{
			return _cari_TeminatMekAlacakMuhKodu1;
		}
		set
		{
			_cari_TeminatMekAlacakMuhKodu1 = value;
			if (_cari_TeminatMekAlacakMuhKodu1.Length > 40)
			{
				_cari_TeminatMekAlacakMuhKodu1 = _cari_TeminatMekAlacakMuhKodu1.Substring(0, 40);
			}
		}
	}

	public string cari_TeminatMekAlacakMuhKodu2
	{
		get
		{
			return _cari_TeminatMekAlacakMuhKodu2;
		}
		set
		{
			_cari_TeminatMekAlacakMuhKodu2 = value;
			if (_cari_TeminatMekAlacakMuhKodu2.Length > 40)
			{
				_cari_TeminatMekAlacakMuhKodu2 = _cari_TeminatMekAlacakMuhKodu2.Substring(0, 40);
			}
		}
	}

	public string cari_TeminatMekBorcMuhKodu
	{
		get
		{
			return _cari_TeminatMekBorcMuhKodu;
		}
		set
		{
			_cari_TeminatMekBorcMuhKodu = value;
			if (_cari_TeminatMekBorcMuhKodu.Length > 40)
			{
				_cari_TeminatMekBorcMuhKodu = _cari_TeminatMekBorcMuhKodu.Substring(0, 40);
			}
		}
	}

	public string cari_TeminatMekBorcMuhKodu1
	{
		get
		{
			return _cari_TeminatMekBorcMuhKodu1;
		}
		set
		{
			_cari_TeminatMekBorcMuhKodu1 = value;
			if (_cari_TeminatMekBorcMuhKodu1.Length > 40)
			{
				_cari_TeminatMekBorcMuhKodu1 = _cari_TeminatMekBorcMuhKodu1.Substring(0, 40);
			}
		}
	}

	public string cari_TeminatMekBorcMuhKodu2
	{
		get
		{
			return _cari_TeminatMekBorcMuhKodu2;
		}
		set
		{
			_cari_TeminatMekBorcMuhKodu2 = value;
			if (_cari_TeminatMekBorcMuhKodu2.Length > 40)
			{
				_cari_TeminatMekBorcMuhKodu2 = _cari_TeminatMekBorcMuhKodu2.Substring(0, 40);
			}
		}
	}

	public string cari_VerilenDepozitoTeminatMuhKodu
	{
		get
		{
			return _cari_VerilenDepozitoTeminatMuhKodu;
		}
		set
		{
			_cari_VerilenDepozitoTeminatMuhKodu = value;
			if (_cari_VerilenDepozitoTeminatMuhKodu.Length > 40)
			{
				_cari_VerilenDepozitoTeminatMuhKodu = _cari_VerilenDepozitoTeminatMuhKodu.Substring(0, 40);
			}
		}
	}

	public string cari_VerilenDepozitoTeminatMuhKodu1
	{
		get
		{
			return _cari_VerilenDepozitoTeminatMuhKodu1;
		}
		set
		{
			_cari_VerilenDepozitoTeminatMuhKodu1 = value;
			if (_cari_VerilenDepozitoTeminatMuhKodu1.Length > 40)
			{
				_cari_VerilenDepozitoTeminatMuhKodu1 = _cari_VerilenDepozitoTeminatMuhKodu1.Substring(0, 40);
			}
		}
	}

	public string cari_VerilenDepozitoTeminatMuhKodu2
	{
		get
		{
			return _cari_VerilenDepozitoTeminatMuhKodu2;
		}
		set
		{
			_cari_VerilenDepozitoTeminatMuhKodu2 = value;
			if (_cari_VerilenDepozitoTeminatMuhKodu2.Length > 40)
			{
				_cari_VerilenDepozitoTeminatMuhKodu2 = _cari_VerilenDepozitoTeminatMuhKodu2.Substring(0, 40);
			}
		}
	}

	public string cari_AlinanDepozitoTeminatMuhKodu
	{
		get
		{
			return _cari_AlinanDepozitoTeminatMuhKodu;
		}
		set
		{
			_cari_AlinanDepozitoTeminatMuhKodu = value;
			if (_cari_AlinanDepozitoTeminatMuhKodu.Length > 40)
			{
				_cari_AlinanDepozitoTeminatMuhKodu = _cari_AlinanDepozitoTeminatMuhKodu.Substring(0, 40);
			}
		}
	}

	public string cari_AlinanDepozitoTeminatMuhKodu1
	{
		get
		{
			return _cari_AlinanDepozitoTeminatMuhKodu1;
		}
		set
		{
			_cari_AlinanDepozitoTeminatMuhKodu1 = value;
			if (_cari_AlinanDepozitoTeminatMuhKodu1.Length > 40)
			{
				_cari_AlinanDepozitoTeminatMuhKodu1 = _cari_AlinanDepozitoTeminatMuhKodu1.Substring(0, 40);
			}
		}
	}

	public string cari_AlinanDepozitoTeminatMuhKodu2
	{
		get
		{
			return _cari_AlinanDepozitoTeminatMuhKodu2;
		}
		set
		{
			_cari_AlinanDepozitoTeminatMuhKodu2 = value;
			if (_cari_AlinanDepozitoTeminatMuhKodu2.Length > 40)
			{
				_cari_AlinanDepozitoTeminatMuhKodu2 = _cari_AlinanDepozitoTeminatMuhKodu2.Substring(0, 40);
			}
		}
	}

	public enum_cari_def_efatura_cinsi cari_def_efatura_cinsi
	{
		get
		{
			return _cari_def_efatura_cinsi;
		}
		set
		{
			_cari_def_efatura_cinsi = value;
		}
	}

	public bool cari_otv_tevkifatina_tabii_fl
	{
		get
		{
			return _cari_otv_tevkifatina_tabii_fl;
		}
		set
		{
			_cari_otv_tevkifatina_tabii_fl = value;
		}
	}

	public Cari()
	{
		_cari_fatura_adres_no = 1;
		_cari_sevk_adres_no = 1;
		_cari_vade_fark_yuz = 25f;
		_cari_kod_prefix = "";
		_cari_kod = "";
		_cari_unvan1 = "";
		_cari_unvan2 = "";
		_cari_muh_kod = "";
		_cari_muh_kod1 = "";
		_cari_muh_kod2 = "";
		_cari_muhartikeli = "";
		_cari_vdaire_adi = "";
		_cari_vdaire_no = "";
		_cari_Ana_cari_kodu = "";
		_cari_bolge_kodu = "";
		_cari_grup_kodu = "";
		_cari_temsilci_kodu = "";
		_cari_sektor_kodu = "";
		_cari_satis_isk_kod = "";
		_cari_special1 = "";
		_cari_special2 = "";
		_cari_special3 = "";
		_cari_sicil_no = "";
		_cari_VergiKimlikNo = "";
		_cari_banka_hesapno1 = "";
		_cari_CepTel = "";
		_cari_Email = "";
		_cari_wwwadresi = "";
		_cari_Portal_PW = "";
		_cari_Portal_Enabled = false;
		_cari_VarsayilanGirisDepo = 0;
		_cari_VarsayilanCikisDepo = 0;
		_cari_KurHesapSekli = 1;
		_cari_satis_fk = 1;
		_cari_cari_kilitli_flg = false;
		CariAdresleri = new List<CariAdres>();
		CariYetkilileri = new List<CariYetkili>();
		_cari_stok_alim_cinsi = enum_cari_stok_alim_cinsi.ToptanVePerakende;
		_cari_stok_satim_cinsi = enum_cari_stok_satim_cinsi.ToptanVePerakende;
		_cari_banka_swiftkodu1 = "";
		_cari_banka_swiftkodu2 = "";
		_cari_banka_swiftkodu3 = "";
		_cari_banka_tcmb_kod4 = "";
		_cari_banka_tcmb_subekod4 = "";
		_cari_banka_tcmb_ilkod4 = "";
		_cari_banka_hesapno4 = "";
		_cari_banka_swiftkodu4 = "";
		_cari_banka_tcmb_kod5 = "";
		_cari_banka_tcmb_subekod5 = "";
		_cari_banka_tcmb_ilkod5 = "";
		_cari_banka_hesapno5 = "";
		_cari_banka_swiftkodu5 = "";
		_cari_banka_tcmb_kod6 = "";
		_cari_banka_tcmb_subekod6 = "";
		_cari_banka_tcmb_ilkod6 = "";
		_cari_banka_hesapno6 = "";
		_cari_banka_swiftkodu6 = "";
		_cari_banka_tcmb_kod7 = "";
		_cari_banka_tcmb_subekod7 = "";
		_cari_banka_tcmb_ilkod7 = "";
		_cari_banka_hesapno7 = "";
		_cari_banka_swiftkodu7 = "";
		_cari_banka_tcmb_kod8 = "";
		_cari_banka_tcmb_subekod8 = "";
		_cari_banka_tcmb_ilkod8 = "";
		_cari_banka_hesapno8 = "";
		_cari_banka_swiftkodu8 = "";
		_cari_banka_tcmb_kod9 = "";
		_cari_banka_tcmb_subekod9 = "";
		_cari_banka_tcmb_ilkod9 = "";
		_cari_banka_hesapno9 = "";
		_cari_banka_swiftkodu9 = "";
		_cari_banka_tcmb_kod10 = "";
		_cari_banka_tcmb_subekod10 = "";
		_cari_banka_tcmb_ilkod10 = "";
		_cari_banka_hesapno10 = "";
		_cari_banka_swiftkodu10 = "";
		_cari_efatura_fl = false;
		_cari_odeme_sekli = enum_cari_odeme_sekli.Serbest;
		_cari_TeminatMekAlacakMuhKodu = "";
		_cari_TeminatMekAlacakMuhKodu1 = "";
		_cari_TeminatMekAlacakMuhKodu2 = "";
		_cari_TeminatMekBorcMuhKodu = "";
		_cari_TeminatMekBorcMuhKodu1 = "";
		_cari_TeminatMekBorcMuhKodu2 = "";
		_cari_VerilenDepozitoTeminatMuhKodu = "";
		_cari_VerilenDepozitoTeminatMuhKodu1 = "";
		_cari_VerilenDepozitoTeminatMuhKodu2 = "";
		_cari_AlinanDepozitoTeminatMuhKodu = "";
		_cari_AlinanDepozitoTeminatMuhKodu1 = "";
		_cari_AlinanDepozitoTeminatMuhKodu2 = "";
		_cari_def_efatura_cinsi = enum_cari_def_efatura_cinsi.TemelFatura;
		_cari_otv_tevkifatina_tabii_fl = false;
	}

	public static byte[] WriteToByteArray(Cari toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Cari toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Cari toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 3;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.cari_kod);
		writer.Write(toWrite.cari_unvan1);
		writer.Write(toWrite.cari_unvan2);
		writer.Write(toWrite.cari_muh_kod);
		writer.Write(toWrite.cari_muh_kod1);
		writer.Write(toWrite.cari_muh_kod2);
		writer.Write(toWrite.cari_muhartikeli);
		writer.Write(toWrite.cari_vdaire_adi);
		writer.Write(toWrite.cari_vdaire_no);
		writer.Write(toWrite.cari_Ana_cari_kodu);
		writer.Write(toWrite.cari_bolge_kodu);
		writer.Write(toWrite.cari_grup_kodu);
		writer.Write(toWrite.cari_temsilci_kodu);
		writer.Write(toWrite.cari_sektor_kodu);
		writer.Write(toWrite.cari_satis_isk_kod);
		writer.Write(toWrite.cari_special1);
		writer.Write(toWrite.cari_special2);
		writer.Write(toWrite.cari_special3);
		writer.Write(toWrite.cari_sicil_no);
		writer.Write(toWrite.cari_VergiKimlikNo);
		writer.Write(toWrite.cari_banka_hesapno1);
		writer.Write(toWrite.cari_vade_fark_yuz);
		writer.Write(toWrite.cari_vade_fark_yuz1);
		writer.Write(toWrite.cari_vade_fark_yuz2);
		writer.Write(toWrite.cari_tipi);
		writer.Write(toWrite.cari_doviz_cinsi);
		writer.Write(toWrite.cari_doviz_cinsi1);
		writer.Write(toWrite.cari_doviz_cinsi2);
		writer.Write(toWrite.cari_odeme_gunu);
		writer.Write(toWrite.cari_hareket_tipi);
		writer.Write(toWrite.cari_odemeplan_no);
		writer.Write(toWrite.cari_satis_fk);
		writer.Write(toWrite.cari_KurHesapSekli);
		writer.Write(toWrite.cari_odeme_cinsi);
		writer.Write(toWrite.cari_fatura_adres_no);
		writer.Write(toWrite.cari_sevk_adres_no);
		writer.Write(toWrite.cari_VarsayilanGirisDepo);
		writer.Write(toWrite.cari_VarsayilanCikisDepo);
		writer.Write(toWrite.cari_CepTel);
		writer.Write(toWrite.cari_Email);
		writer.Write(toWrite.cari_cari_kilitli_flg);
		writer.Write(toWrite.cari_wwwadresi);
		writer.Write(toWrite.cari_Portal_PW);
		writer.Write(toWrite.cari_Portal_Enabled);
		writer.Write(toWrite.CariAdresleri.Count);
		foreach (CariAdres item in toWrite.CariAdresleri)
		{
			CariAdres.WriteToBinaryWriter(item, writer, 1);
		}
		writer.Write(toWrite.CariYetkilileri.Count);
		foreach (CariYetkili item2 in toWrite.CariYetkilileri)
		{
			CariYetkili.WriteToBinaryWriter(item2, writer);
		}
		if (versiyon >= 2)
		{
			writer.Write((int)toWrite.cari_stok_alim_cinsi);
			writer.Write((int)toWrite.cari_stok_satim_cinsi);
			writer.Write(toWrite.cari_banka_swiftkodu1);
			writer.Write(toWrite.cari_banka_swiftkodu2);
			writer.Write(toWrite.cari_banka_swiftkodu3);
			writer.Write(toWrite.cari_banka_tcmb_kod4);
			writer.Write(toWrite.cari_banka_tcmb_subekod4);
			writer.Write(toWrite.cari_banka_tcmb_ilkod4);
			writer.Write(toWrite.cari_banka_hesapno4);
			writer.Write(toWrite.cari_banka_swiftkodu4);
			writer.Write(toWrite.cari_banka_tcmb_kod5);
			writer.Write(toWrite.cari_banka_tcmb_subekod5);
			writer.Write(toWrite.cari_banka_tcmb_ilkod5);
			writer.Write(toWrite.cari_banka_hesapno5);
			writer.Write(toWrite.cari_banka_swiftkodu5);
			writer.Write(toWrite.cari_banka_tcmb_kod6);
			writer.Write(toWrite.cari_banka_tcmb_subekod6);
			writer.Write(toWrite.cari_banka_tcmb_ilkod6);
			writer.Write(toWrite.cari_banka_hesapno6);
			writer.Write(toWrite.cari_banka_swiftkodu6);
			writer.Write(toWrite.cari_banka_tcmb_kod7);
			writer.Write(toWrite.cari_banka_tcmb_subekod7);
			writer.Write(toWrite.cari_banka_tcmb_ilkod7);
			writer.Write(toWrite.cari_banka_hesapno7);
			writer.Write(toWrite.cari_banka_swiftkodu7);
			writer.Write(toWrite.cari_banka_tcmb_kod8);
			writer.Write(toWrite.cari_banka_tcmb_subekod8);
			writer.Write(toWrite.cari_banka_tcmb_ilkod8);
			writer.Write(toWrite.cari_banka_hesapno8);
			writer.Write(toWrite.cari_banka_swiftkodu8);
			writer.Write(toWrite.cari_banka_tcmb_kod9);
			writer.Write(toWrite.cari_banka_tcmb_subekod9);
			writer.Write(toWrite.cari_banka_tcmb_ilkod9);
			writer.Write(toWrite.cari_banka_hesapno9);
			writer.Write(toWrite.cari_banka_swiftkodu9);
			writer.Write(toWrite.cari_banka_tcmb_kod10);
			writer.Write(toWrite.cari_banka_tcmb_subekod10);
			writer.Write(toWrite.cari_banka_tcmb_ilkod10);
			writer.Write(toWrite.cari_banka_hesapno10);
			writer.Write(toWrite.cari_banka_swiftkodu10);
			writer.Write(toWrite.cari_efatura_fl);
			writer.Write((int)toWrite.cari_odeme_sekli);
			writer.Write(toWrite.cari_TeminatMekAlacakMuhKodu);
			writer.Write(toWrite.cari_TeminatMekAlacakMuhKodu1);
			writer.Write(toWrite.cari_TeminatMekAlacakMuhKodu2);
			writer.Write(toWrite.cari_TeminatMekBorcMuhKodu);
			writer.Write(toWrite.cari_TeminatMekBorcMuhKodu1);
			writer.Write(toWrite.cari_TeminatMekBorcMuhKodu2);
			writer.Write(toWrite.cari_VerilenDepozitoTeminatMuhKodu);
			writer.Write(toWrite.cari_VerilenDepozitoTeminatMuhKodu1);
			writer.Write(toWrite.cari_VerilenDepozitoTeminatMuhKodu2);
			writer.Write(toWrite.cari_AlinanDepozitoTeminatMuhKodu);
			writer.Write(toWrite.cari_AlinanDepozitoTeminatMuhKodu1);
			writer.Write(toWrite.cari_AlinanDepozitoTeminatMuhKodu2);
			writer.Write((int)toWrite.cari_def_efatura_cinsi);
			writer.Write(toWrite.cari_otv_tevkifatina_tabii_fl);
		}
		if (versiyon >= 3)
		{
			writer.Write(toWrite.cari_kod_prefix);
		}
	}

	public static Cari ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Cari result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Cari ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Cari ReadFromBinaryReader(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		Cari cari = new Cari();
		cari.cari_kod = reader.ReadString();
		cari.cari_unvan1 = reader.ReadString();
		cari.cari_unvan2 = reader.ReadString();
		cari.cari_muh_kod = reader.ReadString();
		cari.cari_muh_kod1 = reader.ReadString();
		cari.cari_muh_kod2 = reader.ReadString();
		cari.cari_muhartikeli = reader.ReadString();
		cari.cari_vdaire_adi = reader.ReadString();
		cari.cari_vdaire_no = reader.ReadString();
		cari.cari_Ana_cari_kodu = reader.ReadString();
		cari.cari_bolge_kodu = reader.ReadString();
		cari.cari_grup_kodu = reader.ReadString();
		cari.cari_temsilci_kodu = reader.ReadString();
		cari.cari_sektor_kodu = reader.ReadString();
		cari.cari_satis_isk_kod = reader.ReadString();
		cari.cari_special1 = reader.ReadString();
		cari.cari_special2 = reader.ReadString();
		cari.cari_special3 = reader.ReadString();
		cari.cari_sicil_no = reader.ReadString();
		cari.cari_VergiKimlikNo = reader.ReadString();
		cari.cari_banka_hesapno1 = reader.ReadString();
		cari.cari_vade_fark_yuz = reader.ReadSingle();
		cari.cari_vade_fark_yuz1 = reader.ReadSingle();
		cari.cari_vade_fark_yuz2 = reader.ReadSingle();
		cari.cari_tipi = reader.ReadInt32();
		cari.cari_doviz_cinsi = reader.ReadInt32();
		cari.cari_doviz_cinsi1 = reader.ReadInt32();
		cari.cari_doviz_cinsi2 = reader.ReadInt32();
		cari.cari_odeme_gunu = reader.ReadInt32();
		cari.cari_hareket_tipi = reader.ReadInt32();
		cari.cari_odemeplan_no = reader.ReadInt32();
		cari.cari_satis_fk = reader.ReadInt32();
		cari.cari_KurHesapSekli = reader.ReadInt32();
		cari.cari_odeme_cinsi = reader.ReadInt32();
		cari.cari_fatura_adres_no = reader.ReadInt32();
		cari.cari_sevk_adres_no = reader.ReadInt32();
		cari.cari_VarsayilanGirisDepo = reader.ReadInt32();
		cari.cari_VarsayilanCikisDepo = reader.ReadInt32();
		cari.cari_CepTel = reader.ReadString();
		cari.cari_Email = reader.ReadString();
		cari.cari_cari_kilitli_flg = reader.ReadBoolean();
		cari.cari_wwwadresi = reader.ReadString();
		cari.cari_Portal_PW = reader.ReadString();
		cari.cari_Portal_Enabled = reader.ReadBoolean();
		int num2 = reader.ReadInt32();
		cari.CariAdresleri = new List<CariAdres>();
		for (int i = 0; i < num2; i++)
		{
			cari.CariAdresleri.Add(CariAdres.ReadFromBinaryReader(reader));
		}
		int num3 = reader.ReadInt32();
		cari.CariYetkilileri = new List<CariYetkili>();
		for (int j = 0; j < num3; j++)
		{
			cari.CariYetkilileri.Add(CariYetkili.ReadFromBinaryReader(reader));
		}
		if (num >= 2)
		{
			cari.cari_stok_alim_cinsi = (enum_cari_stok_alim_cinsi)reader.ReadInt32();
			cari.cari_stok_satim_cinsi = (enum_cari_stok_satim_cinsi)reader.ReadInt32();
			cari.cari_banka_swiftkodu1 = reader.ReadString();
			cari.cari_banka_swiftkodu2 = reader.ReadString();
			cari.cari_banka_swiftkodu3 = reader.ReadString();
			cari.cari_banka_tcmb_kod4 = reader.ReadString();
			cari.cari_banka_tcmb_subekod4 = reader.ReadString();
			cari.cari_banka_tcmb_ilkod4 = reader.ReadString();
			cari.cari_banka_hesapno4 = reader.ReadString();
			cari.cari_banka_swiftkodu4 = reader.ReadString();
			cari.cari_banka_tcmb_kod5 = reader.ReadString();
			cari.cari_banka_tcmb_subekod5 = reader.ReadString();
			cari.cari_banka_tcmb_ilkod5 = reader.ReadString();
			cari.cari_banka_hesapno5 = reader.ReadString();
			cari.cari_banka_swiftkodu5 = reader.ReadString();
			cari.cari_banka_tcmb_kod6 = reader.ReadString();
			cari.cari_banka_tcmb_subekod6 = reader.ReadString();
			cari.cari_banka_tcmb_ilkod6 = reader.ReadString();
			cari.cari_banka_hesapno6 = reader.ReadString();
			cari.cari_banka_swiftkodu6 = reader.ReadString();
			cari.cari_banka_tcmb_kod7 = reader.ReadString();
			cari.cari_banka_tcmb_subekod7 = reader.ReadString();
			cari.cari_banka_tcmb_ilkod7 = reader.ReadString();
			cari.cari_banka_hesapno7 = reader.ReadString();
			cari.cari_banka_swiftkodu7 = reader.ReadString();
			cari.cari_banka_tcmb_kod8 = reader.ReadString();
			cari.cari_banka_tcmb_subekod8 = reader.ReadString();
			cari.cari_banka_tcmb_ilkod8 = reader.ReadString();
			cari.cari_banka_hesapno8 = reader.ReadString();
			cari.cari_banka_swiftkodu8 = reader.ReadString();
			cari.cari_banka_tcmb_kod9 = reader.ReadString();
			cari.cari_banka_tcmb_subekod9 = reader.ReadString();
			cari.cari_banka_tcmb_ilkod9 = reader.ReadString();
			cari.cari_banka_hesapno9 = reader.ReadString();
			cari.cari_banka_swiftkodu9 = reader.ReadString();
			cari.cari_banka_tcmb_kod10 = reader.ReadString();
			cari.cari_banka_tcmb_subekod10 = reader.ReadString();
			cari.cari_banka_tcmb_ilkod10 = reader.ReadString();
			cari.cari_banka_hesapno10 = reader.ReadString();
			cari.cari_banka_swiftkodu10 = reader.ReadString();
			cari.cari_efatura_fl = reader.ReadBoolean();
			cari.cari_odeme_sekli = (enum_cari_odeme_sekli)reader.ReadInt32();
			cari.cari_TeminatMekAlacakMuhKodu = reader.ReadString();
			cari.cari_TeminatMekAlacakMuhKodu1 = reader.ReadString();
			cari.cari_TeminatMekAlacakMuhKodu2 = reader.ReadString();
			cari.cari_TeminatMekBorcMuhKodu = reader.ReadString();
			cari.cari_TeminatMekBorcMuhKodu1 = reader.ReadString();
			cari.cari_TeminatMekBorcMuhKodu2 = reader.ReadString();
			cari.cari_VerilenDepozitoTeminatMuhKodu = reader.ReadString();
			cari.cari_VerilenDepozitoTeminatMuhKodu1 = reader.ReadString();
			cari.cari_VerilenDepozitoTeminatMuhKodu2 = reader.ReadString();
			cari.cari_AlinanDepozitoTeminatMuhKodu = reader.ReadString();
			cari.cari_AlinanDepozitoTeminatMuhKodu1 = reader.ReadString();
			cari.cari_AlinanDepozitoTeminatMuhKodu2 = reader.ReadString();
			cari.cari_def_efatura_cinsi = (enum_cari_def_efatura_cinsi)reader.ReadInt32();
			cari.cari_otv_tevkifatina_tabii_fl = reader.ReadBoolean();
		}
		if (num >= 3)
		{
			cari._cari_kod_prefix = reader.ReadString();
		}
		return cari;
	}
}
