using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.App.Win.Mikro.Properties;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Cekilis;

public class CekilisGiris : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _KullaniciParametreleri;

	private IContainer components;

	private TextEdit te_kullaniciadi;

	private SimpleButton sb_yeni_giris;

	private LabelControl labelControl9;

	private PictureBox pictureBox1;

	public CekilisGiris(MikroUygulamaBilgileri mikrouygulamabilgileri)
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
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_KUPON_KODLARI]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[_FORA_KUPON_KODLARI]([RECno] [int] IDENTITY(1,1) NOT NULL,[cari_kod] [nvarchar](50) NOT NULL,[kupon_kodu] [int] NOT NULL,[tarih] [datetime] NOT NULL,CONSTRAINT [PK__FORA_KUPON_KODLARI] PRIMARY KEY CLUSTERED ([RECno] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] END";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.ExecuteNonQuery();
		}
		sqlDB.ConnectionClose();
	}

	private void sb_yeni_giris_Click(object sender, EventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		Cari cariByCariKod = CariData.GetCariByCariKod(sqlDB.Connection, te_kullaniciadi.Text, AdreslerTemsilciyeGore: false, "");
		sqlDB.ConnectionClose();
		if (cariByCariKod.cari_kod == "")
		{
			MessageBox.Show("Hesap bulunamadı.");
		}
		else
		{
			new CekilisKuponVerme(_mikrouygulamabilgileri, cariByCariKod).ShowDialog();
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
		this.te_kullaniciadi = new DevExpress.XtraEditors.TextEdit();
		this.sb_yeni_giris = new DevExpress.XtraEditors.SimpleButton();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.te_kullaniciadi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.te_kullaniciadi.EditValue = "";
		this.te_kullaniciadi.EnterMoveNextControl = true;
		this.te_kullaniciadi.Location = new System.Drawing.Point(264, 210);
		this.te_kullaniciadi.Name = "te_kullaniciadi";
		this.te_kullaniciadi.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 30f);
		this.te_kullaniciadi.Properties.Appearance.Options.UseFont = true;
		this.te_kullaniciadi.Size = new System.Drawing.Size(300, 54);
		this.te_kullaniciadi.TabIndex = 1;
		this.sb_yeni_giris.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.sb_yeni_giris.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_yeni_giris.Appearance.Options.UseFont = true;
		this.sb_yeni_giris.Location = new System.Drawing.Point(264, 270);
		this.sb_yeni_giris.Name = "sb_yeni_giris";
		this.sb_yeni_giris.Size = new System.Drawing.Size(300, 73);
		this.sb_yeni_giris.TabIndex = 11;
		this.sb_yeni_giris.Text = "Giriş";
		this.sb_yeni_giris.Click += new System.EventHandler(sb_yeni_giris_Click);
		this.labelControl9.Appearance.Font = new System.Drawing.Font("Tahoma", 30f);
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.Location = new System.Drawing.Point(29, 213);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(229, 48);
		this.labelControl9.TabIndex = 26;
		this.labelControl9.Text = "Firma Kodu :";
		this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.pictureBox1.Image = Fora.App.Win.Mikro.Properties.Resources.derya_dagitim_logo;
		this.pictureBox1.Location = new System.Drawing.Point(264, 29);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(300, 142);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.pictureBox1.TabIndex = 27;
		this.pictureBox1.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(626, 413);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.labelControl9);
		base.Controls.Add(this.sb_yeni_giris);
		base.Controls.Add(this.te_kullaniciadi);
		base.Name = "CekilisGiris";
		this.Text = "Kupon kodu girişi";
		base.Load += new System.EventHandler(Giris_Load);
		((System.ComponentModel.ISupportInitialize)this.te_kullaniciadi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
