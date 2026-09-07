using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.Data.Sql;

namespace Fora.App.Win.Mikro.CariHesaplar;

public class YeniCariEkle : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Cari _cari;

	private IContainer components;

	private SimpleButton cari_kaydet;

	private XtraTabPage xtraTabPage4;

	private TextEdit Ulke;

	private LabelControl labelControl14;

	private TextEdit Il;

	private LabelControl labelControl13;

	private TextEdit PostaKodu;

	private LabelControl labelControl12;

	private TextEdit Ilce;

	private LabelControl labelControl11;

	private TextEdit Adres2;

	private LabelControl labelControl10;

	private TextEdit Adres1;

	private LabelControl labelControl9;

	private XtraTabPage xtraTabPage3;

	private TextEdit Unvan2;

	private LabelControl labelControl2;

	private TextEdit Kodu;

	private LabelControl labelControl1;

	private TextEdit BankaHesapNo;

	private LabelControl labelControl4;

	private TextEdit Unvan1;

	private LabelControl labelControl3;

	private XtraTabControl xtraTabControl2;

	private TextEdit YetkiliCepTel;

	private LabelControl labelControl22;

	private TextEdit EPostaAdresi;

	private LabelControl labelControl8;

	private TextEdit VergiTcKimlikNo;

	private LabelControl labelControl7;

	private TextEdit VergiDairesi;

	private LabelControl labelControl6;

	private System.Windows.Forms.ComboBox Kodu_Prefix;

	private TextEdit adr_telno1;

	private LabelControl labelControl5;

	private TextEdit MuhasebeKodu;

	private LabelControl labelControl15;

	public YeniCariEkle(MikroUygulamaBilgileri mikrouygulamabilgileri, Cari cari)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_cari = cari;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void ForaAndroidKullaniciParametreleri_Load(object sender, EventArgs e)
	{
		Kodu_Prefix.Items.Clear();
		string[] array = _mikrouygulamabilgileri.GenelParametreler._GetParametre("CariYeniCariHesapOnEkleri")._GetString.Split(',');
		DataTable dataTable = new DataTable
		{
			Columns = 
			{
				{
					"ID",
					typeof(string)
				},
				{
					"Isim",
					typeof(string)
				}
			}
		};
		string[] array2 = array;
		foreach (string text in array2)
		{
			dataTable.Rows.Add(text, text);
		}
		Kodu_Prefix.DataSource = dataTable;
		Kodu_Prefix.ValueMember = "ID";
		Kodu_Prefix.DisplayMember = "Isim";
		if (Kodu_Prefix.Items.Count != 0)
		{
			Kodu_Prefix.SelectedIndex = 0;
		}
		Kodu.Text = "0";
		EkranBilgiGuncelle();
	}

	private void EkranBilgiGuncelle()
	{
		Unvan1.Text = _cari.cari_unvan1;
		Unvan2.Text = _cari.cari_unvan2;
		VergiDairesi.Text = _cari.cari_vdaire_adi;
		VergiTcKimlikNo.Text = _cari.cari_vdaire_no;
		EPostaAdresi.Text = _cari.cari_Email;
		YetkiliCepTel.Text = _cari.cari_CepTel;
		BankaHesapNo.Text = _cari.cari_banka_hesapno1;
		MuhasebeKodu.Text = _cari.cari_muh_kod;
		if (_cari.CariAdresleri.Count != 0)
		{
			Adres1.Text = _cari.CariAdresleri[0].adr_cadde;
			Adres2.Text = _cari.CariAdresleri[0].adr_sokak;
			PostaKodu.Text = _cari.CariAdresleri[0].adr_posta_kodu;
			Ilce.Text = _cari.CariAdresleri[0].adr_ilce;
			Il.Text = _cari.CariAdresleri[0].adr_il;
			Ulke.Text = _cari.CariAdresleri[0].adr_ulke;
			adr_telno1.Text = _cari.CariAdresleri[0].adr_tel_no1;
		}
	}

	private void cari_kaydet_Click(object sender, EventArgs e)
	{
		_cari.cari_fatura_adres_no = 1;
		_cari.cari_sevk_adres_no = 1;
		_cari.cari_doviz_cinsi1 = 255;
		_cari.cari_doviz_cinsi2 = 255;
		_cari.cari_KurHesapSekli = 1;
		_cari.cari_satis_fk = 1;
		_cari.cari_unvan1 = Unvan1.Text;
		_cari.cari_unvan2 = Unvan2.Text;
		_cari.cari_muh_kod = MuhasebeKodu.Text;
		_cari.cari_vdaire_adi = VergiDairesi.Text;
		_cari.cari_vdaire_no = VergiTcKimlikNo.Text;
		_cari.cari_Email = EPostaAdresi.Text;
		_cari.cari_CepTel = YetkiliCepTel.Text;
		_cari.cari_banka_hesapno1 = BankaHesapNo.Text;
		_cari.CariAdresleri = new List<CariAdres>();
		CariAdres cariAdres = new CariAdres();
		cariAdres.adr_adres_no = 1;
		cariAdres.adr_cadde = Adres1.Text;
		cariAdres.adr_sokak = Adres2.Text;
		cariAdres.adr_posta_kodu = PostaKodu.Text;
		cariAdres.adr_ilce = Ilce.Text;
		cariAdres.adr_il = Il.Text;
		cariAdres.adr_ulke = Ulke.Text;
		cariAdres.adr_tel_no1 = adr_telno1.Text;
		_cari.CariAdresleri.Add(cariAdres);
		string cari_kod = "";
		string text = "";
		if (Kodu_Prefix.SelectedValue != null)
		{
			text = Kodu_Prefix.SelectedValue.ToString();
		}
		if (Kodu.EditValue.ToString() != "0")
		{
			cari_kod = text + Kodu.EditValue;
		}
		else
		{
			SqlDB sqlDB = new SqlDB();
			try
			{
				sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					sqlCommand.CommandText = "SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WITH (NOLOCK) WHERE cari_kod like '" + text + "%' ORDER BY cari_kod DESC";
					object obj = sqlCommand.ExecuteScalar();
					if (obj != null)
					{
						string text2 = obj.ToString();
						if (text.Length != 0)
						{
							text2 = text2.Replace(text, "");
						}
						string text3 = "";
						text2.Substring(text2.Length - 1, 1);
						int result = 0;
						for (int i = 0; i < text2.Length; i++)
						{
							string s = text2.Substring(i, text2.Length - i);
							result = 0;
							if (int.TryParse(s, out result))
							{
								break;
							}
						}
						text3 = result.ToString();
						if (text3.Length != 0)
						{
							long result2 = 0L;
							long.TryParse(text3, out result2);
							result2++;
							string text4 = text2.Replace(text3, "");
							if ((result2 == 10 || result2 == 100 || result2 == 1000 || result2 == 10000 || result2 == 100000 || result2 == 1000000 || result2 == 10000000 || result2 == 100000000 || result2 == 1000000000 || result2 == 10000000000L || result2 == 100000000000L || result2 == 1000000000000L || result2 == 10000000000000L || result2 == 100000000000000L || result2 == 1000000000000000L || result2 == 10000000000000000L) && text4.Length > 0)
							{
								text4 = text4.Substring(0, text4.Length - 1);
							}
							cari_kod = text + text4 + result2;
						}
						else
						{
							cari_kod = text + "0";
						}
					}
					else
					{
						cari_kod = text + "0";
					}
				}
				sqlDB.ConnectionClose();
			}
			catch
			{
				MessageBox.Show("Yeni cari kodu belirlenemedi");
				return;
			}
		}
		_cari.cari_kod = cari_kod;
		foreach (CariAdres item in _cari.CariAdresleri)
		{
			item.adr_cari_kod = _cari.cari_kod;
		}
		SqlDB sqlDB2 = new SqlDB();
		sqlDB2.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		if (_cari.KaydetYeniCari(sqlDB2.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.mikrokullanici.User_no))
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
		else
		{
			MessageBox.Show("Cari hesap eklenemedi. Bilgileri kontrol edip tekrar deneyiniz.");
		}
	}

	private void Kodu_CustomDisplayText(object sender, CustomDisplayTextEventArgs e)
	{
		if (e.Value != null && e.Value.ToString() == "0")
		{
			e.DisplayText = "Otomatik";
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
		this.cari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.xtraTabPage4 = new DevExpress.XtraTab.XtraTabPage();
		this.adr_telno1 = new DevExpress.XtraEditors.TextEdit();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.Ulke = new DevExpress.XtraEditors.TextEdit();
		this.labelControl14 = new DevExpress.XtraEditors.LabelControl();
		this.Il = new DevExpress.XtraEditors.TextEdit();
		this.labelControl13 = new DevExpress.XtraEditors.LabelControl();
		this.PostaKodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
		this.Ilce = new DevExpress.XtraEditors.TextEdit();
		this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
		this.Adres2 = new DevExpress.XtraEditors.TextEdit();
		this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
		this.Adres1 = new DevExpress.XtraEditors.TextEdit();
		this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage3 = new DevExpress.XtraTab.XtraTabPage();
		this.YetkiliCepTel = new DevExpress.XtraEditors.TextEdit();
		this.labelControl22 = new DevExpress.XtraEditors.LabelControl();
		this.EPostaAdresi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
		this.VergiTcKimlikNo = new DevExpress.XtraEditors.TextEdit();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.VergiDairesi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.Kodu_Prefix = new System.Windows.Forms.ComboBox();
		this.Unvan2 = new DevExpress.XtraEditors.TextEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.Kodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.BankaHesapNo = new DevExpress.XtraEditors.TextEdit();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.Unvan1 = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
		this.MuhasebeKodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl15 = new DevExpress.XtraEditors.LabelControl();
		this.xtraTabPage4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.adr_telno1.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Ulke.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Il.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.PostaKodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Ilce.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Adres2.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Adres1.Properties).BeginInit();
		this.xtraTabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.YetkiliCepTel.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EPostaAdresi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.VergiTcKimlikNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.VergiDairesi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Unvan2.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Kodu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BankaHesapNo.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Unvan1.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).BeginInit();
		this.xtraTabControl2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.MuhasebeKodu.Properties).BeginInit();
		base.SuspendLayout();
		this.cari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.cari_kaydet.Appearance.Options.UseFont = true;
		this.cari_kaydet.Location = new System.Drawing.Point(421, 361);
		this.cari_kaydet.Name = "cari_kaydet";
		this.cari_kaydet.Size = new System.Drawing.Size(132, 23);
		this.cari_kaydet.TabIndex = 10;
		this.cari_kaydet.Text = "Cari kaydet";
		this.cari_kaydet.Click += new System.EventHandler(cari_kaydet_Click);
		this.xtraTabPage4.Controls.Add(this.adr_telno1);
		this.xtraTabPage4.Controls.Add(this.labelControl5);
		this.xtraTabPage4.Controls.Add(this.Ulke);
		this.xtraTabPage4.Controls.Add(this.labelControl14);
		this.xtraTabPage4.Controls.Add(this.Il);
		this.xtraTabPage4.Controls.Add(this.labelControl13);
		this.xtraTabPage4.Controls.Add(this.PostaKodu);
		this.xtraTabPage4.Controls.Add(this.labelControl12);
		this.xtraTabPage4.Controls.Add(this.Ilce);
		this.xtraTabPage4.Controls.Add(this.labelControl11);
		this.xtraTabPage4.Controls.Add(this.Adres2);
		this.xtraTabPage4.Controls.Add(this.labelControl10);
		this.xtraTabPage4.Controls.Add(this.Adres1);
		this.xtraTabPage4.Controls.Add(this.labelControl9);
		this.xtraTabPage4.Name = "xtraTabPage4";
		this.xtraTabPage4.Size = new System.Drawing.Size(535, 315);
		this.xtraTabPage4.Text = "Adres bilgileri";
		this.adr_telno1.Location = new System.Drawing.Point(154, 179);
		this.adr_telno1.Name = "adr_telno1";
		this.adr_telno1.Properties.MaxLength = 10;
		this.adr_telno1.Size = new System.Drawing.Size(208, 20);
		this.adr_telno1.TabIndex = 14;
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(12, 182);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(136, 13);
		this.labelControl5.TabIndex = 15;
		this.labelControl5.Text = "Telno 1 :";
		this.Ulke.Location = new System.Drawing.Point(154, 153);
		this.Ulke.Name = "Ulke";
		this.Ulke.Size = new System.Drawing.Size(208, 20);
		this.Ulke.TabIndex = 6;
		this.labelControl14.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl14.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl14.Location = new System.Drawing.Point(12, 156);
		this.labelControl14.Name = "labelControl14";
		this.labelControl14.Size = new System.Drawing.Size(136, 13);
		this.labelControl14.TabIndex = 13;
		this.labelControl14.Text = "Ülke :";
		this.Il.Location = new System.Drawing.Point(154, 127);
		this.Il.Name = "Il";
		this.Il.Size = new System.Drawing.Size(208, 20);
		this.Il.TabIndex = 5;
		this.labelControl13.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl13.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl13.Location = new System.Drawing.Point(31, 130);
		this.labelControl13.Name = "labelControl13";
		this.labelControl13.Size = new System.Drawing.Size(117, 13);
		this.labelControl13.TabIndex = 11;
		this.labelControl13.Text = "İl :";
		this.PostaKodu.Location = new System.Drawing.Point(154, 75);
		this.PostaKodu.Name = "PostaKodu";
		this.PostaKodu.Size = new System.Drawing.Size(208, 20);
		this.PostaKodu.TabIndex = 3;
		this.labelControl12.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl12.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl12.Location = new System.Drawing.Point(31, 78);
		this.labelControl12.Name = "labelControl12";
		this.labelControl12.Size = new System.Drawing.Size(117, 13);
		this.labelControl12.TabIndex = 9;
		this.labelControl12.Text = "Posta kodu :";
		this.Ilce.Location = new System.Drawing.Point(154, 101);
		this.Ilce.Name = "Ilce";
		this.Ilce.Size = new System.Drawing.Size(208, 20);
		this.Ilce.TabIndex = 4;
		this.labelControl11.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl11.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl11.Location = new System.Drawing.Point(31, 104);
		this.labelControl11.Name = "labelControl11";
		this.labelControl11.Size = new System.Drawing.Size(117, 13);
		this.labelControl11.TabIndex = 7;
		this.labelControl11.Text = "İlçe :";
		this.Adres2.Location = new System.Drawing.Point(154, 49);
		this.Adres2.Name = "Adres2";
		this.Adres2.Size = new System.Drawing.Size(208, 20);
		this.Adres2.TabIndex = 2;
		this.labelControl10.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl10.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl10.Location = new System.Drawing.Point(31, 52);
		this.labelControl10.Name = "labelControl10";
		this.labelControl10.Size = new System.Drawing.Size(117, 13);
		this.labelControl10.TabIndex = 5;
		this.labelControl10.Text = "Adres 2 :";
		this.Adres1.Location = new System.Drawing.Point(154, 23);
		this.Adres1.Name = "Adres1";
		this.Adres1.Size = new System.Drawing.Size(208, 20);
		this.Adres1.TabIndex = 1;
		this.labelControl9.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl9.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl9.Location = new System.Drawing.Point(31, 26);
		this.labelControl9.Name = "labelControl9";
		this.labelControl9.Size = new System.Drawing.Size(117, 13);
		this.labelControl9.TabIndex = 3;
		this.labelControl9.Text = "Adres 1 :";
		this.xtraTabPage3.Controls.Add(this.MuhasebeKodu);
		this.xtraTabPage3.Controls.Add(this.labelControl15);
		this.xtraTabPage3.Controls.Add(this.YetkiliCepTel);
		this.xtraTabPage3.Controls.Add(this.labelControl22);
		this.xtraTabPage3.Controls.Add(this.EPostaAdresi);
		this.xtraTabPage3.Controls.Add(this.labelControl8);
		this.xtraTabPage3.Controls.Add(this.VergiTcKimlikNo);
		this.xtraTabPage3.Controls.Add(this.labelControl7);
		this.xtraTabPage3.Controls.Add(this.VergiDairesi);
		this.xtraTabPage3.Controls.Add(this.labelControl6);
		this.xtraTabPage3.Controls.Add(this.Kodu_Prefix);
		this.xtraTabPage3.Controls.Add(this.Unvan2);
		this.xtraTabPage3.Controls.Add(this.labelControl2);
		this.xtraTabPage3.Controls.Add(this.Kodu);
		this.xtraTabPage3.Controls.Add(this.labelControl1);
		this.xtraTabPage3.Controls.Add(this.BankaHesapNo);
		this.xtraTabPage3.Controls.Add(this.labelControl4);
		this.xtraTabPage3.Controls.Add(this.Unvan1);
		this.xtraTabPage3.Controls.Add(this.labelControl3);
		this.xtraTabPage3.Name = "xtraTabPage3";
		this.xtraTabPage3.Size = new System.Drawing.Size(535, 315);
		this.xtraTabPage3.Text = "Genel bilgiler";
		this.YetkiliCepTel.Location = new System.Drawing.Point(175, 220);
		this.YetkiliCepTel.Name = "YetkiliCepTel";
		this.YetkiliCepTel.Size = new System.Drawing.Size(230, 20);
		this.YetkiliCepTel.TabIndex = 8;
		this.labelControl22.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl22.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl22.Location = new System.Drawing.Point(16, 223);
		this.labelControl22.Name = "labelControl22";
		this.labelControl22.Size = new System.Drawing.Size(153, 13);
		this.labelControl22.TabIndex = 101;
		this.labelControl22.Text = "Yetkili cep tel :";
		this.EPostaAdresi.Location = new System.Drawing.Point(175, 194);
		this.EPostaAdresi.Name = "EPostaAdresi";
		this.EPostaAdresi.Size = new System.Drawing.Size(230, 20);
		this.EPostaAdresi.TabIndex = 7;
		this.labelControl8.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl8.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl8.Location = new System.Drawing.Point(16, 197);
		this.labelControl8.Name = "labelControl8";
		this.labelControl8.Size = new System.Drawing.Size(153, 13);
		this.labelControl8.TabIndex = 99;
		this.labelControl8.Text = "E-posta adresi :";
		this.VergiTcKimlikNo.Location = new System.Drawing.Point(175, 168);
		this.VergiTcKimlikNo.Name = "VergiTcKimlikNo";
		this.VergiTcKimlikNo.Size = new System.Drawing.Size(118, 20);
		this.VergiTcKimlikNo.TabIndex = 6;
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(16, 171);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(153, 13);
		this.labelControl7.TabIndex = 97;
		this.labelControl7.Text = "Vergi daire no / Tc kimlik no :";
		this.VergiDairesi.Location = new System.Drawing.Point(175, 142);
		this.VergiDairesi.Name = "VergiDairesi";
		this.VergiDairesi.Size = new System.Drawing.Size(118, 20);
		this.VergiDairesi.TabIndex = 5;
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(52, 145);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(117, 13);
		this.labelControl6.TabIndex = 95;
		this.labelControl6.Text = "Vergi dairesi :";
		this.Kodu_Prefix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.Kodu_Prefix.FormattingEnabled = true;
		this.Kodu_Prefix.Location = new System.Drawing.Point(175, 13);
		this.Kodu_Prefix.Name = "Kodu_Prefix";
		this.Kodu_Prefix.Size = new System.Drawing.Size(118, 21);
		this.Kodu_Prefix.TabIndex = 1;
		this.Unvan2.Location = new System.Drawing.Point(175, 65);
		this.Unvan2.Name = "Unvan2";
		this.Unvan2.Size = new System.Drawing.Size(230, 20);
		this.Unvan2.TabIndex = 4;
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(52, 68);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(117, 13);
		this.labelControl2.TabIndex = 62;
		this.labelControl2.Text = "Ünvan 2 :";
		this.Kodu.Location = new System.Drawing.Point(299, 13);
		this.Kodu.Name = "Kodu";
		this.Kodu.Size = new System.Drawing.Size(106, 20);
		this.Kodu.TabIndex = 2;
		this.Kodu.CustomDisplayText += new DevExpress.XtraEditors.Controls.CustomDisplayTextEventHandler(Kodu_CustomDisplayText);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(52, 16);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 60;
		this.labelControl1.Text = "Kodu :";
		this.BankaHesapNo.Location = new System.Drawing.Point(175, 278);
		this.BankaHesapNo.Name = "BankaHesapNo";
		this.BankaHesapNo.Size = new System.Drawing.Size(118, 20);
		this.BankaHesapNo.TabIndex = 9;
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(52, 281);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(117, 13);
		this.labelControl4.TabIndex = 6;
		this.labelControl4.Text = "Banka hesap no :";
		this.Unvan1.Location = new System.Drawing.Point(175, 39);
		this.Unvan1.Name = "Unvan1";
		this.Unvan1.Size = new System.Drawing.Size(230, 20);
		this.Unvan1.TabIndex = 3;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(52, 42);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(117, 13);
		this.labelControl3.TabIndex = 4;
		this.labelControl3.Text = "Ünvan 1 :";
		this.xtraTabControl2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.xtraTabControl2.Location = new System.Drawing.Point(12, 12);
		this.xtraTabControl2.Name = "xtraTabControl2";
		this.xtraTabControl2.SelectedTabPage = this.xtraTabPage3;
		this.xtraTabControl2.Size = new System.Drawing.Size(541, 343);
		this.xtraTabControl2.TabIndex = 0;
		this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[2] { this.xtraTabPage3, this.xtraTabPage4 });
		this.MuhasebeKodu.Location = new System.Drawing.Point(175, 91);
		this.MuhasebeKodu.Name = "MuhasebeKodu";
		this.MuhasebeKodu.Size = new System.Drawing.Size(230, 20);
		this.MuhasebeKodu.TabIndex = 102;
		this.labelControl15.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl15.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl15.Location = new System.Drawing.Point(52, 94);
		this.labelControl15.Name = "labelControl15";
		this.labelControl15.Size = new System.Drawing.Size(117, 13);
		this.labelControl15.TabIndex = 103;
		this.labelControl15.Text = "Muhasebe kodu :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(567, 399);
		base.Controls.Add(this.xtraTabControl2);
		base.Controls.Add(this.cari_kaydet);
		base.Name = "YeniCariEkle";
		this.Text = "Yeni cari ekleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciParametreleri_Load);
		this.xtraTabPage4.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.adr_telno1.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Ulke.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Il.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.PostaKodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Ilce.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Adres2.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Adres1.Properties).EndInit();
		this.xtraTabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.YetkiliCepTel.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EPostaAdresi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.VergiTcKimlikNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.VergiDairesi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Unvan2.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Kodu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BankaHesapNo.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Unvan1.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.xtraTabControl2).EndInit();
		this.xtraTabControl2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.MuhasebeKodu.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
