using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Fora.Mikro;

namespace Fora.App.Mikro;

public class Hakkinda : Form
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private IContainer components;

	private Label label3;

	private Label label11;

	private Label label_lisans_sahibi;

	private Label label_moduller;

	private Label label2;

	public Hakkinda(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
	}

	private void Hakkinda_Load(object sender, EventArgs e)
	{
		Lisans lisans = Lisans.GetLisans(_mikrouygulamabilgileri.baglantibilgileri.SqlServer, "123");
		label_lisans_sahibi.Text = lisans.LisansSahibi;
		label_moduller.Text = lisans.LisansliModuller;
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
		this.label3 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label_lisans_sahibi = new System.Windows.Forms.Label();
		this.label_moduller = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label3.Location = new System.Drawing.Point(19, 54);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(564, 17);
		this.label3.TabIndex = 4;
		this.label3.Text = "Lisans sahibi";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label11.Location = new System.Drawing.Point(11, 9);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(572, 34);
		this.label11.TabIndex = 19;
		this.label11.Text = "LİSANS BİLGİLERİ";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label_lisans_sahibi.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_lisans_sahibi.Location = new System.Drawing.Point(16, 78);
		this.label_lisans_sahibi.Name = "label_lisans_sahibi";
		this.label_lisans_sahibi.Size = new System.Drawing.Size(564, 17);
		this.label_lisans_sahibi.TabIndex = 20;
		this.label_lisans_sahibi.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_moduller.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_moduller.Location = new System.Drawing.Point(13, 133);
		this.label_moduller.Name = "label_moduller";
		this.label_moduller.Size = new System.Drawing.Size(564, 38);
		this.label_moduller.TabIndex = 22;
		this.label_moduller.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label2.Location = new System.Drawing.Point(16, 114);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(564, 17);
		this.label2.TabIndex = 21;
		this.label2.Text = "Modüller";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoScroll = true;
		base.ClientSize = new System.Drawing.Size(592, 201);
		base.Controls.Add(this.label_moduller);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label_lisans_sahibi);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.label3);
		base.Name = "Hakkinda";
		this.Text = "Fora Mikro - Hakkında";
		base.Load += new System.EventHandler(Hakkinda_Load);
		base.ResumeLayout(false);
	}
}
