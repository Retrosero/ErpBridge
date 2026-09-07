using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
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
using Fora.App.Win.Mikro.CariHesaplar;
using Fora.App.Win.Mikro.Classes;
using Fora.Mikro;
using Fora.Mikro.Bankalar;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Hizmetler;
using Fora.Mikro.Kurlar;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Utility;
using Fora.Mikro.Win.Form.DevEx.F10;
using fastJSON;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi;

[Guid("8497A9D8-C87A-4568-96CB-95E9EDC3EA79")]
public class TopluBankaEvrakGirisi : XtraForm
{
	private BankaEvrakDataSet _bankaevrakdataset;

	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _genelparametreler;

	private string _tag;

	private SqlDB Db;

	private Color RenkBeyaz = Color.White;

	private Color RenkYesil = Color.LightGreen;

	private Color RenkSari = Color.Yellow;

	private Color RenkKirmizi = Color.Salmon;

	private BandedGridColumn gc_satir_metin1;

	private BandedGridColumn gc_satir_metin2;

	private BandedGridColumn gc_satir_metin3;

	private BandedGridColumn gc_satir_metin4;

	private BandedGridColumn gc_satir_metin5;

	private BandedGridColumn gc_satir_metin6;

	private BandedGridColumn gc_satir_metin7;

	private BandedGridColumn gc_satir_metin8;

	private BandedGridColumn gc_satir_metin9;

	private BandedGridColumn gc_satir_metin10;

	private BandedGridColumn gc_satir_metin11;

	private BandedGridColumn gc_satir_metin12;

	private BandedGridColumn gc_satir_metin13;

	private BandedGridColumn gc_satir_metin14;

	private BandedGridColumn gc_satir_metin15;

	private BandedGridColumn gc_satir_metin16;

	private BandedGridColumn gc_satir_metin17;

	private BandedGridColumn gc_satir_metin18;

	private BandedGridColumn gc_satir_metin19;

	private BandedGridColumn gc_satir_metin20;

	private BandedGridColumn gc_satir_metin21;

	private BandedGridColumn gc_satir_metin22;

	private BandedGridColumn gc_satir_metin23;

	private BandedGridColumn gc_satir_metin24;

	private BandedGridColumn gc_satir_metin25;

	private BandedGridColumn gc_satir_metin26;

	private BandedGridColumn gc_satir_metin27;

	private BandedGridColumn gc_satir_metin28;

	private BandedGridColumn gc_satir_metin29;

	private BandedGridColumn gc_satir_metin30;

	private BandedGridColumn gc_satir_metin31;

	private BandedGridColumn gc_satir_metin32;

	private BandedGridColumn gc_satir_metin33;

	private BandedGridColumn gc_satir_metin34;

	private BandedGridColumn gc_satir_metin35;

	private BandedGridColumn gc_satir_metin36;

	private BandedGridColumn gc_satir_metin37;

	private BandedGridColumn gc_satir_metin38;

	private BandedGridColumn gc_satir_metin39;

	private BandedGridColumn gc_satir_metin40;

	private BandedGridColumn gc_satir_metin41;

	private BandedGridColumn gc_satir_metin42;

	private BandedGridColumn gc_satir_metin43;

	private BandedGridColumn gc_satir_metin44;

	private BandedGridColumn gc_satir_metin45;

	private BandedGridColumn gc_satir_metin46;

	private BandedGridColumn gc_satir_metin47;

	private BandedGridColumn gc_satir_metin48;

	private BandedGridColumn gc_satir_metin49;

	private BandedGridColumn gc_satir_metin50;

	private BandedGridColumn gc_satir_dropbox1;

	private BandedGridColumn gc_satir_dropbox2;

	private BandedGridColumn gc_satir_dropbox3;

	private BandedGridColumn gc_satir_dropbox4;

	private BandedGridColumn gc_satir_dropbox5;

	private BandedGridColumn gc_satir_dropbox6;

	private BandedGridColumn gc_satir_dropbox7;

	private BandedGridColumn gc_satir_dropbox8;

	private BandedGridColumn gc_satir_dropbox9;

	private BandedGridColumn gc_satir_dropbox10;

	private BandedGridColumn gc_satir_checkbox1;

	private BandedGridColumn gc_satir_checkbox2;

	private BandedGridColumn gc_satir_checkbox3;

	private BandedGridColumn gc_satir_checkbox4;

	private BandedGridColumn gc_satir_checkbox5;

	private BandedGridColumn gc_satir_checkbox6;

	private BandedGridColumn gc_satir_checkbox7;

	private BandedGridColumn gc_satir_checkbox8;

	private BandedGridColumn gc_satir_checkbox9;

	private BandedGridColumn gc_satir_checkbox10;

	private DevExpressRepositoryItemGridLookupEdit _RepositoryItems;

	private MemoryStream evraklar_defaultlayoutStream;

	private MemoryStream satirlar_defaultlayoutStream;

	private int AktarilacakEvrakSayisi;

	private bool KontrolSonucu = true;

	private string islemismi = "";

	private IContainer components;

	private SimpleButton sb_Secili_Evraklari_aktar;

	private MenuStrip menuStrip1;

	private BackgroundWorker backgroundWorker_Kontrol;

	private System.Windows.Forms.ProgressBar progressBar1;

	private Label label_islem_adi;

	private Label label_islem_bilgi;

	private BackgroundWorker backgroundWorker_Aktarim;

	private GridControl gridControl_evraklar;

	private AdvBandedGridView advBandedGridView_satirlar;

	private BandedGridColumn gc_satir_EvrakID;

	private BandedGridColumn gc_satir_Cinsi;

	private BandedGridColumn gc_satir_HesapKodu;

	private BandedGridColumn gc_satir_HesapAdi;

	private BandedGridColumn gc_satir_FaturaMiktar;

	private BandedGridColumn gc_satir_Tutar;

	private BandedGridColumn gc_satir_SatirAciklama;

	private AdvBandedGridView advBandedGridView_master;

	private BandedGridColumn gc_evrak_EvrakID;

	private BandedGridColumn gc_evrak_Aktar;

	private BandedGridColumn gc_evrak_AktarimDurumu;

	private BandedGridColumn gc_evrak_EvrakTipi;

	private BandedGridColumn gc_evrak_EvrakSeri;

	private BandedGridColumn gc_evrak_EvrakSira;

	private BandedGridColumn gc_evrak_Tarih;

	private BandedGridColumn gc_evrak_BelgeNo;

	private BandedGridColumn gc_evrak_BelgeTarihi;

	private BandedGridColumn gc_evrak_Kur;

	private BandedGridColumn gc_evrak_SorumlulukMerkeziKodu;

	private BandedGridColumn gc_evrak_special1;

	private BandedGridColumn gc_evrak_special2;

	private BandedGridColumn gc_evrak_special3;

	private BandedGridColumn gc_evrak_yekun;

	private BandedGridColumn gc_satir_SatirID;

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

	private ToolStripSeparator toolStripSeparator1;

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

	private ToolStripMenuItem satırlarToolStripMenuItem;

	private ToolStripMenuItem satirlar_kolonlaraGoreGruplamaToolStripMenuItem;

	private ToolStripMenuItem satirlar_kolonSeciciyiGosterToolStripMenuItem;

	private ToolStripMenuItem satirlar_butunGruplariAcToolStripMenuItem;

	private ToolStripMenuItem satirlar_butunGruplariKapatToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem görünümüSaklaToolStripMenuItem;

	private ToolStripMenuItem satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem;

	private ToolStripMenuItem satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1;

	private ToolStripMenuItem görünümüYükleToolStripMenuItem1;

	private ToolStripMenuItem satirlar_otomatikDosyadan_yukle_ToolStripMenuItem;

	private ToolStripMenuItem satirlar_farkliDosyadan_yukle_ToolStripMenuItem1;

	private ToolStripMenuItem satirlar_otomatikDosyayiSilToolStripMenuItem;

	private ToolStripMenuItem satirlar_varsayilanaGeriDonToolStripMenuItem;

	private RepositoryItemDateEdit repositoryItemDateEdit_EvrakTarih;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Evrak_DovizCinsi;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_AcikKapali;

	private GridView repositoryItemGridLookUpEdit1View;

	private ToolStripMenuItem aramaTablolariToolStripMenuItem;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_TicaretTuru;

	private GridView gridView_TicaretTuru;

	private ToolStripMenuItem parametrelerToolStripMenuItem;

	private ToolStripMenuItem varsayilanDegerlerToolStripMenuItem;

	private ToolStripMenuItem akisParametreleriToolStripMenuItem;

	private RepositoryItemTextEdit repositoryItemTextEdit_FaturaAciklama;

	private RepositoryItemTextEdit repositoryItemTextEdit_Aciklamalar;

	private BandedGridColumn gc_satir_FaturaDurumu;

	private BandedGridColumn gc_satir_FaturaSeri;

	private BandedGridColumn gc_satir_FaturaSira;

	private BandedGridColumn gc_satir_FaturaHesapKodu;

	private BandedGridColumn gc_satir_FaturaHesapAdi;

	private BandedGridColumn gc_satir_FaturaSorumlulukMerkeziKodu;

	private BandedGridColumn gc_satir_FaturaSorumlulukMerkeziAdi;

	private BandedGridColumn gc_satir_FaturaProjeKodu;

	private BandedGridColumn gc_satir_FaturaProjeAdi;

	private BandedGridColumn gc_satir_BV_Aciklama;

	private BandedGridColumn gc_satir_BV_IslemKodu;

	private BandedGridColumn gc_satir_BV_HesapNo;

	private BandedGridColumn gc_satir_BV_Unvan;

	private BandedGridColumn gc_satir_BV_Adres;

	private BandedGridColumn gc_satir_BV_Telefon;

	private BandedGridColumn gc_satir_BV_Eposta;

	private BandedGridColumn gc_satir_BV_TcVergiNo;

	private BandedGridColumn gc_satir_MV_TcVergiNo;

	private LabelControl labelControl1;

	private LabelControl labelControl2;

	private LabelControl lc_banka_adi;

	private LabelControl lc_banka_sube;

	private LabelControl labelControl5;

	private LabelControl lc_banka_kodu;

	private LabelControl lc_banka_hesapno;

	private LabelControl labelControl4;

	private LabelControl lc_mevcut_bakiye;

	private LabelControl labelControl6;

	private LabelControl lc_net_islem_tutari;

	private LabelControl labelControl8;

	private LabelControl lc_aktarim_sonrasi_bakiye;

	private LabelControl labelControl10;

	private LabelControl lc_dosya_tarih_araligi;

	private LabelControl labelControl12;

	private LabelControl lc_dosya_acilis_bakiyesi;

	private LabelControl labelControl14;

	private LabelControl lc_dosya_kapanis_bakiyesi;

	private LabelControl labelControl16;

	private BandedGridColumn gc_satir_BV_Unvan2;

	private BandedGridColumn gc_satir_BV_Mahalle;

	private BandedGridColumn gc_satir_BV_Ilce;

	private BandedGridColumn gc_satir_BV_PostaKodu;

	private BandedGridColumn gc_satir_BV_Il;

	private BandedGridColumn gc_satir_BV_Ulke;

	private BandedGridColumn gc_satir_MV_Unvan;

	private BandedGridColumn gc_satir_MV_Unvan2;

	private BandedGridColumn gc_satir_MV_HesapNo;

	private BandedGridColumn gc_satir_MV_Telefon;

	private BandedGridColumn gc_satir_MV_EPosta;

	private BandedGridColumn gc_satir_MV_Adres;

	private BandedGridColumn gc_satir_MV_Mahalle;

	private BandedGridColumn gc_satir_MV_Ilce;

	private BandedGridColumn gc_satir_MV_PostaKodu;

	private BandedGridColumn gc_satir_MV_Il;

	private BandedGridColumn gc_satir_MV_Ulke;

	private GridBand gridBand15;

	private GridBand gridBand9;

	private GridBand gridBand10;

	private GridBand gridBand11;

	private GridBand gridBand12;

	private GridBand gridBand5;

	private BandedGridColumn gc_evrak_SorumlulukMerkesiAdi;

	private GridBand gridBand7;

	private GridBand gridBand17;

	private RepositoryItemTextEdit repositoryItemTextEdit_specialalan;

	private RepositoryItemButtonEdit repositoryItemButtonEdit_Cari_Update;

	private BandedGridColumn gc_cari_ekle;

	private RepositoryItemButtonEdit repositoryItemButtonEdit_Cari_Ekle;

	private ToolStripMenuItem satırlarToolStripMenuItem1;

	private ToolStripMenuItem aktifSatirinHesapKodunuOnaylaToolStripMenuItem;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_Satir_Fatura_Durumu;

	private GridView gridView2;

	private RepositoryItemTextEdit repositoryItemTextEdit_SatirAciklama;

	private ToolStripMenuItem ustAlaniKopyalaToolStripMenuItem;

	private ToolStripMenuItem yeniEvrakEkleToolStripMenuItem1;

	private ToolStripMenuItem aramaTablolarininVerileriniGuncelleToolStripMenuItem;

	private BandedGridColumn gc_satir_GrupNo;

	private BandedGridColumn gc_satir_aktar;

	private ToolStripMenuItem satirlarinAktarimSeciminiTersCevirToolStripMenuItem;

	private GridBand gridBand27;

	private GridBand gridBand30;

	private GridBand gridBand2;

	private GridBand gridBand3;

	private GridBand gridBand1;

	private GridBand gridBand4;

	private GridBand gridBand6;

	public TopluBankaEvrakGirisi(BankaEvrakDataSet bankaevrakdataset, MikroUygulamaBilgileri mikrouygulamabilgileri, string tag, string baslik, bool OtomatikDosyaAc)
	{
		InitializeComponent();
		_bankaevrakdataset = bankaevrakdataset;
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
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
		gc_satir_metin1 = new BandedGridColumn();
		gc_satir_metin1.Caption = "Metin 1";
		gc_satir_metin1.FieldName = "metin1";
		gc_satir_metin1.Name = "gc_satir_metin1";
		gc_satir_metin1.Visible = true;
		gc_satir_metin1.Width = 100;
		gc_satir_metin2 = new BandedGridColumn();
		gc_satir_metin2.Caption = "Metin 2";
		gc_satir_metin2.FieldName = "metin2";
		gc_satir_metin2.Name = "gc_satir_metin2";
		gc_satir_metin2.Visible = true;
		gc_satir_metin2.Width = 100;
		gc_satir_metin3 = new BandedGridColumn();
		gc_satir_metin3.Caption = "Metin 3";
		gc_satir_metin3.FieldName = "metin3";
		gc_satir_metin3.Name = "gc_satir_metin3";
		gc_satir_metin3.Visible = true;
		gc_satir_metin3.Width = 100;
		gc_satir_metin4 = new BandedGridColumn();
		gc_satir_metin4.Caption = "Metin 4";
		gc_satir_metin4.FieldName = "metin4";
		gc_satir_metin4.Name = "gc_satir_metin4";
		gc_satir_metin4.Visible = true;
		gc_satir_metin4.Width = 100;
		gc_satir_metin5 = new BandedGridColumn();
		gc_satir_metin5.Caption = "Metin 5";
		gc_satir_metin5.FieldName = "metin5";
		gc_satir_metin5.Name = "gc_satir_metin5";
		gc_satir_metin5.Visible = true;
		gc_satir_metin5.Width = 100;
		gc_satir_metin6 = new BandedGridColumn();
		gc_satir_metin6.Caption = "Metin 6";
		gc_satir_metin6.FieldName = "metin6";
		gc_satir_metin6.Name = "gc_satir_metin6";
		gc_satir_metin6.Visible = true;
		gc_satir_metin6.Width = 100;
		gc_satir_metin7 = new BandedGridColumn();
		gc_satir_metin7.Caption = "Metin 7";
		gc_satir_metin7.FieldName = "metin7";
		gc_satir_metin7.Name = "gc_satir_metin7";
		gc_satir_metin7.Visible = true;
		gc_satir_metin7.Width = 100;
		gc_satir_metin8 = new BandedGridColumn();
		gc_satir_metin8.Caption = "Metin 8";
		gc_satir_metin8.FieldName = "metin8";
		gc_satir_metin8.Name = "gc_satir_metin8";
		gc_satir_metin8.Visible = true;
		gc_satir_metin8.Width = 100;
		gc_satir_metin9 = new BandedGridColumn();
		gc_satir_metin9.Caption = "Metin 9";
		gc_satir_metin9.FieldName = "metin9";
		gc_satir_metin9.Name = "gc_satir_metin9";
		gc_satir_metin9.Visible = true;
		gc_satir_metin9.Width = 100;
		gc_satir_metin10 = new BandedGridColumn();
		gc_satir_metin10.Caption = "Metin 10";
		gc_satir_metin10.FieldName = "metin10";
		gc_satir_metin10.Name = "gc_satir_metin10";
		gc_satir_metin10.Visible = true;
		gc_satir_metin10.Width = 100;
		gc_satir_metin11 = new BandedGridColumn();
		gc_satir_metin11.Caption = "Metin 11";
		gc_satir_metin11.FieldName = "metin11";
		gc_satir_metin11.Name = "gc_satir_metin11";
		gc_satir_metin11.Visible = true;
		gc_satir_metin11.Width = 100;
		gc_satir_metin12 = new BandedGridColumn();
		gc_satir_metin12.Caption = "Metin 12";
		gc_satir_metin12.FieldName = "metin12";
		gc_satir_metin12.Name = "gc_satir_metin12";
		gc_satir_metin12.Visible = true;
		gc_satir_metin12.Width = 100;
		gc_satir_metin13 = new BandedGridColumn();
		gc_satir_metin13.Caption = "Metin 13";
		gc_satir_metin13.FieldName = "metin13";
		gc_satir_metin13.Name = "gc_satir_metin13";
		gc_satir_metin13.Visible = true;
		gc_satir_metin13.Width = 100;
		gc_satir_metin14 = new BandedGridColumn();
		gc_satir_metin14.Caption = "Metin 14";
		gc_satir_metin14.FieldName = "metin14";
		gc_satir_metin14.Name = "gc_satir_metin14";
		gc_satir_metin14.Visible = true;
		gc_satir_metin14.Width = 100;
		gc_satir_metin15 = new BandedGridColumn();
		gc_satir_metin15.Caption = "Metin 15";
		gc_satir_metin15.FieldName = "metin15";
		gc_satir_metin15.Name = "gc_satir_metin15";
		gc_satir_metin15.Visible = true;
		gc_satir_metin15.Width = 100;
		gc_satir_metin16 = new BandedGridColumn();
		gc_satir_metin16.Caption = "Metin 16";
		gc_satir_metin16.FieldName = "metin16";
		gc_satir_metin16.Name = "gc_satir_metin16";
		gc_satir_metin16.Visible = true;
		gc_satir_metin16.Width = 100;
		gc_satir_metin17 = new BandedGridColumn();
		gc_satir_metin17.Caption = "Metin 17";
		gc_satir_metin17.FieldName = "metin17";
		gc_satir_metin17.Name = "gc_satir_metin17";
		gc_satir_metin17.Visible = true;
		gc_satir_metin17.Width = 100;
		gc_satir_metin18 = new BandedGridColumn();
		gc_satir_metin18.Caption = "Metin 18";
		gc_satir_metin18.FieldName = "metin18";
		gc_satir_metin18.Name = "gc_satir_metin18";
		gc_satir_metin18.Visible = true;
		gc_satir_metin18.Width = 100;
		gc_satir_metin19 = new BandedGridColumn();
		gc_satir_metin19.Caption = "Metin 19";
		gc_satir_metin19.FieldName = "metin19";
		gc_satir_metin19.Name = "gc_satir_metin19";
		gc_satir_metin19.Visible = true;
		gc_satir_metin19.Width = 100;
		gc_satir_metin20 = new BandedGridColumn();
		gc_satir_metin20.Caption = "Metin 20";
		gc_satir_metin20.FieldName = "metin20";
		gc_satir_metin20.Name = "gc_satir_metin20";
		gc_satir_metin20.Visible = true;
		gc_satir_metin20.Width = 100;
		gc_satir_metin21 = new BandedGridColumn();
		gc_satir_metin21.Caption = "Metin 21";
		gc_satir_metin21.FieldName = "metin21";
		gc_satir_metin21.Name = "gc_satir_metin21";
		gc_satir_metin21.Visible = true;
		gc_satir_metin21.Width = 100;
		gc_satir_metin22 = new BandedGridColumn();
		gc_satir_metin22.Caption = "Metin 22";
		gc_satir_metin22.FieldName = "metin22";
		gc_satir_metin22.Name = "gc_satir_metin22";
		gc_satir_metin22.Visible = true;
		gc_satir_metin22.Width = 100;
		gc_satir_metin23 = new BandedGridColumn();
		gc_satir_metin23.Caption = "Metin 23";
		gc_satir_metin23.FieldName = "metin23";
		gc_satir_metin23.Name = "gc_satir_metin23";
		gc_satir_metin23.Visible = true;
		gc_satir_metin23.Width = 100;
		gc_satir_metin24 = new BandedGridColumn();
		gc_satir_metin24.Caption = "Metin 24";
		gc_satir_metin24.FieldName = "metin24";
		gc_satir_metin24.Name = "gc_satir_metin24";
		gc_satir_metin24.Visible = true;
		gc_satir_metin24.Width = 100;
		gc_satir_metin25 = new BandedGridColumn();
		gc_satir_metin25.Caption = "Metin 25";
		gc_satir_metin25.FieldName = "metin25";
		gc_satir_metin25.Name = "gc_satir_metin25";
		gc_satir_metin25.Visible = true;
		gc_satir_metin25.Width = 100;
		gc_satir_metin26 = new BandedGridColumn();
		gc_satir_metin26.Caption = "Metin 26";
		gc_satir_metin26.FieldName = "metin26";
		gc_satir_metin26.Name = "gc_satir_metin26";
		gc_satir_metin26.Visible = true;
		gc_satir_metin26.Width = 100;
		gc_satir_metin27 = new BandedGridColumn();
		gc_satir_metin27.Caption = "Metin 27";
		gc_satir_metin27.FieldName = "metin27";
		gc_satir_metin27.Name = "gc_satir_metin27";
		gc_satir_metin27.Visible = true;
		gc_satir_metin27.Width = 100;
		gc_satir_metin28 = new BandedGridColumn();
		gc_satir_metin28.Caption = "Metin 28";
		gc_satir_metin28.FieldName = "metin28";
		gc_satir_metin28.Name = "gc_satir_metin28";
		gc_satir_metin28.Visible = true;
		gc_satir_metin28.Width = 100;
		gc_satir_metin29 = new BandedGridColumn();
		gc_satir_metin29.Caption = "Metin 29";
		gc_satir_metin29.FieldName = "metin29";
		gc_satir_metin29.Name = "gc_satir_metin29";
		gc_satir_metin29.Visible = true;
		gc_satir_metin29.Width = 100;
		gc_satir_metin30 = new BandedGridColumn();
		gc_satir_metin30.Caption = "Metin 30";
		gc_satir_metin30.FieldName = "metin30";
		gc_satir_metin30.Name = "gc_satir_metin30";
		gc_satir_metin30.Visible = true;
		gc_satir_metin30.Width = 100;
		gc_satir_metin31 = new BandedGridColumn();
		gc_satir_metin31.Caption = "Metin 31";
		gc_satir_metin31.FieldName = "metin31";
		gc_satir_metin31.Name = "gc_satir_metin31";
		gc_satir_metin31.Visible = true;
		gc_satir_metin31.Width = 100;
		gc_satir_metin32 = new BandedGridColumn();
		gc_satir_metin32.Caption = "Metin 32";
		gc_satir_metin32.FieldName = "metin32";
		gc_satir_metin32.Name = "gc_satir_metin32";
		gc_satir_metin32.Visible = true;
		gc_satir_metin32.Width = 100;
		gc_satir_metin33 = new BandedGridColumn();
		gc_satir_metin33.Caption = "Metin 33";
		gc_satir_metin33.FieldName = "metin33";
		gc_satir_metin33.Name = "gc_satir_metin33";
		gc_satir_metin33.Visible = true;
		gc_satir_metin33.Width = 100;
		gc_satir_metin34 = new BandedGridColumn();
		gc_satir_metin34.Caption = "Metin 34";
		gc_satir_metin34.FieldName = "metin34";
		gc_satir_metin34.Name = "gc_satir_metin34";
		gc_satir_metin34.Visible = true;
		gc_satir_metin34.Width = 100;
		gc_satir_metin35 = new BandedGridColumn();
		gc_satir_metin35.Caption = "Metin 35";
		gc_satir_metin35.FieldName = "metin35";
		gc_satir_metin35.Name = "gc_satir_metin35";
		gc_satir_metin35.Visible = true;
		gc_satir_metin35.Width = 100;
		gc_satir_metin36 = new BandedGridColumn();
		gc_satir_metin36.Caption = "Metin 36";
		gc_satir_metin36.FieldName = "metin36";
		gc_satir_metin36.Name = "gc_satir_metin36";
		gc_satir_metin36.Visible = true;
		gc_satir_metin36.Width = 100;
		gc_satir_metin37 = new BandedGridColumn();
		gc_satir_metin37.Caption = "Metin 37";
		gc_satir_metin37.FieldName = "metin37";
		gc_satir_metin37.Name = "gc_satir_metin37";
		gc_satir_metin37.Visible = true;
		gc_satir_metin37.Width = 100;
		gc_satir_metin38 = new BandedGridColumn();
		gc_satir_metin38.Caption = "Metin 38";
		gc_satir_metin38.FieldName = "metin38";
		gc_satir_metin38.Name = "gc_satir_metin38";
		gc_satir_metin38.Visible = true;
		gc_satir_metin38.Width = 100;
		gc_satir_metin39 = new BandedGridColumn();
		gc_satir_metin39.Caption = "Metin 39";
		gc_satir_metin39.FieldName = "metin39";
		gc_satir_metin39.Name = "gc_satir_metin39";
		gc_satir_metin39.Visible = true;
		gc_satir_metin39.Width = 100;
		gc_satir_metin40 = new BandedGridColumn();
		gc_satir_metin40.Caption = "Metin 40";
		gc_satir_metin40.FieldName = "metin40";
		gc_satir_metin40.Name = "gc_satir_metin40";
		gc_satir_metin40.Visible = true;
		gc_satir_metin40.Width = 100;
		gc_satir_metin41 = new BandedGridColumn();
		gc_satir_metin41.Caption = "Metin 41";
		gc_satir_metin41.FieldName = "metin41";
		gc_satir_metin41.Name = "gc_satir_metin41";
		gc_satir_metin41.Visible = true;
		gc_satir_metin41.Width = 100;
		gc_satir_metin42 = new BandedGridColumn();
		gc_satir_metin42.Caption = "Metin 42";
		gc_satir_metin42.FieldName = "metin42";
		gc_satir_metin42.Name = "gc_satir_metin42";
		gc_satir_metin42.Visible = true;
		gc_satir_metin42.Width = 100;
		gc_satir_metin43 = new BandedGridColumn();
		gc_satir_metin43.Caption = "Metin 43";
		gc_satir_metin43.FieldName = "metin43";
		gc_satir_metin43.Name = "gc_satir_metin43";
		gc_satir_metin43.Visible = true;
		gc_satir_metin43.Width = 100;
		gc_satir_metin44 = new BandedGridColumn();
		gc_satir_metin44.Caption = "Metin 44";
		gc_satir_metin44.FieldName = "metin44";
		gc_satir_metin44.Name = "gc_satir_metin44";
		gc_satir_metin44.Visible = true;
		gc_satir_metin44.Width = 100;
		gc_satir_metin45 = new BandedGridColumn();
		gc_satir_metin45.Caption = "Metin 45";
		gc_satir_metin45.FieldName = "metin45";
		gc_satir_metin45.Name = "gc_satir_metin45";
		gc_satir_metin45.Visible = true;
		gc_satir_metin45.Width = 100;
		gc_satir_metin46 = new BandedGridColumn();
		gc_satir_metin46.Caption = "Metin 46";
		gc_satir_metin46.FieldName = "metin46";
		gc_satir_metin46.Name = "gc_satir_metin46";
		gc_satir_metin46.Visible = true;
		gc_satir_metin46.Width = 100;
		gc_satir_metin47 = new BandedGridColumn();
		gc_satir_metin47.Caption = "Metin 47";
		gc_satir_metin47.FieldName = "metin47";
		gc_satir_metin47.Name = "gc_satir_metin47";
		gc_satir_metin47.Visible = true;
		gc_satir_metin47.Width = 100;
		gc_satir_metin48 = new BandedGridColumn();
		gc_satir_metin48.Caption = "Metin 48";
		gc_satir_metin48.FieldName = "metin48";
		gc_satir_metin48.Name = "gc_satir_metin48";
		gc_satir_metin48.Visible = true;
		gc_satir_metin48.Width = 100;
		gc_satir_metin49 = new BandedGridColumn();
		gc_satir_metin49.Caption = "Metin 49";
		gc_satir_metin49.FieldName = "metin49";
		gc_satir_metin49.Name = "gc_satir_metin49";
		gc_satir_metin49.Visible = true;
		gc_satir_metin49.Width = 100;
		gc_satir_metin50 = new BandedGridColumn();
		gc_satir_metin50.Caption = "Metin 50";
		gc_satir_metin50.FieldName = "metin50";
		gc_satir_metin50.Name = "gc_satir_metin50";
		gc_satir_metin50.Visible = true;
		gc_satir_metin50.Width = 100;
		gc_satir_dropbox1 = new BandedGridColumn();
		gc_satir_dropbox1.Caption = "Açılır kutu 1";
		gc_satir_dropbox1.FieldName = "dropbox1";
		gc_satir_dropbox1.Name = "gc_satir_dropbox1";
		gc_satir_dropbox1.Visible = true;
		gc_satir_dropbox1.Width = 100;
		gc_satir_dropbox2 = new BandedGridColumn();
		gc_satir_dropbox2.Caption = "Açılır kutu 2";
		gc_satir_dropbox2.FieldName = "dropbox2";
		gc_satir_dropbox2.Name = "gc_satir_dropbox2";
		gc_satir_dropbox2.Visible = true;
		gc_satir_dropbox2.Width = 100;
		gc_satir_dropbox3 = new BandedGridColumn();
		gc_satir_dropbox3.Caption = "Açılır kutu 3";
		gc_satir_dropbox3.FieldName = "dropbox3";
		gc_satir_dropbox3.Name = "gc_satir_dropbox3";
		gc_satir_dropbox3.Visible = true;
		gc_satir_dropbox3.Width = 100;
		gc_satir_dropbox4 = new BandedGridColumn();
		gc_satir_dropbox4.Caption = "Açılır kutu 4";
		gc_satir_dropbox4.FieldName = "dropbox4";
		gc_satir_dropbox4.Name = "gc_satir_dropbox4";
		gc_satir_dropbox4.Visible = true;
		gc_satir_dropbox4.Width = 100;
		gc_satir_dropbox5 = new BandedGridColumn();
		gc_satir_dropbox5.Caption = "Açılır kutu 5";
		gc_satir_dropbox5.FieldName = "dropbox5";
		gc_satir_dropbox5.Name = "gc_satir_dropbox5";
		gc_satir_dropbox5.Visible = true;
		gc_satir_dropbox5.Width = 100;
		gc_satir_dropbox6 = new BandedGridColumn();
		gc_satir_dropbox6.Caption = "Açılır kutu 6";
		gc_satir_dropbox6.FieldName = "dropbox6";
		gc_satir_dropbox6.Name = "gc_satir_dropbox6";
		gc_satir_dropbox6.Visible = true;
		gc_satir_dropbox6.Width = 100;
		gc_satir_dropbox7 = new BandedGridColumn();
		gc_satir_dropbox7.Caption = "Açılır kutu 7";
		gc_satir_dropbox7.FieldName = "dropbox7";
		gc_satir_dropbox7.Name = "gc_satir_dropbox7";
		gc_satir_dropbox7.Visible = true;
		gc_satir_dropbox7.Width = 100;
		gc_satir_dropbox8 = new BandedGridColumn();
		gc_satir_dropbox8.Caption = "Açılır kutu 8";
		gc_satir_dropbox8.FieldName = "dropbox8";
		gc_satir_dropbox8.Name = "gc_satir_dropbox8";
		gc_satir_dropbox8.Visible = true;
		gc_satir_dropbox8.Width = 100;
		gc_satir_dropbox9 = new BandedGridColumn();
		gc_satir_dropbox9.Caption = "Açılır kutu 9";
		gc_satir_dropbox9.FieldName = "dropbox9";
		gc_satir_dropbox9.Name = "gc_satir_dropbox9";
		gc_satir_dropbox9.Visible = true;
		gc_satir_dropbox9.Width = 100;
		gc_satir_dropbox10 = new BandedGridColumn();
		gc_satir_dropbox10.Caption = "Açılır kutu 10";
		gc_satir_dropbox10.FieldName = "dropbox10";
		gc_satir_dropbox10.Name = "gc_satir_dropbox10";
		gc_satir_dropbox10.Visible = true;
		gc_satir_dropbox10.Width = 100;
		gc_satir_checkbox1 = new BandedGridColumn();
		gc_satir_checkbox1.Caption = "Onay kutusu 1";
		gc_satir_checkbox1.FieldName = "checkbox1";
		gc_satir_checkbox1.Name = "gc_satir_checkbox1";
		gc_satir_checkbox1.Visible = true;
		gc_satir_checkbox1.Width = 100;
		gc_satir_checkbox2 = new BandedGridColumn();
		gc_satir_checkbox2.Caption = "Onay kutusu 2";
		gc_satir_checkbox2.FieldName = "checkbox2";
		gc_satir_checkbox2.Name = "gc_satir_checkbox2";
		gc_satir_checkbox2.Visible = true;
		gc_satir_checkbox2.Width = 100;
		gc_satir_checkbox3 = new BandedGridColumn();
		gc_satir_checkbox3.Caption = "Onay kutusu 3";
		gc_satir_checkbox3.FieldName = "checkbox3";
		gc_satir_checkbox3.Name = "gc_satir_checkbox3";
		gc_satir_checkbox3.Visible = true;
		gc_satir_checkbox3.Width = 100;
		gc_satir_checkbox4 = new BandedGridColumn();
		gc_satir_checkbox4.Caption = "Onay kutusu 4";
		gc_satir_checkbox4.FieldName = "checkbox4";
		gc_satir_checkbox4.Name = "gc_satir_checkbox4";
		gc_satir_checkbox4.Visible = true;
		gc_satir_checkbox4.Width = 100;
		gc_satir_checkbox5 = new BandedGridColumn();
		gc_satir_checkbox5.Caption = "Onay kutusu 5";
		gc_satir_checkbox5.FieldName = "checkbox5";
		gc_satir_checkbox5.Name = "gc_satir_checkbox5";
		gc_satir_checkbox5.Visible = true;
		gc_satir_checkbox5.Width = 100;
		gc_satir_checkbox6 = new BandedGridColumn();
		gc_satir_checkbox6.Caption = "Onay kutusu 6";
		gc_satir_checkbox6.FieldName = "checkbox6";
		gc_satir_checkbox6.Name = "gc_satir_checkbox6";
		gc_satir_checkbox6.Visible = true;
		gc_satir_checkbox6.Width = 100;
		gc_satir_checkbox7 = new BandedGridColumn();
		gc_satir_checkbox7.Caption = "Onay kutusu 7";
		gc_satir_checkbox7.FieldName = "checkbox7";
		gc_satir_checkbox7.Name = "gc_satir_checkbox7";
		gc_satir_checkbox7.Visible = true;
		gc_satir_checkbox7.Width = 100;
		gc_satir_checkbox8 = new BandedGridColumn();
		gc_satir_checkbox8.Caption = "Onay kutusu 8";
		gc_satir_checkbox8.FieldName = "checkbox8";
		gc_satir_checkbox8.Name = "gc_satir_checkbox8";
		gc_satir_checkbox8.Visible = true;
		gc_satir_checkbox8.Width = 100;
		gc_satir_checkbox9 = new BandedGridColumn();
		gc_satir_checkbox9.Caption = "Onay kutusu 9";
		gc_satir_checkbox9.FieldName = "checkbox9";
		gc_satir_checkbox9.Name = "gc_satir_checkbox9";
		gc_satir_checkbox9.Visible = true;
		gc_satir_checkbox9.Width = 100;
		gc_satir_checkbox10 = new BandedGridColumn();
		gc_satir_checkbox10.Caption = "Onay kutusu 10";
		gc_satir_checkbox10.FieldName = "checkbox10";
		gc_satir_checkbox10.Name = "gc_satir_checkbox10";
		gc_satir_checkbox10.Visible = true;
		gc_satir_checkbox10.Width = 100;
		advBandedGridView_satirlar.Columns.AddRange(new BandedGridColumn[70]
		{
			gc_satir_metin1, gc_satir_metin2, gc_satir_metin3, gc_satir_metin4, gc_satir_metin5, gc_satir_metin6, gc_satir_metin7, gc_satir_metin8, gc_satir_metin9, gc_satir_metin10,
			gc_satir_metin11, gc_satir_metin12, gc_satir_metin13, gc_satir_metin14, gc_satir_metin15, gc_satir_metin16, gc_satir_metin17, gc_satir_metin18, gc_satir_metin19, gc_satir_metin20,
			gc_satir_metin21, gc_satir_metin22, gc_satir_metin23, gc_satir_metin24, gc_satir_metin25, gc_satir_metin26, gc_satir_metin27, gc_satir_metin28, gc_satir_metin29, gc_satir_metin30,
			gc_satir_metin31, gc_satir_metin32, gc_satir_metin33, gc_satir_metin34, gc_satir_metin35, gc_satir_metin36, gc_satir_metin37, gc_satir_metin38, gc_satir_metin39, gc_satir_metin40,
			gc_satir_metin41, gc_satir_metin42, gc_satir_metin43, gc_satir_metin44, gc_satir_metin45, gc_satir_metin46, gc_satir_metin47, gc_satir_metin48, gc_satir_metin49, gc_satir_metin50,
			gc_satir_dropbox1, gc_satir_dropbox2, gc_satir_dropbox3, gc_satir_dropbox4, gc_satir_dropbox5, gc_satir_dropbox6, gc_satir_dropbox7, gc_satir_dropbox8, gc_satir_dropbox9, gc_satir_dropbox10,
			gc_satir_checkbox1, gc_satir_checkbox2, gc_satir_checkbox3, gc_satir_checkbox4, gc_satir_checkbox5, gc_satir_checkbox6, gc_satir_checkbox7, gc_satir_checkbox8, gc_satir_checkbox9, gc_satir_checkbox10
		});
		gridBand6.Columns.Add(gc_satir_metin1);
		gridBand6.Columns.Add(gc_satir_metin2);
		gridBand6.Columns.Add(gc_satir_metin3);
		gridBand6.Columns.Add(gc_satir_metin4);
		gridBand6.Columns.Add(gc_satir_metin5);
		gridBand6.Columns.Add(gc_satir_metin6);
		gridBand6.Columns.Add(gc_satir_metin7);
		gridBand6.Columns.Add(gc_satir_metin8);
		gridBand6.Columns.Add(gc_satir_metin9);
		gridBand6.Columns.Add(gc_satir_metin10);
		gridBand6.Columns.Add(gc_satir_metin11);
		gridBand6.Columns.Add(gc_satir_metin12);
		gridBand6.Columns.Add(gc_satir_metin13);
		gridBand6.Columns.Add(gc_satir_metin14);
		gridBand6.Columns.Add(gc_satir_metin15);
		gridBand6.Columns.Add(gc_satir_metin16);
		gridBand6.Columns.Add(gc_satir_metin17);
		gridBand6.Columns.Add(gc_satir_metin18);
		gridBand6.Columns.Add(gc_satir_metin19);
		gridBand6.Columns.Add(gc_satir_metin20);
		gridBand6.Columns.Add(gc_satir_metin21);
		gridBand6.Columns.Add(gc_satir_metin22);
		gridBand6.Columns.Add(gc_satir_metin23);
		gridBand6.Columns.Add(gc_satir_metin24);
		gridBand6.Columns.Add(gc_satir_metin25);
		gridBand6.Columns.Add(gc_satir_metin26);
		gridBand6.Columns.Add(gc_satir_metin27);
		gridBand6.Columns.Add(gc_satir_metin28);
		gridBand6.Columns.Add(gc_satir_metin29);
		gridBand6.Columns.Add(gc_satir_metin30);
		gridBand6.Columns.Add(gc_satir_metin31);
		gridBand6.Columns.Add(gc_satir_metin32);
		gridBand6.Columns.Add(gc_satir_metin33);
		gridBand6.Columns.Add(gc_satir_metin34);
		gridBand6.Columns.Add(gc_satir_metin35);
		gridBand6.Columns.Add(gc_satir_metin36);
		gridBand6.Columns.Add(gc_satir_metin37);
		gridBand6.Columns.Add(gc_satir_metin38);
		gridBand6.Columns.Add(gc_satir_metin39);
		gridBand6.Columns.Add(gc_satir_metin40);
		gridBand6.Columns.Add(gc_satir_metin41);
		gridBand6.Columns.Add(gc_satir_metin42);
		gridBand6.Columns.Add(gc_satir_metin43);
		gridBand6.Columns.Add(gc_satir_metin44);
		gridBand6.Columns.Add(gc_satir_metin45);
		gridBand6.Columns.Add(gc_satir_metin46);
		gridBand6.Columns.Add(gc_satir_metin47);
		gridBand6.Columns.Add(gc_satir_metin48);
		gridBand6.Columns.Add(gc_satir_metin49);
		gridBand6.Columns.Add(gc_satir_metin50);
		gridBand6.Columns.Add(gc_satir_dropbox1);
		gridBand6.Columns.Add(gc_satir_dropbox2);
		gridBand6.Columns.Add(gc_satir_dropbox3);
		gridBand6.Columns.Add(gc_satir_dropbox4);
		gridBand6.Columns.Add(gc_satir_dropbox5);
		gridBand6.Columns.Add(gc_satir_dropbox6);
		gridBand6.Columns.Add(gc_satir_dropbox7);
		gridBand6.Columns.Add(gc_satir_dropbox8);
		gridBand6.Columns.Add(gc_satir_dropbox9);
		gridBand6.Columns.Add(gc_satir_dropbox10);
		gridBand6.Columns.Add(gc_satir_checkbox1);
		gridBand6.Columns.Add(gc_satir_checkbox2);
		gridBand6.Columns.Add(gc_satir_checkbox3);
		gridBand6.Columns.Add(gc_satir_checkbox4);
		gridBand6.Columns.Add(gc_satir_checkbox5);
		gridBand6.Columns.Add(gc_satir_checkbox6);
		gridBand6.Columns.Add(gc_satir_checkbox7);
		gridBand6.Columns.Add(gc_satir_checkbox8);
		gridBand6.Columns.Add(gc_satir_checkbox9);
		gridBand6.Columns.Add(gc_satir_checkbox10);
		if (OtomatikDosyaAc)
		{
			DosyaAc();
		}
		else
		{
			mikri_Disi_Ek_Veriler_Baslik_Ayarla();
		}
	}

