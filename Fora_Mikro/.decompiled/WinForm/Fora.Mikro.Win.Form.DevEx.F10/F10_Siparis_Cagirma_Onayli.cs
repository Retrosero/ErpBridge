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
using Fora.Mikro.Utility;

namespace Fora.Mikro.Win.Form.DevEx.F10;

public class F10_Siparis_Cagirma_Onayli : XtraForm
{
	private SqlBaglantiBilgileri _BaglantiBilgileri;

	private string _DBName;

	public int _RECno;

	public string _SiparisSeri = "";

	public int _SiparisSira;

	private IContainer components;

	private GridControl gridControl1;

	private GridView gridView1;

	private GridColumn gridColumn_SiparisSeri;

	private GridColumn gridColumn_SiparisSira;

	private GridColumn gridColumn_SiparisKarsilanmaTarihi;

	private GridColumn gridColumn_MikroyaAktarilmaTarihi;

	private GridColumn gridColumn_MikroyaAktarildi;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem GorunumToolStripMenuItem;

	private ToolStripMenuItem GorunumuSaklaToolStripMenuItem;

	private GridColumn gridColumn_KayitNo;

	private GridColumn gridColumn_EvrakTipi;

	private GridColumn gridColumn_MikroSeri;

	private GridColumn gridColumn_MikroSira;

	public F10_Siparis_Cagirma_Onayli(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string SiparisSeri)
	{
		_BaglantiBilgileri = BaglantiBilgileri;
		_DBName = DBName;
		_SiparisSeri = SiparisSeri;
		InitializeComponent();
	}

	private void F10_Siparis_Load(object sender, EventArgs e)
	{
		gridView1.Appearance.FocusedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		if (File.Exists("SiparisCagirmaOnayli.xml"))
		{
			gridView1.RestoreLayoutFromXml("SiparisCagirmaOnayli.xml");
		}
		DataTable siparislerOnayli = SiparislerData.GetSiparislerOnayli(_BaglantiBilgileri, _DBName, _SiparisSeri);
		gridControl1.DataSource = siparislerOnayli;
	}

