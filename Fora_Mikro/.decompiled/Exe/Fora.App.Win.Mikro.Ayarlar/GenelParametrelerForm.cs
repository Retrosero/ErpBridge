using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;

namespace Fora.App.Win.Mikro.Ayarlar;

public class GenelParametrelerForm : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private IContainer components;

	private SimpleButton sb_ayarlari_kaydet;

	private XtraTabControl xtraTabControl2;

	private XtraTabPage xtraTabPage3;

	private XtraTabPage xtraTabPage1;

	private LabelControl labelControl2;

	private TextEdit CariYeniCariHesapOnEkleri;

	private LabelControl labelControl1;

	private TextEdit Vergi1KisaAdi;

	private LabelControl labelControl3;

	private LabelControl labelControl15;

	private LabelControl labelControl5;

	private TextEdit Vergi1UzunAdi;

	private LabelControl labelControl4;

	private SpinEdit Vergi10Yuzde;

	private TextEdit Vergi10UzunAdi;

	private TextEdit Vergi10KisaAdi;

	private LabelControl labelControl16;

	private SpinEdit Vergi9Yuzde;

	private TextEdit Vergi9UzunAdi;

	private TextEdit Vergi9KisaAdi;

	private LabelControl labelControl14;

	private SpinEdit Vergi8Yuzde;

	private TextEdit Vergi8UzunAdi;

	private TextEdit Vergi8KisaAdi;

	private LabelControl labelControl13;

	private SpinEdit Vergi7Yuzde;

	private TextEdit Vergi7UzunAdi;

	private TextEdit Vergi7KisaAdi;

	private LabelControl labelControl12;

	private SpinEdit Vergi6Yuzde;

	private TextEdit Vergi6UzunAdi;

	private TextEdit Vergi6KisaAdi;

	private LabelControl labelControl11;

	private SpinEdit Vergi5Yuzde;

	private TextEdit Vergi5UzunAdi;

	private TextEdit Vergi5KisaAdi;

	private LabelControl labelControl10;

	private SpinEdit Vergi4Yuzde;

	private TextEdit Vergi4UzunAdi;

	private TextEdit Vergi4KisaAdi;

	private LabelControl labelControl9;

	private SpinEdit Vergi3Yuzde;

	private TextEdit Vergi3UzunAdi;

	private TextEdit Vergi3KisaAdi;

	private LabelControl labelControl8;

	private SpinEdit Vergi2Yuzde;

	private TextEdit Vergi2UzunAdi;

	private TextEdit Vergi2KisaAdi;

	private LabelControl labelControl7;

	private LabelControl labelControl6;

	private SpinEdit Vergi1Yuzde;

	private TextEdit CariYeniCariMuhasebeKodu;

	private LabelControl labelControl17;

	public GenelParametrelerForm(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
		EkranGuncelle();
	}

	private void EkranGuncelle()
	{
		CariYeniCariHesapOnEkleri.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("CariYeniCariHesapOnEkleri")._GetString;
		Vergi1KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1KisaAdi")._GetString;
		Vergi1UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1UzunAdi")._GetString;
		Vergi1Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1Yuzde")._GetDouble;
		Vergi2KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2KisaAdi")._GetString;
		Vergi2UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2UzunAdi")._GetString;
		Vergi2Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2Yuzde")._GetDouble;
		Vergi3KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3KisaAdi")._GetString;
		Vergi3UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3UzunAdi")._GetString;
		Vergi3Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3Yuzde")._GetDouble;
		Vergi4KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4KisaAdi")._GetString;
		Vergi4UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4UzunAdi")._GetString;
		Vergi4Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4Yuzde")._GetDouble;
		Vergi5KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5KisaAdi")._GetString;
		Vergi5UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5UzunAdi")._GetString;
		Vergi5Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5Yuzde")._GetDouble;
		Vergi6KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6KisaAdi")._GetString;
		Vergi6UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6UzunAdi")._GetString;
		Vergi6Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6Yuzde")._GetDouble;
		Vergi7KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7KisaAdi")._GetString;
		Vergi7UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7UzunAdi")._GetString;
		Vergi7Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7Yuzde")._GetDouble;
		Vergi8KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8KisaAdi")._GetString;
		Vergi8UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8UzunAdi")._GetString;
		Vergi8Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8Yuzde")._GetDouble;
		Vergi9KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9KisaAdi")._GetString;
		Vergi9UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9UzunAdi")._GetString;
		Vergi9Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9Yuzde")._GetDouble;
		Vergi10KisaAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10KisaAdi")._GetString;
		Vergi10UzunAdi.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10UzunAdi")._GetString;
		Vergi10Yuzde.Value = (decimal)_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10Yuzde")._GetDouble;
		CariYeniCariMuhasebeKodu.Text = _mikrouygulamabilgileri.GenelParametreler._GetParametre("CariYeniCariMuhasebeKodu")._GetString;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("CariYeniCariHesapOnEkleri")._SetString = CariYeniCariHesapOnEkleri.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1KisaAdi")._SetString = Vergi1KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1UzunAdi")._SetString = Vergi1UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi1Yuzde")._SetDouble = (double)Vergi1Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2KisaAdi")._SetString = Vergi2KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2UzunAdi")._SetString = Vergi2UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi2Yuzde")._SetDouble = (double)Vergi2Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3KisaAdi")._SetString = Vergi3KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3UzunAdi")._SetString = Vergi3UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi3Yuzde")._SetDouble = (double)Vergi3Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4KisaAdi")._SetString = Vergi4KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4UzunAdi")._SetString = Vergi4UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi4Yuzde")._SetDouble = (double)Vergi4Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5KisaAdi")._SetString = Vergi5KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5UzunAdi")._SetString = Vergi5UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi5Yuzde")._SetDouble = (double)Vergi5Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6KisaAdi")._SetString = Vergi6KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6UzunAdi")._SetString = Vergi6UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi6Yuzde")._SetDouble = (double)Vergi6Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7KisaAdi")._SetString = Vergi7KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7UzunAdi")._SetString = Vergi7UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi7Yuzde")._SetDouble = (double)Vergi7Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8KisaAdi")._SetString = Vergi8KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8UzunAdi")._SetString = Vergi8UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi8Yuzde")._SetDouble = (double)Vergi8Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9KisaAdi")._SetString = Vergi9KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9UzunAdi")._SetString = Vergi9UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi9Yuzde")._SetDouble = (double)Vergi9Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10KisaAdi")._SetString = Vergi10KisaAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10UzunAdi")._SetString = Vergi10UzunAdi.Text;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("Vergi10Yuzde")._SetDouble = (double)Vergi10Yuzde.Value;
		_mikrouygulamabilgileri.GenelParametreler._GetParametre("CariYeniCariMuhasebeKodu")._SetString = CariYeniCariMuhasebeKodu.Text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.GenelParametreler);
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
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.Vergi10Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi10UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi10KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl16 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi9Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi9UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi9KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl14 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi8Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi8UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi8KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl13 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi7Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi7UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi7KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi6Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi6UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi6KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi5Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi5UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi5KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi4Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi4UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi4KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi3Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi3UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi3KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi2Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.Vergi2UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.Vergi2KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi1Yuzde = new DevExpress.XtraEditors.SpinEdit();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi1UzunAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
		this.Vergi1KisaAdi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.CariYeniCariHesapOnEkleri = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.CariYeniCariMuhasebeKodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).BeginInit();
		this.xtraTabControl2.SuspendLayout();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.Vergi10Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi10UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi10KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi9Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi9UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi9KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi8Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi8UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi8KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi7Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi7UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi7KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi6Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi6UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi6KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi5Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi5UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi5KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi4Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi4UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi4KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi3Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi3UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi3KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi2Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi2UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi2KisaAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi1Yuzde.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi1UzunAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi1KisaAdi.Properties).BeginInit();
		this.xtraTabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.CariYeniCariHesapOnEkleri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.CariYeniCariMuhasebeKodu.Properties).BeginInit();
		base.SuspendLayout();
		this.sb_ayarlari_kaydet.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(511, 458);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(214, 24);
		this.sb_ayarlari_kaydet.TabIndex = 9;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.xtraTabControl2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.xtraTabControl2.Location = new System.Drawing.Point(12, 12);
		this.xtraTabControl2.Name = "xtraTabControl2";
		this.xtraTabControl2.SelectedTabPage = this.xtraTabPage3;
		this.xtraTabControl2.Size = new System.Drawing.Size(713, 440);
		this.xtraTabControl2.TabIndex = 10;
		this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[2] { this.xtraTabPage3, this.xtraTabPage1 });
		this.xtraTabPage3.Controls.Add(this.Vergi10Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi10UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi10KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl16);
		this.xtraTabPage3.Controls.Add(this.Vergi9Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi9UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi9KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl14);
		this.xtraTabPage3.Controls.Add(this.Vergi8Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi8UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi8KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl13);
		this.xtraTabPage3.Controls.Add(this.Vergi7Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi7UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi7KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl12);
		this.xtraTabPage3.Controls.Add(this.Vergi6Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi6UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi6KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl11);
		this.xtraTabPage3.Controls.Add(this.Vergi5Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi5UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi5KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl10);
		this.xtraTabPage3.Controls.Add(this.Vergi4Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi4UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi4KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl9);
		this.xtraTabPage3.Controls.Add(this.Vergi3Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi3UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi3KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl8);
		this.xtraTabPage3.Controls.Add(this.Vergi2Yuzde);
		this.xtraTabPage3.Controls.Add(this.Vergi2UzunAdi);
		this.xtraTabPage3.Controls.Add(this.Vergi2KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl7);
		this.xtraTabPage3.Controls.Add(this.labelControl6);
		this.xtraTabPage3.Controls.Add(this.Vergi1Yuzde);
		this.xtraTabPage3.Controls.Add(this.labelControl5);
		this.xtraTabPage3.Controls.Add(this.Vergi1UzunAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl4);
		this.xtraTabPage3.Controls.Add(this.labelControl15);
		this.xtraTabPage3.Controls.Add(this.Vergi1KisaAdi);
		this.xtraTabPage3.Controls.Add(this.labelControl3);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(707, 412);
		this.xtraTabPage3.Text = "Genel";
		this.Vergi10Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi10Yuzde.Location = new System.Drawing.Point(320, 356);
		this.Vergi10Yuzde.Name = "Vergi10Yuzde";
		this.Vergi10Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi10Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi10Yuzde.TabIndex = 218;
		this.Vergi10UzunAdi.Location = new System.Drawing.Point(188, 356);
		this.Vergi10UzunAdi.Name = "Vergi10UzunAdi";
		this.Vergi10UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi10UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi10UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi10UzunAdi.TabIndex = 217;
		this.Vergi10KisaAdi.Location = new System.Drawing.Point(56, 356);
		this.Vergi10KisaAdi.Name = "Vergi10KisaAdi";
		this.Vergi10KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi10KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi10KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi10KisaAdi.TabIndex = 216;
		this.labelControl16.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl16.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl16.Location = new System.Drawing.Point(17, 359);
		this.labelControl16.Name = "labelControl16";
		this.labelControl16.Size = new System.Drawing.Size(33, 13);
		this.labelControl16.TabIndex = 215;
		this.labelControl16.Text = "10 :";
		this.Vergi9Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi9Yuzde.Location = new System.Drawing.Point(320, 330);
		this.Vergi9Yuzde.Name = "Vergi9Yuzde";
		this.Vergi9Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi9Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi9Yuzde.TabIndex = 214;
		this.Vergi9UzunAdi.Location = new System.Drawing.Point(188, 330);
		this.Vergi9UzunAdi.Name = "Vergi9UzunAdi";
		this.Vergi9UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi9UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi9UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi9UzunAdi.TabIndex = 213;
		this.Vergi9KisaAdi.Location = new System.Drawing.Point(56, 330);
		this.Vergi9KisaAdi.Name = "Vergi9KisaAdi";
		this.Vergi9KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi9KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi9KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi9KisaAdi.TabIndex = 212;
		this.labelControl14.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl14.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl14.Location = new System.Drawing.Point(17, 333);
		this.labelControl14.Name = "labelControl14";
		this.labelControl14.Size = new System.Drawing.Size(33, 13);
		this.labelControl14.TabIndex = 211;
		this.labelControl14.Text = "9 :";
		this.Vergi8Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi8Yuzde.Location = new System.Drawing.Point(320, 304);
		this.Vergi8Yuzde.Name = "Vergi8Yuzde";
		this.Vergi8Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi8Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi8Yuzde.TabIndex = 210;
		this.Vergi8UzunAdi.Location = new System.Drawing.Point(188, 304);
		this.Vergi8UzunAdi.Name = "Vergi8UzunAdi";
		this.Vergi8UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi8UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi8UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi8UzunAdi.TabIndex = 209;
		this.Vergi8KisaAdi.Location = new System.Drawing.Point(56, 304);
		this.Vergi8KisaAdi.Name = "Vergi8KisaAdi";
		this.Vergi8KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi8KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi8KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi8KisaAdi.TabIndex = 208;
		this.labelControl13.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl13.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl13.Location = new System.Drawing.Point(17, 307);
		this.labelControl13.Name = "labelControl13";
		this.labelControl13.Size = new System.Drawing.Size(33, 13);
		this.labelControl13.TabIndex = 207;
		this.labelControl13.Text = "8 :";
		this.Vergi7Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi7Yuzde.Location = new System.Drawing.Point(320, 278);
		this.Vergi7Yuzde.Name = "Vergi7Yuzde";
		this.Vergi7Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi7Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi7Yuzde.TabIndex = 206;
		this.Vergi7UzunAdi.Location = new System.Drawing.Point(188, 278);
		this.Vergi7UzunAdi.Name = "Vergi7UzunAdi";
		this.Vergi7UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi7UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi7UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi7UzunAdi.TabIndex = 205;
		this.Vergi7KisaAdi.Location = new System.Drawing.Point(56, 278);
		this.Vergi7KisaAdi.Name = "Vergi7KisaAdi";
		this.Vergi7KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi7KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi7KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi7KisaAdi.TabIndex = 204;
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(17, 281);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(33, 13);
		this.labelControl12.TabIndex = 203;
		this.labelControl12.Text = "7 :";
		this.Vergi6Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi6Yuzde.Location = new System.Drawing.Point(320, 252);
		this.Vergi6Yuzde.Name = "Vergi6Yuzde";
		this.Vergi6Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi6Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi6Yuzde.TabIndex = 202;
		this.Vergi6UzunAdi.Location = new System.Drawing.Point(188, 252);
		this.Vergi6UzunAdi.Name = "Vergi6UzunAdi";
		this.Vergi6UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi6UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi6UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi6UzunAdi.TabIndex = 201;
		this.Vergi6KisaAdi.Location = new System.Drawing.Point(56, 252);
		this.Vergi6KisaAdi.Name = "Vergi6KisaAdi";
		this.Vergi6KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi6KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi6KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi6KisaAdi.TabIndex = 200;
		this.labelControl11.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl11.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl11.Location = new System.Drawing.Point(17, 255);
		this.labelControl11.Name = "labelControl11";
		this.labelControl11.Size = new System.Drawing.Size(33, 13);
		this.labelControl11.TabIndex = 199;
		this.labelControl11.Text = "6 :";
		this.Vergi5Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi5Yuzde.Location = new System.Drawing.Point(320, 226);
		this.Vergi5Yuzde.Name = "Vergi5Yuzde";
		this.Vergi5Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi5Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi5Yuzde.TabIndex = 198;
		this.Vergi5UzunAdi.Location = new System.Drawing.Point(188, 226);
		this.Vergi5UzunAdi.Name = "Vergi5UzunAdi";
		this.Vergi5UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi5UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi5UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi5UzunAdi.TabIndex = 197;
		this.Vergi5KisaAdi.Location = new System.Drawing.Point(56, 226);
		this.Vergi5KisaAdi.Name = "Vergi5KisaAdi";
		this.Vergi5KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi5KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi5KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi5KisaAdi.TabIndex = 196;
		this.labelControl10.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl10.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl10.Location = new System.Drawing.Point(17, 229);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(33, 13);
		this.labelControl10.TabIndex = 195;
		this.labelControl10.Text = "5 :";
		this.Vergi4Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi4Yuzde.Location = new System.Drawing.Point(320, 200);
		this.Vergi4Yuzde.Name = "Vergi4Yuzde";
		this.Vergi4Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi4Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi4Yuzde.TabIndex = 194;
		this.Vergi4UzunAdi.Location = new System.Drawing.Point(188, 200);
		this.Vergi4UzunAdi.Name = "Vergi4UzunAdi";
		this.Vergi4UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi4UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi4UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi4UzunAdi.TabIndex = 193;
		this.Vergi4KisaAdi.Location = new System.Drawing.Point(56, 200);
		this.Vergi4KisaAdi.Name = "Vergi4KisaAdi";
		this.Vergi4KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi4KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi4KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi4KisaAdi.TabIndex = 192;
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(17, 203);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(33, 13);
		this.labelControl9.TabIndex = 191;
		this.labelControl9.Text = "4 :";
		this.Vergi3Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi3Yuzde.Location = new System.Drawing.Point(320, 174);
		this.Vergi3Yuzde.Name = "Vergi3Yuzde";
		this.Vergi3Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi3Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi3Yuzde.TabIndex = 190;
		this.Vergi3UzunAdi.Location = new System.Drawing.Point(188, 174);
		this.Vergi3UzunAdi.Name = "Vergi3UzunAdi";
		this.Vergi3UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi3UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi3UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi3UzunAdi.TabIndex = 189;
		this.Vergi3KisaAdi.Location = new System.Drawing.Point(56, 174);
		this.Vergi3KisaAdi.Name = "Vergi3KisaAdi";
		this.Vergi3KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi3KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi3KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi3KisaAdi.TabIndex = 188;
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(17, 177);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(33, 13);
		this.labelControl8.TabIndex = 187;
		this.labelControl8.Text = "3 :";
		this.Vergi2Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi2Yuzde.Location = new System.Drawing.Point(320, 148);
		this.Vergi2Yuzde.Name = "Vergi2Yuzde";
		this.Vergi2Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi2Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi2Yuzde.TabIndex = 186;
		this.Vergi2UzunAdi.Location = new System.Drawing.Point(188, 148);
		this.Vergi2UzunAdi.Name = "Vergi2UzunAdi";
		this.Vergi2UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi2UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi2UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi2UzunAdi.TabIndex = 185;
		this.Vergi2KisaAdi.Location = new System.Drawing.Point(56, 148);
		this.Vergi2KisaAdi.Name = "Vergi2KisaAdi";
		this.Vergi2KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi2KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi2KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi2KisaAdi.TabIndex = 184;
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(17, 151);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(33, 13);
		this.labelControl7.TabIndex = 183;
		this.labelControl7.Text = "2 :";
		this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 10f);
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(320, 100);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(67, 16);
		this.labelControl6.TabIndex = 182;
		this.labelControl6.Text = "Yüzde";
		this.Vergi1Yuzde.EditValue = new decimal(new int[4]);
		this.Vergi1Yuzde.Location = new System.Drawing.Point(320, 122);
		this.Vergi1Yuzde.Name = "Vergi1Yuzde";
		this.Vergi1Yuzde.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.Vergi1Yuzde.Size = new System.Drawing.Size(67, 20);
		this.Vergi1Yuzde.TabIndex = 181;
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 10f);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(188, 100);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(126, 16);
		this.labelControl5.TabIndex = 169;
		this.labelControl5.Text = "Uzun adı";
		this.Vergi1UzunAdi.Location = new System.Drawing.Point(188, 122);
		this.Vergi1UzunAdi.Name = "Vergi1UzunAdi";
		this.Vergi1UzunAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi1UzunAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi1UzunAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi1UzunAdi.TabIndex = 168;
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 10f);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(56, 100);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(126, 16);
		this.labelControl4.TabIndex = 167;
		this.labelControl4.Text = "Kısa adı";
		this.labelControl15.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl15.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl15.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl15.Location = new System.Drawing.Point(56, 75);
		this.labelControl15.Name = "labelControl15";
		this.labelControl15.Size = new System.Drawing.Size(331, 19);
		this.labelControl15.TabIndex = 166;
		this.labelControl15.Text = "Vergi tanımları";
		this.Vergi1KisaAdi.Location = new System.Drawing.Point(56, 122);
		this.Vergi1KisaAdi.Name = "Vergi1KisaAdi";
		this.Vergi1KisaAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Vergi1KisaAdi.Properties.Appearance.Options.UseForeColor = true;
		this.Vergi1KisaAdi.Size = new System.Drawing.Size(126, 20);
		this.Vergi1KisaAdi.TabIndex = 4;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(17, 125);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(33, 13);
		this.labelControl3.TabIndex = 3;
		this.labelControl3.Text = "1 :";
		this.xtraTabPage1.Controls.Add(this.CariYeniCariMuhasebeKodu);
		this.xtraTabPage1.Controls.Add(this.labelControl17);
		this.xtraTabPage1.Controls.Add(this.labelControl2);
		this.xtraTabPage1.Controls.Add(this.CariYeniCariHesapOnEkleri);
		this.xtraTabPage1.Controls.Add(this.labelControl1);
		this.xtraTabPage1.Name = "xtraTabPage1";
		this.xtraTabPage1.Size = new System.Drawing.Size(707, 412);
		this.xtraTabPage1.Text = "Cari hesaplar";
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(165, 41);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(306, 13);
		this.labelControl2.TabIndex = 9;
		this.labelControl2.Text = "Birden fazla ön eki virgül ile ayırınız. Örn: 120.01.,120.02.";
		this.CariYeniCariHesapOnEkleri.Location = new System.Drawing.Point(165, 15);
		this.CariYeniCariHesapOnEkleri.Name = "CariYeniCariHesapOnEkleri";
		this.CariYeniCariHesapOnEkleri.Size = new System.Drawing.Size(306, 20);
		this.CariYeniCariHesapOnEkleri.TabIndex = 7;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(2, 18);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(157, 13);
		this.labelControl1.TabIndex = 8;
		this.labelControl1.Text = "Yeni cari hesap ön ekleri :";
		this.CariYeniCariMuhasebeKodu.Location = new System.Drawing.Point(165, 74);
		this.CariYeniCariMuhasebeKodu.Name = "CariYeniCariMuhasebeKodu";
		this.CariYeniCariMuhasebeKodu.Size = new System.Drawing.Size(306, 20);
		this.CariYeniCariMuhasebeKodu.TabIndex = 10;
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(2, 77);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(157, 13);
		this.labelControl17.TabIndex = 11;
		this.labelControl17.Text = "Yeni cari muhasebe kodu :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(735, 492);
		base.Controls.Add(this.xtraTabControl2);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Name = "GenelParametrelerForm";
		this.Text = "Genel Parametreler";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).EndInit();
		this.xtraTabControl2.ResumeLayout(false);
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.Vergi10Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi10UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi10KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi9Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi9UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi9KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi8Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi8UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi8KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi7Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi7UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi7KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi6Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi6UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi6KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi5Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi5UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi5KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi4Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi4UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi4KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi3Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi3UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi3KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi2Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi2UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi2KisaAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi1Yuzde.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi1UzunAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Vergi1KisaAdi.Properties).EndInit();
		this.xtraTabPage1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.CariYeniCariHesapOnEkleri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.CariYeniCariMuhasebeKodu.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
