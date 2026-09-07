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

public class Aktarim_Banka_Aktarim_Kriter_Duzenleme : XtraForm
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

	private TextEdit te_eklenecek_parametre_adi;

	private LabelControl labelControl17;

	private CheckEdit AramaMinimumTutarKullan;

	private Label label18;

	private CheckEdit AramaAciklamaKullan;

	private TextEdit AramaAciklama;

	private Label label17;

	private CheckEdit AramaIslemKoduKullan;

	private TextEdit AramaIslemKodu;

	private Label label16;

	private Label label44;

	private Label label43;

	private CheckEdit AramaMaksimumTutarKullan;

	private Label label1;

	private SpinEdit DegistirmeFaturaMiktar;

	private System.Windows.Forms.ComboBox DegistirmeFaturaOlusturmaDurumu;

	private System.Windows.Forms.ComboBox DegistirmeSatirCinsi;

	private System.Windows.Forms.ComboBox DegistirmeEvrakTipi;

	private LabelControl labelControl2;

	private TextEdit DegistirmeFaturaEvrakSeri;

	private Label label13;

	private TextEdit DegistirmeEvrakSorumlulukMerkezi;

	private Label label12;

	private TextEdit DegistirmeFaturaProjeKodu;

	private Label label9;

	private TextEdit DegistirmeFaturaSorumlulukMerkezi;

	private Label DefaultSatirFaturaSorumlulukYazi;

	private Label label7;

	private TextEdit DegistirmeFaturaHesapKodu;

	private Label label6;

	private Label label5;

	private TextEdit DegistirmeHesapKodu;

	private Label label10;

	private Label label19;

	private TextEdit DegistirmeAciklama;

	private Label label21;

	private Label label22;

	private SpinEdit AramaMaksimumTutar;

	private SpinEdit AramaMinimumTutar;

	private CheckEdit DegistirmeFaturaEvrakSeriKullan;

	private CheckEdit DegistirmeFaturaMiktarKullan;

	private CheckEdit DegistirmeFaturaHesapKoduKullan;

	private CheckEdit DegistirmeFaturaOlusturmaDurumuKullan;

	private CheckEdit DegistirmeFaturaProjeKoduKullan;

	private CheckEdit DegistirmeFaturaSorumlulukMerkeziKullan;

	private CheckEdit DegistirmeEvrakSorumlulukMerkeziKullan;

	private CheckEdit DegistirmeHesapKoduKullan;

	private CheckEdit DegistirmeSatirCinsiKullan;

	private CheckEdit DegistirmeAciklamaKullan;

	private CheckEdit DegistirmeEvrakTipiKullan;

	public Aktarim_Banka_Aktarim_Kriter_Duzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
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
		DegistirmeEvrakTipi.DataSource = dataSource;
		DegistirmeEvrakTipi.ValueMember = "ID";
		DegistirmeEvrakTipi.DisplayMember = "Isim";
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
		DegistirmeSatirCinsi.DataSource = dataSource2;
		DegistirmeSatirCinsi.ValueMember = "ID";
		DegistirmeSatirCinsi.DisplayMember = "Isim";
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
		DegistirmeFaturaOlusturmaDurumu.DataSource = dataSource3;
		DegistirmeFaturaOlusturmaDurumu.ValueMember = "ID";
		DegistirmeFaturaOlusturmaDurumu.DisplayMember = "Isim";
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		KullanicilariListele();
	}

	private void KullanicilariListele()
	{
		lb_kullanicilar.Items.Clear();
		foreach (string item in AktarimBankaKriterParametre.GetKriterAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
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
			_kullaniciparametreleri = ParametrelerDefault.BankaAktarimKriter(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "BankaAktarim", "", "Kriter", AktifKullanici);
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
		AramaIslemKodu.Text = _kullaniciparametreleri._GetParametre("AramaIslemKodu")._GetString;
		AramaIslemKoduKullan.Checked = _kullaniciparametreleri._GetParametre("AramaIslemKoduKullan")._GetBoolean;
		AramaAciklama.Text = _kullaniciparametreleri._GetParametre("AramaAciklama")._GetString;
		AramaAciklamaKullan.Checked = _kullaniciparametreleri._GetParametre("AramaAciklamaKullan")._GetBoolean;
		AramaMinimumTutar.Value = _kullaniciparametreleri._GetParametre("AramaMinimumTutar")._GetInt;
		AramaMinimumTutarKullan.Checked = _kullaniciparametreleri._GetParametre("AramaMinimumTutarKullan")._GetBoolean;
		AramaMaksimumTutar.Value = _kullaniciparametreleri._GetParametre("AramaMaksimumTutar")._GetInt;
		AramaMaksimumTutarKullan.Checked = _kullaniciparametreleri._GetParametre("AramaMaksimumTutarKullan")._GetBoolean;
		DegistirmeEvrakTipi.SelectedValue = _kullaniciparametreleri._GetParametre("DegistirmeEvrakTipi")._GetInt;
		DegistirmeEvrakTipiKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeEvrakTipiKullan")._GetBoolean;
		DegistirmeEvrakSorumlulukMerkezi.Text = _kullaniciparametreleri._GetParametre("DegistirmeEvrakSorumlulukMerkezi")._GetString;
		DegistirmeEvrakSorumlulukMerkeziKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeEvrakSorumlulukMerkeziKullan")._GetBoolean;
		DegistirmeSatirCinsi.SelectedValue = _kullaniciparametreleri._GetParametre("DegistirmeSatirCinsi")._GetInt;
		DegistirmeSatirCinsiKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeSatirCinsiKullan")._GetBoolean;
		DegistirmeHesapKodu.Text = _kullaniciparametreleri._GetParametre("DegistirmeHesapKodu")._GetString;
		DegistirmeHesapKoduKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeHesapKoduKullan")._GetBoolean;
		DegistirmeFaturaSorumlulukMerkezi.Text = _kullaniciparametreleri._GetParametre("DegistirmeFaturaSorumlulukMerkezi")._GetString;
		DegistirmeFaturaSorumlulukMerkeziKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeFaturaSorumlulukMerkeziKullan")._GetBoolean;
		DegistirmeFaturaProjeKodu.Text = _kullaniciparametreleri._GetParametre("DegistirmeFaturaProjeKodu")._GetString;
		DegistirmeFaturaProjeKoduKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeFaturaProjeKoduKullan")._GetBoolean;
		DegistirmeAciklama.Text = _kullaniciparametreleri._GetParametre("DegistirmeAciklama")._GetString;
		DegistirmeAciklamaKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeAciklamaKullan")._GetBoolean;
		DegistirmeFaturaOlusturmaDurumu.SelectedValue = _kullaniciparametreleri._GetParametre("DegistirmeFaturaOlusturmaDurumu")._GetInt;
		DegistirmeFaturaOlusturmaDurumuKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeFaturaOlusturmaDurumuKullan")._GetBoolean;
		DegistirmeFaturaHesapKodu.Text = _kullaniciparametreleri._GetParametre("DegistirmeFaturaHesapKodu")._GetString;
		DegistirmeFaturaHesapKoduKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeFaturaHesapKoduKullan")._GetBoolean;
		DegistirmeFaturaMiktar.Value = _kullaniciparametreleri._GetParametre("DegistirmeFaturaMiktar")._GetInt;
		DegistirmeFaturaMiktarKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeFaturaMiktarKullan")._GetBoolean;
		DegistirmeFaturaEvrakSeri.Text = _kullaniciparametreleri._GetParametre("DegistirmeFaturaEvrakSeri")._GetString;
		DegistirmeFaturaEvrakSeriKullan.Checked = _kullaniciparametreleri._GetParametre("DegistirmeFaturaEvrakSeriKullan")._GetBoolean;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		KullaniciParametreKaydet();
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_kullaniciparametreleri._GetParametre("KriterAdi")._SetString = AktifKullanici;
		_kullaniciparametreleri._GetParametre("AramaIslemKodu")._SetString = AramaIslemKodu.Text;
		_kullaniciparametreleri._GetParametre("AramaIslemKoduKullan")._SetBoolean = AramaIslemKoduKullan.Checked;
		_kullaniciparametreleri._GetParametre("AramaAciklama")._SetString = AramaAciklama.Text;
		_kullaniciparametreleri._GetParametre("AramaAciklamaKullan")._SetBoolean = AramaAciklamaKullan.Checked;
		_kullaniciparametreleri._GetParametre("AramaMinimumTutar")._SetInt = (int)AramaMinimumTutar.Value;
		_kullaniciparametreleri._GetParametre("AramaMinimumTutarKullan")._SetBoolean = AramaMinimumTutarKullan.Checked;
		_kullaniciparametreleri._GetParametre("AramaMaksimumTutar")._SetInt = (int)AramaMaksimumTutar.Value;
		_kullaniciparametreleri._GetParametre("AramaMaksimumTutarKullan")._SetBoolean = AramaMaksimumTutarKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeEvrakTipi")._SetInt = (int)DegistirmeEvrakTipi.SelectedValue;
		_kullaniciparametreleri._GetParametre("DegistirmeEvrakTipiKullan")._SetBoolean = DegistirmeEvrakTipiKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeEvrakSorumlulukMerkezi")._SetString = DegistirmeEvrakSorumlulukMerkezi.Text;
		_kullaniciparametreleri._GetParametre("DegistirmeEvrakSorumlulukMerkeziKullan")._SetBoolean = DegistirmeEvrakSorumlulukMerkeziKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeSatirCinsi")._SetInt = (int)DegistirmeSatirCinsi.SelectedValue;
		_kullaniciparametreleri._GetParametre("DegistirmeSatirCinsiKullan")._SetBoolean = DegistirmeSatirCinsiKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeHesapKodu")._SetString = DegistirmeHesapKodu.Text;
		_kullaniciparametreleri._GetParametre("DegistirmeHesapKoduKullan")._SetBoolean = DegistirmeHesapKoduKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaSorumlulukMerkezi")._SetString = DegistirmeFaturaSorumlulukMerkezi.Text;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaSorumlulukMerkeziKullan")._SetBoolean = DegistirmeFaturaSorumlulukMerkeziKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaProjeKodu")._SetString = DegistirmeFaturaProjeKodu.Text;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaProjeKoduKullan")._SetBoolean = DegistirmeFaturaProjeKoduKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeAciklama")._SetString = DegistirmeAciklama.Text;
		_kullaniciparametreleri._GetParametre("DegistirmeAciklamaKullan")._SetBoolean = DegistirmeAciklamaKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaOlusturmaDurumu")._SetInt = (int)DegistirmeFaturaOlusturmaDurumu.SelectedValue;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaOlusturmaDurumuKullan")._SetBoolean = DegistirmeFaturaOlusturmaDurumuKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaHesapKodu")._SetString = DegistirmeFaturaHesapKodu.Text;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaHesapKoduKullan")._SetBoolean = DegistirmeFaturaHesapKoduKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaMiktar")._SetInt = (int)DegistirmeFaturaMiktar.Value;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaMiktarKullan")._SetBoolean = DegistirmeFaturaMiktarKullan.Checked;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaEvrakSeri")._SetString = DegistirmeFaturaEvrakSeri.Text;
		_kullaniciparametreleri._GetParametre("DegistirmeFaturaEvrakSeriKullan")._SetBoolean = DegistirmeFaturaEvrakSeriKullan.Checked;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri);
		_kullaniciparametreleri = ParametrelerDefault.BankaAktarimKriter(AktifKullanici);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "BankaAktarim", "", "Kriter", AktifKullanici);
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
			Parametreler parametreler = ParametrelerDefault.BankaAktarimKriter(te_eklenecek_parametre_adi.Text);
			parametreler._GetParametre("KriterAdi")._SetString = te_eklenecek_parametre_adi.Text;
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
			AktarimBankaKriterParametre.KriterSil(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text);
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
		this.label44 = new System.Windows.Forms.Label();
		this.label43 = new System.Windows.Forms.Label();
		this.AramaMinimumTutarKullan = new DevExpress.XtraEditors.CheckEdit();
		this.label18 = new System.Windows.Forms.Label();
		this.AramaAciklamaKullan = new DevExpress.XtraEditors.CheckEdit();
		this.AramaAciklama = new DevExpress.XtraEditors.TextEdit();
		this.label17 = new System.Windows.Forms.Label();
		this.AramaIslemKoduKullan = new DevExpress.XtraEditors.CheckEdit();
		this.AramaIslemKodu = new DevExpress.XtraEditors.TextEdit();
		this.label16 = new System.Windows.Forms.Label();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.te_eklenecek_parametre_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		this.AramaMaksimumTutarKullan = new DevExpress.XtraEditors.CheckEdit();
		this.label1 = new System.Windows.Forms.Label();
		this.DegistirmeFaturaMiktar = new DevExpress.XtraEditors.SpinEdit();
		this.DegistirmeFaturaOlusturmaDurumu = new System.Windows.Forms.ComboBox();
		this.DegistirmeSatirCinsi = new System.Windows.Forms.ComboBox();
		this.DegistirmeEvrakTipi = new System.Windows.Forms.ComboBox();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.DegistirmeFaturaEvrakSeri = new DevExpress.XtraEditors.TextEdit();
		this.label13 = new System.Windows.Forms.Label();
		this.DegistirmeEvrakSorumlulukMerkezi = new DevExpress.XtraEditors.TextEdit();
		this.label12 = new System.Windows.Forms.Label();
		this.DegistirmeFaturaProjeKodu = new DevExpress.XtraEditors.TextEdit();
		this.label9 = new System.Windows.Forms.Label();
		this.DegistirmeFaturaSorumlulukMerkezi = new DevExpress.XtraEditors.TextEdit();
		this.DefaultSatirFaturaSorumlulukYazi = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.DegistirmeFaturaHesapKodu = new DevExpress.XtraEditors.TextEdit();
		this.label6 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.DegistirmeHesapKodu = new DevExpress.XtraEditors.TextEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.label19 = new System.Windows.Forms.Label();
		this.DegistirmeAciklama = new DevExpress.XtraEditors.TextEdit();
		this.label21 = new System.Windows.Forms.Label();
		this.label22 = new System.Windows.Forms.Label();
		this.AramaMinimumTutar = new DevExpress.XtraEditors.SpinEdit();
		this.AramaMaksimumTutar = new DevExpress.XtraEditors.SpinEdit();
		this.DegistirmeEvrakTipiKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeAciklamaKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeSatirCinsiKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeHesapKoduKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeEvrakSorumlulukMerkeziKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeFaturaSorumlulukMerkeziKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeFaturaProjeKoduKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeFaturaOlusturmaDurumuKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeFaturaHesapKoduKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeFaturaMiktarKullan = new DevExpress.XtraEditors.CheckEdit();
		this.DegistirmeFaturaEvrakSeriKullan = new DevExpress.XtraEditors.CheckEdit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).BeginInit();
		this.tc_parametreler.SuspendLayout();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.AramaMinimumTutarKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AramaAciklamaKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AramaAciklama.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AramaIslemKoduKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AramaIslemKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_parametre_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AramaMaksimumTutarKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaMiktar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaEvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeEvrakSorumlulukMerkezi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaProjeKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaSorumlulukMerkezi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaHesapKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeHesapKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeAciklama.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AramaMinimumTutar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AramaMaksimumTutar.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeEvrakTipiKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeAciklamaKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeSatirCinsiKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeHesapKoduKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeEvrakSorumlulukMerkeziKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaSorumlulukMerkeziKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaProjeKoduKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaOlusturmaDurumuKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaHesapKoduKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaMiktarKullan.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaEvrakSeriKullan.Properties).BeginInit();
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
		this.sb_kullanici_ekle.Text = "Kriter Ekle";
		this.sb_kullanici_ekle.Click += new System.EventHandler(sb_kullanici_ekle_Click);
		this.sb_kullanici_sil.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_sil.Appearance.Options.UseFont = true;
		this.sb_kullanici_sil.Enabled = false;
		this.sb_kullanici_sil.Location = new System.Drawing.Point(13, 262);
		this.sb_kullanici_sil.Name = "sb_kullanici_sil";
		this.sb_kullanici_sil.Size = new System.Drawing.Size(153, 23);
		this.sb_kullanici_sil.TabIndex = 7;
		this.sb_kullanici_sil.Text = "Kriter Sil";
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
		this.tc_parametreler.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[1] { this.xtraTabPage3 });
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaEvrakSeriKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaMiktarKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaHesapKoduKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaOlusturmaDurumuKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaProjeKoduKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaSorumlulukMerkeziKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeEvrakSorumlulukMerkeziKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeHesapKoduKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeSatirCinsiKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeAciklamaKullan);
		this.xtraTabPage3.Controls.Add(this.DegistirmeEvrakTipiKullan);
		this.xtraTabPage3.Controls.Add(this.AramaMaksimumTutar);
		this.xtraTabPage3.Controls.Add(this.AramaMinimumTutar);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaMiktar);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaOlusturmaDurumu);
		this.xtraTabPage3.Controls.Add(this.DegistirmeSatirCinsi);
		this.xtraTabPage3.Controls.Add(this.DegistirmeEvrakTipi);
		this.xtraTabPage3.Controls.Add(this.labelControl2);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaEvrakSeri);
		this.xtraTabPage3.Controls.Add(this.label13);
		this.xtraTabPage3.Controls.Add(this.DegistirmeEvrakSorumlulukMerkezi);
		this.xtraTabPage3.Controls.Add(this.label12);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaProjeKodu);
		this.xtraTabPage3.Controls.Add(this.label9);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaSorumlulukMerkezi);
		this.xtraTabPage3.Controls.Add(this.DefaultSatirFaturaSorumlulukYazi);
		this.xtraTabPage3.Controls.Add(this.label7);
		this.xtraTabPage3.Controls.Add(this.DegistirmeFaturaHesapKodu);
		this.xtraTabPage3.Controls.Add(this.label6);
		this.xtraTabPage3.Controls.Add(this.label5);
		this.xtraTabPage3.Controls.Add(this.DegistirmeHesapKodu);
		this.xtraTabPage3.Controls.Add(this.label10);
		this.xtraTabPage3.Controls.Add(this.label19);
		this.xtraTabPage3.Controls.Add(this.DegistirmeAciklama);
		this.xtraTabPage3.Controls.Add(this.label21);
		this.xtraTabPage3.Controls.Add(this.label22);
		this.xtraTabPage3.Controls.Add(this.AramaMaksimumTutarKullan);
		this.xtraTabPage3.Controls.Add(this.label1);
		this.xtraTabPage3.Controls.Add(this.label44);
		this.xtraTabPage3.Controls.Add(this.label43);
		this.xtraTabPage3.Controls.Add(this.AramaMinimumTutarKullan);
		this.xtraTabPage3.Controls.Add(this.label18);
		this.xtraTabPage3.Controls.Add(this.AramaAciklamaKullan);
		this.xtraTabPage3.Controls.Add(this.AramaAciklama);
		this.xtraTabPage3.Controls.Add(this.label17);
		this.xtraTabPage3.Controls.Add(this.AramaIslemKoduKullan);
		this.xtraTabPage3.Controls.Add(this.AramaIslemKodu);
		this.xtraTabPage3.Controls.Add(this.label16);
		this.xtraTabPage3.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage3.Controls.Add(this.labelControl1);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(740, 452);
		this.xtraTabPage3.Text = "Genel";
		this.label44.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label44.Location = new System.Drawing.Point(454, 44);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(204, 19);
		this.label44.TabIndex = 150;
		this.label44.Text = "DEĞİŞECEK ALANLAR";
		this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label43.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label43.Location = new System.Drawing.Point(80, 127);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(158, 19);
		this.label43.TabIndex = 149;
		this.label43.Text = "Aranacak özellikler";
		this.label43.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.AramaMinimumTutarKullan.Location = new System.Drawing.Point(180, 204);
		this.AramaMinimumTutarKullan.Name = "AramaMinimumTutarKullan";
		this.AramaMinimumTutarKullan.Properties.Caption = "kullan";
		this.AramaMinimumTutarKullan.Size = new System.Drawing.Size(58, 19);
		this.AramaMinimumTutarKullan.TabIndex = 48;
		this.AramaMinimumTutarKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AramaMinimumTutarKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label18.Location = new System.Drawing.Point(4, 203);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(96, 19);
		this.label18.TabIndex = 51;
		this.label18.Text = "Minimum tutar :";
		this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.AramaAciklamaKullan.Location = new System.Drawing.Point(220, 177);
		this.AramaAciklamaKullan.Name = "AramaAciklamaKullan";
		this.AramaAciklamaKullan.Properties.Caption = "kullan";
		this.AramaAciklamaKullan.Size = new System.Drawing.Size(58, 19);
		this.AramaAciklamaKullan.TabIndex = 46;
		this.AramaAciklamaKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AramaAciklamaKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.AramaAciklama.Location = new System.Drawing.Point(105, 177);
		this.AramaAciklama.Name = "AramaAciklama";
		this.AramaAciklama.Size = new System.Drawing.Size(106, 20);
		this.AramaAciklama.TabIndex = 45;
		this.AramaAciklama.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label17.Location = new System.Drawing.Point(24, 177);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(76, 19);
		this.label17.TabIndex = 50;
		this.label17.Text = "Açıklama :";
		this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.AramaIslemKoduKullan.Location = new System.Drawing.Point(220, 153);
		this.AramaIslemKoduKullan.Name = "AramaIslemKoduKullan";
		this.AramaIslemKoduKullan.Properties.Caption = "kullan";
		this.AramaIslemKoduKullan.Size = new System.Drawing.Size(58, 19);
		this.AramaIslemKoduKullan.TabIndex = 44;
		this.AramaIslemKoduKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AramaIslemKoduKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.AramaIslemKodu.Location = new System.Drawing.Point(105, 153);
		this.AramaIslemKodu.Name = "AramaIslemKodu";
		this.AramaIslemKodu.Size = new System.Drawing.Size(106, 20);
		this.AramaIslemKodu.TabIndex = 43;
		this.AramaIslemKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label16.Location = new System.Drawing.Point(21, 153);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(79, 19);
		this.label16.TabIndex = 49;
		this.label16.Text = "İşlem kodu :";
		this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(105, 75);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(106, 20);
		this.te_kullanici_adi.TabIndex = 2;
		this.te_kullanici_adi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(20, 78);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(79, 13);
		this.labelControl1.TabIndex = 0;
		this.labelControl1.Text = "Kriter adı :";
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
		this.labelControl17.Text = "Eklenecek kriter adı";
		this.AramaMaksimumTutarKullan.Location = new System.Drawing.Point(180, 230);
		this.AramaMaksimumTutarKullan.Name = "AramaMaksimumTutarKullan";
		this.AramaMaksimumTutarKullan.Properties.Caption = "kullan";
		this.AramaMaksimumTutarKullan.Size = new System.Drawing.Size(58, 19);
		this.AramaMaksimumTutarKullan.TabIndex = 152;
		this.AramaMaksimumTutarKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AramaMaksimumTutarKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label1.Location = new System.Drawing.Point(4, 229);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(96, 19);
		this.label1.TabIndex = 153;
		this.label1.Text = "Maksimum tutar :";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DegistirmeFaturaMiktar.EditValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DegistirmeFaturaMiktar.Location = new System.Drawing.Point(455, 337);
		this.DegistirmeFaturaMiktar.Name = "DegistirmeFaturaMiktar";
		this.DegistirmeFaturaMiktar.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.DegistirmeFaturaMiktar.Properties.IsFloatValue = false;
		this.DegistirmeFaturaMiktar.Properties.Mask.EditMask = "N00";
		this.DegistirmeFaturaMiktar.Properties.MaxValue = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.DegistirmeFaturaMiktar.Properties.MinValue = new decimal(new int[4] { 1, 0, 0, 0 });
		this.DegistirmeFaturaMiktar.Size = new System.Drawing.Size(54, 20);
		this.DegistirmeFaturaMiktar.TabIndex = 176;
		this.DegistirmeFaturaMiktar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaMiktar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeFaturaOlusturmaDurumu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DegistirmeFaturaOlusturmaDurumu.FormattingEnabled = true;
		this.DegistirmeFaturaOlusturmaDurumu.Location = new System.Drawing.Point(455, 284);
		this.DegistirmeFaturaOlusturmaDurumu.Name = "DegistirmeFaturaOlusturmaDurumu";
		this.DegistirmeFaturaOlusturmaDurumu.Size = new System.Drawing.Size(205, 21);
		this.DegistirmeFaturaOlusturmaDurumu.TabIndex = 175;
		this.DegistirmeFaturaOlusturmaDurumu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaOlusturmaDurumu.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeSatirCinsi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DegistirmeSatirCinsi.FormattingEnabled = true;
		this.DegistirmeSatirCinsi.Location = new System.Drawing.Point(454, 127);
		this.DegistirmeSatirCinsi.Name = "DegistirmeSatirCinsi";
		this.DegistirmeSatirCinsi.Size = new System.Drawing.Size(204, 21);
		this.DegistirmeSatirCinsi.TabIndex = 174;
		this.DegistirmeSatirCinsi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeSatirCinsi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeEvrakTipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DegistirmeEvrakTipi.FormattingEnabled = true;
		this.DegistirmeEvrakTipi.Location = new System.Drawing.Point(454, 75);
		this.DegistirmeEvrakTipi.Name = "DegistirmeEvrakTipi";
		this.DegistirmeEvrakTipi.Size = new System.Drawing.Size(205, 21);
		this.DegistirmeEvrakTipi.TabIndex = 173;
		this.DegistirmeEvrakTipi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeEvrakTipi.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(455, 259);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(207, 19);
		this.labelControl2.TabIndex = 172;
		this.labelControl2.Text = "Fatura";
		this.DegistirmeFaturaEvrakSeri.Location = new System.Drawing.Point(455, 363);
		this.DegistirmeFaturaEvrakSeri.Name = "DegistirmeFaturaEvrakSeri";
		this.DegistirmeFaturaEvrakSeri.Properties.MaxLength = 4;
		this.DegistirmeFaturaEvrakSeri.Size = new System.Drawing.Size(54, 20);
		this.DegistirmeFaturaEvrakSeri.TabIndex = 170;
		this.DegistirmeFaturaEvrakSeri.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label13.Location = new System.Drawing.Point(284, 363);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(164, 19);
		this.label13.TabIndex = 171;
		this.label13.Text = "Fatura evrak seri :";
		this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DegistirmeEvrakSorumlulukMerkezi.Location = new System.Drawing.Point(454, 180);
		this.DegistirmeEvrakSorumlulukMerkezi.Name = "DegistirmeEvrakSorumlulukMerkezi";
		this.DegistirmeEvrakSorumlulukMerkezi.Size = new System.Drawing.Size(206, 20);
		this.DegistirmeEvrakSorumlulukMerkezi.TabIndex = 168;
		this.DegistirmeEvrakSorumlulukMerkezi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label12.Location = new System.Drawing.Point(281, 180);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(164, 19);
		this.label12.TabIndex = 169;
		this.label12.Text = "Evrak Srm. merkezi kodu :";
		this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DegistirmeFaturaProjeKodu.Location = new System.Drawing.Point(455, 232);
		this.DegistirmeFaturaProjeKodu.Name = "DegistirmeFaturaProjeKodu";
		this.DegistirmeFaturaProjeKodu.Size = new System.Drawing.Size(206, 20);
		this.DegistirmeFaturaProjeKodu.TabIndex = 164;
		this.DegistirmeFaturaProjeKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label9.Location = new System.Drawing.Point(284, 232);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(164, 19);
		this.label9.TabIndex = 167;
		this.label9.Text = "Satır proje kodu :";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DegistirmeFaturaSorumlulukMerkezi.Location = new System.Drawing.Point(455, 206);
		this.DegistirmeFaturaSorumlulukMerkezi.Name = "DegistirmeFaturaSorumlulukMerkezi";
		this.DegistirmeFaturaSorumlulukMerkezi.Size = new System.Drawing.Size(206, 20);
		this.DegistirmeFaturaSorumlulukMerkezi.TabIndex = 163;
		this.DegistirmeFaturaSorumlulukMerkezi.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DefaultSatirFaturaSorumlulukYazi.Location = new System.Drawing.Point(284, 206);
		this.DefaultSatirFaturaSorumlulukYazi.Name = "DefaultSatirFaturaSorumlulukYazi";
		this.DefaultSatirFaturaSorumlulukYazi.Size = new System.Drawing.Size(164, 19);
		this.DefaultSatirFaturaSorumlulukYazi.TabIndex = 166;
		this.DefaultSatirFaturaSorumlulukYazi.Text = "Satır Srm. merkezi kodu :";
		this.DefaultSatirFaturaSorumlulukYazi.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label7.Location = new System.Drawing.Point(287, 338);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(161, 19);
		this.label7.TabIndex = 165;
		this.label7.Text = "Fatura miktarı :";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DegistirmeFaturaHesapKodu.Location = new System.Drawing.Point(455, 310);
		this.DegistirmeFaturaHesapKodu.Name = "DegistirmeFaturaHesapKodu";
		this.DegistirmeFaturaHesapKodu.Size = new System.Drawing.Size(206, 20);
		this.DegistirmeFaturaHesapKodu.TabIndex = 161;
		this.DegistirmeFaturaHesapKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label6.Location = new System.Drawing.Point(287, 312);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(161, 19);
		this.label6.TabIndex = 162;
		this.label6.Text = "Fatura hesap kodu :";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label5.Location = new System.Drawing.Point(284, 285);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(164, 19);
		this.label5.TabIndex = 160;
		this.label5.Text = "Fatura :";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DegistirmeHesapKodu.Location = new System.Drawing.Point(454, 154);
		this.DegistirmeHesapKodu.Name = "DegistirmeHesapKodu";
		this.DegistirmeHesapKodu.Size = new System.Drawing.Size(206, 20);
		this.DegistirmeHesapKodu.TabIndex = 159;
		this.DegistirmeHesapKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label10.Location = new System.Drawing.Point(282, 156);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(164, 19);
		this.label10.TabIndex = 158;
		this.label10.Text = "Hesap kodu :";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label19.Location = new System.Drawing.Point(282, 129);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(164, 19);
		this.label19.TabIndex = 157;
		this.label19.Text = "Cinsi :";
		this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.DegistirmeAciklama.Location = new System.Drawing.Point(454, 102);
		this.DegistirmeAciklama.Name = "DegistirmeAciklama";
		this.DegistirmeAciklama.Properties.MaxLength = 40;
		this.DegistirmeAciklama.Size = new System.Drawing.Size(205, 20);
		this.DegistirmeAciklama.TabIndex = 156;
		this.DegistirmeAciklama.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.label21.Location = new System.Drawing.Point(285, 104);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(161, 19);
		this.label21.TabIndex = 155;
		this.label21.Text = "Açıklama (Mak. 40 karakter) :";
		this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label22.Location = new System.Drawing.Point(345, 75);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(101, 19);
		this.label22.TabIndex = 154;
		this.label22.Text = "Evrak tipi :";
		this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.AramaMinimumTutar.EditValue = new decimal(new int[4]);
		this.AramaMinimumTutar.Location = new System.Drawing.Point(105, 203);
		this.AramaMinimumTutar.Name = "AramaMinimumTutar";
		this.AramaMinimumTutar.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.AramaMinimumTutar.Properties.IsFloatValue = false;
		this.AramaMinimumTutar.Properties.Mask.EditMask = "N00";
		this.AramaMinimumTutar.Properties.MaxValue = new decimal(new int[4] { 40, 0, 0, 0 });
		this.AramaMinimumTutar.Size = new System.Drawing.Size(64, 20);
		this.AramaMinimumTutar.TabIndex = 177;
		this.AramaMinimumTutar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AramaMinimumTutar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.AramaMaksimumTutar.EditValue = new decimal(new int[4]);
		this.AramaMaksimumTutar.Location = new System.Drawing.Point(105, 229);
		this.AramaMaksimumTutar.Name = "AramaMaksimumTutar";
		this.AramaMaksimumTutar.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.AramaMaksimumTutar.Properties.IsFloatValue = false;
		this.AramaMaksimumTutar.Properties.Mask.EditMask = "N00";
		this.AramaMaksimumTutar.Properties.MaxValue = new decimal(new int[4] { 40, 0, 0, 0 });
		this.AramaMaksimumTutar.Size = new System.Drawing.Size(64, 20);
		this.AramaMaksimumTutar.TabIndex = 178;
		this.AramaMaksimumTutar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.AramaMaksimumTutar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeEvrakTipiKullan.Location = new System.Drawing.Point(665, 76);
		this.DegistirmeEvrakTipiKullan.Name = "DegistirmeEvrakTipiKullan";
		this.DegistirmeEvrakTipiKullan.Properties.Caption = "kullan";
		this.DegistirmeEvrakTipiKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeEvrakTipiKullan.TabIndex = 179;
		this.DegistirmeEvrakTipiKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeEvrakTipiKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeAciklamaKullan.Location = new System.Drawing.Point(665, 102);
		this.DegistirmeAciklamaKullan.Name = "DegistirmeAciklamaKullan";
		this.DegistirmeAciklamaKullan.Properties.Caption = "kullan";
		this.DegistirmeAciklamaKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeAciklamaKullan.TabIndex = 180;
		this.DegistirmeAciklamaKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeAciklamaKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeSatirCinsiKullan.Location = new System.Drawing.Point(665, 129);
		this.DegistirmeSatirCinsiKullan.Name = "DegistirmeSatirCinsiKullan";
		this.DegistirmeSatirCinsiKullan.Properties.Caption = "kullan";
		this.DegistirmeSatirCinsiKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeSatirCinsiKullan.TabIndex = 181;
		this.DegistirmeSatirCinsiKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeSatirCinsiKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeHesapKoduKullan.Location = new System.Drawing.Point(666, 153);
		this.DegistirmeHesapKoduKullan.Name = "DegistirmeHesapKoduKullan";
		this.DegistirmeHesapKoduKullan.Properties.Caption = "kullan";
		this.DegistirmeHesapKoduKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeHesapKoduKullan.TabIndex = 182;
		this.DegistirmeHesapKoduKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeHesapKoduKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeEvrakSorumlulukMerkeziKullan.Location = new System.Drawing.Point(666, 181);
		this.DegistirmeEvrakSorumlulukMerkeziKullan.Name = "DegistirmeEvrakSorumlulukMerkeziKullan";
		this.DegistirmeEvrakSorumlulukMerkeziKullan.Properties.Caption = "kullan";
		this.DegistirmeEvrakSorumlulukMerkeziKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeEvrakSorumlulukMerkeziKullan.TabIndex = 183;
		this.DegistirmeEvrakSorumlulukMerkeziKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeEvrakSorumlulukMerkeziKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeFaturaSorumlulukMerkeziKullan.Location = new System.Drawing.Point(667, 206);
		this.DegistirmeFaturaSorumlulukMerkeziKullan.Name = "DegistirmeFaturaSorumlulukMerkeziKullan";
		this.DegistirmeFaturaSorumlulukMerkeziKullan.Properties.Caption = "kullan";
		this.DegistirmeFaturaSorumlulukMerkeziKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeFaturaSorumlulukMerkeziKullan.TabIndex = 184;
		this.DegistirmeFaturaSorumlulukMerkeziKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaSorumlulukMerkeziKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeFaturaProjeKoduKullan.Location = new System.Drawing.Point(667, 232);
		this.DegistirmeFaturaProjeKoduKullan.Name = "DegistirmeFaturaProjeKoduKullan";
		this.DegistirmeFaturaProjeKoduKullan.Properties.Caption = "kullan";
		this.DegistirmeFaturaProjeKoduKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeFaturaProjeKoduKullan.TabIndex = 185;
		this.DegistirmeFaturaProjeKoduKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaProjeKoduKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeFaturaOlusturmaDurumuKullan.Location = new System.Drawing.Point(667, 284);
		this.DegistirmeFaturaOlusturmaDurumuKullan.Name = "DegistirmeFaturaOlusturmaDurumuKullan";
		this.DegistirmeFaturaOlusturmaDurumuKullan.Properties.Caption = "kullan";
		this.DegistirmeFaturaOlusturmaDurumuKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeFaturaOlusturmaDurumuKullan.TabIndex = 186;
		this.DegistirmeFaturaOlusturmaDurumuKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaOlusturmaDurumuKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeFaturaHesapKoduKullan.Location = new System.Drawing.Point(667, 310);
		this.DegistirmeFaturaHesapKoduKullan.Name = "DegistirmeFaturaHesapKoduKullan";
		this.DegistirmeFaturaHesapKoduKullan.Properties.Caption = "kullan";
		this.DegistirmeFaturaHesapKoduKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeFaturaHesapKoduKullan.TabIndex = 187;
		this.DegistirmeFaturaHesapKoduKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaHesapKoduKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeFaturaMiktarKullan.Location = new System.Drawing.Point(515, 338);
		this.DegistirmeFaturaMiktarKullan.Name = "DegistirmeFaturaMiktarKullan";
		this.DegistirmeFaturaMiktarKullan.Properties.Caption = "kullan";
		this.DegistirmeFaturaMiktarKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeFaturaMiktarKullan.TabIndex = 188;
		this.DegistirmeFaturaMiktarKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaMiktarKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.DegistirmeFaturaEvrakSeriKullan.Location = new System.Drawing.Point(515, 363);
		this.DegistirmeFaturaEvrakSeriKullan.Name = "DegistirmeFaturaEvrakSeriKullan";
		this.DegistirmeFaturaEvrakSeriKullan.Properties.Caption = "kullan";
		this.DegistirmeFaturaEvrakSeriKullan.Size = new System.Drawing.Size(58, 19);
		this.DegistirmeFaturaEvrakSeriKullan.TabIndex = 189;
		this.DegistirmeFaturaEvrakSeriKullan.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.DegistirmeFaturaEvrakSeriKullan.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
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
		base.Name = "Aktarim_Banka_Aktarim_Kriter_Duzenleme";
		this.Text = "Banka Aktarımı Kriter Düzenleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).EndInit();
		this.tc_parametreler.ResumeLayout(false);
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.AramaMinimumTutarKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AramaAciklamaKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AramaAciklama.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AramaIslemKoduKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AramaIslemKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_eklenecek_parametre_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AramaMaksimumTutarKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaMiktar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaEvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeEvrakSorumlulukMerkezi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaProjeKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaSorumlulukMerkezi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaHesapKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeHesapKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeAciklama.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AramaMinimumTutar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AramaMaksimumTutar.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeEvrakTipiKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeAciklamaKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeSatirCinsiKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeHesapKoduKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeEvrakSorumlulukMerkeziKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaSorumlulukMerkeziKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaProjeKoduKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaOlusturmaDurumuKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaHesapKoduKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaMiktarKullan.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DegistirmeFaturaEvrakSeriKullan.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
