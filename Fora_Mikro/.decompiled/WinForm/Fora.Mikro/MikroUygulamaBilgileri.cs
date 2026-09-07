using System.Collections.Generic;
using Fora.Mikro.Dovizler;
using Fora.Mikro.MikroKullanicilari;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;
using Fora.Mikro.Vergiler;
using Fora.Mikro.Veritabanlari;

namespace Fora.Mikro;

public class MikroUygulamaBilgileri
{
	public List<VergiTanimi> vergitanimlari;

	public int AktarilanEvrakSayisi;

	public SqlBaglantiBilgileri baglantibilgileri { get; set; }

	public Veritabani veritabani { get; set; }

	public MikroKullanici mikrokullanici { get; set; }

	public List<ForaAppWinAnaMenu> anamenu { get; set; }

	public string KullaniciAdi { get; set; }

	public Parametreler KullaniciParametreleri { get; set; }

	public Parametreler GenelParametreler { get; set; }

	public DovizCinsiTanimlari doviz_cinsi_tanimlari { get; set; }

	public int FirmaNo { get; set; }

	public int SubeNo { get; set; }

	public string MikroAnaDBName { get; set; }

	public string MikroFirmaDBName => $"{MikroAnaDBName}_{veritabani.DB_kod}";

	public MikroUygulamaBilgileri(string mikroanadbname)
	{
		baglantibilgileri = new SqlBaglantiBilgileri();
		veritabani = new Veritabani();
		mikrokullanici = new MikroKullanici();
		KullaniciParametreleri = new Parametreler();
		MikroAnaDBName = mikroanadbname;
		KullaniciAdi = "";
		FirmaNo = 0;
		SubeNo = 0;
		AktarilanEvrakSayisi = 0;
		ResetGenelParametreler();
	}

	public void ResetGenelParametreler()
	{
		GenelParametreler = ParametrelerDefault.ForaMikro();
	}
}
