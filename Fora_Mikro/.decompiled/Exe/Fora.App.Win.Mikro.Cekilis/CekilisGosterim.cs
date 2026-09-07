using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.App.Mikro.Cekilis;
using Fora.App.Win.Mikro.Properties;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Cekilis;

public class CekilisGosterim : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _KullaniciParametreleri;

	public List<CekilisKazananlar> kazananlar;

	public List<int> KuponKodlari;

	public int AktifCekilis;

	private IContainer components;

	private LabelControl label_hediye_adi;

	private SimpleButton sb_cekilise_basla;

	private PictureBox pictureBox1;

	private LabelControl label_numarator;

	private LabelControl label_cari_unvan1;

	private LabelControl label_cari_unvan2;

	private SimpleButton sb_sonraki;

	public CekilisGosterim(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.None;
		base.WindowState = FormWindowState.Maximized;
		CenterToScreen();
		AktifCekilis = -1;
		label_hediye_adi.Text = "";
		label_numarator.Text = "";
		label_cari_unvan1.Text = "";
		label_cari_unvan2.Text = "";
		sb_cekilise_basla.Enabled = false;
		UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
	}

	private void Giris_Load(object sender, EventArgs e)
	{
		kazananlar = new List<CekilisKazananlar>();
		KuponKodlari = new List<int>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		SqlDataReader sqlDataReader = new SqlCommand("SELECT hediye.HediyeKodu,hediye.HediyeAdi,kazananlar.Kazanan_kupon_kodu,cari.cari_unvan1,cari.cari_unvan2 FROM _FORA_CEKILIS_KAZANANLAR AS kazananlar INNER JOIN _FORA_KUPON_KODLARI AS kupon ON kazananlar.Kazanan_kupon_kodu=kupon.kupon_kodu INNER JOIN CARI_HESAPLAR AS cari ON kupon.cari_kod=cari.cari_kod INNER JOIN _FORA_CEKILIS_HEDIYELER AS hediye ON kazananlar.HediyeKodu=hediye.HediyeKodu ORDER BY hediye.CekilisSirasi", sqlDB.Connection).ExecuteReader();
		while (sqlDataReader.Read())
		{
			CekilisKazananlar cekilisKazananlar = new CekilisKazananlar();
			cekilisKazananlar.HediyeKodu = sqlDataReader.GetSafeString(0);
			cekilisKazananlar.HediyeAdi = sqlDataReader.GetSafeString(1);
			cekilisKazananlar.KazananKuponKodu = sqlDataReader.GetSafeInt32(2);
			cekilisKazananlar.KazananCariUnvan1 = sqlDataReader.GetSafeString(3);
			cekilisKazananlar.KazananCariUnvan2 = sqlDataReader.GetSafeString(4);
			kazananlar.Add(cekilisKazananlar);
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlDataReader = new SqlCommand("SELECT kupon_kodu FROM _FORA_KUPON_KODLARI", sqlDB.Connection).ExecuteReader();
		while (sqlDataReader.Read())
		{
			KuponKodlari.Add(sqlDataReader.GetSafeInt32(0));
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlDB.ConnectionClose();
		SonrakiCekilisHazirla();
	}

	private void sb_sonraki_Click(object sender, EventArgs e)
	{
		SonrakiCekilisHazirla();
	}

	private void sb_cekilise_basla_Click(object sender, EventArgs e)
	{
		sb_cekilise_basla.Enabled = false;
		Stopwatch stopwatch = new Stopwatch();
		List<int> list = new List<int>();
		list.Add(6000);
		list.Add(4000);
		list.Add(3000);
		list.Add(2500);
		list.Add(2000);
		list.Add(1500);
		list.Add(1000);
		List<int> list2 = new List<int>();
		list2.Add(10);
		list2.Add(20);
		list2.Add(40);
		list2.Add(50);
		list2.Add(100);
		list2.Add(150);
		list2.Add(200);
		Random random = new Random((int)DateTime.Now.Ticks);
		int num = 0;
		stopwatch.Start();
		do
		{
			int index = random.Next(KuponKodlari.Count);
			label_numarator.Text = KuponKodlari[index].ToString();
			Application.DoEvents();
			Thread.Sleep(list2[num]);
			if (stopwatch.ElapsedMilliseconds > list[num])
			{
				num++;
				stopwatch.Restart();
			}
		}
		while (num + 1 <= list.Count);
		label_numarator.Text = kazananlar[AktifCekilis].KazananKuponKodu.ToString();
		Application.DoEvents();
		Thread.Sleep(1000);
		label_numarator.ForeColor = Color.Red;
		Application.DoEvents();
		Thread.Sleep(1000);
		label_cari_unvan1.Text = kazananlar[AktifCekilis].KazananCariUnvan1;
		label_cari_unvan2.Text = kazananlar[AktifCekilis].KazananCariUnvan2;
		Application.DoEvents();
		sb_cekilise_basla.Enabled = false;
		sb_sonraki.Enabled = true;
	}

	private void SonrakiCekilisHazirla()
	{
		AktifCekilis++;
		if (AktifCekilis >= kazananlar.Count)
		{
			label_hediye_adi.Text = "";
			label_numarator.Text = "Çekiliş tamamlanmıştır.";
			label_cari_unvan1.Text = "";
			label_cari_unvan2.Text = "";
			sb_sonraki.Enabled = false;
		}
		else
		{
			label_hediye_adi.Text = kazananlar[AktifCekilis].HediyeAdi;
			label_numarator.Text = "";
			label_cari_unvan1.Text = "";
			label_cari_unvan2.Text = "";
			label_numarator.ForeColor = Color.Black;
			sb_cekilise_basla.Enabled = true;
			sb_sonraki.Enabled = false;
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
		this.label_hediye_adi = new DevExpress.XtraEditors.LabelControl();
		this.sb_cekilise_basla = new DevExpress.XtraEditors.SimpleButton();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.label_numarator = new DevExpress.XtraEditors.LabelControl();
		this.label_cari_unvan1 = new DevExpress.XtraEditors.LabelControl();
		this.label_cari_unvan2 = new DevExpress.XtraEditors.LabelControl();
		this.sb_sonraki = new DevExpress.XtraEditors.SimpleButton();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.label_hediye_adi.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.label_hediye_adi.Appearance.Font = new System.Drawing.Font("Tahoma", 40f, System.Drawing.FontStyle.Bold);
		this.label_hediye_adi.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.label_hediye_adi.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.label_hediye_adi.Location = new System.Drawing.Point(38, 195);
		this.label_hediye_adi.Name = "label_hediye_adi";
		this.label_hediye_adi.Size = new System.Drawing.Size(1188, 74);
		this.label_hediye_adi.TabIndex = 7;
		this.label_hediye_adi.Text = "HEDİYE ADI";
		this.sb_cekilise_basla.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.sb_cekilise_basla.Appearance.Font = new System.Drawing.Font("Tahoma", 50f, System.Drawing.FontStyle.Bold);
		this.sb_cekilise_basla.Appearance.Options.UseFont = true;
		this.sb_cekilise_basla.Location = new System.Drawing.Point(566, 603);
		this.sb_cekilise_basla.Name = "sb_cekilise_basla";
		this.sb_cekilise_basla.Size = new System.Drawing.Size(676, 101);
		this.sb_cekilise_basla.TabIndex = 10;
		this.sb_cekilise_basla.Text = "ÇEKİLİŞE BAŞLA";
		this.sb_cekilise_basla.Click += new System.EventHandler(sb_cekilise_basla_Click);
		this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.pictureBox1.Image = Fora.App.Win.Mikro.Properties.Resources.derya_dagitim_logo;
		this.pictureBox1.Location = new System.Drawing.Point(482, 49);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(300, 142);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.pictureBox1.TabIndex = 13;
		this.pictureBox1.TabStop = false;
		this.label_numarator.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.label_numarator.Appearance.Font = new System.Drawing.Font("Tahoma", 78f, System.Drawing.FontStyle.Bold);
		this.label_numarator.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.label_numarator.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.label_numarator.Location = new System.Drawing.Point(12, 273);
		this.label_numarator.Name = "label_numarator";
		this.label_numarator.Size = new System.Drawing.Size(1240, 133);
		this.label_numarator.TabIndex = 14;
		this.label_numarator.Text = "9999";
		this.label_cari_unvan1.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.label_cari_unvan1.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.label_cari_unvan1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.label_cari_unvan1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.label_cari_unvan1.Location = new System.Drawing.Point(2, 415);
		this.label_cari_unvan1.Name = "label_cari_unvan1";
		this.label_cari_unvan1.Size = new System.Drawing.Size(1259, 74);
		this.label_cari_unvan1.TabIndex = 15;
		this.label_cari_unvan1.Text = "KAZANAN MÜŞTERİ ÜNVAN 1";
		this.label_cari_unvan2.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.label_cari_unvan2.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.label_cari_unvan2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.label_cari_unvan2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.label_cari_unvan2.Location = new System.Drawing.Point(2, 495);
		this.label_cari_unvan2.Name = "label_cari_unvan2";
		this.label_cari_unvan2.Size = new System.Drawing.Size(1259, 74);
		this.label_cari_unvan2.TabIndex = 16;
		this.label_cari_unvan2.Text = "KAZANAN MÜŞTERİ ÜNVAN 2";
		this.sb_sonraki.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.sb_sonraki.Appearance.Font = new System.Drawing.Font("Tahoma", 50f, System.Drawing.FontStyle.Bold);
		this.sb_sonraki.Appearance.Options.UseFont = true;
		this.sb_sonraki.Location = new System.Drawing.Point(12, 603);
		this.sb_sonraki.Name = "sb_sonraki";
		this.sb_sonraki.Size = new System.Drawing.Size(370, 101);
		this.sb_sonraki.TabIndex = 17;
		this.sb_sonraki.Text = "SONRAKİ";
		this.sb_sonraki.Click += new System.EventHandler(sb_sonraki_Click);
		base.Appearance.BackColor = System.Drawing.Color.White;
		base.Appearance.Options.UseBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1264, 730);
		base.Controls.Add(this.sb_sonraki);
		base.Controls.Add(this.label_cari_unvan2);
		base.Controls.Add(this.label_cari_unvan1);
		base.Controls.Add(this.label_numarator);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.sb_cekilise_basla);
		base.Controls.Add(this.label_hediye_adi);
		base.Name = "CekilisGosterim";
		base.Load += new System.EventHandler(Giris_Load);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
