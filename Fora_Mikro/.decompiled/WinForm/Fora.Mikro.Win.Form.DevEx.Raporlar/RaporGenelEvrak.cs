using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Fora.Mikro.Evraklar;

namespace Fora.Mikro.Win.Form.DevEx.Raporlar;

public class RaporGenelEvrak : XtraReport
{
	private IContainer components;

	private DetailBand Detail;

	private TopMarginBand TopMargin;

	private BottomMarginBand BottomMargin;

	private BindingSource bindingSource1;

	private XRLabel xrLabel7;

	private XRLabel xrLabel6;

	private XRLabel xrLabel5;

	private DetailReportBand DetailReport;

	private DetailBand Detail1;

	private XRLabel xrLabel21;

	private XRLabel xrLabel20;

	private XRLabel xrLabel19;

	private XRLabel xrLabel18;

	private ReportFooterBand ReportFooter;

	private XRLabel xrLabel27;

	private XRLabel xrLabel31;

	private XRLabel xrLabel32;

	private XRLabel xrLabel33;

	private XRLabel xrLabel34;

	private CalculatedField hesap_adi_satir_nokta_vuruslu;

	private XRLabel xrLabel35;

	private ReportHeaderBand ReportHeader;

	private XRLabel xrLabel36;

	private CalculatedField hesap_kodu_satir_nokta_vuruslu;

	private CalculatedField miktar_satir_nokta_vuruslu;

	private CalculatedField toplam_fiyat_net_satir_nokta_vuruslu;

	private XRLabel xrLabel39;

	private XRLabel xrLabel38;

	private XRLabel xrLabel37;

	private CalculatedField ara_toplam_nokta_vuruslu;

	private XRLabel xrLabel40;

	private CalculatedField toplam_iskonto_tutari_nokta_vuruslu;

	private CalculatedField toplam_vergi_tutari_nokta_vuruslu;

	private CalculatedField yekun_nokta_vuruslu;

	private CalculatedField cari_unvan1_nokta_vuruslu;

	private CalculatedField cari_unvan2_nokta_vuruslu;

	private CalculatedField cari_vdaire_adi_nkta_vuruslu;

	private XRLabel xrLabel10;

	private XRLabel xrLabel9;

	private XRLabel xrLabel8;

	private XRLabel xrLabel14;

	private XRLabel xrLabel13;

	private XRLabel xrLabel12;

	private XRLabel xrLabel11;

	private XRLabel xrLabel17;

	private XRLabel xrLabel16;

	private XRLabel xrLabel15;

	private CalculatedField cari_vdaire_no_nokta_vuruslu;

	private CalculatedField fatura_adresi_adr_cadde_nokta_vuruslu;

	private CalculatedField fatura_adresi_adr_sokak_nokta_vuruslu;

	private CalculatedField fatura_adresi_adr_posta_kodu_nokta_vuruslu;

	private XRLabel xrLabel25;

	private XRLabel xrLabel24;

	private XRLabel xrLabel23;

	private XRLabel xrLabel22;

	private XRLabel xrLabel1;

	private CalculatedField fatura_adresi_adr_ilce_nokta_vuruslu;

	private CalculatedField fatura_adresi_adr_il_nokta_vuruslu;

	private CalculatedField sevk_adresi_adr_cadde_nokta_vuruslu;

	private CalculatedField sevk_adresi_adr_sokak_nokta_vuruslu;

	private CalculatedField sevk_adresi_adr_posta_kodu_nokta_vuruslu;

	private CalculatedField sevk_adresi_adr_ilce_nokta_vuruslu;

	private CalculatedField sevk_adresi_adr_il_nokta_vuruslu;

	private XRLabel xrLabel3;

	private XRLabel xrLabel2;

	private XRLabel xrLabel4;

	private XRLabel xrLabel28;

	private XRLabel xrLabel26;

	public RaporGenelEvrak()
	{
		InitializeComponent();
	}

