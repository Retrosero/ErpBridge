using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Fora.Mikro.CariHesaplar.CariEkstresi;

namespace Fora.Mikro.Win.Form.DevEx.Raporlar;

public class RaporEkstre : XtraReport
{
	private IContainer components;

	private DetailBand Ust_Bilgiler;

	private TopMarginBand TopMargin;

	private BottomMarginBand BottomMargin;

	private DetailReportBand DetailReport;

	private DetailBand Detail1;

	private BindingSource bindingSource1;

	private XRLabel xrLabel3;

	private XRLabel xrLabel2;

	private XRLabel xrLabel1;

	private DetailReportBand DetailReport1;

	private DetailBand Detail;

	private ReportHeaderBand ReportHeader1;

	private XRLabel xrLabel12;

	private XRTable xrTable2;

	private XRTableRow xrTableRow2;

	private XRTableCell xrTableCell8;

	private XRTableCell xrTableCell9;

	private XRTableCell xrTableCell10;

	private XRTableCell xrTableCell11;

	private XRTableCell xrTableCell12;

	private XRTableCell xrTableCell13;

	private XRTableCell xrTableCell14;

	private XRTable xrTable1;

	private XRTableRow xrTableRow1;

	private XRTableCell xrTableCell7;

	private XRTableCell xrTableCell5;

	private XRTableCell xrTableCell1;

	private XRTableCell xrTableCell6;

	private XRTableCell xrTableCell2;

	private XRTableCell xrTableCell4;

	private XRTableCell xrTableCell3;

	private XRLabel xrLabel5;

	private XRLabel xrLabel4;

	private ReportFooterBand ReportFooter;

	private XRLabel xrLabel6;

	private XRLabel xrLabel7;

