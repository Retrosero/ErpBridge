using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.Touristica;

public class Touristica_GenelParametreler : XtraForm
{
	private SqlBaglantiBilgileri _baglantibilgileri;

	private string _DBName;

	private Parametreler _genelparametreler;

	private IContainer components;

	private SimpleButton sb_ayarlari_kaydet;

	private TextEdit te_specialalan1;

	private Label label1;

	private Label label2;

	private TextEdit te_specialalan2;

	private Label label3;

	private TextEdit te_specialalan3;

	private Label label4;

	private Label label5;

	private TextEdit te_autkey;

	private Label label6;

	private TextEdit te_branchcode;

	private Label label7;

	private TextEdit te_username;

	private Label label8;

	private TextEdit te_password;

	private Label label9;

	private TextEdit te_normalfaturaevrakseri;

	private Label label10;

	private TextEdit te_acentefaturaevrakseri;

	public Touristica_GenelParametreler(SqlBaglantiBilgileri baglantibilgileri, string DBName, Parametreler parametreler)
	{
		InitializeComponent();
		_baglantibilgileri = baglantibilgileri;
		_DBName = DBName;
		_genelparametreler = parametreler;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
		te_autkey.Text = _genelparametreler._GetParametre("GenelParametreler", "", "AutKey")._GetString;
		te_branchcode.Text = _genelparametreler._GetParametre("GenelParametreler", "", "BranchCode")._GetString;
		te_username.Text = _genelparametreler._GetParametre("GenelParametreler", "", "UserName")._GetString;
		te_password.Text = _genelparametreler._GetParametre("GenelParametreler", "", "Password")._GetString;
		te_normalfaturaevrakseri.Text = _genelparametreler._GetParametre("GenelParametreler", "", "NormalFaturaEvrakSeri")._GetString;
		te_acentefaturaevrakseri.Text = _genelparametreler._GetParametre("GenelParametreler", "", "AcenteFaturaEvrakSeri")._GetString;
		te_specialalan1.Text = _genelparametreler._GetParametre("GenelParametreler", "", "specialalan1")._GetString;
		te_specialalan2.Text = _genelparametreler._GetParametre("GenelParametreler", "", "specialalan2")._GetString;
		te_specialalan3.Text = _genelparametreler._GetParametre("GenelParametreler", "", "specialalan3")._GetString;
	}

