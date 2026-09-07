using System.Collections.Generic;
using Fora.Mikro.Depolar;
using Fora.Mikro.DisTicaret;
using Fora.Mikro.Projeler;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Subeler;
using Fora.Mikro.Vergiler;

namespace Fora.Mikro.Evraklar;

public class EvrakParametreler
{
	public class KoliEtiketiParametreler
	{
		private EvrakParametreler _parent;

		public bool Goster { get; set; }

		public string BluetoothAygitIsmi { get; set; }

		public string Form { get; set; }

		public string Metin1 { get; set; }

		public string Metin2 { get; set; }

		public string Metin3 { get; set; }

		public string Metin4 { get; set; }

		public string Metin5 { get; set; }

		public bool OtomatikYazdir { get; set; }

		public int Sayac { get; set; }

		public List<Stok> Stoklar { get; set; }

		public bool StokListesiAktif { get; set; }

		public int StokListesiMaksimumSatirSayisi { get; set; }

		public int StokListesiDegiskenAlanKarakterSayisi { get; set; }

		public bool StokListesiAktifStokKodu { get; set; }

		public bool StokListesiAktifStokIsmi { get; set; }

		public bool StokListesiAktifStokKisaIsmi { get; set; }

		public bool StokListesiAktifStokYabanciIsmi { get; set; }

		public bool StokListesiAktifStokMiktar { get; set; }

		public bool StokListesiAktifStokBirimAdi { get; set; }

		public string StokListesiBaslangicMetniStokKodu { get; set; }

		public string StokListesiBaslangicMetniStokIsmi { get; set; }

		public string StokListesiBaslangicMetniStokKisaIsmi { get; set; }

		public string StokListesiBaslangicMetniStokYabanciIsmi { get; set; }

		public string StokListesiBaslangicMetniStokMiktar { get; set; }

		public string StokListesiBaslangicMetniStokBirimAdi { get; set; }

		public int StokListesiIlkDegerStokKodu { get; set; }

		public int StokListesiIlkDegerStokIsmi { get; set; }

		public int StokListesiIlkDegerStokKisaIsmi { get; set; }

		public int StokListesiIlkDegerStokYabanciIsmi { get; set; }

		public int StokListesiIlkDegerStokMiktar { get; set; }

		public int StokListesiIlkDegerStokBirimAdi { get; set; }

		public int StokListesiMesafeStokKodu { get; set; }

		public int StokListesiMesafeStokIsmi { get; set; }

		public int StokListesiMesafeStokKisaIsmi { get; set; }

		public int StokListesiMesafeStokYabanciIsmi { get; set; }

		public int StokListesiMesafeStokMiktar { get; set; }

		public int StokListesiMesafeStokBirimAdi { get; set; }

		public int StokListesiMaksimumKarakterStokKodu { get; set; }

		public int StokListesiMaksimumKarakterStokIsmi { get; set; }

		public int StokListesiMaksimumKarakterStokKisaIsmi { get; set; }

		public int StokListesiMaksimumKarakterStokYabanciIsmi { get; set; }

		public int StokListesiMaksimumKarakterStokMiktar { get; set; }

		public int StokListesiMaksimumKarakterStokBirimAdi { get; set; }

		public int StokListesiMaksimumKarakterStokMiktarOndalikHane { get; set; }

