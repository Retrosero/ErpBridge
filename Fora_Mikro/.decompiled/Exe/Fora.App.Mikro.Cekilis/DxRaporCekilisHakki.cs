using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace Fora.App.Mikro.Cekilis;

public class DxRaporCekilisHakki : XtraReport
{
	private IContainer components;

	private DetailBand Detail;

	private TopMarginBand TopMargin;

	private BottomMarginBand BottomMargin;

	private BindingSource bindingSource1;

	private XRLabel xrLabel2;

	private XRLabel xrLabel1;

	private DetailReportBand DetailReport;

	private DetailBand Detail1;

	private XRLabel xrLabel3;

	public DxRaporCekilisHakki()
	{
		InitializeComponent();
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
		components = new Container();
		Detail = new DetailBand();
		TopMargin = new TopMarginBand();
		BottomMargin = new BottomMarginBand();
		xrLabel1 = new XRLabel();
		xrLabel2 = new XRLabel();
		DetailReport = new DetailReportBand();
		Detail1 = new DetailBand();
		xrLabel3 = new XRLabel();
		bindingSource1 = new BindingSource(components);
		((ISupportInitialize)bindingSource1).BeginInit();
		((ISupportInitialize)this).BeginInit();
		Detail.Controls.AddRange(new XRControl[2] { xrLabel1, xrLabel2 });
		Detail.HeightF = 71.875f;
		Detail.Name = "Detail";
		Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		Detail.TextAlignment = TextAlignment.TopLeft;
		TopMargin.HeightF = 51f;
		TopMargin.Name = "TopMargin";
		TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		TopMargin.TextAlignment = TextAlignment.TopLeft;
		BottomMargin.Name = "BottomMargin";
		BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		BottomMargin.TextAlignment = TextAlignment.TopLeft;
		xrLabel1.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari.cari_unvan1")
		});
		xrLabel1.Font = new Font("Tahoma", 12f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel1.LocationFloat = new PointFloat(0f, 10.00001f);
		xrLabel1.Name = "xrLabel1";
		xrLabel1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel1.SizeF = new SizeF(710f, 23f);
		xrLabel1.StylePriority.UseFont = false;
		xrLabel1.Text = "xrLabel1";
		xrLabel2.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari.cari_unvan2")
		});
		xrLabel2.Font = new Font("Tahoma", 12f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel2.LocationFloat = new PointFloat(0f, 32.99999f);
		xrLabel2.Name = "xrLabel2";
		xrLabel2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel2.SizeF = new SizeF(710f, 23f);
		xrLabel2.StylePriority.UseFont = false;
		xrLabel2.Text = "xrLabel2";
		DetailReport.Bands.AddRange(new Band[1] { Detail1 });
		DetailReport.DataMember = "cekilis_numaralari";
		DetailReport.DataSource = bindingSource1;
		DetailReport.Level = 0;
		DetailReport.Name = "DetailReport";
		Detail1.Controls.AddRange(new XRControl[1] { xrLabel3 });
		Detail1.HeightF = 23f;
		Detail1.MultiColumn.ColumnCount = 7;
		Detail1.MultiColumn.Layout = ColumnLayout.AcrossThenDown;
		Detail1.MultiColumn.Mode = MultiColumnMode.UseColumnCount;
		Detail1.Name = "Detail1";
		xrLabel3.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cekilis_numaralari.Kod")
		});
		xrLabel3.Font = new Font("Tahoma", 12f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel3.LocationFloat = new PointFloat(0f, 0f);
		xrLabel3.Name = "xrLabel3";
		xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel3.SizeF = new SizeF(95.41664f, 23f);
		xrLabel3.StylePriority.UseFont = false;
		xrLabel3.Text = "xrLabel3";
		bindingSource1.DataSource = typeof(CekilisListe);
		base.Bands.AddRange(new Band[4] { Detail, TopMargin, BottomMargin, DetailReport });
		base.DataSource = bindingSource1;
		base.Margins = new Margins(51, 56, 51, 100);
		base.PageHeight = 1169;
		base.PageWidth = 827;
		base.PaperKind = PaperKind.A4;
		base.Version = "13.1";
		((ISupportInitialize)bindingSource1).EndInit();
		((ISupportInitialize)this).EndInit();
	}
}
