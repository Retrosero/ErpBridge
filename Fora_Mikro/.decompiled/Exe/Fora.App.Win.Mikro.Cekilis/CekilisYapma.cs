using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;

namespace Fora.App.Win.Mikro.Cekilis;

public class CekilisYapma : XtraForm
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	public Parametreler _KullaniciParametreleri;

	private IContainer components;

	private SimpleButton sb_cekilis_yap;

	public CekilisYapma(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void Giris_Load(object sender, EventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string commandText = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo]._FORA_CEKILIS_KAZANANLAR]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[_FORA_CEKILIS_KAZANANLAR]([RECno] [int] IDENTITY(1,1) NOT NULL,[Kazanan_kupon_kodu] [int] NOT NULL,[HediyeKodu] [nvarchar](50) NOT NULL,CONSTRAINT [PK__FORA_CEKILIS_KAZANANLAR] PRIMARY KEY CLUSTERED ([RECno] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] END";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.ExecuteNonQuery();
		}
		sqlDB.ConnectionClose();
	}

	private void sb_cekilis_yap_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show("Eski çekiliş sonuçları sıfırlanıp, tekrar çekiliş yapılacak. Onaylıyor musunuz?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		try
		{
			SqlCommand sqlCommand = new SqlCommand("DELETE _FORA_CEKILIS_KAZANANLAR", sqlDB.Connection);
			sqlCommand.ExecuteNonQuery();
			sqlCommand.Dispose();
			List<CekilisHediyeler> list = new List<CekilisHediyeler>();
			SqlDataReader sqlDataReader = new SqlCommand("SELECT HediyeKodu,HediyeAdi,Adet FROM _FORA_CEKILIS_HEDIYELER ORDER BY CekilisSirasi", sqlDB.Connection).ExecuteReader();
			while (sqlDataReader.Read())
			{
				CekilisHediyeler cekilisHediyeler = new CekilisHediyeler();
				cekilisHediyeler.HediyeKodu = sqlDataReader.GetSafeString(0);
				cekilisHediyeler.HediyeAdi = sqlDataReader.GetSafeString(1);
				cekilisHediyeler.HediyeAdeti = sqlDataReader.GetSafeInt32(2);
				list.Add(cekilisHediyeler);
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			Random random = new Random((int)DateTime.Now.Ticks);
			bool flag = true;
			foreach (CekilisHediyeler item in list)
			{
				if (!flag)
				{
					break;
				}
				for (int i = 0; i < item.HediyeAdeti; i++)
				{
					List<int> list2 = new List<int>();
					sqlDataReader = new SqlCommand("SELECT kupon_kodu FROM _FORA_KUPON_KODLARI WHERE cari_kod not in ( SELECT cari_kod FROM _FORA_KUPON_KODLARI WHERE kupon_kodu in ( SELECT Kazanan_kupon_kodu FROM _FORA_CEKILIS_KAZANANLAR))", sqlDB.Connection).ExecuteReader();
					while (sqlDataReader.Read())
					{
						list2.Add(sqlDataReader.GetSafeInt32(0));
					}
					sqlDataReader.Close();
					sqlDataReader.Dispose();
					sqlDataReader = null;
					if (list2.Count == 0)
					{
						MessageBox.Show("Hediye kazanmamış müşteri kalmadığı için bütün hediyeler dağıtılmadan çekiliş sonlandırıldı.");
						flag = false;
						break;
					}
					Console.WriteLine("Kupon kodu sayısı : " + list2.Count);
					Console.WriteLine("Hediye adı : " + item.HediyeAdi);
					int index = random.Next(list2.Count);
					int num = list2[index];
					SqlCommand sqlCommand2 = new SqlCommand("INSERT INTO _FORA_CEKILIS_KAZANANLAR (Kazanan_kupon_kodu,HediyeKodu) VALUES (@Kazanan_kupon_kodu,@HediyeKodu)", sqlDB.Connection);
					sqlCommand2.Parameters.AddWithValue("@Kazanan_kupon_kodu", num);
					sqlCommand2.Parameters.AddWithValue("@HediyeKodu", item.HediyeKodu);
					sqlCommand2.ExecuteNonQuery();
					sqlCommand2.Dispose();
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Çekiliş sonuçları OLUŞTURULAMADI! :" + ex.ToString());
			sqlDB.ConnectionClose();
			return;
		}
		sqlDB.ConnectionClose();
		MessageBox.Show("Çekiliş sonuçları oluşturuldu.");
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
		this.sb_cekilis_yap = new DevExpress.XtraEditors.SimpleButton();
		base.SuspendLayout();
		this.sb_cekilis_yap.Appearance.Font = new System.Drawing.Font("Tahoma", 30f, System.Drawing.FontStyle.Bold);
		this.sb_cekilis_yap.Appearance.Options.UseFont = true;
		this.sb_cekilis_yap.Location = new System.Drawing.Point(96, 62);
		this.sb_cekilis_yap.Name = "sb_cekilis_yap";
		this.sb_cekilis_yap.Size = new System.Drawing.Size(387, 87);
		this.sb_cekilis_yap.TabIndex = 10;
		this.sb_cekilis_yap.Text = "Çekiliş Yap";
		this.sb_cekilis_yap.Click += new System.EventHandler(sb_cekilis_yap_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(570, 221);
		base.Controls.Add(this.sb_cekilis_yap);
		base.Name = "CekilisYapma";
		this.Text = "Çekiliş yapma";
		base.Load += new System.EventHandler(Giris_Load);
		base.ResumeLayout(false);
	}
}
