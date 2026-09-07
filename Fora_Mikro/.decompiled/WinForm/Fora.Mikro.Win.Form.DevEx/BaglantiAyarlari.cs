using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Win.Form.DevEx;

public class BaglantiAyarlari : XtraForm
{
	public SqlBaglantiBilgileri _BaglantiBilgileri;

	private string _TestDB;

	private IContainer components;

	private TextEdit textEdit_SqlServer;

	private TextEdit textEdit_SqlUserName;

	private TextEdit textEdit_SqlPassword;

	private LabelControl labelControl1;

	private LabelControl labelControl3;

	private LabelControl labelControl4;

	private LabelControl labelControl_TestSonuc;

	private SimpleButton simpleButton1;

	private SimpleButton sb_kaydet;

	private SimpleButton sb_baglanti_testi;

	public BaglantiAyarlari(string Baslik, string TestDB)
	{
		InitializeComponent();
		Text = Baslik;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		_TestDB = TestDB;
	}

	private void button_BaglantiTesti_Click(object sender, EventArgs e)
	{
		BaglantiTestiYap();
	}

	private void BaglantiAyarlari_Load(object sender, EventArgs e)
	{
		textEdit_SqlServer.Text = _BaglantiBilgileri.SqlServer;
		textEdit_SqlUserName.Text = _BaglantiBilgileri.SqlUserName;
		textEdit_SqlPassword.Text = _BaglantiBilgileri.SqlPassword;
	}

	private void BaglantiTestiYap()
	{
		if (textEdit_SqlServer.Text == "")
		{
			labelControl_TestSonuc.Text = "SQL Server Adresi Girilmek Zorunda!";
			labelControl_TestSonuc.ForeColor = Color.Red;
			return;
		}
		Cursor.Current = Cursors.WaitCursor;
		labelControl_TestSonuc.Text = "Test Ediliyor.";
		labelControl_TestSonuc.ForeColor = Color.Black;
		Refresh();
		try
		{
			SqlDB sqlDB = new SqlDB();
			SqlBaglantiBilgileri baglantiBilgileri = new SqlBaglantiBilgileri(textEdit_SqlServer.Text, textEdit_SqlUserName.Text, textEdit_SqlPassword.Text);
			sqlDB.ConnectionOpen(baglantiBilgileri, _TestDB);
			sqlDB.ConnectionClose();
			labelControl_TestSonuc.Text = "Sql bağlantısı sağlandı.";
			labelControl_TestSonuc.ForeColor = Color.Green;
		}
		catch
		{
			labelControl_TestSonuc.Text = "Sql bağlantısı sağlanamadı!";
			labelControl_TestSonuc.ForeColor = Color.Red;
		}
		Cursor.Current = Cursors.Default;
	}

	private void sb_kaydet_Click(object sender, EventArgs e)
	{
		if (textEdit_SqlServer.Text == "")
		{
			labelControl_TestSonuc.Text = "SQL Server Adresi Girilmek Zorunda!";
			labelControl_TestSonuc.ForeColor = Color.Red;
		}
		else
		{
			_BaglantiBilgileri.SqlServer = textEdit_SqlServer.Text;
			_BaglantiBilgileri.SqlUserName = textEdit_SqlUserName.Text;
			_BaglantiBilgileri.SqlPassword = textEdit_SqlPassword.Text;
		}
	}

