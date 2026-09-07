using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.TablolarV16;

namespace Fora.App.Mikro.Kurulum;

public class ForaMikroVeritabaniKurulumu : Form
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private List<TabloV16> _tablolar;

	private List<string> HataMesajlari;

	private IContainer components;

	private Button b_kurulumu_baslat;

	private Label label_islem_bilgi;

	private ProgressBar progressBar1;

	private Label label2;

	private BackgroundWorker bw_kurulum;

	private Button button1;

	private TextBox tb_diger_veritabani;

	private Label label1;

	private Label label3;

	private Label label4;

	private Label label5;

	public ForaMikroVeritabaniKurulumu(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		_tablolar = TabloHelperV16.GetMikroV16DefaultTablolar();
		TabloV16 mikroV16_Tablo__FORA_PARAMETRELER = TabloHelperV16.Get_MikroV16_Tablo__FORA_PARAMETRELER();
		_tablolar.Add(mikroV16_Tablo__FORA_PARAMETRELER);
		try
		{
			string commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_SYNC]')) BEGIN CREATE TABLE [dbo].[_FORA_SYNC]([TriggerRECno] [int] IDENTITY(1,1) NOT NULL,[TabloID] [int] NOT NULL,[KayitGuid] [uniqueidentifier] NOT NULL, CONSTRAINT [PK__FORA_SYNC] PRIMARY KEY CLUSTERED  ([TriggerRECno] ASC ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] ) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_FORA_SYNC] ([TabloID] ASC,[TriggerRECno] ASC) INCLUDE ([KayitGuid]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [02] ON [dbo].[_FORA_SYNC] ([KayitGuid] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.ExecuteScalar();
			}
			commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_FORA_SYNC_DEL]')) BEGIN CREATE TABLE [dbo].[_FORA_SYNC_DEL]([TriggerRECno] [int] IDENTITY(1,1) NOT NULL,[TabloID] [int] NOT NULL,[KayitGuid] [uniqueidentifier] NOT NULL, CONSTRAINT [PK__FORA_SYNC_DEL] PRIMARY KEY CLUSTERED  ([TriggerRECno] ASC ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] ) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_FORA_SYNC_DEL] ([TabloID] ASC,[TriggerRECno] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
			using SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand();
			sqlCommand2.CommandText = commandText;
			sqlCommand2.ExecuteScalar();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Fora senkronizasyon tabloları oluşturulamadı. ex : " + ex.ToString());
		}
		try
		{
			string commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_ZIYARET_HAREKETLERI]')) BEGIN CREATE TABLE [dbo].[_ZIYARET_HAREKETLERI]([zyrt_Guid] [uniqueidentifier] NOT NULL,[zyrt_Create_Date] [datetime] NOT NULL,[zyrt_Lastup_Date] [datetime] NOT NULL,[zyrt_iptal] [bit] NOT NULL,[zyrt_Tarihi] [datetime] NOT NULL,[zyrt_Temsilci_Kodu] [nvarchar](25) NOT NULL,[zyrt_Bolge_Kodu] [nvarchar](25) NOT NULL,[zyrt_Cari_Kodu] [nvarchar](25) NOT NULL,[zyrt_Cari_Adres_No] [int] NOT NULL,[zyrt_Cari_Adres_Enlem] [float] NOT NULL,[zyrt_Cari_Adres_Boylam] [float] NOT NULL,[zyrt_Baslama_Saati] [datetime] NOT NULL,[zyrt_Baslama_Enlem] [float] NOT NULL,[zyrt_Baslama_Boylam] [float] NOT NULL,[zyrt_Bitis_Saati] [datetime] NOT NULL,[zyrt_Bitis_Enlem] [float] NOT NULL,[zyrt_Bitis_Boylam] [float] NOT NULL,[zyrt_Tamamlandi] [bit] NOT NULL,[zyrt_Fotograf_Id] [int] NOT NULL,[zyrt_Proje_Kodu] [nvarchar](25) NOT NULL,[zyrt_Sor_Mer_Kodu] [nvarchar](25) NOT NULL,[zyrt_Bakim_Evrak_Seri] [nvarchar](6) NOT NULL,[zyrt_Bakim_Evrak_Sira] [int] NOT NULL,[zyrt_Satis_Yapildi] [bit] NOT NULL,[zyrt_Siparis_Alindi] [bit] NOT NULL,[zyrt_Urun_Teslim_Edildi] [bit] NOT NULL,[zyrt_Tahsilat_Yapildi] [bit] NOT NULL,[zyrt_Katalog_Birakildi] [bit] NOT NULL,[zyrt_Fiyat_Listesi_Birakildi] [bit] NOT NULL,[zyrt_Numune_Urun_Birakildi] [bit] NOT NULL,[zyrt_Konsinye_Urun_Birakildi] [bit] NOT NULL,[zyrt_Promosyon_Birakildi] [bit] NOT NULL,[zyrt_Firma_Durumu] [tinyint] NOT NULL,[zyrt_Rakip_Firma_Var] [bit] NOT NULL,[zyrt_Rakip_Firma_Durumu] [tinyint] NOT NULL,[zyrt_Urun_Yerlesim_Durumu] [tinyint] NOT NULL,[zyrt_Rakip_Urun_Yerlesim_Durumu] [tinyint] NOT NULL,[zyrt_Temsilci_Aciklama] [nvarchar](127) NOT NULL,[zyrt_Cari_Firma_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Urun_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Fiyat_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Temsilci_Memnuniyeti] [tinyint] NOT NULL,[zyrt_Cari_Aciklama] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_01_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_01_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_01_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_01_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_01_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_01_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_01_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_02_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_02_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_02_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_02_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_02_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_02_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_02_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_03_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_03_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_03_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_03_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_03_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_03_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_03_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_04_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_04_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_04_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_04_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_04_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_04_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_04_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_05_Var_Yok] [bit] NOT NULL,[zyrt_Temsilci_Ozel_05_Evet_Hayir] [bit] NOT NULL,[zyrt_Temsilci_Ozel_05_Derece] [tinyint] NOT NULL,[zyrt_Temsilci_Ozel_05_TamSayi] [int] NOT NULL,[zyrt_Temsilci_Ozel_05_OndalikliSayi] [float] NOT NULL,[zyrt_Temsilci_Ozel_05_Metin] [nvarchar](127) NOT NULL,[zyrt_Temsilci_Ozel_05_Fotograf_Id] [int] NOT NULL,[zyrt_Temsilci_Ozel_06_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_06_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_06_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_06_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_07_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_07_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_07_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_07_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_08_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_08_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_08_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_08_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_09_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_09_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_09_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_09_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_10_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_10_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_10_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_10_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_11_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_11_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_11_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_11_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_12_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_12_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_12_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_12_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_13_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_13_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_13_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_13_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_14_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_14_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_14_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_14_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_15_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_15_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_15_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_15_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_16_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_16_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_16_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_16_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_17_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_17_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_17_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_17_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_18_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_18_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_18_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_18_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_19_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_19_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_19_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_19_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_20_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_20_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_20_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_20_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_21_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_21_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_21_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_21_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_22_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_22_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_22_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_22_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_23_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_23_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_23_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_23_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_24_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_24_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_24_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_24_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_25_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_25_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_25_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_25_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_26_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_26_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_26_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_26_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_27_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_27_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_27_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_27_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_28_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_28_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_28_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_28_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_29_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_29_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_29_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_29_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_30_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_30_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_30_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_30_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_31_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_31_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_31_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_31_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_32_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_32_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_32_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_32_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_33_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_33_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_33_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_33_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_34_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_34_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_34_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_34_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_35_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_35_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_35_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_35_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_36_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_36_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_36_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_36_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_37_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_37_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_37_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_37_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_38_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_38_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_38_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_38_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_39_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_39_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_39_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_39_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_40_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_40_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_40_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_40_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_41_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_41_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_41_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_41_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_42_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_42_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_42_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_42_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_43_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_43_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_43_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_43_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_44_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_44_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_44_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_44_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_45_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_45_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_45_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_45_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_46_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_46_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_46_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_46_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_47_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_47_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_47_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_47_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_48_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_48_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_48_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_48_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_49_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_49_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_49_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_49_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_50_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_50_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_50_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_50_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_51_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_51_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_51_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_51_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_52_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_52_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_52_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_52_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_53_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_53_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_53_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_53_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_54_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_54_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_54_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_54_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_55_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_55_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_55_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_55_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_56_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_56_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_56_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_56_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_57_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_57_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_57_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_57_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_58_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_58_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_58_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_58_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_59_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_59_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_59_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_59_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_60_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_60_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_60_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_60_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_61_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_61_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_61_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_61_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_62_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_62_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_62_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_62_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_63_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_63_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_63_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_63_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_64_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_64_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_64_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_64_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_65_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_65_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_65_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_65_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_66_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_66_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_66_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_66_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_67_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_67_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_67_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_67_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_68_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_68_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_68_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_68_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_69_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_69_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_69_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_69_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_70_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_70_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_70_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_70_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_71_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_71_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_71_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_71_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_72_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_72_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_72_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_72_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_73_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_73_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_73_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_73_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_74_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_74_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_74_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_74_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_75_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_75_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_75_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_75_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_76_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_76_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_76_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_76_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_77_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_77_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_77_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_77_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_78_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_78_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_78_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_78_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_79_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_79_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_79_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_79_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_80_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_80_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_80_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_80_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_81_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_81_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_81_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_81_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_82_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_82_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_82_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_82_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_83_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_83_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_83_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_83_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_84_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_84_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_84_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_84_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_85_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_85_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_85_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_85_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_86_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_86_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_86_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_86_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_87_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_87_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_87_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_87_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_88_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_88_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_88_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_88_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_89_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_89_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_89_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_89_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_90_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_90_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_90_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_90_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_91_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_91_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_91_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_91_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_92_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_92_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_92_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_92_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_93_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_93_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_93_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_93_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_94_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_94_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_94_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_94_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_95_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_95_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_95_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_95_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_96_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_96_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_96_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_96_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_97_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_97_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_97_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_97_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_98_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_98_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_98_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_98_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_99_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_99_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_99_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_99_Metin] [nvarchar](127) NULL,[zyrt_Temsilci_Ozel_100_Var_Yok] [bit] NULL,[zyrt_Temsilci_Ozel_100_Evet_Hayir] [bit] NULL,[zyrt_Temsilci_Ozel_100_Derece] [tinyint] NULL,[zyrt_Temsilci_Ozel_100_Metin] [nvarchar](127) NULL,[zyrt_Cari_Ozel_01_Var_Yok] [bit] NOT NULL,[zyrt_Cari_Ozel_01_Evet_Hayir] [bit] NOT NULL,[zyrt_Cari_Ozel_01_Derece] [tinyint] NOT NULL,[zyrt_Cari_Ozel_01_TamSayi] [int] NOT NULL,[zyrt_Cari_Ozel_01_OndalikliSayi] [float] NOT NULL,[zyrt_Cari_Ozel_01_Metin] [nvarchar](127) NOT NULL,[zyrt_Cari_Ozel_01_Fotograf_Id] [int] NOT NULL,[zyrt_Cari_Ozel_02_Var_Yok] [bit] NOT NULL,[zyrt_Cari_Ozel_02_Evet_Hayir] [bit] NOT NULL,[zyrt_Cari_Ozel_02_Derece] [tinyint] NOT NULL,[zyrt_Cari_Ozel_02_TamSayi] [int] NOT NULL,[zyrt_Cari_Ozel_02_OndalikliSayi] [float] NOT NULL,[zyrt_Cari_Ozel_02_Metin] [nvarchar](127) NOT NULL,[zyrt_Cari_Ozel_02_Fotograf_Id] [int] NOT NULL, CONSTRAINT [PK__ZIYARET_HAREKETLERI] PRIMARY KEY CLUSTERED  ( [zyrt_Guid] ASC )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] ) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [02] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Temsilci_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [03] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Cari_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [04] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Cari_Kodu] ASC,[zyrt_Cari_Adres_No] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [05] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Temsilci_Kodu] ASC,[zyrt_Cari_Kodu] ASC,[zyrt_Cari_Adres_No] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [06] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Proje_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [07] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Sor_Mer_Kodu] ASC,[zyrt_Proje_Kodu] ASC,[zyrt_Tarihi] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [08] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Sor_Mer_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [09] ON [dbo].[_ZIYARET_HAREKETLERI] ([zyrt_Tarihi] ASC,[zyrt_Bolge_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
			using SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand();
			sqlCommand3.CommandText = commandText;
			sqlCommand3.ExecuteScalar();
		}
		catch (Exception ex2)
		{
			MessageBox.Show("Fora ziyaret hareketleri tablosu oluşturulamadı. ex : " + ex2.ToString());
		}
		try
		{
			string commandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[_TEMSILCI_GUNLUK_HAREKETLER]')) BEGIN CREATE TABLE [dbo].[_TEMSILCI_GUNLUK_HAREKETLER]([Guid] [uniqueidentifier] NOT NULL,[Temsilci_Kodu] [nvarchar](25) NOT NULL,[Tarih] [datetime] NOT NULL,[Baslama_Yapildi] [bit] NOT NULL,[Baslama_Saati] [datetime] NOT NULL,[Baslama_Kayit_Saati] [datetime] NOT NULL,[Baslama_Enlem] [float] NOT NULL,[Baslama_Boylam] [float] NOT NULL,[Baslama_Arac_Km] [int] NOT NULL,[Baslama_Mesaj] [nvarchar](127) NOT NULL,[Ogle_Arasi_Baslama_Saati] [datetime] NOT NULL,[Ogle_Arasi_Bitis_Saati] [datetime] NOT NULL,[Bitis_Yapildi] [bit] NOT NULL,[Bitis_Saati] [datetime] NOT NULL,[Bitis_Kayit_Saati] [datetime] NOT NULL,[Bitis_Enlem] [float] NOT NULL,[Bitis_Boylam] [float] NOT NULL,[Bitis_Arac_Km] [int] NOT NULL,[Bitis_Mesaj] [nvarchar](127) NOT NULL,[Create_Date] [datetime] NOT NULL,[Lastup_Date] [datetime] NOT NULL,CONSTRAINT [PK__TEMSILCI_GUNLUK_HAREKETLER] PRIMARY KEY CLUSTERED ([Guid] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] CREATE NONCLUSTERED INDEX [01] ON [dbo].[_TEMSILCI_GUNLUK_HAREKETLER] ([Temsilci_Kodu] ASC,[Tarih] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [02] ON [dbo].[_TEMSILCI_GUNLUK_HAREKETLER] ([Tarih] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] CREATE NONCLUSTERED INDEX [03] ON [dbo].[_TEMSILCI_GUNLUK_HAREKETLER] ([Temsilci_Kodu] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] SELECT 1 END ELSE SELECT 0";
			using SqlCommand sqlCommand4 = sqlDB.Connection.CreateCommand();
			sqlCommand4.CommandText = commandText;
			sqlCommand4.ExecuteScalar();
		}
		catch (Exception ex3)
		{
			MessageBox.Show("Fora temsilci günlük hareketler tablosu oluşturulamadı. ex : " + ex3.ToString());
		}
		sqlDB.ConnectionClose();
	}

	private void LisansYoneticisi_Load(object sender, EventArgs e)
	{
	}

	private void b_kurulumu_baslat_Click(object sender, EventArgs e)
	{
		label_islem_bilgi.Text = "";
		progressBar1.Maximum = 51;
		progressBar1.Value = 0;
		b_kurulumu_baslat.Enabled = false;
		bw_kurulum.RunWorkerAsync();
	}

	private void bw_kurulum_DoWork(object sender, DoWorkEventArgs e)
	{
		HataMesajlari = new List<string>();
		int num = 0;
		foreach (TabloV16 item in _tablolar)
		{
			bw_kurulum.ReportProgress(num);
			num++;
			string text = TabloKurulumuYap(item);
			if (text != "")
			{
				HataMesajlari.Add(text);
			}
		}
	}

	private void bw_kurulum_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		label_islem_bilgi.Text = _tablolar[e.ProgressPercentage].TabloAdi;
		progressBar1.Value = e.ProgressPercentage;
	}

	private void bw_kurulum_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		label_islem_bilgi.Text = "";
		progressBar1.Value = 0;
		foreach (string item in HataMesajlari)
		{
			MessageBox.Show(item);
		}
		MessageBox.Show("İşlem tamamlandı!");
	}

	private string TabloKurulumuYap(TabloV16 tablo)
	{
		string result = "";
		SqlDB sqlDB = new SqlDB();
		if (tablo.TabloAdi == "DOVIZ_KURLARI" || tablo.TabloAdi == "YEREL_BANKA_KODLARI" || tablo.TabloAdi == "BANKA_TCMB_KODLARI" || tablo.TabloAdi == "KUR_ISIMLERI")
		{
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroAnaDBName);
		}
		else
		{
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		}
		string text = "";
		try
		{
			text = "IF  EXISTS (SELECT * FROM sys.triggers WHERE name = N'" + tablo.TabloAdi + "_FORA_SYNC' AND type = 'TR') DROP TRIGGER dbo." + tablo.TabloAdi + "_FORA_SYNC  EXEC('  CREATE TRIGGER dbo." + tablo.TabloAdi + "_FORA_SYNC  ON " + tablo.TabloAdi + "  AFTER INSERT, UPDATE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = " + tablo.TabloID + "; DELETE " + _mikrouygulamabilgileri.MikroFirmaDBName + ".dbo._FORA_SYNC WHERE KayitGuid in (SELECT " + tablo.Fieldlar[0].Adi + " FROM inserted WITH (NOLOCK)) INSERT INTO " + _mikrouygulamabilgileri.MikroFirmaDBName + ".dbo._FORA_SYNC SELECT @TabloID, " + tablo.Fieldlar[0].Adi + " FROM inserted WITH (NOLOCK) END  ')  SELECT 1";
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			sqlCommand.CommandText = text;
			sqlCommand.ExecuteScalar();
		}
		catch (Exception ex)
		{
			result = tablo.TabloAdi + "_FORA_SYNC triggerı oluşturulamadı! Hata : " + ex.ToString();
		}
		try
		{
			text = "IF  EXISTS (SELECT * FROM sys.triggers WHERE name = N'" + tablo.TabloAdi + "_FORA_SYNC_DEL' AND type = 'TR') DROP TRIGGER dbo." + tablo.TabloAdi + "_FORA_SYNC_DEL  EXEC('  CREATE TRIGGER dbo." + tablo.TabloAdi + "_FORA_SYNC_DEL  ON " + tablo.TabloAdi + "  AFTER DELETE  AS   BEGIN  SET NOCOUNT ON;  DECLARE @TabloID as int; SET @TabloID = " + tablo.TabloID + "; DELETE " + _mikrouygulamabilgileri.MikroFirmaDBName + ".dbo._FORA_SYNC WHERE KayitGuid in (SELECT " + tablo.Fieldlar[0].Adi + " FROM deleted WITH (NOLOCK)) INSERT INTO " + _mikrouygulamabilgileri.MikroFirmaDBName + ".dbo._FORA_SYNC_DEL SELECT @TabloID, " + tablo.Fieldlar[0].Adi + " FROM deleted WITH (NOLOCK) END  ')  SELECT 1";
			using SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand();
			sqlCommand2.CommandText = text;
			sqlCommand2.ExecuteScalar();
		}
		catch (Exception ex2)
		{
			result = tablo.TabloAdi + "_FORA_SYNC_DEL triggerı oluşturulamadı! Hata : " + ex2.ToString();
		}
		try
		{
			text = "INSERT INTO _FORA_SYNC SELECT " + tablo.TabloID + "," + tablo.Fieldlar[0].Adi + " FROM " + tablo.TabloAdi + " WHERE " + tablo.Fieldlar[0].Adi + " NOT in (SELECT KayitGuid FROM _FORA_SYNC WHERE TabloID=" + tablo.TabloID + ") DELETE _FORA_SYNC WHERE TabloID = " + tablo.TabloID + " AND KayitGuid NOT IN(SELECT " + tablo.Fieldlar[0].Adi + " FROM " + tablo.TabloAdi + ")";
			if (tablo.TabloAdi == "DOVIZ_KURLARI" || tablo.TabloAdi == "YEREL_BANKA_KODLARI" || tablo.TabloAdi == "BANKA_TCMB_KODLARI" || tablo.TabloAdi == "KUR_ISIMLERI")
			{
				text = text.Replace("_FORA_SYNC", "[" + _mikrouygulamabilgileri.MikroFirmaDBName + "].[dbo].[_FORA_SYNC]");
			}
			using SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand();
			sqlCommand3.CommandText = text;
			sqlCommand3.CommandTimeout = 300;
			sqlCommand3.ExecuteScalar();
		}
		catch (Exception ex3)
		{
			result = "Fora Sync tablosu verileri doldurulamadı! Tablo : " + tablo.TabloAdi + " Hata : " + ex3.ToString();
		}
		sqlDB.ConnectionClose();
		return result;
	}

	private void button1_Click(object sender, EventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			string commandText = "DELETE _FORA_PARAMETRELER;INSERT INTO _FORA_PARAMETRELER SELECT NEWID(),ParametreProgram,ParametreUser,ParametreAnaGrubu,ParametreAltGrubu,ParametreID,ParametreAdi,ParametreDegeri FROM [" + tb_diger_veritabani.Text + "].[dbo].[_FORA_PARAMETRELER]";
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.ExecuteScalar();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
			MessageBox.Show("Parametreler kopyalanamadı!");
			return;
		}
		MessageBox.Show("İşlem tamamlandı!");
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
		this.b_kurulumu_baslat = new System.Windows.Forms.Button();
		this.label_islem_bilgi = new System.Windows.Forms.Label();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.label2 = new System.Windows.Forms.Label();
		this.bw_kurulum = new System.ComponentModel.BackgroundWorker();
		this.button1 = new System.Windows.Forms.Button();
		this.tb_diger_veritabani = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.b_kurulumu_baslat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.b_kurulumu_baslat.Location = new System.Drawing.Point(110, 67);
		this.b_kurulumu_baslat.Name = "b_kurulumu_baslat";
		this.b_kurulumu_baslat.Size = new System.Drawing.Size(177, 23);
		this.b_kurulumu_baslat.TabIndex = 15;
		this.b_kurulumu_baslat.Text = "KURULUMU BAŞLAT";
		this.b_kurulumu_baslat.UseVisualStyleBackColor = true;
		this.b_kurulumu_baslat.Click += new System.EventHandler(b_kurulumu_baslat_Click);
		this.label_islem_bilgi.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_islem_bilgi.ForeColor = System.Drawing.Color.Red;
		this.label_islem_bilgi.Location = new System.Drawing.Point(40, 133);
		this.label_islem_bilgi.Name = "label_islem_bilgi";
		this.label_islem_bilgi.Size = new System.Drawing.Size(296, 19);
		this.label_islem_bilgi.TabIndex = 71;
		this.label_islem_bilgi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.progressBar1.Location = new System.Drawing.Point(40, 100);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(296, 23);
		this.progressBar1.TabIndex = 70;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label2.Location = new System.Drawing.Point(39, 36);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(296, 18);
		this.label2.TabIndex = 72;
		this.label2.Text = "Verilerin yoğunluğuna göre bu işlem uzun sürebilir.";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.bw_kurulum.WorkerReportsProgress = true;
		this.bw_kurulum.WorkerSupportsCancellation = true;
		this.bw_kurulum.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_kurulum_DoWork);
		this.bw_kurulum.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_kurulum_ProgressChanged);
		this.bw_kurulum.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_kurulum_RunWorkerCompleted);
		this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.button1.Location = new System.Drawing.Point(110, 301);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(177, 27);
		this.button1.TabIndex = 73;
		this.button1.Text = "KOPYALA";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.tb_diger_veritabani.Location = new System.Drawing.Point(110, 275);
		this.tb_diger_veritabani.Name = "tb_diger_veritabani";
		this.tb_diger_veritabani.Size = new System.Drawing.Size(177, 20);
		this.tb_diger_veritabani.TabIndex = 74;
		this.tb_diger_veritabani.Text = "MikroDB_V15_";
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label1.Location = new System.Drawing.Point(110, 254);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(177, 18);
		this.label1.TabIndex = 75;
		this.label1.Text = "Diğer veritabanı";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label3.Location = new System.Drawing.Point(39, 9);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(296, 18);
		this.label3.TabIndex = 76;
		this.label3.Text = "KURULUM";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label4.Location = new System.Drawing.Point(37, 174);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(309, 41);
		this.label4.TabIndex = 77;
		this.label4.Text = "BAŞKA VERİTABANINDAN FORA PARAMETRELERİ KOPYALA";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label5.Location = new System.Drawing.Point(40, 215);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(296, 39);
		this.label5.TabIndex = 78;
		this.label5.Text = "Dikkat : Bu işlem sırasında mevcut veritabanındaki bütün parametreler silinecektir.";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(369, 354);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.tb_diger_veritabani);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label_islem_bilgi);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.b_kurulumu_baslat);
		base.Name = "ForaMikroVeritabaniKurulumu";
		this.Text = "Fora Mikro veritabani kurulumu";
		base.Load += new System.EventHandler(LisansYoneticisi_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
