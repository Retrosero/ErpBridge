using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Siparis;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Siparis_Cagirma : XtraForm
{
	private SqlBaglantiBilgileri _BaglantiBilgileri;

	private string _DBName;

	private enum_sip_tip _sip_tip;

	private enum_sip_cins _sip_cins;

	public string _SiparisSeri = "";

	public int _SiparisSira;

	private string _CariKodu;

	private enum_sip_orderby _Siralama;

	private IContainer components;

	private GridControl gridControl1;

	private GridView gridView1;

	private GridColumn gridColumn_Seri;

	private GridColumn gridColumn_Sira;

	private GridColumn gridColumn_SiparisTarihi;

	private GridColumn gridColumn_TeslimTarihi;

	private GridColumn gridColumn_Tip;

	private GridColumn gridColumn_Cinsi;

	private GridColumn gridColumn_carikodu;

	private GridColumn gridColumn_CariIsmi;

	private GridColumn gridColumn_Miktar;

	private GridColumn gridColumn_Tutar;

	private GridColumn gridColumn_KalanMiktar;

	private GridColumn gridColumn_SatirSayisi;

	private GridColumn gridColumn_TeslimMiktar;

	private GridColumn gridColumn_Karsilandi;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem GorunumToolStripMenuItem;

	private ToolStripMenuItem GorunumuSaklaToolStripMenuItem;

	public F10_Siparis_Cagirma(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, enum_sip_tip SiparisTipi, enum_sip_cins SiparisCinsi, string SiparisSeri, string CariKodu, enum_sip_orderby Siralama, bool KarsilanmisSiparisSecebilir)
	{
		_BaglantiBilgileri = BaglantiBilgileri;
		_DBName = DBName;
		_sip_tip = SiparisTipi;
		_sip_cins = SiparisCinsi;
		_SiparisSeri = SiparisSeri;
		_CariKodu = CariKodu;
		_Siralama = Siralama;
		InitializeComponent();
	}

	private void F10_Siparis_Load(object sender, EventArgs e)
	{
		gridView1.Appearance.FocusedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		if (File.Exists("SiparisCagirma.xml"))
		{
			gridView1.RestoreLayoutFromXml("SiparisCagirma.xml");
		}
		DataTable siparisler = SiparislerData.GetSiparisler(_BaglantiBilgileri, _DBName, _sip_tip, _sip_cins, _SiparisSeri, _CariKodu, _Siralama);
		gridControl1.DataSource = siparisler;
	}

	private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
	{
		GridView gridView = sender as GridView;
		if (e.RowHandle >= 0)
		{
			double num = double.Parse(gridView.GetRowCellDisplayText(e.RowHandle, gridView.Columns["KalanMiktar"]));
			double num2 = double.Parse(gridView.GetRowCellDisplayText(e.RowHandle, gridView.Columns["sip_teslim_miktar"]));
			if (num == 0.0)
			{
				e.Appearance.BackColor = Color.LightGreen;
				e.Appearance.BackColor2 = Color.LightGreen;
			}
			else if (num2 > 0.0)
			{
				e.Appearance.BackColor = Color.Yellow;
				e.Appearance.BackColor2 = Color.Yellow;
			}
			else
			{
				e.Appearance.BackColor = Color.Salmon;
				e.Appearance.BackColor2 = Color.Salmon;
			}
		}
	}

	private void gridView1_DoubleClick(object sender, EventArgs e)
	{
		GridView gridView = (GridView)sender;
		Point pt = gridView.GridControl.PointToClient(Control.MousePosition);
		DoRowDoubleClick(gridView, pt);
	}

	private void DoRowDoubleClick(GridView view, Point pt)
	{
		GridHitInfo gridHitInfo = view.CalcHitInfo(pt);
		if (gridHitInfo.InRow || gridHitInfo.InRowCell)
		{
			if (gridHitInfo.Column != null)
			{
				gridHitInfo.Column.GetCaption();
			}
			int rowHandle = gridHitInfo.RowHandle;
			SiparisSecildi(rowHandle);
		}
	}

	private void gridView1_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SiparisSecildi(gridView1.GetSelectedRows()[0]);
		}
		if (e.KeyCode == Keys.Escape)
		{
			base.DialogResult = DialogResult.Abort;
			Close();
		}
	}

	private void SiparisSecildi(int Index)
	{
		_SiparisSeri = gridView1.GetDataRow(Index)["sip_evrakno_seri"].ToString();
		_SiparisSira = int.Parse(gridView1.GetDataRow(Index)["sip_evrakno_sira"].ToString());
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void GorunumuSaklaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		gridView1.SaveLayoutToXml("SiparisCagirma.xml");
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
		this.gridColumn_Karsilandi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Seri = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Sira = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_SiparisTarihi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_TeslimTarihi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Tip = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Cinsi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_carikodu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_CariIsmi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Miktar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_Tutar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_TeslimMiktar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_KalanMiktar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_SatirSayisi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.GorunumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.GorunumuSaklaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.gridControl1.Location = new System.Drawing.Point(0, 24);
		this.gridControl1.MainView = this.gridView1;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(917, 449);
		this.gridControl1.TabIndex = 0;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gridView1 });
		this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[14]
		{
			this.gridColumn_Karsilandi, this.gridColumn_Seri, this.gridColumn_Sira, this.gridColumn_SiparisTarihi, this.gridColumn_TeslimTarihi, this.gridColumn_Tip, this.gridColumn_Cinsi, this.gridColumn_carikodu, this.gridColumn_CariIsmi, this.gridColumn_Miktar,
			this.gridColumn_Tutar, this.gridColumn_TeslimMiktar, this.gridColumn_KalanMiktar, this.gridColumn_SatirSayisi
		});
		this.gridView1.GridControl = this.gridControl1;
		this.gridView1.Name = "gridView1";
		this.gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
		this.gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
		this.gridView1.OptionsBehavior.Editable = false;
		this.gridView1.OptionsBehavior.ReadOnly = true;
		this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView1.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(gridView1_RowStyle);
		this.gridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(gridView1_KeyDown);
		this.gridView1.DoubleClick += new System.EventHandler(gridView1_DoubleClick);
		this.gridColumn_Karsilandi.Caption = "Karşılandı";
		this.gridColumn_Karsilandi.FieldName = "Karsilandi";
		this.gridColumn_Karsilandi.Name = "gridColumn_Karsilandi";
		this.gridColumn_Karsilandi.Visible = true;
		this.gridColumn_Karsilandi.VisibleIndex = 0;
		this.gridColumn_Seri.Caption = "Seri";
		this.gridColumn_Seri.FieldName = "sip_evrakno_seri";
		this.gridColumn_Seri.Name = "gridColumn_Seri";
		this.gridColumn_Seri.Visible = true;
		this.gridColumn_Seri.VisibleIndex = 1;
		this.gridColumn_Sira.Caption = "Sıra";
		this.gridColumn_Sira.FieldName = "sip_evrakno_sira";
		this.gridColumn_Sira.Name = "gridColumn_Sira";
		this.gridColumn_Sira.Visible = true;
		this.gridColumn_Sira.VisibleIndex = 2;
		this.gridColumn_SiparisTarihi.Caption = "Sipariş Tarihi";
		this.gridColumn_SiparisTarihi.FieldName = "sip_tarih";
		this.gridColumn_SiparisTarihi.Name = "gridColumn_SiparisTarihi";
		this.gridColumn_SiparisTarihi.Visible = true;
		this.gridColumn_SiparisTarihi.VisibleIndex = 3;
		this.gridColumn_TeslimTarihi.Caption = "Teslim Tarihi";
		this.gridColumn_TeslimTarihi.FieldName = "sip_teslim_tarih";
		this.gridColumn_TeslimTarihi.Name = "gridColumn_TeslimTarihi";
		this.gridColumn_TeslimTarihi.Visible = true;
		this.gridColumn_TeslimTarihi.VisibleIndex = 4;
		this.gridColumn_Tip.Caption = "Tipi";
		this.gridColumn_Tip.FieldName = "MetinSiparisTipi";
		this.gridColumn_Tip.Name = "gridColumn_Tip";
		this.gridColumn_Tip.Visible = true;
		this.gridColumn_Tip.VisibleIndex = 5;
		this.gridColumn_Cinsi.Caption = "Sipariş Cinsi";
		this.gridColumn_Cinsi.FieldName = "MetinSiparisCins";
		this.gridColumn_Cinsi.Name = "gridColumn_Cinsi";
		this.gridColumn_Cinsi.Visible = true;
		this.gridColumn_Cinsi.VisibleIndex = 6;
		this.gridColumn_carikodu.Caption = "Cari Kodu";
		this.gridColumn_carikodu.FieldName = "sip_musteri_kod";
		this.gridColumn_carikodu.Name = "gridColumn_carikodu";
		this.gridColumn_carikodu.Visible = true;
		this.gridColumn_carikodu.VisibleIndex = 7;
		this.gridColumn_CariIsmi.Caption = "Cari İsmi";
		this.gridColumn_CariIsmi.FieldName = "CariIsmi";
		this.gridColumn_CariIsmi.Name = "gridColumn_CariIsmi";
		this.gridColumn_CariIsmi.Visible = true;
		this.gridColumn_CariIsmi.VisibleIndex = 8;
		this.gridColumn_Miktar.Caption = "Miktar";
		this.gridColumn_Miktar.FieldName = "sip_miktar";
		this.gridColumn_Miktar.Name = "gridColumn_Miktar";
		this.gridColumn_Miktar.Visible = true;
		this.gridColumn_Miktar.VisibleIndex = 9;
		this.gridColumn_Tutar.Caption = "Tutar";
		this.gridColumn_Tutar.FieldName = "SiparisTutari";
		this.gridColumn_Tutar.Name = "gridColumn_Tutar";
		this.gridColumn_Tutar.Visible = true;
		this.gridColumn_Tutar.VisibleIndex = 10;
		this.gridColumn_TeslimMiktar.Caption = "Teslim Miktar";
		this.gridColumn_TeslimMiktar.FieldName = "sip_teslim_miktar";
		this.gridColumn_TeslimMiktar.Name = "gridColumn_TeslimMiktar";
		this.gridColumn_TeslimMiktar.Visible = true;
		this.gridColumn_TeslimMiktar.VisibleIndex = 11;
		this.gridColumn_KalanMiktar.Caption = "Kalan Miktar";
		this.gridColumn_KalanMiktar.FieldName = "KalanMiktar";
		this.gridColumn_KalanMiktar.Name = "gridColumn_KalanMiktar";
		this.gridColumn_KalanMiktar.Visible = true;
		this.gridColumn_KalanMiktar.VisibleIndex = 12;
		this.gridColumn_SatirSayisi.Caption = "Satır Sayısı";
		this.gridColumn_SatirSayisi.FieldName = "SatirSayisi";
		this.gridColumn_SatirSayisi.Name = "gridColumn_SatirSayisi";
		this.gridColumn_SatirSayisi.Visible = true;
		this.gridColumn_SatirSayisi.VisibleIndex = 13;
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.GorunumToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(917, 24);
		this.menuStrip1.TabIndex = 1;
		this.menuStrip1.Text = "menuStrip1";
		this.GorunumToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.GorunumuSaklaToolStripMenuItem });
		this.GorunumToolStripMenuItem.Name = "GorunumToolStripMenuItem";
		this.GorunumToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
		this.GorunumToolStripMenuItem.Text = "Görünüm";
		this.GorunumuSaklaToolStripMenuItem.Name = "GorunumuSaklaToolStripMenuItem";
		this.GorunumuSaklaToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.GorunumuSaklaToolStripMenuItem.Text = "Görünümü Sakla";
		this.GorunumuSaklaToolStripMenuItem.Click += new System.EventHandler(GorunumuSaklaToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(917, 473);
		base.Controls.Add(this.gridControl1);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "F10_Siparis_Cagirma";
		this.Text = "Sipariş Çağırma";
		base.Load += new System.EventHandler(F10_Siparis_Load);
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
