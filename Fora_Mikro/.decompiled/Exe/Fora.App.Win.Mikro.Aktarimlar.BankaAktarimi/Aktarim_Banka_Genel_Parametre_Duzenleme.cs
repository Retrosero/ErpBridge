using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;

public class Aktarim_Banka_Genel_Parametre_Duzenleme : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _kullaniciparametreleri = new Parametreler();

	private bool DegisiklikVar;

	private string AktifKullanici;

	private IContainer components;

	private ListBoxControl lb_kullanicilar;

	private SimpleButton sb_kullanici_ekle;

	private SimpleButton sb_kullanici_sil;

	private SimpleButton sb_ayarlari_kaydet;

	private XtraTabControl tc_parametreler;

	private XtraTabPage xtraTabPage3;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private XtraTabPage xtraTabPage4;

	private TextEdit DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu;

	private LabelControl labelControl29;

	private TextEdit DefaultEvrakSeriTahsildekiSenetOdeme;

	private LabelControl labelControl28;

	private TextEdit DefaultEvrakSeriTahsileSenetCikis;

	private LabelControl labelControl27;

	private TextEdit DefaultEvrakSeriTahsileCekCikis;

	private LabelControl labelControl26;

	private TextEdit DefaultEvrakSeriTahsildekiCekOdeme;

	private LabelControl labelControl25;

	private TextEdit DefaultEvrakSeriNakitYatirma;

	private LabelControl labelControl24;

	private TextEdit DefaultEvrakSeriNakitCekme;

	private LabelControl labelControl23;

	private TextEdit DefaultEvrakSeriGidenHavale;

	private LabelControl labelControl22;

	private LabelControl labelControl21;

	private TextEdit DefaultEvrakSeriGelenHavale;

	private LabelControl labelControl20;

	private XtraTabPage xtraTabPage9;

	private CheckEdit EvrakAciklamaBankaKullan;

	private Label label18;

	private Label label17;

	private Label label16;

	private TextEdit DefaultSpecialAlan1;

	private Label label2;

	private TextEdit DefaultSatirFaturaProjeKodu;

	private Label label9;

	private TextEdit DefaultSatirFaturaSorumlulukMerkeziKodu;

	private Label DefaultSatirFaturaSorumlulukYazi;

	private Label label7;

	private TextEdit DefaultSatirFaturaHesapKodu;

	private Label label6;

	private Label label5;

	private TextEdit DefaultSatirHesapKodu;

	private Label label10;

	private Label label19;

	private TextEdit DefaultEvrakAciklama;

	private Label label21;

	private Label label22;

	private LabelControl labelControl3;

	private LabelControl labelControl2;

	private TextEdit DefaultSatirFaturaEvrakSeri;

	private Label label13;

	private TextEdit DefaultEvrakSorumlulukMerkezi;

	private Label label12;

	private TextEdit DefaultSpecialAlan3;

	private Label label11;

	private TextEdit DefaultSpecialAlan2;

	private Label label3;

	private Label label4;

	private LabelControl labelControl16;

	private CheckEdit AraMasraf;

	private CheckEdit AraPersonel;

	private CheckEdit AraCariPersonel;

	private CheckEdit AraCariHesaplar;

	private Label label31;

	private LabelControl labelControl15;

	private LabelControl labelControl14;

	private LabelControl labelControl13;

	private LabelControl labelControl4;

	private LabelControl labelControl5;

	private LabelControl labelControl6;

	private LabelControl labelControl7;

	private LabelControl labelControl8;

	private LabelControl labelControl9;

	private LabelControl labelControl10;

	private LabelControl labelControl11;

	private LabelControl labelControl12;

	private TextEdit te_eklenecek_parametre_adi;

	private LabelControl labelControl17;

	private TrackBarControl GelismisAramaAgirlikTcVergiNo;

	private Label label1;

	private TextEdit GelismisAramaHaricTutulacakKelimeler;

	private LabelControl labelControl18;

	private TrackBarControl GelismisAramaAgirlikPostaKodu;

	private TrackBarControl GelismisAramaAgirlikIl;

	private TrackBarControl GelismisAramaAgirlikIlce;

	private TrackBarControl GelismisAramaAgirlikMahalle;

	private TrackBarControl GelismisAramaAgirlikTelefon;

	private TrackBarControl GelismisAramaAgirlikAciklama;

	private TrackBarControl GelismisAramaAgirlikUnvan2;

	private TrackBarControl GelismisAramaAgirlikUnvan;

	private TrackBarControl GelismisAramaAgirlikEPosta;

	private TrackBarControl GelismisAramaAgirlikHesapNo;

	private System.Windows.Forms.ComboBox DefaultEvrakTipi;

	private System.Windows.Forms.ComboBox DefaultSatirCinsi;

	private System.Windows.Forms.ComboBox DefaultSatirFaturaOlusturmaDurumu;

	private SpinEdit EvrakAciklamaBankaUzunluk;

	private SpinEdit EvrakAciklamaBankaBaslangic;

	private CheckEdit AraCekSenet;

	private SpinEdit DefaultSatirFaturaMiktari;

	private TextEdit DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu;

	private LabelControl labelControl30;

	private CheckEdit AraBankalar;

	private TextEdit DefaultEvrakSeriBankalarArasiVirmanDekontu;

	private LabelControl labelControl19;

	public Aktarim_Banka_Genel_Parametre_Duzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		DataTable dataSource = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 11, "Gelen Havale" },
				new object[2] { 12, "Giden Havale" },
				new object[2] { 29, "Bankalar Arası Virman Dekontu" },
				new object[2] { 18, "Bankadan Kasaya Nakit Çekme Makbuzu" },
				new object[2] { 19, "Kasadan Bankaya Nakit Yatırma Makbuzu" },
				new object[2] { 14, "Tahsildeki Çek Ödeme Bordrosu" },
				new object[2] { 17, "Tahsile Çek Çıkış Bordrosu" },
				new object[2] { 20, "Tahsile Senet Çıkış Bordrosu" },
				new object[2] { 21, "Tahsildeki Senet Ödeme Bordrosu" },
				new object[2] { 22, "Verilen Firma Çeki Ödeme Bordrosu" },
				new object[2] { 23, "Verilen Firma Senedi Ödeme Bordrosu" }
			}
		};
		DefaultEvrakTipi.DataSource = dataSource;
		DefaultEvrakTipi.ValueMember = "ID";
		DefaultEvrakTipi.DisplayMember = "Isim";
		DataTable dataSource2 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Cari Hesap" },
				new object[2] { 1, "Cari Personel" },
				new object[2] { 2, "Banka" },
				new object[2] { 3, "Hizmet" },
				new object[2] { 4, "Kasa" },
				new object[2] { 5, "Masraf" },
				new object[2] { 6, "Muh. Hesabı" },
				new object[2] { 7, "Personel" },
				new object[2] { 8, "Demirbaş" },
				new object[2] { 9, "İthalat" },
				new object[2] { 10, "Çek" },
				new object[2] { 11, "Senet" }
			}
		};
		DefaultSatirCinsi.DataSource = dataSource2;
		DefaultSatirCinsi.ValueMember = "ID";
		DefaultSatirCinsi.DisplayMember = "Isim";
		DataTable dataSource3 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Oluşturma" },
				new object[2] { 1, "Hizmet" },
				new object[2] { 2, "Ürün" }
			}
		};
		DefaultSatirFaturaOlusturmaDurumu.DataSource = dataSource3;
		DefaultSatirFaturaOlusturmaDurumu.ValueMember = "ID";
		DefaultSatirFaturaOlusturmaDurumu.DisplayMember = "Isim";
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		KullanicilariListele();
	}

	private void KullanicilariListele()
	{
		lb_kullanicilar.Items.Clear();
		foreach (string item in AktarimBankaGenelParametre.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			lb_kullanicilar.Items.Add(item);
		}
		if (lb_kullanicilar.ItemCount == 0)
		{
			sb_kullanici_sil.Enabled = false;
		}
	}

	private void lb_kullanicilar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue == null)
		{
			return;
		}
		sb_kullanici_sil.Enabled = true;
		bool flag = true;
		if (DegisiklikVar && AktifKullanici != lb_kullanicilar.SelectedValue.ToString())
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		if (AktifKullanici == lb_kullanicilar.SelectedValue.ToString())
		{
			flag = false;
		}
		if (flag)
		{
			tc_parametreler.Enabled = true;
			sb_ayarlari_kaydet.Enabled = true;
			AktifKullanici = lb_kullanicilar.SelectedValue.ToString();
			_kullaniciparametreleri = ParametrelerDefault.BankaAktarimGenel(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "BankaAktarim", "", "GenelSablon", AktifKullanici);
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			lb_kullanicilar.SelectedItem = AktifKullanici;
		}
	}

	private void EkranBilgiGuncelle()
	{
		te_kullanici_adi.Text = AktifKullanici;
		AraCariHesaplar.Checked = _kullaniciparametreleri._GetParametre("AraCariHesaplar")._GetBoolean;
		AraCariPersonel.Checked = _kullaniciparametreleri._GetParametre("AraCariPersonel")._GetBoolean;
		AraPersonel.Checked = _kullaniciparametreleri._GetParametre("AraPersonel")._GetBoolean;
		AraMasraf.Checked = _kullaniciparametreleri._GetParametre("AraMasraf")._GetBoolean;
		AraCekSenet.Checked = _kullaniciparametreleri._GetParametre("AraCekSenet")._GetBoolean;
		AraBankalar.Checked = _kullaniciparametreleri._GetParametre("AraBankalar")._GetBoolean;
		EvrakAciklamaBankaKullan.Checked = _kullaniciparametreleri._GetParametre("EvrakAciklamaBankaKullan")._GetBoolean;
		EvrakAciklamaBankaBaslangic.Value = _kullaniciparametreleri._GetParametre("EvrakAciklamaBankaBaslangic")._GetInt;
		EvrakAciklamaBankaUzunluk.Value = _kullaniciparametreleri._GetParametre("EvrakAciklamaBankaUzunluk")._GetInt;
		DefaultSpecialAlan1.Text = _kullaniciparametreleri._GetParametre("DefaultSpecialAlan1")._GetString;
		DefaultSpecialAlan2.Text = _kullaniciparametreleri._GetParametre("DefaultSpecialAlan2")._GetString;
		DefaultSpecialAlan3.Text = _kullaniciparametreleri._GetParametre("DefaultSpecialAlan3")._GetString;
		DefaultEvrakTipi.SelectedValue = _kullaniciparametreleri._GetParametre("DefaultEvrakTipi")._GetInt;
		DefaultEvrakAciklama.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakAciklama")._GetString;
		DefaultEvrakSorumlulukMerkezi.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSorumlulukMerkezi")._GetString;
		DefaultEvrakSeriGelenHavale.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriGelenHavale")._GetString;
		DefaultEvrakSeriGidenHavale.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriGidenHavale")._GetString;
		DefaultEvrakSeriNakitCekme.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriNakitCekme")._GetString;
		DefaultEvrakSeriNakitYatirma.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriNakitYatirma")._GetString;
		DefaultEvrakSeriTahsildekiCekOdeme.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsildekiCekOdeme")._GetString;
		DefaultEvrakSeriTahsileCekCikis.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsileCekCikis")._GetString;
		DefaultEvrakSeriTahsildekiSenetOdeme.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsildekiSenetOdeme")._GetString;
		DefaultEvrakSeriTahsileSenetCikis.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsileSenetCikis")._GetString;
		DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu")._GetString;
		DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu")._GetString;
		DefaultEvrakSeriBankalarArasiVirmanDekontu.Text = _kullaniciparametreleri._GetParametre("DefaultEvrakSeriBankalarArasiVirmanDekontu")._GetString;
		DefaultSatirCinsi.SelectedValue = _kullaniciparametreleri._GetParametre("DefaultSatirCinsi")._GetInt;
		DefaultSatirHesapKodu.Text = _kullaniciparametreleri._GetParametre("DefaultSatirHesapKodu")._GetString;
		DefaultSatirFaturaOlusturmaDurumu.SelectedValue = _kullaniciparametreleri._GetParametre("DefaultSatirFaturaOlusturmaDurumu")._GetInt;
		DefaultSatirFaturaHesapKodu.Text = _kullaniciparametreleri._GetParametre("DefaultSatirFaturaHesapKodu")._GetString;
		DefaultSatirFaturaMiktari.Value = _kullaniciparametreleri._GetParametre("DefaultSatirFaturaMiktari")._GetInt;
		DefaultSatirFaturaEvrakSeri.Text = _kullaniciparametreleri._GetParametre("DefaultSatirFaturaEvrakSeri")._GetString;
		DefaultSatirFaturaSorumlulukMerkeziKodu.Text = _kullaniciparametreleri._GetParametre("DefaultSatirFaturaSorumlulukMerkeziKodu")._GetString;
		DefaultSatirFaturaProjeKodu.Text = _kullaniciparametreleri._GetParametre("DefaultSatirFaturaProjeKodu")._GetString;
		GelismisAramaAgirlikTcVergiNo.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikTcVergiNo")._GetInt;
		GelismisAramaAgirlikHesapNo.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikHesapNo")._GetInt;
		GelismisAramaAgirlikEPosta.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikEPosta")._GetInt;
		GelismisAramaAgirlikUnvan.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikUnvan")._GetInt;
		GelismisAramaAgirlikUnvan2.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikUnvan2")._GetInt;
		GelismisAramaAgirlikAciklama.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikAciklama")._GetInt;
		GelismisAramaAgirlikTelefon.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikTelefon")._GetInt;
		GelismisAramaAgirlikMahalle.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikMahalle")._GetInt;
		GelismisAramaAgirlikIlce.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikIlce")._GetInt;
		GelismisAramaAgirlikIl.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikIl")._GetInt;
		GelismisAramaAgirlikPostaKodu.EditValue = _kullaniciparametreleri._GetParametre("GelismisAramaAgirlikPostaKodu")._GetInt;
		GelismisAramaHaricTutulacakKelimeler.Text = _kullaniciparametreleri._GetParametre("GelismisAramaHaricTutulacakKelimeler")._GetString;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		KullaniciParametreKaydet();
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_kullaniciparametreleri._GetParametre("SablonAdi")._SetString = AktifKullanici;
		_kullaniciparametreleri._GetParametre("AraCariHesaplar")._SetBoolean = AraCariHesaplar.Checked;
		_kullaniciparametreleri._GetParametre("AraCariPersonel")._SetBoolean = AraCariPersonel.Checked;
		_kullaniciparametreleri._GetParametre("AraPersonel")._SetBoolean = AraPersonel.Checked;
		_kullaniciparametreleri._GetParametre("AraMasraf")._SetBoolean = AraMasraf.Checked;
		_kullaniciparametreleri._GetParametre("AraCekSenet")._SetBoolean = AraCekSenet.Checked;
		_kullaniciparametreleri._GetParametre("AraBankalar")._SetBoolean = AraBankalar.Checked;
		_kullaniciparametreleri._GetParametre("EvrakAciklamaBankaKullan")._SetBoolean = EvrakAciklamaBankaKullan.Checked;
		_kullaniciparametreleri._GetParametre("EvrakAciklamaBankaBaslangic")._SetInt = (int)EvrakAciklamaBankaBaslangic.Value;
		_kullaniciparametreleri._GetParametre("EvrakAciklamaBankaUzunluk")._SetInt = (int)EvrakAciklamaBankaUzunluk.Value;
		_kullaniciparametreleri._GetParametre("DefaultSpecialAlan1")._SetString = DefaultSpecialAlan1.Text;
		_kullaniciparametreleri._GetParametre("DefaultSpecialAlan2")._SetString = DefaultSpecialAlan2.Text;
		_kullaniciparametreleri._GetParametre("DefaultSpecialAlan3")._SetString = DefaultSpecialAlan3.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakTipi")._SetInt = (int)DefaultEvrakTipi.SelectedValue;
		_kullaniciparametreleri._GetParametre("DefaultEvrakAciklama")._SetString = DefaultEvrakAciklama.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSorumlulukMerkezi")._SetString = DefaultEvrakSorumlulukMerkezi.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriGelenHavale")._SetString = DefaultEvrakSeriGelenHavale.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriGidenHavale")._SetString = DefaultEvrakSeriGidenHavale.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriNakitCekme")._SetString = DefaultEvrakSeriNakitCekme.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriNakitYatirma")._SetString = DefaultEvrakSeriNakitYatirma.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsildekiCekOdeme")._SetString = DefaultEvrakSeriTahsildekiCekOdeme.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsileCekCikis")._SetString = DefaultEvrakSeriTahsileCekCikis.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsildekiSenetOdeme")._SetString = DefaultEvrakSeriTahsildekiSenetOdeme.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriTahsileSenetCikis")._SetString = DefaultEvrakSeriTahsileSenetCikis.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu")._SetString = DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu")._SetString = DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Text;
		_kullaniciparametreleri._GetParametre("DefaultEvrakSeriBankalarArasiVirmanDekontu")._SetString = DefaultEvrakSeriBankalarArasiVirmanDekontu.Text;
		_kullaniciparametreleri._GetParametre("DefaultSatirCinsi")._SetInt = (int)DefaultSatirCinsi.SelectedValue;
		_kullaniciparametreleri._GetParametre("DefaultSatirHesapKodu")._SetString = DefaultSatirHesapKodu.Text;
		_kullaniciparametreleri._GetParametre("DefaultSatirFaturaOlusturmaDurumu")._SetInt = (int)DefaultSatirFaturaOlusturmaDurumu.SelectedValue;
		_kullaniciparametreleri._GetParametre("DefaultSatirFaturaHesapKodu")._SetString = DefaultSatirFaturaHesapKodu.Text;
		_kullaniciparametreleri._GetParametre("DefaultSatirFaturaMiktari")._SetInt = (int)DefaultSatirFaturaMiktari.Value;
		_kullaniciparametreleri._GetParametre("DefaultSatirFaturaEvrakSeri")._SetString = DefaultSatirFaturaEvrakSeri.Text;
		_kullaniciparametreleri._GetParametre("DefaultSatirFaturaSorumlulukMerkeziKodu")._SetString = DefaultSatirFaturaSorumlulukMerkeziKodu.Text;
		_kullaniciparametreleri._GetParametre("DefaultSatirFaturaProjeKodu")._SetString = DefaultSatirFaturaProjeKodu.Text;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikTcVergiNo")._SetInt = (int)GelismisAramaAgirlikTcVergiNo.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikHesapNo")._SetInt = (int)GelismisAramaAgirlikHesapNo.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikEPosta")._SetInt = (int)GelismisAramaAgirlikEPosta.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikUnvan")._SetInt = (int)GelismisAramaAgirlikUnvan.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikUnvan2")._SetInt = (int)GelismisAramaAgirlikUnvan2.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikAciklama")._SetInt = (int)GelismisAramaAgirlikAciklama.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikTelefon")._SetInt = (int)GelismisAramaAgirlikTelefon.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikMahalle")._SetInt = (int)GelismisAramaAgirlikMahalle.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikIlce")._SetInt = (int)GelismisAramaAgirlikIlce.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikIl")._SetInt = (int)GelismisAramaAgirlikIl.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaAgirlikPostaKodu")._SetInt = (int)GelismisAramaAgirlikPostaKodu.EditValue;
		_kullaniciparametreleri._GetParametre("GelismisAramaHaricTutulacakKelimeler")._SetString = GelismisAramaHaricTutulacakKelimeler.Text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri);
		_kullaniciparametreleri = ParametrelerDefault.BankaAktarimGenel(AktifKullanici);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "BankaAktarim", "", "GenelSablon", AktifKullanici);
		EkranBilgiGuncelle();
		DegisiklikVar = false;
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void sb_kullanici_ekle_Click(object sender, EventArgs e)
	{
		bool flag = true;
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		if (te_eklenecek_parametre_adi.Text == "")
		{
			MessageBox.Show("Eklenecek kaydın adını girmelisiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			flag = false;
		}
		if (flag)
		{
			Parametreler parametreler = ParametrelerDefault.BankaAktarimGenel(te_eklenecek_parametre_adi.Text);
			parametreler._GetParametre("SablonAdi")._SetString = te_eklenecek_parametre_adi.Text;
			ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
			KullanicilariListele();
			lb_kullanicilar.SelectedItem = te_eklenecek_parametre_adi.Text;
		}
	}

	private void sb_kullanici_sil_Click(object sender, EventArgs e)
	{
		if (lb_kullanicilar.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		string text = lb_kullanicilar.SelectedValue.ToString();
		if (flag)
		{
			DialogResult dialogResult = MessageBox.Show(text + " kullanıcısını silmek istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult != DialogResult.Yes)
			{
				_ = 7;
				return;
			}
			AktarimBankaGenelParametre.ParametreSil(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text);
			KullanicilariListele();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.lb_kullanicilar = new DevExpress.XtraEditors.ListBoxControl();
		this.sb_kullanici_ekle = new DevExpress.XtraEditors.SimpleButton();
		this.sb_kullanici_sil = new DevExpress.XtraEditors.SimpleButton();
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.tc_parametreler = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.EvrakAciklamaBankaBaslangic = new DevExpress.XtraEditors.SpinEdit();
		this.EvrakAciklamaBankaUzunluk = new DevExpress.XtraEditors.SpinEdit();
		this.EvrakAciklamaBankaKullan = new DevExpress.XtraEditors.CheckEdit();
		this.label18 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage4 = new DevExpress.XtraTab.XtraTabPage();
		this.DefaultEvrakSeriBankalarArasiVirmanDekontu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl19 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl30 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultSatirFaturaMiktari = new DevExpress.XtraEditors.SpinEdit();
		this.DefaultSatirFaturaOlusturmaDurumu = new System.Windows.Forms.ComboBox();
		this.DefaultSatirCinsi = new System.Windows.Forms.ComboBox();
		this.DefaultEvrakTipi = new System.Windows.Forms.ComboBox();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultSatirFaturaEvrakSeri = new DevExpress.XtraEditors.TextEdit();
		this.label13 = new System.Windows.Forms.Label();
		this.DefaultEvrakSorumlulukMerkezi = new DevExpress.XtraEditors.TextEdit();
		this.label12 = new System.Windows.Forms.Label();
		this.DefaultSpecialAlan3 = new DevExpress.XtraEditors.TextEdit();
		this.label11 = new System.Windows.Forms.Label();
		this.DefaultSpecialAlan2 = new DevExpress.XtraEditors.TextEdit();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.DefaultSpecialAlan1 = new DevExpress.XtraEditors.TextEdit();
		this.label2 = new System.Windows.Forms.Label();
		this.DefaultSatirFaturaProjeKodu = new DevExpress.XtraEditors.TextEdit();
		this.label9 = new System.Windows.Forms.Label();
		this.DefaultSatirFaturaSorumlulukMerkeziKodu = new DevExpress.XtraEditors.TextEdit();
		this.DefaultSatirFaturaSorumlulukYazi = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.DefaultSatirFaturaHesapKodu = new DevExpress.XtraEditors.TextEdit();
		this.label6 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.DefaultSatirHesapKodu = new DevExpress.XtraEditors.TextEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.label19 = new System.Windows.Forms.Label();
		this.DefaultEvrakAciklama = new DevExpress.XtraEditors.TextEdit();
		this.label21 = new System.Windows.Forms.Label();
		this.label22 = new System.Windows.Forms.Label();
		this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl29 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriTahsildekiSenetOdeme = new DevExpress.XtraEditors.TextEdit();
		this.labelControl28 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriTahsileSenetCikis = new DevExpress.XtraEditors.TextEdit();
		this.labelControl27 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriTahsileCekCikis = new DevExpress.XtraEditors.TextEdit();
		this.labelControl26 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriTahsildekiCekOdeme = new DevExpress.XtraEditors.TextEdit();
		this.labelControl25 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriNakitYatirma = new DevExpress.XtraEditors.TextEdit();
		this.labelControl24 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriNakitCekme = new DevExpress.XtraEditors.TextEdit();
		this.labelControl23 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriGidenHavale = new DevExpress.XtraEditors.TextEdit();
		this.labelControl22 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl21 = new DevExpress.XtraEditors.LabelControl();
		this.DefaultEvrakSeriGelenHavale = new DevExpress.XtraEditors.TextEdit();
		this.labelControl20 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage9 = new DevExpress.XtraTab.XtraTabPage();
		this.AraBankalar = new DevExpress.XtraEditors.CheckEdit();
		this.AraCekSenet = new DevExpress.XtraEditors.CheckEdit();
		this.label1 = new System.Windows.Forms.Label();
		this.GelismisAramaHaricTutulacakKelimeler = new DevExpress.XtraEditors.TextEdit();
		this.labelControl18 = new DevExpress.XtraEditors.LabelControl();
		this.GelismisAramaAgirlikPostaKodu = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikIl = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikIlce = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikMahalle = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikTelefon = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikAciklama = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikUnvan2 = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikUnvan = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikEPosta = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikHesapNo = new DevExpress.XtraEditors.TrackBarControl();
		this.GelismisAramaAgirlikTcVergiNo = new DevExpress.XtraEditors.TrackBarControl();
		this.labelControl16 = new DevExpress.XtraEditors.LabelControl();
		this.AraMasraf = new DevExpress.XtraEditors.CheckEdit();
		this.AraPersonel = new DevExpress.XtraEditors.CheckEdit();
		this.AraCariPersonel = new DevExpress.XtraEditors.CheckEdit();
		this.AraCariHesaplar = new DevExpress.XtraEditors.CheckEdit();
		this.label31 = new System.Windows.Forms.Label();
		this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl14 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl13 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.te_eklenecek_parametre_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).BeginInit();
		this.tc_parametreler.SuspendLayout();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.EvrakAciklamaBankaBaslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakAciklamaBankaUzunluk.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakAciklamaBankaKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		this.xtraTabPage4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriBankalarArasiVirmanDekontu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaMiktari.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaEvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSorumlulukMerkezi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSpecialAlan3.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSpecialAlan2.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSpecialAlan1.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaProjeKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaSorumlulukMerkeziKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaHesapKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirHesapKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakAciklama.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsildekiSenetOdeme.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsileSenetCikis.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsileCekCikis.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsildekiCekOdeme.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriNakitYatirma.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriNakitCekme.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriGidenHavale.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriGelenHavale.Properties).BeginInit();
		this.xtraTabPage9.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.AraBankalar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AraCekSenet.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaHaricTutulacakKelimeler.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikPostaKodu).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikPostaKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIl).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIl.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIlce).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIlce.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikMahalle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikMahalle.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTelefon).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTelefon.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikAciklama).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikAciklama.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan2.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikEPosta).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikEPosta.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikHesapNo).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikHesapNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTcVergiNo).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTcVergiNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AraMasraf.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AraPersonel.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AraCariPersonel.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AraCariHesaplar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_parametre_adi.Properties).BeginInit();
		base.SuspendLayout();
		this.lb_kullanicilar.Location = new System.Drawing.Point(12, 35);
		this.lb_kullanicilar.Name = "lb_kullanicilar";
		this.lb_kullanicilar.Size = new System.Drawing.Size(153, 221);
		this.lb_kullanicilar.TabIndex = 5;
		this.lb_kullanicilar.SelectedValueChanged += new System.EventHandler(lb_kullanicilar_SelectedValueChanged);
		this.sb_kullanici_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_ekle.Appearance.Options.UseFont = true;
		this.sb_kullanici_ekle.Location = new System.Drawing.Point(13, 378);
		this.sb_kullanici_ekle.Name = "sb_kullanici_ekle";
		this.sb_kullanici_ekle.Size = new System.Drawing.Size(153, 23);
		this.sb_kullanici_ekle.TabIndex = 6;
		this.sb_kullanici_ekle.Text = "Genel Parametre Ekle";
		this.sb_kullanici_ekle.Click += new System.EventHandler(sb_kullanici_ekle_Click);
		this.sb_kullanici_sil.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_sil.Appearance.Options.UseFont = true;
		this.sb_kullanici_sil.Enabled = false;
		this.sb_kullanici_sil.Location = new System.Drawing.Point(13, 262);
		this.sb_kullanici_sil.Name = "sb_kullanici_sil";
		this.sb_kullanici_sil.Size = new System.Drawing.Size(153, 23);
		this.sb_kullanici_sil.TabIndex = 7;
		this.sb_kullanici_sil.Text = "Genel Parametre Sil";
		this.sb_kullanici_sil.Click += new System.EventHandler(sb_kullanici_sil_Click);
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.Enabled = false;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(785, 533);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(132, 23);
		this.sb_ayarlari_kaydet.TabIndex = 8;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.tc_parametreler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametreler.Location = new System.Drawing.Point(171, 35);
		this.tc_parametreler.Name = "tc_parametreler";
		this.tc_parametreler.SelectedTabPage = this.xtraTabPage3;
		this.tc_parametreler.Size = new System.Drawing.Size(746, 480);
		this.tc_parametreler.TabIndex = 0;
		this.tc_parametreler.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[3] { this.xtraTabPage3, this.xtraTabPage4, this.xtraTabPage9 });
		this.xtraTabPage3.Controls.Add(this.EvrakAciklamaBankaBaslangic);
		this.xtraTabPage3.Controls.Add(this.EvrakAciklamaBankaUzunluk);
		this.xtraTabPage3.Controls.Add(this.EvrakAciklamaBankaKullan);
		this.xtraTabPage3.Controls.Add(this.label18);
		this.xtraTabPage3.Controls.Add(this.label17);
		this.xtraTabPage3.Controls.Add(this.label16);
		this.xtraTabPage3.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage3.Controls.Add(this.labelControl1);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(740, 452);
		this.xtraTabPage3.Text = "Genel";
		this.EvrakAciklamaBankaBaslangic.EditValue = new decimal(new int[4]);
		this.EvrakAciklamaBankaBaslangic.Location = new System.Drawing.Point(220, 75);
		this.EvrakAciklamaBankaBaslangic.Name = "EvrakAciklamaBankaBaslangic";
		this.EvrakAciklamaBankaBaslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.EvrakAciklamaBankaBaslangic.Properties.IsFloatValue = false;
		this.EvrakAciklamaBankaBaslangic.Properties.Mask.EditMask = "N00";
		this.EvrakAciklamaBankaBaslangic.Properties.MaxValue = new decimal(new int[4] { 40, 0, 0, 0 });
		this.EvrakAciklamaBankaBaslangic.Size = new System.Drawing.Size(64, 20);
		this.EvrakAciklamaBankaBaslangic.TabIndex = 70;
		this.EvrakAciklamaBankaBaslangic.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.EvrakAciklamaBankaBaslangic.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.EvrakAciklamaBankaUzunluk.EditValue = new decimal(new int[4]);
		this.EvrakAciklamaBankaUzunluk.Location = new System.Drawing.Point(290, 75);
		this.EvrakAciklamaBankaUzunluk.Name = "EvrakAciklamaBankaUzunluk";
		this.EvrakAciklamaBankaUzunluk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.EvrakAciklamaBankaUzunluk.Properties.IsFloatValue = false;
		this.EvrakAciklamaBankaUzunluk.Properties.Mask.EditMask = "N00";
		this.EvrakAciklamaBankaUzunluk.Properties.MaxValue = new decimal(new int[4] { 40, 0, 0, 0 });
		this.EvrakAciklamaBankaUzunluk.Size = new System.Drawing.Size(54, 20);
		this.EvrakAciklamaBankaUzunluk.TabIndex = 69;
		this.EvrakAciklamaBankaUzunluk.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.EvrakAciklamaBankaUzunluk.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.EvrakAciklamaBankaKullan.Location = new System.Drawing.Point(351, 75);
		this.EvrakAciklamaBankaKullan.Name = "EvrakAciklamaBankaKullan";
		this.EvrakAciklamaBankaKullan.Properties.Caption = "Kullan";
		this.EvrakAciklamaBankaKullan.Size = new System.Drawing.Size(60, 19);
		this.EvrakAciklamaBankaKullan.TabIndex = 65;
		this.EvrakAciklamaBankaKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.EvrakAciklamaBankaKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label18.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label18.Location = new System.Drawing.Point(287, 52);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(54, 19);
		this.label18.TabIndex = 68;
		this.label18.Text = "Uzunluk";
		this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label17.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label17.Location = new System.Drawing.Point(217, 52);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(64, 19);
		this.label17.TabIndex = 67;
		this.label17.Text = "Başlangıç";
		this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label16.Location = new System.Drawing.Point(11, 74);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(202, 19);
		this.label16.TabIndex = 66;
		this.label16.Text = "Banka açıklamasını açıklamaya kopyala :";
		this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(219, 15);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(106, 20);
		this.te_kullanici_adi.TabIndex = 2;
		this.te_kullanici_adi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(96, 18);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 0;
		this.labelControl1.Text = "Genel parametre adı :";
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriBankalarArasiVirmanDekontu);
		this.xtraTabPage4.Controls.Add(this.labelControl19);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu);
		this.xtraTabPage4.Controls.Add(this.labelControl30);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirFaturaMiktari);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirFaturaOlusturmaDurumu);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirCinsi);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakTipi);
		this.xtraTabPage4.Controls.Add(this.labelControl3);
		this.xtraTabPage4.Controls.Add(this.labelControl2);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirFaturaEvrakSeri);
		this.xtraTabPage4.Controls.Add(this.label13);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSorumlulukMerkezi);
		this.xtraTabPage4.Controls.Add(this.label12);
		this.xtraTabPage4.Controls.Add(this.DefaultSpecialAlan3);
		this.xtraTabPage4.Controls.Add(this.label11);
		this.xtraTabPage4.Controls.Add(this.DefaultSpecialAlan2);
		this.xtraTabPage4.Controls.Add(this.label3);
		this.xtraTabPage4.Controls.Add(this.label4);
		this.xtraTabPage4.Controls.Add(this.DefaultSpecialAlan1);
		this.xtraTabPage4.Controls.Add(this.label2);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirFaturaProjeKodu);
		this.xtraTabPage4.Controls.Add(this.label9);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirFaturaSorumlulukMerkeziKodu);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirFaturaSorumlulukYazi);
		this.xtraTabPage4.Controls.Add(this.label7);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirFaturaHesapKodu);
		this.xtraTabPage4.Controls.Add(this.label6);
		this.xtraTabPage4.Controls.Add(this.label5);
		this.xtraTabPage4.Controls.Add(this.DefaultSatirHesapKodu);
		this.xtraTabPage4.Controls.Add(this.label10);
		this.xtraTabPage4.Controls.Add(this.label19);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakAciklama);
		this.xtraTabPage4.Controls.Add(this.label21);
		this.xtraTabPage4.Controls.Add(this.label22);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu);
		this.xtraTabPage4.Controls.Add(this.labelControl29);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriTahsildekiSenetOdeme);
		this.xtraTabPage4.Controls.Add(this.labelControl28);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriTahsileSenetCikis);
		this.xtraTabPage4.Controls.Add(this.labelControl27);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriTahsileCekCikis);
		this.xtraTabPage4.Controls.Add(this.labelControl26);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriTahsildekiCekOdeme);
		this.xtraTabPage4.Controls.Add(this.labelControl25);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriNakitYatirma);
		this.xtraTabPage4.Controls.Add(this.labelControl24);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriNakitCekme);
		this.xtraTabPage4.Controls.Add(this.labelControl23);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriGidenHavale);
		this.xtraTabPage4.Controls.Add(this.labelControl22);
		this.xtraTabPage4.Controls.Add(this.labelControl21);
		this.xtraTabPage4.Controls.Add(this.DefaultEvrakSeriGelenHavale);
		this.xtraTabPage4.Controls.Add(this.labelControl20);
		this.xtraTabPage4.Name = "xtraTabPage4";
		this.xtraTabPage4.Size = new System.Drawing.Size(740, 452);
		this.xtraTabPage4.Text = "Varsayılan değerler";
		this.DefaultEvrakSeriBankalarArasiVirmanDekontu.Location = new System.Drawing.Point(663, 300);
		this.DefaultEvrakSeriBankalarArasiVirmanDekontu.Name = "DefaultEvrakSeriBankalarArasiVirmanDekontu";
		this.DefaultEvrakSeriBankalarArasiVirmanDekontu.Properties.MaxLength = 4;
		this.DefaultEvrakSeriBankalarArasiVirmanDekontu.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriBankalarArasiVirmanDekontu.TabIndex = 97;
		this.labelControl19.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl19.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl19.Location = new System.Drawing.Point(444, 303);
		this.labelControl19.Name = "labelControl19";
		this.labelControl19.Size = new System.Drawing.Size(213, 13);
		this.labelControl19.TabIndex = 96;
		this.labelControl19.Text = "Bankalar arası virman dekontu :";
		this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Location = new System.Drawing.Point(663, 274);
		this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Name = "DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu";
		this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Properties.MaxLength = 4;
		this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.TabIndex = 95;
		this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl30.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl30.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl30.Location = new System.Drawing.Point(444, 277);
		this.labelControl30.Name = "labelControl30";
		this.labelControl30.Size = new System.Drawing.Size(213, 13);
		this.labelControl30.TabIndex = 94;
		this.labelControl30.Text = "Verilen Firma senedi ödeme bordrosu :";
		this.DefaultSatirFaturaMiktari.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DefaultSatirFaturaMiktari.Location = new System.Drawing.Point(188, 317);
		this.DefaultSatirFaturaMiktari.Name = "DefaultSatirFaturaMiktari";
		this.DefaultSatirFaturaMiktari.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.DefaultSatirFaturaMiktari.Properties.IsFloatValue = false;
		this.DefaultSatirFaturaMiktari.Properties.Mask.EditMask = "N00";
		this.DefaultSatirFaturaMiktari.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.DefaultSatirFaturaMiktari.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DefaultSatirFaturaMiktari.Size = new System.Drawing.Size(54, 20);
		this.DefaultSatirFaturaMiktari.TabIndex = 93;
		this.DefaultSatirFaturaMiktari.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DefaultSatirFaturaMiktari.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DefaultSatirFaturaOlusturmaDurumu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DefaultSatirFaturaOlusturmaDurumu.FormattingEnabled = true;
		this.DefaultSatirFaturaOlusturmaDurumu.Location = new System.Drawing.Point(188, 264);
		this.DefaultSatirFaturaOlusturmaDurumu.Name = "DefaultSatirFaturaOlusturmaDurumu";
		this.DefaultSatirFaturaOlusturmaDurumu.Size = new System.Drawing.Size(205, 21);
		this.DefaultSatirFaturaOlusturmaDurumu.TabIndex = 91;
		this.DefaultSatirFaturaOlusturmaDurumu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DefaultSatirFaturaOlusturmaDurumu.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DefaultSatirCinsi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DefaultSatirCinsi.FormattingEnabled = true;
		this.DefaultSatirCinsi.Location = new System.Drawing.Point(186, 88);
		this.DefaultSatirCinsi.Name = "DefaultSatirCinsi";
		this.DefaultSatirCinsi.Size = new System.Drawing.Size(204, 21);
		this.DefaultSatirCinsi.TabIndex = 90;
		this.DefaultSatirCinsi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DefaultSatirCinsi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DefaultEvrakTipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DefaultEvrakTipi.FormattingEnabled = true;
		this.DefaultEvrakTipi.Location = new System.Drawing.Point(186, 36);
		this.DefaultEvrakTipi.Name = "DefaultEvrakTipi";
		this.DefaultEvrakTipi.Size = new System.Drawing.Size(205, 21);
		this.DefaultEvrakTipi.TabIndex = 89;
		this.DefaultEvrakTipi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DefaultEvrakTipi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(520, 324);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(206, 19);
		this.labelControl3.TabIndex = 88;
		this.labelControl3.Text = "Özel alanlar";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(188, 239);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(207, 19);
		this.labelControl2.TabIndex = 87;
		this.labelControl2.Text = "Fatura ayarları";
		this.DefaultSatirFaturaEvrakSeri.Location = new System.Drawing.Point(188, 343);
		this.DefaultSatirFaturaEvrakSeri.Name = "DefaultSatirFaturaEvrakSeri";
		this.DefaultSatirFaturaEvrakSeri.Properties.MaxLength = 4;
		this.DefaultSatirFaturaEvrakSeri.Size = new System.Drawing.Size(54, 20);
		this.DefaultSatirFaturaEvrakSeri.TabIndex = 85;
		this.DefaultSatirFaturaEvrakSeri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label13.Location = new System.Drawing.Point(17, 343);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(164, 19);
		this.label13.TabIndex = 86;
		this.label13.Text = "Fatura evrak seri :";
		this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultEvrakSorumlulukMerkezi.Location = new System.Drawing.Point(186, 141);
		this.DefaultEvrakSorumlulukMerkezi.Name = "DefaultEvrakSorumlulukMerkezi";
		this.DefaultEvrakSorumlulukMerkezi.Size = new System.Drawing.Size(206, 20);
		this.DefaultEvrakSorumlulukMerkezi.TabIndex = 83;
		this.DefaultEvrakSorumlulukMerkezi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label12.Location = new System.Drawing.Point(13, 141);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(164, 19);
		this.label12.TabIndex = 84;
		this.label12.Text = "Evrak Srm. merkezi kodu :";
		this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultSpecialAlan3.Location = new System.Drawing.Point(545, 405);
		this.DefaultSpecialAlan3.Name = "DefaultSpecialAlan3";
		this.DefaultSpecialAlan3.Properties.MaxLength = 4;
		this.DefaultSpecialAlan3.Size = new System.Drawing.Size(46, 20);
		this.DefaultSpecialAlan3.TabIndex = 81;
		this.DefaultSpecialAlan3.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label11.Location = new System.Drawing.Point(462, 405);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(76, 19);
		this.label11.TabIndex = 82;
		this.label11.Text = "Özel alan 3 :";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultSpecialAlan2.Location = new System.Drawing.Point(545, 379);
		this.DefaultSpecialAlan2.Name = "DefaultSpecialAlan2";
		this.DefaultSpecialAlan2.Properties.MaxLength = 4;
		this.DefaultSpecialAlan2.Size = new System.Drawing.Size(46, 20);
		this.DefaultSpecialAlan2.TabIndex = 79;
		this.DefaultSpecialAlan2.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label3.Location = new System.Drawing.Point(462, 379);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(76, 19);
		this.label3.TabIndex = 80;
		this.label3.Text = "Özel alan 2 :";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label4.Location = new System.Drawing.Point(597, 360);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(129, 57);
		this.label4.TabIndex = 78;
		this.label4.Text = "Özel alanlar maksimum 4 karakter olabilir. Mikro'ya aktarım esnasında special kolonlarına yazılır.";
		this.DefaultSpecialAlan1.Location = new System.Drawing.Point(544, 353);
		this.DefaultSpecialAlan1.Name = "DefaultSpecialAlan1";
		this.DefaultSpecialAlan1.Properties.MaxLength = 4;
		this.DefaultSpecialAlan1.Size = new System.Drawing.Size(46, 20);
		this.DefaultSpecialAlan1.TabIndex = 76;
		this.DefaultSpecialAlan1.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label2.Location = new System.Drawing.Point(461, 353);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(76, 19);
		this.label2.TabIndex = 77;
		this.label2.Text = "Özel alan 1 :";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultSatirFaturaProjeKodu.Location = new System.Drawing.Point(187, 193);
		this.DefaultSatirFaturaProjeKodu.Name = "DefaultSatirFaturaProjeKodu";
		this.DefaultSatirFaturaProjeKodu.Size = new System.Drawing.Size(206, 20);
		this.DefaultSatirFaturaProjeKodu.TabIndex = 72;
		this.DefaultSatirFaturaProjeKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label9.Location = new System.Drawing.Point(16, 193);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(164, 19);
		this.label9.TabIndex = 75;
		this.label9.Text = "Satır proje kodu :";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultSatirFaturaSorumlulukMerkeziKodu.Location = new System.Drawing.Point(187, 167);
		this.DefaultSatirFaturaSorumlulukMerkeziKodu.Name = "DefaultSatirFaturaSorumlulukMerkeziKodu";
		this.DefaultSatirFaturaSorumlulukMerkeziKodu.Size = new System.Drawing.Size(206, 20);
		this.DefaultSatirFaturaSorumlulukMerkeziKodu.TabIndex = 71;
		this.DefaultSatirFaturaSorumlulukMerkeziKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DefaultSatirFaturaSorumlulukYazi.Location = new System.Drawing.Point(16, 167);
		this.DefaultSatirFaturaSorumlulukYazi.Name = "DefaultSatirFaturaSorumlulukYazi";
		this.DefaultSatirFaturaSorumlulukYazi.Size = new System.Drawing.Size(164, 19);
		this.DefaultSatirFaturaSorumlulukYazi.TabIndex = 74;
		this.DefaultSatirFaturaSorumlulukYazi.Text = "Satır Srm. merkezi kodu :";
		this.DefaultSatirFaturaSorumlulukYazi.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label7.Location = new System.Drawing.Point(20, 318);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(161, 19);
		this.label7.TabIndex = 73;
		this.label7.Text = "Fatura miktarı :";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultSatirFaturaHesapKodu.Location = new System.Drawing.Point(188, 290);
		this.DefaultSatirFaturaHesapKodu.Name = "DefaultSatirFaturaHesapKodu";
		this.DefaultSatirFaturaHesapKodu.Size = new System.Drawing.Size(206, 20);
		this.DefaultSatirFaturaHesapKodu.TabIndex = 68;
		this.DefaultSatirFaturaHesapKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label6.Location = new System.Drawing.Point(20, 292);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(161, 19);
		this.label6.TabIndex = 69;
		this.label6.Text = "Fatura hesap kodu :";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label5.Location = new System.Drawing.Point(17, 265);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(164, 19);
		this.label5.TabIndex = 66;
		this.label5.Text = "Fatura :";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultSatirHesapKodu.Location = new System.Drawing.Point(186, 115);
		this.DefaultSatirHesapKodu.Name = "DefaultSatirHesapKodu";
		this.DefaultSatirHesapKodu.Size = new System.Drawing.Size(206, 20);
		this.DefaultSatirHesapKodu.TabIndex = 65;
		this.DefaultSatirHesapKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label10.Location = new System.Drawing.Point(14, 117);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(164, 19);
		this.label10.TabIndex = 64;
		this.label10.Text = "Hesap kodu :";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label19.Location = new System.Drawing.Point(14, 90);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(164, 19);
		this.label19.TabIndex = 60;
		this.label19.Text = "Cinsi :";
		this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultEvrakAciklama.Location = new System.Drawing.Point(186, 63);
		this.DefaultEvrakAciklama.Name = "DefaultEvrakAciklama";
		this.DefaultEvrakAciklama.Properties.MaxLength = 40;
		this.DefaultEvrakAciklama.Size = new System.Drawing.Size(205, 20);
		this.DefaultEvrakAciklama.TabIndex = 58;
		this.DefaultEvrakAciklama.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label21.Location = new System.Drawing.Point(17, 65);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(161, 19);
		this.label21.TabIndex = 57;
		this.label21.Text = "Açıklama (Mak. 40 karakter) :";
		this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label22.Location = new System.Drawing.Point(77, 36);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(101, 19);
		this.label22.TabIndex = 54;
		this.label22.Text = "Evrak tipi :";
		this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Location = new System.Drawing.Point(663, 248);
		this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Name = "DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu";
		this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Properties.MaxLength = 4;
		this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.TabIndex = 43;
		this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl29.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl29.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl29.Location = new System.Drawing.Point(444, 251);
		this.labelControl29.Name = "labelControl29";
		this.labelControl29.Size = new System.Drawing.Size(213, 13);
		this.labelControl29.TabIndex = 42;
		this.labelControl29.Text = "Verilen Firma çeki ödeme bordrosu :";
		this.DefaultEvrakSeriTahsildekiSenetOdeme.Location = new System.Drawing.Point(663, 222);
		this.DefaultEvrakSeriTahsildekiSenetOdeme.Name = "DefaultEvrakSeriTahsildekiSenetOdeme";
		this.DefaultEvrakSeriTahsildekiSenetOdeme.Properties.MaxLength = 4;
		this.DefaultEvrakSeriTahsildekiSenetOdeme.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriTahsildekiSenetOdeme.TabIndex = 41;
		this.DefaultEvrakSeriTahsildekiSenetOdeme.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl28.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl28.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl28.Location = new System.Drawing.Point(444, 225);
		this.labelControl28.Name = "labelControl28";
		this.labelControl28.Size = new System.Drawing.Size(213, 13);
		this.labelControl28.TabIndex = 40;
		this.labelControl28.Text = "Tahsildeki senet ödeme bordrosu :";
		this.DefaultEvrakSeriTahsileSenetCikis.Location = new System.Drawing.Point(663, 196);
		this.DefaultEvrakSeriTahsileSenetCikis.Name = "DefaultEvrakSeriTahsileSenetCikis";
		this.DefaultEvrakSeriTahsileSenetCikis.Properties.MaxLength = 4;
		this.DefaultEvrakSeriTahsileSenetCikis.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriTahsileSenetCikis.TabIndex = 39;
		this.DefaultEvrakSeriTahsileSenetCikis.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl27.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl27.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl27.Location = new System.Drawing.Point(444, 199);
		this.labelControl27.Name = "labelControl27";
		this.labelControl27.Size = new System.Drawing.Size(213, 13);
		this.labelControl27.TabIndex = 38;
		this.labelControl27.Text = "Tahsile senet çıkış bordrosu :";
		this.DefaultEvrakSeriTahsileCekCikis.Location = new System.Drawing.Point(663, 170);
		this.DefaultEvrakSeriTahsileCekCikis.Name = "DefaultEvrakSeriTahsileCekCikis";
		this.DefaultEvrakSeriTahsileCekCikis.Properties.MaxLength = 4;
		this.DefaultEvrakSeriTahsileCekCikis.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriTahsileCekCikis.TabIndex = 37;
		this.DefaultEvrakSeriTahsileCekCikis.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl26.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl26.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl26.Location = new System.Drawing.Point(444, 173);
		this.labelControl26.Name = "labelControl26";
		this.labelControl26.Size = new System.Drawing.Size(213, 13);
		this.labelControl26.TabIndex = 36;
		this.labelControl26.Text = "Tahsile çek çıkış bordrosu :";
		this.DefaultEvrakSeriTahsildekiCekOdeme.Location = new System.Drawing.Point(663, 144);
		this.DefaultEvrakSeriTahsildekiCekOdeme.Name = "DefaultEvrakSeriTahsildekiCekOdeme";
		this.DefaultEvrakSeriTahsildekiCekOdeme.Properties.MaxLength = 4;
		this.DefaultEvrakSeriTahsildekiCekOdeme.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriTahsildekiCekOdeme.TabIndex = 35;
		this.DefaultEvrakSeriTahsildekiCekOdeme.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl25.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl25.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl25.Location = new System.Drawing.Point(444, 147);
		this.labelControl25.Name = "labelControl25";
		this.labelControl25.Size = new System.Drawing.Size(213, 13);
		this.labelControl25.TabIndex = 34;
		this.labelControl25.Text = "Tahsildeki çek ödeme bordrosu :";
		this.DefaultEvrakSeriNakitYatirma.Location = new System.Drawing.Point(663, 118);
		this.DefaultEvrakSeriNakitYatirma.Name = "DefaultEvrakSeriNakitYatirma";
		this.DefaultEvrakSeriNakitYatirma.Properties.MaxLength = 4;
		this.DefaultEvrakSeriNakitYatirma.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriNakitYatirma.TabIndex = 33;
		this.DefaultEvrakSeriNakitYatirma.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl24.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl24.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl24.Location = new System.Drawing.Point(444, 121);
		this.labelControl24.Name = "labelControl24";
		this.labelControl24.Size = new System.Drawing.Size(213, 13);
		this.labelControl24.TabIndex = 32;
		this.labelControl24.Text = "Kasadan bankaya nakit yatırma makbuzu :";
		this.DefaultEvrakSeriNakitCekme.Location = new System.Drawing.Point(663, 92);
		this.DefaultEvrakSeriNakitCekme.Name = "DefaultEvrakSeriNakitCekme";
		this.DefaultEvrakSeriNakitCekme.Properties.MaxLength = 4;
		this.DefaultEvrakSeriNakitCekme.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriNakitCekme.TabIndex = 31;
		this.DefaultEvrakSeriNakitCekme.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl23.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl23.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl23.Location = new System.Drawing.Point(444, 95);
		this.labelControl23.Name = "labelControl23";
		this.labelControl23.Size = new System.Drawing.Size(213, 13);
		this.labelControl23.TabIndex = 30;
		this.labelControl23.Text = "Bankadan kasaya nakit çekme makbuzu :";
		this.DefaultEvrakSeriGidenHavale.Location = new System.Drawing.Point(663, 66);
		this.DefaultEvrakSeriGidenHavale.Name = "DefaultEvrakSeriGidenHavale";
		this.DefaultEvrakSeriGidenHavale.Properties.MaxLength = 4;
		this.DefaultEvrakSeriGidenHavale.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriGidenHavale.TabIndex = 29;
		this.DefaultEvrakSeriGidenHavale.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl22.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl22.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl22.Location = new System.Drawing.Point(444, 69);
		this.labelControl22.Name = "labelControl22";
		this.labelControl22.Size = new System.Drawing.Size(213, 13);
		this.labelControl22.TabIndex = 28;
		this.labelControl22.Text = "Giden havale :";
		this.labelControl21.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl21.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl21.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl21.Location = new System.Drawing.Point(584, 15);
		this.labelControl21.Name = "labelControl21";
		this.labelControl21.Size = new System.Drawing.Size(126, 19);
		this.labelControl21.TabIndex = 27;
		this.labelControl21.Text = "Evrak serileri";
		this.DefaultEvrakSeriGelenHavale.Location = new System.Drawing.Point(663, 40);
		this.DefaultEvrakSeriGelenHavale.Name = "DefaultEvrakSeriGelenHavale";
		this.DefaultEvrakSeriGelenHavale.Properties.MaxLength = 4;
		this.DefaultEvrakSeriGelenHavale.Size = new System.Drawing.Size(47, 20);
		this.DefaultEvrakSeriGelenHavale.TabIndex = 26;
		this.DefaultEvrakSeriGelenHavale.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl20.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl20.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl20.Location = new System.Drawing.Point(444, 43);
		this.labelControl20.Name = "labelControl20";
		this.labelControl20.Size = new System.Drawing.Size(213, 13);
		this.labelControl20.TabIndex = 25;
		this.labelControl20.Text = "Gelen havale :";
		this.xtraTabPage9.Controls.Add(this.AraBankalar);
		this.xtraTabPage9.Controls.Add(this.AraCekSenet);
		this.xtraTabPage9.Controls.Add(this.label1);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaHaricTutulacakKelimeler);
		this.xtraTabPage9.Controls.Add(this.labelControl18);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikPostaKodu);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikIl);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikIlce);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikMahalle);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikTelefon);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikAciklama);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikUnvan2);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikUnvan);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikEPosta);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikHesapNo);
		this.xtraTabPage9.Controls.Add(this.GelismisAramaAgirlikTcVergiNo);
		this.xtraTabPage9.Controls.Add(this.labelControl16);
		this.xtraTabPage9.Controls.Add(this.AraMasraf);
		this.xtraTabPage9.Controls.Add(this.AraPersonel);
		this.xtraTabPage9.Controls.Add(this.AraCariPersonel);
		this.xtraTabPage9.Controls.Add(this.AraCariHesaplar);
		this.xtraTabPage9.Controls.Add(this.label31);
		this.xtraTabPage9.Controls.Add(this.labelControl15);
		this.xtraTabPage9.Controls.Add(this.labelControl14);
		this.xtraTabPage9.Controls.Add(this.labelControl13);
		this.xtraTabPage9.Controls.Add(this.labelControl4);
		this.xtraTabPage9.Controls.Add(this.labelControl5);
		this.xtraTabPage9.Controls.Add(this.labelControl6);
		this.xtraTabPage9.Controls.Add(this.labelControl7);
		this.xtraTabPage9.Controls.Add(this.labelControl8);
		this.xtraTabPage9.Controls.Add(this.labelControl9);
		this.xtraTabPage9.Controls.Add(this.labelControl10);
		this.xtraTabPage9.Controls.Add(this.labelControl11);
		this.xtraTabPage9.Controls.Add(this.labelControl12);
		this.xtraTabPage9.Name = "xtraTabPage9";
		this.xtraTabPage9.Size = new System.Drawing.Size(740, 452);
		this.xtraTabPage9.Text = "Hesap arama";
		this.AraBankalar.Location = new System.Drawing.Point(39, 176);
		this.AraBankalar.Name = "AraBankalar";
		this.AraBankalar.Properties.Caption = "Bankalar";
		this.AraBankalar.Size = new System.Drawing.Size(108, 19);
		this.AraBankalar.TabIndex = 124;
		this.AraCekSenet.Location = new System.Drawing.Point(39, 151);
		this.AraCekSenet.Name = "AraCekSenet";
		this.AraCekSenet.Properties.Caption = "Çek ve Senetler";
		this.AraCekSenet.Size = new System.Drawing.Size(108, 19);
		this.AraCekSenet.TabIndex = 123;
		this.AraCekSenet.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AraCekSenet.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label1.Location = new System.Drawing.Point(38, 278);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(353, 47);
		this.label1.TabIndex = 122;
		this.label1.Text = "Bankadan gelen bilgilerin içinde hesap kodunu bulmayı etkilememesini istediğiniz kelimeleri bu alana yazabilirsiniz. Birden fazla kelimeyi virgül ile ayırınız.";
		this.GelismisAramaHaricTutulacakKelimeler.Location = new System.Drawing.Point(41, 253);
		this.GelismisAramaHaricTutulacakKelimeler.Name = "GelismisAramaHaricTutulacakKelimeler";
		this.GelismisAramaHaricTutulacakKelimeler.Size = new System.Drawing.Size(350, 20);
		this.GelismisAramaHaricTutulacakKelimeler.TabIndex = 121;
		this.GelismisAramaHaricTutulacakKelimeler.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl18.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl18.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl18.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl18.Location = new System.Drawing.Point(41, 226);
		this.labelControl18.Name = "labelControl18";
		this.labelControl18.Size = new System.Drawing.Size(225, 19);
		this.labelControl18.TabIndex = 120;
		this.labelControl18.Text = "Dikkate alınmayacak kelimeler";
		this.GelismisAramaAgirlikPostaKodu.EditValue = 5;
		this.GelismisAramaAgirlikPostaKodu.Location = new System.Drawing.Point(528, 310);
		this.GelismisAramaAgirlikPostaKodu.Name = "GelismisAramaAgirlikPostaKodu";
		this.GelismisAramaAgirlikPostaKodu.Properties.AutoSize = false;
		this.GelismisAramaAgirlikPostaKodu.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikPostaKodu.TabIndex = 119;
		this.GelismisAramaAgirlikPostaKodu.Value = 5;
		this.GelismisAramaAgirlikPostaKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikPostaKodu.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikIl.EditValue = 5;
		this.GelismisAramaAgirlikIl.Location = new System.Drawing.Point(528, 284);
		this.GelismisAramaAgirlikIl.Name = "GelismisAramaAgirlikIl";
		this.GelismisAramaAgirlikIl.Properties.AutoSize = false;
		this.GelismisAramaAgirlikIl.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikIl.TabIndex = 118;
		this.GelismisAramaAgirlikIl.Value = 5;
		this.GelismisAramaAgirlikIl.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikIl.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikIlce.EditValue = 5;
		this.GelismisAramaAgirlikIlce.Location = new System.Drawing.Point(528, 258);
		this.GelismisAramaAgirlikIlce.Name = "GelismisAramaAgirlikIlce";
		this.GelismisAramaAgirlikIlce.Properties.AutoSize = false;
		this.GelismisAramaAgirlikIlce.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikIlce.TabIndex = 117;
		this.GelismisAramaAgirlikIlce.Value = 5;
		this.GelismisAramaAgirlikIlce.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikIlce.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikMahalle.EditValue = 5;
		this.GelismisAramaAgirlikMahalle.Location = new System.Drawing.Point(528, 232);
		this.GelismisAramaAgirlikMahalle.Name = "GelismisAramaAgirlikMahalle";
		this.GelismisAramaAgirlikMahalle.Properties.AutoSize = false;
		this.GelismisAramaAgirlikMahalle.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikMahalle.TabIndex = 116;
		this.GelismisAramaAgirlikMahalle.Value = 5;
		this.GelismisAramaAgirlikMahalle.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikMahalle.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikTelefon.EditValue = 5;
		this.GelismisAramaAgirlikTelefon.Location = new System.Drawing.Point(528, 206);
		this.GelismisAramaAgirlikTelefon.Name = "GelismisAramaAgirlikTelefon";
		this.GelismisAramaAgirlikTelefon.Properties.AutoSize = false;
		this.GelismisAramaAgirlikTelefon.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikTelefon.TabIndex = 115;
		this.GelismisAramaAgirlikTelefon.Value = 5;
		this.GelismisAramaAgirlikTelefon.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikTelefon.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikAciklama.EditValue = 5;
		this.GelismisAramaAgirlikAciklama.Location = new System.Drawing.Point(528, 180);
		this.GelismisAramaAgirlikAciklama.Name = "GelismisAramaAgirlikAciklama";
		this.GelismisAramaAgirlikAciklama.Properties.AutoSize = false;
		this.GelismisAramaAgirlikAciklama.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikAciklama.TabIndex = 114;
		this.GelismisAramaAgirlikAciklama.Value = 5;
		this.GelismisAramaAgirlikAciklama.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikAciklama.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikUnvan2.EditValue = 5;
		this.GelismisAramaAgirlikUnvan2.Location = new System.Drawing.Point(528, 154);
		this.GelismisAramaAgirlikUnvan2.Name = "GelismisAramaAgirlikUnvan2";
		this.GelismisAramaAgirlikUnvan2.Properties.AutoSize = false;
		this.GelismisAramaAgirlikUnvan2.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikUnvan2.TabIndex = 113;
		this.GelismisAramaAgirlikUnvan2.Value = 5;
		this.GelismisAramaAgirlikUnvan2.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikUnvan2.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikUnvan.EditValue = 5;
		this.GelismisAramaAgirlikUnvan.Location = new System.Drawing.Point(528, 128);
		this.GelismisAramaAgirlikUnvan.Name = "GelismisAramaAgirlikUnvan";
		this.GelismisAramaAgirlikUnvan.Properties.AutoSize = false;
		this.GelismisAramaAgirlikUnvan.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikUnvan.TabIndex = 112;
		this.GelismisAramaAgirlikUnvan.Value = 5;
		this.GelismisAramaAgirlikUnvan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikUnvan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikEPosta.EditValue = 5;
		this.GelismisAramaAgirlikEPosta.Location = new System.Drawing.Point(528, 102);
		this.GelismisAramaAgirlikEPosta.Name = "GelismisAramaAgirlikEPosta";
		this.GelismisAramaAgirlikEPosta.Properties.AutoSize = false;
		this.GelismisAramaAgirlikEPosta.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikEPosta.TabIndex = 111;
		this.GelismisAramaAgirlikEPosta.Value = 5;
		this.GelismisAramaAgirlikEPosta.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikEPosta.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikHesapNo.EditValue = 5;
		this.GelismisAramaAgirlikHesapNo.Location = new System.Drawing.Point(528, 76);
		this.GelismisAramaAgirlikHesapNo.Name = "GelismisAramaAgirlikHesapNo";
		this.GelismisAramaAgirlikHesapNo.Properties.AutoSize = false;
		this.GelismisAramaAgirlikHesapNo.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikHesapNo.TabIndex = 110;
		this.GelismisAramaAgirlikHesapNo.Value = 5;
		this.GelismisAramaAgirlikHesapNo.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikHesapNo.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.GelismisAramaAgirlikTcVergiNo.EditValue = 5;
		this.GelismisAramaAgirlikTcVergiNo.Location = new System.Drawing.Point(528, 50);
		this.GelismisAramaAgirlikTcVergiNo.Name = "GelismisAramaAgirlikTcVergiNo";
		this.GelismisAramaAgirlikTcVergiNo.Properties.AutoSize = false;
		this.GelismisAramaAgirlikTcVergiNo.Size = new System.Drawing.Size(142, 19);
		this.GelismisAramaAgirlikTcVergiNo.TabIndex = 109;
		this.GelismisAramaAgirlikTcVergiNo.Value = 5;
		this.GelismisAramaAgirlikTcVergiNo.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.GelismisAramaAgirlikTcVergiNo.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl16.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl16.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl16.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl16.Location = new System.Drawing.Point(41, 26);
		this.labelControl16.Name = "labelControl16";
		this.labelControl16.Size = new System.Drawing.Size(258, 19);
		this.labelControl16.TabIndex = 106;
		this.labelControl16.Text = "Banka verilerinin aranacağı hesaplar";
		this.AraMasraf.Location = new System.Drawing.Point(39, 126);
		this.AraMasraf.Name = "AraMasraf";
		this.AraMasraf.Properties.Caption = "Masraf hesapları";
		this.AraMasraf.Size = new System.Drawing.Size(108, 19);
		this.AraMasraf.TabIndex = 104;
		this.AraMasraf.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AraMasraf.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.AraPersonel.Location = new System.Drawing.Point(39, 101);
		this.AraPersonel.Name = "AraPersonel";
		this.AraPersonel.Properties.Caption = "Personel";
		this.AraPersonel.Size = new System.Drawing.Size(108, 19);
		this.AraPersonel.TabIndex = 103;
		this.AraPersonel.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AraPersonel.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.AraCariPersonel.Location = new System.Drawing.Point(39, 76);
		this.AraCariPersonel.Name = "AraCariPersonel";
		this.AraCariPersonel.Properties.Caption = "Cari personel";
		this.AraCariPersonel.Size = new System.Drawing.Size(108, 19);
		this.AraCariPersonel.TabIndex = 102;
		this.AraCariPersonel.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AraCariPersonel.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.AraCariHesaplar.Location = new System.Drawing.Point(39, 51);
		this.AraCariHesaplar.Name = "AraCariHesaplar";
		this.AraCariHesaplar.Properties.Caption = "Cari Hesaplar";
		this.AraCariHesaplar.Size = new System.Drawing.Size(108, 19);
		this.AraCariHesaplar.TabIndex = 101;
		this.AraCariHesaplar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AraCariHesaplar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label31.Location = new System.Drawing.Point(427, 344);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(290, 47);
		this.label31.TabIndex = 100;
		this.label31.Text = "Bankadan gelen bilgilerin hesaplarda aranırken verilecek önem derecesi belirlenir. Yüksek rakamlar daha fazla önem verilecek alanları ifade eder.";
		this.labelControl15.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl15.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl15.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl15.Location = new System.Drawing.Point(503, 22);
		this.labelControl15.Name = "labelControl15";
		this.labelControl15.Size = new System.Drawing.Size(114, 19);
		this.labelControl15.TabIndex = 88;
		this.labelControl15.Text = "Arama ağırlıkları";
		this.labelControl14.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl14.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl14.Location = new System.Drawing.Point(447, 310);
		this.labelControl14.Name = "labelControl14";
		this.labelControl14.Size = new System.Drawing.Size(75, 13);
		this.labelControl14.TabIndex = 64;
		this.labelControl14.Text = "Posta kodu :";
		this.labelControl13.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl13.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl13.Location = new System.Drawing.Point(447, 284);
		this.labelControl13.Name = "labelControl13";
		this.labelControl13.Size = new System.Drawing.Size(75, 13);
		this.labelControl13.TabIndex = 62;
		this.labelControl13.Text = "İl :";
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(447, 258);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(75, 13);
		this.labelControl4.TabIndex = 60;
		this.labelControl4.Text = "İlçe :";
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(447, 232);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(75, 13);
		this.labelControl5.TabIndex = 58;
		this.labelControl5.Text = "Mahalle :";
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(447, 54);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(75, 13);
		this.labelControl6.TabIndex = 56;
		this.labelControl6.Text = "Tc/Vergi no :";
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(447, 104);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(75, 13);
		this.labelControl7.TabIndex = 54;
		this.labelControl7.Text = "E-posta :";
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(447, 210);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(75, 13);
		this.labelControl8.TabIndex = 52;
		this.labelControl8.Text = "Telefon :";
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(447, 154);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(75, 13);
		this.labelControl9.TabIndex = 50;
		this.labelControl9.Text = "Ünvan 2 :";
		this.labelControl10.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl10.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl10.Location = new System.Drawing.Point(447, 128);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(75, 13);
		this.labelControl10.TabIndex = 48;
		this.labelControl10.Text = "Ünvan :";
		this.labelControl11.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl11.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl11.Location = new System.Drawing.Point(447, 76);
		this.labelControl11.Name = "labelControl11";
		this.labelControl11.Size = new System.Drawing.Size(75, 13);
		this.labelControl11.TabIndex = 46;
		this.labelControl11.Text = "Hesap no :";
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(447, 183);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(75, 13);
		this.labelControl12.TabIndex = 44;
		this.labelControl12.Text = "Açıklama :";
		this.te_eklenecek_parametre_adi.Location = new System.Drawing.Point(13, 352);
		this.te_eklenecek_parametre_adi.Name = "te_eklenecek_parametre_adi";
		this.te_eklenecek_parametre_adi.Size = new System.Drawing.Size(153, 20);
		this.te_eklenecek_parametre_adi.TabIndex = 89;
		this.labelControl17.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(13, 327);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(153, 19);
		this.labelControl17.TabIndex = 89;
		this.labelControl17.Text = "Eklenecek parametre adı";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(934, 562);
		base.Controls.Add(this.labelControl17);
		base.Controls.Add(this.te_eklenecek_parametre_adi);
		base.Controls.Add(this.tc_parametreler);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Controls.Add(this.sb_kullanici_sil);
		base.Controls.Add(this.sb_kullanici_ekle);
		base.Controls.Add(this.lb_kullanicilar);
		base.Name = "Aktarim_Banka_Genel_Parametre_Duzenleme";
		this.Text = "Banka Aktarımı Genel Parametre Düzenleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).EndInit();
		this.tc_parametreler.ResumeLayout(false);
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.EvrakAciklamaBankaBaslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakAciklamaBankaUzunluk.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakAciklamaBankaKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		this.xtraTabPage4.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriBankalarArasiVirmanDekontu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriVerilenFirmaSenediOdemeBordrosu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaMiktari.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaEvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSorumlulukMerkezi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSpecialAlan3.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSpecialAlan2.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSpecialAlan1.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaProjeKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaSorumlulukMerkeziKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirFaturaHesapKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultSatirHesapKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakAciklama.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriVerilenFirmaCekiOdemeBordrosu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsildekiSenetOdeme.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsileSenetCikis.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsileCekCikis.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriTahsildekiCekOdeme.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriNakitYatirma.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriNakitCekme.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriGidenHavale.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DefaultEvrakSeriGelenHavale.Properties).EndInit();
		this.xtraTabPage9.ResumeLayout(false);
		this.xtraTabPage9.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.AraBankalar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AraCekSenet.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaHaricTutulacakKelimeler.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikPostaKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikPostaKodu).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIl.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIl).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIlce.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikIlce).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikMahalle.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikMahalle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTelefon.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTelefon).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikAciklama.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikAciklama).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan2.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikUnvan).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikEPosta.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikEPosta).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikHesapNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikHesapNo).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTcVergiNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.GelismisAramaAgirlikTcVergiNo).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AraMasraf.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AraPersonel.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AraCariPersonel.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AraCariHesaplar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_parametre_adi.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
