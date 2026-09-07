using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.B2B;

public class B2BPArametreleriForm : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _b2bparametreleri;

	private IContainer components;

	private SimpleButton sb_ayarlari_kaydet;

	private XtraTabControl xtraTabControl2;

	private XtraTabPage xtraTabPage3;

	private System.Windows.Forms.ComboBox EvrakMikroUserNo;

	private LabelControl labelControl37;

	private LabelControl labelControl17;

	private System.Windows.Forms.ComboBox EvrakTipi;

	private LabelControl labelControl2;

	private LabelControl labelControl65;

	private TextEdit SiteBasligi;

	private LabelControl labelControl62;

	private TextEdit EvrakSeri;

	private LabelControl labelControl1;

	private TextEdit KaynakDepoNo;

	private LabelControl labelControl3;

	private CheckEdit SiparisGirisiZamanKontrollu;

	private TextEdit SiparisGirisiSonlanmaZamani;

	private LabelControl labelControl5;

	private TextEdit SiparisGirisiBaslamaZamani;

	private LabelControl labelControl4;

	private LabelControl labelControl6;

	private System.Windows.Forms.ComboBox StokListelemeWebSayfasinaGonderilecek;

	private LabelControl labelControl69;

	private System.Windows.Forms.ComboBox DepolarArasiSevkFiyat;

	private LabelControl labelControl7;

	private TextEdit SiparisGirisiZamanKontrolluSifre;

	private LabelControl labelControl8;

	public B2BPArametreleriForm(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		DataTable dataSource = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 5, "Alınan sipariş" },
				new object[2] { 9, "Depolar arası sipariş" }
			}
		};
		EvrakTipi.DataSource = dataSource;
		EvrakTipi.ValueMember = "ID";
		EvrakTipi.DisplayMember = "Isim";
		DataTable dataSource2 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = 
			{
				new object[2] { 0, "Hepsi" },
				new object[2] { 1, "Web sayfasına gönderilecekler" },
				new object[2] { 2, "Web sayfasına gönderilmeyecekler" }
			}
		};
		StokListelemeWebSayfasinaGonderilecek.DataSource = dataSource2;
		StokListelemeWebSayfasinaGonderilecek.ValueMember = "ID";
		StokListelemeWebSayfasinaGonderilecek.DisplayMember = "Isim";
		DataTable dataSource3 = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(int)
				},
				{
					"Isim",
					typeof(string)
				}
			},
			Rows = { new object[2] { 1, "Stok tanıtım kartındaki standart maliyet" } }
		};
		DepolarArasiSevkFiyat.DataSource = dataSource3;
		DepolarArasiSevkFiyat.ValueMember = "ID";
		DepolarArasiSevkFiyat.DisplayMember = "Isim";
		DataTable kullanicilarDataTable = MikroKullaniciData.GetKullanicilarDataTable(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName);
		EvrakMikroUserNo.DataSource = kullanicilarDataTable;
		EvrakMikroUserNo.ValueMember = "KULLANICI NO";
		EvrakMikroUserNo.DisplayMember = "KULLANICI ADI";
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
		_b2bparametreleri = ParametrelerDefault.B2B();
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _b2bparametreleri, "b2b", "", "", "");
		EkranGuncelle();
	}

	private void EkranGuncelle()
	{
		SiteBasligi.Text = _b2bparametreleri._GetParametre("SiteBasligi")._GetString;
		EvrakMikroUserNo.SelectedValue = _b2bparametreleri._GetParametre("EvrakMikroUserNo")._GetInt;
		EvrakTipi.SelectedValue = _b2bparametreleri._GetParametre("EvrakTipi")._GetInt;
		EvrakSeri.Text = _b2bparametreleri._GetParametre("EvrakSeri")._GetString;
		KaynakDepoNo.Text = _b2bparametreleri._GetParametre("KaynakDepoNo")._GetString;
		SiparisGirisiZamanKontrollu.Checked = _b2bparametreleri._GetParametre("SiparisGirisiZamanKontrollu")._GetBoolean;
		SiparisGirisiBaslamaZamani.Text = _b2bparametreleri._GetParametre("SiparisGirisiBaslamaZamani")._GetString;
		SiparisGirisiSonlanmaZamani.Text = _b2bparametreleri._GetParametre("SiparisGirisiSonlanmaZamani")._GetString;
		StokListelemeWebSayfasinaGonderilecek.SelectedValue = _b2bparametreleri._GetParametre("StokListelemeWebSayfasinaGonderilecek")._GetInt;
		DepolarArasiSevkFiyat.SelectedValue = _b2bparametreleri._GetParametre("DepolarArasiSevkFiyat")._GetInt;
		SiparisGirisiZamanKontrolluSifre.Text = _b2bparametreleri._GetParametre("SiparisGirisiZamanKontrolluSifre")._GetString;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		_b2bparametreleri._GetParametre("SiteBasligi")._SetString = SiteBasligi.Text;
		_b2bparametreleri._GetParametre("EvrakMikroUserNo")._SetInt = (int)EvrakMikroUserNo.SelectedValue;
		_b2bparametreleri._GetParametre("EvrakTipi")._SetInt = (int)EvrakTipi.SelectedValue;
		_b2bparametreleri._GetParametre("EvrakSeri")._SetString = EvrakSeri.Text;
		_b2bparametreleri._GetParametre("KaynakDepoNo")._SetString = KaynakDepoNo.Text;
		_b2bparametreleri._GetParametre("SiparisGirisiZamanKontrollu")._SetBoolean = SiparisGirisiZamanKontrollu.Checked;
		_b2bparametreleri._GetParametre("SiparisGirisiBaslamaZamani")._SetString = SiparisGirisiBaslamaZamani.Text;
		_b2bparametreleri._GetParametre("SiparisGirisiSonlanmaZamani")._SetString = SiparisGirisiSonlanmaZamani.Text;
		_b2bparametreleri._GetParametre("StokListelemeWebSayfasinaGonderilecek")._SetInt = (int)StokListelemeWebSayfasinaGonderilecek.SelectedValue;
		_b2bparametreleri._GetParametre("DepolarArasiSevkFiyat")._SetInt = (int)DepolarArasiSevkFiyat.SelectedValue;
		_b2bparametreleri._GetParametre("SiparisGirisiZamanKontrolluSifre")._SetString = SiparisGirisiZamanKontrolluSifre.Text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _b2bparametreleri);
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
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.DepolarArasiSevkFiyat = new System.Windows.Forms.ComboBox();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.StokListelemeWebSayfasinaGonderilecek = new System.Windows.Forms.ComboBox();
		this.labelControl69 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.SiparisGirisiSonlanmaZamani = new DevExpress.XtraEditors.TextEdit();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.SiparisGirisiBaslamaZamani = new DevExpress.XtraEditors.TextEdit();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.SiparisGirisiZamanKontrollu = new DevExpress.XtraEditors.CheckEdit();
		this.KaynakDepoNo = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.EvrakSeri = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.SiteBasligi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl62 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl65 = new DevExpress.XtraEditors.LabelControl();
		this.EvrakTipi = new System.Windows.Forms.ComboBox();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.EvrakMikroUserNo = new System.Windows.Forms.ComboBox();
		this.labelControl37 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl17 = new DevExpress.XtraEditors.LabelControl();
		this.SiparisGirisiZamanKontrolluSifre = new DevExpress.XtraEditors.TextEdit();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).BeginInit();
		this.xtraTabControl2.SuspendLayout();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiSonlanmaZamani.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiBaslamaZamani.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiZamanKontrollu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.KaynakDepoNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SiteBasligi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiZamanKontrolluSifre.Properties).BeginInit();
		base.SuspendLayout();
		this.sb_ayarlari_kaydet.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(511, 458);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(214, 24);
		this.sb_ayarlari_kaydet.TabIndex = 9;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.xtraTabControl2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.xtraTabControl2.Location = new System.Drawing.Point(12, 12);
		this.xtraTabControl2.Name = "xtraTabControl2";
		this.xtraTabControl2.SelectedTabPage = this.xtraTabPage3;
		this.xtraTabControl2.Size = new System.Drawing.Size(713, 440);
		this.xtraTabControl2.TabIndex = 10;
		this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[1] { this.xtraTabPage3 });
		this.xtraTabPage3.Controls.Add(this.SiparisGirisiZamanKontrolluSifre);
		this.xtraTabPage3.Controls.Add(this.labelControl8);
		this.xtraTabPage3.Controls.Add(this.DepolarArasiSevkFiyat);
		this.xtraTabPage3.Controls.Add(this.labelControl7);
		this.xtraTabPage3.Controls.Add(this.StokListelemeWebSayfasinaGonderilecek);
		this.xtraTabPage3.Controls.Add(this.labelControl69);
		this.xtraTabPage3.Controls.Add(this.labelControl6);
		this.xtraTabPage3.Controls.Add(this.SiparisGirisiSonlanmaZamani);
		this.xtraTabPage3.Controls.Add(this.labelControl5);
		this.xtraTabPage3.Controls.Add(this.SiparisGirisiBaslamaZamani);
		this.xtraTabPage3.Controls.Add(this.labelControl4);
		this.xtraTabPage3.Controls.Add(this.SiparisGirisiZamanKontrollu);
		this.xtraTabPage3.Controls.Add(this.KaynakDepoNo);
		this.xtraTabPage3.Controls.Add(this.labelControl3);
		this.xtraTabPage3.Controls.Add(this.EvrakSeri);
		this.xtraTabPage3.Controls.Add(this.labelControl1);
		this.xtraTabPage3.Controls.Add(this.SiteBasligi);
		this.xtraTabPage3.Controls.Add(this.labelControl62);
		this.xtraTabPage3.Controls.Add(this.labelControl65);
		this.xtraTabPage3.Controls.Add(this.EvrakTipi);
		this.xtraTabPage3.Controls.Add(this.labelControl2);
		this.xtraTabPage3.Controls.Add(this.EvrakMikroUserNo);
		this.xtraTabPage3.Controls.Add(this.labelControl37);
		this.xtraTabPage3.Controls.Add(this.labelControl17);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(707, 412);
		this.xtraTabPage3.Text = "Genel";
		this.DepolarArasiSevkFiyat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.DepolarArasiSevkFiyat.FormattingEnabled = true;
		this.DepolarArasiSevkFiyat.Location = new System.Drawing.Point(169, 379);
		this.DepolarArasiSevkFiyat.Name = "DepolarArasiSevkFiyat";
		this.DepolarArasiSevkFiyat.Size = new System.Drawing.Size(203, 21);
		this.DepolarArasiSevkFiyat.TabIndex = 243;
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(11, 382);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(152, 13);
		this.labelControl7.TabIndex = 242;
		this.labelControl7.Text = "Depolar arası sipariş için fiyat :";
		this.StokListelemeWebSayfasinaGonderilecek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.StokListelemeWebSayfasinaGonderilecek.FormattingEnabled = true;
		this.StokListelemeWebSayfasinaGonderilecek.Location = new System.Drawing.Point(169, 352);
		this.StokListelemeWebSayfasinaGonderilecek.Name = "StokListelemeWebSayfasinaGonderilecek";
		this.StokListelemeWebSayfasinaGonderilecek.Size = new System.Drawing.Size(203, 21);
		this.StokListelemeWebSayfasinaGonderilecek.TabIndex = 241;
		this.labelControl69.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl69.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl69.Location = new System.Drawing.Point(49, 355);
		this.labelControl69.Name = "labelControl69";
		this.labelControl69.Size = new System.Drawing.Size(114, 13);
		this.labelControl69.TabIndex = 240;
		this.labelControl69.Text = "Gösterilecek stoklar :";
		this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(58, 329);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(126, 19);
		this.labelControl6.TabIndex = 239;
		this.labelControl6.Text = "Stoklar";
		this.SiparisGirisiSonlanmaZamani.Location = new System.Drawing.Point(169, 250);
		this.SiparisGirisiSonlanmaZamani.Name = "SiparisGirisiSonlanmaZamani";
		this.SiparisGirisiSonlanmaZamani.Size = new System.Drawing.Size(106, 20);
		this.SiparisGirisiSonlanmaZamani.TabIndex = 236;
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(58, 253);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(105, 13);
		this.labelControl5.TabIndex = 235;
		this.labelControl5.Text = "Bitiş zamanı :";
		this.SiparisGirisiBaslamaZamani.Location = new System.Drawing.Point(169, 224);
		this.SiparisGirisiBaslamaZamani.Name = "SiparisGirisiBaslamaZamani";
		this.SiparisGirisiBaslamaZamani.Size = new System.Drawing.Size(106, 20);
		this.SiparisGirisiBaslamaZamani.TabIndex = 234;
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(58, 227);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(105, 13);
		this.labelControl4.TabIndex = 233;
		this.labelControl4.Text = "Başlama zamanı :";
		this.SiparisGirisiZamanKontrollu.Location = new System.Drawing.Point(167, 199);
		this.SiparisGirisiZamanKontrollu.Name = "SiparisGirisiZamanKontrollu";
		this.SiparisGirisiZamanKontrollu.Properties.Caption = "Zaman kontrolü sipariş girilsin";
		this.SiparisGirisiZamanKontrollu.Size = new System.Drawing.Size(193, 19);
		this.SiparisGirisiZamanKontrollu.TabIndex = 232;
		this.KaynakDepoNo.Location = new System.Drawing.Point(169, 159);
		this.KaynakDepoNo.Name = "KaynakDepoNo";
		this.KaynakDepoNo.Size = new System.Drawing.Size(106, 20);
		this.KaynakDepoNo.TabIndex = 231;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(58, 162);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(105, 13);
		this.labelControl3.TabIndex = 230;
		this.labelControl3.Text = "Kaynak depo no :";
		this.EvrakSeri.Location = new System.Drawing.Point(169, 133);
		this.EvrakSeri.Name = "EvrakSeri";
		this.EvrakSeri.Size = new System.Drawing.Size(106, 20);
		this.EvrakSeri.TabIndex = 229;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(58, 136);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(105, 13);
		this.labelControl1.TabIndex = 228;
		this.labelControl1.Text = "Evrak seri :";
		this.SiteBasligi.Location = new System.Drawing.Point(169, 13);
		this.SiteBasligi.Name = "SiteBasligi";
		this.SiteBasligi.Size = new System.Drawing.Size(407, 20);
		this.SiteBasligi.TabIndex = 227;
		this.labelControl62.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl62.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl62.Location = new System.Drawing.Point(58, 16);
		this.labelControl62.Name = "labelControl62";
		this.labelControl62.Size = new System.Drawing.Size(105, 13);
		this.labelControl62.TabIndex = 226;
		this.labelControl62.Text = "Site başlığı :";
		this.labelControl65.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl65.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl65.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl65.Location = new System.Drawing.Point(58, 81);
		this.labelControl65.Name = "labelControl65";
		this.labelControl65.Size = new System.Drawing.Size(126, 19);
		this.labelControl65.TabIndex = 225;
		this.labelControl65.Text = "Sipariş";
		this.EvrakTipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.EvrakTipi.FormattingEnabled = true;
		this.EvrakTipi.Location = new System.Drawing.Point(169, 106);
		this.EvrakTipi.Name = "EvrakTipi";
		this.EvrakTipi.Size = new System.Drawing.Size(106, 21);
		this.EvrakTipi.TabIndex = 224;
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(46, 109);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(117, 13);
		this.labelControl2.TabIndex = 222;
		this.labelControl2.Text = "Evrak tipi :";
		this.EvrakMikroUserNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.EvrakMikroUserNo.FormattingEnabled = true;
		this.EvrakMikroUserNo.Location = new System.Drawing.Point(169, 39);
		this.EvrakMikroUserNo.Name = "EvrakMikroUserNo";
		this.EvrakMikroUserNo.Size = new System.Drawing.Size(106, 21);
		this.EvrakMikroUserNo.TabIndex = 221;
		this.labelControl37.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl37.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl37.Location = new System.Drawing.Point(281, 42);
		this.labelControl37.Name = "labelControl37";
		this.labelControl37.Size = new System.Drawing.Size(399, 13);
		this.labelControl37.TabIndex = 220;
		this.labelControl37.Text = "Oluşturulacak evraklar hangi kullanıcı tarafından kayıt edilmiş olarak gösterilecek";
		this.labelControl17.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl17.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl17.Location = new System.Drawing.Point(57, 42);
		this.labelControl17.Name = "labelControl17";
		this.labelControl17.Size = new System.Drawing.Size(106, 13);
		this.labelControl17.TabIndex = 219;
		this.labelControl17.Text = "Bağlı mikro kullanıcısı :";
		this.SiparisGirisiZamanKontrolluSifre.Location = new System.Drawing.Point(169, 276);
		this.SiparisGirisiZamanKontrolluSifre.Name = "SiparisGirisiZamanKontrolluSifre";
		this.SiparisGirisiZamanKontrolluSifre.Properties.UseSystemPasswordChar = true;
		this.SiparisGirisiZamanKontrolluSifre.Size = new System.Drawing.Size(106, 20);
		this.SiparisGirisiZamanKontrolluSifre.TabIndex = 245;
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(58, 279);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(105, 13);
		this.labelControl8.TabIndex = 244;
		this.labelControl8.Text = "Özel şifre :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(735, 492);
		base.Controls.Add(this.xtraTabControl2);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Name = "B2BPArametreleriForm";
		this.Text = "B2B Parametreleri";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).EndInit();
		this.xtraTabControl2.ResumeLayout(false);
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiSonlanmaZamani.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiBaslamaZamani.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiZamanKontrollu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.KaynakDepoNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SiteBasligi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisGirisiZamanKontrolluSifre.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
