using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Fora.Mikro.Rapor.StokSatis;

namespace Fora.App.Mikro.ForaAndroid;

public class DxRaporStokSatis : XtraReport
{
	private IContainer components;

	private DetailBand DetailRaport;

	private TopMarginBand TopMargin;

	private BottomMarginBand BottomMargin;

	private DetailReportBand DetailReport;

	private DetailBand Detail1;

	private ReportHeaderBand ReportHeader;

	private GroupFooterBand GroupFooter1;

	private XRLabel xrLabel12;

	private XRTable xrTable1;

	private XRTableRow xrTableRow1;

	private XRTableCell xrTableCell7;

	private XRTableCell xrTableCell5;

	private XRTableCell xrTableCell1;

	private XRTableCell xrTableCell6;

	private XRTableCell xrTableCell2;

	private XRTableCell xrTableCell4;

	private XRTableCell xrTableCell3;

	private XRTableCell xrTableCell8;

	private XRTable xrTable2;

	private XRTableRow xrTableRow2;

	private XRTableCell xrTableCell9;

	private XRTableCell xrTableCell10;

	private XRTableCell xrTableCell11;

	private XRTableCell xrTableCell12;

	private XRTableCell xrTableCell13;

	private XRTableCell xrTableCell14;

	private XRTableCell xrTableCell16;

	private XRTableCell xrTableCell15;

	private XRTable xrTable3;

	private XRTableRow xrTableRow3;

	private XRTableCell xrTableCell18;

	private XRTableCell xrTableCell19;

	private XRTableCell xrTableCell20;

	private XRTableCell xrTableCell21;

	private XRTableCell xrTableCell22;

	private XRTableCell xrTableCell23;

	private XRTableCell xrTableCell24;

	private BindingSource bindingSource1;

