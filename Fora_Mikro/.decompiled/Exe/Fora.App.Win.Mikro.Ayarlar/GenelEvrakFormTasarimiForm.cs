using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Siparis;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Win.Form.DevEx.Raporlar;

namespace Fora.App.Win.Mikro.Ayarlar;

public class GenelEvrakFormTasarimiForm : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private IContainer components;

	private SimpleButton sb_form_tasarimini_ac;

	private TextEdit te_seri;

	private LabelControl labelControl17;

	private System.Windows.Forms.ComboBox EvrakTipi;

	private LabelControl labelControl1;

	private LabelControl labelControl2;

	private TextEdit te_sira;

	private LabelControl labelControl3;

	private LabelControl labelControl4;

	private TextEdit te_dosya;

	private SimpleButton sb_dosya_sec;

	private LabelControl labelControl5;

	public GenelEvrakFormTasarimiForm(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		EvrakTipi.SelectedIndex = 0;
	}

	private void sb_form_tasarimini_ac_Click(object sender, EventArgs e)
	{
		string text = EvrakTipi.SelectedItem.ToString();
		string evrakSeri = te_seri.Text;
		string s = te_sira.Text;
		GenelEvrak genelEvrak = new GenelEvrak();
		enum_GenelEvrakTipleri evrakTipi = enum_GenelEvrakTipleri.SatisFaturasi;
		switch (text)
		{
		case "Satış faturası":
			evrakTipi = enum_GenelEvrakTipleri.SatisFaturasi;
			break;
		case "Satış irsaliyesi":
			evrakTipi = enum_GenelEvrakTipleri.SatisIrsaliyesi;
			break;
		case "Alınan sipariş":
			evrakTipi = enum_GenelEvrakTipleri.AlinanSiparis;
			break;
		case "Proforma sipariş":
			evrakTipi = enum_GenelEvrakTipleri.ProformaSiparis;
			break;
		case "Tahsilat":
			evrakTipi = enum_GenelEvrakTipleri.Tahsilat;
			break;
		case "Tediye":
			evrakTipi = enum_GenelEvrakTipleri.Tediye;
			break;
		case "Alış faturası":
			evrakTipi = enum_GenelEvrakTipleri.AlisFaturasi;
			break;
		case "Alış irsaliyesi":
			evrakTipi = enum_GenelEvrakTipleri.AlisIrsaliyesi;
			break;
		case "Depolar arası nakliye fişi":
			evrakTipi = enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi;
			break;
		case "Depolar arası sevk":
			evrakTipi = enum_GenelEvrakTipleri.DepolarArasiSevk;
			break;
		case "Depolar arası sipariş":
			evrakTipi = enum_GenelEvrakTipleri.DepolarArasiSiparis;
			break;
		case "Gelen havale":
			evrakTipi = enum_GenelEvrakTipleri.GelenHavale;
			break;
		case "Giden havale":
			evrakTipi = enum_GenelEvrakTipleri.GidenHavale;
			break;
		}
		SqlConnection sqlConnection = new SqlConnection();
		if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
		{
			sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Trusted_Connection=true;";
		}
		else
		{
			sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; User Id=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + ";Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + ";";
		}
		sqlConnection.Open();
		int dB_alternatif_doviz = _mikrouygulamabilgileri.veritabani.DB_alternatif_doviz;
		enum_sth_cins sth_cins = enum_sth_cins.Toptan;
		if (!(text == "Depolar arası sevk"))
		{
			if (text == "Depolar arası nakliye")
			{
				sth_cins = enum_sth_cins.Transfer;
			}
		}
		else
		{
			sth_cins = enum_sth_cins.Transfer;
		}
		Evrak evrak = EvrakData.GetEvrak(sqlConnection, evrakSeri, int.Parse(s), evrakTipi, dB_alternatif_doviz, enum_sip_cins.NormalSiparis, sth_cins);
		genelEvrak = GenelEvrakData.ConvertEvrak(sqlConnection, evrak, enum_satir_gruplandirma_secenekleri.StokKodu);
		sqlConnection.Close();
		RaporGenelEvrak raporGenelEvrak = new RaporGenelEvrak();
		List<GenelEvrak> list = new List<GenelEvrak>();
		list.Add(genelEvrak);
		raporGenelEvrak.DataSource = list;
		if (te_dosya.Text != "" && File.Exists(te_dosya.Text))
		{
			raporGenelEvrak.LoadLayout(te_dosya.Text);
		}
		ReportDesignTool reportDesignTool = new ReportDesignTool(raporGenelEvrak);
		try
		{
			raporGenelEvrak.DisplayName = "genelevrak";
			reportDesignTool.ShowDesignerDialog();
		}
		catch
		{
			MessageBox.Show("Seçili raporun tipi uygun değil. Lütfen uygun bir rapor seçiniz.");
		}
	}

	private void sb_dosya_sec_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.FileName = "genelevrak.repx";
		saveFileDialog.Filter = "Rapor dosyası|*.repx|Tüm dosyalar|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			te_dosya.Text = saveFileDialog.FileName;
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
		this.sb_form_tasarimini_ac = new DevExpress.XtraEditors.SimpleButton();
		this.te_seri = new DevExpress.XtraEditors.TextEdit();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		this.EvrakTipi = new System.Windows.Forms.ComboBox();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.te_sira = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.te_dosya = new DevExpress.XtraEditors.TextEdit();
		this.sb_dosya_sec = new DevExpress.XtraEditors.SimpleButton();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.te_seri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_sira.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_dosya.Properties).BeginInit();
		base.SuspendLayout();
		this.sb_form_tasarimini_ac.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_form_tasarimini_ac.Appearance.Options.UseFont = true;
		this.sb_form_tasarimini_ac.Location = new System.Drawing.Point(123, 187);
		this.sb_form_tasarimini_ac.Name = "sb_form_tasarimini_ac";
		this.sb_form_tasarimini_ac.Size = new System.Drawing.Size(234, 41);
		this.sb_form_tasarimini_ac.TabIndex = 4;
		this.sb_form_tasarimini_ac.Text = "FORM TASARIMINI AÇ";
		this.sb_form_tasarimini_ac.Click += new System.EventHandler(sb_form_tasarimini_ac_Click);
		this.te_seri.EditValue = "A";
		this.te_seri.Location = new System.Drawing.Point(234, 71);
		this.te_seri.Name = "te_seri";
		this.te_seri.Size = new System.Drawing.Size(123, 20);
		this.te_seri.TabIndex = 2;
		this.labelControl17.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(155, 43);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(73, 19);
		this.labelControl17.TabIndex = 89;
		this.labelControl17.Text = "Evrak tipi : ";
		this.EvrakTipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.EvrakTipi.FormattingEnabled = true;
		this.EvrakTipi.Items.AddRange(new object[12]
		{
			"Satış faturası", "Satış irsaliyesi", "Alınan sipariş", "Proforma sipariş", "Tahsilat", "Alış faturası", "Alış irsaliyesi", "Depolar arası nakliye fişi", "Depolar arası sevk", "Depolar arası sipariş",
			"Gelen havale", "Giden havale"
		});
		this.EvrakTipi.Location = new System.Drawing.Point(234, 43);
		this.EvrakTipi.Name = "EvrakTipi";
		this.EvrakTipi.Size = new System.Drawing.Size(207, 21);
		this.EvrakTipi.TabIndex = 1;
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(155, 71);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(73, 19);
		this.labelControl1.TabIndex = 91;
		this.labelControl1.Text = "Evrak seri : ";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(155, 97);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(73, 19);
		this.labelControl2.TabIndex = 93;
		this.labelControl2.Text = "Evrak sıra : ";
		this.te_sira.EditValue = "83";
		this.te_sira.Location = new System.Drawing.Point(234, 97);
		this.te_sira.Name = "te_sira";
		this.te_sira.Size = new System.Drawing.Size(123, 20);
		this.te_sira.TabIndex = 3;
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(155, 18);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(173, 19);
		this.labelControl3.TabIndex = 94;
		this.labelControl3.Text = "ÖRNEK DOSYA BİLGİLERİ";
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(31, 136);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(86, 19);
		this.labelControl4.TabIndex = 96;
		this.labelControl4.Text = "Dosya : ";
		this.te_dosya.EditValue = "";
		this.te_dosya.Location = new System.Drawing.Point(123, 136);
		this.te_dosya.Name = "te_dosya";
		this.te_dosya.Size = new System.Drawing.Size(234, 20);
		this.te_dosya.TabIndex = 97;
		this.sb_dosya_sec.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_dosya_sec.Appearance.Options.UseFont = true;
		this.sb_dosya_sec.Location = new System.Drawing.Point(363, 136);
		this.sb_dosya_sec.Name = "sb_dosya_sec";
		this.sb_dosya_sec.Size = new System.Drawing.Size(78, 23);
		this.sb_dosya_sec.TabIndex = 98;
		this.sb_dosya_sec.Text = "Dosya seç";
		this.sb_dosya_sec.Click += new System.EventHandler(sb_dosya_sec_Click);
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 9f);
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(123, 162);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(234, 19);
		this.labelControl5.TabIndex = 99;
		this.labelControl5.Text = "Boş bırakılabilir.";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(481, 240);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.sb_dosya_sec);
		base.Controls.Add(this.te_dosya);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.te_sira);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.EvrakTipi);
		base.Controls.Add(this.labelControl17);
		base.Controls.Add(this.te_seri);
		base.Controls.Add(this.sb_form_tasarimini_ac);
		base.Name = "GenelEvrakFormTasarimiForm";
		this.Text = "Genel evrak form tasarımı";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.te_seri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_sira.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_dosya.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