	private void bindingSource1_CurrentChanged(object sender, EventArgs e)
	{
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
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(RaporGenelEvrak));
		Detail = new DetailBand();
		xrLabel14 = new XRLabel();
		xrLabel21 = new XRLabel();
		xrLabel20 = new XRLabel();
		xrLabel19 = new XRLabel();
		xrLabel18 = new XRLabel();
		xrLabel5 = new XRLabel();
		xrLabel6 = new XRLabel();
		xrLabel7 = new XRLabel();
		TopMargin = new TopMarginBand();
		BottomMargin = new BottomMarginBand();
		DetailReport = new DetailReportBand();
		Detail1 = new DetailBand();
		xrLabel39 = new XRLabel();
		xrLabel38 = new XRLabel();
		xrLabel37 = new XRLabel();
		xrLabel35 = new XRLabel();
		ReportFooter = new ReportFooterBand();
		xrLabel13 = new XRLabel();
		xrLabel12 = new XRLabel();
		xrLabel10 = new XRLabel();
		xrLabel9 = new XRLabel();
		xrLabel8 = new XRLabel();
		xrLabel40 = new XRLabel();
		xrLabel31 = new XRLabel();
		xrLabel32 = new XRLabel();
		xrLabel33 = new XRLabel();
		xrLabel34 = new XRLabel();
		xrLabel27 = new XRLabel();
		ReportHeader = new ReportHeaderBand();
		xrLabel11 = new XRLabel();
		xrLabel36 = new XRLabel();
		bindingSource1 = new BindingSource(components);
		hesap_adi_satir_nokta_vuruslu = new CalculatedField();
		hesap_kodu_satir_nokta_vuruslu = new CalculatedField();
		miktar_satir_nokta_vuruslu = new CalculatedField();
		toplam_fiyat_net_satir_nokta_vuruslu = new CalculatedField();
		ara_toplam_nokta_vuruslu = new CalculatedField();
		toplam_iskonto_tutari_nokta_vuruslu = new CalculatedField();
		toplam_vergi_tutari_nokta_vuruslu = new CalculatedField();
		yekun_nokta_vuruslu = new CalculatedField();
		cari_unvan1_nokta_vuruslu = new CalculatedField();
		cari_unvan2_nokta_vuruslu = new CalculatedField();
		cari_vdaire_adi_nkta_vuruslu = new CalculatedField();
		xrLabel15 = new XRLabel();
		xrLabel16 = new XRLabel();
		xrLabel17 = new XRLabel();
		cari_vdaire_no_nokta_vuruslu = new CalculatedField();
		fatura_adresi_adr_cadde_nokta_vuruslu = new CalculatedField();
		fatura_adresi_adr_sokak_nokta_vuruslu = new CalculatedField();
		fatura_adresi_adr_posta_kodu_nokta_vuruslu = new CalculatedField();
		xrLabel22 = new XRLabel();
		xrLabel23 = new XRLabel();
		xrLabel24 = new XRLabel();
		xrLabel25 = new XRLabel();
		fatura_adresi_adr_ilce_nokta_vuruslu = new CalculatedField();
		fatura_adresi_adr_il_nokta_vuruslu = new CalculatedField();
		sevk_adresi_adr_cadde_nokta_vuruslu = new CalculatedField();
		sevk_adresi_adr_sokak_nokta_vuruslu = new CalculatedField();
		sevk_adresi_adr_posta_kodu_nokta_vuruslu = new CalculatedField();
		sevk_adresi_adr_ilce_nokta_vuruslu = new CalculatedField();
		sevk_adresi_adr_il_nokta_vuruslu = new CalculatedField();
		xrLabel1 = new XRLabel();
		xrLabel2 = new XRLabel();
		xrLabel3 = new XRLabel();
		xrLabel4 = new XRLabel();
		xrLabel26 = new XRLabel();
		xrLabel28 = new XRLabel();
		((ISupportInitialize)bindingSource1).BeginInit();
		((ISupportInitialize)this).BeginInit();
		Detail.Controls.AddRange(new XRControl[21]
		{
			xrLabel28, xrLabel26, xrLabel4, xrLabel3, xrLabel2, xrLabel1, xrLabel25, xrLabel24, xrLabel23, xrLabel22,
			xrLabel17, xrLabel16, xrLabel15, xrLabel14, xrLabel21, xrLabel20, xrLabel19, xrLabel18, xrLabel5, xrLabel6,
			xrLabel7
		});
		Detail.HeightF = 299.2917f;
		Detail.Name = "Detail";
		Detail.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		Detail.TextAlignment = TextAlignment.TopLeft;
		xrLabel14.LocationFloat = new PointFloat(0f, 276.2917f);
		xrLabel14.Name = "xrLabel14";
		xrLabel14.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel14.SizeF = new SizeF(176.0417f, 23f);
		xrLabel14.Text = " ";
		xrLabel21.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "saat")
		});
		xrLabel21.Font = new Font("Tahoma", 9.75f);
		xrLabel21.LocationFloat = new PointFloat(176.0419f, 207.2917f);
		xrLabel21.Name = "xrLabel21";
		xrLabel21.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel21.SizeF = new SizeF(139.4166f, 23f);
		xrLabel21.StylePriority.UseFont = false;
		xrLabel21.Text = "xrLabel21";
		xrLabel20.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "fatura_adresi_adr_il_nokta_vuruslu")
		});
		xrLabel20.Font = new Font("Tahoma", 9.75f);
		xrLabel20.LocationFloat = new PointFloat(596.5421f, 138.2917f);
		xrLabel20.Name = "xrLabel20";
		xrLabel20.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel20.SizeF = new SizeF(119.1666f, 23f);
		xrLabel20.StylePriority.UseFont = false;
		xrLabel20.Text = "xrLabel20";
		xrLabel19.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "fatura_adresi_adr_ilce_nokta_vuruslu")
		});
		xrLabel19.Font = new Font("Tahoma", 9.75f);
		xrLabel19.LocationFloat = new PointFloat(315.4584f, 138.2917f);
		xrLabel19.Name = "xrLabel19";
		xrLabel19.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel19.SizeF = new SizeF(152.0834f, 23f);
		xrLabel19.StylePriority.UseFont = false;
		xrLabel19.StylePriority.UseTextAlignment = false;
		xrLabel19.Text = "xrLabel19";
		xrLabel19.TextAlignment = TextAlignment.TopRight;
		xrLabel18.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "fatura_adresi_adr_posta_kodu_nokta_vuruslu")
		});
		xrLabel18.Font = new Font("Tahoma", 9.75f);
		xrLabel18.LocationFloat = new PointFloat(6.357829E-05f, 138.2917f);
		xrLabel18.Name = "xrLabel18";
		xrLabel18.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel18.SizeF = new SizeF(176.0416f, 23f);
		xrLabel18.StylePriority.UseFont = false;
		xrLabel18.Text = "xrLabel18";
		xrLabel5.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "fatura_adresi_adr_cadde_nokta_vuruslu")
		});
		xrLabel5.Font = new Font("Tahoma", 9.75f);
		xrLabel5.LocationFloat = new PointFloat(6.357829E-05f, 92.29167f);
		xrLabel5.Name = "xrLabel5";
		xrLabel5.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel5.SizeF = new SizeF(176.0416f, 23.00001f);
		xrLabel5.StylePriority.UseFont = false;
		xrLabel5.Text = "xrLabel5";
		xrLabel6.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "fatura_adresi_adr_sokak_nokta_vuruslu")
		});
		xrLabel6.Font = new Font("Tahoma", 9.75f);
		xrLabel6.LocationFloat = new PointFloat(6.357829E-05f, 115.2917f);
		xrLabel6.Name = "xrLabel6";
		xrLabel6.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel6.SizeF = new SizeF(176.0416f, 23.00002f);
		xrLabel6.StylePriority.UseFont = false;
		xrLabel6.Text = "xrLabel6";
		xrLabel7.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "evrak_tarih", "{0:dd.MM.yyyy}")
		});
		xrLabel7.Font = new Font("Tahoma", 9.75f);
		xrLabel7.LocationFloat = new PointFloat(176.0417f, 184.2917f);
		xrLabel7.Name = "xrLabel7";
		xrLabel7.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel7.SizeF = new SizeF(139.4167f, 23f);
		xrLabel7.StylePriority.UseFont = false;
		xrLabel7.Text = "xrLabel7";
		TopMargin.HeightF = 47.66668f;
		TopMargin.Name = "TopMargin";
		TopMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		TopMargin.TextAlignment = TextAlignment.TopLeft;
		BottomMargin.HeightF = 172.8333f;
		BottomMargin.Name = "BottomMargin";
		BottomMargin.Padding = new PaddingInfo(0, 0, 0, 0, 100f);
		BottomMargin.TextAlignment = TextAlignment.TopLeft;
		DetailReport.Bands.AddRange(new Band[3] { Detail1, ReportFooter, ReportHeader });
		DetailReport.DataMember = "satirlar";
		DetailReport.DataSource = bindingSource1;
		DetailReport.Level = 0;
		DetailReport.Name = "DetailReport";
		Detail1.Controls.AddRange(new XRControl[4] { xrLabel39, xrLabel38, xrLabel37, xrLabel35 });
		Detail1.HeightF = 23f;
		Detail1.Name = "Detail1";
		xrLabel39.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "satirlar.toplam_fiyat_net_satir_nokta_vuruslu")
		});
		xrLabel39.LocationFloat = new PointFloat(467.5418f, 0f);
		xrLabel39.Name = "xrLabel39";
		xrLabel39.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel39.SizeF = new SizeF(129.0002f, 23f);
		xrLabel39.Text = "xrLabel39";
		xrLabel38.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "satirlar.miktar_satir_nokta_vuruslu")
		});
		xrLabel38.LocationFloat = new PointFloat(315.4584f, 0f);
		xrLabel38.Name = "xrLabel38";
		xrLabel38.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel38.SizeF = new SizeF(152.0834f, 23f);
		xrLabel38.Text = "xrLabel38";
		xrLabel37.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "satirlar.hesap_kodu_satir_nokta_vuruslu")
		});
		xrLabel37.LocationFloat = new PointFloat(0f, 0f);
		xrLabel37.Name = "xrLabel37";
		xrLabel37.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel37.SizeF = new SizeF(176.0418f, 23f);
		xrLabel37.Text = "xrLabel37";
		xrLabel35.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "satirlar.hesap_adi_satir_nokta_vuruslu")
		});
		xrLabel35.LocationFloat = new PointFloat(176.0419f, 0f);
		xrLabel35.Name = "xrLabel35";
		xrLabel35.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel35.SizeF = new SizeF(139.4166f, 23f);
		xrLabel35.Text = "xrLabel35";
		ReportFooter.Controls.AddRange(new XRControl[11]
		{
			xrLabel13, xrLabel12, xrLabel10, xrLabel9, xrLabel8, xrLabel40, xrLabel31, xrLabel32, xrLabel33, xrLabel34,
			xrLabel27
		});
		ReportFooter.HeightF = 175.9583f;
		ReportFooter.Name = "ReportFooter";
		xrLabel13.LocationFloat = new PointFloat(0f, 115.0001f);
		xrLabel13.Name = "xrLabel13";
		xrLabel13.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel13.SizeF = new SizeF(176.0418f, 22.99998f);
		xrLabel13.Text = " ";
		xrLabel12.LocationFloat = new PointFloat(0f, 0f);
		xrLabel12.Name = "xrLabel12";
		xrLabel12.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel12.SizeF = new SizeF(176.0417f, 23f);
		xrLabel12.Text = "------ -------------------- ---- ---------";
		xrLabel10.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "yekun_nokta_vuruslu")
		});
		xrLabel10.LocationFloat = new PointFloat(176.0417f, 92.00014f);
		xrLabel10.Name = "xrLabel10";
		xrLabel10.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel10.SizeF = new SizeF(139.4165f, 23f);
		xrLabel10.Text = "xrLabel10";
		xrLabel9.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_vergi_tutari_nokta_vuruslu")
		});
		xrLabel9.LocationFloat = new PointFloat(176.0417f, 69.00011f);
		xrLabel9.Name = "xrLabel9";
		xrLabel9.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel9.SizeF = new SizeF(139.4165f, 23f);
		xrLabel9.Text = "xrLabel9";
		xrLabel8.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "toplam_iskonto_tutari_nokta_vuruslu")
		});
		xrLabel8.LocationFloat = new PointFloat(176.0417f, 45.99997f);
		xrLabel8.Name = "xrLabel8";
		xrLabel8.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel8.SizeF = new SizeF(139.4165f, 23f);
		xrLabel8.Text = "xrLabel8";
		xrLabel40.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "ara_toplam_nokta_vuruslu")
		});
		xrLabel40.LocationFloat = new PointFloat(176.0417f, 22.99999f);
		xrLabel40.Name = "xrLabel40";
		xrLabel40.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel40.SizeF = new SizeF(139.4165f, 23f);
		xrLabel40.Text = "xrLabel40";
		xrLabel31.Font = new Font("Tahoma", 9.75f);
		xrLabel31.LocationFloat = new PointFloat(0f, 22.99999f);
		xrLabel31.Name = "xrLabel31";
		xrLabel31.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel31.SizeF = new SizeF(176.0417f, 23f);
		xrLabel31.StylePriority.UseFont = false;
		xrLabel31.StylePriority.UseTextAlignment = false;
		xrLabel31.Text = "                   ARA TOPLAM :";
		xrLabel31.TextAlignment = TextAlignment.TopRight;
		xrLabel32.Font = new Font("Tahoma", 9.75f);
		xrLabel32.LocationFloat = new PointFloat(0f, 45.99997f);
		xrLabel32.Name = "xrLabel32";
		xrLabel32.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel32.SizeF = new SizeF(176.0418f, 23f);
		xrLabel32.StylePriority.UseFont = false;
		xrLabel32.StylePriority.UseTextAlignment = false;
		xrLabel32.Text = "                      İSKONTO :";
		xrLabel32.TextAlignment = TextAlignment.TopRight;
		xrLabel33.Font = new Font("Tahoma", 9.75f);
		xrLabel33.LocationFloat = new PointFloat(0f, 68.99999f);
		xrLabel33.Name = "xrLabel33";
		xrLabel33.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel33.SizeF = new SizeF(176.0418f, 23f);
		xrLabel33.StylePriority.UseFont = false;
		xrLabel33.StylePriority.UseTextAlignment = false;
		xrLabel33.Text = "                          KDV :";
		xrLabel33.TextAlignment = TextAlignment.TopRight;
		xrLabel34.Font = new Font("Tahoma", 9.75f);
		xrLabel34.LocationFloat = new PointFloat(0f, 92.00007f);
		xrLabel34.Name = "xrLabel34";
		xrLabel34.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel34.SizeF = new SizeF(176.0418f, 23.00001f);
		xrLabel34.StylePriority.UseFont = false;
		xrLabel34.StylePriority.UseTextAlignment = false;
		xrLabel34.Text = "                        YEKÜN :";
		xrLabel34.TextAlignment = TextAlignment.TopRight;
		xrLabel27.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "yazi_ile_yekun")
		});
		xrLabel27.Font = new Font("Tahoma", 9.75f);
		xrLabel27.LocationFloat = new PointFloat(0f, 138.0001f);
		xrLabel27.Name = "xrLabel27";
		xrLabel27.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel27.SizeF = new SizeF(176.0419f, 23f);
		xrLabel27.StylePriority.UseFont = false;
		xrLabel27.Text = "xrLabel14";
		ReportHeader.Controls.AddRange(new XRControl[2] { xrLabel11, xrLabel36 });
		ReportHeader.HeightF = 45.99999f;
		ReportHeader.Name = "ReportHeader";
		xrLabel11.LocationFloat = new PointFloat(0f, 22.99995f);
		xrLabel11.Name = "xrLabel11";
		xrLabel11.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel11.SizeF = new SizeF(176.0418f, 23f);
		xrLabel11.Text = "------ -------------------- ---- ---------";
		xrLabel36.LocationFloat = new PointFloat(0f, 0f);
		xrLabel36.Name = "xrLabel36";
		xrLabel36.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel36.SizeF = new SizeF(176.0418f, 23f);
		xrLabel36.Text = "S.KOD  STOK ISMI           MİKTAR    TUTAR";
		bindingSource1.DataSource = typeof(GenelEvrak);
		bindingSource1.CurrentChanged += bindingSource1_CurrentChanged;
		hesap_adi_satir_nokta_vuruslu.DataMember = "satirlar";
		hesap_adi_satir_nokta_vuruslu.FieldType = FieldType.String;
		hesap_adi_satir_nokta_vuruslu.Name = "hesap_adi_satir_nokta_vuruslu";
		hesap_adi_satir_nokta_vuruslu.Scripts.OnGetValue = "hesap_adi_nokta_vuruslu_GetValue";
		hesap_kodu_satir_nokta_vuruslu.DataMember = "satirlar";
		hesap_kodu_satir_nokta_vuruslu.FieldType = FieldType.String;
		hesap_kodu_satir_nokta_vuruslu.Name = "hesap_kodu_satir_nokta_vuruslu";
		hesap_kodu_satir_nokta_vuruslu.Scripts.OnGetValue = "hesap_kodu_nokta_vuruslu_GetValue";
		miktar_satir_nokta_vuruslu.DataMember = "satirlar";
		miktar_satir_nokta_vuruslu.FieldType = FieldType.String;
		miktar_satir_nokta_vuruslu.Name = "miktar_satir_nokta_vuruslu";
		miktar_satir_nokta_vuruslu.Scripts.OnGetValue = "miktar_nokta_vuruslu_GetValue";
		toplam_fiyat_net_satir_nokta_vuruslu.DataMember = "satirlar";
		toplam_fiyat_net_satir_nokta_vuruslu.FieldType = FieldType.String;
		toplam_fiyat_net_satir_nokta_vuruslu.Name = "toplam_fiyat_net_satir_nokta_vuruslu";
		toplam_fiyat_net_satir_nokta_vuruslu.Scripts.OnGetValue = "toplam_fiyat_net_nokta_vuruslu_GetValue";
		ara_toplam_nokta_vuruslu.FieldType = FieldType.String;
		ara_toplam_nokta_vuruslu.Name = "ara_toplam_nokta_vuruslu";
		ara_toplam_nokta_vuruslu.Scripts.OnGetValue = "ara_toplam_nokta_vuruslu_GetValue";
		toplam_iskonto_tutari_nokta_vuruslu.FieldType = FieldType.String;
		toplam_iskonto_tutari_nokta_vuruslu.Name = "toplam_iskonto_tutari_nokta_vuruslu";
		toplam_iskonto_tutari_nokta_vuruslu.Scripts.OnGetValue = "toplam_iskonto_tutari_nokta_vuruslu_GetValue";
		toplam_vergi_tutari_nokta_vuruslu.FieldType = FieldType.String;
		toplam_vergi_tutari_nokta_vuruslu.Name = "toplam_vergi_tutari_nokta_vuruslu";
		toplam_vergi_tutari_nokta_vuruslu.Scripts.OnGetValue = "toplam_vergi_tutari_nokta_vuruslu_GetValue";
		yekun_nokta_vuruslu.FieldType = FieldType.String;
		yekun_nokta_vuruslu.Name = "yekun_nokta_vuruslu";
		yekun_nokta_vuruslu.Scripts.OnGetValue = "yekun_nokta_vuruslu_GetValue";
		cari_unvan1_nokta_vuruslu.FieldType = FieldType.String;
		cari_unvan1_nokta_vuruslu.Name = "cari_unvan1_nokta_vuruslu";
		cari_unvan1_nokta_vuruslu.Scripts.OnGetValue = "cari_unvan1_nokta_vuruslu_GetValue";
		cari_unvan2_nokta_vuruslu.FieldType = FieldType.String;
		cari_unvan2_nokta_vuruslu.Name = "cari_unvan2_nokta_vuruslu";
		cari_unvan2_nokta_vuruslu.Scripts.OnGetValue = "cari_unvan2_nokta_vuruslu_GetValue";
		cari_vdaire_adi_nkta_vuruslu.FieldType = FieldType.String;
		cari_vdaire_adi_nkta_vuruslu.Name = "cari_vdaire_adi_nkta_vuruslu";
		cari_vdaire_adi_nkta_vuruslu.Scripts.OnGetValue = "cari_vdaire_adi_nkta_vuruslu_GetValue";
		xrLabel15.LocationFloat = new PointFloat(0f, 0f);
		xrLabel15.Name = "xrLabel15";
		xrLabel15.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel15.SizeF = new SizeF(176.0417f, 23f);
		xrLabel15.Text = " ";
		xrLabel16.LocationFloat = new PointFloat(176.0417f, 253.2917f);
		xrLabel16.Name = "xrLabel16";
		xrLabel16.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel16.SizeF = new SizeF(139.4167f, 23f);
		xrLabel16.Text = "        ";
		xrLabel17.LocationFloat = new PointFloat(467.5417f, 138.2917f);
		xrLabel17.Name = "xrLabel17";
		xrLabel17.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel17.SizeF = new SizeF(129.0003f, 23f);
		xrLabel17.Text = "-";
		cari_vdaire_no_nokta_vuruslu.FieldType = FieldType.String;
		cari_vdaire_no_nokta_vuruslu.Name = "cari_vdaire_no_nokta_vuruslu";
		cari_vdaire_no_nokta_vuruslu.Scripts.OnGetValue = "cari_vdaire_no_nokta_vuruslu_GetValue";
		fatura_adresi_adr_cadde_nokta_vuruslu.FieldType = FieldType.String;
		fatura_adresi_adr_cadde_nokta_vuruslu.Name = "fatura_adresi_adr_cadde_nokta_vuruslu";
		fatura_adresi_adr_cadde_nokta_vuruslu.Scripts.OnGetValue = "fatura_adresi_adr_cadde_nokta_vuruslu_GetValue";
		fatura_adresi_adr_sokak_nokta_vuruslu.FieldType = FieldType.String;
		fatura_adresi_adr_sokak_nokta_vuruslu.Name = "fatura_adresi_adr_sokak_nokta_vuruslu";
		fatura_adresi_adr_sokak_nokta_vuruslu.Scripts.OnGetValue = "fatura_adresi_adr_sokak_nokta_vuruslu_GetValue";
		fatura_adresi_adr_posta_kodu_nokta_vuruslu.FieldType = FieldType.String;
		fatura_adresi_adr_posta_kodu_nokta_vuruslu.Name = "fatura_adresi_adr_posta_kodu_nokta_vuruslu";
		fatura_adresi_adr_posta_kodu_nokta_vuruslu.Scripts.OnGetValue = "fatura_adresi_adr_posta_kodu_nokta_vuruslu_GetValue";
		xrLabel22.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari_unvan1_nokta_vuruslu")
		});
		xrLabel22.LocationFloat = new PointFloat(0f, 23f);
		xrLabel22.Name = "xrLabel22";
		xrLabel22.Padding = new PaddingInfo(2, 2, 0, 0, 96f);
		xrLabel22.SizeF = new SizeF(176.0417f, 23f);
		xrLabel22.Text = "xrLabel22";
		xrLabel23.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari_unvan2_nokta_vuruslu")
		});
		xrLabel23.LocationFloat = new PointFloat(0f, 46f);
		xrLabel23.Name = "xrLabel23";
		xrLabel23.Padding = new PaddingInfo(2, 2, 0, 0, 96f);
		xrLabel23.SizeF = new SizeF(176.0417f, 23f);
		xrLabel23.Text = "xrLabel23";
		xrLabel24.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari_vdaire_adi_nkta_vuruslu")
		});
		xrLabel24.LocationFloat = new PointFloat(0.0001907349f, 253.2917f);
		xrLabel24.Name = "xrLabel24";
		xrLabel24.Padding = new PaddingInfo(2, 2, 0, 0, 96f);
		xrLabel24.SizeF = new SizeF(176.0417f, 23f);
		xrLabel24.Text = "xrLabel24";
		xrLabel25.DataBindings.AddRange(new XRBinding[1]
		{
			new XRBinding("Text", null, "cari_vdaire_no_nokta_vuruslu")
		});
		xrLabel25.LocationFloat = new PointFloat(315.4584f, 253.2917f);
		xrLabel25.Name = "xrLabel25";
		xrLabel25.Padding = new PaddingInfo(2, 2, 0, 0, 96f);
		xrLabel25.SizeF = new SizeF(152.0834f, 23f);
		xrLabel25.Text = "xrLabel25";
		fatura_adresi_adr_ilce_nokta_vuruslu.FieldType = FieldType.String;
		fatura_adresi_adr_ilce_nokta_vuruslu.Name = "fatura_adresi_adr_ilce_nokta_vuruslu";
		fatura_adresi_adr_ilce_nokta_vuruslu.Scripts.OnGetValue = "fatura_adresi_adr_ilce_nokta_vuruslu_GetValue";
		fatura_adresi_adr_il_nokta_vuruslu.FieldType = FieldType.String;
		fatura_adresi_adr_il_nokta_vuruslu.Name = "fatura_adresi_adr_il_nokta_vuruslu";
		fatura_adresi_adr_il_nokta_vuruslu.Scripts.OnGetValue = "fatura_adresi_adr_il_nokta_vuruslu_GetValue";
		sevk_adresi_adr_cadde_nokta_vuruslu.FieldType = FieldType.String;
		sevk_adresi_adr_cadde_nokta_vuruslu.Name = "sevk_adresi_adr_cadde_nokta_vuruslu";
		sevk_adresi_adr_cadde_nokta_vuruslu.Scripts.OnGetValue = "sevk_adresi_adr_cadde_nokta_vuruslu_GetValue";
		sevk_adresi_adr_sokak_nokta_vuruslu.FieldType = FieldType.String;
		sevk_adresi_adr_sokak_nokta_vuruslu.Name = "sevk_adresi_adr_sokak_nokta_vuruslu";
		sevk_adresi_adr_sokak_nokta_vuruslu.Scripts.OnGetValue = "sevk_adresi_adr_sokak_nokta_vuruslu_GetValue";
		sevk_adresi_adr_posta_kodu_nokta_vuruslu.FieldType = FieldType.String;
		sevk_adresi_adr_posta_kodu_nokta_vuruslu.Name = "sevk_adresi_adr_posta_kodu_nokta_vuruslu";
		sevk_adresi_adr_posta_kodu_nokta_vuruslu.Scripts.OnGetValue = "sevk_adresi_adr_posta_kodu_nokta_vuruslu_GetValue";
		sevk_adresi_adr_ilce_nokta_vuruslu.FieldType = FieldType.String;
		sevk_adresi_adr_ilce_nokta_vuruslu.Name = "sevk_adresi_adr_ilce_nokta_vuruslu";
		sevk_adresi_adr_ilce_nokta_vuruslu.Scripts.OnGetValue = "sevk_adresi_adr_ilce_nokta_vuruslu_GetValue";
		sevk_adresi_adr_il_nokta_vuruslu.FieldType = FieldType.String;
		sevk_adresi_adr_il_nokta_vuruslu.Name = "sevk_adresi_adr_il_nokta_vuruslu";
		sevk_adresi_adr_il_nokta_vuruslu.Scripts.OnGetValue = "sevk_adresi_adr_il_nokta_vuruslu_GetValue";
		xrLabel1.LocationFloat = new PointFloat(176.0417f, 138.2917f);
		xrLabel1.Name = "xrLabel1";
		xrLabel1.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel1.SizeF = new SizeF(139.4167f, 23f);
		xrLabel1.Text = " ";
		xrLabel2.LocationFloat = new PointFloat(0f, 184.2917f);
		xrLabel2.Name = "xrLabel2";
		xrLabel2.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel2.SizeF = new SizeF(176.0417f, 23f);
		xrLabel2.Text = "                         ";
		xrLabel3.LocationFloat = new PointFloat(0f, 207.2917f);
		xrLabel3.Name = "xrLabel3";
		xrLabel3.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel3.SizeF = new SizeF(176.0417f, 23f);
		xrLabel3.Text = "                         ";
		xrLabel4.LocationFloat = new PointFloat(0.0001589457f, 69.29166f);
		xrLabel4.Name = "xrLabel4";
		xrLabel4.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel4.SizeF = new SizeF(176.0417f, 23f);
		xrLabel4.Text = " ";
		xrLabel26.LocationFloat = new PointFloat(0.0001589457f, 161.2917f);
		xrLabel26.Name = "xrLabel26";
		xrLabel26.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel26.SizeF = new SizeF(176.0417f, 23f);
		xrLabel26.Text = "                         ";
		xrLabel28.LocationFloat = new PointFloat(0f, 230.2917f);
		xrLabel28.Name = "xrLabel28";
		xrLabel28.Padding = new PaddingInfo(2, 2, 0, 0, 100f);
		xrLabel28.SizeF = new SizeF(176.0417f, 23f);
		xrLabel28.Text = "                         ";
		base.Bands.AddRange(new Band[4] { Detail, TopMargin, BottomMargin, DetailReport });
		base.CalculatedFields.AddRange(new CalculatedField[22]
		{
			hesap_adi_satir_nokta_vuruslu, hesap_kodu_satir_nokta_vuruslu, miktar_satir_nokta_vuruslu, toplam_fiyat_net_satir_nokta_vuruslu, ara_toplam_nokta_vuruslu, toplam_iskonto_tutari_nokta_vuruslu, toplam_vergi_tutari_nokta_vuruslu, yekun_nokta_vuruslu, cari_unvan1_nokta_vuruslu, cari_unvan2_nokta_vuruslu,
			cari_vdaire_adi_nkta_vuruslu, cari_vdaire_no_nokta_vuruslu, fatura_adresi_adr_cadde_nokta_vuruslu, fatura_adresi_adr_sokak_nokta_vuruslu, fatura_adresi_adr_posta_kodu_nokta_vuruslu, fatura_adresi_adr_ilce_nokta_vuruslu, fatura_adresi_adr_il_nokta_vuruslu, sevk_adresi_adr_cadde_nokta_vuruslu, sevk_adresi_adr_sokak_nokta_vuruslu, sevk_adresi_adr_posta_kodu_nokta_vuruslu,
			sevk_adresi_adr_ilce_nokta_vuruslu, sevk_adresi_adr_il_nokta_vuruslu
		});
		base.DataSource = bindingSource1;
		base.ExportOptions.Csv.EncodingType = EncodingType.UTF8;
		base.ExportOptions.Text.EncodingType = EncodingType.UTF8;
		base.ExportOptions.Text.Separator = " ";
		base.Margins = new Margins(49, 49, 48, 173);
		base.PageHeight = 1169;
		base.PageWidth = 827;
		base.PaperKind = PaperKind.A4;
		base.ScriptsSource = componentResourceManager.GetString("$this.ScriptsSource");
		base.Version = "13.1";
		((ISupportInitialize)bindingSource1).EndInit();
		((ISupportInitialize)this).EndInit();
	}
}
