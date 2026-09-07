using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;

public class Aktarim_Banka_Parametreler : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Parametreler _genelparametreler;

	private string _bankakodu;

	private DosyaTipi _dosyatipi;

	private IContainer components;

	private SimpleButton sb_ayarlari_kaydet;

	private TextEdit DosyaYolu;

	private Label label1;

	private Label label2;

	private TextEdit ArsivDosyaYolu;

	private Label label3;

	private SimpleButton sb_genel_parametreler_duzenle;

	private CheckedListBoxControl KriterListesi;

	private Label label4;

	private SimpleButton sb_kriterleri_duzenle;

	private System.Windows.Forms.ComboBox GenelParametrelerSablonAdi;

	private System.Windows.Forms.ComboBox AktarimParametreleriSablonAdi;

	private SimpleButton sb_aktarim_parametrelerini_duzenle;

	private Label label_aktarim;

	private CheckBox ButunVerileriBuyukKarakterYap;

	public Aktarim_Banka_Parametreler(MikroUygulamaBilgileri mikrouygulamabilgileri, Parametreler parametreler, string bankakodu, DosyaTipi dosyatipi)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_genelparametreler = parametreler;
		_bankakodu = bankakodu;
		_dosyatipi = dosyatipi;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		GenelParametreSablonlariOlustur();
		AktarimParametreSablonlariOlustur();
		AktarimKriterSablonlariOlustur();
		if (_dosyatipi == DosyaTipi.Mt940)
		{
			label_aktarim.Visible = false;
			AktarimParametreleriSablonAdi.Visible = false;
			sb_aktarim_parametrelerini_duzenle.Visible = false;
		}
		else
		{
			label_aktarim.Visible = true;
			AktarimParametreleriSablonAdi.Visible = true;
			sb_aktarim_parametrelerini_duzenle.Visible = true;
		}
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
		EkranGuncelle();
	}

	private void EkranGuncelle()
	{
		DosyaYolu.Text = _genelparametreler._GetParametre("DosyaYolu")._GetString;
		ArsivDosyaYolu.Text = _genelparametreler._GetParametre("ArsivDosyaYolu")._GetString;
		GenelParametrelerSablonAdi.SelectedValue = _genelparametreler._GetParametre("GenelParametrelerSablonAdi")._GetString;
		AktarimParametreleriSablonAdi.SelectedValue = _genelparametreler._GetParametre("AktarimParametreleriSablonAdi")._GetString;
		ButunVerileriBuyukKarakterYap.Checked = _genelparametreler._GetParametre("ButunVerileriBuyukKarakterYap")._GetBoolean;
		string[] array = _genelparametreler._GetParametre("KriterListesi")._GetString.Split(',');
		int num = 0;
		foreach (object item in KriterListesi.Items)
		{
			string text = item.ToString();
			bool flag = false;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text == text2)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				KriterListesi.Items[num].CheckState = CheckState.Checked;
			}
			else
			{
				KriterListesi.Items[num].CheckState = CheckState.Unchecked;
			}
			num++;
		}
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		_genelparametreler._GetParametre("DosyaYolu")._SetString = DosyaYolu.Text;
		_genelparametreler._GetParametre("ArsivDosyaYolu")._SetString = ArsivDosyaYolu.Text;
		_genelparametreler._GetParametre("GenelParametrelerSablonAdi")._SetString = (string)GenelParametrelerSablonAdi.SelectedValue;
		_genelparametreler._GetParametre("AktarimParametreleriSablonAdi")._SetString = (string)AktarimParametreleriSablonAdi.SelectedValue;
		_genelparametreler._GetParametre("ButunVerileriBuyukKarakterYap")._SetBoolean = ButunVerileriBuyukKarakterYap.Checked;
		string text = "";
		bool flag = true;
		foreach (object item in (IEnumerable)KriterListesi.CheckedItems)
		{
			if (!flag)
			{
				text += ",";
			}
			string text2 = item.ToString();
			text += text2;
			flag = false;
		}
		_genelparametreler._GetParametre("KriterListesi")._SetString = text;
		ParametreData.ParametreYaz(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _genelparametreler);
	}

	private void sb_genel_parametreler_duzenle_Click(object sender, EventArgs e)
	{
		new Aktarim_Banka_Genel_Parametre_Duzenleme(_mikrouygulamabilgileri).ShowDialog();
		GenelParametreSablonlariOlustur();
	}

	private void sb_aktarim_parametrelerini_duzenle_Click(object sender, EventArgs e)
	{
		new Aktarim_Banka_Aktarim_Parametre_Duzenleme(_mikrouygulamabilgileri).ShowDialog();
		AktarimParametreSablonlariOlustur();
	}

	private void GenelParametreSablonlariOlustur()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(string));
		dataTable.Columns.Add("Isim", typeof(string));
		foreach (string item in AktarimBankaGenelParametre.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			dataTable.Rows.Add(item, item);
		}
		GenelParametrelerSablonAdi.DataSource = dataTable;
		GenelParametrelerSablonAdi.ValueMember = "ID";
		GenelParametrelerSablonAdi.DisplayMember = "Isim";
		DataTable dataTable2 = new DataTable();
		dataTable2.Columns.Add("ID", typeof(string));
		dataTable2.Columns.Add("Isim", typeof(string));
		foreach (string item2 in AktarimBankaAktarimParametre.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			dataTable2.Rows.Add(item2, item2);
		}
		AktarimParametreleriSablonAdi.DataSource = dataTable2;
		AktarimParametreleriSablonAdi.ValueMember = "ID";
		AktarimParametreleriSablonAdi.DisplayMember = "Isim";
	}

	private void AktarimParametreSablonlariOlustur()
	{
		DataTable dataTable = new DataTable();
		dataTable.Columns.Add("ID", typeof(string));
		dataTable.Columns.Add("Isim", typeof(string));
		foreach (string item in AktarimBankaGenelParametre.GetParametreAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			dataTable.Rows.Add(item, item);
		}
		GenelParametrelerSablonAdi.DataSource = dataTable;
		GenelParametrelerSablonAdi.ValueMember = "ID";
		GenelParametrelerSablonAdi.DisplayMember = "Isim";
	}

	private void AktarimKriterSablonlariOlustur()
	{
		KriterListesi.Items.Clear();
		foreach (string item in AktarimBankaKriterParametre.GetKriterAdlari(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName))
		{
			KriterListesi.Items.Add(item);
		}
	}

	private void sb_kriterleri_duzenle_Click(object sender, EventArgs e)
	{
		new Aktarim_Banka_Aktarim_Kriter_Duzenleme(_mikrouygulamabilgileri).ShowDialog();
		AktarimKriterSablonlariOlustur();
		EkranGuncelle();
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
		this.DosyaYolu = new DevExpress.XtraEditors.TextEdit();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.ArsivDosyaYolu = new DevExpress.XtraEditors.TextEdit();
		this.label3 = new System.Windows.Forms.Label();
		this.sb_genel_parametreler_duzenle = new DevExpress.XtraEditors.SimpleButton();
		this.KriterListesi = new DevExpress.XtraEditors.CheckedListBoxControl();
		this.label4 = new System.Windows.Forms.Label();
		this.sb_kriterleri_duzenle = new DevExpress.XtraEditors.SimpleButton();
		this.GenelParametrelerSablonAdi = new System.Windows.Forms.ComboBox();
		this.AktarimParametreleriSablonAdi = new System.Windows.Forms.ComboBox();
		this.sb_aktarim_parametrelerini_duzenle = new DevExpress.XtraEditors.SimpleButton();
		this.label_aktarim = new System.Windows.Forms.Label();
		this.ButunVerileriBuyukKarakterYap = new System.Windows.Forms.CheckBox();
		((System.ComponentModel.ISupportInitialize)this.DosyaYolu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ArsivDosyaYolu.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.KriterListesi).BeginInit();
		base.SuspendLayout();
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(150, 387);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(214, 24);
		this.sb_ayarlari_kaydet.TabIndex = 9;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.DosyaYolu.Location = new System.Drawing.Point(150, 12);
		this.DosyaYolu.Name = "DosyaYolu";
		this.DosyaYolu.Size = new System.Drawing.Size(214, 20);
		this.DosyaYolu.TabIndex = 6;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(72, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(67, 13);
		this.label1.TabIndex = 8;
		this.label1.Text = "Dosya yolu :";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(46, 41);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(93, 13);
		this.label2.TabIndex = 10;
		this.label2.Text = "Arşiv dosya yolu :";
		this.ArsivDosyaYolu.Location = new System.Drawing.Point(150, 38);
		this.ArsivDosyaYolu.Name = "ArsivDosyaYolu";
		this.ArsivDosyaYolu.Size = new System.Drawing.Size(214, 20);
		this.ArsivDosyaYolu.TabIndex = 7;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(38, 71);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(106, 13);
		this.label3.TabIndex = 12;
		this.label3.Text = "Genel parametreler :";
		this.sb_genel_parametreler_duzenle.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_genel_parametreler_duzenle.Appearance.Options.UseFont = true;
		this.sb_genel_parametreler_duzenle.Location = new System.Drawing.Point(150, 94);
		this.sb_genel_parametreler_duzenle.Name = "sb_genel_parametreler_duzenle";
		this.sb_genel_parametreler_duzenle.Size = new System.Drawing.Size(214, 24);
		this.sb_genel_parametreler_duzenle.TabIndex = 14;
		this.sb_genel_parametreler_duzenle.Text = "Genel parametreleri düzenle";
		this.sb_genel_parametreler_duzenle.Click += new System.EventHandler(sb_genel_parametreler_duzenle_Click);
		this.KriterListesi.CheckOnClick = true;
		this.KriterListesi.Location = new System.Drawing.Point(150, 211);
		this.KriterListesi.Name = "KriterListesi";
		this.KriterListesi.Size = new System.Drawing.Size(214, 108);
		this.KriterListesi.TabIndex = 15;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(28, 211);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(116, 13);
		this.label4.TabIndex = 16;
		this.label4.Text = "Uygulanacak Kriterler :";
		this.sb_kriterleri_duzenle.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kriterleri_duzenle.Appearance.Options.UseFont = true;
		this.sb_kriterleri_duzenle.Location = new System.Drawing.Point(150, 325);
		this.sb_kriterleri_duzenle.Name = "sb_kriterleri_duzenle";
		this.sb_kriterleri_duzenle.Size = new System.Drawing.Size(214, 24);
		this.sb_kriterleri_duzenle.TabIndex = 17;
		this.sb_kriterleri_duzenle.Text = "Kriterleri düzenle";
		this.sb_kriterleri_duzenle.Click += new System.EventHandler(sb_kriterleri_duzenle_Click);
		this.GenelParametrelerSablonAdi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.GenelParametrelerSablonAdi.FormattingEnabled = true;
		this.GenelParametrelerSablonAdi.Location = new System.Drawing.Point(150, 68);
		this.GenelParametrelerSablonAdi.Name = "GenelParametrelerSablonAdi";
		this.GenelParametrelerSablonAdi.Size = new System.Drawing.Size(214, 21);
		this.GenelParametrelerSablonAdi.TabIndex = 90;
		this.AktarimParametreleriSablonAdi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.AktarimParametreleriSablonAdi.FormattingEnabled = true;
		this.AktarimParametreleriSablonAdi.Location = new System.Drawing.Point(150, 139);
		this.AktarimParametreleriSablonAdi.Name = "AktarimParametreleriSablonAdi";
		this.AktarimParametreleriSablonAdi.Size = new System.Drawing.Size(214, 21);
		this.AktarimParametreleriSablonAdi.TabIndex = 93;
		this.sb_aktarim_parametrelerini_duzenle.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_aktarim_parametrelerini_duzenle.Appearance.Options.UseFont = true;
		this.sb_aktarim_parametrelerini_duzenle.Location = new System.Drawing.Point(150, 165);
		this.sb_aktarim_parametrelerini_duzenle.Name = "sb_aktarim_parametrelerini_duzenle";
		this.sb_aktarim_parametrelerini_duzenle.Size = new System.Drawing.Size(214, 24);
		this.sb_aktarim_parametrelerini_duzenle.TabIndex = 92;
		this.sb_aktarim_parametrelerini_duzenle.Text = "Aktarım parametreleri düzenle";
		this.sb_aktarim_parametrelerini_duzenle.Click += new System.EventHandler(sb_aktarim_parametrelerini_duzenle_Click);
		this.label_aktarim.AutoSize = true;
		this.label_aktarim.Location = new System.Drawing.Point(22, 142);
		this.label_aktarim.Name = "label_aktarim";
		this.label_aktarim.Size = new System.Drawing.Size(117, 13);
		this.label_aktarim.TabIndex = 91;
		this.label_aktarim.Text = "Aktarım parametreleri :";
		this.ButunVerileriBuyukKarakterYap.AutoSize = true;
		this.ButunVerileriBuyukKarakterYap.Location = new System.Drawing.Point(150, 355);
		this.ButunVerileriBuyukKarakterYap.Name = "ButunVerileriBuyukKarakterYap";
		this.ButunVerileriBuyukKarakterYap.Size = new System.Drawing.Size(164, 17);
		this.ButunVerileriBuyukKarakterYap.TabIndex = 94;
		this.ButunVerileriBuyukKarakterYap.Text = "Bütün karakterleri büyük yap";
		this.ButunVerileriBuyukKarakterYap.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(395, 423);
		base.Controls.Add(this.ButunVerileriBuyukKarakterYap);
		base.Controls.Add(this.AktarimParametreleriSablonAdi);
		base.Controls.Add(this.sb_aktarim_parametrelerini_duzenle);
		base.Controls.Add(this.label_aktarim);
		base.Controls.Add(this.GenelParametrelerSablonAdi);
		base.Controls.Add(this.sb_kriterleri_duzenle);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.KriterListesi);
		base.Controls.Add(this.sb_genel_parametreler_duzenle);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.ArsivDosyaYolu);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.DosyaYolu);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Name = "Aktarim_Banka_Parametreler";
		this.Text = "Genel Banka Parametreleri";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		((System.ComponentModel.ISupportInitialize)this.DosyaYolu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ArsivDosyaYolu.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.KriterListesi).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
