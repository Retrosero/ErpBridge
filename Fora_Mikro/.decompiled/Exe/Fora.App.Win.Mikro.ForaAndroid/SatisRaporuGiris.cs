using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.App.Win.Mikro.Properties;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Rapor.StokSatis;
using Fora.Mikro.Utility;
using Newtonsoft.Json;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class SatisRaporuGiris : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _KullaniciParametreleri;

	private IContainer components;

	private LabelControl lc_kullanici_kodu;

	private LabelControl labelControl5;

	private TextEdit te_giris_sifresi;

	private TextEdit te_kullaniciadi;

	private SimpleButton sb_giris;

	private TableLayoutPanel tableLayoutPanel1;

	private Label label_uyari;

	private PictureBox pictureBox_derya;

	public SatisRaporuGiris(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuar")
		{
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			base.FormBorderStyle = FormBorderStyle.None;
			base.WindowState = FormWindowState.Maximized;
			CenterToScreen();
			base.KeyPreview = true;
		}
		else
		{
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.StartPosition = FormStartPosition.CenterScreen;
		}
		if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuar")
		{
			pictureBox_derya.Visible = true;
			label_uyari.Visible = true;
			lc_kullanici_kodu.Text = "Firma kodu : ";
		}
	}

	private void Giris_Load(object sender, EventArgs e)
	{
	}

	private void te_giris_sifresi_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			GirisYap();
		}
	}

	private void GirisYap()
	{
		string user = te_kullaniciadi.Text;
		string toEncrypt = te_giris_sifresi.Text;
		_KullaniciParametreleri = ParametrelerDefault.MobilKullanici(user);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _KullaniciParametreleri, "akilli", user, "", "");
		bool flag = false;
		if (_KullaniciParametreleri._GetParametre("Sifre")._GetString == GenelUtilityWin.EncryptText("drjbq8777!#45", toEncrypt, useHashing: true))
		{
			flag = true;
		}
		te_kullaniciadi.Focus();
		if (!flag)
		{
			MessageBox.Show("Hatalı giriş. Tekrar deneyiniz.");
		}
		else
		{
			RaporAc();
		}
	}

	private void RaporSecimiAc()
	{
		SatisRaporuSecim satisRaporuSecim = new SatisRaporuSecim(_mikrouygulamabilgileri, _KullaniciParametreleri);
		if (satisRaporuSecim.ShowDialog() == DialogResult.OK)
		{
			Parametreler parametreler = ParametrelerDefault.MobilRaporStokSatis(satisRaporuSecim._secili_rapor);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "MobilRaporStokSatis", satisRaporuSecim._secili_rapor, "", "");
			RaporStokSatisSecenekleri raporsecenekleri = JsonConvert.DeserializeObject<RaporStokSatisSecenekleri>(parametreler._GetParametre("RaporJson")._GetString);
			RaporOlustur(raporsecenekleri, satisRaporuSecim._secili_rapor);
		}
	}

	private void RaporAc()
	{
		te_kullaniciadi.Text = "";
		te_giris_sifresi.Text = "";
		te_kullaniciadi.Focus();
		string getString = _KullaniciParametreleri._GetParametre("RaporStokSatisAlabilecegiRaporlar")._GetString;
		if (getString != "")
		{
			if (getString.Contains(","))
			{
				RaporSecimiAc();
				return;
			}
			Parametreler parametreler = ParametrelerDefault.MobilRaporStokSatis(getString);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "MobilRaporStokSatis", getString, "", "");
			RaporStokSatisSecenekleri raporsecenekleri = JsonConvert.DeserializeObject<RaporStokSatisSecenekleri>(parametreler._GetParametre("RaporJson")._GetString);
			RaporOlustur(raporsecenekleri, getString);
		}
		else
		{
			RaporStokSatisSecenekleri raporsecenekleri2 = new RaporStokSatisSecenekleri();
			RaporOlustur(raporsecenekleri2, "");
		}
	}

	private void RaporOlustur(RaporStokSatisSecenekleri raporsecenekleri, string secili_rapor_kod)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		RaporStokSatisSonuc raporStokSatisSonuc = new RaporStokSatisSonuc
		{
			gruplandirma_secenegi = raporsecenekleri.gruplandirma_secenegi,
			miktar1_secenek = raporsecenekleri.miktar1_secenek,
			miktar2_secenek = raporsecenekleri.miktar2_secenek,
			miktar3_secenek = raporsecenekleri.miktar3_secenek,
			tutar1_secenek = raporsecenekleri.tutar1_secenek,
			tutar2_secenek = raporsecenekleri.tutar2_secenek,
			tutar3_secenek = raporsecenekleri.tutar3_secenek,
			siralama_secenegi = raporsecenekleri.siralama_secenegi
		};
		RaporStokSatisSonucHam rapor = RaporStokSatisData.GetRapor(sqlDB.Connection, raporsecenekleri, _KullaniciParametreleri._GetParametre("DepoNo")._GetString, _KullaniciParametreleri._GetParametre("ProjeKodu")._GetString, _KullaniciParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString, _KullaniciParametreleri._GetParametre("CariPersonelKodu")._GetString, _KullaniciParametreleri._GetParametre("StokKodu")._GetString, _KullaniciParametreleri._GetParametre("StokAnaGrupKodu")._GetString, _KullaniciParametreleri._GetParametre("StokUreticiKodu")._GetString, _KullaniciParametreleri._GetParametre("StokMarkaKodu")._GetString, _KullaniciParametreleri._GetParametre("StokReyonKodu")._GetString, _KullaniciParametreleri._GetParametre("StokKategoriKodu")._GetString, _KullaniciParametreleri._GetParametre("CariKodu")._GetString, _KullaniciParametreleri._GetParametre("CariBolgeKodu")._GetString, _KullaniciParametreleri._GetParametre("CariGrupKodu")._GetString, _KullaniciParametreleri._GetParametre("RaporStokSatisMaliyetHesaplamaSekli")._GetInt);
		sqlDB.ConnectionClose();
		raporStokSatisSonuc.list_items = new List<RaporStokSatisListItem>();
		raporStokSatisSonuc.list_items = RaporStokSatisHelper.RaporGruplandir(raporStokSatisSonuc, rapor);
		SatisRaporu satisRaporu = new SatisRaporu(_mikrouygulamabilgileri, raporStokSatisSonuc, secili_rapor_kod);
		satisRaporu.ShowDialog();
		satisRaporu.BringToFront();
	}

	private void sb_giris_Click_1(object sender, EventArgs e)
	{
		GirisYap();
	}

	private void SatisRaporuGiris_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuar")
		{
			e.Cancel = true;
		}
	}

	private void SatisRaporuGiris_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.D && Control.ModifierKeys.HasFlag(Keys.Control) && Control.ModifierKeys.HasFlag(Keys.Alt) && Control.ModifierKeys.HasFlag(Keys.Shift))
		{
			base.FormClosing -= SatisRaporuGiris_FormClosing;
			Close();
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
		this.lc_kullanici_kodu = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.te_giris_sifresi = new DevExpress.XtraEditors.TextEdit();
		this.te_kullaniciadi = new DevExpress.XtraEditors.TextEdit();
		this.sb_giris = new DevExpress.XtraEditors.SimpleButton();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.label_uyari = new System.Windows.Forms.Label();
		this.pictureBox_derya = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.te_giris_sifresi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullaniciadi.Properties).BeginInit();
		this.tableLayoutPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox_derya).BeginInit();
		base.SuspendLayout();
		this.lc_kullanici_kodu.Appearance.Font = new System.Drawing.Font("Tahoma", 30f);
		this.lc_kullanici_kodu.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.lc_kullanici_kodu.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_kullanici_kodu.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lc_kullanici_kodu.Location = new System.Drawing.Point(3, 3);
		this.lc_kullanici_kodu.Name = "lc_kullanici_kodu";
		this.lc_kullanici_kodu.Size = new System.Drawing.Size(301, 52);
		this.lc_kullanici_kodu.TabIndex = 7;
		this.lc_kullanici_kodu.Text = "Kullanici kodu :";
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 30f);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Dock = System.Windows.Forms.DockStyle.Fill;
		this.labelControl5.Location = new System.Drawing.Point(3, 72);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(301, 51);
		this.labelControl5.TabIndex = 9;
		this.labelControl5.Text = "Şifre :";
		this.te_giris_sifresi.Dock = System.Windows.Forms.DockStyle.Fill;
		this.te_giris_sifresi.EditValue = "";
		this.te_giris_sifresi.Location = new System.Drawing.Point(310, 72);
		this.te_giris_sifresi.Name = "te_giris_sifresi";
		this.te_giris_sifresi.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 30f);
		this.te_giris_sifresi.Properties.Appearance.Options.UseFont = true;
		this.te_giris_sifresi.Properties.UseSystemPasswordChar = true;
		this.te_giris_sifresi.Size = new System.Drawing.Size(294, 54);
		this.te_giris_sifresi.TabIndex = 2;
		this.te_giris_sifresi.KeyDown += new System.Windows.Forms.KeyEventHandler(te_giris_sifresi_KeyDown);
		this.te_kullaniciadi.Dock = System.Windows.Forms.DockStyle.Fill;
		this.te_kullaniciadi.EditValue = "";
		this.te_kullaniciadi.EnterMoveNextControl = true;
		this.te_kullaniciadi.Location = new System.Drawing.Point(310, 3);
		this.te_kullaniciadi.Name = "te_kullaniciadi";
		this.te_kullaniciadi.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 30f);
		this.te_kullaniciadi.Properties.Appearance.Options.UseFont = true;
		this.te_kullaniciadi.Size = new System.Drawing.Size(294, 54);
		this.te_kullaniciadi.TabIndex = 1;
		this.sb_giris.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_giris.Appearance.Options.UseFont = true;
		this.sb_giris.Dock = System.Windows.Forms.DockStyle.Top;
		this.sb_giris.Location = new System.Drawing.Point(310, 146);
		this.sb_giris.Name = "sb_giris";
		this.sb_giris.Size = new System.Drawing.Size(294, 87);
		this.sb_giris.TabIndex = 10;
		this.sb_giris.Text = "Giriş";
		this.sb_giris.Click += new System.EventHandler(sb_giris_Click_1);
		this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.tableLayoutPanel1.ColumnCount = 2;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.57661f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.42339f));
		this.tableLayoutPanel1.Controls.Add(this.lc_kullanici_kodu, 0, 0);
		this.tableLayoutPanel1.Controls.Add(this.te_kullaniciadi, 1, 0);
		this.tableLayoutPanel1.Controls.Add(this.te_giris_sifresi, 1, 2);
		this.tableLayoutPanel1.Controls.Add(this.labelControl5, 0, 2);
		this.tableLayoutPanel1.Controls.Add(this.sb_giris, 1, 4);
		this.tableLayoutPanel1.Controls.Add(this.label_uyari, 1, 5);
		this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 183);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 6;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 84.05797f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.94203f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 93f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(607, 333);
		this.tableLayoutPanel1.TabIndex = 11;
		this.label_uyari.AutoSize = true;
		this.label_uyari.Dock = System.Windows.Forms.DockStyle.Fill;
		this.label_uyari.Font = new System.Drawing.Font("Tahoma", 20f);
		this.label_uyari.Location = new System.Drawing.Point(310, 236);
		this.label_uyari.Name = "label_uyari";
		this.label_uyari.Size = new System.Drawing.Size(294, 97);
		this.label_uyari.TabIndex = 11;
		this.label_uyari.Text = "Lütfen firma kodunuzu girin veya barkod okutunuz.";
		this.label_uyari.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label_uyari.Visible = false;
		this.pictureBox_derya.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.pictureBox_derya.Image = Fora.App.Win.Mikro.Properties.Resources.derya_dagitim_logo;
		this.pictureBox_derya.Location = new System.Drawing.Point(319, 35);
		this.pictureBox_derya.Name = "pictureBox_derya";
		this.pictureBox_derya.Size = new System.Drawing.Size(300, 142);
		this.pictureBox_derya.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.pictureBox_derya.TabIndex = 12;
		this.pictureBox_derya.TabStop = false;
		this.pictureBox_derya.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(703, 528);
		base.Controls.Add(this.pictureBox_derya);
		base.Controls.Add(this.tableLayoutPanel1);
		base.Name = "SatisRaporuGiris";
		this.Text = "Satış raporu girişi";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(SatisRaporuGiris_FormClosing);
		base.Load += new System.EventHandler(Giris_Load);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(SatisRaporuGiris_KeyDown);
		((System.ComponentModel.ISupportInitialize)this.te_giris_sifresi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullaniciadi.Properties).EndInit();
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox_derya).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
