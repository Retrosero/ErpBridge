using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ServiceTools;

namespace Fora.App.Mikro.Kurulum;

public class ForaMikroServisKurulumu : Form
{
	private string ServisIsmi = "ForaMikroService";

	private IContainer components;

	private Button b_servisi_kaldir;

	private Button b_servisi_kur;

	private Label label1;

	private Label label_mesaj;

	private Button b_durdur;

	private Button b_baslat;

	private Timer timer1;

	private TextBox tb_firma_id;

	private Label label2;

	private Label label3;

	private Button b_kurulum_kontrolu;

	public ForaMikroServisKurulumu()
	{
		InitializeComponent();
	}

	private void b_baslat_Click(object sender, EventArgs e)
	{
		try
		{
			ServiceTool.StartService(ServisIsmi);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Servis başlatılamadı. Programı yönetici olarak çalıştırdığınızdan emin olunuz. Hata : " + ex.ToString());
		}
	}

	private void b_durdur_Click(object sender, EventArgs e)
	{
		try
		{
			ServiceTool.StopService(ServisIsmi);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Servis durdurulamadı. Programı yönetici olarak çalıştırdığınızdan emin olunuz. Hata : " + ex.ToString());
		}
	}

	private void b_servisi_kur_Click(object sender, EventArgs e)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			ServiceTool.InstallAndStart(ServisIsmi, "Fora Mikro Service", directoryName + "\\ForaMikroService.exe");
		}
		catch (Exception ex)
		{
			MessageBox.Show("Servis kurulamadı. Programı yönetici olarak çalıştırdığınızdan emin olunuz. Hata : " + ex.ToString());
		}
	}

	private void b_servisi_kaldir_Click(object sender, EventArgs e)
	{
		try
		{
			if (MessageBox.Show("Servisi kaldırmak ve tekrar kurmak bilgisayarın tekrar başlatılmasını gerekmektedir. İşleme devam etmek istiyor musunuz?", "UYARI", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				ServiceTool.Uninstall(ServisIsmi);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Servis kaldırılamadı. Programı yönetici olarak çalıştırdığınızdan emin olunuz. Hata : " + ex.ToString());
		}
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		SetDisplay();
	}

	private void SetDisplay()
	{
		switch (ServiceTool.GetServiceStatus(ServisIsmi))
		{
		case ServiceState.NotFound:
			b_durdur.Enabled = false;
			b_baslat.Enabled = false;
			label_mesaj.Text = "Servis yüklü değil.";
			break;
		case ServiceState.Run:
			b_durdur.Enabled = true;
			b_baslat.Enabled = false;
			label_mesaj.Text = "Servis çalışıyor.";
			break;
		case ServiceState.Starting:
			b_durdur.Enabled = true;
			b_baslat.Enabled = false;
			label_mesaj.Text = "Servis çalışıyor.";
			break;
		case ServiceState.Stop:
			b_durdur.Enabled = false;
			b_baslat.Enabled = true;
			label_mesaj.Text = "Servis durduruldu.";
			break;
		case ServiceState.Stopping:
			b_durdur.Enabled = false;
			b_baslat.Enabled = true;
			label_mesaj.Text = "Servis durduruldu.";
			break;
		case ServiceState.Unknown:
			b_durdur.Enabled = false;
			b_baslat.Enabled = false;
			label_mesaj.Text = "Bilinmeyen durum.";
			break;
		}
	}

	private void LisansYoneticisi_Load(object sender, EventArgs e)
	{
		SetDisplay();
		timer1.Start();
	}

	private void b_kurulum_kontrolu_Click(object sender, EventArgs e)
	{
		Process.Start(new ProcessStartInfo("http://server.forayazilim.com/KurulumKontrol/?q=" + tb_firma_id.Text));
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
		this.components = new System.ComponentModel.Container();
		this.b_servisi_kaldir = new System.Windows.Forms.Button();
		this.b_servisi_kur = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label_mesaj = new System.Windows.Forms.Label();
		this.b_durdur = new System.Windows.Forms.Button();
		this.b_baslat = new System.Windows.Forms.Button();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.tb_firma_id = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.b_kurulum_kontrolu = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.b_servisi_kaldir.Location = new System.Drawing.Point(163, 99);
		this.b_servisi_kaldir.Name = "b_servisi_kaldir";
		this.b_servisi_kaldir.Size = new System.Drawing.Size(105, 23);
		this.b_servisi_kaldir.TabIndex = 11;
		this.b_servisi_kaldir.Text = "Servisi kaldır";
		this.b_servisi_kaldir.UseVisualStyleBackColor = true;
		this.b_servisi_kaldir.Click += new System.EventHandler(b_servisi_kaldir_Click);
		this.b_servisi_kur.Location = new System.Drawing.Point(52, 99);
		this.b_servisi_kur.Name = "b_servisi_kur";
		this.b_servisi_kur.Size = new System.Drawing.Size(105, 23);
		this.b_servisi_kur.TabIndex = 10;
		this.b_servisi_kur.Text = "Servisi kur";
		this.b_servisi_kur.UseVisualStyleBackColor = true;
		this.b_servisi_kur.Click += new System.EventHandler(b_servisi_kur_Click);
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(295, 18);
		this.label1.TabIndex = 9;
		this.label1.Text = "Servis durumu";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label_mesaj.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_mesaj.ForeColor = System.Drawing.Color.Red;
		this.label_mesaj.Location = new System.Drawing.Point(12, 36);
		this.label_mesaj.Name = "label_mesaj";
		this.label_mesaj.Size = new System.Drawing.Size(295, 18);
		this.label_mesaj.TabIndex = 8;
		this.label_mesaj.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.b_durdur.Location = new System.Drawing.Point(163, 57);
		this.b_durdur.Name = "b_durdur";
		this.b_durdur.Size = new System.Drawing.Size(105, 23);
		this.b_durdur.TabIndex = 7;
		this.b_durdur.Text = "Durdur";
		this.b_durdur.UseVisualStyleBackColor = true;
		this.b_durdur.Click += new System.EventHandler(b_durdur_Click);
		this.b_baslat.Location = new System.Drawing.Point(52, 57);
		this.b_baslat.Name = "b_baslat";
		this.b_baslat.Size = new System.Drawing.Size(105, 23);
		this.b_baslat.TabIndex = 6;
		this.b_baslat.Text = "Başlat";
		this.b_baslat.UseVisualStyleBackColor = true;
		this.b_baslat.Click += new System.EventHandler(b_baslat_Click);
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.tb_firma_id.Location = new System.Drawing.Point(112, 168);
		this.tb_firma_id.Name = "tb_firma_id";
		this.tb_firma_id.Size = new System.Drawing.Size(133, 20);
		this.tb_firma_id.TabIndex = 12;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label2.Location = new System.Drawing.Point(12, 147);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(295, 18);
		this.label2.TabIndex = 13;
		this.label2.Text = "Kurulum kontrolü";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(54, 171);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(52, 13);
		this.label3.TabIndex = 14;
		this.label3.Text = "FirmaID : ";
		this.b_kurulum_kontrolu.Location = new System.Drawing.Point(112, 194);
		this.b_kurulum_kontrolu.Name = "b_kurulum_kontrolu";
		this.b_kurulum_kontrolu.Size = new System.Drawing.Size(133, 23);
		this.b_kurulum_kontrolu.TabIndex = 15;
		this.b_kurulum_kontrolu.Text = "Kurulum kontrolü yap";
		this.b_kurulum_kontrolu.UseVisualStyleBackColor = true;
		this.b_kurulum_kontrolu.Click += new System.EventHandler(b_kurulum_kontrolu_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(322, 237);
		base.Controls.Add(this.b_kurulum_kontrolu);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.tb_firma_id);
		base.Controls.Add(this.b_servisi_kaldir);
		base.Controls.Add(this.b_servisi_kur);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.label_mesaj);
		base.Controls.Add(this.b_durdur);
		base.Controls.Add(this.b_baslat);
		base.Name = "ForaMikroServisKurulumu";
		this.Text = "Fora Mikro servis kurulumu";
		base.Load += new System.EventHandler(LisansYoneticisi_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
