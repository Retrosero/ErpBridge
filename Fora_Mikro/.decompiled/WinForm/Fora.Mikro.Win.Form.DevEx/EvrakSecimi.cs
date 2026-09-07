using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Fora.Mikro.Evraklar;

namespace Fora.Mikro.Win.Form.DevEx;

public class EvrakSecimi : XtraForm
{
	private List<Evrak> _Faturalar;

	public int[] _SelectedItems;

	private string _tag;

	private IContainer components;

	private GridControl gridControl1;

	private GridView gridView1;

	private GridColumn gridColumn_Seri;

	private GridColumn gridColumn_Sira;

	private GridColumn gridColumn_CariKod;

	private GridColumn gridColumn_EvrakTipi;

	private GridColumn gridColumn_Tarih;

	private GridColumn gridColumn_CariUnvan;

	private GridColumn gridColumn_SatirSayisi;

	private GridColumn gridColumn_AraToplam;

	private GridColumn gridColumn_IskontoTutari;

	private GridColumn gridColumn_MasrafTutari;

	private GridColumn gridColumn_KdvTutari;

	private GridColumn gridColumn_Yekun;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem gorunumToolStripMenuItem;

	private ToolStripMenuItem secimToolStripMenuItem;

	private ToolStripMenuItem tumunuSecToolStripMenuItem;

	private ToolStripMenuItem secimiOnaylaToolStripMenuItem;

	private ToolStripMenuItem gorunumuSaklaToolStripMenuItem;

	public EvrakSecimi(List<Evrak> Faturalar, string tag)
	{
		_tag = tag;
		_Faturalar = Faturalar;
		InitializeComponent();
	}

	private void Faturalar_Load(object sender, EventArgs e)
	{
		if (File.Exists(_tag + ".xml"))
		{
			gridView1.RestoreLayoutFromXml(_tag + ".xml");
		}
		gridControl1.DataSource = _Faturalar;
	}

	private void tumunuSecToolStripMenuItem_Click(object sender, EventArgs e)
	{
		gridView1.SelectAll();
	}

