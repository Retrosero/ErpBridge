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
using Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;
using Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli.Import;
using Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli.Kriter;
using Fora.Mikro;
using Fora.Mikro.Bankalar;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Depolar;
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
using Fora.Mikro.Tahsilatlar;
using Fora.Mikro.Utility;
using fastJSON;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli;

[Guid("8497A9D8-C87A-4568-96CB-95E9EDC3EA79")]
public class SatirBazliTahsilatEvrakGirisi : XtraForm
{
	private TahsilatEvrakSatirBaslik _satirlar;

	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private List<TahsilatEvrakKriter> _uygulanacak_kriterler;

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

	private RepositoryItemGridLookUpEdit repositoryItem_Kasa;

	private GridView gridView_Arama_Kasa;

	private MemoryStream arama_kasa_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Banka;

	private GridView gridView_Arama_Banka;

	private MemoryStream arama_banka_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_CariPersonel;

	private GridView gridView_Arama_CariPersonel;

	private MemoryStream arama_caripersonel_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_Proje;

	private GridView gridView_Arama_Proje;

	private MemoryStream arama_proje_temp_defaultlayoutStream;

	private RepositoryItemGridLookUpEdit repositoryItem_SorumlulukMerkezi;

	private GridView gridView_Arama_SorumlulukMerkezi;

	private MemoryStream arama_sorumlulukmerkezi_temp_defaultlayoutStream;

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

	private BandedGridColumn gc_evrak_EvrakSeri;

	private BandedGridColumn gc_evrak_EvrakSira;

	private BandedGridColumn gc_evrak_Tarih;

	private BandedGridColumn gc_evrak_BelgeNo;

	private BandedGridColumn gc_evrak_BelgeTarihi;

	private BandedGridColumn gc_evrak_CariKodu;

	private BandedGridColumn gc_evrak_CariAdi;

	private BandedGridColumn gc_evrak_DovizCinsi;

	private BandedGridColumn gc_evrak_Kur;

	private BandedGridColumn gc_evrak_Plasiyer;

	private BandedGridColumn gc_evrak_ProjeKodu;

	private BandedGridColumn gc_evrak_SorMerKodu;

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

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_TicaretTuru;

	private GridView gridView_TicaretTuru;

	private ToolStripMenuItem parametrelerToolStripMenuItem;

	private ToolStripMenuItem varsayilanDegerlerToolStripMenuItem;

	private ToolStripMenuItem akisParametreleriToolStripMenuItem;

	private RepositoryItemTextEdit repositoryItemTextEdit_FaturaAciklama;

	private RepositoryItemTextEdit repositoryItemTextEdit_Aciklamalar;

	private BandedGridColumn gc_evrak_ProjeAdi;

	private BandedGridColumn gc_evrak_SorMerAdi;

	private BandedGridColumn gc_evrak_KayitID;

	private ToolStripSeparator toolStripSeparator2;

	private SimpleButton sb_TxtCsv_Al;

	private BandedGridColumn gc_firma_no;

	private BandedGridColumn gc_sube_no;

	private BandedGridColumn gc_satir_cinsi;

	private BandedGridColumn gc_satir_HesapKodu;

	private BandedGridColumn gc_satir_HesapAdi;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Satir_OtvSekli;

	private ToolStripSeparator toolStripSeparator1;

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

	private BandedGridColumn gc_satir_tutar;

	private BandedGridColumn gc_satir_vadesi;

	private GridBand gridBand15;

	private GridBand gridBand9;

	private GridBand gridBand10;

	private GridBand gridBand2;

	private GridBand gridBand1;

	private CheckBox cb_belgeno_kontrolu_yap;

	public SatirBazliTahsilatEvrakGirisi(MikroUygulamaBilgileri mikrouygulamabilgileri, string tag, string baslik)
	{
		InitializeComponent();
		_satirlar = new TahsilatEvrakSatirBaslik();
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
		BindingList<TahsilatEvrakSatir> obj = (BindingList<TahsilatEvrakSatir>)sender;
		e.NewObject = new TahsilatEvrakSatir();
		TahsilatEvrakSatir tahsilatEvrakSatir = (TahsilatEvrakSatir)e.NewObject;
		int num = 0;
		foreach (TahsilatEvrakSatir item in obj)
		{
			if (item.KayitID >= num)
			{
				num = item.KayitID + 1;
			}
		}
		tahsilatEvrakSatir.KayitID = num;
		tahsilatEvrakSatir.evraknoseri = "AA";
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

	private void RepositoryItemAyarla()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(int));
		dataTable.Columns.Add("Isim", typeof(string));
		dataTable.Rows.Add(0, "Nakit");
		dataTable.Rows.Add(1, "Müşteri çeki");
		dataTable.Rows.Add(2, "Müşteri seneti");
		dataTable.Rows.Add(3, "Müşteri kredi kartı");
		dataTable.Rows.Add(4, "Gelen havale");
		dataTable.Rows.Add(5, "Giden havale");
		repositoryItemGridLookUpEdit_Satir_Cinsi.DataSource = dataTable;
		repositoryItemGridLookUpEdit_Satir_Cinsi.ValueMember = "ID";
		repositoryItemGridLookUpEdit_Satir_Cinsi.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_Satir_Cinsi.PopulateViewColumns();
		foreach (GridColumn column in repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns)
		{
			column.Visible = false;
		}
		repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns[1].Caption = "Cinsi";
		repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_Satir_Cinsi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_Satir_Cinsi.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_Satir_Cinsi.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_Satir_Cinsi.AutoComplete = false;
		RepositoryItem_Cari_Init();
		RepositoryItem_Kasa_Init();
		RepositoryItem_Banka_Init();
		RepositoryItem_CariPersonel_Init();
		RepositoryItem_Proje_Init();
		RepositoryItem_SorumlulukMerkezi_Init();
		gc_evrak_CariKodu.ColumnEdit = repositoryItem_Cari;
		gc_evrak_CariAdi.ColumnEdit = repositoryItem_Cari;
		gc_evrak_ProjeKodu.ColumnEdit = repositoryItem_Proje;
		gc_evrak_ProjeAdi.ColumnEdit = repositoryItem_Proje;
		gc_evrak_Plasiyer.ColumnEdit = repositoryItem_CariPersonel;
		gc_evrak_SorMerKodu.ColumnEdit = repositoryItem_SorumlulukMerkezi;
		gc_evrak_SorMerAdi.ColumnEdit = repositoryItem_SorumlulukMerkezi;
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

	private void advBandedGridView_master_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		_ = (AdvBandedGridView)sender;
		TahsilatEvrakSatir tahsilatEvrakSatir = (TahsilatEvrakSatir)e.Row;
		if (e.IsSetData)
		{
			switch (e.Column.FieldName)
			{
			case "gc_satir_cinsi":
				tahsilatEvrakSatir.satir_cinsi = (enum_tahsilat_cinsi)e.Value;
				break;
			case "gc_evrak_Kur":
				tahsilatEvrakSatir.kur.dov_fiyat = (double)(decimal)e.Value;
				break;
			case "gc_evrak_Cari":
				EvrakCariDegistir(tahsilatEvrakSatir, e.Value.ToString(), FiyatGuncelle: true);
				break;
			case "gc_evrak_SorMer":
				EvrakSorumlulukMerkeziDegistir(tahsilatEvrakSatir, (string)e.Value);
				break;
			case "gc_evrak_Proje":
				EvrakProjeDegistir(tahsilatEvrakSatir, (string)e.Value);
				break;
			case "gc_evrak_DovizCinsi":
				EvrakDovizCinsiDegistir(tahsilatEvrakSatir, int.Parse(e.Value.ToString()));
				break;
			}
		}
		if (!e.IsGetData)
		{
			return;
		}
		switch (e.Column.FieldName)
		{
		case "gc_satir_cinsi":
			e.Value = (int)tahsilatEvrakSatir.satir_cinsi;
			break;
		case "gc_evrak_Kur":
			e.Value = (decimal)tahsilatEvrakSatir.kur.dov_fiyat;
			break;
		case "gc_evrak_AktarimDurumu":
		{
			enum_GenelEvrakAktarimDurumu aktarimDurumu = tahsilatEvrakSatir.AktarimDurumu;
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
		case "gc_evrak_Cari":
			e.Value = tahsilatEvrakSatir.cari.cari_kod;
			break;
		case "gc_satir_HesapKodu":
			e.Value = tahsilatEvrakSatir.satir_kasa_banka_kodu;
			break;
		case "gc_evrak_SorMer":
			e.Value = tahsilatEvrakSatir.sorumlulukmerkezi.som_kod;
			break;
		case "gc_evrak_Proje":
			e.Value = tahsilatEvrakSatir.proje.pro_kodu;
			break;
		case "gc_evrak_DovizCinsi":
		{
			int dovizcinsi = tahsilatEvrakSatir.dovizcinsi;
			if (dovizcinsi == tahsilatEvrakSatir.cari.cari_doviz_cinsi)
			{
				e.Value = 0;
			}
			if (dovizcinsi == tahsilatEvrakSatir.cari.cari_doviz_cinsi1)
			{
				e.Value = 1;
			}
			if (dovizcinsi == tahsilatEvrakSatir.cari.cari_doviz_cinsi2)
			{
				e.Value = 2;
			}
			break;
		}
		}
	}

