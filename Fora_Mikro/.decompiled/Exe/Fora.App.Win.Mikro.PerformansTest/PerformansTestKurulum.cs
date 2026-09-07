using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using Fora.Mikro;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Tahsilatlar;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.PerformansTest;

public class PerformansTestKurulum : Form
{
	public MikroUygulamaBilgileri _mikrouygulamabilgileri;

	private DateTime baslama_zamani;

	private DateTime bitis_zamani;

	private int olusturulacak_stok_sayisi;

	private string stok_kod_on_eki;

	private string stok_ismi_on_eki;

	private int olusturulacak_musteri_sayisi;

	private string musteri_cari_kod_on_eki;

	private string musteri_cari_unvan_on_eki;

	private int olusturulacak_tedarikci_sayisi;

	private string tedarikci_cari_kod_on_eki;

	private string tedarikci_cari_unvan_on_eki;

	private int alis_faturasi_sayisi;

	private string alis_faturasi_evrak_seri;

	private int alis_faturasi_satir_sayisi;

	private DateTime alis_faturasi_ilk_tarih;

	private DateTime alis_faturasi_son_tarih;

	private int satis_faturasi_sayisi;

	private string satis_faturasi_evrak_seri;

	private int satis_faturasi_satir_sayisi;

	private DateTime satis_faturasi_ilk_tarih;

	private DateTime satis_faturasi_son_tarih;

	private int alinan_siparis_sayisi;

	private string alinan_siparis_evrak_seri;

	private int alinan_siparis_satir_sayisi;

	private DateTime alinan_siparis_ilk_tarih;

	private DateTime alinan_siparis_son_tarih;

	private int tahsilat_makbuzu_sayisi;

	private string tahsilat_makbuzu_evrak_seri;

	private DateTime tahsilat_makbuzu_ilk_tarih;

	private DateTime tahsilat_makbuzu_son_tarih;

	private int tediye_makbuzu_sayisi;

	private string tediye_makbuzu_evrak_seri;

	private DateTime tediye_makbuzu_ilk_tarih;

	private DateTime tediye_makbuzu_son_tarih;

	private int StokAlisMinimumMiktar = 10;

	private int StokAlisMaksimumMiktar = 1000;

	private int StokSatisMinimumMiktar = 1;

	private int StokSatisMaksimumMiktar = 50;

	private int StokFiyatAlisMinimum = 1;

	private int StokFiyatAlisMaksumum = 100;

	private double TahsilatTutari = 2500.0;

	private double SatisKarMarji = 1.15;

	private List<Stok> EklenenStoklar;

	private List<Cari> EklenenMusteriler;

	private List<Cari> EklenenTedarikciler;

	private IContainer components;

	private Label label1;

	private Label label2;

	private SpinEdit se_olusturulacak_musteri_sayisi;

	private Label label3;

	private TextEdit te_musteri_cari_kod_on_eki;

	private TextEdit te_musteri_cari_unvan_on_eki;

	private Label label4;

	private Button b_kayitlari_olustur;

	private Button b_kayitlari_sil;

	private Label label5;

	private TextEdit te_stok_ismi_on_eki;

	private Label label6;

	private TextEdit te_stok_kod_on_eki;

	private Label label7;

	private SpinEdit se_olusturulacak_stok_sayisi;

	private Label label8;

	private Label label9;

	private TextEdit te_alis_faturasi_evrak_seri;

	private Label label10;

	private SpinEdit se_alis_faturasi_sayisi;

	private SpinEdit se_alis_faturasi_satir_sayisi;

	private Label label12;

	private DateTimePicker dtp_alis_faturasi_ilk_tarih;

	private Label label13;

	private Label label14;

	private DateTimePicker dtp_alis_faturasi_son_tarih;

	private Label label15;

	private Label label16;

	private DateTimePicker dtp_satis_faturasi_son_tarih;

	private Label label17;

	private DateTimePicker dtp_satis_faturasi_ilk_tarih;

	private SpinEdit se_satis_faturasi_satir_sayisi;

	private Label label18;

	private TextEdit te_satis_faturasi_evrak_seri;

	private Label label19;

	private SpinEdit se_satis_faturasi_sayisi;

	private TextEdit te_tedarikci_cari_unvan_on_eki;

	private Label label21;

	private TextEdit te_tedarikci_cari_kod_on_eki;

	private Label label22;

	private SpinEdit se_olusturulacak_tedarikci_sayisi;

	private Label label23;

	private Label label24;

	private Label label25;

	private DateTimePicker dtp_alinan_siparis_son_tarih;

	private Label label26;

	private DateTimePicker dtp_alinan_siparis_ilk_tarih;

	private SpinEdit se_alinan_siparis_satir_sayisi;

	private Label label27;

	private TextEdit te_alinan_siparis_evrak_seri;

	private Label label28;

	private SpinEdit se_alinan_siparis_sayisi;

	private Label label29;

	private Label label30;

	private Label label31;

	private DateTimePicker dtp_tahsilat_makbuzu_son_tarih;

	private Label label32;

	private DateTimePicker dtp_tahsilat_makbuzu_ilk_tarih;

	private TextEdit te_tahsilat_makbuzu_evrak_seri;

	private Label label34;

	private SpinEdit se_tahsilat_makbuzu_sayisi;

	private Label label35;

	private Label label11;

	private Label label20;

	private Label label36;

	private Label label37;

	private DateTimePicker dtp_tediye_makbuzu_son_tarih;

	private Label label38;

	private DateTimePicker dtp_tediye_makbuzu_ilk_tarih;

	private TextEdit te_tediye_makbuzu_evrak_seri;

	private Label label40;

	private SpinEdit se_tediye_makbuzu_sayisi;

	private Label label41;

	private Label label42;

	private Label label33;

	private Label label39;

	private Label label43;

	private Label label44;

	private Label label_stok_hareketi_sayisi;

	private Label label_cari_hesap_hareketi_sayisi;

	private Label label_siparis_hareketi_sayisi;

	private Label label45;

	private Label label46;

	private Label label_baslama_zamani;

	private Label label_bitis_zamani;

	private Label label49;

	private Label label_toplam_sure;

	private Label label51;

	private Label label_islem_bilgisi;

	private Label label_ilerleme;

	private System.Windows.Forms.ProgressBar progressBar1;

	private BackgroundWorker bw_cari_olusturma;

	private BackgroundWorker bw_stok_olusturma;

	private BackgroundWorker bw_tedarikci_olusturma;

	private BackgroundWorker bw_alis_faturasi_olusturma;

	private BackgroundWorker bw_satis_faturasi_olusturma;

	private BackgroundWorker bw_alinan_siparis_olusturma;

	private BackgroundWorker bw_tahsilat_makbuzu_olusturma;

	private BackgroundWorker bw_tediye_makbuzu_olusturma;

	private BackgroundWorker bw_kayitlari_silme;

	public PerformansTestKurulum(MikroUygulamaBilgileri mikrouygulamabilgileri)
	{
		InitializeComponent();
		_mikrouygulamabilgileri = mikrouygulamabilgileri;
		dtp_alis_faturasi_ilk_tarih.Value = new DateTime(DateTime.Now.Year, 1, 1);
		dtp_alis_faturasi_son_tarih.Value = new DateTime(DateTime.Now.Year, 12, 31);
		dtp_satis_faturasi_ilk_tarih.Value = new DateTime(DateTime.Now.Year, 1, 1);
		dtp_satis_faturasi_son_tarih.Value = new DateTime(DateTime.Now.Year, 12, 31);
		dtp_alinan_siparis_ilk_tarih.Value = new DateTime(DateTime.Now.Year, 1, 1);
		dtp_alinan_siparis_son_tarih.Value = new DateTime(DateTime.Now.Year, 12, 31);
		dtp_tahsilat_makbuzu_ilk_tarih.Value = new DateTime(DateTime.Now.Year, 1, 1);
		dtp_tahsilat_makbuzu_son_tarih.Value = new DateTime(DateTime.Now.Year, 12, 31);
		dtp_tediye_makbuzu_ilk_tarih.Value = new DateTime(DateTime.Now.Year, 1, 1);
		dtp_tediye_makbuzu_son_tarih.Value = new DateTime(DateTime.Now.Year, 12, 31);
		Hesapla();
	}

	private void b_kayitlari_sil_Click(object sender, EventArgs e)
	{
		baslama_zamani = DateTime.Now;
		label_baslama_zamani.Text = baslama_zamani.ToString("hh:mm:ss.f");
		label_islem_bilgisi.Text = "Kayıtları silme";
		progressBar1.Maximum = 4;
		progressBar1.Value = 0;
		b_kayitlari_olustur.Enabled = false;
		b_kayitlari_sil.Enabled = false;
		bw_kayitlari_silme.RunWorkerAsync();
	}

	private void genel_EditValueChanged(object sender, EventArgs e)
	{
		Hesapla();
	}

	private void Hesapla()
	{
		int num = 0;
		num += (int)se_alis_faturasi_sayisi.Value * (int)se_alis_faturasi_satir_sayisi.Value;
		num += (int)se_satis_faturasi_sayisi.Value * (int)se_satis_faturasi_satir_sayisi.Value;
		label_stok_hareketi_sayisi.Text = num.ToString("N0");
		int num2 = 0;
		num2 += (int)se_alis_faturasi_sayisi.Value + (int)se_satis_faturasi_satir_sayisi.Value + (int)se_tahsilat_makbuzu_sayisi.Value + (int)se_tediye_makbuzu_sayisi.Value;
		label_cari_hesap_hareketi_sayisi.Text = num2.ToString("N0");
		int num3 = 0;
		num3 += (int)se_alinan_siparis_sayisi.Value * (int)se_alinan_siparis_satir_sayisi.Value;
		label_siparis_hareketi_sayisi.Text = num3.ToString("N0");
	}