		public KoliEtiketiParametreler(EvrakParametreler parent)
		{
			_parent = parent;
			Goster = false;
			BluetoothAygitIsmi = "";
			Form = "";
			Metin1 = "";
			Metin2 = "";
			Metin3 = "";
			Metin4 = "";
			Metin5 = "";
			OtomatikYazdir = false;
			Stoklar = new List<Stok>();
			Sayac = 1;
			StokListesiAktif = false;
			StokListesiMaksimumSatirSayisi = 10;
			StokListesiDegiskenAlanKarakterSayisi = 4;
			StokListesiAktifStokKodu = false;
			StokListesiAktifStokIsmi = false;
			StokListesiAktifStokKisaIsmi = false;
			StokListesiAktifStokYabanciIsmi = false;
			StokListesiAktifStokMiktar = false;
			StokListesiAktifStokBirimAdi = false;
			StokListesiBaslangicMetniStokKodu = "49113010064[degisken]";
			StokListesiBaslangicMetniStokIsmi = "49113010064[degisken]";
			StokListesiBaslangicMetniStokKisaIsmi = "49113010064[degisken]";
			StokListesiBaslangicMetniStokYabanciIsmi = "49113010064[degisken]";
			StokListesiBaslangicMetniStokMiktar = "49113010064[degisken]";
			StokListesiBaslangicMetniStokBirimAdi = "49113010064[degisken]";
			StokListesiIlkDegerStokKodu = 270;
			StokListesiIlkDegerStokIsmi = 270;
			StokListesiIlkDegerStokKisaIsmi = 270;
			StokListesiIlkDegerStokYabanciIsmi = 270;
			StokListesiIlkDegerStokMiktar = 270;
			StokListesiIlkDegerStokBirimAdi = 270;
			StokListesiMesafeStokKodu = 10;
			StokListesiMesafeStokIsmi = 10;
			StokListesiMesafeStokKisaIsmi = 10;
			StokListesiMesafeStokYabanciIsmi = 10;
			StokListesiMesafeStokMiktar = 10;
			StokListesiMesafeStokBirimAdi = 10;
			StokListesiMaksimumKarakterStokKodu = 5;
			StokListesiMaksimumKarakterStokIsmi = 15;
			StokListesiMaksimumKarakterStokKisaIsmi = 15;
			StokListesiMaksimumKarakterStokYabanciIsmi = 15;
			StokListesiMaksimumKarakterStokMiktar = 5;
			StokListesiMaksimumKarakterStokBirimAdi = 5;
			StokListesiMaksimumKarakterStokMiktarOndalikHane = 1;
		}
	}

	public class CekiListesiParametreler
	{
		private EvrakParametreler _parent;

		public bool Olustur { get; set; }

		public int AktifAnaAmbalajNo { get; set; }

		public int AktifAltAmbalajNo { get; set; }

		public CekiListesiParametreler(EvrakParametreler parent)
		{
			_parent = parent;
			Olustur = false;
			AktifAltAmbalajNo = 1;
			AktifAnaAmbalajNo = 1;
		}

		public double GetAktifMiktar(string stok_kodu)
		{
			double num = 0.0;
			foreach (CEKI_LISTESI item in _parent._parent.CekiListesi)
			{
				if (item.Ckl_StokKodu == stok_kodu && item.Ckl_AnaAmbalajNo == AktifAnaAmbalajNo && item.Ckl_AltAmbalajNo == AktifAltAmbalajNo)
				{
					num += item.Ckl_Miktari;
				}
			}
			return num;
		}
	}

	private Evrak _parent;

	public List<VergiTanimi> vergitanimlari { get; set; }

	public bool Miktar2Arttir { get; set; }

	public bool SiparisCagrilabilir_fl { get; set; }

	public int MaksimumSatirSayisi { get; set; }

	public int BarkodOkumaSayisiBasarili { get; set; }

	public int BarkodOkumaSayisiBasarisiz { get; set; }

	public bool EvrakGirisiBarkodOkuyucuZorunlu { get; set; }

	public KoliEtiketiParametreler KoliEtiketi { get; set; }

	public CekiListesiParametreler CekiListesi { get; set; }

	public double CariBakiye { get; set; }

	public double CariTanimliKrediTutari { get; set; }

	public double CariKalanKredisi { get; set; }

	public bool CariKilitliIseSiparisAlma { get; set; }

	public bool CariKilitliIseFaturaKesme { get; set; }

