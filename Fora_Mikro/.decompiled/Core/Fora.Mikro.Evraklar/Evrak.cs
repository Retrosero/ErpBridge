using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fora.Mikro.Bakim;
using Fora.Mikro.BedenHareketleri;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Depolar;
using Fora.Mikro.DepolarArasiSiparisler;
using Fora.Mikro.DisTicaret;
using Fora.Mikro.Enumler;
using Fora.Mikro.Firmalar;
using Fora.Mikro.Hizmetler;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Masraflar;
using Fora.Mikro.Projeler;
using Fora.Mikro.SayimSonuclari;
using Fora.Mikro.Siparis;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Stoklar.StokSerino;
using Fora.Mikro.Subeler;
using Fora.Mikro.Tahsilatlar;
using Fora.Mikro.Vergiler;

namespace Fora.Mikro.Evraklar;

public class Evrak
{
	private enum_GenelEvrakTipleri _evraktipi;

	private enum_KapamaSekli _kapamasekli;

	private enum_cha_normal_Iade _normaliade;

	private enum_cha_ticaret_turu _ticaretturu;

	private enum_EvrakAktarimDurumu _aktarimdurumu;

	private int _offlinerecno;

	private bool _yenikayit;

	private bool _evrakkilitli;

	private bool _sipariskarsilamami;

	private bool _teklifmi;

	private bool _miktarformulyaz;

	private DateTime _evraktarih;

	private string _evraknoseri;

	private int _evraknosira;

	private string _belgeno;

	private DateTime _belgetarih;

	private Cari _cari;

	private int _odemeplani;

	private FiyatListesi _fiyatlistesi;

	private int _dovizcinsi;

	private Kur _kur;

	private int _alternatifdovizcinsi;

	private Kur _alternatifdovizkuru;

	private Depo _kaynakdepo;

	private Depo _hedefdepo;

	private Depo _nakliyedepo;

	private Proje _proje;

	private SorumlulukMerkezi _sorumlulukmerkezi;

	private string _TemsilciKodu;

	private Firma _firma;

	private Sube _sube;

	private int _mikrouserno;

	private string _aciklama = "";

	private DateTime _sevkteslimtarihi;

	private int _sevkadresno;

	private string _kapamahesapkodu;

	private string _aciklama1;

	private string _aciklama2;

	private string _aciklama3;

	private string _aciklama4;

	private string _aciklama5;

	private string _aciklama6;

	private string _aciklama7;

	private string _aciklama8;

	private string _aciklama9;

	private string _aciklama10;

	private string _sip_teslimturu = "";

	private string _degistirspecialalan1;

	private string _degistirspecialalan2;

	private string _degistirspecialalan3;

	private string _EximKodu;

	private int _DBCno;

	private List<STOK_HAREKETLERI> _StokHareketleri;

	private List<SIPARISLER> _Siparisler;

	private CARI_HESAP_HAREKETLERI _StokCariHesapHareketi;

	private CARI_HESAP_HAREKETLERI _StokCariHesapHareketiIadeliSatis;

	private CARI_HESAP_HAREKETLERI _StokFiyatFarkiCariHesapHareketi;

	private List<CARI_HESAP_HAREKETLERI> _HizmetHareketleri;

	private List<CARI_HESAP_HAREKETLERI> _TahsilatHareketleri;

	private List<CARI_HESAP_HAREKETLERI> _GenelCariHesapHareketleri;

	private List<DEPOLAR_ARASI_SIPARISLER> _DepolarArasiSiparisHareketleri;

	private List<SAYIM_SONUCLARI> _SayimSonuclariHareketleri;

	private List<BAKIM_KABUL_HAREKETLERI> _BakimKabulHareketleri;

	public Firma Firma => _firma;

	public Sube Sube => _sube;

	public string EvrakNoSeri => _evraknoseri;

	public int EvrakNoSira => _evraknosira;

	public string BelgeNo => _belgeno;

	public FiyatListesi FiyatListesi => _fiyatlistesi;

	public Depo KaynakDepo => _kaynakdepo;

	public Depo HedefDepo => _hedefdepo;

	public Depo NakliyeDepo => _nakliyedepo;

	public SorumlulukMerkezi sorumlulukmerkezi => _sorumlulukmerkezi;

	public Proje proje => _proje;

	public string aciklama1 => _aciklama1;

	public string aciklama2 => _aciklama2;

	public string aciklama3 => _aciklama3;

	public string aciklama4 => _aciklama4;

	public string aciklama5 => _aciklama5;

	public string aciklama6 => _aciklama6;

	public string aciklama7 => _aciklama7;

	public string aciklama8 => _aciklama8;

	public string aciklama9 => _aciklama9;

	public string aciklama10 => _aciklama10;

	public int offlinerecno => _offlinerecno;

	public bool yenikayit => _yenikayit;

	public bool evrakkilitli => _evrakkilitli;

	public bool sipariskarsilamami => _sipariskarsilamami;

	public int odemeplani => _odemeplani;

	public int dovizcinsi => _dovizcinsi;

	public int alternatifdovizcinsi => _alternatifdovizcinsi;

	public string TemsilciKodu => _TemsilciKodu;

	public int mikrouserno => _mikrouserno;

	public string aciklama => _aciklama;

	public int sevkadresno => _sevkadresno;

	public string kapamahesapkodu => _kapamahesapkodu;

	public string sip_teslimturu => _sip_teslimturu;

	public string degistirspecialalan1 => _degistirspecialalan1;

	public string degistirspecialalan2 => _degistirspecialalan2;

	public string degistirspecialalan3 => _degistirspecialalan3;

	public string EximKodu => _EximKodu;

	public int DBCno => _DBCno;

	public Cari cari
	{
		get
		{
			return _cari;
		}
		set
		{
			_cari = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public Kur kur
	{
		get
		{
			return _kur;
		}
		set
		{
			_kur = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public Kur alternatifdovizkuru
	{
		get
		{
			return _alternatifdovizkuru;
		}
		set
		{
			_alternatifdovizkuru = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public enum_GenelEvrakTipleri evraktipi
	{
		get
		{
			return _evraktipi;
		}
		set
		{
			_evraktipi = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public enum_KapamaSekli kapamasekli
	{
		get
		{
			return _kapamasekli;
		}
		set
		{
			_kapamasekli = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public enum_cha_normal_Iade normaliade
	{
		get
		{
			return _normaliade;
		}
		set
		{
			_normaliade = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public enum_cha_ticaret_turu ticaretturu
	{
		get
		{
			return _ticaretturu;
		}
		set
		{
			_ticaretturu = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public bool teklifmi
	{
		get
		{
			return _teklifmi;
		}
		set
		{
			_teklifmi = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public bool miktarformulyaz
	{
		get
		{
			return _miktarformulyaz;
		}
		set
		{
			_miktarformulyaz = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public enum_EvrakAktarimDurumu aktarimdurumu
	{
		get
		{
			return _aktarimdurumu;
		}
		set
		{
			_aktarimdurumu = value;
			OnEvrakChanged(EventArgs.Empty);
		}
	}

	public List<CEKI_LISTESI> CekiListesi { get; set; }

	public EvrakParametreler Parametreler { get; set; }

	public enum_SatisAlis GetSatisAlis => evraktipi switch
	{
		enum_GenelEvrakTipleri.AlisFaturasi => enum_SatisAlis.Alis, 
		enum_GenelEvrakTipleri.SatisFaturasi => enum_SatisAlis.Satis, 
		enum_GenelEvrakTipleri.AlinanSiparis => enum_SatisAlis.Satis, 
		enum_GenelEvrakTipleri.ProformaSiparis => enum_SatisAlis.Satis, 
		enum_GenelEvrakTipleri.AlisIrsaliyesi => enum_SatisAlis.Alis, 
		enum_GenelEvrakTipleri.DepolarArasiSevk => enum_SatisAlis.Diger, 
		enum_GenelEvrakTipleri.DepolarArasiSiparis => enum_SatisAlis.Diger, 
		enum_GenelEvrakTipleri.KonsinyedenIadeIrsaliyesi => enum_SatisAlis.Diger, 
		enum_GenelEvrakTipleri.KonsinyeIrsaliyesi => enum_SatisAlis.Diger, 
		enum_GenelEvrakTipleri.SatisIrsaliyesi => enum_SatisAlis.Satis, 
		enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => enum_SatisAlis.Diger, 
		enum_GenelEvrakTipleri.Tahsilat => enum_SatisAlis.Diger, 
		enum_GenelEvrakTipleri.Masraf => enum_SatisAlis.Diger, 
		enum_GenelEvrakTipleri.Tediye => enum_SatisAlis.Diger, 
		_ => enum_SatisAlis.Diger, 
	};

	public DateTime EvrakTarihi => _evraktarih;

	public DateTime SevkTeslimTarihi => _sevkteslimtarihi;

	public DateTime BelgeTarihi => _belgetarih;

	public event EventHandler EvrakChanged;

	protected virtual void OnEvrakChanged(EventArgs e)
	{
		this.EvrakChanged?.Invoke(this, e);
	}

	public Evrak()
	{
		_evraktipi = enum_GenelEvrakTipleri.AlinanSiparis;
		_yenikayit = true;
		_evrakkilitli = false;
		_kapamasekli = enum_KapamaSekli.AcikHesap;
		_normaliade = enum_cha_normal_Iade.Normal;
		_ticaretturu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
		InitDefaults();
	}

	public Evrak(enum_GenelEvrakTipleri EvrakTip, bool YeniKayitMi, bool EvrakKilitliMi, enum_KapamaSekli KapamaSekli, enum_cha_normal_Iade NormalIade, enum_cha_ticaret_turu TicaretTuru)
	{
		_evraktipi = EvrakTip;
		_yenikayit = YeniKayitMi;
		_evrakkilitli = EvrakKilitliMi;
		_kapamasekli = KapamaSekli;
		_normaliade = NormalIade;
		_ticaretturu = TicaretTuru;
		InitDefaults();
	}

	private void InitDefaults()
	{
		_aktarimdurumu = enum_EvrakAktarimDurumu.Aktarilacak;
		_offlinerecno = 0;
		_sipariskarsilamami = false;
		_cari = new Cari();
		_odemeplani = 0;
		_evraknoseri = "";
		_evraknosira = 0;
		_belgeno = "";
		_firma = new Firma();
		_sube = new Sube();
		_sevkadresno = 1;
		_mikrouserno = 1;
		_fiyatlistesi = new FiyatListesi();
		_dovizcinsi = 0;
		_alternatifdovizcinsi = 1;
		_kur = new Kur();
		_alternatifdovizkuru = new Kur();
		_proje = new Proje();
		_sorumlulukmerkezi = new SorumlulukMerkezi();
		_kaynakdepo = new Depo();
		_hedefdepo = new Depo();
		_nakliyedepo = new Depo();
		_evraktarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		_belgetarih = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		_sevkteslimtarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		_aciklama = "";
		_kapamahesapkodu = "";
		_aciklama1 = "";
		_aciklama2 = "";
		_aciklama3 = "";
		_aciklama4 = "";
		_aciklama5 = "";
		_aciklama6 = "";
		_aciklama7 = "";
		_aciklama8 = "";
		_aciklama9 = "";
		_aciklama10 = "";
		_sip_teslimturu = "";
		_degistirspecialalan1 = "";
		_degistirspecialalan2 = "";
		_degistirspecialalan3 = "";
		_TemsilciKodu = "";
		_EximKodu = "";
		_StokHareketleri = new List<STOK_HAREKETLERI>();
		_Siparisler = new List<SIPARISLER>();
		_StokCariHesapHareketi = new CARI_HESAP_HAREKETLERI();
		_StokCariHesapHareketiIadeliSatis = new CARI_HESAP_HAREKETLERI();
		_StokFiyatFarkiCariHesapHareketi = new CARI_HESAP_HAREKETLERI();
		_HizmetHareketleri = new List<CARI_HESAP_HAREKETLERI>();
		_TahsilatHareketleri = new List<CARI_HESAP_HAREKETLERI>();
		_GenelCariHesapHareketleri = new List<CARI_HESAP_HAREKETLERI>();
		_DepolarArasiSiparisHareketleri = new List<DEPOLAR_ARASI_SIPARISLER>();
		_SayimSonuclariHareketleri = new List<SAYIM_SONUCLARI>();
		_BakimKabulHareketleri = new List<BAKIM_KABUL_HAREKETLERI>();
		CekiListesi = new List<CEKI_LISTESI>();
		Parametreler = new EvrakParametreler(this);
	}

	public string GetDegistirSpecial1()
	{
		return _degistirspecialalan1;
	}

	public string GetDegistirSpecial2()
	{
		return _degistirspecialalan2;
	}

	public string GetDegistirSpecial3()
	{
		return _degistirspecialalan3;
	}

	public enum_toptan_perakende GetToptanPerakende()
	{
		enum_toptan_perakende result = enum_toptan_perakende.Toptan;
		switch (ticaretturu)
		{
		case enum_cha_ticaret_turu.ToptanYurtIciTicaret:
			result = enum_toptan_perakende.Toptan;
			break;
		case enum_cha_ticaret_turu.PerakendeYurtIciTicaret:
			result = enum_toptan_perakende.Perakende;
			break;
		}
		return result;
	}

	public StokEklemeBilgileri GetStokEklemeBilgileri(int Index)
	{
		StokEklemeBilgileri stokEklemeBilgileri = new StokEklemeBilgileri();
		stokEklemeBilgileri.Miktar = GetMiktar(Index);
		stokEklemeBilgileri.sth_birim_pntr = GetBirimPntr(Index);
		stokEklemeBilgileri.BirimFiyat.FiyatKaynagi = enum_Fiyat_Kaynagi.Tanimsiz;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			stokEklemeBilgileri.renk_beden_hareketleri = _Siparisler[Index].renk_beden_hareketleri;
			stokEklemeBilgileri.parti_kodu = _Siparisler[Index].sip_parti_kodu;
			stokEklemeBilgileri.lot_no = _Siparisler[Index].sip_lot_no;
			stokEklemeBilgileri.Aciklama1 = _Siparisler[Index].sip_aciklama;
			stokEklemeBilgileri.Aciklama2 = _Siparisler[Index].sip_aciklama2;
			stokEklemeBilgileri.proje_kodu = _Siparisler[Index].sip_projekodu;
			stokEklemeBilgileri.sorumluluk_merkezi_kodu = _Siparisler[Index].sip_stok_sormerk;
			stokEklemeBilgileri.sorumluluk_merkezi_kodu = _Siparisler[Index].sip_cari_sormerk;
			stokEklemeBilgileri.StokDovizCinsiKuru = _Siparisler[Index].sip_doviz_kuru;
			stokEklemeBilgileri.vergi_pntr = _Siparisler[Index].sip_vergi_pntr;
			stokEklemeBilgileri.BirimFiyat.BeginInit();
			stokEklemeBilgileri.BirimFiyat.FiyatBrut = _Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar;
			stokEklemeBilgileri.BirimFiyat.DovizCinsi = _Siparisler[Index].sip_doviz_cinsi;
			stokEklemeBilgileri.BirimFiyat.Kur.dov_fiyat = _Siparisler[Index].sip_doviz_kuru;
			stokEklemeBilgileri.BirimFiyat.Kur.dov_no = _Siparisler[Index].sip_doviz_cinsi;
			stokEklemeBilgileri.BirimFiyat.Iskonto_1_UygulamaSekli = _Siparisler[Index].sip_iskonto1;
			stokEklemeBilgileri.BirimFiyat.Iskonto_2_UygulamaSekli = _Siparisler[Index].sip_iskonto2;
			stokEklemeBilgileri.BirimFiyat.Iskonto_3_UygulamaSekli = _Siparisler[Index].sip_iskonto3;
			stokEklemeBilgileri.BirimFiyat.Iskonto_4_UygulamaSekli = _Siparisler[Index].sip_iskonto4;
			stokEklemeBilgileri.BirimFiyat.Iskonto_5_UygulamaSekli = _Siparisler[Index].sip_iskonto5;
			stokEklemeBilgileri.BirimFiyat.Iskonto_6_UygulamaSekli = _Siparisler[Index].sip_iskonto6;
			stokEklemeBilgileri.BirimFiyat.Masraf_1_UygulamaSekli = _Siparisler[Index].sip_masraf1;
			stokEklemeBilgileri.BirimFiyat.Masraf_2_UygulamaSekli = _Siparisler[Index].sip_masraf2;
			stokEklemeBilgileri.BirimFiyat.Masraf_3_UygulamaSekli = _Siparisler[Index].sip_masraf3;
			stokEklemeBilgileri.BirimFiyat.Masraf_4_UygulamaSekli = _Siparisler[Index].sip_masraf4;
			stokEklemeBilgileri.Miktar_Formul_Olcu1 = _Siparisler[Index].sip_Olcu1;
			stokEklemeBilgileri.Miktar_Formul_Olcu2 = _Siparisler[Index].sip_Olcu2;
			stokEklemeBilgileri.Miktar_Formul_Olcu3 = _Siparisler[Index].sip_Olcu3;
			stokEklemeBilgileri.Miktar_Formul_Olcu4 = _Siparisler[Index].sip_Olcu4;
			stokEklemeBilgileri.Miktar_Formul_Olcu5 = _Siparisler[Index].sip_Olcu5;
			stokEklemeBilgileri.Miktar_Formul_FormulMiktar = _Siparisler[Index].sip_FormulMiktar;
			stokEklemeBilgileri.Miktar_Formul_FormulMiktarNo = _Siparisler[Index].sip_FormulMiktarNo;
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_1_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_2_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_3_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_4_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2 - _Siparisler[Index].sip_iskonto_3) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_5_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2 - _Siparisler[Index].sip_iskonto_3 - _Siparisler[Index].sip_iskonto_4) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_6_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2 - _Siparisler[Index].sip_iskonto_3 - _Siparisler[Index].sip_iskonto_4 - _Siparisler[Index].sip_iskonto_5) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6 / _Siparisler[Index].sip_miktar;
				break;
			}
			stokEklemeBilgileri.BirimFiyat.EndInit();
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			stokEklemeBilgileri.renk_beden_hareketleri = _Siparisler[Index].renk_beden_hareketleri;
			stokEklemeBilgileri.parti_kodu = _Siparisler[Index].sip_parti_kodu;
			stokEklemeBilgileri.lot_no = _Siparisler[Index].sip_lot_no;
			stokEklemeBilgileri.Aciklama1 = _Siparisler[Index].sip_aciklama;
			stokEklemeBilgileri.Aciklama2 = _Siparisler[Index].sip_aciklama2;
			stokEklemeBilgileri.proje_kodu = _Siparisler[Index].sip_projekodu;
			stokEklemeBilgileri.sorumluluk_merkezi_kodu = _Siparisler[Index].sip_stok_sormerk;
			stokEklemeBilgileri.sorumluluk_merkezi_kodu = _Siparisler[Index].sip_cari_sormerk;
			stokEklemeBilgileri.StokDovizCinsiKuru = _Siparisler[Index].sip_doviz_kuru;
			stokEklemeBilgileri.vergi_pntr = _Siparisler[Index].sip_vergi_pntr;
			stokEklemeBilgileri.BirimFiyat.BeginInit();
			stokEklemeBilgileri.BirimFiyat.FiyatBrut = _Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar;
			stokEklemeBilgileri.BirimFiyat.DovizCinsi = _Siparisler[Index].sip_doviz_cinsi;
			stokEklemeBilgileri.BirimFiyat.Kur.dov_fiyat = _Siparisler[Index].sip_doviz_kuru;
			stokEklemeBilgileri.BirimFiyat.Kur.dov_no = _Siparisler[Index].sip_doviz_cinsi;
			stokEklemeBilgileri.BirimFiyat.Iskonto_1_UygulamaSekli = _Siparisler[Index].sip_iskonto1;
			stokEklemeBilgileri.BirimFiyat.Iskonto_2_UygulamaSekli = _Siparisler[Index].sip_iskonto2;
			stokEklemeBilgileri.BirimFiyat.Iskonto_3_UygulamaSekli = _Siparisler[Index].sip_iskonto3;
			stokEklemeBilgileri.BirimFiyat.Iskonto_4_UygulamaSekli = _Siparisler[Index].sip_iskonto4;
			stokEklemeBilgileri.BirimFiyat.Iskonto_5_UygulamaSekli = _Siparisler[Index].sip_iskonto5;
			stokEklemeBilgileri.BirimFiyat.Iskonto_6_UygulamaSekli = _Siparisler[Index].sip_iskonto6;
			stokEklemeBilgileri.BirimFiyat.Masraf_1_UygulamaSekli = _Siparisler[Index].sip_masraf1;
			stokEklemeBilgileri.BirimFiyat.Masraf_2_UygulamaSekli = _Siparisler[Index].sip_masraf2;
			stokEklemeBilgileri.BirimFiyat.Masraf_3_UygulamaSekli = _Siparisler[Index].sip_masraf3;
			stokEklemeBilgileri.BirimFiyat.Masraf_4_UygulamaSekli = _Siparisler[Index].sip_masraf4;
			stokEklemeBilgileri.Miktar_Formul_Olcu1 = _Siparisler[Index].sip_Olcu1;
			stokEklemeBilgileri.Miktar_Formul_Olcu2 = _Siparisler[Index].sip_Olcu2;
			stokEklemeBilgileri.Miktar_Formul_Olcu3 = _Siparisler[Index].sip_Olcu3;
			stokEklemeBilgileri.Miktar_Formul_Olcu4 = _Siparisler[Index].sip_Olcu4;
			stokEklemeBilgileri.Miktar_Formul_Olcu5 = _Siparisler[Index].sip_Olcu5;
			stokEklemeBilgileri.Miktar_Formul_FormulMiktar = _Siparisler[Index].sip_FormulMiktar;
			stokEklemeBilgileri.Miktar_Formul_FormulMiktarNo = _Siparisler[Index].sip_FormulMiktarNo;
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_1_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_1 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_2_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_2 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_3_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_3 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_4_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2 - _Siparisler[Index].sip_iskonto_3) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_4 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_5_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2 - _Siparisler[Index].sip_iskonto_3 - _Siparisler[Index].sip_iskonto_4) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_5 / _Siparisler[Index].sip_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_6_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6 / _Siparisler[Index].sip_miktar / (_Siparisler[Index].sip_tutar / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6 / _Siparisler[Index].sip_miktar / ((_Siparisler[Index].sip_tutar - _Siparisler[Index].sip_iskonto_1 - _Siparisler[Index].sip_iskonto_2 - _Siparisler[Index].sip_iskonto_3 - _Siparisler[Index].sip_iskonto_4 - _Siparisler[Index].sip_iskonto_5) / _Siparisler[Index].sip_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _Siparisler[Index].sip_iskonto_6 / _Siparisler[Index].sip_miktar;
				break;
			}
			stokEklemeBilgileri.BirimFiyat.EndInit();
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			stokEklemeBilgileri.renk_beden_hareketleri = _DepolarArasiSiparisHareketleri[Index].renk_beden_hareketleri;
			stokEklemeBilgileri.Aciklama1 = _DepolarArasiSiparisHareketleri[Index].ssip_aciklama;
			stokEklemeBilgileri.proje_kodu = _Siparisler[Index].sip_projekodu;
			stokEklemeBilgileri.sorumluluk_merkezi_kodu = _DepolarArasiSiparisHareketleri[Index].ssip_sormerkezi;
			stokEklemeBilgileri.BirimFiyat.BeginInit();
			stokEklemeBilgileri.BirimFiyat.FiyatBrut = _DepolarArasiSiparisHareketleri[Index].ssip_tutar / _DepolarArasiSiparisHareketleri[Index].ssip_miktar;
			stokEklemeBilgileri.BirimFiyat.EndInit();
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			stokEklemeBilgileri.renk_beden_hareketleri = new List<BEDEN_HAREKETLERI>();
			if (_SayimSonuclariHareketleri[Index].sym_bedenno != 0 || _SayimSonuclariHareketleri[Index].sym_renkno != 0)
			{
				BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
				bEDEN_HAREKETLERI.BdnHar_BedenNo = _SayimSonuclariHareketleri[Index].sym_bedenno;
				stokEklemeBilgileri.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI);
			}
			stokEklemeBilgileri.parti_kodu = _SayimSonuclariHareketleri[Index].sym_parti_kodu;
			stokEklemeBilgileri.lot_no = _SayimSonuclariHareketleri[Index].sym_lot_no;
			if (_SayimSonuclariHareketleri[Index].sym_serino != "")
			{
				stokEklemeBilgileri.serino_tanimlari = new List<STOK_SERINO_TANIMLARI>();
				STOK_SERINO_TANIMLARI sTOK_SERINO_TANIMLARI = new STOK_SERINO_TANIMLARI();
				sTOK_SERINO_TANIMLARI.chz_serino = _SayimSonuclariHareketleri[Index].sym_serino;
				stokEklemeBilgileri.serino_tanimlari.Add(sTOK_SERINO_TANIMLARI);
			}
			break;
		default:
			stokEklemeBilgileri.renk_beden_hareketleri = _StokHareketleri[Index].renk_beden_hareketleri;
			stokEklemeBilgileri.parti_kodu = _StokHareketleri[Index].sth_parti_kodu;
			stokEklemeBilgileri.lot_no = _StokHareketleri[Index].sth_lot_no;
			stokEklemeBilgileri.serino_tanimlari = _StokHareketleri[Index].stok_serinolari;
			stokEklemeBilgileri.Aciklama1 = _StokHareketleri[Index].sth_aciklama;
			stokEklemeBilgileri.Miktar2 = _StokHareketleri[Index].sth_miktar2;
			stokEklemeBilgileri.proje_kodu = _StokHareketleri[Index].sth_proje_kodu;
			stokEklemeBilgileri.sorumluluk_merkezi_kodu = _StokHareketleri[Index].sth_stok_srm_merkezi;
			stokEklemeBilgileri.sorumluluk_merkezi_kodu = _StokHareketleri[Index].sth_cari_srm_merkezi;
			stokEklemeBilgileri.StokDovizCinsiKuru = _StokHareketleri[Index].sth_har_doviz_kuru;
			stokEklemeBilgileri.vergi_pntr = _StokHareketleri[Index].sth_vergi_pntr;
			stokEklemeBilgileri.BirimFiyat.BeginInit();
			stokEklemeBilgileri.BirimFiyat.FiyatBrut = _StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar;
			stokEklemeBilgileri.BirimFiyat.DovizCinsi = _StokHareketleri[Index].sth_stok_doviz_cinsi;
			stokEklemeBilgileri.BirimFiyat.Kur.dov_fiyat = _StokHareketleri[Index].sth_stok_doviz_kuru;
			stokEklemeBilgileri.BirimFiyat.Kur.dov_no = _StokHareketleri[Index].sth_stok_doviz_cinsi;
			stokEklemeBilgileri.BirimFiyat.Iskonto_1_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas1;
			stokEklemeBilgileri.BirimFiyat.Iskonto_2_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas2;
			stokEklemeBilgileri.BirimFiyat.Iskonto_3_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas3;
			stokEklemeBilgileri.BirimFiyat.Iskonto_4_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas4;
			stokEklemeBilgileri.BirimFiyat.Iskonto_5_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas5;
			stokEklemeBilgileri.BirimFiyat.Iskonto_6_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas6;
			stokEklemeBilgileri.BirimFiyat.Masraf_1_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas7;
			stokEklemeBilgileri.BirimFiyat.Masraf_2_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas8;
			stokEklemeBilgileri.BirimFiyat.Masraf_3_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas9;
			stokEklemeBilgileri.BirimFiyat.Masraf_4_UygulamaSekli = _StokHareketleri[Index].sth_isk_mas10;
			stokEklemeBilgileri.Miktar_Formul_Olcu1 = _StokHareketleri[Index].sth_Olcu1;
			stokEklemeBilgileri.Miktar_Formul_Olcu2 = _StokHareketleri[Index].sth_Olcu2;
			stokEklemeBilgileri.Miktar_Formul_Olcu3 = _StokHareketleri[Index].sth_Olcu3;
			stokEklemeBilgileri.Miktar_Formul_Olcu4 = _StokHareketleri[Index].sth_Olcu4;
			stokEklemeBilgileri.Miktar_Formul_Olcu5 = _StokHareketleri[Index].sth_Olcu5;
			stokEklemeBilgileri.Miktar_Formul_FormulMiktar = _StokHareketleri[Index].sth_FormulMiktar;
			stokEklemeBilgileri.Miktar_Formul_FormulMiktarNo = _StokHareketleri[Index].sth_FormulMiktarNo;
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_1_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto1 / _StokHareketleri[Index].sth_miktar / (_StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto1 / _StokHareketleri[Index].sth_miktar / (_StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto1;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_1_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto1 / _StokHareketleri[Index].sth_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_2_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto2 / _StokHareketleri[Index].sth_miktar / (_StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto2 / _StokHareketleri[Index].sth_miktar / ((_StokHareketleri[Index].sth_tutar - _StokHareketleri[Index].sth_iskonto1) / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto2;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_2_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto2 / _StokHareketleri[Index].sth_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_3_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto3 / _StokHareketleri[Index].sth_miktar / (_StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto3 / _StokHareketleri[Index].sth_miktar / ((_StokHareketleri[Index].sth_tutar - _StokHareketleri[Index].sth_iskonto1 - _StokHareketleri[Index].sth_iskonto2) / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto3;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_3_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto3 / _StokHareketleri[Index].sth_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_4_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto4 / _StokHareketleri[Index].sth_miktar / (_StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto4 / _StokHareketleri[Index].sth_miktar / ((_StokHareketleri[Index].sth_tutar - _StokHareketleri[Index].sth_iskonto1 - _StokHareketleri[Index].sth_iskonto2 - _StokHareketleri[Index].sth_iskonto3) / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto4;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_4_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto4 / _StokHareketleri[Index].sth_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_5_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto5 / _StokHareketleri[Index].sth_miktar / (_StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto5 / _StokHareketleri[Index].sth_miktar / ((_StokHareketleri[Index].sth_tutar - _StokHareketleri[Index].sth_iskonto1 - _StokHareketleri[Index].sth_iskonto2 - _StokHareketleri[Index].sth_iskonto3 - _StokHareketleri[Index].sth_iskonto4) / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto5;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_5_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto5 / _StokHareketleri[Index].sth_miktar;
				break;
			}
			switch (stokEklemeBilgileri.BirimFiyat.Iskonto_6_UygulamaSekli)
			{
			case 0:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto6 / _StokHareketleri[Index].sth_miktar / (_StokHareketleri[Index].sth_tutar / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 1:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto6 / _StokHareketleri[Index].sth_miktar / ((_StokHareketleri[Index].sth_tutar - _StokHareketleri[Index].sth_iskonto1 - _StokHareketleri[Index].sth_iskonto2 - _StokHareketleri[Index].sth_iskonto3 - _StokHareketleri[Index].sth_iskonto4 - _StokHareketleri[Index].sth_iskonto5) / _StokHareketleri[Index].sth_miktar) * 100.0;
				break;
			case 2:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto6;
				break;
			case 3:
				stokEklemeBilgileri.BirimFiyat.Iskonto_6_YuzdeVeyaMiktar = _StokHareketleri[Index].sth_iskonto6 / _StokHareketleri[Index].sth_miktar;
				break;
			}
			stokEklemeBilgileri.BirimFiyat.EndInit();
			break;
		}
		return stokEklemeBilgileri;
	}

	public double GetMiktar(int Index)
	{
		double num = 0.0;
		return evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => _Siparisler[Index].sip_miktar, 
			enum_GenelEvrakTipleri.ProformaSiparis => _Siparisler[Index].sip_miktar, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => _DepolarArasiSiparisHareketleri[Index].ssip_miktar, 
			enum_GenelEvrakTipleri.Tahsilat => _TahsilatHareketleri[Index].cha_meblag, 
			enum_GenelEvrakTipleri.Tediye => _TahsilatHareketleri[Index].cha_meblag, 
			enum_GenelEvrakTipleri.Masraf => _TahsilatHareketleri[Index].cha_meblag, 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => _SayimSonuclariHareketleri[Index].sym_miktar1, 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama => _StokHareketleri[Index].sth_miktar, 
			enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama => _StokHareketleri[Index].sth_miktar, 
			_ => _StokHareketleri[Index].sth_miktar, 
		};
	}

	public string GetStokKodu(int Index)
	{
		string text = "";
		return evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => _Siparisler[Index].sip_stok_kod, 
			enum_GenelEvrakTipleri.ProformaSiparis => _Siparisler[Index].sip_stok_kod, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => _DepolarArasiSiparisHareketleri[Index].ssip_stok_kod, 
			enum_GenelEvrakTipleri.Tahsilat => "", 
			enum_GenelEvrakTipleri.Tediye => "", 
			enum_GenelEvrakTipleri.Masraf => "", 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => _SayimSonuclariHareketleri[Index].sym_Stokkodu, 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama => _StokHareketleri[Index].sth_stok_kod, 
			enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama => _StokHareketleri[Index].sth_stok_kod, 
			_ => _StokHareketleri[Index].sth_stok_kod, 
		};
	}

	public int GetBirimPntr(int Index)
	{
		int num = 0;
		return evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => _Siparisler[Index].sip_birim_pntr, 
			enum_GenelEvrakTipleri.ProformaSiparis => _Siparisler[Index].sip_birim_pntr, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => _DepolarArasiSiparisHareketleri[Index].ssip_birim_pntr, 
			enum_GenelEvrakTipleri.Tahsilat => 1, 
			enum_GenelEvrakTipleri.Tediye => 1, 
			enum_GenelEvrakTipleri.Masraf => 1, 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => _SayimSonuclariHareketleri[Index].sym_birim_pntr, 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama => _StokHareketleri[Index].sth_birim_pntr, 
			enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama => _StokHareketleri[Index].sth_birim_pntr, 
			_ => _StokHareketleri[Index].sth_birim_pntr, 
		};
	}

	public double GetSiparisKalanMiktar(int Index)
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return _DepolarArasiSiparisHareketleri[Index].ssip_miktar - _DepolarArasiSiparisHareketleri[Index].ssip_teslim_miktar;
		}
		return _Siparisler[Index].sip_miktar - _Siparisler[Index].sip_teslim_miktar;
	}

	public int Getchagrupno()
	{
		if ((evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi) && _kapamasekli == enum_KapamaSekli.BankadanKapanacak)
		{
			return 1;
		}
		if (cari.cari_doviz_cinsi == dovizcinsi)
		{
			return 0;
		}
		if (cari.cari_doviz_cinsi1 == dovizcinsi)
		{
			return 1;
		}
		if (cari.cari_doviz_cinsi2 == dovizcinsi)
		{
			return 2;
		}
		return 0;
	}

	public string GetEvrakTipiString()
	{
		return EnumUtility.EnumToLocalizedString(evraktipi);
	}

	public double GetSepetStokMiktari(string sto_kod)
	{
		double num = 0.0;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				if (item.sip_stok_kod == sto_kod)
				{
					num += item.sip_miktar;
				}
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				if (item2.sip_stok_kod == sto_kod)
				{
					num += item2.sip_miktar;
				}
			}
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			foreach (DEPOLAR_ARASI_SIPARISLER item3 in _DepolarArasiSiparisHareketleri)
			{
				if (item3.ssip_stok_kod == sto_kod)
				{
					num += item3.ssip_miktar;
				}
			}
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			foreach (SAYIM_SONUCLARI item4 in _SayimSonuclariHareketleri)
			{
				if (item4.sym_Stokkodu == sto_kod)
				{
					num += item4.sym_miktar1;
				}
			}
			break;
		default:
			foreach (STOK_HAREKETLERI item5 in _StokHareketleri)
			{
				if (item5.sth_stok_kod == sto_kod)
				{
					num += item5.sth_miktar;
				}
			}
			break;
		}
		return num;
	}

	public List<BEDEN_HAREKETLERI> GetSepetStokMiktariRenkBeden(string sto_kod)
	{
		List<BEDEN_HAREKETLERI> list = new List<BEDEN_HAREKETLERI>();
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				if (!(item.sip_stok_kod == sto_kod))
				{
					continue;
				}
				foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
				{
					bool flag5 = false;
					foreach (BEDEN_HAREKETLERI item3 in list)
					{
						if (item2.BdnHar_BedenNo == item3.BdnHar_BedenNo)
						{
							flag5 = true;
							item3.BdnHar_HarGor += item2.BdnHar_HarGor;
							break;
						}
					}
					if (!flag5)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI5 = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI5.BdnHar_BedenNo = item2.BdnHar_BedenNo;
						bEDEN_HAREKETLERI5.BdnHar_HarGor = item2.BdnHar_HarGor;
						list.Add(bEDEN_HAREKETLERI5);
					}
				}
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item4 in _Siparisler)
			{
				if (!(item4.sip_stok_kod == sto_kod))
				{
					continue;
				}
				foreach (BEDEN_HAREKETLERI item5 in item4.renk_beden_hareketleri)
				{
					bool flag4 = false;
					foreach (BEDEN_HAREKETLERI item6 in list)
					{
						if (item5.BdnHar_BedenNo == item6.BdnHar_BedenNo)
						{
							flag4 = true;
							item6.BdnHar_HarGor += item5.BdnHar_HarGor;
							break;
						}
					}
					if (!flag4)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI4 = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI4.BdnHar_BedenNo = item5.BdnHar_BedenNo;
						bEDEN_HAREKETLERI4.BdnHar_HarGor = item5.BdnHar_HarGor;
						list.Add(bEDEN_HAREKETLERI4);
					}
				}
			}
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			foreach (DEPOLAR_ARASI_SIPARISLER item7 in _DepolarArasiSiparisHareketleri)
			{
				if (!(item7.ssip_stok_kod == sto_kod))
				{
					continue;
				}
				foreach (BEDEN_HAREKETLERI item8 in item7.renk_beden_hareketleri)
				{
					bool flag3 = false;
					foreach (BEDEN_HAREKETLERI item9 in list)
					{
						if (item8.BdnHar_BedenNo == item9.BdnHar_BedenNo)
						{
							flag3 = true;
							item9.BdnHar_HarGor += item8.BdnHar_HarGor;
							break;
						}
					}
					if (!flag3)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI3 = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI3.BdnHar_BedenNo = item8.BdnHar_BedenNo;
						bEDEN_HAREKETLERI3.BdnHar_HarGor = item8.BdnHar_HarGor;
						list.Add(bEDEN_HAREKETLERI3);
					}
				}
			}
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			foreach (SAYIM_SONUCLARI item10 in _SayimSonuclariHareketleri)
			{
				if (!(item10.sym_Stokkodu == sto_kod))
				{
					continue;
				}
				bool flag2 = false;
				foreach (BEDEN_HAREKETLERI item11 in list)
				{
					if (item10.sym_bedenno == item11.BdnHar_BedenNo)
					{
						flag2 = true;
						item11.BdnHar_HarGor += item10.sym_miktar1;
						break;
					}
				}
				if (!flag2)
				{
					BEDEN_HAREKETLERI bEDEN_HAREKETLERI2 = new BEDEN_HAREKETLERI();
					bEDEN_HAREKETLERI2.BdnHar_BedenNo = item10.sym_bedenno;
					bEDEN_HAREKETLERI2.BdnHar_HarGor = item10.sym_miktar1;
					list.Add(bEDEN_HAREKETLERI2);
				}
			}
			break;
		default:
			foreach (STOK_HAREKETLERI item12 in _StokHareketleri)
			{
				if (!(item12.sth_stok_kod == sto_kod))
				{
					continue;
				}
				foreach (BEDEN_HAREKETLERI item13 in item12.renk_beden_hareketleri)
				{
					bool flag = false;
					foreach (BEDEN_HAREKETLERI item14 in list)
					{
						if (item13.BdnHar_BedenNo == item14.BdnHar_BedenNo)
						{
							flag = true;
							item14.BdnHar_HarGor += item13.BdnHar_HarGor;
							break;
						}
					}
					if (!flag)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI.BdnHar_BedenNo = item13.BdnHar_BedenNo;
						bEDEN_HAREKETLERI.BdnHar_HarGor = item13.BdnHar_HarGor;
						list.Add(bEDEN_HAREKETLERI);
					}
				}
			}
			break;
		}
		return list;
	}

	public int GetKalemSayisi()
	{
		return evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => _Siparisler.Count, 
			enum_GenelEvrakTipleri.ProformaSiparis => _Siparisler.Count, 
			enum_GenelEvrakTipleri.Tahsilat => _TahsilatHareketleri.Count, 
			enum_GenelEvrakTipleri.Tediye => _TahsilatHareketleri.Count, 
			enum_GenelEvrakTipleri.Masraf => _GenelCariHesapHareketleri.Count, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => _DepolarArasiSiparisHareketleri.Count, 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => _SayimSonuclariHareketleri.Count, 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => _StokHareketleri.Count, 
			_ => _StokHareketleri.Count + _HizmetHareketleri.Count, 
		};
	}

	public double GetToplamMiktar()
	{
		double num = 0.0;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				num += item.sip_miktar;
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				num += item2.sip_miktar;
			}
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			foreach (CARI_HESAP_HAREKETLERI item3 in _TahsilatHareketleri)
			{
				num += item3.cha_meblag;
			}
			break;
		case enum_GenelEvrakTipleri.Tediye:
			foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
			{
				num += item4.cha_meblag;
			}
			break;
		case enum_GenelEvrakTipleri.Masraf:
			foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
			{
				num += item5.cha_meblag;
			}
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
			{
				num += item6.ssip_miktar;
			}
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			foreach (SAYIM_SONUCLARI item7 in _SayimSonuclariHareketleri)
			{
				num += item7.sym_miktar1;
			}
			break;
		default:
			foreach (STOK_HAREKETLERI item8 in _StokHareketleri)
			{
				num += item8.sth_miktar;
			}
			break;
		}
		return num;
	}

	public double GetAraToplam()
	{
		double num = 0.0;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				num += item.sip_tutar;
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				num += item2.sip_tutar;
			}
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			foreach (CARI_HESAP_HAREKETLERI item3 in _TahsilatHareketleri)
			{
				num += item3.cha_aratoplam;
			}
			break;
		case enum_GenelEvrakTipleri.Tediye:
			foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
			{
				num += item4.cha_aratoplam;
			}
			break;
		case enum_GenelEvrakTipleri.Masraf:
			foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
			{
				num += item5.cha_aratoplam;
			}
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
			{
				num += item6.ssip_tutar;
			}
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			num = 0.0;
			break;
		default:
			foreach (STOK_HAREKETLERI item7 in _StokHareketleri)
			{
				bool flag = true;
				if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item7.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
				{
					flag = false;
				}
				num = ((!flag) ? (num - item7.sth_tutar) : (num + item7.sth_tutar));
			}
			foreach (CARI_HESAP_HAREKETLERI item8 in _HizmetHareketleri)
			{
				num += item8.cha_aratoplam;
			}
			break;
		}
		return num;
	}

	public double GetIskontoTutari()
	{
		double num = 0.0;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				num += item.IskontoTutariToplam;
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				num += item2.IskontoTutariToplam;
			}
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			return 0.0;
		case enum_GenelEvrakTipleri.Tediye:
			return 0.0;
		case enum_GenelEvrakTipleri.Masraf:
			return 0.0;
		default:
			foreach (STOK_HAREKETLERI item3 in _StokHareketleri)
			{
				bool flag = true;
				if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item3.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
				{
					flag = false;
				}
				num = ((!flag) ? (num - item3.IskontoTutariToplam) : (num + item3.IskontoTutariToplam));
			}
			foreach (CARI_HESAP_HAREKETLERI item4 in _HizmetHareketleri)
			{
				num += item4.IskontoTutariToplam;
			}
			break;
		}
		return num;
	}

	public double GetMasrafTutari()
	{
		double num = 0.0;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				num += item.MasrafTutariToplam;
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				num += item2.MasrafTutariToplam;
			}
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			return 0.0;
		case enum_GenelEvrakTipleri.Tediye:
			return 0.0;
		case enum_GenelEvrakTipleri.Masraf:
			return 0.0;
		default:
			foreach (STOK_HAREKETLERI item3 in _StokHareketleri)
			{
				bool flag = true;
				if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item3.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
				{
					flag = false;
				}
				num = ((!flag) ? (num - item3.MasrafTutariToplam) : (num + item3.MasrafTutariToplam));
			}
			foreach (CARI_HESAP_HAREKETLERI item4 in _HizmetHareketleri)
			{
				num += item4.MasrafTutariToplam;
			}
			break;
		}
		return num;
	}

	public double GetKdvTutari()
	{
		double num = 0.0;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				num += item.KdvTutariToplam;
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				num += item2.KdvTutariToplam;
			}
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			return 0.0;
		case enum_GenelEvrakTipleri.Tediye:
			return 0.0;
		case enum_GenelEvrakTipleri.Masraf:
			foreach (CARI_HESAP_HAREKETLERI item3 in _GenelCariHesapHareketleri)
			{
				num += item3.KdvTutariToplam;
			}
			break;
		default:
			foreach (STOK_HAREKETLERI item4 in _StokHareketleri)
			{
				bool flag = true;
				if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item4.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
				{
					flag = false;
				}
				num = ((!flag) ? (num - item4.KdvTutariToplam) : (num + item4.KdvTutariToplam));
			}
			foreach (CARI_HESAP_HAREKETLERI item5 in _HizmetHareketleri)
			{
				num += item5.KdvTutariToplam;
			}
			break;
		}
		return num;
	}

	public double GetYekun()
	{
		return GetAraToplam() - GetIskontoTutari() + GetMasrafTutari() + GetKdvTutari();
	}

	public int GetTahsilatlarOrtalamaGun()
	{
		double num = 0.0;
		double num2 = 0.0;
		foreach (CARI_HESAP_HAREKETLERI item in _TahsilatHareketleri)
		{
			int year = int.Parse(item.cha_vade.ToString().Substring(0, 4));
			int month = int.Parse(item.cha_vade.ToString().Substring(4, 2));
			int day = int.Parse(item.cha_vade.ToString().Substring(6, 2));
			int days = (new DateTime(year, month, day) - _evraktarih).Days;
			num += item.cha_meblag * (double)days;
			num2 += item.cha_meblag;
		}
		if (num == 0.0)
		{
			return 0;
		}
		return Convert.ToInt32(Math.Round(num / num2, MidpointRounding.AwayFromZero));
	}

	public List<SepetListItem> GetSepetListItems()
	{
		List<SepetListItem> list = new List<SepetListItem>();
		int num = 0;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			num = 0;
			foreach (SIPARISLER item in _Siparisler)
			{
				SepetListItem sepetListItem3 = new SepetListItem();
				sepetListItem3.Type = enum_SepetListItemType.Siparis;
				sepetListItem3.ListePozisyonu = num;
				sepetListItem3.Kodu = item.sip_stok_kod;
				sepetListItem3.Ismi = item.sto_isim;
				num++;
				sepetListItem3.Siparis = item;
				list.Add(sepetListItem3);
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			num = 0;
			foreach (SIPARISLER item2 in _Siparisler)
			{
				SepetListItem sepetListItem9 = new SepetListItem();
				sepetListItem9.Type = enum_SepetListItemType.Siparis;
				sepetListItem9.ListePozisyonu = num;
				sepetListItem9.Kodu = item2.sip_stok_kod;
				sepetListItem9.Ismi = item2.sto_isim;
				num++;
				sepetListItem9.Siparis = item2;
				list.Add(sepetListItem9);
			}
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			num = 0;
			foreach (DEPOLAR_ARASI_SIPARISLER item3 in _DepolarArasiSiparisHareketleri)
			{
				SepetListItem sepetListItem8 = new SepetListItem();
				sepetListItem8.Type = enum_SepetListItemType.DepolarArasiSiparisler;
				sepetListItem8.ListePozisyonu = num;
				sepetListItem8.Kodu = item3.ssip_stok_kod;
				sepetListItem8.Ismi = item3.sto_isim;
				num++;
				sepetListItem8.DepolarArasiSiparis = item3;
				list.Add(sepetListItem8);
			}
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			num = 0;
			foreach (SAYIM_SONUCLARI item4 in _SayimSonuclariHareketleri)
			{
				SepetListItem sepetListItem7 = new SepetListItem();
				sepetListItem7.Type = enum_SepetListItemType.SayimSonuclari;
				sepetListItem7.ListePozisyonu = num;
				sepetListItem7.Kodu = item4.sym_Stokkodu;
				sepetListItem7.Ismi = item4.sym_Stokkodu;
				num++;
				sepetListItem7.SayimSonuclariHareketi = item4;
				list.Add(sepetListItem7);
			}
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			num = 0;
			foreach (CARI_HESAP_HAREKETLERI item5 in _TahsilatHareketleri)
			{
				SepetListItem sepetListItem6 = new SepetListItem();
				sepetListItem6.Type = enum_SepetListItemType.CariHesapHareketi;
				sepetListItem6.ListePozisyonu = num;
				sepetListItem6.Kodu = item5.cha_kasa_hizkod;
				sepetListItem6.Ismi = item5.cha_kasa_hizkod;
				num++;
				sepetListItem6.CariHesapHareketi = item5;
				list.Add(sepetListItem6);
			}
			break;
		case enum_GenelEvrakTipleri.Tediye:
			num = 0;
			foreach (CARI_HESAP_HAREKETLERI item6 in _TahsilatHareketleri)
			{
				SepetListItem sepetListItem5 = new SepetListItem();
				sepetListItem5.Type = enum_SepetListItemType.CariHesapHareketi;
				sepetListItem5.ListePozisyonu = num;
				sepetListItem5.Kodu = item6.cha_kasa_hizkod;
				sepetListItem5.Ismi = item6.cha_kasa_hizkod;
				num++;
				sepetListItem5.CariHesapHareketi = item6;
				list.Add(sepetListItem5);
			}
			break;
		case enum_GenelEvrakTipleri.Masraf:
			num = 0;
			foreach (CARI_HESAP_HAREKETLERI item7 in _GenelCariHesapHareketleri)
			{
				SepetListItem sepetListItem4 = new SepetListItem();
				sepetListItem4.Type = enum_SepetListItemType.CariHesapHareketi;
				sepetListItem4.ListePozisyonu = num;
				sepetListItem4.Kodu = item7.cha_kasa_hizkod;
				sepetListItem4.Ismi = item7.cha_kasa_hizkod;
				num++;
				sepetListItem4.CariHesapHareketi = item7;
				list.Add(sepetListItem4);
			}
			break;
		default:
			num = 0;
			foreach (STOK_HAREKETLERI item8 in _StokHareketleri)
			{
				SepetListItem sepetListItem = new SepetListItem();
				sepetListItem.Type = enum_SepetListItemType.StokHareketi;
				sepetListItem.ListePozisyonu = num;
				sepetListItem.Kodu = item8.sth_stok_kod;
				sepetListItem.Ismi = item8.sto_isim;
				num++;
				sepetListItem.StokHareketi = item8;
				list.Add(sepetListItem);
			}
			num = 0;
			foreach (CARI_HESAP_HAREKETLERI item9 in _HizmetHareketleri)
			{
				SepetListItem sepetListItem2 = new SepetListItem();
				sepetListItem2.Type = enum_SepetListItemType.CariHesapHareketi;
				sepetListItem2.ListePozisyonu = num;
				sepetListItem2.Kodu = item9.cha_kasa_hizkod;
				sepetListItem2.Ismi = item9.cha_kasa_hizkod;
				num++;
				sepetListItem2.CariHesapHareketi = item9;
				list.Add(sepetListItem2);
			}
			break;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama || evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			List<SepetListItem> list2 = new List<SepetListItem>();
			List<SepetListItem> list3 = new List<SepetListItem>();
			foreach (SepetListItem item10 in list)
			{
				if (item10.StokHareketi.sth_miktar != item10.StokHareketi.OkutulanMiktar)
				{
					list2.Add(item10);
				}
				else
				{
					list3.Add(item10);
				}
			}
			list = new List<SepetListItem>();
			list.AddRange(list2);
			list.AddRange(list3);
			num = 0;
			foreach (SepetListItem item11 in list)
			{
				item11.ListePozisyonu = num;
				num++;
			}
		}
		switch (Parametreler.sepet_siralama_secenegi)
		{
		case enum_evrak_sepet_siralama_secenekleri.EklemeSirasiArtan:
			list = Enumerable.ToList<SepetListItem>((IEnumerable<SepetListItem>)Enumerable.OrderBy<SepetListItem, int>((IEnumerable<SepetListItem>)list, (Func<SepetListItem, int>)((SepetListItem y) => y.ListePozisyonu)));
			break;
		case enum_evrak_sepet_siralama_secenekleri.EklemeSirasiAzalan:
			list = Enumerable.ToList<SepetListItem>((IEnumerable<SepetListItem>)Enumerable.OrderByDescending<SepetListItem, int>((IEnumerable<SepetListItem>)list, (Func<SepetListItem, int>)((SepetListItem y) => y.ListePozisyonu)));
			break;
		case enum_evrak_sepet_siralama_secenekleri.IsmeGoreArtan:
			list = Enumerable.ToList<SepetListItem>((IEnumerable<SepetListItem>)Enumerable.OrderBy<SepetListItem, string>((IEnumerable<SepetListItem>)list, (Func<SepetListItem, string>)((SepetListItem y) => y.Ismi)));
			break;
		case enum_evrak_sepet_siralama_secenekleri.IsmeGoreAzalan:
			list = Enumerable.ToList<SepetListItem>((IEnumerable<SepetListItem>)Enumerable.OrderByDescending<SepetListItem, string>((IEnumerable<SepetListItem>)list, (Func<SepetListItem, string>)((SepetListItem y) => y.Ismi)));
			break;
		case enum_evrak_sepet_siralama_secenekleri.KodaGoreArtan:
			list = Enumerable.ToList<SepetListItem>((IEnumerable<SepetListItem>)Enumerable.OrderBy<SepetListItem, string>((IEnumerable<SepetListItem>)list, (Func<SepetListItem, string>)((SepetListItem y) => y.Kodu)));
			break;
		case enum_evrak_sepet_siralama_secenekleri.KodaGoreAzalan:
			list = Enumerable.ToList<SepetListItem>((IEnumerable<SepetListItem>)Enumerable.OrderByDescending<SepetListItem, string>((IEnumerable<SepetListItem>)list, (Func<SepetListItem, string>)((SepetListItem y) => y.Kodu)));
			break;
		}
		return list;
	}

	public List<STOK_HAREKETLERI> GetStokHareketleri()
	{
		return _StokHareketleri;
	}

	public List<DEPOLAR_ARASI_SIPARISLER> GetDepolarArasiSiparisHareketleri()
	{
		return _DepolarArasiSiparisHareketleri;
	}

	public List<SAYIM_SONUCLARI> GetSayimSonuclariHareketleri()
	{
		return _SayimSonuclariHareketleri;
	}

	public List<CARI_HESAP_HAREKETLERI> GetHizmetHareketleri()
	{
		return _HizmetHareketleri;
	}

	public List<CARI_HESAP_HAREKETLERI> GetTahsilatHareketleri()
	{
		return _TahsilatHareketleri;
	}

	public List<CARI_HESAP_HAREKETLERI> GetGenelCariHesapHareketleri()
	{
		return _GenelCariHesapHareketleri;
	}

	public List<BAKIM_KABUL_HAREKETLERI> GetBakimKabulHareketleri()
	{
		return _BakimKabulHareketleri;
	}

	public CARI_HESAP_HAREKETLERI GetStokCariHesapHareketi()
	{
		return _StokCariHesapHareketi;
	}

	public CARI_HESAP_HAREKETLERI GetStokCariHesapHareketiIadeliSatis()
	{
		return _StokCariHesapHareketiIadeliSatis;
	}

	public CARI_HESAP_HAREKETLERI GetStokFiyatFarkiCariHesapHareketi()
	{
		return _StokFiyatFarkiCariHesapHareketi;
	}

	public List<SIPARISLER> GetSiparisler()
	{
		return _Siparisler;
	}

	public List<SIPARISLER> GetSiparisKarsilamaItems()
	{
		return _Siparisler;
	}

	public bool IsUrunListedeVar(string sto_kod)
	{
		bool result = false;
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				if (item.sip_stok_kod == sto_kod)
				{
					result = true;
					break;
				}
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				if (item2.sip_stok_kod == sto_kod)
				{
					result = true;
					break;
				}
			}
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			foreach (DEPOLAR_ARASI_SIPARISLER item3 in _DepolarArasiSiparisHareketleri)
			{
				if (item3.ssip_stok_kod == sto_kod)
				{
					result = true;
					break;
				}
			}
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			foreach (SAYIM_SONUCLARI item4 in _SayimSonuclariHareketleri)
			{
				if (item4.sym_Stokkodu == sto_kod)
				{
					result = true;
					break;
				}
			}
			break;
		default:
			foreach (STOK_HAREKETLERI item5 in _StokHareketleri)
			{
				if (item5.sth_stok_kod == sto_kod)
				{
					result = true;
					break;
				}
			}
			break;
		}
		return result;
	}

	public bool IsSiparisKarsilamayaUygun()
	{
		return evraktipi switch
		{
			enum_GenelEvrakTipleri.AlisFaturasi => true, 
			enum_GenelEvrakTipleri.SatisFaturasi => true, 
			enum_GenelEvrakTipleri.AlinanSiparis => false, 
			enum_GenelEvrakTipleri.ProformaSiparis => false, 
			enum_GenelEvrakTipleri.AlisIrsaliyesi => true, 
			enum_GenelEvrakTipleri.DepolarArasiSevk => true, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => false, 
			enum_GenelEvrakTipleri.KonsinyedenIadeIrsaliyesi => false, 
			enum_GenelEvrakTipleri.KonsinyeIrsaliyesi => false, 
			enum_GenelEvrakTipleri.SatisIrsaliyesi => true, 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => false, 
			enum_GenelEvrakTipleri.Tahsilat => false, 
			enum_GenelEvrakTipleri.Tediye => false, 
			enum_GenelEvrakTipleri.Masraf => false, 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => true, 
			_ => false, 
		};
	}

	public bool IsElleStokEklenebilir()
	{
		bool result = false;
		if (!evrakkilitli && yenikayit)
		{
			result = true;
		}
		if ((evraktipi == enum_GenelEvrakTipleri.AlinanSiparis || evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi || evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama || evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis || evraktipi == enum_GenelEvrakTipleri.ProformaSiparis || evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi) && Parametreler.EvrakGirisiBarkodOkuyucuZorunlu)
		{
			result = false;
		}
		return result;
	}

	public bool IsBarkodOkutulabilir()
	{
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evrakkilitli)
		{
			return false;
		}
		if (!yenikayit)
		{
			return false;
		}
		return true;
	}

	public bool IsEvrakDuzenlenebilir()
	{
		bool result = true;
		if (!evrakkilitli && yenikayit)
		{
			result = false;
		}
		return result;
	}

	public enum_evrak_cari_secilebilir_sonuc IsCariSecilebilir(Cari kontrol_cari)
	{
		enum_evrak_cari_secilebilir_sonuc result = enum_evrak_cari_secilebilir_sonuc.Secilebilir;
		switch (kontrol_cari.cari_hareket_tipi)
		{
		case 1:
			if (GetSatisAlis == enum_SatisAlis.Alis)
			{
				result = enum_evrak_cari_secilebilir_sonuc.SadeceSatisYapilabilir;
			}
			break;
		case 2:
			if (GetSatisAlis == enum_SatisAlis.Satis)
			{
				result = enum_evrak_cari_secilebilir_sonuc.SadeceAlisYapilabilir;
			}
			break;
		case 3:
			if (evraktipi != enum_GenelEvrakTipleri.Tahsilat && evraktipi != enum_GenelEvrakTipleri.Tediye)
			{
				result = enum_evrak_cari_secilebilir_sonuc.SadeceParasalHareketYapilabilir;
			}
			break;
		case 4:
			result = enum_evrak_cari_secilebilir_sonuc.CariHareketYapilamaz;
			break;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseSiparisAlma)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		if (evraktipi == enum_GenelEvrakTipleri.ProformaSiparis && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseSiparisAlma)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseFaturaKesme)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseIrsaliyeKesme)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseFaturaAlma)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseIrsaliyeAlma)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseTahsilatYapma)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye && kontrol_cari.cari_cari_kilitli_flg && Parametreler.CariKilitliIseTahsilatYapma)
		{
			result = enum_evrak_cari_secilebilir_sonuc.CariKilitliDurumda;
		}
		return result;
	}

	public bool IsEvrakDetaylariDegistirilebilir()
	{
		if (!evrakkilitli && yenikayit && evraktipi != enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return true;
		}
		return false;
	}

	public bool IsCariDestekliyor()
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return true;
	}

	public bool IsGosterFirmaNo()
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		return Parametreler.GormeFirmaNo;
	}

	public bool IsGosterSubeNo()
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		return Parametreler.GormeSubeNo;
	}

	public bool IsGosterNormalIade()
	{
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeNormalIade;
	}

	public bool IsGosterTicaretTuru()
	{
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeTicaretTuru;
	}

	public bool IsGosterEvrakSeri()
	{
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeEvrakSeri;
	}

	public bool IsGosterEvrakSira()
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeEvrakSira;
	}

	public bool IsGosterBelgeNo()
	{
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeBelgeNo;
	}

	public bool IsGosterBelgeTarihi()
	{
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeBelgeTarihi;
	}

	public bool IsGosterCariKodu()
	{
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeCariKodu;
	}

	public bool IsGosterCariAdres()
	{
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeCariAdres;
	}

	public bool IsGosterKapamaSekli()
	{
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeKapamaSekli;
	}

	public bool IsGosterKapamaHesapKodu()
	{
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeKapamaHesapKodu;
	}

	public bool IsGosterOdemePlani()
	{
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		return Parametreler.GormeOdemePlani;
	}

	public bool IsGosterFiyatListesi()
	{
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		return Parametreler.GormeFiyatListesi;
	}

	public bool IsGosterDovizCinsi()
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		return Parametreler.GormeDovizCinsi;
	}

	public bool IsGosterKur()
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeKur;
	}

	public bool IsGosterProje()
	{
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeProje;
	}

	public bool IsGosterSorumlulukMerkezi()
	{
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeSorumlulukMerkezi;
	}

	public bool IsGosterKaynakDepo()
	{
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		return Parametreler.GormeKaynakDepo;
	}

	public bool IsGosterNakliyeDepo()
	{
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeNakliyeDepo;
	}

	public bool IsGosterHedefDepo()
	{
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeHedefDepo;
	}

	public bool IsGosterTarih()
	{
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeTarih;
	}

	public bool IsGosterTeslimTarihi()
	{
		if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeTeslimTarihi;
	}

	public bool IsGosterSiparisTeslimTuru()
	{
		if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeSiparisTeslimTuru;
	}

	public bool IsGosterIhracatKodu()
	{
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SatisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Tediye)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.Masraf)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama)
		{
			return false;
		}
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return true;
	}

	public bool IsGosterAciklama1()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama1;
	}

	public bool IsGosterAciklama2()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama2;
	}

	public bool IsGosterAciklama3()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama3;
	}

	public bool IsGosterAciklama4()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama4;
	}

	public bool IsGosterAciklama5()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama5;
	}

	public bool IsGosterAciklama6()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama6;
	}

	public bool IsGosterAciklama7()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama7;
	}

	public bool IsGosterAciklama8()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama8;
	}

	public bool IsGosterAciklama9()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama9;
	}

	public bool IsGosterAciklama10()
	{
		if (evraktipi == enum_GenelEvrakTipleri.StokFiyatEtiketiYazdirma)
		{
			return false;
		}
		return Parametreler.GormeAciklama10;
	}

	public void SetMikroUserNo(int YeniDeger)
	{
		_mikrouserno = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama(string YeniDeger)
	{
		string text = YeniDeger;
		if (text.Length > 40)
		{
			text = text.Substring(0, 40);
		}
		_aciklama = text;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetSevkAdresNo(int YeniDeger)
	{
		_sevkadresno = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetKapamaHesapKodu(string YeniDeger)
	{
		int num = 25;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_kapamahesapkodu = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetDegistirSpecialAlan1(string YeniDeger)
	{
		int num = 4;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_degistirspecialalan1 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetDegistirSpecialAlan2(string YeniDeger)
	{
		int num = 4;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_degistirspecialalan2 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetDegistirSpecialAlan3(string YeniDeger)
	{
		int num = 4;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_degistirspecialalan3 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetEximKodu(string YeniDeger)
	{
		int num = 25;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_EximKodu = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetDBCno(int YeniDeger)
	{
		_DBCno = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetTemsilciKodu(string YeniDeger)
	{
		int num = 25;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_TemsilciKodu = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAlternatifDovizCinsi(int YeniDeger)
	{
		_alternatifdovizcinsi = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetDovizCinsi(int YeniDeger)
	{
		_dovizcinsi = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetOdemePlani(int YeniDeger)
	{
		_odemeplani = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetSiparisKarsilamaMi(bool YeniDeger)
	{
		_sipariskarsilamami = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetEvrakKilitli(bool YeniDeger)
	{
		_evrakkilitli = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetYeniKayit(bool YeniDeger)
	{
		_yenikayit = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetOfflineRecNo(int YeniDeger)
	{
		_offlinerecno = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetDovizCinsiveKur(int dovizcinsi, Kur kur)
	{
		_dovizcinsi = dovizcinsi;
		_kur = kur;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetSip_TeslimTuru(string YeniDeger)
	{
		int num = 4;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_sip_teslimturu = YeniDeger;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_teslimturu = YeniDeger;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama1(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama1 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama2(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama2 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama3(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama3 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama4(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama4 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama5(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama5 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama6(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama6 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama7(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama7 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama8(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama8 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama9(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama9 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetAciklama10(string YeniDeger)
	{
		int num = 126;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_aciklama10 = YeniDeger;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetIskonto(List<int> indexler, int Iskonto_1_UygulamaSekli, double Iskonto_1_YuzdeVeyaMiktar, int Iskonto_2_UygulamaSekli, double Iskonto_2_YuzdeVeyaMiktar, int Iskonto_3_UygulamaSekli, double Iskonto_3_YuzdeVeyaMiktar, int Iskonto_4_UygulamaSekli, double Iskonto_4_YuzdeVeyaMiktar, int Iskonto_5_UygulamaSekli, double Iskonto_5_YuzdeVeyaMiktar, int Iskonto_6_UygulamaSekli, double Iskonto_6_YuzdeVeyaMiktar)
	{
		if (_evraktipi == enum_GenelEvrakTipleri.AlinanSiparis || _evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			foreach (int item in indexler)
			{
				double num = _Siparisler[item].sip_vergi * 100.0 / (_Siparisler[item].sip_tutar - _Siparisler[item].sip_iskonto_1 - _Siparisler[item].sip_iskonto_2 - _Siparisler[item].sip_iskonto_3 - _Siparisler[item].sip_iskonto_4 - _Siparisler[item].sip_iskonto_5 - _Siparisler[item].sip_iskonto_6);
				if (Iskonto_1_YuzdeVeyaMiktar != 0.0)
				{
					double num2 = 0.0;
					switch (Iskonto_1_UygulamaSekli)
					{
					case 0:
						num2 = _Siparisler[item].sip_tutar;
						_Siparisler[item].sip_iskonto_1 = num2 * Iskonto_1_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num2 = _Siparisler[item].sip_tutar;
						_Siparisler[item].sip_iskonto_1 = num2 * Iskonto_1_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_Siparisler[item].sip_iskonto_1 = Iskonto_1_YuzdeVeyaMiktar * _Siparisler[item].sip_miktar;
						break;
					case 4:
						_Siparisler[item].sip_iskonto_1 = Iskonto_1_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_Siparisler[item].sip_iskonto_1 = Iskonto_1_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_Siparisler[item].sip_iskonto_1 = 0.0;
				}
				if (Iskonto_2_YuzdeVeyaMiktar != 0.0)
				{
					double num3 = 0.0;
					switch (Iskonto_2_UygulamaSekli)
					{
					case 0:
						num3 = _Siparisler[item].sip_tutar;
						_Siparisler[item].sip_iskonto_2 = num3 * Iskonto_2_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num3 = _Siparisler[item].sip_tutar - _Siparisler[item].sip_iskonto_1;
						_Siparisler[item].sip_iskonto_2 = num3 * Iskonto_2_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_Siparisler[item].sip_iskonto_2 = Iskonto_2_YuzdeVeyaMiktar * _Siparisler[item].sip_miktar;
						break;
					case 4:
						_Siparisler[item].sip_iskonto_2 = Iskonto_2_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_Siparisler[item].sip_iskonto_2 = Iskonto_2_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_Siparisler[item].sip_iskonto_2 = 0.0;
				}
				if (Iskonto_3_YuzdeVeyaMiktar != 0.0)
				{
					double num4 = 0.0;
					switch (Iskonto_3_UygulamaSekli)
					{
					case 0:
						num4 = _Siparisler[item].sip_tutar;
						_Siparisler[item].sip_iskonto_3 = num4 * Iskonto_3_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num4 = _Siparisler[item].sip_tutar - _Siparisler[item].sip_iskonto_1 - _Siparisler[item].sip_iskonto_2;
						_Siparisler[item].sip_iskonto_3 = num4 * Iskonto_3_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_Siparisler[item].sip_iskonto_3 = Iskonto_3_YuzdeVeyaMiktar * _Siparisler[item].sip_miktar;
						break;
					case 4:
						_Siparisler[item].sip_iskonto_3 = Iskonto_3_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_Siparisler[item].sip_iskonto_3 = Iskonto_3_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_Siparisler[item].sip_iskonto_3 = 0.0;
				}
				if (Iskonto_4_YuzdeVeyaMiktar != 0.0)
				{
					double num5 = 0.0;
					switch (Iskonto_4_UygulamaSekli)
					{
					case 0:
						num5 = _Siparisler[item].sip_tutar;
						_Siparisler[item].sip_iskonto_4 = num5 * Iskonto_4_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num5 = _Siparisler[item].sip_tutar - _Siparisler[item].sip_iskonto_1 - _Siparisler[item].sip_iskonto_2 - _Siparisler[item].sip_iskonto_3;
						_Siparisler[item].sip_iskonto_4 = num5 * Iskonto_4_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_Siparisler[item].sip_iskonto_4 = Iskonto_4_YuzdeVeyaMiktar * _Siparisler[item].sip_miktar;
						break;
					case 4:
						_Siparisler[item].sip_iskonto_4 = Iskonto_4_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_Siparisler[item].sip_iskonto_4 = Iskonto_4_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_Siparisler[item].sip_iskonto_4 = 0.0;
				}
				if (Iskonto_5_YuzdeVeyaMiktar != 0.0)
				{
					double num6 = 0.0;
					switch (Iskonto_5_UygulamaSekli)
					{
					case 0:
						num6 = _Siparisler[item].sip_tutar;
						_Siparisler[item].sip_iskonto_5 = num6 * Iskonto_5_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num6 = _Siparisler[item].sip_tutar - _Siparisler[item].sip_iskonto_1 - _Siparisler[item].sip_iskonto_2 - _Siparisler[item].sip_iskonto_3 - _Siparisler[item].sip_iskonto_4;
						_Siparisler[item].sip_iskonto_5 = num6 * Iskonto_5_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_Siparisler[item].sip_iskonto_5 = Iskonto_5_YuzdeVeyaMiktar * _Siparisler[item].sip_miktar;
						break;
					case 4:
						_Siparisler[item].sip_iskonto_5 = Iskonto_5_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_Siparisler[item].sip_iskonto_5 = Iskonto_5_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_Siparisler[item].sip_iskonto_5 = 0.0;
				}
				if (Iskonto_6_YuzdeVeyaMiktar != 0.0)
				{
					double num7 = 0.0;
					switch (Iskonto_6_UygulamaSekli)
					{
					case 0:
						num7 = _Siparisler[item].sip_tutar;
						_Siparisler[item].sip_iskonto_6 = num7 * Iskonto_6_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num7 = _Siparisler[item].sip_tutar - _Siparisler[item].sip_iskonto_1 - _Siparisler[item].sip_iskonto_2 - _Siparisler[item].sip_iskonto_3 - _Siparisler[item].sip_iskonto_4 - _Siparisler[item].sip_iskonto_5;
						_Siparisler[item].sip_iskonto_6 = num7 * Iskonto_6_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_Siparisler[item].sip_iskonto_6 = Iskonto_6_YuzdeVeyaMiktar * _Siparisler[item].sip_miktar;
						break;
					case 4:
						_Siparisler[item].sip_iskonto_6 = Iskonto_6_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_Siparisler[item].sip_iskonto_6 = Iskonto_6_YuzdeVeyaMiktar * (_Siparisler[item].sip_miktar / (_Siparisler[item].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_Siparisler[item].sip_iskonto_6 = 0.0;
				}
				_Siparisler[item].sip_vergi = (_Siparisler[item].sip_tutar - _Siparisler[item].sip_iskonto_1 - _Siparisler[item].sip_iskonto_2 - _Siparisler[item].sip_iskonto_3 - _Siparisler[item].sip_iskonto_4 - _Siparisler[item].sip_iskonto_5 - _Siparisler[item].sip_iskonto_6) / 100.0 * num;
			}
		}
		else
		{
			foreach (int item2 in indexler)
			{
				double num8 = _StokHareketleri[item2].sth_vergi * 100.0 / (_StokHareketleri[item2].sth_tutar - _StokHareketleri[item2].sth_iskonto1 - _StokHareketleri[item2].sth_iskonto2 - _StokHareketleri[item2].sth_iskonto3 - _StokHareketleri[item2].sth_iskonto4 - _StokHareketleri[item2].sth_iskonto5 - _StokHareketleri[item2].sth_iskonto6);
				if (Iskonto_1_YuzdeVeyaMiktar != 0.0)
				{
					double num9 = 0.0;
					switch (Iskonto_1_UygulamaSekli)
					{
					case 0:
						num9 = _StokHareketleri[item2].sth_tutar;
						_StokHareketleri[item2].sth_iskonto1 = num9 * Iskonto_1_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num9 = _StokHareketleri[item2].sth_tutar;
						_StokHareketleri[item2].sth_iskonto1 = num9 * Iskonto_1_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_StokHareketleri[item2].sth_iskonto1 = Iskonto_1_YuzdeVeyaMiktar * _StokHareketleri[item2].sth_miktar;
						break;
					case 4:
						_StokHareketleri[item2].sth_iskonto1 = Iskonto_1_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_StokHareketleri[item2].sth_iskonto1 = Iskonto_1_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_StokHareketleri[item2].sth_iskonto1 = 0.0;
				}
				if (Iskonto_2_YuzdeVeyaMiktar != 0.0)
				{
					double num10 = 0.0;
					switch (Iskonto_2_UygulamaSekli)
					{
					case 0:
						num10 = _StokHareketleri[item2].sth_tutar;
						_StokHareketleri[item2].sth_iskonto2 = num10 * Iskonto_2_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num10 = _StokHareketleri[item2].sth_tutar - _StokHareketleri[item2].sth_iskonto1;
						_StokHareketleri[item2].sth_iskonto2 = num10 * Iskonto_2_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_StokHareketleri[item2].sth_iskonto2 = Iskonto_2_YuzdeVeyaMiktar * _StokHareketleri[item2].sth_miktar;
						break;
					case 4:
						_StokHareketleri[item2].sth_iskonto2 = Iskonto_2_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_StokHareketleri[item2].sth_iskonto2 = Iskonto_2_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_StokHareketleri[item2].sth_iskonto2 = 0.0;
				}
				if (Iskonto_3_YuzdeVeyaMiktar != 0.0)
				{
					double num11 = 0.0;
					switch (Iskonto_3_UygulamaSekli)
					{
					case 0:
						num11 = _StokHareketleri[item2].sth_tutar;
						_StokHareketleri[item2].sth_iskonto3 = num11 * Iskonto_3_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num11 = _StokHareketleri[item2].sth_tutar - _StokHareketleri[item2].sth_iskonto1 - _StokHareketleri[item2].sth_iskonto2;
						_StokHareketleri[item2].sth_iskonto3 = num11 * Iskonto_3_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_StokHareketleri[item2].sth_iskonto3 = Iskonto_3_YuzdeVeyaMiktar * _StokHareketleri[item2].sth_miktar;
						break;
					case 4:
						_StokHareketleri[item2].sth_iskonto3 = Iskonto_3_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_StokHareketleri[item2].sth_iskonto3 = Iskonto_3_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_StokHareketleri[item2].sth_iskonto3 = 0.0;
				}
				if (Iskonto_4_YuzdeVeyaMiktar != 0.0)
				{
					double num12 = 0.0;
					switch (Iskonto_4_UygulamaSekli)
					{
					case 0:
						num12 = _StokHareketleri[item2].sth_tutar;
						_StokHareketleri[item2].sth_iskonto4 = num12 * Iskonto_4_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num12 = _StokHareketleri[item2].sth_tutar - _StokHareketleri[item2].sth_iskonto1 - _StokHareketleri[item2].sth_iskonto2 - _StokHareketleri[item2].sth_iskonto3;
						_StokHareketleri[item2].sth_iskonto4 = num12 * Iskonto_4_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_StokHareketleri[item2].sth_iskonto4 = Iskonto_4_YuzdeVeyaMiktar * _StokHareketleri[item2].sth_miktar;
						break;
					case 4:
						_StokHareketleri[item2].sth_iskonto4 = Iskonto_4_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_StokHareketleri[item2].sth_iskonto4 = Iskonto_4_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_StokHareketleri[item2].sth_iskonto4 = 0.0;
				}
				if (Iskonto_5_YuzdeVeyaMiktar != 0.0)
				{
					double num13 = 0.0;
					switch (Iskonto_5_UygulamaSekli)
					{
					case 0:
						num13 = _StokHareketleri[item2].sth_tutar;
						_StokHareketleri[item2].sth_iskonto5 = num13 * Iskonto_5_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num13 = _StokHareketleri[item2].sth_tutar - _StokHareketleri[item2].sth_iskonto1 - _StokHareketleri[item2].sth_iskonto2 - _StokHareketleri[item2].sth_iskonto3 - _StokHareketleri[item2].sth_iskonto4;
						_StokHareketleri[item2].sth_iskonto5 = num13 * Iskonto_5_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_StokHareketleri[item2].sth_iskonto5 = Iskonto_5_YuzdeVeyaMiktar * _StokHareketleri[item2].sth_miktar;
						break;
					case 4:
						_StokHareketleri[item2].sth_iskonto5 = Iskonto_5_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_StokHareketleri[item2].sth_iskonto5 = Iskonto_5_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_StokHareketleri[item2].sth_iskonto5 = 0.0;
				}
				if (Iskonto_6_YuzdeVeyaMiktar != 0.0)
				{
					double num14 = 0.0;
					switch (Iskonto_6_UygulamaSekli)
					{
					case 0:
						num14 = _StokHareketleri[item2].sth_tutar;
						_StokHareketleri[item2].sth_iskonto6 = num14 * Iskonto_6_YuzdeVeyaMiktar / 100.0;
						break;
					case 1:
						num14 = _StokHareketleri[item2].sth_tutar - _StokHareketleri[item2].sth_iskonto1 - _StokHareketleri[item2].sth_iskonto2 - _StokHareketleri[item2].sth_iskonto3 - _StokHareketleri[item2].sth_iskonto4 - _StokHareketleri[item2].sth_iskonto5;
						_StokHareketleri[item2].sth_iskonto6 = num14 * Iskonto_6_YuzdeVeyaMiktar / 100.0;
						break;
					case 2:
						_StokHareketleri[item2].sth_iskonto6 = Iskonto_6_YuzdeVeyaMiktar * _StokHareketleri[item2].sth_miktar;
						break;
					case 4:
						_StokHareketleri[item2].sth_iskonto6 = Iskonto_6_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim2_katsayi * -1.0));
						break;
					case 5:
						_StokHareketleri[item2].sth_iskonto6 = Iskonto_6_YuzdeVeyaMiktar * (_StokHareketleri[item2].sth_miktar / (_StokHareketleri[item2].sto_birim3_katsayi * -1.0));
						break;
					}
				}
				else
				{
					_StokHareketleri[item2].sth_iskonto6 = 0.0;
				}
				_StokHareketleri[item2].sth_vergi = (_StokHareketleri[item2].sth_tutar - _StokHareketleri[item2].sth_iskonto1 - _StokHareketleri[item2].sth_iskonto2 - _StokHareketleri[item2].sth_iskonto3 - _StokHareketleri[item2].sth_iskonto4 - _StokHareketleri[item2].sth_iskonto5 - _StokHareketleri[item2].sth_iskonto6) / 100.0 * num8;
			}
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public bool SetDipIskonto(double YeniYekun, int DipIskontoAlani, double MakismumIskontoYuzdesi)
	{
		bool flag = true;
		if (_evraktipi == enum_GenelEvrakTipleri.AlinanSiparis || _evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
		{
			foreach (SIPARISLER item in _Siparisler)
			{
				if (item.sip_tutar != item.sip_iskonto_1)
				{
					double num = item.sip_vergi * 100.0 / (item.sip_tutar - item.sip_iskonto_1 - item.sip_iskonto_2 - item.sip_iskonto_3 - item.sip_iskonto_4 - item.sip_iskonto_5 - item.sip_iskonto_6);
					switch (DipIskontoAlani)
					{
					case 1:
						item.sip_iskonto_1 = 0.0;
						break;
					case 2:
						item.sip_iskonto_2 = 0.0;
						break;
					case 3:
						item.sip_iskonto_3 = 0.0;
						break;
					case 4:
						item.sip_iskonto_4 = 0.0;
						break;
					case 5:
						item.sip_iskonto_5 = 0.0;
						break;
					case 6:
						item.sip_iskonto_6 = 0.0;
						break;
					}
					item.sip_vergi = (item.sip_tutar - item.sip_iskonto_1 - item.sip_iskonto_2 - item.sip_iskonto_3 - item.sip_iskonto_4 - item.sip_iskonto_5 - item.sip_iskonto_6) / 100.0 * num;
				}
			}
		}
		else
		{
			foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
			{
				if (item2.sth_tutar != item2.sth_iskonto1)
				{
					double num2 = item2.sth_vergi * 100.0 / (item2.sth_tutar - item2.sth_iskonto1 - item2.sth_iskonto2 - item2.sth_iskonto3 - item2.sth_iskonto4 - item2.sth_iskonto5 - item2.sth_iskonto6);
					switch (DipIskontoAlani)
					{
					case 1:
						item2.sth_iskonto1 = 0.0;
						break;
					case 2:
						item2.sth_iskonto2 = 0.0;
						break;
					case 3:
						item2.sth_iskonto3 = 0.0;
						break;
					case 4:
						item2.sth_iskonto4 = 0.0;
						break;
					case 5:
						item2.sth_iskonto5 = 0.0;
						break;
					case 6:
						item2.sth_iskonto6 = 0.0;
						break;
					}
					item2.sth_vergi = (item2.sth_tutar - item2.sth_iskonto1 - item2.sth_iskonto2 - item2.sth_iskonto3 - item2.sth_iskonto4 - item2.sth_iskonto5 - item2.sth_iskonto6) / 100.0 * num2;
				}
			}
		}
		double num3 = YeniYekun / GetYekun();
		if ((1.0 - num3) * 100.0 > MakismumIskontoYuzdesi)
		{
			flag = false;
		}
		if (flag)
		{
			if (_evraktipi == enum_GenelEvrakTipleri.AlinanSiparis || _evraktipi == enum_GenelEvrakTipleri.ProformaSiparis)
			{
				foreach (SIPARISLER item3 in _Siparisler)
				{
					if (item3.sip_tutar != item3.sip_iskonto_1)
					{
						double num4 = item3.sip_vergi * 100.0 / (item3.sip_tutar - item3.sip_iskonto_1 - item3.sip_iskonto_2 - item3.sip_iskonto_3 - item3.sip_iskonto_4 - item3.sip_iskonto_5 - item3.sip_iskonto_6);
						double num5 = item3.sip_tutar - item3.sip_iskonto_1 - item3.sip_iskonto_2 - item3.sip_iskonto_3 - item3.sip_iskonto_4 - item3.sip_iskonto_5 - item3.sip_iskonto_6;
						double num6 = num5 - num5 * num3;
						switch (DipIskontoAlani)
						{
						case 1:
							item3.sip_iskonto_1 = num6;
							break;
						case 2:
							item3.sip_iskonto_2 = num6;
							break;
						case 3:
							item3.sip_iskonto_3 = num6;
							break;
						case 4:
							item3.sip_iskonto_4 = num6;
							break;
						case 5:
							item3.sip_iskonto_5 = num6;
							break;
						case 6:
							item3.sip_iskonto_6 = num6;
							break;
						}
						item3.sip_vergi = (item3.sip_tutar - item3.sip_iskonto_1 - item3.sip_iskonto_2 - item3.sip_iskonto_3 - item3.sip_iskonto_4 - item3.sip_iskonto_5 - item3.sip_iskonto_6) / 100.0 * num4;
					}
				}
			}
			else
			{
				foreach (STOK_HAREKETLERI item4 in _StokHareketleri)
				{
					if (item4.sth_tutar != item4.sth_iskonto1)
					{
						double num7 = item4.sth_vergi * 100.0 / (item4.sth_tutar - item4.sth_iskonto1 - item4.sth_iskonto2 - item4.sth_iskonto3 - item4.sth_iskonto4 - item4.sth_iskonto5 - item4.sth_iskonto6);
						double num8 = item4.sth_tutar - item4.sth_iskonto1 - item4.sth_iskonto2 - item4.sth_iskonto3 - item4.sth_iskonto4 - item4.sth_iskonto5 - item4.sth_iskonto6;
						double num9 = num8 - num8 * num3;
						switch (DipIskontoAlani)
						{
						case 1:
							item4.sth_iskonto1 = num9;
							break;
						case 2:
							item4.sth_iskonto2 = num9;
							break;
						case 3:
							item4.sth_iskonto3 = num9;
							break;
						case 4:
							item4.sth_iskonto4 = num9;
							break;
						case 5:
							item4.sth_iskonto5 = num9;
							break;
						case 6:
							item4.sth_iskonto6 = num9;
							break;
						}
						item4.sth_vergi = (item4.sth_tutar - item4.sth_iskonto1 - item4.sth_iskonto2 - item4.sth_iskonto3 - item4.sth_iskonto4 - item4.sth_iskonto5 - item4.sth_iskonto6) / 100.0 * num7;
					}
				}
			}
		}
		OnEvrakChanged(EventArgs.Empty);
		return flag;
	}

	public void SetEvrakTarihi(DateTime YeniEvrakTarihi)
	{
		_evraktarih = YeniEvrakTarihi;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_tarih = YeniEvrakTarihi;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_tarih = YeniEvrakTarihi;
		}
		_StokCariHesapHareketi.cha_tarihi = YeniEvrakTarihi;
		_StokCariHesapHareketiIadeliSatis.cha_tarihi = YeniEvrakTarihi;
		_StokFiyatFarkiCariHesapHareketi.cha_tarihi = YeniEvrakTarihi;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_tarihi = YeniEvrakTarihi;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_tarihi = YeniEvrakTarihi;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_tarihi = YeniEvrakTarihi;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_tarih = YeniEvrakTarihi;
		}
		foreach (SAYIM_SONUCLARI item7 in _SayimSonuclariHareketleri)
		{
			item7.sym_tarihi = YeniEvrakTarihi;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item8 in _BakimKabulHareketleri)
		{
			item8.bkmkb_tarihi = YeniEvrakTarihi;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetSevkTeslimTarihi(DateTime Tarih)
	{
		_sevkteslimtarihi = Tarih;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_teslim_tarih = Tarih;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_malkbl_sevk_tarihi = Tarih;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item3 in _DepolarArasiSiparisHareketleri)
		{
			item3.ssip_teslim_tarih = Tarih;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetBelgeTarihi(DateTime Tarih)
	{
		_belgetarih = Tarih;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_belge_tarih = Tarih;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_tarih = Tarih;
		}
		_StokCariHesapHareketi.cha_belge_tarih = Tarih;
		_StokCariHesapHareketiIadeliSatis.cha_belge_tarih = Tarih;
		_StokFiyatFarkiCariHesapHareketi.cha_belge_tarih = Tarih;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_belge_tarih = Tarih;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_belge_tarih = Tarih;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_belge_tarih = Tarih;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_belge_tarih = Tarih;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item7 in _BakimKabulHareketleri)
		{
			item7.bkmkb_belge_tarihi = Tarih;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetFirma(Firma firma)
	{
		_firma = firma;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_firmano = firma.fir_sirano;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_firmano = firma.fir_sirano;
		}
		_StokCariHesapHareketi.cha_firmano = firma.fir_sirano;
		_StokCariHesapHareketiIadeliSatis.cha_firmano = firma.fir_sirano;
		_StokFiyatFarkiCariHesapHareketi.cha_firmano = firma.fir_sirano;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_firmano = firma.fir_sirano;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_firmano = firma.fir_sirano;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_firmano = firma.fir_sirano;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_firmano = firma.fir_sirano;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item7 in _BakimKabulHareketleri)
		{
			item7.bkmkb_firmano = firma.fir_sirano;
		}
		if (Parametreler.FirmaDegisinceSubeDegistir)
		{
			if (Parametreler.FirmaDegisinceFirmaNo0 == _firma.fir_sirano)
			{
				_SetSube(Parametreler.FirmaDegisinceSubeDegistirFirmaNo0);
			}
			if (Parametreler.FirmaDegisinceFirmaNo1 == _firma.fir_sirano)
			{
				_SetSube(Parametreler.FirmaDegisinceSubeDegistirFirmaNo1);
			}
			if (Parametreler.FirmaDegisinceFirmaNo2 == _firma.fir_sirano)
			{
				_SetSube(Parametreler.FirmaDegisinceSubeDegistirFirmaNo2);
			}
			if (Parametreler.FirmaDegisinceFirmaNo3 == _firma.fir_sirano)
			{
				_SetSube(Parametreler.FirmaDegisinceSubeDegistirFirmaNo3);
			}
		}
		if (Parametreler.FirmaDegisinceEvrakSeriDegistir)
		{
			if (Parametreler.FirmaDegisinceFirmaNo0 == _firma.fir_sirano)
			{
				_SetEvrakNoSeri(Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo0);
			}
			if (Parametreler.FirmaDegisinceFirmaNo1 == _firma.fir_sirano)
			{
				_SetEvrakNoSeri(Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo1);
			}
			if (Parametreler.FirmaDegisinceFirmaNo2 == _firma.fir_sirano)
			{
				_SetEvrakNoSeri(Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo2);
			}
			if (Parametreler.FirmaDegisinceFirmaNo3 == _firma.fir_sirano)
			{
				_SetEvrakNoSeri(Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo3);
			}
		}
		if (Parametreler.FirmaDegisinceDepoDegistir)
		{
			if (Parametreler.FirmaDegisinceFirmaNo0 == _firma.fir_sirano)
			{
				_SetKaynakDepo(Parametreler.FirmaDegisinceDepoDegistirFirmaNo0);
			}
			if (Parametreler.FirmaDegisinceFirmaNo1 == _firma.fir_sirano)
			{
				_SetKaynakDepo(Parametreler.FirmaDegisinceDepoDegistirFirmaNo1);
			}
			if (Parametreler.FirmaDegisinceFirmaNo2 == _firma.fir_sirano)
			{
				_SetKaynakDepo(Parametreler.FirmaDegisinceDepoDegistirFirmaNo2);
			}
			if (Parametreler.FirmaDegisinceFirmaNo3 == _firma.fir_sirano)
			{
				_SetKaynakDepo(Parametreler.FirmaDegisinceDepoDegistirFirmaNo3);
			}
		}
		if (Parametreler.FirmaDegisinceSorumlulukMerkeziDegistir)
		{
			if (Parametreler.FirmaDegisinceFirmaNo0 == _firma.fir_sirano)
			{
				_SetSorumlulukMerkezi(Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0);
			}
			if (Parametreler.FirmaDegisinceFirmaNo1 == _firma.fir_sirano)
			{
				_SetSorumlulukMerkezi(Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1);
			}
			if (Parametreler.FirmaDegisinceFirmaNo2 == _firma.fir_sirano)
			{
				_SetSorumlulukMerkezi(Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2);
			}
			if (Parametreler.FirmaDegisinceFirmaNo3 == _firma.fir_sirano)
			{
				_SetSorumlulukMerkezi(Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3);
			}
		}
		if (Parametreler.FirmaDegisinceProjeKoduDegistir)
		{
			if (Parametreler.FirmaDegisinceFirmaNo0 == _firma.fir_sirano)
			{
				_SetProje(Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo0);
			}
			if (Parametreler.FirmaDegisinceFirmaNo1 == _firma.fir_sirano)
			{
				_SetProje(Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo1);
			}
			if (Parametreler.FirmaDegisinceFirmaNo2 == _firma.fir_sirano)
			{
				_SetProje(Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo2);
			}
			if (Parametreler.FirmaDegisinceFirmaNo3 == _firma.fir_sirano)
			{
				_SetProje(Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo3);
			}
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetSube(Sube sube)
	{
		_SetSube(sube);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetEvrakNoSeri(string YeniDeger)
	{
		int num = 6;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_SetEvrakNoSeri(YeniDeger);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetEvraknoSira(int YeniEvraknoSira)
	{
		_evraknosira = YeniEvraknoSira;
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis || evraktipi == enum_GenelEvrakTipleri.ProformaSiparis || evraktipi == enum_GenelEvrakTipleri.VerilenSiparis)
		{
			foreach (SIPARISLER item in _Siparisler)
			{
				item.sip_evrakno_sira = YeniEvraknoSira;
			}
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_evrakno_sira = YeniEvraknoSira;
		}
		_StokCariHesapHareketi.cha_evrakno_sira = YeniEvraknoSira;
		_StokCariHesapHareketiIadeliSatis.cha_evrakno_sira = YeniEvraknoSira;
		_StokFiyatFarkiCariHesapHareketi.cha_evrakno_sira = YeniEvraknoSira;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_evrakno_sira = YeniEvraknoSira;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_evrakno_sira = YeniEvraknoSira;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_evrakno_sira = YeniEvraknoSira;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_evrakno_sira = YeniEvraknoSira;
		}
		foreach (SAYIM_SONUCLARI item7 in _SayimSonuclariHareketleri)
		{
			item7.sym_evrakno = YeniEvraknoSira;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item8 in _BakimKabulHareketleri)
		{
			item8.bkmkb_evrakno_sira = YeniEvraknoSira;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetBelgeNo(string YeniDeger)
	{
		int num = 20;
		if (YeniDeger.Length > num)
		{
			YeniDeger = YeniDeger.Substring(0, num);
		}
		_belgeno = YeniDeger;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_belgeno = _belgeno;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_belge_no = _belgeno;
		}
		_StokCariHesapHareketi.cha_belge_no = _belgeno;
		_StokCariHesapHareketiIadeliSatis.cha_belge_no = _belgeno;
		_StokFiyatFarkiCariHesapHareketi.cha_belge_no = _belgeno;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_belge_no = _belgeno;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_belge_no = _belgeno;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_belge_no = _belgeno;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_belgeno = _belgeno;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item7 in _BakimKabulHareketleri)
		{
			item7.bkmkb_belgeno = _belgeno;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetFiyatListesi(FiyatListesi fiyatlistesi)
	{
		_fiyatlistesi = fiyatlistesi;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetKaynakDepo(Depo YeniDepo)
	{
		_SetKaynakDepo(YeniDepo);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetHedefDepo(Depo YeniDepo)
	{
		_SetHedefDepo(YeniDepo);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetNakliyeDepo(Depo YeniDepo)
	{
		_SetNakliyeDepo(YeniDepo);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetSorumlulukMerkezi(SorumlulukMerkezi value)
	{
		_SetSorumlulukMerkezi(value);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetProje(Proje value)
	{
		_SetProje(value);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetStokHareketleri(List<STOK_HAREKETLERI> stok_hareketleri)
	{
		_StokHareketleri = stok_hareketleri;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetCari(Cari cari, int adresno, int odemeplani, string temsilcikodu, FiyatListesi fiyatlistesi, int dovizcinsi, Kur kur)
	{
		int num = 25;
		if (temsilcikodu.Length > num)
		{
			temsilcikodu = temsilcikodu.Substring(0, num);
		}
		_cari = cari;
		_sevkadresno = adresno;
		_odemeplani = odemeplani;
		_TemsilciKodu = temsilcikodu;
		_fiyatlistesi = fiyatlistesi;
		_dovizcinsi = dovizcinsi;
		_kur = kur;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetStokCariHesapHareketi(CARI_HESAP_HAREKETLERI hareket)
	{
		_StokCariHesapHareketi = hareket;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetStokCariHesapHareketiIadeliSatis(CARI_HESAP_HAREKETLERI hareket)
	{
		_StokCariHesapHareketiIadeliSatis = hareket;
		OnEvrakChanged(EventArgs.Empty);
	}

	public void SetStokFiyatFarkiCariHesapHareketi(CARI_HESAP_HAREKETLERI hareket)
	{
		_StokFiyatFarkiCariHesapHareketi = hareket;
		OnEvrakChanged(EventArgs.Empty);
	}

	public enum_evrak_satir_miktar_duzenleme_sonuc SetSatirMiktar(int Index, double yeni_miktar, int birim_pntr)
	{
		if (yeni_miktar <= 0.0)
		{
			return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarUygunDegil;
		}
		if (Parametreler.CekiListesi.Olustur && evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			double num = yeni_miktar - GetMiktar(Index);
			if (num < 0.0)
			{
				double aktifMiktar = Parametreler.CekiListesi.GetAktifMiktar(GetStokHareketleri()[Index].sth_stok_kod);
				if (num * -1.0 > aktifMiktar)
				{
					return enum_evrak_satir_miktar_duzenleme_sonuc.CekiListesiUygunDegil;
				}
			}
		}
		enum_evrak_satir_miktar_duzenleme_sonuc enum_evrak_satir_miktar_duzenleme_sonuc2 = enum_evrak_satir_miktar_duzenleme_sonuc.MiktarUygunDegil;
		enum_evrak_satir_miktar_duzenleme_sonuc2 = evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => SetSiparisSatirMiktar(Index, yeni_miktar, birim_pntr), 
			enum_GenelEvrakTipleri.ProformaSiparis => SetSiparisSatirMiktar(Index, yeni_miktar, birim_pntr), 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => SetDepolarArasiSiparisSatirMiktar(Index, yeni_miktar, birim_pntr), 
			enum_GenelEvrakTipleri.Tahsilat => SetTahsilatHareketleriSatirMiktar(Index, yeni_miktar), 
			enum_GenelEvrakTipleri.Tediye => SetTahsilatHareketleriSatirMiktar(Index, yeni_miktar), 
			enum_GenelEvrakTipleri.Masraf => SetTahsilatHareketleriSatirMiktar(Index, yeni_miktar), 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => SetSayimSonuclariSatirMiktar(Index, yeni_miktar, birim_pntr), 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama => enum_evrak_satir_miktar_duzenleme_sonuc.EvrakUygunDegil, 
			enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama => enum_evrak_satir_miktar_duzenleme_sonuc.EvrakUygunDegil, 
			_ => SetStokHareketiSatirMiktar(Index, yeni_miktar, birim_pntr), 
		};
		if (enum_evrak_satir_miktar_duzenleme_sonuc2 == enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi)
		{
			OnEvrakChanged(EventArgs.Empty);
		}
		return enum_evrak_satir_miktar_duzenleme_sonuc2;
	}

	private void _SetSube(Sube sube)
	{
		_sube = sube;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_subeno = sube.Sube_no;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_subeno = sube.Sube_no;
		}
		_StokCariHesapHareketi.cha_subeno = sube.Sube_no;
		_StokCariHesapHareketiIadeliSatis.cha_subeno = sube.Sube_no;
		_StokFiyatFarkiCariHesapHareketi.cha_subeno = sube.Sube_no;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_subeno = sube.Sube_no;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_subeno = sube.Sube_no;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_subeno = sube.Sube_no;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_subeno = sube.Sube_no;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item7 in _BakimKabulHareketleri)
		{
			item7.bkmkb_subeno = sube.Sube_no;
		}
	}

	private void _SetEvrakNoSeri(string YeniEvraknoSeri)
	{
		if (YeniEvraknoSeri.Length > 6)
		{
			YeniEvraknoSeri = YeniEvraknoSeri.Substring(0, 6);
		}
		_evraknoseri = YeniEvraknoSeri;
		if (evraktipi == enum_GenelEvrakTipleri.AlinanSiparis || evraktipi == enum_GenelEvrakTipleri.ProformaSiparis || evraktipi == enum_GenelEvrakTipleri.VerilenSiparis)
		{
			foreach (SIPARISLER item in _Siparisler)
			{
				item.sip_evrakno_seri = YeniEvraknoSeri;
			}
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_evrakno_seri = YeniEvraknoSeri;
		}
		_StokCariHesapHareketi.cha_evrakno_seri = YeniEvraknoSeri;
		_StokCariHesapHareketiIadeliSatis.cha_evrakno_seri = YeniEvraknoSeri;
		_StokFiyatFarkiCariHesapHareketi.cha_evrakno_seri = YeniEvraknoSeri;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_evrakno_seri = YeniEvraknoSeri;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_evrakno_seri = YeniEvraknoSeri;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_evrakno_seri = YeniEvraknoSeri;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_evrakno_seri = YeniEvraknoSeri;
		}
		foreach (SAYIM_SONUCLARI item7 in _SayimSonuclariHareketleri)
		{
			_ = item7;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item8 in _BakimKabulHareketleri)
		{
			item8.bkmkb_evrakno_seri = YeniEvraknoSeri;
		}
	}

	private void _SetKaynakDepo(Depo YeniDepo)
	{
		_kaynakdepo = YeniDepo;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_depono = YeniDepo.dep_no;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_cikis_depo_no = YeniDepo.dep_no;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item3 in _DepolarArasiSiparisHareketleri)
		{
			item3.ssip_cikdepo = YeniDepo.dep_no;
		}
		foreach (SAYIM_SONUCLARI item4 in _SayimSonuclariHareketleri)
		{
			item4.sym_depono = YeniDepo.dep_no;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item5 in _BakimKabulHareketleri)
		{
			item5.bkmkb_depono = YeniDepo.dep_no;
		}
	}

	private void _SetHedefDepo(Depo YeniDepo)
	{
		_hedefdepo = YeniDepo;
		foreach (DEPOLAR_ARASI_SIPARISLER item in _DepolarArasiSiparisHareketleri)
		{
			item.ssip_girdepo = YeniDepo.dep_no;
		}
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
			{
				item2.sth_giris_depo_no = YeniDepo.dep_no;
			}
		}
		if (evraktipi != enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return;
		}
		foreach (STOK_HAREKETLERI item3 in _StokHareketleri)
		{
			item3.sth_nakliyedeposu = YeniDepo.dep_no;
		}
	}

	private void _SetNakliyeDepo(Depo YeniDepo)
	{
		_nakliyedepo = YeniDepo;
		if (evraktipi != enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			return;
		}
		foreach (STOK_HAREKETLERI item in _StokHareketleri)
		{
			item.sth_giris_depo_no = YeniDepo.dep_no;
		}
	}

	private void _SetSorumlulukMerkezi(SorumlulukMerkezi value)
	{
		_sorumlulukmerkezi = value;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_cari_sormerk = value.som_kod;
			item.sip_stok_sormerk = value.som_kod;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_cari_srm_merkezi = value.som_kod;
			item2.sth_stok_srm_merkezi = value.som_kod;
		}
		_StokCariHesapHareketi.cha_srmrkkodu = value.som_kod;
		_StokCariHesapHareketi.cha_karsisrmrkkodu = value.som_kod;
		_StokCariHesapHareketiIadeliSatis.cha_srmrkkodu = value.som_kod;
		_StokCariHesapHareketiIadeliSatis.cha_karsisrmrkkodu = value.som_kod;
		_StokFiyatFarkiCariHesapHareketi.cha_srmrkkodu = value.som_kod;
		_StokFiyatFarkiCariHesapHareketi.cha_karsisrmrkkodu = value.som_kod;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_srmrkkodu = value.som_kod;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_srmrkkodu = value.som_kod;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_srmrkkodu = value.som_kod;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_sormerkezi = value.som_kod;
		}
	}

	private void _SetProje(Proje value)
	{
		_proje = value;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_projekodu = value.pro_kodu;
		}
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			item2.sth_proje_kodu = value.pro_kodu;
		}
		_StokCariHesapHareketi.cha_projekodu = value.pro_kodu;
		_StokCariHesapHareketiIadeliSatis.cha_projekodu = value.pro_kodu;
		_StokFiyatFarkiCariHesapHareketi.cha_projekodu = value.pro_kodu;
		foreach (CARI_HESAP_HAREKETLERI item3 in _HizmetHareketleri)
		{
			item3.cha_projekodu = value.pro_kodu;
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in _TahsilatHareketleri)
		{
			item4.cha_projekodu = value.pro_kodu;
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in _GenelCariHesapHareketleri)
		{
			item5.cha_projekodu = value.pro_kodu;
		}
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
		{
			item6.ssip_projekodu = value.pro_kodu;
		}
	}

	private enum_evrak_satir_miktar_duzenleme_sonuc SetDepolarArasiSiparisSatirMiktar(int Index, double yeni_miktar, int birim_pntr)
	{
		double ssip_miktar = _DepolarArasiSiparisHareketleri[Index].ssip_miktar;
		double num = yeni_miktar / ssip_miktar;
		_DepolarArasiSiparisHareketleri[Index].ssip_miktar = yeni_miktar;
		_DepolarArasiSiparisHareketleri[Index].ssip_tutar *= num;
		_DepolarArasiSiparisHareketleri[Index].ssip_birim_pntr = birim_pntr;
		return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi;
	}

	private enum_evrak_satir_miktar_duzenleme_sonuc SetSiparisSatirMiktar(int Index, double yeni_miktar, int birim_pntr)
	{
		double sip_miktar = _Siparisler[Index].sip_miktar;
		double num = yeni_miktar / sip_miktar;
		_Siparisler[Index].sip_miktar = yeni_miktar;
		_Siparisler[Index].sip_iskonto_1 *= num;
		_Siparisler[Index].sip_iskonto_2 *= num;
		_Siparisler[Index].sip_iskonto_3 *= num;
		_Siparisler[Index].sip_iskonto_4 *= num;
		_Siparisler[Index].sip_iskonto_5 *= num;
		_Siparisler[Index].sip_iskonto_6 *= num;
		_Siparisler[Index].sip_masraf_1 *= num;
		_Siparisler[Index].sip_masraf_2 *= num;
		_Siparisler[Index].sip_masraf_3 *= num;
		_Siparisler[Index].sip_masraf_4 *= num;
		_Siparisler[Index].sip_masvergi *= num;
		_Siparisler[Index].sip_Otv_Vergi *= num;
		_Siparisler[Index].sip_otvtutari *= num;
		_Siparisler[Index].sip_tutar *= num;
		_Siparisler[Index].sip_vergi *= num;
		_Siparisler[Index].sip_birim_pntr = birim_pntr;
		return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi;
	}

	private enum_evrak_satir_miktar_duzenleme_sonuc SetStokHareketiSatirMiktar(int Index, double yeni_miktar, int birim_pntr)
	{
		enum_evrak_satir_miktar_duzenleme_sonuc result = enum_evrak_satir_miktar_duzenleme_sonuc.MiktarUygunDegil;
		if (sipariskarsilamami)
		{
			if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
			{
				double sth_miktar = _StokHareketleri[Index].sth_miktar;
				double num = yeni_miktar - sth_miktar;
				double num2 = yeni_miktar / sth_miktar;
				foreach (DEPOLAR_ARASI_SIPARISLER item in _DepolarArasiSiparisHareketleri)
				{
					if (item.ssip_stok_kod == _StokHareketleri[Index].sth_stok_kod)
					{
						if (item.ssip_miktar - item.ssip_teslim_miktar >= num)
						{
							item.ssip_teslim_miktar += num;
							_StokHareketleri[Index].sth_miktar *= num2;
							_StokHareketleri[Index].sth_iskonto1 *= num2;
							_StokHareketleri[Index].sth_iskonto2 *= num2;
							_StokHareketleri[Index].sth_iskonto3 *= num2;
							_StokHareketleri[Index].sth_iskonto4 *= num2;
							_StokHareketleri[Index].sth_iskonto5 *= num2;
							_StokHareketleri[Index].sth_iskonto6 *= num2;
							_StokHareketleri[Index].sth_masraf_vergi *= num2;
							_StokHareketleri[Index].sth_masraf1 *= num2;
							_StokHareketleri[Index].sth_masraf2 *= num2;
							_StokHareketleri[Index].sth_masraf3 *= num2;
							_StokHareketleri[Index].sth_masraf4 *= num2;
							_StokHareketleri[Index].sth_oiv_vergi *= num2;
							_StokHareketleri[Index].sth_oivtutari *= num2;
							_StokHareketleri[Index].sth_otv_vergi *= num2;
							_StokHareketleri[Index].sth_otvtutari *= num2;
							_StokHareketleri[Index].sth_tutar *= num2;
							_StokHareketleri[Index].sth_vergi *= num2;
							return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi;
						}
						return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarUygunDegil;
					}
				}
			}
			else
			{
				double sth_miktar2 = _StokHareketleri[Index].sth_miktar;
				double num3 = yeni_miktar - sth_miktar2;
				double num4 = yeni_miktar / sth_miktar2;
				foreach (SIPARISLER item2 in _Siparisler)
				{
					if (!(item2.sip_stok_kod == _StokHareketleri[Index].sth_stok_kod))
					{
						continue;
					}
					if (item2.sip_miktar - item2.sip_teslim_miktar >= num3)
					{
						item2.sip_teslim_miktar += num3;
						_StokHareketleri[Index].sth_miktar *= num4;
						_StokHareketleri[Index].sth_iskonto1 *= num4;
						_StokHareketleri[Index].sth_iskonto2 *= num4;
						_StokHareketleri[Index].sth_iskonto3 *= num4;
						_StokHareketleri[Index].sth_iskonto4 *= num4;
						_StokHareketleri[Index].sth_iskonto5 *= num4;
						_StokHareketleri[Index].sth_iskonto6 *= num4;
						_StokHareketleri[Index].sth_masraf_vergi *= num4;
						_StokHareketleri[Index].sth_masraf1 *= num4;
						_StokHareketleri[Index].sth_masraf2 *= num4;
						_StokHareketleri[Index].sth_masraf3 *= num4;
						_StokHareketleri[Index].sth_masraf4 *= num4;
						_StokHareketleri[Index].sth_oiv_vergi *= num4;
						_StokHareketleri[Index].sth_oivtutari *= num4;
						_StokHareketleri[Index].sth_otv_vergi *= num4;
						_StokHareketleri[Index].sth_otvtutari *= num4;
						_StokHareketleri[Index].sth_tutar *= num4;
						_StokHareketleri[Index].sth_vergi *= num4;
						if (num3 > 0.0)
						{
							AddUrunCekiListesi(_StokHareketleri[Index].sth_stok_kod, num3, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
						}
						else
						{
							DeleteCekiListesiUrun(_StokHareketleri[Index].sth_stok_kod, num3 * -1.0, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
						}
						return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi;
					}
					return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarUygunDegil;
				}
			}
			return result;
		}
		double sth_miktar3 = _StokHareketleri[Index].sth_miktar;
		double num5 = yeni_miktar - sth_miktar3;
		double num6 = yeni_miktar / sth_miktar3;
		_StokHareketleri[Index].sth_miktar *= num6;
		_StokHareketleri[Index].sth_iskonto1 *= num6;
		_StokHareketleri[Index].sth_iskonto2 *= num6;
		_StokHareketleri[Index].sth_iskonto3 *= num6;
		_StokHareketleri[Index].sth_iskonto4 *= num6;
		_StokHareketleri[Index].sth_iskonto5 *= num6;
		_StokHareketleri[Index].sth_iskonto6 *= num6;
		_StokHareketleri[Index].sth_masraf_vergi *= num6;
		_StokHareketleri[Index].sth_masraf1 *= num6;
		_StokHareketleri[Index].sth_masraf2 *= num6;
		_StokHareketleri[Index].sth_masraf3 *= num6;
		_StokHareketleri[Index].sth_masraf4 *= num6;
		_StokHareketleri[Index].sth_oiv_vergi *= num6;
		_StokHareketleri[Index].sth_oivtutari *= num6;
		_StokHareketleri[Index].sth_otv_vergi *= num6;
		_StokHareketleri[Index].sth_otvtutari *= num6;
		_StokHareketleri[Index].sth_tutar *= num6;
		_StokHareketleri[Index].sth_vergi *= num6;
		_StokHareketleri[Index].sth_birim_pntr = birim_pntr;
		if (num5 > 0.0)
		{
			AddUrunCekiListesi(_StokHareketleri[Index].sth_stok_kod, num5, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
		}
		else
		{
			DeleteCekiListesiUrun(_StokHareketleri[Index].sth_stok_kod, num5 * -1.0, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
		}
		return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi;
	}

	private enum_evrak_satir_miktar_duzenleme_sonuc SetTahsilatHareketleriSatirMiktar(int Index, double yeni_miktar)
	{
		double cha_meblag = _TahsilatHareketleri[Index].cha_meblag;
		double num = yeni_miktar / cha_meblag;
		_TahsilatHareketleri[Index].cha_meblag = yeni_miktar;
		_TahsilatHareketleri[Index].cha_aratoplam *= num;
		_TahsilatHareketleri[Index].cha_ft_iskonto1 *= num;
		_TahsilatHareketleri[Index].cha_ft_iskonto2 *= num;
		_TahsilatHareketleri[Index].cha_ft_iskonto3 *= num;
		_TahsilatHareketleri[Index].cha_ft_iskonto4 *= num;
		_TahsilatHareketleri[Index].cha_ft_iskonto5 *= num;
		_TahsilatHareketleri[Index].cha_ft_iskonto6 *= num;
		_TahsilatHareketleri[Index].cha_ft_masraf1 *= num;
		_TahsilatHareketleri[Index].cha_ft_masraf2 *= num;
		_TahsilatHareketleri[Index].cha_ft_masraf3 *= num;
		_TahsilatHareketleri[Index].cha_ft_masraf4 *= num;
		_TahsilatHareketleri[Index].cha_oiv_vergi *= num;
		_TahsilatHareketleri[Index].cha_oivtutari *= num;
		_TahsilatHareketleri[Index].cha_otvtutari *= num;
		_TahsilatHareketleri[Index].cha_vergi1 *= num;
		_TahsilatHareketleri[Index].cha_vergi2 *= num;
		_TahsilatHareketleri[Index].cha_vergi3 *= num;
		_TahsilatHareketleri[Index].cha_vergi4 *= num;
		_TahsilatHareketleri[Index].cha_vergi5 *= num;
		_TahsilatHareketleri[Index].cha_vergi6 *= num;
		_TahsilatHareketleri[Index].cha_vergi7 *= num;
		_TahsilatHareketleri[Index].cha_vergi8 *= num;
		_TahsilatHareketleri[Index].cha_vergi9 *= num;
		_TahsilatHareketleri[Index].cha_vergi10 *= num;
		return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi;
	}

	private enum_evrak_satir_miktar_duzenleme_sonuc SetSayimSonuclariSatirMiktar(int Index, double yeni_miktar, int birim_pntr)
	{
		double sym_miktar = _SayimSonuclariHareketleri[Index].sym_miktar1;
		_ = yeni_miktar / sym_miktar;
		_SayimSonuclariHareketleri[Index].sym_miktar1 = yeni_miktar;
		_SayimSonuclariHareketleri[Index].sym_birim_pntr = birim_pntr;
		return enum_evrak_satir_miktar_duzenleme_sonuc.MiktarDuzenlendi;
	}

	public void DeleteKdv()
	{
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			foreach (SIPARISLER item in _Siparisler)
			{
				item.sip_vergi = 0.0;
				item.sip_vergi_pntr = 0;
				item.sip_masvergi = 0.0;
				item.sip_masvergi_pntr = 0;
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			foreach (SIPARISLER item2 in _Siparisler)
			{
				item2.sip_vergi = 0.0;
				item2.sip_vergi_pntr = 0;
				item2.sip_masvergi = 0.0;
				item2.sip_masvergi_pntr = 0;
			}
			break;
		default:
			foreach (STOK_HAREKETLERI item3 in _StokHareketleri)
			{
				item3.sth_vergi = 0.0;
				item3.sth_vergi_pntr = 0;
				item3.sth_masraf_vergi = 0.0;
				item3.sth_masraf_vergi_pntr = 0;
			}
			break;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public bool DeleteSatir(int Index)
	{
		bool flag = false;
		if (Parametreler.CekiListesi.Olustur && evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			double miktar = GetMiktar(Index);
			double aktifMiktar = Parametreler.CekiListesi.GetAktifMiktar(GetStokHareketleri()[Index].sth_stok_kod);
			if (miktar > aktifMiktar)
			{
				return false;
			}
		}
		flag = evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => DeleteSiparisSatir(Index), 
			enum_GenelEvrakTipleri.ProformaSiparis => DeleteSiparisSatir(Index), 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => DeleteDepolarArasiSiparisSatir(Index), 
			enum_GenelEvrakTipleri.Tahsilat => DeleteTahsilatHareketleriSatir(Index), 
			enum_GenelEvrakTipleri.Tediye => DeleteTahsilatHareketleriSatir(Index), 
			enum_GenelEvrakTipleri.Masraf => DeleteTahsilatHareketleriSatir(Index), 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => DeleteSayimSonuclariSatir(Index), 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama => DeleteDepolarArasiNakliyeOnaylamaSatir(Index), 
			enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama => DeleteBarkodKontrolluFaturaOnaylamaSatir(Index), 
			_ => DeleteStokHareketiSatir(Index), 
		};
		OnEvrakChanged(EventArgs.Empty);
		return flag;
	}

	private bool DeleteDepolarArasiSiparisSatir(int Index)
	{
		bool flag = false;
		int num = 0;
		_DepolarArasiSiparisHareketleri.RemoveAt(Index);
		flag = true;
		num = 0;
		foreach (DEPOLAR_ARASI_SIPARISLER item in _DepolarArasiSiparisHareketleri)
		{
			item.ssip_satirno = num;
			num++;
		}
		return flag;
	}

	private bool DeleteSiparisSatir(int Index)
	{
		bool flag = false;
		int num = 0;
		_Siparisler.RemoveAt(Index);
		flag = true;
		num = 0;
		foreach (SIPARISLER item in _Siparisler)
		{
			item.sip_satirno = num;
			num++;
		}
		return flag;
	}

	private bool DeleteDepolarArasiNakliyeOnaylamaSatir(int Index)
	{
		_StokHareketleri[Index].OkutulanMiktar = 0.0;
		return false;
	}

	private bool DeleteBarkodKontrolluFaturaOnaylamaSatir(int Index)
	{
		_StokHareketleri[Index].OkutulanMiktar = 0.0;
		return false;
	}

	private bool DeleteStokHareketiSatir(int Index)
	{
		bool flag = false;
		if (sipariskarsilamami)
		{
			STOK_HAREKETLERI sTOK_HAREKETLERI = _StokHareketleri[Index];
			if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
			{
				double num = sTOK_HAREKETLERI.sth_miktar;
				double num2 = 0.0;
				foreach (DEPOLAR_ARASI_SIPARISLER item in _DepolarArasiSiparisHareketleri)
				{
					if (item.ssip_stok_kod == sTOK_HAREKETLERI.sth_stok_kod)
					{
						if (item.ssip_teslim_miktar >= num)
						{
							num2 = num;
							num = 0.0;
						}
						else
						{
							num2 = item.ssip_teslim_miktar;
							num -= item.ssip_teslim_miktar;
						}
						item.ssip_teslim_miktar -= num2;
						if (num == 0.0)
						{
							break;
						}
					}
				}
				foreach (BEDEN_HAREKETLERI item2 in sTOK_HAREKETLERI.renk_beden_hareketleri)
				{
					_ = item2.BdnHar_BedenNo;
					double num3 = item2.BdnHar_HarGor;
					double num4 = 0.0;
					foreach (DEPOLAR_ARASI_SIPARISLER item3 in _DepolarArasiSiparisHareketleri)
					{
						if (item3.ssip_stok_kod == sTOK_HAREKETLERI.sth_stok_kod)
						{
							foreach (BEDEN_HAREKETLERI item4 in item3.renk_beden_hareketleri)
							{
								if (item2.BdnHar_BedenNo == item4.BdnHar_BedenNo)
								{
									if (item4.BdnHar_TesMik >= num3)
									{
										num4 = num3;
										num3 = 0.0;
									}
									else
									{
										num4 = item4.BdnHar_TesMik;
										num3 -= item4.BdnHar_TesMik;
									}
									item4.BdnHar_TesMik -= num4;
								}
							}
						}
						if (num3 == 0.0)
						{
							break;
						}
					}
				}
			}
			else
			{
				double num5 = sTOK_HAREKETLERI.sth_miktar;
				double num6 = 0.0;
				foreach (SIPARISLER item5 in _Siparisler)
				{
					if (item5.sip_stok_kod == sTOK_HAREKETLERI.sth_stok_kod)
					{
						if (item5.sip_teslim_miktar >= num5)
						{
							num6 = num5;
							num5 = 0.0;
						}
						else
						{
							num6 = item5.sip_teslim_miktar;
							num5 -= item5.sip_teslim_miktar;
						}
						item5.sip_teslim_miktar -= num6;
						if (num5 == 0.0)
						{
							break;
						}
					}
				}
				foreach (BEDEN_HAREKETLERI item6 in sTOK_HAREKETLERI.renk_beden_hareketleri)
				{
					_ = item6.BdnHar_BedenNo;
					double num7 = item6.BdnHar_HarGor;
					double num8 = 0.0;
					foreach (SIPARISLER item7 in _Siparisler)
					{
						if (item7.sip_stok_kod == sTOK_HAREKETLERI.sth_stok_kod)
						{
							foreach (BEDEN_HAREKETLERI item8 in item7.renk_beden_hareketleri)
							{
								if (item6.BdnHar_BedenNo == item8.BdnHar_BedenNo)
								{
									if (item8.BdnHar_TesMik >= num7)
									{
										num8 = num7;
										num7 = 0.0;
									}
									else
									{
										num8 = item8.BdnHar_TesMik;
										num7 -= item8.BdnHar_TesMik;
									}
									item8.BdnHar_TesMik -= num8;
								}
							}
						}
						if (num7 == 0.0)
						{
							break;
						}
					}
				}
			}
		}
		DeleteCekiListesiUrun(_StokHareketleri[Index].sth_stok_kod, _StokHareketleri[Index].sth_miktar, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
		_StokHareketleri.RemoveAt(Index);
		flag = true;
		int num9 = 0;
		foreach (STOK_HAREKETLERI item9 in _StokHareketleri)
		{
			item9.sth_satirno = num9;
			num9++;
		}
		return flag;
	}

	private bool DeleteTahsilatHareketleriSatir(int Index)
	{
		bool result = false;
		_TahsilatHareketleri.RemoveAt(Index);
		int num = 0;
		foreach (CARI_HESAP_HAREKETLERI item in _TahsilatHareketleri)
		{
			item.cha_satir_no = num;
			num++;
		}
		return result;
	}

	private bool DeleteSayimSonuclariSatir(int Index)
	{
		bool result = false;
		_SayimSonuclariHareketleri.RemoveAt(Index);
		int num = 0;
		foreach (SAYIM_SONUCLARI item in _SayimSonuclariHareketleri)
		{
			item.sym_satirno = num;
			num++;
		}
		return result;
	}

	private bool DeleteMasrafHareketleriSatir(int Index)
	{
		bool result = false;
		_GenelCariHesapHareketleri.RemoveAt(Index);
		int num = 0;
		foreach (CARI_HESAP_HAREKETLERI item in _GenelCariHesapHareketleri)
		{
			item.cha_satir_no = num;
			num++;
		}
		return result;
	}

	private void DeleteCekiListesiUrun(string stok_kodu, double cikartilacak_miktar, int Ckl_BedenPntr, int Ckl_AnaAmbalajNo, int Ckl_AltAmbalajNo)
	{
		if (!Parametreler.CekiListesi.Olustur || evraktipi != enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return;
		}
		foreach (CEKI_LISTESI item in CekiListesi)
		{
			if (item.Ckl_StokKodu == stok_kodu && item.Ckl_BedenPntr == Ckl_BedenPntr && item.Ckl_AnaAmbalajNo == Ckl_AnaAmbalajNo && item.Ckl_AltAmbalajNo == Ckl_AltAmbalajNo)
			{
				if (!(item.Ckl_Miktari < cikartilacak_miktar))
				{
					item.Ckl_Miktari -= cikartilacak_miktar;
					break;
				}
				cikartilacak_miktar -= item.Ckl_Miktari;
				item.Ckl_Miktari = 0.0;
			}
		}
		int num = 0;
		List<int> list = new List<int>();
		foreach (CEKI_LISTESI item2 in CekiListesi)
		{
			if (item2.Ckl_Miktari == 0.0)
			{
				list.Add(num);
			}
			num++;
		}
		list.Reverse();
		foreach (int item3 in list)
		{
			CekiListesi.RemoveAt(item3);
		}
	}

	public enum_evrak_siparis_ekleme_sonuc AddUrun(Stok stok)
	{
		enum_evrak_siparis_ekleme_sonuc result = AddUrunMain(stok);
		OnEvrakChanged(EventArgs.Empty);
		return result;
	}

	public enum_evrak_siparis_ekleme_sonuc AddUrun(List<Stok> stoklist, bool BarkodOkuma)
	{
		enum_evrak_siparis_ekleme_sonuc enum_evrak_siparis_ekleme_sonuc2 = enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
		foreach (Stok item in stoklist)
		{
			enum_evrak_siparis_ekleme_sonuc enum_evrak_siparis_ekleme_sonuc3 = AddUrunMain(item);
			if (enum_evrak_siparis_ekleme_sonuc3 != enum_evrak_siparis_ekleme_sonuc.UrunEklendi)
			{
				enum_evrak_siparis_ekleme_sonuc2 = enum_evrak_siparis_ekleme_sonuc3;
				break;
			}
		}
		if (BarkodOkuma)
		{
			if (enum_evrak_siparis_ekleme_sonuc2 == enum_evrak_siparis_ekleme_sonuc.UrunEklendi)
			{
				Parametreler.BarkodOkumaSayisiBasarili++;
			}
			else
			{
				Parametreler.BarkodOkumaSayisiBasarisiz++;
			}
		}
		OnEvrakChanged(EventArgs.Empty);
		return enum_evrak_siparis_ekleme_sonuc2;
	}

	public void AddBakimKabulHareketi(BAKIM_KABUL_HAREKETLERI hareket)
	{
		_BakimKabulHareketleri.Add(hareket);
		OnEvrakChanged(EventArgs.Empty);
	}

	public int AddHizmet(Hizmet hizmet, string ProjeKodu, string SorumlulukMerkeziKodu)
	{
		CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
			cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.AlisFaturasi;
		}
		else
		{
			cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Borc;
			cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.SatisFaturasi;
		}
		cARI_HESAP_HAREKETLERI.cha_RECid_DBCno = _DBCno;
		cARI_HESAP_HAREKETLERI.cha_normal_Iade = normaliade;
		cARI_HESAP_HAREKETLERI.cha_ticaret_turu = ticaretturu;
		cARI_HESAP_HAREKETLERI.cha_create_user = mikrouserno;
		cARI_HESAP_HAREKETLERI.cha_create_date = DateTime.Now;
		cARI_HESAP_HAREKETLERI.cha_lastup_user = mikrouserno;
		cARI_HESAP_HAREKETLERI.cha_lastup_date = DateTime.Now;
		cARI_HESAP_HAREKETLERI.cha_firmano = Firma.fir_sirano;
		cARI_HESAP_HAREKETLERI.cha_subeno = Sube.Sube_no;
		cARI_HESAP_HAREKETLERI.cha_tarihi = _evraktarih;
		cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.HizmetFaturasi;
		cARI_HESAP_HAREKETLERI.cha_satir_no = _HizmetHareketleri.Count;
		cARI_HESAP_HAREKETLERI.cha_evrakno_seri = EvrakNoSeri;
		cARI_HESAP_HAREKETLERI.cha_evrakno_sira = EvrakNoSira;
		cARI_HESAP_HAREKETLERI.cha_belge_no = BelgeNo;
		cARI_HESAP_HAREKETLERI.cha_belge_tarih = _belgetarih;
		cARI_HESAP_HAREKETLERI.cha_cari_cins = enum_cha_cari_cins.Carimiz;
		cARI_HESAP_HAREKETLERI.cha_kod = cari.cari_kod;
		cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = enum_cha_kasa_hizmet.Hizmetimiz;
		cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = hizmet.hiz_kod;
		cARI_HESAP_HAREKETLERI.cha_d_kurtar = kur.dov_tarih;
		cARI_HESAP_HAREKETLERI.cha_d_cins = kur.dov_no;
		cARI_HESAP_HAREKETLERI.cha_d_kur = kur.dov_fiyat;
		cARI_HESAP_HAREKETLERI.cha_altd_kur = alternatifdovizkuru.dov_fiyat;
		cARI_HESAP_HAREKETLERI.cha_grupno = Getchagrupno();
		cARI_HESAP_HAREKETLERI.cha_vade = odemeplani;
		cARI_HESAP_HAREKETLERI.cha_fis_tarih = new DateTime(1900, 1, 1);
		cARI_HESAP_HAREKETLERI.cha_fis_sirano = 0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = hizmet.BirimFiyat.Iskonto_1_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = hizmet.BirimFiyat.Iskonto_2_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = hizmet.BirimFiyat.Iskonto_3_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = hizmet.BirimFiyat.Iskonto_4_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = hizmet.BirimFiyat.Iskonto_5_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = hizmet.BirimFiyat.Iskonto_6_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = hizmet.BirimFiyat.Masraf_1_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = hizmet.BirimFiyat.Masraf_2_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = hizmet.BirimFiyat.Masraf_3_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = hizmet.BirimFiyat.Masraf_4_Tutari * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_vergi1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi7 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi8 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi9 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi10 = 0.0;
		switch (hizmet.hiz_KDV)
		{
		case 2:
			cARI_HESAP_HAREKETLERI.cha_vergi2 = hizmet.KdvTutariToplamNetMasrafsiz(Parametreler.vergitanimlari);
			break;
		case 3:
			cARI_HESAP_HAREKETLERI.cha_vergi3 = hizmet.KdvTutariToplamNetMasrafsiz(Parametreler.vergitanimlari);
			break;
		case 4:
			cARI_HESAP_HAREKETLERI.cha_vergi4 = hizmet.KdvTutariToplamNetMasrafsiz(Parametreler.vergitanimlari);
			break;
		case 5:
			cARI_HESAP_HAREKETLERI.cha_vergi5 = hizmet.KdvTutariToplamNetMasrafsiz(Parametreler.vergitanimlari);
			break;
		default:
			cARI_HESAP_HAREKETLERI.cha_vergi1 = hizmet.KdvTutariToplamNetMasrafsiz(Parametreler.vergitanimlari);
			break;
		}
		cARI_HESAP_HAREKETLERI.cha_yuvarlama = 0.0;
		if (kapamasekli == enum_KapamaSekli.AcikHesap)
		{
			cARI_HESAP_HAREKETLERI.cha_tpoz = enum_cha_tpoz.Acik;
		}
		else
		{
			cARI_HESAP_HAREKETLERI.cha_tpoz = enum_cha_tpoz.Kapali;
			cARI_HESAP_HAREKETLERI.cha_kod = kapamahesapkodu;
			switch (kapamasekli)
			{
			case enum_KapamaSekli.BankadanKapanacak:
				cARI_HESAP_HAREKETLERI.cha_cari_cins = enum_cha_cari_cins.Bankamiz;
				break;
			case enum_KapamaSekli.CariPersoneldenKapanacak:
				cARI_HESAP_HAREKETLERI.cha_cari_cins = enum_cha_cari_cins.CariPersonelimiz;
				break;
			case enum_KapamaSekli.KasadanKapanacak:
				cARI_HESAP_HAREKETLERI.cha_cari_cins = enum_cha_cari_cins.Kasamiz;
				break;
			}
		}
		cARI_HESAP_HAREKETLERI.cha_aciklama = hizmet.Aciklama;
		cARI_HESAP_HAREKETLERI.cha_trefno = "";
		cARI_HESAP_HAREKETLERI.cha_sntck_poz = enum_cha_sntck_poz.Portfoyde;
		cARI_HESAP_HAREKETLERI.cha_karsidcinsi = 0;
		cARI_HESAP_HAREKETLERI.cha_karsid_kur = 1.0;
		cARI_HESAP_HAREKETLERI.cha_karsidgrupno = 0;
		cARI_HESAP_HAREKETLERI.cha_srmrkkodu = sorumlulukmerkezi.som_kod;
		cARI_HESAP_HAREKETLERI.cha_reftarihi = new DateTime(1899, 12, 30);
		cARI_HESAP_HAREKETLERI.cha_odeme_arr1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr7 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr8 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_miktari = hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_aratoplam = hizmet.BirimFiyat.FiyatBrut * hizmet.Miktar;
		cARI_HESAP_HAREKETLERI.cha_vergipntr = hizmet.hiz_KDV;
		cARI_HESAP_HAREKETLERI.cha_istisnakodu = 0;
		cARI_HESAP_HAREKETLERI.cha_ver_tev_carpani = 0.0;
		cARI_HESAP_HAREKETLERI.cha_stopaj = 0.0;
		cARI_HESAP_HAREKETLERI.cha_savsandesfonu = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_satici_kodu = TemsilciKodu;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_borsa = 0.0;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_bagkur = 0.0;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_diger = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalMSDF = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliye = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalStopaj = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalKomisyonu = 0.0;
		cARI_HESAP_HAREKETLERI.cha_StFonPntr = 0;
		cARI_HESAP_HAREKETLERI.cha_pos_hareketi = false;
		cARI_HESAP_HAREKETLERI.cha_vardiya_tarihi = new DateTime(1900, 1, 1);
		cARI_HESAP_HAREKETLERI.cha_vardiya_no = 0;
		cARI_HESAP_HAREKETLERI.cha_vardiya_evrak_ti = enum_cha_vardita_evrak_ti.HizmetSatisi;
		cARI_HESAP_HAREKETLERI.cha_HalRusum = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalNavlunTut = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalRehinFuture = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalKomisyon = 0.0;
		cARI_HESAP_HAREKETLERI.cha_Vade_Farki_Yuz = 0.0;
		cARI_HESAP_HAREKETLERI.cha_karsisrmrkkodu = SorumlulukMerkeziKodu;
		cARI_HESAP_HAREKETLERI.cha_EXIMkodu = "";
		cARI_HESAP_HAREKETLERI.cha_HalRehinSandikmiktari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikVrMiktar = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikKDVTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalrehinSandikTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_Tevkifat_turu = enum_cha_Tevkifat_turu.TevkifatYok;
		cARI_HESAP_HAREKETLERI.cha_otvtutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_otvvergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_projekodu = ProjeKodu;
		cARI_HESAP_HAREKETLERI.cha_sozlesme_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_sozlesme_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_yat_tes_kodu = "";
		cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = cari.cari_kod;
		cARI_HESAP_HAREKETLERI.cha_oivergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_meblag_ana_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_meblag_alt_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_meblag_orj_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_ciroprim_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_ciroprim_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliyeKdv = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliyeVergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_bakimhar_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_bakimhar_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_avanstalep_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_avanstalep_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_oiv_pntr = 0;
		cARI_HESAP_HAREKETLERI.cha_oiv_vergi = 0.0;
		cARI_HESAP_HAREKETLERI.cha_oivtutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas1 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas2 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas3 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas4 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas5 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas6 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas7 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas8 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas9 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas10 = 0;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas1 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas2 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas3 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas4 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas5 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas6 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas7 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas8 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas9 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas10 = false;
		cARI_HESAP_HAREKETLERI.cha_meblag = cARI_HESAP_HAREKETLERI.cha_aratoplam - cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 - cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 - cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 - cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 - cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 - cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 + (cARI_HESAP_HAREKETLERI.cha_ft_masraf1 + cARI_HESAP_HAREKETLERI.cha_ft_masraf2 + cARI_HESAP_HAREKETLERI.cha_ft_masraf3 + cARI_HESAP_HAREKETLERI.cha_ft_masraf4) + (cARI_HESAP_HAREKETLERI.cha_vergi1 + cARI_HESAP_HAREKETLERI.cha_vergi2 + cARI_HESAP_HAREKETLERI.cha_vergi3 + cARI_HESAP_HAREKETLERI.cha_vergi4 + cARI_HESAP_HAREKETLERI.cha_vergi5 + cARI_HESAP_HAREKETLERI.cha_vergi6 + cARI_HESAP_HAREKETLERI.cha_vergi7 + cARI_HESAP_HAREKETLERI.cha_vergi8 + cARI_HESAP_HAREKETLERI.cha_vergi9 + cARI_HESAP_HAREKETLERI.cha_vergi10 + cARI_HESAP_HAREKETLERI.cha_otvtutari + cARI_HESAP_HAREKETLERI.cha_oivtutari);
		_HizmetHareketleri.Add(cARI_HESAP_HAREKETLERI);
		OnEvrakChanged(EventArgs.Empty);
		return 1;
	}

	public int AddTahsilat(Tahsilat tahsilat)
	{
		string text = "";
		if (tahsilat.sck_bankano == "")
		{
			tahsilat.sck_borclu = _cari.cari_unvan1 + " " + _cari.cari_unvan2;
			text = tahsilat.sck_borclu;
		}
		else
		{
			tahsilat.sck_borclu = "";
			text = tahsilat.sck_bankano;
		}
		if (tahsilat.aciklama == "" && tahsilat.cinsi == enum_tahsilat_cinsi.MusteriCeki)
		{
			string text2 = "";
			text2 = tahsilat.sck_banka_adres1;
			if (text2.Length > 8)
			{
				text2 = text2.Substring(0, 8);
			}
			if (text.Length > 23)
			{
				text = text.Substring(0, 23);
			}
			tahsilat.aciklama = tahsilat.sck_no + "/" + text2 + "/" + text;
		}
		CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
		cARI_HESAP_HAREKETLERI.cha_RECid_DBCno = _DBCno;
		if (_evraktipi == enum_GenelEvrakTipleri.Tahsilat)
		{
			cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
			cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.TahsilatMakbuzu;
		}
		else
		{
			cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Borc;
			cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.TediyeMakbuzu;
		}
		cARI_HESAP_HAREKETLERI.cha_normal_Iade = enum_cha_normal_Iade.Normal;
		cARI_HESAP_HAREKETLERI.cha_ticaret_turu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
		cARI_HESAP_HAREKETLERI.cha_create_user = mikrouserno;
		cARI_HESAP_HAREKETLERI.cha_create_date = DateTime.Now;
		cARI_HESAP_HAREKETLERI.cha_lastup_user = mikrouserno;
		cARI_HESAP_HAREKETLERI.cha_lastup_date = DateTime.Now;
		cARI_HESAP_HAREKETLERI.cha_firmano = Firma.fir_sirano;
		cARI_HESAP_HAREKETLERI.cha_subeno = Sube.Sube_no;
		cARI_HESAP_HAREKETLERI.cha_tarihi = _evraktarih;
		switch (tahsilat.cinsi)
		{
		case enum_tahsilat_cinsi.Nakit:
			cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.Nakit;
			break;
		case enum_tahsilat_cinsi.MusteriCeki:
			cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.MusteriCeki;
			break;
		case enum_tahsilat_cinsi.MusteriSenedi:
			cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.MusteriSenedi;
			break;
		case enum_tahsilat_cinsi.MusteriKrediKarti:
			cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.MusteriKrediKarti;
			break;
		}
		cARI_HESAP_HAREKETLERI.cha_satir_no = _TahsilatHareketleri.Count;
		cARI_HESAP_HAREKETLERI.cha_evrakno_seri = EvrakNoSeri;
		cARI_HESAP_HAREKETLERI.cha_evrakno_sira = EvrakNoSira;
		cARI_HESAP_HAREKETLERI.cha_belge_no = BelgeNo;
		cARI_HESAP_HAREKETLERI.cha_belge_tarih = _belgetarih;
		cARI_HESAP_HAREKETLERI.cha_cari_cins = enum_cha_cari_cins.Carimiz;
		cARI_HESAP_HAREKETLERI.cha_kod = cari.cari_kod;
		enum_tahsilat_cinsi cinsi = tahsilat.cinsi;
		if (cinsi == enum_tahsilat_cinsi.MusteriKrediKarti)
		{
			cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = enum_cha_kasa_hizmet.Bankamiz;
		}
		else
		{
			cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = enum_cha_kasa_hizmet.Kasamiz;
		}
		cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = tahsilat.kasa_banka_kodu;
		cARI_HESAP_HAREKETLERI.cha_d_kurtar = kur.dov_tarih;
		cARI_HESAP_HAREKETLERI.cha_d_cins = kur.dov_no;
		cARI_HESAP_HAREKETLERI.cha_d_kur = kur.dov_fiyat;
		cARI_HESAP_HAREKETLERI.cha_altd_kur = alternatifdovizkuru.dov_fiyat;
		cARI_HESAP_HAREKETLERI.cha_grupno = Getchagrupno();
		cARI_HESAP_HAREKETLERI.cha_meblag = tahsilat.tutar;
		string text3 = tahsilat.vadesi.Year.ToString();
		string text4 = tahsilat.vadesi.Month.ToString();
		if (text4.Length == 1)
		{
			text4 = "0" + text4;
		}
		string text5 = tahsilat.vadesi.Day.ToString();
		if (text5.Length == 1)
		{
			text5 = "0" + text5;
		}
		cARI_HESAP_HAREKETLERI.cha_vade = int.Parse(text3 + text4 + text5);
		cARI_HESAP_HAREKETLERI.cha_fis_tarih = new DateTime(1900, 1, 1);
		cARI_HESAP_HAREKETLERI.cha_fis_sirano = 0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi7 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi8 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi9 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi10 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_yuvarlama = 0.0;
		cARI_HESAP_HAREKETLERI.cha_tpoz = enum_cha_tpoz.Acik;
		cARI_HESAP_HAREKETLERI.cha_aciklama = tahsilat.aciklama;
		cARI_HESAP_HAREKETLERI.cha_trefno = "";
		cinsi = tahsilat.cinsi;
		if (cinsi == enum_tahsilat_cinsi.MusteriKrediKarti)
		{
			cARI_HESAP_HAREKETLERI.cha_sntck_poz = enum_cha_sntck_poz.Tahsilde;
		}
		else
		{
			cARI_HESAP_HAREKETLERI.cha_sntck_poz = enum_cha_sntck_poz.Portfoyde;
		}
		cARI_HESAP_HAREKETLERI.cha_karsidcinsi = tahsilat.kasabanka_dovizcinsi;
		cARI_HESAP_HAREKETLERI.cha_karsid_kur = tahsilat.kasabanka_dovizcinsi_kur.dov_fiyat;
		cinsi = tahsilat.cinsi;
		if (cinsi == enum_tahsilat_cinsi.MusteriKrediKarti)
		{
			cARI_HESAP_HAREKETLERI.cha_karsidgrupno = 7;
		}
		else
		{
			cARI_HESAP_HAREKETLERI.cha_karsidgrupno = 0;
		}
		cARI_HESAP_HAREKETLERI.cha_srmrkkodu = sorumlulukmerkezi.som_kod;
		cARI_HESAP_HAREKETLERI.cha_reftarihi = new DateTime(1899, 12, 30);
		cARI_HESAP_HAREKETLERI.cha_odeme_arr1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr7 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr8 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_miktari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_aratoplam = tahsilat.tutar;
		cARI_HESAP_HAREKETLERI.cha_vergipntr = 0;
		cARI_HESAP_HAREKETLERI.cha_istisnakodu = 0;
		cARI_HESAP_HAREKETLERI.cha_ver_tev_carpani = 0.0;
		cARI_HESAP_HAREKETLERI.cha_stopaj = 0.0;
		cARI_HESAP_HAREKETLERI.cha_savsandesfonu = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_satici_kodu = TemsilciKodu;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_borsa = 0.0;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_bagkur = 0.0;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_diger = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalMSDF = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliye = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalStopaj = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalKomisyonu = 0.0;
		cARI_HESAP_HAREKETLERI.cha_StFonPntr = 0;
		cARI_HESAP_HAREKETLERI.cha_pos_hareketi = false;
		cARI_HESAP_HAREKETLERI.cha_vardiya_tarihi = new DateTime(1900, 1, 1);
		cARI_HESAP_HAREKETLERI.cha_vardiya_no = 0;
		cARI_HESAP_HAREKETLERI.cha_vardiya_evrak_ti = enum_cha_vardita_evrak_ti.HizmetSatisi;
		cARI_HESAP_HAREKETLERI.cha_HalRusum = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalNavlunTut = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalRehinFuture = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalKomisyon = 0.0;
		cARI_HESAP_HAREKETLERI.cha_Vade_Farki_Yuz = 0.0;
		cARI_HESAP_HAREKETLERI.cha_karsisrmrkkodu = tahsilat.sorumlulukmerkezi.som_kod;
		cARI_HESAP_HAREKETLERI.cha_EXIMkodu = "";
		cARI_HESAP_HAREKETLERI.cha_HalRehinSandikmiktari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikVrMiktar = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikKDVTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalrehinSandikTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_Tevkifat_turu = enum_cha_Tevkifat_turu.TevkifatYok;
		cARI_HESAP_HAREKETLERI.cha_otvtutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_otvvergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_projekodu = proje.pro_kodu;
		cARI_HESAP_HAREKETLERI.cha_sozlesme_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_sozlesme_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_yat_tes_kodu = "";
		cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = "";
		cARI_HESAP_HAREKETLERI.cha_oivergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_meblag_ana_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_meblag_alt_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_meblag_orj_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_ciroprim_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_ciroprim_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliyeKdv = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliyeVergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_bakimhar_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_bakimhar_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_avanstalep_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_avanstalep_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_oiv_pntr = 0;
		cARI_HESAP_HAREKETLERI.cha_oiv_vergi = 0.0;
		cARI_HESAP_HAREKETLERI.cha_oivtutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas1 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas2 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas3 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas4 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas5 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas6 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas7 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas8 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas9 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas10 = 0;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas1 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas2 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas3 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas4 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas5 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas6 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas7 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas8 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas9 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas10 = false;
		cARI_HESAP_HAREKETLERI.sck_borclu = tahsilat.sck_borclu;
		cARI_HESAP_HAREKETLERI.sck_bankano = tahsilat.sck_bankano;
		cARI_HESAP_HAREKETLERI.sck_vdaire_no = tahsilat.sck_vdaire_no;
		cARI_HESAP_HAREKETLERI.sck_banka_adres1 = tahsilat.sck_banka_adres1;
		cARI_HESAP_HAREKETLERI.sck_sube_adres2 = tahsilat.sck_sube_adres2;
		cARI_HESAP_HAREKETLERI.sck_hesapno_sehir = tahsilat.sck_hesapno_sehir;
		cARI_HESAP_HAREKETLERI.sck_no = tahsilat.sck_no;
		cARI_HESAP_HAREKETLERI.Sck_TCMB_Banka_kodu = tahsilat.Sck_TCMB_Banka_kodu;
		cARI_HESAP_HAREKETLERI.Sck_TCMB_Sube_kodu = tahsilat.Sck_TCMB_Sube_kodu;
		cARI_HESAP_HAREKETLERI.Sck_TCMB_il_kodu = tahsilat.Sck_TCMB_il_kodu;
		_TahsilatHareketleri.Add(cARI_HESAP_HAREKETLERI);
		OnEvrakChanged(EventArgs.Empty);
		return 1;
	}

	public int AddMasraf(Masraf masraf)
	{
		CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
		double yuzde = Parametreler.vergitanimlari[masraf.vergi_pntr].Yuzde;
		if (masraf.kdvdahil && (masraf.vergi_pntr != 0 || masraf.vergi_pntr != 1))
		{
			masraf.tutar /= yuzde / 100.0 + 1.0;
		}
		double num = masraf.tutar / 100.0 * yuzde;
		cARI_HESAP_HAREKETLERI.cha_RECid_DBCno = _DBCno;
		cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
		cARI_HESAP_HAREKETLERI.cha_normal_Iade = enum_cha_normal_Iade.Normal;
		cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.AlisFaturasi;
		cARI_HESAP_HAREKETLERI.cha_ticaret_turu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
		cARI_HESAP_HAREKETLERI.cha_create_user = mikrouserno;
		cARI_HESAP_HAREKETLERI.cha_create_date = DateTime.Now;
		cARI_HESAP_HAREKETLERI.cha_lastup_user = mikrouserno;
		cARI_HESAP_HAREKETLERI.cha_lastup_date = DateTime.Now;
		cARI_HESAP_HAREKETLERI.cha_firmano = Firma.fir_sirano;
		cARI_HESAP_HAREKETLERI.cha_subeno = Sube.Sube_no;
		cARI_HESAP_HAREKETLERI.cha_tarihi = _evraktarih;
		cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.HizmetFaturasi;
		cARI_HESAP_HAREKETLERI.cha_satir_no = _GenelCariHesapHareketleri.Count;
		cARI_HESAP_HAREKETLERI.cha_evrakno_seri = EvrakNoSeri;
		cARI_HESAP_HAREKETLERI.cha_evrakno_sira = EvrakNoSira;
		cARI_HESAP_HAREKETLERI.cha_belge_no = BelgeNo;
		cARI_HESAP_HAREKETLERI.cha_belge_tarih = _belgetarih;
		cARI_HESAP_HAREKETLERI.cha_cari_cins = enum_cha_cari_cins.Kasamiz;
		cARI_HESAP_HAREKETLERI.cha_kod = kapamahesapkodu;
		cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = enum_cha_kasa_hizmet.Giderimiz;
		cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = masraf.his_kod;
		cARI_HESAP_HAREKETLERI.cha_d_kurtar = kur.dov_tarih;
		cARI_HESAP_HAREKETLERI.cha_d_cins = kur.dov_no;
		cARI_HESAP_HAREKETLERI.cha_d_kur = kur.dov_fiyat;
		cARI_HESAP_HAREKETLERI.cha_altd_kur = alternatifdovizkuru.dov_fiyat;
		cARI_HESAP_HAREKETLERI.cha_grupno = Getchagrupno();
		cARI_HESAP_HAREKETLERI.cha_meblag = masraf.tutar + num;
		cARI_HESAP_HAREKETLERI.cha_vade = 0;
		cARI_HESAP_HAREKETLERI.cha_fis_tarih = new DateTime(1900, 1, 1);
		cARI_HESAP_HAREKETLERI.cha_fis_sirano = 0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi7 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi8 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi9 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergi10 = 0.0;
		switch (masraf.vergi_pntr)
		{
		case 2:
			cARI_HESAP_HAREKETLERI.cha_vergi2 = num;
			break;
		case 3:
			cARI_HESAP_HAREKETLERI.cha_vergi3 = num;
			break;
		case 4:
			cARI_HESAP_HAREKETLERI.cha_vergi4 = num;
			break;
		case 5:
			cARI_HESAP_HAREKETLERI.cha_vergi5 = num;
			break;
		}
		cARI_HESAP_HAREKETLERI.cha_yuvarlama = 0.0;
		cARI_HESAP_HAREKETLERI.cha_tpoz = enum_cha_tpoz.Kapali;
		cARI_HESAP_HAREKETLERI.cha_aciklama = masraf.aciklama;
		cARI_HESAP_HAREKETLERI.cha_trefno = "";
		cARI_HESAP_HAREKETLERI.cha_sntck_poz = enum_cha_sntck_poz.Portfoyde;
		cARI_HESAP_HAREKETLERI.cha_karsidcinsi = 0;
		cARI_HESAP_HAREKETLERI.cha_karsid_kur = 1.0;
		cARI_HESAP_HAREKETLERI.cha_karsidgrupno = 0;
		cARI_HESAP_HAREKETLERI.cha_srmrkkodu = sorumlulukmerkezi.som_kod;
		cARI_HESAP_HAREKETLERI.cha_reftarihi = new DateTime(1899, 12, 30);
		cARI_HESAP_HAREKETLERI.cha_odeme_arr1 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr2 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr3 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr4 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr5 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr6 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr7 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_odeme_arr8 = 0.0;
		cARI_HESAP_HAREKETLERI.cha_miktari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_aratoplam = masraf.tutar;
		cARI_HESAP_HAREKETLERI.cha_vergipntr = masraf.vergi_pntr;
		cARI_HESAP_HAREKETLERI.cha_istisnakodu = 0;
		cARI_HESAP_HAREKETLERI.cha_ver_tev_carpani = 0.0;
		cARI_HESAP_HAREKETLERI.cha_stopaj = 0.0;
		cARI_HESAP_HAREKETLERI.cha_savsandesfonu = 0.0;
		cARI_HESAP_HAREKETLERI.cha_vergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_satici_kodu = TemsilciKodu;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_borsa = 0.0;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_bagkur = 0.0;
		cARI_HESAP_HAREKETLERI.cha_mustahsil_diger = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalMSDF = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliye = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalStopaj = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalKomisyonu = 0.0;
		cARI_HESAP_HAREKETLERI.cha_StFonPntr = 0;
		cARI_HESAP_HAREKETLERI.cha_pos_hareketi = false;
		cARI_HESAP_HAREKETLERI.cha_vardiya_tarihi = new DateTime(1900, 1, 1);
		cARI_HESAP_HAREKETLERI.cha_vardiya_no = 0;
		cARI_HESAP_HAREKETLERI.cha_vardiya_evrak_ti = enum_cha_vardita_evrak_ti.HizmetSatisi;
		cARI_HESAP_HAREKETLERI.cha_HalRusum = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalNavlunTut = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalRehinFuture = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalKomisyon = 0.0;
		cARI_HESAP_HAREKETLERI.cha_Vade_Farki_Yuz = 0.0;
		cARI_HESAP_HAREKETLERI.cha_karsisrmrkkodu = sorumlulukmerkezi.som_kod;
		cARI_HESAP_HAREKETLERI.cha_EXIMkodu = "";
		cARI_HESAP_HAREKETLERI.cha_HalRehinSandikmiktari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikVrMiktar = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalSandikKDVTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalrehinSandikTutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_Tevkifat_turu = enum_cha_Tevkifat_turu.TevkifatYok;
		cARI_HESAP_HAREKETLERI.cha_otvtutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_otvvergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_projekodu = proje.pro_kodu;
		cARI_HESAP_HAREKETLERI.cha_sozlesme_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_sozlesme_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_yat_tes_kodu = "";
		cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = "";
		cARI_HESAP_HAREKETLERI.cha_oivergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_meblag_ana_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_meblag_alt_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_meblag_orj_doviz_icin_gecersiz_fl = 0;
		cARI_HESAP_HAREKETLERI.cha_ciroprim_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_ciroprim_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliyeKdv = 0.0;
		cARI_HESAP_HAREKETLERI.cha_HalHamaliyeVergisiz_fl = false;
		cARI_HESAP_HAREKETLERI.cha_bakimhar_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_bakimhar_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_avanstalep_DBCno = 0;
		cARI_HESAP_HAREKETLERI.cha_avanstalep_RECno = 0;
		cARI_HESAP_HAREKETLERI.cha_oiv_pntr = 0;
		cARI_HESAP_HAREKETLERI.cha_oiv_vergi = 0.0;
		cARI_HESAP_HAREKETLERI.cha_oivtutari = 0.0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas1 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas2 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas3 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas4 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas5 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas6 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas7 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas8 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas9 = 0;
		cARI_HESAP_HAREKETLERI.cha_isk_mas10 = 0;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas1 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas2 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas3 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas4 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas5 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas6 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas7 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas8 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas9 = false;
		cARI_HESAP_HAREKETLERI.cha_sat_iskmas10 = false;
		_GenelCariHesapHareketleri.Add(cARI_HESAP_HAREKETLERI);
		OnEvrakChanged(EventArgs.Empty);
		return 1;
	}

	public void AddSiparisHareketi(SIPARISLER hareket, bool SiparisKarsilamaYap)
	{
		_Siparisler.Add(hareket);
		if (SiparisKarsilamaYap)
		{
			_sipariskarsilamami = true;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void AddSiparisHareketi(List<SIPARISLER> hareket, bool SiparisKarsilamaYap)
	{
		_Siparisler.AddRange(hareket);
		if (SiparisKarsilamaYap)
		{
			_sipariskarsilamami = true;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunMain(Stok stok)
	{
		if (stok.ekleme_bilgileri.Miktar == 0.0)
		{
			return enum_evrak_siparis_ekleme_sonuc.MiktarSifirOlamaz;
		}
		if (Parametreler.MaksimumSatirSayisi > 0 && GetKalemSayisi() >= Parametreler.MaksimumSatirSayisi && !IsUrunListedeVar(stok.sto_kod))
		{
			return enum_evrak_siparis_ekleme_sonuc.MaksimumSatirSayisinaUlasildi;
		}
		if (stok.sto_detay_takip == 3 && (_evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || _evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi || _evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || _evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || _evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi))
		{
			foreach (STOK_HAREKETLERI item in _StokHareketleri)
			{
				if (!(item.sth_stok_kod == stok.sto_kod))
				{
					continue;
				}
				foreach (STOK_SERINO_TANIMLARI item2 in item.stok_serinolari)
				{
					foreach (STOK_SERINO_TANIMLARI item3 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						if (item2.chz_serino == item3.chz_serino)
						{
							return enum_evrak_siparis_ekleme_sonuc.SeriNoDahaOnceEklenmis;
						}
					}
				}
			}
		}
		if (stok.sto_detay_takip == 3 && _evraktipi == enum_GenelEvrakTipleri.SayimSonuclariGirisFisi)
		{
			foreach (SAYIM_SONUCLARI item4 in _SayimSonuclariHareketleri)
			{
				if (!(item4.sym_Stokkodu == stok.sto_kod))
				{
					continue;
				}
				foreach (STOK_SERINO_TANIMLARI item5 in stok.ekleme_bilgileri.serino_tanimlari)
				{
					if (item4.sym_serino == item5.chz_serino)
					{
						return enum_evrak_siparis_ekleme_sonuc.SeriNoDahaOnceEklenmis;
					}
				}
			}
		}
		if (stok.sto_detay_takip == 3 && (_evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || _evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi || _evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || _evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || _evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi) && stok.ekleme_bilgileri.Miktar != (double)stok.ekleme_bilgileri.serino_tanimlari.Count)
		{
			return enum_evrak_siparis_ekleme_sonuc.SeriNoGirilmekZorunda;
		}
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			return AddUrunSiparis(stok);
		case enum_GenelEvrakTipleri.ProformaSiparis:
			return AddUrunSiparis(stok);
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			return AddUrunDepolarArasiSiparis(stok);
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			return AddUrunSayimSonuclari(stok);
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeOnaylama:
			return AddUrunDepolarArasiNakliyeOnaylama(stok);
		case enum_GenelEvrakTipleri.BarkodKontrolluFaturaOnaylama:
			return AddUrunBarkodKontrolluFaturaOnaylama(stok);
		default:
			if (sipariskarsilamami)
			{
				if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
				{
					return AddUrunNormalSiparisKarsilamaDepolarArasi(stok);
				}
				return AddUrunNormalSiparisKarsilamaNormal(stok);
			}
			return AddUrunNormal(stok);
		}
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunDepolarArasiSiparis(Stok stok)
	{
		if (stok.ekleme_bilgileri.SatirBirlestir)
		{
			foreach (DEPOLAR_ARASI_SIPARISLER item in _DepolarArasiSiparisHareketleri)
			{
				if (!(item.ssip_stok_kod == stok.sto_kod) || !(stok.ekleme_bilgileri.Aciklama1 == item.ssip_aciklama))
				{
					continue;
				}
				item.ssip_b_fiyat = stok.ekleme_bilgileri.BirimFiyat.FiyatBrut;
				item.ssip_birim_pntr = stok.ekleme_bilgileri.sth_birim_pntr;
				item.ssip_miktar += stok.ekleme_bilgileri.Miktar;
				item.ssip_aciklama = stok.ekleme_bilgileri.Aciklama1;
				item.ssip_tutar += stok.ekleme_bilgileri.BirimFiyat.FiyatBrut * stok.ekleme_bilgileri.Miktar;
				foreach (BEDEN_HAREKETLERI item2 in stok.ekleme_bilgileri.renk_beden_hareketleri)
				{
					bool flag = false;
					foreach (BEDEN_HAREKETLERI item3 in item.renk_beden_hareketleri)
					{
						if (item2.BdnHar_BedenNo == item3.BdnHar_BedenNo)
						{
							item3.BdnHar_HarGor += item2.BdnHar_HarGor;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI.BdnHar_HarGor = item2.BdnHar_HarGor;
						bEDEN_HAREKETLERI.BdnHar_BedenNo = item2.BdnHar_BedenNo;
						item.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI);
					}
				}
				return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
			}
		}
		DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER = new DEPOLAR_ARASI_SIPARISLER();
		dEPOLAR_ARASI_SIPARISLER.ssip_RECid_DBCno = _DBCno;
		dEPOLAR_ARASI_SIPARISLER.ssip_create_user = mikrouserno;
		dEPOLAR_ARASI_SIPARISLER.ssip_lastup_user = mikrouserno;
		dEPOLAR_ARASI_SIPARISLER.ssip_firmano = Firma.fir_sirano;
		dEPOLAR_ARASI_SIPARISLER.ssip_subeno = Sube.Sube_no;
		dEPOLAR_ARASI_SIPARISLER.ssip_tarih = _evraktarih;
		dEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih = _sevkteslimtarihi;
		dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri = EvrakNoSeri;
		dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira = EvrakNoSira;
		dEPOLAR_ARASI_SIPARISLER.ssip_satirno = _DepolarArasiSiparisHareketleri.Count;
		dEPOLAR_ARASI_SIPARISLER.ssip_belgeno = BelgeNo;
		dEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih = _belgetarih;
		dEPOLAR_ARASI_SIPARISLER.ssip_stok_kod = stok.sto_kod;
		dEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat = stok.ekleme_bilgileri.BirimFiyat.FiyatBrut;
		dEPOLAR_ARASI_SIPARISLER.ssip_miktar = stok.ekleme_bilgileri.Miktar;
		dEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr = stok.ekleme_bilgileri.sth_birim_pntr;
		dEPOLAR_ARASI_SIPARISLER.ssip_tutar = stok.ekleme_bilgileri.BirimFiyat.FiyatBrut * stok.ekleme_bilgileri.Miktar;
		dEPOLAR_ARASI_SIPARISLER.ssip_aciklama = stok.ekleme_bilgileri.Aciklama1;
		dEPOLAR_ARASI_SIPARISLER.ssip_girdepo = _hedefdepo.dep_no;
		dEPOLAR_ARASI_SIPARISLER.ssip_cikdepo = _kaynakdepo.dep_no;
		dEPOLAR_ARASI_SIPARISLER.ssip_projekodu = proje.pro_kodu;
		dEPOLAR_ARASI_SIPARISLER.ssip_sormerkezi = sorumlulukmerkezi.som_kod;
		dEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no = FiyatListesi.sfl_sirano;
		dEPOLAR_ARASI_SIPARISLER.sto_birim1_ad = stok.sto_birim1_ad;
		dEPOLAR_ARASI_SIPARISLER.sto_birim1_katsayi = stok.sto_birim1_katsayi;
		dEPOLAR_ARASI_SIPARISLER.sto_birim2_ad = stok.sto_birim2_ad;
		dEPOLAR_ARASI_SIPARISLER.sto_birim2_katsayi = stok.sto_birim2_katsayi;
		dEPOLAR_ARASI_SIPARISLER.sto_birim3_ad = stok.sto_birim3_ad;
		dEPOLAR_ARASI_SIPARISLER.sto_birim3_katsayi = stok.sto_birim3_katsayi;
		dEPOLAR_ARASI_SIPARISLER.sto_birim4_ad = stok.sto_birim4_ad;
		dEPOLAR_ARASI_SIPARISLER.sto_birim4_katsayi = stok.sto_birim4_katsayi;
		dEPOLAR_ARASI_SIPARISLER.sto_isim = stok.sto_isim;
		foreach (BEDEN_HAREKETLERI item4 in stok.ekleme_bilgileri.renk_beden_hareketleri)
		{
			BEDEN_HAREKETLERI bEDEN_HAREKETLERI2 = new BEDEN_HAREKETLERI();
			bEDEN_HAREKETLERI2.BdnHar_HarGor = item4.BdnHar_HarGor;
			bEDEN_HAREKETLERI2.BdnHar_BedenNo = item4.BdnHar_BedenNo;
			dEPOLAR_ARASI_SIPARISLER.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI2);
		}
		_DepolarArasiSiparisHareketleri.Add(dEPOLAR_ARASI_SIPARISLER);
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunSiparis(Stok stok)
	{
		if (stok.ekleme_bilgileri.SatirBirlestir)
		{
			foreach (SIPARISLER item in _Siparisler)
			{
				if (!(item.sip_stok_kod == stok.sto_kod) || !(stok.ekleme_bilgileri.Aciklama1 == item.sip_aciklama) || !(stok.ekleme_bilgileri.parti_kodu == item.sip_parti_kodu) || stok.ekleme_bilgileri.lot_no != item.sip_lot_no)
				{
					continue;
				}
				item.sip_b_fiyat = stok.ekleme_bilgileri.BirimFiyat.FiyatBrut;
				item.sip_birim_pntr = stok.ekleme_bilgileri.sth_birim_pntr;
				item.sip_miktar += stok.ekleme_bilgileri.Miktar;
				item.sip_aciklama = stok.ekleme_bilgileri.Aciklama1;
				item.sip_aciklama2 = stok.ekleme_bilgileri.Aciklama2;
				item.sip_otvtutari += stok.ekleme_bilgileri.BirimFiyat.OtvTutariToplam * stok.ekleme_bilgileri.Miktar;
				item.sip_iskonto_1 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_iskonto_2 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_iskonto_3 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_iskonto_4 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_iskonto_5 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_iskonto_6 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_iskonto1 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_UygulamaSekli;
				item.sip_iskonto2 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_UygulamaSekli;
				item.sip_iskonto3 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_UygulamaSekli;
				item.sip_iskonto4 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_UygulamaSekli;
				item.sip_iskonto5 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_UygulamaSekli;
				item.sip_iskonto6 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_UygulamaSekli;
				item.sip_masraf_1 += stok.ekleme_bilgileri.BirimFiyat.Masraf_1_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_masraf_2 += stok.ekleme_bilgileri.BirimFiyat.Masraf_2_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_masraf_3 += stok.ekleme_bilgileri.BirimFiyat.Masraf_3_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_masraf_4 += stok.ekleme_bilgileri.BirimFiyat.Masraf_4_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sip_masraf1 = stok.ekleme_bilgileri.BirimFiyat.Masraf_1_UygulamaSekli;
				item.sip_masraf2 = stok.ekleme_bilgileri.BirimFiyat.Masraf_2_UygulamaSekli;
				item.sip_masraf3 = stok.ekleme_bilgileri.BirimFiyat.Masraf_3_UygulamaSekli;
				item.sip_masraf4 = stok.ekleme_bilgileri.BirimFiyat.Masraf_4_UygulamaSekli;
				item.sip_tutar += stok.ekleme_bilgileri.BirimFiyat.FiyatBrut * stok.ekleme_bilgileri.Miktar;
				item.sip_vergi += stok.KdvTutariToplamNetMasrafsiz();
				item.sip_masvergi += stok.ekleme_bilgileri.BirimFiyat.MasrafTutariToplam * stok.ekleme_bilgileri.Miktar / 100.0 * 18.0;
				item.sip_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
				item.sip_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
				item.sip_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
				item.sip_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
				item.sip_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
				item.sip_FormulMiktar += stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
				foreach (BEDEN_HAREKETLERI item2 in stok.ekleme_bilgileri.renk_beden_hareketleri)
				{
					bool flag = false;
					foreach (BEDEN_HAREKETLERI item3 in item.renk_beden_hareketleri)
					{
						if (item2.BdnHar_BedenNo == item3.BdnHar_BedenNo)
						{
							item3.BdnHar_HarGor += item2.BdnHar_HarGor;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI.BdnHar_HarGor = item2.BdnHar_HarGor;
						bEDEN_HAREKETLERI.BdnHar_BedenNo = item2.BdnHar_BedenNo;
						item.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI);
					}
				}
				return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
			}
		}
		SIPARISLER sIPARISLER = new SIPARISLER();
		sIPARISLER.sip_RECid_DBCno = _DBCno;
		sIPARISLER.sip_create_user = mikrouserno;
		sIPARISLER.sip_lastup_user = mikrouserno;
		sIPARISLER.sip_firmano = Firma.fir_sirano;
		sIPARISLER.sip_subeno = Sube.Sube_no;
		sIPARISLER.sip_tarih = _evraktarih;
		sIPARISLER.sip_teslim_tarih = _sevkteslimtarihi;
		sIPARISLER.sip_tip = enum_sip_tip.Talep;
		sIPARISLER.sip_cins = enum_sip_cins.NormalSiparis;
		sIPARISLER.sip_evrakno_seri = EvrakNoSeri;
		sIPARISLER.sip_evrakno_sira = EvrakNoSira;
		sIPARISLER.sip_satirno = _Siparisler.Count;
		sIPARISLER.sip_belgeno = BelgeNo;
		sIPARISLER.sip_belge_tarih = _belgetarih;
		sIPARISLER.sip_satici_kod = TemsilciKodu;
		sIPARISLER.sip_musteri_kod = cari.cari_kod;
		sIPARISLER.sip_stok_kod = stok.sto_kod;
		sIPARISLER.sip_b_fiyat = stok.ekleme_bilgileri.BirimFiyat.FiyatBrut;
		sIPARISLER.sip_miktar = stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_birim_pntr = stok.ekleme_bilgileri.sth_birim_pntr;
		sIPARISLER.sip_tutar = stok.ekleme_bilgileri.BirimFiyat.FiyatBrut * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_iskonto_1 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_iskonto_2 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_iskonto_3 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_iskonto_4 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_iskonto_5 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_iskonto_6 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_masraf_1 = stok.ekleme_bilgileri.BirimFiyat.Masraf_1_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_masraf_2 = stok.ekleme_bilgileri.BirimFiyat.Masraf_2_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_masraf_3 = stok.ekleme_bilgileri.BirimFiyat.Masraf_3_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_masraf_4 = stok.ekleme_bilgileri.BirimFiyat.Masraf_4_Tutari * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_vergi_pntr = stok.ekleme_bilgileri.vergi_pntr;
		sIPARISLER.sip_vergi = stok.KdvTutariToplamNetMasrafsiz();
		sIPARISLER.sip_masvergi_pntr = 4;
		sIPARISLER.sip_masvergi = stok.ekleme_bilgileri.BirimFiyat.MasrafTutariToplam * stok.ekleme_bilgileri.Miktar / 100.0 * 18.0;
		sIPARISLER.sip_opno = odemeplani;
		sIPARISLER.sip_teslimturu = sip_teslimturu;
		sIPARISLER.sip_aciklama = stok.ekleme_bilgileri.Aciklama1;
		sIPARISLER.sip_aciklama2 = stok.ekleme_bilgileri.Aciklama2;
		sIPARISLER.sip_depono = _kaynakdepo.dep_no;
		sIPARISLER.sip_cari_sormerk = stok.ekleme_bilgileri.sorumluluk_merkezi_kodu;
		sIPARISLER.sip_stok_sormerk = stok.ekleme_bilgileri.sorumluluk_merkezi_kodu;
		sIPARISLER.sip_cari_grupno = 0;
		sIPARISLER.sip_doviz_cinsi = dovizcinsi;
		sIPARISLER.sip_doviz_kuru = kur.dov_fiyat;
		sIPARISLER.sip_alt_doviz_kuru = alternatifdovizkuru.dov_fiyat;
		sIPARISLER.sip_adresno = sevkadresno;
		sIPARISLER.sip_iskonto1 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_UygulamaSekli;
		sIPARISLER.sip_iskonto2 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_UygulamaSekli;
		sIPARISLER.sip_iskonto3 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_UygulamaSekli;
		sIPARISLER.sip_iskonto4 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_UygulamaSekli;
		sIPARISLER.sip_iskonto5 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_UygulamaSekli;
		sIPARISLER.sip_iskonto6 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_UygulamaSekli;
		sIPARISLER.sip_masraf1 = stok.ekleme_bilgileri.BirimFiyat.Masraf_1_UygulamaSekli;
		sIPARISLER.sip_masraf2 = stok.ekleme_bilgileri.BirimFiyat.Masraf_2_UygulamaSekli;
		sIPARISLER.sip_masraf3 = stok.ekleme_bilgileri.BirimFiyat.Masraf_3_UygulamaSekli;
		sIPARISLER.sip_masraf4 = stok.ekleme_bilgileri.BirimFiyat.Masraf_4_UygulamaSekli;
		sIPARISLER.sip_durumu = enum_sip_durumu.StoktanSevkEdilecek;
		sIPARISLER.sip_parti_kodu = stok.ekleme_bilgileri.parti_kodu;
		sIPARISLER.sip_lot_no = stok.ekleme_bilgileri.lot_no;
		sIPARISLER.sip_projekodu = stok.ekleme_bilgileri.proje_kodu;
		sIPARISLER.sip_fiyat_liste_no = FiyatListesi.sfl_sirano;
		sIPARISLER.sip_harekettipi = enum_sip_harekettipi.Stok;
		sIPARISLER.sto_birim1_ad = stok.sto_birim1_ad;
		sIPARISLER.sto_birim1_katsayi = stok.sto_birim1_katsayi;
		sIPARISLER.sto_birim2_ad = stok.sto_birim2_ad;
		sIPARISLER.sto_birim2_katsayi = stok.sto_birim2_katsayi;
		sIPARISLER.sto_birim3_ad = stok.sto_birim3_ad;
		sIPARISLER.sto_birim3_katsayi = stok.sto_birim3_katsayi;
		sIPARISLER.sto_birim4_ad = stok.sto_birim4_ad;
		sIPARISLER.sto_birim4_katsayi = stok.sto_birim4_katsayi;
		sIPARISLER.sto_isim = stok.sto_isim;
		sIPARISLER.sip_otvtutari = stok.ekleme_bilgileri.BirimFiyat.OtvTutariToplam * stok.ekleme_bilgileri.Miktar;
		sIPARISLER.sip_cagrilabilir_fl = Parametreler.SiparisCagrilabilir_fl;
		sIPARISLER.sip_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
		sIPARISLER.sip_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
		sIPARISLER.sip_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
		sIPARISLER.sip_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
		sIPARISLER.sip_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
		sIPARISLER.sip_FormulMiktar = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
		sIPARISLER.sip_FormulMiktarNo = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktarNo;
		foreach (BEDEN_HAREKETLERI item4 in stok.ekleme_bilgileri.renk_beden_hareketleri)
		{
			BEDEN_HAREKETLERI bEDEN_HAREKETLERI2 = new BEDEN_HAREKETLERI();
			bEDEN_HAREKETLERI2.BdnHar_HarGor = item4.BdnHar_HarGor;
			bEDEN_HAREKETLERI2.BdnHar_BedenNo = item4.BdnHar_BedenNo;
			sIPARISLER.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI2);
		}
		_Siparisler.Add(sIPARISLER);
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunDepolarArasiNakliyeOnaylama(Stok stok)
	{
		bool flag = false;
		double num = 0.0;
		foreach (STOK_HAREKETLERI item in _StokHareketleri)
		{
			if (item.sth_stok_kod == stok.sto_kod && item.sth_parti_kodu == stok.ekleme_bilgileri.parti_kodu && item.sth_lot_no == stok.ekleme_bilgileri.lot_no)
			{
				flag = true;
				num += item.sth_miktar - item.OkutulanMiktar;
			}
		}
		if (!flag)
		{
			return enum_evrak_siparis_ekleme_sonuc.UrunSiparisteYok;
		}
		if (stok.ekleme_bilgileri.Miktar > num)
		{
			return enum_evrak_siparis_ekleme_sonuc.UrunMiktariSiparisMiktarindanFazla;
		}
		double num2 = stok.ekleme_bilgileri.Miktar;
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			if (stok.sto_kod == item2.sth_stok_kod)
			{
				double num3 = item2.sth_miktar - item2.OkutulanMiktar;
				double num4 = 0.0;
				if (num2 >= num3)
				{
					num4 = num3;
					num2 -= num3;
				}
				else
				{
					num4 = num2;
					num2 = 0.0;
				}
				item2.OkutulanMiktar += num4;
			}
		}
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunBarkodKontrolluFaturaOnaylama(Stok stok)
	{
		bool flag = false;
		double num = 0.0;
		foreach (STOK_HAREKETLERI item in _StokHareketleri)
		{
			if (item.sth_stok_kod == stok.sto_kod && item.sth_parti_kodu == stok.ekleme_bilgileri.parti_kodu && item.sth_lot_no == stok.ekleme_bilgileri.lot_no)
			{
				flag = true;
				num += item.sth_miktar - item.OkutulanMiktar;
			}
		}
		if (!flag)
		{
			return enum_evrak_siparis_ekleme_sonuc.UrunSiparisteYok;
		}
		if (stok.ekleme_bilgileri.Miktar > num)
		{
			return enum_evrak_siparis_ekleme_sonuc.UrunMiktariSiparisMiktarindanFazla;
		}
		double num2 = stok.ekleme_bilgileri.Miktar;
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			if (stok.sto_kod == item2.sth_stok_kod)
			{
				double num3 = item2.sth_miktar - item2.OkutulanMiktar;
				double num4 = 0.0;
				if (num2 >= num3)
				{
					num4 = num3;
					num2 -= num3;
				}
				else
				{
					num4 = num2;
					num2 = 0.0;
				}
				item2.OkutulanMiktar += num4;
			}
		}
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunNormal(Stok stok)
	{
		double yuzde;
		if (stok.ekleme_bilgileri.SatirBirlestir)
		{
			foreach (STOK_HAREKETLERI item in _StokHareketleri)
			{
				if (!(item.sth_stok_kod == stok.sto_kod) || !(item.sth_aciklama == stok.ekleme_bilgileri.Aciklama1) || !(item.sth_parti_kodu == stok.ekleme_bilgileri.parti_kodu) || item.sth_lot_no != stok.ekleme_bilgileri.lot_no)
				{
					continue;
				}
				item.sth_miktar += stok.ekleme_bilgileri.Miktar;
				if (Parametreler.Miktar2Arttir)
				{
					item.sth_miktar2++;
				}
				else
				{
					item.sth_miktar2 += stok.ekleme_bilgileri.Miktar2;
				}
				item.sth_birim_pntr = stok.ekleme_bilgileri.sth_birim_pntr;
				item.sth_aciklama = stok.ekleme_bilgileri.Aciklama1;
				item.sth_tutar += stok.ekleme_bilgileri.BirimFiyat.FiyatBrut * stok.ekleme_bilgileri.Miktar;
				item.sth_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
				item.sth_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
				item.sth_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
				item.sth_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
				item.sth_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
				item.sth_FormulMiktar += stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
				if (EximKodu == "")
				{
					item.sth_otvtutari += stok.ekleme_bilgileri.BirimFiyat.OtvTutariToplam * stok.ekleme_bilgileri.Miktar;
					item.sth_otv_pntr = stok.ekleme_bilgileri.BirimFiyat.OtvVergiPntr;
					yuzde = Parametreler.vergitanimlari[item.sth_otv_pntr].Yuzde;
					item.sth_otv_vergi += stok.ekleme_bilgileri.BirimFiyat.OtvTutariToplam * stok.ekleme_bilgileri.Miktar / 100.0 * yuzde;
					item.sth_vergi += stok.KdvTutariToplamNetMasrafsiz();
					item.sth_masraf_vergi += stok.ekleme_bilgileri.BirimFiyat.MasrafTutariToplam * stok.ekleme_bilgileri.Miktar / 100.0 * 18.0;
				}
				item.sth_iskonto1 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_iskonto2 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_iskonto3 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_iskonto4 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_iskonto5 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_iskonto6 += stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_masraf1 += stok.ekleme_bilgileri.BirimFiyat.Masraf_1_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_masraf2 += stok.ekleme_bilgileri.BirimFiyat.Masraf_2_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_masraf3 += stok.ekleme_bilgileri.BirimFiyat.Masraf_3_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_masraf4 += stok.ekleme_bilgileri.BirimFiyat.Masraf_4_Tutari * stok.ekleme_bilgileri.Miktar;
				item.sth_isk_mas1 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_UygulamaSekli;
				item.sth_isk_mas2 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_UygulamaSekli;
				item.sth_isk_mas3 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_UygulamaSekli;
				item.sth_isk_mas4 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_UygulamaSekli;
				item.sth_isk_mas5 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_UygulamaSekli;
				item.sth_isk_mas6 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_UygulamaSekli;
				item.sth_isk_mas7 = stok.ekleme_bilgileri.BirimFiyat.Masraf_1_UygulamaSekli;
				item.sth_isk_mas8 = stok.ekleme_bilgileri.BirimFiyat.Masraf_2_UygulamaSekli;
				item.sth_isk_mas9 = stok.ekleme_bilgileri.BirimFiyat.Masraf_3_UygulamaSekli;
				item.sth_isk_mas10 = stok.ekleme_bilgileri.BirimFiyat.Masraf_4_UygulamaSekli;
				foreach (BEDEN_HAREKETLERI item2 in stok.ekleme_bilgileri.renk_beden_hareketleri)
				{
					bool flag = false;
					foreach (BEDEN_HAREKETLERI item3 in item.renk_beden_hareketleri)
					{
						if (item2.BdnHar_BedenNo == item3.BdnHar_BedenNo)
						{
							item3.BdnHar_HarGor += item2.BdnHar_HarGor;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI.BdnHar_HarGor = item2.BdnHar_HarGor;
						bEDEN_HAREKETLERI.BdnHar_BedenNo = item2.BdnHar_BedenNo;
						item.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI);
					}
				}
				foreach (STOK_SERINO_TANIMLARI item4 in stok.ekleme_bilgileri.serino_tanimlari)
				{
					item.stok_serinolari.Add(item4);
				}
				AddUrunCekiListesi(stok.sto_kod, stok.ekleme_bilgileri.Miktar, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
				return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
			}
		}
		STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
		sTOK_HAREKETLERI.sth_RECid_DBCno = _DBCno;
		sTOK_HAREKETLERI.sth_create_user = mikrouserno;
		sTOK_HAREKETLERI.sth_lastup_user = mikrouserno;
		sTOK_HAREKETLERI.sth_firmano = Firma.fir_sirano;
		sTOK_HAREKETLERI.sth_subeno = Sube.Sube_no;
		sTOK_HAREKETLERI.sth_tarih = _evraktarih;
		sTOK_HAREKETLERI.sth_evrakno_seri = EvrakNoSeri;
		sTOK_HAREKETLERI.sth_evrakno_sira = EvrakNoSira;
		sTOK_HAREKETLERI.sth_satirno = _StokHareketleri.Count;
		sTOK_HAREKETLERI.sth_belge_no = BelgeNo;
		sTOK_HAREKETLERI.sth_belge_tarih = _belgetarih;
		sTOK_HAREKETLERI.sth_stok_kod = stok.sto_kod;
		sTOK_HAREKETLERI.sth_netagirlik = 0.0;
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
			sTOK_HAREKETLERI.sth_giris_depo_no = _hedefdepo.dep_no;
		}
		else
		{
			sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
			sTOK_HAREKETLERI.sth_giris_depo_no = _kaynakdepo.dep_no;
		}
		sTOK_HAREKETLERI.sth_nakliyedeposu = 0;
		sTOK_HAREKETLERI.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
		if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
			sTOK_HAREKETLERI.sth_giris_depo_no = _nakliyedepo.dep_no;
			sTOK_HAREKETLERI.sth_nakliyedeposu = _hedefdepo.dep_no;
			sTOK_HAREKETLERI.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
		}
		sTOK_HAREKETLERI.sto_birim1_ad = stok.sto_birim1_ad;
		sTOK_HAREKETLERI.sto_birim2_ad = stok.sto_birim2_ad;
		sTOK_HAREKETLERI.sto_birim3_ad = stok.sto_birim3_ad;
		sTOK_HAREKETLERI.sto_birim4_ad = stok.sto_birim4_ad;
		sTOK_HAREKETLERI.sto_birim1_katsayi = stok.sto_birim1_katsayi;
		sTOK_HAREKETLERI.sto_birim2_katsayi = stok.sto_birim2_katsayi;
		sTOK_HAREKETLERI.sto_birim3_katsayi = stok.sto_birim3_katsayi;
		sTOK_HAREKETLERI.sto_birim4_katsayi = stok.sto_birim4_katsayi;
		sTOK_HAREKETLERI.sto_isim = stok.sto_isim;
		sTOK_HAREKETLERI.sth_maliyet_ana = 0.0;
		sTOK_HAREKETLERI.sth_maliyet_alternatif = 0.0;
		sTOK_HAREKETLERI.sth_maliyet_orjinal = 0.0;
		sTOK_HAREKETLERI.sth_parti_kodu = stok.ekleme_bilgileri.parti_kodu;
		sTOK_HAREKETLERI.sth_lot_no = stok.ekleme_bilgileri.lot_no;
		sTOK_HAREKETLERI.sth_cari_grup_no = Getchagrupno();
		switch (evraktipi)
		{
		case enum_GenelEvrakTipleri.SatisFaturasi:
			sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Cikis;
			if (stok.ekleme_bilgileri.SatisFaturasiIade)
			{
				sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
			}
			if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
			}
			else
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
			}
			if (normaliade == enum_cha_normal_Iade.Normal)
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
			}
			else
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Iade;
			}
			if (stok.ekleme_bilgileri.SatisFaturasiIade)
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Iade;
			}
			sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.CikisFaturasi;
			if (stok.ekleme_bilgileri.SatisFaturasiIade)
			{
				sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.GirisFaturasi;
			}
			sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
			break;
		case enum_GenelEvrakTipleri.AlisFaturasi:
			sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
			if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
			}
			else
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
			}
			if (normaliade == enum_cha_normal_Iade.Normal)
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
			}
			else
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Iade;
			}
			sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.GirisFaturasi;
			sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
			if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
			}
			else
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
			}
			if (normaliade == enum_cha_normal_Iade.Normal)
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
			}
			else
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Iade;
			}
			sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.GirisIrsaliyesi;
			sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Cikis;
			if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
			}
			else
			{
				sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
			}
			if (normaliade == enum_cha_normal_Iade.Normal)
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
			}
			else
			{
				sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Iade;
			}
			sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.CikisIrsaliyesi;
			sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			sTOK_HAREKETLERI.sth_tip = enum_sth_tip.DepoTransfer;
			sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Transfer;
			sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
			sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.DepoTransferFisi;
			sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			sTOK_HAREKETLERI.sth_tip = enum_sth_tip.DepoTransfer;
			sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Transfer;
			sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
			sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.DepolarArasiNakliyeFisi;
			sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
			break;
		}
		if (stok.ekleme_bilgileri.fiyat_farki_mi)
		{
			sTOK_HAREKETLERI.sth_cins = enum_sth_cins.DegerFarki;
		}
		sTOK_HAREKETLERI.sth_kons_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_kons_recid_recno = 0;
		sTOK_HAREKETLERI.sth_subesip_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_subesip_recid_recno = 0;
		sTOK_HAREKETLERI.sth_satistipi = enum_sth_satistipi.Kredilisatis;
		sTOK_HAREKETLERI.sth_proje_kodu = stok.ekleme_bilgileri.proje_kodu;
		sTOK_HAREKETLERI.sth_ihracat_kredi_kodu = "";
		sTOK_HAREKETLERI.sth_otv_pntr = 0;
		sTOK_HAREKETLERI.sth_otv_vergi = 0.0;
		sTOK_HAREKETLERI.sth_bkm_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_bkm_recid_recno = 0;
		sTOK_HAREKETLERI.sth_karsikons_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_karsikons_recid_recno = 0;
		sTOK_HAREKETLERI.sth_iade_evrak_seri = "";
		sTOK_HAREKETLERI.sth_iade_evrak_sira = 0;
		sTOK_HAREKETLERI.sth_diib_belge_no = "";
		sTOK_HAREKETLERI.sth_diib_satir_no = 0;
		sTOK_HAREKETLERI.sth_mensey_ulke_tipi = 0;
		sTOK_HAREKETLERI.sth_mensey_ulke_kodu = "";
		sTOK_HAREKETLERI.sth_brutagirlik = 0.0;
		sTOK_HAREKETLERI.sth_halrehmiktari = 0.0;
		sTOK_HAREKETLERI.sth_halrehfiyati = 0.0;
		sTOK_HAREKETLERI.sth_halsandikmiktari = 0.0;
		sTOK_HAREKETLERI.sth_halsandikfiyati = 0.0;
		sTOK_HAREKETLERI.sth_halsandikkdvtutari = 0.0;
		sTOK_HAREKETLERI.sth_direkt_iscilik_1 = 0.0;
		sTOK_HAREKETLERI.sth_direkt_iscilik_2 = 0.0;
		sTOK_HAREKETLERI.sth_direkt_iscilik_3 = 0.0;
		sTOK_HAREKETLERI.sth_direkt_iscilik_4 = 0.0;
		sTOK_HAREKETLERI.sth_direkt_iscilik_5 = 0.0;
		sTOK_HAREKETLERI.sth_genel_uretim_1 = 0.0;
		sTOK_HAREKETLERI.sth_genel_uretim_2 = 0.0;
		sTOK_HAREKETLERI.sth_genel_uretim_3 = 0.0;
		sTOK_HAREKETLERI.sth_genel_uretim_4 = 0.0;
		sTOK_HAREKETLERI.sth_genel_uretim_5 = 0.0;
		sTOK_HAREKETLERI.sth_yat_tes_kodu = "";
		sTOK_HAREKETLERI.sth_oiv_pntr = 0;
		sTOK_HAREKETLERI.sth_oiv_vergi = 0.0;
		if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			sTOK_HAREKETLERI.sth_malkbl_sevk_tarihi = _sevkteslimtarihi;
		}
		else
		{
			sTOK_HAREKETLERI.sth_malkbl_sevk_tarihi = _evraktarih;
		}
		sTOK_HAREKETLERI.sth_oivvergisiz_fl = false;
		sTOK_HAREKETLERI.sth_otvvergisiz_fl = false;
		sTOK_HAREKETLERI.sth_disticaret_turu = enum_sth_disticaret_turu.ToptanYurticiTicaret;
		sTOK_HAREKETLERI.sth_fiyat_liste_no = FiyatListesi.sfl_sirano;
		sTOK_HAREKETLERI.sth_fis_sirano2 = 0;
		sTOK_HAREKETLERI.sth_rez_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_rez_recid_recno = 0;
		sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_seri = "";
		sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_sira = 0;
		sTOK_HAREKETLERI.sth_fiyfark_esas_satir_no = 0;
		sTOK_HAREKETLERI.sth_optamam_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_optamam_recid_recno = 0;
		sTOK_HAREKETLERI.sth_oivtutari = 0.0;
		sTOK_HAREKETLERI.sth_Tevkifat_turu = enum_sth_Tevkifat_turu.TevkifatYok;
		sTOK_HAREKETLERI.sth_HalKomisyonuKdv = 0.0;
		sTOK_HAREKETLERI.sth_iadeTlp_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_iadeTlp_recid_recno = 0;
		sTOK_HAREKETLERI.sth_HalSatisRecid_dbcno = 0;
		sTOK_HAREKETLERI.sth_HalSatisRecid_recno = 0;
		sTOK_HAREKETLERI.sth_ciroprim_dbcno = 0;
		sTOK_HAREKETLERI.sth_ciroprim_recno = 0;
		sTOK_HAREKETLERI.sth_yetkili_recid_dbcno = 0;
		sTOK_HAREKETLERI.sth_yetkili_recid_recno = 0;
		sTOK_HAREKETLERI.sth_taxfree_fl = false;
		sTOK_HAREKETLERI.sth_HalRusum = 0.0;
		sTOK_HAREKETLERI.sth_isk_mas1 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas2 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas3 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas4 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas5 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas6 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas7 = stok.ekleme_bilgileri.BirimFiyat.Masraf_1_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas8 = stok.ekleme_bilgileri.BirimFiyat.Masraf_2_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas9 = stok.ekleme_bilgileri.BirimFiyat.Masraf_3_UygulamaSekli;
		sTOK_HAREKETLERI.sth_isk_mas10 = stok.ekleme_bilgileri.BirimFiyat.Masraf_4_UygulamaSekli;
		sTOK_HAREKETLERI.sth_miktar = stok.ekleme_bilgileri.Miktar;
		if (Parametreler.Miktar2Arttir)
		{
			sTOK_HAREKETLERI.sth_miktar2 = 1.0;
		}
		else
		{
			sTOK_HAREKETLERI.sth_miktar2 = stok.ekleme_bilgileri.Miktar2;
		}
		sTOK_HAREKETLERI.sth_birim_pntr = stok.ekleme_bilgileri.sth_birim_pntr;
		sTOK_HAREKETLERI.sth_tutar = stok.ekleme_bilgileri.BirimFiyat.FiyatBrut * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_otvtutari = stok.ekleme_bilgileri.BirimFiyat.OtvTutariToplam * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_otv_pntr = stok.ekleme_bilgileri.BirimFiyat.OtvVergiPntr;
		yuzde = Parametreler.vergitanimlari[sTOK_HAREKETLERI.sth_otv_pntr].Yuzde;
		sTOK_HAREKETLERI.sth_otv_vergi = stok.ekleme_bilgileri.BirimFiyat.OtvTutariToplam * stok.ekleme_bilgileri.Miktar / 100.0 * yuzde;
		sTOK_HAREKETLERI.sth_iskonto1 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_1_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_iskonto2 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_2_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_iskonto3 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_3_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_iskonto4 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_4_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_iskonto5 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_5_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_iskonto6 = stok.ekleme_bilgileri.BirimFiyat.Iskonto_6_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_masraf1 = stok.ekleme_bilgileri.BirimFiyat.Masraf_1_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_masraf2 = stok.ekleme_bilgileri.BirimFiyat.Masraf_2_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_masraf3 = stok.ekleme_bilgileri.BirimFiyat.Masraf_3_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_masraf4 = stok.ekleme_bilgileri.BirimFiyat.Masraf_4_Tutari * stok.ekleme_bilgileri.Miktar;
		sTOK_HAREKETLERI.sth_vergi_pntr = stok.ekleme_bilgileri.vergi_pntr;
		sTOK_HAREKETLERI.sth_vergi = stok.KdvTutariToplamNetMasrafsiz();
		sTOK_HAREKETLERI.sth_masraf_vergi_pntr = 4;
		sTOK_HAREKETLERI.sth_masraf_vergi = stok.ekleme_bilgileri.BirimFiyat.MasrafTutariToplam * stok.ekleme_bilgileri.Miktar / 100.0 * 18.0;
		sTOK_HAREKETLERI.sth_plasiyer_kodu = TemsilciKodu;
		sTOK_HAREKETLERI.sth_cari_kodu = cari.cari_kod;
		sTOK_HAREKETLERI.sth_kur_tarihi = kur.dov_tarih;
		sTOK_HAREKETLERI.sth_har_doviz_cinsi = kur.dov_no;
		sTOK_HAREKETLERI.sth_har_doviz_kuru = kur.dov_fiyat;
		sTOK_HAREKETLERI.sth_alt_doviz_kuru = alternatifdovizkuru.dov_fiyat;
		sTOK_HAREKETLERI.sth_stok_doviz_cinsi = stok.sto_doviz_cinsi;
		sTOK_HAREKETLERI.sth_stok_doviz_kuru = stok.ekleme_bilgileri.StokDovizCinsiKuru;
		sTOK_HAREKETLERI.sth_odeme_op = odemeplani;
		sTOK_HAREKETLERI.sth_aciklama = stok.ekleme_bilgileri.Aciklama1;
		sTOK_HAREKETLERI.sth_cari_srm_merkezi = stok.ekleme_bilgileri.sorumluluk_merkezi_kodu;
		sTOK_HAREKETLERI.sth_stok_srm_merkezi = stok.ekleme_bilgileri.sorumluluk_merkezi_kodu;
		sTOK_HAREKETLERI.sth_adres_no = sevkadresno;
		sTOK_HAREKETLERI.sth_sip_recid_recno = 0;
		sTOK_HAREKETLERI.sth_fat_recid_recno = 0;
		sTOK_HAREKETLERI.sth_fat_recid_dbcno = _DBCno;
		if (EximKodu != "")
		{
			sTOK_HAREKETLERI.sth_cins = enum_sth_cins.IthalatIhracat;
			sTOK_HAREKETLERI.sth_vergi_pntr = 0;
			sTOK_HAREKETLERI.sth_vergi = 0.0;
			sTOK_HAREKETLERI.sth_vergisiz_fl = true;
			sTOK_HAREKETLERI.sth_exim_kodu = EximKodu;
			sTOK_HAREKETLERI.sth_disticaret_turu = enum_sth_disticaret_turu.YurtdisiTicaret;
			sTOK_HAREKETLERI.sth_otvtutari = 0.0;
			sTOK_HAREKETLERI.sth_otvvergisiz_fl = true;
			sTOK_HAREKETLERI.sth_oiv_pntr = 0;
			sTOK_HAREKETLERI.sth_oiv_vergi = 0.0;
			sTOK_HAREKETLERI.sth_oivvergisiz_fl = true;
			sTOK_HAREKETLERI.sth_oivtutari = 0.0;
		}
		sTOK_HAREKETLERI.sth_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
		sTOK_HAREKETLERI.sth_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
		sTOK_HAREKETLERI.sth_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
		sTOK_HAREKETLERI.sth_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
		sTOK_HAREKETLERI.sth_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
		sTOK_HAREKETLERI.sth_FormulMiktar = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
		sTOK_HAREKETLERI.sth_FormulMiktarNo = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktarNo;
		foreach (BEDEN_HAREKETLERI item5 in stok.ekleme_bilgileri.renk_beden_hareketleri)
		{
			BEDEN_HAREKETLERI bEDEN_HAREKETLERI2 = new BEDEN_HAREKETLERI();
			bEDEN_HAREKETLERI2.BdnHar_HarGor = item5.BdnHar_HarGor;
			bEDEN_HAREKETLERI2.BdnHar_BedenNo = item5.BdnHar_BedenNo;
			sTOK_HAREKETLERI.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI2);
		}
		foreach (STOK_SERINO_TANIMLARI item6 in stok.ekleme_bilgileri.serino_tanimlari)
		{
			sTOK_HAREKETLERI.stok_serinolari.Add(item6);
		}
		_StokHareketleri.Add(sTOK_HAREKETLERI);
		AddUrunCekiListesi(stok.sto_kod, stok.ekleme_bilgileri.Miktar, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunNormalSiparisKarsilamaDepolarArasi(Stok stok)
	{
		List<int> list = new List<int>();
		List<Guid> list2 = new List<Guid>();
		List<int> BdnHar_RECid_RECno = new List<int>();
		List<Guid> BdnHar_Guid = new List<Guid>();
		List<double> list3 = new List<double>();
		if (stok.ekleme_bilgileri.renk_beden_hareketleri.Count > 0)
		{
			foreach (BEDEN_HAREKETLERI item in stok.ekleme_bilgileri.renk_beden_hareketleri)
			{
				double num = item.BdnHar_HarGor - item.BdnHar_TesMik;
				foreach (DEPOLAR_ARASI_SIPARISLER item2 in _DepolarArasiSiparisHareketleri)
				{
					if (!(item2.ssip_stok_kod == stok.sto_kod) || item2.ssip_birim_pntr != stok.ekleme_bilgileri.sth_birim_pntr)
					{
						continue;
					}
					bool flag = false;
					foreach (BEDEN_HAREKETLERI item3 in item2.renk_beden_hareketleri)
					{
						if (item.BdnHar_BedenNo == item3.BdnHar_BedenNo && item3.BdnHar_HarGor - item3.BdnHar_TesMik >= num)
						{
							list.Add(item2.ssip_RECid_RECno);
							list2.Add(item2.ssip_Guid);
							list3.Add(num);
							BdnHar_RECid_RECno.Add(item3.BdnHar_RECid_RECno);
							BdnHar_Guid.Add(item3.BdnHar_Guid);
							num = 0.0;
							flag = true;
							break;
						}
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (num > 0.0)
				{
					foreach (DEPOLAR_ARASI_SIPARISLER item4 in Enumerable.ToList<DEPOLAR_ARASI_SIPARISLER>((IEnumerable<DEPOLAR_ARASI_SIPARISLER>)Enumerable.OrderBy<DEPOLAR_ARASI_SIPARISLER, double>((IEnumerable<DEPOLAR_ARASI_SIPARISLER>)_DepolarArasiSiparisHareketleri, (Func<DEPOLAR_ARASI_SIPARISLER, double>)((DEPOLAR_ARASI_SIPARISLER x) => x.KalanMiktar))))
					{
						bool flag2 = false;
						foreach (BEDEN_HAREKETLERI item5 in item4.renk_beden_hareketleri)
						{
							if (item.BdnHar_BedenNo == item5.BdnHar_BedenNo && item5.BdnHar_HarGor - item5.BdnHar_TesMik >= num)
							{
								list.Add(item4.ssip_RECid_RECno);
								list2.Add(item4.ssip_Guid);
								list3.Add(num);
								BdnHar_RECid_RECno.Add(item5.BdnHar_RECid_RECno);
								BdnHar_Guid.Add(item5.BdnHar_Guid);
								num = 0.0;
								flag2 = true;
								break;
							}
							if (flag2)
							{
								break;
							}
						}
						if (flag2)
						{
							break;
						}
					}
				}
				if (num != 0.0)
				{
					return enum_evrak_siparis_ekleme_sonuc.UrunMiktariSiparisMiktarindanFazla;
				}
			}
		}
		else
		{
			double num2 = stok.ekleme_bilgileri.Miktar;
			foreach (DEPOLAR_ARASI_SIPARISLER item6 in _DepolarArasiSiparisHareketleri)
			{
				if (item6.ssip_stok_kod == stok.sto_kod && item6.ssip_birim_pntr == stok.ekleme_bilgileri.sth_birim_pntr && item6.KalanMiktar >= num2)
				{
					list.Add(item6.ssip_RECid_RECno);
					list2.Add(item6.ssip_Guid);
					list3.Add(num2);
					num2 = 0.0;
					break;
				}
			}
			if (num2 > 0.0)
			{
				foreach (DEPOLAR_ARASI_SIPARISLER item7 in Enumerable.ToList<DEPOLAR_ARASI_SIPARISLER>((IEnumerable<DEPOLAR_ARASI_SIPARISLER>)Enumerable.OrderBy<DEPOLAR_ARASI_SIPARISLER, double>((IEnumerable<DEPOLAR_ARASI_SIPARISLER>)_DepolarArasiSiparisHareketleri, (Func<DEPOLAR_ARASI_SIPARISLER, double>)((DEPOLAR_ARASI_SIPARISLER x) => x.KalanMiktar))))
				{
					if (item7.ssip_stok_kod == stok.sto_kod && item7.KalanMiktar >= num2)
					{
						list.Add(item7.ssip_RECid_RECno);
						list2.Add(item7.ssip_Guid);
						list3.Add(num2);
						num2 = 0.0;
						break;
					}
				}
			}
			if (num2 != 0.0)
			{
				return enum_evrak_siparis_ekleme_sonuc.UrunMiktariSiparisMiktarindanFazla;
			}
		}
		int num3 = 0;
		int bedenhareket_islemsira = 0;
		if (AppBase.MikroVersiyonu > 15)
		{
			foreach (Guid siparis_guid in list2)
			{
				int num4 = -1;
				DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER = _DepolarArasiSiparisHareketleri.Find((DEPOLAR_ARASI_SIPARISLER r) => r.ssip_Guid == siparis_guid);
				int num5 = 0;
				foreach (STOK_HAREKETLERI item8 in _StokHareketleri)
				{
					if (item8.sth_subesip_uid == dEPOLAR_ARASI_SIPARISLER.ssip_Guid)
					{
						num4 = num5;
					}
					num5++;
				}
				if (num4 >= 0)
				{
					double num6 = list3[num3];
					_StokHareketleri[num4].sth_miktar += num6;
					if (Parametreler.Miktar2Arttir)
					{
						_StokHareketleri[num4].sth_miktar2++;
					}
					else
					{
						_StokHareketleri[num4].sth_miktar2 += 0.0;
					}
					_StokHareketleri[num4].sth_birim_pntr = dEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr;
					_StokHareketleri[num4].sth_aciklama = dEPOLAR_ARASI_SIPARISLER.ssip_aciklama;
					_StokHareketleri[num4].sth_tutar += dEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat * num6;
					_StokHareketleri[num4].sth_otvtutari += 0.0;
					_StokHareketleri[num4].sth_vergi += 0.0;
					_StokHareketleri[num4].sth_masraf_vergi += 0.0;
					_StokHareketleri[num4].sth_iskonto1 += 0.0;
					_StokHareketleri[num4].sth_iskonto2 += 0.0;
					_StokHareketleri[num4].sth_iskonto3 += 0.0;
					_StokHareketleri[num4].sth_iskonto4 += 0.0;
					_StokHareketleri[num4].sth_iskonto5 += 0.0;
					_StokHareketleri[num4].sth_iskonto6 += 0.0;
					_StokHareketleri[num4].sth_masraf1 += 0.0;
					_StokHareketleri[num4].sth_masraf2 += 0.0;
					_StokHareketleri[num4].sth_masraf3 += 0.0;
					_StokHareketleri[num4].sth_masraf4 += 0.0;
					_StokHareketleri[num4].sth_isk_mas1 = 0;
					_StokHareketleri[num4].sth_isk_mas2 = 0;
					_StokHareketleri[num4].sth_isk_mas3 = 0;
					_StokHareketleri[num4].sth_isk_mas4 = 0;
					_StokHareketleri[num4].sth_isk_mas5 = 0;
					_StokHareketleri[num4].sth_isk_mas6 = 0;
					_StokHareketleri[num4].sth_isk_mas7 = 0;
					_StokHareketleri[num4].sth_isk_mas8 = 0;
					_StokHareketleri[num4].sth_isk_mas9 = 0;
					_StokHareketleri[num4].sth_isk_mas10 = 0;
					_StokHareketleri[num4].sth_subesip_uid = dEPOLAR_ARASI_SIPARISLER.ssip_Guid;
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar += num6;
					if (dEPOLAR_ARASI_SIPARISLER.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI = dEPOLAR_ARASI_SIPARISLER.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_Guid == BdnHar_Guid[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI.BdnHar_TesMik += num6;
						bool flag3 = false;
						foreach (BEDEN_HAREKETLERI item9 in _StokHareketleri[num4].renk_beden_hareketleri)
						{
							if (bEDEN_HAREKETLERI.BdnHar_BedenNo == item9.BdnHar_BedenNo)
							{
								item9.BdnHar_HarGor += num6;
								flag3 = true;
								break;
							}
						}
						if (!flag3)
						{
							BEDEN_HAREKETLERI bEDEN_HAREKETLERI2 = new BEDEN_HAREKETLERI();
							bEDEN_HAREKETLERI2.BdnHar_HarGor = num6;
							bEDEN_HAREKETLERI2.BdnHar_BedenNo = bEDEN_HAREKETLERI.BdnHar_BedenNo;
							_StokHareketleri[num4].renk_beden_hareketleri.Add(bEDEN_HAREKETLERI2);
						}
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item10 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						_StokHareketleri[num4].stok_serinolari.Add(item10);
					}
				}
				else
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_RECid_DBCno = _DBCno;
					sTOK_HAREKETLERI.sth_create_user = mikrouserno;
					sTOK_HAREKETLERI.sth_lastup_user = mikrouserno;
					sTOK_HAREKETLERI.sth_firmano = Firma.fir_sirano;
					sTOK_HAREKETLERI.sth_subeno = Sube.Sube_no;
					sTOK_HAREKETLERI.sth_tarih = _evraktarih;
					sTOK_HAREKETLERI.sth_evrakno_seri = EvrakNoSeri;
					sTOK_HAREKETLERI.sth_evrakno_sira = EvrakNoSira;
					sTOK_HAREKETLERI.sth_satirno = _StokHareketleri.Count;
					sTOK_HAREKETLERI.sth_belge_no = BelgeNo;
					sTOK_HAREKETLERI.sth_belge_tarih = _belgetarih;
					sTOK_HAREKETLERI.sth_stok_kod = stok.sto_kod;
					sTOK_HAREKETLERI.sth_netagirlik = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
					{
						sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI.sth_giris_depo_no = _hedefdepo.dep_no;
					}
					else
					{
						sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI.sth_giris_depo_no = _kaynakdepo.dep_no;
					}
					sTOK_HAREKETLERI.sth_nakliyedeposu = 0;
					sTOK_HAREKETLERI.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
					{
						sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI.sth_giris_depo_no = _nakliyedepo.dep_no;
						sTOK_HAREKETLERI.sth_nakliyedeposu = _hedefdepo.dep_no;
						sTOK_HAREKETLERI.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					}
					sTOK_HAREKETLERI.sto_birim1_ad = stok.sto_birim1_ad;
					sTOK_HAREKETLERI.sto_birim2_ad = stok.sto_birim2_ad;
					sTOK_HAREKETLERI.sto_birim3_ad = stok.sto_birim3_ad;
					sTOK_HAREKETLERI.sto_birim4_ad = stok.sto_birim4_ad;
					sTOK_HAREKETLERI.sto_birim1_katsayi = stok.sto_birim1_katsayi;
					sTOK_HAREKETLERI.sto_birim2_katsayi = stok.sto_birim2_katsayi;
					sTOK_HAREKETLERI.sto_birim3_katsayi = stok.sto_birim3_katsayi;
					sTOK_HAREKETLERI.sto_birim4_katsayi = stok.sto_birim4_katsayi;
					sTOK_HAREKETLERI.sto_isim = stok.sto_isim;
					sTOK_HAREKETLERI.sth_maliyet_ana = 0.0;
					sTOK_HAREKETLERI.sth_maliyet_alternatif = 0.0;
					sTOK_HAREKETLERI.sth_maliyet_orjinal = 0.0;
					sTOK_HAREKETLERI.sth_parti_kodu = stok.ekleme_bilgileri.parti_kodu;
					sTOK_HAREKETLERI.sth_lot_no = 0;
					sTOK_HAREKETLERI.sth_cari_grup_no = Getchagrupno();
					switch (evraktipi)
					{
					case enum_GenelEvrakTipleri.SatisFaturasi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.CikisFaturasi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisFaturasi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.GirisFaturasi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisIrsaliyesi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.GirisIrsaliyesi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.SatisIrsaliyesi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.CikisIrsaliyesi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiSevk:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.DepoTransferFisi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.DepolarArasiNakliyeFisi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					}
					if (stok.ekleme_bilgileri.fiyat_farki_mi)
					{
						sTOK_HAREKETLERI.sth_cins = enum_sth_cins.DegerFarki;
					}
					sTOK_HAREKETLERI.sth_kons_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_kons_recid_recno = 0;
					sTOK_HAREKETLERI.sth_subesip_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_subesip_recid_recno = 0;
					sTOK_HAREKETLERI.sth_satistipi = enum_sth_satistipi.Kredilisatis;
					sTOK_HAREKETLERI.sth_proje_kodu = stok.ekleme_bilgileri.proje_kodu;
					sTOK_HAREKETLERI.sth_ihracat_kredi_kodu = "";
					sTOK_HAREKETLERI.sth_otv_pntr = 0;
					sTOK_HAREKETLERI.sth_otv_vergi = 0.0;
					sTOK_HAREKETLERI.sth_bkm_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_bkm_recid_recno = 0;
					sTOK_HAREKETLERI.sth_karsikons_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_karsikons_recid_recno = 0;
					sTOK_HAREKETLERI.sth_iade_evrak_seri = "";
					sTOK_HAREKETLERI.sth_iade_evrak_sira = 0;
					sTOK_HAREKETLERI.sth_diib_belge_no = "";
					sTOK_HAREKETLERI.sth_diib_satir_no = 0;
					sTOK_HAREKETLERI.sth_mensey_ulke_tipi = 0;
					sTOK_HAREKETLERI.sth_mensey_ulke_kodu = "";
					sTOK_HAREKETLERI.sth_brutagirlik = 0.0;
					sTOK_HAREKETLERI.sth_halrehmiktari = 0.0;
					sTOK_HAREKETLERI.sth_halrehfiyati = 0.0;
					sTOK_HAREKETLERI.sth_halsandikmiktari = 0.0;
					sTOK_HAREKETLERI.sth_halsandikfiyati = 0.0;
					sTOK_HAREKETLERI.sth_halsandikkdvtutari = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_1 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_2 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_3 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_4 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_5 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_1 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_2 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_3 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_4 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_5 = 0.0;
					sTOK_HAREKETLERI.sth_yat_tes_kodu = "";
					sTOK_HAREKETLERI.sth_oiv_pntr = 0;
					sTOK_HAREKETLERI.sth_oiv_vergi = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
					{
						sTOK_HAREKETLERI.sth_malkbl_sevk_tarihi = _sevkteslimtarihi;
					}
					else
					{
						sTOK_HAREKETLERI.sth_malkbl_sevk_tarihi = _evraktarih;
					}
					sTOK_HAREKETLERI.sth_oivvergisiz_fl = false;
					sTOK_HAREKETLERI.sth_otvvergisiz_fl = false;
					sTOK_HAREKETLERI.sth_disticaret_turu = enum_sth_disticaret_turu.ToptanYurticiTicaret;
					sTOK_HAREKETLERI.sth_fiyat_liste_no = FiyatListesi.sfl_sirano;
					sTOK_HAREKETLERI.sth_fis_sirano2 = 0;
					sTOK_HAREKETLERI.sth_rez_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_rez_recid_recno = 0;
					sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_seri = "";
					sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_sira = 0;
					sTOK_HAREKETLERI.sth_fiyfark_esas_satir_no = 0;
					sTOK_HAREKETLERI.sth_optamam_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_optamam_recid_recno = 0;
					sTOK_HAREKETLERI.sth_oivtutari = 0.0;
					sTOK_HAREKETLERI.sth_Tevkifat_turu = enum_sth_Tevkifat_turu.TevkifatYok;
					sTOK_HAREKETLERI.sth_HalKomisyonuKdv = 0.0;
					sTOK_HAREKETLERI.sth_iadeTlp_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_iadeTlp_recid_recno = 0;
					sTOK_HAREKETLERI.sth_HalSatisRecid_dbcno = 0;
					sTOK_HAREKETLERI.sth_HalSatisRecid_recno = 0;
					sTOK_HAREKETLERI.sth_ciroprim_dbcno = 0;
					sTOK_HAREKETLERI.sth_ciroprim_recno = 0;
					sTOK_HAREKETLERI.sth_yetkili_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_yetkili_recid_recno = 0;
					sTOK_HAREKETLERI.sth_taxfree_fl = false;
					sTOK_HAREKETLERI.sth_HalRusum = 0.0;
					double num7 = list3[num3];
					sTOK_HAREKETLERI.sth_miktar += num7;
					if (Parametreler.Miktar2Arttir)
					{
						sTOK_HAREKETLERI.sth_miktar2 = 1.0;
					}
					else
					{
						sTOK_HAREKETLERI.sth_miktar2 = 0.0;
					}
					sTOK_HAREKETLERI.sth_birim_pntr = dEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr;
					sTOK_HAREKETLERI.sth_aciklama = dEPOLAR_ARASI_SIPARISLER.ssip_aciklama;
					sTOK_HAREKETLERI.sth_tutar += dEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat * num7;
					sTOK_HAREKETLERI.sth_otvtutari = 0.0;
					sTOK_HAREKETLERI.sth_vergi += 0.0;
					sTOK_HAREKETLERI.sth_masraf_vergi += 0.0;
					sTOK_HAREKETLERI.sth_iskonto1 += 0.0;
					sTOK_HAREKETLERI.sth_iskonto2 += 0.0;
					sTOK_HAREKETLERI.sth_iskonto3 += 0.0;
					sTOK_HAREKETLERI.sth_iskonto4 += 0.0;
					sTOK_HAREKETLERI.sth_iskonto5 += 0.0;
					sTOK_HAREKETLERI.sth_iskonto6 += 0.0;
					sTOK_HAREKETLERI.sth_masraf1 += 0.0;
					sTOK_HAREKETLERI.sth_masraf2 += 0.0;
					sTOK_HAREKETLERI.sth_masraf3 += 0.0;
					sTOK_HAREKETLERI.sth_masraf4 += 0.0;
					sTOK_HAREKETLERI.sth_isk_mas1 = 0;
					sTOK_HAREKETLERI.sth_isk_mas2 = 0;
					sTOK_HAREKETLERI.sth_isk_mas3 = 0;
					sTOK_HAREKETLERI.sth_isk_mas4 = 0;
					sTOK_HAREKETLERI.sth_isk_mas5 = 0;
					sTOK_HAREKETLERI.sth_isk_mas6 = 0;
					sTOK_HAREKETLERI.sth_isk_mas7 = 0;
					sTOK_HAREKETLERI.sth_isk_mas8 = 0;
					sTOK_HAREKETLERI.sth_isk_mas9 = 0;
					sTOK_HAREKETLERI.sth_isk_mas10 = 0;
					sTOK_HAREKETLERI.sth_vergi_pntr = 0;
					sTOK_HAREKETLERI.sth_masraf_vergi_pntr = 0;
					sTOK_HAREKETLERI.sth_plasiyer_kodu = "";
					sTOK_HAREKETLERI.sth_cari_kodu = "";
					sTOK_HAREKETLERI.sth_kur_tarihi = kur.dov_tarih;
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = dovizcinsi;
					sTOK_HAREKETLERI.sth_har_doviz_kuru = 1.0;
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = alternatifdovizkuru.dov_fiyat;
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = stok.sto_doviz_cinsi;
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = stok.ekleme_bilgileri.StokDovizCinsiKuru;
					sTOK_HAREKETLERI.sth_odeme_op = 0;
					sTOK_HAREKETLERI.sth_aciklama = dEPOLAR_ARASI_SIPARISLER.ssip_aciklama;
					sTOK_HAREKETLERI.sth_cari_srm_merkezi = dEPOLAR_ARASI_SIPARISLER.ssip_sormerkezi;
					sTOK_HAREKETLERI.sth_stok_srm_merkezi = dEPOLAR_ARASI_SIPARISLER.ssip_sormerkezi;
					sTOK_HAREKETLERI.sth_adres_no = 1;
					sTOK_HAREKETLERI.sth_sip_recid_recno = 0;
					sTOK_HAREKETLERI.sth_fat_recid_recno = 0;
					sTOK_HAREKETLERI.sth_subesip_uid = dEPOLAR_ARASI_SIPARISLER.ssip_Guid;
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar += num7;
					if (dEPOLAR_ARASI_SIPARISLER.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI3 = dEPOLAR_ARASI_SIPARISLER.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_Guid == BdnHar_Guid[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI3.BdnHar_TesMik += num7;
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI4 = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI4.BdnHar_HarGor = num7;
						bEDEN_HAREKETLERI4.BdnHar_BedenNo = bEDEN_HAREKETLERI3.BdnHar_BedenNo;
						sTOK_HAREKETLERI.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI4);
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item11 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						sTOK_HAREKETLERI.stok_serinolari.Add(item11);
					}
					_StokHareketleri.Add(sTOK_HAREKETLERI);
				}
				num3++;
			}
		}
		else
		{
			foreach (int siparis_recno in list)
			{
				int num8 = -1;
				DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER2 = _DepolarArasiSiparisHareketleri.Find((DEPOLAR_ARASI_SIPARISLER r) => r.ssip_RECid_RECno == siparis_recno);
				int num9 = 0;
				foreach (STOK_HAREKETLERI item12 in _StokHareketleri)
				{
					if (item12.sth_subesip_recid_recno == dEPOLAR_ARASI_SIPARISLER2.ssip_RECid_RECno)
					{
						num8 = num9;
					}
					num9++;
				}
				if (num8 >= 0)
				{
					double num10 = list3[num3];
					_StokHareketleri[num8].sth_miktar += num10;
					if (Parametreler.Miktar2Arttir)
					{
						_StokHareketleri[num8].sth_miktar2++;
					}
					else
					{
						_StokHareketleri[num8].sth_miktar2 += 0.0;
					}
					_StokHareketleri[num8].sth_birim_pntr = dEPOLAR_ARASI_SIPARISLER2.ssip_birim_pntr;
					_StokHareketleri[num8].sth_aciklama = dEPOLAR_ARASI_SIPARISLER2.ssip_aciklama;
					_StokHareketleri[num8].sth_tutar += dEPOLAR_ARASI_SIPARISLER2.ssip_b_fiyat * num10;
					_StokHareketleri[num8].sth_otvtutari += 0.0;
					_StokHareketleri[num8].sth_vergi += 0.0;
					_StokHareketleri[num8].sth_masraf_vergi += 0.0;
					_StokHareketleri[num8].sth_iskonto1 += 0.0;
					_StokHareketleri[num8].sth_iskonto2 += 0.0;
					_StokHareketleri[num8].sth_iskonto3 += 0.0;
					_StokHareketleri[num8].sth_iskonto4 += 0.0;
					_StokHareketleri[num8].sth_iskonto5 += 0.0;
					_StokHareketleri[num8].sth_iskonto6 += 0.0;
					_StokHareketleri[num8].sth_masraf1 += 0.0;
					_StokHareketleri[num8].sth_masraf2 += 0.0;
					_StokHareketleri[num8].sth_masraf3 += 0.0;
					_StokHareketleri[num8].sth_masraf4 += 0.0;
					_StokHareketleri[num8].sth_isk_mas1 = 0;
					_StokHareketleri[num8].sth_isk_mas2 = 0;
					_StokHareketleri[num8].sth_isk_mas3 = 0;
					_StokHareketleri[num8].sth_isk_mas4 = 0;
					_StokHareketleri[num8].sth_isk_mas5 = 0;
					_StokHareketleri[num8].sth_isk_mas6 = 0;
					_StokHareketleri[num8].sth_isk_mas7 = 0;
					_StokHareketleri[num8].sth_isk_mas8 = 0;
					_StokHareketleri[num8].sth_isk_mas9 = 0;
					_StokHareketleri[num8].sth_isk_mas10 = 0;
					_StokHareketleri[num8].sth_subesip_recid_recno = dEPOLAR_ARASI_SIPARISLER2.ssip_RECid_RECno;
					_StokHareketleri[num8].sth_subesip_recid_dbcno = dEPOLAR_ARASI_SIPARISLER2.ssip_RECid_DBCno;
					dEPOLAR_ARASI_SIPARISLER2.ssip_teslim_miktar += num10;
					if (dEPOLAR_ARASI_SIPARISLER2.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI5 = dEPOLAR_ARASI_SIPARISLER2.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_RECid_RECno == BdnHar_RECid_RECno[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI5.BdnHar_TesMik += num10;
						bool flag4 = false;
						foreach (BEDEN_HAREKETLERI item13 in _StokHareketleri[num8].renk_beden_hareketleri)
						{
							if (bEDEN_HAREKETLERI5.BdnHar_BedenNo == item13.BdnHar_BedenNo)
							{
								item13.BdnHar_HarGor += num10;
								flag4 = true;
								break;
							}
						}
						if (!flag4)
						{
							BEDEN_HAREKETLERI bEDEN_HAREKETLERI6 = new BEDEN_HAREKETLERI();
							bEDEN_HAREKETLERI6.BdnHar_HarGor = num10;
							bEDEN_HAREKETLERI6.BdnHar_BedenNo = bEDEN_HAREKETLERI5.BdnHar_BedenNo;
							_StokHareketleri[num8].renk_beden_hareketleri.Add(bEDEN_HAREKETLERI6);
						}
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item14 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						_StokHareketleri[num8].stok_serinolari.Add(item14);
					}
				}
				else
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI2 = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI2.sth_RECid_DBCno = _DBCno;
					sTOK_HAREKETLERI2.sth_create_user = mikrouserno;
					sTOK_HAREKETLERI2.sth_lastup_user = mikrouserno;
					sTOK_HAREKETLERI2.sth_firmano = Firma.fir_sirano;
					sTOK_HAREKETLERI2.sth_subeno = Sube.Sube_no;
					sTOK_HAREKETLERI2.sth_tarih = _evraktarih;
					sTOK_HAREKETLERI2.sth_evrakno_seri = EvrakNoSeri;
					sTOK_HAREKETLERI2.sth_evrakno_sira = EvrakNoSira;
					sTOK_HAREKETLERI2.sth_satirno = _StokHareketleri.Count;
					sTOK_HAREKETLERI2.sth_belge_no = BelgeNo;
					sTOK_HAREKETLERI2.sth_belge_tarih = _belgetarih;
					sTOK_HAREKETLERI2.sth_stok_kod = stok.sto_kod;
					sTOK_HAREKETLERI2.sth_netagirlik = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
					{
						sTOK_HAREKETLERI2.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI2.sth_giris_depo_no = _hedefdepo.dep_no;
					}
					else
					{
						sTOK_HAREKETLERI2.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI2.sth_giris_depo_no = _kaynakdepo.dep_no;
					}
					sTOK_HAREKETLERI2.sth_nakliyedeposu = 0;
					sTOK_HAREKETLERI2.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
					{
						sTOK_HAREKETLERI2.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI2.sth_giris_depo_no = _nakliyedepo.dep_no;
						sTOK_HAREKETLERI2.sth_nakliyedeposu = _hedefdepo.dep_no;
						sTOK_HAREKETLERI2.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					}
					sTOK_HAREKETLERI2.sto_birim1_ad = stok.sto_birim1_ad;
					sTOK_HAREKETLERI2.sto_birim2_ad = stok.sto_birim2_ad;
					sTOK_HAREKETLERI2.sto_birim3_ad = stok.sto_birim3_ad;
					sTOK_HAREKETLERI2.sto_birim4_ad = stok.sto_birim4_ad;
					sTOK_HAREKETLERI2.sto_birim1_katsayi = stok.sto_birim1_katsayi;
					sTOK_HAREKETLERI2.sto_birim2_katsayi = stok.sto_birim2_katsayi;
					sTOK_HAREKETLERI2.sto_birim3_katsayi = stok.sto_birim3_katsayi;
					sTOK_HAREKETLERI2.sto_birim4_katsayi = stok.sto_birim4_katsayi;
					sTOK_HAREKETLERI2.sto_isim = stok.sto_isim;
					sTOK_HAREKETLERI2.sth_maliyet_ana = 0.0;
					sTOK_HAREKETLERI2.sth_maliyet_alternatif = 0.0;
					sTOK_HAREKETLERI2.sth_maliyet_orjinal = 0.0;
					sTOK_HAREKETLERI2.sth_parti_kodu = stok.ekleme_bilgileri.parti_kodu;
					sTOK_HAREKETLERI2.sth_lot_no = 0;
					sTOK_HAREKETLERI2.sth_cari_grup_no = Getchagrupno();
					switch (evraktipi)
					{
					case enum_GenelEvrakTipleri.SatisFaturasi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.CikisFaturasi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisFaturasi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.GirisFaturasi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisIrsaliyesi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.GirisIrsaliyesi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.SatisIrsaliyesi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.CikisIrsaliyesi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiSevk:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.DepoTransferFisi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.DepolarArasiNakliyeFisi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					}
					if (stok.ekleme_bilgileri.fiyat_farki_mi)
					{
						sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.DegerFarki;
					}
					sTOK_HAREKETLERI2.sth_kons_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_kons_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_subesip_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_subesip_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_satistipi = enum_sth_satistipi.Kredilisatis;
					sTOK_HAREKETLERI2.sth_proje_kodu = stok.ekleme_bilgileri.proje_kodu;
					sTOK_HAREKETLERI2.sth_ihracat_kredi_kodu = "";
					sTOK_HAREKETLERI2.sth_otv_pntr = 0;
					sTOK_HAREKETLERI2.sth_otv_vergi = 0.0;
					sTOK_HAREKETLERI2.sth_bkm_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_bkm_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_karsikons_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_karsikons_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_iade_evrak_seri = "";
					sTOK_HAREKETLERI2.sth_iade_evrak_sira = 0;
					sTOK_HAREKETLERI2.sth_diib_belge_no = "";
					sTOK_HAREKETLERI2.sth_diib_satir_no = 0;
					sTOK_HAREKETLERI2.sth_mensey_ulke_tipi = 0;
					sTOK_HAREKETLERI2.sth_mensey_ulke_kodu = "";
					sTOK_HAREKETLERI2.sth_brutagirlik = 0.0;
					sTOK_HAREKETLERI2.sth_halrehmiktari = 0.0;
					sTOK_HAREKETLERI2.sth_halrehfiyati = 0.0;
					sTOK_HAREKETLERI2.sth_halsandikmiktari = 0.0;
					sTOK_HAREKETLERI2.sth_halsandikfiyati = 0.0;
					sTOK_HAREKETLERI2.sth_halsandikkdvtutari = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_1 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_2 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_3 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_4 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_5 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_1 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_2 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_3 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_4 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_5 = 0.0;
					sTOK_HAREKETLERI2.sth_yat_tes_kodu = "";
					sTOK_HAREKETLERI2.sth_oiv_pntr = 0;
					sTOK_HAREKETLERI2.sth_oiv_vergi = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
					{
						sTOK_HAREKETLERI2.sth_malkbl_sevk_tarihi = _sevkteslimtarihi;
					}
					else
					{
						sTOK_HAREKETLERI2.sth_malkbl_sevk_tarihi = _evraktarih;
					}
					sTOK_HAREKETLERI2.sth_oivvergisiz_fl = false;
					sTOK_HAREKETLERI2.sth_otvvergisiz_fl = false;
					sTOK_HAREKETLERI2.sth_disticaret_turu = enum_sth_disticaret_turu.ToptanYurticiTicaret;
					sTOK_HAREKETLERI2.sth_fiyat_liste_no = FiyatListesi.sfl_sirano;
					sTOK_HAREKETLERI2.sth_fis_sirano2 = 0;
					sTOK_HAREKETLERI2.sth_rez_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_rez_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_fiyfark_esas_evrak_seri = "";
					sTOK_HAREKETLERI2.sth_fiyfark_esas_evrak_sira = 0;
					sTOK_HAREKETLERI2.sth_fiyfark_esas_satir_no = 0;
					sTOK_HAREKETLERI2.sth_optamam_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_optamam_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_oivtutari = 0.0;
					sTOK_HAREKETLERI2.sth_Tevkifat_turu = enum_sth_Tevkifat_turu.TevkifatYok;
					sTOK_HAREKETLERI2.sth_HalKomisyonuKdv = 0.0;
					sTOK_HAREKETLERI2.sth_iadeTlp_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_iadeTlp_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_HalSatisRecid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_HalSatisRecid_recno = 0;
					sTOK_HAREKETLERI2.sth_ciroprim_dbcno = 0;
					sTOK_HAREKETLERI2.sth_ciroprim_recno = 0;
					sTOK_HAREKETLERI2.sth_yetkili_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_yetkili_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_taxfree_fl = false;
					sTOK_HAREKETLERI2.sth_HalRusum = 0.0;
					double num11 = list3[num3];
					sTOK_HAREKETLERI2.sth_miktar += num11;
					if (Parametreler.Miktar2Arttir)
					{
						sTOK_HAREKETLERI2.sth_miktar2 = 1.0;
					}
					else
					{
						sTOK_HAREKETLERI2.sth_miktar2 = 0.0;
					}
					sTOK_HAREKETLERI2.sth_birim_pntr = dEPOLAR_ARASI_SIPARISLER2.ssip_birim_pntr;
					sTOK_HAREKETLERI2.sth_aciklama = dEPOLAR_ARASI_SIPARISLER2.ssip_aciklama;
					sTOK_HAREKETLERI2.sth_tutar += dEPOLAR_ARASI_SIPARISLER2.ssip_b_fiyat * num11;
					sTOK_HAREKETLERI2.sth_otvtutari = 0.0;
					sTOK_HAREKETLERI2.sth_vergi += 0.0;
					sTOK_HAREKETLERI2.sth_masraf_vergi += 0.0;
					sTOK_HAREKETLERI2.sth_iskonto1 += 0.0;
					sTOK_HAREKETLERI2.sth_iskonto2 += 0.0;
					sTOK_HAREKETLERI2.sth_iskonto3 += 0.0;
					sTOK_HAREKETLERI2.sth_iskonto4 += 0.0;
					sTOK_HAREKETLERI2.sth_iskonto5 += 0.0;
					sTOK_HAREKETLERI2.sth_iskonto6 += 0.0;
					sTOK_HAREKETLERI2.sth_masraf1 += 0.0;
					sTOK_HAREKETLERI2.sth_masraf2 += 0.0;
					sTOK_HAREKETLERI2.sth_masraf3 += 0.0;
					sTOK_HAREKETLERI2.sth_masraf4 += 0.0;
					sTOK_HAREKETLERI2.sth_isk_mas1 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas2 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas3 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas4 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas5 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas6 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas7 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas8 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas9 = 0;
					sTOK_HAREKETLERI2.sth_isk_mas10 = 0;
					sTOK_HAREKETLERI2.sth_vergi_pntr = 0;
					sTOK_HAREKETLERI2.sth_masraf_vergi_pntr = 0;
					sTOK_HAREKETLERI2.sth_plasiyer_kodu = "";
					sTOK_HAREKETLERI2.sth_cari_kodu = "";
					sTOK_HAREKETLERI2.sth_kur_tarihi = kur.dov_tarih;
					sTOK_HAREKETLERI2.sth_har_doviz_cinsi = dovizcinsi;
					sTOK_HAREKETLERI2.sth_har_doviz_kuru = 1.0;
					sTOK_HAREKETLERI2.sth_alt_doviz_kuru = alternatifdovizkuru.dov_fiyat;
					sTOK_HAREKETLERI2.sth_stok_doviz_cinsi = stok.sto_doviz_cinsi;
					sTOK_HAREKETLERI2.sth_stok_doviz_kuru = stok.ekleme_bilgileri.StokDovizCinsiKuru;
					sTOK_HAREKETLERI2.sth_odeme_op = 0;
					sTOK_HAREKETLERI2.sth_aciklama = dEPOLAR_ARASI_SIPARISLER2.ssip_aciklama;
					sTOK_HAREKETLERI2.sth_cari_srm_merkezi = dEPOLAR_ARASI_SIPARISLER2.ssip_sormerkezi;
					sTOK_HAREKETLERI2.sth_stok_srm_merkezi = dEPOLAR_ARASI_SIPARISLER2.ssip_sormerkezi;
					sTOK_HAREKETLERI2.sth_adres_no = 1;
					sTOK_HAREKETLERI2.sth_sip_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_fat_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_subesip_recid_recno = dEPOLAR_ARASI_SIPARISLER2.ssip_RECid_RECno;
					sTOK_HAREKETLERI2.sth_subesip_recid_dbcno = dEPOLAR_ARASI_SIPARISLER2.ssip_RECid_DBCno;
					dEPOLAR_ARASI_SIPARISLER2.ssip_teslim_miktar += num11;
					if (dEPOLAR_ARASI_SIPARISLER2.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI7 = dEPOLAR_ARASI_SIPARISLER2.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_RECid_RECno == BdnHar_RECid_RECno[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI7.BdnHar_TesMik += num11;
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI8 = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI8.BdnHar_HarGor = num11;
						bEDEN_HAREKETLERI8.BdnHar_BedenNo = bEDEN_HAREKETLERI7.BdnHar_BedenNo;
						sTOK_HAREKETLERI2.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI8);
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item15 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						sTOK_HAREKETLERI2.stok_serinolari.Add(item15);
					}
					_StokHareketleri.Add(sTOK_HAREKETLERI2);
				}
				num3++;
			}
		}
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunNormalSiparisKarsilamaNormal(Stok stok)
	{
		List<int> list = new List<int>();
		List<Guid> list2 = new List<Guid>();
		List<int> BdnHar_RECid_RECno = new List<int>();
		List<Guid> BdnHar_Guid = new List<Guid>();
		List<double> list3 = new List<double>();
		if (stok.ekleme_bilgileri.renk_beden_hareketleri.Count > 0)
		{
			foreach (BEDEN_HAREKETLERI item in stok.ekleme_bilgileri.renk_beden_hareketleri)
			{
				double num = item.BdnHar_HarGor - item.BdnHar_TesMik;
				foreach (SIPARISLER item2 in _Siparisler)
				{
					if (!(item2.sip_stok_kod == stok.sto_kod) || item2.sip_birim_pntr != stok.ekleme_bilgileri.sth_birim_pntr)
					{
						continue;
					}
					bool flag = false;
					foreach (BEDEN_HAREKETLERI item3 in item2.renk_beden_hareketleri)
					{
						if (item.BdnHar_BedenNo == item3.BdnHar_BedenNo && item3.BdnHar_HarGor - item3.BdnHar_TesMik >= num)
						{
							list.Add(item2.sip_RECid_RECno);
							list2.Add(item2.sip_Guid);
							list3.Add(num);
							BdnHar_RECid_RECno.Add(item3.BdnHar_RECid_RECno);
							BdnHar_Guid.Add(item3.BdnHar_Guid);
							num = 0.0;
							flag = true;
							break;
						}
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (num > 0.0)
				{
					foreach (SIPARISLER item4 in Enumerable.ToList<SIPARISLER>((IEnumerable<SIPARISLER>)Enumerable.OrderBy<SIPARISLER, double>((IEnumerable<SIPARISLER>)_Siparisler, (Func<SIPARISLER, double>)((SIPARISLER x) => x.KalanMiktar))))
					{
						bool flag2 = false;
						foreach (BEDEN_HAREKETLERI item5 in item4.renk_beden_hareketleri)
						{
							if (item.BdnHar_BedenNo == item5.BdnHar_BedenNo && item5.BdnHar_HarGor - item5.BdnHar_TesMik >= num)
							{
								list.Add(item4.sip_RECid_RECno);
								list2.Add(item4.sip_Guid);
								list3.Add(num);
								BdnHar_RECid_RECno.Add(item5.BdnHar_RECid_RECno);
								BdnHar_Guid.Add(item5.BdnHar_Guid);
								num = 0.0;
								flag2 = true;
								break;
							}
							if (flag2)
							{
								break;
							}
						}
						if (flag2)
						{
							break;
						}
					}
				}
				if (num != 0.0)
				{
					return enum_evrak_siparis_ekleme_sonuc.UrunMiktariSiparisMiktarindanFazla;
				}
			}
		}
		else
		{
			double num2 = stok.ekleme_bilgileri.Miktar;
			foreach (SIPARISLER item6 in _Siparisler)
			{
				if (item6.sip_stok_kod == stok.sto_kod && item6.sip_birim_pntr == stok.ekleme_bilgileri.sth_birim_pntr && item6.KalanMiktar >= num2)
				{
					list.Add(item6.sip_RECid_RECno);
					list2.Add(item6.sip_Guid);
					list3.Add(num2);
					num2 = 0.0;
					break;
				}
			}
			if (num2 > 0.0)
			{
				foreach (SIPARISLER item7 in Enumerable.ToList<SIPARISLER>((IEnumerable<SIPARISLER>)Enumerable.OrderBy<SIPARISLER, double>((IEnumerable<SIPARISLER>)_Siparisler, (Func<SIPARISLER, double>)((SIPARISLER x) => x.KalanMiktar))))
				{
					if (item7.sip_stok_kod == stok.sto_kod && item7.KalanMiktar >= num2)
					{
						list.Add(item7.sip_RECid_RECno);
						list2.Add(item7.sip_Guid);
						list3.Add(num2);
						num2 = 0.0;
						break;
					}
				}
			}
			if (num2 != 0.0)
			{
				return enum_evrak_siparis_ekleme_sonuc.UrunMiktariSiparisMiktarindanFazla;
			}
		}
		int num3 = 0;
		int bedenhareket_islemsira = 0;
		if (AppBase.MikroVersiyonu > 15)
		{
			foreach (Guid siparis_guid in list2)
			{
				int num4 = -1;
				SIPARISLER sIPARISLER = _Siparisler.Find((SIPARISLER r) => r.sip_Guid == siparis_guid);
				int num5 = 0;
				foreach (STOK_HAREKETLERI item8 in _StokHareketleri)
				{
					if (item8.sth_sip_uid == sIPARISLER.sip_Guid)
					{
						num4 = num5;
					}
					num5++;
				}
				if (num4 >= 0)
				{
					double num6 = list3[num3];
					_StokHareketleri[num4].sth_miktar += num6;
					if (Parametreler.Miktar2Arttir)
					{
						_StokHareketleri[num4].sth_miktar2++;
					}
					else
					{
						_StokHareketleri[num4].sth_miktar2 += 0.0;
					}
					_StokHareketleri[num4].sth_birim_pntr = sIPARISLER.sip_birim_pntr;
					_StokHareketleri[num4].sth_aciklama = sIPARISLER.sip_aciklama;
					_StokHareketleri[num4].sth_tutar += sIPARISLER.sip_b_fiyat * num6;
					_StokHareketleri[num4].sth_otvtutari += sIPARISLER.sip_otvtutari / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_otv_pntr = sIPARISLER.sip_Otv_Pntr;
					double yuzde = Parametreler.vergitanimlari[_StokHareketleri[num4].sth_otv_pntr].Yuzde;
					_StokHareketleri[num4].sth_otv_vergi += sIPARISLER.sip_otvtutari / sIPARISLER.sip_miktar * num6 / 100.0 * yuzde;
					_StokHareketleri[num4].sth_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
					_StokHareketleri[num4].sth_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
					_StokHareketleri[num4].sth_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
					_StokHareketleri[num4].sth_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
					_StokHareketleri[num4].sth_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
					_StokHareketleri[num4].sth_FormulMiktar += stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
					_StokHareketleri[num4].sth_vergi += sIPARISLER.sip_vergi / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_masraf_vergi += sIPARISLER.sip_masvergi / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_iskonto1 += sIPARISLER.sip_iskonto_1 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_iskonto2 += sIPARISLER.sip_iskonto_2 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_iskonto3 += sIPARISLER.sip_iskonto_3 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_iskonto4 += sIPARISLER.sip_iskonto_4 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_iskonto5 += sIPARISLER.sip_iskonto_5 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_iskonto6 += sIPARISLER.sip_iskonto_6 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_masraf1 += sIPARISLER.sip_masraf_1 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_masraf2 += sIPARISLER.sip_masraf_2 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_masraf3 += sIPARISLER.sip_masraf_3 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_masraf4 += sIPARISLER.sip_masraf_4 / sIPARISLER.sip_miktar * num6;
					_StokHareketleri[num4].sth_isk_mas1 = sIPARISLER.sip_iskonto1;
					_StokHareketleri[num4].sth_isk_mas2 = sIPARISLER.sip_iskonto2;
					_StokHareketleri[num4].sth_isk_mas3 = sIPARISLER.sip_iskonto3;
					_StokHareketleri[num4].sth_isk_mas4 = sIPARISLER.sip_iskonto4;
					_StokHareketleri[num4].sth_isk_mas5 = sIPARISLER.sip_iskonto5;
					_StokHareketleri[num4].sth_isk_mas6 = sIPARISLER.sip_iskonto6;
					_StokHareketleri[num4].sth_isk_mas7 = sIPARISLER.sip_masraf1;
					_StokHareketleri[num4].sth_isk_mas8 = sIPARISLER.sip_masraf2;
					_StokHareketleri[num4].sth_isk_mas9 = sIPARISLER.sip_masraf3;
					_StokHareketleri[num4].sth_isk_mas10 = sIPARISLER.sip_masraf4;
					if (EximKodu != "")
					{
						_StokHareketleri[num4].sth_vergi_pntr = 0;
						_StokHareketleri[num4].sth_vergi = 0.0;
						_StokHareketleri[num4].sth_vergisiz_fl = true;
						_StokHareketleri[num4].sth_otvtutari = 0.0;
						_StokHareketleri[num4].sth_otvvergisiz_fl = true;
						_StokHareketleri[num4].sth_oiv_pntr = 0;
						_StokHareketleri[num4].sth_oiv_vergi = 0.0;
						_StokHareketleri[num4].sth_oivvergisiz_fl = true;
						_StokHareketleri[num4].sth_oivtutari = 0.0;
					}
					_StokHareketleri[num4].sth_sip_uid = sIPARISLER.sip_Guid;
					sIPARISLER.sip_teslim_miktar += num6;
					if (sIPARISLER.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI = sIPARISLER.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_Guid == BdnHar_Guid[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI.BdnHar_TesMik += num6;
						bool flag3 = false;
						foreach (BEDEN_HAREKETLERI item9 in _StokHareketleri[num4].renk_beden_hareketleri)
						{
							if (bEDEN_HAREKETLERI.BdnHar_BedenNo == item9.BdnHar_BedenNo)
							{
								item9.BdnHar_HarGor += num6;
								flag3 = true;
								break;
							}
						}
						if (!flag3)
						{
							BEDEN_HAREKETLERI bEDEN_HAREKETLERI2 = new BEDEN_HAREKETLERI();
							bEDEN_HAREKETLERI2.BdnHar_HarGor = num6;
							bEDEN_HAREKETLERI2.BdnHar_BedenNo = bEDEN_HAREKETLERI.BdnHar_BedenNo;
							_StokHareketleri[num4].renk_beden_hareketleri.Add(bEDEN_HAREKETLERI2);
						}
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item10 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						_StokHareketleri[num4].stok_serinolari.Add(item10);
					}
				}
				else
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_RECid_DBCno = _DBCno;
					sTOK_HAREKETLERI.sth_create_user = mikrouserno;
					sTOK_HAREKETLERI.sth_lastup_user = mikrouserno;
					sTOK_HAREKETLERI.sth_firmano = Firma.fir_sirano;
					sTOK_HAREKETLERI.sth_subeno = Sube.Sube_no;
					sTOK_HAREKETLERI.sth_tarih = _evraktarih;
					sTOK_HAREKETLERI.sth_evrakno_seri = EvrakNoSeri;
					sTOK_HAREKETLERI.sth_evrakno_sira = EvrakNoSira;
					sTOK_HAREKETLERI.sth_satirno = _StokHareketleri.Count;
					sTOK_HAREKETLERI.sth_belge_no = BelgeNo;
					sTOK_HAREKETLERI.sth_belge_tarih = _belgetarih;
					sTOK_HAREKETLERI.sth_stok_kod = stok.sto_kod;
					sTOK_HAREKETLERI.sth_netagirlik = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
					{
						sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI.sth_giris_depo_no = _hedefdepo.dep_no;
					}
					else
					{
						sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI.sth_giris_depo_no = _kaynakdepo.dep_no;
					}
					sTOK_HAREKETLERI.sth_nakliyedeposu = 0;
					sTOK_HAREKETLERI.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
					{
						sTOK_HAREKETLERI.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI.sth_giris_depo_no = _nakliyedepo.dep_no;
						sTOK_HAREKETLERI.sth_nakliyedeposu = _hedefdepo.dep_no;
						sTOK_HAREKETLERI.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					}
					sTOK_HAREKETLERI.sto_birim1_ad = stok.sto_birim1_ad;
					sTOK_HAREKETLERI.sto_birim2_ad = stok.sto_birim2_ad;
					sTOK_HAREKETLERI.sto_birim3_ad = stok.sto_birim3_ad;
					sTOK_HAREKETLERI.sto_birim4_ad = stok.sto_birim4_ad;
					sTOK_HAREKETLERI.sto_birim1_katsayi = stok.sto_birim1_katsayi;
					sTOK_HAREKETLERI.sto_birim2_katsayi = stok.sto_birim2_katsayi;
					sTOK_HAREKETLERI.sto_birim3_katsayi = stok.sto_birim3_katsayi;
					sTOK_HAREKETLERI.sto_birim4_katsayi = stok.sto_birim4_katsayi;
					sTOK_HAREKETLERI.sto_isim = stok.sto_isim;
					sTOK_HAREKETLERI.sth_maliyet_ana = 0.0;
					sTOK_HAREKETLERI.sth_maliyet_alternatif = 0.0;
					sTOK_HAREKETLERI.sth_maliyet_orjinal = 0.0;
					sTOK_HAREKETLERI.sth_parti_kodu = stok.ekleme_bilgileri.parti_kodu;
					sTOK_HAREKETLERI.sth_lot_no = stok.ekleme_bilgileri.lot_no;
					sTOK_HAREKETLERI.sth_cari_grup_no = Getchagrupno();
					switch (evraktipi)
					{
					case enum_GenelEvrakTipleri.SatisFaturasi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.CikisFaturasi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisFaturasi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.GirisFaturasi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisIrsaliyesi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.GirisIrsaliyesi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.SatisIrsaliyesi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.CikisIrsaliyesi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiSevk:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.DepoTransferFisi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
						sTOK_HAREKETLERI.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI.sth_evraktip = enum_sth_evraktip.DepolarArasiNakliyeFisi;
						sTOK_HAREKETLERI.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					}
					if (stok.ekleme_bilgileri.fiyat_farki_mi)
					{
						sTOK_HAREKETLERI.sth_cins = enum_sth_cins.DegerFarki;
					}
					sTOK_HAREKETLERI.sth_kons_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_kons_recid_recno = 0;
					sTOK_HAREKETLERI.sth_subesip_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_subesip_recid_recno = 0;
					sTOK_HAREKETLERI.sth_satistipi = enum_sth_satistipi.Kredilisatis;
					sTOK_HAREKETLERI.sth_proje_kodu = stok.ekleme_bilgileri.proje_kodu;
					sTOK_HAREKETLERI.sth_ihracat_kredi_kodu = "";
					sTOK_HAREKETLERI.sth_otv_pntr = 0;
					sTOK_HAREKETLERI.sth_otv_vergi = 0.0;
					sTOK_HAREKETLERI.sth_bkm_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_bkm_recid_recno = 0;
					sTOK_HAREKETLERI.sth_karsikons_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_karsikons_recid_recno = 0;
					sTOK_HAREKETLERI.sth_iade_evrak_seri = "";
					sTOK_HAREKETLERI.sth_iade_evrak_sira = 0;
					sTOK_HAREKETLERI.sth_diib_belge_no = "";
					sTOK_HAREKETLERI.sth_diib_satir_no = 0;
					sTOK_HAREKETLERI.sth_mensey_ulke_tipi = 0;
					sTOK_HAREKETLERI.sth_mensey_ulke_kodu = "";
					sTOK_HAREKETLERI.sth_brutagirlik = 0.0;
					sTOK_HAREKETLERI.sth_halrehmiktari = 0.0;
					sTOK_HAREKETLERI.sth_halrehfiyati = 0.0;
					sTOK_HAREKETLERI.sth_halsandikmiktari = 0.0;
					sTOK_HAREKETLERI.sth_halsandikfiyati = 0.0;
					sTOK_HAREKETLERI.sth_halsandikkdvtutari = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_1 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_2 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_3 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_4 = 0.0;
					sTOK_HAREKETLERI.sth_direkt_iscilik_5 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_1 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_2 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_3 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_4 = 0.0;
					sTOK_HAREKETLERI.sth_genel_uretim_5 = 0.0;
					sTOK_HAREKETLERI.sth_yat_tes_kodu = "";
					sTOK_HAREKETLERI.sth_oiv_pntr = 0;
					sTOK_HAREKETLERI.sth_oiv_vergi = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
					{
						sTOK_HAREKETLERI.sth_malkbl_sevk_tarihi = _sevkteslimtarihi;
					}
					else
					{
						sTOK_HAREKETLERI.sth_malkbl_sevk_tarihi = _evraktarih;
					}
					sTOK_HAREKETLERI.sth_oivvergisiz_fl = false;
					sTOK_HAREKETLERI.sth_otvvergisiz_fl = false;
					sTOK_HAREKETLERI.sth_disticaret_turu = enum_sth_disticaret_turu.ToptanYurticiTicaret;
					sTOK_HAREKETLERI.sth_fiyat_liste_no = FiyatListesi.sfl_sirano;
					sTOK_HAREKETLERI.sth_fis_sirano2 = 0;
					sTOK_HAREKETLERI.sth_rez_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_rez_recid_recno = 0;
					sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_seri = "";
					sTOK_HAREKETLERI.sth_fiyfark_esas_evrak_sira = 0;
					sTOK_HAREKETLERI.sth_fiyfark_esas_satir_no = 0;
					sTOK_HAREKETLERI.sth_optamam_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_optamam_recid_recno = 0;
					sTOK_HAREKETLERI.sth_oivtutari = 0.0;
					sTOK_HAREKETLERI.sth_Tevkifat_turu = enum_sth_Tevkifat_turu.TevkifatYok;
					sTOK_HAREKETLERI.sth_HalKomisyonuKdv = 0.0;
					sTOK_HAREKETLERI.sth_iadeTlp_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_iadeTlp_recid_recno = 0;
					sTOK_HAREKETLERI.sth_HalSatisRecid_dbcno = 0;
					sTOK_HAREKETLERI.sth_HalSatisRecid_recno = 0;
					sTOK_HAREKETLERI.sth_ciroprim_dbcno = 0;
					sTOK_HAREKETLERI.sth_ciroprim_recno = 0;
					sTOK_HAREKETLERI.sth_yetkili_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_yetkili_recid_recno = 0;
					sTOK_HAREKETLERI.sth_taxfree_fl = false;
					sTOK_HAREKETLERI.sth_HalRusum = 0.0;
					double num7 = list3[num3];
					sTOK_HAREKETLERI.sth_miktar += num7;
					if (Parametreler.Miktar2Arttir)
					{
						sTOK_HAREKETLERI.sth_miktar2 = 1.0;
					}
					else
					{
						sTOK_HAREKETLERI.sth_miktar2 = 0.0;
					}
					sTOK_HAREKETLERI.sth_birim_pntr = sIPARISLER.sip_birim_pntr;
					sTOK_HAREKETLERI.sth_aciklama = sIPARISLER.sip_aciklama + " " + sIPARISLER.sip_aciklama2;
					sTOK_HAREKETLERI.sth_tutar += sIPARISLER.sip_b_fiyat * num7;
					sTOK_HAREKETLERI.sth_otvtutari += sIPARISLER.sip_otvtutari / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_otv_pntr = sIPARISLER.sip_Otv_Pntr;
					double yuzde2 = Parametreler.vergitanimlari[sTOK_HAREKETLERI.sth_otv_pntr].Yuzde;
					sTOK_HAREKETLERI.sth_otv_vergi += sIPARISLER.sip_otvtutari / sIPARISLER.sip_miktar * num7 / 100.0 * yuzde2;
					sTOK_HAREKETLERI.sth_vergi += sIPARISLER.sip_vergi / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_masraf_vergi += sIPARISLER.sip_masvergi / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_iskonto1 += sIPARISLER.sip_iskonto_1 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_iskonto2 += sIPARISLER.sip_iskonto_2 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_iskonto3 += sIPARISLER.sip_iskonto_3 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_iskonto4 += sIPARISLER.sip_iskonto_4 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_iskonto5 += sIPARISLER.sip_iskonto_5 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_iskonto6 += sIPARISLER.sip_iskonto_6 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_masraf1 += sIPARISLER.sip_masraf_1 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_masraf2 += sIPARISLER.sip_masraf_2 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_masraf3 += sIPARISLER.sip_masraf_3 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_masraf4 += sIPARISLER.sip_masraf_4 / sIPARISLER.sip_miktar * num7;
					sTOK_HAREKETLERI.sth_isk_mas1 = sIPARISLER.sip_iskonto1;
					sTOK_HAREKETLERI.sth_isk_mas2 = sIPARISLER.sip_iskonto2;
					sTOK_HAREKETLERI.sth_isk_mas3 = sIPARISLER.sip_iskonto3;
					sTOK_HAREKETLERI.sth_isk_mas4 = sIPARISLER.sip_iskonto4;
					sTOK_HAREKETLERI.sth_isk_mas5 = sIPARISLER.sip_iskonto5;
					sTOK_HAREKETLERI.sth_isk_mas6 = sIPARISLER.sip_iskonto6;
					sTOK_HAREKETLERI.sth_isk_mas7 = sIPARISLER.sip_masraf1;
					sTOK_HAREKETLERI.sth_isk_mas8 = sIPARISLER.sip_masraf2;
					sTOK_HAREKETLERI.sth_isk_mas9 = sIPARISLER.sip_masraf3;
					sTOK_HAREKETLERI.sth_isk_mas10 = sIPARISLER.sip_masraf4;
					sTOK_HAREKETLERI.sth_vergi_pntr = sIPARISLER.sip_vergi_pntr;
					sTOK_HAREKETLERI.sth_masraf_vergi_pntr = 4;
					sTOK_HAREKETLERI.sth_plasiyer_kodu = sIPARISLER.sip_satici_kod;
					sTOK_HAREKETLERI.sth_cari_kodu = sIPARISLER.sip_musteri_kod;
					sTOK_HAREKETLERI.sth_kur_tarihi = kur.dov_tarih;
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = sIPARISLER.sip_doviz_cinsi;
					sTOK_HAREKETLERI.sth_har_doviz_kuru = sIPARISLER.sip_doviz_kuru;
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = sIPARISLER.sip_alt_doviz_kuru;
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = stok.sto_doviz_cinsi;
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = stok.ekleme_bilgileri.StokDovizCinsiKuru;
					sTOK_HAREKETLERI.sth_odeme_op = sIPARISLER.sip_opno;
					sTOK_HAREKETLERI.sth_aciklama = sIPARISLER.sip_aciklama;
					sTOK_HAREKETLERI.sth_cari_srm_merkezi = sIPARISLER.sip_cari_sormerk;
					sTOK_HAREKETLERI.sth_stok_srm_merkezi = sIPARISLER.sip_stok_sormerk;
					sTOK_HAREKETLERI.sth_adres_no = sIPARISLER.sip_adresno;
					sTOK_HAREKETLERI.sth_sip_recid_recno = 0;
					sTOK_HAREKETLERI.sth_fat_recid_recno = 0;
					if (EximKodu != "")
					{
						sTOK_HAREKETLERI.sth_cins = enum_sth_cins.IthalatIhracat;
						sTOK_HAREKETLERI.sth_vergi_pntr = 0;
						sTOK_HAREKETLERI.sth_vergi = 0.0;
						sTOK_HAREKETLERI.sth_vergisiz_fl = true;
						sTOK_HAREKETLERI.sth_exim_kodu = EximKodu;
						sTOK_HAREKETLERI.sth_disticaret_turu = enum_sth_disticaret_turu.YurtdisiTicaret;
						sTOK_HAREKETLERI.sth_otvtutari = 0.0;
						sTOK_HAREKETLERI.sth_otvvergisiz_fl = true;
						sTOK_HAREKETLERI.sth_oiv_pntr = 0;
						sTOK_HAREKETLERI.sth_oiv_vergi = 0.0;
						sTOK_HAREKETLERI.sth_oivvergisiz_fl = true;
						sTOK_HAREKETLERI.sth_oivtutari = 0.0;
					}
					sTOK_HAREKETLERI.sth_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
					sTOK_HAREKETLERI.sth_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
					sTOK_HAREKETLERI.sth_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
					sTOK_HAREKETLERI.sth_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
					sTOK_HAREKETLERI.sth_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
					sTOK_HAREKETLERI.sth_FormulMiktar = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
					sTOK_HAREKETLERI.sth_FormulMiktarNo = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktarNo;
					sTOK_HAREKETLERI.sth_sip_uid = sIPARISLER.sip_Guid;
					sIPARISLER.sip_teslim_miktar += num7;
					if (sIPARISLER.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI3 = sIPARISLER.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_Guid == BdnHar_Guid[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI3.BdnHar_TesMik += num7;
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI4 = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI4.BdnHar_HarGor = num7;
						bEDEN_HAREKETLERI4.BdnHar_BedenNo = bEDEN_HAREKETLERI3.BdnHar_BedenNo;
						sTOK_HAREKETLERI.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI4);
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item11 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						sTOK_HAREKETLERI.stok_serinolari.Add(item11);
					}
					_StokHareketleri.Add(sTOK_HAREKETLERI);
				}
				num3++;
			}
		}
		else
		{
			foreach (int siparis_recno in list)
			{
				int num8 = -1;
				SIPARISLER sIPARISLER2 = _Siparisler.Find((SIPARISLER r) => r.sip_RECid_RECno == siparis_recno);
				int num9 = 0;
				foreach (STOK_HAREKETLERI item12 in _StokHareketleri)
				{
					if (item12.sth_sip_recid_recno == sIPARISLER2.sip_RECid_RECno)
					{
						num8 = num9;
					}
					num9++;
				}
				if (num8 >= 0)
				{
					double num10 = list3[num3];
					_StokHareketleri[num8].sth_miktar += num10;
					if (Parametreler.Miktar2Arttir)
					{
						_StokHareketleri[num8].sth_miktar2++;
					}
					else
					{
						_StokHareketleri[num8].sth_miktar2 += 0.0;
					}
					_StokHareketleri[num8].sth_birim_pntr = sIPARISLER2.sip_birim_pntr;
					_StokHareketleri[num8].sth_aciklama = sIPARISLER2.sip_aciklama;
					_StokHareketleri[num8].sth_tutar += sIPARISLER2.sip_b_fiyat * num10;
					_StokHareketleri[num8].sth_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
					_StokHareketleri[num8].sth_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
					_StokHareketleri[num8].sth_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
					_StokHareketleri[num8].sth_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
					_StokHareketleri[num8].sth_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
					_StokHareketleri[num8].sth_FormulMiktar += stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
					_StokHareketleri[num8].sth_otvtutari += sIPARISLER2.sip_otvtutari / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_otv_pntr = sIPARISLER2.sip_Otv_Pntr;
					double yuzde3 = Parametreler.vergitanimlari[_StokHareketleri[num8].sth_otv_pntr].Yuzde;
					_StokHareketleri[num8].sth_otv_vergi += sIPARISLER2.sip_otvtutari / sIPARISLER2.sip_miktar * num10 / 100.0 * yuzde3;
					_StokHareketleri[num8].sth_vergi += sIPARISLER2.sip_vergi / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_masraf_vergi += sIPARISLER2.sip_masvergi / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_iskonto1 += sIPARISLER2.sip_iskonto_1 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_iskonto2 += sIPARISLER2.sip_iskonto_2 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_iskonto3 += sIPARISLER2.sip_iskonto_3 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_iskonto4 += sIPARISLER2.sip_iskonto_4 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_iskonto5 += sIPARISLER2.sip_iskonto_5 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_iskonto6 += sIPARISLER2.sip_iskonto_6 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_masraf1 += sIPARISLER2.sip_masraf_1 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_masraf2 += sIPARISLER2.sip_masraf_2 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_masraf3 += sIPARISLER2.sip_masraf_3 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_masraf4 += sIPARISLER2.sip_masraf_4 / sIPARISLER2.sip_miktar * num10;
					_StokHareketleri[num8].sth_isk_mas1 = sIPARISLER2.sip_iskonto1;
					_StokHareketleri[num8].sth_isk_mas2 = sIPARISLER2.sip_iskonto2;
					_StokHareketleri[num8].sth_isk_mas3 = sIPARISLER2.sip_iskonto3;
					_StokHareketleri[num8].sth_isk_mas4 = sIPARISLER2.sip_iskonto4;
					_StokHareketleri[num8].sth_isk_mas5 = sIPARISLER2.sip_iskonto5;
					_StokHareketleri[num8].sth_isk_mas6 = sIPARISLER2.sip_iskonto6;
					_StokHareketleri[num8].sth_isk_mas7 = sIPARISLER2.sip_masraf1;
					_StokHareketleri[num8].sth_isk_mas8 = sIPARISLER2.sip_masraf2;
					_StokHareketleri[num8].sth_isk_mas9 = sIPARISLER2.sip_masraf3;
					_StokHareketleri[num8].sth_isk_mas10 = sIPARISLER2.sip_masraf4;
					if (EximKodu != "")
					{
						_StokHareketleri[num8].sth_vergi_pntr = 0;
						_StokHareketleri[num8].sth_vergi = 0.0;
						_StokHareketleri[num8].sth_vergisiz_fl = true;
						_StokHareketleri[num8].sth_otvtutari = 0.0;
						_StokHareketleri[num8].sth_otvvergisiz_fl = true;
						_StokHareketleri[num8].sth_oiv_pntr = 0;
						_StokHareketleri[num8].sth_oiv_vergi = 0.0;
						_StokHareketleri[num8].sth_oivvergisiz_fl = true;
						_StokHareketleri[num8].sth_oivtutari = 0.0;
					}
					_StokHareketleri[num8].sth_sip_recid_recno = sIPARISLER2.sip_RECid_RECno;
					_StokHareketleri[num8].sth_sip_recid_dbcno = sIPARISLER2.sip_RECid_DBCno;
					sIPARISLER2.sip_teslim_miktar += num10;
					if (sIPARISLER2.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI5 = sIPARISLER2.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_RECid_RECno == BdnHar_RECid_RECno[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI5.BdnHar_TesMik += num10;
						bool flag4 = false;
						foreach (BEDEN_HAREKETLERI item13 in _StokHareketleri[num8].renk_beden_hareketleri)
						{
							if (bEDEN_HAREKETLERI5.BdnHar_BedenNo == item13.BdnHar_BedenNo)
							{
								item13.BdnHar_HarGor += num10;
								flag4 = true;
								break;
							}
						}
						if (!flag4)
						{
							BEDEN_HAREKETLERI bEDEN_HAREKETLERI6 = new BEDEN_HAREKETLERI();
							bEDEN_HAREKETLERI6.BdnHar_HarGor = num10;
							bEDEN_HAREKETLERI6.BdnHar_BedenNo = bEDEN_HAREKETLERI5.BdnHar_BedenNo;
							_StokHareketleri[num8].renk_beden_hareketleri.Add(bEDEN_HAREKETLERI6);
						}
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item14 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						_StokHareketleri[num8].stok_serinolari.Add(item14);
					}
				}
				else
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI2 = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI2.sth_RECid_DBCno = _DBCno;
					sTOK_HAREKETLERI2.sth_create_user = mikrouserno;
					sTOK_HAREKETLERI2.sth_lastup_user = mikrouserno;
					sTOK_HAREKETLERI2.sth_firmano = Firma.fir_sirano;
					sTOK_HAREKETLERI2.sth_subeno = Sube.Sube_no;
					sTOK_HAREKETLERI2.sth_tarih = _evraktarih;
					sTOK_HAREKETLERI2.sth_evrakno_seri = EvrakNoSeri;
					sTOK_HAREKETLERI2.sth_evrakno_sira = EvrakNoSira;
					sTOK_HAREKETLERI2.sth_satirno = _StokHareketleri.Count;
					sTOK_HAREKETLERI2.sth_belge_no = BelgeNo;
					sTOK_HAREKETLERI2.sth_belge_tarih = _belgetarih;
					sTOK_HAREKETLERI2.sth_stok_kod = stok.sto_kod;
					sTOK_HAREKETLERI2.sth_netagirlik = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk)
					{
						sTOK_HAREKETLERI2.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI2.sth_giris_depo_no = _hedefdepo.dep_no;
					}
					else
					{
						sTOK_HAREKETLERI2.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI2.sth_giris_depo_no = _kaynakdepo.dep_no;
					}
					sTOK_HAREKETLERI2.sth_nakliyedeposu = 0;
					sTOK_HAREKETLERI2.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					if (evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
					{
						sTOK_HAREKETLERI2.sth_cikis_depo_no = _kaynakdepo.dep_no;
						sTOK_HAREKETLERI2.sth_giris_depo_no = _nakliyedepo.dep_no;
						sTOK_HAREKETLERI2.sth_nakliyedeposu = _hedefdepo.dep_no;
						sTOK_HAREKETLERI2.sth_nakliyedurumu = enum_sth_nakliyedurumu.Yolda;
					}
					sTOK_HAREKETLERI2.sto_birim1_ad = stok.sto_birim1_ad;
					sTOK_HAREKETLERI2.sto_birim2_ad = stok.sto_birim2_ad;
					sTOK_HAREKETLERI2.sto_birim3_ad = stok.sto_birim3_ad;
					sTOK_HAREKETLERI2.sto_birim4_ad = stok.sto_birim4_ad;
					sTOK_HAREKETLERI2.sto_birim1_katsayi = stok.sto_birim1_katsayi;
					sTOK_HAREKETLERI2.sto_birim2_katsayi = stok.sto_birim2_katsayi;
					sTOK_HAREKETLERI2.sto_birim3_katsayi = stok.sto_birim3_katsayi;
					sTOK_HAREKETLERI2.sto_birim4_katsayi = stok.sto_birim4_katsayi;
					sTOK_HAREKETLERI2.sto_isim = stok.sto_isim;
					sTOK_HAREKETLERI2.sth_maliyet_ana = 0.0;
					sTOK_HAREKETLERI2.sth_maliyet_alternatif = 0.0;
					sTOK_HAREKETLERI2.sth_maliyet_orjinal = 0.0;
					sTOK_HAREKETLERI2.sth_parti_kodu = stok.ekleme_bilgileri.parti_kodu;
					sTOK_HAREKETLERI2.sth_lot_no = stok.ekleme_bilgileri.lot_no;
					sTOK_HAREKETLERI2.sth_cari_grup_no = Getchagrupno();
					switch (evraktipi)
					{
					case enum_GenelEvrakTipleri.SatisFaturasi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.CikisFaturasi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisFaturasi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.GirisFaturasi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.AlisIrsaliyesi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Giris;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.GirisIrsaliyesi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.SatisIrsaliyesi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.Cikis;
						if (ticaretturu == enum_cha_ticaret_turu.ToptanYurtIciTicaret)
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Toptan;
						}
						else
						{
							sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Perakende;
						}
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.CikisIrsaliyesi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiSevk:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.DepoTransferFisi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
						sTOK_HAREKETLERI2.sth_tip = enum_sth_tip.DepoTransfer;
						sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.Transfer;
						sTOK_HAREKETLERI2.sth_normal_iade = enum_sth_normal_iade.Normal;
						sTOK_HAREKETLERI2.sth_evraktip = enum_sth_evraktip.DepolarArasiNakliyeFisi;
						sTOK_HAREKETLERI2.sth_cari_cinsi = enum_sth_cari_cinsi.Carimiz;
						break;
					}
					if (stok.ekleme_bilgileri.fiyat_farki_mi)
					{
						sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.DegerFarki;
					}
					sTOK_HAREKETLERI2.sth_kons_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_kons_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_subesip_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_subesip_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_satistipi = enum_sth_satistipi.Kredilisatis;
					sTOK_HAREKETLERI2.sth_proje_kodu = stok.ekleme_bilgileri.proje_kodu;
					sTOK_HAREKETLERI2.sth_ihracat_kredi_kodu = "";
					sTOK_HAREKETLERI2.sth_otv_pntr = 0;
					sTOK_HAREKETLERI2.sth_otv_vergi = 0.0;
					sTOK_HAREKETLERI2.sth_bkm_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_bkm_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_karsikons_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_karsikons_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_iade_evrak_seri = "";
					sTOK_HAREKETLERI2.sth_iade_evrak_sira = 0;
					sTOK_HAREKETLERI2.sth_diib_belge_no = "";
					sTOK_HAREKETLERI2.sth_diib_satir_no = 0;
					sTOK_HAREKETLERI2.sth_mensey_ulke_tipi = 0;
					sTOK_HAREKETLERI2.sth_mensey_ulke_kodu = "";
					sTOK_HAREKETLERI2.sth_brutagirlik = 0.0;
					sTOK_HAREKETLERI2.sth_halrehmiktari = 0.0;
					sTOK_HAREKETLERI2.sth_halrehfiyati = 0.0;
					sTOK_HAREKETLERI2.sth_halsandikmiktari = 0.0;
					sTOK_HAREKETLERI2.sth_halsandikfiyati = 0.0;
					sTOK_HAREKETLERI2.sth_halsandikkdvtutari = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_1 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_2 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_3 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_4 = 0.0;
					sTOK_HAREKETLERI2.sth_direkt_iscilik_5 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_1 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_2 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_3 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_4 = 0.0;
					sTOK_HAREKETLERI2.sth_genel_uretim_5 = 0.0;
					sTOK_HAREKETLERI2.sth_yat_tes_kodu = "";
					sTOK_HAREKETLERI2.sth_oiv_pntr = 0;
					sTOK_HAREKETLERI2.sth_oiv_vergi = 0.0;
					if (evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi || evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
					{
						sTOK_HAREKETLERI2.sth_malkbl_sevk_tarihi = _sevkteslimtarihi;
					}
					else
					{
						sTOK_HAREKETLERI2.sth_malkbl_sevk_tarihi = _evraktarih;
					}
					sTOK_HAREKETLERI2.sth_oivvergisiz_fl = false;
					sTOK_HAREKETLERI2.sth_otvvergisiz_fl = false;
					sTOK_HAREKETLERI2.sth_disticaret_turu = enum_sth_disticaret_turu.ToptanYurticiTicaret;
					sTOK_HAREKETLERI2.sth_fiyat_liste_no = FiyatListesi.sfl_sirano;
					sTOK_HAREKETLERI2.sth_fis_sirano2 = 0;
					sTOK_HAREKETLERI2.sth_rez_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_rez_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_fiyfark_esas_evrak_seri = "";
					sTOK_HAREKETLERI2.sth_fiyfark_esas_evrak_sira = 0;
					sTOK_HAREKETLERI2.sth_fiyfark_esas_satir_no = 0;
					sTOK_HAREKETLERI2.sth_optamam_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_optamam_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_oivtutari = 0.0;
					sTOK_HAREKETLERI2.sth_Tevkifat_turu = enum_sth_Tevkifat_turu.TevkifatYok;
					sTOK_HAREKETLERI2.sth_HalKomisyonuKdv = 0.0;
					sTOK_HAREKETLERI2.sth_iadeTlp_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_iadeTlp_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_HalSatisRecid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_HalSatisRecid_recno = 0;
					sTOK_HAREKETLERI2.sth_ciroprim_dbcno = 0;
					sTOK_HAREKETLERI2.sth_ciroprim_recno = 0;
					sTOK_HAREKETLERI2.sth_yetkili_recid_dbcno = 0;
					sTOK_HAREKETLERI2.sth_yetkili_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_taxfree_fl = false;
					sTOK_HAREKETLERI2.sth_HalRusum = 0.0;
					double num11 = list3[num3];
					sTOK_HAREKETLERI2.sth_miktar += num11;
					if (Parametreler.Miktar2Arttir)
					{
						sTOK_HAREKETLERI2.sth_miktar2 = 1.0;
					}
					else
					{
						sTOK_HAREKETLERI2.sth_miktar2 = 0.0;
					}
					sTOK_HAREKETLERI2.sth_birim_pntr = sIPARISLER2.sip_birim_pntr;
					sTOK_HAREKETLERI2.sth_aciklama = sIPARISLER2.sip_aciklama + " " + sIPARISLER2.sip_aciklama2;
					sTOK_HAREKETLERI2.sth_tutar += sIPARISLER2.sip_b_fiyat * num11;
					sTOK_HAREKETLERI2.sth_otvtutari += sIPARISLER2.sip_otvtutari / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_otv_pntr = sIPARISLER2.sip_Otv_Pntr;
					double yuzde4 = Parametreler.vergitanimlari[sTOK_HAREKETLERI2.sth_otv_pntr].Yuzde;
					sTOK_HAREKETLERI2.sth_otv_vergi += sIPARISLER2.sip_otvtutari / sIPARISLER2.sip_miktar * num11 / 100.0 * yuzde4;
					sTOK_HAREKETLERI2.sth_vergi += sIPARISLER2.sip_vergi / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_masraf_vergi += sIPARISLER2.sip_masvergi / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_iskonto1 += sIPARISLER2.sip_iskonto_1 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_iskonto2 += sIPARISLER2.sip_iskonto_2 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_iskonto3 += sIPARISLER2.sip_iskonto_3 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_iskonto4 += sIPARISLER2.sip_iskonto_4 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_iskonto5 += sIPARISLER2.sip_iskonto_5 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_iskonto6 += sIPARISLER2.sip_iskonto_6 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_masraf1 += sIPARISLER2.sip_masraf_1 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_masraf2 += sIPARISLER2.sip_masraf_2 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_masraf3 += sIPARISLER2.sip_masraf_3 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_masraf4 += sIPARISLER2.sip_masraf_4 / sIPARISLER2.sip_miktar * num11;
					sTOK_HAREKETLERI2.sth_isk_mas1 = sIPARISLER2.sip_iskonto1;
					sTOK_HAREKETLERI2.sth_isk_mas2 = sIPARISLER2.sip_iskonto2;
					sTOK_HAREKETLERI2.sth_isk_mas3 = sIPARISLER2.sip_iskonto3;
					sTOK_HAREKETLERI2.sth_isk_mas4 = sIPARISLER2.sip_iskonto4;
					sTOK_HAREKETLERI2.sth_isk_mas5 = sIPARISLER2.sip_iskonto5;
					sTOK_HAREKETLERI2.sth_isk_mas6 = sIPARISLER2.sip_iskonto6;
					sTOK_HAREKETLERI2.sth_isk_mas7 = sIPARISLER2.sip_masraf1;
					sTOK_HAREKETLERI2.sth_isk_mas8 = sIPARISLER2.sip_masraf2;
					sTOK_HAREKETLERI2.sth_isk_mas9 = sIPARISLER2.sip_masraf3;
					sTOK_HAREKETLERI2.sth_isk_mas10 = sIPARISLER2.sip_masraf4;
					sTOK_HAREKETLERI2.sth_vergi_pntr = sIPARISLER2.sip_vergi_pntr;
					sTOK_HAREKETLERI2.sth_masraf_vergi_pntr = 4;
					sTOK_HAREKETLERI2.sth_plasiyer_kodu = sIPARISLER2.sip_satici_kod;
					sTOK_HAREKETLERI2.sth_cari_kodu = sIPARISLER2.sip_musteri_kod;
					sTOK_HAREKETLERI2.sth_kur_tarihi = kur.dov_tarih;
					sTOK_HAREKETLERI2.sth_har_doviz_cinsi = sIPARISLER2.sip_doviz_cinsi;
					sTOK_HAREKETLERI2.sth_har_doviz_kuru = sIPARISLER2.sip_doviz_kuru;
					sTOK_HAREKETLERI2.sth_alt_doviz_kuru = sIPARISLER2.sip_alt_doviz_kuru;
					sTOK_HAREKETLERI2.sth_stok_doviz_cinsi = stok.sto_doviz_cinsi;
					sTOK_HAREKETLERI2.sth_stok_doviz_kuru = stok.ekleme_bilgileri.StokDovizCinsiKuru;
					sTOK_HAREKETLERI2.sth_odeme_op = sIPARISLER2.sip_opno;
					sTOK_HAREKETLERI2.sth_aciklama = sIPARISLER2.sip_aciklama;
					sTOK_HAREKETLERI2.sth_cari_srm_merkezi = sIPARISLER2.sip_cari_sormerk;
					sTOK_HAREKETLERI2.sth_stok_srm_merkezi = sIPARISLER2.sip_stok_sormerk;
					sTOK_HAREKETLERI2.sth_adres_no = sIPARISLER2.sip_adresno;
					sTOK_HAREKETLERI2.sth_sip_recid_recno = 0;
					sTOK_HAREKETLERI2.sth_fat_recid_recno = 0;
					if (EximKodu != "")
					{
						sTOK_HAREKETLERI2.sth_cins = enum_sth_cins.IthalatIhracat;
						sTOK_HAREKETLERI2.sth_vergi_pntr = 0;
						sTOK_HAREKETLERI2.sth_vergi = 0.0;
						sTOK_HAREKETLERI2.sth_vergisiz_fl = true;
						sTOK_HAREKETLERI2.sth_exim_kodu = EximKodu;
						sTOK_HAREKETLERI2.sth_disticaret_turu = enum_sth_disticaret_turu.YurtdisiTicaret;
						sTOK_HAREKETLERI2.sth_otvtutari = 0.0;
						sTOK_HAREKETLERI2.sth_otvvergisiz_fl = true;
						sTOK_HAREKETLERI2.sth_oiv_pntr = 0;
						sTOK_HAREKETLERI2.sth_oiv_vergi = 0.0;
						sTOK_HAREKETLERI2.sth_oivvergisiz_fl = true;
						sTOK_HAREKETLERI2.sth_oivtutari = 0.0;
					}
					sTOK_HAREKETLERI2.sth_Olcu1 = stok.ekleme_bilgileri.Miktar_Formul_Olcu1;
					sTOK_HAREKETLERI2.sth_Olcu2 = stok.ekleme_bilgileri.Miktar_Formul_Olcu2;
					sTOK_HAREKETLERI2.sth_Olcu3 = stok.ekleme_bilgileri.Miktar_Formul_Olcu3;
					sTOK_HAREKETLERI2.sth_Olcu4 = stok.ekleme_bilgileri.Miktar_Formul_Olcu4;
					sTOK_HAREKETLERI2.sth_Olcu5 = stok.ekleme_bilgileri.Miktar_Formul_Olcu5;
					sTOK_HAREKETLERI2.sth_FormulMiktar = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktar;
					sTOK_HAREKETLERI2.sth_FormulMiktarNo = stok.ekleme_bilgileri.Miktar_Formul_FormulMiktarNo;
					sTOK_HAREKETLERI2.sth_sip_recid_recno = sIPARISLER2.sip_RECid_RECno;
					sTOK_HAREKETLERI2.sth_sip_recid_dbcno = sIPARISLER2.sip_RECid_DBCno;
					sIPARISLER2.sip_teslim_miktar += num11;
					if (sIPARISLER2.renk_beden_hareketleri.Count > 0)
					{
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI7 = sIPARISLER2.renk_beden_hareketleri.Find((BEDEN_HAREKETLERI r) => r.BdnHar_RECid_RECno == BdnHar_RECid_RECno[bedenhareket_islemsira]);
						bEDEN_HAREKETLERI7.BdnHar_TesMik += num11;
						BEDEN_HAREKETLERI bEDEN_HAREKETLERI8 = new BEDEN_HAREKETLERI();
						bEDEN_HAREKETLERI8.BdnHar_HarGor = num11;
						bEDEN_HAREKETLERI8.BdnHar_BedenNo = bEDEN_HAREKETLERI7.BdnHar_BedenNo;
						sTOK_HAREKETLERI2.renk_beden_hareketleri.Add(bEDEN_HAREKETLERI8);
						bedenhareket_islemsira++;
					}
					foreach (STOK_SERINO_TANIMLARI item15 in stok.ekleme_bilgileri.serino_tanimlari)
					{
						sTOK_HAREKETLERI2.stok_serinolari.Add(item15);
					}
					_StokHareketleri.Add(sTOK_HAREKETLERI2);
				}
				num3++;
			}
		}
		AddUrunCekiListesi(stok.sto_kod, stok.ekleme_bilgileri.Miktar, 0, Parametreler.CekiListesi.AktifAnaAmbalajNo, Parametreler.CekiListesi.AktifAltAmbalajNo);
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private enum_evrak_siparis_ekleme_sonuc AddUrunSayimSonuclari(Stok stok)
	{
		if (stok.sto_detay_takip == 3 && stok.ekleme_bilgileri.serino_tanimlari.Count == 0)
		{
			return enum_evrak_siparis_ekleme_sonuc.SeriNoGirilmekZorunda;
		}
		if (stok.ekleme_bilgileri.SatirBirlestir && stok.sto_detay_takip != 3)
		{
			foreach (SAYIM_SONUCLARI item in _SayimSonuclariHareketleri)
			{
				if (!(item.sym_Stokkodu == stok.sto_kod) || !(item.sym_parti_kodu == stok.ekleme_bilgileri.parti_kodu) || item.sym_lot_no != stok.ekleme_bilgileri.lot_no)
				{
					continue;
				}
				bool flag = true;
				foreach (BEDEN_HAREKETLERI item2 in stok.ekleme_bilgileri.renk_beden_hareketleri)
				{
					BEDEN_HAREKETLERI bEDEN_HAREKETLERI = new BEDEN_HAREKETLERI();
					bEDEN_HAREKETLERI.BdnHar_HarGor = item2.BdnHar_HarGor;
					bEDEN_HAREKETLERI.BdnHar_BedenNo = item2.BdnHar_BedenNo;
					if (item.sym_bedenno != bEDEN_HAREKETLERI.GetBedenNo() || item.sym_renkno != bEDEN_HAREKETLERI.GetRenkNo())
					{
						flag = false;
					}
				}
				if (flag)
				{
					item.sym_miktar1 += stok.ekleme_bilgileri.Miktar;
					return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
				}
			}
		}
		SAYIM_SONUCLARI sAYIM_SONUCLARI = new SAYIM_SONUCLARI();
		sAYIM_SONUCLARI.sym_RECid_DBCno = _DBCno;
		sAYIM_SONUCLARI.sym_create_user = mikrouserno;
		sAYIM_SONUCLARI.sym_lastup_user = mikrouserno;
		sAYIM_SONUCLARI.sym_tarihi = _evraktarih;
		sAYIM_SONUCLARI.sym_depono = _kaynakdepo.dep_no;
		sAYIM_SONUCLARI.sym_evrakno = EvrakNoSira;
		sAYIM_SONUCLARI.sym_satirno = _SayimSonuclariHareketleri.Count;
		sAYIM_SONUCLARI.sym_Stokkodu = stok.sto_kod;
		sAYIM_SONUCLARI.sym_reyonkodu = "0";
		sAYIM_SONUCLARI.sym_koridorkodu = "0";
		sAYIM_SONUCLARI.sym_rafkodu = "0";
		sAYIM_SONUCLARI.sym_miktar1 = stok.ekleme_bilgileri.Miktar;
		sAYIM_SONUCLARI.sym_miktar2 = 0.0;
		sAYIM_SONUCLARI.sym_miktar3 = 0.0;
		sAYIM_SONUCLARI.sym_miktar4 = 0.0;
		sAYIM_SONUCLARI.sym_miktar5 = 0.0;
		sAYIM_SONUCLARI.sym_birim_pntr = stok.ekleme_bilgileri.sth_birim_pntr;
		sAYIM_SONUCLARI.sym_barkod = "";
		sAYIM_SONUCLARI.sym_renkno = 0;
		sAYIM_SONUCLARI.sym_bedenno = 0;
		sAYIM_SONUCLARI.sym_parti_kodu = stok.ekleme_bilgileri.parti_kodu;
		sAYIM_SONUCLARI.sym_lot_no = stok.ekleme_bilgileri.lot_no;
		sAYIM_SONUCLARI.sym_serino = "";
		if (stok.sto_detay_takip == 3 && stok.ekleme_bilgileri.serino_tanimlari.Count > 0)
		{
			sAYIM_SONUCLARI.sym_serino = stok.ekleme_bilgileri.serino_tanimlari[0].chz_serino;
			sAYIM_SONUCLARI.sym_miktar1 = 1.0;
		}
		foreach (BEDEN_HAREKETLERI item3 in stok.ekleme_bilgileri.renk_beden_hareketleri)
		{
			BEDEN_HAREKETLERI bEDEN_HAREKETLERI2 = new BEDEN_HAREKETLERI();
			bEDEN_HAREKETLERI2.BdnHar_HarGor = item3.BdnHar_HarGor;
			bEDEN_HAREKETLERI2.BdnHar_BedenNo = item3.BdnHar_BedenNo;
			sAYIM_SONUCLARI.sym_bedenno = bEDEN_HAREKETLERI2.GetBedenNo();
			sAYIM_SONUCLARI.sym_renkno = bEDEN_HAREKETLERI2.GetRenkNo();
		}
		_SayimSonuclariHareketleri.Add(sAYIM_SONUCLARI);
		return enum_evrak_siparis_ekleme_sonuc.UrunEklendi;
	}

	private void AddUrunCekiListesi(string stok_kodu, double miktar, int Ckl_BedenPntr, int Ckl_AnaAmbalajNo, int Ckl_AltAmbalajNo)
	{
		if (!Parametreler.CekiListesi.Olustur || evraktipi != enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return;
		}
		bool flag = false;
		foreach (CEKI_LISTESI item in CekiListesi)
		{
			if (item.Ckl_StokKodu == stok_kodu && item.Ckl_BedenPntr == Ckl_BedenPntr && item.Ckl_AnaAmbalajNo == Ckl_AnaAmbalajNo && item.Ckl_AltAmbalajNo == Ckl_AltAmbalajNo)
			{
				item.Ckl_Miktari += miktar;
				flag = true;
			}
		}
		if (!flag)
		{
			CEKI_LISTESI cEKI_LISTESI = new CEKI_LISTESI();
			cEKI_LISTESI.Ckl_AltAmbalajNo = Ckl_AltAmbalajNo;
			cEKI_LISTESI.Ckl_AnaAmbalajNo = Ckl_AnaAmbalajNo;
			cEKI_LISTESI.Ckl_BedenPntr = Ckl_BedenPntr;
			cEKI_LISTESI.Ckl_create_user = mikrouserno;
			cEKI_LISTESI.Ckl_EvrakSeri = EvrakNoSeri;
			cEKI_LISTESI.Ckl_EvrakTip = enum_Ckl_EvrakTip.CikisIrsaliyesi;
			cEKI_LISTESI.Ckl_lastup_user = mikrouserno;
			cEKI_LISTESI.Ckl_Miktari = miktar;
			cEKI_LISTESI.Ckl_special1 = "FORA";
			cEKI_LISTESI.Ckl_StokKodu = stok_kodu;
			CekiListesi.Add(cEKI_LISTESI);
		}
	}

	public void AddStokHareketi(STOK_HAREKETLERI hareket)
	{
		_StokHareketleri.Add(hareket);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void AddDepolarArasiSiparisHareketi(List<DEPOLAR_ARASI_SIPARISLER> hareket, bool SiparisKarsilamaYap)
	{
		_DepolarArasiSiparisHareketleri.AddRange(hareket);
		if (SiparisKarsilamaYap)
		{
			_sipariskarsilamami = true;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void AddDepolarArasiSiparisHareketi(DEPOLAR_ARASI_SIPARISLER hareket, bool SiparisKarsilamaYap)
	{
		_DepolarArasiSiparisHareketleri.Add(hareket);
		if (SiparisKarsilamaYap)
		{
			_sipariskarsilamami = true;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	public void AddSayimSonuclariHareketi(SAYIM_SONUCLARI hareket)
	{
		_SayimSonuclariHareketleri.Add(hareket);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void AddHizmetHareketi(CARI_HESAP_HAREKETLERI hareket)
	{
		_HizmetHareketleri.Add(hareket);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void AddTahsilatHareketleri(CARI_HESAP_HAREKETLERI hareket)
	{
		_TahsilatHareketleri.Add(hareket);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void AddGenelCariHesapHareketleri(CARI_HESAP_HAREKETLERI hareket)
	{
		_GenelCariHesapHareketleri.Add(hareket);
		OnEvrakChanged(EventArgs.Empty);
	}

	public void StokCariHesapHareketiOlustur()
	{
		int num = 0;
		_StokCariHesapHareketi = new CARI_HESAP_HAREKETLERI();
		_StokCariHesapHareketiIadeliSatis = new CARI_HESAP_HAREKETLERI();
		_StokFiyatFarkiCariHesapHareketi = new CARI_HESAP_HAREKETLERI();
		bool flag = false;
		foreach (STOK_HAREKETLERI item in _StokHareketleri)
		{
			if (item.sth_cins != enum_sth_cins.DegerFarki)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			StokCariHesapHareketiOlustur(_StokCariHesapHareketi, num, FiyatFarki: false, IadeliSatisFaturasi: false);
			num++;
		}
		bool flag2 = false;
		foreach (STOK_HAREKETLERI item2 in _StokHareketleri)
		{
			if (item2.sth_evraktip == enum_sth_evraktip.GirisFaturasi && evraktipi == enum_GenelEvrakTipleri.SatisFaturasi)
			{
				flag2 = true;
				break;
			}
		}
		if (flag2)
		{
			StokCariHesapHareketiOlustur(_StokCariHesapHareketiIadeliSatis, num, FiyatFarki: false, IadeliSatisFaturasi: true);
			num++;
		}
		bool flag3 = false;
		foreach (STOK_HAREKETLERI item3 in _StokHareketleri)
		{
			if (item3.sth_cins == enum_sth_cins.DegerFarki)
			{
				flag3 = true;
				break;
			}
		}
		if (flag3)
		{
			StokCariHesapHareketiOlustur(_StokFiyatFarkiCariHesapHareketi, num, FiyatFarki: true, IadeliSatisFaturasi: false);
			num++;
		}
		OnEvrakChanged(EventArgs.Empty);
	}

	private void StokCariHesapHareketiOlustur(CARI_HESAP_HAREKETLERI carihar, int satirno, bool FiyatFarki, bool IadeliSatisFaturasi)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double cha_vergi = 0.0;
		double cha_vergi2 = 0.0;
		double cha_vergi3 = 0.0;
		double cha_vergi4 = 0.0;
		double cha_vergi5 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		double num11 = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		double num14 = 0.0;
		double num15 = 0.0;
		double num16 = 0.0;
		double num17 = 0.0;
		foreach (STOK_HAREKETLERI item in _StokHareketleri)
		{
			bool flag = false;
			if (item.sth_cins == enum_sth_cins.DegerFarki && FiyatFarki)
			{
				flag = true;
			}
			if (item.sth_cins != enum_sth_cins.DegerFarki && !FiyatFarki && (item.sth_evraktip != enum_sth_evraktip.GirisFaturasi || evraktipi != enum_GenelEvrakTipleri.SatisFaturasi))
			{
				flag = true;
			}
			if (IadeliSatisFaturasi)
			{
				flag = ((item.sth_evraktip == enum_sth_evraktip.GirisFaturasi && evraktipi == enum_GenelEvrakTipleri.SatisFaturasi) ? true : false);
			}
			if (flag)
			{
				num += item.sth_tutar;
				num7 += item.sth_masraf1;
				num8 += item.sth_masraf2;
				num9 += item.sth_masraf3;
				num10 += item.sth_masraf4;
				num11 += item.sth_iskonto1;
				num12 += item.sth_iskonto2;
				num13 += item.sth_iskonto3;
				num14 += item.sth_iskonto4;
				num15 += item.sth_iskonto5;
				num16 += item.sth_iskonto6;
				num17 += item.sth_otvtutari;
				switch (item.sth_vergi_pntr)
				{
				case 2:
					num3 += item.sth_vergi;
					break;
				case 3:
					num4 += item.sth_vergi;
					break;
				case 4:
					num5 += item.sth_vergi;
					break;
				case 5:
					num6 += item.sth_vergi;
					break;
				default:
					num2 += item.sth_vergi;
					break;
				}
				switch (item.sth_masraf_vergi_pntr)
				{
				case 2:
					num3 += item.sth_masraf_vergi;
					break;
				case 3:
					num4 += item.sth_masraf_vergi;
					break;
				case 4:
					num5 += item.sth_masraf_vergi;
					break;
				case 5:
					num6 += item.sth_masraf_vergi;
					break;
				default:
					num2 += item.sth_masraf_vergi;
					break;
				}
				switch (item.sth_otv_pntr)
				{
				case 2:
					num3 += item.sth_otv_vergi;
					break;
				case 3:
					num4 += item.sth_otv_vergi;
					break;
				case 4:
					num5 += item.sth_otv_vergi;
					break;
				case 5:
					num6 += item.sth_otv_vergi;
					break;
				default:
					num2 += item.sth_otv_vergi;
					break;
				}
			}
		}
		if (evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			carihar.cha_tip = enum_cha_tip.Alacak;
			carihar.cha_evrak_tip = enum_cha_evrak_tip.AlisFaturasi;
		}
		else
		{
			carihar.cha_tip = enum_cha_tip.Borc;
			carihar.cha_evrak_tip = enum_cha_evrak_tip.SatisFaturasi;
			if (IadeliSatisFaturasi)
			{
				carihar.cha_tip = enum_cha_tip.Alacak;
				carihar.cha_evrak_tip = enum_cha_evrak_tip.AlisFaturasi;
			}
		}
		if (FiyatFarki)
		{
			carihar.cha_cinsi = enum_cha_cinsi.DegerFarkiFaturasi;
		}
		else
		{
			switch (ticaretturu)
			{
			case enum_cha_ticaret_turu.ToptanYurtIciTicaret:
				carihar.cha_cinsi = enum_cha_cinsi.ToptanFatura;
				break;
			case enum_cha_ticaret_turu.PerakendeYurtIciTicaret:
				carihar.cha_cinsi = enum_cha_cinsi.PerakendeFaturasi;
				break;
			default:
				carihar.cha_cinsi = enum_cha_cinsi.ToptanFatura;
				break;
			}
		}
		carihar.cha_ticaret_turu = ticaretturu;
		carihar.cha_EXIMkodu = "";
		if (IadeliSatisFaturasi)
		{
			carihar.cha_normal_Iade = enum_cha_normal_Iade.Iade;
		}
		else
		{
			carihar.cha_normal_Iade = normaliade;
		}
		carihar.cha_RECid_DBCno = _DBCno;
		carihar.cha_create_user = mikrouserno;
		carihar.cha_create_date = DateTime.Now;
		carihar.cha_lastup_user = mikrouserno;
		carihar.cha_lastup_date = DateTime.Now;
		carihar.cha_firmano = Firma.fir_sirano;
		carihar.cha_subeno = Sube.Sube_no;
		carihar.cha_tarihi = _evraktarih;
		carihar.cha_satir_no = satirno;
		carihar.cha_evrakno_seri = EvrakNoSeri;
		carihar.cha_evrakno_sira = EvrakNoSira;
		carihar.cha_belge_no = BelgeNo;
		carihar.cha_belge_tarih = _belgetarih;
		carihar.cha_kod = cari.cari_kod;
		carihar.cha_d_kurtar = kur.dov_tarih;
		carihar.cha_d_cins = kur.dov_no;
		carihar.cha_d_kur = kur.dov_fiyat;
		carihar.cha_altd_kur = alternatifdovizkuru.dov_fiyat;
		carihar.cha_grupno = Getchagrupno();
		carihar.cha_vade = odemeplani;
		carihar.cha_fis_tarih = new DateTime(1900, 1, 1);
		carihar.cha_fis_sirano = 0;
		carihar.cha_ft_iskonto1 = num11;
		carihar.cha_ft_iskonto2 = num12;
		carihar.cha_ft_iskonto3 = num13;
		carihar.cha_ft_iskonto4 = num14;
		carihar.cha_ft_iskonto5 = num15;
		carihar.cha_ft_iskonto6 = num16;
		carihar.cha_ft_masraf1 = num7;
		carihar.cha_ft_masraf2 = num8;
		carihar.cha_ft_masraf3 = num9;
		carihar.cha_ft_masraf4 = num10;
		carihar.cha_vergi1 = num2;
		carihar.cha_vergi2 = num3;
		carihar.cha_vergi3 = num4;
		carihar.cha_vergi4 = num5;
		carihar.cha_vergi5 = num6;
		carihar.cha_vergi6 = cha_vergi;
		carihar.cha_vergi7 = cha_vergi2;
		carihar.cha_vergi8 = cha_vergi3;
		carihar.cha_vergi9 = cha_vergi4;
		carihar.cha_vergi10 = cha_vergi5;
		carihar.cha_yuvarlama = 0.0;
		if (kapamasekli == enum_KapamaSekli.AcikHesap)
		{
			carihar.cha_tpoz = enum_cha_tpoz.Acik;
		}
		else
		{
			carihar.cha_tpoz = enum_cha_tpoz.Kapali;
		}
		carihar.cha_aciklama = aciklama;
		carihar.cha_trefno = "";
		carihar.cha_sntck_poz = enum_cha_sntck_poz.Portfoyde;
		carihar.cha_karsidcinsi = 0;
		carihar.cha_karsid_kur = 1.0;
		carihar.cha_karsidgrupno = 0;
		carihar.cha_srmrkkodu = sorumlulukmerkezi.som_kod;
		carihar.cha_reftarihi = new DateTime(1899, 12, 30);
		carihar.cha_odeme_arr1 = 0.0;
		carihar.cha_odeme_arr2 = 0.0;
		carihar.cha_odeme_arr3 = 0.0;
		carihar.cha_odeme_arr4 = 0.0;
		carihar.cha_odeme_arr5 = 0.0;
		carihar.cha_odeme_arr6 = 0.0;
		carihar.cha_odeme_arr7 = 0.0;
		carihar.cha_odeme_arr8 = 0.0;
		carihar.cha_miktari = 0.0;
		carihar.cha_aratoplam = num;
		carihar.cha_vergipntr = 0;
		carihar.cha_istisnakodu = 0;
		carihar.cha_ver_tev_carpani = 0.0;
		carihar.cha_stopaj = 0.0;
		carihar.cha_savsandesfonu = 0.0;
		carihar.cha_vergisiz_fl = false;
		carihar.cha_satici_kodu = TemsilciKodu;
		carihar.cha_mustahsil_borsa = 0.0;
		carihar.cha_mustahsil_bagkur = 0.0;
		carihar.cha_mustahsil_diger = 0.0;
		carihar.cha_HalMSDF = 0.0;
		carihar.cha_HalHamaliye = 0.0;
		carihar.cha_HalStopaj = 0.0;
		carihar.cha_HalKomisyonu = 0.0;
		carihar.cha_StFonPntr = 0;
		carihar.cha_pos_hareketi = false;
		carihar.cha_vardiya_tarihi = new DateTime(1900, 1, 1);
		carihar.cha_vardiya_no = 0;
		carihar.cha_vardiya_evrak_ti = enum_cha_vardita_evrak_ti.HizmetSatisi;
		carihar.cha_HalRusum = 0.0;
		carihar.cha_HalNavlunTut = 0.0;
		carihar.cha_HalRehinFuture = 0.0;
		carihar.cha_HalKomisyon = 0.0;
		carihar.cha_Vade_Farki_Yuz = 0.0;
		carihar.cha_karsisrmrkkodu = "";
		carihar.cha_HalRehinSandikmiktari = 0.0;
		carihar.cha_HalSandikVrMiktar = 0.0;
		carihar.cha_HalSandikTutari = 0.0;
		carihar.cha_HalSandikKDVTutari = 0.0;
		carihar.cha_HalrehinSandikTutari = 0.0;
		carihar.cha_Tevkifat_turu = enum_cha_Tevkifat_turu.TevkifatYok;
		carihar.cha_otvtutari = num17;
		carihar.cha_otvvergisiz_fl = false;
		carihar.cha_projekodu = proje.pro_kodu;
		carihar.cha_sozlesme_DBCno = 0;
		carihar.cha_sozlesme_RECno = 0;
		carihar.cha_yat_tes_kodu = "";
		carihar.cha_ciro_cari_kodu = cari.cari_kod;
		carihar.cha_oivergisiz_fl = false;
		carihar.cha_meblag_ana_doviz_icin_gecersiz_fl = 0;
		carihar.cha_meblag_alt_doviz_icin_gecersiz_fl = 0;
		carihar.cha_meblag_orj_doviz_icin_gecersiz_fl = 0;
		carihar.cha_ciroprim_DBCno = 0;
		carihar.cha_ciroprim_RECno = 0;
		carihar.cha_HalHamaliyeKdv = 0.0;
		carihar.cha_HalHamaliyeVergisiz_fl = false;
		carihar.cha_bakimhar_DBCno = 0;
		carihar.cha_bakimhar_RECno = 0;
		carihar.cha_avanstalep_DBCno = 0;
		carihar.cha_avanstalep_RECno = 0;
		carihar.cha_oiv_pntr = 0;
		carihar.cha_oiv_vergi = 0.0;
		carihar.cha_oivtutari = 0.0;
		carihar.cha_isk_mas1 = 0;
		carihar.cha_isk_mas2 = 0;
		carihar.cha_isk_mas3 = 0;
		carihar.cha_isk_mas4 = 0;
		carihar.cha_isk_mas5 = 0;
		carihar.cha_isk_mas6 = 0;
		carihar.cha_isk_mas7 = 0;
		carihar.cha_isk_mas8 = 0;
		carihar.cha_isk_mas9 = 0;
		carihar.cha_isk_mas10 = 0;
		carihar.cha_sat_iskmas1 = false;
		carihar.cha_sat_iskmas2 = false;
		carihar.cha_sat_iskmas3 = false;
		carihar.cha_sat_iskmas4 = false;
		carihar.cha_sat_iskmas5 = false;
		carihar.cha_sat_iskmas6 = false;
		carihar.cha_sat_iskmas7 = false;
		carihar.cha_sat_iskmas8 = false;
		carihar.cha_sat_iskmas9 = false;
		carihar.cha_sat_iskmas10 = false;
		carihar.cha_cari_cins = enum_cha_cari_cins.Carimiz;
		carihar.cha_kasa_hizmet = enum_cha_kasa_hizmet.Carimiz;
		carihar.cha_kasa_hizkod = "";
		if (kapamasekli != enum_KapamaSekli.AcikHesap)
		{
			switch (kapamasekli)
			{
			case enum_KapamaSekli.BankadanKapanacak:
				carihar.cha_cari_cins = enum_cha_cari_cins.Bankamiz;
				break;
			case enum_KapamaSekli.CariPersoneldenKapanacak:
				carihar.cha_cari_cins = enum_cha_cari_cins.CariPersonelimiz;
				break;
			case enum_KapamaSekli.KasadanKapanacak:
				carihar.cha_cari_cins = enum_cha_cari_cins.Kasamiz;
				break;
			}
			carihar.cha_kod = kapamahesapkodu;
			carihar.cha_kasa_hizmet = enum_cha_kasa_hizmet.Carimiz;
			carihar.cha_kasa_hizkod = "";
			carihar.cha_aciklama = cari.cari_unvan1 + " " + cari.cari_unvan2;
			if (carihar.cha_aciklama.Length > 40)
			{
				carihar.cha_aciklama = carihar.cha_aciklama.Substring(0, 40);
			}
		}
		carihar.cha_meblag = carihar.cha_aratoplam - carihar.cha_ft_iskonto1 - carihar.cha_ft_iskonto2 - carihar.cha_ft_iskonto3 - carihar.cha_ft_iskonto4 - carihar.cha_ft_iskonto5 - carihar.cha_ft_iskonto6 + (carihar.cha_ft_masraf1 + carihar.cha_ft_masraf2 + carihar.cha_ft_masraf3 + carihar.cha_ft_masraf4) + (carihar.cha_vergi1 + carihar.cha_vergi2 + carihar.cha_vergi3 + carihar.cha_vergi4 + carihar.cha_vergi5 + carihar.cha_vergi6 + carihar.cha_vergi7 + carihar.cha_vergi8 + carihar.cha_vergi9 + carihar.cha_vergi10 + carihar.cha_otvtutari + carihar.cha_oivtutari);
	}

	public static byte[] WriteToByteArray(Evrak toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(Evrak toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(Evrak toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 11;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write((int)toWrite.evraktipi);
		writer.Write((int)toWrite.kapamasekli);
		writer.Write((int)toWrite.normaliade);
		writer.Write((int)toWrite.ticaretturu);
		writer.Write((int)toWrite.aktarimdurumu);
		writer.Write(toWrite.offlinerecno);
		writer.Write(toWrite.yenikayit);
		writer.Write(toWrite.evrakkilitli);
		writer.Write(toWrite.sipariskarsilamami);
		writer.Write(toWrite._evraktarih.Ticks);
		writer.Write(toWrite.EvrakNoSeri);
		writer.Write(toWrite.EvrakNoSira);
		writer.Write(toWrite.BelgeNo);
		writer.Write(toWrite._belgetarih.Ticks);
		writer.Write(toWrite.odemeplani);
		writer.Write(toWrite.TemsilciKodu);
		writer.Write(toWrite.mikrouserno);
		writer.Write(toWrite.aciklama);
		writer.Write(toWrite._sevkteslimtarihi.Ticks);
		writer.Write(toWrite.sevkadresno);
		writer.Write(toWrite.kapamahesapkodu);
		writer.Write(toWrite.aciklama1);
		writer.Write(toWrite.aciklama2);
		writer.Write(toWrite.aciklama3);
		writer.Write(toWrite.aciklama4);
		writer.Write(toWrite.aciklama5);
		writer.Write(toWrite.aciklama6);
		writer.Write(toWrite.aciklama7);
		writer.Write(toWrite.aciklama8);
		writer.Write(toWrite.aciklama9);
		writer.Write(toWrite.aciklama10);
		writer.Write(toWrite.sip_teslimturu);
		writer.Write(toWrite.degistirspecialalan1);
		writer.Write(toWrite.degistirspecialalan2);
		writer.Write(toWrite.degistirspecialalan3);
		Cari.WriteToBinaryWriter(toWrite.cari, writer, 1);
		FiyatListesi.WriteToBinaryWriter(toWrite.FiyatListesi, writer);
		writer.Write(toWrite.dovizcinsi);
		Kur.WriteToBinaryWriter(toWrite.kur, writer);
		writer.Write(toWrite.alternatifdovizcinsi);
		Kur.WriteToBinaryWriter(toWrite.alternatifdovizkuru, writer);
		Depo.WriteToBinaryWriter(toWrite._kaynakdepo, writer);
		Depo.WriteToBinaryWriter(toWrite._hedefdepo, writer);
		Depo.WriteToBinaryWriter(toWrite._nakliyedepo, writer);
		Proje.WriteToBinaryWriter(toWrite.proje, writer);
		SorumlulukMerkezi.WriteToBinaryWriter(toWrite.sorumlulukmerkezi, writer);
		Firma.WriteToBinaryWriter(toWrite.Firma, writer);
		Sube.WriteToBinaryWriter(toWrite.Sube, writer);
		writer.Write(toWrite.GetStokHareketleri().Count);
		foreach (STOK_HAREKETLERI item in toWrite.GetStokHareketleri())
		{
			STOK_HAREKETLERI.WriteToBinaryWriter(item, writer, 1);
		}
		writer.Write(toWrite.GetSiparisler().Count);
		foreach (SIPARISLER item2 in toWrite.GetSiparisler())
		{
			SIPARISLER.WriteToBinaryWriter(item2, writer, 1);
		}
		CARI_HESAP_HAREKETLERI.WriteToBinaryWriter(toWrite.GetStokCariHesapHareketi(), writer, 1);
		CARI_HESAP_HAREKETLERI.WriteToBinaryWriter(toWrite.GetStokFiyatFarkiCariHesapHareketi(), writer, 1);
		writer.Write(toWrite.GetHizmetHareketleri().Count);
		foreach (CARI_HESAP_HAREKETLERI item3 in toWrite.GetHizmetHareketleri())
		{
			CARI_HESAP_HAREKETLERI.WriteToBinaryWriter(item3, writer, 1);
		}
		writer.Write(toWrite.GetTahsilatHareketleri().Count);
		foreach (CARI_HESAP_HAREKETLERI item4 in toWrite.GetTahsilatHareketleri())
		{
			CARI_HESAP_HAREKETLERI.WriteToBinaryWriter(item4, writer, 1);
		}
		writer.Write(toWrite.GetGenelCariHesapHareketleri().Count);
		foreach (CARI_HESAP_HAREKETLERI item5 in toWrite.GetGenelCariHesapHareketleri())
		{
			CARI_HESAP_HAREKETLERI.WriteToBinaryWriter(item5, writer, 1);
		}
		writer.Write(toWrite.GetDepolarArasiSiparisHareketleri().Count);
		foreach (DEPOLAR_ARASI_SIPARISLER item6 in toWrite.GetDepolarArasiSiparisHareketleri())
		{
			DEPOLAR_ARASI_SIPARISLER.WriteToBinaryWriter(item6, writer, 1);
		}
		writer.Write(toWrite.GetSayimSonuclariHareketleri().Count);
		foreach (SAYIM_SONUCLARI item7 in toWrite.GetSayimSonuclariHareketleri())
		{
			SAYIM_SONUCLARI.WriteToBinaryWriter(item7, writer, 1);
		}
		writer.Write(toWrite.GetBakimKabulHareketleri().Count);
		foreach (BAKIM_KABUL_HAREKETLERI item8 in toWrite.GetBakimKabulHareketleri())
		{
			BAKIM_KABUL_HAREKETLERI.WriteToBinaryWriter(item8, writer, 1);
		}
		if (versiyon >= 2)
		{
			writer.Write(toWrite.EximKodu);
			writer.Write(toWrite.Parametreler.CekiListesi.Olustur);
			writer.Write(toWrite.Parametreler.CekiListesi.AktifAltAmbalajNo);
			writer.Write(toWrite.Parametreler.CekiListesi.AktifAnaAmbalajNo);
			writer.Write(toWrite.CekiListesi.Count);
			foreach (CEKI_LISTESI item9 in toWrite.CekiListesi)
			{
				CEKI_LISTESI.WriteToBinaryWriter(item9, writer, 1);
			}
		}
		if (versiyon >= 3)
		{
			writer.Write(toWrite.Parametreler.vergitanimlari.Count);
			foreach (VergiTanimi item10 in toWrite.Parametreler.vergitanimlari)
			{
				writer.Write(item10.KisaAdi);
				writer.Write(item10.UzunAdi);
				writer.Write(item10.Yuzde);
			}
			writer.Write(toWrite.Parametreler.Miktar2Arttir);
			writer.Write(toWrite.Parametreler.SiparisCagrilabilir_fl);
			writer.Write(toWrite.Parametreler.MaksimumSatirSayisi);
			writer.Write(toWrite.Parametreler.BarkodOkumaSayisiBasarili);
			writer.Write(toWrite.Parametreler.BarkodOkumaSayisiBasarisiz);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Goster);
			writer.Write(toWrite.Parametreler.KoliEtiketi.BluetoothAygitIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Form);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Metin1);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Metin2);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Metin3);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Metin4);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Metin5);
			writer.Write(toWrite.Parametreler.KoliEtiketi.OtomatikYazdir);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Sayac);
			writer.Write(toWrite.Parametreler.KoliEtiketi.Stoklar.Count);
			foreach (Stok item11 in toWrite.Parametreler.KoliEtiketi.Stoklar)
			{
				Stok.WriteToBinaryWriter(item11, writer, 1);
			}
			writer.Write(toWrite.Parametreler.CariBakiye);
			writer.Write(toWrite.Parametreler.CariTanimliKrediTutari);
			writer.Write(toWrite.Parametreler.CariKalanKredisi);
			writer.Write(toWrite.Parametreler.EvrakGirisiBarkodOkuyucuZorunlu);
			writer.Write(toWrite.Parametreler.CariKilitliIseSiparisAlma);
			writer.Write(toWrite.Parametreler.CariKilitliIseFaturaKesme);
			writer.Write(toWrite.Parametreler.CariKilitliIseIrsaliyeKesme);
			writer.Write(toWrite.Parametreler.CariKilitliIseFaturaAlma);
			writer.Write(toWrite.Parametreler.CariKilitliIseIrsaliyeAlma);
			writer.Write(toWrite.Parametreler.CariKilitliIseTahsilatYapma);
		}
		if (versiyon >= 4)
		{
			writer.Write(toWrite._DBCno);
		}
		if (versiyon >= 5)
		{
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiAktif);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumSatirSayisi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiDegiskenAlanKarakterSayisi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiAktifStokKodu);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiAktifStokIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiAktifStokKisaIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiAktifStokYabanciIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiAktifStokMiktar);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiAktifStokBirimAdi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokKodu);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokKisaIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokYabanciIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokMiktar);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokBirimAdi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiIlkDegerStokKodu);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiIlkDegerStokIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiIlkDegerStokKisaIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiIlkDegerStokYabanciIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiIlkDegerStokMiktar);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiIlkDegerStokBirimAdi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMesafeStokKodu);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMesafeStokIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMesafeStokKisaIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMesafeStokYabanciIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMesafeStokMiktar);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMesafeStokBirimAdi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokKodu);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokKisaIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokYabanciIsmi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokMiktar);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokBirimAdi);
			writer.Write(toWrite.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokMiktarOndalikHane);
		}
		if (versiyon >= 6)
		{
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistir);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo0.Sube_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo0.Sube_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo1.Sube_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo1.Sube_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo2.Sube_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo2.Sube_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo3.Sube_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSubeDegistirFirmaNo3.Sube_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistir);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo0.dep_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo0.dep_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo1.dep_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo1.dep_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo2.dep_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo2.dep_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo3.dep_no);
			writer.Write(toWrite.Parametreler.FirmaDegisinceDepoDegistirFirmaNo3.dep_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistir);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0.som_kod);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0.som_isim);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1.som_kod);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1.som_isim);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2.som_kod);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2.som_isim);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3.som_kod);
			writer.Write(toWrite.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3.som_isim);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistir);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo0.pro_kodu);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo0.pro_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo1.pro_kodu);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo1.pro_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo2.pro_kodu);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo2.pro_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo3.pro_kodu);
			writer.Write(toWrite.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo3.pro_adi);
			writer.Write(toWrite.Parametreler.FirmaDegisinceEvrakSeriDegistir);
			writer.Write(toWrite.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo0);
			writer.Write(toWrite.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo1);
			writer.Write(toWrite.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo2);
			writer.Write(toWrite.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo3);
			writer.Write(toWrite.Parametreler.FirmaDegisinceFirmaNo0);
			writer.Write(toWrite.Parametreler.FirmaDegisinceFirmaNo1);
			writer.Write(toWrite.Parametreler.FirmaDegisinceFirmaNo2);
			writer.Write(toWrite.Parametreler.FirmaDegisinceFirmaNo3);
			writer.Write((int)toWrite.Parametreler.sepet_siralama_secenegi);
			writer.Write((int)toWrite.Parametreler.karsilama_siralama_secenegi);
		}
		if (versiyon >= 7)
		{
			writer.Write(toWrite.Parametreler.GormeFirmaNo);
			writer.Write(toWrite.Parametreler.GormeSubeNo);
			writer.Write(toWrite.Parametreler.GormeNormalIade);
			writer.Write(toWrite.Parametreler.GormeTicaretTuru);
			writer.Write(toWrite.Parametreler.GormeEvrakSeri);
			writer.Write(toWrite.Parametreler.GormeEvrakSira);
			writer.Write(toWrite.Parametreler.GormeBelgeNo);
			writer.Write(toWrite.Parametreler.GormeBelgeTarihi);
			writer.Write(toWrite.Parametreler.GormeCariKodu);
			writer.Write(toWrite.Parametreler.GormeCariAdres);
			writer.Write(toWrite.Parametreler.GormeKapamaSekli);
			writer.Write(toWrite.Parametreler.GormeKapamaHesapKodu);
			writer.Write(toWrite.Parametreler.GormeOdemePlani);
			writer.Write(toWrite.Parametreler.GormeFiyatListesi);
			writer.Write(toWrite.Parametreler.GormeDovizCinsi);
			writer.Write(toWrite.Parametreler.GormeKur);
			writer.Write(toWrite.Parametreler.GormeProje);
			writer.Write(toWrite.Parametreler.GormeSorumlulukMerkezi);
			writer.Write(toWrite.Parametreler.GormeKaynakDepo);
			writer.Write(toWrite.Parametreler.GormeNakliyeDepo);
			writer.Write(toWrite.Parametreler.GormeHedefDepo);
			writer.Write(toWrite.Parametreler.GormeTarih);
			writer.Write(toWrite.Parametreler.GormeTeslimTarihi);
			writer.Write(toWrite.Parametreler.GormeAciklama1);
			writer.Write(toWrite.Parametreler.GormeAciklama2);
			writer.Write(toWrite.Parametreler.GormeAciklama3);
			writer.Write(toWrite.Parametreler.GormeAciklama4);
			writer.Write(toWrite.Parametreler.GormeAciklama5);
			writer.Write(toWrite.Parametreler.GormeAciklama6);
			writer.Write(toWrite.Parametreler.GormeAciklama7);
			writer.Write(toWrite.Parametreler.GormeAciklama8);
			writer.Write(toWrite.Parametreler.GormeAciklama9);
			writer.Write(toWrite.Parametreler.GormeAciklama10);
			writer.Write(toWrite.Parametreler.GormeSiparisTeslimTuru);
		}
		if (versiyon >= 8)
		{
			writer.Write(toWrite._StokHareketleri.Count);
			foreach (STOK_HAREKETLERI item12 in toWrite._StokHareketleri)
			{
				writer.Write(item12.renk_beden_hareketleri.Count);
				foreach (BEDEN_HAREKETLERI item13 in item12.renk_beden_hareketleri)
				{
					BEDEN_HAREKETLERI.WriteToBinaryWriter(item13, writer, 1);
				}
			}
			writer.Write(toWrite._Siparisler.Count);
			foreach (SIPARISLER item14 in toWrite._Siparisler)
			{
				writer.Write(item14.renk_beden_hareketleri.Count);
				foreach (BEDEN_HAREKETLERI item15 in item14.renk_beden_hareketleri)
				{
					BEDEN_HAREKETLERI.WriteToBinaryWriter(item15, writer, 1);
				}
			}
			writer.Write(toWrite._DepolarArasiSiparisHareketleri.Count);
			foreach (DEPOLAR_ARASI_SIPARISLER item16 in toWrite._DepolarArasiSiparisHareketleri)
			{
				writer.Write(item16.renk_beden_hareketleri.Count);
				foreach (BEDEN_HAREKETLERI item17 in item16.renk_beden_hareketleri)
				{
					BEDEN_HAREKETLERI.WriteToBinaryWriter(item17, writer, 1);
				}
			}
		}
		if (versiyon >= 9)
		{
			writer.Write(toWrite._StokHareketleri.Count);
			foreach (STOK_HAREKETLERI item18 in toWrite._StokHareketleri)
			{
				writer.Write(item18.stok_serinolari.Count);
				foreach (STOK_SERINO_TANIMLARI item19 in item18.stok_serinolari)
				{
					STOK_SERINO_TANIMLARI.WriteToBinaryWriter(item19, writer, 1);
				}
			}
		}
		if (versiyon >= 10)
		{
			writer.Write(toWrite._StokHareketleri.Count);
			foreach (STOK_HAREKETLERI item20 in toWrite._StokHareketleri)
			{
				writer.Write(item20.sth_Guid.ToByteArray());
				writer.Write(item20.sth_fat_uid.ToByteArray());
				writer.Write(item20.sth_sip_uid.ToByteArray());
				writer.Write(item20.sth_kons_uid.ToByteArray());
				writer.Write(item20.sth_subesip_uid.ToByteArray());
				writer.Write(item20.sth_yetkili_uid.ToByteArray());
			}
		}
		if (versiyon < 11)
		{
			return;
		}
		writer.Write(toWrite._teklifmi);
		writer.Write(toWrite._miktarformulyaz);
		writer.Write(toWrite._StokHareketleri.Count);
		foreach (STOK_HAREKETLERI item21 in toWrite._StokHareketleri)
		{
			writer.Write(item21.sth_Olcu1);
			writer.Write(item21.sth_Olcu2);
			writer.Write(item21.sth_Olcu3);
			writer.Write(item21.sth_Olcu4);
			writer.Write(item21.sth_Olcu5);
			writer.Write(item21.sth_FormulMiktar);
			writer.Write(item21.sth_FormulMiktarNo);
		}
		writer.Write(toWrite._Siparisler.Count);
		foreach (SIPARISLER item22 in toWrite._Siparisler)
		{
			writer.Write(item22.sip_Olcu1);
			writer.Write(item22.sip_Olcu2);
			writer.Write(item22.sip_Olcu3);
			writer.Write(item22.sip_Olcu4);
			writer.Write(item22.sip_Olcu5);
			writer.Write(item22.sip_FormulMiktar);
			writer.Write(item22.sip_FormulMiktarNo);
		}
	}

	public static Evrak ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		Evrak result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static Evrak ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static Evrak ReadFromBinaryReader(BinaryReader reader)
	{
		Evrak evrak = new Evrak();
		int num = reader.ReadInt32();
		evrak.evraktipi = (enum_GenelEvrakTipleri)reader.ReadInt32();
		evrak.kapamasekli = (enum_KapamaSekli)reader.ReadInt32();
		evrak.normaliade = (enum_cha_normal_Iade)reader.ReadInt32();
		evrak.ticaretturu = (enum_cha_ticaret_turu)reader.ReadInt32();
		evrak.aktarimdurumu = (enum_EvrakAktarimDurumu)reader.ReadInt32();
		evrak.SetOfflineRecNo(reader.ReadInt32());
		evrak.SetYeniKayit(reader.ReadBoolean());
		evrak.SetEvrakKilitli(reader.ReadBoolean());
		evrak.SetSiparisKarsilamaMi(reader.ReadBoolean());
		evrak.SetEvrakTarihi(new DateTime(reader.ReadInt64()));
		evrak.SetEvrakNoSeri(reader.ReadString());
		evrak.SetEvraknoSira(reader.ReadInt32());
		evrak.SetBelgeNo(reader.ReadString());
		evrak.SetBelgeTarihi(new DateTime(reader.ReadInt64()));
		evrak.SetOdemePlani(reader.ReadInt32());
		evrak.SetTemsilciKodu(reader.ReadString());
		evrak.SetMikroUserNo(reader.ReadInt32());
		evrak.SetAciklama(reader.ReadString());
		evrak.SetSevkTeslimTarihi(new DateTime(reader.ReadInt64()));
		evrak.SetSevkAdresNo(reader.ReadInt32());
		evrak.SetKapamaHesapKodu(reader.ReadString());
		evrak.SetAciklama1(reader.ReadString());
		evrak.SetAciklama2(reader.ReadString());
		evrak.SetAciklama3(reader.ReadString());
		evrak.SetAciklama4(reader.ReadString());
		evrak.SetAciklama5(reader.ReadString());
		evrak.SetAciklama6(reader.ReadString());
		evrak.SetAciklama7(reader.ReadString());
		evrak.SetAciklama8(reader.ReadString());
		evrak.SetAciklama9(reader.ReadString());
		evrak.SetAciklama10(reader.ReadString());
		evrak.SetSip_TeslimTuru(reader.ReadString());
		evrak.SetDegistirSpecialAlan1(reader.ReadString());
		evrak.SetDegistirSpecialAlan2(reader.ReadString());
		evrak.SetDegistirSpecialAlan3(reader.ReadString());
		evrak.cari = Cari.ReadFromBinaryReader(reader);
		evrak.SetFiyatListesi(FiyatListesi.ReadFromBinaryReader(reader));
		evrak.SetDovizCinsi(reader.ReadInt32());
		evrak.kur = Kur.ReadFromBinaryReader(reader);
		evrak.SetAlternatifDovizCinsi(reader.ReadInt32());
		evrak.alternatifdovizkuru = Kur.ReadFromBinaryReader(reader);
		evrak._kaynakdepo = Depo.ReadFromBinaryReader(reader);
		evrak._hedefdepo = Depo.ReadFromBinaryReader(reader);
		evrak._nakliyedepo = Depo.ReadFromBinaryReader(reader);
		evrak._proje = Proje.ReadFromBinaryReader(reader);
		evrak._sorumlulukmerkezi = SorumlulukMerkezi.ReadFromBinaryReader(reader);
		evrak.SetFirma(Firma.ReadFromBinaryReader(reader));
		evrak.SetSube(Sube.ReadFromBinaryReader(reader));
		int num2 = reader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			evrak.AddStokHareketi(STOK_HAREKETLERI.ReadFromBinaryReader(reader));
		}
		int num3 = reader.ReadInt32();
		for (int j = 0; j < num3; j++)
		{
			evrak.AddSiparisHareketi(SIPARISLER.ReadFromBinaryReader(reader), SiparisKarsilamaYap: false);
		}
		evrak.SetStokCariHesapHareketi(CARI_HESAP_HAREKETLERI.ReadFromBinaryReader(reader));
		evrak.SetStokFiyatFarkiCariHesapHareketi(CARI_HESAP_HAREKETLERI.ReadFromBinaryReader(reader));
		int num4 = reader.ReadInt32();
		for (int k = 0; k < num4; k++)
		{
			evrak.AddHizmetHareketi(CARI_HESAP_HAREKETLERI.ReadFromBinaryReader(reader));
		}
		int num5 = reader.ReadInt32();
		for (int l = 0; l < num5; l++)
		{
			evrak.AddTahsilatHareketleri(CARI_HESAP_HAREKETLERI.ReadFromBinaryReader(reader));
		}
		int num6 = reader.ReadInt32();
		for (int m = 0; m < num6; m++)
		{
			evrak.AddGenelCariHesapHareketleri(CARI_HESAP_HAREKETLERI.ReadFromBinaryReader(reader));
		}
		int num7 = reader.ReadInt32();
		for (int n = 0; n < num7; n++)
		{
			evrak.AddDepolarArasiSiparisHareketi(DEPOLAR_ARASI_SIPARISLER.ReadFromBinaryReader(reader), SiparisKarsilamaYap: false);
		}
		int num8 = reader.ReadInt32();
		for (int num9 = 0; num9 < num8; num9++)
		{
			evrak.AddSayimSonuclariHareketi(SAYIM_SONUCLARI.ReadFromBinaryReader(reader));
		}
		int num10 = reader.ReadInt32();
		for (int num11 = 0; num11 < num10; num11++)
		{
			evrak.AddBakimKabulHareketi(BAKIM_KABUL_HAREKETLERI.ReadFromBinaryReader(reader));
		}
		if (num >= 2)
		{
			evrak.SetEximKodu(reader.ReadString());
			evrak.Parametreler.CekiListesi.Olustur = reader.ReadBoolean();
			evrak.Parametreler.CekiListesi.AktifAltAmbalajNo = reader.ReadInt32();
			evrak.Parametreler.CekiListesi.AktifAnaAmbalajNo = reader.ReadInt32();
			int num12 = reader.ReadInt32();
			for (int num13 = 0; num13 < num12; num13++)
			{
				evrak.CekiListesi.Add(CEKI_LISTESI.ReadFromBinaryReader(reader));
			}
		}
		if (num >= 3)
		{
			evrak.Parametreler.vergitanimlari = new List<VergiTanimi>();
			int num14 = reader.ReadInt32();
			for (int num15 = 0; num15 < num14; num15++)
			{
				VergiTanimi vergiTanimi = new VergiTanimi();
				vergiTanimi.KisaAdi = reader.ReadString();
				vergiTanimi.UzunAdi = reader.ReadString();
				vergiTanimi.Yuzde = reader.ReadDouble();
				evrak.Parametreler.vergitanimlari.Add(vergiTanimi);
			}
			evrak.Parametreler.Miktar2Arttir = reader.ReadBoolean();
			evrak.Parametreler.SiparisCagrilabilir_fl = reader.ReadBoolean();
			evrak.Parametreler.MaksimumSatirSayisi = reader.ReadInt32();
			evrak.Parametreler.BarkodOkumaSayisiBasarili = reader.ReadInt32();
			evrak.Parametreler.BarkodOkumaSayisiBasarisiz = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.Goster = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.BluetoothAygitIsmi = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.Form = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.Metin1 = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.Metin2 = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.Metin3 = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.Metin4 = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.Metin5 = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.OtomatikYazdir = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.Sayac = reader.ReadInt32();
			int num16 = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.Stoklar = new List<Stok>();
			for (int num17 = 0; num17 < num16; num17++)
			{
				evrak.Parametreler.KoliEtiketi.Stoklar.Add(Stok.ReadFromBinaryReader(reader));
			}
			evrak.Parametreler.CariBakiye = reader.ReadDouble();
			evrak.Parametreler.CariTanimliKrediTutari = reader.ReadDouble();
			evrak.Parametreler.CariKalanKredisi = reader.ReadDouble();
			evrak.Parametreler.EvrakGirisiBarkodOkuyucuZorunlu = reader.ReadBoolean();
			evrak.Parametreler.CariKilitliIseSiparisAlma = reader.ReadBoolean();
			evrak.Parametreler.CariKilitliIseFaturaKesme = reader.ReadBoolean();
			evrak.Parametreler.CariKilitliIseIrsaliyeKesme = reader.ReadBoolean();
			evrak.Parametreler.CariKilitliIseFaturaAlma = reader.ReadBoolean();
			evrak.Parametreler.CariKilitliIseIrsaliyeAlma = reader.ReadBoolean();
			evrak.Parametreler.CariKilitliIseTahsilatYapma = reader.ReadBoolean();
		}
		if (num >= 4)
		{
			evrak._DBCno = reader.ReadInt32();
		}
		if (num >= 5)
		{
			evrak.Parametreler.KoliEtiketi.StokListesiAktif = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumSatirSayisi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiDegiskenAlanKarakterSayisi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiAktifStokKodu = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.StokListesiAktifStokIsmi = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.StokListesiAktifStokKisaIsmi = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.StokListesiAktifStokYabanciIsmi = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.StokListesiAktifStokMiktar = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.StokListesiAktifStokBirimAdi = reader.ReadBoolean();
			evrak.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokKodu = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokIsmi = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokKisaIsmi = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokYabanciIsmi = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokMiktar = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.StokListesiBaslangicMetniStokBirimAdi = reader.ReadString();
			evrak.Parametreler.KoliEtiketi.StokListesiIlkDegerStokKodu = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiIlkDegerStokIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiIlkDegerStokKisaIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiIlkDegerStokYabanciIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiIlkDegerStokMiktar = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiIlkDegerStokBirimAdi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMesafeStokKodu = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMesafeStokIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMesafeStokKisaIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMesafeStokYabanciIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMesafeStokMiktar = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMesafeStokBirimAdi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokKodu = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokKisaIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokYabanciIsmi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokMiktar = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokBirimAdi = reader.ReadInt32();
			evrak.Parametreler.KoliEtiketi.StokListesiMaksimumKarakterStokMiktarOndalikHane = reader.ReadInt32();
		}
		if (num >= 6)
		{
			evrak.Parametreler.FirmaDegisinceSubeDegistir = reader.ReadBoolean();
			evrak.Parametreler.FirmaDegisinceSubeDegistirFirmaNo0 = new Sube(reader.ReadInt32(), reader.ReadString());
			evrak.Parametreler.FirmaDegisinceSubeDegistirFirmaNo1 = new Sube(reader.ReadInt32(), reader.ReadString());
			evrak.Parametreler.FirmaDegisinceSubeDegistirFirmaNo2 = new Sube(reader.ReadInt32(), reader.ReadString());
			evrak.Parametreler.FirmaDegisinceSubeDegistirFirmaNo3 = new Sube(reader.ReadInt32(), reader.ReadString());
			evrak.Parametreler.FirmaDegisinceDepoDegistir = reader.ReadBoolean();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo0 = new Depo();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo0.dep_no = reader.ReadInt32();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo0.dep_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo1 = new Depo();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo1.dep_no = reader.ReadInt32();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo1.dep_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo2 = new Depo();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo2.dep_no = reader.ReadInt32();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo2.dep_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo3 = new Depo();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo3.dep_no = reader.ReadInt32();
			evrak.Parametreler.FirmaDegisinceDepoDegistirFirmaNo3.dep_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistir = reader.ReadBoolean();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0 = new SorumlulukMerkezi();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0.som_kod = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0.som_isim = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1 = new SorumlulukMerkezi();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1.som_kod = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1.som_isim = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2 = new SorumlulukMerkezi();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2.som_kod = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2.som_isim = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3 = new SorumlulukMerkezi();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3.som_kod = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3.som_isim = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistir = reader.ReadBoolean();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo0 = new Proje();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo0.pro_kodu = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo0.pro_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo1 = new Proje();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo1.pro_kodu = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo1.pro_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo2 = new Proje();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo2.pro_kodu = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo2.pro_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo3 = new Proje();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo3.pro_kodu = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceProjeKoduDegistirFirmaNo3.pro_adi = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceEvrakSeriDegistir = reader.ReadBoolean();
			evrak.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo0 = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo1 = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo2 = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceEvrakSeriDegistirFirmaNo3 = reader.ReadString();
			evrak.Parametreler.FirmaDegisinceFirmaNo0 = reader.ReadInt32();
			evrak.Parametreler.FirmaDegisinceFirmaNo1 = reader.ReadInt32();
			evrak.Parametreler.FirmaDegisinceFirmaNo2 = reader.ReadInt32();
			evrak.Parametreler.FirmaDegisinceFirmaNo3 = reader.ReadInt32();
			evrak.Parametreler.sepet_siralama_secenegi = (enum_evrak_sepet_siralama_secenekleri)reader.ReadInt32();
			evrak.Parametreler.karsilama_siralama_secenegi = (enum_evrak_karsilama_siralama_secenekleri)reader.ReadInt32();
		}
		if (num >= 7)
		{
			evrak.Parametreler.GormeFirmaNo = reader.ReadBoolean();
			evrak.Parametreler.GormeSubeNo = reader.ReadBoolean();
			evrak.Parametreler.GormeNormalIade = reader.ReadBoolean();
			evrak.Parametreler.GormeTicaretTuru = reader.ReadBoolean();
			evrak.Parametreler.GormeEvrakSeri = reader.ReadBoolean();
			evrak.Parametreler.GormeEvrakSira = reader.ReadBoolean();
			evrak.Parametreler.GormeBelgeNo = reader.ReadBoolean();
			evrak.Parametreler.GormeBelgeTarihi = reader.ReadBoolean();
			evrak.Parametreler.GormeCariKodu = reader.ReadBoolean();
			evrak.Parametreler.GormeCariAdres = reader.ReadBoolean();
			evrak.Parametreler.GormeKapamaSekli = reader.ReadBoolean();
			evrak.Parametreler.GormeKapamaHesapKodu = reader.ReadBoolean();
			evrak.Parametreler.GormeOdemePlani = reader.ReadBoolean();
			evrak.Parametreler.GormeFiyatListesi = reader.ReadBoolean();
			evrak.Parametreler.GormeDovizCinsi = reader.ReadBoolean();
			evrak.Parametreler.GormeKur = reader.ReadBoolean();
			evrak.Parametreler.GormeProje = reader.ReadBoolean();
			evrak.Parametreler.GormeSorumlulukMerkezi = reader.ReadBoolean();
			evrak.Parametreler.GormeKaynakDepo = reader.ReadBoolean();
			evrak.Parametreler.GormeNakliyeDepo = reader.ReadBoolean();
			evrak.Parametreler.GormeHedefDepo = reader.ReadBoolean();
			evrak.Parametreler.GormeTarih = reader.ReadBoolean();
			evrak.Parametreler.GormeTeslimTarihi = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama1 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama2 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama3 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama4 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama5 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama6 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama7 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama8 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama9 = reader.ReadBoolean();
			evrak.Parametreler.GormeAciklama10 = reader.ReadBoolean();
			evrak.Parametreler.GormeSiparisTeslimTuru = reader.ReadBoolean();
		}
		if (num >= 8)
		{
			int num18 = 0;
			int num19 = 0;
			num18 = reader.ReadInt32();
			for (int num20 = 0; num20 < num18; num20++)
			{
				num19 = reader.ReadInt32();
				for (int num21 = 0; num21 < num19; num21++)
				{
					evrak._StokHareketleri[num20].renk_beden_hareketleri.Add(BEDEN_HAREKETLERI.ReadFromBinaryReader(reader));
				}
			}
			num18 = reader.ReadInt32();
			for (int num22 = 0; num22 < num18; num22++)
			{
				num19 = reader.ReadInt32();
				for (int num23 = 0; num23 < num19; num23++)
				{
					evrak._Siparisler[num22].renk_beden_hareketleri.Add(BEDEN_HAREKETLERI.ReadFromBinaryReader(reader));
				}
			}
			num18 = reader.ReadInt32();
			for (int num24 = 0; num24 < num18; num24++)
			{
				num19 = reader.ReadInt32();
				for (int num25 = 0; num25 < num19; num25++)
				{
					evrak._DepolarArasiSiparisHareketleri[num24].renk_beden_hareketleri.Add(BEDEN_HAREKETLERI.ReadFromBinaryReader(reader));
				}
			}
		}
		if (num >= 9)
		{
			int num26 = 0;
			int num27 = 0;
			num26 = reader.ReadInt32();
			for (int num28 = 0; num28 < num26; num28++)
			{
				num27 = reader.ReadInt32();
				for (int num29 = 0; num29 < num27; num29++)
				{
					evrak._StokHareketleri[num28].stok_serinolari.Add(STOK_SERINO_TANIMLARI.ReadFromBinaryReader(reader));
				}
			}
		}
		if (num >= 10)
		{
			int num30 = 0;
			num30 = reader.ReadInt32();
			for (int num31 = 0; num31 < num30; num31++)
			{
				evrak._StokHareketleri[num31].sth_Guid = new Guid(reader.ReadBytes(16));
				evrak._StokHareketleri[num31].sth_fat_uid = new Guid(reader.ReadBytes(16));
				evrak._StokHareketleri[num31].sth_sip_uid = new Guid(reader.ReadBytes(16));
				evrak._StokHareketleri[num31].sth_kons_uid = new Guid(reader.ReadBytes(16));
				evrak._StokHareketleri[num31].sth_subesip_uid = new Guid(reader.ReadBytes(16));
				evrak._StokHareketleri[num31].sth_yetkili_uid = new Guid(reader.ReadBytes(16));
			}
		}
		if (num >= 11)
		{
			evrak._teklifmi = reader.ReadBoolean();
			evrak._miktarformulyaz = reader.ReadBoolean();
			int num32 = 0;
			num32 = reader.ReadInt32();
			for (int num33 = 0; num33 < num32; num33++)
			{
				evrak._StokHareketleri[num33].sth_Olcu1 = reader.ReadDouble();
				evrak._StokHareketleri[num33].sth_Olcu2 = reader.ReadDouble();
				evrak._StokHareketleri[num33].sth_Olcu3 = reader.ReadDouble();
				evrak._StokHareketleri[num33].sth_Olcu4 = reader.ReadDouble();
				evrak._StokHareketleri[num33].sth_Olcu5 = reader.ReadDouble();
				evrak._StokHareketleri[num33].sth_FormulMiktar = reader.ReadDouble();
				evrak._StokHareketleri[num33].sth_FormulMiktarNo = reader.ReadInt32();
			}
			num32 = reader.ReadInt32();
			for (int num34 = 0; num34 < num32; num34++)
			{
				evrak._Siparisler[num34].sip_Olcu1 = reader.ReadDouble();
				evrak._Siparisler[num34].sip_Olcu2 = reader.ReadDouble();
				evrak._Siparisler[num34].sip_Olcu3 = reader.ReadDouble();
				evrak._Siparisler[num34].sip_Olcu4 = reader.ReadDouble();
				evrak._Siparisler[num34].sip_Olcu5 = reader.ReadDouble();
				evrak._Siparisler[num34].sip_FormulMiktar = reader.ReadDouble();
				evrak._Siparisler[num34].sip_FormulMiktarNo = reader.ReadInt32();
			}
		}
		return evrak;
	}
}
