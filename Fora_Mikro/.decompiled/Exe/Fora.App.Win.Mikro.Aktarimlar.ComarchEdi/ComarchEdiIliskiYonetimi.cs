using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.ServiceModel;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using Fora.App.Win.Mikro.SR_EDI;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Aktarimlar.ComarchEdi;

public class ComarchEdiIliskiYonetimi : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _GenelParametreler;

	public Parametreler IliskiParametreleri;

	private bool DegisiklikVar;

	private string AktifSablon;

	private IContainer components;

	private ListBoxControl lb_sablonlar;

	private SimpleButton sb_ayarlari_kaydet;

	private XtraTabControl tc_parametreler;

	private XtraTabPage xtraTabPage_Genel;

	private TextEdit SorumlulukMerkeziKodu;

	private LabelControl labelControl1;

	private CheckEdit SiparisAktarimiAktif;

	private TextEdit HataBilgilendirmeEpostaAdresi;

	private LabelControl labelControl3;

	private TextEdit YeniSiparisBilgilendirmeEpostaAdresi;

	private LabelControl labelControl2;

	private TextEdit EvrakSeri;

	private LabelControl labelControl9;

	private TextEdit ProjeKodu;

	private LabelControl labelControl8;

	private LabelControl labelControl4;

	private TextEdit te_IliskiAdi;

	private CheckEdit BirimFiyatiBirim2KatsayisinaBol;

	private LabelControl labelControl6;

	private LabelControl labelControl5;

	public ComarchEdiIliskiYonetimi(MikroUygulamaBilgileri mikrouygulamabilgileri, Parametreler GenelParametreler)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_GenelParametreler = GenelParametreler;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		SablonlariListele();
	}

	private void SablonlariListele()
	{
		lb_sablonlar.Items.Clear();
		try
		{
			RetRes retRes = new EDIServiceSoapClient().Relationships(_GenelParametreler._GetParametre("KullaniciAdi")._GetString, _GenelParametreler._GetParametre("Sifre")._GetString, (int)_GenelParametreler._GetParametre("ZamanAsimi")._GetDouble * 1000);
			if (retRes.Res != "00000000")
			{
				string text = "İlişkiler çekilemedi!";
				switch (retRes.Res)
				{
				case "00000001":
					text = text + " Kullanıcı adı veya şifre hatalı! Hata kodu : " + retRes.Res;
					break;
				case "00000003":
					text = text + " Servis hatası! Hata kodu : " + retRes.Res;
					break;
				case "00000004":
					text = text + " Servis hatası! Hata kodu : " + retRes.Res;
					break;
				case "00000006":
					text = text + " Servis hatası! Hata kodu : " + retRes.Res;
					break;
				case "00000005":
					text = text + " Zaman aşımı süresi doldu! Hata kodu : " + retRes.Res;
					break;
				}
				MessageBox.Show(text, "HATA");
				return;
			}
			using XmlReader reader = XmlReader.Create(new StringReader(retRes.Cnt));
			foreach (XElement item in XDocument.Load(reader, LoadOptions.None).Descendants("relation"))
			{
				lb_sablonlar.Items.Add(item.Element("partner-name").Value);
			}
		}
		catch (EndpointNotFoundException ex)
		{
			MessageBox.Show("İlişkiler çekilemedi! Hata : Servise erişilemiyor. İnternet bağlantısını kontrol ediniz. Hata : " + ex.ToString(), "HATA");
		}
		catch (Exception ex2)
		{
			MessageBox.Show("İlişkiler çekilemedi! Hata : " + ex2.ToString(), "HATA");
		}
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		KullaniciParametreKaydet();
	}

	private void KullaniciParametreKaydet()
	{
		IliskiParametreleri._GetParametre("SiparisAktarimiAktif")._SetBoolean = SiparisAktarimiAktif.Checked;
		IliskiParametreleri._GetParametre("SorumlulukMerkeziKodu")._SetString = SorumlulukMerkeziKodu.Text;
		IliskiParametreleri._GetParametre("ProjeKodu")._SetString = ProjeKodu.Text;
		IliskiParametreleri._GetParametre("EvrakSeri")._SetString = EvrakSeri.Text;
		IliskiParametreleri._GetParametre("YeniSiparisBilgilendirmeEpostaAdresi")._SetString = YeniSiparisBilgilendirmeEpostaAdresi.Text;
		IliskiParametreleri._GetParametre("HataBilgilendirmeEpostaAdresi")._SetString = HataBilgilendirmeEpostaAdresi.Text;
		IliskiParametreleri._GetParametre("BirimFiyatiBirim2KatsayisinaBol")._SetBoolean = BirimFiyatiBirim2KatsayisinaBol.Checked;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, IliskiParametreleri);
		EkranBilgiGuncelle();
		DegisiklikVar = false;
	}

	private void lb_sablonlar_SelectedValueChanged(object sender, EventArgs e)
	{
		if (lb_sablonlar.SelectedValue == null)
		{
			return;
		}
		bool flag = true;
		if (DegisiklikVar && AktifSablon != lb_sablonlar.SelectedValue.ToString())
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
		if (AktifSablon == lb_sablonlar.SelectedValue.ToString())
		{
			flag = false;
		}
		if (flag)
		{
			tc_parametreler.Enabled = true;
			sb_ayarlari_kaydet.Enabled = true;
			AktifSablon = lb_sablonlar.SelectedValue.ToString();
			EkranBilgiGuncelle();
			DegisiklikVar = false;
		}
		else
		{
			lb_sablonlar.SelectedItem = AktifSablon;
		}
	}

	private void EkranBilgiGuncelle()
	{
		te_IliskiAdi.Text = AktifSablon;
		IliskiParametreleri = ParametrelerDefault.ComarchEdiIliskiParametreleri(AktifSablon);
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, IliskiParametreleri, "ComarchEdiIliski", AktifSablon, "", "");
		SiparisAktarimiAktif.Checked = IliskiParametreleri._GetParametre("SiparisAktarimiAktif")._GetBoolean;
		SorumlulukMerkeziKodu.Text = IliskiParametreleri._GetParametre("SorumlulukMerkeziKodu")._GetString;
		ProjeKodu.Text = IliskiParametreleri._GetParametre("ProjeKodu")._GetString;
		EvrakSeri.Text = IliskiParametreleri._GetParametre("EvrakSeri")._GetString;
		YeniSiparisBilgilendirmeEpostaAdresi.Text = IliskiParametreleri._GetParametre("YeniSiparisBilgilendirmeEpostaAdresi")._GetString;
		HataBilgilendirmeEpostaAdresi.Text = IliskiParametreleri._GetParametre("HataBilgilendirmeEpostaAdresi")._GetString;
		BirimFiyatiBirim2KatsayisinaBol.Checked = IliskiParametreleri._GetParametre("BirimFiyatiBirim2KatsayisinaBol")._GetBoolean;
	}

	private void SiparisAktarimiAktif_MouseDown(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreTextEdit_KeyDown(object sender, KeyEventArgs e)
	{
		DegisiklikVar = true;
	}

	private void ParametreCheckEditMouseClick(object sender, MouseEventArgs e)
	{
		DegisiklikVar = true;
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
		this.lb_sablonlar = new DevExpress.XtraEditors.ListBoxControl();
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.tc_parametreler = new DevExpress.XtraTab.XtraTabControl();
		this.xtraTabPage_Genel = new DevExpress.XtraTab.XtraTabPage();
		this.BirimFiyatiBirim2KatsayisinaBol = new DevExpress.XtraEditors.CheckEdit();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.te_IliskiAdi = new DevExpress.XtraEditors.TextEdit();
		this.HataBilgilendirmeEpostaAdresi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.YeniSiparisBilgilendirmeEpostaAdresi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.EvrakSeri = new DevExpress.XtraEditors.TextEdit();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.ProjeKodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.SiparisAktarimiAktif = new DevExpress.XtraEditors.CheckEdit();
		this.SorumlulukMerkeziKodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.lb_sablonlar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).BeginInit();
		this.tc_parametreler.SuspendLayout();
		this.xtraTabPage_Genel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.BirimFiyatiBirim2KatsayisinaBol.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_IliskiAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.HataBilgilendirmeEpostaAdresi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.YeniSiparisBilgilendirmeEpostaAdresi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakSeri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ProjeKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisAktarimiAktif.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SorumlulukMerkeziKodu.Properties).BeginInit();
		base.SuspendLayout();
		this.lb_sablonlar.Location = new System.Drawing.Point(12, 58);
		this.lb_sablonlar.Name = "lb_sablonlar";
		this.lb_sablonlar.Size = new System.Drawing.Size(153, 346);
		this.lb_sablonlar.TabIndex = 5;
		this.lb_sablonlar.SelectedValueChanged += new System.EventHandler(lb_sablonlar_SelectedValueChanged);
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.Enabled = false;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(562, 415);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(132, 23);
		this.sb_ayarlari_kaydet.TabIndex = 8;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.tc_parametreler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tc_parametreler.Location = new System.Drawing.Point(171, 35);
		this.tc_parametreler.Name = "tc_parametreler";
		this.tc_parametreler.SelectedTabPage = this.xtraTabPage_Genel;
		this.tc_parametreler.Size = new System.Drawing.Size(528, 374);
		this.tc_parametreler.TabIndex = 0;
		this.tc_parametreler.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[1] { this.xtraTabPage_Genel });
		this.xtraTabPage_Genel.Controls.Add(this.labelControl6);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl5);
		this.xtraTabPage_Genel.Controls.Add(this.BirimFiyatiBirim2KatsayisinaBol);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl4);
		this.xtraTabPage_Genel.Controls.Add(this.te_IliskiAdi);
		this.xtraTabPage_Genel.Controls.Add(this.HataBilgilendirmeEpostaAdresi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl3);
		this.xtraTabPage_Genel.Controls.Add(this.YeniSiparisBilgilendirmeEpostaAdresi);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl2);
		this.xtraTabPage_Genel.Controls.Add(this.EvrakSeri);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl9);
		this.xtraTabPage_Genel.Controls.Add(this.ProjeKodu);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl8);
		this.xtraTabPage_Genel.Controls.Add(this.SiparisAktarimiAktif);
		this.xtraTabPage_Genel.Controls.Add(this.SorumlulukMerkeziKodu);
		this.xtraTabPage_Genel.Controls.Add(this.labelControl1);
		this.xtraTabPage_Genel.Name = "xtraTabPage_Genel";
		this.xtraTabPage_Genel.Size = new System.Drawing.Size(522, 346);
		this.xtraTabPage_Genel.Text = "GENEL";
		this.BirimFiyatiBirim2KatsayisinaBol.Location = new System.Drawing.Point(229, 297);
		this.BirimFiyatiBirim2KatsayisinaBol.Name = "BirimFiyatiBirim2KatsayisinaBol";
		this.BirimFiyatiBirim2KatsayisinaBol.Properties.Caption = "Birim fiyatı, birim 2 katsayısına böl";
		this.BirimFiyatiBirim2KatsayisinaBol.Size = new System.Drawing.Size(207, 19);
		this.BirimFiyatiBirim2KatsayisinaBol.TabIndex = 129;
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(58, 20);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(57, 13);
		this.labelControl4.TabIndex = 128;
		this.labelControl4.Text = "İlişki adı :";
		this.te_IliskiAdi.Enabled = false;
		this.te_IliskiAdi.Location = new System.Drawing.Point(121, 17);
		this.te_IliskiAdi.Name = "te_IliskiAdi";
		this.te_IliskiAdi.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.te_IliskiAdi.Properties.Appearance.Options.UseForeColor = true;
		this.te_IliskiAdi.Size = new System.Drawing.Size(147, 20);
		this.te_IliskiAdi.TabIndex = 127;
		this.HataBilgilendirmeEpostaAdresi.Location = new System.Drawing.Point(229, 259);
		this.HataBilgilendirmeEpostaAdresi.Name = "HataBilgilendirmeEpostaAdresi";
		this.HataBilgilendirmeEpostaAdresi.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.HataBilgilendirmeEpostaAdresi.Properties.Appearance.Options.UseForeColor = true;
		this.HataBilgilendirmeEpostaAdresi.Size = new System.Drawing.Size(147, 20);
		this.HataBilgilendirmeEpostaAdresi.TabIndex = 126;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(17, 262);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(206, 13);
		this.labelControl3.TabIndex = 125;
		this.labelControl3.Text = "Hata bilgilendirme e-posta adresi :";
		this.YeniSiparisBilgilendirmeEpostaAdresi.Location = new System.Drawing.Point(229, 233);
		this.YeniSiparisBilgilendirmeEpostaAdresi.Name = "YeniSiparisBilgilendirmeEpostaAdresi";
		this.YeniSiparisBilgilendirmeEpostaAdresi.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.YeniSiparisBilgilendirmeEpostaAdresi.Properties.Appearance.Options.UseForeColor = true;
		this.YeniSiparisBilgilendirmeEpostaAdresi.Size = new System.Drawing.Size(147, 20);
		this.YeniSiparisBilgilendirmeEpostaAdresi.TabIndex = 124;
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(17, 236);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(206, 13);
		this.labelControl2.TabIndex = 123;
		this.labelControl2.Text = "Yeni sipariş bilgilendirme e-posta adresi :";
		this.EvrakSeri.Location = new System.Drawing.Point(229, 207);
		this.EvrakSeri.Name = "EvrakSeri";
		this.EvrakSeri.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.EvrakSeri.Properties.Appearance.Options.UseForeColor = true;
		this.EvrakSeri.Size = new System.Drawing.Size(147, 20);
		this.EvrakSeri.TabIndex = 122;
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(76, 210);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(147, 13);
		this.labelControl9.TabIndex = 121;
		this.labelControl9.Text = "Evrak seri :";
		this.ProjeKodu.Location = new System.Drawing.Point(229, 181);
		this.ProjeKodu.Name = "ProjeKodu";
		this.ProjeKodu.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.ProjeKodu.Properties.Appearance.Options.UseForeColor = true;
		this.ProjeKodu.Size = new System.Drawing.Size(147, 20);
		this.ProjeKodu.TabIndex = 120;
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(76, 184);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(147, 13);
		this.labelControl8.TabIndex = 119;
		this.labelControl8.Text = "Proje kodu :";
		this.SiparisAktarimiAktif.Location = new System.Drawing.Point(229, 75);
		this.SiparisAktarimiAktif.Name = "SiparisAktarimiAktif";
		this.SiparisAktarimiAktif.Properties.Caption = "Sipariş aktarımı aktif";
		this.SiparisAktarimiAktif.Size = new System.Drawing.Size(147, 19);
		this.SiparisAktarimiAktif.TabIndex = 118;
		this.SiparisAktarimiAktif.MouseDown += new System.Windows.Forms.MouseEventHandler(SiparisAktarimiAktif_MouseDown);
		this.SorumlulukMerkeziKodu.Location = new System.Drawing.Point(229, 100);
		this.SorumlulukMerkeziKodu.Name = "SorumlulukMerkeziKodu";
		this.SorumlulukMerkeziKodu.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
		this.SorumlulukMerkeziKodu.Properties.Appearance.Options.UseForeColor = true;
		this.SorumlulukMerkeziKodu.Size = new System.Drawing.Size(147, 20);
		this.SorumlulukMerkeziKodu.TabIndex = 4;
		this.SorumlulukMerkeziKodu.KeyDown += new System.Windows.Forms.KeyEventHandler(ParametreTextEdit_KeyDown);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(76, 103);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(147, 13);
		this.labelControl1.TabIndex = 3;
		this.labelControl1.Text = "Sorumluluk merkezi kodu :";
		this.labelControl5.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(229, 126);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(290, 16);
		this.labelControl5.TabIndex = 130;
		this.labelControl5.Text = "Farklı CodeByBuyer kodları için virgülle ayrılmış yazılabilir.";
		this.labelControl6.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(229, 148);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(290, 16);
		this.labelControl6.TabIndex = 131;
		this.labelControl6.Text = "Örn : 13595,A-DONUK,13418,A-TAZE";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(707, 449);
		base.Controls.Add(this.tc_parametreler);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Controls.Add(this.lb_sablonlar);
		base.Name = "ComarchEdiIliskiYonetimi";
		this.Text = "Comarch Edi ilişki yönetimi";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		((System.ComponentModel.ISupportInitialize)this.lb_sablonlar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.tc_parametreler).EndInit();
		this.tc_parametreler.ResumeLayout(false);
		this.xtraTabPage_Genel.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.BirimFiyatiBirim2KatsayisinaBol.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_IliskiAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.HataBilgilendirmeEpostaAdresi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.YeniSiparisBilgilendirmeEpostaAdresi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EvrakSeri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ProjeKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SiparisAktarimiAktif.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SorumlulukMerkeziKodu.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
