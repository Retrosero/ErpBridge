using System;
using System.Collections.Generic;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Senkronizasyon;
using Fora.Mikro.Siparis;
using Fora.Mikro.StokHareket;

namespace Fora.Mikro.DepolarArasiSiparisler;

public class GetDepolarArasiSiparisEvrakListTask
{
	public delegate void GetDepolarArasiSiparisEvrakListBittiHandler(object sender, List<SiparisEvrakListItem> SonucListe, string Cevap);

	private string _sip_evrakseri;

	private int _kaynak_depo_no;

	private int _hedef_depo_no;

	private enum_sip_orderby _sip_orderby;

	private siparis_teslim_durumu _teslim_durumu;

	private List<SiparisEvrakListItem> _SonucListe;

	private string _Cevap;

	private ServisKontrolu servis_kontrolu;

	public event GetDepolarArasiSiparisEvrakListBittiHandler OnGetDepolarArasiSiparisEvrakListBitti;

	public void Baslat(string sip_evrakseri, int kaynak_depo_no, int hedef_depo_no, enum_sip_orderby sip_orderby, siparis_teslim_durumu teslim_durumu)
	{
		_sip_evrakseri = sip_evrakseri;
		_kaynak_depo_no = kaynak_depo_no;
		_hedef_depo_no = hedef_depo_no;
		_sip_orderby = sip_orderby;
		_teslim_durumu = teslim_durumu;
		_Cevap = "";
		Main();
	}

	public void Main()
	{
		try
		{
			_SonucListe = new List<SiparisEvrakListItem>();
			_Cevap = "";
			if (AppBase.KullaniciParametreleri._GetParametre("SenkronizeEt_DEPOLAR_ARASI_SIPARISLER")._GetBoolean)
			{
				_SonucListe = DepolarArasiSiparislerSqlite.GetDepolarArasiSiparisEvrakList(AppBase.GetDbConnectionSync(), _sip_evrakseri, _kaynak_depo_no, _hedef_depo_no, _sip_orderby, _teslim_durumu);
			}
			else
			{
				servis_kontrolu = new ServisKontrolu(1);
				if (servis_kontrolu.Sonuc != enum_servis_kontrol_sonuc.ServisCalisiyor)
				{
					switch (servis_kontrolu.Sonuc)
					{
					case enum_servis_kontrol_sonuc.AgBaglantisiYok:
						_Cevap = "Hata : " + AppResource.guncelleme_internete_baglanilamadi;
						break;
					case enum_servis_kontrol_sonuc.ServiseErisilemedi:
						_Cevap = "Hata : " + AppResource.guncelleme_servise_erisilemedi;
						break;
					case enum_servis_kontrol_sonuc.ServisVersiyonuEski:
						_Cevap = "Hata : " + AppResource.guncelleme_servis_guncelleme_gerekmekte;
						break;
					}
				}
				if (_Cevap == "")
				{
					try
					{
						_SonucListe = DepolarArasiSevkRestClient.GetDepolarArasiSiparisEvrakListV3(servis_kontrolu.ServisAdresi, AppBase._FirmaID, AppBase._KullaniciAdi, AppBase._Sifre, _kaynak_depo_no, _hedef_depo_no, _sip_orderby);
					}
					catch (Exception ex)
					{
						_Cevap = "Hata : " + AppResource.genel_bilgiler_cekilemedi + " : " + ex.ToString();
					}
				}
			}
		}
		catch (Exception ex2)
		{
			_Cevap = "Hata : " + ex2.ToString();
		}
		servis_kontrolu = null;
		this.OnGetDepolarArasiSiparisEvrakListBitti(this, _SonucListe, _Cevap);
	}
}
