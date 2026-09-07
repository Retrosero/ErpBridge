using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using Fora.App.Win.Mikro.GenelFormlar;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;
using fastJSON;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public class KriterDuzenleme : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _kullaniciparametreleri = new Parametreler();

	private GenelEvrakKriter genel_evrak_kriter;

	private BindingList<AlanOperatorDeger> bindinglist_alan1;

	private BindingList<AlanOperatorDeger> bindinglist_alan2;

	private BindingList<AlanIslemTipiDeger> bindinglist_degistirilecek_alanlar;

	private bool DegisiklikVar;

	private string AktifKullanici;

	private IContainer components;

	private ListBoxControl lb_kriterler;

	private XtraTabControl tc_parametreler;

	private XtraTabPage xtraTabPage3;

	private LabelControl labelControl17;

	private Label label44;

	private System.Windows.Forms.ComboBox arama_alan1_baglac;

	private GridControl gc_aranacak1;

	private AdvBandedGridView advBandedGridView_master;

	private BandedGridColumn gc_operator;

	private BandedGridColumn gc_alan;

	private System.Windows.Forms.ComboBox arama_alan1_alan2_baglac;

	private System.Windows.Forms.ComboBox arama_alan2_baglac;

	private Label label1;

	private BandedGridColumn gc_deger;

	private GridBand gridBand10;

	private GridBand gridBand1;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_aranacak_alan1;

	private GridView repositoryItemGridLookUpEdit1View;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_aranacak_operator1;

	private GridView gridView1;

	private GridControl gc_aranacak2;

	private AdvBandedGridView advBandedGridView1;

	private GridBand gridBand2;

	private BandedGridColumn bandedGridColumn1;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_aranacak_alan2;

	private GridView gridView3;

	private BandedGridColumn bandedGridColumn2;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_aranacak_operator2;

	private GridView gridView2;

	private BandedGridColumn bandedGridColumn3;

	private GridBand gridBand3;

	private Label label4;

	private XtraTabPage xtraTabPage_degistirilecek_alanlar;

	private Label label2;

	private GridControl gc_degistirilecek_alanlar;

	private AdvBandedGridView advBandedGridView2;

	private BandedGridColumn gc_ek_kontrol_yap;

	private BandedGridColumn gc_aranacak_alan;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_aranacak_alan;

	private GridView gridView7;

	private BandedGridColumn gc_kullanilan_operator;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_operator;

	private GridView gridView8;

	private BandedGridColumn gc_aranacak_deger;

	private BandedGridColumn gc_degistirilecek_alan;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_degistirilecek_alan;

	private GridView gridView4;

	private BandedGridColumn gc_islem_tipi;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_islem_tipi;

	private GridView gridView5;

	private BandedGridColumn gc_deger_tipi;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_deger_tipi;

	private GridView gridView6;

	private BandedGridColumn gc_ozel_deger;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_aranacak_deger;

	private GridView gridView9;

	private BandedGridColumn gc_islem_sirasi;

	private BandedGridColumn gc_yapilacak_islem;

	private BandedGridColumn gc_ozel_islem_tipi;

	private BandedGridColumn gc_ozel_islem_yapilacak_alan1;

	private BandedGridColumn gc_ozel_islem_yapilacak_alan2;

	private BandedGridColumn gc_ozel_islem_yapilacak_alan3;

	private BandedGridColumn gc_ozel_islem_yapilacak_alan4;

	private BandedGridColumn gc_ozel_islem_yapilacak_alan5;

	private BandedGridColumn gc_ozel_islem_parametre1_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre1_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre2_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre2_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre3_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre3_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre4_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre4_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre5_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre5_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre6_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre6_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre7_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre7_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre8_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre8_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre9_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre9_ozel_deger;

	private BandedGridColumn gc_ozel_islem_parametre10_deger_tipi;

	private BandedGridColumn gc_ozel_islem_parametre10_ozel_deger;

	private GridBand gridBand5;

	private GridBand gridBand6;

	private GridBand gridBand4;

	private GridBand gridBand7;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_yapilacak_islem;

	private GridView gridView10;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_ozel_islem_tipi;

	private GridView gridView11;

	private XtraTabPage xtraTabPage1;

	private RichTextBox richTextBox1;

	private TextEdit te_kullanici_adi;

	private LabelControl labelControl1;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem degisiklikleriKaydetToolStripMenuItem;

	private ToolStripMenuItem kriterSilToolStripMenuItem;

	private ToolStripMenuItem kriterEkleToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem dosyayaYazToolStripMenuItem;

	private ToolStripMenuItem dosyadanOkuToolStripMenuItem;

	public KriterDuzenleme(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		arama_alan1_baglac.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_Baglac));
		arama_alan1_baglac.ValueMember = "ID";
		arama_alan1_baglac.DisplayMember = "Isim";
		arama_alan2_baglac.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_Baglac));
		arama_alan2_baglac.ValueMember = "ID";
		arama_alan2_baglac.DisplayMember = "Isim";
		arama_alan1_alan2_baglac.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_Baglac));
		arama_alan1_alan2_baglac.ValueMember = "ID";
		arama_alan1_alan2_baglac.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_aranacak_alan.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_KriterAramaAlanlari));
		repositoryItemGridLookUpEdit_aranacak_alan.ValueMember = "ID";
		repositoryItemGridLookUpEdit_aranacak_alan.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_aranacak_alan.PopulateViewColumns();
		foreach (GridColumn column in repositoryItemGridLookUpEdit_aranacak_alan.View.Columns)
		{
			column.Visible = false;
		}
		repositoryItemGridLookUpEdit_aranacak_alan.View.Columns[1].Caption = "Alanlar";
		repositoryItemGridLookUpEdit_aranacak_alan.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_aranacak_alan.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_aranacak_alan.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_aranacak_alan.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_aranacak_alan.AutoComplete = false;
		repositoryItemGridLookUpEdit_aranacak_alan1.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_KriterAramaAlanlari));
		repositoryItemGridLookUpEdit_aranacak_alan1.ValueMember = "ID";
		repositoryItemGridLookUpEdit_aranacak_alan1.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_aranacak_alan1.PopulateViewColumns();
		foreach (GridColumn column2 in repositoryItemGridLookUpEdit_aranacak_alan1.View.Columns)
		{
			column2.Visible = false;
		}
		repositoryItemGridLookUpEdit_aranacak_alan1.View.Columns[1].Caption = "Alanlar";
		repositoryItemGridLookUpEdit_aranacak_alan1.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_aranacak_alan1.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_aranacak_alan1.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_aranacak_alan1.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_aranacak_alan1.AutoComplete = false;
		repositoryItemGridLookUpEdit_aranacak_alan2.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_KriterAramaAlanlari));
		repositoryItemGridLookUpEdit_aranacak_alan2.ValueMember = "ID";
		repositoryItemGridLookUpEdit_aranacak_alan2.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_aranacak_alan2.PopulateViewColumns();
		foreach (GridColumn column3 in repositoryItemGridLookUpEdit_aranacak_alan2.View.Columns)
		{
			column3.Visible = false;
		}
		repositoryItemGridLookUpEdit_aranacak_alan2.View.Columns[1].Caption = "Alanlar";
		repositoryItemGridLookUpEdit_aranacak_alan2.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_aranacak_alan2.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_aranacak_alan2.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_aranacak_alan2.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_aranacak_alan2.AutoComplete = false;
		repositoryItemGridLookUpEdit_operator.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_Operator));
		repositoryItemGridLookUpEdit_operator.ValueMember = "ID";
		repositoryItemGridLookUpEdit_operator.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_operator.PopulateViewColumns();
		foreach (GridColumn column4 in repositoryItemGridLookUpEdit_operator.View.Columns)
		{
			column4.Visible = false;
		}
		repositoryItemGridLookUpEdit_operator.View.Columns[1].Caption = "Operatorler";
		repositoryItemGridLookUpEdit_operator.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_operator.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_operator.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_operator.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_operator.AutoComplete = false;
		repositoryItemGridLookUpEdit_aranacak_operator1.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_Operator));
		repositoryItemGridLookUpEdit_aranacak_operator1.ValueMember = "ID";
		repositoryItemGridLookUpEdit_aranacak_operator1.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_aranacak_operator1.PopulateViewColumns();
		foreach (GridColumn column5 in repositoryItemGridLookUpEdit_aranacak_operator1.View.Columns)
		{
			column5.Visible = false;
		}
		repositoryItemGridLookUpEdit_aranacak_operator1.View.Columns[1].Caption = "Operatorler";
		repositoryItemGridLookUpEdit_aranacak_operator1.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_aranacak_operator1.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_aranacak_operator1.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_aranacak_operator1.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_aranacak_operator1.AutoComplete = false;
		repositoryItemGridLookUpEdit_aranacak_operator2.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_Operator));
		repositoryItemGridLookUpEdit_aranacak_operator2.ValueMember = "ID";
		repositoryItemGridLookUpEdit_aranacak_operator2.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_aranacak_operator2.PopulateViewColumns();
		foreach (GridColumn column6 in repositoryItemGridLookUpEdit_aranacak_operator2.View.Columns)
		{
			column6.Visible = false;
		}
		repositoryItemGridLookUpEdit_aranacak_operator2.View.Columns[1].Caption = "Operatorler";
		repositoryItemGridLookUpEdit_aranacak_operator2.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_aranacak_operator2.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_aranacak_operator2.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_aranacak_operator2.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_aranacak_operator2.AutoComplete = false;
		repositoryItemGridLookUpEdit_degistirilecek_alan.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_KriterDegistirmeAlanlari));
		repositoryItemGridLookUpEdit_degistirilecek_alan.ValueMember = "ID";
		repositoryItemGridLookUpEdit_degistirilecek_alan.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_degistirilecek_alan.PopulateViewColumns();
		foreach (GridColumn column7 in repositoryItemGridLookUpEdit_degistirilecek_alan.View.Columns)
		{
			column7.Visible = false;
		}
		repositoryItemGridLookUpEdit_degistirilecek_alan.View.Columns[1].Caption = "Alanlar";
		repositoryItemGridLookUpEdit_degistirilecek_alan.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_degistirilecek_alan.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_degistirilecek_alan.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_degistirilecek_alan.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_degistirilecek_alan.AutoComplete = false;
		repositoryItemGridLookUpEdit_islem_tipi.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_IslemTipi));
		repositoryItemGridLookUpEdit_islem_tipi.ValueMember = "ID";
		repositoryItemGridLookUpEdit_islem_tipi.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_islem_tipi.PopulateViewColumns();
		foreach (GridColumn column8 in repositoryItemGridLookUpEdit_islem_tipi.View.Columns)
		{
			column8.Visible = false;
		}
		repositoryItemGridLookUpEdit_islem_tipi.View.Columns[1].Caption = "İşlem tipleri";
		repositoryItemGridLookUpEdit_islem_tipi.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_islem_tipi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_islem_tipi.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_islem_tipi.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_islem_tipi.AutoComplete = false;
		repositoryItemGridLookUpEdit_deger_tipi.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_DegerTipi));
		repositoryItemGridLookUpEdit_deger_tipi.ValueMember = "ID";
		repositoryItemGridLookUpEdit_deger_tipi.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_deger_tipi.PopulateViewColumns();
		foreach (GridColumn column9 in repositoryItemGridLookUpEdit_deger_tipi.View.Columns)
		{
			column9.Visible = false;
		}
		repositoryItemGridLookUpEdit_deger_tipi.View.Columns[1].Caption = "Değer tipleri";
		repositoryItemGridLookUpEdit_deger_tipi.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_deger_tipi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_deger_tipi.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_deger_tipi.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_deger_tipi.AutoComplete = false;
		repositoryItemGridLookUpEdit_yapilacak_islem.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_YapilacakIslem));
		repositoryItemGridLookUpEdit_yapilacak_islem.ValueMember = "ID";
		repositoryItemGridLookUpEdit_yapilacak_islem.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_yapilacak_islem.PopulateViewColumns();
		foreach (GridColumn column10 in repositoryItemGridLookUpEdit_yapilacak_islem.View.Columns)
		{
			column10.Visible = false;
		}
		repositoryItemGridLookUpEdit_yapilacak_islem.View.Columns[1].Caption = "Yapılacak işlem";
		repositoryItemGridLookUpEdit_yapilacak_islem.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_yapilacak_islem.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_yapilacak_islem.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_yapilacak_islem.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_yapilacak_islem.AutoComplete = false;
		repositoryItemGridLookUpEdit_ozel_islem_tipi.DataSource = GenelUtilityWin.EnumToDataTable(typeof(enum_OzelIslem));
		repositoryItemGridLookUpEdit_ozel_islem_tipi.ValueMember = "ID";
		repositoryItemGridLookUpEdit_ozel_islem_tipi.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_ozel_islem_tipi.PopulateViewColumns();
		foreach (GridColumn column11 in repositoryItemGridLookUpEdit_ozel_islem_tipi.View.Columns)
		{
			column11.Visible = false;
		}
		repositoryItemGridLookUpEdit_ozel_islem_tipi.View.Columns[1].Caption = "Özel işlemler";
		repositoryItemGridLookUpEdit_ozel_islem_tipi.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_ozel_islem_tipi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_ozel_islem_tipi.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_ozel_islem_tipi.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_ozel_islem_tipi.AutoComplete = false;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		KullanicilariListele();
	}

	private void KullanicilariListele()
	{
		lb_kriterler.Items.Clear();
		foreach (string item in GenelEvrakKriterParametreleri.GetKriterAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			lb_kriterler.Items.Add(item);
		}
	}

	private void lb_kullanicilar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lb_kriterler.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar && AktifKullanici != lb_kriterler.SelectedValue.ToString())
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		if (AktifKullanici == lb_kriterler.SelectedValue.ToString())
		{
			flag = false;
		}
		if (flag)
		{
			tc_parametreler.Enabled = true;
			genel_evrak_kriter = new GenelEvrakKriter();
			AktifKullanici = lb_kriterler.SelectedValue.ToString();
			_kullaniciparametreleri = ParametrelerDefault.GenelAktarimKriter(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "GenelAktarim", "", "Kriter", AktifKullanici);
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			lb_kriterler.SelectedItem = AktifKullanici;
		}
	}

	private void EkranBilgiGuncelle()
	{
		te_kullanici_adi.Text = AktifKullanici;
		arama_alan1_baglac.SelectedValue = _kullaniciparametreleri._GetParametre("arama_alan1_baglac")._GetInt;
		arama_alan2_baglac.SelectedValue = _kullaniciparametreleri._GetParametre("arama_alan2_baglac")._GetInt;
		arama_alan1_alan2_baglac.SelectedValue = _kullaniciparametreleri._GetParametre("arama_alan1_alan2_baglac")._GetInt;
		string getString = _kullaniciparametreleri._GetParametre("arama_alan1")._GetString;
		string getString2 = _kullaniciparametreleri._GetParametre("arama_alan2")._GetString;
		string getString3 = _kullaniciparametreleri._GetParametre("degistirilecek_alanlar")._GetString;
		if (getString != "")
		{
			genel_evrak_kriter.arama_alan1 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString);
		}
		else
		{
			genel_evrak_kriter.arama_alan1 = new List<AlanOperatorDeger>();
		}
		if (getString2 != "")
		{
			genel_evrak_kriter.arama_alan2 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString2);
		}
		else
		{
			genel_evrak_kriter.arama_alan2 = new List<AlanOperatorDeger>();
		}
		if (getString3 != "")
		{
			genel_evrak_kriter.degistirilecek_alanlar = JSON.Instance.ToObject<List<AlanIslemTipiDeger>>(getString3);
		}
		else
		{
			genel_evrak_kriter.degistirilecek_alanlar = new List<AlanIslemTipiDeger>();
		}
		bindinglist_alan1 = new BindingList<AlanOperatorDeger>();
		foreach (AlanOperatorDeger item in genel_evrak_kriter.arama_alan1)
		{
			bindinglist_alan1.Add(item);
		}
		bindinglist_alan2 = new BindingList<AlanOperatorDeger>();
		foreach (AlanOperatorDeger item2 in genel_evrak_kriter.arama_alan2)
		{
			bindinglist_alan2.Add(item2);
		}
		bindinglist_degistirilecek_alanlar = new BindingList<AlanIslemTipiDeger>();
		foreach (AlanIslemTipiDeger item3 in genel_evrak_kriter.degistirilecek_alanlar)
		{
			bindinglist_degistirilecek_alanlar.Add(item3);
		}
		gc_aranacak1.DataSource = bindinglist_alan1;
		gc_aranacak2.DataSource = bindinglist_alan2;
		gc_degistirilecek_alanlar.DataSource = bindinglist_degistirilecek_alanlar;
	}

	private void KullaniciParametreKaydet()
	{
		te_kullanici_adi.Text = AktifKullanici;
		_kullaniciparametreleri._GetParametre("kriter_adi")._SetString = AktifKullanici;
		_kullaniciparametreleri._GetParametre("arama_alan1_baglac")._SetInt = (int)arama_alan1_baglac.SelectedValue;
		_kullaniciparametreleri._GetParametre("arama_alan2_baglac")._SetInt = (int)arama_alan2_baglac.SelectedValue;
		_kullaniciparametreleri._GetParametre("arama_alan1_alan2_baglac")._SetInt = (int)arama_alan1_alan2_baglac.SelectedValue;
		genel_evrak_kriter.arama_alan1 = new List<AlanOperatorDeger>();
		foreach (AlanOperatorDeger item in bindinglist_alan1)
		{
			genel_evrak_kriter.arama_alan1.Add(item);
		}
		genel_evrak_kriter.arama_alan2 = new List<AlanOperatorDeger>();
		foreach (AlanOperatorDeger item2 in bindinglist_alan2)
		{
			genel_evrak_kriter.arama_alan2.Add(item2);
		}
		genel_evrak_kriter.degistirilecek_alanlar = bindinglist_degistirilecek_alanlar.OrderBy((AlanIslemTipiDeger x) => x.islem_sirasi).ToList();
		if (genel_evrak_kriter.arama_alan1.Count != 0)
		{
			_kullaniciparametreleri._GetParametre("arama_alan1")._SetString = JSON.Instance.ToJSON(genel_evrak_kriter.arama_alan1);
		}
		else
		{
			_kullaniciparametreleri._GetParametre("arama_alan1")._SetString = "";
		}
		if (genel_evrak_kriter.arama_alan2.Count != 0)
		{
			_kullaniciparametreleri._GetParametre("arama_alan2")._SetString = JSON.Instance.ToJSON(genel_evrak_kriter.arama_alan2);
		}
		else
		{
			_kullaniciparametreleri._GetParametre("arama_alan2")._SetString = "";
		}
		if (genel_evrak_kriter.degistirilecek_alanlar.Count != 0)
		{
			_kullaniciparametreleri._GetParametre("degistirilecek_alanlar")._SetString = JSON.Instance.ToJSON(genel_evrak_kriter.degistirilecek_alanlar);
		}
		else
		{
			_kullaniciparametreleri._GetParametre("degistirilecek_alanlar")._SetString = "";
		}
		if (ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri))
		{
			MessageBox.Show("Ayarlar kayıt edildi.", "İşlem başarılı", MessageBoxButtons.OK);
			_kullaniciparametreleri = ParametrelerDefault.GenelAktarimKriter(AktifKullanici);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _kullaniciparametreleri, "GenelAktarim", "", "Kriter", AktifKullanici);
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			MessageBox.Show("Ayarlar kayıt edilemedi!", "İşlem başarısız", MessageBoxButtons.OK);
		}
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void advBandedGridView_master_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		_ = (AdvBandedGridView)sender;
		AlanOperatorDeger alanOperatorDeger = (AlanOperatorDeger)e.Row;
		string fieldName;
		if (e.IsSetData)
		{
			fieldName = e.Column.FieldName;
			if (!(fieldName == "gc_alan"))
			{
				if (fieldName == "gc_operator")
				{
					alanOperatorDeger.kullanilan_operator = (enum_Operator)e.Value;
				}
			}
			else
			{
				alanOperatorDeger.aranacak_alan = (enum_KriterAramaAlanlari)e.Value;
			}
		}
		if (!e.IsGetData)
		{
			return;
		}
		fieldName = e.Column.FieldName;
		if (!(fieldName == "gc_alan"))
		{
			if (fieldName == "gc_operator")
			{
				e.Value = (int)alanOperatorDeger.kullanilan_operator;
			}
		}
		else
		{
			e.Value = (int)alanOperatorDeger.aranacak_alan;
		}
	}

	private void advBandedGridView2_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		_ = (AdvBandedGridView)sender;
		AlanIslemTipiDeger alanIslemTipiDeger = (AlanIslemTipiDeger)e.Row;
		if (e.IsSetData)
		{
			switch (e.Column.FieldName)
			{
			case "gc_degistirilecek_alan":
				alanIslemTipiDeger.degistirilecek_alan = (enum_KriterDegistirmeAlanlari)e.Value;
				break;
			case "gc_islem_tipi":
				alanIslemTipiDeger.islem_tipi = (enum_IslemTipi)e.Value;
				break;
			case "gc_deger_tipi":
				alanIslemTipiDeger.deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_aranacak_alan":
				alanIslemTipiDeger.aranacak_alan = (enum_KriterAramaAlanlari)e.Value;
				break;
			case "gc_kullanilan_operator":
				alanIslemTipiDeger.kullanilan_operator = (enum_Operator)e.Value;
				break;
			case "gc_yapilacak_islem":
				alanIslemTipiDeger.yapilacak_islem = (enum_YapilacakIslem)e.Value;
				break;
			case "gc_ozel_islem_tipi":
				alanIslemTipiDeger.ozel_islem_tipi = (enum_OzelIslem)e.Value;
				break;
			case "gc_ozel_islem_parametre1_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre1_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre2_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre2_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre3_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre3_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre4_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre4_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre5_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre5_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre6_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre6_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre7_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre7_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre8_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre8_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre9_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre9_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_parametre10_deger_tipi":
				alanIslemTipiDeger.ozel_islem_parametre10_deger_tipi = (enum_DegerTipi)e.Value;
				break;
			case "gc_ozel_islem_yapilacak_alan1":
				alanIslemTipiDeger.ozel_islem_yapilacak_alan1 = (enum_KriterDegistirmeAlanlari)e.Value;
				break;
			case "gc_ozel_islem_yapilacak_alan2":
				alanIslemTipiDeger.ozel_islem_yapilacak_alan2 = (enum_KriterDegistirmeAlanlari)e.Value;
				break;
			case "gc_ozel_islem_yapilacak_alan3":
				alanIslemTipiDeger.ozel_islem_yapilacak_alan3 = (enum_KriterDegistirmeAlanlari)e.Value;
				break;
			case "gc_ozel_islem_yapilacak_alan4":
				alanIslemTipiDeger.ozel_islem_yapilacak_alan4 = (enum_KriterDegistirmeAlanlari)e.Value;
				break;
			case "gc_ozel_islem_yapilacak_alan5":
				alanIslemTipiDeger.ozel_islem_yapilacak_alan5 = (enum_KriterDegistirmeAlanlari)e.Value;
				break;
			}
		}
		if (e.IsGetData)
		{
			switch (e.Column.FieldName)
			{
			case "gc_degistirilecek_alan":
				e.Value = (int)alanIslemTipiDeger.degistirilecek_alan;
				break;
			case "gc_islem_tipi":
				e.Value = (int)alanIslemTipiDeger.islem_tipi;
				break;
			case "gc_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.deger_tipi;
				break;
			case "gc_aranacak_alan":
				e.Value = (int)alanIslemTipiDeger.aranacak_alan;
				break;
			case "gc_kullanilan_operator":
				e.Value = (int)alanIslemTipiDeger.kullanilan_operator;
				break;
			case "gc_yapilacak_islem":
				e.Value = (int)alanIslemTipiDeger.yapilacak_islem;
				break;
			case "gc_ozel_islem_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_tipi;
				break;
			case "gc_ozel_islem_parametre1_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre1_deger_tipi;
				break;
			case "gc_ozel_islem_parametre2_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre2_deger_tipi;
				break;
			case "gc_ozel_islem_parametre3_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre3_deger_tipi;
				break;
			case "gc_ozel_islem_parametre4_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre4_deger_tipi;
				break;
			case "gc_ozel_islem_parametre5_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre5_deger_tipi;
				break;
			case "gc_ozel_islem_parametre6_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre6_deger_tipi;
				break;
			case "gc_ozel_islem_parametre7_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre7_deger_tipi;
				break;
			case "gc_ozel_islem_parametre8_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre8_deger_tipi;
				break;
			case "gc_ozel_islem_parametre9_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre9_deger_tipi;
				break;
			case "gc_ozel_islem_parametre10_deger_tipi":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_parametre10_deger_tipi;
				break;
			case "gc_ozel_islem_yapilacak_alan1":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_yapilacak_alan1;
				break;
			case "gc_ozel_islem_yapilacak_alan2":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_yapilacak_alan2;
				break;
			case "gc_ozel_islem_yapilacak_alan3":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_yapilacak_alan3;
				break;
			case "gc_ozel_islem_yapilacak_alan4":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_yapilacak_alan4;
				break;
			case "gc_ozel_islem_yapilacak_alan5":
				e.Value = (int)alanIslemTipiDeger.ozel_islem_yapilacak_alan5;
				break;
			}
		}
	}

	private void degisiklikleriKaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (lb_kriterler.SelectedValue != null)
		{
			KullaniciParametreKaydet();
		}
	}

	private void kriterSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (lb_kriterler.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				flag = true;
				break;
			case DialogResult.No:
				flag = true;
				break;
			case DialogResult.Cancel:
				flag = false;
				break;
			}
		}
		string text = lb_kriterler.SelectedValue.ToString();
		if (flag)
		{
			DialogResult dialogResult = MessageBox.Show(text + " kullanıcısını silmek istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dialogResult != DialogResult.Yes)
			{
				_ = 7;
				return;
			}
			GenelEvrakKriterParametreleri.KriterSil(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text);
			KullanicilariListele();
		}
	}

	private void kriterEkleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				break;
			case DialogResult.Cancel:
				return;
			}
		}
		TextSor textSor = new TextSor("Kriter adı", "DEVAM");
		if (textSor.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (string item in lb_kriterler.Items)
		{
			if (item == textSor.te_cevap.Text)
			{
				MessageBox.Show("Bu kriter adı daha önce kullanılmış. İşlem tamamlanamadı!");
				return;
			}
		}
		Parametreler parametreler = ParametrelerDefault.GenelAktarimKriter(textSor.te_cevap.Text);
		parametreler._GetParametre("kriter_adi")._SetString = textSor.te_cevap.Text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
		KullanicilariListele();
		lb_kriterler.SelectedItem = textSor.te_cevap.Text;
	}

	private void dosyayaYazToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (DegisiklikVar)
		{
			switch (MessageBox.Show("Kayıt edilmemiş değişiklikler var kaydetmek istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Yes:
				KullaniciParametreKaydet();
				break;
			case DialogResult.Cancel:
				return;
			}
		}
		try
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Fora parametreler|*.fpt|Tüm dosyalar|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.CreateNew);
				_kullaniciparametreleri.KullanimAlani = "GenelAktarimKriter";
				Parametreler.WriteToStream(_kullaniciparametreleri, fileStream, 2);
				fileStream.Close();
				fileStream.Dispose();
			}
		}
		catch
		{
			MessageBox.Show("Dosya oluşturulamadı!");
		}
	}

	private void dosyadanOkuToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Fora parametreler|*.fpt|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		TextSor textSor = new TextSor("Kriter adı", "DEVAM");
		if (textSor.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		foreach (string item in lb_kriterler.Items)
		{
			if (item == textSor.te_cevap.Text)
			{
				MessageBox.Show("Bu kriter adı daha önce kullanılmış. İşlem tamamlanamadı!");
				return;
			}
		}
		FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);
		Parametreler parametreler = Parametreler.ReadFromStream(fileStream);
		fileStream.Close();
		fileStream.Dispose();
		if (parametreler.KullanimAlani != "GenelAktarimKriter")
		{
			MessageBox.Show("Dosya uygun değil. İşlem tamamlanamadı!");
			return;
		}
		parametreler._GetParametre("kriter_adi")._SetString = textSor.te_cevap.Text;
		foreach (Parametre item2 in parametreler.ParametreListesi)
		{
			item2.EskiID = 0;
			item2.IDGuid = null;
			item2.ParametreAltGrubu = textSor.te_cevap.Text;
		}
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler);
		KullanicilariListele();
		lb_kriterler.SelectedItem = textSor.te_cevap.Text;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter.KriterDuzenleme));
		this.lb_kriterler = new DevExpress.XtraEditors.ListBoxControl();
		this.tc_parametreler = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage_degistirilecek_alanlar = new DevExpress.XtraTab.XtraTabPage();
		this.label2 = new System.Windows.Forms.Label();
		this.gc_degistirilecek_alanlar = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView2 = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_islem_sirasi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_yapilacak_islem = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_yapilacak_islem = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView10 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gridBand6 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_ek_kontrol_yap = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_aranacak_alan = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_aranacak_alan = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView7 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_kullanilan_operator = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_operator = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView8 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_aranacak_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand4 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_degistirilecek_alan = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_degistirilecek_alan = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView4 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_islem_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_islem_tipi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView5 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_deger_tipi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView6 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand7 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_ozel_islem_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_ozel_islem_tipi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView11 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_ozel_islem_yapilacak_alan1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_yapilacak_alan2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_yapilacak_alan3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_yapilacak_alan4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_yapilacak_alan5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre1_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre1_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre2_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre2_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre3_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre3_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre4_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre4_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre5_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre5_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre6_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre6_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre7_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre7_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre8_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre8_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre9_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre9_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre10_deger_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_ozel_islem_parametre10_ozel_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_aranacak_deger = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView9 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.te_kullanici_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.label4 = new System.Windows.Forms.Label();
		this.gc_aranacak2 = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView1 = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_aranacak_alan2 = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView3 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_aranacak_operator2 = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.arama_alan2_baglac = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.arama_alan1_alan2_baglac = new System.Windows.Forms.ComboBox();
		this.gc_aranacak1 = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView_master = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand10 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_alan = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_aranacak_alan1 = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.repositoryItemGridLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_operator = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_aranacak_operator1 = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_deger = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.arama_alan1_baglac = new System.Windows.Forms.ComboBox();
		this.label44 = new System.Windows.Forms.Label();
		this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
		this.richTextBox1 = new System.Windows.Forms.RichTextBox();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.degisiklikleriKaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kriterSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kriterEkleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.dosyayaYazToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.dosyadanOkuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.lb_kriterler).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).BeginInit();
		this.tc_parametreler.SuspendLayout();
		this.xtraTabPage_degistirilecek_alanlar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.gc_degistirilecek_alanlar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_yapilacak_islem).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView10).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_alan).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView7).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_operator).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView8).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_degistirilecek_alan).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_islem_tipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_deger_tipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_ozel_islem_tipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView11).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_deger).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView9).BeginInit();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gc_aranacak2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_alan2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_operator2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gc_aranacak1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_alan1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_operator1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		this.xtraTabPage1.SuspendLayout();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.lb_kriterler.Location = new System.Drawing.Point(12, 58);
		this.lb_kriterler.Name = "lb_kriterler";
		this.lb_kriterler.Size = new System.Drawing.Size(154, 496);
		this.lb_kriterler.TabIndex = 5;
		this.lb_kriterler.SelectedValueChanged += new System.EventHandler(lb_kullanicilar_SelectedValueChanged);
		this.tc_parametreler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametreler.Location = new System.Drawing.Point(171, 35);
		this.tc_parametreler.Name = "tc_parametreler";
		this.tc_parametreler.SelectedTabPage = this.xtraTabPage_degistirilecek_alanlar;
		this.tc_parametreler.Size = new System.Drawing.Size(865, 524);
		this.tc_parametreler.TabIndex = 0;
		this.tc_parametreler.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[3] { this.xtraTabPage3, this.xtraTabPage_degistirilecek_alanlar, this.xtraTabPage1 });
		this.xtraTabPage_degistirilecek_alanlar.Controls.Add(this.label2);
		this.xtraTabPage_degistirilecek_alanlar.Controls.Add(this.gc_degistirilecek_alanlar);
		this.xtraTabPage_degistirilecek_alanlar.Name = "xtraTabPage_degistirilecek_alanlar";
		this.xtraTabPage_degistirilecek_alanlar.Size = new System.Drawing.Size(859, 496);
		this.xtraTabPage_degistirilecek_alanlar.Text = "Yapılacak işlemler";
		this.label2.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label2.Location = new System.Drawing.Point(14, 6);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(828, 19);
		this.label2.TabIndex = 205;
		this.label2.Text = "YAPILACAK İŞLEMLER";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.gc_degistirilecek_alanlar.Cursor = System.Windows.Forms.Cursors.Default;
		this.gc_degistirilecek_alanlar.Location = new System.Drawing.Point(3, 28);
		this.gc_degistirilecek_alanlar.MainView = this.advBandedGridView2;
		this.gc_degistirilecek_alanlar.Name = "gc_degistirilecek_alanlar";
		this.gc_degistirilecek_alanlar.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[8] { this.repositoryItemGridLookUpEdit_degistirilecek_alan, this.repositoryItemGridLookUpEdit_islem_tipi, this.repositoryItemGridLookUpEdit_deger_tipi, this.repositoryItemGridLookUpEdit_aranacak_alan, this.repositoryItemGridLookUpEdit_operator, this.repositoryItemGridLookUpEdit_aranacak_deger, this.repositoryItemGridLookUpEdit_yapilacak_islem, this.repositoryItemGridLookUpEdit_ozel_islem_tipi });
		this.gc_degistirilecek_alanlar.Size = new System.Drawing.Size(849, 421);
		this.gc_degistirilecek_alanlar.TabIndex = 204;
		this.gc_degistirilecek_alanlar.UseEmbeddedNavigator = true;
		this.gc_degistirilecek_alanlar.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.advBandedGridView2 });
		this.gc_degistirilecek_alanlar.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.gc_degistirilecek_alanlar.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.advBandedGridView2.Appearance.BandPanel.BackColor = System.Drawing.Color.Red;
		this.advBandedGridView2.Appearance.BandPanel.BackColor2 = System.Drawing.Color.FromArgb(255, 128, 128);
		this.advBandedGridView2.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView2.Appearance.BandPanel.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView2.Appearance.BandPanel.Options.UseBackColor = true;
		this.advBandedGridView2.Appearance.BandPanel.Options.UseFont = true;
		this.advBandedGridView2.Appearance.BandPanel.Options.UseForeColor = true;
		this.advBandedGridView2.Appearance.BandPanel.Options.UseTextOptions = true;
		this.advBandedGridView2.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.advBandedGridView2.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView2.Appearance.BandPanelBackground.BackColor2 = System.Drawing.Color.FromArgb(255, 192, 128);
		this.advBandedGridView2.Appearance.BandPanelBackground.Options.UseBackColor = true;
		this.advBandedGridView2.Appearance.Empty.Font = new System.Drawing.Font("Tahoma", 11f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView2.Appearance.Empty.Options.UseFont = true;
		this.advBandedGridView2.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(192, 192, 0);
		this.advBandedGridView2.Appearance.EvenRow.Options.UseBackColor = true;
		this.advBandedGridView2.Appearance.FooterPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView2.Appearance.FooterPanel.Options.UseFont = true;
		this.advBandedGridView2.Appearance.GroupFooter.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView2.Appearance.GroupFooter.Options.UseFont = true;
		this.advBandedGridView2.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[4] { this.gridBand5, this.gridBand6, this.gridBand4, this.gridBand7 });
		this.advBandedGridView2.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[36]
		{
			this.gc_degistirilecek_alan, this.gc_islem_tipi, this.gc_deger_tipi, this.gc_ozel_deger, this.gc_ek_kontrol_yap, this.gc_aranacak_alan, this.gc_kullanilan_operator, this.gc_aranacak_deger, this.gc_islem_sirasi, this.gc_yapilacak_islem,
			this.gc_ozel_islem_tipi, this.gc_ozel_islem_parametre1_deger_tipi, this.gc_ozel_islem_parametre1_ozel_deger, this.gc_ozel_islem_parametre2_deger_tipi, this.gc_ozel_islem_parametre2_ozel_deger, this.gc_ozel_islem_parametre3_deger_tipi, this.gc_ozel_islem_parametre3_ozel_deger, this.gc_ozel_islem_parametre4_deger_tipi, this.gc_ozel_islem_parametre4_ozel_deger, this.gc_ozel_islem_parametre5_deger_tipi,
			this.gc_ozel_islem_parametre5_ozel_deger, this.gc_ozel_islem_parametre6_deger_tipi, this.gc_ozel_islem_parametre6_ozel_deger, this.gc_ozel_islem_parametre7_deger_tipi, this.gc_ozel_islem_parametre7_ozel_deger, this.gc_ozel_islem_parametre8_deger_tipi, this.gc_ozel_islem_parametre8_ozel_deger, this.gc_ozel_islem_parametre9_deger_tipi, this.gc_ozel_islem_parametre9_ozel_deger, this.gc_ozel_islem_parametre10_deger_tipi,
			this.gc_ozel_islem_parametre10_ozel_deger, this.gc_ozel_islem_yapilacak_alan1, this.gc_ozel_islem_yapilacak_alan2, this.gc_ozel_islem_yapilacak_alan3, this.gc_ozel_islem_yapilacak_alan4, this.gc_ozel_islem_yapilacak_alan5
		});
		this.advBandedGridView2.DetailVerticalIndent = 20;
		this.advBandedGridView2.GridControl = this.gc_degistirilecek_alanlar;
		this.advBandedGridView2.Name = "advBandedGridView2";
		this.advBandedGridView2.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView2.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView2.OptionsDetail.AllowExpandEmptyDetails = true;
		this.advBandedGridView2.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView2.OptionsSelection.MultiSelect = true;
		this.advBandedGridView2.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
		this.advBandedGridView2.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView2.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(advBandedGridView2_CustomUnboundColumnData);
		this.gridBand5.Caption = "Kriter bilgileri";
		this.gridBand5.Columns.Add(this.gc_islem_sirasi);
		this.gridBand5.Columns.Add(this.gc_yapilacak_islem);
		this.gridBand5.Name = "gridBand5";
		this.gridBand5.VisibleIndex = 0;
		this.gridBand5.Width = 142;
		this.gc_islem_sirasi.Caption = "İşlem sırası";
		this.gc_islem_sirasi.FieldName = "islem_sirasi";
		this.gc_islem_sirasi.Name = "gc_islem_sirasi";
		this.gc_islem_sirasi.Visible = true;
		this.gc_islem_sirasi.Width = 64;
		this.gc_yapilacak_islem.Caption = "Yapılacak işlem";
		this.gc_yapilacak_islem.ColumnEdit = this.repositoryItemGridLookUpEdit_yapilacak_islem;
		this.gc_yapilacak_islem.FieldName = "gc_yapilacak_islem";
		this.gc_yapilacak_islem.Name = "gc_yapilacak_islem";
		this.gc_yapilacak_islem.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_yapilacak_islem.Visible = true;
		this.gc_yapilacak_islem.Width = 78;
		this.repositoryItemGridLookUpEdit_yapilacak_islem.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_yapilacak_islem.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_yapilacak_islem.Name = "repositoryItemGridLookUpEdit_yapilacak_islem";
		this.repositoryItemGridLookUpEdit_yapilacak_islem.View = this.gridView10;
		this.gridView10.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView10.Name = "gridView10";
		this.gridView10.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView10.OptionsView.ShowGroupPanel = false;
		this.gridBand6.Caption = "Kontrol";
		this.gridBand6.Columns.Add(this.gc_ek_kontrol_yap);
		this.gridBand6.Columns.Add(this.gc_aranacak_alan);
		this.gridBand6.Columns.Add(this.gc_kullanilan_operator);
		this.gridBand6.Columns.Add(this.gc_aranacak_deger);
		this.gridBand6.Name = "gridBand6";
		this.gridBand6.VisibleIndex = 1;
		this.gridBand6.Width = 317;
		this.gc_ek_kontrol_yap.Caption = "Kontrol yap";
		this.gc_ek_kontrol_yap.FieldName = "ek_kontrol_yap";
		this.gc_ek_kontrol_yap.Name = "gc_ek_kontrol_yap";
		this.gc_ek_kontrol_yap.Visible = true;
		this.gc_ek_kontrol_yap.Width = 57;
		this.gc_aranacak_alan.Caption = "Aranacak alan";
		this.gc_aranacak_alan.ColumnEdit = this.repositoryItemGridLookUpEdit_aranacak_alan;
		this.gc_aranacak_alan.FieldName = "gc_aranacak_alan";
		this.gc_aranacak_alan.Name = "gc_aranacak_alan";
		this.gc_aranacak_alan.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_aranacak_alan.Visible = true;
		this.gc_aranacak_alan.Width = 80;
		this.repositoryItemGridLookUpEdit_aranacak_alan.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_aranacak_alan.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_aranacak_alan.Name = "repositoryItemGridLookUpEdit_aranacak_alan";
		this.repositoryItemGridLookUpEdit_aranacak_alan.View = this.gridView7;
		this.gridView7.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView7.Name = "gridView7";
		this.gridView7.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView7.OptionsView.ShowGroupPanel = false;
		this.gc_kullanilan_operator.Caption = "Operatör";
		this.gc_kullanilan_operator.ColumnEdit = this.repositoryItemGridLookUpEdit_operator;
		this.gc_kullanilan_operator.FieldName = "gc_kullanilan_operator";
		this.gc_kullanilan_operator.Name = "gc_kullanilan_operator";
		this.gc_kullanilan_operator.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_kullanilan_operator.Visible = true;
		this.repositoryItemGridLookUpEdit_operator.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_operator.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_operator.Name = "repositoryItemGridLookUpEdit_operator";
		this.repositoryItemGridLookUpEdit_operator.View = this.gridView8;
		this.gridView8.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView8.Name = "gridView8";
		this.gridView8.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView8.OptionsView.ShowGroupPanel = false;
		this.gc_aranacak_deger.Caption = "Aranacak değer";
		this.gc_aranacak_deger.FieldName = "aranacak_deger";
		this.gc_aranacak_deger.Name = "gc_aranacak_deger";
		this.gc_aranacak_deger.Visible = true;
		this.gc_aranacak_deger.Width = 105;
		this.gridBand4.Caption = "Veri değiştir";
		this.gridBand4.Columns.Add(this.gc_degistirilecek_alan);
		this.gridBand4.Columns.Add(this.gc_islem_tipi);
		this.gridBand4.Columns.Add(this.gc_deger_tipi);
		this.gridBand4.Columns.Add(this.gc_ozel_deger);
		this.gridBand4.Name = "gridBand4";
		this.gridBand4.VisibleIndex = 2;
		this.gridBand4.Width = 493;
		this.gc_degistirilecek_alan.Caption = "Değişecek alan";
		this.gc_degistirilecek_alan.ColumnEdit = this.repositoryItemGridLookUpEdit_degistirilecek_alan;
		this.gc_degistirilecek_alan.FieldName = "gc_degistirilecek_alan";
		this.gc_degistirilecek_alan.Name = "gc_degistirilecek_alan";
		this.gc_degistirilecek_alan.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_degistirilecek_alan.Visible = true;
		this.gc_degistirilecek_alan.Width = 101;
		this.repositoryItemGridLookUpEdit_degistirilecek_alan.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_degistirilecek_alan.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_degistirilecek_alan.Name = "repositoryItemGridLookUpEdit_degistirilecek_alan";
		this.repositoryItemGridLookUpEdit_degistirilecek_alan.View = this.gridView4;
		this.gridView4.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView4.Name = "gridView4";
		this.gridView4.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView4.OptionsView.ShowGroupPanel = false;
		this.gc_islem_tipi.Caption = "İşlem tipi";
		this.gc_islem_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_islem_tipi;
		this.gc_islem_tipi.FieldName = "gc_islem_tipi";
		this.gc_islem_tipi.Name = "gc_islem_tipi";
		this.gc_islem_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_islem_tipi.Visible = true;
		this.gc_islem_tipi.Width = 107;
		this.repositoryItemGridLookUpEdit_islem_tipi.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_islem_tipi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_islem_tipi.Name = "repositoryItemGridLookUpEdit_islem_tipi";
		this.repositoryItemGridLookUpEdit_islem_tipi.View = this.gridView5;
		this.gridView5.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView5.Name = "gridView5";
		this.gridView5.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView5.OptionsView.ShowGroupPanel = false;
		this.gc_deger_tipi.Caption = "Değer tipi";
		this.gc_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_deger_tipi.FieldName = "gc_deger_tipi";
		this.gc_deger_tipi.Name = "gc_deger_tipi";
		this.gc_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_deger_tipi.Visible = true;
		this.gc_deger_tipi.Width = 123;
		this.repositoryItemGridLookUpEdit_deger_tipi.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_deger_tipi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_deger_tipi.Name = "repositoryItemGridLookUpEdit_deger_tipi";
		this.repositoryItemGridLookUpEdit_deger_tipi.View = this.gridView6;
		this.gridView6.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView6.Name = "gridView6";
		this.gridView6.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView6.OptionsView.ShowGroupPanel = false;
		this.gc_ozel_deger.Caption = "Özel değer";
		this.gc_ozel_deger.FieldName = "ozel_deger";
		this.gc_ozel_deger.Name = "gc_ozel_deger";
		this.gc_ozel_deger.Visible = true;
		this.gc_ozel_deger.Width = 162;
		this.gridBand7.Caption = "Özel işlem";
		this.gridBand7.Columns.Add(this.gc_ozel_islem_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_yapilacak_alan1);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_yapilacak_alan2);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_yapilacak_alan3);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_yapilacak_alan4);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_yapilacak_alan5);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre1_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre1_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre2_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre2_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre3_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre3_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre4_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre4_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre5_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre5_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre6_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre6_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre7_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre7_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre8_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre8_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre9_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre9_ozel_deger);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre10_deger_tipi);
		this.gridBand7.Columns.Add(this.gc_ozel_islem_parametre10_ozel_deger);
		this.gridBand7.Name = "gridBand7";
		this.gridBand7.VisibleIndex = 3;
		this.gridBand7.Width = 3463;
		this.gc_ozel_islem_tipi.Caption = "Yapılacak özel işlem";
		this.gc_ozel_islem_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_ozel_islem_tipi;
		this.gc_ozel_islem_tipi.FieldName = "gc_ozel_islem_tipi";
		this.gc_ozel_islem_tipi.Name = "gc_ozel_islem_tipi";
		this.gc_ozel_islem_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_tipi.Visible = true;
		this.gc_ozel_islem_tipi.Width = 214;
		this.repositoryItemGridLookUpEdit_ozel_islem_tipi.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_ozel_islem_tipi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_ozel_islem_tipi.Name = "repositoryItemGridLookUpEdit_ozel_islem_tipi";
		this.repositoryItemGridLookUpEdit_ozel_islem_tipi.View = this.gridView11;
		this.gridView11.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView11.Name = "gridView11";
		this.gridView11.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView11.OptionsView.ShowGroupPanel = false;
		this.gc_ozel_islem_yapilacak_alan1.Caption = "İşlem yapılacak alan 1";
		this.gc_ozel_islem_yapilacak_alan1.ColumnEdit = this.repositoryItemGridLookUpEdit_degistirilecek_alan;
		this.gc_ozel_islem_yapilacak_alan1.FieldName = "gc_ozel_islem_yapilacak_alan1";
		this.gc_ozel_islem_yapilacak_alan1.Name = "gc_ozel_islem_yapilacak_alan1";
		this.gc_ozel_islem_yapilacak_alan1.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_yapilacak_alan1.Visible = true;
		this.gc_ozel_islem_yapilacak_alan1.Width = 124;
		this.gc_ozel_islem_yapilacak_alan2.Caption = "İşlem yapılacak alan 2";
		this.gc_ozel_islem_yapilacak_alan2.ColumnEdit = this.repositoryItemGridLookUpEdit_degistirilecek_alan;
		this.gc_ozel_islem_yapilacak_alan2.FieldName = "gc_ozel_islem_yapilacak_alan2";
		this.gc_ozel_islem_yapilacak_alan2.Name = "gc_ozel_islem_yapilacak_alan2";
		this.gc_ozel_islem_yapilacak_alan2.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_yapilacak_alan2.Visible = true;
		this.gc_ozel_islem_yapilacak_alan2.Width = 124;
		this.gc_ozel_islem_yapilacak_alan3.Caption = "İşlem yapılacak alan 3";
		this.gc_ozel_islem_yapilacak_alan3.ColumnEdit = this.repositoryItemGridLookUpEdit_degistirilecek_alan;
		this.gc_ozel_islem_yapilacak_alan3.FieldName = "gc_ozel_islem_yapilacak_alan3";
		this.gc_ozel_islem_yapilacak_alan3.Name = "gc_ozel_islem_yapilacak_alan3";
		this.gc_ozel_islem_yapilacak_alan3.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_yapilacak_alan3.Visible = true;
		this.gc_ozel_islem_yapilacak_alan3.Width = 120;
		this.gc_ozel_islem_yapilacak_alan4.Caption = "İşlem yapılacak alan 4";
		this.gc_ozel_islem_yapilacak_alan4.ColumnEdit = this.repositoryItemGridLookUpEdit_degistirilecek_alan;
		this.gc_ozel_islem_yapilacak_alan4.FieldName = "gc_ozel_islem_yapilacak_alan4";
		this.gc_ozel_islem_yapilacak_alan4.Name = "gc_ozel_islem_yapilacak_alan4";
		this.gc_ozel_islem_yapilacak_alan4.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_yapilacak_alan4.Visible = true;
		this.gc_ozel_islem_yapilacak_alan4.Width = 116;
		this.gc_ozel_islem_yapilacak_alan5.Caption = "İşlem yapılacak alan 5";
		this.gc_ozel_islem_yapilacak_alan5.ColumnEdit = this.repositoryItemGridLookUpEdit_degistirilecek_alan;
		this.gc_ozel_islem_yapilacak_alan5.FieldName = "gc_ozel_islem_yapilacak_alan5";
		this.gc_ozel_islem_yapilacak_alan5.Name = "gc_ozel_islem_yapilacak_alan5";
		this.gc_ozel_islem_yapilacak_alan5.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_yapilacak_alan5.Visible = true;
		this.gc_ozel_islem_yapilacak_alan5.Width = 124;
		this.gc_ozel_islem_parametre1_deger_tipi.Caption = "Parametre 1 değer tipi";
		this.gc_ozel_islem_parametre1_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre1_deger_tipi.FieldName = "gc_ozel_islem_parametre1_deger_tipi";
		this.gc_ozel_islem_parametre1_deger_tipi.Name = "gc_ozel_islem_parametre1_deger_tipi";
		this.gc_ozel_islem_parametre1_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre1_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre1_deger_tipi.Width = 148;
		this.gc_ozel_islem_parametre1_ozel_deger.Caption = "Parametre 1 özel değer";
		this.gc_ozel_islem_parametre1_ozel_deger.FieldName = "ozel_islem_parametre1_ozel_deger";
		this.gc_ozel_islem_parametre1_ozel_deger.Name = "gc_ozel_islem_parametre1_ozel_deger";
		this.gc_ozel_islem_parametre1_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre1_ozel_deger.Width = 169;
		this.gc_ozel_islem_parametre2_deger_tipi.Caption = "Parametre 2 değer tipi";
		this.gc_ozel_islem_parametre2_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre2_deger_tipi.FieldName = "gc_ozel_islem_parametre2_deger_tipi";
		this.gc_ozel_islem_parametre2_deger_tipi.Name = "gc_ozel_islem_parametre2_deger_tipi";
		this.gc_ozel_islem_parametre2_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre2_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre2_deger_tipi.Width = 123;
		this.gc_ozel_islem_parametre2_ozel_deger.Caption = "Parametre 2 özel değer";
		this.gc_ozel_islem_parametre2_ozel_deger.FieldName = "ozel_islem_parametre2_ozel_deger";
		this.gc_ozel_islem_parametre2_ozel_deger.Name = "gc_ozel_islem_parametre2_ozel_deger";
		this.gc_ozel_islem_parametre2_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre2_ozel_deger.Width = 123;
		this.gc_ozel_islem_parametre3_deger_tipi.Caption = "Parametre 3 değer tipi";
		this.gc_ozel_islem_parametre3_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre3_deger_tipi.FieldName = "gc_ozel_islem_parametre3_deger_tipi";
		this.gc_ozel_islem_parametre3_deger_tipi.Name = "gc_ozel_islem_parametre3_deger_tipi";
		this.gc_ozel_islem_parametre3_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre3_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre3_deger_tipi.Width = 123;
		this.gc_ozel_islem_parametre3_ozel_deger.Caption = "Parametre 3 özel değer";
		this.gc_ozel_islem_parametre3_ozel_deger.FieldName = "ozel_islem_parametre3_ozel_deger";
		this.gc_ozel_islem_parametre3_ozel_deger.Name = "gc_ozel_islem_parametre3_ozel_deger";
		this.gc_ozel_islem_parametre3_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre3_ozel_deger.Width = 123;
		this.gc_ozel_islem_parametre4_deger_tipi.Caption = "Parametre 4 değer tipi";
		this.gc_ozel_islem_parametre4_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre4_deger_tipi.FieldName = "gc_ozel_islem_parametre4_deger_tipi";
		this.gc_ozel_islem_parametre4_deger_tipi.Name = "gc_ozel_islem_parametre4_deger_tipi";
		this.gc_ozel_islem_parametre4_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre4_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre4_deger_tipi.Width = 123;
		this.gc_ozel_islem_parametre4_ozel_deger.Caption = "Parametre 4 özel değer";
		this.gc_ozel_islem_parametre4_ozel_deger.FieldName = "ozel_islem_parametre4_ozel_deger";
		this.gc_ozel_islem_parametre4_ozel_deger.Name = "gc_ozel_islem_parametre4_ozel_deger";
		this.gc_ozel_islem_parametre4_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre4_ozel_deger.Width = 123;
		this.gc_ozel_islem_parametre5_deger_tipi.Caption = "Parametre 5 değer tipi";
		this.gc_ozel_islem_parametre5_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre5_deger_tipi.FieldName = "gc_ozel_islem_parametre5_deger_tipi";
		this.gc_ozel_islem_parametre5_deger_tipi.Name = "gc_ozel_islem_parametre5_deger_tipi";
		this.gc_ozel_islem_parametre5_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre5_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre5_deger_tipi.Width = 123;
		this.gc_ozel_islem_parametre5_ozel_deger.Caption = "Parametre 5 özel değer";
		this.gc_ozel_islem_parametre5_ozel_deger.FieldName = "ozel_islem_parametre5_ozel_deger";
		this.gc_ozel_islem_parametre5_ozel_deger.Name = "gc_ozel_islem_parametre5_ozel_deger";
		this.gc_ozel_islem_parametre5_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre5_ozel_deger.Width = 123;
		this.gc_ozel_islem_parametre6_deger_tipi.Caption = "Parametre 6 değer tipi";
		this.gc_ozel_islem_parametre6_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre6_deger_tipi.FieldName = "gc_ozel_islem_parametre6_deger_tipi";
		this.gc_ozel_islem_parametre6_deger_tipi.Name = "gc_ozel_islem_parametre6_deger_tipi";
		this.gc_ozel_islem_parametre6_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre6_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre6_deger_tipi.Width = 123;
		this.gc_ozel_islem_parametre6_ozel_deger.Caption = "Parametre 6 özel değer";
		this.gc_ozel_islem_parametre6_ozel_deger.FieldName = "ozel_islem_parametre6_ozel_deger";
		this.gc_ozel_islem_parametre6_ozel_deger.Name = "gc_ozel_islem_parametre6_ozel_deger";
		this.gc_ozel_islem_parametre6_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre6_ozel_deger.Width = 123;
		this.gc_ozel_islem_parametre7_deger_tipi.Caption = "Parametre 7 değer tipi";
		this.gc_ozel_islem_parametre7_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre7_deger_tipi.FieldName = "gc_ozel_islem_parametre7_deger_tipi";
		this.gc_ozel_islem_parametre7_deger_tipi.Name = "gc_ozel_islem_parametre7_deger_tipi";
		this.gc_ozel_islem_parametre7_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre7_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre7_deger_tipi.Width = 123;
		this.gc_ozel_islem_parametre7_ozel_deger.Caption = "Parametre 7 özel değer";
		this.gc_ozel_islem_parametre7_ozel_deger.FieldName = "ozel_islem_parametre7_ozel_deger";
		this.gc_ozel_islem_parametre7_ozel_deger.Name = "gc_ozel_islem_parametre7_ozel_deger";
		this.gc_ozel_islem_parametre7_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre7_ozel_deger.Width = 123;
		this.gc_ozel_islem_parametre8_deger_tipi.Caption = "Parametre 8 değer tipi";
		this.gc_ozel_islem_parametre8_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre8_deger_tipi.FieldName = "gc_ozel_islem_parametre8_deger_tipi";
		this.gc_ozel_islem_parametre8_deger_tipi.Name = "gc_ozel_islem_parametre8_deger_tipi";
		this.gc_ozel_islem_parametre8_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre8_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre8_deger_tipi.Width = 123;
		this.gc_ozel_islem_parametre8_ozel_deger.Caption = "Parametre 8 özel değer";
		this.gc_ozel_islem_parametre8_ozel_deger.FieldName = "ozel_islem_parametre8_ozel_deger";
		this.gc_ozel_islem_parametre8_ozel_deger.Name = "gc_ozel_islem_parametre8_ozel_deger";
		this.gc_ozel_islem_parametre8_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre8_ozel_deger.Width = 123;
		this.gc_ozel_islem_parametre9_deger_tipi.Caption = "Parametre 9 değer tipi";
		this.gc_ozel_islem_parametre9_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre9_deger_tipi.FieldName = "gc_ozel_islem_parametre9_deger_tipi";
		this.gc_ozel_islem_parametre9_deger_tipi.Name = "gc_ozel_islem_parametre9_deger_tipi";
		this.gc_ozel_islem_parametre9_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre9_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre9_deger_tipi.Width = 122;
		this.gc_ozel_islem_parametre9_ozel_deger.Caption = "Parametre 9 özel değer";
		this.gc_ozel_islem_parametre9_ozel_deger.FieldName = "ozel_islem_parametre9_ozel_deger";
		this.gc_ozel_islem_parametre9_ozel_deger.Name = "gc_ozel_islem_parametre9_ozel_deger";
		this.gc_ozel_islem_parametre9_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre9_ozel_deger.Width = 122;
		this.gc_ozel_islem_parametre10_deger_tipi.Caption = "Parametre 10 değer tipi";
		this.gc_ozel_islem_parametre10_deger_tipi.ColumnEdit = this.repositoryItemGridLookUpEdit_deger_tipi;
		this.gc_ozel_islem_parametre10_deger_tipi.FieldName = "gc_ozel_islem_parametre10_deger_tipi";
		this.gc_ozel_islem_parametre10_deger_tipi.Name = "gc_ozel_islem_parametre10_deger_tipi";
		this.gc_ozel_islem_parametre10_deger_tipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_ozel_islem_parametre10_deger_tipi.Visible = true;
		this.gc_ozel_islem_parametre10_deger_tipi.Width = 117;
		this.gc_ozel_islem_parametre10_ozel_deger.Caption = "Parametre 10 özel değer";
		this.gc_ozel_islem_parametre10_ozel_deger.FieldName = "ozel_islem_parametre10_ozel_deger";
		this.gc_ozel_islem_parametre10_ozel_deger.Name = "gc_ozel_islem_parametre10_ozel_deger";
		this.gc_ozel_islem_parametre10_ozel_deger.Visible = true;
		this.gc_ozel_islem_parametre10_ozel_deger.Width = 241;
		this.repositoryItemGridLookUpEdit_aranacak_deger.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_aranacak_deger.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_aranacak_deger.Name = "repositoryItemGridLookUpEdit_aranacak_deger";
		this.repositoryItemGridLookUpEdit_aranacak_deger.View = this.gridView9;
		this.gridView9.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView9.Name = "gridView9";
		this.gridView9.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView9.OptionsView.ShowGroupPanel = false;
		this.xtraTabPage3.AutoScrollMargin = new System.Drawing.Size(0, 40);
		this.xtraTabPage3.Controls.Add(this.te_kullanici_adi);
		this.xtraTabPage3.Controls.Add(this.labelControl1);
		this.xtraTabPage3.Controls.Add(this.label4);
		this.xtraTabPage3.Controls.Add(this.gc_aranacak2);
		this.xtraTabPage3.Controls.Add(this.arama_alan2_baglac);
		this.xtraTabPage3.Controls.Add(this.label1);
		this.xtraTabPage3.Controls.Add(this.arama_alan1_alan2_baglac);
		this.xtraTabPage3.Controls.Add(this.gc_aranacak1);
		this.xtraTabPage3.Controls.Add(this.arama_alan1_baglac);
		this.xtraTabPage3.Controls.Add(this.label44);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(859, 496);
		this.xtraTabPage3.Text = "Filitre";
		this.te_kullanici_adi.Enabled = false;
		this.te_kullanici_adi.Location = new System.Drawing.Point(68, 12);
		this.te_kullanici_adi.Name = "te_kullanici_adi";
		this.te_kullanici_adi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(255, 255, 128);
		this.te_kullanici_adi.Properties.Appearance.Options.UseForeColor = true;
		this.te_kullanici_adi.Size = new System.Drawing.Size(205, 20);
		this.te_kullanici_adi.TabIndex = 209;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(9, 15);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(53, 13);
		this.labelControl1.TabIndex = 208;
		this.labelControl1.Text = "Kriter adı :";
		this.label4.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label4.Location = new System.Drawing.Point(380, 221);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(86, 19);
		this.label4.TabIndex = 202;
		this.label4.Text = "BAĞLAÇ";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.gc_aranacak2.Cursor = System.Windows.Forms.Cursors.Default;
		this.gc_aranacak2.Location = new System.Drawing.Point(472, 98);
		this.gc_aranacak2.MainView = this.advBandedGridView1;
		this.gc_aranacak2.Name = "gc_aranacak2";
		this.gc_aranacak2.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[2] { this.repositoryItemGridLookUpEdit_aranacak_alan2, this.repositoryItemGridLookUpEdit_aranacak_operator2 });
		this.gc_aranacak2.Size = new System.Drawing.Size(369, 351);
		this.gc_aranacak2.TabIndex = 201;
		this.gc_aranacak2.UseEmbeddedNavigator = true;
		this.gc_aranacak2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.advBandedGridView1 });
		this.gc_aranacak2.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.gc_aranacak2.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.advBandedGridView1.Appearance.BandPanel.BackColor = System.Drawing.Color.Red;
		this.advBandedGridView1.Appearance.BandPanel.BackColor2 = System.Drawing.Color.FromArgb(255, 128, 128);
		this.advBandedGridView1.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView1.Appearance.BandPanel.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView1.Appearance.BandPanel.Options.UseBackColor = true;
		this.advBandedGridView1.Appearance.BandPanel.Options.UseFont = true;
		this.advBandedGridView1.Appearance.BandPanel.Options.UseForeColor = true;
		this.advBandedGridView1.Appearance.BandPanel.Options.UseTextOptions = true;
		this.advBandedGridView1.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.advBandedGridView1.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView1.Appearance.BandPanelBackground.BackColor2 = System.Drawing.Color.FromArgb(255, 192, 128);
		this.advBandedGridView1.Appearance.BandPanelBackground.Options.UseBackColor = true;
		this.advBandedGridView1.Appearance.Empty.Font = new System.Drawing.Font("Tahoma", 11f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView1.Appearance.Empty.Options.UseFont = true;
		this.advBandedGridView1.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(192, 192, 0);
		this.advBandedGridView1.Appearance.EvenRow.Options.UseBackColor = true;
		this.advBandedGridView1.Appearance.FooterPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView1.Appearance.FooterPanel.Options.UseFont = true;
		this.advBandedGridView1.Appearance.GroupFooter.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView1.Appearance.GroupFooter.Options.UseFont = true;
		this.advBandedGridView1.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[2] { this.gridBand2, this.gridBand3 });
		this.advBandedGridView1.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[3] { this.bandedGridColumn1, this.bandedGridColumn2, this.bandedGridColumn3 });
		this.advBandedGridView1.DetailVerticalIndent = 20;
		this.advBandedGridView1.GridControl = this.gc_aranacak2;
		this.advBandedGridView1.Name = "advBandedGridView1";
		this.advBandedGridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView1.OptionsDetail.AllowExpandEmptyDetails = true;
		this.advBandedGridView1.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView1.OptionsSelection.MultiSelect = true;
		this.advBandedGridView1.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
		this.advBandedGridView1.OptionsView.ShowBands = false;
		this.advBandedGridView1.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView1.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(advBandedGridView_master_CustomUnboundColumnData);
		this.gridBand2.Caption = "Evrak Genel";
		this.gridBand2.Columns.Add(this.bandedGridColumn1);
		this.gridBand2.Columns.Add(this.bandedGridColumn2);
		this.gridBand2.Columns.Add(this.bandedGridColumn3);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.VisibleIndex = 0;
		this.gridBand2.Width = 344;
		this.bandedGridColumn1.Caption = "Alan";
		this.bandedGridColumn1.ColumnEdit = this.repositoryItemGridLookUpEdit_aranacak_alan2;
		this.bandedGridColumn1.FieldName = "gc_alan";
		this.bandedGridColumn1.Name = "bandedGridColumn1";
		this.bandedGridColumn1.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.bandedGridColumn1.Visible = true;
		this.bandedGridColumn1.Width = 93;
		this.repositoryItemGridLookUpEdit_aranacak_alan2.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_aranacak_alan2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_aranacak_alan2.Name = "repositoryItemGridLookUpEdit_aranacak_alan2";
		this.repositoryItemGridLookUpEdit_aranacak_alan2.View = this.gridView3;
		this.gridView3.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView3.Name = "gridView3";
		this.gridView3.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView3.OptionsView.ShowGroupPanel = false;
		this.bandedGridColumn2.Caption = "Operatör";
		this.bandedGridColumn2.ColumnEdit = this.repositoryItemGridLookUpEdit_aranacak_operator2;
		this.bandedGridColumn2.FieldName = "gc_operator";
		this.bandedGridColumn2.Name = "bandedGridColumn2";
		this.bandedGridColumn2.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.bandedGridColumn2.Visible = true;
		this.bandedGridColumn2.Width = 93;
		this.repositoryItemGridLookUpEdit_aranacak_operator2.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_aranacak_operator2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_aranacak_operator2.Name = "repositoryItemGridLookUpEdit_aranacak_operator2";
		this.repositoryItemGridLookUpEdit_aranacak_operator2.View = this.gridView2;
		this.gridView2.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView2.Name = "gridView2";
		this.gridView2.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView2.OptionsView.ShowGroupPanel = false;
		this.bandedGridColumn3.Caption = "Değer";
		this.bandedGridColumn3.FieldName = "aranacak_deger";
		this.bandedGridColumn3.Name = "bandedGridColumn3";
		this.bandedGridColumn3.Visible = true;
		this.bandedGridColumn3.Width = 158;
		this.gridBand3.Caption = "Kriter verileri";
		this.gridBand3.Name = "gridBand3";
		this.gridBand3.Visible = false;
		this.gridBand3.VisibleIndex = -1;
		this.gridBand3.Width = 1326;
		this.arama_alan2_baglac.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.arama_alan2_baglac.FormattingEnabled = true;
		this.arama_alan2_baglac.Location = new System.Drawing.Point(636, 71);
		this.arama_alan2_baglac.Name = "arama_alan2_baglac";
		this.arama_alan2_baglac.Size = new System.Drawing.Size(69, 21);
		this.arama_alan2_baglac.TabIndex = 193;
		this.arama_alan2_baglac.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.arama_alan2_baglac.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label1.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label1.Location = new System.Drawing.Point(502, 49);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(336, 19);
		this.label1.TabIndex = 192;
		this.label1.Text = "FİLİTRE 2";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.arama_alan1_alan2_baglac.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.arama_alan1_alan2_baglac.FormattingEnabled = true;
		this.arama_alan1_alan2_baglac.Location = new System.Drawing.Point(380, 243);
		this.arama_alan1_alan2_baglac.Name = "arama_alan1_alan2_baglac";
		this.arama_alan1_alan2_baglac.Size = new System.Drawing.Size(86, 21);
		this.arama_alan1_alan2_baglac.TabIndex = 191;
		this.arama_alan1_alan2_baglac.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.arama_alan1_alan2_baglac.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.gc_aranacak1.Location = new System.Drawing.Point(10, 98);
		this.gc_aranacak1.MainView = this.advBandedGridView_master;
		this.gc_aranacak1.Name = "gc_aranacak1";
		this.gc_aranacak1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[2] { this.repositoryItemGridLookUpEdit_aranacak_alan1, this.repositoryItemGridLookUpEdit_aranacak_operator1 });
		this.gc_aranacak1.Size = new System.Drawing.Size(364, 351);
		this.gc_aranacak1.TabIndex = 190;
		this.gc_aranacak1.UseEmbeddedNavigator = true;
		this.gc_aranacak1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.advBandedGridView_master });
		this.gc_aranacak1.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.gc_aranacak1.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.advBandedGridView_master.Appearance.BandPanel.BackColor = System.Drawing.Color.Red;
		this.advBandedGridView_master.Appearance.BandPanel.BackColor2 = System.Drawing.Color.FromArgb(255, 128, 128);
		this.advBandedGridView_master.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_master.Appearance.BandPanel.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseBackColor = true;
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseFont = true;
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseForeColor = true;
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseTextOptions = true;
		this.advBandedGridView_master.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.advBandedGridView_master.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView_master.Appearance.BandPanelBackground.BackColor2 = System.Drawing.Color.FromArgb(255, 192, 128);
		this.advBandedGridView_master.Appearance.BandPanelBackground.Options.UseBackColor = true;
		this.advBandedGridView_master.Appearance.Empty.Font = new System.Drawing.Font("Tahoma", 11f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_master.Appearance.Empty.Options.UseFont = true;
		this.advBandedGridView_master.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(192, 192, 0);
		this.advBandedGridView_master.Appearance.EvenRow.Options.UseBackColor = true;
		this.advBandedGridView_master.Appearance.FooterPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_master.Appearance.FooterPanel.Options.UseFont = true;
		this.advBandedGridView_master.Appearance.GroupFooter.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_master.Appearance.GroupFooter.Options.UseFont = true;
		this.advBandedGridView_master.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[2] { this.gridBand10, this.gridBand1 });
		this.advBandedGridView_master.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[3] { this.gc_alan, this.gc_operator, this.gc_deger });
		this.advBandedGridView_master.DetailVerticalIndent = 20;
		this.advBandedGridView_master.GridControl = this.gc_aranacak1;
		this.advBandedGridView_master.Name = "advBandedGridView_master";
		this.advBandedGridView_master.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsDetail.AllowExpandEmptyDetails = true;
		this.advBandedGridView_master.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView_master.OptionsSelection.MultiSelect = true;
		this.advBandedGridView_master.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
		this.advBandedGridView_master.OptionsView.ShowBands = false;
		this.advBandedGridView_master.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView_master.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(advBandedGridView_master_CustomUnboundColumnData);
		this.gridBand10.Caption = "Evrak Genel";
		this.gridBand10.Columns.Add(this.gc_alan);
		this.gridBand10.Columns.Add(this.gc_operator);
		this.gridBand10.Columns.Add(this.gc_deger);
		this.gridBand10.Name = "gridBand10";
		this.gridBand10.VisibleIndex = 0;
		this.gridBand10.Width = 339;
		this.gc_alan.Caption = "Alan";
		this.gc_alan.ColumnEdit = this.repositoryItemGridLookUpEdit_aranacak_alan1;
		this.gc_alan.FieldName = "gc_alan";
		this.gc_alan.Name = "gc_alan";
		this.gc_alan.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_alan.Visible = true;
		this.gc_alan.Width = 92;
		this.repositoryItemGridLookUpEdit_aranacak_alan1.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_aranacak_alan1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_aranacak_alan1.Name = "repositoryItemGridLookUpEdit_aranacak_alan1";
		this.repositoryItemGridLookUpEdit_aranacak_alan1.View = this.repositoryItemGridLookUpEdit1View;
		this.repositoryItemGridLookUpEdit1View.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.repositoryItemGridLookUpEdit1View.Name = "repositoryItemGridLookUpEdit1View";
		this.repositoryItemGridLookUpEdit1View.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.repositoryItemGridLookUpEdit1View.OptionsView.ShowGroupPanel = false;
		this.gc_operator.Caption = "Operatör";
		this.gc_operator.ColumnEdit = this.repositoryItemGridLookUpEdit_aranacak_operator1;
		this.gc_operator.FieldName = "gc_operator";
		this.gc_operator.Name = "gc_operator";
		this.gc_operator.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_operator.Visible = true;
		this.gc_operator.Width = 92;
		this.repositoryItemGridLookUpEdit_aranacak_operator1.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_aranacak_operator1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_aranacak_operator1.Name = "repositoryItemGridLookUpEdit_aranacak_operator1";
		this.repositoryItemGridLookUpEdit_aranacak_operator1.View = this.gridView1;
		this.gridView1.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView1.Name = "gridView1";
		this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView1.OptionsView.ShowGroupPanel = false;
		this.gc_deger.Caption = "Değer";
		this.gc_deger.FieldName = "aranacak_deger";
		this.gc_deger.Name = "gc_deger";
		this.gc_deger.Visible = true;
		this.gc_deger.Width = 155;
		this.gridBand1.Caption = "Kriter verileri";
		this.gridBand1.Name = "gridBand1";
		this.gridBand1.Visible = false;
		this.gridBand1.VisibleIndex = -1;
		this.gridBand1.Width = 1326;
		this.arama_alan1_baglac.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.arama_alan1_baglac.FormattingEnabled = true;
		this.arama_alan1_baglac.Location = new System.Drawing.Point(144, 71);
		this.arama_alan1_baglac.Name = "arama_alan1_baglac";
		this.arama_alan1_baglac.Size = new System.Drawing.Size(69, 21);
		this.arama_alan1_baglac.TabIndex = 173;
		this.arama_alan1_baglac.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.arama_alan1_baglac.MouseClick += new System.Windows.Forms.MouseEventHandler(ParametreCheckEditMouseClick);
		this.label44.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label44.Location = new System.Drawing.Point(10, 49);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(336, 19);
		this.label44.TabIndex = 150;
		this.label44.Text = "FİLİTRE 1";
		this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.xtraTabPage1.Controls.Add(this.richTextBox1);
		this.xtraTabPage1.Name = "xtraTabPage1";
		this.xtraTabPage1.Size = new System.Drawing.Size(859, 496);
		this.xtraTabPage1.Text = "Özel işlem açıklamaları";
		this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.richTextBox1.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.richTextBox1.Location = new System.Drawing.Point(0, 0);
		this.richTextBox1.Name = "richTextBox1";
		this.richTextBox1.ReadOnly = true;
		this.richTextBox1.Size = new System.Drawing.Size(859, 496);
		this.richTextBox1.TabIndex = 0;
		this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
		this.labelControl17.Appearance.Font = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(13, 35);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(153, 19);
		this.labelControl17.TabIndex = 89;
		this.labelControl17.Text = "Kriterler";
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.dosyaToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(1040, 24);
		this.menuStrip1.TabIndex = 90;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.degisiklikleriKaydetToolStripMenuItem, this.kriterEkleToolStripMenuItem, this.kriterSilToolStripMenuItem, this.toolStripSeparator1, this.dosyayaYazToolStripMenuItem, this.dosyadanOkuToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.degisiklikleriKaydetToolStripMenuItem.Name = "degisiklikleriKaydetToolStripMenuItem";
		this.degisiklikleriKaydetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.degisiklikleriKaydetToolStripMenuItem.Text = "Kaydet";
		this.degisiklikleriKaydetToolStripMenuItem.Click += new System.EventHandler(degisiklikleriKaydetToolStripMenuItem_Click);
		this.kriterSilToolStripMenuItem.Name = "kriterSilToolStripMenuItem";
		this.kriterSilToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.kriterSilToolStripMenuItem.Text = "Sil";
		this.kriterSilToolStripMenuItem.Click += new System.EventHandler(kriterSilToolStripMenuItem_Click);
		this.kriterEkleToolStripMenuItem.Name = "kriterEkleToolStripMenuItem";
		this.kriterEkleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.kriterEkleToolStripMenuItem.Text = "Ekle";
		this.kriterEkleToolStripMenuItem.Click += new System.EventHandler(kriterEkleToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
		this.dosyayaYazToolStripMenuItem.Name = "dosyayaYazToolStripMenuItem";
		this.dosyayaYazToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.dosyayaYazToolStripMenuItem.Text = "Dosyaya yaz";
		this.dosyayaYazToolStripMenuItem.Click += new System.EventHandler(dosyayaYazToolStripMenuItem_Click);
		this.dosyadanOkuToolStripMenuItem.Name = "dosyadanOkuToolStripMenuItem";
		this.dosyadanOkuToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.dosyadanOkuToolStripMenuItem.Text = "Dosyadan oku";
		this.dosyadanOkuToolStripMenuItem.Click += new System.EventHandler(dosyadanOkuToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1040, 562);
		base.Controls.Add(this.labelControl17);
		base.Controls.Add(this.tc_parametreler);
		base.Controls.Add(this.lb_kriterler);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "KriterDuzenleme";
		this.Text = "Kriter Düzenleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.lb_kriterler).EndInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).EndInit();
		this.tc_parametreler.ResumeLayout(false);
		this.xtraTabPage_degistirilecek_alanlar.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.gc_degistirilecek_alanlar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_yapilacak_islem).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView10).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_alan).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView7).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_operator).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView8).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_degistirilecek_alan).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_islem_tipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_deger_tipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_ozel_islem_tipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView11).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_deger).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView9).EndInit();
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.te_kullanici_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gc_aranacak2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_alan2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_operator2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gc_aranacak1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_alan1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_aranacak_operator1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.xtraTabPage1.ResumeLayout(false);
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
