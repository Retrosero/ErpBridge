using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Fora.Mikro.Rapor.YapilacakTahsilat;

namespace Fora.Mikro.Win.Form.DevEx.Raporlar;

public class RaporYapilacakTahsilatlar : XtraReport
{
	private IContainer components;

	private DetailBand Detail;

	private TopMarginBand TopMargin;

	private BottomMarginBand BottomMargin;

	private BindingSource bindingSource1;

	private DetailReportBand DetailReport;

	private DetailBand Detail1;

	private ReportHeaderBand ReportHeader;

	private XRTable xrTable1;

	private XRTableRow xrTableRow1;

	private XRTableCell xrTableCell7;

	private XRTableCell xrTableCell5;

	private XRTableCell xrTableCell1;

	private XRTableCell xrTableCell2;

	private XRLabel xrLabel12;

	private XRTable xrTable2;

	private XRTableRow xrTableRow2;

	private XRTableCell xrTableCell9;

	private XRTableCell xrTableCell10;

	private XRTableCell xrTableCell11;

	private XRTableCell xrTableCell14;

	private XRTableCell xrTableCell4;

	private XRTableCell xrTableCell3;

	public RaporYapilacakTahsilatlar()
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
		bindingSource1 = new BindingSource(components);
		DetailReport = new DetailReportBand();
		Detail1 = new DetailBand();
		xrTable2 = new XRTable();
		xrTableRow2 = new XRTableRow();
		xrTableCell9 = new XRTableCell();
		xrTableCell10 = new XRTableCell();
		xrTableCell11 = new XRTableCell();
		xrTableCell14 = new XRTableCell();
		xrTableCell4 = new XRTableCell();
		ReportHeader = new ReportHeaderBand();
		xrTable1 = new XRTable();
		xrTableRow1 = new XRTableRow();
		xrTableCell7 = new XRTableCell();
		xrTableCell5 = new XRTableCell();
		xrTableCell1 = new XRTableCell();
		xrTableCell2 = new XRTableCell();
		xrTableCell3 = new XRTableCell();
		xrLabel12 = new XRLabel();
		((ISupportInitialize)bindingSource1).BeginInit();
		((ISupportInitialize)xrTable2).BeginInit();
		((ISupportInitialize)xrTable1).BeginInit();
		((ISupportInitialize)this).BeginInit();
		Detail.HeightF = 31.25f;
		Detail.Name = "Detail";
		Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		Detail.TextAlignment = TextAlignment.TopLeft;
		TopMargin.HeightF = 52f;
		TopMargin.Name = "TopMargin";
		TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		TopMargin.TextAlignment = TextAlignment.TopLeft;
		BottomMargin.HeightF = 100f;
		BottomMargin.Name = "BottomMargin";
		BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		BottomMargin.TextAlignment = TextAlignment.TopLeft;
		bindingSource1.DataSource = typeof(RaporYapilacakTahsilatlarSonuc);
		DetailReport.Bands.AddRange(new Band[2] { Detail1, ReportHeader });
		DetailReport.DataMember = "list_items";
		DetailReport.DataSource = bindingSource1;
		DetailReport.Level = 0;
		DetailReport.Name = "DetailReport";
		Detail1.Controls.AddRange(new XRControl[1] { xrTable2 });
		Detail1.HeightF = 20f;
		Detail1.Name = "Detail1";
		xrTable2.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable2.Font = new Font("Tahoma", 8f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable2.LocationFloat = new PointFloat(11.99989f, 0f);
		xrTable2.Name = "xrTable2";
		xrTable2.Rows.AddRange(new XRTableRow[1] { xrTableRow2 });
		xrTable2.SizeF = new SizeF(751.0002f, 20f);
		xrTable2.StylePriority.UseBorders = false;
		xrTable2.StylePriority.UseFont = false;
		xrTable2.StylePriority.UseTextAlignment = false;
		xrTable2.TextAlignment = TextAlignment.MiddleCenter;
		xrTableRow2.Cells.AddRange(new XRTableCell[5] { xrTableCell9, xrTableCell10, xrTableCell11, xrTableCell14, xrTableCell4 });
		xrTableRow2.Name = "xrTableRow2";
		xrTableRow2.Weight = 1.0;
		xrTableCell9.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.kodu")
		});
		xrTableCell9.Name = "xrTableCell9";
		xrTableCell9.Text = "xrTableCell9";
		xrTableCell9.Weight = 0.5366124417441072;
		xrTableCell10.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.adi")
		});
		xrTableCell10.Name = "xrTableCell10";
		xrTableCell10.Text = "xrTableCell10";
		xrTableCell10.Weight = 1.4334320915207528;
		xrTableCell11.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.OrtalamaVade", "{0:d.MM.yyyy}")
		});
		xrTableCell11.Name = "xrTableCell11";
		xrTableCell11.Weight = 0.5749927189313875;
		xrTableCell14.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.ToplamMeblag", "{0:n2}")
		});
		xrTableCell14.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell14.Name = "xrTableCell14";
		xrTableCell14.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell14.StylePriority.UseFont = false;
		xrTableCell14.StylePriority.UsePadding = false;
		xrTableCell14.StylePriority.UseTextAlignment = false;
		xrTableCell14.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell14.Weight = 0.47567317192813513;
		xrTableCell4.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.DovizKodu")
		});
		xrTableCell4.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell4.Name = "xrTableCell4";
		xrTableCell4.StylePriority.UseFont = false;
		xrTableCell4.Weight = 0.3210064922210336;
		ReportHeader.Controls.AddRange(new XRControl[2] { xrTable1, xrLabel12 });
		ReportHeader.HeightF = 54.99996f;
		ReportHeader.Name = "ReportHeader";
		xrTable1.BackColor = Color.Gainsboro;
		xrTable1.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable1.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTable1.LocationFloat = new PointFloat(10.00001f, 32.99996f);
		xrTable1.Name = "xrTable1";
		xrTable1.Rows.AddRange(new XRTableRow[1] { xrTableRow1 });
		xrTable1.SizeF = new SizeF(753.0001f, 22f);
		xrTable1.StylePriority.UseBackColor = false;
		xrTable1.StylePriority.UseBorders = false;
		xrTable1.StylePriority.UseFont = false;
		xrTable1.StylePriority.UseTextAlignment = false;
		xrTable1.TextAlignment = TextAlignment.MiddleCenter;
		xrTableRow1.Cells.AddRange(new XRTableCell[5] { xrTableCell7, xrTableCell5, xrTableCell1, xrTableCell2, xrTableCell3 });
		xrTableRow1.Name = "xrTableRow1";
		xrTableRow1.Weight = 1.0;
		xrTableCell7.Name = "xrTableCell7";
		xrTableCell7.Text = "Kod";
		xrTableCell7.Weight = 0.5366124249149627;
		xrTableCell5.Name = "xrTableCell5";
		xrTableCell5.Text = "İsim";
		xrTableCell5.Weight = 1.433432059441131;
		xrTableCell1.Name = "xrTableCell1";
		xrTableCell1.Text = "Ortalama Vade";
		xrTableCell1.Weight = 0.5771078034926098;
		xrTableCell2.Multiline = true;
		xrTableCell2.Name = "xrTableCell2";
		xrTableCell2.Text = "Tutar\r\n";
		xrTableCell2.Weight = 0.47355793607660346;
		xrTableCell3.Name = "xrTableCell3";
		xrTableCell3.Text = "Birim";
		xrTableCell3.Weight = 0.3210061667442986;
		xrLabel12.BackColor = Color.DarkGray;
		xrLabel12.Borders = BorderSide.All;
		xrLabel12.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel12.LocationFloat = new PointFloat(10.00001f, 10.00001f);
		xrLabel12.Name = "xrLabel12";
		xrLabel12.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel12.SizeF = new SizeF(753f, 23f);
		xrLabel12.StylePriority.UseBackColor = false;
		xrLabel12.StylePriority.UseBorders = false;
		xrLabel12.StylePriority.UseFont = false;
		xrLabel12.StylePriority.UseTextAlignment = false;
		xrLabel12.Text = "Yapılacak tahsilatlar raporu";
		xrLabel12.TextAlignment = TextAlignment.MiddleCenter;
		base.Bands.AddRange(new Band[4] { Detail, TopMargin, BottomMargin, DetailReport });
		base.DataSource = bindingSource1;
		base.Margins = new Margins(41, 34, 52, 100);
		base.Version = "14.1";
		((ISupportInitialize)bindingSource1).EndInit();
		((ISupportInitialize)xrTable2).EndInit();
		((ISupportInitialize)xrTable1).EndInit();
		((ISupportInitialize)this).EndInit();
	}
}
