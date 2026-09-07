using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Card;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro;
using Fora.Mikro.Data.Sql;

namespace Fora.App.Win.Mikro.KaliteKontrolYonetimi;

public class KayitHikayesi : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private DataTable _DataTable;

	private IContainer components;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem kapatToolStripMenuItem;

	private ToolStripMenuItem işlemlerToolStripMenuItem;

	private ToolStripMenuItem yenileToolStripMenuItem;

	private GridControl gridControl_kayitlar;

	private AdvBandedGridView advBandedGridView_master;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_Evrak_Tipi;

	private GridView gridView_EvrakTipi;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_NormalIade;

	private GridView gridView_NormalIade;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_TicaretTuru;

	private GridView gridView_TicaretTuru;

	private RepositoryItemDateEdit repositoryItemDateEdit_EvrakTarih;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Evrak_DovizCinsi;

	private RepositoryItemTextEdit repositoryItemTextEdit_Evrak_Kur;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_AcikKapali;

	private GridView repositoryItemGridLookUpEdit1View;

	private RepositoryItemTextEdit repositoryItemTextEdit_FaturaAciklama;

	private RepositoryItemTextEdit repositoryItemTextEdit_Aciklamalar;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Satir_VergiPntr;

	private RepositoryItemLookUpEdit repositoryItemLookUpEdit_Satir_IskontoSekli;

	private RepositoryItemGridLookUpEdit repositoryItemGridLookUpEdit_Satir_Cinsi;

	private GridView gridView_SatirCinsi;

	private AdvBandedGridView advBandedGridView_satirlar;

	private GridBand gridBand27;

	private BandedGridColumn gc_satir_SatirID;

	private BandedGridColumn gc_satir_EvrakID;

	private GridBand gridBand30;

	private BandedGridColumn gc_satir_Cinsi;

	private BandedGridColumn gc_satir_HesapKodu;

	private BandedGridColumn gc_satir_HesapAdi;

	private GridBand gridBand1;

	private BandedGridColumn gc_satir_Miktar;

	private BandedGridColumn gc_satir_Miktar2;

	private BandedGridColumn gc_satir_Birim;

	private BandedGridColumn gc_satir_VergiPntr;

	private BandedGridColumn gc_satir_BirimFiyat;

	private BandedGridColumn gc_satir_FiyatKaynagi;

	private GridBand gridBand29;

	private BandedGridColumn gc_satir_SatirAciklama;

	private GridBand gridBand2;

	private BandedGridColumn gc_satir_Iskonto_1_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Iskonto_1_YuzdeVeyaMiktar;

	private GridBand gridBand4;

	private BandedGridColumn gc_satir_Iskonto_2_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Iskonto_2_YuzdeVeyaMiktar;

	private GridBand gridBand19;

	private BandedGridColumn gc_satir_Iskonto_3_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Iskonto_3_YuzdeVeyaMiktar;

	private GridBand gridBand20;

	private BandedGridColumn gc_satir_Iskonto_4_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Iskonto_4_YuzdeVeyaMiktar;

	private GridBand gridBand21;

	private BandedGridColumn gc_satir_Iskonto_5_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Iskonto_5_YuzdeVeyaMiktar;

	private GridBand gridBand22;

	private BandedGridColumn gc_satir_Iskonto_6_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Iskonto_6_YuzdeVeyaMiktar;

	private GridBand gridBand23;

	private BandedGridColumn gc_satir_Masraf_1_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Masraf_1_YuzdeVeyaMiktar;

	private GridBand gridBand24;

	private BandedGridColumn gc_satir_Masraf_2_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Masraf_2_YuzdeVeyaMiktar;

	private GridBand gridBand25;

	private BandedGridColumn gc_satir_Masraf_3_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Masraf_3_YuzdeVeyaMiktar;

	private GridBand gridBand26;

	private BandedGridColumn gc_satir_Masraf_4_Uygulama_Sekli;

	private BandedGridColumn gc_satir_Masraf_4_YuzdeVeyaMiktar;

	private GridBand gridBand3;

	private BandedGridColumn gc_satir_ara_toplam;

	private BandedGridColumn gc_satir_iskonto_tutari;

	private BandedGridColumn gc_satir_masraf_tutari;

	private BandedGridColumn gc_satir_kdv_tutari;

	private BandedGridColumn gc_satir_yekun;

	private CardView cardView1;

	private BandedGridColumn bandedGridColumn1;

	private BandedGridColumn bandedGridColumn2;

	private BandedGridColumn bandedGridColumn3;

	private BandedGridColumn bandedGridColumn4;

	private BandedGridColumn bandedGridColumn8;

	private BandedGridColumn bandedGridColumn6;

	private BandedGridColumn bandedGridColumn7;

	private BandedGridColumn bandedGridColumn5;

	private BandedGridColumn bandedGridColumn11;

	private BandedGridColumn bandedGridColumn10;

	private BandedGridColumn bandedGridColumn9;

	private BandedGridColumn bandedGridColumn12;

	private BandedGridColumn bandedGridColumn13;

	private GridBand gridBand5;

	private GridBand gridBand8;

	private GridBand gridBand7;

	private BandedGridColumn bandedGridColumn14;

	private BandedGridColumn bandedGridColumn15;

	private GridBand gridBand6;

	private BandedGridColumn bandedGridColumn16;

	private BandedGridColumn bandedGridColumn17;

	private BandedGridColumn bandedGridColumn18;

	private BandedGridColumn bandedGridColumn19;

	private BandedGridColumn bandedGridColumn20;

	private GridBand gridBand9;

	private BandedGridColumn bandedGridColumn22;

	private BandedGridColumn bandedGridColumn23;

	private BandedGridColumn bandedGridColumn24;

	private BandedGridColumn bandedGridColumn25;

	private BandedGridColumn bandedGridColumn26;

	private GridBand gridBand10;

	private BandedGridColumn bandedGridColumn28;

	private BandedGridColumn bandedGridColumn29;

	private BandedGridColumn bandedGridColumn30;

	private BandedGridColumn bandedGridColumn31;

	private BandedGridColumn bandedGridColumn32;

	private SimpleButton sb_sil;

	private LabelControl labelControl1;

	private DateEdit dateEdit_baslangic;

	private LabelControl labelControl2;

	private DateEdit dateEdit_bitis;

	private SimpleButton sb_tarih_araligi_degistir;

	public KayitHikayesi(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		InitializeComponent();
		dateEdit_baslangic.DateTime = DateTime.Now.AddDays(-1 * _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiBaslangicTarihi")._GetInt);
		dateEdit_bitis.DateTime = DateTime.Now;
		ListeyiYenile();
	}

	public void ListeyiYenile()
	{
		DateTime dateTime = new DateTime(dateEdit_baslangic.DateTime.Year, dateEdit_baslangic.DateTime.Month, dateEdit_baslangic.DateTime.Day);
		DateTime dateTime2 = new DateTime(dateEdit_bitis.DateTime.Year, dateEdit_bitis.DateTime.Month, dateEdit_bitis.DateTime.Day).AddHours(23.0).AddMinutes(59.0).AddSeconds(59.0)
			.AddMilliseconds(99.0);
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string text = " WHERE (islem_tarih BETWEEN @baslangictarihi AND @bitistarihi) ";
		if (!_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiGorebilirButunKullanicilar")._GetBoolean)
		{
			text = " AND kullanici_adi='" + _mikrouygulamabilgileri.KullaniciAdi + "' ";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT RECno,sth_RECid_RECno,sth_stok_kod,(SELECT TOP 1 sto_isim FROM STOKLAR WITH(NOLOCK) WHERE sto_kod=sth_stok_kod) AS sto_isim,sth_parti_kodu,sth_aciklama,sth_miktar,sth_tarih,kullanici_adi,islem_tarih,islem_yapildi,islem_aciklama,kaynak_depo_no,(SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=kaynak_depo_no) AS kaynak_depo_adi,onay_depo_no,(SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=onay_depo_no) AS onay_depo_adi,onay_miktar,ret_depo_no,(SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=ret_depo_no) AS ret_depo_adi,ret_miktar,ret_aciklama,ret_evrak_seri,ret_evrak_sira,ret_evrak_sira,hurda_depo_no,(SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=hurda_depo_no) AS hurda_depo_adi,hurda_miktar,hurda_aciklama,hurda_evrak_seri,hurda_evrak_sira,hurda_evrak_sira,iade_depo_no,(SELECT TOP 1 dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=iade_depo_no) AS iade_depo_adi,iade_miktar,iade_aciklama,iade_evrak_seri,iade_evrak_sira,iade_evrak_sira FROM _FORA_KALITE_KONTROL WITH(NOLOCK) " + text + " ORDER BY islem_tarih DESC", sqlDB.Connection);
		sqlCommand.CommandType = CommandType.Text;
		sqlCommand.Parameters.AddWithValue("@baslangictarihi", dateTime);
		sqlCommand.Parameters.AddWithValue("@bitistarihi", dateTime2);
		SqlDataAdapter obj = new SqlDataAdapter
		{
			SelectCommand = sqlCommand
		};
		_DataTable = new DataTable();
		obj.Fill(_DataTable);
		sqlDB.ConnectionClose();
		gridControl_kayitlar.DataSource = _DataTable;
	}

	private void sb_sil_Click(object sender, EventArgs e)
	{
		if (!_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilir")._GetBoolean)
		{
			MessageBox.Show("İşlem silme yetkiniz bulunmamaktadır.");
		}
		else
		{
			if (MessageBox.Show("Seçili işlemleri silmek istediğinize emin misiniz?", "ONAY", MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				return;
			}
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
			int[] selectedRows = advBandedGridView_master.GetSelectedRows();
			foreach (int num in selectedRows)
			{
				if (num < 0)
				{
					continue;
				}
				int dataSourceRowIndex = advBandedGridView_master.GetDataSourceRowIndex(num);
				DataRow dataRow = _DataTable.Rows[dataSourceRowIndex];
				int num2 = (int)dataRow[0];
				int num3 = (int)dataRow[1];
				string sth_stok_kod = (string)dataRow[2];
				_ = (string)dataRow[3];
				string sth_parti_kodu = (string)dataRow[4];
				_ = (double)dataRow[6];
				_ = (DateTime)dataRow[7];
				string text = (string)dataRow[8];
				DateTime dateTime = (DateTime)dataRow[9];
				bool flag = (bool)dataRow[10];
				int num4 = (int)dataRow[12];
				_ = (int)dataRow[14];
				_ = (double)dataRow[16];
				_ = (int)dataRow[17];
				double miktar = (double)dataRow[19];
				string evrak_seri = (string)dataRow[21];
				int num5 = (int)dataRow[22];
				_ = (int)dataRow[24];
				double miktar2 = (double)dataRow[26];
				string evrak_seri2 = (string)dataRow[28];
				int num6 = (int)dataRow[29];
				_ = (int)dataRow[31];
				double miktar3 = (double)dataRow[33];
				string evrak_seri3 = (string)dataRow[35];
				int num7 = (int)dataRow[36];
				bool flag2 = true;
				if (!_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiKayitHikayesiIslemSilebilirButunKullanicilar")._GetBoolean && text != _mikrouygulamabilgileri.KullaniciAdi)
				{
					flag2 = false;
					MessageBox.Show(num2 + " kayıt numaralı işlem " + text + " kullanıcısı tarafından yapıldığı için silme yetkiniz bulunmamaktadır");
				}
				if (!flag2)
				{
					continue;
				}
				bool flag3 = false;
				string commandText = "SELECT TOP 1RECno,kullanici_adi FROM _FORA_KALITE_KONTROL WITH(NOLOCK)  WHERE sth_RECid_RECno=@sth_RECid_RECno AND islem_tarih>@islem_tarih ORDER BY islem_tarih ASC";
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					sqlCommand.CommandText = commandText;
					sqlCommand.Parameters.AddWithValue("@sth_RECid_RECno", num3);
					sqlCommand.Parameters.AddWithValue("@islem_tarih", dateTime);
					SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
					if (sqlDataReader.HasRows)
					{
						sqlDataReader.Read();
						flag3 = true;
						MessageBox.Show(num2 + " kayıt numaralı işlem silinemedi. " + (string)sqlDataReader[1] + " kullanıcısı " + (int)sqlDataReader[0] + " kayıt numarası ile işlem yapmış");
					}
					sqlDataReader.Close();
				}
				if (flag3)
				{
					continue;
				}
				bool flag4 = true;
				if (flag)
				{
					if (num5 != 0)
					{
						DepolarArasiSevkSatirSil(sqlDB, evrak_seri, num5, sth_stok_kod, sth_parti_kodu, miktar);
					}
					if (num6 != 0)
					{
						DepolarArasiSevkSatirSil(sqlDB, evrak_seri2, num6, sth_stok_kod, sth_parti_kodu, miktar2);
					}
					if (num7 != 0)
					{
						DepolarArasiSevkSatirSil(sqlDB, evrak_seri3, num7, sth_stok_kod, sth_parti_kodu, miktar3);
					}
					using SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand();
					string commandText2 = "UPDATE STOK_HAREKETLERI SET sth_lastup_user=@sth_lastup_user,sth_lastup_date=getdate(), sth_giris_depo_no=@sth_giris_depo_no WHERE sth_RECid_RECno=@sth_RECid_RECno";
					sqlCommand2.CommandText = commandText2;
					sqlCommand2.Parameters.AddWithValue("@sth_giris_depo_no", num4);
					sqlCommand2.Parameters.AddWithValue("@sth_lastup_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
					sqlCommand2.Parameters.AddWithValue("@sth_RECid_RECno", num3);
					sqlCommand2.ExecuteNonQuery();
				}
				if (flag4)
				{
					using SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand();
					string commandText3 = "DELETE _FORA_KALITE_KONTROL WHERE RECno=@RECno";
					sqlCommand3.CommandText = commandText3;
					sqlCommand3.Parameters.AddWithValue("@RECno", num2);
					sqlCommand3.ExecuteNonQuery();
				}
			}
			sqlDB.ConnectionClose();
			ListeyiYenile();
		}
	}

	private void DepolarArasiSevkSatirSil(SqlDB Db, string evrak_seri, int evrak_sira, string sth_stok_kod, string sth_parti_kodu, double miktar)
	{
		int num = 0;
		List<int> list = new List<int>();
		string commandText = "SELECT sth_RECno,sth_stok_kod,sth_miktar,sth_parti_kodu FROM STOK_HAREKETLERI WITH(NOLOCK)  WHERE sth_evraktip=" + 2 + " AND sth_tip=" + 2 + " AND sth_evrakno_seri=@sth_evrakno_seri AND sth_evrakno_sira=@sth_evrakno_sira  ORDER BY sth_satirno";
		using (SqlCommand sqlCommand = Db.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@sth_evrakno_seri", evrak_seri);
			sqlCommand.Parameters.AddWithValue("@sth_evrakno_sira", evrak_sira);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				if (sth_stok_kod == (string)sqlDataReader[1] && miktar == (double)sqlDataReader[2] && sth_parti_kodu == (string)sqlDataReader[3] && num == 0)
				{
					num = (int)sqlDataReader[0];
				}
				else
				{
					list.Add((int)sqlDataReader[0]);
				}
			}
			sqlDataReader.Close();
		}
		if (num <= 0)
		{
			return;
		}
		using (SqlCommand sqlCommand2 = Db.Connection.CreateCommand())
		{
			string commandText2 = "DELETE STOK_HAREKETLERI WHERE sth_RECno=@sth_RECno";
			sqlCommand2.CommandText = commandText2;
			sqlCommand2.Parameters.AddWithValue("@sth_RECno", num);
			sqlCommand2.ExecuteNonQuery();
		}
		int num2 = 1000;
		foreach (int item in list)
		{
			using (SqlCommand sqlCommand3 = Db.Connection.CreateCommand())
			{
				string commandText3 = "UPDATE STOK_HAREKETLERI SET sth_lastup_user=@sth_lastup_user,sth_lastup_date=getdate(), sth_satirno=@sth_satirno WHERE sth_RECno=@sth_RECno";
				sqlCommand3.CommandText = commandText3;
				sqlCommand3.Parameters.AddWithValue("@sth_RECno", item);
				sqlCommand3.Parameters.AddWithValue("@sth_lastup_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
				sqlCommand3.Parameters.AddWithValue("@sth_satirno", num2);
				sqlCommand3.ExecuteNonQuery();
			}
			num2++;
		}
		num2 = 0;
		foreach (int item2 in list)
		{
			using (SqlCommand sqlCommand4 = Db.Connection.CreateCommand())
			{
				string commandText4 = "UPDATE STOK_HAREKETLERI SET sth_lastup_user=@sth_lastup_user,sth_lastup_date=getdate(), sth_satirno=@sth_satirno WHERE sth_RECno=@sth_RECno";
				sqlCommand4.CommandText = commandText4;
				sqlCommand4.Parameters.AddWithValue("@sth_RECno", item2);
				sqlCommand4.Parameters.AddWithValue("@sth_lastup_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
				sqlCommand4.Parameters.AddWithValue("@sth_satirno", num2);
				sqlCommand4.ExecuteNonQuery();
			}
			num2++;
		}
	}

	private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ListeyiYenile();
	}

	private void sb_tarih_araligi_degistir_Click(object sender, EventArgs e)
	{
		ListeyiYenile();
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
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.işlemlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yenileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.gridControl_kayitlar = new DevExpress.XtraGrid.GridControl();
		this.advBandedGridView_master = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.bandedGridColumn3 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn4 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn5 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand8 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.bandedGridColumn2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn8 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn6 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn7 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand7 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.bandedGridColumn1 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn11 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn10 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn9 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn12 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn13 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn14 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn15 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand6 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.bandedGridColumn16 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn17 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn18 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn19 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn20 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand9 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.bandedGridColumn22 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn23 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn24 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn25 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn26 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand10 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.bandedGridColumn28 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn29 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn30 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn31 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.bandedGridColumn32 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.repositoryItemLookUpEdit_Satir_VergiPntr = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.repositoryItemLookUpEdit_Satir_IskontoSekli = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.repositoryItemGridLookUpEdit_Satir_Cinsi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_SatirCinsi = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemGridLookUpEdit_Evrak_Tipi = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_EvrakTipi = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemGridLookUpEdit_NormalIade = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_NormalIade = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemDateEdit_EvrakTarih = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
		this.repositoryItemGridLookUpEdit_AcikKapali = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.repositoryItemGridLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemTextEdit_Evrak_Kur = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.repositoryItemGridLookUpEdit_TicaretTuru = new DevExpress.XtraEditors.Repository.RepositoryItemGridLookUpEdit();
		this.gridView_TicaretTuru = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.repositoryItemTextEdit_FaturaAciklama = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.repositoryItemTextEdit_Aciklamalar = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.advBandedGridView_satirlar = new DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView();
		this.gridBand27 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_SatirID = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_EvrakID = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand30 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Cinsi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_HesapKodu = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_HesapAdi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand1 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Miktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Miktar2 = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Birim = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_VergiPntr = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_BirimFiyat = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_FiyatKaynagi = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand29 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_SatirAciklama = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Iskonto_1_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Iskonto_1_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand4 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Iskonto_2_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Iskonto_2_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand19 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Iskonto_3_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Iskonto_3_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand20 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Iskonto_4_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Iskonto_4_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand21 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Iskonto_5_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Iskonto_5_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand22 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Iskonto_6_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Iskonto_6_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand23 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Masraf_1_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Masraf_1_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand24 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Masraf_2_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Masraf_2_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand25 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Masraf_3_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Masraf_3_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand26 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_Masraf_4_Uygulama_Sekli = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_Masraf_4_YuzdeVeyaMiktar = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
		this.gc_satir_ara_toplam = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_iskonto_tutari = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_masraf_tutari = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_kdv_tutari = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.gc_satir_yekun = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
		this.cardView1 = new DevExpress.XtraGrid.Views.Card.CardView();
		this.sb_sil = new DevExpress.XtraEditors.SimpleButton();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.dateEdit_baslangic = new DevExpress.XtraEditors.DateEdit();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.dateEdit_bitis = new DevExpress.XtraEditors.DateEdit();
		this.sb_tarih_araligi_degistir = new DevExpress.XtraEditors.SimpleButton();
		this.menuStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.gridControl_kayitlar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_VergiPntr).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_IskontoSekli).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Cinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_SatirCinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Evrak_Kur).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_satirlar).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cardView1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic.Properties.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis.Properties.CalendarTimeProperties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis.Properties).BeginInit();
		base.SuspendLayout();
		this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.dosyaToolStripMenuItem, this.işlemlerToolStripMenuItem });
		this.menuStrip1.Location = new System.Drawing.Point(0, 0);
		this.menuStrip1.Name = "menuStrip1";
		this.menuStrip1.Size = new System.Drawing.Size(952, 24);
		this.menuStrip1.TabIndex = 14;
		this.menuStrip1.Text = "menuStrip1";
		this.dosyaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.kapatToolStripMenuItem });
		this.dosyaToolStripMenuItem.Name = "dosyaToolStripMenuItem";
		this.dosyaToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
		this.dosyaToolStripMenuItem.Text = "Dosya";
		this.kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
		this.kapatToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
		this.kapatToolStripMenuItem.Text = "Kapat";
		this.kapatToolStripMenuItem.Click += new System.EventHandler(kapatToolStripMenuItem_Click);
		this.işlemlerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.yenileToolStripMenuItem });
		this.işlemlerToolStripMenuItem.Name = "işlemlerToolStripMenuItem";
		this.işlemlerToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
		this.işlemlerToolStripMenuItem.Text = "İşlemler";
		this.yenileToolStripMenuItem.Name = "yenileToolStripMenuItem";
		this.yenileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
		this.yenileToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
		this.yenileToolStripMenuItem.Text = "Yenile";
		this.yenileToolStripMenuItem.Click += new System.EventHandler(yenileToolStripMenuItem_Click);
		this.gridControl_kayitlar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.gridControl_kayitlar.Location = new System.Drawing.Point(0, 24);
		this.gridControl_kayitlar.MainView = this.advBandedGridView_master;
		this.gridControl_kayitlar.Name = "gridControl_kayitlar";
		this.gridControl_kayitlar.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[12]
		{
			this.repositoryItemLookUpEdit_Satir_VergiPntr, this.repositoryItemLookUpEdit_Satir_IskontoSekli, this.repositoryItemGridLookUpEdit_Satir_Cinsi, this.repositoryItemGridLookUpEdit_Evrak_Tipi, this.repositoryItemGridLookUpEdit_NormalIade, this.repositoryItemDateEdit_EvrakTarih, this.repositoryItemLookUpEdit_Evrak_DovizCinsi, this.repositoryItemGridLookUpEdit_AcikKapali, this.repositoryItemTextEdit_Evrak_Kur, this.repositoryItemGridLookUpEdit_TicaretTuru,
			this.repositoryItemTextEdit_FaturaAciklama, this.repositoryItemTextEdit_Aciklamalar
		});
		this.gridControl_kayitlar.Size = new System.Drawing.Size(952, 357);
		this.gridControl_kayitlar.TabIndex = 15;
		this.gridControl_kayitlar.UseEmbeddedNavigator = true;
		this.gridControl_kayitlar.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[3] { this.advBandedGridView_master, this.advBandedGridView_satirlar, this.cardView1 });
		this.advBandedGridView_master.Appearance.BandPanel.BackColor = System.Drawing.Color.Red;
		this.advBandedGridView_master.Appearance.BandPanel.BackColor2 = System.Drawing.Color.FromArgb(255, 128, 128);
		this.advBandedGridView_master.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_master.Appearance.BandPanel.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseBackColor = true;
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseFont = true;
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseForeColor = true;
		this.advBandedGridView_master.Appearance.BandPanel.Options.UseTextOptions = true;
		this.advBandedGridView_master.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.advBandedGridView_master.Appearance.BandPanelBackground.BackColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView_master.Appearance.BandPanelBackground.BackColor2 = System.Drawing.Color.FromArgb(255, 192, 128);
		this.advBandedGridView_master.Appearance.BandPanelBackground.Options.UseBackColor = true;
		this.advBandedGridView_master.Appearance.Empty.Font = new System.Drawing.Font("Tahoma", 11f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_master.Appearance.Empty.Options.UseFont = true;
		this.advBandedGridView_master.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(192, 192, 0);
		this.advBandedGridView_master.Appearance.EvenRow.Options.UseBackColor = true;
		this.advBandedGridView_master.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[6] { this.gridBand5, this.gridBand8, this.gridBand7, this.gridBand6, this.gridBand9, this.gridBand10 });
		this.advBandedGridView_master.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[30]
		{
			this.bandedGridColumn1, this.bandedGridColumn2, this.bandedGridColumn3, this.bandedGridColumn4, this.bandedGridColumn5, this.bandedGridColumn6, this.bandedGridColumn7, this.bandedGridColumn8, this.bandedGridColumn9, this.bandedGridColumn10,
			this.bandedGridColumn11, this.bandedGridColumn12, this.bandedGridColumn13, this.bandedGridColumn14, this.bandedGridColumn15, this.bandedGridColumn16, this.bandedGridColumn17, this.bandedGridColumn18, this.bandedGridColumn19, this.bandedGridColumn20,
			this.bandedGridColumn22, this.bandedGridColumn23, this.bandedGridColumn24, this.bandedGridColumn25, this.bandedGridColumn26, this.bandedGridColumn28, this.bandedGridColumn29, this.bandedGridColumn30, this.bandedGridColumn31, this.bandedGridColumn32
		});
		this.advBandedGridView_master.DetailVerticalIndent = 20;
		this.advBandedGridView_master.GridControl = this.gridControl_kayitlar;
		this.advBandedGridView_master.Name = "advBandedGridView_master";
		this.advBandedGridView_master.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_master.OptionsDetail.AllowExpandEmptyDetails = true;
		this.advBandedGridView_master.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView_master.OptionsSelection.MultiSelect = true;
		this.gridBand5.Caption = "Stok bilgileri";
		this.gridBand5.Columns.Add(this.bandedGridColumn3);
		this.gridBand5.Columns.Add(this.bandedGridColumn4);
		this.gridBand5.Columns.Add(this.bandedGridColumn5);
		this.gridBand5.Name = "gridBand5";
		this.gridBand5.VisibleIndex = 0;
		this.gridBand5.Width = 225;
		this.bandedGridColumn3.Caption = "Stok kodu";
		this.bandedGridColumn3.FieldName = "sth_stok_kod";
		this.bandedGridColumn3.Name = "bandedGridColumn3";
		this.bandedGridColumn3.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn3.Visible = true;
		this.bandedGridColumn4.Caption = "Stok ismi";
		this.bandedGridColumn4.FieldName = "sto_isim";
		this.bandedGridColumn4.Name = "bandedGridColumn4";
		this.bandedGridColumn4.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn4.Visible = true;
		this.bandedGridColumn5.Caption = "Parti kodu";
		this.bandedGridColumn5.FieldName = "sth_parti_kodu";
		this.bandedGridColumn5.Name = "bandedGridColumn5";
		this.bandedGridColumn5.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn5.Visible = true;
		this.gridBand8.Caption = "Stok hareketi bilgileri";
		this.gridBand8.Columns.Add(this.bandedGridColumn2);
		this.gridBand8.Columns.Add(this.bandedGridColumn8);
		this.gridBand8.Columns.Add(this.bandedGridColumn6);
		this.gridBand8.Columns.Add(this.bandedGridColumn7);
		this.gridBand8.Name = "gridBand8";
		this.gridBand8.VisibleIndex = 1;
		this.gridBand8.Width = 352;
		this.bandedGridColumn2.Caption = "Sth kayıt no";
		this.bandedGridColumn2.FieldName = "sth_RECid_RECno";
		this.bandedGridColumn2.Name = "bandedGridColumn2";
		this.bandedGridColumn2.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn2.Visible = true;
		this.bandedGridColumn8.Caption = "Sth tarih";
		this.bandedGridColumn8.FieldName = "sth_tarih";
		this.bandedGridColumn8.Name = "bandedGridColumn8";
		this.bandedGridColumn8.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn8.Visible = true;
		this.bandedGridColumn8.Width = 73;
		this.bandedGridColumn6.Caption = "Sth açıklama";
		this.bandedGridColumn6.FieldName = "sth_aciklama";
		this.bandedGridColumn6.Name = "bandedGridColumn6";
		this.bandedGridColumn6.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn6.Visible = true;
		this.bandedGridColumn6.Width = 139;
		this.bandedGridColumn7.Caption = "Sth miktar";
		this.bandedGridColumn7.FieldName = "sth_miktar";
		this.bandedGridColumn7.Name = "bandedGridColumn7";
		this.bandedGridColumn7.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn7.Visible = true;
		this.bandedGridColumn7.Width = 65;
		this.gridBand7.Caption = "İşlem bilgileri";
		this.gridBand7.Columns.Add(this.bandedGridColumn1);
		this.gridBand7.Columns.Add(this.bandedGridColumn11);
		this.gridBand7.Columns.Add(this.bandedGridColumn10);
		this.gridBand7.Columns.Add(this.bandedGridColumn9);
		this.gridBand7.Columns.Add(this.bandedGridColumn12);
		this.gridBand7.Columns.Add(this.bandedGridColumn13);
		this.gridBand7.Columns.Add(this.bandedGridColumn14);
		this.gridBand7.Columns.Add(this.bandedGridColumn15);
		this.gridBand7.Name = "gridBand7";
		this.gridBand7.VisibleIndex = 2;
		this.gridBand7.Width = 591;
		this.bandedGridColumn1.Caption = "Kayıt no";
		this.bandedGridColumn1.FieldName = "RECno";
		this.bandedGridColumn1.Name = "bandedGridColumn1";
		this.bandedGridColumn1.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn1.Visible = true;
		this.bandedGridColumn11.Caption = "İşlem yapıldı";
		this.bandedGridColumn11.FieldName = "islem_yapildi";
		this.bandedGridColumn11.Name = "bandedGridColumn11";
		this.bandedGridColumn11.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn11.Visible = true;
		this.bandedGridColumn11.Width = 66;
		this.bandedGridColumn10.Caption = "İşlem tarihi";
		this.bandedGridColumn10.DisplayFormat.FormatString = "d";
		this.bandedGridColumn10.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
		this.bandedGridColumn10.FieldName = "islem_tarih";
		this.bandedGridColumn10.Name = "bandedGridColumn10";
		this.bandedGridColumn10.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn10.Visible = true;
		this.bandedGridColumn9.Caption = "Kullanıcı adı";
		this.bandedGridColumn9.FieldName = "kullanici_adi";
		this.bandedGridColumn9.Name = "bandedGridColumn9";
		this.bandedGridColumn9.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn9.Visible = true;
		this.bandedGridColumn12.Caption = "İşlem açıklama";
		this.bandedGridColumn12.FieldName = "islem_aciklama";
		this.bandedGridColumn12.Name = "bandedGridColumn12";
		this.bandedGridColumn12.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn12.Visible = true;
		this.bandedGridColumn13.Caption = "Kaynak depo adı";
		this.bandedGridColumn13.FieldName = "kaynak_depo_adi";
		this.bandedGridColumn13.Name = "bandedGridColumn13";
		this.bandedGridColumn13.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn13.Visible = true;
		this.bandedGridColumn14.Caption = "Onay depo adı";
		this.bandedGridColumn14.FieldName = "onay_depo_adi";
		this.bandedGridColumn14.Name = "bandedGridColumn14";
		this.bandedGridColumn14.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn14.Visible = true;
		this.bandedGridColumn15.Caption = "Onay miktarı";
		this.bandedGridColumn15.FieldName = "onay_miktar";
		this.bandedGridColumn15.Name = "bandedGridColumn15";
		this.bandedGridColumn15.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn15.Visible = true;
		this.gridBand6.Caption = "Ret bilgileri";
		this.gridBand6.Columns.Add(this.bandedGridColumn16);
		this.gridBand6.Columns.Add(this.bandedGridColumn17);
		this.gridBand6.Columns.Add(this.bandedGridColumn18);
		this.gridBand6.Columns.Add(this.bandedGridColumn19);
		this.gridBand6.Columns.Add(this.bandedGridColumn20);
		this.gridBand6.Name = "gridBand6";
		this.gridBand6.VisibleIndex = 3;
		this.gridBand6.Width = 375;
		this.bandedGridColumn16.Caption = "Ret depo adı";
		this.bandedGridColumn16.FieldName = "ret_depo_adi";
		this.bandedGridColumn16.Name = "bandedGridColumn16";
		this.bandedGridColumn16.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn16.Visible = true;
		this.bandedGridColumn17.Caption = "Ret miktarı";
		this.bandedGridColumn17.FieldName = "ret_miktar";
		this.bandedGridColumn17.Name = "bandedGridColumn17";
		this.bandedGridColumn17.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn17.Visible = true;
		this.bandedGridColumn18.Caption = "Ret açıklama";
		this.bandedGridColumn18.FieldName = "ret_aciklama";
		this.bandedGridColumn18.Name = "bandedGridColumn18";
		this.bandedGridColumn18.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn18.Visible = true;
		this.bandedGridColumn19.Caption = "Ret evrak seri";
		this.bandedGridColumn19.FieldName = "ret_evrak_seri";
		this.bandedGridColumn19.Name = "bandedGridColumn19";
		this.bandedGridColumn19.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn19.Visible = true;
		this.bandedGridColumn20.Caption = "Ret evrak sıra";
		this.bandedGridColumn20.FieldName = "ret_evrak_sira";
		this.bandedGridColumn20.Name = "bandedGridColumn20";
		this.bandedGridColumn20.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn20.Visible = true;
		this.gridBand9.Caption = "Hurda bilgileri";
		this.gridBand9.Columns.Add(this.bandedGridColumn22);
		this.gridBand9.Columns.Add(this.bandedGridColumn23);
		this.gridBand9.Columns.Add(this.bandedGridColumn24);
		this.gridBand9.Columns.Add(this.bandedGridColumn25);
		this.gridBand9.Columns.Add(this.bandedGridColumn26);
		this.gridBand9.Name = "gridBand9";
		this.gridBand9.VisibleIndex = 4;
		this.gridBand9.Width = 480;
		this.bandedGridColumn22.Caption = "Hurda depo adı";
		this.bandedGridColumn22.FieldName = "hurda_depo_adi";
		this.bandedGridColumn22.Name = "bandedGridColumn22";
		this.bandedGridColumn22.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn22.Visible = true;
		this.bandedGridColumn22.Width = 85;
		this.bandedGridColumn23.Caption = "Hurda miktarı";
		this.bandedGridColumn23.FieldName = "hurda_miktar";
		this.bandedGridColumn23.Name = "bandedGridColumn23";
		this.bandedGridColumn23.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn23.Visible = true;
		this.bandedGridColumn23.Width = 73;
		this.bandedGridColumn24.Caption = "Hurda açıklama";
		this.bandedGridColumn24.FieldName = "hurda_aciklama";
		this.bandedGridColumn24.Name = "bandedGridColumn24";
		this.bandedGridColumn24.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn24.Visible = true;
		this.bandedGridColumn24.Width = 147;
		this.bandedGridColumn25.Caption = "Hurda evrak seri";
		this.bandedGridColumn25.FieldName = "hurda_evrak_seri";
		this.bandedGridColumn25.Name = "bandedGridColumn25";
		this.bandedGridColumn25.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn25.Visible = true;
		this.bandedGridColumn25.Width = 88;
		this.bandedGridColumn26.Caption = "Hurda evrak sıra";
		this.bandedGridColumn26.FieldName = "hurda_evrak_sira";
		this.bandedGridColumn26.Name = "bandedGridColumn26";
		this.bandedGridColumn26.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn26.Visible = true;
		this.bandedGridColumn26.Width = 87;
		this.gridBand10.Caption = "İade bilgileri";
		this.gridBand10.Columns.Add(this.bandedGridColumn28);
		this.gridBand10.Columns.Add(this.bandedGridColumn29);
		this.gridBand10.Columns.Add(this.bandedGridColumn30);
		this.gridBand10.Columns.Add(this.bandedGridColumn31);
		this.gridBand10.Columns.Add(this.bandedGridColumn32);
		this.gridBand10.Name = "gridBand10";
		this.gridBand10.VisibleIndex = 5;
		this.gridBand10.Width = 466;
		this.bandedGridColumn28.Caption = "İade depo adı";
		this.bandedGridColumn28.FieldName = "iade_depo_adi";
		this.bandedGridColumn28.Name = "bandedGridColumn28";
		this.bandedGridColumn28.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn28.Visible = true;
		this.bandedGridColumn29.Caption = "İade miktarı";
		this.bandedGridColumn29.FieldName = "iade_miktar";
		this.bandedGridColumn29.Name = "bandedGridColumn29";
		this.bandedGridColumn29.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn29.Visible = true;
		this.bandedGridColumn29.Width = 65;
		this.bandedGridColumn30.Caption = "İade açıklama";
		this.bandedGridColumn30.FieldName = "iade_aciklama";
		this.bandedGridColumn30.Name = "bandedGridColumn30";
		this.bandedGridColumn30.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn30.Visible = true;
		this.bandedGridColumn30.Width = 166;
		this.bandedGridColumn31.Caption = "İade evrak seri";
		this.bandedGridColumn31.FieldName = "iade_evrak_seri";
		this.bandedGridColumn31.Name = "bandedGridColumn31";
		this.bandedGridColumn31.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn31.Visible = true;
		this.bandedGridColumn31.Width = 80;
		this.bandedGridColumn32.Caption = "İade evrak sıra";
		this.bandedGridColumn32.FieldName = "iade_evrak_sira";
		this.bandedGridColumn32.Name = "bandedGridColumn32";
		this.bandedGridColumn32.OptionsColumn.AllowEdit = false;
		this.bandedGridColumn32.Visible = true;
		this.bandedGridColumn32.Width = 80;
		this.repositoryItemLookUpEdit_Satir_VergiPntr.AutoHeight = false;
		this.repositoryItemLookUpEdit_Satir_VergiPntr.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Satir_VergiPntr.Name = "repositoryItemLookUpEdit_Satir_VergiPntr";
		this.repositoryItemLookUpEdit_Satir_IskontoSekli.AutoHeight = false;
		this.repositoryItemLookUpEdit_Satir_IskontoSekli.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Satir_IskontoSekli.Name = "repositoryItemLookUpEdit_Satir_IskontoSekli";
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.Name = "repositoryItemGridLookUpEdit_Satir_Cinsi";
		this.repositoryItemGridLookUpEdit_Satir_Cinsi.View = this.gridView_SatirCinsi;
		this.gridView_SatirCinsi.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_SatirCinsi.Name = "gridView_SatirCinsi";
		this.gridView_SatirCinsi.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_SatirCinsi.OptionsView.ShowGroupPanel = false;
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.Name = "repositoryItemGridLookUpEdit_Evrak_Tipi";
		this.repositoryItemGridLookUpEdit_Evrak_Tipi.View = this.gridView_EvrakTipi;
		this.gridView_EvrakTipi.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_EvrakTipi.Name = "gridView_EvrakTipi";
		this.gridView_EvrakTipi.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_EvrakTipi.OptionsView.ShowGroupPanel = false;
		this.repositoryItemGridLookUpEdit_NormalIade.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_NormalIade.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_NormalIade.Name = "repositoryItemGridLookUpEdit_NormalIade";
		this.repositoryItemGridLookUpEdit_NormalIade.View = this.gridView_NormalIade;
		this.gridView_NormalIade.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_NormalIade.Name = "gridView_NormalIade";
		this.gridView_NormalIade.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_NormalIade.OptionsView.ShowGroupPanel = false;
		this.repositoryItemDateEdit_EvrakTarih.AutoHeight = false;
		this.repositoryItemDateEdit_EvrakTarih.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton()
		});
		this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F4);
		this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties.PopupBorderStyle = DevExpress.XtraEditors.Controls.PopupBorderStyles.Default;
		this.repositoryItemDateEdit_EvrakTarih.Name = "repositoryItemDateEdit_EvrakTarih";
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.AutoHeight = false;
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemLookUpEdit_Evrak_DovizCinsi.Name = "repositoryItemLookUpEdit_Evrak_DovizCinsi";
		this.repositoryItemGridLookUpEdit_AcikKapali.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_AcikKapali.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_AcikKapali.Name = "repositoryItemGridLookUpEdit_AcikKapali";
		this.repositoryItemGridLookUpEdit_AcikKapali.View = this.repositoryItemGridLookUpEdit1View;
		this.repositoryItemGridLookUpEdit1View.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.repositoryItemGridLookUpEdit1View.Name = "repositoryItemGridLookUpEdit1View";
		this.repositoryItemGridLookUpEdit1View.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.repositoryItemGridLookUpEdit1View.OptionsView.ShowGroupPanel = false;
		this.repositoryItemTextEdit_Evrak_Kur.AutoHeight = false;
		this.repositoryItemTextEdit_Evrak_Kur.Name = "repositoryItemTextEdit_Evrak_Kur";
		this.repositoryItemTextEdit_Evrak_Kur.ValidateOnEnterKey = true;
		this.repositoryItemGridLookUpEdit_TicaretTuru.AutoHeight = false;
		this.repositoryItemGridLookUpEdit_TicaretTuru.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.repositoryItemGridLookUpEdit_TicaretTuru.Name = "repositoryItemGridLookUpEdit_TicaretTuru";
		this.repositoryItemGridLookUpEdit_TicaretTuru.View = this.gridView_TicaretTuru;
		this.gridView_TicaretTuru.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
		this.gridView_TicaretTuru.Name = "gridView_TicaretTuru";
		this.gridView_TicaretTuru.OptionsSelection.EnableAppearanceFocusedCell = false;
		this.gridView_TicaretTuru.OptionsView.ShowGroupPanel = false;
		this.repositoryItemTextEdit_FaturaAciklama.AutoHeight = false;
		this.repositoryItemTextEdit_FaturaAciklama.MaxLength = 40;
		this.repositoryItemTextEdit_FaturaAciklama.Name = "repositoryItemTextEdit_FaturaAciklama";
		this.repositoryItemTextEdit_Aciklamalar.AutoHeight = false;
		this.repositoryItemTextEdit_Aciklamalar.MaxLength = 127;
		this.repositoryItemTextEdit_Aciklamalar.Name = "repositoryItemTextEdit_Aciklamalar";
		this.advBandedGridView_satirlar.Appearance.BandPanel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.advBandedGridView_satirlar.Appearance.BandPanel.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.advBandedGridView_satirlar.Appearance.BandPanel.Options.UseFont = true;
		this.advBandedGridView_satirlar.Appearance.BandPanel.Options.UseForeColor = true;
		this.advBandedGridView_satirlar.Appearance.BandPanel.Options.UseTextOptions = true;
		this.advBandedGridView_satirlar.Appearance.BandPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.advBandedGridView_satirlar.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[15]
		{
			this.gridBand27, this.gridBand30, this.gridBand1, this.gridBand29, this.gridBand2, this.gridBand4, this.gridBand19, this.gridBand20, this.gridBand21, this.gridBand22,
			this.gridBand23, this.gridBand24, this.gridBand25, this.gridBand26, this.gridBand3
		});
		this.advBandedGridView_satirlar.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[37]
		{
			this.gc_satir_SatirID, this.gc_satir_EvrakID, this.gc_satir_Cinsi, this.gc_satir_HesapKodu, this.gc_satir_HesapAdi, this.gc_satir_VergiPntr, this.gc_satir_Miktar, this.gc_satir_Miktar2, this.gc_satir_Birim, this.gc_satir_BirimFiyat,
			this.gc_satir_Iskonto_1_Uygulama_Sekli, this.gc_satir_Iskonto_1_YuzdeVeyaMiktar, this.gc_satir_Iskonto_2_Uygulama_Sekli, this.gc_satir_Iskonto_2_YuzdeVeyaMiktar, this.gc_satir_Iskonto_3_Uygulama_Sekli, this.gc_satir_Iskonto_3_YuzdeVeyaMiktar, this.gc_satir_Iskonto_4_Uygulama_Sekli, this.gc_satir_Iskonto_4_YuzdeVeyaMiktar, this.gc_satir_Iskonto_5_Uygulama_Sekli, this.gc_satir_Iskonto_5_YuzdeVeyaMiktar,
			this.gc_satir_Iskonto_6_Uygulama_Sekli, this.gc_satir_Iskonto_6_YuzdeVeyaMiktar, this.gc_satir_Masraf_1_Uygulama_Sekli, this.gc_satir_Masraf_1_YuzdeVeyaMiktar, this.gc_satir_Masraf_2_Uygulama_Sekli, this.gc_satir_Masraf_2_YuzdeVeyaMiktar, this.gc_satir_Masraf_3_Uygulama_Sekli, this.gc_satir_Masraf_3_YuzdeVeyaMiktar, this.gc_satir_Masraf_4_Uygulama_Sekli, this.gc_satir_Masraf_4_YuzdeVeyaMiktar,
			this.gc_satir_SatirAciklama, this.gc_satir_ara_toplam, this.gc_satir_iskonto_tutari, this.gc_satir_masraf_tutari, this.gc_satir_kdv_tutari, this.gc_satir_yekun, this.gc_satir_FiyatKaynagi
		});
		this.advBandedGridView_satirlar.GridControl = this.gridControl_kayitlar;
		this.advBandedGridView_satirlar.Name = "advBandedGridView_satirlar";
		this.advBandedGridView_satirlar.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_satirlar.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
		this.advBandedGridView_satirlar.OptionsNavigation.EnterMoveNextColumn = true;
		this.advBandedGridView_satirlar.OptionsSelection.MultiSelect = true;
		this.advBandedGridView_satirlar.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
		this.advBandedGridView_satirlar.OptionsView.ShowGroupPanel = false;
		this.gridBand27.Caption = "ID";
		this.gridBand27.Columns.Add(this.gc_satir_SatirID);
		this.gridBand27.Columns.Add(this.gc_satir_EvrakID);
		this.gridBand27.Name = "gridBand27";
		this.gridBand27.VisibleIndex = 0;
		this.gridBand27.Width = 106;
		this.gc_satir_SatirID.Caption = "Satır ID";
		this.gc_satir_SatirID.FieldName = "SatirID";
		this.gc_satir_SatirID.Name = "gc_satir_SatirID";
		this.gc_satir_SatirID.OptionsColumn.AllowEdit = false;
		this.gc_satir_SatirID.OptionsColumn.AllowFocus = false;
		this.gc_satir_SatirID.Visible = true;
		this.gc_satir_SatirID.Width = 46;
		this.gc_satir_EvrakID.Caption = "Evrak ID";
		this.gc_satir_EvrakID.FieldName = "EvrakID";
		this.gc_satir_EvrakID.Name = "gc_satir_EvrakID";
		this.gc_satir_EvrakID.OptionsColumn.AllowEdit = false;
		this.gc_satir_EvrakID.OptionsColumn.AllowFocus = false;
		this.gc_satir_EvrakID.Visible = true;
		this.gc_satir_EvrakID.Width = 60;
		this.gridBand30.Caption = "Hesap";
		this.gridBand30.Columns.Add(this.gc_satir_Cinsi);
		this.gridBand30.Columns.Add(this.gc_satir_HesapKodu);
		this.gridBand30.Columns.Add(this.gc_satir_HesapAdi);
		this.gridBand30.Name = "gridBand30";
		this.gridBand30.VisibleIndex = 1;
		this.gridBand30.Width = 297;
		this.gc_satir_Cinsi.Caption = "Cinsi";
		this.gc_satir_Cinsi.ColumnEdit = this.repositoryItemGridLookUpEdit_Satir_Cinsi;
		this.gc_satir_Cinsi.FieldName = "Cinsi";
		this.gc_satir_Cinsi.Name = "gc_satir_Cinsi";
		this.gc_satir_Cinsi.Visible = true;
		this.gc_satir_Cinsi.Width = 52;
		this.gc_satir_HesapKodu.Caption = "Hesap Kodu";
		this.gc_satir_HesapKodu.FieldName = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.Name = "gc_satir_HesapKodu";
		this.gc_satir_HesapKodu.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapKodu.Visible = true;
		this.gc_satir_HesapKodu.Width = 106;
		this.gc_satir_HesapAdi.Caption = "Hesap Adı";
		this.gc_satir_HesapAdi.FieldName = "gc_satir_HesapKodu";
		this.gc_satir_HesapAdi.Name = "gc_satir_HesapAdi";
		this.gc_satir_HesapAdi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_HesapAdi.Visible = true;
		this.gc_satir_HesapAdi.Width = 139;
		this.gridBand1.Caption = "Miktar & Fiyat";
		this.gridBand1.Columns.Add(this.gc_satir_Miktar);
		this.gridBand1.Columns.Add(this.gc_satir_Miktar2);
		this.gridBand1.Columns.Add(this.gc_satir_Birim);
		this.gridBand1.Columns.Add(this.gc_satir_VergiPntr);
		this.gridBand1.Columns.Add(this.gc_satir_BirimFiyat);
		this.gridBand1.Columns.Add(this.gc_satir_FiyatKaynagi);
		this.gridBand1.Name = "gridBand1";
		this.gridBand1.VisibleIndex = 2;
		this.gridBand1.Width = 424;
		this.gc_satir_Miktar.Caption = "Miktar";
		this.gc_satir_Miktar.FieldName = "gc_satir_Miktar";
		this.gc_satir_Miktar.Name = "gc_satir_Miktar";
		this.gc_satir_Miktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Miktar.Visible = true;
		this.gc_satir_Miktar.Width = 53;
		this.gc_satir_Miktar2.Caption = "Miktar 2";
		this.gc_satir_Miktar2.FieldName = "gc_satir_Miktar2";
		this.gc_satir_Miktar2.Name = "gc_satir_Miktar2";
		this.gc_satir_Miktar2.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Miktar2.Visible = true;
		this.gc_satir_Miktar2.Width = 56;
		this.gc_satir_Birim.Caption = "Birim";
		this.gc_satir_Birim.FieldName = "gc_satir_Birim";
		this.gc_satir_Birim.Name = "gc_satir_Birim";
		this.gc_satir_Birim.OptionsColumn.AllowEdit = false;
		this.gc_satir_Birim.OptionsColumn.AllowFocus = false;
		this.gc_satir_Birim.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_Birim.Visible = true;
		this.gc_satir_Birim.Width = 74;
		this.gc_satir_VergiPntr.Caption = "Vergi";
		this.gc_satir_VergiPntr.ColumnEdit = this.repositoryItemLookUpEdit_Satir_VergiPntr;
		this.gc_satir_VergiPntr.FieldName = "gc_satir_VergiPntr";
		this.gc_satir_VergiPntr.Name = "gc_satir_VergiPntr";
		this.gc_satir_VergiPntr.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_VergiPntr.Visible = true;
		this.gc_satir_VergiPntr.Width = 94;
		this.gc_satir_BirimFiyat.Caption = "Birim Fiyat";
		this.gc_satir_BirimFiyat.FieldName = "gc_satir_BirimFiyat";
		this.gc_satir_BirimFiyat.Name = "gc_satir_BirimFiyat";
		this.gc_satir_BirimFiyat.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_BirimFiyat.Visible = true;
		this.gc_satir_BirimFiyat.Width = 72;
		this.gc_satir_FiyatKaynagi.Caption = "Fiyat kaynağı";
		this.gc_satir_FiyatKaynagi.FieldName = "gc_satir_FiyatKaynagi";
		this.gc_satir_FiyatKaynagi.Name = "gc_satir_FiyatKaynagi";
		this.gc_satir_FiyatKaynagi.OptionsColumn.AllowEdit = false;
		this.gc_satir_FiyatKaynagi.OptionsColumn.AllowFocus = false;
		this.gc_satir_FiyatKaynagi.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_FiyatKaynagi.Visible = true;
		this.gridBand29.Caption = "Detay";
		this.gridBand29.Columns.Add(this.gc_satir_SatirAciklama);
		this.gridBand29.Name = "gridBand29";
		this.gridBand29.VisibleIndex = 3;
		this.gridBand29.Width = 131;
		this.gc_satir_SatirAciklama.Caption = "Açıklama";
		this.gc_satir_SatirAciklama.FieldName = "gc_satir_SatirAciklama";
		this.gc_satir_SatirAciklama.Name = "gc_satir_SatirAciklama";
		this.gc_satir_SatirAciklama.UnboundType = DevExpress.Data.UnboundColumnType.String;
		this.gc_satir_SatirAciklama.Visible = true;
		this.gc_satir_SatirAciklama.Width = 131;
		this.gridBand2.Caption = "İskonto 1";
		this.gridBand2.Columns.Add(this.gc_satir_Iskonto_1_Uygulama_Sekli);
		this.gridBand2.Columns.Add(this.gc_satir_Iskonto_1_YuzdeVeyaMiktar);
		this.gridBand2.Name = "gridBand2";
		this.gridBand2.VisibleIndex = 4;
		this.gridBand2.Width = 155;
		this.gc_satir_Iskonto_1_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Iskonto_1_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Iskonto_1_Uygulama_Sekli.FieldName = "gc_satir_Iskonto_1_Uygulama_Sekli";
		this.gc_satir_Iskonto_1_Uygulama_Sekli.Name = "gc_satir_Iskonto_1_Uygulama_Sekli";
		this.gc_satir_Iskonto_1_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Iskonto_1_Uygulama_Sekli.Visible = true;
		this.gc_satir_Iskonto_1_Uygulama_Sekli.Width = 95;
		this.gc_satir_Iskonto_1_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Iskonto_1_YuzdeVeyaMiktar.FieldName = "gc_satir_Iskonto_1_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_1_YuzdeVeyaMiktar.Name = "gc_satir_Iskonto_1_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_1_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Iskonto_1_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Iskonto_1_YuzdeVeyaMiktar.Width = 60;
		this.gridBand4.Caption = "İskonto 2";
		this.gridBand4.Columns.Add(this.gc_satir_Iskonto_2_Uygulama_Sekli);
		this.gridBand4.Columns.Add(this.gc_satir_Iskonto_2_YuzdeVeyaMiktar);
		this.gridBand4.Name = "gridBand4";
		this.gridBand4.VisibleIndex = 5;
		this.gridBand4.Width = 142;
		this.gc_satir_Iskonto_2_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Iskonto_2_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Iskonto_2_Uygulama_Sekli.FieldName = "gc_satir_Iskonto_2_Uygulama_Sekli";
		this.gc_satir_Iskonto_2_Uygulama_Sekli.Name = "gc_satir_Iskonto_2_Uygulama_Sekli";
		this.gc_satir_Iskonto_2_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Iskonto_2_Uygulama_Sekli.Visible = true;
		this.gc_satir_Iskonto_2_Uygulama_Sekli.Width = 71;
		this.gc_satir_Iskonto_2_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Iskonto_2_YuzdeVeyaMiktar.FieldName = "gc_satir_Iskonto_2_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_2_YuzdeVeyaMiktar.Name = "gc_satir_Iskonto_2_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_2_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Iskonto_2_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Iskonto_2_YuzdeVeyaMiktar.Width = 71;
		this.gridBand19.Caption = "İskonto 3";
		this.gridBand19.Columns.Add(this.gc_satir_Iskonto_3_Uygulama_Sekli);
		this.gridBand19.Columns.Add(this.gc_satir_Iskonto_3_YuzdeVeyaMiktar);
		this.gridBand19.Name = "gridBand19";
		this.gridBand19.VisibleIndex = 6;
		this.gridBand19.Width = 134;
		this.gc_satir_Iskonto_3_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Iskonto_3_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Iskonto_3_Uygulama_Sekli.FieldName = "gc_satir_Iskonto_3_Uygulama_Sekli";
		this.gc_satir_Iskonto_3_Uygulama_Sekli.Name = "gc_satir_Iskonto_3_Uygulama_Sekli";
		this.gc_satir_Iskonto_3_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Iskonto_3_Uygulama_Sekli.Visible = true;
		this.gc_satir_Iskonto_3_Uygulama_Sekli.Width = 67;
		this.gc_satir_Iskonto_3_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Iskonto_3_YuzdeVeyaMiktar.FieldName = "gc_satir_Iskonto_3_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_3_YuzdeVeyaMiktar.Name = "gc_satir_Iskonto_3_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_3_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Iskonto_3_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Iskonto_3_YuzdeVeyaMiktar.Width = 67;
		this.gridBand20.Caption = "İskonto 4";
		this.gridBand20.Columns.Add(this.gc_satir_Iskonto_4_Uygulama_Sekli);
		this.gridBand20.Columns.Add(this.gc_satir_Iskonto_4_YuzdeVeyaMiktar);
		this.gridBand20.Name = "gridBand20";
		this.gridBand20.VisibleIndex = 7;
		this.gridBand20.Width = 142;
		this.gc_satir_Iskonto_4_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Iskonto_4_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Iskonto_4_Uygulama_Sekli.FieldName = "gc_satir_Iskonto_4_Uygulama_Sekli";
		this.gc_satir_Iskonto_4_Uygulama_Sekli.Name = "gc_satir_Iskonto_4_Uygulama_Sekli";
		this.gc_satir_Iskonto_4_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Iskonto_4_Uygulama_Sekli.Visible = true;
		this.gc_satir_Iskonto_4_Uygulama_Sekli.Width = 71;
		this.gc_satir_Iskonto_4_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Iskonto_4_YuzdeVeyaMiktar.FieldName = "gc_satir_Iskonto_4_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_4_YuzdeVeyaMiktar.Name = "gc_satir_Iskonto_4_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_4_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Iskonto_4_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Iskonto_4_YuzdeVeyaMiktar.Width = 71;
		this.gridBand21.Caption = "İskonto 5";
		this.gridBand21.Columns.Add(this.gc_satir_Iskonto_5_Uygulama_Sekli);
		this.gridBand21.Columns.Add(this.gc_satir_Iskonto_5_YuzdeVeyaMiktar);
		this.gridBand21.Name = "gridBand21";
		this.gridBand21.VisibleIndex = 8;
		this.gridBand21.Width = 134;
		this.gc_satir_Iskonto_5_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Iskonto_5_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Iskonto_5_Uygulama_Sekli.FieldName = "gc_satir_Iskonto_5_Uygulama_Sekli";
		this.gc_satir_Iskonto_5_Uygulama_Sekli.Name = "gc_satir_Iskonto_5_Uygulama_Sekli";
		this.gc_satir_Iskonto_5_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Iskonto_5_Uygulama_Sekli.Visible = true;
		this.gc_satir_Iskonto_5_Uygulama_Sekli.Width = 67;
		this.gc_satir_Iskonto_5_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Iskonto_5_YuzdeVeyaMiktar.FieldName = "gc_satir_Iskonto_5_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_5_YuzdeVeyaMiktar.Name = "gc_satir_Iskonto_5_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_5_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Iskonto_5_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Iskonto_5_YuzdeVeyaMiktar.Width = 67;
		this.gridBand22.Caption = "İskonto 6";
		this.gridBand22.Columns.Add(this.gc_satir_Iskonto_6_Uygulama_Sekli);
		this.gridBand22.Columns.Add(this.gc_satir_Iskonto_6_YuzdeVeyaMiktar);
		this.gridBand22.Name = "gridBand22";
		this.gridBand22.VisibleIndex = 9;
		this.gridBand22.Width = 134;
		this.gc_satir_Iskonto_6_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Iskonto_6_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Iskonto_6_Uygulama_Sekli.FieldName = "gc_satir_Iskonto_6_Uygulama_Sekli";
		this.gc_satir_Iskonto_6_Uygulama_Sekli.Name = "gc_satir_Iskonto_6_Uygulama_Sekli";
		this.gc_satir_Iskonto_6_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Iskonto_6_Uygulama_Sekli.Visible = true;
		this.gc_satir_Iskonto_6_Uygulama_Sekli.Width = 67;
		this.gc_satir_Iskonto_6_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Iskonto_6_YuzdeVeyaMiktar.FieldName = "gc_satir_Iskonto_6_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_6_YuzdeVeyaMiktar.Name = "gc_satir_Iskonto_6_YuzdeVeyaMiktar";
		this.gc_satir_Iskonto_6_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Iskonto_6_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Iskonto_6_YuzdeVeyaMiktar.Width = 67;
		this.gridBand23.Caption = "Masraf 1";
		this.gridBand23.Columns.Add(this.gc_satir_Masraf_1_Uygulama_Sekli);
		this.gridBand23.Columns.Add(this.gc_satir_Masraf_1_YuzdeVeyaMiktar);
		this.gridBand23.Name = "gridBand23";
		this.gridBand23.VisibleIndex = 10;
		this.gridBand23.Width = 130;
		this.gc_satir_Masraf_1_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Masraf_1_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Masraf_1_Uygulama_Sekli.FieldName = "gc_satir_Masraf_1_Uygulama_Sekli";
		this.gc_satir_Masraf_1_Uygulama_Sekli.Name = "gc_satir_Masraf_1_Uygulama_Sekli";
		this.gc_satir_Masraf_1_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Masraf_1_Uygulama_Sekli.Visible = true;
		this.gc_satir_Masraf_1_Uygulama_Sekli.Width = 65;
		this.gc_satir_Masraf_1_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Masraf_1_YuzdeVeyaMiktar.FieldName = "gc_satir_Masraf_1_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_1_YuzdeVeyaMiktar.Name = "gc_satir_Masraf_1_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_1_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Masraf_1_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Masraf_1_YuzdeVeyaMiktar.Width = 65;
		this.gridBand24.Caption = "Masraf 2";
		this.gridBand24.Columns.Add(this.gc_satir_Masraf_2_Uygulama_Sekli);
		this.gridBand24.Columns.Add(this.gc_satir_Masraf_2_YuzdeVeyaMiktar);
		this.gridBand24.Name = "gridBand24";
		this.gridBand24.VisibleIndex = 11;
		this.gridBand24.Width = 130;
		this.gc_satir_Masraf_2_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Masraf_2_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Masraf_2_Uygulama_Sekli.FieldName = "gc_satir_Masraf_2_Uygulama_Sekli";
		this.gc_satir_Masraf_2_Uygulama_Sekli.Name = "gc_satir_Masraf_2_Uygulama_Sekli";
		this.gc_satir_Masraf_2_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Masraf_2_Uygulama_Sekli.Visible = true;
		this.gc_satir_Masraf_2_Uygulama_Sekli.Width = 65;
		this.gc_satir_Masraf_2_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Masraf_2_YuzdeVeyaMiktar.FieldName = "gc_satir_Masraf_2_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_2_YuzdeVeyaMiktar.Name = "gc_satir_Masraf_2_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_2_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Masraf_2_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Masraf_2_YuzdeVeyaMiktar.Width = 65;
		this.gridBand25.Caption = "Masraf 3";
		this.gridBand25.Columns.Add(this.gc_satir_Masraf_3_Uygulama_Sekli);
		this.gridBand25.Columns.Add(this.gc_satir_Masraf_3_YuzdeVeyaMiktar);
		this.gridBand25.Name = "gridBand25";
		this.gridBand25.VisibleIndex = 12;
		this.gridBand25.Width = 132;
		this.gc_satir_Masraf_3_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Masraf_3_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Masraf_3_Uygulama_Sekli.FieldName = "gc_satir_Masraf_3_Uygulama_Sekli";
		this.gc_satir_Masraf_3_Uygulama_Sekli.Name = "gc_satir_Masraf_3_Uygulama_Sekli";
		this.gc_satir_Masraf_3_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Masraf_3_Uygulama_Sekli.Visible = true;
		this.gc_satir_Masraf_3_Uygulama_Sekli.Width = 66;
		this.gc_satir_Masraf_3_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Masraf_3_YuzdeVeyaMiktar.FieldName = "gc_satir_Masraf_3_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_3_YuzdeVeyaMiktar.Name = "gc_satir_Masraf_3_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_3_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Masraf_3_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Masraf_3_YuzdeVeyaMiktar.Width = 66;
		this.gridBand26.Caption = "Masraf 4";
		this.gridBand26.Columns.Add(this.gc_satir_Masraf_4_Uygulama_Sekli);
		this.gridBand26.Columns.Add(this.gc_satir_Masraf_4_YuzdeVeyaMiktar);
		this.gridBand26.Name = "gridBand26";
		this.gridBand26.VisibleIndex = 13;
		this.gridBand26.Width = 122;
		this.gc_satir_Masraf_4_Uygulama_Sekli.Caption = "Uyg. Şek.";
		this.gc_satir_Masraf_4_Uygulama_Sekli.ColumnEdit = this.repositoryItemLookUpEdit_Satir_IskontoSekli;
		this.gc_satir_Masraf_4_Uygulama_Sekli.FieldName = "gc_satir_Masraf_4_Uygulama_Sekli";
		this.gc_satir_Masraf_4_Uygulama_Sekli.Name = "gc_satir_Masraf_4_Uygulama_Sekli";
		this.gc_satir_Masraf_4_Uygulama_Sekli.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
		this.gc_satir_Masraf_4_Uygulama_Sekli.Visible = true;
		this.gc_satir_Masraf_4_Uygulama_Sekli.Width = 61;
		this.gc_satir_Masraf_4_YuzdeVeyaMiktar.Caption = "Değer";
		this.gc_satir_Masraf_4_YuzdeVeyaMiktar.FieldName = "gc_satir_Masraf_4_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_4_YuzdeVeyaMiktar.Name = "gc_satir_Masraf_4_YuzdeVeyaMiktar";
		this.gc_satir_Masraf_4_YuzdeVeyaMiktar.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_Masraf_4_YuzdeVeyaMiktar.Visible = true;
		this.gc_satir_Masraf_4_YuzdeVeyaMiktar.Width = 61;
		this.gridBand3.Caption = "Toplamlar";
		this.gridBand3.Columns.Add(this.gc_satir_ara_toplam);
		this.gridBand3.Columns.Add(this.gc_satir_iskonto_tutari);
		this.gridBand3.Columns.Add(this.gc_satir_masraf_tutari);
		this.gridBand3.Columns.Add(this.gc_satir_kdv_tutari);
		this.gridBand3.Columns.Add(this.gc_satir_yekun);
		this.gridBand3.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Right;
		this.gridBand3.Name = "gridBand3";
		this.gridBand3.VisibleIndex = 14;
		this.gridBand3.Width = 361;
		this.gc_satir_ara_toplam.Caption = "Ara Toplam";
		this.gc_satir_ara_toplam.FieldName = "gc_satir_ara_toplam";
		this.gc_satir_ara_toplam.Name = "gc_satir_ara_toplam";
		this.gc_satir_ara_toplam.OptionsColumn.AllowEdit = false;
		this.gc_satir_ara_toplam.OptionsColumn.AllowFocus = false;
		this.gc_satir_ara_toplam.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_ara_toplam.Visible = true;
		this.gc_satir_ara_toplam.Width = 85;
		this.gc_satir_iskonto_tutari.Caption = "İskonto";
		this.gc_satir_iskonto_tutari.FieldName = "gc_satir_iskonto_tutari";
		this.gc_satir_iskonto_tutari.Name = "gc_satir_iskonto_tutari";
		this.gc_satir_iskonto_tutari.OptionsColumn.AllowEdit = false;
		this.gc_satir_iskonto_tutari.OptionsColumn.AllowFocus = false;
		this.gc_satir_iskonto_tutari.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_iskonto_tutari.Visible = true;
		this.gc_satir_iskonto_tutari.Width = 72;
		this.gc_satir_masraf_tutari.Caption = "Masraf";
		this.gc_satir_masraf_tutari.FieldName = "gc_satir_masraf_tutari";
		this.gc_satir_masraf_tutari.Name = "gc_satir_masraf_tutari";
		this.gc_satir_masraf_tutari.OptionsColumn.AllowEdit = false;
		this.gc_satir_masraf_tutari.OptionsColumn.AllowFocus = false;
		this.gc_satir_masraf_tutari.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_masraf_tutari.Visible = true;
		this.gc_satir_masraf_tutari.Width = 78;
		this.gc_satir_kdv_tutari.Caption = "Kdv";
		this.gc_satir_kdv_tutari.FieldName = "gc_satir_kdv_tutari";
		this.gc_satir_kdv_tutari.Name = "gc_satir_kdv_tutari";
		this.gc_satir_kdv_tutari.OptionsColumn.AllowEdit = false;
		this.gc_satir_kdv_tutari.OptionsColumn.AllowFocus = false;
		this.gc_satir_kdv_tutari.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_kdv_tutari.Visible = true;
		this.gc_satir_kdv_tutari.Width = 72;
		this.gc_satir_yekun.Caption = "Yekün";
		this.gc_satir_yekun.FieldName = "gc_satir_yekun";
		this.gc_satir_yekun.Name = "gc_satir_yekun";
		this.gc_satir_yekun.OptionsColumn.AllowEdit = false;
		this.gc_satir_yekun.OptionsColumn.AllowFocus = false;
		this.gc_satir_yekun.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_satir_yekun.Visible = true;
		this.gc_satir_yekun.Width = 54;
		this.cardView1.FocusedCardTopFieldIndex = 0;
		this.cardView1.GridControl = this.gridControl_kayitlar;
		this.cardView1.Name = "cardView1";
		this.sb_sil.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.sb_sil.Appearance.Font = new System.Drawing.Font("Arial Black", 10f, System.Drawing.FontStyle.Bold);
		this.sb_sil.Appearance.Options.UseFont = true;
		this.sb_sil.Location = new System.Drawing.Point(758, 390);
		this.sb_sil.Name = "sb_sil";
		this.sb_sil.Size = new System.Drawing.Size(182, 28);
		this.sb_sil.TabIndex = 16;
		this.sb_sil.Text = "SEÇİLİ İŞLEMLERİ SİL";
		this.sb_sil.Click += new System.EventHandler(sb_sil_Click);
		this.labelControl1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Arial Narrow", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.labelControl1.Location = new System.Drawing.Point(12, 396);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(100, 16);
		this.labelControl1.TabIndex = 17;
		this.labelControl1.Text = "İşlem tarih aralığı : ";
		this.dateEdit_baslangic.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.dateEdit_baslangic.EditValue = null;
		this.dateEdit_baslangic.Location = new System.Drawing.Point(118, 395);
		this.dateEdit_baslangic.Name = "dateEdit_baslangic";
		this.dateEdit_baslangic.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_baslangic.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_baslangic.Properties.CalendarTimeProperties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F4);
		this.dateEdit_baslangic.Properties.CalendarTimeProperties.PopupBorderStyle = DevExpress.XtraEditors.Controls.PopupBorderStyles.Default;
		this.dateEdit_baslangic.Size = new System.Drawing.Size(100, 20);
		this.dateEdit_baslangic.TabIndex = 18;
		this.labelControl2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Arial Narrow", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.labelControl2.Location = new System.Drawing.Point(224, 396);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(4, 16);
		this.labelControl2.TabIndex = 19;
		this.labelControl2.Text = "-";
		this.dateEdit_bitis.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.dateEdit_bitis.EditValue = null;
		this.dateEdit_bitis.Location = new System.Drawing.Point(234, 395);
		this.dateEdit_bitis.Name = "dateEdit_bitis";
		this.dateEdit_bitis.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_bitis.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.dateEdit_bitis.Properties.CalendarTimeProperties.CloseUpKey = new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.F4);
		this.dateEdit_bitis.Properties.CalendarTimeProperties.PopupBorderStyle = DevExpress.XtraEditors.Controls.PopupBorderStyles.Default;
		this.dateEdit_bitis.Size = new System.Drawing.Size(100, 20);
		this.dateEdit_bitis.TabIndex = 20;
		this.sb_tarih_araligi_degistir.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.sb_tarih_araligi_degistir.Appearance.Font = new System.Drawing.Font("Arial Black", 10f, System.Drawing.FontStyle.Bold);
		this.sb_tarih_araligi_degistir.Appearance.Options.UseFont = true;
		this.sb_tarih_araligi_degistir.Location = new System.Drawing.Point(340, 390);
		this.sb_tarih_araligi_degistir.Name = "sb_tarih_araligi_degistir";
		this.sb_tarih_araligi_degistir.Size = new System.Drawing.Size(221, 28);
		this.sb_tarih_araligi_degistir.TabIndex = 21;
		this.sb_tarih_araligi_degistir.Text = "TARİH ARALIĞINI DEĞİŞTİR";
		this.sb_tarih_araligi_degistir.Click += new System.EventHandler(sb_tarih_araligi_degistir_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(952, 427);
		base.Controls.Add(this.sb_tarih_araligi_degistir);
		base.Controls.Add(this.dateEdit_bitis);
		base.Controls.Add(this.labelControl2);
		base.Controls.Add(this.dateEdit_baslangic);
		base.Controls.Add(this.labelControl1);
		base.Controls.Add(this.sb_sil);
		base.Controls.Add(this.gridControl_kayitlar);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		this.MinimumSize = new System.Drawing.Size(780, 250);
		base.Name = "KayitHikayesi";
		this.Text = "Kalite Kontrol Yönetimi - Kayıt hikayesi";
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.gridControl_kayitlar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_master).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_VergiPntr).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Satir_IskontoSekli).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Satir_Cinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_SatirCinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_Evrak_Tipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_EvrakTipi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_NormalIade).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemDateEdit_EvrakTarih).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemLookUpEdit_Evrak_DovizCinsi).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_AcikKapali).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit1View).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Evrak_Kur).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemGridLookUpEdit_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gridView_TicaretTuru).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_FaturaAciklama).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit_Aciklamalar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.advBandedGridView_satirlar).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cardView1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic.Properties.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_baslangic.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis.Properties.CalendarTimeProperties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateEdit_bitis.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
