using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Fora.Mikro;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.Rapor.StokEnvanter;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Tablolar;
using Fora.Mikro.TablolarV16;

namespace Fora.App.Win.Mikro.Debug;

public class DebugIslemleri : Form
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private IContainer components;

	private Button b_veritabani_karsilastir;

	private Label label5;

	private Label label8;

	private Button b_alinan_siparis_kaydet;

	private Button button1;

	public DebugIslemleri(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
	}

	private void b_veritabani_karsilastir_Click(object sender, EventArgs e)
	{
		List<TabloV16> mikroV16DefaultTablolar = TabloHelperV16.GetMikroV16DefaultTablolar();
		List<Tablo> mikroV14DefaultTablolar = TabloHelper.GetMikroV14DefaultTablolar();
		mikroV14DefaultTablolar.Add(TabloHelper.Get_MikroV15_Tablo_KUR_ISIMLERI());
		List<string> list = new List<string>();
		SqlDB sqlDB = new SqlDB();
		SqlDB sqlDB2 = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		sqlDB2.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName);
		foreach (TabloV16 item in mikroV16DefaultTablolar)
		{
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			DataTable dataTable = new DataTable();
			SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 * FROM " + item.TabloAdi + " WITH (NOLOCK) WHERE 1=0");
			if (item.TabloAdi == "DOVIZ_KURLARI" || item.TabloAdi == "YEREL_BANKA_KODLARI" || item.TabloAdi == "KUR_ISIMLERI")
			{
				sqlCommand.Connection = sqlDB2.Connection;
			}
			else
			{
				sqlCommand.Connection = sqlDB.Connection;
			}
			SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
			sqlDataAdapter.Fill(dataTable);
			sqlDataAdapter.Dispose();
			sqlCommand.Dispose();
			sqlCommand = null;
			Tablo tablo = null;
			foreach (Tablo item2 in mikroV14DefaultTablolar)
			{
				if (item2.TabloAdi == item.TabloAdi)
				{
					tablo = item2;
					break;
				}
			}
			foreach (Field item3 in tablo.Fieldlar)
			{
				bool flag = false;
				foreach (FieldV16 item4 in item.Fieldlar)
				{
					if (item3.Adi == item4.Adi)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list4.Add(item3.Adi);
				}
			}
			foreach (FieldV16 item5 in item.Fieldlar)
			{
				bool flag2 = false;
				foreach (DataColumn column in dataTable.Columns)
				{
					if (item5.Adi == column.ColumnName)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					list2.Add(item5.Adi);
				}
			}
			foreach (DataColumn column2 in dataTable.Columns)
			{
				bool flag3 = false;
				foreach (FieldV16 item6 in item.Fieldlar)
				{
					if (item6.Adi == column2.ColumnName)
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					list3.Add(column2.ColumnName);
				}
			}
			list.Add(item.TabloAdi);
			list.Add("==============");
			list.Add("Offline fieldlar");
			string text = "";
			string text2 = "";
			foreach (FieldV16 item7 in item.Fieldlar)
			{
				text = text + text2 + item7.Adi;
				text2 = ",";
			}
			list.Add(text);
			list.Add("V15 de fazla olan fieldlar");
			text = "";
			text2 = "";
			foreach (string item8 in list4)
			{
				text = text + text2 + item8;
				text2 = ",";
			}
			list.Add(text);
			list.Add("Offline da fazla olan fieldlar");
			text = "";
			text2 = "";
			foreach (string item9 in list2)
			{
				text = text + text2 + item9;
				text2 = ",";
			}
			list.Add(text);
			list.Add("Onlineda da fazla olan fieldlar");
			text = "";
			text2 = "";
			foreach (string item10 in list3)
			{
				text = text + text2 + item10;
				text2 = ",";
			}
			list.Add(text);
			list.Add("");
			list.Add("");
		}
		sqlDB.ConnectionClose();
		sqlDB2.ConnectionClose();
		string text3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "VeritabaniYapisi.txt");
		using (StreamWriter streamWriter = new StreamWriter(text3))
		{
			foreach (string item11 in list)
			{
				streamWriter.WriteLine(item11);
			}
		}
		MessageBox.Show("Veriler " + text3 + " dosyasina yazildi!");
	}

	private void b_alinan_siparis_kaydet_Click(object sender, EventArgs e)
	{
		SqlConnection sqlConnection = new SqlConnection();
		if (_mikrouygulamabilgileri.baglantibilgileri.SqlUserName == "")
		{
			sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; Trusted_Connection=true;";
		}
		else
		{
			sqlConnection.ConnectionString = "Data Source=" + _mikrouygulamabilgileri.baglantibilgileri.SqlServer + "; Initial Catalog=" + _mikrouygulamabilgileri.MikroFirmaDBName + "; User Id=" + _mikrouygulamabilgileri.baglantibilgileri.SqlUserName + ";Password=" + _mikrouygulamabilgileri.baglantibilgileri.SqlPassword + ";";
		}
		sqlConnection.Open();
		SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		int num;
		try
		{
			Evrak evrak = new Evrak();
			evrak.evraktipi = enum_GenelEvrakTipleri.AlinanSiparis;
			Cari cari = new Cari();
			cari.cari_kod = "CAR.01";
			evrak.SetCari(cari, 1, 0, "", new FiyatListesi(), 0, new Kur());
			Stok stok = new Stok();
			stok.sto_kod = "STO.01";
			stok.ekleme_bilgileri = new StokEklemeBilgileri();
			stok.ekleme_bilgileri.BirimFiyat = new FiyatTanimlamasi();
			stok.ekleme_bilgileri.BirimFiyat.FiyatBrut = 10.0;
			stok.ekleme_bilgileri.Miktar = 2.0;
			evrak.AddUrun(stok);
			num = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false);
			sqlTransaction.Commit();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Hata: " + ex.ToString());
			num = -1;
			sqlTransaction.Rollback();
		}
		finally
		{
			sqlConnection.Close();
		}
		MessageBox.Show(num.ToString());
	}

	private void button1_Click(object sender, EventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		SqlDB sqlDB2 = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		sqlDB2.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName);
		RaporStokEnvanterData.GetRapor(sqlDB.Connection, sqlDB2.Connection, new RaporStokEnvanterSecenekleri(), "", "", "", "", "", "", "");
		sqlDB.ConnectionClose();
		sqlDB2.ConnectionClose();
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
		this.b_veritabani_karsilastir = new System.Windows.Forms.Button();
		this.label5 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.b_alinan_siparis_kaydet = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.b_veritabani_karsilastir.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.b_veritabani_karsilastir.Location = new System.Drawing.Point(255, 69);
		this.b_veritabani_karsilastir.Name = "b_veritabani_karsilastir";
		this.b_veritabani_karsilastir.Size = new System.Drawing.Size(249, 23);
		this.b_veritabani_karsilastir.TabIndex = 33;
		this.b_veritabani_karsilastir.Text = "VERİTABANLARINI KARŞILAŞTIR";
		this.b_veritabani_karsilastir.UseVisualStyleBackColor = true;
		this.b_veritabani_karsilastir.Click += new System.EventHandler(b_veritabani_karsilastir_Click);
		this.label5.BackColor = System.Drawing.SystemColors.GrayText;
		this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label5.ForeColor = System.Drawing.SystemColors.Menu;
		this.label5.Location = new System.Drawing.Point(12, 9);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(739, 23);
		this.label5.TabIndex = 9;
		this.label5.Text = "V16 online ve offline veritabanı karşılaştırma.";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(12, 43);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(129, 13);
		this.label8.TabIndex = 10;
		this.label8.Text = "Oluşturulacak stok sayısı :";
		this.b_alinan_siparis_kaydet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.b_alinan_siparis_kaydet.Location = new System.Drawing.Point(15, 158);
		this.b_alinan_siparis_kaydet.Name = "b_alinan_siparis_kaydet";
		this.b_alinan_siparis_kaydet.Size = new System.Drawing.Size(249, 23);
		this.b_alinan_siparis_kaydet.TabIndex = 34;
		this.b_alinan_siparis_kaydet.Text = "ALINAN SİPARİŞ KAYDET";
		this.b_alinan_siparis_kaydet.UseVisualStyleBackColor = true;
		this.b_alinan_siparis_kaydet.Click += new System.EventHandler(b_alinan_siparis_kaydet_Click);
		this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.button1.Location = new System.Drawing.Point(270, 158);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(249, 23);
		this.button1.TabIndex = 35;
		this.button1.Text = "STOK ENVANTER RAPORU AL";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(763, 593);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.b_alinan_siparis_kaydet);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.b_veritabani_karsilastir);
		base.Name = "DebugIslemleri";
		this.Text = "Performans Test Kurulumu";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
