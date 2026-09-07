using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Aktarimlar.ComarchEdi;

public class ComarchEdiGenelParametrelerForm : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler GenelParametreler;

	private IContainer components;

	private SimpleButton sb_ayarlari_kaydet;

	private TextEdit KullaniciAdi;

	private SpinEdit ZamanAsimi;

	private Label label1;

	private Label label2;

	private TextEdit Sifre;

	private Label label3;

	private Label label4;

	private SpinEdit SenkronizasyonSuresi;

	private Label label5;

	private TextEdit SpecialAlan1;

	private Label label6;

	private TextEdit SpecialAlan2;

	private Label label7;

	private TextEdit SpecialAlan3;

	private CheckEdit EMailSmtpUseSSL;

	private TextEdit EMailGorunenAd;

	private LabelControl labelControl212;

	private TextEdit EMailSmtpPassword;

	private LabelControl labelControl211;

	private TextEdit EMailAdress;

	private LabelControl labelControl210;

	private LabelControl labelControl209;

	private TextEdit EMailSmtpServer;

	private LabelControl labelControl208;

	private LabelControl labelControl175;

	private LabelControl labelControl1;

	private LabelControl labelControl2;

	private Label label8;

	private Label label9;

	private SpinEdit EMailSmtpPort;

	private Label label10;

	private Label label11;

	private SimpleButton simpleButton1;

	private TextEdit te_test_mail_adresi;

	private LabelControl labelControl3;

	private CheckEdit AktarilanKayitlariOkunduOlarakIsaretle;

	private Label label12;

	public ComarchEdiGenelParametrelerForm(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
		EkranGuncelle();
	}

	private void EkranGuncelle()
	{
		GenelParametreler = ParametrelerDefault.ComarchEdiGenelParametreler();
		ParametreData.ParametreOku(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, GenelParametreler, "ComarchEdiGenel", "", "", "");
		KullaniciAdi.Text = GenelParametreler._GetParametre("KullaniciAdi")._GetString;
		Sifre.Text = GenelParametreler._GetParametre("Sifre")._GetString;
		ZamanAsimi.Value = (decimal)GenelParametreler._GetParametre("ZamanAsimi")._GetDouble;
		SenkronizasyonSuresi.Value = (decimal)GenelParametreler._GetParametre("SenkronizasyonSuresi")._GetDouble;
		SpecialAlan1.Text = GenelParametreler._GetParametre("SpecialAlan1")._GetString;
		SpecialAlan2.Text = GenelParametreler._GetParametre("SpecialAlan2")._GetString;
		SpecialAlan3.Text = GenelParametreler._GetParametre("SpecialAlan3")._GetString;
		EMailSmtpServer.Text = GenelParametreler._GetParametre("EMailSmtpServer")._GetString;
		EMailSmtpPort.Value = (decimal)GenelParametreler._GetParametre("EMailSmtpPort")._GetDouble;
		EMailAdress.Text = GenelParametreler._GetParametre("EMailAdress")._GetString;
		EMailSmtpPassword.Text = GenelParametreler._GetParametre("EMailSmtpPassword")._GetString;
		EMailSmtpUseSSL.Checked = GenelParametreler._GetParametre("EMailSmtpUseSSL")._GetBoolean;
		EMailGorunenAd.Text = GenelParametreler._GetParametre("EMailGorunenAd")._GetString;
		AktarilanKayitlariOkunduOlarakIsaretle.Checked = GenelParametreler._GetParametre("AktarilanKayitlariOkunduOlarakIsaretle")._GetBoolean;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		GenelParametreler._GetParametre("KullaniciAdi")._SetString = KullaniciAdi.Text;
		GenelParametreler._GetParametre("Sifre")._SetString = Sifre.Text;
		GenelParametreler._GetParametre("ZamanAsimi")._SetDouble = (double)ZamanAsimi.Value;
		GenelParametreler._GetParametre("SenkronizasyonSuresi")._SetDouble = (double)SenkronizasyonSuresi.Value;
		GenelParametreler._GetParametre("SpecialAlan1")._SetString = SpecialAlan1.Text;
		GenelParametreler._GetParametre("SpecialAlan2")._SetString = SpecialAlan2.Text;
		GenelParametreler._GetParametre("SpecialAlan3")._SetString = SpecialAlan3.Text;
		GenelParametreler._GetParametre("EMailSmtpServer")._SetString = EMailSmtpServer.Text;
		GenelParametreler._GetParametre("EMailSmtpPort")._SetDouble = (double)EMailSmtpPort.Value;
		GenelParametreler._GetParametre("EMailAdress")._SetString = EMailAdress.Text;
		GenelParametreler._GetParametre("EMailSmtpPassword")._SetString = EMailSmtpPassword.Text;
		GenelParametreler._GetParametre("EMailSmtpUseSSL")._SetBoolean = EMailSmtpUseSSL.Checked;
		GenelParametreler._GetParametre("EMailGorunenAd")._SetString = EMailGorunenAd.Text;
		GenelParametreler._GetParametre("AktarilanKayitlariOkunduOlarakIsaretle")._SetBoolean = AktarilanKayitlariOkunduOlarakIsaretle.Checked;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, GenelParametreler);
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void simpleButton1_Click(object sender, EventArgs e)
	{
		try
		{
			MailMessage mailMessage = new MailMessage();
			SmtpClient smtpClient = new SmtpClient(EMailSmtpServer.Text);
			mailMessage.From = new MailAddress(EMailAdress.Text, EMailGorunenAd.Text, Encoding.UTF8);
			string[] array = te_test_mail_adresi.Text.Split(';');
			foreach (string text in array)
			{
				if (text != "")
				{
					mailMessage.To.Add(text);
				}
			}
			mailMessage.Subject = "EDI Aktarım hatalar test mesajı";
			mailMessage.Body = "Bu bir test mesajıdır.";
			smtpClient.Port = (int)EMailSmtpPort.Value;
			smtpClient.Credentials = new NetworkCredential(EMailAdress.Text, EMailSmtpPassword.Text);
			if (EMailSmtpUseSSL.Checked)
			{
				smtpClient.EnableSsl = true;
			}
			else
			{
				smtpClient.EnableSsl = false;
			}
			smtpClient.Send(mailMessage);
		}
		catch (Exception ex)
		{
			MessageBox.Show("E-postası gönderilemedi. Ayarları kontrol ediniz. Hata : " + ex.ToString());
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
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.ZamanAsimi = new DevExpress.XtraEditors.SpinEdit();
		this.KullaniciAdi = new DevExpress.XtraEditors.TextEdit();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.Sifre = new DevExpress.XtraEditors.TextEdit();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.SenkronizasyonSuresi = new DevExpress.XtraEditors.SpinEdit();
		this.label5 = new System.Windows.Forms.Label();
		this.SpecialAlan1 = new DevExpress.XtraEditors.TextEdit();
		this.label6 = new System.Windows.Forms.Label();
		this.SpecialAlan2 = new DevExpress.XtraEditors.TextEdit();
		this.label7 = new System.Windows.Forms.Label();
		this.SpecialAlan3 = new DevExpress.XtraEditors.TextEdit();
		this.EMailSmtpUseSSL = new DevExpress.XtraEditors.CheckEdit();
		this.EMailGorunenAd = new DevExpress.XtraEditors.TextEdit();
		this.labelControl212 = new DevExpress.XtraEditors.LabelControl();
		this.EMailSmtpPassword = new DevExpress.XtraEditors.TextEdit();
		this.labelControl211 = new DevExpress.XtraEditors.LabelControl();
		this.EMailAdress = new DevExpress.XtraEditors.TextEdit();
		this.labelControl210 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl209 = new DevExpress.XtraEditors.LabelControl();
		this.EMailSmtpServer = new DevExpress.XtraEditors.TextEdit();
		this.labelControl208 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl175 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.EMailSmtpPort = new DevExpress.XtraEditors.SpinEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
		this.te_test_mail_adresi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.AktarilanKayitlariOkunduOlarakIsaretle = new DevExpress.XtraEditors.CheckEdit();
		this.label12 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.ZamanAsimi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.KullaniciAdi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Sifre.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SenkronizasyonSuresi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SpecialAlan1.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SpecialAlan2.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SpecialAlan3.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpUseSSL.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EMailGorunenAd.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpPassword.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EMailAdress.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpServer.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpPort.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_test_mail_adresi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AktarilanKayitlariOkunduOlarakIsaretle.Properties).BeginInit();
		base.SuspendLayout();
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(12, 589);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(440, 24);
		this.sb_ayarlari_kaydet.TabIndex = 14;
		this.sb_ayarlari_kaydet.Text = "AYARLARI KAYDET";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.ZamanAsimi.EditValue = new decimal(new int[4]);
		this.ZamanAsimi.Location = new System.Drawing.Point(224, 149);
		this.ZamanAsimi.Name = "ZamanAsimi";
		this.ZamanAsimi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.ZamanAsimi.Size = new System.Drawing.Size(67, 20);
		this.ZamanAsimi.TabIndex = 3;
		this.KullaniciAdi.Location = new System.Drawing.Point(224, 48);
		this.KullaniciAdi.Name = "KullaniciAdi";
		this.KullaniciAdi.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.KullaniciAdi.Properties.Appearance.Options.UseForeColor = true;
		this.KullaniciAdi.Size = new System.Drawing.Size(213, 20);
		this.KullaniciAdi.TabIndex = 1;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(147, 51);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(71, 13);
		this.label1.TabIndex = 182;
		this.label1.Text = "Kullanıcı adı : ";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(179, 77);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(39, 13);
		this.label2.TabIndex = 184;
		this.label2.Text = "Şifre : ";
		this.Sifre.Location = new System.Drawing.Point(224, 74);
		this.Sifre.Name = "Sifre";
		this.Sifre.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.Sifre.Properties.Appearance.Options.UseForeColor = true;
		this.Sifre.Size = new System.Drawing.Size(213, 20);
		this.Sifre.TabIndex = 2;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(143, 152);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(75, 13);
		this.label3.TabIndex = 185;
		this.label3.Text = "Zaman aşımı : ";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(95, 178);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(123, 13);
		this.label4.TabIndex = 187;
		this.label4.Text = "Senkronizasyon aralığı : ";
		this.SenkronizasyonSuresi.EditValue = new decimal(new int[4]);
		this.SenkronizasyonSuresi.Location = new System.Drawing.Point(224, 175);
		this.SenkronizasyonSuresi.Name = "SenkronizasyonSuresi";
		this.SenkronizasyonSuresi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.SenkronizasyonSuresi.Size = new System.Drawing.Size(67, 20);
		this.SenkronizasyonSuresi.TabIndex = 4;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(148, 242);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(70, 13);
		this.label5.TabIndex = 189;
		this.label5.Text = "Özel alan 1 : ";
		this.SpecialAlan1.Location = new System.Drawing.Point(224, 239);
		this.SpecialAlan1.Name = "SpecialAlan1";
		this.SpecialAlan1.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.SpecialAlan1.Properties.Appearance.Options.UseForeColor = true;
		this.SpecialAlan1.Properties.MaxLength = 4;
		this.SpecialAlan1.Size = new System.Drawing.Size(67, 20);
		this.SpecialAlan1.TabIndex = 5;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(148, 268);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(70, 13);
		this.label6.TabIndex = 191;
		this.label6.Text = "Özel alan 2 : ";
		this.SpecialAlan2.Location = new System.Drawing.Point(224, 265);
		this.SpecialAlan2.Name = "SpecialAlan2";
		this.SpecialAlan2.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.SpecialAlan2.Properties.Appearance.Options.UseForeColor = true;
		this.SpecialAlan2.Properties.MaxLength = 4;
		this.SpecialAlan2.Size = new System.Drawing.Size(67, 20);
		this.SpecialAlan2.TabIndex = 6;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(148, 294);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(70, 13);
		this.label7.TabIndex = 193;
		this.label7.Text = "Özel alan 3 : ";
		this.SpecialAlan3.Location = new System.Drawing.Point(224, 291);
		this.SpecialAlan3.Name = "SpecialAlan3";
		this.SpecialAlan3.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(32, 31, 53);
		this.SpecialAlan3.Properties.Appearance.Options.UseForeColor = true;
		this.SpecialAlan3.Properties.MaxLength = 4;
		this.SpecialAlan3.Size = new System.Drawing.Size(67, 20);
		this.SpecialAlan3.TabIndex = 7;
		this.EMailSmtpUseSSL.Location = new System.Drawing.Point(224, 417);
		this.EMailSmtpUseSSL.Name = "EMailSmtpUseSSL";
		this.EMailSmtpUseSSL.Properties.Caption = "SSL kullan";
		this.EMailSmtpUseSSL.Size = new System.Drawing.Size(80, 19);
		this.EMailSmtpUseSSL.TabIndex = 10;
		this.EMailGorunenAd.Location = new System.Drawing.Point(224, 442);
		this.EMailGorunenAd.Name = "EMailGorunenAd";
		this.EMailGorunenAd.Size = new System.Drawing.Size(213, 20);
		this.EMailGorunenAd.TabIndex = 11;
		this.labelControl212.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl212.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl212.Location = new System.Drawing.Point(113, 445);
		this.labelControl212.Name = "labelControl212";
		this.labelControl212.Size = new System.Drawing.Size(105, 13);
		this.labelControl212.TabIndex = 202;
		this.labelControl212.Text = "Görünen ad :";
		this.EMailSmtpPassword.Location = new System.Drawing.Point(224, 494);
		this.EMailSmtpPassword.Name = "EMailSmtpPassword";
		this.EMailSmtpPassword.Properties.UseSystemPasswordChar = true;
		this.EMailSmtpPassword.Size = new System.Drawing.Size(213, 20);
		this.EMailSmtpPassword.TabIndex = 13;
		this.labelControl211.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl211.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl211.Location = new System.Drawing.Point(113, 497);
		this.labelControl211.Name = "labelControl211";
		this.labelControl211.Size = new System.Drawing.Size(105, 13);
		this.labelControl211.TabIndex = 200;
		this.labelControl211.Text = "Şifre :";
		this.EMailAdress.Location = new System.Drawing.Point(224, 468);
		this.EMailAdress.Name = "EMailAdress";
		this.EMailAdress.Size = new System.Drawing.Size(213, 20);
		this.EMailAdress.TabIndex = 12;
		this.labelControl210.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl210.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl210.Location = new System.Drawing.Point(113, 471);
		this.labelControl210.Name = "labelControl210";
		this.labelControl210.Size = new System.Drawing.Size(105, 13);
		this.labelControl210.TabIndex = 198;
		this.labelControl210.Text = "E-posta (hesap adı) :";
		this.labelControl209.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl209.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl209.Location = new System.Drawing.Point(113, 394);
		this.labelControl209.Name = "labelControl209";
		this.labelControl209.Size = new System.Drawing.Size(105, 13);
		this.labelControl209.TabIndex = 196;
		this.labelControl209.Text = "Smtp port :";
		this.EMailSmtpServer.Location = new System.Drawing.Point(224, 365);
		this.EMailSmtpServer.Name = "EMailSmtpServer";
		this.EMailSmtpServer.Size = new System.Drawing.Size(213, 20);
		this.EMailSmtpServer.TabIndex = 8;
		this.labelControl208.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl208.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl208.Location = new System.Drawing.Point(113, 368);
		this.labelControl208.Name = "labelControl208";
		this.labelControl208.Size = new System.Drawing.Size(105, 13);
		this.labelControl208.TabIndex = 194;
		this.labelControl208.Text = "Smtp sunucusu :";
		this.labelControl175.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl175.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl175.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl175.Location = new System.Drawing.Point(224, 23);
		this.labelControl175.Name = "labelControl175";
		this.labelControl175.Size = new System.Drawing.Size(175, 19);
		this.labelControl175.TabIndex = 222;
		this.labelControl175.Text = "Genel tanımlamalar";
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(224, 201);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(175, 19);
		this.labelControl1.TabIndex = 223;
		this.labelControl1.Text = "Evrak tanımlamaları";
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(224, 326);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(202, 19);
		this.labelControl2.TabIndex = 224;
		this.labelControl2.Text = "E-posta tanımlamaları";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(188, 223);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(181, 13);
		this.label8.TabIndex = 225;
		this.label8.Text = "Oluşturulacak evrakların özel alanları";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(123, 348);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(314, 13);
		this.label9.TabIndex = 226;
		this.label9.Text = "Gönderilecek e-postanın hangi hesaptan gönderileceğinin bilgileri";
		this.EMailSmtpPort.EditValue = new decimal(new int[4]);
		this.EMailSmtpPort.Location = new System.Drawing.Point(224, 391);
		this.EMailSmtpPort.Name = "EMailSmtpPort";
		this.EMailSmtpPort.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.EMailSmtpPort.Size = new System.Drawing.Size(67, 20);
		this.EMailSmtpPort.TabIndex = 9;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(297, 152);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(38, 13);
		this.label10.TabIndex = 227;
		this.label10.Text = "saniye";
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(297, 178);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(37, 13);
		this.label11.TabIndex = 228;
		this.label11.Text = "dakika";
		this.simpleButton1.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.simpleButton1.Appearance.Options.UseFont = true;
		this.simpleButton1.Location = new System.Drawing.Point(239, 547);
		this.simpleButton1.Name = "simpleButton1";
		this.simpleButton1.Size = new System.Drawing.Size(213, 24);
		this.simpleButton1.TabIndex = 229;
		this.simpleButton1.Text = "Test e-postası gönder";
		this.simpleButton1.Click += new System.EventHandler(simpleButton1_Click);
		this.te_test_mail_adresi.Location = new System.Drawing.Point(224, 520);
		this.te_test_mail_adresi.Name = "te_test_mail_adresi";
		this.te_test_mail_adresi.Size = new System.Drawing.Size(213, 20);
		this.te_test_mail_adresi.TabIndex = 230;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(3, 523);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(215, 13);
		this.labelControl3.TabIndex = 231;
		this.labelControl3.Text = "Test mesajının gönderileceği e-post adresi :";
		this.AktarilanKayitlariOkunduOlarakIsaretle.Location = new System.Drawing.Point(224, 100);
		this.AktarilanKayitlariOkunduOlarakIsaretle.Name = "AktarilanKayitlariOkunduOlarakIsaretle";
		this.AktarilanKayitlariOkunduOlarakIsaretle.Properties.Caption = "Aktarılan kayıtları okundu olarak işaretle";
		this.AktarilanKayitlariOkunduOlarakIsaretle.Size = new System.Drawing.Size(222, 19);
		this.AktarilanKayitlariOkunduOlarakIsaretle.TabIndex = 232;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(221, 122);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(261, 13);
		this.label12.TabIndex = 233;
		this.label12.Text = "Test aşaması sonlandıktan sonra işaretlenmesi gerekli";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(489, 624);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.AktarilanKayitlariOkunduOlarakIsaretle);
		base.Controls.Add(this.te_test_mail_adresi);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.simpleButton1);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.EMailSmtpPort);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.labelControl175);
		base.Controls.Add(this.EMailSmtpUseSSL);
		base.Controls.Add(this.EMailGorunenAd);
		base.Controls.Add(this.labelControl212);
		base.Controls.Add(this.EMailSmtpPassword);
		base.Controls.Add(this.labelControl211);
		base.Controls.Add(this.EMailAdress);
		base.Controls.Add(this.labelControl210);
		base.Controls.Add(this.labelControl209);
		base.Controls.Add(this.EMailSmtpServer);
		base.Controls.Add(this.labelControl208);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.SpecialAlan3);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.SpecialAlan2);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.SpecialAlan1);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.SenkronizasyonSuresi);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.Sifre);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Controls.Add(this.KullaniciAdi);
		base.Controls.Add(this.ZamanAsimi);
		base.Name = "ComarchEdiGenelParametrelerForm";
		this.Text = "Genel Parametreler";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		((System.ComponentModel.ISupportInitialize)this.ZamanAsimi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.KullaniciAdi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Sifre.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SenkronizasyonSuresi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SpecialAlan1.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SpecialAlan2.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SpecialAlan3.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpUseSSL.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EMailGorunenAd.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpPassword.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EMailAdress.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpServer.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EMailSmtpPort.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_test_mail_adresi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AktarilanKayitlariOkunduOlarakIsaretle.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
