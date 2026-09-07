using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Stoklar;

namespace Fora.App.Mikro.TopluStokEkleme;

public class TopluStokEkleme : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private BindingList<Stok> _stoklar;

	private string _tag;

	private MemoryStream evraklar_defaultlayoutStream;

	private IContainer components;

	private SimpleButton sb_stoklari_ekle;

	private GridControl gc;

	private BindingSource bindingSource1;

	private AdvBandedGridView abgv;

	private BandedGridColumn colsto_kod;

	private BandedGridColumn colsto_isim;

	private BandedGridColumn colsto_yabanci_isim;

	private BandedGridColumn colsto_kisa_ismi;

	private BandedGridColumn colsto_perakende_vergi;

	private BandedGridColumn colsto_toptan_vergi;

	private BandedGridColumn colsto_cins;

	private BandedGridColumn colsto_doviz_cinsi;

	private BandedGridColumn colsto_sat_cari_kod;

	private BandedGridColumn colsto_detay_takip;

	private BandedGridColumn colsto_birim1_ad;

	private BandedGridColumn colsto_birim1_katsayi;

	private BandedGridColumn colsto_birim2_ad;

	private BandedGridColumn colsto_birim2_katsayi;

	private BandedGridColumn colsto_birim3_ad;

	private BandedGridColumn colsto_birim3_katsayi;

	private BandedGridColumn colsto_birim4_ad;

	private BandedGridColumn colsto_birim4_katsayi;

	private BandedGridColumn colsto_bedenli_takip;

	private BandedGridColumn colsto_renkDetayli;

	private BandedGridColumn colsto_beden_kodu;

	private BandedGridColumn colsto_renk_kodu;

	private BandedGridColumn colsto_altgrup_kod;

	private BandedGridColumn colsto_anagrup_kod;

	private BandedGridColumn colsto_sektor_kodu;

	private BandedGridColumn colsto_marka_kodu;

	private BandedGridColumn colsto_model_kodu;

	private BandedGridColumn colsto_uretici_kodu;

	private BandedGridColumn colsto_reyon_kodu;

	private BandedGridColumn colsto_standartmaliyet;

	private BandedGridColumn colsto_birim1_agirlik;

	private BandedGridColumn colsto_birim1_en;

	private BandedGridColumn colsto_birim1_boy;

	private BandedGridColumn colsto_birim1_yukseklik;

	private BandedGridColumn colsto_birim1_dara;

	private BandedGridColumn colsto_birim2_agirlik;

	private BandedGridColumn colsto_birim2_en;

	private BandedGridColumn colsto_birim2_boy;

	private BandedGridColumn colsto_birim2_yukseklik;

	private BandedGridColumn colsto_birim2_dara;

	private BandedGridColumn colsto_birim3_agirlik;

	private BandedGridColumn colsto_birim3_en;

	private BandedGridColumn colsto_birim3_boy;

	private BandedGridColumn colsto_birim3_yukseklik;

	private BandedGridColumn colsto_birim3_dara;

	private BandedGridColumn colsto_birim4_agirlik;

	private BandedGridColumn colsto_birim4_en;

	private BandedGridColumn colsto_birim4_boy;

	private BandedGridColumn colsto_birim4_yukseklik;

	private BandedGridColumn colsto_birim4_dara;

	private BandedGridColumn colsto_muh_kod;

	private BandedGridColumn colsto_muh_Iade_kod;

	private BandedGridColumn colsto_muh_sat_muh_kod;

	private BandedGridColumn colsto_muh_satIadmuhkod;

	private BandedGridColumn colsto_muh_sat_isk_kod;

	private BandedGridColumn colsto_muh_aIiskmuhkod;

	private BandedGridColumn colsto_muh_satmalmuhkod;

	private BandedGridColumn colsto_yurtdisi_satmuhk;

	private BandedGridColumn colsto_ilavemasmuhkod;

	private BandedGridColumn colsto_yatirimtesmuhkod;

	private BandedGridColumn colsto_depsatmuhkod;

	private BandedGridColumn colsto_depsatmalmuhkod;

	private BandedGridColumn colsto_bagortsatmuhkod;

	private BandedGridColumn colsto_bagortsatIadmuhkod;

	private BandedGridColumn colsto_bagortsatIskmuhkod;

	private BandedGridColumn colsto_satfiyfarkmuhkod;

	private BandedGridColumn colsto_yurtdisisatmalmuhkod;

	private BandedGridColumn colsto_bagortsatmalmuhkod;

	private BandedGridColumn colsto_karorani;

	private BandedGridColumn colsto_min_stok;

	private BandedGridColumn colsto_siparis_stok;

	private BandedGridColumn colsto_max_stok;

	private BandedGridColumn colsto_ver_sip_birim;

	private BandedGridColumn colsto_al_sip_birim;

	private BandedGridColumn colsto_siparis_sure;

	private BandedGridColumn colsto_yer_kod;

	private BandedGridColumn colsto_elk_etk_tipi;

	private BandedGridColumn colsto_raf_etiketli;

	private BandedGridColumn colsto_satis_dursun;

	private BandedGridColumn colsto_siparis_dursun;

	private BandedGridColumn colsto_malkabul_dursun;

	private BandedGridColumn colsto_iskon_yapilamaz;

	private BandedGridColumn colsto_kategori_kodu;

	private BandedGridColumn colsto_urun_sorkod;

	private BandedGridColumn colsto_muhgrup_kodu;

	private BandedGridColumn colsto_ambalaj_kodu;

	private BandedGridColumn colsto_sezon_kodu;

	private BandedGridColumn colsto_hammadde_kodu;

	private BandedGridColumn colsto_prim_kodu;

	private BandedGridColumn colsto_mkod_artik;

	private BandedGridColumn colsto_eksiyedusebilir_fl;

	private BandedGridColumn colsto_otvuygulama;

	private BandedGridColumn colsto_otvtutar;

	private BandedGridColumn colsto_otvliste;

	private BandedGridColumn colsto_prim_orani;

	private BandedGridColumn colsto_garanti_sure;

	private BandedGridColumn colsto_garanti_sure_tipi;

	private BandedGridColumn colsto_oivuygulama;

	private BandedGridColumn colsto_maxiskonto_orani;

	private BandedGridColumn colsto_oivtutar;

	private BandedGridColumn colsto_oivvergipntr;

	private BandedGridColumn colsto_webe_gonderilecek_fl;

	private BandedGridColumn colsto_special1;

	private BandedGridColumn colsto_special2;

	private BandedGridColumn colsto_special3;

	private GridBand gridBand2;

	private GridBand gridBand8;

	private GridBand gridBand7;

	private GridBand gridBand4;

	private GridBand gridBand3;

	private GridBand gridBand6;

	private GridBand gridBand5;

	private GridBand gridBand1;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem yeniToolStripMenuItem;

	private ToolStripMenuItem kapatToolStripMenuItem;

	private ToolStripMenuItem görünümToolStripMenuItem;

	private ToolStripMenuItem stoklarToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem aramaTablolariToolStripMenuItem;

	private ToolStripMenuItem kolonlaraGöreGruplamaToolStripMenuItem;

	private ToolStripMenuItem kolonSeçiciyiGösterToolStripMenuItem;

	private ToolStripMenuItem bütünGruplarıAçToolStripMenuItem;

	private ToolStripMenuItem bütünGruplarıKapatToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripMenuItem görünümüSaklaToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyayaToolStripMenuItem;

	private ToolStripMenuItem farklıDosyayaToolStripMenuItem;

	private ToolStripMenuItem görünümüYükleToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyadanToolStripMenuItem;

	private ToolStripMenuItem farklıDosyadanToolStripMenuItem;

	private ToolStripMenuItem otomatikDosyayıSilToolStripMenuItem;

	private ToolStripMenuItem varsayılanaGeriDönToolStripMenuItem;

	public TopluStokEkleme(MikroUygulamaBilgileri mikrouygulamabilgileri, BindingList<Stok> stoklar, string tag, string baslik)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_stoklar = stoklar;
		InitializeComponent();
		_tag = tag;
		Text = baslik;
	}

	private void bindData()
	{
		gc.DataSource = _stoklar;
	}

	private void TopluStokEkleme_Load(object sender, EventArgs e)
	{
		abgv.Appearance.SelectedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		evraklar_defaultlayoutStream = new MemoryStream();
		abgv.SaveLayoutToStream(evraklar_defaultlayoutStream);
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			abgv.RestoreLayoutFromXml("data\\views\\" + _tag + ".lgm");
		}
		bindData();
	}

	private void sb_stoklari_ekle_Click(object sender, EventArgs e)
	{
		foreach (Stok item in _stoklar)
		{
			StokData.YeniStokKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item, _mikrouygulamabilgileri.mikrokullanici.User_no);
		}
		base.DialogResult = DialogResult.OK;
		Close();
		Dispose();
	}

	private void TopluStokEkleme_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.DialogResult != DialogResult.OK && MessageBox.Show("Çıkmak istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
		{
			e.Cancel = true;
		}
	}

	private void yeniToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show("Kayıt edilmemiş değişiklikleri iptal edip, yeni çalışma dosyası açmak istiyor musunuz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			BindingList<Stok> stoklar = new BindingList<Stok>();
			_stoklar = stoklar;
			bindData();
		}
	}

	private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void kolonlaraGöreGruplamaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (abgv.OptionsView.ShowGroupPanel)
		{
			abgv.OptionsView.ShowGroupPanel = false;
		}
		else
		{
			abgv.OptionsView.ShowGroupPanel = true;
		}
	}

	private void kolonSeçiciyiGösterToolStripMenuItem_Click(object sender, EventArgs e)
	{
		abgv.ShowCustomization();
	}

	private void bütünGruplarıAçToolStripMenuItem_Click(object sender, EventArgs e)
	{
		abgv.ExpandAllGroups();
	}

	private void bütünGruplarıKapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		abgv.CollapseAllGroups();
	}

	private void otomatikDosyayaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		abgv.SaveLayoutToXml("data\\views\\" + _tag + ".lgm");
	}

	private void farklıDosyayaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		saveFileDialog.Filter = "Genel Görünüm |*.lgm|Tüm dosyalar|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			abgv.SaveLayoutToXml(saveFileDialog.FileName);
		}
	}

	private void otomatikDosyadanToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			abgv.RestoreLayoutFromXml("data\\views\\" + _tag + ".lgm");
		}
	}

	private void farklıDosyadanToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		openFileDialog.Filter = "Genel Görünüm |*.lgm|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			abgv.RestoreLayoutFromXml(openFileDialog.FileName);
		}
	}

	private void otomatikDosyayıSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			File.Delete("data\\views\\" + _tag + ".lgm");
		}
	}

	private void varsayılanaGeriDönToolStripMenuItem_Click(object sender, EventArgs e)
	{
		evraklar_defaultlayoutStream.Position = 0L;
		abgv.RestoreLayoutFromStream(evraklar_defaultlayoutStream);
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
		this.components = new System.ComponentModel.Container();
		this.sb_stoklari_ekle = new DevExpress.XtraEditors.SimpleButton();
		this.gc = new DevExpress.XtraGrid.GridControl();
		this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
		this.abgv = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_isim = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_yabanci_isim = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_kisa_ismi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_cins = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_doviz_cinsi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_standartmaliyet = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_karorani = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand8 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_perakende_vergi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_toptan_vergi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_otvuygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_otvtutar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_otvliste = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_oivuygulama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_oivtutar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_oivvergipntr = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand7 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_anagrup_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_altgrup_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_sat_cari_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_uretici_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_reyon_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_ambalaj_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_marka_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_muhgrup_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_sektor_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_urun_sorkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_model_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_sezon_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_hammadde_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_kategori_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_prim_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand4 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_yer_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_min_stok = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_siparis_stok = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_max_stok = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_ver_sip_birim = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_al_sip_birim = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_siparis_sure = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_special1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_special2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_special3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_birim1_ad = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim1_katsayi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim1_agirlik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim1_en = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim1_boy = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim1_yukseklik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim1_dara = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim2_ad = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim2_katsayi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim2_agirlik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim2_en = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim2_boy = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim2_yukseklik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim2_dara = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim3_ad = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim3_katsayi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim3_agirlik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim3_en = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim3_boy = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim3_yukseklik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim3_dara = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim4_ad = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim4_katsayi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim4_agirlik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim4_en = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim4_boy = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim4_yukseklik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_birim4_dara = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand6 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_muh_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_muh_Iade_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_muh_sat_muh_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_muh_satmalmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_yurtdisi_satmuhk = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_yurtdisisatmalmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_muh_satIadmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_muh_sat_isk_kod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_muh_aIiskmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_ilavemasmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_yatirimtesmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_depsatmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_depsatmalmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_bagortsatmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_bagortsatIadmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_bagortsatIskmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_bagortsatmalmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_satfiyfarkmuhkod = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_mkod_artik = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_detay_takip = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_bedenli_takip = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_beden_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_renkDetayli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_renk_kodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.colsto_raf_etiketli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_elk_etk_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_satis_dursun = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_siparis_dursun = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_malkabul_dursun = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_iskon_yapilamaz = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_eksiyedusebilir_fl = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_prim_orani = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_garanti_sure = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_garanti_sure_tipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_maxiskonto_orani = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.colsto_webe_gonderilecek_fl = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yeniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.görünümToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.stoklarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.aramaTablolariToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kolonlaraGöreGruplamaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kolonSeçiciyiGösterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.bütünGruplarıAçToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.bütünGruplarıKapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.görünümüSaklaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyayaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.farklıDosyayaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.görünümüYükleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyadanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.farklıDosyadanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.otomatikDosyayıSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.varsayılanaGeriDönToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		((System.ComponentModel.ISupportInitialize)this.gc).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.bindingSource1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.abgv).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.sb_stoklari_ekle.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.sb_stoklari_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_stoklari_ekle.Appearance.Options.UseFont = true;
		this.sb_stoklari_ekle.Location = new System.Drawing.Point(928, 425);
		this.sb_stoklari_ekle.Name = "sb_stoklari_ekle";
		this.sb_stoklari_ekle.Size = new System.Drawing.Size(128, 23);
		this.sb_stoklari_ekle.TabIndex = 0;
		this.sb_stoklari_ekle.Text = "STOKLARI EKLE";
		this.sb_stoklari_ekle.Click += new System.EventHandler(sb_stoklari_ekle_Click);
		this.gc.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.gc.Cursor = System.Windows.Forms.Cursors.Default;
		this.gc.DataSource = this.bindingSource1;
		this.gc.Location = new System.Drawing.Point(12, 27);
		this.gc.MainView = this.abgv;
		this.gc.Name = "gc";
		this.gc.Size = new System.Drawing.Size(1044, 392);
		this.gc.TabIndex = 1;
		this.gc.UseEmbeddedNavigator = true;
		this.gc.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.abgv });
		this.bindingSource1.DataSource = typeof(Fora.Mikro.Stoklar.Stok);
		this.abgv.Appearance.BandPanel.BackColor = System.Drawing.Color.Red;
		this.abgv.Appearance.BandPanel.BackColor2 = System.Drawing.Color.FromArgb(255, 128, 128);
		this.abgv.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.abgv.Appearance.BandPanel.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.abgv.Appearance.BandPanel.Options.UseBackColor = true;
		this.abgv.Appearance.BandPanel.Options.UseFont = true;
		this.abgv.Appearance.BandPanel.Options.UseForeColor = true;
		this.abgv.Appearance.BandPanel.Options.UseTextOptions = true;
		this.abgv.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.abgv.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.abgv.Appearance.BandPanelBackground.BackColor2 = System.Drawing.Color.FromArgb(255, 192, 128);
		this.abgv.Appearance.BandPanelBackground.Options.UseBackColor = true;
		this.abgv.Appearance.Empty.Font = new System.Drawing.Font("Tahoma", 11f, System.Drawing.FontStyle.Bold);
		this.abgv.Appearance.Empty.Options.UseFont = true;
		this.abgv.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(192, 192, 0);
		this.abgv.Appearance.EvenRow.Options.UseBackColor = true;
		this.abgv.Appearance.FooterPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.abgv.Appearance.FooterPanel.Options.UseFont = true;
		this.abgv.Appearance.GroupFooter.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.abgv.Appearance.GroupFooter.Options.UseFont = true;
		this.abgv.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[8] { this.gridBand2, this.gridBand8, this.gridBand7, this.gridBand4, this.gridBand3, this.gridBand6, this.gridBand5, this.gridBand1 });
		this.abgv.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[105]
		{
			this.colsto_kod, this.colsto_isim, this.colsto_perakende_vergi, this.colsto_toptan_vergi, this.colsto_kisa_ismi, this.colsto_yabanci_isim, this.colsto_sat_cari_kod, this.colsto_cins, this.colsto_doviz_cinsi, this.colsto_detay_takip,
			this.colsto_birim1_ad, this.colsto_birim1_katsayi, this.colsto_birim2_ad, this.colsto_birim2_katsayi, this.colsto_birim3_ad, this.colsto_birim3_katsayi, this.colsto_birim4_ad, this.colsto_birim4_katsayi, this.colsto_bedenli_takip, this.colsto_renkDetayli,
			this.colsto_beden_kodu, this.colsto_renk_kodu, this.colsto_altgrup_kod, this.colsto_anagrup_kod, this.colsto_sektor_kodu, this.colsto_marka_kodu, this.colsto_model_kodu, this.colsto_uretici_kodu, this.colsto_reyon_kodu, this.colsto_standartmaliyet,
			this.colsto_birim1_agirlik, this.colsto_birim1_en, this.colsto_birim1_boy, this.colsto_birim1_yukseklik, this.colsto_birim1_dara, this.colsto_birim2_agirlik, this.colsto_birim2_en, this.colsto_birim2_boy, this.colsto_birim2_yukseklik, this.colsto_birim2_dara,
			this.colsto_birim3_agirlik, this.colsto_birim3_en, this.colsto_birim3_boy, this.colsto_birim3_yukseklik, this.colsto_birim3_dara, this.colsto_birim4_agirlik, this.colsto_birim4_en, this.colsto_birim4_boy, this.colsto_birim4_yukseklik, this.colsto_birim4_dara,
			this.colsto_muh_kod, this.colsto_muh_Iade_kod, this.colsto_muh_sat_muh_kod, this.colsto_muh_satIadmuhkod, this.colsto_muh_sat_isk_kod, this.colsto_muh_aIiskmuhkod, this.colsto_muh_satmalmuhkod, this.colsto_yurtdisi_satmuhk, this.colsto_ilavemasmuhkod, this.colsto_yatirimtesmuhkod,
			this.colsto_depsatmuhkod, this.colsto_depsatmalmuhkod, this.colsto_bagortsatmuhkod, this.colsto_bagortsatIadmuhkod, this.colsto_bagortsatIskmuhkod, this.colsto_satfiyfarkmuhkod, this.colsto_yurtdisisatmalmuhkod, this.colsto_bagortsatmalmuhkod, this.colsto_karorani, this.colsto_min_stok,
			this.colsto_siparis_stok, this.colsto_max_stok, this.colsto_ver_sip_birim, this.colsto_al_sip_birim, this.colsto_siparis_sure, this.colsto_yer_kod, this.colsto_elk_etk_tipi, this.colsto_raf_etiketli, this.colsto_satis_dursun, this.colsto_siparis_dursun,
			this.colsto_malkabul_dursun, this.colsto_iskon_yapilamaz, this.colsto_kategori_kodu, this.colsto_urun_sorkod, this.colsto_muhgrup_kodu, this.colsto_ambalaj_kodu, this.colsto_sezon_kodu, this.colsto_hammadde_kodu, this.colsto_prim_kodu, this.colsto_mkod_artik,
			this.colsto_eksiyedusebilir_fl, this.colsto_otvuygulama, this.colsto_otvtutar, this.colsto_otvliste, this.colsto_prim_orani, this.colsto_garanti_sure, this.colsto_garanti_sure_tipi, this.colsto_oivuygulama, this.colsto_maxiskonto_orani, this.colsto_oivtutar,
			this.colsto_oivvergipntr, this.colsto_webe_gonderilecek_fl, this.colsto_special1, this.colsto_special2, this.colsto_special3
		});
		this.abgv.GridControl = this.gc;
		this.abgv.Name = "abgv";
		this.abgv.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.abgv.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.abgv.OptionsDetail.AllowExpandEmptyDetails = true;
		this.abgv.OptionsNavigation.EnterMoveNextColumn = true;
		this.abgv.OptionsSelection.MultiSelect = true;
		this.abgv.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
		this.abgv.OptionsView.ShowFooter = true;
		this.abgv.OptionsView.ShowGroupPanel = false;
		this.gridBand2.Caption = "Genel bilgiler";
		this.gridBand2.Columns.Add(this.colsto_kod);
		this.gridBand2.Columns.Add(this.colsto_isim);
		this.gridBand2.Columns.Add(this.colsto_yabanci_isim);
		this.gridBand2.Columns.Add(this.colsto_kisa_ismi);
		this.gridBand2.Columns.Add(this.colsto_cins);
		this.gridBand2.Columns.Add(this.colsto_doviz_cinsi);
		this.gridBand2.Columns.Add(this.colsto_standartmaliyet);
		this.gridBand2.Columns.Add(this.colsto_karorani);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.VisibleIndex = 0;
		this.gridBand2.Width = 781;
		this.colsto_kod.Caption = "Kodu";
		this.colsto_kod.FieldName = "sto_kod";
		this.colsto_kod.Name = "colsto_kod";
		this.colsto_kod.Visible = true;
		this.colsto_kod.Width = 105;
		this.colsto_isim.Caption = "İsmi";
		this.colsto_isim.FieldName = "sto_isim";
		this.colsto_isim.Name = "colsto_isim";
		this.colsto_isim.Visible = true;
		this.colsto_isim.Width = 153;
		this.colsto_yabanci_isim.Caption = "Yabancı ismi";
		this.colsto_yabanci_isim.FieldName = "sto_yabanci_isim";
		this.colsto_yabanci_isim.Name = "colsto_yabanci_isim";
		this.colsto_yabanci_isim.Visible = true;
		this.colsto_yabanci_isim.Width = 92;
		this.colsto_kisa_ismi.Caption = "Kısa ismi";
		this.colsto_kisa_ismi.FieldName = "sto_kisa_ismi";
		this.colsto_kisa_ismi.Name = "colsto_kisa_ismi";
		this.colsto_kisa_ismi.Visible = true;
		this.colsto_kisa_ismi.Width = 107;
		this.colsto_cins.Caption = "Cinsi";
		this.colsto_cins.FieldName = "sto_cins";
		this.colsto_cins.Name = "colsto_cins";
		this.colsto_cins.Visible = true;
		this.colsto_cins.Width = 81;
		this.colsto_doviz_cinsi.Caption = "Döviz cinsi";
		this.colsto_doviz_cinsi.FieldName = "sto_doviz_cinsi";
		this.colsto_doviz_cinsi.Name = "colsto_doviz_cinsi";
		this.colsto_doviz_cinsi.Visible = true;
		this.colsto_doviz_cinsi.Width = 94;
		this.colsto_standartmaliyet.Caption = "Standart maliyet";
		this.colsto_standartmaliyet.FieldName = "sto_standartmaliyet";
		this.colsto_standartmaliyet.Name = "colsto_standartmaliyet";
		this.colsto_standartmaliyet.Visible = true;
		this.colsto_standartmaliyet.Width = 92;
		this.colsto_karorani.Caption = "Kar oranı";
		this.colsto_karorani.FieldName = "sto_karorani";
		this.colsto_karorani.Name = "colsto_karorani";
		this.colsto_karorani.Visible = true;
		this.colsto_karorani.Width = 57;
		this.gridBand8.Caption = "Vergi tanımları";
		this.gridBand8.Columns.Add(this.colsto_perakende_vergi);
		this.gridBand8.Columns.Add(this.colsto_toptan_vergi);
		this.gridBand8.Columns.Add(this.colsto_otvuygulama);
		this.gridBand8.Columns.Add(this.colsto_otvtutar);
		this.gridBand8.Columns.Add(this.colsto_otvliste);
		this.gridBand8.Columns.Add(this.colsto_oivuygulama);
		this.gridBand8.Columns.Add(this.colsto_oivtutar);
		this.gridBand8.Columns.Add(this.colsto_oivvergipntr);
		this.gridBand8.Name = "gridBand8";
		this.gridBand8.VisibleIndex = 1;
		this.gridBand8.Width = 660;
		this.colsto_perakende_vergi.Caption = "Perakende KDV";
		this.colsto_perakende_vergi.FieldName = "sto_perakende_vergi";
		this.colsto_perakende_vergi.Name = "colsto_perakende_vergi";
		this.colsto_perakende_vergi.Visible = true;
		this.colsto_perakende_vergi.Width = 90;
		this.colsto_toptan_vergi.Caption = "Toptan KDV";
		this.colsto_toptan_vergi.FieldName = "sto_toptan_vergi";
		this.colsto_toptan_vergi.Name = "colsto_toptan_vergi";
		this.colsto_toptan_vergi.Visible = true;
		this.colsto_toptan_vergi.Width = 83;
		this.colsto_otvuygulama.Caption = "ÖTV uygulama";
		this.colsto_otvuygulama.FieldName = "sto_otvuygulama";
		this.colsto_otvuygulama.Name = "colsto_otvuygulama";
		this.colsto_otvuygulama.Visible = true;
		this.colsto_otvuygulama.Width = 89;
		this.colsto_otvtutar.Caption = "ÖTV tutar/oran";
		this.colsto_otvtutar.FieldName = "sto_otvtutar";
		this.colsto_otvtutar.Name = "colsto_otvtutar";
		this.colsto_otvtutar.Visible = true;
		this.colsto_otvtutar.Width = 90;
		this.colsto_otvliste.Caption = "ÖTV liste no";
		this.colsto_otvliste.FieldName = "sto_otvliste";
		this.colsto_otvliste.Name = "colsto_otvliste";
		this.colsto_otvliste.Visible = true;
		this.colsto_otvliste.Width = 77;
		this.colsto_oivuygulama.Caption = "ÖİV uygulama";
		this.colsto_oivuygulama.FieldName = "sto_oivuygulama";
		this.colsto_oivuygulama.Name = "colsto_oivuygulama";
		this.colsto_oivuygulama.Visible = true;
		this.colsto_oivuygulama.Width = 77;
		this.colsto_oivtutar.Caption = "ÖİV tutar/oran";
		this.colsto_oivtutar.FieldName = "sto_oivtutar";
		this.colsto_oivtutar.Name = "colsto_oivtutar";
		this.colsto_oivtutar.Visible = true;
		this.colsto_oivtutar.Width = 79;
		this.colsto_oivvergipntr.Caption = "ÖİV vergi tipi";
		this.colsto_oivvergipntr.FieldName = "sto_oivvergipntr";
		this.colsto_oivvergipntr.Name = "colsto_oivvergipntr";
		this.colsto_oivvergipntr.Visible = true;
		this.gridBand7.Caption = "Grup tanıtım kodları";
		this.gridBand7.Columns.Add(this.colsto_anagrup_kod);
		this.gridBand7.Columns.Add(this.colsto_altgrup_kod);
		this.gridBand7.Columns.Add(this.colsto_sat_cari_kod);
		this.gridBand7.Columns.Add(this.colsto_uretici_kodu);
		this.gridBand7.Columns.Add(this.colsto_reyon_kodu);
		this.gridBand7.Columns.Add(this.colsto_ambalaj_kodu);
		this.gridBand7.Columns.Add(this.colsto_marka_kodu);
		this.gridBand7.Columns.Add(this.colsto_muhgrup_kodu);
		this.gridBand7.Columns.Add(this.colsto_sektor_kodu);
		this.gridBand7.Columns.Add(this.colsto_urun_sorkod);
		this.gridBand7.Columns.Add(this.colsto_model_kodu);
		this.gridBand7.Columns.Add(this.colsto_sezon_kodu);
		this.gridBand7.Columns.Add(this.colsto_hammadde_kodu);
		this.gridBand7.Columns.Add(this.colsto_kategori_kodu);
		this.gridBand7.Columns.Add(this.colsto_prim_kodu);
		this.gridBand7.Name = "gridBand7";
		this.gridBand7.VisibleIndex = 2;
		this.gridBand7.Width = 1319;
		this.colsto_anagrup_kod.Caption = "Ana grup kodu";
		this.colsto_anagrup_kod.FieldName = "sto_anagrup_kod";
		this.colsto_anagrup_kod.Name = "colsto_anagrup_kod";
		this.colsto_anagrup_kod.Visible = true;
		this.colsto_anagrup_kod.Width = 82;
		this.colsto_altgrup_kod.Caption = "Alt grup kodu";
		this.colsto_altgrup_kod.FieldName = "sto_altgrup_kod";
		this.colsto_altgrup_kod.Name = "colsto_altgrup_kod";
		this.colsto_altgrup_kod.Visible = true;
		this.colsto_altgrup_kod.Width = 82;
		this.colsto_sat_cari_kod.Caption = "Ana sağlayıcı";
		this.colsto_sat_cari_kod.FieldName = "sto_sat_cari_kod";
		this.colsto_sat_cari_kod.Name = "colsto_sat_cari_kod";
		this.colsto_sat_cari_kod.Visible = true;
		this.colsto_sat_cari_kod.Width = 82;
		this.colsto_uretici_kodu.Caption = "Üretici kodu";
		this.colsto_uretici_kodu.FieldName = "sto_uretici_kodu";
		this.colsto_uretici_kodu.Name = "colsto_uretici_kodu";
		this.colsto_uretici_kodu.Visible = true;
		this.colsto_uretici_kodu.Width = 82;
		this.colsto_reyon_kodu.Caption = "Reyon kodu";
		this.colsto_reyon_kodu.FieldName = "sto_reyon_kodu";
		this.colsto_reyon_kodu.Name = "colsto_reyon_kodu";
		this.colsto_reyon_kodu.Visible = true;
		this.colsto_reyon_kodu.Width = 82;
		this.colsto_ambalaj_kodu.Caption = "Ambalaj kodu";
		this.colsto_ambalaj_kodu.FieldName = "sto_ambalaj_kodu";
		this.colsto_ambalaj_kodu.Name = "colsto_ambalaj_kodu";
		this.colsto_ambalaj_kodu.Visible = true;
		this.colsto_ambalaj_kodu.Width = 82;
		this.colsto_marka_kodu.Caption = "Marka kodu";
		this.colsto_marka_kodu.FieldName = "sto_marka_kodu";
		this.colsto_marka_kodu.Name = "colsto_marka_kodu";
		this.colsto_marka_kodu.Visible = true;
		this.colsto_marka_kodu.Width = 72;
		this.colsto_muhgrup_kodu.Caption = "Muhasebe grup kodu";
		this.colsto_muhgrup_kodu.FieldName = "sto_muhgrup_kodu";
		this.colsto_muhgrup_kodu.Name = "colsto_muhgrup_kodu";
		this.colsto_muhgrup_kodu.Visible = true;
		this.colsto_muhgrup_kodu.Width = 116;
		this.colsto_sektor_kodu.Caption = "Sektör kodu";
		this.colsto_sektor_kodu.FieldName = "sto_sektor_kodu";
		this.colsto_sektor_kodu.Name = "colsto_sektor_kodu";
		this.colsto_sektor_kodu.Visible = true;
		this.colsto_sektor_kodu.Width = 69;
		this.colsto_urun_sorkod.Caption = "Ürün sorumlusu kodu";
		this.colsto_urun_sorkod.FieldName = "sto_urun_sorkod";
		this.colsto_urun_sorkod.Name = "colsto_urun_sorkod";
		this.colsto_urun_sorkod.Visible = true;
		this.colsto_urun_sorkod.Width = 120;
		this.colsto_model_kodu.Caption = "Model kodu";
		this.colsto_model_kodu.FieldName = "sto_model_kodu";
		this.colsto_model_kodu.Name = "colsto_model_kodu";
		this.colsto_model_kodu.Visible = true;
		this.colsto_model_kodu.Width = 73;
		this.colsto_sezon_kodu.Caption = "Yıl ve sezon kodu";
		this.colsto_sezon_kodu.FieldName = "sto_sezon_kodu";
		this.colsto_sezon_kodu.Name = "colsto_sezon_kodu";
		this.colsto_sezon_kodu.Visible = true;
		this.colsto_sezon_kodu.Width = 91;
		this.colsto_hammadde_kodu.Caption = "Anahammadde kodu";
		this.colsto_hammadde_kodu.FieldName = "sto_hammadde_kodu";
		this.colsto_hammadde_kodu.Name = "colsto_hammadde_kodu";
		this.colsto_hammadde_kodu.Visible = true;
		this.colsto_hammadde_kodu.Width = 116;
		this.colsto_kategori_kodu.Caption = "Kategori kodu";
		this.colsto_kategori_kodu.FieldName = "sto_kategori_kodu";
		this.colsto_kategori_kodu.Name = "colsto_kategori_kodu";
		this.colsto_kategori_kodu.Visible = true;
		this.colsto_kategori_kodu.Width = 83;
		this.colsto_prim_kodu.Caption = "Prim grup kodu";
		this.colsto_prim_kodu.FieldName = "sto_prim_kodu";
		this.colsto_prim_kodu.Name = "colsto_prim_kodu";
		this.colsto_prim_kodu.Visible = true;
		this.colsto_prim_kodu.Width = 87;
		this.gridBand4.Caption = "Detaylar";
		this.gridBand4.Columns.Add(this.colsto_yer_kod);
		this.gridBand4.Columns.Add(this.colsto_min_stok);
		this.gridBand4.Columns.Add(this.colsto_siparis_stok);
		this.gridBand4.Columns.Add(this.colsto_max_stok);
		this.gridBand4.Columns.Add(this.colsto_ver_sip_birim);
		this.gridBand4.Columns.Add(this.colsto_al_sip_birim);
		this.gridBand4.Columns.Add(this.colsto_siparis_sure);
		this.gridBand4.Columns.Add(this.colsto_special1);
		this.gridBand4.Columns.Add(this.colsto_special2);
		this.gridBand4.Columns.Add(this.colsto_special3);
		this.gridBand4.Name = "gridBand4";
		this.gridBand4.VisibleIndex = 3;
		this.gridBand4.Width = 861;
		this.colsto_yer_kod.Caption = "Ambar adresi";
		this.colsto_yer_kod.FieldName = "sto_yer_kod";
		this.colsto_yer_kod.Name = "colsto_yer_kod";
		this.colsto_yer_kod.Visible = true;
		this.colsto_yer_kod.Width = 71;
		this.colsto_min_stok.Caption = "Minimum seviyesi";
		this.colsto_min_stok.FieldName = "sto_min_stok";
		this.colsto_min_stok.Name = "colsto_min_stok";
		this.colsto_min_stok.Visible = true;
		this.colsto_min_stok.Width = 92;
		this.colsto_siparis_stok.Caption = "Sipariş seviyesi";
		this.colsto_siparis_stok.FieldName = "sto_siparis_stok";
		this.colsto_siparis_stok.Name = "colsto_siparis_stok";
		this.colsto_siparis_stok.Visible = true;
		this.colsto_siparis_stok.Width = 83;
		this.colsto_max_stok.Caption = "Hedef seviyesi";
		this.colsto_max_stok.FieldName = "sto_max_stok";
		this.colsto_max_stok.Name = "colsto_max_stok";
		this.colsto_max_stok.Visible = true;
		this.colsto_max_stok.Width = 104;
		this.colsto_ver_sip_birim.Caption = "Verilen sipariş birimi";
		this.colsto_ver_sip_birim.FieldName = "sto_ver_sip_birim";
		this.colsto_ver_sip_birim.Name = "colsto_ver_sip_birim";
		this.colsto_ver_sip_birim.Visible = true;
		this.colsto_ver_sip_birim.Width = 107;
		this.colsto_al_sip_birim.Caption = "Alınan sipariş birimi";
		this.colsto_al_sip_birim.FieldName = "sto_al_sip_birim";
		this.colsto_al_sip_birim.Name = "colsto_al_sip_birim";
		this.colsto_al_sip_birim.Visible = true;
		this.colsto_al_sip_birim.Width = 99;
		this.colsto_siparis_sure.Caption = "Sipariş süresi (gün)";
		this.colsto_siparis_sure.FieldName = "sto_siparis_sure";
		this.colsto_siparis_sure.Name = "colsto_siparis_sure";
		this.colsto_siparis_sure.Visible = true;
		this.colsto_siparis_sure.Width = 106;
		this.colsto_special1.Caption = "Özel kod 1";
		this.colsto_special1.FieldName = "sto_special1";
		this.colsto_special1.Name = "colsto_special1";
		this.colsto_special1.Visible = true;
		this.colsto_special1.Width = 68;
		this.colsto_special2.Caption = "Özel kod 2";
		this.colsto_special2.FieldName = "sto_special2";
		this.colsto_special2.Name = "colsto_special2";
		this.colsto_special2.Visible = true;
		this.colsto_special2.Width = 65;
		this.colsto_special3.Caption = "Özel kod 3";
		this.colsto_special3.FieldName = "sto_special3";
		this.colsto_special3.Name = "colsto_special3";
		this.colsto_special3.Visible = true;
		this.colsto_special3.Width = 66;
		this.gridBand3.Caption = "Birimler";
		this.gridBand3.Columns.Add(this.colsto_birim1_ad);
		this.gridBand3.Columns.Add(this.colsto_birim1_katsayi);
		this.gridBand3.Columns.Add(this.colsto_birim1_agirlik);
		this.gridBand3.Columns.Add(this.colsto_birim1_en);
		this.gridBand3.Columns.Add(this.colsto_birim1_boy);
		this.gridBand3.Columns.Add(this.colsto_birim1_yukseklik);
		this.gridBand3.Columns.Add(this.colsto_birim1_dara);
		this.gridBand3.Columns.Add(this.colsto_birim2_ad);
		this.gridBand3.Columns.Add(this.colsto_birim2_katsayi);
		this.gridBand3.Columns.Add(this.colsto_birim2_agirlik);
		this.gridBand3.Columns.Add(this.colsto_birim2_en);
		this.gridBand3.Columns.Add(this.colsto_birim2_boy);
		this.gridBand3.Columns.Add(this.colsto_birim2_yukseklik);
		this.gridBand3.Columns.Add(this.colsto_birim2_dara);
		this.gridBand3.Columns.Add(this.colsto_birim3_ad);
		this.gridBand3.Columns.Add(this.colsto_birim3_katsayi);
		this.gridBand3.Columns.Add(this.colsto_birim3_agirlik);
		this.gridBand3.Columns.Add(this.colsto_birim3_en);
		this.gridBand3.Columns.Add(this.colsto_birim3_boy);
		this.gridBand3.Columns.Add(this.colsto_birim3_yukseklik);
		this.gridBand3.Columns.Add(this.colsto_birim3_dara);
		this.gridBand3.Columns.Add(this.colsto_birim4_ad);
		this.gridBand3.Columns.Add(this.colsto_birim4_katsayi);
		this.gridBand3.Columns.Add(this.colsto_birim4_agirlik);
		this.gridBand3.Columns.Add(this.colsto_birim4_en);
		this.gridBand3.Columns.Add(this.colsto_birim4_boy);
		this.gridBand3.Columns.Add(this.colsto_birim4_yukseklik);
		this.gridBand3.Columns.Add(this.colsto_birim4_dara);
		this.gridBand3.Name = "gridBand3";
		this.gridBand3.VisibleIndex = 4;
		this.gridBand3.Width = 2167;
		this.colsto_birim1_ad.Caption = "1. birim adı";
		this.colsto_birim1_ad.FieldName = "sto_birim1_ad";
		this.colsto_birim1_ad.Name = "colsto_birim1_ad";
		this.colsto_birim1_ad.Visible = true;
		this.colsto_birim1_katsayi.Caption = "1. birim katsayı";
		this.colsto_birim1_katsayi.FieldName = "sto_birim1_katsayi";
		this.colsto_birim1_katsayi.Name = "colsto_birim1_katsayi";
		this.colsto_birim1_katsayi.Visible = true;
		this.colsto_birim1_katsayi.Width = 85;
		this.colsto_birim1_agirlik.Caption = "1. birim ağırlık";
		this.colsto_birim1_agirlik.FieldName = "sto_birim1_agirlik";
		this.colsto_birim1_agirlik.Name = "colsto_birim1_agirlik";
		this.colsto_birim1_agirlik.Visible = true;
		this.colsto_birim1_en.Caption = "1. birim en";
		this.colsto_birim1_en.FieldName = "sto_birim1_en";
		this.colsto_birim1_en.Name = "colsto_birim1_en";
		this.colsto_birim1_en.Visible = true;
		this.colsto_birim1_boy.Caption = "1. birim boy";
		this.colsto_birim1_boy.FieldName = "sto_birim1_boy";
		this.colsto_birim1_boy.Name = "colsto_birim1_boy";
		this.colsto_birim1_boy.Visible = true;
		this.colsto_birim1_yukseklik.Caption = "1. birim yükseklik";
		this.colsto_birim1_yukseklik.FieldName = "sto_birim1_yukseklik";
		this.colsto_birim1_yukseklik.Name = "colsto_birim1_yukseklik";
		this.colsto_birim1_yukseklik.Visible = true;
		this.colsto_birim1_yukseklik.Width = 88;
		this.colsto_birim1_dara.Caption = "1. birim dara";
		this.colsto_birim1_dara.FieldName = "sto_birim1_dara";
		this.colsto_birim1_dara.Name = "colsto_birim1_dara";
		this.colsto_birim1_dara.Visible = true;
		this.colsto_birim2_ad.Caption = "2. birim adı";
		this.colsto_birim2_ad.FieldName = "sto_birim2_ad";
		this.colsto_birim2_ad.Name = "colsto_birim2_ad";
		this.colsto_birim2_ad.Visible = true;
		this.colsto_birim2_katsayi.Caption = "2. birim kaysayı";
		this.colsto_birim2_katsayi.FieldName = "sto_birim2_katsayi";
		this.colsto_birim2_katsayi.Name = "colsto_birim2_katsayi";
		this.colsto_birim2_katsayi.Visible = true;
		this.colsto_birim2_katsayi.Width = 85;
		this.colsto_birim2_agirlik.Caption = "2. birim ağırlık";
		this.colsto_birim2_agirlik.FieldName = "sto_birim2_agirlik";
		this.colsto_birim2_agirlik.Name = "colsto_birim2_agirlik";
		this.colsto_birim2_agirlik.Visible = true;
		this.colsto_birim2_en.Caption = "2. birim en";
		this.colsto_birim2_en.FieldName = "sto_birim2_en";
		this.colsto_birim2_en.Name = "colsto_birim2_en";
		this.colsto_birim2_en.Visible = true;
		this.colsto_birim2_boy.Caption = "2. birim boy";
		this.colsto_birim2_boy.FieldName = "sto_birim2_boy";
		this.colsto_birim2_boy.Name = "colsto_birim2_boy";
		this.colsto_birim2_boy.Visible = true;
		this.colsto_birim2_yukseklik.Caption = "2. birim yükseklik";
		this.colsto_birim2_yukseklik.FieldName = "sto_birim2_yukseklik";
		this.colsto_birim2_yukseklik.Name = "colsto_birim2_yukseklik";
		this.colsto_birim2_yukseklik.Visible = true;
		this.colsto_birim2_yukseklik.Width = 95;
		this.colsto_birim2_dara.Caption = "2. birim dara";
		this.colsto_birim2_dara.FieldName = "sto_birim2_dara";
		this.colsto_birim2_dara.Name = "colsto_birim2_dara";
		this.colsto_birim2_dara.Visible = true;
		this.colsto_birim3_ad.Caption = "3. birim adı";
		this.colsto_birim3_ad.FieldName = "sto_birim3_ad";
		this.colsto_birim3_ad.Name = "colsto_birim3_ad";
		this.colsto_birim3_ad.Visible = true;
		this.colsto_birim3_katsayi.Caption = "3. birim katsayı";
		this.colsto_birim3_katsayi.FieldName = "sto_birim3_katsayi";
		this.colsto_birim3_katsayi.Name = "colsto_birim3_katsayi";
		this.colsto_birim3_katsayi.Visible = true;
		this.colsto_birim3_katsayi.Width = 81;
		this.colsto_birim3_agirlik.Caption = "3. birim ağırlık";
		this.colsto_birim3_agirlik.FieldName = "sto_birim3_agirlik";
		this.colsto_birim3_agirlik.Name = "colsto_birim3_agirlik";
		this.colsto_birim3_agirlik.Visible = true;
		this.colsto_birim3_en.Caption = "3. birim en";
		this.colsto_birim3_en.FieldName = "sto_birim3_en";
		this.colsto_birim3_en.Name = "colsto_birim3_en";
		this.colsto_birim3_en.Visible = true;
		this.colsto_birim3_en.Width = 65;
		this.colsto_birim3_boy.Caption = "3. birim boy";
		this.colsto_birim3_boy.FieldName = "sto_birim3_boy";
		this.colsto_birim3_boy.Name = "colsto_birim3_boy";
		this.colsto_birim3_boy.Visible = true;
		this.colsto_birim3_boy.Width = 69;
		this.colsto_birim3_yukseklik.Caption = "3. birim yükseklik";
		this.colsto_birim3_yukseklik.FieldName = "sto_birim3_yukseklik";
		this.colsto_birim3_yukseklik.Name = "colsto_birim3_yukseklik";
		this.colsto_birim3_yukseklik.Visible = true;
		this.colsto_birim3_yukseklik.Width = 91;
		this.colsto_birim3_dara.Caption = "3. birim dara";
		this.colsto_birim3_dara.FieldName = "sto_birim3_dara";
		this.colsto_birim3_dara.Name = "colsto_birim3_dara";
		this.colsto_birim3_dara.Visible = true;
		this.colsto_birim4_ad.Caption = "4. birim adı";
		this.colsto_birim4_ad.FieldName = "sto_birim4_ad";
		this.colsto_birim4_ad.Name = "colsto_birim4_ad";
		this.colsto_birim4_ad.Visible = true;
		this.colsto_birim4_katsayi.Caption = "4. birim katsayı";
		this.colsto_birim4_katsayi.FieldName = "sto_birim4_katsayi";
		this.colsto_birim4_katsayi.Name = "colsto_birim4_katsayi";
		this.colsto_birim4_katsayi.Visible = true;
		this.colsto_birim4_katsayi.Width = 82;
		this.colsto_birim4_agirlik.Caption = "4. birim ağırlık";
		this.colsto_birim4_agirlik.FieldName = "sto_birim4_agirlik";
		this.colsto_birim4_agirlik.Name = "colsto_birim4_agirlik";
		this.colsto_birim4_agirlik.Visible = true;
		this.colsto_birim4_en.Caption = "4. birim en";
		this.colsto_birim4_en.FieldName = "sto_birim4_en";
		this.colsto_birim4_en.Name = "colsto_birim4_en";
		this.colsto_birim4_en.Visible = true;
		this.colsto_birim4_en.Width = 68;
		this.colsto_birim4_boy.Caption = "4. birim boy";
		this.colsto_birim4_boy.FieldName = "sto_birim4_boy";
		this.colsto_birim4_boy.Name = "colsto_birim4_boy";
		this.colsto_birim4_boy.Visible = true;
		this.colsto_birim4_boy.Width = 68;
		this.colsto_birim4_yukseklik.Caption = "4. birim yükseklik";
		this.colsto_birim4_yukseklik.FieldName = "sto_birim4_yukseklik";
		this.colsto_birim4_yukseklik.Name = "colsto_birim4_yukseklik";
		this.colsto_birim4_yukseklik.Visible = true;
		this.colsto_birim4_yukseklik.Width = 88;
		this.colsto_birim4_dara.Caption = "4. birim dara";
		this.colsto_birim4_dara.FieldName = "sto_birim4_dara";
		this.colsto_birim4_dara.Name = "colsto_birim4_dara";
		this.colsto_birim4_dara.Visible = true;
		this.colsto_birim4_dara.Width = 77;
		this.gridBand6.Caption = "Entegrasyon kodları";
		this.gridBand6.Columns.Add(this.colsto_muh_kod);
		this.gridBand6.Columns.Add(this.colsto_muh_Iade_kod);
		this.gridBand6.Columns.Add(this.colsto_muh_sat_muh_kod);
		this.gridBand6.Columns.Add(this.colsto_muh_satmalmuhkod);
		this.gridBand6.Columns.Add(this.colsto_yurtdisi_satmuhk);
		this.gridBand6.Columns.Add(this.colsto_yurtdisisatmalmuhkod);
		this.gridBand6.Columns.Add(this.colsto_muh_satIadmuhkod);
		this.gridBand6.Columns.Add(this.colsto_muh_sat_isk_kod);
		this.gridBand6.Columns.Add(this.colsto_muh_aIiskmuhkod);
		this.gridBand6.Columns.Add(this.colsto_ilavemasmuhkod);
		this.gridBand6.Columns.Add(this.colsto_yatirimtesmuhkod);
		this.gridBand6.Columns.Add(this.colsto_depsatmuhkod);
		this.gridBand6.Columns.Add(this.colsto_depsatmalmuhkod);
		this.gridBand6.Columns.Add(this.colsto_bagortsatmuhkod);
		this.gridBand6.Columns.Add(this.colsto_bagortsatIadmuhkod);
		this.gridBand6.Columns.Add(this.colsto_bagortsatIskmuhkod);
		this.gridBand6.Columns.Add(this.colsto_bagortsatmalmuhkod);
		this.gridBand6.Columns.Add(this.colsto_satfiyfarkmuhkod);
		this.gridBand6.Columns.Add(this.colsto_mkod_artik);
		this.gridBand6.Name = "gridBand6";
		this.gridBand6.VisibleIndex = 5;
		this.gridBand6.Width = 2017;
		this.colsto_muh_kod.Caption = "Stok";
		this.colsto_muh_kod.FieldName = "sto_muh_kod";
		this.colsto_muh_kod.Name = "colsto_muh_kod";
		this.colsto_muh_kod.Visible = true;
		this.colsto_muh_Iade_kod.Caption = "Stok iade";
		this.colsto_muh_Iade_kod.FieldName = "sto_muh_Iade_kod";
		this.colsto_muh_Iade_kod.Name = "colsto_muh_Iade_kod";
		this.colsto_muh_Iade_kod.Visible = true;
		this.colsto_muh_sat_muh_kod.Caption = "Yurt içi satış";
		this.colsto_muh_sat_muh_kod.FieldName = "sto_muh_sat_muh_kod";
		this.colsto_muh_sat_muh_kod.Name = "colsto_muh_sat_muh_kod";
		this.colsto_muh_sat_muh_kod.Visible = true;
		this.colsto_muh_satmalmuhkod.Caption = "Satılan mal maliyeti";
		this.colsto_muh_satmalmuhkod.FieldName = "sto_muh_satmalmuhkod";
		this.colsto_muh_satmalmuhkod.Name = "colsto_muh_satmalmuhkod";
		this.colsto_muh_satmalmuhkod.Visible = true;
		this.colsto_muh_satmalmuhkod.Width = 104;
		this.colsto_yurtdisi_satmuhk.Caption = "Yurt dışı satış";
		this.colsto_yurtdisi_satmuhk.FieldName = "sto_yurtdisi_satmuhk";
		this.colsto_yurtdisi_satmuhk.Name = "colsto_yurtdisi_satmuhk";
		this.colsto_yurtdisi_satmuhk.Visible = true;
		this.colsto_yurtdisisatmalmuhkod.Caption = "Yurt dışı satılan mal maliyeti";
		this.colsto_yurtdisisatmalmuhkod.FieldName = "sto_yurtdisisatmalmuhkod";
		this.colsto_yurtdisisatmalmuhkod.Name = "colsto_yurtdisisatmalmuhkod";
		this.colsto_yurtdisisatmalmuhkod.Visible = true;
		this.colsto_yurtdisisatmalmuhkod.Width = 147;
		this.colsto_muh_satIadmuhkod.Caption = "Satış iade";
		this.colsto_muh_satIadmuhkod.FieldName = "sto_muh_satIadmuhkod";
		this.colsto_muh_satIadmuhkod.Name = "colsto_muh_satIadmuhkod";
		this.colsto_muh_satIadmuhkod.Visible = true;
		this.colsto_muh_sat_isk_kod.Caption = "Satış iskonto";
		this.colsto_muh_sat_isk_kod.FieldName = "sto_muh_sat_isk_kod";
		this.colsto_muh_sat_isk_kod.Name = "colsto_muh_sat_isk_kod";
		this.colsto_muh_sat_isk_kod.Visible = true;
		this.colsto_muh_aIiskmuhkod.Caption = "Alış iskonto";
		this.colsto_muh_aIiskmuhkod.FieldName = "sto_muh_aIiskmuhkod";
		this.colsto_muh_aIiskmuhkod.Name = "colsto_muh_aIiskmuhkod";
		this.colsto_muh_aIiskmuhkod.Visible = true;
		this.colsto_ilavemasmuhkod.Caption = "İlave masraf";
		this.colsto_ilavemasmuhkod.FieldName = "sto_ilavemasmuhkod";
		this.colsto_ilavemasmuhkod.Name = "colsto_ilavemasmuhkod";
		this.colsto_ilavemasmuhkod.Visible = true;
		this.colsto_yatirimtesmuhkod.Caption = "Yatırım teşvik";
		this.colsto_yatirimtesmuhkod.FieldName = "sto_yatirimtesmuhkod";
		this.colsto_yatirimtesmuhkod.Name = "colsto_yatirimtesmuhkod";
		this.colsto_yatirimtesmuhkod.Visible = true;
		this.colsto_depsatmuhkod.Caption = "Depolar arası satış";
		this.colsto_depsatmuhkod.FieldName = "sto_depsatmuhkod";
		this.colsto_depsatmuhkod.Name = "colsto_depsatmuhkod";
		this.colsto_depsatmuhkod.Visible = true;
		this.colsto_depsatmuhkod.Width = 100;
		this.colsto_depsatmalmuhkod.Caption = "Depolar arası satış maliyeti";
		this.colsto_depsatmalmuhkod.FieldName = "sto_depsatmalmuhkod";
		this.colsto_depsatmalmuhkod.Name = "colsto_depsatmalmuhkod";
		this.colsto_depsatmalmuhkod.Visible = true;
		this.colsto_depsatmalmuhkod.Width = 142;
		this.colsto_bagortsatmuhkod.Caption = "Bağlı ortaklara yapılan satış";
		this.colsto_bagortsatmuhkod.FieldName = "sto_bagortsatmuhkod";
		this.colsto_bagortsatmuhkod.Name = "colsto_bagortsatmuhkod";
		this.colsto_bagortsatmuhkod.Visible = true;
		this.colsto_bagortsatmuhkod.Width = 150;
		this.colsto_bagortsatIadmuhkod.Caption = "Bağlı ortaklara yapılan satış iade";
		this.colsto_bagortsatIadmuhkod.FieldName = "sto_bagortsatIadmuhkod";
		this.colsto_bagortsatIadmuhkod.Name = "colsto_bagortsatIadmuhkod";
		this.colsto_bagortsatIadmuhkod.Visible = true;
		this.colsto_bagortsatIadmuhkod.Width = 164;
		this.colsto_bagortsatIskmuhkod.Caption = "Bağlı ortaklara yapılan satış iskonto";
		this.colsto_bagortsatIskmuhkod.FieldName = "sto_bagortsatIskmuhkod";
		this.colsto_bagortsatIskmuhkod.Name = "colsto_bagortsatIskmuhkod";
		this.colsto_bagortsatIskmuhkod.Visible = true;
		this.colsto_bagortsatIskmuhkod.Width = 179;
		this.colsto_bagortsatmalmuhkod.Caption = "Bağlı ortaklara satılan mal maliyeti";
		this.colsto_bagortsatmalmuhkod.FieldName = "sto_bagortsatmalmuhkod";
		this.colsto_bagortsatmalmuhkod.Name = "colsto_bagortsatmalmuhkod";
		this.colsto_bagortsatmalmuhkod.Visible = true;
		this.colsto_bagortsatmalmuhkod.Width = 177;
		this.colsto_satfiyfarkmuhkod.Caption = "Satış fiyat farkı";
		this.colsto_satfiyfarkmuhkod.FieldName = "sto_satfiyfarkmuhkod";
		this.colsto_satfiyfarkmuhkod.Name = "colsto_satfiyfarkmuhkod";
		this.colsto_satfiyfarkmuhkod.Visible = true;
		this.colsto_satfiyfarkmuhkod.Width = 85;
		this.colsto_mkod_artik.Caption = "Muh. kod artikeli";
		this.colsto_mkod_artik.FieldName = "sto_mkod_artik";
		this.colsto_mkod_artik.Name = "colsto_mkod_artik";
		this.colsto_mkod_artik.Visible = true;
		this.colsto_mkod_artik.Width = 94;
		this.gridBand5.Caption = "Takip detayları";
		this.gridBand5.Columns.Add(this.colsto_detay_takip);
		this.gridBand5.Columns.Add(this.colsto_bedenli_takip);
		this.gridBand5.Columns.Add(this.colsto_beden_kodu);
		this.gridBand5.Columns.Add(this.colsto_renkDetayli);
		this.gridBand5.Columns.Add(this.colsto_renk_kodu);
		this.gridBand5.Name = "gridBand5";
		this.gridBand5.VisibleIndex = 6;
		this.gridBand5.Width = 387;
		this.colsto_detay_takip.Caption = "Detay takip şekli";
		this.colsto_detay_takip.FieldName = "sto_detay_takip";
		this.colsto_detay_takip.Name = "colsto_detay_takip";
		this.colsto_detay_takip.Visible = true;
		this.colsto_detay_takip.Width = 87;
		this.colsto_bedenli_takip.Caption = "Beden detaylı";
		this.colsto_bedenli_takip.FieldName = "sto_bedenli_takip";
		this.colsto_bedenli_takip.Name = "colsto_bedenli_takip";
		this.colsto_bedenli_takip.Visible = true;
		this.colsto_beden_kodu.Caption = "Beden kodu";
		this.colsto_beden_kodu.FieldName = "sto_beden_kodu";
		this.colsto_beden_kodu.Name = "colsto_beden_kodu";
		this.colsto_beden_kodu.Visible = true;
		this.colsto_renkDetayli.Caption = "Renk detaylı";
		this.colsto_renkDetayli.FieldName = "sto_renkDetayli";
		this.colsto_renkDetayli.Name = "colsto_renkDetayli";
		this.colsto_renkDetayli.Visible = true;
		this.colsto_renk_kodu.Caption = "Renk kodu";
		this.colsto_renk_kodu.FieldName = "sto_renk_kodu";
		this.colsto_renk_kodu.Name = "colsto_renk_kodu";
		this.colsto_renk_kodu.Visible = true;
		this.gridBand1.Caption = "Pozisyon bayrakları";
		this.gridBand1.Columns.Add(this.colsto_raf_etiketli);
		this.gridBand1.Columns.Add(this.colsto_elk_etk_tipi);
		this.gridBand1.Columns.Add(this.colsto_satis_dursun);
		this.gridBand1.Columns.Add(this.colsto_siparis_dursun);
		this.gridBand1.Columns.Add(this.colsto_malkabul_dursun);
		this.gridBand1.Columns.Add(this.colsto_iskon_yapilamaz);
		this.gridBand1.Columns.Add(this.colsto_eksiyedusebilir_fl);
		this.gridBand1.Columns.Add(this.colsto_prim_orani);
		this.gridBand1.Columns.Add(this.colsto_garanti_sure);
		this.gridBand1.Columns.Add(this.colsto_garanti_sure_tipi);
		this.gridBand1.Columns.Add(this.colsto_maxiskonto_orani);
		this.gridBand1.Columns.Add(this.colsto_webe_gonderilecek_fl);
		this.gridBand1.Name = "gridBand1";
		this.gridBand1.VisibleIndex = 7;
		this.gridBand1.Width = 1285;
		this.colsto_raf_etiketli.Caption = "Raf etiketi";
		this.colsto_raf_etiketli.FieldName = "sto_raf_etiketli";
		this.colsto_raf_etiketli.Name = "colsto_raf_etiketli";
		this.colsto_raf_etiketli.Visible = true;
		this.colsto_raf_etiketli.Width = 78;
		this.colsto_elk_etk_tipi.Caption = "Elektronik etiket tipi";
		this.colsto_elk_etk_tipi.FieldName = "sto_elk_etk_tipi";
		this.colsto_elk_etk_tipi.Name = "colsto_elk_etk_tipi";
		this.colsto_elk_etk_tipi.Visible = true;
		this.colsto_elk_etk_tipi.Width = 112;
		this.colsto_satis_dursun.Caption = "Satış";
		this.colsto_satis_dursun.FieldName = "sto_satis_dursun";
		this.colsto_satis_dursun.Name = "colsto_satis_dursun";
		this.colsto_satis_dursun.Visible = true;
		this.colsto_satis_dursun.Width = 78;
		this.colsto_siparis_dursun.Caption = "Sipariş";
		this.colsto_siparis_dursun.FieldName = "sto_siparis_dursun";
		this.colsto_siparis_dursun.Name = "colsto_siparis_dursun";
		this.colsto_siparis_dursun.Visible = true;
		this.colsto_siparis_dursun.Width = 78;
		this.colsto_malkabul_dursun.Caption = "Mal kabul";
		this.colsto_malkabul_dursun.FieldName = "sto_malkabul_dursun";
		this.colsto_malkabul_dursun.Name = "colsto_malkabul_dursun";
		this.colsto_malkabul_dursun.Visible = true;
		this.colsto_malkabul_dursun.Width = 78;
		this.colsto_iskon_yapilamaz.Caption = "İskonto yapılamaz";
		this.colsto_iskon_yapilamaz.FieldName = "sto_iskon_yapilamaz";
		this.colsto_iskon_yapilamaz.Name = "colsto_iskon_yapilamaz";
		this.colsto_iskon_yapilamaz.Visible = true;
		this.colsto_iskon_yapilamaz.Width = 111;
		this.colsto_eksiyedusebilir_fl.Caption = "Eksiye düşme kontrolünde göz ardı edilsin";
		this.colsto_eksiyedusebilir_fl.FieldName = "sto_eksiyedusebilir_fl";
		this.colsto_eksiyedusebilir_fl.Name = "colsto_eksiyedusebilir_fl";
		this.colsto_eksiyedusebilir_fl.Visible = true;
		this.colsto_eksiyedusebilir_fl.Width = 218;
		this.colsto_prim_orani.Caption = "Prim oranı";
		this.colsto_prim_orani.FieldName = "sto_prim_orani";
		this.colsto_prim_orani.Name = "colsto_prim_orani";
		this.colsto_prim_orani.Visible = true;
		this.colsto_prim_orani.Width = 78;
		this.colsto_garanti_sure.Caption = "Garanti süresi";
		this.colsto_garanti_sure.FieldName = "sto_garanti_sure";
		this.colsto_garanti_sure.Name = "colsto_garanti_sure";
		this.colsto_garanti_sure.Visible = true;
		this.colsto_garanti_sure.Width = 78;
		this.colsto_garanti_sure_tipi.Caption = "Garanti süresi tipi";
		this.colsto_garanti_sure_tipi.FieldName = "sto_garanti_sure_tipi";
		this.colsto_garanti_sure_tipi.Name = "colsto_garanti_sure_tipi";
		this.colsto_garanti_sure_tipi.Visible = true;
		this.colsto_garanti_sure_tipi.Width = 104;
		this.colsto_maxiskonto_orani.Caption = "Maksimum iskonto oranı";
		this.colsto_maxiskonto_orani.FieldName = "sto_maxiskonto_orani";
		this.colsto_maxiskonto_orani.Name = "colsto_maxiskonto_orani";
		this.colsto_maxiskonto_orani.Visible = true;
		this.colsto_maxiskonto_orani.Width = 128;
		this.colsto_webe_gonderilecek_fl.Caption = "Web sayfasına gönderilecek";
		this.colsto_webe_gonderilecek_fl.FieldName = "sto_webe_gonderilecek_fl";
		this.colsto_webe_gonderilecek_fl.Name = "colsto_webe_gonderilecek_fl";
		this.colsto_webe_gonderilecek_fl.Visible = true;
		this.colsto_webe_gonderilecek_fl.Width = 144;
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.dosyaToolStripMenuItem, this.görünümToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(1068, 24);
		this.menuStrip1.TabIndex = 2;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.yeniToolStripMenuItem, this.kapatToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.yeniToolStripMenuItem.Name = "yeniToolStripMenuItem";
		this.yeniToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.yeniToolStripMenuItem.Text = "Yeni";
		this.yeniToolStripMenuItem.Click += new System.EventHandler(yeniToolStripMenuItem_Click);
		this.kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
		this.kapatToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.kapatToolStripMenuItem.Text = "Kapat";
		this.kapatToolStripMenuItem.Click += new System.EventHandler(kapatToolStripMenuItem_Click);
		this.görünümToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.stoklarToolStripMenuItem, this.toolStripSeparator1, this.aramaTablolariToolStripMenuItem });
		this.görünümToolStripMenuItem.Name = "görünümToolStripMenuItem";
		this.görünümToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
		this.görünümToolStripMenuItem.Text = "Görünüm";
		this.stoklarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.kolonlaraGöreGruplamaToolStripMenuItem, this.kolonSeçiciyiGösterToolStripMenuItem, this.bütünGruplarıAçToolStripMenuItem, this.bütünGruplarıKapatToolStripMenuItem, this.toolStripSeparator2, this.görünümüSaklaToolStripMenuItem, this.görünümüYükleToolStripMenuItem, this.otomatikDosyayıSilToolStripMenuItem, this.varsayılanaGeriDönToolStripMenuItem });
		this.stoklarToolStripMenuItem.Name = "stoklarToolStripMenuItem";
		this.stoklarToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
		this.stoklarToolStripMenuItem.Text = "Stoklar";
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
		this.aramaTablolariToolStripMenuItem.Name = "aramaTablolariToolStripMenuItem";
		this.aramaTablolariToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
		this.aramaTablolariToolStripMenuItem.Text = "Arama tabloları";
		this.kolonlaraGöreGruplamaToolStripMenuItem.Name = "kolonlaraGöreGruplamaToolStripMenuItem";
		this.kolonlaraGöreGruplamaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.kolonlaraGöreGruplamaToolStripMenuItem.Text = "Kolonlara göre gruplama";
		this.kolonlaraGöreGruplamaToolStripMenuItem.Click += new System.EventHandler(kolonlaraGöreGruplamaToolStripMenuItem_Click);
		this.kolonSeçiciyiGösterToolStripMenuItem.Name = "kolonSeçiciyiGösterToolStripMenuItem";
		this.kolonSeçiciyiGösterToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.kolonSeçiciyiGösterToolStripMenuItem.Text = "Kolon seçiciyi göster";
		this.kolonSeçiciyiGösterToolStripMenuItem.Click += new System.EventHandler(kolonSeçiciyiGösterToolStripMenuItem_Click);
		this.bütünGruplarıAçToolStripMenuItem.Name = "bütünGruplarıAçToolStripMenuItem";
		this.bütünGruplarıAçToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.bütünGruplarıAçToolStripMenuItem.Text = "Bütün grupları aç";
		this.bütünGruplarıAçToolStripMenuItem.Click += new System.EventHandler(bütünGruplarıAçToolStripMenuItem_Click);
		this.bütünGruplarıKapatToolStripMenuItem.Name = "bütünGruplarıKapatToolStripMenuItem";
		this.bütünGruplarıKapatToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.bütünGruplarıKapatToolStripMenuItem.Text = "Bütün grupları kapat";
		this.bütünGruplarıKapatToolStripMenuItem.Click += new System.EventHandler(bütünGruplarıKapatToolStripMenuItem_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(202, 6);
		this.görünümüSaklaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikDosyayaToolStripMenuItem, this.farklıDosyayaToolStripMenuItem });
		this.görünümüSaklaToolStripMenuItem.Name = "görünümüSaklaToolStripMenuItem";
		this.görünümüSaklaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.görünümüSaklaToolStripMenuItem.Text = "Görünümü sakla";
		this.otomatikDosyayaToolStripMenuItem.Name = "otomatikDosyayaToolStripMenuItem";
		this.otomatikDosyayaToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
		this.otomatikDosyayaToolStripMenuItem.Text = "Otomatik dosyaya";
		this.otomatikDosyayaToolStripMenuItem.Click += new System.EventHandler(otomatikDosyayaToolStripMenuItem_Click);
		this.farklıDosyayaToolStripMenuItem.Name = "farklıDosyayaToolStripMenuItem";
		this.farklıDosyayaToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
		this.farklıDosyayaToolStripMenuItem.Text = "Farklı dosyaya...";
		this.farklıDosyayaToolStripMenuItem.Click += new System.EventHandler(farklıDosyayaToolStripMenuItem_Click);
		this.görünümüYükleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.otomatikDosyadanToolStripMenuItem, this.farklıDosyadanToolStripMenuItem });
		this.görünümüYükleToolStripMenuItem.Name = "görünümüYükleToolStripMenuItem";
		this.görünümüYükleToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.görünümüYükleToolStripMenuItem.Text = "Görünümü yükle";
		this.otomatikDosyadanToolStripMenuItem.Name = "otomatikDosyadanToolStripMenuItem";
		this.otomatikDosyadanToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.otomatikDosyadanToolStripMenuItem.Text = "Otomatik dosyadan";
		this.otomatikDosyadanToolStripMenuItem.Click += new System.EventHandler(otomatikDosyadanToolStripMenuItem_Click);
		this.farklıDosyadanToolStripMenuItem.Name = "farklıDosyadanToolStripMenuItem";
		this.farklıDosyadanToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.farklıDosyadanToolStripMenuItem.Text = "Farklı dosyadan...";
		this.farklıDosyadanToolStripMenuItem.Click += new System.EventHandler(farklıDosyadanToolStripMenuItem_Click);
		this.otomatikDosyayıSilToolStripMenuItem.Name = "otomatikDosyayıSilToolStripMenuItem";
		this.otomatikDosyayıSilToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.otomatikDosyayıSilToolStripMenuItem.Text = "Otomatik dosyayı sil";
		this.otomatikDosyayıSilToolStripMenuItem.Click += new System.EventHandler(otomatikDosyayıSilToolStripMenuItem_Click);
		this.varsayılanaGeriDönToolStripMenuItem.Name = "varsayılanaGeriDönToolStripMenuItem";
		this.varsayılanaGeriDönToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.varsayılanaGeriDönToolStripMenuItem.Text = "Varsayılana geri dön";
		this.varsayılanaGeriDönToolStripMenuItem.Click += new System.EventHandler(varsayılanaGeriDönToolStripMenuItem_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1068, 460);
		base.Controls.Add(this.gc);
		base.Controls.Add(this.sb_stoklari_ekle);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "TopluStokEkleme";
		this.Text = "Toplu stok ekleme";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(TopluStokEkleme_FormClosing);
		base.Load += new System.EventHandler(TopluStokEkleme_Load);
		((System.ComponentModel.ISupportInitialize)this.gc).EndInit();
		((System.ComponentModel.ISupportInitialize)this.bindingSource1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.abgv).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