	public bool CariKilitliIseIrsaliyeKesme { get; set; }

	public bool CariKilitliIseFaturaAlma { get; set; }

	public bool CariKilitliIseIrsaliyeAlma { get; set; }

	public bool CariKilitliIseTahsilatYapma { get; set; }

	public bool FirmaDegisinceSubeDegistir { get; set; }

	public Sube FirmaDegisinceSubeDegistirFirmaNo0 { get; set; }

	public Sube FirmaDegisinceSubeDegistirFirmaNo1 { get; set; }

	public Sube FirmaDegisinceSubeDegistirFirmaNo2 { get; set; }

	public Sube FirmaDegisinceSubeDegistirFirmaNo3 { get; set; }

	public bool FirmaDegisinceDepoDegistir { get; set; }

	public Depo FirmaDegisinceDepoDegistirFirmaNo0 { get; set; }

	public Depo FirmaDegisinceDepoDegistirFirmaNo1 { get; set; }

	public Depo FirmaDegisinceDepoDegistirFirmaNo2 { get; set; }

	public Depo FirmaDegisinceDepoDegistirFirmaNo3 { get; set; }

	public bool FirmaDegisinceSorumlulukMerkeziDegistir { get; set; }

	public SorumlulukMerkezi FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0 { get; set; }

	public SorumlulukMerkezi FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1 { get; set; }

	public SorumlulukMerkezi FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2 { get; set; }

	public SorumlulukMerkezi FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3 { get; set; }

	public bool FirmaDegisinceProjeKoduDegistir { get; set; }

	public Proje FirmaDegisinceProjeKoduDegistirFirmaNo0 { get; set; }

	public Proje FirmaDegisinceProjeKoduDegistirFirmaNo1 { get; set; }

	public Proje FirmaDegisinceProjeKoduDegistirFirmaNo2 { get; set; }

	public Proje FirmaDegisinceProjeKoduDegistirFirmaNo3 { get; set; }

	public bool FirmaDegisinceEvrakSeriDegistir { get; set; }

	public string FirmaDegisinceEvrakSeriDegistirFirmaNo0 { get; set; }

	public string FirmaDegisinceEvrakSeriDegistirFirmaNo1 { get; set; }

	public string FirmaDegisinceEvrakSeriDegistirFirmaNo2 { get; set; }

	public string FirmaDegisinceEvrakSeriDegistirFirmaNo3 { get; set; }

	public int FirmaDegisinceFirmaNo0 { get; set; }

	public int FirmaDegisinceFirmaNo1 { get; set; }

	public int FirmaDegisinceFirmaNo2 { get; set; }

	public int FirmaDegisinceFirmaNo3 { get; set; }

	public enum_evrak_sepet_siralama_secenekleri sepet_siralama_secenegi { get; set; }

	public enum_evrak_karsilama_siralama_secenekleri karsilama_siralama_secenegi { get; set; }

	public bool GormeFirmaNo { get; set; }

	public bool GormeSubeNo { get; set; }

	public bool GormeNormalIade { get; set; }

	public bool GormeTicaretTuru { get; set; }

	public bool GormeEvrakSeri { get; set; }

	public bool GormeEvrakSira { get; set; }

	public bool GormeBelgeNo { get; set; }

	public bool GormeBelgeTarihi { get; set; }

	public bool GormeCariKodu { get; set; }

	public bool GormeCariAdres { get; set; }

	public bool GormeKapamaSekli { get; set; }

	public bool GormeKapamaHesapKodu { get; set; }

	public bool GormeOdemePlani { get; set; }

	public bool GormeFiyatListesi { get; set; }

	public bool GormeDovizCinsi { get; set; }

	public bool GormeKur { get; set; }

	public bool GormeProje { get; set; }

	public bool GormeSorumlulukMerkezi { get; set; }

	public bool GormeKaynakDepo { get; set; }

	public bool GormeNakliyeDepo { get; set; }