	public RaporEkstre()
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
		Ust_Bilgiler = new DetailBand();
		xrLabel7 = new XRLabel();
		xrLabel5 = new XRLabel();
		xrLabel4 = new XRLabel();
		xrLabel3 = new XRLabel();
		xrLabel2 = new XRLabel();
		xrLabel1 = new XRLabel();
		TopMargin = new TopMarginBand();
		BottomMargin = new BottomMarginBand();
		DetailReport = new DetailReportBand();
		Detail1 = new DetailBand();
		DetailReport1 = new DetailReportBand();
		Detail = new DetailBand();
		xrTable2 = new XRTable();
		xrTableRow2 = new XRTableRow();
		xrTableCell8 = new XRTableCell();
		xrTableCell9 = new XRTableCell();
		xrTableCell10 = new XRTableCell();
		xrTableCell11 = new XRTableCell();
		xrTableCell12 = new XRTableCell();
		xrTableCell13 = new XRTableCell();
		xrTableCell14 = new XRTableCell();
		ReportHeader1 = new ReportHeaderBand();
		xrTable1 = new XRTable();
		xrTableRow1 = new XRTableRow();
		xrTableCell7 = new XRTableCell();
		xrTableCell5 = new XRTableCell();
		xrTableCell1 = new XRTableCell();
		xrTableCell6 = new XRTableCell();
		xrTableCell2 = new XRTableCell();
		xrTableCell4 = new XRTableCell();
		xrTableCell3 = new XRTableCell();
		xrLabel12 = new XRLabel();
		ReportFooter = new ReportFooterBand();
		xrLabel6 = new XRLabel();
		bindingSource1 = new BindingSource();
		((ISupportInitialize)xrTable2).BeginInit();
		((ISupportInitialize)xrTable1).BeginInit();
		((ISupportInitialize)bindingSource1).BeginInit();
		((ISupportInitialize)this).BeginInit();
		Ust_Bilgiler.Controls.AddRange(new XRControl[6] { xrLabel7, xrLabel5, xrLabel4, xrLabel3, xrLabel2, xrLabel1 });
		Ust_Bilgiler.HeightF = 131.25f;
		Ust_Bilgiler.Name = "Ust_Bilgiler";
		Ust_Bilgiler.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		Ust_Bilgiler.TextAlignment = TextAlignment.TopLeft;
		xrLabel7.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "tarih", "{0:dd.MM.yyyy}")
		});
		xrLabel7.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel7.LocationFloat = new PointFloat(598.9166f, 102f);
		xrLabel7.Name = "xrLabel7";
		xrLabel7.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel7.SizeF = new SizeF(177.0833f, 23f);
		xrLabel7.StylePriority.UseFont = false;
		xrLabel7.StylePriority.UseTextAlignment = false;
		xrLabel7.Text = "xrLabel7";
		xrLabel7.TextAlignment = TextAlignment.MiddleRight;
		xrLabel5.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel5.LocationFloat = new PointFloat(0f, 79.00002f);
		xrLabel5.Name = "xrLabel5";
		xrLabel5.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel5.SizeF = new SizeF(89.58335f, 23f);
		xrLabel5.StylePriority.UseFont = false;
		xrLabel5.StylePriority.UseTextAlignment = false;
		xrLabel5.Text = "Cari ünvan :";
		xrLabel5.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel4.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel4.LocationFloat = new PointFloat(0f, 56.00001f);
		xrLabel4.Name = "xrLabel4";
		xrLabel4.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel4.SizeF = new SizeF(89.58335f, 23f);
		xrLabel4.StylePriority.UseFont = false;
		xrLabel4.StylePriority.UseTextAlignment = false;
		xrLabel4.Text = "Cari kodu   :";
		xrLabel4.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel3.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari.cari_unvan2")
		});
		xrLabel3.Font = new Font("Tahoma", 10f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrLabel3.LocationFloat = new PointFloat(89.58337f, 102f);
		xrLabel3.Name = "xrLabel3";
		xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel3.SizeF = new SizeF(460.4166f, 23f);
		xrLabel3.StylePriority.UseFont = false;
		xrLabel3.StylePriority.UseTextAlignment = false;
		xrLabel3.Text = "xrLabel3";
		xrLabel3.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel2.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari.cari_unvan1")
		});
		xrLabel2.Font = new Font("Tahoma", 10f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrLabel2.LocationFloat = new PointFloat(89.58337f, 79.00003f);
		xrLabel2.Name = "xrLabel2";
		xrLabel2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel2.SizeF = new SizeF(460.4166f, 23f);
		xrLabel2.StylePriority.UseFont = false;
		xrLabel2.StylePriority.UseTextAlignment = false;
		xrLabel2.Text = "xrLabel2";
		xrLabel2.TextAlignment = TextAlignment.MiddleLeft;
		xrLabel1.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari.cari_kod")
		});
		xrLabel1.Font = new Font("Tahoma", 10f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrLabel1.LocationFloat = new PointFloat(89.58337f, 56.00001f);
		xrLabel1.Name = "xrLabel1";
		xrLabel1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel1.SizeF = new SizeF(227.0833f, 23f);
		xrLabel1.StylePriority.UseFont = false;
		xrLabel1.StylePriority.UseTextAlignment = false;
		xrLabel1.Text = "xrLabel1";
		xrLabel1.TextAlignment = TextAlignment.MiddleLeft;
		TopMargin.HeightF = 55.04166f;
		TopMargin.Name = "TopMargin";
		TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		TopMargin.TextAlignment = TextAlignment.TopLeft;
		BottomMargin.Name = "BottomMargin";
		BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		BottomMargin.TextAlignment = TextAlignment.TopLeft;
		DetailReport.Bands.AddRange(new Band[2] { Detail1, DetailReport1 });
		DetailReport.DataMember = "gruplar";
		DetailReport.DataSource = bindingSource1;
		DetailReport.Level = 0;
		DetailReport.Name = "DetailReport";
		Detail1.HeightF = 0f;
		Detail1.Name = "Detail1";
		DetailReport1.Bands.AddRange(new Band[3] { Detail, ReportHeader1, ReportFooter });
		DetailReport1.DataMember = "gruplar.Satirlar";
		DetailReport1.DataSource = bindingSource1;
		DetailReport1.Level = 0;
		DetailReport1.Name = "DetailReport1";
		Detail.Controls.AddRange(new XRControl[1] { xrTable2 });
		Detail.HeightF = 20f;
		Detail.Name = "Detail";
		xrTable2.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable2.Font = new Font("Tahoma", 8f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable2.LocationFloat = new PointFloat(0f, 0f);
		xrTable2.Name = "xrTable2";
		xrTable2.Rows.AddRange(new XRTableRow[1] { xrTableRow2 });
		xrTable2.SizeF = new SizeF(775.9999f, 20f);
		xrTable2.StylePriority.UseBorders = false;
		xrTable2.StylePriority.UseFont = false;
		xrTable2.StylePriority.UseTextAlignment = false;
		xrTable2.TextAlignment = TextAlignment.MiddleCenter;
		xrTableRow2.Cells.AddRange(new XRTableCell[7] { xrTableCell8, xrTableCell9, xrTableCell10, xrTableCell11, xrTableCell12, xrTableCell13, xrTableCell14 });
		xrTableRow2.Name = "xrTableRow2";
		xrTableRow2.Weight = 1.0;
		xrTableCell8.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.Satirlar.Tarih", "{0:dd.MM.yyyy}")
		});
		xrTableCell8.Name = "xrTableCell8";
		xrTableCell8.Text = "xrTableCell8";
		xrTableCell8.Weight = 0.32396451531358555;
		xrTableCell9.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.Satirlar.VadeTarihi", "{0:dd.MM.yyyy}")
		});
		xrTableCell9.Name = "xrTableCell9";
		xrTableCell9.Text = "xrTableCell9";
		xrTableCell9.Weight = 0.3239644814553191;
		xrTableCell10.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.Satirlar.EvrakTipi")
		});
		xrTableCell10.Name = "xrTableCell10";
		xrTableCell10.Text = "xrTableCell10";
		xrTableCell10.Weight = 0.2973373460228693;
		xrTableCell11.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.Satirlar.EvrakCinsi")
		});
		xrTableCell11.Name = "xrTableCell11";
		xrTableCell11.Text = "xrTableCell11";
		xrTableCell11.Weight = 0.8210060942991376;
		xrTableCell12.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.Satirlar.EvrakSeriSira")
		});
		xrTableCell12.Name = "xrTableCell12";
		xrTableCell12.Text = "xrTableCell12";
		xrTableCell12.Weight = 0.3550296696307114;
		xrTableCell13.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.Satirlar.Meblag", "{0:n}")
		});
		xrTableCell13.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell13.Name = "xrTableCell13";
		xrTableCell13.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell13.StylePriority.UseFont = false;
		xrTableCell13.StylePriority.UsePadding = false;
		xrTableCell13.StylePriority.UseTextAlignment = false;
		xrTableCell13.Text = "xrTableCell13";
		xrTableCell13.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell13.Weight = 0.6434912658540228;
		xrTableCell14.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.Satirlar.Bakiye", "{0:n}")
		});
		xrTableCell14.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell14.Name = "xrTableCell14";
		xrTableCell14.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell14.StylePriority.UseFont = false;
		xrTableCell14.StylePriority.UsePadding = false;
		xrTableCell14.StylePriority.UseTextAlignment = false;
		xrTableCell14.Text = "xrTableCell14";
		xrTableCell14.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell14.Weight = 0.6789937575273143;
		ReportHeader1.Controls.AddRange(new XRControl[2] { xrTable1, xrLabel12 });
		ReportHeader1.HeightF = 44.99998f;
		ReportHeader1.Name = "ReportHeader1";
		xrTable1.BackColor = Color.Gainsboro;
		xrTable1.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable1.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTable1.LocationFloat = new PointFloat(0f, 22.99999f);
		xrTable1.Name = "xrTable1";
		xrTable1.Rows.AddRange(new XRTableRow[1] { xrTableRow1 });
		xrTable1.SizeF = new SizeF(776f, 22f);
		xrTable1.StylePriority.UseBackColor = false;
		xrTable1.StylePriority.UseBorders = false;
		xrTable1.StylePriority.UseFont = false;
		xrTable1.StylePriority.UseTextAlignment = false;
		xrTable1.TextAlignment = TextAlignment.MiddleCenter;
		xrTableRow1.Cells.AddRange(new XRTableCell[7] { xrTableCell7, xrTableCell5, xrTableCell1, xrTableCell6, xrTableCell2, xrTableCell4, xrTableCell3 });
		xrTableRow1.Name = "xrTableRow1";
		xrTableRow1.Weight = 1.0;
		xrTableCell7.Name = "xrTableCell7";
		xrTableCell7.Text = "Tarih";
		xrTableCell7.Weight = 0.32396454917185213;
		xrTableCell5.Name = "xrTableCell5";
		xrTableCell5.Text = "Vade";
		xrTableCell5.Weight = 0.3239645491718521;
		xrTableCell1.Name = "xrTableCell1";
		xrTableCell1.Text = "Tipi";
		xrTableCell1.Weight = 0.29733734602286926;
		xrTableCell6.Name = "xrTableCell6";
		xrTableCell6.Text = "Cinsi";
		xrTableCell6.Weight = 0.8210060604408709;
		xrTableCell2.Name = "xrTableCell2";
		xrTableCell2.Text = "Seri-sıra";
		xrTableCell2.Weight = 0.3550296696307114;
		xrTableCell4.Name = "xrTableCell4";
		xrTableCell4.Text = "Meblağ";
		xrTableCell4.Weight = 0.6434912658540227;
		xrTableCell3.Name = "xrTableCell3";
		xrTableCell3.Text = "Bakiye";
		xrTableCell3.Weight = 0.6789936898107812;
		xrLabel12.BackColor = Color.DarkGray;
		xrLabel12.Borders = BorderSide.All;
		xrLabel12.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "gruplar.GrupBaslik")
		});
		xrLabel12.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel12.LocationFloat = new PointFloat(3.178914E-05f, 0f);
		xrLabel12.Name = "xrLabel12";
		xrLabel12.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel12.SizeF = new SizeF(775.9999f, 23f);
		xrLabel12.StylePriority.UseBackColor = false;
		xrLabel12.StylePriority.UseBorders = false;
		xrLabel12.StylePriority.UseFont = false;
		xrLabel12.StylePriority.UseTextAlignment = false;
		xrLabel12.Text = "xrLabel12";
		xrLabel12.TextAlignment = TextAlignment.MiddleCenter;
		ReportFooter.Controls.AddRange(new XRControl[1] { xrLabel6 });
		ReportFooter.HeightF = 23f;
		ReportFooter.Name = "ReportFooter";
		xrLabel6.LocationFloat = new PointFloat(675.9999f, 0f);
		xrLabel6.Name = "xrLabel6";
		xrLabel6.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel6.SizeF = new SizeF(100f, 23f);
		bindingSource1.DataSource = typeof(CariEkstreButun);
		base.Bands.AddRange(new Band[4] { Ust_Bilgiler, TopMargin, BottomMargin, DetailReport });
		base.DataSource = bindingSource1;
		base.Margins = new Margins(33, 41, 55, 100);
		base.Version = "13.1";
		((ISupportInitialize)xrTable2).EndInit();
		((ISupportInitialize)xrTable1).EndInit();
		((ISupportInitialize)bindingSource1).EndInit();
		((ISupportInitialize)this).EndInit();
	}
}
