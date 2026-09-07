using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Xsl;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Card;
using DevExpress.XtraGrid.Views.Grid;
using Fora.App.Mikro.TopluStokEkleme;
using Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Import;
using Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;
using Fora.Mikro;
using Fora.Mikro.Barkodlar;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Depolar;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Firmalar;
using Fora.Mikro.Hizmetler;
using Fora.Mikro.Kurlar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Projeler;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Subeler;
using Fora.Mikro.Utility;
using fastJSON;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli;

[Guid("8497A9D8-C87A-4568-96CB-95E9EDC3EA79")]
public class SatirBazliGenelEvrakGirisi : XtraForm
{
	private GenelEvrakSatirBaslik _satirlar;

	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private List<GenelEvrakKriter> _uygulanacak_kriterler;

	private DataSet _lookuptablolar;

	private string _tag;

	private Color RenkBeyaz = Color.White;

	private Color RenkYesil = Color.LightGreen;

	private Color RenkSari = Color.Yellow;

	private Color RenkKirmizi = Color.Salmon;

	private SqlDB Db;

	private GridView _tempview;

	private RepositoryItemGridLookUpEdit repositoryItem_Cari;

	private GridView gridView_Arama_Cari;

	private MemoryStream arama_cari_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Stok;

	private GridView gridView_Arama_Stok;

	private MemoryStream arama_stok_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Hizmet;

	private GridView gridView_Arama_Hizmet;

	private MemoryStream arama_hizmet_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Kasa;

	private GridView gridView_Arama_Kasa;

	private MemoryStream arama_kasa_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Banka;

	private GridView gridView_Arama_Banka;

	private MemoryStream arama_banka_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_CariPersonel;

	private GridView gridView_Arama_CariPersonel;

	private MemoryStream arama_caripersonel_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_OdemePlani;

	private GridView gridView_Arama_OdemePlani;

	private MemoryStream arama_odemeplani_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Depo;

	private GridView gridView_Arama_Depo;

	private MemoryStream arama_depo_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Hedef_Depo;

	private GridView gridView_Arama_Hedef_Depo;

	private MemoryStream arama_hedef_depo_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Proje;

	private GridView gridView_Arama_Proje;

	private MemoryStream arama_proje_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_SorumlulukMerkezi;

	private GridView gridView_Arama_SorumlulukMerkezi;

	private MemoryStream arama_sorumlulukmerkezi_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_FiyatListesi;

	private GridView gridView_Arama_FiyatListesi;

	private MemoryStream arama_fiyatlisteleri_temp_defaultlayoutStream;

	private MemoryStream evraklar_defaultlayoutStream;

	private int AktarilacakEvrakSayisi;

	private bool KontrolSonucu = true;

	private string islemismi = "";

	private List<Evrak> _AktarilacakEvraklar;

	private IContainer components;

	private SimpleButton sb_Secili_Evraklari_aktar;

	private MenuStrip menuStrip1;

	private BackgroundWorker backgroundWorker_Kontrol;

	private System.Windows.Forms.ProgressBar progressBar1;

	private Label label_islem_adi;

	private Label label_islem_bilgi;

	private BackgroundWorker backgroundWorker_Aktarim;

	private GridControl gridControl_evraklar;

	private CardView cardView1;

	private AdvBandedGridView advBandedGridView_master;

	private BandedGridColumn gc_evrak_Aktar;

	private BandedGridColumn gc_evrak_AktarimDurumu;

	private BandedGridColumn gc_evrak_EvrakTipi;

	private BandedGridColumn gc_evrak_NormalIade;

	private BandedGridColumn gc_evrak_EvrakSeri;

	private BandedGridColumn gc_evrak_EvrakSira;

	private BandedGridColumn gc_evrak_Tarih;

	private BandedGridColumn gc_evrak_BelgeNo;

	private BandedGridColumn gc_evrak_BelgeTarihi;

	private BandedGridColumn gc_evrak_CariKodu;

	private BandedGridColumn gc_evrak_CariAdi;

	private BandedGridColumn gc_evrak_SevkAdresi;

	private BandedGridColumn gc_evrak_DovizCinsi;

	private BandedGridColumn gc_evrak_Kur;

	private BandedGridColumn gc_evrak_OdemePlani;

	private BandedGridColumn gc_evrak_DepoNo;

	private BandedGridColumn gc_evrak_Plasiyer;

	private BandedGridColumn gc_evrak_ProjeKodu;

	private BandedGridColumn gc_evrak_SorMerKodu;

	private BandedGridColumn gc_evrak_AcikKapali;

	private BandedGridColumn gc_evrak_KapamaHesapKodu;

	private BandedGridColumn gc_evrak_special1;

	private BandedGridColumn gc_evrak_special2;

	private BandedGridColumn gc_evrak_special3;

	private BandedGridColumn gc_evrak_FaturaAciklama;

	private BandedGridColumn gc_evrak_Aciklama1;

	private BandedGridColumn gc_evrak_Aciklama2;

	private BandedGridColumn gc_evrak_Aciklama3;

	private BandedGridColumn gc_evrak_Aciklama4;

	private BandedGridColumn gc_evrak_Aciklama5;

	private BandedGridColumn gc_evrak_Aciklama6;

	private BandedGridColumn gc_evrak_Aciklama7;

	private BandedGridColumn gc_evrak_Aciklama8;

	private BandedGridColumn gc_evrak_Aciklama9;

	private BandedGridColumn gc_evrak_Aciklama10;

	private BandedGridColumn gc_evrak_ara_toplam;

	private BandedGridColumn gc_evrak_iskonto_toplam;

	private BandedGridColumn gc_evrak_masraf_toplam;

	private BandedGridColumn gc_evrak_kdv_toplam;

	private BandedGridColumn gc_evrak_yekun;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Satir_VergiPntr;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Satir_IskontoSekli;

	private GridView gridView1;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_Satir_Cinsi;

	private GridView gridView_SatirCinsi;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_Evrak_Tipi;

	private GridView gridView_EvrakTipi;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_NormalIade;

	private GridView gridView_NormalIade;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem acToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripMenuItem kaydetToolStripMenuItem;

	private ToolStripMenuItem farklıKaydetToolStripMenuItem;

	private ToolStripMenuItem kapatToolStripMenuItem;

	private OpenFileDialog openFileDialog1;

	private SaveFileDialog saveFileDialog1;

	private ToolStripMenuItem yeniToolStripMenuItem;

	private ToolStripMenuItem gorunumToolStripMenuItem;

	private ToolStripMenuItem evraklarToolStripMenuItem;

	private ToolStripMenuItem görünümüSaklaToolStripMenuItem1;

	private ToolStripMenuItem evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1;

	private ToolStripMenuItem görünümüYükleToolStripMenuItem;

	private ToolStripMenuItem evraklar_otomatikDosyadan_yukle_ToolStripMenuItem;

	private ToolStripMenuItem evraklar_varsayılanaGeriDonToolStripMenuItem;

	private ToolStripMenuItem evraklar_otomatikDosyayiSilToolStripMenuItem;

	private ToolStripMenuItem evraklar_kolonlaraGoreGruplamaToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem evraklar_kolonSeciciyiGosterToolStripMenuItem;

	private ToolStripMenuItem evraklar_butunGruplariAcToolStripMenuItem;

	private ToolStripMenuItem evraklar_butunGruplariKapatToolStripMenuItem;

	private ToolStripMenuItem evraklar_farkliDosyaya_kaydet_ToolStripMenuItem;

	private ToolStripMenuItem evraklar_farkliDosyadan_yukle_ToolStripMenuItem;

	private RepositoryItemDateEdit repositoryItemDateEdit_EvrakTarih;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Evrak_DovizCinsi;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_AcikKapali;

	private GridView repositoryItemGridLookUpEdit1View;

	private RepositoryItemTextEdit repositoryItemTextEdit_Evrak_Kur;

	private ToolStripMenuItem aramaTablolariToolStripMenuItem;

	private BandedGridColumn gc_evrak_TicaretTuru;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_TicaretTuru;

	private GridView gridView_TicaretTuru;

	private BandedGridColumn gc_evrak_FiyatListesi;

	private BandedGridColumn gc_evrak_SevkTeslimTarihi;

	private ToolStripMenuItem parametrelerToolStripMenuItem;

	private ToolStripMenuItem varsayilanDegerlerToolStripMenuItem;

	private ToolStripMenuItem akisParametreleriToolStripMenuItem;

	private RepositoryItemTextEdit repositoryItemTextEdit_FaturaAciklama;

	private RepositoryItemTextEdit repositoryItemTextEdit_Aciklamalar;

	private BandedGridColumn gc_evrak_DepoAdi;

	private BandedGridColumn gc_evrak_ProjeAdi;

	private BandedGridColumn gc_evrak_SorMerAdi;

	private BandedGridColumn gc_evrak_KayitID;

	private ToolStripSeparator toolStripSeparator2;

	private SimpleButton sb_TxtCsv_Al;

	private BandedGridColumn gc_firma_no;

	private BandedGridColumn gc_sube_no;

	private BandedGridColumn gc_evrak_miktar;

	private BandedGridColumn gc_evrak_miktar2;

	private BandedGridColumn gc_evrak_vergi_pntr;

	private BandedGridColumn gc_evrak_birim_fiyat;

	private BandedGridColumn gc_evrak_otv_sekli;

	private BandedGridColumn gc_evrak_otv_yuzde;

	private BandedGridColumn gc_satir_cinsi;

	private BandedGridColumn gc_satir_HesapKodu;

	private BandedGridColumn gc_satir_HesapAdi;

	private BandedGridColumn gc_satir_iskonto1_uygulama;

	private BandedGridColumn gc_satir_iskonto2_uygulama;

	private BandedGridColumn gc_satir_iskonto3_uygulama;

	private BandedGridColumn gc_satir_iskonto4_uygulama;

	private BandedGridColumn gc_satir_iskonto5_uygulama;

	private BandedGridColumn gc_satir_iskonto6_uygulama;

	private BandedGridColumn gc_satir_iskonto1_yuzde_veya_miktar;

	private BandedGridColumn gc_satir_iskonto2_yuzde_veya_miktar;

	private BandedGridColumn gc_satir_iskonto3_yuzde_veya_miktar;

	private BandedGridColumn gc_satir_iskonto4_yuzde_veya_miktar;

	private BandedGridColumn gc_satir_iskonto5_yuzde_veya_miktar;

	private BandedGridColumn gc_satir_iskonto6_yuzde_veya_miktar;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Satir_OtvSekli;

	private BandedGridColumn gc_evrak_otv_toplam;

	private ToolStripSeparator toolStripSeparator1;

	private BandedGridColumn gc_satir_fiyat_farki_mi;

	private BandedGridColumn gc_evrak_otv_vergi_pntr;

	private BandedGridColumn gc_satir_kriter_string1;

	private BandedGridColumn gc_satir_kriter_string2;

	private BandedGridColumn gc_satir_kriter_string3;

	private BandedGridColumn gc_satir_kriter_string4;

	private BandedGridColumn gc_satir_kriter_string5;

	private BandedGridColumn gc_satir_kriter_double1;

	private BandedGridColumn gc_satir_kriter_double2;

	private BandedGridColumn gc_satir_kriter_double3;

	private BandedGridColumn gc_satir_kriter_double4;

	private BandedGridColumn gc_satir_kriter_double5;

	private BandedGridColumn gc_satir_kriter_bool1;

	private BandedGridColumn gc_satir_kriter_bool2;

	private BandedGridColumn gc_satir_kriter_bool3;

	private BandedGridColumn gc_satir_kriter_bool4;

	private BandedGridColumn gc_satir_kriter_bool5;

	private ToolStripMenuItem kriterToolStripMenuItem;

	private ToolStripMenuItem kriterleriDuzenleToolStripMenuItem;

	private ToolStripMenuItem aktarımParametreleriToolStripMenuItem;

	private ToolStripMenuItem txtCsvAktarimParametreleriToolStripMenuItem;

	private SimpleButton sb_Sql_Al;

	private ToolStripMenuItem sqlAktarimParametreleriToolStripMenuItem;

	private ToolStripMenuItem islemlerToolStripMenuItem;

	private ToolStripMenuItem otomatikEvrakSiraNoVerToolStripMenuItem;

	private ToolStripMenuItem otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem;

	private BandedGridColumn gc_satir_dbc_no;

	private BandedGridColumn gc_evrak_Hedef_DepoNo;

	private BandedGridColumn gc_evrak_Hedef_DepoAdi;

	private BandedGridColumn gc_satir_parti_kodu;

	private BandedGridColumn gc_satir_lot_no;

	private GridBand gridBand15;

	private GridBand gridBand9;

	private GridBand gridBand10;

	private GridBand gridBand2;

	private GridBand gridBand3;

	private GridBand gridBand1;

	private GridBand gridBand17;

	private CheckBox cb_belgeno_kontrolu_yap;

	public SatirBazliGenelEvrakGirisi(MikroUygulamaBilgileri mikrouygulamabilgileri, string tag, string baslik)
	{
		InitializeComponent();
		_satirlar = new GenelEvrakSatirBaslik();
		_satirlar._satirlar.AddingNew += _satirlar_AddingNew;
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_lookuptablolar = GenelData.GetLookupTablolar(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		_tag = tag;
		Text = baslik;
		if (!Directory.Exists("data"))
		{
			Directory.CreateDirectory("data");
		}
		if (!Directory.Exists("data\\views"))
		{
			Directory.CreateDirectory("data\\views");
		}
		Db = new SqlDB();
		Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		GenelParametreTanimlaveOku();
	}

	private void _satirlar_AddingNew(object sender, AddingNewEventArgs e)
	{
		BindingList<GenelEvrakSatir> obj = (BindingList<GenelEvrakSatir>)sender;
		e.NewObject = new GenelEvrakSatir();
		GenelEvrakSatir genelEvrakSatir = (GenelEvrakSatir)e.NewObject;
		int num = 0;
		foreach (GenelEvrakSatir item in obj)
		{
			if (item.KayitID >= num)
			{
				num = item.KayitID + 1;
			}
		}
		genelEvrakSatir.KayitID = num;
		genelEvrakSatir.evraknoseri = "AA";
	}

	private void EvrakDuzenleme_Load(object sender, EventArgs e)
	{
		advBandedGridView_master.Appearance.SelectedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		evraklar_defaultlayoutStream = new MemoryStream();
		advBandedGridView_master.SaveLayoutToStream(evraklar_defaultlayoutStream);
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			advBandedGridView_master.RestoreLayoutFromXml("data\\views\\" + _tag + ".lgm");
		}
		RepositoryItemAyarla();
		bindData();
	}

	private void bindData()
	{
		gridControl_evraklar.DataSource = _satirlar._satirlar;
	}

	private void GenelParametreTanimlaveOku()
	{
	}

	private void TopluGenelEvrakGirisi_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.DialogResult != DialogResult.OK && MessageBox.Show("Çıkmak istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
		{
			e.Cancel = true;
		}
	}

	private void SatirBazliGenelEvrakGirisi_FormClosed(object sender, FormClosedEventArgs e)
	{
		try
		{
			if (Db.Connection.State == ConnectionState.Open)
			{
				Db.ConnectionClose();
			}
		}
		catch
		{
		}
	}

	private void RepositoryItemAyarla()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(int));
		dataTable.Columns.Add("Isim", typeof(string));
		dataTable.Rows.Add(1, "Satış Faturası");
		dataTable.Rows.Add(2, "Satış İrsaliyesi");
		dataTable.Rows.Add(3, "Alış Faturası");
		dataTable.Rows.Add(4, "Alış İrsaliyesi");
		dataTable.Rows.Add(5, "Alınan Sipariş");
		dataTable.Rows.Add(8, "Depolar arası sevk");
		repositoryItemGridLookUpEdit_Evrak_Tipi.DataSource = dataTable;
		repositoryItemGridLookUpEdit_Evrak_Tipi.ValueMember = "ID";
		repositoryItemGridLookUpEdit_Evrak_Tipi.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_Evrak_Tipi.PopulateViewColumns();
		foreach (GridColumn column in repositoryItemGridLookUpEdit_Evrak_Tipi.View.Columns)
		{
			column.Visible = false;
		}
		repositoryItemGridLookUpEdit_Evrak_Tipi.View.Columns[1].Caption = "Evrak Tipi";
		repositoryItemGridLookUpEdit_Evrak_Tipi.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_Evrak_Tipi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_Evrak_Tipi.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_Evrak_Tipi.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_Evrak_Tipi.AutoComplete = false;
		DataTable dataTable2 = new DataTable();
		dataTable2.Columns.Add("ID", typeof(int));
		dataTable2.Columns.Add("Isim", typeof(string));
		dataTable2.Rows.Add(0, "Toptan Yurt İçi Ticaret");
		dataTable2.Rows.Add(1, "Perakende Yurt İçi Ticaret");
		dataTable2.Rows.Add(2, "İhraç kayıtlı Yurt İçi Ticaret");
		dataTable2.Rows.Add(3, "Yurt Dışı Ticaret");
		dataTable2.Rows.Add(4, "Yurt Dışı Nitelikli İhraç Kayıtlı Ticaret");
		dataTable2.Rows.Add(5, "Yurt Dışı Nitelikli Yurt İçi Ticaret");
		repositoryItemGridLookUpEdit_TicaretTuru.DataSource = dataTable2;
		repositoryItemGridLookUpEdit_TicaretTuru.ValueMember = "ID";
		repositoryItemGridLookUpEdit_TicaretTuru.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_TicaretTuru.PopulateViewColumns();
		foreach (GridColumn column2 in repositoryItemGridLookUpEdit_TicaretTuru.View.Columns)
		{
			column2.Visible = false;
		}
		repositoryItemGridLookUpEdit_TicaretTuru.View.Columns[1].Caption = "Ticaret türleri";
		repositoryItemGridLookUpEdit_TicaretTuru.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_TicaretTuru.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_TicaretTuru.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_TicaretTuru.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_TicaretTuru.AutoComplete = false;
		DataTable dataTable3 = new DataTable();
		dataTable3.Columns.Add("ID", typeof(int));
		dataTable3.Columns.Add("Isim", typeof(string));
		dataTable3.Rows.Add(0, "Normal");
		dataTable3.Rows.Add(1, "İade");
		repositoryItemGridLookUpEdit_NormalIade.DataSource = dataTable3;
		repositoryItemGridLookUpEdit_NormalIade.ValueMember = "ID";
		repositoryItemGridLookUpEdit_NormalIade.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_NormalIade.PopulateViewColumns();
		foreach (GridColumn column3 in repositoryItemGridLookUpEdit_NormalIade.View.Columns)
		{
			column3.Visible = false;
		}
		repositoryItemGridLookUpEdit_NormalIade.View.Columns[1].Caption = "Normal/İade";
		repositoryItemGridLookUpEdit_NormalIade.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_NormalIade.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_NormalIade.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_NormalIade.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_NormalIade.AutoComplete = false;
		DataTable dataTable4 = new DataTable();
		dataTable4.Columns.Add("ID", typeof(int));
		dataTable4.Columns.Add("Isim", typeof(string));
		dataTable4.Rows.Add(0, "Açık Hesap");
		dataTable4.Rows.Add(1, "Kasadan Kapanacak");
		dataTable4.Rows.Add(2, "Bankadan Kapanacak");
		dataTable4.Rows.Add(3, "Cari Personelden Kapanacak");
		repositoryItemGridLookUpEdit_AcikKapali.DataSource = dataTable4;
		repositoryItemGridLookUpEdit_AcikKapali.ValueMember = "ID";
		repositoryItemGridLookUpEdit_AcikKapali.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_AcikKapali.PopulateViewColumns();
		foreach (GridColumn column4 in repositoryItemGridLookUpEdit_AcikKapali.View.Columns)
		{
			column4.Visible = false;
		}
		repositoryItemGridLookUpEdit_AcikKapali.View.Columns[1].Caption = "Kapama şekli";
		repositoryItemGridLookUpEdit_AcikKapali.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_AcikKapali.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_AcikKapali.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_AcikKapali.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_AcikKapali.AutoComplete = false;
		DataTable dataTable5 = new DataTable();
		dataTable5.Columns.Add("ID", typeof(int));
		dataTable5.Columns.Add("Isim", typeof(string));
		dataTable5.Rows.Add(0, _mikrouygulamabilgileri.vergitanimlari[0].UzunAdi);
		dataTable5.Rows.Add(1, _mikrouygulamabilgileri.vergitanimlari[1].UzunAdi);
		dataTable5.Rows.Add(2, _mikrouygulamabilgileri.vergitanimlari[2].UzunAdi);
		dataTable5.Rows.Add(3, _mikrouygulamabilgileri.vergitanimlari[3].UzunAdi);
		dataTable5.Rows.Add(4, _mikrouygulamabilgileri.vergitanimlari[4].UzunAdi);
		dataTable5.Rows.Add(5, _mikrouygulamabilgileri.vergitanimlari[5].UzunAdi);
		dataTable5.Rows.Add(6, _mikrouygulamabilgileri.vergitanimlari[6].UzunAdi);
		repositoryItemLookUpEdit_Satir_VergiPntr.DataSource = dataTable5;
		repositoryItemLookUpEdit_Satir_VergiPntr.ValueMember = "ID";
		repositoryItemLookUpEdit_Satir_VergiPntr.DisplayMember = "Isim";
		repositoryItemLookUpEdit_Satir_VergiPntr.Columns.Add(new LookUpColumnInfo("Isim", 100, "K.D.V"));
		repositoryItemLookUpEdit_Satir_VergiPntr.AutoSearchColumnIndex = 1;
		repositoryItemLookUpEdit_Satir_VergiPntr.ValidateOnEnterKey = true;
		repositoryItemLookUpEdit_Satir_VergiPntr.SearchMode = SearchMode.OnlyInPopup;
		repositoryItemLookUpEdit_Satir_VergiPntr.TextEditStyle = TextEditStyles.Standard;
		DataTable dataTable6 = new DataTable();
		dataTable6.Columns.Add("ID", typeof(int));
		dataTable6.Columns.Add("Isim", typeof(string));
		dataTable6.Rows.Add(0, "Yüzde");
		dataTable6.Rows.Add(1, "Tutar");
		repositoryItemLookUpEdit_Satir_OtvSekli.DataSource = dataTable6;
		repositoryItemLookUpEdit_Satir_OtvSekli.ValueMember = "ID";
		repositoryItemLookUpEdit_Satir_OtvSekli.DisplayMember = "Isim";
		repositoryItemLookUpEdit_Satir_OtvSekli.Columns.Add(new LookUpColumnInfo("Isim", 100, "ÖTV Şekli"));
		repositoryItemLookUpEdit_Satir_OtvSekli.AutoSearchColumnIndex = 1;
		repositoryItemLookUpEdit_Satir_OtvSekli.ValidateOnEnterKey = true;
		repositoryItemLookUpEdit_Satir_OtvSekli.SearchMode = SearchMode.OnlyInPopup;
		repositoryItemLookUpEdit_Satir_OtvSekli.TextEditStyle = TextEditStyles.Standard;
		DataTable dataTable7 = new DataTable();
		dataTable7.Columns.Add("ID", typeof(int));
		dataTable7.Columns.Add("Isim", typeof(string));
		dataTable7.Rows.Add(0, "Brüt toplamdan yüzde");
		dataTable7.Rows.Add(1, "Önceki ara toplamdan yüzde");
		dataTable7.Rows.Add(2, "Tutar");
		repositoryItemLookUpEdit_Satir_IskontoSekli.DataSource = dataTable7;
		repositoryItemLookUpEdit_Satir_IskontoSekli.ValueMember = "ID";
		repositoryItemLookUpEdit_Satir_IskontoSekli.DisplayMember = "Isim";
		repositoryItemLookUpEdit_Satir_IskontoSekli.Columns.Add(new LookUpColumnInfo("Isim", 100, "Uygulama Şekli"));
		repositoryItemLookUpEdit_Satir_IskontoSekli.AutoSearchColumnIndex = 1;
		repositoryItemLookUpEdit_Satir_IskontoSekli.ValidateOnEnterKey = true;
		repositoryItemLookUpEdit_Satir_IskontoSekli.SearchMode = SearchMode.OnlyInPopup;
		repositoryItemLookUpEdit_Satir_IskontoSekli.TextEditStyle = TextEditStyles.Standard;
		DataTable dataTable8 = new DataTable();
		dataTable8.Columns.Add("ID", typeof(int));
		dataTable8.Columns.Add("Isim", typeof(string));
		dataTable8.Rows.Add(0, "Stok");
		dataTable8.Rows.Add(1, "Hizmet");
		repositoryItemGridLookUpEdit_Satir_Cinsi.DataSource = dataTable8;
		repositoryItemGridLookUpEdit_Satir_Cinsi.ValueMember = "ID";
		repositoryItemGridLookUpEdit_Satir_Cinsi.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_Satir_Cinsi.PopulateViewColumns();
		foreach (GridColumn column5 in repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns)
		{
			column5.Visible = false;
		}
		repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns[1].Caption = "Cinsi";
		repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_Satir_Cinsi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_Satir_Cinsi.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_Satir_Cinsi.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_Satir_Cinsi.AutoComplete = false;
		RepositoryItem_Cari_Init();
		RepositoryItem_Stok_Init();
		RepositoryItem_Hizmet_Init();
		RepositoryItem_Kasa_Init();
		RepositoryItem_Banka_Init();
		RepositoryItem_CariPersonel_Init();
		RepositoryItem_OdemePlani_Init();
		RepositoryItem_Depo_Init();
		RepositoryItem_Hedef_Depo_Init();
		RepositoryItem_Proje_Init();
		RepositoryItem_SorumlulukMerkezi_Init();
		RepositoryItem_FiyatListesi_Init();
		gc_evrak_CariKodu.ColumnEdit = repositoryItem_Cari;
		gc_evrak_CariAdi.ColumnEdit = repositoryItem_Cari;
		gc_evrak_DepoNo.ColumnEdit = repositoryItem_Depo;
		gc_evrak_DepoAdi.ColumnEdit = repositoryItem_Depo;
		gc_evrak_Hedef_DepoNo.ColumnEdit = repositoryItem_Hedef_Depo;
		gc_evrak_Hedef_DepoAdi.ColumnEdit = repositoryItem_Hedef_Depo;
		gc_evrak_ProjeKodu.ColumnEdit = repositoryItem_Proje;
		gc_evrak_ProjeAdi.ColumnEdit = repositoryItem_Proje;
		gc_evrak_Plasiyer.ColumnEdit = repositoryItem_CariPersonel;
		gc_evrak_SorMerKodu.ColumnEdit = repositoryItem_SorumlulukMerkezi;
		gc_evrak_SorMerAdi.ColumnEdit = repositoryItem_SorumlulukMerkezi;
		gc_evrak_FiyatListesi.ColumnEdit = repositoryItem_FiyatListesi;
	}

	private void RepositoryItem_Cari_Init()
	{
		gridView_Arama_Cari = new GridView();
		gridView_Arama_Cari.BeginInit();
		gridView_Arama_Cari.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Cari.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Cari.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Cari.EndInit();
		repositoryItem_Cari = new RepositoryItemGridLookUpEdit();
		repositoryItem_Cari.BeginInit();
		repositoryItem_Cari.AutoHeight = false;
		repositoryItem_Cari.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Cari.View = gridView_Arama_Cari;
		repositoryItem_Cari.CloseUp += repositoryItem_Cari_CloseUp;
		repositoryItem_Cari.QueryPopUp += repositoryItem_Cari_QueryPopUp;
		repositoryItem_Cari.EndInit();
		repositoryItem_Cari.DataSource = _lookuptablolar.Tables["CARI_HESAPLAR"];
		repositoryItem_Cari.ValueMember = "KOD";
		repositoryItem_Cari.DisplayMember = "KOD";
		repositoryItem_Cari.PopulateViewColumns();
		repositoryItem_Cari.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Cari.ValidateOnEnterKey = true;
		repositoryItem_Cari.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Cari.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += cariler_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += cariler_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Cariler";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_Cari_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_cari_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_cari_temp_defaultlayoutStream);
	}

	private void repositoryItem_Cari_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_cari.lcr"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_cari.lcr");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void cariler_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_cari_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_cari_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_cari.lcr", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_cari_temp_defaultlayoutStream.Length];
		arama_cari_temp_defaultlayoutStream.Read(array, 0, (int)arama_cari_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void cariler_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_cari.lcr"))
		{
			File.Delete("data\\views\\" + _tag + "_cari.lcr");
		}
	}

	private void RepositoryItem_Stok_Init()
	{
		gridView_Arama_Stok = new GridView();
		gridView_Arama_Stok.BeginInit();
		gridView_Arama_Stok.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Stok.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Stok.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Stok.EndInit();
		repositoryItem_Stok = new RepositoryItemGridLookUpEdit();
		repositoryItem_Stok.BeginInit();
		repositoryItem_Stok.AutoHeight = false;
		repositoryItem_Stok.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Stok.View = gridView_Arama_Stok;
		repositoryItem_Stok.CloseUp += repositoryItem_Stok_CloseUp;
		repositoryItem_Stok.QueryPopUp += repositoryItem_Stok_QueryPopUp;
		repositoryItem_Stok.EndInit();
		repositoryItem_Stok.DataSource = _lookuptablolar.Tables["STOKLAR"];
		repositoryItem_Stok.ValueMember = "KOD";
		repositoryItem_Stok.DisplayMember = "KOD";
		repositoryItem_Stok.PopulateViewColumns();
		repositoryItem_Stok.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Stok.ValidateOnEnterKey = true;
		repositoryItem_Stok.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Stok.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += stoklar_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += stoklar_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Stoklar";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_Stok_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_stok_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_stok_temp_defaultlayoutStream);
	}

	private void repositoryItem_Stok_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_stok.lst"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_stok.lst");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void stoklar_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_stok_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_stok_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_stok.lst", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_stok_temp_defaultlayoutStream.Length];
		arama_stok_temp_defaultlayoutStream.Read(array, 0, (int)arama_stok_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void stoklar_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_stok.lst"))
		{
			File.Delete("data\\views\\" + _tag + "_stok.lst");
		}
	}

	private void RepositoryItem_Hizmet_Init()
	{
		gridView_Arama_Hizmet = new GridView();
		gridView_Arama_Hizmet.BeginInit();
		gridView_Arama_Hizmet.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Hizmet.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Hizmet.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Hizmet.EndInit();
		repositoryItem_Hizmet = new RepositoryItemGridLookUpEdit();
		repositoryItem_Hizmet.BeginInit();
		repositoryItem_Hizmet.AutoHeight = false;
		repositoryItem_Hizmet.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Hizmet.View = gridView_Arama_Hizmet;
		repositoryItem_Hizmet.CloseUp += repositoryItem_Hizmet_CloseUp;
		repositoryItem_Hizmet.QueryPopUp += repositoryItem_Hizmet_QueryPopUp;
		repositoryItem_Hizmet.EndInit();
		repositoryItem_Hizmet.DataSource = _lookuptablolar.Tables["HIZMET_HESAPLARI"];
		repositoryItem_Hizmet.ValueMember = "KOD";
		repositoryItem_Hizmet.DisplayMember = "KOD";
		repositoryItem_Hizmet.PopulateViewColumns();
		repositoryItem_Hizmet.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Hizmet.ValidateOnEnterKey = true;
		repositoryItem_Hizmet.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Hizmet.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += hizmetler_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += hizmetler_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Hizmetler";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_Hizmet_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_hizmet_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_hizmet_temp_defaultlayoutStream);
	}

	private void repositoryItem_Hizmet_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_hizmet.lhz"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_hizmet.lhz");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void hizmetler_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_hizmet_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_hizmet_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_hizmet.lhz", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_hizmet_temp_defaultlayoutStream.Length];
		arama_hizmet_temp_defaultlayoutStream.Read(array, 0, (int)arama_hizmet_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void hizmetler_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_hizmet.lhz"))
		{
			File.Delete("data\\views\\" + _tag + "_hizmet.lhz");
		}
	}

	private void RepositoryItem_Kasa_Init()
	{
		gridView_Arama_Kasa = new GridView();
		gridView_Arama_Kasa.BeginInit();
		gridView_Arama_Kasa.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Kasa.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Kasa.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Kasa.EndInit();
		repositoryItem_Kasa = new RepositoryItemGridLookUpEdit();
		repositoryItem_Kasa.BeginInit();
		repositoryItem_Kasa.AutoHeight = false;
		repositoryItem_Kasa.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Kasa.View = gridView_Arama_Kasa;
		repositoryItem_Kasa.CloseUp += repositoryItem_Kasa_CloseUp;
		repositoryItem_Kasa.QueryPopUp += repositoryItem_Kasa_QueryPopUp;
		repositoryItem_Kasa.EndInit();
		repositoryItem_Kasa.DataSource = _lookuptablolar.Tables["KASALAR"];
		repositoryItem_Kasa.ValueMember = "KOD";
		repositoryItem_Kasa.DisplayMember = "İSİM";
		repositoryItem_Kasa.PopulateViewColumns();
		repositoryItem_Kasa.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Kasa.ValidateOnEnterKey = true;
		repositoryItem_Kasa.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Kasa.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += kasalar_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += kasalar_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Kasalar";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_Kasa_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_kasa_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_kasa_temp_defaultlayoutStream);
	}

	private void repositoryItem_Kasa_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_kasa.lks"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_kasa.lks");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void kasalar_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_kasa_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_kasa_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_kasa.lks", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_kasa_temp_defaultlayoutStream.Length];
		arama_kasa_temp_defaultlayoutStream.Read(array, 0, (int)arama_kasa_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void kasalar_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_kasa.lks"))
		{
			File.Delete("data\\views\\" + _tag + "_kasa.lks");
		}
	}

	private void RepositoryItem_Banka_Init()
	{
		gridView_Arama_Banka = new GridView();
		gridView_Arama_Banka.BeginInit();
		gridView_Arama_Banka.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Banka.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Banka.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Banka.EndInit();
		repositoryItem_Banka = new RepositoryItemGridLookUpEdit();
		repositoryItem_Banka.BeginInit();
		repositoryItem_Banka.AutoHeight = false;
		repositoryItem_Banka.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Banka.View = gridView_Arama_Banka;
		repositoryItem_Banka.CloseUp += repositoryItem_Banka_CloseUp;
		repositoryItem_Banka.QueryPopUp += repositoryItem_Banka_QueryPopUp;
		repositoryItem_Banka.EndInit();
		repositoryItem_Banka.DataSource = _lookuptablolar.Tables["BANKALAR"];
		repositoryItem_Banka.ValueMember = "KOD";
		repositoryItem_Banka.DisplayMember = "İSİM";
		repositoryItem_Banka.PopulateViewColumns();
		repositoryItem_Banka.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Banka.ValidateOnEnterKey = true;
		repositoryItem_Banka.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Banka.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += bankalar_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += bankalar_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Bankalar";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_Banka_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_banka_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_banka_temp_defaultlayoutStream);
	}

	private void repositoryItem_Banka_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_banka.lbn"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_banka.lbn");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void bankalar_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_banka_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_banka_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_banka.lbn", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_banka_temp_defaultlayoutStream.Length];
		arama_banka_temp_defaultlayoutStream.Read(array, 0, (int)arama_banka_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void bankalar_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_banka.lbn"))
		{
			File.Delete("data\\views\\" + _tag + "_banka.lbn");
		}
	}

	private void RepositoryItem_CariPersonel_Init()
	{
		gridView_Arama_CariPersonel = new GridView();
		gridView_Arama_CariPersonel.BeginInit();
		gridView_Arama_CariPersonel.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_CariPersonel.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_CariPersonel.OptionsView.ShowGroupPanel = false;
		gridView_Arama_CariPersonel.EndInit();
		repositoryItem_CariPersonel = new RepositoryItemGridLookUpEdit();
		repositoryItem_CariPersonel.BeginInit();
		repositoryItem_CariPersonel.AutoHeight = false;
		repositoryItem_CariPersonel.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_CariPersonel.View = gridView_Arama_CariPersonel;
		repositoryItem_CariPersonel.CloseUp += repositoryItem_CariPersonel_CloseUp;
		repositoryItem_CariPersonel.QueryPopUp += repositoryItem_CariPersonel_QueryPopUp;
		repositoryItem_CariPersonel.EndInit();
		repositoryItem_CariPersonel.DataSource = _lookuptablolar.Tables["CARI_PERSONEL_TANIMLARI"];
		repositoryItem_CariPersonel.ValueMember = "KOD";
		repositoryItem_CariPersonel.DisplayMember = "KOD";
		repositoryItem_CariPersonel.PopulateViewColumns();
		repositoryItem_CariPersonel.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_CariPersonel.ValidateOnEnterKey = true;
		repositoryItem_CariPersonel.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_CariPersonel.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += caripersoneller_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += caripersoneller_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Cari personeller";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_CariPersonel_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_caripersonel_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_caripersonel_temp_defaultlayoutStream);
	}

	private void repositoryItem_CariPersonel_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_caripersonel.lcp"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_caripersonel.lcp");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void caripersoneller_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_caripersonel_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_caripersonel_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_caripersonel.lcp", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_caripersonel_temp_defaultlayoutStream.Length];
		arama_caripersonel_temp_defaultlayoutStream.Read(array, 0, (int)arama_caripersonel_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void caripersoneller_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_caripersonel.lcp"))
		{
			File.Delete("data\\views\\" + _tag + "_caripersonel.lcp");
		}
	}

	private void RepositoryItem_OdemePlani_Init()
	{
		gridView_Arama_OdemePlani = new GridView();
		gridView_Arama_OdemePlani.BeginInit();
		gridView_Arama_OdemePlani.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_OdemePlani.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_OdemePlani.OptionsView.ShowGroupPanel = false;
		gridView_Arama_OdemePlani.EndInit();
		repositoryItem_OdemePlani = new RepositoryItemGridLookUpEdit();
		repositoryItem_OdemePlani.BeginInit();
		repositoryItem_OdemePlani.AutoHeight = false;
		repositoryItem_OdemePlani.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_OdemePlani.View = gridView_Arama_OdemePlani;
		repositoryItem_OdemePlani.CloseUp += repositoryItem_OdemePlani_CloseUp;
		repositoryItem_OdemePlani.QueryPopUp += repositoryItem_OdemePlani_QueryPopUp;
		repositoryItem_OdemePlani.EndInit();
		repositoryItem_OdemePlani.DataSource = _lookuptablolar.Tables["ODEME_PLANLARI"];
		repositoryItem_OdemePlani.ValueMember = "ÖDEME PLAN NO";
		repositoryItem_OdemePlani.DisplayMember = "ÖDEME PLANI KODU";
		repositoryItem_OdemePlani.PopulateViewColumns();
		repositoryItem_OdemePlani.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_OdemePlani.ValidateOnEnterKey = true;
		repositoryItem_OdemePlani.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_OdemePlani.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += odemeplanlari_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += odemeplanlari_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Ödeme planları";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_OdemePlani_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_odemeplani_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_odemeplani_temp_defaultlayoutStream);
	}

	private void repositoryItem_OdemePlani_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_odemeplani.lop"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_odemeplani.lop");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void odemeplanlari_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_odemeplani_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_odemeplani_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_odemeplani.lop", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_odemeplani_temp_defaultlayoutStream.Length];
		arama_odemeplani_temp_defaultlayoutStream.Read(array, 0, (int)arama_odemeplani_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void odemeplanlari_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_odemeplani.lop"))
		{
			File.Delete("data\\views\\" + _tag + "_odemeplani.lop");
		}
	}

	private void RepositoryItem_Depo_Init()
	{
		gridView_Arama_Depo = new GridView();
		gridView_Arama_Depo.BeginInit();
		gridView_Arama_Depo.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Depo.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Depo.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Depo.EndInit();
		repositoryItem_Depo = new RepositoryItemGridLookUpEdit();
		repositoryItem_Depo.BeginInit();
		repositoryItem_Depo.AutoHeight = false;
		repositoryItem_Depo.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Depo.View = gridView_Arama_Depo;
		repositoryItem_Depo.CloseUp += repositoryItem_Depo_CloseUp;
		repositoryItem_Depo.QueryPopUp += repositoryItem_Depo_QueryPopUp;
		repositoryItem_Depo.EndInit();
		repositoryItem_Depo.DataSource = _lookuptablolar.Tables["DEPOLAR"];
		repositoryItem_Depo.ValueMember = "DEPO NO";
		repositoryItem_Depo.DisplayMember = "DEPO ADI";
		repositoryItem_Depo.PopulateViewColumns();
		repositoryItem_Depo.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Depo.ValidateOnEnterKey = true;
		repositoryItem_Depo.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Depo.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += depolar_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += depolar_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Depolar";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_Depo_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_depo_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_depo_temp_defaultlayoutStream);
	}

	private void repositoryItem_Depo_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_depo.ldp"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_depo.ldp");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void depolar_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_depo_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_depo_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_depo.ldp", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_depo_temp_defaultlayoutStream.Length];
		arama_depo_temp_defaultlayoutStream.Read(array, 0, (int)arama_depo_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void depolar_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_depo.ldp"))
		{
			File.Delete("data\\views\\" + _tag + "_depo.ldp");
		}
	}

	private void RepositoryItem_Hedef_Depo_Init()
	{
		gridView_Arama_Hedef_Depo = new GridView();
		gridView_Arama_Hedef_Depo.BeginInit();
		gridView_Arama_Hedef_Depo.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Hedef_Depo.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Hedef_Depo.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Hedef_Depo.EndInit();
		repositoryItem_Hedef_Depo = new RepositoryItemGridLookUpEdit();
		repositoryItem_Hedef_Depo.BeginInit();
		repositoryItem_Hedef_Depo.AutoHeight = false;
		repositoryItem_Hedef_Depo.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Hedef_Depo.View = gridView_Arama_Hedef_Depo;
		repositoryItem_Hedef_Depo.CloseUp += repositoryItem_Hedef_Depo_CloseUp;
		repositoryItem_Hedef_Depo.QueryPopUp += repositoryItem_Hedef_Depo_QueryPopUp;
		repositoryItem_Hedef_Depo.EndInit();
		repositoryItem_Hedef_Depo.DataSource = _lookuptablolar.Tables["DEPOLAR"];
		repositoryItem_Hedef_Depo.ValueMember = "DEPO NO";
		repositoryItem_Hedef_Depo.DisplayMember = "DEPO ADI";
		repositoryItem_Hedef_Depo.PopulateViewColumns();
		repositoryItem_Hedef_Depo.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Hedef_Depo.ValidateOnEnterKey = true;
		repositoryItem_Hedef_Depo.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Hedef_Depo.AutoComplete = false;
	}

	private void repositoryItem_Hedef_Depo_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_hedef_depo_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_hedef_depo_temp_defaultlayoutStream);
	}

	private void repositoryItem_Hedef_Depo_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_depo.ldp"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_depo.ldp");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void RepositoryItem_Proje_Init()
	{
		gridView_Arama_Proje = new GridView();
		gridView_Arama_Proje.BeginInit();
		gridView_Arama_Proje.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_Proje.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_Proje.OptionsView.ShowGroupPanel = false;
		gridView_Arama_Proje.EndInit();
		repositoryItem_Proje = new RepositoryItemGridLookUpEdit();
		repositoryItem_Proje.BeginInit();
		repositoryItem_Proje.AutoHeight = false;
		repositoryItem_Proje.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_Proje.View = gridView_Arama_Proje;
		repositoryItem_Proje.CloseUp += repositoryItem_Proje_CloseUp;
		repositoryItem_Proje.QueryPopUp += repositoryItem_Proje_QueryPopUp;
		repositoryItem_Proje.EndInit();
		repositoryItem_Proje.DataSource = _lookuptablolar.Tables["PROJELER"];
		repositoryItem_Proje.ValueMember = "PROJE KODU";
		repositoryItem_Proje.DisplayMember = "PROJE ADI";
		repositoryItem_Proje.PopulateViewColumns();
		repositoryItem_Proje.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_Proje.ValidateOnEnterKey = true;
		repositoryItem_Proje.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_Proje.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += projeler_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += projeler_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Projeler";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_Proje_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_proje_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_proje_temp_defaultlayoutStream);
	}

	private void repositoryItem_Proje_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_proje.lpr"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_proje.lpr");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void projeler_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_proje_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_proje_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_proje.lpr", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_proje_temp_defaultlayoutStream.Length];
		arama_proje_temp_defaultlayoutStream.Read(array, 0, (int)arama_proje_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void projeler_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_proje.lpr"))
		{
			File.Delete("data\\views\\" + _tag + "_proje.lpr");
		}
	}

	private void RepositoryItem_SorumlulukMerkezi_Init()
	{
		gridView_Arama_SorumlulukMerkezi = new GridView();
		gridView_Arama_SorumlulukMerkezi.BeginInit();
		gridView_Arama_SorumlulukMerkezi.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_SorumlulukMerkezi.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_SorumlulukMerkezi.OptionsView.ShowGroupPanel = false;
		gridView_Arama_SorumlulukMerkezi.EndInit();
		repositoryItem_SorumlulukMerkezi = new RepositoryItemGridLookUpEdit();
		repositoryItem_SorumlulukMerkezi.BeginInit();
		repositoryItem_SorumlulukMerkezi.AutoHeight = false;
		repositoryItem_SorumlulukMerkezi.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_SorumlulukMerkezi.View = gridView_Arama_SorumlulukMerkezi;
		repositoryItem_SorumlulukMerkezi.CloseUp += repositoryItem_SorumlulukMerkezi_CloseUp;
		repositoryItem_SorumlulukMerkezi.QueryPopUp += repositoryItem_SorumlulukMerkezi_QueryPopUp;
		repositoryItem_SorumlulukMerkezi.EndInit();
		repositoryItem_SorumlulukMerkezi.DataSource = _lookuptablolar.Tables["SORUMLULUK_MERKEZLERI"];
		repositoryItem_SorumlulukMerkezi.ValueMember = "KODU";
		repositoryItem_SorumlulukMerkezi.DisplayMember = "ADI";
		repositoryItem_SorumlulukMerkezi.PopulateViewColumns();
		repositoryItem_SorumlulukMerkezi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_SorumlulukMerkezi.ValidateOnEnterKey = true;
		repositoryItem_SorumlulukMerkezi.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_SorumlulukMerkezi.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += sorumlulukmerkezleri_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += sorumlulukmerkezleri_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Sorumluluk merkezleri";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_SorumlulukMerkezi_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_sorumlulukmerkezi_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_sorumlulukmerkezi_temp_defaultlayoutStream);
	}

	private void repositoryItem_SorumlulukMerkezi_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_sm.lsm"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_sm.lsm");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void sorumlulukmerkezleri_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_sorumlulukmerkezi_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_sorumlulukmerkezi_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_sm.lsm", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_sorumlulukmerkezi_temp_defaultlayoutStream.Length];
		arama_sorumlulukmerkezi_temp_defaultlayoutStream.Read(array, 0, (int)arama_sorumlulukmerkezi_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void sorumlulukmerkezleri_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_sm.lsm"))
		{
			File.Delete("data\\views\\" + _tag + "_sm.lsm");
		}
	}

	private void RepositoryItem_FiyatListesi_Init()
	{
		gridView_Arama_FiyatListesi = new GridView();
		gridView_Arama_FiyatListesi.BeginInit();
		gridView_Arama_FiyatListesi.FocusRectStyle = DrawFocusRectStyle.RowFocus;
		gridView_Arama_FiyatListesi.OptionsSelection.EnableAppearanceFocusedCell = false;
		gridView_Arama_FiyatListesi.OptionsView.ShowGroupPanel = false;
		gridView_Arama_FiyatListesi.EndInit();
		repositoryItem_FiyatListesi = new RepositoryItemGridLookUpEdit();
		repositoryItem_FiyatListesi.BeginInit();
		repositoryItem_FiyatListesi.AutoHeight = false;
		repositoryItem_FiyatListesi.Buttons.AddRange(new EditorButton[1]
		{
			new EditorButton(ButtonPredefines.Combo)
		});
		repositoryItem_FiyatListesi.View = gridView_Arama_FiyatListesi;
		repositoryItem_FiyatListesi.CloseUp += repositoryItem_FiyatListesi_CloseUp;
		repositoryItem_FiyatListesi.QueryPopUp += repositoryItem_FiyatListesi_QueryPopUp;
		repositoryItem_FiyatListesi.EndInit();
		repositoryItem_FiyatListesi.DataSource = _lookuptablolar.Tables["STOK_SATIS_FIYAT_LISTE_TANIMLARI"];
		repositoryItem_FiyatListesi.ValueMember = "SIRA NO";
		repositoryItem_FiyatListesi.DisplayMember = "AÇIKLAMA";
		repositoryItem_FiyatListesi.PopulateViewColumns();
		repositoryItem_FiyatListesi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItem_FiyatListesi.ValidateOnEnterKey = true;
		repositoryItem_FiyatListesi.TextEditStyle = TextEditStyles.Standard;
		repositoryItem_FiyatListesi.AutoComplete = false;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem.Size = new Size(208, 22);
		toolStripMenuItem.Text = "Otomatik dosyaya kaydet";
		toolStripMenuItem.Click += fiyatlisteleri_otomatikDosyayaKaydet_Click;
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
		toolStripMenuItem2.Size = new Size(208, 22);
		toolStripMenuItem2.Text = "Otomatik dosyayı sil";
		toolStripMenuItem2.Click += fiyatlisteleri_otomatikDosyayiSil_Click;
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
		toolStripMenuItem3.DropDownItems.AddRange(new ToolStripItem[2] { toolStripMenuItem, toolStripMenuItem2 });
		toolStripMenuItem3.Size = new Size(152, 22);
		toolStripMenuItem3.Text = "Fiyat listeleri";
		aramaTablolariToolStripMenuItem.DropDownItems.Add(toolStripMenuItem3);
	}

	private void repositoryItem_FiyatListesi_CloseUp(object sender, CloseUpEventArgs e)
	{
		arama_fiyatlisteleri_temp_defaultlayoutStream = new MemoryStream();
		_tempview.SaveLayoutToStream(arama_fiyatlisteleri_temp_defaultlayoutStream);
	}

	private void repositoryItem_FiyatListesi_QueryPopUp(object sender, CancelEventArgs e)
	{
		GridLookUpEdit gridLookUpEdit = sender as GridLookUpEdit;
		_tempview = gridLookUpEdit.Properties.View;
		if (File.Exists("data\\views\\" + _tag + "_fiyatlistesi.lfl"))
		{
			_tempview.RestoreLayoutFromXml("data\\views\\" + _tag + "_fiyatlistesi.lfl");
			return;
		}
		_tempview.PopulateColumns();
		_tempview.BestFitColumns();
	}

	private void fiyatlisteleri_otomatikDosyayaKaydet_Click(object sender, EventArgs e)
	{
		if (arama_fiyatlisteleri_temp_defaultlayoutStream == null)
		{
			MessageBox.Show("Kayıt etmek için önce ilgili görünümü değiştirmeniz gerekli.");
			return;
		}
		arama_fiyatlisteleri_temp_defaultlayoutStream.Position = 0L;
		using FileStream fileStream = new FileStream("data\\views\\" + _tag + "_fiyatlistesi.lfl", FileMode.Create, FileAccess.Write);
		byte[] array = new byte[arama_fiyatlisteleri_temp_defaultlayoutStream.Length];
		arama_fiyatlisteleri_temp_defaultlayoutStream.Read(array, 0, (int)arama_fiyatlisteleri_temp_defaultlayoutStream.Length);
		fileStream.Write(array, 0, array.Length);
	}

	private void fiyatlisteleri_otomatikDosyayiSil_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + "_fiyatlistesi.lfl"))
		{
			File.Delete("data\\views\\" + _tag + "_fiyatlistesi.lfl");
		}
	}

	private void advBandedGridView_master_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		_ = (AdvBandedGridView)sender;
		GenelEvrakSatir genelEvrakSatir = (GenelEvrakSatir)e.Row;
		if (e.IsSetData)
		{
			switch (e.Column.FieldName)
			{
			case "gc_evrak_EvrakTipi":
				genelEvrakSatir.evraktipi = (enum_GenelEvrakTipleri)e.Value;
				break;
			case "gc_evrak_TicaretTuru":
				genelEvrakSatir.ticaretturu = (enum_cha_ticaret_turu)e.Value;
				break;
			case "gc_evrak_NormalIade":
				genelEvrakSatir.normaliade = (enum_cha_normal_Iade)e.Value;
				break;
			case "gc_evrak_AcikKapali":
				genelEvrakSatir.kapamasekli = (enum_KapamaSekli)e.Value;
				break;
			case "gc_satir_cinsi":
				genelEvrakSatir.satircinsi = (enum_SatirCinsi)e.Value;
				break;
			case "gc_evrak_otv_sekli":
				genelEvrakSatir.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)e.Value;
				break;
			case "gc_evrak_Kur":
				genelEvrakSatir.kur.dov_fiyat = (double)(decimal)e.Value;
				break;
			case "gc_evrak_OdemePlani":
				genelEvrakSatir.odemeplani = (int)e.Value;
				break;
			case "gc_evrak_Cari":
				EvrakCariDegistir(genelEvrakSatir, e.Value.ToString(), FiyatGuncelle: true);
				break;
			case "gc_satir_HesapKodu":
				EvrakStokHizmetKoduDegistir(genelEvrakSatir, e.Value.ToString(), FiyatGuncelle: true);
				break;
			case "gc_evrak_Depo":
				EvrakDepoDegistir(genelEvrakSatir, (int)e.Value);
				break;
			case "gc_evrak_Hedef_Depo":
				EvrakHedef_DepoDegistir(genelEvrakSatir, (int)e.Value);
				break;
			case "gc_evrak_SorMer":
				EvrakSorumlulukMerkeziDegistir(genelEvrakSatir, (string)e.Value);
				break;
			case "gc_evrak_Proje":
				EvrakProjeDegistir(genelEvrakSatir, (string)e.Value);
				break;
			case "gc_evrak_DovizCinsi":
				EvrakDovizCinsiDegistir(genelEvrakSatir, int.Parse(e.Value.ToString()));
				break;
			case "gc_evrak_KapamaHesapKodu":
				EvrakKapamaHesapKoduDegistir(genelEvrakSatir, e.Value.ToString());
				break;
			case "gc_evrak_FiyatListesi":
				EvrakFiyatListesiDegistir(genelEvrakSatir, (int)e.Value);
				break;
			}
		}
		if (!e.IsGetData)
		{
			return;
		}
		switch (e.Column.FieldName)
		{
		case "gc_evrak_EvrakTipi":
			e.Value = (int)genelEvrakSatir.evraktipi;
			break;
		case "gc_evrak_TicaretTuru":
			e.Value = (int)genelEvrakSatir.ticaretturu;
			break;
		case "gc_evrak_NormalIade":
			e.Value = (int)genelEvrakSatir.normaliade;
			break;
		case "gc_evrak_AcikKapali":
			e.Value = (int)genelEvrakSatir.kapamasekli;
			break;
		case "gc_satir_cinsi":
			e.Value = (int)genelEvrakSatir.satircinsi;
			break;
		case "gc_evrak_otv_sekli":
			e.Value = (int)genelEvrakSatir.birimfiyat.OtvUygulamaSekli;
			break;
		case "gc_evrak_Kur":
			e.Value = (decimal)genelEvrakSatir.kur.dov_fiyat;
			break;
		case "gc_evrak_AktarimDurumu":
		{
			enum_GenelEvrakAktarimDurumu aktarimDurumu = genelEvrakSatir.AktarimDurumu;
			if (aktarimDurumu == enum_GenelEvrakAktarimDurumu.Aktarilmamis)
			{
				e.Value = "";
			}
			else
			{
				e.Value = GenelUtilityWin.EnumToString(aktarimDurumu);
			}
			break;
		}
		case "gc_evrak_FiyatListesi":
			e.Value = genelEvrakSatir.fiyatlistesi.sfl_sirano;
			break;
		case "gc_evrak_OdemePlani":
			e.Value = genelEvrakSatir.odemeplani;
			break;
		case "gc_evrak_Cari":
			e.Value = genelEvrakSatir.cari.cari_kod;
			break;
		case "gc_satir_HesapKodu":
			e.Value = genelEvrakSatir.stokhizmetkodu;
			break;
		case "gc_evrak_Depo":
			e.Value = genelEvrakSatir.kaynakdepo.dep_no;
			break;
		case "gc_evrak_Hedef_Depo":
			e.Value = genelEvrakSatir.hedefdepo.dep_no;
			break;
		case "gc_evrak_SorMer":
			e.Value = genelEvrakSatir.sorumlulukmerkezi.som_kod;
			break;
		case "gc_evrak_Proje":
			e.Value = genelEvrakSatir.proje.pro_kodu;
			break;
		case "gc_evrak_ara_toplam":
			e.Value = genelEvrakSatir.GetEvrakAraToplam();
			break;
		case "gc_evrak_iskonto_toplam":
			e.Value = genelEvrakSatir.GetEvrakIskontoToplam();
			break;
		case "gc_evrak_masraf_toplam":
			e.Value = genelEvrakSatir.GetEvrakMasrafToplam();
			break;
		case "gc_evrak_kdv_toplam":
			e.Value = genelEvrakSatir.GetEvrakKdvToplam(_mikrouygulamabilgileri.vergitanimlari);
			break;
		case "gc_evrak_otv_toplam":
			e.Value = genelEvrakSatir.GetEvrakOtvToplam();
			break;
		case "gc_evrak_yekun":
			e.Value = genelEvrakSatir.GetEvrakYekun(_mikrouygulamabilgileri.vergitanimlari);
			break;
		case "gc_evrak_DovizCinsi":
		{
			int dovizcinsi = genelEvrakSatir.dovizcinsi;
			if (dovizcinsi == genelEvrakSatir.cari.cari_doviz_cinsi)
			{
				e.Value = 0;
			}
			if (dovizcinsi == genelEvrakSatir.cari.cari_doviz_cinsi1)
			{
				e.Value = 1;
			}
			if (dovizcinsi == genelEvrakSatir.cari.cari_doviz_cinsi2)
			{
				e.Value = 2;
			}
			break;
		}
		case "gc_evrak_KapamaHesapKodu":
			e.Value = genelEvrakSatir.kapamahesapkodu;
			break;
		}
	}

	private void advBandedGridView_master_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
	{
		GenelEvrakSatir genelEvrakSatir = (GenelEvrakSatir)(sender as AdvBandedGridView).GetRow(e.RowHandle);
		if (genelEvrakSatir == null)
		{
			return;
		}
		enum_KapamaSekli kapamasekli = genelEvrakSatir.kapamasekli;
		switch (e.Column.Name)
		{
		case "gc_evrak_OdemePlani":
			break;
		case "gc_evrak_KapamaHesapKodu":
			switch (kapamasekli)
			{
			case enum_KapamaSekli.BankadanKapanacak:
				repositoryItem_Banka.DisplayMember = "KOD";
				e.RepositoryItem = repositoryItem_Banka;
				break;
			case enum_KapamaSekli.CariPersoneldenKapanacak:
				repositoryItem_CariPersonel.DisplayMember = "KOD";
				e.RepositoryItem = repositoryItem_CariPersonel;
				break;
			case enum_KapamaSekli.KasadanKapanacak:
				repositoryItem_Kasa.DisplayMember = "KOD";
				e.RepositoryItem = repositoryItem_Kasa;
				break;
			}
			break;
		case "gc_evrak_CariKodu":
			repositoryItem_Cari.DisplayMember = "KOD";
			break;
		case "gc_evrak_CariAdi":
			repositoryItem_Cari.DisplayMember = "İSİM";
			break;
		case "gc_satir_HesapKodu":
			switch (genelEvrakSatir.satircinsi)
			{
			case enum_SatirCinsi.Hizmet:
				repositoryItem_Hizmet.DisplayMember = "KOD";
				e.RepositoryItem = repositoryItem_Hizmet;
				break;
			case enum_SatirCinsi.Stok:
				repositoryItem_Stok.DisplayMember = "KOD";
				e.RepositoryItem = repositoryItem_Stok;
				break;
			}
			break;
		case "gc_satir_HesapAdi":
			switch (genelEvrakSatir.satircinsi)
			{
			case enum_SatirCinsi.Hizmet:
				repositoryItem_Hizmet.DisplayMember = "İSİM";
				e.RepositoryItem = repositoryItem_Hizmet;
				break;
			case enum_SatirCinsi.Stok:
				repositoryItem_Stok.DisplayMember = "İSİM";
				e.RepositoryItem = repositoryItem_Stok;
				break;
			}
			break;
		case "gc_evrak_DepoNo":
			repositoryItem_Depo.DisplayMember = "DEPO NO";
			break;
		case "gc_evrak_DepoAdi":
			repositoryItem_Depo.DisplayMember = "DEPO ADI";
			break;
		case "gc_evrak_Hedef_DepoNo":
			repositoryItem_Hedef_Depo.DisplayMember = "DEPO NO";
			break;
		case "gc_evrak_Hedef_DepoAdi":
			repositoryItem_Hedef_Depo.DisplayMember = "DEPO ADI";
			break;
		case "gc_evrak_ProjeKodu":
			repositoryItem_Proje.DisplayMember = "PROJE KODU";
			break;
		case "gc_evrak_ProjeAdi":
			repositoryItem_Proje.DisplayMember = "PROJE ADI";
			break;
		case "gc_evrak_SorMerKodu":
			repositoryItem_SorumlulukMerkezi.DisplayMember = "KODU";
			break;
		case "gc_evrak_SorMerAdi":
			repositoryItem_SorumlulukMerkezi.DisplayMember = "ADI";
			break;
		}
	}

	private void advBandedGridView_master_ShownEditor(object sender, EventArgs e)
	{
		AdvBandedGridView advBandedGridView = (AdvBandedGridView)sender;
		if (advBandedGridView.FocusedColumn.FieldName == "gc_evrak_DovizCinsi" && advBandedGridView.ActiveEditor is LookUpEdit)
		{
			LookUpEdit lookUpEdit = (LookUpEdit)advBandedGridView.ActiveEditor;
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ID", typeof(int));
			dataTable.Columns.Add("Isim", typeof(string));
			Cari cari = ((GenelEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow()).cari;
			if (cari.cari_doviz_cinsi != 255)
			{
				int cari_doviz_cinsi = cari.cari_doviz_cinsi;
				dataTable.Rows.Add(0, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari_doviz_cinsi).Kur_adi);
			}
			if (cari.cari_doviz_cinsi1 != 255)
			{
				int cari_doviz_cinsi2 = cari.cari_doviz_cinsi1;
				dataTable.Rows.Add(1, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari_doviz_cinsi2).Kur_adi);
			}
			if (cari.cari_doviz_cinsi2 != 255)
			{
				int cari_doviz_cinsi3 = cari.cari_doviz_cinsi2;
				dataTable.Rows.Add(2, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari_doviz_cinsi3).Kur_adi);
			}
			lookUpEdit.Properties.DataSource = dataTable;
			lookUpEdit.Properties.ValueMember = "ID";
			lookUpEdit.Properties.DisplayMember = "Isim";
			lookUpEdit.Properties.Columns.Add(new LookUpColumnInfo("Isim", 100, "Döviz cinsleri"));
			lookUpEdit.Properties.AutoSearchColumnIndex = 1;
			lookUpEdit.Properties.ValidateOnEnterKey = true;
			lookUpEdit.Properties.SearchMode = SearchMode.AutoComplete;
			lookUpEdit.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
		}
	}

	private void advBandedGridView_master_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
	{
		if (e.Value == null || e.ListSourceRowIndex < 0)
		{
			return;
		}
		GenelEvrakSatir genelEvrakSatir = _satirlar._satirlar[e.ListSourceRowIndex];
		int dovizcinsi = genelEvrakSatir.dovizcinsi;
		switch (e.Column.FieldName)
		{
		case "EvrakSira":
			if (Convert.ToInt32(e.Value) == 0)
			{
				e.DisplayText = "Otomatik";
			}
			break;
		case "gc_evrak_yekun":
			e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(dovizcinsi).Kur_sembol;
			break;
		case "gc_evrak_kdv_toplam":
			e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(dovizcinsi).Kur_sembol;
			break;
		case "gc_evrak_otv_toplam":
			e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(dovizcinsi).Kur_sembol;
			break;
		case "gc_evrak_masraf_toplam":
			e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(dovizcinsi).Kur_sembol;
			break;
		case "gc_evrak_iskonto_toplam":
			e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(dovizcinsi).Kur_sembol;
			break;
		case "gc_evrak_ara_toplam":
			e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(dovizcinsi).Kur_sembol;
			break;
		case "gc_evrak_DovizCinsi":
			e.DisplayText = _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(dovizcinsi).Kur_adi;
			break;
		case "gc_evrak_KapamaHesapKodu":
			e.DisplayText = genelEvrakSatir.kapamahesapkodu;
			break;
		case "gc_evrak_OdemePlani":
			if ((int)e.Value == 0)
			{
				e.DisplayText = "PEŞİN";
			}
			else if ((int)e.Value < 0)
			{
				e.DisplayText = (int)e.Value * -1 + " GÜN";
			}
			break;
		}
	}

	private void advBandedGridView_master_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
	{
		if (e.RowHandle < 0)
		{
			return;
		}
		GenelEvrakSatir genelEvrakSatir = _satirlar._satirlar[advBandedGridView_master.GetDataSourceRowIndex(e.RowHandle)];
		switch (e.Column.Name)
		{
		case "gc_evrak_AktarimDurumu":
			switch (genelEvrakSatir.AktarimDurumu)
			{
			case enum_GenelEvrakAktarimDurumu.Aktarilmamis:
				e.Appearance.BackColor = RenkBeyaz;
				break;
			case enum_GenelEvrakAktarimDurumu.Aktarilmis:
				e.Appearance.BackColor = RenkYesil;
				break;
			default:
				e.Appearance.BackColor = RenkKirmizi;
				break;
			}
			break;
		case "gc_satir_HesapKodu":
			if (genelEvrakSatir.stokhizmetkodu == "" || genelEvrakSatir.stokhizmetkodu == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		case "gc_satir_HesapAdi":
			if (genelEvrakSatir.stokhizmetkodu == "" || genelEvrakSatir.stokhizmetkodu == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		case "gc_evrak_CariKodu":
			if ((genelEvrakSatir.cari.cari_kod == "" || genelEvrakSatir.cari.cari_kod == null) && genelEvrakSatir.evraktipi != enum_GenelEvrakTipleri.DepolarArasiSevk)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		case "gc_evrak_CariAdi":
			if ((genelEvrakSatir.cari.cari_kod == "" || genelEvrakSatir.cari.cari_kod == null) && genelEvrakSatir.evraktipi != enum_GenelEvrakTipleri.DepolarArasiSevk)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		case "gc_evrak_KapamaHesapKodu":
			if (genelEvrakSatir.kapamasekli != enum_KapamaSekli.AcikHesap)
			{
				string kapamahesapkodu = genelEvrakSatir.kapamahesapkodu;
				if (kapamahesapkodu == "" || kapamahesapkodu == null)
				{
					e.Appearance.BackColor = RenkKirmizi;
				}
				else
				{
					e.Appearance.BackColor = RenkBeyaz;
				}
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		}
	}

	private void advBandedGridView_master_CustomDrawEmptyForeground(object sender, CustomDrawEventArgs e)
	{
		if ((sender as GridView).RowCount == 0)
		{
			StringFormat stringFormat = new StringFormat();
			StringAlignment alignment = (stringFormat.LineAlignment = StringAlignment.Center);
			stringFormat.Alignment = alignment;
			e.Graphics.DrawString("Evrak yok.\n Yeni evrak eklemek için CTRL+N tuşlarına basabilir yada Evraklar -> Yeni evrak ekle menüsünü kullanabilirsiniz.", e.Appearance.Font, SystemBrushes.ControlDark, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), stringFormat);
		}
	}

	private void repositoryItemDateEdit_EvrakTarih_EditValueChanged(object sender, EventArgs e)
	{
		DateEdit dateEdit = (DateEdit)sender;
		GenelEvrakSatir genelEvrakSatir = (GenelEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow();
		int dovizcinsi = genelEvrakSatir.dovizcinsi;
		Kur kur = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, dovizcinsi, "1", dateEdit.DateTime, _mikrouygulamabilgileri.MikroAnaDBName);
		EvrakKurDegistir(genelEvrakSatir, kur.dov_fiyat);
	}

	private void repositoryItemTextEdit_Evrak_Kur_Validating(object sender, CancelEventArgs e)
	{
		if (((GenelEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow()).dovizcinsi == 0)
		{
			e.Cancel = true;
		}
	}

	private void repositoryItemGridLookUpEdit_AcikKapali_EditValueChanged(object sender, EventArgs e)
	{
		((GenelEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow()).kapamahesapkodu = "";
	}

	private void repositoryItemGridLookUpEdit_Satir_Cinsi_EditValueChanging(object sender, ChangingEventArgs e)
	{
		int result = 0;
		if (int.TryParse(e.NewValue.ToString(), out result))
		{
			GenelEvrakSatir genelEvrakSatir = (GenelEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow();
			if (genelEvrakSatir != null)
			{
				enum_SatirCinsi satircinsi = (enum_SatirCinsi)result;
				genelEvrakSatir.satircinsi = satircinsi;
				genelEvrakSatir.stokhizmetkodu = "";
			}
		}
	}

	private void EvrakCariDegistir(GenelEvrakSatir row, string YeniCariKodu, bool FiyatGuncelle)
	{
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraktipi == item.evraktipi && row.normaliade == item.normaliade && row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
				Cari cariByCariKod = CariData.GetCariByCariKod(sqlDB.Connection, YeniCariKodu, AdreslerTemsilciyeGore: false, "");
				sqlDB.ConnectionClose();
				item.cari = cariByCariKod;
			}
		}
		EvrakDovizCinsiDegistir(row, 0);
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakStokHizmetKoduDegistir(GenelEvrakSatir row, string YeniStokHizmetKodu, bool FiyatGuncelle)
	{
		row.stokhizmetkodu = YeniStokHizmetKodu;
		enum_toptan_perakende toptan_perakende = enum_toptan_perakende.Toptan;
		if (row.ticaretturu == enum_cha_ticaret_turu.PerakendeYurtIciTicaret)
		{
			toptan_perakende = enum_toptan_perakende.Perakende;
		}
		if (row.satircinsi == enum_SatirCinsi.Stok)
		{
			row.vergi_pntr = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, row.stokhizmetkodu, toptan_perakende).ekleme_bilgileri.vergi_pntr;
		}
		else
		{
			row.vergi_pntr = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, row.stokhizmetkodu).hiz_KDV;
		}
		if (FiyatGuncelle)
		{
			SatirFiyatiGuncelle(row);
		}
	}

	private void SatirFiyatiGuncelle(GenelEvrakSatir satirrow)
	{
		if (satirrow.satircinsi == enum_SatirCinsi.Stok)
		{
			List<enum_Fiyat_Kaynagi> list = new List<enum_Fiyat_Kaynagi>();
			bool flag = true;
			switch (satirrow.evraktipi)
			{
			case enum_GenelEvrakTipleri.AlinanSiparis:
				flag = true;
				break;
			case enum_GenelEvrakTipleri.ProformaSiparis:
				flag = true;
				break;
			case enum_GenelEvrakTipleri.AlisFaturasi:
				flag = false;
				break;
			case enum_GenelEvrakTipleri.AlisIrsaliyesi:
				flag = false;
				break;
			case enum_GenelEvrakTipleri.SatisFaturasi:
				flag = true;
				break;
			case enum_GenelEvrakTipleri.SatisIrsaliyesi:
				flag = true;
				break;
			case enum_GenelEvrakTipleri.DepolarArasiSevk:
				flag = true;
				break;
			}
			if (flag)
			{
				list.Add(enum_Fiyat_Kaynagi.SatisSarti);
				list.Add(enum_Fiyat_Kaynagi.FiyatListesi);
				list.Add(enum_Fiyat_Kaynagi.CariyeSonSatisFiyati);
				list.Add(enum_Fiyat_Kaynagi.GenelSonSatisFiyati);
			}
			else
			{
				list.Add(enum_Fiyat_Kaynagi.AlisSarti);
				list.Add(enum_Fiyat_Kaynagi.CaridenSonAlisFiyati);
				list.Add(enum_Fiyat_Kaynagi.GenelSonAlisFiyati);
			}
			int otvVergiPntr = satirrow.birimfiyat.OtvVergiPntr;
			satirrow.birimfiyat = StokData.GetFiyatFromMultiSource(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satirrow.stokhizmetkodu, satirrow.vergi_pntr, satirrow.fiyatlistesi.sfl_sirano, satirrow.fiyatlistesi.sfl_kdvdahil, satirrow.cari.cari_kod, satirrow.cari.cari_satis_isk_kod, satirrow.kaynakdepo.dep_no, satirrow.evraktarih, list, _mikrouygulamabilgileri.vergitanimlari, 0);
			satirrow.birimfiyat.OtvVergiPntr = otvVergiPntr;
		}
		else
		{
			Hizmet hizmet = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satirrow.stokhizmetkodu);
			satirrow.birimfiyat = new FiyatTanimlamasi();
			satirrow.birimfiyat.DovizCinsi = satirrow.dovizcinsi;
			satirrow.birimfiyat.FiyatBrut = hizmet.BirimFiyat.FiyatBrut;
			satirrow.birimfiyat.OtvVergiPntr = hizmet.BirimFiyat.OtvVergiPntr;
		}
	}

	private void EvrakDepoDegistir(GenelEvrakSatir row, int YeniDepoNo)
	{
		row.kaynakdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, YeniDepoNo);
	}

	private void EvrakHedef_DepoDegistir(GenelEvrakSatir row, int YeniHedef_DepoNo)
	{
		row.hedefdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, YeniHedef_DepoNo);
	}

	private void EvrakProjeDegistir(GenelEvrakSatir row, string YeniProjeKodu)
	{
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraktipi == item.evraktipi && row.normaliade == item.normaliade && row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				Proje proje = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, YeniProjeKodu);
				item.proje = proje;
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakSorumlulukMerkeziDegistir(GenelEvrakSatir row, string YeniSorumlulukMerkeziKodu)
	{
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraktipi == item.evraktipi && row.normaliade == item.normaliade && row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				SorumlulukMerkezi sorumlulukMerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, YeniSorumlulukMerkeziKodu);
				item.sorumlulukmerkezi = sorumlulukMerkezi;
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakDovizCinsiDegistir(GenelEvrakSatir row, int YeniDovizCinsiSira)
	{
		int num = YeniDovizCinsiSira switch
		{
			0 => row.cari.cari_doviz_cinsi, 
			1 => row.cari.cari_doviz_cinsi1, 
			2 => row.cari.cari_doviz_cinsi2, 
			_ => row.cari.cari_doviz_cinsi, 
		};
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraktipi == item.evraktipi && row.normaliade == item.normaliade && row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				item.dovizcinsi = num;
			}
		}
		Kur kur = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, num, "1", row.evraktarih, _mikrouygulamabilgileri.MikroAnaDBName);
		EvrakKurDegistir(row, kur.dov_fiyat);
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakKapamaHesapKoduDegistir(GenelEvrakSatir row, string YeniHesapKodu)
	{
		enum_KapamaSekli kapamasekli = row.kapamasekli;
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraktipi == item.evraktipi && row.normaliade == item.normaliade && row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				if (kapamasekli == enum_KapamaSekli.AcikHesap)
				{
					item.kapamahesapkodu = "";
				}
				else
				{
					item.kapamahesapkodu = YeniHesapKodu;
				}
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakFiyatListesiDegistir(GenelEvrakSatir row, int YeniFiyatListeno)
	{
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraktipi == item.evraktipi && row.normaliade == item.normaliade && row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				FiyatListesi fiyatListesi = FiyatListesiData.GetFiyatListesi(Db.Connection, YeniFiyatListeno);
				item.fiyatlistesi = fiyatListesi;
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakKurDegistir(GenelEvrakSatir row, double YeniKur)
	{
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraktipi == item.evraktipi && row.normaliade == item.normaliade && row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				item.kur.dov_fiyat = YeniKur;
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void acToolStripMenuItem_Click(object sender, EventArgs e)
	{
		openFileDialog1.Filter = "Genel Evrak Dosyaları|*.fge|Tüm dosyalar|*.*";
		if (openFileDialog1.ShowDialog() == DialogResult.OK)
		{
			GenelEvrakSatirBaslik satirlar = GenelEvrakSatirBaslik.ReadFromByteArray((byte[])GenelUtilityWin.ToObjectGZip(File.ReadAllBytes(openFileDialog1.FileName)));
			_satirlar = satirlar;
			bindData();
		}
	}

	private void kaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (openFileDialog1.FileName != "")
		{
			Kaydet(openFileDialog1.FileName);
		}
		else
		{
			FarkliKaydet();
		}
	}

	private void farklıKaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FarkliKaydet();
	}

	private void yeniToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show("Kayıt edilmemiş değişiklikleri iptal edip, yeni çalışma dosyası açmak istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			GenelEvrakSatirBaslik satirlar = new GenelEvrakSatirBaslik();
			_satirlar = satirlar;
			bindData();
		}
	}

	private void FarkliKaydet()
	{
		saveFileDialog1.Filter = "Genel Evrak Dosyaları|*.fge|Tüm dosyalar|*.*";
		if (saveFileDialog1.ShowDialog() == DialogResult.OK)
		{
			openFileDialog1.FileName = saveFileDialog1.FileName;
			Kaydet(saveFileDialog1.FileName);
		}
	}

	private void Kaydet(string DosyaAdi)
	{
		Console.WriteLine("CALISMA DOSYASI KAYIT EDILECEK");
		try
		{
			if (!File.Exists(DosyaAdi))
			{
				File.Delete(DosyaAdi);
			}
			GenelEvrakSatirBaslik genelEvrakSatirBaslik = new GenelEvrakSatirBaslik();
			genelEvrakSatirBaslik._satirlar = _satirlar._satirlar;
			File.WriteAllBytes(DosyaAdi, GenelUtilityWin.ToByteArrayGzip(GenelEvrakSatirBaslik.WriteToByteArray(genelEvrakSatirBaslik, 1)));
		}
		catch
		{
			MessageBox.Show("Kayıt edilemedi. Lütfen daha sonra tekrar deneyiniz.");
		}
	}

	private void evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		advBandedGridView_master.SaveLayoutToXml("data\\views\\" + _tag + ".lgm");
	}

	private void evraklar_varsayılanaGeriDonToolStripMenuItem_Click(object sender, EventArgs e)
	{
		evraklar_defaultlayoutStream.Position = 0L;
		advBandedGridView_master.RestoreLayoutFromStream(evraklar_defaultlayoutStream);
	}

	private void evraklar_otomatikDosyadan_yukle_ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			advBandedGridView_master.RestoreLayoutFromXml("data\\views\\" + _tag + ".lgm");
		}
	}

	private void evraklar_otomatikDosyayiSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			File.Delete("data\\views\\" + _tag + ".lgm");
		}
	}

	private void evraklar_kolonlaraGoreGruplamaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (advBandedGridView_master.OptionsView.ShowGroupPanel)
		{
			advBandedGridView_master.OptionsView.ShowGroupPanel = false;
		}
		else
		{
			advBandedGridView_master.OptionsView.ShowGroupPanel = true;
		}
	}

	private void evraklar_kolonSeciciyiGosterToolStripMenuItem_Click(object sender, EventArgs e)
	{
		advBandedGridView_master.ShowCustomization();
	}

	private void evraklar_butunGruplariAcToolStripMenuItem_Click(object sender, EventArgs e)
	{
		advBandedGridView_master.ExpandAllGroups();
	}

	private void evraklar_butunGruplariKapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		advBandedGridView_master.CollapseAllGroups();
	}

	private void evraklar_farkliDosyaya_kaydet_ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		saveFileDialog.Filter = "Genel Evrak Görünüm |*.lgm|Tüm dosyalar|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			advBandedGridView_master.SaveLayoutToXml(saveFileDialog.FileName);
		}
	}

	private void evraklar_farkliDosyadan_yukle_ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		openFileDialog.Filter = "Genel Evrak Görünüm |*.lgm|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			advBandedGridView_master.RestoreLayoutFromXml(openFileDialog.FileName);
		}
	}

	private void otomatikEvrakSiraNoVerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OtomatikEvrakSiraNoVer(SatirBirlestir: true);
	}

	private void otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OtomatikEvrakSiraNoVer(SatirBirlestir: false);
	}

	private void varsayilanDegerlerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		MessageBox.Show("YAPIM AŞAMASINDA");
	}

	private void akisParametreleriToolStripMenuItem_Click(object sender, EventArgs e)
	{
		MessageBox.Show("YAPIM AŞAMASINDA");
	}

	private void kriterleriDuzenleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		new KriterDuzenleme(_mikrouygulamabilgileri).ShowDialog();
	}

	private void txtCsvAktarimParametreleriToolStripMenuItem_Click(object sender, EventArgs e)
	{
		new TxtCsvImportAktarimParametreleriDuzenle(_mikrouygulamabilgileri).ShowDialog();
	}

	private void sqlAktarimParametreleriToolStripMenuItem_Click(object sender, EventArgs e)
	{
		new SqlImportAktarimParametreleriDuzenle(_mikrouygulamabilgileri).ShowDialog();
	}

	private void sb_Secili_Evraklari_aktar_Click(object sender, EventArgs e)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		AktarilacakEvrakSayisi = 0;
		KontrolSonucu = true;
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (item.Aktar)
			{
				AktarilacakEvrakSayisi++;
			}
		}
		IslemeBasla();
		label_islem_adi.Text = "Bilgi kontrolü";
		progressBar1.Maximum = AktarilacakEvrakSayisi;
		progressBar1.Value = 0;
		backgroundWorker_Kontrol.RunWorkerAsync();
	}

	private void IslemeBasla()
	{
		label_islem_bilgi.Text = "";
		gridControl_evraklar.Enabled = false;
		menuStrip1.Enabled = false;
		sb_Secili_Evraklari_aktar.Enabled = false;
	}

	private void IslemiBitir()
	{
		gridControl_evraklar.Enabled = true;
		menuStrip1.Enabled = true;
		sb_Secili_Evraklari_aktar.Enabled = true;
	}

	private void backgroundWorker_Kontrol_DoWork(object sender, DoWorkEventArgs e)
	{
		int num = 0;
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (item.Aktar)
			{
				TekSatirKontrolEt(item);
				num++;
				backgroundWorker_Kontrol.ReportProgress(num);
			}
		}
	}

	private void TekSatirKontrolEt(GenelEvrakSatir satir)
	{
		satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
		if ((satir.cari.cari_kod == "" || satir.cari.cari_kod == null) && satir.kapamasekli == enum_KapamaSekli.AcikHesap && satir.evraktipi != enum_GenelEvrakTipleri.DepolarArasiSevk)
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.CariKoduGirilmemis;
			KontrolSonucu = false;
		}
		if (satir.kapamasekli != enum_KapamaSekli.AcikHesap && (satir.kapamahesapkodu == "" || satir.kapamahesapkodu == null))
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.KapamaHesapKoduGirilmemis;
			KontrolSonucu = false;
		}
		if (satir.stokhizmetkodu == "" || satir.stokhizmetkodu == null)
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.HesapKoduGirilmemis;
			KontrolSonucu = false;
		}
		if (satir.evraknosira <= 0)
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.EvrakSiraSifirdanBuyukOlmali;
			KontrolSonucu = false;
		}
		if (satir.miktar <= 0.0)
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.MiktarSifirdanBuyukOlmali;
			KontrolSonucu = false;
		}
	}

	private void backgroundWorker_Kontrol_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_islem_bilgi.Text = e.ProgressPercentage + "/" + AktarilacakEvrakSayisi;
	}

	private void backgroundWorker_Kontrol_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (KontrolSonucu)
		{
			IslemeBasla();
			SatirlariEvragaCevir();
			AktarilacakEvrakSayisi = _AktarilacakEvraklar.Count;
			label_islem_adi.Text = "Aktarılıyor";
			progressBar1.Maximum = _AktarilacakEvraklar.Count;
			progressBar1.Value = 0;
			backgroundWorker_Aktarim.RunWorkerAsync();
		}
		else
		{
			IslemiBitir();
			gridControl_evraklar.RefreshDataSource();
			label_islem_adi.Text = "Hata var.";
			MessageBox.Show("Satırlarda hatalar var. Lütfen hataları düzeltip tekrar deneyiniz!");
		}
	}

	private void SatirlariEvragaCevir()
	{
		_AktarilacakEvraklar = new List<Evrak>();
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (!item.Aktar)
			{
				continue;
			}
			bool flag = false;
			foreach (Evrak item2 in _AktarilacakEvraklar)
			{
				if (item.evraktipi == item2.evraktipi && item.evraknoseri == item2.EvrakNoSeri && item.evraknosira == item2.EvrakNoSira && item.normaliade == item2.normaliade)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Evrak evrak = new Evrak(item.evraktipi, YeniKayitMi: true, EvrakKilitliMi: false, item.kapamasekli, item.normaliade, item.ticaretturu);
				evrak.Parametreler.vergitanimlari = _mikrouygulamabilgileri.vergitanimlari;
				evrak.SetDBCno(item.DBCno);
				evrak.SetEvrakTarihi(item.evraktarih);
				evrak.SetEvrakNoSeri(item.evraknoseri);
				evrak.SetEvraknoSira(item.evraknosira);
				evrak.SetBelgeNo(item.belgeno);
				evrak.SetBelgeTarihi(item.belgetarih);
				evrak.cari = item.cari;
				evrak.SetOdemePlani(item.odemeplani);
				evrak.SetFiyatListesi(item.fiyatlistesi);
				evrak.SetDovizCinsi(item.dovizcinsi);
				evrak.kur = item.kur;
				evrak.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
				evrak.alternatifdovizkuru = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, evrak.alternatifdovizcinsi, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
				evrak.SetKaynakDepo(item.kaynakdepo);
				evrak.SetHedefDepo(item.hedefdepo);
				evrak.SetProje(item.proje);
				evrak.SetSorumlulukMerkezi(item.sorumlulukmerkezi);
				evrak.SetTemsilciKodu(item.temsilcikodu);
				evrak.SetFirma(item.firma);
				evrak.SetSube(item.sube);
				evrak.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
				evrak.SetSevkTeslimTarihi(item.sevkteslimtarihi);
				evrak.SetSevkAdresNo(item.sevkadresno);
				evrak.SetKapamaHesapKodu(item.kapamahesapkodu);
				evrak.SetAciklama1(item.aciklama1);
				evrak.SetAciklama2(item.aciklama2);
				evrak.SetAciklama3(item.aciklama3);
				evrak.SetAciklama4(item.aciklama4);
				evrak.SetAciklama5(item.aciklama5);
				evrak.SetAciklama6(item.aciklama6);
				evrak.SetAciklama7(item.aciklama7);
				evrak.SetAciklama8(item.aciklama8);
				evrak.SetAciklama9(item.aciklama9);
				evrak.SetAciklama10(item.aciklama10);
				evrak.SetDegistirSpecialAlan1(item.degistirspecialalan1);
				evrak.SetDegistirSpecialAlan2(item.degistirspecialalan2);
				evrak.SetDegistirSpecialAlan3(item.degistirspecialalan3);
				_AktarilacakEvraklar.Add(evrak);
			}
			foreach (Evrak item3 in _AktarilacakEvraklar)
			{
				if (item.evraktipi != item3.evraktipi || !(item.evraknoseri == item3.EvrakNoSeri) || item.evraknosira != item3.EvrakNoSira || item.normaliade != item3.normaliade)
				{
					continue;
				}
				switch (item.satircinsi)
				{
				case enum_SatirCinsi.Stok:
				{
					Stok stok = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item.stokhizmetkodu, enum_toptan_perakende.Toptan);
					stok.sto_toptan_vergi_yeni = item.vergi_pntr;
					stok.sto_perakende_vergi_yeni = item.vergi_pntr;
					stok.ekleme_bilgileri.fiyat_farki_mi = item.fiyat_fark_mi;
					stok.ekleme_bilgileri.BirimFiyat = item.birimfiyat;
					stok.ekleme_bilgileri.vergi_pntr = item.vergi_pntr;
					stok.ekleme_bilgileri.Miktar = item.miktar;
					stok.ekleme_bilgileri.Miktar2 = item.miktar2;
					stok.ekleme_bilgileri.Aciklama1 = item.faturaaciklama;
					stok.ekleme_bilgileri.proje_kodu = item.proje.pro_kodu;
					stok.ekleme_bilgileri.sorumluluk_merkezi_kodu = item.sorumlulukmerkezi.som_kod;
					stok.ekleme_bilgileri.parti_kodu = item.stokpartikodu;
					stok.ekleme_bilgileri.lot_no = item.stoklotno;
					item3.AddUrun(stok);
					break;
				}
				case enum_SatirCinsi.Hizmet:
					if (item.evraktipi != enum_GenelEvrakTipleri.AlisIrsaliyesi && item.evraktipi != enum_GenelEvrakTipleri.SatisIrsaliyesi)
					{
						Hizmet hizmet = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item.stokhizmetkodu);
						hizmet.hiz_KDV = item.vergi_pntr;
						hizmet.BirimFiyat = item.birimfiyat;
						hizmet.Miktar = item.miktar;
						hizmet.Aciklama = item.faturaaciklama;
						item3.AddHizmet(hizmet, item.proje.pro_kodu, item.sorumlulukmerkezi.som_kod);
					}
					break;
				}
				break;
			}
		}
	}

	private void backgroundWorker_Aktarim_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		label_islem_bilgi.Text = islemismi;
		progressBar1.Value = e.ProgressPercentage;
		label_islem_bilgi.Text = e.ProgressPercentage + "/" + AktarilacakEvrakSayisi;
	}

	private void backgroundWorker_Aktarim_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		IslemiBitir();
		label_islem_adi.Text = "İşlem tamam";
	}

	private void backgroundWorker_Aktarim_DoWork(object sender, DoWorkEventArgs e)
	{
		int num = 999999999;
		string text = "genel_evrak_aktarimi";
		if (AppBase.MikroVersiyonu >= 16)
		{
			text = text + "_v" + AppBase.MikroVersiyonu;
		}
		if (!Lisans.GetLisans(_mikrouygulamabilgileri.baglantibilgileri.SqlServer, text).ModulLisansli(text))
		{
			num = 1;
		}
		int num2 = 0;
		foreach (Evrak item in _AktarilacakEvraklar)
		{
			if (_mikrouygulamabilgileri.AktarilanEvrakSayisi < num)
			{
				SqlConnection sqlConnection = new SqlConnection();
				if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
				{
					sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Trusted_Connection=true;";
				}
				else
				{
					sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; User Id=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + ";Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + ";";
				}
				sqlConnection.Open();
				int num3 = 0;
				bool flag = true;
				if (cb_belgeno_kontrolu_yap.Checked && EvrakData.BelgeNoVarMi(sqlConnection, item.evraktipi, item.BelgeNo))
				{
					flag = false;
					num3 = -3;
				}
				if (flag)
				{
					SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
					try
					{
						num3 = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, item, EArsivAktif: false);
						sqlTransaction.Commit();
					}
					catch (SqlException ex)
					{
						num3 = -1;
						Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
						sqlTransaction.Rollback();
						MessageBox.Show("Evrak kayıt edilirken bir hata meydana geldi. ErrorCode : " + ex.ErrorCode + " Errors : " + ex.Errors.ToString() + " Line Number : " + ex.LineNumber + " Message : " + ex.Message + " Source : " + ex.Source + " Procedure : " + ex.Procedure + " Hata : " + ex.ToString(), "HATA");
					}
					catch (Exception ex2)
					{
						num3 = -1;
						Console.WriteLine("HATA GERI ALINIYOR : " + ex2.ToString());
						sqlTransaction.Rollback();
						MessageBox.Show("Evrak kayıt edilirken bir hata meydana geldi. Hata : " + ex2.ToString(), "HATA");
					}
				}
				try
				{
					sqlConnection.Close();
				}
				catch
				{
				}
				enum_GenelEvrakAktarimDurumu enum_GenelEvrakAktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
				bool aktar = true;
				if (num3 <= 0)
				{
					enum_GenelEvrakAktarimDurumu = ((num3 != -2) ? enum_GenelEvrakAktarimDurumu.BeklenmedikHata : enum_GenelEvrakAktarimDurumu.EvrakVar);
				}
				else
				{
					aktar = false;
					enum_GenelEvrakAktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmis;
				}
				if (num3 == -3)
				{
					enum_GenelEvrakAktarimDurumu = enum_GenelEvrakAktarimDurumu.EvrakVar;
				}
				foreach (GenelEvrakSatir item2 in _satirlar._satirlar)
				{
					if (item2.Aktar && item2.evraktipi == item.evraktipi && item2.evraknoseri == item.EvrakNoSeri && item2.evraknosira == item.EvrakNoSira && item2.normaliade == item.normaliade)
					{
						item2.AktarimDurumu = enum_GenelEvrakAktarimDurumu;
						item2.Aktar = aktar;
					}
				}
			}
			else
			{
				enum_GenelEvrakAktarimDurumu aktarimDurumu = enum_GenelEvrakAktarimDurumu.LisansBulunamadi;
				foreach (GenelEvrakSatir item3 in _satirlar._satirlar)
				{
					if (item3.Aktar && item3.evraktipi == item.evraktipi && item3.evraknoseri == item.EvrakNoSeri && item3.evraknosira == item.EvrakNoSira && item3.normaliade == item.normaliade)
					{
						item3.AktarimDurumu = aktarimDurumu;
						item3.Aktar = true;
					}
				}
			}
			num2++;
			_mikrouygulamabilgileri.AktarilanEvrakSayisi++;
			backgroundWorker_Aktarim.ReportProgress(num2);
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void sb_TxtCsv_Al_Click(object sender, EventArgs e)
	{
		TxtCsvImport txtCsvImport = new TxtCsvImport(_mikrouygulamabilgileri);
		if (txtCsvImport.ShowDialog() == DialogResult.OK)
		{
			TxtCsvImport(txtCsvImport.te_dosyaadi.Text, txtCsvImport.cb_aktarimayarlari.SelectedValue.ToString());
		}
	}

	private void TxtCsvImport(string DosyaAdi, string AktarimAyarlariAdi)
	{
		BindingList<GenelEvrakSatir> bindingList = new BindingList<GenelEvrakSatir>();
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		bool flag = false;
		bool satirBirlestir = true;
		try
		{
			Parametreler parametreler = ParametrelerDefault.GenelAktarimTxtCsvSablon(AktarimAyarlariAdi);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "GenelAktarim", "", "TxtCsvAktarimSablon", AktarimAyarlariAdi);
			string getString = parametreler._GetParametre("KriterListesi")._GetString;
			string getString2 = parametreler._GetParametre("alinacak_satirlarin_baslangic_karakteri")._GetString;
			bool getBoolean = parametreler._GetParametre("alinacak_satirlarin_baslangic_karakteri_kullan")._GetBoolean;
			int getInt = parametreler._GetParametre("bilgilerin_baslangic_satiri")._GetInt;
			bool getBoolean2 = parametreler._GetParametre("bilgilerin_baslangic_satiri_kullan")._GetBoolean;
			string getString3 = parametreler._GetParametre("ayrac_karakteri")._GetString;
			bool getBoolean3 = parametreler._GetParametre("ayrac_karakteri_kullan")._GetBoolean;
			int getInt2 = parametreler._GetParametre("firma_no")._GetInt;
			int getInt3 = parametreler._GetParametre("sube_no")._GetInt;
			int getInt4 = parametreler._GetParametre("dbc_no")._GetInt;
			int getInt5 = parametreler._GetParametre("evrak_tarihi_yil_baslangic")._GetInt;
			int getInt6 = parametreler._GetParametre("evrak_tarihi_yil_uzunluk")._GetInt;
			int getInt7 = parametreler._GetParametre("evrak_tarihi_ay_baslangic")._GetInt;
			int getInt8 = parametreler._GetParametre("evrak_tarihi_ay_uzunluk")._GetInt;
			int getInt9 = parametreler._GetParametre("evrak_tarihi_gun_baslangic")._GetInt;
			int getInt10 = parametreler._GetParametre("evrak_tarihi_gun_uzunluk")._GetInt;
			int getInt11 = parametreler._GetParametre("belge_no_baslangic")._GetInt;
			int getInt12 = parametreler._GetParametre("belge_no_uzunluk")._GetInt;
			int getInt13 = parametreler._GetParametre("belge_tarihi_yil_baslangic")._GetInt;
			int getInt14 = parametreler._GetParametre("belge_tarihi_yil_uzunluk")._GetInt;
			int getInt15 = parametreler._GetParametre("belge_tarihi_ay_baslangic")._GetInt;
			int getInt16 = parametreler._GetParametre("belge_tarihi_ay_uzunluk")._GetInt;
			int getInt17 = parametreler._GetParametre("belge_tarihi_gun_baslangic")._GetInt;
			int getInt18 = parametreler._GetParametre("belge_tarihi_gun_uzunluk")._GetInt;
			int getInt19 = parametreler._GetParametre("kur_baslangic")._GetInt;
			int getInt20 = parametreler._GetParametre("kur_uzunluk")._GetInt;
			int getInt21 = parametreler._GetParametre("sevk_teslim_tarihi_yil_baslangic")._GetInt;
			int getInt22 = parametreler._GetParametre("sevk_teslim_tarihi_yil_uzunluk")._GetInt;
			int getInt23 = parametreler._GetParametre("sevk_teslim_tarihi_ay_baslangic")._GetInt;
			int getInt24 = parametreler._GetParametre("sevk_teslim_tarihi_ay_uzunluk")._GetInt;
			int getInt25 = parametreler._GetParametre("sevk_teslim_tarihi_gun_baslangic")._GetInt;
			int getInt26 = parametreler._GetParametre("sevk_teslim_tarihi_gun_uzunluk")._GetInt;
			bool getBoolean4 = parametreler._GetParametre("evrak_tipi_sabit_kullan")._GetBoolean;
			int getInt27 = parametreler._GetParametre("evrak_tipi_sabit_deger")._GetInt;
			int getInt28 = parametreler._GetParametre("evrak_tipi_baslangic")._GetInt;
			int getInt29 = parametreler._GetParametre("evrak_tipi_uzunluk")._GetInt;
			string getString4 = parametreler._GetParametre("evrak_tipi_veri_satis_faturasi")._GetString;
			string getString5 = parametreler._GetParametre("evrak_tipi_veri_satis_irsaliyesi")._GetString;
			string getString6 = parametreler._GetParametre("evrak_tipi_veri_alis_faturasi")._GetString;
			string getString7 = parametreler._GetParametre("evrak_tipi_veri_alis_irsaliyesi")._GetString;
			string getString8 = parametreler._GetParametre("evrak_tipi_veri_alinan_siparis")._GetString;
			string getString9 = parametreler._GetParametre("evrak_tipi_veri_depolar_arasi_sevk")._GetString;
			bool getBoolean5 = parametreler._GetParametre("normal_iade_sabit_kullan")._GetBoolean;
			int getInt30 = parametreler._GetParametre("normal_iade_sabit_deger")._GetInt;
			int getInt31 = parametreler._GetParametre("normal_iade_baslangic")._GetInt;
			int getInt32 = parametreler._GetParametre("normal_iade_uzunluk")._GetInt;
			string getString10 = parametreler._GetParametre("normal_iade_veri_normal")._GetString;
			string getString11 = parametreler._GetParametre("normal_iade_veri_iade")._GetString;
			bool getBoolean6 = parametreler._GetParametre("satir_cinsi_sabit_kullan")._GetBoolean;
			int getInt33 = parametreler._GetParametre("satir_cinsi_sabit_deger")._GetInt;
			int getInt34 = parametreler._GetParametre("satir_cinsi_baslangic")._GetInt;
			int getInt35 = parametreler._GetParametre("satir_cinsi_uzunluk")._GetInt;
			string getString12 = parametreler._GetParametre("satir_cinsi_veri_stok")._GetString;
			string getString13 = parametreler._GetParametre("satir_cinsi_veri_hizmet")._GetString;
			bool getBoolean7 = parametreler._GetParametre("satir_hesap_kodu_sabit_kullan")._GetBoolean;
			string getString14 = parametreler._GetParametre("satir_hesap_kodu_sabit_deger")._GetString;
			int getInt36 = parametreler._GetParametre("satir_hesap_kodu_baslangic")._GetInt;
			int getInt37 = parametreler._GetParametre("satir_hesap_kodu_uzunluk")._GetInt;
			bool getBoolean8 = parametreler._GetParametre("satir_parti_kodu_sabit_kullan")._GetBoolean;
			string getString15 = parametreler._GetParametre("satir_parti_kodu_sabit_deger")._GetString;
			int getInt38 = parametreler._GetParametre("satir_parti_kodu_baslangic")._GetInt;
			int getInt39 = parametreler._GetParametre("satir_parti_kodu_uzunluk")._GetInt;
			bool getBoolean9 = parametreler._GetParametre("satir_lot_no_sabit_kullan")._GetBoolean;
			int getInt40 = parametreler._GetParametre("satir_lot_no_sabit_deger")._GetInt;
			int getInt41 = parametreler._GetParametre("satir_lot_no_baslangic")._GetInt;
			int getInt42 = parametreler._GetParametre("satir_lot_no_uzunluk")._GetInt;
			bool getBoolean10 = parametreler._GetParametre("cari_kod_sabit_kullan")._GetBoolean;
			string getString16 = parametreler._GetParametre("cari_kod_sabit_deger")._GetString;
			int getInt43 = parametreler._GetParametre("cari_kod_baslangic")._GetInt;
			int getInt44 = parametreler._GetParametre("cari_kod_uzunluk")._GetInt;
			string getString17 = parametreler._GetParametre("cari_arama_secenekleri")._GetString;
			_ = parametreler._GetParametre("cari_unvan_turkce_karakterleri_kaldir")._GetBoolean;
			int getInt45 = parametreler._GetParametre("cari_unvan_baslangic")._GetInt;
			int getInt46 = parametreler._GetParametre("cari_unvan_uzunluk")._GetInt;
			int getInt47 = parametreler._GetParametre("cari_unvan2_baslangic")._GetInt;
			int getInt48 = parametreler._GetParametre("cari_unvan2_uzunluk")._GetInt;
			int getInt49 = parametreler._GetParametre("cari_vergi_no_baslangic")._GetInt;
			int getInt50 = parametreler._GetParametre("cari_vergi_no_uzunluk")._GetInt;
			int getInt51 = parametreler._GetParametre("cari_tc_kimlik_no_baslangic")._GetInt;
			int getInt52 = parametreler._GetParametre("cari_tc_kimlik_no_uzunluk")._GetInt;
			int getInt53 = parametreler._GetParametre("cari_vergi_dairesi_baslangic")._GetInt;
			int getInt54 = parametreler._GetParametre("cari_vergi_dairesi_uzunluk")._GetInt;
			int getInt55 = parametreler._GetParametre("cari_banka_hesap_no_baslangic")._GetInt;
			int getInt56 = parametreler._GetParametre("cari_banka_hesap_no_uzunluk")._GetInt;
			int getInt57 = parametreler._GetParametre("cari_adres_baslangic")._GetInt;
			int getInt58 = parametreler._GetParametre("cari_adres_uzunluk")._GetInt;
			int getInt59 = parametreler._GetParametre("cari_mahalle_baslangic")._GetInt;
			int getInt60 = parametreler._GetParametre("cari_mahalle_uzunluk")._GetInt;
			int getInt61 = parametreler._GetParametre("cari_ilce_baslangic")._GetInt;
			int getInt62 = parametreler._GetParametre("cari_ilce_uzunluk")._GetInt;
			int getInt63 = parametreler._GetParametre("cari_il_baslangic")._GetInt;
			int getInt64 = parametreler._GetParametre("cari_il_uzunluk")._GetInt;
			int getInt65 = parametreler._GetParametre("cari_ulke_baslangic")._GetInt;
			int getInt66 = parametreler._GetParametre("cari_ulke_uzunluk")._GetInt;
			int getInt67 = parametreler._GetParametre("cari_posta_kodu_baslangic")._GetInt;
			int getInt68 = parametreler._GetParametre("cari_posta_kodu_uzunluk")._GetInt;
			int getInt69 = parametreler._GetParametre("cari_telefon_baslangic")._GetInt;
			int getInt70 = parametreler._GetParametre("cari_telefon_uzunluk")._GetInt;
			int getInt71 = parametreler._GetParametre("cari_eposta_baslangic")._GetInt;
			int getInt72 = parametreler._GetParametre("cari_eposta_uzunluk")._GetInt;
			bool getBoolean11 = parametreler._GetParametre("kapama_sekli_sabit_kullan")._GetBoolean;
			int getInt73 = parametreler._GetParametre("kapama_sekli_sabit_deger")._GetInt;
			int getInt74 = parametreler._GetParametre("kapama_sekli_baslangic")._GetInt;
			int getInt75 = parametreler._GetParametre("kapama_sekli_uzunluk")._GetInt;
			string getString18 = parametreler._GetParametre("kapama_sekli_veri_acik_hesap")._GetString;
			string getString19 = parametreler._GetParametre("kapama_sekli_veri_kasadan_kapanacak")._GetString;
			string getString20 = parametreler._GetParametre("kapama_sekli_veri_bankadan_kapanacak")._GetString;
			string getString21 = parametreler._GetParametre("kapama_sekli_veri_cari_personelden_kapanacak")._GetString;
			bool getBoolean12 = parametreler._GetParametre("kapama_hesap_kodu_sabit_kullan")._GetBoolean;
			string getString22 = parametreler._GetParametre("kapama_hesap_kodu_sabit_deger")._GetString;
			int getInt76 = parametreler._GetParametre("kapama_hesap_kodu_baslangic")._GetInt;
			int getInt77 = parametreler._GetParametre("kapama_hesap_kodu_uzunluk")._GetInt;
			bool getBoolean13 = parametreler._GetParametre("proje_kodu_sabit_kullan")._GetBoolean;
			string getString23 = parametreler._GetParametre("proje_kodu_sabit_deger")._GetString;
			int getInt78 = parametreler._GetParametre("proje_kodu_baslangic")._GetInt;
			int getInt79 = parametreler._GetParametre("proje_kodu_uzunluk")._GetInt;
			bool getBoolean14 = parametreler._GetParametre("sor_mer_kodu_sabit_kullan")._GetBoolean;
			string getString24 = parametreler._GetParametre("sor_mer_kodu_sabit_deger")._GetString;
			int getInt80 = parametreler._GetParametre("sor_mer_kodu_baslangic")._GetInt;
			int getInt81 = parametreler._GetParametre("sor_mer_kodu_uzunluk")._GetInt;
			bool getBoolean15 = parametreler._GetParametre("plasiyer_kodu_sabit_kullan")._GetBoolean;
			bool getBoolean16 = parametreler._GetParametre("plasiyer_kodu_cariden_kullan")._GetBoolean;
			string getString25 = parametreler._GetParametre("plasiyer_kodu_sabit_deger")._GetString;
			int getInt82 = parametreler._GetParametre("plasiyer_kodu_baslangic")._GetInt;
			int getInt83 = parametreler._GetParametre("plasiyer_kodu_uzunluk")._GetInt;
			bool getBoolean17 = parametreler._GetParametre("satir_aciklama_sabit_kullan")._GetBoolean;
			string getString26 = parametreler._GetParametre("satir_aciklama_sabit_deger")._GetString;
			int getInt84 = parametreler._GetParametre("satir_aciklama_baslangic")._GetInt;
			int getInt85 = parametreler._GetParametre("satir_aciklama_uzunluk")._GetInt;
			bool getBoolean18 = parametreler._GetParametre("aciklama1_sabit_kullan")._GetBoolean;
			string getString27 = parametreler._GetParametre("aciklama1_sabit_deger")._GetString;
			int getInt86 = parametreler._GetParametre("aciklama1_baslangic")._GetInt;
			int getInt87 = parametreler._GetParametre("aciklama1_uzunluk")._GetInt;
			bool getBoolean19 = parametreler._GetParametre("aciklama2_sabit_kullan")._GetBoolean;
			string getString28 = parametreler._GetParametre("aciklama2_sabit_deger")._GetString;
			int getInt88 = parametreler._GetParametre("aciklama2_baslangic")._GetInt;
			int getInt89 = parametreler._GetParametre("aciklama2_uzunluk")._GetInt;
			bool getBoolean20 = parametreler._GetParametre("aciklama3_sabit_kullan")._GetBoolean;
			string getString29 = parametreler._GetParametre("aciklama3_sabit_deger")._GetString;
			int getInt90 = parametreler._GetParametre("aciklama3_baslangic")._GetInt;
			int getInt91 = parametreler._GetParametre("aciklama3_uzunluk")._GetInt;
			bool getBoolean21 = parametreler._GetParametre("aciklama4_sabit_kullan")._GetBoolean;
			string getString30 = parametreler._GetParametre("aciklama4_sabit_deger")._GetString;
			int getInt92 = parametreler._GetParametre("aciklama4_baslangic")._GetInt;
			int getInt93 = parametreler._GetParametre("aciklama4_uzunluk")._GetInt;
			bool getBoolean22 = parametreler._GetParametre("aciklama5_sabit_kullan")._GetBoolean;
			string getString31 = parametreler._GetParametre("aciklama5_sabit_deger")._GetString;
			int getInt94 = parametreler._GetParametre("aciklama5_baslangic")._GetInt;
			int getInt95 = parametreler._GetParametre("aciklama5_uzunluk")._GetInt;
			bool getBoolean23 = parametreler._GetParametre("aciklama6_sabit_kullan")._GetBoolean;
			string getString32 = parametreler._GetParametre("aciklama6_sabit_deger")._GetString;
			int getInt96 = parametreler._GetParametre("aciklama6_baslangic")._GetInt;
			int getInt97 = parametreler._GetParametre("aciklama6_uzunluk")._GetInt;
			bool getBoolean24 = parametreler._GetParametre("aciklama7_sabit_kullan")._GetBoolean;
			string getString33 = parametreler._GetParametre("aciklama7_sabit_deger")._GetString;
			int getInt98 = parametreler._GetParametre("aciklama7_baslangic")._GetInt;
			int getInt99 = parametreler._GetParametre("aciklama7_uzunluk")._GetInt;
			bool getBoolean25 = parametreler._GetParametre("aciklama8_sabit_kullan")._GetBoolean;
			string getString34 = parametreler._GetParametre("aciklama8_sabit_deger")._GetString;
			int getInt100 = parametreler._GetParametre("aciklama8_baslangic")._GetInt;
			int getInt101 = parametreler._GetParametre("aciklama8_uzunluk")._GetInt;
			bool getBoolean26 = parametreler._GetParametre("aciklama9_sabit_kullan")._GetBoolean;
			string getString35 = parametreler._GetParametre("aciklama9_sabit_deger")._GetString;
			int getInt102 = parametreler._GetParametre("aciklama9_baslangic")._GetInt;
			int getInt103 = parametreler._GetParametre("aciklama9_uzunluk")._GetInt;
			bool getBoolean27 = parametreler._GetParametre("aciklama10_sabit_kullan")._GetBoolean;
			string getString36 = parametreler._GetParametre("aciklama10_sabit_deger")._GetString;
			int getInt104 = parametreler._GetParametre("aciklama10_baslangic")._GetInt;
			int getInt105 = parametreler._GetParametre("aciklama10_uzunluk")._GetInt;
			bool getBoolean28 = parametreler._GetParametre("ozel_alan_1_sabit_kullan")._GetBoolean;
			string getString37 = parametreler._GetParametre("ozel_alan_1_sabit_deger")._GetString;
			int getInt106 = parametreler._GetParametre("ozel_alan_1_baslangic")._GetInt;
			int getInt107 = parametreler._GetParametre("ozel_alan_1_uzunluk")._GetInt;
			bool getBoolean29 = parametreler._GetParametre("ozel_alan_2_sabit_kullan")._GetBoolean;
			string getString38 = parametreler._GetParametre("ozel_alan_2_sabit_deger")._GetString;
			int getInt108 = parametreler._GetParametre("ozel_alan_2_baslangic")._GetInt;
			int getInt109 = parametreler._GetParametre("ozel_alan_2_uzunluk")._GetInt;
			bool getBoolean30 = parametreler._GetParametre("ozel_alan_3_sabit_kullan")._GetBoolean;
			string getString39 = parametreler._GetParametre("ozel_alan_3_sabit_deger")._GetString;
			int getInt110 = parametreler._GetParametre("ozel_alan_3_baslangic")._GetInt;
			int getInt111 = parametreler._GetParametre("ozel_alan_3_uzunluk")._GetInt;
			bool getBoolean31 = parametreler._GetParametre("birim_fiyat_mikroda_tanimli_fiyati_kullan")._GetBoolean;
			string getString40 = parametreler._GetParametre("birim_fiyat_satis_fiyat_kaynaklari")._GetString;
			string getString41 = parametreler._GetParametre("birim_fiyat_alis_fiyat_kaynaklari")._GetString;
			bool getBoolean32 = parametreler._GetParametre("stok_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt112 = parametreler._GetParametre("stok_birim_fiyat_baslangic")._GetInt;
			int getInt113 = parametreler._GetParametre("stok_birim_fiyat_uzunluk")._GetInt;
			int getInt114 = parametreler._GetParametre("stok_iskonto_1_uygulama_sekli")._GetInt;
			int getInt115 = parametreler._GetParametre("stok_iskonto_1_baslangic")._GetInt;
			int getInt116 = parametreler._GetParametre("stok_iskonto_1_uzunluk")._GetInt;
			int getInt117 = parametreler._GetParametre("stok_iskonto_2_uygulama_sekli")._GetInt;
			int getInt118 = parametreler._GetParametre("stok_iskonto_2_baslangic")._GetInt;
			int getInt119 = parametreler._GetParametre("stok_iskonto_2_uzunluk")._GetInt;
			int getInt120 = parametreler._GetParametre("stok_iskonto_3_uygulama_sekli")._GetInt;
			int getInt121 = parametreler._GetParametre("stok_iskonto_3_baslangic")._GetInt;
			int getInt122 = parametreler._GetParametre("stok_iskonto_3_uzunluk")._GetInt;
			int getInt123 = parametreler._GetParametre("stok_iskonto_4_uygulama_sekli")._GetInt;
			int getInt124 = parametreler._GetParametre("stok_iskonto_4_baslangic")._GetInt;
			int getInt125 = parametreler._GetParametre("stok_iskonto_4_uzunluk")._GetInt;
			int getInt126 = parametreler._GetParametre("stok_iskonto_5_uygulama_sekli")._GetInt;
			int getInt127 = parametreler._GetParametre("stok_iskonto_5_baslangic")._GetInt;
			int getInt128 = parametreler._GetParametre("stok_iskonto_5_uzunluk")._GetInt;
			int getInt129 = parametreler._GetParametre("stok_iskonto_6_uygulama_sekli")._GetInt;
			int getInt130 = parametreler._GetParametre("stok_iskonto_6_baslangic")._GetInt;
			int getInt131 = parametreler._GetParametre("stok_iskonto_6_uzunluk")._GetInt;
			int getInt132 = parametreler._GetParametre("stok_otv_uygulama_sekli")._GetInt;
			int getInt133 = parametreler._GetParametre("stok_otv_tutar_yuzde_baslangic")._GetInt;
			int getInt134 = parametreler._GetParametre("stok_otv_tutar_yuzde_uzunluk")._GetInt;
			bool getBoolean33 = parametreler._GetParametre("miktar_sabit_kullan")._GetBoolean;
			string getString42 = parametreler._GetParametre("miktar_sabit_deger")._GetString;
			int getInt135 = parametreler._GetParametre("miktar_baslangic")._GetInt;
			int getInt136 = parametreler._GetParametre("miktar_uzunluk")._GetInt;
			bool getBoolean34 = parametreler._GetParametre("miktar2_sabit_kullan")._GetBoolean;
			string getString43 = parametreler._GetParametre("miktar2_sabit_deger")._GetString;
			int getInt137 = parametreler._GetParametre("miktar2_baslangic")._GetInt;
			int getInt138 = parametreler._GetParametre("miktar2_uzunluk")._GetInt;
			bool getBoolean35 = parametreler._GetParametre("evrak_seri_sabit_kullan")._GetBoolean;
			string getString44 = parametreler._GetParametre("evrak_seri_sabit_deger_satis_faturasi")._GetString;
			string getString45 = parametreler._GetParametre("evrak_seri_sabit_deger_satis_irsaliyesi")._GetString;
			string getString46 = parametreler._GetParametre("evrak_seri_sabit_deger_alis_faturasi")._GetString;
			string getString47 = parametreler._GetParametre("evrak_seri_sabit_deger_alis_irsaliyesi")._GetString;
			string getString48 = parametreler._GetParametre("evrak_seri_sabit_deger_alinan_siparis")._GetString;
			string getString49 = parametreler._GetParametre("evrak_seri_sabit_deger_efatura_satis_faturasi")._GetString;
			string getString50 = parametreler._GetParametre("evrak_seri_sabit_deger_efatura_alis_faturasi")._GetString;
			string getString51 = parametreler._GetParametre("evrak_seri_sabit_deger_depolar_arasi_sevk")._GetString;
			bool getBoolean36 = parametreler._GetParametre("evrak_seri_efatura_carisi_mi_kontrol_et")._GetBoolean;
			int getInt139 = parametreler._GetParametre("evrak_seri_baslangic")._GetInt;
			int getInt140 = parametreler._GetParametre("evrak_seri_uzunluk")._GetInt;
			int getInt141 = parametreler._GetParametre("evrak_sira_baslangic")._GetInt;
			int getInt142 = parametreler._GetParametre("evrak_sira_uzunluk")._GetInt;
			int getInt143 = parametreler._GetParametre("stok_kdv_orani_bakilacak_yer")._GetInt;
			int getInt144 = parametreler._GetParametre("stok_kdv_orani_sabit_deger")._GetInt;
			int getInt145 = parametreler._GetParametre("stok_kdv_orani_baslangic")._GetInt;
			int getInt146 = parametreler._GetParametre("stok_kdv_orani_uzunluk")._GetInt;
			bool getBoolean37 = parametreler._GetParametre("hizmet_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt147 = parametreler._GetParametre("hizmet_birim_fiyat_baslangic")._GetInt;
			int getInt148 = parametreler._GetParametre("hizmet_birim_fiyat_uzunluk")._GetInt;
			int getInt149 = parametreler._GetParametre("hizmet_iskonto_1_uygulama_sekli")._GetInt;
			int getInt150 = parametreler._GetParametre("hizmet_iskonto_1_baslangic")._GetInt;
			int getInt151 = parametreler._GetParametre("hizmet_iskonto_1_uzunluk")._GetInt;
			int getInt152 = parametreler._GetParametre("hizmet_iskonto_2_uygulama_sekli")._GetInt;
			int getInt153 = parametreler._GetParametre("hizmet_iskonto_2_baslangic")._GetInt;
			int getInt154 = parametreler._GetParametre("hizmet_iskonto_2_uzunluk")._GetInt;
			int getInt155 = parametreler._GetParametre("hizmet_iskonto_3_uygulama_sekli")._GetInt;
			int getInt156 = parametreler._GetParametre("hizmet_iskonto_3_baslangic")._GetInt;
			int getInt157 = parametreler._GetParametre("hizmet_iskonto_3_uzunluk")._GetInt;
			int getInt158 = parametreler._GetParametre("hizmet_iskonto_4_uygulama_sekli")._GetInt;
			int getInt159 = parametreler._GetParametre("hizmet_iskonto_4_baslangic")._GetInt;
			int getInt160 = parametreler._GetParametre("hizmet_iskonto_4_uzunluk")._GetInt;
			int getInt161 = parametreler._GetParametre("hizmet_iskonto_5_uygulama_sekli")._GetInt;
			int getInt162 = parametreler._GetParametre("hizmet_iskonto_5_baslangic")._GetInt;
			int getInt163 = parametreler._GetParametre("hizmet_iskonto_5_uzunluk")._GetInt;
			int getInt164 = parametreler._GetParametre("hizmet_iskonto_6_uygulama_sekli")._GetInt;
			int getInt165 = parametreler._GetParametre("hizmet_iskonto_6_baslangic")._GetInt;
			int getInt166 = parametreler._GetParametre("hizmet_iskonto_6_uzunluk")._GetInt;
			int getInt167 = parametreler._GetParametre("hizmet_otv_uygulama_sekli")._GetInt;
			int getInt168 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_baslangic")._GetInt;
			int getInt169 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_uzunluk")._GetInt;
			int getInt170 = parametreler._GetParametre("hizmet_kdv_orani_bakilacak_yer")._GetInt;
			int getInt171 = parametreler._GetParametre("hizmet_kdv_orani_sabit_deger")._GetInt;
			int getInt172 = parametreler._GetParametre("hizmet_kdv_orani_baslangic")._GetInt;
			int getInt173 = parametreler._GetParametre("hizmet_kdv_orani_uzunluk")._GetInt;
			bool getBoolean38 = parametreler._GetParametre("depo_no_sabit_deger_kullan")._GetBoolean;
			int getInt174 = parametreler._GetParametre("depo_no_sabit_deger")._GetInt;
			bool getBoolean39 = parametreler._GetParametre("depo_no_cari_karttan_getir")._GetBoolean;
			int getInt175 = parametreler._GetParametre("depo_no_baslangic")._GetInt;
			int getInt176 = parametreler._GetParametre("depo_no_uzunluk")._GetInt;
			bool getBoolean40 = parametreler._GetParametre("hedef_depo_no_sabit_deger_kullan")._GetBoolean;
			int getInt177 = parametreler._GetParametre("hedef_depo_no_sabit_deger")._GetInt;
			int getInt178 = parametreler._GetParametre("hedef_depo_no_baslangic")._GetInt;
			int getInt179 = parametreler._GetParametre("hedef_depo_no_uzunluk")._GetInt;
			bool getBoolean41 = parametreler._GetParametre("evrakvedetaylarayrisatirlarda")._GetBoolean;
			string getString52 = parametreler._GetParametre("evrakbaslangickarakteri")._GetString;
			string getString53 = parametreler._GetParametre("satirbaslangickarakteri")._GetString;
			bool getBoolean42 = parametreler._GetParametre("satircinsiniotomatikbul")._GetBoolean;
			bool getBoolean43 = parametreler._GetParametre("cari_kodu_on_ek_kullan")._GetBoolean;
			string getString54 = parametreler._GetParametre("cari_kodu_on_ek_satis")._GetString;
			string getString55 = parametreler._GetParametre("cari_kodu_on_ek_alis")._GetString;
			bool getBoolean44 = parametreler._GetParametre("satir_aciklama_on_ek_kullan")._GetBoolean;
			string getString56 = parametreler._GetParametre("satir_aciklama_on_ek_deger")._GetString;
			bool getBoolean45 = parametreler._GetParametre("aciklama1_on_ek_kullan")._GetBoolean;
			string getString57 = parametreler._GetParametre("aciklama1_on_ek_deger")._GetString;
			bool getBoolean46 = parametreler._GetParametre("aciklama2_on_ek_kullan")._GetBoolean;
			string getString58 = parametreler._GetParametre("aciklama2_on_ek_deger")._GetString;
			bool getBoolean47 = parametreler._GetParametre("aciklama3_on_ek_kullan")._GetBoolean;
			string getString59 = parametreler._GetParametre("aciklama3_on_ek_deger")._GetString;
			bool getBoolean48 = parametreler._GetParametre("aciklama4_on_ek_kullan")._GetBoolean;
			string getString60 = parametreler._GetParametre("aciklama4_on_ek_deger")._GetString;
			bool getBoolean49 = parametreler._GetParametre("aciklama5_on_ek_kullan")._GetBoolean;
			string getString61 = parametreler._GetParametre("aciklama5_on_ek_deger")._GetString;
			bool getBoolean50 = parametreler._GetParametre("aciklama6_on_ek_kullan")._GetBoolean;
			string getString62 = parametreler._GetParametre("aciklama6_on_ek_deger")._GetString;
			bool getBoolean51 = parametreler._GetParametre("aciklama7_on_ek_kullan")._GetBoolean;
			string getString63 = parametreler._GetParametre("aciklama7_on_ek_deger")._GetString;
			bool getBoolean52 = parametreler._GetParametre("aciklama8_on_ek_kullan")._GetBoolean;
			string getString64 = parametreler._GetParametre("aciklama8_on_ek_deger")._GetString;
			bool getBoolean53 = parametreler._GetParametre("aciklama9_on_ek_kullan")._GetBoolean;
			string getString65 = parametreler._GetParametre("aciklama9_on_ek_deger")._GetString;
			bool getBoolean54 = parametreler._GetParametre("aciklama10_on_ek_kullan")._GetBoolean;
			string getString66 = parametreler._GetParametre("aciklama10_on_ek_deger")._GetString;
			bool getBoolean55 = parametreler._GetParametre("ticaret_turu_sabit_kullan")._GetBoolean;
			int getInt180 = parametreler._GetParametre("ticaret_turu_sabit_deger")._GetInt;
			bool getBoolean56 = parametreler._GetParametre("cari_il_bilgisi_plaka_kodu")._GetBoolean;
			int getInt181 = parametreler._GetParametre("stok_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt182 = parametreler._GetParametre("stok_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt183 = parametreler._GetParametre("stok_birim_fiyat_islem_1_uzunluk")._GetInt;
			int getInt184 = parametreler._GetParametre("stok_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt185 = parametreler._GetParametre("stok_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt186 = parametreler._GetParametre("stok_birim_fiyat_islem_2_uzunluk")._GetInt;
			int getInt187 = parametreler._GetParametre("stok_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt188 = parametreler._GetParametre("stok_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt189 = parametreler._GetParametre("stok_kdv_orani_islem_1_uzunluk")._GetInt;
			int getInt190 = parametreler._GetParametre("stok_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt191 = parametreler._GetParametre("stok_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt192 = parametreler._GetParametre("stok_kdv_orani_islem_2_uzunluk")._GetInt;
			int getInt193 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt194 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt195 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_1_uzunluk")._GetInt;
			int getInt196 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt197 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt198 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_2_uzunluk")._GetInt;
			int getInt199 = parametreler._GetParametre("stok_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt200 = parametreler._GetParametre("stok_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt201 = parametreler._GetParametre("stok_iskonto_1_islem_1_uzunluk")._GetInt;
			int getInt202 = parametreler._GetParametre("stok_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt203 = parametreler._GetParametre("stok_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt204 = parametreler._GetParametre("stok_iskonto_1_islem_2_uzunluk")._GetInt;
			int getInt205 = parametreler._GetParametre("stok_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt206 = parametreler._GetParametre("stok_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt207 = parametreler._GetParametre("stok_iskonto_2_islem_1_uzunluk")._GetInt;
			int getInt208 = parametreler._GetParametre("stok_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt209 = parametreler._GetParametre("stok_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt210 = parametreler._GetParametre("stok_iskonto_2_islem_2_uzunluk")._GetInt;
			int getInt211 = parametreler._GetParametre("stok_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt212 = parametreler._GetParametre("stok_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt213 = parametreler._GetParametre("stok_iskonto_3_islem_1_uzunluk")._GetInt;
			int getInt214 = parametreler._GetParametre("stok_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt215 = parametreler._GetParametre("stok_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt216 = parametreler._GetParametre("stok_iskonto_3_islem_2_uzunluk")._GetInt;
			int getInt217 = parametreler._GetParametre("stok_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt218 = parametreler._GetParametre("stok_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt219 = parametreler._GetParametre("stok_iskonto_4_islem_1_uzunluk")._GetInt;
			int getInt220 = parametreler._GetParametre("stok_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt221 = parametreler._GetParametre("stok_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt222 = parametreler._GetParametre("stok_iskonto_4_islem_2_uzunluk")._GetInt;
			int getInt223 = parametreler._GetParametre("stok_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt224 = parametreler._GetParametre("stok_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt225 = parametreler._GetParametre("stok_iskonto_5_islem_1_uzunluk")._GetInt;
			int getInt226 = parametreler._GetParametre("stok_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt227 = parametreler._GetParametre("stok_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt228 = parametreler._GetParametre("stok_iskonto_5_islem_2_uzunluk")._GetInt;
			int getInt229 = parametreler._GetParametre("stok_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt230 = parametreler._GetParametre("stok_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt231 = parametreler._GetParametre("stok_iskonto_6_islem_1_uzunluk")._GetInt;
			int getInt232 = parametreler._GetParametre("stok_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt233 = parametreler._GetParametre("stok_iskonto_6_islem_2_baslangic")._GetInt;
			int getInt234 = parametreler._GetParametre("stok_iskonto_6_islem_2_uzunluk")._GetInt;
			int getInt235 = parametreler._GetParametre("hizmet_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt236 = parametreler._GetParametre("hizmet_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt237 = parametreler._GetParametre("hizmet_birim_fiyat_islem_1_uzunluk")._GetInt;
			int getInt238 = parametreler._GetParametre("hizmet_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt239 = parametreler._GetParametre("hizmet_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt240 = parametreler._GetParametre("hizmet_birim_fiyat_islem_2_uzunluk")._GetInt;
			int getInt241 = parametreler._GetParametre("hizmet_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt242 = parametreler._GetParametre("hizmet_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt243 = parametreler._GetParametre("hizmet_kdv_orani_islem_1_uzunluk")._GetInt;
			int getInt244 = parametreler._GetParametre("hizmet_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt245 = parametreler._GetParametre("hizmet_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt246 = parametreler._GetParametre("hizmet_kdv_orani_islem_2_uzunluk")._GetInt;
			int getInt247 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt248 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt249 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_1_uzunluk")._GetInt;
			int getInt250 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt251 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt252 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_2_uzunluk")._GetInt;
			int getInt253 = parametreler._GetParametre("hizmet_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt254 = parametreler._GetParametre("hizmet_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt255 = parametreler._GetParametre("hizmet_iskonto_1_islem_1_uzunluk")._GetInt;
			int getInt256 = parametreler._GetParametre("hizmet_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt257 = parametreler._GetParametre("hizmet_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt258 = parametreler._GetParametre("hizmet_iskonto_1_islem_2_uzunluk")._GetInt;
			int getInt259 = parametreler._GetParametre("hizmet_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt260 = parametreler._GetParametre("hizmet_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt261 = parametreler._GetParametre("hizmet_iskonto_2_islem_1_uzunluk")._GetInt;
			int getInt262 = parametreler._GetParametre("hizmet_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt263 = parametreler._GetParametre("hizmet_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt264 = parametreler._GetParametre("hizmet_iskonto_2_islem_2_uzunluk")._GetInt;
			int getInt265 = parametreler._GetParametre("hizmet_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt266 = parametreler._GetParametre("hizmet_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt267 = parametreler._GetParametre("hizmet_iskonto_3_islem_1_uzunluk")._GetInt;
			int getInt268 = parametreler._GetParametre("hizmet_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt269 = parametreler._GetParametre("hizmet_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt270 = parametreler._GetParametre("hizmet_iskonto_3_islem_2_uzunluk")._GetInt;
			int getInt271 = parametreler._GetParametre("hizmet_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt272 = parametreler._GetParametre("hizmet_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt273 = parametreler._GetParametre("hizmet_iskonto_4_islem_1_uzunluk")._GetInt;
			int getInt274 = parametreler._GetParametre("hizmet_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt275 = parametreler._GetParametre("hizmet_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt276 = parametreler._GetParametre("hizmet_iskonto_4_islem_2_uzunluk")._GetInt;
			int getInt277 = parametreler._GetParametre("hizmet_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt278 = parametreler._GetParametre("hizmet_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt279 = parametreler._GetParametre("hizmet_iskonto_5_islem_1_uzunluk")._GetInt;
			int getInt280 = parametreler._GetParametre("hizmet_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt281 = parametreler._GetParametre("hizmet_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt282 = parametreler._GetParametre("hizmet_iskonto_5_islem_2_uzunluk")._GetInt;
			int getInt283 = parametreler._GetParametre("hizmet_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt284 = parametreler._GetParametre("hizmet_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt285 = parametreler._GetParametre("hizmet_iskonto_6_islem_1_uzunluk")._GetInt;
			int getInt286 = parametreler._GetParametre("hizmet_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt287 = parametreler._GetParametre("hizmet_iskonto_6_islem_2_baslangic")._GetInt;
			int getInt288 = parametreler._GetParametre("hizmet_iskonto_6_islem_2_uzunluk")._GetInt;
			bool getBoolean57 = parametreler._GetParametre("ikinci_satir_bilgisini_kullan")._GetBoolean;
			bool getBoolean58 = parametreler._GetParametre("ikinci_satir_cinsi_sabit_kullan")._GetBoolean;
			int getInt289 = parametreler._GetParametre("ikinci_satir_cinsi_sabit_deger")._GetInt;
			int getInt290 = parametreler._GetParametre("ikinci_satir_cinsi_baslangic")._GetInt;
			int getInt291 = parametreler._GetParametre("ikinci_satir_cinsi_uzunluk")._GetInt;
			string getString67 = parametreler._GetParametre("ikinci_satir_cinsi_veri_stok")._GetString;
			string getString68 = parametreler._GetParametre("ikinci_satir_cinsi_veri_hizmet")._GetString;
			bool getBoolean59 = parametreler._GetParametre("ikinci_satir_hesap_kodu_sabit_kullan")._GetBoolean;
			string getString69 = parametreler._GetParametre("ikinci_satir_hesap_kodu_sabit_deger")._GetString;
			int getInt292 = parametreler._GetParametre("ikinci_satir_hesap_kodu_baslangic")._GetInt;
			int getInt293 = parametreler._GetParametre("ikinci_satir_hesap_kodu_uzunluk")._GetInt;
			bool getBoolean60 = parametreler._GetParametre("ikinci_satir_parti_kodu_sabit_kullan")._GetBoolean;
			string getString70 = parametreler._GetParametre("ikinci_satir_parti_kodu_sabit_deger")._GetString;
			int getInt294 = parametreler._GetParametre("ikinci_satir_parti_kodu_baslangic")._GetInt;
			int getInt295 = parametreler._GetParametre("ikinci_satir_parti_kodu_uzunluk")._GetInt;
			bool getBoolean61 = parametreler._GetParametre("ikinci_satir_lot_no_sabit_kullan")._GetBoolean;
			int getInt296 = parametreler._GetParametre("ikinci_satir_lot_no_sabit_deger")._GetInt;
			int getInt297 = parametreler._GetParametre("ikinci_satir_lot_no_baslangic")._GetInt;
			int getInt298 = parametreler._GetParametre("ikinci_satir_lot_no_uzunluk")._GetInt;
			bool getBoolean62 = parametreler._GetParametre("ikinci_satir_aciklama_sabit_kullan")._GetBoolean;
			string getString71 = parametreler._GetParametre("ikinci_satir_aciklama_sabit_deger")._GetString;
			int getInt299 = parametreler._GetParametre("ikinci_satir_aciklama_baslangic")._GetInt;
			int getInt300 = parametreler._GetParametre("ikinci_satir_aciklama_uzunluk")._GetInt;
			bool getBoolean63 = parametreler._GetParametre("ikinci_birim_fiyat_mikroda_tanimli_fiyati_kullan")._GetBoolean;
			string getString72 = parametreler._GetParametre("ikinci_birim_fiyat_satis_fiyat_kaynaklari")._GetString;
			string getString73 = parametreler._GetParametre("ikinci_birim_fiyat_alis_fiyat_kaynaklari")._GetString;
			bool getBoolean64 = parametreler._GetParametre("ikinci_stok_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt301 = parametreler._GetParametre("ikinci_stok_birim_fiyat_baslangic")._GetInt;
			int getInt302 = parametreler._GetParametre("ikinci_stok_birim_fiyat_uzunluk")._GetInt;
			int getInt303 = parametreler._GetParametre("ikinci_stok_iskonto_1_uygulama_sekli")._GetInt;
			int getInt304 = parametreler._GetParametre("ikinci_stok_iskonto_1_baslangic")._GetInt;
			int getInt305 = parametreler._GetParametre("ikinci_stok_iskonto_1_uzunluk")._GetInt;
			int getInt306 = parametreler._GetParametre("ikinci_stok_iskonto_2_uygulama_sekli")._GetInt;
			int getInt307 = parametreler._GetParametre("ikinci_stok_iskonto_2_baslangic")._GetInt;
			int getInt308 = parametreler._GetParametre("ikinci_stok_iskonto_2_uzunluk")._GetInt;
			int getInt309 = parametreler._GetParametre("ikinci_stok_iskonto_3_uygulama_sekli")._GetInt;
			int getInt310 = parametreler._GetParametre("ikinci_stok_iskonto_3_baslangic")._GetInt;
			int getInt311 = parametreler._GetParametre("ikinci_stok_iskonto_3_uzunluk")._GetInt;
			int getInt312 = parametreler._GetParametre("ikinci_stok_iskonto_4_uygulama_sekli")._GetInt;
			int getInt313 = parametreler._GetParametre("ikinci_stok_iskonto_4_baslangic")._GetInt;
			int getInt314 = parametreler._GetParametre("ikinci_stok_iskonto_4_uzunluk")._GetInt;
			int getInt315 = parametreler._GetParametre("ikinci_stok_iskonto_5_uygulama_sekli")._GetInt;
			int getInt316 = parametreler._GetParametre("ikinci_stok_iskonto_5_baslangic")._GetInt;
			int getInt317 = parametreler._GetParametre("ikinci_stok_iskonto_5_uzunluk")._GetInt;
			int getInt318 = parametreler._GetParametre("ikinci_stok_iskonto_6_uygulama_sekli")._GetInt;
			int getInt319 = parametreler._GetParametre("ikinci_stok_iskonto_6_baslangic")._GetInt;
			int getInt320 = parametreler._GetParametre("ikinci_stok_iskonto_6_uzunluk")._GetInt;
			int getInt321 = parametreler._GetParametre("ikinci_stok_otv_uygulama_sekli")._GetInt;
			int getInt322 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_baslangic")._GetInt;
			int getInt323 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_uzunluk")._GetInt;
			bool getBoolean65 = parametreler._GetParametre("ikinci_miktar_sabit_kullan")._GetBoolean;
			string getString74 = parametreler._GetParametre("ikinci_miktar_sabit_deger")._GetString;
			int getInt324 = parametreler._GetParametre("ikinci_miktar_baslangic")._GetInt;
			int getInt325 = parametreler._GetParametre("ikinci_miktar_uzunluk")._GetInt;
			bool getBoolean66 = parametreler._GetParametre("ikinci_miktar2_sabit_kullan")._GetBoolean;
			string getString75 = parametreler._GetParametre("ikinci_miktar2_sabit_deger")._GetString;
			int getInt326 = parametreler._GetParametre("ikinci_miktar2_baslangic")._GetInt;
			int getInt327 = parametreler._GetParametre("ikinci_miktar2_uzunluk")._GetInt;
			int getInt328 = parametreler._GetParametre("ikinci_stok_kdv_orani_bakilacak_yer")._GetInt;
			int getInt329 = parametreler._GetParametre("ikinci_stok_kdv_orani_sabit_deger")._GetInt;
			int getInt330 = parametreler._GetParametre("ikinci_stok_kdv_orani_baslangic")._GetInt;
			int getInt331 = parametreler._GetParametre("ikinci_stok_kdv_orani_uzunluk")._GetInt;
			bool getBoolean67 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt332 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_baslangic")._GetInt;
			int getInt333 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_uzunluk")._GetInt;
			int getInt334 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_uygulama_sekli")._GetInt;
			int getInt335 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_baslangic")._GetInt;
			int getInt336 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_uzunluk")._GetInt;
			int getInt337 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_uygulama_sekli")._GetInt;
			int getInt338 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_baslangic")._GetInt;
			int getInt339 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_uzunluk")._GetInt;
			int getInt340 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_uygulama_sekli")._GetInt;
			int getInt341 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_baslangic")._GetInt;
			int getInt342 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_uzunluk")._GetInt;
			int getInt343 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_uygulama_sekli")._GetInt;
			int getInt344 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_baslangic")._GetInt;
			int getInt345 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_uzunluk")._GetInt;
			int getInt346 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_uygulama_sekli")._GetInt;
			int getInt347 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_baslangic")._GetInt;
			int getInt348 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_uzunluk")._GetInt;
			int getInt349 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_uygulama_sekli")._GetInt;
			int getInt350 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_baslangic")._GetInt;
			int getInt351 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_uzunluk")._GetInt;
			int getInt352 = parametreler._GetParametre("ikinci_hizmet_otv_uygulama_sekli")._GetInt;
			int getInt353 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_baslangic")._GetInt;
			int getInt354 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_uzunluk")._GetInt;
			int getInt355 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_bakilacak_yer")._GetInt;
			int getInt356 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_sabit_deger")._GetInt;
			int getInt357 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_baslangic")._GetInt;
			int getInt358 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_uzunluk")._GetInt;
			_ = parametreler._GetParametre("ikinci_satircinsiniotomatikbul")._GetBoolean;
			bool getBoolean68 = parametreler._GetParametre("ikinci_satir_aciklama_on_ek_kullan")._GetBoolean;
			string getString76 = parametreler._GetParametre("ikinci_satir_aciklama_on_ek_deger")._GetString;
			int getInt359 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt360 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt361 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_1_uzunluk")._GetInt;
			int getInt362 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt363 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt364 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_2_uzunluk")._GetInt;
			int getInt365 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt366 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt367 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_1_uzunluk")._GetInt;
			int getInt368 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt369 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt370 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_2_uzunluk")._GetInt;
			int getInt371 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt372 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt373 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_1_uzunluk")._GetInt;
			int getInt374 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt375 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt376 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_2_uzunluk")._GetInt;
			int getInt377 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt378 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt379 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_1_uzunluk")._GetInt;
			int getInt380 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt381 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt382 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_2_uzunluk")._GetInt;
			int getInt383 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt384 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt385 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_1_uzunluk")._GetInt;
			int getInt386 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt387 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt388 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_2_uzunluk")._GetInt;
			int getInt389 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt390 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt391 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_1_uzunluk")._GetInt;
			int getInt392 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt393 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt394 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_2_uzunluk")._GetInt;
			int getInt395 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt396 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt397 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_1_uzunluk")._GetInt;
			int getInt398 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt399 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt400 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_2_uzunluk")._GetInt;
			int getInt401 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt402 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt403 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_1_uzunluk")._GetInt;
			int getInt404 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt405 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt406 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_2_uzunluk")._GetInt;
			int getInt407 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt408 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt409 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_1_uzunluk")._GetInt;
			int getInt410 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt411 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_2_baslangic")._GetInt;
			int getInt412 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_2_uzunluk")._GetInt;
			int getInt413 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt414 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt415 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_1_uzunluk")._GetInt;
			int getInt416 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt417 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt418 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_2_uzunluk")._GetInt;
			int getInt419 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt420 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt421 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_1_uzunluk")._GetInt;
			int getInt422 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt423 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt424 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_2_uzunluk")._GetInt;
			int getInt425 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt426 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt427 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_1_uzunluk")._GetInt;
			int getInt428 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt429 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt430 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_2_uzunluk")._GetInt;
			int getInt431 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt432 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt433 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_1_uzunluk")._GetInt;
			int getInt434 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt435 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt436 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_2_uzunluk")._GetInt;
			int getInt437 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt438 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt439 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_1_uzunluk")._GetInt;
			int getInt440 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt441 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt442 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_2_uzunluk")._GetInt;
			int getInt443 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt444 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt445 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_1_uzunluk")._GetInt;
			int getInt446 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt447 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt448 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_2_uzunluk")._GetInt;
			int getInt449 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt450 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt451 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_1_uzunluk")._GetInt;
			int getInt452 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt453 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt454 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_2_uzunluk")._GetInt;
			int getInt455 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt456 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt457 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_1_uzunluk")._GetInt;
			int getInt458 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt459 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt460 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_2_uzunluk")._GetInt;
			int getInt461 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt462 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt463 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_1_uzunluk")._GetInt;
			int getInt464 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt465 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_2_baslangic")._GetInt;
			int getInt466 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_2_uzunluk")._GetInt;
			int getInt467 = parametreler._GetParametre("cari_doviz_cinsi")._GetInt;
			int getInt468 = parametreler._GetParametre("cari_doviz_cinsi1")._GetInt;
			int getInt469 = parametreler._GetParametre("cari_doviz_cinsi2")._GetInt;
			string getString77 = parametreler._GetParametre("cari_muh_kod_satis")._GetString;
			string getString78 = parametreler._GetParametre("cari_muh_kod1_satis")._GetString;
			string getString79 = parametreler._GetParametre("cari_muh_kod2_satis")._GetString;
			string getString80 = parametreler._GetParametre("cari_muhartikeli")._GetString;
			string getString81 = parametreler._GetParametre("cari_muh_kod_alis")._GetString;
			string getString82 = parametreler._GetParametre("cari_muh_kod1_alis")._GetString;
			string getString83 = parametreler._GetParametre("cari_muh_kod2_alis")._GetString;
			bool getBoolean69 = parametreler._GetParametre("fiyat_eksi_oldugunda_evrak_tipini_degistir")._GetBoolean;
			int getInt470 = parametreler._GetParametre("fiyat_eksi_oldugunda_yeni_evrak_tipi")._GetInt;
			bool getBoolean70 = parametreler._GetParametre("ikinci_fiyat_eksi_oldugunda_evrak_tipini_degistir")._GetBoolean;
			int getInt471 = parametreler._GetParametre("ikinci_fiyat_eksi_oldugunda_yeni_evrak_tipi")._GetInt;
			int getInt472 = parametreler._GetParametre("cari_kod2_baslangic")._GetInt;
			int getInt473 = parametreler._GetParametre("cari_kod2_uzunluk")._GetInt;
			bool getBoolean71 = parametreler._GetParametre("sto_isim_sabit_kullan")._GetBoolean;
			string getString84 = parametreler._GetParametre("sto_isim_sabit_deger")._GetString;
			int getInt474 = parametreler._GetParametre("sto_isim_baslangic")._GetInt;
			int getInt475 = parametreler._GetParametre("sto_isim_uzunluk")._GetInt;
			bool getBoolean72 = parametreler._GetParametre("sto_kisa_ismi_sabit_kullan")._GetBoolean;
			string getString85 = parametreler._GetParametre("sto_kisa_ismi_sabit_deger")._GetString;
			int getInt476 = parametreler._GetParametre("sto_kisa_ismi_baslangic")._GetInt;
			int getInt477 = parametreler._GetParametre("sto_kisa_ismi_uzunluk")._GetInt;
			bool getBoolean73 = parametreler._GetParametre("sto_yabanci_ismi_sabit_kullan")._GetBoolean;
			string getString86 = parametreler._GetParametre("sto_yabanci_ismi_sabit_deger")._GetString;
			int getInt478 = parametreler._GetParametre("sto_yabanci_ismi_baslangic")._GetInt;
			int getInt479 = parametreler._GetParametre("sto_yabanci_ismi_uzunluk")._GetInt;
			bool getBoolean74 = parametreler._GetParametre("sto_birim1_ad_sabit_kullan")._GetBoolean;
			string getString87 = parametreler._GetParametre("sto_birim1_ad_sabit_deger")._GetString;
			int getInt480 = parametreler._GetParametre("sto_birim1_ad_baslangic")._GetInt;
			int getInt481 = parametreler._GetParametre("sto_birim1_ad_uzunluk")._GetInt;
			bool getBoolean75 = parametreler._GetParametre("sto_birim1_katsayi_sabit_kullan")._GetBoolean;
			string getString88 = parametreler._GetParametre("sto_birim1_katsayi_sabit_deger")._GetString;
			int getInt482 = parametreler._GetParametre("sto_birim1_katsayi_baslangic")._GetInt;
			int getInt483 = parametreler._GetParametre("sto_birim1_katsayi_uzunluk")._GetInt;
			int getInt484 = parametreler._GetParametre("sto_otvuygulama")._GetInt;
			string getString89 = parametreler._GetParametre("sto_muh_kod")._GetString;
			string getString90 = parametreler._GetParametre("sto_muh_Iade_kod")._GetString;
			string getString91 = parametreler._GetParametre("sto_muh_sat_muh_kod")._GetString;
			string getString92 = parametreler._GetParametre("sto_muh_satIadmuhkod")._GetString;
			string getString93 = parametreler._GetParametre("sto_muh_sat_isk_kod")._GetString;
			string getString94 = parametreler._GetParametre("sto_muh_aIiskmuhkod")._GetString;
			string getString95 = parametreler._GetParametre("sto_muh_satmalmuhkod")._GetString;
			string getString96 = parametreler._GetParametre("sto_yurtdisi_satmuhk")._GetString;
			string getString97 = parametreler._GetParametre("sto_ilavemasmuhkod")._GetString;
			string getString98 = parametreler._GetParametre("sto_yatirimtesmuhkod")._GetString;
			string getString99 = parametreler._GetParametre("sto_depsatmuhkod")._GetString;
			string getString100 = parametreler._GetParametre("sto_depsatmalmuhkod")._GetString;
			string getString101 = parametreler._GetParametre("sto_bagortsatmuhkod")._GetString;
			string getString102 = parametreler._GetParametre("sto_bagortsatIadmuhkod")._GetString;
			string getString103 = parametreler._GetParametre("sto_bagortsatIskmuhkod")._GetString;
			string getString104 = parametreler._GetParametre("sto_satfiyfarkmuhkod")._GetString;
			string getString105 = parametreler._GetParametre("sto_yurtdisisatmalmuhkod")._GetString;
			string getString106 = parametreler._GetParametre("sto_bagortsatmalmuhkod")._GetString;
			string getString107 = parametreler._GetParametre("sto_muhgrup_kodu")._GetString;
			string getString108 = parametreler._GetParametre("sto_anagrup_kod")._GetString;
			string getString109 = parametreler._GetParametre("sto_altgrup_kod")._GetString;
			bool getBoolean76 = parametreler._GetParametre("hiz_isim_sabit_kullan")._GetBoolean;
			string getString110 = parametreler._GetParametre("hiz_isim_sabit_deger")._GetString;
			int getInt485 = parametreler._GetParametre("hiz_isim_baslangic")._GetInt;
			int getInt486 = parametreler._GetParametre("hiz_isim_uzunluk")._GetInt;
			bool getBoolean77 = parametreler._GetParametre("hiz_yabanci_isim_sabit_kullan")._GetBoolean;
			string getString111 = parametreler._GetParametre("hiz_yabanci_isim_sabit_deger")._GetString;
			int getInt487 = parametreler._GetParametre("hiz_yabanci_isim_baslangic")._GetInt;
			int getInt488 = parametreler._GetParametre("hiz_yabanci_isim_uzunluk")._GetInt;
			string getString112 = parametreler._GetParametre("hiz_tipkod")._GetString;
			string getString113 = parametreler._GetParametre("hiz_sinifkod")._GetString;
			string getString114 = parametreler._GetParametre("hiz_grupkod")._GetString;
			string getString115 = parametreler._GetParametre("hiz_sat_muh_kod")._GetString;
			string getString116 = parametreler._GetParametre("hiz_sat_iade_muh_kod")._GetString;
			string getString117 = parametreler._GetParametre("hiz_mal_muh_kod")._GetString;
			string getString118 = parametreler._GetParametre("hiz_sat_mal_muh_kod")._GetString;
			string getString119 = parametreler._GetParametre("hiz_mal_yan_muh_kod")._GetString;
			string getString120 = parametreler._GetParametre("hiz_muh_sat_isk_kod")._GetString;
			string getString121 = parametreler._GetParametre("hiz_muh_aIskmuhkod")._GetString;
			string getString122 = parametreler._GetParametre("hiz_ilavemasmuhkod")._GetString;
			bool getBoolean78 = parametreler._GetParametre("stok_fiyat_farki_mi_sabit_kullan")._GetBoolean;
			bool getBoolean79 = parametreler._GetParametre("stok_fiyat_farki_mi_sabit_deger")._GetBoolean;
			int getInt489 = parametreler._GetParametre("stok_fiyat_farki_mi_baslangic")._GetInt;
			int getInt490 = parametreler._GetParametre("stok_fiyat_farki_mi_uzunluk")._GetInt;
			string getString123 = parametreler._GetParametre("stok_fiyat_farki_icin_deger")._GetString;
			bool getBoolean80 = parametreler._GetParametre("ikinci_stok_fiyat_farki_mi_sabit_kullan")._GetBoolean;
			bool getBoolean81 = parametreler._GetParametre("ikinci_stok_fiyat_farki_mi_sabit_deger")._GetBoolean;
			int getInt491 = parametreler._GetParametre("ikinci_stok_fiyat_farki_mi_baslangic")._GetInt;
			int getInt492 = parametreler._GetParametre("ikinci_stok_fiyat_farki_mi_uzunluk")._GetInt;
			string getString124 = parametreler._GetParametre("ikinci_stok_fiyat_farki_icin_deger")._GetString;
			int getInt493 = parametreler._GetParametre("otv_vergi_pntr")._GetInt;
			int getInt494 = parametreler._GetParametre("kriter_metin1_baslangic")._GetInt;
			int getInt495 = parametreler._GetParametre("kriter_metin1_uzunluk")._GetInt;
			int getInt496 = parametreler._GetParametre("kriter_metin2_baslangic")._GetInt;
			int getInt497 = parametreler._GetParametre("kriter_metin2_uzunluk")._GetInt;
			int getInt498 = parametreler._GetParametre("kriter_metin3_baslangic")._GetInt;
			int getInt499 = parametreler._GetParametre("kriter_metin3_uzunluk")._GetInt;
			int getInt500 = parametreler._GetParametre("kriter_metin4_baslangic")._GetInt;
			int getInt501 = parametreler._GetParametre("kriter_metin4_uzunluk")._GetInt;
			int getInt502 = parametreler._GetParametre("kriter_metin5_baslangic")._GetInt;
			int getInt503 = parametreler._GetParametre("kriter_metin5_uzunluk")._GetInt;
			int getInt504 = parametreler._GetParametre("kriter_double1_baslangic")._GetInt;
			int getInt505 = parametreler._GetParametre("kriter_double1_uzunluk")._GetInt;
			int getInt506 = parametreler._GetParametre("kriter_double2_baslangic")._GetInt;
			int getInt507 = parametreler._GetParametre("kriter_double2_uzunluk")._GetInt;
			int getInt508 = parametreler._GetParametre("kriter_double3_baslangic")._GetInt;
			int getInt509 = parametreler._GetParametre("kriter_double3_uzunluk")._GetInt;
			int getInt510 = parametreler._GetParametre("kriter_double4_baslangic")._GetInt;
			int getInt511 = parametreler._GetParametre("kriter_double4_uzunluk")._GetInt;
			int getInt512 = parametreler._GetParametre("kriter_double5_baslangic")._GetInt;
			int getInt513 = parametreler._GetParametre("kriter_double5_uzunluk")._GetInt;
			int getInt514 = parametreler._GetParametre("kriter_bool1_baslangic")._GetInt;
			int getInt515 = parametreler._GetParametre("kriter_bool1_uzunluk")._GetInt;
			string getString125 = parametreler._GetParametre("kriter_bool1_evet_icin_deger")._GetString;
			int getInt516 = parametreler._GetParametre("kriter_bool2_baslangic")._GetInt;
			int getInt517 = parametreler._GetParametre("kriter_bool2_uzunluk")._GetInt;
			string getString126 = parametreler._GetParametre("kriter_bool2_evet_icin_deger")._GetString;
			int getInt518 = parametreler._GetParametre("kriter_bool3_baslangic")._GetInt;
			int getInt519 = parametreler._GetParametre("kriter_bool3_uzunluk")._GetInt;
			string getString127 = parametreler._GetParametre("kriter_bool3_evet_icin_deger")._GetString;
			int getInt520 = parametreler._GetParametre("kriter_bool4_baslangic")._GetInt;
			int getInt521 = parametreler._GetParametre("kriter_bool4_uzunluk")._GetInt;
			string getString128 = parametreler._GetParametre("kriter_bool4_evet_icin_deger")._GetString;
			int getInt522 = parametreler._GetParametre("kriter_bool5_baslangic")._GetInt;
			int getInt523 = parametreler._GetParametre("kriter_bool5_uzunluk")._GetInt;
			string getString129 = parametreler._GetParametre("kriter_bool5_evet_icin_deger")._GetString;
			bool getBoolean82 = parametreler._GetParametre("kayit_id_otomatik_ver")._GetBoolean;
			int getInt524 = parametreler._GetParametre("kayit_id_baslangic")._GetInt;
			int getInt525 = parametreler._GetParametre("kayit_id_uzunluk")._GetInt;
			int getInt526 = parametreler._GetParametre("otomatik_hesap_acma_secenek_cari")._GetInt;
			int getInt527 = parametreler._GetParametre("otomatik_hesap_acma_secenek_stok")._GetInt;
			int getInt528 = parametreler._GetParametre("otomatik_hesap_acma_secenek_hizmet")._GetInt;
			int getInt529 = parametreler._GetParametre("otomatik_hesap_acma_secenek_proje")._GetInt;
			int getInt530 = parametreler._GetParametre("otomatik_hesap_acma_secenek_sorumluluk")._GetInt;
			flag = parametreler._GetParametre("evrak_sira_otomatik_ver")._GetBoolean;
			satirBirlestir = parametreler._GetParametre("OtomatikEvrakSeriSatirBirlestir")._GetBoolean;
			bool getBoolean83 = parametreler._GetParametre("pro_adi_sabit_kullan")._GetBoolean;
			string getString130 = parametreler._GetParametre("pro_adi_sabit_deger")._GetString;
			int getInt531 = parametreler._GetParametre("pro_adi_baslangic")._GetInt;
			int getInt532 = parametreler._GetParametre("pro_adi_uzunluk")._GetInt;
			bool getBoolean84 = parametreler._GetParametre("pro_musterikodu_sabit_kullan")._GetBoolean;
			string getString131 = parametreler._GetParametre("pro_musterikodu_sabit_deger")._GetString;
			int getInt533 = parametreler._GetParametre("pro_musterikodu_baslangic")._GetInt;
			int getInt534 = parametreler._GetParametre("pro_musterikodu_uzunluk")._GetInt;
			bool getBoolean85 = parametreler._GetParametre("pro_sormerkodu_sabit_kullan")._GetBoolean;
			string getString132 = parametreler._GetParametre("pro_sormerkodu_sabit_deger")._GetString;
			int getInt535 = parametreler._GetParametre("pro_sormerkodu_baslangic")._GetInt;
			int getInt536 = parametreler._GetParametre("pro_sormerkodu_uzunluk")._GetInt;
			bool getBoolean86 = parametreler._GetParametre("pro_grupkodu_sabit_kullan")._GetBoolean;
			string getString133 = parametreler._GetParametre("pro_grupkodu_sabit_deger")._GetString;
			int getInt537 = parametreler._GetParametre("pro_grupkodu_baslangic")._GetInt;
			int getInt538 = parametreler._GetParametre("pro_grupkodu_uzunluk")._GetInt;
			bool getBoolean87 = parametreler._GetParametre("pro_sektorkodu_sabit_kullan")._GetBoolean;
			string getString134 = parametreler._GetParametre("pro_sektorkodu_sabit_deger")._GetString;
			int getInt539 = parametreler._GetParametre("pro_sektorkodu_baslangic")._GetInt;
			int getInt540 = parametreler._GetParametre("pro_sektorkodu_uzunluk")._GetInt;
			bool getBoolean88 = parametreler._GetParametre("pro_bolgekodu_sabit_kullan")._GetBoolean;
			string getString135 = parametreler._GetParametre("pro_bolgekodu_sabit_deger")._GetString;
			int getInt541 = parametreler._GetParametre("pro_bolgekodu_baslangic")._GetInt;
			int getInt542 = parametreler._GetParametre("pro_bolgekodu_uzunluk")._GetInt;
			bool getBoolean89 = parametreler._GetParametre("pro_ana_projekodu_sabit_kullan")._GetBoolean;
			string getString136 = parametreler._GetParametre("pro_ana_projekodu_sabit_deger")._GetString;
			int getInt543 = parametreler._GetParametre("pro_ana_projekodu_baslangic")._GetInt;
			int getInt544 = parametreler._GetParametre("pro_ana_projekodu_uzunluk")._GetInt;
			bool getBoolean90 = parametreler._GetParametre("pro_aciklama_sabit_kullan")._GetBoolean;
			string getString137 = parametreler._GetParametre("pro_aciklama_sabit_deger")._GetString;
			int getInt545 = parametreler._GetParametre("pro_aciklama_baslangic")._GetInt;
			int getInt546 = parametreler._GetParametre("pro_aciklama_uzunluk")._GetInt;
			bool getBoolean91 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_kullan")._GetBoolean;
			string getString138 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_deger")._GetString;
			int getInt547 = parametreler._GetParametre("pro_muh_kod_artikeli_baslangic")._GetInt;
			int getInt548 = parametreler._GetParametre("pro_muh_kod_artikeli_uzunluk")._GetInt;
			bool getBoolean92 = parametreler._GetParametre("som_isim_sabit_kullan")._GetBoolean;
			string getString139 = parametreler._GetParametre("som_isim_sabit_deger")._GetString;
			int getInt549 = parametreler._GetParametre("som_isim_baslangic")._GetInt;
			int getInt550 = parametreler._GetParametre("som_isim_uzunluk")._GetInt;
			bool getBoolean93 = parametreler._GetParametre("som_MuhArtikeli_sabit_kullan")._GetBoolean;
			string getString140 = parametreler._GetParametre("som_MuhArtikeli_sabit_deger")._GetString;
			int getInt551 = parametreler._GetParametre("som_MuhArtikeli_baslangic")._GetInt;
			int getInt552 = parametreler._GetParametre("som_MuhArtikeli_uzunluk")._GetInt;
			bool getBoolean94 = parametreler._GetParametre("cari_muhartikeli_sabit_kullan")._GetBoolean;
			int getInt553 = parametreler._GetParametre("cari_muhartikeli_baslangic")._GetInt;
			int getInt554 = parametreler._GetParametre("cari_muhartikeli_uzunluk")._GetInt;
			bool getBoolean95 = parametreler._GetParametre("cari_muh_kod_satis_sabit_kullan")._GetBoolean;
			int getInt555 = parametreler._GetParametre("cari_muh_kod_satis_baslangic")._GetInt;
			int getInt556 = parametreler._GetParametre("cari_muh_kod_satis_uzunluk")._GetInt;
			bool getBoolean96 = parametreler._GetParametre("cari_muh_kod_alis_sabit_kullan")._GetBoolean;
			int getInt557 = parametreler._GetParametre("cari_muh_kod_alis_baslangic")._GetInt;
			int getInt558 = parametreler._GetParametre("cari_muh_kod_alis_uzunluk")._GetInt;
			bool getBoolean97 = parametreler._GetParametre("cari_muh_kod1_satis_sabit_kullan")._GetBoolean;
			int getInt559 = parametreler._GetParametre("cari_muh_kod1_satis_baslangic")._GetInt;
			int getInt560 = parametreler._GetParametre("cari_muh_kod1_satis_uzunluk")._GetInt;
			bool getBoolean98 = parametreler._GetParametre("cari_muh_kod1_alis_sabit_kullan")._GetBoolean;
			int getInt561 = parametreler._GetParametre("cari_muh_kod1_alis_baslangic")._GetInt;
			int getInt562 = parametreler._GetParametre("cari_muh_kod1_alis_uzunluk")._GetInt;
			bool getBoolean99 = parametreler._GetParametre("cari_muh_kod2_satis_sabit_kullan")._GetBoolean;
			int getInt563 = parametreler._GetParametre("cari_muh_kod2_satis_baslangic")._GetInt;
			int getInt564 = parametreler._GetParametre("cari_muh_kod2_satis_uzunluk")._GetInt;
			bool getBoolean100 = parametreler._GetParametre("cari_muh_kod2_alis_sabit_kullan")._GetBoolean;
			int getInt565 = parametreler._GetParametre("cari_muh_kod2_alis_baslangic")._GetInt;
			int getInt566 = parametreler._GetParametre("cari_muh_kod2_alis_uzunluk")._GetInt;
			bool getBoolean101 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_kullan")._GetBoolean;
			string getString141 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_deger")._GetString;
			int getInt567 = parametreler._GetParametre("cari_Ana_cari_kodu_baslangic")._GetInt;
			int getInt568 = parametreler._GetParametre("cari_Ana_cari_kodu_uzunluk")._GetInt;
			bool getBoolean102 = parametreler._GetParametre("cari_temsilci_kodu_sabit_kullan")._GetBoolean;
			string getString142 = parametreler._GetParametre("cari_temsilci_kodu_sabit_deger")._GetString;
			int getInt569 = parametreler._GetParametre("cari_temsilci_kodu_baslangic")._GetInt;
			int getInt570 = parametreler._GetParametre("cari_temsilci_kodu_uzunluk")._GetInt;
			bool getBoolean103 = parametreler._GetParametre("cari_grup_kodu_sabit_kullan")._GetBoolean;
			string getString143 = parametreler._GetParametre("cari_grup_kodu_sabit_deger")._GetString;
			int getInt571 = parametreler._GetParametre("cari_grup_kodu_baslangic")._GetInt;
			int getInt572 = parametreler._GetParametre("cari_grup_kodu_uzunluk")._GetInt;
			bool getBoolean104 = parametreler._GetParametre("cari_sektor_kodu_sabit_kullan")._GetBoolean;
			string getString144 = parametreler._GetParametre("cari_sektor_kodu_sabit_deger")._GetString;
			int getInt573 = parametreler._GetParametre("cari_sektor_kodu_baslangic")._GetInt;
			int getInt574 = parametreler._GetParametre("cari_sektor_kodu_uzunluk")._GetInt;
			bool getBoolean105 = parametreler._GetParametre("cari_bolge_kodu_sabit_kullan")._GetBoolean;
			string getString145 = parametreler._GetParametre("cari_bolge_kodu_sabit_deger")._GetString;
			int getInt575 = parametreler._GetParametre("cari_bolge_kodu_baslangic")._GetInt;
			int getInt576 = parametreler._GetParametre("cari_bolge_kodu_uzunluk")._GetInt;
			bool getBoolean106 = parametreler._GetParametre("cari_wwwadresi_sabit_kullan")._GetBoolean;
			string getString146 = parametreler._GetParametre("cari_wwwadresi_sabit_deger")._GetString;
			int getInt577 = parametreler._GetParametre("cari_wwwadresi_baslangic")._GetInt;
			int getInt578 = parametreler._GetParametre("cari_wwwadresi_uzunluk")._GetInt;
			bool getBoolean107 = parametreler._GetParametre("cari_CepTel_sabit_kullan")._GetBoolean;
			string getString147 = parametreler._GetParametre("cari_CepTel_sabit_deger")._GetString;
			int getInt579 = parametreler._GetParametre("cari_CepTel_baslangic")._GetInt;
			int getInt580 = parametreler._GetParametre("cari_CepTel_uzunluk")._GetInt;
			bool getBoolean108 = parametreler._GetParametre("cari_satis_isk_kod_sabit_kullan")._GetBoolean;
			string getString148 = parametreler._GetParametre("cari_satis_isk_kod_sabit_deger")._GetString;
			int getInt581 = parametreler._GetParametre("cari_satis_isk_kod_baslangic")._GetInt;
			int getInt582 = parametreler._GetParametre("cari_satis_isk_kod_uzunluk")._GetInt;
			bool getBoolean109 = parametreler._GetParametre("cari_sicil_no_sabit_kullan")._GetBoolean;
			string getString149 = parametreler._GetParametre("cari_sicil_no_sabit_deger")._GetString;
			int getInt583 = parametreler._GetParametre("cari_sicil_no_baslangic")._GetInt;
			int getInt584 = parametreler._GetParametre("cari_sicil_no_uzunluk")._GetInt;
			bool getBoolean110 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_kullan")._GetBoolean;
			string getString150 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_deger")._GetString;
			int getInt585 = parametreler._GetParametre("cari_VarsayilanGirisDepo_baslangic")._GetInt;
			int getInt586 = parametreler._GetParametre("cari_VarsayilanGirisDepo_uzunluk")._GetInt;
			bool getBoolean111 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_kullan")._GetBoolean;
			string getString151 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_deger")._GetString;
			int getInt587 = parametreler._GetParametre("cari_VarsayilanCikisDepo_baslangic")._GetInt;
			int getInt588 = parametreler._GetParametre("cari_VarsayilanCikisDepo_uzunluk")._GetInt;
			bool getBoolean112 = parametreler._GetParametre("cari_Portal_PW_sabit_kullan")._GetBoolean;
			string getString152 = parametreler._GetParametre("cari_Portal_PW_sabit_deger")._GetString;
			int getInt589 = parametreler._GetParametre("cari_Portal_PW_baslangic")._GetInt;
			int getInt590 = parametreler._GetParametre("cari_Portal_PW_uzunluk")._GetInt;
			bool getBoolean113 = parametreler._GetParametre("cari_Portal_Enabled")._GetBoolean;
			string getString153 = parametreler._GetParametre("XslDosyaAdi")._GetString;
			if (getString153 != "")
			{
				string inputfile = DosyaAdi;
				DosyaAdi = Path.GetTempFileName();
				try
				{
					XslTransform xslTransform = new XslTransform();
					xslTransform.Load(getString153);
					xslTransform.Transform(inputfile, DosyaAdi);
				}
				catch
				{
				}
			}
			_uygulanacak_kriterler = new List<GenelEvrakKriter>();
			string[] array = getString.Split(',');
			foreach (string text in array)
			{
				if (text != "")
				{
					GenelEvrakKriter genelEvrakKriter = new GenelEvrakKriter();
					Parametreler parametreler2 = ParametrelerDefault.GenelAktarimKriter(text);
					ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler2, "GenelAktarim", "", "Kriter", text);
					genelEvrakKriter.arama_alan1_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_baglac")._GetInt;
					genelEvrakKriter.arama_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan2_baglac")._GetInt;
					genelEvrakKriter.arama_alan1_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_alan2_baglac")._GetInt;
					string getString154 = parametreler2._GetParametre("arama_alan1")._GetString;
					string getString155 = parametreler2._GetParametre("arama_alan2")._GetString;
					string getString156 = parametreler2._GetParametre("degistirilecek_alanlar")._GetString;
					getString154 = getString154.Replace("Fora_Mikro", "Fora.App.Win.Mikro");
					getString155 = getString155.Replace("Fora_Mikro", "Fora.App.Win.Mikro");
					getString156 = getString156.Replace("Fora_Mikro", "Fora.App.Win.Mikro");
					if (getString154 != "")
					{
						genelEvrakKriter.arama_alan1 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString154);
					}
					else
					{
						genelEvrakKriter.arama_alan1 = new List<AlanOperatorDeger>();
					}
					if (getString155 != "")
					{
						genelEvrakKriter.arama_alan2 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString155);
					}
					else
					{
						genelEvrakKriter.arama_alan2 = new List<AlanOperatorDeger>();
					}
					if (getString156 != "")
					{
						genelEvrakKriter.degistirilecek_alanlar = JSON.Instance.ToObject<List<AlanIslemTipiDeger>>(getString156);
					}
					else
					{
						genelEvrakKriter.degistirilecek_alanlar = new List<AlanIslemTipiDeger>();
					}
					_uygulanacak_kriterler.Add(genelEvrakKriter);
				}
			}
			List<Cari> list = new List<Cari>();
			BindingList<Stok> bindingList2 = new BindingList<Stok>();
			List<Hizmet> list2 = new List<Hizmet>();
			List<Proje> list3 = new List<Proje>();
			List<SorumlulukMerkezi> list4 = new List<SorumlulukMerkezi>();
			using (StreamReader streamReader = new StreamReader(DosyaAdi, Encoding.GetEncoding("windows-1254")))
			{
				num4 = 0;
				string text2;
				while ((text2 = streamReader.ReadLine()) != null)
				{
					bool flag2 = true;
					bool flag3 = false;
					if (getBoolean && !text2.StartsWith(getString2))
					{
						flag2 = false;
					}
					if (getBoolean41)
					{
						flag2 = false;
						if (text2.StartsWith(getString53))
						{
							flag2 = true;
						}
					}
					if (getBoolean2 && num4 < getInt)
					{
						flag2 = false;
						flag3 = true;
					}
					if (text2.Trim() == "")
					{
						flag2 = false;
					}
					bool flag4 = false;
					if (getBoolean41)
					{
						if (text2.StartsWith(getString52) && !flag3)
						{
							flag4 = true;
						}
					}
					else if (flag2)
					{
						flag4 = true;
					}
					string[] array2 = null;
					if (getBoolean3)
					{
						array2 = text2.Split(getString3.ToCharArray()[0]);
					}
					if (flag4)
					{
						enum_GenelEvrakTipleri enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.SatisFaturasi;
						if (getBoolean4)
						{
							enum_GenelEvrakTipleri = (enum_GenelEvrakTipleri)getInt27;
						}
						else
						{
							num2 = getInt28;
							num3 = getInt29;
							string text3 = "";
							if (num2 != 0 || num3 != 0)
							{
								text3 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							array = getString8.Split(',');
							foreach (string text4 in array)
							{
								if (text3 == text4)
								{
									enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.AlinanSiparis;
									break;
								}
							}
							array = getString6.Split(',');
							foreach (string text5 in array)
							{
								if (text3 == text5)
								{
									enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.AlisFaturasi;
									break;
								}
							}
							array = getString7.Split(',');
							foreach (string text6 in array)
							{
								if (text3 == text6)
								{
									enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.AlisIrsaliyesi;
									break;
								}
							}
							array = getString4.Split(',');
							foreach (string text7 in array)
							{
								if (text3 == text7)
								{
									enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.SatisFaturasi;
									break;
								}
							}
							array = getString5.Split(',');
							foreach (string text8 in array)
							{
								if (text3 == text8)
								{
									enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.SatisIrsaliyesi;
									break;
								}
							}
							array = getString9.Split(',');
							foreach (string text9 in array)
							{
								if (text3 == text9)
								{
									enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.DepolarArasiSevk;
									break;
								}
							}
						}
						string text10 = "";
						if (getInt49 != 0 || getInt50 != 0)
						{
							text10 = ((!getBoolean3) ? text2.Substring(getInt49 - 1, getInt50).Trim() : array2[getInt49 - 1]);
						}
						if (text10 == "" && (getInt51 != 0 || getInt52 != 0))
						{
							text10 = ((!getBoolean3) ? text2.Substring(getInt51 - 1, getInt52).Trim() : array2[getInt51 - 1]);
						}
						string text11 = "";
						if (getInt45 != 0 || getInt46 != 0)
						{
							text11 = ((!getBoolean3) ? text2.Substring(getInt45 - 1, getInt46).Trim() : array2[getInt45 - 1]);
						}
						if (text11.Length > 50)
						{
							text11 = text11.Substring(0, 50);
						}
						string text12 = "";
						if (getInt47 != 0 || getInt48 != 0)
						{
							text12 = ((!getBoolean3) ? text2.Substring(getInt47 - 1, getInt48).Trim() : array2[getInt47 - 1]);
						}
						if (text12.Length > 50)
						{
							text12 = text12.Substring(0, 50);
						}
						string text13 = "";
						if (getInt55 != 0 || getInt56 != 0)
						{
							text13 = ((!getBoolean3) ? text2.Substring(getInt55 - 1, getInt56).Trim() : array2[getInt55 - 1]);
						}
						if (text13.Length > 30)
						{
							text13 = text13.Substring(0, 30);
						}
						string text14 = "";
						if (getInt71 != 0 || getInt72 != 0)
						{
							text14 = ((!getBoolean3) ? text2.Substring(getInt71 - 1, getInt72).Trim() : array2[getInt71 - 1]);
						}
						if (text14.Length > 80)
						{
							text14 = text14.Substring(0, 80);
						}
						string text15 = "";
						if (getBoolean43)
						{
							text15 = ((enum_GenelEvrakTipleri != enum_GenelEvrakTipleri.AlisFaturasi && enum_GenelEvrakTipleri != enum_GenelEvrakTipleri.AlisIrsaliyesi) ? getString54 : getString55);
						}
						if (getBoolean10)
						{
							text15 += getString16;
						}
						else
						{
							num2 = getInt43;
							num3 = getInt44;
							string text16 = "";
							if (num2 != 0 || num3 != 0)
							{
								text16 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							text15 += text16;
							if (text16 == "")
							{
								int num5 = getInt472;
								int num6 = getInt473;
								string text17 = "";
								if (num5 != 0 || num6 != 0)
								{
									text17 = ((!getBoolean3) ? text2.Substring(num5 - 1, num6).Trim() : array2[num5 - 1]);
								}
								text15 += text17;
							}
						}
						string[] array3 = getString17.Split(',');
						bool flag5 = false;
						array = array3;
						foreach (string text18 in array)
						{
							if (flag5)
							{
								break;
							}
							switch (text18)
							{
							case "1":
								if (text15 != "" && CariData.GetCariByCariKod(Db.Connection, text15, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "2":
								if (text10 != "" && CariData.GetCariByVergiTcKimlikNo(Db.Connection, text10, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "3":
								if (text14 != "" && CariData.GetCariByEMail(Db.Connection, text14, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "4":
								if (text13 != "" && CariData.GetCariByBankaHesapNo(Db.Connection, text13, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "5":
								if (text11 != "" && CariData.GetCariByCariUnvan(Db.Connection, text11, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							}
						}
						if (!flag5)
						{
							foreach (Cari item in list)
							{
								if (text15 == item.cari_kod)
								{
									flag5 = true;
									break;
								}
							}
						}
						if (!flag5)
						{
							Cari cari = new Cari();
							cari.CariAdresleri = new List<CariAdres>();
							cari.cari_kod = text15;
							cari.cari_vdaire_no = text10;
							cari.cari_unvan1 = text11;
							cari.cari_unvan2 = text12;
							cari.cari_banka_hesapno1 = text13;
							cari.cari_Email = text14;
							string text19 = "";
							if (getInt53 != 0 || getInt54 != 0)
							{
								text19 = ((!getBoolean3) ? text2.Substring(getInt53 - 1, getInt54).Trim() : array2[getInt53 - 1]);
							}
							if (text19.Length > 20)
							{
								text19 = text19.Substring(0, 20);
							}
							cari.cari_vdaire_adi = text19;
							string text20 = "";
							if (getBoolean94)
							{
								text20 = getString80;
							}
							else
							{
								if (getInt553 != 0 || getInt554 != 0)
								{
									text20 = ((!getBoolean3) ? text2.Substring(getInt553 - 1, getInt554).Trim() : array2[getInt553 - 1]);
								}
								if (text20.Length > 10)
								{
									text20 = text20.Substring(0, 10);
								}
							}
							cari.cari_muhartikeli = text20;
							string text21 = "";
							if (getBoolean101)
							{
								text21 = getString141;
							}
							else
							{
								if (getInt567 != 0 || getInt568 != 0)
								{
									text21 = ((!getBoolean3) ? text2.Substring(getInt567 - 1, getInt568).Trim() : array2[getInt567 - 1]);
								}
								if (text21.Length > 25)
								{
									text21 = text21.Substring(0, 25);
								}
							}
							cari.cari_Ana_cari_kodu = text21;
							string text22 = "";
							if (getBoolean102)
							{
								text22 = getString142;
							}
							else
							{
								if (getInt569 != 0 || getInt570 != 0)
								{
									text22 = ((!getBoolean3) ? text2.Substring(getInt569 - 1, getInt570).Trim() : array2[getInt569 - 1]);
								}
								if (text22.Length > 25)
								{
									text22 = text22.Substring(0, 25);
								}
							}
							cari.cari_temsilci_kodu = text22;
							string text23 = "";
							if (getBoolean103)
							{
								text23 = getString143;
							}
							else
							{
								if (getInt571 != 0 || getInt572 != 0)
								{
									text23 = ((!getBoolean3) ? text2.Substring(getInt571 - 1, getInt572).Trim() : array2[getInt571 - 1]);
								}
								if (text23.Length > 25)
								{
									text23 = text23.Substring(0, 25);
								}
							}
							cari.cari_grup_kodu = text23;
							string text24 = "";
							if (getBoolean104)
							{
								text24 = getString144;
							}
							else
							{
								if (getInt573 != 0 || getInt574 != 0)
								{
									text24 = ((!getBoolean3) ? text2.Substring(getInt573 - 1, getInt574).Trim() : array2[getInt573 - 1]);
								}
								if (text24.Length > 25)
								{
									text24 = text24.Substring(0, 25);
								}
							}
							cari.cari_sektor_kodu = text24;
							string text25 = "";
							if (getBoolean105)
							{
								text25 = getString145;
							}
							else
							{
								if (getInt575 != 0 || getInt576 != 0)
								{
									text25 = ((!getBoolean3) ? text2.Substring(getInt575 - 1, getInt576).Trim() : array2[getInt575 - 1]);
								}
								if (text25.Length > 25)
								{
									text25 = text25.Substring(0, 25);
								}
							}
							cari.cari_bolge_kodu = text25;
							string text26 = "";
							if (getBoolean106)
							{
								text26 = getString146;
							}
							else
							{
								if (getInt577 != 0 || getInt578 != 0)
								{
									text26 = ((!getBoolean3) ? text2.Substring(getInt577 - 1, getInt578).Trim() : array2[getInt577 - 1]);
								}
								if (text26.Length > 30)
								{
									text26 = text26.Substring(0, 30);
								}
							}
							cari.cari_wwwadresi = text26;
							string text27 = "";
							if (getBoolean107)
							{
								text27 = getString147;
							}
							else
							{
								if (getInt579 != 0 || getInt580 != 0)
								{
									text27 = ((!getBoolean3) ? text2.Substring(getInt579 - 1, getInt580).Trim() : array2[getInt579 - 1]);
								}
								if (text27.Length > 20)
								{
									text27 = text27.Substring(0, 20);
								}
							}
							cari.cari_CepTel = text27;
							string text28 = "";
							if (getBoolean108)
							{
								text28 = getString148;
							}
							else
							{
								if (getInt581 != 0 || getInt582 != 0)
								{
									text28 = ((!getBoolean3) ? text2.Substring(getInt581 - 1, getInt582).Trim() : array2[getInt581 - 1]);
								}
								if (text28.Length > 4)
								{
									text28 = text28.Substring(0, 4);
								}
							}
							cari.cari_satis_isk_kod = text28;
							string text29 = "";
							if (getBoolean109)
							{
								text29 = getString149;
							}
							else
							{
								if (getInt583 != 0 || getInt584 != 0)
								{
									text29 = ((!getBoolean3) ? text2.Substring(getInt583 - 1, getInt584).Trim() : array2[getInt583 - 1]);
								}
								if (text29.Length > 15)
								{
									text29 = text29.Substring(0, 15);
								}
							}
							cari.cari_sicil_no = text29;
							string text30 = "";
							if (getBoolean110)
							{
								text30 = getString150;
							}
							else
							{
								if (getInt585 != 0 || getInt586 != 0)
								{
									text30 = ((!getBoolean3) ? text2.Substring(getInt585 - 1, getInt586).Trim() : array2[getInt585 - 1]);
								}
								if (text30.Length > 25)
								{
									text30 = text30.Substring(0, 25);
								}
							}
							int result = 0;
							int.TryParse(text30, out result);
							cari.cari_VarsayilanGirisDepo = result;
							string text31 = "";
							if (getBoolean111)
							{
								text31 = getString151;
							}
							else
							{
								if (getInt587 != 0 || getInt588 != 0)
								{
									text31 = ((!getBoolean3) ? text2.Substring(getInt587 - 1, getInt588).Trim() : array2[getInt587 - 1]);
								}
								if (text31.Length > 25)
								{
									text31 = text31.Substring(0, 25);
								}
							}
							int result2 = 0;
							int.TryParse(text31, out result2);
							cari.cari_VarsayilanCikisDepo = result2;
							string text32 = "";
							if (getBoolean112)
							{
								text32 = getString152;
							}
							else
							{
								if (getInt589 != 0 || getInt590 != 0)
								{
									text32 = ((!getBoolean3) ? text2.Substring(getInt589 - 1, getInt590).Trim() : array2[getInt589 - 1]);
								}
								if (text32.Length > 127)
								{
									text32 = text32.Substring(0, 127);
								}
							}
							cari.cari_Portal_PW = text32;
							if (enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.AlisFaturasi || enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.AlisIrsaliyesi)
							{
								string text33 = "";
								if (getBoolean96)
								{
									text33 = getString81;
								}
								else
								{
									if (getInt557 != 0 || getInt558 != 0)
									{
										text33 = ((!getBoolean3) ? text2.Substring(getInt557 - 1, getInt558).Trim() : array2[getInt557 - 1]);
									}
									if (text33.Length > 40)
									{
										text33 = text33.Substring(0, 40);
									}
								}
								cari.cari_muh_kod = text33;
								string text34 = "";
								if (getBoolean98)
								{
									text34 = getString82;
								}
								else
								{
									if (getInt561 != 0 || getInt562 != 0)
									{
										text34 = ((!getBoolean3) ? text2.Substring(getInt561 - 1, getInt562).Trim() : array2[getInt561 - 1]);
									}
									if (text34.Length > 40)
									{
										text34 = text34.Substring(0, 40);
									}
								}
								cari.cari_muh_kod1 = text34;
								string text35 = "";
								if (getBoolean100)
								{
									text35 = getString83;
								}
								else
								{
									if (getInt565 != 0 || getInt566 != 0)
									{
										text35 = ((!getBoolean3) ? text2.Substring(getInt565 - 1, getInt566).Trim() : array2[getInt565 - 1]);
									}
									if (text35.Length > 40)
									{
										text35 = text35.Substring(0, 40);
									}
								}
								cari.cari_muh_kod2 = text35;
							}
							else
							{
								string text36 = "";
								if (getBoolean95)
								{
									text36 = getString77;
								}
								else
								{
									if (getInt555 != 0 || getInt556 != 0)
									{
										text36 = ((!getBoolean3) ? text2.Substring(getInt555 - 1, getInt556).Trim() : array2[getInt555 - 1]);
									}
									if (text36.Length > 40)
									{
										text36 = text36.Substring(0, 40);
									}
								}
								cari.cari_muh_kod = text36;
								string text37 = "";
								if (getBoolean97)
								{
									text37 = getString78;
								}
								else
								{
									if (getInt559 != 0 || getInt560 != 0)
									{
										text37 = ((!getBoolean3) ? text2.Substring(getInt559 - 1, getInt560).Trim() : array2[getInt559 - 1]);
									}
									if (text37.Length > 40)
									{
										text37 = text37.Substring(0, 40);
									}
								}
								cari.cari_muh_kod1 = text37;
								string text38 = "";
								if (getBoolean99)
								{
									text38 = getString79;
								}
								else
								{
									if (getInt563 != 0 || getInt564 != 0)
									{
										text38 = ((!getBoolean3) ? text2.Substring(getInt563 - 1, getInt564).Trim() : array2[getInt563 - 1]);
									}
									if (text38.Length > 40)
									{
										text38 = text38.Substring(0, 40);
									}
								}
								cari.cari_muh_kod2 = text38;
							}
							cari.cari_doviz_cinsi = getInt467;
							cari.cari_doviz_cinsi1 = getInt468;
							cari.cari_doviz_cinsi2 = getInt469;
							cari.cari_Portal_Enabled = getBoolean113;
							string text39 = "";
							string text40 = "";
							string text41 = "";
							if (getBoolean28)
							{
								text39 = getString37;
							}
							else
							{
								num2 = getInt106;
								num3 = getInt107;
								string text42 = "";
								if (num2 != 0 || num3 != 0)
								{
									text42 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text39 = text42;
							}
							if (getBoolean29)
							{
								text40 = getString38;
							}
							else
							{
								num2 = getInt108;
								num3 = getInt109;
								string text43 = "";
								if (num2 != 0 || num3 != 0)
								{
									text43 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text40 = text43;
							}
							if (getBoolean30)
							{
								text41 = getString39;
							}
							else
							{
								num2 = getInt110;
								num3 = getInt111;
								string text44 = "";
								if (num2 != 0 || num3 != 0)
								{
									text44 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text41 = text44;
							}
							cari.cari_special1 = text39;
							cari.cari_special2 = text40;
							cari.cari_special3 = text41;
							string text45 = "";
							if (getInt57 != 0 || getInt58 != 0)
							{
								text45 = ((!getBoolean3) ? text2.Substring(getInt57 - 1, getInt58).Trim() : array2[getInt57 - 1]);
							}
							if (text45.Length > 50)
							{
								text45 = text45.Substring(0, 50);
							}
							if (text45 != "")
							{
								CariAdres cariAdres = new CariAdres();
								cariAdres.adr_cadde = text45;
								string text46 = "";
								if (getInt59 != 0 || getInt60 != 0)
								{
									text46 = ((!getBoolean3) ? text2.Substring(getInt59 - 1, getInt60).Trim() : array2[getInt59 - 1]);
								}
								if (text46.Length > 50)
								{
									text46 = text46.Substring(0, 50);
								}
								cariAdres.adr_sokak = text46;
								string text47 = "";
								if (getInt61 != 0 || getInt62 != 0)
								{
									text47 = ((!getBoolean3) ? text2.Substring(getInt61 - 1, getInt62).Trim() : array2[getInt61 - 1]);
								}
								if (text47.Length > 15)
								{
									text47 = text47.Substring(0, 15);
								}
								cariAdres.adr_ilce = text47;
								string text48 = "";
								if (getInt63 != 0 || getInt64 != 0)
								{
									text48 = ((!getBoolean3) ? text2.Substring(getInt63 - 1, getInt64).Trim() : array2[getInt63 - 1]);
								}
								if (text48.Length > 15)
								{
									text48 = text48.Substring(0, 15);
								}
								if (getBoolean56)
								{
									switch (int.Parse(text48))
									{
									case 1:
										text48 = "ADANA";
										break;
									case 2:
										text48 = "ADIYAMAN";
										break;
									case 3:
										text48 = "AFYONKARAHİSAR";
										break;
									case 4:
										text48 = "AĞRI";
										break;
									case 5:
										text48 = "AMASYA";
										break;
									case 6:
										text48 = "ANKARA";
										break;
									case 7:
										text48 = "ANTALYA";
										break;
									case 8:
										text48 = "ARTVİN";
										break;
									case 9:
										text48 = "AYDIN";
										break;
									case 10:
										text48 = "BALIKESİR";
										break;
									case 11:
										text48 = "BİLECİK";
										break;
									case 12:
										text48 = "BİNGÖL";
										break;
									case 13:
										text48 = "BİTLİS";
										break;
									case 14:
										text48 = "BOLU";
										break;
									case 15:
										text48 = "BURDUR";
										break;
									case 16:
										text48 = "BURSA";
										break;
									case 17:
										text48 = "ÇANAKKALE";
										break;
									case 18:
										text48 = "ÇANKIRI";
										break;
									case 19:
										text48 = "ÇORUM";
										break;
									case 20:
										text48 = "DENİZLİ";
										break;
									case 21:
										text48 = "DİYARBAKIR";
										break;
									case 22:
										text48 = "EDİRNE";
										break;
									case 23:
										text48 = "ELAZIĞ";
										break;
									case 24:
										text48 = "ERZİNCAN";
										break;
									case 25:
										text48 = "ERZURUM";
										break;
									case 26:
										text48 = "ESKİŞEHİR";
										break;
									case 27:
										text48 = "GAZİANTEP";
										break;
									case 28:
										text48 = "GİRESUN";
										break;
									case 29:
										text48 = "GÜMÜŞHANE";
										break;
									case 30:
										text48 = "HAKKARİ";
										break;
									case 31:
										text48 = "HATAY";
										break;
									case 32:
										text48 = "ISPARTA";
										break;
									case 33:
										text48 = "MERSİN";
										break;
									case 34:
										text48 = "İSTANBUL";
										break;
									case 35:
										text48 = "İZMİR";
										break;
									case 36:
										text48 = "KARS";
										break;
									case 37:
										text48 = "KASTAMONU";
										break;
									case 38:
										text48 = "KAYSERİ";
										break;
									case 39:
										text48 = "KIRKLARELİ";
										break;
									case 40:
										text48 = "KIRŞEHİR";
										break;
									case 41:
										text48 = "KOCAELİ";
										break;
									case 42:
										text48 = "KONYA";
										break;
									case 43:
										text48 = "KÜTAHYA";
										break;
									case 44:
										text48 = "MALATYA";
										break;
									case 45:
										text48 = "MANİSA";
										break;
									case 46:
										text48 = "KAHRAMANMARAŞ";
										break;
									case 47:
										text48 = "MARDİN";
										break;
									case 48:
										text48 = "MUĞLA";
										break;
									case 49:
										text48 = "MUŞ";
										break;
									case 50:
										text48 = "NEVŞEHİR";
										break;
									case 51:
										text48 = "NİĞDE";
										break;
									case 52:
										text48 = "ORDU";
										break;
									case 53:
										text48 = "RİZE";
										break;
									case 54:
										text48 = "SAKARYA";
										break;
									case 55:
										text48 = "SAMSUN";
										break;
									case 56:
										text48 = "SİİRT";
										break;
									case 57:
										text48 = "SİNOP";
										break;
									case 58:
										text48 = "SİVAS";
										break;
									case 59:
										text48 = "TEKİRDAĞ";
										break;
									case 60:
										text48 = "TOKAT";
										break;
									case 61:
										text48 = "TRABZON";
										break;
									case 62:
										text48 = "TUNCELİ";
										break;
									case 63:
										text48 = "ŞANLIURFA";
										break;
									case 64:
										text48 = "UŞAK";
										break;
									case 65:
										text48 = "VAN";
										break;
									case 66:
										text48 = "YOZGAT";
										break;
									case 67:
										text48 = "ZONGULDAK";
										break;
									case 68:
										text48 = "AKSARAY";
										break;
									case 69:
										text48 = "BAYBURT";
										break;
									case 70:
										text48 = "KARAMAN";
										break;
									case 71:
										text48 = "KIRIKKALE";
										break;
									case 72:
										text48 = "BATMAN";
										break;
									case 73:
										text48 = "ŞIRNAK";
										break;
									case 74:
										text48 = "BARTIN";
										break;
									case 75:
										text48 = "ARDAHAN";
										break;
									case 76:
										text48 = "IĞDIR";
										break;
									case 77:
										text48 = "YALOVA";
										break;
									case 78:
										text48 = "KARABÜK";
										break;
									case 79:
										text48 = "KİLİS";
										break;
									case 80:
										text48 = "OSMANİYE";
										break;
									case 81:
										text48 = "DÜZCE";
										break;
									}
								}
								cariAdres.adr_il = text48;
								string text49 = "";
								if (getInt65 != 0 || getInt66 != 0)
								{
									text49 = ((!getBoolean3) ? text2.Substring(getInt65 - 1, getInt66).Trim() : array2[getInt65 - 1]);
								}
								if (text49.Length > 15)
								{
									text49 = text49.Substring(0, 15);
								}
								cariAdres.adr_ulke = text49;
								string text50 = "";
								if (getInt67 != 0 || getInt68 != 0)
								{
									text50 = ((!getBoolean3) ? text2.Substring(getInt67 - 1, getInt68).Trim() : array2[getInt67 - 1]);
								}
								if (text50.Length > 8)
								{
									text50 = text50.Substring(0, 8);
								}
								cariAdres.adr_posta_kodu = text50;
								string text51 = "";
								if (getInt69 != 0 || getInt70 != 0)
								{
									text51 = ((!getBoolean3) ? text2.Substring(getInt69 - 1, getInt70).Trim() : array2[getInt69 - 1]);
								}
								if (text51.Length > 10)
								{
									text51 = text51.Substring(0, 10);
								}
								cariAdres.adr_tel_no1 = text51;
								cariAdres.adr_special1 = text39;
								cariAdres.adr_special2 = text40;
								cariAdres.adr_special3 = text41;
								cariAdres.adr_cari_kod = cari.cari_kod;
								cariAdres.adr_adres_no = 1;
								cari.CariAdresleri.Add(cariAdres);
							}
							list.Add(cari);
						}
					}
					if (flag2)
					{
						string text52 = "";
						enum_SatirCinsi enum_SatirCinsi = enum_SatirCinsi.Stok;
						if (getBoolean7)
						{
							text52 = getString14;
						}
						else
						{
							num2 = getInt36;
							num3 = getInt37;
							string text53 = "";
							if (num2 != 0 || num3 != 0)
							{
								text53 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							text52 = text53;
						}
						if (getBoolean6)
						{
							enum_SatirCinsi = (enum_SatirCinsi)getInt33;
						}
						else
						{
							num2 = getInt34;
							num3 = getInt35;
							string text54 = "";
							if (num2 != 0 || num3 != 0)
							{
								text54 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							if (text54 == getString12)
							{
								enum_SatirCinsi = enum_SatirCinsi.Stok;
							}
							if (text54 == getString13)
							{
								enum_SatirCinsi = enum_SatirCinsi.Hizmet;
							}
						}
						if (getBoolean42)
						{
							Stok stok = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text52, enum_toptan_perakende.Toptan);
							if (!(stok.sto_kod == "") && stok.sto_kod != null)
							{
								enum_SatirCinsi = enum_SatirCinsi.Stok;
							}
							else
							{
								Hizmet hizmet = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text52);
								if (!(hizmet.hiz_kod == "") && hizmet.hiz_kod != null)
								{
									enum_SatirCinsi = enum_SatirCinsi.Hizmet;
								}
							}
						}
						bool flag6 = false;
						if (enum_SatirCinsi == enum_SatirCinsi.Stok)
						{
							Stok stok2 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text52, enum_toptan_perakende.Toptan);
							if (stok2.sto_kod == "" || stok2.sto_kod == null)
							{
								Barkod barkodBilgisi = BarkodData.GetBarkodBilgisi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text52);
								if (barkodBilgisi.bar_stokkodu != "")
								{
									stok2 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, barkodBilgisi.bar_stokkodu, enum_toptan_perakende.Toptan);
									if (stok2.sto_kod != "" && stok2.sto_kod != null)
									{
										text52 = stok2.sto_kod;
									}
								}
							}
							if (!(stok2.sto_kod == "") && stok2.sto_kod != null)
							{
								flag6 = true;
							}
						}
						else
						{
							Hizmet hizmet2 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text52);
							if (!(hizmet2.hiz_kod == "") && hizmet2.hiz_kod != null)
							{
								flag6 = true;
							}
						}
						if (!flag6)
						{
							string text55 = "";
							string text56 = "";
							string text57 = "";
							if (getBoolean28)
							{
								text55 = getString37;
							}
							else
							{
								num2 = getInt106;
								num3 = getInt107;
								string text58 = "";
								if (num2 != 0 || num3 != 0)
								{
									text58 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text55 = text58;
							}
							if (getBoolean29)
							{
								text56 = getString38;
							}
							else
							{
								num2 = getInt108;
								num3 = getInt109;
								string text59 = "";
								if (num2 != 0 || num3 != 0)
								{
									text59 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text56 = text59;
							}
							if (getBoolean30)
							{
								text57 = getString39;
							}
							else
							{
								num2 = getInt110;
								num3 = getInt111;
								string text60 = "";
								if (num2 != 0 || num3 != 0)
								{
									text60 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text57 = text60;
							}
							if (enum_SatirCinsi == enum_SatirCinsi.Stok)
							{
								bool flag7 = false;
								foreach (Stok item2 in bindingList2)
								{
									if (text52 == item2.sto_kod)
									{
										flag7 = true;
										break;
									}
								}
								if (!flag7)
								{
									Stok stok3 = new Stok();
									stok3.sto_kod = text52;
									stok3.sto_special1 = text55;
									stok3.sto_special2 = text56;
									stok3.sto_special3 = text57;
									int num7 = 4;
									switch (getInt143)
									{
									case 0:
										num7 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, stok3.sto_kod, enum_toptan_perakende.Toptan).sto_toptan_vergi_yeni;
										break;
									case 1:
										num7 = getInt144;
										break;
									case 2:
									{
										num2 = getInt145;
										num3 = getInt146;
										string text61 = "0";
										if (num2 != 0 || num3 != 0)
										{
											text61 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
										}
										double result3 = 0.0;
										double.TryParse(text61.Replace(".", ","), out result3);
										num = getInt187;
										num2 = getInt188;
										num3 = getInt189;
										if (num != 0)
										{
											text61 = "";
											if (num2 != 0 || num3 != 0)
											{
												text61 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
											}
											double result4 = 0.0;
											double.TryParse(text61.Replace(".", ","), out result4);
											switch (num)
											{
											case 1:
												result3 += result4;
												break;
											case 2:
												result3 -= result4;
												break;
											case 3:
												result3 /= result4;
												break;
											case 4:
												result3 *= result4;
												break;
											}
										}
										num = getInt190;
										num2 = getInt191;
										num3 = getInt192;
										if (num != 0)
										{
											text61 = "";
											if (num2 != 0 || num3 != 0)
											{
												text61 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
											}
											double result5 = 0.0;
											double.TryParse(text61.Replace(".", ","), out result5);
											switch (num)
											{
											case 1:
												result3 += result5;
												break;
											case 2:
												result3 -= result5;
												break;
											case 3:
												result3 /= result5;
												break;
											case 4:
												result3 *= result5;
												break;
											}
										}
										num7 = GenelUtility.GetKdvPntr((int)result3);
										break;
									}
									}
									stok3.sto_toptan_vergi_yeni = num7;
									stok3.sto_perakende_vergi_yeni = num7;
									double num8 = 0.0;
									if (getInt133 != 0 || getInt134 != 0)
									{
										if (getBoolean3)
										{
											double result6 = 0.0;
											double.TryParse(array2[getInt133 - 1].Replace(".", ","), out result6);
											num8 = result6;
										}
										else
										{
											double result7 = 0.0;
											double.TryParse(text2.Substring(getInt133 - 1, getInt134).Trim().Replace(".", ","), out result7);
											num8 = result7;
										}
									}
									num = getInt193;
									num2 = getInt194;
									num3 = getInt195;
									if (num != 0)
									{
										string text62 = "";
										if (num2 != 0 || num3 != 0)
										{
											text62 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
										}
										double result8 = 0.0;
										double.TryParse(text62.Replace(".", ","), out result8);
										switch (num)
										{
										case 1:
											num8 += result8;
											break;
										case 2:
											num8 -= result8;
											break;
										case 3:
											num8 /= result8;
											break;
										case 4:
											num8 *= result8;
											break;
										}
									}
									num = getInt196;
									num2 = getInt197;
									num3 = getInt198;
									if (num != 0)
									{
										string text63 = "";
										if (num2 != 0 || num3 != 0)
										{
											text63 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
										}
										double result9 = 0.0;
										double.TryParse(text63.Replace(".", ","), out result9);
										switch (num)
										{
										case 1:
											num8 += result9;
											break;
										case 2:
											num8 -= result9;
											break;
										case 3:
											num8 /= result9;
											break;
										case 4:
											num8 *= result9;
											break;
										}
									}
									stok3.sto_otvtutar = num8;
									string text64 = "";
									if (getBoolean71)
									{
										text64 = getString84;
									}
									else
									{
										if (getInt474 != 0 || getInt475 != 0)
										{
											text64 = ((!getBoolean3) ? text2.Substring(getInt474 - 1, getInt475).Trim() : array2[getInt474 - 1]);
										}
										if (text64.Length > 50)
										{
											text64 = text64.Substring(0, 50);
										}
									}
									stok3.sto_isim = text64;
									string text65 = "";
									if (getBoolean72)
									{
										text65 = getString85;
									}
									else
									{
										if (getInt476 != 0 || getInt477 != 0)
										{
											text65 = ((!getBoolean3) ? text2.Substring(getInt476 - 1, getInt477).Trim() : array2[getInt476 - 1]);
										}
										if (text65.Length > 25)
										{
											text65 = text65.Substring(0, 25);
										}
									}
									stok3.sto_kisa_ismi = text65;
									string text66 = "";
									if (getBoolean73)
									{
										text66 = getString86;
									}
									else
									{
										if (getInt478 != 0 || getInt479 != 0)
										{
											text66 = ((!getBoolean3) ? text2.Substring(getInt478 - 1, getInt479).Trim() : array2[getInt478 - 1]);
										}
										if (text66.Length > 50)
										{
											text66 = text66.Substring(0, 50);
										}
									}
									stok3.sto_yabanci_isim = text66;
									string text67 = "";
									if (getBoolean74)
									{
										text67 = getString87;
									}
									else
									{
										if (getInt480 != 0 || getInt481 != 0)
										{
											text67 = ((!getBoolean3) ? text2.Substring(getInt480 - 1, getInt481).Trim() : array2[getInt480 - 1]);
										}
										if (text67.Length > 10)
										{
											text67 = text67.Substring(0, 10);
										}
									}
									stok3.sto_birim1_ad = text67;
									string s = "";
									if (getBoolean75)
									{
										s = getString88;
									}
									else if (getInt482 != 0 || getInt483 != 0)
									{
										s = ((!getBoolean3) ? text2.Substring(getInt482 - 1, getInt483).Trim() : array2[getInt482 - 1]);
									}
									float result10 = 1f;
									float.TryParse(s, out result10);
									stok3.sto_birim1_katsayi = result10;
									stok3.sto_otvuygulama = getInt484;
									stok3.sto_muh_kod = getString89;
									stok3.sto_muh_Iade_kod = getString90;
									stok3.sto_muh_sat_muh_kod = getString91;
									stok3.sto_muh_satIadmuhkod = getString92;
									stok3.sto_muh_sat_isk_kod = getString93;
									stok3.sto_muh_aIiskmuhkod = getString94;
									stok3.sto_muh_satmalmuhkod = getString95;
									stok3.sto_yurtdisi_satmuhk = getString96;
									stok3.sto_ilavemasmuhkod = getString97;
									stok3.sto_yatirimtesmuhkod = getString98;
									stok3.sto_depsatmuhkod = getString99;
									stok3.sto_depsatmalmuhkod = getString100;
									stok3.sto_bagortsatmuhkod = getString101;
									stok3.sto_bagortsatIadmuhkod = getString102;
									stok3.sto_bagortsatIskmuhkod = getString103;
									stok3.sto_satfiyfarkmuhkod = getString104;
									stok3.sto_yurtdisisatmalmuhkod = getString105;
									stok3.sto_bagortsatmalmuhkod = getString106;
									stok3.sto_muhgrup_kodu = getString107;
									stok3.sto_anagrup_kod = getString108;
									stok3.sto_altgrup_kod = getString109;
									bindingList2.Add(stok3);
								}
							}
							else
							{
								bool flag8 = false;
								foreach (Hizmet item3 in list2)
								{
									if (text52 == item3.hiz_kod)
									{
										flag8 = true;
										break;
									}
								}
								if (!flag8)
								{
									Hizmet hizmet3 = new Hizmet();
									hizmet3.hiz_kod = text52;
									hizmet3.hiz_special1 = text55;
									hizmet3.hiz_special2 = text56;
									hizmet3.hiz_special3 = text57;
									int hiz_KDV = 4;
									switch (getInt170)
									{
									case 1:
										hiz_KDV = getInt171;
										break;
									case 2:
									{
										num2 = getInt172;
										num3 = getInt173;
										string text68 = "0";
										if (num2 != 0 || num3 != 0)
										{
											text68 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
										}
										double result11 = 0.0;
										double.TryParse(text68.Replace(".", ","), out result11);
										num = getInt241;
										num2 = getInt242;
										num3 = getInt243;
										if (num != 0)
										{
											text68 = "";
											if (num2 != 0 || num3 != 0)
											{
												text68 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
											}
											double result12 = 0.0;
											double.TryParse(text68.Replace(".", ","), out result12);
											switch (num)
											{
											case 1:
												result11 += result12;
												break;
											case 2:
												result11 -= result12;
												break;
											case 3:
												result11 /= result12;
												break;
											case 4:
												result11 *= result12;
												break;
											}
										}
										num = getInt244;
										num2 = getInt245;
										num3 = getInt246;
										if (num != 0)
										{
											text68 = "";
											if (num2 != 0 || num3 != 0)
											{
												text68 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
											}
											double result13 = 0.0;
											double.TryParse(text68.Replace(".", ","), out result13);
											switch (num)
											{
											case 1:
												result11 += result13;
												break;
											case 2:
												result11 -= result13;
												break;
											case 3:
												result11 /= result13;
												break;
											case 4:
												result11 *= result13;
												break;
											}
										}
										hiz_KDV = GenelUtility.GetKdvPntr((int)result11);
										break;
									}
									}
									hizmet3.hiz_KDV = hiz_KDV;
									string text69 = "";
									if (getBoolean76)
									{
										text69 = getString110;
									}
									else
									{
										if (getInt485 != 0 || getInt486 != 0)
										{
											text69 = ((!getBoolean3) ? text2.Substring(getInt485 - 1, getInt486).Trim() : array2[getInt485 - 1]);
										}
										if (text69.Length > 50)
										{
											text69 = text69.Substring(0, 50);
										}
									}
									hizmet3.hiz_isim = text69;
									string text70 = "";
									if (getBoolean77)
									{
										text70 = getString111;
									}
									else
									{
										if (getInt487 != 0 || getInt488 != 0)
										{
											text70 = ((!getBoolean3) ? text2.Substring(getInt487 - 1, getInt488).Trim() : array2[getInt487 - 1]);
										}
										if (text70.Length > 25)
										{
											text70 = text70.Substring(0, 25);
										}
									}
									hizmet3.hiz_yabanci_isim = text70;
									double num9 = 0.0;
									if (getInt147 != 0 || getInt148 != 0)
									{
										if (getBoolean3)
										{
											double result14 = 0.0;
											double.TryParse(array2[getInt147 - 1].Replace(".", ","), out result14);
											num9 = result14;
										}
										else
										{
											double result15 = 0.0;
											double.TryParse(text2.Substring(getInt147 - 1, getInt148).Trim().Replace(".", ","), out result15);
											num9 = result15;
										}
									}
									num = getInt235;
									num2 = getInt236;
									num3 = getInt237;
									if (num != 0)
									{
										string text71 = "";
										if (num2 != 0 || num3 != 0)
										{
											text71 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
										}
										double result16 = 0.0;
										double.TryParse(text71.Replace(".", ","), out result16);
										switch (num)
										{
										case 1:
											num9 += result16;
											break;
										case 2:
											num9 -= result16;
											break;
										case 3:
											num9 /= result16;
											break;
										case 4:
											num9 *= result16;
											break;
										}
									}
									num = getInt238;
									num2 = getInt239;
									num3 = getInt240;
									if (num != 0)
									{
										string text72 = "";
										if (num2 != 0 || num3 != 0)
										{
											text72 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
										}
										double result17 = 0.0;
										double.TryParse(text72.Replace(".", ","), out result17);
										switch (num)
										{
										case 1:
											num9 += result17;
											break;
										case 2:
											num9 -= result17;
											break;
										case 3:
											num9 /= result17;
											break;
										case 4:
											num9 *= result17;
											break;
										}
									}
									if (getBoolean37)
									{
										double yuzde = _mikrouygulamabilgileri.vergitanimlari[hizmet3.hiz_KDV].Yuzde;
										double num10 = 100.0;
										num9 /= yuzde / num10 + 1.0;
									}
									if (num9 < 0.0)
									{
										num9 *= -1.0;
									}
									hizmet3.BirimFiyat.FiyatBrut = num9;
									hizmet3.hiz_tipkod = getString112;
									hizmet3.hiz_sinifkod = getString113;
									hizmet3.hiz_grupkod = getString114;
									hizmet3.hiz_sat_muh_kod = getString115;
									hizmet3.hiz_sat_iade_muh_kod = getString116;
									hizmet3.hiz_mal_muh_kod = getString117;
									hizmet3.hiz_sat_mal_muh_kod = getString118;
									hizmet3.hiz_mal_yan_muh_kod = getString119;
									hizmet3.hiz_muh_sat_isk_kod = getString120;
									hizmet3.hiz_muh_aIiskmuhkod = getString121;
									hizmet3.hiz_ilavemasmuhkod = getString122;
									list2.Add(hizmet3);
								}
							}
						}
						string text73 = "";
						if (getBoolean13)
						{
							text73 = getString23;
						}
						else
						{
							num2 = getInt78;
							num3 = getInt79;
							string text74 = "";
							if (num2 != 0 || num3 != 0)
							{
								text74 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							text73 = text74;
						}
						bool flag9 = false;
						Proje proje = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text73);
						if (!(proje.pro_kodu == "") && proje.pro_kodu != null)
						{
							flag9 = true;
						}
						if (text73 == "")
						{
							flag9 = true;
						}
						if (!flag9)
						{
							bool flag10 = false;
							foreach (Proje item4 in list3)
							{
								if (text73 == item4.pro_kodu)
								{
									flag10 = true;
									break;
								}
							}
							if (!flag10)
							{
								Proje proje2 = new Proje();
								proje2.pro_kodu = text73;
								string text75 = "";
								if (getBoolean83)
								{
									text75 = getString130;
								}
								else
								{
									if (getInt531 != 0 || getInt532 != 0)
									{
										text75 = ((!getBoolean3) ? text2.Substring(getInt531 - 1, getInt532).Trim() : array2[getInt531 - 1]);
									}
									if (text75.Length > 40)
									{
										text75 = text75.Substring(0, 40);
									}
								}
								proje2.pro_adi = text75;
								string text76 = "";
								if (getBoolean84)
								{
									text76 = getString131;
								}
								else
								{
									if (getInt533 != 0 || getInt534 != 0)
									{
										text76 = ((!getBoolean3) ? text2.Substring(getInt533 - 1, getInt534).Trim() : array2[getInt533 - 1]);
									}
									if (text76.Length > 25)
									{
										text76 = text76.Substring(0, 25);
									}
								}
								proje2.pro_musterikodu = text76;
								string text77 = "";
								if (getBoolean85)
								{
									text77 = getString132;
								}
								else
								{
									if (getInt535 != 0 || getInt536 != 0)
									{
										text77 = ((!getBoolean3) ? text2.Substring(getInt535 - 1, getInt536).Trim() : array2[getInt535 - 1]);
									}
									if (text77.Length > 25)
									{
										text77 = text77.Substring(0, 25);
									}
								}
								proje2.pro_sormerkodu = text77;
								string text78 = "";
								if (getBoolean86)
								{
									text78 = getString133;
								}
								else
								{
									if (getInt537 != 0 || getInt538 != 0)
									{
										text78 = ((!getBoolean3) ? text2.Substring(getInt537 - 1, getInt538).Trim() : array2[getInt537 - 1]);
									}
									if (text78.Length > 25)
									{
										text78 = text78.Substring(0, 25);
									}
								}
								proje2.pro_grupkodu = text78;
								string text79 = "";
								if (getBoolean87)
								{
									text79 = getString134;
								}
								else
								{
									if (getInt539 != 0 || getInt540 != 0)
									{
										text79 = ((!getBoolean3) ? text2.Substring(getInt539 - 1, getInt540).Trim() : array2[getInt539 - 1]);
									}
									if (text79.Length > 25)
									{
										text79 = text79.Substring(0, 25);
									}
								}
								proje2.pro_sektorkodu = text79;
								string text80 = "";
								if (getBoolean88)
								{
									text80 = getString135;
								}
								else
								{
									if (getInt541 != 0 || getInt542 != 0)
									{
										text80 = ((!getBoolean3) ? text2.Substring(getInt541 - 1, getInt542).Trim() : array2[getInt541 - 1]);
									}
									if (text80.Length > 25)
									{
										text80 = text80.Substring(0, 25);
									}
								}
								proje2.pro_bolgekodu = text80;
								string text81 = "";
								if (getBoolean89)
								{
									text81 = getString136;
								}
								else
								{
									if (getInt543 != 0 || getInt544 != 0)
									{
										text81 = ((!getBoolean3) ? text2.Substring(getInt543 - 1, getInt544).Trim() : array2[getInt543 - 1]);
									}
									if (text81.Length > 25)
									{
										text81 = text81.Substring(0, 25);
									}
								}
								proje2.pro_ana_projekodu = text81;
								string text82 = "";
								if (getBoolean90)
								{
									text82 = getString137;
								}
								else
								{
									if (getInt545 != 0 || getInt546 != 0)
									{
										text82 = ((!getBoolean3) ? text2.Substring(getInt545 - 1, getInt546).Trim() : array2[getInt545 - 1]);
									}
									if (text82.Length > 50)
									{
										text82 = text82.Substring(0, 50);
									}
								}
								proje2.pro_aciklama = text82;
								string text83 = "";
								if (getBoolean91)
								{
									text83 = getString138;
								}
								else
								{
									if (getInt547 != 0 || getInt548 != 0)
									{
										text83 = ((!getBoolean3) ? text2.Substring(getInt547 - 1, getInt548).Trim() : array2[getInt547 - 1]);
									}
									if (text83.Length > 10)
									{
										text83 = text83.Substring(0, 10);
									}
								}
								proje2.pro_muh_kod_artikeli = text83;
								list3.Add(proje2);
							}
						}
						string text84 = "";
						if (getBoolean14)
						{
							text84 = getString24;
						}
						else
						{
							num2 = getInt80;
							num3 = getInt81;
							string text85 = "";
							if (num2 != 0 || num3 != 0)
							{
								text85 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							text84 = text85;
						}
						bool flag11 = false;
						SorumlulukMerkezi sorumlulukMerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text84);
						if (!(sorumlulukMerkezi.som_kod == "") && sorumlulukMerkezi.som_kod != null)
						{
							flag11 = true;
						}
						if (text84 == "")
						{
							flag11 = true;
						}
						if (!flag11)
						{
							bool flag12 = false;
							foreach (SorumlulukMerkezi item5 in list4)
							{
								if (text84 == item5.som_kod)
								{
									flag12 = true;
									break;
								}
							}
							if (!flag12)
							{
								SorumlulukMerkezi sorumlulukMerkezi2 = new SorumlulukMerkezi();
								sorumlulukMerkezi2.som_kod = text84;
								string text86 = "";
								if (getBoolean92)
								{
									text86 = getString139;
								}
								else
								{
									if (getInt549 != 0 || getInt550 != 0)
									{
										text86 = ((!getBoolean3) ? text2.Substring(getInt549 - 1, getInt550).Trim() : array2[getInt549 - 1]);
									}
									if (text86.Length > 40)
									{
										text86 = text86.Substring(0, 40);
									}
								}
								sorumlulukMerkezi2.som_isim = text86;
								string text87 = "";
								if (getBoolean93)
								{
									text87 = getString140;
								}
								else
								{
									if (getInt551 != 0 || getInt552 != 0)
									{
										text87 = ((!getBoolean3) ? text2.Substring(getInt551 - 1, getInt552).Trim() : array2[getInt551 - 1]);
									}
									if (text87.Length > 10)
									{
										text87 = text87.Substring(0, 10);
									}
								}
								sorumlulukMerkezi2.som_MuhArtikeli = text87;
								list4.Add(sorumlulukMerkezi2);
							}
						}
					}
					num4++;
				}
			}
			if (list.Count > 0)
			{
				switch (getInt526)
				{
				case 1:
					foreach (Cari item6 in list)
					{
						item6.KaydetYeniCari(Db.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniCarileriSorVeAc(list);
					break;
				}
			}
			if (bindingList2.Count > 0)
			{
				switch (getInt527)
				{
				case 1:
					foreach (Stok item7 in bindingList2)
					{
						StokData.YeniStokKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item7, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniStoklariSorVeAc(bindingList2);
					break;
				}
			}
			if (list2.Count > 0)
			{
				switch (getInt528)
				{
				case 1:
					foreach (Hizmet item8 in list2)
					{
						HizmetData.YeniHizmetKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item8, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniHizmetleriSorVeAc(list2);
					break;
				}
			}
			if (list3.Count > 0)
			{
				switch (getInt529)
				{
				case 1:
					foreach (Proje item9 in list3)
					{
						ProjeData.YeniProjeKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item9, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniProjeleriSorVeAc(list3);
					break;
				}
			}
			if (list4.Count > 0)
			{
				switch (getInt530)
				{
				case 1:
					foreach (SorumlulukMerkezi item10 in list4)
					{
						SorumlulukMerkeziData.YeniSorumlulukMerkeziKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item10, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniSorumlulukMerkezleriSorVeAc(list4);
					break;
				}
			}
			using StreamReader streamReader2 = new StreamReader(DosyaAdi, Encoding.GetEncoding("windows-1254"));
			enum_GenelEvrakTipleri enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.SatisFaturasi;
			enum_cha_normal_Iade normaliade = enum_cha_normal_Iade.Normal;
			enum_KapamaSekli kapamasekli = enum_KapamaSekli.AcikHesap;
			enum_cha_ticaret_turu ticaretturu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
			DateTime dateTime = DateTime.Now;
			string evraknoseri = "";
			int evraknosira = 0;
			string belgeno = "";
			DateTime belgetarih = DateTime.Now;
			Cari cari2 = new Cari();
			int odemeplani = 0;
			FiyatListesi fiyatlistesi = new FiyatListesi();
			int dovizcinsi = 0;
			Kur kur = new Kur();
			int alternatifdovizcinsi = 0;
			Kur alternatifdovizkuru = new Kur();
			Depo kaynakdepo = new Depo();
			Depo hedefdepo = new Depo();
			Proje proje3 = new Proje();
			SorumlulukMerkezi sorumlulukmerkezi = new SorumlulukMerkezi();
			string temsilcikodu = "";
			Firma firma = new Firma();
			Sube sube = new Sube();
			int mikrouserno = 1;
			DateTime sevkteslimtarihi = DateTime.Now;
			int sevkadresno = 1;
			string kapamahesapkodu = "";
			string text88 = "";
			string text89 = "";
			string text90 = "";
			string text91 = "";
			string text92 = "";
			string text93 = "";
			string text94 = "";
			string text95 = "";
			string text96 = "";
			string text97 = "";
			string degistirspecialalan = "";
			string degistirspecialalan2 = "";
			string degistirspecialalan3 = "";
			string kriter_string = "";
			string kriter_string2 = "";
			string kriter_string3 = "";
			string kriter_string4 = "";
			string kriter_string5 = "";
			double result18 = 0.0;
			double result19 = 0.0;
			double result20 = 0.0;
			double result21 = 0.0;
			double result22 = 0.0;
			bool kriter_bool = false;
			bool kriter_bool2 = false;
			bool kriter_bool3 = false;
			bool kriter_bool4 = false;
			bool kriter_bool5 = false;
			num4 = 0;
			string text98;
			while ((text98 = streamReader2.ReadLine()) != null)
			{
				bool flag13 = true;
				bool flag14 = false;
				if (getBoolean && !text98.StartsWith(getString2))
				{
					flag13 = false;
				}
				if (getBoolean41)
				{
					flag13 = false;
					if (text98.StartsWith(getString53))
					{
						flag13 = true;
					}
				}
				if (getBoolean2 && num4 < getInt)
				{
					flag13 = false;
					flag14 = true;
				}
				if (text98.Trim() == "")
				{
					flag13 = false;
				}
				bool flag15 = false;
				if (getBoolean41)
				{
					if (text98.StartsWith(getString52) && !flag14)
					{
						flag15 = true;
					}
				}
				else if (flag13)
				{
					flag15 = true;
				}
				string[] array4 = null;
				if (getBoolean3)
				{
					array4 = text98.Split(getString3.ToCharArray()[0]);
				}
				if (flag15)
				{
					firma = FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt2);
					sube = SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt3);
					if (getBoolean4)
					{
						enum_GenelEvrakTipleri2 = (enum_GenelEvrakTipleri)getInt27;
					}
					else
					{
						num2 = getInt28;
						num3 = getInt29;
						string text99 = "";
						if (num2 != 0 || num3 != 0)
						{
							text99 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						array = getString8.Split(',');
						foreach (string text100 in array)
						{
							if (text99 == text100)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.AlinanSiparis;
								break;
							}
						}
						array = getString6.Split(',');
						foreach (string text101 in array)
						{
							if (text99 == text101)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.AlisFaturasi;
								break;
							}
						}
						array = getString7.Split(',');
						foreach (string text102 in array)
						{
							if (text99 == text102)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.AlisIrsaliyesi;
								break;
							}
						}
						array = getString4.Split(',');
						foreach (string text103 in array)
						{
							if (text99 == text103)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.SatisFaturasi;
								break;
							}
						}
						array = getString5.Split(',');
						foreach (string text104 in array)
						{
							if (text99 == text104)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.SatisIrsaliyesi;
								break;
							}
						}
						array = getString9.Split(',');
						foreach (string text105 in array)
						{
							if (text99 == text105)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.DepolarArasiSevk;
								break;
							}
						}
					}
					if (getBoolean5)
					{
						normaliade = (enum_cha_normal_Iade)getInt30;
					}
					else
					{
						num2 = getInt31;
						num3 = getInt32;
						string text106 = "";
						if (num2 != 0 || num3 != 0)
						{
							text106 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						array = getString10.Split(',');
						foreach (string text107 in array)
						{
							if (text106 == text107)
							{
								normaliade = enum_cha_normal_Iade.Normal;
								break;
							}
						}
						array = getString11.Split(',');
						foreach (string text108 in array)
						{
							if (text106 == text108)
							{
								normaliade = enum_cha_normal_Iade.Iade;
								break;
							}
						}
					}
					if (getBoolean55)
					{
						ticaretturu = (enum_cha_ticaret_turu)getInt180;
					}
					string text109 = "";
					if (getInt49 != 0 || getInt50 != 0)
					{
						text109 = ((!getBoolean3) ? text98.Substring(getInt49 - 1, getInt50).Trim() : array4[getInt49 - 1]);
					}
					if (text109 == "" && (getInt51 != 0 || getInt52 != 0))
					{
						text109 = ((!getBoolean3) ? text98.Substring(getInt51 - 1, getInt52).Trim() : array4[getInt51 - 1]);
					}
					string text110 = "";
					if (getInt45 != 0 || getInt46 != 0)
					{
						text110 = ((!getBoolean3) ? text98.Substring(getInt45 - 1, getInt46).Trim() : array4[getInt45 - 1]);
					}
					if (text110.Length > 50)
					{
						text110 = text110.Substring(0, 50);
					}
					string text111 = "";
					if (getInt47 != 0 || getInt48 != 0)
					{
						text111 = ((!getBoolean3) ? text98.Substring(getInt47 - 1, getInt48).Trim() : array4[getInt47 - 1]);
					}
					if (text111.Length > 50)
					{
						text111 = text111.Substring(0, 50);
					}
					string text112 = "";
					if (getInt55 != 0 || getInt56 != 0)
					{
						text112 = ((!getBoolean3) ? text98.Substring(getInt55 - 1, getInt56).Trim() : array4[getInt55 - 1]);
					}
					if (text112.Length > 30)
					{
						text112 = text112.Substring(0, 30);
					}
					string text113 = "";
					if (getInt71 != 0 || getInt72 != 0)
					{
						text113 = ((!getBoolean3) ? text98.Substring(getInt71 - 1, getInt72).Trim() : array4[getInt71 - 1]);
					}
					if (text113.Length > 80)
					{
						text113 = text113.Substring(0, 80);
					}
					string text114 = "";
					if (getBoolean43)
					{
						text114 = ((enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisFaturasi && enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisIrsaliyesi) ? getString54 : getString55);
					}
					if (getBoolean10)
					{
						text114 += getString16;
					}
					else
					{
						num2 = getInt43;
						num3 = getInt44;
						string text115 = "";
						if (num2 != 0 || num3 != 0)
						{
							text115 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text114 += text115;
						if (text115 == "")
						{
							int num11 = getInt472;
							int num12 = getInt473;
							string text116 = "";
							if (num11 != 0 || num12 != 0)
							{
								text116 = ((!getBoolean3) ? text98.Substring(num11 - 1, num12).Trim() : array4[num11 - 1]);
							}
							text114 += text116;
						}
					}
					string[] array5 = getString17.Split(',');
					bool flag16 = false;
					cari2 = new Cari();
					array = array5;
					foreach (string text117 in array)
					{
						if (flag16)
						{
							break;
						}
						switch (text117)
						{
						case "1":
							if (text114 != "")
							{
								cari2 = CariData.GetCariByCariKod(Db.Connection, text114, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag16 = true;
								}
							}
							break;
						case "2":
							if (text109 != "")
							{
								cari2 = CariData.GetCariByVergiTcKimlikNo(Db.Connection, text109, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag16 = true;
								}
							}
							break;
						case "3":
							if (text113 != "")
							{
								cari2 = CariData.GetCariByEMail(Db.Connection, text113, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag16 = true;
								}
							}
							break;
						case "4":
							if (text112 != "")
							{
								cari2 = CariData.GetCariByBankaHesapNo(Db.Connection, text112, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag16 = true;
								}
							}
							break;
						case "5":
							if (text110 != "")
							{
								cari2 = CariData.GetCariByCariUnvan(Db.Connection, text110, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag16 = true;
								}
							}
							break;
						}
					}
					fiyatlistesi = FiyatListesiData.GetFiyatListesi(Db.Connection, cari2.cari_satis_fk);
					dovizcinsi = cari2.cari_doviz_cinsi;
					odemeplani = cari2.cari_odemeplan_no;
					kaynakdepo = new Depo();
					if (getBoolean38)
					{
						kaynakdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt174);
					}
					if (getBoolean39)
					{
						kaynakdepo = ((enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisFaturasi && enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisIrsaliyesi) ? DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, cari2.cari_VarsayilanCikisDepo) : DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, cari2.cari_VarsayilanGirisDepo));
					}
					if (getInt175 != 0 || getInt176 != 0)
					{
						try
						{
							kaynakdepo = ((!getBoolean3) ? DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, int.Parse(text98.Substring(getInt175 - 1, getInt176).Trim())) : DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, int.Parse(array4[getInt175 - 1])));
						}
						catch
						{
						}
					}
					hedefdepo = new Depo();
					if (getBoolean40)
					{
						hedefdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt177);
					}
					if (getInt178 != 0 || getInt179 != 0)
					{
						try
						{
							hedefdepo = ((!getBoolean3) ? DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, int.Parse(text98.Substring(getInt178 - 1, getInt179).Trim())) : DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, int.Parse(array4[getInt178 - 1])));
						}
						catch
						{
						}
					}
					if (getInt5 != 0 || getInt6 != 0)
					{
						if (getBoolean3)
						{
							dateTime = DateTime.Parse(array4[getInt5 - 1]);
						}
						else
						{
							int num13 = int.Parse(text98.Substring(getInt5 - 1, getInt6).Trim());
							if (getInt6 == 2)
							{
								num13 += 2000;
							}
							int month = int.Parse(text98.Substring(getInt7 - 1, getInt8).Trim());
							int day = int.Parse(text98.Substring(getInt9 - 1, getInt10).Trim());
							dateTime = new DateTime(num13, month, day);
						}
					}
					bool flag17 = false;
					if (getBoolean36)
					{
						flag17 = CariData.GetEFaturaCarisiMi(Db.Connection, cari2.cari_kod);
					}
					if (getBoolean35)
					{
						switch (enum_GenelEvrakTipleri2)
						{
						case enum_GenelEvrakTipleri.SatisFaturasi:
							evraknoseri = getString44;
							if (flag17)
							{
								evraknoseri = getString49;
							}
							break;
						case enum_GenelEvrakTipleri.SatisIrsaliyesi:
							evraknoseri = getString45;
							break;
						case enum_GenelEvrakTipleri.AlisFaturasi:
							evraknoseri = getString46;
							if (flag17)
							{
								evraknoseri = getString50;
							}
							break;
						case enum_GenelEvrakTipleri.AlisIrsaliyesi:
							evraknoseri = getString47;
							break;
						case enum_GenelEvrakTipleri.AlinanSiparis:
							evraknoseri = getString48;
							break;
						case enum_GenelEvrakTipleri.DepolarArasiSevk:
							evraknoseri = getString51;
							break;
						}
					}
					else
					{
						num2 = getInt139;
						num3 = getInt140;
						string text118 = "";
						if (num2 != 0 || num3 != 0)
						{
							text118 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						evraknoseri = text118;
					}
					if (getInt141 != 0 || getInt142 != 0)
					{
						evraknosira = ((!getBoolean3) ? int.Parse(text98.Substring(getInt141 - 1, getInt142).Trim()) : int.Parse(array4[getInt141 - 1]));
					}
					belgetarih = dateTime;
					if (getInt13 != 0 || getInt14 != 0)
					{
						if (getBoolean3)
						{
							belgetarih = DateTime.Parse(array4[getInt13 - 1]);
						}
						else
						{
							int num14 = int.Parse(text98.Substring(getInt13 - 1, getInt14).Trim());
							if (getInt14 == 2)
							{
								num14 += 2000;
							}
							int month2 = int.Parse(text98.Substring(getInt15 - 1, getInt16).Trim());
							int day2 = int.Parse(text98.Substring(getInt17 - 1, getInt18).Trim());
							belgetarih = new DateTime(num14, month2, day2);
						}
					}
					belgeno = "";
					if (getInt11 != 0 || getInt12 != 0)
					{
						belgeno = ((!getBoolean3) ? text98.Substring(getInt11 - 1, getInt12).Trim() : array4[getInt11 - 1]);
					}
					kur = new Kur();
					if (getInt19 != 0 || getInt20 != 0)
					{
						if (getBoolean3)
						{
							double result23 = 1.0;
							double.TryParse(array4[getInt19 - 1].Replace(".", ","), out result23);
							kur.dov_fiyat = result23;
						}
						else
						{
							double result24 = 1.0;
							double.TryParse(text98.Substring(getInt19 - 1, getInt20).Trim().Replace(".", ","), out result24);
							kur.dov_fiyat = result24;
						}
					}
					if (getBoolean11)
					{
						kapamasekli = (enum_KapamaSekli)getInt73;
					}
					else
					{
						num2 = getInt74;
						num3 = getInt75;
						string text119 = "";
						if (num2 != 0 || num3 != 0)
						{
							text119 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (text119 == getString18)
						{
							kapamasekli = enum_KapamaSekli.AcikHesap;
						}
						if (text119 == getString20)
						{
							kapamasekli = enum_KapamaSekli.BankadanKapanacak;
						}
						if (text119 == getString21)
						{
							kapamasekli = enum_KapamaSekli.CariPersoneldenKapanacak;
						}
						if (text119 == getString19)
						{
							kapamasekli = enum_KapamaSekli.KasadanKapanacak;
						}
					}
					if (getBoolean12)
					{
						kapamahesapkodu = getString22;
					}
					else
					{
						num2 = getInt76;
						num3 = getInt77;
						string text120 = "";
						if (num2 != 0 || num3 != 0)
						{
							text120 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						kapamahesapkodu = text120;
					}
					sevkteslimtarihi = dateTime;
					if (getInt21 != 0 || getInt22 != 0)
					{
						if (getBoolean3)
						{
							sevkteslimtarihi = DateTime.Parse(array4[getInt21 - 1]);
						}
						else
						{
							int num15 = int.Parse(text98.Substring(getInt21 - 1, getInt22).Trim());
							if (getInt14 == 2)
							{
								num15 += 2000;
							}
							int month3 = int.Parse(text98.Substring(getInt23 - 1, getInt24).Trim());
							int day3 = int.Parse(text98.Substring(getInt25 - 1, getInt26).Trim());
							sevkteslimtarihi = new DateTime(num15, month3, day3);
						}
					}
					proje3 = new Proje();
					string text121 = "";
					if (getBoolean13)
					{
						text121 = getString23;
					}
					else
					{
						num2 = getInt78;
						num3 = getInt79;
						string text122 = "";
						if (num2 != 0 || num3 != 0)
						{
							text122 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text121 = text122;
					}
					if (text121 != "")
					{
						proje3 = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text121);
					}
					sorumlulukmerkezi = new SorumlulukMerkezi();
					string text123 = "";
					if (getBoolean14)
					{
						text123 = getString24;
					}
					else
					{
						num2 = getInt80;
						num3 = getInt81;
						string text124 = "";
						if (num2 != 0 || num3 != 0)
						{
							text124 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text123 = text124;
					}
					if (text123 != "")
					{
						sorumlulukmerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text123);
					}
					if (getBoolean15)
					{
						temsilcikodu = getString25;
					}
					else
					{
						num2 = getInt82;
						num3 = getInt83;
						string text125 = "";
						if (num2 != 0 || num3 != 0)
						{
							text125 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (getBoolean16)
						{
							text125 = cari2.cari_temsilci_kodu;
						}
						temsilcikodu = text125;
					}
					if (getBoolean18)
					{
						text88 = getString27;
					}
					else
					{
						num2 = getInt86;
						num3 = getInt87;
						string text126 = "";
						if (num2 != 0 || num3 != 0)
						{
							text126 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text88 = text126;
					}
					if (getBoolean45)
					{
						text88 = getString57 + text88;
					}
					if (getBoolean19)
					{
						text89 = getString28;
					}
					else
					{
						num2 = getInt88;
						num3 = getInt89;
						string text127 = "";
						if (num2 != 0 || num3 != 0)
						{
							text127 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text89 = text127;
					}
					if (getBoolean46)
					{
						text89 = getString58 + text89;
					}
					if (getBoolean20)
					{
						text90 = getString29;
					}
					else
					{
						num2 = getInt90;
						num3 = getInt91;
						string text128 = "";
						if (num2 != 0 || num3 != 0)
						{
							text128 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text90 = text128;
					}
					if (getBoolean47)
					{
						text90 = getString59 + text90;
					}
					if (getBoolean21)
					{
						text91 = getString30;
					}
					else
					{
						num2 = getInt92;
						num3 = getInt93;
						string text129 = "";
						if (num2 != 0 || num3 != 0)
						{
							text129 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text91 = text129;
					}
					if (getBoolean48)
					{
						text91 = getString60 + text91;
					}
					if (getBoolean22)
					{
						text92 = getString31;
					}
					else
					{
						num2 = getInt94;
						num3 = getInt95;
						string text130 = "";
						if (num2 != 0 || num3 != 0)
						{
							text130 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text92 = text130;
					}
					if (getBoolean49)
					{
						text92 = getString61 + text92;
					}
					if (getBoolean23)
					{
						text93 = getString32;
					}
					else
					{
						num2 = getInt96;
						num3 = getInt97;
						string text131 = "";
						if (num2 != 0 || num3 != 0)
						{
							text131 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text93 = text131;
					}
					if (getBoolean50)
					{
						text93 = getString62 + text93;
					}
					if (getBoolean24)
					{
						text94 = getString33;
					}
					else
					{
						num2 = getInt98;
						num3 = getInt99;
						string text132 = "";
						if (num2 != 0 || num3 != 0)
						{
							text132 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text94 = text132;
					}
					if (getBoolean51)
					{
						text94 = getString63 + text94;
					}
					if (getBoolean25)
					{
						text95 = getString34;
					}
					else
					{
						num2 = getInt100;
						num3 = getInt101;
						string text133 = "";
						if (num2 != 0 || num3 != 0)
						{
							text133 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text95 = text133;
					}
					if (getBoolean52)
					{
						text95 = getString64 + text95;
					}
					if (getBoolean26)
					{
						text96 = getString35;
					}
					else
					{
						num2 = getInt102;
						num3 = getInt103;
						string text134 = "";
						if (num2 != 0 || num3 != 0)
						{
							text134 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text96 = text134;
					}
					if (getBoolean53)
					{
						text96 = getString65 + text96;
					}
					if (getBoolean27)
					{
						text97 = getString36;
					}
					else
					{
						num2 = getInt104;
						num3 = getInt105;
						string text135 = "";
						if (num2 != 0 || num3 != 0)
						{
							text135 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text97 = text135;
					}
					if (getBoolean54)
					{
						text97 = getString66 + text97;
					}
					if (getBoolean28)
					{
						degistirspecialalan = getString37;
					}
					else
					{
						num2 = getInt106;
						num3 = getInt107;
						string text136 = "";
						if (num2 != 0 || num3 != 0)
						{
							text136 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						degistirspecialalan = text136;
					}
					if (getBoolean29)
					{
						degistirspecialalan2 = getString38;
					}
					else
					{
						num2 = getInt108;
						num3 = getInt109;
						string text137 = "";
						if (num2 != 0 || num3 != 0)
						{
							text137 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						degistirspecialalan2 = text137;
					}
					if (getBoolean30)
					{
						degistirspecialalan3 = getString39;
					}
					else
					{
						num2 = getInt110;
						num3 = getInt111;
						string text138 = "";
						if (num2 != 0 || num3 != 0)
						{
							text138 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						degistirspecialalan3 = text138;
					}
				}
				num2 = getInt494;
				num3 = getInt495;
				if (num2 != 0 || num3 != 0)
				{
					string text139 = "";
					text139 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string = text139;
				}
				num2 = getInt496;
				num3 = getInt497;
				if (num2 != 0 || num3 != 0)
				{
					string text140 = "";
					text140 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string2 = text140;
				}
				num2 = getInt498;
				num3 = getInt499;
				if (num2 != 0 || num3 != 0)
				{
					string text141 = "";
					text141 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string3 = text141;
				}
				num2 = getInt500;
				num3 = getInt501;
				if (num2 != 0 || num3 != 0)
				{
					string text142 = "";
					text142 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string4 = text142;
				}
				num2 = getInt502;
				num3 = getInt503;
				if (num2 != 0 || num3 != 0)
				{
					string text143 = "";
					text143 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string5 = text143;
				}
				num2 = getInt504;
				num3 = getInt505;
				if (num2 != 0 || num3 != 0)
				{
					string text144 = "";
					text144 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text144.Replace(".", ","), out result18);
				}
				num2 = getInt506;
				num3 = getInt507;
				if (num2 != 0 || num3 != 0)
				{
					string text145 = "";
					text145 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text145.Replace(".", ","), out result19);
				}
				num2 = getInt508;
				num3 = getInt509;
				if (num2 != 0 || num3 != 0)
				{
					string text146 = "";
					text146 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text146.Replace(".", ","), out result20);
				}
				num2 = getInt510;
				num3 = getInt511;
				if (num2 != 0 || num3 != 0)
				{
					string text147 = "";
					text147 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text147.Replace(".", ","), out result21);
				}
				num2 = getInt512;
				num3 = getInt513;
				if (num2 != 0 || num3 != 0)
				{
					string text148 = "";
					text148 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text148.Replace(".", ","), out result22);
				}
				num2 = getInt514;
				num3 = getInt515;
				if (num2 != 0 || num3 != 0)
				{
					string text149 = "";
					text149 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text149 == getString125)
					{
						kriter_bool = true;
					}
				}
				num2 = getInt516;
				num3 = getInt517;
				if (num2 != 0 || num3 != 0)
				{
					string text150 = "";
					text150 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text150 == getString126)
					{
						kriter_bool2 = true;
					}
				}
				num2 = getInt518;
				num3 = getInt519;
				if (num2 != 0 || num3 != 0)
				{
					string text151 = "";
					text151 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text151 == getString127)
					{
						kriter_bool3 = true;
					}
				}
				num2 = getInt520;
				num3 = getInt521;
				if (num2 != 0 || num3 != 0)
				{
					string text152 = "";
					text152 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text152 == getString128)
					{
						kriter_bool4 = true;
					}
				}
				num2 = getInt522;
				num3 = getInt523;
				if (num2 != 0 || num3 != 0)
				{
					string text153 = "";
					text153 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text153 == getString129)
					{
						kriter_bool5 = true;
					}
				}
				if (flag13)
				{
					GenelEvrakSatir genelEvrakSatir = new GenelEvrakSatir();
					if (getBoolean82)
					{
						genelEvrakSatir.KayitID = num4;
					}
					else
					{
						num2 = getInt524;
						num3 = getInt525;
						string s2 = "";
						if (num2 != 0 || num3 != 0)
						{
							s2 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						int result25 = 0;
						int.TryParse(s2, out result25);
						genelEvrakSatir.KayitID = result25;
					}
					genelEvrakSatir.evraktipi = enum_GenelEvrakTipleri2;
					genelEvrakSatir.normaliade = normaliade;
					genelEvrakSatir.kapamasekli = kapamasekli;
					genelEvrakSatir.ticaretturu = ticaretturu;
					genelEvrakSatir.evraktarih = dateTime;
					genelEvrakSatir.evraknoseri = evraknoseri;
					genelEvrakSatir.evraknosira = evraknosira;
					genelEvrakSatir.belgeno = belgeno;
					genelEvrakSatir.belgetarih = belgetarih;
					genelEvrakSatir.cari = cari2;
					genelEvrakSatir.odemeplani = odemeplani;
					genelEvrakSatir.fiyatlistesi = fiyatlistesi;
					genelEvrakSatir.dovizcinsi = dovizcinsi;
					genelEvrakSatir.kur = kur;
					genelEvrakSatir.alternatifdovizcinsi = alternatifdovizcinsi;
					genelEvrakSatir.alternatifdovizkuru = alternatifdovizkuru;
					genelEvrakSatir.kaynakdepo = kaynakdepo;
					genelEvrakSatir.hedefdepo = hedefdepo;
					genelEvrakSatir.proje = proje3;
					genelEvrakSatir.sorumlulukmerkezi = sorumlulukmerkezi;
					genelEvrakSatir.temsilcikodu = temsilcikodu;
					genelEvrakSatir.firma = firma;
					genelEvrakSatir.sube = sube;
					genelEvrakSatir.DBCno = getInt4;
					genelEvrakSatir.mikrouserno = mikrouserno;
					genelEvrakSatir.sevkteslimtarihi = sevkteslimtarihi;
					genelEvrakSatir.sevkadresno = sevkadresno;
					genelEvrakSatir.kapamahesapkodu = kapamahesapkodu;
					genelEvrakSatir.aciklama1 = text88;
					genelEvrakSatir.aciklama2 = text89;
					genelEvrakSatir.aciklama3 = text90;
					genelEvrakSatir.aciklama4 = text91;
					genelEvrakSatir.aciklama5 = text92;
					genelEvrakSatir.aciklama6 = text93;
					genelEvrakSatir.aciklama7 = text94;
					genelEvrakSatir.aciklama8 = text95;
					genelEvrakSatir.aciklama9 = text96;
					genelEvrakSatir.aciklama10 = text97;
					genelEvrakSatir.degistirspecialalan1 = degistirspecialalan;
					genelEvrakSatir.degistirspecialalan2 = degistirspecialalan2;
					genelEvrakSatir.degistirspecialalan3 = degistirspecialalan3;
					genelEvrakSatir.kriter_string1 = kriter_string;
					genelEvrakSatir.kriter_string2 = kriter_string2;
					genelEvrakSatir.kriter_string3 = kriter_string3;
					genelEvrakSatir.kriter_string4 = kriter_string4;
					genelEvrakSatir.kriter_string5 = kriter_string5;
					genelEvrakSatir.kriter_double1 = result18;
					genelEvrakSatir.kriter_double2 = result19;
					genelEvrakSatir.kriter_double3 = result20;
					genelEvrakSatir.kriter_double4 = result21;
					genelEvrakSatir.kriter_double5 = result22;
					genelEvrakSatir.kriter_bool1 = kriter_bool;
					genelEvrakSatir.kriter_bool2 = kriter_bool2;
					genelEvrakSatir.kriter_bool3 = kriter_bool3;
					genelEvrakSatir.kriter_bool4 = kriter_bool4;
					genelEvrakSatir.kriter_bool5 = kriter_bool5;
					if (getBoolean7)
					{
						genelEvrakSatir.stokhizmetkodu = getString14;
					}
					else
					{
						num2 = getInt36;
						num3 = getInt37;
						string stokhizmetkodu = "";
						if (num2 != 0 || num3 != 0)
						{
							stokhizmetkodu = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						genelEvrakSatir.stokhizmetkodu = stokhizmetkodu;
					}
					if (getBoolean8)
					{
						genelEvrakSatir.stokpartikodu = getString15;
					}
					else
					{
						num2 = getInt38;
						num3 = getInt39;
						string stokpartikodu = "";
						if (num2 != 0 || num3 != 0)
						{
							stokpartikodu = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						genelEvrakSatir.stokpartikodu = stokpartikodu;
					}
					if (getBoolean9)
					{
						genelEvrakSatir.stoklotno = getInt40;
					}
					else
					{
						num2 = getInt41;
						num3 = getInt42;
						string s3 = "";
						if (num2 != 0 || num3 != 0)
						{
							s3 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						int result26 = 0;
						int.TryParse(s3, out result26);
						genelEvrakSatir.stoklotno = result26;
					}
					if (getBoolean6)
					{
						genelEvrakSatir.satircinsi = (enum_SatirCinsi)getInt33;
					}
					else
					{
						num2 = getInt34;
						num3 = getInt35;
						string text154 = "";
						if (num2 != 0 || num3 != 0)
						{
							text154 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (text154 == getString12)
						{
							genelEvrakSatir.satircinsi = enum_SatirCinsi.Stok;
						}
						if (text154 == getString13)
						{
							genelEvrakSatir.satircinsi = enum_SatirCinsi.Hizmet;
						}
					}
					if (getBoolean42)
					{
						Stok stok4 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
						if (!(stok4.sto_kod == "") && stok4.sto_kod != null)
						{
							genelEvrakSatir.satircinsi = enum_SatirCinsi.Stok;
						}
						else
						{
							Hizmet hizmet4 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							if (!(hizmet4.hiz_kod == "") && hizmet4.hiz_kod != null)
							{
								genelEvrakSatir.satircinsi = enum_SatirCinsi.Hizmet;
							}
						}
					}
					switch (genelEvrakSatir.satircinsi)
					{
					case enum_SatirCinsi.Stok:
					{
						Stok stok5 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
						if (stok5.sto_kod == "" || stok5.sto_kod == null)
						{
							Barkod barkodBilgisi2 = BarkodData.GetBarkodBilgisi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							if (barkodBilgisi2.bar_stokkodu != "")
							{
								stok5 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, barkodBilgisi2.bar_stokkodu, enum_toptan_perakende.Toptan);
								if (stok5.sto_kod != "" && stok5.sto_kod != null)
								{
									genelEvrakSatir.stokhizmetkodu = stok5.sto_kod;
								}
							}
						}
						if (stok5.sto_kod == "" || stok5.sto_kod == null)
						{
							genelEvrakSatir.stokhizmetkodu = "";
						}
						break;
					}
					case enum_SatirCinsi.Hizmet:
					{
						Hizmet hizmet5 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
						if (hizmet5.hiz_kod == "" || hizmet5.hiz_kod == null)
						{
							genelEvrakSatir.stokhizmetkodu = "";
						}
						break;
					}
					}
					if (getBoolean78)
					{
						genelEvrakSatir.fiyat_fark_mi = getBoolean79;
					}
					else
					{
						num2 = getInt489;
						num3 = getInt490;
						string text155 = "";
						if (num2 != 0 || num3 != 0)
						{
							text155 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (text155 == getString123)
						{
							genelEvrakSatir.fiyat_fark_mi = true;
						}
					}
					if (genelEvrakSatir.satircinsi == enum_SatirCinsi.Stok)
					{
						switch (getInt143)
						{
						case 0:
						{
							Stok stok6 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
							genelEvrakSatir.vergi_pntr = stok6.sto_toptan_vergi_yeni;
							break;
						}
						case 1:
							genelEvrakSatir.vergi_pntr = getInt144;
							break;
						case 2:
						{
							num2 = getInt145;
							num3 = getInt146;
							string text156 = "0";
							if (num2 != 0 || num3 != 0)
							{
								text156 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result27 = 0.0;
							double.TryParse(text156.Replace(".", ","), out result27);
							num = getInt187;
							num2 = getInt188;
							num3 = getInt189;
							if (num != 0)
							{
								text156 = "";
								if (num2 != 0 || num3 != 0)
								{
									text156 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result28 = 0.0;
								double.TryParse(text156.Replace(".", ","), out result28);
								switch (num)
								{
								case 1:
									result27 += result28;
									break;
								case 2:
									result27 -= result28;
									break;
								case 3:
									result27 /= result28;
									break;
								case 4:
									result27 *= result28;
									break;
								}
							}
							num = getInt190;
							num2 = getInt191;
							num3 = getInt192;
							if (num != 0)
							{
								text156 = "";
								if (num2 != 0 || num3 != 0)
								{
									text156 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result29 = 0.0;
								double.TryParse(text156.Replace(".", ","), out result29);
								switch (num)
								{
								case 1:
									result27 += result29;
									break;
								case 2:
									result27 -= result29;
									break;
								case 3:
									result27 /= result29;
									break;
								case 4:
									result27 *= result29;
									break;
								}
							}
							genelEvrakSatir.vergi_pntr = GenelUtility.GetKdvPntr((int)result27);
							break;
						}
						}
					}
					else
					{
						switch (getInt170)
						{
						case 0:
						{
							Hizmet hizmet6 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							genelEvrakSatir.vergi_pntr = hizmet6.hiz_KDV;
							break;
						}
						case 1:
							genelEvrakSatir.vergi_pntr = getInt171;
							break;
						case 2:
						{
							num2 = getInt172;
							num3 = getInt173;
							string text157 = "0";
							if (num2 != 0 || num3 != 0)
							{
								text157 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result30 = 0.0;
							double.TryParse(text157.Replace(".", ","), out result30);
							num = getInt241;
							num2 = getInt242;
							num3 = getInt243;
							if (num != 0)
							{
								text157 = "";
								if (num2 != 0 || num3 != 0)
								{
									text157 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result31 = 0.0;
								double.TryParse(text157.Replace(".", ","), out result31);
								switch (num)
								{
								case 1:
									result30 += result31;
									break;
								case 2:
									result30 -= result31;
									break;
								case 3:
									result30 /= result31;
									break;
								case 4:
									result30 *= result31;
									break;
								}
							}
							num = getInt244;
							num2 = getInt245;
							num3 = getInt246;
							if (num != 0)
							{
								text157 = "";
								if (num2 != 0 || num3 != 0)
								{
									text157 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result32 = 0.0;
								double.TryParse(text157.Replace(".", ","), out result32);
								switch (num)
								{
								case 1:
									result30 += result32;
									break;
								case 2:
									result30 -= result32;
									break;
								case 3:
									result30 /= result32;
									break;
								case 4:
									result30 *= result32;
									break;
								}
							}
							genelEvrakSatir.vergi_pntr = GenelUtility.GetKdvPntr((int)result30);
							break;
						}
						}
					}
					if (getBoolean17)
					{
						genelEvrakSatir.faturaaciklama = getString26;
					}
					else
					{
						num2 = getInt84;
						num3 = getInt85;
						string faturaaciklama = "";
						if (num2 != 0 || num3 != 0)
						{
							faturaaciklama = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						genelEvrakSatir.faturaaciklama = faturaaciklama;
					}
					if (getBoolean44)
					{
						genelEvrakSatir.faturaaciklama = getString56 + genelEvrakSatir.faturaaciklama;
					}
					if (getBoolean33)
					{
						double result33 = 1.0;
						double.TryParse(getString42.Replace(".", ","), out result33);
						genelEvrakSatir.miktar = result33;
					}
					else
					{
						num2 = getInt135;
						num3 = getInt136;
						string text158 = "";
						if (num2 != 0 || num3 != 0)
						{
							text158 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (text158 != "")
						{
							double result34 = 1.0;
							double.TryParse(text158.Replace(".", ","), out result34);
							genelEvrakSatir.miktar = result34;
						}
					}
					if (getBoolean34)
					{
						double result35 = 0.0;
						double.TryParse(getString43, out result35);
						genelEvrakSatir.miktar2 = result35;
					}
					else
					{
						num2 = getInt137;
						num3 = getInt138;
						string text159 = "";
						if (num2 != 0 || num3 != 0)
						{
							text159 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (text159 != "")
						{
							double result36 = 0.0;
							double.TryParse(text159.Replace(".", ","), out result36);
							genelEvrakSatir.miktar2 = result36;
						}
					}
					if (getBoolean31)
					{
						if (genelEvrakSatir.satircinsi == enum_SatirCinsi.Stok)
						{
							string virgulileayrilmiskaynaklar = getString40;
							if (genelEvrakSatir.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || genelEvrakSatir.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
							{
								virgulileayrilmiskaynaklar = getString41;
							}
							List<enum_Fiyat_Kaynagi> fiyatKaynaklariList = FiyatUtility.GetFiyatKaynaklariList(virgulileayrilmiskaynaklar);
							Stok stok7 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
							genelEvrakSatir.birimfiyat = StokData.GetFiyatFromMultiSource(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, stok7.sto_kod, stok7.ekleme_bilgileri.vergi_pntr, genelEvrakSatir.fiyatlistesi.sfl_sirano, genelEvrakSatir.fiyatlistesi.sfl_kdvdahil, genelEvrakSatir.cari.cari_kod, genelEvrakSatir.cari.cari_satis_isk_kod, genelEvrakSatir.kaynakdepo.dep_no, genelEvrakSatir.evraktarih, fiyatKaynaklariList, _mikrouygulamabilgileri.vergitanimlari, 0);
						}
						else
						{
							Hizmet hizmet7 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							genelEvrakSatir.birimfiyat.DovizCinsi = genelEvrakSatir.dovizcinsi;
							genelEvrakSatir.birimfiyat.FiyatBrut = hizmet7.BirimFiyat.FiyatBrut;
						}
					}
					else if (genelEvrakSatir.satircinsi == enum_SatirCinsi.Stok)
					{
						if (getInt112 != 0 || getInt113 != 0)
						{
							if (getBoolean3)
							{
								double result37 = 0.0;
								double.TryParse(array4[getInt112 - 1].Replace(".", ","), out result37);
								genelEvrakSatir.birimfiyat.FiyatBrut = result37;
							}
							else
							{
								double result38 = 0.0;
								double.TryParse(text98.Substring(getInt112 - 1, getInt113).Trim().Replace(".", ","), out result38);
								genelEvrakSatir.birimfiyat.FiyatBrut = result38;
							}
						}
						num = getInt181;
						num2 = getInt182;
						num3 = getInt183;
						if (num != 0)
						{
							string text160 = "";
							if (num2 != 0 || num3 != 0)
							{
								text160 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result39 = 0.0;
							double.TryParse(text160.Replace(".", ","), out result39);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result39;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result39;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result39;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result39;
								break;
							}
						}
						num = getInt184;
						num2 = getInt185;
						num3 = getInt186;
						if (num != 0)
						{
							string text161 = "";
							if (num2 != 0 || num3 != 0)
							{
								text161 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result40 = 0.0;
							double.TryParse(text161.Replace(".", ","), out result40);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result40;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result40;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result40;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result40;
								break;
							}
						}
						if (getBoolean32)
						{
							double yuzde2 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir.vergi_pntr].Yuzde;
							double num16 = 100.0;
							genelEvrakSatir.birimfiyat.FiyatBrut = genelEvrakSatir.birimfiyat.FiyatBrut / (yuzde2 / num16 + 1.0);
						}
						genelEvrakSatir.birimfiyat.Iskonto_1_UygulamaSekli = getInt114;
						genelEvrakSatir.birimfiyat.Iskonto_2_UygulamaSekli = getInt117;
						genelEvrakSatir.birimfiyat.Iskonto_3_UygulamaSekli = getInt120;
						genelEvrakSatir.birimfiyat.Iskonto_4_UygulamaSekli = getInt123;
						genelEvrakSatir.birimfiyat.Iskonto_5_UygulamaSekli = getInt126;
						genelEvrakSatir.birimfiyat.Iskonto_6_UygulamaSekli = getInt129;
						if (getInt115 != 0 || getInt116 != 0)
						{
							if (getBoolean3)
							{
								double result41 = 0.0;
								double.TryParse(array4[getInt115 - 1].Replace(".", ","), out result41);
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result41;
							}
							else
							{
								double result42 = 0.0;
								double.TryParse(text98.Substring(getInt115 - 1, getInt116).Trim().Replace(".", ","), out result42);
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result42;
							}
						}
						num = getInt199;
						num2 = getInt200;
						num3 = getInt201;
						if (num != 0)
						{
							string text162 = "";
							if (num2 != 0 || num3 != 0)
							{
								text162 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result43 = 0.0;
							double.TryParse(text162.Replace(".", ","), out result43);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result43;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result43;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result43;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result43;
								break;
							}
						}
						num = getInt202;
						num2 = getInt203;
						num3 = getInt204;
						if (num != 0)
						{
							string text163 = "";
							if (num2 != 0 || num3 != 0)
							{
								text163 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result44 = 0.0;
							double.TryParse(text163.Replace(".", ","), out result44);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result44;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result44;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result44;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result44;
								break;
							}
						}
						if (getInt118 != 0 || getInt119 != 0)
						{
							if (getBoolean3)
							{
								double result45 = 0.0;
								double.TryParse(array4[getInt118 - 1].Replace(".", ","), out result45);
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result45;
							}
							else
							{
								double result46 = 0.0;
								double.TryParse(text98.Substring(getInt118 - 1, getInt119).Trim().Replace(".", ","), out result46);
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result46;
							}
						}
						num = getInt205;
						num2 = getInt206;
						num3 = getInt207;
						if (num != 0)
						{
							string text164 = "";
							if (num2 != 0 || num3 != 0)
							{
								text164 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result47 = 0.0;
							double.TryParse(text164.Replace(".", ","), out result47);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result47;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result47;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result47;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result47;
								break;
							}
						}
						num = getInt208;
						num2 = getInt209;
						num3 = getInt210;
						if (num != 0)
						{
							string text165 = "";
							if (num2 != 0 || num3 != 0)
							{
								text165 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result48 = 0.0;
							double.TryParse(text165.Replace(".", ","), out result48);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result48;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result48;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result48;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result48;
								break;
							}
						}
						if (getInt121 != 0 || getInt122 != 0)
						{
							if (getBoolean3)
							{
								double result49 = 0.0;
								double.TryParse(array4[getInt121 - 1].Replace(".", ","), out result49);
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result49;
							}
							else
							{
								double result50 = 0.0;
								double.TryParse(text98.Substring(getInt121 - 1, getInt122).Trim().Replace(".", ","), out result50);
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result50;
							}
						}
						num = getInt211;
						num2 = getInt212;
						num3 = getInt213;
						if (num != 0)
						{
							string text166 = "";
							if (num2 != 0 || num3 != 0)
							{
								text166 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result51 = 0.0;
							double.TryParse(text166.Replace(".", ","), out result51);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result51;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result51;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result51;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result51;
								break;
							}
						}
						num = getInt214;
						num2 = getInt215;
						num3 = getInt216;
						if (num != 0)
						{
							string text167 = "";
							if (num2 != 0 || num3 != 0)
							{
								text167 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result52 = 0.0;
							double.TryParse(text167.Replace(".", ","), out result52);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result52;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result52;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result52;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result52;
								break;
							}
						}
						if (getInt124 != 0 || getInt125 != 0)
						{
							if (getBoolean3)
							{
								double result53 = 0.0;
								double.TryParse(array4[getInt124 - 1].Replace(".", ","), out result53);
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result53;
							}
							else
							{
								double result54 = 0.0;
								double.TryParse(text98.Substring(getInt124 - 1, getInt125).Trim().Replace(".", ","), out result54);
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result54;
							}
						}
						num = getInt217;
						num2 = getInt218;
						num3 = getInt219;
						if (num != 0)
						{
							string text168 = "";
							if (num2 != 0 || num3 != 0)
							{
								text168 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result55 = 0.0;
							double.TryParse(text168.Replace(".", ","), out result55);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result55;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result55;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result55;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result55;
								break;
							}
						}
						num = getInt220;
						num2 = getInt221;
						num3 = getInt222;
						if (num != 0)
						{
							string text169 = "";
							if (num2 != 0 || num3 != 0)
							{
								text169 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result56 = 0.0;
							double.TryParse(text169.Replace(".", ","), out result56);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result56;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result56;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result56;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result56;
								break;
							}
						}
						if (getInt127 != 0 || getInt128 != 0)
						{
							if (getBoolean3)
							{
								double result57 = 0.0;
								double.TryParse(array4[getInt127 - 1].Replace(".", ","), out result57);
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result57;
							}
							else
							{
								double result58 = 0.0;
								double.TryParse(text98.Substring(getInt127 - 1, getInt128).Trim().Replace(".", ","), out result58);
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result58;
							}
						}
						num = getInt223;
						num2 = getInt224;
						num3 = getInt225;
						if (num != 0)
						{
							string text170 = "";
							if (num2 != 0 || num3 != 0)
							{
								text170 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result59 = 0.0;
							double.TryParse(text170.Replace(".", ","), out result59);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result59;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result59;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result59;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result59;
								break;
							}
						}
						num = getInt226;
						num2 = getInt227;
						num3 = getInt228;
						if (num != 0)
						{
							string text171 = "";
							if (num2 != 0 || num3 != 0)
							{
								text171 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result60 = 0.0;
							double.TryParse(text171.Replace(".", ","), out result60);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result60;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result60;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result60;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result60;
								break;
							}
						}
						if (getInt130 != 0 || getInt131 != 0)
						{
							if (getBoolean3)
							{
								double result61 = 0.0;
								double.TryParse(array4[getInt130 - 1].Replace(".", ","), out result61);
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result61;
							}
							else
							{
								double result62 = 0.0;
								double.TryParse(text98.Substring(getInt130 - 1, getInt131).Trim().Replace(".", ","), out result62);
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result62;
							}
						}
						num = getInt229;
						num2 = getInt230;
						num3 = getInt231;
						if (num != 0)
						{
							string text172 = "";
							if (num2 != 0 || num3 != 0)
							{
								text172 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result63 = 0.0;
							double.TryParse(text172.Replace(".", ","), out result63);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result63;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result63;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result63;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result63;
								break;
							}
						}
						num = getInt232;
						num2 = getInt233;
						num3 = getInt234;
						if (num != 0)
						{
							string text173 = "";
							if (num2 != 0 || num3 != 0)
							{
								text173 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result64 = 0.0;
							double.TryParse(text173.Replace(".", ","), out result64);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result64;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result64;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result64;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result64;
								break;
							}
						}
						genelEvrakSatir.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt132;
						if (getInt133 != 0 || getInt134 != 0)
						{
							if (getBoolean3)
							{
								double result65 = 0.0;
								double.TryParse(array4[getInt133 - 1].Replace(".", ","), out result65);
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar = result65;
							}
							else
							{
								double result66 = 0.0;
								double.TryParse(text98.Substring(getInt133 - 1, getInt134).Trim().Replace(".", ","), out result66);
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar = result66;
							}
						}
						num = getInt193;
						num2 = getInt194;
						num3 = getInt195;
						if (num != 0)
						{
							string text174 = "";
							if (num2 != 0 || num3 != 0)
							{
								text174 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result67 = 0.0;
							double.TryParse(text174.Replace(".", ","), out result67);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result67;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result67;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result67;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result67;
								break;
							}
						}
						num = getInt196;
						num2 = getInt197;
						num3 = getInt198;
						if (num != 0)
						{
							string text175 = "";
							if (num2 != 0 || num3 != 0)
							{
								text175 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result68 = 0.0;
							double.TryParse(text175.Replace(".", ","), out result68);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result68;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result68;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result68;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result68;
								break;
							}
						}
					}
					else
					{
						if (getInt147 != 0 || getInt148 != 0)
						{
							if (getBoolean3)
							{
								double result69 = 0.0;
								double.TryParse(array4[getInt147 - 1].Replace(".", ","), out result69);
								genelEvrakSatir.birimfiyat.FiyatBrut = result69;
							}
							else
							{
								double result70 = 0.0;
								double.TryParse(text98.Substring(getInt147 - 1, getInt148).Trim().Replace(".", ","), out result70);
								genelEvrakSatir.birimfiyat.FiyatBrut = result70;
							}
						}
						num = getInt235;
						num2 = getInt236;
						num3 = getInt237;
						if (num != 0)
						{
							string text176 = "";
							if (num2 != 0 || num3 != 0)
							{
								text176 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result71 = 0.0;
							double.TryParse(text176.Replace(".", ","), out result71);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result71;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result71;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result71;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result71;
								break;
							}
						}
						num = getInt238;
						num2 = getInt239;
						num3 = getInt240;
						if (num != 0)
						{
							string text177 = "";
							if (num2 != 0 || num3 != 0)
							{
								text177 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result72 = 0.0;
							double.TryParse(text177.Replace(".", ","), out result72);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result72;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result72;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result72;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result72;
								break;
							}
						}
						if (getBoolean37)
						{
							double yuzde3 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir.vergi_pntr].Yuzde;
							double num17 = 100.0;
							genelEvrakSatir.birimfiyat.FiyatBrut = genelEvrakSatir.birimfiyat.FiyatBrut / (yuzde3 / num17 + 1.0);
						}
						genelEvrakSatir.birimfiyat.Iskonto_1_UygulamaSekli = getInt149;
						genelEvrakSatir.birimfiyat.Iskonto_2_UygulamaSekli = getInt152;
						genelEvrakSatir.birimfiyat.Iskonto_3_UygulamaSekli = getInt155;
						genelEvrakSatir.birimfiyat.Iskonto_4_UygulamaSekli = getInt158;
						genelEvrakSatir.birimfiyat.Iskonto_5_UygulamaSekli = getInt161;
						genelEvrakSatir.birimfiyat.Iskonto_6_UygulamaSekli = getInt164;
						if (getInt150 != 0 || getInt151 != 0)
						{
							if (getBoolean3)
							{
								double result73 = 0.0;
								double.TryParse(array4[getInt150 - 1].Replace(".", ","), out result73);
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result73;
							}
							else
							{
								double result74 = 0.0;
								double.TryParse(text98.Substring(getInt150 - 1, getInt151).Trim().Replace(".", ","), out result74);
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result74;
							}
						}
						num = getInt253;
						num2 = getInt254;
						num3 = getInt255;
						if (num != 0)
						{
							string text178 = "";
							if (num2 != 0 || num3 != 0)
							{
								text178 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result75 = 0.0;
							double.TryParse(text178.Replace(".", ","), out result75);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result75;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result75;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result75;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result75;
								break;
							}
						}
						num = getInt256;
						num2 = getInt257;
						num3 = getInt258;
						if (num != 0)
						{
							string text179 = "";
							if (num2 != 0 || num3 != 0)
							{
								text179 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result76 = 0.0;
							double.TryParse(text179.Replace(".", ","), out result76);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result76;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result76;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result76;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result76;
								break;
							}
						}
						if (getInt153 != 0 || getInt154 != 0)
						{
							if (getBoolean3)
							{
								double result77 = 0.0;
								double.TryParse(array4[getInt153 - 1].Replace(".", ","), out result77);
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result77;
							}
							else
							{
								double result78 = 0.0;
								double.TryParse(text98.Substring(getInt153 - 1, getInt154).Trim().Replace(".", ","), out result78);
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result78;
							}
						}
						num = getInt259;
						num2 = getInt260;
						num3 = getInt261;
						if (num != 0)
						{
							string text180 = "";
							if (num2 != 0 || num3 != 0)
							{
								text180 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result79 = 0.0;
							double.TryParse(text180.Replace(".", ","), out result79);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result79;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result79;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result79;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result79;
								break;
							}
						}
						num = getInt262;
						num2 = getInt263;
						num3 = getInt264;
						if (num != 0)
						{
							string text181 = "";
							if (num2 != 0 || num3 != 0)
							{
								text181 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result80 = 0.0;
							double.TryParse(text181.Replace(".", ","), out result80);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result80;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result80;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result80;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result80;
								break;
							}
						}
						if (getInt156 != 0 || getInt157 != 0)
						{
							if (getBoolean3)
							{
								double result81 = 0.0;
								double.TryParse(array4[getInt156 - 1].Replace(".", ","), out result81);
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result81;
							}
							else
							{
								double result82 = 0.0;
								double.TryParse(text98.Substring(getInt156 - 1, getInt157).Trim().Replace(".", ","), out result82);
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result82;
							}
						}
						num = getInt265;
						num2 = getInt266;
						num3 = getInt267;
						if (num != 0)
						{
							string text182 = "";
							if (num2 != 0 || num3 != 0)
							{
								text182 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result83 = 0.0;
							double.TryParse(text182.Replace(".", ","), out result83);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result83;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result83;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result83;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result83;
								break;
							}
						}
						num = getInt268;
						num2 = getInt269;
						num3 = getInt270;
						if (num != 0)
						{
							string text183 = "";
							if (num2 != 0 || num3 != 0)
							{
								text183 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result84 = 0.0;
							double.TryParse(text183.Replace(".", ","), out result84);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result84;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result84;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result84;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result84;
								break;
							}
						}
						if (getInt159 != 0 || getInt160 != 0)
						{
							if (getBoolean3)
							{
								double result85 = 0.0;
								double.TryParse(array4[getInt159 - 1].Replace(".", ","), out result85);
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result85;
							}
							else
							{
								double result86 = 0.0;
								double.TryParse(text98.Substring(getInt159 - 1, getInt160).Trim().Replace(".", ","), out result86);
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result86;
							}
						}
						num = getInt271;
						num2 = getInt272;
						num3 = getInt273;
						if (num != 0)
						{
							string text184 = "";
							if (num2 != 0 || num3 != 0)
							{
								text184 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result87 = 0.0;
							double.TryParse(text184.Replace(".", ","), out result87);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result87;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result87;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result87;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result87;
								break;
							}
						}
						num = getInt274;
						num2 = getInt275;
						num3 = getInt276;
						if (num != 0)
						{
							string text185 = "";
							if (num2 != 0 || num3 != 0)
							{
								text185 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result88 = 0.0;
							double.TryParse(text185.Replace(".", ","), out result88);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result88;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result88;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result88;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result88;
								break;
							}
						}
						if (getInt162 != 0 || getInt163 != 0)
						{
							if (getBoolean3)
							{
								double result89 = 0.0;
								double.TryParse(array4[getInt162 - 1].Replace(".", ","), out result89);
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result89;
							}
							else
							{
								double result90 = 0.0;
								double.TryParse(text98.Substring(getInt162 - 1, getInt163).Trim().Replace(".", ","), out result90);
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result90;
							}
						}
						num = getInt277;
						num2 = getInt278;
						num3 = getInt279;
						if (num != 0)
						{
							string text186 = "";
							if (num2 != 0 || num3 != 0)
							{
								text186 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result91 = 0.0;
							double.TryParse(text186.Replace(".", ","), out result91);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result91;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result91;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result91;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result91;
								break;
							}
						}
						num = getInt280;
						num2 = getInt281;
						num3 = getInt282;
						if (num != 0)
						{
							string text187 = "";
							if (num2 != 0 || num3 != 0)
							{
								text187 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result92 = 0.0;
							double.TryParse(text187.Replace(".", ","), out result92);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result92;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result92;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result92;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result92;
								break;
							}
						}
						if (getInt165 != 0 || getInt166 != 0)
						{
							if (getBoolean3)
							{
								double result93 = 0.0;
								double.TryParse(array4[getInt165 - 1].Replace(".", ","), out result93);
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result93;
							}
							else
							{
								double result94 = 0.0;
								double.TryParse(text98.Substring(getInt165 - 1, getInt166).Trim().Replace(".", ","), out result94);
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result94;
							}
						}
						num = getInt283;
						num2 = getInt284;
						num3 = getInt285;
						if (num != 0)
						{
							string text188 = "";
							if (num2 != 0 || num3 != 0)
							{
								text188 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result95 = 0.0;
							double.TryParse(text188.Replace(".", ","), out result95);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result95;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result95;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result95;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result95;
								break;
							}
						}
						num = getInt286;
						num2 = getInt287;
						num3 = getInt288;
						if (num != 0)
						{
							string text189 = "";
							if (num2 != 0 || num3 != 0)
							{
								text189 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result96 = 0.0;
							double.TryParse(text189.Replace(".", ","), out result96);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result96;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result96;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result96;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result96;
								break;
							}
						}
						genelEvrakSatir.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt167;
						if (getInt168 != 0 || getInt169 != 0)
						{
							if (getBoolean3)
							{
								double result97 = 0.0;
								double.TryParse(array4[getInt168 - 1].Replace(".", ","), out result97);
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar = result97;
							}
							else
							{
								double result98 = 0.0;
								double.TryParse(text98.Substring(getInt168 - 1, getInt169).Trim().Replace(".", ","), out result98);
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar = result98;
							}
						}
						num = getInt247;
						num2 = getInt248;
						num3 = getInt249;
						if (num != 0)
						{
							string text190 = "";
							if (num2 != 0 || num3 != 0)
							{
								text190 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result99 = 0.0;
							double.TryParse(text190.Replace(".", ","), out result99);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result99;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result99;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result99;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result99;
								break;
							}
						}
						num = getInt250;
						num2 = getInt251;
						num3 = getInt252;
						if (num != 0)
						{
							string text191 = "";
							if (num2 != 0 || num3 != 0)
							{
								text191 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							double result100 = 0.0;
							double.TryParse(text191.Replace(".", ","), out result100);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result100;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result100;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result100;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result100;
								break;
							}
						}
					}
					genelEvrakSatir.birimfiyat.OtvVergiPntr = getInt493;
					if (genelEvrakSatir.birimfiyat.FiyatBrut != 0.0)
					{
						if (genelEvrakSatir.birimfiyat.FiyatBrut < 0.0 && getBoolean69)
						{
							genelEvrakSatir.evraktipi = (enum_GenelEvrakTipleri)getInt470;
						}
						if (genelEvrakSatir.birimfiyat.FiyatBrut < 0.0)
						{
							genelEvrakSatir.birimfiyat.FiyatBrut *= -1.0;
						}
						bindingList.Add(genelEvrakSatir);
					}
					if (getBoolean57)
					{
						GenelEvrakSatir genelEvrakSatir2 = new GenelEvrakSatir();
						if (getBoolean82)
						{
							genelEvrakSatir2.KayitID = 1000000000 + num4;
						}
						else
						{
							num2 = getInt524;
							num3 = getInt525;
							string s4 = "";
							if (num2 != 0 || num3 != 0)
							{
								s4 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							int result101 = 0;
							int.TryParse(s4, out result101);
							genelEvrakSatir2.KayitID = 1000000000 + result101;
						}
						genelEvrakSatir2.evraktipi = enum_GenelEvrakTipleri2;
						genelEvrakSatir2.normaliade = normaliade;
						genelEvrakSatir2.kapamasekli = kapamasekli;
						genelEvrakSatir2.ticaretturu = ticaretturu;
						genelEvrakSatir2.evraktarih = dateTime;
						genelEvrakSatir2.evraknoseri = evraknoseri;
						genelEvrakSatir2.evraknosira = evraknosira;
						genelEvrakSatir2.belgeno = belgeno;
						genelEvrakSatir2.belgetarih = belgetarih;
						genelEvrakSatir2.cari = cari2;
						genelEvrakSatir2.odemeplani = odemeplani;
						genelEvrakSatir2.fiyatlistesi = fiyatlistesi;
						genelEvrakSatir2.dovizcinsi = dovizcinsi;
						genelEvrakSatir2.kur = kur;
						genelEvrakSatir2.alternatifdovizcinsi = alternatifdovizcinsi;
						genelEvrakSatir2.alternatifdovizkuru = alternatifdovizkuru;
						genelEvrakSatir2.kaynakdepo = kaynakdepo;
						genelEvrakSatir2.hedefdepo = hedefdepo;
						genelEvrakSatir2.proje = proje3;
						genelEvrakSatir2.sorumlulukmerkezi = sorumlulukmerkezi;
						genelEvrakSatir2.temsilcikodu = temsilcikodu;
						genelEvrakSatir2.firma = firma;
						genelEvrakSatir2.sube = sube;
						genelEvrakSatir2.mikrouserno = mikrouserno;
						genelEvrakSatir2.sevkteslimtarihi = sevkteslimtarihi;
						genelEvrakSatir2.sevkadresno = sevkadresno;
						genelEvrakSatir2.kapamahesapkodu = kapamahesapkodu;
						genelEvrakSatir2.aciklama1 = text88;
						genelEvrakSatir2.aciklama2 = text89;
						genelEvrakSatir2.aciklama3 = text90;
						genelEvrakSatir2.aciklama4 = text91;
						genelEvrakSatir2.aciklama5 = text92;
						genelEvrakSatir2.aciklama6 = text93;
						genelEvrakSatir2.aciklama7 = text94;
						genelEvrakSatir2.aciklama8 = text95;
						genelEvrakSatir2.aciklama9 = text96;
						genelEvrakSatir2.aciklama10 = text97;
						genelEvrakSatir2.degistirspecialalan1 = degistirspecialalan;
						genelEvrakSatir2.degistirspecialalan2 = degistirspecialalan2;
						genelEvrakSatir2.degistirspecialalan3 = degistirspecialalan3;
						genelEvrakSatir2.kriter_string1 = kriter_string;
						genelEvrakSatir2.kriter_string2 = kriter_string2;
						genelEvrakSatir2.kriter_string3 = kriter_string3;
						genelEvrakSatir2.kriter_string4 = kriter_string4;
						genelEvrakSatir2.kriter_string5 = kriter_string5;
						genelEvrakSatir2.kriter_double1 = result18;
						genelEvrakSatir2.kriter_double2 = result19;
						genelEvrakSatir2.kriter_double3 = result20;
						genelEvrakSatir2.kriter_double4 = result21;
						genelEvrakSatir2.kriter_double5 = result22;
						genelEvrakSatir2.kriter_bool1 = kriter_bool;
						genelEvrakSatir2.kriter_bool2 = kriter_bool2;
						genelEvrakSatir2.kriter_bool3 = kriter_bool3;
						genelEvrakSatir2.kriter_bool4 = kriter_bool4;
						genelEvrakSatir2.kriter_bool5 = kriter_bool5;
						if (getBoolean59)
						{
							genelEvrakSatir2.stokhizmetkodu = getString69;
						}
						else
						{
							num2 = getInt292;
							num3 = getInt293;
							string stokhizmetkodu2 = "";
							if (num2 != 0 || num3 != 0)
							{
								stokhizmetkodu2 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							genelEvrakSatir2.stokhizmetkodu = stokhizmetkodu2;
						}
						if (getBoolean60)
						{
							genelEvrakSatir2.stokpartikodu = getString70;
						}
						else
						{
							num2 = getInt294;
							num3 = getInt295;
							string stokpartikodu2 = "";
							if (num2 != 0 || num3 != 0)
							{
								stokpartikodu2 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							genelEvrakSatir2.stokpartikodu = stokpartikodu2;
						}
						if (getBoolean61)
						{
							genelEvrakSatir2.stoklotno = getInt296;
						}
						else
						{
							num2 = getInt297;
							num3 = getInt298;
							string s5 = "";
							if (num2 != 0 || num3 != 0)
							{
								s5 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							int result102 = 0;
							int.TryParse(s5, out result102);
							genelEvrakSatir2.stoklotno = result102;
						}
						if (getBoolean58)
						{
							genelEvrakSatir2.satircinsi = (enum_SatirCinsi)getInt289;
						}
						else
						{
							num2 = getInt290;
							num3 = getInt291;
							string text192 = "";
							if (num2 != 0 || num3 != 0)
							{
								text192 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							if (text192 == getString67)
							{
								genelEvrakSatir2.satircinsi = enum_SatirCinsi.Stok;
							}
							if (text192 == getString68)
							{
								genelEvrakSatir2.satircinsi = enum_SatirCinsi.Hizmet;
							}
						}
						if (getBoolean42)
						{
							Stok stok8 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu, enum_toptan_perakende.Toptan);
							if (!(stok8.sto_kod == "") && stok8.sto_kod != null)
							{
								genelEvrakSatir2.satircinsi = enum_SatirCinsi.Stok;
							}
							else
							{
								Hizmet hizmet8 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu);
								if (!(hizmet8.hiz_kod == "") && hizmet8.hiz_kod != null)
								{
									genelEvrakSatir2.satircinsi = enum_SatirCinsi.Hizmet;
								}
							}
						}
						switch (genelEvrakSatir.satircinsi)
						{
						case enum_SatirCinsi.Stok:
						{
							Stok stok9 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
							if (stok9.sto_kod == "" || stok9.sto_kod == null)
							{
								Barkod barkodBilgisi3 = BarkodData.GetBarkodBilgisi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
								if (barkodBilgisi3.bar_stokkodu != "")
								{
									stok9 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, barkodBilgisi3.bar_stokkodu, enum_toptan_perakende.Toptan);
									if (stok9.sto_kod != "" && stok9.sto_kod != null)
									{
										genelEvrakSatir.stokhizmetkodu = stok9.sto_kod;
									}
								}
							}
							if (stok9.sto_kod == "" || stok9.sto_kod == null)
							{
								genelEvrakSatir.stokhizmetkodu = "";
							}
							break;
						}
						case enum_SatirCinsi.Hizmet:
						{
							Hizmet hizmet9 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							if (hizmet9.hiz_kod == "" || hizmet9.hiz_kod == null)
							{
								genelEvrakSatir.stokhizmetkodu = "";
							}
							break;
						}
						}
						if (getBoolean80)
						{
							genelEvrakSatir2.fiyat_fark_mi = getBoolean81;
						}
						else
						{
							num2 = getInt491;
							num3 = getInt492;
							string text193 = "";
							if (num2 != 0 || num3 != 0)
							{
								text193 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							if (text193 == getString124)
							{
								genelEvrakSatir2.fiyat_fark_mi = true;
							}
						}
						if (genelEvrakSatir2.satircinsi == enum_SatirCinsi.Stok)
						{
							switch (getInt328)
							{
							case 0:
							{
								Stok stok10 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu, enum_toptan_perakende.Toptan);
								genelEvrakSatir2.vergi_pntr = stok10.ekleme_bilgileri.vergi_pntr;
								break;
							}
							case 1:
								genelEvrakSatir2.vergi_pntr = getInt329;
								break;
							case 2:
							{
								num2 = getInt330;
								num3 = getInt331;
								string text194 = "0";
								if (num2 != 0 || num3 != 0)
								{
									text194 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result103 = 0.0;
								double.TryParse(text194.Replace(".", ","), out result103);
								num = getInt365;
								num2 = getInt366;
								num3 = getInt367;
								if (num != 0)
								{
									text194 = "";
									if (num2 != 0 || num3 != 0)
									{
										text194 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
									}
									double result104 = 0.0;
									double.TryParse(text194.Replace(".", ","), out result104);
									switch (num)
									{
									case 1:
										result103 += result104;
										break;
									case 2:
										result103 -= result104;
										break;
									case 3:
										result103 /= result104;
										break;
									case 4:
										result103 *= result104;
										break;
									}
								}
								num = getInt368;
								num2 = getInt369;
								num3 = getInt370;
								if (num != 0)
								{
									text194 = "";
									if (num2 != 0 || num3 != 0)
									{
										text194 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
									}
									double result105 = 0.0;
									double.TryParse(text194.Replace(".", ","), out result105);
									switch (num)
									{
									case 1:
										result103 += result105;
										break;
									case 2:
										result103 -= result105;
										break;
									case 3:
										result103 /= result105;
										break;
									case 4:
										result103 *= result105;
										break;
									}
								}
								genelEvrakSatir2.vergi_pntr = GenelUtility.GetKdvPntr((int)result103);
								break;
							}
							}
						}
						else
						{
							switch (getInt355)
							{
							case 0:
							{
								Hizmet hizmet10 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu);
								genelEvrakSatir2.vergi_pntr = hizmet10.hiz_KDV;
								break;
							}
							case 1:
								genelEvrakSatir2.vergi_pntr = getInt356;
								break;
							case 2:
							{
								num2 = getInt357;
								num3 = getInt358;
								string text195 = "0";
								if (num2 != 0 || num3 != 0)
								{
									text195 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result106 = 0.0;
								double.TryParse(text195.Replace(".", ","), out result106);
								num = getInt419;
								num2 = getInt420;
								num3 = getInt421;
								if (num != 0)
								{
									text195 = "";
									if (num2 != 0 || num3 != 0)
									{
										text195 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
									}
									double result107 = 0.0;
									double.TryParse(text195.Replace(".", ","), out result107);
									switch (num)
									{
									case 1:
										result106 += result107;
										break;
									case 2:
										result106 -= result107;
										break;
									case 3:
										result106 /= result107;
										break;
									case 4:
										result106 *= result107;
										break;
									}
								}
								num = getInt422;
								num2 = getInt423;
								num3 = getInt424;
								if (num != 0)
								{
									text195 = "";
									if (num2 != 0 || num3 != 0)
									{
										text195 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
									}
									double result108 = 0.0;
									double.TryParse(text195.Replace(".", ","), out result108);
									switch (num)
									{
									case 1:
										result106 += result108;
										break;
									case 2:
										result106 -= result108;
										break;
									case 3:
										result106 /= result108;
										break;
									case 4:
										result106 *= result108;
										break;
									}
								}
								genelEvrakSatir2.vergi_pntr = GenelUtility.GetKdvPntr((int)result106);
								break;
							}
							}
						}
						if (getBoolean62)
						{
							genelEvrakSatir2.faturaaciklama = getString71;
						}
						else
						{
							num2 = getInt299;
							num3 = getInt300;
							string faturaaciklama2 = "";
							if (num2 != 0 || num3 != 0)
							{
								faturaaciklama2 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							genelEvrakSatir2.faturaaciklama = faturaaciklama2;
						}
						if (getBoolean68)
						{
							genelEvrakSatir2.faturaaciklama = getString76 + genelEvrakSatir2.faturaaciklama;
						}
						if (getBoolean65)
						{
							double result109 = 1.0;
							double.TryParse(getString74.Replace(".", ","), out result109);
							genelEvrakSatir2.miktar = result109;
						}
						else
						{
							num2 = getInt324;
							num3 = getInt325;
							string text196 = "";
							if (num2 != 0 || num3 != 0)
							{
								text196 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							if (text196 != "")
							{
								double result110 = 1.0;
								double.TryParse(text196.Replace(".", ","), out result110);
								genelEvrakSatir2.miktar = result110;
							}
						}
						if (getBoolean66)
						{
							double result111 = 0.0;
							double.TryParse(getString75, out result111);
							genelEvrakSatir2.miktar2 = result111;
						}
						else
						{
							num2 = getInt326;
							num3 = getInt327;
							string text197 = "";
							if (num2 != 0 || num3 != 0)
							{
								text197 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
							}
							if (text197 != "")
							{
								double result112 = 0.0;
								double.TryParse(text197.Replace(".", ","), out result112);
								genelEvrakSatir2.miktar2 = result112;
							}
						}
						if (getBoolean63)
						{
							if (genelEvrakSatir2.satircinsi == enum_SatirCinsi.Stok)
							{
								string virgulileayrilmiskaynaklar2 = getString72;
								if (genelEvrakSatir2.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || genelEvrakSatir2.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
								{
									virgulileayrilmiskaynaklar2 = getString73;
								}
								List<enum_Fiyat_Kaynagi> fiyatKaynaklariList2 = FiyatUtility.GetFiyatKaynaklariList(virgulileayrilmiskaynaklar2);
								Stok stok11 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu, enum_toptan_perakende.Toptan);
								genelEvrakSatir2.birimfiyat = StokData.GetFiyatFromMultiSource(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, stok11.sto_kod, stok11.ekleme_bilgileri.vergi_pntr, genelEvrakSatir2.fiyatlistesi.sfl_sirano, genelEvrakSatir2.fiyatlistesi.sfl_kdvdahil, genelEvrakSatir2.cari.cari_kod, genelEvrakSatir2.cari.cari_satis_isk_kod, genelEvrakSatir2.kaynakdepo.dep_no, genelEvrakSatir2.evraktarih, fiyatKaynaklariList2, _mikrouygulamabilgileri.vergitanimlari, 0);
							}
							else
							{
								Hizmet hizmet11 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu);
								genelEvrakSatir2.birimfiyat.DovizCinsi = genelEvrakSatir2.dovizcinsi;
								genelEvrakSatir2.birimfiyat.FiyatBrut = hizmet11.BirimFiyat.FiyatBrut;
							}
						}
						else if (genelEvrakSatir2.satircinsi == enum_SatirCinsi.Stok)
						{
							if (getInt301 != 0 || getInt302 != 0)
							{
								if (getBoolean3)
								{
									double result113 = 0.0;
									double.TryParse(array4[getInt301 - 1].Replace(".", ","), out result113);
									genelEvrakSatir2.birimfiyat.FiyatBrut = result113;
								}
								else
								{
									double result114 = 0.0;
									double.TryParse(text98.Substring(getInt301 - 1, getInt302).Trim().Replace(".", ","), out result114);
									genelEvrakSatir2.birimfiyat.FiyatBrut = result114;
								}
							}
							num = getInt359;
							num2 = getInt360;
							num3 = getInt361;
							if (num != 0)
							{
								string text198 = "";
								if (num2 != 0 || num3 != 0)
								{
									text198 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result115 = 0.0;
								double.TryParse(text198.Replace(".", ","), out result115);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result115;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result115;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result115;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result115;
									break;
								}
							}
							num = getInt362;
							num2 = getInt363;
							num3 = getInt364;
							if (num != 0)
							{
								string text199 = "";
								if (num2 != 0 || num3 != 0)
								{
									text199 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result116 = 0.0;
								double.TryParse(text199.Replace(".", ","), out result116);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result116;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result116;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result116;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result116;
									break;
								}
							}
							if (getBoolean64)
							{
								double yuzde4 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir2.vergi_pntr].Yuzde;
								double num18 = 100.0;
								genelEvrakSatir2.birimfiyat.FiyatBrut = genelEvrakSatir2.birimfiyat.FiyatBrut / (yuzde4 / num18 + 1.0);
							}
							genelEvrakSatir2.birimfiyat.Iskonto_1_UygulamaSekli = getInt303;
							genelEvrakSatir2.birimfiyat.Iskonto_2_UygulamaSekli = getInt306;
							genelEvrakSatir2.birimfiyat.Iskonto_3_UygulamaSekli = getInt309;
							genelEvrakSatir2.birimfiyat.Iskonto_4_UygulamaSekli = getInt312;
							genelEvrakSatir2.birimfiyat.Iskonto_5_UygulamaSekli = getInt315;
							genelEvrakSatir2.birimfiyat.Iskonto_6_UygulamaSekli = getInt318;
							if (getInt304 != 0 || getInt305 != 0)
							{
								if (getBoolean3)
								{
									double result117 = 0.0;
									double.TryParse(array4[getInt304 - 1].Replace(".", ","), out result117);
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result117;
								}
								else
								{
									double result118 = 0.0;
									double.TryParse(text98.Substring(getInt304 - 1, getInt305).Trim().Replace(".", ","), out result118);
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result118;
								}
							}
							num = getInt377;
							num2 = getInt378;
							num3 = getInt379;
							if (num != 0)
							{
								string text200 = "";
								if (num2 != 0 || num3 != 0)
								{
									text200 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result119 = 0.0;
								double.TryParse(text200.Replace(".", ","), out result119);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result119;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result119;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result119;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result119;
									break;
								}
							}
							num = getInt380;
							num2 = getInt381;
							num3 = getInt382;
							if (num != 0)
							{
								string text201 = "";
								if (num2 != 0 || num3 != 0)
								{
									text201 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result120 = 0.0;
								double.TryParse(text201.Replace(".", ","), out result120);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result120;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result120;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result120;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result120;
									break;
								}
							}
							if (getInt307 != 0 || getInt308 != 0)
							{
								if (getBoolean3)
								{
									double result121 = 0.0;
									double.TryParse(array4[getInt307 - 1].Replace(".", ","), out result121);
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result121;
								}
								else
								{
									double result122 = 0.0;
									double.TryParse(text98.Substring(getInt307 - 1, getInt308).Trim().Replace(".", ","), out result122);
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result122;
								}
							}
							num = getInt383;
							num2 = getInt384;
							num3 = getInt385;
							if (num != 0)
							{
								string text202 = "";
								if (num2 != 0 || num3 != 0)
								{
									text202 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result123 = 0.0;
								double.TryParse(text202.Replace(".", ","), out result123);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result123;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result123;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result123;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result123;
									break;
								}
							}
							num = getInt386;
							num2 = getInt387;
							num3 = getInt388;
							if (num != 0)
							{
								string text203 = "";
								if (num2 != 0 || num3 != 0)
								{
									text203 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result124 = 0.0;
								double.TryParse(text203.Replace(".", ","), out result124);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result124;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result124;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result124;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result124;
									break;
								}
							}
							if (getInt310 != 0 || getInt311 != 0)
							{
								if (getBoolean3)
								{
									double result125 = 0.0;
									double.TryParse(array4[getInt310 - 1].Replace(".", ","), out result125);
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result125;
								}
								else
								{
									double result126 = 0.0;
									double.TryParse(text98.Substring(getInt310 - 1, getInt311).Trim().Replace(".", ","), out result126);
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result126;
								}
							}
							num = getInt389;
							num2 = getInt390;
							num3 = getInt391;
							if (num != 0)
							{
								string text204 = "";
								if (num2 != 0 || num3 != 0)
								{
									text204 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result127 = 0.0;
								double.TryParse(text204.Replace(".", ","), out result127);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result127;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result127;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result127;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result127;
									break;
								}
							}
							num = getInt392;
							num2 = getInt393;
							num3 = getInt394;
							if (num != 0)
							{
								string text205 = "";
								if (num2 != 0 || num3 != 0)
								{
									text205 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result128 = 0.0;
								double.TryParse(text205.Replace(".", ","), out result128);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result128;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result128;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result128;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result128;
									break;
								}
							}
							if (getInt313 != 0 || getInt314 != 0)
							{
								if (getBoolean3)
								{
									double result129 = 0.0;
									double.TryParse(array4[getInt313 - 1].Replace(".", ","), out result129);
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result129;
								}
								else
								{
									double result130 = 0.0;
									double.TryParse(text98.Substring(getInt313 - 1, getInt314).Trim().Replace(".", ","), out result130);
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result130;
								}
							}
							num = getInt395;
							num2 = getInt396;
							num3 = getInt397;
							if (num != 0)
							{
								string text206 = "";
								if (num2 != 0 || num3 != 0)
								{
									text206 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result131 = 0.0;
								double.TryParse(text206.Replace(".", ","), out result131);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result131;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result131;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result131;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result131;
									break;
								}
							}
							num = getInt398;
							num2 = getInt399;
							num3 = getInt400;
							if (num != 0)
							{
								string text207 = "";
								if (num2 != 0 || num3 != 0)
								{
									text207 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result132 = 0.0;
								double.TryParse(text207.Replace(".", ","), out result132);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result132;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result132;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result132;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result132;
									break;
								}
							}
							if (getInt316 != 0 || getInt317 != 0)
							{
								if (getBoolean3)
								{
									double result133 = 0.0;
									double.TryParse(array4[getInt316 - 1].Replace(".", ","), out result133);
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result133;
								}
								else
								{
									double result134 = 0.0;
									double.TryParse(text98.Substring(getInt316 - 1, getInt317).Trim().Replace(".", ","), out result134);
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result134;
								}
							}
							num = getInt401;
							num2 = getInt402;
							num3 = getInt403;
							if (num != 0)
							{
								string text208 = "";
								if (num2 != 0 || num3 != 0)
								{
									text208 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result135 = 0.0;
								double.TryParse(text208.Replace(".", ","), out result135);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result135;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result135;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result135;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result135;
									break;
								}
							}
							num = getInt404;
							num2 = getInt405;
							num3 = getInt406;
							if (num != 0)
							{
								string text209 = "";
								if (num2 != 0 || num3 != 0)
								{
									text209 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result136 = 0.0;
								double.TryParse(text209.Replace(".", ","), out result136);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result136;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result136;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result136;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result136;
									break;
								}
							}
							if (getInt319 != 0 || getInt320 != 0)
							{
								if (getBoolean3)
								{
									double result137 = 0.0;
									double.TryParse(array4[getInt319 - 1].Replace(".", ","), out result137);
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result137;
								}
								else
								{
									double result138 = 0.0;
									double.TryParse(text98.Substring(getInt319 - 1, getInt320).Trim().Replace(".", ","), out result138);
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result138;
								}
							}
							num = getInt407;
							num2 = getInt408;
							num3 = getInt409;
							if (num != 0)
							{
								string text210 = "";
								if (num2 != 0 || num3 != 0)
								{
									text210 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result139 = 0.0;
								double.TryParse(text210.Replace(".", ","), out result139);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result139;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result139;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result139;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result139;
									break;
								}
							}
							num = getInt410;
							num2 = getInt411;
							num3 = getInt412;
							if (num != 0)
							{
								string text211 = "";
								if (num2 != 0 || num3 != 0)
								{
									text211 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result140 = 0.0;
								double.TryParse(text211.Replace(".", ","), out result140);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result140;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result140;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result140;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result140;
									break;
								}
							}
							genelEvrakSatir2.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt321;
							if (getInt322 != 0 || getInt323 != 0)
							{
								if (getBoolean3)
								{
									double result141 = 0.0;
									double.TryParse(array4[getInt322 - 1].Replace(".", ","), out result141);
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar = result141;
								}
								else
								{
									double result142 = 0.0;
									double.TryParse(text98.Substring(getInt322 - 1, getInt323).Trim().Replace(".", ","), out result142);
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar = result142;
								}
							}
							num = getInt371;
							num2 = getInt372;
							num3 = getInt373;
							if (num != 0)
							{
								string text212 = "";
								if (num2 != 0 || num3 != 0)
								{
									text212 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result143 = 0.0;
								double.TryParse(text212.Replace(".", ","), out result143);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result143;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result143;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result143;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result143;
									break;
								}
							}
							num = getInt374;
							num2 = getInt375;
							num3 = getInt376;
							if (num != 0)
							{
								string text213 = "";
								if (num2 != 0 || num3 != 0)
								{
									text213 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result144 = 0.0;
								double.TryParse(text213.Replace(".", ","), out result144);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result144;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result144;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result144;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result144;
									break;
								}
							}
						}
						else
						{
							if (getInt332 != 0 || getInt333 != 0)
							{
								if (getBoolean3)
								{
									double result145 = 0.0;
									double.TryParse(array4[getInt332 - 1].Replace(".", ","), out result145);
									genelEvrakSatir2.birimfiyat.FiyatBrut = result145;
								}
								else
								{
									double result146 = 0.0;
									double.TryParse(text98.Substring(getInt332 - 1, getInt333).Trim().Replace(".", ","), out result146);
									genelEvrakSatir2.birimfiyat.FiyatBrut = result146;
								}
							}
							num = getInt413;
							num2 = getInt414;
							num3 = getInt415;
							if (num != 0)
							{
								string text214 = "";
								if (num2 != 0 || num3 != 0)
								{
									text214 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result147 = 0.0;
								double.TryParse(text214.Replace(".", ","), out result147);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result147;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result147;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result147;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result147;
									break;
								}
							}
							num = getInt416;
							num2 = getInt417;
							num3 = getInt418;
							if (num != 0)
							{
								string text215 = "";
								if (num2 != 0 || num3 != 0)
								{
									text215 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result148 = 0.0;
								double.TryParse(text215.Replace(".", ","), out result148);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result148;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result148;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result148;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result148;
									break;
								}
							}
							if (getBoolean67)
							{
								double yuzde5 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir2.vergi_pntr].Yuzde;
								double num19 = 100.0;
								genelEvrakSatir2.birimfiyat.FiyatBrut = genelEvrakSatir2.birimfiyat.FiyatBrut / (yuzde5 / num19 + 1.0);
							}
							genelEvrakSatir2.birimfiyat.Iskonto_1_UygulamaSekli = getInt334;
							genelEvrakSatir2.birimfiyat.Iskonto_2_UygulamaSekli = getInt337;
							genelEvrakSatir2.birimfiyat.Iskonto_3_UygulamaSekli = getInt340;
							genelEvrakSatir2.birimfiyat.Iskonto_4_UygulamaSekli = getInt343;
							genelEvrakSatir2.birimfiyat.Iskonto_5_UygulamaSekli = getInt346;
							genelEvrakSatir2.birimfiyat.Iskonto_6_UygulamaSekli = getInt349;
							if (getInt335 != 0 || getInt336 != 0)
							{
								if (getBoolean3)
								{
									double result149 = 0.0;
									double.TryParse(array4[getInt335 - 1].Replace(".", ","), out result149);
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result149;
								}
								else
								{
									double result150 = 0.0;
									double.TryParse(text98.Substring(getInt335 - 1, getInt336).Trim().Replace(".", ","), out result150);
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result150;
								}
							}
							num = getInt431;
							num2 = getInt432;
							num3 = getInt433;
							if (num != 0)
							{
								string text216 = "";
								if (num2 != 0 || num3 != 0)
								{
									text216 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result151 = 0.0;
								double.TryParse(text216.Replace(".", ","), out result151);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result151;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result151;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result151;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result151;
									break;
								}
							}
							num = getInt434;
							num2 = getInt435;
							num3 = getInt436;
							if (num != 0)
							{
								string text217 = "";
								if (num2 != 0 || num3 != 0)
								{
									text217 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result152 = 0.0;
								double.TryParse(text217.Replace(".", ","), out result152);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result152;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result152;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result152;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result152;
									break;
								}
							}
							if (getInt338 != 0 || getInt339 != 0)
							{
								if (getBoolean3)
								{
									double result153 = 0.0;
									double.TryParse(array4[getInt338 - 1].Replace(".", ","), out result153);
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result153;
								}
								else
								{
									double result154 = 0.0;
									double.TryParse(text98.Substring(getInt338 - 1, getInt339).Trim().Replace(".", ","), out result154);
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result154;
								}
							}
							num = getInt437;
							num2 = getInt438;
							num3 = getInt439;
							if (num != 0)
							{
								string text218 = "";
								if (num2 != 0 || num3 != 0)
								{
									text218 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result155 = 0.0;
								double.TryParse(text218.Replace(".", ","), out result155);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result155;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result155;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result155;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result155;
									break;
								}
							}
							num = getInt440;
							num2 = getInt441;
							num3 = getInt442;
							if (num != 0)
							{
								string text219 = "";
								if (num2 != 0 || num3 != 0)
								{
									text219 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result156 = 0.0;
								double.TryParse(text219.Replace(".", ","), out result156);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result156;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result156;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result156;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result156;
									break;
								}
							}
							if (getInt341 != 0 || getInt342 != 0)
							{
								if (getBoolean3)
								{
									double result157 = 0.0;
									double.TryParse(array4[getInt341 - 1].Replace(".", ","), out result157);
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result157;
								}
								else
								{
									double result158 = 0.0;
									double.TryParse(text98.Substring(getInt341 - 1, getInt342).Trim().Replace(".", ","), out result158);
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result158;
								}
							}
							num = getInt443;
							num2 = getInt444;
							num3 = getInt445;
							if (num != 0)
							{
								string text220 = "";
								if (num2 != 0 || num3 != 0)
								{
									text220 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result159 = 0.0;
								double.TryParse(text220.Replace(".", ","), out result159);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result159;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result159;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result159;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result159;
									break;
								}
							}
							num = getInt446;
							num2 = getInt447;
							num3 = getInt448;
							if (num != 0)
							{
								string text221 = "";
								if (num2 != 0 || num3 != 0)
								{
									text221 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result160 = 0.0;
								double.TryParse(text221.Replace(".", ","), out result160);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result160;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result160;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result160;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result160;
									break;
								}
							}
							if (getInt344 != 0 || getInt345 != 0)
							{
								if (getBoolean3)
								{
									double result161 = 0.0;
									double.TryParse(array4[getInt344 - 1].Replace(".", ","), out result161);
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result161;
								}
								else
								{
									double result162 = 0.0;
									double.TryParse(text98.Substring(getInt344 - 1, getInt345).Trim().Replace(".", ","), out result162);
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result162;
								}
							}
							num = getInt449;
							num2 = getInt450;
							num3 = getInt451;
							if (num != 0)
							{
								string text222 = "";
								if (num2 != 0 || num3 != 0)
								{
									text222 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result163 = 0.0;
								double.TryParse(text222.Replace(".", ","), out result163);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result163;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result163;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result163;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result163;
									break;
								}
							}
							num = getInt452;
							num2 = getInt453;
							num3 = getInt454;
							if (num != 0)
							{
								string text223 = "";
								if (num2 != 0 || num3 != 0)
								{
									text223 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result164 = 0.0;
								double.TryParse(text223.Replace(".", ","), out result164);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result164;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result164;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result164;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result164;
									break;
								}
							}
							if (getInt347 != 0 || getInt348 != 0)
							{
								if (getBoolean3)
								{
									double result165 = 0.0;
									double.TryParse(array4[getInt347 - 1].Replace(".", ","), out result165);
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result165;
								}
								else
								{
									double result166 = 0.0;
									double.TryParse(text98.Substring(getInt347 - 1, getInt348).Trim().Replace(".", ","), out result166);
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result166;
								}
							}
							num = getInt455;
							num2 = getInt456;
							num3 = getInt457;
							if (num != 0)
							{
								string text224 = "";
								if (num2 != 0 || num3 != 0)
								{
									text224 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result167 = 0.0;
								double.TryParse(text224.Replace(".", ","), out result167);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result167;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result167;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result167;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result167;
									break;
								}
							}
							num = getInt458;
							num2 = getInt459;
							num3 = getInt460;
							if (num != 0)
							{
								string text225 = "";
								if (num2 != 0 || num3 != 0)
								{
									text225 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result168 = 0.0;
								double.TryParse(text225.Replace(".", ","), out result168);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result168;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result168;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result168;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result168;
									break;
								}
							}
							if (getInt350 != 0 || getInt351 != 0)
							{
								if (getBoolean3)
								{
									double result169 = 0.0;
									double.TryParse(array4[getInt350 - 1].Replace(".", ","), out result169);
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result169;
								}
								else
								{
									double result170 = 0.0;
									double.TryParse(text98.Substring(getInt350 - 1, getInt351).Trim().Replace(".", ","), out result170);
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result170;
								}
							}
							num = getInt461;
							num2 = getInt462;
							num3 = getInt463;
							if (num != 0)
							{
								string text226 = "";
								if (num2 != 0 || num3 != 0)
								{
									text226 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result171 = 0.0;
								double.TryParse(text226.Replace(".", ","), out result171);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result171;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result171;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result171;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result171;
									break;
								}
							}
							num = getInt464;
							num2 = getInt465;
							num3 = getInt466;
							if (num != 0)
							{
								string text227 = "";
								if (num2 != 0 || num3 != 0)
								{
									text227 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result172 = 0.0;
								double.TryParse(text227.Replace(".", ","), out result172);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result172;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result172;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result172;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result172;
									break;
								}
							}
							genelEvrakSatir2.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt352;
							if (getInt353 != 0 || getInt354 != 0)
							{
								if (getBoolean3)
								{
									double result173 = 0.0;
									double.TryParse(array4[getInt353 - 1].Replace(".", ","), out result173);
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar = result173;
								}
								else
								{
									double result174 = 0.0;
									double.TryParse(text98.Substring(getInt353 - 1, getInt354).Trim().Replace(".", ","), out result174);
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar = result174;
								}
							}
							num = getInt425;
							num2 = getInt426;
							num3 = getInt427;
							if (num != 0)
							{
								string text228 = "";
								if (num2 != 0 || num3 != 0)
								{
									text228 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result175 = 0.0;
								double.TryParse(text228.Replace(".", ","), out result175);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result175;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result175;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result175;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result175;
									break;
								}
							}
							num = getInt428;
							num2 = getInt429;
							num3 = getInt430;
							if (num != 0)
							{
								string text229 = "";
								if (num2 != 0 || num3 != 0)
								{
									text229 = ((!getBoolean3) ? text98.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
								}
								double result176 = 0.0;
								double.TryParse(text229.Replace(".", ","), out result176);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result176;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result176;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result176;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result176;
									break;
								}
							}
						}
						genelEvrakSatir2.birimfiyat.OtvVergiPntr = getInt493;
						if (genelEvrakSatir2.birimfiyat.FiyatBrut != 0.0)
						{
							if (genelEvrakSatir2.birimfiyat.FiyatBrut < 0.0 && getBoolean70)
							{
								genelEvrakSatir2.evraktipi = (enum_GenelEvrakTipleri)getInt471;
							}
							if (genelEvrakSatir2.birimfiyat.FiyatBrut < 0.0)
							{
								genelEvrakSatir2.birimfiyat.FiyatBrut *= -1.0;
							}
							bindingList.Add(genelEvrakSatir2);
						}
					}
				}
				num4++;
			}
		}
		catch (Exception ex)
		{
			if (MessageBox.Show("Bilgiler çekilirken hata oluştu. Lütfen ayarları kontrol ediniz. Hata oluşan satır : " + num4 + ". Devam etmek istiyor musunuz? Hata açıklaması : " + ex.ToString(), "HATA", MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				return;
			}
		}
		for (int j = 0; j < bindingList.Count; j++)
		{
			bindingList[j] = KriterleriUygula(bindingList[j]);
		}
		foreach (GenelEvrakSatir item11 in bindingList)
		{
			if (item11 != null)
			{
				_satirlar._satirlar.Add(item11);
			}
		}
		if (flag)
		{
			OtomatikEvrakSiraNoVer(satirBirlestir);
		}
	}

	private void sb_Sql_Al_Click(object sender, EventArgs e)
	{
		SqlImport sqlImport = new SqlImport(_mikrouygulamabilgileri);
		if (sqlImport.ShowDialog() == DialogResult.OK)
		{
			SqlImport(sqlImport.cb_aktarimayarlari.SelectedValue.ToString(), sqlImport.dateEdit_baslangic_tarihi.DateTime, sqlImport.dateEdit_bitis_tarihi.DateTime);
		}
	}

	private void SqlImport(string AktarimAyarlariAdi, DateTime baslangic_tarihi, DateTime bitis_tarihi)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		BindingList<GenelEvrakSatir> bindingList = new BindingList<GenelEvrakSatir>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		bool satirBirlestir = true;
		try
		{
			Parametreler parametreler = ParametrelerDefault.GenelAktarimSqlSablon(AktarimAyarlariAdi);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "GenelAktarim", "", "SqlAktarimSablon", AktarimAyarlariAdi);
			string getString = parametreler._GetParametre("KriterListesi")._GetString;
			string getString2 = parametreler._GetParametre("SqlServer")._GetString;
			_ = parametreler._GetParametre("SqlServerPort")._GetString;
			string getString3 = parametreler._GetParametre("SqlUserName")._GetString;
			string getString4 = parametreler._GetParametre("SqlPassword")._GetString;
			string getString5 = parametreler._GetParametre("DBName")._GetString;
			string getString6 = parametreler._GetParametre("Query")._GetString;
			int getInt = parametreler._GetParametre("firma_no")._GetInt;
			int getInt2 = parametreler._GetParametre("sube_no")._GetInt;
			int getInt3 = parametreler._GetParametre("dbc_no")._GetInt;
			int getInt4 = parametreler._GetParametre("evrak_tarihi_baslangic")._GetInt;
			int getInt5 = parametreler._GetParametre("belge_no_baslangic")._GetInt;
			int getInt6 = parametreler._GetParametre("belge_tarihi_baslangic")._GetInt;
			int getInt7 = parametreler._GetParametre("kur_baslangic")._GetInt;
			int getInt8 = parametreler._GetParametre("sevk_teslim_tarihi_baslangic")._GetInt;
			bool getBoolean = parametreler._GetParametre("evrak_tipi_sabit_kullan")._GetBoolean;
			int getInt9 = parametreler._GetParametre("evrak_tipi_sabit_deger")._GetInt;
			int getInt10 = parametreler._GetParametre("evrak_tipi_baslangic")._GetInt;
			string getString7 = parametreler._GetParametre("evrak_tipi_veri_satis_faturasi")._GetString;
			string getString8 = parametreler._GetParametre("evrak_tipi_veri_satis_irsaliyesi")._GetString;
			string getString9 = parametreler._GetParametre("evrak_tipi_veri_alis_faturasi")._GetString;
			string getString10 = parametreler._GetParametre("evrak_tipi_veri_alis_irsaliyesi")._GetString;
			string getString11 = parametreler._GetParametre("evrak_tipi_veri_alinan_siparis")._GetString;
			string getString12 = parametreler._GetParametre("evrak_tipi_veri_depolar_arasi_sevk")._GetString;
			bool getBoolean2 = parametreler._GetParametre("normal_iade_sabit_kullan")._GetBoolean;
			int getInt11 = parametreler._GetParametre("normal_iade_sabit_deger")._GetInt;
			int getInt12 = parametreler._GetParametre("normal_iade_baslangic")._GetInt;
			string getString13 = parametreler._GetParametre("normal_iade_veri_normal")._GetString;
			string getString14 = parametreler._GetParametre("normal_iade_veri_iade")._GetString;
			bool getBoolean3 = parametreler._GetParametre("satir_cinsi_sabit_kullan")._GetBoolean;
			int getInt13 = parametreler._GetParametre("satir_cinsi_sabit_deger")._GetInt;
			int getInt14 = parametreler._GetParametre("satir_cinsi_baslangic")._GetInt;
			string getString15 = parametreler._GetParametre("satir_cinsi_veri_stok")._GetString;
			string getString16 = parametreler._GetParametre("satir_cinsi_veri_hizmet")._GetString;
			bool getBoolean4 = parametreler._GetParametre("satir_hesap_kodu_sabit_kullan")._GetBoolean;
			string getString17 = parametreler._GetParametre("satir_hesap_kodu_sabit_deger")._GetString;
			int getInt15 = parametreler._GetParametre("satir_hesap_kodu_baslangic")._GetInt;
			bool getBoolean5 = parametreler._GetParametre("satir_parti_kodu_sabit_kullan")._GetBoolean;
			string getString18 = parametreler._GetParametre("satir_parti_kodu_sabit_deger")._GetString;
			int getInt16 = parametreler._GetParametre("satir_parti_kodu_baslangic")._GetInt;
			bool getBoolean6 = parametreler._GetParametre("satir_lot_no_sabit_kullan")._GetBoolean;
			int getInt17 = parametreler._GetParametre("satir_lot_no_sabit_deger")._GetInt;
			int getInt18 = parametreler._GetParametre("satir_lot_no_baslangic")._GetInt;
			bool getBoolean7 = parametreler._GetParametre("cari_kod_sabit_kullan")._GetBoolean;
			string getString19 = parametreler._GetParametre("cari_kod_sabit_deger")._GetString;
			int getInt19 = parametreler._GetParametre("cari_kod_baslangic")._GetInt;
			string getString20 = parametreler._GetParametre("cari_arama_secenekleri")._GetString;
			_ = parametreler._GetParametre("cari_unvan_turkce_karakterleri_kaldir")._GetBoolean;
			int getInt20 = parametreler._GetParametre("cari_unvan_baslangic")._GetInt;
			int getInt21 = parametreler._GetParametre("cari_unvan2_baslangic")._GetInt;
			int getInt22 = parametreler._GetParametre("cari_vergi_no_baslangic")._GetInt;
			int getInt23 = parametreler._GetParametre("cari_tc_kimlik_no_baslangic")._GetInt;
			int getInt24 = parametreler._GetParametre("cari_vergi_dairesi_baslangic")._GetInt;
			int getInt25 = parametreler._GetParametre("cari_banka_hesap_no_baslangic")._GetInt;
			int getInt26 = parametreler._GetParametre("cari_adres_baslangic")._GetInt;
			int getInt27 = parametreler._GetParametre("cari_mahalle_baslangic")._GetInt;
			int getInt28 = parametreler._GetParametre("cari_ilce_baslangic")._GetInt;
			int getInt29 = parametreler._GetParametre("cari_il_baslangic")._GetInt;
			int getInt30 = parametreler._GetParametre("cari_ulke_baslangic")._GetInt;
			int getInt31 = parametreler._GetParametre("cari_posta_kodu_baslangic")._GetInt;
			int getInt32 = parametreler._GetParametre("cari_telefon_baslangic")._GetInt;
			int getInt33 = parametreler._GetParametre("cari_eposta_baslangic")._GetInt;
			bool getBoolean8 = parametreler._GetParametre("kapama_sekli_sabit_kullan")._GetBoolean;
			int getInt34 = parametreler._GetParametre("kapama_sekli_sabit_deger")._GetInt;
			int getInt35 = parametreler._GetParametre("kapama_sekli_baslangic")._GetInt;
			string getString21 = parametreler._GetParametre("kapama_sekli_veri_acik_hesap")._GetString;
			string getString22 = parametreler._GetParametre("kapama_sekli_veri_kasadan_kapanacak")._GetString;
			string getString23 = parametreler._GetParametre("kapama_sekli_veri_bankadan_kapanacak")._GetString;
			string getString24 = parametreler._GetParametre("kapama_sekli_veri_cari_personelden_kapanacak")._GetString;
			bool getBoolean9 = parametreler._GetParametre("kapama_hesap_kodu_sabit_kullan")._GetBoolean;
			string getString25 = parametreler._GetParametre("kapama_hesap_kodu_sabit_deger")._GetString;
			int getInt36 = parametreler._GetParametre("kapama_hesap_kodu_baslangic")._GetInt;
			bool getBoolean10 = parametreler._GetParametre("proje_kodu_sabit_kullan")._GetBoolean;
			string getString26 = parametreler._GetParametre("proje_kodu_sabit_deger")._GetString;
			int getInt37 = parametreler._GetParametre("proje_kodu_baslangic")._GetInt;
			bool getBoolean11 = parametreler._GetParametre("sor_mer_kodu_sabit_kullan")._GetBoolean;
			string getString27 = parametreler._GetParametre("sor_mer_kodu_sabit_deger")._GetString;
			int getInt38 = parametreler._GetParametre("sor_mer_kodu_baslangic")._GetInt;
			bool getBoolean12 = parametreler._GetParametre("plasiyer_kodu_sabit_kullan")._GetBoolean;
			bool getBoolean13 = parametreler._GetParametre("plasiyer_kodu_cariden_kullan")._GetBoolean;
			string getString28 = parametreler._GetParametre("plasiyer_kodu_sabit_deger")._GetString;
			int getInt39 = parametreler._GetParametre("plasiyer_kodu_baslangic")._GetInt;
			bool getBoolean14 = parametreler._GetParametre("satir_aciklama_sabit_kullan")._GetBoolean;
			string getString29 = parametreler._GetParametre("satir_aciklama_sabit_deger")._GetString;
			int getInt40 = parametreler._GetParametre("satir_aciklama_baslangic")._GetInt;
			bool getBoolean15 = parametreler._GetParametre("aciklama1_sabit_kullan")._GetBoolean;
			string getString30 = parametreler._GetParametre("aciklama1_sabit_deger")._GetString;
			int getInt41 = parametreler._GetParametre("aciklama1_baslangic")._GetInt;
			bool getBoolean16 = parametreler._GetParametre("aciklama2_sabit_kullan")._GetBoolean;
			string getString31 = parametreler._GetParametre("aciklama2_sabit_deger")._GetString;
			int getInt42 = parametreler._GetParametre("aciklama2_baslangic")._GetInt;
			bool getBoolean17 = parametreler._GetParametre("aciklama3_sabit_kullan")._GetBoolean;
			string getString32 = parametreler._GetParametre("aciklama3_sabit_deger")._GetString;
			int getInt43 = parametreler._GetParametre("aciklama3_baslangic")._GetInt;
			bool getBoolean18 = parametreler._GetParametre("aciklama4_sabit_kullan")._GetBoolean;
			string getString33 = parametreler._GetParametre("aciklama4_sabit_deger")._GetString;
			int getInt44 = parametreler._GetParametre("aciklama4_baslangic")._GetInt;
			bool getBoolean19 = parametreler._GetParametre("aciklama5_sabit_kullan")._GetBoolean;
			string getString34 = parametreler._GetParametre("aciklama5_sabit_deger")._GetString;
			int getInt45 = parametreler._GetParametre("aciklama5_baslangic")._GetInt;
			bool getBoolean20 = parametreler._GetParametre("aciklama6_sabit_kullan")._GetBoolean;
			string getString35 = parametreler._GetParametre("aciklama6_sabit_deger")._GetString;
			int getInt46 = parametreler._GetParametre("aciklama6_baslangic")._GetInt;
			bool getBoolean21 = parametreler._GetParametre("aciklama7_sabit_kullan")._GetBoolean;
			string getString36 = parametreler._GetParametre("aciklama7_sabit_deger")._GetString;
			int getInt47 = parametreler._GetParametre("aciklama7_baslangic")._GetInt;
			bool getBoolean22 = parametreler._GetParametre("aciklama8_sabit_kullan")._GetBoolean;
			string getString37 = parametreler._GetParametre("aciklama8_sabit_deger")._GetString;
			int getInt48 = parametreler._GetParametre("aciklama8_baslangic")._GetInt;
			bool getBoolean23 = parametreler._GetParametre("aciklama9_sabit_kullan")._GetBoolean;
			string getString38 = parametreler._GetParametre("aciklama9_sabit_deger")._GetString;
			int getInt49 = parametreler._GetParametre("aciklama9_baslangic")._GetInt;
			bool getBoolean24 = parametreler._GetParametre("aciklama10_sabit_kullan")._GetBoolean;
			string getString39 = parametreler._GetParametre("aciklama10_sabit_deger")._GetString;
			int getInt50 = parametreler._GetParametre("aciklama10_baslangic")._GetInt;
			bool getBoolean25 = parametreler._GetParametre("ozel_alan_1_sabit_kullan")._GetBoolean;
			string getString40 = parametreler._GetParametre("ozel_alan_1_sabit_deger")._GetString;
			int getInt51 = parametreler._GetParametre("ozel_alan_1_baslangic")._GetInt;
			bool getBoolean26 = parametreler._GetParametre("ozel_alan_2_sabit_kullan")._GetBoolean;
			string getString41 = parametreler._GetParametre("ozel_alan_2_sabit_deger")._GetString;
			int getInt52 = parametreler._GetParametre("ozel_alan_2_baslangic")._GetInt;
			bool getBoolean27 = parametreler._GetParametre("ozel_alan_3_sabit_kullan")._GetBoolean;
			string getString42 = parametreler._GetParametre("ozel_alan_3_sabit_deger")._GetString;
			int getInt53 = parametreler._GetParametre("ozel_alan_3_baslangic")._GetInt;
			bool getBoolean28 = parametreler._GetParametre("birim_fiyat_mikroda_tanimli_fiyati_kullan")._GetBoolean;
			string getString43 = parametreler._GetParametre("birim_fiyat_satis_fiyat_kaynaklari")._GetString;
			string getString44 = parametreler._GetParametre("birim_fiyat_alis_fiyat_kaynaklari")._GetString;
			bool getBoolean29 = parametreler._GetParametre("stok_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt54 = parametreler._GetParametre("stok_birim_fiyat_baslangic")._GetInt;
			int getInt55 = parametreler._GetParametre("stok_iskonto_1_uygulama_sekli")._GetInt;
			int getInt56 = parametreler._GetParametre("stok_iskonto_1_baslangic")._GetInt;
			int getInt57 = parametreler._GetParametre("stok_iskonto_2_uygulama_sekli")._GetInt;
			int getInt58 = parametreler._GetParametre("stok_iskonto_2_baslangic")._GetInt;
			int getInt59 = parametreler._GetParametre("stok_iskonto_3_uygulama_sekli")._GetInt;
			int getInt60 = parametreler._GetParametre("stok_iskonto_3_baslangic")._GetInt;
			int getInt61 = parametreler._GetParametre("stok_iskonto_4_uygulama_sekli")._GetInt;
			int getInt62 = parametreler._GetParametre("stok_iskonto_4_baslangic")._GetInt;
			int getInt63 = parametreler._GetParametre("stok_iskonto_5_uygulama_sekli")._GetInt;
			int getInt64 = parametreler._GetParametre("stok_iskonto_5_baslangic")._GetInt;
			int getInt65 = parametreler._GetParametre("stok_iskonto_6_uygulama_sekli")._GetInt;
			int getInt66 = parametreler._GetParametre("stok_iskonto_6_baslangic")._GetInt;
			int getInt67 = parametreler._GetParametre("stok_otv_uygulama_sekli")._GetInt;
			int getInt68 = parametreler._GetParametre("stok_otv_tutar_yuzde_baslangic")._GetInt;
			bool getBoolean30 = parametreler._GetParametre("miktar_sabit_kullan")._GetBoolean;
			string getString45 = parametreler._GetParametre("miktar_sabit_deger")._GetString;
			int getInt69 = parametreler._GetParametre("miktar_baslangic")._GetInt;
			bool getBoolean31 = parametreler._GetParametre("miktar2_sabit_kullan")._GetBoolean;
			string getString46 = parametreler._GetParametre("miktar2_sabit_deger")._GetString;
			int getInt70 = parametreler._GetParametre("miktar2_baslangic")._GetInt;
			bool getBoolean32 = parametreler._GetParametre("evrak_seri_sabit_kullan")._GetBoolean;
			string getString47 = parametreler._GetParametre("evrak_seri_sabit_deger_satis_faturasi")._GetString;
			string getString48 = parametreler._GetParametre("evrak_seri_sabit_deger_satis_irsaliyesi")._GetString;
			string getString49 = parametreler._GetParametre("evrak_seri_sabit_deger_alis_faturasi")._GetString;
			string getString50 = parametreler._GetParametre("evrak_seri_sabit_deger_alis_irsaliyesi")._GetString;
			string getString51 = parametreler._GetParametre("evrak_seri_sabit_deger_alinan_siparis")._GetString;
			string getString52 = parametreler._GetParametre("evrak_seri_sabit_deger_depolar_arasi_sevk")._GetString;
			string getString53 = parametreler._GetParametre("evrak_seri_sabit_deger_efatura_satis_faturasi")._GetString;
			string getString54 = parametreler._GetParametre("evrak_seri_sabit_deger_efatura_alis_faturasi")._GetString;
			bool getBoolean33 = parametreler._GetParametre("evrak_seri_efatura_carisi_mi_kontrol_et")._GetBoolean;
			int getInt71 = parametreler._GetParametre("evrak_seri_baslangic")._GetInt;
			int getInt72 = parametreler._GetParametre("evrak_sira_baslangic")._GetInt;
			int getInt73 = parametreler._GetParametre("stok_kdv_orani_bakilacak_yer")._GetInt;
			int getInt74 = parametreler._GetParametre("stok_kdv_orani_sabit_deger")._GetInt;
			int getInt75 = parametreler._GetParametre("stok_kdv_orani_baslangic")._GetInt;
			bool getBoolean34 = parametreler._GetParametre("hizmet_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt76 = parametreler._GetParametre("hizmet_birim_fiyat_baslangic")._GetInt;
			int getInt77 = parametreler._GetParametre("hizmet_iskonto_1_uygulama_sekli")._GetInt;
			int getInt78 = parametreler._GetParametre("hizmet_iskonto_1_baslangic")._GetInt;
			int getInt79 = parametreler._GetParametre("hizmet_iskonto_2_uygulama_sekli")._GetInt;
			int getInt80 = parametreler._GetParametre("hizmet_iskonto_2_baslangic")._GetInt;
			int getInt81 = parametreler._GetParametre("hizmet_iskonto_3_uygulama_sekli")._GetInt;
			int getInt82 = parametreler._GetParametre("hizmet_iskonto_3_baslangic")._GetInt;
			int getInt83 = parametreler._GetParametre("hizmet_iskonto_4_uygulama_sekli")._GetInt;
			int getInt84 = parametreler._GetParametre("hizmet_iskonto_4_baslangic")._GetInt;
			int getInt85 = parametreler._GetParametre("hizmet_iskonto_5_uygulama_sekli")._GetInt;
			int getInt86 = parametreler._GetParametre("hizmet_iskonto_5_baslangic")._GetInt;
			int getInt87 = parametreler._GetParametre("hizmet_iskonto_6_uygulama_sekli")._GetInt;
			int getInt88 = parametreler._GetParametre("hizmet_iskonto_6_baslangic")._GetInt;
			int getInt89 = parametreler._GetParametre("hizmet_otv_uygulama_sekli")._GetInt;
			int getInt90 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_baslangic")._GetInt;
			int getInt91 = parametreler._GetParametre("hizmet_kdv_orani_bakilacak_yer")._GetInt;
			int getInt92 = parametreler._GetParametre("hizmet_kdv_orani_sabit_deger")._GetInt;
			int getInt93 = parametreler._GetParametre("hizmet_kdv_orani_baslangic")._GetInt;
			bool getBoolean35 = parametreler._GetParametre("depo_no_sabit_deger_kullan")._GetBoolean;
			int getInt94 = parametreler._GetParametre("depo_no_sabit_deger")._GetInt;
			bool getBoolean36 = parametreler._GetParametre("depo_no_cari_karttan_getir")._GetBoolean;
			int getInt95 = parametreler._GetParametre("depo_no_baslangic")._GetInt;
			bool getBoolean37 = parametreler._GetParametre("hedef_depo_no_sabit_deger_kullan")._GetBoolean;
			int getInt96 = parametreler._GetParametre("hedef_depo_no_sabit_deger")._GetInt;
			int getInt97 = parametreler._GetParametre("hedef_depo_no_baslangic")._GetInt;
			bool getBoolean38 = parametreler._GetParametre("satircinsiniotomatikbul")._GetBoolean;
			bool getBoolean39 = parametreler._GetParametre("cari_kodu_on_ek_kullan")._GetBoolean;
			string getString55 = parametreler._GetParametre("cari_kodu_on_ek_satis")._GetString;
			string getString56 = parametreler._GetParametre("cari_kodu_on_ek_alis")._GetString;
			bool getBoolean40 = parametreler._GetParametre("satir_aciklama_on_ek_kullan")._GetBoolean;
			string getString57 = parametreler._GetParametre("satir_aciklama_on_ek_deger")._GetString;
			bool getBoolean41 = parametreler._GetParametre("aciklama1_on_ek_kullan")._GetBoolean;
			string getString58 = parametreler._GetParametre("aciklama1_on_ek_deger")._GetString;
			bool getBoolean42 = parametreler._GetParametre("aciklama2_on_ek_kullan")._GetBoolean;
			string getString59 = parametreler._GetParametre("aciklama2_on_ek_deger")._GetString;
			bool getBoolean43 = parametreler._GetParametre("aciklama3_on_ek_kullan")._GetBoolean;
			string getString60 = parametreler._GetParametre("aciklama3_on_ek_deger")._GetString;
			bool getBoolean44 = parametreler._GetParametre("aciklama4_on_ek_kullan")._GetBoolean;
			string getString61 = parametreler._GetParametre("aciklama4_on_ek_deger")._GetString;
			bool getBoolean45 = parametreler._GetParametre("aciklama5_on_ek_kullan")._GetBoolean;
			string getString62 = parametreler._GetParametre("aciklama5_on_ek_deger")._GetString;
			bool getBoolean46 = parametreler._GetParametre("aciklama6_on_ek_kullan")._GetBoolean;
			string getString63 = parametreler._GetParametre("aciklama6_on_ek_deger")._GetString;
			bool getBoolean47 = parametreler._GetParametre("aciklama7_on_ek_kullan")._GetBoolean;
			string getString64 = parametreler._GetParametre("aciklama7_on_ek_deger")._GetString;
			bool getBoolean48 = parametreler._GetParametre("aciklama8_on_ek_kullan")._GetBoolean;
			string getString65 = parametreler._GetParametre("aciklama8_on_ek_deger")._GetString;
			bool getBoolean49 = parametreler._GetParametre("aciklama9_on_ek_kullan")._GetBoolean;
			string getString66 = parametreler._GetParametre("aciklama9_on_ek_deger")._GetString;
			bool getBoolean50 = parametreler._GetParametre("aciklama10_on_ek_kullan")._GetBoolean;
			string getString67 = parametreler._GetParametre("aciklama10_on_ek_deger")._GetString;
			bool getBoolean51 = parametreler._GetParametre("ticaret_turu_sabit_kullan")._GetBoolean;
			int getInt98 = parametreler._GetParametre("ticaret_turu_sabit_deger")._GetInt;
			bool getBoolean52 = parametreler._GetParametre("cari_il_bilgisi_plaka_kodu")._GetBoolean;
			int getInt99 = parametreler._GetParametre("stok_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt100 = parametreler._GetParametre("stok_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt101 = parametreler._GetParametre("stok_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt102 = parametreler._GetParametre("stok_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt103 = parametreler._GetParametre("stok_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt104 = parametreler._GetParametre("stok_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt105 = parametreler._GetParametre("stok_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt106 = parametreler._GetParametre("stok_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt107 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt108 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt109 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt110 = parametreler._GetParametre("stok_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt111 = parametreler._GetParametre("stok_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt112 = parametreler._GetParametre("stok_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt113 = parametreler._GetParametre("stok_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt114 = parametreler._GetParametre("stok_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt115 = parametreler._GetParametre("stok_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt116 = parametreler._GetParametre("stok_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt117 = parametreler._GetParametre("stok_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt118 = parametreler._GetParametre("stok_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt119 = parametreler._GetParametre("stok_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt120 = parametreler._GetParametre("stok_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt121 = parametreler._GetParametre("stok_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt122 = parametreler._GetParametre("stok_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt123 = parametreler._GetParametre("stok_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt124 = parametreler._GetParametre("stok_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt125 = parametreler._GetParametre("stok_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt126 = parametreler._GetParametre("stok_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt127 = parametreler._GetParametre("stok_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt128 = parametreler._GetParametre("stok_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt129 = parametreler._GetParametre("stok_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt130 = parametreler._GetParametre("stok_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt131 = parametreler._GetParametre("stok_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt132 = parametreler._GetParametre("stok_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt133 = parametreler._GetParametre("stok_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt134 = parametreler._GetParametre("stok_iskonto_6_islem_2_baslangic")._GetInt;
			int getInt135 = parametreler._GetParametre("hizmet_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt136 = parametreler._GetParametre("hizmet_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt137 = parametreler._GetParametre("hizmet_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt138 = parametreler._GetParametre("hizmet_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt139 = parametreler._GetParametre("hizmet_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt140 = parametreler._GetParametre("hizmet_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt141 = parametreler._GetParametre("hizmet_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt142 = parametreler._GetParametre("hizmet_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt143 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt144 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt145 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt146 = parametreler._GetParametre("hizmet_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt147 = parametreler._GetParametre("hizmet_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt148 = parametreler._GetParametre("hizmet_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt149 = parametreler._GetParametre("hizmet_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt150 = parametreler._GetParametre("hizmet_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt151 = parametreler._GetParametre("hizmet_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt152 = parametreler._GetParametre("hizmet_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt153 = parametreler._GetParametre("hizmet_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt154 = parametreler._GetParametre("hizmet_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt155 = parametreler._GetParametre("hizmet_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt156 = parametreler._GetParametre("hizmet_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt157 = parametreler._GetParametre("hizmet_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt158 = parametreler._GetParametre("hizmet_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt159 = parametreler._GetParametre("hizmet_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt160 = parametreler._GetParametre("hizmet_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt161 = parametreler._GetParametre("hizmet_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt162 = parametreler._GetParametre("hizmet_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt163 = parametreler._GetParametre("hizmet_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt164 = parametreler._GetParametre("hizmet_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt165 = parametreler._GetParametre("hizmet_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt166 = parametreler._GetParametre("hizmet_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt167 = parametreler._GetParametre("hizmet_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt168 = parametreler._GetParametre("hizmet_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt169 = parametreler._GetParametre("hizmet_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt170 = parametreler._GetParametre("hizmet_iskonto_6_islem_2_baslangic")._GetInt;
			bool getBoolean53 = parametreler._GetParametre("ikinci_satir_bilgisini_kullan")._GetBoolean;
			bool getBoolean54 = parametreler._GetParametre("ikinci_satir_cinsi_sabit_kullan")._GetBoolean;
			int getInt171 = parametreler._GetParametre("ikinci_satir_cinsi_sabit_deger")._GetInt;
			int getInt172 = parametreler._GetParametre("ikinci_satir_cinsi_baslangic")._GetInt;
			string getString68 = parametreler._GetParametre("ikinci_satir_cinsi_veri_stok")._GetString;
			string getString69 = parametreler._GetParametre("ikinci_satir_cinsi_veri_hizmet")._GetString;
			bool getBoolean55 = parametreler._GetParametre("ikinci_satir_hesap_kodu_sabit_kullan")._GetBoolean;
			string getString70 = parametreler._GetParametre("ikinci_satir_hesap_kodu_sabit_deger")._GetString;
			int getInt173 = parametreler._GetParametre("ikinci_satir_hesap_kodu_baslangic")._GetInt;
			bool getBoolean56 = parametreler._GetParametre("ikinci_satir_parti_kodu_sabit_kullan")._GetBoolean;
			string getString71 = parametreler._GetParametre("ikinci_satir_parti_kodu_sabit_deger")._GetString;
			int getInt174 = parametreler._GetParametre("ikinci_satir_parti_kodu_baslangic")._GetInt;
			bool getBoolean57 = parametreler._GetParametre("ikinci_satir_lot_no_sabit_kullan")._GetBoolean;
			int getInt175 = parametreler._GetParametre("ikinci_satir_lot_no_sabit_deger")._GetInt;
			int getInt176 = parametreler._GetParametre("ikinci_satir_lot_no_baslangic")._GetInt;
			bool getBoolean58 = parametreler._GetParametre("ikinci_satir_aciklama_sabit_kullan")._GetBoolean;
			string getString72 = parametreler._GetParametre("ikinci_satir_aciklama_sabit_deger")._GetString;
			int getInt177 = parametreler._GetParametre("ikinci_satir_aciklama_baslangic")._GetInt;
			bool getBoolean59 = parametreler._GetParametre("ikinci_birim_fiyat_mikroda_tanimli_fiyati_kullan")._GetBoolean;
			string getString73 = parametreler._GetParametre("ikinci_birim_fiyat_satis_fiyat_kaynaklari")._GetString;
			string getString74 = parametreler._GetParametre("ikinci_birim_fiyat_alis_fiyat_kaynaklari")._GetString;
			bool getBoolean60 = parametreler._GetParametre("ikinci_stok_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt178 = parametreler._GetParametre("ikinci_stok_birim_fiyat_baslangic")._GetInt;
			int getInt179 = parametreler._GetParametre("ikinci_stok_iskonto_1_uygulama_sekli")._GetInt;
			int getInt180 = parametreler._GetParametre("ikinci_stok_iskonto_1_baslangic")._GetInt;
			int getInt181 = parametreler._GetParametre("ikinci_stok_iskonto_2_uygulama_sekli")._GetInt;
			int getInt182 = parametreler._GetParametre("ikinci_stok_iskonto_2_baslangic")._GetInt;
			int getInt183 = parametreler._GetParametre("ikinci_stok_iskonto_3_uygulama_sekli")._GetInt;
			int getInt184 = parametreler._GetParametre("ikinci_stok_iskonto_3_baslangic")._GetInt;
			int getInt185 = parametreler._GetParametre("ikinci_stok_iskonto_4_uygulama_sekli")._GetInt;
			int getInt186 = parametreler._GetParametre("ikinci_stok_iskonto_4_baslangic")._GetInt;
			int getInt187 = parametreler._GetParametre("ikinci_stok_iskonto_5_uygulama_sekli")._GetInt;
			int getInt188 = parametreler._GetParametre("ikinci_stok_iskonto_5_baslangic")._GetInt;
			int getInt189 = parametreler._GetParametre("ikinci_stok_iskonto_6_uygulama_sekli")._GetInt;
			int getInt190 = parametreler._GetParametre("ikinci_stok_iskonto_6_baslangic")._GetInt;
			int getInt191 = parametreler._GetParametre("ikinci_stok_otv_uygulama_sekli")._GetInt;
			int getInt192 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_baslangic")._GetInt;
			bool getBoolean61 = parametreler._GetParametre("ikinci_miktar_sabit_kullan")._GetBoolean;
			string getString75 = parametreler._GetParametre("ikinci_miktar_sabit_deger")._GetString;
			int getInt193 = parametreler._GetParametre("ikinci_miktar_baslangic")._GetInt;
			bool getBoolean62 = parametreler._GetParametre("ikinci_miktar2_sabit_kullan")._GetBoolean;
			string getString76 = parametreler._GetParametre("ikinci_miktar2_sabit_deger")._GetString;
			int getInt194 = parametreler._GetParametre("ikinci_miktar2_baslangic")._GetInt;
			int getInt195 = parametreler._GetParametre("ikinci_stok_kdv_orani_bakilacak_yer")._GetInt;
			int getInt196 = parametreler._GetParametre("ikinci_stok_kdv_orani_sabit_deger")._GetInt;
			int getInt197 = parametreler._GetParametre("ikinci_stok_kdv_orani_baslangic")._GetInt;
			bool getBoolean63 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_kdv_dahil")._GetBoolean;
			int getInt198 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_baslangic")._GetInt;
			int getInt199 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_uygulama_sekli")._GetInt;
			int getInt200 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_baslangic")._GetInt;
			int getInt201 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_uygulama_sekli")._GetInt;
			int getInt202 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_baslangic")._GetInt;
			int getInt203 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_uygulama_sekli")._GetInt;
			int getInt204 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_baslangic")._GetInt;
			int getInt205 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_uygulama_sekli")._GetInt;
			int getInt206 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_baslangic")._GetInt;
			int getInt207 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_uygulama_sekli")._GetInt;
			int getInt208 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_baslangic")._GetInt;
			int getInt209 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_uygulama_sekli")._GetInt;
			int getInt210 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_baslangic")._GetInt;
			int getInt211 = parametreler._GetParametre("ikinci_hizmet_otv_uygulama_sekli")._GetInt;
			int getInt212 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_baslangic")._GetInt;
			int getInt213 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_bakilacak_yer")._GetInt;
			int getInt214 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_sabit_deger")._GetInt;
			int getInt215 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_baslangic")._GetInt;
			_ = parametreler._GetParametre("ikinci_satircinsiniotomatikbul")._GetBoolean;
			bool getBoolean64 = parametreler._GetParametre("ikinci_satir_aciklama_on_ek_kullan")._GetBoolean;
			string getString77 = parametreler._GetParametre("ikinci_satir_aciklama_on_ek_deger")._GetString;
			int getInt216 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt217 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt218 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt219 = parametreler._GetParametre("ikinci_stok_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt220 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt221 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt222 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt223 = parametreler._GetParametre("ikinci_stok_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt224 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt225 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt226 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt227 = parametreler._GetParametre("ikinci_stok_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt228 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt229 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt230 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt231 = parametreler._GetParametre("ikinci_stok_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt232 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt233 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt234 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt235 = parametreler._GetParametre("ikinci_stok_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt236 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt237 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt238 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt239 = parametreler._GetParametre("ikinci_stok_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt240 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt241 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt242 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt243 = parametreler._GetParametre("ikinci_stok_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt244 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt245 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt246 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt247 = parametreler._GetParametre("ikinci_stok_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt248 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt249 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt250 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt251 = parametreler._GetParametre("ikinci_stok_iskonto_6_islem_2_baslangic")._GetInt;
			int getInt252 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_1_islem_tipi")._GetInt;
			int getInt253 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_1_baslangic")._GetInt;
			int getInt254 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_2_islem_tipi")._GetInt;
			int getInt255 = parametreler._GetParametre("ikinci_hizmet_birim_fiyat_islem_2_baslangic")._GetInt;
			int getInt256 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_1_islem_tipi")._GetInt;
			int getInt257 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_1_baslangic")._GetInt;
			int getInt258 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_2_islem_tipi")._GetInt;
			int getInt259 = parametreler._GetParametre("ikinci_hizmet_kdv_orani_islem_2_baslangic")._GetInt;
			int getInt260 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_1_islem_tipi")._GetInt;
			int getInt261 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_1_baslangic")._GetInt;
			int getInt262 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_2_islem_tipi")._GetInt;
			int getInt263 = parametreler._GetParametre("ikinci_hizmet_otv_tutar_yuzde_islem_2_baslangic")._GetInt;
			int getInt264 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_1_islem_tipi")._GetInt;
			int getInt265 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_1_baslangic")._GetInt;
			int getInt266 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_2_islem_tipi")._GetInt;
			int getInt267 = parametreler._GetParametre("ikinci_hizmet_iskonto_1_islem_2_baslangic")._GetInt;
			int getInt268 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_1_islem_tipi")._GetInt;
			int getInt269 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_1_baslangic")._GetInt;
			int getInt270 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_2_islem_tipi")._GetInt;
			int getInt271 = parametreler._GetParametre("ikinci_hizmet_iskonto_2_islem_2_baslangic")._GetInt;
			int getInt272 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_1_islem_tipi")._GetInt;
			int getInt273 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_1_baslangic")._GetInt;
			int getInt274 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_2_islem_tipi")._GetInt;
			int getInt275 = parametreler._GetParametre("ikinci_hizmet_iskonto_3_islem_2_baslangic")._GetInt;
			int getInt276 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_1_islem_tipi")._GetInt;
			int getInt277 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_1_baslangic")._GetInt;
			int getInt278 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_2_islem_tipi")._GetInt;
			int getInt279 = parametreler._GetParametre("ikinci_hizmet_iskonto_4_islem_2_baslangic")._GetInt;
			int getInt280 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_1_islem_tipi")._GetInt;
			int getInt281 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_1_baslangic")._GetInt;
			int getInt282 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_2_islem_tipi")._GetInt;
			int getInt283 = parametreler._GetParametre("ikinci_hizmet_iskonto_5_islem_2_baslangic")._GetInt;
			int getInt284 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_1_islem_tipi")._GetInt;
			int getInt285 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_1_baslangic")._GetInt;
			int getInt286 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_2_islem_tipi")._GetInt;
			int getInt287 = parametreler._GetParametre("ikinci_hizmet_iskonto_6_islem_2_baslangic")._GetInt;
			int getInt288 = parametreler._GetParametre("cari_doviz_cinsi")._GetInt;
			int getInt289 = parametreler._GetParametre("cari_doviz_cinsi1")._GetInt;
			int getInt290 = parametreler._GetParametre("cari_doviz_cinsi2")._GetInt;
			string getString78 = parametreler._GetParametre("cari_muh_kod_satis")._GetString;
			string getString79 = parametreler._GetParametre("cari_muh_kod1_satis")._GetString;
			string getString80 = parametreler._GetParametre("cari_muh_kod2_satis")._GetString;
			string getString81 = parametreler._GetParametre("cari_muhartikeli")._GetString;
			string getString82 = parametreler._GetParametre("cari_muh_kod_alis")._GetString;
			string getString83 = parametreler._GetParametre("cari_muh_kod1_alis")._GetString;
			string getString84 = parametreler._GetParametre("cari_muh_kod2_alis")._GetString;
			bool getBoolean65 = parametreler._GetParametre("fiyat_eksi_oldugunda_evrak_tipini_degistir")._GetBoolean;
			int getInt291 = parametreler._GetParametre("fiyat_eksi_oldugunda_yeni_evrak_tipi")._GetInt;
			bool getBoolean66 = parametreler._GetParametre("ikinci_fiyat_eksi_oldugunda_evrak_tipini_degistir")._GetBoolean;
			int getInt292 = parametreler._GetParametre("ikinci_fiyat_eksi_oldugunda_yeni_evrak_tipi")._GetInt;
			int getInt293 = parametreler._GetParametre("cari_kod2_baslangic")._GetInt;
			bool getBoolean67 = parametreler._GetParametre("sto_isim_sabit_kullan")._GetBoolean;
			string getString85 = parametreler._GetParametre("sto_isim_sabit_deger")._GetString;
			int getInt294 = parametreler._GetParametre("sto_isim_baslangic")._GetInt;
			bool getBoolean68 = parametreler._GetParametre("sto_kisa_ismi_sabit_kullan")._GetBoolean;
			string getString86 = parametreler._GetParametre("sto_kisa_ismi_sabit_deger")._GetString;
			int getInt295 = parametreler._GetParametre("sto_kisa_ismi_baslangic")._GetInt;
			bool getBoolean69 = parametreler._GetParametre("sto_yabanci_ismi_sabit_kullan")._GetBoolean;
			string getString87 = parametreler._GetParametre("sto_yabanci_ismi_sabit_deger")._GetString;
			int getInt296 = parametreler._GetParametre("sto_yabanci_ismi_baslangic")._GetInt;
			bool getBoolean70 = parametreler._GetParametre("sto_birim1_ad_sabit_kullan")._GetBoolean;
			string getString88 = parametreler._GetParametre("sto_birim1_ad_sabit_deger")._GetString;
			int getInt297 = parametreler._GetParametre("sto_birim1_ad_baslangic")._GetInt;
			bool getBoolean71 = parametreler._GetParametre("sto_birim1_katsayi_sabit_kullan")._GetBoolean;
			string getString89 = parametreler._GetParametre("sto_birim1_katsayi_sabit_deger")._GetString;
			int getInt298 = parametreler._GetParametre("sto_birim1_katsayi_baslangic")._GetInt;
			int getInt299 = parametreler._GetParametre("sto_otvuygulama")._GetInt;
			string getString90 = parametreler._GetParametre("sto_muh_kod")._GetString;
			string getString91 = parametreler._GetParametre("sto_muh_Iade_kod")._GetString;
			string getString92 = parametreler._GetParametre("sto_muh_sat_muh_kod")._GetString;
			string getString93 = parametreler._GetParametre("sto_muh_satIadmuhkod")._GetString;
			string getString94 = parametreler._GetParametre("sto_muh_sat_isk_kod")._GetString;
			string getString95 = parametreler._GetParametre("sto_muh_aIiskmuhkod")._GetString;
			string getString96 = parametreler._GetParametre("sto_muh_satmalmuhkod")._GetString;
			string getString97 = parametreler._GetParametre("sto_yurtdisi_satmuhk")._GetString;
			string getString98 = parametreler._GetParametre("sto_ilavemasmuhkod")._GetString;
			string getString99 = parametreler._GetParametre("sto_yatirimtesmuhkod")._GetString;
			string getString100 = parametreler._GetParametre("sto_depsatmuhkod")._GetString;
			string getString101 = parametreler._GetParametre("sto_depsatmalmuhkod")._GetString;
			string getString102 = parametreler._GetParametre("sto_bagortsatmuhkod")._GetString;
			string getString103 = parametreler._GetParametre("sto_bagortsatIadmuhkod")._GetString;
			string getString104 = parametreler._GetParametre("sto_bagortsatIskmuhkod")._GetString;
			string getString105 = parametreler._GetParametre("sto_satfiyfarkmuhkod")._GetString;
			string getString106 = parametreler._GetParametre("sto_yurtdisisatmalmuhkod")._GetString;
			string getString107 = parametreler._GetParametre("sto_bagortsatmalmuhkod")._GetString;
			string getString108 = parametreler._GetParametre("sto_muhgrup_kodu")._GetString;
			string getString109 = parametreler._GetParametre("sto_anagrup_kod")._GetString;
			string getString110 = parametreler._GetParametre("sto_altgrup_kod")._GetString;
			bool getBoolean72 = parametreler._GetParametre("hiz_isim_sabit_kullan")._GetBoolean;
			string getString111 = parametreler._GetParametre("hiz_isim_sabit_deger")._GetString;
			int getInt300 = parametreler._GetParametre("hiz_isim_baslangic")._GetInt;
			bool getBoolean73 = parametreler._GetParametre("hiz_yabanci_isim_sabit_kullan")._GetBoolean;
			string getString112 = parametreler._GetParametre("hiz_yabanci_isim_sabit_deger")._GetString;
			int getInt301 = parametreler._GetParametre("hiz_yabanci_isim_baslangic")._GetInt;
			string getString113 = parametreler._GetParametre("hiz_tipkod")._GetString;
			string getString114 = parametreler._GetParametre("hiz_sinifkod")._GetString;
			string getString115 = parametreler._GetParametre("hiz_grupkod")._GetString;
			string getString116 = parametreler._GetParametre("hiz_sat_muh_kod")._GetString;
			string getString117 = parametreler._GetParametre("hiz_sat_iade_muh_kod")._GetString;
			string getString118 = parametreler._GetParametre("hiz_mal_muh_kod")._GetString;
			string getString119 = parametreler._GetParametre("hiz_sat_mal_muh_kod")._GetString;
			string getString120 = parametreler._GetParametre("hiz_mal_yan_muh_kod")._GetString;
			string getString121 = parametreler._GetParametre("hiz_muh_sat_isk_kod")._GetString;
			string getString122 = parametreler._GetParametre("hiz_muh_aIskmuhkod")._GetString;
			string getString123 = parametreler._GetParametre("hiz_ilavemasmuhkod")._GetString;
			bool getBoolean74 = parametreler._GetParametre("stok_fiyat_farki_mi_sabit_kullan")._GetBoolean;
			bool getBoolean75 = parametreler._GetParametre("stok_fiyat_farki_mi_sabit_deger")._GetBoolean;
			int getInt302 = parametreler._GetParametre("stok_fiyat_farki_mi_baslangic")._GetInt;
			string getString124 = parametreler._GetParametre("stok_fiyat_farki_icin_deger")._GetString;
			bool getBoolean76 = parametreler._GetParametre("ikinci_stok_fiyat_farki_mi_sabit_kullan")._GetBoolean;
			bool getBoolean77 = parametreler._GetParametre("ikinci_stok_fiyat_farki_mi_sabit_deger")._GetBoolean;
			int getInt303 = parametreler._GetParametre("ikinci_stok_fiyat_farki_mi_baslangic")._GetInt;
			string getString125 = parametreler._GetParametre("ikinci_stok_fiyat_farki_icin_deger")._GetString;
			int getInt304 = parametreler._GetParametre("otv_vergi_pntr")._GetInt;
			int getInt305 = parametreler._GetParametre("kriter_metin1_baslangic")._GetInt;
			int getInt306 = parametreler._GetParametre("kriter_metin2_baslangic")._GetInt;
			int getInt307 = parametreler._GetParametre("kriter_metin3_baslangic")._GetInt;
			int getInt308 = parametreler._GetParametre("kriter_metin4_baslangic")._GetInt;
			int getInt309 = parametreler._GetParametre("kriter_metin5_baslangic")._GetInt;
			int getInt310 = parametreler._GetParametre("kriter_double1_baslangic")._GetInt;
			int getInt311 = parametreler._GetParametre("kriter_double2_baslangic")._GetInt;
			int getInt312 = parametreler._GetParametre("kriter_double3_baslangic")._GetInt;
			int getInt313 = parametreler._GetParametre("kriter_double4_baslangic")._GetInt;
			int getInt314 = parametreler._GetParametre("kriter_double5_baslangic")._GetInt;
			int getInt315 = parametreler._GetParametre("kriter_bool1_baslangic")._GetInt;
			string getString126 = parametreler._GetParametre("kriter_bool1_evet_icin_deger")._GetString;
			int getInt316 = parametreler._GetParametre("kriter_bool2_baslangic")._GetInt;
			string getString127 = parametreler._GetParametre("kriter_bool2_evet_icin_deger")._GetString;
			int getInt317 = parametreler._GetParametre("kriter_bool3_baslangic")._GetInt;
			string getString128 = parametreler._GetParametre("kriter_bool3_evet_icin_deger")._GetString;
			int getInt318 = parametreler._GetParametre("kriter_bool4_baslangic")._GetInt;
			string getString129 = parametreler._GetParametre("kriter_bool4_evet_icin_deger")._GetString;
			int getInt319 = parametreler._GetParametre("kriter_bool5_baslangic")._GetInt;
			string getString130 = parametreler._GetParametre("kriter_bool5_evet_icin_deger")._GetString;
			bool getBoolean78 = parametreler._GetParametre("kayit_id_otomatik_ver")._GetBoolean;
			int getInt320 = parametreler._GetParametre("kayit_id_baslangic")._GetInt;
			int getInt321 = parametreler._GetParametre("otomatik_hesap_acma_secenek_cari")._GetInt;
			int getInt322 = parametreler._GetParametre("otomatik_hesap_acma_secenek_stok")._GetInt;
			int getInt323 = parametreler._GetParametre("otomatik_hesap_acma_secenek_hizmet")._GetInt;
			int getInt324 = parametreler._GetParametre("otomatik_hesap_acma_secenek_proje")._GetInt;
			int getInt325 = parametreler._GetParametre("otomatik_hesap_acma_secenek_sorumluluk")._GetInt;
			flag = parametreler._GetParametre("evrak_sira_otomatik_ver")._GetBoolean;
			satirBirlestir = parametreler._GetParametre("OtomatikEvrakSeriSatirBirlestir")._GetBoolean;
			bool getBoolean79 = parametreler._GetParametre("pro_adi_sabit_kullan")._GetBoolean;
			string getString131 = parametreler._GetParametre("pro_adi_sabit_deger")._GetString;
			int getInt326 = parametreler._GetParametre("pro_adi_baslangic")._GetInt;
			bool getBoolean80 = parametreler._GetParametre("pro_musterikodu_sabit_kullan")._GetBoolean;
			string getString132 = parametreler._GetParametre("pro_musterikodu_sabit_deger")._GetString;
			int getInt327 = parametreler._GetParametre("pro_musterikodu_baslangic")._GetInt;
			bool getBoolean81 = parametreler._GetParametre("pro_sormerkodu_sabit_kullan")._GetBoolean;
			string getString133 = parametreler._GetParametre("pro_sormerkodu_sabit_deger")._GetString;
			int getInt328 = parametreler._GetParametre("pro_sormerkodu_baslangic")._GetInt;
			bool getBoolean82 = parametreler._GetParametre("pro_grupkodu_sabit_kullan")._GetBoolean;
			string getString134 = parametreler._GetParametre("pro_grupkodu_sabit_deger")._GetString;
			int getInt329 = parametreler._GetParametre("pro_grupkodu_baslangic")._GetInt;
			bool getBoolean83 = parametreler._GetParametre("pro_sektorkodu_sabit_kullan")._GetBoolean;
			string getString135 = parametreler._GetParametre("pro_sektorkodu_sabit_deger")._GetString;
			int getInt330 = parametreler._GetParametre("pro_sektorkodu_baslangic")._GetInt;
			bool getBoolean84 = parametreler._GetParametre("pro_bolgekodu_sabit_kullan")._GetBoolean;
			string getString136 = parametreler._GetParametre("pro_bolgekodu_sabit_deger")._GetString;
			int getInt331 = parametreler._GetParametre("pro_bolgekodu_baslangic")._GetInt;
			bool getBoolean85 = parametreler._GetParametre("pro_ana_projekodu_sabit_kullan")._GetBoolean;
			string getString137 = parametreler._GetParametre("pro_ana_projekodu_sabit_deger")._GetString;
			int getInt332 = parametreler._GetParametre("pro_ana_projekodu_baslangic")._GetInt;
			bool getBoolean86 = parametreler._GetParametre("pro_aciklama_sabit_kullan")._GetBoolean;
			string getString138 = parametreler._GetParametre("pro_aciklama_sabit_deger")._GetString;
			int getInt333 = parametreler._GetParametre("pro_aciklama_baslangic")._GetInt;
			bool getBoolean87 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_kullan")._GetBoolean;
			string getString139 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_deger")._GetString;
			int getInt334 = parametreler._GetParametre("pro_muh_kod_artikeli_baslangic")._GetInt;
			bool getBoolean88 = parametreler._GetParametre("som_isim_sabit_kullan")._GetBoolean;
			string getString140 = parametreler._GetParametre("som_isim_sabit_deger")._GetString;
			int getInt335 = parametreler._GetParametre("som_isim_baslangic")._GetInt;
			bool getBoolean89 = parametreler._GetParametre("som_MuhArtikeli_sabit_kullan")._GetBoolean;
			string getString141 = parametreler._GetParametre("som_MuhArtikeli_sabit_deger")._GetString;
			int getInt336 = parametreler._GetParametre("som_MuhArtikeli_baslangic")._GetInt;
			bool getBoolean90 = parametreler._GetParametre("cari_muhartikeli_sabit_kullan")._GetBoolean;
			int getInt337 = parametreler._GetParametre("cari_muhartikeli_baslangic")._GetInt;
			bool getBoolean91 = parametreler._GetParametre("cari_muh_kod_satis_sabit_kullan")._GetBoolean;
			int getInt338 = parametreler._GetParametre("cari_muh_kod_satis_baslangic")._GetInt;
			bool getBoolean92 = parametreler._GetParametre("cari_muh_kod_alis_sabit_kullan")._GetBoolean;
			int getInt339 = parametreler._GetParametre("cari_muh_kod_alis_baslangic")._GetInt;
			bool getBoolean93 = parametreler._GetParametre("cari_muh_kod1_satis_sabit_kullan")._GetBoolean;
			int getInt340 = parametreler._GetParametre("cari_muh_kod1_satis_baslangic")._GetInt;
			bool getBoolean94 = parametreler._GetParametre("cari_muh_kod1_alis_sabit_kullan")._GetBoolean;
			int getInt341 = parametreler._GetParametre("cari_muh_kod1_alis_baslangic")._GetInt;
			bool getBoolean95 = parametreler._GetParametre("cari_muh_kod2_satis_sabit_kullan")._GetBoolean;
			int getInt342 = parametreler._GetParametre("cari_muh_kod2_satis_baslangic")._GetInt;
			bool getBoolean96 = parametreler._GetParametre("cari_muh_kod2_alis_sabit_kullan")._GetBoolean;
			int getInt343 = parametreler._GetParametre("cari_muh_kod2_alis_baslangic")._GetInt;
			bool getBoolean97 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_kullan")._GetBoolean;
			string getString142 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_deger")._GetString;
			int getInt344 = parametreler._GetParametre("cari_Ana_cari_kodu_baslangic")._GetInt;
			bool getBoolean98 = parametreler._GetParametre("cari_temsilci_kodu_sabit_kullan")._GetBoolean;
			string getString143 = parametreler._GetParametre("cari_temsilci_kodu_sabit_deger")._GetString;
			int getInt345 = parametreler._GetParametre("cari_temsilci_kodu_baslangic")._GetInt;
			bool getBoolean99 = parametreler._GetParametre("cari_grup_kodu_sabit_kullan")._GetBoolean;
			string getString144 = parametreler._GetParametre("cari_grup_kodu_sabit_deger")._GetString;
			int getInt346 = parametreler._GetParametre("cari_grup_kodu_baslangic")._GetInt;
			bool getBoolean100 = parametreler._GetParametre("cari_sektor_kodu_sabit_kullan")._GetBoolean;
			string getString145 = parametreler._GetParametre("cari_sektor_kodu_sabit_deger")._GetString;
			int getInt347 = parametreler._GetParametre("cari_sektor_kodu_baslangic")._GetInt;
			bool getBoolean101 = parametreler._GetParametre("cari_bolge_kodu_sabit_kullan")._GetBoolean;
			string getString146 = parametreler._GetParametre("cari_bolge_kodu_sabit_deger")._GetString;
			int getInt348 = parametreler._GetParametre("cari_bolge_kodu_baslangic")._GetInt;
			bool getBoolean102 = parametreler._GetParametre("cari_wwwadresi_sabit_kullan")._GetBoolean;
			string getString147 = parametreler._GetParametre("cari_wwwadresi_sabit_deger")._GetString;
			int getInt349 = parametreler._GetParametre("cari_wwwadresi_baslangic")._GetInt;
			bool getBoolean103 = parametreler._GetParametre("cari_CepTel_sabit_kullan")._GetBoolean;
			string getString148 = parametreler._GetParametre("cari_CepTel_sabit_deger")._GetString;
			int getInt350 = parametreler._GetParametre("cari_CepTel_baslangic")._GetInt;
			bool getBoolean104 = parametreler._GetParametre("cari_satis_isk_kod_sabit_kullan")._GetBoolean;
			string getString149 = parametreler._GetParametre("cari_satis_isk_kod_sabit_deger")._GetString;
			int getInt351 = parametreler._GetParametre("cari_satis_isk_kod_baslangic")._GetInt;
			bool getBoolean105 = parametreler._GetParametre("cari_sicil_no_sabit_kullan")._GetBoolean;
			string getString150 = parametreler._GetParametre("cari_sicil_no_sabit_deger")._GetString;
			int getInt352 = parametreler._GetParametre("cari_sicil_no_baslangic")._GetInt;
			bool getBoolean106 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_kullan")._GetBoolean;
			string getString151 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_deger")._GetString;
			int getInt353 = parametreler._GetParametre("cari_VarsayilanGirisDepo_baslangic")._GetInt;
			bool getBoolean107 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_kullan")._GetBoolean;
			string getString152 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_deger")._GetString;
			int getInt354 = parametreler._GetParametre("cari_VarsayilanCikisDepo_baslangic")._GetInt;
			bool getBoolean108 = parametreler._GetParametre("cari_Portal_PW_sabit_kullan")._GetBoolean;
			string getString153 = parametreler._GetParametre("cari_Portal_PW_sabit_deger")._GetString;
			int getInt355 = parametreler._GetParametre("cari_Portal_PW_baslangic")._GetInt;
			bool getBoolean109 = parametreler._GetParametre("cari_Portal_Enabled")._GetBoolean;
			_uygulanacak_kriterler = new List<GenelEvrakKriter>();
			string[] array = getString.Split(',');
			foreach (string text in array)
			{
				if (text != "")
				{
					GenelEvrakKriter genelEvrakKriter = new GenelEvrakKriter();
					Parametreler parametreler2 = ParametrelerDefault.GenelAktarimKriter(text);
					ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler2, "GenelAktarim", "", "Kriter", text);
					genelEvrakKriter.arama_alan1_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_baglac")._GetInt;
					genelEvrakKriter.arama_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan2_baglac")._GetInt;
					genelEvrakKriter.arama_alan1_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_alan2_baglac")._GetInt;
					string getString154 = parametreler2._GetParametre("arama_alan1")._GetString;
					string getString155 = parametreler2._GetParametre("arama_alan2")._GetString;
					string getString156 = parametreler2._GetParametre("degistirilecek_alanlar")._GetString;
					if (getString154 != "")
					{
						genelEvrakKriter.arama_alan1 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString154);
					}
					else
					{
						genelEvrakKriter.arama_alan1 = new List<AlanOperatorDeger>();
					}
					if (getString155 != "")
					{
						genelEvrakKriter.arama_alan2 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString155);
					}
					else
					{
						genelEvrakKriter.arama_alan2 = new List<AlanOperatorDeger>();
					}
					if (getString156 != "")
					{
						genelEvrakKriter.degistirilecek_alanlar = JSON.Instance.ToObject<List<AlanIslemTipiDeger>>(getString156);
					}
					else
					{
						genelEvrakKriter.degistirilecek_alanlar = new List<AlanIslemTipiDeger>();
					}
					_uygulanacak_kriterler.Add(genelEvrakKriter);
				}
			}
			SqlConnection sqlConnection = new SqlConnection();
			if (getString3 == "")
			{
				sqlConnection.ConnectionString = "Integrated Security=SSPI;Data Source=" + getString2 + "; Persist Security Info=False; Database=" + getString5 + " ; Initial Catalog=" + getString5 + ";";
			}
			else
			{
				sqlConnection.ConnectionString = "Password=" + getString4 + "; User ID=" + getString3 + "; Initial Catalog=" + getString5 + "; Data Source=" + getString2;
			}
			sqlConnection.Open();
			SqlCommand sqlCommand = sqlConnection.CreateCommand();
			sqlCommand.CommandText = getString6;
			if (getString6.Contains("@baslangic_tarihi"))
			{
				sqlCommand.Parameters.AddWithValue("@baslangic_tarihi", baslangic_tarihi);
			}
			if (getString6.Contains("@bitis_tarihi"))
			{
				sqlCommand.Parameters.AddWithValue("@bitis_tarihi", bitis_tarihi);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			List<Cari> list = new List<Cari>();
			BindingList<Stok> bindingList2 = new BindingList<Stok>();
			List<Hizmet> list2 = new List<Hizmet>();
			List<Proje> list3 = new List<Proje>();
			List<SorumlulukMerkezi> list4 = new List<SorumlulukMerkezi>();
			num3 = 0;
			while (sqlDataReader.Read())
			{
				bool flag2 = true;
				if (true)
				{
					enum_GenelEvrakTipleri enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.SatisFaturasi;
					if (getBoolean)
					{
						enum_GenelEvrakTipleri = (enum_GenelEvrakTipleri)getInt9;
					}
					else
					{
						num2 = getInt10;
						string text2 = "";
						if (num2 != 0)
						{
							text2 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						array = getString11.Split(',');
						foreach (string text3 in array)
						{
							if (text2 == text3)
							{
								enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.AlinanSiparis;
								break;
							}
						}
						array = getString9.Split(',');
						foreach (string text4 in array)
						{
							if (text2 == text4)
							{
								enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.AlisFaturasi;
								break;
							}
						}
						array = getString10.Split(',');
						foreach (string text5 in array)
						{
							if (text2 == text5)
							{
								enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.AlisIrsaliyesi;
								break;
							}
						}
						array = getString7.Split(',');
						foreach (string text6 in array)
						{
							if (text2 == text6)
							{
								enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.SatisFaturasi;
								break;
							}
						}
						array = getString8.Split(',');
						foreach (string text7 in array)
						{
							if (text2 == text7)
							{
								enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.SatisIrsaliyesi;
								break;
							}
						}
						array = getString12.Split(',');
						foreach (string text8 in array)
						{
							if (text2 == text8)
							{
								enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.DepolarArasiSevk;
								break;
							}
						}
					}
					string text9 = "";
					if (getInt22 != 0)
					{
						text9 = sqlDataReader[getInt22 - 1].ToString().Trim();
					}
					if (text9 == "" && getInt23 != 0)
					{
						text9 = sqlDataReader[getInt23 - 1].ToString().Trim();
					}
					string text10 = "";
					if (getInt20 != 0)
					{
						text10 = sqlDataReader[getInt20 - 1].ToString().Trim();
					}
					if (text10.Length > 50)
					{
						text10 = text10.Substring(0, 50);
					}
					string text11 = "";
					if (getInt21 != 0)
					{
						text11 = sqlDataReader[getInt21 - 1].ToString().Trim();
					}
					if (text11.Length > 50)
					{
						text11 = text11.Substring(0, 50);
					}
					string text12 = "";
					if (getInt25 != 0)
					{
						text12 = sqlDataReader[getInt25 - 1].ToString().Trim();
					}
					if (text12.Length > 30)
					{
						text12 = text12.Substring(0, 30);
					}
					string text13 = "";
					if (getInt33 != 0)
					{
						text13 = sqlDataReader[getInt33 - 1].ToString().Trim();
					}
					if (text13.Length > 80)
					{
						text13 = text13.Substring(0, 80);
					}
					string text14 = "";
					if (getBoolean39)
					{
						text14 = ((enum_GenelEvrakTipleri != enum_GenelEvrakTipleri.AlisFaturasi && enum_GenelEvrakTipleri != enum_GenelEvrakTipleri.AlisIrsaliyesi) ? getString55 : getString56);
					}
					if (getBoolean7)
					{
						text14 += getString19;
					}
					else
					{
						num2 = getInt19;
						string text15 = "";
						if (num2 != 0)
						{
							text15 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text14 += text15;
						if (text15 == "")
						{
							int num4 = getInt293;
							string text16 = "";
							if (num4 != 0)
							{
								text16 = sqlDataReader[num4 - 1].ToString().Trim();
							}
							text14 += text16;
						}
					}
					string[] array2 = getString20.Split(',');
					bool flag3 = false;
					array = array2;
					foreach (string text17 in array)
					{
						if (flag3)
						{
							break;
						}
						switch (text17)
						{
						case "1":
							if (text14 != "" && CariData.GetCariByCariKod(Db.Connection, text14, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "2":
							if (text9 != "" && CariData.GetCariByVergiTcKimlikNo(Db.Connection, text9, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "3":
							if (text13 != "" && CariData.GetCariByEMail(Db.Connection, text13, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "4":
							if (text12 != "" && CariData.GetCariByBankaHesapNo(Db.Connection, text12, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "5":
							if (text10 != "" && CariData.GetCariByCariUnvan(Db.Connection, text10, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						}
					}
					if (!flag3)
					{
						foreach (Cari item in list)
						{
							if (text14 == item.cari_kod)
							{
								flag3 = true;
								break;
							}
						}
					}
					if (!flag3)
					{
						Cari cari = new Cari();
						cari.CariAdresleri = new List<CariAdres>();
						cari.cari_kod = text14;
						cari.cari_vdaire_no = text9;
						cari.cari_unvan1 = text10;
						cari.cari_unvan2 = text11;
						cari.cari_banka_hesapno1 = text12;
						cari.cari_Email = text13;
						string text18 = "";
						if (getInt24 != 0)
						{
							text18 = sqlDataReader[getInt24 - 1].ToString().Trim();
						}
						if (text18.Length > 20)
						{
							text18 = text18.Substring(0, 20);
						}
						cari.cari_vdaire_adi = text18;
						string text19 = "";
						if (getBoolean90)
						{
							text19 = getString81;
						}
						else
						{
							if (getInt337 != 0)
							{
								text19 = sqlDataReader[getInt337 - 1].ToString().Trim();
							}
							if (text19.Length > 10)
							{
								text19 = text19.Substring(0, 10);
							}
						}
						cari.cari_muhartikeli = text19;
						string text20 = "";
						if (getBoolean97)
						{
							text20 = getString142;
						}
						else
						{
							if (getInt344 != 0)
							{
								text20 = sqlDataReader[getInt344 - 1].ToString().Trim();
							}
							if (text20.Length > 25)
							{
								text20 = text20.Substring(0, 25);
							}
						}
						cari.cari_Ana_cari_kodu = text20;
						string text21 = "";
						if (getBoolean98)
						{
							text21 = getString143;
						}
						else
						{
							if (getInt345 != 0)
							{
								text21 = sqlDataReader[getInt345 - 1].ToString().Trim();
							}
							if (text21.Length > 25)
							{
								text21 = text21.Substring(0, 25);
							}
						}
						cari.cari_temsilci_kodu = text21;
						string text22 = "";
						if (getBoolean99)
						{
							text22 = getString144;
						}
						else
						{
							if (getInt346 != 0)
							{
								text22 = sqlDataReader[getInt346 - 1].ToString().Trim();
							}
							if (text22.Length > 25)
							{
								text22 = text22.Substring(0, 25);
							}
						}
						cari.cari_grup_kodu = text22;
						string text23 = "";
						if (getBoolean100)
						{
							text23 = getString145;
						}
						else
						{
							if (getInt347 != 0)
							{
								text23 = sqlDataReader[getInt347 - 1].ToString().Trim();
							}
							if (text23.Length > 25)
							{
								text23 = text23.Substring(0, 25);
							}
						}
						cari.cari_sektor_kodu = text23;
						string text24 = "";
						if (getBoolean101)
						{
							text24 = getString146;
						}
						else
						{
							if (getInt348 != 0)
							{
								text24 = sqlDataReader[getInt348 - 1].ToString().Trim();
							}
							if (text24.Length > 25)
							{
								text24 = text24.Substring(0, 25);
							}
						}
						cari.cari_bolge_kodu = text24;
						string text25 = "";
						if (getBoolean102)
						{
							text25 = getString147;
						}
						else
						{
							if (getInt349 != 0)
							{
								text25 = sqlDataReader[getInt349 - 1].ToString().Trim();
							}
							if (text25.Length > 30)
							{
								text25 = text25.Substring(0, 30);
							}
						}
						cari.cari_wwwadresi = text25;
						string text26 = "";
						if (getBoolean103)
						{
							text26 = getString148;
						}
						else
						{
							if (getInt350 != 0)
							{
								text26 = sqlDataReader[getInt350 - 1].ToString().Trim();
							}
							if (text26.Length > 20)
							{
								text26 = text26.Substring(0, 20);
							}
						}
						cari.cari_CepTel = text26;
						string text27 = "";
						if (getBoolean104)
						{
							text27 = getString149;
						}
						else
						{
							if (getInt351 != 0)
							{
								text27 = sqlDataReader[getInt351 - 1].ToString().Trim();
							}
							if (text27.Length > 4)
							{
								text27 = text27.Substring(0, 4);
							}
						}
						cari.cari_satis_isk_kod = text27;
						string text28 = "";
						if (getBoolean105)
						{
							text28 = getString150;
						}
						else
						{
							if (getInt352 != 0)
							{
								text28 = sqlDataReader[getInt352 - 1].ToString().Trim();
							}
							if (text28.Length > 15)
							{
								text28 = text28.Substring(0, 15);
							}
						}
						cari.cari_sicil_no = text28;
						string text29 = "";
						if (getBoolean106)
						{
							text29 = getString151;
						}
						else
						{
							if (getInt353 != 0)
							{
								text29 = sqlDataReader[getInt353 - 1].ToString().Trim();
							}
							if (text29.Length > 25)
							{
								text29 = text29.Substring(0, 25);
							}
						}
						int result = 0;
						int.TryParse(text29, out result);
						cari.cari_VarsayilanGirisDepo = result;
						string text30 = "";
						if (getBoolean107)
						{
							text30 = getString152;
						}
						else
						{
							if (getInt354 != 0)
							{
								text30 = sqlDataReader[getInt354 - 1].ToString().Trim();
							}
							if (text30.Length > 25)
							{
								text30 = text30.Substring(0, 25);
							}
						}
						int result2 = 0;
						int.TryParse(text30, out result2);
						cari.cari_VarsayilanCikisDepo = result2;
						string text31 = "";
						if (getBoolean108)
						{
							text31 = getString153;
						}
						else
						{
							if (getInt355 != 0)
							{
								text31 = sqlDataReader[getInt355 - 1].ToString().Trim();
							}
							if (text31.Length > 127)
							{
								text31 = text31.Substring(0, 127);
							}
						}
						cari.cari_Portal_PW = text31;
						if (enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.AlisFaturasi || enum_GenelEvrakTipleri == enum_GenelEvrakTipleri.AlisIrsaliyesi)
						{
							string text32 = "";
							if (getBoolean92)
							{
								text32 = getString82;
							}
							else
							{
								if (getInt339 != 0)
								{
									text32 = sqlDataReader[getInt339 - 1].ToString().Trim();
								}
								if (text32.Length > 40)
								{
									text32 = text32.Substring(0, 40);
								}
							}
							cari.cari_muh_kod = text32;
							string text33 = "";
							if (getBoolean94)
							{
								text33 = getString83;
							}
							else
							{
								if (getInt341 != 0)
								{
									text33 = sqlDataReader[getInt341 - 1].ToString().Trim();
								}
								if (text33.Length > 40)
								{
									text33 = text33.Substring(0, 40);
								}
							}
							cari.cari_muh_kod1 = text33;
							string text34 = "";
							if (getBoolean96)
							{
								text34 = getString84;
							}
							else
							{
								if (getInt343 != 0)
								{
									text34 = sqlDataReader[getInt343 - 1].ToString().Trim();
								}
								if (text34.Length > 40)
								{
									text34 = text34.Substring(0, 40);
								}
							}
							cari.cari_muh_kod2 = text34;
						}
						else
						{
							string text35 = "";
							if (getBoolean91)
							{
								text35 = getString78;
							}
							else
							{
								if (getInt338 != 0)
								{
									text35 = sqlDataReader[getInt338 - 1].ToString().Trim();
								}
								if (text35.Length > 40)
								{
									text35 = text35.Substring(0, 40);
								}
							}
							cari.cari_muh_kod = text35;
							string text36 = "";
							if (getBoolean93)
							{
								text36 = getString79;
							}
							else
							{
								if (getInt340 != 0)
								{
									text36 = sqlDataReader[getInt340 - 1].ToString().Trim();
								}
								if (text36.Length > 40)
								{
									text36 = text36.Substring(0, 40);
								}
							}
							cari.cari_muh_kod1 = text36;
							string text37 = "";
							if (getBoolean95)
							{
								text37 = getString80;
							}
							else
							{
								if (getInt342 != 0)
								{
									text37 = sqlDataReader[getInt342 - 1].ToString().Trim();
								}
								if (text37.Length > 40)
								{
									text37 = text37.Substring(0, 40);
								}
							}
							cari.cari_muh_kod2 = text37;
						}
						cari.cari_doviz_cinsi = getInt288;
						cari.cari_doviz_cinsi1 = getInt289;
						cari.cari_doviz_cinsi2 = getInt290;
						cari.cari_Portal_Enabled = getBoolean109;
						string text38 = "";
						string text39 = "";
						string text40 = "";
						if (getBoolean25)
						{
							text38 = getString40;
						}
						else if (getInt51 != 0)
						{
							text38 = sqlDataReader[getInt51 - 1].ToString().Trim();
						}
						if (getBoolean26)
						{
							text39 = getString41;
						}
						else if (getInt52 != 0)
						{
							text39 = sqlDataReader[getInt52 - 1].ToString().Trim();
						}
						if (getBoolean27)
						{
							text40 = getString42;
						}
						else if (getInt53 != 0)
						{
							text40 = sqlDataReader[getInt53 - 1].ToString().Trim();
						}
						cari.cari_special1 = text38;
						cari.cari_special2 = text39;
						cari.cari_special3 = text40;
						string text41 = "";
						if (getInt26 != 0)
						{
							text41 = sqlDataReader[getInt26 - 1].ToString().Trim();
						}
						if (text41.Length > 50)
						{
							text41 = text41.Substring(0, 50);
						}
						if (text41 != "")
						{
							CariAdres cariAdres = new CariAdres();
							cariAdres.adr_cadde = text41;
							string text42 = "";
							if (getInt27 != 0)
							{
								text42 = sqlDataReader[getInt27 - 1].ToString().Trim();
							}
							if (text42.Length > 50)
							{
								text42 = text42.Substring(0, 50);
							}
							cariAdres.adr_sokak = text42;
							string text43 = "";
							if (getInt28 != 0)
							{
								text43 = sqlDataReader[getInt28 - 1].ToString().Trim();
							}
							if (text43.Length > 15)
							{
								text43 = text43.Substring(0, 15);
							}
							cariAdres.adr_ilce = text43;
							string text44 = "";
							if (getInt29 != 0)
							{
								text44 = sqlDataReader[getInt29 - 1].ToString().Trim();
							}
							if (text44.Length > 15)
							{
								text44 = text44.Substring(0, 15);
							}
							if (getBoolean52)
							{
								switch (int.Parse(text44))
								{
								case 1:
									text44 = "ADANA";
									break;
								case 2:
									text44 = "ADIYAMAN";
									break;
								case 3:
									text44 = "AFYONKARAHİSAR";
									break;
								case 4:
									text44 = "AĞRI";
									break;
								case 5:
									text44 = "AMASYA";
									break;
								case 6:
									text44 = "ANKARA";
									break;
								case 7:
									text44 = "ANTALYA";
									break;
								case 8:
									text44 = "ARTVİN";
									break;
								case 9:
									text44 = "AYDIN";
									break;
								case 10:
									text44 = "BALIKESİR";
									break;
								case 11:
									text44 = "BİLECİK";
									break;
								case 12:
									text44 = "BİNGÖL";
									break;
								case 13:
									text44 = "BİTLİS";
									break;
								case 14:
									text44 = "BOLU";
									break;
								case 15:
									text44 = "BURDUR";
									break;
								case 16:
									text44 = "BURSA";
									break;
								case 17:
									text44 = "ÇANAKKALE";
									break;
								case 18:
									text44 = "ÇANKIRI";
									break;
								case 19:
									text44 = "ÇORUM";
									break;
								case 20:
									text44 = "DENİZLİ";
									break;
								case 21:
									text44 = "DİYARBAKIR";
									break;
								case 22:
									text44 = "EDİRNE";
									break;
								case 23:
									text44 = "ELAZIĞ";
									break;
								case 24:
									text44 = "ERZİNCAN";
									break;
								case 25:
									text44 = "ERZURUM";
									break;
								case 26:
									text44 = "ESKİŞEHİR";
									break;
								case 27:
									text44 = "GAZİANTEP";
									break;
								case 28:
									text44 = "GİRESUN";
									break;
								case 29:
									text44 = "GÜMÜŞHANE";
									break;
								case 30:
									text44 = "HAKKARİ";
									break;
								case 31:
									text44 = "HATAY";
									break;
								case 32:
									text44 = "ISPARTA";
									break;
								case 33:
									text44 = "MERSİN";
									break;
								case 34:
									text44 = "İSTANBUL";
									break;
								case 35:
									text44 = "İZMİR";
									break;
								case 36:
									text44 = "KARS";
									break;
								case 37:
									text44 = "KASTAMONU";
									break;
								case 38:
									text44 = "KAYSERİ";
									break;
								case 39:
									text44 = "KIRKLARELİ";
									break;
								case 40:
									text44 = "KIRŞEHİR";
									break;
								case 41:
									text44 = "KOCAELİ";
									break;
								case 42:
									text44 = "KONYA";
									break;
								case 43:
									text44 = "KÜTAHYA";
									break;
								case 44:
									text44 = "MALATYA";
									break;
								case 45:
									text44 = "MANİSA";
									break;
								case 46:
									text44 = "KAHRAMANMARAŞ";
									break;
								case 47:
									text44 = "MARDİN";
									break;
								case 48:
									text44 = "MUĞLA";
									break;
								case 49:
									text44 = "MUŞ";
									break;
								case 50:
									text44 = "NEVŞEHİR";
									break;
								case 51:
									text44 = "NİĞDE";
									break;
								case 52:
									text44 = "ORDU";
									break;
								case 53:
									text44 = "RİZE";
									break;
								case 54:
									text44 = "SAKARYA";
									break;
								case 55:
									text44 = "SAMSUN";
									break;
								case 56:
									text44 = "SİİRT";
									break;
								case 57:
									text44 = "SİNOP";
									break;
								case 58:
									text44 = "SİVAS";
									break;
								case 59:
									text44 = "TEKİRDAĞ";
									break;
								case 60:
									text44 = "TOKAT";
									break;
								case 61:
									text44 = "TRABZON";
									break;
								case 62:
									text44 = "TUNCELİ";
									break;
								case 63:
									text44 = "ŞANLIURFA";
									break;
								case 64:
									text44 = "UŞAK";
									break;
								case 65:
									text44 = "VAN";
									break;
								case 66:
									text44 = "YOZGAT";
									break;
								case 67:
									text44 = "ZONGULDAK";
									break;
								case 68:
									text44 = "AKSARAY";
									break;
								case 69:
									text44 = "BAYBURT";
									break;
								case 70:
									text44 = "KARAMAN";
									break;
								case 71:
									text44 = "KIRIKKALE";
									break;
								case 72:
									text44 = "BATMAN";
									break;
								case 73:
									text44 = "ŞIRNAK";
									break;
								case 74:
									text44 = "BARTIN";
									break;
								case 75:
									text44 = "ARDAHAN";
									break;
								case 76:
									text44 = "IĞDIR";
									break;
								case 77:
									text44 = "YALOVA";
									break;
								case 78:
									text44 = "KARABÜK";
									break;
								case 79:
									text44 = "KİLİS";
									break;
								case 80:
									text44 = "OSMANİYE";
									break;
								case 81:
									text44 = "DÜZCE";
									break;
								}
							}
							cariAdres.adr_il = text44;
							string text45 = "";
							if (getInt30 != 0)
							{
								text45 = sqlDataReader[getInt30 - 1].ToString().Trim();
							}
							if (text45.Length > 15)
							{
								text45 = text45.Substring(0, 15);
							}
							cariAdres.adr_ulke = text45;
							string text46 = "";
							if (getInt31 != 0)
							{
								text46 = sqlDataReader[getInt31 - 1].ToString().Trim();
							}
							if (text46.Length > 8)
							{
								text46 = text46.Substring(0, 8);
							}
							cariAdres.adr_posta_kodu = text46;
							string text47 = "";
							if (getInt32 != 0)
							{
								text47 = sqlDataReader[getInt32 - 1].ToString().Trim();
							}
							if (text47.Length > 10)
							{
								text47 = text47.Substring(0, 10);
							}
							cariAdres.adr_tel_no1 = text47;
							cariAdres.adr_special1 = text38;
							cariAdres.adr_special2 = text39;
							cariAdres.adr_special3 = text40;
							cariAdres.adr_cari_kod = cari.cari_kod;
							cariAdres.adr_adres_no = 1;
							cari.CariAdresleri.Add(cariAdres);
						}
						list.Add(cari);
					}
				}
				if (flag2)
				{
					string text48 = "";
					enum_SatirCinsi enum_SatirCinsi = enum_SatirCinsi.Stok;
					if (getBoolean4)
					{
						text48 = getString17;
					}
					else
					{
						num2 = getInt15;
						string text49 = "";
						if (num2 != 0)
						{
							text49 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text48 = text49;
					}
					if (getBoolean3)
					{
						enum_SatirCinsi = (enum_SatirCinsi)getInt13;
					}
					else
					{
						num2 = getInt14;
						string text50 = "";
						if (num2 != 0)
						{
							text50 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (text50 == getString15)
						{
							enum_SatirCinsi = enum_SatirCinsi.Stok;
						}
						if (text50 == getString16)
						{
							enum_SatirCinsi = enum_SatirCinsi.Hizmet;
						}
					}
					if (getBoolean38)
					{
						Stok stok = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text48, enum_toptan_perakende.Toptan);
						if (!(stok.sto_kod == "") && stok.sto_kod != null)
						{
							enum_SatirCinsi = enum_SatirCinsi.Stok;
						}
						else
						{
							Hizmet hizmet = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text48);
							if (!(hizmet.hiz_kod == "") && hizmet.hiz_kod != null)
							{
								enum_SatirCinsi = enum_SatirCinsi.Hizmet;
							}
						}
					}
					bool flag4 = false;
					if (enum_SatirCinsi == enum_SatirCinsi.Stok)
					{
						Stok stok2 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text48, enum_toptan_perakende.Toptan);
						if (stok2.sto_kod == "" || stok2.sto_kod == null)
						{
							Barkod barkodBilgisi = BarkodData.GetBarkodBilgisi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text48);
							if (barkodBilgisi.bar_stokkodu != "")
							{
								stok2 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, barkodBilgisi.bar_stokkodu, enum_toptan_perakende.Toptan);
								if (stok2.sto_kod != "" && stok2.sto_kod != null)
								{
									text48 = stok2.sto_kod;
								}
							}
						}
						if (!(stok2.sto_kod == "") && stok2.sto_kod != null)
						{
							flag4 = true;
						}
					}
					else
					{
						Hizmet hizmet2 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text48);
						if (!(hizmet2.hiz_kod == "") && hizmet2.hiz_kod != null)
						{
							flag4 = true;
						}
					}
					if (!flag4)
					{
						string text51 = "";
						string text52 = "";
						string text53 = "";
						if (getBoolean25)
						{
							text51 = getString40;
						}
						else if (getInt51 != 0)
						{
							text51 = sqlDataReader[getInt51 - 1].ToString().Trim();
						}
						if (getBoolean26)
						{
							text52 = getString41;
						}
						else if (getInt52 != 0)
						{
							text52 = sqlDataReader[getInt52 - 1].ToString().Trim();
						}
						if (getBoolean27)
						{
							text53 = getString42;
						}
						else if (getInt53 != 0)
						{
							text53 = sqlDataReader[getInt53 - 1].ToString().Trim();
						}
						if (enum_SatirCinsi == enum_SatirCinsi.Stok)
						{
							bool flag5 = false;
							foreach (Stok item2 in bindingList2)
							{
								if (text48 == item2.sto_kod)
								{
									flag5 = true;
									break;
								}
							}
							if (!flag5)
							{
								Stok stok3 = new Stok();
								stok3.sto_kod = text48;
								stok3.sto_special1 = text51;
								stok3.sto_special2 = text52;
								stok3.sto_special3 = text53;
								int num5 = 4;
								switch (getInt73)
								{
								case 0:
									num5 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, stok3.sto_kod, enum_toptan_perakende.Toptan).ekleme_bilgileri.vergi_pntr;
									break;
								case 1:
									num5 = getInt74;
									break;
								case 2:
								{
									num2 = getInt75;
									string text54 = "0";
									if (num2 != 0)
									{
										text54 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result3 = 0.0;
									double.TryParse(text54.Replace(".", ","), out result3);
									num = getInt103;
									num2 = getInt104;
									if (num != 0)
									{
										text54 = "";
										if (num2 != 0)
										{
											text54 = sqlDataReader[num2 - 1].ToString().Trim();
										}
										double result4 = 0.0;
										double.TryParse(text54.Replace(".", ","), out result4);
										switch (num)
										{
										case 1:
											result3 += result4;
											break;
										case 2:
											result3 -= result4;
											break;
										case 3:
											result3 /= result4;
											break;
										case 4:
											result3 *= result4;
											break;
										}
									}
									num = getInt105;
									num2 = getInt106;
									if (num != 0)
									{
										text54 = "";
										if (num2 != 0)
										{
											text54 = sqlDataReader[num2 - 1].ToString().Trim();
										}
										double result5 = 0.0;
										double.TryParse(text54.Replace(".", ","), out result5);
										switch (num)
										{
										case 1:
											result3 += result5;
											break;
										case 2:
											result3 -= result5;
											break;
										case 3:
											result3 /= result5;
											break;
										case 4:
											result3 *= result5;
											break;
										}
									}
									num5 = GenelUtility.GetKdvPntr((int)result3);
									break;
								}
								}
								stok3.sto_toptan_vergi_yeni = num5;
								stok3.sto_perakende_vergi_yeni = num5;
								double num6 = 0.0;
								if (getInt68 != 0)
								{
									double result6 = 0.0;
									double.TryParse(sqlDataReader[getInt68 - 1].ToString().Trim().Replace(".", ","), out result6);
									num6 = result6;
								}
								num = getInt107;
								num2 = getInt108;
								if (num != 0)
								{
									string text55 = "";
									if (num2 != 0)
									{
										text55 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result7 = 0.0;
									double.TryParse(text55.Replace(".", ","), out result7);
									switch (num)
									{
									case 1:
										num6 += result7;
										break;
									case 2:
										num6 -= result7;
										break;
									case 3:
										num6 /= result7;
										break;
									case 4:
										num6 *= result7;
										break;
									}
								}
								num = getInt109;
								num2 = getInt110;
								if (num != 0)
								{
									string text56 = "";
									if (num2 != 0)
									{
										text56 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result8 = 0.0;
									double.TryParse(text56.Replace(".", ","), out result8);
									switch (num)
									{
									case 1:
										num6 += result8;
										break;
									case 2:
										num6 -= result8;
										break;
									case 3:
										num6 /= result8;
										break;
									case 4:
										num6 *= result8;
										break;
									}
								}
								stok3.sto_otvtutar = num6;
								string text57 = "";
								if (getBoolean67)
								{
									text57 = getString85;
								}
								else
								{
									if (getInt294 != 0)
									{
										text57 = sqlDataReader[getInt294 - 1].ToString().Trim();
									}
									if (text57.Length > 50)
									{
										text57 = text57.Substring(0, 50);
									}
								}
								stok3.sto_isim = text57;
								string text58 = "";
								if (getBoolean68)
								{
									text58 = getString86;
								}
								else
								{
									if (getInt295 != 0)
									{
										text58 = sqlDataReader[getInt295 - 1].ToString().Trim();
									}
									if (text58.Length > 25)
									{
										text58 = text58.Substring(0, 25);
									}
								}
								stok3.sto_kisa_ismi = text58;
								string text59 = "";
								if (getBoolean69)
								{
									text59 = getString87;
								}
								else
								{
									if (getInt296 != 0)
									{
										text59 = sqlDataReader[getInt296 - 1].ToString().Trim();
									}
									if (text59.Length > 50)
									{
										text59 = text59.Substring(0, 50);
									}
								}
								stok3.sto_yabanci_isim = text59;
								string text60 = "";
								if (getBoolean70)
								{
									text60 = getString88;
								}
								else
								{
									if (getInt297 != 0)
									{
										text60 = sqlDataReader[getInt297 - 1].ToString().Trim();
									}
									if (text60.Length > 10)
									{
										text60 = text60.Substring(0, 10);
									}
								}
								stok3.sto_birim1_ad = text60;
								string s = "";
								if (getBoolean71)
								{
									s = getString89;
								}
								else if (getInt298 != 0)
								{
									s = sqlDataReader[getInt298 - 1].ToString().Trim();
								}
								float result9 = 1f;
								float.TryParse(s, out result9);
								stok3.sto_birim1_katsayi = result9;
								stok3.sto_otvuygulama = getInt299;
								stok3.sto_muh_kod = getString90;
								stok3.sto_muh_Iade_kod = getString91;
								stok3.sto_muh_sat_muh_kod = getString92;
								stok3.sto_muh_satIadmuhkod = getString93;
								stok3.sto_muh_sat_isk_kod = getString94;
								stok3.sto_muh_aIiskmuhkod = getString95;
								stok3.sto_muh_satmalmuhkod = getString96;
								stok3.sto_yurtdisi_satmuhk = getString97;
								stok3.sto_ilavemasmuhkod = getString98;
								stok3.sto_yatirimtesmuhkod = getString99;
								stok3.sto_depsatmuhkod = getString100;
								stok3.sto_depsatmalmuhkod = getString101;
								stok3.sto_bagortsatmuhkod = getString102;
								stok3.sto_bagortsatIadmuhkod = getString103;
								stok3.sto_bagortsatIskmuhkod = getString104;
								stok3.sto_satfiyfarkmuhkod = getString105;
								stok3.sto_yurtdisisatmalmuhkod = getString106;
								stok3.sto_bagortsatmalmuhkod = getString107;
								stok3.sto_muhgrup_kodu = getString108;
								stok3.sto_anagrup_kod = getString109;
								stok3.sto_altgrup_kod = getString110;
								bindingList2.Add(stok3);
							}
						}
						else
						{
							bool flag6 = false;
							foreach (Hizmet item3 in list2)
							{
								if (text48 == item3.hiz_kod)
								{
									flag6 = true;
									break;
								}
							}
							if (!flag6)
							{
								Hizmet hizmet3 = new Hizmet();
								hizmet3.hiz_kod = text48;
								hizmet3.hiz_special1 = text51;
								hizmet3.hiz_special2 = text52;
								hizmet3.hiz_special3 = text53;
								int hiz_KDV = 4;
								switch (getInt91)
								{
								case 1:
									hiz_KDV = getInt92;
									break;
								case 2:
								{
									num2 = getInt93;
									string text61 = "0";
									if (num2 != 0)
									{
										text61 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result10 = 0.0;
									double.TryParse(text61.Replace(".", ","), out result10);
									num = getInt139;
									num2 = getInt140;
									if (num != 0)
									{
										text61 = "";
										if (num2 != 0)
										{
											text61 = sqlDataReader[num2 - 1].ToString().Trim();
										}
										double result11 = 0.0;
										double.TryParse(text61.Replace(".", ","), out result11);
										switch (num)
										{
										case 1:
											result10 += result11;
											break;
										case 2:
											result10 -= result11;
											break;
										case 3:
											result10 /= result11;
											break;
										case 4:
											result10 *= result11;
											break;
										}
									}
									num = getInt141;
									num2 = getInt142;
									if (num != 0)
									{
										text61 = "";
										if (num2 != 0)
										{
											text61 = sqlDataReader[num2 - 1].ToString().Trim();
										}
										double result12 = 0.0;
										double.TryParse(text61.Replace(".", ","), out result12);
										switch (num)
										{
										case 1:
											result10 += result12;
											break;
										case 2:
											result10 -= result12;
											break;
										case 3:
											result10 /= result12;
											break;
										case 4:
											result10 *= result12;
											break;
										}
									}
									hiz_KDV = GenelUtility.GetKdvPntr((int)result10);
									break;
								}
								}
								hizmet3.hiz_KDV = hiz_KDV;
								string text62 = "";
								if (getBoolean72)
								{
									text62 = getString111;
								}
								else
								{
									if (getInt300 != 0)
									{
										text62 = sqlDataReader[getInt300 - 1].ToString().Trim();
									}
									if (text62.Length > 50)
									{
										text62 = text62.Substring(0, 50);
									}
								}
								hizmet3.hiz_isim = text62;
								string text63 = "";
								if (getBoolean73)
								{
									text63 = getString112;
								}
								else
								{
									if (getInt301 != 0)
									{
										text63 = sqlDataReader[getInt301 - 1].ToString().Trim();
									}
									if (text63.Length > 25)
									{
										text63 = text63.Substring(0, 25);
									}
								}
								hizmet3.hiz_yabanci_isim = text63;
								double num7 = 0.0;
								if (getInt76 != 0)
								{
									double result13 = 0.0;
									double.TryParse(sqlDataReader[getInt76 - 1].ToString().Trim().Replace(".", ","), out result13);
									num7 = result13;
								}
								num = getInt135;
								num2 = getInt136;
								if (num != 0)
								{
									string text64 = "";
									if (num2 != 0)
									{
										text64 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result14 = 0.0;
									double.TryParse(text64.Replace(".", ","), out result14);
									switch (num)
									{
									case 1:
										num7 += result14;
										break;
									case 2:
										num7 -= result14;
										break;
									case 3:
										num7 /= result14;
										break;
									case 4:
										num7 *= result14;
										break;
									}
								}
								num = getInt137;
								num2 = getInt138;
								if (num != 0)
								{
									string text65 = "";
									if (num2 != 0)
									{
										text65 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result15 = 0.0;
									double.TryParse(text65.Replace(".", ","), out result15);
									switch (num)
									{
									case 1:
										num7 += result15;
										break;
									case 2:
										num7 -= result15;
										break;
									case 3:
										num7 /= result15;
										break;
									case 4:
										num7 *= result15;
										break;
									}
								}
								if (getBoolean34)
								{
									double yuzde = _mikrouygulamabilgileri.vergitanimlari[hizmet3.hiz_KDV].Yuzde;
									double num8 = 100.0;
									num7 /= yuzde / num8 + 1.0;
								}
								if (num7 < 0.0)
								{
									num7 *= -1.0;
								}
								hizmet3.BirimFiyat.FiyatBrut = num7;
								hizmet3.hiz_tipkod = getString113;
								hizmet3.hiz_sinifkod = getString114;
								hizmet3.hiz_grupkod = getString115;
								hizmet3.hiz_sat_muh_kod = getString116;
								hizmet3.hiz_sat_iade_muh_kod = getString117;
								hizmet3.hiz_mal_muh_kod = getString118;
								hizmet3.hiz_sat_mal_muh_kod = getString119;
								hizmet3.hiz_mal_yan_muh_kod = getString120;
								hizmet3.hiz_muh_sat_isk_kod = getString121;
								hizmet3.hiz_muh_aIiskmuhkod = getString122;
								hizmet3.hiz_ilavemasmuhkod = getString123;
								list2.Add(hizmet3);
							}
						}
					}
					string text66 = "";
					if (getBoolean10)
					{
						text66 = getString26;
					}
					else
					{
						num2 = getInt37;
						string text67 = "";
						if (num2 != 0)
						{
							text67 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text66 = text67;
					}
					bool flag7 = false;
					Proje proje = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text66);
					if (!(proje.pro_kodu == "") && proje.pro_kodu != null)
					{
						flag7 = true;
					}
					if (text66 == "")
					{
						flag7 = true;
					}
					if (!flag7)
					{
						bool flag8 = false;
						foreach (Proje item4 in list3)
						{
							if (text66 == item4.pro_kodu)
							{
								flag8 = true;
								break;
							}
						}
						if (!flag8)
						{
							Proje proje2 = new Proje();
							proje2.pro_kodu = text66;
							string text68 = "";
							if (getBoolean79)
							{
								text68 = getString131;
							}
							else
							{
								if (getInt326 != 0)
								{
									text68 = sqlDataReader[getInt326 - 1].ToString().Trim();
								}
								if (text68.Length > 40)
								{
									text68 = text68.Substring(0, 40);
								}
							}
							proje2.pro_adi = text68;
							string text69 = "";
							if (getBoolean80)
							{
								text69 = getString132;
							}
							else
							{
								if (getInt327 != 0)
								{
									text69 = sqlDataReader[getInt327 - 1].ToString().Trim();
								}
								if (text69.Length > 25)
								{
									text69 = text69.Substring(0, 25);
								}
							}
							proje2.pro_musterikodu = text69;
							string text70 = "";
							if (getBoolean81)
							{
								text70 = getString133;
							}
							else
							{
								if (getInt328 != 0)
								{
									text70 = sqlDataReader[getInt328 - 1].ToString().Trim();
								}
								if (text70.Length > 25)
								{
									text70 = text70.Substring(0, 25);
								}
							}
							proje2.pro_sormerkodu = text70;
							string text71 = "";
							if (getBoolean82)
							{
								text71 = getString134;
							}
							else
							{
								if (getInt329 != 0)
								{
									text71 = sqlDataReader[getInt329 - 1].ToString().Trim();
								}
								if (text71.Length > 25)
								{
									text71 = text71.Substring(0, 25);
								}
							}
							proje2.pro_grupkodu = text71;
							string text72 = "";
							if (getBoolean83)
							{
								text72 = getString135;
							}
							else
							{
								if (getInt330 != 0)
								{
									text72 = sqlDataReader[getInt330 - 1].ToString().Trim();
								}
								if (text72.Length > 25)
								{
									text72 = text72.Substring(0, 25);
								}
							}
							proje2.pro_sektorkodu = text72;
							string text73 = "";
							if (getBoolean84)
							{
								text73 = getString136;
							}
							else
							{
								if (getInt331 != 0)
								{
									text73 = sqlDataReader[getInt331 - 1].ToString().Trim();
								}
								if (text73.Length > 25)
								{
									text73 = text73.Substring(0, 25);
								}
							}
							proje2.pro_bolgekodu = text73;
							string text74 = "";
							if (getBoolean85)
							{
								text74 = getString137;
							}
							else
							{
								if (getInt332 != 0)
								{
									text74 = sqlDataReader[getInt332 - 1].ToString().Trim();
								}
								if (text74.Length > 25)
								{
									text74 = text74.Substring(0, 25);
								}
							}
							proje2.pro_ana_projekodu = text74;
							string text75 = "";
							if (getBoolean86)
							{
								text75 = getString138;
							}
							else
							{
								if (getInt333 != 0)
								{
									text75 = sqlDataReader[getInt333 - 1].ToString().Trim();
								}
								if (text75.Length > 50)
								{
									text75 = text75.Substring(0, 50);
								}
							}
							proje2.pro_aciklama = text75;
							string text76 = "";
							if (getBoolean87)
							{
								text76 = getString139;
							}
							else
							{
								if (getInt334 != 0)
								{
									text76 = sqlDataReader[getInt334 - 1].ToString().Trim();
								}
								if (text76.Length > 10)
								{
									text76 = text76.Substring(0, 10);
								}
							}
							proje2.pro_muh_kod_artikeli = text76;
							list3.Add(proje2);
						}
					}
					string text77 = "";
					if (getBoolean11)
					{
						text77 = getString27;
					}
					else
					{
						num2 = getInt38;
						string text78 = "";
						if (num2 != 0)
						{
							text78 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text77 = text78;
					}
					bool flag9 = false;
					SorumlulukMerkezi sorumlulukMerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text77);
					if (!(sorumlulukMerkezi.som_kod == "") && sorumlulukMerkezi.som_kod != null)
					{
						flag9 = true;
					}
					if (text77 == "")
					{
						flag9 = true;
					}
					if (!flag9)
					{
						bool flag10 = false;
						foreach (SorumlulukMerkezi item5 in list4)
						{
							if (text77 == item5.som_kod)
							{
								flag10 = true;
								break;
							}
						}
						if (!flag10)
						{
							SorumlulukMerkezi sorumlulukMerkezi2 = new SorumlulukMerkezi();
							sorumlulukMerkezi2.som_kod = text77;
							string text79 = "";
							if (getBoolean88)
							{
								text79 = getString140;
							}
							else
							{
								if (getInt335 != 0)
								{
									text79 = sqlDataReader[getInt335 - 1].ToString().Trim();
								}
								if (text79.Length > 40)
								{
									text79 = text79.Substring(0, 40);
								}
							}
							sorumlulukMerkezi2.som_isim = text79;
							string text80 = "";
							if (getBoolean89)
							{
								text80 = getString141;
							}
							else
							{
								if (getInt336 != 0)
								{
									text80 = sqlDataReader[getInt336 - 1].ToString().Trim();
								}
								if (text80.Length > 10)
								{
									text80 = text80.Substring(0, 10);
								}
							}
							sorumlulukMerkezi2.som_MuhArtikeli = text80;
							list4.Add(sorumlulukMerkezi2);
						}
					}
				}
				num3++;
			}
			sqlDataReader.Close();
			sqlDataReader = sqlCommand.ExecuteReader();
			if (list.Count > 0)
			{
				switch (getInt321)
				{
				case 1:
					foreach (Cari item6 in list)
					{
						item6.KaydetYeniCari(Db.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniCarileriSorVeAc(list);
					break;
				}
			}
			if (bindingList2.Count > 0)
			{
				switch (getInt322)
				{
				case 1:
					foreach (Stok item7 in bindingList2)
					{
						StokData.YeniStokKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item7, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniStoklariSorVeAc(bindingList2);
					break;
				}
			}
			if (list2.Count > 0)
			{
				switch (getInt323)
				{
				case 1:
					foreach (Hizmet item8 in list2)
					{
						HizmetData.YeniHizmetKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item8, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniHizmetleriSorVeAc(list2);
					break;
				}
			}
			if (list3.Count > 0)
			{
				switch (getInt324)
				{
				case 1:
					foreach (Proje item9 in list3)
					{
						ProjeData.YeniProjeKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item9, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniProjeleriSorVeAc(list3);
					break;
				}
			}
			if (list4.Count > 0)
			{
				switch (getInt325)
				{
				case 1:
					foreach (SorumlulukMerkezi item10 in list4)
					{
						SorumlulukMerkeziData.YeniSorumlulukMerkeziKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item10, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniSorumlulukMerkezleriSorVeAc(list4);
					break;
				}
			}
			enum_GenelEvrakTipleri enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.SatisFaturasi;
			enum_cha_normal_Iade normaliade = enum_cha_normal_Iade.Normal;
			enum_KapamaSekli kapamasekli = enum_KapamaSekli.AcikHesap;
			enum_cha_ticaret_turu ticaretturu = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
			DateTime dateTime = DateTime.Now;
			string evraknoseri = "";
			int evraknosira = 0;
			string belgeno = "";
			DateTime belgetarih = DateTime.Now;
			Cari cari2 = new Cari();
			int odemeplani = 0;
			FiyatListesi fiyatlistesi = new FiyatListesi();
			int dovizcinsi = 0;
			Kur kur = new Kur();
			int alternatifdovizcinsi = 0;
			Kur alternatifdovizkuru = new Kur();
			Depo kaynakdepo = new Depo();
			Depo hedefdepo = new Depo();
			Proje proje3 = new Proje();
			SorumlulukMerkezi sorumlulukmerkezi = new SorumlulukMerkezi();
			string temsilcikodu = "";
			Firma firma = new Firma();
			Sube sube = new Sube();
			int mikrouserno = 1;
			DateTime sevkteslimtarihi = DateTime.Now;
			int sevkadresno = 1;
			string kapamahesapkodu = "";
			string text81 = "";
			string text82 = "";
			string text83 = "";
			string text84 = "";
			string text85 = "";
			string text86 = "";
			string text87 = "";
			string text88 = "";
			string text89 = "";
			string text90 = "";
			string degistirspecialalan = "";
			string degistirspecialalan2 = "";
			string degistirspecialalan3 = "";
			string kriter_string = "";
			string kriter_string2 = "";
			string kriter_string3 = "";
			string kriter_string4 = "";
			string kriter_string5 = "";
			double result16 = 0.0;
			double result17 = 0.0;
			double result18 = 0.0;
			double result19 = 0.0;
			double result20 = 0.0;
			bool kriter_bool = false;
			bool kriter_bool2 = false;
			bool kriter_bool3 = false;
			bool kriter_bool4 = false;
			bool kriter_bool5 = false;
			num3 = 0;
			while (sqlDataReader.Read())
			{
				bool flag11 = true;
				if (true)
				{
					firma = FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt);
					sube = SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt2);
					if (getBoolean)
					{
						enum_GenelEvrakTipleri2 = (enum_GenelEvrakTipleri)getInt9;
					}
					else
					{
						num2 = getInt10;
						string text91 = "";
						if (num2 != 0)
						{
							text91 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						array = getString11.Split(',');
						foreach (string text92 in array)
						{
							if (text91 == text92)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.AlinanSiparis;
								break;
							}
						}
						array = getString9.Split(',');
						foreach (string text93 in array)
						{
							if (text91 == text93)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.AlisFaturasi;
								break;
							}
						}
						array = getString10.Split(',');
						foreach (string text94 in array)
						{
							if (text91 == text94)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.AlisIrsaliyesi;
								break;
							}
						}
						array = getString7.Split(',');
						foreach (string text95 in array)
						{
							if (text91 == text95)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.SatisFaturasi;
								break;
							}
						}
						array = getString8.Split(',');
						foreach (string text96 in array)
						{
							if (text91 == text96)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.SatisIrsaliyesi;
								break;
							}
						}
						array = getString12.Split(',');
						foreach (string text97 in array)
						{
							if (text91 == text97)
							{
								enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.DepolarArasiSevk;
								break;
							}
						}
					}
					if (getBoolean2)
					{
						normaliade = (enum_cha_normal_Iade)getInt11;
					}
					else
					{
						num2 = getInt12;
						string text98 = "";
						if (num2 != 0)
						{
							text98 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						array = getString13.Split(',');
						foreach (string text99 in array)
						{
							if (text98 == text99)
							{
								normaliade = enum_cha_normal_Iade.Normal;
								break;
							}
						}
						array = getString14.Split(',');
						foreach (string text100 in array)
						{
							if (text98 == text100)
							{
								normaliade = enum_cha_normal_Iade.Iade;
								break;
							}
						}
					}
					if (getBoolean51)
					{
						ticaretturu = (enum_cha_ticaret_turu)getInt98;
					}
					string text101 = "";
					if (getInt22 != 0)
					{
						text101 = sqlDataReader[getInt22 - 1].ToString().Trim();
					}
					if (text101 == "" && getInt23 != 0)
					{
						text101 = sqlDataReader[getInt23 - 1].ToString().Trim();
					}
					string text102 = "";
					if (getInt20 != 0)
					{
						text102 = sqlDataReader[getInt20 - 1].ToString().Trim();
					}
					if (text102.Length > 50)
					{
						text102 = text102.Substring(0, 50);
					}
					string text103 = "";
					if (getInt21 != 0)
					{
						text103 = sqlDataReader[getInt21 - 1].ToString().Trim();
					}
					if (text103.Length > 50)
					{
						text103 = text103.Substring(0, 50);
					}
					string text104 = "";
					if (getInt25 != 0)
					{
						text104 = sqlDataReader[getInt25 - 1].ToString().Trim();
					}
					if (text104.Length > 30)
					{
						text104 = text104.Substring(0, 30);
					}
					string text105 = "";
					if (getInt33 != 0)
					{
						text105 = sqlDataReader[getInt33 - 1].ToString().Trim();
					}
					if (text105.Length > 80)
					{
						text105 = text105.Substring(0, 80);
					}
					string text106 = "";
					if (getBoolean39)
					{
						text106 = ((enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisFaturasi && enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisIrsaliyesi) ? getString55 : getString56);
					}
					if (getBoolean7)
					{
						text106 += getString19;
					}
					else
					{
						num2 = getInt19;
						string text107 = "";
						if (num2 != 0)
						{
							text107 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text106 += text107;
						if (text107 == "")
						{
							int num9 = getInt293;
							string text108 = "";
							if (num9 != 0)
							{
								text108 = sqlDataReader[num9 - 1].ToString().Trim();
							}
							text106 += text108;
						}
					}
					string[] array3 = getString20.Split(',');
					bool flag12 = false;
					cari2 = new Cari();
					array = array3;
					foreach (string text109 in array)
					{
						if (flag12)
						{
							break;
						}
						switch (text109)
						{
						case "1":
							if (text106 != "")
							{
								cari2 = CariData.GetCariByCariKod(Db.Connection, text106, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag12 = true;
								}
							}
							break;
						case "2":
							if (text101 != "")
							{
								SqlConnection sqlConnection2 = new SqlConnection();
								if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
								{
									sqlConnection2.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Trusted_Connection=true;";
								}
								else
								{
									sqlConnection2.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; User Id=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + ";Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + ";";
								}
								sqlConnection2.Open();
								cari2 = CariData.GetCariByVergiTcKimlikNo(sqlConnection2, text101, AdreslerTemsilciyeGore: false, "");
								sqlConnection2.Close();
								if (cari2.cari_kod != "")
								{
									flag12 = true;
								}
							}
							break;
						case "3":
							if (text105 != "")
							{
								cari2 = CariData.GetCariByEMail(Db.Connection, text105, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag12 = true;
								}
							}
							break;
						case "4":
							if (text104 != "")
							{
								cari2 = CariData.GetCariByBankaHesapNo(Db.Connection, text104, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag12 = true;
								}
							}
							break;
						case "5":
							if (text102 != "")
							{
								cari2 = CariData.GetCariByCariUnvan(Db.Connection, text102, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag12 = true;
								}
							}
							break;
						}
					}
					fiyatlistesi = FiyatListesiData.GetFiyatListesi(Db.Connection, cari2.cari_satis_fk);
					dovizcinsi = cari2.cari_doviz_cinsi;
					odemeplani = cari2.cari_odemeplan_no;
					kaynakdepo = new Depo();
					if (getBoolean35)
					{
						kaynakdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt94);
					}
					if (getBoolean36)
					{
						kaynakdepo = ((enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisFaturasi && enum_GenelEvrakTipleri2 != enum_GenelEvrakTipleri.AlisIrsaliyesi) ? DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, cari2.cari_VarsayilanCikisDepo) : DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, cari2.cari_VarsayilanGirisDepo));
					}
					if (getInt95 != 0)
					{
						try
						{
							kaynakdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, int.Parse(sqlDataReader[getInt95 - 1].ToString().Trim()));
						}
						catch
						{
						}
					}
					hedefdepo = new Depo();
					if (getBoolean37)
					{
						hedefdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt96);
					}
					if (getInt97 != 0)
					{
						try
						{
							hedefdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, int.Parse(sqlDataReader[getInt97 - 1].ToString().Trim()));
						}
						catch
						{
						}
					}
					if (getInt4 != 0)
					{
						dateTime = sqlDataReader.GetSafeDateTime(getInt4 - 1);
					}
					bool flag13 = false;
					if (getBoolean33)
					{
						flag13 = CariData.GetEFaturaCarisiMi(Db.Connection, cari2.cari_kod);
					}
					if (getBoolean32)
					{
						switch (enum_GenelEvrakTipleri2)
						{
						case enum_GenelEvrakTipleri.SatisFaturasi:
							evraknoseri = getString47;
							if (flag13)
							{
								evraknoseri = getString53;
							}
							break;
						case enum_GenelEvrakTipleri.SatisIrsaliyesi:
							evraknoseri = getString48;
							break;
						case enum_GenelEvrakTipleri.AlisFaturasi:
							evraknoseri = getString49;
							if (flag13)
							{
								evraknoseri = getString54;
							}
							break;
						case enum_GenelEvrakTipleri.AlisIrsaliyesi:
							evraknoseri = getString50;
							break;
						case enum_GenelEvrakTipleri.AlinanSiparis:
							evraknoseri = getString51;
							break;
						case enum_GenelEvrakTipleri.DepolarArasiSevk:
							evraknoseri = getString52;
							break;
						}
					}
					else
					{
						num2 = getInt71;
						string text110 = "";
						if (num2 != 0)
						{
							text110 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						evraknoseri = text110;
					}
					if (getInt72 != 0)
					{
						evraknosira = int.Parse(sqlDataReader[getInt72 - 1].ToString().Trim());
					}
					belgetarih = dateTime;
					if (getInt6 != 0)
					{
						belgetarih = sqlDataReader.GetSafeDateTime(getInt6 - 1);
					}
					belgeno = "";
					if (getInt5 != 0)
					{
						belgeno = sqlDataReader[getInt5 - 1].ToString().Trim();
					}
					kur = new Kur();
					if (getInt7 != 0)
					{
						double result21 = 1.0;
						double.TryParse(sqlDataReader[getInt7 - 1].ToString().Trim().Replace(".", ","), out result21);
						kur.dov_fiyat = result21;
					}
					if (getBoolean8)
					{
						kapamasekli = (enum_KapamaSekli)getInt34;
					}
					else
					{
						num2 = getInt35;
						string text111 = "";
						if (num2 != 0)
						{
							text111 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (text111 == getString21)
						{
							kapamasekli = enum_KapamaSekli.AcikHesap;
						}
						if (text111 == getString23)
						{
							kapamasekli = enum_KapamaSekli.BankadanKapanacak;
						}
						if (text111 == getString24)
						{
							kapamasekli = enum_KapamaSekli.CariPersoneldenKapanacak;
						}
						if (text111 == getString22)
						{
							kapamasekli = enum_KapamaSekli.KasadanKapanacak;
						}
					}
					if (getBoolean9)
					{
						kapamahesapkodu = getString25;
					}
					else
					{
						num2 = getInt36;
						string text112 = "";
						if (num2 != 0)
						{
							text112 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						kapamahesapkodu = text112;
					}
					sevkteslimtarihi = dateTime;
					if (getInt8 != 0)
					{
						sevkteslimtarihi = sqlDataReader.GetSafeDateTime(getInt8 - 1);
					}
					proje3 = new Proje();
					string text113 = "";
					if (getBoolean10)
					{
						text113 = getString26;
					}
					else
					{
						num2 = getInt37;
						string text114 = "";
						if (num2 != 0)
						{
							text114 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text113 = text114;
					}
					if (text113 != "")
					{
						proje3 = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text113);
					}
					sorumlulukmerkezi = new SorumlulukMerkezi();
					string text115 = "";
					if (getBoolean11)
					{
						text115 = getString27;
					}
					else
					{
						num2 = getInt38;
						string text116 = "";
						if (num2 != 0)
						{
							text116 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text115 = text116;
					}
					if (text115 != "")
					{
						sorumlulukmerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text115);
					}
					if (getBoolean12)
					{
						temsilcikodu = getString28;
					}
					else
					{
						num2 = getInt39;
						string text117 = "";
						if (num2 != 0)
						{
							text117 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (getBoolean13)
						{
							text117 = cari2.cari_temsilci_kodu;
						}
						temsilcikodu = text117;
					}
					if (getBoolean15)
					{
						text81 = getString30;
					}
					else
					{
						num2 = getInt41;
						string text118 = "";
						if (num2 != 0)
						{
							text118 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text81 = text118;
					}
					if (getBoolean41)
					{
						text81 = getString58 + text81;
					}
					if (getBoolean16)
					{
						text82 = getString31;
					}
					else
					{
						num2 = getInt42;
						string text119 = "";
						if (num2 != 0)
						{
							text119 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text82 = text119;
					}
					if (getBoolean42)
					{
						text82 = getString59 + text82;
					}
					if (getBoolean17)
					{
						text83 = getString32;
					}
					else
					{
						num2 = getInt43;
						string text120 = "";
						if (num2 != 0)
						{
							text120 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text83 = text120;
					}
					if (getBoolean43)
					{
						text83 = getString60 + text83;
					}
					if (getBoolean18)
					{
						text84 = getString33;
					}
					else
					{
						num2 = getInt44;
						string text121 = "";
						if (num2 != 0)
						{
							text121 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text84 = text121;
					}
					if (getBoolean44)
					{
						text84 = getString61 + text84;
					}
					if (getBoolean19)
					{
						text85 = getString34;
					}
					else
					{
						num2 = getInt45;
						string text122 = "";
						if (num2 != 0)
						{
							text122 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text85 = text122;
					}
					if (getBoolean45)
					{
						text85 = getString62 + text85;
					}
					if (getBoolean20)
					{
						text86 = getString35;
					}
					else
					{
						num2 = getInt46;
						string text123 = "";
						if (num2 != 0)
						{
							text123 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text86 = text123;
					}
					if (getBoolean46)
					{
						text86 = getString63 + text86;
					}
					if (getBoolean21)
					{
						text87 = getString36;
					}
					else
					{
						num2 = getInt47;
						string text124 = "";
						if (num2 != 0)
						{
							text124 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text87 = text124;
					}
					if (getBoolean47)
					{
						text87 = getString64 + text87;
					}
					if (getBoolean22)
					{
						text88 = getString37;
					}
					else
					{
						num2 = getInt48;
						string text125 = "";
						if (num2 != 0)
						{
							text125 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text88 = text125;
					}
					if (getBoolean48)
					{
						text88 = getString65 + text88;
					}
					if (getBoolean23)
					{
						text89 = getString38;
					}
					else
					{
						num2 = getInt49;
						string text126 = "";
						if (num2 != 0)
						{
							text126 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text89 = text126;
					}
					if (getBoolean49)
					{
						text89 = getString66 + text89;
					}
					if (getBoolean24)
					{
						text90 = getString39;
					}
					else
					{
						num2 = getInt50;
						string text127 = "";
						if (num2 != 0)
						{
							text127 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text90 = text127;
					}
					if (getBoolean50)
					{
						text90 = getString67 + text90;
					}
					if (getBoolean25)
					{
						degistirspecialalan = getString40;
					}
					else
					{
						num2 = getInt51;
						string text128 = "";
						if (num2 != 0)
						{
							text128 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						degistirspecialalan = text128;
					}
					if (getBoolean26)
					{
						degistirspecialalan2 = getString41;
					}
					else
					{
						num2 = getInt52;
						string text129 = "";
						if (num2 != 0)
						{
							text129 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						degistirspecialalan2 = text129;
					}
					if (getBoolean27)
					{
						degistirspecialalan3 = getString42;
					}
					else
					{
						num2 = getInt53;
						string text130 = "";
						if (num2 != 0)
						{
							text130 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						degistirspecialalan3 = text130;
					}
				}
				num2 = getInt305;
				if (num2 != 0)
				{
					kriter_string = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt306;
				if (num2 != 0)
				{
					kriter_string2 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt307;
				if (num2 != 0)
				{
					kriter_string3 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt308;
				if (num2 != 0)
				{
					kriter_string4 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt309;
				if (num2 != 0)
				{
					kriter_string5 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt310;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result16);
				}
				num2 = getInt311;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result17);
				}
				num2 = getInt312;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result18);
				}
				num2 = getInt313;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result19);
				}
				num2 = getInt314;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result20);
				}
				num2 = getInt315;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString126)
				{
					kriter_bool = true;
				}
				num2 = getInt316;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString127)
				{
					kriter_bool2 = true;
				}
				num2 = getInt317;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString128)
				{
					kriter_bool3 = true;
				}
				num2 = getInt318;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString129)
				{
					kriter_bool4 = true;
				}
				num2 = getInt319;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString130)
				{
					kriter_bool5 = true;
				}
				if (flag11)
				{
					GenelEvrakSatir genelEvrakSatir = new GenelEvrakSatir();
					if (getBoolean78)
					{
						genelEvrakSatir.KayitID = num3;
					}
					else
					{
						num2 = getInt320;
						string s2 = "";
						if (num2 != 0)
						{
							s2 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						int result22 = 0;
						int.TryParse(s2, out result22);
						genelEvrakSatir.KayitID = result22;
					}
					genelEvrakSatir.evraktipi = enum_GenelEvrakTipleri2;
					genelEvrakSatir.normaliade = normaliade;
					genelEvrakSatir.kapamasekli = kapamasekli;
					genelEvrakSatir.ticaretturu = ticaretturu;
					genelEvrakSatir.evraktarih = dateTime;
					genelEvrakSatir.evraknoseri = evraknoseri;
					genelEvrakSatir.evraknosira = evraknosira;
					genelEvrakSatir.belgeno = belgeno;
					genelEvrakSatir.belgetarih = belgetarih;
					genelEvrakSatir.cari = cari2;
					genelEvrakSatir.odemeplani = odemeplani;
					genelEvrakSatir.fiyatlistesi = fiyatlistesi;
					genelEvrakSatir.dovizcinsi = dovizcinsi;
					genelEvrakSatir.kur = kur;
					genelEvrakSatir.alternatifdovizcinsi = alternatifdovizcinsi;
					genelEvrakSatir.alternatifdovizkuru = alternatifdovizkuru;
					genelEvrakSatir.kaynakdepo = kaynakdepo;
					genelEvrakSatir.hedefdepo = hedefdepo;
					genelEvrakSatir.proje = proje3;
					genelEvrakSatir.sorumlulukmerkezi = sorumlulukmerkezi;
					genelEvrakSatir.temsilcikodu = temsilcikodu;
					genelEvrakSatir.firma = firma;
					genelEvrakSatir.sube = sube;
					genelEvrakSatir.DBCno = getInt3;
					genelEvrakSatir.mikrouserno = mikrouserno;
					genelEvrakSatir.sevkteslimtarihi = sevkteslimtarihi;
					genelEvrakSatir.sevkadresno = sevkadresno;
					genelEvrakSatir.kapamahesapkodu = kapamahesapkodu;
					genelEvrakSatir.aciklama1 = text81;
					genelEvrakSatir.aciklama2 = text82;
					genelEvrakSatir.aciklama3 = text83;
					genelEvrakSatir.aciklama4 = text84;
					genelEvrakSatir.aciklama5 = text85;
					genelEvrakSatir.aciklama6 = text86;
					genelEvrakSatir.aciklama7 = text87;
					genelEvrakSatir.aciklama8 = text88;
					genelEvrakSatir.aciklama9 = text89;
					genelEvrakSatir.aciklama10 = text90;
					genelEvrakSatir.degistirspecialalan1 = degistirspecialalan;
					genelEvrakSatir.degistirspecialalan2 = degistirspecialalan2;
					genelEvrakSatir.degistirspecialalan3 = degistirspecialalan3;
					genelEvrakSatir.kriter_string1 = kriter_string;
					genelEvrakSatir.kriter_string2 = kriter_string2;
					genelEvrakSatir.kriter_string3 = kriter_string3;
					genelEvrakSatir.kriter_string4 = kriter_string4;
					genelEvrakSatir.kriter_string5 = kriter_string5;
					genelEvrakSatir.kriter_double1 = result16;
					genelEvrakSatir.kriter_double2 = result17;
					genelEvrakSatir.kriter_double3 = result18;
					genelEvrakSatir.kriter_double4 = result19;
					genelEvrakSatir.kriter_double5 = result20;
					genelEvrakSatir.kriter_bool1 = kriter_bool;
					genelEvrakSatir.kriter_bool2 = kriter_bool2;
					genelEvrakSatir.kriter_bool3 = kriter_bool3;
					genelEvrakSatir.kriter_bool4 = kriter_bool4;
					genelEvrakSatir.kriter_bool5 = kriter_bool5;
					if (getBoolean4)
					{
						genelEvrakSatir.stokhizmetkodu = getString17;
					}
					else
					{
						num2 = getInt15;
						string stokhizmetkodu = "";
						if (num2 != 0)
						{
							stokhizmetkodu = sqlDataReader[num2 - 1].ToString().Trim();
						}
						genelEvrakSatir.stokhizmetkodu = stokhizmetkodu;
					}
					if (getBoolean5)
					{
						genelEvrakSatir.stokpartikodu = getString18;
					}
					else
					{
						num2 = getInt16;
						string stokpartikodu = "";
						if (num2 != 0)
						{
							stokpartikodu = sqlDataReader[num2 - 1].ToString().Trim();
						}
						genelEvrakSatir.stokpartikodu = stokpartikodu;
					}
					if (getBoolean6)
					{
						genelEvrakSatir.stoklotno = getInt17;
					}
					else
					{
						num2 = getInt18;
						string s3 = "";
						if (num2 != 0)
						{
							s3 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						int result23 = 0;
						int.TryParse(s3, out result23);
						genelEvrakSatir.stoklotno = result23;
					}
					if (getBoolean3)
					{
						genelEvrakSatir.satircinsi = (enum_SatirCinsi)getInt13;
					}
					else
					{
						num2 = getInt14;
						string text131 = "";
						if (num2 != 0)
						{
							text131 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (text131 == getString15)
						{
							genelEvrakSatir.satircinsi = enum_SatirCinsi.Stok;
						}
						if (text131 == getString16)
						{
							genelEvrakSatir.satircinsi = enum_SatirCinsi.Hizmet;
						}
					}
					if (getBoolean38)
					{
						Stok stok4 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
						if (!(stok4.sto_kod == "") && stok4.sto_kod != null)
						{
							genelEvrakSatir.satircinsi = enum_SatirCinsi.Stok;
						}
						else
						{
							Hizmet hizmet4 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							if (!(hizmet4.hiz_kod == "") && hizmet4.hiz_kod != null)
							{
								genelEvrakSatir.satircinsi = enum_SatirCinsi.Hizmet;
							}
						}
					}
					switch (genelEvrakSatir.satircinsi)
					{
					case enum_SatirCinsi.Stok:
					{
						Stok stok5 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
						if (stok5.sto_kod == "" || stok5.sto_kod == null)
						{
							Barkod barkodBilgisi2 = BarkodData.GetBarkodBilgisi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							if (barkodBilgisi2.bar_stokkodu != "")
							{
								stok5 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, barkodBilgisi2.bar_stokkodu, enum_toptan_perakende.Toptan);
								if (stok5.sto_kod != "" && stok5.sto_kod != null)
								{
									genelEvrakSatir.stokhizmetkodu = stok5.sto_kod;
								}
							}
						}
						if (stok5.sto_kod == "" || stok5.sto_kod == null)
						{
							genelEvrakSatir.stokhizmetkodu = "";
						}
						break;
					}
					case enum_SatirCinsi.Hizmet:
					{
						Hizmet hizmet5 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
						if (hizmet5.hiz_kod == "" || hizmet5.hiz_kod == null)
						{
							genelEvrakSatir.stokhizmetkodu = "";
						}
						break;
					}
					}
					if (getBoolean74)
					{
						genelEvrakSatir.fiyat_fark_mi = getBoolean75;
					}
					else
					{
						num2 = getInt302;
						string text132 = "";
						if (num2 != 0)
						{
							text132 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (text132 == getString124)
						{
							genelEvrakSatir.fiyat_fark_mi = true;
						}
					}
					if (genelEvrakSatir.satircinsi == enum_SatirCinsi.Stok)
					{
						switch (getInt73)
						{
						case 0:
						{
							Stok stok6 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
							genelEvrakSatir.vergi_pntr = stok6.ekleme_bilgileri.vergi_pntr;
							break;
						}
						case 1:
							genelEvrakSatir.vergi_pntr = getInt74;
							break;
						case 2:
						{
							num2 = getInt75;
							string text133 = "0";
							if (num2 != 0)
							{
								text133 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result24 = 0.0;
							double.TryParse(text133.Replace(".", ","), out result24);
							num = getInt103;
							num2 = getInt104;
							if (num != 0)
							{
								text133 = "";
								if (num2 != 0)
								{
									text133 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result25 = 0.0;
								double.TryParse(text133.Replace(".", ","), out result25);
								switch (num)
								{
								case 1:
									result24 += result25;
									break;
								case 2:
									result24 -= result25;
									break;
								case 3:
									result24 /= result25;
									break;
								case 4:
									result24 *= result25;
									break;
								}
							}
							num = getInt105;
							num2 = getInt106;
							if (num != 0)
							{
								text133 = "";
								if (num2 != 0)
								{
									text133 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result26 = 0.0;
								double.TryParse(text133.Replace(".", ","), out result26);
								switch (num)
								{
								case 1:
									result24 += result26;
									break;
								case 2:
									result24 -= result26;
									break;
								case 3:
									result24 /= result26;
									break;
								case 4:
									result24 *= result26;
									break;
								}
							}
							genelEvrakSatir.vergi_pntr = GenelUtility.GetKdvPntr((int)result24);
							break;
						}
						}
					}
					else
					{
						switch (getInt91)
						{
						case 0:
						{
							Hizmet hizmet6 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							genelEvrakSatir.vergi_pntr = hizmet6.hiz_KDV;
							break;
						}
						case 1:
							genelEvrakSatir.vergi_pntr = getInt92;
							break;
						case 2:
						{
							num2 = getInt93;
							string text134 = "0";
							if (num2 != 0)
							{
								text134 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result27 = 0.0;
							double.TryParse(text134.Replace(".", ","), out result27);
							num = getInt139;
							num2 = getInt140;
							if (num != 0)
							{
								text134 = "";
								if (num2 != 0)
								{
									text134 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result28 = 0.0;
								double.TryParse(text134.Replace(".", ","), out result28);
								switch (num)
								{
								case 1:
									result27 += result28;
									break;
								case 2:
									result27 -= result28;
									break;
								case 3:
									result27 /= result28;
									break;
								case 4:
									result27 *= result28;
									break;
								}
							}
							num = getInt141;
							num2 = getInt142;
							if (num != 0)
							{
								text134 = "";
								if (num2 != 0)
								{
									text134 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result29 = 0.0;
								double.TryParse(text134.Replace(".", ","), out result29);
								switch (num)
								{
								case 1:
									result27 += result29;
									break;
								case 2:
									result27 -= result29;
									break;
								case 3:
									result27 /= result29;
									break;
								case 4:
									result27 *= result29;
									break;
								}
							}
							genelEvrakSatir.vergi_pntr = GenelUtility.GetKdvPntr((int)result27);
							break;
						}
						}
					}
					if (getBoolean14)
					{
						genelEvrakSatir.faturaaciklama = getString29;
					}
					else
					{
						num2 = getInt40;
						string faturaaciklama = "";
						if (num2 != 0)
						{
							faturaaciklama = sqlDataReader[num2 - 1].ToString().Trim();
						}
						genelEvrakSatir.faturaaciklama = faturaaciklama;
					}
					if (getBoolean40)
					{
						genelEvrakSatir.faturaaciklama = getString57 + genelEvrakSatir.faturaaciklama;
					}
					if (getBoolean30)
					{
						double result30 = 1.0;
						double.TryParse(getString45.Replace(".", ","), out result30);
						genelEvrakSatir.miktar = result30;
					}
					else
					{
						num2 = getInt69;
						string text135 = "";
						if (num2 != 0)
						{
							text135 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (text135 != "")
						{
							double result31 = 1.0;
							double.TryParse(text135.Replace(".", ","), out result31);
							genelEvrakSatir.miktar = result31;
						}
					}
					if (getBoolean31)
					{
						double result32 = 0.0;
						double.TryParse(getString46, out result32);
						genelEvrakSatir.miktar2 = result32;
					}
					else
					{
						num2 = getInt70;
						string text136 = "";
						if (num2 != 0)
						{
							text136 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (text136 != "")
						{
							double result33 = 0.0;
							double.TryParse(text136.Replace(".", ","), out result33);
							genelEvrakSatir.miktar2 = result33;
						}
					}
					if (getBoolean28)
					{
						if (genelEvrakSatir.satircinsi == enum_SatirCinsi.Stok)
						{
							string virgulileayrilmiskaynaklar = getString43;
							if (genelEvrakSatir.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || genelEvrakSatir.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
							{
								virgulileayrilmiskaynaklar = getString44;
							}
							List<enum_Fiyat_Kaynagi> fiyatKaynaklariList = FiyatUtility.GetFiyatKaynaklariList(virgulileayrilmiskaynaklar);
							Stok stok7 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
							genelEvrakSatir.birimfiyat = StokData.GetFiyatFromMultiSource(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, stok7.sto_kod, stok7.ekleme_bilgileri.vergi_pntr, genelEvrakSatir.fiyatlistesi.sfl_sirano, genelEvrakSatir.fiyatlistesi.sfl_kdvdahil, genelEvrakSatir.cari.cari_kod, genelEvrakSatir.cari.cari_satis_isk_kod, genelEvrakSatir.kaynakdepo.dep_no, genelEvrakSatir.evraktarih, fiyatKaynaklariList, _mikrouygulamabilgileri.vergitanimlari, 0);
						}
						else
						{
							Hizmet hizmet7 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							genelEvrakSatir.birimfiyat.DovizCinsi = genelEvrakSatir.dovizcinsi;
							genelEvrakSatir.birimfiyat.FiyatBrut = hizmet7.BirimFiyat.FiyatBrut;
						}
					}
					else if (genelEvrakSatir.satircinsi == enum_SatirCinsi.Stok)
					{
						if (getInt54 != 0)
						{
							double result34 = 0.0;
							double.TryParse(sqlDataReader[getInt54 - 1].ToString().Trim().Replace(".", ","), out result34);
							genelEvrakSatir.birimfiyat.FiyatBrut = result34;
						}
						num = getInt99;
						num2 = getInt100;
						if (num != 0)
						{
							string text137 = "";
							if (num2 != 0)
							{
								text137 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result35 = 0.0;
							double.TryParse(text137.Replace(".", ","), out result35);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result35;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result35;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result35;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result35;
								break;
							}
						}
						num = getInt101;
						num2 = getInt102;
						if (num != 0)
						{
							string text138 = "";
							if (num2 != 0)
							{
								text138 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result36 = 0.0;
							double.TryParse(text138.Replace(".", ","), out result36);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result36;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result36;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result36;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result36;
								break;
							}
						}
						if (getBoolean29)
						{
							double yuzde2 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir.vergi_pntr].Yuzde;
							double num10 = 100.0;
							genelEvrakSatir.birimfiyat.FiyatBrut = genelEvrakSatir.birimfiyat.FiyatBrut / (yuzde2 / num10 + 1.0);
						}
						genelEvrakSatir.birimfiyat.Iskonto_1_UygulamaSekli = getInt55;
						genelEvrakSatir.birimfiyat.Iskonto_2_UygulamaSekli = getInt57;
						genelEvrakSatir.birimfiyat.Iskonto_3_UygulamaSekli = getInt59;
						genelEvrakSatir.birimfiyat.Iskonto_4_UygulamaSekli = getInt61;
						genelEvrakSatir.birimfiyat.Iskonto_5_UygulamaSekli = getInt63;
						genelEvrakSatir.birimfiyat.Iskonto_6_UygulamaSekli = getInt65;
						if (getInt56 != 0)
						{
							double result37 = 0.0;
							double.TryParse(sqlDataReader[getInt56 - 1].ToString().Trim().Replace(".", ","), out result37);
							genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result37;
						}
						num = getInt111;
						num2 = getInt112;
						if (num != 0)
						{
							string text139 = "";
							if (num2 != 0)
							{
								text139 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result38 = 0.0;
							double.TryParse(text139.Replace(".", ","), out result38);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result38;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result38;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result38;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result38;
								break;
							}
						}
						num = getInt113;
						num2 = getInt114;
						if (num != 0)
						{
							string text140 = "";
							if (num2 != 0)
							{
								text140 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result39 = 0.0;
							double.TryParse(text140.Replace(".", ","), out result39);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result39;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result39;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result39;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result39;
								break;
							}
						}
						if (getInt58 != 0)
						{
							double result40 = 0.0;
							double.TryParse(sqlDataReader[getInt58 - 1].ToString().Trim().Replace(".", ","), out result40);
							genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result40;
						}
						num = getInt115;
						num2 = getInt116;
						if (num != 0)
						{
							string text141 = "";
							if (num2 != 0)
							{
								text141 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result41 = 0.0;
							double.TryParse(text141.Replace(".", ","), out result41);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result41;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result41;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result41;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result41;
								break;
							}
						}
						num = getInt117;
						num2 = getInt118;
						if (num != 0)
						{
							string text142 = "";
							if (num2 != 0)
							{
								text142 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result42 = 0.0;
							double.TryParse(text142.Replace(".", ","), out result42);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result42;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result42;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result42;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result42;
								break;
							}
						}
						if (getInt60 != 0)
						{
							double result43 = 0.0;
							double.TryParse(sqlDataReader[getInt60 - 1].ToString().Trim().Replace(".", ","), out result43);
							genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result43;
						}
						num = getInt119;
						num2 = getInt120;
						if (num != 0)
						{
							string text143 = "";
							if (num2 != 0)
							{
								text143 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result44 = 0.0;
							double.TryParse(text143.Replace(".", ","), out result44);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result44;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result44;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result44;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result44;
								break;
							}
						}
						num = getInt121;
						num2 = getInt122;
						if (num != 0)
						{
							string text144 = "";
							if (num2 != 0)
							{
								text144 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result45 = 0.0;
							double.TryParse(text144.Replace(".", ","), out result45);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result45;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result45;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result45;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result45;
								break;
							}
						}
						if (getInt62 != 0)
						{
							double result46 = 0.0;
							double.TryParse(sqlDataReader[getInt62 - 1].ToString().Trim().Replace(".", ","), out result46);
							genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result46;
						}
						num = getInt123;
						num2 = getInt124;
						if (num != 0)
						{
							string text145 = "";
							if (num2 != 0)
							{
								text145 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result47 = 0.0;
							double.TryParse(text145.Replace(".", ","), out result47);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result47;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result47;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result47;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result47;
								break;
							}
						}
						num = getInt125;
						num2 = getInt126;
						if (num != 0)
						{
							string text146 = "";
							if (num2 != 0)
							{
								text146 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result48 = 0.0;
							double.TryParse(text146.Replace(".", ","), out result48);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result48;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result48;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result48;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result48;
								break;
							}
						}
						if (getInt64 != 0)
						{
							double result49 = 0.0;
							double.TryParse(sqlDataReader[getInt64 - 1].ToString().Trim().Replace(".", ","), out result49);
							genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result49;
						}
						num = getInt127;
						num2 = getInt128;
						if (num != 0)
						{
							string text147 = "";
							if (num2 != 0)
							{
								text147 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result50 = 0.0;
							double.TryParse(text147.Replace(".", ","), out result50);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result50;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result50;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result50;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result50;
								break;
							}
						}
						num = getInt129;
						num2 = getInt130;
						if (num != 0)
						{
							string text148 = "";
							if (num2 != 0)
							{
								text148 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result51 = 0.0;
							double.TryParse(text148.Replace(".", ","), out result51);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result51;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result51;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result51;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result51;
								break;
							}
						}
						if (getInt66 != 0)
						{
							double result52 = 0.0;
							double.TryParse(sqlDataReader[getInt66 - 1].ToString().Trim().Replace(".", ","), out result52);
							genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result52;
						}
						num = getInt131;
						num2 = getInt132;
						if (num != 0)
						{
							string text149 = "";
							if (num2 != 0)
							{
								text149 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result53 = 0.0;
							double.TryParse(text149.Replace(".", ","), out result53);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result53;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result53;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result53;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result53;
								break;
							}
						}
						num = getInt133;
						num2 = getInt134;
						if (num != 0)
						{
							string text150 = "";
							if (num2 != 0)
							{
								text150 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result54 = 0.0;
							double.TryParse(text150.Replace(".", ","), out result54);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result54;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result54;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result54;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result54;
								break;
							}
						}
						genelEvrakSatir.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt67;
						if (getInt68 != 0)
						{
							double result55 = 0.0;
							double.TryParse(sqlDataReader[getInt68 - 1].ToString().Trim().Replace(".", ","), out result55);
							genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar = result55;
						}
						num = getInt107;
						num2 = getInt108;
						if (num != 0)
						{
							string text151 = "";
							if (num2 != 0)
							{
								text151 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result56 = 0.0;
							double.TryParse(text151.Replace(".", ","), out result56);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result56;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result56;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result56;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result56;
								break;
							}
						}
						num = getInt109;
						num2 = getInt110;
						if (num != 0)
						{
							string text152 = "";
							if (num2 != 0)
							{
								text152 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result57 = 0.0;
							double.TryParse(text152.Replace(".", ","), out result57);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result57;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result57;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result57;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result57;
								break;
							}
						}
					}
					else
					{
						if (getInt76 != 0)
						{
							double result58 = 0.0;
							double.TryParse(sqlDataReader[getInt76 - 1].ToString().Trim().Replace(".", ","), out result58);
							genelEvrakSatir.birimfiyat.FiyatBrut = result58;
						}
						num = getInt135;
						num2 = getInt136;
						if (num != 0)
						{
							string text153 = "";
							if (num2 != 0)
							{
								text153 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result59 = 0.0;
							double.TryParse(text153.Replace(".", ","), out result59);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result59;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result59;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result59;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result59;
								break;
							}
						}
						num = getInt137;
						num2 = getInt138;
						if (num != 0)
						{
							string text154 = "";
							if (num2 != 0)
							{
								text154 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result60 = 0.0;
							double.TryParse(text154.Replace(".", ","), out result60);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.FiyatBrut += result60;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.FiyatBrut -= result60;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.FiyatBrut /= result60;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.FiyatBrut *= result60;
								break;
							}
						}
						if (getBoolean34)
						{
							double yuzde3 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir.vergi_pntr].Yuzde;
							double num11 = 100.0;
							genelEvrakSatir.birimfiyat.FiyatBrut = genelEvrakSatir.birimfiyat.FiyatBrut / (yuzde3 / num11 + 1.0);
						}
						genelEvrakSatir.birimfiyat.Iskonto_1_UygulamaSekli = getInt77;
						genelEvrakSatir.birimfiyat.Iskonto_2_UygulamaSekli = getInt79;
						genelEvrakSatir.birimfiyat.Iskonto_3_UygulamaSekli = getInt81;
						genelEvrakSatir.birimfiyat.Iskonto_4_UygulamaSekli = getInt83;
						genelEvrakSatir.birimfiyat.Iskonto_5_UygulamaSekli = getInt85;
						genelEvrakSatir.birimfiyat.Iskonto_6_UygulamaSekli = getInt87;
						if (getInt78 != 0)
						{
							double result61 = 0.0;
							double.TryParse(sqlDataReader[getInt78 - 1].ToString().Trim().Replace(".", ","), out result61);
							genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result61;
						}
						num = getInt147;
						num2 = getInt148;
						if (num != 0)
						{
							string text155 = "";
							if (num2 != 0)
							{
								text155 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result62 = 0.0;
							double.TryParse(text155.Replace(".", ","), out result62);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result62;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result62;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result62;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result62;
								break;
							}
						}
						num = getInt149;
						num2 = getInt150;
						if (num != 0)
						{
							string text156 = "";
							if (num2 != 0)
							{
								text156 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result63 = 0.0;
							double.TryParse(text156.Replace(".", ","), out result63);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result63;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result63;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result63;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result63;
								break;
							}
						}
						if (getInt80 != 0)
						{
							double result64 = 0.0;
							double.TryParse(sqlDataReader[getInt80 - 1].ToString().Trim().Replace(".", ","), out result64);
							genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result64;
						}
						num = getInt151;
						num2 = getInt152;
						if (num != 0)
						{
							string text157 = "";
							if (num2 != 0)
							{
								text157 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result65 = 0.0;
							double.TryParse(text157.Replace(".", ","), out result65);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result65;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result65;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result65;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result65;
								break;
							}
						}
						num = getInt153;
						num2 = getInt154;
						if (num != 0)
						{
							string text158 = "";
							if (num2 != 0)
							{
								text158 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result66 = 0.0;
							double.TryParse(text158.Replace(".", ","), out result66);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result66;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result66;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result66;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result66;
								break;
							}
						}
						if (getInt82 != 0)
						{
							double result67 = 0.0;
							double.TryParse(sqlDataReader[getInt82 - 1].ToString().Trim().Replace(".", ","), out result67);
							genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result67;
						}
						num = getInt155;
						num2 = getInt156;
						if (num != 0)
						{
							string text159 = "";
							if (num2 != 0)
							{
								text159 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result68 = 0.0;
							double.TryParse(text159.Replace(".", ","), out result68);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result68;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result68;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result68;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result68;
								break;
							}
						}
						num = getInt157;
						num2 = getInt158;
						if (num != 0)
						{
							string text160 = "";
							if (num2 != 0)
							{
								text160 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result69 = 0.0;
							double.TryParse(text160.Replace(".", ","), out result69);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result69;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result69;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result69;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result69;
								break;
							}
						}
						if (getInt84 != 0)
						{
							double result70 = 0.0;
							double.TryParse(sqlDataReader[getInt84 - 1].ToString().Trim().Replace(".", ","), out result70);
							genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result70;
						}
						num = getInt159;
						num2 = getInt160;
						if (num != 0)
						{
							string text161 = "";
							if (num2 != 0)
							{
								text161 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result71 = 0.0;
							double.TryParse(text161.Replace(".", ","), out result71);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result71;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result71;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result71;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result71;
								break;
							}
						}
						num = getInt161;
						num2 = getInt162;
						if (num != 0)
						{
							string text162 = "";
							if (num2 != 0)
							{
								text162 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result72 = 0.0;
							double.TryParse(text162.Replace(".", ","), out result72);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result72;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result72;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result72;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result72;
								break;
							}
						}
						if (getInt86 != 0)
						{
							double result73 = 0.0;
							double.TryParse(sqlDataReader[getInt86 - 1].ToString().Trim().Replace(".", ","), out result73);
							genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result73;
						}
						num = getInt163;
						num2 = getInt164;
						if (num != 0)
						{
							string text163 = "";
							if (num2 != 0)
							{
								text163 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result74 = 0.0;
							double.TryParse(text163.Replace(".", ","), out result74);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result74;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result74;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result74;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result74;
								break;
							}
						}
						num = getInt165;
						num2 = getInt166;
						if (num != 0)
						{
							string text164 = "";
							if (num2 != 0)
							{
								text164 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result75 = 0.0;
							double.TryParse(text164.Replace(".", ","), out result75);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result75;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result75;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result75;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result75;
								break;
							}
						}
						if (getInt88 != 0)
						{
							double result76 = 0.0;
							double.TryParse(sqlDataReader[getInt88 - 1].ToString().Trim().Replace(".", ","), out result76);
							genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result76;
						}
						num = getInt167;
						num2 = getInt168;
						if (num != 0)
						{
							string text165 = "";
							if (num2 != 0)
							{
								text165 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result77 = 0.0;
							double.TryParse(text165.Replace(".", ","), out result77);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result77;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result77;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result77;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result77;
								break;
							}
						}
						num = getInt169;
						num2 = getInt170;
						if (num != 0)
						{
							string text166 = "";
							if (num2 != 0)
							{
								text166 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result78 = 0.0;
							double.TryParse(text166.Replace(".", ","), out result78);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result78;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result78;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result78;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result78;
								break;
							}
						}
						genelEvrakSatir.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt89;
						if (getInt90 != 0)
						{
							double result79 = 0.0;
							double.TryParse(sqlDataReader[getInt90 - 1].ToString().Trim().Replace(".", ","), out result79);
							genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar = result79;
						}
						num = getInt143;
						num2 = getInt144;
						if (num != 0)
						{
							string text167 = "";
							if (num2 != 0)
							{
								text167 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result80 = 0.0;
							double.TryParse(text167.Replace(".", ","), out result80);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result80;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result80;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result80;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result80;
								break;
							}
						}
						num = getInt145;
						num2 = getInt146;
						if (num != 0)
						{
							string text168 = "";
							if (num2 != 0)
							{
								text168 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							double result81 = 0.0;
							double.TryParse(text168.Replace(".", ","), out result81);
							switch (num)
							{
							case 1:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar += result81;
								break;
							case 2:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar -= result81;
								break;
							case 3:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar /= result81;
								break;
							case 4:
								genelEvrakSatir.birimfiyat.OtvYuzdeVeyaTutar *= result81;
								break;
							}
						}
					}
					genelEvrakSatir.birimfiyat.OtvVergiPntr = getInt304;
					if (genelEvrakSatir.birimfiyat.FiyatBrut != 0.0)
					{
						if (genelEvrakSatir.birimfiyat.FiyatBrut < 0.0 && getBoolean65)
						{
							genelEvrakSatir.evraktipi = (enum_GenelEvrakTipleri)getInt291;
						}
						if (genelEvrakSatir.birimfiyat.FiyatBrut < 0.0)
						{
							genelEvrakSatir.birimfiyat.FiyatBrut *= -1.0;
						}
						bindingList.Add(genelEvrakSatir);
					}
					if (getBoolean53)
					{
						GenelEvrakSatir genelEvrakSatir2 = new GenelEvrakSatir();
						if (getBoolean78)
						{
							genelEvrakSatir2.KayitID = 1000000000 + num3;
						}
						else
						{
							num2 = getInt320;
							string s4 = "";
							if (num2 != 0)
							{
								s4 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							int result82 = 0;
							int.TryParse(s4, out result82);
							genelEvrakSatir2.KayitID = 1000000000 + result82;
						}
						genelEvrakSatir2.evraktipi = enum_GenelEvrakTipleri2;
						genelEvrakSatir2.normaliade = normaliade;
						genelEvrakSatir2.kapamasekli = kapamasekli;
						genelEvrakSatir2.ticaretturu = ticaretturu;
						genelEvrakSatir2.evraktarih = dateTime;
						genelEvrakSatir2.evraknoseri = evraknoseri;
						genelEvrakSatir2.evraknosira = evraknosira;
						genelEvrakSatir2.belgeno = belgeno;
						genelEvrakSatir2.belgetarih = belgetarih;
						genelEvrakSatir2.cari = cari2;
						genelEvrakSatir2.odemeplani = odemeplani;
						genelEvrakSatir2.fiyatlistesi = fiyatlistesi;
						genelEvrakSatir2.dovizcinsi = dovizcinsi;
						genelEvrakSatir2.kur = kur;
						genelEvrakSatir2.alternatifdovizcinsi = alternatifdovizcinsi;
						genelEvrakSatir2.alternatifdovizkuru = alternatifdovizkuru;
						genelEvrakSatir2.kaynakdepo = kaynakdepo;
						genelEvrakSatir2.hedefdepo = hedefdepo;
						genelEvrakSatir2.proje = proje3;
						genelEvrakSatir2.sorumlulukmerkezi = sorumlulukmerkezi;
						genelEvrakSatir2.temsilcikodu = temsilcikodu;
						genelEvrakSatir2.firma = firma;
						genelEvrakSatir2.sube = sube;
						genelEvrakSatir2.mikrouserno = mikrouserno;
						genelEvrakSatir2.sevkteslimtarihi = sevkteslimtarihi;
						genelEvrakSatir2.sevkadresno = sevkadresno;
						genelEvrakSatir2.kapamahesapkodu = kapamahesapkodu;
						genelEvrakSatir2.aciklama1 = text81;
						genelEvrakSatir2.aciklama2 = text82;
						genelEvrakSatir2.aciklama3 = text83;
						genelEvrakSatir2.aciklama4 = text84;
						genelEvrakSatir2.aciklama5 = text85;
						genelEvrakSatir2.aciklama6 = text86;
						genelEvrakSatir2.aciklama7 = text87;
						genelEvrakSatir2.aciklama8 = text88;
						genelEvrakSatir2.aciklama9 = text89;
						genelEvrakSatir2.aciklama10 = text90;
						genelEvrakSatir2.degistirspecialalan1 = degistirspecialalan;
						genelEvrakSatir2.degistirspecialalan2 = degistirspecialalan2;
						genelEvrakSatir2.degistirspecialalan3 = degistirspecialalan3;
						genelEvrakSatir2.kriter_string1 = kriter_string;
						genelEvrakSatir2.kriter_string2 = kriter_string2;
						genelEvrakSatir2.kriter_string3 = kriter_string3;
						genelEvrakSatir2.kriter_string4 = kriter_string4;
						genelEvrakSatir2.kriter_string5 = kriter_string5;
						genelEvrakSatir2.kriter_double1 = result16;
						genelEvrakSatir2.kriter_double2 = result17;
						genelEvrakSatir2.kriter_double3 = result18;
						genelEvrakSatir2.kriter_double4 = result19;
						genelEvrakSatir2.kriter_double5 = result20;
						genelEvrakSatir2.kriter_bool1 = kriter_bool;
						genelEvrakSatir2.kriter_bool2 = kriter_bool2;
						genelEvrakSatir2.kriter_bool3 = kriter_bool3;
						genelEvrakSatir2.kriter_bool4 = kriter_bool4;
						genelEvrakSatir2.kriter_bool5 = kriter_bool5;
						if (getBoolean55)
						{
							genelEvrakSatir2.stokhizmetkodu = getString70;
						}
						else
						{
							num2 = getInt173;
							string stokhizmetkodu2 = "";
							if (num2 != 0)
							{
								stokhizmetkodu2 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							genelEvrakSatir2.stokhizmetkodu = stokhizmetkodu2;
						}
						if (getBoolean56)
						{
							genelEvrakSatir2.stokpartikodu = getString71;
						}
						else
						{
							num2 = getInt174;
							string stokpartikodu2 = "";
							if (num2 != 0)
							{
								stokpartikodu2 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							genelEvrakSatir2.stokpartikodu = stokpartikodu2;
						}
						if (getBoolean57)
						{
							genelEvrakSatir2.stoklotno = getInt175;
						}
						else
						{
							num2 = getInt176;
							string s5 = "";
							if (num2 != 0)
							{
								s5 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							int result83 = 0;
							int.TryParse(s5, out result83);
							genelEvrakSatir2.stoklotno = result83;
						}
						if (getBoolean54)
						{
							genelEvrakSatir2.satircinsi = (enum_SatirCinsi)getInt171;
						}
						else
						{
							num2 = getInt172;
							string text169 = "";
							if (num2 != 0)
							{
								text169 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							if (text169 == getString68)
							{
								genelEvrakSatir2.satircinsi = enum_SatirCinsi.Stok;
							}
							if (text169 == getString69)
							{
								genelEvrakSatir2.satircinsi = enum_SatirCinsi.Hizmet;
							}
						}
						if (getBoolean38)
						{
							Stok stok8 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu, enum_toptan_perakende.Toptan);
							if (!(stok8.sto_kod == "") && stok8.sto_kod != null)
							{
								genelEvrakSatir2.satircinsi = enum_SatirCinsi.Stok;
							}
							else
							{
								Hizmet hizmet8 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu);
								if (!(hizmet8.hiz_kod == "") && hizmet8.hiz_kod != null)
								{
									genelEvrakSatir2.satircinsi = enum_SatirCinsi.Hizmet;
								}
							}
						}
						switch (genelEvrakSatir.satircinsi)
						{
						case enum_SatirCinsi.Stok:
						{
							Stok stok9 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu, enum_toptan_perakende.Toptan);
							if (stok9.sto_kod == "" || stok9.sto_kod == null)
							{
								Barkod barkodBilgisi3 = BarkodData.GetBarkodBilgisi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
								if (barkodBilgisi3.bar_stokkodu != "")
								{
									stok9 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, barkodBilgisi3.bar_stokkodu, enum_toptan_perakende.Toptan);
									if (stok9.sto_kod != "" && stok9.sto_kod != null)
									{
										genelEvrakSatir.stokhizmetkodu = stok9.sto_kod;
									}
								}
							}
							if (stok9.sto_kod == "" || stok9.sto_kod == null)
							{
								genelEvrakSatir.stokhizmetkodu = "";
							}
							break;
						}
						case enum_SatirCinsi.Hizmet:
						{
							Hizmet hizmet9 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir.stokhizmetkodu);
							if (hizmet9.hiz_kod == "" || hizmet9.hiz_kod == null)
							{
								genelEvrakSatir.stokhizmetkodu = "";
							}
							break;
						}
						}
						if (getBoolean76)
						{
							genelEvrakSatir2.fiyat_fark_mi = getBoolean77;
						}
						else
						{
							num2 = getInt303;
							string text170 = "";
							if (num2 != 0)
							{
								text170 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							if (text170 == getString125)
							{
								genelEvrakSatir2.fiyat_fark_mi = true;
							}
						}
						if (genelEvrakSatir2.satircinsi == enum_SatirCinsi.Stok)
						{
							switch (getInt195)
							{
							case 0:
							{
								Stok stok10 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu, enum_toptan_perakende.Toptan);
								genelEvrakSatir2.vergi_pntr = stok10.ekleme_bilgileri.vergi_pntr;
								break;
							}
							case 1:
								genelEvrakSatir2.vergi_pntr = getInt196;
								break;
							case 2:
							{
								num2 = getInt197;
								string text171 = "0";
								if (num2 != 0)
								{
									text171 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result84 = 0.0;
								double.TryParse(text171.Replace(".", ","), out result84);
								num = getInt220;
								num2 = getInt221;
								if (num != 0)
								{
									text171 = "";
									if (num2 != 0)
									{
										text171 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result85 = 0.0;
									double.TryParse(text171.Replace(".", ","), out result85);
									switch (num)
									{
									case 1:
										result84 += result85;
										break;
									case 2:
										result84 -= result85;
										break;
									case 3:
										result84 /= result85;
										break;
									case 4:
										result84 *= result85;
										break;
									}
								}
								num = getInt222;
								num2 = getInt223;
								if (num != 0)
								{
									text171 = "";
									if (num2 != 0)
									{
										text171 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result86 = 0.0;
									double.TryParse(text171.Replace(".", ","), out result86);
									switch (num)
									{
									case 1:
										result84 += result86;
										break;
									case 2:
										result84 -= result86;
										break;
									case 3:
										result84 /= result86;
										break;
									case 4:
										result84 *= result86;
										break;
									}
								}
								genelEvrakSatir2.vergi_pntr = GenelUtility.GetKdvPntr((int)result84);
								break;
							}
							}
						}
						else
						{
							switch (getInt213)
							{
							case 0:
							{
								Hizmet hizmet10 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu);
								genelEvrakSatir2.vergi_pntr = hizmet10.hiz_KDV;
								break;
							}
							case 1:
								genelEvrakSatir2.vergi_pntr = getInt214;
								break;
							case 2:
							{
								num2 = getInt215;
								string text172 = "0";
								if (num2 != 0)
								{
									text172 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result87 = 0.0;
								double.TryParse(text172.Replace(".", ","), out result87);
								num = getInt256;
								num2 = getInt257;
								if (num != 0)
								{
									text172 = "";
									if (num2 != 0)
									{
										text172 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result88 = 0.0;
									double.TryParse(text172.Replace(".", ","), out result88);
									switch (num)
									{
									case 1:
										result87 += result88;
										break;
									case 2:
										result87 -= result88;
										break;
									case 3:
										result87 /= result88;
										break;
									case 4:
										result87 *= result88;
										break;
									}
								}
								num = getInt258;
								num2 = getInt259;
								if (num != 0)
								{
									text172 = "";
									if (num2 != 0)
									{
										text172 = sqlDataReader[num2 - 1].ToString().Trim();
									}
									double result89 = 0.0;
									double.TryParse(text172.Replace(".", ","), out result89);
									switch (num)
									{
									case 1:
										result87 += result89;
										break;
									case 2:
										result87 -= result89;
										break;
									case 3:
										result87 /= result89;
										break;
									case 4:
										result87 *= result89;
										break;
									}
								}
								genelEvrakSatir2.vergi_pntr = GenelUtility.GetKdvPntr((int)result87);
								break;
							}
							}
						}
						if (getBoolean58)
						{
							genelEvrakSatir2.faturaaciklama = getString72;
						}
						else
						{
							num2 = getInt177;
							string faturaaciklama2 = "";
							if (num2 != 0)
							{
								faturaaciklama2 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							genelEvrakSatir2.faturaaciklama = faturaaciklama2;
						}
						if (getBoolean64)
						{
							genelEvrakSatir2.faturaaciklama = getString77 + genelEvrakSatir2.faturaaciklama;
						}
						if (getBoolean61)
						{
							double result90 = 1.0;
							double.TryParse(getString75.Replace(".", ","), out result90);
							genelEvrakSatir2.miktar = result90;
						}
						else
						{
							num2 = getInt193;
							string text173 = "";
							if (num2 != 0)
							{
								text173 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							if (text173 != "")
							{
								double result91 = 1.0;
								double.TryParse(text173.Replace(".", ","), out result91);
								genelEvrakSatir2.miktar = result91;
							}
						}
						if (getBoolean62)
						{
							double result92 = 0.0;
							double.TryParse(getString76, out result92);
							genelEvrakSatir2.miktar2 = result92;
						}
						else
						{
							num2 = getInt194;
							string text174 = "";
							if (num2 != 0)
							{
								text174 = sqlDataReader[num2 - 1].ToString().Trim();
							}
							if (text174 != "")
							{
								double result93 = 0.0;
								double.TryParse(text174.Replace(".", ","), out result93);
								genelEvrakSatir2.miktar2 = result93;
							}
						}
						if (getBoolean59)
						{
							if (genelEvrakSatir2.satircinsi == enum_SatirCinsi.Stok)
							{
								string virgulileayrilmiskaynaklar2 = getString73;
								if (genelEvrakSatir2.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || genelEvrakSatir2.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
								{
									virgulileayrilmiskaynaklar2 = getString74;
								}
								List<enum_Fiyat_Kaynagi> fiyatKaynaklariList2 = FiyatUtility.GetFiyatKaynaklariList(virgulileayrilmiskaynaklar2);
								Stok stok11 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu, enum_toptan_perakende.Toptan);
								genelEvrakSatir2.birimfiyat = StokData.GetFiyatFromMultiSource(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, stok11.sto_kod, stok11.ekleme_bilgileri.vergi_pntr, genelEvrakSatir2.fiyatlistesi.sfl_sirano, genelEvrakSatir2.fiyatlistesi.sfl_kdvdahil, genelEvrakSatir2.cari.cari_kod, genelEvrakSatir2.cari.cari_satis_isk_kod, genelEvrakSatir2.kaynakdepo.dep_no, genelEvrakSatir2.evraktarih, fiyatKaynaklariList2, _mikrouygulamabilgileri.vergitanimlari, 0);
							}
							else
							{
								Hizmet hizmet11 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, genelEvrakSatir2.stokhizmetkodu);
								genelEvrakSatir2.birimfiyat.DovizCinsi = genelEvrakSatir2.dovizcinsi;
								genelEvrakSatir2.birimfiyat.FiyatBrut = hizmet11.BirimFiyat.FiyatBrut;
							}
						}
						else if (genelEvrakSatir2.satircinsi == enum_SatirCinsi.Stok)
						{
							if (getInt178 != 0)
							{
								double result94 = 0.0;
								double.TryParse(sqlDataReader[getInt178 - 1].ToString().Trim().Replace(".", ","), out result94);
								genelEvrakSatir2.birimfiyat.FiyatBrut = result94;
							}
							num = getInt216;
							num2 = getInt217;
							if (num != 0)
							{
								string text175 = "";
								if (num2 != 0)
								{
									text175 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result95 = 0.0;
								double.TryParse(text175.Replace(".", ","), out result95);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result95;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result95;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result95;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result95;
									break;
								}
							}
							num = getInt218;
							num2 = getInt219;
							if (num != 0)
							{
								string text176 = "";
								if (num2 != 0)
								{
									text176 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result96 = 0.0;
								double.TryParse(text176.Replace(".", ","), out result96);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result96;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result96;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result96;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result96;
									break;
								}
							}
							if (getBoolean60)
							{
								double yuzde4 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir2.vergi_pntr].Yuzde;
								double num12 = 100.0;
								genelEvrakSatir2.birimfiyat.FiyatBrut = genelEvrakSatir2.birimfiyat.FiyatBrut / (yuzde4 / num12 + 1.0);
							}
							genelEvrakSatir2.birimfiyat.Iskonto_1_UygulamaSekli = getInt179;
							genelEvrakSatir2.birimfiyat.Iskonto_2_UygulamaSekli = getInt181;
							genelEvrakSatir2.birimfiyat.Iskonto_3_UygulamaSekli = getInt183;
							genelEvrakSatir2.birimfiyat.Iskonto_4_UygulamaSekli = getInt185;
							genelEvrakSatir2.birimfiyat.Iskonto_5_UygulamaSekli = getInt187;
							genelEvrakSatir2.birimfiyat.Iskonto_6_UygulamaSekli = getInt189;
							if (getInt180 != 0)
							{
								double result97 = 0.0;
								double.TryParse(sqlDataReader[getInt180 - 1].ToString().Trim().Replace(".", ","), out result97);
								genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result97;
							}
							num = getInt228;
							num2 = getInt229;
							if (num != 0)
							{
								string text177 = "";
								if (num2 != 0)
								{
									text177 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result98 = 0.0;
								double.TryParse(text177.Replace(".", ","), out result98);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result98;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result98;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result98;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result98;
									break;
								}
							}
							num = getInt230;
							num2 = getInt231;
							if (num != 0)
							{
								string text178 = "";
								if (num2 != 0)
								{
									text178 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result99 = 0.0;
								double.TryParse(text178.Replace(".", ","), out result99);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result99;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result99;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result99;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result99;
									break;
								}
							}
							if (getInt182 != 0)
							{
								double result100 = 0.0;
								double.TryParse(sqlDataReader[getInt182 - 1].ToString().Trim().Replace(".", ","), out result100);
								genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result100;
							}
							num = getInt232;
							num2 = getInt233;
							if (num != 0)
							{
								string text179 = "";
								if (num2 != 0)
								{
									text179 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result101 = 0.0;
								double.TryParse(text179.Replace(".", ","), out result101);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result101;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result101;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result101;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result101;
									break;
								}
							}
							num = getInt234;
							num2 = getInt235;
							if (num != 0)
							{
								string text180 = "";
								if (num2 != 0)
								{
									text180 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result102 = 0.0;
								double.TryParse(text180.Replace(".", ","), out result102);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result102;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result102;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result102;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result102;
									break;
								}
							}
							if (getInt184 != 0)
							{
								double result103 = 0.0;
								double.TryParse(sqlDataReader[getInt184 - 1].ToString().Trim().Replace(".", ","), out result103);
								genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result103;
							}
							num = getInt236;
							num2 = getInt237;
							if (num != 0)
							{
								string text181 = "";
								if (num2 != 0)
								{
									text181 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result104 = 0.0;
								double.TryParse(text181.Replace(".", ","), out result104);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result104;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result104;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result104;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result104;
									break;
								}
							}
							num = getInt238;
							num2 = getInt239;
							if (num != 0)
							{
								string text182 = "";
								if (num2 != 0)
								{
									text182 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result105 = 0.0;
								double.TryParse(text182.Replace(".", ","), out result105);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result105;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result105;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result105;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result105;
									break;
								}
							}
							if (getInt186 != 0)
							{
								double result106 = 0.0;
								double.TryParse(sqlDataReader[getInt186 - 1].ToString().Trim().Replace(".", ","), out result106);
								genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result106;
							}
							num = getInt240;
							num2 = getInt241;
							if (num != 0)
							{
								string text183 = "";
								if (num2 != 0)
								{
									text183 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result107 = 0.0;
								double.TryParse(text183.Replace(".", ","), out result107);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result107;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result107;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result107;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result107;
									break;
								}
							}
							num = getInt242;
							num2 = getInt243;
							if (num != 0)
							{
								string text184 = "";
								if (num2 != 0)
								{
									text184 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result108 = 0.0;
								double.TryParse(text184.Replace(".", ","), out result108);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result108;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result108;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result108;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result108;
									break;
								}
							}
							if (getInt188 != 0)
							{
								double result109 = 0.0;
								double.TryParse(sqlDataReader[getInt188 - 1].ToString().Trim().Replace(".", ","), out result109);
								genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result109;
							}
							num = getInt244;
							num2 = getInt245;
							if (num != 0)
							{
								string text185 = "";
								if (num2 != 0)
								{
									text185 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result110 = 0.0;
								double.TryParse(text185.Replace(".", ","), out result110);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result110;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result110;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result110;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result110;
									break;
								}
							}
							num = getInt246;
							num2 = getInt247;
							if (num != 0)
							{
								string text186 = "";
								if (num2 != 0)
								{
									text186 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result111 = 0.0;
								double.TryParse(text186.Replace(".", ","), out result111);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result111;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result111;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result111;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result111;
									break;
								}
							}
							if (getInt190 != 0)
							{
								double result112 = 0.0;
								double.TryParse(sqlDataReader[getInt190 - 1].ToString().Trim().Replace(".", ","), out result112);
								genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result112;
							}
							num = getInt248;
							num2 = getInt249;
							if (num != 0)
							{
								string text187 = "";
								if (num2 != 0)
								{
									text187 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result113 = 0.0;
								double.TryParse(text187.Replace(".", ","), out result113);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result113;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result113;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result113;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result113;
									break;
								}
							}
							num = getInt250;
							num2 = getInt251;
							if (num != 0)
							{
								string text188 = "";
								if (num2 != 0)
								{
									text188 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result114 = 0.0;
								double.TryParse(text188.Replace(".", ","), out result114);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result114;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result114;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result114;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result114;
									break;
								}
							}
							genelEvrakSatir2.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt191;
							if (getInt192 != 0)
							{
								double result115 = 0.0;
								double.TryParse(sqlDataReader[getInt192 - 1].ToString().Trim().Replace(".", ","), out result115);
								genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar = result115;
							}
							num = getInt224;
							num2 = getInt225;
							if (num != 0)
							{
								string text189 = "";
								if (num2 != 0)
								{
									text189 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result116 = 0.0;
								double.TryParse(text189.Replace(".", ","), out result116);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result116;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result116;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result116;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result116;
									break;
								}
							}
							num = getInt226;
							num2 = getInt227;
							if (num != 0)
							{
								string text190 = "";
								if (num2 != 0)
								{
									text190 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result117 = 0.0;
								double.TryParse(text190.Replace(".", ","), out result117);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result117;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result117;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result117;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result117;
									break;
								}
							}
						}
						else
						{
							if (getInt198 != 0)
							{
								double result118 = 0.0;
								double.TryParse(sqlDataReader[getInt198 - 1].ToString().Trim().Replace(".", ","), out result118);
								genelEvrakSatir2.birimfiyat.FiyatBrut = result118;
							}
							num = getInt252;
							num2 = getInt253;
							if (num != 0)
							{
								string text191 = "";
								if (num2 != 0)
								{
									text191 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result119 = 0.0;
								double.TryParse(text191.Replace(".", ","), out result119);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result119;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result119;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result119;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result119;
									break;
								}
							}
							num = getInt254;
							num2 = getInt255;
							if (num != 0)
							{
								string text192 = "";
								if (num2 != 0)
								{
									text192 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result120 = 0.0;
								double.TryParse(text192.Replace(".", ","), out result120);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.FiyatBrut += result120;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.FiyatBrut -= result120;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.FiyatBrut /= result120;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.FiyatBrut *= result120;
									break;
								}
							}
							if (getBoolean63)
							{
								double yuzde5 = _mikrouygulamabilgileri.vergitanimlari[genelEvrakSatir2.vergi_pntr].Yuzde;
								double num13 = 100.0;
								genelEvrakSatir2.birimfiyat.FiyatBrut = genelEvrakSatir2.birimfiyat.FiyatBrut / (yuzde5 / num13 + 1.0);
							}
							genelEvrakSatir2.birimfiyat.Iskonto_1_UygulamaSekli = getInt199;
							genelEvrakSatir2.birimfiyat.Iskonto_2_UygulamaSekli = getInt201;
							genelEvrakSatir2.birimfiyat.Iskonto_3_UygulamaSekli = getInt203;
							genelEvrakSatir2.birimfiyat.Iskonto_4_UygulamaSekli = getInt205;
							genelEvrakSatir2.birimfiyat.Iskonto_5_UygulamaSekli = getInt207;
							genelEvrakSatir2.birimfiyat.Iskonto_6_UygulamaSekli = getInt209;
							if (getInt200 != 0)
							{
								double result121 = 0.0;
								double.TryParse(sqlDataReader[getInt200 - 1].ToString().Trim().Replace(".", ","), out result121);
								genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result121;
							}
							num = getInt264;
							num2 = getInt265;
							if (num != 0)
							{
								string text193 = "";
								if (num2 != 0)
								{
									text193 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result122 = 0.0;
								double.TryParse(text193.Replace(".", ","), out result122);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result122;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result122;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result122;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result122;
									break;
								}
							}
							num = getInt266;
							num2 = getInt267;
							if (num != 0)
							{
								string text194 = "";
								if (num2 != 0)
								{
									text194 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result123 = 0.0;
								double.TryParse(text194.Replace(".", ","), out result123);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar += result123;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar -= result123;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar /= result123;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_1_YuzdeVeyaMiktar *= result123;
									break;
								}
							}
							if (getInt202 != 0)
							{
								double result124 = 0.0;
								double.TryParse(sqlDataReader[getInt202 - 1].ToString().Trim().Replace(".", ","), out result124);
								genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result124;
							}
							num = getInt268;
							num2 = getInt269;
							if (num != 0)
							{
								string text195 = "";
								if (num2 != 0)
								{
									text195 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result125 = 0.0;
								double.TryParse(text195.Replace(".", ","), out result125);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result125;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result125;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result125;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result125;
									break;
								}
							}
							num = getInt270;
							num2 = getInt271;
							if (num != 0)
							{
								string text196 = "";
								if (num2 != 0)
								{
									text196 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result126 = 0.0;
								double.TryParse(text196.Replace(".", ","), out result126);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar += result126;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar -= result126;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar /= result126;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_2_YuzdeVeyaMiktar *= result126;
									break;
								}
							}
							if (getInt204 != 0)
							{
								double result127 = 0.0;
								double.TryParse(sqlDataReader[getInt204 - 1].ToString().Trim().Replace(".", ","), out result127);
								genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result127;
							}
							num = getInt272;
							num2 = getInt273;
							if (num != 0)
							{
								string text197 = "";
								if (num2 != 0)
								{
									text197 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result128 = 0.0;
								double.TryParse(text197.Replace(".", ","), out result128);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result128;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result128;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result128;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result128;
									break;
								}
							}
							num = getInt274;
							num2 = getInt275;
							if (num != 0)
							{
								string text198 = "";
								if (num2 != 0)
								{
									text198 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result129 = 0.0;
								double.TryParse(text198.Replace(".", ","), out result129);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar += result129;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar -= result129;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar /= result129;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_3_YuzdeVeyaMiktar *= result129;
									break;
								}
							}
							if (getInt206 != 0)
							{
								double result130 = 0.0;
								double.TryParse(sqlDataReader[getInt206 - 1].ToString().Trim().Replace(".", ","), out result130);
								genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result130;
							}
							num = getInt276;
							num2 = getInt277;
							if (num != 0)
							{
								string text199 = "";
								if (num2 != 0)
								{
									text199 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result131 = 0.0;
								double.TryParse(text199.Replace(".", ","), out result131);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result131;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result131;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result131;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result131;
									break;
								}
							}
							num = getInt278;
							num2 = getInt279;
							if (num != 0)
							{
								string text200 = "";
								if (num2 != 0)
								{
									text200 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result132 = 0.0;
								double.TryParse(text200.Replace(".", ","), out result132);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar += result132;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar -= result132;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar /= result132;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_4_YuzdeVeyaMiktar *= result132;
									break;
								}
							}
							if (getInt208 != 0)
							{
								double result133 = 0.0;
								double.TryParse(sqlDataReader[getInt208 - 1].ToString().Trim().Replace(".", ","), out result133);
								genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result133;
							}
							num = getInt280;
							num2 = getInt281;
							if (num != 0)
							{
								string text201 = "";
								if (num2 != 0)
								{
									text201 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result134 = 0.0;
								double.TryParse(text201.Replace(".", ","), out result134);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result134;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result134;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result134;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result134;
									break;
								}
							}
							num = getInt282;
							num2 = getInt283;
							if (num != 0)
							{
								string text202 = "";
								if (num2 != 0)
								{
									text202 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result135 = 0.0;
								double.TryParse(text202.Replace(".", ","), out result135);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar += result135;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar -= result135;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar /= result135;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_5_YuzdeVeyaMiktar *= result135;
									break;
								}
							}
							if (getInt210 != 0)
							{
								double result136 = 0.0;
								double.TryParse(sqlDataReader[getInt210 - 1].ToString().Trim().Replace(".", ","), out result136);
								genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result136;
							}
							num = getInt284;
							num2 = getInt285;
							if (num != 0)
							{
								string text203 = "";
								if (num2 != 0)
								{
									text203 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result137 = 0.0;
								double.TryParse(text203.Replace(".", ","), out result137);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result137;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result137;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result137;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result137;
									break;
								}
							}
							num = getInt286;
							num2 = getInt287;
							if (num != 0)
							{
								string text204 = "";
								if (num2 != 0)
								{
									text204 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result138 = 0.0;
								double.TryParse(text204.Replace(".", ","), out result138);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar += result138;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar -= result138;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar /= result138;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.Iskonto_6_YuzdeVeyaMiktar *= result138;
									break;
								}
							}
							genelEvrakSatir2.birimfiyat.OtvUygulamaSekli = (enum_YuzdeTutar)getInt211;
							if (getInt212 != 0)
							{
								double result139 = 0.0;
								double.TryParse(sqlDataReader[getInt212 - 1].ToString().Trim().Replace(".", ","), out result139);
								genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar = result139;
							}
							num = getInt260;
							num2 = getInt261;
							if (num != 0)
							{
								string text205 = "";
								if (num2 != 0)
								{
									text205 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result140 = 0.0;
								double.TryParse(text205.Replace(".", ","), out result140);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result140;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result140;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result140;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result140;
									break;
								}
							}
							num = getInt262;
							num2 = getInt263;
							if (num != 0)
							{
								string text206 = "";
								if (num2 != 0)
								{
									text206 = sqlDataReader[num2 - 1].ToString().Trim();
								}
								double result141 = 0.0;
								double.TryParse(text206.Replace(".", ","), out result141);
								switch (num)
								{
								case 1:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar += result141;
									break;
								case 2:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar -= result141;
									break;
								case 3:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar /= result141;
									break;
								case 4:
									genelEvrakSatir2.birimfiyat.OtvYuzdeVeyaTutar *= result141;
									break;
								}
							}
						}
						genelEvrakSatir2.birimfiyat.OtvVergiPntr = getInt304;
						if (genelEvrakSatir2.birimfiyat.FiyatBrut != 0.0)
						{
							if (genelEvrakSatir2.birimfiyat.FiyatBrut < 0.0 && getBoolean66)
							{
								genelEvrakSatir2.evraktipi = (enum_GenelEvrakTipleri)getInt292;
							}
							if (genelEvrakSatir2.birimfiyat.FiyatBrut < 0.0)
							{
								genelEvrakSatir2.birimfiyat.FiyatBrut *= -1.0;
							}
							bindingList.Add(genelEvrakSatir2);
						}
					}
				}
				num3++;
			}
			sqlConnection.Close();
		}
		catch (Exception ex)
		{
			if (MessageBox.Show("Bilgiler çekilirken hata oluştu. Lütfen ayarları kontrol ediniz. Hata oluşan satır : " + num3 + ". Devam etmek istiyor musunuz? Hata açıklaması : " + ex.ToString(), "HATA", MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				return;
			}
		}
		for (int j = 0; j < bindingList.Count; j++)
		{
			bindingList[j] = KriterleriUygula(bindingList[j]);
		}
		foreach (GenelEvrakSatir item11 in bindingList)
		{
			if (item11 != null)
			{
				_satirlar._satirlar.Add(item11);
			}
		}
		if (flag)
		{
			OtomatikEvrakSiraNoVer(satirBirlestir);
		}
	}

	private GenelEvrakSatir KriterleriUygula(GenelEvrakSatir satir)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		foreach (GenelEvrakKriter item in _uygulanacak_kriterler)
		{
			if (satir == null)
			{
				continue;
			}
			bool flag = true;
			List<bool> list = new List<bool>();
			foreach (AlanOperatorDeger item2 in item.arama_alan1)
			{
				list.Add(KriterVeriKontrolu(satir, item2.aranacak_alan, item2.kullanilan_operator, item2.aranacak_deger));
			}
			List<bool> list2 = new List<bool>();
			foreach (AlanOperatorDeger item3 in item.arama_alan2)
			{
				list2.Add(KriterVeriKontrolu(satir, item3.aranacak_alan, item3.kullanilan_operator, item3.aranacak_deger));
			}
			bool flag2 = true;
			switch (item.arama_alan1_baglac)
			{
			case enum_Baglac.Ve:
				flag2 = true;
				foreach (bool item4 in list)
				{
					if (!item4)
					{
						flag2 = false;
						break;
					}
				}
				break;
			case enum_Baglac.Veya:
				flag2 = false;
				foreach (bool item5 in list)
				{
					if (item5)
					{
						flag2 = true;
						break;
					}
				}
				break;
			default:
				flag2 = false;
				break;
			}
			if (list.Count == 0)
			{
				flag2 = true;
			}
			bool flag3 = true;
			switch (item.arama_alan2_baglac)
			{
			case enum_Baglac.Ve:
				flag3 = true;
				foreach (bool item6 in list2)
				{
					if (!item6)
					{
						flag3 = false;
						break;
					}
				}
				break;
			case enum_Baglac.Veya:
				flag3 = false;
				foreach (bool item7 in list2)
				{
					if (item7)
					{
						flag3 = true;
						break;
					}
				}
				break;
			default:
				flag3 = false;
				break;
			}
			if (list2.Count == 0)
			{
				flag3 = true;
			}
			switch (item.arama_alan1_alan2_baglac)
			{
			case enum_Baglac.Ve:
				flag = ((flag2 && flag3) ? true : false);
				break;
			case enum_Baglac.Veya:
				flag = ((flag2 || flag3) ? true : false);
				break;
			}
			if (!flag)
			{
				continue;
			}
			foreach (AlanIslemTipiDeger item8 in item.degistirilecek_alanlar)
			{
				if (satir == null)
				{
					continue;
				}
				bool flag4 = true;
				if (item8.ek_kontrol_yap)
				{
					flag4 = KriterVeriKontrolu(satir, item8.aranacak_alan, item8.kullanilan_operator, item8.aranacak_deger);
				}
				if (!flag4)
				{
					continue;
				}
				switch (item8.yapilacak_islem)
				{
				case enum_YapilacakIslem.VeriDegistir:
				{
					string text2 = "";
					text2 = KriterAdayYeniDegerBul(satir, item8.deger_tipi, item8.ozel_deger);
					try
					{
						switch (item8.degistirilecek_alan)
						{
						case enum_KriterDegistirmeAlanlari.KayitID:
						{
							string eskideger49 = satir.KayitID.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger49), out var result43);
							satir.KayitID = result43;
							break;
						}
						case enum_KriterDegistirmeAlanlari.evraktipi:
						{
							string eskideger48 = satir.evraktipi.ToString();
							string value6 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger48);
							try
							{
								satir.evraktipi = GenelUtilityWin.ParseEnum<enum_GenelEvrakTipleri>(value6);
							}
							catch
							{
							}
							break;
						}
						case enum_KriterDegistirmeAlanlari.normaliade:
						{
							string eskideger47 = satir.normaliade.ToString();
							string value5 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger47);
							try
							{
								satir.normaliade = GenelUtilityWin.ParseEnum<enum_cha_normal_Iade>(value5);
							}
							catch
							{
							}
							break;
						}
						case enum_KriterDegistirmeAlanlari.kapamasekli:
						{
							string eskideger46 = satir.kapamasekli.ToString();
							string value4 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger46);
							try
							{
								satir.kapamasekli = GenelUtilityWin.ParseEnum<enum_KapamaSekli>(value4);
							}
							catch
							{
							}
							break;
						}
						case enum_KriterDegistirmeAlanlari.ticaretturu:
						{
							string eskideger45 = satir.ticaretturu.ToString();
							string value3 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger45);
							try
							{
								satir.ticaretturu = GenelUtilityWin.ParseEnum<enum_cha_ticaret_turu>(value3);
							}
							catch
							{
							}
							break;
						}
						case enum_KriterDegistirmeAlanlari.evraktarih:
						{
							string eskideger44 = satir.evraktarih.ToString();
							DateTime result42 = satir.evraktarih;
							DateTime.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger44), out result42);
							satir.evraktarih = result42;
							break;
						}
						case enum_KriterDegistirmeAlanlari.evraknoseri:
						{
							string evraknoseri = satir.evraknoseri;
							satir.evraknoseri = KriterGercekYeniDegerBul(item8.islem_tipi, text2, evraknoseri);
							break;
						}
						case enum_KriterDegistirmeAlanlari.evraknosira:
						{
							string eskideger43 = satir.evraknosira.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger43), out var result41);
							satir.evraknosira = result41;
							break;
						}
						case enum_KriterDegistirmeAlanlari.belgeno:
						{
							string belgeno = satir.belgeno;
							satir.belgeno = KriterGercekYeniDegerBul(item8.islem_tipi, text2, belgeno);
							break;
						}
						case enum_KriterDegistirmeAlanlari.belgetarih:
						{
							string eskideger42 = satir.belgetarih.ToString();
							DateTime result40 = satir.belgetarih;
							DateTime.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger42), out result40);
							satir.belgetarih = result40;
							break;
						}
						case enum_KriterDegistirmeAlanlari.odemeplani:
						{
							string eskideger41 = satir.odemeplani.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger41), out var result39);
							satir.odemeplani = result39;
							break;
						}
						case enum_KriterDegistirmeAlanlari.fiyatlistesi:
						{
							string eskideger40 = satir.fiyatlistesi.sfl_sirano.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger40), out var result38);
							satir.fiyatlistesi = FiyatListesiData.GetFiyatListesi(Db.Connection, result38);
							break;
						}
						case enum_KriterDegistirmeAlanlari.dovizcinsi:
						{
							string eskideger39 = satir.dovizcinsi.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger39), out var result37);
							satir.dovizcinsi = result37;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kur:
						{
							string eskideger38 = satir.kur.dov_fiyat.ToString();
							double result36 = 1.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger38), out result36);
							satir.kur = new Kur();
							satir.kur.dov_fiyat = result36;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kaynakdepo:
						{
							string eskideger37 = satir.kaynakdepo.dep_no.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger37), out var result35);
							satir.kaynakdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, result35);
							break;
						}
						case enum_KriterDegistirmeAlanlari.hedefdepo:
						{
							string eskideger36 = satir.hedefdepo.dep_no.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger36), out var result34);
							satir.hedefdepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, result34);
							break;
						}
						case enum_KriterDegistirmeAlanlari.proje:
						{
							string pro_kodu = satir.proje.pro_kodu;
							string pro_kodu2 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, pro_kodu);
							satir.proje = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, pro_kodu2);
							break;
						}
						case enum_KriterDegistirmeAlanlari.sorumlulukmerkezi:
						{
							string som_kod = satir.sorumlulukmerkezi.som_kod;
							string som_kod2 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, som_kod);
							satir.sorumlulukmerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, som_kod2);
							break;
						}
						case enum_KriterDegistirmeAlanlari.temsilcikodu:
						{
							string temsilcikodu = satir.temsilcikodu;
							satir.temsilcikodu = KriterGercekYeniDegerBul(item8.islem_tipi, text2, temsilcikodu);
							break;
						}
						case enum_KriterDegistirmeAlanlari.firma:
						{
							string eskideger35 = satir.firma.fir_sirano.ToString();
							int result33 = 0;
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger35), out result33);
							satir.firma = FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, result33);
							break;
						}
						case enum_KriterDegistirmeAlanlari.sube:
						{
							string eskideger34 = satir.sube.Sube_no.ToString();
							int result32 = 0;
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger34), out result32);
							satir.sube = SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, result32);
							break;
						}
						case enum_KriterDegistirmeAlanlari.sevkteslimtarihi:
						{
							string eskideger33 = satir.sevkteslimtarihi.ToString();
							DateTime result31 = satir.sevkteslimtarihi;
							DateTime.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger33), out result31);
							satir.sevkteslimtarihi = result31;
							break;
						}
						case enum_KriterDegistirmeAlanlari.sevkadresno:
						{
							string eskideger32 = satir.sevkadresno.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger32), out var result30);
							satir.sevkadresno = result30;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kapamahesapkodu:
						{
							string kapamahesapkodu = satir.kapamahesapkodu;
							satir.kapamahesapkodu = KriterGercekYeniDegerBul(item8.islem_tipi, text2, kapamahesapkodu);
							break;
						}
						case enum_KriterDegistirmeAlanlari.faturaaciklama:
						{
							string faturaaciklama = satir.faturaaciklama;
							satir.faturaaciklama = KriterGercekYeniDegerBul(item8.islem_tipi, text2, faturaaciklama);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama1:
						{
							string aciklama10 = satir.aciklama1;
							satir.aciklama1 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama10);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama2:
						{
							string aciklama9 = satir.aciklama2;
							satir.aciklama2 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama9);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama3:
						{
							string aciklama8 = satir.aciklama3;
							satir.aciklama3 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama8);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama4:
						{
							string aciklama7 = satir.aciklama4;
							satir.aciklama4 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama7);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama5:
						{
							string aciklama6 = satir.aciklama5;
							satir.aciklama5 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama6);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama6:
						{
							string aciklama5 = satir.aciklama6;
							satir.aciklama6 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama5);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama7:
						{
							string aciklama4 = satir.aciklama7;
							satir.aciklama7 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama4);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama8:
						{
							string aciklama3 = satir.aciklama8;
							satir.aciklama8 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama3);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama9:
						{
							string aciklama2 = satir.aciklama9;
							satir.aciklama9 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama2);
							break;
						}
						case enum_KriterDegistirmeAlanlari.aciklama10:
						{
							string aciklama = satir.aciklama10;
							satir.aciklama10 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, aciklama);
							break;
						}
						case enum_KriterDegistirmeAlanlari.satircinsi:
						{
							string eskideger31 = satir.satircinsi.ToString();
							string value2 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger31);
							try
							{
								satir.satircinsi = GenelUtilityWin.ParseEnum<enum_SatirCinsi>(value2);
							}
							catch
							{
							}
							break;
						}
						case enum_KriterDegistirmeAlanlari.miktar:
						{
							string eskideger30 = satir.miktar.ToString();
							double result29 = 1.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger30), out result29);
							satir.miktar = result29;
							break;
						}
						case enum_KriterDegistirmeAlanlari.miktar2:
						{
							string eskideger29 = satir.miktar2.ToString();
							double result28 = 1.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger29), out result28);
							satir.miktar2 = result28;
							break;
						}
						case enum_KriterDegistirmeAlanlari.vergi_pntr:
						{
							string eskideger28 = satir.vergi_pntr.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger28), out var result27);
							satir.vergi_pntr = result27;
							break;
						}
						case enum_KriterDegistirmeAlanlari.fiyat_fark_mi:
						{
							string eskideger27 = satir.fiyat_fark_mi.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger27), out var result26);
							satir.fiyat_fark_mi = result26;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_string1:
						{
							string kriter_string5 = satir.kriter_string1;
							satir.kriter_string1 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, kriter_string5);
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_string2:
						{
							string kriter_string4 = satir.kriter_string2;
							satir.kriter_string2 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, kriter_string4);
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_string3:
						{
							string kriter_string3 = satir.kriter_string3;
							satir.kriter_string3 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, kriter_string3);
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_string4:
						{
							string kriter_string2 = satir.kriter_string4;
							satir.kriter_string4 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, kriter_string2);
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_string5:
						{
							string kriter_string = satir.kriter_string5;
							satir.kriter_string5 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, kriter_string);
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double1:
						{
							string eskideger26 = satir.kriter_double1.ToString();
							double result25 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger26), out result25);
							satir.kriter_double1 = result25;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double2:
						{
							string eskideger25 = satir.kriter_double2.ToString();
							double result24 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger25), out result24);
							satir.kriter_double2 = result24;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double3:
						{
							string eskideger24 = satir.kriter_double3.ToString();
							double result23 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger24), out result23);
							satir.kriter_double3 = result23;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double4:
						{
							string eskideger23 = satir.kriter_double4.ToString();
							double result22 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger23), out result22);
							satir.kriter_double4 = result22;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double5:
						{
							string eskideger22 = satir.kriter_double5.ToString();
							double result21 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger22), out result21);
							satir.kriter_double5 = result21;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool1:
						{
							string eskideger21 = satir.kriter_bool1.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger21), out var result20);
							satir.kriter_bool1 = result20;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool2:
						{
							string eskideger20 = satir.kriter_bool2.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger20), out var result19);
							satir.kriter_bool2 = result19;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool3:
						{
							string eskideger19 = satir.kriter_bool3.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger19), out var result18);
							satir.kriter_bool3 = result18;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool4:
						{
							string eskideger18 = satir.kriter_bool4.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger18), out var result17);
							satir.kriter_bool4 = result17;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool5:
						{
							string eskideger17 = satir.kriter_bool5.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger17), out var result16);
							satir.kriter_bool5 = result16;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_brut_fiyat:
						{
							string eskideger16 = satir.birimfiyat.FiyatBrut.ToString();
							double result15 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger16), out result15);
							satir.birimfiyat.FiyatBrut = result15;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto1_uygulama_sekli:
						{
							string eskideger15 = satir.birimfiyat.Iskonto_1_UygulamaSekli.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger15), out var result14);
							satir.birimfiyat.Iskonto_1_UygulamaSekli = result14;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto1_yuzdeveyatutar:
						{
							string eskideger14 = satir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar.ToString();
							double result13 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger14), out result13);
							satir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar = result13;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto2_uygulama_sekli:
						{
							string eskideger13 = satir.birimfiyat.Iskonto_2_UygulamaSekli.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger13), out var result12);
							satir.birimfiyat.Iskonto_2_UygulamaSekli = result12;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto2_yuzdeveyatutar:
						{
							string eskideger12 = satir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar.ToString();
							double result11 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger12), out result11);
							satir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar = result11;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto3_uygulama_sekli:
						{
							string eskideger11 = satir.birimfiyat.Iskonto_3_UygulamaSekli.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger11), out var result10);
							satir.birimfiyat.Iskonto_3_UygulamaSekli = result10;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto3_yuzdeveyatutar:
						{
							string eskideger10 = satir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar.ToString();
							double result9 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger10), out result9);
							satir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar = result9;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto4_uygulama_sekli:
						{
							string eskideger9 = satir.birimfiyat.Iskonto_4_UygulamaSekli.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger9), out var result8);
							satir.birimfiyat.Iskonto_4_UygulamaSekli = result8;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto4_yuzdeveyatutar:
						{
							string eskideger8 = satir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar.ToString();
							double result7 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger8), out result7);
							satir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar = result7;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto5_uygulama_sekli:
						{
							string eskideger7 = satir.birimfiyat.Iskonto_5_UygulamaSekli.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger7), out var result6);
							satir.birimfiyat.Iskonto_5_UygulamaSekli = result6;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto5_yuzdeveyatutar:
						{
							string eskideger6 = satir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar.ToString();
							double result5 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger6), out result5);
							satir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar = result5;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto6_uygulama_sekli:
						{
							string eskideger5 = satir.birimfiyat.Iskonto_6_UygulamaSekli.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger5), out var result4);
							satir.birimfiyat.Iskonto_6_UygulamaSekli = result4;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto6_yuzdeveyatutar:
						{
							string eskideger4 = satir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar.ToString();
							double result3 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger4), out result3);
							satir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar = result3;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_otv_uygulama_sekli:
						{
							string eskideger3 = satir.birimfiyat.OtvUygulamaSekli.ToString();
							string value = KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger3);
							try
							{
								satir.birimfiyat.OtvUygulamaSekli = GenelUtilityWin.ParseEnum<enum_YuzdeTutar>(value);
							}
							catch
							{
							}
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_otv_yuzdeveyatutar:
						{
							string eskideger2 = satir.birimfiyat.OtvYuzdeVeyaTutar.ToString();
							double result2 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger2), out result2);
							satir.birimfiyat.OtvYuzdeVeyaTutar = result2;
							break;
						}
						case enum_KriterDegistirmeAlanlari.birimfiyat_otv_vergipntr:
						{
							string eskideger = satir.birimfiyat.OtvVergiPntr.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger), out var result);
							satir.birimfiyat.OtvVergiPntr = result;
							break;
						}
						case enum_KriterDegistirmeAlanlari.cari_kod:
						{
							string cari_kod = satir.cari.cari_kod;
							string cari_kod2 = KriterGercekYeniDegerBul(item8.islem_tipi, text2, cari_kod);
							satir.cari = CariData.GetCariByCariKod(Db.Connection, cari_kod2, AdreslerTemsilciyeGore: false, "");
							break;
						}
						case enum_KriterDegistirmeAlanlari.stokhizmetkodu:
						{
							string stokhizmetkodu = satir.stokhizmetkodu;
							satir.stokhizmetkodu = KriterGercekYeniDegerBul(item8.islem_tipi, text2, stokhizmetkodu);
							break;
						}
						}
					}
					catch
					{
					}
					break;
				}
				case enum_YapilacakIslem.OzelIslem:
				{
					string text = KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre1_deger_tipi, item8.ozel_islem_parametre1_ozel_deger);
					string newValue = KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre2_deger_tipi, item8.ozel_islem_parametre2_ozel_deger);
					string newValue2 = KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre3_deger_tipi, item8.ozel_islem_parametre3_ozel_deger);
					string newValue3 = KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre4_deger_tipi, item8.ozel_islem_parametre4_ozel_deger);
					string newValue4 = KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre5_deger_tipi, item8.ozel_islem_parametre5_ozel_deger);
					string newValue5 = KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre6_deger_tipi, item8.ozel_islem_parametre6_ozel_deger);
					KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre7_deger_tipi, item8.ozel_islem_parametre7_ozel_deger);
					KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre8_deger_tipi, item8.ozel_islem_parametre8_ozel_deger);
					KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre9_deger_tipi, item8.ozel_islem_parametre9_ozel_deger);
					KriterAdayYeniDegerBul(satir, item8.ozel_islem_parametre10_deger_tipi, item8.ozel_islem_parametre10_ozel_deger);
					switch (item8.ozel_islem_tipi)
					{
					case enum_OzelIslem.Miktar1Miktar2YerDegistir:
					{
						double miktar = satir.miktar;
						double miktar2 = satir.miktar2;
						satir.miktar = miktar2;
						satir.miktar2 = miktar;
						break;
					}
					case enum_OzelIslem.UyariMesajiVer:
						MessageBox.Show(text.Replace("[mesaj1]", newValue).Replace("[mesaj2]", newValue2).Replace("[mesaj3]", newValue3)
							.Replace("[mesaj4]", newValue4)
							.Replace("[mesaj5]", newValue5));
						break;
					case enum_OzelIslem.SatiriAktarilmayacakOlarakIsaretle:
						satir.Aktar = false;
						break;
					case enum_OzelIslem.SatiriSil:
						satir = null;
						break;
					}
					break;
				}
				}
			}
		}
		return satir;
	}

	private string KriterAdayYeniDegerBul(GenelEvrakSatir satir, enum_DegerTipi deger_tipi, string ozel_deger)
	{
		string result = "";
		switch (deger_tipi)
		{
		case enum_DegerTipi.OzelDeger:
			result = ozel_deger;
			break;
		case enum_DegerTipi.evraktipi:
			result = satir.evraktipi.ToString();
			break;
		case enum_DegerTipi.normaliade:
			result = satir.normaliade.ToString();
			break;
		case enum_DegerTipi.kapamasekli:
			result = satir.kapamasekli.ToString();
			break;
		case enum_DegerTipi.ticaretturu:
			result = satir.ticaretturu.ToString();
			break;
		case enum_DegerTipi.evraktarih:
			result = satir.evraktarih.ToString("dd.MM.yyyy");
			break;
		case enum_DegerTipi.evraknoseri:
			result = satir.evraknoseri;
			break;
		case enum_DegerTipi.evraknosira:
			result = satir.evraknosira.ToString();
			break;
		case enum_DegerTipi.belgeno:
			result = satir.belgeno;
			break;
		case enum_DegerTipi.belgetarih:
			result = satir.belgetarih.ToString("dd.MM.yyyy");
			break;
		case enum_DegerTipi.odemeplani:
			result = satir.odemeplani.ToString();
			break;
		case enum_DegerTipi.fiyatlistesi_aciklama:
			result = satir.fiyatlistesi.sfl_aciklama;
			break;
		case enum_DegerTipi.fiyatlistesi_kdvdahil:
			result = satir.fiyatlistesi.sfl_kdvdahil.ToString();
			break;
		case enum_DegerTipi.fiyatlistesi_sirano:
			result = satir.fiyatlistesi.sfl_sirano.ToString();
			break;
		case enum_DegerTipi.dovizcinsi_doviz_adi:
			result = _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(satir.dovizcinsi).Kur_adi;
			break;
		case enum_DegerTipi.dovizcinsi_doviz_kod:
			result = satir.dovizcinsi.ToString();
			break;
		case enum_DegerTipi.dovizcinsi_doviz_sembol:
			result = _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(satir.dovizcinsi).Kur_sembol;
			break;
		case enum_DegerTipi.kur:
			result = satir.kur.ToString();
			break;
		case enum_DegerTipi.kaynakdepo_depo_adi:
			result = satir.kaynakdepo.dep_adi;
			break;
		case enum_DegerTipi.kaynakdepo_depo_no:
			result = satir.kaynakdepo.dep_no.ToString();
			break;
		case enum_DegerTipi.hedefdepo_depo_adi:
			result = satir.hedefdepo.dep_adi;
			break;
		case enum_DegerTipi.hedefdepo_depo_no:
			result = satir.hedefdepo.dep_no.ToString();
			break;
		case enum_DegerTipi.proje_adi:
			result = satir.proje.pro_adi;
			break;
		case enum_DegerTipi.proje_kodu:
			result = satir.proje.pro_kodu;
			break;
		case enum_DegerTipi.sorumlulukmerkezi_adi:
			result = satir.sorumlulukmerkezi.som_isim;
			break;
		case enum_DegerTipi.sorumlulukmerkezi_kodu:
			result = satir.sorumlulukmerkezi.som_kod;
			break;
		case enum_DegerTipi.temsilcikodu:
			result = satir.temsilcikodu;
			break;
		case enum_DegerTipi.firma_sirano:
			result = satir.firma.fir_sirano.ToString();
			break;
		case enum_DegerTipi.firma_unvan:
			result = satir.firma.fir_unvan;
			break;
		case enum_DegerTipi.sube_adi:
			result = satir.sube.Sube_adi;
			break;
		case enum_DegerTipi.sube_no:
			result = satir.sube.Sube_no.ToString();
			break;
		case enum_DegerTipi.sevkteslimtarihi:
			result = satir.sevkteslimtarihi.ToString("dd.MM.yyyy");
			break;
		case enum_DegerTipi.kapamahesapkodu:
			result = satir.kapamahesapkodu;
			break;
		case enum_DegerTipi.faturaaciklama:
			result = satir.faturaaciklama;
			break;
		case enum_DegerTipi.aciklama1:
			result = satir.aciklama1;
			break;
		case enum_DegerTipi.aciklama2:
			result = satir.aciklama2;
			break;
		case enum_DegerTipi.aciklama3:
			result = satir.aciklama3;
			break;
		case enum_DegerTipi.aciklama4:
			result = satir.aciklama4;
			break;
		case enum_DegerTipi.aciklama5:
			result = satir.aciklama5;
			break;
		case enum_DegerTipi.aciklama6:
			result = satir.aciklama6;
			break;
		case enum_DegerTipi.aciklama7:
			result = satir.aciklama7;
			break;
		case enum_DegerTipi.aciklama8:
			result = satir.aciklama8;
			break;
		case enum_DegerTipi.aciklama9:
			result = satir.aciklama9;
			break;
		case enum_DegerTipi.aciklama10:
			result = satir.aciklama10;
			break;
		case enum_DegerTipi.satircinsi:
			result = satir.satircinsi.ToString();
			break;
		case enum_DegerTipi.miktar:
			result = satir.miktar.ToString();
			break;
		case enum_DegerTipi.miktar2:
			result = satir.miktar2.ToString();
			break;
		case enum_DegerTipi.vergi_pntr:
			result = satir.vergi_pntr.ToString();
			break;
		case enum_DegerTipi.fiyat_fark_mi:
			result = satir.fiyat_fark_mi.ToString();
			break;
		case enum_DegerTipi.kriter_string1:
			result = satir.kriter_string1;
			break;
		case enum_DegerTipi.kriter_string2:
			result = satir.kriter_string2;
			break;
		case enum_DegerTipi.kriter_string3:
			result = satir.kriter_string3;
			break;
		case enum_DegerTipi.kriter_string4:
			result = satir.kriter_string4;
			break;
		case enum_DegerTipi.kriter_string5:
			result = satir.kriter_string5;
			break;
		case enum_DegerTipi.kriter_double1:
			result = satir.kriter_double1.ToString();
			break;
		case enum_DegerTipi.kriter_double2:
			result = satir.kriter_double2.ToString();
			break;
		case enum_DegerTipi.kriter_double3:
			result = satir.kriter_double3.ToString();
			break;
		case enum_DegerTipi.kriter_double4:
			result = satir.kriter_double4.ToString();
			break;
		case enum_DegerTipi.kriter_double5:
			result = satir.kriter_double5.ToString();
			break;
		case enum_DegerTipi.kriter_bool1:
			result = satir.kriter_bool1.ToString();
			break;
		case enum_DegerTipi.kriter_bool2:
			result = satir.kriter_bool2.ToString();
			break;
		case enum_DegerTipi.kriter_bool3:
			result = satir.kriter_bool3.ToString();
			break;
		case enum_DegerTipi.kriter_bool4:
			result = satir.kriter_bool4.ToString();
			break;
		case enum_DegerTipi.kriter_bool5:
			result = satir.kriter_bool5.ToString();
			break;
		case enum_DegerTipi.birimfiyat_brut_fiyat:
			result = satir.birimfiyat.FiyatBrut.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto1_uygulama_sekli:
			result = satir.birimfiyat.Iskonto_1_UygulamaSekli.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto1_yuzdeveyatutar:
			result = satir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto2_uygulama_sekli:
			result = satir.birimfiyat.Iskonto_2_UygulamaSekli.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto2_yuzdeveyatutar:
			result = satir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto3_uygulama_sekli:
			result = satir.birimfiyat.Iskonto_3_UygulamaSekli.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto3_yuzdeveyatutar:
			result = satir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto4_uygulama_sekli:
			result = satir.birimfiyat.Iskonto_4_UygulamaSekli.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto4_yuzdeveyatutar:
			result = satir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto5_uygulama_sekli:
			result = satir.birimfiyat.Iskonto_5_UygulamaSekli.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto5_yuzdeveyatutar:
			result = satir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto6_uygulama_sekli:
			result = satir.birimfiyat.Iskonto_6_UygulamaSekli.ToString();
			break;
		case enum_DegerTipi.birimfiyat_iskonto6_yuzdeveyatutar:
			result = satir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar.ToString();
			break;
		case enum_DegerTipi.birimfiyat_otv_uygulama_sekli:
			result = satir.birimfiyat.OtvUygulamaSekli.ToString();
			break;
		case enum_DegerTipi.birimfiyat_otv_yuzdeveyatutar:
			result = satir.birimfiyat.OtvYuzdeVeyaTutar.ToString();
			break;
		case enum_DegerTipi.birimfiyat_otv_vergipntr:
			result = satir.birimfiyat.OtvVergiPntr.ToString();
			break;
		case enum_DegerTipi.ara_toplam:
			result = satir.GetEvrakAraToplam().ToString();
			break;
		case enum_DegerTipi.iskonto_toplam:
			result = satir.GetEvrakIskontoToplam().ToString();
			break;
		case enum_DegerTipi.kdv_toplam:
			result = satir.GetEvrakKdvToplam(_mikrouygulamabilgileri.vergitanimlari).ToString();
			break;
		case enum_DegerTipi.masraf_toplam:
			result = satir.GetEvrakMasrafToplam().ToString();
			break;
		case enum_DegerTipi.otv_toplam:
			result = satir.GetEvrakOtvToplam().ToString();
			break;
		case enum_DegerTipi.yekun:
			result = satir.GetEvrakYekun(_mikrouygulamabilgileri.vergitanimlari).ToString();
			break;
		case enum_DegerTipi.ara_toplam_yazi_ile:
			result = GenelUtility.YaziIleTutar(satir.GetEvrakAraToplam());
			break;
		case enum_DegerTipi.iskonto_toplam_yazi_ile:
			result = GenelUtility.YaziIleTutar(satir.GetEvrakIskontoToplam());
			break;
		case enum_DegerTipi.kdv_toplam_yazi_ile:
			result = GenelUtility.YaziIleTutar(satir.GetEvrakKdvToplam(_mikrouygulamabilgileri.vergitanimlari));
			break;
		case enum_DegerTipi.masraf_toplam_yazi_ile:
			result = GenelUtility.YaziIleTutar(satir.GetEvrakMasrafToplam());
			break;
		case enum_DegerTipi.otv_toplam_yazi_ile:
			result = GenelUtility.YaziIleTutar(satir.GetEvrakOtvToplam());
			break;
		case enum_DegerTipi.yekun_yazi_ile:
			result = GenelUtility.YaziIleTutar(satir.GetEvrakYekun(_mikrouygulamabilgileri.vergitanimlari));
			break;
		case enum_DegerTipi.KayitID:
			result = satir.KayitID.ToString();
			break;
		case enum_DegerTipi.cari_kod:
			result = satir.cari.cari_kod;
			break;
		case enum_DegerTipi.cari_unvan1:
			result = satir.cari.cari_unvan1;
			break;
		case enum_DegerTipi.cari_unvan2:
			result = satir.cari.cari_unvan2;
			break;
		case enum_DegerTipi.cari_muh_kod:
			result = satir.cari.cari_muh_kod;
			break;
		case enum_DegerTipi.cari_muh_kod1:
			result = satir.cari.cari_muh_kod1;
			break;
		case enum_DegerTipi.cari_muh_kod2:
			result = satir.cari.cari_muh_kod2;
			break;
		case enum_DegerTipi.cari_vdaire_adi:
			result = satir.cari.cari_vdaire_adi;
			break;
		case enum_DegerTipi.cari_vdaire_no:
			result = satir.cari.cari_vdaire_no;
			break;
		case enum_DegerTipi.cari_Ana_cari_kodu:
			result = satir.cari.cari_Ana_cari_kodu;
			break;
		case enum_DegerTipi.cari_bolge_kodu:
			result = satir.cari.cari_bolge_kodu;
			break;
		case enum_DegerTipi.cari_grup_kodu:
			result = satir.cari.cari_grup_kodu;
			break;
		case enum_DegerTipi.cari_temsilci_kodu:
			result = satir.cari.cari_temsilci_kodu;
			break;
		case enum_DegerTipi.cari_sektor_kodu:
			result = satir.cari.cari_sektor_kodu;
			break;
		case enum_DegerTipi.cari_satis_isk_kod:
			result = satir.cari.cari_satis_isk_kod;
			break;
		case enum_DegerTipi.cari_special1:
			result = satir.cari.cari_special1;
			break;
		case enum_DegerTipi.cari_special2:
			result = satir.cari.cari_special2;
			break;
		case enum_DegerTipi.cari_special3:
			result = satir.cari.cari_special3;
			break;
		case enum_DegerTipi.cari_sicil_no:
			result = satir.cari.cari_sicil_no;
			break;
		case enum_DegerTipi.cari_VergiKimlikNo:
			result = satir.cari.cari_VergiKimlikNo;
			break;
		case enum_DegerTipi.cari_vade_fark_yuz:
			result = satir.cari.cari_vade_fark_yuz.ToString();
			break;
		case enum_DegerTipi.cari_vade_fark_yuz1:
			result = satir.cari.cari_vade_fark_yuz1.ToString();
			break;
		case enum_DegerTipi.cari_vade_fark_yuz2:
			result = satir.cari.cari_vade_fark_yuz2.ToString();
			break;
		case enum_DegerTipi.cari_tipi:
			result = satir.cari.cari_tipi.ToString();
			break;
		case enum_DegerTipi.cari_doviz_cinsi:
			result = satir.cari.cari_doviz_cinsi.ToString();
			break;
		case enum_DegerTipi.cari_doviz_cinsi1:
			result = satir.cari.cari_doviz_cinsi1.ToString();
			break;
		case enum_DegerTipi.cari_doviz_cinsi2:
			result = satir.cari.cari_doviz_cinsi2.ToString();
			break;
		case enum_DegerTipi.cari_odeme_gunu:
			result = satir.cari.cari_odeme_gunu.ToString();
			break;
		case enum_DegerTipi.cari_hareket_tipi:
			result = satir.cari.cari_hareket_tipi.ToString();
			break;
		case enum_DegerTipi.cari_odemeplan_no:
			result = satir.cari.cari_odemeplan_no.ToString();
			break;
		case enum_DegerTipi.cari_satis_fk:
			result = satir.cari.cari_satis_fk.ToString();
			break;
		case enum_DegerTipi.cari_KurHesapSekli:
			result = satir.cari.cari_KurHesapSekli.ToString();
			break;
		case enum_DegerTipi.cari_odeme_cinsi:
			result = satir.cari.cari_odeme_cinsi.ToString();
			break;
		case enum_DegerTipi.cari_fatura_adres_no:
			result = satir.cari.cari_fatura_adres_no.ToString();
			break;
		case enum_DegerTipi.cari_sevk_adres_no:
			result = satir.cari.cari_sevk_adres_no.ToString();
			break;
		case enum_DegerTipi.cari_banka_hesapno1:
			result = satir.cari.cari_banka_hesapno1;
			break;
		case enum_DegerTipi.cari_CepTel:
			result = satir.cari.cari_CepTel;
			break;
		case enum_DegerTipi.cari_Email:
			result = satir.cari.cari_Email;
			break;
		case enum_DegerTipi.cari_VarsayilanGirisDepo:
			result = satir.cari.cari_VarsayilanGirisDepo.ToString();
			break;
		case enum_DegerTipi.cari_VarsayilanCikisDepo:
			result = satir.cari.cari_VarsayilanCikisDepo.ToString();
			break;
		case enum_DegerTipi.sto_kod:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_kod;
			break;
		case enum_DegerTipi.sto_isim:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_isim;
			break;
		case enum_DegerTipi.sto_perakende_vergi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_perakende_vergi_yeni.ToString();
			break;
		case enum_DegerTipi.sto_toptan_vergi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_toptan_vergi_yeni.ToString();
			break;
		case enum_DegerTipi.sto_kisa_ismi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_kisa_ismi;
			break;
		case enum_DegerTipi.sto_yabanci_isim:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_yabanci_isim;
			break;
		case enum_DegerTipi.sto_sat_cari_kod:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_sat_cari_kod;
			break;
		case enum_DegerTipi.sto_cins:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_cins.ToString();
			break;
		case enum_DegerTipi.sto_doviz_cinsi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_doviz_cinsi.ToString();
			break;
		case enum_DegerTipi.sto_detay_takip:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_detay_takip.ToString();
			break;
		case enum_DegerTipi.sto_birim1_ad:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim1_ad;
			break;
		case enum_DegerTipi.sto_birim1_katsayi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim1_katsayi.ToString();
			break;
		case enum_DegerTipi.sto_birim2_ad:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim2_ad;
			break;
		case enum_DegerTipi.sto_birim2_katsayi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim2_katsayi.ToString();
			break;
		case enum_DegerTipi.sto_birim3_ad:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim3_ad;
			break;
		case enum_DegerTipi.sto_birim3_katsayi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim3_katsayi.ToString();
			break;
		case enum_DegerTipi.sto_birim4_ad:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim4_ad;
			break;
		case enum_DegerTipi.sto_birim4_katsayi:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_birim4_katsayi.ToString();
			break;
		case enum_DegerTipi.sto_bedenli_takip:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_bedenli_takip.ToString();
			break;
		case enum_DegerTipi.sto_renkDetayli:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_renkDetayli.ToString();
			break;
		case enum_DegerTipi.sto_beden_kodu:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_beden_kodu;
			break;
		case enum_DegerTipi.sto_renk_kodu:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_renk_kodu;
			break;
		case enum_DegerTipi.sto_altgrup_kod:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_altgrup_kod;
			break;
		case enum_DegerTipi.sto_anagrup_kod:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_anagrup_kod;
			break;
		case enum_DegerTipi.sto_sektor_kodu:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_sektor_kodu;
			break;
		case enum_DegerTipi.sto_marka_kodu:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_marka_kodu;
			break;
		case enum_DegerTipi.sto_model_kodu:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_model_kodu;
			break;
		case enum_DegerTipi.sto_uretici_kodu:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_uretici_kodu;
			break;
		case enum_DegerTipi.sto_reyon_kodu:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_reyon_kodu;
			break;
		case enum_DegerTipi.sto_standartmaliyet:
			result = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan).sto_standartmaliyet.ToString();
			break;
		case enum_DegerTipi.hiz_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_kod;
			break;
		case enum_DegerTipi.hiz_tip:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_tip.ToString();
			break;
		case enum_DegerTipi.hiz_isim:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_isim;
			break;
		case enum_DegerTipi.hiz_yabanci_isim:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_yabanci_isim;
			break;
		case enum_DegerTipi.hiz_tipkod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_tipkod;
			break;
		case enum_DegerTipi.hiz_sinifkod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_sinifkod;
			break;
		case enum_DegerTipi.hiz_grupkod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_grupkod;
			break;
		case enum_DegerTipi.hiz_sat_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_sat_muh_kod;
			break;
		case enum_DegerTipi.hiz_sat_iade_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_sat_iade_muh_kod;
			break;
		case enum_DegerTipi.hiz_mal_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_mal_muh_kod;
			break;
		case enum_DegerTipi.hiz_sat_mal_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_sat_mal_muh_kod;
			break;
		case enum_DegerTipi.hiz_mal_yan_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_mal_yan_muh_kod;
			break;
		case enum_DegerTipi.hiz_isk_grup:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_isk_grup;
			break;
		case enum_DegerTipi.hiz_KDV:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_KDV.ToString();
			break;
		case enum_DegerTipi.hiz_muh_sat_isk_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_muh_sat_isk_kod;
			break;
		case enum_DegerTipi.hiz_muh_aIiskmuhkod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_muh_aIiskmuhkod;
			break;
		case enum_DegerTipi.hiz_ilavemasmuhkod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_ilavemasmuhkod;
			break;
		case enum_DegerTipi.hiz_operasyon_suresi:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_operasyon_suresi.ToString();
			break;
		case enum_DegerTipi.hiz_oivuygulama:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_oivuygulama.ToString();
			break;
		case enum_DegerTipi.hiz_oivtutar:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_oivtutar.ToString();
			break;
		case enum_DegerTipi.hiz_sat_ufrs_fark_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_sat_ufrs_fark_muh_kod;
			break;
		case enum_DegerTipi.hiz_sat_iade_ufrs_fark_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_sat_iade_ufrs_fark_muh_kod;
			break;
		case enum_DegerTipi.hiz_mal_ufrs_fark_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_mal_ufrs_fark_muh_kod;
			break;
		case enum_DegerTipi.hiz_sat_mal_ufrs_fark_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_sat_mal_ufrs_fark_muh_kod;
			break;
		case enum_DegerTipi.hiz_mal_yan_ufrs_fark_muh_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_mal_yan_ufrs_fark_muh_kod;
			break;
		case enum_DegerTipi.hiz_muh_sat_ufrs_fark_isk_kod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_muh_sat_ufrs_fark_isk_kod;
			break;
		case enum_DegerTipi.hiz_muh_aIiskufrs_fark_muhkod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_muh_aIiskufrs_fark_muhkod;
			break;
		case enum_DegerTipi.hiz_ilavemasufrs_fark_muhkod:
			result = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu).hiz_ilavemasufrs_fark_muhkod;
			break;
		case enum_DegerTipi.sevk_adr_adres_no:
		{
			CariAdres cariAdres30 = new CariAdres();
			foreach (CariAdres item in satir.cari.CariAdresleri)
			{
				if (item.adr_adres_no == satir.sevkadresno)
				{
					cariAdres30 = item;
					break;
				}
			}
			result = cariAdres30.adr_adres_no.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_cari_kod:
		{
			CariAdres cariAdres29 = new CariAdres();
			foreach (CariAdres item2 in satir.cari.CariAdresleri)
			{
				if (item2.adr_adres_no == satir.sevkadresno)
				{
					cariAdres29 = item2;
					break;
				}
			}
			result = cariAdres29.adr_cari_kod;
			break;
		}
		case enum_DegerTipi.sevk_adr_cadde:
		{
			CariAdres cariAdres28 = new CariAdres();
			foreach (CariAdres item3 in satir.cari.CariAdresleri)
			{
				if (item3.adr_adres_no == satir.sevkadresno)
				{
					cariAdres28 = item3;
					break;
				}
			}
			result = cariAdres28.adr_cadde;
			break;
		}
		case enum_DegerTipi.sevk_adr_sokak:
		{
			CariAdres cariAdres27 = new CariAdres();
			foreach (CariAdres item4 in satir.cari.CariAdresleri)
			{
				if (item4.adr_adres_no == satir.sevkadresno)
				{
					cariAdres27 = item4;
					break;
				}
			}
			result = cariAdres27.adr_sokak;
			break;
		}
		case enum_DegerTipi.sevk_adr_posta_kodu:
		{
			CariAdres cariAdres26 = new CariAdres();
			foreach (CariAdres item5 in satir.cari.CariAdresleri)
			{
				if (item5.adr_adres_no == satir.sevkadresno)
				{
					cariAdres26 = item5;
					break;
				}
			}
			result = cariAdres26.adr_posta_kodu;
			break;
		}
		case enum_DegerTipi.sevk_adr_ilce:
		{
			CariAdres cariAdres25 = new CariAdres();
			foreach (CariAdres item6 in satir.cari.CariAdresleri)
			{
				if (item6.adr_adres_no == satir.sevkadresno)
				{
					cariAdres25 = item6;
					break;
				}
			}
			result = cariAdres25.adr_ilce;
			break;
		}
		case enum_DegerTipi.sevk_adr_il:
		{
			CariAdres cariAdres24 = new CariAdres();
			foreach (CariAdres item7 in satir.cari.CariAdresleri)
			{
				if (item7.adr_adres_no == satir.sevkadresno)
				{
					cariAdres24 = item7;
					break;
				}
			}
			result = cariAdres24.adr_il;
			break;
		}
		case enum_DegerTipi.sevk_adr_ulke:
		{
			CariAdres cariAdres23 = new CariAdres();
			foreach (CariAdres item8 in satir.cari.CariAdresleri)
			{
				if (item8.adr_adres_no == satir.sevkadresno)
				{
					cariAdres23 = item8;
					break;
				}
			}
			result = cariAdres23.adr_ulke;
			break;
		}
		case enum_DegerTipi.sevk_adr_tel_ulke_kodu:
		{
			CariAdres cariAdres22 = new CariAdres();
			foreach (CariAdres item9 in satir.cari.CariAdresleri)
			{
				if (item9.adr_adres_no == satir.sevkadresno)
				{
					cariAdres22 = item9;
					break;
				}
			}
			result = cariAdres22.adr_tel_ulke_kodu;
			break;
		}
		case enum_DegerTipi.sevk_adr_tel_bolge_kodu:
		{
			CariAdres cariAdres21 = new CariAdres();
			foreach (CariAdres item10 in satir.cari.CariAdresleri)
			{
				if (item10.adr_adres_no == satir.sevkadresno)
				{
					cariAdres21 = item10;
					break;
				}
			}
			result = cariAdres21.adr_tel_bolge_kodu;
			break;
		}
		case enum_DegerTipi.sevk_adr_tel_no1:
		{
			CariAdres cariAdres20 = new CariAdres();
			foreach (CariAdres item11 in satir.cari.CariAdresleri)
			{
				if (item11.adr_adres_no == satir.sevkadresno)
				{
					cariAdres20 = item11;
					break;
				}
			}
			result = cariAdres20.adr_tel_no1;
			break;
		}
		case enum_DegerTipi.sevk_adr_tel_no2:
		{
			CariAdres cariAdres19 = new CariAdres();
			foreach (CariAdres item12 in satir.cari.CariAdresleri)
			{
				if (item12.adr_adres_no == satir.sevkadresno)
				{
					cariAdres19 = item12;
					break;
				}
			}
			result = cariAdres19.adr_tel_no2;
			break;
		}
		case enum_DegerTipi.sevk_adr_tel_faxno:
		{
			CariAdres cariAdres18 = new CariAdres();
			foreach (CariAdres item13 in satir.cari.CariAdresleri)
			{
				if (item13.adr_adres_no == satir.sevkadresno)
				{
					cariAdres18 = item13;
					break;
				}
			}
			result = cariAdres18.adr_tel_faxno;
			break;
		}
		case enum_DegerTipi.sevk_adr_tel_modem:
		{
			CariAdres cariAdres17 = new CariAdres();
			foreach (CariAdres item14 in satir.cari.CariAdresleri)
			{
				if (item14.adr_adres_no == satir.sevkadresno)
				{
					cariAdres17 = item14;
					break;
				}
			}
			result = cariAdres17.adr_tel_modem;
			break;
		}
		case enum_DegerTipi.sevk_adr_yon_kodu:
		{
			CariAdres cariAdres16 = new CariAdres();
			foreach (CariAdres item15 in satir.cari.CariAdresleri)
			{
				if (item15.adr_adres_no == satir.sevkadresno)
				{
					cariAdres16 = item15;
					break;
				}
			}
			result = cariAdres16.adr_yon_kodu;
			break;
		}
		case enum_DegerTipi.sevk_adr_temsilci_kodu:
		{
			CariAdres cariAdres15 = new CariAdres();
			foreach (CariAdres item16 in satir.cari.CariAdresleri)
			{
				if (item16.adr_adres_no == satir.sevkadresno)
				{
					cariAdres15 = item16;
					break;
				}
			}
			result = cariAdres15.adr_temsilci_kodu;
			break;
		}
		case enum_DegerTipi.sevk_adr_ozel_not:
		{
			CariAdres cariAdres14 = new CariAdres();
			foreach (CariAdres item17 in satir.cari.CariAdresleri)
			{
				if (item17.adr_adres_no == satir.sevkadresno)
				{
					cariAdres14 = item17;
					break;
				}
			}
			result = cariAdres14.adr_ozel_not;
			break;
		}
		case enum_DegerTipi.sevk_adr_ziyaretgunu:
		{
			CariAdres cariAdres13 = new CariAdres();
			foreach (CariAdres item18 in satir.cari.CariAdresleri)
			{
				if (item18.adr_adres_no == satir.sevkadresno)
				{
					cariAdres13 = item18;
					break;
				}
			}
			result = cariAdres13.adr_ziyaretgunu;
			break;
		}
		case enum_DegerTipi.sevk_adr_gps_enlem:
		{
			CariAdres cariAdres12 = new CariAdres();
			foreach (CariAdres item19 in satir.cari.CariAdresleri)
			{
				if (item19.adr_adres_no == satir.sevkadresno)
				{
					cariAdres12 = item19;
					break;
				}
			}
			result = cariAdres12.adr_gps_enlem.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_gps_boylam:
		{
			CariAdres cariAdres11 = new CariAdres();
			foreach (CariAdres item20 in satir.cari.CariAdresleri)
			{
				if (item20.adr_adres_no == satir.sevkadresno)
				{
					cariAdres11 = item20;
					break;
				}
			}
			result = cariAdres11.adr_gps_boylam.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_uzaklik_kodu:
		{
			CariAdres cariAdres10 = new CariAdres();
			foreach (CariAdres item21 in satir.cari.CariAdresleri)
			{
				if (item21.adr_adres_no == satir.sevkadresno)
				{
					cariAdres10 = item21;
					break;
				}
			}
			result = cariAdres10.adr_uzaklik_kodu.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziyaretperyodu:
		{
			CariAdres cariAdres9 = new CariAdres();
			foreach (CariAdres item22 in satir.cari.CariAdresleri)
			{
				if (item22.adr_adres_no == satir.sevkadresno)
				{
					cariAdres9 = item22;
					break;
				}
			}
			result = cariAdres9.adr_ziyaretperyodu.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziyarethaftasi:
		{
			CariAdres cariAdres8 = new CariAdres();
			foreach (CariAdres item23 in satir.cari.CariAdresleri)
			{
				if (item23.adr_adres_no == satir.sevkadresno)
				{
					cariAdres8 = item23;
					break;
				}
			}
			result = cariAdres8.adr_ziyarethaftasi.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziygunu2_1:
		{
			CariAdres cariAdres7 = new CariAdres();
			foreach (CariAdres item24 in satir.cari.CariAdresleri)
			{
				if (item24.adr_adres_no == satir.sevkadresno)
				{
					cariAdres7 = item24;
					break;
				}
			}
			result = cariAdres7.adr_ziygunu2_1.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziygunu2_2:
		{
			CariAdres cariAdres6 = new CariAdres();
			foreach (CariAdres item25 in satir.cari.CariAdresleri)
			{
				if (item25.adr_adres_no == satir.sevkadresno)
				{
					cariAdres6 = item25;
					break;
				}
			}
			result = cariAdres6.adr_ziygunu2_2.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziygunu2_3:
		{
			CariAdres cariAdres5 = new CariAdres();
			foreach (CariAdres item26 in satir.cari.CariAdresleri)
			{
				if (item26.adr_adres_no == satir.sevkadresno)
				{
					cariAdres5 = item26;
					break;
				}
			}
			result = cariAdres5.adr_ziygunu2_3.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziygunu2_4:
		{
			CariAdres cariAdres4 = new CariAdres();
			foreach (CariAdres item27 in satir.cari.CariAdresleri)
			{
				if (item27.adr_adres_no == satir.sevkadresno)
				{
					cariAdres4 = item27;
					break;
				}
			}
			result = cariAdres4.adr_ziygunu2_4.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziygunu2_5:
		{
			CariAdres cariAdres3 = new CariAdres();
			foreach (CariAdres item28 in satir.cari.CariAdresleri)
			{
				if (item28.adr_adres_no == satir.sevkadresno)
				{
					cariAdres3 = item28;
					break;
				}
			}
			result = cariAdres3.adr_ziygunu2_5.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziygunu2_6:
		{
			CariAdres cariAdres2 = new CariAdres();
			foreach (CariAdres item29 in satir.cari.CariAdresleri)
			{
				if (item29.adr_adres_no == satir.sevkadresno)
				{
					cariAdres2 = item29;
					break;
				}
			}
			result = cariAdres2.adr_ziygunu2_6.ToString();
			break;
		}
		case enum_DegerTipi.sevk_adr_ziygunu2_7:
		{
			CariAdres cariAdres = new CariAdres();
			foreach (CariAdres item30 in satir.cari.CariAdresleri)
			{
				if (item30.adr_adres_no == satir.sevkadresno)
				{
					cariAdres = item30;
					break;
				}
			}
			result = cariAdres.adr_ziygunu2_7.ToString();
			break;
		}
		}
		return result;
	}

	private string KriterGercekYeniDegerBul(enum_IslemTipi islem_tipi, string yenideger, string eskideger)
	{
		string result = eskideger;
		switch (islem_tipi)
		{
		case enum_IslemTipi.Degistir:
			result = yenideger;
			break;
		case enum_IslemTipi.BasinaEkle:
			result = yenideger + eskideger;
			break;
		case enum_IslemTipi.SonunaEkle:
			result = eskideger + yenideger;
			break;
		case enum_IslemTipi.BasindanNkadarKarakterKirp:
		{
			int num13 = int.Parse(yenideger);
			result = eskideger.Substring(num13, eskideger.Length - num13);
			break;
		}
		case enum_IslemTipi.BasindanBelirtilenKaraktereKadarKirp:
		{
			int num12 = eskideger.IndexOf(yenideger);
			num12++;
			if (num12 > 0)
			{
				result = eskideger.Substring(num12, eskideger.Length - num12);
			}
			break;
		}
		case enum_IslemTipi.SonundanNkadarKarakterKirp:
		{
			int num11 = int.Parse(yenideger);
			result = eskideger.Substring(0, eskideger.Length - num11);
			break;
		}
		case enum_IslemTipi.SonundanBelirtilenKaraktereKadarKirp:
		{
			int num10 = eskideger.LastIndexOf(yenideger);
			if (num10 > 0)
			{
				result = eskideger.Substring(0, eskideger.Length - (eskideger.Length - num10));
			}
			break;
		}
		case enum_IslemTipi.Topla:
			try
			{
				double num8 = double.Parse(eskideger);
				double num9 = double.Parse(yenideger);
				result = (num8 + num9).ToString();
			}
			catch
			{
			}
			break;
		case enum_IslemTipi.Cikart:
			try
			{
				double num6 = double.Parse(eskideger);
				double num7 = double.Parse(yenideger);
				result = (num6 - num7).ToString();
			}
			catch
			{
			}
			break;
		case enum_IslemTipi.Carp:
			try
			{
				double num4 = double.Parse(eskideger);
				double num5 = double.Parse(yenideger);
				result = (num4 * num5).ToString();
			}
			catch
			{
			}
			break;
		case enum_IslemTipi.Bol:
			try
			{
				double num2 = double.Parse(eskideger);
				double num3 = double.Parse(yenideger);
				result = (num2 / num3).ToString();
			}
			catch
			{
			}
			break;
		case enum_IslemTipi.GunEkle:
			try
			{
				DateTime dateTime2 = DateTime.Parse(eskideger);
				double value = double.Parse(yenideger);
				result = dateTime2.AddDays(value).ToString();
			}
			catch
			{
			}
			break;
		case enum_IslemTipi.GunCikart:
			try
			{
				DateTime dateTime = DateTime.Parse(eskideger);
				double num = double.Parse(yenideger);
				result = dateTime.AddDays(-1.0 * num).ToString();
			}
			catch
			{
			}
			break;
		}
		return result;
	}

	private bool KriterVeriKontrolu(GenelEvrakSatir satir, enum_KriterAramaAlanlari aranacak_alan, enum_Operator kullanilan_operator, string aranacak_deger)
	{
		switch (aranacak_alan)
		{
		case enum_KriterAramaAlanlari.KayitID:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.KayitID.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.evraktipi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.evraktipi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.normaliade:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.normaliade.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kapamasekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kapamasekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.ticaretturu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.ticaretturu.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.evraktarih:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.evraktarih.ToString("dd.MM.yyyy"), aranacak_deger);
		case enum_KriterAramaAlanlari.evraknoseri:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.evraknoseri, aranacak_deger);
		case enum_KriterAramaAlanlari.evraknosira:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.evraknosira.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.belgeno:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.belgeno, aranacak_deger);
		case enum_KriterAramaAlanlari.belgetarih:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.belgetarih.ToString("dd.MM.yyyy"), aranacak_deger);
		case enum_KriterAramaAlanlari.odemeplani:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.odemeplani.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.fiyatlistesi_aciklama:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.fiyatlistesi.sfl_aciklama, aranacak_deger);
		case enum_KriterAramaAlanlari.fiyatlistesi_kdvdahil:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.fiyatlistesi.sfl_kdvdahil.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.fiyatlistesi_sirano:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.fiyatlistesi.sfl_sirano.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.dovizcinsi_doviz_adi:
			return KriterTekVeriKontrolu(kullanilan_operator, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(satir.dovizcinsi).Kur_adi, aranacak_deger);
		case enum_KriterAramaAlanlari.dovizcinsi_doviz_kod:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.dovizcinsi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.dovizcinsi_doviz_sembol:
			return KriterTekVeriKontrolu(kullanilan_operator, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(satir.dovizcinsi).Kur_sembol, aranacak_deger);
		case enum_KriterAramaAlanlari.kur:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kur.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kaynakdepo_depo_adi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kaynakdepo.dep_adi, aranacak_deger);
		case enum_KriterAramaAlanlari.kaynakdepo_depo_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kaynakdepo.dep_no.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.hedefdepo_depo_adi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.hedefdepo.dep_adi, aranacak_deger);
		case enum_KriterAramaAlanlari.hedefdepo_depo_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.hedefdepo.dep_no.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.proje_kodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.proje.pro_kodu, aranacak_deger);
		case enum_KriterAramaAlanlari.proje_adi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.proje.pro_adi, aranacak_deger);
		case enum_KriterAramaAlanlari.sorumlulukmerkezi_kodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.sorumlulukmerkezi.som_kod, aranacak_deger);
		case enum_KriterAramaAlanlari.sorumlulukmerkezi_adi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.sorumlulukmerkezi.som_isim, aranacak_deger);
		case enum_KriterAramaAlanlari.temsilcikodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.temsilcikodu, aranacak_deger);
		case enum_KriterAramaAlanlari.firma_sirano:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.firma.fir_sirano.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.firma_unvan:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.firma.fir_unvan, aranacak_deger);
		case enum_KriterAramaAlanlari.sube_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.sube.Sube_no.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.sube_adi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.sube.Sube_adi, aranacak_deger);
		case enum_KriterAramaAlanlari.sevkteslimtarihi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.sevkteslimtarihi.ToString("dd.MM.yyyy"), aranacak_deger);
		case enum_KriterAramaAlanlari.kapamahesapkodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kapamahesapkodu, aranacak_deger);
		case enum_KriterAramaAlanlari.faturaaciklama:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.faturaaciklama, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama1, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama2, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama3:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama3, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama4:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama4, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama5:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama5, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama6:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama6, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama7:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama7, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama8:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama8, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama9:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama9, aranacak_deger);
		case enum_KriterAramaAlanlari.aciklama10:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama10, aranacak_deger);
		case enum_KriterAramaAlanlari.satircinsi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.satircinsi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.miktar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.miktar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.miktar2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.miktar2.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.vergi_pntr:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.vergi_pntr.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.fiyat_fark_mi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.fiyat_fark_mi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_string1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string1, aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_string2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string2, aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_string3:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string3, aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_string4:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string4, aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_string5:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string5, aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_double1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double1.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_double2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double2.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_double3:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double3.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_double4:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double4.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_double5:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double5.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_bool1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool1.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_bool2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool2.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_bool3:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool3.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_bool4:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool4.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kriter_bool5:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool5.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_brut_fiyat:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.FiyatBrut.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto1_uygulama_sekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_1_UygulamaSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto1_yuzdeveyatutar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_1_YuzdeVeyaMiktar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto2_uygulama_sekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_2_UygulamaSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto2_yuzdeveyatutar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_2_YuzdeVeyaMiktar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto3_uygulama_sekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_3_UygulamaSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto3_yuzdeveyatutar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_3_YuzdeVeyaMiktar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto4_uygulama_sekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_4_UygulamaSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto4_yuzdeveyatutar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_4_YuzdeVeyaMiktar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto5_uygulama_sekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_5_UygulamaSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto5_yuzdeveyatutar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_5_YuzdeVeyaMiktar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto6_uygulama_sekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_6_UygulamaSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_iskonto6_yuzdeveyatutar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.Iskonto_6_YuzdeVeyaMiktar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_otv_uygulama_sekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.OtvUygulamaSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_otv_yuzdeveyatutar:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.OtvYuzdeVeyaTutar.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.birimfiyat_otv_vergipntr:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.birimfiyat.OtvVergiPntr.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.ara_toplam:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.GetEvrakAraToplam().ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.iskonto_toplam:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.GetEvrakIskontoToplam().ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.kdv_toplam:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.GetEvrakKdvToplam(_mikrouygulamabilgileri.vergitanimlari).ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.masraf_toplam:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.GetEvrakMasrafToplam().ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.otv_toplam:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.GetEvrakOtvToplam().ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.yekun:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.GetEvrakYekun(_mikrouygulamabilgileri.vergitanimlari).ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_kod:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_kod, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_unvan1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_unvan1, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_unvan2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_unvan2, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_muh_kod:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_muh_kod, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_muh_kod1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_muh_kod1, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_muh_kod2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_muh_kod2, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_vdaire_adi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vdaire_adi, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_vdaire_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vdaire_no, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_Ana_cari_kodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_Ana_cari_kodu, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_bolge_kodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_bolge_kodu, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_grup_kodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_grup_kodu, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_temsilci_kodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_temsilci_kodu, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_sektor_kodu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_sektor_kodu, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_satis_isk_kod:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_satis_isk_kod, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_special1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_special1, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_special2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_special2, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_special3:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_special3, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_sicil_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_sicil_no, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_VergiKimlikNo:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_VergiKimlikNo, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_vade_fark_yuz:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vade_fark_yuz.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_vade_fark_yuz1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vade_fark_yuz1.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_vade_fark_yuz2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vade_fark_yuz2.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_tipi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_tipi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_doviz_cinsi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_doviz_cinsi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_doviz_cinsi1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_doviz_cinsi1.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_doviz_cinsi2:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_doviz_cinsi2.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_odeme_gunu:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_odeme_gunu.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_hareket_tipi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_hareket_tipi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_odemeplan_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_odemeplan_no.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_satis_fk:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_satis_fk.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_KurHesapSekli:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_KurHesapSekli.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_odeme_cinsi:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_odeme_cinsi.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_fatura_adres_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_fatura_adres_no.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_sevk_adres_no:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_sevk_adres_no.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_banka_hesapno1:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_banka_hesapno1, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_CepTel:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_CepTel, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_Email:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_Email, aranacak_deger);
		case enum_KriterAramaAlanlari.cari_VarsayilanGirisDepo:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_VarsayilanGirisDepo.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.cari_VarsayilanCikisDepo:
			return KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_VarsayilanCikisDepo.ToString(), aranacak_deger);
		case enum_KriterAramaAlanlari.sto_kod:
		{
			Stok stok30 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok30.sto_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_isim:
		{
			Stok stok29 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok29.sto_isim, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_perakende_vergi:
		{
			Stok stok28 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok28.sto_perakende_vergi_yeni.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_toptan_vergi:
		{
			Stok stok27 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok27.sto_toptan_vergi_yeni.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_kisa_ismi:
		{
			Stok stok26 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok26.sto_kisa_ismi, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_yabanci_isim:
		{
			Stok stok25 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok25.sto_yabanci_isim, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_sat_cari_kod:
		{
			Stok stok24 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok24.sto_sat_cari_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_cins:
		{
			Stok stok23 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok23.sto_cins.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_doviz_cinsi:
		{
			Stok stok22 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok22.sto_doviz_cinsi.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_detay_takip:
		{
			Stok stok21 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok21.sto_detay_takip.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim1_ad:
		{
			Stok stok20 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok20.sto_birim1_ad, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim1_katsayi:
		{
			Stok stok19 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok19.sto_birim1_katsayi.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim2_ad:
		{
			Stok stok18 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok18.sto_birim2_ad, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim2_katsayi:
		{
			Stok stok17 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok17.sto_birim2_katsayi.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim3_ad:
		{
			Stok stok16 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok16.sto_birim3_ad, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim3_katsayi:
		{
			Stok stok15 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok15.sto_birim3_katsayi.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim4_ad:
		{
			Stok stok14 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok14.sto_birim4_ad, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_birim4_katsayi:
		{
			Stok stok13 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok13.sto_birim4_katsayi.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_bedenli_takip:
		{
			Stok stok12 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok12.sto_bedenli_takip.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_renkDetayli:
		{
			Stok stok11 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok11.sto_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_beden_kodu:
		{
			Stok stok10 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok10.sto_beden_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_renk_kodu:
		{
			Stok stok9 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok9.sto_renk_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_altgrup_kod:
		{
			Stok stok8 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok8.sto_altgrup_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_anagrup_kod:
		{
			Stok stok7 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok7.sto_anagrup_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_sektor_kodu:
		{
			Stok stok6 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok6.sto_sektor_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_marka_kodu:
		{
			Stok stok5 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok5.sto_marka_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_model_kodu:
		{
			Stok stok4 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok4.sto_model_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_uretici_kodu:
		{
			Stok stok3 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok3.sto_uretici_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_reyon_kodu:
		{
			Stok stok2 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok2.sto_reyon_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sto_standartmaliyet:
		{
			Stok stok = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu, enum_toptan_perakende.Toptan);
			return KriterTekVeriKontrolu(kullanilan_operator, stok.sto_standartmaliyet.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_kod:
		{
			Hizmet hizmet28 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet28.hiz_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_tip:
		{
			Hizmet hizmet27 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet27.hiz_tip.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_isim:
		{
			Hizmet hizmet26 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet26.hiz_isim, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_yabanci_isim:
		{
			Hizmet hizmet25 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet25.hiz_yabanci_isim, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_tipkod:
		{
			Hizmet hizmet24 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet24.hiz_tipkod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_sinifkod:
		{
			Hizmet hizmet23 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet23.hiz_sinifkod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_grupkod:
		{
			Hizmet hizmet22 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet22.hiz_grupkod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_sat_muh_kod:
		{
			Hizmet hizmet21 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet21.hiz_sat_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_sat_iade_muh_kod:
		{
			Hizmet hizmet20 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet20.hiz_sat_iade_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_mal_muh_kod:
		{
			Hizmet hizmet19 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet19.hiz_mal_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_sat_mal_muh_kod:
		{
			Hizmet hizmet18 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet18.hiz_sat_mal_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_mal_yan_muh_kod:
		{
			Hizmet hizmet17 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet17.hiz_mal_yan_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_isk_grup:
		{
			Hizmet hizmet16 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet16.hiz_isk_grup, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_KDV:
		{
			Hizmet hizmet15 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet15.hiz_KDV.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_muh_sat_isk_kod:
		{
			Hizmet hizmet14 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet14.hiz_muh_sat_isk_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_muh_aIiskmuhkod:
		{
			Hizmet hizmet13 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet13.hiz_muh_aIiskmuhkod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_ilavemasmuhkod:
		{
			Hizmet hizmet12 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet12.hiz_ilavemasmuhkod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_operasyon_suresi:
		{
			Hizmet hizmet11 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet11.hiz_operasyon_suresi.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_oivuygulama:
		{
			Hizmet hizmet10 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet10.hiz_oivuygulama.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_oivtutar:
		{
			Hizmet hizmet9 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet9.hiz_oivtutar.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_sat_ufrs_fark_muh_kod:
		{
			Hizmet hizmet8 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet8.hiz_sat_ufrs_fark_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_sat_iade_ufrs_fark_muh_kod:
		{
			Hizmet hizmet7 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet7.hiz_sat_iade_ufrs_fark_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_mal_ufrs_fark_muh_kod:
		{
			Hizmet hizmet6 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet6.hiz_mal_ufrs_fark_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_sat_mal_ufrs_fark_muh_kod:
		{
			Hizmet hizmet5 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet5.hiz_sat_mal_ufrs_fark_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_mal_yan_ufrs_fark_muh_kod:
		{
			Hizmet hizmet4 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet4.hiz_mal_yan_ufrs_fark_muh_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_muh_sat_ufrs_fark_isk_kod:
		{
			Hizmet hizmet3 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet3.hiz_muh_sat_ufrs_fark_isk_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_muh_aIiskufrs_fark_muhkod:
		{
			Hizmet hizmet2 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet2.hiz_muh_aIiskufrs_fark_muhkod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.hiz_ilavemasufrs_fark_muhkod:
		{
			Hizmet hizmet = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, satir.stokhizmetkodu);
			return KriterTekVeriKontrolu(kullanilan_operator, hizmet.hiz_ilavemasufrs_fark_muhkod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_adres_no:
		{
			CariAdres cariAdres30 = new CariAdres();
			foreach (CariAdres item in satir.cari.CariAdresleri)
			{
				if (item.adr_adres_no == satir.sevkadresno)
				{
					cariAdres30 = item;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres30.adr_adres_no.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_cari_kod:
		{
			CariAdres cariAdres29 = new CariAdres();
			foreach (CariAdres item2 in satir.cari.CariAdresleri)
			{
				if (item2.adr_adres_no == satir.sevkadresno)
				{
					cariAdres29 = item2;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres29.adr_cari_kod, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_cadde:
		{
			CariAdres cariAdres28 = new CariAdres();
			foreach (CariAdres item3 in satir.cari.CariAdresleri)
			{
				if (item3.adr_adres_no == satir.sevkadresno)
				{
					cariAdres28 = item3;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres28.adr_cadde, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_sokak:
		{
			CariAdres cariAdres27 = new CariAdres();
			foreach (CariAdres item4 in satir.cari.CariAdresleri)
			{
				if (item4.adr_adres_no == satir.sevkadresno)
				{
					cariAdres27 = item4;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres27.adr_sokak, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_posta_kodu:
		{
			CariAdres cariAdres26 = new CariAdres();
			foreach (CariAdres item5 in satir.cari.CariAdresleri)
			{
				if (item5.adr_adres_no == satir.sevkadresno)
				{
					cariAdres26 = item5;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres26.adr_posta_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ilce:
		{
			CariAdres cariAdres25 = new CariAdres();
			foreach (CariAdres item6 in satir.cari.CariAdresleri)
			{
				if (item6.adr_adres_no == satir.sevkadresno)
				{
					cariAdres25 = item6;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres25.adr_ilce, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_il:
		{
			CariAdres cariAdres24 = new CariAdres();
			foreach (CariAdres item7 in satir.cari.CariAdresleri)
			{
				if (item7.adr_adres_no == satir.sevkadresno)
				{
					cariAdres24 = item7;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres24.adr_il, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ulke:
		{
			CariAdres cariAdres23 = new CariAdres();
			foreach (CariAdres item8 in satir.cari.CariAdresleri)
			{
				if (item8.adr_adres_no == satir.sevkadresno)
				{
					cariAdres23 = item8;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres23.adr_ulke, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_tel_ulke_kodu:
		{
			CariAdres cariAdres22 = new CariAdres();
			foreach (CariAdres item9 in satir.cari.CariAdresleri)
			{
				if (item9.adr_adres_no == satir.sevkadresno)
				{
					cariAdres22 = item9;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres22.adr_tel_ulke_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_tel_bolge_kodu:
		{
			CariAdres cariAdres21 = new CariAdres();
			foreach (CariAdres item10 in satir.cari.CariAdresleri)
			{
				if (item10.adr_adres_no == satir.sevkadresno)
				{
					cariAdres21 = item10;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres21.adr_tel_bolge_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_tel_no1:
		{
			CariAdres cariAdres20 = new CariAdres();
			foreach (CariAdres item11 in satir.cari.CariAdresleri)
			{
				if (item11.adr_adres_no == satir.sevkadresno)
				{
					cariAdres20 = item11;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres20.adr_tel_no1, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_tel_no2:
		{
			CariAdres cariAdres19 = new CariAdres();
			foreach (CariAdres item12 in satir.cari.CariAdresleri)
			{
				if (item12.adr_adres_no == satir.sevkadresno)
				{
					cariAdres19 = item12;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres19.adr_tel_no2, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_tel_faxno:
		{
			CariAdres cariAdres18 = new CariAdres();
			foreach (CariAdres item13 in satir.cari.CariAdresleri)
			{
				if (item13.adr_adres_no == satir.sevkadresno)
				{
					cariAdres18 = item13;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres18.adr_tel_faxno, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_tel_modem:
		{
			CariAdres cariAdres17 = new CariAdres();
			foreach (CariAdres item14 in satir.cari.CariAdresleri)
			{
				if (item14.adr_adres_no == satir.sevkadresno)
				{
					cariAdres17 = item14;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres17.adr_tel_modem, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_yon_kodu:
		{
			CariAdres cariAdres16 = new CariAdres();
			foreach (CariAdres item15 in satir.cari.CariAdresleri)
			{
				if (item15.adr_adres_no == satir.sevkadresno)
				{
					cariAdres16 = item15;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres16.adr_yon_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_temsilci_kodu:
		{
			CariAdres cariAdres15 = new CariAdres();
			foreach (CariAdres item16 in satir.cari.CariAdresleri)
			{
				if (item16.adr_adres_no == satir.sevkadresno)
				{
					cariAdres15 = item16;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres15.adr_temsilci_kodu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ozel_not:
		{
			CariAdres cariAdres14 = new CariAdres();
			foreach (CariAdres item17 in satir.cari.CariAdresleri)
			{
				if (item17.adr_adres_no == satir.sevkadresno)
				{
					cariAdres14 = item17;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres14.adr_ozel_not, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziyaretgunu:
		{
			CariAdres cariAdres13 = new CariAdres();
			foreach (CariAdres item18 in satir.cari.CariAdresleri)
			{
				if (item18.adr_adres_no == satir.sevkadresno)
				{
					cariAdres13 = item18;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres13.adr_ziyaretgunu, aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_gps_enlem:
		{
			CariAdres cariAdres12 = new CariAdres();
			foreach (CariAdres item19 in satir.cari.CariAdresleri)
			{
				if (item19.adr_adres_no == satir.sevkadresno)
				{
					cariAdres12 = item19;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres12.adr_gps_enlem.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_gps_boylam:
		{
			CariAdres cariAdres11 = new CariAdres();
			foreach (CariAdres item20 in satir.cari.CariAdresleri)
			{
				if (item20.adr_adres_no == satir.sevkadresno)
				{
					cariAdres11 = item20;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres11.adr_gps_boylam.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_uzaklik_kodu:
		{
			CariAdres cariAdres10 = new CariAdres();
			foreach (CariAdres item21 in satir.cari.CariAdresleri)
			{
				if (item21.adr_adres_no == satir.sevkadresno)
				{
					cariAdres10 = item21;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres10.adr_uzaklik_kodu.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziyaretperyodu:
		{
			CariAdres cariAdres9 = new CariAdres();
			foreach (CariAdres item22 in satir.cari.CariAdresleri)
			{
				if (item22.adr_adres_no == satir.sevkadresno)
				{
					cariAdres9 = item22;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres9.adr_ziyaretperyodu.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziyarethaftasi:
		{
			CariAdres cariAdres8 = new CariAdres();
			foreach (CariAdres item23 in satir.cari.CariAdresleri)
			{
				if (item23.adr_adres_no == satir.sevkadresno)
				{
					cariAdres8 = item23;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres8.adr_ziyarethaftasi.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziygunu2_1:
		{
			CariAdres cariAdres7 = new CariAdres();
			foreach (CariAdres item24 in satir.cari.CariAdresleri)
			{
				if (item24.adr_adres_no == satir.sevkadresno)
				{
					cariAdres7 = item24;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres7.adr_ziygunu2_1.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziygunu2_2:
		{
			CariAdres cariAdres6 = new CariAdres();
			foreach (CariAdres item25 in satir.cari.CariAdresleri)
			{
				if (item25.adr_adres_no == satir.sevkadresno)
				{
					cariAdres6 = item25;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres6.adr_ziygunu2_2.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziygunu2_3:
		{
			CariAdres cariAdres5 = new CariAdres();
			foreach (CariAdres item26 in satir.cari.CariAdresleri)
			{
				if (item26.adr_adres_no == satir.sevkadresno)
				{
					cariAdres5 = item26;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres5.adr_ziygunu2_3.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziygunu2_4:
		{
			CariAdres cariAdres4 = new CariAdres();
			foreach (CariAdres item27 in satir.cari.CariAdresleri)
			{
				if (item27.adr_adres_no == satir.sevkadresno)
				{
					cariAdres4 = item27;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres4.adr_ziygunu2_4.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziygunu2_5:
		{
			CariAdres cariAdres3 = new CariAdres();
			foreach (CariAdres item28 in satir.cari.CariAdresleri)
			{
				if (item28.adr_adres_no == satir.sevkadresno)
				{
					cariAdres3 = item28;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres3.adr_ziygunu2_5.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziygunu2_6:
		{
			CariAdres cariAdres2 = new CariAdres();
			foreach (CariAdres item29 in satir.cari.CariAdresleri)
			{
				if (item29.adr_adres_no == satir.sevkadresno)
				{
					cariAdres2 = item29;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres2.adr_ziygunu2_6.ToString(), aranacak_deger);
		}
		case enum_KriterAramaAlanlari.sevk_adr_ziygunu2_7:
		{
			CariAdres cariAdres = new CariAdres();
			foreach (CariAdres item30 in satir.cari.CariAdresleri)
			{
				if (item30.adr_adres_no == satir.sevkadresno)
				{
					cariAdres = item30;
					break;
				}
			}
			return KriterTekVeriKontrolu(kullanilan_operator, cariAdres.adr_ziygunu2_7.ToString(), aranacak_deger);
		}
		default:
			return false;
		}
	}

	private bool KriterTekVeriKontrolu(enum_Operator kullanilan_operator, string orjinal_veri, string aranacak_veri)
	{
		orjinal_veri = orjinal_veri.Trim().ToLower();
		aranacak_veri = aranacak_veri.Trim().ToLower();
		switch (kullanilan_operator)
		{
		case enum_Operator.Esit:
			if (orjinal_veri == aranacak_veri)
			{
				return true;
			}
			return false;
		case enum_Operator.EsitDegil:
			if (orjinal_veri != aranacak_veri)
			{
				return true;
			}
			return false;
		case enum_Operator.Icerir:
			return orjinal_veri.Contains(aranacak_veri);
		case enum_Operator.Icermez:
			return !orjinal_veri.Contains(aranacak_veri);
		case enum_Operator.IleBaslar:
			return orjinal_veri.StartsWith(aranacak_veri);
		case enum_Operator.IleBiter:
			return orjinal_veri.EndsWith(aranacak_veri);
		case enum_Operator.Buyuk:
			try
			{
				double num7 = double.Parse(orjinal_veri);
				double num8 = double.Parse(aranacak_veri);
				if (num7 > num8)
				{
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		case enum_Operator.BuyukEsit:
			try
			{
				double num5 = double.Parse(orjinal_veri);
				double num6 = double.Parse(aranacak_veri);
				if (num5 >= num6)
				{
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		case enum_Operator.Kucuk:
			try
			{
				double num3 = double.Parse(orjinal_veri);
				double num4 = double.Parse(aranacak_veri);
				if (num3 < num4)
				{
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		case enum_Operator.KucukEsit:
			try
			{
				double num = double.Parse(orjinal_veri);
				double num2 = double.Parse(aranacak_veri);
				if (num <= num2)
				{
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		default:
			return false;
		}
	}

	private void OtomatikEvrakSiraNoVer(bool SatirBirlestir)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		foreach (GenelEvrakSatir item in _satirlar._satirlar)
		{
			if (item.Aktar && item.evraknosira == 0)
			{
				item.evraknosira = OtomatikTekSatirEvrakSiraNoVer(item, SatirBirlestir);
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private int OtomatikTekSatirEvrakSiraNoVer(GenelEvrakSatir satir, bool SatirBirlestir)
	{
		int num = 0;
		if (SatirBirlestir)
		{
			foreach (GenelEvrakSatir item in _satirlar._satirlar)
			{
				if (item.AktarimDurumu != enum_GenelEvrakAktarimDurumu.Aktarilmis && item.Aktar && item.evraknosira != 0 && item.firma.fir_sirano == satir.firma.fir_sirano && item.sube.Sube_no == satir.sube.Sube_no && item.evraktipi == satir.evraktipi && item.normaliade == satir.normaliade && item.ticaretturu == satir.ticaretturu && item.evraknoseri == satir.evraknoseri && item.evraktarih.Year == satir.evraktarih.Year && item.evraktarih.Month == satir.evraktarih.Month && item.evraktarih.Day == satir.evraktarih.Day && item.belgeno == satir.belgeno && item.belgetarih.Year == satir.belgetarih.Year && item.belgetarih.Month == satir.belgetarih.Month && item.belgetarih.Day == satir.belgetarih.Day && item.cari.cari_kod == satir.cari.cari_kod && item.sevkadresno == satir.sevkadresno && item.dovizcinsi == satir.dovizcinsi && item.kapamasekli == satir.kapamasekli && item.kapamahesapkodu == satir.kapamahesapkodu && item.odemeplani == satir.odemeplani && item.kaynakdepo.dep_no == satir.kaynakdepo.dep_no && item.hedefdepo.dep_no == satir.hedefdepo.dep_no)
				{
					num = item.evraknosira;
					break;
				}
			}
		}
		if (num == 0)
		{
			int num2 = 0;
			foreach (GenelEvrakSatir item2 in _satirlar._satirlar)
			{
				if (item2.AktarimDurumu != enum_GenelEvrakAktarimDurumu.Aktarilmis && item2.Aktar && item2.evraknosira != 0 && item2.evraktipi == satir.evraktipi && item2.evraknoseri == satir.evraknoseri && item2.evraknosira > num2)
				{
					num2 = item2.evraknosira;
				}
			}
			int num3 = 0;
			num3 = EvrakData.SonEvrakSiraNoBul(Db.Connection, satir.evraktipi, satir.evraknoseri);
			num = ((num2 <= num3) ? (num3 + 1) : (num2 + 1));
		}
		return num;
	}

	private void YeniCarileriSorVeAc(List<Cari> olmayan_cariler)
	{
		string text = "";
		foreach (Cari item in olmayan_cariler)
		{
			text = text + item.cari_kod + " , ";
		}
		if (MessageBox.Show(olmayan_cariler.Count + " adet yeni cari bulundu (" + text + "). Otomatik açılsın mı?", "ONAY", MessageBoxButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}
		foreach (Cari item2 in olmayan_cariler)
		{
			item2.KaydetYeniCari(Db.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.mikrokullanici.User_no);
		}
		RepositoryItemlarinLookupTablelariniGuncelle();
	}

	private void YeniStoklariSorVeAc(BindingList<Stok> olmayan_stoklar)
	{
		if (new TopluStokEkleme(_mikrouygulamabilgileri, olmayan_stoklar, "satirbazligenelevrakgirisi_stok_ekleme", "Yeni stoklar bulundu").ShowDialog() == DialogResult.OK)
		{
			RepositoryItemlarinLookupTablelariniGuncelle();
		}
	}

	private void YeniHizmetleriSorVeAc(List<Hizmet> olmayan_hizmetler)
	{
		string text = "";
		foreach (Hizmet item in olmayan_hizmetler)
		{
			text = text + item.hiz_kod + " , ";
		}
		if (MessageBox.Show(olmayan_hizmetler.Count + " adet yeni hizmet bulundu (" + text + "). Otomatik açılsın mı?", "ONAY", MessageBoxButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}
		foreach (Hizmet item2 in olmayan_hizmetler)
		{
			HizmetData.YeniHizmetKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item2, _mikrouygulamabilgileri.mikrokullanici.User_no);
		}
		RepositoryItemlarinLookupTablelariniGuncelle();
	}

	private void YeniProjeleriSorVeAc(List<Proje> olmayan_projeler)
	{
		string text = "";
		foreach (Proje item in olmayan_projeler)
		{
			text = text + item.pro_kodu + " , ";
		}
		if (MessageBox.Show(olmayan_projeler.Count + " adet yeni proje bulundu (" + text + "). Otomatik açılsın mı?", "ONAY", MessageBoxButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}
		foreach (Proje item2 in olmayan_projeler)
		{
			ProjeData.YeniProjeKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item2, _mikrouygulamabilgileri.mikrokullanici.User_no);
		}
		RepositoryItemlarinLookupTablelariniGuncelle();
	}

	private void YeniSorumlulukMerkezleriSorVeAc(List<SorumlulukMerkezi> olmayan_sorumlulukmerkezleri)
	{
		string text = "";
		foreach (SorumlulukMerkezi item in olmayan_sorumlulukmerkezleri)
		{
			text = text + item.som_kod + " , ";
		}
		if (MessageBox.Show(olmayan_sorumlulukmerkezleri.Count + " adet yeni sorumluluk merkezi bulundu (" + text + "). Otomatik açılsın mı?", "ONAY", MessageBoxButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}
		foreach (SorumlulukMerkezi item2 in olmayan_sorumlulukmerkezleri)
		{
			SorumlulukMerkeziData.YeniSorumlulukMerkeziKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item2, _mikrouygulamabilgileri.mikrokullanici.User_no);
		}
		RepositoryItemlarinLookupTablelariniGuncelle();
	}

	private void RepositoryItemlarinLookupTablelariniGuncelle()
	{
		_lookuptablolar = GenelData.GetLookupTablolar(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		RepositoryItemAyarla();
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
		this.repositoryItemGridLookUpEdit_Satir_Cinsi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_SatirCinsi = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemLookUpEdit_Satir_VergiPntr = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.repositoryItemLookUpEdit_Satir_IskontoSekli = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.gridControl_evraklar = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView_master = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand15 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_KayitID = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand9 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_Aktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_AktarimDurumu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand10 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_firma_no = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_sube_no = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_dbc_no = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_EvrakTipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_Evrak_Tipi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_EvrakTipi = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_evrak_NormalIade = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_NormalIade = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_NormalIade = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_evrak_TicaretTuru = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_TicaretTuru = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_TicaretTuru = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_evrak_EvrakSeri = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_EvrakSira = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Tarih = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemDateEdit_EvrakTarih = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
		this.gc_evrak_BelgeNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_BelgeTarihi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_CariKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_CariAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SevkAdresi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_FiyatListesi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_DovizCinsi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.gc_evrak_Kur = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemTextEdit_Evrak_Kur = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_evrak_AcikKapali = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_AcikKapali = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.repositoryItemGridLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_evrak_KapamaHesapKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_OdemePlani = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_DepoNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_DepoAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Hedef_DepoNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Hedef_DepoAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_ProjeKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_ProjeAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SorMerKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SorMerAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Plasiyer = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SevkTeslimTarihi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemTextEdit_Aciklamalar = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_evrak_Aciklama2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Aciklama10 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_special1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_special2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_special3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_cinsi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_fiyat_farki_mi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_HesapKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_HesapAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_parti_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_lot_no = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_birim_fiyat = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_miktar2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_vergi_pntr = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_otv_sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemLookUpEdit_Satir_OtvSekli = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.gc_evrak_otv_yuzde = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_otv_vergi_pntr = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_FaturaAciklama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemTextEdit_FaturaAciklama = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_iskonto1_uygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto1_yuzde_veya_miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto2_uygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto2_yuzde_veya_miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto3_uygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto3_yuzde_veya_miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto4_uygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto4_yuzde_veya_miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto5_uygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto5_yuzde_veya_miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto6_uygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto6_yuzde_veya_miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_kriter_string1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_string2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_string3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_string4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_string5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_double1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_double2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_double3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_double4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_double5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_bool1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_bool2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_bool3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_bool4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kriter_bool5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand17 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_ara_toplam = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_iskonto_toplam = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_masraf_toplam = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_otv_toplam = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_kdv_toplam = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_yekun = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.cardView1 = new DevExpress.XtraGrid.Views.Card.CardView();
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.sb_Secili_Evraklari_aktar = new DevExpress.XtraEditors.SimpleButton();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yeniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.acToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.kaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.farklıKaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.kapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.gorunumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_kolonlaraGoreGruplamaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_kolonSeciciyiGosterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_butunGruplariAcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_butunGruplariKapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.görünümüSaklaToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_farkliDosyaya_kaydet_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.görünümüYükleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_otomatikDosyadan_yukle_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_farkliDosyadan_yukle_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_otomatikDosyayiSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.evraklar_varsayılanaGeriDonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.aramaTablolariToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.islemlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikEvrakSiraNoVerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.parametrelerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.varsayilanDegerlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.akisParametreleriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kriterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kriterleriDuzenleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.aktarımParametreleriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.txtCsvAktarimParametreleriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sqlAktarimParametreleriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.backgroundWorker_Kontrol = new System.ComponentModel.BackgroundWorker();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.label_islem_adi = new System.Windows.Forms.Label();
		this.label_islem_bilgi = new System.Windows.Forms.Label();
		this.backgroundWorker_Aktarim = new System.ComponentModel.BackgroundWorker();
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		this.sb_TxtCsv_Al = new DevExpress.XtraEditors.SimpleButton();
		this.sb_Sql_Al = new DevExpress.XtraEditors.SimpleButton();
		this.cb_belgeno_kontrolu_yap = new System.Windows.Forms.CheckBox();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Cinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_SatirCinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_VergiPntr).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_IskontoSekli).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl_evraklar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Evrak_Kur).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_OtvSekli).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cardView1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.Name = "repositoryItemGridLookUpEdit_Satir_Cinsi";
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.View = this.gridView_SatirCinsi;
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(repositoryItemGridLookUpEdit_Satir_Cinsi_EditValueChanging);
		this.gridView_SatirCinsi.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_SatirCinsi.Name = "gridView_SatirCinsi";
		this.gridView_SatirCinsi.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_SatirCinsi.OptionsView.ShowGroupPanel = false;
		this.repositoryItemLookUpEdit_Satir_VergiPntr.AutoHeight = false;
		this.repositoryItemLookUpEdit_Satir_VergiPntr.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Satir_VergiPntr.Name = "repositoryItemLookUpEdit_Satir_VergiPntr";
		this.repositoryItemLookUpEdit_Satir_IskontoSekli.AutoHeight = false;
		this.repositoryItemLookUpEdit_Satir_IskontoSekli.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Satir_IskontoSekli.Name = "repositoryItemLookUpEdit_Satir_IskontoSekli";
		this.gridControl_evraklar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.gridControl_evraklar.Cursor = System.Windows.Forms.Cursors.Default;
		this.gridControl_evraklar.Location = new System.Drawing.Point(0, 56);
		this.gridControl_evraklar.MainView = this.advBandedGridView_master;
		this.gridControl_evraklar.Name = "gridControl_evraklar";
		this.gridControl_evraklar.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[13]
		{
			this.repositoryItemLookUpEdit_Satir_VergiPntr, this.repositoryItemLookUpEdit_Satir_IskontoSekli, this.repositoryItemGridLookUpEdit_Satir_Cinsi, this.repositoryItemGridLookUpEdit_Evrak_Tipi, this.repositoryItemGridLookUpEdit_NormalIade, this.repositoryItemDateEdit_EvrakTarih, this.repositoryItemLookUpEdit_Evrak_DovizCinsi, this.repositoryItemGridLookUpEdit_AcikKapali, this.repositoryItemTextEdit_Evrak_Kur, this.repositoryItemGridLookUpEdit_TicaretTuru,
			this.repositoryItemTextEdit_FaturaAciklama, this.repositoryItemTextEdit_Aciklamalar, this.repositoryItemLookUpEdit_Satir_OtvSekli
		});
		this.gridControl_evraklar.Size = new System.Drawing.Size(1206, 436);
		this.gridControl_evraklar.TabIndex = 1;
		this.gridControl_evraklar.UseEmbeddedNavigator = true;
		this.gridControl_evraklar.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[2] { this.advBandedGridView_master, this.cardView1 });
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
		this.advBandedGridView_master.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[7] { this.gridBand15, this.gridBand9, this.gridBand10, this.gridBand2, this.gridBand3, this.gridBand1, this.gridBand17 });
		this.advBandedGridView_master.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[93]
		{
			this.gc_evrak_KayitID, this.gc_evrak_Aktar, this.gc_evrak_AktarimDurumu, this.gc_firma_no, this.gc_sube_no, this.gc_evrak_EvrakTipi, this.gc_evrak_TicaretTuru, this.gc_evrak_NormalIade, this.gc_evrak_EvrakSeri, this.gc_evrak_EvrakSira,
			this.gc_evrak_Tarih, this.gc_evrak_BelgeNo, this.gc_evrak_BelgeTarihi, this.gc_evrak_CariKodu, this.gc_evrak_CariAdi, this.gc_evrak_SevkAdresi, this.gc_evrak_DovizCinsi, this.gc_evrak_Kur, this.gc_evrak_OdemePlani, this.gc_evrak_FiyatListesi,
			this.gc_evrak_DepoNo, this.gc_evrak_DepoAdi, this.gc_evrak_Hedef_DepoNo, this.gc_evrak_Hedef_DepoAdi, this.gc_evrak_SevkTeslimTarihi, this.gc_evrak_Plasiyer, this.gc_evrak_ProjeKodu, this.gc_evrak_ProjeAdi, this.gc_evrak_SorMerKodu, this.gc_evrak_SorMerAdi,
			this.gc_evrak_AcikKapali, this.gc_evrak_KapamaHesapKodu, this.gc_evrak_special1, this.gc_evrak_special2, this.gc_evrak_special3, this.gc_evrak_FaturaAciklama, this.gc_evrak_Aciklama1, this.gc_evrak_Aciklama2, this.gc_evrak_Aciklama3, this.gc_evrak_Aciklama4,
			this.gc_evrak_Aciklama5, this.gc_evrak_Aciklama6, this.gc_evrak_Aciklama7, this.gc_evrak_Aciklama8, this.gc_evrak_Aciklama9, this.gc_evrak_Aciklama10, this.gc_evrak_ara_toplam, this.gc_evrak_iskonto_toplam, this.gc_evrak_masraf_toplam, this.gc_evrak_otv_toplam,
			this.gc_evrak_kdv_toplam, this.gc_evrak_yekun, this.gc_evrak_miktar, this.gc_evrak_miktar2, this.gc_evrak_vergi_pntr, this.gc_evrak_birim_fiyat, this.gc_evrak_otv_sekli, this.gc_evrak_otv_yuzde, this.gc_evrak_otv_vergi_pntr, this.gc_satir_cinsi,
			this.gc_satir_HesapKodu, this.gc_satir_HesapAdi, this.gc_satir_parti_kodu, this.gc_satir_lot_no, this.gc_satir_iskonto1_uygulama, this.gc_satir_iskonto2_uygulama, this.gc_satir_iskonto3_uygulama, this.gc_satir_iskonto4_uygulama, this.gc_satir_iskonto5_uygulama, this.gc_satir_iskonto6_uygulama,
			this.gc_satir_iskonto1_yuzde_veya_miktar, this.gc_satir_iskonto2_yuzde_veya_miktar, this.gc_satir_iskonto3_yuzde_veya_miktar, this.gc_satir_iskonto4_yuzde_veya_miktar, this.gc_satir_iskonto5_yuzde_veya_miktar, this.gc_satir_iskonto6_yuzde_veya_miktar, this.gc_satir_fiyat_farki_mi, this.gc_satir_kriter_string1, this.gc_satir_kriter_string2, this.gc_satir_kriter_string3,
			this.gc_satir_kriter_string4, this.gc_satir_kriter_string5, this.gc_satir_kriter_double1, this.gc_satir_kriter_double2, this.gc_satir_kriter_double4, this.gc_satir_kriter_double5, this.gc_satir_kriter_bool1, this.gc_satir_kriter_bool2, this.gc_satir_kriter_bool3, this.gc_satir_kriter_bool4,
			this.gc_satir_kriter_bool5, this.gc_satir_kriter_double3, this.gc_satir_dbc_no
		});
		this.advBandedGridView_master.DetailVerticalIndent = 20;
		this.advBandedGridView_master.GridControl = this.gridControl_evraklar;
		this.advBandedGridView_master.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[8]
		{
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_ara_toplam", this.gc_evrak_ara_toplam, "{0:c2}"),
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_iskonto_toplam", this.gc_evrak_iskonto_toplam, "{0:c2}"),
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_masraf_toplam", this.gc_evrak_masraf_toplam, "{0:c2}"),
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_otv_toplam", this.gc_evrak_otv_toplam, "{0:c2}"),
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_kdv_toplam", this.gc_evrak_kdv_toplam, "{0:c2}"),
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_yekun", this.gc_evrak_yekun, "{0:c2}"),
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_miktar", this.gc_evrak_miktar, "{0}"),
			new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_miktar2", this.gc_evrak_miktar2, "{0}")
		});
		this.advBandedGridView_master.Name = "advBandedGridView_master";
		this.advBandedGridView_master.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsDetail.AllowExpandEmptyDetails = true;
		this.advBandedGridView_master.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView_master.OptionsSelection.MultiSelect = true;
		this.advBandedGridView_master.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
		this.advBandedGridView_master.OptionsView.ShowFooter = true;
		this.advBandedGridView_master.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(advBandedGridView_master_CustomDrawCell);
		this.advBandedGridView_master.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(advBandedGridView_master_CustomRowCellEdit);
		this.advBandedGridView_master.CustomDrawEmptyForeground += new DevExpress.XtraGrid.Views.Base.CustomDrawEventHandler(advBandedGridView_master_CustomDrawEmptyForeground);
		this.advBandedGridView_master.ShownEditor += new System.EventHandler(advBandedGridView_master_ShownEditor);
		this.advBandedGridView_master.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(advBandedGridView_master_CustomUnboundColumnData);
		this.advBandedGridView_master.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(advBandedGridView_master_CustomColumnDisplayText);
		this.gridBand15.AppearanceHeader.BackColor = System.Drawing.Color.FromArgb(255, 192, 192);
		this.gridBand15.AppearanceHeader.BackColor2 = System.Drawing.Color.Red;
		this.gridBand15.AppearanceHeader.BorderColor = System.Drawing.Color.Yellow;
		this.gridBand15.AppearanceHeader.Options.UseBackColor = true;
		this.gridBand15.AppearanceHeader.Options.UseBorderColor = true;
		this.gridBand15.Caption = "ID";
		this.gridBand15.Columns.Add(this.gc_evrak_KayitID);
		this.gridBand15.Name = "gridBand15";
		this.gridBand15.VisibleIndex = 0;
		this.gridBand15.Width = 45;
		this.gc_evrak_KayitID.Caption = "Kayıt ID";
		this.gc_evrak_KayitID.FieldName = "KayitID";
		this.gc_evrak_KayitID.Name = "gc_evrak_KayitID";
		this.gc_evrak_KayitID.OptionsColumn.AllowEdit = false;
		this.gc_evrak_KayitID.OptionsColumn.AllowFocus = false;
		this.gc_evrak_KayitID.Visible = true;
		this.gc_evrak_KayitID.Width = 45;
		this.gridBand9.Caption = "Aktarım";
		this.gridBand9.Columns.Add(this.gc_evrak_Aktar);
		this.gridBand9.Columns.Add(this.gc_evrak_AktarimDurumu);
		this.gridBand9.Name = "gridBand9";
		this.gridBand9.VisibleIndex = 1;
		this.gridBand9.Width = 150;
		this.gc_evrak_Aktar.Caption = "Aktar";
		this.gc_evrak_Aktar.FieldName = "Aktar";
		this.gc_evrak_Aktar.Name = "gc_evrak_Aktar";
		this.gc_evrak_Aktar.Visible = true;
		this.gc_evrak_Aktar.Width = 34;
		this.gc_evrak_AktarimDurumu.Caption = "Aktarım Durumu";
		this.gc_evrak_AktarimDurumu.FieldName = "gc_evrak_AktarimDurumu";
		this.gc_evrak_AktarimDurumu.Name = "gc_evrak_AktarimDurumu";
		this.gc_evrak_AktarimDurumu.OptionsColumn.AllowEdit = false;
		this.gc_evrak_AktarimDurumu.OptionsColumn.AllowFocus = false;
		this.gc_evrak_AktarimDurumu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_AktarimDurumu.Visible = true;
		this.gc_evrak_AktarimDurumu.Width = 116;
		this.gridBand10.Caption = "Evrak Genel";
		this.gridBand10.Columns.Add(this.gc_firma_no);
		this.gridBand10.Columns.Add(this.gc_sube_no);
		this.gridBand10.Columns.Add(this.gc_satir_dbc_no);
		this.gridBand10.Columns.Add(this.gc_evrak_EvrakTipi);
		this.gridBand10.Columns.Add(this.gc_evrak_NormalIade);
		this.gridBand10.Columns.Add(this.gc_evrak_TicaretTuru);
		this.gridBand10.Columns.Add(this.gc_evrak_EvrakSeri);
		this.gridBand10.Columns.Add(this.gc_evrak_EvrakSira);
		this.gridBand10.Columns.Add(this.gc_evrak_Tarih);
		this.gridBand10.Columns.Add(this.gc_evrak_BelgeNo);
		this.gridBand10.Columns.Add(this.gc_evrak_BelgeTarihi);
		this.gridBand10.Columns.Add(this.gc_evrak_CariKodu);
		this.gridBand10.Columns.Add(this.gc_evrak_CariAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_SevkAdresi);
		this.gridBand10.Columns.Add(this.gc_evrak_FiyatListesi);
		this.gridBand10.Columns.Add(this.gc_evrak_DovizCinsi);
		this.gridBand10.Columns.Add(this.gc_evrak_Kur);
		this.gridBand10.Columns.Add(this.gc_evrak_AcikKapali);
		this.gridBand10.Columns.Add(this.gc_evrak_KapamaHesapKodu);
		this.gridBand10.Columns.Add(this.gc_evrak_OdemePlani);
		this.gridBand10.Columns.Add(this.gc_evrak_DepoNo);
		this.gridBand10.Columns.Add(this.gc_evrak_DepoAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_Hedef_DepoNo);
		this.gridBand10.Columns.Add(this.gc_evrak_Hedef_DepoAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_ProjeKodu);
		this.gridBand10.Columns.Add(this.gc_evrak_ProjeAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_SorMerKodu);
		this.gridBand10.Columns.Add(this.gc_evrak_SorMerAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_Plasiyer);
		this.gridBand10.Columns.Add(this.gc_evrak_SevkTeslimTarihi);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama1);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama2);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama3);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama4);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama5);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama6);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama7);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama8);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama9);
		this.gridBand10.Columns.Add(this.gc_evrak_Aciklama10);
		this.gridBand10.Columns.Add(this.gc_evrak_special1);
		this.gridBand10.Columns.Add(this.gc_evrak_special2);
		this.gridBand10.Columns.Add(this.gc_evrak_special3);
		this.gridBand10.Name = "gridBand10";
		this.gridBand10.VisibleIndex = 2;
		this.gridBand10.Width = 3734;
		this.gc_firma_no.Caption = "Firma no";
		this.gc_firma_no.FieldName = "firma.fir_sirano";
		this.gc_firma_no.Name = "gc_firma_no";
		this.gc_firma_no.Visible = true;
		this.gc_firma_no.Width = 66;
		this.gc_sube_no.Caption = "Şube no";
		this.gc_sube_no.FieldName = "sube.Sube_no";
		this.gc_sube_no.Name = "gc_sube_no";
		this.gc_sube_no.Visible = true;
		this.gc_sube_no.Width = 58;
		this.gc_satir_dbc_no.Caption = "DBCno";
		this.gc_satir_dbc_no.FieldName = "DBCno";
		this.gc_satir_dbc_no.Name = "gc_satir_dbc_no";
		this.gc_satir_dbc_no.Visible = true;
		this.gc_evrak_EvrakTipi.Caption = "Tipi";
		this.gc_evrak_EvrakTipi.ColumnEdit = this.repositoryItemGridLookUpEdit_Evrak_Tipi;
		this.gc_evrak_EvrakTipi.FieldName = "gc_evrak_EvrakTipi";
		this.gc_evrak_EvrakTipi.Name = "gc_evrak_EvrakTipi";
		this.gc_evrak_EvrakTipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_EvrakTipi.Visible = true;
		this.gc_evrak_EvrakTipi.Width = 95;
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.Name = "repositoryItemGridLookUpEdit_Evrak_Tipi";
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.View = this.gridView_EvrakTipi;
		this.gridView_EvrakTipi.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_EvrakTipi.Name = "gridView_EvrakTipi";
		this.gridView_EvrakTipi.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_EvrakTipi.OptionsView.ShowGroupPanel = false;
		this.gc_evrak_NormalIade.Caption = "Normal/İade";
		this.gc_evrak_NormalIade.ColumnEdit = this.repositoryItemGridLookUpEdit_NormalIade;
		this.gc_evrak_NormalIade.FieldName = "gc_evrak_NormalIade";
		this.gc_evrak_NormalIade.Name = "gc_evrak_NormalIade";
		this.gc_evrak_NormalIade.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_NormalIade.Visible = true;
		this.gc_evrak_NormalIade.Width = 73;
		this.repositoryItemGridLookUpEdit_NormalIade.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_NormalIade.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_NormalIade.Name = "repositoryItemGridLookUpEdit_NormalIade";
		this.repositoryItemGridLookUpEdit_NormalIade.View = this.gridView_NormalIade;
		this.gridView_NormalIade.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_NormalIade.Name = "gridView_NormalIade";
		this.gridView_NormalIade.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_NormalIade.OptionsView.ShowGroupPanel = false;
		this.gc_evrak_TicaretTuru.Caption = "Ticaret türü";
		this.gc_evrak_TicaretTuru.ColumnEdit = this.repositoryItemGridLookUpEdit_TicaretTuru;
		this.gc_evrak_TicaretTuru.FieldName = "gc_evrak_TicaretTuru";
		this.gc_evrak_TicaretTuru.Name = "gc_evrak_TicaretTuru";
		this.gc_evrak_TicaretTuru.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_TicaretTuru.Visible = true;
		this.gc_evrak_TicaretTuru.Width = 109;
		this.repositoryItemGridLookUpEdit_TicaretTuru.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_TicaretTuru.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_TicaretTuru.Name = "repositoryItemGridLookUpEdit_TicaretTuru";
		this.repositoryItemGridLookUpEdit_TicaretTuru.View = this.gridView_TicaretTuru;
		this.gridView_TicaretTuru.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_TicaretTuru.Name = "gridView_TicaretTuru";
		this.gridView_TicaretTuru.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_TicaretTuru.OptionsView.ShowGroupPanel = false;
		this.gc_evrak_EvrakSeri.Caption = "Evrak Seri";
		this.gc_evrak_EvrakSeri.FieldName = "evraknoseri";
		this.gc_evrak_EvrakSeri.Name = "gc_evrak_EvrakSeri";
		this.gc_evrak_EvrakSeri.Visible = true;
		this.gc_evrak_EvrakSeri.Width = 56;
		this.gc_evrak_EvrakSira.Caption = "Evrak Sıra";
		this.gc_evrak_EvrakSira.FieldName = "evraknosira";
		this.gc_evrak_EvrakSira.Name = "gc_evrak_EvrakSira";
		this.gc_evrak_EvrakSira.Visible = true;
		this.gc_evrak_EvrakSira.Width = 63;
		this.gc_evrak_Tarih.Caption = "Evrak Tarihi";
		this.gc_evrak_Tarih.ColumnEdit = this.repositoryItemDateEdit_EvrakTarih;
		this.gc_evrak_Tarih.FieldName = "evraktarih";
		this.gc_evrak_Tarih.Name = "gc_evrak_Tarih";
		this.gc_evrak_Tarih.Visible = true;
		this.gc_evrak_Tarih.Width = 87;
		this.repositoryItemDateEdit_EvrakTarih.AutoHeight = false;
		this.repositoryItemDateEdit_EvrakTarih.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.repositoryItemDateEdit_EvrakTarih.Name = "repositoryItemDateEdit_EvrakTarih";
		this.repositoryItemDateEdit_EvrakTarih.EditValueChanged += new System.EventHandler(repositoryItemDateEdit_EvrakTarih_EditValueChanged);
		this.gc_evrak_BelgeNo.Caption = "Belge No";
		this.gc_evrak_BelgeNo.FieldName = "belgeno";
		this.gc_evrak_BelgeNo.Name = "gc_evrak_BelgeNo";
		this.gc_evrak_BelgeNo.Visible = true;
		this.gc_evrak_BelgeNo.Width = 73;
		this.gc_evrak_BelgeTarihi.Caption = "Belge Tarihi";
		this.gc_evrak_BelgeTarihi.FieldName = "belgetarih";
		this.gc_evrak_BelgeTarihi.Name = "gc_evrak_BelgeTarihi";
		this.gc_evrak_BelgeTarihi.Visible = true;
		this.gc_evrak_BelgeTarihi.Width = 82;
		this.gc_evrak_CariKodu.Caption = "Cari Kodu";
		this.gc_evrak_CariKodu.FieldName = "gc_evrak_Cari";
		this.gc_evrak_CariKodu.Name = "gc_evrak_CariKodu";
		this.gc_evrak_CariKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_CariKodu.Visible = true;
		this.gc_evrak_CariKodu.Width = 70;
		this.gc_evrak_CariAdi.Caption = "Cari Adı";
		this.gc_evrak_CariAdi.FieldName = "gc_evrak_Cari";
		this.gc_evrak_CariAdi.Name = "gc_evrak_CariAdi";
		this.gc_evrak_CariAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_CariAdi.Visible = true;
		this.gc_evrak_CariAdi.Width = 145;
		this.gc_evrak_SevkAdresi.Caption = "Sevk Adresi";
		this.gc_evrak_SevkAdresi.FieldName = "sevkadresno";
		this.gc_evrak_SevkAdresi.Name = "gc_evrak_SevkAdresi";
		this.gc_evrak_SevkAdresi.Visible = true;
		this.gc_evrak_SevkAdresi.Width = 84;
		this.gc_evrak_FiyatListesi.Caption = "Fiyat Listesi";
		this.gc_evrak_FiyatListesi.FieldName = "gc_evrak_FiyatListesi";
		this.gc_evrak_FiyatListesi.Name = "gc_evrak_FiyatListesi";
		this.gc_evrak_FiyatListesi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_FiyatListesi.Visible = true;
		this.gc_evrak_FiyatListesi.Width = 93;
		this.gc_evrak_DovizCinsi.Caption = "Döviz Cinsi";
		this.gc_evrak_DovizCinsi.ColumnEdit = this.repositoryItemLookUpEdit_Evrak_DovizCinsi;
		this.gc_evrak_DovizCinsi.FieldName = "gc_evrak_DovizCinsi";
		this.gc_evrak_DovizCinsi.Name = "gc_evrak_DovizCinsi";
		this.gc_evrak_DovizCinsi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_DovizCinsi.Visible = true;
		this.gc_evrak_DovizCinsi.Width = 85;
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.AutoHeight = false;
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.Name = "repositoryItemLookUpEdit_Evrak_DovizCinsi";
		this.gc_evrak_Kur.Caption = "Kur";
		this.gc_evrak_Kur.ColumnEdit = this.repositoryItemTextEdit_Evrak_Kur;
		this.gc_evrak_Kur.FieldName = "gc_evrak_Kur";
		this.gc_evrak_Kur.Name = "gc_evrak_Kur";
		this.gc_evrak_Kur.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_Kur.Visible = true;
		this.gc_evrak_Kur.Width = 57;
		this.repositoryItemTextEdit_Evrak_Kur.AutoHeight = false;
		this.repositoryItemTextEdit_Evrak_Kur.Name = "repositoryItemTextEdit_Evrak_Kur";
		this.repositoryItemTextEdit_Evrak_Kur.ValidateOnEnterKey = true;
		this.repositoryItemTextEdit_Evrak_Kur.Validating += new System.ComponentModel.CancelEventHandler(repositoryItemTextEdit_Evrak_Kur_Validating);
		this.gc_evrak_AcikKapali.Caption = "Açık/Kapalı";
		this.gc_evrak_AcikKapali.ColumnEdit = this.repositoryItemGridLookUpEdit_AcikKapali;
		this.gc_evrak_AcikKapali.FieldName = "gc_evrak_AcikKapali";
		this.gc_evrak_AcikKapali.Name = "gc_evrak_AcikKapali";
		this.gc_evrak_AcikKapali.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_AcikKapali.Visible = true;
		this.gc_evrak_AcikKapali.Width = 76;
		this.repositoryItemGridLookUpEdit_AcikKapali.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_AcikKapali.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_AcikKapali.Name = "repositoryItemGridLookUpEdit_AcikKapali";
		this.repositoryItemGridLookUpEdit_AcikKapali.View = this.repositoryItemGridLookUpEdit1View;
		this.repositoryItemGridLookUpEdit_AcikKapali.EditValueChanged += new System.EventHandler(repositoryItemGridLookUpEdit_AcikKapali_EditValueChanged);
		this.repositoryItemGridLookUpEdit1View.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.repositoryItemGridLookUpEdit1View.Name = "repositoryItemGridLookUpEdit1View";
		this.repositoryItemGridLookUpEdit1View.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.repositoryItemGridLookUpEdit1View.OptionsView.ShowGroupPanel = false;
		this.gc_evrak_KapamaHesapKodu.Caption = "Kapama Hesap Kodu";
		this.gc_evrak_KapamaHesapKodu.FieldName = "gc_evrak_KapamaHesapKodu";
		this.gc_evrak_KapamaHesapKodu.Name = "gc_evrak_KapamaHesapKodu";
		this.gc_evrak_KapamaHesapKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_KapamaHesapKodu.Visible = true;
		this.gc_evrak_KapamaHesapKodu.Width = 136;
		this.gc_evrak_OdemePlani.Caption = "Ödeme Planı";
		this.gc_evrak_OdemePlani.FieldName = "gc_evrak_OdemePlani";
		this.gc_evrak_OdemePlani.Name = "gc_evrak_OdemePlani";
		this.gc_evrak_OdemePlani.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_OdemePlani.Visible = true;
		this.gc_evrak_OdemePlani.Width = 111;
		this.gc_evrak_DepoNo.Caption = "Kaynak depo no";
		this.gc_evrak_DepoNo.FieldName = "gc_evrak_Depo";
		this.gc_evrak_DepoNo.Name = "gc_evrak_DepoNo";
		this.gc_evrak_DepoNo.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_DepoNo.Visible = true;
		this.gc_evrak_DepoNo.Width = 89;
		this.gc_evrak_DepoAdi.Caption = "Kaynak depo adı";
		this.gc_evrak_DepoAdi.FieldName = "gc_evrak_Depo";
		this.gc_evrak_DepoAdi.Name = "gc_evrak_DepoAdi";
		this.gc_evrak_DepoAdi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_DepoAdi.Visible = true;
		this.gc_evrak_DepoAdi.Width = 119;
		this.gc_evrak_Hedef_DepoNo.Caption = "Hedef depo no";
		this.gc_evrak_Hedef_DepoNo.FieldName = "gc_evrak_Hedef_Depo";
		this.gc_evrak_Hedef_DepoNo.Name = "gc_evrak_Hedef_DepoNo";
		this.gc_evrak_Hedef_DepoNo.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_Hedef_DepoNo.Visible = true;
		this.gc_evrak_Hedef_DepoNo.Width = 89;
		this.gc_evrak_Hedef_DepoAdi.Caption = "Hedef depo adı";
		this.gc_evrak_Hedef_DepoAdi.FieldName = "gc_evrak_Hedef_Depo";
		this.gc_evrak_Hedef_DepoAdi.Name = "gc_evrak_Hedef_DepoAdi";
		this.gc_evrak_Hedef_DepoAdi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_Hedef_DepoAdi.Visible = true;
		this.gc_evrak_Hedef_DepoAdi.Width = 119;
		this.gc_evrak_ProjeKodu.Caption = "Proje kodu";
		this.gc_evrak_ProjeKodu.FieldName = "gc_evrak_Proje";
		this.gc_evrak_ProjeKodu.Name = "gc_evrak_ProjeKodu";
		this.gc_evrak_ProjeKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_ProjeKodu.Visible = true;
		this.gc_evrak_ProjeKodu.Width = 69;
		this.gc_evrak_ProjeAdi.Caption = "Proje Adı";
		this.gc_evrak_ProjeAdi.FieldName = "gc_evrak_Proje";
		this.gc_evrak_ProjeAdi.Name = "gc_evrak_ProjeAdi";
		this.gc_evrak_ProjeAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_ProjeAdi.Visible = true;
		this.gc_evrak_ProjeAdi.Width = 129;
		this.gc_evrak_SorMerKodu.Caption = "Sor. Mer. Kodu";
		this.gc_evrak_SorMerKodu.FieldName = "gc_evrak_SorMer";
		this.gc_evrak_SorMerKodu.Name = "gc_evrak_SorMerKodu";
		this.gc_evrak_SorMerKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_SorMerKodu.Visible = true;
		this.gc_evrak_SorMerKodu.Width = 91;
		this.gc_evrak_SorMerAdi.Caption = "Sor. Mer. Adı";
		this.gc_evrak_SorMerAdi.FieldName = "gc_evrak_SorMer";
		this.gc_evrak_SorMerAdi.Name = "gc_evrak_SorMerAdi";
		this.gc_evrak_SorMerAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_SorMerAdi.Visible = true;
		this.gc_evrak_SorMerAdi.Width = 133;
		this.gc_evrak_Plasiyer.Caption = "Plasiyer";
		this.gc_evrak_Plasiyer.FieldName = "temsilcikodu";
		this.gc_evrak_Plasiyer.Name = "gc_evrak_Plasiyer";
		this.gc_evrak_Plasiyer.Visible = true;
		this.gc_evrak_Plasiyer.Width = 90;
		this.gc_evrak_SevkTeslimTarihi.Caption = "Sevk/Teslim Tarihi";
		this.gc_evrak_SevkTeslimTarihi.FieldName = "sevkteslimtarihi";
		this.gc_evrak_SevkTeslimTarihi.Name = "gc_evrak_SevkTeslimTarihi";
		this.gc_evrak_SevkTeslimTarihi.Visible = true;
		this.gc_evrak_SevkTeslimTarihi.Width = 93;
		this.gc_evrak_Aciklama1.Caption = "Açıklama 1";
		this.gc_evrak_Aciklama1.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama1.FieldName = "aciklama1";
		this.gc_evrak_Aciklama1.Name = "gc_evrak_Aciklama1";
		this.gc_evrak_Aciklama1.Visible = true;
		this.gc_evrak_Aciklama1.Width = 74;
		this.repositoryItemTextEdit_Aciklamalar.AutoHeight = false;
		this.repositoryItemTextEdit_Aciklamalar.MaxLength = 127;
		this.repositoryItemTextEdit_Aciklamalar.Name = "repositoryItemTextEdit_Aciklamalar";
		this.gc_evrak_Aciklama2.Caption = "Açıklama 2";
		this.gc_evrak_Aciklama2.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama2.FieldName = "aciklama2";
		this.gc_evrak_Aciklama2.Name = "gc_evrak_Aciklama2";
		this.gc_evrak_Aciklama2.Visible = true;
		this.gc_evrak_Aciklama2.Width = 74;
		this.gc_evrak_Aciklama3.Caption = "Açıklama 3";
		this.gc_evrak_Aciklama3.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama3.FieldName = "aciklama3";
		this.gc_evrak_Aciklama3.Name = "gc_evrak_Aciklama3";
		this.gc_evrak_Aciklama3.Visible = true;
		this.gc_evrak_Aciklama3.Width = 81;
		this.gc_evrak_Aciklama4.Caption = "Açıklama 4";
		this.gc_evrak_Aciklama4.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama4.FieldName = "aciklama4";
		this.gc_evrak_Aciklama4.Name = "gc_evrak_Aciklama4";
		this.gc_evrak_Aciklama4.Visible = true;
		this.gc_evrak_Aciklama4.Width = 69;
		this.gc_evrak_Aciklama5.Caption = "Açıklama 5";
		this.gc_evrak_Aciklama5.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama5.FieldName = "aciklama5";
		this.gc_evrak_Aciklama5.Name = "gc_evrak_Aciklama5";
		this.gc_evrak_Aciklama5.Visible = true;
		this.gc_evrak_Aciklama5.Width = 69;
		this.gc_evrak_Aciklama6.Caption = "Açıklama 6";
		this.gc_evrak_Aciklama6.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama6.FieldName = "aciklama6";
		this.gc_evrak_Aciklama6.Name = "gc_evrak_Aciklama6";
		this.gc_evrak_Aciklama6.Visible = true;
		this.gc_evrak_Aciklama6.Width = 79;
		this.gc_evrak_Aciklama7.Caption = "Açıklama 7";
		this.gc_evrak_Aciklama7.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama7.FieldName = "aciklama7";
		this.gc_evrak_Aciklama7.Name = "gc_evrak_Aciklama7";
		this.gc_evrak_Aciklama7.Visible = true;
		this.gc_evrak_Aciklama7.Width = 86;
		this.gc_evrak_Aciklama8.Caption = "Açıklama 8";
		this.gc_evrak_Aciklama8.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama8.FieldName = "aciklama8";
		this.gc_evrak_Aciklama8.Name = "gc_evrak_Aciklama8";
		this.gc_evrak_Aciklama8.Visible = true;
		this.gc_evrak_Aciklama8.Width = 93;
		this.gc_evrak_Aciklama9.Caption = "Açıklama 9";
		this.gc_evrak_Aciklama9.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama9.FieldName = "aciklama9";
		this.gc_evrak_Aciklama9.Name = "gc_evrak_Aciklama9";
		this.gc_evrak_Aciklama9.Visible = true;
		this.gc_evrak_Aciklama9.Width = 93;
		this.gc_evrak_Aciklama10.Caption = "Açıklama 10";
		this.gc_evrak_Aciklama10.ColumnEdit = this.repositoryItemTextEdit_Aciklamalar;
		this.gc_evrak_Aciklama10.FieldName = "aciklama10";
		this.gc_evrak_Aciklama10.Name = "gc_evrak_Aciklama10";
		this.gc_evrak_Aciklama10.Visible = true;
		this.gc_evrak_Aciklama10.Width = 94;
		this.gc_evrak_special1.Caption = "Özel alan 1";
		this.gc_evrak_special1.FieldName = "degistirspecialalan1";
		this.gc_evrak_special1.Name = "gc_evrak_special1";
		this.gc_evrak_special1.Visible = true;
		this.gc_evrak_special1.Width = 78;
		this.gc_evrak_special2.Caption = "Özel alan 2";
		this.gc_evrak_special2.FieldName = "degistirspecialalan2";
		this.gc_evrak_special2.Name = "gc_evrak_special2";
		this.gc_evrak_special2.Visible = true;
		this.gc_evrak_special2.Width = 78;
		this.gc_evrak_special3.Caption = "Özel alan 3";
		this.gc_evrak_special3.FieldName = "degistirspecialalan3";
		this.gc_evrak_special3.Name = "gc_evrak_special3";
		this.gc_evrak_special3.Visible = true;
		this.gc_evrak_special3.Width = 51;
		this.gridBand2.Caption = "Satır Detayları";
		this.gridBand2.Columns.Add(this.gc_satir_cinsi);
		this.gridBand2.Columns.Add(this.gc_satir_fiyat_farki_mi);
		this.gridBand2.Columns.Add(this.gc_satir_HesapKodu);
		this.gridBand2.Columns.Add(this.gc_satir_HesapAdi);
		this.gridBand2.Columns.Add(this.gc_satir_parti_kodu);
		this.gridBand2.Columns.Add(this.gc_satir_lot_no);
		this.gridBand2.Columns.Add(this.gc_evrak_birim_fiyat);
		this.gridBand2.Columns.Add(this.gc_evrak_miktar);
		this.gridBand2.Columns.Add(this.gc_evrak_miktar2);
		this.gridBand2.Columns.Add(this.gc_evrak_vergi_pntr);
		this.gridBand2.Columns.Add(this.gc_evrak_otv_sekli);
		this.gridBand2.Columns.Add(this.gc_evrak_otv_yuzde);
		this.gridBand2.Columns.Add(this.gc_evrak_otv_vergi_pntr);
		this.gridBand2.Columns.Add(this.gc_evrak_FaturaAciklama);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.VisibleIndex = 3;
		this.gridBand2.Width = 1156;
		this.gc_satir_cinsi.Caption = "Cinsi";
		this.gc_satir_cinsi.ColumnEdit = this.repositoryItemGridLookUpEdit_Satir_Cinsi;
		this.gc_satir_cinsi.FieldName = "gc_satir_cinsi";
		this.gc_satir_cinsi.Name = "gc_satir_cinsi";
		this.gc_satir_cinsi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_cinsi.Visible = true;
		this.gc_satir_cinsi.Width = 64;
		this.gc_satir_fiyat_farki_mi.Caption = "Fiyat farkı mı";
		this.gc_satir_fiyat_farki_mi.FieldName = "fiyat_fark_mi";
		this.gc_satir_fiyat_farki_mi.Name = "gc_satir_fiyat_farki_mi";
		this.gc_satir_fiyat_farki_mi.Visible = true;
		this.gc_satir_HesapKodu.Caption = "Hesap kodu";
		this.gc_satir_HesapKodu.FieldName = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.Name = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapKodu.Visible = true;
		this.gc_satir_HesapKodu.Width = 93;
		this.gc_satir_HesapAdi.Caption = "Hesap adı";
		this.gc_satir_HesapAdi.FieldName = "gc_satir_HesapKodu";
		this.gc_satir_HesapAdi.Name = "gc_satir_HesapAdi";
		this.gc_satir_HesapAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapAdi.Visible = true;
		this.gc_satir_HesapAdi.Width = 219;
		this.gc_satir_parti_kodu.Caption = "Parti kodu";
		this.gc_satir_parti_kodu.FieldName = "stokpartikodu";
		this.gc_satir_parti_kodu.Name = "gc_satir_parti_kodu";
		this.gc_satir_parti_kodu.Visible = true;
		this.gc_satir_lot_no.Caption = "Lot no";
		this.gc_satir_lot_no.FieldName = "stoklotno";
		this.gc_satir_lot_no.Name = "gc_satir_lot_no";
		this.gc_satir_lot_no.Visible = true;
		this.gc_evrak_birim_fiyat.Caption = "Birim fiyat";
		this.gc_evrak_birim_fiyat.FieldName = "birimfiyat.FiyatBrut";
		this.gc_evrak_birim_fiyat.Name = "gc_evrak_birim_fiyat";
		this.gc_evrak_birim_fiyat.Visible = true;
		this.gc_evrak_birim_fiyat.Width = 62;
		this.gc_evrak_miktar.Caption = "Miktar";
		this.gc_evrak_miktar.FieldName = "miktar";
		this.gc_evrak_miktar.Name = "gc_evrak_miktar";
		this.gc_evrak_miktar.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "miktar", "{0}")
		});
		this.gc_evrak_miktar.Visible = true;
		this.gc_evrak_miktar.Width = 46;
		this.gc_evrak_miktar2.Caption = "Miktar 2";
		this.gc_evrak_miktar2.FieldName = "miktar2";
		this.gc_evrak_miktar2.Name = "gc_evrak_miktar2";
		this.gc_evrak_miktar2.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "miktar2", "{0}")
		});
		this.gc_evrak_miktar2.Visible = true;
		this.gc_evrak_miktar2.Width = 63;
		this.gc_evrak_vergi_pntr.Caption = "Vergi";
		this.gc_evrak_vergi_pntr.ColumnEdit = this.repositoryItemLookUpEdit_Satir_VergiPntr;
		this.gc_evrak_vergi_pntr.FieldName = "vergi_pntr";
		this.gc_evrak_vergi_pntr.Name = "gc_evrak_vergi_pntr";
		this.gc_evrak_vergi_pntr.Visible = true;
		this.gc_evrak_vergi_pntr.Width = 64;
		this.gc_evrak_otv_sekli.Caption = "ÖTV şekli";
		this.gc_evrak_otv_sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_OtvSekli;
		this.gc_evrak_otv_sekli.FieldName = "gc_evrak_otv_sekli";
		this.gc_evrak_otv_sekli.Name = "gc_evrak_otv_sekli";
		this.gc_evrak_otv_sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_otv_sekli.Visible = true;
		this.gc_evrak_otv_sekli.Width = 56;
		this.repositoryItemLookUpEdit_Satir_OtvSekli.AutoHeight = false;
		this.repositoryItemLookUpEdit_Satir_OtvSekli.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Satir_OtvSekli.Name = "repositoryItemLookUpEdit_Satir_OtvSekli";
		this.gc_evrak_otv_yuzde.Caption = "ÖTV değer";
		this.gc_evrak_otv_yuzde.FieldName = "birimfiyat.OtvYuzdeVeyaTutar";
		this.gc_evrak_otv_yuzde.Name = "gc_evrak_otv_yuzde";
		this.gc_evrak_otv_yuzde.Visible = true;
		this.gc_evrak_otv_yuzde.Width = 70;
		this.gc_evrak_otv_vergi_pntr.Caption = "ÖTV vergisi";
		this.gc_evrak_otv_vergi_pntr.ColumnEdit = this.repositoryItemLookUpEdit_Satir_VergiPntr;
		this.gc_evrak_otv_vergi_pntr.FieldName = "birimfiyat.OtvVergiPntr";
		this.gc_evrak_otv_vergi_pntr.Name = "gc_evrak_otv_vergi_pntr";
		this.gc_evrak_otv_vergi_pntr.Visible = true;
		this.gc_evrak_FaturaAciklama.Caption = "Satır Açıklama";
		this.gc_evrak_FaturaAciklama.ColumnEdit = this.repositoryItemTextEdit_FaturaAciklama;
		this.gc_evrak_FaturaAciklama.FieldName = "faturaaciklama";
		this.gc_evrak_FaturaAciklama.Name = "gc_evrak_FaturaAciklama";
		this.gc_evrak_FaturaAciklama.Visible = true;
		this.gc_evrak_FaturaAciklama.Width = 119;
		this.repositoryItemTextEdit_FaturaAciklama.AutoHeight = false;
		this.repositoryItemTextEdit_FaturaAciklama.MaxLength = 40;
		this.repositoryItemTextEdit_FaturaAciklama.Name = "repositoryItemTextEdit_FaturaAciklama";
		this.gridBand3.Caption = "Satır İskonto";
		this.gridBand3.Columns.Add(this.gc_satir_iskonto1_uygulama);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto1_yuzde_veya_miktar);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto2_uygulama);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto2_yuzde_veya_miktar);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto3_uygulama);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto3_yuzde_veya_miktar);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto4_uygulama);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto4_yuzde_veya_miktar);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto5_uygulama);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto5_yuzde_veya_miktar);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto6_uygulama);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto6_yuzde_veya_miktar);
		this.gridBand3.Name = "gridBand3";
		this.gridBand3.VisibleIndex = 4;
		this.gridBand3.Width = 900;
		this.gc_satir_iskonto1_uygulama.Caption = "1. Uyg. Şek.";
		this.gc_satir_iskonto1_uygulama.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_iskonto1_uygulama.FieldName = "birimfiyat.Iskonto_1_UygulamaSekli";
		this.gc_satir_iskonto1_uygulama.Name = "gc_satir_iskonto1_uygulama";
		this.gc_satir_iskonto1_uygulama.Visible = true;
		this.gc_satir_iskonto1_uygulama.Width = 73;
		this.gc_satir_iskonto1_yuzde_veya_miktar.Caption = "1. Değer";
		this.gc_satir_iskonto1_yuzde_veya_miktar.FieldName = "birimfiyat.Iskonto_1_YuzdeVeyaMiktar";
		this.gc_satir_iskonto1_yuzde_veya_miktar.Name = "gc_satir_iskonto1_yuzde_veya_miktar";
		this.gc_satir_iskonto1_yuzde_veya_miktar.Visible = true;
		this.gc_satir_iskonto1_yuzde_veya_miktar.Width = 77;
		this.gc_satir_iskonto2_uygulama.Caption = "2. Uyg. Şek.";
		this.gc_satir_iskonto2_uygulama.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_iskonto2_uygulama.FieldName = "birimfiyat.Iskonto_2_UygulamaSekli";
		this.gc_satir_iskonto2_uygulama.Name = "gc_satir_iskonto2_uygulama";
		this.gc_satir_iskonto2_uygulama.Visible = true;
		this.gc_satir_iskonto2_yuzde_veya_miktar.Caption = "2. Değer";
		this.gc_satir_iskonto2_yuzde_veya_miktar.FieldName = "birimfiyat.Iskonto_2_YuzdeVeyaMiktar";
		this.gc_satir_iskonto2_yuzde_veya_miktar.Name = "gc_satir_iskonto2_yuzde_veya_miktar";
		this.gc_satir_iskonto2_yuzde_veya_miktar.Visible = true;
		this.gc_satir_iskonto3_uygulama.Caption = "3. Uyg. Şek.";
		this.gc_satir_iskonto3_uygulama.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_iskonto3_uygulama.FieldName = "birimfiyat.Iskonto_3_UygulamaSekli";
		this.gc_satir_iskonto3_uygulama.Name = "gc_satir_iskonto3_uygulama";
		this.gc_satir_iskonto3_uygulama.Visible = true;
		this.gc_satir_iskonto3_yuzde_veya_miktar.Caption = "3. Değer";
		this.gc_satir_iskonto3_yuzde_veya_miktar.FieldName = "birimfiyat.Iskonto_3_YuzdeVeyaMiktar";
		this.gc_satir_iskonto3_yuzde_veya_miktar.Name = "gc_satir_iskonto3_yuzde_veya_miktar";
		this.gc_satir_iskonto3_yuzde_veya_miktar.Visible = true;
		this.gc_satir_iskonto4_uygulama.Caption = "4. Uyg. Şek.";
		this.gc_satir_iskonto4_uygulama.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_iskonto4_uygulama.FieldName = "birimfiyat.Iskonto_4_UygulamaSekli";
		this.gc_satir_iskonto4_uygulama.Name = "gc_satir_iskonto4_uygulama";
		this.gc_satir_iskonto4_uygulama.Visible = true;
		this.gc_satir_iskonto4_yuzde_veya_miktar.Caption = "4. Değer";
		this.gc_satir_iskonto4_yuzde_veya_miktar.FieldName = "birimfiyat.Iskonto_4_YuzdeVeyaMiktar";
		this.gc_satir_iskonto4_yuzde_veya_miktar.Name = "gc_satir_iskonto4_yuzde_veya_miktar";
		this.gc_satir_iskonto4_yuzde_veya_miktar.Visible = true;
		this.gc_satir_iskonto5_uygulama.Caption = "5. Uyg. Şek.";
		this.gc_satir_iskonto5_uygulama.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_iskonto5_uygulama.FieldName = "birimfiyat.Iskonto_5_UygulamaSekli";
		this.gc_satir_iskonto5_uygulama.Name = "gc_satir_iskonto5_uygulama";
		this.gc_satir_iskonto5_uygulama.Visible = true;
		this.gc_satir_iskonto5_yuzde_veya_miktar.Caption = "5. Değer";
		this.gc_satir_iskonto5_yuzde_veya_miktar.FieldName = "birimfiyat.Iskonto_5_YuzdeVeyaMiktar";
		this.gc_satir_iskonto5_yuzde_veya_miktar.Name = "gc_satir_iskonto5_yuzde_veya_miktar";
		this.gc_satir_iskonto5_yuzde_veya_miktar.Visible = true;
		this.gc_satir_iskonto6_uygulama.Caption = "6. Uyg. Şek.";
		this.gc_satir_iskonto6_uygulama.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_iskonto6_uygulama.FieldName = "birimfiyat.Iskonto_6_UygulamaSekli";
		this.gc_satir_iskonto6_uygulama.Name = "gc_satir_iskonto6_uygulama";
		this.gc_satir_iskonto6_uygulama.Visible = true;
		this.gc_satir_iskonto6_yuzde_veya_miktar.Caption = "6. Değer";
		this.gc_satir_iskonto6_yuzde_veya_miktar.FieldName = "birimfiyat.Iskonto_6_YuzdeVeyaMiktar";
		this.gc_satir_iskonto6_yuzde_veya_miktar.Name = "gc_satir_iskonto6_yuzde_veya_miktar";
		this.gc_satir_iskonto6_yuzde_veya_miktar.Visible = true;
		this.gridBand1.Caption = "Kriter verileri";
		this.gridBand1.Columns.Add(this.gc_satir_kriter_string1);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_string2);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_string3);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_string4);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_string5);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_double1);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_double2);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_double3);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_double4);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_double5);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_bool1);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_bool2);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_bool3);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_bool4);
		this.gridBand1.Columns.Add(this.gc_satir_kriter_bool5);
		this.gridBand1.Name = "gridBand1";
		this.gridBand1.Visible = false;
		this.gridBand1.VisibleIndex = -1;
		this.gridBand1.Width = 1326;
		this.gc_satir_kriter_string1.Caption = "Kriter metin 1";
		this.gc_satir_kriter_string1.FieldName = "kriter_string1";
		this.gc_satir_kriter_string1.Name = "gc_satir_kriter_string1";
		this.gc_satir_kriter_string1.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_string1.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_string1.Visible = true;
		this.gc_satir_kriter_string1.Width = 90;
		this.gc_satir_kriter_string2.Caption = "Kriter metin 2";
		this.gc_satir_kriter_string2.FieldName = "kriter_string2";
		this.gc_satir_kriter_string2.Name = "gc_satir_kriter_string2";
		this.gc_satir_kriter_string2.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_string2.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_string2.Visible = true;
		this.gc_satir_kriter_string2.Width = 90;
		this.gc_satir_kriter_string3.Caption = "Kriter metin 3";
		this.gc_satir_kriter_string3.FieldName = "kriter_string3";
		this.gc_satir_kriter_string3.Name = "gc_satir_kriter_string3";
		this.gc_satir_kriter_string3.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_string3.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_string3.Visible = true;
		this.gc_satir_kriter_string3.Width = 90;
		this.gc_satir_kriter_string4.Caption = "Kriter metin 4";
		this.gc_satir_kriter_string4.FieldName = "kriter_string4";
		this.gc_satir_kriter_string4.Name = "gc_satir_kriter_string4";
		this.gc_satir_kriter_string4.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_string4.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_string4.Visible = true;
		this.gc_satir_kriter_string4.Width = 90;
		this.gc_satir_kriter_string5.Caption = "Kriter metin 5";
		this.gc_satir_kriter_string5.FieldName = "kriter_string5";
		this.gc_satir_kriter_string5.Name = "gc_satir_kriter_string5";
		this.gc_satir_kriter_string5.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_string5.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_string5.Visible = true;
		this.gc_satir_kriter_string5.Width = 90;
		this.gc_satir_kriter_double1.Caption = "Kriter sayı 1";
		this.gc_satir_kriter_double1.FieldName = "kriter_double1";
		this.gc_satir_kriter_double1.Name = "gc_satir_kriter_double1";
		this.gc_satir_kriter_double1.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_double1.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_double1.Visible = true;
		this.gc_satir_kriter_double1.Width = 90;
		this.gc_satir_kriter_double2.Caption = "Kriter sayı 2";
		this.gc_satir_kriter_double2.FieldName = "kriter_double2";
		this.gc_satir_kriter_double2.Name = "gc_satir_kriter_double2";
		this.gc_satir_kriter_double2.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_double2.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_double2.Visible = true;
		this.gc_satir_kriter_double2.Width = 90;
		this.gc_satir_kriter_double3.Caption = "Kriter sayı 3";
		this.gc_satir_kriter_double3.FieldName = "kriter_double3";
		this.gc_satir_kriter_double3.Name = "gc_satir_kriter_double3";
		this.gc_satir_kriter_double3.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_double3.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_double3.Visible = true;
		this.gc_satir_kriter_double3.Width = 90;
		this.gc_satir_kriter_double4.Caption = "Kriter sayı 4";
		this.gc_satir_kriter_double4.FieldName = "kriter_double4";
		this.gc_satir_kriter_double4.Name = "gc_satir_kriter_double4";
		this.gc_satir_kriter_double4.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_double4.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_double4.Visible = true;
		this.gc_satir_kriter_double4.Width = 90;
		this.gc_satir_kriter_double5.Caption = "Kriter sayı 5";
		this.gc_satir_kriter_double5.FieldName = "kriter_double5";
		this.gc_satir_kriter_double5.Name = "gc_satir_kriter_double5";
		this.gc_satir_kriter_double5.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_double5.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_double5.Visible = true;
		this.gc_satir_kriter_double5.Width = 90;
		this.gc_satir_kriter_bool1.Caption = "Kriter evet/hayır 1";
		this.gc_satir_kriter_bool1.FieldName = "kriter_bool1";
		this.gc_satir_kriter_bool1.Name = "gc_satir_kriter_bool1";
		this.gc_satir_kriter_bool1.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_bool1.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_bool1.Visible = true;
		this.gc_satir_kriter_bool1.Width = 102;
		this.gc_satir_kriter_bool2.Caption = "Kriter evet/hayır 2";
		this.gc_satir_kriter_bool2.FieldName = "kriter_bool2";
		this.gc_satir_kriter_bool2.Name = "gc_satir_kriter_bool2";
		this.gc_satir_kriter_bool2.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_bool2.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_bool2.Visible = true;
		this.gc_satir_kriter_bool2.Width = 99;
		this.gc_satir_kriter_bool3.Caption = "Kriter evet/hayır 3";
		this.gc_satir_kriter_bool3.FieldName = "kriter_bool3";
		this.gc_satir_kriter_bool3.Name = "gc_satir_kriter_bool3";
		this.gc_satir_kriter_bool3.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_bool3.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_bool3.Visible = true;
		this.gc_satir_kriter_bool4.Caption = "Kriter evet/hayır 4";
		this.gc_satir_kriter_bool4.FieldName = "kriter_bool4";
		this.gc_satir_kriter_bool4.Name = "gc_satir_kriter_bool4";
		this.gc_satir_kriter_bool4.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_bool4.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_bool4.Visible = true;
		this.gc_satir_kriter_bool5.Caption = "Kriter evet/hayır 5";
		this.gc_satir_kriter_bool5.FieldName = "kriter_bool5";
		this.gc_satir_kriter_bool5.Name = "gc_satir_kriter_bool5";
		this.gc_satir_kriter_bool5.OptionsColumn.AllowEdit = false;
		this.gc_satir_kriter_bool5.OptionsColumn.AllowFocus = false;
		this.gc_satir_kriter_bool5.Visible = true;
		this.gridBand17.Caption = "Satır Toplamları";
		this.gridBand17.Columns.Add(this.gc_evrak_ara_toplam);
		this.gridBand17.Columns.Add(this.gc_evrak_iskonto_toplam);
		this.gridBand17.Columns.Add(this.gc_evrak_masraf_toplam);
		this.gridBand17.Columns.Add(this.gc_evrak_otv_toplam);
		this.gridBand17.Columns.Add(this.gc_evrak_kdv_toplam);
		this.gridBand17.Columns.Add(this.gc_evrak_yekun);
		this.gridBand17.Name = "gridBand17";
		this.gridBand17.VisibleIndex = 5;
		this.gridBand17.Width = 533;
		this.gc_evrak_ara_toplam.Caption = "Ara Toplam";
		this.gc_evrak_ara_toplam.FieldName = "gc_evrak_ara_toplam";
		this.gc_evrak_ara_toplam.Name = "gc_evrak_ara_toplam";
		this.gc_evrak_ara_toplam.OptionsColumn.AllowEdit = false;
		this.gc_evrak_ara_toplam.OptionsColumn.AllowFocus = false;
		this.gc_evrak_ara_toplam.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_ara_toplam", "{0:c2}")
		});
		this.gc_evrak_ara_toplam.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_ara_toplam.Visible = true;
		this.gc_evrak_ara_toplam.Width = 105;
		this.gc_evrak_iskonto_toplam.Caption = "İskonto";
		this.gc_evrak_iskonto_toplam.FieldName = "gc_evrak_iskonto_toplam";
		this.gc_evrak_iskonto_toplam.Name = "gc_evrak_iskonto_toplam";
		this.gc_evrak_iskonto_toplam.OptionsColumn.AllowEdit = false;
		this.gc_evrak_iskonto_toplam.OptionsColumn.AllowFocus = false;
		this.gc_evrak_iskonto_toplam.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_iskonto_toplam", "{0:c2}")
		});
		this.gc_evrak_iskonto_toplam.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_iskonto_toplam.Visible = true;
		this.gc_evrak_iskonto_toplam.Width = 74;
		this.gc_evrak_masraf_toplam.Caption = "Masraf";
		this.gc_evrak_masraf_toplam.FieldName = "gc_evrak_masraf_toplam";
		this.gc_evrak_masraf_toplam.Name = "gc_evrak_masraf_toplam";
		this.gc_evrak_masraf_toplam.OptionsColumn.AllowEdit = false;
		this.gc_evrak_masraf_toplam.OptionsColumn.AllowFocus = false;
		this.gc_evrak_masraf_toplam.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_masraf_toplam", "{0:c2}")
		});
		this.gc_evrak_masraf_toplam.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_masraf_toplam.Visible = true;
		this.gc_evrak_masraf_toplam.Width = 76;
		this.gc_evrak_otv_toplam.Caption = "Ötv";
		this.gc_evrak_otv_toplam.FieldName = "gc_evrak_otv_toplam";
		this.gc_evrak_otv_toplam.Name = "gc_evrak_otv_toplam";
		this.gc_evrak_otv_toplam.OptionsColumn.AllowEdit = false;
		this.gc_evrak_otv_toplam.OptionsColumn.AllowFocus = false;
		this.gc_evrak_otv_toplam.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_otv_toplam", "{0:c2}")
		});
		this.gc_evrak_otv_toplam.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_otv_toplam.Visible = true;
		this.gc_evrak_kdv_toplam.Caption = "Kdv";
		this.gc_evrak_kdv_toplam.FieldName = "gc_evrak_kdv_toplam";
		this.gc_evrak_kdv_toplam.Name = "gc_evrak_kdv_toplam";
		this.gc_evrak_kdv_toplam.OptionsColumn.AllowEdit = false;
		this.gc_evrak_kdv_toplam.OptionsColumn.AllowFocus = false;
		this.gc_evrak_kdv_toplam.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_kdv_toplam", "{0:c2}")
		});
		this.gc_evrak_kdv_toplam.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_kdv_toplam.Visible = true;
		this.gc_evrak_kdv_toplam.Width = 80;
		this.gc_evrak_yekun.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.gc_evrak_yekun.AppearanceCell.Options.UseFont = true;
		this.gc_evrak_yekun.Caption = "Yekün";
		this.gc_evrak_yekun.FieldName = "gc_evrak_yekun";
		this.gc_evrak_yekun.Name = "gc_evrak_yekun";
		this.gc_evrak_yekun.OptionsColumn.AllowEdit = false;
		this.gc_evrak_yekun.OptionsColumn.AllowFocus = false;
		this.gc_evrak_yekun.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[1]
		{
			new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "gc_evrak_yekun", "{0:c2}")
		});
		this.gc_evrak_yekun.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_yekun.Visible = true;
		this.gc_evrak_yekun.Width = 123;
		this.cardView1.FocusedCardTopFieldIndex = 0;
		this.cardView1.GridControl = this.gridControl_evraklar;
		this.cardView1.Name = "cardView1";
		this.gridView1.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView1.Name = "gridView1";
		this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView1.OptionsView.ShowGroupPanel = false;
		this.sb_Secili_Evraklari_aktar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.sb_Secili_Evraklari_aktar.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_Secili_Evraklari_aktar.Appearance.Options.UseFont = true;
		this.sb_Secili_Evraklari_aktar.Location = new System.Drawing.Point(1036, 498);
		this.sb_Secili_Evraklari_aktar.Name = "sb_Secili_Evraklari_aktar";
		this.sb_Secili_Evraklari_aktar.Size = new System.Drawing.Size(164, 23);
		this.sb_Secili_Evraklari_aktar.TabIndex = 6;
		this.sb_Secili_Evraklari_aktar.Text = "SEÇİLİ EVRAKLARI AKTAR";
		this.sb_Secili_Evraklari_aktar.Click += new System.EventHandler(sb_Secili_Evraklari_aktar_Click);
		this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.dosyaToolStripMenuItem, this.gorunumToolStripMenuItem, this.islemlerToolStripMenuItem, this.parametrelerToolStripMenuItem, this.kriterToolStripMenuItem, this.aktarımParametreleriToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(1206, 24);
		this.menuStrip1.TabIndex = 2;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.yeniToolStripMenuItem, this.acToolStripMenuItem, this.toolStripSeparator3, this.kaydetToolStripMenuItem, this.farklıKaydetToolStripMenuItem, this.toolStripSeparator2, this.kapatToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.yeniToolStripMenuItem.Name = "yeniToolStripMenuItem";
		this.yeniToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
		this.yeniToolStripMenuItem.Text = "Yeni";
		this.yeniToolStripMenuItem.Click += new System.EventHandler(yeniToolStripMenuItem_Click);
		this.acToolStripMenuItem.Name = "acToolStripMenuItem";
		this.acToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
		this.acToolStripMenuItem.Text = "Aç...";
		this.acToolStripMenuItem.Click += new System.EventHandler(acToolStripMenuItem_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(147, 6);
		this.kaydetToolStripMenuItem.Name = "kaydetToolStripMenuItem";
		this.kaydetToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
		this.kaydetToolStripMenuItem.Text = "Kaydet";
		this.kaydetToolStripMenuItem.Click += new System.EventHandler(kaydetToolStripMenuItem_Click);
		this.farklıKaydetToolStripMenuItem.Name = "farklıKaydetToolStripMenuItem";
		this.farklıKaydetToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
		this.farklıKaydetToolStripMenuItem.Text = "Farklı Kaydet...";
		this.farklıKaydetToolStripMenuItem.Click += new System.EventHandler(farklıKaydetToolStripMenuItem_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(147, 6);
		this.kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
		this.kapatToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
		this.kapatToolStripMenuItem.Text = "Kapat";
		this.kapatToolStripMenuItem.Click += new System.EventHandler(kapatToolStripMenuItem_Click);
		this.gorunumToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.evraklarToolStripMenuItem, this.toolStripSeparator1, this.aramaTablolariToolStripMenuItem });
		this.gorunumToolStripMenuItem.Name = "gorunumToolStripMenuItem";
		this.gorunumToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
		this.gorunumToolStripMenuItem.Text = "Görünüm";
		this.evraklarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.evraklar_kolonlaraGoreGruplamaToolStripMenuItem, this.evraklar_kolonSeciciyiGosterToolStripMenuItem, this.evraklar_butunGruplariAcToolStripMenuItem, this.evraklar_butunGruplariKapatToolStripMenuItem, this.toolStripSeparator4, this.görünümüSaklaToolStripMenuItem1, this.görünümüYükleToolStripMenuItem, this.evraklar_otomatikDosyayiSilToolStripMenuItem, this.evraklar_varsayılanaGeriDonToolStripMenuItem });
		this.evraklarToolStripMenuItem.Name = "evraklarToolStripMenuItem";
		this.evraklarToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
		this.evraklarToolStripMenuItem.Text = "Evraklar";
		this.evraklar_kolonlaraGoreGruplamaToolStripMenuItem.Name = "evraklar_kolonlaraGoreGruplamaToolStripMenuItem";
		this.evraklar_kolonlaraGoreGruplamaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.evraklar_kolonlaraGoreGruplamaToolStripMenuItem.Text = "Kolonlara göre gruplama";
		this.evraklar_kolonlaraGoreGruplamaToolStripMenuItem.Click += new System.EventHandler(evraklar_kolonlaraGoreGruplamaToolStripMenuItem_Click);
		this.evraklar_kolonSeciciyiGosterToolStripMenuItem.Name = "evraklar_kolonSeciciyiGosterToolStripMenuItem";
		this.evraklar_kolonSeciciyiGosterToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.evraklar_kolonSeciciyiGosterToolStripMenuItem.Text = "Kolon seçiciyi göster";
		this.evraklar_kolonSeciciyiGosterToolStripMenuItem.Click += new System.EventHandler(evraklar_kolonSeciciyiGosterToolStripMenuItem_Click);
		this.evraklar_butunGruplariAcToolStripMenuItem.Name = "evraklar_butunGruplariAcToolStripMenuItem";
		this.evraklar_butunGruplariAcToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.evraklar_butunGruplariAcToolStripMenuItem.Text = "Bütün grupları aç";
		this.evraklar_butunGruplariAcToolStripMenuItem.Click += new System.EventHandler(evraklar_butunGruplariAcToolStripMenuItem_Click);
		this.evraklar_butunGruplariKapatToolStripMenuItem.Name = "evraklar_butunGruplariKapatToolStripMenuItem";
		this.evraklar_butunGruplariKapatToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.evraklar_butunGruplariKapatToolStripMenuItem.Text = "Bütün grupları kapat";
		this.evraklar_butunGruplariKapatToolStripMenuItem.Click += new System.EventHandler(evraklar_butunGruplariKapatToolStripMenuItem_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(202, 6);
		this.görünümüSaklaToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1, this.evraklar_farkliDosyaya_kaydet_ToolStripMenuItem });
		this.görünümüSaklaToolStripMenuItem1.Name = "görünümüSaklaToolStripMenuItem1";
		this.görünümüSaklaToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
		this.görünümüSaklaToolStripMenuItem1.Text = "Görünümü sakla";
		this.evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1.Name = "evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1";
		this.evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1.Size = new System.Drawing.Size(170, 22);
		this.evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1.Text = "Otomatik dosyaya";
		this.evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1.Click += new System.EventHandler(evraklar_otomatikDosyaya_kaydet_ToolStripMenuItem1_Click);
		this.evraklar_farkliDosyaya_kaydet_ToolStripMenuItem.Name = "evraklar_farkliDosyaya_kaydet_ToolStripMenuItem";
		this.evraklar_farkliDosyaya_kaydet_ToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
		this.evraklar_farkliDosyaya_kaydet_ToolStripMenuItem.Text = "Farklı dosyaya...";
		this.evraklar_farkliDosyaya_kaydet_ToolStripMenuItem.Click += new System.EventHandler(evraklar_farkliDosyaya_kaydet_ToolStripMenuItem_Click);
		this.görünümüYükleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.evraklar_otomatikDosyadan_yukle_ToolStripMenuItem, this.evraklar_farkliDosyadan_yukle_ToolStripMenuItem });
		this.görünümüYükleToolStripMenuItem.Name = "görünümüYükleToolStripMenuItem";
		this.görünümüYükleToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.görünümüYükleToolStripMenuItem.Text = "Görünümü yükle";
		this.evraklar_otomatikDosyadan_yukle_ToolStripMenuItem.Name = "evraklar_otomatikDosyadan_yukle_ToolStripMenuItem";
		this.evraklar_otomatikDosyadan_yukle_ToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.evraklar_otomatikDosyadan_yukle_ToolStripMenuItem.Text = "Otomatik dosyadan";
		this.evraklar_otomatikDosyadan_yukle_ToolStripMenuItem.Click += new System.EventHandler(evraklar_otomatikDosyadan_yukle_ToolStripMenuItem_Click);
		this.evraklar_farkliDosyadan_yukle_ToolStripMenuItem.Name = "evraklar_farkliDosyadan_yukle_ToolStripMenuItem";
		this.evraklar_farkliDosyadan_yukle_ToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.evraklar_farkliDosyadan_yukle_ToolStripMenuItem.Text = "Farklı dosyadan...";
		this.evraklar_farkliDosyadan_yukle_ToolStripMenuItem.Click += new System.EventHandler(evraklar_farkliDosyadan_yukle_ToolStripMenuItem_Click);
		this.evraklar_otomatikDosyayiSilToolStripMenuItem.Name = "evraklar_otomatikDosyayiSilToolStripMenuItem";
		this.evraklar_otomatikDosyayiSilToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.evraklar_otomatikDosyayiSilToolStripMenuItem.Text = "Otomatik dosyayı sil";
		this.evraklar_otomatikDosyayiSilToolStripMenuItem.Click += new System.EventHandler(evraklar_otomatikDosyayiSilToolStripMenuItem_Click);
		this.evraklar_varsayılanaGeriDonToolStripMenuItem.Name = "evraklar_varsayılanaGeriDonToolStripMenuItem";
		this.evraklar_varsayılanaGeriDonToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.evraklar_varsayılanaGeriDonToolStripMenuItem.Text = "Varsayılana geri dön";
		this.evraklar_varsayılanaGeriDonToolStripMenuItem.Click += new System.EventHandler(evraklar_varsayılanaGeriDonToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
		this.aramaTablolariToolStripMenuItem.Name = "aramaTablolariToolStripMenuItem";
		this.aramaTablolariToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
		this.aramaTablolariToolStripMenuItem.Text = "Arama tabloları";
		this.islemlerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikEvrakSiraNoVerToolStripMenuItem, this.otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem });
		this.islemlerToolStripMenuItem.Name = "islemlerToolStripMenuItem";
		this.islemlerToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
		this.islemlerToolStripMenuItem.Text = "İşlemler";
		this.otomatikEvrakSiraNoVerToolStripMenuItem.Name = "otomatikEvrakSiraNoVerToolStripMenuItem";
		this.otomatikEvrakSiraNoVerToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
		this.otomatikEvrakSiraNoVerToolStripMenuItem.Text = "Otomatik evrak sıra no ver (Satır birleştir)";
		this.otomatikEvrakSiraNoVerToolStripMenuItem.Click += new System.EventHandler(otomatikEvrakSiraNoVerToolStripMenuItem_Click);
		this.otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem.Name = "otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem";
		this.otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
		this.otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem.Text = "Otomatik evrak sıra no ver (Satırlar ayrı evrak)";
		this.otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem.Click += new System.EventHandler(otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem_Click);
		this.parametrelerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.varsayilanDegerlerToolStripMenuItem, this.akisParametreleriToolStripMenuItem });
		this.parametrelerToolStripMenuItem.Name = "parametrelerToolStripMenuItem";
		this.parametrelerToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
		this.parametrelerToolStripMenuItem.Text = "Parametreler";
		this.varsayilanDegerlerToolStripMenuItem.Name = "varsayilanDegerlerToolStripMenuItem";
		this.varsayilanDegerlerToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
		this.varsayilanDegerlerToolStripMenuItem.Text = "Varsayılan değerler";
		this.varsayilanDegerlerToolStripMenuItem.Click += new System.EventHandler(varsayilanDegerlerToolStripMenuItem_Click);
		this.akisParametreleriToolStripMenuItem.Name = "akisParametreleriToolStripMenuItem";
		this.akisParametreleriToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
		this.akisParametreleriToolStripMenuItem.Text = "Akış parametreleri";
		this.akisParametreleriToolStripMenuItem.Click += new System.EventHandler(akisParametreleriToolStripMenuItem_Click);
		this.kriterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.kriterleriDuzenleToolStripMenuItem });
		this.kriterToolStripMenuItem.Name = "kriterToolStripMenuItem";
		this.kriterToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
		this.kriterToolStripMenuItem.Text = "Kriter";
		this.kriterleriDuzenleToolStripMenuItem.Name = "kriterleriDuzenleToolStripMenuItem";
		this.kriterleriDuzenleToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.kriterleriDuzenleToolStripMenuItem.Text = "Kriterleri düzenle";
		this.kriterleriDuzenleToolStripMenuItem.Click += new System.EventHandler(kriterleriDuzenleToolStripMenuItem_Click);
		this.aktarımParametreleriToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.txtCsvAktarimParametreleriToolStripMenuItem, this.sqlAktarimParametreleriToolStripMenuItem });
		this.aktarımParametreleriToolStripMenuItem.Name = "aktarımParametreleriToolStripMenuItem";
		this.aktarımParametreleriToolStripMenuItem.Size = new System.Drawing.Size(115, 20);
		this.aktarımParametreleriToolStripMenuItem.Text = "Aktarım şablonları";
		this.txtCsvAktarimParametreleriToolStripMenuItem.Name = "txtCsvAktarimParametreleriToolStripMenuItem";
		this.txtCsvAktarimParametreleriToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
		this.txtCsvAktarimParametreleriToolStripMenuItem.Text = "Txt/Csv/Xml aktarım şablonları";
		this.txtCsvAktarimParametreleriToolStripMenuItem.Click += new System.EventHandler(txtCsvAktarimParametreleriToolStripMenuItem_Click);
		this.sqlAktarimParametreleriToolStripMenuItem.Name = "sqlAktarimParametreleriToolStripMenuItem";
		this.sqlAktarimParametreleriToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
		this.sqlAktarimParametreleriToolStripMenuItem.Text = "Sql aktarım şablonları";
		this.sqlAktarimParametreleriToolStripMenuItem.Click += new System.EventHandler(sqlAktarimParametreleriToolStripMenuItem_Click);
		this.backgroundWorker_Kontrol.WorkerReportsProgress = true;
		this.backgroundWorker_Kontrol.WorkerSupportsCancellation = true;
		this.backgroundWorker_Kontrol.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_Kontrol_DoWork);
		this.backgroundWorker_Kontrol.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker_Kontrol_ProgressChanged);
		this.backgroundWorker_Kontrol.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorker_Kontrol_RunWorkerCompleted);
		this.progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.progressBar1.Location = new System.Drawing.Point(12, 498);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(610, 23);
		this.progressBar1.TabIndex = 3;
		this.label_islem_adi.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label_islem_adi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_adi.Location = new System.Drawing.Point(709, 498);
		this.label_islem_adi.Name = "label_islem_adi";
		this.label_islem_adi.Size = new System.Drawing.Size(117, 23);
		this.label_islem_adi.TabIndex = 4;
		this.label_islem_adi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label_islem_bilgi.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label_islem_bilgi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_bilgi.ForeColor = System.Drawing.Color.Red;
		this.label_islem_bilgi.Location = new System.Drawing.Point(628, 498);
		this.label_islem_bilgi.Name = "label_islem_bilgi";
		this.label_islem_bilgi.Size = new System.Drawing.Size(75, 23);
		this.label_islem_bilgi.TabIndex = 5;
		this.label_islem_bilgi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.backgroundWorker_Aktarim.WorkerReportsProgress = true;
		this.backgroundWorker_Aktarim.WorkerSupportsCancellation = true;
		this.backgroundWorker_Aktarim.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_Aktarim_DoWork);
		this.backgroundWorker_Aktarim.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker_Aktarim_ProgressChanged);
		this.backgroundWorker_Aktarim.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorker_Aktarim_RunWorkerCompleted);
		this.sb_TxtCsv_Al.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_TxtCsv_Al.Appearance.Options.UseFont = true;
		this.sb_TxtCsv_Al.Location = new System.Drawing.Point(12, 27);
		this.sb_TxtCsv_Al.Name = "sb_TxtCsv_Al";
		this.sb_TxtCsv_Al.Size = new System.Drawing.Size(126, 23);
		this.sb_TxtCsv_Al.TabIndex = 7;
		this.sb_TxtCsv_Al.Text = "TXT/CSV/XML AL";
		this.sb_TxtCsv_Al.Click += new System.EventHandler(sb_TxtCsv_Al_Click);
		this.sb_Sql_Al.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_Sql_Al.Appearance.Options.UseFont = true;
		this.sb_Sql_Al.Location = new System.Drawing.Point(144, 27);
		this.sb_Sql_Al.Name = "sb_Sql_Al";
		this.sb_Sql_Al.Size = new System.Drawing.Size(95, 23);
		this.sb_Sql_Al.TabIndex = 8;
		this.sb_Sql_Al.Text = "SQL AL";
		this.sb_Sql_Al.Click += new System.EventHandler(sb_Sql_Al_Click);
		this.cb_belgeno_kontrolu_yap.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cb_belgeno_kontrolu_yap.AutoSize = true;
		this.cb_belgeno_kontrolu_yap.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.cb_belgeno_kontrolu_yap.Location = new System.Drawing.Point(851, 498);
		this.cb_belgeno_kontrolu_yap.Name = "cb_belgeno_kontrolu_yap";
		this.cb_belgeno_kontrolu_yap.Size = new System.Drawing.Size(179, 21);
		this.cb_belgeno_kontrolu_yap.TabIndex = 9;
		this.cb_belgeno_kontrolu_yap.Text = "Belge no kontrolü yap";
		this.cb_belgeno_kontrolu_yap.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1206, 524);
		base.Controls.Add(this.cb_belgeno_kontrolu_yap);
		base.Controls.Add(this.sb_Sql_Al);
		base.Controls.Add(this.sb_TxtCsv_Al);
		base.Controls.Add(this.gridControl_evraklar);
		base.Controls.Add(this.label_islem_bilgi);
		base.Controls.Add(this.label_islem_adi);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.sb_Secili_Evraklari_aktar);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "SatirBazliGenelEvrakGirisi";
		this.Text = "Satır Bazlı Genel Evrak Girişi";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(TopluGenelEvrakGirisi_FormClosing);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(SatirBazliGenelEvrakGirisi_FormClosed);
		base.Load += new System.EventHandler(EvrakDuzenleme_Load);
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Cinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_SatirCinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_VergiPntr).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_IskontoSekli).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl_evraklar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Evrak_Kur).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_OtvSekli).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cardView1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