	public bool GormeHedefDepo { get; set; }

	public bool GormeTarih { get; set; }

	public bool GormeTeslimTarihi { get; set; }

	public bool GormeAciklama1 { get; set; }

	public bool GormeAciklama2 { get; set; }

	public bool GormeAciklama3 { get; set; }

	public bool GormeAciklama4 { get; set; }

	public bool GormeAciklama5 { get; set; }

	public bool GormeAciklama6 { get; set; }

	public bool GormeAciklama7 { get; set; }

	public bool GormeAciklama8 { get; set; }

	public bool GormeAciklama9 { get; set; }

	public bool GormeAciklama10 { get; set; }

	public bool GormeSiparisTeslimTuru { get; set; }

	public EvrakParametreler(Evrak parent)
	{
		_parent = parent;
		vergitanimlari = new List<VergiTanimi>();
		VergiTanimi item = new VergiTanimi
		{
			KisaAdi = "Tanımsız",
			UzunAdi = "TANIMSIZ",
			Yuzde = 0.0
		};
		VergiTanimi item2 = new VergiTanimi
		{
			KisaAdi = "Yok",
			UzunAdi = "YOK",
			Yuzde = 0.0
		};
		VergiTanimi item3 = new VergiTanimi
		{
			KisaAdi = "%1",
			UzunAdi = "K.D.V. (%) 1",
			Yuzde = 1.0
		};
		VergiTanimi item4 = new VergiTanimi
		{
			KisaAdi = "%8",
			UzunAdi = "K.D.V. (%) 8",
			Yuzde = 8.0
		};
		VergiTanimi item5 = new VergiTanimi
		{
			KisaAdi = "%18",
			UzunAdi = "K.D.V. (%) 18",
			Yuzde = 18.0
		};
		VergiTanimi item6 = new VergiTanimi
		{
			KisaAdi = "%26",
			UzunAdi = "K.D.V. (%) 26",
			Yuzde = 26.0
		};
		VergiTanimi item7 = new VergiTanimi
		{
			KisaAdi = "Özel Mahtah",
			UzunAdi = "ÖZEL MATRAH",
			Yuzde = 0.0
		};
		VergiTanimi item8 = new VergiTanimi
		{
			KisaAdi = "",
			UzunAdi = "",
			Yuzde = 0.0
		};
		VergiTanimi item9 = new VergiTanimi
		{
			KisaAdi = "",
			UzunAdi = "",
			Yuzde = 0.0
		};
		VergiTanimi item10 = new VergiTanimi
		{
			KisaAdi = "",
			UzunAdi = "",
			Yuzde = 0.0
		};
		VergiTanimi item11 = new VergiTanimi
		{
			KisaAdi = "",
			UzunAdi = "",
			Yuzde = 0.0
		};
		vergitanimlari.Add(item);
		vergitanimlari.Add(item2);
		vergitanimlari.Add(item3);
		vergitanimlari.Add(item4);
		vergitanimlari.Add(item5);
		vergitanimlari.Add(item6);
		vergitanimlari.Add(item7);
		vergitanimlari.Add(item8);
		vergitanimlari.Add(item9);
		vergitanimlari.Add(item10);
		vergitanimlari.Add(item11);
		Miktar2Arttir = false;
		SiparisCagrilabilir_fl = true;
		MaksimumSatirSayisi = 0;
		BarkodOkumaSayisiBasarili = 0;
		BarkodOkumaSayisiBasarisiz = 0;
		EvrakGirisiBarkodOkuyucuZorunlu = false;
		KoliEtiketi = new KoliEtiketiParametreler(this);
		CekiListesi = new CekiListesiParametreler(this);
		CariBakiye = 0.0;
		CariTanimliKrediTutari = 0.0;
		CariKalanKredisi = 0.0;
		CariKilitliIseSiparisAlma = true;
		CariKilitliIseFaturaKesme = true;
		CariKilitliIseIrsaliyeKesme = true;
		CariKilitliIseFaturaAlma = true;
		CariKilitliIseIrsaliyeAlma = true;
		CariKilitliIseTahsilatYapma = true;
		FirmaDegisinceSubeDegistir = false;
		FirmaDegisinceSubeDegistirFirmaNo0 = new Sube();
		FirmaDegisinceSubeDegistirFirmaNo1 = new Sube();
		FirmaDegisinceSubeDegistirFirmaNo2 = new Sube();
		FirmaDegisinceSubeDegistirFirmaNo3 = new Sube();
		FirmaDegisinceDepoDegistir = false;
		FirmaDegisinceDepoDegistirFirmaNo0 = new Depo();
		FirmaDegisinceDepoDegistirFirmaNo1 = new Depo();
		FirmaDegisinceDepoDegistirFirmaNo2 = new Depo();
		FirmaDegisinceDepoDegistirFirmaNo3 = new Depo();
		FirmaDegisinceSorumlulukMerkeziDegistir = false;
		FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo0 = new SorumlulukMerkezi();
		FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo1 = new SorumlulukMerkezi();
		FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo2 = new SorumlulukMerkezi();
		FirmaDegisinceSorumlulukMerkeziDegistirFirmaNo3 = new SorumlulukMerkezi();
		FirmaDegisinceProjeKoduDegistir = false;
		FirmaDegisinceProjeKoduDegistirFirmaNo0 = new Proje();
		FirmaDegisinceProjeKoduDegistirFirmaNo1 = new Proje();
		FirmaDegisinceProjeKoduDegistirFirmaNo2 = new Proje();
		FirmaDegisinceProjeKoduDegistirFirmaNo3 = new Proje();
		FirmaDegisinceEvrakSeriDegistir = false;
		FirmaDegisinceEvrakSeriDegistirFirmaNo0 = "";
		FirmaDegisinceEvrakSeriDegistirFirmaNo1 = "";
		FirmaDegisinceEvrakSeriDegistirFirmaNo2 = "";
		FirmaDegisinceEvrakSeriDegistirFirmaNo3 = "";
		FirmaDegisinceFirmaNo0 = 0;
		FirmaDegisinceFirmaNo1 = 1;
		FirmaDegisinceFirmaNo2 = 2;
		FirmaDegisinceFirmaNo3 = 3;
		sepet_siralama_secenegi = enum_evrak_sepet_siralama_secenekleri.EklemeSirasiArtan;
		karsilama_siralama_secenegi = enum_evrak_karsilama_siralama_secenekleri.EklemeSirasiArtan;
		GormeFirmaNo = true;
		GormeSubeNo = true;
		GormeNormalIade = true;
		GormeTicaretTuru = true;
		GormeEvrakSeri = true;
		GormeEvrakSira = true;
		GormeBelgeNo = true;
		GormeBelgeTarihi = true;
		GormeCariKodu = true;
		GormeCariAdres = true;
		GormeKapamaSekli = true;
		GormeKapamaHesapKodu = true;
		GormeOdemePlani = true;
		GormeFiyatListesi = true;
		GormeDovizCinsi = true;
		GormeKur = true;
		GormeProje = true;
		GormeSorumlulukMerkezi = true;
		GormeKaynakDepo = true;
		GormeNakliyeDepo = true;
		GormeHedefDepo = true;
		GormeTarih = true;
		GormeTeslimTarihi = true;
		GormeAciklama1 = true;
		GormeAciklama2 = true;
		GormeAciklama3 = true;
		GormeAciklama4 = true;
		GormeAciklama5 = true;
		GormeAciklama6 = true;
		GormeAciklama7 = true;
		GormeAciklama8 = true;
		GormeAciklama9 = true;
		GormeAciklama10 = true;
		GormeSiparisTeslimTuru = true;
	}
}