	private void gridView1_RowStyle(object sender, RowStyleEventArgs e)
	{
		GridView gridView = sender as GridView;
		if (e.RowHandle >= 0)
		{
			if (gridView.GetRowCellDisplayText(e.RowHandle, gridView.Columns["MikroyaAktarildiYazi"]) == "Evet")
			{
				e.Appearance.BackColor = Color.LightGreen;
				e.Appearance.BackColor2 = Color.LightGreen;
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
		_RECno = int.Parse(gridView1.GetDataRow(Index)["RECno"].ToString());
		_SiparisSeri = gridView1.GetDataRow(Index)["SiparisSeriNo"].ToString();
		_SiparisSira = int.Parse(gridView1.GetDataRow(Index)["SiparisSiraNo"].ToString());
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void GorunumuSaklaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		gridView1.SaveLayoutToXml("SiparisCagirmaOnayli.xml");
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
		this.gridColumn_MikroyaAktarildi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_KayitNo = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_SiparisSeri = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_SiparisSira = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_SiparisKarsilanmaTarihi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_MikroyaAktarilmaTarihi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.GorunumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.GorunumuSaklaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.gridColumn_EvrakTipi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_MikroSeri = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gridColumn_MikroSira = new DevExpress.XtraGrid.Columns.GridColumn();
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
		this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[9] { this.gridColumn_MikroyaAktarildi, this.gridColumn_KayitNo, this.gridColumn_SiparisSeri, this.gridColumn_SiparisSira, this.gridColumn_SiparisKarsilanmaTarihi, this.gridColumn_EvrakTipi, this.gridColumn_MikroSeri, this.gridColumn_MikroSira, this.gridColumn_MikroyaAktarilmaTarihi });
		this.gridView1.GridControl = this.gridControl1;
		this.gridView1.Name = "gridView1";
		this.gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
		this.gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
		this.gridView1.OptionsBehavior.Editable = false;
		this.gridView1.OptionsBehavior.ReadOnly = true;
		this.gridView1.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(gridView1_RowStyle);
		this.gridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(gridView1_KeyDown);
		this.gridView1.DoubleClick += new System.EventHandler(gridView1_DoubleClick);
		this.gridColumn_MikroyaAktarildi.Caption = "Mikroya Aktarıldı";
		this.gridColumn_MikroyaAktarildi.FieldName = "MikroyaAktarildiYazi";
		this.gridColumn_MikroyaAktarildi.Name = "gridColumn_MikroyaAktarildi";
		this.gridColumn_MikroyaAktarildi.Visible = true;
		this.gridColumn_MikroyaAktarildi.VisibleIndex = 0;
		this.gridColumn_KayitNo.Caption = "Kayıt No";
		this.gridColumn_KayitNo.FieldName = "RECno";
		this.gridColumn_KayitNo.Name = "gridColumn_KayitNo";
		this.gridColumn_KayitNo.Visible = true;
		this.gridColumn_KayitNo.VisibleIndex = 1;
		this.gridColumn_SiparisSeri.Caption = "Sipariş Seri";
		this.gridColumn_SiparisSeri.FieldName = "SiparisSeriNo";
		this.gridColumn_SiparisSeri.Name = "gridColumn_SiparisSeri";
		this.gridColumn_SiparisSeri.Visible = true;
		this.gridColumn_SiparisSeri.VisibleIndex = 2;
		this.gridColumn_SiparisSira.Caption = "Sipariş Sıra";
		this.gridColumn_SiparisSira.FieldName = "SiparisSiraNo";
		this.gridColumn_SiparisSira.Name = "gridColumn_SiparisSira";
		this.gridColumn_SiparisSira.Visible = true;
		this.gridColumn_SiparisSira.VisibleIndex = 3;
		this.gridColumn_SiparisKarsilanmaTarihi.Caption = "Sipariş Karşılanma Tarihi";
		this.gridColumn_SiparisKarsilanmaTarihi.FieldName = "SiparisKarsilanmaTarihi";
		this.gridColumn_SiparisKarsilanmaTarihi.Name = "gridColumn_SiparisKarsilanmaTarihi";
		this.gridColumn_SiparisKarsilanmaTarihi.Visible = true;
		this.gridColumn_SiparisKarsilanmaTarihi.VisibleIndex = 4;
		this.gridColumn_MikroyaAktarilmaTarihi.Caption = "Mikroya Aktarılma Tarihi";
		this.gridColumn_MikroyaAktarilmaTarihi.FieldName = "MikroyaAktarilmaTarihi";
		this.gridColumn_MikroyaAktarilmaTarihi.Name = "gridColumn_MikroyaAktarilmaTarihi";
		this.gridColumn_MikroyaAktarilmaTarihi.Visible = true;
		this.gridColumn_MikroyaAktarilmaTarihi.VisibleIndex = 8;
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
		this.gridColumn_EvrakTipi.Caption = "Evrak Tipi";
		this.gridColumn_EvrakTipi.FieldName = "EvrakTipi";
		this.gridColumn_EvrakTipi.Name = "gridColumn_EvrakTipi";
		this.gridColumn_EvrakTipi.Visible = true;
		this.gridColumn_EvrakTipi.VisibleIndex = 5;
		this.gridColumn_MikroSeri.Caption = "Seri No";
		this.gridColumn_MikroSeri.FieldName = "MikroSeriNo";
		this.gridColumn_MikroSeri.Name = "gridColumn_MikroSeri";
		this.gridColumn_MikroSeri.Visible = true;
		this.gridColumn_MikroSeri.VisibleIndex = 6;
		this.gridColumn_MikroSira.Caption = "Sıra No";
		this.gridColumn_MikroSira.FieldName = "MikroSiraNo";
		this.gridColumn_MikroSira.Name = "gridColumn_MikroSira";
		this.gridColumn_MikroSira.Visible = true;
		this.gridColumn_MikroSira.VisibleIndex = 7;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(917, 473);
		base.Controls.Add(this.gridControl1);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "F10_Siparis_Cagirma_Onayli";
		this.Text = "Sipariş Çağırma Onaylı";
		base.Load += new System.EventHandler(F10_Siparis_Load);
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
