using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;

namespace Fora.App.Win.Mikro.Aktarimlar.ComarchEdi;

public class ComarchEdiYardim : XtraForm
{
	private IContainer components;

	private SimpleButton sb_ayarlari_kaydet;

	private LabelControl labelControl175;

	private Label label12;

	private Label label1;

	private LabelControl labelControl1;

	private Label label2;

	private LabelControl labelControl2;

	private Label label3;

	private LabelControl labelControl3;

	public ComarchEdiYardim()
	{
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Fora.App.Win.Mikro.Aktarimlar.ComarchEdi.ComarchEdiYardim));
		this.sb_ayarlari_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.labelControl175 = new DevExpress.XtraEditors.LabelControl();
		this.label12 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.label2 = new System.Windows.Forms.Label();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.label3 = new System.Windows.Forms.Label();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		base.SuspendLayout();
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(12, 617);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(557, 36);
		this.sb_ayarlari_kaydet.TabIndex = 14;
		this.sb_ayarlari_kaydet.Text = "KAPAT";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.labelControl175.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl175.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl175.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl175.Location = new System.Drawing.Point(12, 12);
		this.labelControl175.Name = "labelControl175";
		this.labelControl175.Size = new System.Drawing.Size(183, 19);
		this.labelControl175.TabIndex = 222;
		this.labelControl175.Text = "Açıklama";
		this.label12.Location = new System.Drawing.Point(12, 34);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(557, 147);
		this.label12.TabIndex = 233;
		this.label12.Text = resources.GetString("label12.Text");
		this.label1.Location = new System.Drawing.Point(12, 232);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(557, 103);
		this.label1.TabIndex = 235;
		this.label1.Text = resources.GetString("label1.Text");
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(12, 210);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(183, 19);
		this.labelControl1.TabIndex = 234;
		this.labelControl1.Text = "Cari eşleştirme";
		this.label2.Location = new System.Drawing.Point(12, 360);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(557, 106);
		this.label2.TabIndex = 237;
		this.label2.Text = resources.GetString("label2.Text");
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(12, 338);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(183, 19);
		this.labelControl2.TabIndex = 236;
		this.labelControl2.Text = "Stok eşleştirme";
		this.label3.Location = new System.Drawing.Point(15, 508);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(557, 106);
		this.label3.TabIndex = 239;
		this.label3.Text = resources.GetString("label3.Text");
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(15, 486);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(183, 19);
		this.labelControl3.TabIndex = 238;
		this.labelControl3.Text = "Sipariş evrağı";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(581, 667);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.labelControl175);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Name = "ComarchEdiYardim";
		this.Text = "Comarch Edi Aktarım Yardım";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		base.ResumeLayout(false);
	}
}