	private void EvrakDuzenleme_Load(object sender, EventArgs e)
	{
		advBandedGridView_master.Appearance.SelectedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		advBandedGridView_satirlar.Appearance.SelectedRow.BackColor = Color.FromArgb(50, 0, 0, 0);
		evraklar_defaultlayoutStream = new MemoryStream();
		advBandedGridView_master.SaveLayoutToStream(evraklar_defaultlayoutStream);
		satirlar_defaultlayoutStream = new MemoryStream();
		advBandedGridView_satirlar.SaveLayoutToStream(satirlar_defaultlayoutStream);
		if (File.Exists("data\\views\\" + _tag + ".lgm"))
		{
			advBandedGridView_master.RestoreLayoutFromXml("data\\views\\" + _tag + ".lgm");
		}
		if (File.Exists("data\\views\\" + _tag + ".lgs"))
		{
			advBandedGridView_satirlar.RestoreLayoutFromXml("data\\views\\" + _tag + ".lgs");
		}
		RepositoryItemAyarla();
		bindData();
		EkranGuncelle();
	}

	private void EkranGuncelle()
	{
		lc_banka_kodu.Text = _bankaevrakdataset.banka.ban_kod;
		lc_banka_adi.Text = _bankaevrakdataset.banka.ban_ismi;
		lc_banka_sube.Text = _bankaevrakdataset.banka.ban_sube;
		lc_banka_hesapno.Text = _bankaevrakdataset.banka.ban_hesapno;
		lc_dosya_tarih_araligi.Text = _bankaevrakdataset._AktarimDosyasiIlkTarih.ToString("dd.MM.yyyy") + "-" + _bankaevrakdataset._AktarimDosyasiSonTarih.ToString("dd.MM.yyyy");
		lc_dosya_acilis_bakiyesi.Text = _bankaevrakdataset._AktarimDosyasiAcilisBakiyesi.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_bankaevrakdataset.banka.ban_doviz_cinsi).Kur_sembol;
		lc_dosya_kapanis_bakiyesi.Text = _bankaevrakdataset._AktarimDosyasiKapanisBakiyesi.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_bankaevrakdataset.banka.ban_doviz_cinsi).Kur_sembol;
		NetIslemTutariveAktarimSonrasiBakiyeAyarla();
	}

	private void NetIslemTutariveAktarimSonrasiBakiyeAyarla()
	{
		Console.WriteLine("NET ISLEM TUTARI HESAPLANIYOR");
		double num = 0.0;
		foreach (DataRow row in _bankaevrakdataset.dataset.Tables[0].Rows)
		{
			if ((bool)row[1])
			{
				num += _bankaevrakdataset.GetEvrakYekun(row);
			}
		}
		double num2 = 0.0;
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			string commandText = "SELECT dbo.fn_CariHesapOrjinalDovizBakiye(@firmano,2,@bankakodu,'','',1,NULL,@sontarih,0)";
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@firmano", _bankaevrakdataset.banka.ban_firma_no);
				sqlCommand.Parameters.AddWithValue("@bankakodu", _bankaevrakdataset.banka.ban_kod);
				sqlCommand.Parameters.AddWithValue("@sontarih", _bankaevrakdataset._AktarimDosyasiIlkTarih);
				num2 = double.Parse(sqlCommand.ExecuteScalar().ToString());
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		lc_mevcut_bakiye.Text = num2.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_bankaevrakdataset.banka.ban_doviz_cinsi).Kur_sembol;
		lc_net_islem_tutari.Text = num.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_bankaevrakdataset.banka.ban_doviz_cinsi).Kur_sembol;
		lc_aktarim_sonrasi_bakiye.Text = (num2 + num).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_bankaevrakdataset.banka.ban_doviz_cinsi).Kur_sembol;
	}

	private void bindData()
	{
		BindingSource bindingSource = new BindingSource();
		bindingSource.DataSource = _bankaevrakdataset.dataset;
		bindingSource.DataMember = _bankaevrakdataset.dataset.Tables[0].TableName;
		gridControl_evraklar.DataSource = bindingSource;
	}

	private void GenelParametreTanimlaveOku()
	{
		_genelparametreler = new Parametreler();
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 1, "mt940IBANCariAra", "1"));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 2, "mt940IBANCariPersoneldeAra", "1"));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 3, "mt940IBANPersoneldeAra", "1"));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 4, "ozeldosyadaunvaniara", "1"));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 5, "ozeldosyadatckimliknoyuara", "1"));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 6, "ozeldosyadatelefonnoyuara", "1"));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 7, "specialalan1", ""));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 8, "specialalan2", ""));
		_genelparametreler.ParametreListesi.Add(new Parametre(_tag, "", "GenelParametreler", "", 9, "specialalan3", ""));
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _genelparametreler, _tag, "", "GenelParametreler", "");
	}

	private void TopluGenelEvrakGirisi_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.DialogResult != DialogResult.OK && MessageBox.Show("Çıkmak istediğinize emin misiniz?", "Onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
		{
			e.Cancel = true;
		}
	}

	private void RepositoryItemlariGuncelle()
	{
		DataSet lookupTablolar = GenelData.GetLookupTablolar(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		foreach (DevExpressRepositoryItemGridLookupEditExtended item in _RepositoryItems.Items)
		{
			item.DataGuncelle(lookupTablolar);
		}
	}

	private void RepositoryItemAyarla()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(int));
		dataTable.Columns.Add("Isim", typeof(string));
		dataTable.Rows.Add(11, "Gelen Havale");
		dataTable.Rows.Add(12, "Giden Havale");
		dataTable.Rows.Add(29, "Bankalar Arası Virman Dekontu");
		dataTable.Rows.Add(18, "Bankadan Kasaya Nakit Çekme Makbuzu");
		dataTable.Rows.Add(19, "Kasadan Bankaya Nakit Yatırma Makbuzu");
		dataTable.Rows.Add(14, "Tahsildeki Çek Ödeme Bordrosu");
		dataTable.Rows.Add(17, "Tahsile Çek Çıkış Bordrosu");
		dataTable.Rows.Add(20, "Tahsile Senet Çıkış Bordrosu");
		dataTable.Rows.Add(21, "Tahsildeki Senet Ödeme Bordrosu");
		dataTable.Rows.Add(22, "Verilen Firma Çeki Ödeme Bordrosu");
		dataTable.Rows.Add(23, "Verilen Firma Senedi Ödeme Bordrosu");
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
		dataTable2.Rows.Add(0, "Cari Hesap");
		dataTable2.Rows.Add(1, "Cari Personel");
		dataTable2.Rows.Add(2, "Banka");
		dataTable2.Rows.Add(3, "Hizmet");
		dataTable2.Rows.Add(4, "Kasa");
		dataTable2.Rows.Add(5, "Masraf");
		dataTable2.Rows.Add(6, "Muh. Hesabı");
		dataTable2.Rows.Add(7, "Personel");
		dataTable2.Rows.Add(8, "Demirbaş");
		dataTable2.Rows.Add(9, "İthalat");
		dataTable2.Rows.Add(10, "Müşteri Çeki");
		dataTable2.Rows.Add(11, "Müşteri Seneti");
		dataTable2.Rows.Add(12, "Firma Çeki");
		dataTable2.Rows.Add(13, "Firma Seneti");
		repositoryItemGridLookUpEdit_Satir_Cinsi.DataSource = dataTable2;
		repositoryItemGridLookUpEdit_Satir_Cinsi.ValueMember = "ID";
		repositoryItemGridLookUpEdit_Satir_Cinsi.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_Satir_Cinsi.PopulateViewColumns();
		foreach (GridColumn column2 in repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns)
		{
			column2.Visible = false;
		}
		repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns[1].Caption = "Cinsi";
		repositoryItemGridLookUpEdit_Satir_Cinsi.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_Satir_Cinsi.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_Satir_Cinsi.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_Satir_Cinsi.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_Satir_Cinsi.AutoComplete = false;
		DataTable dataTable3 = new DataTable();
		dataTable3.Columns.Add("ID", typeof(int));
		dataTable3.Columns.Add("Isim", typeof(string));
		dataTable3.Rows.Add(0, "Oluşturma");
		dataTable3.Rows.Add(1, "Hizmet");
		dataTable3.Rows.Add(2, "Ürün");
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.DataSource = dataTable3;
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.ValueMember = "ID";
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.DisplayMember = "Isim";
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.PopulateViewColumns();
		foreach (GridColumn column3 in repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.View.Columns)
		{
			column3.Visible = false;
		}
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.View.Columns[1].Caption = "Cinsi";
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.View.Columns[1].Visible = true;
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.BestFitMode = BestFitMode.BestFitResizePopup;
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.ValidateOnEnterKey = true;
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.TextEditStyle = TextEditStyles.Standard;
		repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.AutoComplete = false;
		DataSet lookupTablolar = GenelData.GetLookupTablolar(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		List<enum_DevExpressRepositoryItemGridLookUpEdit> list = new List<enum_DevExpressRepositoryItemGridLookUpEdit>();
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.CariKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.CariUnvan);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.StokKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.StokAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.HizmetKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.HizmetAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.KasaKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.KasaAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.BankaKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.BankaAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.CariPersonelKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.CariPersonelAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.ProjeKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.ProjeAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.DemirbasKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.DemirbasAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.MasrafKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.MasrafAdi);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.PersonelKodu);
		list.Add(enum_DevExpressRepositoryItemGridLookUpEdit.PersonelAdi);
		_RepositoryItems = new DevExpressRepositoryItemGridLookupEdit();
		_RepositoryItems.Init(list, lookupTablolar, _tag);
		foreach (DevExpressRepositoryItemGridLookupEditExtended item in _RepositoryItems.Items)
		{
			aramaTablolariToolStripMenuItem.DropDownItems.Add(item._ToolStripMenuItemAnaBaslik);
		}
		gc_evrak_SorumlulukMerkeziKodu.ColumnEdit = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziKodu);
		gc_evrak_SorumlulukMerkesiAdi.ColumnEdit = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziAdi);
		gc_satir_FaturaSorumlulukMerkeziKodu.ColumnEdit = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziKodu);
		gc_satir_FaturaSorumlulukMerkeziAdi.ColumnEdit = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.SorumlulukMerkeziAdi);
		gc_satir_FaturaProjeKodu.ColumnEdit = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.ProjeKodu);
		gc_satir_FaturaProjeAdi.ColumnEdit = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.ProjeAdi);
	}

	private void advBandedGridView_master_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		_ = (AdvBandedGridView)sender;
		DataRow row = ((DataRowView)e.Row).Row;
		if (e.IsSetData)
		{
			switch (e.Column.FieldName)
			{
			case "gc_evrak_Kur":
				if (_bankaevrakdataset.banka.ban_doviz_cinsi != 0)
				{
					double result = 0.0;
					double.TryParse(((string)e.Value).Replace(".", ","), out result);
					if (result != 0.0)
					{
						row[9] = result;
					}
				}
				break;
			case "gc_evrak_EvrakTipi":
				row[3] = (int)e.Value;
				NetIslemTutariveAktarimSonrasiBakiyeAyarla();
				break;
			case "gc_evrak_Aktar":
				row[1] = (bool)e.Value;
				NetIslemTutariveAktarimSonrasiBakiyeAyarla();
				break;
			}
		}
		if (!e.IsGetData)
		{
			return;
		}
		switch (e.Column.FieldName)
		{
		case "gc_evrak_Kur":
			e.Value = (decimal)(double)row[9];
			break;
		case "gc_evrak_EvrakTipi":
			e.Value = (int)row[3];
			break;
		case "gc_evrak_Aktar":
			e.Value = (bool)row[1];
			break;
		case "gc_evrak_AktarimDurumu":
		{
			enum_GenelEvrakAktarimDurumu enum_GenelEvrakAktarimDurumu = (enum_GenelEvrakAktarimDurumu)row[2];
			if (enum_GenelEvrakAktarimDurumu == enum_GenelEvrakAktarimDurumu.Aktarilmamis)
			{
				e.Value = "";
			}
			else
			{
				e.Value = GenelUtility.EnumToString(enum_GenelEvrakAktarimDurumu);
			}
			break;
		}
		case "gc_evrak_yekun":
			e.Value = _bankaevrakdataset.GetEvrakYekun(row);
			break;
		}
	}

	private void advBandedGridView_master_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
	{
		DataRowView dataRowView = (DataRowView)(sender as AdvBandedGridView).GetRow(e.RowHandle);
		if (dataRowView != null && !Convert.IsDBNull(dataRowView))
		{
			_ = e.Column.Name;
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
			Cari cari = (Cari)((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedDataRow()[10];
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
		if (e.Value == null)
		{
			return;
		}
		string fieldName = e.Column.FieldName;
		if (!(fieldName == "EvrakSira"))
		{
			if (fieldName == "gc_evrak_yekun")
			{
				_ = ((DataSet)((BindingSource)advBandedGridView_master.DataSource).DataSource).Tables[0].Rows[e.ListSourceRowIndex];
				e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_bankaevrakdataset.banka.ban_doviz_cinsi).Kur_sembol;
			}
		}
		else if (Convert.ToInt32(e.Value) == 0)
		{
			e.DisplayText = "Otomatik";
		}
	}

	private void advBandedGridView_master_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
	{
		switch (e.Column.Name)
		{
		case "gc_evrak_AktarimDurumu":
			switch ((enum_GenelEvrakAktarimDurumu)advBandedGridView_master.GetDataRow(e.RowHandle)[2])
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
		case "gc_evrak_CariKodu":
		{
			Cari cari2 = (Cari)advBandedGridView_master.GetDataRow(e.RowHandle)[10];
			if (cari2.cari_kod == "" || cari2.cari_kod == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		}
		case "gc_evrak_CariAdi":
		{
			Cari cari = (Cari)advBandedGridView_master.GetDataRow(e.RowHandle)[10];
			if (cari.cari_kod == "" || cari.cari_kod == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			break;
		}
		case "gc_evrak_KapamaHesapKodu":
		{
			DataRow dataRow = advBandedGridView_master.GetDataRow(e.RowHandle);
			if ((enum_KapamaSekli)dataRow[19] != enum_KapamaSekli.AcikHesap)
			{
				string text = (string)dataRow[20];
				if (text == "" || text == null)
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
	}

	private void repositoryItemGridLookUpEdit_AcikKapali_EditValueChanged(object sender, EventArgs e)
	{
	}

	private void advBandedGridView_satirlar_InitNewRow(object sender, InitNewRowEventArgs e)
	{
		AdvBandedGridView obj = sender as AdvBandedGridView;
		obj.SetRowCellValue(obj.FocusedRowHandle, "SatirID", _bankaevrakdataset.NewSatirID);
	}

	private void advBandedGridView_satirlar_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
	{
		if (e.RowHandle < 0)
		{
			return;
		}
		DataRow dataRow = ((AdvBandedGridView)sender).GetDataRow(e.RowHandle);
		switch (e.Column.Name)
		{
		case "gc_satir_FaturaHesapAdi":
			if ((int)dataRow[6] == 0)
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			else if ((string)dataRow[7] == "" || dataRow[7] == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else if ((bool)dataRow[28])
			{
				e.Appearance.BackColor = RenkYesil;
			}
			else
			{
				e.Appearance.BackColor = RenkSari;
			}
			break;
		case "gc_satir_FaturaHesapKodu":
			if ((int)dataRow[6] == 0)
			{
				e.Appearance.BackColor = RenkBeyaz;
			}
			else if ((string)dataRow[7] == "" || dataRow[7] == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else if ((bool)dataRow[28])
			{
				e.Appearance.BackColor = RenkYesil;
			}
			else
			{
				e.Appearance.BackColor = RenkSari;
			}
			break;
		case "gc_satir_HesapKodu":
			if ((string)dataRow[3] == "" || dataRow[3] == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else if ((bool)dataRow[27])
			{
				e.Appearance.BackColor = RenkYesil;
			}
			else
			{
				e.Appearance.BackColor = RenkSari;
			}
			break;
		case "gc_satir_HesapAdi":
			if ((string)dataRow[3] == "" || dataRow[3] == null)
			{
				e.Appearance.BackColor = RenkKirmizi;
			}
			else if ((bool)dataRow[27])
			{
				e.Appearance.BackColor = RenkYesil;
			}
			else
			{
				e.Appearance.BackColor = RenkSari;
			}
			break;
		}
	}

	private void advBandedGridView_satirlar_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		DataRow row = ((DataRowView)e.Row).Row;
		if (Convert.IsDBNull(row["Cinsi"]))
		{
			row["Cinsi"] = enum_BankaSatirCinsi.CariHesap;
		}
		enum_BankaSatirCinsi enum_BankaSatirCinsi = (enum_BankaSatirCinsi)row[2];
		if (e.IsSetData)
		{
			switch (e.Column.FieldName)
			{
			case "gc_satir_Cinsi":
				row[2] = (enum_BankaSatirCinsi)e.Value;
				switch ((enum_BankaSatirCinsi)row[2])
				{
				case enum_BankaSatirCinsi.Banka:
					row[32] = 1;
					break;
				case enum_BankaSatirCinsi.Ithalat:
					row[32] = 1;
					break;
				case enum_BankaSatirCinsi.FirmaCeki:
					row[32] = 2;
					break;
				case enum_BankaSatirCinsi.FirmaSeneti:
					row[32] = 1;
					break;
				case enum_BankaSatirCinsi.MusteriCeki:
					row[32] = 3;
					break;
				case enum_BankaSatirCinsi.MusteriSeneti:
					row[32] = 4;
					break;
				default:
					row[32] = 0;
					break;
				}
				break;
			case "gc_satir_FaturaDurumu":
				row[6] = e.Value;
				row[7] = "";
				break;
			case "gc_satir_FaturaHesapKodu":
				row[7] = (string)e.Value;
				row[28] = true;
				break;
			case "gc_satir_FaturaHesapAdi":
				row[7] = (string)e.Value;
				row[28] = true;
				break;
			case "gc_satir_HesapKodu":
			{
				row[3] = (string)e.Value;
				row[27] = true;
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				Cari cari2 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, (string)row[3], AdreslerTemsilciyeGore: false, ""));
				row[32] = 0;
				if (cari2.cari_kod != "")
				{
					if (cari2.cari_doviz_cinsi == _bankaevrakdataset.banka.ban_doviz_cinsi)
					{
						row[32] = 0;
					}
					if (cari2.cari_doviz_cinsi1 == _bankaevrakdataset.banka.ban_doviz_cinsi)
					{
						row[32] = 1;
					}
					if (cari2.cari_doviz_cinsi2 == _bankaevrakdataset.banka.ban_doviz_cinsi)
					{
						row[32] = 2;
					}
				}
				break;
			}
			case "gc_satir_HesapAdi":
			{
				row[3] = (string)e.Value;
				row[27] = true;
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				Cari cari = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, (string)row[3], AdreslerTemsilciyeGore: false, ""));
				row[32] = 0;
				if (cari.cari_kod != "")
				{
					if (cari.cari_doviz_cinsi == _bankaevrakdataset.banka.ban_doviz_cinsi)
					{
						row[32] = 0;
					}
					if (cari.cari_doviz_cinsi1 == _bankaevrakdataset.banka.ban_doviz_cinsi)
					{
						row[32] = 1;
					}
					if (cari.cari_doviz_cinsi2 == _bankaevrakdataset.banka.ban_doviz_cinsi)
					{
						row[32] = 2;
					}
				}
				break;
			}
			case "gc_satir_GrupNo":
				row[32] = (int)e.Value;
				break;
			case "gc_satir_Tutar":
				row[4] = (double)(decimal)e.Value;
				NetIslemTutariveAktarimSonrasiBakiyeAyarla();
				break;
			}
		}
		if (!e.IsGetData)
		{
			return;
		}
		try
		{
			switch (e.Column.FieldName)
			{
			case "gc_satir_Cinsi":
				e.Value = (int)row[2];
				break;
			case "gc_satir_FaturaHesapKodu":
				e.Value = (string)row[7];
				break;
			case "gc_satir_FaturaHesapAdi":
				e.Value = (string)row[7];
				break;
			case "gc_satir_HesapKodu":
				e.Value = (string)row[3];
				break;
			case "gc_satir_HesapAdi":
				e.Value = (string)row[3];
				break;
			case "gc_satir_GrupNo":
				e.Value = (int)row[32];
				break;
			case "gc_satir_Tutar":
				e.Value = (double)row[4];
				break;
			case "gc_satir_FaturaDurumu":
				e.Value = (int)row[6];
				break;
			case "gc_satir_MV_HesapNo":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text12 = (string)row[3];
				if (text12 != "")
				{
					Cari cari14 = (Cari)row[31];
					if (text12 != cari14.cari_kod)
					{
						Console.WriteLine("CARI BUL 1 :" + text12);
						cari14 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text12, AdreslerTemsilciyeGore: false, ""));
					}
					e.Value = cari14.cari_banka_hesapno1;
				}
				break;
			}
			case "gc_satir_MV_Unvan":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text9 = (string)row[3];
				if (text9 != "")
				{
					Cari cari11 = (Cari)row[31];
					if (text9 != cari11.cari_kod)
					{
						Console.WriteLine("CARI BUL 2 :" + text9);
						cari11 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text9, AdreslerTemsilciyeGore: false, ""));
					}
					e.Value = cari11.cari_unvan1;
				}
				break;
			}
			case "gc_satir_MV_Unvan2":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text3 = (string)row[3];
				if (text3 != "")
				{
					Cari cari5 = (Cari)row[31];
					if (text3 != cari5.cari_kod)
					{
						Console.WriteLine("CARI BUL 3 :" + text3);
						cari5 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text3, AdreslerTemsilciyeGore: false, ""));
					}
					e.Value = cari5.cari_unvan2;
				}
				break;
			}
			case "gc_satir_MV_Telefon":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text10 = (string)row[3];
				if (text10 != "")
				{
					Cari cari12 = (Cari)row[31];
					if (text10 != cari12.cari_kod)
					{
						Console.WriteLine("CARI BUL 4 :" + text10);
						cari12 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text10, AdreslerTemsilciyeGore: false, ""));
					}
					e.Value = cari12.cari_CepTel;
				}
				break;
			}
			case "gc_satir_MV_EPosta":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text6 = (string)row[3];
				if (text6 != "")
				{
					Cari cari8 = (Cari)row[31];
					if (text6 != cari8.cari_kod)
					{
						Console.WriteLine("CARI BUL 5 :" + text6);
						cari8 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text6, AdreslerTemsilciyeGore: false, ""));
					}
					e.Value = cari8.cari_Email;
				}
				break;
			}
			case "gc_satir_MV_TcVergiNo":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text2 = (string)row[3];
				if (text2 != "")
				{
					Cari cari4 = (Cari)row[31];
					if (text2 != cari4.cari_kod)
					{
						Console.WriteLine("CARI BUL 6 :" + text2);
						cari4 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text2, AdreslerTemsilciyeGore: false, ""));
					}
					e.Value = cari4.cari_vdaire_no;
				}
				break;
			}
			case "gc_satir_MV_Adres":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text8 = (string)row[3];
				if (text8 != "")
				{
					Cari cari10 = (Cari)row[31];
					if (text8 != cari10.cari_kod)
					{
						Console.WriteLine("CARI BUL 7 :" + text8);
						cari10 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text8, AdreslerTemsilciyeGore: false, ""));
					}
					if (cari10.CariAdresleri.Count > 0)
					{
						e.Value = cari10.CariAdresleri[0].adr_cadde;
					}
				}
				break;
			}
			case "gc_satir_MV_Mahalle":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text4 = (string)row[3];
				if (text4 != "")
				{
					Cari cari6 = (Cari)row[31];
					if (text4 != cari6.cari_kod)
					{
						Console.WriteLine("CARI BUL 8 :" + text4);
						cari6 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text4, AdreslerTemsilciyeGore: false, ""));
					}
					if (cari6.CariAdresleri.Count > 0)
					{
						e.Value = cari6.CariAdresleri[0].adr_sokak;
					}
				}
				break;
			}
			case "gc_satir_MV_Ilce":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text11 = (string)row[3];
				if (text11 != "")
				{
					Cari cari13 = (Cari)row[31];
					if (text11 != cari13.cari_kod)
					{
						Console.WriteLine("CARI BUL 9 :" + text11);
						cari13 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text11, AdreslerTemsilciyeGore: false, ""));
					}
					if (cari13.CariAdresleri.Count > 0)
					{
						e.Value = cari13.CariAdresleri[0].adr_ilce;
					}
				}
				break;
			}
			case "gc_satir_MV_Il":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text7 = (string)row[3];
				if (text7 != "")
				{
					Cari cari9 = (Cari)row[31];
					if (text7 != cari9.cari_kod)
					{
						Console.WriteLine("CARI BUL 10 :" + text7);
						cari9 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text7, AdreslerTemsilciyeGore: false, ""));
					}
					if (cari9.CariAdresleri.Count > 0)
					{
						e.Value = cari9.CariAdresleri[0].adr_il;
					}
				}
				break;
			}
			case "gc_satir_MV_PostaKodu":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text5 = (string)row[3];
				if (text5 != "")
				{
					Cari cari7 = (Cari)row[31];
					if (text5 != cari7.cari_kod)
					{
						Console.WriteLine("CARI BUL 11 :" + text5);
						cari7 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text5, AdreslerTemsilciyeGore: false, ""));
					}
					if (cari7.CariAdresleri.Count > 0)
					{
						e.Value = cari7.CariAdresleri[0].adr_posta_kodu;
					}
				}
				break;
			}
			case "gc_satir_MV_Ulke":
			{
				if (enum_BankaSatirCinsi != enum_BankaSatirCinsi.CariHesap)
				{
					break;
				}
				string text = (string)row[3];
				if (text != "")
				{
					Cari cari3 = (Cari)row[31];
					if (text != cari3.cari_kod)
					{
						Console.WriteLine("CARI BUL 12 :" + text);
						cari3 = (Cari)(row[31] = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, ""));
					}
					if (cari3.CariAdresleri.Count > 0)
					{
						e.Value = cari3.CariAdresleri[0].adr_ulke;
					}
				}
				break;
			}
			}
		}
		catch
		{
		}
	}

	private void advBandedGridView_satirlar_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
	{
		if (e.Value == null)
		{
			return;
		}
		string fieldName = e.Column.FieldName;
		if (!(fieldName == "FaturaSira"))
		{
			if (fieldName == "gc_satir_Tutar")
			{
				e.DisplayText = ((double)e.Value).ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(_bankaevrakdataset.banka.ban_doviz_cinsi).Kur_sembol;
			}
		}
		else if (Convert.ToInt32(e.Value) == 0)
		{
			e.DisplayText = "Otomatik";
		}
	}

	private void advBandedGridView_satirlar_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
	{
		AdvBandedGridView advBandedGridView = sender as AdvBandedGridView;
		DataRowView dataRowView = (DataRowView)advBandedGridView.GetRow(e.RowHandle);
		DataRow dataRow = advBandedGridView_master.GetDataRow(advBandedGridView.SourceRowHandle);
		if (dataRowView == null || Convert.IsDBNull(dataRowView))
		{
			return;
		}
		object obj = dataRowView["Cinsi"];
		if (Convert.IsDBNull(obj))
		{
			return;
		}
		object obj2 = dataRowView["FaturaOlusturmaDurumu"];
		if (Convert.IsDBNull(obj2))
		{
			return;
		}
		enum_GenelEvrakTipleri enum_GenelEvrakTipleri = (enum_GenelEvrakTipleri)dataRow[3];
		enum_BankaSatirCinsi enum_BankaSatirCinsi = (enum_BankaSatirCinsi)obj;
		enum_FaturaOlusturmaDurumu enum_FaturaOlusturmaDurumu = (enum_FaturaOlusturmaDurumu)obj2;
		switch (e.Column.Name)
		{
		case "gc_satir_Cinsi":
		{
			DataTable dataTable2 = new DataTable();
			dataTable2.Columns.Add("ID", typeof(int));
			dataTable2.Columns.Add("Isim", typeof(string));
			switch (enum_GenelEvrakTipleri)
			{
			case enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu:
				dataTable2.Rows.Add(4, "Kasa");
				break;
			case enum_GenelEvrakTipleri.GelenHavale:
				dataTable2.Rows.Add(0, "Cari Hesap");
				dataTable2.Rows.Add(1, "Cari Personel");
				dataTable2.Rows.Add(2, "Banka");
				dataTable2.Rows.Add(3, "Hizmet");
				dataTable2.Rows.Add(4, "Kasa");
				dataTable2.Rows.Add(5, "Masraf");
				dataTable2.Rows.Add(6, "Muh. Hesabı");
				dataTable2.Rows.Add(7, "Personel");
				dataTable2.Rows.Add(8, "Demirbaş");
				dataTable2.Rows.Add(9, "İthalat");
				break;
			case enum_GenelEvrakTipleri.GidenHavale:
				dataTable2.Rows.Add(0, "Cari Hesap");
				dataTable2.Rows.Add(1, "Cari Personel");
				dataTable2.Rows.Add(2, "Banka");
				dataTable2.Rows.Add(3, "Hizmet");
				dataTable2.Rows.Add(4, "Kasa");
				dataTable2.Rows.Add(5, "Masraf");
				dataTable2.Rows.Add(6, "Muh. Hesabı");
				dataTable2.Rows.Add(7, "Personel");
				dataTable2.Rows.Add(8, "Demirbaş");
				dataTable2.Rows.Add(9, "İthalat");
				break;
			case enum_GenelEvrakTipleri.KasadanBankayaNakitYatirmaMakbuzu:
				dataTable2.Rows.Add(4, "Kasa");
				break;
			case enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu:
				dataTable2.Rows.Add(10, "Müşteri Çeki");
				break;
			case enum_GenelEvrakTipleri.TahsildekiSenetOdemeBordrosu:
				dataTable2.Rows.Add(11, "Müşteri Seneti");
				break;
			case enum_GenelEvrakTipleri.TahsileCekCikisBordrosu:
				dataTable2.Rows.Add(10, "Müşteri Çeki");
				break;
			case enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu:
				dataTable2.Rows.Add(11, "Müşteri Seneti");
				break;
			case enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu:
				dataTable2.Rows.Add(12, "Firma Çeki");
				break;
			case enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu:
				dataTable2.Rows.Add(13, "Firma Seneti");
				break;
			case enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu:
				dataTable2.Rows.Add(2, "Banka");
				break;
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit2 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit2.DataSource = dataTable2;
			repositoryItemGridLookUpEdit2.ValueMember = "ID";
			repositoryItemGridLookUpEdit2.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit2.PopulateViewColumns();
			foreach (GridColumn column in repositoryItemGridLookUpEdit2.View.Columns)
			{
				column.Visible = false;
			}
			repositoryItemGridLookUpEdit2.View.Columns[1].Caption = "Cinsi";
			repositoryItemGridLookUpEdit2.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit2.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit2.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit2.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit2.AutoComplete = false;
			e.RepositoryItem = repositoryItemGridLookUpEdit2;
			break;
		}
		case "gc_satir_GrupNo":
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ID", typeof(int));
			dataTable.Columns.Add("Isim", typeof(string));
			switch (enum_BankaSatirCinsi)
			{
			case enum_BankaSatirCinsi.Banka:
				dataTable.Rows.Add(1, "Banka mevduat");
				dataTable.Rows.Add(2, "Verilen çekler");
				dataTable.Rows.Add(3, "Tahsildeki çekler");
				dataTable.Rows.Add(4, "Tahsildeki senetler");
				dataTable.Rows.Add(5, "Teminata verilen çekler");
				dataTable.Rows.Add(6, "Teminata verilen senetler");
				dataTable.Rows.Add(7, "Müşteri kredi kartları");
				dataTable.Rows.Add(8, "Firma kredi kartları");
				dataTable.Rows.Add(9, "Müşteri havale sözleri");
				dataTable.Rows.Add(10, "Firma havale emirleri");
				break;
			case enum_BankaSatirCinsi.CariHesap:
			{
				Cari cari = (Cari)dataRowView[31];
				if (cari.cari_kod != "")
				{
					if (cari.cari_doviz_cinsi != 255)
					{
						dataTable.Rows.Add(0, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi).Kur_adi);
					}
					if (cari.cari_doviz_cinsi1 != 255)
					{
						dataTable.Rows.Add(1, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi1).Kur_adi);
					}
					if (cari.cari_doviz_cinsi2 != 255)
					{
						dataTable.Rows.Add(2, _mikrouygulamabilgileri.doviz_cinsi_tanimlari.GetDovizCinsiTanimi(cari.cari_doviz_cinsi2).Kur_adi);
					}
				}
				break;
			}
			case enum_BankaSatirCinsi.CariPersonel:
				dataTable.Rows.Add(0, "İş avansı");
				dataTable.Rows.Add(1, "Bordro avansı");
				dataTable.Rows.Add(2, "Prim hesabı");
				dataTable.Rows.Add(3, "Kısa vadeli borç");
				dataTable.Rows.Add(4, "Uzun vadeli borç");
				break;
			case enum_BankaSatirCinsi.Ithalat:
				dataTable.Rows.Add(1, "Navlun");
				dataTable.Rows.Add(2, "Sigorta");
				dataTable.Rows.Add(3, "Gümrük vergi ve resimler");
				dataTable.Rows.Add(4, "Banka masraf ve fonları");
				dataTable.Rows.Add(5, "Diğer masraflar1");
				dataTable.Rows.Add(6, "Diğer masraflar2");
				dataTable.Rows.Add(7, "Diğer masraflar3");
				dataTable.Rows.Add(8, "Diğer masraflar4");
				dataTable.Rows.Add(9, "Diğer masraflar5");
				dataTable.Rows.Add(10, "Diğer masraflar6");
				break;
			case enum_BankaSatirCinsi.Demirbas:
				dataTable.Rows.Add(0, "Yok");
				break;
			case enum_BankaSatirCinsi.Hizmet:
				dataTable.Rows.Add(0, "Yok");
				break;
			case enum_BankaSatirCinsi.Kasa:
				dataTable.Rows.Add(0, "Yok");
				break;
			case enum_BankaSatirCinsi.Masraf:
				dataTable.Rows.Add(0, "Yok");
				break;
			case enum_BankaSatirCinsi.Personel:
				dataTable.Rows.Add(0, "Yok");
				break;
			case enum_BankaSatirCinsi.FirmaCeki:
				dataTable.Rows.Add(2, "Firma çeki");
				break;
			case enum_BankaSatirCinsi.FirmaSeneti:
				dataTable.Rows.Add(1, "Firma senedi");
				break;
			case enum_BankaSatirCinsi.MusteriCeki:
				dataTable.Rows.Add(3, "Müşteri çeki");
				break;
			case enum_BankaSatirCinsi.MusteriSeneti:
				dataTable.Rows.Add(4, "Müşteri senedi");
				break;
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit.DataSource = dataTable;
			repositoryItemGridLookUpEdit.ValueMember = "ID";
			repositoryItemGridLookUpEdit.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit.PopulateViewColumns();
			foreach (GridColumn column2 in repositoryItemGridLookUpEdit.View.Columns)
			{
				column2.Visible = false;
			}
			repositoryItemGridLookUpEdit.View.Columns[1].Caption = "Gruplar";
			repositoryItemGridLookUpEdit.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit.AutoComplete = false;
			e.RepositoryItem = repositoryItemGridLookUpEdit;
			break;
		}
		case "gc_satir_FaturaHesapKodu":
			switch (enum_FaturaOlusturmaDurumu)
			{
			case enum_FaturaOlusturmaDurumu.Hizmet:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.HizmetKodu);
				break;
			case enum_FaturaOlusturmaDurumu.Urun:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.StokKodu);
				break;
			}
			break;
		case "gc_satir_FaturaHesapAdi":
			switch (enum_FaturaOlusturmaDurumu)
			{
			case enum_FaturaOlusturmaDurumu.Hizmet:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.HizmetAdi);
				break;
			case enum_FaturaOlusturmaDurumu.Urun:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.StokAdi);
				break;
			}
			break;
		case "gc_satir_HesapKodu":
			switch (enum_BankaSatirCinsi)
			{
			case enum_BankaSatirCinsi.Banka:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.BankaKodu);
				break;
			case enum_BankaSatirCinsi.CariHesap:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.CariKodu);
				break;
			case enum_BankaSatirCinsi.CariPersonel:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.CariPersonelKodu);
				break;
			case enum_BankaSatirCinsi.Demirbas:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.DemirbasKodu);
				break;
			case enum_BankaSatirCinsi.Hizmet:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.HizmetKodu);
				break;
			case enum_BankaSatirCinsi.Kasa:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.KasaKodu);
				break;
			case enum_BankaSatirCinsi.Masraf:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.MasrafKodu);
				break;
			case enum_BankaSatirCinsi.Personel:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.PersonelKodu);
				break;
			case enum_BankaSatirCinsi.MuhasebeHesabi:
				break;
			}
			break;
		case "gc_satir_HesapAdi":
			switch (enum_BankaSatirCinsi)
			{
			case enum_BankaSatirCinsi.Banka:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.BankaAdi);
				break;
			case enum_BankaSatirCinsi.CariHesap:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.CariUnvan);
				break;
			case enum_BankaSatirCinsi.CariPersonel:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.CariPersonelAdi);
				break;
			case enum_BankaSatirCinsi.Demirbas:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.DemirbasAdi);
				break;
			case enum_BankaSatirCinsi.Hizmet:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.HizmetAdi);
				break;
			case enum_BankaSatirCinsi.Kasa:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.KasaAdi);
				break;
			case enum_BankaSatirCinsi.Masraf:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.MasrafAdi);
				break;
			case enum_BankaSatirCinsi.Personel:
				e.RepositoryItem = _RepositoryItems.FindRepositoryItem(enum_DevExpressRepositoryItemGridLookUpEdit.PersonelAdi);
				break;
			case enum_BankaSatirCinsi.MuhasebeHesabi:
				break;
			}
			break;
		}
	}

	private void repositoryItemGridLookUpEdit_Satir_Cinsi_EditValueChanging(object sender, ChangingEventArgs e)
	{
		int result = 0;
		if (int.TryParse(e.NewValue.ToString(), out result))
		{
			DataRow focusedDataRow = ((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedDataRow();
			if (focusedDataRow != null)
			{
				focusedDataRow[3] = "";
			}
		}
	}

	private void repositoryItemButtonEdit_Cari_Update_ButtonClick(object sender, ButtonPressedEventArgs e)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		AdvBandedGridView advBandedGridView = (AdvBandedGridView)gridControl_evraklar.FocusedView;
		DataRow focusedDataRow = ((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedDataRow();
		if ((enum_BankaSatirCinsi)focusedDataRow[2] == enum_BankaSatirCinsi.CariHesap)
		{
			if (MessageBox.Show("Mikro kaydının değiştirilmesini onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				return;
			}
			string text = (string)focusedDataRow[3];
			if (text != "")
			{
				advBandedGridView.CloseEditor();
				advBandedGridView.UpdateCurrentRow();
				switch (advBandedGridView.FocusedColumn.FieldName)
				{
				case "BV_HesapNo":
				{
					string value8 = (string)focusedDataRow["BV_HesapNo"];
					string commandText8 = "UPDATE CARI_HESAPLAR SET cari_banka_hesapno1=@deger,cari_lastup_date=getdate() WHERE cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand8 = Db.Connection.CreateCommand();
						sqlCommand8.CommandText = commandText8;
						sqlCommand8.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand8.Parameters.AddWithValue("@deger", value8);
						sqlCommand8.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod8 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod8;
					break;
				}
				case "BV_Unvan":
				{
					string value12 = (string)focusedDataRow["BV_Unvan"];
					string commandText12 = "UPDATE CARI_HESAPLAR SET cari_unvan1=@deger,cari_lastup_date=getdate() WHERE cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand12 = Db.Connection.CreateCommand();
						sqlCommand12.CommandText = commandText12;
						sqlCommand12.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand12.Parameters.AddWithValue("@deger", value12);
						sqlCommand12.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod12 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod12;
					break;
				}
				case "BV_Unvan2":
				{
					string value4 = (string)focusedDataRow["BV_Unvan2"];
					string commandText4 = "UPDATE CARI_HESAPLAR SET cari_unvan2=@deger,cari_lastup_date=getdate() WHERE cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand4 = Db.Connection.CreateCommand();
						sqlCommand4.CommandText = commandText4;
						sqlCommand4.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand4.Parameters.AddWithValue("@deger", value4);
						sqlCommand4.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod4 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod4;
					break;
				}
				case "BV_Telefon":
				{
					string value10 = (string)focusedDataRow["BV_Telefon"];
					string commandText10 = "UPDATE CARI_HESAPLAR SET cari_CepTel=@deger,cari_lastup_date=getdate() WHERE cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand10 = Db.Connection.CreateCommand();
						sqlCommand10.CommandText = commandText10;
						sqlCommand10.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand10.Parameters.AddWithValue("@deger", value10);
						sqlCommand10.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod10 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod10;
					break;
				}
				case "BV_EPosta":
				{
					string value6 = (string)focusedDataRow["BV_EPosta"];
					string commandText6 = "UPDATE CARI_HESAPLAR SET cari_EMail=@deger,cari_lastup_date=getdate() WHERE cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand6 = Db.Connection.CreateCommand();
						sqlCommand6.CommandText = commandText6;
						sqlCommand6.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand6.Parameters.AddWithValue("@deger", value6);
						sqlCommand6.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod6 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod6;
					break;
				}
				case "BV_TcVergiNo":
				{
					string value2 = (string)focusedDataRow["BV_TcVergiNo"];
					string commandText2 = "UPDATE CARI_HESAPLAR SET cari_vdaire_no=@deger,cari_lastup_date=getdate() WHERE cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand2 = Db.Connection.CreateCommand();
						sqlCommand2.CommandText = commandText2;
						sqlCommand2.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand2.Parameters.AddWithValue("@deger", value2);
						sqlCommand2.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod2 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod2;
					break;
				}
				case "BV_Adres":
				{
					CariAdresYoksaOlustur(text);
					string value11 = (string)focusedDataRow["BV_Adres"];
					string commandText11 = "UPDATE CARI_HESAP_ADRESLERI SET adr_cadde=@deger,adr_lastup_date=getdate() WHERE adr_adres_no=1 AND adr_cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand11 = Db.Connection.CreateCommand();
						sqlCommand11.CommandText = commandText11;
						sqlCommand11.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand11.Parameters.AddWithValue("@deger", value11);
						sqlCommand11.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod11 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod11;
					break;
				}
				case "BV_Mahalle":
				{
					CariAdresYoksaOlustur(text);
					string value9 = (string)focusedDataRow["BV_Mahalle"];
					string commandText9 = "UPDATE CARI_HESAP_ADRESLERI SET adr_sokak=@deger,adr_lastup_date=getdate() WHERE adr_adres_no=1 AND adr_cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand9 = Db.Connection.CreateCommand();
						sqlCommand9.CommandText = commandText9;
						sqlCommand9.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand9.Parameters.AddWithValue("@deger", value9);
						sqlCommand9.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod9 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod9;
					break;
				}
				case "BV_Ilce":
				{
					CariAdresYoksaOlustur(text);
					string value7 = (string)focusedDataRow["BV_Ilce"];
					string commandText7 = "UPDATE CARI_HESAP_ADRESLERI SET adr_ilce=@deger,adr_lastup_date=getdate() WHERE adr_adres_no=1 AND adr_cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand7 = Db.Connection.CreateCommand();
						sqlCommand7.CommandText = commandText7;
						sqlCommand7.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand7.Parameters.AddWithValue("@deger", value7);
						sqlCommand7.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod7 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod7;
					break;
				}
				case "BV_Il":
				{
					CariAdresYoksaOlustur(text);
					string value5 = (string)focusedDataRow["BV_Il"];
					string commandText5 = "UPDATE CARI_HESAP_ADRESLERI SET adr_il=@deger,adr_lastup_date=getdate() WHERE adr_adres_no=1 AND adr_cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand5 = Db.Connection.CreateCommand();
						sqlCommand5.CommandText = commandText5;
						sqlCommand5.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand5.Parameters.AddWithValue("@deger", value5);
						sqlCommand5.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod5 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod5;
					break;
				}
				case "BV_PostaKodu":
				{
					CariAdresYoksaOlustur(text);
					string value3 = (string)focusedDataRow["BV_PostaKodu"];
					string commandText3 = "UPDATE CARI_HESAP_ADRESLERI SET adr_posta_kodu=@deger,adr_lastup_date=getdate() WHERE adr_adres_no=1 AND adr_cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand3 = Db.Connection.CreateCommand();
						sqlCommand3.CommandText = commandText3;
						sqlCommand3.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand3.Parameters.AddWithValue("@deger", value3);
						sqlCommand3.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod3 = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod3;
					break;
				}
				case "BV_Ulke":
				{
					CariAdresYoksaOlustur(text);
					string value = (string)focusedDataRow["BV_Ulke"];
					string commandText = "UPDATE CARI_HESAP_ADRESLERI SET adr_ulke=@deger,adr_lastup_date=getdate() WHERE adr_adres_no=1 AND adr_cari_kod=@cari_kod";
					try
					{
						using SqlCommand sqlCommand = Db.Connection.CreateCommand();
						sqlCommand.CommandText = commandText;
						sqlCommand.Parameters.AddWithValue("@cari_kod", text);
						sqlCommand.Parameters.AddWithValue("@deger", value);
						sqlCommand.ExecuteScalar();
					}
					catch
					{
						MessageBox.Show("Bilgi güncellenemedi. Lütfen uzunluğu kontrol ediniz.");
					}
					Cari cariByCariKod = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					focusedDataRow[31] = cariByCariKod;
					break;
				}
				}
			}
			else
			{
				MessageBox.Show("Önce cari hesap seçiniz.");
			}
		}
		else
		{
			MessageBox.Show("Sadece cari hesap bilgileri değiştirilebilir.");
		}
	}

	private void CariAdresYoksaOlustur(string carikod)
	{
		if (AppBase.MikroVersiyonu >= 16)
		{
			V16_CariAdresYoksaOlustur(carikod);
		}
		else
		{
			V15_CariAdresYoksaOlustur(carikod);
		}
	}

	private void V15_CariAdresYoksaOlustur(string carikod)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		if (CariData.GetCariByCariKod(Db.Connection, carikod, AdreslerTemsilciyeGore: false, "").CariAdresleri.Count != 0)
		{
			return;
		}
		try
		{
			CariAdres cariAdres = new CariAdres();
			cariAdres.adr_adres_no = 1;
			cariAdres.adr_cari_kod = carikod;
			using SqlCommand sqlCommand = Db.Connection.CreateCommand();
			string commandText = "BEGIN INSERT INTO CARI_HESAP_ADRESLERI(adr_RECid_DBCno,adr_RECid_RECno,adr_SpecRECno,adr_iptal,adr_fileid,adr_hidden,adr_kilitli,adr_degisti,adr_checksum,adr_create_user,adr_create_date,adr_lastup_user,adr_lastup_date,adr_special1,adr_special2,adr_special3,adr_cari_kod,adr_adres_no,adr_aprint_fl,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_yon_kodu,adr_uzaklik_kodu,adr_temsilci_kodu,adr_ozel_not,adr_ziyaretperyodu,adr_ziyaretgunu,adr_gps_enlem,adr_gps_boylam,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7) VALUES(@adr_RECid_DBCno,@adr_RECid_RECno,@adr_SpecRECno,@adr_iptal,@adr_fileid,@adr_hidden,@adr_kilitli,@adr_degisti,@adr_checksum,@adr_create_user,getdate(),@adr_lastup_user,getdate(),@adr_special1,@adr_special2,@adr_special3,@adr_cari_kod,@adr_adres_no,@adr_aprint_fl,@adr_cadde,@adr_sokak,@adr_posta_kodu,@adr_ilce,@adr_il,@adr_ulke,@adr_tel_ulke_kodu,@adr_tel_bolge_kodu,@adr_tel_no1,@adr_tel_no2,@adr_tel_faxno,@adr_tel_modem,@adr_yon_kodu,@adr_uzaklik_kodu,@adr_temsilci_kodu,@adr_ozel_not,@adr_ziyaretperyodu,@adr_ziyaretgunu,@adr_gps_enlem,@adr_gps_boylam,@adr_ziyarethaftasi,@adr_ziygunu2_1,@adr_ziygunu2_2,@adr_ziygunu2_3,@adr_ziygunu2_4,@adr_ziygunu2_5,@adr_ziygunu2_6,@adr_ziygunu2_7) UPDATE CARI_HESAP_ADRESLERI SET adr_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE adr_RECno=(SELECT SCOPE_IDENTITY()) END";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@adr_RECid_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@adr_RECid_RECno", 0);
			sqlCommand.Parameters.AddWithValue("@adr_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@adr_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@adr_fileid", 32);
			sqlCommand.Parameters.AddWithValue("@adr_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@adr_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@adr_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@adr_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@adr_create_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
			sqlCommand.Parameters.AddWithValue("@adr_lastup_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
			sqlCommand.Parameters.AddWithValue("@adr_special1", cariAdres.adr_special1);
			sqlCommand.Parameters.AddWithValue("@adr_special2", cariAdres.adr_special2);
			sqlCommand.Parameters.AddWithValue("@adr_special3", cariAdres.adr_special3);
			sqlCommand.Parameters.AddWithValue("@adr_cari_kod", cariAdres.adr_cari_kod);
			sqlCommand.Parameters.AddWithValue("@adr_adres_no", cariAdres.adr_adres_no);
			sqlCommand.Parameters.AddWithValue("@adr_aprint_fl", 0);
			sqlCommand.Parameters.AddWithValue("@adr_cadde", cariAdres.adr_cadde);
			sqlCommand.Parameters.AddWithValue("@adr_sokak", cariAdres.adr_sokak);
			sqlCommand.Parameters.AddWithValue("@adr_posta_kodu", cariAdres.adr_posta_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_ilce", cariAdres.adr_ilce);
			sqlCommand.Parameters.AddWithValue("@adr_il", cariAdres.adr_il);
			sqlCommand.Parameters.AddWithValue("@adr_ulke", cariAdres.adr_ulke);
			sqlCommand.Parameters.AddWithValue("@adr_tel_ulke_kodu", cariAdres.adr_tel_ulke_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_tel_bolge_kodu", cariAdres.adr_tel_bolge_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_tel_no1", cariAdres.adr_tel_no1);
			sqlCommand.Parameters.AddWithValue("@adr_tel_no2", cariAdres.adr_tel_no2);
			sqlCommand.Parameters.AddWithValue("@adr_tel_faxno", cariAdres.adr_tel_faxno);
			sqlCommand.Parameters.AddWithValue("@adr_tel_modem", cariAdres.adr_tel_modem);
			sqlCommand.Parameters.AddWithValue("@adr_yon_kodu", cariAdres.adr_yon_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_uzaklik_kodu", cariAdres.adr_uzaklik_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_temsilci_kodu", cariAdres.adr_temsilci_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_ozel_not", cariAdres.adr_ozel_not);
			sqlCommand.Parameters.AddWithValue("@adr_ziyaretperyodu", cariAdres.adr_ziyaretperyodu);
			sqlCommand.Parameters.AddWithValue("@adr_ziyaretgunu", cariAdres.adr_ziyaretgunu);
			sqlCommand.Parameters.AddWithValue("@adr_gps_enlem", cariAdres.adr_gps_enlem);
			sqlCommand.Parameters.AddWithValue("@adr_gps_boylam", cariAdres.adr_gps_boylam);
			sqlCommand.Parameters.AddWithValue("@adr_ziyarethaftasi", cariAdres.adr_ziyarethaftasi);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_1", cariAdres.adr_ziygunu2_1);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_2", cariAdres.adr_ziygunu2_2);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_3", cariAdres.adr_ziygunu2_3);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_4", cariAdres.adr_ziygunu2_4);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_5", cariAdres.adr_ziygunu2_5);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_6", cariAdres.adr_ziygunu2_6);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_7", cariAdres.adr_ziygunu2_7);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = Db.Connection;
			sqlCommand.ExecuteNonQuery();
		}
		catch
		{
		}
	}

	private void V16_CariAdresYoksaOlustur(string carikod)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		if (CariData.GetCariByCariKod(Db.Connection, carikod, AdreslerTemsilciyeGore: false, "").CariAdresleri.Count != 0)
		{
			return;
		}
		try
		{
			CariAdres cariAdres = new CariAdres();
			cariAdres.adr_adres_no = 1;
			cariAdres.adr_cari_kod = carikod;
			using SqlCommand sqlCommand = Db.Connection.CreateCommand();
			string commandText = "BEGIN INSERT INTO CARI_HESAP_ADRESLERI(adr_Guid,adr_DBCno,adr_SpecRECno,adr_iptal,adr_fileid,adr_hidden,adr_kilitli,adr_degisti,adr_checksum,adr_create_user,adr_create_date,adr_lastup_user,adr_lastup_date,adr_special1,adr_special2,adr_special3,adr_cari_kod,adr_adres_no,adr_aprint_fl,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_yon_kodu,adr_uzaklik_kodu,adr_temsilci_kodu,adr_ozel_not,adr_ziyaretperyodu,adr_ziyaretgunu,adr_gps_enlem,adr_gps_boylam,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7) VALUES(NEWID(),@adr_DBCno,@adr_SpecRECno,@adr_iptal,@adr_fileid,@adr_hidden,@adr_kilitli,@adr_degisti,@adr_checksum,@adr_create_user,getdate(),@adr_lastup_user,getdate(),@adr_special1,@adr_special2,@adr_special3,@adr_cari_kod,@adr_adres_no,@adr_aprint_fl,@adr_cadde,@adr_sokak,@adr_posta_kodu,@adr_ilce,@adr_il,@adr_ulke,@adr_tel_ulke_kodu,@adr_tel_bolge_kodu,@adr_tel_no1,@adr_tel_no2,@adr_tel_faxno,@adr_tel_modem,@adr_yon_kodu,@adr_uzaklik_kodu,@adr_temsilci_kodu,@adr_ozel_not,@adr_ziyaretperyodu,@adr_ziyaretgunu,@adr_gps_enlem,@adr_gps_boylam,@adr_ziyarethaftasi,@adr_ziygunu2_1,@adr_ziygunu2_2,@adr_ziygunu2_3,@adr_ziygunu2_4,@adr_ziygunu2_5,@adr_ziygunu2_6,@adr_ziygunu2_7) END";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@adr_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@adr_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@adr_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@adr_fileid", 32);
			sqlCommand.Parameters.AddWithValue("@adr_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@adr_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@adr_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@adr_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@adr_create_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
			sqlCommand.Parameters.AddWithValue("@adr_lastup_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
			sqlCommand.Parameters.AddWithValue("@adr_special1", cariAdres.adr_special1);
			sqlCommand.Parameters.AddWithValue("@adr_special2", cariAdres.adr_special2);
			sqlCommand.Parameters.AddWithValue("@adr_special3", cariAdres.adr_special3);
			sqlCommand.Parameters.AddWithValue("@adr_cari_kod", cariAdres.adr_cari_kod);
			sqlCommand.Parameters.AddWithValue("@adr_adres_no", cariAdres.adr_adres_no);
			sqlCommand.Parameters.AddWithValue("@adr_aprint_fl", 0);
			sqlCommand.Parameters.AddWithValue("@adr_cadde", cariAdres.adr_cadde);
			sqlCommand.Parameters.AddWithValue("@adr_sokak", cariAdres.adr_sokak);
			sqlCommand.Parameters.AddWithValue("@adr_posta_kodu", cariAdres.adr_posta_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_ilce", cariAdres.adr_ilce);
			sqlCommand.Parameters.AddWithValue("@adr_il", cariAdres.adr_il);
			sqlCommand.Parameters.AddWithValue("@adr_ulke", cariAdres.adr_ulke);
			sqlCommand.Parameters.AddWithValue("@adr_tel_ulke_kodu", cariAdres.adr_tel_ulke_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_tel_bolge_kodu", cariAdres.adr_tel_bolge_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_tel_no1", cariAdres.adr_tel_no1);
			sqlCommand.Parameters.AddWithValue("@adr_tel_no2", cariAdres.adr_tel_no2);
			sqlCommand.Parameters.AddWithValue("@adr_tel_faxno", cariAdres.adr_tel_faxno);
			sqlCommand.Parameters.AddWithValue("@adr_tel_modem", cariAdres.adr_tel_modem);
			sqlCommand.Parameters.AddWithValue("@adr_yon_kodu", cariAdres.adr_yon_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_uzaklik_kodu", cariAdres.adr_uzaklik_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_temsilci_kodu", cariAdres.adr_temsilci_kodu);
			sqlCommand.Parameters.AddWithValue("@adr_ozel_not", cariAdres.adr_ozel_not);
			sqlCommand.Parameters.AddWithValue("@adr_ziyaretperyodu", cariAdres.adr_ziyaretperyodu);
			sqlCommand.Parameters.AddWithValue("@adr_ziyaretgunu", cariAdres.adr_ziyaretgunu);
			sqlCommand.Parameters.AddWithValue("@adr_gps_enlem", cariAdres.adr_gps_enlem);
			sqlCommand.Parameters.AddWithValue("@adr_gps_boylam", cariAdres.adr_gps_boylam);
			sqlCommand.Parameters.AddWithValue("@adr_ziyarethaftasi", cariAdres.adr_ziyarethaftasi);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_1", cariAdres.adr_ziygunu2_1);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_2", cariAdres.adr_ziygunu2_2);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_3", cariAdres.adr_ziygunu2_3);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_4", cariAdres.adr_ziygunu2_4);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_5", cariAdres.adr_ziygunu2_5);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_6", cariAdres.adr_ziygunu2_6);
			sqlCommand.Parameters.AddWithValue("@adr_ziygunu2_7", cariAdres.adr_ziygunu2_7);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = Db.Connection;
			sqlCommand.ExecuteNonQuery();
		}
		catch
		{
		}
	}

	private void repositoryItemButtonEdit_Cari_Ekle_ButtonClick(object sender, ButtonPressedEventArgs e)
	{
		Console.WriteLine("CARI EKLEME BUTON BASILDI");
		DataRow focusedDataRow = ((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedDataRow();
		string cari_banka_hesapno = (string)focusedDataRow["BV_HesapNo"];
		string cari_unvan = (string)focusedDataRow["BV_Unvan"];
		string cari_unvan2 = (string)focusedDataRow["BV_Unvan2"];
		string cari_CepTel = (string)focusedDataRow["BV_Telefon"];
		string cari_Email = (string)focusedDataRow["BV_EPosta"];
		string cari_vdaire_no = (string)focusedDataRow["BV_TcVergiNo"];
		string adr_cadde = (string)focusedDataRow["BV_Adres"];
		string adr_sokak = (string)focusedDataRow["BV_Mahalle"];
		string adr_ilce = (string)focusedDataRow["BV_Ilce"];
		string adr_il = (string)focusedDataRow["BV_Il"];
		string adr_ulke = (string)focusedDataRow["BV_Ulke"];
		string adr_posta_kodu = (string)focusedDataRow["BV_PostaKodu"];
		Cari cari = new Cari();
		cari.cari_banka_hesapno1 = cari_banka_hesapno;
		cari.cari_unvan1 = cari_unvan;
		cari.cari_unvan2 = cari_unvan2;
		cari.cari_CepTel = cari_CepTel;
		cari.cari_Email = cari_Email;
		cari.cari_vdaire_no = cari_vdaire_no;
		cari.cari_doviz_cinsi = _bankaevrakdataset.banka.ban_doviz_cinsi;
		cari.cari_muh_kod = _mikrouygulamabilgileri.GenelParametreler._GetParametre("CariYeniCariMuhasebeKodu")._GetString;
		CariAdres cariAdres = new CariAdres();
		cariAdres.adr_adres_no = 1;
		cariAdres.adr_cadde = adr_cadde;
		cariAdres.adr_sokak = adr_sokak;
		cariAdres.adr_ilce = adr_ilce;
		cariAdres.adr_il = adr_il;
		cariAdres.adr_ulke = adr_ulke;
		cariAdres.adr_posta_kodu = adr_posta_kodu;
		cari.CariAdresleri = new List<CariAdres>();
		cari.CariAdresleri.Add(cariAdres);
		YeniCariEkle yeniCariEkle = new YeniCariEkle(_mikrouygulamabilgileri, cari);
		if (yeniCariEkle.ShowDialog() == DialogResult.OK)
		{
			RepositoryItemlariGuncelle();
			focusedDataRow["Cinsi"] = enum_BankaSatirCinsi.CariHesap;
			focusedDataRow["HesapKodu"] = yeniCariEkle._cari.cari_kod;
			focusedDataRow["GrupNo"] = 0;
		}
	}

	private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void acToolStripMenuItem_Click(object sender, EventArgs e)
	{
		openFileDialog1.Filter = "Banka Evrak Dosyaları|*.fbe|Tüm dosyalar|*.*";
		if (openFileDialog1.ShowDialog() == DialogResult.OK)
		{
			_bankaevrakdataset._DosyaAdi = openFileDialog1.FileName;
			DosyaAc();
		}
	}

	private void DosyaAc()
	{
		string json = (string)GenelUtility.ToObjectGZip(File.ReadAllBytes(_bankaevrakdataset._DosyaAdi));
		BankaEvrakDataSet bankaEvrakDataSet = JSON.Instance.ToObject<BankaEvrakDataSet>(json);
		foreach (DataRow row in bankaEvrakDataSet.dataset.Tables["Satirlar"].Rows)
		{
			row["TempCari"] = new Cari();
		}
		_bankaevrakdataset = bankaEvrakDataSet;
		_bankaevrakdataset.RelationEkle();
		bindData();
		mikri_Disi_Ek_Veriler_Baslik_Ayarla();
	}

	private void mikri_Disi_Ek_Veriler_Baslik_Ayarla()
	{
		try
		{
			gc_satir_metin1.Caption = _bankaevrakdataset.metin1_baslik;
			gc_satir_metin2.Caption = _bankaevrakdataset.metin2_baslik;
			gc_satir_metin3.Caption = _bankaevrakdataset.metin3_baslik;
			gc_satir_metin4.Caption = _bankaevrakdataset.metin4_baslik;
			gc_satir_metin5.Caption = _bankaevrakdataset.metin5_baslik;
			gc_satir_metin6.Caption = _bankaevrakdataset.metin6_baslik;
			gc_satir_metin7.Caption = _bankaevrakdataset.metin7_baslik;
			gc_satir_metin8.Caption = _bankaevrakdataset.metin8_baslik;
			gc_satir_metin9.Caption = _bankaevrakdataset.metin9_baslik;
			gc_satir_metin10.Caption = _bankaevrakdataset.metin10_baslik;
			gc_satir_metin11.Caption = _bankaevrakdataset.metin11_baslik;
			gc_satir_metin12.Caption = _bankaevrakdataset.metin12_baslik;
			gc_satir_metin13.Caption = _bankaevrakdataset.metin13_baslik;
			gc_satir_metin14.Caption = _bankaevrakdataset.metin14_baslik;
			gc_satir_metin15.Caption = _bankaevrakdataset.metin15_baslik;
			gc_satir_metin16.Caption = _bankaevrakdataset.metin16_baslik;
			gc_satir_metin17.Caption = _bankaevrakdataset.metin17_baslik;
			gc_satir_metin18.Caption = _bankaevrakdataset.metin18_baslik;
			gc_satir_metin19.Caption = _bankaevrakdataset.metin19_baslik;
			gc_satir_metin20.Caption = _bankaevrakdataset.metin20_baslik;
			gc_satir_metin21.Caption = _bankaevrakdataset.metin21_baslik;
			gc_satir_metin22.Caption = _bankaevrakdataset.metin22_baslik;
			gc_satir_metin23.Caption = _bankaevrakdataset.metin23_baslik;
			gc_satir_metin24.Caption = _bankaevrakdataset.metin24_baslik;
			gc_satir_metin25.Caption = _bankaevrakdataset.metin25_baslik;
			gc_satir_metin26.Caption = _bankaevrakdataset.metin26_baslik;
			gc_satir_metin27.Caption = _bankaevrakdataset.metin27_baslik;
			gc_satir_metin28.Caption = _bankaevrakdataset.metin28_baslik;
			gc_satir_metin29.Caption = _bankaevrakdataset.metin29_baslik;
			gc_satir_metin30.Caption = _bankaevrakdataset.metin30_baslik;
			gc_satir_metin31.Caption = _bankaevrakdataset.metin31_baslik;
			gc_satir_metin32.Caption = _bankaevrakdataset.metin32_baslik;
			gc_satir_metin33.Caption = _bankaevrakdataset.metin33_baslik;
			gc_satir_metin34.Caption = _bankaevrakdataset.metin34_baslik;
			gc_satir_metin35.Caption = _bankaevrakdataset.metin35_baslik;
			gc_satir_metin36.Caption = _bankaevrakdataset.metin36_baslik;
			gc_satir_metin37.Caption = _bankaevrakdataset.metin37_baslik;
			gc_satir_metin38.Caption = _bankaevrakdataset.metin38_baslik;
			gc_satir_metin39.Caption = _bankaevrakdataset.metin39_baslik;
			gc_satir_metin40.Caption = _bankaevrakdataset.metin40_baslik;
			gc_satir_metin41.Caption = _bankaevrakdataset.metin41_baslik;
			gc_satir_metin42.Caption = _bankaevrakdataset.metin42_baslik;
			gc_satir_metin43.Caption = _bankaevrakdataset.metin43_baslik;
			gc_satir_metin44.Caption = _bankaevrakdataset.metin44_baslik;
			gc_satir_metin45.Caption = _bankaevrakdataset.metin45_baslik;
			gc_satir_metin46.Caption = _bankaevrakdataset.metin46_baslik;
			gc_satir_metin47.Caption = _bankaevrakdataset.metin47_baslik;
			gc_satir_metin48.Caption = _bankaevrakdataset.metin48_baslik;
			gc_satir_metin49.Caption = _bankaevrakdataset.metin49_baslik;
			gc_satir_metin50.Caption = _bankaevrakdataset.metin50_baslik;
			gc_satir_dropbox1.Caption = _bankaevrakdataset.dropbox1_baslik;
			gc_satir_dropbox2.Caption = _bankaevrakdataset.dropbox2_baslik;
			gc_satir_dropbox3.Caption = _bankaevrakdataset.dropbox3_baslik;
			gc_satir_dropbox4.Caption = _bankaevrakdataset.dropbox4_baslik;
			gc_satir_dropbox5.Caption = _bankaevrakdataset.dropbox5_baslik;
			gc_satir_dropbox6.Caption = _bankaevrakdataset.dropbox6_baslik;
			gc_satir_dropbox7.Caption = _bankaevrakdataset.dropbox7_baslik;
			gc_satir_dropbox8.Caption = _bankaevrakdataset.dropbox8_baslik;
			gc_satir_dropbox9.Caption = _bankaevrakdataset.dropbox9_baslik;
			gc_satir_dropbox10.Caption = _bankaevrakdataset.dropbox10_baslik;
			gc_satir_checkbox1.Caption = _bankaevrakdataset.checkbox1_baslik;
			gc_satir_checkbox2.Caption = _bankaevrakdataset.checkbox2_baslik;
			gc_satir_checkbox3.Caption = _bankaevrakdataset.checkbox3_baslik;
			gc_satir_checkbox4.Caption = _bankaevrakdataset.checkbox4_baslik;
			gc_satir_checkbox5.Caption = _bankaevrakdataset.checkbox5_baslik;
			gc_satir_checkbox6.Caption = _bankaevrakdataset.checkbox6_baslik;
			gc_satir_checkbox7.Caption = _bankaevrakdataset.checkbox7_baslik;
			gc_satir_checkbox8.Caption = _bankaevrakdataset.checkbox8_baslik;
			gc_satir_checkbox9.Caption = _bankaevrakdataset.checkbox9_baslik;
			gc_satir_checkbox10.Caption = _bankaevrakdataset.checkbox10_baslik;
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ID", typeof(string));
			dataTable.Columns.Add("Isim", typeof(string));
			string[] array = _bankaevrakdataset.dropbox1_secenekler_veri.Split(',');
			string[] array2 = _bankaevrakdataset.dropbox1_secenekler_yazi.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				dataTable.Rows.Add(array[i], array2[i]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit.DataSource = dataTable;
			repositoryItemGridLookUpEdit.ValueMember = "ID";
			repositoryItemGridLookUpEdit.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit.PopulateViewColumns();
			foreach (GridColumn column in repositoryItemGridLookUpEdit.View.Columns)
			{
				column.Visible = false;
			}
			repositoryItemGridLookUpEdit.View.Columns[1].Caption = _bankaevrakdataset.dropbox1_baslik;
			repositoryItemGridLookUpEdit.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit.AutoComplete = false;
			gc_satir_dropbox1.ColumnEdit = repositoryItemGridLookUpEdit;
			DataTable dataTable2 = new DataTable();
			dataTable2.Columns.Add("ID", typeof(string));
			dataTable2.Columns.Add("Isim", typeof(string));
			string[] array3 = _bankaevrakdataset.dropbox2_secenekler_veri.Split(',');
			string[] array4 = _bankaevrakdataset.dropbox2_secenekler_yazi.Split(',');
			for (int j = 0; j < array3.Length; j++)
			{
				dataTable2.Rows.Add(array3[j], array4[j]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit2 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit2.DataSource = dataTable2;
			repositoryItemGridLookUpEdit2.ValueMember = "ID";
			repositoryItemGridLookUpEdit2.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit2.PopulateViewColumns();
			foreach (GridColumn column2 in repositoryItemGridLookUpEdit2.View.Columns)
			{
				column2.Visible = false;
			}
			repositoryItemGridLookUpEdit2.View.Columns[1].Caption = _bankaevrakdataset.dropbox2_baslik;
			repositoryItemGridLookUpEdit2.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit2.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit2.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit2.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit2.AutoComplete = false;
			gc_satir_dropbox2.ColumnEdit = repositoryItemGridLookUpEdit2;
			DataTable dataTable3 = new DataTable();
			dataTable3.Columns.Add("ID", typeof(string));
			dataTable3.Columns.Add("Isim", typeof(string));
			string[] array5 = _bankaevrakdataset.dropbox3_secenekler_veri.Split(',');
			string[] array6 = _bankaevrakdataset.dropbox3_secenekler_yazi.Split(',');
			for (int k = 0; k < array5.Length; k++)
			{
				dataTable3.Rows.Add(array5[k], array6[k]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit3 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit3.DataSource = dataTable3;
			repositoryItemGridLookUpEdit3.ValueMember = "ID";
			repositoryItemGridLookUpEdit3.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit3.PopulateViewColumns();
			foreach (GridColumn column3 in repositoryItemGridLookUpEdit3.View.Columns)
			{
				column3.Visible = false;
			}
			repositoryItemGridLookUpEdit3.View.Columns[1].Caption = _bankaevrakdataset.dropbox3_baslik;
			repositoryItemGridLookUpEdit3.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit3.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit3.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit3.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit3.AutoComplete = false;
			gc_satir_dropbox3.ColumnEdit = repositoryItemGridLookUpEdit3;
			DataTable dataTable4 = new DataTable();
			dataTable4.Columns.Add("ID", typeof(string));
			dataTable4.Columns.Add("Isim", typeof(string));
			string[] array7 = _bankaevrakdataset.dropbox4_secenekler_veri.Split(',');
			string[] array8 = _bankaevrakdataset.dropbox4_secenekler_yazi.Split(',');
			for (int l = 0; l < array7.Length; l++)
			{
				dataTable4.Rows.Add(array7[l], array8[l]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit4 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit4.DataSource = dataTable4;
			repositoryItemGridLookUpEdit4.ValueMember = "ID";
			repositoryItemGridLookUpEdit4.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit4.PopulateViewColumns();
			foreach (GridColumn column4 in repositoryItemGridLookUpEdit4.View.Columns)
			{
				column4.Visible = false;
			}
			repositoryItemGridLookUpEdit4.View.Columns[1].Caption = _bankaevrakdataset.dropbox4_baslik;
			repositoryItemGridLookUpEdit4.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit4.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit4.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit4.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit4.AutoComplete = false;
			gc_satir_dropbox4.ColumnEdit = repositoryItemGridLookUpEdit4;
			DataTable dataTable5 = new DataTable();
			dataTable5.Columns.Add("ID", typeof(string));
			dataTable5.Columns.Add("Isim", typeof(string));
			string[] array9 = _bankaevrakdataset.dropbox5_secenekler_veri.Split(',');
			string[] array10 = _bankaevrakdataset.dropbox5_secenekler_yazi.Split(',');
			for (int m = 0; m < array9.Length; m++)
			{
				dataTable5.Rows.Add(array9[m], array10[m]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit5 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit5.DataSource = dataTable5;
			repositoryItemGridLookUpEdit5.ValueMember = "ID";
			repositoryItemGridLookUpEdit5.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit5.PopulateViewColumns();
			foreach (GridColumn column5 in repositoryItemGridLookUpEdit5.View.Columns)
			{
				column5.Visible = false;
			}
			repositoryItemGridLookUpEdit5.View.Columns[1].Caption = _bankaevrakdataset.dropbox5_baslik;
			repositoryItemGridLookUpEdit5.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit5.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit5.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit5.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit5.AutoComplete = false;
			gc_satir_dropbox5.ColumnEdit = repositoryItemGridLookUpEdit5;
			DataTable dataTable6 = new DataTable();
			dataTable6.Columns.Add("ID", typeof(string));
			dataTable6.Columns.Add("Isim", typeof(string));
			string[] array11 = _bankaevrakdataset.dropbox6_secenekler_veri.Split(',');
			string[] array12 = _bankaevrakdataset.dropbox6_secenekler_yazi.Split(',');
			for (int n = 0; n < array11.Length; n++)
			{
				dataTable6.Rows.Add(array11[n], array12[n]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit6 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit6.DataSource = dataTable6;
			repositoryItemGridLookUpEdit6.ValueMember = "ID";
			repositoryItemGridLookUpEdit6.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit6.PopulateViewColumns();
			foreach (GridColumn column6 in repositoryItemGridLookUpEdit6.View.Columns)
			{
				column6.Visible = false;
			}
			repositoryItemGridLookUpEdit6.View.Columns[1].Caption = _bankaevrakdataset.dropbox6_baslik;
			repositoryItemGridLookUpEdit6.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit6.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit6.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit6.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit6.AutoComplete = false;
			gc_satir_dropbox6.ColumnEdit = repositoryItemGridLookUpEdit6;
			DataTable dataTable7 = new DataTable();
			dataTable7.Columns.Add("ID", typeof(string));
			dataTable7.Columns.Add("Isim", typeof(string));
			string[] array13 = _bankaevrakdataset.dropbox7_secenekler_veri.Split(',');
			string[] array14 = _bankaevrakdataset.dropbox7_secenekler_yazi.Split(',');
			for (int num = 0; num < array13.Length; num++)
			{
				dataTable7.Rows.Add(array13[num], array14[num]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit7 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit7.DataSource = dataTable7;
			repositoryItemGridLookUpEdit7.ValueMember = "ID";
			repositoryItemGridLookUpEdit7.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit7.PopulateViewColumns();
			foreach (GridColumn column7 in repositoryItemGridLookUpEdit7.View.Columns)
			{
				column7.Visible = false;
			}
			repositoryItemGridLookUpEdit7.View.Columns[1].Caption = _bankaevrakdataset.dropbox7_baslik;
			repositoryItemGridLookUpEdit7.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit7.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit7.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit7.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit7.AutoComplete = false;
			gc_satir_dropbox7.ColumnEdit = repositoryItemGridLookUpEdit7;
			DataTable dataTable8 = new DataTable();
			dataTable8.Columns.Add("ID", typeof(string));
			dataTable8.Columns.Add("Isim", typeof(string));
			string[] array15 = _bankaevrakdataset.dropbox8_secenekler_veri.Split(',');
			string[] array16 = _bankaevrakdataset.dropbox8_secenekler_yazi.Split(',');
			for (int num2 = 0; num2 < array15.Length; num2++)
			{
				dataTable8.Rows.Add(array15[num2], array16[num2]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit8 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit8.DataSource = dataTable8;
			repositoryItemGridLookUpEdit8.ValueMember = "ID";
			repositoryItemGridLookUpEdit8.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit8.PopulateViewColumns();
			foreach (GridColumn column8 in repositoryItemGridLookUpEdit8.View.Columns)
			{
				column8.Visible = false;
			}
			repositoryItemGridLookUpEdit8.View.Columns[1].Caption = _bankaevrakdataset.dropbox8_baslik;
			repositoryItemGridLookUpEdit8.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit8.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit8.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit8.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit8.AutoComplete = false;
			gc_satir_dropbox8.ColumnEdit = repositoryItemGridLookUpEdit8;
			DataTable dataTable9 = new DataTable();
			dataTable9.Columns.Add("ID", typeof(string));
			dataTable9.Columns.Add("Isim", typeof(string));
			string[] array17 = _bankaevrakdataset.dropbox9_secenekler_veri.Split(',');
			string[] array18 = _bankaevrakdataset.dropbox9_secenekler_yazi.Split(',');
			for (int num3 = 0; num3 < array17.Length; num3++)
			{
				dataTable9.Rows.Add(array17[num3], array18[num3]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit9 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit9.DataSource = dataTable9;
			repositoryItemGridLookUpEdit9.ValueMember = "ID";
			repositoryItemGridLookUpEdit9.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit9.PopulateViewColumns();
			foreach (GridColumn column9 in repositoryItemGridLookUpEdit9.View.Columns)
			{
				column9.Visible = false;
			}
			repositoryItemGridLookUpEdit9.View.Columns[1].Caption = _bankaevrakdataset.dropbox9_baslik;
			repositoryItemGridLookUpEdit9.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit9.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit9.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit9.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit9.AutoComplete = false;
			gc_satir_dropbox9.ColumnEdit = repositoryItemGridLookUpEdit9;
			DataTable dataTable10 = new DataTable();
			dataTable10.Columns.Add("ID", typeof(string));
			dataTable10.Columns.Add("Isim", typeof(string));
			string[] array19 = _bankaevrakdataset.dropbox10_secenekler_veri.Split(',');
			string[] array20 = _bankaevrakdataset.dropbox10_secenekler_yazi.Split(',');
			for (int num4 = 0; num4 < array19.Length; num4++)
			{
				dataTable10.Rows.Add(array19[num4], array20[num4]);
			}
			RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit10 = new RepositoryItemGridLookUpEdit();
			repositoryItemGridLookUpEdit10.DataSource = dataTable10;
			repositoryItemGridLookUpEdit10.ValueMember = "ID";
			repositoryItemGridLookUpEdit10.DisplayMember = "Isim";
			repositoryItemGridLookUpEdit10.PopulateViewColumns();
			foreach (GridColumn column10 in repositoryItemGridLookUpEdit10.View.Columns)
			{
				column10.Visible = false;
			}
			repositoryItemGridLookUpEdit10.View.Columns[1].Caption = _bankaevrakdataset.dropbox10_baslik;
			repositoryItemGridLookUpEdit10.View.Columns[1].Visible = true;
			repositoryItemGridLookUpEdit10.BestFitMode = BestFitMode.BestFitResizePopup;
			repositoryItemGridLookUpEdit10.ValidateOnEnterKey = true;
			repositoryItemGridLookUpEdit10.TextEditStyle = TextEditStyles.Standard;
			repositoryItemGridLookUpEdit10.AutoComplete = false;
			gc_satir_dropbox10.ColumnEdit = repositoryItemGridLookUpEdit10;
		}
		catch
		{
		}
	}

	private void kaydetToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (_bankaevrakdataset._DosyaAdi != "")
		{
			Kaydet(_bankaevrakdataset._DosyaAdi);
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
			BankaEvrakDataSet bankaEvrakDataSet = new BankaEvrakDataSet();
			bankaEvrakDataSet._CalismaTipi = BankaEvrakCalismaTipi.Serbest;
			_bankaevrakdataset = bankaEvrakDataSet;
			bindData();
		}
	}

	private void FarkliKaydet()
	{
		saveFileDialog1.Filter = "Genel Evrak Dosyaları|*.fbe|Tüm dosyalar|*.*";
		if (saveFileDialog1.ShowDialog() == DialogResult.OK)
		{
			_bankaevrakdataset._DosyaAdi = saveFileDialog1.FileName;
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
			BankaEvrakDataSet bankaEvrakDataSet = new BankaEvrakDataSet();
			bankaEvrakDataSet.dataset = _bankaevrakdataset.dataset.Copy();
			bankaEvrakDataSet._AktarimDosyasiAcilisBakiyesi = _bankaevrakdataset._AktarimDosyasiAcilisBakiyesi;
			bankaEvrakDataSet._AktarimDosyasiIlkTarih = _bankaevrakdataset._AktarimDosyasiIlkTarih;
			bankaEvrakDataSet._AktarimDosyasiKapanisBakiyesi = _bankaevrakdataset._AktarimDosyasiKapanisBakiyesi;
			bankaEvrakDataSet._AktarimDosyasiSonTarih = _bankaevrakdataset._AktarimDosyasiSonTarih;
			bankaEvrakDataSet._ArsivKlasoru = _bankaevrakdataset._ArsivKlasoru;
			bankaEvrakDataSet._CalismaTipi = _bankaevrakdataset._CalismaTipi;
			bankaEvrakDataSet._DosyaAdi = _bankaevrakdataset._DosyaAdi;
			bankaEvrakDataSet.banka = _bankaevrakdataset.banka;
			bankaEvrakDataSet.metin1_baslik = _bankaevrakdataset.metin1_baslik;
			bankaEvrakDataSet.metin2_baslik = _bankaevrakdataset.metin2_baslik;
			bankaEvrakDataSet.metin3_baslik = _bankaevrakdataset.metin3_baslik;
			bankaEvrakDataSet.metin4_baslik = _bankaevrakdataset.metin4_baslik;
			bankaEvrakDataSet.metin5_baslik = _bankaevrakdataset.metin5_baslik;
			bankaEvrakDataSet.metin6_baslik = _bankaevrakdataset.metin6_baslik;
			bankaEvrakDataSet.metin7_baslik = _bankaevrakdataset.metin7_baslik;
			bankaEvrakDataSet.metin8_baslik = _bankaevrakdataset.metin8_baslik;
			bankaEvrakDataSet.metin9_baslik = _bankaevrakdataset.metin9_baslik;
			bankaEvrakDataSet.metin10_baslik = _bankaevrakdataset.metin10_baslik;
			bankaEvrakDataSet.metin11_baslik = _bankaevrakdataset.metin11_baslik;
			bankaEvrakDataSet.metin12_baslik = _bankaevrakdataset.metin12_baslik;
			bankaEvrakDataSet.metin13_baslik = _bankaevrakdataset.metin13_baslik;
			bankaEvrakDataSet.metin14_baslik = _bankaevrakdataset.metin14_baslik;
			bankaEvrakDataSet.metin15_baslik = _bankaevrakdataset.metin15_baslik;
			bankaEvrakDataSet.metin16_baslik = _bankaevrakdataset.metin16_baslik;
			bankaEvrakDataSet.metin17_baslik = _bankaevrakdataset.metin17_baslik;
			bankaEvrakDataSet.metin18_baslik = _bankaevrakdataset.metin18_baslik;
			bankaEvrakDataSet.metin19_baslik = _bankaevrakdataset.metin19_baslik;
			bankaEvrakDataSet.metin20_baslik = _bankaevrakdataset.metin20_baslik;
			bankaEvrakDataSet.metin21_baslik = _bankaevrakdataset.metin21_baslik;
			bankaEvrakDataSet.metin22_baslik = _bankaevrakdataset.metin22_baslik;
			bankaEvrakDataSet.metin23_baslik = _bankaevrakdataset.metin23_baslik;
			bankaEvrakDataSet.metin24_baslik = _bankaevrakdataset.metin24_baslik;
			bankaEvrakDataSet.metin25_baslik = _bankaevrakdataset.metin25_baslik;
			bankaEvrakDataSet.metin26_baslik = _bankaevrakdataset.metin26_baslik;
			bankaEvrakDataSet.metin27_baslik = _bankaevrakdataset.metin27_baslik;
			bankaEvrakDataSet.metin28_baslik = _bankaevrakdataset.metin28_baslik;
			bankaEvrakDataSet.metin29_baslik = _bankaevrakdataset.metin29_baslik;
			bankaEvrakDataSet.metin30_baslik = _bankaevrakdataset.metin30_baslik;
			bankaEvrakDataSet.metin31_baslik = _bankaevrakdataset.metin31_baslik;
			bankaEvrakDataSet.metin32_baslik = _bankaevrakdataset.metin32_baslik;
			bankaEvrakDataSet.metin33_baslik = _bankaevrakdataset.metin33_baslik;
			bankaEvrakDataSet.metin34_baslik = _bankaevrakdataset.metin34_baslik;
			bankaEvrakDataSet.metin35_baslik = _bankaevrakdataset.metin35_baslik;
			bankaEvrakDataSet.metin36_baslik = _bankaevrakdataset.metin36_baslik;
			bankaEvrakDataSet.metin37_baslik = _bankaevrakdataset.metin37_baslik;
			bankaEvrakDataSet.metin38_baslik = _bankaevrakdataset.metin38_baslik;
			bankaEvrakDataSet.metin39_baslik = _bankaevrakdataset.metin39_baslik;
			bankaEvrakDataSet.metin40_baslik = _bankaevrakdataset.metin40_baslik;
			bankaEvrakDataSet.metin41_baslik = _bankaevrakdataset.metin41_baslik;
			bankaEvrakDataSet.metin42_baslik = _bankaevrakdataset.metin42_baslik;
			bankaEvrakDataSet.metin43_baslik = _bankaevrakdataset.metin43_baslik;
			bankaEvrakDataSet.metin44_baslik = _bankaevrakdataset.metin44_baslik;
			bankaEvrakDataSet.metin45_baslik = _bankaevrakdataset.metin45_baslik;
			bankaEvrakDataSet.metin46_baslik = _bankaevrakdataset.metin46_baslik;
			bankaEvrakDataSet.metin47_baslik = _bankaevrakdataset.metin47_baslik;
			bankaEvrakDataSet.metin48_baslik = _bankaevrakdataset.metin48_baslik;
			bankaEvrakDataSet.metin49_baslik = _bankaevrakdataset.metin49_baslik;
			bankaEvrakDataSet.metin50_baslik = _bankaevrakdataset.metin50_baslik;
			bankaEvrakDataSet.dropbox1_baslik = _bankaevrakdataset.dropbox1_baslik;
			bankaEvrakDataSet.dropbox2_baslik = _bankaevrakdataset.dropbox2_baslik;
			bankaEvrakDataSet.dropbox3_baslik = _bankaevrakdataset.dropbox3_baslik;
			bankaEvrakDataSet.dropbox4_baslik = _bankaevrakdataset.dropbox4_baslik;
			bankaEvrakDataSet.dropbox5_baslik = _bankaevrakdataset.dropbox5_baslik;
			bankaEvrakDataSet.dropbox6_baslik = _bankaevrakdataset.dropbox6_baslik;
			bankaEvrakDataSet.dropbox7_baslik = _bankaevrakdataset.dropbox7_baslik;
			bankaEvrakDataSet.dropbox8_baslik = _bankaevrakdataset.dropbox8_baslik;
			bankaEvrakDataSet.dropbox9_baslik = _bankaevrakdataset.dropbox9_baslik;
			bankaEvrakDataSet.dropbox10_baslik = _bankaevrakdataset.dropbox10_baslik;
			bankaEvrakDataSet.checkbox1_baslik = _bankaevrakdataset.checkbox1_baslik;
			bankaEvrakDataSet.checkbox2_baslik = _bankaevrakdataset.checkbox2_baslik;
			bankaEvrakDataSet.checkbox3_baslik = _bankaevrakdataset.checkbox3_baslik;
			bankaEvrakDataSet.checkbox4_baslik = _bankaevrakdataset.checkbox4_baslik;
			bankaEvrakDataSet.checkbox5_baslik = _bankaevrakdataset.checkbox5_baslik;
			bankaEvrakDataSet.checkbox6_baslik = _bankaevrakdataset.checkbox6_baslik;
			bankaEvrakDataSet.checkbox7_baslik = _bankaevrakdataset.checkbox7_baslik;
			bankaEvrakDataSet.checkbox8_baslik = _bankaevrakdataset.checkbox8_baslik;
			bankaEvrakDataSet.checkbox9_baslik = _bankaevrakdataset.checkbox9_baslik;
			bankaEvrakDataSet.checkbox10_baslik = _bankaevrakdataset.checkbox10_baslik;
			bankaEvrakDataSet.dropbox1_secenekler_veri = _bankaevrakdataset.dropbox1_secenekler_veri;
			bankaEvrakDataSet.dropbox1_secenekler_yazi = _bankaevrakdataset.dropbox1_secenekler_yazi;
			bankaEvrakDataSet.dropbox2_secenekler_veri = _bankaevrakdataset.dropbox2_secenekler_veri;
			bankaEvrakDataSet.dropbox2_secenekler_yazi = _bankaevrakdataset.dropbox2_secenekler_yazi;
			bankaEvrakDataSet.dropbox3_secenekler_veri = _bankaevrakdataset.dropbox3_secenekler_veri;
			bankaEvrakDataSet.dropbox3_secenekler_yazi = _bankaevrakdataset.dropbox3_secenekler_yazi;
			bankaEvrakDataSet.dropbox4_secenekler_veri = _bankaevrakdataset.dropbox4_secenekler_veri;
			bankaEvrakDataSet.dropbox4_secenekler_yazi = _bankaevrakdataset.dropbox4_secenekler_yazi;
			bankaEvrakDataSet.dropbox5_secenekler_veri = _bankaevrakdataset.dropbox5_secenekler_veri;
			bankaEvrakDataSet.dropbox5_secenekler_yazi = _bankaevrakdataset.dropbox5_secenekler_yazi;
			bankaEvrakDataSet.dropbox6_secenekler_veri = _bankaevrakdataset.dropbox6_secenekler_veri;
			bankaEvrakDataSet.dropbox6_secenekler_yazi = _bankaevrakdataset.dropbox6_secenekler_yazi;
			bankaEvrakDataSet.dropbox7_secenekler_veri = _bankaevrakdataset.dropbox7_secenekler_veri;
			bankaEvrakDataSet.dropbox7_secenekler_yazi = _bankaevrakdataset.dropbox7_secenekler_yazi;
			bankaEvrakDataSet.dropbox8_secenekler_veri = _bankaevrakdataset.dropbox8_secenekler_veri;
			bankaEvrakDataSet.dropbox8_secenekler_yazi = _bankaevrakdataset.dropbox8_secenekler_yazi;
			bankaEvrakDataSet.dropbox9_secenekler_veri = _bankaevrakdataset.dropbox9_secenekler_veri;
			bankaEvrakDataSet.dropbox9_secenekler_yazi = _bankaevrakdataset.dropbox9_secenekler_yazi;
			bankaEvrakDataSet.dropbox10_secenekler_veri = _bankaevrakdataset.dropbox10_secenekler_veri;
			bankaEvrakDataSet.dropbox10_secenekler_yazi = _bankaevrakdataset.dropbox10_secenekler_yazi;
			foreach (DataRow row in bankaEvrakDataSet.dataset.Tables["Satirlar"].Rows)
			{
				row["TempCari"] = "";
			}
			byte[] bytes = GenelUtility.ToByteArrayGzip(JSON.Instance.ToJSON(bankaEvrakDataSet));
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

	private void satirlar_kolonlaraGoreGruplamaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (advBandedGridView_satirlar.OptionsView.ShowGroupPanel)
		{
			advBandedGridView_satirlar.OptionsView.ShowGroupPanel = false;
		}
		else
		{
			advBandedGridView_satirlar.OptionsView.ShowGroupPanel = true;
		}
	}

	private void satirlar_kolonSeciciyiGosterToolStripMenuItem_Click(object sender, EventArgs e)
	{
		advBandedGridView_satirlar.ShowCustomization();
	}

	private void satirlar_butunGruplariAcToolStripMenuItem_Click(object sender, EventArgs e)
	{
		advBandedGridView_satirlar.ExpandAllGroups();
	}

	private void satirlar_butunGruplariKapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		advBandedGridView_satirlar.CollapseAllGroups();
	}

	private void satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		advBandedGridView_satirlar.SaveLayoutToXml("data\\views\\" + _tag + ".lgs");
	}

	private void satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		saveFileDialog.Filter = "Genel Evrak Satır Görünüm |*.lgm|Tüm dosyalar|*.*";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			advBandedGridView_satirlar.SaveLayoutToXml(saveFileDialog.FileName);
		}
	}

	private void satirlar_otomatikDosyadan_yukle_ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".lgs"))
		{
			advBandedGridView_satirlar.RestoreLayoutFromXml("data\\views\\" + _tag + ".lgs");
		}
	}

	private void satirlar_farkliDosyadan_yukle_ToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\data\\views";
		openFileDialog.Filter = "Genel Evrak Satır Görünüm |*.lgs|Tüm dosyalar|*.*";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			advBandedGridView_satirlar.RestoreLayoutFromXml(openFileDialog.FileName);
		}
	}

	private void satirlar_otomatikDosyayiSilToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (File.Exists("data\\views\\" + _tag + ".lgs"))
		{
			File.Delete("data\\views\\" + _tag + ".lgs");
		}
	}

	private void satirlar_varsayilanaGeriDonToolStripMenuItem_Click(object sender, EventArgs e)
	{
		satirlar_defaultlayoutStream.Position = 0L;
		advBandedGridView_satirlar.RestoreLayoutFromStream(satirlar_defaultlayoutStream);
	}

	private void yeniEvrakEkleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		int newEvrakID = _bankaevrakdataset.NewEvrakID;
		bool flag = true;
		enum_GenelEvrakAktarimDurumu enum_GenelEvrakAktarimDurumu = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
		int num = 0;
		DateTime now = DateTime.Now;
		string text = "";
		DateTime now2 = DateTime.Now;
		double dov_fiyat = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, _bankaevrakdataset.banka.ban_doviz_cinsi, "1", now, _mikrouygulamabilgileri.MikroAnaDBName).dov_fiyat;
		enum_GenelEvrakTipleri enum_GenelEvrakTipleri = enum_GenelEvrakTipleri.GelenHavale;
		string text2 = "";
		string text3 = "";
		string text4 = "";
		string text5 = "";
		string text6 = "";
		_bankaevrakdataset.dataset.Tables["Evraklar"].Rows.Add(newEvrakID, flag, enum_GenelEvrakAktarimDurumu, enum_GenelEvrakTipleri, text2, num, now, text, now2, dov_fiyat, text3, text4, text5, text6);
	}

	private void aktifSatirinHesapKodunuOnaylaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!(gridControl_evraklar.FocusedView.Name == "advBandedGridView_master"))
		{
			DataRow focusedDataRow = ((AdvBandedGridView)gridControl_evraklar.FocusedView).GetFocusedDataRow();
			if (focusedDataRow != null)
			{
				focusedDataRow[27] = true;
			}
		}
	}

	private void ustAlaniKopyalaToolStripMenuItem_Click(object sender, EventArgs e)
	{
		AdvBandedGridView advBandedGridView = (AdvBandedGridView)gridControl_evraklar.FocusedView;
		advBandedGridView.GetFocusedDataRow();
		Console.WriteLine("aktifview.FocusedRowHandle : " + advBandedGridView.FocusedRowHandle);
		if (advBandedGridView.FocusedRowHandle > 0)
		{
			_ = advBandedGridView.FocusedRowHandle;
			DataRow dataRow = advBandedGridView.GetDataRow(advBandedGridView.FocusedRowHandle);
			DataRow dataRow2 = advBandedGridView.GetDataRow(advBandedGridView.FocusedRowHandle - 1);
			GridColumn focusedColumn = advBandedGridView.FocusedColumn;
			bool flag = false;
			string name = focusedColumn.Name;
			switch (name)
			{
			case "gc_satir_HesapKodu":
			case "gc_satir_Cinsi":
			case "gc_satir_HesapAdi":
				dataRow[2] = dataRow2[2];
				dataRow[3] = dataRow2[3];
				flag = true;
				break;
			}
			if (name == "gc_satir_FaturaSorumlulukMerkeziKodu" || name == "gc_satir_FaturaSorumlulukMerkeziAdi")
			{
				dataRow[11] = dataRow2[11];
				flag = true;
			}
			if (name == "gc_satir_FaturaProjeKodu" || name == "gc_satir_FaturaProjeAdi")
			{
				dataRow[12] = dataRow2[12];
				flag = true;
			}
			if (name == "gc_satir_SatirAciklama")
			{
				dataRow[5] = dataRow2[5];
				flag = true;
			}
			if (name == "gc_satir_FaturaSeri")
			{
				dataRow[9] = dataRow2[9];
				flag = true;
			}
			if (name == "gc_satir_FaturaSira")
			{
				dataRow[10] = dataRow2[10];
				flag = true;
			}
			switch (name)
			{
			case "gc_satir_FaturaDurumu":
			case "gc_satir_FaturaHesapKodu":
			case "gc_satir_FaturaHesapAdi":
				dataRow[6] = dataRow2[6];
				dataRow[7] = dataRow2[7];
				flag = true;
				break;
			}
			if (name == "gc_satir_FaturaMiktar")
			{
				dataRow[8] = dataRow2[8];
				flag = true;
			}
			if (flag)
			{
				advBandedGridView.RefreshData();
			}
		}
		advBandedGridView.MoveNext();
	}

	private void aramaTablolarininVerileriniGuncelleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Cursor.Current = Cursors.WaitCursor;
		RepositoryItemlariGuncelle();
		Cursor.Current = Cursors.Default;
	}

	private void varsayilanDegerlerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		MessageBox.Show("YAPIM AŞAMASINDA");
	}

	private void akisParametreleriToolStripMenuItem_Click(object sender, EventArgs e)
	{
		MessageBox.Show("YAPIM AŞAMASINDA");
	}

	private void sb_Secili_Evraklari_aktar_Click(object sender, EventArgs e)
	{
		AktarilacakEvrakSayisi = 0;
		KontrolSonucu = true;
		foreach (DataRow row in _bankaevrakdataset.dataset.Tables[0].Rows)
		{
			if ((bool)row[1])
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
		foreach (DataRow row in _bankaevrakdataset.dataset.Tables[0].Rows)
		{
			if ((bool)row[1])
			{
				TekEvrakKontrolEt(row);
				num++;
				backgroundWorker_Kontrol.ReportProgress(num);
			}
		}
	}

	private void TekEvrakKontrolEt(DataRow evrakrow)
	{
		evrakrow[2] = enum_GenelEvrakAktarimDurumu.Aktarilmamis;
		if (evrakrow.GetChildRows(_bankaevrakdataset.dataset.Relations[0]).GetLength(0) == 0)
		{
			evrakrow[2] = enum_GenelEvrakAktarimDurumu.SatirlardaSorunVar;
			KontrolSonucu = false;
		}
		DataRow[] childRows = evrakrow.GetChildRows(_bankaevrakdataset.dataset.Relations[0]);
		foreach (DataRow dataRow in childRows)
		{
			string text = (string)dataRow[3];
			if (text == "" || text == null)
			{
				evrakrow[2] = enum_GenelEvrakAktarimDurumu.SatirlardaSorunVar;
				KontrolSonucu = false;
			}
			if ((enum_FaturaOlusturmaDurumu)dataRow[6] != enum_FaturaOlusturmaDurumu.Olusturma)
			{
				string text2 = (string)dataRow[7];
				if (text2 == "" || text2 == null)
				{
					evrakrow[2] = enum_GenelEvrakAktarimDurumu.SatirlardaSorunVar;
					KontrolSonucu = false;
				}
			}
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
			label_islem_adi.Text = "Aktarılıyor";
			progressBar1.Maximum = AktarilacakEvrakSayisi;
			progressBar1.Value = 0;
			backgroundWorker_Aktarim.RunWorkerAsync();
		}
		else
		{
			IslemiBitir();
			label_islem_adi.Text = "Hata var.";
			MessageBox.Show("Evraklarda hatalar var. Lütfen hataları düzeltip tekrar deneyiniz!");
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

	private void TekEvrakAktar(DataRow evrakrow)
	{
		if (Db.Connection.State != ConnectionState.Open)
		{
			Db.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		enum_GenelEvrakTipleri evrakTip = (enum_GenelEvrakTipleri)evrakrow[3];
		enum_KapamaSekli kapamaSekli = enum_KapamaSekli.AcikHesap;
		enum_cha_normal_Iade normalIade = enum_cha_normal_Iade.Normal;
		enum_cha_ticaret_turu ticaretTuru = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
		Evrak evrak = new Evrak(evrakTip, YeniKayitMi: true, EvrakKilitliMi: false, kapamaSekli, normalIade, ticaretTuru);
		evrak.SetBelgeNo((string)evrakrow[7]);
		evrak.SetBelgeTarihi((DateTime)evrakrow[8]);
		evrak.SetDegistirSpecialAlan1((string)evrakrow[11]);
		evrak.SetDegistirSpecialAlan2((string)evrakrow[12]);
		evrak.SetDegistirSpecialAlan3((string)evrakrow[13]);
		evrak.SetDovizCinsi(_bankaevrakdataset.banka.ban_doviz_cinsi);
		evrak.SetEvrakNoSeri((string)evrakrow[4]);
		evrak.SetEvraknoSira((int)evrakrow[5]);
		evrak.SetEvrakTarihi((DateTime)evrakrow[6]);
		evrak.SetFirma(FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.FirmaNo));
		evrak.SetFiyatListesi(new FiyatListesi());
		evrak.SetKapamaHesapKodu("");
		evrak.kur = new Kur();
		evrak.kur.dov_fiyat = (double)evrakrow[9];
		evrak.kur.dov_no = evrak.dovizcinsi;
		evrak.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
		evrak.SetOdemePlani(0);
		evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)evrakrow[10]));
		evrak.SetSube(SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.SubeNo));
		evrak.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
		evrak.alternatifdovizkuru = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, evrak.alternatifdovizcinsi, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
		double num = 0.0;
		DataRow[] childRows = evrakrow.GetChildRows(_bankaevrakdataset.dataset.Relations[0]);
		foreach (DataRow dataRow in childRows)
		{
			if (!(bool)dataRow[33])
			{
				continue;
			}
			enum_BankaSatirCinsi enum_BankaSatirCinsi = (enum_BankaSatirCinsi)dataRow[2];
			if (evrak.evraktipi != enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu)
			{
				string text = ((evrak.evraktipi != enum_GenelEvrakTipleri.GelenHavale && evrak.evraktipi != enum_GenelEvrakTipleri.GidenHavale) ? _bankaevrakdataset.banka.ban_kod : ((string)dataRow[3]));
				string cha_kasa_hizkod = _bankaevrakdataset.banka.ban_kod;
				if (evrak.evraktipi == enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu || evrak.evraktipi == enum_GenelEvrakTipleri.KasadanBankayaNakitYatirmaMakbuzu)
				{
					cha_kasa_hizkod = (string)dataRow[3];
				}
				if (evrak.evraktipi == enum_GenelEvrakTipleri.TahsileCekCikisBordrosu)
				{
					cha_kasa_hizkod = OdemeEmirleriData.GetOdemeEmiriNeredeCariKodu(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)dataRow[3]);
				}
				if (evrak.evraktipi == enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu)
				{
					cha_kasa_hizkod = OdemeEmirleriData.GetOdemeEmiriNeredeCariKodu(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)dataRow[3]);
				}
				if (evrak.evraktipi == enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu)
				{
					cha_kasa_hizkod = OdemeEmirleriData.GetOdemeEmiriSahipCariKodu(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)dataRow[3]);
				}
				enum_cha_tpoz cha_tpoz = enum_cha_tpoz.Acik;
				DateTime cha_d_kurtar = new DateTime(1899, 12, 30);
				string cha_aciklama = (string)dataRow[5];
				string text2 = "";
				string text3 = "";
				if (evrak.evraktipi == enum_GenelEvrakTipleri.GelenHavale || evrak.evraktipi == enum_GenelEvrakTipleri.GidenHavale)
				{
					text2 = (string)dataRow[11];
					text3 = evrak.sorumlulukmerkezi.som_kod;
				}
				else
				{
					text2 = evrak.sorumlulukmerkezi.som_kod;
					text3 = (string)dataRow[11];
				}
				string cha_projekodu = (string)dataRow[12];
				string text4 = "";
				text4 = ((evrak.evraktipi != enum_GenelEvrakTipleri.GelenHavale && evrak.evraktipi != enum_GenelEvrakTipleri.GidenHavale && evrak.evraktipi != enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu && evrak.evraktipi != enum_GenelEvrakTipleri.KasadanBankayaNakitYatirmaMakbuzu) ? ((string)dataRow[3]) : "");
				enum_cha_kasa_hizmet cha_kasa_hizmet = enum_cha_kasa_hizmet.Bankamiz;
				if (evrak.evraktipi == enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu || evrak.evraktipi == enum_GenelEvrakTipleri.KasadanBankayaNakitYatirmaMakbuzu || evrak.evraktipi == enum_GenelEvrakTipleri.TahsileCekCikisBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu)
				{
					cha_kasa_hizmet = enum_cha_kasa_hizmet.Kasamiz;
				}
				enum_cha_sntck_poz cha_sntck_poz = enum_cha_sntck_poz.Portfoyde;
				if (evrak.evraktipi == enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.TahsildekiSenetOdemeBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu)
				{
					cha_sntck_poz = enum_cha_sntck_poz.Odendi;
				}
				if (evrak.evraktipi == enum_GenelEvrakTipleri.TahsileCekCikisBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu)
				{
					cha_sntck_poz = enum_cha_sntck_poz.Tahsilde;
				}
				int num2 = (int)dataRow[32];
				int ban_doviz_cinsi = _bankaevrakdataset.banka.ban_doviz_cinsi;
				double num3 = (double)evrakrow[9];
				int cha_karsidgrupno = 1;
				switch (evrak.evraktipi)
				{
				case enum_GenelEvrakTipleri.GelenHavale:
					cha_karsidgrupno = 1;
					break;
				case enum_GenelEvrakTipleri.GidenHavale:
					cha_karsidgrupno = 1;
					break;
				case enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu:
					cha_karsidgrupno = 0;
					break;
				case enum_GenelEvrakTipleri.KasadanBankayaNakitYatirmaMakbuzu:
					cha_karsidgrupno = 0;
					break;
				case enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu:
					cha_karsidgrupno = 1;
					break;
				case enum_GenelEvrakTipleri.TahsildekiSenetOdemeBordrosu:
					cha_karsidgrupno = 1;
					break;
				case enum_GenelEvrakTipleri.TahsileCekCikisBordrosu:
					cha_karsidgrupno = 0;
					break;
				case enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu:
					cha_karsidgrupno = 0;
					break;
				case enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu:
					cha_karsidgrupno = 1;
					break;
				case enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu:
					cha_karsidgrupno = 0;
					break;
				}
				double num4 = 1.0;
				int num5 = 0;
				switch (enum_BankaSatirCinsi)
				{
				case enum_BankaSatirCinsi.Banka:
					num5 = BankaData.GetBanka(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text).ban_doviz_cinsi;
					break;
				case enum_BankaSatirCinsi.CariHesap:
				{
					Cari cariByCariKod = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "");
					if (num2 == 0)
					{
						num5 = cariByCariKod.cari_doviz_cinsi;
					}
					if (num2 == 1)
					{
						num5 = cariByCariKod.cari_doviz_cinsi1;
					}
					if (num2 == 2)
					{
						num5 = cariByCariKod.cari_doviz_cinsi2;
					}
					break;
				}
				case enum_BankaSatirCinsi.CariPersonel:
					num5 = CariPersonelData.GetCariPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text).cari_per_doviz_cinsi;
					break;
				case enum_BankaSatirCinsi.Demirbas:
					num5 = DemirbasData.GetDemirbas(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text).dem_doviz_cinsi;
					break;
				case enum_BankaSatirCinsi.Hizmet:
					num5 = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text).hiz_doviz_cinsi;
					break;
				case enum_BankaSatirCinsi.Kasa:
					num5 = KasaData.GetKasa(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text).kas_doviz_cinsi;
					break;
				case enum_BankaSatirCinsi.Masraf:
					num5 = MasrafData.GetMasraf(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text).his_dovcinsi;
					break;
				case enum_BankaSatirCinsi.MuhasebeHesabi:
					num5 = 0;
					break;
				case enum_BankaSatirCinsi.Personel:
					num5 = PersonelData.GetPersonel(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, text).per_doviz_cinsi;
					break;
				}
				num4 = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, num5, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName).dov_fiyat;
				double num6 = (double)dataRow[4] * num3 / num4;
				_ = (double)dataRow[4];
				enum_cha_cari_cins cha_cari_cins = enum_cha_cari_cins.Carimiz;
				string cha_satici_kodu = "";
				if (evrak.evraktipi == enum_GenelEvrakTipleri.GelenHavale || evrak.evraktipi == enum_GenelEvrakTipleri.GidenHavale)
				{
					switch (enum_BankaSatirCinsi)
					{
					case enum_BankaSatirCinsi.Banka:
						cha_cari_cins = enum_cha_cari_cins.Bankamiz;
						break;
					case enum_BankaSatirCinsi.CariHesap:
						cha_cari_cins = enum_cha_cari_cins.Carimiz;
						cha_satici_kodu = CariData.GetCariByCariKod(Db.Connection, text, AdreslerTemsilciyeGore: false, "").cari_temsilci_kodu;
						break;
					case enum_BankaSatirCinsi.CariPersonel:
						cha_cari_cins = enum_cha_cari_cins.CariPersonelimiz;
						break;
					case enum_BankaSatirCinsi.Demirbas:
						cha_cari_cins = enum_cha_cari_cins.Demirbasimiz;
						break;
					case enum_BankaSatirCinsi.Hizmet:
						cha_cari_cins = enum_cha_cari_cins.Hizmetimiz;
						break;
					case enum_BankaSatirCinsi.Ithalat:
						cha_cari_cins = enum_cha_cari_cins.IthalatDosyamiz;
						break;
					case enum_BankaSatirCinsi.Kasa:
						cha_cari_cins = enum_cha_cari_cins.Kasamiz;
						break;
					case enum_BankaSatirCinsi.Masraf:
						cha_cari_cins = enum_cha_cari_cins.Giderimiz;
						break;
					case enum_BankaSatirCinsi.MuhasebeHesabi:
						cha_cari_cins = enum_cha_cari_cins.MuhasebeHesabimiz;
						break;
					case enum_BankaSatirCinsi.Personel:
						cha_cari_cins = enum_cha_cari_cins.Personelimiz;
						break;
					}
				}
				else
				{
					cha_cari_cins = enum_cha_cari_cins.Bankamiz;
				}
				CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
				switch (evrak.evraktipi)
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
				case enum_GenelEvrakTipleri.BankadanKasayaNakitCekmeMakbuzu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.ParaNakitCekme;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.Nakit;
					break;
				case enum_GenelEvrakTipleri.KasadanBankayaNakitYatirmaMakbuzu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.ParaNakitYatirma;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Borc;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.Nakit;
					break;
				case enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.TakasCekOdeme;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.MusteriCeki;
					break;
				case enum_GenelEvrakTipleri.TahsildekiSenetOdemeBordrosu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.TahsilSenetOdeme;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.MusteriSenedi;
					break;
				case enum_GenelEvrakTipleri.TahsileCekCikisBordrosu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.TakasCekCikis;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Borc;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.MusteriCeki;
					break;
				case enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.TahsilSenetCikis;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Borc;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.MusteriSenedi;
					break;
				case enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.BankadanFirmaCekOdeme;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Borc;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.FirmaCeki;
					break;
				case enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu:
					cARI_HESAP_HAREKETLERI.cha_evrak_tip = enum_cha_evrak_tip.BankadanFirmaSenetodeme;
					cARI_HESAP_HAREKETLERI.cha_tip = enum_cha_tip.Alacak;
					cARI_HESAP_HAREKETLERI.cha_cinsi = enum_cha_cinsi.FirmaSenedi;
					break;
				}
				cARI_HESAP_HAREKETLERI.cha_RECid_DBCno = evrak.DBCno;
				cARI_HESAP_HAREKETLERI.cha_create_user = evrak.mikrouserno;
				cARI_HESAP_HAREKETLERI.cha_lastup_user = evrak.mikrouserno;
				cARI_HESAP_HAREKETLERI.cha_normal_Iade = evrak.normaliade;
				cARI_HESAP_HAREKETLERI.cha_ticaret_turu = evrak.ticaretturu;
				cARI_HESAP_HAREKETLERI.cha_firmano = evrak.Firma.fir_sirano;
				cARI_HESAP_HAREKETLERI.cha_subeno = evrak.Sube.Sube_no;
				cARI_HESAP_HAREKETLERI.cha_tarihi = evrak.EvrakTarihi;
				cARI_HESAP_HAREKETLERI.cha_satir_no = evrak.GetGenelCariHesapHareketleri().Count;
				cARI_HESAP_HAREKETLERI.cha_evrakno_seri = evrak.EvrakNoSeri;
				cARI_HESAP_HAREKETLERI.cha_evrakno_sira = evrak.EvrakNoSira;
				cARI_HESAP_HAREKETLERI.cha_belge_no = evrak.BelgeNo;
				cARI_HESAP_HAREKETLERI.cha_belge_tarih = evrak.BelgeTarihi;
				cARI_HESAP_HAREKETLERI.cha_altd_kur = evrak.alternatifdovizkuru.dov_fiyat;
				string obj = evrak.EvrakTarihi.Year.ToString();
				string text5 = evrak.EvrakTarihi.Month.ToString();
				if (text5.Length == 1)
				{
					text5 = "0" + text5;
				}
				string text6 = evrak.EvrakTarihi.Day.ToString();
				if (text6.Length == 1)
				{
					text6 = "0" + text6;
				}
				int cha_vade = int.Parse(obj + text5 + text6);
				cARI_HESAP_HAREKETLERI.cha_cari_cins = cha_cari_cins;
				cARI_HESAP_HAREKETLERI.cha_kod = text;
				cARI_HESAP_HAREKETLERI.cha_satici_kodu = cha_satici_kodu;
				cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = cha_kasa_hizkod;
				cARI_HESAP_HAREKETLERI.cha_d_kurtar = cha_d_kurtar;
				cARI_HESAP_HAREKETLERI.cha_d_cins = num5;
				cARI_HESAP_HAREKETLERI.cha_d_kur = num4;
				cARI_HESAP_HAREKETLERI.cha_grupno = num2;
				cARI_HESAP_HAREKETLERI.cha_meblag = num6;
				cARI_HESAP_HAREKETLERI.cha_aratoplam = num6;
				cARI_HESAP_HAREKETLERI.cha_vade = cha_vade;
				cARI_HESAP_HAREKETLERI.cha_aciklama = cha_aciklama;
				cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = cha_kasa_hizmet;
				cARI_HESAP_HAREKETLERI.cha_tpoz = cha_tpoz;
				cARI_HESAP_HAREKETLERI.cha_trefno = text4;
				cARI_HESAP_HAREKETLERI.cha_sntck_poz = cha_sntck_poz;
				cARI_HESAP_HAREKETLERI.cha_karsidcinsi = ban_doviz_cinsi;
				cARI_HESAP_HAREKETLERI.cha_karsid_kur = num3;
				cARI_HESAP_HAREKETLERI.cha_karsidgrupno = cha_karsidgrupno;
				cARI_HESAP_HAREKETLERI.cha_srmrkkodu = text2;
				cARI_HESAP_HAREKETLERI.cha_karsisrmrkkodu = text3;
				cARI_HESAP_HAREKETLERI.cha_projekodu = cha_projekodu;
				cARI_HESAP_HAREKETLERI.cha_create_date = DateTime.Now;
				cARI_HESAP_HAREKETLERI.cha_lastup_date = DateTime.Now;
				cARI_HESAP_HAREKETLERI.cha_fis_tarih = new DateTime(1899, 12, 30);
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
				cARI_HESAP_HAREKETLERI.cha_vardiya_tarihi = new DateTime(1899, 12, 30);
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
				evrak.AddGenelCariHesapHareketleri(cARI_HESAP_HAREKETLERI);
			}
			else
			{
				string ban_kod = _bankaevrakdataset.banka.ban_kod;
				string cha_kod = (string)dataRow[3];
				enum_cha_tpoz cha_tpoz2 = enum_cha_tpoz.Acik;
				DateTime cha_d_kurtar2 = new DateTime(1899, 12, 30);
				string cha_aciklama2 = (string)dataRow[5];
				string som_kod = evrak.sorumlulukmerkezi.som_kod;
				string cha_srmrkkodu = (string)dataRow[11];
				string cha_karsisrmrkkodu = "";
				string cha_projekodu2 = (string)dataRow[12];
				string cha_trefno = "";
				enum_cha_kasa_hizmet cha_kasa_hizmet2 = enum_cha_kasa_hizmet.Carimiz;
				enum_cha_sntck_poz cha_sntck_poz2 = enum_cha_sntck_poz.Portfoyde;
				int cha_grupno = 1;
				int ban_doviz_cinsi2 = _bankaevrakdataset.banka.ban_doviz_cinsi;
				double num7 = (double)evrakrow[9];
				int cha_karsidgrupno2 = 0;
				double num8 = 1.0;
				int ban_doviz_cinsi3 = BankaData.GetBanka(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, ban_kod).ban_doviz_cinsi;
				num8 = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, ban_doviz_cinsi3, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName).dov_fiyat;
				double num9 = (double)dataRow[4] * num7 / num8;
				_ = (double)dataRow[4];
				CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI2 = new CARI_HESAP_HAREKETLERI();
				CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI3 = new CARI_HESAP_HAREKETLERI();
				cARI_HESAP_HAREKETLERI2.cha_evrak_tip = enum_cha_evrak_tip.BankalarVirmanDekontu;
				cARI_HESAP_HAREKETLERI2.cha_cinsi = enum_cha_cinsi.Dekont;
				cARI_HESAP_HAREKETLERI3.cha_evrak_tip = enum_cha_evrak_tip.BankalarVirmanDekontu;
				cARI_HESAP_HAREKETLERI3.cha_cinsi = enum_cha_cinsi.Dekont;
				if (num9 < 0.0)
				{
					cARI_HESAP_HAREKETLERI2.cha_tip = enum_cha_tip.Alacak;
					cARI_HESAP_HAREKETLERI3.cha_tip = enum_cha_tip.Borc;
				}
				else
				{
					cARI_HESAP_HAREKETLERI2.cha_tip = enum_cha_tip.Borc;
					cARI_HESAP_HAREKETLERI3.cha_tip = enum_cha_tip.Alacak;
				}
				cARI_HESAP_HAREKETLERI2.cha_RECid_DBCno = evrak.DBCno;
				cARI_HESAP_HAREKETLERI2.cha_create_user = evrak.mikrouserno;
				cARI_HESAP_HAREKETLERI2.cha_lastup_user = evrak.mikrouserno;
				cARI_HESAP_HAREKETLERI2.cha_normal_Iade = evrak.normaliade;
				cARI_HESAP_HAREKETLERI2.cha_ticaret_turu = evrak.ticaretturu;
				cARI_HESAP_HAREKETLERI2.cha_firmano = evrak.Firma.fir_sirano;
				cARI_HESAP_HAREKETLERI2.cha_subeno = evrak.Sube.Sube_no;
				cARI_HESAP_HAREKETLERI2.cha_tarihi = evrak.EvrakTarihi;
				cARI_HESAP_HAREKETLERI2.cha_satir_no = evrak.GetGenelCariHesapHareketleri().Count;
				cARI_HESAP_HAREKETLERI2.cha_evrakno_seri = evrak.EvrakNoSeri;
				cARI_HESAP_HAREKETLERI2.cha_evrakno_sira = evrak.EvrakNoSira;
				cARI_HESAP_HAREKETLERI2.cha_belge_no = evrak.BelgeNo;
				cARI_HESAP_HAREKETLERI2.cha_belge_tarih = evrak.BelgeTarihi;
				cARI_HESAP_HAREKETLERI2.cha_altd_kur = evrak.alternatifdovizkuru.dov_fiyat;
				cARI_HESAP_HAREKETLERI3.cha_RECid_DBCno = evrak.DBCno;
				cARI_HESAP_HAREKETLERI3.cha_create_user = evrak.mikrouserno;
				cARI_HESAP_HAREKETLERI3.cha_lastup_user = evrak.mikrouserno;
				cARI_HESAP_HAREKETLERI3.cha_normal_Iade = evrak.normaliade;
				cARI_HESAP_HAREKETLERI3.cha_ticaret_turu = evrak.ticaretturu;
				cARI_HESAP_HAREKETLERI3.cha_firmano = evrak.Firma.fir_sirano;
				cARI_HESAP_HAREKETLERI3.cha_subeno = evrak.Sube.Sube_no;
				cARI_HESAP_HAREKETLERI3.cha_tarihi = evrak.EvrakTarihi;
				cARI_HESAP_HAREKETLERI3.cha_satir_no = evrak.GetGenelCariHesapHareketleri().Count + 1;
				cARI_HESAP_HAREKETLERI3.cha_evrakno_seri = evrak.EvrakNoSeri;
				cARI_HESAP_HAREKETLERI3.cha_evrakno_sira = evrak.EvrakNoSira;
				cARI_HESAP_HAREKETLERI3.cha_belge_no = evrak.BelgeNo;
				cARI_HESAP_HAREKETLERI3.cha_belge_tarih = evrak.BelgeTarihi;
				cARI_HESAP_HAREKETLERI3.cha_altd_kur = evrak.alternatifdovizkuru.dov_fiyat;
				cARI_HESAP_HAREKETLERI2.cha_cari_cins = enum_cha_cari_cins.Bankamiz;
				cARI_HESAP_HAREKETLERI3.cha_cari_cins = enum_cha_cari_cins.Bankamiz;
				cARI_HESAP_HAREKETLERI2.cha_kod = ban_kod;
				cARI_HESAP_HAREKETLERI3.cha_kod = cha_kod;
				cARI_HESAP_HAREKETLERI2.cha_satici_kodu = "";
				cARI_HESAP_HAREKETLERI3.cha_satici_kodu = "";
				cARI_HESAP_HAREKETLERI2.cha_kasa_hizkod = "";
				cARI_HESAP_HAREKETLERI3.cha_kasa_hizkod = "";
				cARI_HESAP_HAREKETLERI2.cha_d_kurtar = cha_d_kurtar2;
				cARI_HESAP_HAREKETLERI3.cha_d_kurtar = cha_d_kurtar2;
				cARI_HESAP_HAREKETLERI2.cha_d_cins = ban_doviz_cinsi3;
				cARI_HESAP_HAREKETLERI3.cha_d_cins = ban_doviz_cinsi3;
				cARI_HESAP_HAREKETLERI2.cha_d_kur = num8;
				cARI_HESAP_HAREKETLERI3.cha_d_kur = num8;
				cARI_HESAP_HAREKETLERI2.cha_grupno = cha_grupno;
				cARI_HESAP_HAREKETLERI3.cha_grupno = cha_grupno;
				if (num9 > 0.0)
				{
					cARI_HESAP_HAREKETLERI2.cha_meblag = num9;
					cARI_HESAP_HAREKETLERI3.cha_meblag = num9;
				}
				else
				{
					cARI_HESAP_HAREKETLERI2.cha_meblag = num9 * -1.0;
					cARI_HESAP_HAREKETLERI3.cha_meblag = num9 * -1.0;
				}
				cARI_HESAP_HAREKETLERI2.cha_aratoplam = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_aratoplam = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vade = 0;
				cARI_HESAP_HAREKETLERI3.cha_vade = 0;
				cARI_HESAP_HAREKETLERI2.cha_aciklama = cha_aciklama2;
				cARI_HESAP_HAREKETLERI2.cha_kasa_hizmet = cha_kasa_hizmet2;
				cARI_HESAP_HAREKETLERI2.cha_tpoz = cha_tpoz2;
				cARI_HESAP_HAREKETLERI2.cha_trefno = cha_trefno;
				cARI_HESAP_HAREKETLERI2.cha_sntck_poz = cha_sntck_poz2;
				cARI_HESAP_HAREKETLERI2.cha_karsidcinsi = ban_doviz_cinsi2;
				cARI_HESAP_HAREKETLERI2.cha_karsid_kur = num7;
				cARI_HESAP_HAREKETLERI2.cha_karsidgrupno = cha_karsidgrupno2;
				cARI_HESAP_HAREKETLERI2.cha_srmrkkodu = som_kod;
				cARI_HESAP_HAREKETLERI2.cha_karsisrmrkkodu = cha_karsisrmrkkodu;
				cARI_HESAP_HAREKETLERI2.cha_projekodu = cha_projekodu2;
				cARI_HESAP_HAREKETLERI3.cha_aciklama = cha_aciklama2;
				cARI_HESAP_HAREKETLERI3.cha_kasa_hizmet = cha_kasa_hizmet2;
				cARI_HESAP_HAREKETLERI3.cha_tpoz = cha_tpoz2;
				cARI_HESAP_HAREKETLERI3.cha_trefno = cha_trefno;
				cARI_HESAP_HAREKETLERI3.cha_sntck_poz = cha_sntck_poz2;
				cARI_HESAP_HAREKETLERI3.cha_karsidcinsi = ban_doviz_cinsi2;
				cARI_HESAP_HAREKETLERI3.cha_karsid_kur = num7;
				cARI_HESAP_HAREKETLERI3.cha_karsidgrupno = cha_karsidgrupno2;
				cARI_HESAP_HAREKETLERI3.cha_srmrkkodu = cha_srmrkkodu;
				cARI_HESAP_HAREKETLERI3.cha_karsisrmrkkodu = cha_karsisrmrkkodu;
				cARI_HESAP_HAREKETLERI3.cha_projekodu = cha_projekodu2;
				cARI_HESAP_HAREKETLERI2.cha_create_date = DateTime.Now;
				cARI_HESAP_HAREKETLERI2.cha_lastup_date = DateTime.Now;
				cARI_HESAP_HAREKETLERI2.cha_fis_tarih = new DateTime(1899, 12, 30);
				cARI_HESAP_HAREKETLERI2.cha_fis_sirano = 0;
				cARI_HESAP_HAREKETLERI2.cha_ft_iskonto1 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_iskonto2 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_iskonto3 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_iskonto4 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_iskonto5 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_iskonto6 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_masraf1 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_masraf2 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_masraf3 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_ft_masraf4 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi1 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi2 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi3 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi4 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi5 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi6 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi7 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi8 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi9 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergi10 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_yuvarlama = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_reftarihi = new DateTime(1899, 12, 30);
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr1 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr2 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr3 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr4 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr5 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr6 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr7 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_odeme_arr8 = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_miktari = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergipntr = 0;
				cARI_HESAP_HAREKETLERI2.cha_istisnakodu = 0;
				cARI_HESAP_HAREKETLERI2.cha_ver_tev_carpani = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_stopaj = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_savsandesfonu = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_vergisiz_fl = false;
				cARI_HESAP_HAREKETLERI2.cha_mustahsil_borsa = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_mustahsil_bagkur = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_mustahsil_diger = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalMSDF = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalHamaliye = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalStopaj = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalKomisyonu = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_StFonPntr = 0;
				cARI_HESAP_HAREKETLERI2.cha_pos_hareketi = false;
				cARI_HESAP_HAREKETLERI2.cha_vardiya_tarihi = new DateTime(1899, 12, 30);
				cARI_HESAP_HAREKETLERI2.cha_vardiya_no = 0;
				cARI_HESAP_HAREKETLERI2.cha_vardiya_evrak_ti = enum_cha_vardita_evrak_ti.HizmetSatisi;
				cARI_HESAP_HAREKETLERI2.cha_HalRusum = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalNavlunTut = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalRehinFuture = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalKomisyon = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_Vade_Farki_Yuz = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_EXIMkodu = "";
				cARI_HESAP_HAREKETLERI2.cha_HalRehinSandikmiktari = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalSandikVrMiktar = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalSandikTutari = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalSandikKDVTutari = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalrehinSandikTutari = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_Tevkifat_turu = enum_cha_Tevkifat_turu.TevkifatYok;
				cARI_HESAP_HAREKETLERI2.cha_otvtutari = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_otvvergisiz_fl = false;
				cARI_HESAP_HAREKETLERI2.cha_sozlesme_DBCno = 0;
				cARI_HESAP_HAREKETLERI2.cha_sozlesme_RECno = 0;
				cARI_HESAP_HAREKETLERI2.cha_yat_tes_kodu = "";
				cARI_HESAP_HAREKETLERI2.cha_ciro_cari_kodu = "";
				cARI_HESAP_HAREKETLERI2.cha_oivergisiz_fl = false;
				cARI_HESAP_HAREKETLERI2.cha_meblag_ana_doviz_icin_gecersiz_fl = 0;
				cARI_HESAP_HAREKETLERI2.cha_meblag_alt_doviz_icin_gecersiz_fl = 0;
				cARI_HESAP_HAREKETLERI2.cha_meblag_orj_doviz_icin_gecersiz_fl = 0;
				cARI_HESAP_HAREKETLERI2.cha_ciroprim_DBCno = 0;
				cARI_HESAP_HAREKETLERI2.cha_ciroprim_RECno = 0;
				cARI_HESAP_HAREKETLERI2.cha_HalHamaliyeKdv = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_HalHamaliyeVergisiz_fl = false;
				cARI_HESAP_HAREKETLERI2.cha_bakimhar_DBCno = 0;
				cARI_HESAP_HAREKETLERI2.cha_bakimhar_RECno = 0;
				cARI_HESAP_HAREKETLERI2.cha_avanstalep_DBCno = 0;
				cARI_HESAP_HAREKETLERI2.cha_avanstalep_RECno = 0;
				cARI_HESAP_HAREKETLERI2.cha_oiv_pntr = 0;
				cARI_HESAP_HAREKETLERI2.cha_oiv_vergi = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_oivtutari = 0.0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas1 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas2 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas3 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas4 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas5 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas6 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas7 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas8 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas9 = 0;
				cARI_HESAP_HAREKETLERI2.cha_isk_mas10 = 0;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas1 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas2 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas3 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas4 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas5 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas6 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas7 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas8 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas9 = false;
				cARI_HESAP_HAREKETLERI2.cha_sat_iskmas10 = false;
				cARI_HESAP_HAREKETLERI3.cha_create_date = DateTime.Now;
				cARI_HESAP_HAREKETLERI3.cha_lastup_date = DateTime.Now;
				cARI_HESAP_HAREKETLERI3.cha_fis_tarih = new DateTime(1899, 12, 30);
				cARI_HESAP_HAREKETLERI3.cha_fis_sirano = 0;
				cARI_HESAP_HAREKETLERI3.cha_ft_iskonto1 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_iskonto2 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_iskonto3 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_iskonto4 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_iskonto5 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_iskonto6 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_masraf1 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_masraf2 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_masraf3 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_ft_masraf4 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi1 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi2 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi3 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi4 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi5 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi6 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi7 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi8 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi9 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergi10 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_yuvarlama = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_reftarihi = new DateTime(1899, 12, 30);
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr1 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr2 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr3 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr4 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr5 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr6 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr7 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_odeme_arr8 = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_miktari = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergipntr = 0;
				cARI_HESAP_HAREKETLERI3.cha_istisnakodu = 0;
				cARI_HESAP_HAREKETLERI3.cha_ver_tev_carpani = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_stopaj = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_savsandesfonu = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_vergisiz_fl = false;
				cARI_HESAP_HAREKETLERI3.cha_mustahsil_borsa = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_mustahsil_bagkur = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_mustahsil_diger = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalMSDF = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalHamaliye = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalStopaj = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalKomisyonu = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_StFonPntr = 0;
				cARI_HESAP_HAREKETLERI3.cha_pos_hareketi = false;
				cARI_HESAP_HAREKETLERI3.cha_vardiya_tarihi = new DateTime(1899, 12, 30);
				cARI_HESAP_HAREKETLERI3.cha_vardiya_no = 0;
				cARI_HESAP_HAREKETLERI3.cha_vardiya_evrak_ti = enum_cha_vardita_evrak_ti.HizmetSatisi;
				cARI_HESAP_HAREKETLERI3.cha_HalRusum = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalNavlunTut = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalRehinFuture = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalKomisyon = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_Vade_Farki_Yuz = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_EXIMkodu = "";
				cARI_HESAP_HAREKETLERI3.cha_HalRehinSandikmiktari = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalSandikVrMiktar = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalSandikTutari = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalSandikKDVTutari = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalrehinSandikTutari = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_Tevkifat_turu = enum_cha_Tevkifat_turu.TevkifatYok;
				cARI_HESAP_HAREKETLERI3.cha_otvtutari = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_otvvergisiz_fl = false;
				cARI_HESAP_HAREKETLERI3.cha_sozlesme_DBCno = 0;
				cARI_HESAP_HAREKETLERI3.cha_sozlesme_RECno = 0;
				cARI_HESAP_HAREKETLERI3.cha_yat_tes_kodu = "";
				cARI_HESAP_HAREKETLERI3.cha_ciro_cari_kodu = "";
				cARI_HESAP_HAREKETLERI3.cha_oivergisiz_fl = false;
				cARI_HESAP_HAREKETLERI3.cha_meblag_ana_doviz_icin_gecersiz_fl = 0;
				cARI_HESAP_HAREKETLERI3.cha_meblag_alt_doviz_icin_gecersiz_fl = 0;
				cARI_HESAP_HAREKETLERI3.cha_meblag_orj_doviz_icin_gecersiz_fl = 0;
				cARI_HESAP_HAREKETLERI3.cha_ciroprim_DBCno = 0;
				cARI_HESAP_HAREKETLERI3.cha_ciroprim_RECno = 0;
				cARI_HESAP_HAREKETLERI3.cha_HalHamaliyeKdv = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_HalHamaliyeVergisiz_fl = false;
				cARI_HESAP_HAREKETLERI3.cha_bakimhar_DBCno = 0;
				cARI_HESAP_HAREKETLERI3.cha_bakimhar_RECno = 0;
				cARI_HESAP_HAREKETLERI3.cha_avanstalep_DBCno = 0;
				cARI_HESAP_HAREKETLERI3.cha_avanstalep_RECno = 0;
				cARI_HESAP_HAREKETLERI3.cha_oiv_pntr = 0;
				cARI_HESAP_HAREKETLERI3.cha_oiv_vergi = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_oivtutari = 0.0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas1 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas2 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas3 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas4 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas5 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas6 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas7 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas8 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas9 = 0;
				cARI_HESAP_HAREKETLERI3.cha_isk_mas10 = 0;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas1 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas2 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas3 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas4 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas5 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas6 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas7 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas8 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas9 = false;
				cARI_HESAP_HAREKETLERI3.cha_sat_iskmas10 = false;
				evrak.AddGenelCariHesapHareketleri(cARI_HESAP_HAREKETLERI2);
				evrak.AddGenelCariHesapHareketleri(cARI_HESAP_HAREKETLERI3);
			}
		}
		int num10 = 0;
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
		SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		try
		{
			num10 = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: true);
		}
		catch (Exception)
		{
			num10 = -1;
		}
		if (num10 > 0)
		{
			evrak.SetEvraknoSira(num10);
			evrakrow[1] = false;
			evrakrow[2] = enum_GenelEvrakAktarimDurumu.Aktarilmis;
			evrakrow[5] = evrak.EvrakNoSira;
			int num11 = 0;
			childRows = evrakrow.GetChildRows(_bankaevrakdataset.dataset.Relations[0]);
			foreach (DataRow dataRow2 in childRows)
			{
				if ((bool)dataRow2[33])
				{
					int cha_RECno = evrak.GetGenelCariHesapHareketleri()[num11].cha_RECno;
					Guid cha_Guid = evrak.GetGenelCariHesapHareketleri()[num11].cha_Guid;
					string text7 = "cari_har_rec_no,";
					if (AppBase.MikroVersiyonu >= 16)
					{
						text7 = "cari_har_guid,";
					}
					string commandText = "BEGIN INSERT INTO _FORA_BANKA_AKTARIM_EK_BILGILER(" + text7 + "Tarih,metin1,metin2,metin3,metin4,metin5,metin6,metin7,metin8,metin9,metin10,metin11,metin12,metin13,metin14,metin15,metin16,metin17,metin18,metin19,metin20,metin21,metin22,metin23,metin24,metin25,metin26,metin27,metin28,metin29,metin30,metin31,metin32,metin33,metin34,metin35,metin36,metin37,metin38,metin39,metin40,metin41,metin42,metin43,metin44,metin45,metin46,metin47,metin48,metin49,metin50,dropbox1,dropbox2,dropbox3,dropbox4,dropbox5,dropbox6,dropbox7,dropbox8,dropbox9,dropbox10,checkbox1,checkbox2,checkbox3,checkbox4,checkbox5,checkbox6,checkbox7,checkbox8,checkbox9,checkbox10) VALUES(@cari_har,getdate(),@metin1,@metin2,@metin3,@metin4,@metin5,@metin6,@metin7,@metin8,@metin9,@metin10,@metin11,@metin12,@metin13,@metin14,@metin15,@metin16,@metin17,@metin18,@metin19,@metin20,@metin21,@metin22,@metin23,@metin24,@metin25,@metin26,@metin27,@metin28,@metin29,@metin30,@metin31,@metin32,@metin33,@metin34,@metin35,@metin36,@metin37,@metin38,@metin39,@metin40,@metin41,@metin42,@metin43,@metin44,@metin45,@metin46,@metin47,@metin48,@metin49,@metin50,@dropbox1,@dropbox2,@dropbox3,@dropbox4,@dropbox5,@dropbox6,@dropbox7,@dropbox8,@dropbox9,@dropbox10,@checkbox1,@checkbox2,@checkbox3,@checkbox4,@checkbox5,@checkbox6,@checkbox7,@checkbox8,@checkbox9,@checkbox10) END";
					SqlCommand sqlCommand = new SqlCommand();
					sqlCommand.CommandText = commandText;
					if (AppBase.MikroVersiyonu >= 16)
					{
						sqlCommand.Parameters.AddWithValue("@cari_har", cha_Guid);
					}
					else
					{
						sqlCommand.Parameters.AddWithValue("@cari_har", cha_RECno);
					}
					sqlCommand.Parameters.AddWithValue("@metin1", (string)dataRow2[35]);
					sqlCommand.Parameters.AddWithValue("@metin2", (string)dataRow2[36]);
					sqlCommand.Parameters.AddWithValue("@metin3", (string)dataRow2[37]);
					sqlCommand.Parameters.AddWithValue("@metin4", (string)dataRow2[38]);
					sqlCommand.Parameters.AddWithValue("@metin5", (string)dataRow2[39]);
					sqlCommand.Parameters.AddWithValue("@metin6", (string)dataRow2[40]);
					sqlCommand.Parameters.AddWithValue("@metin7", (string)dataRow2[41]);
					sqlCommand.Parameters.AddWithValue("@metin8", (string)dataRow2[42]);
					sqlCommand.Parameters.AddWithValue("@metin9", (string)dataRow2[43]);
					sqlCommand.Parameters.AddWithValue("@metin10", (string)dataRow2[44]);
					sqlCommand.Parameters.AddWithValue("@metin11", (string)dataRow2[45]);
					sqlCommand.Parameters.AddWithValue("@metin12", (string)dataRow2[46]);
					sqlCommand.Parameters.AddWithValue("@metin13", (string)dataRow2[47]);
					sqlCommand.Parameters.AddWithValue("@metin14", (string)dataRow2[48]);
					sqlCommand.Parameters.AddWithValue("@metin15", (string)dataRow2[49]);
					sqlCommand.Parameters.AddWithValue("@metin16", (string)dataRow2[50]);
					sqlCommand.Parameters.AddWithValue("@metin17", (string)dataRow2[51]);
					sqlCommand.Parameters.AddWithValue("@metin18", (string)dataRow2[52]);
					sqlCommand.Parameters.AddWithValue("@metin19", (string)dataRow2[53]);
					sqlCommand.Parameters.AddWithValue("@metin20", (string)dataRow2[54]);
					sqlCommand.Parameters.AddWithValue("@metin21", (string)dataRow2[55]);
					sqlCommand.Parameters.AddWithValue("@metin22", (string)dataRow2[56]);
					sqlCommand.Parameters.AddWithValue("@metin23", (string)dataRow2[57]);
					sqlCommand.Parameters.AddWithValue("@metin24", (string)dataRow2[58]);
					sqlCommand.Parameters.AddWithValue("@metin25", (string)dataRow2[59]);
					sqlCommand.Parameters.AddWithValue("@metin26", (string)dataRow2[60]);
					sqlCommand.Parameters.AddWithValue("@metin27", (string)dataRow2[61]);
					sqlCommand.Parameters.AddWithValue("@metin28", (string)dataRow2[62]);
					sqlCommand.Parameters.AddWithValue("@metin29", (string)dataRow2[63]);
					sqlCommand.Parameters.AddWithValue("@metin30", (string)dataRow2[64]);
					sqlCommand.Parameters.AddWithValue("@metin31", (string)dataRow2[65]);
					sqlCommand.Parameters.AddWithValue("@metin32", (string)dataRow2[66]);
					sqlCommand.Parameters.AddWithValue("@metin33", (string)dataRow2[67]);
					sqlCommand.Parameters.AddWithValue("@metin34", (string)dataRow2[68]);
					sqlCommand.Parameters.AddWithValue("@metin35", (string)dataRow2[69]);
					sqlCommand.Parameters.AddWithValue("@metin36", (string)dataRow2[70]);
					sqlCommand.Parameters.AddWithValue("@metin37", (string)dataRow2[71]);
					sqlCommand.Parameters.AddWithValue("@metin38", (string)dataRow2[72]);
					sqlCommand.Parameters.AddWithValue("@metin39", (string)dataRow2[73]);
					sqlCommand.Parameters.AddWithValue("@metin40", (string)dataRow2[74]);
					sqlCommand.Parameters.AddWithValue("@metin41", (string)dataRow2[75]);
					sqlCommand.Parameters.AddWithValue("@metin42", (string)dataRow2[76]);
					sqlCommand.Parameters.AddWithValue("@metin43", (string)dataRow2[77]);
					sqlCommand.Parameters.AddWithValue("@metin44", (string)dataRow2[78]);
					sqlCommand.Parameters.AddWithValue("@metin45", (string)dataRow2[79]);
					sqlCommand.Parameters.AddWithValue("@metin46", (string)dataRow2[80]);
					sqlCommand.Parameters.AddWithValue("@metin47", (string)dataRow2[81]);
					sqlCommand.Parameters.AddWithValue("@metin48", (string)dataRow2[82]);
					sqlCommand.Parameters.AddWithValue("@metin49", (string)dataRow2[83]);
					sqlCommand.Parameters.AddWithValue("@metin50", (string)dataRow2[84]);
					sqlCommand.Parameters.AddWithValue("@dropbox1", (string)dataRow2[85]);
					sqlCommand.Parameters.AddWithValue("@dropbox2", (string)dataRow2[86]);
					sqlCommand.Parameters.AddWithValue("@dropbox3", (string)dataRow2[87]);
					sqlCommand.Parameters.AddWithValue("@dropbox4", (string)dataRow2[88]);
					sqlCommand.Parameters.AddWithValue("@dropbox5", (string)dataRow2[89]);
					sqlCommand.Parameters.AddWithValue("@dropbox6", (string)dataRow2[90]);
					sqlCommand.Parameters.AddWithValue("@dropbox7", (string)dataRow2[91]);
					sqlCommand.Parameters.AddWithValue("@dropbox8", (string)dataRow2[92]);
					sqlCommand.Parameters.AddWithValue("@dropbox9", (string)dataRow2[93]);
					sqlCommand.Parameters.AddWithValue("@dropbox10", (string)dataRow2[94]);
					sqlCommand.Parameters.AddWithValue("@checkbox1", (bool)dataRow2[95]);
					sqlCommand.Parameters.AddWithValue("@checkbox2", (bool)dataRow2[96]);
					sqlCommand.Parameters.AddWithValue("@checkbox3", (bool)dataRow2[97]);
					sqlCommand.Parameters.AddWithValue("@checkbox4", (bool)dataRow2[98]);
					sqlCommand.Parameters.AddWithValue("@checkbox5", (bool)dataRow2[99]);
					sqlCommand.Parameters.AddWithValue("@checkbox6", (bool)dataRow2[100]);
					sqlCommand.Parameters.AddWithValue("@checkbox7", (bool)dataRow2[101]);
					sqlCommand.Parameters.AddWithValue("@checkbox8", (bool)dataRow2[102]);
					sqlCommand.Parameters.AddWithValue("@checkbox9", (bool)dataRow2[103]);
					sqlCommand.Parameters.AddWithValue("@checkbox10", (bool)dataRow2[104]);
					sqlCommand.Connection = sqlConnection;
					sqlCommand.Transaction = sqlTransaction;
					sqlCommand.ExecuteScalar();
					num11++;
				}
			}
		}
		else if (num10 == -2)
		{
			evrakrow[2] = enum_GenelEvrakAktarimDurumu.EvrakVar;
		}
		else
		{
			evrakrow[2] = enum_GenelEvrakAktarimDurumu.BeklenmedikHata;
		}
		if (num10 == -1)
		{
			sqlTransaction.Rollback();
		}
		else
		{
			sqlTransaction.Commit();
		}
		sqlConnection.Close();
		childRows = evrakrow.GetChildRows(_bankaevrakdataset.dataset.Relations[0]);
		foreach (DataRow dataRow3 in childRows)
		{
			if (!(bool)dataRow3[33])
			{
				continue;
			}
			enum_FaturaOlusturmaDurumu enum_FaturaOlusturmaDurumu = (enum_FaturaOlusturmaDurumu)dataRow3[6];
			enum_BankaSatirCinsi enum_BankaSatirCinsi2 = (enum_BankaSatirCinsi)dataRow3[2];
			if (enum_FaturaOlusturmaDurumu == enum_FaturaOlusturmaDurumu.Olusturma || enum_BankaSatirCinsi2 != enum_BankaSatirCinsi.CariHesap || evrak.evraktipi != enum_GenelEvrakTipleri.GelenHavale)
			{
				continue;
			}
			int num12 = (int)dataRow3[32];
			int num13 = 0;
			Cari cariByCariKod2 = CariData.GetCariByCariKod(Db.Connection, (string)dataRow3[3], AdreslerTemsilciyeGore: false, "");
			if (num12 == 0)
			{
				num13 = cariByCariKod2.cari_doviz_cinsi;
			}
			if (num12 == 1)
			{
				num13 = cariByCariKod2.cari_doviz_cinsi1;
			}
			if (num12 == 2)
			{
				num13 = cariByCariKod2.cari_doviz_cinsi2;
			}
			num = (double)dataRow3[4] * (double)evrakrow[9];
			enum_KapamaSekli kapamaSekli2 = enum_KapamaSekli.AcikHesap;
			enum_cha_normal_Iade normalIade2 = enum_cha_normal_Iade.Normal;
			enum_cha_ticaret_turu ticaretTuru2 = enum_cha_ticaret_turu.ToptanYurtIciTicaret;
			Evrak evrak2 = new Evrak(enum_GenelEvrakTipleri.SatisFaturasi, YeniKayitMi: true, EvrakKilitliMi: false, kapamaSekli2, normalIade2, ticaretTuru2);
			evrak2.Parametreler.vergitanimlari = _mikrouygulamabilgileri.vergitanimlari;
			evrak2.SetBelgeNo((string)evrakrow[7]);
			evrak2.SetBelgeTarihi((DateTime)evrakrow[8]);
			evrak2.cari = CariData.GetCariByCariKod(Db.Connection, (string)dataRow3[3], AdreslerTemsilciyeGore: false, "");
			evrak2.SetDegistirSpecialAlan1((string)evrakrow[11]);
			evrak2.SetDegistirSpecialAlan2((string)evrakrow[12]);
			evrak2.SetDegistirSpecialAlan3((string)evrakrow[13]);
			evrak2.SetDovizCinsi(num13);
			evrak2.SetEvrakNoSeri((string)dataRow3[9]);
			evrak2.SetEvraknoSira((int)dataRow3[10]);
			evrak2.SetEvrakTarihi((DateTime)evrakrow[6]);
			evrak2.SetFirma(FirmaData.GetFirma(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.FirmaNo));
			evrak2.SetFiyatListesi(new FiyatListesi());
			evrak2.SetKapamaHesapKodu("");
			evrak2.SetKaynakDepo(DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, 1));
			evrak2.kur = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, num13, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
			evrak2.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
			evrak2.SetOdemePlani(0);
			evrak2.SetProje(ProjeData.GetProje(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)dataRow3[12]));
			evrak2.SetSevkAdresNo(1);
			evrak2.SetSevkTeslimTarihi((DateTime)evrakrow[6]);
			evrak2.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)dataRow3[11]));
			evrak2.SetSube(SubeData.GetSube(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.SubeNo));
			evrak2.SetTemsilciKodu("");
			evrak2.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
			evrak2.alternatifdovizkuru = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, evrak.alternatifdovizcinsi, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
			evrak2.SetAciklama((string)dataRow3[5]);
			evrak2.SetAciklama1("");
			evrak2.SetAciklama2("");
			evrak2.SetAciklama3("");
			evrak2.SetAciklama4("");
			evrak2.SetAciklama5("");
			evrak2.SetAciklama6("");
			evrak2.SetAciklama7("");
			evrak2.SetAciklama8("");
			evrak2.SetAciklama9("");
			evrak2.SetAciklama10("");
			if (enum_FaturaOlusturmaDurumu == enum_FaturaOlusturmaDurumu.Hizmet)
			{
				Hizmet hizmet = new Hizmet();
				hizmet = HizmetData.GetHizmet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)dataRow3[7]);
				hizmet.Miktar = (double)dataRow3[8];
				hizmet.Aciklama = evrak2.aciklama;
				double yuzde = _mikrouygulamabilgileri.vergitanimlari[hizmet.hiz_KDV].Yuzde;
				hizmet.BirimFiyat.FiyatBrut = num / evrak2.kur.dov_fiyat / (1.0 + yuzde / 100.0) / hizmet.Miktar;
				evrak2.AddHizmet(hizmet, evrak2.proje.pro_kodu, evrak2.sorumlulukmerkezi.som_kod);
			}
			else
			{
				Stok stok = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, (string)dataRow3[7], enum_toptan_perakende.Toptan);
				stok.ekleme_bilgileri.BirimFiyat = new FiyatTanimlamasi();
				stok.ekleme_bilgileri.Miktar = (double)dataRow3[8];
				stok.ekleme_bilgileri.Aciklama1 = evrak2.aciklama;
				double yuzde2 = _mikrouygulamabilgileri.vergitanimlari[stok.sto_toptan_vergi_yeni].Yuzde;
				stok.ekleme_bilgileri.BirimFiyat.FiyatBrut = num / evrak2.kur.dov_fiyat / (1.0 + yuzde2 / 100.0) / stok.ekleme_bilgileri.Miktar;
				stok.ekleme_bilgileri.sorumluluk_merkezi_kodu = (string)dataRow3[11];
				stok.ekleme_bilgileri.proje_kodu = (string)dataRow3[12];
				evrak2.AddUrun(stok);
			}
			evrak2.cari = CariData.GetCariByCariKod(Db.Connection, (string)dataRow3[3], AdreslerTemsilciyeGore: false, "");
			int num14 = 0;
			sqlConnection = new SqlConnection();
			if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
			{
				sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Trusted_Connection=true;";
			}
			else
			{
				sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; User Id=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + ";Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + ";";
			}
			sqlConnection.Open();
			sqlTransaction = sqlConnection.BeginTransaction();
			try
			{
				num14 = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak2, EArsivAktif: true);
				sqlTransaction.Commit();
			}
			catch (Exception ex2)
			{
				num14 = -1;
				Console.WriteLine("HATA GERI ALINIYOR : " + ex2.ToString());
				sqlTransaction.Rollback();
			}
			finally
			{
				sqlConnection.Close();
			}
			if (num14 > 0)
			{
				evrak2.SetEvraknoSira(num14);
				dataRow3[10] = evrak2.EvrakNoSira;
			}
		}
	}

	private void backgroundWorker_Aktarim_DoWork(object sender, DoWorkEventArgs e)
	{
		int num = 0;
		foreach (DataRow row in _bankaevrakdataset.dataset.Tables[0].Rows)
		{
			if ((bool)row[1])
			{
				TekEvrakAktar(row);
				num++;
				backgroundWorker_Aktarim.ReportProgress(num);
			}
		}
	}

	private void advBandedGridView_satirlar_ShownEditor(object sender, EventArgs e)
	{
		_ = (AdvBandedGridView)sender;
	}

	private void TopluBankaEvrakGirisi_FormClosed(object sender, FormClosedEventArgs e)
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

	private void satirlarinAktarimSeciminiTersCevirToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (DataRow row in _bankaevrakdataset.dataset.Tables[1].Rows)
		{
			if ((bool)row[33])
			{
				row[33] = false;
			}
			else
			{
				row[33] = true;
			}
		}
	}

	private void lc_banka_kodu_Click(object sender, EventArgs e)
	{
		if (AppBase.MikroVersiyonu >= 16)
		{
			F10_Banka_Secimi_V16 f10_Banka_Secimi_V = new F10_Banka_Secimi_V16();
			f10_Banka_Secimi_V.Setup(_mikrouygulamabilgileri, "BANKALAR_CHOOSE_3", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Banka_Secimi_V.ShowDialog() == DialogResult.OK)
			{
				_bankaevrakdataset.banka = f10_Banka_Secimi_V._selecteditems[0];
				EkranGuncelle();
			}
		}
		else
		{
			F10_Banka_Secimi f10_Banka_Secimi = new F10_Banka_Secimi();
			f10_Banka_Secimi.Setup(_mikrouygulamabilgileri, "BANKALAR_CHOOSE_3", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
			if (f10_Banka_Secimi.ShowDialog() == DialogResult.OK)
			{
				_bankaevrakdataset.banka = f10_Banka_Secimi._selecteditems[0];
				EkranGuncelle();
			}
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
		DevExpress.Utils.SerializableAppearanceObject appearance = new DevExpress.Utils.SerializableAppearanceObject();
		DevExpress.Utils.SerializableAppearanceObject appearance2 = new DevExpress.Utils.SerializableAppearanceObject();
		DevExpress.XtraGrid.GridLevelNode gridLevelNode = new DevExpress.XtraGrid.GridLevelNode();
		this.advBandedGridView_satirlar = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gc_satir_aktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_SatirID = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_EvrakID = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Cinsi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_Satir_Cinsi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_SatirCinsi = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_satir_HesapKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_HesapAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_GrupNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Tutar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaSorumlulukMerkeziKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaSorumlulukMerkeziAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaProjeKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaProjeAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_SatirAciklama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemTextEdit_SatirAciklama = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_satir_BV_Aciklama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_IslemKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Unvan = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemButtonEdit_Cari_Update = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
		this.gc_satir_BV_Unvan2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_TcVergiNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_HesapNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Telefon = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Eposta = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Adres = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Mahalle = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Ilce = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_PostaKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Il = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BV_Ulke = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Unvan = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Unvan2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_TcVergiNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_HesapNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Telefon = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_EPosta = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Adres = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Mahalle = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Ilce = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_PostaKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Il = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_MV_Ulke = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaDurumu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_satir_FaturaSeri = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaSira = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaHesapKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaHesapAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FaturaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_cari_ekle = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemButtonEdit_Cari_Ekle = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
		this.gridControl_evraklar = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView_master = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand15 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_EvrakID = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand9 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_Aktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_AktarimDurumu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand10 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_EvrakTipi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemGridLookUpEdit_Evrak_Tipi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_EvrakTipi = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gridBand11 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_EvrakSeri = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_EvrakSira = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_Tarih = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemDateEdit_EvrakTarih = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
		this.gridBand12 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_BelgeNo = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_BelgeTarihi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_Kur = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SorumlulukMerkeziKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_SorumlulukMerkesiAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand7 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_special1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemTextEdit_specialalan = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_evrak_special2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_evrak_special3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand17 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_evrak_yekun = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemLookUpEdit_Satir_VergiPntr = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.repositoryItemLookUpEdit_Satir_IskontoSekli = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.repositoryItemGridLookUpEdit_NormalIade = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_NormalIade = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.repositoryItemGridLookUpEdit_AcikKapali = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.repositoryItemGridLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemGridLookUpEdit_TicaretTuru = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_TicaretTuru = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemTextEdit_FaturaAciklama = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.repositoryItemTextEdit_Aciklamalar = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.sb_Secili_Evraklari_aktar = new DevExpress.XtraEditors.SimpleButton();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yeniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.acToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.kaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.farklıKaydetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
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
		this.satırlarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_kolonlaraGoreGruplamaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_kolonSeciciyiGosterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_butunGruplariAcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_butunGruplariKapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.görünümüSaklaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.görünümüYükleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_otomatikDosyadan_yukle_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_farkliDosyadan_yukle_ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_otomatikDosyayiSilToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlar_varsayilanaGeriDonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.aramaTablolariToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satırlarToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.yeniEvrakEkleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.aktifSatirinHesapKodunuOnaylaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ustAlaniKopyalaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.aramaTablolarininVerileriniGuncelleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.satirlarinAktarimSeciminiTersCevirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.parametrelerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.varsayilanDegerlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.akisParametreleriToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.backgroundWorker_Kontrol = new System.ComponentModel.BackgroundWorker();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.label_islem_adi = new System.Windows.Forms.Label();
		this.label_islem_bilgi = new System.Windows.Forms.Label();
		this.backgroundWorker_Aktarim = new System.ComponentModel.BackgroundWorker();
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.lc_banka_adi = new DevExpress.XtraEditors.LabelControl();
		this.lc_banka_sube = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.lc_banka_kodu = new DevExpress.XtraEditors.LabelControl();
		this.lc_banka_hesapno = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.lc_mevcut_bakiye = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.lc_net_islem_tutari = new DevExpress.XtraEditors.LabelControl();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.lc_aktarim_sonrasi_bakiye = new DevExpress.XtraEditors.LabelControl();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.lc_dosya_tarih_araligi = new DevExpress.XtraEditors.LabelControl();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.lc_dosya_acilis_bakiyesi = new DevExpress.XtraEditors.LabelControl();
		this.labelControl14 = new DevExpress.XtraEditors.LabelControl();
		this.lc_dosya_kapanis_bakiyesi = new DevExpress.XtraEditors.LabelControl();
		this.labelControl16 = new DevExpress.XtraEditors.LabelControl();
		this.gridBand27 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridBand30 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridBand4 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gridBand6 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_satirlar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Cinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_SatirCinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_SatirAciklama).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemButtonEdit_Cari_Update).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemButtonEdit_Cari_Ekle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl_evraklar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_specialalan).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_VergiPntr).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_IskontoSekli).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).BeginInit();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.advBandedGridView_satirlar.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_satirlar.Appearance.BandPanel.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView_satirlar.Appearance.BandPanel.Options.UseFont = true;
		this.advBandedGridView_satirlar.Appearance.BandPanel.Options.UseForeColor = true;
		this.advBandedGridView_satirlar.Appearance.BandPanel.Options.UseTextOptions = true;
		this.advBandedGridView_satirlar.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.advBandedGridView_satirlar.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[7] { this.gridBand27, this.gridBand30, this.gridBand2, this.gridBand3, this.gridBand1, this.gridBand4, this.gridBand6 });
		this.advBandedGridView_satirlar.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[46]
		{
			this.gc_satir_aktar, this.gc_satir_SatirID, this.gc_satir_EvrakID, this.gc_satir_Cinsi, this.gc_satir_HesapKodu, this.gc_satir_HesapAdi, this.gc_satir_GrupNo, this.gc_satir_FaturaDurumu, this.gc_satir_FaturaHesapKodu, this.gc_satir_FaturaHesapAdi,
			this.gc_satir_FaturaMiktar, this.gc_satir_FaturaSeri, this.gc_satir_FaturaSira, this.gc_satir_FaturaSorumlulukMerkeziKodu, this.gc_satir_FaturaSorumlulukMerkeziAdi, this.gc_satir_FaturaProjeKodu, this.gc_satir_FaturaProjeAdi, this.gc_satir_Tutar, this.gc_satir_SatirAciklama, this.gc_satir_BV_Aciklama,
			this.gc_satir_BV_IslemKodu, this.gc_satir_BV_HesapNo, this.gc_satir_MV_HesapNo, this.gc_satir_BV_Unvan, this.gc_satir_MV_Unvan, this.gc_satir_BV_Unvan2, this.gc_satir_MV_Unvan2, this.gc_satir_BV_Adres, this.gc_satir_MV_Adres, this.gc_satir_BV_Mahalle,
			this.gc_satir_MV_Mahalle, this.gc_satir_BV_Ilce, this.gc_satir_MV_Ilce, this.gc_satir_BV_Il, this.gc_satir_MV_Il, this.gc_satir_BV_PostaKodu, this.gc_satir_MV_PostaKodu, this.gc_satir_BV_Ulke, this.gc_satir_MV_Ulke, this.gc_satir_BV_Telefon,
			this.gc_satir_MV_Telefon, this.gc_satir_BV_Eposta, this.gc_satir_MV_EPosta, this.gc_satir_BV_TcVergiNo, this.gc_satir_MV_TcVergiNo, this.gc_cari_ekle
		});
		this.advBandedGridView_satirlar.GridControl = this.gridControl_evraklar;
		this.advBandedGridView_satirlar.Name = "advBandedGridView_satirlar";
		this.advBandedGridView_satirlar.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_satirlar.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_satirlar.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView_satirlar.OptionsSelection.MultiSelect = true;
		this.advBandedGridView_satirlar.OptionsView.ShowGroupPanel = false;
		this.advBandedGridView_satirlar.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(advBandedGridView_satirlar_CustomDrawCell);
		this.advBandedGridView_satirlar.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(advBandedGridView_satirlar_CustomRowCellEdit);
		this.advBandedGridView_satirlar.InitNewRow += new DevExpress.XtraGrid.Views.Grid.InitNewRowEventHandler(advBandedGridView_satirlar_InitNewRow);
		this.advBandedGridView_satirlar.ShownEditor += new System.EventHandler(advBandedGridView_satirlar_ShownEditor);
		this.advBandedGridView_satirlar.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(advBandedGridView_satirlar_CustomUnboundColumnData);
		this.advBandedGridView_satirlar.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(advBandedGridView_satirlar_CustomColumnDisplayText);
		this.gc_satir_aktar.Caption = "Aktar";
		this.gc_satir_aktar.FieldName = "Aktar";
		this.gc_satir_aktar.Name = "gc_satir_aktar";
		this.gc_satir_aktar.Visible = true;
		this.gc_satir_aktar.Width = 42;
		this.gc_satir_SatirID.Caption = "Satır ID";
		this.gc_satir_SatirID.FieldName = "SatirID";
		this.gc_satir_SatirID.Name = "gc_satir_SatirID";
		this.gc_satir_SatirID.OptionsColumn.AllowEdit = false;
		this.gc_satir_SatirID.OptionsColumn.AllowFocus = false;
		this.gc_satir_SatirID.Visible = true;
		this.gc_satir_SatirID.Width = 45;
		this.gc_satir_EvrakID.Caption = "Evrak ID";
		this.gc_satir_EvrakID.FieldName = "EvrakID";
		this.gc_satir_EvrakID.Name = "gc_satir_EvrakID";
		this.gc_satir_EvrakID.OptionsColumn.AllowEdit = false;
		this.gc_satir_EvrakID.OptionsColumn.AllowFocus = false;
		this.gc_satir_EvrakID.Visible = true;
		this.gc_satir_EvrakID.Width = 53;
		this.gc_satir_Cinsi.Caption = "Cinsi";
		this.gc_satir_Cinsi.ColumnEdit = this.repositoryItemGridLookUpEdit_Satir_Cinsi;
		this.gc_satir_Cinsi.FieldName = "gc_satir_Cinsi";
		this.gc_satir_Cinsi.Name = "gc_satir_Cinsi";
		this.gc_satir_Cinsi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Cinsi.Visible = true;
		this.gc_satir_Cinsi.Width = 85;
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
		this.gc_satir_HesapKodu.Caption = "Hesap Kodu";
		this.gc_satir_HesapKodu.FieldName = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.Name = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapKodu.Visible = true;
		this.gc_satir_HesapKodu.Width = 74;
		this.gc_satir_HesapAdi.Caption = "Hesap Adı";
		this.gc_satir_HesapAdi.FieldName = "gc_satir_HesapAdi";
		this.gc_satir_HesapAdi.Name = "gc_satir_HesapAdi";
		this.gc_satir_HesapAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapAdi.Visible = true;
		this.gc_satir_HesapAdi.Width = 140;
		this.gc_satir_GrupNo.Caption = "Grup";
		this.gc_satir_GrupNo.FieldName = "gc_satir_GrupNo";
		this.gc_satir_GrupNo.Name = "gc_satir_GrupNo";
		this.gc_satir_GrupNo.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_GrupNo.Visible = true;
		this.gc_satir_Tutar.Caption = "Tutar";
		this.gc_satir_Tutar.FieldName = "gc_satir_Tutar";
		this.gc_satir_Tutar.Name = "gc_satir_Tutar";
		this.gc_satir_Tutar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Tutar.Visible = true;
		this.gc_satir_Tutar.Width = 64;
		this.gc_satir_FaturaSorumlulukMerkeziKodu.Caption = "Srm. Merkezi Kodu";
		this.gc_satir_FaturaSorumlulukMerkeziKodu.FieldName = "FaturaSorumlulukMerkezi";
		this.gc_satir_FaturaSorumlulukMerkeziKodu.Name = "gc_satir_FaturaSorumlulukMerkeziKodu";
		this.gc_satir_FaturaSorumlulukMerkeziKodu.Visible = true;
		this.gc_satir_FaturaSorumlulukMerkeziKodu.Width = 63;
		this.gc_satir_FaturaSorumlulukMerkeziAdi.Caption = "Srm. Merkezi Adı";
		this.gc_satir_FaturaSorumlulukMerkeziAdi.FieldName = "FaturaSorumlulukMerkezi";
		this.gc_satir_FaturaSorumlulukMerkeziAdi.Name = "gc_satir_FaturaSorumlulukMerkeziAdi";
		this.gc_satir_FaturaSorumlulukMerkeziAdi.Visible = true;
		this.gc_satir_FaturaSorumlulukMerkeziAdi.Width = 118;
		this.gc_satir_FaturaProjeKodu.Caption = "Proje Kodu";
		this.gc_satir_FaturaProjeKodu.FieldName = "FaturaProje";
		this.gc_satir_FaturaProjeKodu.Name = "gc_satir_FaturaProjeKodu";
		this.gc_satir_FaturaProjeKodu.Visible = true;
		this.gc_satir_FaturaProjeKodu.Width = 53;
		this.gc_satir_FaturaProjeAdi.Caption = "Proje Adı";
		this.gc_satir_FaturaProjeAdi.FieldName = "FaturaProje";
		this.gc_satir_FaturaProjeAdi.Name = "gc_satir_FaturaProjeAdi";
		this.gc_satir_FaturaProjeAdi.Visible = true;
		this.gc_satir_FaturaProjeAdi.Width = 141;
		this.gc_satir_SatirAciklama.Caption = "Açıklama";
		this.gc_satir_SatirAciklama.ColumnEdit = this.repositoryItemTextEdit_SatirAciklama;
		this.gc_satir_SatirAciklama.FieldName = "Aciklama";
		this.gc_satir_SatirAciklama.Name = "gc_satir_SatirAciklama";
		this.gc_satir_SatirAciklama.Visible = true;
		this.gc_satir_SatirAciklama.Width = 217;
		this.repositoryItemTextEdit_SatirAciklama.AutoHeight = false;
		this.repositoryItemTextEdit_SatirAciklama.MaxLength = 40;
		this.repositoryItemTextEdit_SatirAciklama.Name = "repositoryItemTextEdit_SatirAciklama";
		this.gc_satir_BV_Aciklama.Caption = "BV Açıklama";
		this.gc_satir_BV_Aciklama.FieldName = "BV_Aciklama";
		this.gc_satir_BV_Aciklama.Name = "gc_satir_BV_Aciklama";
		this.gc_satir_BV_Aciklama.Visible = true;
		this.gc_satir_BV_Aciklama.Width = 151;
		this.gc_satir_BV_IslemKodu.Caption = "BV İşlem Kodu";
		this.gc_satir_BV_IslemKodu.FieldName = "BV_IslemKodu";
		this.gc_satir_BV_IslemKodu.Name = "gc_satir_BV_IslemKodu";
		this.gc_satir_BV_IslemKodu.OptionsColumn.AllowEdit = false;
		this.gc_satir_BV_IslemKodu.OptionsColumn.AllowFocus = false;
		this.gc_satir_BV_IslemKodu.Visible = true;
		this.gc_satir_BV_IslemKodu.Width = 106;
		this.gc_satir_BV_Unvan.Caption = "BV Ünvan";
		this.gc_satir_BV_Unvan.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Unvan.FieldName = "BV_Unvan";
		this.gc_satir_BV_Unvan.Name = "gc_satir_BV_Unvan";
		this.gc_satir_BV_Unvan.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Unvan.Visible = true;
		this.gc_satir_BV_Unvan.Width = 118;
		this.repositoryItemButtonEdit_Cari_Update.AutoHeight = false;
		this.repositoryItemButtonEdit_Cari_Update.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Güncelle", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), appearance, "", null, null, true)
		});
		this.repositoryItemButtonEdit_Cari_Update.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
		this.repositoryItemButtonEdit_Cari_Update.Name = "repositoryItemButtonEdit_Cari_Update";
		this.repositoryItemButtonEdit_Cari_Update.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(repositoryItemButtonEdit_Cari_Update_ButtonClick);
		this.gc_satir_BV_Unvan2.Caption = "BV Ünvan 2";
		this.gc_satir_BV_Unvan2.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Unvan2.FieldName = "BV_Unvan2";
		this.gc_satir_BV_Unvan2.Name = "gc_satir_BV_Unvan2";
		this.gc_satir_BV_Unvan2.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Unvan2.Visible = true;
		this.gc_satir_BV_Unvan2.Width = 109;
		this.gc_satir_BV_TcVergiNo.Caption = "BV Tc/Vergi No";
		this.gc_satir_BV_TcVergiNo.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_TcVergiNo.FieldName = "BV_TcVergiNo";
		this.gc_satir_BV_TcVergiNo.Name = "gc_satir_BV_TcVergiNo";
		this.gc_satir_BV_TcVergiNo.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_TcVergiNo.Visible = true;
		this.gc_satir_BV_TcVergiNo.Width = 87;
		this.gc_satir_BV_HesapNo.Caption = "BV Hesap No";
		this.gc_satir_BV_HesapNo.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_HesapNo.FieldName = "BV_HesapNo";
		this.gc_satir_BV_HesapNo.Name = "gc_satir_BV_HesapNo";
		this.gc_satir_BV_HesapNo.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_HesapNo.Visible = true;
		this.gc_satir_BV_HesapNo.Width = 82;
		this.gc_satir_BV_Telefon.Caption = "BV Telefon";
		this.gc_satir_BV_Telefon.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Telefon.FieldName = "BV_Telefon";
		this.gc_satir_BV_Telefon.Name = "gc_satir_BV_Telefon";
		this.gc_satir_BV_Telefon.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Telefon.Visible = true;
		this.gc_satir_BV_Telefon.Width = 83;
		this.gc_satir_BV_Eposta.Caption = "BV E-Posta";
		this.gc_satir_BV_Eposta.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Eposta.FieldName = "BV_EPosta";
		this.gc_satir_BV_Eposta.Name = "gc_satir_BV_Eposta";
		this.gc_satir_BV_Eposta.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Eposta.Visible = true;
		this.gc_satir_BV_Eposta.Width = 97;
		this.gc_satir_BV_Adres.Caption = "BV Adres";
		this.gc_satir_BV_Adres.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Adres.FieldName = "BV_Adres";
		this.gc_satir_BV_Adres.Name = "gc_satir_BV_Adres";
		this.gc_satir_BV_Adres.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Adres.Visible = true;
		this.gc_satir_BV_Adres.Width = 179;
		this.gc_satir_BV_Mahalle.Caption = "BV Mahalle";
		this.gc_satir_BV_Mahalle.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Mahalle.FieldName = "BV_Mahalle";
		this.gc_satir_BV_Mahalle.Name = "gc_satir_BV_Mahalle";
		this.gc_satir_BV_Mahalle.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Mahalle.Visible = true;
		this.gc_satir_BV_Mahalle.Width = 126;
		this.gc_satir_BV_Ilce.Caption = "BV İlçe";
		this.gc_satir_BV_Ilce.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Ilce.FieldName = "BV_Ilce";
		this.gc_satir_BV_Ilce.Name = "gc_satir_BV_Ilce";
		this.gc_satir_BV_Ilce.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Ilce.Visible = true;
		this.gc_satir_BV_Ilce.Width = 102;
		this.gc_satir_BV_PostaKodu.Caption = "BV Posta Kodu";
		this.gc_satir_BV_PostaKodu.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_PostaKodu.FieldName = "BV_PostaKodu";
		this.gc_satir_BV_PostaKodu.Name = "gc_satir_BV_PostaKodu";
		this.gc_satir_BV_PostaKodu.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_PostaKodu.Visible = true;
		this.gc_satir_BV_PostaKodu.Width = 113;
		this.gc_satir_BV_Il.Caption = "BV İl";
		this.gc_satir_BV_Il.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Il.FieldName = "BV_Il";
		this.gc_satir_BV_Il.Name = "gc_satir_BV_Il";
		this.gc_satir_BV_Il.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Il.Visible = true;
		this.gc_satir_BV_Il.Width = 108;
		this.gc_satir_BV_Ulke.Caption = "BV Ülke";
		this.gc_satir_BV_Ulke.ColumnEdit = this.repositoryItemButtonEdit_Cari_Update;
		this.gc_satir_BV_Ulke.FieldName = "BV_Ulke";
		this.gc_satir_BV_Ulke.Name = "gc_satir_BV_Ulke";
		this.gc_satir_BV_Ulke.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_BV_Ulke.Visible = true;
		this.gc_satir_BV_Ulke.Width = 111;
		this.gc_satir_MV_Unvan.Caption = "MV Ünvan";
		this.gc_satir_MV_Unvan.FieldName = "gc_satir_MV_Unvan";
		this.gc_satir_MV_Unvan.Name = "gc_satir_MV_Unvan";
		this.gc_satir_MV_Unvan.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Unvan.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Unvan.RowIndex = 1;
		this.gc_satir_MV_Unvan.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_satir_MV_Unvan.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Unvan.Visible = true;
		this.gc_satir_MV_Unvan.Width = 118;
		this.gc_satir_MV_Unvan2.Caption = "MV Ünvan 2";
		this.gc_satir_MV_Unvan2.FieldName = "gc_satir_MV_Unvan2";
		this.gc_satir_MV_Unvan2.Name = "gc_satir_MV_Unvan2";
		this.gc_satir_MV_Unvan2.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Unvan2.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Unvan2.RowIndex = 1;
		this.gc_satir_MV_Unvan2.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Unvan2.Visible = true;
		this.gc_satir_MV_Unvan2.Width = 110;
		this.gc_satir_MV_TcVergiNo.Caption = "MV Tc/Vergi no";
		this.gc_satir_MV_TcVergiNo.FieldName = "gc_satir_MV_TcVergiNo";
		this.gc_satir_MV_TcVergiNo.Name = "gc_satir_MV_TcVergiNo";
		this.gc_satir_MV_TcVergiNo.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_TcVergiNo.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_TcVergiNo.RowIndex = 1;
		this.gc_satir_MV_TcVergiNo.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_TcVergiNo.Visible = true;
		this.gc_satir_MV_TcVergiNo.Width = 86;
		this.gc_satir_MV_HesapNo.Caption = "MV Hesap No";
		this.gc_satir_MV_HesapNo.FieldName = "gc_satir_MV_HesapNo";
		this.gc_satir_MV_HesapNo.Name = "gc_satir_MV_HesapNo";
		this.gc_satir_MV_HesapNo.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_HesapNo.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_HesapNo.RowIndex = 1;
		this.gc_satir_MV_HesapNo.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_HesapNo.Visible = true;
		this.gc_satir_MV_HesapNo.Width = 83;
		this.gc_satir_MV_Telefon.Caption = "MV Telefon";
		this.gc_satir_MV_Telefon.FieldName = "gc_satir_MV_Telefon";
		this.gc_satir_MV_Telefon.Name = "gc_satir_MV_Telefon";
		this.gc_satir_MV_Telefon.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Telefon.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Telefon.RowIndex = 1;
		this.gc_satir_MV_Telefon.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Telefon.Visible = true;
		this.gc_satir_MV_Telefon.Width = 83;
		this.gc_satir_MV_EPosta.Caption = "MV E-Posta";
		this.gc_satir_MV_EPosta.FieldName = "gc_satir_MV_EPosta";
		this.gc_satir_MV_EPosta.Name = "gc_satir_MV_EPosta";
		this.gc_satir_MV_EPosta.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_EPosta.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_EPosta.RowIndex = 1;
		this.gc_satir_MV_EPosta.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_EPosta.Visible = true;
		this.gc_satir_MV_EPosta.Width = 96;
		this.gc_satir_MV_Adres.Caption = "MV Adres";
		this.gc_satir_MV_Adres.FieldName = "gc_satir_MV_Adres";
		this.gc_satir_MV_Adres.Name = "gc_satir_MV_Adres";
		this.gc_satir_MV_Adres.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Adres.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Adres.RowIndex = 1;
		this.gc_satir_MV_Adres.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Adres.Visible = true;
		this.gc_satir_MV_Adres.Width = 180;
		this.gc_satir_MV_Mahalle.Caption = "MV Mahalle";
		this.gc_satir_MV_Mahalle.FieldName = "gc_satir_MV_Mahalle";
		this.gc_satir_MV_Mahalle.Name = "gc_satir_MV_Mahalle";
		this.gc_satir_MV_Mahalle.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Mahalle.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Mahalle.RowIndex = 1;
		this.gc_satir_MV_Mahalle.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Mahalle.Visible = true;
		this.gc_satir_MV_Mahalle.Width = 126;
		this.gc_satir_MV_Ilce.Caption = "MV İlçe";
		this.gc_satir_MV_Ilce.FieldName = "gc_satir_MV_Ilce";
		this.gc_satir_MV_Ilce.Name = "gc_satir_MV_Ilce";
		this.gc_satir_MV_Ilce.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Ilce.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Ilce.RowIndex = 1;
		this.gc_satir_MV_Ilce.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Ilce.Visible = true;
		this.gc_satir_MV_Ilce.Width = 102;
		this.gc_satir_MV_PostaKodu.Caption = "MV Posta Kodu";
		this.gc_satir_MV_PostaKodu.FieldName = "gc_satir_MV_PostaKodu";
		this.gc_satir_MV_PostaKodu.Name = "gc_satir_MV_PostaKodu";
		this.gc_satir_MV_PostaKodu.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_PostaKodu.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_PostaKodu.RowIndex = 1;
		this.gc_satir_MV_PostaKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_PostaKodu.Visible = true;
		this.gc_satir_MV_PostaKodu.Width = 113;
		this.gc_satir_MV_Il.Caption = "MV İl";
		this.gc_satir_MV_Il.FieldName = "gc_satir_MV_Il";
		this.gc_satir_MV_Il.Name = "gc_satir_MV_Il";
		this.gc_satir_MV_Il.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Il.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Il.RowIndex = 1;
		this.gc_satir_MV_Il.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Il.Visible = true;
		this.gc_satir_MV_Il.Width = 108;
		this.gc_satir_MV_Ulke.Caption = "MV Ülke";
		this.gc_satir_MV_Ulke.FieldName = "gc_satir_MV_Ulke";
		this.gc_satir_MV_Ulke.Name = "gc_satir_MV_Ulke";
		this.gc_satir_MV_Ulke.OptionsColumn.AllowEdit = false;
		this.gc_satir_MV_Ulke.OptionsColumn.AllowFocus = false;
		this.gc_satir_MV_Ulke.RowIndex = 1;
		this.gc_satir_MV_Ulke.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_MV_Ulke.Visible = true;
		this.gc_satir_MV_Ulke.Width = 110;
		this.gc_satir_FaturaDurumu.Caption = "Durumu";
		this.gc_satir_FaturaDurumu.ColumnEdit = this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu;
		this.gc_satir_FaturaDurumu.FieldName = "gc_satir_FaturaDurumu";
		this.gc_satir_FaturaDurumu.Name = "gc_satir_FaturaDurumu";
		this.gc_satir_FaturaDurumu.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_FaturaDurumu.Visible = true;
		this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.Name = "repositoryItemGridLookUpEdit_Satir_Fatura_Durumu";
		this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu.View = this.gridView2;
		this.gridView2.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView2.Name = "gridView2";
		this.gridView2.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView2.OptionsView.ShowGroupPanel = false;
		this.gc_satir_FaturaSeri.Caption = "Seri";
		this.gc_satir_FaturaSeri.FieldName = "FaturaSeri";
		this.gc_satir_FaturaSeri.Name = "gc_satir_FaturaSeri";
		this.gc_satir_FaturaSeri.Visible = true;
		this.gc_satir_FaturaSeri.Width = 41;
		this.gc_satir_FaturaSira.Caption = "Sıra";
		this.gc_satir_FaturaSira.FieldName = "FaturaSira";
		this.gc_satir_FaturaSira.Name = "gc_satir_FaturaSira";
		this.gc_satir_FaturaSira.Visible = true;
		this.gc_satir_FaturaSira.Width = 56;
		this.gc_satir_FaturaHesapKodu.Caption = "Hesap Kodu";
		this.gc_satir_FaturaHesapKodu.FieldName = "gc_satir_FaturaHesapKodu";
		this.gc_satir_FaturaHesapKodu.Name = "gc_satir_FaturaHesapKodu";
		this.gc_satir_FaturaHesapKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_FaturaHesapKodu.Visible = true;
		this.gc_satir_FaturaHesapAdi.Caption = "Hesap Adı";
		this.gc_satir_FaturaHesapAdi.FieldName = "gc_satir_FaturaHesapAdi";
		this.gc_satir_FaturaHesapAdi.Name = "gc_satir_FaturaHesapAdi";
		this.gc_satir_FaturaHesapAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_FaturaHesapAdi.Visible = true;
		this.gc_satir_FaturaHesapAdi.Width = 120;
		this.gc_satir_FaturaMiktar.Caption = "Miktar";
		this.gc_satir_FaturaMiktar.FieldName = "FaturaMiktari";
		this.gc_satir_FaturaMiktar.Name = "gc_satir_FaturaMiktar";
		this.gc_satir_FaturaMiktar.Visible = true;
		this.gc_satir_FaturaMiktar.Width = 52;
		this.gc_cari_ekle.Caption = "Cari Ekle";
		this.gc_cari_ekle.ColumnEdit = this.repositoryItemButtonEdit_Cari_Ekle;
		this.gc_cari_ekle.Name = "gc_cari_ekle";
		this.gc_cari_ekle.RowCount = 2;
		this.gc_cari_ekle.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
		this.gc_cari_ekle.Visible = true;
		this.repositoryItemButtonEdit_Cari_Ekle.AutoHeight = false;
		this.repositoryItemButtonEdit_Cari_Ekle.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "Cari ekle", -1, true, true, false, DevExpress.XtraEditors.ImageLocation.MiddleCenter, null, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), appearance2, "", null, null, true)
		});
		this.repositoryItemButtonEdit_Cari_Ekle.Name = "repositoryItemButtonEdit_Cari_Ekle";
		this.repositoryItemButtonEdit_Cari_Ekle.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(repositoryItemButtonEdit_Cari_Ekle_ButtonClick);
		this.gridControl_evraklar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		gridLevelNode.LevelTemplate = this.advBandedGridView_satirlar;
		gridLevelNode.RelationName = "Satirlar";
		this.gridControl_evraklar.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[1] { gridLevelNode });
		this.gridControl_evraklar.Location = new System.Drawing.Point(0, 76);
		this.gridControl_evraklar.MainView = this.advBandedGridView_master;
		this.gridControl_evraklar.Name = "gridControl_evraklar";
		this.gridControl_evraklar.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[16]
		{
			this.repositoryItemLookUpEdit_Satir_VergiPntr, this.repositoryItemLookUpEdit_Satir_IskontoSekli, this.repositoryItemGridLookUpEdit_Satir_Cinsi, this.repositoryItemGridLookUpEdit_Evrak_Tipi, this.repositoryItemGridLookUpEdit_NormalIade, this.repositoryItemDateEdit_EvrakTarih, this.repositoryItemLookUpEdit_Evrak_DovizCinsi, this.repositoryItemGridLookUpEdit_AcikKapali, this.repositoryItemGridLookUpEdit_TicaretTuru, this.repositoryItemTextEdit_FaturaAciklama,
			this.repositoryItemTextEdit_Aciklamalar, this.repositoryItemTextEdit_specialalan, this.repositoryItemButtonEdit_Cari_Update, this.repositoryItemButtonEdit_Cari_Ekle, this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu, this.repositoryItemTextEdit_SatirAciklama
		});
		this.gridControl_evraklar.Size = new System.Drawing.Size(1185, 415);
		this.gridControl_evraklar.TabIndex = 1;
		this.gridControl_evraklar.UseEmbeddedNavigator = true;
		this.gridControl_evraklar.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[2] { this.advBandedGridView_master, this.advBandedGridView_satirlar });
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
		this.advBandedGridView_master.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[8] { this.gridBand15, this.gridBand9, this.gridBand10, this.gridBand11, this.gridBand12, this.gridBand5, this.gridBand7, this.gridBand17 });
		this.advBandedGridView_master.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[16]
		{
			this.gc_evrak_EvrakID, this.gc_evrak_Aktar, this.gc_evrak_AktarimDurumu, this.gc_evrak_EvrakTipi, this.gc_evrak_EvrakSeri, this.gc_evrak_EvrakSira, this.gc_evrak_Tarih, this.gc_evrak_BelgeNo, this.gc_evrak_BelgeTarihi, this.gc_evrak_Kur,
			this.gc_evrak_SorumlulukMerkeziKodu, this.gc_evrak_SorumlulukMerkesiAdi, this.gc_evrak_special1, this.gc_evrak_special2, this.gc_evrak_special3, this.gc_evrak_yekun
		});
		this.advBandedGridView_master.DetailVerticalIndent = 20;
		this.advBandedGridView_master.GridControl = this.gridControl_evraklar;
		this.advBandedGridView_master.Name = "advBandedGridView_master";
		this.advBandedGridView_master.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsDetail.AllowExpandEmptyDetails = true;
		this.advBandedGridView_master.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView_master.OptionsSelection.MultiSelect = true;
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
		this.gridBand15.Columns.Add(this.gc_evrak_EvrakID);
		this.gridBand15.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
		this.gridBand15.Name = "gridBand15";
		this.gridBand15.VisibleIndex = 0;
		this.gridBand15.Width = 47;
		this.gc_evrak_EvrakID.Caption = "Evrak ID";
		this.gc_evrak_EvrakID.FieldName = "EvrakID";
		this.gc_evrak_EvrakID.Name = "gc_evrak_EvrakID";
		this.gc_evrak_EvrakID.OptionsColumn.AllowEdit = false;
		this.gc_evrak_EvrakID.OptionsColumn.AllowFocus = false;
		this.gc_evrak_EvrakID.Visible = true;
		this.gc_evrak_EvrakID.Width = 47;
		this.gridBand9.Caption = "Aktarım";
		this.gridBand9.Columns.Add(this.gc_evrak_Aktar);
		this.gridBand9.Columns.Add(this.gc_evrak_AktarimDurumu);
		this.gridBand9.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
		this.gridBand9.Name = "gridBand9";
		this.gridBand9.VisibleIndex = 1;
		this.gridBand9.Width = 131;
		this.gc_evrak_Aktar.Caption = "Aktar";
		this.gc_evrak_Aktar.FieldName = "gc_evrak_Aktar";
		this.gc_evrak_Aktar.Name = "gc_evrak_Aktar";
		this.gc_evrak_Aktar.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
		this.gc_evrak_Aktar.Visible = true;
		this.gc_evrak_Aktar.Width = 42;
		this.gc_evrak_AktarimDurumu.Caption = "Aktarım Durumu";
		this.gc_evrak_AktarimDurumu.FieldName = "gc_evrak_AktarimDurumu";
		this.gc_evrak_AktarimDurumu.Name = "gc_evrak_AktarimDurumu";
		this.gc_evrak_AktarimDurumu.OptionsColumn.AllowEdit = false;
		this.gc_evrak_AktarimDurumu.OptionsColumn.AllowFocus = false;
		this.gc_evrak_AktarimDurumu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_AktarimDurumu.Visible = true;
		this.gc_evrak_AktarimDurumu.Width = 89;
		this.gridBand10.Caption = "Evrak Tipi";
		this.gridBand10.Columns.Add(this.gc_evrak_EvrakTipi);
		this.gridBand10.Name = "gridBand10";
		this.gridBand10.VisibleIndex = 2;
		this.gridBand10.Width = 106;
		this.gc_evrak_EvrakTipi.Caption = "Tipi";
		this.gc_evrak_EvrakTipi.ColumnEdit = this.repositoryItemGridLookUpEdit_Evrak_Tipi;
		this.gc_evrak_EvrakTipi.FieldName = "gc_evrak_EvrakTipi";
		this.gc_evrak_EvrakTipi.Name = "gc_evrak_EvrakTipi";
		this.gc_evrak_EvrakTipi.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_evrak_EvrakTipi.Visible = true;
		this.gc_evrak_EvrakTipi.Width = 106;
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
		this.gridBand11.Caption = "Seri-Sıra-Tarih";
		this.gridBand11.Columns.Add(this.gc_evrak_EvrakSeri);
		this.gridBand11.Columns.Add(this.gc_evrak_EvrakSira);
		this.gridBand11.Columns.Add(this.gc_evrak_Tarih);
		this.gridBand11.Name = "gridBand11";
		this.gridBand11.VisibleIndex = 3;
		this.gridBand11.Width = 182;
		this.gc_evrak_EvrakSeri.Caption = "Seri";
		this.gc_evrak_EvrakSeri.FieldName = "EvrakSeri";
		this.gc_evrak_EvrakSeri.Name = "gc_evrak_EvrakSeri";
		this.gc_evrak_EvrakSeri.Visible = true;
		this.gc_evrak_EvrakSeri.Width = 44;
		this.gc_evrak_EvrakSira.Caption = "Sıra";
		this.gc_evrak_EvrakSira.FieldName = "EvrakSira";
		this.gc_evrak_EvrakSira.Name = "gc_evrak_EvrakSira";
		this.gc_evrak_EvrakSira.Visible = true;
		this.gc_evrak_EvrakSira.Width = 66;
		this.gc_evrak_Tarih.Caption = "Tarih";
		this.gc_evrak_Tarih.ColumnEdit = this.repositoryItemDateEdit_EvrakTarih;
		this.gc_evrak_Tarih.FieldName = "Tarih";
		this.gc_evrak_Tarih.Name = "gc_evrak_Tarih";
		this.gc_evrak_Tarih.Visible = true;
		this.gc_evrak_Tarih.Width = 72;
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
		this.gridBand12.Caption = "Belge";
		this.gridBand12.Columns.Add(this.gc_evrak_BelgeNo);
		this.gridBand12.Columns.Add(this.gc_evrak_BelgeTarihi);
		this.gridBand12.Name = "gridBand12";
		this.gridBand12.VisibleIndex = 4;
		this.gridBand12.Width = 134;
		this.gc_evrak_BelgeNo.Caption = "Belge No";
		this.gc_evrak_BelgeNo.FieldName = "Belgeno";
		this.gc_evrak_BelgeNo.Name = "gc_evrak_BelgeNo";
		this.gc_evrak_BelgeNo.Visible = true;
		this.gc_evrak_BelgeNo.Width = 58;
		this.gc_evrak_BelgeTarihi.Caption = "Belge Tarihi";
		this.gc_evrak_BelgeTarihi.FieldName = "BelgeTarihi";
		this.gc_evrak_BelgeTarihi.Name = "gc_evrak_BelgeTarihi";
		this.gc_evrak_BelgeTarihi.Visible = true;
		this.gc_evrak_BelgeTarihi.Width = 76;
		this.gridBand5.Caption = "Detaylar";
		this.gridBand5.Columns.Add(this.gc_evrak_Kur);
		this.gridBand5.Columns.Add(this.gc_evrak_SorumlulukMerkeziKodu);
		this.gridBand5.Columns.Add(this.gc_evrak_SorumlulukMerkesiAdi);
		this.gridBand5.Name = "gridBand5";
		this.gridBand5.VisibleIndex = 5;
		this.gridBand5.Width = 289;
		this.gc_evrak_Kur.Caption = "Kur";
		this.gc_evrak_Kur.FieldName = "gc_evrak_Kur";
		this.gc_evrak_Kur.Name = "gc_evrak_Kur";
		this.gc_evrak_Kur.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_evrak_Kur.Visible = true;
		this.gc_evrak_Kur.Width = 53;
		this.gc_evrak_SorumlulukMerkeziKodu.Caption = "Srm. Merkezi Kodu";
		this.gc_evrak_SorumlulukMerkeziKodu.FieldName = "SorumlulukMerkezi";
		this.gc_evrak_SorumlulukMerkeziKodu.Name = "gc_evrak_SorumlulukMerkeziKodu";
		this.gc_evrak_SorumlulukMerkeziKodu.Visible = true;
		this.gc_evrak_SorumlulukMerkeziKodu.Width = 96;
		this.gc_evrak_SorumlulukMerkesiAdi.Caption = "Srm. Merkezi Adı";
		this.gc_evrak_SorumlulukMerkesiAdi.FieldName = "SorumlulukMerkezi";
		this.gc_evrak_SorumlulukMerkesiAdi.Name = "gc_evrak_SorumlulukMerkesiAdi";
		this.gc_evrak_SorumlulukMerkesiAdi.Visible = true;
		this.gc_evrak_SorumlulukMerkesiAdi.Width = 140;
		this.gridBand7.Caption = "Özel Alanlar";
		this.gridBand7.Columns.Add(this.gc_evrak_special1);
		this.gridBand7.Columns.Add(this.gc_evrak_special2);
		this.gridBand7.Columns.Add(this.gc_evrak_special3);
		this.gridBand7.Name = "gridBand7";
		this.gridBand7.VisibleIndex = 6;
		this.gridBand7.Width = 163;
		this.gc_evrak_special1.Caption = "Special 1";
		this.gc_evrak_special1.ColumnEdit = this.repositoryItemTextEdit_specialalan;
		this.gc_evrak_special1.FieldName = "special1";
		this.gc_evrak_special1.Name = "gc_evrak_special1";
		this.gc_evrak_special1.Visible = true;
		this.gc_evrak_special1.Width = 57;
		this.repositoryItemTextEdit_specialalan.AutoHeight = false;
		this.repositoryItemTextEdit_specialalan.MaxLength = 4;
		this.repositoryItemTextEdit_specialalan.Name = "repositoryItemTextEdit_specialalan";
		this.gc_evrak_special2.Caption = "Special 2";
		this.gc_evrak_special2.ColumnEdit = this.repositoryItemTextEdit_specialalan;
		this.gc_evrak_special2.FieldName = "special2";
		this.gc_evrak_special2.Name = "gc_evrak_special2";
		this.gc_evrak_special2.Visible = true;
		this.gc_evrak_special2.Width = 56;
		this.gc_evrak_special3.Caption = "Special 3";
		this.gc_evrak_special3.ColumnEdit = this.repositoryItemTextEdit_specialalan;
		this.gc_evrak_special3.FieldName = "special3";
		this.gc_evrak_special3.Name = "gc_evrak_special3";
		this.gc_evrak_special3.Visible = true;
		this.gc_evrak_special3.Width = 50;
		this.gridBand17.Caption = "Toplam";
		this.gridBand17.Columns.Add(this.gc_evrak_yekun);
		this.gridBand17.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Right;
		this.gridBand17.Name = "gridBand17";
		this.gridBand17.VisibleIndex = 7;
		this.gridBand17.Width = 129;
		this.gc_evrak_yekun.AppearanceCell.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.gc_evrak_yekun.AppearanceCell.Options.UseFont = true;
		this.gc_evrak_yekun.Caption = "Yekün";
		this.gc_evrak_yekun.FieldName = "gc_evrak_yekun";
		this.gc_evrak_yekun.Name = "gc_evrak_yekun";
		this.gc_evrak_yekun.OptionsColumn.AllowEdit = false;
		this.gc_evrak_yekun.OptionsColumn.AllowFocus = false;
		this.gc_evrak_yekun.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_evrak_yekun.Visible = true;
		this.gc_evrak_yekun.Width = 129;
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
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.AutoHeight = false;
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.Name = "repositoryItemLookUpEdit_Evrak_DovizCinsi";
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
		this.repositoryItemTextEdit_FaturaAciklama.AutoHeight = false;
		this.repositoryItemTextEdit_FaturaAciklama.MaxLength = 40;
		this.repositoryItemTextEdit_FaturaAciklama.Name = "repositoryItemTextEdit_FaturaAciklama";
		this.repositoryItemTextEdit_Aciklamalar.AutoHeight = false;
		this.repositoryItemTextEdit_Aciklamalar.MaxLength = 127;
		this.repositoryItemTextEdit_Aciklamalar.Name = "repositoryItemTextEdit_Aciklamalar";
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
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.dosyaToolStripMenuItem, this.gorunumToolStripMenuItem, this.satırlarToolStripMenuItem1, this.parametrelerToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(1197, 24);
		this.menuStrip1.TabIndex = 2;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.yeniToolStripMenuItem, this.acToolStripMenuItem, this.toolStripSeparator3, this.kaydetToolStripMenuItem, this.farklıKaydetToolStripMenuItem, this.toolStripSeparator1, this.kapatToolStripMenuItem });
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
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(147, 6);
		this.kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
		this.kapatToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
		this.kapatToolStripMenuItem.Text = "Kapat";
		this.kapatToolStripMenuItem.Click += new System.EventHandler(kapatToolStripMenuItem_Click);
		this.gorunumToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.evraklarToolStripMenuItem, this.satırlarToolStripMenuItem, this.aramaTablolariToolStripMenuItem });
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
		this.satırlarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.satirlar_kolonlaraGoreGruplamaToolStripMenuItem, this.satirlar_kolonSeciciyiGosterToolStripMenuItem, this.satirlar_butunGruplariAcToolStripMenuItem, this.satirlar_butunGruplariKapatToolStripMenuItem, this.toolStripSeparator5, this.görünümüSaklaToolStripMenuItem, this.görünümüYükleToolStripMenuItem1, this.satirlar_otomatikDosyayiSilToolStripMenuItem, this.satirlar_varsayilanaGeriDonToolStripMenuItem });
		this.satırlarToolStripMenuItem.Name = "satırlarToolStripMenuItem";
		this.satırlarToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
		this.satırlarToolStripMenuItem.Text = "Satırlar";
		this.satirlar_kolonlaraGoreGruplamaToolStripMenuItem.Name = "satirlar_kolonlaraGoreGruplamaToolStripMenuItem";
		this.satirlar_kolonlaraGoreGruplamaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.satirlar_kolonlaraGoreGruplamaToolStripMenuItem.Text = "Kolonlara göre gruplama";
		this.satirlar_kolonlaraGoreGruplamaToolStripMenuItem.Click += new System.EventHandler(satirlar_kolonlaraGoreGruplamaToolStripMenuItem_Click);
		this.satirlar_kolonSeciciyiGosterToolStripMenuItem.Name = "satirlar_kolonSeciciyiGosterToolStripMenuItem";
		this.satirlar_kolonSeciciyiGosterToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.satirlar_kolonSeciciyiGosterToolStripMenuItem.Text = "Kolon seçiciyi göster";
		this.satirlar_kolonSeciciyiGosterToolStripMenuItem.Click += new System.EventHandler(satirlar_kolonSeciciyiGosterToolStripMenuItem_Click);
		this.satirlar_butunGruplariAcToolStripMenuItem.Name = "satirlar_butunGruplariAcToolStripMenuItem";
		this.satirlar_butunGruplariAcToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.satirlar_butunGruplariAcToolStripMenuItem.Text = "Bütün grupları aç";
		this.satirlar_butunGruplariAcToolStripMenuItem.Click += new System.EventHandler(satirlar_butunGruplariAcToolStripMenuItem_Click);
		this.satirlar_butunGruplariKapatToolStripMenuItem.Name = "satirlar_butunGruplariKapatToolStripMenuItem";
		this.satirlar_butunGruplariKapatToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.satirlar_butunGruplariKapatToolStripMenuItem.Text = "Bütün grupları kapat";
		this.satirlar_butunGruplariKapatToolStripMenuItem.Click += new System.EventHandler(satirlar_butunGruplariKapatToolStripMenuItem_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(202, 6);
		this.görünümüSaklaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem, this.satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1 });
		this.görünümüSaklaToolStripMenuItem.Name = "görünümüSaklaToolStripMenuItem";
		this.görünümüSaklaToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.görünümüSaklaToolStripMenuItem.Text = "Görünümü sakla";
		this.satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem.Name = "satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem";
		this.satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
		this.satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem.Text = "Otomatik dosyaya";
		this.satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem.Click += new System.EventHandler(satirlar_otomatikDosyaya_kaydet_ToolStripMenuItem_Click);
		this.satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1.Name = "satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1";
		this.satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1.Size = new System.Drawing.Size(170, 22);
		this.satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1.Text = "Farklı dosyaya...";
		this.satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1.Click += new System.EventHandler(satirlar_farklıDosyaya_kaydet_ToolStripMenuItem1_Click);
		this.görünümüYükleToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.satirlar_otomatikDosyadan_yukle_ToolStripMenuItem, this.satirlar_farkliDosyadan_yukle_ToolStripMenuItem1 });
		this.görünümüYükleToolStripMenuItem1.Name = "görünümüYükleToolStripMenuItem1";
		this.görünümüYükleToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
		this.görünümüYükleToolStripMenuItem1.Text = "Görünümü yükle";
		this.satirlar_otomatikDosyadan_yukle_ToolStripMenuItem.Name = "satirlar_otomatikDosyadan_yukle_ToolStripMenuItem";
		this.satirlar_otomatikDosyadan_yukle_ToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
		this.satirlar_otomatikDosyadan_yukle_ToolStripMenuItem.Text = "Otomatik dosyadan";
		this.satirlar_otomatikDosyadan_yukle_ToolStripMenuItem.Click += new System.EventHandler(satirlar_otomatikDosyadan_yukle_ToolStripMenuItem_Click);
		this.satirlar_farkliDosyadan_yukle_ToolStripMenuItem1.Name = "satirlar_farkliDosyadan_yukle_ToolStripMenuItem1";
		this.satirlar_farkliDosyadan_yukle_ToolStripMenuItem1.Size = new System.Drawing.Size(178, 22);
		this.satirlar_farkliDosyadan_yukle_ToolStripMenuItem1.Text = "Farklı dosyadan...";
		this.satirlar_farkliDosyadan_yukle_ToolStripMenuItem1.Click += new System.EventHandler(satirlar_farkliDosyadan_yukle_ToolStripMenuItem1_Click);
		this.satirlar_otomatikDosyayiSilToolStripMenuItem.Name = "satirlar_otomatikDosyayiSilToolStripMenuItem";
		this.satirlar_otomatikDosyayiSilToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.satirlar_otomatikDosyayiSilToolStripMenuItem.Text = "Otomatik dosyayı sil";
		this.satirlar_otomatikDosyayiSilToolStripMenuItem.Click += new System.EventHandler(satirlar_otomatikDosyayiSilToolStripMenuItem_Click);
		this.satirlar_varsayilanaGeriDonToolStripMenuItem.Name = "satirlar_varsayilanaGeriDonToolStripMenuItem";
		this.satirlar_varsayilanaGeriDonToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
		this.satirlar_varsayilanaGeriDonToolStripMenuItem.Text = "Varsayılana geri dön";
		this.satirlar_varsayilanaGeriDonToolStripMenuItem.Click += new System.EventHandler(satirlar_varsayilanaGeriDonToolStripMenuItem_Click);
		this.aramaTablolariToolStripMenuItem.Name = "aramaTablolariToolStripMenuItem";
		this.aramaTablolariToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
		this.aramaTablolariToolStripMenuItem.Text = "Arama tabloları";
		this.satırlarToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.yeniEvrakEkleToolStripMenuItem1, this.aktifSatirinHesapKodunuOnaylaToolStripMenuItem, this.ustAlaniKopyalaToolStripMenuItem, this.aramaTablolarininVerileriniGuncelleToolStripMenuItem, this.satirlarinAktarimSeciminiTersCevirToolStripMenuItem });
		this.satırlarToolStripMenuItem1.Name = "satırlarToolStripMenuItem1";
		this.satırlarToolStripMenuItem1.Size = new System.Drawing.Size(60, 20);
		this.satırlarToolStripMenuItem1.Text = "İşlemler";
		this.yeniEvrakEkleToolStripMenuItem1.Name = "yeniEvrakEkleToolStripMenuItem1";
		this.yeniEvrakEkleToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.N | System.Windows.Forms.Keys.Control;
		this.yeniEvrakEkleToolStripMenuItem1.Size = new System.Drawing.Size(291, 22);
		this.yeniEvrakEkleToolStripMenuItem1.Text = "Yeni evrak ekle";
		this.yeniEvrakEkleToolStripMenuItem1.Click += new System.EventHandler(yeniEvrakEkleToolStripMenuItem_Click);
		this.aktifSatirinHesapKodunuOnaylaToolStripMenuItem.Name = "aktifSatirinHesapKodunuOnaylaToolStripMenuItem";
		this.aktifSatirinHesapKodunuOnaylaToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.X | System.Windows.Forms.Keys.Control;
		this.aktifSatirinHesapKodunuOnaylaToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
		this.aktifSatirinHesapKodunuOnaylaToolStripMenuItem.Text = "Aktif satırın hesap kodunu onayla";
		this.aktifSatirinHesapKodunuOnaylaToolStripMenuItem.Click += new System.EventHandler(aktifSatirinHesapKodunuOnaylaToolStripMenuItem_Click);
		this.ustAlaniKopyalaToolStripMenuItem.Name = "ustAlaniKopyalaToolStripMenuItem";
		this.ustAlaniKopyalaToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
		this.ustAlaniKopyalaToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
		this.ustAlaniKopyalaToolStripMenuItem.Text = "Üst alanı kopyala";
		this.ustAlaniKopyalaToolStripMenuItem.Click += new System.EventHandler(ustAlaniKopyalaToolStripMenuItem_Click);
		this.aramaTablolarininVerileriniGuncelleToolStripMenuItem.Name = "aramaTablolarininVerileriniGuncelleToolStripMenuItem";
		this.aramaTablolarininVerileriniGuncelleToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
		this.aramaTablolarininVerileriniGuncelleToolStripMenuItem.Text = "Arama tablolarının verilerini güncelle";
		this.aramaTablolarininVerileriniGuncelleToolStripMenuItem.Click += new System.EventHandler(aramaTablolarininVerileriniGuncelleToolStripMenuItem_Click);
		this.satirlarinAktarimSeciminiTersCevirToolStripMenuItem.Name = "satirlarinAktarimSeciminiTersCevirToolStripMenuItem";
		this.satirlarinAktarimSeciminiTersCevirToolStripMenuItem.Size = new System.Drawing.Size(291, 22);
		this.satirlarinAktarimSeciminiTersCevirToolStripMenuItem.Text = "Satırların aktarım seçimini ters çevir";
		this.satirlarinAktarimSeciminiTersCevirToolStripMenuItem.Click += new System.EventHandler(satirlarinAktarimSeciminiTersCevirToolStripMenuItem_Click);
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
		this.backgroundWorker_Kontrol.WorkerReportsProgress = true;
		this.backgroundWorker_Kontrol.WorkerSupportsCancellation = true;
		this.backgroundWorker_Kontrol.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_Kontrol_DoWork);
		this.backgroundWorker_Kontrol.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker_Kontrol_ProgressChanged);
		this.backgroundWorker_Kontrol.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorker_Kontrol_RunWorkerCompleted);
		this.progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.progressBar1.Location = new System.Drawing.Point(12, 494);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(805, 23);
		this.progressBar1.TabIndex = 3;
		this.label_islem_adi.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label_islem_adi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_adi.Location = new System.Drawing.Point(904, 494);
		this.label_islem_adi.Name = "label_islem_adi";
		this.label_islem_adi.Size = new System.Drawing.Size(117, 23);
		this.label_islem_adi.TabIndex = 4;
		this.label_islem_adi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label_islem_bilgi.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label_islem_bilgi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_bilgi.ForeColor = System.Drawing.Color.Red;
		this.label_islem_bilgi.Location = new System.Drawing.Point(823, 494);
		this.label_islem_bilgi.Name = "label_islem_bilgi";
		this.label_islem_bilgi.Size = new System.Drawing.Size(75, 23);
		this.label_islem_bilgi.TabIndex = 5;
		this.label_islem_bilgi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.backgroundWorker_Aktarim.WorkerReportsProgress = true;
		this.backgroundWorker_Aktarim.WorkerSupportsCancellation = true;
		this.backgroundWorker_Aktarim.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_Aktarim_DoWork);
		this.backgroundWorker_Aktarim.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker_Aktarim_ProgressChanged);
		this.backgroundWorker_Aktarim.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorker_Aktarim_RunWorkerCompleted);
		this.labelControl1.Location = new System.Drawing.Point(12, 36);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(62, 13);
		this.labelControl1.TabIndex = 7;
		this.labelControl1.Text = "Banka kodu :";
		this.labelControl2.Location = new System.Drawing.Point(184, 36);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(53, 13);
		this.labelControl2.TabIndex = 8;
		this.labelControl2.Text = "Banka adı :";
		this.lc_banka_adi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_banka_adi.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_banka_adi.Location = new System.Drawing.Point(243, 36);
		this.lc_banka_adi.Name = "lc_banka_adi";
		this.lc_banka_adi.Size = new System.Drawing.Size(193, 13);
		this.lc_banka_adi.TabIndex = 9;
		this.lc_banka_adi.Text = "BANKA ADI";
		this.lc_banka_sube.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_banka_sube.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_banka_sube.Location = new System.Drawing.Point(482, 36);
		this.lc_banka_sube.Name = "lc_banka_sube";
		this.lc_banka_sube.Size = new System.Drawing.Size(132, 13);
		this.lc_banka_sube.TabIndex = 12;
		this.lc_banka_sube.Text = "BANKA ADI";
		this.labelControl5.Location = new System.Drawing.Point(445, 36);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(31, 13);
		this.labelControl5.TabIndex = 11;
		this.labelControl5.Text = "Şube :";
		this.lc_banka_kodu.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_banka_kodu.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_banka_kodu.Location = new System.Drawing.Point(80, 36);
		this.lc_banka_kodu.Name = "lc_banka_kodu";
		this.lc_banka_kodu.Size = new System.Drawing.Size(98, 13);
		this.lc_banka_kodu.TabIndex = 13;
		this.lc_banka_kodu.Text = "BANKA KODU";
		this.lc_banka_kodu.Click += new System.EventHandler(lc_banka_kodu_Click);
		this.lc_banka_hesapno.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_banka_hesapno.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_banka_hesapno.Location = new System.Drawing.Point(678, 36);
		this.lc_banka_hesapno.Name = "lc_banka_hesapno";
		this.lc_banka_hesapno.Size = new System.Drawing.Size(92, 13);
		this.lc_banka_hesapno.TabIndex = 15;
		this.lc_banka_hesapno.Text = "HESAP NO";
		this.labelControl4.Location = new System.Drawing.Point(620, 36);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(52, 13);
		this.labelControl4.TabIndex = 14;
		this.labelControl4.Text = "Hesap no :";
		this.lc_mevcut_bakiye.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_mevcut_bakiye.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_mevcut_bakiye.Location = new System.Drawing.Point(858, 36);
		this.lc_mevcut_bakiye.Name = "lc_mevcut_bakiye";
		this.lc_mevcut_bakiye.Size = new System.Drawing.Size(114, 13);
		this.lc_mevcut_bakiye.TabIndex = 17;
		this.lc_mevcut_bakiye.Text = "10.250.405,50 USD";
		this.labelControl6.Location = new System.Drawing.Point(776, 36);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(76, 13);
		this.labelControl6.TabIndex = 16;
		this.labelControl6.Text = "Mevcut bakiye :";
		this.lc_net_islem_tutari.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_net_islem_tutari.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_net_islem_tutari.Location = new System.Drawing.Point(1062, 36);
		this.lc_net_islem_tutari.Name = "lc_net_islem_tutari";
		this.lc_net_islem_tutari.Size = new System.Drawing.Size(114, 13);
		this.lc_net_islem_tutari.TabIndex = 19;
		this.lc_net_islem_tutari.Text = "10.250.405,50 USD";
		this.labelControl8.Location = new System.Drawing.Point(977, 36);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(79, 13);
		this.labelControl8.TabIndex = 18;
		this.labelControl8.Text = "Net işlem tutarı :";
		this.lc_aktarim_sonrasi_bakiye.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_aktarim_sonrasi_bakiye.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_aktarim_sonrasi_bakiye.Location = new System.Drawing.Point(1062, 55);
		this.lc_aktarim_sonrasi_bakiye.Name = "lc_aktarim_sonrasi_bakiye";
		this.lc_aktarim_sonrasi_bakiye.Size = new System.Drawing.Size(114, 13);
		this.lc_aktarim_sonrasi_bakiye.TabIndex = 21;
		this.lc_aktarim_sonrasi_bakiye.Text = "10.250.405,50 USD";
		this.labelControl10.Location = new System.Drawing.Point(942, 55);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(114, 13);
		this.labelControl10.TabIndex = 20;
		this.labelControl10.Text = "Aktarım sonrası bakiye :";
		this.lc_dosya_tarih_araligi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_dosya_tarih_araligi.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_dosya_tarih_araligi.Location = new System.Drawing.Point(81, 57);
		this.lc_dosya_tarih_araligi.Name = "lc_dosya_tarih_araligi";
		this.lc_dosya_tarih_araligi.Size = new System.Drawing.Size(144, 13);
		this.lc_dosya_tarih_araligi.TabIndex = 23;
		this.lc_dosya_tarih_araligi.Text = "20.09.2013-20.10.2013";
		this.labelControl12.Location = new System.Drawing.Point(13, 57);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(62, 13);
		this.labelControl12.TabIndex = 22;
		this.labelControl12.Text = "Tarih aralığı :";
		this.lc_dosya_acilis_bakiyesi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_dosya_acilis_bakiyesi.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_dosya_acilis_bakiyesi.Location = new System.Drawing.Point(336, 57);
		this.lc_dosya_acilis_bakiyesi.Name = "lc_dosya_acilis_bakiyesi";
		this.lc_dosya_acilis_bakiyesi.Size = new System.Drawing.Size(114, 13);
		this.lc_dosya_acilis_bakiyesi.TabIndex = 25;
		this.lc_dosya_acilis_bakiyesi.Text = "10.250.405,50 USD";
		this.labelControl14.Location = new System.Drawing.Point(227, 57);
		this.labelControl14.Name = "labelControl14";
		this.labelControl14.Size = new System.Drawing.Size(103, 13);
		this.labelControl14.TabIndex = 24;
		this.labelControl14.Text = "Dosya açılış bakiyesi :";
		this.lc_dosya_kapanis_bakiyesi.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.lc_dosya_kapanis_bakiyesi.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.lc_dosya_kapanis_bakiyesi.Location = new System.Drawing.Point(579, 57);
		this.lc_dosya_kapanis_bakiyesi.Name = "lc_dosya_kapanis_bakiyesi";
		this.lc_dosya_kapanis_bakiyesi.Size = new System.Drawing.Size(114, 13);
		this.lc_dosya_kapanis_bakiyesi.TabIndex = 27;
		this.lc_dosya_kapanis_bakiyesi.Text = "10.250.405,50 USD";
		this.labelControl16.Location = new System.Drawing.Point(456, 57);
		this.labelControl16.Name = "labelControl16";
		this.labelControl16.Size = new System.Drawing.Size(117, 13);
		this.labelControl16.TabIndex = 26;
		this.labelControl16.Text = "Dosya kapanış bakiyesi :";
		this.gridBand27.Caption = "ID";
		this.gridBand27.Columns.Add(this.gc_satir_aktar);
		this.gridBand27.Columns.Add(this.gc_satir_SatirID);
		this.gridBand27.Columns.Add(this.gc_satir_EvrakID);
		this.gridBand27.Name = "gridBand27";
		this.gridBand27.VisibleIndex = 0;
		this.gridBand27.Width = 140;
		this.gridBand30.Caption = "Banka Hareketi";
		this.gridBand30.Columns.Add(this.gc_satir_Cinsi);
		this.gridBand30.Columns.Add(this.gc_satir_HesapKodu);
		this.gridBand30.Columns.Add(this.gc_satir_HesapAdi);
		this.gridBand30.Columns.Add(this.gc_satir_GrupNo);
		this.gridBand30.Columns.Add(this.gc_satir_Tutar);
		this.gridBand30.Columns.Add(this.gc_satir_FaturaSorumlulukMerkeziKodu);
		this.gridBand30.Columns.Add(this.gc_satir_FaturaSorumlulukMerkeziAdi);
		this.gridBand30.Columns.Add(this.gc_satir_FaturaProjeKodu);
		this.gridBand30.Columns.Add(this.gc_satir_FaturaProjeAdi);
		this.gridBand30.Columns.Add(this.gc_satir_SatirAciklama);
		this.gridBand30.Name = "gridBand30";
		this.gridBand30.VisibleIndex = 1;
		this.gridBand30.Width = 1030;
		this.gridBand2.Caption = "Banka Verisi";
		this.gridBand2.Columns.Add(this.gc_satir_BV_Aciklama);
		this.gridBand2.Columns.Add(this.gc_satir_BV_IslemKodu);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.VisibleIndex = 2;
		this.gridBand2.Width = 257;
		this.gridBand3.Caption = "Veri kontrolü";
		this.gridBand3.Columns.Add(this.gc_satir_BV_Unvan);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Unvan2);
		this.gridBand3.Columns.Add(this.gc_satir_BV_TcVergiNo);
		this.gridBand3.Columns.Add(this.gc_satir_BV_HesapNo);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Telefon);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Eposta);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Adres);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Mahalle);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Ilce);
		this.gridBand3.Columns.Add(this.gc_satir_BV_PostaKodu);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Il);
		this.gridBand3.Columns.Add(this.gc_satir_BV_Ulke);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Unvan);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Unvan2);
		this.gridBand3.Columns.Add(this.gc_satir_MV_TcVergiNo);
		this.gridBand3.Columns.Add(this.gc_satir_MV_HesapNo);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Telefon);
		this.gridBand3.Columns.Add(this.gc_satir_MV_EPosta);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Adres);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Mahalle);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Ilce);
		this.gridBand3.Columns.Add(this.gc_satir_MV_PostaKodu);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Il);
		this.gridBand3.Columns.Add(this.gc_satir_MV_Ulke);
		this.gridBand3.Name = "gridBand3";
		this.gridBand3.VisibleIndex = 3;
		this.gridBand3.Width = 1315;
		this.gridBand1.Caption = "Fatura Hareketi";
		this.gridBand1.Columns.Add(this.gc_satir_FaturaDurumu);
		this.gridBand1.Columns.Add(this.gc_satir_FaturaSeri);
		this.gridBand1.Columns.Add(this.gc_satir_FaturaSira);
		this.gridBand1.Columns.Add(this.gc_satir_FaturaHesapKodu);
		this.gridBand1.Columns.Add(this.gc_satir_FaturaHesapAdi);
		this.gridBand1.Columns.Add(this.gc_satir_FaturaMiktar);
		this.gridBand1.Name = "gridBand1";
		this.gridBand1.VisibleIndex = 4;
		this.gridBand1.Width = 419;
		this.gridBand4.Caption = "İşlemler";
		this.gridBand4.Columns.Add(this.gc_cari_ekle);
		this.gridBand4.Name = "gridBand4";
		this.gridBand4.VisibleIndex = 5;
		this.gridBand4.Width = 75;
		this.gridBand6.Caption = "Mikro dışı ek veriler";
		this.gridBand6.Name = "gridBand6";
		this.gridBand6.VisibleIndex = 6;
		this.gridBand6.Width = 83;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1197, 520);
		base.Controls.Add(this.lc_dosya_kapanis_bakiyesi);
		base.Controls.Add(this.labelControl16);
		base.Controls.Add(this.lc_dosya_acilis_bakiyesi);
		base.Controls.Add(this.labelControl14);
		base.Controls.Add(this.lc_dosya_tarih_araligi);
		base.Controls.Add(this.labelControl12);
		base.Controls.Add(this.lc_aktarim_sonrasi_bakiye);
		base.Controls.Add(this.labelControl10);
		base.Controls.Add(this.lc_net_islem_tutari);
		base.Controls.Add(this.labelControl8);
		base.Controls.Add(this.lc_mevcut_bakiye);
		base.Controls.Add(this.labelControl6);
		base.Controls.Add(this.lc_banka_hesapno);
		base.Controls.Add(this.labelControl4);
		base.Controls.Add(this.lc_banka_kodu);
		base.Controls.Add(this.lc_banka_sube);
		base.Controls.Add(this.labelControl5);
		base.Controls.Add(this.lc_banka_adi);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.gridControl_evraklar);
		base.Controls.Add(this.label_islem_bilgi);
		base.Controls.Add(this.label_islem_adi);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.sb_Secili_Evraklari_aktar);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		base.Name = "TopluBankaEvrakGirisi";
		this.Text = "Toplu Banka Evrak Girişi";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(TopluGenelEvrakGirisi_FormClosing);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(TopluBankaEvrakGirisi_FormClosed);
		base.Load += new System.EventHandler(EvrakDuzenleme_Load);
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_satirlar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Cinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_SatirCinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_SatirAciklama).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemButtonEdit_Cari_Update).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Fatura_Durumu).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemButtonEdit_Cari_Ekle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridControl_evraklar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_specialalan).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_VergiPntr).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_IskontoSekli).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView1).EndInit();
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