	private void sb_baglanti_testi_Click(object sender, EventArgs e)
	{
		BaglantiTestiYap();
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
		this.textEdit_SqlServer = new DevExpress.XtraEditors.TextEdit();
		this.textEdit_SqlUserName = new DevExpress.XtraEditors.TextEdit();
		this.textEdit_SqlPassword = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl_TestSonuc = new DevExpress.XtraEditors.LabelControl();
		this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
		this.sb_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.sb_baglanti_testi = new DevExpress.XtraEditors.SimpleButton();
		((System.ComponentModel.ISupportInitialize)this.textEdit_SqlServer.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textEdit_SqlUserName.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textEdit_SqlPassword.Properties).BeginInit();
		base.SuspendLayout();
		this.textEdit_SqlServer.Location = new System.Drawing.Point(132, 11);
		this.textEdit_SqlServer.Name = "textEdit_SqlServer";
		this.textEdit_SqlServer.Size = new System.Drawing.Size(188, 20);
		this.textEdit_SqlServer.TabIndex = 1;
		this.textEdit_SqlUserName.Location = new System.Drawing.Point(132, 37);
		this.textEdit_SqlUserName.Name = "textEdit_SqlUserName";
		this.textEdit_SqlUserName.Size = new System.Drawing.Size(188, 20);
		this.textEdit_SqlUserName.TabIndex = 2;
		this.textEdit_SqlPassword.Location = new System.Drawing.Point(132, 63);
		this.textEdit_SqlPassword.Name = "textEdit_SqlPassword";
		this.textEdit_SqlPassword.Properties.UseSystemPasswordChar = true;
		this.textEdit_SqlPassword.Size = new System.Drawing.Size(188, 20);
		this.textEdit_SqlPassword.TabIndex = 3;
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Location = new System.Drawing.Point(45, 12);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(81, 16);
		this.labelControl1.TabIndex = 4;
		this.labelControl1.Text = "SQL Server :";
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Location = new System.Drawing.Point(18, 38);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(108, 16);
		this.labelControl3.TabIndex = 6;
		this.labelControl3.Text = "Sql Kullanıcı Adı :";
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl4.Location = new System.Drawing.Point(64, 64);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(62, 16);
		this.labelControl4.TabIndex = 7;
		this.labelControl4.Text = "Sql Şifre :";
		this.labelControl_TestSonuc.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl_TestSonuc.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl_TestSonuc.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl_TestSonuc.Location = new System.Drawing.Point(55, 148);
		this.labelControl_TestSonuc.Name = "labelControl_TestSonuc";
		this.labelControl_TestSonuc.Size = new System.Drawing.Size(265, 16);
		this.labelControl_TestSonuc.TabIndex = 11;
		this.simpleButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.simpleButton1.Location = new System.Drawing.Point(132, 89);
		this.simpleButton1.Name = "simpleButton1";
		this.simpleButton1.Size = new System.Drawing.Size(83, 23);
		this.simpleButton1.TabIndex = 5;
		this.simpleButton1.Text = "VAZGEÇ";
		this.sb_kaydet.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_kaydet.Location = new System.Drawing.Point(221, 89);
		this.sb_kaydet.Name = "sb_kaydet";
		this.sb_kaydet.Size = new System.Drawing.Size(99, 23);
		this.sb_kaydet.TabIndex = 4;
		this.sb_kaydet.Text = "KAYDET";
		this.sb_kaydet.Click += new System.EventHandler(sb_kaydet_Click);
		this.sb_baglanti_testi.Location = new System.Drawing.Point(132, 119);
		this.sb_baglanti_testi.Name = "sb_baglanti_testi";
		this.sb_baglanti_testi.Size = new System.Drawing.Size(188, 23);
		this.sb_baglanti_testi.TabIndex = 6;
		this.sb_baglanti_testi.Text = "BAĞLANTI TESTİ";
		this.sb_baglanti_testi.Click += new System.EventHandler(sb_baglanti_testi_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		base.ClientSize = new System.Drawing.Size(332, 178);
		base.Controls.Add(this.sb_baglanti_testi);
		base.Controls.Add(this.sb_kaydet);
		base.Controls.Add(this.simpleButton1);
		base.Controls.Add(this.labelControl_TestSonuc);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.textEdit_SqlPassword);
		base.Controls.Add(this.textEdit_SqlUserName);
		base.Controls.Add(this.textEdit_SqlServer);
		base.Name = "BaglantiAyarlari";
		this.Text = "Bağlantı Ayarları";
		base.Load += new System.EventHandler(BaglantiAyarlari_Load);
		((System.ComponentModel.ISupportInitialize)this.textEdit_SqlServer.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textEdit_SqlUserName.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textEdit_SqlPassword.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
