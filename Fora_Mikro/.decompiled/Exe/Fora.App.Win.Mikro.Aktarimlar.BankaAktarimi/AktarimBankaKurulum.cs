using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;

public class AktarimBankaKurulum : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private string _DBName;

	private Parametreler _parametreler;

	private IContainer components;

	private SimpleButton sb_kuruluma_basla;

	private Label label_islem;

	private System.Windows.Forms.ProgressBar progressBar1;

	public AktarimBankaKurulum(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void GenelParametreler_Load(object sender, EventArgs e)
	{
	}

	private void sb_kuruluma_basla_Click(object sender, EventArgs e)
	{
		bool flag = true;
		try
		{
			string text = "RECno";
			string text2 = "int";
			if (AppBase.MikroVersiyonu >= 16)
			{
				text = "Guid";
				text2 = "uniqueidentifier";
			}
			string mikroFirmaDBName = _mikrouygulamabilgileri.MikroFirmaDBName;
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			label_islem.Text = "Katalag oluşturuluyor";
			Application.DoEvents();
			string commandText = "IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'Fora') BEGIN CREATE FULLTEXT CATALOG [Fora] WITH ACCENT_SENSITIVITY = OFF AS DEFAULT END";
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.ExecuteNonQuery();
			}
			label_islem.Text = "Tablo oluşturuluyor";
			Application.DoEvents();
			commandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_BANKA_AKTARIM_SEARCH]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[_FORA_BANKA_AKTARIM_SEARCH]([ID] [int] IDENTITY(1,1) NOT NULL,[TableID] [int] NOT NULL,[Table" + text + "] [" + text2 + "] NOT NULL,[FullText] [nvarchar](max) NOT NULL,CONSTRAINT [PK__FORA_BANKA_AKTARIM_SEARCH] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY]CREATE FULLTEXT INDEX ON [dbo]. [_FORA_BANKA_AKTARIM_SEARCH] (FullText) KEY INDEX PK__FORA_BANKA_AKTARIM_SEARCH ON Fora WITH CHANGE_TRACKING AUTO END";
			using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand2.CommandText = commandText;
				sqlCommand2.ExecuteNonQuery();
			}
			label_islem.Text = "Tablo sıfırlanıyor";
			Application.DoEvents();
			commandText = "DELETE FROM _FORA_BANKA_AKTARIM_SEARCH";
			using (SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand3.CommandText = commandText;
				sqlCommand3.ExecuteNonQuery();
			}
			List<int> list = new List<int>();
			List<Guid> list2 = new List<Guid>();
			List<int> list3 = new List<int>();
			List<string> list4 = new List<string>();
			label_islem.Text = "Cari hesaplar indeksleniyor";
			Application.DoEvents();
			commandText = "SELECT cari_" + text + ",cari_fileid,cari_unvan1,cari_unvan2,cari_vdaire_no,cari_VergiKimlikNo,cari_banka_hesapno1,cari_banka_hesapno2,cari_banka_hesapno3,cari_EMail,cari_CepTel,adres.adr_ilce,adres.adr_il FROM CARI_HESAPLAR WITH(NOLOCK) LEFT OUTER JOIN CARI_HESAP_ADRESLERI AS adres WITH(NOLOCK) ON CARI_HESAPLAR.cari_kod=adres.adr_cari_kod WHERE (adres.adr_adres_no=1 OR adres.adr_adres_no IS NULL)";
			using (SqlCommand sqlCommand4 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand4.CommandText = commandText;
				SqlDataReader sqlDataReader = sqlCommand4.ExecuteReader();
				while (sqlDataReader.Read())
				{
					int safeInt = sqlDataReader.GetSafeInt16(1);
					string text3 = "";
					for (int i = 2; i < sqlDataReader.FieldCount; i++)
					{
						string safeString = sqlDataReader.GetSafeString(i);
						if (safeString != "")
						{
							text3 = text3 + safeString + " ";
						}
					}
					if (AppBase.MikroVersiyonu >= 16)
					{
						list2.Add(sqlDataReader.GetGuid(0));
					}
					else
					{
						list.Add(sqlDataReader.GetSafeInt32(0));
					}
					list3.Add(safeInt);
					list4.Add(text3);
				}
				sqlDataReader.Close();
				sqlDataReader.Dispose();
				sqlDataReader = null;
			}
			string commandText2 = "IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'CARI_HESAPLAR_BANKA_AKTARIM' AND type = 'TR') DROP TRIGGER dbo.CARI_HESAPLAR_BANKA_AKTARIM  EXEC('  CREATE TRIGGER dbo.CARI_HESAPLAR_BANKA_AKTARIM  ON CARI_HESAPLAR  AFTER INSERT, UPDATE, DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = 31; IF EXISTS (SELECT * FROM deleted)  DELETE " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH WHERE TableID=@TabloID AND Table" + text + " in (SELECT cari_" + text + " FROM deleted) END IF EXISTS (SELECT * FROM inserted) INSERT INTO " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH  SELECT cari_fileid AS TableID,carihesap.cari_" + text + " AS Table" + text + ",COALESCE(carihesap.cari_unvan1,'''') + '' '' + COALESCE(carihesap.cari_unvan2,'''') + '' '' + COALESCE(carihesap.cari_vdaire_no,'''') + '' '' + COALESCE(carihesap.cari_VergiKimlikNo,'''') + '' '' + COALESCE(carihesap.cari_banka_hesapno1,'''') + '' '' + COALESCE(carihesap.cari_banka_hesapno2,'''') + '' '' + COALESCE(carihesap.cari_banka_hesapno3,'''') + '' '' + COALESCE(carihesap.cari_EMail,'''') + '' '' + COALESCE(carihesap.cari_CepTel,'''') + '' '' + COALESCE(adres.adr_ilce,'''') + '' '' + COALESCE(adres.adr_il,'''') AS FullText FROM inserted AS carihesap WITH(NOLOCK)  LEFT OUTER JOIN CARI_HESAP_ADRESLERI AS adres WITH(NOLOCK) ON carihesap.cari_kod=adres.adr_cari_kod WHERE (adres.adr_adres_no=1 OR adres.adr_adres_no IS NULL) ')  SELECT 1";
			using (SqlCommand sqlCommand5 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand5.CommandText = commandText2;
				sqlCommand5.ExecuteScalar();
			}
			label_islem.Text = "Cari personeller indeksleniyor";
			Application.DoEvents();
			commandText = "SELECT cari_per_" + text + ",cari_per_fileid,cari_per_adi,cari_per_soyadi,cari_per_banka_hesapno FROM CARI_PERSONEL_TANIMLARI WITH(NOLOCK)";
			using (SqlCommand sqlCommand6 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand6.CommandText = commandText;
				SqlDataReader sqlDataReader2 = sqlCommand6.ExecuteReader();
				while (sqlDataReader2.Read())
				{
					int safeInt2 = sqlDataReader2.GetSafeInt16(1);
					string text4 = "";
					for (int j = 2; j < sqlDataReader2.FieldCount; j++)
					{
						string safeString2 = sqlDataReader2.GetSafeString(j);
						if (safeString2 != "")
						{
							text4 = text4 + safeString2 + " ";
						}
					}
					if (AppBase.MikroVersiyonu >= 16)
					{
						list2.Add(sqlDataReader2.GetGuid(0));
					}
					else
					{
						list.Add(sqlDataReader2.GetSafeInt32(0));
					}
					list3.Add(safeInt2);
					list4.Add(text4);
				}
				sqlDataReader2.Close();
				sqlDataReader2.Dispose();
				sqlDataReader2 = null;
			}
			commandText2 = "IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'CARI_PERSONEL_TANIMLARI_BANKA_AKTARIM' AND type = 'TR') DROP TRIGGER dbo.CARI_PERSONEL_TANIMLARI_BANKA_AKTARIM  EXEC('  CREATE TRIGGER dbo.CARI_PERSONEL_TANIMLARI_BANKA_AKTARIM  ON CARI_PERSONEL_TANIMLARI  AFTER INSERT, UPDATE, DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = 104; IF EXISTS (SELECT * FROM deleted)  DELETE " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH WHERE TableID=@TabloID AND Table" + text + " in (SELECT cari_per_" + text + " FROM deleted) END IF EXISTS (SELECT * FROM inserted) INSERT INTO " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH  SELECT cari_per_fileid AS TableID,cari_per_" + text + " AS Table" + text + ",COALESCE(cari_per_adi,'''') + '' '' + COALESCE(cari_per_soyadi,'''') + '' '' + COALESCE(cari_per_banka_hesapno,'''') FROM inserted WITH(NOLOCK)  ')  SELECT 1";
			using (SqlCommand sqlCommand7 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand7.CommandText = commandText2;
				sqlCommand7.ExecuteScalar();
			}
			label_islem.Text = "Personeller indeksleniyor";
			Application.DoEvents();
			commandText = "SELECT per_" + text + ",per_fileid,per_adi,per_soyadi,per_ucr_hesapno FROM PERSONELLER WITH(NOLOCK)";
			using (SqlCommand sqlCommand8 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand8.CommandText = commandText;
				SqlDataReader sqlDataReader3 = sqlCommand8.ExecuteReader();
				while (sqlDataReader3.Read())
				{
					int safeInt3 = sqlDataReader3.GetSafeInt16(1);
					string text5 = "";
					for (int k = 2; k < sqlDataReader3.FieldCount; k++)
					{
						string safeString3 = sqlDataReader3.GetSafeString(k);
						if (safeString3 != "")
						{
							text5 = text5 + safeString3 + " ";
						}
					}
					if (AppBase.MikroVersiyonu >= 16)
					{
						list2.Add(sqlDataReader3.GetGuid(0));
					}
					else
					{
						list.Add(sqlDataReader3.GetSafeInt32(0));
					}
					list3.Add(safeInt3);
					list4.Add(text5);
				}
				sqlDataReader3.Close();
				sqlDataReader3.Dispose();
				sqlDataReader3 = null;
			}
			commandText2 = "IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'PERSONELLER_BANKA_AKTARIM' AND type = 'TR') DROP TRIGGER dbo.PERSONELLER_BANKA_AKTARIM  EXEC('  CREATE TRIGGER dbo.PERSONELLER_BANKA_AKTARIM  ON PERSONELLER  AFTER INSERT, UPDATE, DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = 71; IF EXISTS (SELECT * FROM deleted)  DELETE " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH WHERE TableID=@TabloID AND Table" + text + " in (SELECT per_" + text + " FROM deleted) END IF EXISTS (SELECT * FROM inserted) INSERT INTO " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH  SELECT per_fileid AS TableID,per_" + text + " AS Table" + text + ",COALESCE(per_adi,'''') + '' '' + COALESCE(per_soyadi,'''') + '' '' + COALESCE(per_ucr_hesapno,'''') FROM inserted WITH(NOLOCK)  ')  SELECT 1";
			using (SqlCommand sqlCommand9 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand9.CommandText = commandText2;
				sqlCommand9.ExecuteScalar();
			}
			label_islem.Text = "Hizmet hesapları indeksleniyor";
			Application.DoEvents();
			commandText = "SELECT his_" + text + ",his_fileid,his_isim,his_yabanci_isim FROM MASRAF_HESAPLARI WITH(NOLOCK)";
			using (SqlCommand sqlCommand10 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand10.CommandText = commandText;
				SqlDataReader sqlDataReader4 = sqlCommand10.ExecuteReader();
				while (sqlDataReader4.Read())
				{
					int safeInt4 = sqlDataReader4.GetSafeInt16(1);
					string text6 = "";
					for (int l = 2; l < sqlDataReader4.FieldCount; l++)
					{
						string safeString4 = sqlDataReader4.GetSafeString(l);
						if (safeString4 != "")
						{
							text6 = text6 + safeString4 + " ";
						}
					}
					if (AppBase.MikroVersiyonu >= 16)
					{
						list2.Add(sqlDataReader4.GetGuid(0));
					}
					else
					{
						list.Add(sqlDataReader4.GetSafeInt32(0));
					}
					list3.Add(safeInt4);
					list4.Add(text6);
				}
				sqlDataReader4.Close();
				sqlDataReader4.Dispose();
				sqlDataReader4 = null;
			}
			commandText2 = "IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'MASRAF_HESAPLARI_BANKA_AKTARIM' AND type = 'TR') DROP TRIGGER dbo.MASRAF_HESAPLARI_BANKA_AKTARIM  EXEC('  CREATE TRIGGER dbo.MASRAF_HESAPLARI_BANKA_AKTARIM  ON MASRAF_HESAPLARI  AFTER INSERT, UPDATE, DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = 62; IF EXISTS (SELECT * FROM deleted)  DELETE " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH WHERE TableID=@TabloID AND Table" + text + " in (SELECT his_" + text + " FROM deleted) END IF EXISTS (SELECT * FROM inserted) INSERT INTO " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH  SELECT his_fileid AS TableID,his_" + text + " AS Table" + text + ",COALESCE(his_isim,'''') + '' '' + COALESCE(his_yabanci_isim,'''') FROM inserted WITH(NOLOCK)  ')  SELECT 1";
			using (SqlCommand sqlCommand11 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand11.CommandText = commandText2;
				sqlCommand11.ExecuteScalar();
			}
			label_islem.Text = "Banka hesapları indeksleniyor";
			Application.DoEvents();
			commandText = "SELECT ban_" + text + ",ban_fileid,ban_ismi,ban_sube,ban_SwiftKodu,ban_IBANKodu,ban_hesapno FROM BANKALAR WITH(NOLOCK)";
			using (SqlCommand sqlCommand12 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand12.CommandText = commandText;
				SqlDataReader sqlDataReader5 = sqlCommand12.ExecuteReader();
				while (sqlDataReader5.Read())
				{
					int safeInt5 = sqlDataReader5.GetSafeInt16(1);
					string text7 = "";
					for (int m = 2; m < sqlDataReader5.FieldCount; m++)
					{
						string safeString5 = sqlDataReader5.GetSafeString(m);
						if (safeString5 != "")
						{
							text7 = text7 + safeString5 + " ";
						}
					}
					if (AppBase.MikroVersiyonu >= 16)
					{
						list2.Add(sqlDataReader5.GetGuid(0));
					}
					else
					{
						list.Add(sqlDataReader5.GetSafeInt32(0));
					}
					list3.Add(safeInt5);
					list4.Add(text7);
				}
				sqlDataReader5.Close();
				sqlDataReader5.Dispose();
				sqlDataReader5 = null;
			}
			commandText2 = "IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'BANKALAR_BANKA_AKTARIM' AND type = 'TR') DROP TRIGGER dbo.BANKALAR_BANKA_AKTARIM  EXEC('  CREATE TRIGGER dbo.BANKALAR_BANKA_AKTARIM  ON BANKALAR  AFTER INSERT, UPDATE, DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = 52; IF EXISTS (SELECT * FROM deleted)  DELETE " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH WHERE TableID=@TabloID AND Table" + text + " in (SELECT ban_" + text + " FROM deleted) END IF EXISTS (SELECT * FROM inserted) INSERT INTO " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH  SELECT ban_fileid AS TableID,ban_" + text + " AS Table" + text + ",COALESCE(ban_ismi,'''') + '' '' + COALESCE(ban_sube,'''') + '' '' + COALESCE(ban_SwiftKodu,'''') + '' '' + COALESCE(ban_IBANKodu,'''') + '' '' + COALESCE(ban_hesapno,'''') FROM inserted WITH(NOLOCK)  ')  SELECT 1";
			using (SqlCommand sqlCommand13 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand13.CommandText = commandText2;
				sqlCommand13.ExecuteScalar();
			}
			label_islem.Text = "Ödeme emirleri indeksleniyor";
			Application.DoEvents();
			commandText = "SELECT sck_" + text + ",sck_fileid,sck_no FROM ODEME_EMIRLERI WITH(NOLOCK) WHERE sck_nerede_cari_cins in (0,2) AND sck_tutar<>sck_odenen";
			using (SqlCommand sqlCommand14 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand14.CommandText = commandText;
				SqlDataReader sqlDataReader6 = sqlCommand14.ExecuteReader();
				while (sqlDataReader6.Read())
				{
					int safeInt6 = sqlDataReader6.GetSafeInt16(1);
					string text8 = "";
					for (int n = 2; n < sqlDataReader6.FieldCount; n++)
					{
						string safeString6 = sqlDataReader6.GetSafeString(n);
						if (safeString6 != "")
						{
							text8 = text8 + safeString6 + " ";
						}
					}
					if (text8 != "")
					{
						if (AppBase.MikroVersiyonu >= 16)
						{
							list2.Add(sqlDataReader6.GetGuid(0));
						}
						else
						{
							list.Add(sqlDataReader6.GetSafeInt32(0));
						}
						list3.Add(safeInt6);
						list4.Add(text8);
					}
				}
				sqlDataReader6.Close();
				sqlDataReader6.Dispose();
				sqlDataReader6 = null;
			}
			commandText2 = "IF EXISTS (SELECT * FROM sys.triggers WHERE name = N'ODEME_EMIRLERI_BANKA_AKTARIM' AND type = 'TR') DROP TRIGGER dbo.ODEME_EMIRLERI_BANKA_AKTARIM  EXEC('  CREATE TRIGGER dbo.ODEME_EMIRLERI_BANKA_AKTARIM  ON ODEME_EMIRLERI  AFTER INSERT, UPDATE, DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = 54; IF EXISTS (SELECT * FROM deleted)  DELETE " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH WHERE TableID=@TabloID AND Table" + text + " in (SELECT sck_" + text + " FROM deleted) END IF EXISTS (SELECT * FROM inserted) INSERT INTO " + mikroFirmaDBName + ".dbo._FORA_BANKA_AKTARIM_SEARCH  SELECT sck_fileid AS TableID,sck_" + text + " AS Table" + text + ",COALESCE(sck_no,'''') FROM inserted WITH(NOLOCK) WHERE  sck_nerede_cari_cins in (0,2) AND sck_tutar<>sck_odenen ')  SELECT 1";
			using (SqlCommand sqlCommand15 = sqlDB.Connection.CreateCommand())
			{
				sqlCommand15.CommandText = commandText2;
				sqlCommand15.ExecuteScalar();
			}
			label_islem.Text = "İndeksler yazılıyor";
			progressBar1.Maximum = list3.Count;
			progressBar1.Value = 0;
			Application.DoEvents();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
			SqlCommand sqlCommand16 = new SqlCommand(string.Concat("INSERT INTO _FORA_BANKA_AKTARIM_SEARCH (TableID,Table" + text + ",FullText) VALUES", "(@TableID,@Table", text, ",@FullText)"), sqlDB.Connection);
			sqlCommand16.Transaction = sqlTransaction;
			sqlCommand16.Parameters.Add("@TableID", DbType.Int32);
			if (AppBase.MikroVersiyonu >= 16)
			{
				sqlCommand16.Parameters.Add("@Table" + text, DbType.Guid);
			}
			else
			{
				sqlCommand16.Parameters.Add("@Table" + text, DbType.Int32);
			}
			sqlCommand16.Parameters.Add("@FullText", DbType.String);
			for (int num = 0; num < list3.Count; num++)
			{
				sqlCommand16.Parameters[0].Value = list3[num];
				if (AppBase.MikroVersiyonu >= 16)
				{
					sqlCommand16.Parameters[1].Value = list2[num];
				}
				else
				{
					sqlCommand16.Parameters[1].Value = list[num];
				}
				sqlCommand16.Parameters[2].Value = list4[num];
				sqlCommand16.ExecuteNonQuery();
				progressBar1.Value = num;
			}
			sqlTransaction.Commit();
			sqlCommand16.Dispose();
			sqlTransaction.Dispose();
			stopwatch.Stop();
			Console.WriteLine("İndeksleri yazma suresi = {0}", stopwatch.Elapsed);
			sqlDB.ConnectionClose();
		}
		catch (Exception)
		{
			flag = false;
		}
		if (flag)
		{
			MessageBox.Show("Banka aktarım kurulumu başarılı bir şekilde tamamlandı.", "TAMAMLANDI", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		else
		{
			MessageBox.Show("Kurulum Tamamlanamadı. Sql bağlantısını ve Sql kullanıcı haklarını kontrol ediniz.", "HATA", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
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
		this.sb_kuruluma_basla = new DevExpress.XtraEditors.SimpleButton();
		this.label_islem = new System.Windows.Forms.Label();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		base.SuspendLayout();
		this.sb_kuruluma_basla.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kuruluma_basla.Appearance.Options.UseFont = true;
		this.sb_kuruluma_basla.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.sb_kuruluma_basla.Location = new System.Drawing.Point(77, 12);
		this.sb_kuruluma_basla.Name = "sb_kuruluma_basla";
		this.sb_kuruluma_basla.Size = new System.Drawing.Size(198, 24);
		this.sb_kuruluma_basla.TabIndex = 20;
		this.sb_kuruluma_basla.Text = "Kuruluma Başla";
		this.sb_kuruluma_basla.Click += new System.EventHandler(sb_kuruluma_basla_Click);
		this.label_islem.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem.Location = new System.Drawing.Point(12, 50);
		this.label_islem.Name = "label_islem";
		this.label_islem.Size = new System.Drawing.Size(326, 23);
		this.label_islem.TabIndex = 21;
		this.label_islem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.progressBar1.Location = new System.Drawing.Point(15, 86);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(323, 23);
		this.progressBar1.TabIndex = 22;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(350, 130);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.label_islem);
		base.Controls.Add(this.sb_kuruluma_basla);
		base.Name = "AktarimBankaKurulum";
		this.Text = "Aktarım Banka Kurulumu";
		base.Load += new System.EventHandler(GenelParametreler_Load);
		base.ResumeLayout(false);
	}
}
