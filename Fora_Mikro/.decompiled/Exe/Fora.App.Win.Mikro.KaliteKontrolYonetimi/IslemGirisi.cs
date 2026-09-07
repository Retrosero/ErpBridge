using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Fora.Mikro;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Depolar;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Firmalar;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Subeler;
using Fora.Mikro.Win.Form.DevEx.F10;

namespace Fora.App.Win.Mikro.KaliteKontrolYonetimi;

public class IslemGirisi : XtraForm
{
	private MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private Depo _KaynakDepo;

	private Depo _OnayDepo;

	private Depo _RetDepo;

	private Depo _HurdaDepo;

	private Depo _IadeDepo;

	private string _DepolarArasiSevkEvrakSeri;

	private List<Satirlar> _Satirlar;

	private IContainer components;

	private GridControl gc_Bekleyenler;

	private GridView gv_Bekleyenler;

	private GridColumn gcstok_kodu;

	private TableLayoutPanel tableLayoutPanel1;

	private LabelControl labelControl2;

	private LabelControl labelControl4;

	private GridColumn gc_tarih;

	private GridColumn gc_evraktipi;

	private GridColumn gc_evrak_seri;

	private GridColumn gc_evrak_sira;

	private GridColumn gc_satir_no;

	private GridColumn gc_stok_ismi;

	private GridColumn gc_aciklama;

	private GridColumn gc_miktar;

	private GridColumn gc_onay_miktari;

	private RepositoryItemTextEdit repositoryItemTextEdit1;

	private GridColumn gc_kayit_no;

	private MenuStrip menuStrip1;

	private ToolStripMenuItem dosyaToolStripMenuItem;

	private ToolStripMenuItem kapatToolStripMenuItem;

	private GridColumn gc_ret_miktari;

	private GridColumn gc_hurda_miktari;

	private GridColumn gc_iade_miktari;

	private TableLayoutPanel tableLayoutPanel2;

	private LabelControl label_depolararasisevkevrakseri;

	private LabelControl labelControl7;

	private LabelControl labelControl3;

	private LabelControl labelControl1;

	private LabelControl labelControl5;

	private LabelControl labelControl6;

	private SimpleButton sb_kaydet;

	private GridColumn gc_parti_kodu;

	private GridColumn gc_islem_yap;

	private ToolStripMenuItem işlemlerToolStripMenuItem;

	private ToolStripMenuItem seciliSatirlariListedenKaldirToolStripMenuItem;

	private GridColumn gc_islem_aciklamasi;

	private GridColumn gc_ret_aciklama;

	private GridColumn gc_hurda_aciklama;

	private GridColumn gc_iade_aciklama;

	private ToolStripMenuItem yenileToolStripMenuItem;

	private SimpleButton b_kaynak_depo;

	private SimpleButton b_iade_depo;

	private SimpleButton b_hurda_depo;

	private SimpleButton b_ret_depo;

	private SimpleButton b_onay_depo;