	private void sb_ayarlari_kaydet_Click(object sender, EventArgs e)
	{
		_genelparametreler._GetParametre("GenelParametreler", "", "AutKey")._SetString = te_autkey.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "BranchCode")._SetString = te_branchcode.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "UserName")._SetString = te_username.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "Password")._SetString = te_password.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "NormalFaturaEvrakSeri")._SetString = te_normalfaturaevrakseri.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "AcenteFaturaEvrakSeri")._SetString = te_acentefaturaevrakseri.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "specialalan1")._SetString = te_specialalan1.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "specialalan2")._SetString = te_specialalan2.Text;
		_genelparametreler._GetParametre("GenelParametreler", "", "specialalan3")._SetString = te_specialalan3.Text;
		ParametreData.ParametreYaz(_baglantibilgileri, _DBName, _genelparametreler);
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
		this.te_specialalan1 = new DevExpress.XtraEditors.TextEdit();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.te_specialalan2 = new DevExpress.XtraEditors.TextEdit();
		this.label3 = new System.Windows.Forms.Label();
		this.te_specialalan3 = new DevExpress.XtraEditors.TextEdit();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.te_autkey = new DevExpress.XtraEditors.TextEdit();
		this.label6 = new System.Windows.Forms.Label();
		this.te_branchcode = new DevExpress.XtraEditors.TextEdit();
		this.label7 = new System.Windows.Forms.Label();
		this.te_username = new DevExpress.XtraEditors.TextEdit();
		this.label8 = new System.Windows.Forms.Label();
		this.te_password = new DevExpress.XtraEditors.TextEdit();
		this.label9 = new System.Windows.Forms.Label();
		this.te_normalfaturaevrakseri = new DevExpress.XtraEditors.TextEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.te_acentefaturaevrakseri = new DevExpress.XtraEditors.TextEdit();
		((System.ComponentModel.ISupportInitialize)this.te_specialalan1.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_specialalan2.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_specialalan3.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_autkey.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_branchcode.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_username.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_password.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_normalfaturaevrakseri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_acentefaturaevrakseri.Properties).BeginInit();
		base.SuspendLayout();
		this.sb_ayarlari_kaydet.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_ayarlari_kaydet.Appearance.Options.UseFont = true;
		this.sb_ayarlari_kaydet.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_ayarlari_kaydet.Location = new System.Drawing.Point(122, 300);
		this.sb_ayarlari_kaydet.Name = "sb_ayarlari_kaydet";
		this.sb_ayarlari_kaydet.Size = new System.Drawing.Size(139, 24);
		this.sb_ayarlari_kaydet.TabIndex = 9;
		this.sb_ayarlari_kaydet.Text = "Ayarları kaydet";
		this.sb_ayarlari_kaydet.Click += new System.EventHandler(sb_ayarlari_kaydet_Click);
		this.te_specialalan1.Location = new System.Drawing.Point(131, 176);
		this.te_specialalan1.Name = "te_specialalan1";
		this.te_specialalan1.Properties.MaxLength = 4;
		this.te_specialalan1.Size = new System.Drawing.Size(58, 20);
		this.te_specialalan1.TabIndex = 6;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(58, 179);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(67, 13);
		this.label1.TabIndex = 8;
		this.label1.Text = "Özel alan 1 :";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(58, 205);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(67, 13);
		this.label2.TabIndex = 10;
		this.label2.Text = "Özel alan 2 :";
		this.te_specialalan2.Location = new System.Drawing.Point(131, 202);
		this.te_specialalan2.Name = "te_specialalan2";
		this.te_specialalan2.Properties.MaxLength = 4;
		this.te_specialalan2.Size = new System.Drawing.Size(58, 20);
		this.te_specialalan2.TabIndex = 7;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(58, 231);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(67, 13);
		this.label3.TabIndex = 12;
		this.label3.Text = "Özel alan 3 :";
		this.te_specialalan3.Location = new System.Drawing.Point(131, 228);
		this.te_specialalan3.Name = "te_specialalan3";
		this.te_specialalan3.Properties.MaxLength = 4;
		this.te_specialalan3.Size = new System.Drawing.Size(58, 20);
		this.te_specialalan3.TabIndex = 8;
		this.label4.Location = new System.Drawing.Point(53, 251);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(290, 33);
		this.label4.TabIndex = 13;
		this.label4.Text = "Özel alanlar maksimum 4 karakter olabilir. Mikro'ya aktarım esnasında special kolonlarına yazılır.";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(76, 24);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(49, 13);
		this.label5.TabIndex = 15;
		this.label5.Text = "AutKey :";
		this.te_autkey.Location = new System.Drawing.Point(131, 21);
		this.te_autkey.Name = "te_autkey";
		this.te_autkey.Size = new System.Drawing.Size(212, 20);
		this.te_autkey.TabIndex = 14;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(50, 50);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(75, 13);
		this.label6.TabIndex = 17;
		this.label6.Text = "Branch Code :";
		this.te_branchcode.Location = new System.Drawing.Point(131, 47);
		this.te_branchcode.Name = "te_branchcode";
		this.te_branchcode.Size = new System.Drawing.Size(212, 20);
		this.te_branchcode.TabIndex = 16;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(59, 76);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(66, 13);
		this.label7.TabIndex = 19;
		this.label7.Text = "User Name :";
		this.te_username.Location = new System.Drawing.Point(131, 73);
		this.te_username.Name = "te_username";
		this.te_username.Size = new System.Drawing.Size(212, 20);
		this.te_username.TabIndex = 18;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(65, 102);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(60, 13);
		this.label8.TabIndex = 21;
		this.label8.Text = "Password :";
		this.te_password.Location = new System.Drawing.Point(131, 99);
		this.te_password.Name = "te_password";
		this.te_password.Size = new System.Drawing.Size(212, 20);
		this.te_password.TabIndex = 20;
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(27, 128);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(98, 13);
		this.label9.TabIndex = 23;
		this.label9.Text = "Normal Evrak Seri :";
		this.te_normalfaturaevrakseri.Location = new System.Drawing.Point(131, 125);
		this.te_normalfaturaevrakseri.Name = "te_normalfaturaevrakseri";
		this.te_normalfaturaevrakseri.Size = new System.Drawing.Size(58, 20);
		this.te_normalfaturaevrakseri.TabIndex = 22;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(26, 153);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(99, 13);
		this.label10.TabIndex = 25;
		this.label10.Text = "Acente Evrak Seri :";
		this.te_acentefaturaevrakseri.Location = new System.Drawing.Point(131, 150);
		this.te_acentefaturaevrakseri.Name = "te_acentefaturaevrakseri";
		this.te_acentefaturaevrakseri.Size = new System.Drawing.Size(58, 20);
		this.te_acentefaturaevrakseri.TabIndex = 24;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(382, 336);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.te_acentefaturaevrakseri);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.te_normalfaturaevrakseri);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.te_password);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.te_username);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.te_branchcode);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.te_autkey);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.te_specialalan3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.te_specialalan2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.te_specialalan1);
		base.Controls.Add(this.sb_ayarlari_kaydet);
		base.Name = "Touristica_GenelParametreler";
		this.Text = "Genel Parametreler";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		((System.ComponentModel.ISupportInitialize)this.te_specialalan1.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_specialalan2.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_specialalan3.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_autkey.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_branchcode.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_username.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_password.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_normalfaturaevrakseri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_acentefaturaevrakseri.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