	private void advBandedGridView_master_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
	{
		TahsilatEvrakSatir tahsilatEvrakSatir = (TahsilatEvrakSatir)(sender as AdvBandedGridView).GetRow(e.RowHandle);
		if (tahsilatEvrakSatir == null)
		{
			return;
		}
		switch (e.Column.Name)
		{
		case "gc_evrak_CariKodu":
			repositoryItem_Cari.DisplayMember = "KOD";
			break;
		case "gc_evrak_CariAdi":
			repositoryItem_Cari.DisplayMember = "İSİM";
			break;
		case "gc_satir_HesapKodu":
			if (tahsilatEvrakSatir.satir_cinsi != enum_tahsilat_cinsi.Nakit)
			{
				_ = 1;
			}
			break;
		case "gc_satir_HesapAdi":
			if (tahsilatEvrakSatir.satir_cinsi != enum_tahsilat_cinsi.Nakit)
			{
				_ = 1;
			}
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
			Cari cari = ((TahsilatEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow()).cari;
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
		int dovizcinsi = _satirlar._satirlar[e.ListSourceRowIndex].dovizcinsi;
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
		TahsilatEvrakSatir tahsilatEvrakSatir = _satirlar._satirlar[advBandedGridView_master.GetDataSourceRowIndex(e.RowHandle)];
		switch (e.Column.Name)
		{
		case "gc_evrak_AktarimDurumu":
			switch (tahsilatEvrakSatir.AktarimDurumu)
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
			if (tahsilatEvrakSatir.satir_kasa_banka_kodu == "" || tahsilatEvrakSatir.satir_kasa_banka_kodu == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		case "gc_satir_HesapAdi":
			if (tahsilatEvrakSatir.satir_kasa_banka_kodu == "" || tahsilatEvrakSatir.satir_kasa_banka_kodu == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		case "gc_evrak_CariKodu":
			if (tahsilatEvrakSatir.cari.cari_kod == "" || tahsilatEvrakSatir.cari.cari_kod == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		case "gc_evrak_CariAdi":
			if (tahsilatEvrakSatir.cari.cari_kod == "" || tahsilatEvrakSatir.cari.cari_kod == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
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
		TahsilatEvrakSatir tahsilatEvrakSatir = (TahsilatEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow();
		Kur kur = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, tahsilatEvrakSatir.dovizcinsi, "1", dateEdit.DateTime, _mikrouygulamabilgileri.MikroAnaDBName);
		EvrakKurDegistir(tahsilatEvrakSatir, kur.dov_fiyat);
	}

	private void repositoryItemTextEdit_Evrak_Kur_Validating(object sender, CancelEventArgs e)
	{
		if (((TahsilatEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow()).dovizcinsi == 0)
		{
			e.Cancel = true;
		}
	}

	private void repositoryItemGridLookUpEdit_AcikKapali_EditValueChanged(object sender, EventArgs e)
	{
	}

	private void repositoryItemGridLookUpEdit_Satir_Cinsi_EditValueChanging(object sender, ChangingEventArgs e)
	{
		int result = 0;
		if (int.TryParse(e.NewValue.ToString(), out result))
		{
			TahsilatEvrakSatir tahsilatEvrakSatir = (TahsilatEvrakSatir)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedRow();
			if (tahsilatEvrakSatir != null)
			{
				enum_tahsilat_cinsi satir_cinsi = (enum_tahsilat_cinsi)result;
				tahsilatEvrakSatir.satir_cinsi = satir_cinsi;
				tahsilatEvrakSatir.satir_kasa_banka_kodu = "";
			}
		}
	}

	private void EvrakCariDegistir(TahsilatEvrakSatir row, string YeniCariKodu, bool FiyatGuncelle)
	{
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
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

	private void EvrakStokHizmetKoduDegistir(TahsilatEvrakSatir row, string YeniStokHizmetKodu, bool FiyatGuncelle)
	{
		row.satir_kasa_banka_kodu = YeniStokHizmetKodu;
	}

	private void EvrakProjeDegistir(TahsilatEvrakSatir row, string YeniProjeKodu)
	{
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				Proje proje = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, YeniProjeKodu);
				item.proje = proje;
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakSorumlulukMerkeziDegistir(TahsilatEvrakSatir row, string YeniSorumlulukMerkeziKodu)
	{
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				SorumlulukMerkezi sorumlulukMerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, YeniSorumlulukMerkeziKodu);
				item.sorumlulukmerkezi = sorumlulukMerkezi;
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakDovizCinsiDegistir(TahsilatEvrakSatir row, int YeniDovizCinsiSira)
	{
		int num = YeniDovizCinsiSira switch
		{
			0 => row.cari.cari_doviz_cinsi, 
			1 => row.cari.cari_doviz_cinsi1, 
			2 => row.cari.cari_doviz_cinsi2, 
			_ => row.cari.cari_doviz_cinsi, 
		};
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				item.dovizcinsi = num;
			}
		}
		Kur kur = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, num, "1", row.evraktarih, _mikrouygulamabilgileri.MikroAnaDBName);
		EvrakKurDegistir(row, kur.dov_fiyat);
		gridControl_evraklar.RefreshDataSource();
	}

	private void EvrakKurDegistir(TahsilatEvrakSatir row, double YeniKur)
	{
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (row.evraknoseri == item.evraknoseri && row.evraknosira == item.evraknosira)
			{
				item.kur = new Kur();
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
		openFileDialog1.Filter = "Tahsilat Evrak Dosyaları|*.fte|Tüm dosyalar|*.*";
		if (openFileDialog1.ShowDialog() == DialogResult.OK)
		{
			TahsilatEvrakSatirBaslik satirlar = TahsilatEvrakSatirBaslik.ReadFromByteArray((byte[])GenelUtilityWin.ToObjectGZip(File.ReadAllBytes(openFileDialog1.FileName)));
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
			TahsilatEvrakSatirBaslik satirlar = new TahsilatEvrakSatirBaslik();
			_satirlar = satirlar;
			bindData();
		}
	}

	private void FarkliKaydet()
	{
		saveFileDialog1.Filter = "Tahsilat Evrak Dosyaları|*.fte|Tüm dosyalar|*.*";
		if (saveFileDialog1.ShowDialog() == DialogResult.OK)
		{
			openFileDialog1.FileName = saveFileDialog1.FileName;
			Kaydet(saveFileDialog1.FileName);
		}
	}

	private void Kaydet(string DosyaAdi)
	{
		try
		{
			if (!File.Exists(DosyaAdi))
			{
				File.Delete(DosyaAdi);
			}
			byte[] bytes = GenelUtilityWin.ToByteArrayGzip(TahsilatEvrakSatirBaslik.WriteToByteArray(new TahsilatEvrakSatirBaslik
			{
				_satirlar = _satirlar._satirlar
			}, 1));
			File.WriteAllBytes(DosyaAdi, bytes);
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
		saveFileDialog.Filter = "Tahsilat Evrak Görünüm |*.lgm|Tüm dosyalar|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			advBandedGridView_master.SaveLayoutToXml(saveFileDialog.FileName);
		}
	}

	private void evraklar_farkliDosyadan_yukle_ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		openFileDialog.Filter = "Tahsilat Evrak Görünüm |*.lgm|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			advBandedGridView_master.RestoreLayoutFromXml(openFileDialog.FileName);
		}
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
		new Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli.Kriter.KriterDuzenleme(_mikrouygulamabilgileri).ShowDialog();
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
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
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
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (item.Aktar)
			{
				TekSatirKontrolEt(item);
				num++;
				backgroundWorker_Kontrol.ReportProgress(num);
			}
		}
	}

	private void TekSatirKontrolEt(TahsilatEvrakSatir satir)
	{
		satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
		if (satir.cari.cari_kod == "" || satir.cari.cari_kod == null)
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.CariKoduGirilmemis;
			KontrolSonucu = false;
		}
		if (satir.satir_kasa_banka_kodu == "" || satir.satir_kasa_banka_kodu == null)
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.HesapKoduGirilmemis;
			KontrolSonucu = false;
		}
		if (satir.evraknosira <= 0)
		{
			satir.AktarimDurumu = enum_GenelEvrakAktarimDurumu.EvrakSiraSifirdanBuyukOlmali;
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
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (!item.Aktar)
			{
				continue;
			}
			bool flag = false;
			foreach (Evrak item2 in _AktarilacakEvraklar)
			{
				enum_GenelEvrakTipleri enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.Tahsilat;
				if (item.satir_cinsi == enum_tahsilat_cinsi.GelenHavale)
				{
					enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.GelenHavale;
				}
				if (item.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
				{
					enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.GidenHavale;
				}
				if (item.evraknoseri == item2.EvrakNoSeri && item2.evraktipi == enum_GenelEvrakTipleri && item.evraknosira == item2.EvrakNoSira)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				enum_GenelEvrakTipleri evrakTip = enum_GenelEvrakTipleri.Tahsilat;
				if (item.satir_cinsi == enum_tahsilat_cinsi.GelenHavale)
				{
					evrakTip = enum_GenelEvrakTipleri.GelenHavale;
				}
				if (item.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
				{
					evrakTip = enum_GenelEvrakTipleri.GidenHavale;
				}
				Evrak evrak = new Evrak(evrakTip, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
				evrak.SetDBCno(item.DBCno);
				evrak.SetEvrakTarihi(item.evraktarih);
				evrak.SetEvrakNoSeri(item.evraknoseri);
				evrak.SetEvraknoSira(item.evraknosira);
				evrak.SetBelgeNo(item.belgeno);
				evrak.SetBelgeTarihi(item.belgetarih);
				evrak.cari = item.cari;
				evrak.SetOdemePlani(0);
				evrak.SetFiyatListesi(new FiyatListesi());
				evrak.SetDovizCinsi(item.dovizcinsi);
				evrak.kur = item.kur;
				evrak.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
				evrak.alternatifdovizkuru = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, evrak.alternatifdovizcinsi, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
				evrak.SetKaynakDepo(new Depo());
				evrak.SetProje(item.proje);
				evrak.SetSorumlulukMerkezi(item.sorumlulukmerkezi);
				evrak.SetTemsilciKodu(item.temsilcikodu);
				evrak.SetFirma(item.firma);
				evrak.SetSube(item.sube);
				evrak.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
				evrak.SetSevkTeslimTarihi(DateTime.Now);
				evrak.SetSevkAdresNo(1);
				evrak.SetKapamaHesapKodu("");
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
				enum_GenelEvrakTipleri enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.Tahsilat;
				if (item.satir_cinsi == enum_tahsilat_cinsi.GelenHavale)
				{
					enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.GelenHavale;
				}
				if (item.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
				{
					enum_GenelEvrakTipleri2 = enum_GenelEvrakTipleri.GidenHavale;
				}
				if (enum_GenelEvrakTipleri2 != item3.evraktipi || !(item.evraknoseri == item3.EvrakNoSeri) || item.evraknosira != item3.EvrakNoSira)
				{
					continue;
				}
				switch (enum_GenelEvrakTipleri2)
				{
				case enum_GenelEvrakTipleri.Tahsilat:
				{
					Tahsilat tahsilat = new Tahsilat();
					tahsilat.aciklama = item.satir_aciklama;
					if (tahsilat.aciklama.Length > 40)
					{
						tahsilat.aciklama = tahsilat.aciklama.Substring(0, 40);
					}
					tahsilat.cinsi = item.satir_cinsi;
					tahsilat.evrak_dovizcinsi = item3.dovizcinsi;
					tahsilat.kasa_banka_kodu = item.satir_kasa_banka_kodu;
					tahsilat.referans = item.satir_referans;
					tahsilat.sck_banka_adres1 = item.satir_sck_banka_adres1;
					tahsilat.sck_bankano = item.satir_sck_bankano;
					tahsilat.sck_borclu = item.satir_sck_borclu;
					tahsilat.sck_hesapno_sehir = item.satir_sck_hesapno_sehir;
					tahsilat.sck_no = item.satir_sck_no;
					tahsilat.sck_sube_adres2 = item.satir_sck_sube_adres2;
					tahsilat.Sck_TCMB_Banka_kodu = item.satir_Sck_TCMB_Banka_kodu;
					tahsilat.Sck_TCMB_il_kodu = item.satir_Sck_TCMB_il_kodu;
					tahsilat.Sck_TCMB_Sube_kodu = item.satir_Sck_TCMB_Sube_kodu;
					tahsilat.sck_vdaire_no = item.satir_sck_vdaire_no;
					tahsilat.sorumlulukmerkezi = item.satir_sorumlulukmerkezi;
					tahsilat.tutar = item.satir_tutar;
					tahsilat.vadesi = item.satir_vadesi;
					item3.AddTahsilat(tahsilat);
					break;
				}
				case enum_GenelEvrakTipleri.Tediye:
				{
					Tahsilat tahsilat2 = new Tahsilat();
					tahsilat2.aciklama = item.satir_aciklama;
					if (tahsilat2.aciklama.Length > 40)
					{
						tahsilat2.aciklama = tahsilat2.aciklama.Substring(0, 40);
					}
					tahsilat2.cinsi = item.satir_cinsi;
					tahsilat2.evrak_dovizcinsi = item3.dovizcinsi;
					tahsilat2.kasa_banka_kodu = item.satir_kasa_banka_kodu;
					tahsilat2.referans = item.satir_referans;
					tahsilat2.sck_banka_adres1 = item.satir_sck_banka_adres1;
					tahsilat2.sck_bankano = item.satir_sck_bankano;
					tahsilat2.sck_borclu = item.satir_sck_borclu;
					tahsilat2.sck_hesapno_sehir = item.satir_sck_hesapno_sehir;
					tahsilat2.sck_no = item.satir_sck_no;
					tahsilat2.sck_sube_adres2 = item.satir_sck_sube_adres2;
					tahsilat2.Sck_TCMB_Banka_kodu = item.satir_Sck_TCMB_Banka_kodu;
					tahsilat2.Sck_TCMB_il_kodu = item.satir_Sck_TCMB_il_kodu;
					tahsilat2.Sck_TCMB_Sube_kodu = item.satir_Sck_TCMB_Sube_kodu;
					tahsilat2.sck_vdaire_no = item.satir_sck_vdaire_no;
					tahsilat2.sorumlulukmerkezi = item.satir_sorumlulukmerkezi;
					tahsilat2.tutar = item.satir_tutar;
					tahsilat2.vadesi = item.satir_vadesi;
					item3.AddTahsilat(tahsilat2);
					break;
				}
				default:
				{
					string cari_kod = item.cari.cari_kod;
					string satir_kasa_banka_kodu = item.satir_kasa_banka_kodu;
					enum_cha_tpoz cha_tpoz = enum_cha_tpoz.Acik;
					DateTime cha_d_kurtar = new DateTime(1900, 1, 1);
					string satir_aciklama = item.satir_aciklama;
					string som_kod = item.satir_sorumlulukmerkezi.som_kod;
					string som_kod2 = item3.sorumlulukmerkezi.som_kod;
					string pro_kodu = item.proje.pro_kodu;
					string cha_trefno = "";
					enum_cha_kasa_hizmet cha_kasa_hizmet = enum_cha_kasa_hizmet.Bankamiz;
					enum_cha_sntck_poz cha_sntck_poz = enum_cha_sntck_poz.Portfoyde;
					Banka banka = BankaData.GetBanka(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item.satir_kasa_banka_kodu);
					int num = 0;
					int ban_doviz_cinsi = banka.ban_doviz_cinsi;
					double num2 = 1.0;
					int cha_karsidgrupno = 1;
					double num3 = 1.0;
					int num4 = 0;
					if (num == 0)
					{
						num4 = item3.cari.cari_doviz_cinsi;
					}
					if (num == 1)
					{
						num4 = item3.cari.cari_doviz_cinsi1;
					}
					if (num == 2)
					{
						num4 = item3.cari.cari_doviz_cinsi2;
					}
					num3 = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, num4, "1", item3.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName).dov_fiyat;
					double num5 = item.satir_tutar * num2 / num3;
					_ = item.satir_tutar;
					enum_cha_cari_cins enum_cha_cari_cins = enum_cha_cari_cins.Carimiz;
					string text = "";
					enum_cha_cari_cins = enum_cha_cari_cins.Carimiz;
					text = CariData.GetCariByCariKod(Db.Connection, cari_kod, AdreslerTemsilciyeGore: false, "").cari_temsilci_kodu;
					CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
					switch (item3.evraktipi)
					{
					case enum_GenelEvrakTipleri.GelenHavale:
						cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.GelenHavale;
						cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
						cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.Nakit;
						break;
					case enum_GenelEvrakTipleri.GidenHavale:
						cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.GonderilenHavale;
						cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Borc;
						cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.Nakit;
						break;
					}
					cARI_HESAP_HAREKETLERI.cha_RECid_DBCno = item3.DBCno;
					cARI_HESAP_HAREKETLERI.cha_create_user = item3.mikrouserno;
					cARI_HESAP_HAREKETLERI.cha_lastup_user = item3.mikrouserno;
					cARI_HESAP_HAREKETLERI.cha_normal_Iade = item3.normaliade;
					cARI_HESAP_HAREKETLERI.cha_ticaret_turu = item3.ticaretturu;
					cARI_HESAP_HAREKETLERI.cha_firmano = item3.Firma.fir_sirano;
					cARI_HESAP_HAREKETLERI.cha_subeno = item3.Sube.Sube_no;
					cARI_HESAP_HAREKETLERI.cha_tarihi = item3.EvrakTarihi;
					cARI_HESAP_HAREKETLERI.cha_satir_no = item3.GetGenelCariHesapHareketleri().Count;
					cARI_HESAP_HAREKETLERI.cha_evrakno_seri = item3.EvrakNoSeri;
					cARI_HESAP_HAREKETLERI.cha_evrakno_sira = item3.EvrakNoSira;
					cARI_HESAP_HAREKETLERI.cha_belge_no = item3.BelgeNo;
					cARI_HESAP_HAREKETLERI.cha_belge_tarih = item3.BelgeTarihi;
					cARI_HESAP_HAREKETLERI.cha_altd_kur = item3.alternatifdovizkuru.dov_fiyat;
					string obj = item3.EvrakTarihi.Year.ToString();
					string text2 = item3.EvrakTarihi.Month.ToString();
					if (text2.Length == 1)
					{
						text2 = "0" + text2;
					}
					string text3 = item3.EvrakTarihi.Day.ToString();
					if (text3.Length == 1)
					{
						text3 = "0" + text3;
					}
					int cha_vade = int.Parse(obj + text2 + text3);
					cARI_HESAP_HAREKETLERI.cha_cari_cins = enum_cha_cari_cins;
					cARI_HESAP_HAREKETLERI.cha_kod = cari_kod;
					cARI_HESAP_HAREKETLERI.cha_satici_kodu = text;
					cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = satir_kasa_banka_kodu;
					cARI_HESAP_HAREKETLERI.cha_d_kurtar = cha_d_kurtar;
					cARI_HESAP_HAREKETLERI.cha_d_cins = num4;
					cARI_HESAP_HAREKETLERI.cha_d_kur = num3;
					cARI_HESAP_HAREKETLERI.cha_grupno = num;
					cARI_HESAP_HAREKETLERI.cha_meblag = num5;
					cARI_HESAP_HAREKETLERI.cha_aratoplam = num5;
					cARI_HESAP_HAREKETLERI.cha_vade = cha_vade;
					cARI_HESAP_HAREKETLERI.cha_aciklama = satir_aciklama;
					if (cARI_HESAP_HAREKETLERI.cha_aciklama.Length > 40)
					{
						cARI_HESAP_HAREKETLERI.cha_aciklama = cARI_HESAP_HAREKETLERI.cha_aciklama.Substring(0, 40);
					}
					cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = cha_kasa_hizmet;
					cARI_HESAP_HAREKETLERI.cha_tpoz = cha_tpoz;
					cARI_HESAP_HAREKETLERI.cha_trefno = cha_trefno;
					cARI_HESAP_HAREKETLERI.cha_sntck_poz = cha_sntck_poz;
					cARI_HESAP_HAREKETLERI.cha_karsidcinsi = ban_doviz_cinsi;
					cARI_HESAP_HAREKETLERI.cha_karsid_kur = num2;
					cARI_HESAP_HAREKETLERI.cha_karsidgrupno = cha_karsidgrupno;
					cARI_HESAP_HAREKETLERI.cha_srmrkkodu = som_kod;
					cARI_HESAP_HAREKETLERI.cha_karsisrmrkkodu = som_kod2;
					cARI_HESAP_HAREKETLERI.cha_projekodu = pro_kodu;
					cARI_HESAP_HAREKETLERI.cha_create_date = DateTime.Now;
					cARI_HESAP_HAREKETLERI.cha_lastup_date = DateTime.Now;
					cARI_HESAP_HAREKETLERI.cha_fis_tarih = new DateTime(1900, 1, 1);
					cARI_HESAP_HAREKETLERI.cha_fis_sirano = 0;
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi1 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi2 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi3 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi4 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi5 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi6 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi7 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi8 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi9 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergi10 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_yuvarlama = 0.0;
					cARI_HESAP_HAREKETLERI.cha_reftarihi = new DateTime(1899, 12, 30);
					cARI_HESAP_HAREKETLERI.cha_odeme_arr1 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_odeme_arr2 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_odeme_arr3 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_odeme_arr4 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_odeme_arr5 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_odeme_arr6 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_odeme_arr7 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_odeme_arr8 = 0.0;
					cARI_HESAP_HAREKETLERI.cha_miktari = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergipntr = 0;
					cARI_HESAP_HAREKETLERI.cha_istisnakodu = 0;
					cARI_HESAP_HAREKETLERI.cha_ver_tev_carpani = 0.0;
					cARI_HESAP_HAREKETLERI.cha_stopaj = 0.0;
					cARI_HESAP_HAREKETLERI.cha_savsandesfonu = 0.0;
					cARI_HESAP_HAREKETLERI.cha_vergisiz_fl = false;
					cARI_HESAP_HAREKETLERI.cha_mustahsil_borsa = 0.0;
					cARI_HESAP_HAREKETLERI.cha_mustahsil_bagkur = 0.0;
					cARI_HESAP_HAREKETLERI.cha_mustahsil_diger = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalMSDF = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalHamaliye = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalStopaj = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalKomisyonu = 0.0;
					cARI_HESAP_HAREKETLERI.cha_StFonPntr = 0;
					cARI_HESAP_HAREKETLERI.cha_pos_hareketi = false;
					cARI_HESAP_HAREKETLERI.cha_vardiya_tarihi = new DateTime(1900, 1, 1);
					cARI_HESAP_HAREKETLERI.cha_vardiya_no = 0;
					cARI_HESAP_HAREKETLERI.cha_vardiya_evrak_ti = enum_cha_vardita_evrak_ti.HizmetSatisi;
					cARI_HESAP_HAREKETLERI.cha_HalRusum = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalNavlunTut = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalRehinFuture = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalKomisyon = 0.0;
					cARI_HESAP_HAREKETLERI.cha_Vade_Farki_Yuz = 0.0;
					cARI_HESAP_HAREKETLERI.cha_EXIMkodu = "";
					cARI_HESAP_HAREKETLERI.cha_HalRehinSandikmiktari = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalSandikVrMiktar = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalSandikTutari = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalSandikKDVTutari = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalrehinSandikTutari = 0.0;
					cARI_HESAP_HAREKETLERI.cha_Tevkifat_turu = enum_cha_Tevkifat_turu.TevkifatYok;
					cARI_HESAP_HAREKETLERI.cha_otvtutari = 0.0;
					cARI_HESAP_HAREKETLERI.cha_otvvergisiz_fl = false;
					cARI_HESAP_HAREKETLERI.cha_sozlesme_DBCno = 0;
					cARI_HESAP_HAREKETLERI.cha_sozlesme_RECno = 0;
					cARI_HESAP_HAREKETLERI.cha_yat_tes_kodu = "";
					cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = "";
					cARI_HESAP_HAREKETLERI.cha_oivergisiz_fl = false;
					cARI_HESAP_HAREKETLERI.cha_meblag_ana_doviz_icin_gecersiz_fl = 0;
					cARI_HESAP_HAREKETLERI.cha_meblag_alt_doviz_icin_gecersiz_fl = 0;
					cARI_HESAP_HAREKETLERI.cha_meblag_orj_doviz_icin_gecersiz_fl = 0;
					cARI_HESAP_HAREKETLERI.cha_ciroprim_DBCno = 0;
					cARI_HESAP_HAREKETLERI.cha_ciroprim_RECno = 0;
					cARI_HESAP_HAREKETLERI.cha_HalHamaliyeKdv = 0.0;
					cARI_HESAP_HAREKETLERI.cha_HalHamaliyeVergisiz_fl = false;
					cARI_HESAP_HAREKETLERI.cha_bakimhar_DBCno = 0;
					cARI_HESAP_HAREKETLERI.cha_bakimhar_RECno = 0;
					cARI_HESAP_HAREKETLERI.cha_avanstalep_DBCno = 0;
					cARI_HESAP_HAREKETLERI.cha_avanstalep_RECno = 0;
					cARI_HESAP_HAREKETLERI.cha_oiv_pntr = 0;
					cARI_HESAP_HAREKETLERI.cha_oiv_vergi = 0.0;
					cARI_HESAP_HAREKETLERI.cha_oivtutari = 0.0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas1 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas2 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas3 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas4 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas5 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas6 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas7 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas8 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas9 = 0;
					cARI_HESAP_HAREKETLERI.cha_isk_mas10 = 0;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas1 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas2 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas3 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas4 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas5 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas6 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas7 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas8 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas9 = false;
					cARI_HESAP_HAREKETLERI.cha_sat_iskmas10 = false;
					item3.AddGenelCariHesapHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				}
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
		int num = 0;
		foreach (Evrak item in _AktarilacakEvraklar)
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
			int num2 = 0;
			bool flag = true;
			if (cb_belgeno_kontrolu_yap.Checked && EvrakData.BelgeNoVarMi(sqlConnection, item.evraktipi, item.BelgeNo))
			{
				flag = false;
				num2 = -3;
			}
			if (flag)
			{
				SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
				try
				{
					num2 = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, item, EArsivAktif: false);
					sqlTransaction.Commit();
				}
				catch (SqlException ex)
				{
					num2 = -1;
					Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
					sqlTransaction.Rollback();
					MessageBox.Show("Evrak kayıt edilirken bir hata meydana geldi. ErrorCode : " + ex.ErrorCode + " Errors : " + ex.Errors.ToString() + " Line Number : " + ex.LineNumber + " Message : " + ex.Message + " Source : " + ex.Source + " Procedure : " + ex.Procedure + " Hata : " + ex.ToString(), "HATA");
				}
				catch (Exception ex2)
				{
					num2 = -1;
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
			if (num2 <= 0)
			{
				enum_GenelEvrakAktarimDurumu = ((num2 != -2) ? enum_GenelEvrakAktarimDurumu.BeklenmedikHata : enum_GenelEvrakAktarimDurumu.EvrakVar);
			}
			else
			{
				aktar = false;
				enum_GenelEvrakAktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmis;
			}
			if (num2 == -3)
			{
				enum_GenelEvrakAktarimDurumu = enum_GenelEvrakAktarimDurumu.EvrakVar;
			}
			foreach (TahsilatEvrakSatir item2 in _satirlar._satirlar)
			{
				if (item2.Aktar && item2.evraknoseri == item.EvrakNoSeri && item2.evraknosira == item.EvrakNoSira)
				{
					item2.AktarimDurumu = enum_GenelEvrakAktarimDurumu;
					item2.Aktar = aktar;
				}
			}
			num++;
			backgroundWorker_Aktarim.ReportProgress(num);
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
		BindingList<TahsilatEvrakSatir> bindingList = new BindingList<TahsilatEvrakSatir>();
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
			Parametreler parametreler = ParametrelerDefault.TahsilatAktarimTxtCsvSablon(AktarimAyarlariAdi);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "TahsilatAktarim", "", "TxtCsvAktarimSablon", AktarimAyarlariAdi);
			string getString = parametreler._GetParametre("KriterListesi")._GetString;
			int getInt = parametreler._GetParametre("dbc_no")._GetInt;
			string getString2 = parametreler._GetParametre("alinacak_satirlarin_baslangic_karakteri")._GetString;
			bool getBoolean = parametreler._GetParametre("alinacak_satirlarin_baslangic_karakteri_kullan")._GetBoolean;
			int getInt2 = parametreler._GetParametre("bilgilerin_baslangic_satiri")._GetInt;
			bool getBoolean2 = parametreler._GetParametre("bilgilerin_baslangic_satiri_kullan")._GetBoolean;
			string getString3 = parametreler._GetParametre("ayrac_karakteri")._GetString;
			bool getBoolean3 = parametreler._GetParametre("ayrac_karakteri_kullan")._GetBoolean;
			bool getBoolean4 = parametreler._GetParametre("evrakvedetaylarayrisatirlarda")._GetBoolean;
			string getString4 = parametreler._GetParametre("evrakbaslangickarakteri")._GetString;
			string getString5 = parametreler._GetParametre("satirbaslangickarakteri")._GetString;
			int getInt3 = parametreler._GetParametre("firma_no")._GetInt;
			int getInt4 = parametreler._GetParametre("sube_no")._GetInt;
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
			bool getBoolean5 = parametreler._GetParametre("satir_cinsi_sabit_kullan")._GetBoolean;
			int getInt21 = parametreler._GetParametre("satir_cinsi_sabit_deger")._GetInt;
			int getInt22 = parametreler._GetParametre("satir_cinsi_baslangic")._GetInt;
			int getInt23 = parametreler._GetParametre("satir_cinsi_uzunluk")._GetInt;
			string getString6 = parametreler._GetParametre("satir_cinsi_veri_nakit")._GetString;
			string getString7 = parametreler._GetParametre("satir_cinsi_veri_musteri_ceki")._GetString;
			string getString8 = parametreler._GetParametre("satir_cinsi_veri_musteri_seneti")._GetString;
			string getString9 = parametreler._GetParametre("satir_cinsi_veri_musteri_kredi_karti")._GetString;
			string getString10 = parametreler._GetParametre("satir_cinsi_veri_gelen_havale")._GetString;
			string getString11 = parametreler._GetParametre("satir_cinsi_veri_giden_havale")._GetString;
			bool getBoolean6 = parametreler._GetParametre("satir_hesap_kodu_nakit_sabit_kullan")._GetBoolean;
			string getString12 = parametreler._GetParametre("satir_hesap_kodu_nakit_sabit_deger")._GetString;
			int getInt24 = parametreler._GetParametre("satir_hesap_kodu_nakit_baslangic")._GetInt;
			int getInt25 = parametreler._GetParametre("satir_hesap_kodu_nakit_uzunluk")._GetInt;
			string getString13 = parametreler._GetParametre("satir_hesap_kodu_nakit_on_ek")._GetString;
			string getString14 = parametreler._GetParametre("satir_hesap_kodu_nakit_son_ek")._GetString;
			bool getBoolean7 = parametreler._GetParametre("satir_hesap_kodu_cek_sabit_kullan")._GetBoolean;
			string getString15 = parametreler._GetParametre("satir_hesap_kodu_cek_sabit_deger")._GetString;
			int getInt26 = parametreler._GetParametre("satir_hesap_kodu_cek_baslangic")._GetInt;
			int getInt27 = parametreler._GetParametre("satir_hesap_kodu_cek_uzunluk")._GetInt;
			string getString16 = parametreler._GetParametre("satir_hesap_kodu_cek_on_ek")._GetString;
			string getString17 = parametreler._GetParametre("satir_hesap_kodu_cek_son_ek")._GetString;
			bool getBoolean8 = parametreler._GetParametre("satir_hesap_kodu_senet_sabit_kullan")._GetBoolean;
			string getString18 = parametreler._GetParametre("satir_hesap_kodu_senet_sabit_deger")._GetString;
			int getInt28 = parametreler._GetParametre("satir_hesap_kodu_senet_baslangic")._GetInt;
			int getInt29 = parametreler._GetParametre("satir_hesap_kodu_senet_uzunluk")._GetInt;
			string getString19 = parametreler._GetParametre("satir_hesap_kodu_senet_on_ek")._GetString;
			string getString20 = parametreler._GetParametre("satir_hesap_kodu_senet_son_ek")._GetString;
			bool getBoolean9 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_sabit_kullan")._GetBoolean;
			string getString21 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_sabit_deger")._GetString;
			int getInt30 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_baslangic")._GetInt;
			int getInt31 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_uzunluk")._GetInt;
			string getString22 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_on_ek")._GetString;
			string getString23 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_son_ek")._GetString;
			bool getBoolean10 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_sabit_kullan")._GetBoolean;
			string getString24 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_sabit_deger")._GetString;
			int getInt32 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_baslangic")._GetInt;
			int getInt33 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_uzunluk")._GetInt;
			string getString25 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_on_ek")._GetString;
			string getString26 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_son_ek")._GetString;
			bool getBoolean11 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_sabit_kullan")._GetBoolean;
			string getString27 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_sabit_deger")._GetString;
			int getInt34 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_baslangic")._GetInt;
			int getInt35 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_uzunluk")._GetInt;
			string getString28 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_on_ek")._GetString;
			string getString29 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_son_ek")._GetString;
			bool getBoolean12 = parametreler._GetParametre("cari_kod_sabit_kullan")._GetBoolean;
			string getString30 = parametreler._GetParametre("cari_kod_sabit_deger")._GetString;
			int getInt36 = parametreler._GetParametre("cari_kod_baslangic")._GetInt;
			int getInt37 = parametreler._GetParametre("cari_kod_uzunluk")._GetInt;
			string getString31 = parametreler._GetParametre("cari_arama_secenekleri")._GetString;
			_ = parametreler._GetParametre("cari_unvan_turkce_karakterleri_kaldir")._GetBoolean;
			int getInt38 = parametreler._GetParametre("cari_unvan_baslangic")._GetInt;
			int getInt39 = parametreler._GetParametre("cari_unvan_uzunluk")._GetInt;
			int getInt40 = parametreler._GetParametre("cari_unvan2_baslangic")._GetInt;
			int getInt41 = parametreler._GetParametre("cari_unvan2_uzunluk")._GetInt;
			int getInt42 = parametreler._GetParametre("cari_vergi_no_baslangic")._GetInt;
			int getInt43 = parametreler._GetParametre("cari_vergi_no_uzunluk")._GetInt;
			int getInt44 = parametreler._GetParametre("cari_tc_kimlik_no_baslangic")._GetInt;
			int getInt45 = parametreler._GetParametre("cari_tc_kimlik_no_uzunluk")._GetInt;
			int getInt46 = parametreler._GetParametre("cari_vergi_dairesi_baslangic")._GetInt;
			int getInt47 = parametreler._GetParametre("cari_vergi_dairesi_uzunluk")._GetInt;
			int getInt48 = parametreler._GetParametre("cari_banka_hesap_no_baslangic")._GetInt;
			int getInt49 = parametreler._GetParametre("cari_banka_hesap_no_uzunluk")._GetInt;
			int getInt50 = parametreler._GetParametre("cari_adres_baslangic")._GetInt;
			int getInt51 = parametreler._GetParametre("cari_adres_uzunluk")._GetInt;
			int getInt52 = parametreler._GetParametre("cari_mahalle_baslangic")._GetInt;
			int getInt53 = parametreler._GetParametre("cari_mahalle_uzunluk")._GetInt;
			int getInt54 = parametreler._GetParametre("cari_ilce_baslangic")._GetInt;
			int getInt55 = parametreler._GetParametre("cari_ilce_uzunluk")._GetInt;
			int getInt56 = parametreler._GetParametre("cari_il_baslangic")._GetInt;
			int getInt57 = parametreler._GetParametre("cari_il_uzunluk")._GetInt;
			int getInt58 = parametreler._GetParametre("cari_ulke_baslangic")._GetInt;
			int getInt59 = parametreler._GetParametre("cari_ulke_uzunluk")._GetInt;
			int getInt60 = parametreler._GetParametre("cari_posta_kodu_baslangic")._GetInt;
			int getInt61 = parametreler._GetParametre("cari_posta_kodu_uzunluk")._GetInt;
			int getInt62 = parametreler._GetParametre("cari_telefon_baslangic")._GetInt;
			int getInt63 = parametreler._GetParametre("cari_telefon_uzunluk")._GetInt;
			int getInt64 = parametreler._GetParametre("cari_eposta_baslangic")._GetInt;
			int getInt65 = parametreler._GetParametre("cari_eposta_uzunluk")._GetInt;
			bool getBoolean13 = parametreler._GetParametre("proje_kodu_sabit_kullan")._GetBoolean;
			string getString32 = parametreler._GetParametre("proje_kodu_sabit_deger")._GetString;
			int getInt66 = parametreler._GetParametre("proje_kodu_baslangic")._GetInt;
			int getInt67 = parametreler._GetParametre("proje_kodu_uzunluk")._GetInt;
			bool getBoolean14 = parametreler._GetParametre("sor_mer_kodu_sabit_kullan")._GetBoolean;
			string getString33 = parametreler._GetParametre("sor_mer_kodu_sabit_deger")._GetString;
			int getInt68 = parametreler._GetParametre("sor_mer_kodu_baslangic")._GetInt;
			int getInt69 = parametreler._GetParametre("sor_mer_kodu_uzunluk")._GetInt;
			bool getBoolean15 = parametreler._GetParametre("plasiyer_kodu_sabit_kullan")._GetBoolean;
			bool getBoolean16 = parametreler._GetParametre("plasiyer_kodu_cariden_kullan")._GetBoolean;
			string getString34 = parametreler._GetParametre("plasiyer_kodu_sabit_deger")._GetString;
			int getInt70 = parametreler._GetParametre("plasiyer_kodu_baslangic")._GetInt;
			int getInt71 = parametreler._GetParametre("plasiyer_kodu_uzunluk")._GetInt;
			bool getBoolean17 = parametreler._GetParametre("satir_aciklama_sabit_kullan")._GetBoolean;
			string getString35 = parametreler._GetParametre("satir_aciklama_sabit_deger")._GetString;
			int getInt72 = parametreler._GetParametre("satir_aciklama_baslangic")._GetInt;
			int getInt73 = parametreler._GetParametre("satir_aciklama_uzunluk")._GetInt;
			bool getBoolean18 = parametreler._GetParametre("aciklama1_sabit_kullan")._GetBoolean;
			string getString36 = parametreler._GetParametre("aciklama1_sabit_deger")._GetString;
			int getInt74 = parametreler._GetParametre("aciklama1_baslangic")._GetInt;
			int getInt75 = parametreler._GetParametre("aciklama1_uzunluk")._GetInt;
			bool getBoolean19 = parametreler._GetParametre("aciklama2_sabit_kullan")._GetBoolean;
			string getString37 = parametreler._GetParametre("aciklama2_sabit_deger")._GetString;
			int getInt76 = parametreler._GetParametre("aciklama2_baslangic")._GetInt;
			int getInt77 = parametreler._GetParametre("aciklama2_uzunluk")._GetInt;
			bool getBoolean20 = parametreler._GetParametre("aciklama3_sabit_kullan")._GetBoolean;
			string getString38 = parametreler._GetParametre("aciklama3_sabit_deger")._GetString;
			int getInt78 = parametreler._GetParametre("aciklama3_baslangic")._GetInt;
			int getInt79 = parametreler._GetParametre("aciklama3_uzunluk")._GetInt;
			bool getBoolean21 = parametreler._GetParametre("aciklama4_sabit_kullan")._GetBoolean;
			string getString39 = parametreler._GetParametre("aciklama4_sabit_deger")._GetString;
			int getInt80 = parametreler._GetParametre("aciklama4_baslangic")._GetInt;
			int getInt81 = parametreler._GetParametre("aciklama4_uzunluk")._GetInt;
			bool getBoolean22 = parametreler._GetParametre("aciklama5_sabit_kullan")._GetBoolean;
			string getString40 = parametreler._GetParametre("aciklama5_sabit_deger")._GetString;
			int getInt82 = parametreler._GetParametre("aciklama5_baslangic")._GetInt;
			int getInt83 = parametreler._GetParametre("aciklama5_uzunluk")._GetInt;
			bool getBoolean23 = parametreler._GetParametre("aciklama6_sabit_kullan")._GetBoolean;
			string getString41 = parametreler._GetParametre("aciklama6_sabit_deger")._GetString;
			int getInt84 = parametreler._GetParametre("aciklama6_baslangic")._GetInt;
			int getInt85 = parametreler._GetParametre("aciklama6_uzunluk")._GetInt;
			bool getBoolean24 = parametreler._GetParametre("aciklama7_sabit_kullan")._GetBoolean;
			string getString42 = parametreler._GetParametre("aciklama7_sabit_deger")._GetString;
			int getInt86 = parametreler._GetParametre("aciklama7_baslangic")._GetInt;
			int getInt87 = parametreler._GetParametre("aciklama7_uzunluk")._GetInt;
			bool getBoolean25 = parametreler._GetParametre("aciklama8_sabit_kullan")._GetBoolean;
			string getString43 = parametreler._GetParametre("aciklama8_sabit_deger")._GetString;
			int getInt88 = parametreler._GetParametre("aciklama8_baslangic")._GetInt;
			int getInt89 = parametreler._GetParametre("aciklama8_uzunluk")._GetInt;
			bool getBoolean26 = parametreler._GetParametre("aciklama9_sabit_kullan")._GetBoolean;
			string getString44 = parametreler._GetParametre("aciklama9_sabit_deger")._GetString;
			int getInt90 = parametreler._GetParametre("aciklama9_baslangic")._GetInt;
			int getInt91 = parametreler._GetParametre("aciklama9_uzunluk")._GetInt;
			bool getBoolean27 = parametreler._GetParametre("aciklama10_sabit_kullan")._GetBoolean;
			string getString45 = parametreler._GetParametre("aciklama10_sabit_deger")._GetString;
			int getInt92 = parametreler._GetParametre("aciklama10_baslangic")._GetInt;
			int getInt93 = parametreler._GetParametre("aciklama10_uzunluk")._GetInt;
			bool getBoolean28 = parametreler._GetParametre("ozel_alan_1_sabit_kullan")._GetBoolean;
			string getString46 = parametreler._GetParametre("ozel_alan_1_sabit_deger")._GetString;
			int getInt94 = parametreler._GetParametre("ozel_alan_1_baslangic")._GetInt;
			int getInt95 = parametreler._GetParametre("ozel_alan_1_uzunluk")._GetInt;
			bool getBoolean29 = parametreler._GetParametre("ozel_alan_2_sabit_kullan")._GetBoolean;
			string getString47 = parametreler._GetParametre("ozel_alan_2_sabit_deger")._GetString;
			int getInt96 = parametreler._GetParametre("ozel_alan_2_baslangic")._GetInt;
			int getInt97 = parametreler._GetParametre("ozel_alan_2_uzunluk")._GetInt;
			bool getBoolean30 = parametreler._GetParametre("ozel_alan_3_sabit_kullan")._GetBoolean;
			string getString48 = parametreler._GetParametre("ozel_alan_3_sabit_deger")._GetString;
			int getInt98 = parametreler._GetParametre("ozel_alan_3_baslangic")._GetInt;
			int getInt99 = parametreler._GetParametre("ozel_alan_3_uzunluk")._GetInt;
			bool getBoolean31 = parametreler._GetParametre("evrak_seri_sabit_kullan")._GetBoolean;
			string getString49 = parametreler._GetParametre("evrak_seri_sabit_deger")._GetString;
			int getInt100 = parametreler._GetParametre("evrak_seri_baslangic")._GetInt;
			int getInt101 = parametreler._GetParametre("evrak_seri_uzunluk")._GetInt;
			int getInt102 = parametreler._GetParametre("evrak_sira_baslangic")._GetInt;
			int getInt103 = parametreler._GetParametre("evrak_sira_uzunluk")._GetInt;
			bool getBoolean32 = parametreler._GetParametre("cari_kodu_on_ek_kullan")._GetBoolean;
			string getString50 = parametreler._GetParametre("cari_kodu_on_ek_satis")._GetString;
			_ = parametreler._GetParametre("cari_kodu_on_ek_alis")._GetString;
			bool getBoolean33 = parametreler._GetParametre("satir_aciklama_on_ek_kullan")._GetBoolean;
			string getString51 = parametreler._GetParametre("satir_aciklama_on_ek_deger")._GetString;
			bool getBoolean34 = parametreler._GetParametre("aciklama1_on_ek_kullan")._GetBoolean;
			string getString52 = parametreler._GetParametre("aciklama1_on_ek_deger")._GetString;
			bool getBoolean35 = parametreler._GetParametre("aciklama2_on_ek_kullan")._GetBoolean;
			string getString53 = parametreler._GetParametre("aciklama2_on_ek_deger")._GetString;
			bool getBoolean36 = parametreler._GetParametre("aciklama3_on_ek_kullan")._GetBoolean;
			string getString54 = parametreler._GetParametre("aciklama3_on_ek_deger")._GetString;
			bool getBoolean37 = parametreler._GetParametre("aciklama4_on_ek_kullan")._GetBoolean;
			string getString55 = parametreler._GetParametre("aciklama4_on_ek_deger")._GetString;
			bool getBoolean38 = parametreler._GetParametre("aciklama5_on_ek_kullan")._GetBoolean;
			string getString56 = parametreler._GetParametre("aciklama5_on_ek_deger")._GetString;
			bool getBoolean39 = parametreler._GetParametre("aciklama6_on_ek_kullan")._GetBoolean;
			string getString57 = parametreler._GetParametre("aciklama6_on_ek_deger")._GetString;
			bool getBoolean40 = parametreler._GetParametre("aciklama7_on_ek_kullan")._GetBoolean;
			string getString58 = parametreler._GetParametre("aciklama7_on_ek_deger")._GetString;
			bool getBoolean41 = parametreler._GetParametre("aciklama8_on_ek_kullan")._GetBoolean;
			string getString59 = parametreler._GetParametre("aciklama8_on_ek_deger")._GetString;
			bool getBoolean42 = parametreler._GetParametre("aciklama9_on_ek_kullan")._GetBoolean;
			string getString60 = parametreler._GetParametre("aciklama9_on_ek_deger")._GetString;
			bool getBoolean43 = parametreler._GetParametre("aciklama10_on_ek_kullan")._GetBoolean;
			string getString61 = parametreler._GetParametre("aciklama10_on_ek_deger")._GetString;
			bool getBoolean44 = parametreler._GetParametre("cari_il_bilgisi_plaka_kodu")._GetBoolean;
			int getInt104 = parametreler._GetParametre("cari_doviz_cinsi")._GetInt;
			int getInt105 = parametreler._GetParametre("cari_doviz_cinsi1")._GetInt;
			int getInt106 = parametreler._GetParametre("cari_doviz_cinsi2")._GetInt;
			string getString62 = parametreler._GetParametre("cari_muh_kod_satis")._GetString;
			string getString63 = parametreler._GetParametre("cari_muh_kod1_satis")._GetString;
			string getString64 = parametreler._GetParametre("cari_muh_kod2_satis")._GetString;
			string getString65 = parametreler._GetParametre("cari_muhartikeli")._GetString;
			int getInt107 = parametreler._GetParametre("cari_kod2_baslangic")._GetInt;
			int getInt108 = parametreler._GetParametre("cari_kod2_uzunluk")._GetInt;
			int getInt109 = parametreler._GetParametre("kriter_metin1_baslangic")._GetInt;
			int getInt110 = parametreler._GetParametre("kriter_metin1_uzunluk")._GetInt;
			int getInt111 = parametreler._GetParametre("kriter_metin2_baslangic")._GetInt;
			int getInt112 = parametreler._GetParametre("kriter_metin2_uzunluk")._GetInt;
			int getInt113 = parametreler._GetParametre("kriter_metin3_baslangic")._GetInt;
			int getInt114 = parametreler._GetParametre("kriter_metin3_uzunluk")._GetInt;
			int getInt115 = parametreler._GetParametre("kriter_metin4_baslangic")._GetInt;
			int getInt116 = parametreler._GetParametre("kriter_metin4_uzunluk")._GetInt;
			int getInt117 = parametreler._GetParametre("kriter_metin5_baslangic")._GetInt;
			int getInt118 = parametreler._GetParametre("kriter_metin5_uzunluk")._GetInt;
			int getInt119 = parametreler._GetParametre("kriter_double1_baslangic")._GetInt;
			int getInt120 = parametreler._GetParametre("kriter_double1_uzunluk")._GetInt;
			int getInt121 = parametreler._GetParametre("kriter_double2_baslangic")._GetInt;
			int getInt122 = parametreler._GetParametre("kriter_double2_uzunluk")._GetInt;
			int getInt123 = parametreler._GetParametre("kriter_double3_baslangic")._GetInt;
			int getInt124 = parametreler._GetParametre("kriter_double3_uzunluk")._GetInt;
			int getInt125 = parametreler._GetParametre("kriter_double4_baslangic")._GetInt;
			int getInt126 = parametreler._GetParametre("kriter_double4_uzunluk")._GetInt;
			int getInt127 = parametreler._GetParametre("kriter_double5_baslangic")._GetInt;
			int getInt128 = parametreler._GetParametre("kriter_double5_uzunluk")._GetInt;
			int getInt129 = parametreler._GetParametre("kriter_bool1_baslangic")._GetInt;
			int getInt130 = parametreler._GetParametre("kriter_bool1_uzunluk")._GetInt;
			string getString66 = parametreler._GetParametre("kriter_bool1_evet_icin_deger")._GetString;
			int getInt131 = parametreler._GetParametre("kriter_bool2_baslangic")._GetInt;
			int getInt132 = parametreler._GetParametre("kriter_bool2_uzunluk")._GetInt;
			string getString67 = parametreler._GetParametre("kriter_bool2_evet_icin_deger")._GetString;
			int getInt133 = parametreler._GetParametre("kriter_bool3_baslangic")._GetInt;
			int getInt134 = parametreler._GetParametre("kriter_bool3_uzunluk")._GetInt;
			string getString68 = parametreler._GetParametre("kriter_bool3_evet_icin_deger")._GetString;
			int getInt135 = parametreler._GetParametre("kriter_bool4_baslangic")._GetInt;
			int getInt136 = parametreler._GetParametre("kriter_bool4_uzunluk")._GetInt;
			string getString69 = parametreler._GetParametre("kriter_bool4_evet_icin_deger")._GetString;
			int getInt137 = parametreler._GetParametre("kriter_bool5_baslangic")._GetInt;
			int getInt138 = parametreler._GetParametre("kriter_bool5_uzunluk")._GetInt;
			string getString70 = parametreler._GetParametre("kriter_bool5_evet_icin_deger")._GetString;
			bool getBoolean45 = parametreler._GetParametre("kayit_id_otomatik_ver")._GetBoolean;
			int getInt139 = parametreler._GetParametre("kayit_id_baslangic")._GetInt;
			int getInt140 = parametreler._GetParametre("kayit_id_uzunluk")._GetInt;
			int getInt141 = parametreler._GetParametre("otomatik_hesap_acma_secenek_cari")._GetInt;
			int getInt142 = parametreler._GetParametre("otomatik_hesap_acma_secenek_proje")._GetInt;
			int getInt143 = parametreler._GetParametre("otomatik_hesap_acma_secenek_sorumluluk")._GetInt;
			flag = parametreler._GetParametre("evrak_sira_otomatik_ver")._GetBoolean;
			bool getBoolean46 = parametreler._GetParametre("pro_adi_sabit_kullan")._GetBoolean;
			string getString71 = parametreler._GetParametre("pro_adi_sabit_deger")._GetString;
			int getInt144 = parametreler._GetParametre("pro_adi_baslangic")._GetInt;
			int getInt145 = parametreler._GetParametre("pro_adi_uzunluk")._GetInt;
			bool getBoolean47 = parametreler._GetParametre("pro_musterikodu_sabit_kullan")._GetBoolean;
			string getString72 = parametreler._GetParametre("pro_musterikodu_sabit_deger")._GetString;
			int getInt146 = parametreler._GetParametre("pro_musterikodu_baslangic")._GetInt;
			int getInt147 = parametreler._GetParametre("pro_musterikodu_uzunluk")._GetInt;
			bool getBoolean48 = parametreler._GetParametre("pro_sormerkodu_sabit_kullan")._GetBoolean;
			string getString73 = parametreler._GetParametre("pro_sormerkodu_sabit_deger")._GetString;
			int getInt148 = parametreler._GetParametre("pro_sormerkodu_baslangic")._GetInt;
			int getInt149 = parametreler._GetParametre("pro_sormerkodu_uzunluk")._GetInt;
			bool getBoolean49 = parametreler._GetParametre("pro_grupkodu_sabit_kullan")._GetBoolean;
			string getString74 = parametreler._GetParametre("pro_grupkodu_sabit_deger")._GetString;
			int getInt150 = parametreler._GetParametre("pro_grupkodu_baslangic")._GetInt;
			int getInt151 = parametreler._GetParametre("pro_grupkodu_uzunluk")._GetInt;
			bool getBoolean50 = parametreler._GetParametre("pro_sektorkodu_sabit_kullan")._GetBoolean;
			string getString75 = parametreler._GetParametre("pro_sektorkodu_sabit_deger")._GetString;
			int getInt152 = parametreler._GetParametre("pro_sektorkodu_baslangic")._GetInt;
			int getInt153 = parametreler._GetParametre("pro_sektorkodu_uzunluk")._GetInt;
			bool getBoolean51 = parametreler._GetParametre("pro_bolgekodu_sabit_kullan")._GetBoolean;
			string getString76 = parametreler._GetParametre("pro_bolgekodu_sabit_deger")._GetString;
			int getInt154 = parametreler._GetParametre("pro_bolgekodu_baslangic")._GetInt;
			int getInt155 = parametreler._GetParametre("pro_bolgekodu_uzunluk")._GetInt;
			bool getBoolean52 = parametreler._GetParametre("pro_ana_projekodu_sabit_kullan")._GetBoolean;
			string getString77 = parametreler._GetParametre("pro_ana_projekodu_sabit_deger")._GetString;
			int getInt156 = parametreler._GetParametre("pro_ana_projekodu_baslangic")._GetInt;
			int getInt157 = parametreler._GetParametre("pro_ana_projekodu_uzunluk")._GetInt;
			bool getBoolean53 = parametreler._GetParametre("pro_aciklama_sabit_kullan")._GetBoolean;
			string getString78 = parametreler._GetParametre("pro_aciklama_sabit_deger")._GetString;
			int getInt158 = parametreler._GetParametre("pro_aciklama_baslangic")._GetInt;
			int getInt159 = parametreler._GetParametre("pro_aciklama_uzunluk")._GetInt;
			bool getBoolean54 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_kullan")._GetBoolean;
			string getString79 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_deger")._GetString;
			int getInt160 = parametreler._GetParametre("pro_muh_kod_artikeli_baslangic")._GetInt;
			int getInt161 = parametreler._GetParametre("pro_muh_kod_artikeli_uzunluk")._GetInt;
			bool getBoolean55 = parametreler._GetParametre("som_isim_sabit_kullan")._GetBoolean;
			string getString80 = parametreler._GetParametre("som_isim_sabit_deger")._GetString;
			int getInt162 = parametreler._GetParametre("som_isim_baslangic")._GetInt;
			int getInt163 = parametreler._GetParametre("som_isim_uzunluk")._GetInt;
			bool getBoolean56 = parametreler._GetParametre("som_MuhArtikeli_sabit_kullan")._GetBoolean;
			string getString81 = parametreler._GetParametre("som_MuhArtikeli_sabit_deger")._GetString;
			int getInt164 = parametreler._GetParametre("som_MuhArtikeli_baslangic")._GetInt;
			int getInt165 = parametreler._GetParametre("som_MuhArtikeli_uzunluk")._GetInt;
			bool getBoolean57 = parametreler._GetParametre("cari_muhartikeli_sabit_kullan")._GetBoolean;
			int getInt166 = parametreler._GetParametre("cari_muhartikeli_baslangic")._GetInt;
			int getInt167 = parametreler._GetParametre("cari_muhartikeli_uzunluk")._GetInt;
			bool getBoolean58 = parametreler._GetParametre("cari_muh_kod_satis_sabit_kullan")._GetBoolean;
			int getInt168 = parametreler._GetParametre("cari_muh_kod_satis_baslangic")._GetInt;
			int getInt169 = parametreler._GetParametre("cari_muh_kod_satis_uzunluk")._GetInt;
			bool getBoolean59 = parametreler._GetParametre("cari_muh_kod1_satis_sabit_kullan")._GetBoolean;
			int getInt170 = parametreler._GetParametre("cari_muh_kod1_satis_baslangic")._GetInt;
			int getInt171 = parametreler._GetParametre("cari_muh_kod1_satis_uzunluk")._GetInt;
			bool getBoolean60 = parametreler._GetParametre("cari_muh_kod2_satis_sabit_kullan")._GetBoolean;
			int getInt172 = parametreler._GetParametre("cari_muh_kod2_satis_baslangic")._GetInt;
			int getInt173 = parametreler._GetParametre("cari_muh_kod2_satis_uzunluk")._GetInt;
			bool getBoolean61 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_kullan")._GetBoolean;
			string getString82 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_deger")._GetString;
			int getInt174 = parametreler._GetParametre("cari_Ana_cari_kodu_baslangic")._GetInt;
			int getInt175 = parametreler._GetParametre("cari_Ana_cari_kodu_uzunluk")._GetInt;
			bool getBoolean62 = parametreler._GetParametre("cari_temsilci_kodu_sabit_kullan")._GetBoolean;
			string getString83 = parametreler._GetParametre("cari_temsilci_kodu_sabit_deger")._GetString;
			int getInt176 = parametreler._GetParametre("cari_temsilci_kodu_baslangic")._GetInt;
			int getInt177 = parametreler._GetParametre("cari_temsilci_kodu_uzunluk")._GetInt;
			bool getBoolean63 = parametreler._GetParametre("cari_grup_kodu_sabit_kullan")._GetBoolean;
			string getString84 = parametreler._GetParametre("cari_grup_kodu_sabit_deger")._GetString;
			int getInt178 = parametreler._GetParametre("cari_grup_kodu_baslangic")._GetInt;
			int getInt179 = parametreler._GetParametre("cari_grup_kodu_uzunluk")._GetInt;
			bool getBoolean64 = parametreler._GetParametre("cari_sektor_kodu_sabit_kullan")._GetBoolean;
			string getString85 = parametreler._GetParametre("cari_sektor_kodu_sabit_deger")._GetString;
			int getInt180 = parametreler._GetParametre("cari_sektor_kodu_baslangic")._GetInt;
			int getInt181 = parametreler._GetParametre("cari_sektor_kodu_uzunluk")._GetInt;
			bool getBoolean65 = parametreler._GetParametre("cari_bolge_kodu_sabit_kullan")._GetBoolean;
			string getString86 = parametreler._GetParametre("cari_bolge_kodu_sabit_deger")._GetString;
			int getInt182 = parametreler._GetParametre("cari_bolge_kodu_baslangic")._GetInt;
			int getInt183 = parametreler._GetParametre("cari_bolge_kodu_uzunluk")._GetInt;
			bool getBoolean66 = parametreler._GetParametre("cari_wwwadresi_sabit_kullan")._GetBoolean;
			string getString87 = parametreler._GetParametre("cari_wwwadresi_sabit_deger")._GetString;
			int getInt184 = parametreler._GetParametre("cari_wwwadresi_baslangic")._GetInt;
			int getInt185 = parametreler._GetParametre("cari_wwwadresi_uzunluk")._GetInt;
			bool getBoolean67 = parametreler._GetParametre("cari_CepTel_sabit_kullan")._GetBoolean;
			string getString88 = parametreler._GetParametre("cari_CepTel_sabit_deger")._GetString;
			int getInt186 = parametreler._GetParametre("cari_CepTel_baslangic")._GetInt;
			int getInt187 = parametreler._GetParametre("cari_CepTel_uzunluk")._GetInt;
			bool getBoolean68 = parametreler._GetParametre("cari_satis_isk_kod_sabit_kullan")._GetBoolean;
			string getString89 = parametreler._GetParametre("cari_satis_isk_kod_sabit_deger")._GetString;
			int getInt188 = parametreler._GetParametre("cari_satis_isk_kod_baslangic")._GetInt;
			int getInt189 = parametreler._GetParametre("cari_satis_isk_kod_uzunluk")._GetInt;
			bool getBoolean69 = parametreler._GetParametre("cari_sicil_no_sabit_kullan")._GetBoolean;
			string getString90 = parametreler._GetParametre("cari_sicil_no_sabit_deger")._GetString;
			int getInt190 = parametreler._GetParametre("cari_sicil_no_baslangic")._GetInt;
			int getInt191 = parametreler._GetParametre("cari_sicil_no_uzunluk")._GetInt;
			bool getBoolean70 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_kullan")._GetBoolean;
			string getString91 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_deger")._GetString;
			int getInt192 = parametreler._GetParametre("cari_VarsayilanGirisDepo_baslangic")._GetInt;
			int getInt193 = parametreler._GetParametre("cari_VarsayilanGirisDepo_uzunluk")._GetInt;
			bool getBoolean71 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_kullan")._GetBoolean;
			string getString92 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_deger")._GetString;
			int getInt194 = parametreler._GetParametre("cari_VarsayilanCikisDepo_baslangic")._GetInt;
			int getInt195 = parametreler._GetParametre("cari_VarsayilanCikisDepo_uzunluk")._GetInt;
			bool getBoolean72 = parametreler._GetParametre("cari_Portal_PW_sabit_kullan")._GetBoolean;
			string getString93 = parametreler._GetParametre("cari_Portal_PW_sabit_deger")._GetString;
			int getInt196 = parametreler._GetParametre("cari_Portal_PW_baslangic")._GetInt;
			int getInt197 = parametreler._GetParametre("cari_Portal_PW_uzunluk")._GetInt;
			bool getBoolean73 = parametreler._GetParametre("cari_Portal_Enabled")._GetBoolean;
			bool getBoolean74 = parametreler._GetParametre("satir_tutar_sabit_kullan")._GetBoolean;
			string getString94 = parametreler._GetParametre("satir_tutar_sabit_deger")._GetString;
			int getInt198 = parametreler._GetParametre("satir_tutar_baslangic")._GetInt;
			int getInt199 = parametreler._GetParametre("satir_tutar_uzunluk")._GetInt;
			int getInt200 = parametreler._GetParametre("satir_tutar_islem_1_islem_tipi")._GetInt;
			int getInt201 = parametreler._GetParametre("satir_tutar_islem_1_baslangic")._GetInt;
			_ = parametreler._GetParametre("satir_tutar_islem_1_uzunluk")._GetInt;
			int getInt202 = parametreler._GetParametre("satir_tutar_islem_2_islem_tipi")._GetInt;
			int getInt203 = parametreler._GetParametre("satir_tutar_islem_2_baslangic")._GetInt;
			_ = parametreler._GetParametre("satir_tutar_islem_2_uzunluk")._GetInt;
			bool getBoolean75 = parametreler._GetParametre("satir_vadesi_sabit_kullan")._GetBoolean;
			string getString95 = parametreler._GetParametre("satir_vadesi_sabit_deger")._GetString;
			int getInt204 = parametreler._GetParametre("satir_vadesi_yil_baslangic")._GetInt;
			int getInt205 = parametreler._GetParametre("satir_vadesi_yil_uzunluk")._GetInt;
			int getInt206 = parametreler._GetParametre("satir_vadesi_ay_baslangic")._GetInt;
			int getInt207 = parametreler._GetParametre("satir_vadesi_ay_uzunluk")._GetInt;
			int getInt208 = parametreler._GetParametre("satir_vadesi_gun_baslangic")._GetInt;
			int getInt209 = parametreler._GetParametre("satir_vadesi_gun_uzunluk")._GetInt;
			bool getBoolean76 = parametreler._GetParametre("satir_sorumlulukmerkezi_sabit_kullan")._GetBoolean;
			string getString96 = parametreler._GetParametre("satir_sorumlulukmerkezi_sabit_deger")._GetString;
			int getInt210 = parametreler._GetParametre("satir_sorumlulukmerkezi_baslangic")._GetInt;
			int getInt211 = parametreler._GetParametre("satir_sorumlulukmerkezi_uzunluk")._GetInt;
			bool getBoolean77 = parametreler._GetParametre("satir_referans_sabit_kullan")._GetBoolean;
			string getString97 = parametreler._GetParametre("satir_referans_sabit_deger")._GetString;
			int getInt212 = parametreler._GetParametre("satir_referans_baslangic")._GetInt;
			int getInt213 = parametreler._GetParametre("satir_referans_uzunluk")._GetInt;
			bool getBoolean78 = parametreler._GetParametre("satir_sck_banka_adres1_sabit_kullan")._GetBoolean;
			string getString98 = parametreler._GetParametre("satir_sck_banka_adres1_sabit_deger")._GetString;
			int getInt214 = parametreler._GetParametre("satir_sck_banka_adres1_baslangic")._GetInt;
			int getInt215 = parametreler._GetParametre("satir_sck_banka_adres1_uzunluk")._GetInt;
			bool getBoolean79 = parametreler._GetParametre("satir_sck_bankano_sabit_kullan")._GetBoolean;
			string getString99 = parametreler._GetParametre("satir_sck_bankano_sabit_deger")._GetString;
			int getInt216 = parametreler._GetParametre("satir_sck_bankano_baslangic")._GetInt;
			int getInt217 = parametreler._GetParametre("satir_sck_bankano_uzunluk")._GetInt;
			bool getBoolean80 = parametreler._GetParametre("satir_sck_borclu_sabit_kullan")._GetBoolean;
			string getString100 = parametreler._GetParametre("satir_sck_borclu_sabit_deger")._GetString;
			int getInt218 = parametreler._GetParametre("satir_sck_borclu_baslangic")._GetInt;
			int getInt219 = parametreler._GetParametre("satir_sck_borclu_uzunluk")._GetInt;
			bool getBoolean81 = parametreler._GetParametre("satir_sck_hesapno_sehir_sabit_kullan")._GetBoolean;
			string getString101 = parametreler._GetParametre("satir_sck_hesapno_sehir_sabit_deger")._GetString;
			int getInt220 = parametreler._GetParametre("satir_sck_hesapno_sehir_baslangic")._GetInt;
			int getInt221 = parametreler._GetParametre("satir_sck_hesapno_sehir_uzunluk")._GetInt;
			bool getBoolean82 = parametreler._GetParametre("satir_sck_no_sabit_kullan")._GetBoolean;
			string getString102 = parametreler._GetParametre("satir_sck_no_sabit_deger")._GetString;
			int getInt222 = parametreler._GetParametre("satir_sck_no_baslangic")._GetInt;
			int getInt223 = parametreler._GetParametre("satir_sck_no_uzunluk")._GetInt;
			bool getBoolean83 = parametreler._GetParametre("satir_sck_sube_adres2_sabit_kullan")._GetBoolean;
			string getString103 = parametreler._GetParametre("satir_sck_sube_adres2_sabit_deger")._GetString;
			int getInt224 = parametreler._GetParametre("satir_sck_sube_adres2_baslangic")._GetInt;
			int getInt225 = parametreler._GetParametre("satir_sck_sube_adres2_uzunluk")._GetInt;
			bool getBoolean84 = parametreler._GetParametre("satir_sck_TCMB_Banka_kodu_sabit_kullan")._GetBoolean;
			string getString104 = parametreler._GetParametre("satir_sck_TCMB_Banka_kodu_sabit_deger")._GetString;
			int getInt226 = parametreler._GetParametre("satir_sck_TCMB_Banka_kodu_baslangic")._GetInt;
			int getInt227 = parametreler._GetParametre("satir_sck_TCMB_Banka_kodu_uzunluk")._GetInt;
			bool getBoolean85 = parametreler._GetParametre("satir_sck_TCMB_il_kodu_sabit_kullan")._GetBoolean;
			string getString105 = parametreler._GetParametre("satir_sck_TCMB_il_kodu_sabit_deger")._GetString;
			int getInt228 = parametreler._GetParametre("satir_sck_TCMB_il_kodu_baslangic")._GetInt;
			int getInt229 = parametreler._GetParametre("satir_sck_TCMB_il_kodu_uzunluk")._GetInt;
			bool getBoolean86 = parametreler._GetParametre("satir_sck_TCMB_Sube_kodu_sabit_kullan")._GetBoolean;
			string getString106 = parametreler._GetParametre("satir_sck_TCMB_Sube_kodu_sabit_deger")._GetString;
			int getInt230 = parametreler._GetParametre("satir_sck_TCMB_Sube_kodu_baslangic")._GetInt;
			int getInt231 = parametreler._GetParametre("satir_sck_TCMB_Sube_kodu_uzunluk")._GetInt;
			bool getBoolean87 = parametreler._GetParametre("satir_sck_vdaire_no_sabit_kullan")._GetBoolean;
			string getString107 = parametreler._GetParametre("satir_sck_vdaire_no_sabit_deger")._GetString;
			int getInt232 = parametreler._GetParametre("satir_sck_vdaire_no_baslangic")._GetInt;
			int getInt233 = parametreler._GetParametre("satir_sck_vdaire_no_uzunluk")._GetInt;
			string getString108 = parametreler._GetParametre("XslDosyaAdi")._GetString;
			if (getString108 != "")
			{
				string inputfile = DosyaAdi;
				DosyaAdi = Path.GetTempFileName();
				try
				{
					XslTransform xslTransform = new XslTransform();
					xslTransform.Load(getString108);
					xslTransform.Transform(inputfile, DosyaAdi);
				}
				catch
				{
				}
			}
			_uygulanacak_kriterler = new List<TahsilatEvrakKriter>();
			string[] array = getString.Split(',');
			foreach (string text in array)
			{
				if (text != "")
				{
					TahsilatEvrakKriter tahsilatEvrakKriter = new TahsilatEvrakKriter();
					Parametreler parametreler2 = ParametrelerDefault.TahsilatAktarimKriter(text);
					ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler2, "TahsilatAktarim", "", "Kriter", text);
					tahsilatEvrakKriter.arama_alan1_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_baglac")._GetInt;
					tahsilatEvrakKriter.arama_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan2_baglac")._GetInt;
					tahsilatEvrakKriter.arama_alan1_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_alan2_baglac")._GetInt;
					string getString109 = parametreler2._GetParametre("arama_alan1")._GetString;
					string getString110 = parametreler2._GetParametre("arama_alan2")._GetString;
					string getString111 = parametreler2._GetParametre("degistirilecek_alanlar")._GetString;
					if (getString109 != "")
					{
						tahsilatEvrakKriter.arama_alan1 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString109);
					}
					else
					{
						tahsilatEvrakKriter.arama_alan1 = new List<AlanOperatorDeger>();
					}
					if (getString110 != "")
					{
						tahsilatEvrakKriter.arama_alan2 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString110);
					}
					else
					{
						tahsilatEvrakKriter.arama_alan2 = new List<AlanOperatorDeger>();
					}
					if (getString111 != "")
					{
						tahsilatEvrakKriter.degistirilecek_alanlar = JSON.Instance.ToObject<List<AlanIslemTipiDeger>>(getString111);
					}
					else
					{
						tahsilatEvrakKriter.degistirilecek_alanlar = new List<AlanIslemTipiDeger>();
					}
					_uygulanacak_kriterler.Add(tahsilatEvrakKriter);
				}
			}
			List<Cari> list = new List<Cari>();
			new List<Stok>();
			new List<Hizmet>();
			List<Proje> list2 = new List<Proje>();
			List<SorumlulukMerkezi> list3 = new List<SorumlulukMerkezi>();
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
					if (getBoolean4)
					{
						flag2 = false;
						if (text2.StartsWith(getString5))
						{
							flag2 = true;
						}
					}
					if (getBoolean2 && num4 < getInt2)
					{
						flag2 = false;
						flag3 = true;
					}
					if (text2.Trim() == "")
					{
						flag2 = false;
					}
					bool flag4 = false;
					if (getBoolean4)
					{
						if (text2.StartsWith(getString4) && !flag3)
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
						string text3 = "";
						if (getInt42 != 0 || getInt43 != 0)
						{
							text3 = ((!getBoolean3) ? text2.Substring(getInt42 - 1, getInt43).Trim() : array2[getInt42 - 1]);
						}
						if (text3 == "" && (getInt44 != 0 || getInt45 != 0))
						{
							text3 = ((!getBoolean3) ? text2.Substring(getInt44 - 1, getInt45).Trim() : array2[getInt44 - 1]);
						}
						string text4 = "";
						if (getInt38 != 0 || getInt39 != 0)
						{
							text4 = ((!getBoolean3) ? text2.Substring(getInt38 - 1, getInt39).Trim() : array2[getInt38 - 1]);
						}
						if (text4.Length > 50)
						{
							text4 = text4.Substring(0, 50);
						}
						string text5 = "";
						if (getInt40 != 0 || getInt41 != 0)
						{
							text5 = ((!getBoolean3) ? text2.Substring(getInt40 - 1, getInt41).Trim() : array2[getInt40 - 1]);
						}
						if (text5.Length > 50)
						{
							text5 = text5.Substring(0, 50);
						}
						string text6 = "";
						if (getInt48 != 0 || getInt49 != 0)
						{
							text6 = ((!getBoolean3) ? text2.Substring(getInt48 - 1, getInt49).Trim() : array2[getInt48 - 1]);
						}
						if (text6.Length > 30)
						{
							text6 = text6.Substring(0, 30);
						}
						string text7 = "";
						if (getInt64 != 0 || getInt65 != 0)
						{
							text7 = ((!getBoolean3) ? text2.Substring(getInt64 - 1, getInt65).Trim() : array2[getInt64 - 1]);
						}
						if (text7.Length > 80)
						{
							text7 = text7.Substring(0, 80);
						}
						string text8 = "";
						if (getBoolean32)
						{
							text8 = getString50;
						}
						if (getBoolean12)
						{
							text8 += getString30;
						}
						else
						{
							num2 = getInt36;
							num3 = getInt37;
							string text9 = "";
							if (num2 != 0 || num3 != 0)
							{
								text9 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							text8 += text9;
							if (text9 == "")
							{
								int num5 = getInt107;
								int num6 = getInt108;
								string text10 = "";
								if (num5 != 0 || num6 != 0)
								{
									text10 = ((!getBoolean3) ? text2.Substring(num5 - 1, num6).Trim() : array2[num5 - 1]);
								}
								text8 += text10;
							}
						}
						string[] array3 = getString31.Split(',');
						bool flag5 = false;
						array = array3;
						foreach (string text11 in array)
						{
							if (flag5)
							{
								break;
							}
							switch (text11)
							{
							case "1":
								if (text8 != "" && CariData.GetCariByCariKod(Db.Connection, text8, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "2":
								if (text3 != "" && CariData.GetCariByVergiTcKimlikNo(Db.Connection, text3, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "3":
								if (text7 != "" && CariData.GetCariByEMail(Db.Connection, text7, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "4":
								if (text6 != "" && CariData.GetCariByBankaHesapNo(Db.Connection, text6, AdreslerTemsilciyeGore: false, "").cari_kod != "")
								{
									flag5 = true;
								}
								break;
							case "5":
								if (text4 != "" && CariData.GetCariByCariUnvan(Db.Connection, text4, AdreslerTemsilciyeGore: false, "").cari_kod != "")
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
								if (text8 == item.cari_kod)
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
							cari.cari_kod = text8;
							cari.cari_vdaire_no = text3;
							cari.cari_unvan1 = text4;
							cari.cari_unvan2 = text5;
							cari.cari_banka_hesapno1 = text6;
							cari.cari_Email = text7;
							string text12 = "";
							if (getInt46 != 0 || getInt47 != 0)
							{
								text12 = ((!getBoolean3) ? text2.Substring(getInt46 - 1, getInt47).Trim() : array2[getInt46 - 1]);
							}
							if (text12.Length > 20)
							{
								text12 = text12.Substring(0, 20);
							}
							cari.cari_vdaire_adi = text12;
							string text13 = "";
							if (getBoolean57)
							{
								text13 = getString65;
							}
							else
							{
								if (getInt166 != 0 || getInt167 != 0)
								{
									text13 = ((!getBoolean3) ? text2.Substring(getInt166 - 1, getInt167).Trim() : array2[getInt166 - 1]);
								}
								if (text13.Length > 10)
								{
									text13 = text13.Substring(0, 10);
								}
							}
							cari.cari_muhartikeli = text13;
							string text14 = "";
							if (getBoolean61)
							{
								text14 = getString82;
							}
							else
							{
								if (getInt174 != 0 || getInt175 != 0)
								{
									text14 = ((!getBoolean3) ? text2.Substring(getInt174 - 1, getInt175).Trim() : array2[getInt174 - 1]);
								}
								if (text14.Length > 25)
								{
									text14 = text14.Substring(0, 25);
								}
							}
							cari.cari_Ana_cari_kodu = text14;
							string text15 = "";
							if (getBoolean62)
							{
								text15 = getString83;
							}
							else
							{
								if (getInt176 != 0 || getInt177 != 0)
								{
									text15 = ((!getBoolean3) ? text2.Substring(getInt176 - 1, getInt177).Trim() : array2[getInt176 - 1]);
								}
								if (text15.Length > 25)
								{
									text15 = text15.Substring(0, 25);
								}
							}
							cari.cari_temsilci_kodu = text15;
							string text16 = "";
							if (getBoolean63)
							{
								text16 = getString84;
							}
							else
							{
								if (getInt178 != 0 || getInt179 != 0)
								{
									text16 = ((!getBoolean3) ? text2.Substring(getInt178 - 1, getInt179).Trim() : array2[getInt178 - 1]);
								}
								if (text16.Length > 25)
								{
									text16 = text16.Substring(0, 25);
								}
							}
							cari.cari_grup_kodu = text16;
							string text17 = "";
							if (getBoolean64)
							{
								text17 = getString85;
							}
							else
							{
								if (getInt180 != 0 || getInt181 != 0)
								{
									text17 = ((!getBoolean3) ? text2.Substring(getInt180 - 1, getInt181).Trim() : array2[getInt180 - 1]);
								}
								if (text17.Length > 25)
								{
									text17 = text17.Substring(0, 25);
								}
							}
							cari.cari_sektor_kodu = text17;
							string text18 = "";
							if (getBoolean65)
							{
								text18 = getString86;
							}
							else
							{
								if (getInt182 != 0 || getInt183 != 0)
								{
									text18 = ((!getBoolean3) ? text2.Substring(getInt182 - 1, getInt183).Trim() : array2[getInt182 - 1]);
								}
								if (text18.Length > 25)
								{
									text18 = text18.Substring(0, 25);
								}
							}
							cari.cari_bolge_kodu = text18;
							string text19 = "";
							if (getBoolean66)
							{
								text19 = getString87;
							}
							else
							{
								if (getInt184 != 0 || getInt185 != 0)
								{
									text19 = ((!getBoolean3) ? text2.Substring(getInt184 - 1, getInt185).Trim() : array2[getInt184 - 1]);
								}
								if (text19.Length > 30)
								{
									text19 = text19.Substring(0, 30);
								}
							}
							cari.cari_wwwadresi = text19;
							string text20 = "";
							if (getBoolean67)
							{
								text20 = getString88;
							}
							else
							{
								if (getInt186 != 0 || getInt187 != 0)
								{
									text20 = ((!getBoolean3) ? text2.Substring(getInt186 - 1, getInt187).Trim() : array2[getInt186 - 1]);
								}
								if (text20.Length > 20)
								{
									text20 = text20.Substring(0, 20);
								}
							}
							cari.cari_CepTel = text20;
							string text21 = "";
							if (getBoolean68)
							{
								text21 = getString89;
							}
							else
							{
								if (getInt188 != 0 || getInt189 != 0)
								{
									text21 = ((!getBoolean3) ? text2.Substring(getInt188 - 1, getInt189).Trim() : array2[getInt188 - 1]);
								}
								if (text21.Length > 4)
								{
									text21 = text21.Substring(0, 4);
								}
							}
							cari.cari_satis_isk_kod = text21;
							string text22 = "";
							if (getBoolean69)
							{
								text22 = getString90;
							}
							else
							{
								if (getInt190 != 0 || getInt191 != 0)
								{
									text22 = ((!getBoolean3) ? text2.Substring(getInt190 - 1, getInt191).Trim() : array2[getInt190 - 1]);
								}
								if (text22.Length > 15)
								{
									text22 = text22.Substring(0, 15);
								}
							}
							cari.cari_sicil_no = text22;
							string text23 = "";
							if (getBoolean70)
							{
								text23 = getString91;
							}
							else
							{
								if (getInt192 != 0 || getInt193 != 0)
								{
									text23 = ((!getBoolean3) ? text2.Substring(getInt192 - 1, getInt193).Trim() : array2[getInt192 - 1]);
								}
								if (text23.Length > 25)
								{
									text23 = text23.Substring(0, 25);
								}
							}
							int result = 0;
							int.TryParse(text23, out result);
							cari.cari_VarsayilanGirisDepo = result;
							string text24 = "";
							if (getBoolean71)
							{
								text24 = getString92;
							}
							else
							{
								if (getInt194 != 0 || getInt195 != 0)
								{
									text24 = ((!getBoolean3) ? text2.Substring(getInt194 - 1, getInt195).Trim() : array2[getInt194 - 1]);
								}
								if (text24.Length > 25)
								{
									text24 = text24.Substring(0, 25);
								}
							}
							int result2 = 0;
							int.TryParse(text24, out result2);
							cari.cari_VarsayilanCikisDepo = result2;
							string text25 = "";
							if (getBoolean72)
							{
								text25 = getString93;
							}
							else
							{
								if (getInt196 != 0 || getInt197 != 0)
								{
									text25 = ((!getBoolean3) ? text2.Substring(getInt196 - 1, getInt197).Trim() : array2[getInt196 - 1]);
								}
								if (text25.Length > 127)
								{
									text25 = text25.Substring(0, 127);
								}
							}
							cari.cari_Portal_PW = text25;
							string text26 = "";
							if (getBoolean58)
							{
								text26 = getString62;
							}
							else
							{
								if (getInt168 != 0 || getInt169 != 0)
								{
									text26 = ((!getBoolean3) ? text2.Substring(getInt168 - 1, getInt169).Trim() : array2[getInt168 - 1]);
								}
								if (text26.Length > 40)
								{
									text26 = text26.Substring(0, 40);
								}
							}
							cari.cari_muh_kod = text26;
							string text27 = "";
							if (getBoolean59)
							{
								text27 = getString63;
							}
							else
							{
								if (getInt170 != 0 || getInt171 != 0)
								{
									text27 = ((!getBoolean3) ? text2.Substring(getInt170 - 1, getInt171).Trim() : array2[getInt170 - 1]);
								}
								if (text27.Length > 40)
								{
									text27 = text27.Substring(0, 40);
								}
							}
							cari.cari_muh_kod1 = text27;
							string text28 = "";
							if (getBoolean60)
							{
								text28 = getString64;
							}
							else
							{
								if (getInt172 != 0 || getInt173 != 0)
								{
									text28 = ((!getBoolean3) ? text2.Substring(getInt172 - 1, getInt173).Trim() : array2[getInt172 - 1]);
								}
								if (text28.Length > 40)
								{
									text28 = text28.Substring(0, 40);
								}
							}
							cari.cari_muh_kod2 = text28;
							cari.cari_doviz_cinsi = getInt104;
							cari.cari_doviz_cinsi1 = getInt105;
							cari.cari_doviz_cinsi2 = getInt106;
							cari.cari_Portal_Enabled = getBoolean73;
							string text29 = "";
							string text30 = "";
							string text31 = "";
							if (getBoolean28)
							{
								text29 = getString46;
							}
							else
							{
								num2 = getInt94;
								num3 = getInt95;
								string text32 = "";
								if (num2 != 0 || num3 != 0)
								{
									text32 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text29 = text32;
							}
							if (getBoolean29)
							{
								text30 = getString47;
							}
							else
							{
								num2 = getInt96;
								num3 = getInt97;
								string text33 = "";
								if (num2 != 0 || num3 != 0)
								{
									text33 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text30 = text33;
							}
							if (getBoolean30)
							{
								text31 = getString48;
							}
							else
							{
								num2 = getInt98;
								num3 = getInt99;
								string text34 = "";
								if (num2 != 0 || num3 != 0)
								{
									text34 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
								}
								text31 = text34;
							}
							cari.cari_special1 = text29;
							cari.cari_special2 = text30;
							cari.cari_special3 = text31;
							string text35 = "";
							if (getInt50 != 0 || getInt51 != 0)
							{
								text35 = ((!getBoolean3) ? text2.Substring(getInt50 - 1, getInt51).Trim() : array2[getInt50 - 1]);
							}
							if (text35.Length > 50)
							{
								text35 = text35.Substring(0, 50);
							}
							if (text35 != "")
							{
								CariAdres cariAdres = new CariAdres();
								cariAdres.adr_cadde = text35;
								string text36 = "";
								if (getInt52 != 0 || getInt53 != 0)
								{
									text36 = ((!getBoolean3) ? text2.Substring(getInt52 - 1, getInt53).Trim() : array2[getInt52 - 1]);
								}
								if (text36.Length > 50)
								{
									text36 = text36.Substring(0, 50);
								}
								cariAdres.adr_sokak = text36;
								string text37 = "";
								if (getInt54 != 0 || getInt55 != 0)
								{
									text37 = ((!getBoolean3) ? text2.Substring(getInt54 - 1, getInt55).Trim() : array2[getInt54 - 1]);
								}
								if (text37.Length > 15)
								{
									text37 = text37.Substring(0, 15);
								}
								cariAdres.adr_ilce = text37;
								string text38 = "";
								if (getInt56 != 0 || getInt57 != 0)
								{
									text38 = ((!getBoolean3) ? text2.Substring(getInt56 - 1, getInt57).Trim() : array2[getInt56 - 1]);
								}
								if (text38.Length > 15)
								{
									text38 = text38.Substring(0, 15);
								}
								if (getBoolean44)
								{
									switch (int.Parse(text38))
									{
									case 1:
										text38 = "ADANA";
										break;
									case 2:
										text38 = "ADIYAMAN";
										break;
									case 3:
										text38 = "AFYONKARAHİSAR";
										break;
									case 4:
										text38 = "AĞRI";
										break;
									case 5:
										text38 = "AMASYA";
										break;
									case 6:
										text38 = "ANKARA";
										break;
									case 7:
										text38 = "ANTALYA";
										break;
									case 8:
										text38 = "ARTVİN";
										break;
									case 9:
										text38 = "AYDIN";
										break;
									case 10:
										text38 = "BALIKESİR";
										break;
									case 11:
										text38 = "BİLECİK";
										break;
									case 12:
										text38 = "BİNGÖL";
										break;
									case 13:
										text38 = "BİTLİS";
										break;
									case 14:
										text38 = "BOLU";
										break;
									case 15:
										text38 = "BURDUR";
										break;
									case 16:
										text38 = "BURSA";
										break;
									case 17:
										text38 = "ÇANAKKALE";
										break;
									case 18:
										text38 = "ÇANKIRI";
										break;
									case 19:
										text38 = "ÇORUM";
										break;
									case 20:
										text38 = "DENİZLİ";
										break;
									case 21:
										text38 = "DİYARBAKIR";
										break;
									case 22:
										text38 = "EDİRNE";
										break;
									case 23:
										text38 = "ELAZIĞ";
										break;
									case 24:
										text38 = "ERZİNCAN";
										break;
									case 25:
										text38 = "ERZURUM";
										break;
									case 26:
										text38 = "ESKİŞEHİR";
										break;
									case 27:
										text38 = "GAZİANTEP";
										break;
									case 28:
										text38 = "GİRESUN";
										break;
									case 29:
										text38 = "GÜMÜŞHANE";
										break;
									case 30:
										text38 = "HAKKARİ";
										break;
									case 31:
										text38 = "HATAY";
										break;
									case 32:
										text38 = "ISPARTA";
										break;
									case 33:
										text38 = "MERSİN";
										break;
									case 34:
										text38 = "İSTANBUL";
										break;
									case 35:
										text38 = "İZMİR";
										break;
									case 36:
										text38 = "KARS";
										break;
									case 37:
										text38 = "KASTAMONU";
										break;
									case 38:
										text38 = "KAYSERİ";
										break;
									case 39:
										text38 = "KIRKLARELİ";
										break;
									case 40:
										text38 = "KIRŞEHİR";
										break;
									case 41:
										text38 = "KOCAELİ";
										break;
									case 42:
										text38 = "KONYA";
										break;
									case 43:
										text38 = "KÜTAHYA";
										break;
									case 44:
										text38 = "MALATYA";
										break;
									case 45:
										text38 = "MANİSA";
										break;
									case 46:
										text38 = "KAHRAMANMARAŞ";
										break;
									case 47:
										text38 = "MARDİN";
										break;
									case 48:
										text38 = "MUĞLA";
										break;
									case 49:
										text38 = "MUŞ";
										break;
									case 50:
										text38 = "NEVŞEHİR";
										break;
									case 51:
										text38 = "NİĞDE";
										break;
									case 52:
										text38 = "ORDU";
										break;
									case 53:
										text38 = "RİZE";
										break;
									case 54:
										text38 = "SAKARYA";
										break;
									case 55:
										text38 = "SAMSUN";
										break;
									case 56:
										text38 = "SİİRT";
										break;
									case 57:
										text38 = "SİNOP";
										break;
									case 58:
										text38 = "SİVAS";
										break;
									case 59:
										text38 = "TEKİRDAĞ";
										break;
									case 60:
										text38 = "TOKAT";
										break;
									case 61:
										text38 = "TRABZON";
										break;
									case 62:
										text38 = "TUNCELİ";
										break;
									case 63:
										text38 = "ŞANLIURFA";
										break;
									case 64:
										text38 = "UŞAK";
										break;
									case 65:
										text38 = "VAN";
										break;
									case 66:
										text38 = "YOZGAT";
										break;
									case 67:
										text38 = "ZONGULDAK";
										break;
									case 68:
										text38 = "AKSARAY";
										break;
									case 69:
										text38 = "BAYBURT";
										break;
									case 70:
										text38 = "KARAMAN";
										break;
									case 71:
										text38 = "KIRIKKALE";
										break;
									case 72:
										text38 = "BATMAN";
										break;
									case 73:
										text38 = "ŞIRNAK";
										break;
									case 74:
										text38 = "BARTIN";
										break;
									case 75:
										text38 = "ARDAHAN";
										break;
									case 76:
										text38 = "IĞDIR";
										break;
									case 77:
										text38 = "YALOVA";
										break;
									case 78:
										text38 = "KARABÜK";
										break;
									case 79:
										text38 = "KİLİS";
										break;
									case 80:
										text38 = "OSMANİYE";
										break;
									case 81:
										text38 = "DÜZCE";
										break;
									}
								}
								cariAdres.adr_il = text38;
								string text39 = "";
								if (getInt58 != 0 || getInt59 != 0)
								{
									text39 = ((!getBoolean3) ? text2.Substring(getInt58 - 1, getInt59).Trim() : array2[getInt58 - 1]);
								}
								if (text39.Length > 15)
								{
									text39 = text39.Substring(0, 15);
								}
								cariAdres.adr_ulke = text39;
								string text40 = "";
								if (getInt60 != 0 || getInt61 != 0)
								{
									text40 = ((!getBoolean3) ? text2.Substring(getInt60 - 1, getInt61).Trim() : array2[getInt60 - 1]);
								}
								if (text40.Length > 8)
								{
									text40 = text40.Substring(0, 8);
								}
								cariAdres.adr_posta_kodu = text40;
								string text41 = "";
								if (getInt62 != 0 || getInt63 != 0)
								{
									text41 = ((!getBoolean3) ? text2.Substring(getInt62 - 1, getInt63).Trim() : array2[getInt62 - 1]);
								}
								if (text41.Length > 10)
								{
									text41 = text41.Substring(0, 10);
								}
								cariAdres.adr_tel_no1 = text41;
								cariAdres.adr_special1 = text29;
								cariAdres.adr_special2 = text30;
								cariAdres.adr_special3 = text31;
								cariAdres.adr_cari_kod = cari.cari_kod;
								cariAdres.adr_adres_no = 1;
								cari.CariAdresleri.Add(cariAdres);
							}
							list.Add(cari);
						}
					}
					if (flag2)
					{
						string text42 = "";
						if (getBoolean13)
						{
							text42 = getString32;
						}
						else
						{
							num2 = getInt66;
							num3 = getInt67;
							string text43 = "";
							if (num2 != 0 || num3 != 0)
							{
								text43 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							text42 = text43;
						}
						bool flag6 = false;
						Proje proje = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text42);
						if (!(proje.pro_kodu == "") && proje.pro_kodu != null)
						{
							flag6 = true;
						}
						if (text42 == "")
						{
							flag6 = true;
						}
						if (!flag6)
						{
							bool flag7 = false;
							foreach (Proje item2 in list2)
							{
								if (text42 == item2.pro_kodu)
								{
									flag7 = true;
									break;
								}
							}
							if (!flag7)
							{
								Proje proje2 = new Proje();
								proje2.pro_kodu = text42;
								string text44 = "";
								if (getBoolean46)
								{
									text44 = getString71;
								}
								else
								{
									if (getInt144 != 0 || getInt145 != 0)
									{
										text44 = ((!getBoolean3) ? text2.Substring(getInt144 - 1, getInt145).Trim() : array2[getInt144 - 1]);
									}
									if (text44.Length > 40)
									{
										text44 = text44.Substring(0, 40);
									}
								}
								proje2.pro_adi = text44;
								string text45 = "";
								if (getBoolean47)
								{
									text45 = getString72;
								}
								else
								{
									if (getInt146 != 0 || getInt147 != 0)
									{
										text45 = ((!getBoolean3) ? text2.Substring(getInt146 - 1, getInt147).Trim() : array2[getInt146 - 1]);
									}
									if (text45.Length > 25)
									{
										text45 = text45.Substring(0, 25);
									}
								}
								proje2.pro_musterikodu = text45;
								string text46 = "";
								if (getBoolean48)
								{
									text46 = getString73;
								}
								else
								{
									if (getInt148 != 0 || getInt149 != 0)
									{
										text46 = ((!getBoolean3) ? text2.Substring(getInt148 - 1, getInt149).Trim() : array2[getInt148 - 1]);
									}
									if (text46.Length > 25)
									{
										text46 = text46.Substring(0, 25);
									}
								}
								proje2.pro_sormerkodu = text46;
								string text47 = "";
								if (getBoolean49)
								{
									text47 = getString74;
								}
								else
								{
									if (getInt150 != 0 || getInt151 != 0)
									{
										text47 = ((!getBoolean3) ? text2.Substring(getInt150 - 1, getInt151).Trim() : array2[getInt150 - 1]);
									}
									if (text47.Length > 25)
									{
										text47 = text47.Substring(0, 25);
									}
								}
								proje2.pro_grupkodu = text47;
								string text48 = "";
								if (getBoolean50)
								{
									text48 = getString75;
								}
								else
								{
									if (getInt152 != 0 || getInt153 != 0)
									{
										text48 = ((!getBoolean3) ? text2.Substring(getInt152 - 1, getInt153).Trim() : array2[getInt152 - 1]);
									}
									if (text48.Length > 25)
									{
										text48 = text48.Substring(0, 25);
									}
								}
								proje2.pro_sektorkodu = text48;
								string text49 = "";
								if (getBoolean51)
								{
									text49 = getString76;
								}
								else
								{
									if (getInt154 != 0 || getInt155 != 0)
									{
										text49 = ((!getBoolean3) ? text2.Substring(getInt154 - 1, getInt155).Trim() : array2[getInt154 - 1]);
									}
									if (text49.Length > 25)
									{
										text49 = text49.Substring(0, 25);
									}
								}
								proje2.pro_bolgekodu = text49;
								string text50 = "";
								if (getBoolean52)
								{
									text50 = getString77;
								}
								else
								{
									if (getInt156 != 0 || getInt157 != 0)
									{
										text50 = ((!getBoolean3) ? text2.Substring(getInt156 - 1, getInt157).Trim() : array2[getInt156 - 1]);
									}
									if (text50.Length > 25)
									{
										text50 = text50.Substring(0, 25);
									}
								}
								proje2.pro_ana_projekodu = text50;
								string text51 = "";
								if (getBoolean53)
								{
									text51 = getString78;
								}
								else
								{
									if (getInt158 != 0 || getInt159 != 0)
									{
										text51 = ((!getBoolean3) ? text2.Substring(getInt158 - 1, getInt159).Trim() : array2[getInt158 - 1]);
									}
									if (text51.Length > 50)
									{
										text51 = text51.Substring(0, 50);
									}
								}
								proje2.pro_aciklama = text51;
								string text52 = "";
								if (getBoolean54)
								{
									text52 = getString79;
								}
								else
								{
									if (getInt160 != 0 || getInt161 != 0)
									{
										text52 = ((!getBoolean3) ? text2.Substring(getInt160 - 1, getInt161).Trim() : array2[getInt160 - 1]);
									}
									if (text52.Length > 10)
									{
										text52 = text52.Substring(0, 10);
									}
								}
								proje2.pro_muh_kod_artikeli = text52;
								list2.Add(proje2);
							}
						}
						string text53 = "";
						if (getBoolean14)
						{
							text53 = getString33;
						}
						else
						{
							num2 = getInt68;
							num3 = getInt69;
							string text54 = "";
							if (num2 != 0 || num3 != 0)
							{
								text54 = ((!getBoolean3) ? text2.Substring(num2 - 1, num3).Trim() : array2[num2 - 1]);
							}
							text53 = text54;
						}
						bool flag8 = false;
						SorumlulukMerkezi sorumlulukMerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text53);
						if (!(sorumlulukMerkezi.som_kod == "") && sorumlulukMerkezi.som_kod != null)
						{
							flag8 = true;
						}
						if (text53 == "")
						{
							flag8 = true;
						}
						if (!flag8)
						{
							bool flag9 = false;
							foreach (SorumlulukMerkezi item3 in list3)
							{
								if (text53 == item3.som_kod)
								{
									flag9 = true;
									break;
								}
							}
							if (!flag9)
							{
								SorumlulukMerkezi sorumlulukMerkezi2 = new SorumlulukMerkezi();
								sorumlulukMerkezi2.som_kod = text53;
								string text55 = "";
								if (getBoolean55)
								{
									text55 = getString80;
								}
								else
								{
									if (getInt162 != 0 || getInt163 != 0)
									{
										text55 = ((!getBoolean3) ? text2.Substring(getInt162 - 1, getInt163).Trim() : array2[getInt162 - 1]);
									}
									if (text55.Length > 40)
									{
										text55 = text55.Substring(0, 40);
									}
								}
								sorumlulukMerkezi2.som_isim = text55;
								string text56 = "";
								if (getBoolean56)
								{
									text56 = getString81;
								}
								else
								{
									if (getInt164 != 0 || getInt165 != 0)
									{
										text56 = ((!getBoolean3) ? text2.Substring(getInt164 - 1, getInt165).Trim() : array2[getInt164 - 1]);
									}
									if (text56.Length > 10)
									{
										text56 = text56.Substring(0, 10);
									}
								}
								sorumlulukMerkezi2.som_MuhArtikeli = text56;
								list3.Add(sorumlulukMerkezi2);
							}
						}
					}
					num4++;
				}
			}
			if (list.Count > 0)
			{
				switch (getInt141)
				{
				case 1:
					foreach (Cari item4 in list)
					{
						item4.KaydetYeniCari(Db.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniCarileriSorVeAc(list);
					break;
				}
			}
			if (list2.Count > 0)
			{
				switch (getInt142)
				{
				case 1:
					foreach (Proje item5 in list2)
					{
						ProjeData.YeniProjeKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item5, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniProjeleriSorVeAc(list2);
					break;
				}
			}
			if (list3.Count > 0)
			{
				switch (getInt143)
				{
				case 1:
					foreach (SorumlulukMerkezi item6 in list3)
					{
						SorumlulukMerkeziData.YeniSorumlulukMerkeziKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item6, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniSorumlulukMerkezleriSorVeAc(list3);
					break;
				}
			}
			using StreamReader streamReader2 = new StreamReader(DosyaAdi, Encoding.GetEncoding("windows-1254"));
			DateTime dateTime = DateTime.Now;
			string evraknoseri = "";
			int evraknosira = 0;
			string belgeno = "";
			DateTime belgetarih = DateTime.Now;
			Cari cari2 = new Cari();
			new FiyatListesi();
			int dovizcinsi = 0;
			Kur kur = new Kur();
			int alternatifdovizcinsi = 0;
			Kur alternatifdovizkuru = new Kur();
			new Depo();
			Proje proje3 = new Proje();
			SorumlulukMerkezi sorumlulukmerkezi = new SorumlulukMerkezi();
			string temsilcikodu = "";
			Firma firma = new Firma();
			Sube sube = new Sube();
			int mikrouserno = 1;
			_ = DateTime.Now;
			string text57 = "";
			string text58 = "";
			string text59 = "";
			string text60 = "";
			string text61 = "";
			string text62 = "";
			string text63 = "";
			string text64 = "";
			string text65 = "";
			string text66 = "";
			string degistirspecialalan = "";
			string degistirspecialalan2 = "";
			string degistirspecialalan3 = "";
			string kriter_string = "";
			string kriter_string2 = "";
			string kriter_string3 = "";
			string kriter_string4 = "";
			string kriter_string5 = "";
			double result3 = 0.0;
			double result4 = 0.0;
			double result5 = 0.0;
			double result6 = 0.0;
			double result7 = 0.0;
			bool kriter_bool = false;
			bool kriter_bool2 = false;
			bool kriter_bool3 = false;
			bool kriter_bool4 = false;
			bool kriter_bool5 = false;
			num4 = 0;
			string text67;
			while ((text67 = streamReader2.ReadLine()) != null)
			{
				bool flag10 = true;
				bool flag11 = false;
				if (getBoolean && !text67.StartsWith(getString2))
				{
					flag10 = false;
				}
				if (getBoolean4)
				{
					flag10 = false;
					if (text67.StartsWith(getString5))
					{
						flag10 = true;
					}
				}
				if (getBoolean2 && num4 < getInt2)
				{
					flag10 = false;
					flag11 = true;
				}
				if (text67.Trim() == "")
				{
					flag10 = false;
				}
				bool flag12 = false;
				if (getBoolean4)
				{
					if (text67.StartsWith(getString4) && !flag11)
					{
						flag12 = true;
					}
				}
				else if (flag10)
				{
					flag12 = true;
				}
				string[] array4 = null;
				if (getBoolean3)
				{
					array4 = text67.Split(getString3.ToCharArray()[0]);
				}
				if (flag12)
				{
					firma = FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt3);
					sube = SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt4);
					string text68 = "";
					if (getInt42 != 0 || getInt43 != 0)
					{
						text68 = ((!getBoolean3) ? text67.Substring(getInt42 - 1, getInt43).Trim() : array4[getInt42 - 1]);
					}
					if (text68 == "" && (getInt44 != 0 || getInt45 != 0))
					{
						text68 = ((!getBoolean3) ? text67.Substring(getInt44 - 1, getInt45).Trim() : array4[getInt44 - 1]);
					}
					string text69 = "";
					if (getInt38 != 0 || getInt39 != 0)
					{
						text69 = ((!getBoolean3) ? text67.Substring(getInt38 - 1, getInt39).Trim() : array4[getInt38 - 1]);
					}
					if (text69.Length > 50)
					{
						text69 = text69.Substring(0, 50);
					}
					string text70 = "";
					if (getInt40 != 0 || getInt41 != 0)
					{
						text70 = ((!getBoolean3) ? text67.Substring(getInt40 - 1, getInt41).Trim() : array4[getInt40 - 1]);
					}
					if (text70.Length > 50)
					{
						text70 = text70.Substring(0, 50);
					}
					string text71 = "";
					if (getInt48 != 0 || getInt49 != 0)
					{
						text71 = ((!getBoolean3) ? text67.Substring(getInt48 - 1, getInt49).Trim() : array4[getInt48 - 1]);
					}
					if (text71.Length > 30)
					{
						text71 = text71.Substring(0, 30);
					}
					string text72 = "";
					if (getInt64 != 0 || getInt65 != 0)
					{
						text72 = ((!getBoolean3) ? text67.Substring(getInt64 - 1, getInt65).Trim() : array4[getInt64 - 1]);
					}
					if (text72.Length > 80)
					{
						text72 = text72.Substring(0, 80);
					}
					string text73 = "";
					if (getBoolean32)
					{
						text73 = getString50;
					}
					if (getBoolean12)
					{
						text73 += getString30;
					}
					else
					{
						num2 = getInt36;
						num3 = getInt37;
						string text74 = "";
						if (num2 != 0 || num3 != 0)
						{
							text74 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text73 += text74;
						if (text74 == "")
						{
							int num7 = getInt107;
							int num8 = getInt108;
							string text75 = "";
							if (num7 != 0 || num8 != 0)
							{
								text75 = ((!getBoolean3) ? text67.Substring(num7 - 1, num8).Trim() : array4[num7 - 1]);
							}
							text73 += text75;
						}
					}
					string[] array5 = getString31.Split(',');
					bool flag13 = false;
					array = array5;
					foreach (string text76 in array)
					{
						if (flag13)
						{
							break;
						}
						switch (text76)
						{
						case "1":
							if (text73 != "")
							{
								cari2 = CariData.GetCariByCariKod(Db.Connection, text73, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag13 = true;
								}
							}
							break;
						case "2":
							if (text68 != "")
							{
								cari2 = CariData.GetCariByVergiTcKimlikNo(Db.Connection, text68, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag13 = true;
								}
							}
							break;
						case "3":
							if (text72 != "")
							{
								cari2 = CariData.GetCariByEMail(Db.Connection, text72, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag13 = true;
								}
							}
							break;
						case "4":
							if (text71 != "")
							{
								cari2 = CariData.GetCariByBankaHesapNo(Db.Connection, text71, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag13 = true;
								}
							}
							break;
						case "5":
							if (text69 != "")
							{
								cari2 = CariData.GetCariByCariUnvan(Db.Connection, text69, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag13 = true;
								}
							}
							break;
						}
					}
					FiyatListesiData.GetFiyatListesi(Db.Connection, cari2.cari_satis_fk);
					dovizcinsi = cari2.cari_doviz_cinsi;
					_ = cari2.cari_odemeplan_no;
					if (getInt5 != 0 || getInt6 != 0)
					{
						if (getBoolean3)
						{
							dateTime = DateTime.Parse(array4[getInt5 - 1]);
						}
						else
						{
							int num9 = int.Parse(text67.Substring(getInt5 - 1, getInt6).Trim());
							if (getInt6 == 2)
							{
								num9 += 2000;
							}
							int month = int.Parse(text67.Substring(getInt7 - 1, getInt8).Trim());
							int day = int.Parse(text67.Substring(getInt9 - 1, getInt10).Trim());
							dateTime = new DateTime(num9, month, day);
						}
					}
					if (getBoolean31)
					{
						evraknoseri = getString49;
					}
					else if (getInt100 != 0 || getInt101 != 0)
					{
						num2 = getInt100;
						num3 = getInt101;
						string text77 = "";
						text77 = ((!getBoolean3) ? text67.Substring(getInt100 - 1, getInt101).Trim() : array4[getInt100 - 1]);
						evraknoseri = text77;
					}
					if (getInt102 != 0 || getInt103 != 0)
					{
						evraknosira = ((!getBoolean3) ? int.Parse(text67.Substring(getInt102 - 1, getInt103).Trim()) : int.Parse(array4[getInt102 - 1]));
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
							int num10 = int.Parse(text67.Substring(getInt13 - 1, getInt14).Trim());
							if (getInt14 == 2)
							{
								num10 += 2000;
							}
							int month2 = int.Parse(text67.Substring(getInt15 - 1, getInt16).Trim());
							int day2 = int.Parse(text67.Substring(getInt17 - 1, getInt18).Trim());
							belgetarih = new DateTime(num10, month2, day2);
						}
					}
					belgeno = "";
					if (getInt11 != 0 || getInt12 != 0)
					{
						belgeno = ((!getBoolean3) ? text67.Substring(getInt11 - 1, getInt12).Trim() : array4[getInt11 - 1]);
					}
					if (getInt19 != 0 || getInt20 != 0)
					{
						if (getBoolean3)
						{
							kur = new Kur();
							double result8 = 1.0;
							double.TryParse(array4[getInt19 - 1].Replace(".", ","), out result8);
							kur.dov_fiyat = result8;
						}
						else
						{
							kur = new Kur();
							double result9 = 1.0;
							double.TryParse(text67.Substring(getInt19 - 1, getInt20).Trim().Replace(".", ","), out result9);
							kur.dov_fiyat = result9;
						}
					}
					string text78 = "";
					if (getBoolean13)
					{
						text78 = getString32;
					}
					else
					{
						num2 = getInt66;
						num3 = getInt67;
						string text79 = "";
						if (num2 != 0 || num3 != 0)
						{
							text79 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text78 = text79;
					}
					if (text78 != "")
					{
						proje3 = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text78);
					}
					string text80 = "";
					if (getBoolean14)
					{
						text80 = getString33;
					}
					else
					{
						num2 = getInt68;
						num3 = getInt69;
						string text81 = "";
						if (num2 != 0 || num3 != 0)
						{
							text81 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text80 = text81;
					}
					if (text80 != "")
					{
						sorumlulukmerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text80);
					}
					if (getBoolean15)
					{
						temsilcikodu = getString34;
					}
					else
					{
						num2 = getInt70;
						num3 = getInt71;
						string text82 = "";
						if (num2 != 0 || num3 != 0)
						{
							text82 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (getBoolean16)
						{
							text82 = cari2.cari_temsilci_kodu;
						}
						temsilcikodu = text82;
					}
					if (getBoolean18)
					{
						text57 = getString36;
					}
					else
					{
						num2 = getInt74;
						num3 = getInt75;
						string text83 = "";
						if (num2 != 0 || num3 != 0)
						{
							text83 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text57 = text83;
					}
					if (getBoolean34)
					{
						text57 = getString52 + text57;
					}
					if (getBoolean19)
					{
						text58 = getString37;
					}
					else
					{
						num2 = getInt76;
						num3 = getInt77;
						string text84 = "";
						if (num2 != 0 || num3 != 0)
						{
							text84 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text58 = text84;
					}
					if (getBoolean35)
					{
						text58 = getString53 + text58;
					}
					if (getBoolean20)
					{
						text59 = getString38;
					}
					else
					{
						num2 = getInt78;
						num3 = getInt79;
						string text85 = "";
						if (num2 != 0 || num3 != 0)
						{
							text85 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text59 = text85;
					}
					if (getBoolean36)
					{
						text59 = getString54 + text59;
					}
					if (getBoolean21)
					{
						text60 = getString39;
					}
					else
					{
						num2 = getInt80;
						num3 = getInt81;
						string text86 = "";
						if (num2 != 0 || num3 != 0)
						{
							text86 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text60 = text86;
					}
					if (getBoolean37)
					{
						text60 = getString55 + text60;
					}
					if (getBoolean22)
					{
						text61 = getString40;
					}
					else
					{
						num2 = getInt82;
						num3 = getInt83;
						string text87 = "";
						if (num2 != 0 || num3 != 0)
						{
							text87 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text61 = text87;
					}
					if (getBoolean38)
					{
						text61 = getString56 + text61;
					}
					if (getBoolean23)
					{
						text62 = getString41;
					}
					else
					{
						num2 = getInt84;
						num3 = getInt85;
						string text88 = "";
						if (num2 != 0 || num3 != 0)
						{
							text88 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text62 = text88;
					}
					if (getBoolean39)
					{
						text62 = getString57 + text62;
					}
					if (getBoolean24)
					{
						text63 = getString42;
					}
					else
					{
						num2 = getInt86;
						num3 = getInt87;
						string text89 = "";
						if (num2 != 0 || num3 != 0)
						{
							text89 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text63 = text89;
					}
					if (getBoolean40)
					{
						text63 = getString58 + text63;
					}
					if (getBoolean25)
					{
						text64 = getString43;
					}
					else
					{
						num2 = getInt88;
						num3 = getInt89;
						string text90 = "";
						if (num2 != 0 || num3 != 0)
						{
							text90 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text64 = text90;
					}
					if (getBoolean41)
					{
						text64 = getString59 + text64;
					}
					if (getBoolean26)
					{
						text65 = getString44;
					}
					else
					{
						num2 = getInt90;
						num3 = getInt91;
						string text91 = "";
						if (num2 != 0 || num3 != 0)
						{
							text91 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text65 = text91;
					}
					if (getBoolean42)
					{
						text65 = getString60 + text65;
					}
					if (getBoolean27)
					{
						text66 = getString45;
					}
					else
					{
						num2 = getInt92;
						num3 = getInt93;
						string text92 = "";
						if (num2 != 0 || num3 != 0)
						{
							text92 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text66 = text92;
					}
					if (getBoolean43)
					{
						text66 = getString61 + text66;
					}
					if (getBoolean28)
					{
						degistirspecialalan = getString46;
					}
					else
					{
						num2 = getInt94;
						num3 = getInt95;
						string text93 = "";
						if (num2 != 0 || num3 != 0)
						{
							text93 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						degistirspecialalan = text93;
					}
					if (getBoolean29)
					{
						degistirspecialalan2 = getString47;
					}
					else
					{
						num2 = getInt96;
						num3 = getInt97;
						string text94 = "";
						if (num2 != 0 || num3 != 0)
						{
							text94 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						degistirspecialalan2 = text94;
					}
					if (getBoolean30)
					{
						degistirspecialalan3 = getString48;
					}
					else
					{
						num2 = getInt98;
						num3 = getInt99;
						string text95 = "";
						if (num2 != 0 || num3 != 0)
						{
							text95 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						degistirspecialalan3 = text95;
					}
				}
				num2 = getInt109;
				num3 = getInt110;
				if (num2 != 0 || num3 != 0)
				{
					string text96 = "";
					text96 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string = text96;
				}
				num2 = getInt111;
				num3 = getInt112;
				if (num2 != 0 || num3 != 0)
				{
					string text97 = "";
					text97 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string2 = text97;
				}
				num2 = getInt113;
				num3 = getInt114;
				if (num2 != 0 || num3 != 0)
				{
					string text98 = "";
					text98 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string3 = text98;
				}
				num2 = getInt115;
				num3 = getInt116;
				if (num2 != 0 || num3 != 0)
				{
					string text99 = "";
					text99 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string4 = text99;
				}
				num2 = getInt117;
				num3 = getInt118;
				if (num2 != 0 || num3 != 0)
				{
					string text100 = "";
					text100 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					kriter_string5 = text100;
				}
				num2 = getInt119;
				num3 = getInt120;
				if (num2 != 0 || num3 != 0)
				{
					string text101 = "";
					text101 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text101.Replace(".", ","), out result3);
				}
				num2 = getInt121;
				num3 = getInt122;
				if (num2 != 0 || num3 != 0)
				{
					string text102 = "";
					text102 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text102.Replace(".", ","), out result4);
				}
				num2 = getInt123;
				num3 = getInt124;
				if (num2 != 0 || num3 != 0)
				{
					string text103 = "";
					text103 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text103.Replace(".", ","), out result5);
				}
				num2 = getInt125;
				num3 = getInt126;
				if (num2 != 0 || num3 != 0)
				{
					string text104 = "";
					text104 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text104.Replace(".", ","), out result6);
				}
				num2 = getInt127;
				num3 = getInt128;
				if (num2 != 0 || num3 != 0)
				{
					string text105 = "";
					text105 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					double.TryParse(text105.Replace(".", ","), out result7);
				}
				num2 = getInt129;
				num3 = getInt130;
				if (num2 != 0 || num3 != 0)
				{
					string text106 = "";
					text106 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text106 == getString66)
					{
						kriter_bool = true;
					}
				}
				num2 = getInt131;
				num3 = getInt132;
				if (num2 != 0 || num3 != 0)
				{
					string text107 = "";
					text107 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text107 == getString67)
					{
						kriter_bool2 = true;
					}
				}
				num2 = getInt133;
				num3 = getInt134;
				if (num2 != 0 || num3 != 0)
				{
					string text108 = "";
					text108 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text108 == getString68)
					{
						kriter_bool3 = true;
					}
				}
				num2 = getInt135;
				num3 = getInt136;
				if (num2 != 0 || num3 != 0)
				{
					string text109 = "";
					text109 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text109 == getString69)
					{
						kriter_bool4 = true;
					}
				}
				num2 = getInt137;
				num3 = getInt138;
				if (num2 != 0 || num3 != 0)
				{
					string text110 = "";
					text110 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1].Trim());
					if (text110 == getString70)
					{
						kriter_bool5 = true;
					}
				}
				if (flag10)
				{
					TahsilatEvrakSatir tahsilatEvrakSatir = new TahsilatEvrakSatir();
					if (getBoolean45)
					{
						tahsilatEvrakSatir.KayitID = num4;
					}
					else
					{
						num2 = getInt139;
						num3 = getInt140;
						string s = "";
						if (num2 != 0 || num3 != 0)
						{
							s = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						int result10 = 0;
						int.TryParse(s, out result10);
						tahsilatEvrakSatir.KayitID = result10;
					}
					tahsilatEvrakSatir.evraktarih = dateTime;
					tahsilatEvrakSatir.evraknoseri = evraknoseri;
					tahsilatEvrakSatir.evraknosira = evraknosira;
					tahsilatEvrakSatir.belgeno = belgeno;
					tahsilatEvrakSatir.belgetarih = belgetarih;
					tahsilatEvrakSatir.cari = cari2;
					tahsilatEvrakSatir.dovizcinsi = dovizcinsi;
					tahsilatEvrakSatir.kur = kur;
					tahsilatEvrakSatir.alternatifdovizcinsi = alternatifdovizcinsi;
					tahsilatEvrakSatir.alternatifdovizkuru = alternatifdovizkuru;
					tahsilatEvrakSatir.proje = proje3;
					tahsilatEvrakSatir.sorumlulukmerkezi = sorumlulukmerkezi;
					tahsilatEvrakSatir.temsilcikodu = temsilcikodu;
					tahsilatEvrakSatir.firma = firma;
					tahsilatEvrakSatir.sube = sube;
					tahsilatEvrakSatir.DBCno = getInt;
					tahsilatEvrakSatir.mikrouserno = mikrouserno;
					tahsilatEvrakSatir.aciklama1 = text57;
					tahsilatEvrakSatir.aciklama2 = text58;
					tahsilatEvrakSatir.aciklama3 = text59;
					tahsilatEvrakSatir.aciklama4 = text60;
					tahsilatEvrakSatir.aciklama5 = text61;
					tahsilatEvrakSatir.aciklama6 = text62;
					tahsilatEvrakSatir.aciklama7 = text63;
					tahsilatEvrakSatir.aciklama8 = text64;
					tahsilatEvrakSatir.aciklama9 = text65;
					tahsilatEvrakSatir.aciklama10 = text66;
					tahsilatEvrakSatir.degistirspecialalan1 = degistirspecialalan;
					tahsilatEvrakSatir.degistirspecialalan2 = degistirspecialalan2;
					tahsilatEvrakSatir.degistirspecialalan3 = degistirspecialalan3;
					tahsilatEvrakSatir.kriter_string1 = kriter_string;
					tahsilatEvrakSatir.kriter_string2 = kriter_string2;
					tahsilatEvrakSatir.kriter_string3 = kriter_string3;
					tahsilatEvrakSatir.kriter_string4 = kriter_string4;
					tahsilatEvrakSatir.kriter_string5 = kriter_string5;
					tahsilatEvrakSatir.kriter_double1 = result3;
					tahsilatEvrakSatir.kriter_double2 = result4;
					tahsilatEvrakSatir.kriter_double3 = result5;
					tahsilatEvrakSatir.kriter_double4 = result6;
					tahsilatEvrakSatir.kriter_double5 = result7;
					tahsilatEvrakSatir.kriter_bool1 = kriter_bool;
					tahsilatEvrakSatir.kriter_bool2 = kriter_bool2;
					tahsilatEvrakSatir.kriter_bool3 = kriter_bool3;
					tahsilatEvrakSatir.kriter_bool4 = kriter_bool4;
					tahsilatEvrakSatir.kriter_bool5 = kriter_bool5;
					if (getBoolean5)
					{
						tahsilatEvrakSatir.satir_cinsi = (enum_tahsilat_cinsi)getInt21;
					}
					else
					{
						num2 = getInt22;
						num3 = getInt23;
						string text111 = "";
						if (num2 != 0 || num3 != 0)
						{
							text111 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						if (text111 == getString6)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.Nakit;
						}
						if (text111 == getString7)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.MusteriCeki;
						}
						if (text111 == getString8)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.MusteriSenedi;
						}
						if (text111 == getString9)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.MusteriKrediKarti;
						}
						if (text111 == getString10)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.GelenHavale;
						}
						if (text111 == getString11)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.GidenHavale;
						}
					}
					bool flag14 = false;
					string satir_kasa_banka_kodu = "";
					num2 = 0;
					num3 = 0;
					string text112 = "";
					string text113 = "";
					switch (tahsilatEvrakSatir.satir_cinsi)
					{
					case enum_tahsilat_cinsi.Nakit:
						flag14 = getBoolean6;
						satir_kasa_banka_kodu = getString12;
						num2 = getInt24;
						num3 = getInt25;
						text112 = getString13;
						text113 = getString14;
						break;
					case enum_tahsilat_cinsi.MusteriCeki:
						flag14 = getBoolean7;
						satir_kasa_banka_kodu = getString15;
						num2 = getInt26;
						num3 = getInt27;
						text112 = getString16;
						text113 = getString17;
						break;
					case enum_tahsilat_cinsi.MusteriSenedi:
						flag14 = getBoolean8;
						satir_kasa_banka_kodu = getString18;
						num2 = getInt28;
						num3 = getInt29;
						text112 = getString19;
						text113 = getString20;
						break;
					case enum_tahsilat_cinsi.MusteriKrediKarti:
						flag14 = getBoolean9;
						satir_kasa_banka_kodu = getString21;
						num2 = getInt30;
						num3 = getInt31;
						text112 = getString22;
						text113 = getString23;
						break;
					case enum_tahsilat_cinsi.GelenHavale:
						flag14 = getBoolean10;
						satir_kasa_banka_kodu = getString24;
						num2 = getInt32;
						num3 = getInt33;
						text112 = getString25;
						text113 = getString26;
						break;
					case enum_tahsilat_cinsi.GidenHavale:
						flag14 = getBoolean11;
						satir_kasa_banka_kodu = getString27;
						num2 = getInt34;
						num3 = getInt35;
						text112 = getString28;
						text113 = getString29;
						break;
					}
					if (flag14)
					{
						tahsilatEvrakSatir.satir_kasa_banka_kodu = satir_kasa_banka_kodu;
					}
					else
					{
						string satir_kasa_banka_kodu2 = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_kasa_banka_kodu2 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_kasa_banka_kodu = satir_kasa_banka_kodu2;
					}
					tahsilatEvrakSatir.satir_kasa_banka_kodu = text112 + tahsilatEvrakSatir.satir_kasa_banka_kodu + text113;
					if (getBoolean17)
					{
						tahsilatEvrakSatir.satir_aciklama = getString35;
					}
					else
					{
						num2 = getInt72;
						num3 = getInt73;
						string satir_aciklama = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_aciklama = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_aciklama = satir_aciklama;
					}
					if (getBoolean33)
					{
						tahsilatEvrakSatir.satir_aciklama = getString51 + tahsilatEvrakSatir.satir_aciklama;
					}
					if (getBoolean74)
					{
						double result11 = 0.0;
						double.TryParse(getString94.Trim().Replace(".", ","), out result11);
						tahsilatEvrakSatir.satir_tutar = result11;
					}
					else if (getInt198 != 0 || getInt199 != 0)
					{
						if (getBoolean3)
						{
							double result12 = 0.0;
							double.TryParse(array4[getInt198 - 1].Replace(".", ","), out result12);
							tahsilatEvrakSatir.satir_tutar = result12;
						}
						else
						{
							double result13 = 0.0;
							double.TryParse(text67.Substring(getInt198 - 1, getInt199).Trim().Replace(".", ","), out result13);
							tahsilatEvrakSatir.satir_tutar = result13;
						}
					}
					num = getInt200;
					num2 = getInt201;
					if (num != 0)
					{
						string text114 = "";
						if (num2 != 0 || num3 != 0)
						{
							text114 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						double result14 = 0.0;
						double.TryParse(text114.Replace(".", ","), out result14);
						switch (num)
						{
						case 1:
							tahsilatEvrakSatir.satir_tutar += result14;
							break;
						case 2:
							tahsilatEvrakSatir.satir_tutar -= result14;
							break;
						case 3:
							tahsilatEvrakSatir.satir_tutar /= result14;
							break;
						case 4:
							tahsilatEvrakSatir.satir_tutar *= result14;
							break;
						}
					}
					num = getInt202;
					num2 = getInt203;
					if (num != 0)
					{
						string text115 = "";
						if (num2 != 0 || num3 != 0)
						{
							text115 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						double result15 = 0.0;
						double.TryParse(text115.Replace(".", ","), out result15);
						switch (num)
						{
						case 1:
							tahsilatEvrakSatir.satir_tutar += result15;
							break;
						case 2:
							tahsilatEvrakSatir.satir_tutar -= result15;
							break;
						case 3:
							tahsilatEvrakSatir.satir_tutar /= result15;
							break;
						case 4:
							tahsilatEvrakSatir.satir_tutar *= result15;
							break;
						}
					}
					if (getBoolean75)
					{
						tahsilatEvrakSatir.satir_vadesi = DateTime.Parse(getString95);
					}
					else if (getInt204 != 0 || getInt205 != 0)
					{
						if (getBoolean3)
						{
							tahsilatEvrakSatir.satir_vadesi = DateTime.Parse(array4[getInt204 - 1]);
						}
						else
						{
							int num11 = int.Parse(text67.Substring(getInt204 - 1, getInt205).Trim());
							if (getInt205 == 2)
							{
								num11 += 2000;
							}
							int month3 = int.Parse(text67.Substring(getInt206 - 1, getInt207).Trim());
							int day3 = int.Parse(text67.Substring(getInt208 - 1, getInt209).Trim());
							tahsilatEvrakSatir.satir_vadesi = new DateTime(num11, month3, day3);
						}
					}
					if (tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.GelenHavale || tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.GidenHavale || tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.MusteriKrediKarti || tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.Nakit)
					{
						tahsilatEvrakSatir.satir_vadesi = tahsilatEvrakSatir.evraktarih;
					}
					string text116 = "";
					if (getBoolean76)
					{
						text116 = getString96;
					}
					else
					{
						num2 = getInt210;
						num3 = getInt211;
						string text117 = "";
						if (num2 != 0 || num3 != 0)
						{
							text117 = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						text116 = text117;
					}
					if (text116 != "")
					{
						tahsilatEvrakSatir.satir_sorumlulukmerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text116);
					}
					if (getBoolean77)
					{
						tahsilatEvrakSatir.satir_referans = getString97;
					}
					else
					{
						num2 = getInt212;
						num3 = getInt213;
						string satir_referans = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_referans = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_referans = satir_referans;
					}
					if (getBoolean78)
					{
						tahsilatEvrakSatir.satir_sck_banka_adres1 = getString98;
					}
					else
					{
						num2 = getInt214;
						num3 = getInt215;
						string satir_sck_banka_adres = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_sck_banka_adres = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_sck_banka_adres1 = satir_sck_banka_adres;
					}
					if (getBoolean79)
					{
						tahsilatEvrakSatir.satir_sck_bankano = getString99;
					}
					else
					{
						num2 = getInt216;
						num3 = getInt217;
						string satir_sck_bankano = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_sck_bankano = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_sck_bankano = satir_sck_bankano;
					}
					if (getBoolean80)
					{
						tahsilatEvrakSatir.satir_sck_borclu = getString100;
					}
					else
					{
						num2 = getInt218;
						num3 = getInt219;
						string satir_sck_borclu = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_sck_borclu = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_sck_borclu = satir_sck_borclu;
					}
					if (getBoolean81)
					{
						tahsilatEvrakSatir.satir_sck_hesapno_sehir = getString101;
					}
					else
					{
						num2 = getInt220;
						num3 = getInt221;
						string satir_sck_hesapno_sehir = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_sck_hesapno_sehir = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_sck_hesapno_sehir = satir_sck_hesapno_sehir;
					}
					if (getBoolean82)
					{
						tahsilatEvrakSatir.satir_sck_no = getString102;
					}
					else
					{
						num2 = getInt222;
						num3 = getInt223;
						string satir_sck_no = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_sck_no = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_sck_no = satir_sck_no;
					}
					if (getBoolean83)
					{
						tahsilatEvrakSatir.satir_sck_sube_adres2 = getString103;
					}
					else
					{
						num2 = getInt224;
						num3 = getInt225;
						string satir_sck_sube_adres = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_sck_sube_adres = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_sck_sube_adres2 = satir_sck_sube_adres;
					}
					if (getBoolean84)
					{
						tahsilatEvrakSatir.satir_Sck_TCMB_Banka_kodu = getString104;
					}
					else
					{
						num2 = getInt226;
						num3 = getInt227;
						string satir_Sck_TCMB_Banka_kodu = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_Sck_TCMB_Banka_kodu = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_Sck_TCMB_Banka_kodu = satir_Sck_TCMB_Banka_kodu;
					}
					if (getBoolean85)
					{
						tahsilatEvrakSatir.satir_Sck_TCMB_il_kodu = getString105;
					}
					else
					{
						num2 = getInt228;
						num3 = getInt229;
						string satir_Sck_TCMB_il_kodu = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_Sck_TCMB_il_kodu = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_Sck_TCMB_il_kodu = satir_Sck_TCMB_il_kodu;
					}
					if (getBoolean86)
					{
						tahsilatEvrakSatir.satir_Sck_TCMB_Sube_kodu = getString106;
					}
					else
					{
						num2 = getInt230;
						num3 = getInt231;
						string satir_Sck_TCMB_Sube_kodu = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_Sck_TCMB_Sube_kodu = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_Sck_TCMB_Sube_kodu = satir_Sck_TCMB_Sube_kodu;
					}
					if (getBoolean87)
					{
						tahsilatEvrakSatir.satir_sck_vdaire_no = getString107;
					}
					else
					{
						num2 = getInt232;
						num3 = getInt233;
						string satir_sck_vdaire_no = "";
						if (num2 != 0 || num3 != 0)
						{
							satir_sck_vdaire_no = ((!getBoolean3) ? text67.Substring(num2 - 1, num3).Trim() : array4[num2 - 1]);
						}
						tahsilatEvrakSatir.satir_sck_vdaire_no = satir_sck_vdaire_no;
					}
					bindingList.Add(tahsilatEvrakSatir);
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
		foreach (TahsilatEvrakSatir item7 in bindingList)
		{
			if (item7 != null)
			{
				_satirlar._satirlar.Add(item7);
			}
		}
		if (flag)
		{
			OtomatikEvrakSiraNoVer(satirBirlestir);
		}
	}

	private TahsilatEvrakSatir KriterleriUygula(TahsilatEvrakSatir satir)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		foreach (TahsilatEvrakKriter item in _uygulanacak_kriterler)
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
							string eskideger19 = satir.KayitID.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger19), out var result18);
							satir.KayitID = result18;
							break;
						}
						case enum_KriterDegistirmeAlanlari.evraktarih:
						{
							string eskideger18 = satir.evraktarih.ToString();
							DateTime result17 = satir.evraktarih;
							DateTime.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger18), out result17);
							satir.evraktarih = result17;
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
							string eskideger17 = satir.evraknosira.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger17), out var result16);
							satir.evraknosira = result16;
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
							string eskideger16 = satir.belgetarih.ToString();
							DateTime result15 = satir.belgetarih;
							DateTime.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger16), out result15);
							satir.belgetarih = result15;
							break;
						}
						case enum_KriterDegistirmeAlanlari.dovizcinsi:
						{
							string eskideger15 = satir.dovizcinsi.ToString();
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger15), out var result14);
							satir.dovizcinsi = result14;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kur:
						{
							string eskideger14 = satir.kur.dov_fiyat.ToString();
							double result13 = 1.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger14), out result13);
							satir.kur = new Kur();
							satir.kur.dov_fiyat = result13;
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
							string eskideger13 = satir.firma.fir_sirano.ToString();
							int result12 = 0;
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger13), out result12);
							satir.firma = FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, result12);
							break;
						}
						case enum_KriterDegistirmeAlanlari.sube:
						{
							string eskideger12 = satir.sube.Sube_no.ToString();
							int result11 = 0;
							int.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger12), out result11);
							satir.sube = SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, result11);
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
							string eskideger11 = satir.satir_cinsi.ToString();
							string value = KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger11);
							try
							{
								satir.satir_cinsi = GenelUtilityWin.ParseEnum<enum_tahsilat_cinsi>(value);
							}
							catch
							{
							}
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
							string eskideger10 = satir.kriter_double1.ToString();
							double result10 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger10), out result10);
							satir.kriter_double1 = result10;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double2:
						{
							string eskideger9 = satir.kriter_double2.ToString();
							double result9 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger9), out result9);
							satir.kriter_double2 = result9;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double3:
						{
							string eskideger8 = satir.kriter_double3.ToString();
							double result8 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger8), out result8);
							satir.kriter_double3 = result8;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double4:
						{
							string eskideger7 = satir.kriter_double4.ToString();
							double result7 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger7), out result7);
							satir.kriter_double4 = result7;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_double5:
						{
							string eskideger6 = satir.kriter_double5.ToString();
							double result6 = 0.0;
							double.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger6), out result6);
							satir.kriter_double5 = result6;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool1:
						{
							string eskideger5 = satir.kriter_bool1.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger5), out var result5);
							satir.kriter_bool1 = result5;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool2:
						{
							string eskideger4 = satir.kriter_bool2.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger4), out var result4);
							satir.kriter_bool2 = result4;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool3:
						{
							string eskideger3 = satir.kriter_bool3.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger3), out var result3);
							satir.kriter_bool3 = result3;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool4:
						{
							string eskideger2 = satir.kriter_bool4.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger2), out var result2);
							satir.kriter_bool4 = result2;
							break;
						}
						case enum_KriterDegistirmeAlanlari.kriter_bool5:
						{
							string eskideger = satir.kriter_bool5.ToString();
							bool.TryParse(KriterGercekYeniDegerBul(item8.islem_tipi, text2, eskideger), out var result);
							satir.kriter_bool5 = result;
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
							string satir_kasa_banka_kodu = satir.satir_kasa_banka_kodu;
							satir.satir_kasa_banka_kodu = KriterGercekYeniDegerBul(item8.islem_tipi, text2, satir_kasa_banka_kodu);
							break;
						}
						case enum_KriterDegistirmeAlanlari.evraktipi:
						case enum_KriterDegistirmeAlanlari.normaliade:
						case enum_KriterDegistirmeAlanlari.kapamasekli:
						case enum_KriterDegistirmeAlanlari.ticaretturu:
						case enum_KriterDegistirmeAlanlari.odemeplani:
						case enum_KriterDegistirmeAlanlari.fiyatlistesi:
						case enum_KriterDegistirmeAlanlari.kaynakdepo:
						case enum_KriterDegistirmeAlanlari.sevkteslimtarihi:
						case enum_KriterDegistirmeAlanlari.sevkadresno:
						case enum_KriterDegistirmeAlanlari.kapamahesapkodu:
						case enum_KriterDegistirmeAlanlari.faturaaciklama:
						case enum_KriterDegistirmeAlanlari.miktar:
						case enum_KriterDegistirmeAlanlari.miktar2:
						case enum_KriterDegistirmeAlanlari.vergi_pntr:
						case enum_KriterDegistirmeAlanlari.fiyat_fark_mi:
						case enum_KriterDegistirmeAlanlari.birimfiyat_brut_fiyat:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto1_uygulama_sekli:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto1_yuzdeveyatutar:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto2_uygulama_sekli:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto2_yuzdeveyatutar:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto3_uygulama_sekli:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto3_yuzdeveyatutar:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto4_uygulama_sekli:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto4_yuzdeveyatutar:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto5_uygulama_sekli:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto5_yuzdeveyatutar:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto6_uygulama_sekli:
						case enum_KriterDegistirmeAlanlari.birimfiyat_iskonto6_yuzdeveyatutar:
						case enum_KriterDegistirmeAlanlari.birimfiyat_otv_uygulama_sekli:
						case enum_KriterDegistirmeAlanlari.birimfiyat_otv_yuzdeveyatutar:
						case enum_KriterDegistirmeAlanlari.birimfiyat_otv_vergipntr:
							break;
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

	private string KriterAdayYeniDegerBul(TahsilatEvrakSatir satir, enum_DegerTipi deger_tipi, string ozel_deger)
	{
		string result = "";
		switch (deger_tipi)
		{
		case enum_DegerTipi.OzelDeger:
			result = ozel_deger;
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
			result = satir.satir_cinsi.ToString();
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

	private bool KriterVeriKontrolu(TahsilatEvrakSatir satir, enum_KriterAramaAlanlari aranacak_alan, enum_Operator kullanilan_operator, string aranacak_deger)
	{
		return aranacak_alan switch
		{
			enum_KriterAramaAlanlari.KayitID => KriterTekVeriKontrolu(kullanilan_operator, satir.KayitID.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.evraktarih => KriterTekVeriKontrolu(kullanilan_operator, satir.evraktarih.ToString("dd.MM.yyyy"), aranacak_deger), 
			enum_KriterAramaAlanlari.evraknoseri => KriterTekVeriKontrolu(kullanilan_operator, satir.evraknoseri, aranacak_deger), 
			enum_KriterAramaAlanlari.evraknosira => KriterTekVeriKontrolu(kullanilan_operator, satir.evraknosira.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.belgeno => KriterTekVeriKontrolu(kullanilan_operator, satir.belgeno, aranacak_deger), 
			enum_KriterAramaAlanlari.belgetarih => KriterTekVeriKontrolu(kullanilan_operator, satir.belgetarih.ToString("dd.MM.yyyy"), aranacak_deger), 
			enum_KriterAramaAlanlari.dovizcinsi_doviz_adi => KriterTekVeriKontrolu(kullanilan_operator, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(satir.dovizcinsi).Kur_adi, aranacak_deger), 
			enum_KriterAramaAlanlari.dovizcinsi_doviz_kod => KriterTekVeriKontrolu(kullanilan_operator, satir.dovizcinsi.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.dovizcinsi_doviz_sembol => KriterTekVeriKontrolu(kullanilan_operator, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(satir.dovizcinsi).Kur_sembol, aranacak_deger), 
			enum_KriterAramaAlanlari.kur => KriterTekVeriKontrolu(kullanilan_operator, satir.kur.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.proje_kodu => KriterTekVeriKontrolu(kullanilan_operator, satir.proje.pro_kodu, aranacak_deger), 
			enum_KriterAramaAlanlari.proje_adi => KriterTekVeriKontrolu(kullanilan_operator, satir.proje.pro_adi, aranacak_deger), 
			enum_KriterAramaAlanlari.sorumlulukmerkezi_kodu => KriterTekVeriKontrolu(kullanilan_operator, satir.sorumlulukmerkezi.som_kod, aranacak_deger), 
			enum_KriterAramaAlanlari.sorumlulukmerkezi_adi => KriterTekVeriKontrolu(kullanilan_operator, satir.sorumlulukmerkezi.som_isim, aranacak_deger), 
			enum_KriterAramaAlanlari.temsilcikodu => KriterTekVeriKontrolu(kullanilan_operator, satir.temsilcikodu, aranacak_deger), 
			enum_KriterAramaAlanlari.firma_sirano => KriterTekVeriKontrolu(kullanilan_operator, satir.firma.fir_sirano.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.firma_unvan => KriterTekVeriKontrolu(kullanilan_operator, satir.firma.fir_unvan, aranacak_deger), 
			enum_KriterAramaAlanlari.sube_no => KriterTekVeriKontrolu(kullanilan_operator, satir.sube.Sube_no.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.sube_adi => KriterTekVeriKontrolu(kullanilan_operator, satir.sube.Sube_adi, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama1 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama1, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama2 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama2, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama3 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama3, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama4 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama4, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama5 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama5, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama6 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama6, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama7 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama7, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama8 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama8, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama9 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama9, aranacak_deger), 
			enum_KriterAramaAlanlari.aciklama10 => KriterTekVeriKontrolu(kullanilan_operator, satir.aciklama10, aranacak_deger), 
			enum_KriterAramaAlanlari.satircinsi => KriterTekVeriKontrolu(kullanilan_operator, satir.satir_cinsi.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_string1 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string1, aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_string2 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string2, aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_string3 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string3, aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_string4 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string4, aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_string5 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_string5, aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_double1 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double1.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_double2 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double2.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_double3 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double3.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_double4 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double4.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_double5 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_double5.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_bool1 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool1.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_bool2 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool2.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_bool3 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool3.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_bool4 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool4.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.kriter_bool5 => KriterTekVeriKontrolu(kullanilan_operator, satir.kriter_bool5.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_kod => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_kod, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_unvan1 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_unvan1, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_unvan2 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_unvan2, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_muh_kod => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_muh_kod, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_muh_kod1 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_muh_kod1, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_muh_kod2 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_muh_kod2, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_vdaire_adi => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vdaire_adi, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_vdaire_no => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vdaire_no, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_Ana_cari_kodu => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_Ana_cari_kodu, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_bolge_kodu => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_bolge_kodu, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_grup_kodu => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_grup_kodu, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_temsilci_kodu => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_temsilci_kodu, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_sektor_kodu => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_sektor_kodu, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_satis_isk_kod => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_satis_isk_kod, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_special1 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_special1, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_special2 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_special2, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_special3 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_special3, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_sicil_no => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_sicil_no, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_VergiKimlikNo => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_VergiKimlikNo, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_vade_fark_yuz => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vade_fark_yuz.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_vade_fark_yuz1 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vade_fark_yuz1.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_vade_fark_yuz2 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_vade_fark_yuz2.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_tipi => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_tipi.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_doviz_cinsi => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_doviz_cinsi.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_doviz_cinsi1 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_doviz_cinsi1.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_doviz_cinsi2 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_doviz_cinsi2.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_odeme_gunu => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_odeme_gunu.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_hareket_tipi => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_hareket_tipi.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_odemeplan_no => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_odemeplan_no.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_satis_fk => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_satis_fk.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_KurHesapSekli => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_KurHesapSekli.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_odeme_cinsi => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_odeme_cinsi.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_fatura_adres_no => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_fatura_adres_no.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_sevk_adres_no => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_sevk_adres_no.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_banka_hesapno1 => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_banka_hesapno1, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_CepTel => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_CepTel, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_Email => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_Email, aranacak_deger), 
			enum_KriterAramaAlanlari.cari_VarsayilanGirisDepo => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_VarsayilanGirisDepo.ToString(), aranacak_deger), 
			enum_KriterAramaAlanlari.cari_VarsayilanCikisDepo => KriterTekVeriKontrolu(kullanilan_operator, satir.cari.cari_VarsayilanCikisDepo.ToString(), aranacak_deger), 
			_ => false, 
		};
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
		foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
		{
			if (item.Aktar && item.evraknosira == 0)
			{
				item.evraknosira = OtomatikTekSatirEvrakSiraNoVer(item, SatirBirlestir);
			}
		}
		gridControl_evraklar.RefreshDataSource();
	}

	private int OtomatikTekSatirEvrakSiraNoVer(TahsilatEvrakSatir satir, bool SatirBirlestir)
	{
		int num = 0;
		if (SatirBirlestir)
		{
			foreach (TahsilatEvrakSatir item in _satirlar._satirlar)
			{
				if (item.AktarimDurumu != enum_GenelEvrakAktarimDurumu.Aktarilmis && item.Aktar && item.evraknosira != 0 && item.firma.fir_sirano == satir.firma.fir_sirano && item.sube.Sube_no == satir.sube.Sube_no && item.evraknoseri == satir.evraknoseri && item.evraktarih.Year == satir.evraktarih.Year && item.evraktarih.Month == satir.evraktarih.Month && item.evraktarih.Day == satir.evraktarih.Day && item.belgeno == satir.belgeno && item.belgetarih.Year == satir.belgetarih.Year && item.belgetarih.Month == satir.belgetarih.Month && item.belgetarih.Day == satir.belgetarih.Day && item.cari.cari_kod == satir.cari.cari_kod && item.dovizcinsi == satir.dovizcinsi)
				{
					bool flag = false;
					bool flag2 = false;
					if (satir.satir_cinsi == enum_tahsilat_cinsi.GelenHavale || satir.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
					{
						flag = true;
					}
					if (item.satir_cinsi == enum_tahsilat_cinsi.GelenHavale || item.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
					{
						flag2 = true;
					}
					if (flag == flag2)
					{
						num = item.evraknosira;
					}
					break;
				}
			}
		}
		if (num == 0)
		{
			int num2 = 0;
			foreach (TahsilatEvrakSatir item2 in _satirlar._satirlar)
			{
				bool flag3 = false;
				bool flag4 = false;
				if (satir.satir_cinsi == enum_tahsilat_cinsi.GelenHavale || satir.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
				{
					flag3 = true;
				}
				if (item2.satir_cinsi == enum_tahsilat_cinsi.GelenHavale || item2.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
				{
					flag4 = true;
				}
				if (flag3 == flag4 && item2.AktarimDurumu != enum_GenelEvrakAktarimDurumu.Aktarilmis && item2.Aktar && item2.evraknosira != 0 && item2.evraknoseri == satir.evraknoseri && item2.evraknosira > num2)
				{
					num2 = item2.evraknosira;
				}
			}
			int num3 = 0;
			enum_GenelEvrakTipleri evraktipi = enum_GenelEvrakTipleri.Tahsilat;
			if (satir.satir_cinsi == enum_tahsilat_cinsi.GelenHavale)
			{
				evraktipi = enum_GenelEvrakTipleri.GelenHavale;
			}
			if (satir.satir_cinsi == enum_tahsilat_cinsi.GidenHavale)
			{
				evraktipi = enum_GenelEvrakTipleri.GidenHavale;
			}
			num3 = EvrakData.SonEvrakSiraNoBul(Db.Connection, evraktipi, satir.evraknoseri);
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

	private void YeniStoklariSorVeAc(List<Stok> olmayan_stoklar)
	{
		string text = "";
		foreach (Stok item in olmayan_stoklar)
		{
			text = text + item.sto_kod + " , ";
		}
		if (MessageBox.Show(olmayan_stoklar.Count + " adet yeni stok bulundu (" + text + "). Otomatik açılsın mı?", "ONAY", MessageBoxButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}
		foreach (Stok item2 in olmayan_stoklar)
		{
			StokData.YeniStokKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item2, _mikrouygulamabilgileri.mikrokullanici.User_no);
		}
		RepositoryItemlarinLookupTablelariniGuncelle();
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
		BindingList<TahsilatEvrakSatir> bindingList = new BindingList<TahsilatEvrakSatir>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		bool satirBirlestir = true;
		try
		{
			Parametreler parametreler = ParametrelerDefault.TahsilatAktarimSqlSablon(AktarimAyarlariAdi);
			ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler, "TahsilatAktarim", "", "SqlAktarimSablon", AktarimAyarlariAdi);
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
			bool getBoolean = parametreler._GetParametre("satir_cinsi_sabit_kullan")._GetBoolean;
			int getInt8 = parametreler._GetParametre("satir_cinsi_sabit_deger")._GetInt;
			int getInt9 = parametreler._GetParametre("satir_cinsi_baslangic")._GetInt;
			string getString7 = parametreler._GetParametre("satir_cinsi_veri_nakit")._GetString;
			string getString8 = parametreler._GetParametre("satir_cinsi_veri_musteri_ceki")._GetString;
			string getString9 = parametreler._GetParametre("satir_cinsi_veri_musteri_seneti")._GetString;
			string getString10 = parametreler._GetParametre("satir_cinsi_veri_musteri_kredi_karti")._GetString;
			string getString11 = parametreler._GetParametre("satir_cinsi_veri_gelen_havale")._GetString;
			string getString12 = parametreler._GetParametre("satir_cinsi_veri_giden_havale")._GetString;
			bool getBoolean2 = parametreler._GetParametre("satir_hesap_kodu_nakit_sabit_kullan")._GetBoolean;
			string getString13 = parametreler._GetParametre("satir_hesap_kodu_nakit_sabit_deger")._GetString;
			int getInt10 = parametreler._GetParametre("satir_hesap_kodu_nakit_baslangic")._GetInt;
			string getString14 = parametreler._GetParametre("satir_hesap_kodu_nakit_on_ek")._GetString;
			string getString15 = parametreler._GetParametre("satir_hesap_kodu_nakit_son_ek")._GetString;
			bool getBoolean3 = parametreler._GetParametre("satir_hesap_kodu_cek_sabit_kullan")._GetBoolean;
			string getString16 = parametreler._GetParametre("satir_hesap_kodu_cek_sabit_deger")._GetString;
			int getInt11 = parametreler._GetParametre("satir_hesap_kodu_cek_baslangic")._GetInt;
			string getString17 = parametreler._GetParametre("satir_hesap_kodu_cek_on_ek")._GetString;
			string getString18 = parametreler._GetParametre("satir_hesap_kodu_cek_son_ek")._GetString;
			bool getBoolean4 = parametreler._GetParametre("satir_hesap_kodu_senet_sabit_kullan")._GetBoolean;
			string getString19 = parametreler._GetParametre("satir_hesap_kodu_senet_sabit_deger")._GetString;
			int getInt12 = parametreler._GetParametre("satir_hesap_kodu_senet_baslangic")._GetInt;
			string getString20 = parametreler._GetParametre("satir_hesap_kodu_senet_on_ek")._GetString;
			string getString21 = parametreler._GetParametre("satir_hesap_kodu_senet_son_ek")._GetString;
			bool getBoolean5 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_sabit_kullan")._GetBoolean;
			string getString22 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_sabit_deger")._GetString;
			int getInt13 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_baslangic")._GetInt;
			string getString23 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_on_ek")._GetString;
			string getString24 = parametreler._GetParametre("satir_hesap_kodu_kredi_karti_son_ek")._GetString;
			bool getBoolean6 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_sabit_kullan")._GetBoolean;
			string getString25 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_sabit_deger")._GetString;
			int getInt14 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_baslangic")._GetInt;
			string getString26 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_on_ek")._GetString;
			string getString27 = parametreler._GetParametre("satir_hesap_kodu_gelen_havale_son_ek")._GetString;
			bool getBoolean7 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_sabit_kullan")._GetBoolean;
			string getString28 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_sabit_deger")._GetString;
			int getInt15 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_baslangic")._GetInt;
			string getString29 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_on_ek")._GetString;
			string getString30 = parametreler._GetParametre("satir_hesap_kodu_giden_havale_son_ek")._GetString;
			bool getBoolean8 = parametreler._GetParametre("cari_kod_sabit_kullan")._GetBoolean;
			string getString31 = parametreler._GetParametre("cari_kod_sabit_deger")._GetString;
			int getInt16 = parametreler._GetParametre("cari_kod_baslangic")._GetInt;
			string getString32 = parametreler._GetParametre("cari_arama_secenekleri")._GetString;
			_ = parametreler._GetParametre("cari_unvan_turkce_karakterleri_kaldir")._GetBoolean;
			int getInt17 = parametreler._GetParametre("cari_unvan_baslangic")._GetInt;
			int getInt18 = parametreler._GetParametre("cari_unvan2_baslangic")._GetInt;
			int getInt19 = parametreler._GetParametre("cari_vergi_no_baslangic")._GetInt;
			int getInt20 = parametreler._GetParametre("cari_tc_kimlik_no_baslangic")._GetInt;
			int getInt21 = parametreler._GetParametre("cari_vergi_dairesi_baslangic")._GetInt;
			int getInt22 = parametreler._GetParametre("cari_banka_hesap_no_baslangic")._GetInt;
			int getInt23 = parametreler._GetParametre("cari_adres_baslangic")._GetInt;
			int getInt24 = parametreler._GetParametre("cari_mahalle_baslangic")._GetInt;
			int getInt25 = parametreler._GetParametre("cari_ilce_baslangic")._GetInt;
			int getInt26 = parametreler._GetParametre("cari_il_baslangic")._GetInt;
			int getInt27 = parametreler._GetParametre("cari_ulke_baslangic")._GetInt;
			int getInt28 = parametreler._GetParametre("cari_posta_kodu_baslangic")._GetInt;
			int getInt29 = parametreler._GetParametre("cari_telefon_baslangic")._GetInt;
			int getInt30 = parametreler._GetParametre("cari_eposta_baslangic")._GetInt;
			bool getBoolean9 = parametreler._GetParametre("proje_kodu_sabit_kullan")._GetBoolean;
			string getString33 = parametreler._GetParametre("proje_kodu_sabit_deger")._GetString;
			int getInt31 = parametreler._GetParametre("proje_kodu_baslangic")._GetInt;
			bool getBoolean10 = parametreler._GetParametre("sor_mer_kodu_sabit_kullan")._GetBoolean;
			string getString34 = parametreler._GetParametre("sor_mer_kodu_sabit_deger")._GetString;
			int getInt32 = parametreler._GetParametre("sor_mer_kodu_baslangic")._GetInt;
			bool getBoolean11 = parametreler._GetParametre("plasiyer_kodu_sabit_kullan")._GetBoolean;
			bool getBoolean12 = parametreler._GetParametre("plasiyer_kodu_cariden_kullan")._GetBoolean;
			string getString35 = parametreler._GetParametre("plasiyer_kodu_sabit_deger")._GetString;
			int getInt33 = parametreler._GetParametre("plasiyer_kodu_baslangic")._GetInt;
			bool getBoolean13 = parametreler._GetParametre("satir_aciklama_sabit_kullan")._GetBoolean;
			string getString36 = parametreler._GetParametre("satir_aciklama_sabit_deger")._GetString;
			int getInt34 = parametreler._GetParametre("satir_aciklama_baslangic")._GetInt;
			bool getBoolean14 = parametreler._GetParametre("aciklama1_sabit_kullan")._GetBoolean;
			string getString37 = parametreler._GetParametre("aciklama1_sabit_deger")._GetString;
			int getInt35 = parametreler._GetParametre("aciklama1_baslangic")._GetInt;
			bool getBoolean15 = parametreler._GetParametre("aciklama2_sabit_kullan")._GetBoolean;
			string getString38 = parametreler._GetParametre("aciklama2_sabit_deger")._GetString;
			int getInt36 = parametreler._GetParametre("aciklama2_baslangic")._GetInt;
			bool getBoolean16 = parametreler._GetParametre("aciklama3_sabit_kullan")._GetBoolean;
			string getString39 = parametreler._GetParametre("aciklama3_sabit_deger")._GetString;
			int getInt37 = parametreler._GetParametre("aciklama3_baslangic")._GetInt;
			bool getBoolean17 = parametreler._GetParametre("aciklama4_sabit_kullan")._GetBoolean;
			string getString40 = parametreler._GetParametre("aciklama4_sabit_deger")._GetString;
			int getInt38 = parametreler._GetParametre("aciklama4_baslangic")._GetInt;
			bool getBoolean18 = parametreler._GetParametre("aciklama5_sabit_kullan")._GetBoolean;
			string getString41 = parametreler._GetParametre("aciklama5_sabit_deger")._GetString;
			int getInt39 = parametreler._GetParametre("aciklama5_baslangic")._GetInt;
			bool getBoolean19 = parametreler._GetParametre("aciklama6_sabit_kullan")._GetBoolean;
			string getString42 = parametreler._GetParametre("aciklama6_sabit_deger")._GetString;
			int getInt40 = parametreler._GetParametre("aciklama6_baslangic")._GetInt;
			bool getBoolean20 = parametreler._GetParametre("aciklama7_sabit_kullan")._GetBoolean;
			string getString43 = parametreler._GetParametre("aciklama7_sabit_deger")._GetString;
			int getInt41 = parametreler._GetParametre("aciklama7_baslangic")._GetInt;
			bool getBoolean21 = parametreler._GetParametre("aciklama8_sabit_kullan")._GetBoolean;
			string getString44 = parametreler._GetParametre("aciklama8_sabit_deger")._GetString;
			int getInt42 = parametreler._GetParametre("aciklama8_baslangic")._GetInt;
			bool getBoolean22 = parametreler._GetParametre("aciklama9_sabit_kullan")._GetBoolean;
			string getString45 = parametreler._GetParametre("aciklama9_sabit_deger")._GetString;
			int getInt43 = parametreler._GetParametre("aciklama9_baslangic")._GetInt;
			bool getBoolean23 = parametreler._GetParametre("aciklama10_sabit_kullan")._GetBoolean;
			string getString46 = parametreler._GetParametre("aciklama10_sabit_deger")._GetString;
			int getInt44 = parametreler._GetParametre("aciklama10_baslangic")._GetInt;
			bool getBoolean24 = parametreler._GetParametre("ozel_alan_1_sabit_kullan")._GetBoolean;
			string getString47 = parametreler._GetParametre("ozel_alan_1_sabit_deger")._GetString;
			int getInt45 = parametreler._GetParametre("ozel_alan_1_baslangic")._GetInt;
			bool getBoolean25 = parametreler._GetParametre("ozel_alan_2_sabit_kullan")._GetBoolean;
			string getString48 = parametreler._GetParametre("ozel_alan_2_sabit_deger")._GetString;
			int getInt46 = parametreler._GetParametre("ozel_alan_2_baslangic")._GetInt;
			bool getBoolean26 = parametreler._GetParametre("ozel_alan_3_sabit_kullan")._GetBoolean;
			string getString49 = parametreler._GetParametre("ozel_alan_3_sabit_deger")._GetString;
			int getInt47 = parametreler._GetParametre("ozel_alan_3_baslangic")._GetInt;
			bool getBoolean27 = parametreler._GetParametre("evrak_seri_sabit_kullan")._GetBoolean;
			string getString50 = parametreler._GetParametre("evrak_seri_sabit_deger")._GetString;
			int getInt48 = parametreler._GetParametre("evrak_seri_baslangic")._GetInt;
			int getInt49 = parametreler._GetParametre("evrak_sira_baslangic")._GetInt;
			bool getBoolean28 = parametreler._GetParametre("cari_kodu_on_ek_kullan")._GetBoolean;
			string getString51 = parametreler._GetParametre("cari_kodu_on_ek_satis")._GetString;
			_ = parametreler._GetParametre("cari_kodu_on_ek_alis")._GetString;
			bool getBoolean29 = parametreler._GetParametre("satir_aciklama_on_ek_kullan")._GetBoolean;
			string getString52 = parametreler._GetParametre("satir_aciklama_on_ek_deger")._GetString;
			bool getBoolean30 = parametreler._GetParametre("aciklama1_on_ek_kullan")._GetBoolean;
			string getString53 = parametreler._GetParametre("aciklama1_on_ek_deger")._GetString;
			bool getBoolean31 = parametreler._GetParametre("aciklama2_on_ek_kullan")._GetBoolean;
			string getString54 = parametreler._GetParametre("aciklama2_on_ek_deger")._GetString;
			bool getBoolean32 = parametreler._GetParametre("aciklama3_on_ek_kullan")._GetBoolean;
			string getString55 = parametreler._GetParametre("aciklama3_on_ek_deger")._GetString;
			bool getBoolean33 = parametreler._GetParametre("aciklama4_on_ek_kullan")._GetBoolean;
			string getString56 = parametreler._GetParametre("aciklama4_on_ek_deger")._GetString;
			bool getBoolean34 = parametreler._GetParametre("aciklama5_on_ek_kullan")._GetBoolean;
			string getString57 = parametreler._GetParametre("aciklama5_on_ek_deger")._GetString;
			bool getBoolean35 = parametreler._GetParametre("aciklama6_on_ek_kullan")._GetBoolean;
			string getString58 = parametreler._GetParametre("aciklama6_on_ek_deger")._GetString;
			bool getBoolean36 = parametreler._GetParametre("aciklama7_on_ek_kullan")._GetBoolean;
			string getString59 = parametreler._GetParametre("aciklama7_on_ek_deger")._GetString;
			bool getBoolean37 = parametreler._GetParametre("aciklama8_on_ek_kullan")._GetBoolean;
			string getString60 = parametreler._GetParametre("aciklama8_on_ek_deger")._GetString;
			bool getBoolean38 = parametreler._GetParametre("aciklama9_on_ek_kullan")._GetBoolean;
			string getString61 = parametreler._GetParametre("aciklama9_on_ek_deger")._GetString;
			bool getBoolean39 = parametreler._GetParametre("aciklama10_on_ek_kullan")._GetBoolean;
			string getString62 = parametreler._GetParametre("aciklama10_on_ek_deger")._GetString;
			bool getBoolean40 = parametreler._GetParametre("cari_il_bilgisi_plaka_kodu")._GetBoolean;
			int getInt50 = parametreler._GetParametre("cari_doviz_cinsi")._GetInt;
			int getInt51 = parametreler._GetParametre("cari_doviz_cinsi1")._GetInt;
			int getInt52 = parametreler._GetParametre("cari_doviz_cinsi2")._GetInt;
			string getString63 = parametreler._GetParametre("cari_muh_kod_satis")._GetString;
			string getString64 = parametreler._GetParametre("cari_muh_kod1_satis")._GetString;
			string getString65 = parametreler._GetParametre("cari_muh_kod2_satis")._GetString;
			string getString66 = parametreler._GetParametre("cari_muhartikeli")._GetString;
			int getInt53 = parametreler._GetParametre("cari_kod2_baslangic")._GetInt;
			int getInt54 = parametreler._GetParametre("kriter_metin1_baslangic")._GetInt;
			int getInt55 = parametreler._GetParametre("kriter_metin2_baslangic")._GetInt;
			int getInt56 = parametreler._GetParametre("kriter_metin3_baslangic")._GetInt;
			int getInt57 = parametreler._GetParametre("kriter_metin4_baslangic")._GetInt;
			int getInt58 = parametreler._GetParametre("kriter_metin5_baslangic")._GetInt;
			int getInt59 = parametreler._GetParametre("kriter_double1_baslangic")._GetInt;
			int getInt60 = parametreler._GetParametre("kriter_double2_baslangic")._GetInt;
			int getInt61 = parametreler._GetParametre("kriter_double3_baslangic")._GetInt;
			int getInt62 = parametreler._GetParametre("kriter_double4_baslangic")._GetInt;
			int getInt63 = parametreler._GetParametre("kriter_double5_baslangic")._GetInt;
			int getInt64 = parametreler._GetParametre("kriter_bool1_baslangic")._GetInt;
			string getString67 = parametreler._GetParametre("kriter_bool1_evet_icin_deger")._GetString;
			int getInt65 = parametreler._GetParametre("kriter_bool2_baslangic")._GetInt;
			string getString68 = parametreler._GetParametre("kriter_bool2_evet_icin_deger")._GetString;
			int getInt66 = parametreler._GetParametre("kriter_bool3_baslangic")._GetInt;
			string getString69 = parametreler._GetParametre("kriter_bool3_evet_icin_deger")._GetString;
			int getInt67 = parametreler._GetParametre("kriter_bool4_baslangic")._GetInt;
			string getString70 = parametreler._GetParametre("kriter_bool4_evet_icin_deger")._GetString;
			int getInt68 = parametreler._GetParametre("kriter_bool5_baslangic")._GetInt;
			string getString71 = parametreler._GetParametre("kriter_bool5_evet_icin_deger")._GetString;
			bool getBoolean41 = parametreler._GetParametre("kayit_id_otomatik_ver")._GetBoolean;
			int getInt69 = parametreler._GetParametre("kayit_id_baslangic")._GetInt;
			int getInt70 = parametreler._GetParametre("otomatik_hesap_acma_secenek_cari")._GetInt;
			int getInt71 = parametreler._GetParametre("otomatik_hesap_acma_secenek_proje")._GetInt;
			int getInt72 = parametreler._GetParametre("otomatik_hesap_acma_secenek_sorumluluk")._GetInt;
			flag = parametreler._GetParametre("evrak_sira_otomatik_ver")._GetBoolean;
			bool getBoolean42 = parametreler._GetParametre("pro_adi_sabit_kullan")._GetBoolean;
			string getString72 = parametreler._GetParametre("pro_adi_sabit_deger")._GetString;
			int getInt73 = parametreler._GetParametre("pro_adi_baslangic")._GetInt;
			bool getBoolean43 = parametreler._GetParametre("pro_musterikodu_sabit_kullan")._GetBoolean;
			string getString73 = parametreler._GetParametre("pro_musterikodu_sabit_deger")._GetString;
			int getInt74 = parametreler._GetParametre("pro_musterikodu_baslangic")._GetInt;
			bool getBoolean44 = parametreler._GetParametre("pro_sormerkodu_sabit_kullan")._GetBoolean;
			string getString74 = parametreler._GetParametre("pro_sormerkodu_sabit_deger")._GetString;
			int getInt75 = parametreler._GetParametre("pro_sormerkodu_baslangic")._GetInt;
			bool getBoolean45 = parametreler._GetParametre("pro_grupkodu_sabit_kullan")._GetBoolean;
			string getString75 = parametreler._GetParametre("pro_grupkodu_sabit_deger")._GetString;
			int getInt76 = parametreler._GetParametre("pro_grupkodu_baslangic")._GetInt;
			bool getBoolean46 = parametreler._GetParametre("pro_sektorkodu_sabit_kullan")._GetBoolean;
			string getString76 = parametreler._GetParametre("pro_sektorkodu_sabit_deger")._GetString;
			int getInt77 = parametreler._GetParametre("pro_sektorkodu_baslangic")._GetInt;
			bool getBoolean47 = parametreler._GetParametre("pro_bolgekodu_sabit_kullan")._GetBoolean;
			string getString77 = parametreler._GetParametre("pro_bolgekodu_sabit_deger")._GetString;
			int getInt78 = parametreler._GetParametre("pro_bolgekodu_baslangic")._GetInt;
			bool getBoolean48 = parametreler._GetParametre("pro_ana_projekodu_sabit_kullan")._GetBoolean;
			string getString78 = parametreler._GetParametre("pro_ana_projekodu_sabit_deger")._GetString;
			int getInt79 = parametreler._GetParametre("pro_ana_projekodu_baslangic")._GetInt;
			bool getBoolean49 = parametreler._GetParametre("pro_aciklama_sabit_kullan")._GetBoolean;
			string getString79 = parametreler._GetParametre("pro_aciklama_sabit_deger")._GetString;
			int getInt80 = parametreler._GetParametre("pro_aciklama_baslangic")._GetInt;
			bool getBoolean50 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_kullan")._GetBoolean;
			string getString80 = parametreler._GetParametre("pro_muh_kod_artikeli_sabit_deger")._GetString;
			int getInt81 = parametreler._GetParametre("pro_muh_kod_artikeli_baslangic")._GetInt;
			bool getBoolean51 = parametreler._GetParametre("som_isim_sabit_kullan")._GetBoolean;
			string getString81 = parametreler._GetParametre("som_isim_sabit_deger")._GetString;
			int getInt82 = parametreler._GetParametre("som_isim_baslangic")._GetInt;
			bool getBoolean52 = parametreler._GetParametre("som_MuhArtikeli_sabit_kullan")._GetBoolean;
			string getString82 = parametreler._GetParametre("som_MuhArtikeli_sabit_deger")._GetString;
			int getInt83 = parametreler._GetParametre("som_MuhArtikeli_baslangic")._GetInt;
			bool getBoolean53 = parametreler._GetParametre("cari_muhartikeli_sabit_kullan")._GetBoolean;
			int getInt84 = parametreler._GetParametre("cari_muhartikeli_baslangic")._GetInt;
			bool getBoolean54 = parametreler._GetParametre("cari_muh_kod_satis_sabit_kullan")._GetBoolean;
			int getInt85 = parametreler._GetParametre("cari_muh_kod_satis_baslangic")._GetInt;
			bool getBoolean55 = parametreler._GetParametre("cari_muh_kod1_satis_sabit_kullan")._GetBoolean;
			int getInt86 = parametreler._GetParametre("cari_muh_kod1_satis_baslangic")._GetInt;
			bool getBoolean56 = parametreler._GetParametre("cari_muh_kod2_satis_sabit_kullan")._GetBoolean;
			int getInt87 = parametreler._GetParametre("cari_muh_kod2_satis_baslangic")._GetInt;
			bool getBoolean57 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_kullan")._GetBoolean;
			string getString83 = parametreler._GetParametre("cari_Ana_cari_kodu_sabit_deger")._GetString;
			int getInt88 = parametreler._GetParametre("cari_Ana_cari_kodu_baslangic")._GetInt;
			bool getBoolean58 = parametreler._GetParametre("cari_temsilci_kodu_sabit_kullan")._GetBoolean;
			string getString84 = parametreler._GetParametre("cari_temsilci_kodu_sabit_deger")._GetString;
			int getInt89 = parametreler._GetParametre("cari_temsilci_kodu_baslangic")._GetInt;
			bool getBoolean59 = parametreler._GetParametre("cari_grup_kodu_sabit_kullan")._GetBoolean;
			string getString85 = parametreler._GetParametre("cari_grup_kodu_sabit_deger")._GetString;
			int getInt90 = parametreler._GetParametre("cari_grup_kodu_baslangic")._GetInt;
			bool getBoolean60 = parametreler._GetParametre("cari_sektor_kodu_sabit_kullan")._GetBoolean;
			string getString86 = parametreler._GetParametre("cari_sektor_kodu_sabit_deger")._GetString;
			int getInt91 = parametreler._GetParametre("cari_sektor_kodu_baslangic")._GetInt;
			bool getBoolean61 = parametreler._GetParametre("cari_bolge_kodu_sabit_kullan")._GetBoolean;
			string getString87 = parametreler._GetParametre("cari_bolge_kodu_sabit_deger")._GetString;
			int getInt92 = parametreler._GetParametre("cari_bolge_kodu_baslangic")._GetInt;
			bool getBoolean62 = parametreler._GetParametre("cari_wwwadresi_sabit_kullan")._GetBoolean;
			string getString88 = parametreler._GetParametre("cari_wwwadresi_sabit_deger")._GetString;
			int getInt93 = parametreler._GetParametre("cari_wwwadresi_baslangic")._GetInt;
			bool getBoolean63 = parametreler._GetParametre("cari_CepTel_sabit_kullan")._GetBoolean;
			string getString89 = parametreler._GetParametre("cari_CepTel_sabit_deger")._GetString;
			int getInt94 = parametreler._GetParametre("cari_CepTel_baslangic")._GetInt;
			bool getBoolean64 = parametreler._GetParametre("cari_satis_isk_kod_sabit_kullan")._GetBoolean;
			string getString90 = parametreler._GetParametre("cari_satis_isk_kod_sabit_deger")._GetString;
			int getInt95 = parametreler._GetParametre("cari_satis_isk_kod_baslangic")._GetInt;
			bool getBoolean65 = parametreler._GetParametre("cari_sicil_no_sabit_kullan")._GetBoolean;
			string getString91 = parametreler._GetParametre("cari_sicil_no_sabit_deger")._GetString;
			int getInt96 = parametreler._GetParametre("cari_sicil_no_baslangic")._GetInt;
			bool getBoolean66 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_kullan")._GetBoolean;
			string getString92 = parametreler._GetParametre("cari_VarsayilanGirisDepo_sabit_deger")._GetString;
			int getInt97 = parametreler._GetParametre("cari_VarsayilanGirisDepo_baslangic")._GetInt;
			bool getBoolean67 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_kullan")._GetBoolean;
			string getString93 = parametreler._GetParametre("cari_VarsayilanCikisDepo_sabit_deger")._GetString;
			int getInt98 = parametreler._GetParametre("cari_VarsayilanCikisDepo_baslangic")._GetInt;
			bool getBoolean68 = parametreler._GetParametre("cari_Portal_PW_sabit_kullan")._GetBoolean;
			string getString94 = parametreler._GetParametre("cari_Portal_PW_sabit_deger")._GetString;
			int getInt99 = parametreler._GetParametre("cari_Portal_PW_baslangic")._GetInt;
			bool getBoolean69 = parametreler._GetParametre("cari_Portal_Enabled")._GetBoolean;
			bool getBoolean70 = parametreler._GetParametre("satir_tutar_sabit_kullan")._GetBoolean;
			string getString95 = parametreler._GetParametre("satir_tutar_sabit_deger")._GetString;
			int getInt100 = parametreler._GetParametre("satir_tutar_baslangic")._GetInt;
			int getInt101 = parametreler._GetParametre("satir_tutar_islem_1_islem_tipi")._GetInt;
			int getInt102 = parametreler._GetParametre("satir_tutar_islem_1_baslangic")._GetInt;
			int getInt103 = parametreler._GetParametre("satir_tutar_islem_2_islem_tipi")._GetInt;
			int getInt104 = parametreler._GetParametre("satir_tutar_islem_2_baslangic")._GetInt;
			bool getBoolean71 = parametreler._GetParametre("satir_vadesi_sabit_kullan")._GetBoolean;
			string getString96 = parametreler._GetParametre("satir_vadesi_sabit_deger")._GetString;
			int getInt105 = parametreler._GetParametre("satir_vadesi_baslangic")._GetInt;
			bool getBoolean72 = parametreler._GetParametre("satir_sorumlulukmerkezi_sabit_kullan")._GetBoolean;
			string getString97 = parametreler._GetParametre("satir_sorumlulukmerkezi_sabit_deger")._GetString;
			int getInt106 = parametreler._GetParametre("satir_sorumlulukmerkezi_baslangic")._GetInt;
			bool getBoolean73 = parametreler._GetParametre("satir_referans_sabit_kullan")._GetBoolean;
			string getString98 = parametreler._GetParametre("satir_referans_sabit_deger")._GetString;
			int getInt107 = parametreler._GetParametre("satir_referans_baslangic")._GetInt;
			bool getBoolean74 = parametreler._GetParametre("satir_sck_banka_adres1_sabit_kullan")._GetBoolean;
			string getString99 = parametreler._GetParametre("satir_sck_banka_adres1_sabit_deger")._GetString;
			int getInt108 = parametreler._GetParametre("satir_sck_banka_adres1_baslangic")._GetInt;
			bool getBoolean75 = parametreler._GetParametre("satir_sck_bankano_sabit_kullan")._GetBoolean;
			string getString100 = parametreler._GetParametre("satir_sck_bankano_sabit_deger")._GetString;
			int getInt109 = parametreler._GetParametre("satir_sck_bankano_baslangic")._GetInt;
			bool getBoolean76 = parametreler._GetParametre("satir_sck_borclu_sabit_kullan")._GetBoolean;
			string getString101 = parametreler._GetParametre("satir_sck_borclu_sabit_deger")._GetString;
			int getInt110 = parametreler._GetParametre("satir_sck_borclu_baslangic")._GetInt;
			bool getBoolean77 = parametreler._GetParametre("satir_sck_hesapno_sehir_sabit_kullan")._GetBoolean;
			string getString102 = parametreler._GetParametre("satir_sck_hesapno_sehir_sabit_deger")._GetString;
			int getInt111 = parametreler._GetParametre("satir_sck_hesapno_sehir_baslangic")._GetInt;
			bool getBoolean78 = parametreler._GetParametre("satir_sck_no_sabit_kullan")._GetBoolean;
			string getString103 = parametreler._GetParametre("satir_sck_no_sabit_deger")._GetString;
			int getInt112 = parametreler._GetParametre("satir_sck_no_baslangic")._GetInt;
			bool getBoolean79 = parametreler._GetParametre("satir_sck_sube_adres2_sabit_kullan")._GetBoolean;
			string getString104 = parametreler._GetParametre("satir_sck_sube_adres2_sabit_deger")._GetString;
			int getInt113 = parametreler._GetParametre("satir_sck_sube_adres2_baslangic")._GetInt;
			bool getBoolean80 = parametreler._GetParametre("satir_sck_TCMB_Banka_kodu_sabit_kullan")._GetBoolean;
			string getString105 = parametreler._GetParametre("satir_sck_TCMB_Banka_kodu_sabit_deger")._GetString;
			int getInt114 = parametreler._GetParametre("satir_sck_TCMB_Banka_kodu_baslangic")._GetInt;
			bool getBoolean81 = parametreler._GetParametre("satir_sck_TCMB_il_kodu_sabit_kullan")._GetBoolean;
			string getString106 = parametreler._GetParametre("satir_sck_TCMB_il_kodu_sabit_deger")._GetString;
			int getInt115 = parametreler._GetParametre("satir_sck_TCMB_il_kodu_baslangic")._GetInt;
			bool getBoolean82 = parametreler._GetParametre("satir_sck_TCMB_Sube_kodu_sabit_kullan")._GetBoolean;
			string getString107 = parametreler._GetParametre("satir_sck_TCMB_Sube_kodu_sabit_deger")._GetString;
			int getInt116 = parametreler._GetParametre("satir_sck_TCMB_Sube_kodu_baslangic")._GetInt;
			bool getBoolean83 = parametreler._GetParametre("satir_sck_vdaire_no_sabit_kullan")._GetBoolean;
			string getString108 = parametreler._GetParametre("satir_sck_vdaire_no_sabit_deger")._GetString;
			int getInt117 = parametreler._GetParametre("satir_sck_vdaire_no_baslangic")._GetInt;
			_uygulanacak_kriterler = new List<TahsilatEvrakKriter>();
			string[] array = getString.Split(',');
			foreach (string text in array)
			{
				if (text != "")
				{
					TahsilatEvrakKriter tahsilatEvrakKriter = new TahsilatEvrakKriter();
					Parametreler parametreler2 = ParametrelerDefault.TahsilatAktarimKriter(text);
					ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, parametreler2, "TahsilatAktarim", "", "Kriter", text);
					tahsilatEvrakKriter.arama_alan1_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_baglac")._GetInt;
					tahsilatEvrakKriter.arama_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan2_baglac")._GetInt;
					tahsilatEvrakKriter.arama_alan1_alan2_baglac = (enum_Baglac)parametreler2._GetParametre("arama_alan1_alan2_baglac")._GetInt;
					string getString109 = parametreler2._GetParametre("arama_alan1")._GetString;
					string getString110 = parametreler2._GetParametre("arama_alan2")._GetString;
					string getString111 = parametreler2._GetParametre("degistirilecek_alanlar")._GetString;
					if (getString109 != "")
					{
						tahsilatEvrakKriter.arama_alan1 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString109);
					}
					else
					{
						tahsilatEvrakKriter.arama_alan1 = new List<AlanOperatorDeger>();
					}
					if (getString110 != "")
					{
						tahsilatEvrakKriter.arama_alan2 = JSON.Instance.ToObject<List<AlanOperatorDeger>>(getString110);
					}
					else
					{
						tahsilatEvrakKriter.arama_alan2 = new List<AlanOperatorDeger>();
					}
					if (getString111 != "")
					{
						tahsilatEvrakKriter.degistirilecek_alanlar = JSON.Instance.ToObject<List<AlanIslemTipiDeger>>(getString111);
					}
					else
					{
						tahsilatEvrakKriter.degistirilecek_alanlar = new List<AlanIslemTipiDeger>();
					}
					_uygulanacak_kriterler.Add(tahsilatEvrakKriter);
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
			new List<Stok>();
			new List<Hizmet>();
			List<Proje> list2 = new List<Proje>();
			List<SorumlulukMerkezi> list3 = new List<SorumlulukMerkezi>();
			num3 = 0;
			while (sqlDataReader.Read())
			{
				bool flag2 = true;
				if (true)
				{
					string text2 = "";
					if (getInt19 != 0)
					{
						text2 = sqlDataReader[getInt19 - 1].ToString().Trim();
					}
					if (text2 == "" && getInt20 != 0)
					{
						text2 = sqlDataReader[getInt20 - 1].ToString().Trim();
					}
					string text3 = "";
					if (getInt17 != 0)
					{
						text3 = sqlDataReader[getInt17 - 1].ToString().Trim();
					}
					if (text3.Length > 50)
					{
						text3 = text3.Substring(0, 50);
					}
					string text4 = "";
					if (getInt18 != 0)
					{
						text4 = sqlDataReader[getInt18 - 1].ToString().Trim();
					}
					if (text4.Length > 50)
					{
						text4 = text4.Substring(0, 50);
					}
					string text5 = "";
					if (getInt22 != 0)
					{
						text5 = sqlDataReader[getInt22 - 1].ToString().Trim();
					}
					if (text5.Length > 30)
					{
						text5 = text5.Substring(0, 30);
					}
					string text6 = "";
					if (getInt30 != 0)
					{
						text6 = sqlDataReader[getInt30 - 1].ToString().Trim();
					}
					if (text6.Length > 80)
					{
						text6 = text6.Substring(0, 80);
					}
					string text7 = "";
					if (getBoolean28)
					{
						text7 = getString51;
					}
					if (getBoolean8)
					{
						text7 += getString31;
					}
					else
					{
						num2 = getInt16;
						string text8 = "";
						if (num2 != 0)
						{
							text8 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text7 += text8;
						if (text8 == "")
						{
							int num4 = getInt53;
							string text9 = "";
							if (num4 != 0)
							{
								text9 = sqlDataReader[num4 - 1].ToString().Trim();
							}
							text7 += text9;
						}
					}
					string[] array2 = getString32.Split(',');
					bool flag3 = false;
					array = array2;
					foreach (string text10 in array)
					{
						if (flag3)
						{
							break;
						}
						switch (text10)
						{
						case "1":
							if (text7 != "" && CariData.GetCariByCariKod(Db.Connection, text7, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "2":
							if (text2 != "" && CariData.GetCariByVergiTcKimlikNo(Db.Connection, text2, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "3":
							if (text6 != "" && CariData.GetCariByEMail(Db.Connection, text6, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "4":
							if (text5 != "" && CariData.GetCariByBankaHesapNo(Db.Connection, text5, AdreslerTemsilciyeGore: false, "").cari_kod != "")
							{
								flag3 = true;
							}
							break;
						case "5":
							if (text3 != "" && CariData.GetCariByCariUnvan(Db.Connection, text3, AdreslerTemsilciyeGore: false, "").cari_kod != "")
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
							if (text7 == item.cari_kod)
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
						cari.cari_kod = text7;
						cari.cari_vdaire_no = text2;
						cari.cari_unvan1 = text3;
						cari.cari_unvan2 = text4;
						cari.cari_banka_hesapno1 = text5;
						cari.cari_Email = text6;
						string text11 = "";
						if (getInt21 != 0)
						{
							text11 = sqlDataReader[getInt21 - 1].ToString().Trim();
						}
						if (text11.Length > 20)
						{
							text11 = text11.Substring(0, 20);
						}
						cari.cari_vdaire_adi = text11;
						string text12 = "";
						if (getBoolean53)
						{
							text12 = getString66;
						}
						else
						{
							if (getInt84 != 0)
							{
								text12 = sqlDataReader[getInt84 - 1].ToString().Trim();
							}
							if (text12.Length > 10)
							{
								text12 = text12.Substring(0, 10);
							}
						}
						cari.cari_muhartikeli = text12;
						string text13 = "";
						if (getBoolean57)
						{
							text13 = getString83;
						}
						else
						{
							if (getInt88 != 0)
							{
								text13 = sqlDataReader[getInt88 - 1].ToString().Trim();
							}
							if (text13.Length > 25)
							{
								text13 = text13.Substring(0, 25);
							}
						}
						cari.cari_Ana_cari_kodu = text13;
						string text14 = "";
						if (getBoolean58)
						{
							text14 = getString84;
						}
						else
						{
							if (getInt89 != 0)
							{
								text14 = sqlDataReader[getInt89 - 1].ToString().Trim();
							}
							if (text14.Length > 25)
							{
								text14 = text14.Substring(0, 25);
							}
						}
						cari.cari_temsilci_kodu = text14;
						string text15 = "";
						if (getBoolean59)
						{
							text15 = getString85;
						}
						else
						{
							if (getInt90 != 0)
							{
								text15 = sqlDataReader[getInt90 - 1].ToString().Trim();
							}
							if (text15.Length > 25)
							{
								text15 = text15.Substring(0, 25);
							}
						}
						cari.cari_grup_kodu = text15;
						string text16 = "";
						if (getBoolean60)
						{
							text16 = getString86;
						}
						else
						{
							if (getInt91 != 0)
							{
								text16 = sqlDataReader[getInt91 - 1].ToString().Trim();
							}
							if (text16.Length > 25)
							{
								text16 = text16.Substring(0, 25);
							}
						}
						cari.cari_sektor_kodu = text16;
						string text17 = "";
						if (getBoolean61)
						{
							text17 = getString87;
						}
						else
						{
							if (getInt92 != 0)
							{
								text17 = sqlDataReader[getInt92 - 1].ToString().Trim();
							}
							if (text17.Length > 25)
							{
								text17 = text17.Substring(0, 25);
							}
						}
						cari.cari_bolge_kodu = text17;
						string text18 = "";
						if (getBoolean62)
						{
							text18 = getString88;
						}
						else
						{
							if (getInt93 != 0)
							{
								text18 = sqlDataReader[getInt93 - 1].ToString().Trim();
							}
							if (text18.Length > 30)
							{
								text18 = text18.Substring(0, 30);
							}
						}
						cari.cari_wwwadresi = text18;
						string text19 = "";
						if (getBoolean63)
						{
							text19 = getString89;
						}
						else
						{
							if (getInt94 != 0)
							{
								text19 = sqlDataReader[getInt94 - 1].ToString().Trim();
							}
							if (text19.Length > 20)
							{
								text19 = text19.Substring(0, 20);
							}
						}
						cari.cari_CepTel = text19;
						string text20 = "";
						if (getBoolean64)
						{
							text20 = getString90;
						}
						else
						{
							if (getInt95 != 0)
							{
								text20 = sqlDataReader[getInt95 - 1].ToString().Trim();
							}
							if (text20.Length > 4)
							{
								text20 = text20.Substring(0, 4);
							}
						}
						cari.cari_satis_isk_kod = text20;
						string text21 = "";
						if (getBoolean65)
						{
							text21 = getString91;
						}
						else
						{
							if (getInt96 != 0)
							{
								text21 = sqlDataReader[getInt96 - 1].ToString().Trim();
							}
							if (text21.Length > 15)
							{
								text21 = text21.Substring(0, 15);
							}
						}
						cari.cari_sicil_no = text21;
						string text22 = "";
						if (getBoolean66)
						{
							text22 = getString92;
						}
						else
						{
							if (getInt97 != 0)
							{
								text22 = sqlDataReader[getInt97 - 1].ToString().Trim();
							}
							if (text22.Length > 25)
							{
								text22 = text22.Substring(0, 25);
							}
						}
						int result = 0;
						int.TryParse(text22, out result);
						cari.cari_VarsayilanGirisDepo = result;
						string text23 = "";
						if (getBoolean67)
						{
							text23 = getString93;
						}
						else
						{
							if (getInt98 != 0)
							{
								text23 = sqlDataReader[getInt98 - 1].ToString().Trim();
							}
							if (text23.Length > 25)
							{
								text23 = text23.Substring(0, 25);
							}
						}
						int result2 = 0;
						int.TryParse(text23, out result2);
						cari.cari_VarsayilanCikisDepo = result2;
						string text24 = "";
						if (getBoolean68)
						{
							text24 = getString94;
						}
						else
						{
							if (getInt99 != 0)
							{
								text24 = sqlDataReader[getInt99 - 1].ToString().Trim();
							}
							if (text24.Length > 127)
							{
								text24 = text24.Substring(0, 127);
							}
						}
						cari.cari_Portal_PW = text24;
						string text25 = "";
						if (getBoolean54)
						{
							text25 = getString63;
						}
						else
						{
							if (getInt85 != 0)
							{
								text25 = sqlDataReader[getInt85 - 1].ToString().Trim();
							}
							if (text25.Length > 40)
							{
								text25 = text25.Substring(0, 40);
							}
						}
						cari.cari_muh_kod = text25;
						string text26 = "";
						if (getBoolean55)
						{
							text26 = getString64;
						}
						else
						{
							if (getInt86 != 0)
							{
								text26 = sqlDataReader[getInt86 - 1].ToString().Trim();
							}
							if (text26.Length > 40)
							{
								text26 = text26.Substring(0, 40);
							}
						}
						cari.cari_muh_kod1 = text26;
						string text27 = "";
						if (getBoolean56)
						{
							text27 = getString65;
						}
						else
						{
							if (getInt87 != 0)
							{
								text27 = sqlDataReader[getInt87 - 1].ToString().Trim();
							}
							if (text27.Length > 40)
							{
								text27 = text27.Substring(0, 40);
							}
						}
						cari.cari_muh_kod2 = text27;
						cari.cari_doviz_cinsi = getInt50;
						cari.cari_doviz_cinsi1 = getInt51;
						cari.cari_doviz_cinsi2 = getInt52;
						cari.cari_Portal_Enabled = getBoolean69;
						string text28 = "";
						string text29 = "";
						string text30 = "";
						if (getBoolean24)
						{
							text28 = getString47;
						}
						else if (getInt45 != 0)
						{
							text28 = sqlDataReader[getInt45 - 1].ToString().Trim();
						}
						if (getBoolean25)
						{
							text29 = getString48;
						}
						else if (getInt46 != 0)
						{
							text29 = sqlDataReader[getInt46 - 1].ToString().Trim();
						}
						if (getBoolean26)
						{
							text30 = getString49;
						}
						else if (getInt47 != 0)
						{
							text30 = sqlDataReader[getInt47 - 1].ToString().Trim();
						}
						cari.cari_special1 = text28;
						cari.cari_special2 = text29;
						cari.cari_special3 = text30;
						string text31 = "";
						if (getInt23 != 0)
						{
							text31 = sqlDataReader[getInt23 - 1].ToString().Trim();
						}
						if (text31.Length > 50)
						{
							text31 = text31.Substring(0, 50);
						}
						if (text31 != "")
						{
							CariAdres cariAdres = new CariAdres();
							cariAdres.adr_cadde = text31;
							string text32 = "";
							if (getInt24 != 0)
							{
								text32 = sqlDataReader[getInt24 - 1].ToString().Trim();
							}
							if (text32.Length > 50)
							{
								text32 = text32.Substring(0, 50);
							}
							cariAdres.adr_sokak = text32;
							string text33 = "";
							if (getInt25 != 0)
							{
								text33 = sqlDataReader[getInt25 - 1].ToString().Trim();
							}
							if (text33.Length > 15)
							{
								text33 = text33.Substring(0, 15);
							}
							cariAdres.adr_ilce = text33;
							string text34 = "";
							if (getInt26 != 0)
							{
								text34 = sqlDataReader[getInt26 - 1].ToString().Trim();
							}
							if (text34.Length > 15)
							{
								text34 = text34.Substring(0, 15);
							}
							if (getBoolean40)
							{
								switch (int.Parse(text34))
								{
								case 1:
									text34 = "ADANA";
									break;
								case 2:
									text34 = "ADIYAMAN";
									break;
								case 3:
									text34 = "AFYONKARAHİSAR";
									break;
								case 4:
									text34 = "AĞRI";
									break;
								case 5:
									text34 = "AMASYA";
									break;
								case 6:
									text34 = "ANKARA";
									break;
								case 7:
									text34 = "ANTALYA";
									break;
								case 8:
									text34 = "ARTVİN";
									break;
								case 9:
									text34 = "AYDIN";
									break;
								case 10:
									text34 = "BALIKESİR";
									break;
								case 11:
									text34 = "BİLECİK";
									break;
								case 12:
									text34 = "BİNGÖL";
									break;
								case 13:
									text34 = "BİTLİS";
									break;
								case 14:
									text34 = "BOLU";
									break;
								case 15:
									text34 = "BURDUR";
									break;
								case 16:
									text34 = "BURSA";
									break;
								case 17:
									text34 = "ÇANAKKALE";
									break;
								case 18:
									text34 = "ÇANKIRI";
									break;
								case 19:
									text34 = "ÇORUM";
									break;
								case 20:
									text34 = "DENİZLİ";
									break;
								case 21:
									text34 = "DİYARBAKIR";
									break;
								case 22:
									text34 = "EDİRNE";
									break;
								case 23:
									text34 = "ELAZIĞ";
									break;
								case 24:
									text34 = "ERZİNCAN";
									break;
								case 25:
									text34 = "ERZURUM";
									break;
								case 26:
									text34 = "ESKİŞEHİR";
									break;
								case 27:
									text34 = "GAZİANTEP";
									break;
								case 28:
									text34 = "GİRESUN";
									break;
								case 29:
									text34 = "GÜMÜŞHANE";
									break;
								case 30:
									text34 = "HAKKARİ";
									break;
								case 31:
									text34 = "HATAY";
									break;
								case 32:
									text34 = "ISPARTA";
									break;
								case 33:
									text34 = "MERSİN";
									break;
								case 34:
									text34 = "İSTANBUL";
									break;
								case 35:
									text34 = "İZMİR";
									break;
								case 36:
									text34 = "KARS";
									break;
								case 37:
									text34 = "KASTAMONU";
									break;
								case 38:
									text34 = "KAYSERİ";
									break;
								case 39:
									text34 = "KIRKLARELİ";
									break;
								case 40:
									text34 = "KIRŞEHİR";
									break;
								case 41:
									text34 = "KOCAELİ";
									break;
								case 42:
									text34 = "KONYA";
									break;
								case 43:
									text34 = "KÜTAHYA";
									break;
								case 44:
									text34 = "MALATYA";
									break;
								case 45:
									text34 = "MANİSA";
									break;
								case 46:
									text34 = "KAHRAMANMARAŞ";
									break;
								case 47:
									text34 = "MARDİN";
									break;
								case 48:
									text34 = "MUĞLA";
									break;
								case 49:
									text34 = "MUŞ";
									break;
								case 50:
									text34 = "NEVŞEHİR";
									break;
								case 51:
									text34 = "NİĞDE";
									break;
								case 52:
									text34 = "ORDU";
									break;
								case 53:
									text34 = "RİZE";
									break;
								case 54:
									text34 = "SAKARYA";
									break;
								case 55:
									text34 = "SAMSUN";
									break;
								case 56:
									text34 = "SİİRT";
									break;
								case 57:
									text34 = "SİNOP";
									break;
								case 58:
									text34 = "SİVAS";
									break;
								case 59:
									text34 = "TEKİRDAĞ";
									break;
								case 60:
									text34 = "TOKAT";
									break;
								case 61:
									text34 = "TRABZON";
									break;
								case 62:
									text34 = "TUNCELİ";
									break;
								case 63:
									text34 = "ŞANLIURFA";
									break;
								case 64:
									text34 = "UŞAK";
									break;
								case 65:
									text34 = "VAN";
									break;
								case 66:
									text34 = "YOZGAT";
									break;
								case 67:
									text34 = "ZONGULDAK";
									break;
								case 68:
									text34 = "AKSARAY";
									break;
								case 69:
									text34 = "BAYBURT";
									break;
								case 70:
									text34 = "KARAMAN";
									break;
								case 71:
									text34 = "KIRIKKALE";
									break;
								case 72:
									text34 = "BATMAN";
									break;
								case 73:
									text34 = "ŞIRNAK";
									break;
								case 74:
									text34 = "BARTIN";
									break;
								case 75:
									text34 = "ARDAHAN";
									break;
								case 76:
									text34 = "IĞDIR";
									break;
								case 77:
									text34 = "YALOVA";
									break;
								case 78:
									text34 = "KARABÜK";
									break;
								case 79:
									text34 = "KİLİS";
									break;
								case 80:
									text34 = "OSMANİYE";
									break;
								case 81:
									text34 = "DÜZCE";
									break;
								}
							}
							cariAdres.adr_il = text34;
							string text35 = "";
							if (getInt27 != 0)
							{
								text35 = sqlDataReader[getInt27 - 1].ToString().Trim();
							}
							if (text35.Length > 15)
							{
								text35 = text35.Substring(0, 15);
							}
							cariAdres.adr_ulke = text35;
							string text36 = "";
							if (getInt28 != 0)
							{
								text36 = sqlDataReader[getInt28 - 1].ToString().Trim();
							}
							if (text36.Length > 8)
							{
								text36 = text36.Substring(0, 8);
							}
							cariAdres.adr_posta_kodu = text36;
							string text37 = "";
							if (getInt29 != 0)
							{
								text37 = sqlDataReader[getInt29 - 1].ToString().Trim();
							}
							if (text37.Length > 10)
							{
								text37 = text37.Substring(0, 10);
							}
							cariAdres.adr_tel_no1 = text37;
							cariAdres.adr_special1 = text28;
							cariAdres.adr_special2 = text29;
							cariAdres.adr_special3 = text30;
							cariAdres.adr_cari_kod = cari.cari_kod;
							cariAdres.adr_adres_no = 1;
							cari.CariAdresleri.Add(cariAdres);
						}
						list.Add(cari);
					}
				}
				if (flag2)
				{
					string text38 = "";
					if (getBoolean9)
					{
						text38 = getString33;
					}
					else
					{
						num2 = getInt31;
						string text39 = "";
						if (num2 != 0)
						{
							text39 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text38 = text39;
					}
					bool flag4 = false;
					Proje proje = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text38);
					if (!(proje.pro_kodu == "") && proje.pro_kodu != null)
					{
						flag4 = true;
					}
					if (text38 == "")
					{
						flag4 = true;
					}
					if (!flag4)
					{
						bool flag5 = false;
						foreach (Proje item2 in list2)
						{
							if (text38 == item2.pro_kodu)
							{
								flag5 = true;
								break;
							}
						}
						if (!flag5)
						{
							Proje proje2 = new Proje();
							proje2.pro_kodu = text38;
							string text40 = "";
							if (getBoolean42)
							{
								text40 = getString72;
							}
							else
							{
								if (getInt73 != 0)
								{
									text40 = sqlDataReader[getInt73 - 1].ToString().Trim();
								}
								if (text40.Length > 40)
								{
									text40 = text40.Substring(0, 40);
								}
							}
							proje2.pro_adi = text40;
							string text41 = "";
							if (getBoolean43)
							{
								text41 = getString73;
							}
							else
							{
								if (getInt74 != 0)
								{
									text41 = sqlDataReader[getInt74 - 1].ToString().Trim();
								}
								if (text41.Length > 25)
								{
									text41 = text41.Substring(0, 25);
								}
							}
							proje2.pro_musterikodu = text41;
							string text42 = "";
							if (getBoolean44)
							{
								text42 = getString74;
							}
							else
							{
								if (getInt75 != 0)
								{
									text42 = sqlDataReader[getInt75 - 1].ToString().Trim();
								}
								if (text42.Length > 25)
								{
									text42 = text42.Substring(0, 25);
								}
							}
							proje2.pro_sormerkodu = text42;
							string text43 = "";
							if (getBoolean45)
							{
								text43 = getString75;
							}
							else
							{
								if (getInt76 != 0)
								{
									text43 = sqlDataReader[getInt76 - 1].ToString().Trim();
								}
								if (text43.Length > 25)
								{
									text43 = text43.Substring(0, 25);
								}
							}
							proje2.pro_grupkodu = text43;
							string text44 = "";
							if (getBoolean46)
							{
								text44 = getString76;
							}
							else
							{
								if (getInt77 != 0)
								{
									text44 = sqlDataReader[getInt77 - 1].ToString().Trim();
								}
								if (text44.Length > 25)
								{
									text44 = text44.Substring(0, 25);
								}
							}
							proje2.pro_sektorkodu = text44;
							string text45 = "";
							if (getBoolean47)
							{
								text45 = getString77;
							}
							else
							{
								if (getInt78 != 0)
								{
									text45 = sqlDataReader[getInt78 - 1].ToString().Trim();
								}
								if (text45.Length > 25)
								{
									text45 = text45.Substring(0, 25);
								}
							}
							proje2.pro_bolgekodu = text45;
							string text46 = "";
							if (getBoolean48)
							{
								text46 = getString78;
							}
							else
							{
								if (getInt79 != 0)
								{
									text46 = sqlDataReader[getInt79 - 1].ToString().Trim();
								}
								if (text46.Length > 25)
								{
									text46 = text46.Substring(0, 25);
								}
							}
							proje2.pro_ana_projekodu = text46;
							string text47 = "";
							if (getBoolean49)
							{
								text47 = getString79;
							}
							else
							{
								if (getInt80 != 0)
								{
									text47 = sqlDataReader[getInt80 - 1].ToString().Trim();
								}
								if (text47.Length > 50)
								{
									text47 = text47.Substring(0, 50);
								}
							}
							proje2.pro_aciklama = text47;
							string text48 = "";
							if (getBoolean50)
							{
								text48 = getString80;
							}
							else
							{
								if (getInt81 != 0)
								{
									text48 = sqlDataReader[getInt81 - 1].ToString().Trim();
								}
								if (text48.Length > 10)
								{
									text48 = text48.Substring(0, 10);
								}
							}
							proje2.pro_muh_kod_artikeli = text48;
							list2.Add(proje2);
						}
					}
					string text49 = "";
					if (getBoolean10)
					{
						text49 = getString34;
					}
					else
					{
						num2 = getInt32;
						string text50 = "";
						if (num2 != 0)
						{
							text50 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text49 = text50;
					}
					bool flag6 = false;
					SorumlulukMerkezi sorumlulukMerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text49);
					if (!(sorumlulukMerkezi.som_kod == "") && sorumlulukMerkezi.som_kod != null)
					{
						flag6 = true;
					}
					if (text49 == "")
					{
						flag6 = true;
					}
					if (!flag6)
					{
						bool flag7 = false;
						foreach (SorumlulukMerkezi item3 in list3)
						{
							if (text49 == item3.som_kod)
							{
								flag7 = true;
								break;
							}
						}
						if (!flag7)
						{
							SorumlulukMerkezi sorumlulukMerkezi2 = new SorumlulukMerkezi();
							sorumlulukMerkezi2.som_kod = text49;
							string text51 = "";
							if (getBoolean51)
							{
								text51 = getString81;
							}
							else
							{
								if (getInt82 != 0)
								{
									text51 = sqlDataReader[getInt82 - 1].ToString().Trim();
								}
								if (text51.Length > 40)
								{
									text51 = text51.Substring(0, 40);
								}
							}
							sorumlulukMerkezi2.som_isim = text51;
							string text52 = "";
							if (getBoolean52)
							{
								text52 = getString82;
							}
							else
							{
								if (getInt83 != 0)
								{
									text52 = sqlDataReader[getInt83 - 1].ToString().Trim();
								}
								if (text52.Length > 10)
								{
									text52 = text52.Substring(0, 10);
								}
							}
							sorumlulukMerkezi2.som_MuhArtikeli = text52;
							list3.Add(sorumlulukMerkezi2);
						}
					}
				}
				num3++;
			}
			sqlDataReader.Close();
			sqlDataReader = sqlCommand.ExecuteReader();
			if (list.Count > 0)
			{
				switch (getInt70)
				{
				case 1:
					foreach (Cari item4 in list)
					{
						item4.KaydetYeniCari(Db.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniCarileriSorVeAc(list);
					break;
				}
			}
			if (list2.Count > 0)
			{
				switch (getInt71)
				{
				case 1:
					foreach (Proje item5 in list2)
					{
						ProjeData.YeniProjeKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item5, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniProjeleriSorVeAc(list2);
					break;
				}
			}
			if (list3.Count > 0)
			{
				switch (getInt72)
				{
				case 1:
					foreach (SorumlulukMerkezi item6 in list3)
					{
						SorumlulukMerkeziData.YeniSorumlulukMerkeziKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item6, _mikrouygulamabilgileri.mikrokullanici.User_no);
					}
					RepositoryItemlarinLookupTablelariniGuncelle();
					break;
				case 2:
					YeniSorumlulukMerkezleriSorVeAc(list3);
					break;
				}
			}
			DateTime dateTime = DateTime.Now;
			string evraknoseri = "";
			int evraknosira = 0;
			string belgeno = "";
			DateTime belgetarih = DateTime.Now;
			Cari cari2 = new Cari();
			new FiyatListesi();
			int dovizcinsi = 0;
			Kur kur = new Kur();
			int alternatifdovizcinsi = 0;
			Kur alternatifdovizkuru = new Kur();
			new Depo();
			Proje proje3 = new Proje();
			SorumlulukMerkezi sorumlulukmerkezi = new SorumlulukMerkezi();
			string temsilcikodu = "";
			Firma firma = new Firma();
			Sube sube = new Sube();
			int mikrouserno = 1;
			_ = DateTime.Now;
			string text53 = "";
			string text54 = "";
			string text55 = "";
			string text56 = "";
			string text57 = "";
			string text58 = "";
			string text59 = "";
			string text60 = "";
			string text61 = "";
			string text62 = "";
			string degistirspecialalan = "";
			string degistirspecialalan2 = "";
			string degistirspecialalan3 = "";
			string kriter_string = "";
			string kriter_string2 = "";
			string kriter_string3 = "";
			string kriter_string4 = "";
			string kriter_string5 = "";
			double result3 = 0.0;
			double result4 = 0.0;
			double result5 = 0.0;
			double result6 = 0.0;
			double result7 = 0.0;
			bool kriter_bool = false;
			bool kriter_bool2 = false;
			bool kriter_bool3 = false;
			bool kriter_bool4 = false;
			bool kriter_bool5 = false;
			num3 = 0;
			while (sqlDataReader.Read())
			{
				bool flag8 = true;
				if (true)
				{
					firma = FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt);
					sube = SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, getInt2);
					string text63 = "";
					if (getInt19 != 0)
					{
						text63 = sqlDataReader[getInt19 - 1].ToString().Trim();
					}
					if (text63 == "" && getInt20 != 0)
					{
						text63 = sqlDataReader[getInt20 - 1].ToString().Trim();
					}
					string text64 = "";
					if (getInt17 != 0)
					{
						text64 = sqlDataReader[getInt17 - 1].ToString().Trim();
					}
					if (text64.Length > 50)
					{
						text64 = text64.Substring(0, 50);
					}
					string text65 = "";
					if (getInt18 != 0)
					{
						text65 = sqlDataReader[getInt18 - 1].ToString().Trim();
					}
					if (text65.Length > 50)
					{
						text65 = text65.Substring(0, 50);
					}
					string text66 = "";
					if (getInt22 != 0)
					{
						text66 = sqlDataReader[getInt22 - 1].ToString().Trim();
					}
					if (text66.Length > 30)
					{
						text66 = text66.Substring(0, 30);
					}
					string text67 = "";
					if (getInt30 != 0)
					{
						text67 = sqlDataReader[getInt30 - 1].ToString().Trim();
					}
					if (text67.Length > 80)
					{
						text67 = text67.Substring(0, 80);
					}
					string text68 = "";
					if (getBoolean28)
					{
						text68 = getString51;
					}
					if (getBoolean8)
					{
						text68 += getString31;
					}
					else
					{
						num2 = getInt16;
						string text69 = "";
						if (num2 != 0)
						{
							text69 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text68 += text69;
						if (text69 == "")
						{
							int num5 = getInt53;
							string text70 = "";
							if (num5 != 0)
							{
								text70 = sqlDataReader[num5 - 1].ToString().Trim();
							}
							text68 += text70;
						}
					}
					string[] array3 = getString32.Split(',');
					bool flag9 = false;
					array = array3;
					foreach (string text71 in array)
					{
						if (flag9)
						{
							break;
						}
						switch (text71)
						{
						case "1":
							if (text68 != "")
							{
								cari2 = CariData.GetCariByCariKod(Db.Connection, text68, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag9 = true;
								}
							}
							break;
						case "2":
							if (text63 != "")
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
								cari2 = CariData.GetCariByVergiTcKimlikNo(sqlConnection2, text63, AdreslerTemsilciyeGore: false, "");
								sqlConnection2.Close();
								if (cari2.cari_kod != "")
								{
									flag9 = true;
								}
							}
							break;
						case "3":
							if (text67 != "")
							{
								cari2 = CariData.GetCariByEMail(Db.Connection, text67, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag9 = true;
								}
							}
							break;
						case "4":
							if (text66 != "")
							{
								cari2 = CariData.GetCariByBankaHesapNo(Db.Connection, text66, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag9 = true;
								}
							}
							break;
						case "5":
							if (text64 != "")
							{
								cari2 = CariData.GetCariByCariUnvan(Db.Connection, text64, AdreslerTemsilciyeGore: false, "");
								if (cari2.cari_kod != "")
								{
									flag9 = true;
								}
							}
							break;
						}
					}
					FiyatListesiData.GetFiyatListesi(Db.Connection, cari2.cari_satis_fk);
					dovizcinsi = cari2.cari_doviz_cinsi;
					_ = cari2.cari_odemeplan_no;
					if (getInt4 != 0)
					{
						dateTime = sqlDataReader.GetSafeDateTime(getInt4 - 1);
					}
					if (getBoolean27)
					{
						evraknoseri = getString50;
					}
					else
					{
						num2 = getInt48;
						string text72 = "";
						if (num2 != 0)
						{
							text72 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						evraknoseri = text72;
					}
					if (getInt49 != 0)
					{
						evraknosira = int.Parse(sqlDataReader[getInt49 - 1].ToString().Trim());
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
					if (getInt7 != 0)
					{
						kur = new Kur();
						double result8 = 1.0;
						double.TryParse(sqlDataReader[getInt7 - 1].ToString().Trim().Replace(".", ","), out result8);
						kur.dov_fiyat = result8;
					}
					string text73 = "";
					if (getBoolean9)
					{
						text73 = getString33;
					}
					else
					{
						num2 = getInt31;
						string text74 = "";
						if (num2 != 0)
						{
							text74 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text73 = text74;
					}
					if (text73 != "")
					{
						proje3 = ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text73);
					}
					string text75 = "";
					if (getBoolean10)
					{
						text75 = getString34;
					}
					else
					{
						num2 = getInt32;
						string text76 = "";
						if (num2 != 0)
						{
							text76 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text75 = text76;
					}
					if (text75 != "")
					{
						sorumlulukmerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text75);
					}
					if (getBoolean11)
					{
						temsilcikodu = getString35;
					}
					else
					{
						num2 = getInt33;
						string text77 = "";
						if (num2 != 0)
						{
							text77 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (getBoolean12)
						{
							text77 = cari2.cari_temsilci_kodu;
						}
						temsilcikodu = text77;
					}
					if (getBoolean14)
					{
						text53 = getString37;
					}
					else
					{
						num2 = getInt35;
						string text78 = "";
						if (num2 != 0)
						{
							text78 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text53 = text78;
					}
					if (getBoolean30)
					{
						text53 = getString53 + text53;
					}
					if (getBoolean15)
					{
						text54 = getString38;
					}
					else
					{
						num2 = getInt36;
						string text79 = "";
						if (num2 != 0)
						{
							text79 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text54 = text79;
					}
					if (getBoolean31)
					{
						text54 = getString54 + text54;
					}
					if (getBoolean16)
					{
						text55 = getString39;
					}
					else
					{
						num2 = getInt37;
						string text80 = "";
						if (num2 != 0)
						{
							text80 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text55 = text80;
					}
					if (getBoolean32)
					{
						text55 = getString55 + text55;
					}
					if (getBoolean17)
					{
						text56 = getString40;
					}
					else
					{
						num2 = getInt38;
						string text81 = "";
						if (num2 != 0)
						{
							text81 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text56 = text81;
					}
					if (getBoolean33)
					{
						text56 = getString56 + text56;
					}
					if (getBoolean18)
					{
						text57 = getString41;
					}
					else
					{
						num2 = getInt39;
						string text82 = "";
						if (num2 != 0)
						{
							text82 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text57 = text82;
					}
					if (getBoolean34)
					{
						text57 = getString57 + text57;
					}
					if (getBoolean19)
					{
						text58 = getString42;
					}
					else
					{
						num2 = getInt40;
						string text83 = "";
						if (num2 != 0)
						{
							text83 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text58 = text83;
					}
					if (getBoolean35)
					{
						text58 = getString58 + text58;
					}
					if (getBoolean20)
					{
						text59 = getString43;
					}
					else
					{
						num2 = getInt41;
						string text84 = "";
						if (num2 != 0)
						{
							text84 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text59 = text84;
					}
					if (getBoolean36)
					{
						text59 = getString59 + text59;
					}
					if (getBoolean21)
					{
						text60 = getString44;
					}
					else
					{
						num2 = getInt42;
						string text85 = "";
						if (num2 != 0)
						{
							text85 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text60 = text85;
					}
					if (getBoolean37)
					{
						text60 = getString60 + text60;
					}
					if (getBoolean22)
					{
						text61 = getString45;
					}
					else
					{
						num2 = getInt43;
						string text86 = "";
						if (num2 != 0)
						{
							text86 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text61 = text86;
					}
					if (getBoolean38)
					{
						text61 = getString61 + text61;
					}
					if (getBoolean23)
					{
						text62 = getString46;
					}
					else
					{
						num2 = getInt44;
						string text87 = "";
						if (num2 != 0)
						{
							text87 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text62 = text87;
					}
					if (getBoolean39)
					{
						text62 = getString62 + text62;
					}
					if (getBoolean24)
					{
						degistirspecialalan = getString47;
					}
					else
					{
						num2 = getInt45;
						string text88 = "";
						if (num2 != 0)
						{
							text88 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						degistirspecialalan = text88;
					}
					if (getBoolean25)
					{
						degistirspecialalan2 = getString48;
					}
					else
					{
						num2 = getInt46;
						string text89 = "";
						if (num2 != 0)
						{
							text89 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						degistirspecialalan2 = text89;
					}
					if (getBoolean26)
					{
						degistirspecialalan3 = getString49;
					}
					else
					{
						num2 = getInt47;
						string text90 = "";
						if (num2 != 0)
						{
							text90 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						degistirspecialalan3 = text90;
					}
				}
				num2 = getInt54;
				if (num2 != 0)
				{
					kriter_string = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt55;
				if (num2 != 0)
				{
					kriter_string2 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt56;
				if (num2 != 0)
				{
					kriter_string3 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt57;
				if (num2 != 0)
				{
					kriter_string4 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt58;
				if (num2 != 0)
				{
					kriter_string5 = sqlDataReader[num2 - 1].ToString().Trim();
				}
				num2 = getInt59;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result3);
				}
				num2 = getInt60;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result4);
				}
				num2 = getInt61;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result5);
				}
				num2 = getInt62;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result6);
				}
				num2 = getInt63;
				if (num2 != 0)
				{
					double.TryParse(sqlDataReader[num2 - 1].ToString().Trim().Replace(".", ","), out result7);
				}
				num2 = getInt64;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString67)
				{
					kriter_bool = true;
				}
				num2 = getInt65;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString68)
				{
					kriter_bool2 = true;
				}
				num2 = getInt66;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString69)
				{
					kriter_bool3 = true;
				}
				num2 = getInt67;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString70)
				{
					kriter_bool4 = true;
				}
				num2 = getInt68;
				if (num2 != 0 && sqlDataReader[num2 - 1].ToString().Trim() == getString71)
				{
					kriter_bool5 = true;
				}
				if (flag8)
				{
					TahsilatEvrakSatir tahsilatEvrakSatir = new TahsilatEvrakSatir();
					if (getBoolean41)
					{
						tahsilatEvrakSatir.KayitID = num3;
					}
					else
					{
						num2 = getInt69;
						string s = "";
						if (num2 != 0)
						{
							s = sqlDataReader[num2 - 1].ToString().Trim();
						}
						int result9 = 0;
						int.TryParse(s, out result9);
						tahsilatEvrakSatir.KayitID = result9;
					}
					tahsilatEvrakSatir.evraktarih = dateTime;
					tahsilatEvrakSatir.evraknoseri = evraknoseri;
					tahsilatEvrakSatir.evraknosira = evraknosira;
					tahsilatEvrakSatir.belgeno = belgeno;
					tahsilatEvrakSatir.belgetarih = belgetarih;
					tahsilatEvrakSatir.cari = cari2;
					tahsilatEvrakSatir.dovizcinsi = dovizcinsi;
					tahsilatEvrakSatir.kur = kur;
					tahsilatEvrakSatir.alternatifdovizcinsi = alternatifdovizcinsi;
					tahsilatEvrakSatir.alternatifdovizkuru = alternatifdovizkuru;
					tahsilatEvrakSatir.proje = proje3;
					tahsilatEvrakSatir.sorumlulukmerkezi = sorumlulukmerkezi;
					tahsilatEvrakSatir.temsilcikodu = temsilcikodu;
					tahsilatEvrakSatir.firma = firma;
					tahsilatEvrakSatir.sube = sube;
					tahsilatEvrakSatir.DBCno = getInt3;
					tahsilatEvrakSatir.mikrouserno = mikrouserno;
					tahsilatEvrakSatir.aciklama1 = text53;
					tahsilatEvrakSatir.aciklama2 = text54;
					tahsilatEvrakSatir.aciklama3 = text55;
					tahsilatEvrakSatir.aciklama4 = text56;
					tahsilatEvrakSatir.aciklama5 = text57;
					tahsilatEvrakSatir.aciklama6 = text58;
					tahsilatEvrakSatir.aciklama7 = text59;
					tahsilatEvrakSatir.aciklama8 = text60;
					tahsilatEvrakSatir.aciklama9 = text61;
					tahsilatEvrakSatir.aciklama10 = text62;
					tahsilatEvrakSatir.degistirspecialalan1 = degistirspecialalan;
					tahsilatEvrakSatir.degistirspecialalan2 = degistirspecialalan2;
					tahsilatEvrakSatir.degistirspecialalan3 = degistirspecialalan3;
					tahsilatEvrakSatir.kriter_string1 = kriter_string;
					tahsilatEvrakSatir.kriter_string2 = kriter_string2;
					tahsilatEvrakSatir.kriter_string3 = kriter_string3;
					tahsilatEvrakSatir.kriter_string4 = kriter_string4;
					tahsilatEvrakSatir.kriter_string5 = kriter_string5;
					tahsilatEvrakSatir.kriter_double1 = result3;
					tahsilatEvrakSatir.kriter_double2 = result4;
					tahsilatEvrakSatir.kriter_double3 = result5;
					tahsilatEvrakSatir.kriter_double4 = result6;
					tahsilatEvrakSatir.kriter_double5 = result7;
					tahsilatEvrakSatir.kriter_bool1 = kriter_bool;
					tahsilatEvrakSatir.kriter_bool2 = kriter_bool2;
					tahsilatEvrakSatir.kriter_bool3 = kriter_bool3;
					tahsilatEvrakSatir.kriter_bool4 = kriter_bool4;
					tahsilatEvrakSatir.kriter_bool5 = kriter_bool5;
					if (getBoolean)
					{
						tahsilatEvrakSatir.satir_cinsi = (enum_tahsilat_cinsi)getInt8;
					}
					else
					{
						num2 = getInt9;
						string text91 = "";
						if (num2 != 0)
						{
							text91 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						if (text91 == getString7)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.Nakit;
						}
						if (text91 == getString8)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.MusteriCeki;
						}
						if (text91 == getString9)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.MusteriSenedi;
						}
						if (text91 == getString10)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.MusteriKrediKarti;
						}
						if (text91 == getString11)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.GelenHavale;
						}
						if (text91 == getString12)
						{
							tahsilatEvrakSatir.satir_cinsi = enum_tahsilat_cinsi.GidenHavale;
						}
					}
					bool flag10 = false;
					string satir_kasa_banka_kodu = "";
					num2 = 0;
					string text92 = "";
					string text93 = "";
					switch (tahsilatEvrakSatir.satir_cinsi)
					{
					case enum_tahsilat_cinsi.Nakit:
						flag10 = getBoolean2;
						satir_kasa_banka_kodu = getString13;
						num2 = getInt10;
						text92 = getString14;
						text93 = getString15;
						break;
					case enum_tahsilat_cinsi.MusteriCeki:
						flag10 = getBoolean3;
						satir_kasa_banka_kodu = getString16;
						num2 = getInt11;
						text92 = getString17;
						text93 = getString18;
						break;
					case enum_tahsilat_cinsi.MusteriSenedi:
						flag10 = getBoolean4;
						satir_kasa_banka_kodu = getString19;
						num2 = getInt12;
						text92 = getString20;
						text93 = getString21;
						break;
					case enum_tahsilat_cinsi.MusteriKrediKarti:
						flag10 = getBoolean5;
						satir_kasa_banka_kodu = getString22;
						num2 = getInt13;
						text92 = getString23;
						text93 = getString24;
						break;
					case enum_tahsilat_cinsi.GelenHavale:
						flag10 = getBoolean6;
						satir_kasa_banka_kodu = getString25;
						num2 = getInt14;
						text92 = getString26;
						text93 = getString27;
						break;
					case enum_tahsilat_cinsi.GidenHavale:
						flag10 = getBoolean7;
						satir_kasa_banka_kodu = getString28;
						num2 = getInt15;
						text92 = getString29;
						text93 = getString30;
						break;
					}
					if (flag10)
					{
						tahsilatEvrakSatir.satir_kasa_banka_kodu = satir_kasa_banka_kodu;
					}
					else
					{
						string satir_kasa_banka_kodu2 = "";
						if (num2 != 0)
						{
							satir_kasa_banka_kodu2 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_kasa_banka_kodu = satir_kasa_banka_kodu2;
					}
					tahsilatEvrakSatir.satir_kasa_banka_kodu = text92 + tahsilatEvrakSatir.satir_kasa_banka_kodu + text93;
					if (getBoolean13)
					{
						tahsilatEvrakSatir.satir_aciklama = getString36;
					}
					else
					{
						num2 = getInt34;
						string satir_aciklama = "";
						if (num2 != 0)
						{
							satir_aciklama = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_aciklama = satir_aciklama;
					}
					if (getBoolean29)
					{
						tahsilatEvrakSatir.satir_aciklama = getString52 + tahsilatEvrakSatir.satir_aciklama;
					}
					if (getBoolean70)
					{
						double result10 = 0.0;
						double.TryParse(getString95.Trim().Replace(".", ","), out result10);
						tahsilatEvrakSatir.satir_tutar = result10;
					}
					else if (getInt100 != 0)
					{
						double result11 = 0.0;
						double.TryParse(sqlDataReader[getInt100 - 1].ToString().Trim().Replace(".", ","), out result11);
						tahsilatEvrakSatir.satir_tutar = result11;
					}
					num = getInt101;
					num2 = getInt102;
					if (num != 0)
					{
						string text94 = "";
						if (num2 != 0)
						{
							text94 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						double result12 = 0.0;
						double.TryParse(text94.Replace(".", ","), out result12);
						switch (num)
						{
						case 1:
							tahsilatEvrakSatir.satir_tutar += result12;
							break;
						case 2:
							tahsilatEvrakSatir.satir_tutar -= result12;
							break;
						case 3:
							tahsilatEvrakSatir.satir_tutar /= result12;
							break;
						case 4:
							tahsilatEvrakSatir.satir_tutar *= result12;
							break;
						}
					}
					num = getInt103;
					num2 = getInt104;
					if (num != 0)
					{
						string text95 = "";
						if (num2 != 0)
						{
							text95 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						double result13 = 0.0;
						double.TryParse(text95.Replace(".", ","), out result13);
						switch (num)
						{
						case 1:
							tahsilatEvrakSatir.satir_tutar += result13;
							break;
						case 2:
							tahsilatEvrakSatir.satir_tutar -= result13;
							break;
						case 3:
							tahsilatEvrakSatir.satir_tutar /= result13;
							break;
						case 4:
							tahsilatEvrakSatir.satir_tutar *= result13;
							break;
						}
					}
					if (getBoolean71)
					{
						tahsilatEvrakSatir.satir_vadesi = DateTime.Parse(getString96);
					}
					else if (getInt105 != 0)
					{
						tahsilatEvrakSatir.satir_vadesi = sqlDataReader.GetSafeDateTime(getInt105 - 1);
					}
					if (tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.GelenHavale || tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.GidenHavale || tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.MusteriKrediKarti || tahsilatEvrakSatir.satir_cinsi == enum_tahsilat_cinsi.Nakit)
					{
						tahsilatEvrakSatir.satir_vadesi = tahsilatEvrakSatir.evraktarih;
					}
					string text96 = "";
					if (getBoolean72)
					{
						text96 = getString97;
					}
					else
					{
						num2 = getInt106;
						string text97 = "";
						if (num2 != 0)
						{
							text97 = sqlDataReader[num2 - 1].ToString().Trim();
						}
						text96 = text97;
					}
					if (text96 != "")
					{
						tahsilatEvrakSatir.satir_sorumlulukmerkezi = SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text96);
					}
					if (getBoolean73)
					{
						tahsilatEvrakSatir.satir_referans = getString98;
					}
					else
					{
						num2 = getInt107;
						string satir_referans = "";
						if (num2 != 0)
						{
							satir_referans = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_referans = satir_referans;
					}
					if (getBoolean74)
					{
						tahsilatEvrakSatir.satir_sck_banka_adres1 = getString99;
					}
					else
					{
						num2 = getInt108;
						string satir_sck_banka_adres = "";
						if (num2 != 0)
						{
							satir_sck_banka_adres = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_sck_banka_adres1 = satir_sck_banka_adres;
					}
					if (getBoolean75)
					{
						tahsilatEvrakSatir.satir_sck_bankano = getString100;
					}
					else
					{
						num2 = getInt109;
						string satir_sck_bankano = "";
						if (num2 != 0)
						{
							satir_sck_bankano = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_sck_bankano = satir_sck_bankano;
					}
					if (getBoolean76)
					{
						tahsilatEvrakSatir.satir_sck_borclu = getString101;
					}
					else
					{
						num2 = getInt110;
						string satir_sck_borclu = "";
						if (num2 != 0)
						{
							satir_sck_borclu = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_sck_borclu = satir_sck_borclu;
					}
					if (getBoolean77)
					{
						tahsilatEvrakSatir.satir_sck_hesapno_sehir = getString102;
					}
					else
					{
						num2 = getInt111;
						string satir_sck_hesapno_sehir = "";
						if (num2 != 0)
						{
							satir_sck_hesapno_sehir = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_sck_hesapno_sehir = satir_sck_hesapno_sehir;
					}
					if (getBoolean78)
					{
						tahsilatEvrakSatir.satir_sck_no = getString103;
					}
					else
					{
						num2 = getInt112;
						string satir_sck_no = "";
						if (num2 != 0)
						{
							satir_sck_no = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_sck_no = satir_sck_no;
					}
					if (getBoolean79)
					{
						tahsilatEvrakSatir.satir_sck_sube_adres2 = getString104;
					}
					else
					{
						num2 = getInt113;
						string satir_sck_sube_adres = "";
						if (num2 != 0)
						{
							satir_sck_sube_adres = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_sck_sube_adres2 = satir_sck_sube_adres;
					}
					if (getBoolean80)
					{
						tahsilatEvrakSatir.satir_Sck_TCMB_Banka_kodu = getString105;
					}
					else
					{
						num2 = getInt114;
						string satir_Sck_TCMB_Banka_kodu = "";
						if (num2 != 0)
						{
							satir_Sck_TCMB_Banka_kodu = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_Sck_TCMB_Banka_kodu = satir_Sck_TCMB_Banka_kodu;
					}
					if (getBoolean81)
					{
						tahsilatEvrakSatir.satir_Sck_TCMB_il_kodu = getString106;
					}
					else
					{
						num2 = getInt115;
						string satir_Sck_TCMB_il_kodu = "";
						if (num2 != 0)
						{
							satir_Sck_TCMB_il_kodu = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_Sck_TCMB_il_kodu = satir_Sck_TCMB_il_kodu;
					}
					if (getBoolean82)
					{
						tahsilatEvrakSatir.satir_Sck_TCMB_Sube_kodu = getString107;
					}
					else
					{
						num2 = getInt116;
						string satir_Sck_TCMB_Sube_kodu = "";
						if (num2 != 0)
						{
							satir_Sck_TCMB_Sube_kodu = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_Sck_TCMB_Sube_kodu = satir_Sck_TCMB_Sube_kodu;
					}
					if (getBoolean83)
					{
						tahsilatEvrakSatir.satir_sck_vdaire_no = getString108;
					}
					else
					{
						num2 = getInt117;
						string satir_sck_vdaire_no = "";
						if (num2 != 0)
						{
							satir_sck_vdaire_no = sqlDataReader[num2 - 1].ToString().Trim();
						}
						tahsilatEvrakSatir.satir_sck_vdaire_no = satir_sck_vdaire_no;
					}
					bindingList.Add(tahsilatEvrakSatir);
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
		foreach (TahsilatEvrakSatir item7 in bindingList)
		{
			if (item7 != null)
			{
				_satirlar._satirlar.Add(item7);
			}
		}
		if (flag)
		{
			OtomatikEvrakSiraNoVer(satirBirlestir);
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

	private void otomatikEvrakSiraNoVerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OtomatikEvrakSiraNoVer(SatirBirlestir: true);
	}

	private void otomatikEvrakSiraNoVerSatirlarAyriEvrakToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OtomatikEvrakSiraNoVer(SatirBirlestir: false);
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
		this.gc_evrak_EvrakSeri = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_EvrakSira = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Tarih = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemDateEdit_EvrakTarih = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
		this.gc_evrak_BelgeNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_BelgeTarihi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_CariKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_CariAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_DovizCinsi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.gc_evrak_Kur = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemTextEdit_Evrak_Kur = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_evrak_ProjeKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_ProjeAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SorMerKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SorMerAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Plasiyer = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
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
		this.gc_satir_HesapKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_HesapAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_FaturaAciklama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemTextEdit_FaturaAciklama = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_satir_tutar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_vadesi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
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
		this.repositoryItemGridLookUpEdit_Evrak_Tipi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_EvrakTipi = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemGridLookUpEdit_NormalIade = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_NormalIade = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemGridLookUpEdit_AcikKapali = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.repositoryItemGridLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemGridLookUpEdit_TicaretTuru = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_TicaretTuru = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemLookUpEdit_Satir_OtvSekli = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
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
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Evrak_Kur).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_OtvSekli).BeginInit();
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
		this.gridControl_evraklar.Size = new System.Drawing.Size(1197, 432);
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
		this.advBandedGridView_master.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[5] { this.gridBand15, this.gridBand9, this.gridBand10, this.gridBand2, this.gridBand1 });
		this.advBandedGridView_master.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[53]
		{
			this.gc_evrak_KayitID, this.gc_evrak_Aktar, this.gc_evrak_AktarimDurumu, this.gc_firma_no, this.gc_sube_no, this.gc_evrak_EvrakSeri, this.gc_evrak_EvrakSira, this.gc_evrak_Tarih, this.gc_evrak_BelgeNo, this.gc_evrak_BelgeTarihi,
			this.gc_evrak_CariKodu, this.gc_evrak_CariAdi, this.gc_evrak_DovizCinsi, this.gc_evrak_Kur, this.gc_evrak_Plasiyer, this.gc_evrak_ProjeKodu, this.gc_evrak_ProjeAdi, this.gc_evrak_SorMerKodu, this.gc_evrak_SorMerAdi, this.gc_evrak_special1,
			this.gc_evrak_special2, this.gc_evrak_special3, this.gc_evrak_FaturaAciklama, this.gc_evrak_Aciklama1, this.gc_evrak_Aciklama2, this.gc_evrak_Aciklama3, this.gc_evrak_Aciklama4, this.gc_evrak_Aciklama5, this.gc_evrak_Aciklama6, this.gc_evrak_Aciklama7,
			this.gc_evrak_Aciklama8, this.gc_evrak_Aciklama9, this.gc_evrak_Aciklama10, this.gc_satir_cinsi, this.gc_satir_HesapKodu, this.gc_satir_HesapAdi, this.gc_satir_kriter_string1, this.gc_satir_kriter_string2, this.gc_satir_kriter_string3, this.gc_satir_kriter_string4,
			this.gc_satir_kriter_string5, this.gc_satir_kriter_double1, this.gc_satir_kriter_double2, this.gc_satir_kriter_double3, this.gc_satir_kriter_double4, this.gc_satir_kriter_double5, this.gc_satir_kriter_bool1, this.gc_satir_kriter_bool2, this.gc_satir_kriter_bool3, this.gc_satir_kriter_bool4,
			this.gc_satir_kriter_bool5, this.gc_satir_tutar, this.gc_satir_vadesi
		});
		this.advBandedGridView_master.DetailVerticalIndent = 20;
		this.advBandedGridView_master.GridControl = this.gridControl_evraklar;
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
		this.gridBand10.Columns.Add(this.gc_evrak_EvrakSeri);
		this.gridBand10.Columns.Add(this.gc_evrak_EvrakSira);
		this.gridBand10.Columns.Add(this.gc_evrak_Tarih);
		this.gridBand10.Columns.Add(this.gc_evrak_BelgeNo);
		this.gridBand10.Columns.Add(this.gc_evrak_BelgeTarihi);
		this.gridBand10.Columns.Add(this.gc_evrak_CariKodu);
		this.gridBand10.Columns.Add(this.gc_evrak_CariAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_DovizCinsi);
		this.gridBand10.Columns.Add(this.gc_evrak_Kur);
		this.gridBand10.Columns.Add(this.gc_evrak_ProjeKodu);
		this.gridBand10.Columns.Add(this.gc_evrak_ProjeAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_SorMerKodu);
		this.gridBand10.Columns.Add(this.gc_evrak_SorMerAdi);
		this.gridBand10.Columns.Add(this.gc_evrak_Plasiyer);
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
		this.gridBand10.Width = 2388;
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
		this.gc_evrak_SorMerKodu.Width = 79;
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
		this.gc_evrak_special3.Width = 78;
		this.gridBand2.Caption = "Satır Detayları";
		this.gridBand2.Columns.Add(this.gc_satir_cinsi);
		this.gridBand2.Columns.Add(this.gc_satir_HesapKodu);
		this.gridBand2.Columns.Add(this.gc_satir_HesapAdi);
		this.gridBand2.Columns.Add(this.gc_evrak_FaturaAciklama);
		this.gridBand2.Columns.Add(this.gc_satir_tutar);
		this.gridBand2.Columns.Add(this.gc_satir_vadesi);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.VisibleIndex = 3;
		this.gridBand2.Width = 685;
		this.gc_satir_cinsi.Caption = "Cinsi";
		this.gc_satir_cinsi.ColumnEdit = this.repositoryItemGridLookUpEdit_Satir_Cinsi;
		this.gc_satir_cinsi.FieldName = "gc_satir_cinsi";
		this.gc_satir_cinsi.Name = "gc_satir_cinsi";
		this.gc_satir_cinsi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_cinsi.Visible = true;
		this.gc_satir_cinsi.Width = 66;
		this.gc_satir_HesapKodu.Caption = "Hesap kodu";
		this.gc_satir_HesapKodu.FieldName = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.Name = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapKodu.Visible = true;
		this.gc_satir_HesapKodu.Width = 97;
		this.gc_satir_HesapAdi.Caption = "Hesap adı";
		this.gc_satir_HesapAdi.FieldName = "gc_satir_HesapKodu";
		this.gc_satir_HesapAdi.Name = "gc_satir_HesapAdi";
		this.gc_satir_HesapAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapAdi.Visible = true;
		this.gc_satir_HesapAdi.Width = 142;
		this.gc_evrak_FaturaAciklama.Caption = "Satır Açıklama";
		this.gc_evrak_FaturaAciklama.ColumnEdit = this.repositoryItemTextEdit_FaturaAciklama;
		this.gc_evrak_FaturaAciklama.FieldName = "satir_aciklama";
		this.gc_evrak_FaturaAciklama.Name = "gc_evrak_FaturaAciklama";
		this.gc_evrak_FaturaAciklama.Visible = true;
		this.gc_evrak_FaturaAciklama.Width = 196;
		this.repositoryItemTextEdit_FaturaAciklama.AutoHeight = false;
		this.repositoryItemTextEdit_FaturaAciklama.MaxLength = 40;
		this.repositoryItemTextEdit_FaturaAciklama.Name = "repositoryItemTextEdit_FaturaAciklama";
		this.gc_satir_tutar.Caption = "Tutar";
		this.gc_satir_tutar.FieldName = "satir_tutar";
		this.gc_satir_tutar.Name = "gc_satir_tutar";
		this.gc_satir_tutar.Visible = true;
		this.gc_satir_tutar.Width = 113;
		this.gc_satir_vadesi.Caption = "Vadesi";
		this.gc_satir_vadesi.FieldName = "satir_vadesi";
		this.gc_satir_vadesi.Name = "gc_satir_vadesi";
		this.gc_satir_vadesi.Visible = true;
		this.gc_satir_vadesi.Width = 71;
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
		this.repositoryItemLookUpEdit_Satir_OtvSekli.AutoHeight = false;
		this.repositoryItemLookUpEdit_Satir_OtvSekli.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Satir_OtvSekli.Name = "repositoryItemLookUpEdit_Satir_OtvSekli";
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
		this.sb_Secili_Evraklari_aktar.Location = new System.Drawing.Point(1027, 494);
		this.sb_Secili_Evraklari_aktar.Name = "sb_Secili_Evraklari_aktar";
		this.sb_Secili_Evraklari_aktar.Size = new System.Drawing.Size(164, 23);
		this.sb_Secili_Evraklari_aktar.TabIndex = 6;
		this.sb_Secili_Evraklari_aktar.Text = "SEÇİLİ EVRAKLARI AKTAR";
		this.sb_Secili_Evraklari_aktar.Click += new System.EventHandler(sb_Secili_Evraklari_aktar_Click);
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.dosyaToolStripMenuItem, this.gorunumToolStripMenuItem, this.islemlerToolStripMenuItem, this.parametrelerToolStripMenuItem, this.kriterToolStripMenuItem, this.aktarımParametreleriToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(1197, 24);
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
		this.progressBar1.Location = new System.Drawing.Point(12, 494);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(594, 23);
		this.progressBar1.TabIndex = 3;
		this.label_islem_adi.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label_islem_adi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_adi.Location = new System.Drawing.Point(693, 494);
		this.label_islem_adi.Name = "label_islem_adi";
		this.label_islem_adi.Size = new System.Drawing.Size(117, 23);
		this.label_islem_adi.TabIndex = 4;
		this.label_islem_adi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label_islem_bilgi.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label_islem_bilgi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_bilgi.ForeColor = System.Drawing.Color.Red;
		this.label_islem_bilgi.Location = new System.Drawing.Point(612, 494);
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
		this.cb_belgeno_kontrolu_yap.Location = new System.Drawing.Point(842, 494);
		this.cb_belgeno_kontrolu_yap.Name = "cb_belgeno_kontrolu_yap";
		this.cb_belgeno_kontrolu_yap.Size = new System.Drawing.Size(179, 21);
		this.cb_belgeno_kontrolu_yap.TabIndex = 10;
		this.cb_belgeno_kontrolu_yap.Text = "Belge no kontrolü yap";
		this.cb_belgeno_kontrolu_yap.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1197, 520);
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
		base.Name = "SatirBazliTahsilatEvrakGirisi";
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
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Evrak_Kur).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_OtvSekli).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cardView1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
