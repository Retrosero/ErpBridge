using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Fora.App.Mikro.ForaAndroid;
using Fora.Mikro;
using Fora.Mikro.Enumler;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Rapor.StokSatis;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class SatisRaporu : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _KullaniciParametreleri;

	public RaporStokSatisSonuc _rapor_sonuc;

	public string _secili_rapor_kod;

	private IContainer components;

	private SimpleButton sb_yazdir;

	private SimpleButton sb_rapor_tasarimi;

	private TableLayoutPanel tableLayoutPanel1;

	private TableLayoutPanel tableLayoutPanel2;

	private TableLayoutPanel tableLayoutPanel3;

	private SimpleButton sb_kapat;

	private GridControl gridControl1;

	private BindingSource listitemsBindingSource;

	private BindingSource raporStokSatisSonucBindingSource;

	private GridView gridView1;

	private GridColumn gc_hesap_adi;

	private GridColumn gc_tutar1;

	private GridColumn gc_tutar2;

	private GridColumn gc_hesap_kodu;

	private GridColumn colmiktar1;

	private GridColumn colmiktar2;

	private GridColumn colmiktar3;

	private GridColumn gc_tut_3;

	private GridColumn gc_tutar3;

	private GridColumn colkodu;

	public SatisRaporu(MikroUygulamaBilgileri mikrouygulamabilgileri, RaporStokSatisSonuc rapor_sonuc, string secili_rapor_kod)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_rapor_sonuc = rapor_sonuc;
		_secili_rapor_kod = secili_rapor_kod;
		if (_secili_rapor_kod == "")
		{
			_secili_rapor_kod = "rapor_stok_satis_varsayilan";
		}
		InitializeComponent();
		if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuar")
		{
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			base.FormBorderStyle = FormBorderStyle.None;
			base.WindowState = FormWindowState.Maximized;
			CenterToScreen();
			base.KeyPreview = true;
		}
		else
		{
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.StartPosition = FormStartPosition.CenterScreen;
		}
		gc_tutar1.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.tutar1_secenek);
		gc_tutar2.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.tutar2_secenek);
		gc_tut_3.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.tutar3_secenek);
		colmiktar1.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.miktar1_secenek);
		colmiktar2.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.miktar2_secenek);
		colmiktar3.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.miktar3_secenek);
		if (_mikrouygulamabilgileri.KullaniciAdi != "SRV")
		{
			sb_rapor_tasarimi.Visible = false;
		}
	}

	private void Giris_Load(object sender, EventArgs e)
	{
		gc_tutar1.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.tutar1_secenek);
		gc_tutar2.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.tutar2_secenek);
		gc_tutar3.Caption = EnumUtility.EnumToLocalizedString(_rapor_sonuc.tutar3_secenek);
		gridControl1.DataSource = _rapor_sonuc.list_items;
	}

	private void sb_yazdir_Click(object sender, EventArgs e)
	{
		DxRaporStokSatis dxRaporStokSatis = new DxRaporStokSatis();
		List<RaporStokSatisSonuc> list = new List<RaporStokSatisSonuc>();
		list.Add(_rapor_sonuc);
		dxRaporStokSatis.DataSource = list;
		try
		{
			if (File.Exists("data\\raporlar\\" + _secili_rapor_kod + ".repx"))
			{
				dxRaporStokSatis.LoadLayout("data\\raporlar\\" + _secili_rapor_kod + ".repx");
			}
		}
		catch
		{
		}
		try
		{
			dxRaporStokSatis.ExportOptions.Pdf.Compressed = true;
			dxRaporStokSatis.ExportOptions.Pdf.ConvertImagesToJpeg = true;
			dxRaporStokSatis.ExportOptions.Pdf.ImageQuality = PdfJpegImageQuality.High;
			new ReportPrintTool(dxRaporStokSatis).Print();
			Activate();
			BringToFront();
		}
		catch
		{
		}
	}

	private void sb_rapor_tasarimi_Click(object sender, EventArgs e)
	{
		DxRaporStokSatis dxRaporStokSatis = new DxRaporStokSatis();
		List<RaporStokSatisSonuc> list = new List<RaporStokSatisSonuc>();
		list.Add(_rapor_sonuc);
		dxRaporStokSatis.DataSource = list;
		if (File.Exists("data\\raporlar\\" + _secili_rapor_kod + ".repx"))
		{
			dxRaporStokSatis.LoadLayout("data\\raporlar\\" + _secili_rapor_kod + ".repx");
		}
		else
		{
			MessageBox.Show("Rapor tasarımını yaptıktan sonra, 'data\\raporlar' klasörü içinde '[rapor_ismi].repx' olarak kayıt ediniz. Varsayılan rapor tasarımı için ise 'data\\raporlar' klasörü içinde 'rapor_stok_satis_varsayilan.repx' olarak kayıt ediniz.");
		}
		ReportDesignTool reportDesignTool = new ReportDesignTool(dxRaporStokSatis);
		try
		{
			dxRaporStokSatis.DisplayName = _secili_rapor_kod;
			reportDesignTool.ShowDesignerDialog();
		}
		catch
		{
			MessageBox.Show("Seçili raporun tipi uygun değil. Lütfen uygun bir rapor seçiniz.");
		}
	}

	private void sb_kapat_Click(object sender, EventArgs e)
	{
		base.FormClosing -= SatisRaporu_FormClosing;
		Close();
	}

	private void SatisRaporu_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciAdi == "deryafuar")
		{
			e.Cancel = true;
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
		this.components = new System.ComponentModel.Container();
		this.sb_yazdir = new DevExpress.XtraEditors.SimpleButton();
		this.sb_rapor_tasarimi = new DevExpress.XtraEditors.SimpleButton();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
		this.sb_kapat = new DevExpress.XtraEditors.SimpleButton();
		this.gridControl1 = new DevExpress.XtraGrid.GridControl();
		this.listitemsBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.raporStokSatisSonucBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_hesap_kodu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_hesap_adi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colmiktar1 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colmiktar2 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colmiktar3 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_tutar1 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_tutar2 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_tut_3 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_tutar3 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colkodu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.tableLayoutPanel1.SuspendLayout();
		this.tableLayoutPanel2.SuspendLayout();
		this.tableLayoutPanel3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.listitemsBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.raporStokSatisSonucBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		base.SuspendLayout();
		this.sb_yazdir.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_yazdir.Appearance.Options.UseFont = true;
		this.sb_yazdir.Dock = System.Windows.Forms.DockStyle.Fill;
		this.sb_yazdir.Location = new System.Drawing.Point(568, 3);
		this.sb_yazdir.Name = "sb_yazdir";
		this.sb_yazdir.Size = new System.Drawing.Size(395, 56);
		this.sb_yazdir.TabIndex = 3;
		this.sb_yazdir.Text = "Yazdır";
		this.sb_yazdir.Click += new System.EventHandler(sb_yazdir_Click);
		this.sb_rapor_tasarimi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_rapor_tasarimi.Appearance.Options.UseFont = true;
		this.sb_rapor_tasarimi.Dock = System.Windows.Forms.DockStyle.Fill;
		this.sb_rapor_tasarimi.Location = new System.Drawing.Point(3, 3);
		this.sb_rapor_tasarimi.Name = "sb_rapor_tasarimi";
		this.sb_rapor_tasarimi.Size = new System.Drawing.Size(105, 56);
		this.sb_rapor_tasarimi.TabIndex = 4;
		this.sb_rapor_tasarimi.Text = "Rapor Tasarımı";
		this.sb_rapor_tasarimi.Click += new System.EventHandler(sb_rapor_tasarimi_Click);
		this.tableLayoutPanel1.ColumnCount = 1;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
		this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 1;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(978, 604);
		this.tableLayoutPanel1.TabIndex = 5;
		this.tableLayoutPanel2.ColumnCount = 1;
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
		this.tableLayoutPanel2.Controls.Add(this.gridControl1, 0, 0);
		this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
		this.tableLayoutPanel2.Name = "tableLayoutPanel2";
		this.tableLayoutPanel2.RowCount = 2;
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68f));
		this.tableLayoutPanel2.Size = new System.Drawing.Size(972, 598);
		this.tableLayoutPanel2.TabIndex = 0;
		this.tableLayoutPanel3.ColumnCount = 3;
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.72686f));
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.27314f));
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400f));
		this.tableLayoutPanel3.Controls.Add(this.sb_kapat, 1, 0);
		this.tableLayoutPanel3.Controls.Add(this.sb_yazdir, 2, 0);
		this.tableLayoutPanel3.Controls.Add(this.sb_rapor_tasarimi, 0, 0);
		this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 533);
		this.tableLayoutPanel3.Name = "tableLayoutPanel3";
		this.tableLayoutPanel3.RowCount = 1;
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel3.Size = new System.Drawing.Size(966, 62);
		this.tableLayoutPanel3.TabIndex = 0;
		this.sb_kapat.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_kapat.Appearance.Options.UseFont = true;
		this.sb_kapat.Dock = System.Windows.Forms.DockStyle.Fill;
		this.sb_kapat.Location = new System.Drawing.Point(114, 3);
		this.sb_kapat.Name = "sb_kapat";
		this.sb_kapat.Size = new System.Drawing.Size(448, 56);
		this.sb_kapat.TabIndex = 5;
		this.sb_kapat.Text = "Kapat";
		this.sb_kapat.Click += new System.EventHandler(sb_kapat_Click);
		this.gridControl1.DataSource = this.listitemsBindingSource;
		this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.gridControl1.Location = new System.Drawing.Point(3, 3);
		this.gridControl1.MainView = this.gridView1;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(966, 524);
		this.gridControl1.TabIndex = 1;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gridView1 });
		this.listitemsBindingSource.DataMember = "list_items";
		this.listitemsBindingSource.DataSource = this.raporStokSatisSonucBindingSource;
		this.raporStokSatisSonucBindingSource.DataSource = typeof(Fora.Mikro.Rapor.StokSatis.RaporStokSatisSonuc);
		this.gridView1.Appearance.FooterPanel.Font = new System.Drawing.Font("Tahoma", 15f, System.Drawing.FontStyle.Bold);
		this.gridView1.Appearance.FooterPanel.Options.UseFont = true;
		this.gridView1.Appearance.HeaderPanel.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.gridView1.Appearance.HeaderPanel.Options.UseFont = true;
		this.gridView1.Appearance.Row.Font = new System.Drawing.Font("Tahoma", 10f);
		this.gridView1.Appearance.Row.Options.UseFont = true;
		this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[8] { this.gc_hesap_kodu, this.gc_hesap_adi, this.colmiktar1, this.colmiktar2, this.colmiktar3, this.gc_tutar1, this.gc_tutar2, this.gc_tut_3 });
		this.gridView1.GridControl = this.gridControl1;
		this.gridView1.Name = "gridView1";
		this.gridView1.OptionsView.ShowFooter = true;
		this.gridView1.OptionsView.ShowGroupPanel = false;
		this.gc_hesap_kodu.AppearanceCell.Options.UseTextOptions = true;
		this.gc_hesap_kodu.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.gc_hesap_kodu.Caption = "Hesap kodu";
		this.gc_hesap_kodu.FieldName = "kodu";
		this.gc_hesap_kodu.Name = "gc_hesap_kodu";
		this.gc_hesap_kodu.OptionsColumn.AllowEdit = false;
		this.gc_hesap_kodu.OptionsColumn.AllowFocus = false;
		this.gc_hesap_kodu.Visible = true;
		this.gc_hesap_kodu.VisibleIndex = 0;
		this.gc_hesap_kodu.Width = 58;
		this.gc_hesap_adi.AppearanceCell.Options.UseTextOptions = true;
		this.gc_hesap_adi.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.gc_hesap_adi.Caption = "Hesap adı";
		this.gc_hesap_adi.FieldName = "adi";
		this.gc_hesap_adi.Name = "gc_hesap_adi";
		this.gc_hesap_adi.OptionsColumn.AllowEdit = false;
		this.gc_hesap_adi.OptionsColumn.AllowFocus = false;
		this.gc_hesap_adi.Visible = true;
		this.gc_hesap_adi.VisibleIndex = 1;
		this.gc_hesap_adi.Width = 389;
		this.colmiktar1.AppearanceCell.Options.UseTextOptions = true;
		this.colmiktar1.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.colmiktar1.Caption = "Miktar 1";
		this.colmiktar1.DisplayFormat.FormatString = "{0:n}";
		this.colmiktar1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
		this.colmiktar1.FieldName = "miktar1";
		this.colmiktar1.Name = "colmiktar1";
		this.colmiktar1.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "miktar1", "{0:n}")
		});
		this.colmiktar1.Visible = true;
		this.colmiktar1.VisibleIndex = 2;
		this.colmiktar1.Width = 76;
		this.colmiktar2.AppearanceCell.Options.UseTextOptions = true;
		this.colmiktar2.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.colmiktar2.Caption = "Miktar 2";
		this.colmiktar2.DisplayFormat.FormatString = "{0:n}";
		this.colmiktar2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
		this.colmiktar2.FieldName = "miktar2";
		this.colmiktar2.Name = "colmiktar2";
		this.colmiktar2.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "miktar2", "{0:n}")
		});
		this.colmiktar2.Visible = true;
		this.colmiktar2.VisibleIndex = 3;
		this.colmiktar2.Width = 80;
		this.colmiktar3.AppearanceCell.Options.UseTextOptions = true;
		this.colmiktar3.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.colmiktar3.Caption = "Miktar 3";
		this.colmiktar3.DisplayFormat.FormatString = "{0:n}";
		this.colmiktar3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
		this.colmiktar3.FieldName = "miktar3";
		this.colmiktar3.Name = "colmiktar3";
		this.colmiktar3.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "miktar3", "{0:n}")
		});
		this.colmiktar3.Visible = true;
		this.colmiktar3.VisibleIndex = 4;
		this.colmiktar3.Width = 85;
		this.gc_tutar1.AppearanceCell.Options.UseTextOptions = true;
		this.gc_tutar1.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.gc_tutar1.Caption = "Tutar 1";
		this.gc_tutar1.DisplayFormat.FormatString = "{0:n}";
		this.gc_tutar1.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
		this.gc_tutar1.FieldName = "tutar1";
		this.gc_tutar1.Name = "gc_tutar1";
		this.gc_tutar1.OptionsColumn.AllowEdit = false;
		this.gc_tutar1.OptionsColumn.AllowFocus = false;
		this.gc_tutar1.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "tutar1", "{0:n}")
		});
		this.gc_tutar1.Visible = true;
		this.gc_tutar1.VisibleIndex = 5;
		this.gc_tutar1.Width = 83;
		this.gc_tutar2.AppearanceCell.Options.UseTextOptions = true;
		this.gc_tutar2.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.gc_tutar2.Caption = "Tutar 2";
		this.gc_tutar2.DisplayFormat.FormatString = "{0:n}";
		this.gc_tutar2.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
		this.gc_tutar2.FieldName = "tutar2";
		this.gc_tutar2.Name = "gc_tutar2";
		this.gc_tutar2.OptionsColumn.AllowEdit = false;
		this.gc_tutar2.OptionsColumn.AllowFocus = false;
		this.gc_tutar2.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "tutar2", "{0:n}")
		});
		this.gc_tutar2.Visible = true;
		this.gc_tutar2.VisibleIndex = 6;
		this.gc_tutar2.Width = 83;
		this.gc_tut_3.AppearanceCell.Options.UseTextOptions = true;
		this.gc_tut_3.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.gc_tut_3.Caption = "Tutar 3";
		this.gc_tut_3.DisplayFormat.FormatString = "{0:n}";
		this.gc_tut_3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
		this.gc_tut_3.FieldName = "tutar3";
		this.gc_tut_3.Name = "gc_tut_3";
		this.gc_tut_3.OptionsColumn.AllowEdit = false;
		this.gc_tut_3.OptionsColumn.AllowFocus = false;
		this.gc_tut_3.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "tutar3", "{0:n}")
		});
		this.gc_tut_3.Visible = true;
		this.gc_tut_3.VisibleIndex = 7;
		this.gc_tut_3.Width = 94;
		this.gc_tutar3.AppearanceCell.Options.UseTextOptions = true;
		this.gc_tutar3.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.gc_tutar3.Caption = "Tutar 3";
		this.gc_tutar3.DisplayFormat.FormatString = "{0:n}";
		this.gc_tutar3.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
		this.gc_tutar3.FieldName = "tutar3";
		this.gc_tutar3.Name = "gc_tutar3";
		this.gc_tutar3.OptionsColumn.AllowEdit = false;
		this.gc_tutar3.OptionsColumn.AllowFocus = false;
		this.gc_tutar3.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "tutar3", "{0:n}")
		});
		this.gc_tutar3.Width = 136;
		this.colkodu.FieldName = "kodu";
		this.colkodu.Name = "colkodu";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(978, 604);
		base.Controls.Add(this.tableLayoutPanel1);
		base.Name = "SatisRaporu";
		this.Text = "Rapor sonucu";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(SatisRaporu_FormClosing);
		base.Load += new System.EventHandler(Giris_Load);
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel2.ResumeLayout(false);
		this.tableLayoutPanel3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.listitemsBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.raporStokSatisSonucBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		base.ResumeLayout(false);
	}
}
