using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class ForaAndroidKullaniciEkle : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public string _KoyalanacakKullanici = "YOK";

	public string _KullaniciAdi = "";

	public string _Sifre = "";

	public string _CariPersonelKodu = "";

	public string _CariKodu = "";

	public string _Tanimli_Stok_Reyon_Kodu = "";

	public string _Stok_Secimi_Varsayilan_aktif_Grup = "";

	private IContainer components;

	private SimpleButton sb_kullanici_ekle;

	private TextEdit te_sifre;

	private LabelControl labelControl3;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private TextEdit te_cari_personel_kodu;

	private LabelControl labelControl2;

	private ListBoxControl lb_kullanicilar;

	private LabelControl labelControl4;

	private TextEdit te_tanimli_cari_kodu;

	private LabelControl labelControl5;

	private TextEdit te_tanimli_stok_reyon_kodu;

	private LabelControl labelControl6;

	private TextEdit te_stok_secimi_varsayilan_aktif_grup;

	private LabelControl labelControl7;

	public ForaAndroidKullaniciEkle(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void sb_kullanici_ekle_Click(object sender, EventArgs e)
	{
		bool flag = false;
		foreach (string item in AndroidKullaniciData.GetKullaniciAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			if (te_kullanici_adi.Text == item)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			_KoyalanacakKullanici = lb_kullanicilar.SelectedValue.ToString();
			_KullaniciAdi = te_kullanici_adi.Text;
			_Sifre = te_sifre.Text;
			_CariPersonelKodu = te_cari_personel_kodu.Text;
			_CariKodu = te_tanimli_cari_kodu.Text;
			_Tanimli_Stok_Reyon_Kodu = te_tanimli_stok_reyon_kodu.Text;
			_Stok_Secimi_Varsayilan_aktif_Grup = te_stok_secimi_varsayilan_aktif_grup.Text;
			base.DialogResult = DialogResult.OK;
			Close();
		}
		else
		{
			MessageBox.Show("Bu isimde bir kullanıcı sistemde kayıtlı. Lütfen başka bir kullanıcı adı seçiniz.");
		}
	}

	private void ForaAndroidKullaniciEkle_Load(object sender, EventArgs e)
	{
		lb_kullanicilar.Items.Clear();
		List<string> kullaniciAdlari = AndroidKullaniciData.GetKullaniciAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		lb_kullanicilar.Items.Add("YOK");
		foreach (string item in kullaniciAdlari)
		{
			lb_kullanicilar.Items.Add(item);
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
		this.sb_kullanici_ekle = new DevExpress.XtraEditors.SimpleButton();
		this.te_sifre = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.te_cari_personel_kodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.lb_kullanicilar = new DevExpress.XtraEditors.ListBoxControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.te_tanimli_cari_kodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.te_tanimli_stok_reyon_kodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.te_stok_secimi_varsayilan_aktif_grup = new DevExpress.XtraEditors.TextEdit();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.te_sifre.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_cari_personel_kodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tanimli_cari_kodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tanimli_stok_reyon_kodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_stok_secimi_varsayilan_aktif_grup.Properties).BeginInit();
		base.SuspendLayout();
		this.sb_kullanici_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 10.5f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_ekle.Appearance.Options.UseFont = true;
		this.sb_kullanici_ekle.Location = new System.Drawing.Point(351, 207);
		this.sb_kullanici_ekle.Name = "sb_kullanici_ekle";
		this.sb_kullanici_ekle.Size = new System.Drawing.Size(138, 28);
		this.sb_kullanici_ekle.TabIndex = 0;
		this.sb_kullanici_ekle.Text = "KULLANICI EKLE";
		this.sb_kullanici_ekle.Click += new System.EventHandler(sb_kullanici_ekle_Click);
		this.te_sifre.Location = new System.Drawing.Point(351, 66);
		this.te_sifre.Name = "te_sifre";
		this.te_sifre.Properties.UseSystemPasswordChar = true;
		this.te_sifre.Size = new System.Drawing.Size(138, 20);
		this.te_sifre.TabIndex = 9;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(195, 69);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(150, 13);
		this.labelControl3.TabIndex = 8;
		this.labelControl3.Text = "Şifre :";
		this.te_kullanici_adi.Location = new System.Drawing.Point(351, 40);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Size = new System.Drawing.Size(138, 20);
		this.te_kullanici_adi.TabIndex = 7;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(195, 43);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(150, 13);
		this.labelControl1.TabIndex = 6;
		this.labelControl1.Text = "Kullanıcı adı :";
		this.te_cari_personel_kodu.Location = new System.Drawing.Point(351, 92);
		this.te_cari_personel_kodu.Name = "te_cari_personel_kodu";
		this.te_cari_personel_kodu.Size = new System.Drawing.Size(138, 20);
		this.te_cari_personel_kodu.TabIndex = 11;
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(195, 95);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(150, 13);
		this.labelControl2.TabIndex = 10;
		this.labelControl2.Text = "Cari personel kodu :";
		this.lb_kullanicilar.Location = new System.Drawing.Point(17, 44);
		this.lb_kullanicilar.Name = "lb_kullanicilar";
		this.lb_kullanicilar.Size = new System.Drawing.Size(144, 140);
		this.lb_kullanicilar.TabIndex = 12;
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(17, 25);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(144, 13);
		this.labelControl4.TabIndex = 13;
		this.labelControl4.Text = "Kopyalanacak kullanıcı";
		this.te_tanimli_cari_kodu.Location = new System.Drawing.Point(351, 118);
		this.te_tanimli_cari_kodu.Name = "te_tanimli_cari_kodu";
		this.te_tanimli_cari_kodu.Size = new System.Drawing.Size(138, 20);
		this.te_tanimli_cari_kodu.TabIndex = 15;
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(195, 121);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(150, 13);
		this.labelControl5.TabIndex = 14;
		this.labelControl5.Text = "Tanımlı cari kodu :";
		this.te_tanimli_stok_reyon_kodu.Location = new System.Drawing.Point(351, 144);
		this.te_tanimli_stok_reyon_kodu.Name = "te_tanimli_stok_reyon_kodu";
		this.te_tanimli_stok_reyon_kodu.Size = new System.Drawing.Size(138, 20);
		this.te_tanimli_stok_reyon_kodu.TabIndex = 17;
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(195, 147);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(150, 13);
		this.labelControl6.TabIndex = 16;
		this.labelControl6.Text = "Tanımlı stok reyon kodu :";
		this.te_stok_secimi_varsayilan_aktif_grup.Location = new System.Drawing.Point(351, 170);
		this.te_stok_secimi_varsayilan_aktif_grup.Name = "te_stok_secimi_varsayilan_aktif_grup";
		this.te_stok_secimi_varsayilan_aktif_grup.Size = new System.Drawing.Size(138, 20);
		this.te_stok_secimi_varsayilan_aktif_grup.TabIndex = 19;
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(167, 173);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(178, 13);
		this.labelControl7.TabIndex = 18;
		this.labelControl7.Text = "Stok seçimi varsayılan aktif grup :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(501, 248);
		base.Controls.Add(this.te_stok_secimi_varsayilan_aktif_grup);
		base.Controls.Add(this.labelControl7);
		base.Controls.Add(this.te_tanimli_stok_reyon_kodu);
		base.Controls.Add(this.labelControl6);
		base.Controls.Add(this.te_tanimli_cari_kodu);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.lb_kullanicilar);
		base.Controls.Add(this.te_cari_personel_kodu);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.te_sifre);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.te_kullanici_adi);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.sb_kullanici_ekle);
		base.Name = "ForaAndroidKullaniciEkle";
		this.Text = "Fora Android Kullanıcı Ekleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciEkle_Load);
		((System.ComponentModel.ISupportInitialize)this.te_sifre.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_cari_personel_kodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tanimli_cari_kodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tanimli_stok_reyon_kodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_stok_secimi_varsayilan_aktif_grup.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