	public DxRaporStokSatis()
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
		DetailRaport = new DetailBand();
		TopMargin = new TopMarginBand();
		BottomMargin = new BottomMarginBand();
		DetailReport = new DetailReportBand();
		Detail1 = new DetailBand();
		xrTable2 = new XRTable();
		xrTableRow2 = new XRTableRow();
		xrTableCell9 = new XRTableCell();
		xrTableCell10 = new XRTableCell();
		xrTableCell11 = new XRTableCell();
		xrTableCell12 = new XRTableCell();
		xrTableCell13 = new XRTableCell();
		xrTableCell14 = new XRTableCell();
		xrTableCell16 = new XRTableCell();
		xrTableCell15 = new XRTableCell();
		ReportHeader = new ReportHeaderBand();
		xrLabel12 = new XRLabel();
		xrTable1 = new XRTable();
		xrTableRow1 = new XRTableRow();
		xrTableCell7 = new XRTableCell();
		xrTableCell5 = new XRTableCell();
		xrTableCell1 = new XRTableCell();
		xrTableCell8 = new XRTableCell();
		xrTableCell6 = new XRTableCell();
		xrTableCell2 = new XRTableCell();
		xrTableCell4 = new XRTableCell();
		xrTableCell3 = new XRTableCell();
		GroupFooter1 = new GroupFooterBand();
		xrTable3 = new XRTable();
		xrTableRow3 = new XRTableRow();
		xrTableCell18 = new XRTableCell();
		xrTableCell19 = new XRTableCell();
		xrTableCell20 = new XRTableCell();
		xrTableCell21 = new XRTableCell();
		xrTableCell22 = new XRTableCell();
		xrTableCell23 = new XRTableCell();
		xrTableCell24 = new XRTableCell();
		bindingSource1 = new BindingSource(components);
		((ISupportInitialize)xrTable2).BeginInit();
		((ISupportInitialize)xrTable1).BeginInit();
		((ISupportInitialize)xrTable3).BeginInit();
		((ISupportInitialize)bindingSource1).BeginInit();
		((ISupportInitialize)this).BeginInit();
		DetailRaport.HeightF = 60.41667f;
		DetailRaport.Name = "DetailRaport";
		DetailRaport.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		DetailRaport.TextAlignment = TextAlignment.TopLeft;
		TopMargin.HeightF = 46.875f;
		TopMargin.Name = "TopMargin";
		TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		TopMargin.TextAlignment = TextAlignment.TopLeft;
		BottomMargin.HeightF = 100f;
		BottomMargin.Name = "BottomMargin";
		BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		BottomMargin.TextAlignment = TextAlignment.TopLeft;
		DetailReport.Bands.AddRange(new Band[3] { Detail1, ReportHeader, GroupFooter1 });
		DetailReport.DataMember = "list_items";
		DetailReport.DataSource = bindingSource1;
		DetailReport.Level = 0;
		DetailReport.Name = "DetailReport";
		Detail1.Controls.AddRange(new XRControl[1] { xrTable2 });
		Detail1.HeightF = 20f;
		Detail1.Name = "Detail1";
		xrTable2.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable2.Font = new Font("Tahoma", 8f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable2.LocationFloat = new PointFloat(0f, 0f);
		xrTable2.Name = "xrTable2";
		xrTable2.Rows.AddRange(new XRTableRow[1] { xrTableRow2 });
		xrTable2.SizeF = new SizeF(753.0001f, 20f);
		xrTable2.StylePriority.UseBorders = false;
		xrTable2.StylePriority.UseFont = false;
		xrTable2.StylePriority.UseTextAlignment = false;
		xrTable2.TextAlignment = TextAlignment.MiddleCenter;
		xrTableRow2.Cells.AddRange(new XRTableCell[8] { xrTableCell9, xrTableCell10, xrTableCell11, xrTableCell12, xrTableCell13, xrTableCell14, xrTableCell16, xrTableCell15 });
		xrTableRow2.Name = "xrTableRow2";
		xrTableRow2.Weight = 1.0;
		xrTableCell9.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.kodu")
		});
		xrTableCell9.Name = "xrTableCell9";
		xrTableCell9.Text = "xrTableCell9";
		xrTableCell9.Weight = 0.32396451531358555;
		xrTableCell10.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.adi")
		});
		xrTableCell10.Name = "xrTableCell10";
		xrTableCell10.Text = "xrTableCell10";
		xrTableCell10.Weight = 0.6845415444509914;
		xrTableCell11.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.miktar1", "{0:n}")
		});
		xrTableCell11.Name = "xrTableCell11";
		xrTableCell11.Text = "xrTableCell11";
		xrTableCell11.Weight = 0.375924685010114;
		xrTableCell12.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.miktar2", "{0:n}")
		});
		xrTableCell12.Name = "xrTableCell12";
		xrTableCell12.Text = "xrTableCell12";
		xrTableCell12.Weight = 0.345784466804038;
		xrTableCell13.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.miktar3", "{0:n}")
		});
		xrTableCell13.Name = "xrTableCell13";
		xrTableCell13.Text = "xrTableCell13";
		xrTableCell13.Weight = 0.36889788016992;
		xrTableCell14.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.tutar1", "{0:n}")
		});
		xrTableCell14.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell14.Name = "xrTableCell14";
		xrTableCell14.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell14.StylePriority.UseFont = false;
		xrTableCell14.StylePriority.UsePadding = false;
		xrTableCell14.StylePriority.UseTextAlignment = false;
		xrTableCell14.Text = "xrTableCell14";
		xrTableCell14.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell14.Weight = 0.37814443732725456;
		xrTableCell16.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.tutar2")
		});
		xrTableCell16.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell16.Name = "xrTableCell16";
		xrTableCell16.StylePriority.UseFont = false;
		xrTableCell16.Text = "xrTableCell16";
		xrTableCell16.Weight = 0.40310596394338044;
		xrTableCell15.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "list_items.tutar3", "{0:n}")
		});
		xrTableCell15.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell15.Name = "xrTableCell15";
		xrTableCell15.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell15.StylePriority.UseFont = false;
		xrTableCell15.StylePriority.UsePadding = false;
		xrTableCell15.StylePriority.UseTextAlignment = false;
		xrTableCell15.Text = "xrTableCell15";
		xrTableCell15.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell15.Weight = 0.4613536941922643;
		ReportHeader.Controls.AddRange(new XRControl[2] { xrLabel12, xrTable1 });
		ReportHeader.HeightF = 54.99996f;
		ReportHeader.Name = "ReportHeader";
		xrLabel12.BackColor = Color.DarkGray;
		xrLabel12.Borders = BorderSide.All;
		xrLabel12.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrLabel12.LocationFloat = new PointFloat(0f, 10.00001f);
		xrLabel12.Name = "xrLabel12";
		xrLabel12.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel12.SizeF = new SizeF(753f, 23f);
		xrLabel12.StylePriority.UseBackColor = false;
		xrLabel12.StylePriority.UseBorders = false;
		xrLabel12.StylePriority.UseFont = false;
		xrLabel12.StylePriority.UseTextAlignment = false;
		xrLabel12.Text = "Satış raporu";
		xrLabel12.TextAlignment = TextAlignment.MiddleCenter;
		xrTable1.BackColor = Color.Gainsboro;
		xrTable1.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable1.Font = new Font("Tahoma", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTable1.LocationFloat = new PointFloat(0f, 32.99996f);
		xrTable1.Name = "xrTable1";
		xrTable1.Rows.AddRange(new XRTableRow[1] { xrTableRow1 });
		xrTable1.SizeF = new SizeF(753.0001f, 22f);
		xrTable1.StylePriority.UseBackColor = false;
		xrTable1.StylePriority.UseBorders = false;
		xrTable1.StylePriority.UseFont = false;
		xrTable1.StylePriority.UseTextAlignment = false;
		xrTable1.TextAlignment = TextAlignment.MiddleCenter;
		xrTableRow1.Cells.AddRange(new XRTableCell[8] { xrTableCell7, xrTableCell5, xrTableCell1, xrTableCell8, xrTableCell6, xrTableCell2, xrTableCell4, xrTableCell3 });
		xrTableRow1.Name = "xrTableRow1";
		xrTableRow1.Weight = 1.0;
		xrTableCell7.Name = "xrTableCell7";
		xrTableCell7.Text = "Kod";
		xrTableCell7.Weight = 0.32396454917185213;
		xrTableCell5.Name = "xrTableCell5";
		xrTableCell5.Text = "İsim";
		xrTableCell5.Weight = 0.6845414200131461;
		xrTableCell1.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "baslik_miktar1")
		});
		xrTableCell1.Name = "xrTableCell1";
		xrTableCell1.Text = "M 1";
		xrTableCell1.Weight = 0.3759246049312448;
		xrTableCell8.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "baslik_miktar2")
		});
		xrTableCell8.Name = "xrTableCell8";
		xrTableCell8.Text = "M2";
		xrTableCell8.Weight = 0.34578428439405107;
		xrTableCell6.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "baslik_miktar3")
		});
		xrTableCell6.Name = "xrTableCell6";
		xrTableCell6.Text = "M3";
		xrTableCell6.Weight = 0.3688979311318572;
		xrTableCell2.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "baslik_tutar1")
		});
		xrTableCell2.Name = "xrTableCell2";
		xrTableCell2.Text = "T1";
		xrTableCell2.Weight = 0.3781438242424356;
		xrTableCell4.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "baslik_tutar2")
		});
		xrTableCell4.Name = "xrTableCell4";
		xrTableCell4.Text = "T2";
		xrTableCell4.Weight = 0.40310644828533165;
		xrTableCell3.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "baslik_tutar3")
		});
		xrTableCell3.Name = "xrTableCell3";
		xrTableCell3.Text = "T3";
		xrTableCell3.Weight = 0.46135332849968724;
		GroupFooter1.Controls.AddRange(new XRControl[1] { xrTable3 });
		GroupFooter1.HeightF = 20f;
		GroupFooter1.Name = "GroupFooter1";
		xrTable3.Borders = BorderSide.Left | BorderSide.Right | BorderSide.Bottom;
		xrTable3.Font = new Font("Tahoma", 8f, FontStyle.Regular, GraphicsUnit.Point, 162);
		xrTable3.LocationFloat = new PointFloat(0f, 0f);
		xrTable3.Name = "xrTable3";
		xrTable3.Rows.AddRange(new XRTableRow[1] { xrTableRow3 });
		xrTable3.SizeF = new SizeF(753.0001f, 20f);
		xrTable3.StylePriority.UseBorders = false;
		xrTable3.StylePriority.UseFont = false;
		xrTable3.StylePriority.UseTextAlignment = false;
		xrTable3.TextAlignment = TextAlignment.MiddleCenter;
		xrTableRow3.Cells.AddRange(new XRTableCell[7] { xrTableCell18, xrTableCell19, xrTableCell20, xrTableCell21, xrTableCell22, xrTableCell23, xrTableCell24 });
		xrTableRow3.Name = "xrTableRow3";
		xrTableRow3.Weight = 1.0;
		xrTableCell18.Font = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell18.Name = "xrTableCell18";
		xrTableCell18.StylePriority.UseFont = false;
		xrTableCell18.StylePriority.UseTextAlignment = false;
		xrTableCell18.Text = "Toplam :";
		xrTableCell18.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell18.Weight = 1.008506059764577;
		xrTableCell19.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "Toplam_Miktar1", "{0:n}")
		});
		xrTableCell19.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell19.Name = "xrTableCell19";
		xrTableCell19.StylePriority.UseFont = false;
		xrTableCell19.Text = "xrTableCell19";
		xrTableCell19.Weight = 0.375924685010114;
		xrTableCell20.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "Toplam_Miktar2", "{0:n}")
		});
		xrTableCell20.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell20.Name = "xrTableCell20";
		xrTableCell20.StylePriority.UseFont = false;
		xrTableCell20.Text = "xrTableCell20";
		xrTableCell20.Weight = 0.345784466804038;
		xrTableCell21.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "Toplam_Miktar3", "{0:n}")
		});
		xrTableCell21.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell21.Name = "xrTableCell21";
		xrTableCell21.StylePriority.UseFont = false;
		xrTableCell21.Text = "xrTableCell21";
		xrTableCell21.Weight = 0.36889788016992;
		xrTableCell22.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "Toplam_Tutar1", "{0:n}")
		});
		xrTableCell22.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell22.Name = "xrTableCell22";
		xrTableCell22.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell22.StylePriority.UseFont = false;
		xrTableCell22.StylePriority.UsePadding = false;
		xrTableCell22.StylePriority.UseTextAlignment = false;
		xrTableCell22.Text = "xrTableCell22";
		xrTableCell22.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell22.Weight = 0.37814443732725456;
		xrTableCell23.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "Toplam_Tutar2", "{0:n}")
		});
		xrTableCell23.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell23.Name = "xrTableCell23";
		xrTableCell23.StylePriority.UseFont = false;
		xrTableCell23.Text = "xrTableCell23";
		xrTableCell23.Weight = 0.40310596394338044;
		xrTableCell24.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "Toplam_Tutar3", "{0:n}")
		});
		xrTableCell24.Font = new Font("Tahoma", 10f, FontStyle.Bold, GraphicsUnit.Point, 162);
		xrTableCell24.Name = "xrTableCell24";
		xrTableCell24.Padding = new PaddingInfo(0, 3, 0, 0, 100f);
		xrTableCell24.StylePriority.UseFont = false;
		xrTableCell24.StylePriority.UsePadding = false;
		xrTableCell24.StylePriority.UseTextAlignment = false;
		xrTableCell24.Text = "xrTableCell24";
		xrTableCell24.TextAlignment = TextAlignment.MiddleRight;
		xrTableCell24.Weight = 0.4613536941922643;
		bindingSource1.DataSource = typeof(RaporStokSatisSonuc);
		base.Bands.AddRange(new Band[4] { DetailRaport, TopMargin, BottomMargin, DetailReport });
		base.DataSource = bindingSource1;
		base.Margins = new Margins(35, 36, 47, 100);
		base.PageHeight = 1169;
		base.PageWidth = 827;
		base.PaperKind = PaperKind.A4;
		base.Version = "14.1";
		((ISupportInitialize)xrTable2).EndInit();
		((ISupportInitialize)xrTable1).EndInit();
		((ISupportInitialize)xrTable3).EndInit();
		((ISupportInitialize)bindingSource1).EndInit();
		((ISupportInitialize)this).EndInit();
	}
}
