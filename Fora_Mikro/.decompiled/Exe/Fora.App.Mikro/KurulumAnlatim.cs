using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraReports.ReportGeneration;
using Fora.App.Win.Mikro.Properties;

namespace Fora.App.Mikro;

public class KurulumAnlatim : Form
{
	private IContainer components;

	private PictureBox pictureBox1;

	private Label label1;

	private Label label2;

	private PictureBox pictureBox2;

	private Label label3;

	private PictureBox pictureBox3;

	private TextBox textBox1;

	private Label label4;

	private Label label5;

	private TextBox textBox2;

	private Label label6;

	private PictureBox pictureBox4;

	private Label label7;

	private PictureBox pictureBox5;

	private Label label8;

	private PictureBox pictureBox6;

	private Label label9;

	private PictureBox pictureBox7;

	private Label label10;

	private Label label11;

	private Label label12;

	private ReportGenerator reportGenerator1;

	public KurulumAnlatim()
	{
		InitializeComponent();
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
		this.label1 = new System.Windows.Forms.Label();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.label2 = new System.Windows.Forms.Label();
		this.pictureBox2 = new System.Windows.Forms.PictureBox();
		this.label3 = new System.Windows.Forms.Label();
		this.pictureBox3 = new System.Windows.Forms.PictureBox();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.pictureBox4 = new System.Windows.Forms.PictureBox();
		this.label7 = new System.Windows.Forms.Label();
		this.pictureBox5 = new System.Windows.Forms.PictureBox();
		this.label8 = new System.Windows.Forms.Label();
		this.pictureBox6 = new System.Windows.Forms.PictureBox();
		this.label9 = new System.Windows.Forms.Label();
		this.pictureBox7 = new System.Windows.Forms.PictureBox();
		this.label10 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.reportGenerator1 = new DevExpress.XtraReports.ReportGeneration.ReportGenerator(this.components);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox7).BeginInit();
		base.SuspendLayout();
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label1.Location = new System.Drawing.Point(21, 82);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(572, 37);
		this.label1.TabIndex = 1;
		this.label1.Text = "1 - Mikro programına SRV ile giriş yapınız. Daha sonra menü üzerinde farenin sağ tuşuna basarak 'Menü ekle -> Bir üste yeni menü olarak' seçeneğini seçiniz.";
		this.pictureBox1.Image = Fora.App.Win.Mikro.Properties.Resources.kurulum_1;
		this.pictureBox1.Location = new System.Drawing.Point(21, 122);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(572, 581);
		this.pictureBox1.TabIndex = 0;
		this.pictureBox1.TabStop = false;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label2.Location = new System.Drawing.Point(21, 732);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(572, 37);
		this.label2.TabIndex = 2;
		this.label2.Text = "2 - 'Yeni menü' üzerinde tekrar farenin sağ tuşuna basarak 'Harici program bağla' seçeneğini seçiniz.";
		this.pictureBox2.Image = Fora.App.Win.Mikro.Properties.Resources.kurulum_2;
		this.pictureBox2.Location = new System.Drawing.Point(21, 772);
		this.pictureBox2.Name = "pictureBox2";
		this.pictureBox2.Size = new System.Drawing.Size(557, 582);
		this.pictureBox2.TabIndex = 3;
		this.pictureBox2.TabStop = false;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label3.Location = new System.Drawing.Point(19, 1380);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(572, 37);
		this.label3.TabIndex = 4;
		this.label3.Text = "3 - 'Dosya seç' butonuna basarak 'Fora Mikro.exe' dosyasını seçiniz. Parametreler bölümüne de aşağıdaki bilgileri girip devam butonuna basınız.";
		this.pictureBox3.Image = Fora.App.Win.Mikro.Properties.Resources.kurulum_3;
		this.pictureBox3.Location = new System.Drawing.Point(22, 1505);
		this.pictureBox3.Name = "pictureBox3";
		this.pictureBox3.Size = new System.Drawing.Size(720, 333);
		this.pictureBox3.TabIndex = 5;
		this.pictureBox3.TabStop = false;
		this.textBox1.Location = new System.Drawing.Point(114, 1416);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(222, 20);
		this.textBox1.TabIndex = 6;
		this.textBox1.TabStop = false;
		this.textBox1.Text = "14 %AVT% %AKK% 00000000";
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label4.Location = new System.Drawing.Point(19, 1417);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(89, 19);
		this.label4.TabIndex = 7;
		this.label4.Text = "Mikro V14 : ";
		this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label5.Location = new System.Drawing.Point(19, 1443);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(89, 19);
		this.label5.TabIndex = 9;
		this.label5.Text = "Mikro V15 : ";
		this.textBox2.Location = new System.Drawing.Point(114, 1442);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(222, 20);
		this.textBox2.TabIndex = 8;
		this.textBox2.TabStop = false;
		this.textBox2.Text = "15 %AVT% %AKK% 00000000";
		this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label6.Location = new System.Drawing.Point(19, 1866);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(572, 37);
		this.label6.TabIndex = 10;
		this.label6.Text = "4 - 'Fora Mikro' üzerinde tekrar farenin sağ tuşuna basarak 'Başlığını değiştir' seçeneğini seçiniz.";
		this.pictureBox4.Image = Fora.App.Win.Mikro.Properties.Resources.kurulum_4;
		this.pictureBox4.Location = new System.Drawing.Point(21, 1906);
		this.pictureBox4.Name = "pictureBox4";
		this.pictureBox4.Size = new System.Drawing.Size(363, 579);
		this.pictureBox4.TabIndex = 11;
		this.pictureBox4.TabStop = false;
		this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label7.Location = new System.Drawing.Point(18, 2518);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(572, 22);
		this.label7.TabIndex = 12;
		this.label7.Text = "5 - Açılan pencerede isimleri 'Fora Mikro' şeklinde değiştiriniz.";
		this.pictureBox5.Image = Fora.App.Win.Mikro.Properties.Resources.kurulum_5;
		this.pictureBox5.Location = new System.Drawing.Point(21, 2543);
		this.pictureBox5.Name = "pictureBox5";
		this.pictureBox5.Size = new System.Drawing.Size(569, 360);
		this.pictureBox5.TabIndex = 13;
		this.pictureBox5.TabStop = false;
		this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label8.Location = new System.Drawing.Point(18, 2930);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(572, 19);
		this.label8.TabIndex = 14;
		this.label8.Text = "6 - Menü üzerinde sağ tuşa basarak 'Sakla' seçeneğini seçiniz.";
		this.pictureBox6.Image = Fora.App.Win.Mikro.Properties.Resources.kurulum_6;
		this.pictureBox6.Location = new System.Drawing.Point(21, 2952);
		this.pictureBox6.Name = "pictureBox6";
		this.pictureBox6.Size = new System.Drawing.Size(389, 584);
		this.pictureBox6.TabIndex = 15;
		this.pictureBox6.TabStop = false;
		this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label9.Location = new System.Drawing.Point(18, 3564);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(572, 37);
		this.label9.TabIndex = 16;
		this.label9.Text = "7 - Açılan pencerede menünün hangi kullanıcılar için kayıt edilmesini istiyorsanız seçiniz ve enter tuşuna basınız.";
		this.pictureBox7.Image = Fora.App.Win.Mikro.Properties.Resources.kurulum_7;
		this.pictureBox7.Location = new System.Drawing.Point(21, 3604);
		this.pictureBox7.Name = "pictureBox7";
		this.pictureBox7.Size = new System.Drawing.Size(351, 316);
		this.pictureBox7.TabIndex = 17;
		this.pictureBox7.TabStop = false;
		this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label10.Location = new System.Drawing.Point(18, 3957);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(254, 27);
		this.label10.TabIndex = 18;
		this.label10.Text = "Kurulum işlemi tamamlanmıştır.";
		this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 15f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label11.Location = new System.Drawing.Point(21, 33);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(572, 34);
		this.label11.TabIndex = 19;
		this.label11.Text = "FORA MİKRO KURULUMU";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label12.Location = new System.Drawing.Point(111, 1465);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(631, 37);
		this.label12.TabIndex = 20;
		this.label12.Text = "Parametrelerin sonunda yer alan 8 sıfır yerine direk açılmasını istediğiniz menünün kodunu yazabilirsiniz.";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoScroll = true;
		base.AutoScrollMargin = new System.Drawing.Size(0, 75);
		base.ClientSize = new System.Drawing.Size(763, 713);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.pictureBox7);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.pictureBox6);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.pictureBox5);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.pictureBox4);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.textBox2);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.pictureBox3);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.pictureBox2);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.pictureBox1);
		base.Name = "KurulumAnlatim";
		this.Text = "Fora Mikro - Kurulum";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox7).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
