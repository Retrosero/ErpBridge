using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class TemsilciRaporuGiris : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private IContainer components;

	private LabelControl labelControl4;

	private LabelControl labelControl5;

	private SimpleButton sb_giris;

	private TextEdit te_giris_sifresi;

	private TextEdit te_kullaniciadi;

	public TemsilciRaporuGiris(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
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

	private void sb_giris_Click(object sender, EventArgs e)
	{
		GirisYap();
	}

	private void GirisYap()
	{
		string user = te_kullaniciadi.Text;
		string toEncrypt = te_giris_sifresi.Text;
		Parametreler parametreler = ParametrelerDefault.MobilKullanici(user);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "akilli", user, "", "");
		bool flag = false;
		if (parametreler._GetParametre("Sifre")._GetString == GenelUtilityWin.EncryptText("drjbq8777!#45", toEncrypt, useHashing: true))
		{
			flag = true;
		}
		if (!flag)
		{
			MessageBox.Show("Hatalı giriş. Tekrar deneyiniz.");
		}
		else
		{
			new TemsilciRaporuSecim(_mikrouygulamabilgileri, parametreler).ShowDialog();
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
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.sb_giris = new DevExpress.XtraEditors.SimpleButton();
		this.te_giris_sifresi = new DevExpress.XtraEditors.TextEdit();
		this.te_kullaniciadi = new DevExpress.XtraEditors.TextEdit();
		((System.ComponentModel.ISupportInitialize)this.te_giris_sifresi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullaniciadi.Properties).BeginInit();
		base.SuspendLayout();
		this.labelControl4.Location = new System.Drawing.Point(31, 47);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(87, 13);
		this.labelControl4.TabIndex = 7;
		this.labelControl4.Text = "Mobil kullanıcı adı :";
		this.labelControl5.Location = new System.Drawing.Point(60, 73);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(58, 13);
		this.labelControl5.TabIndex = 9;
		this.labelControl5.Text = "Giriş şifresi :";
		this.sb_giris.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_giris.Appearance.Options.UseFont = true;
		this.sb_giris.Location = new System.Drawing.Point(124, 96);
		this.sb_giris.Name = "sb_giris";
		this.sb_giris.Size = new System.Drawing.Size(129, 23);
		this.sb_giris.TabIndex = 3;
		this.sb_giris.Text = "Giriş";
		this.sb_giris.Click += new System.EventHandler(sb_giris_Click);
		this.te_giris_sifresi.EditValue = "";
		this.te_giris_sifresi.Location = new System.Drawing.Point(124, 70);
		this.te_giris_sifresi.Name = "te_giris_sifresi";
		this.te_giris_sifresi.Properties.UseSystemPasswordChar = true;
		this.te_giris_sifresi.Size = new System.Drawing.Size(129, 20);
		this.te_giris_sifresi.TabIndex = 2;
		this.te_giris_sifresi.KeyDown += new System.Windows.Forms.KeyEventHandler(te_giris_sifresi_KeyDown);
		this.te_kullaniciadi.EditValue = "";
		this.te_kullaniciadi.EnterMoveNextControl = true;
		this.te_kullaniciadi.Location = new System.Drawing.Point(124, 44);
		this.te_kullaniciadi.Name = "te_kullaniciadi";
		this.te_kullaniciadi.Size = new System.Drawing.Size(129, 20);
		this.te_kullaniciadi.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(289, 173);
		base.Controls.Add(this.te_kullaniciadi);
		base.Controls.Add(this.sb_giris);
		base.Controls.Add(this.te_giris_sifresi);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.labelControl4);
		base.Name = "TemsilciRaporuGiris";
		this.Text = "Temsilci Raporu - Giriş";
		base.Load += new System.EventHandler(Giris_Load);
		((System.ComponentModel.ISupportInitialize)this.te_giris_sifresi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_kullaniciadi.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
