using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fora.App.Mikro.Kurulum;

public class LisansYoneticisi : Form
{
	private IContainer components;

	private Label label1;

	public LisansYoneticisi()
	{
		InitializeComponent();
	}

	private void LisansYoneticisi_Load(object sender, EventArgs e)
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
		this.label1 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(25, 28);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(368, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Lisans yöneticisini kurmak için Fora Mikro Servis kurulumu yapılması yeterlidir.";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(420, 70);
		base.Controls.Add(this.label1);
		base.Name = "LisansYoneticisi";
		this.Text = "Lisans yöneticisi";
		base.Load += new System.EventHandler(LisansYoneticisi_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