	public IslemGirisi(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		_KaynakDepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiKaynakDepo")._GetInt);
		_OnayDepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiOnayDepo")._GetInt);
		_RetDepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiRetDepo")._GetInt);
		_HurdaDepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiHurdaDepo")._GetInt);
		_IadeDepo = DepoData.GetDepo(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiIadeDepo")._GetInt);
		_DepolarArasiSevkEvrakSeri = _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiDepolarArasiSevkEvrakSeri")._GetString;
		InitializeComponent();
		b_kaynak_depo.Text = _KaynakDepo.dep_adi;
		b_onay_depo.Text = _OnayDepo.dep_adi;
		b_ret_depo.Text = _RetDepo.dep_adi;
		b_hurda_depo.Text = _HurdaDepo.dep_adi;
		b_iade_depo.Text = _IadeDepo.dep_adi;
		label_depolararasisevkevrakseri.Text = _DepolarArasiSevkEvrakSeri;
		ListeleriYenile();
	}

	public void ListeleriYenile()
	{
		_Satirlar = new List<Satirlar>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		string commandText = "SELECT sth_RECid_RECno,sth_tarih,sth_evraktip,sth_evrakno_seri,sth_evrakno_sira,sth_satirno,sth_parti_kodu,sth_stok_kod,(SELECT TOP 1 sto_isim FROM STOKLAR WITH(NOLOCK) WHERE sto_kod=sth_stok_kod) AS sto_isim,sth_miktar,sth_aciklama FROM STOK_HAREKETLERI WITH(NOLOCK) WHERE sth_tarih >= @sth_tarih AND sth_giris_depo_no=@kaynak_depo_no AND sth_tip in (0,2) AND sth_RECid_RECno not in (SELECT sth_RECid_RECno FROM _FORA_KALITE_KONTROL WITH(NOLOCK) WHERE islem_tarih >= @sth_tarih AND kaynak_depo_no=@kaynak_depo_no) ORDER BY sth_tarih DESC,sth_RECid_RECno DESC";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@kaynak_depo_no", _KaynakDepo.dep_no);
			DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			dateTime = dateTime.AddDays(-1 * _mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiGunSayisi")._GetInt);
			sqlCommand.Parameters.AddWithValue("@sth_tarih", dateTime);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				Satirlar satirlar = new Satirlar();
				satirlar.sth_RECid_RECno = sqlDataReader.GetSafeInt32(0);
				satirlar.sth_aciklama = sqlDataReader.GetSafeString(10);
				satirlar.sth_evrakno_seri = sqlDataReader.GetSafeString(3);
				satirlar.sth_evrakno_sira = sqlDataReader.GetSafeInt32(4);
				satirlar.sth_evraktip = (enum_sth_evraktip)sqlDataReader.GetSafeByte(2);
				satirlar.sth_satirno = sqlDataReader.GetSafeInt32(5);
				satirlar.sth_parti_kodu = sqlDataReader.GetSafeString(6);
				satirlar.sth_stok_kod = sqlDataReader.GetSafeString(7);
				satirlar.sth_tarih = sqlDataReader.GetSafeDateTime(1);
				satirlar.sto_isim = sqlDataReader.GetSafeString(8);
				satirlar.sth_miktar = sqlDataReader.GetSafeDouble(9);
				satirlar.ret_miktar = 0.0;
				satirlar.islemyap = false;
				satirlar.onay_miktar = satirlar.sth_miktar;
				satirlar.ret_miktar = 0.0;
				satirlar.hurda_miktar = 0.0;
				satirlar.iade_miktar = 0.0;
				satirlar.islem_aciklama = "";
				satirlar.ret_aciklama = "";
				satirlar.hurda_aciklama = "";
				satirlar.iade_aciklama = "";
				_Satirlar.Add(satirlar);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		gc_Bekleyenler.DataSource = _Satirlar;
	}

	private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void gv_Bekleyenler_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
	{
		if (e.IsGetData)
		{
			switch (e.Column.Name)
			{
			case "gc_ret_miktari":
				e.Value = _Satirlar[e.ListSourceRowIndex].ret_miktar;
				break;
			case "gc_hurda_miktari":
				e.Value = _Satirlar[e.ListSourceRowIndex].hurda_miktar;
				break;
			case "gc_iade_miktari":
				e.Value = _Satirlar[e.ListSourceRowIndex].iade_miktar;
				break;
			}
		}
		if (!e.IsSetData)
		{
			return;
		}
		switch (e.Column.Name)
		{
		case "gc_ret_miktari":
		{
			double num3 = (double)(decimal)e.Value;
			double ret_miktar = _Satirlar[e.ListSourceRowIndex].ret_miktar;
			if (_Satirlar[e.ListSourceRowIndex].onay_miktar - (num3 - ret_miktar) >= 0.0)
			{
				_Satirlar[e.ListSourceRowIndex].ret_miktar = (double)(decimal)e.Value;
				_Satirlar[e.ListSourceRowIndex].onay_miktar -= num3 - ret_miktar;
			}
			else
			{
				MessageBox.Show("Miktar onaylanmadı. Onay, Ret, Hurda ve İade miktarlarının toplamı ürün miktarından fazla!");
			}
			break;
		}
		case "gc_hurda_miktari":
		{
			double num2 = (double)(decimal)e.Value;
			double hurda_miktar = _Satirlar[e.ListSourceRowIndex].hurda_miktar;
			if (_Satirlar[e.ListSourceRowIndex].onay_miktar - (num2 - hurda_miktar) >= 0.0)
			{
				_Satirlar[e.ListSourceRowIndex].hurda_miktar = (double)(decimal)e.Value;
				_Satirlar[e.ListSourceRowIndex].onay_miktar -= num2 - hurda_miktar;
			}
			else
			{
				MessageBox.Show("Miktar onaylanmadı. Onay, Ret, Hurda ve İade miktarlarının toplamı ürün miktarından fazla!");
			}
			break;
		}
		case "gc_iade_miktari":
		{
			double num = (double)(decimal)e.Value;
			double iade_miktar = _Satirlar[e.ListSourceRowIndex].iade_miktar;
			if (_Satirlar[e.ListSourceRowIndex].onay_miktar - (num - iade_miktar) >= 0.0)
			{
				_Satirlar[e.ListSourceRowIndex].iade_miktar = (double)(decimal)e.Value;
				_Satirlar[e.ListSourceRowIndex].onay_miktar -= num - iade_miktar;
			}
			else
			{
				MessageBox.Show("Miktar onaylanmadı. Onay, Ret, Hurda ve İade miktarlarının toplamı ürün miktarından fazla!");
			}
			break;
		}
		}
	}

	private void seciliSatirlariListedenKaldirToolStripMenuItem_Click(object sender, EventArgs e)
	{
		int[] selectedRows = gv_Bekleyenler.GetSelectedRows();
		foreach (int num in selectedRows)
		{
			if (num < 0)
			{
				continue;
			}
			int dataSourceRowIndex = gv_Bekleyenler.GetDataSourceRowIndex(num);
			try
			{
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
				string commandText = "BEGIN INSERT INTO _FORA_KALITE_KONTROL(sth_RECid_RECno,sth_stok_kod,sth_parti_kodu,sth_aciklama,sth_miktar,sth_tarih,kullanici_adi,islem_tarih,islem_yapildi,islem_aciklama,kaynak_depo_no,onay_depo_no,onay_miktar,ret_depo_no,ret_miktar,ret_aciklama,ret_evrak_seri,ret_evrak_sira,hurda_depo_no,hurda_miktar,hurda_aciklama,hurda_evrak_seri,hurda_evrak_sira,iade_depo_no,iade_miktar,iade_aciklama,iade_evrak_seri,iade_evrak_sira) VALUES(@sth_RECid_RECno,@sth_stok_kod,@sth_parti_kodu,@sth_aciklama,@sth_miktar,@sth_tarih,@kullanici_adi,getdate(),@islem_yapildi,@islem_aciklama,@kaynak_depo_no,@onay_depo_no,@onay_miktar,@ret_depo_no,@ret_miktar,@ret_aciklama,@ret_evrak_seri,@ret_evrak_sira,@hurda_depo_no,@hurda_miktar,@hurda_aciklama,@hurda_evrak_seri,@hurda_evrak_sira,@iade_depo_no,@iade_miktar,@iade_aciklama,@iade_evrak_seri,@iade_evrak_sira) SELECT SCOPE_IDENTITY() END";
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					sqlCommand.CommandText = commandText;
					sqlCommand.Parameters.AddWithValue("@sth_RECid_RECno", _Satirlar[dataSourceRowIndex].sth_RECid_RECno);
					sqlCommand.Parameters.AddWithValue("@sth_stok_kod", _Satirlar[dataSourceRowIndex].sth_stok_kod);
					sqlCommand.Parameters.AddWithValue("@sth_parti_kodu", _Satirlar[dataSourceRowIndex].sth_parti_kodu);
					sqlCommand.Parameters.AddWithValue("@sth_aciklama", _Satirlar[dataSourceRowIndex].sth_aciklama);
					sqlCommand.Parameters.AddWithValue("@sth_miktar", _Satirlar[dataSourceRowIndex].sth_miktar);
					sqlCommand.Parameters.AddWithValue("@sth_tarih", _Satirlar[dataSourceRowIndex].sth_tarih);
					sqlCommand.Parameters.AddWithValue("@kullanici_adi", _mikrouygulamabilgileri.KullaniciAdi);
					sqlCommand.Parameters.AddWithValue("@islem_yapildi", false);
					sqlCommand.Parameters.AddWithValue("@islem_aciklama", "Listeden kaldırıldı");
					sqlCommand.Parameters.AddWithValue("@kaynak_depo_no", _KaynakDepo.dep_no);
					sqlCommand.Parameters.AddWithValue("@onay_depo_no", 0);
					sqlCommand.Parameters.AddWithValue("@onay_miktar", 0);
					sqlCommand.Parameters.AddWithValue("@ret_depo_no", 0);
					sqlCommand.Parameters.AddWithValue("@ret_miktar", 0);
					sqlCommand.Parameters.AddWithValue("@ret_aciklama", "");
					sqlCommand.Parameters.AddWithValue("@ret_evrak_seri", "");
					sqlCommand.Parameters.AddWithValue("@ret_evrak_sira", 0);
					sqlCommand.Parameters.AddWithValue("@hurda_depo_no", 0);
					sqlCommand.Parameters.AddWithValue("@hurda_miktar", 0);
					sqlCommand.Parameters.AddWithValue("@hurda_aciklama", "");
					sqlCommand.Parameters.AddWithValue("@hurda_evrak_seri", "");
					sqlCommand.Parameters.AddWithValue("@hurda_evrak_sira", 0);
					sqlCommand.Parameters.AddWithValue("@iade_depo_no", 0);
					sqlCommand.Parameters.AddWithValue("@iade_miktar", 0);
					sqlCommand.Parameters.AddWithValue("@iade_aciklama", "");
					sqlCommand.Parameters.AddWithValue("@iade_evrak_seri", "");
					sqlCommand.Parameters.AddWithValue("@iade_evrak_sira", 0);
					sqlCommand.ExecuteScalar().ToString();
				}
				sqlDB.ConnectionClose();
			}
			catch (Exception ex)
			{
				MessageBox.Show("İşlem yapılamadı. Lütfen daha sonra tekrar deneyiniz. Hata : " + ex.ToString());
			}
		}
		ListeleriYenile();
	}

	private void sb_kaydet_Click(object sender, EventArgs e)
	{
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSevk, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		evrak.SetEvrakNoSeri(_DepolarArasiSevkEvrakSeri);
		evrak.SetHedefDepo(_RetDepo);
		evrak.SetKaynakDepo(_OnayDepo);
		evrak.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
		evrak.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
		evrak.alternatifdovizkuru = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, evrak.alternatifdovizcinsi, "1", evrak.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
		evrak.SetSube(new Sube(_mikrouygulamabilgileri.FirmaNo));
		evrak.SetFirma(new Firma(_mikrouygulamabilgileri.FirmaNo));
		Evrak evrak2 = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSevk, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		evrak2.SetEvrakNoSeri(_DepolarArasiSevkEvrakSeri);
		evrak2.SetHedefDepo(_HurdaDepo);
		evrak2.SetKaynakDepo(_OnayDepo);
		evrak2.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
		evrak2.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
		evrak2.alternatifdovizkuru = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, evrak2.alternatifdovizcinsi, "1", evrak2.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
		evrak2.SetSube(new Sube(_mikrouygulamabilgileri.FirmaNo));
		evrak2.SetFirma(new Firma(_mikrouygulamabilgileri.FirmaNo));
		Evrak evrak3 = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSevk, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		evrak3.SetEvrakNoSeri(_DepolarArasiSevkEvrakSeri);
		evrak3.SetHedefDepo(_IadeDepo);
		evrak3.SetKaynakDepo(_OnayDepo);
		evrak3.SetMikroUserNo(_mikrouygulamabilgileri.mikrokullanici.User_no);
		evrak3.SetAlternatifDovizCinsi(_mikrouygulamabilgileri.veritabani.DB_alternatif_doviz);
		evrak3.alternatifdovizkuru = KurData.GetKur(_mikrouygulamabilgileri.baglantibilgileri, evrak3.alternatifdovizcinsi, "1", evrak3.EvrakTarihi, _mikrouygulamabilgileri.MikroAnaDBName);
		evrak3.SetSube(new Sube(_mikrouygulamabilgileri.FirmaNo));
		evrak3.SetFirma(new Firma(_mikrouygulamabilgileri.FirmaNo));
		bool flag = false;
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
		try
		{
			foreach (Satirlar item in _Satirlar)
			{
				if (item.islemyap)
				{
					flag = true;
					SqlCommand sqlCommand = new SqlCommand("UPDATE STOK_HAREKETLERI SET sth_giris_depo_no=@sth_giris_depo_no,sth_lastup_user=@sth_lastup_user,sth_lastup_date=getdate() WHERE sth_RECid_RECno=@sth_RECid_RECno", sqlConnection, sqlTransaction);
					int dep_no = _OnayDepo.dep_no;
					if (item.ret_miktar == item.sth_miktar)
					{
						dep_no = _RetDepo.dep_no;
					}
					if (item.hurda_miktar == item.sth_miktar)
					{
						dep_no = _HurdaDepo.dep_no;
					}
					if (item.iade_miktar == item.sth_miktar)
					{
						dep_no = _IadeDepo.dep_no;
					}
					sqlCommand.Parameters.AddWithValue("@sth_giris_depo_no", dep_no);
					sqlCommand.Parameters.AddWithValue("@sth_lastup_user", _mikrouygulamabilgileri.mikrokullanici.User_no);
					sqlCommand.Parameters.AddWithValue("@sth_RECid_RECno", item.sth_RECid_RECno);
					sqlCommand.ExecuteNonQuery();
					if (item.ret_miktar > 0.0 && item.ret_miktar != item.sth_miktar)
					{
						Stok stok = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item.sth_stok_kod, evrak.GetToptanPerakende());
						stok.ekleme_bilgileri.BirimFiyat = new FiyatTanimlamasi();
						stok.ekleme_bilgileri.Miktar = item.ret_miktar;
						stok.ekleme_bilgileri.parti_kodu = item.sth_parti_kodu;
						stok.ekleme_bilgileri.proje_kodu = evrak.proje.pro_kodu;
						stok.ekleme_bilgileri.sorumluluk_merkezi_kodu = evrak.sorumlulukmerkezi.som_kod;
						evrak.AddUrun(stok);
					}
					if (item.hurda_miktar > 0.0 && item.hurda_miktar != item.sth_miktar)
					{
						Stok stok2 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item.sth_stok_kod, evrak2.GetToptanPerakende());
						stok2.ekleme_bilgileri.BirimFiyat = new FiyatTanimlamasi();
						stok2.ekleme_bilgileri.Miktar = item.hurda_miktar;
						stok2.ekleme_bilgileri.parti_kodu = item.sth_parti_kodu;
						stok2.ekleme_bilgileri.proje_kodu = evrak2.proje.pro_kodu;
						stok2.ekleme_bilgileri.sorumluluk_merkezi_kodu = evrak2.sorumlulukmerkezi.som_kod;
						evrak2.AddUrun(stok2);
					}
					if (item.iade_miktar > 0.0 && item.iade_miktar != item.sth_miktar)
					{
						Stok stok3 = StokData.GetStok(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, item.sth_stok_kod, evrak3.GetToptanPerakende());
						stok3.ekleme_bilgileri.BirimFiyat = new FiyatTanimlamasi();
						stok3.ekleme_bilgileri.Miktar = item.iade_miktar;
						stok3.ekleme_bilgileri.parti_kodu = item.sth_parti_kodu;
						stok3.ekleme_bilgileri.proje_kodu = evrak3.proje.pro_kodu;
						stok3.ekleme_bilgileri.sorumluluk_merkezi_kodu = evrak3.sorumlulukmerkezi.som_kod;
						evrak3.AddUrun(stok3);
					}
				}
			}
			if (flag)
			{
				int num = 0;
				if (evrak.GetStokHareketleri().Count > 0)
				{
					num = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false);
				}
				int num2 = 0;
				if (evrak2.GetStokHareketleri().Count > 0)
				{
					num2 = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak2, EArsivAktif: false);
				}
				int num3 = 0;
				if (evrak3.GetStokHareketleri().Count > 0)
				{
					num3 = EvrakData.EvrakKaydet(sqlConnection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak3, EArsivAktif: false);
				}
				foreach (Satirlar item2 in _Satirlar)
				{
					if (item2.islemyap)
					{
						SqlCommand sqlCommand2 = new SqlCommand("BEGIN INSERT INTO _FORA_KALITE_KONTROL(sth_RECid_RECno,sth_stok_kod,sth_parti_kodu,sth_aciklama,sth_miktar,sth_tarih,kullanici_adi,islem_tarih,islem_yapildi,islem_aciklama,kaynak_depo_no,onay_depo_no,onay_miktar,ret_depo_no,ret_miktar,ret_aciklama,ret_evrak_seri,ret_evrak_sira,hurda_depo_no,hurda_miktar,hurda_aciklama,hurda_evrak_seri,hurda_evrak_sira,iade_depo_no,iade_miktar,iade_aciklama,iade_evrak_seri,iade_evrak_sira) VALUES(@sth_RECid_RECno,@sth_stok_kod,@sth_parti_kodu,@sth_aciklama,@sth_miktar,@sth_tarih,@kullanici_adi,getdate(),@islem_yapildi,@islem_aciklama,@kaynak_depo_no,@onay_depo_no,@onay_miktar,@ret_depo_no,@ret_miktar,@ret_aciklama,@ret_evrak_seri,@ret_evrak_sira,@hurda_depo_no,@hurda_miktar,@hurda_aciklama,@hurda_evrak_seri,@hurda_evrak_sira,@iade_depo_no,@iade_miktar,@iade_aciklama,@iade_evrak_seri,@iade_evrak_sira) SELECT SCOPE_IDENTITY() END", sqlConnection, sqlTransaction);
						sqlCommand2.Parameters.AddWithValue("@sth_RECid_RECno", item2.sth_RECid_RECno);
						sqlCommand2.Parameters.AddWithValue("@sth_stok_kod", item2.sth_stok_kod);
						sqlCommand2.Parameters.AddWithValue("@sth_parti_kodu", item2.sth_parti_kodu);
						sqlCommand2.Parameters.AddWithValue("@sth_aciklama", item2.sth_aciklama);
						sqlCommand2.Parameters.AddWithValue("@sth_miktar", item2.sth_miktar);
						sqlCommand2.Parameters.AddWithValue("@sth_tarih", item2.sth_tarih);
						sqlCommand2.Parameters.AddWithValue("@kullanici_adi", _mikrouygulamabilgileri.KullaniciAdi);
						sqlCommand2.Parameters.AddWithValue("@islem_yapildi", true);
						sqlCommand2.Parameters.AddWithValue("@islem_aciklama", item2.islem_aciklama);
						sqlCommand2.Parameters.AddWithValue("@kaynak_depo_no", _KaynakDepo.dep_no);
						sqlCommand2.Parameters.AddWithValue("@onay_depo_no", _OnayDepo.dep_no);
						sqlCommand2.Parameters.AddWithValue("@onay_miktar", item2.onay_miktar);
						sqlCommand2.Parameters.AddWithValue("@ret_depo_no", _RetDepo.dep_no);
						sqlCommand2.Parameters.AddWithValue("@ret_miktar", item2.ret_miktar);
						sqlCommand2.Parameters.AddWithValue("@ret_aciklama", item2.ret_aciklama);
						if (item2.ret_miktar > 0.0 && item2.ret_miktar != item2.sth_miktar)
						{
							sqlCommand2.Parameters.AddWithValue("@ret_evrak_seri", _DepolarArasiSevkEvrakSeri);
							sqlCommand2.Parameters.AddWithValue("@ret_evrak_sira", num);
						}
						else
						{
							sqlCommand2.Parameters.AddWithValue("@ret_evrak_seri", "");
							sqlCommand2.Parameters.AddWithValue("@ret_evrak_sira", 0);
						}
						sqlCommand2.Parameters.AddWithValue("@hurda_depo_no", _HurdaDepo.dep_no);
						sqlCommand2.Parameters.AddWithValue("@hurda_miktar", item2.hurda_miktar);
						sqlCommand2.Parameters.AddWithValue("@hurda_aciklama", item2.hurda_aciklama);
						if (item2.hurda_miktar > 0.0 && item2.hurda_miktar != item2.sth_miktar)
						{
							sqlCommand2.Parameters.AddWithValue("@hurda_evrak_seri", _DepolarArasiSevkEvrakSeri);
							sqlCommand2.Parameters.AddWithValue("@hurda_evrak_sira", num2);
						}
						else
						{
							sqlCommand2.Parameters.AddWithValue("@hurda_evrak_seri", "");
							sqlCommand2.Parameters.AddWithValue("@hurda_evrak_sira", 0);
						}
						sqlCommand2.Parameters.AddWithValue("@iade_depo_no", _IadeDepo.dep_no);
						sqlCommand2.Parameters.AddWithValue("@iade_miktar", item2.iade_miktar);
						sqlCommand2.Parameters.AddWithValue("@iade_aciklama", item2.iade_aciklama);
						if (item2.iade_miktar > 0.0 && item2.iade_miktar != item2.sth_miktar)
						{
							sqlCommand2.Parameters.AddWithValue("@iade_evrak_seri", _DepolarArasiSevkEvrakSeri);
							sqlCommand2.Parameters.AddWithValue("@iade_evrak_sira", num3);
						}
						else
						{
							sqlCommand2.Parameters.AddWithValue("@iade_evrak_seri", "");
							sqlCommand2.Parameters.AddWithValue("@iade_evrak_sira", 0);
						}
						sqlCommand2.ExecuteScalar().ToString();
					}
				}
				ListeleriYenile();
			}
			else
			{
				MessageBox.Show("Hiç bir işlem yapılmadı. Lütfen işlem yapılacak satırları işaretleyiniz.");
			}
			sqlTransaction.Commit();
		}
		catch (Exception ex)
		{
			sqlTransaction.Rollback();
			MessageBox.Show("İşlemler yapılırken bir hata oluştu ve bütün işlemler geri alındı. Hata : " + ex.ToString());
		}
		finally
		{
			sqlConnection.Close();
		}
	}

	private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		ListeleriYenile();
	}

	private void b_kaynak_depo_Click(object sender, EventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiKaynakDepoDegistirebilir")._GetBoolean)
		{
			if (AppBase.MikroVersiyonu >= 16)
			{
				F10_Depo_Secimi_V16 f10_Depo_Secimi_V = new F10_Depo_Secimi_V16();
				f10_Depo_Secimi_V.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi_V.ShowDialog() == DialogResult.OK)
				{
					_KaynakDepo = f10_Depo_Secimi_V._selecteditems[0];
					b_kaynak_depo.Text = _KaynakDepo.dep_adi;
					ListeleriYenile();
				}
			}
			else
			{
				F10_Depo_Secimi f10_Depo_Secimi = new F10_Depo_Secimi();
				f10_Depo_Secimi.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi.ShowDialog() == DialogResult.OK)
				{
					_KaynakDepo = f10_Depo_Secimi._selecteditems[0];
					b_kaynak_depo.Text = _KaynakDepo.dep_adi;
					ListeleriYenile();
				}
			}
		}
		else
		{
			MessageBox.Show("Kaynak depo değiştirme yetkiniz yok.");
		}
	}

	private void b_onay_depo_Click(object sender, EventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiOnayDepoDegistirebilir")._GetBoolean)
		{
			if (AppBase.MikroVersiyonu >= 16)
			{
				F10_Depo_Secimi_V16 f10_Depo_Secimi_V = new F10_Depo_Secimi_V16();
				f10_Depo_Secimi_V.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi_V.ShowDialog() == DialogResult.OK)
				{
					_OnayDepo = f10_Depo_Secimi_V._selecteditems[0];
					b_onay_depo.Text = _OnayDepo.dep_adi;
				}
			}
			else
			{
				F10_Depo_Secimi f10_Depo_Secimi = new F10_Depo_Secimi();
				f10_Depo_Secimi.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi.ShowDialog() == DialogResult.OK)
				{
					_OnayDepo = f10_Depo_Secimi._selecteditems[0];
					b_onay_depo.Text = _OnayDepo.dep_adi;
				}
			}
		}
		else
		{
			MessageBox.Show("Onay depo değiştirme yetkiniz yok.");
		}
	}

	private void b_ret_depo_Click(object sender, EventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiRetDepoDegistirebilir")._GetBoolean)
		{
			if (AppBase.MikroVersiyonu >= 16)
			{
				F10_Depo_Secimi_V16 f10_Depo_Secimi_V = new F10_Depo_Secimi_V16();
				f10_Depo_Secimi_V.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi_V.ShowDialog() == DialogResult.OK)
				{
					_RetDepo = f10_Depo_Secimi_V._selecteditems[0];
					b_ret_depo.Text = _RetDepo.dep_adi;
				}
			}
			else
			{
				F10_Depo_Secimi f10_Depo_Secimi = new F10_Depo_Secimi();
				f10_Depo_Secimi.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi.ShowDialog() == DialogResult.OK)
				{
					_RetDepo = f10_Depo_Secimi._selecteditems[0];
					b_ret_depo.Text = _RetDepo.dep_adi;
				}
			}
		}
		else
		{
			MessageBox.Show("Ret depo değiştirme yetkiniz yok.");
		}
	}

	private void b_hurda_depo_Click(object sender, EventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiHurdaDepoDegistirebilir")._GetBoolean)
		{
			if (AppBase.MikroVersiyonu >= 16)
			{
				F10_Depo_Secimi_V16 f10_Depo_Secimi_V = new F10_Depo_Secimi_V16();
				f10_Depo_Secimi_V.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi_V.ShowDialog() == DialogResult.OK)
				{
					_HurdaDepo = f10_Depo_Secimi_V._selecteditems[0];
					b_hurda_depo.Text = _HurdaDepo.dep_adi;
				}
			}
			else
			{
				F10_Depo_Secimi f10_Depo_Secimi = new F10_Depo_Secimi();
				f10_Depo_Secimi.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi.ShowDialog() == DialogResult.OK)
				{
					_HurdaDepo = f10_Depo_Secimi._selecteditems[0];
					b_hurda_depo.Text = _HurdaDepo.dep_adi;
				}
			}
		}
		else
		{
			MessageBox.Show("Hurda depo değiştirme yetkiniz yok.");
		}
	}

	private void b_iade_depo_Click(object sender, EventArgs e)
	{
		if (_mikrouygulamabilgileri.KullaniciParametreleri._GetParametre("DepoKaliteKontrolYonetimiIadeDepoDegistirebilir")._GetBoolean)
		{
			if (AppBase.MikroVersiyonu >= 16)
			{
				F10_Depo_Secimi_V16 f10_Depo_Secimi_V = new F10_Depo_Secimi_V16();
				f10_Depo_Secimi_V.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi_V.ShowDialog() == DialogResult.OK)
				{
					_IadeDepo = f10_Depo_Secimi_V._selecteditems[0];
					b_iade_depo.Text = _IadeDepo.dep_adi;
				}
			}
			else
			{
				F10_Depo_Secimi f10_Depo_Secimi = new F10_Depo_Secimi();
				f10_Depo_Secimi.Setup(_mikrouygulamabilgileri, "DEPOLAR_CHOOSE_2", "", AllowMultiSelect: false, IlkAramaAktifOlsun: false);
				if (f10_Depo_Secimi.ShowDialog() == DialogResult.OK)
				{
					_IadeDepo = f10_Depo_Secimi._selecteditems[0];
					b_iade_depo.Text = _IadeDepo.dep_adi;
				}
			}
		}
		else
		{
			MessageBox.Show("İade depo değiştirme yetkiniz yok.");
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
		this.gc_Bekleyenler = new DevExpress.XtraGrid.GridControl();
		this.gv_Bekleyenler = new DevExpress.XtraGrid.Views.Grid.GridView();
		this.gc_kayit_no = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_tarih = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_evraktipi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_evrak_seri = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_evrak_sira = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_satir_no = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gcstok_kodu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_stok_ismi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_aciklama = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_parti_kodu = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_miktar = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_onay_miktari = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_islem_aciklamasi = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_ret_miktari = new DevExpress.XtraGrid.Columns.GridColumn();
		this.repositoryItemTextEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
		this.gc_ret_aciklama = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_hurda_miktari = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_hurda_aciklama = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_iade_miktari = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_iade_aciklama = new DevExpress.XtraGrid.Columns.GridColumn();
		this.gc_islem_yap = new DevExpress.XtraGrid.Columns.GridColumn();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
		this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.label_depolararasisevkevrakseri = new DevExpress.XtraEditors.LabelControl();
		this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
		this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
		this.sb_kaydet = new DevExpress.XtraEditors.SimpleButton();
		this.menuStrip1 = new System.Windows.Forms.MenuStrip();
		this.dosyaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.kapatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.işlemlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yenileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.seciliSatirlariListedenKaldirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.b_kaynak_depo = new DevExpress.XtraEditors.SimpleButton();
		this.b_onay_depo = new DevExpress.XtraEditors.SimpleButton();
		this.b_ret_depo = new DevExpress.XtraEditors.SimpleButton();
		this.b_hurda_depo = new DevExpress.XtraEditors.SimpleButton();
		this.b_iade_depo = new DevExpress.XtraEditors.SimpleButton();
		((System.ComponentModel.ISupportInitialize)this.gc_Bekleyenler).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.gv_Bekleyenler).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit1).BeginInit();
		this.tableLayoutPanel1.SuspendLayout();
		this.tableLayoutPanel2.SuspendLayout();
		this.menuStrip1.SuspendLayout();
		base.SuspendLayout();
		this.gc_Bekleyenler.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.gc_Bekleyenler.Location = new System.Drawing.Point(199, 28);
		this.gc_Bekleyenler.MainView = this.gv_Bekleyenler;
		this.gc_Bekleyenler.Name = "gc_Bekleyenler";
		this.gc_Bekleyenler.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[1] { this.repositoryItemTextEdit1 });
		this.gc_Bekleyenler.Size = new System.Drawing.Size(750, 397);
		this.gc_Bekleyenler.TabIndex = 0;
		this.gc_Bekleyenler.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[1] { this.gv_Bekleyenler });
		this.gv_Bekleyenler.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[20]
		{
			this.gc_kayit_no, this.gc_tarih, this.gc_evraktipi, this.gc_evrak_seri, this.gc_evrak_sira, this.gc_satir_no, this.gcstok_kodu, this.gc_stok_ismi, this.gc_aciklama, this.gc_parti_kodu,
			this.gc_miktar, this.gc_onay_miktari, this.gc_islem_aciklamasi, this.gc_ret_miktari, this.gc_ret_aciklama, this.gc_hurda_miktari, this.gc_hurda_aciklama, this.gc_iade_miktari, this.gc_iade_aciklama, this.gc_islem_yap
		});
		this.gv_Bekleyenler.GridControl = this.gc_Bekleyenler;
		this.gv_Bekleyenler.Name = "gv_Bekleyenler";
		this.gv_Bekleyenler.OptionsNavigation.EnterMoveNextColumn = true;
		this.gv_Bekleyenler.OptionsSelection.MultiSelect = true;
		this.gv_Bekleyenler.OptionsView.ColumnAutoWidth = false;
		this.gv_Bekleyenler.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(gv_Bekleyenler_CustomUnboundColumnData);
		this.gc_kayit_no.Caption = "Kayıt no";
		this.gc_kayit_no.FieldName = "sth_RECid_RECno";
		this.gc_kayit_no.Name = "gc_kayit_no";
		this.gc_kayit_no.OptionsColumn.AllowEdit = false;
		this.gc_kayit_no.OptionsColumn.AllowFocus = false;
		this.gc_kayit_no.Visible = true;
		this.gc_kayit_no.VisibleIndex = 0;
		this.gc_kayit_no.Width = 52;
		this.gc_tarih.Caption = "Tarih";
		this.gc_tarih.FieldName = "sth_tarih";
		this.gc_tarih.Name = "gc_tarih";
		this.gc_tarih.OptionsColumn.AllowEdit = false;
		this.gc_tarih.OptionsColumn.AllowFocus = false;
		this.gc_tarih.Visible = true;
		this.gc_tarih.VisibleIndex = 1;
		this.gc_tarih.Width = 59;
		this.gc_evraktipi.Caption = "Evrak tipi";
		this.gc_evraktipi.FieldName = "sth_evraktip";
		this.gc_evraktipi.Name = "gc_evraktipi";
		this.gc_evraktipi.OptionsColumn.AllowEdit = false;
		this.gc_evraktipi.OptionsColumn.AllowFocus = false;
		this.gc_evraktipi.Visible = true;
		this.gc_evraktipi.VisibleIndex = 2;
		this.gc_evraktipi.Width = 67;
		this.gc_evrak_seri.Caption = "Evrak seri";
		this.gc_evrak_seri.FieldName = "sth_evrakno_seri";
		this.gc_evrak_seri.Name = "gc_evrak_seri";
		this.gc_evrak_seri.OptionsColumn.AllowEdit = false;
		this.gc_evrak_seri.OptionsColumn.AllowFocus = false;
		this.gc_evrak_seri.Visible = true;
		this.gc_evrak_seri.VisibleIndex = 3;
		this.gc_evrak_seri.Width = 56;
		this.gc_evrak_sira.Caption = "Evrak sıra";
		this.gc_evrak_sira.FieldName = "sth_evrakno_sira";
		this.gc_evrak_sira.Name = "gc_evrak_sira";
		this.gc_evrak_sira.OptionsColumn.AllowEdit = false;
		this.gc_evrak_sira.OptionsColumn.AllowFocus = false;
		this.gc_evrak_sira.Visible = true;
		this.gc_evrak_sira.VisibleIndex = 4;
		this.gc_evrak_sira.Width = 59;
		this.gc_satir_no.Caption = "Satır no";
		this.gc_satir_no.FieldName = "sth_satirno";
		this.gc_satir_no.Name = "gc_satir_no";
		this.gc_satir_no.OptionsColumn.AllowEdit = false;
		this.gc_satir_no.OptionsColumn.AllowFocus = false;
		this.gc_satir_no.Visible = true;
		this.gc_satir_no.VisibleIndex = 5;
		this.gc_satir_no.Width = 46;
		this.gcstok_kodu.Caption = "Stok Kodu";
		this.gcstok_kodu.FieldName = "sth_stok_kod";
		this.gcstok_kodu.Name = "gcstok_kodu";
		this.gcstok_kodu.OptionsColumn.AllowEdit = false;
		this.gcstok_kodu.OptionsColumn.AllowFocus = false;
		this.gcstok_kodu.Visible = true;
		this.gcstok_kodu.VisibleIndex = 6;
		this.gcstok_kodu.Width = 69;
		this.gc_stok_ismi.Caption = "Stok ismi";
		this.gc_stok_ismi.FieldName = "sto_isim";
		this.gc_stok_ismi.Name = "gc_stok_ismi";
		this.gc_stok_ismi.OptionsColumn.AllowEdit = false;
		this.gc_stok_ismi.OptionsColumn.AllowFocus = false;
		this.gc_stok_ismi.Visible = true;
		this.gc_stok_ismi.VisibleIndex = 7;
		this.gc_stok_ismi.Width = 149;
		this.gc_aciklama.Caption = "Açıklama";
		this.gc_aciklama.FieldName = "sth_aciklama";
		this.gc_aciklama.Name = "gc_aciklama";
		this.gc_aciklama.OptionsColumn.AllowEdit = false;
		this.gc_aciklama.OptionsColumn.AllowFocus = false;
		this.gc_aciklama.Visible = true;
		this.gc_aciklama.VisibleIndex = 8;
		this.gc_aciklama.Width = 144;
		this.gc_parti_kodu.Caption = "Parti kodu";
		this.gc_parti_kodu.FieldName = "sth_parti_kodu";
		this.gc_parti_kodu.Name = "gc_parti_kodu";
		this.gc_parti_kodu.OptionsColumn.AllowEdit = false;
		this.gc_parti_kodu.OptionsColumn.AllowFocus = false;
		this.gc_parti_kodu.Visible = true;
		this.gc_parti_kodu.VisibleIndex = 9;
		this.gc_miktar.Caption = "Miktar";
		this.gc_miktar.FieldName = "sth_miktar";
		this.gc_miktar.Name = "gc_miktar";
		this.gc_miktar.OptionsColumn.AllowEdit = false;
		this.gc_miktar.OptionsColumn.AllowFocus = false;
		this.gc_miktar.Visible = true;
		this.gc_miktar.VisibleIndex = 10;
		this.gc_miktar.Width = 40;
		this.gc_onay_miktari.Caption = "Onay miktarı";
		this.gc_onay_miktari.FieldName = "onay_miktar";
		this.gc_onay_miktari.Name = "gc_onay_miktari";
		this.gc_onay_miktari.OptionsColumn.AllowEdit = false;
		this.gc_onay_miktari.OptionsColumn.AllowFocus = false;
		this.gc_onay_miktari.Visible = true;
		this.gc_onay_miktari.VisibleIndex = 11;
		this.gc_onay_miktari.Width = 68;
		this.gc_islem_aciklamasi.Caption = "İşlem açıklaması";
		this.gc_islem_aciklamasi.FieldName = "islem_aciklama";
		this.gc_islem_aciklamasi.Name = "gc_islem_aciklamasi";
		this.gc_islem_aciklamasi.Visible = true;
		this.gc_islem_aciklamasi.VisibleIndex = 12;
		this.gc_ret_miktari.Caption = "Ret miktarı";
		this.gc_ret_miktari.ColumnEdit = this.repositoryItemTextEdit1;
		this.gc_ret_miktari.FieldName = "gridColumn_Ret";
		this.gc_ret_miktari.Name = "gc_ret_miktari";
		this.gc_ret_miktari.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_ret_miktari.Visible = true;
		this.gc_ret_miktari.VisibleIndex = 13;
		this.gc_ret_miktari.Width = 59;
		this.repositoryItemTextEdit1.AutoHeight = false;
		this.repositoryItemTextEdit1.Name = "repositoryItemTextEdit1";
		this.gc_ret_aciklama.Caption = "Ret açıklama";
		this.gc_ret_aciklama.FieldName = "ret_aciklama";
		this.gc_ret_aciklama.Name = "gc_ret_aciklama";
		this.gc_ret_aciklama.Visible = true;
		this.gc_ret_aciklama.VisibleIndex = 14;
		this.gc_hurda_miktari.Caption = "Hurda miktarı";
		this.gc_hurda_miktari.ColumnEdit = this.repositoryItemTextEdit1;
		this.gc_hurda_miktari.FieldName = "gridColumn_Hurda";
		this.gc_hurda_miktari.Name = "gc_hurda_miktari";
		this.gc_hurda_miktari.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_hurda_miktari.Visible = true;
		this.gc_hurda_miktari.VisibleIndex = 15;
		this.gc_hurda_miktari.Width = 70;
		this.gc_hurda_aciklama.Caption = "Hurda açıklama";
		this.gc_hurda_aciklama.FieldName = "hurda_aciklama";
		this.gc_hurda_aciklama.Name = "gc_hurda_aciklama";
		this.gc_hurda_aciklama.Visible = true;
		this.gc_hurda_aciklama.VisibleIndex = 16;
		this.gc_iade_miktari.Caption = "İade miktarı";
		this.gc_iade_miktari.ColumnEdit = this.repositoryItemTextEdit1;
		this.gc_iade_miktari.FieldName = "gridColumn_Iade";
		this.gc_iade_miktari.Name = "gc_iade_miktari";
		this.gc_iade_miktari.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
		this.gc_iade_miktari.Visible = true;
		this.gc_iade_miktari.VisibleIndex = 17;
		this.gc_iade_miktari.Width = 63;
		this.gc_iade_aciklama.Caption = "İade açıklama";
		this.gc_iade_aciklama.FieldName = "iade_aciklama";
		this.gc_iade_aciklama.Name = "gc_iade_aciklama";
		this.gc_iade_aciklama.Visible = true;
		this.gc_iade_aciklama.VisibleIndex = 18;
		this.gc_islem_yap.Caption = "İşlem yap";
		this.gc_islem_yap.FieldName = "islemyap";
		this.gc_islem_yap.Name = "gc_islem_yap";
		this.gc_islem_yap.Visible = true;
		this.gc_islem_yap.VisibleIndex = 19;
		this.tableLayoutPanel1.ColumnCount = 2;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 196f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.Controls.Add(this.labelControl2, 1, 0);
		this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
		this.tableLayoutPanel1.Controls.Add(this.gc_Bekleyenler, 1, 1);
		this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 2;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(952, 428);
		this.tableLayoutPanel1.TabIndex = 5;
		this.labelControl2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl2.Location = new System.Drawing.Point(199, 3);
		this.labelControl2.Name = "labelControl2";
		this.labelControl2.Size = new System.Drawing.Size(750, 19);
		this.labelControl2.TabIndex = 6;
		this.labelControl2.Text = "BEKLEYEN ÜRÜNLER";
		this.tableLayoutPanel2.ColumnCount = 1;
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel2.Controls.Add(this.b_iade_depo, 0, 9);
		this.tableLayoutPanel2.Controls.Add(this.b_hurda_depo, 0, 7);
		this.tableLayoutPanel2.Controls.Add(this.b_ret_depo, 0, 5);
		this.tableLayoutPanel2.Controls.Add(this.b_onay_depo, 0, 3);
		this.tableLayoutPanel2.Controls.Add(this.b_kaynak_depo, 0, 1);
		this.tableLayoutPanel2.Controls.Add(this.label_depolararasisevkevrakseri, 0, 11);
		this.tableLayoutPanel2.Controls.Add(this.labelControl7, 0, 10);
		this.tableLayoutPanel2.Controls.Add(this.labelControl3, 0, 4);
		this.tableLayoutPanel2.Controls.Add(this.labelControl1, 0, 2);
		this.tableLayoutPanel2.Controls.Add(this.labelControl4, 0, 0);
		this.tableLayoutPanel2.Controls.Add(this.labelControl5, 0, 6);
		this.tableLayoutPanel2.Controls.Add(this.labelControl6, 0, 8);
		this.tableLayoutPanel2.Controls.Add(this.sb_kaydet, 0, 12);
		this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 28);
		this.tableLayoutPanel2.Name = "tableLayoutPanel2";
		this.tableLayoutPanel2.RowCount = 13;
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel2.Size = new System.Drawing.Size(190, 397);
		this.tableLayoutPanel2.TabIndex = 7;
		this.label_depolararasisevkevrakseri.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label_depolararasisevkevrakseri.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.label_depolararasisevkevrakseri.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.label_depolararasisevkevrakseri.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.label_depolararasisevkevrakseri.Location = new System.Drawing.Point(3, 308);
		this.label_depolararasisevkevrakseri.Name = "label_depolararasisevkevrakseri";
		this.label_depolararasisevkevrakseri.Size = new System.Drawing.Size(184, 19);
		this.label_depolararasisevkevrakseri.TabIndex = 17;
		this.label_depolararasisevkevrakseri.Text = "SERI";
		this.labelControl7.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelControl7.Appearance.BackColor = System.Drawing.Color.Silver;
		this.labelControl7.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl7.Appearance.ForeColor = System.Drawing.Color.FromArgb(192, 64, 0);
		this.labelControl7.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl7.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl7.Location = new System.Drawing.Point(3, 278);
		this.labelControl7.Name = "labelControl7";
		this.labelControl7.Size = new System.Drawing.Size(184, 24);
		this.labelControl7.TabIndex = 16;
		this.labelControl7.Text = "Sevk evrak seri";
		this.labelControl3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelControl3.Appearance.BackColor = System.Drawing.Color.Silver;
		this.labelControl3.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl3.Appearance.ForeColor = System.Drawing.Color.FromArgb(192, 64, 0);
		this.labelControl3.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl3.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl3.Location = new System.Drawing.Point(3, 113);
		this.labelControl3.Name = "labelControl3";
		this.labelControl3.Size = new System.Drawing.Size(184, 24);
		this.labelControl3.TabIndex = 10;
		this.labelControl3.Text = "Ret depo";
		this.labelControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelControl1.Appearance.BackColor = System.Drawing.Color.Silver;
		this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl1.Appearance.ForeColor = System.Drawing.Color.FromArgb(192, 64, 0);
		this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl1.Location = new System.Drawing.Point(3, 58);
		this.labelControl1.Name = "labelControl1";
		this.labelControl1.Size = new System.Drawing.Size(184, 24);
		this.labelControl1.TabIndex = 8;
		this.labelControl1.Text = "Onay depo";
		this.labelControl4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelControl4.Appearance.BackColor = System.Drawing.Color.Silver;
		this.labelControl4.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl4.Appearance.ForeColor = System.Drawing.Color.FromArgb(192, 64, 0);
		this.labelControl4.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl4.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl4.Location = new System.Drawing.Point(3, 3);
		this.labelControl4.Name = "labelControl4";
		this.labelControl4.Size = new System.Drawing.Size(184, 24);
		this.labelControl4.TabIndex = 6;
		this.labelControl4.Text = "Kaynak depo";
		this.labelControl5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelControl5.Appearance.BackColor = System.Drawing.Color.Silver;
		this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl5.Appearance.ForeColor = System.Drawing.Color.FromArgb(192, 64, 0);
		this.labelControl5.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl5.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl5.Location = new System.Drawing.Point(3, 168);
		this.labelControl5.Name = "labelControl5";
		this.labelControl5.Size = new System.Drawing.Size(184, 24);
		this.labelControl5.TabIndex = 12;
		this.labelControl5.Text = "Hurda depo";
		this.labelControl6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.labelControl6.Appearance.BackColor = System.Drawing.Color.Silver;
		this.labelControl6.Appearance.Font = new System.Drawing.Font("Tahoma", 10f, System.Drawing.FontStyle.Bold);
		this.labelControl6.Appearance.ForeColor = System.Drawing.Color.FromArgb(192, 64, 0);
		this.labelControl6.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
		this.labelControl6.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
		this.labelControl6.Location = new System.Drawing.Point(3, 223);
		this.labelControl6.Name = "labelControl6";
		this.labelControl6.Size = new System.Drawing.Size(184, 24);
		this.labelControl6.TabIndex = 14;
		this.labelControl6.Text = "İade depo";
		this.sb_kaydet.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.sb_kaydet.Appearance.Font = new System.Drawing.Font("Arial Black", 10f, System.Drawing.FontStyle.Bold);
		this.sb_kaydet.Appearance.Options.UseFont = true;
		this.sb_kaydet.Location = new System.Drawing.Point(3, 366);
		this.sb_kaydet.Name = "sb_kaydet";
		this.sb_kaydet.Size = new System.Drawing.Size(184, 28);
		this.sb_kaydet.TabIndex = 6;
		this.sb_kaydet.Text = "KAYDET";
		this.sb_kaydet.Click += new System.EventHandler(sb_kaydet_Click);
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
		this.işlemlerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.yenileToolStripMenuItem, this.seciliSatirlariListedenKaldirToolStripMenuItem });
		this.işlemlerToolStripMenuItem.Name = "işlemlerToolStripMenuItem";
		this.işlemlerToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
		this.işlemlerToolStripMenuItem.Text = "İşlemler";
		this.yenileToolStripMenuItem.Name = "yenileToolStripMenuItem";
		this.yenileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
		this.yenileToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
		this.yenileToolStripMenuItem.Text = "Yenile";
		this.yenileToolStripMenuItem.Click += new System.EventHandler(yenileToolStripMenuItem_Click);
		this.seciliSatirlariListedenKaldirToolStripMenuItem.Name = "seciliSatirlariListedenKaldirToolStripMenuItem";
		this.seciliSatirlariListedenKaldirToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
		this.seciliSatirlariListedenKaldirToolStripMenuItem.Text = "Seçili satırları listeden kaldır";
		this.seciliSatirlariListedenKaldirToolStripMenuItem.Click += new System.EventHandler(seciliSatirlariListedenKaldirToolStripMenuItem_Click);
		this.b_kaynak_depo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.b_kaynak_depo.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.b_kaynak_depo.Appearance.Options.UseFont = true;
		this.b_kaynak_depo.Location = new System.Drawing.Point(3, 33);
		this.b_kaynak_depo.Name = "b_kaynak_depo";
		this.b_kaynak_depo.Size = new System.Drawing.Size(184, 19);
		this.b_kaynak_depo.TabIndex = 15;
		this.b_kaynak_depo.Text = "kaynak depo";
		this.b_kaynak_depo.Click += new System.EventHandler(b_kaynak_depo_Click);
		this.b_onay_depo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.b_onay_depo.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.b_onay_depo.Appearance.Options.UseFont = true;
		this.b_onay_depo.Location = new System.Drawing.Point(3, 88);
		this.b_onay_depo.Name = "b_onay_depo";
		this.b_onay_depo.Size = new System.Drawing.Size(184, 19);
		this.b_onay_depo.TabIndex = 18;
		this.b_onay_depo.Text = "onay depo";
		this.b_onay_depo.Click += new System.EventHandler(b_onay_depo_Click);
		this.b_ret_depo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.b_ret_depo.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.b_ret_depo.Appearance.Options.UseFont = true;
		this.b_ret_depo.Location = new System.Drawing.Point(3, 143);
		this.b_ret_depo.Name = "b_ret_depo";
		this.b_ret_depo.Size = new System.Drawing.Size(184, 19);
		this.b_ret_depo.TabIndex = 19;
		this.b_ret_depo.Text = "ret depo";
		this.b_ret_depo.Click += new System.EventHandler(b_ret_depo_Click);
		this.b_hurda_depo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.b_hurda_depo.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.b_hurda_depo.Appearance.Options.UseFont = true;
		this.b_hurda_depo.Location = new System.Drawing.Point(3, 198);
		this.b_hurda_depo.Name = "b_hurda_depo";
		this.b_hurda_depo.Size = new System.Drawing.Size(184, 19);
		this.b_hurda_depo.TabIndex = 20;
		this.b_hurda_depo.Text = "hurda depo";
		this.b_hurda_depo.Click += new System.EventHandler(b_hurda_depo_Click);
		this.b_iade_depo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.b_iade_depo.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Bold);
		this.b_iade_depo.Appearance.Options.UseFont = true;
		this.b_iade_depo.Location = new System.Drawing.Point(3, 253);
		this.b_iade_depo.Name = "b_iade_depo";
		this.b_iade_depo.Size = new System.Drawing.Size(184, 19);
		this.b_iade_depo.TabIndex = 21;
		this.b_iade_depo.Text = "iade depo";
		this.b_iade_depo.Click += new System.EventHandler(b_iade_depo_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(952, 452);
		base.Controls.Add(this.tableLayoutPanel1);
		base.Controls.Add(this.menuStrip1);
		base.MainMenuStrip = this.menuStrip1;
		this.MinimumSize = new System.Drawing.Size(560, 460);
		base.Name = "IslemGirisi";
		this.Text = "Kalite Kontrol Yönetimi - İşlem girişi";
		((System.ComponentModel.ISupportInitialize)this.gc_Bekleyenler).EndInit();
		((System.ComponentModel.ISupportInitialize)this.gv_Bekleyenler).EndInit();
		((System.ComponentModel.ISupportInitialize)this.repositoryItemTextEdit1).EndInit();
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel2.ResumeLayout(false);
		this.menuStrip1.ResumeLayout(false);
		this.menuStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
