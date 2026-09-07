using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Win.Form.DevEx.Controllers;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Fora.App.Mikro.ForaAndroid;

public class TemsilciRutDuzenleme : Form
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private List<CariRotaListItem> liste;

	private PointLatLng merkez_lokasyon;

	private GMapOverlay marker_overlay;

	private GMapOverlay routes_Overlay;

	private List<GMapMarker> markers = new List<GMapMarker>();

	private GMapRoute routes = new GMapRoute("RUT");

	private IContainer components;

	private CariPersonelSecimi cariPersonelSecimi1;

	private GMapControl MapControl;

	private System.Windows.Forms.ComboBox Gun;

	private SimpleButton simpleButton1;

	private GridControl gridControl1;

	private BindingSource bindingSource1;

	private GridView gridView1;

	private GridColumn colcari_kod;

	private GridColumn colcari_unvan1;

	private GridColumn colcari_unvan2;

	private GridColumn colCariAdres;

	private GridColumn colcari_hareket_tipi;

	private GridColumn colcari_tipi;

	public TemsilciRutDuzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		cariPersonelSecimi1._mikrouygulamabilgileri = _mikrouygulamabilgileri;
		DataTable dataSource = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Pazartesi" },
				new object[2] { 1, "Salı" },
				new object[2] { 2, "Çarşamba" },
				new object[2] { 3, "Perşembe" },
				new object[2] { 4, "Cuma" },
				new object[2] { 5, "Cumartesi" },
				new object[2] { 6, "Pazar" }
			}
		};
		Gun.DataSource = dataSource;
		Gun.ValueMember = "ID";
		Gun.DisplayMember = "Isim";
		merkez_lokasyon = new PointLatLng(41.061958, 28.728958);
		MapControl.MapProvider = BingHybridMapProvider.Instance;
		MapControl.Manager.Mode = AccessMode.ServerAndCache;
		MapControl.Position = new PointLatLng(39.406633, 35.032354);
		MapControl.MinZoom = 3;
		MapControl.MaxZoom = 17;
		MapControl.Zoom = 6.0;
		marker_overlay = new GMapOverlay("CARİLER");
		routes_Overlay = new GMapOverlay("Rotalar");
		MapControl.Overlays.Add(marker_overlay);
		MapControl.Overlays.Add(routes_Overlay);
		markers = new List<GMapMarker>();
		routes = new GMapRoute("RUT");
	}

	private void simpleButton1_Click(object sender, EventArgs e)
	{
		ListeOlustur();
	}

	private void ListeOlustur()
	{
		liste = new List<CariRotaListItem>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		int num = (int)Gun.SelectedValue;
		DateTime tarih = DateTime.Now;
		switch (num)
		{
		case 0:
			tarih = StartOfWeek(DateTime.Now, DayOfWeek.Monday);
			break;
		case 1:
			tarih = StartOfWeek(DateTime.Now, DayOfWeek.Tuesday);
			break;
		case 2:
			tarih = StartOfWeek(DateTime.Now, DayOfWeek.Wednesday);
			break;
		case 3:
			tarih = StartOfWeek(DateTime.Now, DayOfWeek.Tuesday);
			break;
		case 4:
			tarih = StartOfWeek(DateTime.Now, DayOfWeek.Friday);
			break;
		case 5:
			tarih = StartOfWeek(DateTime.Now, DayOfWeek.Saturday);
			break;
		case 6:
			tarih = StartOfWeek(DateTime.Now, DayOfWeek.Sunday);
			break;
		}
		try
		{
			liste = TemsilciData.GetCariRotaListItems(sqlDB.Connection, cariPersonelSecimi1.SelectedItem.cari_per_kod, tarih);
		}
		catch (Exception)
		{
		}
		sqlDB.ConnectionClose();
		gridControl1.DataSource = liste;
		HaritaHazirla();
	}

	private void HaritaHazirla()
	{
		foreach (GMapMarker marker in markers)
		{
			marker_overlay.Markers.Remove(marker);
		}
		markers = new List<GMapMarker>();
		GMapMarker gMapMarker = new GMarkerGoogle(merkez_lokasyon, GMarkerGoogleType.green);
		gMapMarker.ToolTipMode = MarkerTooltipMode.Always;
		gMapMarker.ToolTipText = "MERKEZ";
		markers.Add(gMapMarker);
		foreach (CariRotaListItem item in liste)
		{
			GMapMarker gMapMarker2 = new GMarkerGoogle(new PointLatLng(item.CariAdres.adr_gps_enlem, item.CariAdres.adr_gps_boylam), GMarkerGoogleType.blue_small);
			gMapMarker2.ToolTipMode = MarkerTooltipMode.Always;
			gMapMarker2.ToolTipText = item.cari_unvan1;
			markers.Add(gMapMarker2);
		}
		foreach (GMapMarker marker2 in markers)
		{
			marker_overlay.Markers.Add(marker2);
		}
		MapControl.ZoomAndCenterMarkers(null);
		routes_Overlay.Routes.Remove(routes);
		List<PointLatLng> list = new List<PointLatLng>();
		list.Add(merkez_lokasyon);
		foreach (CariRotaListItem item2 in liste)
		{
			list.Add(new PointLatLng(item2.CariAdres.adr_gps_enlem, item2.CariAdres.adr_gps_boylam));
		}
		routes = new GMapRoute(list, "RUT");
		routes.Stroke = new Pen(Color.Red, 3f);
		routes_Overlay = new GMapOverlay("routes");
		routes_Overlay.Routes.Add(routes);
		MapControl.Overlays.Add(routes_Overlay);
	}

	public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
	{
		int num = dt.DayOfWeek - startOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		return dt.AddDays(-1 * num).Date;
	}

	private void MapControl_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (MessageBox.Show(liste[gridView1.GetFocusedDataSourceRowIndex()].cari_unvan1 + " lokasyonu değiştirilecek emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			PointLatLng pointLatLng = MapControl.FromLocalToLatLng(e.X, e.Y);
			CariAdres cariAdres = liste[gridView1.GetFocusedDataSourceRowIndex()].CariAdres;
			cariAdres.adr_gps_enlem = (float)pointLatLng.Lat;
			cariAdres.adr_gps_boylam = (float)pointLatLng.Lng;
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			CariData.SetCariLokasyon(sqlDB.Connection, cariAdres);
			sqlDB.ConnectionClose();
			ListeOlustur();
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
		this.cariPersonelSecimi1 = new Fora.Mikro.Win.Form.DevEx.Controllers.CariPersonelSecimi();
		this.MapControl = new GMap.NET.WindowsForms.GMapControl();
		this.Gun = new System.Windows.Forms.ComboBox();
		this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
		this.gridControl1 = new DevExpress.XtraGrid.GridControl();
		this.bindingSource1 = new System.Windows.Forms.BindingSource();
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.colcari_kod = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colcari_unvan1 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colcari_unvan2 = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colCariAdres = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colcari_hareket_tipi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.colcari_tipi = new DevExpress.XtraGrid.Columns.GridColumn();
		((System.ComponentModel.ISupportInitialize)this.gridControl1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.bindingSource1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		base.SuspendLayout();
		this.cariPersonelSecimi1._mikrouygulamabilgileri = null;
		this.cariPersonelSecimi1.CodeEmptyMessage = "Cari personel seçiniz";
		this.cariPersonelSecimi1.EnabledCode = true;
		this.cariPersonelSecimi1.EnabledName = true;
		this.cariPersonelSecimi1.FontCode = new System.Drawing.Font("Tahoma", 8.25f);
		this.cariPersonelSecimi1.FontCodeLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.cariPersonelSecimi1.FontName = new System.Drawing.Font("Tahoma", 8.25f);
		this.cariPersonelSecimi1.FontNameLabel = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 162);
		this.cariPersonelSecimi1.Location = new System.Drawing.Point(12, 12);
		this.cariPersonelSecimi1.LocationCode = new System.Drawing.Point(110, 0);
		this.cariPersonelSecimi1.LocationCodeLabel = new System.Drawing.Point(3, 3);
		this.cariPersonelSecimi1.LocationName = new System.Drawing.Point(323, 0);
		this.cariPersonelSecimi1.LocationNameLabel = new System.Drawing.Point(226, 3);
		this.cariPersonelSecimi1.Name = "cariPersonelSecimi1";
		this.cariPersonelSecimi1.NameEmptyMessage = "Cari personel seçiniz";
		this.cariPersonelSecimi1.SelectedItem = null;
		this.cariPersonelSecimi1.Size = new System.Drawing.Size(223, 20);
		this.cariPersonelSecimi1.SizeCode = new System.Drawing.Size(110, 20);
		this.cariPersonelSecimi1.SizeName = new System.Drawing.Size(172, 20);
		this.cariPersonelSecimi1.TabIndex = 0;
		this.cariPersonelSecimi1.TextCodeLabel = "Cari personel kodu :";
		this.cariPersonelSecimi1.TextNameLabel = "Cari personel adı :";
		this.cariPersonelSecimi1.VisibleCode = true;
		this.cariPersonelSecimi1.VisibleCodeLabel = true;
		this.cariPersonelSecimi1.VisibleName = false;
		this.cariPersonelSecimi1.VisibleNameLabel = false;
		this.MapControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.MapControl.Bearing = 0f;
		this.MapControl.CanDragMap = true;
		this.MapControl.GrayScaleMode = false;
		this.MapControl.LevelsKeepInMemmory = 5;
		this.MapControl.Location = new System.Drawing.Point(12, 216);
		this.MapControl.MarkersEnabled = true;
		this.MapControl.MaxZoom = 2;
		this.MapControl.MinZoom = 2;
		this.MapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
		this.MapControl.Name = "MapControl";
		this.MapControl.NegativeMode = false;
		this.MapControl.PolygonsEnabled = true;
		this.MapControl.RetryLoadTile = 0;
		this.MapControl.RoutesEnabled = true;
		this.MapControl.ShowTileGridLines = false;
		this.MapControl.Size = new System.Drawing.Size(824, 323);
		this.MapControl.TabIndex = 5;
		this.MapControl.Zoom = 0.0;
		this.MapControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MapControl_MouseDoubleClick);
		this.Gun.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.Gun.FormattingEnabled = true;
		this.Gun.Location = new System.Drawing.Point(241, 11);
		this.Gun.Name = "Gun";
		this.Gun.Size = new System.Drawing.Size(106, 21);
		this.Gun.TabIndex = 169;
		this.simpleButton1.Location = new System.Drawing.Point(353, 9);
		this.simpleButton1.Name = "simpleButton1";
		this.simpleButton1.Size = new System.Drawing.Size(75, 23);
		this.simpleButton1.TabIndex = 170;
		this.simpleButton1.Text = "RUT GETİR";
		this.simpleButton1.Click += new System.EventHandler(simpleButton1_Click);
		this.gridControl1.Cursor = System.Windows.Forms.Cursors.Default;
		this.gridControl1.DataSource = this.bindingSource1;
		this.gridControl1.Location = new System.Drawing.Point(12, 38);
		this.gridControl1.MainView = this.gridView1;
		this.gridControl1.Name = "gridControl1";
		this.gridControl1.Size = new System.Drawing.Size(824, 172);
		this.gridControl1.TabIndex = 171;
		this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gridView1 });
		this.bindingSource1.DataSource = typeof(Fora.Mikro.CariHesaplar.CariRotaListItem);
		this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[6] { this.colcari_kod, this.colcari_unvan1, this.colcari_unvan2, this.colCariAdres, this.colcari_hareket_tipi, this.colcari_tipi });
		this.gridView1.GridControl = this.gridControl1;
		this.gridView1.Name = "gridView1";
		this.colcari_kod.FieldName = "cari_kod";
		this.colcari_kod.Name = "colcari_kod";
		this.colcari_kod.Visible = true;
		this.colcari_kod.VisibleIndex = 0;
		this.colcari_unvan1.FieldName = "cari_unvan1";
		this.colcari_unvan1.Name = "colcari_unvan1";
		this.colcari_unvan1.Visible = true;
		this.colcari_unvan1.VisibleIndex = 1;
		this.colcari_unvan2.FieldName = "cari_unvan2";
		this.colcari_unvan2.Name = "colcari_unvan2";
		this.colcari_unvan2.Visible = true;
		this.colcari_unvan2.VisibleIndex = 2;
		this.colCariAdres.FieldName = "CariAdres";
		this.colCariAdres.Name = "colCariAdres";
		this.colCariAdres.Visible = true;
		this.colCariAdres.VisibleIndex = 3;
		this.colcari_hareket_tipi.FieldName = "cari_hareket_tipi";
		this.colcari_hareket_tipi.Name = "colcari_hareket_tipi";
		this.colcari_hareket_tipi.Visible = true;
		this.colcari_hareket_tipi.VisibleIndex = 4;
		this.colcari_tipi.FieldName = "cari_tipi";
		this.colcari_tipi.Name = "colcari_tipi";
		this.colcari_tipi.Visible = true;
		this.colcari_tipi.VisibleIndex = 5;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(848, 551);
		base.Controls.Add(this.gridControl1);
		base.Controls.Add(this.simpleButton1);
		base.Controls.Add(this.Gun);
		base.Controls.Add(this.MapControl);
		base.Controls.Add(this.cariPersonelSecimi1);
		base.Name = "TemsilciRutDuzenleme";
		this.Text = "Temsilci rut düzenleme";
		((System.ComponentModel.ISupportInitialize)this.gridControl1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.bindingSource1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		base.ResumeLayout(false);
	}
}
