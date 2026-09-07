using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Fora.Mikro.Win.Form.DevEx;

public class Genel_Giris_Metin : XtraForm
{
	private string _tag;

	private string _baslik;

	private string _onaybutonismi;

	private IContainer components;

	private Label label_baslik;

	private TextEdit te_deger;

	private SimpleButton sb_onay;

	private SimpleButton simpleButton1;

	public string _GetStringValue => te_deger.Text;

	public Genel_Giris_Metin(string tag, string baslik, string onaybutonismi, string defaultdeger)
	{
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		_tag = tag;
		_baslik = baslik;
		_onaybutonismi = onaybutonismi;
		sb_onay.Text = onaybutonismi;
		te_deger.Text = defaultdeger;
		Text = baslik;
		label_baslik.Text = baslik;
	}

	private void F10_Base_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Escape)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
	}

	private void te_ara_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void Genel_Giris_Metin_Load(object sender, EventArgs e)
	{
	}

	private void sb_onay_Click(object sender, EventArgs e)
	{
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
		this.label_baslik = new System.Windows.Forms.Label();
		this.te_deger = new DevExpress.XtraEditors.TextEdit();
		this.sb_onay = new DevExpress.XtraEditors.SimpleButton();
		this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
		((System.ComponentModel.ISupportInitialize)this.te_deger.Properties).BeginInit();
		base.SuspendLayout();
		this.label_baslik.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.label_baslik.Location = new System.Drawing.Point(71, 25);
		this.label_baslik.Name = "label_baslik";
		this.label_baslik.Size = new System.Drawing.Size(232, 22);
		this.label_baslik.TabIndex = 2;
		this.label_baslik.Text = "Baslik";
		this.label_baslik.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_deger.Location = new System.Drawing.Point(71, 50);
		this.te_deger.Name = "te_deger";
		this.te_deger.Size = new System.Drawing.Size(232, 20);
		this.te_deger.TabIndex = 1;
		this.te_deger.KeyDown += new System.Windows.Forms.KeyEventHandler(te_ara_KeyDown);
		this.sb_onay.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_onay.Appearance.Options.UseFont = true;
		this.sb_onay.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_onay.Location = new System.Drawing.Point(71, 76);
		this.sb_onay.Name = "sb_onay";
		this.sb_onay.Size = new System.Drawing.Size(232, 31);
		this.sb_onay.TabIndex = 3;
		this.sb_onay.Text = "Onay Buton ismi";
		this.sb_onay.Click += new System.EventHandler(sb_onay_Click);
		this.simpleButton1.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.simpleButton1.Appearance.Options.UseFont = true;
		this.simpleButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.simpleButton1.Location = new System.Drawing.Point(71, 126);
		this.simpleButton1.Name = "simpleButton1";
		this.simpleButton1.Size = new System.Drawing.Size(232, 31);
		this.simpleButton1.TabIndex = 4;
		this.simpleButton1.Text = "Vazgeç";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(364, 169);
		base.Controls.Add(this.simpleButton1);
		base.Controls.Add(this.sb_onay);
		base.Controls.Add(this.te_deger);
		base.Controls.Add(this.label_baslik);
		base.KeyPreview = true;
		base.Name = "Genel_Giris_Metin";
		this.Text = "BAŞLIK";
		base.Load += new System.EventHandler(Genel_Giris_Metin_Load);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(F10_Base_KeyDown);
		((System.ComponentModel.ISupportInitialize)this.te_deger.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
