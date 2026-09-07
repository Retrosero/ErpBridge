using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;

namespace Fora.App.Win.Mikro.GenelFormlar;

public class TextSor : XtraForm
{
	private IContainer components;

	private OpenFileDialog openFileDialog1;

	private SimpleButton sb_onay;

	private LabelControl l_baslik;

	public TextEdit te_cevap;

	public TextSor(string Baslik, string OnayButonuMetin)
	{
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		Text = Baslik;
		l_baslik.Text = Baslik;
		sb_onay.Text = OnayButonuMetin;
	}

	private void sb_aktarima_basla_Click(object sender, EventArgs e)
	{
		if (te_cevap.Text == "")
		{
			MessageBox.Show("Boş olamaz!", "HATA");
			return;
		}
		base.DialogResult = DialogResult.OK;
		Close();
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
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.sb_onay = new DevExpress.XtraEditors.SimpleButton();
		this.l_baslik = new DevExpress.XtraEditors.LabelControl();
		this.te_cevap = new DevExpress.XtraEditors.TextEdit();
		((System.ComponentModel.ISupportInitialize)this.te_cevap.Properties).BeginInit();
		base.SuspendLayout();
		this.openFileDialog1.FileName = "openFileDialog1";
		this.sb_onay.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.sb_onay.Appearance.Options.UseFont = true;
		this.sb_onay.Location = new System.Drawing.Point(12, 57);
		this.sb_onay.Name = "sb_onay";
		this.sb_onay.Size = new System.Drawing.Size(286, 23);
		this.sb_onay.TabIndex = 9;
		this.sb_onay.Text = "İŞLEM BAŞLIĞI";
		this.sb_onay.Click += new System.EventHandler(sb_aktarima_basla_Click);
		this.l_baslik.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.l_baslik.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.l_baslik.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.l_baslik.Location = new System.Drawing.Point(12, 12);
		this.l_baslik.Name = "l_baslik";
		this.l_baslik.Size = new System.Drawing.Size(286, 13);
		this.l_baslik.TabIndex = 20;
		this.l_baslik.Text = "SORU BAŞLIĞI";
		this.te_cevap.Location = new System.Drawing.Point(12, 31);
		this.te_cevap.Name = "te_cevap";
		this.te_cevap.Size = new System.Drawing.Size(286, 20);
		this.te_cevap.TabIndex = 21;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(315, 95);
		base.Controls.Add(this.te_cevap);
		base.Controls.Add(this.l_baslik);
		base.Controls.Add(this.sb_onay);
		base.Name = "TextSor";
		this.Text = "SORU BAŞLIĞI";
		((System.ComponentModel.ISupportInitialize)this.te_cevap.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
