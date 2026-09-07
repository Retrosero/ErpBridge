using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;

namespace Fora.App.Win.Mikro.Ayarlar;

public class KullaniciEkle : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public string _KoyalanacakKullanici = "YOK";

	public string _KullaniciAdi = "";

	public string _Sifre = "";

	public int _MikroKullaniciNo;

	private IContainer components;

	private SimpleButton sb_kullanici_ekle;

	private TextEdit te_sifre;

	private LabelControl labelControl3;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private LabelControl labelControl2;

	private ListBoxControl lb_kullanicilar;

	private LabelControl labelControl4;

	private System.Windows.Forms.ComboBox MikroUserNo;

	public KullaniciEkle(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		DataTable kullanicilarDataTable = MikroKullaniciData.GetKullanicilarDataTable(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName);
		MikroUserNo.DataSource = kullanicilarDataTable;
		MikroUserNo.ValueMember = "KULLANICI NO";
		MikroUserNo.DisplayMember = "KULLANICI ADI";
		MikroUserNo.SelectedValue = 1;
	}

	private void sb_kullanici_ekle_Click(object sender, EventArgs e)
	{
		bool flag = false;
		foreach (string item in ForaKullaniciData.GetKullaniciAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
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
			_MikroKullaniciNo = (int)MikroUserNo.SelectedValue;
			base.DialogResult = DialogResult.OK;
			Close();
		}
		else
		{
			MessageBox.Show("Bu isimde bir kullanıcı sistemde kayıtlı. Lütfen başka bir kullanıcı adı seçiniz.");
		}
	}

	private void KullaniciEkle_Load(object sender, EventArgs e)
	{
		lb_kullanicilar.Items.Clear();
		List<string> kullaniciAdlari = ForaKullaniciData.GetKullaniciAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
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
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.lb_kullanicilar = new DevExpress.XtraEditors.ListBoxControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.MikroUserNo = new System.Windows.Forms.ComboBox();
		((System.ComponentModel.ISupportInitialize)this.te_sifre.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).BeginInit();
		base.SuspendLayout();
		this.sb_kullanici_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 10.5f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_ekle.Appearance.Options.UseFont = true;
		this.sb_kullanici_ekle.Location = new System.Drawing.Point(290, 156);
		this.sb_kullanici_ekle.Name = "sb_kullanici_ekle";
		this.sb_kullanici_ekle.Size = new System.Drawing.Size(138, 28);
		this.sb_kullanici_ekle.TabIndex = 0;
		this.sb_kullanici_ekle.Text = "KULLANICI EKLE";
		this.sb_kullanici_ekle.Click += new System.EventHandler(sb_kullanici_ekle_Click);
		this.te_sifre.Location = new System.Drawing.Point(290, 104);
		this.te_sifre.Name = "te_sifre";
		this.te_sifre.Properties.UseSystemPasswordChar = true;
		this.te_sifre.Size = new System.Drawing.Size(138, 20);
		this.te_sifre.TabIndex = 9;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(167, 107);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(117, 13);
		this.labelControl3.TabIndex = 8;
		this.labelControl3.Text = "Şifre :";
		this.te_kullanici_adi.Location = new System.Drawing.Point(290, 78);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Size = new System.Drawing.Size(138, 20);
		this.te_kullanici_adi.TabIndex = 7;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(167, 81);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 6;
		this.labelControl1.Text = "Kullanıcı adı :";
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(167, 133);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(117, 13);
		this.labelControl2.TabIndex = 10;
		this.labelControl2.Text = "Bağlı mikro kullanıcısı :";
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
		this.MikroUserNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.MikroUserNo.FormattingEnabled = true;
		this.MikroUserNo.Location = new System.Drawing.Point(290, 129);
		this.MikroUserNo.Name = "MikroUserNo";
		this.MikroUserNo.Size = new System.Drawing.Size(138, 21);
		this.MikroUserNo.TabIndex = 155;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(440, 206);
		base.Controls.Add(this.MikroUserNo);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.lb_kullanicilar);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.te_sifre);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.te_kullanici_adi);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.sb_kullanici_ekle);
		base.Name = "KullaniciEkle";
		this.Text = "Kullanıcı Ekleme";
		base.Load += new System.EventHandler(KullaniciEkle_Load);
		((System.ComponentModel.ISupportInitialize)this.te_sifre.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lb_kullanicilar).EndInit();
		base.ResumeLayout(false);
	}
}