	private void secimiOnaylaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_SelectedItems = gridView1.GetSelectedRows();
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void gorunumuSaklaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		gridView1.SaveLayoutToXml(_tag + ".xml");
	}

	private void gridView1_CustomDrawCell_1(object sender, RowCellCustomDrawEventArgs e)
	{
		if (e.Column.VisibleIndex == 0)
		{
			(e.Cell as GridCellInfo).CellButtonRect = Rectangle.Empty;
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
		this.gridControl1 = new DevExpress.XtraGrid.GridControl();
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gridColumn_EvrakTipi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Seri = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Sira = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Tarih = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_CariKod = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_CariUnvan = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_SatirSayisi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_AraToplam = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_IskontoTutari = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_MasrafTutari = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_KdvTutari = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Yekun = new DevExpress.XtraGrid.Columns.GridColumn();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.gorunumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.gorunumuSaklaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.secimToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.tumunuSecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.secimiOnaylaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.gridControl1.Location = new System.Drawing.Point(0, 24);
		this.gridControl1.MainView = this.gridView1;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(704, 375);
		this.gridControl1.TabIndex = 0;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gridView1 });
		this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[12]
		{
			this.gridColumn_EvrakTipi, this.gridColumn_Seri, this.gridColumn_Sira, this.gridColumn_Tarih, this.gridColumn_CariKod, this.gridColumn_CariUnvan, this.gridColumn_SatirSayisi, this.gridColumn_AraToplam, this.gridColumn_IskontoTutari, this.gridColumn_MasrafTutari,
			this.gridColumn_KdvTutari, this.gridColumn_Yekun
		});
		this.gridView1.GridControl = this.gridControl1;
		this.gridView1.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "GetYekun", this.gridColumn_Yekun, "")
		});
		this.gridView1.Name = "gridView1";
		this.gridView1.OptionsBehavior.Editable = false;
		this.gridView1.OptionsBehavior.ReadOnly = true;
		this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView1.OptionsSelection.MultiSelect = true;
		this.gridView1.OptionsView.ShowFooter = true;
		this.gridView1.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(gridView1_CustomDrawCell_1);
		this.gridColumn_EvrakTipi.Caption = "Evrak Tipi";
		this.gridColumn_EvrakTipi.FieldName = "GetEvrakTipiString";
		this.gridColumn_EvrakTipi.Name = "gridColumn_EvrakTipi";
		this.gridColumn_EvrakTipi.Visible = true;
		this.gridColumn_EvrakTipi.VisibleIndex = 0;
		this.gridColumn_Seri.Caption = "Seri";
		this.gridColumn_Seri.FieldName = "evraknoseri";
		this.gridColumn_Seri.Name = "gridColumn_Seri";
		this.gridColumn_Seri.Visible = true;
		this.gridColumn_Seri.VisibleIndex = 1;
		this.gridColumn_Sira.Caption = "Sıra";
		this.gridColumn_Sira.FieldName = "evraknosira";
		this.gridColumn_Sira.Name = "gridColumn_Sira";
		this.gridColumn_Sira.Visible = true;
		this.gridColumn_Sira.VisibleIndex = 2;
		this.gridColumn_Tarih.Caption = "Tarih";
		this.gridColumn_Tarih.FieldName = "evraktarih";
		this.gridColumn_Tarih.Name = "gridColumn_Tarih";
		this.gridColumn_Tarih.Visible = true;
		this.gridColumn_Tarih.VisibleIndex = 3;
		this.gridColumn_CariKod.Caption = "Cari Kodu";
		this.gridColumn_CariKod.FieldName = "cari.GosterCariKodu";
		this.gridColumn_CariKod.Name = "gridColumn_CariKod";
		this.gridColumn_CariKod.Visible = true;
		this.gridColumn_CariKod.VisibleIndex = 4;
		this.gridColumn_CariUnvan.Caption = "Cari Ünvan";
		this.gridColumn_CariUnvan.FieldName = "cari.cari_unvan1";
		this.gridColumn_CariUnvan.Name = "gridColumn_CariUnvan";
		this.gridColumn_CariUnvan.Visible = true;
		this.gridColumn_CariUnvan.VisibleIndex = 5;
		this.gridColumn_SatirSayisi.Caption = "Satır Sayısı";
		this.gridColumn_SatirSayisi.FieldName = "GetKalemSayisi";
		this.gridColumn_SatirSayisi.Name = "gridColumn_SatirSayisi";
		this.gridColumn_SatirSayisi.Visible = true;
		this.gridColumn_SatirSayisi.VisibleIndex = 6;
		this.gridColumn_AraToplam.Caption = "Ara Toplam";
		this.gridColumn_AraToplam.FieldName = "GetAraToplam";
		this.gridColumn_AraToplam.Name = "gridColumn_AraToplam";
		this.gridColumn_AraToplam.Visible = true;
		this.gridColumn_AraToplam.VisibleIndex = 7;
		this.gridColumn_IskontoTutari.Caption = "İskonto Tutarı";
		this.gridColumn_IskontoTutari.FieldName = "GetIskontoTutari";
		this.gridColumn_IskontoTutari.Name = "gridColumn_IskontoTutari";
		this.gridColumn_IskontoTutari.Visible = true;
		this.gridColumn_IskontoTutari.VisibleIndex = 8;
		this.gridColumn_MasrafTutari.Caption = "Masraf Tutarı";
		this.gridColumn_MasrafTutari.FieldName = "GetMasrafTutari";
		this.gridColumn_MasrafTutari.Name = "gridColumn_MasrafTutari";
		this.gridColumn_MasrafTutari.Visible = true;
		this.gridColumn_MasrafTutari.VisibleIndex = 9;
		this.gridColumn_KdvTutari.Caption = "Kdv Tutarı";
		this.gridColumn_KdvTutari.FieldName = "GetKdvTutari";
		this.gridColumn_KdvTutari.Name = "gridColumn_KdvTutari";
		this.gridColumn_KdvTutari.Visible = true;
		this.gridColumn_KdvTutari.VisibleIndex = 10;
		this.gridColumn_Yekun.Caption = "Yekün";
		this.gridColumn_Yekun.FieldName = "GetYekun";
		this.gridColumn_Yekun.Name = "gridColumn_Yekun";
		this.gridColumn_Yekun.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum)
		});
		this.gridColumn_Yekun.Visible = true;
		this.gridColumn_Yekun.VisibleIndex = 11;
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.gorunumToolStripMenuItem, this.secimToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(704, 24);
		this.menuStrip1.TabIndex = 1;
		this.menuStrip1.Text = "menuStrip1";
		this.gorunumToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.gorunumuSaklaToolStripMenuItem });
		this.gorunumToolStripMenuItem.Name = "gorunumToolStripMenuItem";
		this.gorunumToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
		this.gorunumToolStripMenuItem.Text = "Görünüm";
		this.gorunumuSaklaToolStripMenuItem.Name = "gorunumuSaklaToolStripMenuItem";
		this.gorunumuSaklaToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.gorunumuSaklaToolStripMenuItem.Text = "Görünümü Sakla";
		this.gorunumuSaklaToolStripMenuItem.Click += new System.EventHandler(gorunumuSaklaToolStripMenuItem_Click);
		this.secimToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.tumunuSecToolStripMenuItem, this.secimiOnaylaToolStripMenuItem });
		this.secimToolStripMenuItem.Name = "secimToolStripMenuItem";
		this.secimToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.secimToolStripMenuItem.Text = "Seçim";
		this.tumunuSecToolStripMenuItem.Name = "tumunuSecToolStripMenuItem";
		this.tumunuSecToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.tumunuSecToolStripMenuItem.Text = "Tümünü Seç";
		this.tumunuSecToolStripMenuItem.Click += new System.EventHandler(tumunuSecToolStripMenuItem_Click);
		this.secimiOnaylaToolStripMenuItem.Name = "secimiOnaylaToolStripMenuItem";
		this.secimiOnaylaToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.secimiOnaylaToolStripMenuItem.Text = "Seçimi Onayla";
		this.secimiOnaylaToolStripMenuItem.Click += new System.EventHandler(secimiOnaylaToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(704, 399);
		base.Controls.Add(this.gridControl1);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "EvrakSecimi";
		this.Text = "Evrak Seçimi";
		base.Load += new System.EventHandler(Faturalar_Load);
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
