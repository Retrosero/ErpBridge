using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Import;

public class SqlImport : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public string DosyaYolu = "";

	private IContainer components;

	private OpenFileDialog openFileDialog1;

	private SimpleButton sb_aktarima_basla;

	private LabelControl labelControl1;

	private LabelControl labelControl2;

	public System.Windows.Forms.ComboBox cb_aktarimayarlari;

	private LabelControl labelControl3;

	public DateEdit dateEdit_baslangic_tarihi;

	public DateEdit dateEdit_bitis_tarihi;

	private LabelControl labelControl4;

	private LabelControl labelControl5;

	public SqlImport(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		AktarimAyarlariAyarla();
		cb_aktarimayarlari.SelectedIndex = 0;
	}

	private void AktarimAyarlariAyarla()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(string));
		dataTable.Columns.Add("Isim", typeof(string));
		foreach (string item in SqlImportAktarimParametreleri.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
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
	}

	private void sb_aktarima_basla_Click(object sender, EventArgs e)
	{
		if (cb_aktarimayarlari.SelectedValue == null)
		{
			MessageBox.Show("Aktarım ayarlarını seçmediniz!", "HATA");
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
		Parametreler parametreler = ParametrelerDefault.GenelAktarimSqlSablon(text);
		_ = DateTime.Now;
		DateTime dateTime = DateTime.Now.AddDays(-1.0);
		dateEdit_baslangic_tarihi.DateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
		dateEdit_bitis_tarihi.DateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "GenelAktarim", "", "SqlAktarimSablon", text);
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
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.dateEdit_baslangic_tarihi = new DevExpress.XtraEditors.DateEdit();
		this.dateEdit_bitis_tarihi = new DevExpress.XtraEditors.DateEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic_tarihi.Properties.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic_tarihi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis_tarihi.Properties.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis_tarihi.Properties).BeginInit();
		base.SuspendLayout();
		this.openFileDialog1.FileName = "openFileDialog1";
		this.sb_aktarima_basla.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_aktarima_basla.Appearance.Options.UseFont = true;
		this.sb_aktarima_basla.Location = new System.Drawing.Point(236, 133);
		this.sb_aktarima_basla.Name = "sb_aktarima_basla";
		this.sb_aktarima_basla.Size = new System.Drawing.Size(143, 23);
		this.sb_aktarima_basla.TabIndex = 9;
		this.sb_aktarima_basla.Text = "AKTARIMA BAŞLA";
		this.sb_aktarima_basla.Click += new System.EventHandler(sb_aktarima_basla_Click);
		this.cb_aktarimayarlari.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cb_aktarimayarlari.FormattingEnabled = true;
		this.cb_aktarimayarlari.Location = new System.Drawing.Point(142, 23);
		this.cb_aktarimayarlari.Name = "cb_aktarimayarlari";
		this.cb_aktarimayarlari.Size = new System.Drawing.Size(237, 21);
		this.cb_aktarimayarlari.TabIndex = 17;
		this.cb_aktarimayarlari.SelectedIndexChanged += new System.EventHandler(cb_aktarimayarlari_SelectedIndexChanged);
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(26, 28);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(110, 13);
		this.labelControl1.TabIndex = 18;
		this.labelControl1.Text = "Aktarım ayarları :";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(26, 68);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(110, 13);
		this.labelControl2.TabIndex = 20;
		this.labelControl2.Text = "Başlangıç tarihi  :";
		this.dateEdit_baslangic_tarihi.EditValue = null;
		this.dateEdit_baslangic_tarihi.Location = new System.Drawing.Point(142, 65);
		this.dateEdit_baslangic_tarihi.Name = "dateEdit_baslangic_tarihi";
		this.dateEdit_baslangic_tarihi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_baslangic_tarihi.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_baslangic_tarihi.Size = new System.Drawing.Size(100, 20);
		this.dateEdit_baslangic_tarihi.TabIndex = 21;
		this.dateEdit_bitis_tarihi.EditValue = null;
		this.dateEdit_bitis_tarihi.Location = new System.Drawing.Point(142, 91);
		this.dateEdit_bitis_tarihi.Name = "dateEdit_bitis_tarihi";
		this.dateEdit_bitis_tarihi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_bitis_tarihi.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_bitis_tarihi.Size = new System.Drawing.Size(100, 20);
		this.dateEdit_bitis_tarihi.TabIndex = 23;
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(26, 94);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(110, 13);
		this.labelControl3.TabIndex = 22;
		this.labelControl3.Text = "Bitiş tarihi  :";
		this.labelControl4.Location = new System.Drawing.Point(248, 68);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(84, 13);
		this.labelControl4.TabIndex = 24;
		this.labelControl4.Text = "@baslangic_tarihi";
		this.labelControl5.Location = new System.Drawing.Point(248, 94);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(59, 13);
		this.labelControl5.TabIndex = 25;
		this.labelControl5.Text = "@bitis_tarihi";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(398, 173);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.dateEdit_bitis_tarihi);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.dateEdit_baslangic_tarihi);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.cb_aktarimayarlari);
		base.Controls.Add(this.sb_aktarima_basla);
		base.Name = "SqlImport";
		this.Text = "Genel evrak aktarımı (Sql)";
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic_tarihi.Properties.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic_tarihi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis_tarihi.Properties.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis_tarihi.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
