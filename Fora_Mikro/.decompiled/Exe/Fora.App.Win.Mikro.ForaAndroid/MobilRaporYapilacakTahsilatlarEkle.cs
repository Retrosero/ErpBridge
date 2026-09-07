using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class MobilRaporYapilacakTahsilatlarEkle : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public string _RaporKodu = "";

	public string _RaporAdi = "";

	private IContainer components;

	private SimpleButton sb_kullanici_ekle;

	private TextEdit te_rapor_adi;

	private LabelControl labelControl3;

	private TextEdit te_rapor_kodu;

	private LabelControl labelControl1;

	public MobilRaporYapilacakTahsilatlarEkle(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void sb_kullanici_ekle_Click(object sender, EventArgs e)
	{
		bool flag = false;
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreUser FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='MobilRaporYapilacakTahsilatlar' GROUP BY ParametreUser ORDER BY ParametreUser";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					list.Add(sqlDataReader.GetSafeString(0));
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		foreach (string item in list)
		{
			if (te_rapor_kodu.Text == item)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			_RaporKodu = te_rapor_kodu.Text;
			_RaporAdi = te_rapor_adi.Text;
			base.DialogResult = DialogResult.OK;
			Close();
		}
		else
		{
			MessageBox.Show("Bu rapor kodu sistemde kayıtlı. Lütfen başka bir rapor kodu seçiniz.");
		}
	}

	private void ForaAndroidKullaniciEkle_Load(object sender, EventArgs e)
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
		this.sb_kullanici_ekle = new DevExpress.XtraEditors.SimpleButton();
		this.te_rapor_adi = new DevExpress.XtraEditors.TextEdit();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.te_rapor_kodu = new DevExpress.XtraEditors.TextEdit();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		((System.ComponentModel.ISupportInitialize)this.te_rapor_adi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_rapor_kodu.Properties).BeginInit();
		base.SuspendLayout();
		this.sb_kullanici_ekle.Appearance.Font = new System.Drawing.Font("Tahoma", 10.5f, System.Drawing.FontStyle.Bold);
		this.sb_kullanici_ekle.Appearance.Options.UseFont = true;
		this.sb_kullanici_ekle.Location = new System.Drawing.Point(134, 77);
		this.sb_kullanici_ekle.Name = "sb_kullanici_ekle";
		this.sb_kullanici_ekle.Size = new System.Drawing.Size(138, 28);
		this.sb_kullanici_ekle.TabIndex = 0;
		this.sb_kullanici_ekle.Text = "RAPOR EKLE";
		this.sb_kullanici_ekle.Click += new System.EventHandler(sb_kullanici_ekle_Click);
		this.te_rapor_adi.Location = new System.Drawing.Point(134, 51);
		this.te_rapor_adi.Name = "te_rapor_adi";
		this.te_rapor_adi.Size = new System.Drawing.Size(210, 20);
		this.te_rapor_adi.TabIndex = 9;
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(11, 54);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(117, 13);
		this.labelControl3.TabIndex = 8;
		this.labelControl3.Text = "Rapor adı :";
		this.te_rapor_kodu.Location = new System.Drawing.Point(134, 25);
		this.te_rapor_kodu.Name = "te_rapor_kodu";
		this.te_rapor_kodu.Size = new System.Drawing.Size(102, 20);
		this.te_rapor_kodu.TabIndex = 7;
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(11, 28);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(117, 13);
		this.labelControl1.TabIndex = 6;
		this.labelControl1.Text = "Rapor kodu :";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(370, 126);
		base.Controls.Add(this.te_rapor_adi);
		base.Controls.Add(this.labelControl3);
		base.Controls.Add(this.te_rapor_kodu);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.sb_kullanici_ekle);
		base.Name = "MobilRaporStokSatisEkle";
		this.Text = "Mobil Stok Satış Raporu Ekleme";
		base.Load += new System.EventHandler(ForaAndroidKullaniciEkle_Load);
		((System.ComponentModel.ISupportInitialize)this.te_rapor_adi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_rapor_kodu.Properties).EndInit();
		base.ResumeLayout(false);
	}
}