	private void b_kayitlari_olustur_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show("Bu işlemi test veritabanında yapmanız tavsiye edilir. Devam etmek istiyor musunuz?", "UYARI", MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			olusturulacak_stok_sayisi = (int)se_olusturulacak_stok_sayisi.Value;
			stok_kod_on_eki = te_stok_kod_on_eki.Text;
			stok_ismi_on_eki = te_stok_ismi_on_eki.Text;
			olusturulacak_musteri_sayisi = (int)se_olusturulacak_musteri_sayisi.Value;
			musteri_cari_kod_on_eki = te_musteri_cari_kod_on_eki.Text;
			musteri_cari_unvan_on_eki = te_musteri_cari_unvan_on_eki.Text;
			olusturulacak_tedarikci_sayisi = (int)se_olusturulacak_tedarikci_sayisi.Value;
			tedarikci_cari_kod_on_eki = te_tedarikci_cari_kod_on_eki.Text;
			tedarikci_cari_unvan_on_eki = te_tedarikci_cari_unvan_on_eki.Text;
			alis_faturasi_sayisi = (int)se_alis_faturasi_sayisi.Value;
			alis_faturasi_evrak_seri = te_alis_faturasi_evrak_seri.Text;
			alis_faturasi_satir_sayisi = (int)se_alis_faturasi_satir_sayisi.Value;
			alis_faturasi_ilk_tarih = dtp_alis_faturasi_ilk_tarih.Value;
			alis_faturasi_son_tarih = dtp_alis_faturasi_son_tarih.Value;
			satis_faturasi_sayisi = (int)se_satis_faturasi_sayisi.Value;
			satis_faturasi_evrak_seri = te_satis_faturasi_evrak_seri.Text;
			satis_faturasi_satir_sayisi = (int)se_satis_faturasi_satir_sayisi.Value;
			satis_faturasi_ilk_tarih = dtp_satis_faturasi_ilk_tarih.Value;
			satis_faturasi_son_tarih = dtp_satis_faturasi_son_tarih.Value;
			alinan_siparis_sayisi = (int)se_alinan_siparis_sayisi.Value;
			alinan_siparis_evrak_seri = te_alinan_siparis_evrak_seri.Text;
			alinan_siparis_satir_sayisi = (int)se_alinan_siparis_satir_sayisi.Value;
			alinan_siparis_ilk_tarih = dtp_alinan_siparis_ilk_tarih.Value;
			alinan_siparis_son_tarih = dtp_alinan_siparis_son_tarih.Value;
			tahsilat_makbuzu_sayisi = (int)se_tahsilat_makbuzu_sayisi.Value;
			tahsilat_makbuzu_evrak_seri = te_tahsilat_makbuzu_evrak_seri.Text;
			tahsilat_makbuzu_ilk_tarih = dtp_tahsilat_makbuzu_ilk_tarih.Value;
			tahsilat_makbuzu_son_tarih = dtp_tahsilat_makbuzu_son_tarih.Value;
			tediye_makbuzu_sayisi = (int)se_tediye_makbuzu_sayisi.Value;
			tediye_makbuzu_evrak_seri = te_tediye_makbuzu_evrak_seri.Text;
			tediye_makbuzu_ilk_tarih = dtp_tediye_makbuzu_ilk_tarih.Value;
			tediye_makbuzu_son_tarih = dtp_tediye_makbuzu_son_tarih.Value;
			baslama_zamani = DateTime.Now;
			label_baslama_zamani.Text = baslama_zamani.ToString("hh:mm:ss.f");
			b_kayitlari_olustur.Enabled = false;
			b_kayitlari_sil.Enabled = false;
			StokOlustur();
		}
	}

	private void StokOlustur()
	{
		label_islem_bilgisi.Text = "Stok oluşturma";
		progressBar1.Maximum = olusturulacak_stok_sayisi;
		progressBar1.Value = 0;
		bw_stok_olusturma.RunWorkerAsync();
	}

	private void bw_stok_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
		EklenenStoklar = new List<Stok>();
		for (int i = 1; i < olusturulacak_stok_sayisi + 1; i++)
		{
			Stok stok = new Stok();
			stok.sto_kod = stok_kod_on_eki + i;
			stok.sto_isim = stok_ismi_on_eki + " " + i;
			stok.sto_perakende_vergi_yeni = 4;
			stok.sto_toptan_vergi_yeni = 4;
			stok.sto_special1 = "FORA";
			stok.sto_special2 = "PTA2";
			stok.sto_birim1_ad = "ADET";
			stok.sto_birim1_katsayi = 1.0;
			stok.ekleme_bilgileri.BirimFiyat.FiyatBrut = GenelUtilityWin.RandomNumber.Between(StokFiyatAlisMinimum, StokFiyatAlisMaksumum);
			try
			{
				EklenenStoklar.Add(stok);
				StokData.YeniStokKaydet(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName, stok, 1);
			}
			catch
			{
			}
			bw_stok_olusturma.ReportProgress(i);
		}
	}

	private void bw_stok_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + olusturulacak_stok_sayisi;
	}

	private void bw_stok_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		MusteriOlustur();
	}

	private void MusteriOlustur()
	{
		label_islem_bilgisi.Text = "Müşteri oluşturma";
		progressBar1.Maximum = olusturulacak_musteri_sayisi;
		progressBar1.Value = 0;
		bw_cari_olusturma.RunWorkerAsync();
	}

	private void bw_musteri_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
		EklenenMusteriler = new List<Cari>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		for (int i = 1; i < olusturulacak_musteri_sayisi + 1; i++)
		{
			Cari cari = new Cari();
			cari.cari_kod = musteri_cari_kod_on_eki + i;
			cari.cari_unvan1 = musteri_cari_unvan_on_eki + i;
			cari.cari_special1 = "FORA";
			cari.cari_special2 = "PTA2";
			try
			{
				EklenenMusteriler.Add(cari);
				cari.KaydetYeniCari(sqlDB.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, 1);
			}
			catch
			{
			}
			bw_cari_olusturma.ReportProgress(i);
		}
		sqlDB.ConnectionClose();
	}

	private void bw_musteri_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + olusturulacak_musteri_sayisi;
	}

	private void bw_musteri_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		TedarikciOlustur();
	}

	private void TedarikciOlustur()
	{
		label_islem_bilgisi.Text = "Tedarikçi oluşturma";
		progressBar1.Maximum = olusturulacak_tedarikci_sayisi;
		progressBar1.Value = 0;
		bw_tedarikci_olusturma.RunWorkerAsync();
	}

	private void bw_tedarikci_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
		EklenenTedarikciler = new List<Cari>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		for (int i = 1; i < olusturulacak_tedarikci_sayisi + 1; i++)
		{
			Cari cari = new Cari();
			cari.cari_kod = tedarikci_cari_kod_on_eki + i;
			cari.cari_unvan1 = tedarikci_cari_unvan_on_eki + i;
			cari.cari_special1 = "FORA";
			cari.cari_special2 = "PTA2";
			try
			{
				EklenenTedarikciler.Add(cari);
				cari.KaydetYeniCari(sqlDB.Connection, _mikrouygulamabilgileri.MikroFirmaDBName, 1);
			}
			catch
			{
			}
			bw_tedarikci_olusturma.ReportProgress(i);
		}
		sqlDB.ConnectionClose();
	}

	private void bw_tedarikci_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + olusturulacak_tedarikci_sayisi;
	}

	private void bw_tedarikci_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		AlisFaturasiOlustur();
	}

	private void AlisFaturasiOlustur()
	{
		label_islem_bilgisi.Text = "Alış faturası oluşturma";
		progressBar1.Maximum = alis_faturasi_sayisi;
		progressBar1.Value = 0;
		bw_alis_faturasi_olusturma.RunWorkerAsync();
	}

	private void bw_alis_faturasi_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		for (int i = 1; i < alis_faturasi_sayisi + 1; i++)
		{
			Evrak evrak = new Evrak(enum_GenelEvrakTipleri.AlisFaturasi, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
			evrak.SetEvrakNoSeri(alis_faturasi_evrak_seri);
			evrak.SetDegistirSpecialAlan1("FORA");
			evrak.SetDegistirSpecialAlan2("PTA2");
			int num = (int)(alis_faturasi_son_tarih - alis_faturasi_ilk_tarih).TotalDays;
			DateTime dateTime = alis_faturasi_ilk_tarih.AddDays(GenelUtilityWin.RandomNumber.Between(0, num - 1));
			evrak.SetEvrakTarihi(dateTime);
			evrak.SetBelgeTarihi(dateTime);
			int index = GenelUtilityWin.RandomNumber.Between(0, olusturulacak_tedarikci_sayisi - 1);
			evrak.cari = EklenenTedarikciler[index];
			for (int j = 0; j < alis_faturasi_satir_sayisi; j++)
			{
				int index2 = GenelUtilityWin.RandomNumber.Between(0, olusturulacak_stok_sayisi - 1);
				EklenenStoklar[index2].ekleme_bilgileri.Miktar = GenelUtilityWin.RandomNumber.Between(StokAlisMinimumMiktar, StokAlisMaksimumMiktar);
				evrak.AddUrun(EklenenStoklar[index2]);
			}
			SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
			if (EvrakData.EvrakKaydet(sqlDB.Connection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false) == -1)
			{
				sqlTransaction.Rollback();
			}
			else
			{
				sqlTransaction.Commit();
			}
			bw_alis_faturasi_olusturma.ReportProgress(i);
		}
		sqlDB.ConnectionClose();
	}

	private void bw_alis_faturasi_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + alis_faturasi_sayisi;
	}

	private void bw_alis_faturasi_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		SatisFaturasiOlustur();
	}

	private void SatisFaturasiOlustur()
	{
		label_islem_bilgisi.Text = "Satış faturası oluşturma";
		progressBar1.Maximum = satis_faturasi_sayisi;
		progressBar1.Value = 0;
		foreach (Stok item in EklenenStoklar)
		{
			item.ekleme_bilgileri.BirimFiyat.FiyatBrut = item.ekleme_bilgileri.BirimFiyat.FiyatBrut * SatisKarMarji;
		}
		bw_satis_faturasi_olusturma.RunWorkerAsync();
	}

	private void bw_satis_faturasi_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		for (int i = 1; i < satis_faturasi_sayisi + 1; i++)
		{
			Evrak evrak = new Evrak(enum_GenelEvrakTipleri.SatisFaturasi, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
			evrak.SetEvrakNoSeri(satis_faturasi_evrak_seri);
			evrak.SetDegistirSpecialAlan1("FORA");
			evrak.SetDegistirSpecialAlan2("PTA2");
			int num = (int)(satis_faturasi_son_tarih - satis_faturasi_ilk_tarih).TotalDays;
			DateTime dateTime = satis_faturasi_ilk_tarih.AddDays(GenelUtilityWin.RandomNumber.Between(0, num - 1));
			evrak.SetEvrakTarihi(dateTime);
			evrak.SetBelgeTarihi(dateTime);
			int index = GenelUtilityWin.RandomNumber.Between(0, olusturulacak_musteri_sayisi - 1);
			evrak.cari = EklenenMusteriler[index];
			for (int j = 0; j < satis_faturasi_satir_sayisi; j++)
			{
				int index2 = GenelUtilityWin.RandomNumber.Between(0, olusturulacak_stok_sayisi - 1);
				EklenenStoklar[index2].ekleme_bilgileri.Miktar = GenelUtilityWin.RandomNumber.Between(StokSatisMinimumMiktar, StokSatisMaksimumMiktar);
				evrak.AddUrun(EklenenStoklar[index2]);
			}
			SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
			if (EvrakData.EvrakKaydet(sqlDB.Connection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false) == -1)
			{
				sqlTransaction.Rollback();
			}
			else
			{
				sqlTransaction.Commit();
			}
			bw_satis_faturasi_olusturma.ReportProgress(i);
		}
		sqlDB.ConnectionClose();
	}

	private void bw_satis_faturasi_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + satis_faturasi_sayisi;
	}

	private void bw_satis_faturasi_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		AlinanSiparisOlustur();
	}

	private void AlinanSiparisOlustur()
	{
		label_islem_bilgisi.Text = "Alınan sipariş oluşturma";
		progressBar1.Maximum = alinan_siparis_sayisi;
		progressBar1.Value = 0;
		bw_alinan_siparis_olusturma.RunWorkerAsync();
	}

	private void bw_alinan_siparis_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		for (int i = 1; i < alinan_siparis_sayisi + 1; i++)
		{
			Evrak evrak = new Evrak(enum_GenelEvrakTipleri.AlinanSiparis, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
			evrak.SetEvrakNoSeri(alinan_siparis_evrak_seri);
			evrak.SetDegistirSpecialAlan1("FORA");
			evrak.SetDegistirSpecialAlan2("PTA2");
			int num = (int)(alinan_siparis_son_tarih - alinan_siparis_ilk_tarih).TotalDays;
			DateTime dateTime = alinan_siparis_ilk_tarih.AddDays(GenelUtilityWin.RandomNumber.Between(0, num - 1));
			evrak.SetEvrakTarihi(dateTime);
			evrak.SetBelgeTarihi(dateTime);
			int index = GenelUtilityWin.RandomNumber.Between(0, olusturulacak_musteri_sayisi - 1);
			evrak.cari = EklenenMusteriler[index];
			for (int j = 0; j < alinan_siparis_satir_sayisi; j++)
			{
				int index2 = GenelUtilityWin.RandomNumber.Between(0, olusturulacak_stok_sayisi - 1);
				EklenenStoklar[index2].ekleme_bilgileri.Miktar = GenelUtilityWin.RandomNumber.Between(StokSatisMinimumMiktar, StokSatisMaksimumMiktar);
				evrak.AddUrun(EklenenStoklar[index2]);
			}
			SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
			if (EvrakData.EvrakKaydet(sqlDB.Connection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false) == -1)
			{
				sqlTransaction.Rollback();
			}
			else
			{
				sqlTransaction.Commit();
			}
			bw_alinan_siparis_olusturma.ReportProgress(i);
		}
		sqlDB.ConnectionClose();
	}

	private void bw_alinan_siparis_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + alinan_siparis_sayisi;
	}

	private void bw_alinan_siparis_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		TahsilatMakbuzuOlustur();
	}

	private void TahsilatMakbuzuOlustur()
	{
		label_islem_bilgisi.Text = "Tahsilat makbuzu oluşturma";
		progressBar1.Maximum = tahsilat_makbuzu_sayisi;
		progressBar1.Value = 0;
		bw_tahsilat_makbuzu_olusturma.RunWorkerAsync();
	}

	private void bw_tahsilat_makbuzu_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		for (int i = 1; i < tahsilat_makbuzu_sayisi + 1; i++)
		{
			Evrak evrak = new Evrak(enum_GenelEvrakTipleri.Tahsilat, YeniKayitMi: true, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
			evrak.SetEvrakNoSeri(tahsilat_makbuzu_evrak_seri);
			evrak.SetDegistirSpecialAlan1("FORA");
			evrak.SetDegistirSpecialAlan2("PTA2");
			int num = (int)(tahsilat_makbuzu_son_tarih - tahsilat_makbuzu_ilk_tarih).TotalDays;
			DateTime dateTime = tahsilat_makbuzu_ilk_tarih.AddDays(GenelUtilityWin.RandomNumber.Between(0, num - 1));
			evrak.SetEvrakTarihi(dateTime);
			evrak.SetBelgeTarihi(dateTime);
			int index = GenelUtilityWin.RandomNumber.Between(0, olusturulacak_musteri_sayisi - 1);
			evrak.cari = EklenenMusteriler[index];
			Tahsilat tahsilat = new Tahsilat();
			tahsilat.cinsi = enum_tahsilat_cinsi.Nakit;
			tahsilat.kasa_banka_kodu = "NAKİT.01";
			tahsilat.vadesi = dateTime;
			tahsilat.tutar = TahsilatTutari;
			evrak.AddTahsilat(tahsilat);
			SqlTransaction sqlTransaction = sqlDB.Connection.BeginTransaction();
			if (EvrakData.EvrakKaydet(sqlDB.Connection, sqlTransaction, _mikrouygulamabilgileri.MikroFirmaDBName, evrak, EArsivAktif: false) == -1)
			{
				sqlTransaction.Rollback();
			}
			else
			{
				sqlTransaction.Commit();
			}
			bw_tahsilat_makbuzu_olusturma.ReportProgress(i);
		}
		sqlDB.ConnectionClose();
	}

	private void bw_tahsilat_makbuzu_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + tahsilat_makbuzu_sayisi;
	}

	private void bw_tahsilat_makbuzu_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		TediyeMakbuzuOlustur();
	}

	private void TediyeMakbuzuOlustur()
	{
		label_islem_bilgisi.Text = "Tediye makbuzu oluşturma";
		progressBar1.Maximum = tediye_makbuzu_sayisi;
		progressBar1.Value = 0;
		bw_tediye_makbuzu_olusturma.RunWorkerAsync();
	}

	private void bw_tediye_makbuzu_olusturma_DoWork(object sender, DoWorkEventArgs e)
	{
	}

	private void bw_tediye_makbuzu_olusturma_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/" + tediye_makbuzu_sayisi;
	}

	private void bw_tediye_makbuzu_olusturma_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		OlusturmaBitir();
	}

	private void OlusturmaBitir()
	{
		label_islem_bilgisi.Text = "İşlem tamamlandı";
		progressBar1.Maximum = 100;
		progressBar1.Value = 0;
		label_ilerleme.Text = "- / -";
		bitis_zamani = DateTime.Now;
		label_bitis_zamani.Text = bitis_zamani.ToString("hh:mm:ss.f");
		TimeSpan timeSpan = bitis_zamani - baslama_zamani;
		label_toplam_sure.Text = timeSpan.ToString("mm\\:ss\\.ff");
		b_kayitlari_olustur.Enabled = true;
		b_kayitlari_sil.Enabled = true;
	}

	private void bw_kayitlari_silme_DoWork(object sender, DoWorkEventArgs e)
	{
		string text = "";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(_mikrouygulamabilgileri.baglantibilgileri, _mikrouygulamabilgileri.MikroFirmaDBName);
		text = "DELETE FROM STOK_HAREKETLERI WHERE sth_special1='FORA' AND sth_special2='PTA2'";
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = text;
			sqlCommand.CommandTimeout = 600;
			sqlCommand.ExecuteNonQuery();
		}
		bw_kayitlari_silme.ReportProgress(1);
		text = "DELETE FROM CARI_HESAP_HAREKETLERI WHERE cha_special1='FORA' AND cha_special2='PTA2'";
		using (SqlCommand sqlCommand2 = sqlDB.Connection.CreateCommand())
		{
			sqlCommand2.CommandText = text;
			sqlCommand2.CommandTimeout = 600;
			sqlCommand2.ExecuteNonQuery();
		}
		bw_kayitlari_silme.ReportProgress(2);
		text = "DELETE FROM CARI_HESAPLAR WHERE cari_special1='FORA' AND cari_special2='PTA2'";
		using (SqlCommand sqlCommand3 = sqlDB.Connection.CreateCommand())
		{
			sqlCommand3.CommandText = text;
			sqlCommand3.CommandTimeout = 600;
			sqlCommand3.ExecuteNonQuery();
		}
		bw_kayitlari_silme.ReportProgress(3);
		text = "DELETE FROM STOKLAR WHERE sto_special1='FORA' AND sto_special2='PTA2'";
		using (SqlCommand sqlCommand4 = sqlDB.Connection.CreateCommand())
		{
			sqlCommand4.CommandText = text;
			sqlCommand4.CommandTimeout = 600;
			sqlCommand4.ExecuteNonQuery();
		}
		bw_kayitlari_silme.ReportProgress(4);
		sqlDB.ConnectionClose();
	}

	private void bw_kayitlari_silme_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		progressBar1.Value = e.ProgressPercentage;
		label_ilerleme.Text = e.ProgressPercentage + "/ 8";
	}

	private void bw_kayitlari_silme_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		b_kayitlari_olustur.Enabled = true;
		b_kayitlari_sil.Enabled = true;
		label_islem_bilgisi.Text = "İşlem tamamlandı";
		progressBar1.Maximum = 100;
		progressBar1.Value = 0;
		label_ilerleme.Text = "- / -";
		bitis_zamani = DateTime.Now;
		label_bitis_zamani.Text = bitis_zamani.ToString("hh:mm:ss.f");
		TimeSpan timeSpan = bitis_zamani - baslama_zamani;
		label_toplam_sure.Text = timeSpan.ToString("mm\\:ss\\.ff");
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.se_olusturulacak_musteri_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label3 = new System.Windows.Forms.Label();
		this.te_musteri_cari_kod_on_eki = new DevExpress.XtraEditors.TextEdit();
		this.te_musteri_cari_unvan_on_eki = new DevExpress.XtraEditors.TextEdit();
		this.label4 = new System.Windows.Forms.Label();
		this.b_kayitlari_olustur = new System.Windows.Forms.Button();
		this.b_kayitlari_sil = new System.Windows.Forms.Button();
		this.label5 = new System.Windows.Forms.Label();
		this.te_stok_ismi_on_eki = new DevExpress.XtraEditors.TextEdit();
		this.label6 = new System.Windows.Forms.Label();
		this.te_stok_kod_on_eki = new DevExpress.XtraEditors.TextEdit();
		this.label7 = new System.Windows.Forms.Label();
		this.se_olusturulacak_stok_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.te_alis_faturasi_evrak_seri = new DevExpress.XtraEditors.TextEdit();
		this.label10 = new System.Windows.Forms.Label();
		this.se_alis_faturasi_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.se_alis_faturasi_satir_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label12 = new System.Windows.Forms.Label();
		this.dtp_alis_faturasi_ilk_tarih = new System.Windows.Forms.DateTimePicker();
		this.label13 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.dtp_alis_faturasi_son_tarih = new System.Windows.Forms.DateTimePicker();
		this.label15 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.dtp_satis_faturasi_son_tarih = new System.Windows.Forms.DateTimePicker();
		this.label17 = new System.Windows.Forms.Label();
		this.dtp_satis_faturasi_ilk_tarih = new System.Windows.Forms.DateTimePicker();
		this.se_satis_faturasi_satir_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label18 = new System.Windows.Forms.Label();
		this.te_satis_faturasi_evrak_seri = new DevExpress.XtraEditors.TextEdit();
		this.label19 = new System.Windows.Forms.Label();
		this.se_satis_faturasi_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.te_tedarikci_cari_unvan_on_eki = new DevExpress.XtraEditors.TextEdit();
		this.label21 = new System.Windows.Forms.Label();
		this.te_tedarikci_cari_kod_on_eki = new DevExpress.XtraEditors.TextEdit();
		this.label22 = new System.Windows.Forms.Label();
		this.se_olusturulacak_tedarikci_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label23 = new System.Windows.Forms.Label();
		this.label24 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.dtp_alinan_siparis_son_tarih = new System.Windows.Forms.DateTimePicker();
		this.label26 = new System.Windows.Forms.Label();
		this.dtp_alinan_siparis_ilk_tarih = new System.Windows.Forms.DateTimePicker();
		this.se_alinan_siparis_satir_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label27 = new System.Windows.Forms.Label();
		this.te_alinan_siparis_evrak_seri = new DevExpress.XtraEditors.TextEdit();
		this.label28 = new System.Windows.Forms.Label();
		this.se_alinan_siparis_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label29 = new System.Windows.Forms.Label();
		this.label30 = new System.Windows.Forms.Label();
		this.label31 = new System.Windows.Forms.Label();
		this.dtp_tahsilat_makbuzu_son_tarih = new System.Windows.Forms.DateTimePicker();
		this.label32 = new System.Windows.Forms.Label();
		this.dtp_tahsilat_makbuzu_ilk_tarih = new System.Windows.Forms.DateTimePicker();
		this.te_tahsilat_makbuzu_evrak_seri = new DevExpress.XtraEditors.TextEdit();
		this.label34 = new System.Windows.Forms.Label();
		this.se_tahsilat_makbuzu_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label35 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label20 = new System.Windows.Forms.Label();
		this.label36 = new System.Windows.Forms.Label();
		this.label37 = new System.Windows.Forms.Label();
		this.dtp_tediye_makbuzu_son_tarih = new System.Windows.Forms.DateTimePicker();
		this.label38 = new System.Windows.Forms.Label();
		this.dtp_tediye_makbuzu_ilk_tarih = new System.Windows.Forms.DateTimePicker();
		this.te_tediye_makbuzu_evrak_seri = new DevExpress.XtraEditors.TextEdit();
		this.label40 = new System.Windows.Forms.Label();
		this.se_tediye_makbuzu_sayisi = new DevExpress.XtraEditors.SpinEdit();
		this.label41 = new System.Windows.Forms.Label();
		this.label42 = new System.Windows.Forms.Label();
		this.label33 = new System.Windows.Forms.Label();
		this.label39 = new System.Windows.Forms.Label();
		this.label43 = new System.Windows.Forms.Label();
		this.label44 = new System.Windows.Forms.Label();
		this.label_stok_hareketi_sayisi = new System.Windows.Forms.Label();
		this.label_cari_hesap_hareketi_sayisi = new System.Windows.Forms.Label();
		this.label_siparis_hareketi_sayisi = new System.Windows.Forms.Label();
		this.label45 = new System.Windows.Forms.Label();
		this.label46 = new System.Windows.Forms.Label();
		this.label_baslama_zamani = new System.Windows.Forms.Label();
		this.label_bitis_zamani = new System.Windows.Forms.Label();
		this.label49 = new System.Windows.Forms.Label();
		this.label_toplam_sure = new System.Windows.Forms.Label();
		this.label51 = new System.Windows.Forms.Label();
		this.label_islem_bilgisi = new System.Windows.Forms.Label();
		this.label_ilerleme = new System.Windows.Forms.Label();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.bw_cari_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_stok_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_tedarikci_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_alis_faturasi_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_satis_faturasi_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_alinan_siparis_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_tahsilat_makbuzu_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_tediye_makbuzu_olusturma = new System.ComponentModel.BackgroundWorker();
		this.bw_kayitlari_silme = new System.ComponentModel.BackgroundWorker();
		((System.ComponentModel.ISupportInitialize)this.se_olusturulacak_musteri_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_musteri_cari_kod_on_eki.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_musteri_cari_unvan_on_eki.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_stok_ismi_on_eki.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_stok_kod_on_eki.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_olusturulacak_stok_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_alis_faturasi_evrak_seri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_alis_faturasi_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_alis_faturasi_satir_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_satis_faturasi_satir_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_satis_faturasi_evrak_seri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_satis_faturasi_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tedarikci_cari_unvan_on_eki.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tedarikci_cari_kod_on_eki.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_olusturulacak_tedarikci_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_alinan_siparis_satir_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_alinan_siparis_evrak_seri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_alinan_siparis_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tahsilat_makbuzu_evrak_seri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_tahsilat_makbuzu_sayisi.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.te_tediye_makbuzu_evrak_seri.Properties).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.se_tediye_makbuzu_sayisi.Properties).BeginInit();
		base.SuspendLayout();
		this.label1.BackColor = System.Drawing.SystemColors.GrayText;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label1.ForeColor = System.Drawing.SystemColors.Menu;
		this.label1.Location = new System.Drawing.Point(12, 58);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(739, 23);
		this.label1.TabIndex = 0;
		this.label1.Text = "Müşteri";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(17, 87);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(126, 13);
		this.label2.TabIndex = 1;
		this.label2.Text = "Oluşturulacak cari sayısı :";
		this.se_olusturulacak_musteri_sayisi.EditValue = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.se_olusturulacak_musteri_sayisi.Location = new System.Drawing.Point(149, 84);
		this.se_olusturulacak_musteri_sayisi.Name = "se_olusturulacak_musteri_sayisi";
		this.se_olusturulacak_musteri_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_olusturulacak_musteri_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_olusturulacak_musteri_sayisi.TabIndex = 4;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(228, 87);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(84, 13);
		this.label3.TabIndex = 3;
		this.label3.Text = "Cari kod ön eki :";
		this.te_musteri_cari_kod_on_eki.EditValue = "CAR.";
		this.te_musteri_cari_kod_on_eki.Location = new System.Drawing.Point(318, 84);
		this.te_musteri_cari_kod_on_eki.Name = "te_musteri_cari_kod_on_eki";
		this.te_musteri_cari_kod_on_eki.Size = new System.Drawing.Size(67, 20);
		this.te_musteri_cari_kod_on_eki.TabIndex = 5;
		this.te_musteri_cari_unvan_on_eki.EditValue = "MÜŞTERİ";
		this.te_musteri_cari_unvan_on_eki.Location = new System.Drawing.Point(511, 84);
		this.te_musteri_cari_unvan_on_eki.Name = "te_musteri_cari_unvan_on_eki";
		this.te_musteri_cari_unvan_on_eki.Size = new System.Drawing.Size(240, 20);
		this.te_musteri_cari_unvan_on_eki.TabIndex = 6;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(409, 87);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(96, 13);
		this.label4.TabIndex = 5;
		this.label4.Text = "Cari ünvan ön eki :";
		this.b_kayitlari_olustur.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.b_kayitlari_olustur.Location = new System.Drawing.Point(583, 734);
		this.b_kayitlari_olustur.Name = "b_kayitlari_olustur";
		this.b_kayitlari_olustur.Size = new System.Drawing.Size(168, 23);
		this.b_kayitlari_olustur.TabIndex = 33;
		this.b_kayitlari_olustur.Text = "KAYITLARI OLUŞTUR";
		this.b_kayitlari_olustur.UseVisualStyleBackColor = true;
		this.b_kayitlari_olustur.Click += new System.EventHandler(b_kayitlari_olustur_Click);
		this.b_kayitlari_sil.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.b_kayitlari_sil.Location = new System.Drawing.Point(583, 763);
		this.b_kayitlari_sil.Name = "b_kayitlari_sil";
		this.b_kayitlari_sil.Size = new System.Drawing.Size(168, 23);
		this.b_kayitlari_sil.TabIndex = 34;
		this.b_kayitlari_sil.Text = "KAYITLARI SİL";
		this.b_kayitlari_sil.UseVisualStyleBackColor = true;
		this.b_kayitlari_sil.Click += new System.EventHandler(b_kayitlari_sil_Click);
		this.label5.BackColor = System.Drawing.SystemColors.GrayText;
		this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label5.ForeColor = System.Drawing.SystemColors.Menu;
		this.label5.Location = new System.Drawing.Point(12, 9);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(739, 23);
		this.label5.TabIndex = 9;
		this.label5.Text = "Stok";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_stok_ismi_on_eki.EditValue = "TEST STOĞU";
		this.te_stok_ismi_on_eki.Location = new System.Drawing.Point(511, 35);
		this.te_stok_ismi_on_eki.Name = "te_stok_ismi_on_eki";
		this.te_stok_ismi_on_eki.Size = new System.Drawing.Size(240, 20);
		this.te_stok_ismi_on_eki.TabIndex = 3;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(418, 38);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(87, 13);
		this.label6.TabIndex = 14;
		this.label6.Text = "Stok ismi ön eki :";
		this.te_stok_kod_on_eki.EditValue = "STO.";
		this.te_stok_kod_on_eki.Location = new System.Drawing.Point(318, 35);
		this.te_stok_kod_on_eki.Name = "te_stok_kod_on_eki";
		this.te_stok_kod_on_eki.Size = new System.Drawing.Size(67, 20);
		this.te_stok_kod_on_eki.TabIndex = 2;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(218, 38);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(94, 13);
		this.label7.TabIndex = 12;
		this.label7.Text = "Stok kodu ön eki :";
		this.se_olusturulacak_stok_sayisi.EditValue = new decimal(new int[4] { 800, 0, 0, 0 });
		this.se_olusturulacak_stok_sayisi.Location = new System.Drawing.Point(149, 35);
		this.se_olusturulacak_stok_sayisi.Name = "se_olusturulacak_stok_sayisi";
		this.se_olusturulacak_stok_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_olusturulacak_stok_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_olusturulacak_stok_sayisi.TabIndex = 1;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(14, 38);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(129, 13);
		this.label8.TabIndex = 10;
		this.label8.Text = "Oluşturulacak stok sayısı :";
		this.label9.BackColor = System.Drawing.SystemColors.GrayText;
		this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label9.ForeColor = System.Drawing.SystemColors.Menu;
		this.label9.Location = new System.Drawing.Point(12, 156);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(739, 23);
		this.label9.TabIndex = 16;
		this.label9.Text = "Alış faturası";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.te_alis_faturasi_evrak_seri.EditValue = "A";
		this.te_alis_faturasi_evrak_seri.Location = new System.Drawing.Point(247, 182);
		this.te_alis_faturasi_evrak_seri.Name = "te_alis_faturasi_evrak_seri";
		this.te_alis_faturasi_evrak_seri.Size = new System.Drawing.Size(33, 20);
		this.te_alis_faturasi_evrak_seri.TabIndex = 11;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(174, 185);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(67, 13);
		this.label10.TabIndex = 19;
		this.label10.Text = "Evrak serisi :";
		this.se_alis_faturasi_sayisi.EditValue = new decimal(new int[4] { 1000, 0, 0, 0 });
		this.se_alis_faturasi_sayisi.Location = new System.Drawing.Point(97, 182);
		this.se_alis_faturasi_sayisi.Name = "se_alis_faturasi_sayisi";
		this.se_alis_faturasi_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_alis_faturasi_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_alis_faturasi_sayisi.TabIndex = 10;
		this.se_alis_faturasi_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.se_alis_faturasi_satir_sayisi.EditValue = new decimal(new int[4] { 15, 0, 0, 0 });
		this.se_alis_faturasi_satir_sayisi.Location = new System.Drawing.Point(357, 182);
		this.se_alis_faturasi_satir_sayisi.Name = "se_alis_faturasi_satir_sayisi";
		this.se_alis_faturasi_satir_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_alis_faturasi_satir_sayisi.Size = new System.Drawing.Size(46, 20);
		this.se_alis_faturasi_satir_sayisi.TabIndex = 12;
		this.se_alis_faturasi_satir_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(289, 185);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(62, 13);
		this.label12.TabIndex = 21;
		this.label12.Text = "Satır sayısı :";
		this.dtp_alis_faturasi_ilk_tarih.Location = new System.Drawing.Point(463, 182);
		this.dtp_alis_faturasi_ilk_tarih.Name = "dtp_alis_faturasi_ilk_tarih";
		this.dtp_alis_faturasi_ilk_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_alis_faturasi_ilk_tarih.TabIndex = 13;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(410, 188);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(47, 13);
		this.label13.TabIndex = 24;
		this.label13.Text = "İlk tarih :";
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(574, 188);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(55, 13);
		this.label14.TabIndex = 26;
		this.label14.Text = "Son tarih :";
		this.dtp_alis_faturasi_son_tarih.Location = new System.Drawing.Point(635, 182);
		this.dtp_alis_faturasi_son_tarih.Name = "dtp_alis_faturasi_son_tarih";
		this.dtp_alis_faturasi_son_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_alis_faturasi_son_tarih.TabIndex = 14;
		this.label15.BackColor = System.Drawing.SystemColors.GrayText;
		this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label15.ForeColor = System.Drawing.SystemColors.Menu;
		this.label15.Location = new System.Drawing.Point(12, 205);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(739, 23);
		this.label15.TabIndex = 27;
		this.label15.Text = "Satış faturası";
		this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(574, 237);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(55, 13);
		this.label16.TabIndex = 37;
		this.label16.Text = "Son tarih :";
		this.dtp_satis_faturasi_son_tarih.Location = new System.Drawing.Point(635, 231);
		this.dtp_satis_faturasi_son_tarih.Name = "dtp_satis_faturasi_son_tarih";
		this.dtp_satis_faturasi_son_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_satis_faturasi_son_tarih.TabIndex = 19;
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(410, 237);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(47, 13);
		this.label17.TabIndex = 35;
		this.label17.Text = "İlk tarih :";
		this.dtp_satis_faturasi_ilk_tarih.Location = new System.Drawing.Point(463, 231);
		this.dtp_satis_faturasi_ilk_tarih.Name = "dtp_satis_faturasi_ilk_tarih";
		this.dtp_satis_faturasi_ilk_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_satis_faturasi_ilk_tarih.TabIndex = 18;
		this.se_satis_faturasi_satir_sayisi.EditValue = new decimal(new int[4] { 20, 0, 0, 0 });
		this.se_satis_faturasi_satir_sayisi.Location = new System.Drawing.Point(357, 231);
		this.se_satis_faturasi_satir_sayisi.Name = "se_satis_faturasi_satir_sayisi";
		this.se_satis_faturasi_satir_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_satis_faturasi_satir_sayisi.Size = new System.Drawing.Size(46, 20);
		this.se_satis_faturasi_satir_sayisi.TabIndex = 17;
		this.se_satis_faturasi_satir_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.label18.AutoSize = true;
		this.label18.Location = new System.Drawing.Point(289, 234);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(62, 13);
		this.label18.TabIndex = 32;
		this.label18.Text = "Satır sayısı :";
		this.te_satis_faturasi_evrak_seri.EditValue = "A";
		this.te_satis_faturasi_evrak_seri.Location = new System.Drawing.Point(247, 231);
		this.te_satis_faturasi_evrak_seri.Name = "te_satis_faturasi_evrak_seri";
		this.te_satis_faturasi_evrak_seri.Size = new System.Drawing.Size(33, 20);
		this.te_satis_faturasi_evrak_seri.TabIndex = 16;
		this.label19.AutoSize = true;
		this.label19.Location = new System.Drawing.Point(174, 234);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(67, 13);
		this.label19.TabIndex = 30;
		this.label19.Text = "Evrak serisi :";
		this.se_satis_faturasi_sayisi.EditValue = new decimal(new int[4] { 3650, 0, 0, 0 });
		this.se_satis_faturasi_sayisi.Location = new System.Drawing.Point(97, 231);
		this.se_satis_faturasi_sayisi.Name = "se_satis_faturasi_sayisi";
		this.se_satis_faturasi_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_satis_faturasi_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_satis_faturasi_sayisi.TabIndex = 15;
		this.se_satis_faturasi_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.te_tedarikci_cari_unvan_on_eki.EditValue = "TEDARİKÇİ";
		this.te_tedarikci_cari_unvan_on_eki.Location = new System.Drawing.Point(511, 133);
		this.te_tedarikci_cari_unvan_on_eki.Name = "te_tedarikci_cari_unvan_on_eki";
		this.te_tedarikci_cari_unvan_on_eki.Size = new System.Drawing.Size(240, 20);
		this.te_tedarikci_cari_unvan_on_eki.TabIndex = 9;
		this.label21.AutoSize = true;
		this.label21.Location = new System.Drawing.Point(409, 136);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(96, 13);
		this.label21.TabIndex = 43;
		this.label21.Text = "Cari ünvan ön eki :";
		this.te_tedarikci_cari_kod_on_eki.EditValue = "SAT.";
		this.te_tedarikci_cari_kod_on_eki.Location = new System.Drawing.Point(318, 133);
		this.te_tedarikci_cari_kod_on_eki.Name = "te_tedarikci_cari_kod_on_eki";
		this.te_tedarikci_cari_kod_on_eki.Size = new System.Drawing.Size(67, 20);
		this.te_tedarikci_cari_kod_on_eki.TabIndex = 8;
		this.label22.AutoSize = true;
		this.label22.Location = new System.Drawing.Point(228, 136);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(84, 13);
		this.label22.TabIndex = 41;
		this.label22.Text = "Cari kod ön eki :";
		this.se_olusturulacak_tedarikci_sayisi.EditValue = new decimal(new int[4] { 20, 0, 0, 0 });
		this.se_olusturulacak_tedarikci_sayisi.Location = new System.Drawing.Point(149, 133);
		this.se_olusturulacak_tedarikci_sayisi.Name = "se_olusturulacak_tedarikci_sayisi";
		this.se_olusturulacak_tedarikci_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_olusturulacak_tedarikci_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_olusturulacak_tedarikci_sayisi.TabIndex = 7;
		this.label23.AutoSize = true;
		this.label23.Location = new System.Drawing.Point(17, 136);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(126, 13);
		this.label23.TabIndex = 39;
		this.label23.Text = "Oluşturulacak cari sayısı :";
		this.label24.BackColor = System.Drawing.SystemColors.GrayText;
		this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label24.ForeColor = System.Drawing.SystemColors.Menu;
		this.label24.Location = new System.Drawing.Point(12, 107);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(739, 23);
		this.label24.TabIndex = 38;
		this.label24.Text = "Tedarikçi";
		this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label25.AutoSize = true;
		this.label25.Location = new System.Drawing.Point(574, 286);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(55, 13);
		this.label25.TabIndex = 55;
		this.label25.Text = "Son tarih :";
		this.dtp_alinan_siparis_son_tarih.Location = new System.Drawing.Point(635, 280);
		this.dtp_alinan_siparis_son_tarih.Name = "dtp_alinan_siparis_son_tarih";
		this.dtp_alinan_siparis_son_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_alinan_siparis_son_tarih.TabIndex = 24;
		this.label26.AutoSize = true;
		this.label26.Location = new System.Drawing.Point(410, 286);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(47, 13);
		this.label26.TabIndex = 53;
		this.label26.Text = "İlk tarih :";
		this.dtp_alinan_siparis_ilk_tarih.Location = new System.Drawing.Point(463, 280);
		this.dtp_alinan_siparis_ilk_tarih.Name = "dtp_alinan_siparis_ilk_tarih";
		this.dtp_alinan_siparis_ilk_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_alinan_siparis_ilk_tarih.TabIndex = 23;
		this.se_alinan_siparis_satir_sayisi.EditValue = new decimal(new int[4] { 20, 0, 0, 0 });
		this.se_alinan_siparis_satir_sayisi.Location = new System.Drawing.Point(357, 280);
		this.se_alinan_siparis_satir_sayisi.Name = "se_alinan_siparis_satir_sayisi";
		this.se_alinan_siparis_satir_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_alinan_siparis_satir_sayisi.Size = new System.Drawing.Size(46, 20);
		this.se_alinan_siparis_satir_sayisi.TabIndex = 22;
		this.se_alinan_siparis_satir_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.label27.AutoSize = true;
		this.label27.Location = new System.Drawing.Point(289, 283);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(62, 13);
		this.label27.TabIndex = 50;
		this.label27.Text = "Satır sayısı :";
		this.te_alinan_siparis_evrak_seri.EditValue = "A";
		this.te_alinan_siparis_evrak_seri.Location = new System.Drawing.Point(247, 280);
		this.te_alinan_siparis_evrak_seri.Name = "te_alinan_siparis_evrak_seri";
		this.te_alinan_siparis_evrak_seri.Size = new System.Drawing.Size(33, 20);
		this.te_alinan_siparis_evrak_seri.TabIndex = 21;
		this.label28.AutoSize = true;
		this.label28.Location = new System.Drawing.Point(174, 283);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(67, 13);
		this.label28.TabIndex = 48;
		this.label28.Text = "Evrak serisi :";
		this.se_alinan_siparis_sayisi.EditValue = new decimal(new int[4] { 3650, 0, 0, 0 });
		this.se_alinan_siparis_sayisi.Location = new System.Drawing.Point(97, 280);
		this.se_alinan_siparis_sayisi.Name = "se_alinan_siparis_sayisi";
		this.se_alinan_siparis_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_alinan_siparis_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_alinan_siparis_sayisi.TabIndex = 20;
		this.se_alinan_siparis_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.label29.AutoSize = true;
		this.label29.Location = new System.Drawing.Point(22, 283);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(69, 13);
		this.label29.TabIndex = 46;
		this.label29.Text = "Evrak sayısı :";
		this.label30.BackColor = System.Drawing.SystemColors.GrayText;
		this.label30.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label30.ForeColor = System.Drawing.SystemColors.Menu;
		this.label30.Location = new System.Drawing.Point(12, 254);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(739, 23);
		this.label30.TabIndex = 45;
		this.label30.Text = "Alınan sipariş";
		this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label31.AutoSize = true;
		this.label31.Location = new System.Drawing.Point(574, 335);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(55, 13);
		this.label31.TabIndex = 65;
		this.label31.Text = "Son tarih :";
		this.dtp_tahsilat_makbuzu_son_tarih.Location = new System.Drawing.Point(635, 329);
		this.dtp_tahsilat_makbuzu_son_tarih.Name = "dtp_tahsilat_makbuzu_son_tarih";
		this.dtp_tahsilat_makbuzu_son_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_tahsilat_makbuzu_son_tarih.TabIndex = 28;
		this.label32.AutoSize = true;
		this.label32.Location = new System.Drawing.Point(410, 335);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(47, 13);
		this.label32.TabIndex = 63;
		this.label32.Text = "İlk tarih :";
		this.dtp_tahsilat_makbuzu_ilk_tarih.Location = new System.Drawing.Point(463, 329);
		this.dtp_tahsilat_makbuzu_ilk_tarih.Name = "dtp_tahsilat_makbuzu_ilk_tarih";
		this.dtp_tahsilat_makbuzu_ilk_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_tahsilat_makbuzu_ilk_tarih.TabIndex = 27;
		this.te_tahsilat_makbuzu_evrak_seri.EditValue = "A";
		this.te_tahsilat_makbuzu_evrak_seri.Location = new System.Drawing.Point(247, 329);
		this.te_tahsilat_makbuzu_evrak_seri.Name = "te_tahsilat_makbuzu_evrak_seri";
		this.te_tahsilat_makbuzu_evrak_seri.Size = new System.Drawing.Size(33, 20);
		this.te_tahsilat_makbuzu_evrak_seri.TabIndex = 26;
		this.label34.AutoSize = true;
		this.label34.Location = new System.Drawing.Point(174, 332);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(67, 13);
		this.label34.TabIndex = 58;
		this.label34.Text = "Evrak serisi :";
		this.se_tahsilat_makbuzu_sayisi.EditValue = new decimal(new int[4] { 3650, 0, 0, 0 });
		this.se_tahsilat_makbuzu_sayisi.Location = new System.Drawing.Point(97, 329);
		this.se_tahsilat_makbuzu_sayisi.Name = "se_tahsilat_makbuzu_sayisi";
		this.se_tahsilat_makbuzu_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_tahsilat_makbuzu_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_tahsilat_makbuzu_sayisi.TabIndex = 25;
		this.se_tahsilat_makbuzu_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.label35.BackColor = System.Drawing.SystemColors.GrayText;
		this.label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label35.ForeColor = System.Drawing.SystemColors.Menu;
		this.label35.Location = new System.Drawing.Point(12, 303);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(739, 23);
		this.label35.TabIndex = 56;
		this.label35.Text = "Tahsilat makbuzu";
		this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(22, 234);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(69, 13);
		this.label11.TabIndex = 66;
		this.label11.Text = "Evrak sayısı :";
		this.label20.AutoSize = true;
		this.label20.Location = new System.Drawing.Point(22, 185);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(69, 13);
		this.label20.TabIndex = 67;
		this.label20.Text = "Evrak sayısı :";
		this.label36.AutoSize = true;
		this.label36.Location = new System.Drawing.Point(22, 332);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(69, 13);
		this.label36.TabIndex = 68;
		this.label36.Text = "Evrak sayısı :";
		this.label37.AutoSize = true;
		this.label37.Location = new System.Drawing.Point(574, 384);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(55, 13);
		this.label37.TabIndex = 78;
		this.label37.Text = "Son tarih :";
		this.dtp_tediye_makbuzu_son_tarih.Enabled = false;
		this.dtp_tediye_makbuzu_son_tarih.Location = new System.Drawing.Point(635, 378);
		this.dtp_tediye_makbuzu_son_tarih.Name = "dtp_tediye_makbuzu_son_tarih";
		this.dtp_tediye_makbuzu_son_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_tediye_makbuzu_son_tarih.TabIndex = 32;
		this.label38.AutoSize = true;
		this.label38.Location = new System.Drawing.Point(410, 384);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(47, 13);
		this.label38.TabIndex = 76;
		this.label38.Text = "İlk tarih :";
		this.dtp_tediye_makbuzu_ilk_tarih.Enabled = false;
		this.dtp_tediye_makbuzu_ilk_tarih.Location = new System.Drawing.Point(463, 378);
		this.dtp_tediye_makbuzu_ilk_tarih.Name = "dtp_tediye_makbuzu_ilk_tarih";
		this.dtp_tediye_makbuzu_ilk_tarih.Size = new System.Drawing.Size(103, 20);
		this.dtp_tediye_makbuzu_ilk_tarih.TabIndex = 31;
		this.te_tediye_makbuzu_evrak_seri.EditValue = "A";
		this.te_tediye_makbuzu_evrak_seri.Enabled = false;
		this.te_tediye_makbuzu_evrak_seri.Location = new System.Drawing.Point(247, 378);
		this.te_tediye_makbuzu_evrak_seri.Name = "te_tediye_makbuzu_evrak_seri";
		this.te_tediye_makbuzu_evrak_seri.Size = new System.Drawing.Size(33, 20);
		this.te_tediye_makbuzu_evrak_seri.TabIndex = 30;
		this.label40.AutoSize = true;
		this.label40.Location = new System.Drawing.Point(174, 381);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(67, 13);
		this.label40.TabIndex = 71;
		this.label40.Text = "Evrak serisi :";
		this.se_tediye_makbuzu_sayisi.EditValue = new decimal(new int[4] { 365, 0, 0, 0 });
		this.se_tediye_makbuzu_sayisi.Enabled = false;
		this.se_tediye_makbuzu_sayisi.Location = new System.Drawing.Point(97, 378);
		this.se_tediye_makbuzu_sayisi.Name = "se_tediye_makbuzu_sayisi";
		this.se_tediye_makbuzu_sayisi.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[1]
		{
			new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
		});
		this.se_tediye_makbuzu_sayisi.Size = new System.Drawing.Size(67, 20);
		this.se_tediye_makbuzu_sayisi.TabIndex = 29;
		this.se_tediye_makbuzu_sayisi.EditValueChanged += new System.EventHandler(genel_EditValueChanged);
		this.label41.BackColor = System.Drawing.SystemColors.GrayText;
		this.label41.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label41.ForeColor = System.Drawing.SystemColors.Menu;
		this.label41.Location = new System.Drawing.Point(12, 352);
		this.label41.Name = "label41";
		this.label41.Size = new System.Drawing.Size(739, 23);
		this.label41.TabIndex = 69;
		this.label41.Text = "Tediye makbuzu";
		this.label41.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label42.AutoSize = true;
		this.label42.Location = new System.Drawing.Point(22, 381);
		this.label42.Name = "label42";
		this.label42.Size = new System.Drawing.Size(69, 13);
		this.label42.TabIndex = 79;
		this.label42.Text = "Evrak sayısı :";
		this.label33.AutoSize = true;
		this.label33.Location = new System.Drawing.Point(64, 528);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(104, 13);
		this.label33.TabIndex = 80;
		this.label33.Text = "Stok hareketi sayısı :";
		this.label39.AutoSize = true;
		this.label39.Location = new System.Drawing.Point(289, 528);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(132, 13);
		this.label39.TabIndex = 81;
		this.label39.Text = "Cari hesap hareketi sayısı :";
		this.label43.AutoSize = true;
		this.label43.Location = new System.Drawing.Point(523, 528);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(113, 13);
		this.label43.TabIndex = 82;
		this.label43.Text = "Sipariş hareketi sayısı :";
		this.label44.BackColor = System.Drawing.SystemColors.GrayText;
		this.label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label44.ForeColor = System.Drawing.SystemColors.Menu;
		this.label44.Location = new System.Drawing.Point(12, 494);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(739, 23);
		this.label44.TabIndex = 83;
		this.label44.Text = "Oluşturulacak hareket sayıları";
		this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label_stok_hareketi_sayisi.AutoSize = true;
		this.label_stok_hareketi_sayisi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_stok_hareketi_sayisi.Location = new System.Drawing.Point(174, 528);
		this.label_stok_hareketi_sayisi.Name = "label_stok_hareketi_sayisi";
		this.label_stok_hareketi_sayisi.Size = new System.Drawing.Size(39, 13);
		this.label_stok_hareketi_sayisi.TabIndex = 84;
		this.label_stok_hareketi_sayisi.Text = "1.000";
		this.label_cari_hesap_hareketi_sayisi.AutoSize = true;
		this.label_cari_hesap_hareketi_sayisi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_cari_hesap_hareketi_sayisi.Location = new System.Drawing.Point(427, 528);
		this.label_cari_hesap_hareketi_sayisi.Name = "label_cari_hesap_hareketi_sayisi";
		this.label_cari_hesap_hareketi_sayisi.Size = new System.Drawing.Size(39, 13);
		this.label_cari_hesap_hareketi_sayisi.TabIndex = 85;
		this.label_cari_hesap_hareketi_sayisi.Text = "1.000";
		this.label_siparis_hareketi_sayisi.AutoSize = true;
		this.label_siparis_hareketi_sayisi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_siparis_hareketi_sayisi.Location = new System.Drawing.Point(642, 528);
		this.label_siparis_hareketi_sayisi.Name = "label_siparis_hareketi_sayisi";
		this.label_siparis_hareketi_sayisi.Size = new System.Drawing.Size(39, 13);
		this.label_siparis_hareketi_sayisi.TabIndex = 86;
		this.label_siparis_hareketi_sayisi.Text = "1.000";
		this.label45.BackColor = System.Drawing.SystemColors.GrayText;
		this.label45.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label45.ForeColor = System.Drawing.SystemColors.Menu;
		this.label45.Location = new System.Drawing.Point(12, 558);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(739, 23);
		this.label45.TabIndex = 87;
		this.label45.Text = "İşlem bilgileri";
		this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label46.AutoSize = true;
		this.label46.Location = new System.Drawing.Point(14, 594);
		this.label46.Name = "label46";
		this.label46.Size = new System.Drawing.Size(115, 13);
		this.label46.TabIndex = 88;
		this.label46.Text = "İşlem başlama zamanı :";
		this.label_baslama_zamani.AutoSize = true;
		this.label_baslama_zamani.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_baslama_zamani.Location = new System.Drawing.Point(135, 594);
		this.label_baslama_zamani.Name = "label_baslama_zamani";
		this.label_baslama_zamani.Size = new System.Drawing.Size(11, 13);
		this.label_baslama_zamani.TabIndex = 89;
		this.label_baslama_zamani.Text = "-";
		this.label_bitis_zamani.AutoSize = true;
		this.label_bitis_zamani.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_bitis_zamani.Location = new System.Drawing.Point(318, 594);
		this.label_bitis_zamani.Name = "label_bitis_zamani";
		this.label_bitis_zamani.Size = new System.Drawing.Size(11, 13);
		this.label_bitis_zamani.TabIndex = 91;
		this.label_bitis_zamani.Text = "-";
		this.label49.AutoSize = true;
		this.label49.Location = new System.Drawing.Point(218, 594);
		this.label49.Name = "label49";
		this.label49.Size = new System.Drawing.Size(94, 13);
		this.label49.TabIndex = 90;
		this.label49.Text = "İşlem bitiş zamanı :";
		this.label_toplam_sure.AutoSize = true;
		this.label_toplam_sure.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_toplam_sure.Location = new System.Drawing.Point(487, 594);
		this.label_toplam_sure.Name = "label_toplam_sure";
		this.label_toplam_sure.Size = new System.Drawing.Size(11, 13);
		this.label_toplam_sure.TabIndex = 93;
		this.label_toplam_sure.Text = "-";
		this.label51.AutoSize = true;
		this.label51.Location = new System.Drawing.Point(410, 594);
		this.label51.Name = "label51";
		this.label51.Size = new System.Drawing.Size(71, 13);
		this.label51.TabIndex = 92;
		this.label51.Text = "Toplam süre :";
		this.label_islem_bilgisi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_islem_bilgisi.Location = new System.Drawing.Point(15, 624);
		this.label_islem_bilgisi.Name = "label_islem_bilgisi";
		this.label_islem_bilgisi.Size = new System.Drawing.Size(736, 13);
		this.label_islem_bilgisi.TabIndex = 94;
		this.label_islem_bilgisi.Text = "-";
		this.label_islem_bilgisi.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_ilerleme.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 162);
		this.label_ilerleme.Location = new System.Drawing.Point(15, 675);
		this.label_ilerleme.Name = "label_ilerleme";
		this.label_ilerleme.Size = new System.Drawing.Size(736, 13);
		this.label_ilerleme.TabIndex = 95;
		this.label_ilerleme.Text = "-";
		this.label_ilerleme.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.progressBar1.Location = new System.Drawing.Point(15, 645);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(736, 23);
		this.progressBar1.TabIndex = 96;
		this.bw_cari_olusturma.WorkerReportsProgress = true;
		this.bw_cari_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_musteri_olusturma_DoWork);
		this.bw_cari_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_musteri_olusturma_ProgressChanged);
		this.bw_cari_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_musteri_olusturma_RunWorkerCompleted);
		this.bw_stok_olusturma.WorkerReportsProgress = true;
		this.bw_stok_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_stok_olusturma_DoWork);
		this.bw_stok_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_stok_olusturma_ProgressChanged);
		this.bw_stok_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_stok_olusturma_RunWorkerCompleted);
		this.bw_tedarikci_olusturma.WorkerReportsProgress = true;
		this.bw_tedarikci_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_tedarikci_olusturma_DoWork);
		this.bw_tedarikci_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_tedarikci_olusturma_ProgressChanged);
		this.bw_tedarikci_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_tedarikci_olusturma_RunWorkerCompleted);
		this.bw_alis_faturasi_olusturma.WorkerReportsProgress = true;
		this.bw_alis_faturasi_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_alis_faturasi_olusturma_DoWork);
		this.bw_alis_faturasi_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_alis_faturasi_olusturma_ProgressChanged);
		this.bw_alis_faturasi_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_alis_faturasi_olusturma_RunWorkerCompleted);
		this.bw_satis_faturasi_olusturma.WorkerReportsProgress = true;
		this.bw_satis_faturasi_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_satis_faturasi_olusturma_DoWork);
		this.bw_satis_faturasi_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_satis_faturasi_olusturma_ProgressChanged);
		this.bw_satis_faturasi_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_satis_faturasi_olusturma_RunWorkerCompleted);
		this.bw_alinan_siparis_olusturma.WorkerReportsProgress = true;
		this.bw_alinan_siparis_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_alinan_siparis_olusturma_DoWork);
		this.bw_alinan_siparis_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_alinan_siparis_olusturma_ProgressChanged);
		this.bw_alinan_siparis_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_alinan_siparis_olusturma_RunWorkerCompleted);
		this.bw_tahsilat_makbuzu_olusturma.WorkerReportsProgress = true;
		this.bw_tahsilat_makbuzu_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_tahsilat_makbuzu_olusturma_DoWork);
		this.bw_tahsilat_makbuzu_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_tahsilat_makbuzu_olusturma_ProgressChanged);
		this.bw_tahsilat_makbuzu_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_tahsilat_makbuzu_olusturma_RunWorkerCompleted);
		this.bw_tediye_makbuzu_olusturma.WorkerReportsProgress = true;
		this.bw_tediye_makbuzu_olusturma.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_tediye_makbuzu_olusturma_DoWork);
		this.bw_tediye_makbuzu_olusturma.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_tediye_makbuzu_olusturma_ProgressChanged);
		this.bw_tediye_makbuzu_olusturma.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_tediye_makbuzu_olusturma_RunWorkerCompleted);
		this.bw_kayitlari_silme.WorkerReportsProgress = true;
		this.bw_kayitlari_silme.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_kayitlari_silme_DoWork);
		this.bw_kayitlari_silme.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bw_kayitlari_silme_ProgressChanged);
		this.bw_kayitlari_silme.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_kayitlari_silme_RunWorkerCompleted);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(763, 798);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.label_ilerleme);
		base.Controls.Add(this.label_islem_bilgisi);
		base.Controls.Add(this.label_toplam_sure);
		base.Controls.Add(this.label51);
		base.Controls.Add(this.label_bitis_zamani);
		base.Controls.Add(this.label49);
		base.Controls.Add(this.label_baslama_zamani);
		base.Controls.Add(this.label46);
		base.Controls.Add(this.label45);
		base.Controls.Add(this.label_siparis_hareketi_sayisi);
		base.Controls.Add(this.label_cari_hesap_hareketi_sayisi);
		base.Controls.Add(this.label_stok_hareketi_sayisi);
		base.Controls.Add(this.label44);
		base.Controls.Add(this.label43);
		base.Controls.Add(this.label39);
		base.Controls.Add(this.label33);
		base.Controls.Add(this.label42);
		base.Controls.Add(this.label37);
		base.Controls.Add(this.dtp_tediye_makbuzu_son_tarih);
		base.Controls.Add(this.label38);
		base.Controls.Add(this.dtp_tediye_makbuzu_ilk_tarih);
		base.Controls.Add(this.te_tediye_makbuzu_evrak_seri);
		base.Controls.Add(this.label40);
		base.Controls.Add(this.se_tediye_makbuzu_sayisi);
		base.Controls.Add(this.label41);
		base.Controls.Add(this.label36);
		base.Controls.Add(this.label20);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.label31);
		base.Controls.Add(this.dtp_tahsilat_makbuzu_son_tarih);
		base.Controls.Add(this.label32);
		base.Controls.Add(this.dtp_tahsilat_makbuzu_ilk_tarih);
		base.Controls.Add(this.te_tahsilat_makbuzu_evrak_seri);
		base.Controls.Add(this.label34);
		base.Controls.Add(this.se_tahsilat_makbuzu_sayisi);
		base.Controls.Add(this.label35);
		base.Controls.Add(this.label25);
		base.Controls.Add(this.dtp_alinan_siparis_son_tarih);
		base.Controls.Add(this.label26);
		base.Controls.Add(this.dtp_alinan_siparis_ilk_tarih);
		base.Controls.Add(this.se_alinan_siparis_satir_sayisi);
		base.Controls.Add(this.label27);
		base.Controls.Add(this.te_alinan_siparis_evrak_seri);
		base.Controls.Add(this.label28);
		base.Controls.Add(this.se_alinan_siparis_sayisi);
		base.Controls.Add(this.label29);
		base.Controls.Add(this.label30);
		base.Controls.Add(this.te_tedarikci_cari_unvan_on_eki);
		base.Controls.Add(this.label21);
		base.Controls.Add(this.te_tedarikci_cari_kod_on_eki);
		base.Controls.Add(this.label22);
		base.Controls.Add(this.se_olusturulacak_tedarikci_sayisi);
		base.Controls.Add(this.label23);
		base.Controls.Add(this.label24);
		base.Controls.Add(this.label16);
		base.Controls.Add(this.dtp_satis_faturasi_son_tarih);
		base.Controls.Add(this.label17);
		base.Controls.Add(this.dtp_satis_faturasi_ilk_tarih);
		base.Controls.Add(this.se_satis_faturasi_satir_sayisi);
		base.Controls.Add(this.label18);
		base.Controls.Add(this.te_satis_faturasi_evrak_seri);
		base.Controls.Add(this.label19);
		base.Controls.Add(this.se_satis_faturasi_sayisi);
		base.Controls.Add(this.label15);
		base.Controls.Add(this.label14);
		base.Controls.Add(this.dtp_alis_faturasi_son_tarih);
		base.Controls.Add(this.label13);
		base.Controls.Add(this.dtp_alis_faturasi_ilk_tarih);
		base.Controls.Add(this.se_alis_faturasi_satir_sayisi);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.te_alis_faturasi_evrak_seri);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.se_alis_faturasi_sayisi);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.te_stok_ismi_on_eki);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.te_stok_kod_on_eki);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.se_olusturulacak_stok_sayisi);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.b_kayitlari_sil);
		base.Controls.Add(this.b_kayitlari_olustur);
		base.Controls.Add(this.te_musteri_cari_unvan_on_eki);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.te_musteri_cari_kod_on_eki);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.se_olusturulacak_musteri_sayisi);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Name = "PerformansTestKurulum";
		this.Text = "Performans Test Kurulumu";
		((System.ComponentModel.ISupportInitialize)this.se_olusturulacak_musteri_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_musteri_cari_kod_on_eki.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_musteri_cari_unvan_on_eki.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_stok_ismi_on_eki.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_stok_kod_on_eki.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_olusturulacak_stok_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_alis_faturasi_evrak_seri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_alis_faturasi_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_alis_faturasi_satir_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_satis_faturasi_satir_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_satis_faturasi_evrak_seri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_satis_faturasi_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tedarikci_cari_unvan_on_eki.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tedarikci_cari_kod_on_eki.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_olusturulacak_tedarikci_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_alinan_siparis_satir_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_alinan_siparis_evrak_seri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_alinan_siparis_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tahsilat_makbuzu_evrak_seri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_tahsilat_makbuzu_sayisi.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.te_tediye_makbuzu_evrak_seri.Properties).EndInit();
		((System.ComponentModel.ISupportInitialize)this.se_tediye_makbuzu_sayisi.Properties).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
