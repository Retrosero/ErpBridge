using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli.Import;

public class TxtCsvImport : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public string DosyaYolu = "";

	private IContainer components;

	private OpenFileDialog openFileDialog1;

	private SimpleButton sb_aktarima_basla;

	private LabelControl labelControl1;

	public System.Windows.Forms.ComboBox cb_aktarimayarlari;

	private SimpleButton sb_dosya_sec;

	public TextEdit te_dosyaadi;

	private LabelControl labelControl2;

	public TxtCsvImport(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		AktarimAyarlariAyarla();
		try
		{
			cb_aktarimayarlari.SelectedIndex = 0;
		}
		catch
		{
		}
	}

	private void AktarimAyarlariAyarla()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(string));
		dataTable.Columns.Add("Isim", typeof(string));
		foreach (string item in TxtCsvImportAktarimParametreleri.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			dataTable.Rows.Add(item, item);
		}
		cb_aktarimayarlari.DataSource = dataTable;
		cb_aktarimayarlari.ValueMember = "ID";
		cb_aktarimayarlari.DisplayMember = "Isim";
	}

	private void sb_dosya_sec_Click(object sender, EventArgs e)
	{
		openFileDialog1.Filter = "TXT Dosyaları|*.txt|CSV Dosyaları|*.csv|Tüm dosyalar|*.*";
		openFileDialog1.InitialDirectory = DosyaYolu;
		openFileDialog1.FileName = "";
		openFileDialog1.ShowDialog();
		if (openFileDialog1.FileName != "")
		{
			te_dosyaadi.Text = openFileDialog1.FileName;
		}
	}

	private void sb_aktarima_basla_Click(object sender, EventArgs e)
	{
		if (cb_aktarimayarlari.SelectedValue == null)
		{
			MessageBox.Show("Aktarım ayarlarını seçmediniz!", "HATA");
			return;
		}
		if (te_dosyaadi.Text == "")
		{
			MessageBox.Show("Dosya seçmediniz!", "HATA");
			return;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void cb_aktarimayarlari_SelectedIndexChanged(object sender, EventArgs e)
	{
		string text = "";
		if (cb_aktarimayarlari.SelectedValue != null)
		{
			text = cb_aktarimayarlari.SelectedValue.ToString();
		}
		Parametreler parametreler = ParametrelerDefault.TahsilatAktarimTxtCsvSablon(text);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "TahsilatAktarim", "", "TxtCsvAktarimSablon", text);
		DosyaYolu = parametreler._GetParametre("DosyaYolu")._GetString;
		string getString = parametreler._GetParametre("DosyaAdi")._GetString;
		if (getString != "")
		{
			te_dosyaadi.Text = getString;
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
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.sb_aktarima_basla = new DevExpress.XtraEditors.SimpleButton();
		this.cb_aktarimayarlari = new System.Windows.Forms.ComboBox();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.sb_dosya_sec = new DevExpress.XtraEditors.SimpleButton();
		this.te_dosyaadi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.te_dosyaadi.Properties).BeginInit();
		base.SuspendLayout();
		this.openFileDialog1.FileName = "openFileDialog1";
		this.sb_aktarima_basla.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_aktarima_basla.Appearance.Options.UseFont = true;
		this.sb_aktarima_basla.Location = new System.Drawing.Point(313, 79);
		this.sb_aktarima_basla.Name = "sb_aktarima_basla";
		this.sb_aktarima_basla.Size = new System.Drawing.Size(143, 23);
		this.sb_aktarima_basla.TabIndex = 9;
		this.sb_aktarima_basla.Text = "AKTARIMA BAŞLA";
		this.sb_aktarima_basla.Click += new System.EventHandler(sb_aktarima_basla_Click);
		this.cb_aktarimayarlari.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_aktarimayarlari.FormattingEnabled = true;
		this.cb_aktarimayarlari.Location = new System.Drawing.Point(125, 12);
		this.cb_aktarimayarlari.Name = "cb_aktarimayarlari";
		this.cb_aktarimayarlari.Size = new System.Drawing.Size(237, 21);
		this.cb_aktarimayarlari.TabIndex = 17;
		this.cb_aktarimayarlari.SelectedIndexChanged += new System.EventHandler(cb_aktarimayarlari_SelectedIndexChanged);
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(9, 17);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(110, 13);
		this.labelControl1.TabIndex = 18;
		this.labelControl1.Text = "Aktarım ayarları :";
		this.sb_dosya_sec.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_dosya_sec.Appearance.Options.UseFont = true;
		this.sb_dosya_sec.Location = new System.Drawing.Point(368, 36);
		this.sb_dosya_sec.Name = "sb_dosya_sec";
		this.sb_dosya_sec.Size = new System.Drawing.Size(88, 26);
		this.sb_dosya_sec.TabIndex = 25;
		this.sb_dosya_sec.Text = "DOSYA SEÇ";
		this.sb_dosya_sec.Click += new System.EventHandler(sb_dosya_sec_Click);
		this.te_dosyaadi.Location = new System.Drawing.Point(125, 39);
		this.te_dosyaadi.Name = "te_dosyaadi";
		this.te_dosyaadi.Size = new System.Drawing.Size(237, 20);
		this.te_dosyaadi.TabIndex = 24;
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(9, 42);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(110, 13);
		this.labelControl2.TabIndex = 23;
		this.labelControl2.Text = "Dosya :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(496, 120);
		base.Controls.Add(this.sb_dosya_sec);
		base.Controls.Add(this.te_dosyaadi);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.cb_aktarimayarlari);
		base.Controls.Add(this.sb_aktarima_basla);
		base.Name = "TxtCsvImport";
		this.Text = "Tahsilat evrak aktarımı (Txt/Csv/Xml)";
		((System.ComponentModel.ISupportInitialize)this.te_dosyaadi.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
