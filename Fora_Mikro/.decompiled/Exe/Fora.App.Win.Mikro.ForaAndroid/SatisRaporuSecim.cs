using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class SatisRaporuSecim : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _kullaniciparametreleri;

	public string _secili_rapor;

	private IContainer components;

	private SimpleButton sb_raporu_olustur;

	private System.Windows.Forms.ComboBox cb_raporlar;

	public SatisRaporuSecim(MikroUygulamaBilgileri mikrouygulamabilgileri, Parametreler kullaniciparametreleri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_kullaniciparametreleri = kullaniciparametreleri;
		if (!Directory.Exists("data\\raporlar\\TemsilciRaporu"))
		{
			Directory.CreateDirectory("data\\raporlar\\TemsilciRaporu");
		}
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void TemsilciRaporuSecim_Load(object sender, EventArgs e)
	{
		_secili_rapor = "";
		RaporlariListele();
	}

	private void RaporlariListele()
	{
		cb_raporlar.Items.Clear();
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(string));
		dataTable.Columns.Add("Isim", typeof(string));
		string getString = _kullaniciparametreleri._GetParametre("RaporStokSatisAlabilecegiRaporlar")._GetString;
		if (getString != "")
		{
			try
			{
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
				foreach (GenelList alabilecegiRaporlar in RaporStokSatisData.GetAlabilecegiRaporlarList(sqlDB.Connection, getString))
				{
					dataTable.Rows.Add(alabilecegiRaporlar.Kod, alabilecegiRaporlar.Text);
				}
				sqlDB.ConnectionClose();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		cb_raporlar.DataSource = dataTable;
		cb_raporlar.ValueMember = "ID";
		cb_raporlar.DisplayMember = "Isim";
		if (cb_raporlar.Items.Count != 0)
		{
			cb_raporlar.SelectedIndex = 0;
		}
	}

	private void sb_raporu_olustur_Click(object sender, EventArgs e)
	{
		_secili_rapor = cb_raporlar.SelectedValue.ToString();
		base.DialogResult = DialogResult.OK;
		Close();
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
		this.sb_raporu_olustur = new DevExpress.XtraEditors.SimpleButton();
		this.cb_raporlar = new System.Windows.Forms.ComboBox();
		base.SuspendLayout();
		this.sb_raporu_olustur.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_raporu_olustur.Appearance.Options.UseFont = true;
		this.sb_raporu_olustur.Location = new System.Drawing.Point(42, 121);
		this.sb_raporu_olustur.Name = "sb_raporu_olustur";
		this.sb_raporu_olustur.Size = new System.Drawing.Size(756, 98);
		this.sb_raporu_olustur.TabIndex = 12;
		this.sb_raporu_olustur.Text = "Raporu oluştur";
		this.sb_raporu_olustur.Click += new System.EventHandler(sb_raporu_olustur_Click);
		this.cb_raporlar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_raporlar.Font = new System.Drawing.Font("Tahoma", 20f);
		this.cb_raporlar.FormattingEnabled = true;
		this.cb_raporlar.Location = new System.Drawing.Point(42, 56);
		this.cb_raporlar.Name = "cb_raporlar";
		this.cb_raporlar.Size = new System.Drawing.Size(756, 41);
		this.cb_raporlar.TabIndex = 13;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(831, 257);
		base.Controls.Add(this.cb_raporlar);
		base.Controls.Add(this.sb_raporu_olustur);
		base.Name = "SatisRaporuSecim";
		this.Text = "Rapor seçimi";
		base.Load += new System.EventHandler(TemsilciRaporuSecim_Load);
		base.ResumeLayout(false);
	}
}
